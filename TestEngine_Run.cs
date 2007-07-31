using System;
using System.Collections.Generic;
using System.Text;
using Mogre;
using MogreNewt;
using Microsoft.DirectX.DirectInput;

namespace Ymfas
{
	public partial class TestEngine : IDisposable
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
			// Every viewport has a camera associated with it.
			// The second number is a z-order. Higher z-order viewports
			// are rendered on top (in the case of multiple viewports).
			Camera cam = sceneMgr.CreateCamera("cam");
			root.AutoCreatedWindow.AddViewport(cam, 0);

			Viewport vp = root.AutoCreatedWindow.GetViewport(0);
			vp.BackgroundColour = ColourValue.Black;

			// have the frustum set the aspect ratio automatically
			cam.AutoAspectRatio = true;

			Vector3 position = new Vector3(0.0f, 0.0f, 0.0f);
			Quaternion orientation = Quaternion.IDENTITY;

			// In Ogre, an entity is a renderable object. An entity must be attached
			// to a scene node, however, in order to be rendered. Every entity (really, 
			// every object in Ogre) must be assigned a unique name. 
			playerShip = new Ship(world, sceneMgr, null, "ship", position, orientation);

			shipCam = new ShipCamera(cam);
			shipCam.Radius = playerShip.Mesh.BoundingRadius * 2.25f;
			shipCam.Target = playerShip.SceneNode;

			Light l = sceneMgr.CreateLight("point1");
			l.DiffuseColour = new ColourValue(1.0f, 1.0f, 1.0f);
			l.Position = Vector3.UNIT_Y * 3.0f * playerShip.Mesh.BoundingRadius;
			l.CastShadows = true;
			l.Type = Light.LightTypes.LT_POINT;

			sceneMgr.SetSkyBox(true, "Space", 5000);

			grid = sceneMgr.CreateStaticGeometry("grid");
			float radius = playerShip.Mesh.BoundingRadius;

			Entity cube;

			const int NUM_CUBES_HALF_WIDTH = 2;
			for (int i = -NUM_CUBES_HALF_WIDTH; i < NUM_CUBES_HALF_WIDTH; ++i)
				for (int j = -NUM_CUBES_HALF_WIDTH; j < NUM_CUBES_HALF_WIDTH; ++j)
					for (int k = -NUM_CUBES_HALF_WIDTH; k < NUM_CUBES_HALF_WIDTH; ++k)
					{
						if (i != 0)
						{
							cube = sceneMgr.CreateEntity("cube-" + i + "-" + j + "-" + k,
								SceneManager.PrefabType.PT_CUBE);
							grid.AddEntity(cube, new Vector3(i, j, k) * radius * 10);
						}
					}
			grid.Build();

			ribbon = sceneMgr.CreateRibbonTrail("ribbon");
			ribbon.MaxChainElements = 40;
			ribbon.NumberOfChains = 1;
			ribbon.UseTextureCoords = true;
			ribbon.MaterialName = "Ribbon";
			ribbon.TrailLength = 1.2f * playerShip.Mesh.BoundingRadius;
			ribbon.SetInitialColour(0, ColourValue.Red * 0.5f);
			ribbon.SetColourChange(0, new ColourValue(-0.4f, 0.4f, 0.0f));

			SceneNode back = playerShip.SceneNode.CreateChildSceneNode("back");
			back.Translate(Vector3.NEGATIVE_UNIT_Z * 0.2f * playerShip.Mesh.BoundingRadius);
			ribbon.AddNode(back);

			sceneMgr.RootSceneNode.CreateChildSceneNode().AttachObject(ribbon);

			System.Console.WriteLine("here");
		}

		void Print(uint time, Object msg)
		{
			System.Console.WriteLine(msg.ToString());
		}

		/// <summary>
		/// execute the render loop, which
		/// is actually pretty simple at this point
		/// </summary>
		public void Go()
		{
			frameTimer.Reset();
			float frameTime;

			float MAX_SPEED = 30.0f;

            //init client managers
            ShipManager shipMgr = new ShipManager(this);
            UserInputManager userInputMgr = new UserInputManager(this.input, this.eventMgr, (byte) NetworkEngine.PlayerId);


			// RenderOneFrame returns false when we Ogre
			// is done. Alternatively, we can not have
			// the loop and merely call root.StartRendering
			while (true)
			{
				frameTime = frameTimer.Milliseconds / 1000.0f;
				frameTimer.Reset();

				Console.Out.WriteLine("time");
				Console.Out.WriteLine(frameTime);

                //update input
                input.Update();
				if (input.IsDown(Key.Escape))
					break;

                //process event queue
                eventMgr.Update();

                //update camera
				shipCam.Update();

				if (!root.RenderOneFrame())
					break;
			}
		}

		public void AttachCamera(Ship s)
		{
			shipCam.Target = s.SceneNode;
		}

		private void OnLeaveWorld(World w, Body b)
		{
			Vector3 pos;
			Quaternion orient;
			b.getPositionOrientation(out pos, out orient);
			b.setPositionOrientation(new Vector3(), orient);
		}

		public void DisposeScene()
		{
			playerShip.Dispose();

			root.DestroySceneManager(sceneMgr);
		}

        /// <summary>
        /// Initializes & executes the server runtime loop
        /// </summary>
        public void ServerGo() {
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
