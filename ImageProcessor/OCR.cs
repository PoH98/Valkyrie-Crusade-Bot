using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.OCR;
using Emgu.CV.Structure;
using Emgu.CV.Text;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;


namespace ImageProcessor
{
    public class OCR
    {
        private static Tesseract _ocr;
        /// <summary>
        /// Prepair to OCR
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        public static bool PrepairOcr(string lang)
        {
            try
            {
                if (_ocr != null)
                {
                    _ocr.Dispose();
                    _ocr = null;
                }
                _ocr = new Tesseract(Environment.CurrentDirectory + "\\tessdata", lang,OcrEngineMode.Default);
                return true;
            }
            catch (Exception e)
            {
                _ocr = null;
                Variables.AdbLog.Add("OCR config failed! error code: " + e.HResult);
                return false;
            }
        }

        private static string ReadText(Tesseract ocr, Mat image, bool ReadText, Mat imageColor)
        {
            if (image.NumberOfChannels == 1)
                CvInvoke.CvtColor(image, imageColor, ColorConversion.Gray2Bgr);
            else
                image.CopyTo(imageColor);

            if (ReadText)
            {
                ocr.SetImage(imageColor);

                if (ocr.Recognize() != 0)
                    throw new Exception("Failed to recognizer image");
                Tesseract.Character[] characters = ocr.GetCharacters();
                if (characters.Length == 0)
                {
                    Mat imgGrey = new Mat();
                    CvInvoke.CvtColor(image, imgGrey, ColorConversion.Bgr2Gray);
                    Mat imgThresholded = new Mat();
                    CvInvoke.Threshold(imgGrey, imgThresholded, 65, 255, ThresholdType.Binary);
                    ocr.SetImage(imgThresholded);
                    characters = ocr.GetCharacters();
                    imageColor = imgThresholded;
                    if (characters.Length == 0)
                    {
                        CvInvoke.Threshold(image, imgThresholded, 190, 255, ThresholdType.Binary);
                        ocr.SetImage(imgThresholded);
                        characters = ocr.GetCharacters();
                        imageColor = imgThresholded;
                    }
                }

                return ocr.GetUTF8Text();

            }
            else
            {
                bool checkInvert = true;

                Rectangle[] regions;

                using (
                   ERFilterNM1 er1 = new ERFilterNM1("trained_classifierNM1.xml", 8, 0.00025f, 0.13f, 0.4f, true, 0.1f))
                using (ERFilterNM2 er2 = new ERFilterNM2("trained_classifierNM2.xml", 0.3f))
                {
                    int channelCount = image.NumberOfChannels;
                    UMat[] channels = new UMat[checkInvert ? channelCount * 2 : channelCount];

                    for (int i = 0; i < channelCount; i++)
                    {
                        UMat c = new UMat();
                        CvInvoke.ExtractChannel(image, c, i);
                        channels[i] = c;
                    }

                    if (checkInvert)
                    {
                        for (int i = 0; i < channelCount; i++)
                        {
                            UMat c = new UMat();
                            CvInvoke.BitwiseNot(channels[i], c);
                            channels[i + channelCount] = c;
                        }
                    }

                    VectorOfERStat[] regionVecs = new VectorOfERStat[channels.Length];
                    for (int i = 0; i < regionVecs.Length; i++)
                        regionVecs[i] = new VectorOfERStat();

                    try
                    {
                        for (int i = 0; i < channels.Length; i++)
                        {
                            er1.Run(channels[i], regionVecs[i]);
                            er2.Run(channels[i], regionVecs[i]);
                        }
                        using (VectorOfUMat vm = new VectorOfUMat(channels))
                        {
                            regions = ERFilter.ERGrouping(image, vm, regionVecs, ERFilter.GroupingMethod.OrientationHoriz,
                               "trained_classifier_erGrouping.xml", 0.5f);
                        }
                    }
                    finally
                    {
                        foreach (UMat tmp in channels)
                            if (tmp != null)
                                tmp.Dispose();
                        foreach (VectorOfERStat tmp in regionVecs)
                            if (tmp != null)
                                tmp.Dispose();
                    }

                    Rectangle imageRegion = new Rectangle(Point.Empty, imageColor.Size);
                    for (int i = 0; i < regions.Length; i++)
                    {
                        Rectangle r = ScaleRectangle(regions[i], 1.1);

                        r.Intersect(imageRegion);
                        regions[i] = r;
                    }

                }


                List<Tesseract.Character> allChars = new List<Tesseract.Character>();
                String allText = String.Empty;
                foreach (Rectangle rect in regions)
                {
                    using (Mat region = new Mat(image, rect))
                    {
                        ocr.SetImage(region);
                        if (ocr.Recognize() != 0)
                            throw new Exception("Failed to recognize image");
                        Tesseract.Character[] characters = ocr.GetCharacters();

                        //convert the coordinates from the local region to global
                        for (int i = 0; i < characters.Length; i++)
                        {
                            Rectangle charRegion = characters[i].Region;
                            charRegion.Offset(rect.Location);
                            characters[i].Region = charRegion;

                        }
                        allChars.AddRange(characters);

                        allText += ocr.GetUTF8Text() + Environment.NewLine;

                    }
                }
                return allText;

            }
        }
        private static Rectangle ScaleRectangle(Rectangle r, double scale)
        {
            double centerX = r.Location.X + r.Width / 2.0;
            double centerY = r.Location.Y + r.Height / 2.0;
            double newWidth = Math.Round(r.Width * scale);
            double newHeight = Math.Round(r.Height * scale);
            return new Rectangle((int)Math.Round(centerX - newWidth / 2.0), (int)Math.Round(centerY - newHeight / 2.0),
               (int)newWidth, (int)newHeight);
        }

        public static string OcrImage(byte[] source)
        {
            Mat result = new Mat();
            return ReadText(_ocr, GetMatFromSDImage(EmulatorController.Decompress(source)), true, result);
        }

        private static Mat GetMatFromSDImage(Image image)
        {
            int stride = 0;
            Bitmap bmp = new Bitmap(image);

            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);

            System.Drawing.Imaging.PixelFormat pf = bmp.PixelFormat;
            if (pf == System.Drawing.Imaging.PixelFormat.Format32bppArgb)
            {
                stride = bmp.Width * 4;
            }
            else
            {
                stride = bmp.Width * 3;
            }

            Image<Bgra, byte> cvImage = new Image<Bgra, byte>(bmp.Width, bmp.Height, stride, (IntPtr)bmpData.Scan0);

            bmp.UnlockBits(bmpData);

            return cvImage.Mat;
        }
    }
}
