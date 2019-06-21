using System;
using System.Collections.Generic;
using System.Linq;
using UI;
using System.Windows.Forms;
using BotFramework;
using System.Drawing;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;

namespace DefaultScript
{
    public class defaultScript : BattleScript
    {
        private static List<ComboBox> toolParameterComboBoxes = new List<ComboBox>();
        public void Attack()
        {

                foreach (var cb in toolParameterComboBoxes)
                {
                    int index = 0;
                    cb.Invoke((MethodInvoker)delegate { index = cb.SelectedIndex; });
                    switch (index)
                    {
                        case 0:
                            byte[] crop = BotCore.CropImage(VCBotScript.image, new Point(176, 356), new Point(330, 611));
                            foreach (var f in PrivateVariable.Skills)
                            {
                                if (!ScriptRun.Run)
                                {
                                    return;
                                }
                                try
                                {
                                    Thread.Sleep(10);
                                    Point? p = BotCore.FindImage(crop, f, false);
                                    if (p != null)
                                    {
                                        Variables.ScriptLog("Skill actived", Color.Blue);
                                        BotCore.SendSwipe(new Point(263, 473), new Point(264, 474), 1200);
                                        for (int x = 0; x < 6; x++)
                                        {
                                            Thread.Sleep(100);
                                            BotCore.SendTap(10, 10);
                                        }
                                        break;
                                    }
                                }
                                catch
                                {

                                }
                            }
                            break;
                        case 1:
                            crop = BotCore.CropImage(VCBotScript.image, new Point(357, 356), new Point(543, 610));
                            foreach (var f in PrivateVariable.Skills)
                            {
                                if (!ScriptRun.Run)
                                {
                                    return;
                                }
                                try
                                {
                                    Thread.Sleep(10);
                                    Point? p = BotCore.FindImage(crop, f, false);
                                    if (p != null)
                                    {
                                        Variables.ScriptLog("Skill actived", Color.Blue);
                                        BotCore.SendSwipe(new Point(448, 492), new Point(449, 493), 1200);
                                        for (int x = 0; x < 6; x++)
                                        {
                                            Thread.Sleep(100);
                                            BotCore.SendTap(10, 10);
                                        }
                                        break;
                                    }
                                }
                                catch
                                {

                                }
                            }
                            break;
                        case 2:
                            crop = BotCore.CropImage(VCBotScript.image, new Point(546, 376), new Point(724, 597));
                            foreach (var f in PrivateVariable.Skills)
                            {
                                if (!ScriptRun.Run)
                                {
                                    return;
                                }
                                try
                                {
                                    Thread.Sleep(10);
                                    Point? p = BotCore.FindImage(crop, f, false);
                                    if (p != null)
                                    {
                                        Variables.ScriptLog("Skill actived", Color.Blue);
                                        BotCore.SendSwipe(new Point(641, 473), new Point(642, 474), 1200);
                                        for (int x = 0; x < 6; x++)
                                        {
                                            Thread.Sleep(100);
                                            BotCore.SendTap(10, 10);
                                        }
                                        break;
                                    }
                                }
                                catch
                                {

                                }
                            }
                            break;
                        case 3:
                            crop = BotCore.CropImage(VCBotScript.image, new Point(761, 356), new Point(921, 613));
                            foreach (var f in PrivateVariable.Skills)
                            {
                                if (!ScriptRun.Run)
                                {
                                    return;
                                }
                                try
                                {
                                    Thread.Sleep(10);
                                    Point? p = BotCore.FindImage(crop, f, false);
                                    if (p != null)
                                    {
                                        Variables.ScriptLog("Skill actived", Color.Blue);
                                        BotCore.SendSwipe(new Point(834, 483), new Point(835, 484), 1200);
                                        for (int x = 0; x < 6; x++)
                                        {
                                            Thread.Sleep(100);
                                            BotCore.SendTap(10, 10);
                                        }
                                        break;
                                    }
                                }
                                catch
                                {

                                }
                            }
                            break;
                        case 4:
                            crop = BotCore.CropImage(VCBotScript.image, new Point(934, 356), new Point(1090, 578));
                            foreach (var f in PrivateVariable.Skills)
                            {
                                if (!ScriptRun.Run)
                                {
                                    return;
                                }
                                try
                                {
                                    Thread.Sleep(10);
                                    Point? p = BotCore.FindImage(crop, f, false);
                                    if (p != null)
                                    {
                                        Variables.ScriptLog("Skill actived", Color.Blue);
                                        BotCore.SendSwipe(new Point(1017, 470), new Point(1018, 471), 1200);
                                        for (int x = 0; x < 6; x++)
                                        {
                                            Thread.Sleep(100);
                                            BotCore.SendTap(10, 10);
                                        }
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
            
        }
        string cboxselected ="";
        public Control[] CreateUI()
        {
            Label text = new Label();
            text.Text = "Default Script by PoH98";
            text.AutoSize = true;
            text.Name = "lbl";
            text.Location = new Point(10, 10);
            text.TabIndex = 1000;
            RichTextBox txtBox = new RichTextBox();
            txtBox.Location = new Point(10, 40);
            txtBox.Height = 200;
            txtBox.Width = 400;
            txtBox.Text = "这个是默认的脚本战斗系统，将会自动应用到所有的战斗。如果想要自行创建脚本，请期待未来更新 CustomVCBotScript.dll 插件，或者到www.github.com/PoH98/Bot/了解如何自己创建脚本插件！";
            txtBox.ReadOnly = true;
            txtBox.TabIndex = 1001;
            txtBox.BackColor = Color.Black;
            txtBox.ForeColor = Color.Lime;
            txtBox.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            txtBox.Name = "Description";
            Label lbl = new Label();
            lbl.Text = "控制技能发动检查顺序";
            lbl.Location = new Point(10, 280);
            lbl.AutoSize = true;
            ComboBox box1 = new ComboBox();
            ComboBox box2 = new ComboBox();
            ComboBox box3 = new ComboBox();
            ComboBox box4 = new ComboBox();
            ComboBox box5 = new ComboBox();
            for (int x = 1; x < 6; x++)
            {
                box1.Items.Add(x);
                box2.Items.Add(x);
                box3.Items.Add(x);
                box4.Items.Add(x);
                box5.Items.Add(x);
            }
            if (cboxselected.Length == 5)
            {
                try
                {
                    int y =0;
                    foreach(var ch in cboxselected)
                    {
                        try
                        {
                            int x = Convert.ToInt32(ch.ToString());
                            switch (y)
                            {
                                case 0:
                                    box1.SelectedIndex = Convert.ToInt32(x);
                                    break;
                                case 1:
                                    box2.SelectedIndex = Convert.ToInt32(x);
                                    break;
                                case 2:
                                    box3.SelectedIndex = Convert.ToInt32(x);
                                    break;
                                case 3:
                                    box4.SelectedIndex = Convert.ToInt32(x);
                                    break;
                                case 4:
                                    box5.SelectedIndex = Convert.ToInt32(x);
                                    break;
                            }
                            y++;
                        }
                        catch
                        {

                        }
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    box1.SelectedIndex = 0;
                    box2.SelectedIndex = 1;
                    box3.SelectedIndex = 2;
                    box4.SelectedIndex = 3;
                    box5.SelectedIndex = 4;
                }
                
            }
            else
            {
                box1.SelectedIndex = 0;
                box2.SelectedIndex = 1;
                box3.SelectedIndex = 2;
                box4.SelectedIndex = 3;
                box5.SelectedIndex = 4;
            }

            box2.Width = box3.Width = box4.Width = box5.Width = box1.Width = 45;
            box1.Location = new Point(10, 300);
            box2.Location = new Point(65, 300);
            box3.Location = new Point(120, 300);
            box4.Location = new Point(175, 300);
            box5.Location = new Point(230, 300);
            box1.SelectedIndexChanged += Cards_SelectedIndexChanged;
            box2.SelectedIndexChanged += Cards_SelectedIndexChanged;
            box3.SelectedIndexChanged += Cards_SelectedIndexChanged;
            box4.SelectedIndexChanged += Cards_SelectedIndexChanged;
            box5.SelectedIndexChanged += Cards_SelectedIndexChanged;
            text.Focus();
            var Cards = new ComboBox[] { box1, box2, box3, box4, box5 };
            toolParameterComboBoxes = Cards.ToList();
            Control[] thingsToReturn = { text, txtBox, lbl, box1, box2, box3, box4, box5 };
            Variables.ScriptLog("Default script loaded",Color.Cyan);
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            string save = "";
            foreach(var i in toolParameterComboBoxes)
            {
                save += i.SelectedIndex.ToString();
            }
            WriteConfig("Skill_Acd",save);
        }

        private void Create_Click(object sender, EventArgs e)
        {
            MessageBox.Show("网络上有挺多教学的，学会后再到www.github.com/PoH98/Bot/查看格式。\n打开编程软件\n导入这个exe到Reference\n增加using UI;\n在class后面增加:BattleScript\n根据Github教学编写\n包装dll\n丢到Battle_Script里面\n打开挂机就能看见你写的脚本被加载进入挂机内！", "如何编写C# dll库");
        }

        public void ReadConfig()
        {
            Variables.Configure.TryGetValue("Skill_Acd", out cboxselected);
            if(cboxselected == null)
            {
                cboxselected = "";
            }
        }
        public string ScriptName()
        {
            return "Default Script";
        }
        private static void WriteConfig(string key, string value)
        {
            var config = File.ReadAllLines("Profiles\\" + BotCore.profilePath + "\\bot.ini");
            int x = 0;
            foreach (var c in config)
            {
                if (c.Contains(key + "="))
                {
                    config[x] = key + "=" + value;
                    File.WriteAllLines("Profiles\\" + BotCore.profilePath + "\\bot.ini", config);
                    return;
                }
                x++;
            }
            config[config.Length - 1] = config[config.Length - 1] + "\n" + key + "=" + value;
            if (Variables.Configure.ContainsKey(key))
            {
                Variables.Configure[key] = value;
            }
            else
            {
                Variables.Configure.Add(key, value);
            }
            File.WriteAllLines("Profiles\\" + BotCore.profilePath + "\\bot.ini", config);
        }
    }
}
