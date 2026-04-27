using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using HospitalApp.Services;

namespace HospitalApp
{
public class formNotice : Form
{
DataGridView grid;
string user, pass;
    public formNotice(string u, string p)
    {
        user = u;
        pass = p;

        this.Text = "THÔNG BÁO (OLS)";
        this.WindowState = FormWindowState.Maximized;
        this.BackColor = Color.FromArgb(20, 30, 50);

        BuildUI();
        LoadData();
    }

    // ===== UI =====
    void BuildUI()
    {
        Panel header = new Panel();
        header.Dock = DockStyle.Top;
        header.Height = 80;
        header.BackColor = Color.FromArgb(45, 90, 180);

        Label title = new Label();
        title.Text = "THÔNG BÁO BỆNH VIỆN (OLS)";
        title.Font = new Font("Segoe UI", 22, FontStyle.Bold);
        title.ForeColor = Color.White;
        title.Dock = DockStyle.Fill;
        title.TextAlign = ContentAlignment.MiddleCenter;

        header.Controls.Add(title);

        Label lblUser = new Label();
        lblUser.Text = "User: " + user;
        lblUser.ForeColor = Color.White;
        lblUser.Font = new Font("Segoe UI", 12, FontStyle.Bold);
        lblUser.Dock = DockStyle.Top;
        lblUser.Height = 35;
        lblUser.TextAlign = ContentAlignment.MiddleCenter;

        Panel btnPanel = new Panel();
        btnPanel.Dock = DockStyle.Top;
        btnPanel.Height = 60;

        Button btnReload = CreateButton("Reload", Color.DodgerBlue);
        btnReload.Left = 20;
        btnReload.Top = 10;
        btnReload.Click += (s, e) => LoadData();

        Button btnClose = CreateButton("Đóng", Color.IndianRed);
        btnClose.Left = 160;
        btnClose.Top = 10;
        btnClose.Click += (s, e) => this.Close();

        btnPanel.Controls.Add(btnReload);
        btnPanel.Controls.Add(btnClose);

        grid = new DataGridView();
        grid.Dock = DockStyle.Fill;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.RowHeadersVisible = false;
        grid.BackgroundColor = Color.White;

        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.Navy;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
        grid.EnableHeadersVisualStyles = false;

        grid.DefaultCellStyle.Font = new Font("Segoe UI", 11);
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.LightGray;

        this.Controls.Add(grid);
        this.Controls.Add(btnPanel);
        this.Controls.Add(lblUser);
        this.Controls.Add(header);

        grid.BringToFront();
    }

    Button CreateButton(string text, Color color)
    {
        Button btn = new Button();
        btn.Text = text;
        btn.Width = 120;
        btn.Height = 40;
        btn.BackColor = color;
        btn.ForeColor = Color.White;

        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;

        btn.MouseEnter += (s, e) => btn.BackColor = ControlPaint.Light(color);
        btn.MouseLeave += (s, e) => btn.BackColor = color;

        return btn;
    }

    // ===== LOAD DATA (OLS) =====
    void LoadData()
    {
        string sql = "SELECT NOIDUNG, NGAYGIO, DIADIEM FROM BVOWNER.THONGBAO";

        try
        {
            DBConnection db = new DBConnection();

            using (OracleConnection conn = db.GetConnection(user, pass))
            {
                conn.Open();

                OracleDataAdapter da = new OracleDataAdapter(sql, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                grid.DataSource = dt;

                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("Không có thông báo (OLS đang lọc dữ liệu)");
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi: " + ex.Message);
        }
    }
}

}
