using System;
using System.Collections.Generic;
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
    private sealed class UserDataPage
        {
            public string Title { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string SelectSql { get; set; } = string.Empty;
            public string TargetName { get; set; } = string.Empty;
            public string[] KeyColumns { get; set; } = Array.Empty<string>();
            public string[] InsertColumns { get; set; } = Array.Empty<string>();
            public string[] UpdateColumns { get; set; } = Array.Empty<string>();
            public string[] EditableColumns { get; set; } = Array.Empty<string>();
            public bool AllowInsert { get; set; }
            public bool AllowUpdate { get; set; }
            public bool AllowDelete { get; set; }
            public bool Loaded { get; set; }
            public TabPage? Tab { get; set; }
            public Label? HintLabel { get; set; }
            public DataGridView? Grid { get; set; }
            public DataTable? Table { get; set; }
        }

        private readonly DBConnection db = new DBConnection();
        private readonly Dictionary<TabPage, UserDataPage> pages = new Dictionary<TabPage, UserDataPage>();
        private readonly string currentUser;
        private readonly string currentPass;

        private TabControl? tabControl;
        private Label? lblStatus;
        private Label? lblRole;
        private Button? btnSave;
        private Button? btnAdd;
        private Button? btnDelete;
        private Button? btnReload;

        public FormUser(string user, string pass)
        {
            currentUser = user.ToUpperInvariant();
            currentPass = pass;

            Text = "USER PANEL - " + currentUser;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1280, 800);
            BackColor = Color.FromArgb(244, 246, 249);
            Font = new Font("Segoe UI", 10);

            BuildUI();
            BuildRolePages();
            LoadActivePage();
        }

        private void BuildUI()
        {
            TableLayoutPanel root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 66F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            Controls.Add(root);

            Panel header = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 60, 120)
            };

            Label title = new Label
            {
                Text = "HOSPITAL USER PANEL",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            header.Controls.Add(title);

            TableLayoutPanel infoBar = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                BackColor = Color.FromArgb(236, 242, 248)
            };
            infoBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            infoBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            infoBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));

            Label lblUser = new Label
            {
                Text = "User: " + currentUser,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 41, 55),
                Padding = new Padding(20, 0, 0, 0)
            };

            lblRole = new Label
            {
                Text = "Vai trò: " + GetRoleDisplayName(),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 90, 180)
            };

            lblStatus = new Label
            {
                Text = "Sẵn sàng.",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.DimGray,
                Padding = new Padding(0, 0, 20, 0)
            };

            infoBar.Controls.Add(lblUser, 0, 0);
            infoBar.Controls.Add(lblRole, 1, 0);
            infoBar.Controls.Add(lblStatus, 2, 0);

            Panel toolbar = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            btnReload = CreateButton("Tải lại", Color.DodgerBlue);
            btnReload.Left = 20;
            btnReload.Top = 12;
            btnReload.Click += (s, e) => ReloadActivePage();

            btnSave = CreateButton("Lưu thay đổi", Color.SeaGreen);
            btnSave.Left = 180;
            btnSave.Top = 12;
            btnSave.Click += (s, e) => SaveActivePage();

            btnAdd = CreateButton("Thêm dòng", Color.DarkOrange);
            btnAdd.Left = 370;
            btnAdd.Top = 12;
            btnAdd.Click += (s, e) => AddRowToActivePage();

            btnDelete = CreateButton("Xóa dòng", Color.Firebrick);
            btnDelete.Left = 530;
            btnDelete.Top = 12;
            btnDelete.Click += (s, e) => DeleteCurrentRow();

            Button btnOLS = CreateButton("Thông báo (OLS)", Color.MediumPurple);
            btnOLS.Left = 690;
            btnOLS.Top = 12;
            btnOLS.Click += (s, e) =>
            {
                formNotice f = new formNotice(currentUser, currentPass);
                f.Show();
            };

            Button btnLogout = CreateButton("Logout", Color.IndianRed);
            btnLogout.Left = 880;
            btnLogout.Top = 12;
            btnLogout.Click += (s, e) =>
            {
                Hide();
                LoginForm login = new LoginForm();
                login.Show();
            };

            toolbar.Controls.Add(btnReload);
            toolbar.Controls.Add(btnSave);
            toolbar.Controls.Add(btnAdd);
            toolbar.Controls.Add(btnDelete);
            toolbar.Controls.Add(btnOLS);
            toolbar.Controls.Add(btnLogout);

            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Padding = new Point(18, 8),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            tabControl.SelectedIndexChanged += (s, e) =>
            {
                UpdateActionButtons();
                LoadActivePage();
            };

            root.Controls.Add(header, 0, 0);
            root.Controls.Add(infoBar, 0, 1);
            root.Controls.Add(toolbar, 0, 2);
            root.Controls.Add(tabControl, 0, 3);
        }

        private Button CreateButton(string text, Color color)
        {
            Button btn = new Button
            {
                Text = text,
                Width = 140,
                Height = 40,
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            btn.FlatAppearance.BorderSize = 0;
            btn.MouseEnter += (s, e) => btn.BackColor = ControlPaint.Light(color);
            btn.MouseLeave += (s, e) => btn.BackColor = color;
            return btn;
        }

        private void BuildRolePages()
        {
            if (tabControl == null)
            {
                return;
            }

            if (IsBenhNhan())
            {
                AddPage(new UserDataPage
                {
                    Title = "Thông tin cá nhân",
                    Description = "Bệnh nhân có thể xem và cập nhật thông tin cá nhân được phép chỉnh sửa.",
                    SelectSql = "SELECT * FROM BVOWNER.V_CURRENT_BENHNHAN",
                    TargetName = "BVOWNER.V_CURRENT_BENHNHAN",
                    KeyColumns = new[] { "MABN" },
                    UpdateColumns = new[] { "SONHA", "TENDUONG", "QUANHUYEN", "TINHTP", "TIENSUBENH", "TIENSUBENHGD", "DIUNGTUOC" },
                    EditableColumns = new[] { "SONHA", "TENDUONG", "QUANHUYEN", "TINHTP", "TIENSUBENH", "TIENSUBENHGD", "DIUNGTUOC" },
                    AllowUpdate = true
                });
                return;
            }

            AddPage(new UserDataPage
            {
                Title = "Hồ sơ cá nhân",
                Description = "Nhân viên chỉ được chỉnh sửa các cột tự phục vụ như quê quán và số điện thoại.",
                SelectSql = "SELECT * FROM BVOWNER.V_CURRENT_NHANVIEN_UPDATABLE",
                TargetName = "BVOWNER.V_CURRENT_NHANVIEN_UPDATABLE",
                KeyColumns = new[] { "MANV" },
                UpdateColumns = new[] { "QUEQUAN", "SODT" },
                EditableColumns = new[] { "QUEQUAN", "SODT" },
                AllowUpdate = true
            });

            if (IsDieuPhoiVien())
            {
                AddPage(new UserDataPage
                {
                    Title = "Bệnh nhân",
                    Description = "Điều phối viên có thể xem, thêm và cập nhật hồ sơ bệnh nhân.",
                    SelectSql = "SELECT * FROM BVOWNER.BENHNHAN ORDER BY MABN",
                    TargetName = "BVOWNER.BENHNHAN",
                    KeyColumns = new[] { "MABN" },
                    InsertColumns = new[] { "MABN", "TENBN", "PHAI", "NGAYSINH", "CCCD", "SONHA", "TENDUONG", "QUANHUYEN", "TINHTP", "TIENSUBENH", "TIENSUBENHGD", "DIUNGTUOC", "ORA_USERNAME" },
                    UpdateColumns = new[] { "TENBN", "PHAI", "NGAYSINH", "CCCD", "SONHA", "TENDUONG", "QUANHUYEN", "TINHTP", "TIENSUBENH", "TIENSUBENHGD", "DIUNGTUOC", "ORA_USERNAME" },
                    EditableColumns = new[] { "MABN", "TENBN", "PHAI", "NGAYSINH", "CCCD", "SONHA", "TENDUONG", "QUANHUYEN", "TINHTP", "TIENSUBENH", "TIENSUBENHGD", "DIUNGTUOC", "ORA_USERNAME" },
                    AllowInsert = true,
                    AllowUpdate = true
                });

                AddPage(new UserDataPage
                {
                    Title = "HSBA",
                    Description = "Điều phối viên thêm hồ sơ bệnh án mới và cập nhật phân công khoa, bác sĩ.",
                    SelectSql = "SELECT MAHSBA, MABN, NGAY, CHANDOAN, DIEUTRI, MABS, MAKHOA, KETLUAN FROM BVOWNER.HSBA ORDER BY NGAY DESC, MAHSBA",
                    TargetName = "BVOWNER.HSBA",
                    KeyColumns = new[] { "MAHSBA" },
                    InsertColumns = new[] { "MAHSBA", "MABN", "NGAY", "CHANDOAN", "DIEUTRI", "MABS", "MAKHOA", "KETLUAN" },
                    UpdateColumns = new[] { "MABS", "MAKHOA" },
                    EditableColumns = new[] { "MAHSBA", "MABN", "NGAY", "CHANDOAN", "DIEUTRI", "MABS", "MAKHOA", "KETLUAN" },
                    AllowInsert = true,
                    AllowUpdate = true
                });

                AddPage(new UserDataPage
                {
                    Title = "Điều phối DV",
                    Description = "Điều phối viên gán kỹ thuật viên cho từng dịch vụ hỗ trợ chẩn đoán.",
                    SelectSql = "SELECT MAHSBA, LOAIDV, NGAYDV, MAKTV, KETQUA FROM BVOWNER.HSBA_DV ORDER BY NGAYDV DESC, MAHSBA",
                    TargetName = "BVOWNER.HSBA_DV",
                    KeyColumns = new[] { "MAHSBA", "LOAIDV", "NGAYDV" },
                    UpdateColumns = new[] { "MAKTV" },
                    EditableColumns = new[] { "MAKTV" },
                    AllowUpdate = true
                });
            }

            if (IsBacSi())
            {
                AddPage(new UserDataPage
                {
                    Title = "HSBA phụ trách",
                    Description = "Bác sĩ/Y sĩ cập nhật chẩn đoán, điều trị và kết luận trên các hồ sơ mình phụ trách.",
                    SelectSql = "SELECT MAHSBA, MABN, NGAY, CHANDOAN, DIEUTRI, MABS, MAKHOA, KETLUAN FROM BVOWNER.HSBA ORDER BY NGAY DESC, MAHSBA",
                    TargetName = "BVOWNER.HSBA",
                    KeyColumns = new[] { "MAHSBA" },
                    UpdateColumns = new[] { "CHANDOAN", "DIEUTRI", "KETLUAN" },
                    EditableColumns = new[] { "CHANDOAN", "DIEUTRI", "KETLUAN" },
                    AllowUpdate = true
                });

                AddPage(new UserDataPage
                {
                    Title = "Bệnh nhân điều trị",
                    Description = "Bác sĩ/Y sĩ xem và cập nhật tiền sử bệnh, tiền sử gia đình, dị ứng thuốc của bệnh nhân liên quan.",
                    SelectSql = @"SELECT DISTINCT b.*
                                  FROM BVOWNER.BENHNHAN b
                                  JOIN BVOWNER.HSBA h ON h.MABN = b.MABN
                                  ORDER BY b.MABN",
                    TargetName = "BVOWNER.BENHNHAN",
                    KeyColumns = new[] { "MABN" },
                    UpdateColumns = new[] { "TIENSUBENH", "TIENSUBENHGD", "DIUNGTUOC" },
                    EditableColumns = new[] { "TIENSUBENH", "TIENSUBENHGD", "DIUNGTUOC" },
                    AllowUpdate = true
                });

                AddPage(new UserDataPage
                {
                    Title = "Dịch vụ chẩn đoán",
                    Description = "Bác sĩ/Y sĩ có thể thêm hoặc xóa các dòng dịch vụ hỗ trợ chẩn đoán cho hồ sơ mình phụ trách.",
                    SelectSql = @"SELECT d.MAHSBA, d.LOAIDV, d.NGAYDV, d.MAKTV, d.KETQUA
                                  FROM BVOWNER.HSBA_DV d
                                  JOIN BVOWNER.HSBA h ON h.MAHSBA = d.MAHSBA
                                  ORDER BY d.NGAYDV DESC, d.MAHSBA",
                    TargetName = "BVOWNER.HSBA_DV",
                    KeyColumns = new[] { "MAHSBA", "LOAIDV", "NGAYDV" },
                    InsertColumns = new[] { "MAHSBA", "LOAIDV", "NGAYDV", "MAKTV", "KETQUA" },
                    EditableColumns = new[] { "MAHSBA", "LOAIDV", "NGAYDV", "MAKTV", "KETQUA" },
                    AllowInsert = true,
                    AllowDelete = true
                });

                AddPage(new UserDataPage
                {
                    Title = "Đơn thuốc",
                    Description = "Bác sĩ/Y sĩ thêm, xóa và cập nhật đơn thuốc trên các hồ sơ thuộc phạm vi điều trị của mình.",
                    SelectSql = "SELECT MAHSBA, NGAYDT, TENTHUOC, LIEUDUNG FROM BVOWNER.DONTHUOC ORDER BY NGAYDT DESC, MAHSBA",
                    TargetName = "BVOWNER.DONTHUOC",
                    KeyColumns = new[] { "MAHSBA", "NGAYDT", "TENTHUOC" },
                    InsertColumns = new[] { "MAHSBA", "NGAYDT", "TENTHUOC", "LIEUDUNG" },
                    UpdateColumns = new[] { "MAHSBA", "NGAYDT", "TENTHUOC", "LIEUDUNG" },
                    EditableColumns = new[] { "MAHSBA", "NGAYDT", "TENTHUOC", "LIEUDUNG" },
                    AllowInsert = true,
                    AllowUpdate = true,
                    AllowDelete = true
                });
            }

            if (IsKyThuatVien())
            {
                AddPage(new UserDataPage
                {
                    Title = "Kết quả dịch vụ",
                    Description = "Kỹ thuật viên chỉ xem các dòng được điều phối cho mình và cập nhật cột KETQUA.",
                    SelectSql = "SELECT MAHSBA, LOAIDV, NGAYDV, MAKTV, KETQUA FROM BVOWNER.V_CURRENT_HSBA_DV_KTV ORDER BY NGAYDV DESC, MAHSBA",
                    TargetName = "BVOWNER.V_CURRENT_HSBA_DV_KTV",
                    KeyColumns = new[] { "MAHSBA", "LOAIDV", "NGAYDV" },
                    UpdateColumns = new[] { "KETQUA" },
                    EditableColumns = new[] { "KETQUA" },
                    AllowUpdate = true
                });
            }

            UpdateActionButtons();
        }

        private void AddPage(UserDataPage page)
        {
            if (tabControl == null)
            {
                return;
            }

            TabPage tab = new TabPage(page.Title)
            {
                BackColor = Color.FromArgb(244, 246, 249),
                Padding = new Padding(16)
            };

            Label hint = new Label
            {
                Text = page.Description,
                Dock = DockStyle.Top,
                Height = 38,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.DimGray
            };

            DataGridView pageGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false,
                MultiSelect = false
            };
            pageGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(33, 37, 41);
            pageGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            pageGrid.ColumnHeadersHeight = 42;
            pageGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(41, 128, 185);
            pageGrid.DefaultCellStyle.SelectionForeColor = Color.White;

            Panel gridHost = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 10, 0, 0)
            };
            gridHost.Controls.Add(pageGrid);

            tab.Controls.Add(gridHost);
            tab.Controls.Add(hint);

            page.Tab = tab;
            page.HintLabel = hint;
            page.Grid = pageGrid;

            tab.Tag = page;
            pages[tab] = page;
            tabControl.TabPages.Add(tab);
        }

        private void LoadActivePage()
        {
            UserDataPage? page = GetActivePage();
            if (page == null)
            {
                SetStatus("Không có dữ liệu cho tài khoản này.", Color.Firebrick);
                return;
            }

            if (!page.Loaded)
            {
                LoadPageData(page);
            }
            else
            {
                SetStatus(page.Description, Color.DimGray);
            }
        }

        private void ReloadActivePage()
        {
            UserDataPage? page = GetActivePage();
            if (page == null)
            {
                return;
            }

            page.Loaded = false;
            LoadPageData(page);
        }

        private void LoadPageData(UserDataPage page)
        {
            if (page.Grid == null)
            {
                return;
            }

            try
            {
                SetStatus("Đang tải " + page.Title.ToLowerInvariant() + "...", Color.DodgerBlue);
                using (OracleConnection conn = db.GetConnection(currentUser, currentPass))
                {
                    conn.Open();
                    OracleDataAdapter da = new OracleDataAdapter(page.SelectSql, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    page.Table = dt;
                    page.Grid.DataSource = dt;
                }

                ConfigureGrid(page);
                page.Loaded = true;
                SetStatus("Đã tải " + page.Title.ToLowerInvariant() + ".", Color.SeaGreen);
            }
            catch (Exception ex)
            {
                SetStatus("Không tải được " + page.Title.ToLowerInvariant() + ".", Color.Firebrick);
                MessageBox.Show("Lỗi load dữ liệu [" + page.Title + "]: " + ex.Message);
            }
        }

        private void ConfigureGrid(UserDataPage page)
        {
            if (page.Grid == null || page.Table == null)
            {
                return;
            }

            HashSet<string> editable = new HashSet<string>(page.EditableColumns, StringComparer.OrdinalIgnoreCase);

            foreach (DataGridViewColumn column in page.Grid.Columns)
            {
                bool isEditable = editable.Contains(column.DataPropertyName) || editable.Contains(column.Name);
                column.ReadOnly = !isEditable;
            }

            page.Grid.AllowUserToAddRows = false;
        }

        private void SaveActivePage()
        {
            UserDataPage? page = GetActivePage();
            if (page == null || page.Table == null)
            {
                return;
            }

            if (!page.AllowInsert && !page.AllowUpdate && !page.AllowDelete)
            {
                MessageBox.Show("Trang này chỉ để xem dữ liệu.");
                return;
            }

            DataTable? changes = page.Table.GetChanges();
            if (changes == null)
            {
                SetStatus("Không có thay đổi để lưu.", Color.DimGray);
                return;
            }

            try
            {
                using (OracleConnection conn = db.GetConnection(currentUser, currentPass))
                {
                    conn.Open();
                    using (OracleTransaction transaction = conn.BeginTransaction())
                    {
                        foreach (DataRow row in page.Table.Select(null, null, DataViewRowState.Deleted))
                        {
                            if (page.AllowDelete)
                            {
                                ExecuteDelete(conn, transaction, page, row);
                            }
                        }

                        foreach (DataRow row in page.Table.Select(null, null, DataViewRowState.Added))
                        {
                            if (page.AllowInsert)
                            {
                                ExecuteInsert(conn, transaction, page, row);
                            }
                        }

                        foreach (DataRow row in page.Table.Select(null, null, DataViewRowState.ModifiedCurrent))
                        {
                            if (page.AllowUpdate)
                            {
                                ExecuteUpdate(conn, transaction, page, row);
                            }
                        }

                        transaction.Commit();
                    }
                }

                page.Loaded = false;
                LoadPageData(page);
                SetStatus("Đã lưu thay đổi cho " + page.Title.ToLowerInvariant() + ".", Color.SeaGreen);
            }
            catch (Exception ex)
            {
                SetStatus("Lưu thay đổi thất bại.", Color.Firebrick);
                MessageBox.Show("Lỗi lưu dữ liệu [" + page.Title + "]: " + ex.Message);
            }
        }

        private void ExecuteInsert(OracleConnection conn, OracleTransaction transaction, UserDataPage page, DataRow row)
        {
            string[] columns = page.InsertColumns.Length > 0 ? page.InsertColumns : page.EditableColumns;
            string[] parameterNames = new string[columns.Length];
            for (int i = 0; i < columns.Length; i++)
            {
                parameterNames[i] = ":p" + i;
            }

            string sql = $"INSERT INTO {page.TargetName} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", parameterNames)})";

            using (OracleCommand cmd = new OracleCommand(sql, conn))
            {
                cmd.Transaction = transaction;
                cmd.BindByName = true;

                for (int i = 0; i < columns.Length; i++)
                {
                    cmd.Parameters.Add(parameterNames[i], NormalizeValue(page.Table, columns[i], row[columns[i], DataRowVersion.Current]));
                }

                cmd.ExecuteNonQuery();
            }
        }

        private void ExecuteUpdate(OracleConnection conn, OracleTransaction transaction, UserDataPage page, DataRow row)
        {
            string[] columns = page.UpdateColumns;
            if (columns.Length == 0)
            {
                return;
            }

            List<string> setParts = new List<string>();
            List<string> whereParts = new List<string>();

            using (OracleCommand cmd = new OracleCommand())
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;
                cmd.BindByName = true;

                for (int i = 0; i < columns.Length; i++)
                {
                    string column = columns[i];
                    string parameterName = ":set_" + i;
                    setParts.Add(column + " = " + parameterName);
                    cmd.Parameters.Add(parameterName, NormalizeValue(page.Table, column, row[column, DataRowVersion.Current]));
                }

                for (int i = 0; i < page.KeyColumns.Length; i++)
                {
                    string key = page.KeyColumns[i];
                    string parameterName = ":key_" + i;
                    whereParts.Add(key + " = " + parameterName);
                    cmd.Parameters.Add(parameterName, NormalizeValue(page.Table, key, row[key, DataRowVersion.Original]));
                }

                cmd.CommandText = $"UPDATE {page.TargetName} SET {string.Join(", ", setParts)} WHERE {string.Join(" AND ", whereParts)}";
                cmd.ExecuteNonQuery();
            }
        }

        private void ExecuteDelete(OracleConnection conn, OracleTransaction transaction, UserDataPage page, DataRow row)
        {
            List<string> whereParts = new List<string>();

            using (OracleCommand cmd = new OracleCommand())
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;
                cmd.BindByName = true;

                for (int i = 0; i < page.KeyColumns.Length; i++)
                {
                    string key = page.KeyColumns[i];
                    string parameterName = ":key_" + i;
                    whereParts.Add(key + " = " + parameterName);
                    cmd.Parameters.Add(parameterName, NormalizeValue(page.Table, key, row[key, DataRowVersion.Original]));
                }

                cmd.CommandText = $"DELETE FROM {page.TargetName} WHERE {string.Join(" AND ", whereParts)}";
                cmd.ExecuteNonQuery();
            }
        }

        private object NormalizeValue(DataTable? table, string columnName, object rawValue)
        {
            if (rawValue == DBNull.Value || rawValue == null)
            {
                return DBNull.Value;
            }

            if (table == null || !table.Columns.Contains(columnName))
            {
                return rawValue;
            }

            DataColumn column = table.Columns[columnName];

            if (column.DataType == typeof(string))
            {
                string text = Convert.ToString(rawValue)?.Trim() ?? string.Empty;
                return string.IsNullOrWhiteSpace(text) ? DBNull.Value : text;
            }

            if (column.DataType == typeof(DateTime))
            {
                if (rawValue is DateTime dateValue)
                {
                    return dateValue;
                }

                if (DateTime.TryParse(Convert.ToString(rawValue), out DateTime parsed))
                {
                    return parsed;
                }

                return DBNull.Value;
            }

            return rawValue;
        }

        private void AddRowToActivePage()
        {
            UserDataPage? page = GetActivePage();
            if (page == null || page.Table == null)
            {
                return;
            }

            if (!page.AllowInsert)
            {
                MessageBox.Show("Trang này không hỗ trợ thêm mới.");
                return;
            }

            DataRow newRow = page.Table.NewRow();
            page.Table.Rows.Add(newRow);
            if (page.Grid != null && page.Grid.Rows.Count > 0)
            {
                int rowIndex = page.Grid.Rows.Count - 1;
                page.Grid.ClearSelection();
                page.Grid.Rows[rowIndex].Selected = true;
                page.Grid.CurrentCell = page.Grid.Rows[rowIndex].Cells[0];
            }

            SetStatus("Đã thêm dòng trống. Nhập dữ liệu rồi bấm Lưu thay đổi.", Color.DarkOrange);
        }

        private void DeleteCurrentRow()
        {
            UserDataPage? page = GetActivePage();
            if (page == null || page.Grid == null || page.Table == null)
            {
                return;
            }

            if (!page.AllowDelete)
            {
                MessageBox.Show("Trang này không hỗ trợ xóa dữ liệu.");
                return;
            }

            if (page.Grid.CurrentRow == null || page.Grid.CurrentRow.Index < 0)
            {
                MessageBox.Show("Chọn một dòng cần xóa.");
                return;
            }

            DialogResult confirm = MessageBox.Show(
                "Đánh dấu xóa dòng đang chọn và lưu thay đổi?",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes)
            {
                return;
            }

            DataRowView? rowView = page.Grid.CurrentRow.DataBoundItem as DataRowView;
            if (rowView == null)
            {
                MessageBox.Show("Không xác định được dòng dữ liệu cần xóa.");
                return;
            }

            rowView.Row.Delete();
            SetStatus("Dòng đã được đánh dấu xóa. Bấm Lưu thay đổi để áp dụng.", Color.Firebrick);
        }

        private UserDataPage? GetActivePage()
        {
            if (tabControl == null || tabControl.SelectedTab == null)
            {
                return null;
            }

            return pages.TryGetValue(tabControl.SelectedTab, out UserDataPage? page) ? page : null;
        }

        private void UpdateActionButtons()
        {
            UserDataPage? page = GetActivePage();
            if (btnSave == null || btnAdd == null || btnDelete == null || btnReload == null)
            {
                return;
            }

            bool hasPage = page != null;
            btnReload.Enabled = hasPage;
            btnSave.Enabled = hasPage && (page!.AllowInsert || page.AllowUpdate || page.AllowDelete);
            btnAdd.Enabled = hasPage && page!.AllowInsert;
            btnDelete.Enabled = hasPage && page!.AllowDelete;
        }

        private void SetStatus(string text, Color color)
        {
            if (lblStatus == null)
            {
                return;
            }

            lblStatus.Text = text;
            lblStatus.ForeColor = color;
        }

        private bool IsBenhNhan()
        {
            return currentUser.StartsWith("BN_", StringComparison.OrdinalIgnoreCase) || currentUser.StartsWith("BN", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsDieuPhoiVien()
        {
            return currentUser.StartsWith("DPV_", StringComparison.OrdinalIgnoreCase) || currentUser.StartsWith("DPV", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsBacSi()
        {
            return currentUser.StartsWith("BS_", StringComparison.OrdinalIgnoreCase) || currentUser.StartsWith("BS", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsKyThuatVien()
        {
            return currentUser.StartsWith("KTV_", StringComparison.OrdinalIgnoreCase) || currentUser.StartsWith("KTV", StringComparison.OrdinalIgnoreCase);
        }

        private string GetRoleDisplayName()
        {
            if (IsBenhNhan())
            {
                return "Bệnh nhân";
            }

            if (IsDieuPhoiVien())
            {
                return "Điều phối viên";
            }

            if (IsBacSi())
            {
                return "Bác sĩ / Y sĩ";
            }

            if (IsKyThuatVien())
            {
                return "Kỹ thuật viên";
            }

            return "Người dùng hệ thống";
        }
    }
}
