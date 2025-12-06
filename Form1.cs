using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using NotePad_MVP.view;
using NotePad_MVP.Presenter;
using NotePad_MVP.Model;

namespace NotePad_MVP
{
    public partial class Form1 : Form, IViewForm
    {
        // Win32 API for rounded corners
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, int nTopRect, int nRightRect, int nBottomRect,
            int nWidthEllipse, int nHeightEllipse);

        public string cityName
        {
            get => textBox1.Text;    // Read value from textbox
            set => textBox1.Text = value;  // Set value to textbox
        }
        private WeatherFunction weather;
        private Panel welcomePanel; // Welcome panel for Home view
        private Panel errorPanel; // Error panel for displaying messages
        private Label loadingLabel; // Loading indicator label
        private bool isSearchTabActive = true; // Track which tab is currently active
        
        // Drag functionality fields
        private bool isDragging = false;
        private Point dragOffset;
        
        // Window control buttons
        private Button btnClose;
        private Button btnMaximize;
        private Button btnMinimize;
        
        public Form1()
        {
            InitializeComponent();
            
            // Set borderless form
            this.FormBorderStyle = FormBorderStyle.None;
            this.AutoScroll = false; // Disable scroll bars

            
            weather = new WeatherFunction(this);
            CreateLoadingPanel();
            ApplyDarkTheme();
            CreateWindowControls(); // Create custom window control buttons
            SetupDragFunctionality(); // Enable dragging from title bar
            CreateWelcomePanel();
            CreateErrorPanel(); // Initialize error panel

            
            // Ensure welcome panel is visible on startup by hiding the results panels
            tableLayoutPanel3.Visible = false; // Hide the container with panel6 and panel7
            if (welcomePanel != null) 
            {
                welcomePanel.Visible = true;
                welcomePanel.BringToFront();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // Apply rounded corners
            this.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));
        }

        private void ApplyDarkTheme()
        {
            // Modern Color Palette
            // Modern Blue Theme Palette
            Color deepBackground = Color.FromArgb(15, 23, 42);        // #0F172A - Deep Navy
            Color surfaceColor = Color.FromArgb(30, 41, 59);          // #1E293B - Slate Blue
            Color accentPrimary = Color.FromArgb(59, 130, 246);       // #3B82F6 - Vibrant Blue
            Color accentSecondary = Color.FromArgb(96, 165, 250);     // #60A5FA - Lighter Blue
            Color textPrimary = Color.White;                          // #FFFFFF
            Color textSecondary = Color.FromArgb(148, 163, 184);      // #94A3B8
            Color headerGradientStart = Color.FromArgb(30, 58, 138);  // #1E3A8A
            Color headerGradientEnd = Color.FromArgb(59, 130, 246);   // #3B82F6

            this.BackColor = deepBackground;
            this.ForeColor = textPrimary;

            // Apply modern styling to all controls
            foreach (Control c in this.Controls)
            {
                UpdateControlTheme(c, deepBackground, surfaceColor, textPrimary, textSecondary, accentPrimary);
            }
            
            // Apply gradient to header
            if (panel1 != null)
            {
                panel1.Paint += (s, e) => {
                    using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                        panel1.ClientRectangle,
                        headerGradientStart,
                        headerGradientEnd,
                        System.Drawing.Drawing2D.LinearGradientMode.Horizontal))
                    {
                        e.Graphics.FillRectangle(brush, panel1.ClientRectangle);
                    }
                };
            }

            StyleChart();
        }

        private void UpdateControlTheme(Control c, Color bg, Color surface, Color textPrimary, Color textSecondary, Color accent)
        {
            if (c is Label label)
            {
                c.ForeColor = textPrimary;
                // Enhanced font for better readability
                if (label.Font.Size > 12)
                {
                    label.Font = new Font("Segoe UI", label.Font.Size, FontStyle.Bold);
                }
                else
                {
                    label.Font = new Font("Segoe UI", label.Font.Size, label.Font.Style);
                }
            }
            else if (c is TextBox textBox)
            {
                textBox.BackColor = surface;
                textBox.ForeColor = textPrimary;
                textBox.BorderStyle = BorderStyle.FixedSingle;
                textBox.Font = new Font("Segoe UI", 11F, FontStyle.Regular);
                
                // Add rounded corner effect through padding
                textBox.Padding = new Padding(10, 8, 10, 8);
            }
            else if (c is Button button)
            {
                button.BackColor = accent;
                button.ForeColor = Color.White;
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderSize = 0;
                button.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                button.Cursor = Cursors.Hand;
                
                // Add hover effect
                button.MouseEnter += (s, e) => {
                    button.BackColor = Color.FromArgb(37, 99, 235); // Darker blue on hover
                };
                button.MouseLeave += (s, e) => {
                    button.BackColor = accent;
                };
            }
            else if (c is PictureBox)
            {
                c.BackColor = Color.Transparent;
            }
            else if (c is Panel panel)
            {
                // Special handling for different panels
                if (panel == panel1) // Header panel
                {
                    // Gradient applied in ApplyDarkTheme
                }
                else if (panel == panel2) // Sidebar
                {
                    panel.BackColor = Color.FromArgb(22, 27, 34); // Premium Dark Sidebar
                }
                else if (panel == panel6) // Weather info panel
                {
                    panel.BackColor = surface;
                }
                else
                {
                    panel.BackColor = bg;
                }
                
                panel.ForeColor = textPrimary;
                foreach (Control child in panel.Controls)
                {
                    UpdateControlTheme(child, bg, surface, textPrimary, textSecondary, accent);
                }
            }
            else if (c is TableLayoutPanel || c is FlowLayoutPanel)
            {
                c.BackColor = bg;
                c.ForeColor = textPrimary;
                foreach (Control child in c.Controls)
                {
                    UpdateControlTheme(child, bg, surface, textPrimary, textSecondary, accent);
                }
            }
        }

        private void CreateWindowControls()
        {
            int buttonWidth = 46;
            int buttonHeight = 37;
            int rightMargin = 0;
            
            // Close Button (Red on hover)
            btnClose = new Button
            {
                Text = "✕",
                Width = buttonWidth,
                Height = buttonHeight,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12F, FontStyle.Regular),
                Cursor = Cursors.Hand,
                TabStop = false
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 38, 38); // Red
            btnClose.Location = new Point(panel1.Width - buttonWidth - rightMargin, 0);
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.Click += (s, e) => Application.Exit();
            panel1.Controls.Add(btnClose);
            btnClose.BringToFront();

            // Maximize/Restore Button
            btnMaximize = new Button
            {
                Text = "□",
                Width = buttonWidth,
                Height = buttonHeight,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12F, FontStyle.Regular),
                Cursor = Cursors.Hand,
                TabStop = false
            };
            btnMaximize.FlatAppearance.BorderSize = 0;
            btnMaximize.FlatAppearance.MouseOverBackColor = Color.FromArgb(51, 65, 85);
            btnMaximize.Location = new Point(panel1.Width - (buttonWidth * 2) - rightMargin, 0);
            btnMaximize.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnMaximize.Click += BtnMaximize_Click;
            panel1.Controls.Add(btnMaximize);
            btnMaximize.BringToFront();

            // Minimize Button
            btnMinimize = new Button
            {
                Text = "─",
                Width = buttonWidth,
                Height = buttonHeight,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12F, FontStyle.Regular),
                Cursor = Cursors.Hand,
                TabStop = false
            };
            btnMinimize.FlatAppearance.BorderSize = 0;
            btnMinimize.FlatAppearance.MouseOverBackColor = Color.FromArgb(51, 65, 85);
            btnMinimize.Location = new Point(panel1.Width - (buttonWidth * 3) - rightMargin, 0);
            btnMinimize.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnMinimize.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
            panel1.Controls.Add(btnMinimize);
            btnMinimize.BringToFront();
        }

        private void BtnMaximize_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
                btnMaximize.Text = "□";
                // Reapply rounded corners when restoring
                this.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));
            }
            else
            {
                this.WindowState = FormWindowState.Maximized;
                btnMaximize.Text = "❐";
                // Remove rounded corners when maximized
                this.Region = null;
            }
        }

        private void SetupDragFunctionality()
        {
            // Allow dragging the form by clicking on panel1 (title bar)
            panel1.MouseDown += Panel1_MouseDown;
            panel1.MouseMove += Panel1_MouseMove;
            panel1.MouseUp += Panel1_MouseUp;
            
            // Also allow dragging from the label
            labelHeader.MouseDown += Panel1_MouseDown;
            labelHeader.MouseMove += Panel1_MouseMove;
            labelHeader.MouseUp += Panel1_MouseUp;
        }

        private void Panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                dragOffset = e.Location;
            }
        }

        private void Panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point currentScreenPos = PointToScreen(e.Location);
                Location = new Point(currentScreenPos.X - dragOffset.X, currentScreenPos.Y - dragOffset.Y);
            }
        }

        private void Panel1_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            // Responsive font scaling based on form width
            float scaleFactor = this.Width / 1144f; // Base width
            scaleFactor = Math.Max(0.8f, Math.Min(scaleFactor, 1.5f)); // Clamp between 0.8 and 1.5

            // Update welcome panel if visible
            if (welcomePanel != null && welcomePanel.Visible)
            {
                foreach (Control ctrl in welcomePanel.Controls)
                {
                    if (ctrl is Label label)
                    {
                        if (label.Text.Contains("Welcome"))
                        {
                            label.Font = new Font("Segoe UI", 28F * scaleFactor, FontStyle.Bold);
                        }
                        else if (label.Text.Contains("Get real-time"))
                        {
                            label.Font = new Font("Segoe UI", 14F * scaleFactor, FontStyle.Regular);
                        }
                        else if (label.Text == "🌤️")
                        {
                            label.Font = new Font("Segoe UI", 80F * scaleFactor);
                        }
                    }
                }
            }
        }

        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void tableLayoutPanel3_Paint(object sender, PaintEventArgs e)
        {

        }

        public void SetWeatherInfo(WheatherData weatherInfo)
        {
            // Update weather metrics with enhanced styling
            label3.Text = weatherInfo.winddirection.ToString()+"°";
            label3.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            
            label4.Text = weatherInfo.windspeed.ToString()+ " km/h";
            label4.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            
            label5.Text = weatherInfo.temperature.ToString()+ "°C";
            label5.Font = new Font("Segoe UI", 36F, FontStyle.Bold);
            label5.ForeColor = Color.FromArgb(96, 165, 250); // Light blue accent
            
            label9.Text = cityName;
            label9.Font = new Font("Segoe UI", 28F, FontStyle.Bold);
            
            label10.Text = weatherInfo.time;
            label10.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            label10.ForeColor = Color.FromArgb(148, 163, 184);
            
            label11.Text = weatherInfo.WeatherDescription;
            label11.Font = new Font("Segoe UI", 12F, FontStyle.Regular);
            label11.ForeColor = Color.FromArgb(148, 163, 184);
            
            // Update dedicated details panel

            
            // Style metric labels
            label6.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            label6.ForeColor = Color.FromArgb(148, 163, 184);
            label7.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            label7.ForeColor = Color.FromArgb(148, 163, 184);
            label8.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            label8.ForeColor = Color.FromArgb(148, 163, 184);
            
            if (weatherInfo.is_day == 1)
            {
                pictureBox1.Image = Properties.Resources.ChatGPT_Image_Nov_12__2025__09_53_38_AM;
            }
            else
            {
                pictureBox1.Image = Properties.Resources.ChatGPT_Image_Nov_12__2025__09_52_50_AM;
            }

            // Update Forecast View
            UpdateForecastView(weatherInfo);

            // Populate Line Chart (chart1) - Temperature
            StyleChart();

            var series = chart1.Series[0];
            series.Points.Clear();
            
            if (weatherInfo.HourlyTime != null && weatherInfo.HourlyTemperature != null)
            {
                // Limit to next 24 hours for better visibility
                int count = Math.Min(24, Math.Min(weatherInfo.HourlyTime.Count, weatherInfo.HourlyTemperature.Count));
                
                for (int i = 0; i < count; i++)
                {
                    // Parse time to show only hour
                    DateTime time = DateTime.Parse(weatherInfo.HourlyTime[i]);
                    series.Points.AddXY(time.ToString("HH:mm"), weatherInfo.HourlyTemperature[i]);
                }
            }
            
            // Populate Rainfall Chart (chart2) - 7-Day Precipitation
            PopulateRainfallChart(weatherInfo);
        }



        private Control FindControl(Control parent, string name)
        {
            if (parent.Name == name) return parent;
            foreach (Control child in parent.Controls)
            {
                var result = FindControl(child, name);
                if (result != null) return result;
            }
            return null;
        }





        private void StyleChart()
        {
            // --- Modern Chart Styling ---
            chart1.BackColor = Color.FromArgb(30, 41, 59); // Surface color
            
            var chartArea = chart1.ChartAreas[0];
            chartArea.BackColor = Color.FromArgb(30, 41, 59);
            
            // X-Axis Styling - Modern look
            chartArea.AxisX.LabelStyle.ForeColor = Color.FromArgb(148, 163, 184);
            chartArea.AxisX.LabelStyle.Font = new Font("Segoe UI", 8F, FontStyle.Regular);
            chartArea.AxisX.LineColor = Color.FromArgb(71, 85, 105);
            chartArea.AxisX.MajorGrid.LineColor = Color.FromArgb(51, 65, 85);
            chartArea.AxisX.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea.AxisX.TitleForeColor = Color.White;
            chartArea.AxisX.Interval = 4; // Show every 4th hour for cleaner look

            // Y-Axis Styling - Modern look
            chartArea.AxisY.LabelStyle.ForeColor = Color.FromArgb(148, 163, 184);
            chartArea.AxisY.LabelStyle.Font = new Font("Segoe UI", 8F, FontStyle.Regular);
            chartArea.AxisY.LineColor = Color.FromArgb(71, 85, 105);
            chartArea.AxisY.MajorGrid.LineColor = Color.FromArgb(51, 65, 85);
            chartArea.AxisY.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea.AxisY.TitleForeColor = Color.White;

            var series = chart1.Series[0];
            series.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.SplineArea; // Area chart for modern look
            series.Color = Color.FromArgb(180, 59, 130, 246); // Semi-transparent vibrant blue
            series.BorderColor = Color.FromArgb(59, 130, 246); // Solid blue border
            series.BorderWidth = 3;
            series.Name = "Temperature (°C)";
            
            // Add marker points for better visibility
            series.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Circle;
            series.MarkerSize = 6;
            series.MarkerColor = Color.FromArgb(96, 165, 250);
            series.MarkerBorderColor = Color.White;
            series.MarkerBorderWidth = 2;
            
            // Style Legend with modern look - Position at bottom
            if (chart1.Legends.Count > 0)
            {
                chart1.Legends[0].Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Bottom;
                chart1.Legends[0].Alignment = System.Drawing.StringAlignment.Center;
                chart1.Legends[0].BackColor = Color.Transparent;
                chart1.Legends[0].ForeColor = Color.White;
                chart1.Legends[0].Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            }
            
            // Remove chart border for cleaner look
            chart1.BorderlineColor = Color.Transparent;
            chart1.BorderSkin.SkinStyle = System.Windows.Forms.DataVisualization.Charting.BorderSkinStyle.None;
        }

        private void PopulateRainfallChart(WheatherData weatherInfo)
        {
            StyleRainfallChart();
            
            var series = chart2.Series[0];
            series.Points.Clear();
            
            if (weatherInfo.DailyTime != null && weatherInfo.DailyPrecipitationProbability != null)
            {
                int count = Math.Min(weatherInfo.DailyTime.Count, weatherInfo.DailyPrecipitationProbability.Count);
                
                for (int i = 0; i < count; i++)
                {
                    // Parse date to show day name
                    DateTime date = DateTime.Parse(weatherInfo.DailyTime[i]);
                    int rainChance = weatherInfo.DailyPrecipitationProbability[i];
                    
                    var point = series.Points.AddXY(date.ToString("ddd\nMMM dd"), rainChance);
                    
                    // Color code based on rain probability
                    if (rainChance >= 70)
                        series.Points[point].Color = Color.FromArgb(59, 130, 246); // High - Blue
                    else if (rainChance >= 40)
                        series.Points[point].Color = Color.FromArgb(96, 165, 250); // Medium - Light Blue
                    else
                        series.Points[point].Color = Color.FromArgb(148, 163, 184); // Low - Gray
                }
            }
        }

        private void StyleRainfallChart()
        {
            // Modern Rainfall Chart Styling
            chart2.BackColor = Color.FromArgb(30, 41, 59); // Surface color
            
            var chartArea = chart2.ChartAreas[0];
            chartArea.BackColor = Color.FromArgb(30, 41, 59);
            
            // X-Axis Styling
            chartArea.AxisX.LabelStyle.ForeColor = Color.FromArgb(148, 163, 184);
            chartArea.AxisX.LabelStyle.Font = new Font("Segoe UI", 8F, FontStyle.Regular);
            chartArea.AxisX.LineColor = Color.FromArgb(71, 85, 105);
            chartArea.AxisX.MajorGrid.LineColor = Color.FromArgb(51, 65, 85);
            chartArea.AxisX.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea.AxisX.TitleForeColor = Color.White;

            // Y-Axis Styling
            chartArea.AxisY.LabelStyle.ForeColor = Color.FromArgb(148, 163, 184);
            chartArea.AxisY.LabelStyle.Font = new Font("Segoe UI", 8F, FontStyle.Regular);
            chartArea.AxisY.LineColor = Color.FromArgb(71, 85, 105);
            chartArea.AxisY.MajorGrid.LineColor = Color.FromArgb(51, 65, 85);
            chartArea.AxisY.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea.AxisY.TitleForeColor = Color.White;
            chartArea.AxisY.Title = "Rain Probability (%)";
            chartArea.AxisY.TitleFont = new Font("Segoe UI", 9F, FontStyle.Bold);
            chartArea.AxisY.Maximum = 100;
            chartArea.AxisY.Minimum = 0;

            var series = chart2.Series[0];
            series.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column; // Bar chart
            series.Name = "7-Day Rainfall Probability";
            
            // Label styling - show percentage on bars
            series.IsValueShownAsLabel = true;
            series.LabelFormat = "{0}%";
            series.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            series.LabelForeColor = Color.White;
            
            // Legend styling
            if (chart2.Legends.Count > 0)
            {
                chart2.Legends[0].Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Bottom;
                chart2.Legends[0].Alignment = System.Drawing.StringAlignment.Center;
                chart2.Legends[0].BackColor = Color.Transparent;
                chart2.Legends[0].ForeColor = Color.White;
                chart2.Legends[0].Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            }
            
            // Remove chart border
            chart2.BorderlineColor = Color.Transparent;
            chart2.BorderSkin.SkinStyle = System.Windows.Forms.DataVisualization.Charting.BorderSkinStyle.None;
        }



        public void ShowError(string message)
        {
            if (errorPanel != null)
            {
                // Find the message label within the error panel
                foreach (Control ctrl in errorPanel.Controls)
                {
                    if (ctrl is Label lbl && lbl.Name == "lblErrorMessage")
                    {
                        lbl.Text = message;
                        break;
                    }
                }
                errorPanel.Visible = true;
                errorPanel.BringToFront();
            }
            else
            {
                // Fallback if panel creation failed
                MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ===== EVENT HANDLERS =====

        private async void button1_Click(object sender, EventArgs e)
        {
            // Hide welcome panel when search starts
            welcomePanel.Visible = false;
            
            await weather.FetchAndDisplayWeatherAsync();
            
            // Show results based on active tab
            if (isSearchTabActive)
            {
                // In search tab - show search results
                tableLayoutPanel3.Visible = true;
            }
            else
            {
                // In forecast tab - keep forecast visible, load search data in background
                // Search panel data is already populated by SetWeatherInfo
                // Keep forecast panel visible
                var forecastPanel = Controls.Find("panelForecast", true).FirstOrDefault() as Panel;
                if (forecastPanel != null)
                {
                    forecastPanel.Visible = true;
                    forecastPanel.BringToFront();
                }
            }
        }

        // ===== NAVIGATION METHODS =====
        
        private void CreateWelcomePanel()
        {
            welcomePanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 41, 59), // Surface color
                Visible = true
            };

            // Welcome title - centered
            Label welcomeTitle = new Label
            {
                Text = "This is Weather Prediction App",
                Font = new Font("Segoe UI", 28F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Anchor = AnchorStyles.None
            };

            // Welcome description - centered
            Label welcomeDesc = new Label
            {
                Text = "Enter a city name and click 'Search' to get weather information",
                Font = new Font("Segoe UI", 14F, FontStyle.Regular),
                ForeColor = Color.FromArgb(148, 163, 184),
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter,
                Anchor = AnchorStyles.None
            };

            // Weather icon/emoji - centered
            Label weatherIcon = new Label
            {
                Text = "🌤️",
                Font = new Font("Segoe UI", 80F),
                AutoSize = true,
                Anchor = AnchorStyles.None
            };

            welcomePanel.Controls.Add(welcomeTitle);
            welcomePanel.Controls.Add(welcomeDesc);
            welcomePanel.Controls.Add(weatherIcon);

            // Center elements when panel resizes
            welcomePanel.Resize += (s, e) => {
                int centerX = welcomePanel.Width / 2;
                int centerY = welcomePanel.Height / 2;
                
                welcomeTitle.Location = new Point(centerX - welcomeTitle.Width / 2, centerY - 120);
                welcomeDesc.Location = new Point(centerX - welcomeDesc.Width / 2, centerY - 50);
                weatherIcon.Location = new Point(centerX - weatherIcon.Width / 2, centerY + 30);
            };

            // Add to tableLayoutPanel2 (Row 1) to occupy results area while keeping search bar visible
            tableLayoutPanel2.Controls.Add(welcomePanel, 0, 1);
            welcomePanel.BringToFront();
        }

        private void CreateErrorPanel()
        {
            errorPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 41, 59), // Surface color
                Visible = false,
                Name = "panelError"
            };

            // Error Title
            Label errorTitle = new Label
            {
                Text = "Error",
                Font = new Font("Segoe UI", 28F, FontStyle.Bold),
                ForeColor = Color.FromArgb(239, 68, 68), // Red color
                AutoSize = true,
                Anchor = AnchorStyles.None
            };

            // Error Message
            Label errorMessage = new Label
            {
                Name = "lblErrorMessage",
                Text = "An unknown error occurred.",
                Font = new Font("Segoe UI", 14F, FontStyle.Regular),
                ForeColor = Color.White,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter,
                Anchor = AnchorStyles.None,
                MaximumSize = new Size(600, 0) // Limit width to prevent overflow
            };

            // Close Button
            Button btnCloseError = new Button
            {
                Text = "Close",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(59, 130, 246), // Blue accent
                FlatStyle = FlatStyle.Flat,
                AutoSize = true,
                Padding = new Padding(10, 5, 10, 5),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.None
            };
            btnCloseError.FlatAppearance.BorderSize = 0;
            btnCloseError.Click += (s, e) => {
                errorPanel.Visible = false;
                
                // Logic to check if we have valid data (City Name is set and not default)
                // This prevents showing "unorganized labels" if no search has been performed yet
                bool hasData = !string.IsNullOrEmpty(label9.Text) && label9.Text != "label9" && label9.Text != "City Name";

                if (hasData)
                {
                    // If we had valid data before the error, show it
                    tableLayoutPanel3.Visible = true;
                    if (welcomePanel != null) welcomePanel.Visible = false;
                }
                else
                {
                    // If no valid data (clean state or only default labels), show home/welcome panel
                    if (welcomePanel != null) welcomePanel.Visible = true;
                    tableLayoutPanel3.Visible = false;
                }
            };

            // Error Icon (optional)
            Label errorIcon = new Label
            {
                Text = "⚠️",
                Font = new Font("Segoe UI", 60F),
                AutoSize = true,
                Anchor = AnchorStyles.None,
                ForeColor = Color.FromArgb(239, 68, 68)
            };

            errorPanel.Controls.Add(errorTitle);
            errorPanel.Controls.Add(errorMessage);
            errorPanel.Controls.Add(btnCloseError);
            errorPanel.Controls.Add(errorIcon);

            // Center elements
            errorPanel.Resize += (s, e) => {
                int centerX = errorPanel.Width / 2;
                int centerY = errorPanel.Height / 2;

                errorIcon.Location = new Point(centerX - errorIcon.Width / 2, centerY - 150);
                errorTitle.Location = new Point(centerX - errorTitle.Width / 2, centerY - 50);
                errorMessage.Location = new Point(centerX - errorMessage.Width / 2, centerY + 20);
                btnCloseError.Location = new Point(centerX - btnCloseError.Width / 2, centerY + 100);
            };

            // Add to the main container that covers everything, e.g., tableLayoutPanel2 or panel3
            // Using panel3 or directly to controls if it needs to overlay everything
            // Let's add it to where welcomePanel is added or higher
            if (panel3 != null)
            {
                panel3.Controls.Add(errorPanel);
            }
            else
            {
                this.Controls.Add(errorPanel);
            }
            
            errorPanel.BringToFront();
            
            // Initial positioning
            errorIcon.Location = new Point((errorPanel.Width - errorIcon.Width) / 2, (errorPanel.Height - errorIcon.Height) / 2 - 150);
            errorTitle.Location = new Point((errorPanel.Width - errorTitle.Width) / 2, (errorPanel.Height - errorTitle.Height) / 2 - 50);
            errorMessage.Location = new Point((errorPanel.Width - errorMessage.Width) / 2, (errorPanel.Height - errorMessage.Height) / 2 + 20);
            btnCloseError.Location = new Point((errorPanel.Width - btnCloseError.Width) / 2, (errorPanel.Height - btnCloseError.Height) / 2 + 100);
        }


        private Panel loadingPanel;

        public void ShowLoading()
        {
            if (loadingPanel != null)
            {
                loadingPanel.Visible = true;
                loadingPanel.BringToFront();
            }
        }

        public void HideLoading()
        {
            if (loadingPanel != null)
            {
                loadingPanel.Visible = false;
            }
        }

        private void CreateLoadingPanel()
        {
            loadingPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 41, 59), // Match background
                Visible = false
            };

            Label lbl = new Label
            {
                Text = "Loading...",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(96, 165, 250),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            // Center label
            loadingPanel.Resize += (s, e) => {
                lbl.Location = new Point(
                    (loadingPanel.Width - lbl.Width) / 2,
                    (loadingPanel.Height - lbl.Height) / 2
                );
            };

            loadingPanel.Controls.Add(lbl);
            
            // Add to panel3 to cover everything
            panel3.Controls.Add(loadingPanel);
            loadingPanel.BringToFront();
            
            // Trigger resize to center label initially
            lbl.Location = new Point(
                (panel3.Width - lbl.Width) / 2,
                (panel3.Height - lbl.Height) / 2
            );
        }

        private void ShowSearchView()
        {
            // Set active tab
            isSearchTabActive = true;
            
            // Show search bar
            tableLayoutPanel4.Visible = true;
            
            // Check if we have valid data (City Name is set and not default)
            bool hasData = !string.IsNullOrEmpty(label9.Text) && label9.Text != "label9" && label9.Text != "City Name";

            if (hasData)
            {
                // Restore results view
                tableLayoutPanel3.Visible = true;
                if (welcomePanel != null) welcomePanel.Visible = false;
            }
            else
            {
                // Show welcome panel if no data
                tableLayoutPanel3.Visible = false;
                if (welcomePanel != null) welcomePanel.Visible = true;
            }
            
            // Hide forecast panel
            var forecastPanel = Controls.Find("panelForecast", true).FirstOrDefault() as Panel;
            if (forecastPanel != null) forecastPanel.Visible = false;
            
            // Update sidebar selection
            panel5.BackColor = Color.FromArgb(59, 130, 246); // Active - Vibrant Blue
            panel8.BackColor = Color.Transparent; // Inactive - Blends with sidebar
        }

        private void panel8_Click(object sender, EventArgs e)
        {
            ShowForecastView();
        }

        private void ShowForecastView()
        {
            // Set active tab
            isSearchTabActive = false;
            
            // Check if we have valid data (City Name is set and not default)
            bool hasData = !string.IsNullOrEmpty(label9.Text) && label9.Text != "label9" && label9.Text != "City Name";
            
            if (!hasData)
            {
                // No data - show welcome panel but keep Forecast tab active
                if (welcomePanel != null)
                {
                    welcomePanel.Visible = true;
                    welcomePanel.BringToFront();
                }
                
                // Ensure search bar is visible so user can search
                tableLayoutPanel4.Visible = true;
                
                // Hide forecast panel if exists
                var forecastPanel = Controls.Find("panelForecast", true).FirstOrDefault() as Panel;
                if (forecastPanel != null) forecastPanel.Visible = false;
                
                // Hide results
                tableLayoutPanel3.Visible = false;
            }
            else
            {
                // Has data - show forecast panel
                if (welcomePanel != null) welcomePanel.Visible = false;
                tableLayoutPanel3.Visible = false; // Hide current weather and chart
                
                // Hide details panel if exists
                var detailsPanel = Controls.Find("panelWeatherDetails", true).FirstOrDefault() as Panel;
                if (detailsPanel != null) detailsPanel.Visible = false;

                // Show forecast panel
                if (Controls.Find("panelForecast", true).Length == 0)
                {
                    CreateForecastPanel();
                }
                var forecastPanel = Controls.Find("panelForecast", true).FirstOrDefault() as Panel;
                if (forecastPanel != null) 
                {
                    forecastPanel.Visible = true;
                    forecastPanel.BringToFront();
                }
            }
            
            // Update sidebar selection - Include logic here to ensure it runs
            panel5.BackColor = Color.Transparent; // Inactive
            panel8.BackColor = Color.FromArgb(59, 130, 246); // Active - Vibrant Blue
        }

        private void CreateForecastPanel()
        {
            // Use standard Panel to avoid MetroPanel artifacts
            Panel panelForecast = new Panel
            {
                Name = "panelForecast",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(15, 23, 42),
                AutoScroll = true,
                Visible = false // Initially hidden, will be shown when user clicks Forecast tab
            };
            
            // Add title
            Label title = new Label
            {
                Text = "7-Day Forecast",
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 30)
            };
            panelForecast.Controls.Add(title);

            // City Name Label (Context)
            Label cityLabel = new Label
            {
                Name = "forecastCityLabel",
                Text = "", 
                Font = new Font("Segoe UI", 16F, FontStyle.Regular),
                ForeColor = Color.FromArgb(148, 163, 184), // Secondary text color
                AutoSize = true,
                Location = new Point(20, 75)
            };
            panelForecast.Controls.Add(cityLabel);

            // Container for cards
            FlowLayoutPanel cardsContainer = new FlowLayoutPanel
            {
                Name = "cardsContainer",
                Location = new Point(20, 120),
                Width = panelForecast.Width - 40, // Full width minus margins
                Height = 240,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                AutoSize = false,
                WrapContents = false, // Keep all cards in one row
                AutoScroll = false, // No scrolling, fit all cards
                FlowDirection = FlowDirection.LeftToRight
            };
            panelForecast.Controls.Add(cardsContainer);

            // Details Section Title
            Label detailsTitle = new Label
            {
                Text = "Current Details",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 380) // Adjusted position
            };
            panelForecast.Controls.Add(detailsTitle);

            // Container for details
            FlowLayoutPanel detailsContainer = new FlowLayoutPanel
            {
                Name = "detailsContainer",
                Location = new Point(20, 430),
                Width = panelForecast.Width - 40, // Full width minus margins
                Height = 140,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                AutoSize = false, // Don't auto-size
                WrapContents = false, // Keep all cards in one row
                FlowDirection = FlowDirection.LeftToRight
            };
            panelForecast.Controls.Add(detailsContainer);

            // Add bottom spacer to ensure full scrolling
            Label spacer = new Label
            {
                Text = "",
                Height = 50,
                Width = 860,
                Location = new Point(60, 780),
                BackColor = Color.Transparent
            };
            panelForecast.Controls.Add(spacer);

            // Add resize handler to make containers responsive
            panelForecast.Resize += (s, e) =>
            {
                var forecastPnl = s as Panel;
                if (forecastPnl == null) return;
                
                // Update cardsContainer width
                var cards = forecastPnl.Controls.Find("cardsContainer", false).FirstOrDefault() as FlowLayoutPanel;
                if (cards != null)
                {
                    cards.Width = forecastPnl.Width - 40;
                    
                    // Recalculate card widths
                    int cardWidth = (cards.Width / 7) - 10;
                    foreach (Control ctrl in cards.Controls)
                    {
                        if (ctrl is Panel card)
                        {
                            card.Width = cardWidth;
                            // Reposition icon relative to new width
                            foreach (Control c in card.Controls)
                            {
                                if (c is Label lbl && lbl.Font.Size == 24F) // Icon label
                                {
                                    lbl.Location = new Point(cardWidth - 40, lbl.Location.Y);
                                }
                            }
                        }
                    }
                }
                
                // Update detailsContainer width
                var details = forecastPnl.Controls.Find("detailsContainer", false).FirstOrDefault() as FlowLayoutPanel;
                if (details != null)
                {
                    details.Width = forecastPnl.Width - 40;
                    
                    // Recalculate detail card widths
                    int detailCardWidth = (details.Width / 4) - 20;
                    foreach (Control ctrl in details.Controls)
                    {
                        if (ctrl is Panel card)
                        {
                            card.Width = detailCardWidth;
                            // Reposition icon relative to new width
                            foreach (Control c in card.Controls)
                            {
                                if (c is Label lbl && lbl.Font.Size == 20F) // Icon label
                                {
                                    lbl.Location = new Point(detailCardWidth - 40, lbl.Location.Y);
                                }
                            }
                        }
                    }
                }
            };

            // Add to tableLayoutPanel2 (Row 1) to keep search bar visible
            tableLayoutPanel2.Controls.Add(panelForecast, 0, 1);
            panelForecast.BringToFront();
        }

        private void UpdateForecastView(WheatherData weatherInfo)
        {
            // Create forecast panel if it doesn't exist
            if (Controls.Find("panelForecast", true).Length == 0)
            {
                CreateForecastPanel();
            }
            
            var forecastPanel = Controls.Find("panelForecast", true).FirstOrDefault() as Panel;
            if (forecastPanel == null) return;
            
            var container = forecastPanel.Controls.Find("cardsContainer", true).FirstOrDefault() as FlowLayoutPanel;
            
            // Update City Label
            var cityLabel = forecastPanel.Controls.Find("forecastCityLabel", true).FirstOrDefault() as Label;
            if (cityLabel != null) cityLabel.Text = cityName;

            if (container == null) return;
            container.Controls.Clear();

            if (weatherInfo.DailyTime != null)
            {
                // Calculate card width: divide container width by 7 cards, minus margins
                int cardWidth = (container.Width / 7) - 10; // 7 cards, 10px total margin
                
                for (int i = 0; i < weatherInfo.DailyTime.Count; i++)
                {
                    Panel card = new Panel
                    {
                        Size = new Size(cardWidth, 220),
                        BackColor = Color.FromArgb(30, 41, 59),
                        Margin = new Padding(3, 5, 3, 5) // Reduced margins
                    };

                    // Date
                    DateTime date = DateTime.Parse(weatherInfo.DailyTime[i]);
                    Label lblDate = new Label
                    {
                        Text = date.ToString("ddd\nMMM dd"),
                        Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                        ForeColor = Color.White,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Dock = DockStyle.Top,
                        Height = 50
                    };
                    card.Controls.Add(lblDate);

                    // Icon (simplified based on weather code)
                    Label lblIcon = new Label
                    {
                        Text = GetWeatherEmoji(weatherInfo.DailyWeatherCode?[i] ?? 0),
                        Font = new Font("Segoe UI", 24F),
                        TextAlign = ContentAlignment.MiddleCenter,
                        Dock = DockStyle.Top,
                        Height = 60
                    };
                    card.Controls.Add(lblIcon);
                    lblIcon.BringToFront();

                    // Temp Max/Min
                    Label lblTemp = new Label
                    {
                        Text = $"{weatherInfo.DailyTempMax?[i]:F0}° / {weatherInfo.DailyTempMin?[i]:F0}°",
                        Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                        ForeColor = Color.White,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Dock = DockStyle.Top,
                        Height = 40
                    };
                    card.Controls.Add(lblTemp);
                    lblTemp.BringToFront();
                    
                    // Rain chance
                    Label lblRain = new Label
                    {
                        Text = $"💧 {weatherInfo.DailyPrecipitationProbability?[i]}%",
                        Font = new Font("Segoe UI", 10F),
                        ForeColor = Color.FromArgb(148, 163, 184),
                        TextAlign = ContentAlignment.MiddleCenter,
                        Dock = DockStyle.Top,
                        Height = 30
                    };
                    card.Controls.Add(lblRain);
                    lblRain.BringToFront();

                    container.Controls.Add(card);
                }
            }

            // --- Populate Details Section ---
            var detailsContainer = forecastPanel.Controls.Find("detailsContainer", true).FirstOrDefault() as FlowLayoutPanel;
            if (detailsContainer != null)
            {
                detailsContainer.Controls.Clear();

                // Feels Like
                if (weatherInfo.HourlyFeelsLike != null && weatherInfo.HourlyFeelsLike.Count > 0)
                {
                    AddForecastDetailCard(detailsContainer, "Feels Like", $"{weatherInfo.HourlyFeelsLike[0]:F1}°C", "🌡️");
                }

                // Humidity
                if (weatherInfo.HourlyHumidity != null && weatherInfo.HourlyHumidity.Count > 0)
                {
                    AddForecastDetailCard(detailsContainer, "Humidity", $"{weatherInfo.HourlyHumidity[0]}%", "💧");
                }

                // UV Index
                if (weatherInfo.HourlyUVIndex != null && weatherInfo.HourlyUVIndex.Count > 0)
                {
                    double uvIndex = weatherInfo.HourlyUVIndex[0];
                    string uvWarning = uvIndex < 3 ? "Low" : uvIndex < 6 ? "Moderate" : uvIndex < 8 ? "High" : uvIndex < 11 ? "Very High" : "Extreme";
                    AddForecastDetailCard(detailsContainer, "UV Index", $"{uvIndex:F1} ({uvWarning})", "☀️");
                }

                // Sunrise/Sunset
                if (weatherInfo.DailySunrise != null && weatherInfo.DailySunrise.Count > 0)
                {
                    try
                    {
                        DateTime sunrise = DateTime.Parse(weatherInfo.DailySunrise[0]);
                        DateTime sunset = DateTime.Parse(weatherInfo.DailySunset[0]);
                        AddForecastDetailCard(detailsContainer, "Sun Times", $"{sunrise:hh:mm tt}\n{sunset:hh:mm tt}", "🌅");
                    }
                    catch { }
                }
            }
        }

        private void AddForecastDetailCard(FlowLayoutPanel container, string title, string value, string icon)
        {
            // Calculate card width: 25% of container width minus margins
            int cardWidth = (container.Width / 4) - 20; // 4 cards, 20px total margin (10px each side)
            
            Panel card = new Panel
            {
                Size = new Size(cardWidth, 120),
                BackColor = Color.FromArgb(30, 41, 59),
                Margin = new Padding(5, 5, 5, 5) // Reduced margins for better fit
            };

            Label lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(148, 163, 184),
                Location = new Point(10, 10),
                AutoSize = true
            };
            card.Controls.Add(lblTitle);

            Label lblIcon = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI", 20F),
                Location = new Point(cardWidth - 40, 10), // Position relative to card width
                AutoSize = true
            };
            card.Controls.Add(lblIcon);

            Label lblValue = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(10, 50),
                AutoSize = true,
                MaximumSize = new Size(cardWidth - 20, 0) // Wrap text if needed
            };
            card.Controls.Add(lblValue);

            container.Controls.Add(card);
        }

        private string GetWeatherEmoji(int code)
        {
            if (code == 0) return "☀️";
            if (code <= 3) return "⛅";
            if (code <= 48) return "🌫️";
            if (code <= 67) return "🌧️";
            if (code <= 77) return "❄️";
            if (code <= 82) return "🌧️";
            if (code <= 86) return "❄️";
            if (code <= 99) return "⛈️";
            return "❓";
        }

        private void panel5_Click(object sender, EventArgs e)
        {
            ShowSearchView();
        }

    }
}
