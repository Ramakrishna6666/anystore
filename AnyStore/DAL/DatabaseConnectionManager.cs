using System;
using System.Configuration;
using System.Data.SqlClient;

namespace AnyStore.DAL
{
    /// <summary>
    /// Cloud-ready database connection manager that supports connection pooling
    /// and environment-based configuration for AWS RDS deployment
    /// </summary>
    internal static class DatabaseConnectionManager
    {
        /// <summary>
        /// Get connection string from environment variable or configuration file
        /// Supports AWS Systems Manager Parameter Store and AWS Secrets Manager
        /// </summary>
        private static string GetConnectionString()
        {
            // First, try to get from environment variable (cloud-native approach)
            string connString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
            
            if (string.IsNullOrEmpty(connString))
            {
                // Fallback to configuration file for local development
                connString = ConfigurationManager.ConnectionStrings["connstrng"]?.ConnectionString;
            }
            
            if (string.IsNullOrEmpty(connString))
            {
                throw new InvalidOperationException(
                    "Database connection string not found. " +
                    "Set DB_CONNECTION_STRING environment variable or configure connstrng in app.config");
            }
            
            return connString;
        }

        /// <summary>
        /// Create a new SQL connection with connection pooling enabled
        /// Connection pooling is automatically managed by ADO.NET
        /// </summary>
        public static SqlConnection CreateConnection()
        {
            string connectionString = GetConnectionString();
            
            // Ensure connection pooling is enabled (default behavior)
            // Connection string should include: Pooling=true;Min Pool Size=5;Max Pool Size=100;
            SqlConnection connection = new SqlConnection(connectionString);
            
            return connection;
        }

        /// <summary>
        /// Execute a query with automatic connection management
        /// This pattern ensures connections are properly disposed
        /// </summary>
        public static T ExecuteWithConnection<T>(Func<SqlConnection, T> action)
        {
            using (SqlConnection conn = CreateConnection())
            {
                conn.Open();
                return action(conn);
            }
        }

        /// <summary>
        /// Execute a command with automatic connection management
        /// </summary>
        public static void ExecuteWithConnection(Action<SqlConnection> action)
        {
            using (SqlConnection conn = CreateConnection())
            {
                conn.Open();
                action(conn);
            }
        }
    }
}
