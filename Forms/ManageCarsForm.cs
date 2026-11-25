using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CarStoreServiceManagement.Models;
using CarStoreServiceManagement.Services;
using CarStoreServiceManagement.Utilities;

namespace CarStoreServiceManagement.Forms
{
    public class ManageCarsForm : Form
    {
        private Panel pnlHeader;
        private Panel pnlEditor;
        private Panel pnlGrid;

        // Form Controls
        private Label lblCarId;
        private TextBox txtCarId;
        private Label lblBrand;
        private TextBox txtBrand;
        private Label lblModel;
        private TextBox txtModel;
        private Label lblYear;
        private TextBox txtYear;
        private Label lblPrice;
        private TextBox txtPrice;
        private Label lblStatus;
        private ComboBox cmbStatus;
        private Label lblImagePath;
        private TextBox txtImagePath;
        private Button btnBrowseImage;
        private PictureBox picPreview;

        // Action Buttons
        private Button btnAdd;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnClear;

        // Search & Filter
        private TextBox txtSearch;
        private Button btnSearch;
        private ComboBox cmbFilterStatus;
        private Button btnRefresh;

        private DataGridView dgvCars;

        private readonly CarService _carService;

        public ManageCarsForm()
        {
            _carService = new CarService();
            InitializeComponent();
            LoadCarData();
            AccessGuard.Require(this, user => user.CanManageCars());
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = "Vehicle Inventory Management - CarStore & Service Center";
            this.Size = new Size(1150, 720);
            this.MinimumSize = new Size(680, 480); // Allows split screen side-by-side
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = UITheme.FormBg;
            this.Font = UITheme.FontRegular;

            // 1. Top Header Banner
            pnlHeader = UITheme.CreateTopBanner(
                this,
                "Vehicle Inventory Management (CarsStock)",
                "Full CRUD operations: Add new cars, update pricing, change availability status, and manage images.",
                (s, e) => this.Close(),
                "← Close"
            );

            // 2. Editor Panel (Left side - 340px)
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
                Text = "Vehicle Information & Specs",
                UseMnemonic = false,
                Font = UITheme.FontHeader,
                ForeColor = UITheme.TextDark,
                Location = new Point(15, 15),
                AutoSize = true
            };
            pnlEditor.Controls.Add(lblSection);

            int left = 15;
            int width = 290;

            lblCarId = new Label { Text = "Car ID (Auto-Generated)", Font = UITheme.FontSmall, ForeColor = UITheme.TextMuted, Location = new Point(left, 45), AutoSize = true };
            txtCarId = new TextBox { Location = new Point(left, 65), Width = width, ReadOnly = true, BackColor = Color.FromArgb(241, 245, 249), BorderStyle = BorderStyle.FixedSingle };

            lblBrand = new Label { Text = "Brand / Manufacturer *", Font = UITheme.FontRegularBold, ForeColor = UITheme.TextDark, Location = new Point(left, 95), AutoSize = true };
            txtBrand = new TextBox { Location = new Point(left, 115), Width = width, BorderStyle = BorderStyle.FixedSingle };

            lblModel = new Label { Text = "Model Name *", Font = UITheme.FontRegularBold, ForeColor = UITheme.TextDark, Location = new Point(left, 145), AutoSize = true };
            txtModel = new TextBox { Location = new Point(left, 165), Width = width, BorderStyle = BorderStyle.FixedSingle };

            lblYear = new Label { Text = "Manufacturing Year *", Font = UITheme.FontRegularBold, ForeColor = UITheme.TextDark, Location = new Point(left, 195), AutoSize = true };
            txtYear = new TextBox { Location = new Point(left, 215), Width = width, BorderStyle = BorderStyle.FixedSingle };

            lblPrice = new Label { Text = "Price (৳ Taka) *", Font = UITheme.FontRegularBold, ForeColor = UITheme.TextDark, Location = new Point(left, 245), AutoSize = true };
            txtPrice = new TextBox { Location = new Point(left, 265), Width = width, BorderStyle = BorderStyle.FixedSingle };

            lblStatus = new Label { Text = "Inventory Status *", Font = UITheme.FontRegularBold, ForeColor = UITheme.TextDark, Location = new Point(left, 295), AutoSize = true };
            cmbStatus = new ComboBox { Location = new Point(left, 315), Width = width, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.AddRange(new object[] { "Available", "Reserved", "Sold", "Unavailable" });
            cmbStatus.SelectedIndex = 0;

            lblImagePath = new Label { Text = "Image Path / File Name", Font = UITheme.FontRegularBold, ForeColor = UITheme.TextDark, Location = new Point(left, 348), AutoSize = true };
            txtImagePath = new TextBox { Location = new Point(left, 368), Width = 190, BorderStyle = BorderStyle.FixedSingle };
            btnBrowseImage = new Button { Text = "Browse...", Location = new Point(left + 200, 366), Width = 90, Height = 25 };
            UITheme.StyleSecondaryButton(btnBrowseImage);
            btnBrowseImage.Click += BtnBrowseImage_Click;

            // Picture preview box
            picPreview = new PictureBox
            {
                Location = new Point(left, 400),
                Size = new Size(width, 105),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(248, 250, 252)
            };

            // CRUD Action Buttons
            btnAdd = new Button { Text = "+ Add Car", Location = new Point(left, 515), Width = 140, Height = 34 };
            UITheme.StyleSuccessButton(btnAdd);
            btnAdd.Click += BtnAdd_Click;

            btnUpdate = new Button { Text = "✎ Update Car", Location = new Point(left + 150, 515), Width = 140, Height = 34 };
            UITheme.StylePrimaryButton(btnUpdate);
            btnUpdate.Click += BtnUpdate_Click;

            btnDelete = new Button { Text = "✖ Delete Car", Location = new Point(left, 555), Width = 140, Height = 34 };
            UITheme.StyleDangerButton(btnDelete);
            btnDelete.Click += BtnDelete_Click;

            btnClear = new Button { Text = "Clear Fields", Location = new Point(left + 150, 555), Width = 140, Height = 34 };
            UITheme.StyleSecondaryButton(btnClear);
            btnClear.Click += (s, e) => ClearForm();

            pnlEditor.Controls.Add(lblCarId);
            pnlEditor.Controls.Add(txtCarId);
            pnlEditor.Controls.Add(lblBrand);
            pnlEditor.Controls.Add(txtBrand);
            pnlEditor.Controls.Add(lblModel);
            pnlEditor.Controls.Add(txtModel);
            pnlEditor.Controls.Add(lblYear);
            pnlEditor.Controls.Add(txtYear);
            pnlEditor.Controls.Add(lblPrice);
            pnlEditor.Controls.Add(txtPrice);
            pnlEditor.Controls.Add(lblStatus);
            pnlEditor.Controls.Add(cmbStatus);
            pnlEditor.Controls.Add(lblImagePath);
            pnlEditor.Controls.Add(txtImagePath);
            pnlEditor.Controls.Add(btnBrowseImage);
            pnlEditor.Controls.Add(picPreview);
            pnlEditor.Controls.Add(btnAdd);
            pnlEditor.Controls.Add(btnUpdate);
            pnlEditor.Controls.Add(btnDelete);
            pnlEditor.Controls.Add(btnClear);

            // 3. Grid Panel (Right side)
            pnlGrid = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15)
            };

            // Responsive Wrapping Search Filter Bar
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
            btnSearch.Click += (s, e) => LoadCarData();

            Label lblFilt = new Label { Text = "Status Filter:", AutoSize = true, Font = UITheme.FontRegularBold, Margin = new Padding(0, 6, 6, 6) };
            cmbFilterStatus = new ComboBox { Width = 120, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 4, 10, 6) };
            cmbFilterStatus.Items.AddRange(new object[] { "All", "Available", "Reserved", "Sold", "Unavailable" });
            cmbFilterStatus.SelectedIndex = 0;
            cmbFilterStatus.SelectedIndexChanged += (s, e) => LoadCarData();

            btnRefresh = new Button { Text = "Refresh", Width = 75, Height = 28, Margin = new Padding(0, 3, 6, 6) };
            UITheme.StyleSecondaryButton(btnRefresh);
            btnRefresh.Click += (s, e) => { txtSearch.Clear(); cmbFilterStatus.SelectedIndex = 0; LoadCarData(); };

            pnlSearch.Controls.Add(lblSearch);
            pnlSearch.Controls.Add(txtSearch);
            pnlSearch.Controls.Add(btnSearch);
            pnlSearch.Controls.Add(lblFilt);
            pnlSearch.Controls.Add(cmbFilterStatus);
            pnlSearch.Controls.Add(btnRefresh);

            dgvCars = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            UITheme.StyleDataGridView(dgvCars);
            dgvCars.SelectionChanged += DgvCars_SelectionChanged;
            dgvCars.CellFormatting += DgvCars_CellFormatting;

            pnlGrid.Controls.Add(dgvCars);
            pnlGrid.Controls.Add(pnlSearch);

            this.Controls.Add(pnlGrid);
            this.Controls.Add(pnlEditor);
            this.Controls.Add(pnlHeader);

            this.ResumeLayout(false);
        }

        private void LoadCarData()
        {
            string keyword = txtSearch.Text.Trim();
            string statusFilter = cmbFilterStatus.SelectedItem?.ToString();

            DataTable dt = _carService.GetAllCars(keyword, null, null, null, statusFilter);
            dgvCars.DataSource = dt;

            // Header labels formatting
            if (dgvCars.Columns.Contains("CarID")) dgvCars.Columns["CarID"].HeaderText = "ID";
            if (dgvCars.Columns.Contains("Brand")) dgvCars.Columns["Brand"].HeaderText = "Brand";
            if (dgvCars.Columns.Contains("Model")) dgvCars.Columns["Model"].HeaderText = "Model";
            if (dgvCars.Columns.Contains("Year")) dgvCars.Columns["Year"].HeaderText = "Year";
            if (dgvCars.Columns.Contains("Price"))
            {
                dgvCars.Columns["Price"].HeaderText = "Price (৳)";
                Money.FormatColumn(dgvCars.Columns["Price"]);
            }
            if (dgvCars.Columns.Contains("Status")) dgvCars.Columns["Status"].HeaderText = "Status";
            if (dgvCars.Columns.Contains("Image_Path")) dgvCars.Columns["Image_Path"].Visible = false;
            if (dgvCars.Columns.Contains("CreatedAt")) dgvCars.Columns["CreatedAt"].HeaderText = "Added On";
        }

        private void DgvCars_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvCars.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dgvCars.SelectedRows[0];
                txtCarId.Text = row.Cells["CarID"].Value?.ToString();
                txtBrand.Text = row.Cells["Brand"].Value?.ToString();
                txtModel.Text = row.Cells["Model"].Value?.ToString();
                txtYear.Text = row.Cells["Year"].Value?.ToString();
                txtPrice.Text = row.Cells["Price"].Value?.ToString();

                string status = row.Cells["Status"].Value?.ToString();
                if (!string.IsNullOrEmpty(status) && cmbStatus.Items.Contains(status))
                {
                    cmbStatus.SelectedItem = status;
                }

                string img = row.Cells["Image_Path"].Value?.ToString();
                txtImagePath.Text = img ?? "";
                LoadPreviewImage(img);
            }
        }

        private void DgvCars_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvCars.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                UITheme.ColorizeStatusCell(e, e.Value.ToString());
            }
        }

        private void LoadPreviewImage(string path)
        {
            ImageHelper.Show(picPreview, path);
        }

        private void BtnBrowseImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp";
                ofd.Title = "Select Vehicle Image";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtImagePath.Text = ofd.FileName;
                    LoadPreviewImage(ofd.FileName);
                }
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs(out Car car)) return;

            car.ImagePath = ImageHelper.Import(car.ImagePath);
            bool success = _carService.AddCar(car, out string errorMessage);
            if (success)
            {
                MessageBox.Show("Car added successfully to inventory.", "Operation Succeeded", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
                LoadCarData();
            }
            else
            {
                MessageBox.Show(errorMessage, "Error Adding Car", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtCarId.Text))
            {
                MessageBox.Show("Please select a car from the inventory table to update.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidateInputs(out Car car)) return;
            car.CarID = Convert.ToInt32(txtCarId.Text);

            car.ImagePath = ImageHelper.Import(car.ImagePath);
            bool success = _carService.UpdateCar(car, out string errorMessage);
            if (success)
            {
                MessageBox.Show("Car information updated successfully.", "Operation Succeeded", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadCarData();
            }
            else
            {
                MessageBox.Show(errorMessage, "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtCarId.Text))
            {
                MessageBox.Show("Please select a vehicle to delete.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int carId = Convert.ToInt32(txtCarId.Text);
            string carDesc = $"{txtBrand.Text} {txtModel.Text}";

            var result = MessageBox.Show(
                $"Are you sure you want to delete this car ({carDesc}) from inventory?",
                "Confirm Vehicle Deletion",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                bool success = _carService.DeleteCar(carId, out string errorMessage);
                if (success)
                {
                    MessageBox.Show("Car removed from inventory successfully.", "Car Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm();
                    LoadCarData();
                }
                else
                {
                    MessageBox.Show(errorMessage, "Deletion Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadCarData();
                }
            }
        }

        private bool ValidateInputs(out Car car)
        {
            car = null;

            string brand = txtBrand.Text.Trim();
            string model = txtModel.Text.Trim();
            string yearText = txtYear.Text.Trim();
            string priceText = txtPrice.Text.Trim();
            string status = cmbStatus.SelectedItem?.ToString();
            string imagePath = txtImagePath.Text.Trim();

            if (string.IsNullOrEmpty(brand))
            {
                MessageBox.Show("Brand name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtBrand.Focus();
                return false;
            }

            if (string.IsNullOrEmpty(model))
            {
                MessageBox.Show("Model name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtModel.Focus();
                return false;
            }

            if (!ValidationHelper.IsValidYear(yearText, out int year))
            {
                MessageBox.Show($"Please enter a valid year between 1990 and {DateTime.Now.Year + 2}.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtYear.Focus();
                return false;
            }

            if (!ValidationHelper.IsValidPrice(priceText, out decimal price) || price <= 0)
            {
                MessageBox.Show("Please enter a valid price greater than zero.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPrice.Focus();
                return false;
            }

            car = new Car
            {
                Brand = brand,
                Model = model,
                Year = year,
                Price = price,
                Status = status ?? "Available",
                ImagePath = string.IsNullOrEmpty(imagePath) ? null : imagePath
            };

            return true;
        }

        private void ClearForm()
        {
            txtCarId.Clear();
            txtBrand.Clear();
            txtModel.Clear();
            txtYear.Clear();
            txtPrice.Clear();
            cmbStatus.SelectedIndex = 0;
            txtImagePath.Clear();
            ImageHelper.Show(picPreview, null);
            if (dgvCars.SelectedRows.Count > 0)
            {
                dgvCars.ClearSelection();
            }
        }
    }
}
