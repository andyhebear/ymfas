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
        private int timerTicks;
        private ArrayList playersReady;
        private ArrayList playersNotReady;

        public frmGameLobby() {
            InitializeComponent();
            timerTicks = 0;
            playersReady = new ArrayList();
            playersNotReady = new ArrayList();
            lstPlayers.Items.Add(NetworkEngine.Engine.GetName() + " (Game Host)");
            if (NetworkEngine.EngineType == SpiderEngine.SpiderType.Server) {
                chkReady.Visible = false;
                cmbGameMode.Enabled = true;
            }
        }

        private void timer_Tick(object sender, EventArgs e) {
            timerTicks++;

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
                            NetworkEngine.playerIPs.Add(msg.GetIP(), ((String)msg.GetData()) + " [" + msg.GetIP() + "]");
                            playersNotReady.Add(((String)msg.GetData()) + " [" + msg.GetIP() + "]");
                            lstPlayers.Items.Add(((String)msg.GetData()) + " [" + msg.GetIP() + "]");
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
                                btnStart.Enabled = true;
                            }
                            else {
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
                    if(NetworkEngine.playerIPs.ContainsKey(disconIP)){
                        //Send chat message
                        SpiderEngine.SpiderMessage message = new SpiderEngine.SpiderMessage(((String)NetworkEngine.playerIPs[disconIP]) + " has disconnected.", SpiderEngine.SpiderMessageType.String, "chat");
                        NetworkEngine.Engine.SendMessage(message, Lidgren.Library.Network.NetChannel.ReliableUnordered);

                        //Keep processing disconnects
                        disconIP = NetworkEngine.Engine.GetDisconnectedIP();
                    }
                }

                if ((timer.Interval * timerTicks) % PLAYERLIST_UPDATE_INTERVAL == 0) {
                    //Send player list
                    String playerList = NetworkEngine.Engine.GetName() + " (Game Host)\n";
                    Array playerArray = Array.CreateInstance(typeof(String),NetworkEngine.playerIPs.Count);
                    NetworkEngine.playerIPs.Values.CopyTo(playerArray, 0);
                    for (int i = 0; i < NetworkEngine.playerIPs.Count; i++) {
                        playerList += playerArray.GetValue(i) + "\n";
                    }
                    SpiderEngine.SpiderMessage message = new SpiderEngine.SpiderMessage(playerList, SpiderEngine.SpiderMessageType.String,"players");
                    NetworkEngine.Engine.SendMessage(message, Lidgren.Library.Network.NetChannel.Unreliable);
                }
            }


        }

        private void txtChatInput_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {
                //send chat message
                SpiderEngine.SpiderMessage msg = new SpiderEngine.SpiderMessage(NetworkEngine.Engine.GetName() + " : " + txtChatInput.Text, SpiderEngine.SpiderMessageType.String, "chat");
                NetworkEngine.Engine.SendMessage(msg, Lidgren.Library.Network.NetChannel.ReliableUnordered);

                //if host then display message (no bounce)
                if (NetworkEngine.EngineType == SpiderEngine.SpiderType.Server) {
                    rtxtChatWindow.Text += "\n"+ NetworkEngine.Engine.GetName() + ": " + txtChatInput.Text;
                    rtxtChatWindow.Select(rtxtChatWindow.Text.Length + 1, 2);
                    rtxtChatWindow.ScrollToCaret();
                }

                //clear input
                txtChatInput.Text = "";

            }
        }

        private void frmGameLobby_Load(object sender, EventArgs e) {
            if (NetworkEngine.EngineType == SpiderEngine.SpiderType.Server) {
                btnStart.Visible = true;
                btnStart.Enabled = false;
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

    }
}