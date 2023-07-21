using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.FilesDecryptor
{
    public class FilesDecryptor : Task
    {
        public string SmbComputerName { get; }
        public string SmbDomain { get; }
        public string SmbUsername { get; }
        public string SmbPassword { get; }

        public FilesDecryptor(XElement xe, Workflow wf) : base(xe, wf)
        {
            SmbComputerName = GetSetting("smbComputerName");
            SmbDomain = GetSetting("smbDomain");
            SmbUsername = GetSetting("smbUsername");
            SmbPassword = GetSetting("smbPassword");
        }

        public override TaskStatus Run()
        {
            Info("Decrypting files...");

            bool success;
            var atLeastOneSuccess = false;

            try
            {
                if (!string.IsNullOrEmpty(SmbComputerName) && !string.IsNullOrEmpty(SmbUsername) && !string.IsNullOrEmpty(SmbPassword))
                {
                    using (NetworkShareAccesser.Access(SmbComputerName, SmbDomain, SmbUsername, SmbPassword))
                    {
                        success = DecryptFiles(ref atLeastOneSuccess);
                    }
                }
                else
                {
                    success = DecryptFiles(ref atLeastOneSuccess);
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while decrypting files.", e);
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
            return new TaskStatus(status);
        }

        private bool DecryptFiles(ref bool atLeastOneSuccess)
        {
            var success = true;
            try
            {
                var files = SelectFiles();
                foreach (var file in files)
                {
                    var destPath = Path.Combine(Workflow.WorkflowTempFolder, file.FileName);
                    success &= Decrypt(file.Path, destPath, Workflow.PASS_PHRASE, Workflow.DERIVATION_ITERATIONS);
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
                ErrorFormat("An error occured while decrypting files: {0}", e.Message);
                success = false;
            }
            return success;
        }

        private bool Decrypt(string inputFile, string outputFile, string passphrase, int derivationIterations)
        {
            try
            {
                using (var fsCrypt = new FileStream(inputFile, FileMode.Open))
                {
                    var saltBytes = new byte[32];
                    _ = fsCrypt.Read(saltBytes, 0, saltBytes.Length);

                    var ue = new UnicodeEncoding();

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

                    using (var cs = new CryptoStream(fsCrypt, rmcrypto.CreateDecryptor(), CryptoStreamMode.Read))
                    using (var fsOut = new FileStream(outputFile, FileMode.Create))
                    {
                        int data;
                        while ((data = cs.ReadByte()) != -1)
                        {
                            var b = (byte)data;
                            fsOut.WriteByte(b);
                        }
                    }
                }

                InfoFormat("The file {0} has been decrypted -> {1}", inputFile, outputFile);
                Files.Add(new FileInf(outputFile, Id));
                return true;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while decrypting the file {0}: {1}", inputFile, e.Message);
                return false;
            }
        }
    }
}
