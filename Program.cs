using System;
using System.Collections.Generic;
using Mogre;
using MogreNewt;
using Microsoft.DirectX.DirectInput;
using YCB;

namespace Ymfas
{
	static class Program
	{
		static void Main()
		{
            //Launch the main form
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
            System.Windows.Forms.Application.Run(new frmMainSplash());
		}
	}

	partial class TestEngine : IDisposable
	{
		Ship playerShip;
		ShipCamera shipCam;

		StaticGeometry grid;

		/// <summary>
		/// initialize the scene
		/// </summary>
		private void InitializeScene()
		{
			// create the scene manager
			mgr = root.CreateSceneManager(SceneType.ST_GENERIC, this.SCENE_MANAGER_ID);

			// Every viewport has a camera associated with it.
			// The second number is a z-order. Higher z-order viewports
			// are rendered on top (in the case of multiple viewports).
            Camera cam = mgr.CreateCamera("cam");
            root.AutoCreatedWindow.AddViewport(cam, 0);

			Viewport vp = root.AutoCreatedWindow.GetViewport(0);
			vp.BackgroundColour = ColourValue.Blue;

			// have the frustum set the aspect ratio automatically
			cam.AutoAspectRatio = true;

			// In Ogre, an entity is a renderable object. An entity must be attached
			// to a scene node, however, in order to be rendered. Every entity (really, 
			// every object in Ogre) must be assigned a unique name. 
			playerShip = new Ship(world, mgr, null, "ship");

            shipCam = new ShipCamera(cam);
            shipCam.Radius = playerShip.Mesh.BoundingRadius * 2.25f;
            shipCam.Target = playerShip.SceneNode;

			Light l = mgr.CreateLight("point1");
			l.DiffuseColour = new ColourValue(1.0f, 1.0f, 1.0f);
			l.Position = new Vector3(0, 2, -1) * playerShip.Mesh.BoundingRadius;
			l.CastShadows = true;
			l.Type = Light.LightTypes.LT_POINT;

			grid = mgr.CreateStaticGeometry("grid");
			float radius = playerShip.Mesh.BoundingRadius;

			Entity cube;

			const int NUM_CUBES_HALF_WIDTH = 4;
			for (int i = -NUM_CUBES_HALF_WIDTH; i < NUM_CUBES_HALF_WIDTH; ++i)
				for (int j = -NUM_CUBES_HALF_WIDTH; j < NUM_CUBES_HALF_WIDTH; ++j)
					for (int k = -NUM_CUBES_HALF_WIDTH; k < NUM_CUBES_HALF_WIDTH; ++k)
					{
                        cube = mgr.CreateEntity("cube-" + i + "-" + j + "-" + k,
                            SceneManager.PrefabType.PT_CUBE);
						grid.AddEntity(cube, new Vector3(i, j, k)* radius * 50);
					}
            grid.Build();

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

			// RenderOneFrame returns false when we Ogre
			// is done. Alternatively, we can not have
			// the loop and merely call root.StartRendering
			while (true)
			{
				frameTime = frameTimer.Milliseconds / 1000.0f;
				frameTimer.Reset();

				input.Update();
				if (input.IsDown(Key.Escape))
					break;

				int dx = input.Mouse.dX;
				int dy = input.Mouse.dY;
				int mx = input.Mouse.X;
				int my = input.Mouse.Y;

                Entity s = playerShip.Mesh;
                SceneNode sn = s.ParentSceneNode;
                
                // respond to ship input

                if (input.IsDown(Key.E))
                    playerShip.TorqueRelative(new Vector3(1.0f, 0.0f, 0.0f));

                if (input.IsDown(Key.D))
                    playerShip.TorqueRelative(new Vector3(-1.0f, 0.0f, 0.0f));

                if (input.IsDown(Key.F))
                    playerShip.TorqueRelative(new Vector3(0.0f, -1.0f, 0.0f));

                if (input.IsDown(Key.S))
                    playerShip.TorqueRelative(new Vector3(0.0f, 1.0f, 0.0f));

                if (input.IsDown(Key.R))
                    playerShip.TorqueRelative(new Vector3(0.0f, 0.0f, 1.0f));

                if (input.IsDown(Key.W))
                    playerShip.TorqueRelative(new Vector3(0.0f, 0.0f, -1.0f));

                if (input.IsDown(Key.Space))
                    playerShip.ThrustRelative(new Vector3(0.0f, 0.0f, 1.0f));

                if (input.IsDown(Key.A))
                    playerShip.StopRotation(frameTime);

				world.update(frameTime);

                if (playerShip.Velocity.Length > MAX_SPEED * s.BoundingRadius)
                {
                    Vector3 vel = playerShip.Velocity;
                    vel.Normalise();
                    vel *= MAX_SPEED * s.BoundingRadius;
                    playerShip.Velocity = vel;
                }
                //System.Console.WriteLine(playerShip.Position);

                shipCam.Update();

                if (!root.RenderOneFrame())
                    break;
			}
		}

		private void OnLeaveWorld(World w, Body b)
		{
            System.Console.WriteLine("Reached world end.");
            Vector3 pos = new Vector3(); 
            Quaternion orient = new Quaternion();
            b.getPositionOrientation(out pos, out orient);
            System.Console.WriteLine("Ship position: " + pos);
            System.Console.WriteLine("Current ship speed: " + b.getVelocity().Length);
            System.Console.WriteLine("Restarting ship position...");

			b.setPositionOrientation(new Vector3(), orient);
		}
	}
}