using ImageProcessor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UI
{
    public partial class Suprise : Form
    {
        private Random rnd = new Random();
        private const int HWND_TOPMOST = -1;
        private const int SWP_NOMOVE = 0x0002;
        private const int SWP_NOSIZE = 0x0001;
        public Suprise()
        {
            InitializeComponent();
            timer1.Start();
            HideTask();
        }
        [DllImport("user32.dll")]
        private static extern int FindWindow(string className, string windowText);
        [DllImport("user32.dll")]
        private static extern int ShowWindow(int hwnd, int command);

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 1;

        protected static int ShellHandle
        {
            get
            {
                return FindWindow("Shell_TrayWnd", "");
            }
        }

        public static void ShowTask()
        {
            ShowWindow(ShellHandle, SW_SHOW);
        }

        public static void HideTask()
        {
            ShowWindow(ShellHandle, SW_HIDE);
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            Color randomColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
            BackColor = randomColor;
            label1.Location = new Point((Width / 2) - (label1.Width/2), Height / 2);
        }

        private void Suprise_KeyDown(object sender, KeyEventArgs e)
        {
            DllImport.SetWindowPos(Handle,new IntPtr(HWND_TOPMOST), 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
            e.Handled = true;
        }
    }
}
