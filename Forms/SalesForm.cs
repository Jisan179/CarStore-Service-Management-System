using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CarStoreServiceManagement.Services;
using CarStoreServiceManagement.Utilities;

namespace CarStoreServiceManagement.Forms
{
    public class SalesForm : Form
    {
        private Panel pnlHeader;
        private FlowLayoutPanel pnlActions;
        private Panel pnlGrid;

        private ComboBox cmbStatusFilter;
        private TextBox txtSearch;
        private Button btnSearch;
        private Button btnRefresh;

        private Button btnConfirmSale;
        private Button btnCancelSale;

        private DataGridView dgvSales;

        private readonly SalesService _salesService;

        public SalesForm()
        {
            _salesService = new SalesService();
            InitializeComponent();
            LoadSalesData();
            AccessGuard.Require(this, user => user.CanProcessSales());
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = "Sales Order Processing - CarStore and Service Management System";
            this.Size = new Size(1100, 680);
            this.MinimumSize = new Size(680, 480); // Allows split screen side-by-side
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = UITheme.FormBg;
            this.Font = UITheme.FontRegular;

            // Header Banner
            pnlHeader = UITheme.CreateTopBanner(
                this,
                "Vehicle Sales & Purchase Requests Processing",
                "Review customer purchase orders. Confirm sales to mark inventory as Sold, or cancel requests to restore car availability.",
                (s, e) => this.Close(),
                "← Close"
            );

            // Responsive Wrapping Filter & Action Toolbar
            pnlActions = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = true,
                BackColor = Color.White,
                Padding = new Padding(15, 10, 15, 10),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblStatus = new Label { Text = "Status Filter:", AutoSize = true, Font = UITheme.FontRegularBold, Margin = new Padding(0, 6, 4, 6) };
            cmbStatusFilter = new ComboBox { Width = 115, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 4, 10, 6) };
            cmbStatusFilter.Items.AddRange(new object[] { "All", "Pending", "Confirmed", "Cancelled" });
            cmbStatusFilter.SelectedIndex = 1;
            cmbStatusFilter.SelectedIndexChanged += (s, e) => LoadSalesData();

            Label lblSearch = new Label { Text = "Search:", AutoSize = true, Font = UITheme.FontRegularBold, Margin = new Padding(0, 6, 4, 6) };
            txtSearch = new TextBox { Width = 140, Font = new Font("Segoe UI", 10f), BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 4, 10, 6) };

            btnSearch = new Button { Text = "Search", Width = 75, Height = 28, Margin = new Padding(0, 3, 8, 6) };
            UITheme.StylePrimaryButton(btnSearch);
            btnSearch.Click += (s, e) => LoadSalesData();

            btnRefresh = new Button { Text = "Refresh", Width = 75, Height = 28, Margin = new Padding(0, 3, 16, 6) };
            UITheme.StyleSecondaryButton(btnRefresh);
            btnRefresh.Click += (s, e) =>
            {
                txtSearch.Clear();
                cmbStatusFilter.SelectedIndex = 0;
                LoadSalesData();
            };

            btnConfirmSale = new Button
            {
                Text = "✔ Confirm Sale (Mark Sold)",
                Size = new Size(235, 30),
                Margin = new Padding(0, 2, 8, 6)
            };
            UITheme.StyleSuccessButton(btnConfirmSale);
            btnConfirmSale.Click += BtnConfirmSale_Click;

            btnCancelSale = new Button
            {
                Text = "✖ Cancel Order",
                Size = new Size(130, 30),
                Margin = new Padding(0, 2, 6, 6)
            };
            UITheme.StyleDangerButton(btnCancelSale);
            btnCancelSale.Click += BtnCancelSale_Click;

            pnlActions.Controls.Add(lblStatus);
            pnlActions.Controls.Add(cmbStatusFilter);
            pnlActions.Controls.Add(lblSearch);
            pnlActions.Controls.Add(txtSearch);
            pnlActions.Controls.Add(btnSearch);
            pnlActions.Controls.Add(btnRefresh);
            pnlActions.Controls.Add(btnConfirmSale);
            pnlActions.Controls.Add(btnCancelSale);
            pnlActions.Controls.Add(UITheme.CreateToolbarButton("Print Receipt", 110, (s, e) =>
            {
                DataGridViewRow row = (dgvSales.SelectedRows.Count > 0 ? dgvSales.SelectedRows[0] : null);
                ReceiptPrinter.PreviewSale(row, "CarDetails", row?.Cells["CustomerName"].Value?.ToString());
            }));

            // Grid Container
            pnlGrid = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15)
            };

            dgvSales = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            UITheme.StyleDataGridView(dgvSales);
            dgvSales.CellFormatting += DgvSales_CellFormatting;

            pnlGrid.Controls.Add(dgvSales);

            this.Controls.Add(pnlGrid);
            this.Controls.Add(pnlActions);
            this.Controls.Add(pnlHeader);

            this.ResumeLayout(false);
        }

        private void LoadSalesData()
        {
            string status = cmbStatusFilter.SelectedItem?.ToString();
            string keyword = txtSearch.Text.Trim();

            DataTable dt = _salesService.GetAllSalesRecords(status, keyword);
            dgvSales.DataSource = dt;

            if (dgvSales.Columns.Contains("SalesID")) dgvSales.Columns["SalesID"].HeaderText = "Sales ID";
            if (dgvSales.Columns.Contains("CarID")) dgvSales.Columns["CarID"].HeaderText = "Car ID";
            if (dgvSales.Columns.Contains("CustomerID")) dgvSales.Columns["CustomerID"].Visible = false;
            if (dgvSales.Columns.Contains("EmployeeID")) dgvSales.Columns["EmployeeID"].Visible = false;
            if (dgvSales.Columns.Contains("CarDetails")) dgvSales.Columns["CarDetails"].HeaderText = "Vehicle";
            if (dgvSales.Columns.Contains("CustomerName")) dgvSales.Columns["CustomerName"].HeaderText = "Customer Name";
            if (dgvSales.Columns.Contains("CustomerEmail")) dgvSales.Columns["CustomerEmail"].HeaderText = "Customer Email";
            if (dgvSales.Columns.Contains("EmployeeName")) dgvSales.Columns["EmployeeName"].HeaderText = "Handled By";
            if (dgvSales.Columns.Contains("SaleDate")) dgvSales.Columns["SaleDate"].HeaderText = "Request Date";
            if (dgvSales.Columns.Contains("Price"))
            {
                dgvSales.Columns["Price"].HeaderText = "Price (৳)";
                Money.FormatColumn(dgvSales.Columns["Price"]);
            }
            if (dgvSales.Columns.Contains("SalesStatus")) dgvSales.Columns["SalesStatus"].HeaderText = "Status";
        }

        private void DgvSales_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvSales.Columns[e.ColumnIndex].Name == "SalesStatus" && e.Value != null)
            {
                UITheme.ColorizeStatusCell(e, e.Value.ToString());
            }
        }

        private void BtnConfirmSale_Click(object sender, EventArgs e)
        {
            if (dgvSales.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a sales request from the table to confirm.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataGridViewRow row = dgvSales.SelectedRows[0];
            string status = row.Cells["SalesStatus"].Value?.ToString();

            if (status != "Pending")
            {
                MessageBox.Show($"This sales order is already '{status}' and cannot be confirmed.", "Action Prohibited", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int salesId = Convert.ToInt32(row.Cells["SalesID"].Value);
            int carId = Convert.ToInt32(row.Cells["CarID"].Value);
            string carDesc = row.Cells["CarDetails"].Value?.ToString();
            string customer = row.Cells["CustomerName"].Value?.ToString();

            var confirm = MessageBox.Show(
                $"Confirm sale of {carDesc} to customer {customer}?\n\nThis will transactionally finalize the sale and mark the vehicle as 'Sold'.",
                "Confirm Vehicle Sale",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                int employeeId = SessionManager.CurrentUserID;
                bool success = _salesService.ConfirmSale(salesId, carId, employeeId, out string error);

                if (success)
                {
                    MessageBox.Show(
                        "Sale confirmed successfully! The vehicle status has been updated to Sold in the inventory.",
                        "Sale Finalized",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    LoadSalesData();
                }
                else
                {
                    MessageBox.Show(error ?? "Failed to confirm sale.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnCancelSale_Click(object sender, EventArgs e)
        {
            if (dgvSales.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a sales request to cancel.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataGridViewRow row = dgvSales.SelectedRows[0];
            string status = row.Cells["SalesStatus"].Value?.ToString();

            if (status != "Pending")
            {
                MessageBox.Show($"This sales order is already '{status}'. Only pending orders can be cancelled.", "Action Prohibited", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int salesId = Convert.ToInt32(row.Cells["SalesID"].Value);
            int carId = Convert.ToInt32(row.Cells["CarID"].Value);
            string carDesc = row.Cells["CarDetails"].Value?.ToString();

            var confirm = MessageBox.Show(
                $"Are you sure you want to cancel the sale order for {carDesc}?\n\nThe vehicle will be restored to 'Available' in the showroom inventory.",
                "Confirm Order Cancellation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                int employeeId = SessionManager.CurrentUserID;
                bool success = _salesService.CancelSale(salesId, carId, employeeId, out string error);

                if (success)
                {
                    MessageBox.Show(
                        "Sales request cancelled successfully. Vehicle returned to Available inventory.",
                        "Order Cancelled",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    LoadSalesData();
                }
                else
                {
                    MessageBox.Show(error ?? "Failed to cancel sale.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
