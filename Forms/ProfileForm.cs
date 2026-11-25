using System;
using System.Drawing;
using System.Windows.Forms;
using CarStoreServiceManagement.Services;
using CarStoreServiceManagement.Utilities;

namespace CarStoreServiceManagement.Forms
{
    public class ProfileForm : Form
    {
        private Panel pnlHeader;
        private Label lblUsername;
        private TextBox txtUsername;
        private Label lblRole;
        private TextBox txtRole;
        private Label lblFullName;
        private TextBox txtFullName;
        private Label lblEmail;
        private TextBox txtEmail;
        private Button btnSaveProfile;

        // Password change section
        private GroupBox grpPassword;
        private Label lblCurrentPass;
        private TextBox txtCurrentPass;
        private Label lblNewPass;
        private TextBox txtNewPass;
        private Label lblConfirmPass;
        private TextBox txtConfirmPass;
        private Button btnChangePassword;

        private Button btnClose;

        private readonly UserService _userService;

        public ProfileForm()
        {
            _userService = new UserService();
            InitializeComponent();
            LoadCurrentProfile();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = "My Profile & Account Settings - CarStore";
            this.Size = new Size(580, 640);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.Font = UITheme.FontRegular;

            // Header Banner
            pnlHeader = UITheme.CreateTopBanner(
                this,
                "My Profile & Security Settings",
                $"Manage personal account information for @{SessionManager.CurrentUsername}",
                (s, e) => this.Close(),
                "✕ Close"
            );

            int left = 50;
            int width = 460;

            lblUsername = new Label { Text = "Username (Cannot be changed)", Font = UITheme.FontRegularBold, ForeColor = UITheme.TextMuted, Location = new Point(left, 85), AutoSize = true };
            txtUsername = new TextBox { Location = new Point(left, 108), Width = 220, ReadOnly = true, BackColor = Color.FromArgb(241, 245, 249), BorderStyle = BorderStyle.FixedSingle };

            lblRole = new Label { Text = "Account Role", Font = UITheme.FontRegularBold, ForeColor = UITheme.TextMuted, Location = new Point(left + 240, 85), AutoSize = true };
            txtRole = new TextBox { Location = new Point(left + 240, 108), Width = 220, ReadOnly = true, BackColor = Color.FromArgb(241, 245, 249), BorderStyle = BorderStyle.FixedSingle };

            lblFullName = new Label { Text = "Full Name *", Font = UITheme.FontRegularBold, ForeColor = UITheme.TextDark, Location = new Point(left, 145), AutoSize = true };
            txtFullName = new TextBox { Location = new Point(left, 168), Width = width, Font = new Font("Segoe UI", 10f), BorderStyle = BorderStyle.FixedSingle };

            lblEmail = new Label { Text = "Email Address *", Font = UITheme.FontRegularBold, ForeColor = UITheme.TextDark, Location = new Point(left, 205), AutoSize = true };
            txtEmail = new TextBox { Location = new Point(left, 228), Width = width, Font = new Font("Segoe UI", 10f), BorderStyle = BorderStyle.FixedSingle };

            btnSaveProfile = new Button
            {
                Text = "💾 Save Profile Changes",
                Location = new Point(left, 268),
                Width = 220,
                Height = 36
            };
            UITheme.StylePrimaryButton(btnSaveProfile);
            btnSaveProfile.Click += BtnSaveProfile_Click;

            // Password Group Box
            grpPassword = new GroupBox
            {
                Text = "Change Password",
                Font = UITheme.FontRegularBold,
                ForeColor = UITheme.TextDark,
                Location = new Point(left, 320),
                Size = new Size(width, 215),
                Padding = new Padding(15)
            };

            lblCurrentPass = new Label { Text = "Current Password *", Font = UITheme.FontRegular, Location = new Point(15, 25), AutoSize = true };
            txtCurrentPass = new TextBox { Location = new Point(15, 48), Width = 430, PasswordChar = '•', BorderStyle = BorderStyle.FixedSingle };

            lblNewPass = new Label { Text = "New Password (min 6 chars) *", Font = UITheme.FontRegular, Location = new Point(15, 80), AutoSize = true };
            txtNewPass = new TextBox { Location = new Point(15, 103), Width = 430, PasswordChar = '•', BorderStyle = BorderStyle.FixedSingle };

            lblConfirmPass = new Label { Text = "Confirm New Password *", Font = UITheme.FontRegular, Location = new Point(15, 135), AutoSize = true };
            txtConfirmPass = new TextBox { Location = new Point(15, 158), Width = 230, PasswordChar = '•', BorderStyle = BorderStyle.FixedSingle };

            btnChangePassword = new Button
            {
                Text = "Update Password",
                Location = new Point(255, 156),
                Width = 190,
                Height = 30
            };
            UITheme.StyleWarningButton(btnChangePassword);
            btnChangePassword.Click += BtnChangePassword_Click;

            grpPassword.Controls.Add(lblCurrentPass);
            grpPassword.Controls.Add(txtCurrentPass);
            grpPassword.Controls.Add(lblNewPass);
            grpPassword.Controls.Add(txtNewPass);
            grpPassword.Controls.Add(lblConfirmPass);
            grpPassword.Controls.Add(txtConfirmPass);
            grpPassword.Controls.Add(btnChangePassword);

            btnClose = new Button
            {
                Text = "Close",
                Location = new Point(left + 330, 548),
                Width = 130,
                Height = 34
            };
            UITheme.StyleSecondaryButton(btnClose);
            btnClose.Click += (s, e) => this.Close();

            this.Controls.Add(pnlHeader);
            this.Controls.Add(lblUsername);
            this.Controls.Add(txtUsername);
            this.Controls.Add(lblRole);
            this.Controls.Add(txtRole);
            this.Controls.Add(lblFullName);
            this.Controls.Add(txtFullName);
            this.Controls.Add(lblEmail);
            this.Controls.Add(txtEmail);
            this.Controls.Add(btnSaveProfile);
            this.Controls.Add(grpPassword);
            this.Controls.Add(btnClose);

            this.ResumeLayout(false);
        }

        private void LoadCurrentProfile()
        {
            txtUsername.Text = SessionManager.CurrentUsername;
            txtRole.Text = SessionManager.CurrentRole;
            txtFullName.Text = SessionManager.CurrentFullName;
            txtEmail.Text = SessionManager.CurrentEmail;
        }

        private void BtnSaveProfile_Click(object sender, EventArgs e)
        {
            string fullName = txtFullName.Text.Trim();
            string email = txtEmail.Text.Trim();

            if (string.IsNullOrEmpty(fullName))
            {
                MessageBox.Show("Full Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidationHelper.IsValidEmail(email))
            {
                MessageBox.Show("Please enter a valid email address.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool success = _userService.UpdateProfile(SessionManager.CurrentUserID, fullName, email, out string error);
            if (success)
            {
                MessageBox.Show("Profile updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(error ?? "Failed to update profile.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnChangePassword_Click(object sender, EventArgs e)
        {
            string currentPass = txtCurrentPass.Text;
            string newPass = txtNewPass.Text;
            string confirmPass = txtConfirmPass.Text;

            if (string.IsNullOrEmpty(currentPass) || string.IsNullOrEmpty(newPass) || string.IsNullOrEmpty(confirmPass))
            {
                MessageBox.Show("Please fill out all password fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (newPass != confirmPass)
            {
                MessageBox.Show("New password and confirm password do not match.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidationHelper.IsValidPassword(newPass, out string passErr))
            {
                MessageBox.Show(passErr, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool success = _userService.ChangePassword(SessionManager.CurrentUserID, currentPass, newPass, out string error);
            if (success)
            {
                MessageBox.Show("Password changed successfully.", "Security Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtCurrentPass.Clear();
                txtNewPass.Clear();
                txtConfirmPass.Clear();
            }
            else
            {
                MessageBox.Show(error ?? "Password change failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
