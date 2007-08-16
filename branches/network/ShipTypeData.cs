using System;
using System.Collections.Generic;
using System.Text;

namespace Ymfas {
    //Contains information about the ship class & model
    //if this class is updated (diff properties), it's byte encoding in game events need to be updated...
    public struct ShipTypeData {
        public ShipClass Class;
        public ShipModel Model;

    }

    public enum ShipClass { Interceptor, Corvette, Cruiser, Battleship };
    public enum ShipModel { MogreFighter };
}
