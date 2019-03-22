using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpAdbClient;

namespace ImageProcessor
{
    /// <summary>
    /// Variables that are stored in memory for future usage
    /// </summary>
    public static class Variables
    {
        /// <summary>
        /// Adb Ip:Port
        /// </summary>
        public static string AdbIpPort;
        /// <summary>
        /// The emulator path that is installed at PC with outputing the emulator enum
        /// </summary>
        public static string EmulatorPath(out EmulatorController.Emulators emu)
        {
            string path = "";
            if (EmulatorsInstallationPath.MEmu(out path))
            {
                emu = EmulatorController.Emulators.MEmu;
                return path + "\\MEmu\\MEmu.exe";
            }
            if (EmulatorsInstallationPath.Bluestack(out path))
            {
                emu = EmulatorController.Emulators.Bluestack;
                return path + "";
            }
            if (EmulatorsInstallationPath.Nox(out path))
            {
                emu = EmulatorController.Emulators.Nox;
                return path + "";
            }
            emu = EmulatorController.Emulators.Null;
            return null;
        }

        public static DeviceData Controlled_Device = null;
        /// <summary>
        /// Confiures of bot.ini, use EmulatorController.ReadConfig() to fill up values
        /// </summary>
        public static Dictionary<string, string> Configure = new Dictionary<string, string>();
        /// <summary>
        /// If new devices added, will return true
        /// </summary>
        public static bool DeviceChanged = false;
        /// <summary>
        /// The thread for starting scripts
        /// </summary>
        public static Thread start;
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

        public static RichTextBox richTextBox;

        public static bool AdbLogShow = false;
        public static void AdbLog(string log, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            if (AdbLogShow)
            {
                richTextBox.Invoke((MethodInvoker)delegate
                {
                    richTextBox.SelectionColor = Color.Red;
                    richTextBox.AppendText("\n[" + DateTime.Now + "]:" + log);
                });
            }
        }
        
        public static void ScriptLog(string log, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            richTextBox.Invoke((MethodInvoker)delegate
            {
                richTextBox.SelectionColor = Color.Lime;
                richTextBox.AppendText("\n[" + DateTime.Now + "]:" + log);
            });
                Debug_.WriteLine("ScriptLog: "+log, lineNumber, caller);
            
        }
    }
}
