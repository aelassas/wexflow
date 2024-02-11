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
            var status = Status.Success;
            var succeeded = true;
            var atLeastOneSuccess = false;

            try
            {
                var files = SelectFiles();
                foreach (var file in files)
                {
                    var destPath = Path.Combine(Workflow.WorkflowTempFolder, file.FileName);
                    succeeded &= Decrypt(file.Path, destPath, Workflow.PASS_PHRASE, Workflow.DERIVATION_ITERATIONS);
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
                using (FileStream fsCrypt = new(inputFile, FileMode.Open))
                {
                    var saltBytes = new byte[32];
                    _ = fsCrypt.Read(saltBytes, 0, saltBytes.Length);

                    UnicodeEncoding ue = new();

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

                    using CryptoStream cs = new(fsCrypt, rmcrypto.CreateDecryptor(), CryptoStreamMode.Read);
                    using FileStream fsOut = new(outputFile, FileMode.Create);
                    int data;
                    while ((data = cs.ReadByte()) != -1)
                    {
                        var b = (byte)data;
                        fsOut.WriteByte(b);
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
