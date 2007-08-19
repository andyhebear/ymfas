using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using Lidgren.Library.Network;

namespace Ymfas
{
	/// <summary>
	/// A simple server-side network engine for a multiplayer game.
	/// </summary>
	public class SpiderServer : SpiderBase, IDisposable
	{
		public const int MAX_CONNECTIONS = 10;
		public const int DEFAULT_PORT = 30803;
		
		private NetAppConfiguration spiderConfig;
		private NetLog spiderLog;
		private NetServer spiderNet;
		private String spiderName;
		
		private Queue messageQueue;
        private Queue disconnectQueue;

        public NetConnectionStatus Status;

		private ArrayList clients;

		/// <summary>
		/// Initializes the network engine.
		/// </summary>
		/// <param name="type">Client or Server</param>
		/// <param name="strName">The name of the user</param>
		public SpiderServer(String strName)
		{
			spiderName = strName;
			spiderConfig = new NetAppConfiguration("ymfas",DEFAULT_PORT);
            spiderLog = new NetLog();
            spiderLog.OutputFileName = "YMFAS Net Log (Port 30803).html";
            messageQueue = new Queue(50);
            disconnectQueue = new Queue(50);
			
			spiderConfig.MaximumConnections = MAX_CONNECTIONS;
			spiderConfig.ServerName = spiderName + "'s Game";
			spiderNet = new NetServer(spiderConfig,spiderLog);
			spiderNet.StatusChanged += new EventHandler<NetStatusEventArgs>(SpiderNet_StatusChangedHandler);
			clients = new ArrayList();
		}
		
		/// <summary>
		/// Shuts down the network connection.
		/// </summary>
		public void Dispose()
		{
			spiderNet.Shutdown("");
		}
		
		
		/// <summary>
		/// Handles changes in connection status.
		/// </summary>
		private void SpiderNet_StatusChangedHandler(object Sender, NetStatusEventArgs e)
		{
            
			switch(e.Connection.Status){
				case NetConnectionStatus.Connected:
					clients.Add(e.Connection.RemoteEndpoint.Address);
					break;
				case NetConnectionStatus.Disconnected:
                    Console.Out.WriteLine("someone left the game, yo!");
					clients.Remove(e.Connection.RemoteEndpoint.Address);
                    disconnectQueue.Enqueue(e.Connection.RemoteEndpoint.Address);
					break;
				default:
					break;
			}
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
		/// Receives new messages into the message queue.  Should be called regularly.
		/// </summary>
		public void Update()
		{            
            spiderNet.Heartbeat();

            NetMessage incMsg;
            
            while ((incMsg = spiderNet.ReadMessage()) != null) 
			{
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
        /// Broadcasts the given message using the given UDP delivery type. 
        /// </summary>
        /// <param name="message">The message to be sent</param>
        /// <param name="deliveryType">The UDP delivery type</param>
		public void SendMessage(SpiderMessage message, NetChannel deliveryType){
			NetMessage msg = new NetMessage();
			msg.Write(message.ToString());
				
			spiderNet.Broadcast(msg, deliveryType);
		}

        /// <summary>
        /// Sends the given message with the given UDP delivery type to the targetConnection
        /// </summary>
        /// <param name="message">The message to be send</param>
        /// <param name="deliveryType">The UDP delivery type</param>
        /// <param name="targetConnection">The target connection</param>
        public void SendMessage(SpiderMessage message, NetChannel deliveryType, NetConnection targetConnection) {
            NetMessage msg = new NetMessage();
            msg.Write(message.ToString());

            spiderNet.SendMessage(msg, targetConnection, deliveryType);
        }

		/// <summary>
		/// Gets the ArrayList of client IPAddress
		/// </summary>
		/// <returns>Client IP addresses as an ArrayList</returns>
		public ArrayList GetClients(){
			return this.clients;
		}

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
