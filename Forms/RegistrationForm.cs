using System;
using System.Drawing;
using System.Windows.Forms;
using CarStoreServiceManagement.Services;
using CarStoreServiceManagement.Utilities;

namespace CarStoreServiceManagement.Forms
{
    public class RegistrationForm : Form
    {
        private Panel pnlHeader;
        private Label lblHeaderTitle;
        private Label lblHeaderSub;
        private Label lblFullName;
        private TextBox txtFullName;
        private Label lblUsername;
        private TextBox txtUsername;
        private Label lblEmail;
        private TextBox txtEmail;
        private Label lblPassword;
        private TextBox txtPassword;
        private Label lblConfirmPassword;
        private TextBox txtConfirmPassword;
        private CheckBox chkShowPasswords;
        private Label lblError;
        private Button btnRegister;
        private Button btnCancel;

        private readonly AuthenticationService _authService;

        public RegistrationForm()
        {
            _authService = new AuthenticationService();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = "Customer Registration - CarStore and Service Management System";
            this.Size = new Size(520, 620);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.Font = UITheme.FontRegular;

            // Header Banner
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 85,
                BackColor = UITheme.HeaderBg,
                Padding = new Padding(30, 15, 30, 15)
            };

            lblHeaderTitle = new Label
            {
                Text = "Create Customer Account",
                Font = UITheme.FontTitle,
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(25, 15)
            };

            lblHeaderSub = new Label
            {
                Text = "Register to browse inventory, request purchases, and book services",
                Font = UITheme.FontSmall,
                ForeColor = Color.FromArgb(148, 163, 184),
                AutoSize = true,
                Location = new Point(25, 48)
            };

            pnlHeader.Controls.Add(lblHeaderTitle);
            pnlHeader.Controls.Add(lblHeaderSub);

            // Form fields layout
            int left = 50;
            int width = 400;

            lblFullName = new Label { Text = "Full Name *", Font = UITheme.FontRegularBold, ForeColor = UITheme.TextDark, Location = new Point(left, 105), AutoSize = true };
            txtFullName = new TextBox { Location = new Point(left, 130), Width = width, Font = new Font("Segoe UI", 10f), BorderStyle = BorderStyle.FixedSingle };

            lblUsername = new Label { Text = "Username *", Font = UITheme.FontRegularBold, ForeColor = UITheme.TextDark, Location = new Point(left, 170), AutoSize = true };
            txtUsername = new TextBox { Location = new Point(left, 195), Width = width, Font = new Font("Segoe UI", 10f), BorderStyle = BorderStyle.FixedSingle };

            lblEmail = new Label { Text = "Email Address *", Font = UITheme.FontRegularBold, ForeColor = UITheme.TextDark, Location = new Point(left, 235), AutoSize = true };
            txtEmail = new TextBox { Location = new Point(left, 260), Width = width, Font = new Font("Segoe UI", 10f), BorderStyle = BorderStyle.FixedSingle };

            lblPassword = new Label { Text = "Password (min. 6 characters) *", Font = UITheme.FontRegularBold, ForeColor = UITheme.TextDark, Location = new Point(left, 300), AutoSize = true };
            txtPassword = new TextBox { Location = new Point(left, 325), Width = width, Font = new Font("Segoe UI", 10f), PasswordChar = '•', BorderStyle = BorderStyle.FixedSingle };

            lblConfirmPassword = new Label { Text = "Confirm Password *", Font = UITheme.FontRegularBold, ForeColor = UITheme.TextDark, Location = new Point(left, 365), AutoSize = true };
            txtConfirmPassword = new TextBox { Location = new Point(left, 390), Width = width, Font = new Font("Segoe UI", 10f), PasswordChar = '•', BorderStyle = BorderStyle.FixedSingle };

            chkShowPasswords = new CheckBox
            {
                Text = "Show Passwords",
                Font = UITheme.FontSmall,
                ForeColor = UITheme.TextMuted,
                Location = new Point(left, 425),
                AutoSize = true,
                Cursor = Cursors.Hand
            };
            chkShowPasswords.CheckedChanged += (s, e) =>
            {
                char c = chkShowPasswords.Checked ? '\0' : '•';
                txtPassword.PasswordChar = c;
                txtConfirmPassword.PasswordChar = c;
            };

            lblError = new Label
            {
                Location = new Point(left, 450),
                Width = width,
                Height = 35,
                ForeColor = UITheme.DangerRed,
                Font = UITheme.FontSmall,
                Text = ""
            };

            btnRegister = new Button
            {
                Text = "Register Account",
                Location = new Point(left, 490),
                Width = width,
                Height = 38
            };
            UITheme.StyleSuccessButton(btnRegister);
            btnRegister.Click += BtnRegister_Click;

            btnCancel = new Button
            {
                Text = "Already have an account? Back to Login",
                Location = new Point(left, 535),
                Width = width,
                Height = 32
            };
            UITheme.StyleSecondaryButton(btnCancel);
            btnCancel.Click += (s, e) => this.Close();

            this.Controls.Add(pnlHeader);
            this.Controls.Add(lblFullName);
            this.Controls.Add(txtFullName);
            this.Controls.Add(lblUsername);
            this.Controls.Add(txtUsername);
            this.Controls.Add(lblEmail);
            this.Controls.Add(txtEmail);
            this.Controls.Add(lblPassword);
            this.Controls.Add(txtPassword);
            this.Controls.Add(lblConfirmPassword);
            this.Controls.Add(txtConfirmPassword);
            this.Controls.Add(chkShowPasswords);
            this.Controls.Add(lblError);
            this.Controls.Add(btnRegister);
            this.Controls.Add(btnCancel);

            this.AcceptButton = btnRegister;
            this.ResumeLayout(false);
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            lblError.Text = "";

            string fullName = txtFullName.Text.Trim();
            string username = txtUsername.Text.Trim();
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text;
            string confirmPass = txtConfirmPassword.Text;

            // Form validations
            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPass))
            {
                lblError.Text = "All fields marked with * are required.";
                return;
            }

            if (!ValidationHelper.IsValidEmail(email))
            {
                lblError.Text = "Please enter a valid email address.";
                return;
            }

            if (!ValidationHelper.IsValidPassword(password, out string passErr))
            {
                lblError.Text = passErr;
                return;
            }

            if (password != confirmPass)
            {
                lblError.Text = "Password and confirmation password do not match.";
                return;
            }

            bool success = _authService.RegisterCustomer(fullName, username, email, password, out string errorMessage);
            if (success)
            {
                MessageBox.Show(
                    "Account registered successfully! You may now sign in using your credentials.",
                    "Registration Successful",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                this.Close();
            }
            else
            {
                lblError.Text = errorMessage;
            }
        }
    }
}
