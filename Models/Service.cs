using System;

namespace CarStoreServiceManagement.Models
{
    /// <summary>
    /// Model representing an automobile service offering in ServiceList catalog.
    /// </summary>
    public class Service
    {
        public int ServiceID { get; set; }
        public string ServiceName { get; set; }
        public decimal Price { get; set; }

        public string FormattedPrice => Utilities.Money.Format(Price);
        public string DisplayName => $"{ServiceName} - {FormattedPrice}";

        public Service()
        {
            Price = 0m;
        }

        public Service(int serviceId, string serviceName, decimal price)
        {
            ServiceID = serviceId;
            ServiceName = serviceName;
            Price = price;
        }

        public bool Validate(out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(ServiceName))
            {
                errorMessage = "Service name is required.";
                return false;
            }

            if (Price < 0)
            {
                errorMessage = "Service price cannot be negative.";
                return false;
            }

            errorMessage = null;
            return true;
        }
    }
}
