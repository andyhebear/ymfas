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

    public partial class frmMainSplash : Form {

		enum MainSplashState
		{
			None,
			Searching,
			Connecting
		};

        private frmGameLobby GameLobby;
        public const int MAX_CONNECT_TIME = 10000; //max time to spend connecting in ms
		private MainSplashState splashState;
        private int ticksConnecting;

		private YmfasClient ymfasClient = null;		
		private YmfasServer ymfasServer = null;

        public frmMainSplash() {
            InitializeComponent();
			splashState = MainSplashState.None;
        }

        private void btnExit_Click(object sender, EventArgs e) {
            //Exit the game
			this.DialogResult = DialogResult.Cancel;
			this.Close();
        }

        private void btnHost_Click(object sender, EventArgs e) {
            //Host a game
			// create a client and a server for the game lobby to use
            ymfasServer = new YmfasServer(txtName.Text);
			ymfasClient = new YmfasClient(txtName.Text);
			ymfasClient.Update();
			ymfasClient.Connect(ymfasServer.GetLocalSession().RemoteEndpoint.Address.ToString());

            //Enter lobby
            GameLobby = new frmGameLobby(ymfasClient, ymfasServer);

            GameLobby.ShowDialog();
			// if we actually started a game
			if (GameLobby.DialogResult == DialogResult.OK)
			{
				ymfasClient = GameLobby.Client;
				ymfasServer = GameLobby.Server;
				this.DialogResult = DialogResult.OK;
				this.Close();
			}
        }

        private void btnJoin_Click(object sender, EventArgs e) {
            //Initiate search for servers
            grpServerList.Visible = true;
            ymfasClient = new YmfasClient(txtName.Text);

            btnJoin.Enabled = false;
            btnHost.Enabled = false;
            ymfasClient.SearchSessions();

            //Handle server search in the timer object
			splashState = MainSplashState.Searching;

        }

        private void timer_Tick(object sender, EventArgs e) {
            //looking for servers
            if (splashState == MainSplashState.Searching) {

                Console.Out.WriteLine("searching...");
                //update the engine state
                ymfasClient.Update();
                
                //attempt to find a new server
                Lidgren.Library.Network.NetServerInfo session = ymfasClient.GetLocalSession();
                if (session != null) {
                    String hostname = ymfasClient.GetHostNameFromIP(session.RemoteEndpoint.Address.ToString());
                    lstServers.Items.Add(hostname + " - " + session.RemoteEndpoint.Address.ToString());
                }
            }

            //connecting to a server
            if (splashState == MainSplashState.Connecting) {

                // update the engine state
                ymfasClient.Update();
                
                //time out if too long
                if (ticksConnecting * timer.Interval >= MAX_CONNECT_TIME) {
					splashState = MainSplashState.Searching;
					ymfasClient.Dispose();
					ymfasClient = null;
                    MessageBox.Show("Connection attempt failed.");
                }
                else {
                    ticksConnecting++;

                    //check for successful connection
                    if (ymfasClient.Status == NetConnectionStatus.Connected) {
						splashState = MainSplashState.None;
                         
                        // join lobby
                        GameLobby = new frmGameLobby(ymfasClient);
                        GameLobby.ShowDialog();

						if (GameLobby.DialogResult == DialogResult.OK)
						{
							ymfasClient = GameLobby.Client;
							this.DialogResult = DialogResult.OK;
							this.Close();
						}
                    }
                }
            }
        }

        private void lstServers_SelectedIndexChanged(object sender, EventArgs e) 
		{
            try {
                String temp = ((String)lstServers.Items[lstServers.SelectedIndex]);
                txtConnectIP.Text = temp.Substring(temp.LastIndexOf("-") + 2);
            }
            catch(Exception err) {
				System.Console.WriteLine(err.Message);
            }
        }

        private void btnConnect_Click(object sender, EventArgs e) 
		{
            btnConnect.Enabled = false;
            ymfasClient.Connect(txtConnectIP.Text);

			splashState = MainSplashState.Connecting;
            ticksConnecting = 0;
        }

		public YmfasServer Server
		{
			get { return ymfasServer; }
		}
		public YmfasClient Client
		{
			get { return ymfasClient; }
		}
    }
}