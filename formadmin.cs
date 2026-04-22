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
                #region 1. BIẾN HỆ THỐNG & MÀU SẮC
                
                private DBConnection db = new DBConnection();
                private string currentUser, currentPass;
                private Panel contentPanel;
                private Label lblHeaderTitle;
                private DataGridView grid;

                private Panel pnlUserSubmenu;

                // Controls nhập liệu
                private TextBox txtUser, txtPass, txtRole, txtSearchUser, txtSearchRole, txtObject, txtColumn;
                private ComboBox cbPrivilege;
                private ComboBox cbUser, cbRole;
                private CheckBox chkGrant;
                private DataTable userTable, roleTable;

                private Panel pnlRoleSubmenu;

                private Panel pnlPrivSubmenu;

                // Bảng màu hiện đại
                private readonly Color clrPrimary = Color.FromArgb(41, 128, 185);    // Blue (Dùng cho Header)
                private readonly Color clrSidebar = Color.FromArgb(33, 37, 41);     // Dark Gray
                private readonly Color clrLogoBg = Color.FromArgb(20, 25, 30);      // Deep Dark (Cho Logo)
                private readonly Color clrBackground = Color.FromArgb(244, 246, 249); 
                private readonly Color clrSuccess = Color.FromArgb(40, 167, 69);   
                private readonly Color clrDanger = Color.FromArgb(220, 53, 69);    

                #endregion

                #region 2. KHỞI TẠO FORM

                public FormAdmin(string user, string pass)
                {
                    this.currentUser = user;
                    this.currentPass = pass;

                    this.WindowState = FormWindowState.Maximized;
                    this.MinimumSize = new Size(1280, 800);
                    this.Text = "HOSPITAL OS | QUẢN TRỊ HỆ THỐNG";
                    this.Font = new Font("Segoe UI", 10);

                    InitializeComponentCustom();
                }

                #endregion

                #region 3. GIAO DIỆN CHÍNH (LAYOUT)

           private void InitializeComponentCustom()
{
    // --- KHỞI TẠO LAYOUT CHÍNH ---
    TableLayoutPanel mainLayout = new TableLayoutPanel
    {
        Dock = DockStyle.Fill,
        ColumnCount = 2,
        RowCount = 1
    };
    mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 280)); 
    mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
    this.Controls.Add(mainLayout);

    // --- THANH SIDEBAR BÊN TRÁI ---
    Panel sidebar = new Panel { Dock = DockStyle.Fill, BackColor = clrSidebar, AutoScroll = true };
    mainLayout.Controls.Add(sidebar, 0, 0);

    // 1. LOGO
    Label lblLogo = new Label
    {
        Text = "✚ HOSPITAL OS",
        Dock = DockStyle.Top, Height = 80,
        ForeColor = Color.White, BackColor = clrLogoBg,
        Font = new Font("Segoe UI", 16, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter
    };

    // 2. MENU CON: CẤU HÌNH USER (Quản lý, Tạo mới)
    pnlUserSubmenu = new Panel { Dock = DockStyle.Top, Height = 0, Visible = false, BackColor = Color.FromArgb(45, 50, 55) };
    var btnAllUsers = CreateSubMenuBtn("  ›  Quản lý user", () => LoadPage(CreateUserPage(), "Quản lý Người dùng"));
    var btnCreateUser = CreateSubMenuBtn("  ›  Tạo user mới", () => CreateUser(null, null));
    pnlUserSubmenu.Controls.Add(btnCreateUser);
    pnlUserSubmenu.Controls.Add(btnAllUsers);
    pnlUserSubmenu.Height = pnlUserSubmenu.Controls.Count * 45;

    var btnUserHeader = CreateMenuBtn("👤  Cấu hình User  ▼", () => {
        pnlUserSubmenu.Visible = !pnlUserSubmenu.Visible;
    });

    // 3. MENU CON: VAI TRÒ (Quản lý, Tạo mới)
    pnlRoleSubmenu = new Panel { Dock = DockStyle.Top, Height = 0, Visible = false, BackColor = Color.FromArgb(45, 50, 55) };
    var btnAllRoles = CreateSubMenuBtn("  ›  Quản lý role", () => LoadPage(CreateRolePage(), "Quản lý Vai trò"));
    var btnCreateRole = CreateSubMenuBtn("  ›  Tạo role mới", () => CreateRole(null, null));
    pnlRoleSubmenu.Controls.Add(btnCreateRole);
    pnlRoleSubmenu.Controls.Add(btnAllRoles);
    pnlRoleSubmenu.Height = pnlRoleSubmenu.Controls.Count * 45;

    var btnRoleHeader = CreateMenuBtn("🛡️  Vai trò (Role)  ▼", () => {
        pnlRoleSubmenu.Visible = !pnlRoleSubmenu.Visible;
    });

    // 4. MENU CON: CẤP QUYỀN (Phân quyền, Thu hồi, Xem quyền) - MỚI CẬP NHẬT
    // Lưu ý: Cần khai báo private Panel pnlPrivSubmenu; ở đầu class
    pnlPrivSubmenu = new Panel { Dock = DockStyle.Top, Height = 0, Visible = false, BackColor = Color.FromArgb(45, 50, 55) };
    
    var btnAssignPriv = CreateSubMenuBtn("  ›  Phân quyền hệ thống", () => LoadPage(CreatePrivPage(), "Phân quyền"));
    var btnRevokePriv = CreateSubMenuBtn("  ›  Thu hồi quyền", () => LoadPage(CreateRevokePage(), "Thu hồi quyền"));
    var btnViewPriv = CreateSubMenuBtn("  ›  Xem chi tiết quyền", () => LoadPage(CreateViewPrivPage(), "Chi tiết quyền")); // Trang xem riêng

    pnlPrivSubmenu.Controls.Add(btnViewPriv);
    pnlPrivSubmenu.Controls.Add(btnRevokePriv);
    pnlPrivSubmenu.Controls.Add(btnAssignPriv);
    pnlPrivSubmenu.Height = pnlPrivSubmenu.Controls.Count * 45;

    var btnPrivHeader = CreateMenuBtn("🔑  Cấp quyền  ▼", () => {
        pnlPrivSubmenu.Visible = !pnlPrivSubmenu.Visible;
    });

    // 5. ĐĂNG XUẤT
    var btnLogout = CreateMenuBtn("🚪  ĐĂNG XUẤT", () => this.Close());
    btnLogout.ForeColor = Color.Salmon;

    // 6. ADD VÀO SIDEBAR (Thứ tự hiển thị từ trên xuống)
    sidebar.Controls.Add(btnLogout);
    sidebar.Controls.Add(pnlPrivSubmenu); // Con của Cấp quyền
    sidebar.Controls.Add(btnPrivHeader);  // Cha của Cấp quyền
    sidebar.Controls.Add(pnlRoleSubmenu); 
    sidebar.Controls.Add(btnRoleHeader);  
    sidebar.Controls.Add(pnlUserSubmenu); 
    sidebar.Controls.Add(btnUserHeader);  
    sidebar.Controls.Add(lblLogo);

    // --- KHU VỰC NỘI DUNG BÊN PHẢI ---
    Panel rightArea = new Panel { Dock = DockStyle.Fill };
    mainLayout.Controls.Add(rightArea, 1, 0);

    Panel header = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = clrPrimary };
    lblHeaderTitle = new Label {
        Text = "DASHBOARD", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter,
        Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = Color.White
    };
    header.Controls.Add(lblHeaderTitle);
    rightArea.Controls.Add(header);

    contentPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30), BackColor = clrBackground };
    rightArea.Controls.Add(contentPanel);

    btnAllUsers.PerformClick();
}
                #endregion

                #region 4. CÁC TRANG CHỨC NĂNG

               private Control CreateUserPage()
{
    Panel p = new Panel();
    Panel card = CreateStyledCard("THÔNG TIN TÀI KHOẢN", Color.White, 180); // Giảm chiều cao card

    txtUser = AddInput(card, "Tên đăng nhập", 30, 75);
    txtPass = AddInput(card, "Mật khẩu mới", 260, 75, true);
    txtSearchUser = AddInput(card, "🔍 Tìm kiếm nhanh", 490, 75);

    // Các nút chức năng được sắp xếp sát nhau, không để khoảng cách thừa
    AddBtn(card, "🔧 ĐỔI PASS", clrPrimary, 30, 125, AlterUser);
    AddBtn(card, "❌ XÓA USER", clrDanger, 185, 125, DropUser);
    AddBtn(card, "🔄 LÀM MỚI", Color.DimGray, 340, 125, (s, e) => LoadUsers());

    InitGridInPage(p, card, 20); // Giảm margin top để UI khít hơn
    LoadUsers();
    return p;
}
private Control CreateViewPrivPage()
{
    Panel p = new Panel();
    Panel card = CreateStyledCard("TRA CỨU QUYỀN HỆ THỐNG", Color.White, 150);

    txtSearchUser = AddInput(card, "Nhập User/Role cần xem", 30, 75);
    AddBtn(card, "🔍 KIỂM TRA", clrPrimary, 260, 75, (s, e) => {
         // Logic gọi query hiển thị quyền của user vào Grid
    });

    InitGridInPage(p, card, 20);
    return p;
}
               private Control CreateRolePage()
{
    Panel p = new Panel();
    Panel card = CreateStyledCard("DANH SÁCH VAI TRÒ", Color.White, 180);

    txtRole = AddInput(card, "Tên Vai trò (Role)", 30, 75);
    txtSearchRole = AddInput(card, "🔍 Tìm Role", 260, 75);

    txtSearchRole.TextChanged += (s, e) => {
        if (roleTable != null) roleTable.DefaultView.RowFilter = $"ROLE LIKE '%{txtSearchRole.Text.ToUpper()}%'";
    };

    // Chỉ để lại 2 nút này
    AddBtn(card, "❌ XÓA ROLE", clrDanger, 30, 125, DropRole);
    AddBtn(card, "🔄 LÀM MỚI", Color.DimGray, 185, 125, (s, e) => LoadRoles());

    InitGridInPage(p, card, 20);
    LoadRoles();
    return p;
}
private ComboBox cbObject, cbColumn;
private Control CreatePrivPage()
{
    Panel p = new Panel();
    Panel card = CreateStyledCard("CẤU HÌNH QUYỀN TRÊN ĐỐI TƯỢNG", Color.White, 380);

    TableLayoutPanel layout = new TableLayoutPanel
    {
        Location = new Point(30, 55),
        Size = new Size(850, 170),
        ColumnCount = 3,
        RowCount = 4,
        BackColor = Color.Transparent
    };

    layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
    layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
    layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));

    layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F)); // Label 1
    layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 55F)); // Input 1
    layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F)); // Label 2
    layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 55F)); // Input 2

    // --- HÀNG 1 ---
    layout.Controls.Add(new Label { Text = "Đối tượng User", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray, Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomLeft }, 0, 0);
    layout.Controls.Add(new Label { Text = "Đối tượng Role", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray, Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomLeft }, 1, 0);
    layout.Controls.Add(new Label { Text = "Quyền hệ thống", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray, Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomLeft }, 2, 0);

    cbUser = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
    cbRole = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
    cbPrivilege = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
    cbPrivilege.Items.AddRange(new string[] { "SELECT", "INSERT", "UPDATE", "DELETE" });
    cbPrivilege.SelectedIndex = 0;

    layout.Controls.Add(cbUser, 0, 1);
    layout.Controls.Add(cbRole, 1, 1);
    layout.Controls.Add(cbPrivilege, 2, 1);

    // --- HÀNG 2 (CHUYỂN SANG COMBOBOX) ---
    layout.Controls.Add(new Label { Text = "Bảng / View (Chọn)", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray, Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomLeft }, 0, 2);
    layout.Controls.Add(new Label { Text = "Cột cụ thể (Chọn)", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray, Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomLeft }, 1, 2);

    cbObject = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
    cbColumn = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
    chkGrant = new CheckBox { Text = "With Grant Option", Font = new Font("Segoe UI", 9), Dock = DockStyle.Fill, AutoSize = true };

    // Sự kiện khi chọn Bảng xong sẽ tự load Cột
    cbObject.SelectedIndexChanged += (s, e) => LoadColumnsToCombo(cbObject.Text);

    layout.Controls.Add(cbObject, 0, 3);
    layout.Controls.Add(cbColumn, 1, 3);
    layout.Controls.Add(chkGrant, 2, 3);

    card.Controls.Add(layout);

    // --- NÚT BẤM ---
    int btnY = 245;
    AddBtn(card, "👤 CẤP USER", clrPrimary, 30, btnY, GrantUser);
    AddBtn(card, "🛡️ CẤP ROLE", Color.Orange, 180, btnY, GrantRole);
    AddBtn(card, "🔗 GÁN ROLE", Color.Purple, 330, btnY, GrantRoleToUser);
    AddBtn(card, "📄 XEM QUYỀN", Color.DodgerBlue, 480, btnY, LoadPrivileges);

    InitGridInPage(p, card, 260);

    // --- LOAD DỮ LIỆU BAN ĐẦU ---
    LoadUserToCombo();
    LoadRoleToCombo();
    LoadTablesToCombo(); // Hàm mới để load danh sách Bảng

    return p;
}

private Button CreateSubMenuBtn(string text, Action action)
{
    Button b = new Button {
        Text = text,
        Dock = DockStyle.Top,
        Height = 45,
        FlatStyle = FlatStyle.Flat,
        ForeColor = Color.Silver,
        BackColor = Color.FromArgb(45, 50, 55), // Màu tối hơn sidebar một chút
        Font = new Font("Segoe UI", 9),
        TextAlign = ContentAlignment.MiddleLeft,
        Cursor = Cursors.Hand,
        Padding = new Padding(35, 0, 0, 0) // Tạo độ thụt lề vào trong
    };
    b.FlatAppearance.BorderSize = 0;
    b.Click += (s, e) => action();
    
    // Hiệu ứng rê chuột (Hover)
    b.MouseEnter += (s, e) => { b.BackColor = Color.FromArgb(60, 65, 70); b.ForeColor = Color.White; };
    b.MouseLeave += (s, e) => { b.BackColor = Color.FromArgb(45, 50, 55); b.ForeColor = Color.Silver; };
    
    return b;
}

private void LoadTablesToCombo()
{
    try {
        using (var conn = db.GetConnection(currentUser, currentPass)) {
            conn.Open();
            // Thử dùng USER_TABLES nếu ALL_TABLES không ra dữ liệu
            string sql = "SELECT table_name FROM user_tables ORDER BY table_name";
            var cmd = new OracleCommand(sql, conn);
            var reader = cmd.ExecuteReader();

            cbObject.Items.Clear();
            while (reader.Read()) {
                cbObject.Items.Add(reader.GetString(0));
            }
            if (cbObject.Items.Count > 0) cbObject.SelectedIndex = 0;
        }
    } catch (Exception ex) { 
        // Thêm dòng này để biết chính xác lỗi gì nếu nó không hiện
        MessageBox.Show("Lỗi load bảng: " + ex.Message); 
    }
}

private void LoadColumnsToCombo(string tableName)
{
    if (string.IsNullOrEmpty(tableName)) return;
    try 
    {
        using (var conn = db.GetConnection(currentUser, currentPass)) 
        {
            conn.Open();
            string sql = $"SELECT column_name FROM all_tab_columns WHERE table_name = '{tableName.ToUpper()}' ORDER BY column_id";
            
            var cmd = new OracleCommand(sql, conn);
            var reader = cmd.ExecuteReader();
            
            cbColumn.Items.Clear();
            cbColumn.Items.Add("(Tất cả cột)"); // Option mặc định
            while (reader.Read()) 
            {
                cbColumn.Items.Add(reader.GetString(0));
            }
            cbColumn.SelectedIndex = 0;
        }
    } 
    catch (Exception ex) { MessageBox.Show("Lỗi load cột: " + ex.Message); }
}

                #endregion

                #region 5. DATABASE LOGIC (ORACLE)

                private void LoadUsers() {
                    try {
                        using (var conn = db.GetConnection(currentUser, currentPass)) {
                            conn.Open();
                            var da = new OracleDataAdapter("SELECT USERNAME, ACCOUNT_STATUS, CREATED FROM DBA_USERS ORDER BY CREATED DESC", conn);
                            userTable = new DataTable();
                            da.Fill(userTable);
                            grid.DataSource = userTable;
                        }
                    } catch (Exception ex) { MessageBox.Show(ex.Message); }
                }

private Control CreateRevokePage()
{
    Panel p = new Panel();
    Panel card = CreateStyledCard("THU HỒI QUYỀN", Color.White, 380);

    TableLayoutPanel layout = new TableLayoutPanel {
        Location = new Point(30, 60),
        Size = new Size(850, 170), 
        ColumnCount = 3, RowCount = 4,
        BackColor = Color.Transparent
    };

    layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
    layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
    layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));

    layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F)); 
    layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 55F)); 
    layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F)); 
    layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 55F)); 

    // --- Khởi tạo Control ---
    cbUser = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
    cbRole = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
    cbPrivilege = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
    cbPrivilege.Items.AddRange(new string[] { "SELECT", "INSERT", "UPDATE", "DELETE" });
    cbPrivilege.SelectedIndex = 0;

    cbObject = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
    cbColumn = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };

    // Sự kiện load cột tự động khi chọn bảng
    cbObject.SelectedIndexChanged += (s, e) => LoadColumnsToCombo(cbObject.Text);

    // --- Add vào Layout ---
    layout.Controls.Add(new Label { Text = "User", Font = new Font("Segoe UI", 9, FontStyle.Bold), TextAlign = ContentAlignment.BottomLeft, Dock = DockStyle.Fill }, 0, 0);
    layout.Controls.Add(new Label { Text = "Role", Font = new Font("Segoe UI", 9, FontStyle.Bold), TextAlign = ContentAlignment.BottomLeft, Dock = DockStyle.Fill }, 1, 0);
    layout.Controls.Add(new Label { Text = "Quyền", Font = new Font("Segoe UI", 9, FontStyle.Bold), TextAlign = ContentAlignment.BottomLeft, Dock = DockStyle.Fill }, 2, 0);
    layout.Controls.Add(cbUser, 0, 1);
    layout.Controls.Add(cbRole, 1, 1);
    layout.Controls.Add(cbPrivilege, 2, 1);

    layout.Controls.Add(new Label { Text = "Bảng / View (Chọn)", Font = new Font("Segoe UI", 9, FontStyle.Bold), TextAlign = ContentAlignment.BottomLeft, Dock = DockStyle.Fill }, 0, 2);
    layout.Controls.Add(new Label { Text = "Cột cụ thể (Chọn)", Font = new Font("Segoe UI", 9, FontStyle.Bold), TextAlign = ContentAlignment.BottomLeft, Dock = DockStyle.Fill }, 1, 2);
    layout.Controls.Add(cbObject, 0, 3);
    layout.Controls.Add(cbColumn, 1, 3);

    card.Controls.Add(layout);

    // --- Nút bấm ---
    int btnY = 250;
    AddBtn(card, "🚫 THU HỒI", clrDanger, 30, btnY, Revoke);
    AddBtn(card, "📄 XEM QUYỀN", Color.DodgerBlue, 180, btnY, LoadPrivileges);

    InitGridInPage(p, card, 260);

    // --- QUAN TRỌNG: Gọi load dữ liệu ở đây ---
    LoadUserToCombo();
    LoadRoleToCombo();
    LoadTablesToCombo(); 

    return p;
}
private void DropUser(object s, EventArgs e)
{
    if (string.IsNullOrWhiteSpace(txtUser.Text))
    {
        MessageBox.Show("Vui lòng chọn user!");
        return;
    }

    var confirm = new DropForm($"Bạn có chắc muốn xóa user '{txtUser.Text}'?");
    confirm.ShowDialog();

    if (confirm.IsConfirmed)
    {
        if (ExecuteSql($"DROP USER {txtUser.Text} CASCADE"))
        {
            LoadUsers();
            MessageBox.Show("Đã xóa user!");
        }
    }
}
                private void LoadRoles() {
                    try {
                        using (var conn = db.GetConnection(currentUser, currentPass)) {
                            conn.Open();
                            var da = new OracleDataAdapter("SELECT ROLE, PASSWORD_REQUIRED FROM DBA_ROLES", conn);
                            roleTable = new DataTable();
                            da.Fill(roleTable);
                            grid.DataSource = roleTable;
                        }
                    } catch { }
                }

                private bool ExecuteSql(string sql) {
                    try {
                        using (var conn = db.GetConnection(currentUser, currentPass)) {
                            conn.Open();
                            using (var cmd = new OracleCommand(sql, conn)) { cmd.ExecuteNonQuery(); }
                        }
                        return true;
                    } catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi SQL"); return false; }
                }

               private void CreateUser(object s, EventArgs e)
{
    using (FormCreateUser f = new FormCreateUser())
    {
        if (f.ShowDialog() == DialogResult.OK)
        {
            string user = f.Username;
            string pass = f.Password;

            if (ExecuteSql($"CREATE USER {user} IDENTIFIED BY {pass}"))
                LoadUsers();
        }
    }
}
private void AlterUser(object s, EventArgs e)
{
    if (string.IsNullOrWhiteSpace(txtUser.Text))
    {
        MessageBox.Show("Vui lòng chọn user!");
        return;
    }

    using (FormChangePassword f = new FormChangePassword())
    {
        if (f.ShowDialog() == DialogResult.OK)
        {
            ExecuteSql($"ALTER USER {txtUser.Text} IDENTIFIED BY {f.NewPassword}");
            MessageBox.Show("Đổi mật khẩu thành công!");
        }
    }
}
    
private void CreateRole(object s, EventArgs e)

{
    using (FormCreateRole f = new FormCreateRole())
    {
        if (f.ShowDialog() == DialogResult.OK)
        {
            if (ExecuteSql($"CREATE ROLE {f.RoleName}"))
                LoadRoles();
        }
    }
}

private void DropRole(object s, EventArgs e)
{
    if (string.IsNullOrWhiteSpace(txtRole.Text))
    {
        MessageBox.Show("Vui lòng chọn role!");
        return;
    }

    var confirm = new DropForm($"Bạn có chắc muốn xóa role '{txtRole.Text}'?");
    confirm.ShowDialog();

    if (confirm.IsConfirmed)
    {
        if (ExecuteSql($"DROP ROLE {txtRole.Text}"))
        {
            LoadRoles();
            MessageBox.Show("Đã xóa role!");
        }
    }
}

private void LoadUserToCombo()
{
    using (var conn = db.GetConnection(currentUser, currentPass))
    {
        conn.Open();
        var cmd = new OracleCommand("SELECT USERNAME FROM DBA_USERS", conn);
        var reader = cmd.ExecuteReader();

        cbUser.Items.Clear();
        while (reader.Read())
            cbUser.Items.Add(reader.GetString(0));
    }
}

private void LoadRoleToCombo()
{
    using (var conn = db.GetConnection(currentUser, currentPass))
    {
        conn.Open();
        var cmd = new OracleCommand("SELECT ROLE FROM DBA_ROLES", conn);
        var reader = cmd.ExecuteReader();

        cbRole.Items.Clear();
        while (reader.Read())
            cbRole.Items.Add(reader.GetString(0));
    }
}
      private string GetTarget()
{
    if (cbUser.SelectedItem != null)
        return cbUser.SelectedItem.ToString().ToUpper();

    if (cbRole.SelectedItem != null)
        return cbRole.SelectedItem.ToString().ToUpper();

    MessageBox.Show("Chọn User hoặc Role!");
    return null;
}

private void GrantUser(object s, EventArgs e)
{
    if (cbUser.SelectedItem == null)
    {
        MessageBox.Show("Chọn USER!");
        return;
    }

    if (string.IsNullOrWhiteSpace(txtObject.Text))
    {
        MessageBox.Show("Nhập Object!");
        return;
    }

    ExecuteSql(BuildGrantSql(cbUser.SelectedItem.ToString()));
}

private void GrantRole(object s, EventArgs e)
{
    if (cbRole.SelectedItem == null)
    {
        MessageBox.Show("Chọn ROLE!");
        return;
    }

    if (string.IsNullOrWhiteSpace(txtObject.Text))
    {
        MessageBox.Show("Nhập Object!");
        return;
    }

    ExecuteSql(BuildGrantSql(cbRole.SelectedItem.ToString()));
}

private void GrantRoleToUser(object s, EventArgs e)
{
    if (cbUser.SelectedItem == null || cbRole.SelectedItem == null)
    {
        MessageBox.Show("Chọn User và Role!");
        return;
    }

    ExecuteSql($"GRANT {cbRole.SelectedItem} TO {cbUser.SelectedItem}");
}

private void Revoke(object s, EventArgs e)
{
    string target = GetTarget();
    if (target == null) return;

    if (string.IsNullOrWhiteSpace(txtObject.Text))
    {
        MessageBox.Show("Nhập Object!");
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

    ExecuteSql(sql);
}
 private string BuildGrantSql(string target)
{
    string privilege = cbPrivilege.Text;
    string obj = txtObject.Text.ToUpper();
    string col = txtColumn.Text.Trim();
    string grantOpt = chkGrant.Checked ? " WITH GRANT OPTION" : "";

    if ((privilege == "SELECT" || privilege == "UPDATE") && col != "")
        return $"GRANT {privilege} ({col}) ON {obj} TO {target}{grantOpt}";

    return $"GRANT {privilege} ON {obj} TO {target}{grantOpt}";
}

                #endregion

                #region 6. HELPERS (UI)
void LoadPrivileges(object s, EventArgs e)
{
    string name = GetTarget();
    if (name == null) return;

    using (var conn = db.GetConnection(currentUser, currentPass))
    {
        conn.Open();

        string sql = $@"
        SELECT * FROM DBA_TAB_PRIVS WHERE GRANTEE = '{name}'
        ";

        var da = new OracleDataAdapter(sql, conn);
        DataTable dt = new DataTable();
        da.Fill(dt);
        grid.DataSource = dt;
    }
}
                private void LoadPage(Control page, string title)
                {
                    contentPanel.Controls.Clear();
                    lblHeaderTitle.Text = title.ToUpper();
                    page.Dock = DockStyle.Fill;
                    contentPanel.Controls.Add(page);
                }

                private Panel CreateStyledCard(string title, Color bg, int height)
                {
                    Panel card = new Panel { Dock = DockStyle.Top, Height = height, BackColor = bg, Padding = new Padding(20) };
                    card.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = clrSidebar, AutoSize = true, Location = new Point(20, 15) });
                    return card;
                }

                  private void Grid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
{
    if (e.RowIndex < 0) return;

    DataGridViewRow row = grid.Rows[e.RowIndex];

    // 👉 USER
    if (lblHeaderTitle.Text.Contains("NGƯỜI DÙNG"))
    {
        txtUser.Text = row.Cells["USERNAME"].Value.ToString();
        txtPass.Text = "";
        txtPass.Focus();
    }

    // 👉 ROLE
    if (lblHeaderTitle.Text.Contains("VAI TRÒ"))
    {
        txtRole.Text = row.Cells["ROLE"].Value.ToString();
    }
}   
  private void InitGridInPage(Panel container, Control card, int topMargin = 50)
{
    grid = new DataGridView {
     Dock = DockStyle.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.None,
    SelectionMode = DataGridViewSelectionMode.FullRowSelect, AllowUserToAddRows = false,
    RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
     EnableHeadersVisualStyles = false, RowTemplate = { Height = 35 }
};
    grid.ColumnHeadersDefaultCellStyle.BackColor = clrSidebar;
    grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
     grid.ColumnHeadersHeight = 40;

    Panel gBox = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, topMargin, 0, 0) };
                    gBox.Controls.Add(grid);
                    container.Controls.Add(gBox);
                    container.Controls.Add(card);
                    grid.CellDoubleClick += Grid_CellDoubleClick;
                }

                private Button CreateMenuBtn(string text, Action action)
                {
                    Button b = new Button {
                        Text = "    " + text, Dock = DockStyle.Top, Height = 60, FlatStyle = FlatStyle.Flat,
                        ForeColor = Color.Silver, Font = new Font("Segoe UI", 11), TextAlign = ContentAlignment.MiddleLeft, Cursor = Cursors.Hand
                    };
                    b.FlatAppearance.BorderSize = 0;
                    b.Click += (s, e) => {
                        foreach (Control c in b.Parent.Controls) if (c is Button btn) btn.BackColor = Color.Transparent;
                        b.BackColor = Color.FromArgb(50, 55, 60);
                        action();
                    };
                    return b;
                }

                private TextBox AddInput(Control p, string lab, int x, int y, bool pass = false)
                {
                    p.Controls.Add(new Label { Text = lab, Location = new Point(x, y - 22), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray });
                    TextBox t = new TextBox { Location = new Point(x, y), Width = 210, Font = new Font("Segoe UI", 11), UseSystemPasswordChar = pass };
                    p.Controls.Add(t);
                    return t;
                }

                private void AddBtn(Control p, string txt, Color c, int x, int y, EventHandler ev)
                {
                    Button b = new Button { Text = txt, Size = new Size(140, 40), Location = new Point(x, y), FlatStyle = FlatStyle.Flat, BackColor = c, ForeColor = Color.White, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
                    b.FlatAppearance.BorderSize = 0;
                    b.Click += ev;
                    p.Controls.Add(b);
                }

                #endregion
            }
        }