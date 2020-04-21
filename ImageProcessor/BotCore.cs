using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using SharpAdbClient;
using SharpAdbClient.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Net.Sockets;

namespace BotFramework
{
    /// <summary>
    /// All the android emulator controllers are here!
    /// </summary>
    public class BotCore
    {
        private static BotCore Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new BotCore();
                }
                return instance;
            }
        }

        private static BotCore instance;

        private Random rnd = new Random();

        private object locker = new object();
        /// <summary>
        /// Check Game is foreground and return a bool
        /// </summary>
        /// <param name="packagename">Applications Name in Android, such as com.supercell.clashofclans</param>
        public static bool GameIsForeground(string packagename)
        {
            if (!ScriptRun.Run)
            {
                return false;
            }
            try
            {
                
                if(Variables.Controlled_Device == null)
                {
                    return false;
                }
                var receiver = new ConsoleOutputReceiver();
                AdbInstance.Instance.client.ExecuteRemoteCommand("dumpsys window windows | grep -E 'mCurrentFocus'", (Variables.Controlled_Device as DeviceData), receiver);
                Variables.AdvanceLog(receiver.ToString());
                if (receiver.ToString().Contains(packagename))
                {
                    return true;
                }
            }
            catch (InvalidOperationException)
            {

            }
            catch (ArgumentOutOfRangeException)
            {

            }
            return false;
        }
        /// <summary>
        /// Send Adb command to android
        /// </summary>
        public static string AdbCommand(string command,[CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            if(Variables.Controlled_Device == null)
            {
                throw new DeviceNotFoundException();
            }
            ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
            AdbInstance.Instance.client.ExecuteRemoteCommand(command, (Variables.Controlled_Device as DeviceData), receiver);
            return receiver.ToString();
        }
        /// <summary>
        /// Start game using game package name
        /// </summary>
        /// <param name="packagename"></param>
        /// <returns></returns>
        public static bool StartGame(string packagename)
        {
            if (!ScriptRun.Run)
            {
                return false;
            }
            try
            {
                if (Variables.Controlled_Device == null)
                {
                    return false;
                }
                AdbInstance.Instance.GameName = "";
                ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
                AdbInstance.Instance.client.ExecuteRemoteCommand("input keyevent KEYCODE_HOME", (Variables.Controlled_Device as DeviceData), receiver);
                Thread.Sleep(1000);
                AdbInstance.Instance.client.ExecuteRemoteCommand("am start -n " + packagename, (Variables.Controlled_Device as DeviceData), receiver);
                Thread.Sleep(1000);
                return GameIsForeground(packagename);
            }
            catch (InvalidOperationException)
            {

            }
            catch (ArgumentOutOfRangeException)
            {

            }
            return false;
        }
        /// <summary>
        /// Close the game with package name
        /// </summary>
        /// <param name="packagename"></param>
        public static void KillGame(string packagename)
        {
            if (!ScriptRun.Run)
            {
                return;
            }
            try
            {
                if (Variables.Controlled_Device == null)
                {
                    return;
                }
                ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
                AdbInstance.Instance.client.ExecuteRemoteCommand("input keyevent KEYCODE_HOME", (Variables.Controlled_Device as DeviceData), receiver);
                Thread.Sleep(1000);
                AdbInstance.Instance.client.ExecuteRemoteCommand("am force-stop " + packagename, (Variables.Controlled_Device as DeviceData), receiver);
            }
            catch (InvalidOperationException)
            {

            }
            catch (ArgumentOutOfRangeException)
            {

            }
        }
        
        /// <summary>
        /// Tap at location
        /// </summary>
        /// <param name="point">The location</param>
        public static void SendTap(Point point)
        {
            if (!ScriptRun.Run)
            {
                return;
            }
            SendTap(point.X, point.Y);
        }
        /// <summary>
        /// Swipe the screen
        /// </summary>
        /// <param name="start">Swiping start position</param>
        /// <param name="end">Swiping end position</param>
        /// <param name="usedTime">The time used for swiping, milliseconds</param>
        public static void SendSwipe(Point start, Point end, int usedTime, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            if (!ScriptRun.Run)
            {
                return;
            }
            
            int x = start.X;
            int y = start.Y;
            int ex = end.X;
            int ey = end.Y;
            if (Variables.Controlled_Device == null)
            {
                return;
            }
            ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
            AdbInstance.Instance.client.ExecuteRemoteCommand("input touchscreen swipe " + (x * Variables.ClickPointMultiply).ToString("0") + " " + (y * Variables.ClickPointMultiply).ToString("0") + " " + (ex * Variables.ClickPointMultiply).ToString("0") + " " + (ey * Variables.ClickPointMultiply).ToString("0") + " " + usedTime, (Variables.Controlled_Device as DeviceData), receiver);
            if (receiver.ToString().Contains("Error"))
            {
                Variables.AdvanceLog(receiver.ToString(), lineNumber, caller);
            }
        }
        /// <summary>
        /// Left click adb command on the point for generating background click in emulators
        /// </summary>
        /// <param name="x">X Posiition for clicking</param>
        /// <param name="y">Y Position for clicking</param>
        public static void SendTap(int x, int y)
        {
            if (!ScriptRun.Run)
            {
                return;
            }
            if (Variables.Controlled_Device == null)
            {
                return;
            }
            int pressure = Instance.rnd.Next(300, 500);
            string cmd = $"d 0 {(x * Variables.ClickPointMultiply).ToString("0")} {(y * Variables.ClickPointMultiply).ToString("0")} {(pressure * Variables.ClickPointMultiply).ToString("0")}\nc\nu 0\nc\n";
            Minitouch(cmd);
        }
        /// <summary>
        /// Send minitouch command to device
        /// </summary>
        /// <param name="command">Minitouch command such as d, c, u, m</param>
        public static void Minitouch(string command)
        {
            if (!ScriptRun.Run)
            {
                return;
            }
            try
            {
                Stopwatch s = Stopwatch.StartNew();
                if(AdbInstance.Instance.minitouchSocket == null)
                {
                    EmulatorLoader.ConnectMinitouch();
                }
                byte[] bytes = AdbClient.Encoding.GetBytes(command);
                AdbInstance.Instance.minitouchSocket.Send(bytes, 0, bytes.Length, SocketFlags.None);
                s.Stop();
                Variables.AdvanceLog("Minitouch command "+ command.Replace("\n","\\n") + " sended. Used time: " + s.ElapsedMilliseconds + "ms");
            }
            catch(SocketException)
            {
                AdbInstance.Instance.minitouchPort++;
                EmulatorLoader.ConnectMinitouch();
                Minitouch(command);
            }
        }
        /// <summary>
        /// Swipe the screen
        /// </summary>
        /// <param name="startX">Swiping start position</param>
        /// <param name="startY">Swiping start postion</param>
        /// <param name="endX">Swiping end position</param>
        /// <param name="endY">Swiping end position</param>
        /// <param name="usedTime">The time used for swiping, milliseconds</param>
        public static void SendSwipe(int startX, int startY, int endX, int endY, int usedTime, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            if (!ScriptRun.Run)
            {
                return;
            }

            if (Variables.Controlled_Device == null)
            {
                return;
            }
            SendSwipe(new Point(startX, startY), new Point(endX, endY), usedTime, lineNumber, caller);
        }
        /// <summary>
        /// Emulator supported by this dll
        /// </summary>
        public static bool Is64BitOperatingSystem()
        {
            // Check if this process is natively an x64 process. If it is, it will only run on x64 environments, thus, the environment must be x64.
            if (IntPtr.Size == 8)
                return true;
            // Check if this process is an x86 process running on an x64 environment.
            IntPtr moduleHandle = DllImport.GetModuleHandle("kernel32");
            if (moduleHandle != IntPtr.Zero)
            {
                IntPtr processAddress = DllImport.GetProcAddress(moduleHandle, "IsWow64Process");
                if (processAddress != IntPtr.Zero)
                {
                    if (DllImport.IsWow64Process(DllImport.GetCurrentProcess(), out bool result) && result)
                        return true;
                }
            }
            // The environment must be an x86 environment.
            return false;
        }
        /// <summary>
        /// Get color of location in screenshots
        /// </summary>
        /// <param name="position">The position of image</param>
        /// <returns>color</returns>
        public static Color GetPixel(Point position, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            lock (Instance.locker)
            {
                if (!ScriptRun.Run)
                {
                    return Color.Black;
                }
                byte[] image = null;
                int error = 0;
                do
                {
                    if (Variables.ProchWnd != IntPtr.Zero)
                    {
                        image = Screenshot.ImageCapture(Variables.ProchWnd, Variables.WinApiCaptCropStart, Variables.WinApiCaptCropEnd, lineNumber, caller);
                    }
                    else
                    {
                        image = Screenshot.ImageCapture(Variables.Proc.MainWindowHandle, Variables.WinApiCaptCropStart, Variables.WinApiCaptCropEnd, lineNumber, caller);
                    }
                    error++;
                }
                while (image == null && error < 5);
                return Screenshot.Decompress(image).GetPixel(position.X, position.Y);
            }
        }

        private static Color GetPixel(int x, int y, int step, int Width, int Depth, byte[] pixel)
        {
            lock (Instance.locker)
            {
                if (!ScriptRun.Run)
                {
                    return Color.Black;
                }
                Color clr = Color.Empty;
                int i = ((y * Width + x) * step);
                if (i > pixel.Length)
                {
                    Variables.AdvanceLog("index of pixel array out of range at GetPixel");
                    return clr;
                }
                if (Depth == 32 || Depth == 24)
                {
                    byte b = pixel[i];
                    byte g = pixel[i + 1];
                    byte r = pixel[i + 2];
                    clr = Color.FromArgb(r, g, b);
                }
                else if (Depth == 8)
                {
                    byte b = pixel[i];
                    clr = Color.FromArgb(b, b, b);
                }
                return clr;
            }
        }
        /// <summary>
        /// Compare point RGB from image
        /// </summary>
        /// <param name="point">The point to check for color</param>
        /// <param name="color">The color to check at point is true or false</param>
        /// <param name="tolerance">The tolerance on color, larger will more inaccurate</param>
        /// <param name="image">The image to check color. If not set will auto screenshot using WinAPI</param>
        /// <returns>bool</returns>
        public static bool RGBComparer(Point point, Color color, int tolerance, byte[] image = null, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            if (!ScriptRun.Run)
            {
                return false;
            }
            int red = color.R;
            int blue = color.B;
            int green = color.G;
            int error = 0;
            while (image == null && error < 5)
            {
                if (Variables.ProchWnd != IntPtr.Zero)
                {
                    image = Screenshot.ImageCapture(Variables.ProchWnd, Variables.WinApiCaptCropStart, Variables.WinApiCaptCropEnd, lineNumber, caller);
                }
                else
                {
                    image = Screenshot.ImageCapture(Variables.Proc.MainWindowHandle, Variables.WinApiCaptCropStart, Variables.WinApiCaptCropEnd, lineNumber, caller);
                }
                error++;
            }
            if (image == null)
            {
                return false;
            }
            Bitmap bmp = Screenshot.Decompress(image);
            int Width = bmp.Width;
            int Height = bmp.Height;
            int PixelCount = Width * Height;
            Rectangle rect = new Rectangle(0, 0, Width, Height);
            int Depth = Bitmap.GetPixelFormatSize(bmp.PixelFormat);
            if (Depth != 8 && Depth != 24 && Depth != 32)
            {
                Variables.AdvanceLog("Image bit per pixel format not supported", lineNumber, caller);
                return false;
            }
            BitmapData bd = bmp.LockBits(rect, ImageLockMode.ReadOnly, bmp.PixelFormat);
            int step = Depth / 8;
            try
            {
                byte[] pixel = new byte[PixelCount * step];
                IntPtr ptr = bd.Scan0;
                Marshal.Copy(ptr, pixel, 0, pixel.Length);
                Color clr = GetPixel(point.X, point.Y, step, Width, Depth, pixel);
                if (clr.R >= (red - tolerance) && clr.R <= (red + tolerance))
                {
                    if (clr.G >= (green - tolerance) && clr.G <= (green + tolerance))
                    {
                        if (clr.B >= (blue - tolerance) && clr.B <= (blue + tolerance))
                        {
                            bmp.UnlockBits(bd);
                            return true;
                        }
                    }
                }
                Variables.AdvanceLog("The point " + point.X + ", " + point.Y + " color is " + clr.R + ", " + clr.G + ", " + clr.B, lineNumber, caller);
            }
            catch
            {

            }
            bmp.UnlockBits(bd);
            return false;
        }

        /// <summary>
        /// Compare point RGB from image
        /// </summary>
        /// <param name="point">The point to check for color</param>
        /// <param name="tolerance">Tolerance to the color RGB, example: red=120, Tolerance=20 Result=100~140 red will return true</param>
        /// <param name="blue">Blue value of color</param>
        /// <param name="green">Green value of color</param>
        /// <param name="red">Red value of color</param>
        /// <param name="image">The image to check color. If not set will auto screenshot using WinAPI</param>
        /// <returns>bool</returns>
        public static bool RGBComparer(Point point, int red, int green, int blue, int tolerance, byte[] image = null, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            if (!ScriptRun.Run)
            {
                return false;
            }
            return RGBComparer(point, Color.FromArgb(red, green, blue), tolerance, image);
        }
        /// <summary>
        /// Compare point RGB from image
        /// </summary>
        /// <returns>bool</returns>
        public static bool RGBComparer(byte[] image, Color color, int tolerance)
        {
            lock (Instance.locker)
            {
                if (!ScriptRun.Run)
                {
                    return false;
                }
                int error = 0;
                while (image == null && error < 5)
                {
                    if (Variables.ProchWnd != IntPtr.Zero)
                    {
                        image = Screenshot.ImageCapture(Variables.ProchWnd, Variables.WinApiCaptCropStart, Variables.WinApiCaptCropEnd);
                    }
                    else
                    {
                        image = Screenshot.ImageCapture(Variables.Proc.MainWindowHandle, Variables.WinApiCaptCropStart, Variables.WinApiCaptCropEnd);
                    }
                    error++;
                }

                if (image == null)
                {
                    return false;
                }
                Bitmap bmp = Screenshot.Decompress(image);
                int Width = bmp.Width;
                int Height = bmp.Height;
                int PixelCount = Width * Height;
                Rectangle rect = new Rectangle(0, 0, Width, Height);
                int Depth = Bitmap.GetPixelFormatSize(bmp.PixelFormat);
                if (Depth != 8 && Depth != 24 && Depth != 32)
                {
                    Variables.AdvanceLog("Image bit per pixel format not supported");
                    return false;
                }
                BitmapData bd = bmp.LockBits(rect, ImageLockMode.ReadOnly, bmp.PixelFormat);
                int step = Depth / 8;
                byte[] pixel = new byte[PixelCount * step];
                IntPtr ptr = bd.Scan0;
                Marshal.Copy(ptr, pixel, 0, pixel.Length);
                for (int i = 0; i < bmp.Height; i++)
                {
                    for (int j = 0; j < bmp.Width; j++)
                    {
                        //Get the color at each pixel
                        Color clr = GetPixel(j, i, step, Width, Depth, pixel);
                        if (clr.R >= (color.R - tolerance) && clr.R <= (color.R + tolerance))
                        {
                            if (clr.G >= (color.G - tolerance) && clr.G <= (color.G + tolerance))
                            {
                                if (clr.B >= (color.B - tolerance) && clr.B <= (color.B + tolerance))
                                {
                                    bmp.UnlockBits(bd);
                                    return true;
                                }
                            }
                        }
                    }
                }
                bmp.UnlockBits(bd);
                return false;
            }
        }
        /// <summary>
        /// Compare point RGB from image
        /// </summary>
        /// <returns>bool</returns>
        public static bool RGBComparer(Color color, Point start, Point end, int tolerance, out Point? point, byte[] image = null, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            lock (Instance.locker)
            {
                if (!ScriptRun.Run)
                {
                    point = null;
                    return false;
                }
                int error = 0;
                while (image == null && error < 5)
                {
                    if (Variables.ProchWnd != IntPtr.Zero)
                    {
                        image = Screenshot.ImageCapture(Variables.ProchWnd, Variables.WinApiCaptCropStart, Variables.WinApiCaptCropEnd, lineNumber, caller);
                    }
                    else
                    {
                        image = Screenshot.ImageCapture(Variables.Proc.MainWindowHandle, Variables.WinApiCaptCropStart, Variables.WinApiCaptCropEnd, lineNumber, caller);
                    }
                    error++;
                }
                if (image == null)
                {
                    point = null;
                    return false;
                }
                var crop = Screenshot.CropImage(image, start, end);
                var result = RGBComparer(color, tolerance, out point, crop, lineNumber, caller);
                return result;
            }
        }

        /// <summary>
        /// Compare point RGB from image
        /// </summary>
        /// <returns>bool</returns>
        public static bool RGBComparer(Color color, int tolerance, out Point? point, byte[] image = null,[CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            lock (Instance.locker)
            {
                if (!ScriptRun.Run)
                {
                    point = null;
                    return false;
                }
                int error = 0;
                while (image == null && error < 5)
                {
                    if (Variables.ProchWnd != IntPtr.Zero)
                    {
                        image = Screenshot.ImageCapture(Variables.ProchWnd, Variables.WinApiCaptCropStart, Variables.WinApiCaptCropEnd, lineNumber, caller);
                    }
                    else
                    {
                        image = Screenshot.ImageCapture(Variables.Proc.MainWindowHandle, Variables.WinApiCaptCropStart, Variables.WinApiCaptCropEnd, lineNumber, caller);
                    }
                    error++;
                }
                if (image == null)
                {
                    point = null;
                    return false;
                }
                var bmp = Screenshot.Decompress(image);
                int Width = bmp.Width;
                int Height = bmp.Height;
                int PixelCount = Width * Height;
                Rectangle rect = new Rectangle(0, 0, Width, Height);
                int Depth = Bitmap.GetPixelFormatSize(bmp.PixelFormat);
                if (Depth != 8 && Depth != 24 && Depth != 32)
                {
                    Variables.AdvanceLog("Image bit per pixel format not supported");
                    point = null;
                    return false;
                }
                BitmapData bd = bmp.LockBits(rect, ImageLockMode.ReadOnly, bmp.PixelFormat);
                int step = Depth / 8;
                byte[] pixel = new byte[PixelCount * step];
                IntPtr ptr = bd.Scan0;
                Marshal.Copy(ptr, pixel, 0, pixel.Length);
                for (int i = 0; i < bmp.Height; i++)
                {
                    for (int j = 0; j < bmp.Width; j++)
                    {
                        //Get the color at each pixel
                        Color clr = GetPixel(j, i, step, Width, Depth, pixel);
                        if (clr.R >= (color.R - tolerance) && clr.R <= (color.R + tolerance))
                        {
                            if (clr.G >= (color.G - tolerance) && clr.G <= (color.G + tolerance))
                            {
                                if (clr.B >= (color.B - tolerance) && clr.B <= (color.B + tolerance))
                                {
                                    bmp.UnlockBits(bd);
                                    point = new Point(j, i);
                                    return true;
                                }
                            }
                        }
                    }
                }
                bmp.UnlockBits(bd);
                point = null;
                return false;
            }
        }
        /// <summary>
        /// Return a Point location of the image in screencapture <see cref="Screenshot.ImageCapture(int, string)"/> (will return null if not found)
        /// </summary>
        /// <param name="find">The smaller image for matching</param>
        /// <param name="image">Result from <see cref="Screenshot.ImageCapture(int, string)"/></param>
        /// <param name="GrayStyle">Convert the images to gray for faster detection</param>
        /// <param name="lineNumber"></param>
        /// <param name="caller"></param>
        /// <param name="FindOnlyOne"></param>
        /// <returns>Point or null</returns>
        /// <returns></returns>
        public static Point[] FindImages(byte[] image, Bitmap[] find, bool GrayStyle, bool FindOnlyOne = false, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            lock (Instance.locker)
            {
                int error = 0;
                while (image == null && error < 5)
                {
                    if (Variables.ProchWnd != IntPtr.Zero)
                    {
                        image = Screenshot.ImageCapture(Variables.ProchWnd, Variables.WinApiCaptCropStart, Variables.WinApiCaptCropEnd, lineNumber, caller);
                    }
                    else
                    {
                        image = Screenshot.ImageCapture(Variables.Proc.MainWindowHandle, Variables.WinApiCaptCropStart, Variables.WinApiCaptCropEnd, lineNumber, caller);
                    }
                    error++;
                }
                if (image == null)
                {
                    return null;
                }
            }
            Bitmap original = Screenshot.Decompress(image);
            return FindImages(original, find, GrayStyle, FindOnlyOne, lineNumber, caller);
        }

        /// <summary>
        /// Return a Point location of the image in screencapture <see cref="Screenshot.ImageCapture(int, string)"/> (will return null if not found)
        /// </summary>
        /// <param name="find">The smaller image for matching</param>
        /// <param name="original">Result from <see cref="Screenshot.ImageCapture(int, string)"/></param>
        /// <param name="GrayStyle">Convert the images to gray for faster detection</param>
        /// <param name="FindOnlyOne"></param>
        /// <param name="lineNumber"></param>
        /// <param name="caller"></param>
        /// <returns>Point or null</returns>
        /// <returns></returns>
        public static Point[] FindImages(Bitmap original, Bitmap[] find, bool GrayStyle, bool FindOnlyOne = false, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            lock (Instance.locker)
            {
                if (!ScriptRun.Run)
                {
                    return null;
                }
                Stopwatch s = Stopwatch.StartNew();
                List<Point> matched = new List<Point>();
                try
                {
                    if (GrayStyle)
                    {
                        Image<Gray, byte> source = new Image<Gray, byte>(original);
                        foreach (var image in find)
                        {
                            Image<Gray, byte> template = new Image<Gray, byte>(image);
                            using (Image<Gray, float> result = source.MatchTemplate(template, TemplateMatchingType.CcoeffNormed))
                            {
                                result.MinMax(out double[] minValues, out double[] maxValues, out Point[] minLocations, out Point[] maxLocations);
                                for (int x = 0; x < maxValues.Length; x++)
                                {
                                    if (maxValues[x] > 0.9)
                                    {
                                        source.FillConvexPoly(new Point[] { new Point(maxLocations[x].X - 2, maxLocations[x].Y - 2), new Point(maxLocations[x].X + 2, maxLocations[x].Y + 2) }, new Gray());
                                        matched.Add(maxLocations[x]);
                                        if (FindOnlyOne)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Image<Bgr, byte> source = new Image<Bgr, byte>(original);
                        foreach (var image in find)
                        {
                            Image<Bgr, byte> template = new Image<Bgr, byte>(image);
                            using (Image<Gray, float> result = source.MatchTemplate(template, TemplateMatchingType.CcoeffNormed))
                            {
                                result.MinMax(out double[] minValues, out double[] maxValues, out Point[] minLocations, out Point[] maxLocations);
                                for (int x = 0; x < maxValues.Length; x++)
                                {
                                    if (maxValues[x] > 0.9)
                                    {
                                        source.FillConvexPoly(new Point[] { new Point(maxLocations[x].X - 2, maxLocations[x].Y - 2), new Point(maxLocations[x].X + 2, maxLocations[x].Y + 2) }, new Bgr());
                                        matched.Add(maxLocations[x]);
                                        if (FindOnlyOne)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    Variables.AdvanceLog(ex.ToString());
                }
                s.Stop();
                Variables.AdvanceLog("Image processed. Used time: " + s.ElapsedMilliseconds + " ms", lineNumber, caller);
                if (matched.Count < 1)
                {
                    return null;
                }
                else
                {
                    return matched.ToArray();
                }
            }
        }

        /// <summary>
        /// Return a Point location of the image in screencapture <see cref="Screenshot.ImageCapture(int, string)"/> (will return null if not found)
        /// </summary>
        /// <param name="find">The smaller image for matching</param>
        /// <param name="original">Result from <see cref="Screenshot.ImageCapture(int, string)"/></param>
        /// <param name="GrayStyle">Convert the images to gray for faster detection</param>
        /// <param name="lineNumber"></param>
        /// <param name="caller"></param>
        /// <returns>Point or null</returns>
        /// <returns></returns>
        public static Point[] FindImages(byte[] original, List<byte[]> find, bool GrayStyle,bool FindOnlyOne = false, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            lock (Instance.locker)
            {
                if (!ScriptRun.Run)
                {
                    return null;
                }
                Stopwatch s = Stopwatch.StartNew();
                List<Point> matched = new List<Point>();
                try
                {
                    if (GrayStyle)
                    {
                        Image<Gray, byte> source = new Image<Gray, byte>(Screenshot.Decompress(original));
                        foreach (var image in find)
                        {
                            Image<Gray, byte> template = new Image<Gray, byte>(Screenshot.Decompress(image));
                            using (Image<Gray, float> result = source.MatchTemplate(template, TemplateMatchingType.CcoeffNormed))
                            {
                                result.MinMax(out double[] minValues, out double[] maxValues, out Point[] minLocations, out Point[] maxLocations);
                                for (int x = 0; x < maxValues.Length; x++)
                                {
                                    if (maxValues[x] > 0.9)
                                    {
                                        source.FillConvexPoly(new Point[] { new Point(maxLocations[x].X - 2, maxLocations[x].Y - 2), new Point(maxLocations[x].X + 2, maxLocations[x].Y + 2) }, new Gray());
                                        matched.Add(maxLocations[x]);
                                        if (FindOnlyOne)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Image<Bgr, byte> source = new Image<Bgr, byte>(Screenshot.Decompress(original));
                        foreach (var image in find)
                        {
                            Image<Bgr, byte> template = new Image<Bgr, byte>(Screenshot.Decompress(image));
                            using (Image<Gray, float> result = source.MatchTemplate(template, TemplateMatchingType.CcoeffNormed))
                            {
                                result.MinMax(out double[] minValues, out double[] maxValues, out Point[] minLocations, out Point[] maxLocations);
                                for (int x = 0; x < maxValues.Length; x++)
                                {
                                    if (maxValues[x] > 0.9)
                                    {
                                        source.FillConvexPoly(new Point[] { new Point(maxLocations[x].X - 2, maxLocations[x].Y - 2), new Point(maxLocations[x].X + 2, maxLocations[x].Y + 2) }, new Bgr());
                                        matched.Add(maxLocations[x]);
                                        if (FindOnlyOne)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    Variables.AdvanceLog(ex.ToString());
                }
                s.Stop();
                Variables.AdvanceLog("Image processed. Used time: " + s.ElapsedMilliseconds + " ms", lineNumber, caller);
                if (matched.Count < 1)
                {
                    return null;
                }
                else
                {
                    return matched.ToArray();
                }
            }
        }
        /// <summary>
        /// Return a Point location of the image in screencapture <see cref="Screenshot.ImageCapture(int, string)"/> (will return null if not found)
        /// </summary>
        /// <param name="find">The smaller image for matching</param>
        /// <param name="screencapture">Original image that need to get the point on it</param>
        /// <param name="GrayStyle">Convert the images to gray for faster detection</param>
        /// <returns>Point or null</returns>
        public static Point? FindImage(byte[] screencapture, Bitmap find, bool GrayStyle, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            if (!ScriptRun.Run)
            {
                return null;
            }
            if (screencapture == null)
            {
                Variables.AdvanceLog("Result return null because of null original image", lineNumber, caller);
                return null;
            }
            try
            {
                return FindImage(Screenshot.Decompress(screencapture), find, GrayStyle, lineNumber, caller);
            }
            catch (Exception ex)
            {
                Variables.AdvanceLog(ex.ToString(), lineNumber, caller);
                return null;
            }
        }
        /// <summary>
        /// Return a Point location of the image in screencapture <see cref="Screenshot.ImageCapture(int, string)"/> (will return null if not found)
        /// </summary>
        /// <param name="find">The smaller image for matching</param>
        /// <param name="original">Result from <see cref="Screenshot.ImageCapture(int, string)"/></param>
        /// <param name="GrayStyle">Convert the images to gray for faster detection</param>
        /// <param name="lineNumber"></param>
        /// <param name="caller"></param>
        /// <returns>Point or null</returns>
        /// <returns></returns>
        public static Point? FindImage(Bitmap original, Bitmap find, bool GrayStyle, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            if (!ScriptRun.Run)
            {
                return null;
            }
            lock (Instance.locker)
            {
                Stopwatch s = Stopwatch.StartNew();
                try
                {
                    if (GrayStyle)
                    {

                        Image<Gray, byte> source = new Image<Gray, byte>(original);
                        Image<Gray, byte> template = new Image<Gray, byte>(find);
                        using (Image<Gray, float> result = source.MatchTemplate(template, TemplateMatchingType.CcoeffNormed))
                        {
                            result.MinMax(out double[] minValues, out double[] maxValues, out Point[] minLocations, out Point[] maxLocations);

                            // You can try different values of the threshold. I guess somewhere between 0.75 and 0.95 would be good.
                            if (maxValues[0] > 0.9)
                            {
                                s.Stop();
                                Variables.AdvanceLog("Image matched. Used time: " + s.ElapsedMilliseconds + " ms", lineNumber, caller);
                                return maxLocations[0];
                            }
                        }
                    }
                    else
                    {
                        Image<Bgr, byte> source = new Image<Bgr, byte>(original);
                        Image<Bgr, byte> template = new Image<Bgr, byte>(find);
                        using (Image<Gray, float> result = source.MatchTemplate(template, TemplateMatchingType.CcoeffNormed))
                        {
                            result.MinMax(out double[] minValues, out double[] maxValues, out Point[] minLocations, out Point[] maxLocations);

                            // You can try different values of the threshold. I guess somewhere between 0.75 and 0.95 would be good.
                            if (maxValues[0] > 0.9)
                            {
                                s.Stop();
                                Variables.AdvanceLog("Image matched. Used time: " + s.ElapsedMilliseconds + " ms", lineNumber, caller);
                                return maxLocations[0];
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Variables.AdvanceLog(ex.ToString());
                }
                s.Stop();
                Variables.AdvanceLog("Image not matched. Used time: " + s.ElapsedMilliseconds + " ms", lineNumber, caller);
                return null;
            }            
        }
        /// <summary>
        /// Return a Point location of the image in screencapture <see cref="Screenshot.ImageCapture(int, string)"/> (will return null if not found)
        /// </summary>
        /// <param name="screencapture">Result from <see cref="Screenshot.ImageCapture(int, string)"/></param>
        /// <param name="findPath">Can be set with params:<code>imageName_startX_startY_endX_endY.png</code></param>
        /// <param name="GrayStyle">Convert the images to gray for faster detection</param>
        /// <param name="lineNumber"></param>
        /// <param name="caller"></param>
        public static Point? FindImage(byte[] screencapture, string findPath, bool GrayStyle, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            if (!ScriptRun.Run)
            {
                return null;
            }
            lock (Instance.locker)
            {
                Stopwatch s = Stopwatch.StartNew();
                if (screencapture == null)
                {
                    Variables.AdvanceLog("Result return null because of null original image", lineNumber, caller);
                    return null;
                }
                Bitmap original;
                var split = findPath.Split('\\').Last().Split('_');
                if (split.Length == 5)
                {
                    try
                    {
                        //Have params
                        Point start = new Point(Convert.ToInt32(new string(split[1].Where(char.IsDigit).ToArray())), Convert.ToInt32(new string(split[2].Where(char.IsDigit).ToArray())));
                        Point end = new Point(Convert.ToInt32(new string(split[3].Where(char.IsDigit).ToArray())), Convert.ToInt32(new string(split[4].Where(char.IsDigit).ToArray())));
                        original = Screenshot.Decompress(Screenshot.CropImage(screencapture, start, end));
                    }
                    catch (Exception ex)
                    {
                        //The file parse failed! We need inform user!
                        Variables.ScriptLog("Invalid file name used error as: " + ex.ToString(), Color.Red);
                        return null;
                    }
                }
                else
                    original = Screenshot.Decompress(screencapture);
                if (!File.Exists(findPath))
                {
                    Variables.AdvanceLog("Unable to find image " + findPath.Split('\\').Last() + ", image path not valid", lineNumber, caller);
                    return null;
                }
                try
                {
                    if (GrayStyle)
                    {
                        Image<Gray, byte> source = new Image<Gray, byte>(original);
                        Image<Gray, byte> template = new Image<Gray, byte>(findPath);
                        using (Image<Gray, float> result = source.MatchTemplate(template, TemplateMatchingType.CcoeffNormed))
                        {
                            result.MinMax(out double[] minValues, out double[] maxValues, out Point[] minLocations, out Point[] maxLocations);
                            // You can try different values of the threshold. I guess somewhere between 0.75 and 0.95 would be good.
                            if (maxValues[0] > 0.9)
                            {
                                s.Stop();
                                Variables.AdvanceLog("Image matched. Used time: " + s.ElapsedMilliseconds + " ms", lineNumber, caller);
                                return maxLocations[0];
                            }
                        }
                    }
                    else
                    {
                        Image<Bgr, byte> source = new Image<Bgr, byte>(original);
                        Image<Bgr, byte> template = new Image<Bgr, byte>(findPath);
                        using (Image<Gray, float> result = source.MatchTemplate(template, TemplateMatchingType.CcoeffNormed))
                        {
                            result.MinMax(out double[] minValues, out double[] maxValues, out Point[] minLocations, out Point[] maxLocations);
                            // You can try different values of the threshold. I guess somewhere between 0.75 and 0.95 would be good.
                            if (maxValues[0] > 0.9)
                            {
                                s.Stop();
                                Variables.AdvanceLog("Image matched. Used time: " + s.ElapsedMilliseconds + " ms", lineNumber, caller);
                                return maxLocations[0];
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Variables.AdvanceLog(ex.ToString());
                }
                s.Stop();
                Variables.AdvanceLog("Image not matched. Used time: " + s.ElapsedMilliseconds + " ms", lineNumber, caller);
                return null;
            }
        }
        /// <summary>
        /// Return a Point location of the image in screencapture <see cref="Screenshot.ImageCapture(int, string)"/> (will return null if not found)
        /// </summary>
        /// <param name="image">The smaller image for matching</param>
        /// <param name="screencapture">Result from <see cref="Screenshot.ImageCapture(int, string)"/></param>
        /// <param name="GrayStyle">Convert the images to gray for faster detection</param>
        /// <returns>Point or null</returns>
        public static Point? FindImage(byte[] screencapture, byte[] image, bool GrayStyle, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            if (!ScriptRun.Run)
            {
                return null;
            }
            if (screencapture == null)
            {
                Variables.AdvanceLog("Result return null because of null original image", lineNumber, caller);
                return null;
            }
            return FindImage(Screenshot.Decompress(screencapture), Screenshot.Decompress(image), GrayStyle);
        }

        /// <summary>
        /// Force emulator keep potrait
        /// </summary>
        public static void StayPotrait()
        {
            
            if (Variables.Controlled_Device == null)
            {
                return;
            }
            if ((Variables.Controlled_Device as DeviceData).State == DeviceState.Online)
            {
                ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
                AdbInstance.Instance.client.ExecuteRemoteCommand("content insert --uri content://settings/system --bind name:s:accelerometer_rotation --bind value:i:0", (Variables.Controlled_Device as DeviceData), receiver);
                AdbInstance.Instance.client.ExecuteRemoteCommand("content insert --uri content://settings/system --bind name:s:user_rotation --bind value:i:0", (Variables.Controlled_Device as DeviceData), receiver);
            }
        }
       
        /// <summary>
        /// Pull file from emulator to PC
        /// </summary>
        /// <param name="from">path of file in android</param>
        /// <param name="to">path of file on PC</param>
        /// <returns></returns>
        public static bool Pull(string from, string to)
        {
            if (!ScriptRun.Run)
            {
                return false;
            }
            if (Variables.Controlled_Device == null)
            {
                return false;
            }
            if(!AdbInstance.Instance.socket.Connected)
                AdbInstance.Instance.socket = new AdbSocket(new IPEndPoint(IPAddress.Parse("127.0.0.1"), Adb.CurrentPort));
            using (SyncService service = new SyncService(AdbInstance.Instance.socket, (Variables.Controlled_Device as DeviceData)))
            using (Stream stream = File.OpenWrite(to))
            {
                service.Pull(from, stream, null, CancellationToken.None);
            }
            if (File.Exists(to))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// Push file from PC to emulator
        /// </summary>
        /// <param name="from">path of file on PC</param>
        /// <param name="to">path of file in android</param>
        /// <param name="permission">Permission of file</param>
        public static void Push(string from, string to, int permission)
        {
            if (!ScriptRun.Run)
            {
                return;
            }
            if (!AdbInstance.Instance.socket.Connected)
                AdbInstance.Instance.socket = new AdbSocket(new IPEndPoint(IPAddress.Parse("127.0.0.1"), Adb.CurrentPort));
            using (SyncService service = new SyncService(AdbInstance.Instance.socket, (Variables.Controlled_Device as DeviceData)))
            {
                using (Stream stream = File.OpenRead(from))
                {
                    service.Push(stream, to, permission, DateTime.Now, null, CancellationToken.None);
                }
            }
        }
        /// <summary>
        /// Add delays on script with human like randomize
        /// </summary>
        /// <param name="mintime">Minimum time to delay</param>
        /// <param name="maxtime">Maximum time to delay</param>
        public static void Delay(int mintime, int maxtime)
        {
            if (!ScriptRun.Run)
            {
                return;
            }
            if (maxtime < mintime)
            {
                //swap back
                int temp = maxtime;
                maxtime = mintime;
                mintime = temp;
            }
            if(mintime < 1)
                mintime = 1;
            if (maxtime < 1)
                maxtime = 1;
            Thread.Sleep(Instance.rnd.Next(mintime, maxtime));
        }
        /// <summary>
        /// Add delays on script with human like randomize
        /// </summary>
        /// <param name="randomtime">A actual time for delay</param>
        /// <param name="accurate">Real accurate or random about 200 miliseconds?</param>
        public static void Delay(int randomtime, bool accurate)
        {
            if (!ScriptRun.Run)
            {
                return;
            }
            if (accurate)
            {
                Delay(randomtime, randomtime);
            }
            else
            {
                Delay(randomtime - 100, randomtime + 100);
            }
        }
        /// <summary>
        /// Delay for specific time
        /// </summary>
        /// <param name="time">miliseconds</param>
        public static void Delay(int time)
        {
            Delay(time, true);
        }
        /// <summary>
        /// Delay for specific time
        /// </summary>
        /// <param name="time">time</param>
        public static void Delay(TimeSpan time)
        {
            Delay(Convert.ToInt32(time.TotalMilliseconds));
        }
        /// <summary>
        /// Send text to android emulator, can be used for typing but only ENGLISH!!
        /// </summary>
        /// <param name="text">The text needed to be sent</param>
        public static void SendText(string text)
        {
            if (!ScriptRun.Run)
            {
                return;
            }
            try
            {
                ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
                AdbInstance.Instance.client.ExecuteRemoteCommand("input text \"" + text.Replace(" ", "%s") + "\"", (Variables.Controlled_Device as DeviceData), receiver);
            }
            catch (InvalidOperationException)
            {

            }
            catch (ArgumentOutOfRangeException)
            {

            }
        }
       /// <summary>
       /// Send keyevent via Adb to controlled device
       /// </summary>
       /// <param name="keycode">keycode</param>
        public static void SendEvent(int keycode)
        {
            AdbCommand("input keyevent " + keycode.ToString());
        }
        /// <summary>
        /// Send Adb command without binding with Variables.Device
        /// </summary>
        /// <param name="command"></param>
        /// <param name="device"></param>
        /// <returns></returns>
        public static string AdbCommand(string command, object device)
        {
            ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
            AdbInstance.Instance.client.ExecuteRemoteCommand(command, (device as DeviceData), receiver);
            return receiver.ToString();
        }
    }
}