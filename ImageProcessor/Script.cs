using System;
using System.IO;
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
        private static Thread t;
        /// <summary>
        /// Run the script!
        /// </summary>
        /// <param name="KeepRunning">Keep running or run once?</param>
        /// <param name="dllPath">The path to the script interface dll. Do not include the Dll's name! Only PATH!!</param>
        public static void RunScript(bool KeepRunning, string dllPath)
        {
            t.IsBackground = true;
            t = new Thread(delegate () { _RunScript(KeepRunning,dllPath); });
            t.Start();
        }

        private static void _RunScript(bool KeepRunning, string dllPath)
        {
            ScriptInterface script = null;
            var dlls = Directory.GetFiles(dllPath);
            if (dlls != null)
            {
                foreach (var dll in dlls)
                {
                    Assembly a = Assembly.LoadFrom(dll);
                    foreach (var t in a.GetTypes())
                    {
                        if (t.GetInterface("ScriptInterface") != null)
                        {
                            script = Activator.CreateInstance(t) as ScriptInterface;
                            break;
                        }
                    }
                    if(script != null)
                    {
                        break;
                    }
                }
            }
            if (script != null)
            {
                while (KeepRunning)
                {
                    script.Script();
                }
            }
            else
            {
                throw new Exception("No script dll found!");
            }
        }
        /// <summary>
        /// Stop the script! Now!
        /// </summary>
        [SecurityPermission(SecurityAction.Demand, ControlThread = true)]
        public static void StopScript()
        {
            try
            {
                while (t.IsAlive)
                {
                    t.Abort();
                }
            }
            catch
            {

            }
        }
    }
}
