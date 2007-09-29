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
		Dictionary<int, ClientShip> shipTable = new Dictionary<int, ClientShip>();

		public ICollection<ClientShip> Ships
		{
			get { return shipTable.Values; }
		}

		public ShipManager(TestEngine _engine)
		{
			engine = _engine;
			ShipInit.FiringEvent += new GameEventFiringHandler(handleShipInit);
			ShipStateStatus.FiringEvent += new GameEventFiringHandler(handleShipStateStatus);
		}

		private void handleShipInit(GameEvent e)
		{
			ShipInit ee = (ShipInit)e;
			ClientShip ship = new ClientShip(engine.World, engine.SceneManager, null, ee.PlayerId, ee.Position, ee.Orientation);
			shipTable.Add(ship.ID, ship);

			if (ship.ID == engine.PlayerId)
				engine.AttachCamera(ship);

			Util.Log("Ship " + ship.ID + "inited.  my ID is " + engine.PlayerId);
		}

		private void handleShipStateStatus(GameEvent e)
		{
			ShipStateStatus ee = (ShipStateStatus)e;
			List<ShipState> states = ee.getStates();
			for (int i = 0; i < states.Count; i++)
			{
				ClientShip s;
                shipTable.TryGetValue(states[i].id, out s);
                try {
                    s.ShipState = states[i];
                }
                catch (NullReferenceException nre) {
                    Util.Log("Could not find a ship with ID " + i + ", but received a status update for one.");
                }
			}
		}
	}
}