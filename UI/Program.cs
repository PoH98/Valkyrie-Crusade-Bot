using BotFramework;
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
            if (BotCore.handle != null && Variables.Proc != null)
            {
                DllImport.SetParent(BotCore.handle, IntPtr.Zero);
                DllImport.MoveWindow(BotCore.handle, PrivateVariable.EmuDefaultLocation.X, PrivateVariable.EmuDefaultLocation.Y, 1318, 752, true);
            }
            Debug_.WriteLine(e.ExceptionObject.ToString());
            File.WriteAllText("error.log", e.ExceptionObject.ToString());
            MessageBox.Show(e.ExceptionObject.ToString());
            Environment.Exit(0);
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            if (BotCore.handle != null && Variables.Proc != null)
            {
                DllImport.SetParent(BotCore.handle, IntPtr.Zero);
                DllImport.MoveWindow(BotCore.handle, PrivateVariable.EmuDefaultLocation.X, PrivateVariable.EmuDefaultLocation.Y, 1318, 752, true);
            }
            Debug_.WriteLine(e.Exception + " At: "+e.Exception.Source +" At: "+ e.Exception.TargetSite);
            File.WriteAllText("error.log", e.Exception.ToString());
            MessageBox.Show(e.Exception.Message.ToString());
            Environment.Exit(0);
        }
    }
}
