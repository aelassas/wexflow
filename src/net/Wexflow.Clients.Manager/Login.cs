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

        private const string ForgotPasswordPage = @"..\Backend\forgot-password.html";

        public static string Username = "";
        public static string Password = "";

        private readonly WexflowServiceClient _wexflowServiceClient;

        public Login()
        {
            InitializeComponent();
            txtPassword.PasswordChar = '*';
            lnkForgotPassword.Visible = File.Exists(ForgotPasswordPage);
            _wexflowServiceClient = new WexflowServiceClient(WexflowWebServiceUri);
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            Authenticate();
        }

        private void txtPassword_KeyUp(object sender, KeyEventArgs e)
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
                string username = txtUserName.Text;
                if (string.IsNullOrEmpty(username))
                {
                    MessageBox.Show("Type a username.");
                }
                else
                {
                    string password = txtPassword.Text;
                    User user = _wexflowServiceClient.GetUser(username, password, username);

                    if (user == null)
                    {
                        MessageBox.Show("Wrong credentials.");
                    }
                    else
                    {
                        if (user.UserProfile == UserProfile.Restricted)
                        {
                            MessageBox.Show("You do not have enough rights to access Wexflow Manager.");
                        }
                        else
                        {
                            if (user.Password == GetMd5(password))
                            {
                                Username = user.Username;
                                Password = password;

                                Manager manager = new Manager();
                                manager.Show();
                                Hide();
                            }
                            else
                            {
                                MessageBox.Show("The password is incorrect.");
                            }
                        }

                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show(@"An error occured during the authentication.", "Wexflow", MessageBoxButtons.OK);
            }

        }

        public static string GetMd5(string input)
        {
            // Use input string to calculate MD5 hash
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        private void lnkForgotPassword_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(ForgotPasswordPage))
            {
                Process.Start(ForgotPasswordPage, "");
            }
        }
    }
}
