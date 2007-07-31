using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;

namespace Ymfas {
    public partial class frmGameLobby : Form {
        private const int PLAYERLIST_UPDATE_INTERVAL = 1000;
        private const int GAMESTART_INTERVAL = 1000;
        private int gameStartTime;
        private int timerTicks;
        private int gameStartCount;
        private ArrayList playersReady;
        private ArrayList playersNotReady;
        private bool gameStarting;
        private int idTicketCounter;

        public frmGameLobby() {
            InitializeComponent();
            timerTicks = 0;
            playersReady = new ArrayList();
            playersNotReady = new ArrayList();
            gameStartCount = 3;
            gameStarting = false;

            cmbGameMode.SelectedIndex = 0;
            cmbTeam.SelectedIndex = 0;

            if (NetworkEngine.EngineType == SpiderEngine.SpiderType.Server) {
                chkReady.Visible = false;
                cmbGameMode.Enabled = true;
                NetworkEngine.PlayerId = 0;
                idTicketCounter = 1;
            }
        }

        private void timer_Tick(object sender, EventArgs e) {
            timerTicks++;

            //game starting?
            if (gameStarting) {
                if ((timerTicks * timer.Interval - gameStartTime) % GAMESTART_INTERVAL == 0) {
                    if (gameStartCount == 0) {
                        //this.Visible = false;

                        Console.Out.WriteLine("if the network engine fits you must aquit");
                        System.Windows.Forms.Application.Exit();
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

        private void txtChatInput_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {
                //send chat message
                SpiderEngine.SpiderMessage msg = new SpiderEngine.SpiderMessage(NetworkEngine.Engine.GetName() + ": " + txtChatInput.Text, SpiderEngine.SpiderMessageType.String, "chat");
                NetworkEngine.Engine.SendMessage(msg, Lidgren.Library.Network.NetChannel.ReliableUnordered);

                //if host then display message (no bounce)
                if (NetworkEngine.EngineType == SpiderEngine.SpiderType.Server) {
                    rtxtChatWindow.Text += "\n"+ NetworkEngine.Engine.GetName() + ": " + txtChatInput.Text;
                    rtxtChatWindow.Select(rtxtChatWindow.Text.Length + 1, 2);
                    rtxtChatWindow.ScrollToCaret();
                }

                //clear input
                txtChatInput.Multiline = false;
                txtChatInput.Text = null;
                txtChatInput.Multiline = true;

                e.Handled = true;

            }
        }

        private void frmGameLobby_Load(object sender, EventArgs e) {
            if (NetworkEngine.EngineType == SpiderEngine.SpiderType.Server) {
                btnStart.Visible = true;
            }
            else {
                //Chat connect message
                SpiderEngine.SpiderMessage msg = new SpiderEngine.SpiderMessage(NetworkEngine.Engine.GetName() + " has connected.", SpiderEngine.SpiderMessageType.String, "chat");
                NetworkEngine.Engine.SendMessage(msg, Lidgren.Library.Network.NetChannel.ReliableUnordered);
                
                //Send name
                msg = new SpiderEngine.SpiderMessage(NetworkEngine.Engine.GetName(), SpiderEngine.SpiderMessageType.String, "name");
                NetworkEngine.Engine.SendMessage(msg, Lidgren.Library.Network.NetChannel.ReliableUnordered);   
                
            }
        }

        private void chkReady_CheckedChanged(object sender, EventArgs e) {
            int chk = chkReady.Checked ? 1 : 0;
            String strChk = chkReady.Checked ? "ready" : "not ready";
            SpiderEngine.SpiderMessage msg = new SpiderEngine.SpiderMessage(chk, SpiderEngine.SpiderMessageType.Int, "ready");
            NetworkEngine.Engine.SendMessage(msg, Lidgren.Library.Network.NetChannel.ReliableUnordered);
            msg = new SpiderEngine.SpiderMessage(NetworkEngine.Engine.GetName() + " is " + strChk + " to begin.", SpiderEngine.SpiderMessageType.String, "chat");
            NetworkEngine.Engine.SendMessage(msg, Lidgren.Library.Network.NetChannel.ReliableUnordered);
        }

        private void cmbGameMode_SelectedIndexChanged(object sender, EventArgs e) {
            if (NetworkEngine.EngineType == SpiderEngine.SpiderType.Server) {
                //send out the message
                SpiderEngine.SpiderMessage msg = new SpiderEngine.SpiderMessage(cmbGameMode.SelectedIndex, SpiderEngine.SpiderMessageType.Int, "mode");
                NetworkEngine.Engine.SendMessage(msg, Lidgren.Library.Network.NetChannel.ReliableUnordered);
                
                //parse selection into GameMode
                NetworkEngine.GameMode = (GameMode)Enum.Parse(typeof(GameMode), cmbGameMode.Items[cmbGameMode.SelectedIndex].ToString().Replace(" ", ""));

                //team or solo play?
                if (NetworkEngine.GameMode != GameMode.Deathmatch && NetworkEngine.GameMode != GameMode.KingOfTheAsteroid) {
                    cmbTeam.Enabled = true;
                }
                else {
                    cmbTeam.Enabled = false;
                }
            }

        }

        private void btnStart_Click(object sender, EventArgs e) {
            //send game start messages
            if (NetworkEngine.EngineType == SpiderEngine.SpiderType.Server) {
                SpiderEngine.SpiderMessage msg = new SpiderEngine.SpiderMessage("Game starting...", SpiderEngine.SpiderMessageType.String, "chat");
                NetworkEngine.Engine.SendMessage(msg, Lidgren.Library.Network.NetChannel.Sequenced1);
                msg = new SpiderEngine.SpiderMessage("", SpiderEngine.SpiderMessageType.String, "start");
                NetworkEngine.Engine.SendMessage(msg, Lidgren.Library.Network.NetChannel.Sequenced1);

                //set countdown timer
                gameStartTime = timerTicks * timer.Interval;
                gameStarting = true;
                rtxtChatWindow.Text += "\nGame starting...";
                rtxtChatWindow.Select(rtxtChatWindow.Text.Length + 1, 2);
                rtxtChatWindow.ScrollToCaret();

                cmbGameMode.Enabled = false;
                cmbTeam.Enabled = false;
                btnStart.Enabled = false;

            }
        }

        private void cmbTeam_SelectedIndexChanged(object sender, EventArgs e) {
            NetworkEngine.Team = (GameTeam)Enum.Parse(typeof(GameTeam), cmbTeam.Items[cmbTeam.SelectedIndex].ToString().Replace(" ", ""));
        }

		private void frmGameLobby_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (!gameStarting)
				NetworkEngine.Engine.Destroy();
		}
    }   
}