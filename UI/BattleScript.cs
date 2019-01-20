using ImageProcessor;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UI
{
    /// <summary>
    /// BattleScrpt for all event to fight Bosses
    /// </summary>
    public interface BattleScript
    {
        /// <summary>
        /// You have to read your script how to translate if you let users to customize script!
        /// </summary>
        void ReadConfig();

        /// <summary>
        /// Then you have to set how the attack will be done! Tips: remember to add Script.clickLocation for reading enemies location or click away UI!
        /// </summary>
        void Attack();

        /// <summary>
        /// You need to create bunch of UIs! Else how users enable it?
        /// </summary>
        Control[] CreateUI();

        /// <summary>
        /// Your Script's Title
        /// </summary>
        /// <returns></returns>
        string ScriptName();
    }

    public class defaultScript : BattleScript
    {
        private static List<ComboBox> toolParameterComboBoxes = new List<ComboBox>();
        public void Attack()
        {
            for (int x = 0; x < 10; x++)
            {
                EmulatorController.SendTap(1, 1);
            }
            foreach(var cb in toolParameterComboBoxes)
            {
                int index = 0;
                cb.Invoke((MethodInvoker)delegate { index = cb.SelectedIndex; });
                switch (index)
                {
                    case 0:
                        byte[] crop = EmulatorController.CropImage(Script.image, new Point(176, 356), new Point(330, 611));
                        foreach (var f in PrivateVariable.Skills)
                        {
                            if (!PrivateVariable.Run)
                            {
                                return;
                            }
                            try
                            {
                                Thread.Sleep(10);
                                Point? p = EmulatorController.FindImage(crop, f, false);
                                if (p != null)
                                {
                                    Variables.ScriptLog.Add("Skill actived");
                                    EmulatorController.SendSwipe(new Point(263, 473), new Point(264, 474), 1200);
                                    Thread.Sleep(1000);
                                    break;
                                }
                            }
                            catch
                            {

                            }
                        }
                        break;
                    case 1:
                        crop = EmulatorController.CropImage(Script.image, new Point(357, 356), new Point(543, 610));
                        foreach (var f in PrivateVariable.Skills)
                        {
                            if (!PrivateVariable.Run)
                            {
                                return;
                            }
                            try
                            {
                                Thread.Sleep(10);
                                Point? p = EmulatorController.FindImage(crop, f, false);
                                if (p != null)
                                {
                                    Variables.ScriptLog.Add("Skill actived");
                                    EmulatorController.SendSwipe(new Point(448, 492), new Point(449, 493), 1200);
                                    Thread.Sleep(1000);
                                    break;
                                }
                            }
                            catch
                            {

                            }
                        }
                        break;
                    case 2:
                        crop = EmulatorController.CropImage(Script.image, new Point(546, 376), new Point(724, 597));
                        foreach (var f in PrivateVariable.Skills)
                        {
                            if (!PrivateVariable.Run)
                            {
                                return;
                            }
                            try
                            {
                                Thread.Sleep(10);
                                Point? p = EmulatorController.FindImage(crop, f, false);
                                if (p != null)
                                {
                                    Variables.ScriptLog.Add("Skill actived");
                                    EmulatorController.SendSwipe(new Point(641, 473), new Point(642, 474), 1200);
                                    Thread.Sleep(1000);
                                    break;
                                }
                            }
                            catch
                            {

                            }
                        }
                        break;
                    case 3:
                        crop = EmulatorController.CropImage(Script.image, new Point(761, 356), new Point(921, 613));
                        foreach (var f in PrivateVariable.Skills)
                        {
                            if (!PrivateVariable.Run)
                            {
                                return;
                            }
                            try
                            {
                                Thread.Sleep(10);
                                Point? p = EmulatorController.FindImage(crop, f, false);
                                if (p != null)
                                {
                                    Variables.ScriptLog.Add("Skill actived");
                                    EmulatorController.SendSwipe(new Point(834, 483), new Point(835, 484), 1200);
                                    Thread.Sleep(1000);
                                    break;
                                }
                            }
                            catch
                            {

                            }
                        }
                        break;
                    case 4:
                        crop = EmulatorController.CropImage(Script.image, new Point(934, 356), new Point(1090, 578));
                        foreach (var f in PrivateVariable.Skills)
                        {
                            if (!PrivateVariable.Run)
                            {
                                return;
                            }
                            try
                            {
                                Thread.Sleep(10);
                                Point? p = EmulatorController.FindImage(crop,f, false);
                                if (p != null)
                                {
                                    Variables.ScriptLog.Add("Skill actived");
                                    EmulatorController.SendSwipe(new Point(1017, 470), new Point(1018, 471), 1200);
                                    Thread.Sleep(1000);
                                    break;
                                }
                            }
                            catch
                            {

                            }
                        }
                        break;

                }
            }
            if (Script.clickLocation != null)
            {
                EmulatorController.SendTap(Script.clickLocation.Value);
            }
            else
            {
                EmulatorController.SendTap(640, 156);
                EmulatorController.SendTap(462, 176);
                EmulatorController.SendTap(820, 187);
                EmulatorController.SendTap(311, 190);
                EmulatorController.SendTap(955, 189);
            }
        }

        public Control[] CreateUI()
        {
            Label text = new Label();
            text.Text = "Default Script by PoH98";
            text.Width = 500;
            text.Name = "lbl";
            text.Location = new Point(10,10);
            text.TabIndex = 1000;
            RichTextBox txtBox = new RichTextBox();
            txtBox.Location = new Point(10, 40);
            txtBox.Height = 200;
            txtBox.Width = 510;
            txtBox.Text = "这个是默认的脚本战斗系统，将会自动应用到所有的战斗。如果想要自行创建脚本，请期待未来更新 CustomScript.dll 插件，或者到www.github.com/PoH98/Bot/了解如何自己创建脚本插件！";
            txtBox.ReadOnly = true;
            txtBox.TabIndex = 1001;
            txtBox.BackColor = Color.Black;
            txtBox.ForeColor = Color.Lime;
            txtBox.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            txtBox.Name = "Description";
            CheckBox chk = new CheckBox();
            chk.Text = "使用脚本";
            chk.Checked = true;
            chk.EnabledChanged += Chk_EnabledChanged;
            chk.Location = new Point(10,250);
            chk.AutoSize = true;
            if(PrivateVariable.BattleScript.Count == 1)
            {
                chk.Enabled = false;
            }
            Label lbl = new Label();
            lbl.Text = "控制技能发动检查顺序";
            lbl.Location = new Point(10, 280);
            lbl.AutoSize = true;
            ComboBox box1 = new ComboBox();
            ComboBox box2 = new ComboBox();
            ComboBox box3 = new ComboBox();
            ComboBox box4 = new ComboBox();
            ComboBox box5 = new ComboBox();
            for(int x = 1; x < 6; x++)
            {
                box1.Items.Add(x);
                box2.Items.Add(x);
                box3.Items.Add(x);
                box4.Items.Add(x);
                box5.Items.Add(x);
            }
            box1.SelectedIndex = 0;
            box2.SelectedIndex = 1;
            box3.SelectedIndex = 2;
            box4.SelectedIndex = 3;
            box5.SelectedIndex = 4;
            box2.Width = box3.Width = box4.Width = box5.Width = box1.Width = 50;
            box1.Location = new Point(10, 310);
            box2.Location = new Point(70, 310);
            box3.Location = new Point(130, 310);
            box4.Location = new Point(190, 310);
            box5.Location = new Point(250, 310);
            box1.SelectedIndexChanged += Cards_SelectedIndexChanged;
            box2.SelectedIndexChanged += Cards_SelectedIndexChanged;
            box3.SelectedIndexChanged += Cards_SelectedIndexChanged;
            box4.SelectedIndexChanged += Cards_SelectedIndexChanged;
            box5.SelectedIndexChanged += Cards_SelectedIndexChanged;
            var Cards = new ComboBox[]{ box1, box2, box3, box4, box5};
            toolParameterComboBoxes = Cards.ToList();
            Button Create = new Button();
            Create.Location = new Point(10, 370);
            Create.Click += Create_Click;
            Create.Text = "创建脚本 (C#语言）";
            Create.Width = 510;
            Create.Height = 50;
            Control[] thingsToReturn = { text, txtBox ,chk, Create, lbl, box1, box2, box3, box4, box5};
            return thingsToReturn;
        }

        private void Cards_SelectedIndexChanged(object sender, EventArgs e)
        {
            List<int> existedIndex = new List<int>();
            ComboBox thisCB = sender as ComboBox;
            List<ComboBox> cblist = new List<ComboBox>();
            cblist.Add(thisCB);
            var cbx = toolParameterComboBoxes.Except(cblist).ToList();
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
                    toolParameterComboBoxes[duplicated_index].SelectedIndex = result.Last();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        private void Create_Click(object sender, EventArgs e)
        {
            MessageBox.Show("网络上有挺多教学的，学会后再到www.github.com/PoH98/Bot/查看格式。\n打开编程软件\n导入这个exe到Reference\n增加using UI;\n在class后面增加:BattleScript\n根据Github教学编写\n包装dll\n丢到Battle_Script里面\n打开挂机就能看见你写的脚本被加载进入挂机内！", "如何编写C# dll库");
        }

        private void Chk_EnabledChanged(object sender, EventArgs e)
        {
            if((sender as CheckBox).Checked)
            {
                PrivateVariable.Selected_Script = PrivateVariable.BattleScript.IndexOf(this);
            }

        }

        public void ReadConfig()
        {
            
        }

        public string ScriptName()
        {
            return "Default Script";
        }
    }
}
