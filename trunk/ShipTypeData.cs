using System;
using System.Collections.Generic;
using System.Text;

namespace ymfas {
    //Contains information about the ship class & model
    public class ShipTypeData {
        public ShipClass Class;
        public ShipModel Model;

        
    }

    public enum ShipClass { Interceptor, Corvette, Cruiser, Battleship };
    public enum ShipModel { MogreFighter };
}
