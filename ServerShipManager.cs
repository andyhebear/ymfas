using System;
using System.Collections.Generic;
using System.Text;
using Mogre;
using MogreNewt;
using Ymfas;

namespace Ymfas
{
    public class ServerShipManager
    {
        Dictionary<String, Ship> shipTable = new Dictionary<String, Ship>();
        private World world;
        private EventManager eventMgr;

        public ServerShipManager(World serverWorld, EventManager eventManager)
        {
            world = serverWorld;
            eventMgr = eventManager;

            //init ships
            int[] playerIds = new int[NetworkEngine.PlayerIdsByIP.Count];
            NetworkEngine.PlayerIdsByIP.Values.CopyTo(playerIds, 0);
            for (int i = 0; i < playerIds.Length; i++) {
                ShipTypeData curShipType = new ShipTypeData();
                curShipType.Class = ShipClass.Interceptor;
                curShipType.Model = ShipModel.MogreFighter;
                Vector3 curPosition = new Vector3(Mogre.Math.RangeRandom(-TestEngine.WorldSizeParam / 1.5f, TestEngine.WorldSizeParam / 1.5f), Mogre.Math.RangeRandom(-TestEngine.WorldSizeParam / 1.5f, TestEngine.WorldSizeParam / 1.5f), Mogre.Math.RangeRandom(-TestEngine.WorldSizeParam / 1.5f, TestEngine.WorldSizeParam / 1.5f));
                Quaternion curOrientation = Quaternion.IDENTITY;

                ShipInit curShipInit = new ShipInit(playerIds[i], curShipType, curPosition, curOrientation, (String)NetworkEngine.PlayerNamesById[playerIds[i]]);
                eventMgr.SendEvent(curShipInit);

                //TODO: put them in the world
            }

            //init listeners
            ShipControlStatus.FiringEvent += new GameEventFiringHandler(handleShipControlStatus);
        }

		void handleShipControlStatus(GameEvent e)
		{
			ShipControlStatus ee = (ShipControlStatus)e;
			Ship s;
			shipTable.TryGetValue(ee.playerID.ToString(), out s);
            Vector3 torque = new Vector3();
            if (ee.pitch == UserInputManager.AUTOCORRECT || ee.roll == UserInputManager.AUTOCORRECT || ee.yaw == UserInputManager.AUTOCORRECT)
            {
                torque = s.GetCorrectiveTorque();
            }

            if (ee.pitch != UserInputManager.AUTOCORRECT)
            {
                torque.x = ee.pitch / ((float)UserInputManager.POSITIVE);
            }

            if (ee.yaw != UserInputManager.AUTOCORRECT)
            {
                torque.y = ee.yaw / ((float)UserInputManager.POSITIVE);
            }

            if (ee.roll != UserInputManager.AUTOCORRECT)
            {
                torque.z = ee.roll / ((float)UserInputManager.POSITIVE);
            }

            s.TorqueRelative(torque);
            s.ThrustRelative(new Vector3(0.0f, 0.0f, ee.thrust / ((float)UserInputManager.FULL)));


		}
    }
}
