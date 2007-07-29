/*
 * Created by SharpDevelop.
 * User: Brian Go
 * Date: 6/13/2007
 * Time: 3:30 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using Lidgren.Library.Network;


namespace SpiderEngine
{
	/// <summary>
	/// A simple network engine for a multiplayer game.
	/// </summary>
	public class Spider
	{
		public const int MAX_CONNECTIONS = 10;
		public const int DEFAULT_PORT = 30803;
		
		private NetAppConfiguration spiderConfig;
		private NetLog spiderLog;
		private NetBase spiderNet;
		private SpiderType spiderType;
		private String spiderName;
		
		private Queue localSessionQueue;
		private Queue messageQueue;
        private Queue disconnectQueue;

        public NetConnectionStatus Status;

		private ArrayList clients;
		/// <summary>
		/// Initializes the network engine.
		/// </summary>
		/// <param name="type">Client or Server</param>
		/// <param name="strName">The name of the user</param>
		public Spider(SpiderType type, String strName)
		{
			spiderName = strName;
			spiderType = type;
			spiderConfig = new NetAppConfiguration("ymfas",DEFAULT_PORT);
            spiderLog = new NetLog();
            spiderLog.OutputFileName = "YMFAS Net Log (Port 30803).html";
			localSessionQueue = new Queue(50);
            messageQueue = new Queue(50);
            disconnectQueue = new Queue(50);
			
			if(type == SpiderType.Server){
				spiderConfig.MaximumConnections = MAX_CONNECTIONS;
				spiderConfig.ServerName = spiderName + "'s Game";
				spiderNet = new NetServer(spiderConfig,spiderLog);
				spiderNet.StatusChanged += new EventHandler<NetStatusEventArgs>(SpiderNet_StatusChangedHandler);
				clients = new ArrayList();
			}
			else{
				spiderNet = new NetClient(spiderConfig,spiderLog);
			}
		}
		
		/// <summary>
		/// Shuts down the network connection.
		/// </summary>
		public void Destroy()
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
		/// Initiates a search for all available local hosted game sessions. (Client-only)
		/// </summary>
		public void SearchSessions()
		{
			if(spiderNet == null || spiderType == SpiderType.Server){
				return;
			}
			else{
				((NetClient)spiderNet).ServerDiscovered += new EventHandler<NetServerDiscoveredEventArgs>(SpiderNet_ServerDiscoveredHandler);
				((NetClient)spiderNet).DiscoverLocalServers(DEFAULT_PORT);
			}
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
			if(spiderType == SpiderType.Client){
				((NetClient)spiderNet).Connect(hostIP, DEFAULT_PORT);
			}
		}
		
		public void Connect(String hostIP){
			if(spiderType == SpiderType.Client){
				((NetClient)spiderNet).Connect(hostIP, DEFAULT_PORT);
			}
		}
		
		/// <summary>
		/// Receives new messages into the message queue.  Should be called regularly.
		/// </summary>
		public void Update()
		{
            if (this.spiderType == SpiderType.Client) {
                this.Status = ((NetClient)spiderNet).Status;
            }
         
            if (spiderType == SpiderType.Client)
            {
                ((NetClient)spiderNet).Heartbeat();
                
            }
            else{
                ((NetServer)spiderNet).Heartbeat();
            }
            NetMessage incMsg;
            if (spiderType == SpiderType.Client) {
                while ((incMsg = ((NetClient)spiderNet).ReadMessage()) != null) {
                    Console.Out.WriteLine("got a msg");
                    messageQueue.Enqueue(incMsg);
                }
            }
            else {
                while ((incMsg = ((NetServer)spiderNet).ReadMessage()) != null) {
                    Console.Out.WriteLine("got a msg");
                    messageQueue.Enqueue(incMsg);
                }
            }
		}
		
		/// <summary>
		/// Gets the next message on the message queue as a spiderMessage
		/// </summary>
		/// <returns>The next message on the message queue</returns>
		public SpiderMessage GetNextMessage(){
			if(messageQueue.Count == 0){ return null; }
            Console.Out.WriteLine("hai");
			NetMessage msg = (NetMessage)(messageQueue.Dequeue());
			SpiderMessage result;
			
			try{
				result = new SpiderMessage(msg);
                Console.Out.WriteLine(result.ToString());
                
			}
			catch(Exception e){
                Console.Out.WriteLine(e.ToString());
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

            Console.Out.WriteLine(msg.ReadString());
				
			if(spiderType == SpiderType.Client){
				((NetClient)spiderNet).SendMessage(msg, deliveryType);
			}
			else{
				((NetServer)spiderNet).Broadcast(msg, deliveryType);
			}	
		}

        /// <summary>
        /// Sends the given message with the given UDP delivery type to the targetConnection (Server only)
        /// </summary>
        /// <param name="message">The message to be send</param>
        /// <param name="deliveryType">The UDP delivery type</param>
        /// <param name="targetConnection">The target connection</param>
        public void SendMessage(SpiderMessage message, NetChannel deliveryType, NetConnection targetConnection) {
            if (spiderType == SpiderType.Client)
                return;
            NetMessage msg = new NetMessage();
            msg.Write(message.ToString());

            Console.Out.WriteLine(msg.ReadString());

            ((NetServer)spiderNet).SendMessage(msg, targetConnection, deliveryType);
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
	
	public class SpiderMessage{
		private object data;
		private SpiderMessageType type;
		private String label;
        private IPAddress senderIP;
        private NetConnection connection;
		
		/// <summary>
		/// Creates a new network message.  Do not use pipes ('|') in any of the parameters.
		/// </summary>
		/// <param name="msgContents">The data to be stored in the message.  Must have type msgType.</param>
		/// <param name="msgType">The type of msgContents</param>
		/// <param name="msgLabel">The label for this message</param>
		public SpiderMessage(object msgContents, SpiderMessageType msgType, String msgLabel){
			data = msgContents;
			type = msgType;
			label = msgLabel;
		}
		
		/// <summary>
		/// Creates a network message by decoding a NetMessage
		/// </summary>
		/// <param name="msg">The NetMessage to be decoded</param>
		public SpiderMessage(NetMessage msg) {
			//This is the exception we throw if something goes wrong
			Exception e = new Exception("Could not read message");
			
			String contents = msg.ReadString();
            
			//Parse
			String strType = contents.Substring(0,contents.IndexOf('|'));
			contents = contents.Substring(contents.IndexOf('|')+1);
			String strLabel = contents.Substring(0,contents.IndexOf('|'));
			contents = contents.Substring(contents.IndexOf('|')+1);
			String strData = contents.Substring(0,contents.IndexOf('|'));

            connection = msg.Sender;
            senderIP = msg.Sender.RemoteEndpoint.Address;
			//Create
			switch(strType){
				case "String":
					data = strData;
					type = SpiderMessageType.String;
					break;
				case "Int":
					data = Convert.ToInt32(strData);
					type = SpiderMessageType.Int;
					break;
				case "Double":
					data = Convert.ToDouble(strData);
					type = SpiderMessageType.Double;
					break;
				default:
					throw e;
			}
			label = strLabel;
		}
		
		/// <summary>
		/// Returns the message information as a string.
		/// </summary>
		/// <returns>A pipe-delimeted string encoding the type, label, data, and source IP.</returns>
		public override String ToString(){
			return type.ToString() + "|" + label + "|" + data.ToString() + "|" +((senderIP == null)? "" : senderIP.ToString());
		}
		
		/// <summary>
		/// Retrieves the data stored in this message.
		/// </summary>
		/// <returns>The message data</returns>
		public object GetData(){
			return data;
		}

        /// <summary>
        /// Retrieves the NetConnection to the sender of the message. Only non-null if the NetMessage constructor was used.
        /// </summary>
        /// <returns>A NetConnection to the sender of the message, or null if not constructed from a NetMessage</returns>
        public NetConnection GetConnection() {
            return connection;
        }
		
		/// <summary>
		/// Gets the type of data stored in the message.
		/// </summary>
		/// <returns>The type of data stored.</returns>
		public SpiderMessageType GetMessageType(){
			return type;
		}
		
		
		/// <summary>
		/// Gets the label for this message.
		/// </summary>
		/// <returns>A String containing the message label</returns>
		public String GetLabel(){
			return label;
		}
		
		/// <summary>
		/// Gets the IP address of the message source
		/// </summary>
		/// <returns>The IP address of the message source</returns>
		public IPAddress GetIP(){
			return senderIP;
		}
	}
	
	public enum SpiderMessageType{String, Int, Double};
	public enum SpiderType{Client,Server};
}
