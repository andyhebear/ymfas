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
		ArcballCamera shipCam;
		ShipManager shipMgr;

		StaticGeometry grid;

        public static float WorldSizeParam {get { return 10000.0f; }  }

		/// <summary>
		/// initialize the scene
		/// </summary>
		private void InitializeScene()
		{
            //create scene mgr
            sceneMgr = root.CreateSceneManager(SceneType.ST_GENERIC);

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

			shipCam = new ArcballCamera(cam);
			shipCam.Radius = 250.0f;
			shipCam.Target = sceneMgr.RootSceneNode;

			Light l = sceneMgr.CreateLight("point1");
			l.DiffuseColour = new ColourValue(1.0f, 1.0f, 1.0f);
			l.Position = Vector3.UNIT_Y * 100.0f;
			l.CastShadows = true;
			l.Type = Light.LightTypes.LT_POINT;

			sceneMgr.SetSkyBox(true, "Space", 5000);

			grid = sceneMgr.CreateStaticGeometry("grid");
			float radius = 100.0f;

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

			shipMgr = new ShipManager(this);
            
		}

		void Print(uint time, Object msg)
		{
			System.Console.WriteLine(msg.ToString());
		}

        public void AttachCamera(ClientShip s)
        {
            shipCam.Target = s.SceneNode;
        }

		/// <summary>
		/// execute the render loop, which
		/// is actually pretty simple at this point
		/// </summary>
		public void Go()
		{
			frameTimer.Reset();
			float frameTime;
			uint frameTimeMod50 = 0;

            //float MAX_SPEED = 30.0f;

			// RenderOneFrame returns false when we Ogre
			// is done. Alternatively, we can not have
			// the loop and merely call root.StartRendering
			while (true)
			{
				frameTime = frameTimer.Milliseconds / 1000.0f;

				//update input
				inputMgr.PollInputs();

				if (input.IsDown(Key.Escape))
					break;

				// grab events
				GrabEvents(frameTimer.Milliseconds, null);

				// grab a ship, if there are any
				ICollection<ClientShip> ships = shipMgr.Ships;
				if (ships.Count > 0)
				{
					IEnumerator<ClientShip> e = ships.GetEnumerator();
					e.MoveNext();

					if (input.IsPressed(Key.Return))
						Util.Log("Current State:" + e.Current.SceneNode.Position);
				}

				frameTimeMod50 += frameTimer.Milliseconds;
				frameTimer.Reset();

				if (frameTimeMod50 > 50)
				{
					eventMgr.Update();
					frameTimeMod50 -= 50;
				}
                //this.world.update( frameTime );

                // score update
                TextRenderer.UpdateTextBox("frameCtr","FPS: " +(int)(1 / (frameTime)) );

				shipCam.Update();

				if (!root.RenderOneFrame())
					break;
			}
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
			root.DestroySceneManager(sceneMgr);
		}

		public void GrabEvents(uint x, object obj)
		{
			//System.Console.WriteLine("updating");
			eventMgr.Update();
		}
	}
}
