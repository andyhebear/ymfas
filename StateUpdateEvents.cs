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
            for(int i=0;i<byteArray.Length -1;i++){
                PlayerName += BitConverter.ToChar(byteArray, sizeof(int) * 3 + sizeof(float) * 7 + i*sizeof(char));
            }
            return;
        }

    }

    //Information re: ship controls
    public class ShipControlStatus : GameEvent
    {
        public byte thrust;
        public sbyte pitch, roll, yaw;
        public int playerID;

        public override Lidgren.Library.Network.NetChannel DeliveryType {
            get { return Lidgren.Library.Network.NetChannel.Unreliable; }
        }

        public ShipControlStatus(byte _thrust, sbyte _pitch, sbyte _roll, sbyte _yaw, byte _playerID)
        {
            thrust = _thrust;
            pitch = _pitch;
            roll = _roll;
            yaw = _yaw;
            playerID = _playerID;
            this.DeliveryType = Lidgren.Library.Network.NetChannel.Unreliable;

        }

        public override void SetDataFromByteArray(byte[] data)
        {
            thrust = (byte) data[0];
            pitch = (sbyte) data[1];
            roll = (sbyte) data[2];
            yaw = (sbyte) data[3];
            playerID = BitConverter.ToInt32(data, 4);                        
        }

        public override byte[] ToByteArray()
        {
            byte[] byteArray = new byte[4 + sizeof(int)];
            byteArray[0] = thrust;
            byteArray[1] = (byte)pitch;
            byteArray[2] = (byte)roll;
            byteArray[3] = (byte)yaw;
            BitConverter.GetBytes(playerID).CopyTo(byteArray, 4);

            return byteArray;
        }
    }

    //Info re: status of all ships
    public class ShipStateStatus : GameEvent {
        public override Lidgren.Library.Network.NetChannel DeliveryType {
            get { throw new Exception("The method or operation is not implemented."); }
        }
        public override byte[] ToByteArray() {
            throw new Exception("The method or operation is not implemented.");
        }
        public override void SetDataFromByteArray(byte[] byteArray) {
            throw new Exception("The method or operation is not implemented.");
        }
    }
    public struct ShipState {
        public Vector3 Position;
        public Vector3 Velocity;
        public Vector3 RotationalVelocity;
        public Quaternion Orientation;
    }


    
}

