using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CarStoreServiceManagement.Services;
using CarStoreServiceManagement.Utilities;

namespace CarStoreServiceManagement.Forms
{
    public class CustomerHistoryForm : Form
    {
        private Panel pnlHeader;
        private TabControl tabHistory;
        private TabPage tabPurchases;
        private TabPage tabServices;

        private DataGridView dgvPurchases;
        private DataGridView dgvServices;

        private Button btnRefreshPurchases;
        private Button btnRefreshServices;

        private readonly SalesService _salesService;
        private readonly ServiceManagement _serviceManagement;

        public CustomerHistoryForm(int initialTabIndex = 0)
        {
            _salesService = new SalesService();
            _serviceManagement = new ServiceManagement();
            InitializeComponent();

            if (initialTabIndex >= 0 && initialTabIndex < tabHistory.TabPages.Count)
            {
                tabHistory.SelectedIndex = initialTabIndex;
            }

            LoadPurchases();
            LoadServices();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = "My Order & Service History - CarStore";
            this.Size = new Size(1050, 650);
            this.MinimumSize = new Size(680, 480); // Allows split screen side-by-side
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = UITheme.FormBg;
            this.Font = UITheme.FontRegular;

            // Header Banner
            pnlHeader = UITheme.CreateTopBanner(
                this,
                "Personal Activity & History",
                $"Activity records belonging strictly to @{SessionManager.CurrentUsername} ({SessionManager.CurrentFullName})",
                (s, e) => this.Close(),
                "← Close"
            );

            // Tab Control
            tabHistory = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = UITheme.FontRegularBold,
                Padding = new Point(20, 8)
            };

            // Tab 1: Vehicle Purchases
            tabPurchases = new TabPage
            {
                Text = "  🚗 My Vehicle Purchases  ",
                BackColor = Color.White,
                Padding = new Padding(15)
            };

            FlowLayoutPanel pnlPurchasesTop = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = true,
                Padding = new Padding(0, 0, 0, 8),
                BackColor = Color.Transparent
            };
            Label lblPurchasesTitle = new Label { Text = "Car Purchase Orders & Requests", UseMnemonic = false, Font = UITheme.FontHeader, ForeColor = UITheme.TextDark, AutoSize = true, Margin = new Padding(0, 6, 16, 6) };
            btnRefreshPurchases = new Button { Text = "Refresh Purchases", Width = 140, Height = 28, Margin = new Padding(0, 3, 6, 6) };
            UITheme.StyleSecondaryButton(btnRefreshPurchases);
            btnRefreshPurchases.Click += (s, e) => LoadPurchases();

            pnlPurchasesTop.Controls.Add(lblPurchasesTitle);
            pnlPurchasesTop.Controls.Add(btnRefreshPurchases);
            pnlPurchasesTop.Controls.Add(UITheme.CreateToolbarButton("Print Receipt", 110, (s, e) =>
                ReceiptPrinter.PreviewSale((dgvPurchases.SelectedRows.Count > 0 ? dgvPurchases.SelectedRows[0] : null), "Vehicle", SessionManager.CurrentFullName)));

            dgvPurchases = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            UITheme.StyleDataGridView(dgvPurchases);
            dgvPurchases.CellFormatting += (s, e) =>
            {
                if (dgvPurchases.Columns[e.ColumnIndex].Name == "SalesStatus" && e.Value != null)
                {
                    UITheme.ColorizeStatusCell(e, e.Value.ToString());
                }
            };

            tabPurchases.Controls.Add(dgvPurchases);
            tabPurchases.Controls.Add(pnlPurchasesTop);

            // Tab 2: Service History
            tabServices = new TabPage
            {
                Text = "  🔧 My Service Appointments  ",
                BackColor = Color.White,
                Padding = new Padding(15)
            };

            FlowLayoutPanel pnlServicesTop = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = true,
                Padding = new Padding(0, 0, 0, 8),
                BackColor = Color.Transparent
            };
            Label lblServicesTitle = new Label { Text = "Workshop Maintenance & Repair History", UseMnemonic = false, Font = UITheme.FontHeader, ForeColor = UITheme.TextDark, AutoSize = true, Margin = new Padding(0, 6, 16, 6) };
            btnRefreshServices = new Button { Text = "Refresh Services", Width = 140, Height = 28, Margin = new Padding(0, 3, 6, 6) };
            UITheme.StyleSecondaryButton(btnRefreshServices);
            btnRefreshServices.Click += (s, e) => LoadServices();

            pnlServicesTop.Controls.Add(lblServicesTitle);
            pnlServicesTop.Controls.Add(btnRefreshServices);
            pnlServicesTop.Controls.Add(UITheme.CreateToolbarButton("Print Receipt", 110, (s, e) =>
                ReceiptPrinter.PreviewService((dgvServices.SelectedRows.Count > 0 ? dgvServices.SelectedRows[0] : null), "Status", "Vehicle", "Service", "Cost", SessionManager.CurrentFullName)));

            dgvServices = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            UITheme.StyleDataGridView(dgvServices);
            dgvServices.CellFormatting += (s, e) =>
            {
                if (dgvServices.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
                {
                    UITheme.ColorizeStatusCell(e, e.Value.ToString());
                }
            };

            tabServices.Controls.Add(dgvServices);
            tabServices.Controls.Add(pnlServicesTop);

            tabHistory.TabPages.Add(tabPurchases);
            tabHistory.TabPages.Add(tabServices);

            this.Controls.Add(tabHistory);
            this.Controls.Add(pnlHeader);

            this.ResumeLayout(false);
        }

        private void LoadPurchases()
        {
            int customerId = SessionManager.CurrentUserID;
            DataTable dt = _salesService.GetCustomerPurchases(customerId);
            dgvPurchases.DataSource = dt;

            if (dgvPurchases.Columns.Contains("SalesID")) dgvPurchases.Columns["SalesID"].HeaderText = "Order ID";
            if (dgvPurchases.Columns.Contains("CarID")) dgvPurchases.Columns["CarID"].Visible = false;
            if (dgvPurchases.Columns.Contains("Vehicle")) dgvPurchases.Columns["Vehicle"].HeaderText = "Vehicle";
            if (dgvPurchases.Columns.Contains("Price"))
            {
                dgvPurchases.Columns["Price"].HeaderText = "Price (৳)";
                Money.FormatColumn(dgvPurchases.Columns["Price"]);
            }
            if (dgvPurchases.Columns.Contains("SaleDate")) dgvPurchases.Columns["SaleDate"].HeaderText = "Date";
            if (dgvPurchases.Columns.Contains("SalesStatus")) dgvPurchases.Columns["SalesStatus"].HeaderText = "Status";
            if (dgvPurchases.Columns.Contains("ProcessedBy")) dgvPurchases.Columns["ProcessedBy"].HeaderText = "Handled By";
        }

        private void LoadServices()
        {
            int customerId = SessionManager.CurrentUserID;
            DataTable dt = _serviceManagement.GetCustomerServiceRecords(customerId);
            dgvServices.DataSource = dt;

            if (dgvServices.Columns.Contains("RecordID")) dgvServices.Columns["RecordID"].HeaderText = "Record ID";
            if (dgvServices.Columns.Contains("Vehicle")) dgvServices.Columns["Vehicle"].HeaderText = "Vehicle";
            if (dgvServices.Columns.Contains("Service")) dgvServices.Columns["Service"].HeaderText = "Service Package";
            if (dgvServices.Columns.Contains("ServiceDate")) dgvServices.Columns["ServiceDate"].HeaderText = "Appointment Date";
            if (dgvServices.Columns.Contains("ServiceDate")) dgvServices.Columns["ServiceDate"].DefaultCellStyle.Format = "dd MMM yyyy hh:mm tt";
            if (dgvServices.Columns.Contains("ServiceDate")) dgvServices.Columns["ServiceDate"].MinimumWidth = 155;
            if (dgvServices.Columns.Contains("Cost"))
            {
                dgvServices.Columns["Cost"].HeaderText = "Cost (৳)";
                Money.FormatColumn(dgvServices.Columns["Cost"]);
            }
            if (dgvServices.Columns.Contains("Status")) dgvServices.Columns["Status"].HeaderText = "Status";
            if (dgvServices.Columns.Contains("Notes")) dgvServices.Columns["Notes"].HeaderText = "Diagnostic Notes";
        }
    }
}
