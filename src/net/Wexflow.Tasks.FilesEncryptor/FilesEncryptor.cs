using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.FilesEncryptor
{
    public class FilesEncryptor : Task
    {
        public string SmbComputerName { get; }
        public string SmbDomain { get; }
        public string SmbUsername { get; }
        public string SmbPassword { get; }

        public FilesEncryptor(XElement xe, Workflow wf) : base(xe, wf)
        {
            SmbComputerName = GetSetting("smbComputerName");
            SmbDomain = GetSetting("smbDomain");
            SmbUsername = GetSetting("smbUsername");
            SmbPassword = GetSetting("smbPassword");
        }

        public override TaskStatus Run()
        {
            Info("Encrypting files...");

            bool success;
            var atLeastOneSuccess = false;

            try
            {
                if (!string.IsNullOrEmpty(SmbComputerName) && !string.IsNullOrEmpty(SmbUsername) && !string.IsNullOrEmpty(SmbPassword))
                {
                    using (NetworkShareAccesser.Access(SmbComputerName, SmbDomain, SmbUsername, SmbPassword))
                    {
                        success = EncryptFiles(ref atLeastOneSuccess);
                    }
                }
                else
                {
                    success = EncryptFiles(ref atLeastOneSuccess);
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while encrypting files.", e);
                success = false;
            }

            var status = Status.Success;

            if (!success && atLeastOneSuccess)
            {
                status = Status.Warning;
            }
            else if (!success)
            {
                status = Status.Error;
            }

            Info("Task finished.");
            return new TaskStatus(status, false);
        }

        private bool EncryptFiles(ref bool atLeastOneSuccess)
        {
            var success = true;
            try
            {
                var files = SelectFiles();
                foreach (var file in files)
                {
                    var destPath = Path.Combine(Workflow.WorkflowTempFolder, file.FileName);
                    success &= Encrypt(file.Path, destPath, Workflow.PASS_PHRASE, Workflow.DERIVATION_ITERATIONS);
                    if (!atLeastOneSuccess && success)
                    {
                        atLeastOneSuccess = true;
                    }
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while encrypting files: {0}", e.Message);
                success = false;
            }
            return success;
        }

        private bool Encrypt(string inputFile, string outputFile, string passphrase, int derivationIterations)
        {
            try
            {
                //byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
                var saltBytes = GenerateRandomSalt();
                var ue = new UnicodeEncoding();

                var cryptFile = outputFile;
                var rmcrypto = new RijndaelManaged
                {
                    KeySize = 256,
                    BlockSize = 128
                };

                var key = new Rfc2898DeriveBytes(ue.GetBytes(passphrase), saltBytes, derivationIterations);
                rmcrypto.Key = key.GetBytes(rmcrypto.KeySize / 8);
                rmcrypto.IV = key.GetBytes(rmcrypto.BlockSize / 8);
                rmcrypto.Padding = PaddingMode.Zeros;
                rmcrypto.Mode = CipherMode.CBC;

                using (var fsCrypt = new FileStream(cryptFile, FileMode.Create))
                using (var cs = new CryptoStream(fsCrypt, rmcrypto.CreateEncryptor(), CryptoStreamMode.Write))
                using (var fsIn = new FileStream(inputFile, FileMode.Open))
                {
                    fsCrypt.Write(saltBytes, 0, saltBytes.Length);
                    int data;
                    while ((data = fsIn.ReadByte()) != -1)
                    {
                        cs.WriteByte((byte)data);
                    }
                }

                InfoFormat("The file {0} has been encrypted -> {1}", inputFile, outputFile);
                Files.Add(new FileInf(outputFile, Id));
                return true;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while encrypting the file {0}: {1}", inputFile, e.Message);
                return false;
            }
        }

        private byte[] GenerateRandomSalt()
        {
            var data = new byte[32];

            using (var rng = new RNGCryptoServiceProvider())
            {
                for (var i = 0; i < 10; i++)
                {
                    rng.GetBytes(data);
                }
            }
            return data;
        }
    }
}
