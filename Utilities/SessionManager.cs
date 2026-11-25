using System;
using CarStoreServiceManagement.Models;

namespace CarStoreServiceManagement.Utilities
{
    /// <summary>
    /// Static session manager holding the state of the currently authenticated user.
    /// Accessible throughout all forms to enforce role-based access control.
    /// </summary>
    public static class SessionManager
    {
        public static int CurrentUserID { get; private set; }
        public static string CurrentUsername { get; private set; }
        public static string CurrentFullName { get; private set; }
        public static string CurrentEmail { get; private set; }
        public static string CurrentRole { get; private set; }
        public static DateTime LoginTime { get; private set; }
        public static User CurrentUserObject { get; private set; }

        public static bool IsLoggedIn => CurrentUserID > 0 && !string.IsNullOrEmpty(CurrentUsername);
        public static bool IsAdmin => string.Equals(CurrentRole, "Admin", StringComparison.OrdinalIgnoreCase);
        public static bool IsEmployee => string.Equals(CurrentRole, "Employee", StringComparison.OrdinalIgnoreCase);
        public static bool IsCustomer => string.Equals(CurrentRole, "Customer", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Initializes the session with an authenticated user entity.
        /// </summary>
        public static void SetSession(User user)
        {
            if (user == null)
            {
                ClearSession();
                return;
            }

            CurrentUserID = user.UserID;
            CurrentUsername = user.Username;
            CurrentFullName = user.FullName;
            CurrentEmail = user.Email;
            CurrentRole = user.Role;
            CurrentUserObject = user;
            LoginTime = DateTime.Now;
        }

        /// <summary>
        /// Updates the profile information in session after a user edits their profile.
        /// </summary>
        public static void UpdateCurrentProfile(string fullName, string email)
        {
            CurrentFullName = fullName;
            CurrentEmail = email;
            if (CurrentUserObject != null)
            {
                CurrentUserObject.FullName = fullName;
                CurrentUserObject.Email = email;
            }
        }

        /// <summary>
        /// Destroys the active session on user logout.
        /// </summary>
        public static void ClearSession()
        {
            CurrentUserID = 0;
            CurrentUsername = null;
            CurrentFullName = null;
            CurrentEmail = null;
            CurrentRole = null;
            CurrentUserObject = null;
            LoginTime = DateTime.MinValue;
        }
    }
}
