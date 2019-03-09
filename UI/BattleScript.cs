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
}