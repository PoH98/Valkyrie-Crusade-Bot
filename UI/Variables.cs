using System;
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

        public static bool Run, Battling, InEventScreen, InMainScreen, InMap;

        public static List<int> ProcessID = new List<int>();

        public static bool TakePartInNormalStage, AlwaysAttackNew;

        public static int UserSelectedStage;

        public static int NormalStageNum; //Normal map 1 or map 2

        public static int FirstPageStageNum, FirstPageBossNum;

        public static Point[] NormalStage;

        public static Point[] BossStage;

        public static bool CustomScript = false;

        public static DateTime nospam;

        public static bool EnterRune;
        /// <summary>
        /// The Fixed Adb IP for the emulator
        /// </summary>
        public static string Adb_IP;
        /// <summary>
        /// The real main window of MEmu
        /// </summary>
        public static IntPtr MEmu_MainWindow;

        public static bool CloseEmulator = false;
        //public static List<byte[]> Enemies = new List<byte[]>();
        public static List<BattleScript> BattleScript = new List<UI.BattleScript>();

        public static int Selected_Script = 0;
    }
}
