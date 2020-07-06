using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotFramework
{
    public interface IEmulator
    {
        /// <summary>
        /// Open the emulator method
        /// </summary>
        /// <param name="Restart">If emulator is restart</param>
        /// <returns>Is the emulator start success</returns>
        bool OpenEmulator(bool Restart);
        /// <summary>
        /// Check if the process's command line is it the emulator we need
        /// </summary>
        /// <param name="commandLine">The command line input from bot framework. You don't have to do it yourself</param>
        /// <returns></returns>
        bool IsEmulatorCommandLine(string commandLine);
        /// <summary>
        /// Getting the handler of Android
        /// </summary>
        /// <returns></returns>
        IntPtr WinGetAndroidHandle();
        /// <summary>
        /// Getting the resolution and bla bla bla of emulator
        /// </summary>
        /// <returns></returns>
        void GetEmulatorParameter();
        /// <summary>
        /// Getting Emulator's Path
        /// </summary>
        /// <returns></returns>
        string GetEmulatorPath();
        /// <summary>
        /// Checking the emulator is installed
        /// </summary>
        /// <returns></returns>
        bool InitEmulator();
        /// <summary>
        /// Setting up the shared folder
        /// </summary>
        /// <returns></returns>
        bool ConfigureSharedFolder();
        /// <summary>
        /// The function used to change the resolution when the resolution is incorrect
        /// </summary>
        void SetResolution();
        /// <summary>
        /// The function used to close the emulator
        /// </summary>
        void CloseEmulator();
        /// <summary>
        /// Used to get the emulator process
        /// </summary>
        /// <returns></returns>
        Process GetEmulatorRunningInstance();
        /// <summary>
        /// Used to hide the stupid toolbars or something whatever you like
        /// </summary>
        void EmbedEmulator();
    }
}
