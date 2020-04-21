using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using IniParser;
using IniParser.Model;
using IniParser.Parser;

namespace BotFramework
{
    /// <summary>
    /// Variables that are stored in memory for future usage
    /// </summary>
    public class Variables
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
        /// Save all image capturing files, use this carefully!
        /// </summary>
        public static bool ImageDebug = false;
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
            EmulatorLoader.LoadEmulatorInterface();
            if (emulator == null)
            {
                throw new FileNotFoundException("No supported emulators detected!");
            }
            if (Instance == "")
            {
                Instance = emulator.EmulatorName();
            }
            AdbInstance.Instance.profilePath = Instance;
        }
        /// <summary>
        /// The controlled device
        /// </summary>
        public static Object Controlled_Device = null;
        /// <summary>
        /// Confiures of bot.ini, use BotCore.ReadConfig() to fill up values
        /// </summary>
        private static IniData Config = new IniData();
        /// <summary>
        /// Get config data by key
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="result">Value fetched</param>
        /// <param name="section">Section name</param>
        /// <returns>true if key and value found, else false</returns>
        public static bool FindConfig(string section, string key, out string result)
        {
            if (Config.Sections.ContainsSection(section))
            {
                if (Config[section].ContainsKey(key))
                {
                    result = Config[section][key];
                    return true;
                }
            }
            result = null;
            return false;
        }
        /// <summary>
        /// Read configures from bot.ini or create new if not exist
        /// </summary>
        public static void ReadConfig()
        {
            string path = "Profiles\\" + new string(emulator.EmulatorName().Where(char.IsLetter).ToArray()) + "\\bot.ini";
            if(!Directory.Exists("Profiles\\" + new string(emulator.EmulatorName().Where(char.IsLetter).ToArray())))
            {
                Directory.CreateDirectory("Profiles\\" + new string(emulator.EmulatorName().Where(char.IsLetter).ToArray()));
            }
            if (File.Exists(path))
            {
                FileIniDataParser p = new FileIniDataParser();
                Config = p.ReadFile(path,Encoding.Unicode);
            }
            else
            {
                File.WriteAllText(path,"[General]");
            }
        }
        /// <summary>
        /// Modify the key and value from section
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void ModifyConfig(string section, string key, string value)
        {
            if (Config.Sections.ContainsSection(section))
            {
                if (Config[section].ContainsKey(key))
                {
                    Config[section][key] = value;
                }
                else
                {
                    Config[section].AddKey(key);
                    Config[section][key] = value;
                }
            }
            else
            {
                Config.Sections.AddSection(section);
                Config[section].AddKey(key);
                Config[section][key] = value;
            }
            SaveConfig();
        }
        /// <summary>
        /// Save Config to file. Normaly will automatic execute when ModifyConfig called
        /// </summary>
        public static void SaveConfig()
        {
            string path = "Profiles\\" + new string(emulator.EmulatorName().Where(char.IsLetter).ToArray()) + "\\bot.ini";
            FileIniDataParser p = new FileIniDataParser();
            Config.Configuration.AssigmentSpacer = "";
            p.WriteFile(path, Config, Encoding.Unicode);
        }
        /// <summary>
        /// If new devices added, will return true
        /// </summary>
        public static bool DeviceChanged = false;
        /// <summary>
        /// The process attached, which is the emulator
        /// </summary>
        public static Process Proc;
        /// <summary>
        /// hWnd used to WinApi screenshot
        /// </summary>
        public static IntPtr ProchWnd;
        /// <summary>
        /// Use WinApi capture or adb capture?
        /// </summary>
        public static bool WinApiCapt = false, ForceWinApiCapt = false;
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
        /// <summary>
        /// Emulator's Width and Height will be used in ImageCapture()
        /// </summary>
        public static int EmulatorWidth = 1280, EmulatorHeight = 720, EmulatorDpi = 160;
        /// <summary>
        /// WinApi capture cropping as it might captured some garbage inside
        /// </summary>
        public static Point WinApiCaptCropStart = new Point(0, 0), WinApiCaptCropEnd = new Point(EmulatorWidth, EmulatorHeight);
        /// <summary>
        /// Some emulator might contains larger resolution which need to set the multiplier
        /// </summary>
        public static decimal ClickPointMultiply = 1;
    }
}
