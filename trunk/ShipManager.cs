using System;
using System.Collections.Generic;
using Mogre;
using MogreNewt;
using Ymfas;

namespace Ymfas
{
    /// <summary>
    /// Summary description for Class1
    /// </summary>
    public class ShipManager
    {
		TestEngine engine;
		Dictionary<String, Ship> shipTable = new Dictionary<String, Ship>();

        public ShipManager(TestEngine _engine)
        {
			engine = _engine;
            ShipInit.FiringEvent += new GameEventFiringHandler(handleShipInit);
        }

        private void handleShipInit(GameEvent e)
        {
            ShipInit ee = (ShipInit) e;
            Ship ship = new Ship(engine.World, engine.SceneManager, null, ee.PlayerId.ToString(), ee.Position, ee.Orientation);
            shipTable.Add(ship.ID, ship);

			if (ship.ID == NetworkEngine.PlayerId.ToString())
				engine.AttachCamera(ship);
        }

    }
}