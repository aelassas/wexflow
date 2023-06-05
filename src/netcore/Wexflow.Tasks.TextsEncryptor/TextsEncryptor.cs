using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.TextsEncryptor
{
    public class TextsEncryptor : Task
    {
        public TextsEncryptor(XElement xe, Workflow wf) : base(xe, wf)
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
                FileInf[] files = SelectFiles();
                foreach (FileInf file in files)
                {
                    string destPath = Path.Combine(Workflow.WorkflowTempFolder, file.FileName);
                    succeeded &= Encrypt(file.Path, destPath, Workflow.PassPhrase);
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

        private bool Encrypt(string inputFile, string outputFile, string passphrase)
        {
            if (passphrase is null)
            {
                throw new ArgumentNullException(nameof(passphrase));
            }

            try
            {
                string srcStr = File.ReadAllText(inputFile);
                string destStr = EncryptString(srcStr, Workflow.PassPhrase, Workflow.KeySize, Workflow.DerivationIterations);
                File.WriteAllText(outputFile, destStr);
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

        private static string EncryptString(string plainText, string passPhrase, int keySize, int derivationIterations)
        {
            // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
            // so that the same Salt and IV values can be used when decrypting.  
            byte[] saltStringBytes = Generate128BitsOfRandomEntropy();
            byte[] ivStringBytes = Generate128BitsOfRandomEntropy();
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            using Rfc2898DeriveBytes password = new(passPhrase, saltStringBytes, derivationIterations);
            byte[] keyBytes = password.GetBytes(keySize / 8);
            using RijndaelManaged symmetricKey = new();
            symmetricKey.BlockSize = 128;
            symmetricKey.Mode = CipherMode.CBC;
            symmetricKey.Padding = PaddingMode.PKCS7;
            using ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes);
            using MemoryStream memoryStream = new();
            using CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
            cryptoStream.FlushFinalBlock();
            // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
            byte[] cipherTextBytes = saltStringBytes;
            cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
            cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
            memoryStream.Close();
            cryptoStream.Close();
            return Convert.ToBase64String(cipherTextBytes);
        }

        private static byte[] Generate128BitsOfRandomEntropy()
        {
            byte[] randomBytes = new byte[16]; // 16 Bytes will give us 128 bits.
            using (RNGCryptoServiceProvider rngCsp = new())
            {
                // Fill the array with cryptographically secure random bytes.
                rngCsp.GetBytes(randomBytes);
            }
            return randomBytes;
        }

    }
}
