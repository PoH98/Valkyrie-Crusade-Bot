using System;
using System.Drawing;

namespace BotFramework
{
    public class PCController
    {
        public static void DoMouseClick(Point location)
        {
            //Call the imported function with the cursor's current position
            DllImport.mouse_event(DllImport.MOUSEEVENTF_LEFTDOWN | DllImport.MOUSEEVENTF_LEFTUP, location.X, location.Y, 0, 0);
        }

        public static void DoMouseClick(int x, int y)
        {
            //Call the imported function with the cursor's current position
            DllImport.mouse_event(DllImport.MOUSEEVENTF_LEFTDOWN | DllImport.MOUSEEVENTF_LEFTUP, x, y, 0, 0);
        }
    }
}
