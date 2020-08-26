using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using SharpAdbClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Runtime.InteropServices;

namespace BotFramework
{
    /// <summary>
    /// Screenshots from emulator
    /// </summary>
    public class Screenshot
    {
        /// <summary>
        /// The instance of the screenshot
        /// </summary>
        public static Screenshot Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new Screenshot();
                }
                return instance;
            }
        }

        private static Screenshot instance;
        /// <summary>
        /// The lock object
        /// </summary>
        public object locker = new object();

        private bool captureerror = false;

        private ImageConverter _imageConverter = new ImageConverter();
        /// <summary>
        /// Compress image into byte array to avoid conflict while multiple function trying to access the image
        /// </summary>
        /// <param name="image">The image for compress</param>
        /// <returns></returns>
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        public static byte[] Compress(Image image)
        {
            lock (Instance.locker)
            {
                try
                {
                    byte[] xByte = (byte[])Instance._imageConverter.ConvertTo(image, typeof(byte[]));
                    return xByte;
                }
                catch (Exception ex)
                {
                    Variables.AdvanceLog(ex.ToString());
                    return new byte[Variables.EmulatorHeight*Variables.EmulatorWidth];
                }
            }
        }
        /// <summary>
        /// Decompress the byte array back to image for other usage
        /// </summary>
        /// <param name="buffer">the byte array of image compressed by Compress(Image image)</param>
        /// <returns>Image</returns>
        public static Bitmap Decompress(byte[] buffer)
        {
            lock (Instance.locker)
            {
                try
                {
                    using (var ms = new MemoryStream(buffer))
                    {
                        return Image.FromStream(ms) as Bitmap;
                    }
                }
                catch (Exception ex)
                {
                    Variables.AdvanceLog(ex.ToString());
                    Bitmap bmp = new Bitmap(Variables.EmulatorWidth, Variables.EmulatorHeight);
                    using (Graphics graph = Graphics.FromImage(bmp))
                    {
                        Rectangle ImageSize = new Rectangle(0, 0, Variables.EmulatorWidth, Variables.EmulatorHeight);
                        graph.FillRectangle(Brushes.Black, ImageSize);
                    }
                    return bmp;
                }
            }
        }
        /// <summary> 
        /// Crop the image and return the cropped image
        /// </summary>
        /// <param name="original">Image that need to be cropped</param>
        /// <param name="Start">Starting Point</param>
        /// <param name="End">Ending Point</param>
        /// <param name="caller"></param>
        /// <param name="lineNumber"></param>
        /// <returns></returns>
        public static byte[] CropImage(byte[] original, Point Start, Point End, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            if (!ScriptRun.Run)
            {
                return null;
            }
            return Compress(CropImage(Decompress(original), Start, End, lineNumber, caller));
        }

        private static Bitmap CropImage(Bitmap original, Point start, Point End, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            lock (Instance.locker)
            {
                Stopwatch s = Stopwatch.StartNew();
                if (original == null)
                {
                    Variables.AdvanceLog("Result return null because of null original image");
                    return null;
                }
                Image<Bgr, byte> imgInput = new Image<Bgr, byte>(original);
                Rectangle rect = new Rectangle
                {
                    X = Math.Min(start.X, End.X),
                    Y = Math.Min(start.Y, End.Y),
                    Width = Math.Abs(start.X - End.X),
                    Height = Math.Abs(start.Y - End.Y)
                };
                imgInput.ROI = rect;
                Image<Bgr, byte> temp = imgInput.CopyBlank();
                imgInput.CopyTo(temp);
                imgInput.Dispose();
                s.Stop();
                Variables.AdvanceLog("Image cropped. Used time: " + s.ElapsedMilliseconds + " ms", lineNumber, caller);
                return temp.Bitmap;
            }
        }
        /// <summary>
        /// Capture image using WinAPI
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="cropstart"></param>
        /// <param name="cropend"></param>
        /// <param name="lineNumber"></param>
        /// <param name="caller"></param>
        /// <returns></returns>
        public static byte[] ImageCapture(IntPtr hWnd, Point cropstart, Point cropend, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            try
            {
                Stopwatch s = Stopwatch.StartNew();
                Rectangle rc = new Rectangle();
                DllImport.GetWindowRect(hWnd, ref rc);
                Bitmap bmp = new Bitmap(rc.Width, rc.Height, PixelFormat.Format32bppArgb);
                Graphics gfxBmp = Graphics.FromImage(bmp);
                IntPtr hdcBitmap = gfxBmp.GetHdc();
                DllImport.PrintWindow(hWnd, hdcBitmap, 0);
                gfxBmp.ReleaseHdc(hdcBitmap);
                gfxBmp.Dispose();
                Variables.AdvanceLog("Screenshot saved to memory used " + s.ElapsedMilliseconds + " ms", lineNumber, caller);
                s.Stop();
                bmp = CropImage(bmp, cropstart, cropend, lineNumber, caller);
                if (Variables.ImageDebug)
                {
                    bmp.Save("Profiles\\Logs\\" + Encryption.SHA256(DateTime.Now.ToString()) + ".bmp");
                }
                return Compress(bmp);
            }
            catch
            {
                Instance.captureerror = true;
                return ImageCapture();
            }
        }

        /// <summary>
        /// Fast Capturing screen and return the image, uses WinAPI capture if Variables.Background is false.
        /// </summary>
        public static byte[] ImageCapture([CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            if (!ScriptRun.Run)
            {
                return null;
            }
            if (Variables.WinApiCapt && !Instance.captureerror)
            {
                if (Variables.ProchWnd != IntPtr.Zero)
                {
                    return ImageCapture(Variables.ProchWnd, Variables.WinApiCaptCropStart, Variables.WinApiCaptCropEnd, lineNumber, caller);
                }
                else
                {
                    return ImageCapture(Variables.Proc.MainWindowHandle, Variables.WinApiCaptCropStart, Variables.WinApiCaptCropEnd, lineNumber, caller);
                }
            }
            Instance.captureerror = false;
            try
            {
                Stopwatch s = Stopwatch.StartNew();
                if (!Directory.Exists(Variables.SharedPath))
                {
                    Variables.ScriptLog("Warning, unable to find shared folder! Trying to use WinAPI!", Color.Red);
                    Variables.WinApiCapt = true;
                    return ImageCapture(Variables.Proc.MainWindowHandle, Variables.WinApiCaptCropStart, Variables.WinApiCaptCropEnd, lineNumber, caller);
                }
                if (AdbInstance.Instance.pcimagepath == "" || AdbInstance.Instance.androidimagepath == "")
                {
                    var tempname = Encryption.SHA256(DateTime.Now.ToString());
                    AdbInstance.Instance.pcimagepath = (Variables.SharedPath + "\\" + tempname + ".rgba").Replace("\\\\", "\\");
                    if (Variables.AndroidSharedPath.Contains("|"))
                    {
                        foreach (var path in Variables.AndroidSharedPath.Split('|'))
                        {
                            if (EmulatorLoader.AndroidDirectoryExist(path))
                            {
                                string temppath = path;
                                if (temppath.Last() != '/')
                                {
                                    temppath += "/";
                                }
                                AdbInstance.Instance.androidimagepath = (temppath + tempname + ".rgba");
                                Variables.AdvanceLog("Multiple Android Path settes, selected " + AdbInstance.Instance.androidimagepath);
                                break;
                            }
                        }
                    }
                    else
                    {
                        AdbInstance.Instance.androidimagepath = (Variables.AndroidSharedPath + tempname + ".rgba");
                    }
                }
                byte[] raw = null;

                if (Variables.Controlled_Device == null)
                {
                    Variables.AdvanceLog("No device connected!", lineNumber, caller);
                    EmulatorLoader.ConnectAndroidEmulator();
                    return null;
                }
                if ((Variables.Controlled_Device as DeviceData).State == SharpAdbClient.DeviceState.Offline || !ScriptRun.Run)
                {
                    return null;
                }
                ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
                AdbInstance.Instance.client.ExecuteRemoteCommand("screencap " + AdbInstance.Instance.androidimagepath, (Variables.Controlled_Device as DeviceData), receiver);
                if (Variables.NeedPull)
                {
                    if (File.Exists(AdbInstance.Instance.pcimagepath))
                    {
                        File.Delete(AdbInstance.Instance.pcimagepath);
                    }
                    BotCore.Pull(AdbInstance.Instance.androidimagepath, AdbInstance.Instance.pcimagepath);
                }
                if (!File.Exists(AdbInstance.Instance.pcimagepath))
                {
                    Variables.AdvanceLog("Unable to read rgba file because of file not exist!", lineNumber, caller);
                    return null;
                }
                raw = File.ReadAllBytes(AdbInstance.Instance.pcimagepath);
                int expectedsize = (Variables.EmulatorHeight * Variables.EmulatorWidth * 4) + 12;
                if (raw.Length != expectedsize || raw.Length > int.MaxValue || raw.Length < 1)
                {
                    //Image is not in same size, resize emulator
                    EmulatorLoader.ResizeEmulator();
                    return null;
                }
                byte[] img = new byte[raw.Length - 12]; //remove header
                Array.Copy(raw, 12, img, 0, img.Length);
                Image<Rgba, byte> image = new Image<Rgba, byte>(Variables.EmulatorWidth, Variables.EmulatorHeight);
                image.Bytes = img;
                if (Variables.ImageDebug)
                {
                    image.Save("Profiles\\Logs\\" + Encryption.SHA256(DateTime.Now.ToString()) + ".bmp");
                }
                Variables.AdvanceLog("Screenshot saved to memory used " + s.ElapsedMilliseconds + " ms", lineNumber, caller);
                s.Stop();
                return Compress(image.Bitmap);
            }
            catch (IOException)
            {

            }
            catch (ArgumentOutOfRangeException)
            {

            }
            return null;
        }
        /// <summary>
        /// Rotate image
        /// </summary>
        /// <param name="image"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static Bitmap RotateImage(Image image, float angle)
        {
            if (!ScriptRun.Run)
            {
                return null;
            }
            if (image == null)
                throw new ArgumentNullException("image is not exist!");
            PointF offset = new PointF((float)image.Width / 2, (float)image.Height / 2);
            Bitmap rotatedBmp = new Bitmap(image.Width, image.Height);
            rotatedBmp.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            Graphics g = Graphics.FromImage(rotatedBmp);
            g.TranslateTransform(offset.X, offset.Y);
            g.RotateTransform(angle);
            g.TranslateTransform(-offset.X, -offset.Y);
            g.DrawImage(image, new PointF(0, 0));

            return rotatedBmp;
        }
        /// <summary>
        /// Enlarge image and its pixel amounts
        /// </summary>
        /// <param name="image"></param>
        /// <param name="width">The new width of image</param>
        /// <param name="height">The new height of image</param>
        /// <returns></returns>
        public static Bitmap EnlargeImage(Bitmap image, int width, int height)
        {
            Image<Rgb, byte> captureImage = new Image<Rgb, byte>(image);
            Image<Rgb, byte> resizedImage = captureImage.Resize(width, height, Inter.Linear);
            return resizedImage.ToBitmap();
        }
    }

    #region Native Win32 Interop
    /// &lt;summary&gt;
    /// The RECT structure defines the coordinates of the upper-left and lower-right corners of a rectangle.
    /// &lt;/summary&gt;
    [Serializable, StructLayout(LayoutKind.Sequential)]
    internal struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public RECT(int left, int top, int right, int bottom)
        {
            this.Left = left;
            this.Top = top;
            this.Right = right;
            this.Bottom = bottom;
        }

        public Rectangle AsRectangle
        {
            get
            {
                return new Rectangle(this.Left, this.Top, this.Right - this.Left, this.Bottom - this.Top);
            }
        }

        public static RECT FromXYWH(int x, int y, int width, int height)
        {
            return new RECT(x, y, x + width, y + height);
        }

        public static RECT FromRectangle(Rectangle rect)
        {
            return new RECT(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }
    }

    [SuppressUnmanagedCodeSecurity()]
    internal sealed class NativeMethods
    {
        [DllImport("user32.dll")]
        internal static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        /// &lt;summary&gt;
        /// Get a windows client rectangle in a .NET structure
        /// &lt;/summary&gt;
        /// &lt;param name="hwnd"&gt;The window handle to look up&lt;/param&gt;
        /// &lt;returns&gt;The rectangle&lt;/returns&gt;
        internal static Rectangle GetClientRect(IntPtr hwnd)
        {
            RECT rect = new RECT();
            GetClientRect(hwnd, out rect);
            return rect.AsRectangle;
        }

        /// &lt;summary&gt;
        /// Get a windows rectangle in a .NET structure
        /// &lt;/summary&gt;
        /// &lt;param name="hwnd"&gt;The window handle to look up&lt;/param&gt;
        /// &lt;returns&gt;The rectangle&lt;/returns&gt;
        internal static Rectangle GetWindowRect(IntPtr hwnd)
        {
            RECT rect = new RECT();
            GetWindowRect(hwnd, out rect);
            return rect.AsRectangle;
        }

        internal static Rectangle GetAbsoluteClientRect(IntPtr hWnd)
        {
            Rectangle windowRect = NativeMethods.GetWindowRect(hWnd);
            Rectangle clientRect = NativeMethods.GetClientRect(hWnd);

            // This gives us the width of the left, right and bottom chrome - we can then determine the top height
            int chromeWidth = (int)((windowRect.Width - clientRect.Width) / 2);

            return new Rectangle(new Point(windowRect.X + chromeWidth, windowRect.Y + (windowRect.Height - clientRect.Height - chromeWidth)), clientRect.Size);
        }
    }
    #endregion
}
