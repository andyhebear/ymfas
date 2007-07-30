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

        public ServerShipManager()
        {
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
