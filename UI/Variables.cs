using BotFramework;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace UI
{
    public class PrivateVariable
    {
        public static PrivateVariable Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new PrivateVariable();
                }
                return _instance;
            }
        }

        private static PrivateVariable _instance;

        public bool biubiu = false;
        /// <summary>
        /// Check event type of the game
        /// </summary>
        public EventType VCevent;
        /// <summary>
        /// Event type
        /// </summary>
        public enum EventType
        {
            Tower = 0,
            ArchWitch = 1,
            DemonRealm = 2,
            SoulWeapon = 3,
            GuildWar = 4,
            Unknown = -1
        }

        //public static List<byte[]> Enemies = new List<byte[]>();

        public List<byte[]> Skills = new List<byte[]>();

        public bool Battling, InEventScreen, InMainScreen, InMap;

        public List<int> ProcessID = new List<int>();

        public bool TakePartInNormalStage, AlwaysAttackNew;

        public int UserSelectedStage;

        public int NormalStageNum; //Normal map 1 or map 2

        public int FirstPageStageNum, FirstPageBossNum;

        public Point[] NormalStage;

        public Point[] BossStage;

        public bool CustomScript = false;

        public DateTime nospam;

        //public static List<byte[]> Enemies = new List<byte[]>();
        public List<BattleScript> BattleScript = new List<UI.BattleScript>();

        public int Selected_Script = 0;

        public bool Use_Item = false;

        public bool LocatedGuildWar = false;

        public Dictionary<string, Point> Archwitch = new Dictionary<string, Point>();

        public Dictionary<string, Point> Archwitch2 = new Dictionary<string, Point>();

        public Rectangle EmuDefaultLocation = new Rectangle();

    }
}

