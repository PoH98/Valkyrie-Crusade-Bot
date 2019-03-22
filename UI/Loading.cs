using System;
using MetroFramework.Forms;

namespace UI
{
    public partial class Loading : MetroForm
    {
        public static bool LoadCompleted = false;
        public Loading()
        {
            InitializeComponent();
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (LoadCompleted)
            {
                Close();
            }
        }
    }
}
