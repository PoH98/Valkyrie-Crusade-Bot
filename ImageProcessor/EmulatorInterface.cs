using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace BotFramework
{
    public interface EmulatorInterface
    {
        /// <summary>
        /// Load emulator settings here, such as adb port, shared path...
        /// </summary>
        bool LoadEmulatorSettings();
        /// <summary>
        /// Start emulator function here
        /// </summary>
        void StartEmulator();
        /// <summary>
        /// Close emulator function here
        /// </summary>
        void CloseEmulator();
        /// <summary>
        /// The name of emulator
        /// </summary>
        /// <returns>return as string</returns>
        string EmulatorName();

        void SetResolution(int x, int y);

        void ConnectEmulator();
    }
}
