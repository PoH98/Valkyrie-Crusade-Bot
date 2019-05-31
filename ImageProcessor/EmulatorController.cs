﻿using Emgu.CV;
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
using System.Security.Principal;

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
        static readonly AdbServer server = new AdbServer();
        static readonly AdbClient client = new AdbClient();
        static IAdbSocket socket;
        static bool StartingEmulator; //To confirm only start the emulator once in the same time!
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
        public static void LoadEmulatorInterface()
        {
            if(!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
            {
                MessageBox.Show("Admin needed to run this framework!");
                Environment.Exit(0);
            }
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
                if (emu.LoadEmulatorSettings() && Variables.emulator == null)
                {
                    Variables.emulator = emu;
                    break;
                }
            }
            Variables.AdvanceLog("Detected emulator "+Variables.emulator.EmulatorName());
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
            if (!ScriptRun.Run)
            {
                return;
            }
            try
            {
                var receiver = new ConsoleOutputReceiver();
                client.ExecuteRemoteCommand("adb install " + path, Variables.Controlled_Device, receiver);
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
            if (!ScriptRun.Run)
            {
                return;
            }
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
            if (!ScriptRun.Run)
            {
                return;
            }
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
            if (!ScriptRun.Run)
            {
                return;
            }
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
            int error = 0;
            Connect:
            StartAdb();
            if (!ScriptRun.Run)
            {
                return;
            }
            Variables.AdvanceLog("Connecting emulator...");
            Variables.emulator.ConnectEmulator();
            if(Variables.Proc == null)
            {
                if (!ScriptRun.Run)
                {
                    return;
                }
                Variables.AdvanceLog("Emulator not connected, retrying in 2 second...");
                Thread.Sleep(2000);
                error++;
                if(error > 30) //We had await for 1 minute
                {
                    Variables.ScriptLog("Unable to connect to emulator! Emulator refused to load! Restarting it now!",Color.Red);
                    RestartEmulator();
                    Thread.Sleep(10000);
                    error = 0;
                }
                if(error % 5 == 0)
                {
                    Variables.ScriptLog("Emulator is still not running...",Color.Red);
                }
                goto Connect;
            }
            try
            {
                socket = new AdbSocket(new IPEndPoint(IPAddress.Parse("127.0.0.1"), Adb.CurrentPort));
                client.Connect(new DnsEndPoint("127.0.0.1", Convert.ToInt16(Variables.AdbIpPort.Split(':').Last())));
                do
                {
                    foreach (var device in client.GetDevices())
                    {
                        Variables.AdvanceLog(device.ToString());
                        if (device.ToString() == Variables.AdbIpPort)
                        {
                            Variables.Controlled_Device = device;
                            Variables.DeviceChanged = true;
                            if (error % 5 == 0)
                            {
                                Variables.AdvanceLog("Device found, connection establish on " + Variables.AdbIpPort);
                            }
                            client.SetDevice(socket, device);
                            error = 0;
                            break;
                        }
                    }
                    Thread.Sleep(2000);
                    error++;
                } while (!Variables.DeviceChanged  && error < 30);
                if (error > 20 && !Variables.DeviceChanged) //We had await for 1 minute
                {
                    Variables.ScriptLog("Unable to connect to emulator! Emulator refused to load! Restarting it now!", Color.Red);
                    RestartEmulator();
                    Thread.Sleep(10000);
                    error = 0;
                }
            }
            catch
            {
                Thread.Sleep(2000);
                goto Connect;
            }
            if (Variables.Controlled_Device == null)
            {
                //Unable to connect device
                Variables.AdvanceLog("Unable to connect to device " + Variables.AdbIpPort);
                Variables.ScriptLog("Emulator refused to connect or start, bot stopped!", Color.Red);
                ScriptRun.StopScript();
                return;
            }
            ConnectMinitouch();
        }
        /// <summary>
        /// Warning!!Must run this before exit program, else all sockets will continue in the PC even when restarted!!
        /// </summary>
        public static void EjectSockets()
        {
            try
            {
                if(minitouchSocket != null)
                {
                    minitouchSocket.Dispose();
                }
            }
            catch
            {

            }
            var path = Path.GetTempPath() + "minitouch";
            if (File.Exists(path))
            {
                //Remove current socket port from record as we had dispose it!
                var ports = File.ReadAllLines(path);
                int x = 0;
                string [] newports = new string[ports.Length];
                foreach(var port in ports)
                {
                    if(port.Length > 0)
                    {
                        if (Convert.ToInt32(port) != minitouchPort)
                        {
                            newports[x] = port;
                            x++;
                        }
                    }
                }
                File.WriteAllLines(path, newports.Where(y => !string.IsNullOrEmpty(y)).ToArray());
            }
        }
        /// <summary>
        /// Connect minitouch
        /// </summary>
        private static void ConnectMinitouch()
        {
            if (minitouchSocket != null)
            {
                minitouchSocket.Dispose();
            }
            try
            {
                var path = Path.GetTempPath() + "minitouch";
                if (File.Exists(path))
                {
                    var ports = File.ReadAllLines(path);
                    foreach (var tmp in ports)
                    {
                        foreach (var port in ports)
                        {
                            if (minitouchPort == Convert.ToInt32(port))
                            {
                                minitouchPort++;
                            }
                        }
                    }
                }
                using (var stream = File.AppendText(path))
                {
                    stream.WriteLine(minitouchPort); //Let other instance know that this socket is in use!
                }
            }
            catch
            {

            }
            if (!ScriptRun.Run)
            {
                return;
            }
            Variables.AdvanceLog("Connecting Minitouch");
            Push(Environment.CurrentDirectory + "//adb//minitouch", "/data/local/tmp/minitouch", 777);
            try
            {
                int error = 0;
                while(Variables.Controlled_Device == null && ScriptRun.Run)
                {
                    error++;
                    Thread.Sleep(1000);
                    foreach (var device in client.GetDevices())
                    {
                        Variables.AdvanceLog(device.ToString());
                        if (device.ToString() == Variables.AdbIpPort)
                        {
                            Variables.Controlled_Device = device;
                            Variables.DeviceChanged = true;
                            Variables.AdvanceLog("Device found, connection establish on " + Variables.AdbIpPort);
                            break;
                        }
                    }
                    if (error > 30)
                    {
                        RestartEmulator();
                        error = 0;
                    }
                }
                if (!ScriptRun.Run)
                {
                    return;
                }
                Cm:
                ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
                client.ExecuteRemoteCommandAsync("/data/local/tmp/minitouch", Variables.Controlled_Device, receiver, CancellationToken.None, int.MaxValue);
                client.CreateForward(Variables.Controlled_Device, ForwardSpec.Parse($"tcp:{minitouchPort}"), ForwardSpec.Parse("localabstract:minitouch"), true);
                minitouchSocket = new TcpSocket();
                minitouchSocket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"),minitouchPort));
                if (minitouchSocket.Connected)
                {
                    try
                    {
                        string cmd = "d 0 0 0 100\nc\nu 0\nc\n";
                        byte[] bytes = AdbClient.Encoding.GetBytes(cmd);
                        minitouchSocket.Send(bytes, 0, bytes.Length, SocketFlags.None);
                    }
                    catch
                    {
                        Variables.AdvanceLog("Socket disconnected, retrying...");
                        goto Cm;
                    }
                    Variables.AdvanceLog("Minitouch connected on Port number " + minitouchPort);
                    JustStarted = false;
                }
                else
                {
                    Variables.AdvanceLog("Socket disconnected, retrying...");
                    minitouchPort++;
                    goto Cm;
                }
            }
            catch(Exception ex)
            {
                if(ex is AdbException)
                {
                    if(!IsOnline())
                    {
                        RestartEmulator();
                        minitouchPort = 1111;
                        return;
                    }
                    else
                    {
                        server.RestartServer();
                    }
                }
                Variables.AdvanceLog(ex.ToString());
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
            if (!ScriptRun.Run)
            {
                return false;
            }
            try
            {
                var receiver = new ConsoleOutputReceiver();
                if(Variables.Controlled_Device == null)
                {
                    return false;
                }
                client.ExecuteRemoteCommand("dumpsys window windows | grep -E 'mCurrentFocus'", Variables.Controlled_Device, receiver);
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
                Variables.ScriptLog("Adb exception found!" ,Color.Red);
                Variables.AdvanceLog(ex.Message);
                if (!IsOnline())
                {
                    CloseEmulator();
                    Thread.Sleep(10000);
                }
                else
                {
                    server.RestartServer();
                }
            }
            return false;
        }
        private static void StartAdb()
        {
            if (!ScriptRun.Run)
            {
                return;
            }
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
            Variables.AdvanceLog("Adb connected on " + Variables.AdbIpPort);
            return;
        }
        /// <summary>
        /// Start Game by using CustomImg\Icon.png
        /// </summary>
        public static bool StartGame(Bitmap icon, byte[] img)   
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

                    client.ExecuteRemoteCommand("input keyevent KEYCODE_HOME", Variables.Controlled_Device, receiver);
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
                Variables.AdvanceLog(ex.Message);
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
                    var receiver = new ConsoleOutputReceiver();
                    client.ExecuteRemoteCommand("input keyevent KEYCODE_HOME", Variables.Controlled_Device, receiver);
                    Thread.Sleep(1000);
                    client.ExecuteRemoteCommand("am start -n " + packagename, Variables.Controlled_Device, receiver);
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
                Variables.AdvanceLog(ex.Message);
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

                {
                    var receiver = new ConsoleOutputReceiver();
                    client.ExecuteRemoteCommand("input keyevent KEYCODE_HOME", Variables.Controlled_Device, receiver);
                    Thread.Sleep(1000);
                    client.ExecuteRemoteCommand("am force-stop " + packagename, Variables.Controlled_Device, receiver);
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
                Variables.AdvanceLog(ex.Message);
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
            if (!ScriptRun.Run)
            {
                return null;
            }
            try
            {
                if (!Directory.Exists(Variables.SharedPath))
                {
                    MessageBox.Show("Warning, unable to find shared folder! Try to match it manually!");
                    Environment.Exit(0);
                }
                path = Variables.SharedPath + "\\" + SHA256(Variables.AdbIpPort) + ".raw";
                path = path.Replace("\\\\", "\\");
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
                if (!ScriptRun.Run)
                {
                    return null;
                }
                client.ExecuteRemoteCommand("screencap " + Variables.AndroidSharedPath + SHA256(Variables.AdbIpPort) + ".raw", Variables.Controlled_Device, receiver);
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
                Variables.AdvanceLog(ex.Message);
                if (!IsOnline())
                {
                    CloseEmulator();
                    Thread.Sleep(10000);
                }
                else
                {
                    server.RestartServer();
                }
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
            if (!ScriptRun.Run)
            {
                return;
            }
            SendTap(point.X, point.Y, lineNumber,caller);
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
            if (!ScriptRun.Run)
            {
                return;
            }
            
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
                client.ExecuteRemoteCommand("input touchscreen swipe " + x + " " + y + " " + ex + " " + ey + " " + usedTime, Variables.Controlled_Device, receiver);
                if (receiver.ToString().Contains("Error"))
                {
                    Variables.AdvanceLog(receiver.ToString());
                }
            }
            catch (AdbException ex)
            {
                Variables.AdvanceLog("Adb exception found!");
                Variables.AdvanceLog(ex.Message);
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
            if (!ScriptRun.Run)
            {
                return;
            }
            Stopwatch s = Stopwatch.StartNew();
            try
            {
                if (minitouchSocket == null)
                {
                    ConnectMinitouch();
                    return;
                }
                var receiver = new ConsoleOutputReceiver();
                if (Variables.Controlled_Device == null)
                {
                    return;
                }
                string cmd = $"d 0 {x} {y} 100\nc\nu 0\nc\n";
                byte[] bytes = AdbClient.Encoding.GetBytes(cmd);
                minitouchSocket.Send(bytes, 0, bytes.Length, SocketFlags.None);
            }
            catch (AdbException ex)
            {
                Variables.AdvanceLog("Adb exception found!");
                Variables.AdvanceLog(ex.Message);
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
            if (!ScriptRun.Run)
            {
                return;
            }
            try
            {
                if(minitouchSocket == null)
                {
                    ConnectMinitouch();
                }
                byte[] bytes = AdbClient.Encoding.GetBytes(command);
                minitouchSocket.Send(bytes, 0, bytes.Length, SocketFlags.None);
            }
            catch(SocketException)
            {
                minitouchPort++;
                ConnectMinitouch();
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
        /// /// <param name="lineNumber"></param>
        /// <param name="caller"></param>
        public static void SendSwipe(int startX, int startY, int endX, int endY, int usedTime, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
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
                var receiver = new ConsoleOutputReceiver();

                {
                    client.ExecuteRemoteCommand("input touchscreen swipe " + startX + " " + startY + " " + endX + " " + endY + " " + usedTime, Variables.Controlled_Device, receiver);
                }
                if (receiver.ToString().Contains("Error"))
                {
                    Variables.AdvanceLog(receiver.ToString());
                }
            }
            catch (AdbException ex)
            {
                Variables.AdvanceLog("Adb exception found!");
                Variables.AdvanceLog(ex.Message);
                CloseEmulator();
            }
        }
        /// <summary>
        /// Read Configure in Bot.ini and save it into Variables.Configure (Dictionary)
        /// </summary>
        public static void ReadConfig([CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            
            if (!Directory.Exists("Profiles\\" + profilePath))
            {
                Directory.CreateDirectory("Profiles\\" + profilePath);
            }
            if (!File.Exists("Profiles\\" + profilePath + "\\bot.ini"))
            {
                File.WriteAllText("Profiles\\" + profilePath + "\\bot.ini", "ExampleKey=ExampleValue");
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
        /// Adding settings to Variables.Configure
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void ChangeConfig(string key, string value)
        {
            if (Variables.Configure.ContainsKey(key))
            {
                Variables.Configure[key] = value;
            }
            else
            {
                Variables.Configure.Add(key, value);
            }
        }
        /// <summary>
        /// Save Variables.Config settings into bot.ini
        /// </summary>
        public static void SaveConfig()
        {
            File.WriteAllText("Profiles\\" + profilePath + "\\bot.ini",""); //Reset the bot.ini
            foreach (var value in Variables.Configure)
            {
                File.AppendAllText("Profiles\\" + profilePath + "\\bot.ini", value.Key + "=" + value.Value);//Rewrite All data
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
                    if (DllImport.IsWow64Process(DllImport.GetCurrentProcess(), out bool result) && result)
                        return true;
                }
            }
            // The environment must be an x86 environment.
            return false;
        }
        /// <summary>
        /// Start Emulator according to EmulatorInterface
        /// </summary>
        /// <param name="lineNumber"></param>
        /// <param name="caller"></param>
        public static void StartEmulator([CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            if (StartingEmulator)
            {
                return;
            }
            StartingEmulator = true;
            if (!ScriptRun.Run)
            {
                return;
            }
            
            Variables.emulator.StartEmulator();
            Variables.ScriptLog("Starting Emulator...", Color.LimeGreen);
            StartingEmulator = false;
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
            if (!ScriptRun.Run)
            {
                return Color.Black;
            }
            
            Image image = Decompress(rawimage);
            return new Bitmap(image).GetPixel(position.X, position.Y);
        }

        private static Color GetPixel(int x, int y, IntPtr ptr, int step, int Width, int Height, int Depth, byte[] pixel)
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
            if (!ScriptRun.Run)
            {
                return false;
            }
            
            int red = color.R;
            int blue = color.B;
            int green = color.G;
            return RGBComparer(image,point,red,green,blue,tolerance);
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
            if (!ScriptRun.Run)
            {
                return false;
            }
            
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
            if (!ScriptRun.Run)
            {
                return false;
            }
            
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
            if (!ScriptRun.Run)
            {
                point = null;
                return false;
            }
            var image = CropImage(rawimage, start, end);
            if (image == null)
            {
                point = null;
                return false;
            }
            return RGBComparer(image, color, out point);
        }

        /// <summary>
        /// Compare point RGB from image
        /// </summary>
        /// <returns>bool</returns>
        public static bool RGBComparer(byte[] rawimage, Color color, out Point? point, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            if (!ScriptRun.Run)
            {
                point = null;
                return false;
            }
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
            if (!ScriptRun.Run)
            {
                return null;
            }
            if (screencapture == null)
            {
                Variables.AdvanceLog("Result return null because of null original image");
                return null;
            }
            Bitmap original = new Bitmap(Decompress(screencapture));
            return FindImage(original, find, GrayStyle);
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
            if (!ScriptRun.Run)
            {
                return null;
            }
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
                        result.MinMax(out double[] minValues, out double[] maxValues, out Point[] minLocations, out Point[] maxLocations);

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
            if (!ScriptRun.Run)
            {
                return null;
            }
            Stopwatch s = Stopwatch.StartNew();
            
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
                        result.MinMax(out double[] minValues, out double[] maxValues, out Point[] minLocations, out Point[] maxLocations);
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
                        result.MinMax(out double[] minValues, out double[] maxValues, out Point[] minLocations, out Point[] maxLocations);

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
            if (!ScriptRun.Run)
            {
                return null;
            }
            if (screencapture == null)
            {
                Variables.AdvanceLog("Result return null because of null original image");
                return null;
            }
            Bitmap original = new Bitmap(Decompress(screencapture));
            Bitmap find = new Bitmap(Decompress(image));
            return FindImage(original, find, GrayStyle);
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
            if (!ScriptRun.Run)
            {
                return null;
            }
            Stopwatch s = Stopwatch.StartNew();
            
            if (original == null)
            {
                Variables.AdvanceLog("Result return null because of null original image");
                return null;
            }
            Image<Bgr, byte> imgInput = new Image<Bgr, byte>(new Bitmap(Decompress(original)));
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
                client.ExecuteRemoteCommand("content insert --uri content://settings/system --bind name:s:accelerometer_rotation --bind value:i:0", Variables.Controlled_Device, receiver);
                client.ExecuteRemoteCommand("content insert --uri content://settings/system --bind name:s:user_rotation --bind value:i:0", Variables.Controlled_Device, receiver);
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
            try
            {
                if (Variables.Controlled_Device == null)
                {
                    return false;
                }
                socket = new AdbSocket(new IPEndPoint(IPAddress.Parse("127.0.0.1"), Adb.CurrentPort));
                using (SyncService service = new SyncService(socket, Variables.Controlled_Device))
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
            if (!ScriptRun.Run)
            {
                return;
            }
            try
            {
                socket = new AdbSocket(new IPEndPoint(IPAddress.Parse("127.0.0.1"), Adb.CurrentPort));
                using (SyncService service = new SyncService(socket, Variables.Controlled_Device))
                {
                    using (Stream stream = File.OpenRead(from))
                    {
                        service.Push(stream, to, permission, DateTime.Now, null, CancellationToken.None);
                    }
                }
            }
            catch(AdbException ex)
            {
                Variables.AdvanceLog(ex.ToString());
            }
        }
        /// <summary>
        /// Encrypt text
        /// </summary>
        /// <param name="text">text for encryption</param>
        /// <returns></returns>
        public static string Encrypt(string text)
        {
            if (text == null)
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
                Variables.AdvanceLog("Checking device " + Variables.Controlled_Device + " online: " + online.ToString());
            }
            catch
            {

            }
            return online;
        }

        private static readonly Random rnd = new Random();
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
            Thread.Sleep(rnd.Next(mintime, maxtime));
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
            if (!ScriptRun.Run)
            {
                return;
            }
            try
            {
                var receiver = new ConsoleOutputReceiver();
                client.ExecuteRemoteCommand("input text \"" + text.Replace(" ", "%s") + "\"", Variables.Controlled_Device, receiver);
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
                Variables.AdvanceLog(ex.Message);
                CloseEmulator();
            }
        }
    }
}