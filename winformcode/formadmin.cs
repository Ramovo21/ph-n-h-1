        using System;
        using System.Collections.Generic;
        using System.Data;
        using System.Drawing;
        using System.Windows.Forms;
        using Oracle.ManagedDataAccess.Client;
        using HospitalApp.Forms;
        using HospitalApp.Services;
        

        namespace HospitalApp
        {
            public class FormAdmin : Form
            {
                #region 1. BIẾN HỆ THỐNG & MÀU SẮC
                
                private readonly DBConnection db = new DBConnection();
                private readonly string currentUser;
                private readonly string currentPass;
                private Panel contentPanel = null!;
                private Label lblHeaderTitle = null!;
                private DataGridView grid = null!;

                private Panel pnlUserSubmenu = null!;

                // Controls nhập liệu
                private TextBox txtUser = null!;
                private TextBox txtPass = null!;
                private TextBox txtRole = null!;
                private TextBox txtSearchUser = null!;
                private TextBox txtSearchRole = null!;
                private ComboBox cbPrivilege = null!;
                private ComboBox cbUser = null!;
                private ComboBox cbRole = null!;
                private bool revokeRoleMembershipMode = false;
                private CheckBox chkGrant = null!;
                private DataTable? userTable;
                private DataTable? roleTable;

                private Panel pnlRoleSubmenu = null!;

                private Panel pnlPrivSubmenu = null!;

                private Panel pnlDataSubmenu = null!;

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
    var btnCreateUser = CreateSubMenuBtn("  ›  Tạo user mới", () => CreateUser(this, EventArgs.Empty));
    pnlUserSubmenu.Controls.Add(btnCreateUser);
    pnlUserSubmenu.Controls.Add(btnAllUsers);
    pnlUserSubmenu.Height = pnlUserSubmenu.Controls.Count * 45;

    var btnUserHeader = CreateMenuBtn("👤  Cấu hình User  ▼", () => {
        pnlUserSubmenu.Visible = !pnlUserSubmenu.Visible;
    });

    // 3. MENU CON: VAI TRÒ (Quản lý, Tạo mới)
    pnlRoleSubmenu = new Panel { Dock = DockStyle.Top, Height = 0, Visible = false, BackColor = Color.FromArgb(45, 50, 55) };
    var btnAllRoles = CreateSubMenuBtn("  ›  Quản lý role", () => LoadPage(CreateRolePage(), "Quản lý Vai trò"));
    var btnCreateRole = CreateSubMenuBtn("  ›  Tạo role mới", () => CreateRole(this, EventArgs.Empty));
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

    // 5. MENU CON: XEM DỮ LIỆU (Toàn bộ bảng nghiệp vụ)
    pnlDataSubmenu = new Panel { Dock = DockStyle.Top, Height = 0, Visible = false, BackColor = Color.FromArgb(45, 50, 55) };
    var btnViewNV   = CreateSubMenuBtn("  ›  Nhân viên",       () => LoadPage(CreateDataViewPage("BVOWNER.NHANVIEN", "SELECT * FROM BVOWNER.NHANVIEN ORDER BY MANV"), "Dữ liệu: Nhân viên"));
    var btnViewBN   = CreateSubMenuBtn("  ›  Bệnh nhân",       () => LoadPage(CreateDataViewPage("BVOWNER.BENHNHAN", "SELECT * FROM BVOWNER.BENHNHAN ORDER BY MABN"), "Dữ liệu: Bệnh nhân"));
    var btnViewHSBA = CreateSubMenuBtn("  ›  Hồ sơ bệnh án",   () => LoadPage(CreateDataViewPage("BVOWNER.HSBA", "SELECT * FROM BVOWNER.HSBA ORDER BY MAHSBA"), "Dữ liệu: Hồ sơ bệnh án"));
    var btnViewDT   = CreateSubMenuBtn("  ›  Đơn thuốc",       () => LoadPage(CreateDataViewPage("BVOWNER.DONTHUOC", "SELECT * FROM BVOWNER.DONTHUOC ORDER BY MAHSBA"), "Dữ liệu: Đơn thuốc"));
    var btnViewDV   = CreateSubMenuBtn("  ›  Dịch vụ (HSBA_DV)", () => LoadPage(CreateDataViewPage("BVOWNER.HSBA_DV", "SELECT * FROM BVOWNER.HSBA_DV ORDER BY MAHSBA"), "Dữ liệu: Dịch vụ hỗ trợ"));
    // Thêm vào panel (Dock=Top nên thứ tự ngược)
    pnlDataSubmenu.Controls.Add(btnViewDV);
    pnlDataSubmenu.Controls.Add(btnViewDT);
    pnlDataSubmenu.Controls.Add(btnViewHSBA);
    pnlDataSubmenu.Controls.Add(btnViewBN);
    pnlDataSubmenu.Controls.Add(btnViewNV);
    pnlDataSubmenu.Height = pnlDataSubmenu.Controls.Count * 45;

    var btnDataHeader = CreateMenuBtn("📊  Xem dữ liệu  ▼", () => {
        pnlDataSubmenu.Visible = !pnlDataSubmenu.Visible;
    });

    // 6. THÔNG BÁO (OLS)
    var btnNotice = CreateMenuBtn("📢  Thông báo (OLS)", () => LoadPage(CreateNoticePage(), "Quản lý Thông báo (OLS)"));

    // 7. ĐĂNG XUẤT
    var btnLogout = CreateMenuBtn("🚪  ĐĂNG XUẤT", () => {
        this.Hide();
        LoginForm login = new LoginForm();
        login.Show();
    });
    btnLogout.ForeColor = Color.Salmon;

    // 8. ADD VÀO SIDEBAR (Thứ tự hiển thị từ trên xuống - Dock=Top nên add ngược)
    sidebar.Controls.Add(btnLogout);
    sidebar.Controls.Add(btnNotice);       // Thông báo OLS
    sidebar.Controls.Add(pnlDataSubmenu);  // Con của Xem dữ liệu
    sidebar.Controls.Add(btnDataHeader);   // Cha của Xem dữ liệu
    sidebar.Controls.Add(pnlPrivSubmenu);  // Con của Cấp quyền
    sidebar.Controls.Add(btnPrivHeader);   // Cha của Cấp quyền
    sidebar.Controls.Add(pnlRoleSubmenu); 
    sidebar.Controls.Add(btnRoleHeader);  
    sidebar.Controls.Add(pnlUserSubmenu); 
    sidebar.Controls.Add(btnUserHeader);  
    sidebar.Controls.Add(lblLogo);

    // --- KHU VỰC NỘI DUNG BÊN PHẢI ---
    TableLayoutPanel rightArea = new TableLayoutPanel
    {
        Dock = DockStyle.Fill,
        ColumnCount = 1,
        RowCount = 2
    };
    rightArea.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
    rightArea.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F));
    rightArea.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
    mainLayout.Controls.Add(rightArea, 1, 0);

    Panel header = new Panel { Dock = DockStyle.Fill, BackColor = clrPrimary };
    lblHeaderTitle = new Label {
        Text = "DASHBOARD", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter,
        Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = Color.White
    };
    header.Controls.Add(lblHeaderTitle);
    rightArea.Controls.Add(header, 0, 0);

    contentPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30), BackColor = clrBackground };
    rightArea.Controls.Add(contentPanel, 0, 1);

    btnAllUsers.PerformClick();
}
                #endregion

                #region 4. CÁC TRANG CHỨC NĂNG

               private Control CreateUserPage()
{
    Panel p = new Panel();
    Panel card = CreateStyledCard("Tài khoản", Color.White, 180);

    txtUser = AddInput(card, "Tên đăng nhập", 30, 75);
    txtPass = AddInput(card, "Mật khẩu mới", 260, 75, true);
    txtSearchUser = AddInput(card, "🔍 Tìm kiếm nhanh", 490, 75);
    txtSearchUser.TextChanged += (s, e) => FilterUsers();

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
    Panel card = CreateStyledCard("Điều kiện tra cứu", Color.White, 150);

    txtSearchUser = AddInput(card, "Nhập User/Role cần xem", 30, 75);
    AddBtn(card, "🔍 KIỂM TRA", clrPrimary, 260, 75, SearchPrivilegeDetails);

    InitGridInPage(p, card, 20);
    return p;
}
private Control CreateRolePage()
{
    Panel p = new Panel();
    Panel card = CreateStyledCard("Bộ lọc", Color.White, 180);

    txtRole = AddInput(card, "Tên Vai trò (Role)", 30, 75);
    txtSearchRole = AddInput(card, "🔍 Tìm Role", 260, 75);

    txtSearchRole.TextChanged += (s, e) => {
        if (roleTable != null) roleTable.DefaultView.RowFilter = $"ROLE LIKE '%{txtSearchRole.Text.ToUpper()}%'";
    };
    Label lblRoleStatus = AddStatusLabel(card, 340, 128, 430);

    AddBtn(card, "❌ XÓA ROLE", clrDanger, 30, 125, DropRole);
    AddBtn(card, "🔄 LÀM MỚI", clrPrimary, 185, 125, (s, e) => LoadRolePageData(lblRoleStatus));

    InitGridInPage(p, card, 20);
    ShowGridMessage("Đang tải danh sách vai trò...");
    this.BeginInvoke(new Action(() => LoadRolePageData(lblRoleStatus)));
    return p;
}
private ComboBox cbObject = null!;
private ComboBox cbColumn = null!;
private Control CreatePrivPage()
{
    Panel p = new Panel();
    Panel card = new Panel { Dock = DockStyle.Top, Height = 430, BackColor = Color.White, Padding = new Padding(20) };
    AddSectionTitle(card, "Thông tin xử lý", 30, 20);

    Label subtitle = new Label
    {
        Text = "Chọn user hoặc role, quyền và đối tượng cần xử lý.",
        Location = new Point(30, 48),
        AutoSize = true,
        Font = new Font("Segoe UI", 9),
        ForeColor = Color.DimGray
    };
    card.Controls.Add(subtitle);

    Panel selectorPanel = new Panel
    {
        Location = new Point(30, 82),
        Size = new Size(930, 200),
        BackColor = Color.FromArgb(247, 249, 252),
        Padding = new Padding(18)
    };
    card.Controls.Add(selectorPanel);

    TableLayoutPanel layout = new TableLayoutPanel
    {
        Dock = DockStyle.Fill,
        ColumnCount = 3,
        RowCount = 4,
        BackColor = Color.Transparent
    };

    layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
    layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
    layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));

    layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
    layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
    layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
    layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));

    layout.Controls.Add(new Label { Text = "Đối tượng User", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray, Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomLeft }, 0, 0);
    layout.Controls.Add(new Label { Text = "Đối tượng Role", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray, Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomLeft }, 1, 0);
    layout.Controls.Add(new Label { Text = "Quyền hệ thống", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray, Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomLeft }, 2, 0);

    cbUser = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10), FlatStyle = FlatStyle.Flat };
    cbRole = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10), FlatStyle = FlatStyle.Flat };
    cbPrivilege = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10), FlatStyle = FlatStyle.Flat };
    cbPrivilege.Items.AddRange(new string[] { "SELECT", "INSERT", "UPDATE", "DELETE" });
    cbPrivilege.SelectedIndex = 0;

    layout.Controls.Add(cbUser, 0, 1);
    layout.Controls.Add(cbRole, 1, 1);
    layout.Controls.Add(cbPrivilege, 2, 1);

    layout.Controls.Add(new Label { Text = "Bảng / View (Chọn)", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray, Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomLeft }, 0, 2);
    layout.Controls.Add(new Label { Text = "Cột cụ thể (Chọn)", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray, Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomLeft }, 1, 2);

    cbObject = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10), FlatStyle = FlatStyle.Flat };
    cbColumn = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10), FlatStyle = FlatStyle.Flat };
    chkGrant = new CheckBox { Text = "Cho phép cấp tiếp", Font = new Font("Segoe UI", 9, FontStyle.Bold), Dock = DockStyle.Fill, AutoSize = false, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.FromArgb(55, 65, 81), Padding = new Padding(4, 8, 0, 0) };

    cbObject.SelectedIndexChanged += (s, e) => LoadColumnsToCombo(cbObject.Text);

    layout.Controls.Add(cbObject, 0, 3);
    layout.Controls.Add(cbColumn, 1, 3);
    layout.Controls.Add(chkGrant, 2, 3);

    selectorPanel.Controls.Add(layout);

    Label actionTitle = new Label
    {
        Text = "Thao tác",
        Location = new Point(30, 302),
        AutoSize = true,
        Font = new Font("Segoe UI", 9, FontStyle.Bold),
        ForeColor = Color.Gray
    };
    card.Controls.Add(actionTitle);

    Panel actionPanel = new Panel
    {
        Location = new Point(30, 326),
        Size = new Size(900, 48),
        BackColor = Color.Transparent
    };
    card.Controls.Add(actionPanel);

    Label lblPrivilegeStatus = AddStatusLabel(card, 30, 390, 760);

    AddBtn(actionPanel, "👤 CẤP USER", clrPrimary, 0, 0, GrantUser);
    AddBtn(actionPanel, "🛡️ CẤP ROLE", Color.Orange, 155, 0, GrantRole);
    AddBtn(actionPanel, "🔗 GÁN ROLE", Color.Purple, 310, 0, GrantRoleToUser);
    AddBtn(actionPanel, "📄 XEM QUYỀN", Color.DodgerBlue, 465, 0, LoadPrivileges);
    AddBtn(actionPanel, "🔄 LÀM MỚI", Color.Teal, 620, 0, (s, e) => LoadPrivilegeSetupData(lblPrivilegeStatus));

    InitGridInPage(p, card, 24);

    ShowGridMessage("Đang chuẩn bị dữ liệu phân quyền...");
    this.BeginInvoke(new Action(() => LoadPrivilegeSetupData(lblPrivilegeStatus)));

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

private DataTable RunQueryWithFallback(params string[] sqlOptions)
{
    Exception lastError = null;

    foreach (string sql in sqlOptions)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            continue;
        }

        try
        {
            using (var conn = db.GetConnection(currentUser, currentPass))
            {
                conn.Open();
                var dt = new DataTable();
                using (var da = new OracleDataAdapter(sql, conn))
                {
                    da.Fill(dt);
                }
                return dt;
            }
        }
        catch (Exception ex)
        {
            lastError = ex;
        }
    }

    if (lastError != null)
    {
        throw lastError;
    }

    return new DataTable();
}

private Label AddStatusLabel(Control parent, int x, int y, int width = 520)
{
    Label lbl = new Label
    {
        Location = new Point(x, y),
        Size = new Size(width, 24),
        Font = new Font("Segoe UI", 9, FontStyle.Bold),
        ForeColor = Color.DimGray,
        TextAlign = ContentAlignment.MiddleLeft,
        Text = "Sẵn sàng."
    };
    parent.Controls.Add(lbl);
    return lbl;
}

private void SetStatus(Label? label, string text, Color? color = null)
{
    if (label == null || label.IsDisposed)
    {
        return;
    }

    label.Text = text;
    label.ForeColor = color ?? Color.DimGray;
}

private void ShowGridMessage(string message)
{
    if (grid == null || grid.IsDisposed)
    {
        return;
    }

    DataTable infoTable = new DataTable();
    infoTable.Columns.Add("TRẠNG THÁI");
    infoTable.Rows.Add(message);
    grid.DataSource = infoTable;
}

private void SetBusyState(bool isBusy, params Control[] controls)
{
    Cursor = isBusy ? Cursors.WaitCursor : Cursors.Default;

    foreach (Control control in controls)
    {
        if (control != null && !control.IsDisposed)
        {
            control.Enabled = !isBusy;
        }
    }

    Application.DoEvents();
}

private List<string> LoadDatabaseObjectItems()
{
    List<string> items = new List<string>();

    using (var conn = db.GetConnection(currentUser, currentPass))
    {
        conn.Open();

        string sqlTables = "SELECT table_name FROM all_tables WHERE owner='BVOWNER' ORDER BY table_name";
        using (var reader = new OracleCommand(sqlTables, conn).ExecuteReader())
        {
            while (reader.Read()) items.Add(reader.GetString(0));
        }

        string sqlViews = "SELECT view_name FROM all_views WHERE owner='BVOWNER' ORDER BY view_name";
        using (var reader = new OracleCommand(sqlViews, conn).ExecuteReader())
        {
            while (reader.Read()) items.Add("[VIEW] " + reader.GetString(0));
        }

        string sqlProcs = "SELECT object_name, object_type FROM all_objects WHERE owner='BVOWNER' AND object_type IN ('PROCEDURE','FUNCTION','PACKAGE') ORDER BY object_type, object_name";
        using (var reader = new OracleCommand(sqlProcs, conn).ExecuteReader())
        {
            while (reader.Read()) items.Add("[" + reader.GetString(1) + "] " + reader.GetString(0));
        }
    }

    return items;
}

private void PopulateComboBox(ComboBox comboBox, IEnumerable<string> items, bool selectFirstItem = true)
{
    if (comboBox == null || comboBox.IsDisposed)
    {
        return;
    }

    comboBox.Items.Clear();
    foreach (string item in items)
    {
        comboBox.Items.Add(item);
    }

    if (selectFirstItem && comboBox.Items.Count > 0)
    {
        comboBox.SelectedIndex = 0;
    }
}

private void LoadRolePageData(Label statusLabel)
{
    try
    {
        SetStatus(statusLabel, "Đang tải danh sách role...", clrPrimary);
        ShowGridMessage("Đang tải danh sách vai trò, vui lòng chờ...");
        SetBusyState(true);
        LoadRoles();
        int count = roleTable?.Rows.Count ?? 0;
        SetStatus(statusLabel, $"Đã tải {count} role.", count > 0 ? clrSuccess : Color.DarkOrange);
        if (count == 0)
        {
            ShowGridMessage("Không có role nào hiển thị được với tài khoản hiện tại.");
        }
    }
    catch (Exception ex)
    {
        SetStatus(statusLabel, "Tải vai trò thất bại.", clrDanger);
        ShowGridMessage("Không tải được danh sách vai trò.");
        MessageBox.Show("Lỗi tải vai trò: " + ex.Message);
    }
    finally
    {
        SetBusyState(false);
    }
}

private void LoadPrivilegeSetupData(Label statusLabel, bool clearTargetSelection = false)
{
    try
    {
        SetStatus(statusLabel, "Đang tải user, role và đối tượng...", clrPrimary);
        ShowGridMessage("Đang tải dữ liệu phân quyền, vui lòng chờ...");
        SetBusyState(true, cbUser, cbRole, cbObject, cbColumn, cbPrivilege);

        DataTable users = RunQueryWithFallback(
            "SELECT USERNAME FROM DBA_USERS ORDER BY USERNAME",
            "SELECT USERNAME FROM ALL_USERS ORDER BY USERNAME"
        );
        DataTable roles = RunQueryWithFallback(
            "SELECT ROLE FROM DBA_ROLES WHERE ROLE LIKE 'ROLE_%' ORDER BY ROLE",
            @"SELECT ROLE
              FROM (
                  SELECT DISTINCT GRANTEE AS ROLE
                  FROM ALL_TAB_PRIVS
                  WHERE GRANTEE LIKE 'ROLE_%'
                  UNION
                  SELECT DISTINCT GRANTEE AS ROLE
                  FROM ALL_COL_PRIVS
                  WHERE GRANTEE LIKE 'ROLE_%'
              )
              ORDER BY ROLE"
        );
        List<string> objects = LoadDatabaseObjectItems();

        PopulateComboBox(cbUser, GetColumnValues(users, "USERNAME"), !clearTargetSelection);
        PopulateComboBox(cbRole, GetColumnValues(roles, "ROLE"), !clearTargetSelection);
        PopulateComboBox(cbObject, objects);
        revokeRoleMembershipMode = false;

        if (cbObject.SelectedItem != null)
        {
            LoadColumnsToCombo(cbObject.Text);
        }
        else
        {
            cbColumn.Items.Clear();
            cbColumn.Items.Add("(Tất cả cột)");
            cbColumn.SelectedIndex = 0;
        }

        SetStatus(statusLabel, $"Đã tải {cbUser.Items.Count} user, {cbRole.Items.Count} role, {cbObject.Items.Count} đối tượng.", clrSuccess);
        ShowGridMessage("Đã sẵn sàng phân quyền. Chọn đối tượng để tiếp tục.");
    }
    catch (Exception ex)
    {
        SetStatus(statusLabel, "Tải dữ liệu phân quyền thất bại.", clrDanger);
        ShowGridMessage("Không tải được dữ liệu phân quyền.");
        MessageBox.Show("Lỗi tải dữ liệu phân quyền: " + ex.Message);
    }
    finally
    {
        SetBusyState(false, cbUser, cbRole, cbObject, cbColumn, cbPrivilege);
    }
}

private List<string> GetColumnValues(DataTable table, string columnName)
{
    List<string> values = new List<string>();

    if (table == null || !table.Columns.Contains(columnName))
    {
        return values;
    }

    foreach (DataRow row in table.Rows)
    {
        values.Add(row[columnName]?.ToString() ?? string.Empty);
    }

    return values;
}

private string EscapeSqlLiteral(string value)
{
    return (value ?? string.Empty).Replace("'", "''").ToUpper();
}

private string NormalizeSelectedObjectName(string rawObject)
{
    if (string.IsNullOrWhiteSpace(rawObject))
    {
        return string.Empty;
    }

    string name = rawObject.Trim();
    int prefixEnd = name.IndexOf("] ");
    if (prefixEnd >= 0)
    {
        name = name.Substring(prefixEnd + 2);
    }

    name = name.Trim();
    if (name.Contains("."))
    {
        return name.ToUpper();
    }

    return $"BVOWNER.{name.ToUpper()}";
}

private bool IsExecutableObject(string rawObject)
{
    if (string.IsNullOrWhiteSpace(rawObject))
    {
        return false;
    }

    return rawObject.StartsWith("[PROCEDURE", StringComparison.OrdinalIgnoreCase)
        || rawObject.StartsWith("[FUNCTION", StringComparison.OrdinalIgnoreCase)
        || rawObject.StartsWith("[PACKAGE", StringComparison.OrdinalIgnoreCase);
}

private void LoadTablesToCombo()
{
    try {
        PopulateComboBox(cbObject, LoadDatabaseObjectItems());
    } catch (Exception ex) { 
        MessageBox.Show("Lỗi load đối tượng: " + ex.Message); 
    }
}

private void LoadColumnsToCombo(string tableName)
{
    if (string.IsNullOrEmpty(tableName)) return;
    if (IsExecutableObject(tableName))
    {
        cbColumn.Items.Clear();
        cbColumn.Items.Add("(Không áp dụng)");
        cbColumn.SelectedIndex = 0;
        return;
    }

    try 
    {
        using (var conn = db.GetConnection(currentUser, currentPass)) 
        {
            conn.Open();
            string normalizedObject = NormalizeSelectedObjectName(tableName);
            string objectName = normalizedObject.Contains(".")
                ? normalizedObject.Substring(normalizedObject.IndexOf('.') + 1)
                : normalizedObject;
            string sql = $"SELECT column_name FROM all_tab_columns WHERE owner = 'BVOWNER' AND table_name = '{EscapeSqlLiteral(objectName)}' ORDER BY column_id";
            
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

private void FilterUsers()
{
    if (userTable == null || txtSearchUser == null)
    {
        return;
    }

    string keyword = txtSearchUser.Text.Trim().Replace("'", "''").ToUpper();

    if (string.IsNullOrEmpty(keyword))
    {
        userTable.DefaultView.RowFilter = "";
        return;
    }

    userTable.DefaultView.RowFilter =
        $"USERNAME LIKE '%{keyword}%' OR ACCOUNT_STATUS LIKE '%{keyword}%'";
}

                // ============= TRANG THÔNG BÁO (OLS) =============
                private Control CreateNoticePage()
                {
                    Panel p = new Panel();
                    Panel card = CreateStyledCard("Thiết lập phát hành", Color.White, 320);

                    Label hint = new Label {
                        Text = "Tạo thông báo và gán nhãn OLS để hệ thống tự lọc đúng nhóm người nhận.",
                        Location = new Point(20, 42),
                        AutoSize = true,
                        Font = new Font("Segoe UI", 9),
                        ForeColor = Color.DimGray
                    };
                    card.Controls.Add(hint);

                    var txtMatb = AddInput(card, "Mã thông báo", 30, 92);
                    txtMatb.Width = 180;
                    var txtDiadiem = AddInput(card, "Địa điểm", 235, 92);
                    txtDiadiem.Width = 180;

                    card.Controls.Add(new Label { Text = "OLS Label", Location = new Point(440, 70), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray });
                    var cbLabel = new ComboBox {
                        Location = new Point(440, 92), Width = 330, DropDownStyle = ComboBoxStyle.DropDownList,
                        Font = new Font("Segoe UI", 10),
                        FlatStyle = FlatStyle.Flat
                    };
                    cbLabel.Items.AddRange(new string[] {
                        "BGD::CSALL (Ban Giám Đốc)",
                        "LDK::CSALL (Lãnh Đạo Khoa - Tất cả)",
                        "LDK:TH:CSALL (LDK Tiêu Hóa)",
                        "LDK:TH,TK:HP (LDK TH+TK Hải Phòng)",
                        "NV (Nhân Viên - Tất cả)",
                        "LDK (Lãnh Đạo Khoa)",
                        "NV:TH:HCM",
                        "NV:TH:HN"
                    });
                    cbLabel.SelectedIndex = 0;
                    card.Controls.Add(cbLabel);

                    card.Controls.Add(new Label { Text = "Nội dung", Location = new Point(30, 138), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray });
                    var txtNoidung = new TextBox {
                        Location = new Point(30, 160),
                        Size = new Size(740, 70),
                        Font = new Font("Segoe UI", 10),
                        Multiline = true,
                        ScrollBars = ScrollBars.Vertical
                    };
                    card.Controls.Add(txtNoidung);

                    Label lblNoticeStatus = AddStatusLabel(card, 30, 284, 740);

                    AddBtn(card, "➕ TẠO MỚI", clrSuccess, 30, 238, (s, e) => {
                        if (string.IsNullOrWhiteSpace(txtMatb.Text) || string.IsNullOrWhiteSpace(txtNoidung.Text)) {
                            MessageBox.Show("Nhập mã TB và nội dung!"); return;
                        }
                        string labelText = cbLabel.Text.Split('(')[0].Trim();
                        int labelTag;
                        if (!TryGetOlsLabelTag(labelText, out labelTag))
                        {
                            MessageBox.Show("Label OLS này chưa được tạo trong chính sách BV_POLICY. Vui lòng chọn label khác hoặc bổ sung label trong script OLS.");
                            return;
                        }

                        string matb = txtMatb.Text.Trim().Replace("'", "''");
                        string noidung = txtNoidung.Text.Trim().Replace("'", "''");
                        string diadiem = txtDiadiem.Text.Trim().Replace("'", "''");
                        string sqlInsert = $"INSERT INTO BVOWNER.THONGBAO (MATB, NOIDUNG, NGAYGIO, DIADIEM) VALUES ('{matb}', N'{noidung}', SYSTIMESTAMP, N'{diadiem}')";
                        if (ExecuteSql(sqlInsert)) {
                            try {
                                using (var conn = db.GetConnection(currentUser, currentPass)) {
                                    conn.Open();
                                    string sqlLabel = $"UPDATE BVOWNER.THONGBAO SET OLS_LABEL = {labelTag} WHERE MATB = '{matb}'";
                                    new OracleCommand(sqlLabel, conn).ExecuteNonQuery();
                                }
                            } catch (Exception ex) { MessageBox.Show("Lỗi gán label: " + ex.Message); }
                            LoadNotices(lblNoticeStatus);
                            SetStatus(lblNoticeStatus, "Đã tạo thông báo và gán OLS label.", clrSuccess);
                            MessageBox.Show("Tạo thông báo thành công!");
                        }
                    });
                    AddBtn(card, "❌ XÓA", clrDanger, 185, 238, (s, e) => {
                        if (string.IsNullOrWhiteSpace(txtMatb.Text)) { MessageBox.Show("Chọn thông báo cần xóa!"); return; }
                        string matb = txtMatb.Text.Trim().Replace("'", "''");
                        if (ExecuteSql($"DELETE FROM BVOWNER.THONGBAO WHERE MATB = '{matb}'")) {
                            LoadNotices(lblNoticeStatus);
                            SetStatus(lblNoticeStatus, "Đã xóa thông báo.", clrDanger);
                            MessageBox.Show("Đã xóa!");
                        }
                    });
                    AddBtn(card, "🔄 LÀM MỚI", Color.DimGray, 340, 238, (s, e) => {
                        LoadNotices(lblNoticeStatus);
                        SetStatus(lblNoticeStatus, "Đã tải lại danh sách thông báo.", clrPrimary);
                    });

                    InitGridInPage(p, card, 24);

                    // Double-click grid to select
                    grid.CellDoubleClick += (s, e2) => {
                        if (e2.RowIndex < 0) return;
                        var row = grid.Rows[e2.RowIndex];
                        txtMatb.Text = row.Cells["MATB"].Value?.ToString() ?? "";
                        txtNoidung.Text = row.Cells["NOIDUNG"].Value?.ToString() ?? "";
                        txtDiadiem.Text = row.Cells["DIADIEM"].Value?.ToString() ?? "";
                    };

                    this.BeginInvoke(new Action(() => LoadNotices(lblNoticeStatus)));
                    return p;
                }

                private void LoadNotices(Label? statusLabel = null)
                {
                    try
                    {
                        SetStatus(statusLabel, "Đang tải danh sách thông báo...", clrPrimary);

                        DataTable dt = RunQueryWithFallback(
                            @"SELECT t.MATB, t.NOIDUNG, t.NGAYGIO, t.DIADIEM,
                                     l.LABEL AS OLS_LABEL_TEXT
                              FROM BVOWNER.THONGBAO t
                              LEFT JOIN DBA_SA_LABELS l ON l.LABEL_TAG = t.OLS_LABEL
                                                       AND l.POLICY_NAME = 'BV_POLICY'
                              ORDER BY t.MATB",
                            @"SELECT t.MATB, t.NOIDUNG, t.NGAYGIO, t.DIADIEM,
                                     CASE t.OLS_LABEL
                                         WHEN 30000 THEN 'BGD::CSALL'
                                         WHEN 20000 THEN 'LDK::CSALL'
                                         WHEN 20100 THEN 'LDK:TH:CSALL'
                                         WHEN 20210 THEN 'LDK:TH,TK:HP'
                                         WHEN 10001 THEN 'NV'
                                         WHEN 20001 THEN 'LDK'
                                         WHEN 10110 THEN 'NV:TH:HCM'
                                         WHEN 10130 THEN 'NV:TH:HN'
                                         ELSE TO_CHAR(t.OLS_LABEL)
                                     END AS OLS_LABEL_TEXT
                              FROM BVOWNER.THONGBAO t
                              ORDER BY t.MATB",
                            @"SELECT MATB, NOIDUNG, NGAYGIO, DIADIEM
                              FROM BVOWNER.THONGBAO
                              ORDER BY MATB"
                        );

                        grid.DataSource = dt;
                        SetStatus(statusLabel, $"Đã tải {dt.Rows.Count} thông báo.", dt.Rows.Count > 0 ? clrSuccess : Color.DarkOrange);
                    }
                    catch (Exception ex)
                    {
                        ShowGridMessage("Không tải được thông báo OLS. Kiểm tra bảng BVOWNER.THONGBAO và quyền SELECT/INSERT/DELETE cho tài khoản đang đăng nhập.");
                        SetStatus(statusLabel, "Không tải được thông báo OLS.", clrDanger);
                        MessageBox.Show("Không tải được thông báo OLS.\n\n" +
                                        "Nguyên nhân thường gặp: chưa tạo bảng BVOWNER.THONGBAO, chưa chạy script OLS, hoặc tài khoản hiện tại chưa được cấp quyền trên bảng này.\n\n" +
                                        "Chi tiết: " + ex.Message,
                                        "Thông báo OLS");
                    }
                }

                private bool TryGetOlsLabelTag(string labelText, out int labelTag)
                {
                    Dictionary<string, int> labelTags = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "BGD::CSALL", 30000 },
                        { "LDK::CSALL", 20000 },
                        { "LDK:TH:CSALL", 20100 },
                        { "LDK:TH,TK:HP", 20210 },
                        { "NV", 10001 },
                        { "LDK", 20001 },
                        { "NV:TH:HCM", 10110 },
                        { "NV:TH:HN", 10130 }
                    };

                    return labelTags.TryGetValue(labelText.Trim(), out labelTag);
                }

                // ============= TRANG XEM DỮ LIỆU (BV_ADMIN bypass VPD) =============
                private Control CreateDataViewPage(string tableName, string sql)
                {
                    Panel p = new Panel();
                    string shortName = tableName.Contains(".") ? tableName.Split('.')[1] : tableName;
                    Panel card = CreateStyledCard("Bộ lọc", Color.White, 150);

                    Label lblCount = new Label {
                        Text = "", Location = new Point(30, 55), AutoSize = true,
                        Font = new Font("Segoe UI", 10), ForeColor = Color.DimGray
                    };
                    card.Controls.Add(lblCount);

                    Label lblSearch = new Label {
                        Text = "Tìm kiếm dữ liệu",
                        Location = new Point(30, 82),
                        AutoSize = true,
                        Font = new Font("Segoe UI", 9, FontStyle.Bold),
                        ForeColor = Color.Gray
                    };
                    card.Controls.Add(lblSearch);

                    Panel searchBox = new Panel {
                        Location = new Point(30, 102),
                        Size = new Size(430, 36),
                        BackColor = Color.FromArgb(237, 242, 247),
                        Padding = new Padding(12, 8, 12, 6)
                    };

                    TextBox txtSearch = new TextBox {
                        BorderStyle = BorderStyle.None,
                        Dock = DockStyle.Fill,
                        Font = new Font("Segoe UI", 10),
                        BackColor = searchBox.BackColor,
                        ForeColor = Color.FromArgb(55, 65, 81),
                        Text = "Nhập từ khóa để lọc..."
                    };

                    bool isSearchPlaceholder = true;
                    txtSearch.Enter += (s, e) =>
                    {
                        if (!isSearchPlaceholder) return;
                        isSearchPlaceholder = false;
                        txtSearch.Text = "";
                        txtSearch.ForeColor = Color.FromArgb(17, 24, 39);
                    };
                    txtSearch.Leave += (s, e) =>
                    {
                        if (!string.IsNullOrWhiteSpace(txtSearch.Text)) return;
                        isSearchPlaceholder = true;
                        txtSearch.Text = "Nhập từ khóa để lọc...";
                        txtSearch.ForeColor = Color.FromArgb(107, 114, 128);
                    };
                    searchBox.Controls.Add(txtSearch);
                    card.Controls.Add(searchBox);

                    AddBtn(card, "🔄 LÀM MỚI", Color.DimGray, 650, 95, (s, e) => LoadDataView(sql, lblCount, txtSearch));

                    InitGridInPage(p, card, 10);
                    txtSearch.TextChanged += (s, e) =>
                    {
                        if (!(grid.DataSource is DataTable dt)) return;
                        ApplyGridFilter(dt, isSearchPlaceholder ? "" : txtSearch.Text, lblCount);
                    };

                    LoadDataView(sql, lblCount, txtSearch);
                    return p;
                }

                private void LoadDataView(string sql, Label lblCount, TextBox txtSearch)
                {
                    try {
                        using (var conn = db.GetConnection(currentUser, currentPass)) {
                            conn.Open();
                            var da = new OracleDataAdapter(sql, conn);
                            DataTable dt = new DataTable();
                            da.Fill(dt);
                            grid.DataSource = dt;
                            ApplyGridFilter(dt, txtSearch.Text == "Nhập từ khóa để lọc..." ? "" : txtSearch.Text, lblCount);
                        }
                    } catch (Exception ex) { MessageBox.Show("Lỗi load dữ liệu: " + ex.Message); }
                }

                private void ApplyGridFilter(DataTable dt, string keyword, Label lblCount)
                {
                    if (dt == null) return;

                    string safeKeyword = keyword.Trim().Replace("'", "''");
                    if (string.IsNullOrWhiteSpace(safeKeyword))
                    {
                        dt.DefaultView.RowFilter = "";
                    }
                    else
                    {
                        List<string> filters = new List<string>();
                        foreach (DataColumn col in dt.Columns)
                        {
                            if (col.DataType == typeof(string))
                            {
                                filters.Add($"CONVERT([{col.ColumnName}], 'System.String') LIKE '%{safeKeyword}%'");
                            }
                            else
                            {
                                filters.Add($"CONVERT([{col.ColumnName}], 'System.String') LIKE '%{safeKeyword}%'");
                            }
                        }

                        dt.DefaultView.RowFilter = string.Join(" OR ", filters);
                    }

                    lblCount.Text = $"Tổng số dòng: {dt.DefaultView.Count}";
                }

                #endregion

                #region 5. DATABASE LOGIC (ORACLE)

                private void LoadUsers() {
                    try {
                        userTable = RunQueryWithFallback(
                            "SELECT USERNAME, ACCOUNT_STATUS, CREATED FROM DBA_USERS ORDER BY CREATED DESC",
                            "SELECT USERNAME, NULL AS ACCOUNT_STATUS, CREATED FROM ALL_USERS ORDER BY CREATED DESC"
                        );
                        grid.DataSource = userTable;
                        FilterUsers();
                    } catch (Exception ex) { MessageBox.Show(ex.Message); }
                }

private Control CreateRevokePage()
{
    Panel p = new Panel();
    Panel card = new Panel { Dock = DockStyle.Top, Height = 420, BackColor = Color.White, Padding = new Padding(20) };
    AddSectionTitle(card, "Thông tin xử lý", 30, 20);

    Label subtitle = new Label
    {
        Text = "Chọn user hoặc role, quyền và đối tượng cần xử lý.",
        Location = new Point(30, 48),
        AutoSize = true,
        Font = new Font("Segoe UI", 9),
        ForeColor = Color.DimGray
    };
    card.Controls.Add(subtitle);

    Panel selectorPanel = new Panel
    {
        Location = new Point(30, 82),
        Size = new Size(930, 200),
        BackColor = Color.FromArgb(247, 249, 252),
        Padding = new Padding(18)
    };
    card.Controls.Add(selectorPanel);

    TableLayoutPanel layout = new TableLayoutPanel {
        Dock = DockStyle.Fill,
        ColumnCount = 3,
        RowCount = 4,
        BackColor = Color.Transparent
    };

    layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
    layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
    layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));

    layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F)); 
    layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F)); 
    layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F)); 
    layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F)); 

    cbUser = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10), FlatStyle = FlatStyle.Flat };
    cbRole = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10), FlatStyle = FlatStyle.Flat };
    cbPrivilege = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10), FlatStyle = FlatStyle.Flat };
    cbPrivilege.Items.AddRange(new string[] { "SELECT", "INSERT", "UPDATE", "DELETE" });
    cbPrivilege.SelectedIndex = 0;

    cbObject = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10), FlatStyle = FlatStyle.Flat };
    cbColumn = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10), FlatStyle = FlatStyle.Flat };

    cbObject.SelectedIndexChanged += (s, e) => LoadColumnsToCombo(cbObject.Text);

    layout.Controls.Add(new Label { Text = "Đối tượng User", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray, TextAlign = ContentAlignment.BottomLeft, Dock = DockStyle.Fill }, 0, 0);
    layout.Controls.Add(new Label { Text = "Đối tượng Role", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray, TextAlign = ContentAlignment.BottomLeft, Dock = DockStyle.Fill }, 1, 0);
    layout.Controls.Add(new Label { Text = "Quyền cần thu hồi", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray, TextAlign = ContentAlignment.BottomLeft, Dock = DockStyle.Fill }, 2, 0);
    layout.Controls.Add(cbUser, 0, 1);
    layout.Controls.Add(cbRole, 1, 1);
    layout.Controls.Add(cbPrivilege, 2, 1);

    layout.Controls.Add(new Label { Text = "Bảng / View (Chọn)", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray, TextAlign = ContentAlignment.BottomLeft, Dock = DockStyle.Fill }, 0, 2);
    layout.Controls.Add(new Label { Text = "Cột cụ thể (Chọn)", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray, TextAlign = ContentAlignment.BottomLeft, Dock = DockStyle.Fill }, 1, 2);
    layout.Controls.Add(cbObject, 0, 3);
    layout.Controls.Add(cbColumn, 1, 3);

    selectorPanel.Controls.Add(layout);

    Label actionTitle = new Label
    {
        Text = "Thao tác",
        Location = new Point(30, 302),
        AutoSize = true,
        Font = new Font("Segoe UI", 9, FontStyle.Bold),
        ForeColor = Color.Gray
    };
    card.Controls.Add(actionTitle);

    Panel actionPanel = new Panel
    {
        Location = new Point(30, 326),
        Size = new Size(900, 48),
        BackColor = Color.Transparent
    };
    card.Controls.Add(actionPanel);

    Label lblRevokeStatus = AddStatusLabel(card, 30, 390, 760);

    AddBtn(actionPanel, "🚫 THU HỒI", clrDanger, 0, 0, Revoke);
    AddBtn(actionPanel, "📄 XEM QUYỀN", Color.DodgerBlue, 155, 0, LoadPrivileges);
    AddBtn(actionPanel, "🔄 LÀM MỚI", Color.Teal, 310, 0, (s, e) => LoadPrivilegeSetupData(lblRevokeStatus, true));

    InitGridInPage(p, card, 24);

    ShowGridMessage("Đang chuẩn bị dữ liệu thu hồi quyền...");
    this.BeginInvoke(new Action(() => LoadPrivilegeSetupData(lblRevokeStatus, true)));

    grid.DoubleClick += (s, e) =>
    {
        if (grid.SelectedRows.Count == 0) return;
        DataGridViewRow row = grid.SelectedRows[0];
        string loai = row.Cells["LOAI"].Value?.ToString() ?? "";
        string privilege = row.Cells["PRIVILEGE"].Value?.ToString() ?? "";
        string doiTuong = row.Cells["DOI_TUONG"].Value?.ToString() ?? "";
        string cot = row.Cells["COT"].Value?.ToString() ?? "";

        if (loai == "ROLE")
        {
            if (!string.IsNullOrWhiteSpace(privilege))
            {
                cbRole.Text = privilege;
                revokeRoleMembershipMode = true;
            }
            return;
        }

        cbPrivilege.Text = privilege;
        if (!string.IsNullOrWhiteSpace(doiTuong))
        {
            cbObject.Text = doiTuong;
        }
        cbColumn.Text = string.IsNullOrWhiteSpace(cot) ? "(Tất cả cột)" : cot;
    };

    return p;
}
private void DropUser(object? s, EventArgs e)
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
                        roleTable = RunQueryWithFallback(
                            "SELECT ROLE, PASSWORD_REQUIRED FROM DBA_ROLES WHERE ROLE LIKE 'ROLE_%' ORDER BY ROLE",
                            @"SELECT ROLE, PASSWORD_REQUIRED
                              FROM (
                                  SELECT DISTINCT GRANTEE AS ROLE, 'UNKNOWN' AS PASSWORD_REQUIRED
                                  FROM ALL_TAB_PRIVS
                                  WHERE GRANTEE LIKE 'ROLE_%'
                                  UNION
                                  SELECT DISTINCT GRANTEE AS ROLE, 'UNKNOWN' AS PASSWORD_REQUIRED
                                  FROM ALL_COL_PRIVS
                                  WHERE GRANTEE LIKE 'ROLE_%'
                              )
                              ORDER BY ROLE"
                        );
                        grid.DataSource = roleTable;
                    } catch (Exception ex) { MessageBox.Show("Lỗi load role: " + ex.Message); }
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

               private void CreateUser(object? s, EventArgs e)
{
    using (FormCreateUser f = new FormCreateUser())
    {
        if (f.ShowDialog() == DialogResult.OK)
        {
            string user = f.Username;
            string pass = f.Password;

            if (ExecuteSql($"CREATE USER {user} IDENTIFIED BY {pass}"))
            {
                LoadUsers();
                MessageBox.Show("Tạo user thành công!", "Thông báo");
            }
        }
    }
}
private void AlterUser(object? s, EventArgs e)
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
    
private void CreateRole(object? s, EventArgs e)
{
    using (FormCreateRole f = new FormCreateRole())
    {
        if (f.ShowDialog() == DialogResult.OK)
        {
            if (ExecuteSql($"CREATE ROLE {f.RoleName}"))
            {
                LoadRoles();
                MessageBox.Show("Tạo role thành công!", "Thông báo");
            }
        }
    }
}

private void DropRole(object? s, EventArgs e)
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
    try
    {
        var dt = RunQueryWithFallback(
            "SELECT USERNAME FROM DBA_USERS ORDER BY USERNAME",
            "SELECT USERNAME FROM ALL_USERS ORDER BY USERNAME"
        );

        cbUser.Items.Clear();
        foreach (DataRow row in dt.Rows)
        {
            cbUser.Items.Add(row["USERNAME"].ToString());
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show("Lỗi load user: " + ex.Message);
    }
}

private void LoadRoleToCombo()
{
    try
    {
        var dt = RunQueryWithFallback(
            "SELECT ROLE FROM DBA_ROLES WHERE ROLE LIKE 'ROLE_%' ORDER BY ROLE",
            @"SELECT ROLE
              FROM (
                  SELECT DISTINCT GRANTEE AS ROLE
                  FROM ALL_TAB_PRIVS
                  WHERE GRANTEE LIKE 'ROLE_%'
                  UNION
                  SELECT DISTINCT GRANTEE AS ROLE
                  FROM ALL_COL_PRIVS
                  WHERE GRANTEE LIKE 'ROLE_%'
              )
              ORDER BY ROLE"
        );

        cbRole.Items.Clear();
        foreach (DataRow row in dt.Rows)
        {
            cbRole.Items.Add(row["ROLE"].ToString());
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show("Lỗi load role: " + ex.Message);
    }
}
      private string GetTarget()
{
    bool hasUser = cbUser.SelectedItem != null && !string.IsNullOrWhiteSpace(cbUser.Text);
    bool hasRole = cbRole.SelectedItem != null && !string.IsNullOrWhiteSpace(cbRole.Text);

    if (hasUser && hasRole)
    {
        MessageBox.Show("Vui lòng chỉ chọn một trong User hoặc Role để xem/thu hồi quyền!");
        return null;
    }

    if (hasUser)
        return cbUser.Text.ToUpperInvariant();

    if (hasRole)
        return cbRole.Text.ToUpperInvariant();

    MessageBox.Show("Chọn User hoặc Role!");
    return null;
}

private void GrantUser(object? s, EventArgs e)
{
    if (cbUser.SelectedItem == null)
    {
        MessageBox.Show("Chọn USER!");
        return;
    }

    if (cbObject.SelectedItem == null)
    {
        MessageBox.Show("Chọn Object!");
        return;
    }

    if (ExecuteSql(BuildGrantSql(cbUser.SelectedItem!.ToString()!)))
    {
        MessageBox.Show("Cấp quyền cho User thành công!", "Thông báo");
    }
}

private void GrantRole(object? s, EventArgs e)
{
    if (cbRole.SelectedItem == null)
    {
        MessageBox.Show("Chọn ROLE!");
        return;
    }

    if (cbObject.SelectedItem == null)
    {
        MessageBox.Show("Chọn Object!");
        return;
    }

    if (ExecuteSql(BuildGrantSql(cbRole.SelectedItem!.ToString()!)))
    {
        MessageBox.Show("Cấp quyền cho Role thành công!", "Thông báo");
    }
}

private void GrantRoleToUser(object? s, EventArgs e)
{
    if (cbUser.SelectedItem == null || cbRole.SelectedItem == null)
    {
        MessageBox.Show("Chọn User và Role!");
        return;
    }

    if (ExecuteSql($"GRANT {cbRole.SelectedItem} TO {cbUser.SelectedItem}"))
    {
        MessageBox.Show("Gán Role cho User thành công!", "Thông báo");
    }
}

private void Revoke(object? s, EventArgs e)
{
    if (revokeRoleMembershipMode && cbUser.SelectedItem != null && cbRole.SelectedItem != null)
    {
        string user = cbUser.Text.Trim();
        string role = cbRole.Text.Trim();
        if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(role))
        {
            if (ExecuteSql($"REVOKE {role} FROM {user}"))
            {
                MessageBox.Show("Thu hồi role khỏi user thành công!", "Thông báo");
            }
            revokeRoleMembershipMode = false;
            return;
        }
    }

    string? target = GetTarget();
    if (target == null) return;

    if (cbObject.SelectedItem == null)
    {
        MessageBox.Show("Chọn Object!");
        return;
    }

    string privilege = cbPrivilege.Text;
    string obj = NormalizeSelectedObjectName(cbObject.Text);
    string col = (cbColumn.SelectedItem != null && cbColumn.Text != "(Tất cả cột)") ? cbColumn.Text.Trim() : "";

    string sql;

    if (IsExecutableObject(cbObject.Text))
    {
        sql = $"REVOKE EXECUTE ON {obj} FROM {target}";
        if (ExecuteSql(sql))
        {
            MessageBox.Show("Thu hồi quyền thành công!", "Thông báo");
        }
        return;
    }

    if (privilege == "UPDATE")
        sql = $"REVOKE {privilege} ON {obj} FROM {target}";
    else if (privilege == "SELECT" && col != "")
        sql = $"REVOKE SELECT ON {obj} FROM {target}";
    else
        sql = $"REVOKE {privilege} ON {obj} FROM {target}";

    if (ExecuteSql(sql))
    {
        MessageBox.Show("Thu hồi quyền thành công!", "Thông báo");
    }
}
 private string BuildGrantSql(string target)
{
    string privilege = cbPrivilege.Text;
    string rawObj = cbObject.Text;
    string col = (cbColumn.SelectedItem != null && cbColumn.Text != "(Tất cả cột)") ? cbColumn.Text.Trim() : "";
    string grantOpt = chkGrant.Checked ? " WITH GRANT OPTION" : "";

    string obj = NormalizeSelectedObjectName(rawObj);
    if (IsExecutableObject(rawObj))
        return $"GRANT EXECUTE ON {obj} TO {target}{grantOpt}";

    if (privilege == "UPDATE" && col != "")
        return $"GRANT {privilege} ({col}) ON {obj} TO {target}{grantOpt}";

    return $"GRANT {privilege} ON {obj} TO {target}{grantOpt}";
}

                #endregion

                #region 6. HELPERS (UI)
void LoadPrivileges(object? s, EventArgs e)
{
    string? name = GetTarget();
    if (name == null) return;

    try
    {
        string safeName = EscapeSqlLiteral(name);
        DataTable dt = RunQueryWithFallback(
            $@"
            SELECT 'OBJECT' AS LOAI, PRIVILEGE, OWNER||'.'||TABLE_NAME AS DOI_TUONG, GRANTABLE, NULL AS COT
            FROM DBA_TAB_PRIVS WHERE GRANTEE = '{safeName}'
            UNION ALL
            SELECT 'COLUMN' AS LOAI, PRIVILEGE, OWNER||'.'||TABLE_NAME AS DOI_TUONG, GRANTABLE, COLUMN_NAME AS COT
            FROM DBA_COL_PRIVS WHERE GRANTEE = '{safeName}'
            UNION ALL
            SELECT 'SYSTEM' AS LOAI, PRIVILEGE, NULL AS DOI_TUONG, ADMIN_OPTION AS GRANTABLE, NULL AS COT
            FROM DBA_SYS_PRIVS WHERE GRANTEE = '{safeName}'
            UNION ALL
            SELECT 'ROLE' AS LOAI, GRANTED_ROLE AS PRIVILEGE, NULL AS DOI_TUONG, ADMIN_OPTION AS GRANTABLE, NULL AS COT
            FROM DBA_ROLE_PRIVS WHERE GRANTEE = '{safeName}'
            ORDER BY 1, 2",
            $@"
            SELECT 'OBJECT' AS LOAI, PRIVILEGE, OWNER||'.'||TABLE_NAME AS DOI_TUONG, GRANTABLE, NULL AS COT
            FROM ALL_TAB_PRIVS WHERE GRANTEE = '{safeName}'
            UNION ALL
            SELECT 'COLUMN' AS LOAI, PRIVILEGE, OWNER||'.'||TABLE_NAME AS DOI_TUONG, GRANTABLE, COLUMN_NAME AS COT
            FROM ALL_COL_PRIVS WHERE GRANTEE = '{safeName}'
            UNION ALL
            SELECT 'SYSTEM' AS LOAI, PRIVILEGE, NULL AS DOI_TUONG, ADMIN_OPTION AS GRANTABLE, NULL AS COT
            FROM ALL_SYS_PRIVS WHERE GRANTEE = '{safeName}'
            UNION ALL
            SELECT 'ROLE' AS LOAI, GRANTED_ROLE AS PRIVILEGE, NULL AS DOI_TUONG, ADMIN_OPTION AS GRANTABLE, NULL AS COT
            FROM ALL_ROLE_PRIVS WHERE GRANTEE = '{safeName}'
            ORDER BY 1, 2"
        );
        grid.DataSource = dt;
    }
    catch (Exception ex)
    {
        MessageBox.Show("Lỗi load quyền: " + ex.Message);
    }
}
                private void SearchPrivilegeDetails(object s, EventArgs e)
                {
                    Button triggerButton = s as Button;
                    string name = txtSearchUser.Text.Trim().ToUpper();

                    if (string.IsNullOrWhiteSpace(name))
                    {
                        MessageBox.Show("Nhập User hoặc Role cần xem!");
                        return;
                    }

                    DataTable loadingTable = new DataTable();
                    loadingTable.Columns.Add("TRẠNG THÁI");
                    loadingTable.Rows.Add("Đang truy xuất quyền, vui lòng chờ...");
                    grid.DataSource = loadingTable;

                    if (triggerButton != null)
                    {
                        triggerButton.Enabled = false;
                        triggerButton.Text = "ĐANG KIỂM TRA...";
                    }

                    Cursor previousCursor = this.Cursor;
                    this.Cursor = Cursors.WaitCursor;
                    Application.DoEvents();

                    try
                    {
                        string safeName = EscapeSqlLiteral(name);
                        DataTable dt = RunQueryWithFallback(
                            $@"
                            SELECT GRANTEE, OWNER, TABLE_NAME, PRIVILEGE, GRANTABLE, 'DIRECT' AS SOURCE_ROLE
                            FROM DBA_TAB_PRIVS
                            WHERE GRANTEE = '{safeName}'

                            UNION ALL

                            SELECT RP.GRANTEE, TP.OWNER, TP.TABLE_NAME, TP.PRIVILEGE, TP.GRANTABLE, RP.GRANTED_ROLE AS SOURCE_ROLE
                            FROM DBA_ROLE_PRIVS RP
                            JOIN DBA_TAB_PRIVS TP ON RP.GRANTED_ROLE = TP.GRANTEE
                            WHERE RP.GRANTEE = '{safeName}'

                            ORDER BY TABLE_NAME, PRIVILEGE",
                            $@"
                            SELECT GRANTEE, OWNER, TABLE_NAME, PRIVILEGE, GRANTABLE, 'DIRECT' AS SOURCE_ROLE
                            FROM ALL_TAB_PRIVS
                            WHERE GRANTEE = '{safeName}'
                            ORDER BY TABLE_NAME, PRIVILEGE"
                        );
                        grid.DataSource = dt;

                        if (dt.Rows.Count == 0)
                        {
                            MessageBox.Show("Không tìm thấy quyền nào cho User/Role này!");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi tra cứu quyền: " + ex.Message);
                    }
                    finally
                    {
                        this.Cursor = previousCursor;

                        if (triggerButton != null)
                        {
                            triggerButton.Enabled = true;
                            triggerButton.Text = "🔍 KIỂM TRA";
                        }
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
                    AddSectionTitle(card, title, 20, 16);
                    return card;
                }

                private void AddSectionTitle(Control parent, string text, int x, int y)
                {
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        return;
                    }

                    Panel accent = new Panel
                    {
                        Location = new Point(x, y + 3),
                        Size = new Size(4, 18),
                        BackColor = clrPrimary
                    };

                    Label label = new Label
                    {
                        Text = text,
                        Location = new Point(x + 12, y),
                        AutoSize = true,
                        Font = new Font("Segoe UI", 10, FontStyle.Bold),
                        ForeColor = Color.FromArgb(31, 41, 55)
                    };

                    parent.Controls.Add(accent);
                    parent.Controls.Add(label);
                }

                  private void Grid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
{
    if (e.RowIndex < 0) return;

    DataGridViewRow row = grid.Rows[e.RowIndex];

    // 👉 USER
    if (lblHeaderTitle.Text.Contains("NGƯỜI DÙNG"))
    {
        txtUser.Text = Convert.ToString(row.Cells["USERNAME"].Value) ?? "";
        txtPass.Text = "";
        txtPass.Focus();
    }

    // 👉 ROLE
    if (lblHeaderTitle.Text.Contains("VAI TRÒ"))
    {
        txtRole.Text = Convert.ToString(row.Cells["ROLE"].Value) ?? "";
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
                        var parent = b.Parent;
                        if (parent != null)
                            foreach (Control c in parent.Controls) if (c is Button btn) btn.BackColor = Color.Transparent;
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
