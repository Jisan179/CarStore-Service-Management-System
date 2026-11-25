using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CarStoreServiceManagement.Services;
using CarStoreServiceManagement.Utilities;

namespace CarStoreServiceManagement.Forms
{
    public class SalesRecordsForm : Form
    {
        private Panel pnlHeader;
        private FlowLayoutPanel pnlFilter;
        private Panel pnlSummary;
        private Panel pnlGrid;

        private ComboBox cmbStatus;
        private TextBox txtSearch;
        private Button btnSearch;
        private Button btnRefresh;

        private Label lblTotalOrders;
        private Label lblConfirmedRevenue;

        private DataGridView dgvSales;

        private readonly SalesService _salesService;

        public SalesRecordsForm()
        {
            _salesService = new SalesService();
            InitializeComponent();
            LoadSales();
            AccessGuard.Require(this, user => user.CanProcessSales());
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = "Vehicle Sales History & Audit Records - CarStore";
            this.Size = new Size(1150, 700);
            this.MinimumSize = new Size(680, 480); // Allows split screen side-by-side
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = UITheme.FormBg;
            this.Font = UITheme.FontRegular;

            // Header Banner
            pnlHeader = UITheme.CreateTopBanner(
                this,
                "Vehicle Sales Records & Audit Reporting",
                "Historical ledger of all automobile sales orders, prices, customer transactions, and staff assignments.",
                (s, e) => this.Close(),
                "← Close"
            );

            // Responsive Wrapping Filter Bar
            pnlFilter = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = true,
                BackColor = Color.White,
                Padding = new Padding(15, 10, 15, 10),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblStatus = new Label { Text = "Sales Status:", AutoSize = true, Font = UITheme.FontRegularBold, Margin = new Padding(0, 6, 4, 6) };
            cmbStatus = new ComboBox { Width = 120, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 4, 10, 6) };
            cmbStatus.Items.AddRange(new object[] { "All", "Confirmed", "Pending", "Cancelled" });
            cmbStatus.SelectedIndex = 0;
            cmbStatus.SelectedIndexChanged += (s, e) => LoadSales();

            Label lblSearch = new Label { Text = "Search:", AutoSize = true, Font = UITheme.FontRegularBold, Margin = new Padding(0, 6, 4, 6) };
            txtSearch = new TextBox { Width = 160, Font = new Font("Segoe UI", 10f), BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 4, 10, 6) };

            btnSearch = new Button { Text = "Search", Width = 75, Height = 28, Margin = new Padding(0, 3, 8, 6) };
            UITheme.StylePrimaryButton(btnSearch);
            btnSearch.Click += (s, e) => LoadSales();

            btnRefresh = new Button { Text = "Refresh", Width = 75, Height = 28, Margin = new Padding(0, 3, 6, 6) };
            UITheme.StyleSecondaryButton(btnRefresh);
            btnRefresh.Click += (s, e) => { txtSearch.Clear(); cmbStatus.SelectedIndex = 0; LoadSales(); };

            pnlFilter.Controls.Add(lblStatus);
            pnlFilter.Controls.Add(cmbStatus);
            pnlFilter.Controls.Add(lblSearch);
            pnlFilter.Controls.Add(txtSearch);
            pnlFilter.Controls.Add(btnSearch);
            pnlFilter.Controls.Add(btnRefresh);
            pnlFilter.Controls.Add(UITheme.CreateToolbarButton("Export CSV", 100, (s, e) =>
                ReportExporter.ExportWithDialog(dgvSales, "sales_report")));
            pnlFilter.Controls.Add(UITheme.CreateToolbarButton("Print Receipt", 110, (s, e) =>
            {
                DataGridViewRow row = (dgvSales.SelectedRows.Count > 0 ? dgvSales.SelectedRows[0] : null);
                ReceiptPrinter.PreviewSale(row, "CarDetails", row?.Cells["CustomerName"].Value?.ToString());
            }));

            // Summary Bar (Bottom)
            pnlSummary = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 45,
                BackColor = UITheme.HeaderBg,
                Padding = new Padding(20, 10, 20, 10)
            };

            lblTotalOrders = new Label
            {
                Text = "Total Orders: 0",
                ForeColor = Color.White,
                Font = UITheme.FontRegularBold,
                Location = new Point(20, 12),
                AutoSize = true
            };

            lblConfirmedRevenue = new Label
            {
                Text = "Confirmed Revenue: ৳ 0",
                ForeColor = UITheme.SuccessGreen,
                Font = UITheme.FontRegularBold,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(850, 12),
                AutoSize = true
            };

            pnlSummary.Controls.Add(lblTotalOrders);
            pnlSummary.Controls.Add(lblConfirmedRevenue);

            pnlSummary.Resize += (s, e) =>
            {
                lblConfirmedRevenue.Location = new Point(Math.Max(160, pnlSummary.Width - 280), 12);
            };

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
            this.Controls.Add(pnlSummary);
            this.Controls.Add(pnlFilter);
            this.Controls.Add(pnlHeader);

            this.ResumeLayout(false);
        }

        private void LoadSales()
        {
            string status = cmbStatus.SelectedItem?.ToString();
            string keyword = txtSearch.Text.Trim();

            DataTable dt = _salesService.GetAllSalesRecords(status, keyword);
            dgvSales.DataSource = dt;

            decimal totalRevenue = 0m;
            int count = dt.Rows.Count;

            foreach (DataRow r in dt.Rows)
            {
                if (r["SalesStatus"]?.ToString() == "Confirmed")
                {
                    totalRevenue += Convert.ToDecimal(r["Price"]);
                }
            }

            lblTotalOrders.Text = $"Records Displayed: {count}";
            lblConfirmedRevenue.Text = $"Confirmed Revenue: {Money.Format(totalRevenue)}";

            if (dgvSales.Columns.Contains("SalesID")) dgvSales.Columns["SalesID"].HeaderText = "Sales ID";
            if (dgvSales.Columns.Contains("CarID")) dgvSales.Columns["CarID"].Visible = false;
            if (dgvSales.Columns.Contains("CustomerID")) dgvSales.Columns["CustomerID"].Visible = false;
            if (dgvSales.Columns.Contains("EmployeeID")) dgvSales.Columns["EmployeeID"].Visible = false;
            if (dgvSales.Columns.Contains("CarDetails")) dgvSales.Columns["CarDetails"].HeaderText = "Vehicle Details";
            if (dgvSales.Columns.Contains("CustomerName")) dgvSales.Columns["CustomerName"].HeaderText = "Buyer Name";
            if (dgvSales.Columns.Contains("CustomerEmail")) dgvSales.Columns["CustomerEmail"].HeaderText = "Buyer Email";
            if (dgvSales.Columns.Contains("EmployeeName")) dgvSales.Columns["EmployeeName"].HeaderText = "Sales Staff";
            if (dgvSales.Columns.Contains("SaleDate")) dgvSales.Columns["SaleDate"].HeaderText = "Order Date";
            if (dgvSales.Columns.Contains("Price"))
            {
                dgvSales.Columns["Price"].HeaderText = "Amount (৳)";
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
    }
}
