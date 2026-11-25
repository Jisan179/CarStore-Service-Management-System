using System;
using System.Windows.Forms;
using CarStoreServiceManagement.Models;

namespace CarStoreServiceManagement.Utilities
{
    /// <summary>
    /// Closes a form when the logged-in user does not have the required permission.
    /// The permission methods are overridden by each User subclass (polymorphism).
    /// </summary>
    public static class AccessGuard
    {
        public static void Require(Form form, Func<User, bool> permission)
        {
            form.Load += (s, e) =>
            {
                User user = SessionManager.CurrentUserObject;
                if (user == null || !permission(user))
                {
                    MessageBox.Show("Access Denied: your account does not have permission to open this window.",
                        "Security Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    form.Close();
                }
            };
        }
    }
}
