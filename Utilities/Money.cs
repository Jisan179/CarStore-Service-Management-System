using System.Globalization;
using System.Windows.Forms;

namespace CarStoreServiceManagement.Utilities
{
    /// <summary>
    /// One place for showing amounts in Bangladeshi Taka with lakh/crore grouping (e.g. ৳ 35,00,000).
    /// </summary>
    public static class Money
    {
        private const string Pattern = "'৳' #,0.##";
        private static readonly CultureInfo Bangladesh = CreateCulture();

        private static CultureInfo CreateCulture()
        {
            var culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            culture.NumberFormat.NumberGroupSizes = new[] { 3, 2 };
            return CultureInfo.ReadOnly(culture);
        }

        public static string Format(decimal amount)
        {
            return amount.ToString(Pattern, Bangladesh);
        }

        public static void FormatColumn(DataGridViewColumn column)
        {
            column.DefaultCellStyle.Format = Pattern;
            column.DefaultCellStyle.FormatProvider = Bangladesh;
            column.MinimumWidth = 110;
        }
    }
}
