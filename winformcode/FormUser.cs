using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using HospitalApp.Services;
using HospitalApp.Forms;

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

        // Reload
        Button btnReload = CreateButton("Reload Data", Color.DodgerBlue);
        btnReload.Left = 20;
        btnReload.Top = 10;
        btnReload.Click += (s, e) => LoadData();

        // Logout (quay về login)
        Button btnLogout = CreateButton("Logout", Color.IndianRed);
        btnLogout.Left = 200;
        btnLogout.Top = 10;
        btnLogout.Click += (s, e) =>
        {
            this.Hide();
            LoginForm login = new LoginForm();
            login.Show();
        };

        // OLS
        Button btnOLS = CreateButton("Thông báo (OLS)", Color.MediumPurple);
        btnOLS.Left = 380;
        btnOLS.Top = 10;
        btnOLS.Click += (s, e) =>
        {
            formNotice f = new formNotice(currentUser, currentPass);
            f.Show();
        };

        // (Optional) Quay lại Login riêng
        Button btnBack = CreateButton("Back Login", Color.Gray);
        btnBack.Left = 580;
        btnBack.Top = 10;
        btnBack.Click += (s, e) =>
        {
            this.Hide();
            LoginForm login = new LoginForm();
            login.Show();
        };

        btnPanel.Controls.Add(btnReload);
        btnPanel.Controls.Add(btnLogout);
        btnPanel.Controls.Add(btnOLS);
        btnPanel.Controls.Add(btnBack);

        // ===== GRID =====
        grid = new DataGridView();
        grid.Dock = DockStyle.Fill;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.RowHeadersVisible = false;
        grid.BackgroundColor = Color.White;

        // ===== ADD =====
        this.Controls.Add(grid);
        this.Controls.Add(btnPanel);
        this.Controls.Add(lblUser);
        this.Controls.Add(header);

        grid.BringToFront();
    }

    // ================= BUTTON STYLE =================
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

    // ================= LOAD DATA (VPD) =================
    void LoadData()
    {
        string sql = "";

        if (currentUser.StartsWith("BS") ||
            currentUser.StartsWith("DPV") ||
            currentUser.StartsWith("KTV"))
        {
            sql = "SELECT * FROM BVOWNER.V_CURRENT_NHANVIEN";
        }
        else if (currentUser.StartsWith("BN"))
        {
            sql = "SELECT * FROM BVOWNER.V_CURRENT_BENHNHAN";
        }

        if (string.IsNullOrEmpty(sql))
        {
            MessageBox.Show("⚠ Không có view dữ liệu cho user này! Vui lòng liên hệ admin.");
            return;
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
