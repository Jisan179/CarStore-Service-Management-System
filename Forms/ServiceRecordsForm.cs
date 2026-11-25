using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CarStoreServiceManagement.Services;
using CarStoreServiceManagement.Utilities;

namespace CarStoreServiceManagement.Forms
{
    public class ServiceRecordsForm : Form
    {
        private Panel pnlHeader;
        private FlowLayoutPanel pnlFilter;
        private Panel pnlGrid;

        private ComboBox cmbStatusFilter;
        private TextBox txtSearch;
        private Button btnSearch;
        private Button btnRefresh;

        private ComboBox cmbNewStatus;
        private Button btnUpdateStatus;

        private DataGridView dgvServiceRecords;

        private readonly ServiceManagement _serviceManagement;

        public ServiceRecordsForm()
        {
            _serviceManagement = new ServiceManagement();
            InitializeComponent();
            LoadRecords();
            AccessGuard.Require(this, user => user.CanProcessSales());
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = "Workshop Service & Repair Orders - CarStore and Service Management System";
            this.Size = new Size(1150, 680);
            this.MinimumSize = new Size(680, 480); // Allows split screen side-by-side
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = UITheme.FormBg;
            this.Font = UITheme.FontRegular;

            // Header Banner
            pnlHeader = UITheme.CreateTopBanner(
                this,
                "Workshop Service Records & Progress Tracking",
                "Review repair jobs, diagnose technician requests, and update vehicle maintenance status (Pending, In Progress, Completed, Cancelled).",
                (s, e) => this.Close(),
                "← Close"
            );

            // Responsive Wrapping Filter & Action Toolbar
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

            Label lblStatus = new Label { Text = "Status Filter:", AutoSize = true, Font = UITheme.FontRegularBold, Margin = new Padding(0, 6, 4, 6) };
            cmbStatusFilter = new ComboBox { Width = 115, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 4, 10, 6) };
            cmbStatusFilter.Items.AddRange(new object[] { "All", "Pending", "In Progress", "Completed", "Cancelled" });
            cmbStatusFilter.SelectedIndex = 0;
            cmbStatusFilter.SelectedIndexChanged += (s, e) => LoadRecords();

            Label lblSearch = new Label { Text = "Search:", AutoSize = true, Font = UITheme.FontRegularBold, Margin = new Padding(0, 6, 4, 6) };
            txtSearch = new TextBox { Width = 140, Font = new Font("Segoe UI", 10f), BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 4, 10, 6) };

            btnSearch = new Button { Text = "Search", Width = 75, Height = 28, Margin = new Padding(0, 3, 8, 6) };
            UITheme.StylePrimaryButton(btnSearch);
            btnSearch.Click += (s, e) => LoadRecords();

            btnRefresh = new Button { Text = "Refresh", Width = 75, Height = 28, Margin = new Padding(0, 3, 16, 6) };
            UITheme.StyleSecondaryButton(btnRefresh);
            btnRefresh.Click += (s, e) => { txtSearch.Clear(); cmbStatusFilter.SelectedIndex = 0; LoadRecords(); };

            // Status changer
            Label lblChange = new Label { Text = "Set Selected To:", AutoSize = true, Font = UITheme.FontRegularBold, Margin = new Padding(0, 6, 4, 6) };
            cmbNewStatus = new ComboBox { Width = 115, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 4, 10, 6) };
            cmbNewStatus.Items.AddRange(new object[] { "Pending", "In Progress", "Completed", "Cancelled" });
            cmbNewStatus.SelectedIndex = 1;

            btnUpdateStatus = new Button
            {
                Text = "✔ Update Status",
                Size = new Size(130, 30),
                Margin = new Padding(0, 2, 6, 6)
            };
            UITheme.StyleSuccessButton(btnUpdateStatus);
            btnUpdateStatus.Click += BtnUpdateStatus_Click;

            pnlFilter.Controls.Add(lblStatus);
            pnlFilter.Controls.Add(cmbStatusFilter);
            pnlFilter.Controls.Add(lblSearch);
            pnlFilter.Controls.Add(txtSearch);
            pnlFilter.Controls.Add(btnSearch);
            pnlFilter.Controls.Add(btnRefresh);
            pnlFilter.Controls.Add(lblChange);
            pnlFilter.Controls.Add(cmbNewStatus);
            pnlFilter.Controls.Add(btnUpdateStatus);
            pnlFilter.Controls.Add(UITheme.CreateToolbarButton("Export CSV", 100, (s, e) =>
                ReportExporter.ExportWithDialog(dgvServiceRecords, "service_report")));
            pnlFilter.Controls.Add(UITheme.CreateToolbarButton("Print Receipt", 110, (s, e) =>
            {
                DataGridViewRow row = (dgvServiceRecords.SelectedRows.Count > 0 ? dgvServiceRecords.SelectedRows[0] : null);
                ReceiptPrinter.PreviewService(row, "ServiceStatus", "CarDetails", "ServiceName", "ServiceCost",
                    row?.Cells["CustomerName"].Value?.ToString());
            }));

            // Grid
            pnlGrid = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15)
            };

            dgvServiceRecords = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            UITheme.StyleDataGridView(dgvServiceRecords);
            dgvServiceRecords.CellFormatting += DgvServiceRecords_CellFormatting;

            pnlGrid.Controls.Add(dgvServiceRecords);

            this.Controls.Add(pnlGrid);
            this.Controls.Add(pnlFilter);
            this.Controls.Add(pnlHeader);

            this.ResumeLayout(false);
        }

        private void LoadRecords()
        {
            string status = cmbStatusFilter.SelectedItem?.ToString();
            string keyword = txtSearch.Text.Trim();

            DataTable dt = _serviceManagement.GetAllServiceRecords(status, keyword);
            dgvServiceRecords.DataSource = dt;

            if (dgvServiceRecords.Columns.Contains("RecordID")) dgvServiceRecords.Columns["RecordID"].HeaderText = "Job ID";
            if (dgvServiceRecords.Columns.Contains("CarID")) dgvServiceRecords.Columns["CarID"].Visible = false;
            if (dgvServiceRecords.Columns.Contains("CustomerID")) dgvServiceRecords.Columns["CustomerID"].Visible = false;
            if (dgvServiceRecords.Columns.Contains("CarDetails")) dgvServiceRecords.Columns["CarDetails"].HeaderText = "Vehicle";
            if (dgvServiceRecords.Columns.Contains("CustomerName")) dgvServiceRecords.Columns["CustomerName"].HeaderText = "Customer Name";
            if (dgvServiceRecords.Columns.Contains("CustomerEmail")) dgvServiceRecords.Columns["CustomerEmail"].HeaderText = "Customer Email";
            if (dgvServiceRecords.Columns.Contains("ServiceName")) dgvServiceRecords.Columns["ServiceName"].HeaderText = "Service Procedure";
            if (dgvServiceRecords.Columns.Contains("ServiceDate")) dgvServiceRecords.Columns["ServiceDate"].HeaderText = "Date Scheduled";
            if (dgvServiceRecords.Columns.Contains("ServiceDate")) dgvServiceRecords.Columns["ServiceDate"].DefaultCellStyle.Format = "dd MMM yyyy hh:mm tt";
            if (dgvServiceRecords.Columns.Contains("ServiceDate")) dgvServiceRecords.Columns["ServiceDate"].MinimumWidth = 155;
            if (dgvServiceRecords.Columns.Contains("ServiceCost"))
            {
                dgvServiceRecords.Columns["ServiceCost"].HeaderText = "Cost (৳)";
                Money.FormatColumn(dgvServiceRecords.Columns["ServiceCost"]);
            }
            if (dgvServiceRecords.Columns.Contains("ServiceStatus")) dgvServiceRecords.Columns["ServiceStatus"].HeaderText = "Status";
            if (dgvServiceRecords.Columns.Contains("ServiceDetails")) dgvServiceRecords.Columns["ServiceDetails"].HeaderText = "Work Notes / Symptoms";
        }

        private void DgvServiceRecords_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvServiceRecords.Columns[e.ColumnIndex].Name == "ServiceStatus" && e.Value != null)
            {
                UITheme.ColorizeStatusCell(e, e.Value.ToString());
            }
        }

        private void BtnUpdateStatus_Click(object sender, EventArgs e)
        {
            if (dgvServiceRecords.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a service record to update.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataGridViewRow row = dgvServiceRecords.SelectedRows[0];
            int recordId = Convert.ToInt32(row.Cells["RecordID"].Value);
            string newStatus = cmbNewStatus.SelectedItem?.ToString();
            string carDesc = row.Cells["CarDetails"].Value?.ToString();

            bool success = _serviceManagement.UpdateServiceStatus(recordId, newStatus, out string error);
            if (success)
            {
                MessageBox.Show(
                    $"Service job #{recordId} for {carDesc} updated to '{newStatus}' successfully.",
                    "Status Updated",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                LoadRecords();
            }
            else
            {
                MessageBox.Show(error ?? "Failed to update service status.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
