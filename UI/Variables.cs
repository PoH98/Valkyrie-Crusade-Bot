using System.Collections.Generic;
using System.Drawing;

namespace UI
{
    public class PrivateVariable
    {

        /// <summary>
        /// Check event type of the game
        /// </summary>
        public static int EventType;

        //public static List<byte[]> Enemies = new List<byte[]>();

        public static List<byte[]> Skills = new List<byte[]>();

        public static bool Run, Battling, InEventScreen, InMainScreen;

        public static List<int> ProcessID = new List<int>();

        public static bool TakePartInNormalStage;

        public static int UserSelectedStage;

        public static int NormalStageNum; //Normal map 1 or map 2

        public static int FirstPageStageNum;

        public static Point[] NormalStage;

        public static Point[] BossStage;
    }
}
