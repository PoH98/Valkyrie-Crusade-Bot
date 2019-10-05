using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using BotFramework;
using System.Windows.Forms;

namespace DefaultScript
{
    public partial class UI : Form
    {
        public bool LoadCompleted = false;
        public UI()
        {
            InitializeComponent();
        }

        private void Cards_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (LoadCompleted)
            {
                List<int> existedIndex = new List<int>();
                ComboBox thisCB = sender as ComboBox;
                List<ComboBox> cblist = new List<ComboBox>();
                cblist.Add(thisCB);
                var cbx = defaultScript.toolParameterComboBoxes.Except(cblist).ToList();
                try
                {
                    if (thisCB.Text != "")
                    {
                        foreach (ComboBox cb in cbx)
                        {
                            existedIndex.Add(cb.SelectedIndex);
                        }
                    }
                    int duplicated_index = existedIndex.IndexOf(thisCB.SelectedIndex);
                    if (duplicated_index > -1)
                    {
                        var result = Enumerable.Range(0, 5).Except(existedIndex).ToList();
                        defaultScript.toolParameterComboBoxes[duplicated_index].SelectedIndex = result.First();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                string save = "";
                foreach (var i in defaultScript.toolParameterComboBoxes)
                {
                    save += i.SelectedIndex.ToString();
                }
                Variables.ModifyConfig("DefaultScript", "Skill_Acd", save);
            }
        }
        private void Chkbox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = (sender as CheckBox);
            switch (chk.Name)
            {
                case "checkBox1":
                    defaultScript.cboxchecked[0] = chk.Checked;
                    break;
                case "checkBox2":
                    defaultScript.cboxchecked[1] = chk.Checked;
                    break;
                case "checkBox3":
                    defaultScript.cboxchecked[2] = chk.Checked;
                    break;
                case "checkBox4":
                    defaultScript.cboxchecked[3] = chk.Checked;
                    break;
                case "checkBox5":
                    defaultScript.cboxchecked[4] = chk.Checked;
                    break;
            }
            StringBuilder builder = new StringBuilder();
            foreach(var b in defaultScript.cboxchecked)
            {
                builder.Append(b.ToString().ToLower() + "|");
            }
            builder.Remove(builder.Length - 1 ,1);
            Variables.ModifyConfig("DefaultScript", "KO_Chance", builder.ToString());
        }
    }
}
