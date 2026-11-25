using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CarStoreServiceManagement.Models;
using CarStoreServiceManagement.Services;
using CarStoreServiceManagement.Utilities;

namespace CarStoreServiceManagement.Forms
{
    public class CarDetailsForm : Form
    {
        private Panel pnlHeader;
        private PictureBox picCar;
        private Label lblBrandModel;
        private Label lblYear;
        private Label lblPrice;
        private Label lblStatus;
        private Label lblSpecs;
        private Button btnPurchase;
        private Button btnClose;

        private readonly Car _car;
        private readonly SalesService _salesService;

        public CarDetailsForm(Car car)
        {
            _car = car ?? throw new ArgumentNullException(nameof(car));
            _salesService = new SalesService();
            InitializeComponent();
            PopulateCarDetails();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = $"Vehicle Details - {_car.DisplayName}";
            this.Size = new Size(620, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.Font = UITheme.FontRegular;

            // Top Banner
            pnlHeader = UITheme.CreateTopBanner(
                this,
                _car.DisplayName,
                "Showroom Specifications & Purchase Request",
                (s, e) => this.Close(),
                "✕ Close"
            );

            // Picture Box
            picCar = new PictureBox
            {
                Location = new Point(35, 85),
                Size = new Size(535, 230),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(248, 250, 252)
            };

            // Details labels
            int left = 35;
            lblBrandModel = new Label
            {
                Location = new Point(left, 330),
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = UITheme.TextDark,
                AutoSize = true
            };

            lblYear = new Label
            {
                Location = new Point(left, 365),
                Font = UITheme.FontRegular,
                ForeColor = UITheme.TextMuted,
                AutoSize = true
            };

            lblPrice = new Label
            {
                Location = new Point(left, 395),
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = UITheme.PrimaryBlue,
                AutoSize = true
            };

            lblStatus = new Label
            {
                Location = new Point(left + 350, 398),
                Font = UITheme.FontSubtitle,
                AutoSize = true
            };

            lblSpecs = new Label
            {
                Location = new Point(left, 435),
                Size = new Size(535, 60),
                Font = UITheme.FontSmall,
                ForeColor = Color.FromArgb(71, 85, 105),
                Text = "Comprehensive multi-point inspected automobile. Features standard warranty, diagnostic certification, and dealer support."
            };

            // Action Buttons
            btnPurchase = new Button
            {
                Text = "🛒 Submit Purchase Request",
                Location = new Point(left, 500),
                Size = new Size(360, 42)
            };
            UITheme.StyleSuccessButton(btnPurchase);
            btnPurchase.Click += BtnPurchase_Click;

            btnClose = new Button
            {
                Text = "Back to Showroom",
                Location = new Point(left + 375, 500),
                Size = new Size(160, 42)
            };
            UITheme.StyleSecondaryButton(btnClose);
            btnClose.Click += (s, e) => this.Close();

            this.Controls.Add(pnlHeader);
            this.Controls.Add(picCar);
            this.Controls.Add(lblBrandModel);
            this.Controls.Add(lblYear);
            this.Controls.Add(lblPrice);
            this.Controls.Add(lblStatus);
            this.Controls.Add(lblSpecs);
            this.Controls.Add(btnPurchase);
            this.Controls.Add(btnClose);

            this.ResumeLayout(false);
        }

        private void PopulateCarDetails()
        {
            lblBrandModel.Text = $"{_car.Brand} {_car.Model}";
            lblYear.Text = $"Model Year: {_car.Year}  |  Stock Inventory ID: #{_car.CarID}";
            lblPrice.Text = _car.FormattedPrice;
            lblStatus.Text = $"Status: {_car.Status.ToUpper()}";

            if (string.Equals(_car.Status, "Available", StringComparison.OrdinalIgnoreCase))
            {
                lblStatus.ForeColor = UITheme.SuccessGreen;
                btnPurchase.Enabled = true;
            }
            else
            {
                lblStatus.ForeColor = UITheme.DangerRed;
                btnPurchase.Enabled = false;
                btnPurchase.Text = $"Vehicle is {_car.Status}";
                btnPurchase.BackColor = Color.FromArgb(203, 213, 225);
                btnPurchase.ForeColor = Color.FromArgb(100, 116, 139);
            }

            btnPurchase.Visible = SessionManager.CurrentUserObject?.CanRequestPurchase() == true;
            ImageHelper.Show(picCar, _car.ImagePath);
        }

        private void BtnPurchase_Click(object sender, EventArgs e)
        {
            if (!SessionManager.IsLoggedIn)
            {
                MessageBox.Show("Please log in to submit a purchase request.", "Authentication Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                $"Would you like to request purchase for {_car.DisplayName} at {_car.FormattedPrice}?\n\nA sales representative will review and confirm your order.",
                "Confirm Purchase Request",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                bool success = _salesService.RequestPurchase(_car.CarID, SessionManager.CurrentUserID, _car.Price, out string error);
                if (success)
                {
                    MessageBox.Show(
                        "Purchase request submitted successfully!\n\nYour order is now Pending. An employee will process and confirm your booking.",
                        "Order Submitted",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show(error ?? "Unable to submit purchase request at this time.", "Request Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
