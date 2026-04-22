using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class FormCreateRole : Form
{
    private Color bgColor = Color.FromArgb(20, 22, 30);
    private Color cardColor = Color.FromArgb(40, 42, 54);
    private Color accentColor = Color.FromArgb(0, 184, 148);
    private Color errorColor = Color.FromArgb(255, 118, 117);

    private TextBox txtRole;
    private Label lblError;

    private System.Windows.Forms.Timer animTimer;
    private float opacity = 0;
    private float scale = 0.9f;

    public string RoleName => txtRole.Text.Trim();

    public FormCreateRole()
    {
        this.DoubleBuffered = true;
        this.Size = new Size(420, 320);
        this.FormBorderStyle = FormBorderStyle.None;
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = bgColor;
        this.Opacity = 0;

        InitUI();
        StartAnimation();
    }

    private void InitUI()
    {
        Panel card = new Panel {
            Size = new Size(340, 240),
            BackColor = cardColor
        };

        CenterCard(card);

        Label lblTitle = new Label {
            Text = "TẠO ROLE",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = accentColor,
            Dock = DockStyle.Top,
            Height = 60,
            TextAlign = ContentAlignment.MiddleCenter
        };

        Label lbl = new Label {
            Text = "Role name",
            Location = new Point(30, 70),
            ForeColor = Color.LightGray
        };

        Panel pnlBorder = new Panel {
            Location = new Point(30, 95),
            Size = new Size(280, 40),
            BackColor = Color.FromArgb(60, 65, 85),
            Padding = new Padding(2)
        };

        txtRole = new TextBox {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.None,
            BackColor = Color.FromArgb(50, 55, 75),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 11)
        };

        txtRole.Enter += (s, e) => pnlBorder.BackColor = accentColor;
        txtRole.Leave += (s, e) => pnlBorder.BackColor = Color.FromArgb(60, 65, 85);

        pnlBorder.Controls.Add(txtRole);

        lblError = new Label {
            Location = new Point(30, 140),
            ForeColor = errorColor,
            Font = new Font("Segoe UI", 8),
            AutoSize = true
        };

        Button btnOk = CreateButton("TẠO", 30, 170, accentColor);
        btnOk.Click += (s, e) => ValidateAndClose();

        Button btnCancel = CreateButton("HỦY", 180, 170, Color.FromArgb(80, 80, 90));
        btnCancel.Click += (s, e) => {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        };

        // Nút X
        Button btnClose = new Button {
            Text = "✕",
            Size = new Size(35, 35),
            Location = new Point(this.ClientSize.Width - 45, 10),
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.Gray,
            BackColor = Color.Transparent,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        btnClose.FlatAppearance.BorderSize = 0;

        btnClose.Click += (s, e) => this.Close();

        btnClose.MouseEnter += (s, e) => {
            btnClose.BackColor = Color.FromArgb(80, 255, 0, 0);
            btnClose.ForeColor = Color.White;
        };

        btnClose.MouseLeave += (s, e) => {
            btnClose.BackColor = Color.Transparent;
            btnClose.ForeColor = Color.Gray;
        };

        card.Controls.Add(lblTitle);
        card.Controls.Add(lbl);
        card.Controls.Add(pnlBorder);
        card.Controls.Add(lblError);
        card.Controls.Add(btnOk);
        card.Controls.Add(btnCancel);

        this.Controls.Add(card);
        this.Controls.Add(btnClose);

        // Background glow
        this.Paint += DrawBackground;
    }

    private Button CreateButton(string text, int x, int y, Color color)
    {
        Button btn = new Button {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(120, 40),
            FlatStyle = FlatStyle.Flat,
            BackColor = color,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;

        btn.MouseEnter += (s, e) => btn.BackColor = ControlPaint.Light(color);
        btn.MouseLeave += (s, e) => btn.BackColor = color;
        btn.MouseDown += (s, e) => btn.BackColor = ControlPaint.Dark(color);

        return btn;
    }

    private void ValidateAndClose()
    {
        if (string.IsNullOrWhiteSpace(txtRole.Text))
        {
            lblError.Text = "Vui lòng nhập Role!";
            return;
        }

        this.DialogResult = DialogResult.OK;
    }

    // 🎬 Animation mở form
    private void StartAnimation()
    {
        animTimer = new System.Windows.Forms.Timer { Interval = 15 };
        animTimer.Tick += (s, e) =>
        {
            opacity += 0.05f;
            scale += 0.02f;

            if (opacity >= 1)
            {
                opacity = 1;
                scale = 1;
                animTimer.Stop();
            }

            this.Opacity = opacity;
            this.Invalidate();
        };
        animTimer.Start();
    }

    // 🌌 Background glow
    private void DrawBackground(object sender, PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        using (LinearGradientBrush br = new LinearGradientBrush(
            this.ClientRectangle,
            Color.FromArgb(20, 22, 30),
            Color.FromArgb(30, 40, 80),
            45f))
        {
            g.FillRectangle(br, this.ClientRectangle);
        }

        DrawGlow(g, 50, 50, 250);
        DrawGlow(g, 200, 150, 300);
    }

    private void DrawGlow(Graphics g, int x, int y, int size)
    {
        using (GraphicsPath path = new GraphicsPath())
        {
            path.AddEllipse(x, y, size, size);
            using (PathGradientBrush pgb = new PathGradientBrush(path)
            {
                CenterColor = Color.FromArgb(60, 0, 184, 148),
                SurroundColors = new Color[] { Color.Transparent }
            })
            {
                g.FillEllipse(pgb, x, y, size, size);
            }
        }
    }

    private void CenterCard(Control card)
    {
        card.Location = new Point(
            (this.Width - card.Width) / 2,
            (this.Height - card.Height) / 2
        );
    }
}