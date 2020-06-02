using SharpAdbClient;
using SharpAdbClient.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace BotFramework
{
    /// <summary>
    /// Load all emulator interface to check if emulator is installed
    /// </summary>
    public class EmulatorLoader
    {

        [DllImport("ProcCmdLine32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetProcCmdLine")]
        private extern static bool GetProcCmdLine32(uint nProcId, StringBuilder sb, uint dwSizeBuf);

        [DllImport("ProcCmdLine64.dll", CharSet = CharSet.Unicode, EntryPoint = "GetProcCmdLine")]
        private extern static bool GetProcCmdLine64(uint nProcId, StringBuilder sb, uint dwSizeBuf);

        /// <summary>
        /// Get Process 64it version command line
        /// </summary>
        /// <param name="proc">The process to get command line</param>
        /// <returns>Command Line</returns>
        public static string GetCommandLineOfProcess(Process proc)
        {
            var sb = new StringBuilder(0xFFFF);
            switch (IntPtr.Size)
            {
                case 4: GetProcCmdLine32((uint)proc.Id, sb, (uint)sb.Capacity); break;
                case 8: GetProcCmdLine64((uint)proc.Id, sb, (uint)sb.Capacity); break;
            }
            return sb.ToString();
        }

        /// <summary>
        /// Read emulators dll
        /// </summary>
        public static void LoadEmulatorInterface([CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
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
            if (dlls != null)
            {
                foreach (var dll in dlls)
                {
                    try
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
                    catch
                    {

                    }
                }
            }
            bool[] installed = new bool[emulators.Count];
            for (int x = 0; x < emulators.Count; x++)
            {
                try
                {
                    installed[x] = emulators[x].LoadEmulatorSettings();
                    if (installed[x])
                    {
                        Variables.AdvanceLog("[" + DateTime.Now.ToLongTimeString() + "]:Detected emulator " + emulators[x].EmulatorName(), lineNumber, caller);
                        EmuSelection_Resource.emu.Add(emulators[x]);
                    }
                }
                catch
                {

                }
            }
            Variables.AdbIpPort = "";
            Variables.AndroidSharedPath = "";
            Variables.SharedPath = "";
            Variables.VBoxManagerPath = "";
            Variables.ForceWinApiCapt = false;
            Variables.ClickPointMultiply = 1;
        Emulator:
            if (EmuSelection_Resource.emu.Count() > 1) //More than one installed
            {
                if (!File.Exists("Emulators\\Use_Emulator.ini"))
                {
                    Emulator_Selection em = new Emulator_Selection();
                    em.ShowDialog();
                    if (em.DialogResult == DialogResult.OK)
                    {
                        foreach (var e in emulators)
                        {
                            if (e.GetType().Name == EmuSelection_Resource.selected)
                            {
                                Variables.emulator = e;
                                e.LoadEmulatorSettings();
                                CheckSharedPath();
                                File.WriteAllText("Emulators\\Use_Emulator.ini", "use=" + e.GetType().Name);
                                Debug_.WriteLine("[" + DateTime.Now.ToLongTimeString() + "]:Emulator used: " + Variables.emulator.GetType().Name);
                                return;
                            }
                        }
                    }
                    else //No selection
                    {
                        for (int x = 0; x < emulators.Count(); x++)
                        {
                            if (installed[x])
                            {
                                Variables.emulator = emulators[x];
                                emulators[x].LoadEmulatorSettings();
                                CheckSharedPath();
                                File.WriteAllText("Emulators\\Use_Emulator.ini", "use=" + emulators[x].EmulatorName());
                                Debug_.WriteLine("[" + DateTime.Now.ToLongTimeString() + "]:Emulator used: " + Variables.emulator.GetType().Name);
                                return;
                            }
                        }
                    }
                }
                else
                {
                    var line = File.ReadAllLines("Emulators\\Use_Emulator.ini")[0].Replace("use=", "");
                    foreach (var e in emulators)
                    {
                        if (e.GetType().Name == line)
                        {
                            Variables.emulator = e;
                            e.LoadEmulatorSettings();
                            CheckSharedPath();
                            Debug_.WriteLine("[" + DateTime.Now.ToLongTimeString() + "]:Emulator used: " + Variables.emulator.GetType().Name);
                            return;
                        }
                    }
                    if (Variables.emulator == null)
                    {
                        File.Delete("Emulators\\Use_Emulator.ini");
                        goto Emulator;
                    }
                }
            }
            else if (EmuSelection_Resource.emu.Count() < 1) //No installed
            {
                Debug_.WriteLine("[" + DateTime.Now.ToLongTimeString() + "]:However none of this are usable!");
                MessageBox.Show("Please install any supported emulator first or install extentions to support your current installed emulator!", "No supported emulator found!");
                Environment.Exit(0);
            }
            else
            {
                Variables.emulator = EmuSelection_Resource.emu[0];
                Variables.emulator.LoadEmulatorSettings();
                Debug_.WriteLine("[" + DateTime.Now.ToLongTimeString() + "]:Emulator used: " + Variables.emulator.GetType().Name);
                CheckSharedPath();
            }
        }

        private static void CheckSharedPath()
        {
            try
            {
                Variables.SharedPath = new string(Variables.SharedPath.Select(ch => Path.GetInvalidPathChars().Contains(ch) ? Path.DirectorySeparatorChar : ch).ToArray());
                Variables.SharedPath = Variables.SharedPath.Replace("'", "");
                Variables.SharedPath = Path.GetFullPath(Variables.SharedPath);
            }
            catch
            {
                //Error path
                Variables.SharedPath = "";
                Variables.ForceWinApiCapt = true;
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
                ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
                AdbInstance.Instance.client.ExecuteRemoteCommand("adb install " + path, (Variables.Controlled_Device as DeviceData), receiver);
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
            EjectSockets();
            if(Variables.VBoxManagerPath != null && (Variables.emulator.EmulatorDefaultInstanceName() != null || Variables.Instance != null))
            {
                ProcessStartInfo close = new ProcessStartInfo();
                close.FileName = Variables.VBoxManagerPath;
                if (Variables.Instance.Length > 0)
                {
                    close.Arguments = "controlvm " + Variables.Instance + " poweroff";
                }
                else
                {
                    close.Arguments = "controlvm "+Variables.emulator.EmulatorDefaultInstanceName() + " poweroff";
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
                Thread.Sleep(5000);
                if (!p.HasExited)
                {
                    try
                    {
                        p.Kill();
                    }
                    catch
                    {

                    }
                }
            }
            if(Variables.Proc != null)
            {
                if (!Variables.Proc.HasExited)
                {
                    try
                    {
                        Variables.Proc.Kill();
                    }
                    catch
                    {

                    }
                }
            }
            Variables.Proc = null;
            Variables.Controlled_Device = null;
            Variables.ScriptLog("Emulator Closed", Color.Red);
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
            Thread.Sleep(5000);
            Variables.ScriptLog("Restarting Emulator...", Color.Red);
            StartEmulator();
        }
        /// <summary>
        /// Start Emulator according to EmulatorInterface
        /// </summary>
        public static void StartEmulator()
        {
            if (AdbInstance.Instance.StartingEmulator)
            {
                return;
            }
            AdbInstance.Instance.StartingEmulator = true;
            if (!ScriptRun.Run)
            {
                return;
            }
            Variables.emulator.StartEmulator();
            Variables.ScriptLog("Starting Emulator...", Color.LimeGreen);
            AdbInstance.Instance.StartingEmulator = false;
        }
        /// <summary>
        /// Method for resizing emulators using Variables.EmulatorWidth, Variables.EmulatorHeight, Variables.EmulatorDpi
        /// </summary>
        public static void ResizeEmulator()
        {
            if (!ScriptRun.Run)
            {
                return;
            }
            CloseEmulator();
            BotCore.Delay(3000);
            Variables.emulator.SetResolution(Variables.EmulatorWidth, Variables.EmulatorHeight, Variables.EmulatorDpi);
            Variables.ScriptLog("Restarting Emulator after setting size", Color.Lime);
            StartEmulator();
        }
        /// <summary>
        /// Refresh (Variables.Controlled_Device as DeviceData), Variables.Proc and BotCore.Handle, following with connection with Minitouch. 
        /// Remember to run EjectSockets before exit program to avoid errors!
        /// </summary>
        public static void ConnectAndroidEmulator([CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            int error = 0;
        Connect:
            StartAdb();
            if (!ScriptRun.Run)
            {
                return;
            }
            Variables.AdvanceLog("Connecting emulator...", lineNumber, caller);
            ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
            if (Variables.Proc == null)
            {
                if (!ScriptRun.Run)
                {
                    return;
                }
                foreach (var p in Process.GetProcesses().Where(x => Variables.emulator.EmulatorProcessName().ToLower().Split('|').Contains(x.ProcessName.ToLower())))
                {
                    string command = GetCommandLineOfProcess(p);
                    Variables.AdvanceLog(command);
                    if (Variables.Instance.Length > 0)
                    {
                        if (command.ToLower().EndsWith(Variables.Instance.ToLower()))
                        {
                            Variables.Proc = p;
                            Variables.ScriptLog("Emulator ID: " + p.Id, Color.DarkGreen);
                            break;
                        }
                    }
                    else if (command.ToLower().EndsWith(Variables.emulator.EmulatorDefaultInstanceName().ToLower()))
                    {
                        Variables.Proc = p;
                        Variables.ScriptLog("Emulator ID: " + p.Id, Color.DarkGreen);
                        break;
                    }
                }
                Variables.AdvanceLog("Emulator not connected, retrying in 2 second...", lineNumber, caller);
                Thread.Sleep(1000);
                error++;
                if (error > 10) //We had await for 10 seconds
                {
                    Variables.ScriptLog("Unable to connect to emulator! Emulator refused to load! Restarting it now!", Color.Red);
                    RestartEmulator();
                    Thread.Sleep(10000);
                    error = 0;
                }
                if (error % 5 == 0)
                {
                    Variables.ScriptLog("Emulator is still not running...", Color.Red);
                }
                goto Connect;
            }
            else
            {
                if (Variables.Proc.HasExited)
                {
                    StartEmulator();
                }
            }
            try
            {
                AdbInstance.Instance.socket = new AdbSocket(new IPEndPoint(IPAddress.Parse("127.0.0.1"), Adb.CurrentPort));
                AdbInstance.Instance.client.Connect(new DnsEndPoint("127.0.0.1", Convert.ToInt32(Variables.AdbIpPort.Split(':').Last())));
                Variables.AdvanceLog("Connecting Adb EndPoint " + Variables.AdbIpPort, lineNumber, caller);
                do
                {
                    if (error > 10 && !Variables.DeviceChanged) //We had await for 30 second
                    {
                        Variables.ScriptLog("Unable to connect to emulator! Emulator refused to load! Restarting it now!", Color.Red);
                        RestartEmulator();
                        Thread.Sleep(10000);
                        error = 0;
                    }
                    foreach (var device in AdbInstance.Instance.client.GetDevices())
                    {
                        Variables.AdvanceLog("Detected " + device.ToString(), lineNumber, caller);
                        if (device.ToString() == Variables.AdbIpPort)
                        {
                            Variables.Controlled_Device = device;
                            Variables.DeviceChanged = true;
                            if (error % 5 == 0)
                            {
                                Variables.AdvanceLog("Device found, connection establish on " + Variables.AdbIpPort, lineNumber, caller);
                            }
                            try
                            {
                                AdbInstance.Instance.client.SetDevice(AdbInstance.Instance.socket, device);
                            }
                            catch(AdbException ex)
                            {
                                if (ex.ToString().ToLower().Contains("offline"))
                                {
                                    try
                                    {
                                        //Replace old adb
                                        var oldAdb = Directory.GetFiles(Variables.VBoxManagerPath).Where(x => x.EndsWith("adb.exe"));
                                        if (oldAdb.Count() > 0)
                                        {
                                            foreach (var adb in oldAdb)
                                            {
                                                File.Move(adb, adb.Replace(".exe", ".bak"));
                                            }
                                        }
                                    }
                                    catch
                                    {

                                    }

                                    error = 20;
                                }
                            }
                            BotCore.Delay(5000);
                            //await emulator start
                            do
                            {
                                AdbInstance.Instance.client.ExecuteRemoteCommand("getprop sys.boot_completed", (Variables.Controlled_Device as DeviceData), receiver);
                                if (receiver.ToString().Contains("1"))
                                {
                                    break;
                                }
                                else
                                {
                                    BotCore.Delay(100);
                                }
                            }
                            while (true);
                            error = 0;
                            break;
                        }
                    }
                    Thread.Sleep(2000);
                    error++;
                } while (!Variables.DeviceChanged && error < 10);
                //Ok, so now we have no device change
                Variables.DeviceChanged = false;
                //Here is to crack unbotify system, which used to scan bots

            }
            catch (Exception ex)
            {
                Variables.AdvanceLog(ex.ToString(), lineNumber, caller);
                Thread.Sleep(2000);
                error++;
                goto Connect;
            }
            
            if (Variables.Controlled_Device == null)
            {
                //Unable to connect device
                Variables.AdvanceLog("Unable to connect to device " + Variables.AdbIpPort, lineNumber, caller);
                Variables.ScriptLog("Emulator refused to connect or start, restarting...", Color.Red);
                RestartEmulator();
                return;
            }
            try
            {
                AdbInstance.Instance.client.ExecuteRemoteCommand("settings put system font_scale 1.0", (Variables.Controlled_Device as DeviceData), receiver);
                AdbInstance.Instance.client.ExecuteRemoteCommand("getprop ro.product.locale", (Variables.Controlled_Device as DeviceData), receiver);
            }
            catch(AdbException ex)
            {
                if(ex.ToString().Contains("not found"))
                {
                    StartAdb();
                    AdbInstance.Instance.client.Connect(new DnsEndPoint("127.0.0.1", Convert.ToInt32(Variables.AdbIpPort.Split(':').Last())));
                }
            }
            Variables.AdvanceLog(receiver.ToString().Trim());
            if (!receiver.ToString().Contains("zh-Hans"))
            {
                //Language is not correct;
                AdbInstance.Instance.client.ExecuteRemoteCommand("setprop ro.product.locale zh-Hans", (Variables.Controlled_Device as DeviceData), receiver);
                Variables.AdvanceLog(receiver.ToString().Trim());
            }
            ConnectMinitouch();
        }

        /// <summary>
        /// Warning!!Must run this before exit program, else all sockets records will continue in the PC even when restarted!!
        /// </summary>
        public static void EjectSockets()
        {
            try
            {
                if (AdbInstance.Instance.minitouchSocket != null)
                {
                    AdbInstance.Instance.minitouchSocket.Dispose();
                    AdbInstance.Instance.minitouchSocket = new TcpSocket();
                }
            }
            catch
            {
                AdbInstance.Instance.minitouchSocket = new TcpSocket();
            }
            ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
            var path = Path.GetTempPath() + "minitouch";
            if (File.Exists(path))
            {
                //Remove current socket port from record as we had dispose it!
                var ports = File.ReadAllLines(path);
                int x = 0;
                string[] newports = new string[ports.Length];
                foreach (var port in ports)
                {
                    if (port.Length > 0)
                    {
                        try
                        {
                            if (Convert.ToInt32(port) != AdbInstance.Instance.minitouchPort)
                            {
                                newports[x] = port;
                                x++;
                            }
                        }
                        catch
                        {
                            newports[x] = "";
                        }

                    }
                }
                File.WriteAllLines(path, newports.Where(y => !string.IsNullOrEmpty(y)).ToArray());
            }
            try
            {
                AdbInstance.Instance.client.ExecuteRemoteCommand("find /data/local/tmp/ -maxdepth 1 -type f -delete", (Variables.Controlled_Device as DeviceData), receiver);
            }
            catch
            {
                Variables.AdvanceLog("Unable to delete temp minitouch file, will try at next round!");
            }
            if (Variables.SharedPath != "")
            {
                try
                {
                    var files = Directory.GetFiles(Variables.SharedPath, "*.rgba");
                    foreach (var file in files)
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch
                        {

                        }
                    }
                }
                catch
                {

                }

            }
        }
        /// <summary>
        /// Connect minitouch
        /// </summary>
        public static void ConnectMinitouch([CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            try
            {
                if (AdbInstance.Instance.minitouchSocket != null)
                {
                    AdbInstance.Instance.minitouchSocket = null;
                }
            }
            catch
            {

            }
            Thread.Sleep(100);
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
                            if (AdbInstance.Instance.minitouchPort == Convert.ToInt32(port))
                            {
                                AdbInstance.Instance.minitouchPort++;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
                using (var stream = File.AppendText(path))
                {
                    stream.WriteLine(AdbInstance.Instance.minitouchPort); //Let other instance know that this socket is in use!
                }
            }
            catch
            {

            }
            if (!ScriptRun.Run)
            {
                return;
            }
            Variables.AdvanceLog("Connecting Minitouch", lineNumber, caller);
            string rndMiniTouch = Path.GetRandomFileName();
            ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
            try
            {
                AdbInstance.Instance.client.ExecuteRemoteCommand("find /data/local/tmp/ -maxdepth 1 -type f -delete", (Variables.Controlled_Device as DeviceData), receiver);
            }
            catch
            {
                Variables.AdvanceLog("Unable to delete temp minitouch file, will try at next round!");
            }
            BotCore.Push(Environment.CurrentDirectory + "//adb//minitouch", "/data/local/tmp/" + rndMiniTouch, 777);
            try
            {
                int error = 0;
                while (Variables.Controlled_Device == null && ScriptRun.Run)
                {
                    error++;
                    Thread.Sleep(1000);
                    foreach (var device in AdbInstance.Instance.client.GetDevices())
                    {
                        Variables.AdvanceLog(device.ToString(), lineNumber, caller);
                        if (device.ToString() == Variables.AdbIpPort)
                        {
                            Variables.Controlled_Device = device;
                            Variables.DeviceChanged = true;
                            Variables.AdvanceLog("Device found, connection establish on " + Variables.AdbIpPort, lineNumber, caller);
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
                AdbInstance.Instance.client.ExecuteRemoteCommandAsync("/data/local/tmp/" + rndMiniTouch, (Variables.Controlled_Device as DeviceData), receiver, CancellationToken.None, int.MaxValue);
                AdbInstance.Instance.client.CreateForward((Variables.Controlled_Device as DeviceData), ForwardSpec.Parse($"tcp:{AdbInstance.Instance.minitouchPort}"), ForwardSpec.Parse("localabstract:minitouch"), true);
                AdbInstance.Instance.minitouchSocket = new TcpSocket();
                AdbInstance.Instance.minitouchSocket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), AdbInstance.Instance.minitouchPort));
                if (AdbInstance.Instance.minitouchSocket.Connected)
                {
                    try
                    {
                        string cmd = "d 0 0 0 100\nc\nu 0\nc\n";
                        byte[] bytes = AdbClient.Encoding.GetBytes(cmd);
                        AdbInstance.Instance.minitouchSocket.Send(bytes, 0, bytes.Length, SocketFlags.None);
                    }
                    catch
                    {
                        Variables.AdvanceLog("Socket disconnected, retrying...", lineNumber, caller);
                        goto Cm;
                    }
                    Variables.AdvanceLog("Minitouch connected on Port number " + AdbInstance.Instance.minitouchPort, lineNumber, caller);
                }
                else
                {
                    Variables.AdvanceLog("Socket disconnected, retrying...", lineNumber, caller);
                    EjectSockets();
                    AdbInstance.Instance.minitouchPort++;
                    goto Cm;
                }
            }
            catch (Exception ex)
            {
                if (ex is AdbException)
                {
                    AdbInstance.Instance.server.RestartServer();
                    RestartEmulator();
                    AdbInstance.Instance.minitouchPort = 1111;
                    return;
                }
                Variables.AdvanceLog(ex.ToString(), lineNumber, caller);
                EjectSockets();
                AdbInstance.Instance.minitouchPort++;
                ConnectMinitouch();
            }

        }

        private static void StartAdb([CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            if (!ScriptRun.Run)
            {
                return;
            }
            string adbname = Environment.CurrentDirectory + "\\adb\\adb.exe";
            {
                if (!File.Exists(adbname))
                {
                    if (Variables.FindConfig("General", "AdbPath", out var path))
                    {
                        path = path.Remove(path.LastIndexOf('\\'));
                        IEnumerable<string> exe = Directory.EnumerateFiles(path, "*.exe");
                        foreach (var e in exe)
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
                        File.WriteAllBytes("adb.zip", AdbResource.adb);
                        ZipFile.ExtractToDirectory("adb.zip", Environment.CurrentDirectory);
                        File.Delete("adb.zip");
                    }
                }
            }
        Start:
            try
            {
                AdbInstance.Instance.server.StartServer(adbname, false);
            }
            catch
            {
                goto Start;
            }

            Variables.AdvanceLog("Adb server started", lineNumber, caller);
            return;
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
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + pid);
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
        /// Return objects of connected devices
        /// </summary>
        /// <returns></returns>
        public static object[] GetDevices(out string[] names)
        {
            List<object> devices = new List<object>();
            List<string> name = new List<string>();
            foreach (var device in AdbInstance.Instance.client.GetDevices())
            {
                devices.Add(device);
                name.Add(device.Name);
            }
            names = name.ToArray();
            return devices.ToArray();
        }
        /// <summary>
        /// Check if the android directory is exist
        /// </summary>
        /// <param name="androidpath">The path to check</param>
        /// <returns></returns>
        public static bool AndroidDirectoryExist(string androidpath)
        {
            if (BotCore.AdbCommand("ls " + androidpath + " > /dev/null 2>&1 && echo \"exists\" || echo \"not exists\"").Contains("exist"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
