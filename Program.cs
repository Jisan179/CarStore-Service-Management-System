using System;
using System.Windows.Forms;
using CarStoreServiceManagement.Data;
using CarStoreServiceManagement.Forms;

namespace CarStoreServiceManagement
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Global Exception Handling to ensure application never crashes unexpectedly
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (sender, args) =>
            {
                MessageBox.Show(
                    "An unexpected application error occurred:\n\n" + args.Exception.Message,
                    "Application Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                if (args.ExceptionObject is Exception ex)
                {
                    MessageBox.Show(
                        "A critical system error occurred:\n\n" + ex.Message,
                        "Critical Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            };

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Verify or initialize database tables
            if (!DatabaseInitializer.InitializeDatabase(out string dbStatus))
            {
                MessageBox.Show(
                    dbStatus + "\n\nCheck that SQL Server is running and that the connection string in App.config is correct.",
                    "Database Problem",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            // Start application at the Login form
            Application.Run(new LoginForm());
        }
    }
}
