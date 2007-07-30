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
        World world;
        SceneManager mgr;
        Dictionary<String, Ship> shipTable = new Dictionary<String, Ship>();

        public ShipManager(World _world, SceneManager _mgr)
        {
            world = _world;
            mgr = _mgr;
            ShipInit.FiringEvent += handleShipInit;
        }

        private void handleShipInit(GameEvent e)
        {
            ShipInit ee = (ShipInit) e;
            Ship ship = new Ship(world, mgr, null, ee.PlayerId.ToString(), ee.Position, ee.Orientation);
            shipTable.Add(ship.ID, ship);
        }

    }
}