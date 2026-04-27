using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Runtime.InteropServices;

public class FormChangePassword : Form
{
    // --- Khai báo màu sắc (Modern Dark Theme) ---
    private Color bgColor = Color.FromArgb(15, 16, 22);
    private Color cardColor = Color.FromArgb(28, 30, 38);
    private Color accentColor = Color.FromArgb(0, 210, 158); // Green Mint
    private Color errorColor = Color.FromArgb(255, 107, 107);
    private Color inputBg = Color.FromArgb(38, 41, 54);

    private TextBox txtPass, txtConfirm;
    private Label lblErrPass, lblErrConfirm;
    private Panel card;
    private float opacity = 0;

    public string NewPassword => txtPass.Text;

    // Bo góc cho Form (Dùng Win32 API để mượt hơn)
    [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
    private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

    public FormChangePassword()
    {
        this.DoubleBuffered = true;
        this.Size = new Size(500, 550); // Tăng chiều cao để tránh bị đè
        this.FormBorderStyle = FormBorderStyle.None;
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = bgColor;
        this.Opacity = 0;
        this.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 30, 30));

        InitUI();
        StartAnimation();
    }

    private void InitUI()
    {
        // 1. CARD CONTAINER
        card = new Panel {
            Size = new Size(420, 450),
            BackColor = cardColor
        };
        CenterCard(card);
        card.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, card.Width, card.Height, 20, 20));

        // 2. TITLE
        Label lblTitle = new Label {
            Text = "ĐỔI MẬT KHẨU",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = accentColor,
            Dock = DockStyle.Top,
            Height = 80,
            TextAlign = ContentAlignment.MiddleCenter
        };

        // 3. INPUT FIELDS (Tính toán lại Y để không bị đè)
        int startY = 90;
        int spacing = 115; // Khoảng cách giữa 2 cụm input

        CreateInput(card, "Mật khẩu mới", out txtPass, out lblErrPass, startY, true);
        CreateInput(card, "Xác nhận mật khẩu", out txtConfirm, out lblErrConfirm, startY + spacing, true);

        // Realtime match check
        txtConfirm.TextChanged += (s, e) => {
            if (string.IsNullOrEmpty(txtConfirm.Text)) lblErrConfirm.Text = "";
            else lblErrConfirm.Text = txtConfirm.Text != txtPass.Text ? "Mật khẩu không trùng khớp!" : "";
        };

        // 4. BUTTONS (Đẩy xuống thấp hơn)
        Button btnOk = CreateButton("XÁC NHẬN", 40, 360, accentColor);
        btnOk.Click += (s, e) => ValidateAndClose();

        Button btnCancel = CreateButton("HỦY BỎ", 220, 360, Color.FromArgb(60, 63, 81));
        btnCancel.Click += (s, e) => {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        };

        // 5. CLOSE BUTTON (X)
        Label btnClose = new Label {
            Text = "✕",
            Font = new Font("Arial", 12, FontStyle.Bold),
            Size = new Size(30, 30),
            Location = new Point(this.Width - 40, 15),
            ForeColor = Color.DimGray,
            Cursor = Cursors.Hand,
            TextAlign = ContentAlignment.MiddleCenter
        };
        btnClose.Click += (s, e) => this.Close();
        btnClose.MouseEnter += (s, e) => btnClose.ForeColor = Color.White;
        btnClose.MouseLeave += (s, e) => btnClose.ForeColor = Color.DimGray;

        card.Controls.Add(lblTitle);
        card.Controls.Add(btnOk);
        card.Controls.Add(btnCancel);
        this.Controls.Add(btnClose);
        this.Controls.Add(card);

        this.Paint += DrawBackground;
    }

    private void CreateInput(Panel parent, string labelText, out TextBox txt, out Label err, int y, bool isPass)
    {
        Label lbl = new Label {
            Text = labelText.ToUpper(),
            Location = new Point(40, y),
            ForeColor = Color.Gray,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            AutoSize = true
        };

        Panel pnlInput = new Panel {
            Location = new Point(40, y + 25),
            Size = new Size(340, 45),
            BackColor = inputBg,
            Padding = new Padding(10, 12, 10, 0)
        };
        pnlInput.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlInput.Width, pnlInput.Height, 10, 10));

        txt = new TextBox {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.None,
            BackColor = inputBg,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 12),
            UseSystemPasswordChar = isPass
        };

        // Eye Button
        Button eye = new Button {
            Text = "👁",
            Dock = DockStyle.Right,
            Width = 35,
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.Gray,
            Cursor = Cursors.Hand
        };
        eye.FlatAppearance.BorderSize = 0;
        TextBox localTxt = txt;
        eye.Click += (s, e) => {
            localTxt.UseSystemPasswordChar = !localTxt.UseSystemPasswordChar;
            eye.ForeColor = localTxt.UseSystemPasswordChar ? Color.Gray : accentColor;
        };

        pnlInput.Controls.Add(txt);
        pnlInput.Controls.Add(eye);

        err = new Label {
            Text = "",
            Location = new Point(40, y + 72),
            ForeColor = errorColor,
            Font = new Font("Segoe UI", 8, FontStyle.Italic),
            AutoSize = true,
            Width = 340
        };

        parent.Controls.Add(lbl);
        parent.Controls.Add(pnlInput);
        parent.Controls.Add(err);
    }

    private Button CreateButton(string text, int x, int y, Color color)
    {
        Button btn = new Button {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(160, 50),
            FlatStyle = FlatStyle.Flat,
            BackColor = color,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btn.Width, btn.Height, 12, 12));
        
        btn.MouseEnter += (s, e) => btn.BackColor = ControlPaint.Light(color);
        btn.MouseLeave += (s, e) => btn.BackColor = color;
        return btn;
    }

    private void ValidateAndClose()
    {
        bool isValid = true;
        if (txtPass.Text.Length < 6) {
            lblErrPass.Text = "Mật khẩu phải từ 6 ký tự trở lên!";
            isValid = false;
        }
        if (txtPass.Text != txtConfirm.Text) {
            lblErrConfirm.Text = "Mật khẩu xác nhận không khớp!";
            isValid = false;
        }

        if (isValid) {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }

    private void StartAnimation()
    {
        System.Windows.Forms.Timer t = new System.Windows.Forms.Timer { Interval = 15 };
        t.Tick += (s, e) => {
            opacity += 0.1f;
            if (opacity >= 1) { opacity = 1; t.Stop(); }
            this.Opacity = opacity;
        };
        t.Start();
    }

    private void DrawBackground(object sender, PaintEventArgs e)
    {
        using (LinearGradientBrush br = new LinearGradientBrush(this.ClientRectangle, Color.FromArgb(20, 22, 30), Color.FromArgb(40, 45, 60), 45f))
        {
            e.Graphics.FillRectangle(br, this.ClientRectangle);
        }
    }

    private void CenterCard(Control c)
    {
        c.Location = new Point((this.ClientSize.Width - c.Width) / 2, (this.ClientSize.Height - c.Height) / 2);
    }
}