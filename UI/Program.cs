using ImageProcessor;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UI
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainScreen());
        }
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if(e.ExceptionObject is SharpAdbClient.Exceptions.AdbException)
            {
                try
                {
                    Variables.Proc.Kill();
                    Variables.Controlled_Device = null;
                    Variables.ScriptLog.Add("Device exited, restart now!");
                }
                catch
                {

                }
                return;
            }
            if (EmulatorController.handle != null && Variables.Proc != null)
            {
                DllImport.SetParent(EmulatorController.handle, IntPtr.Zero);
                DllImport.MoveWindow(EmulatorController.handle, PrivateVariable.EmuDefaultLocation.X, PrivateVariable.EmuDefaultLocation.Y, 1280, 720, true);
            }
            Debug_.WriteLine(e.ExceptionObject.ToString());
            File.WriteAllText("error.log", e.ExceptionObject.ToString());
            MessageBox.Show(e.ExceptionObject.ToString());
            Environment.Exit(0);
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            if (e.Exception is SharpAdbClient.Exceptions.AdbException)
            {
                try
                {
                    Variables.Proc.Kill();
                    Variables.Controlled_Device = null;
                    Variables.ScriptLog.Add("Device exited, restart now!");
                }
                catch
                {

                }
                return;
            }
            if (EmulatorController.handle != null && Variables.Proc != null)
            {
                DllImport.SetParent(EmulatorController.handle, IntPtr.Zero);
                DllImport.MoveWindow(EmulatorController.handle, PrivateVariable.EmuDefaultLocation.X, PrivateVariable.EmuDefaultLocation.Y, 1280, 720, true);
            }
            Debug_.WriteLine(e.Exception + " At: "+e.Exception.Source +" At: "+ e.Exception.TargetSite);
            File.WriteAllText("error.log", e.Exception.ToString());
            MessageBox.Show(e.Exception.Message.ToString());
            Environment.Exit(0);
        }

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
                    bool result;
                    if (DllImport.IsWow64Process(DllImport.GetCurrentProcess(), out result) && result)
                        return true;
                }
            }
            // The environment must be an x86 environment.
            return false;
        }
    }
}
