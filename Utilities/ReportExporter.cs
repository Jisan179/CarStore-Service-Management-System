using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CarStoreServiceManagement.Utilities
{
    /// <summary>
    /// Saves the rows shown in a grid as a CSV file that opens in Excel.
    /// </summary>
    public static class ReportExporter
    {
        public static void ExportWithDialog(DataGridView grid, string suggestedName)
        {
            if (grid.Rows.Count == 0)
            {
                MessageBox.Show("There are no records to export.", "Nothing to Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV file (*.csv)|*.csv";
                dialog.FileName = suggestedName + "_" + DateTime.Now.ToString("yyyy-MM-dd") + ".csv";
                if (dialog.ShowDialog() != DialogResult.OK)
                    return;

                try
                {
                    // UTF-8 with BOM so Excel reads non-English text correctly
                    File.WriteAllText(dialog.FileName, BuildCsv(grid), new UTF8Encoding(true));
                    MessageBox.Show("Report saved:\n" + dialog.FileName, "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    MessageBox.Show("The file could not be saved. Close it if it is open in another program.\n\n" + ex.Message,
                        "Export Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public static string BuildCsv(DataGridView grid)
        {
            List<DataGridViewColumn> columns = grid.Columns.Cast<DataGridViewColumn>()
                .Where(c => c.Visible)
                .OrderBy(c => c.DisplayIndex)
                .ToList();

            var csv = new StringBuilder();
            csv.AppendLine(string.Join(",", columns.Select(c => Escape(c.HeaderText))));

            foreach (DataGridViewRow row in grid.Rows)
            {
                csv.AppendLine(string.Join(",", columns.Select(c => Escape(ToText(row.Cells[c.Index].Value)))));
            }
            return csv.ToString();
        }

        private static string ToText(object value)
        {
            if (value == null || value == DBNull.Value) return "";
            if (value is DateTime date) return date.ToString("yyyy-MM-dd HH:mm");

            string text = Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture);

            // Stops a spreadsheet from running user-entered text as a formula
            if (value is string && text.Length > 0 && "=+-@".IndexOf(text[0]) >= 0)
                text = "'" + text;

            return text;
        }

        private static string Escape(string text)
        {
            return "\"" + (text ?? "").Replace("\"", "\"\"") + "\"";
        }
    }
}
