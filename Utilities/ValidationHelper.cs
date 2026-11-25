using System;
using System.Text.RegularExpressions;

namespace CarStoreServiceManagement.Utilities
{
    /// <summary>
    /// Centralized reusable data validation helper.
    /// Eliminates code duplication across forms and services.
    /// </summary>
    public static class ValidationHelper
    {
        private static readonly Regex EmailRegex = new Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static bool IsNotEmpty(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            return EmailRegex.IsMatch(email.Trim());
        }

        public static bool IsValidPrice(string text, out decimal price)
        {
            price = 0m;
            if (string.IsNullOrWhiteSpace(text))
                return false;

            // Remove currency symbols if present
            string clean = text.Replace("৳", "").Replace(",", "").Trim();
            if (decimal.TryParse(clean, out price))
            {
                // Money has at most two decimal places; anything longer is a typing mistake
                return price >= 0m && decimal.Round(price, 2) == price;
            }
            return false;
        }

        public static bool IsValidYear(string text, out int year)
        {
            year = 0;
            if (string.IsNullOrWhiteSpace(text))
                return false;

            if (int.TryParse(text.Trim(), out year))
            {
                return year >= 1990 && year <= DateTime.Now.Year + 2;
            }
            return false;
        }

        public static bool IsValidPassword(string password, out string validationError)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                validationError = "Password cannot be empty.";
                return false;
            }

            if (password.Length < 6)
            {
                validationError = "Password must be at least 6 characters long.";
                return false;
            }

            validationError = null;
            return true;
        }

        public static bool IsNumeric(string text, out int number)
        {
            number = 0;
            if (string.IsNullOrWhiteSpace(text))
                return false;

            return int.TryParse(text.Trim(), out number);
        }
    }
}
