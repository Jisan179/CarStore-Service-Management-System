using System;
using System.Drawing;
using System.Windows.Forms;
using CarStoreServiceManagement.Services;
using CarStoreServiceManagement.Utilities;

namespace CarStoreServiceManagement.Forms
{
    public class EmployeeDashboard : Form
    {
        private Panel pnlHeader;
        private Label lblWelcome;
        private Label lblUserRole;
        private Label lblClock;
        private Button btnLogout;
        private Button btnProfile;

        private Panel pnlBody;
        private Label lblMetricsTitle;
        private TableLayoutPanel pnlMetrics;
        private Label lblNavTitle;
        private FlowLayoutPanel pnlQuickNav;

        private Timer clockTimer;
        private readonly DashboardService _dashboardService;
        private EmployeeDashboardStats _cachedStats;

        // Cached Stat Cards & Value Labels for zero-flicker resizing
        private Panel _cardCars;
        private Label _lblValCars;
        private Panel _cardSales;
        private Label _lblValSales;
        private Panel _cardServices;
        private Label _lblValServices;
        private Panel _cardInventory;
        private Label _lblValInventory;
        private bool? _lastIsWide = null;

        public EmployeeDashboard()
        {
            _dashboardService = new DashboardService();
            InitializeComponent();
            LoadStatistics();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = SessionManager.CurrentUserObject?.GetDashboardTitle() ?? "Employee Operations Portal - CarStore and Service Management System";
            this.Size = new Size(1060, 720);
            this.MinimumSize = new Size(580, 480);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = UITheme.FormBg;
            this.Font = UITheme.FontRegular;

            // 1. Header Panel
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 85,
                BackColor = UITheme.HeaderBg,
                Padding = new Padding(20, 12, 20, 12)
            };

            lblWelcome = new Label
            {
                Text = $"Welcome, {SessionManager.CurrentFullName}",
                Font = UITheme.FontTitle,
                ForeColor = Color.White,
                AutoSize = true,
                UseMnemonic = false,
                Location = new Point(20, 14)
            };

            lblUserRole = new Label
            {
                Text = $"Role: Staff Employee | Operations Center | @{SessionManager.CurrentUsername}",
                Font = UITheme.FontSmall,
                ForeColor = Color.FromArgb(148, 163, 184),
                AutoSize = true,
                UseMnemonic = false,
                Location = new Point(20, 50)
            };

            lblClock = new Label
            {
                Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy - HH:mm:ss"),
                Font = UITheme.FontRegularBold,
                ForeColor = Color.FromArgb(226, 232, 240),
                AutoSize = true,
                UseMnemonic = false,
                Location = new Point(500, 18)
            };

            btnProfile = new Button
            {
                Text = "My Profile",
                Size = new Size(95, 32)
            };
            UITheme.StyleSecondaryButton(btnProfile);
            btnProfile.Click += (s, e) =>
            {
                ProfileForm pf = new ProfileForm();
                pf.ShowDialog();
                lblWelcome.Text = $"Welcome, {SessionManager.CurrentFullName}";
            };

            btnLogout = new Button
            {
                Text = "Logout",
                Size = new Size(85, 32)
            };
            UITheme.StyleDangerButton(btnLogout);
            btnLogout.Click += (s, e) =>
            {
                var confirm = MessageBox.Show("Are you sure you want to log out of the Employee Portal?", "Confirm Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm == DialogResult.Yes)
                {
                    this.Close();
                }
            };

            pnlHeader.Controls.Add(lblWelcome);
            pnlHeader.Controls.Add(lblUserRole);
            pnlHeader.Controls.Add(lblClock);
            pnlHeader.Controls.Add(btnProfile);
            pnlHeader.Controls.Add(btnLogout);

            pnlHeader.Resize += (s, e) => LayoutHeader();

            // 2. Main Scrollable Body
            pnlBody = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = UITheme.FormBg
            };

            lblMetricsTitle = new Label
            {
                Text = "Operational Queue & Inventory Status",
                Font = UITheme.FontSubtitle,
                ForeColor = UITheme.TextDark,
                AutoSize = true,
                UseMnemonic = false,
                Location = new Point(20, 15)
            };
            pnlBody.Controls.Add(lblMetricsTitle);

            pnlMetrics = new TableLayoutPanel
            {
                Location = new Point(20, 45),
                BackColor = Color.Transparent,
                Margin = new Padding(0)
            };
            pnlBody.Controls.Add(pnlMetrics);

            CreateStatCards();

            lblNavTitle = new Label
            {
                Text = "Employee Workflows & Customer Order Processing",
                Font = UITheme.FontSubtitle,
                ForeColor = UITheme.TextDark,
                AutoSize = true,
                UseMnemonic = false,
                Location = new Point(20, 165)
            };
            pnlBody.Controls.Add(lblNavTitle);

            pnlQuickNav = new FlowLayoutPanel
            {
                Location = new Point(20, 195),
                AutoScroll = false,
                WrapContents = true,
                BackColor = Color.Transparent,
                Margin = new Padding(0)
            };
            pnlBody.Controls.Add(pnlQuickNav);

            SetupNavButtons();

            this.Controls.Add(pnlBody);
            this.Controls.Add(pnlHeader);

            pnlBody.Resize += (s, e) => RelayoutDashboard();
            this.Resize += (s, e) => RelayoutDashboard();

            clockTimer = new Timer { Interval = 1000 };
            clockTimer.Tick += (s, e) =>
            {
                lblClock.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy - HH:mm:ss");
            };
            clockTimer.Start();

            this.FormClosed += (s, e) => clockTimer?.Stop();

            this.ResumeLayout(false);
        }

        private void LayoutHeader()
        {
            btnProfile.Location = new Point(pnlHeader.Width - 195, 42);
            btnLogout.Location = new Point(pnlHeader.Width - 95, 42);

            if (pnlHeader.Width >= 750)
            {
                lblClock.Visible = true;
                lblClock.Location = new Point(Math.Max(320, pnlHeader.Width - 430), 18);
            }
            else
            {
                lblClock.Visible = false;
            }
        }

        private void CreateStatCards()
        {
            _cardCars = BuildStatCardItem("AVAILABLE CARS", "0", "Ready in Showroom", UITheme.SuccessGreen, out _lblValCars);
            _cardSales = BuildStatCardItem("PENDING SALES", "0", "Purchase Requests", UITheme.PrimaryBlue, out _lblValSales);
            _cardServices = BuildStatCardItem("PENDING SERVICES", "0", "Workshop Queue", UITheme.WarningAmber, out _lblValServices);
            _cardInventory = BuildStatCardItem("TOTAL INVENTORY", "0", "Cars In Stock", Color.FromArgb(71, 85, 105), out _lblValInventory);

            pnlMetrics.Controls.Add(_cardCars);
            pnlMetrics.Controls.Add(_cardSales);
            pnlMetrics.Controls.Add(_cardServices);
            pnlMetrics.Controls.Add(_cardInventory);
        }

        private Panel BuildStatCardItem(string title, string initialValue, string subtext, Color accent, out Label lblVal)
        {
            Panel card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 10, 10),
                BorderStyle = BorderStyle.FixedSingle
            };

            Panel bar = new Panel { Dock = DockStyle.Left, Width = 5, BackColor = accent };
            card.Controls.Add(bar);

            Label lblT = new Label { Text = title, Font = UITheme.FontSmall, ForeColor = UITheme.TextMuted, Location = new Point(12, 10), AutoSize = true, UseMnemonic = false };
            lblVal = new Label { Text = initialValue, Font = UITheme.FontStatNumber, ForeColor = UITheme.TextDark, Location = new Point(10, 26), AutoSize = true, UseMnemonic = false };
            Label lblS = new Label { Text = subtext, Font = UITheme.FontSmall, ForeColor = Color.FromArgb(100, 116, 139), Location = new Point(12, 68), AutoSize = true, UseMnemonic = false };

            card.Controls.Add(lblT);
            card.Controls.Add(lblVal);
            card.Controls.Add(lblS);

            return card;
        }

        private void LoadStatistics()
        {
            _cachedStats = _dashboardService.GetEmployeeStats();
            if (_cachedStats == null) return;

            _lblValCars.Text = _cachedStats.AvailableCars.ToString();
            _lblValSales.Text = _cachedStats.PendingSales.ToString();
            _lblValServices.Text = _cachedStats.PendingServices.ToString();
            _lblValInventory.Text = _cachedStats.TotalInventory.ToString();
        }

        private void SetupNavButtons()
        {
            pnlQuickNav.Controls.Clear();

            AddActionCard("Manage Car Inventory", "Add new vehicles, update prices and specs, remove stock, and upload car photos.",
                "Vehicle Inventory CRUD", UITheme.PrimaryBlue, (s, e) =>
                {
                    ManageCarsForm f = new ManageCarsForm();
                    f.ShowDialog();
                    LoadStatistics();
                });

            AddActionCard("Sales & Purchase Requests", "Process incoming customer car purchase orders. Confirm sales or cancel requests.",
                "Process Sales Requests", UITheme.SuccessGreen, (s, e) =>
                {
                    SalesForm f = new SalesForm();
                    f.ShowDialog();
                    LoadStatistics();
                });

            AddActionCard("Service & Repair Orders", "Manage workshop service requests, advance status (In Progress, Completed), and review notes.",
                "Process Service Requests", UITheme.WarningAmber, (s, e) =>
                {
                    ServiceRecordsForm f = new ServiceRecordsForm();
                    f.ShowDialog();
                    LoadStatistics();
                });

            AddActionCard("Showroom Car Browser", "Quickly search, filter, and inspect car inventory specifications and pricing.",
                "Browse Showroom", UITheme.AccentCyan, (s, e) =>
                {
                    CarBrowsingForm f = new CarBrowsingForm();
                    f.ShowDialog();
                    LoadStatistics();
                });
        }

        private void AddActionCard(string title, string description, string buttonText, Color themeColor, EventHandler onClick)
        {
            Panel card = new Panel
            {
                Size = new Size(300, 140),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(15)
            };

            Label lblT = new Label { Text = title, Font = UITheme.FontSubtitle, ForeColor = UITheme.TextDark, Location = new Point(14, 12), AutoSize = true, UseMnemonic = false };
            Label lblD = new Label { Text = description, Font = UITheme.FontSmall, ForeColor = UITheme.TextMuted, Location = new Point(14, 38), Size = new Size(270, 42), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, AutoEllipsis = true, UseMnemonic = false };

            Button btn = new Button
            {
                Text = buttonText,
                Location = new Point(14, 88),
                Size = new Size(270, 36),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            UITheme.StylePrimaryButton(btn);
            btn.BackColor = themeColor;
            btn.Click += onClick;

            card.Controls.Add(lblT);
            card.Controls.Add(lblD);
            card.Controls.Add(btn);

            pnlQuickNav.Controls.Add(card);
        }

        private void RelayoutDashboard()
        {
            if (pnlBody == null || pnlMetrics == null || pnlQuickNav == null) return;

            int availableWidth = Math.Max(280, pnlBody.ClientSize.Width - 40);

            pnlMetrics.Location = new Point(20, lblMetricsTitle.Bottom + 10);
            pnlMetrics.Width = availableWidth;

            bool isWide = availableWidth >= 700;
            if (_lastIsWide != isWide)
            {
                _lastIsWide = isWide;
                pnlMetrics.SuspendLayout();
                pnlMetrics.ColumnStyles.Clear();
                pnlMetrics.RowStyles.Clear();

                if (isWide)
                {
                    pnlMetrics.ColumnCount = 4;
                    pnlMetrics.RowCount = 1;
                    pnlMetrics.Height = 100;
                    for (int i = 0; i < 4; i++)
                    {
                        pnlMetrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
                    }
                    pnlMetrics.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

                    pnlMetrics.SetCellPosition(_cardCars, new TableLayoutPanelCellPosition(0, 0));
                    pnlMetrics.SetCellPosition(_cardSales, new TableLayoutPanelCellPosition(1, 0));
                    pnlMetrics.SetCellPosition(_cardServices, new TableLayoutPanelCellPosition(2, 0));
                    pnlMetrics.SetCellPosition(_cardInventory, new TableLayoutPanelCellPosition(3, 0));

                    _cardCars.Margin = new Padding(0, 0, 10, 0);
                    _cardSales.Margin = new Padding(0, 0, 10, 0);
                    _cardServices.Margin = new Padding(0, 0, 10, 0);
                    _cardInventory.Margin = new Padding(0, 0, 0, 0);
                }
                else
                {
                    pnlMetrics.ColumnCount = 2;
                    pnlMetrics.RowCount = 2;
                    pnlMetrics.Height = 210;
                    pnlMetrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
                    pnlMetrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
                    pnlMetrics.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
                    pnlMetrics.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));

                    pnlMetrics.SetCellPosition(_cardCars, new TableLayoutPanelCellPosition(0, 0));
                    pnlMetrics.SetCellPosition(_cardSales, new TableLayoutPanelCellPosition(1, 0));
                    pnlMetrics.SetCellPosition(_cardServices, new TableLayoutPanelCellPosition(0, 1));
                    pnlMetrics.SetCellPosition(_cardInventory, new TableLayoutPanelCellPosition(1, 1));

                    _cardCars.Margin = new Padding(0, 0, 10, 10);
                    _cardSales.Margin = new Padding(0, 0, 0, 10);
                    _cardServices.Margin = new Padding(0, 0, 10, 0);
                    _cardInventory.Margin = new Padding(0, 0, 0, 0);
                }

                pnlMetrics.ResumeLayout(true);
            }

            lblNavTitle.Location = new Point(20, pnlMetrics.Bottom + 20);

            pnlQuickNav.Location = new Point(20, lblNavTitle.Bottom + 10);
            pnlQuickNav.Width = availableWidth;

            int cols = 1;
            if (availableWidth >= 1150) cols = 4;
            else if (availableWidth >= 780) cols = 3;
            else if (availableWidth >= 500) cols = 2;
            else cols = 1;

            int spacing = 14;
            int cardWidth = (availableWidth - ((cols - 1) * spacing) - 4) / cols;

            pnlQuickNav.SuspendLayout();
            for (int i = 0; i < pnlQuickNav.Controls.Count; i++)
            {
                Control c = pnlQuickNav.Controls[i];
                c.Width = cardWidth;
                c.Margin = new Padding(0, 0, ((i + 1) % cols == 0) ? 0 : spacing, spacing);
            }

            int rows = (int)Math.Ceiling((double)pnlQuickNav.Controls.Count / cols);
            pnlQuickNav.Height = rows * (140 + spacing) + 15;
            pnlQuickNav.ResumeLayout(true);
        }
    }
}
