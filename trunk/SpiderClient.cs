using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using Lidgren.Library.Network;

namespace Ymfas
{
	/// <summary>
	/// A simple client-side network engine for a multiplayer game.
	/// </summary>
	public class SpiderClient : SpiderBase, IDisposable
	{
		public const int MAX_CONNECTIONS = 10;
		public const int DEFAULT_PORT = 30803;
		
		private NetAppConfiguration spiderConfig;
		private NetLog spiderLog;
		private NetClient spiderNet;
		private String spiderName;
		
		private Queue localSessionQueue;
		private Queue messageQueue;
        private Queue disconnectQueue;

		public NetConnectionStatus Status
		{
			get { return spiderNet.Status; }
		}

		/// <summary>
		/// Initializes the network engine.
		/// </summary>
		/// <param name="strName">The name of the user</param>
		public SpiderClient(String strName)
		{
			spiderName = strName;
			spiderConfig = new NetAppConfiguration("ymfas",DEFAULT_PORT);
            spiderLog = new NetLog();
            spiderLog.OutputFileName = "YMFAS Net Log (Port " + DEFAULT_PORT + ").html";
			localSessionQueue = new Queue(50);
            messageQueue = new Queue(50);
            disconnectQueue = new Queue(50);
			
			spiderNet = new NetClient(spiderConfig,spiderLog);
		}
		
		/// <summary>
		/// Shuts down the network connection.
		/// </summary>
		public void Dispose()
		{
			spiderNet.Shutdown("");
		}
		

        /// <summary>
        /// Returns an IP address that has disconnected from the server or null if all disconnects have been handled.
        /// </summary>
        /// <returns>The first IP whose disconnect has not yet been handled</returns>
        public IPAddress GetDisconnectedIP() {
            if (disconnectQueue.Count == 0) { return null; }
            return (IPAddress)disconnectQueue.Dequeue();
        }

		
		/// <summary>
		/// Initiates a search for all available local hosted game sessions. (Client-only)
		/// </summary>
		public void SearchSessions()
		{
			if (spiderNet == null)
				return;

			spiderNet.ServerDiscovered += new EventHandler<NetServerDiscoveredEventArgs>(SpiderNet_ServerDiscoveredHandler);
			spiderNet.DiscoverLocalServers(DEFAULT_PORT);
		}		
		
		/// <summary>
		/// Handles the "ServerDiscovered" event for the client by appending the server information to the queue.
		/// </summary>
		private void SpiderNet_ServerDiscoveredHandler(object sender, NetServerDiscoveredEventArgs e)
		{
            Console.Out.WriteLine(e.ToString());
			localSessionQueue.Enqueue(e.ServerInformation);
		}
		
		/// <summary>
		/// Gets the information of a local server on the queue of discovered servers.
		/// </summary>
		/// <returns>The NetServerInfo of the first local server on the queue</returns>
		public NetServerInfo GetLocalSession(){
			if(localSessionQueue.Count == 0){return null;}
			return (NetServerInfo)localSessionQueue.Dequeue();
		}
		

		/// <summary>
		/// Connects a client to a specified server on the default port.
		/// </summary>
		/// <param name="hostIP">The host server IP</param>
		public void Connect(IPAddress hostIP)
		{
			spiderNet.Connect(hostIP, DEFAULT_PORT);
		}
		
		public void Connect(String hostIP)
		{
			spiderNet.Connect(hostIP, DEFAULT_PORT);
		}
		
		/// <summary>
		/// Receives new messages into the message queue.  Should be called regularly.
		/// </summary>
		public void Update()
		{
            //this.Status = spiderNet.Status;
            spiderNet.Heartbeat();

            NetMessage incMsg;

            while ((incMsg = spiderNet.ReadMessage()) != null) {
                messageQueue.Enqueue(incMsg);
            }
		}
		
		/// <summary>
		/// Gets the next message on the message queue as a spiderMessage
		/// </summary>
		/// <returns>The next message on the message queue</returns>
		public SpiderMessage GetNextMessage(){
			if(messageQueue.Count == 0){ return null; }
			NetMessage msg = (NetMessage)(messageQueue.Dequeue());
			SpiderMessage result;
			
			try{
				result = new SpiderMessage(msg);
                
			}
			catch(Exception e){
				return this.GetNextMessage();
			}
			
			return result;
			
		}
		
		/// <summary>
		/// Returns the name that was specified during the constructor
		/// </summary>
		/// <returns></returns>
		public String GetName(){
			return this.spiderName;
		}

        /// <summary>
        /// Sends the given message using the given UDP delivery type.  A client will send to the server, a server will broadcast.
        /// </summary>
        /// <param name="message">The message to be sent</param>
        /// <param name="deliveryType">The UDP delivery type</param>
		public void SendMessage(SpiderMessage message, NetChannel deliveryType){
			NetMessage msg = new NetMessage();
			msg.Write(message.ToString());
				
			spiderNet.SendMessage(msg, deliveryType);
		}

		/// TODO: server only???
        /// <summary>
        /// Get computer name from IP address
        /// </summary>
        /// <param name= " HostName " > the IP address which acquires computer name </param>
        /// <returns> computer name (character string) </returns>
        public string GetHostNameFromIP(string IPAddress)
        {
            IPHostEntry IPHstEnt;
            string strServer;

            try
            {
                // drawing up IPHostEntry instance with the IP address which is appointed
                IPHstEnt = Dns.GetHostEntry(IPAddress);
                strServer = IPHstEnt.HostName;
            }
            catch (System.Net.Sockets.SocketException sockEx)
            {
                // HostName cannot be resolved
                strServer = "";
				System.Console.WriteLine(sockEx.Message);
            }
            catch (System.Security.SecurityException secuEx)
            {
                // no access permission
                strServer = "";
				System.Console.WriteLine(secuEx.Message);
            }

            return strServer;
        }
		
	}
	
	
}
