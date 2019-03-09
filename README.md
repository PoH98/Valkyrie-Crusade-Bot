# 神女控挂机
QQ群号：809233738

__自定义脚本插件例子：__


    //class名字随意，但是必须加上 : BattleScript为后叠
    public class defaultScript : BattleScript
    {
        //最主要的脚本操作
        public void Attack()
        {
            byte[] crop = EmulatorController.CropImage(Script.image, new Point(176, 356), new Point(330, 611));
            for (int x = 0; x < 10; x++)
            {
                EmulatorController.SendTap(1, 1);
            }
            foreach (var f in PrivateVariable.Skills)
            {
                if (!PrivateVariable.Run)
                {
                    return;
                }
                try
                {
                    Point? p = EmulatorController.FindImage(crop, new Bitmap(EmulatorController.Decompress(f)), false);
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
            crop = EmulatorController.CropImage(Script.image, new Point(357, 356), new Point(543, 610));
            foreach (var f in PrivateVariable.Skills)
            {

                if (!PrivateVariable.Run)
                {
                    return;
                }
                try
                {
                    Point? p = EmulatorController.FindImage(crop, new Bitmap(EmulatorController.Decompress(f)), false);
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
            crop = EmulatorController.CropImage(Script.image, new Point(546, 376), new Point(724, 597));
            foreach (var f in PrivateVariable.Skills)
            {
                if (!PrivateVariable.Run)
                {
                    return;
                }
                try
                {
                    Point? p = EmulatorController.FindImage(crop, new Bitmap(EmulatorController.Decompress(f)), false);
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
            crop = EmulatorController.CropImage(Script.image, new Point(761, 356), new Point(921, 613));
            foreach (var f in PrivateVariable.Skills)
            {
                if (!PrivateVariable.Run)
                {
                    return;
                }
                try
                {
                    Point? p = EmulatorController.FindImage(crop, new Bitmap(EmulatorController.Decompress(f)), false);
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
            crop = EmulatorController.CropImage(Script.image, new Point(934, 356), new Point(1090, 578));
            foreach (var f in PrivateVariable.Skills)
            {
                if (!PrivateVariable.Run)
                {
                    return;
                }
                try
                {
                    Point? p = EmulatorController.FindImage(crop, new Bitmap(EmulatorController.Decompress(f)), false);
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
        //创建UI设计
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
            CheckBox chk = new CheckBox();
            chk.Text = "使用脚本";
            chk.Checked = true;
            chk.EnabledChanged += Chk_EnabledChanged;
            chk.Location = new Point(10,250);
            chk.AutoSize = true;
            if(PrivateVariable.BattleScript.Count == 1)
            {
                chk.Enabled = false; //只有默认脚本在，那就不让用户修改要不要用脚本了，不然是要在战斗的时候永久发呆？
            }
            WebBrowser wb = new WebBrowser();
            wb.Location = new Point(10, 270);
            wb.Height = 400;
            wb.Width = 510;
            wb.Navigate("www.github.com/PoH98/Bot/");
            Control[] thingsToReturn = { text, txtBox ,chk,wb};
            return thingsToReturn;
        }
        //创建的GUI可以另外增加功能
        private void Chk_EnabledChanged(object sender, EventArgs e)
        {
            if((sender as CheckBox).Checked)
            {
                PrivateVariable.Selected_Script = 0;
            }

        }
        //如果您的脚本有任何需要读取脚本资料的，可在这个地方加入，如File.ReadAllLines之类
        public void ReadConfig()
        {
            
        }
        //脚本名字
        public string ScriptName()
        {
            return "Default Script";
        }
