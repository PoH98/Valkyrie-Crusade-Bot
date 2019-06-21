using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace BotFramework
{
    /// <summary>
    /// All the data encryption which you do not want user to see!!
    /// </summary>
    public class Encryption
    {
        /// <summary>
        /// Encrypt text
        /// </summary>
        /// <param name="text">text for encryption</param>
        /// <returns></returns>
        public static string Encrypt(string text)
        {
            if (text == null)
            {
                return "";
            }
            StringBuilder sb = new StringBuilder();
            foreach (char c in text)
            {
                char enc = c;
                char y = (char)(Convert.ToUInt16(enc) + 14);
                sb.Append(y);
            }
            return sb.ToString();
        }
        /// <summary>
        /// Decrypt text
        /// </summary>
        /// <param name="text">text for decryption</param>
        /// <returns></returns>
        public static string Decrypt(string text)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in text)
            {
                char enc = c;
                char y = (char)(Convert.ToUInt16(enc) - 14);
                sb.Append(y);
            }
            return sb.ToString();
        }
        /// <summary>
        /// Hash SHA256 of string
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string SHA256(string text)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(text);
            SHA256Managed hashstring = new SHA256Managed();
            byte[] hash = hashstring.ComputeHash(bytes);
            string hashString = string.Empty;
            foreach (byte x in hash)
            {
                hashString += String.Format("{0:x2}", x);
            }
            return hashString;
        }
        /// <summary>
        /// Hash some bytes into SHA256
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string SHA256(byte[] bytes)
        {
            SHA256Managed hashstring = new SHA256Managed();
            byte[] hash = hashstring.ComputeHash(bytes);
            string hashString = string.Empty;
            foreach (byte x in hash)
            {
                hashString += String.Format("{0:x2}", x);
            }
            return hashString;
        }
        /// <summary>
        /// Encrypt data and save as xml file. Decrypt first before any process!
        /// </summary>
        /// <param name="image">Image for encrypt</param>
        /// <param name="filename">The file name of image, do not include any .png or .jpg! It will directly save as xml format!</param>
        /// <returns></returns>
        public static void EncryptData(byte[] image, string filename)
        {
            RijndaelManaged RijndaelCipher = new RijndaelManaged();
            string passPhrase = "PoH98";
            string saltValue = "PoH98";
            RijndaelCipher.Mode = CipherMode.CBC;
            byte[] salt = Encoding.ASCII.GetBytes(saltValue);
            PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, salt, "SHA256", 2);
            ICryptoTransform Encryptor = RijndaelCipher.CreateEncryptor(password.GetBytes(32), password.GetBytes(16));
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, Encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(image, 0, image.Length);
            cryptoStream.FlushFinalBlock();
            byte[] CipherBytes = memoryStream.ToArray();
            memoryStream.Close();
            cryptoStream.Close();
            File.WriteAllBytes(filename + ".xml",CipherBytes);
        }
        /// <summary>
        /// Read encrypted xml data and make it back to image bytes!
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>image bytes</returns>
        public static byte[] DecryptData(string filename)
        {
            if (!filename.Contains(".xml"))
            {
                throw new Exception("File must be in xml format!!");
            }
            byte[] encryptedBytes = File.ReadAllBytes(filename);
            RijndaelManaged RijndaelCipher = new RijndaelManaged();
            string passPhrase = "PoH98";
            string saltValue = "PoH98";
            RijndaelCipher.Mode = CipherMode.CBC;
            byte[] salt = Encoding.ASCII.GetBytes(saltValue);
            PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, salt, "SHA256", 2);
            ICryptoTransform Decryptor = RijndaelCipher.CreateDecryptor(password.GetBytes(32), password.GetBytes(16));
            MemoryStream memoryStream = new MemoryStream(encryptedBytes);
            CryptoStream cryptoStream = new CryptoStream(memoryStream, Decryptor, CryptoStreamMode.Read);
            byte[] plainBytes = new byte[encryptedBytes.Length];
            int DecryptedCount = cryptoStream.Read(plainBytes, 0, plainBytes.Length);
            memoryStream.Close();
            cryptoStream.Close();
            return plainBytes;
        }
        /// <summary>
        /// Hash SHA512 of string
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string SHA512(string text)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(text);
            SHA512Managed hashstring = new SHA512Managed();
            byte[] hash = hashstring.ComputeHash(bytes);
            string hashString = string.Empty;
            foreach (byte x in hash)
            {
                hashString += String.Format("{0:x2}", x);
            }
            return hashString;
        }
        /// <summary>
        /// Hash SHA512 of bytes
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string SHA512(byte[] bytes)
        {
            SHA512Managed hashstring = new SHA512Managed();
            byte[] hash = hashstring.ComputeHash(bytes);
            string hashString = string.Empty;
            foreach (byte x in hash)
            {
                hashString += String.Format("{0:x2}", x);
            }
            return hashString;
        }
    }
}
