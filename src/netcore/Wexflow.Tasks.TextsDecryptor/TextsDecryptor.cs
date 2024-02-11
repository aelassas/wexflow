using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.TextsDecryptor
{
    public class TextsDecryptor : Task
    {
        public TextsDecryptor(XElement xe, Workflow wf) : base(xe, wf)
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
                    succeeded &= Decrypt(file.Path, destPath, Workflow.PASS_PHRASE);
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

        private bool Decrypt(string inputFile, string outputFile, string passphrase)
        {
            try
            {
                var srcStr = File.ReadAllText(inputFile);
                var destStr = Decrypt(srcStr, passphrase, Workflow.KEY_SIZE, Workflow.DERIVATION_ITERATIONS);
                File.WriteAllText(outputFile, destStr);
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

        private static string Decrypt(string cipherText, string passPhrase, int keysize, int derivationIterations)
        {
            // Get the complete stream of bytes that represent:
            // [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]
            var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
            // Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
            var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(keysize / 8).ToArray();
            // Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
            var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(keysize / 8).Take(keysize / 8).ToArray();
            // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
            var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip(keysize / 8 * 2).Take(cipherTextBytesWithSaltAndIv.Length - (keysize / 8 * 2)).ToArray();

            using Rfc2898DeriveBytes password = new(passPhrase, saltStringBytes, derivationIterations, HashAlgorithmName.SHA256);
            var keyBytes = password.GetBytes(keysize / 8);
            //using RijndaelManaged symmetricKey = new();
            using var symmetricKey = Aes.Create();
            symmetricKey.BlockSize = 128;
            symmetricKey.Mode = CipherMode.CBC;
            symmetricKey.Padding = PaddingMode.PKCS7;
            using var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes);
            using MemoryStream memoryStream = new(cipherTextBytes);
            using CryptoStream cryptoStream = new(memoryStream, decryptor, CryptoStreamMode.Read);
            var plainTextBytes = new byte[cipherTextBytes.Length];
            var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
            memoryStream.Close();
            cryptoStream.Close();
            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
        }
    }
}
