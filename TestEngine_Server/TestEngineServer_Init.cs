using System;
using Mogre;
using MogreNewt;
using System.Collections.Generic;
using System.Threading;

namespace Ymfas
{
	partial class TestEngineServer : IDisposable
	{
		private World world;
		private EventManager eventMgr;
		private YmfasServer netServer;

		private Mogre.Timer frameTimer;

		public enum RenderType
		{
			Direct3D9 = 0,
			OpenGL = 1
		};

		public TestEngineServer(YmfasServer _server)
		{
			netServer = _server;
			eventMgr = new EventManager(_server);
		}

		void Singleton_ResourceGroupLoadEnded(string groupName)
		{
			throw new Exception("The method or operation is not implemented.");
		}
		
		public void PrepareGameInstance()
		{
			// physics system
			InitializePhysics();            

			// various other things
			frameTimer = new Mogre.Timer();

			// initalize the scene
			InitializeScene();

            // initialize server/client threads
            //InitializeThreads();
		}

        /*/// <summary>
        /// Creates server/client threads
        /// </summary>
        private void InitializeThreads() {
            //create server thread if necessary
            Thread ServerThread = new Thread(ServerGo);
            ServerThread.Start();
        }*/

		/// <summary>
		/// initialize the physics engine
		/// </summary>
		private void InitializePhysics()
		{
			// use faster, inexact settings
			world = new World();
			world.setSolverModel((int)World.SolverModelMode.SM_ADAPTIVE);
			world.setFrictionModel((int)World.FrictionModelMode.FM_ADAPTIVE);
            world.setWorldSize(new AxisAlignedBox(new Vector3(-WorldSizeParam), new Vector3(WorldSizeParam)));
			world.LeaveWorld += new LeaveWorldEventHandler(OnLeaveWorld);
		}


		public void Dispose()
		{
			// destroy all instance-specific information

			if (world != null)
			{
				world.Dispose();
				world = null;
			}
		}

		public World World
		{
			get { return world; }
		}
	}
}
