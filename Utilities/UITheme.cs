using System;
using System.Drawing;
using System.Windows.Forms;

namespace CarStoreServiceManagement.Utilities
{
    /// <summary>
    /// Centralized UI theme and visual styling engine.
    /// Provides consistent fonts, palettes, card designs, and control stylers.
    /// </summary>
    public static class UITheme
    {
        // Core Palette
        public static readonly Color PrimaryDark = Color.FromArgb(15, 23, 42);     // Slate 900
        public static readonly Color HeaderBg = Color.FromArgb(30, 41, 59);       // Slate 800
        public static readonly Color PrimaryBlue = Color.FromArgb(37, 99, 235);    // Blue 600
        public static readonly Color PrimaryHover = Color.FromArgb(29, 78, 216);   // Blue 700
        public static readonly Color AccentCyan = Color.FromArgb(2, 132, 199);     // Sky 600
        public static readonly Color FormBg = Color.FromArgb(248, 250, 252);      // Slate 50
        public static readonly Color CardBg = Color.White;
        public static readonly Color TextDark = Color.FromArgb(15, 23, 42);
        public static readonly Color TextMuted = Color.FromArgb(100, 116, 139);   // Slate 500
        public static readonly Color BorderColor = Color.FromArgb(203, 213, 225); // Slate 300

        // Status Colors
        public static readonly Color SuccessGreen = Color.FromArgb(16, 185, 129);  // Emerald 500
        public static readonly Color SuccessDark = Color.FromArgb(5, 150, 105);
        public static readonly Color WarningAmber = Color.FromArgb(245, 158, 11);  // Amber 500
        public static readonly Color DangerRed = Color.FromArgb(239, 68, 68);      // Red 500
        public static readonly Color DangerDark = Color.FromArgb(220, 38, 38);
        public static readonly Color SecondaryGray = Color.FromArgb(107, 114, 128); // Gray 500

        // Typography
        public static readonly Font FontTitle = new Font("Segoe UI", 16f, FontStyle.Bold);
        public static readonly Font FontSubtitle = new Font("Segoe UI", 11.5f, FontStyle.Bold);
        public static readonly Font FontHeader = new Font("Segoe UI", 10.5f, FontStyle.Bold);
        public static readonly Font FontRegular = new Font("Segoe UI", 9.5f, FontStyle.Regular);
        public static readonly Font FontRegularBold = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        public static readonly Font FontSmall = new Font("Segoe UI", 8.5f, FontStyle.Regular);
        public static readonly Font FontStatNumber = new Font("Segoe UI", 20f, FontStyle.Bold);

        /// <summary>
        /// Styles a standard primary action button (e.g. Save, Add, Search).
        /// </summary>
        public static void StylePrimaryButton(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = PrimaryBlue;
            btn.ForeColor = Color.White;
            btn.Font = FontRegularBold;
            btn.Cursor = Cursors.Hand;
            btn.Height = 36;
        }

        /// <summary>
        /// Styles a success action button (e.g. Confirm Sale, Book, Approve).
        /// </summary>
        public static void StyleSuccessButton(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = SuccessGreen;
            btn.ForeColor = Color.White;
            btn.Font = FontRegularBold;
            btn.Cursor = Cursors.Hand;
            btn.Height = 36;
        }

        /// <summary>
        /// Styles a danger action button (e.g. Delete, Cancel, Exit).
        /// </summary>
        public static void StyleDangerButton(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = DangerRed;
            btn.ForeColor = Color.White;
            btn.Font = FontRegularBold;
            btn.Cursor = Cursors.Hand;
            btn.Height = 36;
        }

        /// <summary>
        /// Styles a warning action button (e.g. Toggle Status, Change Password).
        /// </summary>
        public static void StyleWarningButton(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = WarningAmber;
            btn.ForeColor = Color.White;
            btn.Font = FontRegularBold;
            btn.Cursor = Cursors.Hand;
            btn.Height = 36;
        }

        /// <summary>
        /// Styles a secondary action button (e.g. Clear, Reset, Refresh).
        /// </summary>
        public static void StyleSecondaryButton(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = BorderColor;
            btn.FlatAppearance.BorderSize = 1;
            btn.BackColor = Color.White;
            btn.ForeColor = TextDark;
            btn.Font = FontRegular;
            btn.Cursor = Cursors.Hand;
            btn.Height = 36;
        }

        /// <summary>
        /// Creates a small secondary button for the filter/action toolbars.
        /// </summary>
        public static Button CreateToolbarButton(string text, int width, EventHandler onClick)
        {
            var button = new Button { Text = text, Width = width, Margin = new Padding(0, 3, 8, 6) };
            StyleSecondaryButton(button);
            button.Height = 30;
            button.Click += onClick;
            return button;
        }

        /// <summary>
        /// Styles DataGridViews for high readability, alternating row highlights, and modern clean aesthetics.
        /// </summary>
        public static void StyleDataGridView(DataGridView dgv)
        {
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.FixedSingle;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.EnableHeadersVisualStyles = false;
            dgv.GridColor = Color.FromArgb(241, 245, 249);
            dgv.RowHeadersVisible = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.ReadOnly = true;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.RowTemplate.Height = 32;

            // Header Style
            dgv.ColumnHeadersDefaultCellStyle.BackColor = HeaderBg;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = FontRegularBold;
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 4, 6, 4);
            dgv.ColumnHeadersHeight = 38;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgv.ColumnAdded += (s, e) =>
            {
                e.Column.FillWeight = GetColumnWeight(e.Column.Name);

                if (e.Column.Name.EndsWith("Status", StringComparison.Ordinal))
                {
                    e.Column.MinimumWidth = 105; // "In Progress" and "Completed" must stay readable
                }

                if (e.Column.ValueType == typeof(DateTime))
                {
                    e.Column.DefaultCellStyle.Format = "dd MMM yyyy";
                    e.Column.MinimumWidth = 100;
                }
            };

            // Row Style
            dgv.DefaultCellStyle.BackColor = Color.White;
            dgv.DefaultCellStyle.ForeColor = TextDark;
            dgv.DefaultCellStyle.Font = FontRegular;
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254); // Blue 100
            dgv.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 58, 138);   // Blue 900
            dgv.DefaultCellStyle.Padding = new Padding(4);

            // Alternating Row Style
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgv.AlternatingRowsDefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 58, 138);
        }

        /// <summary>
        /// Gives long text columns (names, emails, vehicles) more room than ID columns.
        /// </summary>
        private static float GetColumnWeight(string columnName)
        {
            if (columnName.EndsWith("ID", StringComparison.Ordinal))
                return 55f;

            if (columnName == "ServiceName")
                return 220f;

            if (columnName.Contains("Name") || columnName.Contains("Email") || columnName.Contains("Details") ||
                columnName == "Vehicle" || columnName == "Service" || columnName == "Notes")
                return 160f;

            return 100f;
        }

        /// <summary>
        /// Creates a top banner/header for standard child forms.
        /// </summary>
        public static Panel CreateTopBanner(Form form, string title, string subtitle, EventHandler onBack = null, string backText = "← Back")
        {
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = HeaderBg,
                Padding = new Padding(20, 10, 20, 10)
            };

            Label lblTitle = new Label
            {
                Text = title,
                Font = FontSubtitle,
                ForeColor = Color.White,
                AutoSize = true,
                UseMnemonic = false,
                AutoEllipsis = true,
                Location = new Point(20, 12),
                MaximumSize = new Size(Math.Max(100, headerPanel.Width - 140), 0)
            };
            headerPanel.Controls.Add(lblTitle);

            Label lblSub = null;
            if (!string.IsNullOrEmpty(subtitle))
            {
                lblSub = new Label
                {
                    Text = subtitle,
                    Font = FontSmall,
                    ForeColor = Color.FromArgb(148, 163, 184),
                    AutoSize = true,
                    UseMnemonic = false,
                    AutoEllipsis = true,
                    Location = new Point(20, 38),
                    MaximumSize = new Size(Math.Max(100, headerPanel.Width - 140), 0)
                };
                headerPanel.Controls.Add(lblSub);
            }

            if (onBack != null)
            {
                Button btnBack = new Button
                {
                    Text = backText,
                    Font = FontRegularBold,
                    ForeColor = Color.White,
                    BackColor = Color.FromArgb(51, 65, 85),
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand,
                    Size = new Size(95, 32),
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    Location = new Point(headerPanel.Width - 115, 18)
                };
                btnBack.FlatAppearance.BorderSize = 0;
                btnBack.Click += onBack;
                headerPanel.Controls.Add(btnBack);

                headerPanel.Resize += (s, e) =>
                {
                    btnBack.Location = new Point(headerPanel.Width - 115, 18);
                    lblTitle.MaximumSize = new Size(Math.Max(100, headerPanel.Width - 140), 0);
                    if (lblSub != null)
                    {
                        lblSub.MaximumSize = new Size(Math.Max(100, headerPanel.Width - 140), 0);
                    }
                };
            }

            return headerPanel;
        }

        /// <summary>
        /// Formats a badge cell according to status text (Available, Sold, Pending, Confirmed, etc.)
        /// </summary>
        public static void ColorizeStatusCell(DataGridViewCellFormattingEventArgs e, string status)
        {
            if (string.IsNullOrEmpty(status)) return;

            string s = status.Trim().ToLowerInvariant();
            if (s == "available" || s == "completed" || s == "confirmed" || s == "active")
            {
                e.CellStyle.ForeColor = SuccessDark;
                e.CellStyle.Font = FontRegularBold;
            }
            else if (s == "pending" || s == "in progress" || s == "reserved")
            {
                e.CellStyle.ForeColor = WarningAmber;
                e.CellStyle.Font = FontRegularBold;
            }
            else if (s == "sold" || s == "cancelled" || s == "inactive" || s == "unavailable")
            {
                e.CellStyle.ForeColor = DangerDark;
                e.CellStyle.Font = FontRegularBold;
            }
        }
    }
}
