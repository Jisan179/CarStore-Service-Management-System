using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CarStoreServiceManagement.Data;
using CarStoreServiceManagement.Models;

namespace CarStoreServiceManagement.Services
{
    /// <summary>
    /// Service for managing the ServiceList catalog and ServiceRecords operations.
    /// Supports customer appointment booking, employee processing, and admin catalog CRUD.
    /// </summary>
    public class ServiceManagement
    {
        #region Service Catalog (ServiceList) CRUD

        public DataTable GetAllServices(string keyword = null)
        {
            string sql = "SELECT ServiceID, ServiceName, Price FROM dbo.ServiceList WHERE 1=1";
            List<SqlParameter> parameters = new List<SqlParameter>();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                sql += " AND ServiceName LIKE @Keyword";
                parameters.Add(new SqlParameter("@Keyword", SqlDbType.NVarChar, 200) { Value = "%" + keyword.Trim() + "%" });
            }

            sql += " ORDER BY ServiceID ASC";
            return DatabaseHelper.ExecuteQuery(sql, parameters.ToArray());
        }

        public Service GetServiceById(int serviceId)
        {
            string sql = "SELECT ServiceID, ServiceName, Price FROM dbo.ServiceList WHERE ServiceID = @ServiceID";
            SqlParameter[] p = new SqlParameter[]
            {
                new SqlParameter("@ServiceID", SqlDbType.Int) { Value = serviceId }
            };

            DataTable dt = DatabaseHelper.ExecuteQuery(sql, p);
            if (dt.Rows.Count == 0) return null;

            DataRow r = dt.Rows[0];
            return new Service(
                Convert.ToInt32(r["ServiceID"]),
                r["ServiceName"].ToString(),
                Convert.ToDecimal(r["Price"])
            );
        }

        public bool AddService(Service service, out string errorMessage)
        {
            errorMessage = null;
            if (service == null || !service.Validate(out errorMessage))
            {
                return false;
            }

            try
            {
                string sql = "INSERT INTO dbo.ServiceList (ServiceName, Price) VALUES (@ServiceName, @Price)";
                SqlParameter[] p = new SqlParameter[]
                {
                    new SqlParameter("@ServiceName", SqlDbType.NVarChar, 200) { Value = service.ServiceName.Trim() },
                    new SqlParameter("@Price", SqlDbType.Decimal) { Value = service.Price }
                };

                return DatabaseHelper.ExecuteNonQuery(sql, p) > 0;
            }
            catch (Exception ex)
            {
                errorMessage = "Failed to add service: " + ex.Message;
                return false;
            }
        }

        public bool UpdateService(Service service, out string errorMessage)
        {
            errorMessage = null;
            if (service == null || service.ServiceID <= 0 || !service.Validate(out errorMessage))
            {
                return false;
            }

            try
            {
                string sql = "UPDATE dbo.ServiceList SET ServiceName = @ServiceName, Price = @Price WHERE ServiceID = @ServiceID";
                SqlParameter[] p = new SqlParameter[]
                {
                    new SqlParameter("@ServiceName", SqlDbType.NVarChar, 200) { Value = service.ServiceName.Trim() },
                    new SqlParameter("@Price", SqlDbType.Decimal) { Value = service.Price },
                    new SqlParameter("@ServiceID", SqlDbType.Int) { Value = service.ServiceID }
                };

                return DatabaseHelper.ExecuteNonQuery(sql, p) > 0;
            }
            catch (Exception ex)
            {
                errorMessage = "Failed to update service: " + ex.Message;
                return false;
            }
        }

        public bool DeleteService(int serviceId, out string errorMessage)
        {
            errorMessage = null;
            try
            {
                // Check if referenced in ServiceRecords
                string checkSql = "SELECT COUNT(*) FROM dbo.ServiceRecords WHERE ServiceID = @ServiceID";
                SqlParameter[] cp = new SqlParameter[] { new SqlParameter("@ServiceID", SqlDbType.Int) { Value = serviceId } };
                int count = Convert.ToInt32(DatabaseHelper.ExecuteScalar(checkSql, cp));

                if (count > 0)
                {
                    errorMessage = "This service is referenced by historical service records and cannot be deleted.";
                    return false;
                }

                string sql = "DELETE FROM dbo.ServiceList WHERE ServiceID = @ServiceID";
                return DatabaseHelper.ExecuteNonQuery(sql, cp) > 0;
            }
            catch (Exception ex)
            {
                errorMessage = "Failed to delete service: " + ex.Message;
                return false;
            }
        }

        #endregion

        #region Service Records & Booking Workflow

        /// <summary>
        /// Vehicles the customer has bought (confirmed sales). Only these can be booked for service.
        /// </summary>
        public DataTable GetCustomerVehicles(int customerId)
        {
            string sql = @"SELECT DISTINCT c.CarID, c.Brand, c.Model, c.Year
                           FROM dbo.CarsStock c
                           INNER JOIN dbo.SalesRecord s ON s.CarID = c.CarID
                           WHERE s.CustomerID = @CustomerID AND s.SalesStatus = 'Confirmed'
                           ORDER BY c.CarID";

            return DatabaseHelper.ExecuteQuery(sql, new[]
            {
                new SqlParameter("@CustomerID", SqlDbType.Int) { Value = customerId }
            });
        }

        public bool BookService(int carId, int customerId, int serviceId, DateTime serviceDate, string details, out string errorMessage)
        {
            errorMessage = null;

            if (serviceDate < DateTime.Now.AddMinutes(-5))
            {
                errorMessage = "Please choose a service date and time that is not in the past.";
                return false;
            }

            try
            {
                // Ownership is checked in the same statement, and the price comes from the catalog
                string sql = @"INSERT INTO dbo.ServiceRecords (CarID, CustomerID, ServiceID, ServiceDate, ServiceDetails, ServiceCost, ServiceStatus)
                               SELECT @CarID, @CustomerID, sl.ServiceID, @ServiceDate, @ServiceDetails, sl.Price, 'Pending'
                               FROM dbo.ServiceList sl
                               WHERE sl.ServiceID = @ServiceID
                                 AND EXISTS (SELECT 1 FROM dbo.SalesRecord s
                                             WHERE s.CarID = @CarID AND s.CustomerID = @CustomerID AND s.SalesStatus = 'Confirmed')";

                SqlParameter[] p = new SqlParameter[]
                {
                    new SqlParameter("@CarID", SqlDbType.Int) { Value = carId },
                    new SqlParameter("@CustomerID", SqlDbType.Int) { Value = customerId },
                    new SqlParameter("@ServiceID", SqlDbType.Int) { Value = serviceId },
                    new SqlParameter("@ServiceDate", SqlDbType.DateTime) { Value = serviceDate },
                    new SqlParameter("@ServiceDetails", SqlDbType.NVarChar, -1) { Value = (object)details ?? DBNull.Value }
                };

                if (DatabaseHelper.ExecuteNonQuery(sql, p) > 0)
                    return true;

                errorMessage = "You can only book a service for a vehicle you have purchased, using a service from the catalog.";
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = "Service booking failed: " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Allowed moves: Pending -> In Progress / Cancelled, In Progress -> Completed / Cancelled.
        /// </summary>
        public bool UpdateServiceStatus(int recordId, string newStatus, out string errorMessage)
        {
            errorMessage = null;
            try
            {
                string sql = @"UPDATE dbo.ServiceRecords SET ServiceStatus = @Status
                               WHERE RecordID = @RecordID
                                 AND ((ServiceStatus = 'Pending' AND @Status IN ('In Progress', 'Cancelled'))
                                   OR (ServiceStatus = 'In Progress' AND @Status IN ('Completed', 'Cancelled')))";
                SqlParameter[] p = new SqlParameter[]
                {
                    new SqlParameter("@Status", SqlDbType.VarChar, 50) { Value = newStatus },
                    new SqlParameter("@RecordID", SqlDbType.Int) { Value = recordId }
                };

                if (DatabaseHelper.ExecuteNonQuery(sql, p) > 0)
                    return true;

                errorMessage = "This status change is not allowed.\n\nPending can become In Progress or Cancelled.\nIn Progress can become Completed or Cancelled.\nCompleted and Cancelled jobs are final.";
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = "Failed to update service status: " + ex.Message;
                return false;
            }
        }

        public DataTable GetAllServiceRecords(string statusFilter = null, string keyword = null)
        {
            string sql = @"SELECT 
                                sr.RecordID,
                                sr.CarID,
                                sr.CustomerID,
                                (c.Brand + ' ' + c.Model + ' (' + CAST(c.Year AS VARCHAR) + ')') AS CarDetails,
                                u.FullName AS CustomerName,
                                u.Email AS CustomerEmail,
                                ISNULL(sl.ServiceName, 'Custom Service') AS ServiceName,
                                sr.ServiceDate,
                                sr.ServiceCost,
                                sr.ServiceStatus,
                                sr.ServiceDetails
                           FROM dbo.ServiceRecords sr
                           INNER JOIN dbo.CarsStock c ON sr.CarID = c.CarID
                           INNER JOIN dbo.Users u ON sr.CustomerID = u.UserID
                           LEFT JOIN dbo.ServiceList sl ON sr.ServiceID = sl.ServiceID
                           WHERE 1=1";

            List<SqlParameter> parameters = new List<SqlParameter>();

            if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "All")
            {
                sql += " AND sr.ServiceStatus = @Status";
                parameters.Add(new SqlParameter("@Status", SqlDbType.VarChar, 50) { Value = statusFilter });
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                sql += " AND (c.Brand LIKE @Keyword OR c.Model LIKE @Keyword OR u.FullName LIKE @Keyword OR sl.ServiceName LIKE @Keyword)";
                parameters.Add(new SqlParameter("@Keyword", SqlDbType.NVarChar, 100) { Value = "%" + keyword.Trim() + "%" });
            }

            sql += " ORDER BY sr.RecordID DESC";
            return DatabaseHelper.ExecuteQuery(sql, parameters.ToArray());
        }

        public DataTable GetCustomerServiceRecords(int customerId)
        {
            string sql = @"SELECT 
                                sr.RecordID,
                                (c.Brand + ' ' + c.Model + ' (' + CAST(c.Year AS VARCHAR) + ')') AS Vehicle,
                                ISNULL(sl.ServiceName, 'Custom Maintenance') AS Service,
                                sr.ServiceDate,
                                sr.ServiceCost AS Cost,
                                sr.ServiceStatus AS Status,
                                sr.ServiceDetails AS Notes
                           FROM dbo.ServiceRecords sr
                           INNER JOIN dbo.CarsStock c ON sr.CarID = c.CarID
                           LEFT JOIN dbo.ServiceList sl ON sr.ServiceID = sl.ServiceID
                           WHERE sr.CustomerID = @CustomerID
                           ORDER BY sr.RecordID DESC";

            SqlParameter[] p = new SqlParameter[]
            {
                new SqlParameter("@CustomerID", SqlDbType.Int) { Value = customerId }
            };

            return DatabaseHelper.ExecuteQuery(sql, p);
        }

        #endregion
    }
}
