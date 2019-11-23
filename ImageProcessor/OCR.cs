using System.Drawing;
using Emgu.CV.OCR;
using System;
using Emgu.CV.Structure;
using Emgu.CV;
using System.Windows.Forms;
using System.IO;
using System.Net;

namespace BotFramework
{
    /// <summary>
    /// Used to regonize text on images
    /// </summary>
    public class OCR
    {
        private static Tesseract t;
        /// <summary>
        /// OCR the image. Need Prepair OCR first!
        /// </summary>
        /// <param name="source"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        public static string OcrImage(byte[] source, string lang)
        {
            if(t == null)
            {
                throw new Exception("Run PrepairOcr First!");
            }
            Image<Bgr, byte> img = new Image<Bgr, byte>(BotCore.Decompress(source));
            if (File.Exists($"C:\\ProgramData\\tessdata\\{lang}.traineddata"))
            {
                FileInfo f = new FileInfo(($"C:\\ProgramData\\tessdata\\{lang}.traineddata"));
                if (f.Length == 0)
                {
                    throw new FileNotFoundException("The given ocr data is incomplete or not found!");
                }
            }
            t.SetImage(img);
            return t.GetUTF8Text();
        }
        /// <summary>
        /// Prepair to OCR
        /// </summary>
        /// <param name="lang">Language for OCR</param>
        /// <param name="blacklist">Blacklisted characters while OCR</param>
        /// <param name="whitelist">Only allow these specific characters while OCR</param>
        /// <returns></returns>
        public static void PrepairOcr(string whitelist = "", string blacklist = "", string lang = "eng", bool numbersonly = false)
        {
            if (t == null)
            {
                t = new Tesseract();
                if (!Directory.Exists("C:\\ProgramData\\tessdata"))
                {
                    Directory.CreateDirectory("C:\\ProgramData\\tessdata");
                }
                if (File.Exists($"C:\\ProgramData\\tessdata\\{lang}.traineddata"))
                {
                    FileInfo f = new FileInfo(($"C:\\ProgramData\\tessdata\\{lang}.traineddata"));
                    if(f.Length == 0)
                    {
                        File.Delete(($"C:\\ProgramData\\tessdata\\{lang}.traineddata"));
                    }
                }
                Loop:
                while (!File.Exists($"C:\\ProgramData\\tessdata\\{lang}.traineddata"))
                {
                    Download d = new Download();
                    d.url = $"https://github.com/tesseract-ocr/tessdata/raw/master/{lang}.traineddata";
                    d.path = $"C:\\ProgramData\\tessdata\\{lang}.traineddata";
                    d.ShowDialog();
                }
                if(whitelist.Length > 0)
                {
                    t.SetVariable("tessedit_char_whitelist", whitelist);
                }
                if (blacklist.Length > 0)
                {
                    t.SetVariable("tessedit_char_blacklist", blacklist);
                }
                if (numbersonly)
                {
                    t.SetVariable("classify_bln_numeric_mode", "1");
                }
                else
                {
                    t.SetVariable("classify_bln_numeric_mode", "0");
                }
                try
                {
                    t.Init("C:\\ProgramData\\tessdata\\", lang, OcrEngineMode.TesseractOnly);
                }
                catch
                {
                    File.Delete($"C:\\ProgramData\\tessdata\\{lang}.traineddata");
                    goto Loop;
                }
            }
        }
    }
}
