using System;
using System.Drawing;

namespace BotFramework
{
    /// <summary>
    /// Class for controlling PC
    /// </summary>
    public class PCController
    {
        /// <summary>
        /// Send a mouse click to location
        /// </summary>
        /// <param name="location"></param>
        public static void DoMouseClick(Point location)
        {
            //Call the imported function with the cursor's current position
            DllImport.mouse_event(DllImport.MOUSEEVENTF_LEFTDOWN | DllImport.MOUSEEVENTF_LEFTUP, location.X, location.Y, 0, 0);
        }
        /// <summary>
        /// Send a mouse click to location
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void DoMouseClick(int x, int y)
        {
            //Call the imported function with the cursor's current position
            DllImport.mouse_event(DllImport.MOUSEEVENTF_LEFTDOWN | DllImport.MOUSEEVENTF_LEFTUP, x, y, 0, 0);
        }
    }
}
