using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                return t.ThreadState == ThreadState.Running;
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
                    script.Script();
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
    }
}
