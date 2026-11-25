using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CarStoreServiceManagement.Models;
using CarStoreServiceManagement.Services;
using CarStoreServiceManagement.Utilities;

namespace CarStoreServiceManagement.Forms
{
    public class ManageUsersForm : Form
    {
        private Panel pnlHeader;
        private Panel pnlEditor;
        private Panel pnlGrid;

        private Label lblUserId;
        private TextBox txtUserId;
        private Label lblFullName;
        private TextBox txtFullName;
        private Label lblUsername;
        private TextBox txtUsername;
        private Label lblEmail;
        private TextBox txtEmail;
        private Label lblPassword;
        private TextBox txtPassword;
        private Label lblRole;
        private ComboBox cmbRole;
        private Label lblStatus;
        private ComboBox cmbStatus;

        private Button btnAdd;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnToggleStatus;
        private Button btnClear;

        private TextBox txtSearch;
        private Button btnSearch;
        private ComboBox cmbRoleFilter;
        private Button btnRefresh;

        private DataGridView dgvUsers;

        private readonly UserService _userService;

        public ManageUsersForm()
        {
            _userService = new UserService();
            InitializeComponent();
            AccessGuard.Require(this, user => user.CanManageUsers());
            LoadUserData();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = "System User Management - CarStore & Service Center";
            this.Size = new Size(1150, 720);
            this.MinimumSize = new Size(680, 480); // Allows split screen side-by-side
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = UITheme.FormBg;
            this.Font = UITheme.FontRegular;

            // Top Banner
            pnlHeader = UITheme.CreateTopBanner(
                this,
                "System User Management (Admin Only)",
                "Add employee and customer accounts, modify roles, activate/deactivate accounts, and manage permissions.",
                (s, e) => this.Close(),
                "← Close"
            );

            // Editor Panel (Left - 340px)
            pnlEditor = new Panel
            {
                Dock = DockStyle.Left,
                Width = 340,
                BackColor = Color.White,
                Padding = new Padding(15),
                AutoScroll = true
            };

            Label lblSection = new Label
            {
                Text = "User Account Details",
                Font = UITheme.FontHeader,
                ForeColor = UITheme.TextDark,
                Location = new Point(15, 15),
                AutoSize = true
            };
            pnlEditor.Controls.Add(lblSection);

            int left = 15;
            int width = 290;

            lblUserId = new Label { Text = "User ID (Auto)", Font = UITheme.FontSmall, ForeColor = UITheme.TextMuted, Location = new Point(left, 45), AutoSize = true };
            txtUserId = new TextBox { Location = new Point(left, 65), Width = width, ReadOnly = true, BackColor = Color.FromArgb(241, 245, 249), BorderStyle = BorderStyle.FixedSingle };

            lblFullName = new Label { Text = "Full Name *", Font = UITheme.FontRegularBold, ForeColor = UITheme.TextDark, Location = new Point(left, 95), AutoSize = true };
            txtFullName = new TextBox { Location = new Point(left, 115), Width = width, BorderStyle = BorderStyle.FixedSingle };

            lblUsername = new Label { Text = "Username *", Font = UITheme.FontRegularBold, ForeColor = UITheme.TextDark, Location = new Point(left, 145), AutoSize = true };
            txtUsername = new TextBox { Location = new Point(left, 165), Width = width, BorderStyle = BorderStyle.FixedSingle };

            lblEmail = new Label { Text = "Email Address *", Font = UITheme.FontRegularBold, ForeColor = UITheme.TextDark, Location = new Point(left, 195), AutoSize = true };
            txtEmail = new TextBox { Location = new Point(left, 215), Width = width, BorderStyle = BorderStyle.FixedSingle };

            lblPassword = new Label { Text = "Password (Blank to keep existing)", Font = UITheme.FontRegularBold, ForeColor = UITheme.TextDark, Location = new Point(left, 245), AutoSize = true };
            txtPassword = new TextBox { Location = new Point(left, 265), Width = width, PasswordChar = '•', BorderStyle = BorderStyle.FixedSingle };

            lblRole = new Label { Text = "System Role *", Font = UITheme.FontRegularBold, ForeColor = UITheme.TextDark, Location = new Point(left, 295), AutoSize = true };
            cmbRole = new ComboBox { Location = new Point(left, 315), Width = width, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbRole.Items.AddRange(new object[] { "Employee", "Customer", "Admin" });
            cmbRole.SelectedIndex = 0;

            lblStatus = new Label { Text = "Account Status *", Font = UITheme.FontRegularBold, ForeColor = UITheme.TextDark, Location = new Point(left, 345), AutoSize = true };
            cmbStatus = new ComboBox { Location = new Point(left, 365), Width = width, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.AddRange(new object[] { "Active", "Inactive" });
            cmbStatus.SelectedIndex = 0;

            // Buttons
            btnAdd = new Button { Text = "+ Add User", Location = new Point(left, 410), Width = 140, Height = 34 };
            UITheme.StyleSuccessButton(btnAdd);
            btnAdd.Click += BtnAdd_Click;

            btnUpdate = new Button { Text = "✎ Update User", Location = new Point(left + 150, 410), Width = 140, Height = 34 };
            UITheme.StylePrimaryButton(btnUpdate);
            btnUpdate.Click += BtnUpdate_Click;

            btnToggleStatus = new Button { Text = "Toggle Status", Location = new Point(left, 450), Width = 140, Height = 34 };
            UITheme.StyleWarningButton(btnToggleStatus);
            btnToggleStatus.Click += BtnToggleStatus_Click;

            btnDelete = new Button { Text = "✖ Delete User", Location = new Point(left + 150, 450), Width = 140, Height = 34 };
            UITheme.StyleDangerButton(btnDelete);
            btnDelete.Click += BtnDelete_Click;

            btnClear = new Button { Text = "Clear Form", Location = new Point(left, 490), Width = width, Height = 32 };
            UITheme.StyleSecondaryButton(btnClear);
            btnClear.Click += (s, e) => ClearForm();

            pnlEditor.Controls.Add(lblUserId);
            pnlEditor.Controls.Add(txtUserId);
            pnlEditor.Controls.Add(lblFullName);
            pnlEditor.Controls.Add(txtFullName);
            pnlEditor.Controls.Add(lblUsername);
            pnlEditor.Controls.Add(txtUsername);
            pnlEditor.Controls.Add(lblEmail);
            pnlEditor.Controls.Add(txtEmail);
            pnlEditor.Controls.Add(lblPassword);
            pnlEditor.Controls.Add(txtPassword);
            pnlEditor.Controls.Add(lblRole);
            pnlEditor.Controls.Add(cmbRole);
            pnlEditor.Controls.Add(lblStatus);
            pnlEditor.Controls.Add(cmbStatus);
            pnlEditor.Controls.Add(btnAdd);
            pnlEditor.Controls.Add(btnUpdate);
            pnlEditor.Controls.Add(btnToggleStatus);
            pnlEditor.Controls.Add(btnDelete);
            pnlEditor.Controls.Add(btnClear);

            // Grid Panel (Right)
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
            btnSearch = new Button { Text = "Search", Width = 75, Height = 28, Margin = new Padding(0, 3, 14, 6) };
            UITheme.StylePrimaryButton(btnSearch);
            btnSearch.Click += (s, e) => LoadUserData();

            Label lblRoleFilter = new Label { Text = "Role:", AutoSize = true, Font = UITheme.FontRegularBold, Margin = new Padding(0, 6, 6, 6) };
            cmbRoleFilter = new ComboBox { Width = 110, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 4, 10, 6) };
            cmbRoleFilter.Items.AddRange(new object[] { "All", "Admin", "Employee", "Customer" });
            cmbRoleFilter.SelectedIndex = 0;
            cmbRoleFilter.SelectedIndexChanged += (s, e) => LoadUserData();

            btnRefresh = new Button { Text = "Refresh", Width = 75, Height = 28, Margin = new Padding(0, 3, 6, 6) };
            UITheme.StyleSecondaryButton(btnRefresh);
            btnRefresh.Click += (s, e) => { txtSearch.Clear(); cmbRoleFilter.SelectedIndex = 0; LoadUserData(); };

            pnlSearch.Controls.Add(lblSearch);
            pnlSearch.Controls.Add(txtSearch);
            pnlSearch.Controls.Add(btnSearch);
            pnlSearch.Controls.Add(lblRoleFilter);
            pnlSearch.Controls.Add(cmbRoleFilter);
            pnlSearch.Controls.Add(btnRefresh);

            dgvUsers = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            UITheme.StyleDataGridView(dgvUsers);
            dgvUsers.SelectionChanged += DgvUsers_SelectionChanged;
            dgvUsers.CellFormatting += DgvUsers_CellFormatting;

            pnlGrid.Controls.Add(dgvUsers);
            pnlGrid.Controls.Add(pnlSearch);

            this.Controls.Add(pnlGrid);
            this.Controls.Add(pnlEditor);
            this.Controls.Add(pnlHeader);

            this.ResumeLayout(false);
        }

        private void LoadUserData()
        {
            string keyword = txtSearch.Text.Trim();
            string roleFilter = cmbRoleFilter.SelectedItem?.ToString();

            DataTable dt = _userService.GetAllUsers(keyword, roleFilter);
            dgvUsers.DataSource = dt;

            if (dgvUsers.Columns.Contains("UserID")) dgvUsers.Columns["UserID"].HeaderText = "ID";
            if (dgvUsers.Columns.Contains("FullName")) dgvUsers.Columns["FullName"].HeaderText = "Full Name";
            if (dgvUsers.Columns.Contains("Username")) dgvUsers.Columns["Username"].HeaderText = "Username";
            if (dgvUsers.Columns.Contains("Email")) dgvUsers.Columns["Email"].HeaderText = "Email";
            if (dgvUsers.Columns.Contains("Role")) dgvUsers.Columns["Role"].HeaderText = "Role";
            if (dgvUsers.Columns.Contains("Status")) dgvUsers.Columns["Status"].HeaderText = "Status";
            if (dgvUsers.Columns.Contains("CreatedAt")) dgvUsers.Columns["CreatedAt"].HeaderText = "Created At";
        }

        private void DgvUsers_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dgvUsers.SelectedRows[0];
                txtUserId.Text = row.Cells["UserID"].Value?.ToString();
                txtFullName.Text = row.Cells["FullName"].Value?.ToString();
                txtUsername.Text = row.Cells["Username"].Value?.ToString();
                txtEmail.Text = row.Cells["Email"].Value?.ToString();
                txtPassword.Clear();

                string role = row.Cells["Role"].Value?.ToString();
                if (!string.IsNullOrEmpty(role) && cmbRole.Items.Contains(role))
                    cmbRole.SelectedItem = role;

                string status = row.Cells["Status"].Value?.ToString();
                if (!string.IsNullOrEmpty(status) && cmbStatus.Items.Contains(status))
                    cmbStatus.SelectedItem = status;
            }
        }

        private void DgvUsers_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvUsers.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                UITheme.ColorizeStatusCell(e, e.Value.ToString());
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            string fullName = txtFullName.Text.Trim();
            string username = txtUsername.Text.Trim();
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text;
            string role = cmbRole.SelectedItem?.ToString();
            string status = cmbStatus.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Full Name, Username, Email, and Password are all required to create a new user.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidationHelper.IsValidEmail(email))
            {
                MessageBox.Show("Please enter a valid email address.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidationHelper.IsValidPassword(password, out string passErr))
            {
                MessageBox.Show(passErr, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            User user = User.CreateUserByRole(0, fullName, username, password, email, role, status, DateTime.Now);
            bool success = _userService.AddUser(user, out string errorMessage);

            if (success)
            {
                MessageBox.Show($"User account for '{fullName}' created successfully.", "Account Created", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
                LoadUserData();
            }
            else
            {
                MessageBox.Show(errorMessage, "Creation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtUserId.Text))
            {
                MessageBox.Show("Please select a user from the table to edit.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int userId = Convert.ToInt32(txtUserId.Text);
            string fullName = txtFullName.Text.Trim();
            string username = txtUsername.Text.Trim();
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text;
            string role = cmbRole.SelectedItem?.ToString();
            string status = cmbStatus.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Full Name, Username, and Email cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidationHelper.IsValidEmail(email))
            {
                MessageBox.Show("Please enter a valid email address.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            User user = User.CreateUserByRole(userId, fullName, username, password, email, role, status, DateTime.Now);
            bool success = _userService.UpdateUser(user, out string errorMessage);

            if (success)
            {
                MessageBox.Show("User account details updated successfully.", "Update Succeeded", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadUserData();
            }
            else
            {
                MessageBox.Show(errorMessage, "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnToggleStatus_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtUserId.Text))
            {
                MessageBox.Show("Please select a user to toggle status.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int userId = Convert.ToInt32(txtUserId.Text);
            if (userId == SessionManager.CurrentUserID)
            {
                MessageBox.Show("You cannot deactivate your own active session account.", "Action Prohibited", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string currentStatus = cmbStatus.SelectedItem?.ToString();
            string newStatus = (currentStatus == "Active") ? "Inactive" : "Active";

            bool success = _userService.SetUserStatus(userId, newStatus, out string errorMessage);
            if (success)
            {
                MessageBox.Show($"User status updated to '{newStatus}'.", "Status Toggled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadUserData();
            }
            else
            {
                MessageBox.Show(errorMessage, "Action Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtUserId.Text))
            {
                MessageBox.Show("Please select a user to delete.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int userId = Convert.ToInt32(txtUserId.Text);
            string userDesc = $"{txtFullName.Text} (@{txtUsername.Text})";

            var confirm = MessageBox.Show($"Are you sure you want to remove user '{userDesc}'?", "Confirm User Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
            {
                bool success = _userService.DeleteUser(userId, out string errorMessage);
                if (success)
                {
                    MessageBox.Show("User deleted successfully.", "Operation Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm();
                    LoadUserData();
                }
                else
                {
                    MessageBox.Show(errorMessage, "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadUserData();
                }
            }
        }

        private void ClearForm()
        {
            txtUserId.Clear();
            txtFullName.Clear();
            txtUsername.Clear();
            txtEmail.Clear();
            txtPassword.Clear();
            cmbRole.SelectedIndex = 0;
            cmbStatus.SelectedIndex = 0;
            if (dgvUsers.SelectedRows.Count > 0)
            {
                dgvUsers.ClearSelection();
            }
        }
    }
}
