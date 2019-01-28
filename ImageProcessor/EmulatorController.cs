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
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace ImageProcessor
{
    /// <summary>
    /// Functions that used to controls Emulators
    /// </summary>
    public class EmulatorController
    {
        public static IntPtr handle = IntPtr.Zero;
        private static Thread cleaningthread = null;
        private static ImageConverter _imageConverter = new ImageConverter();
        /// <summary>
        /// The path to bot.ini
        /// </summary>
        public static string profilePath = "MEmu";
        public static string path;

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        public static byte[] Compress(Image image)
        {
            try
            {
                byte[] xByte = (byte[])_imageConverter.ConvertTo(image, typeof(byte[]));
                return xByte;
            }
            catch
            {
                return null;
            }

        }

        public static Image Decompress(byte[] buffer)
        {
            try
            {
                using (var ms = new MemoryStream(buffer))
                {
                    return Image.FromStream(ms);
                }
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// Install an apk to selected device from path
        /// </summary>
        public static void InstallAPK(string path)
        {
            try
            {
                var receiver = new ConsoleOutputReceiver();
                AdbClient.Instance.ExecuteRemoteCommand("adb install " + path, Variables.Controlled_Device, receiver);
            }
            catch (InvalidOperationException)
            {

            }
            catch (ArgumentOutOfRangeException)
            {

            }
        }
        /// <summary>
        /// Close the emulator by using vBox command lines
        /// </summary>
        public static void CloseEmulator(string VBoxController)
        {
            ProcessStartInfo close = new ProcessStartInfo();
            close.FileName = Variables.VBoxManagerPath + "\\" + VBoxController;
            if(Variables.Instance.Length > 0)
            {
                close.Arguments = "controlvm " + Variables.Instance + " poweroff";
            }
           else
            {
                close.Arguments = "controlvm MEmu poweroff";
            }
            close.CreateNoWindow = true;
            close.WindowStyle = ProcessWindowStyle.Hidden;
            try
            {
                if (Variables.Proc != null)
                {
                    Variables.Proc.Kill();
                }
            }
            catch
            {

            }
            Process p = Process.Start(close);
            Variables.Proc = null;
            Variables.Controlled_Device = null;
            Variables.ScriptLog.Add("Emulator Closed");
        }
        /// <summary>
        /// Refresh Variables.Proc and EmulatorController.Handle
        /// </summary>
        /// <param name="classes">for checking if the process is really emulator</param>
        /// <param name="name">for checking if the process is really emulator</param>
        public static void ConnectAndroidEmulator(string classes, string name, string[] processName)
        {
            foreach(var n in processName)
            {
                string title = n;
                if (Variables.Instance.Length > 0)
                {
                    title = Variables.Instance;
                }
                foreach (var p in Process.GetProcessesByName(processName[0]))
                {
                    Debug_.WriteLine(p.MainWindowTitle);
                    if (p.MainWindowTitle == title)
                    {
                        IntPtr handle = DllImport.FindWindowEx(p.MainWindowHandle, IntPtr.Zero, classes, name);
                        Variables.Proc = p;
                        EmulatorController.handle = p.MainWindowHandle;
                        Variables.ScriptLog.Add("Emulator ID: " + p.Id);
                        break;
                    }
                }
            }
            
        }

        private static void CleanLog()
        {
            while (true)
            {
                Thread.Sleep(1000);
                if (Variables.AdbLog.Count > 20)
                {
                    Variables.AdbLog.Clear();
                }
                if (Variables.ScriptLog.Count > 20)
                {
                    Variables.ScriptLog.Clear();
                }
            }
        }
        /// <summary>
        /// Check Game is foreground and return a bool
        /// </summary>
        /// <param name="packagename">Applications Name in Android, such as com.supercell.clashofclans</param>
        public static bool GameIsForeground(string packagename)
        {
            try
            {
                var receiver = new ConsoleOutputReceiver();
                 if(Variables.Controlled_Device == null)
                {
                    return false;
                }
                AdbClient.Instance.ExecuteRemoteCommand("dumpsys window windows | grep -E 'mCurrentFocus'", Variables.Controlled_Device, receiver);
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
        /// Start ADB server
        /// </summary>
        public static bool StartAdb()
        {
            if (cleaningthread == null)
            {
                cleaningthread = new Thread(CleanLog);
                cleaningthread.Start();
            }

            while (true)
            {
                try
                {
                    AdbServer server = new AdbServer();
                    try
                    {
                        string adbname, path;
                        if (!Variables.Configure.TryGetValue("Adb_Path", out adbname))
                        {
                            Variables.Configure.TryGetValue("Path", out path);
                            path = path.Remove(path.LastIndexOf('\\'));
                            IEnumerable<string> exe = Directory.EnumerateFiles(path, "*.exe"); // lazy file system lookup
                            foreach (var e in exe)
                            {
                                if (e.Contains("adb"))
                                {
                                    adbname = Path.GetFullPath(e);
                                    break;
                                }
                            }

                        }
                        else
                        {
                            if (!File.Exists(adbname))
                            {
                                Variables.Configure.TryGetValue("Path", out path);
                                path = path.Remove(path.LastIndexOf('\\'));
                                IEnumerable<string> exe = Directory.EnumerateFiles(path, "*.exe"); // lazy file system lookup
                                foreach (var e in exe)
                                {
                                    if (e.Contains("adb"))
                                    {
                                        adbname = e;
                                        break;
                                    }
                                }
                            }
                        }
                        string processname = adbname.Split('\\').Last();
                        if (Process.GetProcessesByName(processname).Length < 1)
                        {
                            ProcessStartInfo adb = new ProcessStartInfo();
                            adb.FileName = adbname;
                            adb.CreateNoWindow = true;
                            adb.WindowStyle = ProcessWindowStyle.Hidden;
                            adb.UseShellExecute = true;
                            Process.Start(adb);
                        }
                        var result = server.StartServer(adbname, true);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Debug_.WriteLine(ex.ToString());
                        return false;
                    }
                }
                catch
                {

                }
            }

        }

        /// <summary>
        /// Start Game by using CustomImg\Icon.png
        /// </summary>
        public static bool StartGame(Bitmap icon, byte[] img)   
        {
            try
            {
                if(Variables.Controlled_Device == null)
                {
                    return false;
                }

                
                    var receiver = new ConsoleOutputReceiver();

                    AdbClient.Instance.ExecuteRemoteCommand("input keyevent KEYCODE_HOME", Variables.Controlled_Device, receiver);
                    Thread.Sleep(1000);
                    var ico = FindImage(img, icon, true);
                    if (ico != null)
                    {
                        SendTap(ico.Value);
                        Thread.Sleep(1000);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
            }
            catch (InvalidOperationException)
            {
                return false;
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }
        }
        /// <summary>
        /// Start game using game package name
        /// </summary>
        /// <param name="packagename"></param>
        /// <returns></returns>
        public static bool StartGame(string packagename)
        {
            try
            {
                if (Variables.Controlled_Device == null)
                {
                    return false;
                }
                    var receiver = new ConsoleOutputReceiver();
                    AdbClient.Instance.ExecuteRemoteCommand("input keyevent KEYCODE_HOME", Variables.Controlled_Device, receiver);
                    Thread.Sleep(1000);
                    AdbClient.Instance.ExecuteRemoteCommand("am start -n " + packagename, Variables.Controlled_Device, receiver);
                    Thread.Sleep(1000);
                    return GameIsForeground(packagename);
            }
            catch (InvalidOperationException)
            {
                return false;
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }

        }
        /// <summary>
        /// Close the game with package name
        /// </summary>
        /// <param name="packagename"></param>
        public static void KillGame(string packagename)
        {
            try
            {
                if (Variables.Controlled_Device == null)
                {
                    return;
                }

                {
                    var receiver = new ConsoleOutputReceiver();
                    AdbClient.Instance.ExecuteRemoteCommand("input keyevent KEYCODE_HOME", Variables.Controlled_Device, receiver);
                    Thread.Sleep(1000);
                    AdbClient.Instance.ExecuteRemoteCommand("am force-stop " + packagename, Variables.Controlled_Device, receiver);
                    receiver.Flush();
                }
            }
            catch (InvalidOperationException)
            {

            }
            catch (ArgumentOutOfRangeException)
            {

            }
        }
        /// <summary>
        /// Fast Capturing screen and return the image, uses WinAPI capture if Variables.Background is false.
        /// </summary>
        public static byte[] ImageCapture()
        {
            try
            {
                if (!Directory.Exists(Variables.SharedPath))
                {
                    MessageBox.Show("Warning, unable to find shared folder! Try to match it manually!");
                    Process.Start("bot.ini");
                    Environment.Exit(0);
                }
                path = Variables.SharedPath + "\\" + SHA256(Variables.AdbIpPort) + ".raw";

                Stopwatch s = Stopwatch.StartNew();
                byte[] raw = null;
                var receiver = new ConsoleOutputReceiver();
                if(Variables.Controlled_Device == null)
                {
                    Variables.AdbLog.Add("Waiting for device");
                    return null;
                }
                AdbClient.Instance.ExecuteRemoteCommand("screencap /sdcard/Download/" + SHA256(Variables.AdbIpPort) + ".raw", Variables.Controlled_Device, receiver);
                if (!File.Exists(path))
                {
                    Variables.AdbLog.Add("Unable to read rgba file because of file not exist!");
                    return null;
                }
                raw = File.ReadAllBytes(path);
                if (raw.Length > int.MaxValue || raw.Length < 1)
                {
                    return null;
                }
                byte[] img = new byte[raw.Length - 12]; //remove header
                for (int x = 12; x < raw.Length; x += 4)
                {
                    img[x - 10] = raw[x];
                    img[x - 11] = raw[x + 1];
                    img[x - 12] = raw[x + 2];
                    img[x - 9] = raw[x + 3];
                }
                raw = null;
                using (var stream = new MemoryStream(img))
                using (var bmp = new Bitmap(1280, 720, PixelFormat.Format32bppArgb))
                {
                    BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);
                    IntPtr pNative = bmpData.Scan0;
                    Marshal.Copy(img, 0, pNative, img.Length);
                    bmp.UnlockBits(bmpData);
                    img = null;
                    s.Stop();
                    Variables.AdbLog.Add("Screenshot saved to memory stream. Used time: " + s.ElapsedMilliseconds + " ms");
                    return Compress(bmp);
                }
            }
            catch (IOException)
            {
                return null;
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
        }
        /// <summary>
        /// Left click adb command on the point for generating background click in emulators
        /// </summary>
        /// <param name="point">Posiition for clicking</param>
        public static void SendTap(Point point, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            Debug_.WriteLine("Called by Line " + lineNumber + " Caller: " + caller);
            try
            {
                var receiver = new ConsoleOutputReceiver();
                int x = point.X;
                int y = point.Y;
                if (Variables.Controlled_Device == null)
                {
                    return;
                }

                {
                    AdbClient.Instance.ExecuteRemoteCommand("input tap " + x + " " + y, Variables.Controlled_Device, receiver);
                    receiver.Flush();
                }
                if (receiver.ToString().Contains("Error"))
                {
                    Variables.AdbLog.Add(receiver.ToString());
                }
            }
            catch (InvalidOperationException)
            {

            }
            catch (ArgumentOutOfRangeException)
            {

            }
        }
        /// <summary>
        /// Swipe the screen
        /// </summary>
        /// <param name="start">Swiping start position</param>
        /// <param name="end">Swiping end position</param>
        /// <param name="usedTime">The time used for swiping, milliseconds</param>
        public static void SendSwipe(Point start, Point end, int usedTime, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            Debug_.WriteLine("Called by Line " + lineNumber + " Caller: " + caller);
            try
            {
                var receiver = new ConsoleOutputReceiver();
                int x = start.X;
                int y = start.Y;
                int ex = end.X;
                int ey = end.Y;
                if (Variables.Controlled_Device == null)
                {
                    return;
                }

                {
                    AdbClient.Instance.ExecuteRemoteCommand("input touchscreen swipe " + x + " " + y + " " + ex + " " + ey + " " + usedTime, Variables.Controlled_Device, receiver);
                    receiver.Flush();
                }
                if (receiver.ToString().Contains("Error"))
                {
                    Variables.AdbLog.Add(receiver.ToString());
                }
            }
            catch (InvalidOperationException)
            {

            }
            catch (ArgumentOutOfRangeException)
            {

            }
        }
        /// <summary>
        /// Left click adb command on the point for generating background click in emulators
        /// </summary>
        /// <param name="point">Posiition for clicking</param>
        public static void SendTap(int x, int y, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            Debug_.WriteLine("Called by Line " + lineNumber + " Caller: " + caller);
            try
            {
                var receiver = new ConsoleOutputReceiver();
                if (Variables.Controlled_Device == null)
                {
                    return;
                }

                {
                    AdbClient.Instance.ExecuteRemoteCommand("input tap " + x + " " + y, Variables.Controlled_Device, receiver);
                    receiver.Flush();
                }
                if (receiver.ToString().Contains("Error"))
                {
                    Variables.AdbLog.Add(receiver.ToString());
                }
            }
            catch (InvalidOperationException)
            {

            }
            catch (ArgumentOutOfRangeException)
            {

            }

        }
        /// <summary>
        /// Swipe the screen
        /// </summary>
        /// <param name="start">Swiping start position</param>
        /// <param name="end">Swiping end position</param>
        /// <param name="usedTime">The time used for swiping, milliseconds</param>
        public static void SendSwipe(int startX, int startY, int endX, int endY, int usedTime, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            Debug_.WriteLine("Called by Line " + lineNumber + " Caller: " + caller);
            try
            {
                if (Variables.Controlled_Device == null)
                {
                    return;
                }
                var receiver = new ConsoleOutputReceiver();

                {
                    AdbClient.Instance.ExecuteRemoteCommand("input touchscreen swipe " + startX + " " + startY + " " + endX + " " + endY + " " + usedTime, Variables.Controlled_Device, receiver);
                }
                if (receiver.ToString().Contains("Error"))
                {
                    Variables.AdbLog.Add(receiver.ToString());
                }
            }
            catch (InvalidOperationException)
            {

            }
            catch (ArgumentOutOfRangeException)
            {

            }
        }
        /// <summary>
        /// Read Configure in Bot.ini and save it into Variables.Configure (Dictionary)
        /// </summary>
        public static void ReadConfig([CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            Debug_.WriteLine("Called by Line " + lineNumber + " Caller: " + caller);
            if (!Directory.Exists("Profiles\\" + EmulatorController.profilePath))
            {
                Directory.CreateDirectory("Profiles\\" + EmulatorController.profilePath);
            }
            if (!File.Exists("Profiles\\" + EmulatorController.profilePath + "\\bot.ini"))
            {
                File.WriteAllText("Profiles\\" + EmulatorController.profilePath + "\\bot.ini", "Emulator=MEmu\nPath=MEmu.exe\nBackground=true");
            }
            var lines = File.ReadAllLines("Profiles\\" + EmulatorController.profilePath + "\\bot.ini");
            foreach (var l in lines)
            {
                string[] temp = l.Split('=');
                if (temp.Length == 2)
                {
                    if (!Variables.Configure.ContainsKey(temp[0]))
                    {
                        Variables.Configure.Add(temp[0], temp[1]);
                    }
                }
            }

        }
        /// <summary>
        /// Start Emulator accoring to Variables.Configure (Dictionary) Key "Emulator" and "Path"
        /// </summary>
        /// <param name="handleName">Check the emulator's main handle is started</param>
        public static void StartEmulator([CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            Debug_.WriteLine("Called by Line " + lineNumber + " Caller: " + caller);
            string temp = "";
            if (Variables.Configure.TryGetValue("Path", out temp))
            {
                try
                {
                    if (!File.Exists(temp))
                    {
                        MessageBox.Show("Unable to locate path of emulator!");
                        Process.Start("Profiles\\" + profilePath + "\\bot.ini");
                    }
                    ProcessStartInfo info = new ProcessStartInfo();
                    info.FileName = temp.Replace("MEmu.exe", "MEmuConsole.exe");
                    if (Variables.Instance.Length > 0)
                    {
                        info.Arguments = Variables.Instance;
                    }
                    else
                    {
                        info.Arguments = "MEmu";
                    }
                    Process.Start(info);
                    foreach(var device in AdbClient.Instance.GetDevices())
                    {
                        if(device.ToString() == Variables.AdbIpPort)
                        {
                            Variables.Controlled_Device = device;
                            break;
                        }
                    }
                    Variables.DeviceChanged = true;
                }
                catch (SocketException)
                {

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error while starting emulator! Error message: " + ex.Message);
                    Process.Start("Profiles\\" + EmulatorController.profilePath + "\\bot.ini");
                    Environment.Exit(0);
                }
            }
            else
            {
                MessageBox.Show("Unable to locate path of emulator!");
                Process.Start("Profiles\\" + EmulatorController.profilePath + "\\bot.ini");
                Environment.Exit(0);
            }
        }

        private static byte[] ForegroundCapture(IntPtr handle)
        {
            Image temp = null;
            var stopwatch = Stopwatch.StartNew();
            try
            {
                DllImport.SetForegroundWindow(Variables.Proc.MainWindowHandle);
            }
            catch
            {

            }
            var rect = new DllImport.Rect();
            DllImport.GetWindowRect(handle, ref rect);
            int width = rect.right - rect.left;
            int height = rect.bottom - rect.top;
            temp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            Graphics graphics = Graphics.FromImage(temp);
            graphics.CopyFromScreen(rect.left, rect.top, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);
            stopwatch.Stop();
            if (stopwatch.ElapsedMilliseconds < 500)
            {
                Thread.Sleep(500);
            }
            Variables.AdbLog.Add("WinAPI capt saved to memory stream. Time used: " + stopwatch.ElapsedMilliseconds + " ms");
            return Compress(temp);
        }
        /// <summary>
        /// Get color of location in screenshots
        /// </summary>
        /// <param name="position">The position of image</param>
        /// <param name="image">The image that need to return color</param>
        /// <returns></returns>
        public static Color GetPixel(Point position, byte[] rawimage, [CallerLineNumber] int lineNumber = 0,[CallerMemberName] string caller = null)
        {
            Debug_.WriteLine("Called by Line " + lineNumber + " Caller: " + caller);
            Image image = Decompress(rawimage);
            return new Bitmap(image).GetPixel(position.X, position.Y);
        }

        private static Color GetPixel(int x, int y, IntPtr ptr, int step, int Width, int Height, int Depth, byte[] pixel)
        {
            Color clr = Color.Empty;
            int i = ((y * Width + x) * step);
            if (i > pixel.Length)
            {
                Variables.AdbLog.Add("index of pixel array out of range at GetPixel");
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
        /// <summary>
        /// Compare point RGB from image
        /// </summary>
        /// <param name="image">The image that need to return</param>
        /// <param name="point">The point to check for color</param>
        /// <param name="color">The color to check at point is true or false</param>
        /// <returns>bool</returns>
        public static bool RGBComparer(byte[] image, Point point, Color color, int tolerance,[CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            Debug_.WriteLine("Called by Line " + lineNumber + " Caller: " + caller);
            int red = color.R;
            int blue = color.B;
            int green = color.G;

            Bitmap bmp = new Bitmap(Decompress(image));
            int Width = bmp.Width;
            int Height = bmp.Height;
            int PixelCount = Width * Height;
            Rectangle rect = new Rectangle(0, 0, Width, Height);
            int Depth = Bitmap.GetPixelFormatSize(bmp.PixelFormat);
            if (Depth != 8 && Depth != 24 && Depth != 32)
            {
                Variables.AdbLog.Add("Image bit per pixel format not supported");
                return false;
            }
            BitmapData bd = bmp.LockBits(rect, ImageLockMode.ReadOnly, bmp.PixelFormat);
            int step = Depth / 8;
            try
            {
                byte[] pixel = new byte[PixelCount * step];
                IntPtr ptr = bd.Scan0;
                Marshal.Copy(ptr, pixel, 0, pixel.Length);
                Color clr = GetPixel(point.X, point.Y, ptr, step, Width, Height, Depth, pixel);
                if (clr.R == red && clr.G == green && clr.B == blue)
                {
                    bmp.UnlockBits(bd);
                    return true;
                }
                else
                {
                    if (clr.R > red - tolerance && clr.R < red + tolerance)
                    {
                        if (clr.G > green - tolerance && clr.G < green + tolerance)
                        {
                            if (clr.B > blue - tolerance && clr.B < blue + tolerance)
                            {
                                bmp.UnlockBits(bd);
                                return true;
                            }
                        }
                    }

                }
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
        /// <param name="image">The image that need to return</param>
        /// <param name="point">The point to check for color</param>
        /// <param name="tolerance">Tolerance to the color RGB, example: red=120, Tolerance=20 Result=100~140 red will return true</param>
        /// <returns>bool</returns>
        public static bool RGBComparer(byte[] image, Point point, int red, int green, int blue, int tolerance, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            Debug_.WriteLine("Called by Line " + lineNumber + " Caller: " + caller);
            if (image == null)
            {
                return false;
            }
            Bitmap bmp = new Bitmap(Decompress(image));
            int Width = bmp.Width;
            int Height = bmp.Height;
            int PixelCount = Width * Height;
            Rectangle rect = new Rectangle(0, 0, Width, Height);
            int Depth = Bitmap.GetPixelFormatSize(bmp.PixelFormat);
            if (Depth != 8 && Depth != 24 && Depth != 32)
            {
                Variables.AdbLog.Add("Image bit per pixel format not supported");
                return false;
            }
            BitmapData bd = bmp.LockBits(rect, ImageLockMode.ReadOnly, bmp.PixelFormat);
            int step = Depth / 8;
            try
            {

                byte[] pixel = new byte[PixelCount * step];
                IntPtr ptr = bd.Scan0;
                Marshal.Copy(ptr, pixel, 0, pixel.Length);
                Color clr = GetPixel(point.X, point.Y, ptr, step, Width, Height, Depth, pixel);
                if (clr.R == red && clr.G == green && clr.B == blue)
                {
                    bmp.UnlockBits(bd);
                    return true;
                }
                else
                {
                    if (clr.R > red - tolerance && clr.R < red + tolerance)
                    {
                        if (clr.G > green - tolerance && clr.G < green + tolerance)
                        {
                            if (clr.B > blue - tolerance && clr.B < blue + tolerance)
                            {
                                bmp.UnlockBits(bd);
                                return true;
                            }
                            else
                            {
                                Variables.AdbLog.Add("The point " + point.X + ", " + point.Y + " color is " + clr.R + ", " + clr.G + ", " + clr.B);
                            }
                        }
                    }
                }
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
        /// <returns>bool</returns>
        public static bool RGBComparer(byte[] rawimage, Color color, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            Debug_.WriteLine("Called by Line " + lineNumber + " Caller: " + caller);
            Image image = Decompress(rawimage);
            if (image == null)
            {
                return false;
            }
            Bitmap bmp = new Bitmap(image);
            int Width = bmp.Width;
            int Height = bmp.Height;
            int PixelCount = Width * Height;
            Rectangle rect = new Rectangle(0, 0, Width, Height);
            int Depth = Bitmap.GetPixelFormatSize(bmp.PixelFormat);
            if (Depth != 8 && Depth != 24 && Depth != 32)
            {
                Variables.AdbLog.Add("Image bit per pixel format not supported");
                return false;
            }
            BitmapData bd = bmp.LockBits(rect, ImageLockMode.ReadOnly, bmp.PixelFormat);
            int step = Depth / 8;
            byte[] pixel = new byte[PixelCount * step];
            IntPtr ptr = bd.Scan0;
            Marshal.Copy(ptr, pixel, 0, pixel.Length);
            unsafe
            {
                // example assumes 24bpp image.  You need to verify your pixel depth
                // loop by row for better data locality
                for (int y = 0; y < image.Height; ++y)
                {
                    byte* pRow = (byte*)bd.Scan0 + y * bd.Stride;
                    for (int x = 0; x < image.Width; ++x)
                    {
                        // windows stores images in BGR pixel order
                        byte r = pRow[1];
                        byte g = pRow[2];
                        byte b = pRow[3];
                        if(r == color.R && g == color.G && b == color.B)
                        {
                            bmp.UnlockBits(bd);
                            return true;
                        }
                        // next pixel in the row
                        pRow += 4;
                    }
                }
            }
            bmp.UnlockBits(bd);
            return false;
        }
        /// <summary>
        /// Compare point RGB from image
        /// </summary>
        /// <returns>bool</returns>
        public static bool RGBComparer(byte[] rawimage, Color color, Point start, Point end, out Point? point, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            Debug_.WriteLine("Called by Line " + lineNumber + " Caller: " + caller);
            Image image = Decompress(CropImage(rawimage, start, end));
            if (image == null)
            {
                point = null;
                return false;
            }
            Bitmap bmp = new Bitmap(image);
            int Width = bmp.Width;
            int Height = bmp.Height;
            int PixelCount = Width * Height;
            Rectangle rect = new Rectangle(0, 0, Width, Height);
            int Depth = Bitmap.GetPixelFormatSize(bmp.PixelFormat);
            if (Depth != 8 && Depth != 24 && Depth != 32)
            {
                Variables.AdbLog.Add("Image bit per pixel format not supported");
                point = null;
                return false;
            }
            BitmapData bd = bmp.LockBits(rect, ImageLockMode.ReadOnly, bmp.PixelFormat);
            int step = Depth / 8;
            byte[] pixel = new byte[PixelCount * step];
            IntPtr ptr = bd.Scan0;
            Marshal.Copy(ptr, pixel, 0, pixel.Length);
            unsafe
            {
                // example assumes 24bpp image.  You need to verify your pixel depth
                // loop by row for better data locality
                for (int y = 0; y < image.Height; ++y)
                {
                    byte* pRow = (byte*)bd.Scan0 + y * bd.Stride;
                    for (int x = 0; x < image.Width; ++x)
                    {
                        // windows stores images in BGR pixel order
                        byte r = pRow[1];
                        byte g = pRow[2];
                        byte b = pRow[3];
                        if (r == color.R && g == color.G && b == color.B)
                        {
                            bmp.UnlockBits(bd);
                            point = new Point(x, y);
                            return true;
                        }
                        // next pixel in the row
                        pRow += 4;
                    }
                }
            }
            bmp.UnlockBits(bd);
            point = null;
            return false;
        }

        /// <summary>
        /// Compare point RGB from image
        /// </summary>
        /// <returns>bool</returns>
        public static bool RGBComparer(byte[] rawimage, Color color, out Point? point, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            Debug_.WriteLine("Called by Line " + lineNumber + " Caller: " + caller);
            Image image = Decompress(rawimage);
            if (image == null)
            {
                point = null;
                return false;
            }
            Bitmap bmp = new Bitmap(image);
            int Width = bmp.Width;
            int Height = bmp.Height;
            int PixelCount = Width * Height;
            Rectangle rect = new Rectangle(0, 0, Width, Height);
            int Depth = Bitmap.GetPixelFormatSize(bmp.PixelFormat);
            if (Depth != 8 && Depth != 24 && Depth != 32)
            {
                Variables.AdbLog.Add("Image bit per pixel format not supported");
                point = null;
                return false;
            }
            BitmapData bd = bmp.LockBits(rect, ImageLockMode.ReadOnly, bmp.PixelFormat);
            int step = Depth / 8;
            byte[] pixel = new byte[PixelCount * step];
            IntPtr ptr = bd.Scan0;
            Marshal.Copy(ptr, pixel, 0, pixel.Length);
            for (int i = 0; i < image.Height; i++)
            {
                for (int j = 0; j < image.Width; j++)
                {
                    //Get the color at each pixel
                    Color now_color = GetPixel(j, i, ptr, step, Width, Height, Depth, pixel);

                    //Compare Pixel's Color ARGB property with the picked color's ARGB property 
                    if (now_color.ToArgb() == color.ToArgb())
                    {
                        point = new Point(j, i);
                        bmp.UnlockBits(bd);
                        return true;
                    }
                }
            }
            bmp.UnlockBits(bd);
            point = null;
            return false;
        }
        /// <summary>
        /// Return a Point location of the image in Variables.Image (will return null if not found)
        /// </summary>
        /// <param name="find">The smaller image for matching</param>
        /// <param name="original">Original image that need to get the point on it</param>
        /// <returns>Point or null</returns>
        public static Point? FindImage(byte[] screencapture, Bitmap find, bool GrayStyle, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            Thread.Sleep(100);
            Debug_.WriteLine("Called by Line " + lineNumber + " Caller: " + caller);
            if (screencapture == null)
            {
                Variables.AdbLog.Add("Result return null because of null original image");
                return null;
            }
            Bitmap original = new Bitmap(Decompress(screencapture));
            if (find == null)
            {
                return null;
            }
            try
            {
                if (GrayStyle)
                {
                    Image<Gray, byte> source = new Image<Gray, byte>(original);
                    Image<Gray, byte> template = new Image<Gray, byte>(find);
                    
                    using (Image<Gray, float> result = source.MatchTemplate(template, TemplateMatchingType.CcoeffNormed))
                    {
                        double[] minValues, maxValues;
                        Point[] minLocations, maxLocations;
                        result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

                        // You can try different values of the threshold. I guess somewhere between 0.75 and 0.95 would be good.
                        if (maxValues[0] > 0.9)
                        {
                            Variables.AdbLog.Add("Image matched");
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
                        double[] minValues, maxValues;
                        Point[] minLocations, maxLocations;
                        result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

                        // You can try different values of the threshold. I guess somewhere between 0.75 and 0.95 would be good.
                        if (maxValues[0] > 0.9)
                        {
                            Variables.AdbLog.Add("Image matched");
                            return maxLocations[0];
                        }
                    }
                }
            }
            catch
            {


            }
            Variables.AdbLog.Add("Image not match");
            return null;
        }
        
        /// <summary>
        /// Return a Point location of the image in Variables.Image (will return null if not found)
        /// </summary>
        /// <param name="imagePath">Path of the smaller image for matching</param>
        /// <param name="original">Original image that need to get the point on it</param>
        /// <returns>Point or null</returns>
        public static Point? FindImage(byte[] screencapture, string findPath, bool GrayStyle, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            Thread.Sleep(100);
            Debug_.WriteLine("Called by Line " + lineNumber + " Caller: " + caller);
            if (screencapture == null)
            {
                Variables.AdbLog.Add("Result return null because of null original image");
                return null;
            }
            Bitmap original = new Bitmap(Decompress(screencapture));
            if (!File.Exists(findPath))
            {
                Variables.AdbLog.Add("Unable to find image " + findPath.Split('\\').Last() + ", image path not valid");
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
                        double[] minValues, maxValues;
                        Point[] minLocations, maxLocations;
                        result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);
                        // You can try different values of the threshold. I guess somewhere between 0.75 and 0.95 would be good.
                        if (maxValues[0] > 0.9)
                        {
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
                        double[] minValues, maxValues;
                        Point[] minLocations, maxLocations;
                        result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

                        // You can try different values of the threshold. I guess somewhere between 0.75 and 0.95 would be good.
                        if (maxValues[0] > 0.9)
                        {
                            return maxLocations[0];
                        }
                    }
                }
            }
            catch
            {

            }
            return null;
        }

        /// <summary>
        /// Return a Point location of the image in Variables.Image (will return null if not found)
        /// </summary>
        /// <param name="imagePath">Path of the smaller image for matching</param>
        /// <param name="original">Original image that need to get the point on it</param>
        /// <returns>Point or null</returns>
        public static Point? FindImage(byte[] screencapture, byte[] image, bool GrayStyle, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            Thread.Sleep(100);
            Debug_.WriteLine("Called by Line " + lineNumber + " Caller: " + caller);
            if (screencapture == null)
            {
                Variables.AdbLog.Add("Result return null because of null original image");
                return null;
            }
            Bitmap original = new Bitmap(Decompress(screencapture));
            Bitmap find = new Bitmap(Decompress(image));
            try
            {
                if (GrayStyle)
                {
                    Image<Gray, byte> source = new Image<Gray, byte>(original);
                    Image<Gray, byte> template = new Image<Gray, byte>(find);
                    using (Image<Gray, float> result = source.MatchTemplate(template, TemplateMatchingType.CcoeffNormed))
                    {
                        double[] minValues, maxValues;
                        Point[] minLocations, maxLocations;
                        result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);
                        // You can try different values of the threshold. I guess somewhere between 0.75 and 0.95 would be good.
                        if (maxValues[0] > 0.9)
                        {
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
                        double[] minValues, maxValues;
                        Point[] minLocations, maxLocations;
                        result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

                        // You can try different values of the threshold. I guess somewhere between 0.75 and 0.95 would be good.
                        if (maxValues[0] > 0.9)
                        {
                            return maxLocations[0];
                        }
                    }
                }
            }
            catch
            {

            }
            return null;
        }

        /// <summary>
        /// Crop the image and return the cropped image
        /// </summary>
        /// <param name="image">Image that need to be cropped</param>
        /// <param name="rect">Rectangle size</param>
        /// <param name="start">Starting Point</param>
        /// <param name="End">Ending Point</param>
        /// <returns></returns>
        public static byte[] CropImage(byte[] original, Point start, Point End, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            Debug_.WriteLine("Called by Line " + lineNumber + " Caller: " + caller);
            if (original == null)
            {
                Variables.AdbLog.Add("Result return null because of null original image");
                return null;
            }
            Image<Bgr, byte> imgInput = new Image<Bgr, byte>(new Bitmap(Decompress(original)));
            Rectangle rect = new Rectangle();
            rect.X = Math.Min(start.X, End.X);
            rect.Y = Math.Min(start.Y, End.Y);
            rect.Width = Math.Abs(start.X - End.X);
            rect.Height = Math.Abs(start.Y - End.Y);
            imgInput.ROI = rect;
            Image<Bgr, byte> temp = imgInput.CopyBlank();
            imgInput.CopyTo(temp);
            imgInput.Dispose();
            return Compress(temp.Bitmap);

        }
        /// <summary>
        /// Force emulator keep potrait
        /// </summary>
        public static void StayPotrait()
        {
            var receiver = new ConsoleOutputReceiver();
            if (Variables.Controlled_Device == null)
            {
                return;
            }
            if (Variables.Controlled_Device.State == DeviceState.Online)
            {
                AdbClient.Instance.ExecuteRemoteCommand("content insert --uri content://settings/system --bind name:s:accelerometer_rotation --bind value:i:0", Variables.Controlled_Device, receiver);
                AdbClient.Instance.ExecuteRemoteCommand("content insert --uri content://settings/system --bind name:s:user_rotation --bind value:i:0", Variables.Controlled_Device, receiver);
                receiver.Flush();
            }
        }
        /// <summary>
        /// Rotate image
        /// </summary>
        /// <param name="image"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static Bitmap RotateImage(Image image, float angle)
        {
            if (image == null)
                throw new ArgumentNullException("image");

            PointF offset = new PointF((float)image.Width / 2, (float)image.Height / 2);

            //create a new empty bitmap to hold rotated image
            Bitmap rotatedBmp = new Bitmap(image.Width, image.Height);
            rotatedBmp.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            //make a graphics object from the empty bitmap
            Graphics g = Graphics.FromImage(rotatedBmp);

            //Put the rotation point in the center of the image
            g.TranslateTransform(offset.X, offset.Y);

            //rotate the image
            g.RotateTransform(angle);

            //move the image back
            g.TranslateTransform(-offset.X, -offset.Y);

            //draw passed in image onto graphics object
            g.DrawImage(image, new PointF(0, 0));

            return rotatedBmp;
        }
        /// <summary>
        /// Pull file from emulator to PC
        /// </summary>
        /// <param name="from">path of file in android</param>
        /// <param name="to">path of file on PC</param>
        /// <returns></returns>
        public static bool Pull(string from, string to)
        {
            if (Variables.Controlled_Device == null)
            {
                return false;
            }
            using (SyncService service = new SyncService(new AdbSocket(new IPEndPoint(IPAddress.Loopback, AdbClient.AdbServerPort)), Variables.Controlled_Device))
            using (Stream stream = File.OpenWrite(to))
            {
                service.Pull(from, stream, null, CancellationToken.None);
            }
            if (File.Exists(to))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Push file from PC to emulator
        /// </summary>
        /// <param name="from">path of file on PC</param>
        /// <param name="to">path of file in android</param>
        /// <param name="permission">Permission of file</param>
        public static void Push(string from, string to, int permission)
        {
            if (Variables.Controlled_Device == null)
            {
                return;
            }
            using (SyncService service = new SyncService(new AdbSocket(new IPEndPoint(IPAddress.Loopback, AdbClient.AdbServerPort)), Variables.Controlled_Device))
            using (Stream stream = File.OpenRead(from))
            {
                service.Push(stream, to, permission, DateTime.Now, null, CancellationToken.None);
            }
        }
        /// <summary>
        /// Encrypt text
        /// </summary>
        /// <param name="text">text for encryption</param>
        /// <returns></returns>
        public static string Encrypt(string text)
        {
            if(text == null)
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
        /// Keycode for SendEvent
        /// </summary>
        public enum KeyCode
        {
            /// <summary>
            /// Zoom in
            /// </summary>
            KEYCODE_ZOOM_IN = 168,
            /// <summary>
            /// Zoom out
            /// </summary>
            KEYCODE_ZOOM_OUT = 169,
            /// <summary>
            /// Mute device
            /// </summary>
            KEYCODE_MUTE = 91
        };
        /// <summary>
        /// Send Event to emulator
        /// </summary>
        /// <param name="code">Keycode</param>
        public static void SendEvent(KeyCode code)
        {
            var receiver = new ConsoleOutputReceiver();
            if (Variables.Controlled_Device == null)
            {
                return;
            }
            AdbClient.Instance.ExecuteRemoteCommand("input keyevent " + (int)code, Variables.Controlled_Device, receiver);
        }
        /// <summary>
        /// Calculate SHA256 of string
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
    }
}
