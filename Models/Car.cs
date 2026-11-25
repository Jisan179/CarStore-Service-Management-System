using System;

namespace CarStoreServiceManagement.Models
{
    /// <summary>
    /// Model representing a vehicle in CarsStock.
    /// Demonstrates encapsulation and validation logic.
    /// </summary>
    public class Car
    {
        // Private fields
        private int _carId;
        private string _brand;
        private string _model;
        private int _year;
        private decimal _price;
        private string _status;
        private string _imagePath;
        private DateTime _createdAt;

        // Default constructor
        public Car()
        {
            _status = "Available";
            _year = DateTime.Now.Year;
            _price = 0m;
            _createdAt = DateTime.Now;
        }

        // Parameterized constructor
        public Car(int carId, string brand, string model, int year, decimal price, string status, string imagePath, DateTime createdAt)
        {
            _carId = carId;
            _brand = brand;
            _model = model;
            _year = year;
            _price = price;
            _status = status;
            _imagePath = imagePath;
            _createdAt = createdAt;
        }

        // Public properties
        public int CarID
        {
            get => _carId;
            set => _carId = value;
        }

        public string Brand
        {
            get => _brand;
            set => _brand = value?.Trim();
        }

        public string Model
        {
            get => _model;
            set => _model = value?.Trim();
        }

        public int Year
        {
            get => _year;
            set => _year = value;
        }

        public decimal Price
        {
            get => _price;
            set => _price = value;
        }

        public string Status
        {
            get => _status;
            set => _status = value?.Trim();
        }

        public string ImagePath
        {
            get => _imagePath;
            set => _imagePath = value?.Trim();
        }

        public DateTime CreatedAt
        {
            get => _createdAt;
            set => _createdAt = value;
        }

        // Computed helper properties
        public string DisplayName => $"{Brand} {Model} ({Year})";

        public string FormattedPrice => Utilities.Money.Format(Price);

        public bool IsAvailable => string.Equals(_status, "Available", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Validates car business rules.
        /// </summary>
        public bool Validate(out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(Brand))
            {
                errorMessage = "Vehicle brand is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Model))
            {
                errorMessage = "Vehicle model is required.";
                return false;
            }

            if (Year < 1990 || Year > DateTime.Now.Year + 2)
            {
                errorMessage = $"Vehicle year must be between 1990 and {DateTime.Now.Year + 2}.";
                return false;
            }

            if (Price <= 0)
            {
                errorMessage = "Vehicle price must be greater than zero.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Status))
            {
                errorMessage = "Vehicle status is required.";
                return false;
            }

            errorMessage = null;
            return true;
        }
    }
}
