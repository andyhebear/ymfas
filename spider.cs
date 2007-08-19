using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;
using Lidgren.Library.Network;

namespace Ymfas
{
	/// <summary>
	/// shared interface for SpiderClient and SpiderServer
	/// </summary>
	public interface SpiderBase
	{
		/// <summary>
		/// grab all incoming messages
		/// </summary>
		void Update();

		/// <summary>
		/// grab the next message from the queue
		/// </summary>
		/// <returns></returns>
		SpiderMessage GetNextMessage();

		/// <summary>
		/// send or broadcast a message
		/// </summary>
		/// <param name="message"></param>
		/// <param name="deliveryType"></param>
		void SendMessage(SpiderMessage message, NetChannel deliveryType);
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

            byte[] contents = msg.ReadBytes(msg.Length);
            type = (SpiderMessageType)contents[0];
            int dataOffset = BitConverter.ToInt32(contents, 1);
            label = Encoding.UTF8.GetString(contents, 1 + sizeof(int), dataOffset - 1 - sizeof(int));

            senderIP = msg.Sender.RemoteEndpoint.Address;
            connection = msg.Sender;

            if (type == SpiderMessageType.Bytes) {
                data = new byte[contents.Length - dataOffset];
                for (int i = 0; i < contents.Length - dataOffset; i++) {
                    ((byte[])data)[i] = contents[i + dataOffset];
                }
            }
            else {
                String tempData = Encoding.UTF8.GetString(contents, dataOffset, contents.Length - dataOffset);
                switch (type) {
                    case SpiderMessageType.Double:
                        data = Convert.ToDouble(tempData);
                        break;
                    case SpiderMessageType.Int:
                        data = Convert.ToInt32(tempData);
                        break;
                    default:
                        data = tempData;
                        break;
                }
            }

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
        /// Returns the message information as a byte array
        /// </summary>
        /// <returns>Byte 0: Type, 1: Data offset, 5: Label, (Data offset): Data</returns>
        public byte[] ToByteArray() {
            byte[] labelBytes = Encoding.UTF8.GetBytes(label);
            byte[] dataBytes = Encoding.UTF8.GetBytes(data.ToString());
            int size = 1 + sizeof(int) + labelBytes.Length  + ((type == SpiderMessageType.Bytes) ? ((byte[])data).Length : dataBytes.Length);
            byte[] retval = new byte[size];

            retval[0] = (byte)type;
            BitConverter.GetBytes(1 + sizeof(int) + labelBytes.Length).CopyTo(retval, 1);  // index of data start
            labelBytes.CopyTo(retval, 1 + sizeof(int));

            //test
            /*byte[] test = new byte[Encoding.UTF8.GetMaxByteCount(label.Length)];
            for (int i = 0; i < Encoding.UTF8.GetMaxByteCount(label.Length); i++) test[i] = retval[1 + sizeof(int) + i];
            Console.Out.WriteLine("*" + Encoding.UTF8.GetString(test) + "*");*/
            //test


            if (type == SpiderMessageType.Bytes) {
                ((byte[])data).CopyTo(retval, 1 + sizeof(int) + labelBytes.Length);
            }
            else {
                dataBytes.CopyTo(retval, 1 + sizeof(int) + labelBytes.Length);
            }
            return retval;
        }

		#region PublicProperties
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
		public String Label
		{
			get { return label; }
		}

		/// <summary>
		/// Gets the IP address of the message source
		/// </summary>
		/// <returns>The IP address of the message source</returns>
		public IPAddress IP
		{
			get { return senderIP; }
		} 
		#endregion
	}

	public enum SpiderMessageType { String, Int, Double, Bytes };
}