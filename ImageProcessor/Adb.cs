using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using SharpAdbClient;
using SharpAdbClient.Exceptions;

namespace BotFramework
{
    /// <summary>
    /// Adb controller
    /// </summary>
    class Adb
    {
        /// <summary>
        /// Default readonly port
        /// </summary>
        public static readonly int CurrentPort = 5037;
        /// <summary>
        /// Return true or false to represent Adb is started successful or not
        /// </summary>
        /// <param name="adbPath"></param>
        /// <returns></returns>
        public static bool StartServer(string adbPath)
        {
            ProcessStartInfo adb = new ProcessStartInfo(adbPath)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true
            };
            Process.Start(adb);
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnections = ipProperties.GetActiveTcpConnections();
            foreach (TcpConnectionInformation info in tcpConnections)
            {
                if (info.LocalEndPoint.Address == IPAddress.Loopback && info.LocalEndPoint.Port >= 5037 && info.LocalEndPoint.Port <= 5040 && info.State == TcpState.Listen && info.LocalEndPoint.Port == CurrentPort)
                {
                    Variables.AdvanceLog("Adb port listening on " + info.LocalEndPoint.Port);
                    AdbServer server = new AdbServer();
                    server.StartServer(adbPath, false);
                    return true;
                }
            }
            return false;
        }
    }
}
