using System;
using System.Collections.Generic;
using Mogre;
using MogreNewt;
using Ymfas;

namespace Ymfas
{
    /// <summary>
    /// Manages all ships
    /// </summary>
	public class ShipManager
	{
		TestEngine engine;
		Dictionary<String, ClientShip> shipTable = new Dictionary<String, ClientShip>();

		public ShipManager(TestEngine _engine)
		{
			engine = _engine;
			ShipInit.FiringEvent += new GameEventFiringHandler(handleShipInit);
			ShipStateStatus.FiringEvent += new GameEventFiringHandler(handleShipStateStatus);
		}

		private void handleShipInit(GameEvent e)
		{
			ShipInit ee = (ShipInit)e;
			ClientShip ship = new ClientShip(engine.World, engine.SceneManager, null, ee.PlayerId.ToString(), ee.Position, ee.Orientation);
			shipTable.Add(ship.ID, ship);

			if (ship.ID == engine.PlayerId.ToString())
				engine.AttachCamera(ship);

			Console.Out.WriteLine("Ship " + ship.ID + "inited.  my ID is " + engine.PlayerId);
		}

		private void handleShipStateStatus(GameEvent e)
		{
			ShipStateStatus ee = (ShipStateStatus)e;
			List<ShipState> states = ee.getStates();
			for (int i = 0; i < states.Count; i++)
			{
				ClientShip s;
				shipTable.TryGetValue(states[i].id.ToString(), out s);
				s.ShipState = states[i];
			}
		}
	}
}