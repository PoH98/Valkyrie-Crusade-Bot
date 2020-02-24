using SharpAdbClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotFramework
{
    /// <summary>
    /// The adb data
    /// </summary>
    public class AdbInstance
    {
        /// <summary>
        /// The path to bot.ini
        /// </summary>
        public string profilePath = Variables.Instance;
        /// <summary>
        /// Adb Server
        /// </summary>
        public readonly AdbServer server = new AdbServer();
        /// <summary>
        /// Image path for screenshot
        /// </summary>
        public string pcimagepath = "", androidimagepath = "";
        /// <summary>
        /// The client side adb
        /// </summary>
        public readonly AdbClient client = new AdbClient();
        /// <summary>
        /// Adb socket
        /// </summary>
        public IAdbSocket socket;
        /// <summary>
        /// Check if emulator is starting
        /// </summary>
        public bool StartingEmulator; //To confirm only start the emulator once in the same time!
        /// <summary>
        /// Minitouch port number
        /// </summary>
        public int minitouchPort = 1111;
        /// <summary>
        /// minitouch Tcp socket. Default null, use connectEmulator to gain a socket connection!
        /// </summary>
        public TcpSocket minitouchSocket;
        /// <summary>
        /// The name of apk
        /// </summary>
        public string GameName;
        /// <summary>
        /// Get the singleton data variables
        /// </summary>
        public static AdbInstance Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new AdbInstance();
                }
                return instance;
            }
        }

        private static AdbInstance instance;
    }
}
