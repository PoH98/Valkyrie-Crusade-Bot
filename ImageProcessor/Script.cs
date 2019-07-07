using SharpAdbClient;
using SharpAdbClient.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Permissions;
using System.Threading;

namespace BotFramework
{
    /// <summary>
    /// The script to be called in ScriptRun.RunScript()
    /// </summary>
    public interface ScriptInterface
    {
        /// <summary>
        /// Main scripting goes here!
        /// </summary>
        void Script();
    }
    /// <summary>
    /// Start ScriptInterface's script
    /// </summary>
    public class ScriptRun
    {
        /// <summary>
        /// Set whether the scipt run or stop
        /// </summary>
        public static bool Run { get; private set; }
        private static Thread t;
        private static List<ScriptInterface> scripts = new List<ScriptInterface>();
        private static List<string> scriptsname = new List<string>();
        /// <summary>
        /// Read all dlls or exe file that contains scripts inside dllpath
        /// </summary>
        /// <param name="dllPath">The path to be readed</param>
        public static void ReadScript(string dllPath)
        {
            DirectoryInfo dinfo = new DirectoryInfo(dllPath);
            FileInfo[] dlls = new string[] { "*.dll", "*.exe" }.SelectMany(i => dinfo.GetFiles(i)).ToArray();
            if (dlls != null)
            {
                foreach (var dll in dlls)
                {
                    try
                    {
                        Assembly a = Assembly.LoadFrom(dll.FullName);
                        foreach (var t in a.GetTypes())
                        {
                            if (t.GetInterface("ScriptInterface") != null)
                            {
                                scripts.Add(Activator.CreateInstance(t) as ScriptInterface);
                                scriptsname.Add(t.Name);
                            }
                        }
                    }
                    catch (BadImageFormatException)
                    {

                    }
                    catch (ReflectionTypeLoadException)
                    {

                    }
                }
            }
        }
        /// <summary>
        /// Run the script!
        /// </summary>
        /// <param name="KeepRunning">Keep running or run once?</param>
        /// <param name="script">The script for running</param>
        public static void RunScript(bool KeepRunning, ScriptInterface script)
        {
            if(!Run)
            {
                t = new Thread(delegate () { _RunScript(KeepRunning, script); });
                t.IsBackground = true;
                Run = true;
                t.Start();
            }
            else
            {
                Variables.AdvanceLog("Stop first before run again!");
            }
        }
        /// <summary>
        /// Run the script!
        /// </summary>
        /// <param name="KeepRunning">Keep running or run once?</param>
        /// <param name="scriptname">The name of the class of the script that extends scriptinterface, if already loaded using ReadScript</param>
        public static void RunScript(bool KeepRunning, string scriptname)
        {
            int index = scriptsname.IndexOf(scriptname);
            ScriptInterface script = scripts[index];
            RunScript(KeepRunning, script);
        }
        /// <summary>
        /// Check the script is running
        /// </summary>
        /// <returns></returns>
        public static bool ScriptRunning()
        {
            try
            {
                return t.ThreadState == System.Threading.ThreadState.Running;
            }
            catch
            {
                return false;
            }
        }
        private static void _RunScript(bool KeepRunning, ScriptInterface script)
        {
            if (script != null)
            {
                do
                {
                    try
                    {
                        script.Script();
                    }
                    catch (Exception ex)
                    {
                        if(ex is SocketException || ex is DeviceNotFoundException || ex is AdbException)
                        {
                            BotCore.server.RestartServer();
                            if (!CheckDeviceOnline())
                            {
                                BotCore.RestartEmulator();
                            }
                            BotCore.Delay(10000);
                            BotCore.ConnectAndroidEmulator();
                            continue;
                        }
                        else if (ex is ThreadAbortException) //Bot stopped
                        {

                        }
                        else
                        {
                            ThrowException(ex);
                            continue;
                        }
                    }
                }
                while (KeepRunning && Run);
            }
            else
            {
                throw new Exception("No script dll found!");
            }
            Run = false;
        }
        /// <summary>
        /// Stop the script! Now!
        /// </summary>
        [SecurityPermission(SecurityAction.Demand, ControlThread = true)]
        public static void StopScript()
        {
            try
            {
                Run = false;
                t.Abort();
                t = null;
            }
            catch
            {

            }
        }
        /// <summary>
        /// Another version for throwing exceptions, without getting freeze UI and exit program, but starts a webpage to tell the exception and continue our script!
        /// </summary>
        /// <param name="ex"></param>
        public static void ThrowException(Exception ex)
        {
            string html = $"<head><title>Ops! Error Found! {ex.HResult}</title><link rel=\"stylesheet\" type=\"text/css\" href=\"style.css\"></head><body><section id=\"not-found\"><div id=\"title\">Oh no! Bot Error Found!</div><div class=\"circles\"><p>{ex.Message}<br><small>{ex.ToString().Replace("\n", "<br>")}</small></p><span class=\"circle big\"></span>" +
                            "<span class=\"circle med\"></span><span class=\"circle small\"></span></div></section></body>";
            if (!Directory.Exists("Error"))
            {
                Directory.CreateDirectory("Error");
            }
            if (!File.Exists("Error\\style.css"))
            {
                File.WriteAllText("Error\\style.css", AdbResource.css);
            }
            string filename = string.Format(@"{0}.html", Encryption.SHA256(Guid.NewGuid()));
            File.WriteAllText("Error\\" + filename, html);
            Process.Start("Error\\" + filename);
        }
        private static bool CheckDeviceOnline()
        {
            if(Variables.Proc != null)
            {
                if (Variables.Proc.HasExited || !Variables.Proc.Responding)
                {
                    return false;
                }
                else
                {
                    if (Variables.Controlled_Device != null)
                    {
                        if((Variables.Controlled_Device as DeviceData).State == DeviceState.Online)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
        }
    }
}
