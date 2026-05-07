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
DataGridView grid = null!;
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
        this.Controls.Clear();

        TableLayoutPanel root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Color.FromArgb(244, 246, 249)
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 86F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        Panel header = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(45, 90, 180)
        };

        Label title = new Label
        {
            Text = "THÔNG BÁO BỆNH VIỆN (OLS)",
            Font = new Font("Segoe UI", 22, FontStyle.Bold),
            ForeColor = Color.White,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        };
        header.Controls.Add(title);

        Panel toolbar = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(24, 14, 24, 12)
        };

        Label lblUser = new Label
        {
            Text = "User: " + user,
            ForeColor = Color.FromArgb(33, 37, 41),
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            Dock = DockStyle.Left,
            Width = 420,
            TextAlign = ContentAlignment.MiddleLeft
        };

        FlowLayoutPanel actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            Width = 290,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            BackColor = Color.Transparent
        };

        Button btnReload = CreateButton("Làm mới", Color.DodgerBlue);
        btnReload.Click += (s, e) => LoadData();

        Button btnClose = CreateButton("Đóng", Color.IndianRed);
        btnClose.Click += (s, e) => this.Close();

        actions.Controls.Add(btnClose);
        actions.Controls.Add(btnReload);
        toolbar.Controls.Add(actions);
        toolbar.Controls.Add(lblUser);

        Panel gridPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = root.BackColor,
            Padding = new Padding(24, 0, 24, 24)
        };

        grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowHeadersVisible = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AllowUserToAddRows = false,
            EnableHeadersVisualStyles = false
        };

        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(33, 37, 41);
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
        grid.ColumnHeadersHeight = 42;

        grid.DefaultCellStyle.Font = new Font("Segoe UI", 11);
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(41, 128, 185);
        grid.DefaultCellStyle.SelectionForeColor = Color.White;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(242, 245, 249);

        gridPanel.Controls.Add(grid);

        root.Controls.Add(header, 0, 0);
        root.Controls.Add(toolbar, 0, 1);
        root.Controls.Add(gridPanel, 0, 2);
        this.Controls.Add(root);
    }

    Button CreateButton(string text, Color color)
    {
        Button btn = new Button();
        btn.Text = text;
        btn.Width = 124;
        btn.Height = 40;
        btn.BackColor = color;
        btn.ForeColor = Color.White;
        btn.Font = new Font("Segoe UI", 9, FontStyle.Bold);

        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.Margin = new Padding(8, 0, 0, 0);

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
