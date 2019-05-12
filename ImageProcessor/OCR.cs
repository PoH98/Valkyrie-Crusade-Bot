using System.Drawing;
using Emgu.CV.OCR;
using System;
using Emgu.CV.Structure;
using Emgu.CV;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.IO.Compression;
using System.Security.Cryptography;

namespace BotFramework
{
    public class OCR
    {
        private static Tesseract t;
        
        /// <summary>
        /// Prepair to OCR
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        public static string OcrImage(byte[] source, string lang)
        {
            if(t == null)
            {
                throw new Exception("Run PrepairOcr First!");
            }
            Image<Bgr, byte> img = new Image<Bgr, byte>(new Bitmap(BotCore.Decompress(source)));
             t.SetImage(img);
            return t.GetUTF8Text();
        }
       
        public static void PrepairOcr(string whitelist = "", string blacklist = "", string lang = "eng")
        {
            if (t == null)
            {
                t = new Tesseract();
                if (!Directory.Exists("C:\\ProgramData\\tessdata"))
                {
                    Directory.CreateDirectory("C:\\ProgramData\\tessdata");
                }
                try
                {
                    if (File.Exists("C:\\ProgramData\\tessdata\\eng.traineddata"))
                    {
                        if (CheckSUM("C:\\ProgramData\\tessdata\\eng.traineddata") != "57E0DF3D84FED9FBF8C7A8E589F8F012")
                        {
                            File.Delete("C:\\ProgramData\\tessdata\\eng.traineddata");
                        }
                    }
                }
                catch
                {

                }
                if (!File.Exists("C:\\ProgramData\\tessdata\\eng.traineddata"))
                {
                    File.WriteAllBytes("C:\\ProgramData\\tessdata\\temp.zip", Resource.eng);
                    ZipFile.ExtractToDirectory("C:\\ProgramData\\tessdata\\temp.zip", "C:\\ProgramData\\tessdata\\");
                    File.Delete("C:\\ProgramData\\tessdata\\temp.zip");
                }
                if(whitelist.Length > 0)
                {
                    t.SetVariable("tessedit_char_whitelist", whitelist);
                }
                if (blacklist.Length > 0)
                {
                    t.SetVariable("tessedit_char_blacklist", blacklist);
                }
                t.Init("C:\\ProgramData\\tessdata\\", lang, OcrEngineMode.TesseractOnly);
            }
        }
        private static string CheckSUM(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToUpper();
                }
            }
        }
    }
}
