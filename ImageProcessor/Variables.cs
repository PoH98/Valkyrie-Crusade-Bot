using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpAdbClient;

namespace BotFramework
{
    /// <summary>
    /// Variables that are stored in memory for future usage
    /// </summary>
    public static class Variables
    {
        /// <summary>
        /// The emulator to be used
        /// </summary>
        public static string useemulator;
        /// <summary>
        /// Adb Ip:Port
        /// </summary>
        public static string AdbIpPort;
        /// <summary>
        /// The path of screenshot saved in emulator
        /// </summary>
        public static string AndroidSharedPath;
        /// <summary>
        /// The current using EmulatorInterface for controlling emulator
        /// </summary>
        public static EmulatorInterface emulator;
        /// <summary>
        /// The emulator path that is installed at PC with outputing the emulator enum
        /// </summary>
        public static void EmulatorPath()
        {
            BotCore.LoadEmulatorInterface();
            if (emulator == null)
            {
                if (File.Exists("Updater.exe"))
                {
                    MessageBox.Show("正在自动下载模拟器...");
                    Process p = Process.Start("Updater.exe", "http://dl.memuplay.com/download/backup/Memu-Setup-3.7.0.0.exe");
                    p.WaitForExit();
                }
                MessageBox.Show("请安装完毕模拟器后再继续！");
                Environment.Exit(0);
            }
            if (Instance == "")
            {
                Instance = emulator.EmulatorName();
            }
            BotCore.profilePath = Instance;
        }
        /// <summary>
        /// The controlled device
        /// </summary>
        public static Object Controlled_Device = null;
        /// <summary>
        /// Confiures of bot.ini, use BotCore.ReadConfig() to fill up values
        /// </summary>
        public static Dictionary<string, string> Configure = new Dictionary<string, string>();
        /// <summary>
        /// If new devices added, will return true
        /// </summary>
        public static bool DeviceChanged = false;
        /// <summary>
        /// Available Adb Capture in Fast Capture if background is true else use WinAPI for capturing (Make Image not usable in RGBComparer and GetPixel)
        /// </summary>
        public static bool Background;
        /// <summary>
        /// The process attached, which is the emulator
        /// </summary>
        public static Process Proc;
        /// <summary>
        /// Virtual box path
        /// </summary>
        public static string VBoxManagerPath;
        /// <summary>
        /// The download path that shared for image capturing
        /// </summary>
        public static string SharedPath;
        /// <summary>
        /// The instance name of emulator to multi-bot
        /// </summary>
        public static string Instance = "";
        /// <summary>
        /// The richtextbox for showing logs
        /// </summary>
        public static RichTextBox richTextBox;
        /// <summary>
        /// Shows more log or only script log?
        /// </summary>
        public static bool AdvanceLogShow = false;
        /// <summary>
        /// The advanced log which might not show, except enabled for debuging...
        /// </summary>
        /// <param name="log"></param>
        /// <param name="lineNumber"></param>
        /// <param name="caller"></param>
        public static void AdvanceLog(string log, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            if(richTextBox != null)
            {
                try
                {
                    if (AdvanceLogShow)
                    {
                        richTextBox.Invoke((MethodInvoker)delegate
                        {
                            richTextBox.SelectionColor = Color.Red;
                            richTextBox.AppendText("[" + DateTime.Now.ToLongTimeString() + "]:" + log + "\n");
                        });
                    }
                    Debug_.WriteLine(log,lineNumber,caller);
                }
                catch
                {

                }
            }
        }
        /// <summary>
        /// Script log for showing
        /// </summary>
        /// <param name="log">The log</param>
        /// <param name="color">The color of log</param>
        /// <param name="lineNumber"></param>
        /// <param name="caller"></param>
        public static void ScriptLog(string log, Color color, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            if(richTextBox != null)
            {
                try
                {
                    richTextBox.Invoke((MethodInvoker)delegate
                    {
                        richTextBox.SelectionColor = color;
                        richTextBox.AppendText("[" + DateTime.Now.ToLongTimeString() + "]:" + log + "\n");
                    });
                    Debug_.WriteLine("ScriptLog: " + log, lineNumber, caller);
                }
                catch
                {

                }
            }
            
        }
        /// <summary>
        /// Script log for showing
        /// </summary>
        /// <param name="log">The log</param>
        /// <param name="lineNumber"></param>
        /// <param name="caller"></param>
        public static void ScriptLog(string log, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            ScriptLog(log, Color.Black, lineNumber, caller);
        }
        /// <summary>
        /// Set if screencapture need to use pull function
        /// </summary>
        public static bool NeedPull = false;
    }
}
