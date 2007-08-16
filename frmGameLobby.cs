using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
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
		private LobbyMode lobbyMode = LobbyMode.ClientOnly;

        public frmGameLobby(YmfasClient _client, YmfasServer _server) 
		{
			client = _client;
			server = _server;
			lobbyMode = LobbyMode.Hosting;
			Initialize();

			chkReady.Visible = false;
			cmbGameMode.Enabled = true;
			server.PlayerId = 0;
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
            NetworkEngine.Engine.Update();
            SpiderEngine.SpiderMessage msg = NetworkEngine.Engine.GetNextMessage();
            while (msg != null) {
                if (NetworkEngine.EngineType == SpiderEngine.SpiderType.Server) {

                    //server message check
                    Console.Out.WriteLine(msg.GetLabel());
                    switch (msg.GetLabel()) {
                        case "chat":
                            //display
                            rtxtChatWindow.Text += "\n" + (String)msg.GetData();
                            rtxtChatWindow.Select(rtxtChatWindow.Text.Length + 1, 2);
                            rtxtChatWindow.ScrollToCaret();
                            //bounce
                            NetworkEngine.Engine.SendMessage(msg,Lidgren.Library.Network.NetChannel.ReliableUnordered);
                            break;
                        case "name":
                            //newly connected player is identifying himself                            
                            NetworkEngine.PlayerIPs.Add(msg.GetIP(), ((String)msg.GetData()) + " [" + msg.GetIP() + "]");
                            NetworkEngine.PlayerIdsByIP.Add(msg.GetIP(), idTicketCounter);
                            NetworkEngine.PlayerNamesById.Add(idTicketCounter, (String)msg.GetData());
                            playersNotReady.Add(((String)msg.GetData()) + " [" + msg.GetIP() + "]");

                            //reply with a player identifier
                            SpiderEngine.SpiderMessage responseMsg = new SpiderEngine.SpiderMessage(idTicketCounter, SpiderEngine.SpiderMessageType.Int, "id");
                            idTicketCounter++;
                            NetworkEngine.Engine.SendMessage(responseMsg, Lidgren.Library.Network.NetChannel.Ordered1, msg.GetConnection());

                            btnStart.Enabled = false;
                            break;
                        case "ready":
                            Console.Out.WriteLine("value : " + msg.GetData());
                            //player is ready
                            if (((int)msg.GetData()) == 1) {
                                for (int i = 0; i < playersNotReady.Count; i++) {
                                    Console.Out.WriteLine(((String)playersNotReady[i]) + " ---- " + ((String)playersNotReady[i]).IndexOf(msg.GetIP().ToString()));
                                    if (((String)playersNotReady[i]).IndexOf(msg.GetIP().ToString()) != -1) {
                                        
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
                                    if (((String)playersReady[i]).IndexOf(msg.GetIP().ToString()) != -1) {
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
                            rtxtChatWindow.Text += "\nUnknown network message recieved.";
                            break;
                    }

                }
                else {
                    // client message check
                    switch (msg.GetLabel()) {
                        case "chat":
                            rtxtChatWindow.Text += "\n" + (String)msg.GetData();
                            rtxtChatWindow.Select(rtxtChatWindow.Text.Length + 1, 2);
                            rtxtChatWindow.ScrollToCaret();
                            break;
                        case "players":
                            Console.Out.WriteLine("playerlist update");
                            //Clear & Set player list
                            lstPlayers.Items.Clear();
                            String players = (String)(msg.GetData());

                            while (!players.Equals("")) {
                                int i = players.IndexOf('\n');
                                lstPlayers.Items.Add(players.Substring(0, i));
                                players = players.Substring(i+1);
                            }
                            break;
                        case "id":
                            NetworkEngine.PlayerId = (int)msg.GetData();
                            break;
                        case "mode":
                            try {
                                cmbGameMode.SelectedIndex = (int)msg.GetData();
                                chkReady.Checked = false;

                                //parse selection into GameMode
                                NetworkEngine.GameMode = (GameMode)Enum.Parse(typeof(GameMode), cmbGameMode.Items[cmbGameMode.SelectedIndex].ToString().Replace(" ", ""));
                            }
                            catch (Exception err) {
								System.Console.WriteLine(err.Message);
                            }

                            //team or solo play?
                            if (NetworkEngine.GameMode != GameMode.Deathmatch && NetworkEngine.GameMode != GameMode.KingOfTheAsteroid) {
                                cmbTeam.Enabled = true;
                            }
                            else {
                                cmbTeam.Enabled = false;
                            }
                            break;
                        case "start":
                            cmbTeam.Enabled = false;
                            chkReady.Enabled = false;
                            gameStartTime = timerTicks * timer.Interval;
                            gameStarting = true;
                            break;
                        default:
                            rtxtChatWindow.Text += "\nUnknown network message recieved.";
                            break;
                    }
                }

                //Keep processing queued messages
                msg = NetworkEngine.Engine.GetNextMessage();
            }

            //server management items
            if (NetworkEngine.EngineType == SpiderEngine.SpiderType.Server) {
                IPAddress disconIP = NetworkEngine.Engine.GetDisconnectedIP();
                while (disconIP != null) {
                    if(NetworkEngine.PlayerIPs.ContainsKey(disconIP)){
                        //Send chat message
                        SpiderEngine.SpiderMessage message = new SpiderEngine.SpiderMessage(((String)NetworkEngine.PlayerIPs[disconIP]) + " has disconnected.", SpiderEngine.SpiderMessageType.String, "chat");
                        NetworkEngine.Engine.SendMessage(message, Lidgren.Library.Network.NetChannel.ReliableUnordered);

                        //Add to host chat window
                        rtxtChatWindow.Text += "\n" + ((String)NetworkEngine.PlayerIPs[disconIP]) + " has disconnected.";
                        rtxtChatWindow.Select(rtxtChatWindow.Text.Length + 1, 2);
                        rtxtChatWindow.ScrollToCaret();

                        //Remove key from hashtable
                        NetworkEngine.PlayerIPs.Remove(disconIP);
                        NetworkEngine.PlayerNamesById.Remove(NetworkEngine.PlayerIdsByIP[disconIP]);
                        NetworkEngine.PlayerIdsByIP.Remove(disconIP);

                        //Keep processing disconnects
                        disconIP = NetworkEngine.Engine.GetDisconnectedIP();
                    }
                }

                if ((timer.Interval * timerTicks) % PLAYERLIST_UPDATE_INTERVAL == 0) {
                    //Send player list & update host player list
                    lstPlayers.Items.Clear();
                    String playerList = NetworkEngine.Engine.GetName() + " (Game Host)\n";
                    lstPlayers.Items.Add(NetworkEngine.Engine.GetName() + " (Game Host)");

                    Array playerArray = Array.CreateInstance(typeof(String),NetworkEngine.PlayerIPs.Count);
                    NetworkEngine.PlayerIPs.Values.CopyTo(playerArray, 0);

                    
                    for (int i = 0; i < NetworkEngine.PlayerIPs.Count; i++) {
                        playerList += playerArray.GetValue(i) + "\n";
                        lstPlayers.Items.Add(playerArray.GetValue(i));
                    }
                    SpiderEngine.SpiderMessage message = new SpiderEngine.SpiderMessage(playerList, SpiderEngine.SpiderMessageType.String,"players");
                    NetworkEngine.Engine.SendMessage(message, Lidgren.Library.Network.NetChannel.Ordered1);
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
        private void frmGameLobby_Load(object sender, EventArgs e) {
            
			if (lobbyMode == LobbyMode.Host) 
                btnStart.Visible = true;
            
            // Chat connect message
            SpiderMessage msg = new SpiderMessage(client.GetName() + " has connected.", SpiderMessageType.String, "chat");
            client.SendMessage(msg, Lidgren.Library.Network.NetChannel.ReliableUnordered);
            
            // Send name
            msg = new SpiderMessage(client.GetName(), SpiderMessageType.String, "name");
            client.SendMessage(msg, Lidgren.Library.Network.NetChannel.ReliableUnordered);   
         
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
