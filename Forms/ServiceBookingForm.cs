using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CarStoreServiceManagement.Models;
using CarStoreServiceManagement.Services;
using CarStoreServiceManagement.Utilities;

namespace CarStoreServiceManagement.Forms
{
    public class ServiceBookingForm : Form
    {
        private Panel pnlHeader;
        private Label lblVehicle;
        private ComboBox cmbCars;
        private Label lblService;
        private ComboBox cmbServices;
        private Label lblPrice;
        private TextBox txtPrice;
        private Label lblDate;
        private DateTimePicker dtpDate;
        private Label lblNoVehicle;
        private Label lblDetails;
        private TextBox txtDetails;

        private Button btnBook;
        private Button btnCancel;

        private readonly ServiceManagement _serviceManagement;

        private List<CarComboItem> _carItems = new List<CarComboItem>();
        private List<ServiceComboItem> _serviceItems = new List<ServiceComboItem>();

        private class CarComboItem
        {
            public int CarID { get; set; }
            public string Display { get; set; }
            public override string ToString() => Display;
        }

        private class ServiceComboItem
        {
            public int ServiceID { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
            public override string ToString() => $"{Name} ({Money.Format(Price)})";
        }

        public ServiceBookingForm()
        {
            _serviceManagement = new ServiceManagement();
            InitializeComponent();
            LoadDropdownData();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = "Book Vehicle Service Appointment - CarStore & Service Center";
            this.Size = new Size(580, 580);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.Font = UITheme.FontRegular;

            // Top Banner
            pnlHeader = UITheme.CreateTopBanner(
                this,
                "Book Automobile Service",
                "Select a vehicle and service package to schedule workshop maintenance.",
                (s, e) => this.Close(),
                "✕ Close"
            );

            int left = 50;
            int width = 460;

            lblVehicle = new Label { Text = "Select Vehicle *", Font = UITheme.FontRegularBold, ForeColor = UITheme.TextDark, Location = new Point(left, 90), AutoSize = true };
            cmbCars = new ComboBox { Location = new Point(left, 115), Width = width, DropDownStyle = ComboBoxStyle.DropDownList };

            lblService = new Label { Text = "Select Service Package *", Font = UITheme.FontRegularBold, ForeColor = UITheme.TextDark, Location = new Point(left, 162), AutoSize = true };
            cmbServices = new ComboBox { Location = new Point(left, 185), Width = width, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbServices.SelectedIndexChanged += CmbServices_SelectedIndexChanged;

            lblPrice = new Label { Text = "Estimated Service Cost (৳)", Font = UITheme.FontRegularBold, ForeColor = UITheme.TextDark, Location = new Point(left, 220), AutoSize = true };
            txtPrice = new TextBox { Location = new Point(left, 245), Width = width, ReadOnly = true, BackColor = Color.FromArgb(241, 245, 249), BorderStyle = BorderStyle.FixedSingle };

            lblDate = new Label { Text = "Appointment Date & Time *", UseMnemonic = false, Font = UITheme.FontRegularBold, ForeColor = UITheme.TextDark, Location = new Point(left, 285), AutoSize = true };
            dtpDate = new DateTimePicker { Location = new Point(left, 310), Width = width, Format = DateTimePickerFormat.Custom, CustomFormat = "dd MMM yyyy   hh:mm tt", MinDate = DateTime.Today, Value = DateTime.Today.AddDays(1).AddHours(10) };
            lblNoVehicle = new Label { Text = "Only vehicles you have purchased (confirmed sales) can be booked.", Font = UITheme.FontSmall, ForeColor = UITheme.TextMuted, Location = new Point(left, 141), AutoSize = true };

            lblDetails = new Label { Text = "Specific Instructions / Problem Symptoms (Optional)", Font = UITheme.FontRegularBold, ForeColor = UITheme.TextDark, Location = new Point(left, 350), AutoSize = true };
            txtDetails = new TextBox { Location = new Point(left, 375), Width = width, Height = 75, Multiline = true, BorderStyle = BorderStyle.FixedSingle };

            btnBook = new Button
            {
                Text = "✔ Confirm & Book Service",
                UseMnemonic = false,
                Location = new Point(left, 470),
                Width = 260,
                Height = 40
            };
            UITheme.StyleSuccessButton(btnBook);
            btnBook.Click += BtnBook_Click;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(left + 280, 470),
                Width = 180,
                Height = 40
            };
            UITheme.StyleSecondaryButton(btnCancel);
            btnCancel.Click += (s, e) => this.Close();

            this.Controls.Add(pnlHeader);
            this.Controls.Add(lblVehicle);
            this.Controls.Add(cmbCars);
            this.Controls.Add(lblService);
            this.Controls.Add(cmbServices);
            this.Controls.Add(lblPrice);
            this.Controls.Add(txtPrice);
            this.Controls.Add(lblDate);
            this.Controls.Add(dtpDate);
            this.Controls.Add(lblNoVehicle);
            this.Controls.Add(lblDetails);
            this.Controls.Add(txtDetails);
            this.Controls.Add(btnBook);
            this.Controls.Add(btnCancel);

            this.AcceptButton = btnBook;
            this.ResumeLayout(false);
        }

        private void LoadDropdownData()
        {
            // 1. Load the vehicles owned by this customer
            _carItems.Clear();
            DataTable carsDt = _serviceManagement.GetCustomerVehicles(SessionManager.CurrentUserID);
            foreach (DataRow r in carsDt.Rows)
            {
                _carItems.Add(new CarComboItem
                {
                    CarID = Convert.ToInt32(r["CarID"]),
                    Display = $"#{r["CarID"]} - {r["Brand"]} {r["Model"]} ({r["Year"]})"
                });
            }
            cmbCars.DataSource = _carItems;

            if (_carItems.Count == 0)
            {
                lblNoVehicle.Text = "You have no purchased vehicle yet. Service booking opens after a sale is confirmed.";
                lblNoVehicle.ForeColor = UITheme.DangerRed;
                btnBook.Enabled = false;
                btnBook.BackColor = Color.FromArgb(203, 213, 225);
            }

            // 2. Load Services
            _serviceItems.Clear();
            DataTable srvDt = _serviceManagement.GetAllServices();
            foreach (DataRow r in srvDt.Rows)
            {
                _serviceItems.Add(new ServiceComboItem
                {
                    ServiceID = Convert.ToInt32(r["ServiceID"]),
                    Name = r["ServiceName"].ToString(),
                    Price = Convert.ToDecimal(r["Price"])
                });
            }
            cmbServices.DataSource = _serviceItems;
        }

        private void CmbServices_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbServices.SelectedItem is ServiceComboItem item)
            {
                txtPrice.Text = Money.Format(item.Price);
            }
        }

        private void BtnBook_Click(object sender, EventArgs e)
        {
            if (cmbCars.SelectedItem == null)
            {
                MessageBox.Show("Please select a vehicle.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbServices.SelectedItem == null)
            {
                MessageBox.Show("Please select a service procedure.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            CarComboItem carItem = (CarComboItem)cmbCars.SelectedItem;
            ServiceComboItem srvItem = (ServiceComboItem)cmbServices.SelectedItem;
            string details = txtDetails.Text.Trim();

            bool success = _serviceManagement.BookService(
                carItem.CarID,
                SessionManager.CurrentUserID,
                srvItem.ServiceID,
                dtpDate.Value,
                string.IsNullOrEmpty(details) ? srvItem.Name : details,
                out string errorMessage);

            if (success)
            {
                MessageBox.Show(
                    "Service booked successfully!\n\nYour service record is now in 'Pending' status. Our workshop technicians will review and process your vehicle.",
                    "Service Booked",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show(errorMessage ?? "Service booking failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
