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
            char [] nameChars = PlayerName.ToCharArray();
            Byte[] byteArray = new Byte[sizeof(int) * 3 + sizeof(float) * 7 +  nameChars.Length * sizeof(char) + 1];
            BitConverter.GetBytes(PlayerId).CopyTo(byteArray, 0);
            BitConverter.GetBytes(Position.x).CopyTo(byteArray, sizeof(int));
            BitConverter.GetBytes(Position.y).CopyTo(byteArray, sizeof(int) + sizeof(float));
            BitConverter.GetBytes(Position.z).CopyTo(byteArray, sizeof(int) + sizeof(float)*2);
            BitConverter.GetBytes(Orientation.w).CopyTo(byteArray, sizeof(int) + sizeof(float) * 3);
            BitConverter.GetBytes(Orientation.x).CopyTo(byteArray, sizeof(int) + sizeof(float) * 4);
            BitConverter.GetBytes(Orientation.y).CopyTo(byteArray, sizeof(int) + sizeof(float) * 5);
            BitConverter.GetBytes(Orientation.z).CopyTo(byteArray, sizeof(int) + sizeof(float) * 6);
            BitConverter.GetBytes((int)ShipType.Model).CopyTo(byteArray, sizeof(int) + sizeof(float) * 7);
            BitConverter.GetBytes((int)ShipType.Class).CopyTo(byteArray, sizeof(int) * 2 + sizeof(float) * 7);
            for(int i=0;i<nameChars.Length;i++){
                BitConverter.GetBytes(nameChars[i]).CopyTo(byteArray, sizeof(int) * 3 + sizeof(float) * 7 + i*sizeof(char));
            }
            byteArray[byteArray.Length-1] = (Byte)0;
          
            return byteArray;
        }

        public override void SetDataFromByteArray(Byte[] byteArray) {
            PlayerId = BitConverter.ToInt32(byteArray, 0);
            Position.x = BitConverter.ToSingle(byteArray, sizeof(int));
            Position.y = BitConverter.ToSingle(byteArray, sizeof(int) + sizeof(float));
            Position.z = BitConverter.ToSingle(byteArray, sizeof(int) + sizeof(float) * 2);
            Orientation.w = BitConverter.ToSingle(byteArray, sizeof(int) + sizeof(float) * 3);
            Orientation.x = BitConverter.ToSingle(byteArray, sizeof(int) + sizeof(float) * 4);
            Orientation.y = BitConverter.ToSingle(byteArray, sizeof(int) + sizeof(float) * 5);
            Orientation.z = BitConverter.ToSingle(byteArray, sizeof(int) + sizeof(float) * 6);
            ShipType.Model = (ShipModel)BitConverter.ToInt32(byteArray, sizeof(int) + sizeof(float) * 7);
            ShipType.Class = (ShipClass)BitConverter.ToInt32(byteArray, sizeof(int) * 2 + sizeof(float) * 7);
            PlayerName = "";
            int position = sizeof(int) * 3 + sizeof(float) * 7;
            for (int i = 0; position < byteArray.Length; i++) {
                Console.Out.WriteLine("Writing byte " + position + " of " + byteArray.Length);
                PlayerName += BitConverter.ToChar(byteArray, position);
                position = sizeof(int) * 3 + sizeof(float) * 7 + (i+1) * sizeof(char);
                
            }
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
			playerID = BitConverter.ToInt32(data, 0);			
			pitch = BitConverter.ToInt32(data, sizeof(int));
            roll = BitConverter.ToInt32(data, 2 * sizeof(int));
            yaw = BitConverter.ToInt32(data, 3 * sizeof(int));
            thrust = BitConverter.ToInt32(data, 4 * sizeof(int));
        }

        public override byte[] ToByteArray()
        {
            byte[] byteArray = new byte[5*sizeof(int)+1];
            //byteArray[0] = thrust;
            //byteArray[1] = (byte)pitch;
            //byteArray[2] = (byte)roll;
            //byteArray[3] = (byte)yaw;
			BitConverter.GetBytes(playerID).CopyTo(byteArray, 0);            
			BitConverter.GetBytes(pitch).CopyTo(byteArray, sizeof(int));        
            BitConverter.GetBytes(roll).CopyTo(byteArray, 2 * sizeof(int));
            BitConverter.GetBytes(yaw).CopyTo(byteArray, 3 * sizeof(int));
            BitConverter.GetBytes(thrust).CopyTo(byteArray, 4 * sizeof(int));
            byteArray[byteArray.Length - 1] = (byte)0;
            return byteArray;
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
            int blockSize = sizeof(int) + sizeof(float) * 13;
            byte[] retval = new byte[blockSize];
            for (int i = 0; i < states.Count; i++ ) {
                BitConverter.GetBytes(states[i].id).CopyTo(retval, i * blockSize);

                BitConverter.GetBytes(states[i].Position.x).CopyTo(retval, i * blockSize + sizeof(int));
                BitConverter.GetBytes(states[i].Position.y).CopyTo(retval, i * blockSize + sizeof(int) + sizeof(float));
                BitConverter.GetBytes(states[i].Position.z).CopyTo(retval, i * blockSize + sizeof(int) + 2 * sizeof(float));

                BitConverter.GetBytes(states[i].Velocity.x).CopyTo(retval, i * blockSize + sizeof(int) + 3 * sizeof(float));
                BitConverter.GetBytes(states[i].Velocity.y).CopyTo(retval, i * blockSize + sizeof(int) + 4 * sizeof(float));
                BitConverter.GetBytes(states[i].Velocity.z).CopyTo(retval, i * blockSize + sizeof(int) + 5 * sizeof(float));

                BitConverter.GetBytes(states[i].Orientation.w).CopyTo(retval, i * blockSize + sizeof(int) + 6 * sizeof(float));
                BitConverter.GetBytes(states[i].Orientation.x).CopyTo(retval, i * blockSize + sizeof(int) + 7 * sizeof(float));
                BitConverter.GetBytes(states[i].Orientation.y).CopyTo(retval, i * blockSize + sizeof(int) + 8 * sizeof(float));
                BitConverter.GetBytes(states[i].Orientation.z).CopyTo(retval, i * blockSize + sizeof(int) + 9 * sizeof(float));

                BitConverter.GetBytes(states[i].RotationalVelocity.x).CopyTo(retval, i * blockSize + sizeof(int) + 10 * sizeof(float));
                BitConverter.GetBytes(states[i].RotationalVelocity.y).CopyTo(retval, i * blockSize + sizeof(int) + 11 * sizeof(float));
                BitConverter.GetBytes(states[i].RotationalVelocity.z).CopyTo(retval, i * blockSize + sizeof(int) + 12 * sizeof(float));
            }
            return retval;
        }
        public override void SetDataFromByteArray(byte[] byteArray) {
            states = new List<ShipState>();
            int blockSize = sizeof(int) + sizeof(float) * 13;
            ShipState curState;
            for (int i = 0; (i + 1) * blockSize < byteArray.Length; i++) {
                curState = new ShipState();
                curState.id = BitConverter.ToInt32(byteArray, i * blockSize);
                curState.Position.x = BitConverter.ToSingle(byteArray, i * blockSize + sizeof(int));
                curState.Position.y = BitConverter.ToSingle(byteArray, i * blockSize + sizeof(int) + sizeof(float));
                curState.Position.z = BitConverter.ToSingle(byteArray, i * blockSize + sizeof(int) + 2 * sizeof(float));
                curState.Velocity.x = BitConverter.ToSingle(byteArray, i * blockSize + sizeof(int) + 3 * sizeof(float));
                curState.Velocity.y = BitConverter.ToSingle(byteArray, i * blockSize + sizeof(int) + 4 * sizeof(float));
                curState.Velocity.z = BitConverter.ToSingle(byteArray, i * blockSize + sizeof(int) + 5 * sizeof(float));
                curState.Orientation.w = BitConverter.ToSingle(byteArray, i * blockSize + sizeof(int) + 6 * sizeof(float));
                curState.Orientation.x = BitConverter.ToSingle(byteArray, i * blockSize + sizeof(int) + 7 * sizeof(float));
                curState.Orientation.y = BitConverter.ToSingle(byteArray, i * blockSize + sizeof(int) + 8 * sizeof(float));
                curState.Orientation.z = BitConverter.ToSingle(byteArray, i * blockSize + sizeof(int) + 9 * sizeof(float));
                curState.RotationalVelocity.x = BitConverter.ToSingle(byteArray, i * blockSize + sizeof(int) + 10 * sizeof(float));
                curState.RotationalVelocity.y = BitConverter.ToSingle(byteArray, i * blockSize + sizeof(int) + 11 * sizeof(float));
                curState.RotationalVelocity.z = BitConverter.ToSingle(byteArray, i * blockSize + sizeof(int) + 12 * sizeof(float));
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


    
}

