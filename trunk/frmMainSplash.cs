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

namespace Ymfas {

    public partial class frmMainSplash : Form {
        private frmGameLobby GameLobby;
        public const int MAX_CONNECT_TIME = 10000; //max time to spend connecting in ms
        private Boolean searchingForServers;
        private Boolean connectingToServer;
        private int ticksConnecting;

        public frmMainSplash() {
            InitializeComponent();
            searchingForServers = false;
            connectingToServer = false;
        }

        private void btnExit_Click(object sender, EventArgs e) {
            //Exit the game
            System.Windows.Forms.Application.Exit();
        }

        private void btnHost_Click(object sender, EventArgs e) {
            //Host a game
            NetworkEngine.Engine = new SpiderEngine.Spider(SpiderEngine.SpiderType.Server, txtName.Text);
            NetworkEngine.EngineType = SpiderEngine.SpiderType.Server;
            NetworkEngine.PlayerIPs = new Hashtable();

            //Enter lobby
            GameLobby = new frmGameLobby();
            GameLobby.ShowDialog();
            
        }

        private void btnJoin_Click(object sender, EventArgs e) {
            //Initiate search for servers
            grpServerList.Visible = true;
            NetworkEngine.Engine = new SpiderEngine.Spider(SpiderEngine.SpiderType.Client, txtName.Text);
            NetworkEngine.EngineType = SpiderEngine.SpiderType.Client;
            btnJoin.Enabled = false;
            btnHost.Enabled = false;
            NetworkEngine.Engine.SearchSessions();

            //Handle server search in the timer object
            searchingForServers = true;

        }

        private void timer_Tick(object sender, EventArgs e) {
            //looking for servers
            if (searchingForServers) {
                Console.Out.WriteLine("searching...");
                //update the engine state
                NetworkEngine.Engine.Update();
                
                //attempt to find a new server
                Lidgren.Library.Network.NetServerInfo session = NetworkEngine.Engine.GetLocalSession();
                if (session != null) {
                    String hostname = NetworkEngine.Engine.GetHostNameFromIP(session.RemoteEndpoint.Address.ToString());
                    lstServers.Items.Add(hostname + " - " + session.RemoteEndpoint.Address.ToString());
                }
            }
            //connecting to a server
            if (connectingToServer) {
                //update the engine state
                NetworkEngine.Engine.Update();
                
                //time out if too long
                if (ticksConnecting * timer.Interval >= MAX_CONNECT_TIME) {
                    connectingToServer = false;
                    NetworkEngine.Engine.Destroy();
                    MessageBox.Show("Connection attempt failed.");
                    Application.Restart();
                }
                else {
                    ticksConnecting++;

                    //check for successful connection
                    if (NetworkEngine.Engine.Status == Lidgren.Library.Network.NetConnectionStatus.Connected) {
                        connectingToServer = false;
                         
                        //join lobby
                        GameLobby = new frmGameLobby();
                        GameLobby.ShowDialog();
                    }
                }
            }

            //hidden
            if (!this.Visible) {
                if (GameLobby.Visible == false) {
                    NetworkEngine.Engine.Destroy();
                    Application.Restart();
                }
            }

        }

        private void lstServers_SelectedIndexChanged(object sender, EventArgs e) {
            try {
                String temp = ((String)lstServers.Items[lstServers.SelectedIndex]);
                txtConnectIP.Text = temp.Substring(temp.LastIndexOf("-") + 2);
            }
            catch(Exception err) {
				System.Console.WriteLine(err.Message);
            }
        }

        private void btnConnect_Click(object sender, EventArgs e) {
            searchingForServers = false;
            btnConnect.Enabled = false;
            NetworkEngine.Engine.Connect(txtConnectIP.Text);
            
            connectingToServer = true;
            ticksConnecting = 0;
        }
    }

    
}