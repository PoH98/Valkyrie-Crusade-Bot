﻿namespace BotFramework
{
    /// <summary>
    /// Emulator controlling interface for different emulators
    /// </summary>
    public interface EmulatorInterface
    {
        /// <summary>
        /// Load emulator settings here, such as adb port, shared path and etc
        /// </summary>
        bool LoadEmulatorSettings();
        /// <summary>
        /// Start emulator function here
        /// </summary>
        void StartEmulator();
        /// <summary>
        /// The default emulator instance name.
        /// </summary>
        /// <returns>The emulator default instance</returns>
        string EmulatorDefaultInstanceName();
        /// <summary>
        /// The name of emulator
        /// </summary>
        /// <returns>return as string</returns>
        string EmulatorName();
        /// <summary>
        /// Set Resolution of the emulator
        /// </summary>
        /// <param name="x">x value</param>
        /// <param name="y">y value</param>
        /// <param name="dpi">dpi of emulator</param>
        void SetResolution(int x, int y, int dpi);
        /// <summary>
        /// The process name used to Process.GetProcessByName
        /// </summary>
        /// <returns></returns>
        string EmulatorProcessName();
        /// <summary>
        /// Un Unbotify system
        /// </summary>
        void UnUnBotify();
    }
}
