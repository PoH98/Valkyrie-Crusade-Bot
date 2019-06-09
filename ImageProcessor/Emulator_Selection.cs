using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BotFramework
{
    public partial class Emulator_Selection : Form
    {
        public Emulator_Selection()
        {
            InitializeComponent();
        }

        private void Emulator_Selection_Load(object sender, EventArgs e)
        {
            foreach(var em in EmuSelection_Resource.emu)
            {
                comboBox1.Items.Add(em.EmulatorName());
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if(comboBox1.SelectedItem == null)
            {
                MessageBox.Show("Please select a emulator first!\n请选择一个模拟器！");
                return;
            }
            EmuSelection_Resource.selected = comboBox1.SelectedItem.ToString();
            DialogResult = DialogResult.OK;
            Close();
        }
    }

    public class EmuSelection_Resource
    {
        public static List<EmulatorInterface> emu = new List<EmulatorInterface>();
        public static string selected;

    }
}
