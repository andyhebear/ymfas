using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Ymfas {
    public enum GameMode { Deathmatch, TeamDeathmatch, CaptureTheFlag, ConvoyDefense, KingOfTheAsteroid };
    public enum GameTeam { NoTeam, Team1, Team2 };

	public class YmfasClient : SpiderClient
	{
		public Hashtable PlayerIPs;
		public GameMode GameMode;
		public GameTeam Team;
		public int PlayerId;
		public Hashtable PlayerIdsByIP;
		public Hashtable PlayerNamesById;

		public YmfasClient(string name) : base(name) 
		{
		}
	}

    public class YmfasServer : SpiderServer 
	{
        public Hashtable PlayerIPs;
        public GameMode GameMode;
        public GameTeam Team;
        public int PlayerId;
        public Hashtable PlayerIdsByIP;
        public Hashtable PlayerNamesById;

		public YmfasServer(string name) : base(name)
		{
			PlayerIPs = new Hashtable();
			PlayerIdsByIP = new Hashtable();
			PlayerNamesById = new Hashtable();
		}
    }


}
