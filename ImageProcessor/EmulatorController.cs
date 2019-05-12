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
using System.Management;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Reflection;
using System.Net.Sockets;
using BotFramework;
using System.Threading.Tasks;

namespace BotFramework
{
    /// <summary>
    /// All the android emulator controllers are here!
    /// </summary>
    public class BotCore
    {
        /// <summary>
        /// The path to bot.ini
        /// </summary>
        public static string profilePath = Variables.Instance;
        private static string path;
        private static AdbServer server = new AdbServer();
        /// <summary>
        /// Minitouch port number
        /// </summary>
        public static int minitouchPort = 1111;
        /// <summary>
        /// minitouch Tcp socket. Default null, use connectEmulator to gain a socket connection!
        /// </summary>
        public static TcpSocket minitouchSocket;
        private static bool JustStarted = true;
        /// <summary>
        /// Read emulators dll
        /// </summary>
        public static void LoadEmulatorInterface(string[] args)
        {
            List<EmulatorInterface> emulators = new List<EmulatorInterface>();
            if (!Directory.Exists("Emulators"))
            {
                Directory.CreateDirectory("Emulators");
            }
            var dlls = Directory.GetFiles("Emulators", "*.dll");
            if(dlls!=null)
            {
                foreach (var dll in dlls)
                {
                    Assembly a = Assembly.LoadFrom(dll);
                    foreach (var t in a.GetTypes())
                    {
                        if (t.GetInterface("EmulatorInterface") != null)
                        {
                            emulators.Add(Activator.CreateInstance(t) as EmulatorInterface);
                        }
                    }
                }
            }
            foreach (var emu in emulators)
            {
                if (args.Contains(emu.EmulatorName()))
                {
                    emu.LoadEmulatorSettings();
                    Variables.emulator = emu;
                    break;
                }
                else
                {
                    if (emu.LoadEmulatorSettings() && Variables.emulator == null)
                    {
                        Variables.emulator = emu;
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// Compress image into byte array to avoid conflict while multiple function trying to access the image
        /// </summary>
        /// <param name="image">The image for compress</param>
        /// <returns></returns>
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        public static byte[] Compress(Image image)
        {
            try
            {
                ImageConverter _imageConverter = new ImageConverter();
                byte[] xByte = (byte[])_imageConverter.ConvertTo(image, typeof(byte[]));
                return xByte;
            }
            catch
            {
                return null;
            }

        }
        /// <summary>
        /// Decompress the byte array back to image for other usage
        /// </summary>
        /// <param name="buffer">the byte array of image compressed by Compress(Image image)</param>
        /// <returns>Image</returns>
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
        public static void CloseEmulator()
        {
            Variables.emulator.CloseEmulator();
            Variables.Proc = null;
            Variables.Controlled_Device = null;
            Variables.ScriptLog("Emulator Closed",Color.Red);
        }
        /// <summary>
        /// Restart emulator
        /// </summary>
        public static void RestartEmulator()
        {
            CloseEmulator();
            Variables.ScriptLog("Restarting Emulator...",Color.Red);
            Thread.Sleep(1000);
            StartEmulator();
            JustStarted = true;
        }
        /// <summary>
        /// Method for resizing emulators
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void ResizeEmulator(int x, int y)
        {
            CloseEmulator();
            Variables.emulator.SetResolution(x,y);
            Variables.ScriptLog("Restarting Emulator after setting size", Color.Lime);
            StartEmulator();
           
        }
        /// <summary>
        /// Refresh Variables.Controlled_Device, Variables.Proc and BotCore.Handle, following with connection with Minitouch
        /// </summary>
        public static void ConnectAndroidEmulator()
        {
            Variables.emulator.ConnectEmulator();
            try
            {
                foreach (var device in AdbClient.Instance.GetDevices())
                {
                    Variables.AdvanceLog(device.ToString());
                    if (device.ToString() == Variables.AdbIpPort)
                    {
                        Variables.Controlled_Device = device;
                        Variables.DeviceChanged = true;
                        Debug_.WriteLine("Device found, connection establish on " + Variables.AdbIpPort);
                        return;
                    }
                }
            }
            catch
            {

            }
            Push(Environment.CurrentDirectory + "//adb//minitouch", "/data/local/tmp/minitouch", 777);
            ConnectMinitouch();
        }
        /// <summary>
        /// Connect minitouch
        /// </summary>
        private static void ConnectMinitouch()
        {
            try
            {
                int error = 0;
                while(Variables.Controlled_Device == null)
                {
                    error++;
                    Thread.Sleep(1000);
                    foreach (var device in AdbClient.Instance.GetDevices())
                    {
                        Variables.AdvanceLog(device.ToString());
                        if (device.ToString() == Variables.AdbIpPort)
                        {
                            Variables.Controlled_Device = device;
                            Variables.DeviceChanged = true;
                            Debug_.WriteLine("Device found, connection establish on " + Variables.AdbIpPort);
                            break;
                        }
                    }
                    if (error > 60)
                    {
                        RestartEmulator();
                        error = 0;
                    }
                }
                ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
                AdbClient.Instance.ExecuteRemoteCommandAsync("/data/local/tmp/minitouch", Variables.Controlled_Device, receiver, CancellationToken.None, int.MaxValue);
                AdbClient.Instance.CreateForward(Variables.Controlled_Device, ForwardSpec.Parse($"tcp:{minitouchPort}"), ForwardSpec.Parse("localabstract:minitouch"), true);
                minitouchSocket = new TcpSocket();
                minitouchSocket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"),minitouchPort));
                JustStarted = false;
            }
            catch(Exception ex)
            {
                if(ex is AdbException)
                {
                    if(ex.Message.Contains("device offline"))
                    {
                        RestartEmulator();
                        minitouchPort = 1111;
                        return;
                    }
                }
                minitouchPort++;
                ConnectMinitouch();
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
            catch (AdbException ex)
            {
                Variables.ScriptLog("Adb exception found!",Color.Red);
                Debug_.WriteLine(ex.Message);
                if (!IsOnline())
                {
                    CloseEmulator();
                    Thread.Sleep(10000);
                }
            }
            return false;
        }
        /// <summary>
        /// Start ADB server, must be started emulator first!
        /// </summary>
        public static bool StartAdb()
        {
            string adbname = Environment.CurrentDirectory + "\\adb\\adb.exe";
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
            server.StartServer(adbname, false);
            return true;
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
            }
            catch (InvalidOperationException)
            {

            }
            catch (ArgumentOutOfRangeException)
            {
                
            }
            catch (AdbException ex)
            {
                Variables.ScriptLog("Adb exception found!",Color.Red);
                Debug_.WriteLine(ex.Message);
            }
            return false;
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

            }
            catch (ArgumentOutOfRangeException)
            {

            }
            catch (AdbException ex)
            {
                Variables.ScriptLog("Adb exception found!",Color.Red);
                Debug_.WriteLine(ex.Message);
            }
            return false;
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
                }
            }
            catch (InvalidOperationException)
            {

            }
            catch (ArgumentOutOfRangeException)
            {

            }
            catch (AdbException ex)
            {
                Variables.ScriptLog("Adb exception found!",Color.Red);
                Debug_.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// Kill All process tree
        /// </summary>
        /// <param name="pid"></param>
        public static void KillProcessAndChildren(int pid)
        {
            // Cannot close 'system idle process'.
            if (pid == 0)
            {
                return;
            }
            ManagementObjectSearcher searcher = new ManagementObjectSearcher ("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection moc = searcher.Get();
            foreach (ManagementObject mo in moc)
            {
                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            }
            try
            {
                Process proc = Process.GetProcessById(pid);
                proc.Kill();
            }
            catch (ArgumentException)
            {
                // Process already exited.
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
                    Environment.Exit(0);
                }
                path = Variables.SharedPath + "\\" + SHA256(Variables.AdbIpPort) + ".raw";
                Stopwatch s = Stopwatch.StartNew();
                byte[] raw = null;
                var receiver = new ConsoleOutputReceiver();
                if (Variables.Controlled_Device == null)
                {
                    Variables.AdvanceLog("No device connected!");
                    ConnectAndroidEmulator();
                    return null;
                }
                if (Variables.Controlled_Device.State == DeviceState.Offline)
                {
                    if (JustStarted)
                    {
                        Variables.AdvanceLog("Device offline!");
                        return null;
                    }
                    else
                    {
                        RestartEmulator();
                        return null;
                    }
                }
                AdbClient.Instance.ExecuteRemoteCommand("screencap " + Variables.AndroidSharedPath + SHA256(Variables.AdbIpPort) + ".raw", Variables.Controlled_Device, receiver);
                if (Variables.NeedPull)
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                    Pull(Variables.AndroidSharedPath + SHA256(Variables.AdbIpPort) + ".raw", path);
                }
                if (!File.Exists(path))
                {
                    Variables.AdvanceLog("Unable to read rgba file because of file not exist!");
                    return null;
                }
                path = path.Replace("\\\\", "\\");
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
                int expectedsize = 1280 * 720 * 4;
                if (img.Length > expectedsize + 1000 || img.Length < expectedsize - 1000000)
                {
                    ResizeEmulator(1280, 720);
                    return null;
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
                    Variables.AdvanceLog("Screenshot saved to memory stream. Used time: " + s.ElapsedMilliseconds + " ms");
                    return Compress(bmp);
                }
            }
            catch (IOException)
            {

            }
            catch (ArgumentOutOfRangeException)
            {

            }
            catch (AdbException ex)
            {
                Variables.AdvanceLog("Adb exception found!");
                Debug_.WriteLine(ex.Message);
                CloseEmulator();
            }
            catch (SocketException)
            {

            }
            return null;
        }
        /// <summary>
        /// Tap at location
        /// </summary>
        /// <param name="point">The location</param>
        /// <param name="lineNumber"></param>
        /// <param name="caller"></param>
        public static void SendTap(Point point, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            Stopwatch s = Stopwatch.StartNew();
            Debug_.WriteLine("Called by Line " + lineNumber + " Caller: " + caller);
            try
            {
                if(minitouchSocket == null)
                {
                    ConnectMinitouch();
                    return;
                }
                var receiver = new ConsoleOutputReceiver();
                int x = point.X;
                int y = point.Y;
                if (Variables.Controlled_Device == null)
                {
                    return;
                }
                string cmd = $"d 0 {x} {y} 100\nc\nu 0\nc\n";
                byte[] bytes = AdbClient.Encoding.GetBytes(cmd);
                minitouchSocket.Send(bytes, 0, bytes.Length, SocketFlags.None);
            }
            catch (InvalidOperationException)
            {

            }
            catch (ArgumentOutOfRangeException)
            {

            }
            catch (AdbException ex)
            {
                Variables.AdvanceLog("Adb exception found!");
                Debug_.WriteLine(ex.Message);
                CloseEmulator();
            }
            catch (SocketException)
            {
                minitouchPort++;
                ConnectMinitouch();
            }
            s.Stop();
            Variables.AdvanceLog("Tap sended to point " + point.X + ":" + point.Y + ". Used time: " + s.ElapsedMilliseconds + "ms");
        }
        /// <summary>
        /// Swipe the screen
        /// </summary>
        /// <param name="start">Swiping start position</param>
        /// <param name="end">Swiping end position</param>
        /// <param name="usedTime">The time used for swiping, milliseconds</param>
        /// /// <param name="lineNumber"></param>
        /// <param name="caller"></param>
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
                AdbClient.Instance.ExecuteRemoteCommand("input touchscreen swipe " + x + " " + y + " " + ex + " " + ey + " " + usedTime, Variables.Controlled_Device, receiver);
                if (receiver.ToString().Contains("Error"))
                {
                    Variables.AdvanceLog(receiver.ToString());
                }
            }
            catch (InvalidOperationException)
            {

            }
            catch (ArgumentOutOfRangeException)
            {

            }
            catch (AdbException ex)
            {
                Variables.AdvanceLog("Adb exception found!");
                Debug_.WriteLine(ex.Message);
                CloseEmulator();
            }
        }
        /// <summary>
        /// Left click adb command on the point for generating background click in emulators
        /// </summary>
        /// <param name="x">X Posiition for clicking</param>
        /// <param name="y">Y Position for clicking</param>
        /// <param name="lineNumber"></param>
        /// <param name="caller"></param>
        public static void SendTap(int x, int y, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            Stopwatch s = Stopwatch.StartNew();
            Debug_.WriteLine("Called by Line " + lineNumber + " Caller: " + caller);
            try
            {
                var receiver = new ConsoleOutputReceiver();
                if (Variables.Controlled_Device == null)
                {
                    return;
                }
                string cmd = $"d 0 {x} {y} 100\nc\nu 0\nc\n";
                byte[] bytes = AdbClient.Encoding.GetBytes(cmd);
                minitouchSocket.Send(bytes, 0, bytes.Length, SocketFlags.None);
            }
            catch (InvalidOperationException)
            {

            }
            catch (ArgumentOutOfRangeException)
            {

            }
            catch (AdbException ex)
            {
                Variables.AdvanceLog("Adb exception found!");
                Debug_.WriteLine(ex.Message);
                CloseEmulator();
            }
            catch (SocketException)
            {
                minitouchPort++;
                ConnectMinitouch();
            }
            s.Stop();
            Variables.AdvanceLog("Tap sended to point " + x + ":" + y + ". Used time: " + s.ElapsedMilliseconds + "ms");
        }
        /// <summary>
        /// Send minitouch command to device
        /// </summary>
        /// <param name="command">Minitouch command such as d, c, u, m</param>
        public static void Minitouch(string command)
        {
            try
            {
                byte[] bytes = AdbClient.Encoding.GetBytes(command);
                minitouchSocket.Send(bytes, 0, bytes.Length, SocketFlags.None);
            }
            catch(SocketException)
            {
                minitouchPort++;
                ConnectMinitouch();
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
        /// /// <param name="lineNumber"></param>
        /// <param name="caller"></param>
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
                    Variables.AdvanceLog(receiver.ToString());
                }
            }
            catch (InvalidOperationException)
            {

            }
            catch (ArgumentOutOfRangeException)
            {

            }
            catch (AdbException ex)
            {
                Variables.AdvanceLog("Adb exception found!");
                Debug_.WriteLine(ex.Message);
                CloseEmulator();
            }
        }
        /// <summary>
        /// Read Configure in Bot.ini and save it into Variables.Configure (Dictionary)
        /// </summary>
        public static void ReadConfig([CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            Debug_.WriteLine("Called by Line " + lineNumber + " Caller: " + caller);
            if (!Directory.Exists("Profiles\\" + profilePath))
            {
                Directory.CreateDirectory("Profiles\\" + profilePath);
            }
            if (!File.Exists("Profiles\\" + profilePath + "\\bot.ini"))
            {
                File.WriteAllText("Profiles\\" + profilePath + "\\bot.ini", "Emulator=MEmu\nPath=MEmu.exe\nBackground=true");
            }
            var lines = File.ReadAllLines("Profiles\\" + profilePath + "\\bot.ini");
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
                    bool result;
                    if (DllImport.IsWow64Process(DllImport.GetCurrentProcess(), out result) && result)
                        return true;
                }
            }
            // The environment must be an x86 environment.
            return false;
        }
        /// <summary>
        /// Start Emulator accoring to Variables.Configure (Dictionary) Key "Emulator" and "Path"
        /// </summary>
        /// <param name="lineNumber"></param>
        /// <param name="caller"></param>
        public static void StartEmulator([CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            Debug_.WriteLine("Called by Line " + lineNumber + " Caller: " + caller);
            Variables.emulator.StartEmulator();
            Variables.ScriptLog("Unable to connect to selected device!",Color.Red);

        }
        /// <summary>
        /// Get color of location in screenshots
        /// </summary>
        /// <param name="position">The position of image</param>
        /// <param name="rawimage">The image that need to return color</param>
        /// /// <param name="lineNumber"></param>
        /// <param name="caller"></param>
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
        /// <summary>
        /// Compare point RGB from image
        /// </summary>
        /// <param name="image">The image that need to return</param>
        /// <param name="point">The point to check for color</param>
        /// <param name="color">The color to check at point is true or false</param>
        /// <param name="tolerance">The tolerance on color, larger will more inaccurate</param>
        /// <param name="lineNumber"></param>
        /// <param name="caller"></param>
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
                Variables.AdvanceLog("Image bit per pixel format not supported");
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
        /// <param name="blue">Blue value of color</param>
        /// <param name="green">Green value of color</param>
        /// <param name="red">Red value of color</param>
        /// <param name="lineNumber"></param>
        /// <param name="caller"></param>
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
                Variables.AdvanceLog("Image bit per pixel format not supported");
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
                                Variables.AdvanceLog("The point " + point.X + ", " + point.Y + " color is " + clr.R + ", " + clr.G + ", " + clr.B);
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
                Variables.AdvanceLog("Image bit per pixel format not supported");
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
                        bmp.UnlockBits(bd);
                        return true;
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
                Variables.AdvanceLog("Image bit per pixel format not supported");
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
                Variables.AdvanceLog("Image bit per pixel format not supported");
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
        /// <param name="screencapture">Original image that need to get the point on it</param>
        /// <param name="GrayStyle">Convert the images to gray for faster detection</param>
        /// <param name="lineNumber"></param>
        /// <param name="caller"></param>
        /// <returns>Point or null</returns>
        public static Point? FindImage(byte[] screencapture, Bitmap find, bool GrayStyle, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            Stopwatch s = Stopwatch.StartNew();
            Debug_.WriteLine("Called by Line " + lineNumber + " Caller: " + caller);
            if (screencapture == null)
            {
                Variables.AdvanceLog("Result return null because of null original image");
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
                            s.Stop();
                            Variables.AdvanceLog("Image matched. Used time: " + s.ElapsedMilliseconds + " ms");
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
                            s.Stop();
                            Variables.AdvanceLog("Image matched. Used time: " + s.ElapsedMilliseconds + " ms");
                            return maxLocations[0];
                        }
                    }
                }
            }
            catch
            {


            }
            s.Stop();
            Variables.AdvanceLog("Image not matched. Used time: " + s.ElapsedMilliseconds + " ms");
            return null;
        }
        /// <summary>
        /// Return a Point location of the image in Variables.Image (will return null if not found)
        /// </summary>
        /// <param name="find">The smaller image for matching</param>
        /// <param name="original">Original image that need to get the point on it</param>
        /// <param name="GrayStyle">Convert the images to gray for faster detection</param>
        /// <param name="lineNumber"></param>
        /// <param name="caller"></param>
        /// <returns>Point or null</returns>
        /// <returns></returns>
        public static Point? FindImage(Bitmap original, Bitmap find, bool GrayStyle, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            Stopwatch s = Stopwatch.StartNew();
            Debug_.WriteLine("Called by Line " + lineNumber + " Caller: " + caller);
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
                            s.Stop();
                            Variables.AdvanceLog("Image matched. Used time: " + s.ElapsedMilliseconds + " ms");
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
                            s.Stop();
                            Variables.AdvanceLog("Image matched. Used time: " + s.ElapsedMilliseconds + " ms");
                            return maxLocations[0];
                        }
                    }
                }
            }
            catch
            {


            }
            s.Stop();
            Variables.AdvanceLog("Image not matched. Used time: " + s.ElapsedMilliseconds + " ms");
            return null;
        }
        /// <summary>
        /// Return a Point location of the image in Variables.Image (will return null if not found)
        /// </summary>
        /// <param name="findPath">The path of smaller image for matching</param>
        /// <param name="screencapture">Original image that need to get the point on it</param>
        /// <param name="GrayStyle">Convert the images to gray for faster detection</param>
        /// <param name="lineNumber"></param>
        /// <param name="caller"></param>
        /// <returns>Point or null</returns>
        public static Point? FindImage(byte[] screencapture, string findPath, bool GrayStyle, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            Stopwatch s = Stopwatch.StartNew();
            Debug_.WriteLine("Called by Line " + lineNumber + " Caller: " + caller);
            if (screencapture == null)
            {
                Variables.AdvanceLog("Result return null because of null original image");
                return null;
            }
            Bitmap original = new Bitmap(Decompress(screencapture));
            if (!File.Exists(findPath))
            {
                Variables.AdvanceLog("Unable to find image " + findPath.Split('\\').Last() + ", image path not valid");
                return null;
            }
            try
            {
                if (GrayStyle)
                {
                    Image <Gray, byte> source = new Image<Gray, byte>(original);
                    Image<Gray, byte> template = new Image<Gray, byte>(findPath);
                    using (Image<Gray, float> result = source.MatchTemplate(template, TemplateMatchingType.CcoeffNormed))
                    {
                        double[] minValues, maxValues;
                        Point[] minLocations, maxLocations;
                        result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);
                        // You can try different values of the threshold. I guess somewhere between 0.75 and 0.95 would be good.
                        if (maxValues[0] > 0.9)
                        {
                            s.Stop();
                            Variables.AdvanceLog("Image matched. Used time: " + s.ElapsedMilliseconds + " ms");
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
                            s.Stop();
                            Variables.AdvanceLog("Image matched. Used time: " + s.ElapsedMilliseconds + " ms");
                            return maxLocations[0];
                        }
                    }
                }
            }
            catch
            {

            }
            s.Stop();
            Variables.AdvanceLog("Image not matched. Used time: " + s.ElapsedMilliseconds + " ms");
            return null;
        }
        /// <summary>
        /// Return a Point location of the image in Variables.Image (will return null if not found)
        /// </summary>
        /// <param name="image">The smaller image for matching</param>
        /// <param name="screencapture">Original image that need to get the point on it</param>
        /// <param name="GrayStyle">Convert the images to gray for faster detection</param>
        /// <param name="lineNumber"></param>
        /// <param name="caller"></param>
        /// <returns>Point or null</returns>
        public static Point? FindImage(byte[] screencapture, byte[] image, bool GrayStyle, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            Stopwatch s = Stopwatch.StartNew();
            Debug_.WriteLine("Called by Line " + lineNumber + " Caller: " + caller);
            if (screencapture == null)
            {
                Variables.AdvanceLog("Result return null because of null original image");
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
                            s.Stop();
                            
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
                            s.Stop();
                            Variables.AdvanceLog("Image matched. Used time: " + s.ElapsedMilliseconds + " ms");
                            return maxLocations[0];
                        }
                    }
                }
            }
            catch
            {

            }
            s.Stop();
            Variables.AdvanceLog("Image not matched. Used time: " + s.ElapsedMilliseconds + " ms");
            return null;
        }
        /// <summary> 
        /// Crop the image and return the cropped image
        /// </summary>
        /// <param name="original">Image that need to be cropped</param>
        /// <param name="start">Starting Point</param>
        /// <param name="End">Ending Point</param>
        /// <param name="lineNumber"></param>
        /// <param name="caller"></param>
        /// <returns></returns>
        public static byte[] CropImage(byte[] original, Point start, Point End, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            Stopwatch s = Stopwatch.StartNew();
            Debug_.WriteLine("Called by Line " + lineNumber + " Caller: " + caller);
            if (original == null)
            {
                Variables.AdvanceLog("Result return null because of null original image");
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
            s.Stop();
            Variables.AdvanceLog("Image cropped. Used time: " + s.ElapsedMilliseconds + " ms");
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
            try
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
            }
            catch (AdbException)
            {

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
            try
            {
                if (Variables.Controlled_Device == null)
                {
                    return;
                }
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), AdbClient.AdbServerPort);
                AdbSocket socket = new AdbSocket(endPoint);
                for(int x = 0; x < 10; x++)
                {
                    if (!socket.Connected)
                    {
                        socket.Reconnect();
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        break;
                    }
                }
                if (!socket.Connected)
                {
                    Variables.ScriptLog("Unable to create push socket!",Color.Red);
                    return;
                }
                using (SyncService service = new SyncService(socket, Variables.Controlled_Device))
                {
                    using (Stream stream = File.OpenRead(from))
                    {
                        service.Push(stream, to, permission, DateTime.Now, null, CancellationToken.None);
                    }
                }
            }
            catch(AdbException)
            {

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
        /// <summary>
        /// Check device is online
        /// </summary>
        /// <returns></returns>
        private static bool IsOnline()
        {
            bool online = false;
            try
            {
                if (Variables.Controlled_Device != null)
                {
                    if (Variables.Controlled_Device.State == DeviceState.Online)
                    {
                        online = true;
                    }
                }
                Variables.AdvanceLog("Checking device" + Variables.Controlled_Device + " online: " + online.ToString());
            }
            catch
            {

            }
            return online;
        }

        private static Random rnd = new Random();
        /// <summary>
        /// Add delays on script with human like randomize
        /// </summary>
        /// <param name="mintime">Minimum time to delay</param>
        /// <param name="maxtime">Maximum time to delay</param>
        public static void Delay(int mintime, int maxtime)
        {
            if(maxtime < mintime)
            {
                //swap back
                int temp = maxtime;
                maxtime = mintime;
                mintime = temp;
            }
            Thread.Sleep(rnd.Next(mintime, maxtime));
        }
        /// <summary>
        /// Add delays on script with human like randomize
        /// </summary>
        /// <param name="randomtime">A actual time for delay</param>
        /// <param name="accurate">Real accurate or random about 200 miliseconds?</param>
        public static void Delay(int randomtime, bool accurate)
        {
            if (accurate)
            {
                Thread.Sleep(randomtime);
            }
            else
            {
                Thread.Sleep(rnd.Next(randomtime - 100, randomtime + 100));
            }
        }
        /// <summary>
        /// Send text to android emulator, can be used for typing but only ENGLISH!!
        /// </summary>
        /// <param name="text">The text needed to be sent</param>
        public static void SendText(string text)
        {
            try
            {
                var receiver = new ConsoleOutputReceiver();
                AdbClient.Instance.ExecuteRemoteCommand("input text \"" + text + "\"", Variables.Controlled_Device, receiver);
            }
            catch (InvalidOperationException)
            {

            }
            catch (ArgumentOutOfRangeException)
            {

            }
            catch (AdbException ex)
            {
                Variables.AdvanceLog("Adb exception found!");
                Debug_.WriteLine(ex.Message);
                CloseEmulator();
            }
        }
    }
}