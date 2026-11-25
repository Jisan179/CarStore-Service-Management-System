using System;
using System.Data;
using System.Data.SqlClient;
using CarStoreServiceManagement.Data;
using CarStoreServiceManagement.Models;

namespace CarStoreServiceManagement.Services
{
    /// <summary>
    /// Service for processing vehicle sales and customer purchase requests.
    /// Employs ACID SQL transactions to guarantee consistency across SalesRecord and CarsStock.
    /// </summary>
    public class SalesService
    {
        /// <summary>
        /// Customer initiates a purchase request for an available vehicle.
        /// Transactionally creates a Pending SalesRecord and marks car status as 'Reserved'.
        /// </summary>
        public bool RequestPurchase(int carId, int customerId, decimal price, out string errorMessage)
        {
            errorMessage = null;

            return DatabaseHelper.ExecuteTransaction((conn, trans) =>
            {
                // 1. Verify vehicle is currently available
                string checkSql = "SELECT Status, Price FROM dbo.CarsStock WITH (UPDLOCK) WHERE CarID = @CarID";
                using (SqlCommand checkCmd = new SqlCommand(checkSql, conn, trans))
                {
                    checkCmd.Parameters.Add(new SqlParameter("@CarID", SqlDbType.Int) { Value = carId });
                    using (SqlDataReader reader = checkCmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            throw new InvalidOperationException("This vehicle no longer exists.");
                        }
                        string status = reader["Status"].ToString();
                        if (!string.Equals(status, "Available", StringComparison.OrdinalIgnoreCase))
                        {
                            throw new InvalidOperationException("Sorry, this vehicle is no longer available.");
                        }
                        if (Convert.ToDecimal(reader["Price"]) != price)
                        {
                            throw new InvalidOperationException("The price of this vehicle has changed. Please refresh the showroom and try again.");
                        }
                    }
                }

                // 2. Insert SalesRecord with Pending status
                string insertSql = @"INSERT INTO dbo.SalesRecord (CarID, CustomerID, EmployeeID, SaleDate, Price, SalesStatus)
                                     VALUES (@CarID, @CustomerID, NULL, GETDATE(), @Price, 'Pending')";
                using (SqlCommand insertCmd = new SqlCommand(insertSql, conn, trans))
                {
                    insertCmd.Parameters.Add(new SqlParameter("@CarID", SqlDbType.Int) { Value = carId });
                    insertCmd.Parameters.Add(new SqlParameter("@CustomerID", SqlDbType.Int) { Value = customerId });
                    insertCmd.Parameters.Add(new SqlParameter("@Price", SqlDbType.Decimal) { Value = price });
                    insertCmd.ExecuteNonQuery();
                }

                // 3. Update Car status to 'Reserved' so others cannot request it concurrently
                string updateCarSql = "UPDATE dbo.CarsStock SET Status = 'Reserved' WHERE CarID = @CarID";
                using (SqlCommand updateCmd = new SqlCommand(updateCarSql, conn, trans))
                {
                    updateCmd.Parameters.Add(new SqlParameter("@CarID", SqlDbType.Int) { Value = carId });
                    updateCmd.ExecuteNonQuery();
                }

                return true;
            }, out errorMessage);
        }

        /// <summary>
        /// Employee or Admin confirms a pending sale.
        /// Transactionally updates SalesRecord to 'Confirmed' and CarsStock to 'Sold'.
        /// </summary>
        public bool ConfirmSale(int salesId, int carId, int employeeId, out string errorMessage)
        {
            return ClosePendingSale(salesId, carId, employeeId, "Confirmed", "Sold", out errorMessage);
        }

        /// <summary>
        /// Employee or Admin cancels a pending sale request. Confirmed sales cannot be cancelled.
        /// Transactionally updates SalesRecord to 'Cancelled' and restores CarsStock to 'Available'.
        /// </summary>
        public bool CancelSale(int salesId, int carId, int employeeId, out string errorMessage)
        {
            return ClosePendingSale(salesId, carId, employeeId, "Cancelled", "Available", out errorMessage);
        }

        private bool ClosePendingSale(int salesId, int carId, int employeeId, string saleStatus, string carStatus, out string errorMessage)
        {
            return DatabaseHelper.ExecuteTransaction((conn, trans) =>
            {
                string updateSaleSql = @"UPDATE dbo.SalesRecord
                                         SET SalesStatus = @SaleStatus, EmployeeID = @EmployeeID
                                         WHERE SalesID = @SalesID AND CarID = @CarID AND SalesStatus = 'Pending'";
                using (SqlCommand cmd = new SqlCommand(updateSaleSql, conn, trans))
                {
                    cmd.Parameters.Add(new SqlParameter("@SaleStatus", SqlDbType.VarChar, 50) { Value = saleStatus });
                    cmd.Parameters.Add(new SqlParameter("@SalesID", SqlDbType.Int) { Value = salesId });
                    cmd.Parameters.Add(new SqlParameter("@CarID", SqlDbType.Int) { Value = carId });
                    cmd.Parameters.Add(new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = employeeId });
                    if (cmd.ExecuteNonQuery() == 0)
                    {
                        throw new InvalidOperationException("Only a pending order can be changed. Please refresh the list.");
                    }
                }

                // The vehicle must still be reserved for this order
                string updateCarSql = "UPDATE dbo.CarsStock SET Status = @CarStatus WHERE CarID = @CarID AND Status = 'Reserved'";
                using (SqlCommand cmd = new SqlCommand(updateCarSql, conn, trans))
                {
                    cmd.Parameters.Add(new SqlParameter("@CarStatus", SqlDbType.NVarChar, 20) { Value = carStatus });
                    cmd.Parameters.Add(new SqlParameter("@CarID", SqlDbType.Int) { Value = carId });
                    if (cmd.ExecuteNonQuery() == 0)
                    {
                        throw new InvalidOperationException("This vehicle is not in 'Reserved' status, so the order was not changed.");
                    }
                }

                return true;
            }, out errorMessage);
        }

        /// <summary>
        /// Retrieves all sales records joined with car, customer, and employee information.
        /// </summary>
        public DataTable GetAllSalesRecords(string statusFilter = null, string keyword = null)
        {
            string sql = @"SELECT 
                                s.SalesID,
                                s.CarID,
                                s.CustomerID,
                                s.EmployeeID,
                                (c.Brand + ' ' + c.Model + ' (' + CAST(c.Year AS VARCHAR) + ')') AS CarDetails,
                                uCust.FullName AS CustomerName,
                                uCust.Email AS CustomerEmail,
                                ISNULL(uEmp.FullName, 'Unassigned') AS EmployeeName,
                                s.SaleDate,
                                s.Price,
                                s.SalesStatus
                           FROM dbo.SalesRecord s
                           INNER JOIN dbo.CarsStock c ON s.CarID = c.CarID
                           INNER JOIN dbo.Users uCust ON s.CustomerID = uCust.UserID
                           LEFT JOIN dbo.Users uEmp ON s.EmployeeID = uEmp.UserID
                           WHERE 1=1";

            var parameters = new System.Collections.Generic.List<SqlParameter>();

            if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "All")
            {
                sql += " AND s.SalesStatus = @Status";
                parameters.Add(new SqlParameter("@Status", SqlDbType.VarChar, 50) { Value = statusFilter });
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                sql += " AND (c.Brand LIKE @Keyword OR c.Model LIKE @Keyword OR uCust.FullName LIKE @Keyword)";
                parameters.Add(new SqlParameter("@Keyword", SqlDbType.NVarChar, 100) { Value = "%" + keyword.Trim() + "%" });
            }

            sql += " ORDER BY s.SalesID DESC";

            return DatabaseHelper.ExecuteQuery(sql, parameters.ToArray());
        }

        /// <summary>
        /// Retrieves purchases belonging exclusively to the specified customer.
        /// Strictly isolates other customers' transactions.
        /// </summary>
        public DataTable GetCustomerPurchases(int customerId)
        {
            string sql = @"SELECT 
                                s.SalesID,
                                s.CarID,
                                (c.Brand + ' ' + c.Model + ' (' + CAST(c.Year AS VARCHAR) + ')') AS Vehicle,
                                s.Price,
                                s.SaleDate,
                                s.SalesStatus,
                                ISNULL(uEmp.FullName, 'Pending Assignment') AS ProcessedBy
                           FROM dbo.SalesRecord s
                           INNER JOIN dbo.CarsStock c ON s.CarID = c.CarID
                           LEFT JOIN dbo.Users uEmp ON s.EmployeeID = uEmp.UserID
                           WHERE s.CustomerID = @CustomerID
                           ORDER BY s.SalesID DESC";

            SqlParameter[] p = new SqlParameter[]
            {
                new SqlParameter("@CustomerID", SqlDbType.Int) { Value = customerId }
            };

            return DatabaseHelper.ExecuteQuery(sql, p);
        }
    }
}
