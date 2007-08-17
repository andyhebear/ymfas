using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using Mogre;
using MogreNewt;

namespace Ymfas
{
	public partial class TestEngineServer : IDisposable
	{
        public static float WorldSizeParam {get { return 10000.0f; }  }
		
		void Print(uint time, Object msg)
		{
			System.Console.WriteLine(msg.ToString());
		}

		private void OnLeaveWorld(World w, Body b)
		{
			Vector3 pos;
			Quaternion orient;
			b.getPositionOrientation(out pos, out orient);
			b.setPositionOrientation(new Vector3(), orient);
		}

        /// <summary>
        /// Initializes & executes the server runtime loop
        /// </summary>
        public void Go() 
		{
            ServerShipManager serverShipMgr = new ServerShipManager(world, eventMgr, netServer);

            while (true) {
				eventMgr.Update();

				IPAddress ip = null;
				while ((ip = netServer.GetDisconnectedIP()) != null)
				{
					netServer.RemovePlayer(ip);
				}

				if (netServer.NumPlayers == 0)
					break;
			}
        }
	}
}
