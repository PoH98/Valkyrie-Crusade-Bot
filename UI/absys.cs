using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BotFramework;
using MetroFramework.Forms;

namespace UI
{
    public partial class absys : MetroForm
    {
        public absys()
        {
            InitializeComponent();
        }
        static object[] devices;
        private void Absys_Load(object sender, EventArgs e)
        {
            devices = BotCore.GetDevices(out string[] names);
            foreach(var name in names)
            {
                comboBox1.Items.Add(name);
                comboBox2.Items.Add(name);
            }

        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if(comboBox1.SelectedItem != null && comboBox2.SelectedItem != null)
            {
                BotCore.AdbCommand("backup -noapk com.nubee.valkyriecrusade", devices[comboBox1.SelectedIndex]);
                BotCore.AdbCommand("restore backup.ab", devices[comboBox2.SelectedIndex]);
            }
            else
            {
                MessageBox.Show("Please select the devices for transfering data!");
            }
        }
    }
}
