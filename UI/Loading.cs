using System;
using System.Drawing;
using System.Windows.Forms;
using MetroFramework.Forms;

namespace UI
{
    public partial class Login : MetroForm
    {
        public static bool LoadCompleted = false;
        
        public Login()
        {
            InitializeComponent();
            timer1.Start();
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (LoadCompleted)
            {
                Close();
            }
        }

        private void Button1_MouseEnter(object sender, EventArgs e)
        {
            (sender as Button).BackColor = Color.DarkGray;
        }

        private void Button1_MouseLeave(object sender, EventArgs e)
        {
            (sender as Button).BackColor = Color.DimGray;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                MessageBox.Show("Username is empty");
                textBox1.Focus();
                return;
            }
        }

        private void Login_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
            this.Focus();
            this.BringToFront();
            this.TopMost = false;
        }

        private void Login_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!LoadCompleted)
            {
                Environment.Exit(0);
            }
        }
    }
}
