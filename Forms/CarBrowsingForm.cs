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
    public class CarBrowsingForm : Form
    {
        private Panel pnlHeader;
        private FlowLayoutPanel pnlFilter;
        private Panel pnlPreview;
        private Panel pnlGrid;

        private TextBox txtSearch;
        private ComboBox cmbBrand;
        private TextBox txtMaxPrice;
        private Button btnFilter;
        private Button btnReset;

        private DataGridView dgvCars;

        // Preview Card Controls
        private PictureBox picPreview;
        private Label lblPreviewTitle;
        private Label lblPreviewPrice;
        private Label lblPreviewYear;
        private Label lblPreviewStatus;
        private Button btnViewDetails;
        private Button btnRequestPurchase;

        private readonly CarService _carService;
        private readonly SalesService _salesService;
        private Car _selectedCar;

        public CarBrowsingForm()
        {
            _carService = new CarService();
            _salesService = new SalesService();
            InitializeComponent();
            LoadBrands();
            LoadAvailableCars();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = "Showroom - Browse Available Vehicles";
            this.Size = new Size(1150, 720);
            this.MinimumSize = new Size(680, 480); // Allows split screen side-by-side
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = UITheme.FormBg;
            this.Font = UITheme.FontRegular;

            // 1. Top Header Banner
            pnlHeader = UITheme.CreateTopBanner(
                this,
                "Automobile Showroom & Inventory",
                "Explore certified vehicles, filter by brand & budget, and submit immediate purchase requests.",
                (s, e) => this.Close(),
                "← Close"
            );

            // 2. Responsive Wrapping Filter Bar
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

            Label lblSearch = new Label { Text = "Keyword:", AutoSize = true, Font = UITheme.FontRegularBold, Margin = new Padding(0, 6, 4, 6) };
            txtSearch = new TextBox { Width = 140, Font = new Font("Segoe UI", 10f), BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 4, 10, 6) };

            Label lblBrand = new Label { Text = "Brand:", AutoSize = true, Font = UITheme.FontRegularBold, Margin = new Padding(0, 6, 4, 6) };
            cmbBrand = new ComboBox { Width = 130, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 4, 10, 6) };

            Label lblMax = new Label { Text = "Max Budget (৳):", AutoSize = true, Font = UITheme.FontRegularBold, Margin = new Padding(0, 6, 4, 6) };
            txtMaxPrice = new TextBox { Width = 95, Font = new Font("Segoe UI", 10f), BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 4, 12, 6) };

            btnFilter = new Button { Text = "Filter Cars", Width = 90, Height = 28, Margin = new Padding(0, 3, 8, 6) };
            UITheme.StylePrimaryButton(btnFilter);
            btnFilter.Click += (s, e) => LoadAvailableCars();

            btnReset = new Button { Text = "Reset", Width = 75, Height = 28, Margin = new Padding(0, 3, 6, 6) };
            UITheme.StyleSecondaryButton(btnReset);
            btnReset.Click += (s, e) =>
            {
                txtSearch.Clear();
                cmbBrand.SelectedIndex = 0;
                txtMaxPrice.Clear();
                LoadAvailableCars();
            };

            pnlFilter.Controls.Add(lblSearch);
            pnlFilter.Controls.Add(txtSearch);
            pnlFilter.Controls.Add(lblBrand);
            pnlFilter.Controls.Add(cmbBrand);
            pnlFilter.Controls.Add(lblMax);
            pnlFilter.Controls.Add(txtMaxPrice);
            pnlFilter.Controls.Add(btnFilter);
            pnlFilter.Controls.Add(btnReset);

            // 3. Side Preview Card Panel (Right side - 310px)
            pnlPreview = new Panel
            {
                Dock = DockStyle.Right,
                Width = 310,
                BackColor = Color.White,
                Padding = new Padding(15),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblPreviewHeader = new Label
            {
                Text = "Selected Vehicle",
                Font = UITheme.FontHeader,
                ForeColor = UITheme.TextDark,
                Location = new Point(20, 15),
                AutoSize = true
            };
            pnlPreview.Controls.Add(lblPreviewHeader);

            picPreview = new PictureBox
            {
                Location = new Point(20, 45),
                Size = new Size(270, 160),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(248, 250, 252)
            };
            pnlPreview.Controls.Add(picPreview);

            lblPreviewTitle = new Label
            {
                Text = "Select a vehicle",
                Font = UITheme.FontSubtitle,
                ForeColor = UITheme.TextDark,
                Location = new Point(20, 215),
                AutoSize = true
            };
            pnlPreview.Controls.Add(lblPreviewTitle);

            lblPreviewYear = new Label
            {
                Text = "Year / Specs",
                Font = UITheme.FontSmall,
                ForeColor = UITheme.TextMuted,
                Location = new Point(20, 242),
                AutoSize = true
            };
            pnlPreview.Controls.Add(lblPreviewYear);

            lblPreviewPrice = new Label
            {
                Text = "৳ 0",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = UITheme.PrimaryBlue,
                Location = new Point(20, 270),
                AutoSize = true
            };
            pnlPreview.Controls.Add(lblPreviewPrice);

            lblPreviewStatus = new Label
            {
                Text = "Available",
                Font = UITheme.FontRegularBold,
                ForeColor = UITheme.SuccessGreen,
                Location = new Point(20, 305),
                AutoSize = true
            };
            pnlPreview.Controls.Add(lblPreviewStatus);

            btnViewDetails = new Button
            {
                Text = "🔍 View Full Details & Specs",
                UseMnemonic = false,
                Location = new Point(20, 345),
                Size = new Size(270, 36)
            };
            UITheme.StyleSecondaryButton(btnViewDetails);
            btnViewDetails.Click += BtnViewDetails_Click;
            pnlPreview.Controls.Add(btnViewDetails);

            btnRequestPurchase = new Button
            {
                Text = "🛒 Request Purchase",
                Location = new Point(20, 390),
                Size = new Size(270, 40)
            };
            UITheme.StyleSuccessButton(btnRequestPurchase);
            btnRequestPurchase.Click += BtnRequestPurchase_Click;
            btnRequestPurchase.Visible = SessionManager.CurrentUserObject?.CanRequestPurchase() == true;
            pnlPreview.Controls.Add(btnRequestPurchase);

            // 4. Grid Panel (Fill Center)
            pnlGrid = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            dgvCars = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            UITheme.StyleDataGridView(dgvCars);
            dgvCars.SelectionChanged += DgvCars_SelectionChanged;
            dgvCars.CellFormatting += DgvCars_CellFormatting;
            dgvCars.DoubleClick += (s, e) => BtnViewDetails_Click(s, e);

            pnlGrid.Controls.Add(dgvCars);

            this.Controls.Add(pnlGrid);
            this.Controls.Add(pnlPreview);
            this.Controls.Add(pnlFilter);
            this.Controls.Add(pnlHeader);

            this.ResumeLayout(false);
        }

        private void LoadBrands()
        {
            cmbBrand.Items.Clear();
            cmbBrand.Items.Add("All Brands");
            var brands = _carService.GetDistinctBrands();
            foreach (var b in brands)
            {
                cmbBrand.Items.Add(b);
            }
            cmbBrand.SelectedIndex = 0;
        }

        private void LoadAvailableCars()
        {
            string keyword = txtSearch.Text.Trim();
            string brand = cmbBrand.SelectedItem?.ToString();
            decimal? maxPrice = null;

            if (ValidationHelper.IsValidPrice(txtMaxPrice.Text, out decimal parsedMax) && parsedMax > 0)
            {
                maxPrice = parsedMax;
            }

            DataTable dt = _carService.GetAvailableCars(keyword, brand, null, maxPrice);
            dgvCars.DataSource = dt;

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
            if (dgvCars.Columns.Contains("CreatedAt")) dgvCars.Columns["CreatedAt"].HeaderText = "Listed On";
        }

        private void DgvCars_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvCars.SelectedRows.Count > 0)
            {
                DataGridViewRow r = dgvCars.SelectedRows[0];
                int carId = Convert.ToInt32(r.Cells["CarID"].Value);
                _selectedCar = _carService.GetCarById(carId);

                if (_selectedCar != null)
                {
                    lblPreviewTitle.Text = $"{_selectedCar.Brand} {_selectedCar.Model}";
                    lblPreviewYear.Text = $"Year: {_selectedCar.Year} | ID: #{_selectedCar.CarID}";
                    lblPreviewPrice.Text = _selectedCar.FormattedPrice;
                    lblPreviewStatus.Text = _selectedCar.Status;

                    if (_selectedCar.IsAvailable)
                    {
                        lblPreviewStatus.ForeColor = UITheme.SuccessGreen;
                        btnRequestPurchase.Enabled = true;
                    }
                    else
                    {
                        lblPreviewStatus.ForeColor = UITheme.DangerRed;
                        btnRequestPurchase.Enabled = false;
                    }

                    LoadPreviewImage(_selectedCar.ImagePath);
                }
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

        private void BtnViewDetails_Click(object sender, EventArgs e)
        {
            if (_selectedCar == null)
            {
                MessageBox.Show("Please select a car from the showroom grid.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            CarDetailsForm details = new CarDetailsForm(_selectedCar);
            if (details.ShowDialog() == DialogResult.OK)
            {
                LoadAvailableCars();
            }
        }

        private void BtnRequestPurchase_Click(object sender, EventArgs e)
        {
            if (_selectedCar == null)
            {
                MessageBox.Show("Please select a car to request purchase.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!SessionManager.IsLoggedIn)
            {
                MessageBox.Show("Please log in to submit a purchase request.", "Login Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                $"Submit purchase request for {_selectedCar.DisplayName} at {_selectedCar.FormattedPrice}?",
                "Confirm Purchase Request",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                bool success = _salesService.RequestPurchase(_selectedCar.CarID, SessionManager.CurrentUserID, _selectedCar.Price, out string err);
                if (success)
                {
                    MessageBox.Show(
                        "Purchase request submitted successfully!\n\nYour order is now Pending review by our sales staff.",
                        "Request Submitted",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    LoadAvailableCars();
                }
                else
                {
                    MessageBox.Show(err ?? "Purchase request could not be processed.", "Request Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
