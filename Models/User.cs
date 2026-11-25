using System;

namespace CarStoreServiceManagement.Models
{
    /// <summary>
    /// Base class representing a user in the CarStore and Service Management System.
    /// Demonstrates Object-Oriented Principles: Encapsulation, Abstraction, and Inheritance base.
    /// </summary>
    public class User
    {
        // Private fields (Encapsulation)
        private int _userId;
        private string _fullName;
        private string _username;
        private string _password;
        private string _email;
        private string _role;
        private string _status;
        private DateTime _createdAt;

        // Default constructor
        public User()
        {
            _role = "Customer";
            _status = "Active";
            _createdAt = DateTime.Now;
        }

        // Parameterized constructor
        public User(int userId, string fullName, string username, string password, string email, string role, string status, DateTime createdAt)
        {
            _userId = userId;
            _fullName = fullName;
            _username = username;
            _password = password;
            _email = email;
            _role = role;
            _status = status;
            _createdAt = createdAt;
        }

        // Public properties (Encapsulation with validation)
        public int UserID
        {
            get => _userId;
            set => _userId = value;
        }

        public string FullName
        {
            get => _fullName;
            set => _fullName = value?.Trim();
        }

        public string Username
        {
            get => _username;
            set => _username = value?.Trim();
        }

        public string Password
        {
            get => _password;
            set => _password = value;
        }

        public string Email
        {
            get => _email;
            set => _email = value?.Trim();
        }

        public string Role
        {
            get => _role;
            set => _role = value;
        }

        public string Status
        {
            get => _status;
            set => _status = value;
        }

        public DateTime CreatedAt
        {
            get => _createdAt;
            set => _createdAt = value;
        }

        // Polymorphic methods demonstrating behavior customization per role
        public virtual string GetDashboardTitle()
        {
            return "User Portal - CarStore Management System";
        }

        public virtual bool CanManageUsers()
        {
            return false;
        }

        public virtual bool CanManageCars()
        {
            return false;
        }

        public virtual bool CanProcessSales()
        {
            return false;
        }

        public virtual bool CanBookServices()
        {
            return false;
        }

        public virtual bool CanManageCatalog()
        {
            return false;
        }

        public virtual bool CanRequestPurchase()
        {
            return false;
        }

        public bool IsActive => string.Equals(_status, "Active", StringComparison.OrdinalIgnoreCase);

        // Factory method to create appropriate subclass based on role
        public static User CreateUserByRole(int userId, string fullName, string username, string password, string email, string role, string status, DateTime createdAt)
        {
            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                return new AdminUser(userId, fullName, username, password, email, status, createdAt);
            }
            else if (string.Equals(role, "Employee", StringComparison.OrdinalIgnoreCase))
            {
                return new EmployeeUser(userId, fullName, username, password, email, status, createdAt);
            }
            else
            {
                return new CustomerUser(userId, fullName, username, password, email, status, createdAt);
            }
        }
    }

    /// <summary>
    /// Administrator user with top-level administrative and system configuration privileges.
    /// Demonstrates Inheritance and Polymorphism (overriding permissions).
    /// </summary>
    public class AdminUser : User
    {
        public AdminUser() : base()
        {
            Role = "Admin";
        }

        public AdminUser(int userId, string fullName, string username, string password, string email, string status, DateTime createdAt)
            : base(userId, fullName, username, password, email, "Admin", status, createdAt)
        {
        }

        public override string GetDashboardTitle()
        {
            return "Admin Portal - Executive System Dashboard";
        }

        public override bool CanManageUsers() => true;
        public override bool CanManageCars() => true;
        public override bool CanProcessSales() => true;
        public override bool CanBookServices() => true;
        public override bool CanManageCatalog() => true;
    }

    /// <summary>
    /// Staff/Employee user responsible for inventory management, sale confirmations, and service servicing.
    /// Demonstrates Inheritance and Polymorphism.
    /// </summary>
    public class EmployeeUser : User
    {
        public EmployeeUser() : base()
        {
            Role = "Employee";
        }

        public EmployeeUser(int userId, string fullName, string username, string password, string email, string status, DateTime createdAt)
            : base(userId, fullName, username, password, email, "Employee", status, createdAt)
        {
        }

        public override string GetDashboardTitle()
        {
            return "Employee Portal - Operations & Sales Center";
        }

        public override bool CanManageUsers() => false;
        public override bool CanManageCars() => true;
        public override bool CanProcessSales() => true;
        public override bool CanBookServices() => true;
        public override bool CanManageCatalog() => false;
    }

    /// <summary>
    /// Customer user authorized to browse vehicles, submit purchase requests, and book service appointments.
    /// Demonstrates Inheritance and Polymorphism.
    /// </summary>
    public class CustomerUser : User
    {
        public CustomerUser() : base()
        {
            Role = "Customer";
        }

        public CustomerUser(int userId, string fullName, string username, string password, string email, string status, DateTime createdAt)
            : base(userId, fullName, username, password, email, "Customer", status, createdAt)
        {
        }

        public override string GetDashboardTitle()
        {
            return "Customer Hub - Welcome to CarStore & Service Center";
        }

        public override bool CanManageUsers() => false;
        public override bool CanManageCars() => false;
        public override bool CanProcessSales() => false;
        public override bool CanBookServices() => true;
        public override bool CanManageCatalog() => false;
        public override bool CanRequestPurchase() => true;
    }
}
