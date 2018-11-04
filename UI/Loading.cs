using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UI
{
    public partial class Loading : Form
    {
        public static bool LoadCompleted = false;
        int num = 0;
        string[] text = {"正在读取设置...请稍后...", "正在导入dll...请稍后...", "正在尝试连接服务器...请稍后..." };
        public Loading()
        {
            InitializeComponent();
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(num < 3)
            {
                label1.Text = text[num];
                num++;
            }
            if (LoadCompleted)
            {
                Close();
            }
        }
    }
}
