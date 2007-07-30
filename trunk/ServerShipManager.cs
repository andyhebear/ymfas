using System;
using System.Collections.Generic;
using System.Text;
using Ymfas;

namespace Ymfas
{
    public class ServerShipManager
    {
        Dictionary<String, Ship> shipTable = new Dictionary<String, Ship>();

        public ServerShipManager()
        {
            ShipControlStatus.FiringEvent += handleShipControlStatus;
        }

        void handleShipControlStatus(GameEvent e)
        {
            ShipControlStatus ee = (ShipControlStatus) e;
            Ship s;
            shipTable.TryGetValue(ee.playerID.ToString(), out s);
        }



    }


}
