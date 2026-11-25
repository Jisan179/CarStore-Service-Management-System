using System;
using System.Drawing;
using System.Windows.Forms;
using CarStoreServiceManagement.Data;
using CarStoreServiceManagement.Models;
using CarStoreServiceManagement.Services;
using CarStoreServiceManagement.Utilities;

namespace CarStoreServiceManagement.Forms
{
    public class LoginForm : Form
    {
        private Panel pnlBrand;
        private Panel pnlLogin;
        private Label lblBrandTitle;
        private Label lblBrandSub;
        private Label lblAcademicInfo;
        private Label lblLoginHeader;
        private Label lblLoginSub;
        private Label lblUsername;
        private TextBox txtUsername;
        private Label lblPassword;
        private TextBox txtPassword;
        private CheckBox chkShowPassword;
        private Button btnLogin;
        private Button btnRegister;
        private Button btnExit;
        private Label lblError;

        private readonly AuthenticationService _authService;

        public LoginForm()
        {
            _authService = new AuthenticationService();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = "Login - CarStore and Service Management System";
            this.Size = new Size(820, 520);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = UITheme.FormBg;
            this.Font = UITheme.FontRegular;

            // Brand Panel (Left Side - 340px)
            pnlBrand = new Panel
            {
                Dock = DockStyle.Left,
                Width = 340,
                BackColor = UITheme.HeaderBg,
                Padding = new Padding(30)
            };

            lblBrandTitle = new Label
            {
                Text = "CarStore & Service",
                UseMnemonic = false,
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(30, 80)
            };

            lblBrandSub = new Label
            {
                Text = "Automobile Dealership &\nService Management System",
                UseMnemonic = false,
                Font = new Font("Segoe UI", 11f, FontStyle.Regular),
                ForeColor = Color.FromArgb(148, 163, 184),
                AutoSize = true,
                Location = new Point(30, 125)
            };

            lblAcademicInfo = new Label
            {
                Text = "American International University-Bangladesh\n" +
                       "Course: Object Oriented Programming 2\n" +
                       "Semester: Summer 2025-2026",
                Font = UITheme.FontSmall,
                ForeColor = Color.FromArgb(100, 116, 139),
                AutoSize = true,
                Location = new Point(30, 360)
            };

            pnlBrand.Controls.Add(lblBrandTitle);
            pnlBrand.Controls.Add(lblBrandSub);
            pnlBrand.Controls.Add(lblAcademicInfo);

            // Login Panel (Right Side)
            pnlLogin = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(45, 40, 45, 40)
            };

            lblLoginHeader = new Label
            {
                Text = "Welcome Back",
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = UITheme.TextDark,
                AutoSize = true,
                Location = new Point(45, 45)
            };

            lblLoginSub = new Label
            {
                Text = "Sign in to access your role dashboard",
                Font = UITheme.FontRegular,
                ForeColor = UITheme.TextMuted,
                AutoSize = true,
                Location = new Point(45, 80)
            };

            lblUsername = new Label
            {
                Text = "Username",
                Font = UITheme.FontRegularBold,
                ForeColor = UITheme.TextDark,
                AutoSize = true,
                Location = new Point(45, 125)
            };

            txtUsername = new TextBox
            {
                Location = new Point(45, 150),
                Width = 370,
                Font = new Font("Segoe UI", 10.5f),
                BorderStyle = BorderStyle.FixedSingle
            };

            lblPassword = new Label
            {
                Text = "Password",
                Font = UITheme.FontRegularBold,
                ForeColor = UITheme.TextDark,
                AutoSize = true,
                Location = new Point(45, 195)
            };

            txtPassword = new TextBox
            {
                Location = new Point(45, 220),
                Width = 370,
                Font = new Font("Segoe UI", 10.5f),
                PasswordChar = '•',
                BorderStyle = BorderStyle.FixedSingle
            };

            chkShowPassword = new CheckBox
            {
                Text = "Show Password",
                Font = UITheme.FontSmall,
                ForeColor = UITheme.TextMuted,
                AutoSize = true,
                Location = new Point(45, 255),
                Cursor = Cursors.Hand
            };
            chkShowPassword.CheckedChanged += (s, e) =>
            {
                txtPassword.PasswordChar = chkShowPassword.Checked ? '\0' : '•';
            };

            lblError = new Label
            {
                ForeColor = UITheme.DangerRed,
                Font = UITheme.FontSmall,
                AutoSize = false,
                Width = 370,
                Height = 35,
                Location = new Point(45, 280),
                Text = ""
            };

            btnLogin = new Button
            {
                Text = "Log In",
                Location = new Point(45, 320),
                Width = 370,
                Height = 40
            };
            UITheme.StylePrimaryButton(btnLogin);
            btnLogin.Click += BtnLogin_Click;

            btnRegister = new Button
            {
                Text = "Don't have an account? Register as Customer",
                Location = new Point(45, 370),
                Width = 370,
                Height = 36
            };
            UITheme.StyleSecondaryButton(btnRegister);
            btnRegister.Click += (s, e) =>
            {
                RegistrationForm regForm = new RegistrationForm();
                this.Hide();
                regForm.ShowDialog();
                this.Show();
                txtUsername.Focus();
            };

            btnExit = new Button
            {
                Text = "Exit Application",
                Location = new Point(45, 415),
                Width = 370,
                Height = 32,
                ForeColor = UITheme.TextMuted
            };
            UITheme.StyleSecondaryButton(btnExit);
            btnExit.Click += (s, e) => Application.Exit();

            pnlLogin.Controls.Add(lblLoginHeader);
            pnlLogin.Controls.Add(lblLoginSub);
            pnlLogin.Controls.Add(lblUsername);
            pnlLogin.Controls.Add(txtUsername);
            pnlLogin.Controls.Add(lblPassword);
            pnlLogin.Controls.Add(txtPassword);
            pnlLogin.Controls.Add(chkShowPassword);
            pnlLogin.Controls.Add(lblError);
            pnlLogin.Controls.Add(btnLogin);
            pnlLogin.Controls.Add(btnRegister);
            pnlLogin.Controls.Add(btnExit);

            this.Controls.Add(pnlLogin);
            this.Controls.Add(pnlBrand);

            this.AcceptButton = btnLogin;

            this.ResumeLayout(false);
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            lblError.Text = "";
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                lblError.Text = "Please enter both username and password.";
                return;
            }

            User user = _authService.Authenticate(username, password, out string errorMessage);
            if (user == null)
            {
                lblError.Text = errorMessage;
                return;
            }

            // Route dynamically based on user role
            Form dashboard = null;
            if (user is AdminUser || user.Role == "Admin")
            {
                dashboard = new AdminDashboard();
            }
            else if (user is EmployeeUser || user.Role == "Employee")
            {
                dashboard = new EmployeeDashboard();
            }
            else
            {
                dashboard = new CustomerDashboard();
            }

            // Transition cleanly
            this.Hide();
            txtPassword.Clear();

            dashboard.FormClosed += (s, args) =>
            {
                // When dashboard closes (via logout), return to login
                SessionManager.ClearSession();
                this.Show();
                txtUsername.Clear();
                txtPassword.Clear();
                txtUsername.Focus();
            };

            dashboard.Show();
        }
    }
}
