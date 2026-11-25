using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CarStoreServiceManagement.Data;
using CarStoreServiceManagement.Models;

namespace CarStoreServiceManagement.Services
{
    /// <summary>
    /// Service for vehicle inventory management (CarsStock).
    /// Supports CRUD operations, search, multi-field filtering, and status transitions.
    /// </summary>
    public class CarService
    {
        public DataTable GetAllCars(string keyword = null, string brand = null, decimal? minPrice = null, decimal? maxPrice = null, string status = null)
        {
            string sql = @"SELECT CarID, Brand, Model, Year, Price, Status, Image_Path, CreatedAt 
                           FROM dbo.CarsStock 
                           WHERE 1=1";

            List<SqlParameter> parameters = new List<SqlParameter>();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                sql += " AND (Brand LIKE @Keyword OR Model LIKE @Keyword)";
                parameters.Add(new SqlParameter("@Keyword", SqlDbType.NVarChar, 100) { Value = "%" + keyword.Trim() + "%" });
            }

            if (!string.IsNullOrWhiteSpace(brand) && brand != "All Brands")
            {
                sql += " AND Brand = @Brand";
                parameters.Add(new SqlParameter("@Brand", SqlDbType.NVarChar, 50) { Value = brand });
            }

            if (minPrice.HasValue && minPrice.Value > 0)
            {
                sql += " AND Price >= @MinPrice";
                parameters.Add(new SqlParameter("@MinPrice", SqlDbType.Decimal) { Value = minPrice.Value });
            }

            if (maxPrice.HasValue && maxPrice.Value > 0)
            {
                sql += " AND Price <= @MaxPrice";
                parameters.Add(new SqlParameter("@MaxPrice", SqlDbType.Decimal) { Value = maxPrice.Value });
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                sql += " AND Status = @Status";
                parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar, 20) { Value = status });
            }

            sql += " ORDER BY CarID DESC";

            return DatabaseHelper.ExecuteQuery(sql, parameters.ToArray());
        }

        public DataTable GetAvailableCars(string keyword = null, string brand = null, decimal? minPrice = null, decimal? maxPrice = null)
        {
            return GetAllCars(keyword, brand, minPrice, maxPrice, "Available");
        }

        public Car GetCarById(int carId)
        {
            string sql = @"SELECT CarID, Brand, Model, Year, Price, Status, Image_Path, CreatedAt 
                           FROM dbo.CarsStock 
                           WHERE CarID = @CarID";

            SqlParameter[] p = new SqlParameter[]
            {
                new SqlParameter("@CarID", SqlDbType.Int) { Value = carId }
            };

            DataTable dt = DatabaseHelper.ExecuteQuery(sql, p);
            if (dt.Rows.Count == 0) return null;

            DataRow r = dt.Rows[0];
            return new Car(
                Convert.ToInt32(r["CarID"]),
                r["Brand"].ToString(),
                r["Model"].ToString(),
                Convert.ToInt32(r["Year"]),
                Convert.ToDecimal(r["Price"]),
                r["Status"].ToString(),
                r["Image_Path"] != DBNull.Value ? r["Image_Path"].ToString() : null,
                Convert.ToDateTime(r["CreatedAt"])
            );
        }

        public bool AddCar(Car car, out string errorMessage)
        {
            errorMessage = null;

            if (car == null || !car.Validate(out errorMessage))
            {
                return false;
            }

            try
            {
                string sql = @"INSERT INTO dbo.CarsStock (Brand, Model, Year, Price, Status, Image_Path)
                               VALUES (@Brand, @Model, @Year, @Price, @Status, @ImagePath);
                               SELECT SCOPE_IDENTITY();";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@Brand", SqlDbType.NVarChar, 50) { Value = car.Brand },
                    new SqlParameter("@Model", SqlDbType.NVarChar, 50) { Value = car.Model },
                    new SqlParameter("@Year", SqlDbType.Int) { Value = car.Year },
                    new SqlParameter("@Price", SqlDbType.Decimal) { Value = car.Price },
                    new SqlParameter("@Status", SqlDbType.NVarChar, 20) { Value = car.Status ?? "Available" },
                    new SqlParameter("@ImagePath", SqlDbType.NVarChar) { Value = (object)car.ImagePath ?? DBNull.Value }
                };

                object newId = DatabaseHelper.ExecuteScalar(sql, parameters);
                if (newId != null && int.TryParse(newId.ToString(), out int id))
                {
                    car.CarID = id;
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = "Database error while adding car: " + ex.Message;
                return false;
            }
        }

        public bool UpdateCar(Car car, out string errorMessage)
        {
            errorMessage = null;

            if (car == null || car.CarID <= 0)
            {
                errorMessage = "Invalid vehicle record.";
                return false;
            }

            if (!car.Validate(out errorMessage))
            {
                return false;
            }

            try
            {
                // Status is driven by the sales workflow once a vehicle has an active order
                string requiredStatus = GetStatusLockedBySales(car.CarID);
                if (requiredStatus != null && car.Status != requiredStatus)
                {
                    errorMessage = requiredStatus == "Reserved"
                        ? "This vehicle has a pending purchase request. Confirm or cancel it in Sales before changing the status."
                        : "This vehicle has a confirmed sale, so its status must stay 'Sold'.";
                    return false;
                }

                string sql = @"UPDATE dbo.CarsStock 
                               SET Brand = @Brand, 
                                   Model = @Model, 
                                   Year = @Year, 
                                   Price = @Price, 
                                   Status = @Status, 
                                   Image_Path = @ImagePath
                               WHERE CarID = @CarID";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@Brand", SqlDbType.NVarChar, 50) { Value = car.Brand },
                    new SqlParameter("@Model", SqlDbType.NVarChar, 50) { Value = car.Model },
                    new SqlParameter("@Year", SqlDbType.Int) { Value = car.Year },
                    new SqlParameter("@Price", SqlDbType.Decimal) { Value = car.Price },
                    new SqlParameter("@Status", SqlDbType.NVarChar, 20) { Value = car.Status },
                    new SqlParameter("@ImagePath", SqlDbType.NVarChar) { Value = (object)car.ImagePath ?? DBNull.Value },
                    new SqlParameter("@CarID", SqlDbType.Int) { Value = car.CarID }
                };

                return DatabaseHelper.ExecuteNonQuery(sql, parameters) > 0;
            }
            catch (Exception ex)
            {
                errorMessage = "Database error while updating car: " + ex.Message;
                return false;
            }
        }

        public bool DeleteCar(int carId, out string errorMessage)
        {
            errorMessage = null;
            try
            {
                string lockedStatus = GetStatusLockedBySales(carId);
                if (lockedStatus != null)
                {
                    errorMessage = lockedStatus == "Reserved"
                        ? "This vehicle has a pending purchase request. Confirm or cancel it before removing the vehicle."
                        : "This vehicle has been sold. It is kept in the records and cannot be deleted.";
                    return false;
                }

                // Check if car has historical sales or service records
                string checkSql = @"SELECT 
                    (SELECT COUNT(*) FROM dbo.SalesRecord WHERE CarID = @CarID) +
                    (SELECT COUNT(*) FROM dbo.ServiceRecords WHERE CarID = @CarID)";

                SqlParameter[] p = new SqlParameter[] { new SqlParameter("@CarID", SqlDbType.Int) { Value = carId } };
                int count = Convert.ToInt32(DatabaseHelper.ExecuteScalar(checkSql, p));

                if (count > 0)
                {
                    // Soft delete or mark Unavailable
                    UpdateCarStatus(carId, "Unavailable", out _);
                    errorMessage = "Car has related sales or service transactions. Its status has been set to 'Unavailable' instead of deleting.";
                    return false;
                }

                string sql = "DELETE FROM dbo.CarsStock WHERE CarID = @CarID";
                return DatabaseHelper.ExecuteNonQuery(sql, p) > 0;
            }
            catch (Exception ex)
            {
                errorMessage = "Failed to delete car: " + ex.Message;
                return false;
            }
        }

        private static string GetStatusLockedBySales(int carId)
        {
            string sql = @"SELECT TOP 1 SalesStatus FROM dbo.SalesRecord
                           WHERE CarID = @CarID AND SalesStatus IN ('Pending', 'Confirmed')
                           ORDER BY CASE SalesStatus WHEN 'Pending' THEN 0 ELSE 1 END";
            object result = DatabaseHelper.ExecuteScalar(sql, new[]
            {
                new SqlParameter("@CarID", SqlDbType.Int) { Value = carId }
            });

            if (result == null) return null;
            return result.ToString() == "Pending" ? "Reserved" : "Sold";
        }

        public bool UpdateCarStatus(int carId, string newStatus, out string errorMessage)
        {
            errorMessage = null;
            try
            {
                string sql = "UPDATE dbo.CarsStock SET Status = @Status WHERE CarID = @CarID";
                SqlParameter[] p = new SqlParameter[]
                {
                    new SqlParameter("@Status", SqlDbType.NVarChar, 20) { Value = newStatus },
                    new SqlParameter("@CarID", SqlDbType.Int) { Value = carId }
                };
                return DatabaseHelper.ExecuteNonQuery(sql, p) > 0;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        public List<string> GetDistinctBrands()
        {
            List<string> brands = new List<string>();
            try
            {
                string sql = "SELECT DISTINCT Brand FROM dbo.CarsStock ORDER BY Brand";
                DataTable dt = DatabaseHelper.ExecuteQuery(sql);
                foreach (DataRow row in dt.Rows)
                {
                    brands.Add(row["Brand"].ToString());
                }
            }
            catch { }
            return brands;
        }
    }
}
