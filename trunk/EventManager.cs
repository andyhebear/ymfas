using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;

namespace Ymfas {
    public delegate void GameEventFiringHandler(GameEvent e);

    public abstract class GameEvent { 
        /// <summary>
        /// A byte array representation of the data contained in this object.  Must be parseable by SetDataFromByteArray.
        /// </summary>
        /// <returns>The data contained in the object as a String</returns>
        public abstract Byte[] ToByteArray();

        /// <summary>
        /// Sets the data based on a byte array using the same format as returned from ToByteArray()
        /// </summary>
        /// <param name="byteArray">A parseable string containing the object data</param>
        public abstract void SetDataFromByteArray(Byte[] byteArray);

		//public static event GameEventFiringHandler FiringEvent;

        /// <summary>
        /// Fires the FiringEvent event
        /// </summary>
		public abstract void FireEvent();
		/*
        public virtual void FireEvent() {
            if (FiringEvent != null) {
                FiringEvent(this);
            }
        }
		*/

        public abstract Lidgren.Library.Network.NetChannel DeliveryType { get; }
    }

	public class EventManager
	{
		private SpiderBase net;
		private Queue<GameEvent> EventQueue;
		private Thread MessagePolling;

		/// <summary>
		/// Creates a new EventManager, which will begin polling the network for event messages
		/// </summary>
		/// <param name="networkEngine">The network engine to monitor for events</param>
		public EventManager(SpiderBase _spider)
		{
			net = _spider;
			EventQueue = new Queue<GameEvent>();

			MessagePolling = new Thread(PollMessages);
			MessagePolling.Start();
		}

		/// <summary>
		/// Polls the message queue of the network engine indefinitely, parsing out events and filling the event
		/// </summary>
		private void PollMessages()
		{
			/*SpiderMessage msg = null;
			while (net != null)
			{
				Thread.Sleep(100);
				net.Update();

				while ((msg = net.GetNextMessage()) != null)
				{
					try
					{
						//The type is contained in the label
						Type eventType = Type.GetType(msg.Label);
                        
						//Create an event object
						GameEvent msgEvent = (GameEvent)System.Activator.CreateInstance(eventType);                   

						//Add data to the event                       
                        msgEvent.SetDataFromByteArray((byte[])msg.Data);


						//Add this event to the queue
						lock (EventQueue)
						{
							EventQueue.Enqueue(msgEvent);
						}
					}
					catch (Exception e)
					{
						Util.RecordException(e);
					}
				}
			}*/
		}

		/// <summary>
		/// Processes all events currently in the queue
		/// </summary>
		public void Update()
		{
            net.Update();
            SpiderMessage msg = null;
            while ((msg = net.GetNextMessage()) != null) {
                try {
                    //The type is contained in the label
                    Type eventType = Type.GetType(msg.Label);

                    //Create an event object
                    GameEvent msgEvent = (GameEvent)System.Activator.CreateInstance(eventType);

                    //Add data to the event                       
                    msgEvent.SetDataFromByteArray((byte[])msg.Data);


                    //Add this event to the queue
                    lock (EventQueue) {
                        EventQueue.Enqueue(msgEvent);
                    }
                }
                catch (Exception e) {
                    Util.RecordException(e);
                }
            }

			GameEvent[] tempArray = new GameEvent[EventQueue.Count];
			lock (EventQueue)
			{
				EventQueue.CopyTo(tempArray, 0);
				EventQueue.Clear();
			}

			//Process each event
			for (int i = 0; i < tempArray.Length; i++)
			{
				tempArray[i].FireEvent();
			}
		}

		/// <summary>
		/// Sends a game event over the network.  If the caller is a server, it will be broadcast, otherwise it will be sent to the server.
		/// </summary>
		/// <param name="e">The game event to be sent</param>
		public void SendEvent(GameEvent e)
		{
			SpiderMessage msg = new SpiderMessage(e.ToByteArray(), SpiderMessageType.Bytes, e.GetType().ToString());         
			net.SendMessage(msg, e.DeliveryType);
		}
	}
}
