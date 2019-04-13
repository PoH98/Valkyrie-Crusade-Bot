using System.Drawing;
using Emgu.CV.OCR;
using System;
using Emgu.CV.Structure;
using Emgu.CV;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.IO.Compression;

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
                t = new Tesseract();
                if (!Directory.Exists("C:\\ProgramData\\tessdata"))
                {
                    Directory.CreateDirectory("C:\\ProgramData\\tessdata");
                }
                if (!File.Exists("C:\\ProgramData\\tessdata\\eng.traineddata"))
                {
                    File.WriteAllBytes("C:\\ProgramData\\tessdata\\temp.zip", Resource.eng);
                    ZipFile.ExtractToDirectory("C:\\ProgramData\\tessdata\\temp.zip", "C:\\ProgramData\\tessdata\\");
                    File.Delete("C:\\ProgramData\\tessdata\\temp.zip");
                }
                t.Init("C:\\ProgramData\\", lang, OcrEngineMode.Default);
            }
            Image<Bgr, byte> img = new Image<Bgr, byte>(new Bitmap(BotCore.Decompress(source)));
             t.SetImage(img);
            return t.GetUTF8Text();
        }
    }
}
