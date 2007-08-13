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

        public static event GameEventFiringHandler FiringEvent;

        /// <summary>
        /// Fires the FiringEvent event
        /// </summary>
        public void FireEvent() {
            if (FiringEvent != null) {
                FiringEvent(this);
            }
        }

        public abstract Lidgren.Library.Network.NetChannel DeliveryType { get; }
    }

    public class EventManager{
        private Queue<GameEvent> EventQueue;
        private Thread MessagePolling;

        /// <summary>
        /// Creates a new EventManager, which will begin polling the network for event messages
        /// </summary>
        /// <param name="networkEngine">The network engine to monitor for events</param>
        public EventManager() {
            EventQueue = new Queue<GameEvent>();

            MessagePolling = new Thread(PollMessages);
            MessagePolling.Start();
        }

        /// <summary>
        /// Polls the message queue of the network engine indefinitely, parsing out events and filling the event
        /// </summary>
        private void PollMessages() {
            

            SpiderEngine.SpiderMessage msg;
            while(true){
				Thread.Sleep(50);
                NetworkEngine.Engine.Update();
                msg = NetworkEngine.Engine.GetNextMessage();
                while(msg != null) {
                    try {
                        Console.Out.WriteLine("got an event!");
                        //The type is contained in the label
                        Type eventType = Type.GetType(msg.GetLabel());
                        Console.Out.WriteLine("the type is " + eventType.ToString());
                        //Create an event object
                        GameEvent msgEvent = (GameEvent)System.Activator.CreateInstance(eventType);

                        //Get the message data
                        String msgData = (String)msg.GetData();
               
                        //Add this info to the event (string to byteArray first)
                        Encoder encoder = Encoding.GetEncoding(28591).GetEncoder();
                        Byte[] byteArray = new Byte[msgData.Length];                   
                        encoder.GetBytes(msgData.ToCharArray(), 0, msgData.Length, byteArray, 0, true);
                        msgEvent.SetDataFromByteArray(byteArray);

                        //Add this event to the queue
                        lock (EventQueue) {
                            EventQueue.Enqueue(msgEvent);
                        }
                    }
                    catch (Exception e) {
                        Util.RecordException(e);
                    }

                    msg = NetworkEngine.Engine.GetNextMessage();
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
                EventQueue.Clear();
            }

            //Process each event
            for (int i = 0; i < tempArray.Length; i++) {
                Console.Out.WriteLine("firing an event");
                tempArray[i].FireEvent();
            }
        }

        /// <summary>
        /// Sends a game event over the network.  If the caller is a server, it will be broadcast, otherwise it will be sent to the server.
        /// </summary>
        /// <param name="e">The game event to be sent</param>
        public void SendEvent(GameEvent e) {
            //latin-1
            Byte[] bytes = e.ToByteArray();
            Decoder decoder = Encoding.GetEncoding(28591).GetDecoder();
            char[] chars = new char[bytes.Length + 1];
            int ignored0;
            int ignored1;
            bool ignored2;
            decoder.Convert(bytes, 0, bytes.Length, chars, 0, bytes.Length, true, out ignored0, out ignored1, out ignored2);
            chars[bytes.Length] = (char)0;  //null-terminate
            String msgString = new String(chars);

            SpiderEngine.SpiderMessage msg = new SpiderEngine.SpiderMessage(msgString, SpiderEngine.SpiderMessageType.String, e.GetType().ToString());
            NetworkEngine.Engine.SendMessage(msg, e.DeliveryType);

            if (NetworkEngine.EngineType == SpiderEngine.SpiderType.Server) {
                e.FireEvent();
            }
        }
    }
}
