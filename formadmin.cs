using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using HospitalApp.Services;

namespace HospitalApp
{
    public class FormAdmin : Form
    {
        DBConnection db = new DBConnection();

        string currentUser;
        string currentPass;

        DataGridView grid;

        TextBox txtUser;
        TextBox txtPass;
        TextBox txtRole;
        TextBox txtObject;
        TextBox txtColumn;

        ComboBox cbPrivilege;
        CheckBox chkGrantOption;

        Color cardBg = Color.FromArgb(35, 45, 65);

        public FormAdmin(string user, string pass)
        {
            currentUser = user;
            currentPass = pass;

            this.WindowState = FormWindowState.Maximized;
            this.Text = "HOSPITAL ORACLE - ADMIN CONSOLE";
            this.Font = new Font("Segoe UI", 10);
            this.BackColor = Color.FromArgb(18, 23, 40);

            this.Paint += DrawBackground;

            BuildUI();
        }

        // ===== BACKGROUND =====
        void DrawBackground(object sender, PaintEventArgs e)
        {
            using (LinearGradientBrush brush =
                new LinearGradientBrush(this.ClientRectangle,
                Color.FromArgb(10, 15, 35),
                Color.FromArgb(40, 80, 160),
                120f))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
        }

        // ===== UI =====
        void BuildUI()
        {
            Panel header = new Panel();
            header.Dock = DockStyle.Top;
            header.Height = 120;
            header.BackColor = Color.Transparent;

            Label title = new Label();
            title.Text = "HOSPITAL SYSTEM ADMIN CONSOLE";
            title.Dock = DockStyle.Fill;
            title.TextAlign = ContentAlignment.MiddleCenter;
            title.Font = new Font("Segoe UI", 26, FontStyle.Bold);
            title.ForeColor = Color.White;

            header.Controls.Add(title);
            this.Controls.Add(header);

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.Padding = new Padding(20, 20, 20, 20);

            layout.ColumnCount = 3;
            layout.RowCount = 2;

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));

            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 55));

            this.Controls.Add(layout);

            // ===== GRID =====
            Panel gridCard = CreateCard("DATABASE OVERVIEW");
            layout.SetColumnSpan(gridCard, 3);
            layout.Controls.Add(gridCard, 0, 0);

            grid = new DataGridView();
            grid.Dock = DockStyle.Fill;

            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.RowHeadersVisible = false;

            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersHeight = 50;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 120, 215);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 13, FontStyle.Bold);

            grid.DefaultCellStyle.Font = new Font("Segoe UI", 11);
            grid.RowTemplate.Height = 35;

            grid.RowsDefaultCellStyle.BackColor = Color.White;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 245, 255);

            gridCard.Controls["container"].Controls.Add(grid);

            // ===== USER =====
            Panel userCard = CreateCard("USER MANAGEMENT");
            layout.Controls.Add(userCard, 0, 1);

            Control u = userCard.Controls["container"];

            AddLabel(u, "Username", 20, 20);
            txtUser = AddTextbox(u, 20, 45);

            AddLabel(u, "Password", 20, 90);
            txtPass = AddTextbox(u, 20, 115);
            txtPass.PasswordChar = '*';

            AddButton(u, "Create User", 20, 180, CreateUser, Color.DodgerBlue);
            AddButton(u, "Drop User", 220, 180, DropUser, Color.IndianRed);

            AddButton(u, "Alter Password", 20, 240, AlterUser, Color.MediumPurple);
            AddButton(u, "Load Users", 220, 240, LoadUsers, Color.MediumSeaGreen);

            // ===== ROLE =====
            Panel roleCard = CreateCard("ROLE MANAGEMENT");
            layout.Controls.Add(roleCard, 1, 1);

            Control r = roleCard.Controls["container"];

            AddLabel(r, "Role Name", 20, 20);
            txtRole = AddTextbox(r, 20, 45);

            AddButton(r, "Create Role", 20, 110, CreateRole, Color.DodgerBlue);
            AddButton(r, "Drop Role", 220, 110, DropRole, Color.IndianRed);
            AddButton(r, "Load Roles", 20, 170, LoadRoles, Color.MediumSeaGreen);

            // ===== PRIVILEGE =====
            Panel privCard = CreateCard("PRIVILEGE CONTROL");
            layout.Controls.Add(privCard, 2, 1);

            Control p = privCard.Controls["container"];

            AddLabel(p, "Object", 20, 20);
            txtObject = AddTextbox(p, 20, 45);

            AddLabel(p, "Privilege", 220, 20);

            cbPrivilege = new ComboBox();
            cbPrivilege.SetBounds(220, 45, 180, 36);
            cbPrivilege.DropDownStyle = ComboBoxStyle.DropDownList;
            cbPrivilege.Items.AddRange(new string[] { "SELECT", "UPDATE", "INSERT", "DELETE", "EXECUTE" });

            p.Controls.Add(cbPrivilege);

            AddLabel(p, "Column", 20, 100);
            txtColumn = AddTextbox(p, 20, 125);

            chkGrantOption = new CheckBox();
            chkGrantOption.Text = "WITH GRANT OPTION";
            chkGrantOption.ForeColor = Color.White;
            chkGrantOption.SetBounds(220, 130, 200, 25);

            p.Controls.Add(chkGrantOption);

            AddButton(p, "Grant User", 20, 190, GrantPrivilegeToUser, Color.MediumSeaGreen);
            AddButton(p, "Grant Role", 220, 190, GrantPrivilegeToRole, Color.MediumSeaGreen);

            AddButton(p, "Role -> User", 20, 250, GrantRoleToUser, Color.HotPink);
            AddButton(p, "Revoke", 220, 250, RevokePrivilege, Color.IndianRed);

            AddButton(p, "Check Rights", 120, 310, ViewPrivileges, Color.Gray);
        }

        // ===== CARD =====
        Panel CreateCard(string title)
        {
            Panel card = new Panel();
            card.Dock = DockStyle.Fill;
            card.BackColor = cardBg;
            card.Margin = new Padding(15);

            card.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                Rectangle rect = card.ClientRectangle;
                rect.Inflate(-1, -1);

                using (GraphicsPath path = RoundedRect(rect, 15))
                using (SolidBrush brush = new SolidBrush(cardBg))
                {
                    g.FillPath(brush, path);
                }
            };

            Label lbl = new Label();
            lbl.Text = title;
            lbl.Dock = DockStyle.Top;
            lbl.Height = 45;
            lbl.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            lbl.ForeColor = Color.Cyan;
            lbl.TextAlign = ContentAlignment.MiddleCenter;

            Panel container = new Panel();
            container.Name = "container";
            container.Dock = DockStyle.Fill;

            card.Controls.Add(container);
            card.Controls.Add(lbl);

            return card;
        }

        // ===== UI HELPER =====
        Label AddLabel(Control parent, string text, int x, int y)
        {
            Label lbl = new Label();
            lbl.Text = text;
            lbl.ForeColor = Color.Silver;
            lbl.SetBounds(x, y, 200, 20);
            parent.Controls.Add(lbl);
            return lbl;
        }

        TextBox AddTextbox(Control parent, int x, int y)
        {
            TextBox txt = new TextBox();
            txt.SetBounds(x, y, 200, 36);
            txt.Font = new Font("Segoe UI", 11);
            txt.BackColor = Color.FromArgb(45, 52, 70);
            txt.ForeColor = Color.White;

            txt.Enter += (s, e) => txt.BackColor = Color.FromArgb(60, 70, 95);
            txt.Leave += (s, e) => txt.BackColor = Color.FromArgb(45, 52, 70);

            parent.Controls.Add(txt);
            return txt;
        }

        Button AddButton(Control parent, string text, int x, int y,
        EventHandler action, Color color)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.SetBounds(x, y, 180, 45);

            btn.BackColor = color;
            btn.ForeColor = Color.White;

            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            btn.MouseEnter += (s, e) => btn.BackColor = ControlPaint.Light(color);
            btn.MouseLeave += (s, e) => btn.BackColor = color;

            btn.Click += action;

            parent.Controls.Add(btn);
            return btn;
        }

        // ===== DB =====
        void ExecuteNonQuery(string sql)
        {
            try
            {
                using (OracleConnection conn = db.GetConnection(currentUser, currentPass))
                {
                    conn.Open();
                    new OracleCommand(sql, conn).ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void CreateUser(object s, EventArgs e) => ExecuteNonQuery($"CREATE USER {txtUser.Text} IDENTIFIED BY {txtPass.Text}");
        void DropUser(object s, EventArgs e) => ExecuteNonQuery($"DROP USER {txtUser.Text} CASCADE");
        void AlterUser(object s, EventArgs e) => ExecuteNonQuery($"ALTER USER {txtUser.Text} IDENTIFIED BY {txtPass.Text}");
        void CreateRole(object s, EventArgs e) => ExecuteNonQuery($"CREATE ROLE {txtRole.Text}");
        void DropRole(object s, EventArgs e) => ExecuteNonQuery($"DROP ROLE {txtRole.Text}");

        void LoadUsers(object s, EventArgs e)
        {
            using (OracleConnection conn = db.GetConnection(currentUser, currentPass))
            {
                conn.Open();

                OracleDataAdapter da = new OracleDataAdapter(
@"SELECT USERNAME, ACCOUNT_STATUS 
FROM DBA_USERS
WHERE USERNAME LIKE 'DPV_%'
   OR USERNAME LIKE 'BS_%'
   OR USERNAME LIKE 'KTV_%'
   OR USERNAME LIKE 'BN_%'", conn);

                DataTable dt = new DataTable();
                da.Fill(dt);
                grid.DataSource = dt;
            }
        }

        void LoadRoles(object s, EventArgs e)
        {
            using (OracleConnection conn = db.GetConnection(currentUser, currentPass))
            {
                conn.Open();
                OracleDataAdapter da = new OracleDataAdapter(
                    "SELECT ROLE FROM DBA_ROLES WHERE ROLE LIKE 'ROLE_%'", conn);

                DataTable dt = new DataTable();
                da.Fill(dt);
                grid.DataSource = dt;
            }
        }

        void GrantPrivilegeToUser(object s, EventArgs e) => Grant(txtUser.Text);
        void GrantPrivilegeToRole(object s, EventArgs e) => Grant(txtRole.Text);

        void Grant(string grantee)
        {
            string sql = $"GRANT {cbPrivilege.Text} ON {txtObject.Text} TO {grantee}";
            if (chkGrantOption.Checked) sql += " WITH GRANT OPTION";
            ExecuteNonQuery(sql);
        }

        void GrantRoleToUser(object s, EventArgs e) =>
            ExecuteNonQuery($"GRANT {txtRole.Text} TO {txtUser.Text}");

        void RevokePrivilege(object s, EventArgs e) =>
            ExecuteNonQuery($"REVOKE {cbPrivilege.Text} ON {txtObject.Text} FROM {txtUser.Text}");

        void ViewPrivileges(object s, EventArgs e)
        {
            using (OracleConnection conn = db.GetConnection(currentUser, currentPass))
            {
                conn.Open();

                string sql =
$@"SELECT GRANTEE, OWNER, TABLE_NAME, PRIVILEGE, GRANTABLE
FROM DBA_TAB_PRIVS
WHERE GRANTEE = '{txtUser.Text.ToUpper()}'";

                OracleDataAdapter da = new OracleDataAdapter(sql, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                grid.DataSource = dt;
            }
        }

        // ===== BO GÓC =====
        GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            GraphicsPath path = new GraphicsPath();

            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);

            path.CloseFigure();
            return path;
        }
    }
}