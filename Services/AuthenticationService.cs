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
    /// Service responsible for authentication, login verification, session initiation, and registration.
    /// Uses parameterized queries to prevent SQL injection vulnerabilities.
    /// </summary>
    public class AuthenticationService
    {
        private const int MaxFailedAttempts = 5;
        private static readonly TimeSpan LockDuration = TimeSpan.FromMinutes(1);

        // Kept in memory only: restarting the application clears it
        private static readonly Dictionary<string, int> FailedAttempts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, DateTime> LockedUntil = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);

        private static string RegisterFailure(string username)
        {
            FailedAttempts.TryGetValue(username, out int count);
            if (++count >= MaxFailedAttempts)
            {
                FailedAttempts.Remove(username);
                LockedUntil[username] = DateTime.UtcNow + LockDuration;
                return "Too many failed attempts. Please wait 1 minute and try again.";
            }

            FailedAttempts[username] = count;
            return "Invalid username or password.";
        }

        /// <summary>
        /// Authenticates user credentials, verifies account status, and initializes session.
        /// Returns appropriate User model subclass instance on success.
        /// </summary>
        public User Authenticate(string username, string password, out string errorMessage)
        {
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                errorMessage = "Username and password cannot be empty.";
                return null;
            }

            string loginName = username.Trim();
            if (LockedUntil.TryGetValue(loginName, out DateTime until) && until > DateTime.UtcNow)
            {
                int seconds = (int)Math.Ceiling((until - DateTime.UtcNow).TotalSeconds);
                errorMessage = $"Too many failed attempts. Try again in {seconds} seconds.";
                return null;
            }

            try
            {
                string sql = @"SELECT UserID, FullName, Username, Password, Email, Role, Status, CreatedAt 
                               FROM dbo.Users 
                               WHERE Username = @Username";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@Username", SqlDbType.NVarChar, 50) { Value = username.Trim() }
                };

                DataTable dt = DatabaseHelper.ExecuteQuery(sql, parameters);
                if (dt.Rows.Count == 0)
                {
                    errorMessage = RegisterFailure(loginName);
                    return null;
                }

                DataRow row = dt.Rows[0];
                string storedPassword = row["Password"].ToString();
                string status = row["Status"].ToString();

                // Verify password
                if (!PasswordHelper.VerifyPassword(password, storedPassword))
                {
                    errorMessage = RegisterFailure(loginName);
                    return null;
                }

                // Check account activation status
                if (!string.Equals(status, "Active", StringComparison.OrdinalIgnoreCase))
                {
                    errorMessage = "This account is currently inactive. Please contact the administrator.";
                    return null;
                }

                FailedAttempts.Remove(loginName);
                LockedUntil.Remove(loginName);

                // Map database record to OOP model hierarchy
                int userId = Convert.ToInt32(row["UserID"]);
                string fullName = row["FullName"].ToString();
                string userEmail = row["Email"].ToString();
                string role = row["Role"].ToString();
                DateTime createdAt = Convert.ToDateTime(row["CreatedAt"]);

                User user = User.CreateUserByRole(userId, fullName, username, storedPassword, userEmail, role, status, createdAt);

                // Set global session
                SessionManager.SetSession(user);

                return user;
            }
            catch (SqlException ex)
            {
                errorMessage = "Database connection error: " + ex.Message;
                return null;
            }
            catch (Exception ex)
            {
                errorMessage = "An unexpected error occurred: " + ex.Message;
                return null;
            }
        }

        /// <summary>
        /// Registers a new customer in the system.
        /// Defaults Role = 'Customer' and Status = 'Active'.
        /// </summary>
        public bool RegisterCustomer(string fullName, string username, string email, string password, out string errorMessage)
        {
            errorMessage = null;

            // 1. Field validation
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                errorMessage = "All registration fields are required.";
                return false;
            }

            if (!ValidationHelper.IsValidEmail(email))
            {
                errorMessage = "Please provide a valid email address.";
                return false;
            }

            if (!ValidationHelper.IsValidPassword(password, out string passErr))
            {
                errorMessage = passErr;
                return false;
            }

            try
            {
                // 2. Check for duplicate username
                string checkSql = "SELECT COUNT(*) FROM dbo.Users WHERE Username = @Username";
                SqlParameter[] checkParams = new SqlParameter[]
                {
                    new SqlParameter("@Username", SqlDbType.NVarChar, 50) { Value = username.Trim() }
                };

                int count = Convert.ToInt32(DatabaseHelper.ExecuteScalar(checkSql, checkParams));
                if (count > 0)
                {
                    errorMessage = "The chosen username is already taken. Please choose another.";
                    return false;
                }

                // 3. Insert new customer
                string insertSql = @"INSERT INTO dbo.Users (FullName, Username, Password, Email, Role, Status) 
                                     VALUES (@FullName, @Username, @Password, @Email, 'Customer', 'Active')";

                SqlParameter[] insertParams = new SqlParameter[]
                {
                    new SqlParameter("@FullName", SqlDbType.NVarChar, 100) { Value = fullName.Trim() },
                    new SqlParameter("@Username", SqlDbType.NVarChar, 50) { Value = username.Trim() },
                    new SqlParameter("@Password", SqlDbType.NVarChar, 200) { Value = PasswordHelper.HashPassword(password) },
                    new SqlParameter("@Email", SqlDbType.NVarChar, 100) { Value = email.Trim() }
                };

                int rows = DatabaseHelper.ExecuteNonQuery(insertSql, insertParams);
                return rows > 0;
            }
            catch (SqlException ex)
            {
                errorMessage = "Database error during registration: " + ex.Message;
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = "Registration error: " + ex.Message;
                return false;
            }
        }
    }
}
