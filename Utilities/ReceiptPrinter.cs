using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace CarStoreServiceManagement.Utilities
{
    /// <summary>
    /// Shows a one-page receipt in a print preview window (print to paper or "Microsoft Print to PDF").
    /// </summary>
    public static class ReceiptPrinter
    {
        public static void PreviewSale(DataGridViewRow row, string vehicleColumn, string customerName)
        {
            if (!HasStatus(row, "SalesStatus", "Confirmed", "A receipt is available only for a confirmed sale."))
                return;

            int id = Convert.ToInt32(row.Cells["SalesID"].Value);
            Preview("Vehicle Sales Receipt", "SALE-" + id.ToString("D5"), new List<KeyValuePair<string, string>>
            {
                Line("Customer", customerName),
                Line("Vehicle", Text(row, vehicleColumn)),
                Line("Order Date", Convert.ToDateTime(row.Cells["SaleDate"].Value).ToString("dd MMM yyyy")),
                Line("Status", "Confirmed")
            }, Convert.ToDecimal(row.Cells["Price"].Value));
        }

        public static void PreviewService(DataGridViewRow row, string statusColumn, string vehicleColumn,
                                          string serviceColumn, string costColumn, string customerName)
        {
            if (!HasStatus(row, statusColumn, "Completed", "A receipt is available only for a completed service."))
                return;

            int id = Convert.ToInt32(row.Cells["RecordID"].Value);
            Preview("Service Receipt", "SRV-" + id.ToString("D5"), new List<KeyValuePair<string, string>>
            {
                Line("Customer", customerName),
                Line("Vehicle", Text(row, vehicleColumn)),
                Line("Service", Text(row, serviceColumn)),
                Line("Appointment", Convert.ToDateTime(row.Cells["ServiceDate"].Value).ToString("dd MMM yyyy hh:mm tt")),
                Line("Status", "Completed")
            }, Convert.ToDecimal(row.Cells[costColumn].Value));
        }

        private static bool HasStatus(DataGridViewRow row, string column, string required, string message)
        {
            if (row == null)
            {
                MessageBox.Show("Please select a record from the table first.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (Text(row, column) != required)
            {
                MessageBox.Show(message, "Receipt Not Available", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            return true;
        }

        private static string Text(DataGridViewRow row, string column) => row.Cells[column].Value?.ToString() ?? "";

        private static KeyValuePair<string, string> Line(string label, string value) => new KeyValuePair<string, string>(label, value);

        private static void Preview(string title, string number, List<KeyValuePair<string, string>> lines, decimal total)
        {
            try
            {
                using (var document = new PrintDocument { DocumentName = title + " " + number })
                using (var preview = new PrintPreviewDialog { Document = document, Width = 800, Height = 900, StartPosition = FormStartPosition.CenterScreen })
                {
                    document.PrintPage += (s, e) => Draw(e.Graphics, e.MarginBounds, title, number, lines, total);
                    preview.PrintPreviewControl.Zoom = 1.0;
                    preview.ShowDialog();
                }
            }
            catch (InvalidPrinterException ex)
            {
                MessageBox.Show("No usable printer was found. Add a printer such as 'Microsoft Print to PDF' and try again.\n\n" + ex.Message,
                    "Printer Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static void Draw(Graphics g, Rectangle area, string title, string number,
                                 List<KeyValuePair<string, string>> lines, decimal total)
        {
            using (var heading = new Font("Segoe UI", 18f, FontStyle.Bold))
            using (var bold = new Font("Segoe UI", 11f, FontStyle.Bold))
            using (var normal = new Font("Segoe UI", 11f))
            using (var amount = new Font("Nirmala UI", 13f, FontStyle.Bold))
            {
                float y = area.Top;
                g.DrawString("CarStore & Service Center", heading, Brushes.Black, area.Left, y);
                y += 40;
                g.DrawString(title, bold, Brushes.Black, area.Left, y);
                y += 24;
                g.DrawString("Receipt No: " + number + "     Printed: " + DateTime.Now.ToString("dd MMM yyyy"), normal, Brushes.Black, area.Left, y);
                y += 34;
                g.DrawLine(Pens.Black, area.Left, y, area.Right, y);
                y += 16;

                foreach (var line in lines)
                {
                    g.DrawString(line.Key, bold, Brushes.Black, area.Left, y);
                    var valueArea = new RectangleF(area.Left + 140, y, area.Width - 140, 60);
                    g.DrawString(line.Value, normal, Brushes.Black, valueArea);
                    y += Math.Max(28, g.MeasureString(line.Value, normal, (int)valueArea.Width).Height + 8);
                }

                y += 8;
                g.DrawLine(Pens.Black, area.Left, y, area.Right, y);
                y += 16;
                g.DrawString("Total Amount", bold, Brushes.Black, area.Left, y);
                g.DrawString(Money.Format(total), amount, Brushes.Black, area.Left + 140, y - 3);
                y += 90;
                g.DrawString("Authorised Signature: ______________________", normal, Brushes.Black, area.Left, y);
            }
        }
    }
}
