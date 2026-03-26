using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using HospitalApp.Services;

namespace HospitalApp
{
    public class FormUser : Form
    {
        DataGridView grid;
        string currentUser;
        string currentPass;

        public FormUser(string user, string pass)
        {
            currentUser = user;
            currentPass = pass;

            this.Text = "USER PANEL - " + user;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(20, 30, 50);

            BuildUI();
            LoadData();
        }

        // ================= UI =================
        void BuildUI()
        {
            // ===== GRID (PHẢI TẠO TRƯỚC) =====
            grid = new DataGridView();
            grid.Dock = DockStyle.Fill;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.RowHeadersVisible = false;

            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.None;

            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersHeight = 50;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 120, 215);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 12, FontStyle.Bold);

            grid.DefaultCellStyle.Font = new Font("Segoe UI", 11);
            grid.RowTemplate.Height = 35;

            grid.RowsDefaultCellStyle.BackColor = Color.White;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(230, 240, 255);

            // ===== HEADER =====
            Panel header = new Panel();
            header.Dock = DockStyle.Top;
            header.Height = 80;
            header.BackColor = Color.FromArgb(30, 60, 120);

            Label title = new Label();
            title.Text = "HOSPITAL USER PANEL";
            title.Font = new Font("Segoe UI", 22, FontStyle.Bold);
            title.ForeColor = Color.White;
            title.Dock = DockStyle.Fill;
            title.TextAlign = ContentAlignment.MiddleCenter;

            header.Controls.Add(title);

            // ===== USER INFO =====
            Label lblUser = new Label();
            lblUser.Text = "User: " + currentUser;
            lblUser.ForeColor = Color.White;
            lblUser.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblUser.Dock = DockStyle.Top;
            lblUser.Height = 40;
            lblUser.TextAlign = ContentAlignment.MiddleCenter;

            // ===== BUTTON PANEL =====
            Panel btnPanel = new Panel();
            btnPanel.Dock = DockStyle.Top;
            btnPanel.Height = 60;

            Button btnReload = CreateButton("Reload Data", Color.DodgerBlue);
            btnReload.Click += (s, e) => LoadData();

            Button btnExit = CreateButton("Logout", Color.IndianRed);
            btnExit.Click += (s, e) => Application.Exit();

            btnReload.Left = 20;
            btnReload.Top = 10;

            btnExit.Left = 200;
            btnExit.Top = 10;

            btnPanel.Controls.Add(btnReload);
            btnPanel.Controls.Add(btnExit);

            // ===== ADD CONTROL (QUAN TRỌNG NHẤT) =====
            this.Controls.Add(grid);       // add FIRST
            this.Controls.Add(btnPanel);
            this.Controls.Add(lblUser);
            this.Controls.Add(header);

            grid.BringToFront(); // đảm bảo không bị đè
        }

        Button CreateButton(string text, Color color)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Width = 150;
            btn.Height = 40;
            btn.BackColor = color;
            btn.ForeColor = Color.White;

            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;

            btn.MouseEnter += (s, e) => btn.BackColor = ControlPaint.Light(color);
            btn.MouseLeave += (s, e) => btn.BackColor = color;

            return btn;
        }

        // ================= LOAD DATA =================
        void LoadData()
        {
            string sql = "";

            if (currentUser.StartsWith("BS") ||
                currentUser.StartsWith("DPV") ||
                currentUser.StartsWith("KTV"))
            {
                sql = "SELECT * FROM V_CURRENT_NHANVIEN";
            }
            else if (currentUser.StartsWith("BN"))
            {
                sql = "SELECT * FROM V_CURRENT_BENHNHAN";
            }

            try
            {
                DBConnection db = new DBConnection();

                using (OracleConnection conn = db.GetConnection(currentUser, currentPass))
                {
                    conn.Open();

                    OracleDataAdapter da = new OracleDataAdapter(sql, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    grid.DataSource = dt;

                    // DEBUG
                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("⚠ Không có dữ liệu cho user này!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi load data: " + ex.Message);
            }
        }
    }
}