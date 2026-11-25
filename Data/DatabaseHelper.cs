using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace CarStoreServiceManagement.Data
{
    /// <summary>
    /// Centralized database helper for ADO.NET SQL operations.
    /// Manages connections, executes parameterized queries, and handles transactions safely.
    /// </summary>
    public static class DatabaseHelper
    {
        private static string _connectionString;

        static DatabaseHelper()
        {
            try
            {
                var connSettings = ConfigurationManager.ConnectionStrings["CarStoreDB"];
                if (connSettings != null && !string.IsNullOrWhiteSpace(connSettings.ConnectionString))
                {
                    _connectionString = connSettings.ConnectionString;
                }
                else
                {
                    // Fallback to local SQL Server Express
                    _connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=CarStoreDB;Integrated Security=True;TrustServerCertificate=True;";
                }
            }
            catch
            {
                _connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=CarStoreDB;Integrated Security=True;TrustServerCertificate=True;";
            }
        }

        public static string ConnectionString
        {
            get => _connectionString;
            set => _connectionString = value;
        }

        /// <summary>
        /// Creates and returns a new SqlConnection object.
        /// </summary>
        public static SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        /// <summary>
        /// Tests if the database connection can be opened successfully.
        /// </summary>
        public static bool TestConnection(out string errorMessage)
        {
            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    errorMessage = null;
                    return true;
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Executes a SELECT query with parameters and returns a DataTable.
        /// </summary>
        public static DataTable ExecuteQuery(string query, SqlParameter[] parameters = null)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    if (parameters != null && parameters.Length > 0)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }

                    conn.Open();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                        cmd.Parameters.Clear(); // lets the caller reuse the parameters
                    }
                }
            }
            return dt;
        }

        /// <summary>
        /// Executes an INSERT, UPDATE, or DELETE query with parameters and returns rows affected.
        /// </summary>
        public static int ExecuteNonQuery(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection conn = GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    if (parameters != null && parameters.Length > 0)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }

                    conn.Open();
                    var result = cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear(); // lets the caller reuse the parameters
                    return result;
                }
            }
        }

        /// <summary>
        /// Executes a query that returns a single scalar value (e.g. COUNT, SCOPE_IDENTITY).
        /// </summary>
        public static object ExecuteScalar(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection conn = GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    if (parameters != null && parameters.Length > 0)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }

                    conn.Open();
                    var result = cmd.ExecuteScalar();
                    cmd.Parameters.Clear(); // lets the caller reuse the parameters
                    return result;
                }
            }
        }

        /// <summary>
        /// Executes multiple operations inside an ACID SQL Transaction.
        /// Automatically rolls back if an exception occurs or the callback returns false.
        /// </summary>
        public static bool ExecuteTransaction(Func<SqlConnection, SqlTransaction, bool> action, out string errorMessage)
        {
            errorMessage = null;
            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    // An exception disposes the uncommitted transaction, which rolls it back.
                    using (SqlTransaction trans = conn.BeginTransaction())
                    {
                        if (action(conn, trans))
                        {
                            trans.Commit();
                            return true;
                        }

                        trans.Rollback();
                        errorMessage = "The operation could not be completed.";
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }
    }
}
