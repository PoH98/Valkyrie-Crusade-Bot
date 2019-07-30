using System;
using System.Collections.Generic;
using System.Windows.Forms;
using UI;

namespace SmartScript
{
    public class SmartScript : BattleScript
    {
        //4 teams, each have 5 cards
        private static char[,] team = new char[4,5];
        public void Attack()
        {
            
        }

        public Control[] CreateUI()
        {
            List<Control> controls = new List<Control>();
            foreach(Control c in new UI().Controls)
            {
                if (c.Name.Contains("_"))
                {
                    c.Text = team[Convert.ToInt32(c.Name.Split('_')[1]), Convert.ToInt32(c.Name.Split('_')[2])].ToString();
                }
                controls.Add(c);
            }
            return controls.ToArray();
        }

        public void ReadConfig()
        {
            
        }

        public string ScriptName()
        {
            return "Smart Script";
        }
    }
}
