using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class FormCreateUser : Form
{
    private Color bgColor = Color.FromArgb(30, 31, 38);
    private Color cardColor = Color.FromArgb(45, 48, 65);
    private Color accentColor = Color.FromArgb(0, 184, 148);
    private Color errorColor = Color.FromArgb(255, 118, 117);

    private TextBox txtUser = null!;
    private TextBox txtPass = null!;
    private TextBox txtConfirm = null!;
    private Label lblErrUser = null!;
    private Label lblErrPass = null!;
    private Label lblErrConfirm = null!;

    public string Username => txtUser.Text.Trim();
    public string Password => txtPass.Text;

    public FormCreateUser()
    {
        this.DoubleBuffered = true;
        this.Size = new Size(450, 600);
        this.FormBorderStyle = FormBorderStyle.None;
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = bgColor;

        InitUI();
    }

    private void InitUI()
    {
        Panel card = new Panel {
            Size = new Size(370, 520),
            Location = new Point(40, 40),
            BackColor = cardColor
        };

        Label lblTitle = new Label {
            Text = "TẠO TÀI KHOẢN",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = accentColor,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Top,
            Height = 80
        };

        int startY = 100;
        CreateInputGroup(card, "Username", out txtUser, out lblErrUser, startY);
        CreateInputGroup(card, "Mật khẩu", out txtPass, out lblErrPass, startY + 90, true);
        CreateInputGroup(card, "Xác nhận mật khẩu", out txtConfirm, out lblErrConfirm, startY + 180, true);

        txtConfirm.TextChanged += (s, e) =>
        {
            lblErrConfirm.Text = txtConfirm.Text != txtPass.Text ? "Mật khẩu không khớp!" : "";
        };

        Button btnCreate = CreateModernButton("ĐĂNG KÝ", startY + 280);
        btnCreate.Click += (s, e) => ValidateFinal();

        card.Controls.Add(lblTitle);
        card.Controls.Add(btnCreate);
        this.Controls.Add(card);

        // ===== NÚT X GÓC PHẢI =====
        Button btnClose = new Button {
            Text = "✕",
            Size = new Size(35, 35),
            Location = new Point(this.ClientSize.Width - 45, 10),
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.Gray,
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        btnClose.FlatAppearance.BorderSize = 0;

        btnClose.Click += (s, e) =>
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        };

        btnClose.MouseEnter += (s, e) =>
        {
            btnClose.ForeColor = Color.White;
            btnClose.BackColor = Color.FromArgb(60, 255, 0, 0);
        };

        btnClose.MouseLeave += (s, e) =>
        {
            btnClose.ForeColor = Color.Gray;
            btnClose.BackColor = Color.Transparent;
        };

        this.Controls.Add(btnClose);

        // Shadow card
        this.Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (Pen p = new Pen(Color.FromArgb(40, 0, 0, 0), 8))
            {
                e.Graphics.DrawRectangle(p, card.Bounds);
            }
        };
    }

    private Panel CreateInputGroup(Panel parent, string labelText, out TextBox tb, out Label err, int y, bool isPass = false)
    {
        Label lbl = new Label {
            Text = labelText,
            Location = new Point(30, y),
            ForeColor = Color.LightGray,
            AutoSize = true,
            Font = new Font("Segoe UI", 9)
        };

        Panel pnlBorder = new Panel {
            Location = new Point(30, y + 20),
            Size = new Size(310, 38),
            BackColor = Color.FromArgb(70, 75, 95),
            Padding = new Padding(2)
        };

        TextBox txt = new TextBox {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(55, 58, 75),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.None,
            Font = new Font("Segoe UI", 11),
            UseSystemPasswordChar = isPass
        };

        // Focus effect
        txt.Enter += (s, e) => pnlBorder.BackColor = accentColor;
        txt.Leave += (s, e) => pnlBorder.BackColor = Color.FromArgb(70, 75, 95);

        err = new Label {
            Location = new Point(30, y + 60),
            ForeColor = errorColor,
            AutoSize = true,
            Font = new Font("Segoe UI", 8)
        };

        if (isPass)
        {
            Button btnEye = new Button {
                Text = "👁",
                Dock = DockStyle.Right,
                Width = 35,
                FlatStyle = FlatStyle.Flat,
                ForeColor = accentColor,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            btnEye.FlatAppearance.BorderSize = 0;

            btnEye.Click += (s, e) =>
            {
                txt.UseSystemPasswordChar = !txt.UseSystemPasswordChar;
            };

            btnEye.MouseEnter += (s, e) => btnEye.ForeColor = Color.White;
            btnEye.MouseLeave += (s, e) => btnEye.ForeColor = accentColor;

            pnlBorder.Controls.Add(btnEye);
        }

        pnlBorder.Controls.Add(txt);
        parent.Controls.Add(lbl);
        parent.Controls.Add(pnlBorder);
        parent.Controls.Add(err);

        tb = txt;
        return pnlBorder;
    }

    private Button CreateModernButton(string text, int y)
    {
        Button btn = new Button {
            Text = text,
            Location = new Point(30, y),
            Size = new Size(310, 45),
            FlatStyle = FlatStyle.Flat,
            BackColor = accentColor,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;

        btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(0, 206, 166);
        btn.MouseLeave += (s, e) => btn.BackColor = accentColor;
        btn.MouseDown += (s, e) => btn.BackColor = Color.FromArgb(0, 150, 120);

        return btn;
    }

    private void ValidateFinal()
    {
        bool isValid = true;

        if (string.IsNullOrEmpty(txtUser.Text))
        {
            lblErrUser.Text = "Vui lòng nhập tên!";
            isValid = false;
        }

        if (txtPass.Text.Length < 6)
        {
            lblErrPass.Text = "Tối thiểu 6 ký tự!";
            isValid = false;
        }

        if (isValid && txtPass.Text == txtConfirm.Text)
        {
            this.DialogResult = DialogResult.OK;
        }
    }
}