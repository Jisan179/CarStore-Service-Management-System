using System;

namespace CarStoreServiceManagement.Models
{
    /// <summary>
    /// Represents a vehicle purchase order or sales record.
    /// Maps to SalesRecord table with relational join projection properties.
    /// </summary>
    public class SalesRecord
    {
        public int SalesID { get; set; }
        public int CarID { get; set; }
        public int CustomerID { get; set; }
        public int? EmployeeID { get; set; }
        public DateTime SaleDate { get; set; }
        public decimal Price { get; set; }
        public string SalesStatus { get; set; }

        // Relational display properties populated via SQL JOINs
        public string CarBrand { get; set; }
        public string CarModel { get; set; }
        public int CarYear { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string EmployeeName { get; set; }

        public string VehicleName => !string.IsNullOrEmpty(CarBrand) ? $"{CarBrand} {CarModel} ({CarYear})" : $"Car #{CarID}";
        public string FormattedPrice => Utilities.Money.Format(Price);
        public string FormattedDate => SaleDate.ToString("yyyy-MM-dd HH:mm");

        public SalesRecord()
        {
            SaleDate = DateTime.Now;
            SalesStatus = "Pending";
        }
    }
}
