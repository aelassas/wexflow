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
            Status status = Status.Success;
            bool succeeded = true;
            bool atLeastOneSuccess = false;

            try
            {
                var files = SelectFiles();
                foreach (var file in files)
                {
                    string destPath = Path.Combine(Workflow.WorkflowTempFolder, file.FileName);
                    succeeded &= Encrypt(file.Path, destPath, Workflow.PassPhrase, Workflow.DerivationIterations);
                    if (!atLeastOneSuccess && succeeded) atLeastOneSuccess = true;
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
            catch (ThreadAbortException)
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
                byte[] saltBytes = GenerateRandomSalt();
                UnicodeEncoding ue = new UnicodeEncoding();

                string cryptFile = outputFile;
                RijndaelManaged rmcrypto = new RijndaelManaged();
                rmcrypto.KeySize = 256;
                rmcrypto.BlockSize = 128;

                var key = new Rfc2898DeriveBytes(ue.GetBytes(passphrase), saltBytes, derivationIterations);
                rmcrypto.Key = key.GetBytes(rmcrypto.KeySize / 8);
                rmcrypto.IV = key.GetBytes(rmcrypto.BlockSize / 8);
                rmcrypto.Padding = PaddingMode.Zeros;
                rmcrypto.Mode = CipherMode.CBC;

                using (FileStream fsCrypt = new FileStream(cryptFile, FileMode.Create))
                using (CryptoStream cs = new CryptoStream(fsCrypt, rmcrypto.CreateEncryptor(), CryptoStreamMode.Write))
                using (FileStream fsIn = new FileStream(inputFile, FileMode.Open))
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
            catch(Exception e)
            {
                ErrorFormat("An error occured while encrypting the file {0}: {1}", inputFile, e.Message);
                return false;
            }
        }

        private byte[] GenerateRandomSalt()
        {
            byte[] data = new byte[32];

            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                for (int i = 0; i < 10; i++)
                {
                    rng.GetBytes(data);
                }
            }
            return data;
        }

    }
}
