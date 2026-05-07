using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms; // Thư viện quan trọng cho giao diện
using Oracle.ManagedDataAccess.Client;
using HospitalApp.Services;

namespace HospitalApp.Forms
{
    public class LoginForm : Form
    {
        // Khai báo các thành phần giao diện
        private TextBox txtUser = null!;
        private TextBox txtPass = null!;
        private TextBox txtDataSource = null!;
        private Button btnLogin = null!;
        private Button btnShowPass = null!;
        private CheckBox chkRemember = null!;
        private Label lblError = null!;
        private Label lblTitle = null!;
        private Panel bg = null!;
        private Panel card = null!;

        // Sử dụng đầy đủ tên namespace để tránh lỗi "Ambiguous reference" cho Timer
        private System.Windows.Forms.Timer animTimer = null!;
        private float tick = 0;
        private bool isPassVisible = false;
        private readonly string rememberFilePath;

        public LoginForm()
        {
            // Tối ưu hóa việc vẽ giao diện, chống giật (flicker)
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | 
                          ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();

            this.Size = new Size(1100, 750);
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(10, 15, 28);
            rememberFilePath = Path.Combine(Application.UserAppDataPath, "remembered_user.txt");

            InitUI();
            LoadRememberedUser();
            StartAnimations();
        }

        private void InitUI()
        {
            // Nền gradient động
            bg = new DoubleBufferedPanel { Dock = DockStyle.Fill };
            bg.Paint += DrawCinematicBackground;
            bg.MouseDown += (s, e) => { ReleaseCapture(); SendMessage(Handle, 0xA1, 0x2, 0); };
            this.Controls.Add(bg);

            // Thẻ Card chứa form đăng nhập
            card = new DoubleBufferedPanel { 
                Size = new Size(460, 700), 
                BackColor = Color.FromArgb(30, 40, 60) 
            };
            card.Paint += (s, e) => DrawCardBorder(e, 35);
            bg.Controls.Add(card);

            // Tiêu đề to rõ
            lblTitle = new Label {
                Text = "HOSPITAL SYSTEM",
                Font = new Font("Segoe UI", 26, FontStyle.Bold),
                ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter,
                Height = 80, Width = 460, Top = 40, BackColor = Color.Transparent
            };

            // --- PHẦN USERNAME ---
            CreateHeaderLabel("Username", 150);
            txtUser = CreateInputBox("👤 Enter Username", 190);

            // --- PHẦN PASSWORD ---
            CreateHeaderLabel("Password", 280);
            txtPass = CreateInputBox("🔒 Enter Password", 320);
            txtPass.Width = 310; // Thu ngắn một chút để đặt nút con mắt
            SetPasswordPlaceholderState(true);

            btnShowPass = new Button {
                Text = "👁", // Biểu tượng con mắt
                Size = new Size(45, 40), Left = 360, Top = 320,
                FlatStyle = FlatStyle.Flat, ForeColor = Color.Gray,
                BackColor = Color.FromArgb(30, 41, 59), Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 14)
            };
            btnShowPass.FlatAppearance.BorderSize = 0;
            btnShowPass.Click += TogglePasswordVisibility;

            // --- GHI NHỚ MẬT KHẨU ---
            chkRemember = new CheckBox {
                Text = "Remember me", 
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 11),
                Left = 50, Top = 475, AutoSize = true, Cursor = Cursors.Hand
            };

            // --- PHẦN DATASOURCE (Oracle) ---
            CreateHeaderLabel("Oracle DataSource", 390);
            txtDataSource = CreateDataSourceInputBox(DBConnection.GetEffectiveDataSource(), 430);

            // Thông báo lỗi
            lblError = new Label {
                ForeColor = Color.FromArgb(255, 80, 100), TextAlign = ContentAlignment.MiddleCenter,
                Height = 35, Width = 360, Left = 50, Top = 515,
                Font = new Font("Segoe UI", 10, FontStyle.Italic), BackColor = Color.Transparent
            };

            // Nút đăng nhập hiện đại
            btnLogin = new ModernButton {
                Text = "SIGN IN", Width = 360, Height = 60, Left = 50, Top = 565,
                BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 14, FontStyle.Bold), Cursor = Cursors.Hand
            };
            btnLogin.Click += BtnLogin_Click;

            card.Controls.AddRange(new Control[] { lblTitle, lblError, btnLogin, btnShowPass, chkRemember });
            CenterCard();

            // Nút đóng ứng dụng
            Button btnClose = new Button {
                Text = "✕", Size = new Size(45, 35), Location = new Point(1040, 15),
                FlatStyle = FlatStyle.Flat, ForeColor = Color.White, BackColor = Color.Transparent
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => Application.Exit();
            bg.Controls.Add(btnClose);
        }

        // Hàm tạo nhãn tiêu đề nhỏ phía trên ô nhập
        private void CreateHeaderLabel(string text, int top) {
            Label lbl = new Label {
                Text = text, ForeColor = Color.FromArgb(56, 189, 248),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Left = 50, Top = top, AutoSize = true, BackColor = Color.Transparent
            };
            card.Controls.Add(lbl);
        }

        // Hàm tạo ô nhập văn bản (TextBox) to rõ
        private TextBox CreateInputBox(string placeholder, int top) {
            TextBox txt = new TextBox {
                Width = 360, Left = 50, Top = top,
                Font = new Font("Segoe UI", 15), // Chữ to
                BackColor = Color.FromArgb(30, 41, 59), ForeColor = Color.Gray,
                BorderStyle = BorderStyle.FixedSingle, Text = placeholder
            };
            txt.Enter += (s, e) => {
                if (txt.Text == placeholder) {
                    txt.Text = "";
                    txt.ForeColor = Color.White;
                    if (txt == txtPass) {
                        SetPasswordPlaceholderState(false);
                    }
                }
            };
            txt.Leave += (s, e) => {
                if (string.IsNullOrWhiteSpace(txt.Text)) {
                    txt.Text = placeholder;
                    txt.ForeColor = Color.Gray;
                    if (txt == txtPass) {
                        SetPasswordPlaceholderState(true);
                    }
                }
            };
            card.Controls.Add(txt);
            return txt;
        }

        // Oracle DataSource textbox (không dùng placeholder để tránh bị hiểu nhầm là "chưa nhập")
        private TextBox CreateDataSourceInputBox(string initialValue, int top)
        {
            TextBox txt = new TextBox {
                Width = 360, Left = 50, Top = top,
                Font = new Font("Segoe UI", 13),
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Text = initialValue
            };
            card.Controls.Add(txt);
            return txt;
        }

        // Logic ẩn/hiện mật khẩu
        private void TogglePasswordVisibility(object sender, EventArgs e) {
            if (txtPass.Text == "🔒 Enter Password")
            {
                return;
            }

            isPassVisible = !isPassVisible;
            if (isPassVisible) {
                txtPass.PasswordChar = '\0'; // Hiện chữ
                btnShowPass.Text = "🙈"; // Đổi icon
                btnShowPass.ForeColor = Color.FromArgb(56, 189, 248);
            } else {
                txtPass.PasswordChar = '●'; // Ẩn chữ
                btnShowPass.Text = "👁";
                btnShowPass.ForeColor = Color.Gray;
            }
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            string user = txtUser.Text.Trim();
            string pass = txtPass.Text.Trim();
            string dataSource = txtDataSource?.Text?.Trim() ?? "";
            
            if (string.IsNullOrEmpty(user) || user.Contains("Enter") || string.IsNullOrEmpty(pass)) {
                lblError.Text = "⚠ Please enter credentials!";
                return;
            }

            // Chặn các tài khoản hệ thống — không cho đăng nhập qua UI
            string upperUser = user.ToUpper();
            string[] blockedAccounts = { "SYS", "SYSTEM", "LBACSYS" };
            foreach (string blocked in blockedAccounts) {
                if (upperUser == blocked) {
                    lblError.Text = "⚠ Tài khoản hệ thống không được phép đăng nhập!";
                    return;
                }
            }

            try {
                DBConnection db = new DBConnection();
                using (OracleConnection conn = db.GetConnection(user, pass, dataSource)) { conn.Open(); }
                DBConnection.SaveUserDataSource(dataSource);
                lblError.Text = "";
                SaveRememberedUser(user);
                if (upperUser == "BVOWNER" || upperUser == "BV_ADMIN") { new FormAdmin(user, pass).Show(); }
                else { new FormUser(user, pass).Show(); }
                this.Hide();
            }
            catch {
                lblError.Text = "❌ Invalid Username or Password!";
                ShowErrorEffect();
            }
        }

        // --- HIỆU ỨNG ANIMATION ---
        private void StartAnimations() {
            animTimer = new System.Windows.Forms.Timer { Interval = 20 };
            animTimer.Tick += (s, e) => { tick += 0.04f; bg.Invalidate(); };
            animTimer.Start();
        }

        private void DrawCinematicBackground(object? sender, PaintEventArgs e) {
            Graphics g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            using (LinearGradientBrush br = new LinearGradientBrush(bg.ClientRectangle, Color.FromArgb(10, 15, 28), Color.FromArgb(30, 50, 100), 45f))
                g.FillRectangle(br, bg.ClientRectangle);
        }

        private void DrawCardBorder(PaintEventArgs e, int r) {
            Graphics g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            using (Pen p = new Pen(Color.FromArgb(60, 255, 255, 255), 2)) {
                GraphicsPath path = new GraphicsPath();
                path.AddArc(0, 0, r, r, 180, 90); path.AddArc(card.Width - r, 0, r, r, 270, 90);
                path.AddArc(card.Width - r, card.Height - r, r, r, 0, 90); path.AddArc(0, card.Height - r, r, r, 90, 90);
                path.CloseFigure(); g.DrawPath(p, path);
            }
        }

        private void ShowErrorEffect() {
            System.Windows.Forms.Timer shake = new System.Windows.Forms.Timer { Interval = 30 };
            int count = 0;
            shake.Tick += (s, e) => {
                card.Left += (count % 2 == 0) ? 10 : -10;
                if (++count > 6) { shake.Stop(); CenterCard(); }
            };
            shake.Start();
        }

        private void CenterCard() {
            card.Location = new Point((bg.Width - card.Width) / 2, (bg.Height - card.Height) / 2);
            ApplyRegion(card, 40);
        }

        private void LoadRememberedUser()
        {
            try
            {
                if (!File.Exists(rememberFilePath))
                {
                    return;
                }

                string rememberedUser = File.ReadAllText(rememberFilePath).Trim();
                if (!string.IsNullOrWhiteSpace(rememberedUser))
                {
                    txtUser.Text = rememberedUser;
                    txtUser.ForeColor = Color.White;
                    chkRemember.Checked = true;
                    txtPass.Focus();
                }
            }
            catch
            {
                chkRemember.Checked = false;
            }
        }

        private void SaveRememberedUser(string username)
        {
            try
            {
                if (chkRemember.Checked)
                {
                    string? dir = Path.GetDirectoryName(rememberFilePath);
                    if (!string.IsNullOrWhiteSpace(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    File.WriteAllText(rememberFilePath, username);
                }
                else if (File.Exists(rememberFilePath))
                {
                    File.Delete(rememberFilePath);
                }
            }
            catch
            {
                // Khong chan dang nhap neu luu username that bai
            }
        }

        private void SetPasswordPlaceholderState(bool isPlaceholder)
        {
            txtPass.PasswordChar = isPlaceholder || isPassVisible ? '\0' : '●';
            txtPass.ForeColor = isPlaceholder ? Color.Gray : Color.White;
        }

        private void ApplyRegion(Control c, int r) {
            GraphicsPath gp = new GraphicsPath();
            gp.AddArc(0, 0, r*2, r*2, 180, 90); gp.AddArc(c.Width - r*2, 0, r*2, r*2, 270, 90);
            gp.AddArc(c.Width - r*2, c.Height - r*2, r*2, r*2, 0, 90); gp.AddArc(0, c.Height - r*2, r*2, r*2, 90, 90);
            c.Region = new Region(gp);
        }

        [DllImport("user32.dll")] public static extern bool ReleaseCapture();
        [DllImport("user32.dll")] public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
    }

    // Lớp bổ trợ cho nút bấm hiện đại
    public class ModernButton : Button {
        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); this.BackColor = Color.FromArgb(59, 130, 246); }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); this.BackColor = Color.FromArgb(37, 99, 235); }
    }

    // Lớp bổ trợ chống nháy hình
    public class DoubleBufferedPanel : Panel {
        public DoubleBufferedPanel() { 
            this.DoubleBuffered = true; 
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true); 
        }
    }
}
