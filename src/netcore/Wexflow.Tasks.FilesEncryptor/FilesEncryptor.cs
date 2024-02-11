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
        public FilesEncryptor(XElement xe, Workflow wf) : base(xe, wf)
        {
        }

        public override TaskStatus Run()
        {
            Info("Encrypting files...");
            var status = Status.Success;
            var succeeded = true;
            var atLeastOneSuccess = false;

            try
            {
                var files = SelectFiles();
                foreach (var file in files)
                {
                    var destPath = Path.Combine(Workflow.WorkflowTempFolder, file.FileName);
                    succeeded &= Encrypt(file.Path, destPath, Workflow.PASS_PHRASE, Workflow.DERIVATION_ITERATIONS);
                    if (!atLeastOneSuccess && succeeded)
                    {
                        atLeastOneSuccess = true;
                    }
                    WaitOne();
                }

                if (!succeeded && atLeastOneSuccess)
                {
                    status = Status.Warning;
                }
                else if (!succeeded)
                {
                    status = Status.Error;
                }
            }
            catch (ThreadInterruptedException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while encrypting files: {0}", e.Message);
                status = Status.Error;
            }

            Info("Task finished");
            return new TaskStatus(status);
        }

        private bool Encrypt(string inputFile, string outputFile, string passphrase, int derivationIterations)
        {
            try
            {
                //byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
                var saltBytes = GenerateRandomSalt();
                UnicodeEncoding ue = new();

                var cryptFile = outputFile;
                //RijndaelManaged rmcrypto = new()
                //{
                //    KeySize = 256,
                //    BlockSize = 128
                //};
                using var rmcrypto = Aes.Create();
                rmcrypto.KeySize = 256;
                rmcrypto.BlockSize = 128;
                Rfc2898DeriveBytes key = new(ue.GetBytes(passphrase), saltBytes, derivationIterations, HashAlgorithmName.SHA256);
                rmcrypto.Key = key.GetBytes(rmcrypto.KeySize / 8);
                rmcrypto.IV = key.GetBytes(rmcrypto.BlockSize / 8);
                rmcrypto.Padding = PaddingMode.PKCS7;
                rmcrypto.Mode = CipherMode.CBC;

                using (FileStream fsCrypt = new(cryptFile, FileMode.Create))
                using (CryptoStream cs = new(fsCrypt, rmcrypto.CreateEncryptor(), CryptoStreamMode.Write))
                using (FileStream fsIn = new(inputFile, FileMode.Open))
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

        private static byte[] GenerateRandomSalt()
        {
            var randomBytes = new byte[32];

            using var rngCsp = RandomNumberGenerator.Create();
            // Fill the array with cryptographically secure random bytes.
            rngCsp.GetBytes(randomBytes);
            return randomBytes;
        }
    }
}
