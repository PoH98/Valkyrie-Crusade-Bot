﻿using System;
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
    public class Variables
    {   /// <summary>
        /// A list of devices that are connected to the computer. Retreive by using EmulatorController.StartAdb()
        /// </summary>
        public static List<DeviceData> Devices_Connected = new List<DeviceData>();
        /// <summary>
        /// Select which device that is need to control. We can't control thousands of them in a same time!
        /// </summary>
        public static int Control_Device_Num = 0, Selected_Process_Num;
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
        /// The emulator's shared pictures path
        /// </summary>
        public static string SharedPicturePath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "\\MEmu Photo\\";

    }
}
