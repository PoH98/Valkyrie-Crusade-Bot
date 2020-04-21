using System.Drawing;
using Emgu.CV.OCR;
using System;
using Emgu.CV.Structure;
using Emgu.CV;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Linq;

namespace BotFramework
{
    /// <summary>
    /// Used to regonize text on images
    /// </summary>
    public class OCR
    {
        private Tesseract t;
        private bool NumOnly;
        private static OCR Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new OCR();
                }
                return instance;
            }
        }

        private static OCR instance;
        /// <summary>
        /// OCR the image. Need Prepair OCR first!
        /// </summary>
        /// <param name="source"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        public static string OcrImage(byte[] source, string lang)
        {
            if(Instance.t == null)
            {
                throw new Exception("Run PrepairOcr First!");
            }
            int error = 0;
            while (source == null && error < 5)
            {
                if (Variables.ProchWnd != IntPtr.Zero)
                {
                    source = Screenshot.ImageCapture(Variables.ProchWnd, Variables.WinApiCaptCropStart, Variables.WinApiCaptCropEnd);
                }
                else
                {
                    source = Screenshot.ImageCapture(Variables.Proc.MainWindowHandle, Variables.WinApiCaptCropStart, Variables.WinApiCaptCropEnd);
                }
                error++;
            }

            if (source == null)
            {
                Variables.AdvanceLog("Source image is null and unable to recapture!");
                return string.Empty;
            }
            Image<Bgr, byte> img = new Image<Bgr, byte>(Screenshot.Decompress(source));
            if (File.Exists($"C:\\ProgramData\\tessdata\\{lang}.traineddata"))
            {
                FileInfo f = new FileInfo(($"C:\\ProgramData\\tessdata\\{lang}.traineddata"));
                if (f.Length == 0)
                {
                    throw new FileNotFoundException("The given ocr data is incomplete or not found!");
                }
            }
            Instance.t.SetImage(img);
            var result = Instance.t.GetUTF8Text();
            if (Instance.NumOnly)
            {
                result = new string(result.Where(Char.IsDigit).ToArray());
            }
            return result.Trim();
        }
        /// <summary>
        /// Prepair to OCR
        /// </summary>
        /// <param name="lang">Language for OCR</param>
        /// <param name="blacklist">Blacklisted characters while OCR</param>
        /// <param name="whitelist">Only allow these specific characters while OCR</param>
        /// <param name="numbersonly">Only numbers are allowed in OCR</param>
        /// <returns></returns>
        public static void PrepairOcr(string whitelist = "", string blacklist = "", string lang = "eng", bool numbersonly = false)
        {
            if (Instance.t == null)
            {
                Instance.t = new Tesseract();
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
                    Instance.t.SetVariable("tessedit_char_whitelist", whitelist);
                }
                if (blacklist.Length > 0)
                {
                    Instance.t.SetVariable("tessedit_char_blacklist", blacklist);
                }
                Instance.NumOnly = numbersonly;
                if (numbersonly)
                {
                    Instance.t.SetVariable("classify_bln_numeric_mode", "1");
                }
                else
                {
                    Instance.t.SetVariable("classify_bln_numeric_mode", "0");
                }
                try
                {
                    Instance.t.Init("C:\\ProgramData\\tessdata\\", lang, OcrEngineMode.TesseractOnly);
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
