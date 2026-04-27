using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace HospitalApp
{
    public class DropForm : Form
    {
        public bool IsConfirmed = false;

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        public DropForm(string message)
        {
            InitUI(message);
            // Bo góc Form
            this.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 12, 12));
        }

        private void InitUI(string message)
        {
            this.Size = new Size(420, 240); // Tăng chiều cao một chút cho Header
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.White;

            // 1. Header Bar (Tạo điểm nhấn màu đỏ cảnh báo ở trên cùng)
            Panel headerBar = new Panel {
                Dock = DockStyle.Top,
                Height = 5,
                BackColor = Color.FromArgb(231, 76, 60) // Màu đỏ Danger
            };

            // 2. Title Section (Dòng chữ tiêu đề nhỏ)
            Label lblTitle = new Label {
                Text = "XÁC NHẬN HÀNH ĐỘNG",
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Color.DarkGray,
                Location = new Point(20, 15),
                AutoSize = true
            };

            // 3. Content Layout
            TableLayoutPanel contentLayout = new TableLayoutPanel {
                Location = new Point(0, 40),
                Size = new Size(420, 120),
                ColumnCount = 2,
                Padding = new Padding(20, 0, 20, 0)
            };
            contentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60F));
            contentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            Label iconLabel = new Label {
                Text = "⚠️",
                Font = new Font("Segoe UI Semibold", 28),
                ForeColor = Color.FromArgb(255, 193, 7),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopCenter
            };

            Label lblMessage = new Label {
                Text = message,
                Font = new Font("Segoe UI Semibold", 11f),
                ForeColor = Color.FromArgb(45, 52, 54),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            contentLayout.Controls.Add(iconLabel, 0, 0);
            contentLayout.Controls.Add(lblMessage, 1, 0);

            // 4. Action Bar (Vùng chứa nút có màu nền khác để tách biệt)
            Panel actionBar = new Panel {
                Dock = DockStyle.Bottom,
                Height = 70,
                BackColor = Color.FromArgb(248, 249, 250) // Màu xám cực nhẹ
            };

            Button btnCancel = CreateModernButton("Hủy bỏ", Color.FromArgb(220, 220, 220), Color.FromArgb(64, 64, 64), false);
            Button btnOK = CreateModernButton("Xác nhận xóa", Color.FromArgb(231, 76, 60), Color.White, true);

            btnOK.Location = new Point(140, 15);
            btnCancel.Location = new Point(270, 15);

            btnOK.Click += (s, e) => { IsConfirmed = true; this.Close(); };
            btnCancel.Click += (s, e) => { IsConfirmed = false; this.Close(); };

            actionBar.Controls.Add(btnOK);
            actionBar.Controls.Add(btnCancel);

            // 5. Viền bao quanh Form (Border) để không bị lẫn vào nền trắng
            this.Paint += (s, e) => {
                e.Graphics.DrawRectangle(new Pen(Color.FromArgb(220, 220, 220), 2), 0, 0, this.Width - 1, this.Height - 1);
            };

            this.Controls.Add(lblTitle);
            this.Controls.Add(contentLayout);
            this.Controls.Add(actionBar);
            this.Controls.Add(headerBar);

            // Phím tắt
            this.KeyPreview = true;
            this.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.Enter) btnOK.PerformClick();
                if (e.KeyCode == Keys.Escape) btnCancel.PerformClick();
            };
        }

        private Button CreateModernButton(string text, Color backColor, Color foreColor, bool isPrimary)
        {
            Button btn = new Button {
                Text = text,
                Size = new Size(120, 40),
                BackColor = backColor,
                ForeColor = foreColor,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;

            // Bo góc cho nút
            IntPtr ptr = CreateRoundRectRgn(0, 0, btn.Width, btn.Height, 8, 8);
            btn.Region = Region.FromHrgn(ptr);

            btn.MouseEnter += (s, e) => btn.BackColor = ControlPaint.Dark(backColor, 0.05f);
            btn.MouseLeave += (s, e) => btn.BackColor = backColor;

            return btn;
        }

        protected override CreateParams CreateParams {
            get {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= 0x20000; // Drop Shadow
                return cp;
            }
        }
    }
}