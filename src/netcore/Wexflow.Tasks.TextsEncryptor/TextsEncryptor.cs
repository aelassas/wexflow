using System;
using System.IO;
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
            var status = Status.Success;
            var succeeded = true;
            var atLeastOneSuccess = false;

            try
            {
                var files = SelectFiles();
                foreach (var file in files)
                {
                    var destPath = Path.Combine(Workflow.WorkflowTempFolder, file.FileName);
                    succeeded &= Encrypt(file.Path, destPath);
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

        private bool Encrypt(string inputFile, string outputFile)
        {
            try
            {
                var srcStr = File.ReadAllText(inputFile);
                var destStr = EncryptString(srcStr, Workflow.PASS_PHRASE, Workflow.KEY_SIZE, Workflow.DERIVATION_ITERATIONS);
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
            var saltStringBytes = Generate128BitsOfRandomEntropy();
            var ivStringBytes = Generate128BitsOfRandomEntropy();
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            using Rfc2898DeriveBytes password = new(passPhrase, saltStringBytes, derivationIterations, HashAlgorithmName.SHA256);
            var keyBytes = password.GetBytes(keySize / 8);
            //using RijndaelManaged symmetricKey = new();
            using var symmetricKey = Aes.Create();
            symmetricKey.BlockSize = 128;
            symmetricKey.Mode = CipherMode.CBC;
            symmetricKey.Padding = PaddingMode.PKCS7;
            using var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes);
            using MemoryStream memoryStream = new();
            using CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
            cryptoStream.FlushFinalBlock();
            // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
            var cipherTextBytes = saltStringBytes;
            cipherTextBytes = [.. cipherTextBytes, .. ivStringBytes];
            cipherTextBytes = [.. cipherTextBytes, .. memoryStream.ToArray()];
            memoryStream.Close();
            cryptoStream.Close();
            return Convert.ToBase64String(cipherTextBytes);
        }

        private static byte[] Generate128BitsOfRandomEntropy()
        {
            var randomBytes = new byte[16]; // 16 Bytes will give us 128 bits.
            using var rngCsp = RandomNumberGenerator.Create();
            // Fill the array with cryptographically secure random bytes.
            rngCsp.GetBytes(randomBytes);
            return randomBytes;
        }
    }
}
