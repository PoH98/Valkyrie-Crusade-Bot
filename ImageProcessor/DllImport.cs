using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Linq;

namespace BotFramework
{
    /// <summary>
    /// C# dll imports
    /// </summary>
    public static class DllImport
    {
        /// <summary>
        /// Set User agent for whole process of accessing internet
        /// </summary>
        /// <param name="dwOption">Use URLMON_OPTION_USERAGENT</param>
        /// <param name="pBuffer"></param>
        /// <param name="dwBufferLength"></param>
        /// <param name="dwReserved"></param>
        /// <returns></returns>
        [DllImport("urlmon.dll", CharSet = CharSet.Ansi)]
        
        public static extern int UrlMkSetSessionOption( int dwOption, string pBuffer, int dwBufferLength, int dwReserved);
        /// <summary>
        /// Set user agent
        /// </summary>
        public const int URLMON_OPTION_USERAGENT = 0x10000001;
        /// <summary>
        /// Set other IntPtr's parent to new IntPtr
        /// </summary>
        /// <param name="hwndChild"></param>
        /// <param name="hwndNewParent"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern IntPtr SetParent(IntPtr hwndChild, IntPtr hwndNewParent);
        /// <summary>
        /// Move specific window to new location
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="nWidth"></param>
        /// <param name="nHeight"></param>
        /// <param name="bRepaint"></param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool MoveWindow(IntPtr hwnd, int x, int y, int nWidth, int nHeight, bool bRepaint);
        /// <summary>
        /// Return IntPtr of specific window, using class name and window text
        /// </summary>
        /// <param name="className"></param>
        /// <param name="WindowText"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string className, string WindowText);
        private delegate bool EnumWindowProc(IntPtr hwnd, IntPtr lParam);
        /// <summary>
        /// Return IntPtr of specific window, using class name and window text, with parent handler
        /// </summary>
        /// <param name="parentHandle"></param>
        /// <param name="childAfter"></param>
        /// <param name="lclassName"></param>
        /// <param name="windowTitle"></param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string lclassName, string windowTitle);
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr lParam);
        public static List<IntPtr> GetAllChildHandles(IntPtr mainHandle)
        {
            List<IntPtr> childHandles = new List<IntPtr>();

            GCHandle gcChildhandlesList = GCHandle.Alloc(childHandles);
            IntPtr pointerChildHandlesList = GCHandle.ToIntPtr(gcChildhandlesList);

            try
            {
                EnumWindowProc childProc = new EnumWindowProc(EnumWindow);
                EnumChildWindows(mainHandle, childProc, pointerChildHandlesList);
            }
            finally
            {
                gcChildhandlesList.Free();
            }

            return childHandles;
        }

        [DllImport("user32.dll", PreserveSig = false)]
        public static extern bool RegisterHotKey(IntPtr hWnd,int id,uint fsModifiers,Keys key);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        public static extern bool SendMessage(IntPtr hWnd, uint Msg, int wParam, StringBuilder lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SendMessage(int hWnd, int Msg, int wparam, int lparam);

        const int WM_GETTEXT = 0x000D;
        const int WM_GETTEXTLENGTH = 0x000E;

        public static string GetControlText(IntPtr hWnd)
        {

            // Get the size of the string required to hold the window title (including trailing null.) 
            Int32 titleSize = SendMessage((int)hWnd, WM_GETTEXTLENGTH, 0, 0).ToInt32();

            // If titleSize is 0, there is no title so return an empty string (or null)
            if (titleSize == 0)
                return String.Empty;

            StringBuilder title = new StringBuilder(titleSize + 1);

            SendMessage(hWnd, (int)WM_GETTEXT, title.Capacity, title);

            return title.ToString();
        }
        private static bool EnumWindow(IntPtr hWnd, IntPtr lParam)
        {
            GCHandle gcChildhandlesList = GCHandle.FromIntPtr(lParam);

            if (gcChildhandlesList == null || gcChildhandlesList.Target == null)
            {
                return false;
            }

            List<IntPtr> childHandles = gcChildhandlesList.Target as List<IntPtr>;
            childHandles.Add(hWnd);

            return true;
        }
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOZORDER = 0x0004;

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rectangle rect);
        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        public static List<IntPtr> GetAllChildrenWindowHandles(IntPtr hParent, string Class, string name, int maxCount)
        {
            List<IntPtr> result = new List<IntPtr>();
            int ct = 0;
            IntPtr prevChild = IntPtr.Zero;
            IntPtr currChild = IntPtr.Zero;
            while (true && ct < maxCount)
            {
                currChild = FindWindowEx(hParent, prevChild, Class, name);
                if (currChild == IntPtr.Zero) break;
                result.Add(currChild);
                prevChild = currChild;
                ++ct;
            }
            return result;
        }

        [DllImport("wininet.dll", SetLastError = true)]
        public static extern long DeleteUrlCacheEntry(string lpszUrlName);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string moduleName);

        [DllImport("kernel32")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll")]
        public static extern bool IsWow64Process(IntPtr hProcess, out bool wow64Process);

        [DllImport("user32.dll", EntryPoint = "GetParent", CharSet = CharSet.Auto)]
        public static extern IntPtr GetParent(IntPtr hWnd);

        public const int WM_NCLBUTTONDOWN = 0xA1;

        public const int HT_CAPTION = 0x2;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindow(IntPtr hWnd);

        public static IEnumerable<Control> GetAll(Control control, Type type = null)
        {
            var controls = control.Controls.Cast<Control>();
            //check the all value, if true then get all the controls
            //otherwise get the controls of the specified type
            if (type == null)
                return controls.SelectMany(ctrl => GetAll(ctrl, type)).Concat(controls);
            else
                return controls.SelectMany(ctrl => GetAll(ctrl, type)).Concat(controls).Where(c => c.GetType() == type);
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }
        [DllImport("user32.dll")]
        static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);
        public static TimeSpan GetInactiveTime()
        {
            LASTINPUTINFO info = new LASTINPUTINFO();
            info.cbSize = (uint)Marshal.SizeOf(info);
            if (GetLastInputInfo(ref info))
                return TimeSpan.FromMilliseconds(Environment.TickCount - info.dwTime);
            else
                return TimeSpan.FromMilliseconds(0);
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, int dx, int dy, uint cButtons, uint dwExtraInfo);
        [DllImport("user32")]
        public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32")]
        public static extern bool EnableMenuItem(IntPtr hMenu, uint itemId, uint uEnable);
        //Mouse actions
        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;
        public const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        public const int MOUSEEVENTF_RIGHTUP = 0x10;
        public const int MOUSEEVENTF_MOVE = 0x0001;
        [DllImport("user32.dll")]
        public static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int nFlags);

    }
}
