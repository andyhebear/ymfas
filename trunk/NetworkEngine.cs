using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Ymfas {
    public enum GameMode { Deathmatch, TeamDeathmatch, CaptureTheFlag, ConvoyDefense, KingOfTheAsteroid };
    public enum GameTeam { NoTeam, Team1, Team2 };
    public static class NetworkEngine {
        public static SpiderEngine.Spider Engine;
        public static SpiderEngine.SpiderType EngineType;
        public static Hashtable PlayerIPs;
        public static GameMode GameMode;
        public static GameTeam Team;
        public static int PlayerId;
    }
}
