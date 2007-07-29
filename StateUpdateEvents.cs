using System;
using System.Collections.Generic;
using System.Text;
using Mogre;
using MogreNewt;
using System.ComponentModel;

namespace Ymfas {

    //Event for initializing a ship belonging to a specific player in the game environment
    public class ShipInit : GameEvent {
        public int PlayerId;
        public ShipTypeData ShipType;
        public Vector3 Position;
        public Quaternion Orientation;


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
        }

        /// <summary>
        /// Creates the event to initialize a ship
        /// </summary>
        /// <param name="playerId">The owner of the ship</param>
        /// <param name="shipType">The type of the ship</param>
        /// <param name="position">The position of the ship</param>
        /// <param name="orientation">The orientation of the ship</param>
        public ShipInit(int playerId, ShipTypeData shipType, Vector3 position, Quaternion orientation) {
            PlayerId = playerId;
            ShipType = shipType;
            Position = position;
            Orientation = orientation;
        }

        public override Byte[] ToByteArray() {
            Byte[] byteArray = new Byte[sizeof(int) * 3 + sizeof(float) * 7 + 1];
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
            byteArray[byteArray.Length] = (Byte)0;

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

            return;
        }

    }
}

