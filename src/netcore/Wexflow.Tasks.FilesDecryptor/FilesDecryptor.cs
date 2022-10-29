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

        public FilesDecryptor(XElement xe, Workflow wf) : base(xe, wf)
        {
        }

        public override TaskStatus Run()
        {
            Info("Decrypting files...");
            Status status = Status.Success;
            bool succeeded = true;
            bool atLeastOneSuccess = false;

            try
            {
                var files = SelectFiles();
                foreach (var file in files)
                {
                    string destPath = Path.Combine(Workflow.WorkflowTempFolder, file.FileName);
                    succeeded &= Decrypt(file.Path, destPath, Workflow.PassPhrase, Workflow.DerivationIterations);
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
                ErrorFormat("An error occured while decrypting files: {0}", e.Message);
                status = Status.Error;
            }

            Info("Task finished");
            return new TaskStatus(status);
        }

        private bool Decrypt(string inputFile, string outputFile, string passphrase, int derivationIterations)
        {
            try
            {
                using (FileStream fsCrypt = new FileStream(inputFile, FileMode.Open))
                {
                    byte[] saltBytes = new byte[32];
                    fsCrypt.Read(saltBytes, 0, saltBytes.Length);

                    UnicodeEncoding ue = new UnicodeEncoding();

                    RijndaelManaged rmcrypto = new RijndaelManaged();
                    rmcrypto.KeySize = 256;
                    rmcrypto.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(ue.GetBytes(passphrase), saltBytes, derivationIterations);
                    rmcrypto.Key = key.GetBytes(rmcrypto.KeySize / 8);
                    rmcrypto.IV = key.GetBytes(rmcrypto.BlockSize / 8);
                    rmcrypto.Padding = PaddingMode.Zeros;
                    rmcrypto.Mode = CipherMode.CBC;


                    using (CryptoStream cs = new CryptoStream(fsCrypt, rmcrypto.CreateDecryptor(), CryptoStreamMode.Read))
                    using (FileStream fsOut = new FileStream(outputFile, FileMode.Create))
                    {
                        int data;
                        while ((data = cs.ReadByte()) != -1)
                        {
                            byte b = (byte)data;
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
