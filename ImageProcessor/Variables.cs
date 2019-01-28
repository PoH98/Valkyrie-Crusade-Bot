using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharpAdbClient;

namespace ImageProcessor
{
    /// <summary>
    /// Variables that are stored in memory for future usage
    /// </summary>
    public static class Variables
    {
        /// <summary>
        /// 
        /// </summary>
        public static string AdbIpPort;

        public static DeviceData Controlled_Device = null;
        /// <summary>
        /// Confiures of bot.ini, use EmulatorController.ReadConfig() to fill up values
        /// </summary>
        public static Dictionary<string, string> Configure = new Dictionary<string, string>();
        /// <summary>
        /// Adb Logs that can be showed, remember to clear this frequently to prevent overload
        /// </summary>
        public static List<string> AdbLog = new List<string>();
        /// <summary>
        /// Script Logs that can be showed, remember to clear this frequently to prevent overload
        /// </summary>
        public static List<string> ScriptLog = new List<string>();
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

    }
}
