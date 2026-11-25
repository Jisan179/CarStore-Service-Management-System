using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CarStoreServiceManagement.Data;
using CarStoreServiceManagement.Models;
using CarStoreServiceManagement.Utilities;

namespace CarStoreServiceManagement.Services
{
    /// <summary>
    /// Service for managing user accounts, admin operations, and user profiles.
    /// </summary>
    public class UserService
    {
        public DataTable GetAllUsers(string searchKeyword = null, string roleFilter = null)
        {
            string sql = @"SELECT UserID, FullName, Username, Email, Role, Status, CreatedAt 
                           FROM dbo.Users 
                           WHERE 1=1";

            List<SqlParameter> parameters = new List<SqlParameter>();

            if (!string.IsNullOrWhiteSpace(searchKeyword))
            {
                sql += " AND (FullName LIKE @Keyword OR Username LIKE @Keyword OR Email LIKE @Keyword)";
                parameters.Add(new SqlParameter("@Keyword", SqlDbType.NVarChar, 100) { Value = "%" + searchKeyword.Trim() + "%" });
            }

            if (!string.IsNullOrWhiteSpace(roleFilter) && roleFilter != "All")
            {
                sql += " AND Role = @Role";
                parameters.Add(new SqlParameter("@Role", SqlDbType.VarChar, 20) { Value = roleFilter });
            }

            sql += " ORDER BY UserID DESC";

            return DatabaseHelper.ExecuteQuery(sql, parameters.ToArray());
        }

        public User GetUserById(int userId)
        {
            string sql = @"SELECT UserID, FullName, Username, Password, Email, Role, Status, CreatedAt 
                           FROM dbo.Users WHERE UserID = @UserID";
            SqlParameter[] p = new SqlParameter[]
            {
                new SqlParameter("@UserID", SqlDbType.Int) { Value = userId }
            };

            DataTable dt = DatabaseHelper.ExecuteQuery(sql, p);
            if (dt.Rows.Count == 0) return null;

            DataRow r = dt.Rows[0];
            return User.CreateUserByRole(
                Convert.ToInt32(r["UserID"]),
                r["FullName"].ToString(),
                r["Username"].ToString(),
                r["Password"].ToString(),
                r["Email"].ToString(),
                r["Role"].ToString(),
                r["Status"].ToString(),
                Convert.ToDateTime(r["CreatedAt"])
            );
        }

        public bool AddUser(User user, out string errorMessage)
        {
            errorMessage = null;

            if (user == null || string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Password))
            {
                errorMessage = "Username and password are required.";
                return false;
            }

            if (!ValidationHelper.IsValidEmail(user.Email))
            {
                errorMessage = "Invalid email format.";
                return false;
            }

            try
            {
                // Check duplicate username
                string checkSql = "SELECT COUNT(*) FROM dbo.Users WHERE Username = @Username";
                SqlParameter[] cp = new SqlParameter[]
                {
                    new SqlParameter("@Username", SqlDbType.NVarChar, 50) { Value = user.Username.Trim() }
                };
                if (Convert.ToInt32(DatabaseHelper.ExecuteScalar(checkSql, cp)) > 0)
                {
                    errorMessage = "Username already exists.";
                    return false;
                }

                string sql = @"INSERT INTO dbo.Users (FullName, Username, Password, Email, Role, Status)
                               VALUES (@FullName, @Username, @Password, @Email, @Role, @Status)";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@FullName", SqlDbType.NVarChar, 100) { Value = user.FullName ?? "" },
                    new SqlParameter("@Username", SqlDbType.NVarChar, 50) { Value = user.Username.Trim() },
                    new SqlParameter("@Password", SqlDbType.NVarChar, 200) { Value = PasswordHelper.HashPassword(user.Password) },
                    new SqlParameter("@Email", SqlDbType.NVarChar, 100) { Value = user.Email.Trim() },
                    new SqlParameter("@Role", SqlDbType.VarChar, 20) { Value = user.Role },
                    new SqlParameter("@Status", SqlDbType.NVarChar, 20) { Value = user.Status ?? "Active" }
                };

                return DatabaseHelper.ExecuteNonQuery(sql, parameters) > 0;
            }
            catch (Exception ex)
            {
                errorMessage = "Database error: " + ex.Message;
                return false;
            }
        }

        public bool UpdateUser(User user, out string errorMessage)
        {
            errorMessage = null;
            if (user == null || user.UserID <= 0)
            {
                errorMessage = "Invalid user record.";
                return false;
            }

            if (!ValidationHelper.IsValidEmail(user.Email))
            {
                errorMessage = "Invalid email format.";
                return false;
            }

            if (!string.IsNullOrEmpty(user.Password) && !ValidationHelper.IsValidPassword(user.Password, out errorMessage))
            {
                return false;
            }

            bool isSelf = user.UserID == SessionManager.CurrentUserID;
            if (isSelf && (user.Status != "Active" || !string.Equals(user.Role, SessionManager.CurrentRole, StringComparison.OrdinalIgnoreCase)))
            {
                errorMessage = "You cannot deactivate your own account or change your own role.";
                return false;
            }

            try
            {
                // Check if username changed and is taken
                string checkSql = "SELECT COUNT(*) FROM dbo.Users WHERE Username = @Username AND UserID <> @UserID";
                SqlParameter[] cp = new SqlParameter[]
                {
                    new SqlParameter("@Username", SqlDbType.NVarChar, 50) { Value = user.Username.Trim() },
                    new SqlParameter("@UserID", SqlDbType.Int) { Value = user.UserID }
                };
                if (Convert.ToInt32(DatabaseHelper.ExecuteScalar(checkSql, cp)) > 0)
                {
                    errorMessage = "Username is already used by another user.";
                    return false;
                }

                string sql = @"UPDATE dbo.Users 
                               SET FullName = @FullName, 
                                   Username = @Username, 
                                   Email = @Email, 
                                   Role = @Role, 
                                   Status = @Status";

                List<SqlParameter> parameters = new List<SqlParameter>
                {
                    new SqlParameter("@FullName", SqlDbType.NVarChar, 100) { Value = user.FullName ?? "" },
                    new SqlParameter("@Username", SqlDbType.NVarChar, 50) { Value = user.Username.Trim() },
                    new SqlParameter("@Email", SqlDbType.NVarChar, 100) { Value = user.Email.Trim() },
                    new SqlParameter("@Role", SqlDbType.VarChar, 20) { Value = user.Role },
                    new SqlParameter("@Status", SqlDbType.NVarChar, 20) { Value = user.Status },
                    new SqlParameter("@UserID", SqlDbType.Int) { Value = user.UserID }
                };

                if (!string.IsNullOrEmpty(user.Password))
                {
                    sql += ", Password = @Password";
                    parameters.Add(new SqlParameter("@Password", SqlDbType.NVarChar, 200) { Value = PasswordHelper.HashPassword(user.Password) });
                }

                sql += " WHERE UserID = @UserID";

                bool updated = DatabaseHelper.ExecuteNonQuery(sql, parameters.ToArray()) > 0;
                if (updated && isSelf)
                {
                    SessionManager.UpdateCurrentProfile(user.FullName, user.Email);
                }
                return updated;
            }
            catch (Exception ex)
            {
                errorMessage = "Database error: " + ex.Message;
                return false;
            }
        }

        public bool SetUserStatus(int userId, string status, out string errorMessage)
        {
            errorMessage = null;
            if (userId == SessionManager.CurrentUserID && status != "Active")
            {
                errorMessage = "You cannot deactivate your own logged-in account.";
                return false;
            }
            try
            {
                string sql = "UPDATE dbo.Users SET Status = @Status WHERE UserID = @UserID";
                SqlParameter[] p = new SqlParameter[]
                {
                    new SqlParameter("@Status", SqlDbType.NVarChar, 20) { Value = status },
                    new SqlParameter("@UserID", SqlDbType.Int) { Value = userId }
                };
                return DatabaseHelper.ExecuteNonQuery(sql, p) > 0;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        public bool DeleteUser(int userId, out string errorMessage)
        {
            errorMessage = null;

            // Prevent deleting active session user
            if (userId == SessionManager.CurrentUserID)
            {
                errorMessage = "You cannot delete your own logged-in account.";
                return false;
            }

            try
            {
                // Check if user has sales or service records
                string checkSql = @"SELECT 
                    (SELECT COUNT(*) FROM dbo.SalesRecord WHERE CustomerID = @UID OR EmployeeID = @UID) +
                    (SELECT COUNT(*) FROM dbo.ServiceRecords WHERE CustomerID = @UID)";
                SqlParameter[] cp = new SqlParameter[] { new SqlParameter("@UID", SqlDbType.Int) { Value = userId } };
                int relatedCount = Convert.ToInt32(DatabaseHelper.ExecuteScalar(checkSql, cp));

                if (relatedCount > 0)
                {
                    // Soft delete by deactivating instead of violating FK
                    SetUserStatus(userId, "Inactive", out _);
                    errorMessage = "User has historical sales or service records. Account has been deactivated instead of permanently deleted to preserve audit integrity.";
                    return false;
                }

                string sql = "DELETE FROM dbo.Users WHERE UserID = @UserID";
                SqlParameter[] p = new SqlParameter[]
                {
                    new SqlParameter("@UserID", SqlDbType.Int) { Value = userId }
                };
                return DatabaseHelper.ExecuteNonQuery(sql, p) > 0;
            }
            catch (Exception ex)
            {
                errorMessage = "Failed to delete user: " + ex.Message;
                return false;
            }
        }

        public bool UpdateProfile(int userId, string fullName, string email, out string errorMessage)
        {
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(fullName))
            {
                errorMessage = "Full Name cannot be empty.";
                return false;
            }

            if (!ValidationHelper.IsValidEmail(email))
            {
                errorMessage = "Please enter a valid email address.";
                return false;
            }

            try
            {
                string sql = "UPDATE dbo.Users SET FullName = @FullName, Email = @Email WHERE UserID = @UserID";
                SqlParameter[] p = new SqlParameter[]
                {
                    new SqlParameter("@FullName", SqlDbType.NVarChar, 100) { Value = fullName.Trim() },
                    new SqlParameter("@Email", SqlDbType.NVarChar, 100) { Value = email.Trim() },
                    new SqlParameter("@UserID", SqlDbType.Int) { Value = userId }
                };

                int rows = DatabaseHelper.ExecuteNonQuery(sql, p);
                if (rows > 0 && userId == SessionManager.CurrentUserID)
                {
                    SessionManager.UpdateCurrentProfile(fullName.Trim(), email.Trim());
                }
                return rows > 0;
            }
            catch (Exception ex)
            {
                errorMessage = "Profile update failed: " + ex.Message;
                return false;
            }
        }

        public bool ChangePassword(int userId, string currentPassword, string newPassword, out string errorMessage)
        {
            errorMessage = null;

            if (!ValidationHelper.IsValidPassword(newPassword, out string passErr))
            {
                errorMessage = passErr;
                return false;
            }

            try
            {
                string fetchSql = "SELECT Password FROM dbo.Users WHERE UserID = @UserID";
                SqlParameter[] fp = new SqlParameter[] { new SqlParameter("@UserID", SqlDbType.Int) { Value = userId } };
                object result = DatabaseHelper.ExecuteScalar(fetchSql, fp);

                if (result == null || !PasswordHelper.VerifyPassword(currentPassword, result.ToString()))
                {
                    errorMessage = "Current password is incorrect.";
                    return false;
                }

                string updateSql = "UPDATE dbo.Users SET Password = @Password WHERE UserID = @UserID";
                SqlParameter[] up = new SqlParameter[]
                {
                    new SqlParameter("@Password", SqlDbType.NVarChar, 200) { Value = PasswordHelper.HashPassword(newPassword) },
                    new SqlParameter("@UserID", SqlDbType.Int) { Value = userId }
                };

                return DatabaseHelper.ExecuteNonQuery(updateSql, up) > 0;
            }
            catch (Exception ex)
            {
                errorMessage = "Password change failed: " + ex.Message;
                return false;
            }
        }
    }
}
