using System;

namespace CarStoreServiceManagement.Models
{
    /// <summary>
    /// Model representing a maintenance or repair service transaction in ServiceRecords.
    /// Maps to ServiceRecords table with relational join properties.
    /// </summary>
    public class ServiceRecord
    {
        public int RecordID { get; set; }
        public int CarID { get; set; }
        public int CustomerID { get; set; }
        public int? ServiceID { get; set; }
        public DateTime ServiceDate { get; set; }
        public string ServiceDetails { get; set; }
        public decimal ServiceCost { get; set; }
        public string ServiceStatus { get; set; }

        // Relational display properties populated via SQL JOINs
        public string CarBrand { get; set; }
        public string CarModel { get; set; }
        public int CarYear { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string ServiceName { get; set; }

        public string VehicleName => !string.IsNullOrEmpty(CarBrand) ? $"{CarBrand} {CarModel} ({CarYear})" : $"Car #{CarID}";
        public string ServiceTitle => !string.IsNullOrEmpty(ServiceName) ? ServiceName : "Custom Maintenance";
        public string FormattedCost => Utilities.Money.Format(ServiceCost);
        public string FormattedDate => ServiceDate.ToString("yyyy-MM-dd HH:mm");

        public ServiceRecord()
        {
            ServiceDate = DateTime.Now;
            ServiceStatus = "Pending";
            ServiceCost = 0m;
        }
    }
}
