using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using Wexflow.Core.Auth;
using Wexflow.Core.Service.Client;
using Wexflow.Core.Service.Contracts;

namespace Wexflow.Clients.Manager
{
    public partial class Login : Form
    {
        private static readonly string WexflowWebServiceUri = ConfigurationManager.AppSettings["WexflowWebServiceUri"];

        private const string FORGOT_PASSWORD_PAGE = @"..\Backend\forgot-password.html";

        public static string Username = "";
        public static string Password = "";
        public static string Token = "";

        private readonly WexflowServiceClient _wexflowServiceClient;

        public Login()
        {
            InitializeComponent();
            txtPassword.PasswordChar = '*';
            lnkForgotPassword.Visible = File.Exists(FORGOT_PASSWORD_PAGE);
            _wexflowServiceClient = new WexflowServiceClient(WexflowWebServiceUri);
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            Authenticate();
        }

        private void TxtPassword_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Authenticate();
            }
        }

        private void Authenticate()
        {
            try
            {
                var username = txtUserName.Text;
                if (string.IsNullOrEmpty(username))
                {
                    _ = MessageBox.Show(@"Type a username.");
                }
                else
                {
                    string token;
                    User user;
                    var password = txtPassword.Text;
                    try
                    {
                        token = _wexflowServiceClient.Login(username, password, true);
                        Token = token;
                    }
                    catch (Exception)
                    {
                        _ = MessageBox.Show(@"Wrong credentials.");
                        return;
                    }

                    user = _wexflowServiceClient.GetUser(username, token);

                    if (user == null)
                    {
                        _ = MessageBox.Show(@"Wrong credentials.");
                    }
                    else
                    {
                        if (user.UserProfile == UserProfile.Restricted)
                        {
                            _ = MessageBox.Show(@"You do not have enough rights to access Wexflow Manager.");
                        }
                        else
                        {
                            if (PasswordHasher.VerifyPassword(password, user.Password))
                            {
                                Username = user.Username;
                                Password = password;

                                var manager = new Manager();
                                manager.Show();
                                Hide();
                            }
                            else
                            {
                                _ = MessageBox.Show(@"The password is incorrect.");
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                _ = MessageBox.Show(@"An error occured during the authentication.", @"Wexflow", MessageBoxButtons.OK);
            }
        }

        private void LnkForgotPassword_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(FORGOT_PASSWORD_PAGE))
            {
                _ = Process.Start(FORGOT_PASSWORD_PAGE, "");
            }
        }
    }
}
