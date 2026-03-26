using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using HospitalApp.Services;

namespace HospitalApp.Forms
{
    public class LoginForm : Form
    {
        private TextBox txtUser;
        private TextBox txtPass;
        private Button btnLogin;

        public LoginForm()
        {
            InitUI();
        }

        // ================= UI =================
        private void InitUI()
        {
            this.Text = "Hospital System Login";
            this.Size = new Size(520, 380);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(15, 25, 45);

            // ===== MAIN PANEL =====
            Panel panel = new Panel();
            panel.Size = new Size(360, 260);
            panel.BackColor = Color.White;
            panel.Left = (this.Width - panel.Width) / 2;
            panel.Top = (this.Height - panel.Height) / 2;

            panel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = panel.ClientRectangle;
                rect.Inflate(-1, -1);

                using (var brush = new SolidBrush(Color.White))
                using (var path = RoundedRect(rect, 15))
                {
                    g.FillPath(brush, path);
                }
            };

            // ===== TITLE =====
            Label lblTitle = new Label()
            {
                Text = "HOSPITAL LOGIN",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 60, 120),
                AutoSize = false,
                Width = 300,
                Height = 50,
                TextAlign = ContentAlignment.MiddleCenter,
                Top = 10,
                Left = 30
            };

            // ===== TEXTBOX =====
            txtUser = CreateTextbox("Username", 80);
            txtPass = CreateTextbox("Password", 130);
            txtPass.PasswordChar = '*';

            // ===== BUTTON LOGIN =====
            btnLogin = new Button()
            {
                Text = "Login",
                Width = 280,
                Height = 45,
                Left = 40,
                Top = 185,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };

            btnLogin.FlatAppearance.BorderSize = 0;

            btnLogin.MouseEnter += (s, e) =>
                btnLogin.BackColor = Color.FromArgb(30, 150, 255);

            btnLogin.MouseLeave += (s, e) =>
                btnLogin.BackColor = Color.FromArgb(0, 120, 215);

            btnLogin.Click += BtnLogin_Click;

            // ===== CLOSE BUTTON =====
            Button btnClose = new Button()
            {
                Text = "X",
                ForeColor = Color.White,
                BackColor = Color.Red,
                Width = 35,
                Height = 30,
                Top = 5,
                Left = this.Width - 45,
                FlatStyle = FlatStyle.Flat
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => Application.Exit();

            // ===== ADD CONTROL =====
            panel.Controls.Add(lblTitle);
            panel.Controls.Add(txtUser);
            panel.Controls.Add(txtPass);
            panel.Controls.Add(btnLogin);

            this.Controls.Add(panel);
            this.Controls.Add(btnClose);

            this.AcceptButton = btnLogin;
        }

        // ================= TEXTBOX CUSTOM =================
        TextBox CreateTextbox(string placeholder, int top)
        {
            TextBox txt = new TextBox();
            txt.Width = 280;
            txt.Height = 35;
            txt.Left = 40;
            txt.Top = top;
            txt.Font = new Font("Segoe UI", 11);
            txt.ForeColor = Color.Gray;
            txt.Text = placeholder;

            txt.Enter += (s, e) =>
            {
                if (txt.Text == placeholder)
                {
                    txt.Text = "";
                    txt.ForeColor = Color.Black;
                }
            };

            txt.Leave += (s, e) =>
            {
                if (txt.Text == "")
                {
                    txt.Text = placeholder;
                    txt.ForeColor = Color.Gray;
                }
            };

            return txt;
        }

        // ================= LOGIN =================
        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUser.Text.Trim();
            string password = txtPass.Text.Trim();

            if (username == "" || username == "Username" ||
                password == "" || password == "Password")
            {
                MessageBox.Show("Please enter username and password");
                return;
            }

            try
            {
                DBConnection db = new DBConnection();

                using (OracleConnection conn = db.GetConnection(username, password))
                {
                    conn.Open();
                }

                // 🔥 PHÂN LUỒNG
                if (username.ToUpper() == "SYS")
                {
                    FormAdmin admin = new FormAdmin(username, password);
                    admin.Show();
                }
                else
                {
                    FormUser userForm = new FormUser(username, password);
                    userForm.Show();
                }

                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Login failed: " + ex.Message);
            }
        }

        // ================= BO GÓC =================
        GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            GraphicsPath path = new GraphicsPath();

            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);

            path.CloseFigure();
            return path;
        }
    }
}