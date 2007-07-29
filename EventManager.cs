using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;

namespace ymfas {
    public delegate void GameEventFiringHandler(GameEvent e);

    public abstract class GameEvent { 
        /// <summary>
        /// A String representation of the data contained in this object.  Must be parseable by SetDataFromString.
        /// </summary>
        /// <returns>The data contained in the object as a String</returns>
        public abstract override String ToString();

        /// <summary>
        /// Sets the data based on a String using the same format as returned from ToString()
        /// </summary>
        /// <param name="data">A parseable string containing the object data</param>
        public abstract void SetDataFromString(String data);

        public static event GameEventFiringHandler FiringEvent;

        /// <summary>
        /// Fires the FiringEvent event
        /// </summary>
        public void FireEvent() {
            FiringEvent(this);
        }

        public Lidgren.Library.Network.NetChannel DeliveryType;
    }

    public class EventManager{
        private SpiderEngine.Spider NetworkEngine;
        private Queue<GameEvent> EventQueue;
        private Thread MessagePolling;

        /// <summary>
        /// Creates a new EventManager, which will begin polling the network for event messages
        /// </summary>
        /// <param name="networkEngine">The network engine to monitor for events</param>
        public EventManager(SpiderEngine.Spider networkEngine) {
            NetworkEngine = networkEngine;
            EventQueue = new Queue<GameEvent>();

            MessagePolling = new Thread(PollMessages);
        }

        /// <summary>
        /// Polls the message queue of the network engine indefinitely, parsing out events and filling the event
        /// </summary>
        private void PollMessages() {
            this.NetworkEngine.Update();
            SpiderEngine.SpiderMessage msg;
            while(true){
                if ((msg = this.NetworkEngine.GetNextMessage()) != null) {
                    try {
                        //The type is contained in the label
                        Type eventType = Type.GetType(msg.GetLabel());
                        //Create an event object
                        GameEvent msgEvent = (GameEvent)(new object());
                        msgEvent = (GameEvent)Convert.ChangeType(msgEvent, eventType);

                        //Get the message data
                        String msgData = (String)msg.GetData();
               
                        //Add this info to the event, casting it to the correct type
                        msgEvent.SetDataFromString(msgData);

                        //Add this event to the queue
                        lock (EventQueue) {
                            EventQueue.Enqueue(msgEvent);
                        }
                    }
                    catch (Exception e) { }
                }
            }
        }

        /// <summary>
        /// Processes all events currently in the queue
        /// </summary>
        public void Update() {
            GameEvent[] tempArray = new GameEvent[EventQueue.Count];
            lock (EventQueue) {
                EventQueue.CopyTo(tempArray, 0);
            }

            //Process each event
            for (int i = 0; i < tempArray.Length; i++) {
                tempArray[i].FireEvent();
            }
        }

        /// <summary>
        /// Sends a game event over the network.  If the caller is a server, it will be broadcast, otherwise it will be sent to the server.
        /// </summary>
        /// <param name="e">The game event to be sent</param>
        public void SendEvent(GameEvent e) {
            SpiderEngine.SpiderMessage msg = new SpiderEngine.SpiderMessage(e.ToString(), SpiderEngine.SpiderMessageType.String, e.GetType().ToString());

            //default delivery type is unreliable unordered
            if (e.DeliveryType == null) {
                e.DeliveryType = Lidgren.Library.Network.NetChannel.Unreliable;
            }

            NetworkEngine.SendMessage(msg, e.DeliveryType);
        }
    }
}
