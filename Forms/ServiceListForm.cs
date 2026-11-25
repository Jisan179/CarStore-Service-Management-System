using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CarStoreServiceManagement.Models;
using CarStoreServiceManagement.Services;
using CarStoreServiceManagement.Utilities;

namespace CarStoreServiceManagement.Forms
{
    public class ServiceListForm : Form
    {
        private Panel pnlHeader;
        private Panel pnlEditor;
        private Panel pnlGrid;

        private Label lblServiceId;
        private TextBox txtServiceId;
        private Label lblServiceName;
        private TextBox txtServiceName;
        private Label lblPrice;
        private TextBox txtPrice;

        private Button btnAdd;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnClear;

        private TextBox txtSearch;
        private Button btnSearch;
        private Button btnRefresh;

        private DataGridView dgvServices;

        private readonly ServiceManagement _serviceManagement;

        public ServiceListForm()
        {
            _serviceManagement = new ServiceManagement();
            InitializeComponent();
            LoadServicesData();
            AccessGuard.Require(this, user => user.CanManageCatalog());
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = "Service Catalog Management - CarStore & Service Center";
            this.Size = new Size(950, 620);
            this.MinimumSize = new Size(680, 480); // Allows split screen side-by-side
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = UITheme.FormBg;
            this.Font = UITheme.FontRegular;

            // Header Banner
            pnlHeader = UITheme.CreateTopBanner(
                this,
                "Service Catalog Management (ServiceList)",
                "Configure standard workshop maintenance procedures, diagnostics packages, and service rates.",
                (s, e) => this.Close(),
                "← Close"
            );

            // Editor Panel (Left side - 320px)
            pnlEditor = new Panel
            {
                Dock = DockStyle.Left,
                Width = 320,
                BackColor = Color.White,
                Padding = new Padding(15),
                AutoScroll = true
            };

            Label lblSection = new Label
            {
                Text = "Service Package Details",
                Font = UITheme.FontHeader,
                ForeColor = UITheme.TextDark,
                Location = new Point(15, 15),
                AutoSize = true
            };
            pnlEditor.Controls.Add(lblSection);

            int left = 15;
            int width = 275;

            lblServiceId = new Label { Text = "Service ID (Auto)", Font = UITheme.FontSmall, ForeColor = UITheme.TextMuted, Location = new Point(left, 45), AutoSize = true };
            txtServiceId = new TextBox { Location = new Point(left, 65), Width = width, ReadOnly = true, BackColor = Color.FromArgb(241, 245, 249), BorderStyle = BorderStyle.FixedSingle };

            lblServiceName = new Label { Text = "Service Name / Description *", Font = UITheme.FontRegularBold, ForeColor = UITheme.TextDark, Location = new Point(left, 105), AutoSize = true };
            txtServiceName = new TextBox { Location = new Point(left, 128), Width = width, Height = 60, Multiline = true, BorderStyle = BorderStyle.FixedSingle };

            lblPrice = new Label { Text = "Standard Labor / Package Price (৳) *", Font = UITheme.FontRegularBold, ForeColor = UITheme.TextDark, Location = new Point(left, 200), AutoSize = true };
            txtPrice = new TextBox { Location = new Point(left, 223), Width = width, BorderStyle = BorderStyle.FixedSingle };

            // Buttons
            btnAdd = new Button { Text = "+ Add Service", Location = new Point(left, 275), Width = 130, Height = 34 };
            UITheme.StyleSuccessButton(btnAdd);
            btnAdd.Click += BtnAdd_Click;

            btnUpdate = new Button { Text = "✎ Update Service", Location = new Point(left + 145, 275), Width = 130, Height = 34 };
            UITheme.StylePrimaryButton(btnUpdate);
            btnUpdate.Click += BtnUpdate_Click;

            btnDelete = new Button { Text = "✖ Delete Service", Location = new Point(left, 320), Width = 130, Height = 34 };
            UITheme.StyleDangerButton(btnDelete);
            btnDelete.Click += BtnDelete_Click;

            btnClear = new Button { Text = "Clear Form", Location = new Point(left + 145, 320), Width = 130, Height = 34 };
            UITheme.StyleSecondaryButton(btnClear);
            btnClear.Click += (s, e) => ClearForm();

            pnlEditor.Controls.Add(lblServiceId);
            pnlEditor.Controls.Add(txtServiceId);
            pnlEditor.Controls.Add(lblServiceName);
            pnlEditor.Controls.Add(txtServiceName);
            pnlEditor.Controls.Add(lblPrice);
            pnlEditor.Controls.Add(txtPrice);
            pnlEditor.Controls.Add(btnAdd);
            pnlEditor.Controls.Add(btnUpdate);
            pnlEditor.Controls.Add(btnDelete);
            pnlEditor.Controls.Add(btnClear);

            // Grid Panel (Right side)
            pnlGrid = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15)
            };

            FlowLayoutPanel pnlSearch = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = true,
                Padding = new Padding(0, 0, 0, 8),
                BackColor = Color.Transparent
            };

            Label lblSearch = new Label { Text = "Search:", AutoSize = true, Font = UITheme.FontRegularBold, Margin = new Padding(0, 6, 6, 6) };
            txtSearch = new TextBox { Width = 160, Font = new Font("Segoe UI", 10f), BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 4, 10, 6) };
            btnSearch = new Button { Text = "Search", Width = 75, Height = 28, Margin = new Padding(0, 3, 10, 6) };
            UITheme.StylePrimaryButton(btnSearch);
            btnSearch.Click += (s, e) => LoadServicesData();

            btnRefresh = new Button { Text = "Refresh", Width = 75, Height = 28, Margin = new Padding(0, 3, 6, 6) };
            UITheme.StyleSecondaryButton(btnRefresh);
            btnRefresh.Click += (s, e) => { txtSearch.Clear(); LoadServicesData(); };

            pnlSearch.Controls.Add(lblSearch);
            pnlSearch.Controls.Add(txtSearch);
            pnlSearch.Controls.Add(btnSearch);
            pnlSearch.Controls.Add(btnRefresh);

            dgvServices = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            UITheme.StyleDataGridView(dgvServices);
            dgvServices.SelectionChanged += DgvServices_SelectionChanged;

            pnlGrid.Controls.Add(dgvServices);
            pnlGrid.Controls.Add(pnlSearch);

            this.Controls.Add(pnlGrid);
            this.Controls.Add(pnlEditor);
            this.Controls.Add(pnlHeader);

            this.ResumeLayout(false);
        }

        private void LoadServicesData()
        {
            string keyword = txtSearch.Text.Trim();
            DataTable dt = _serviceManagement.GetAllServices(keyword);
            dgvServices.DataSource = dt;

            if (dgvServices.Columns.Contains("ServiceID")) dgvServices.Columns["ServiceID"].HeaderText = "Service ID";
            if (dgvServices.Columns.Contains("ServiceName")) dgvServices.Columns["ServiceName"].HeaderText = "Service Procedure Name";
            if (dgvServices.Columns.Contains("Price"))
            {
                dgvServices.Columns["Price"].HeaderText = "Standard Price (৳)";
                Money.FormatColumn(dgvServices.Columns["Price"]);
            }
        }

        private void DgvServices_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvServices.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dgvServices.SelectedRows[0];
                txtServiceId.Text = row.Cells["ServiceID"].Value?.ToString();
                txtServiceName.Text = row.Cells["ServiceName"].Value?.ToString();
                txtPrice.Text = row.Cells["Price"].Value?.ToString();
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            string name = txtServiceName.Text.Trim();
            string priceText = txtPrice.Text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Please specify the service name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtServiceName.Focus();
                return;
            }

            if (!ValidationHelper.IsValidPrice(priceText, out decimal price))
            {
                MessageBox.Show("Please enter a valid price.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPrice.Focus();
                return;
            }

            Service s = new Service(0, name, price);
            bool success = _serviceManagement.AddService(s, out string error);

            if (success)
            {
                MessageBox.Show("Service added successfully to catalog.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
                LoadServicesData();
            }
            else
            {
                MessageBox.Show(error ?? "Failed to add service.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtServiceId.Text))
            {
                MessageBox.Show("Please select a service from the table to update.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int id = Convert.ToInt32(txtServiceId.Text);
            string name = txtServiceName.Text.Trim();
            string priceText = txtPrice.Text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Service name cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidationHelper.IsValidPrice(priceText, out decimal price))
            {
                MessageBox.Show("Please enter a valid price.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Service s = new Service(id, name, price);
            bool success = _serviceManagement.UpdateService(s, out string error);

            if (success)
            {
                MessageBox.Show("Service details updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadServicesData();
            }
            else
            {
                MessageBox.Show(error ?? "Failed to update service.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtServiceId.Text))
            {
                MessageBox.Show("Please select a service to delete.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int id = Convert.ToInt32(txtServiceId.Text);
            string name = txtServiceName.Text;

            var confirm = MessageBox.Show($"Are you sure you want to remove '{name}' from the service catalog?", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
            {
                bool success = _serviceManagement.DeleteService(id, out string error);
                if (success)
                {
                    MessageBox.Show("Service deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm();
                    LoadServicesData();
                }
                else
                {
                    MessageBox.Show(error ?? "Cannot delete service.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void ClearForm()
        {
            txtServiceId.Clear();
            txtServiceName.Clear();
            txtPrice.Clear();
            if (dgvServices.SelectedRows.Count > 0)
            {
                dgvServices.ClearSelection();
            }
        }
    }
}
