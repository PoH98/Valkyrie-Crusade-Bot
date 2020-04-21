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
    /// <summary>
    /// 
    /// </summary>
    public partial class Emulator_Selection : Form
    {
        /// <summary>
        /// 
        /// </summary>
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
            EmuSelection_Resource.selected = EmuSelection_Resource.emu[comboBox1.SelectedIndex].GetType().Name;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class EmuSelection_Resource
    {
        /// <summary>
        /// 
        /// </summary>
        public static List<EmulatorInterface> emu = new List<EmulatorInterface>();
        /// <summary>
        /// 
        /// </summary>
        public static string selected;

    }
}
