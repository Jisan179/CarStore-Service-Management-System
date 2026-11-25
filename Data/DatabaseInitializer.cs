using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;
using CarStoreServiceManagement.Utilities;

namespace CarStoreServiceManagement.Data
{
    /// <summary>
    /// Checks database availability and auto-seeds tables/records if needed.
    /// Ensures seamless setup across different academic testing environments.
    /// </summary>
    public static class DatabaseInitializer
    {
        public static bool InitializeDatabase(out string statusMessage)
        {
            try
            {
                var builder = new SqlConnectionStringBuilder(DatabaseHelper.ConnectionString);
                string databaseName = builder.InitialCatalog;
                builder.InitialCatalog = "master";

                using (SqlConnection masterConn = new SqlConnection(builder.ConnectionString))
                {
                    masterConn.Open();

                    // CarStoreDB.sql drops and recreates every table, so it must only run
                    // when there is no application data at all. A failed connection to the
                    // application database is never treated as "database is empty".
                    if (CountApplicationTables(masterConn, databaseName) == 0)
                    {
                        if (!RunSetupScript(masterConn, out statusMessage))
                            return false;
                    }
                }

                UpgradePasswordStorage();

                statusMessage = "Database is ready.";
                return true;
            }
            catch (Exception ex)
            {
                statusMessage = "Database initialization failed: " + ex.Message;
                return false;
            }
        }

        private static int CountApplicationTables(SqlConnection masterConn, string databaseName)
        {
            using (SqlCommand cmd = new SqlCommand("SELECT DB_ID(@Name)", masterConn))
            {
                cmd.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 128) { Value = databaseName });
                if (cmd.ExecuteScalar() == DBNull.Value)
                    return 0;
            }

            string quoted = "[" + databaseName.Replace("]", "]]") + "]";
            string sql = "SELECT COUNT(*) FROM " + quoted + ".sys.tables " +
                         "WHERE name IN ('Users', 'CarsStock', 'SalesRecord', 'ServiceList', 'ServiceRecords')";
            using (SqlCommand cmd = new SqlCommand(sql, masterConn))
            {
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private static bool RunSetupScript(SqlConnection masterConn, out string statusMessage)
        {
            string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "CarStoreDB.sql");
            if (!File.Exists(scriptPath))
            {
                statusMessage = "Database/CarStoreDB.sql script not found for auto-initialization.";
                return false;
            }

            string script = File.ReadAllText(scriptPath);
            string[] batches = Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

            foreach (string batch in batches)
            {
                string trimmed = batch.Trim();
                if (trimmed.Length > 0)
                {
                    using (SqlCommand cmd = new SqlCommand(trimmed, masterConn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            statusMessage = "Database created and seeded successfully.";
            return true;
        }

        /// <summary>
        /// Widens the password column if needed and replaces any plain-text password
        /// (seed rows or rows from an older version) with a salted PBKDF2 hash.
        /// </summary>
        private static void UpgradePasswordStorage()
        {
            DatabaseHelper.ExecuteNonQuery(
                "IF COL_LENGTH('dbo.Users', 'Password') < 400 " +
                "ALTER TABLE dbo.Users ALTER COLUMN [Password] NVARCHAR(200) NOT NULL");

            DataTable users = DatabaseHelper.ExecuteQuery("SELECT UserID, [Password] FROM dbo.Users");
            foreach (DataRow row in users.Rows)
            {
                string stored = row["Password"].ToString();
                if (PasswordHelper.IsHashed(stored))
                    continue;

                DatabaseHelper.ExecuteNonQuery(
                    "UPDATE dbo.Users SET [Password] = @Password WHERE UserID = @UserID",
                    new[]
                    {
                        new SqlParameter("@Password", SqlDbType.NVarChar, 200) { Value = PasswordHelper.HashPassword(stored) },
                        new SqlParameter("@UserID", SqlDbType.Int) { Value = row["UserID"] }
                    });
            }
        }
    }
}
