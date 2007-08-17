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

		private Mogre.Timer frameTimer;

		public enum RenderType
		{
			Direct3D9 = 0,
			OpenGL = 1
		};

		public TestEngineServer()
		{
			
		}

		void Singleton_ResourceGroupLoadEnded(string groupName)
		{
			throw new Exception("The method or operation is not implemented.");
		}
		
		/// <summary>
		/// Launches the form to configure networking 
		/// upon return, the network engine should be fully initialized
		/// </summary>
        public bool ConfigureNetwork() {
            // launch the main splash window
            frmMainSplash networkForm = new frmMainSplash();
            Console.Out.WriteLine("Running the form");
			networkForm.ShowDialog();
            Console.Out.WriteLine("Form done running");

			System.Console.Write(networkForm.DialogResult);

			if (NetworkEngine.Engine == null || networkForm.DialogResult == System.Windows.Forms.DialogResult.Cancel)
				return false;

            //launch the event manager
			this.eventMgr = new EventManager();

            return true;
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
            InitializeThreads();

		}

        /// <summary>
        /// Creates server/client threads
        /// </summary>
        private void InitializeThreads() {
            //create client thread
            Thread ClientThread = new Thread(ClientGo);
            ClientThread.Start();


            //create server thread if necessary
            if (NetworkEngine.EngineType == SpiderEngine.SpiderType.Server) {
                Thread ServerThread = new Thread(ServerGo);
                ServerThread.Start();
            }
        }

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
			// destroy all scene instance-specific information
			DisposeScene();

			if (world != null)
			{
				world.Dispose();
				world = null;
			}

			if (root != null)
			{
				root.Dispose();
				root = null;
			}
		}

		public World World
		{
			get { return world; }
		}
		public SceneManager SceneManager
		{
			get { return sceneMgr; }
		}
	}
}
