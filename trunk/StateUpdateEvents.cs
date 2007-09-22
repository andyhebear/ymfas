using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.ComponentModel;
using Mogre;
using MogreNewt;
using Ymfas;

namespace Ymfas {

    //Event for initializing a ship belonging to a specific player in the game environment
    public class ShipInit : GameEvent {
        public int PlayerId;
        public ShipTypeData ShipType;
        public Vector3 Position;
        public Quaternion Orientation;
        public String PlayerName;

        /// <summary>
        /// Default constructor
        /// </summary>
        public ShipInit() {
            PlayerId = -1;
            ShipType = new ShipTypeData();
            ShipType.Class = ShipClass.Interceptor;
            ShipType.Model = ShipModel.MogreFighter;
            Position = new Vector3();
            Orientation = new Quaternion();
            PlayerName = "";
            
        }

		public static event GameEventFiringHandler FiringEvent;
		public override void FireEvent()
		{
			if (FiringEvent != null)
			{
				FiringEvent(this);
			}
		}

        /// <summary>
        /// Creates the event to initialize a ship
        /// </summary>
        /// <param name="playerId">The owner of the ship</param>
        /// <param name="shipType">The type of the ship</param>
        /// <param name="position">The position of the ship</param>
        /// <param name="orientation">The orientation of the ship</param>
        public ShipInit(int playerId, ShipTypeData shipType, Vector3 position, Quaternion orientation, String playerName) {
            PlayerId = playerId;
            ShipType = shipType;
            Position = position;
            Orientation = orientation;
            PlayerName = playerName;
        }

        public override Lidgren.Library.Network.NetChannel DeliveryType {
            get { return Lidgren.Library.Network.NetChannel.Ordered1; }
        }

        public override Byte[] ToByteArray() {
            Serializer s = new Serializer();
            s.Add(PlayerId);
            s.Add(Position);
            s.Add(Orientation);
            s.Add((int)ShipType.Model);
            s.Add((int)ShipType.Class);
            s.Add(PlayerName);
            return s.GetBytes();
        }

        public override void SetDataFromByteArray(Byte[] byteArray) {
            Deserializer d = new Deserializer(byteArray);
            PlayerId = d.GetNextInt();
            Position = d.GetNextVector3();
            Orientation = d.GetNextQuaternion();
            ShipType.Model = (ShipModel)d.GetNextInt();
            ShipType.Class = (ShipClass)d.GetNextInt();
            PlayerName = d.GetNextString();
            return;
        }

    }

    //Information re: ship controls
    public class ShipControlStatus : GameEvent
    {
        public int thrust, pitch, roll, yaw;
        public int playerID;

        public override Lidgren.Library.Network.NetChannel DeliveryType {
            get { return Lidgren.Library.Network.NetChannel.Unreliable; }
        }

        public ShipControlStatus() {
            thrust = 0;
            pitch = 0;
            roll = 0;
            yaw = 0;
            playerID = -1;
        }

        public ShipControlStatus(int _thrust, int _pitch, int _roll, int _yaw, int _playerID)
        {
            thrust = _thrust;
            pitch = _pitch;
            roll = _roll;
            yaw = _yaw;
            playerID = _playerID;

        }

        public override void SetDataFromByteArray(byte[] data)
        {
            Deserializer d = new Deserializer(data);
            playerID = d.GetNextInt();
            pitch = d.GetNextInt();
            roll = d.GetNextInt();
            yaw = d.GetNextInt();
            thrust = d.GetNextInt();
        }

        public override byte[] ToByteArray()
        {
            Serializer s = new Serializer();
            s.Add(playerID);
            s.Add(pitch);
            s.Add(roll);
            s.Add(yaw);
            s.Add(thrust);

            return s.GetBytes();
        }

		public static event GameEventFiringHandler FiringEvent;
		public override void FireEvent()
		{
			if (FiringEvent != null)
			{
				FiringEvent(this);
			}
		}
    }

    //Info re: status of all ships
    public class ShipStateStatus : GameEvent {

        private List<ShipState> states;

        public ShipStateStatus() {
        }

        public ShipStateStatus(List<ShipState> stateList) {
            states = stateList;
        }

        public List<ShipState> getStates() {
            return states;
        }

        public override Lidgren.Library.Network.NetChannel DeliveryType {
            get { return Lidgren.Library.Network.NetChannel.Sequenced1; }
        }
        public override byte[] ToByteArray() {
   
            Serializer s = new Serializer();
            for (int i = 0; i < states.Count; i++) {
                s.Add(states[i].id);
                s.Add(states[i].Position);
                s.Add(states[i].Velocity);
                s.Add(states[i].Orientation);
                s.Add(states[i].RotationalVelocity);
            }

            return s.GetBytes();
        }
        public override void SetDataFromByteArray(byte[] byteArray) {
            Deserializer d = new Deserializer(byteArray);
            states = new List<ShipState>();            
            ShipState curState;
            while (d.GetNumBytesRemaining() > 0) {
                curState = new ShipState();
                curState.id = d.GetNextInt();
                curState.Position = d.GetNextVector3();
                curState.Velocity = d.GetNextVector3();
                curState.Orientation = d.GetNextQuaternion();
                curState.RotationalVelocity = d.GetNextVector3();

                states.Add(curState);
            }
        }

        public static event GameEventFiringHandler FiringEvent;
		public override void FireEvent()
		{
			if (FiringEvent != null)
			{
				FiringEvent(this);
			}
		}


    }

    public struct ShipState {
        public int id;
        public Vector3 Position;
        public Vector3 Velocity;
        public Vector3 RotationalVelocity;
        public Quaternion Orientation;
    }

    public class ChatEvent : GameEvent{
        private List<int> targetIds;
        private String msg;
        

        public ChatEvent() { }

        public ChatEvent(String message, List<int> recipientIds) {
            targetIds = recipientIds;
            msg = message;            
        }

        public static event GameEventFiringHandler FiringEvent;
        public override void FireEvent() {
            if (FiringEvent != null) {
                FiringEvent(this);
            }
        }

        public override byte[] ToByteArray() {            
            Serializer s = new Serializer();
            s.Add(targetIds.Count);
            for(int i=0;i<targetIds.Count;i++){
                s.Add(targetIds[i]);
            }
            s.Add(msg);

            return s.GetBytes(); 
        }

        public override void SetDataFromByteArray(byte[] byteArray) {
            Deserializer d = new Deserializer(byteArray);           
            int numTargets = d.GetNextInt();
            for (int i = 1; i <= numTargets; i++) {
                targetIds.Add(d.GetNextInt());
            }
            msg = d.GetNextString();
        }

        public override Lidgren.Library.Network.NetChannel DeliveryType {
            get { return Lidgren.Library.Network.NetChannel.ReliableUnordered; }
        }

        public List<int> getRecipientIds() {
            return targetIds;
        }
        public String getMessage() {
            return msg;
        }


    }

    public class StatBoardEvent : GameEvent {
        private StatBoardEnum stat;
        private Dictionary<int,int> valueById;
        public static event GameEventFiringHandler FiringEvent;
        public StatBoardEvent(StatBoardEnum statType, Dictionary<int, int> statValueByPlayerId) {
            stat = statType;
            valueById = statValueByPlayerId;
        }

        public override byte[] ToByteArray() {
            Serializer s = new Serializer();
            IEnumerator boardEnum = valueById.GetEnumerator();
            boardEnum.Reset();
            while (boardEnum.MoveNext()) {
                KeyValuePair<int, int> curKV = (KeyValuePair<int, int>)boardEnum.Current;
                s.Add(curKV.Key);
                s.Add(curKV.Value);
            }
            return s.GetBytes();
        }

        public override void SetDataFromByteArray(byte[] byteArray) {
            Deserializer d = new Deserializer(byteArray);
            stat = (StatBoardEnum)d.GetNextByte();
            valueById = new Dictionary<int, int>();
            while (d.GetNumBytesRemaining() > 0) {                
                valueById.Add(d.GetNextInt(), d.GetNextInt());
            }
        }

        public override void FireEvent() {
            if (FiringEvent != null) {
                FiringEvent(this);
            }
        }

        public override Lidgren.Library.Network.NetChannel DeliveryType {
            get { return Lidgren.Library.Network.NetChannel.ReliableUnordered; }
        }

    }
    public enum StatBoardEnum { PrimaryScore, Kills, Deaths, PositiveTime, NegativeTime }


}

