using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;
using Lidgren.Library.Network;

namespace Ymfas
{
	public interface SpiderBaseEngine
	{
		void Update();
	}

	public class SpiderMessage
	{
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
		public SpiderMessage(object msgContents, SpiderMessageType msgType, String msgLabel)
		{
			data = msgContents;
			type = msgType;
			label = msgLabel;
		}

		/// <summary>
		/// Creates a network message by decoding a NetMessage
		/// </summary>
		/// <param name="msg">The NetMessage to be decoded</param>
		public SpiderMessage(NetMessage msg)
		{
			//This is the exception we throw if something goes wrong
			Exception e = new Exception("Could not read message");

			String contents = msg.ReadString();

			//Parse
			String strType = contents.Substring(0, contents.IndexOf('|'));
			contents = contents.Substring(contents.IndexOf('|') + 1);
			String strLabel = contents.Substring(0, contents.IndexOf('|'));
			contents = contents.Substring(contents.IndexOf('|') + 1);
			String strData = contents.Substring(0, contents.IndexOf('|'));

			connection = msg.Sender;
			senderIP = msg.Sender.RemoteEndpoint.Address;
			//Create
			switch (strType)
			{
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
		public override String ToString()
		{
			return type.ToString() + "|" + label + "|" + data.ToString() + "|" + ((senderIP == null) ? "" : senderIP.ToString());
		}

		/// <summary>
		/// Retrieves the data stored in this message.
		/// </summary>
		/// <returns>The message data</returns>
		public object Data
		{
			get { return data; }
		}

		/// <summary>
		/// Retrieves the NetConnection to the sender of the message. Only non-null if the NetMessage constructor was used.
		/// </summary>
		/// <returns>A NetConnection to the sender of the message, or null if not constructed from a NetMessage</returns>
		public NetConnection Connection
		{
			get { return connection; }
		}

		/// <summary>
		/// Gets the type of data stored in the message.
		/// </summary>
		/// <returns>The type of data stored.</returns>
		public SpiderMessageType MessageType
		{
			get { return type; }
		}


		/// <summary>
		/// Gets the label for this message.
		/// </summary>
		/// <returns>A String containing the message label</returns>
		public String GetLabel()
		{
			return label;
		}

		/// <summary>
		/// Gets the IP address of the message source
		/// </summary>
		/// <returns>The IP address of the message source</returns>
		public IPAddress GetIP()
		{
			return senderIP;
		}
	}

	public enum SpiderMessageType { String, Int, Double };
}
