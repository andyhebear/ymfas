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
        Dictionary<int, Ship> shipTable = new Dictionary<int, Ship>();
        private World world;
        private EventManager eventMgr;
		private YmfasServer server;

        public ServerShipManager(World serverWorld, EventManager eventManager, YmfasServer _server)
        {
            world = serverWorld;
            eventMgr = eventManager;
			server = _server;

            //init ships
            ShipTypeData curShipType = new ShipTypeData();
            curShipType.Class = ShipClass.Interceptor;
            curShipType.Model = ShipModel.MogreFighter;

            //player ships
			foreach (int id in server.PlayerIds )
			{
                Vector3 curPosition = new Vector3(Mogre.Math.RangeRandom(-TestEngine.WorldSizeParam / 1.5f, TestEngine.WorldSizeParam / 1.5f), Mogre.Math.RangeRandom(-TestEngine.WorldSizeParam / 1.5f, TestEngine.WorldSizeParam / 1.5f), Mogre.Math.RangeRandom(-TestEngine.WorldSizeParam / 1.5f, TestEngine.WorldSizeParam / 1.5f));
                Quaternion curOrientation = Quaternion.IDENTITY;

                ShipInit curShipInit = new ShipInit(id, curShipType, curPosition, curOrientation, 
					server.GetPlayerName(id));
                eventMgr.SendEvent(curShipInit);

                Console.Out.WriteLine("sent init for ship " + id);
                //TODO: put them in the world
				
				Ship s = new Ship(world, id, curPosition, curOrientation);
				shipTable.Add(id, s);
            }

            //init listeners
            ShipControlStatus.FiringEvent += new GameEventFiringHandler(handleShipControlStatus);
        }

		void handleShipControlStatus(GameEvent e)
		{
			ShipControlStatus ee = (ShipControlStatus)e;
			Ship s;
			shipTable.TryGetValue(ee.playerID, out s);
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
            s.ThrustRelative(new Vector3(0.0f, 0.0f, ((float)ee.thrust) / ((float)UserInputManager.FULL)));
		}

        public void sendShipStateStatus()
        {
            List<ShipState> l = new List<ShipState>();
            foreach (Ship s in shipTable.Values)
                l.Add(s.ShipState);

            eventMgr.SendEvent(new ShipStateStatus(l));
        }
    }
}