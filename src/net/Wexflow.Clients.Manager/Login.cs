using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
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
                    var password = txtPassword.Text;
                    var user = _wexflowServiceClient.GetUser(username, password, username);

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
                            if (user.Password == GetMd5(password))
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

        public static string GetMd5(string input)
        {
            // Use input string to calculate MD5 hash
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.ASCII.GetBytes(input);
                var hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                var sb = new StringBuilder();
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < hashBytes.Length; i++)
                {
                    _ = sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
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
