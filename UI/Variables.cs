using BotFramework;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace UI
{
    public class PrivateVariable
    {

        public static bool biubiu = false;
        /// <summary>
        /// Check event type of the game
        /// </summary>
        public static EventType VCevent;
        /// <summary>
        /// Event type
        /// </summary>
        public enum EventType
        {
            Tower = 0,
            ArchWitch = 1,
            DemonRealm = 2,
            SoulWeapon = 3,
            Unknown = -1
        }

        //public static List<byte[]> Enemies = new List<byte[]>();

        public static List<byte[]> Skills = new List<byte[]>();

        public static bool Battling, InEventScreen, InMainScreen, InMap;

        public static List<int> ProcessID = new List<int>();

        public static bool TakePartInNormalStage, AlwaysAttackNew;

        public static int UserSelectedStage;

        public static int NormalStageNum; //Normal map 1 or map 2

        public static int FirstPageStageNum, FirstPageBossNum;

        public static Point[] NormalStage;

        public static Point[] BossStage;

        public static bool CustomScript = false;

        public static DateTime nospam;

        //public static List<byte[]> Enemies = new List<byte[]>();
        public static List<BattleScript> BattleScript = new List<UI.BattleScript>();

        public static int Selected_Script = 0;

        public static bool Use_Item = false;

        public static Dictionary<string, Point> Archwitch = new Dictionary<string, Point>();

        public static Dictionary<string, Point> Archwitch2 = new Dictionary<string, Point>();

        public static Rectangle EmuDefaultLocation = new Rectangle();

    }
}
