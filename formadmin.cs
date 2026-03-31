using System;
using System.Data;
using System.Drawing;
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

TextBox txtUser, txtPass, txtRole;
TextBox txtSearchUser, txtSearchRole;
TextBox txtObject, txtColumn;

ComboBox cbPrivilege;
CheckBox chkGrant;

DataTable userTable;
DataTable roleTable;

public FormAdmin(string user, string pass)
{
    currentUser = user;
    currentPass = pass;

    this.Text = "ADMIN CONSOLE";
    this.WindowState = FormWindowState.Maximized;
    this.BackColor = Color.FromArgb(2, 6, 23);

    BuildUI();
}

// ================= HELPER =================
bool IsEmpty(params TextBox[] arr)
{
    foreach (var t in arr)
        if (string.IsNullOrWhiteSpace(t.Text))
        {
            MessageBox.Show("⚠ Không được để trống!");
            return true;
        }
    return false;
}

void ShowMsg(bool ok, string err, string success)
{
    if (ok)
        MessageBox.Show("✔ " + success);
    else if (err.Contains("ORA-01920"))
        MessageBox.Show("⚠ Đã tồn tại!");
    else if (err.Contains("ORA-01918"))
        MessageBox.Show("⚠ Không tồn tại!");
    else
        MessageBox.Show("❌ " + err);
}

// ================= UI =================
void BuildUI()
{
    TableLayoutPanel main = new TableLayoutPanel();
    main.Dock = DockStyle.Fill;
    main.RowCount = 3;
    main.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
    main.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
    main.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
    this.Controls.Add(main);

    Label title = new Label();
    title.Text = "HOSPITAL SYSTEM ADMIN CONSOLE";
    title.Dock = DockStyle.Fill;
    title.Font = new Font("Segoe UI", 18, FontStyle.Bold);
    title.ForeColor = Color.Cyan;
    title.TextAlign = ContentAlignment.MiddleCenter;
    main.Controls.Add(title, 0, 0);

    grid = new DataGridView();
    grid.Dock = DockStyle.Fill;
    grid.BackgroundColor = Color.FromArgb(15, 23, 42);
    grid.BorderStyle = BorderStyle.None;
    grid.RowHeadersVisible = false;
    grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
    grid.EnableHeadersVisualStyles = false;
    grid.ColumnHeadersDefaultCellStyle.BackColor = Color.Black;
    grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.Cyan;
    main.Controls.Add(grid, 0, 1);

    TableLayoutPanel panel = new TableLayoutPanel();
    panel.Dock = DockStyle.Fill;
    panel.ColumnCount = 3;
    panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
    panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
    panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
    main.Controls.Add(panel, 0, 2);

    panel.Controls.Add(CreateUserPanel(), 0, 0);
    panel.Controls.Add(CreateRolePanel(), 1, 0);
    panel.Controls.Add(CreatePrivPanel(), 2, 0);
}

// ================= USER =================
Panel CreateUserPanel()
{
    Panel p = CreateCard("USER MANAGEMENT");

    txtUser = AddTextbox(p, "Username");
    txtPass = AddTextbox(p, "Password");
    txtSearchUser = AddTextbox(p, "Search User");

    AddButton(p, "Create User", CreateUser);
    AddButton(p, "Drop User", DropUser);
    AddButton(p, "Alter Password", AlterUser);
    AddButton(p, "Load Users", LoadUsers);

    return p;
}

// ================= ROLE =================
Panel CreateRolePanel()
{
    Panel p = CreateCard("ROLE MANAGEMENT");

    txtRole = AddTextbox(p, "Role Name");
    txtSearchRole = AddTextbox(p, "Search Role");

    AddButton(p, "Create Role", CreateRole);
    AddButton(p, "Drop Role", DropRole);
    AddButton(p, "Load Roles", LoadRoles);

    return p;
}

// ================= PRIV =================
Panel CreatePrivPanel()
{
    Panel p = CreateCard("PRIVILEGE CONTROL");

    txtObject = AddTextbox(p, "Object");
    txtColumn = AddTextbox(p, "Column (optional)");

    chkGrant = new CheckBox();
    chkGrant.Text = "WITH GRANT OPTION";
    chkGrant.ForeColor = Color.White;
    chkGrant.Dock = DockStyle.Top;
    AddControl(p, chkGrant);

    cbPrivilege = new ComboBox();
    cbPrivilege.Dock = DockStyle.Top;
    cbPrivilege.BackColor = Color.FromArgb(30, 41, 59);
    cbPrivilege.ForeColor = Color.White;
    cbPrivilege.Items.AddRange(new string[] { "SELECT", "UPDATE", "INSERT", "DELETE" });
    AddControl(p, cbPrivilege);

    AddButton(p, "Grant User", GrantUser);
    AddButton(p, "Grant Role", GrantRole);
    AddButton(p, "Role → User", GrantRoleToUser);
    AddButton(p, "Revoke", Revoke);
    AddButton(p, "View Privileges", LoadPrivileges);

    return p;
}

Panel CreateCard(string title)
{
    Panel card = new Panel();
    card.Dock = DockStyle.Fill;
    card.BackColor = Color.FromArgb(15, 23, 42);
    card.Padding = new Padding(10);

    Label lbl = new Label();
    lbl.Text = title;
    lbl.Dock = DockStyle.Top;
    lbl.Height = 30;
    lbl.ForeColor = Color.Cyan;
    lbl.TextAlign = ContentAlignment.MiddleCenter;

    TableLayoutPanel content = new TableLayoutPanel();
    content.Dock = DockStyle.Fill;
    content.ColumnCount = 1;
    content.AutoScroll = true;

    card.Controls.Add(content);
    card.Controls.Add(lbl);

    return card;
}

TextBox AddTextbox(Control p, string label)
{
    var content = (TableLayoutPanel)p.Controls[0];

    Label l = new Label();
    l.Text = label;
    l.ForeColor = Color.White;

    TextBox t = new TextBox();
    t.Dock = DockStyle.Top;
    t.Height = 28;
    t.BackColor = Color.FromArgb(30, 41, 59);
    t.ForeColor = Color.White;

    content.Controls.Add(l);
    content.Controls.Add(t);

    return t;
}

void AddControl(Control p, Control c)
{
    ((TableLayoutPanel)p.Controls[0]).Controls.Add(c);
}

Button AddButton(Control p, string text, EventHandler ev)
{
    var content = (TableLayoutPanel)p.Controls[0];

    Button b = new Button();
    b.Text = text;
    b.Height = 35;
    b.Dock = DockStyle.Top;
    b.ForeColor = Color.White;
    b.FlatStyle = FlatStyle.Flat;

    if (text.Contains("Create"))
        b.BackColor = Color.FromArgb(37, 99, 235);
    else if (text.Contains("Drop") || text.Contains("Revoke"))
        b.BackColor = Color.FromArgb(220, 38, 38);
    else if (text.Contains("Load") || text.Contains("Grant"))
        b.BackColor = Color.FromArgb(22, 163, 74);
    else if (text.Contains("Alter"))
        b.BackColor = Color.FromArgb(147, 51, 234);
    else if (text.Contains("Role"))
        b.BackColor = Color.FromArgb(219, 39, 119);

    b.Click += ev;
    content.Controls.Add(b);

    return b;
}

// ================= DB =================
bool Exec(string sql, out string err)
{
    try
    {
        using (var conn = db.GetConnection(currentUser, currentPass))
        {
            conn.Open();
            new OracleCommand(sql, conn).ExecuteNonQuery();
        }
        err = "";
        return true;
    }
    catch (Exception ex)
    {
        err = ex.Message;
        return false;
    }
}

// ================= USER =================
void CreateUser(object s, EventArgs e)
{
    if (IsEmpty(txtUser, txtPass)) return;

    string err;
    bool ok = Exec($"CREATE USER {txtUser.Text} IDENTIFIED BY {txtPass.Text}", out err);
    ShowMsg(ok, err, "Tạo user thành công");

    if (ok) LoadUsers(null, null);
}

void DropUser(object s, EventArgs e)
{
    if (IsEmpty(txtUser)) return;

    string err;
    bool ok = Exec($"DROP USER {txtUser.Text} CASCADE", out err);
    ShowMsg(ok, err, "Xóa user thành công");

    if (ok) LoadUsers(null, null);
}

void AlterUser(object s, EventArgs e)
{
    if (IsEmpty(txtUser, txtPass)) return;

    string err;
    bool ok = Exec($"ALTER USER {txtUser.Text} IDENTIFIED BY {txtPass.Text}", out err);
    ShowMsg(ok, err, "Đổi mật khẩu thành công");
}

void LoadUsers(object s, EventArgs e)
{
    using (var conn = db.GetConnection(currentUser, currentPass))
    {
        conn.Open();
        var da = new OracleDataAdapter("SELECT USERNAME FROM DBA_USERS", conn);
        userTable = new DataTable();
        da.Fill(userTable);
        grid.DataSource = userTable;
    }

    txtSearchUser.TextChanged -= SearchUserChanged;
    txtSearchUser.TextChanged += SearchUserChanged;
}

void SearchUserChanged(object sender, EventArgs e)
{
    if (userTable != null)
    {
        string keyword = txtSearchUser.Text.Trim().ToUpper();
        userTable.DefaultView.RowFilter = $"USERNAME LIKE '%{keyword}%'";
    }
}

// ================= ROLE =================
void CreateRole(object s, EventArgs e)
{
    if (IsEmpty(txtRole)) return;

    string err;
    bool ok = Exec($"CREATE ROLE {txtRole.Text}", out err);
    ShowMsg(ok, err, "Tạo role thành công");

    if (ok) LoadRoles(null, null);
}

void DropRole(object s, EventArgs e)
{
    if (IsEmpty(txtRole)) return;

    string err;
    bool ok = Exec($"DROP ROLE {txtRole.Text}", out err);
    ShowMsg(ok, err, "Xóa role thành công");

    if (ok) LoadRoles(null, null);
}

void LoadRoles(object s, EventArgs e)
{
    using (var conn = db.GetConnection(currentUser, currentPass))
    {
        conn.Open();
        var da = new OracleDataAdapter("SELECT ROLE FROM DBA_ROLES", conn);
        roleTable = new DataTable();
        da.Fill(roleTable);
        grid.DataSource = roleTable;
    }

    txtSearchRole.TextChanged -= SearchRoleChanged;
    txtSearchRole.TextChanged += SearchRoleChanged;
}

void SearchRoleChanged(object sender, EventArgs e)
{
    if (roleTable != null)
    {
        string keyword = txtSearchRole.Text.Trim().ToUpper();
        roleTable.DefaultView.RowFilter = $"ROLE LIKE '%{keyword}%'";
    }
}

// ================= GRANT =================
string BuildGrantSQL(string target)
{
    string privilege = cbPrivilege.Text;
    string obj = txtObject.Text.ToUpper();
    string col = txtColumn.Text.Trim();
    string grantOpt = chkGrant.Checked ? " WITH GRANT OPTION" : "";

    if ((privilege == "SELECT" || privilege == "UPDATE") && col != "")
        return $"GRANT {privilege} ({col}) ON {obj} TO {target}{grantOpt}";

    return $"GRANT {privilege} ON {obj} TO {target}{grantOpt}";
}

void GrantUser(object s, EventArgs e)
{
    if (IsEmpty(txtUser, txtObject)) return;

    string err;
    bool ok = Exec(BuildGrantSQL(txtUser.Text.ToUpper()), out err);
    ShowMsg(ok, err, "Grant user thành công");
}

void GrantRole(object s, EventArgs e)
{
    if (IsEmpty(txtRole, txtObject)) return;

    string err;
    bool ok = Exec(BuildGrantSQL(txtRole.Text.ToUpper()), out err);
    ShowMsg(ok, err, "Grant role thành công");
}

void GrantRoleToUser(object s, EventArgs e)
{
    if (IsEmpty(txtUser, txtRole)) return;

    string err;
    bool ok = Exec($"GRANT {txtRole.Text.ToUpper()} TO {txtUser.Text.ToUpper()}", out err);
    ShowMsg(ok, err, "Gán role thành công");
}

// ================= REVOKE =================
void Revoke(object s, EventArgs e)
{
    string target = !string.IsNullOrEmpty(txtUser.Text)
        ? txtUser.Text.ToUpper()
        : txtRole.Text.ToUpper();

    if (string.IsNullOrEmpty(target) || string.IsNullOrEmpty(txtObject.Text))
    {
        MessageBox.Show("⚠ Nhập user/role + object!");
        return;
    }

    string privilege = cbPrivilege.Text;
    string obj = txtObject.Text.ToUpper();
    string col = txtColumn.Text.Trim();

    string sql;

    if ((privilege == "SELECT" || privilege == "UPDATE") && col != "")
        sql = $"REVOKE {privilege} ({col}) ON {obj} FROM {target}";
    else
        sql = $"REVOKE {privilege} ON {obj} FROM {target}";

    string err;
    bool ok = Exec(sql, out err);
    ShowMsg(ok, err, "Revoke thành công");
}

// ================= VIEW =================
void LoadPrivileges(object s, EventArgs e)
{
    using (var conn = db.GetConnection(currentUser, currentPass))
    {
        conn.Open();

        string name = !string.IsNullOrEmpty(txtUser.Text)
            ? txtUser.Text.ToUpper()
            : txtRole.Text.ToUpper();

        string sql = $@"
        SELECT GRANTEE, OWNER, TABLE_NAME, PRIVILEGE, 'DIRECT' AS TYPE
        FROM DBA_TAB_PRIVS
        WHERE GRANTEE = '{name}'

        UNION

        SELECT RP.GRANTEE, TP.OWNER, TP.TABLE_NAME, TP.PRIVILEGE, RP.GRANTED_ROLE AS TYPE
        FROM DBA_ROLE_PRIVS RP
        JOIN DBA_TAB_PRIVS TP ON RP.GRANTED_ROLE = TP.GRANTEE
        WHERE RP.GRANTEE = '{name}'
        ";

        var da = new OracleDataAdapter(sql, conn);
        DataTable dt = new DataTable();
        da.Fill(dt);
        grid.DataSource = dt;
    }
}


}
}
