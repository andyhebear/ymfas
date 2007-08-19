using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using Lidgren.Library.Network;

namespace Ymfas {
    public partial class frmGameLobby : Form {

		#region PrivateConstants
		private const int PLAYERLIST_UPDATE_INTERVAL = 1000;
		private const int GAMESTART_INTERVAL = 1000; 
		#endregion

		enum LobbyMode
		{
			Hosting,
			ClientOnly
		};

		// timing
		private int gameStartTime;
        private int timerTicks;
        private int gameStartCount;
		private bool gameStarting;

		// player lists
		private ArrayList playersReady;
        private ArrayList playersNotReady;
		        
        private int idTicketCounter;

		private YmfasClient client;
		private YmfasServer server;
		
		public YmfasClient Client
		{
			get { return client; }
			set { client = value; }
		}
		public YmfasServer Server
		{
			get { return server; }
			set { server = value; }
		}

		private LobbyMode lobbyMode = LobbyMode.ClientOnly;

        public frmGameLobby(YmfasClient _client, YmfasServer _server) 
		{
			client = _client;
			server = _server;
			lobbyMode = LobbyMode.Hosting;
			Initialize();

			chkReady.Visible = false;
			cmbGameMode.Enabled = true;
			idTicketCounter = 1;
        }

		public frmGameLobby(YmfasClient _client)
		{
			client = _client;
			lobbyMode = LobbyMode.ClientOnly;
			Initialize();			
		}

		/// <summary>
		/// form initialization common to both lobby types
		/// </summary>
		public void Initialize()
		{
			InitializeComponent();
			timerTicks = 0;
			playersReady = new ArrayList();
			playersNotReady = new ArrayList();
			gameStartCount = 3;
			gameStarting = false;

			cmbGameMode.SelectedIndex = 0;
			cmbTeam.SelectedIndex = 0;
		}
		
		/// <summary>
		/// Add a message to the chat window
		/// </summary>
		/// <param name="s"></param>
		private void AddChatMessage( string s )
		{
			rtxtChatWindow.Text += "\n" + s;
			rtxtChatWindow.Select(rtxtChatWindow.Text.Length + 1, 2);
			rtxtChatWindow.ScrollToCaret();
		}
		
        private void timer_Tick(object sender, EventArgs e) {
            timerTicks++;

            //game starting?
            if (gameStarting) {
                if ((timerTicks * timer.Interval - gameStartTime) % GAMESTART_INTERVAL == 0) {
                    if (gameStartCount == 0) {
						this.DialogResult = DialogResult.OK;
						this.Close();
                    }

                    //count down
                    rtxtChatWindow.Text += "\n"+gameStartCount+"...";
                    rtxtChatWindow.Select(rtxtChatWindow.Text.Length + 1, 2);
                    rtxtChatWindow.ScrollToCaret();

                    gameStartCount--;
                }
            }

            //update
			if (lobbyMode == LobbyMode.Hosting)
			{
				server.Update();
				ProcessServerMessages();
			}

			client.Update();
			ProcessClientMessages();
			
            //server management items
            if (lobbyMode == LobbyMode.Hosting) 
			{				
				// process disconnected ips
                IPAddress disconIP;
                while ((disconIP = server.GetDisconnectedIP()) != null) {
                    if(server.IsPlayerConnected(disconIP)){
                        
						//Send chat message
                        SpiderMessage message = new SpiderMessage(
							server.GetPlayerInfoString(disconIP) + " has disconnected.", 
							SpiderMessageType.String, "chat");
                        server.SendMessage(message, NetChannel.ReliableUnordered);

						server.RemovePlayer(disconIP);
                    }
                }
				
				// update player list for everyone
                if ((timer.Interval * timerTicks) % PLAYERLIST_UPDATE_INTERVAL == 0) {
					//SendPlayerList();					
                }
            }
        }

		private void SendPlayerList()
		{
			String playerList = "";

			ICollection<String> playerStrings = server.PlayerInfoStrings;
			foreach (string s in playerStrings)
				playerList += s + "\n";

			SpiderMessage message = new SpiderMessage(playerList, SpiderMessageType.String, "players");
			server.SendMessage(message, Lidgren.Library.Network.NetChannel.Ordered1);
		}

		/// <summary>
		/// Process messages sent to the server by various clients
		/// such as chat messages, new players, etc.
		/// </summary>
		private void ProcessServerMessages()
		{
			SpiderMessage msg = null;

			while ( ( msg = server.GetNextMessage() ) != null) 
			{
                //server message check
                Console.Out.WriteLine("+" + msg.Label + "+");
                Console.Out.WriteLine("+" + msg.Data.ToString() + "+");
                switch (msg.Label) {
                    case "chat":
                        //bounce the message
                        Console.Out.WriteLine("bouncing the message");
                        server.SendMessage(msg, Lidgren.Library.Network.NetChannel.ReliableUnordered);
                        break;

                    case "name":
                        //newly connected player is identifying himself                            
						server.AddPlayer(msg.IP, (string)msg.Data, idTicketCounter); 
                        playersNotReady.Add(((String)msg.Data) + " [" + msg.IP + "]");

                        // reply with a player identifier
                        SpiderMessage responseMsg = new SpiderMessage(idTicketCounter, SpiderMessageType.Int, "id");
                        idTicketCounter++;
                        server.SendMessage(responseMsg, NetChannel.Ordered1, msg.Connection);

						// update the player list
						SendPlayerList();

                        btnStart.Enabled = false;
                        break;

                    case "ready":
                        Console.Out.WriteLine("value : " + msg.Data);

                        //player is ready
                        if (((int)msg.Data) == 1) {
                            for (int i = 0; i < playersNotReady.Count; i++) {
                                Console.Out.WriteLine(((String)playersNotReady[i]) + " ---- " + ((String)playersNotReady[i]).IndexOf(msg.IP.ToString()));
                                if (((String)playersNotReady[i]).IndexOf(msg.IP.ToString()) != -1) {
                                    
                                    String temp = (String)playersNotReady[i];
                                    playersNotReady.RemoveAt(i);
                                    playersReady.Add(temp);
                                    break;
                                }
                            }
                        }

                        //player is not ready
                        else {
                            for (int i = 0; i < playersReady.Count; i++) {
                                if (((String)playersReady[i]).IndexOf(msg.IP.ToString()) != -1) {
                                    String temp = (String)playersReady[i];
                                    playersReady.RemoveAt(i);
                                    playersNotReady.Add(temp);
                                    break;
                                }
                            }
                        }

                        //enable or disable start button depending on readiness
                        if (playersNotReady.Count == 0) {
                            Console.Out.WriteLine("ready");
                            btnStart.Enabled = true;
                        }
                        else {
                            Console.Out.WriteLine("not ready");
                            btnStart.Enabled = false;
                        }
                        break;
                    default:
                        break;
                }
			}
		}

		private void ProcessClientMessages()
		{
			SpiderMessage msg = null;
			while ( (msg = client.GetNextMessage()) != null)
			{
                Console.Out.WriteLine(msg.Label);
                Console.Out.WriteLine((String)msg.Data.ToString());

                // client message check
                switch (msg.Label) 
				{
                    case "chat":
						AddChatMessage( (String)msg.Data );
                        break;

                    case "players":
                        Console.Out.WriteLine("playerlist update");

                        //Clear & Set player list
                        lstPlayers.Items.Clear();
                        String players = (String)(msg.Data);

						// split the list by newlines, which separate players
                        while (!players.Equals("")) {
                            int i = players.IndexOf('\n');
                            lstPlayers.Items.Add(players.Substring(0, i));
                            players = players.Substring(i+1);
                        }
                        break;

                    case "id":
                        client.PlayerId = (int)msg.Data;
                        break;

                    case "mode":
                        try {
                            cmbGameMode.SelectedIndex = (int)msg.Data;
                            chkReady.Checked = false;

                            //parse selection into GameMode
                            client.GameMode = (GameMode)Enum.Parse(typeof(GameMode), cmbGameMode.Items[cmbGameMode.SelectedIndex].ToString().Replace(" ", ""));
                        }
                        catch (Exception err) {
							System.Console.WriteLine(err.Message);
                        }

                        //team or solo play?
						cmbTeam.Enabled = GameInfo.IsTeamGame(client.GameMode);
                        break;

                    case "start":
                        cmbTeam.Enabled = false;
                        chkReady.Enabled = false;
                        gameStartTime = timerTicks * timer.Interval;
                        gameStarting = true;
                        break;              
                    default:
                        AddChatMessage("\nUnknown network message recieved.");
                        break;
                }
            }
		}

		/// <summary>
		/// send a message to the server
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        private void txtChatInput_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {

                // have the client send the message to the server
                SpiderMessage msg = new SpiderMessage(client.GetName() + ": " + txtChatInput.Text, SpiderMessageType.String, "chat");
                client.SendMessage(msg, NetChannel.ReliableUnordered);

                //clear input
                txtChatInput.Multiline = false;
                txtChatInput.Text = null;
                txtChatInput.Multiline = true;

                e.Handled = true;

            }
        }

		/// <summary>
		/// Configure what kind of lobby is seen
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        private void frmGameLobby_Load(object sender, EventArgs e) 
		{                        
            // Chat connect message
			System.Console.WriteLine("Loading game lobby...");
            SpiderMessage msg = new SpiderMessage(client.GetName() + " has connected.", SpiderMessageType.String, "chat");
            client.SendMessage(msg, Lidgren.Library.Network.NetChannel.ReliableUnordered);
            
            // Send name
            msg = new SpiderMessage(client.GetName(), SpiderMessageType.String, "name");
            client.SendMessage(msg, Lidgren.Library.Network.NetChannel.ReliableUnordered);   
			
			// if hosting, tell itself that it's ready
			if (lobbyMode == LobbyMode.Hosting)
			{
				btnStart.Visible = true;
				client.SendMessage(new SpiderMessage(1, SpiderMessageType.Int, "ready"),
					NetChannel.ReliableUnordered);
			}
        }

		/// <summary>
		/// send a ready/not ready message to the server
		/// only applies to LobbyMode.Client only, since only they have ready checks
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        private void chkReady_CheckedChanged(object sender, EventArgs e) {
            int chk = chkReady.Checked ? 1 : 0;
            String strChk = chkReady.Checked ? "ready" : "not ready";

			// send the ready message to the server
            SpiderMessage msg = new SpiderMessage(chk, SpiderMessageType.Int, "ready");
            client.SendMessage(msg, Lidgren.Library.Network.NetChannel.ReliableUnordered);

			// send the chat message to everyone (via the server)
            msg = new SpiderMessage(client.GetName() + " is " + strChk + " to begin.", SpiderMessageType.String, "chat");
            client.SendMessage(msg, Lidgren.Library.Network.NetChannel.ReliableUnordered);
        }

		/// <summary>
		/// change the game mode 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        private void cmbGameMode_SelectedIndexChanged(object sender, EventArgs e) {
			if (lobbyMode == LobbyMode.ClientOnly)
				return;

            //send out the message
            SpiderMessage msg = new SpiderMessage(cmbGameMode.SelectedIndex, SpiderMessageType.Int, "mode");
            server.SendMessage(msg, Lidgren.Library.Network.NetChannel.ReliableUnordered);
            
            //parse selection into GameMode
            server.GameMode = (GameMode)Enum.Parse(typeof(GameMode), cmbGameMode.Items[cmbGameMode.SelectedIndex].ToString().Replace(" ", ""));

            //team or solo play?
            if (server.GameMode != GameMode.Deathmatch && server.GameMode != GameMode.KingOfTheAsteroid) 
                cmbTeam.Enabled = true;
            else
                cmbTeam.Enabled = false;
        }

		/// <summary>
		/// start the game
		/// only valid for hosting mode
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        private void btnStart_Click(object sender, EventArgs e) {
			if (lobbyMode == LobbyMode.ClientOnly)
				return;
			
            //send game start messages
            SpiderMessage msg = new SpiderMessage("Game starting...", SpiderMessageType.String, "chat");
            server.SendMessage(msg, Lidgren.Library.Network.NetChannel.Sequenced1);
            msg = new SpiderMessage("", SpiderMessageType.String, "start");
            server.SendMessage(msg, Lidgren.Library.Network.NetChannel.Sequenced1);

            //set countdown timer
            /*
			gameStartTime = timerTicks * timer.Interval;
            gameStarting = true;
            rtxtChatWindow.Text += "\nGame starting...";
            rtxtChatWindow.Select(rtxtChatWindow.Text.Length + 1, 2);
            rtxtChatWindow.ScrollToCaret();
			*/

			// disable other buttons and boxes
            cmbGameMode.Enabled = false;
            cmbTeam.Enabled = false;
            btnStart.Enabled = false;
        }

		/// <summary>
		/// update the team id to reflect the id box
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        private void cmbTeam_SelectedIndexChanged(object sender, EventArgs e) {
            client.Team = (GameTeam)Enum.Parse(typeof(GameTeam), cmbTeam.Items[cmbTeam.SelectedIndex].ToString().Replace(" ", ""));
        }

		private void frmGameLobby_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (!gameStarting)
			{
				client.Dispose();
				if (lobbyMode == LobbyMode.Hosting)
					server.Dispose();
			}
		}
    }   
}
