using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.OCR;
using Emgu.CV.Structure;
using SharpAdbClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Threading;
using System.Windows.Forms;

namespace ImageProcessor
{
    /// <summary>
    /// Functions that used to controls Emulators
    /// </summary>
    public class EmulatorController
    {
        private static Thread cleaningthread = null;
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        public static byte[] Compress(Image image)
        {
            try
            {
                ImageConverter img = new ImageConverter();
                byte[] buffer = (byte[])img.ConvertTo(image, typeof(byte[]));
                using (var ms = new MemoryStream())
                {
                    var Zip = new GZipStream(ms, CompressionMode.Compress, true);
                    Zip.Write(buffer, 0, buffer.Length);
                    Zip.Close();
                    ms.Position = 0;
                    var compressed = new byte[ms.Length + 4];
                    Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, compressed, 0, 4);
                    ms.Read(compressed, 4, compressed.Length - 4);
                    buffer = null;
                    return compressed;
                }
            }
            catch
            {
                return null;
            }
            
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        public static Image Decompress(byte[] buffer)
        {
            if(buffer == null)
            {
                return null;
            }
            using (var ms = new MemoryStream())
            {
                var msgLength = BitConverter.ToInt32(buffer, 0);
                ms.Write(buffer, 4, buffer.Length - 4);
                var decompressed = new byte[msgLength];
                ms.Position = 0;
                var zip = new GZipStream(ms, CompressionMode.Decompress);
                zip.Read(decompressed, 0, decompressed.Length);
                var bytes = decompressed;
                using(var m = new MemoryStream(bytes))
                {
                    return Image.FromStream(m);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public static void InstallAPK(string path)
        {
            var receiver = new ConsoleOutputReceiver();
            AdbClient.Instance.ExecuteRemoteCommand("adb install " + path, Variables.Devices_Connected[Variables.Control_Device_Num], receiver);
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
                AdbClient.Instance.ExecuteRemoteCommand("dumpsys window windows | grep -E 'mCurrentFocus'", Variables.Devices_Connected[Variables.Control_Device_Num], receiver);
                if (receiver.ToString().Contains(packagename))
                {
                    return true;
                }
            }
            catch
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
            AdbServer server = new AdbServer();
            try
            {
                string adbname, path;
                if(!Variables.Configure.TryGetValue("Adb_Path",out adbname))
                {
                    Variables.Configure.TryGetValue("Path", out path);
                    path = path.Remove(path.LastIndexOf('\\'));
                    IEnumerable<string> exe = Directory.EnumerateFiles(path, "*.exe"); // lazy file system lookup
                    foreach(var e in exe)
                    {
                        if (e.Contains("adb"))
                        {
                            adbname = e;
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
                if(Process.GetProcessesByName(adbname).Length < 1)
                {
                    ProcessStartInfo adb = new ProcessStartInfo();
                    adb.FileName = adbname;
                    adb.CreateNoWindow = true;
                    adb.WindowStyle = ProcessWindowStyle.Hidden;
                    adb.UseShellExecute = true;
                    Process.Start(adb);
                }
                var result = server.StartServer(adbname, true);
                var temp = AdbClient.Instance.GetDevices();
                foreach (var t in temp)
                {
                    Variables.Devices_Connected.Add(t);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Start Game by using CustomImg\Icon.png
        /// </summary>
        public static bool StartGame(Bitmap icon)
        {
            var receiver = new ConsoleOutputReceiver();
            AdbClient.Instance.ExecuteRemoteCommand("input keyevent KEYCODE_HOME", Variables.Devices_Connected[Variables.Control_Device_Num], receiver);
            Thread.Sleep(1000);
            byte[] img = ImageCapture();
            var ico = FindImage(img, icon);
            if (ico != null)
            {
                SendTap(ico.Value);
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Fast Capturing screen and return the image, uses WinAPI capture if Variables.Background is false
        /// </summary>
        public static byte[] ImageCapture()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                if (Variables.Background)
                {
                    Image screenshot = null;
                    try
                    {
                        screenshot = AdbClient.Instance.GetFrameBufferAsync(Variables.Devices_Connected[Variables.Control_Device_Num], CancellationToken.None).Result;
                    }
                    catch (SharpAdbClient.Exceptions.AdbException)
                    {
                        MessageBox.Show("模拟器已关闭！自动退出！");
                        Environment.Exit(0);
                    }
                    stopwatch.Stop();
                    Variables.AdbLog.Add("FastCapt saved to memory stream. Time used: " + stopwatch.ElapsedMilliseconds + " ms");
                    return Compress(screenshot);
                }
                else
                {
                    return ForegroundCapture();
                }
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// Left click adb command on the point for generating background click in emulators
        /// </summary>
        /// <param name="point">Posiition for clicking</param>
        public static void SendTap(Point point)
        {
            var receiver = new ConsoleOutputReceiver();
            int x = point.X;
            int y = point.Y;
            AdbClient.Instance.ExecuteRemoteCommand("input tap " + x + " " + y, Variables.Devices_Connected[Variables.Control_Device_Num], receiver);
            if (receiver.ToString().Contains("Error"))
            {
                Variables.AdbLog.Add(receiver.ToString());
            }
        }
        /// <summary>
        /// Swipe the screen
        /// </summary>
        /// <param name="start">Swiping start position</param>
        /// <param name="end">Swiping end position</param>
        /// <param name="usedTime">The time used for swiping, milliseconds</param>
        public static void SendSwipe(Point start, Point end, int usedTime)
        {
            var receiver = new ConsoleOutputReceiver();
            int x = start.X;
            int y = start.Y;
            int ex = end.X;
            int ey = end.Y;
            AdbClient.Instance.ExecuteRemoteCommand("input touchscreen swipe " + x + " " + y + " " + ex + " " + ey + " " + usedTime, Variables.Devices_Connected[Variables.Control_Device_Num], receiver);
            if (receiver.ToString().Contains("Error"))
            {
                Variables.AdbLog.Add(receiver.ToString());
            }
        }
        /// <summary>
        /// Left click adb command on the point for generating background click in emulators
        /// </summary>
        /// <param name="point">Posiition for clicking</param>
        public static void SendTap(int x, int y)
        {
            var receiver = new ConsoleOutputReceiver();
            AdbClient.Instance.ExecuteRemoteCommand("input tap " + x + " " + y, Variables.Devices_Connected[Variables.Control_Device_Num], receiver);
            if (receiver.ToString().Contains("Error"))
            {
                Variables.AdbLog.Add(receiver.ToString());
            }
        }
        /// <summary>
        /// Swipe the screen
        /// </summary>
        /// <param name="start">Swiping start position</param>
        /// <param name="end">Swiping end position</param>
        /// <param name="usedTime">The time used for swiping, milliseconds</param>
        public static void SendSwipe(int startX, int startY, int endX, int endY, int usedTime)
        {
            var receiver = new ConsoleOutputReceiver();
            AdbClient.Instance.ExecuteRemoteCommand("input touchscreen swipe " + startX + " " + startY + " " + endX + " " + endY+ " " + usedTime, Variables.Devices_Connected[Variables.Control_Device_Num], receiver);
            if (receiver.ToString().Contains("Error"))
            {
                Variables.AdbLog.Add(receiver.ToString());
            }
        }
        /// <summary>
        /// Read Configure in Bot.ini and save it into Variables.Configure (Dictionary)
        /// </summary>
        public static void ReadConfig()
        {
            if (!File.Exists("bot.ini"))
            {
                File.WriteAllText("bot.ini", "Emulator=MEmu\nPath=MEmu.exe\nBackground=true");
            }
            var lines = File.ReadAllLines("bot.ini");
            foreach (var l in lines)
            {
                string[] temp = l.Split('=');
                if (temp.Length > 1)
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
        public static bool StartEmulator()
        {
            int tried = 0;
            while (!StartedEmul && tried < 1)
            {
                Thread t = new Thread(StartEmul);
                t.Start();
                tried++;
            }
            return StartedEmul;
        }
        /// <summary>
        /// Line that await for new devices connected and refresh Variables.Devices_Connected
        /// </summary>
        public static void DeviceConnected()
        {
            try
            {
                var monitor = new DeviceMonitor(new AdbSocket(new IPEndPoint(IPAddress.Loopback, AdbClient.AdbServerPort)));
                monitor.DeviceConnected += OnDeviceConnected;
                monitor.Start();
            }
            catch
            {
                Thread.Sleep(1000);
                DeviceConnected();
            }
        }

        private static byte[] ForegroundCapture()
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
            DllImport.GetWindowRect(Variables.Proc.MainWindowHandle, ref rect);
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

        private static void OnDeviceConnected(object sender, DeviceDataEventArgs e)
        {
            StartAdb();
            Variables.DeviceChanged = true;
        }

        /// <summary>
        /// Get color of location in screenshots
        /// </summary>
        /// <param name="position">The position of image</param>
        /// <param name="image">The image that need to return color</param>
        /// <returns></returns>
        public static Color GetPixel(Point position, byte[] rawimage)
        {
            Image image = Decompress(rawimage);
            return new Bitmap(image).GetPixel(position.X, position.Y);
        }
        /// <summary>
        /// Compare point RGB from image
        /// </summary>
        /// <param name="image">The image that need to return</param>
        /// <param name="point">The point to check for color</param>
        /// <param name="color">The color to check at point is true or false</param>
        /// <returns>bool</returns>
        public static bool RGBComparer(byte[] image, Point point, Color color, int tolerance)
        {
            int red = color.R;
            int blue = color.B;
            int green = color.G;
            using (Bitmap bmp = new Bitmap(Decompress(image)))
            {
                Color clr = bmp.GetPixel(point.X, point.Y);
                if (clr.R == red && clr.G == green && clr.B == blue)
                {
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
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// Compare point RGB from image
        /// </summary>
        /// <param name="image">The image that need to return</param>
        /// <param name="point">The point to check for color</param>
        /// <param name="tolerance">Tolerance to the color RGB, example: red=120, Tolerance=20 Result=100~140 red will return true</param>
        /// <returns>bool</returns>
        public static bool RGBComparer(byte[] image, Point point, int red, int green, int blue, int tolerance)
        {
            if(image == null)
            {
                return false;
            }
            using (Bitmap bmp = new Bitmap(Decompress(image)))
            {
                Color clr = bmp.GetPixel(point.X, point.Y);
                if (clr.R == red && clr.G == green && clr.B == blue)
                {
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
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// Compare point RGB from image
        /// </summary>
        /// <returns>bool</returns>
        public static bool RGBComparer(byte[] rawimage, Color color)
        {
            Image image = Decompress(rawimage);
            if (image == null)
            {
                return false;
            }
            if (image.Height > 1280 || image.Width > 1280)
            {
                throw new Exception("Image is too large for processing! This will slows down the process! Please reduce your image size!");
            }
            Bitmap bmp = new Bitmap(image);
            for (int i = 0; i < image.Height; i++)
            {
                for (int j = 0; j < image.Width; j++)
                {
                    //Get the color at each pixel
                    Color now_color = bmp.GetPixel(j, i);

                    //Compare Pixel's Color ARGB property with the picked color's ARGB property 
                    if (now_color.ToArgb() == color.ToArgb())
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// Compare point RGB from image
        /// </summary>
        /// <returns>bool</returns>
        public static bool RGBComparer(byte[] rawimage, Color color,Point start, Point end, out Point? point)
        {
            Image image = Decompress(rawimage);
            if (image == null)
            {
                point = null;
                return false;
            }
            if (image.Height > 1280 || image.Width > 1280)
            {
                throw new Exception("Image is too large for processing! This will slows down the process! Please reduce your image size!");
            }
            Bitmap bmp = new Bitmap(image);
            for (int i = start.Y; i < end.Y; i++)
            {
                for (int j = start.X; j < end.X; j++)
                {
                    //Get the color at each pixel
                    if(i < image.Height && j < image.Width)
                    {
                        Color now_color = bmp.GetPixel(j, i);

                        //Compare Pixel's Color ARGB property with the picked color's ARGB property 
                        if (now_color.ToArgb() == color.ToArgb())
                        {
                            point = new Point(j, i);
                            return true;
                        }
                    }
                }
            }
            point = null;
            return false;
        }
        /// <summary>
        /// Return a Point location of the image in Variables.Image (will return null if not found)
        /// </summary>
        /// <param name="find">The smaller image for matching</param>
        /// <param name="original">Original image that need to get the point on it</param>
        /// <returns>Point or null</returns>
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        public static Point? FindImage(byte[] screencapture, Bitmap find)
        {
            Bitmap original = new Bitmap(Decompress(screencapture));
            var stopwatch = Stopwatch.StartNew();
            if(find == null)
            {
                return null;
            }
            if (original == null)
            {
                Variables.AdbLog.Add("Result return null because od null original image");
                return null;
            }
            try
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
                        stopwatch.Stop();
                        Variables.AdbLog.Add("Image Processed. Used time " + stopwatch.ElapsedMilliseconds + " ms");
                        // This is a match. Do something with it, for example draw a rectangle around it.
                        source.Dispose();
                        template.Dispose();
                        return maxLocations[0];
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
        /// <param name="find">The smaller image for matching</param>
        /// <param name="original">Original image that need to get the point on it</param>
        /// <returns>Point or null</returns>
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        public static Point?[] FindImage(byte[] screencapture, Bitmap[] find)
        {
            Bitmap original = new Bitmap(Decompress(screencapture));
            List<Point?> p = new List<Point?>();
            var stopwatch = Stopwatch.StartNew();
            if (find == null)
            {
                return null;
            }
            if (original == null)
            {
                Variables.AdbLog.Add("Result return null because od null original image");
                return null;
            }
            try
            {
                Image<Bgr, byte> source = new Image<Bgr, byte>(original);
                int num = 0;
                while(num < find.Length)
                {
                    Image<Bgr, byte> template = new Image<Bgr, byte>(find[num]);
                    using (Image<Gray, float> result = source.MatchTemplate(template, TemplateMatchingType.CcoeffNormed))
                    {
                        double[] minValues, maxValues;
                        Point[] minLocations, maxLocations;
                        result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

                        // You can try different values of the threshold. I guess somewhere between 0.75 and 0.95 would be good.
                        if (maxValues[0] > 0.9)
                        {
                            // This is a match. Do something with it, for example draw a rectangle around it.
                            source.Dispose();
                            template.Dispose();
                            p.Add(maxLocations[0]);
                            num++;
                        }
                    }
                }
                stopwatch.Stop();
                Variables.AdbLog.Add("Image Processed. Used time " + stopwatch.ElapsedMilliseconds + " ms");
                return p.ToArray();
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
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        public static Point? FindImage(byte[] screencapture, string findPath)
        {
            Bitmap original = new Bitmap(Decompress(screencapture));
            if (!File.Exists(findPath))
            {
                Variables.AdbLog.Add("Unable to find image " + findPath.Split('\\').Last() + ", image path not valid");
                return null;
            }
            var stopwatch = Stopwatch.StartNew();
            Bitmap find = null;
            try
            {
                using (Stream bmp = File.Open(findPath, FileMode.Open))
                {
                    Image image = Image.FromStream(bmp);
                    find = new Bitmap(image);
                }
            }
            catch
            {

            }
            if(original == null)
            {
                Variables.AdbLog.Add("Find Image result return null because of null original image");
                return null;
            }
            try
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
                        stopwatch.Stop();
                        Variables.AdbLog.Add("Image Processed. Used time " + stopwatch.ElapsedMilliseconds + " ms");
                        source.Dispose();
                        template.Dispose();
                        // This is a match. Do something with it, for example draw a rectangle around it.
                        return maxLocations[0];
                    }
                }
            }
            catch
            {
                
            }
            return null;
        }
        /// <summary>
        /// Return multiple Point location of the image in Variables,Image (will return null if not found)
        /// </summary>
        /// <param name="imagePath">Path of the smaller image for matching in original</param>
        /// <param name="original">Original Image for finding point</param>
        /// <returns>Point arrays</returns>
        public static Point[] MultipleImage(byte[] screencapture, string imagePath)
        {
            Bitmap original = new Bitmap( Decompress(screencapture));
            var stopwatch = Stopwatch.StartNew();
            List<Point> Return = new List<Point>();
            if (!File.Exists(imagePath))
            {
                Variables.AdbLog.Add("Unable to find image " + imagePath.Split('\\').Last() + ", image path not valid");
                return null;
            }
            Bitmap find;
            using (Stream bmp = File.Open(imagePath, FileMode.Open))
            {
                Image image = Image.FromStream(bmp);
                find = new Bitmap(image);
            }
            if (original.Width < find.Width || original.Height < find.Height)
            {
                Variables.AdbLog.Add("Image is too big to search in the emulator");
                return null;
            }
            if (original == null)
            {
                original = new Bitmap(Decompress(ImageCapture()));
            }
            while (true)
            {
                Image<Bgr, byte> source = new Image<Bgr, byte>(original);
                Image<Bgr, byte> template = new Image<Bgr, byte>(find);
                using (Image<Gray, float> result = source.MatchTemplate(template, TemplateMatchingType.CcoeffNormed))
                {
                    CvInvoke.Threshold(result, result, 0.7, 1, ThresholdType.ToZero);
                    double[] minValues, maxValues;
                    Point[] minLocations, maxLocations;
                    result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);
                    // You can try different values of the threshold. I guess somewhere between 0.75 and 0.95 would be good.
                    if (maxValues[0] > 0.7)
                    {
                        Pen p = new Pen(Brushes.Black);
                        using (Graphics g = Graphics.FromImage(original))
                        {
                            g.Clear(Color.White);
                            g.DrawLine(new Pen(Color.Red), maxLocations[0], new Point(maxLocations[0].X + 5, maxLocations[0].Y+5));
                        }
                        Return.Add(maxLocations[0]);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            stopwatch.Stop();
            Variables.AdbLog.Add("Image Processed. Used time " + stopwatch.ElapsedMilliseconds + " ms");
            return Return.ToArray();
        }
        /// <summary>
        /// Return multiple Point location of the image in Variables,Image (will return null if not found)
        /// </summary>
        /// <param name="find">Smaller image for matching in original</param>
        /// <param name="original">Original Image for finding point</param>
        /// <returns>Point arrays</returns>
        public static Point[] MultipleImage(byte[] screencapture, Bitmap find)
        {
            Bitmap original = new Bitmap(Decompress(screencapture));
            var stopwatch = Stopwatch.StartNew();
            List<Point> Return = new List<Point>();
            if (original.Width < find.Width || original.Height < find.Height)
            {
                Variables.AdbLog.Add("Image is too big to search in the emulator");
                return null;
            }
            while (true)
            {
                Image<Bgr, byte> source = new Image<Bgr, byte>(original);
                Image<Bgr, byte> template = new Image<Bgr, byte>(find);
                using (Image<Gray, float> result = source.MatchTemplate(template, TemplateMatchingType.CcoeffNormed))
                {
                    CvInvoke.Threshold(result, result, 0.7, 1, ThresholdType.ToZero);
                    double[] minValues, maxValues;
                    Point[] minLocations, maxLocations;
                    result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);
                    // You can try different values of the threshold. I guess somewhere between 0.75 and 0.95 would be good.
                    if (maxValues[0] > 0.7)
                    {
                        Pen p = new Pen(Brushes.Black);
                        using (Graphics g = Graphics.FromImage(original))
                        {
                            g.Clear(Color.White);
                            g.DrawLine(new Pen(Color.Red), maxLocations[0], new Point(maxLocations[0].X + 5, maxLocations[0].Y + 5));
                        }
                        Return.Add(maxLocations[0]);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            stopwatch.Stop();
            Variables.AdbLog.Add("Image Processed. Used time " + stopwatch.ElapsedMilliseconds + " ms");
            return Return.ToArray();
        }
        /// <summary>
        /// Crop the image and return the cropped image
        /// </summary>
        /// <param name="image">Image that need to be cropped</param>
        /// <param name="rect">Rectangle size</param>
        /// <param name="start">Starting Point</param>
        /// <param name="End">Ending Point</param>
        /// <returns></returns>
        public static byte[] CropImage(byte[] original, Point start, Point End)
        {
            Bitmap image = new Bitmap(Decompress(original));
            if (image == null)
            {
                Variables.AdbLog.Add("Image crop failed! Image is null!");
                return null;
            }
            image.Save("temp.bmp", ImageFormat.Bmp);
            Image<Bgr, byte> imgInput = new Image<Bgr, byte>("temp.bmp");
            Rectangle rect = new Rectangle();
            rect.X = Math.Min(start.X, End.X);
            rect.Y = Math.Min(start.Y, End.Y);
            rect.Width = Math.Abs(start.X - End.X);
            rect.Height = Math.Abs(start.Y - End.Y);
            imgInput.ROI = rect;
            Image<Bgr, byte> temp = imgInput.CopyBlank();
            imgInput.CopyTo(temp);
            imgInput.Dispose();
            File.Delete("temp.bmp");
            return Compress(temp.Bitmap);

        }

        private static bool StartedEmul = false;

        private static void StartEmul()
        {
            string temp = "";
            Variables.Configure.TryGetValue("Emulator", out temp);
            if (Process.GetProcessesByName(temp).Length == 0)
            {
                if (Variables.Configure.TryGetValue("Path", out temp))
                {
                    try
                    {
                        if (!File.Exists(temp))
                        {
                            MessageBox.Show("Unable to locate path of emulator!");
                            Process.Start("bot.ini");
                            StartedEmul = false;
                        }
                        Process proc = Process.Start(temp);
                        Thread.Sleep(1000);
                        Variables.Devices_Connected = AdbClient.Instance.GetDevices();
                        Variables.DeviceChanged = true;
                        StartedEmul = true;
                    }
                    catch (SocketException)
                    {
                        StartEmul();
                    }
                    catch
                    {
                        MessageBox.Show("Error while starting emulator!");
                        Environment.Exit(0);
                        Process.Start("bot.ini");
                    }
                }
                else
                {
                    MessageBox.Show("Unable to locate path of emulator!");
                    Process.Start("bot.ini");
                }
            }
            else
            {
                StartedEmul = true;
            }
            StartedEmul = false;
        }
    }
}
