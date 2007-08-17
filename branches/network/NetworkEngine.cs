using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace Ymfas {
    public enum GameMode { Deathmatch, TeamDeathmatch, CaptureTheFlag, ConvoyDefense, KingOfTheAsteroid };
    public enum GameTeam { NoTeam, Team1, Team2 };
	
	public static class GameInfo
	{
		public static bool IsTeamGame( GameMode gm )
		{
			return (gm != GameMode.TeamDeathmatch && gm == GameMode.KingOfTheAsteroid );
		}
	}

	public class YmfasClient : SpiderClient
	{
		public GameMode GameMode;
		public GameTeam Team;
		public int playerId;

		public int PlayerId
		{
			get { return playerId; }
			set { playerId = value; }
		}

		public YmfasClient(string name) : base(name) 
		{
		}
	}

    public class YmfasServer : SpiderServer 
	{
        private Dictionary<IPAddress, string> playerIPs;
        public GameMode GameMode;
        public GameTeam Team;
		private Dictionary<IPAddress, int> playerIdsByIP;
		private Dictionary<int, string> playerNamesById;

		public YmfasServer(string name) : base(name)
		{
			playerIPs = new Dictionary<IPAddress, string>();
			playerIdsByIP = new Dictionary<IPAddress, int>();
			playerNamesById = new Dictionary<int,string>();
		}

		// add a player
		public void AddPlayer(IPAddress ip, string name, int id)
		{
			playerIPs.Add(ip, name + " [" + ip + "]");
			playerIdsByIP.Add(ip, id);
			playerNamesById.Add(id, name);
		}

		// remove a player
		public void RemovePlayer(IPAddress ip)
		{
			//Remove key from all hashtables
			playerIPs.Remove(ip);
			playerNamesById.Remove(playerIdsByIP[ip]);
			playerIdsByIP.Remove(ip);
		}

		// grab a player's information
		public string GetPlayerInfoString(IPAddress ip)
		{
			return playerIPs[ip];
		}
		public string GetPlayerName(int id)
		{
			return playerNamesById[id];
		}
		public string GetPlayerName(IPAddress ip)
		{
			return playerNamesById[playerIdsByIP[ip]];
		}
		
		public bool IsPlayerConnected(IPAddress ip)
		{
			return playerIPs.ContainsKey(ip);
		}

		#region PublicProperties
		public int NumPlayers
		{
			get { return playerIPs.Count; }
		}

		public ICollection<String> PlayerInfoStrings
		{
			get { return playerIPs.Values; }
		}

		public ICollection<int> PlayerIds
		{
			get { return playerIdsByIP.Values; }
		}

		#endregion

    }
}
