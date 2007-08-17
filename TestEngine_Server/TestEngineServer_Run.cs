using System;
using System.Collections.Generic;
using System.Text;
using Mogre;
using MogreNewt;
using Microsoft.DirectX.DirectInput;

namespace Ymfas
{
	public partial class TestEngineServer : IDisposable
	{
		Ship playerShip;
		ShipCamera shipCam;

		StaticGeometry grid;
		RibbonTrail ribbon;

        public static float WorldSizeParam {get { return 10000.0f; }  }


		/// <summary>
		/// initialize the scene
		/// </summary>
		private void InitializeScene()
		{
		}

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
        public void Go() {

            //init world
            World serverWorld = new World();
            // use faster, inexact settings
            serverWorld.setSolverModel((int)World.SolverModelMode.SM_ADAPTIVE);
            serverWorld.setFrictionModel((int)World.FrictionModelMode.FM_ADAPTIVE);
            serverWorld.setWorldSize(new AxisAlignedBox(new Vector3(-WorldSizeParam), new Vector3(WorldSizeParam)));
            serverWorld.LeaveWorld += new LeaveWorldEventHandler(OnLeaveWorld);

            ServerShipManager serverShipMgr = new ServerShipManager(serverWorld, eventMgr);

            while (true) {
                //do shit
            }
        }
	}
}
