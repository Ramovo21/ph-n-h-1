using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using HospitalApp.Services;

namespace HospitalApp.Forms
{
public class LoginForm : Form
{
TextBox txtUser, txtPass;
Button btnLogin;
Label lblError;


    public LoginForm()
    {
        InitUI();
    }

    void InitUI()
    {
        this.Text = "Hospital Login";
        this.Size = new Size(520, 420);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.None;
        this.BackColor = Color.FromArgb(15, 23, 42);

        // ===== CARD =====
        Panel card = new Panel();
        card.Size = new Size(360, 300);
        card.BackColor = Color.FromArgb(30, 41, 59);
        card.Left = (this.Width - card.Width) / 2;
        card.Top = (this.Height - card.Height) / 2;

        // ===== TITLE =====
        Label title = new Label();
        title.Text = "HOSPITAL LOGIN";
        title.Font = new Font("Segoe UI", 18, FontStyle.Bold);
        title.ForeColor = Color.White;
        title.Width = 300;
        title.Height = 40;
        title.Left = 30;
        title.Top = 20;
        title.TextAlign = ContentAlignment.MiddleCenter;

        // ===== USER =====
        txtUser = CreateTextbox("👤 Username", 80);

        // ===== PASS =====
        txtPass = CreateTextbox("🔒 Password", 130);
        txtPass.PasswordChar = '*';

        // ===== ERROR =====
        lblError = new Label();
        lblError.ForeColor = Color.Red;
        lblError.Width = 300;
        lblError.Height = 25;
        lblError.Left = 30;
        lblError.Top = 170;
        lblError.TextAlign = ContentAlignment.MiddleCenter;

        // ===== LOGIN BUTTON =====
        btnLogin = new Button();
        btnLogin.Text = "LOGIN";
        btnLogin.Width = 280;
        btnLogin.Height = 45;
        btnLogin.Left = 40;
        btnLogin.Top = 210;

        btnLogin.BackColor = Color.FromArgb(59, 130, 246);
        btnLogin.ForeColor = Color.White;
        btnLogin.Font = new Font("Segoe UI", 12, FontStyle.Bold);
        btnLogin.FlatStyle = FlatStyle.Flat;
        btnLogin.FlatAppearance.BorderSize = 0;

        btnLogin.MouseEnter += (s, e) =>
            btnLogin.BackColor = Color.FromArgb(96, 165, 250);

        btnLogin.MouseLeave += (s, e) =>
            btnLogin.BackColor = Color.FromArgb(59, 130, 246);

        btnLogin.Click += BtnLogin_Click;

        // ===== CLOSE =====
        Button btnClose = new Button();
        btnClose.Text = "X";
        btnClose.BackColor = Color.FromArgb(239, 68, 68);
        btnClose.ForeColor = Color.White;
        btnClose.Width = 40;
        btnClose.Height = 30;
        btnClose.Left = this.Width - 50;
        btnClose.Top = 5;
        btnClose.FlatStyle = FlatStyle.Flat;
        btnClose.FlatAppearance.BorderSize = 0;
        btnClose.Click += (s, e) => Application.Exit();

        // ===== ADD =====
        card.Controls.Add(title);
        card.Controls.Add(txtUser);
        card.Controls.Add(txtPass);
        card.Controls.Add(lblError);
        card.Controls.Add(btnLogin);

        this.Controls.Add(card);
        this.Controls.Add(btnClose);

        this.AcceptButton = btnLogin;
    }

    // ===== TEXTBOX =====
    TextBox CreateTextbox(string placeholder, int top)
    {
        TextBox txt = new TextBox();
        txt.Width = 280;
        txt.Height = 35;
        txt.Left = 40;
        txt.Top = top;
        txt.Font = new Font("Segoe UI", 11);
        txt.BackColor = Color.FromArgb(51, 65, 85);
        txt.ForeColor = Color.White;
        txt.BorderStyle = BorderStyle.FixedSingle;

        txt.Text = placeholder;

        txt.Enter += (s, e) =>
        {
            if (txt.Text == placeholder)
            {
                txt.Text = "";
            }
        };

        txt.Leave += (s, e) =>
        {
            if (txt.Text == "")
            {
                txt.Text = placeholder;
            }
        };

        return txt;
    }

    // ===== LOGIN =====
    private void BtnLogin_Click(object sender, EventArgs e)
    {
        string user = txtUser.Text.Trim();
        string pass = txtPass.Text.Trim();

        if (user == "" || user.Contains("Username") ||
            pass == "" || pass.Contains("Password"))
        {
            lblError.Text = "⚠ Nhập đầy đủ thông tin!";
            return;
        }

        try
        {
            DBConnection db = new DBConnection();

            using (OracleConnection conn = db.GetConnection(user, pass))
            {
                conn.Open();
            }

            lblError.Text = "";

            if (user.ToUpper() == "SYS")
            {
                new FormAdmin(user, pass).Show();
            }
            else
            {
                new FormUser(user, pass).Show();
            }

            this.Hide();
        }
        catch (OracleException ex)
        {
            // 🔥 PHÂN BIỆT LỖI
            if (ex.Number == 1017)
            {
                lblError.Text = "❌ Sai username hoặc password!";
            }
            else if (ex.Number == 1918)
            {
                lblError.Text = "❌ Tài khoản không tồn tại!";
            }
            else
            {
                lblError.Text = "❌ Lỗi kết nối!";
            }
        }
        catch
        {
            lblError.Text = "❌ Không thể kết nối DB!";
        }
    }
}


}
