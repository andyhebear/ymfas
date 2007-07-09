using System;
using System.Collections.Generic;
using Mogre;
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

		/// <summary>
		/// initialize the scene
		/// </summary>
		private void InitializeScene()
		{
			// create the scene manager
			mgr = root.CreateSceneManager(SceneType.ST_GENERIC, this.SCENE_MANAGER_ID);

			// Every viewport has a camera associated with it.
			// ogre can easily render with multiple viewports, 
			// each with different cameras.
			// The second number is a z-order. Higher z-order viewports
			// are rendered on top
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

			//shipNode.AttachObject(cam);
			cam.Position = playerShip.Position + (new Vector3(0, 1, -2)) * playerShip.Mesh.BoundingRadius;
			cam.LookAt(playerShip.Position);
			
			Light l = mgr.CreateLight("point1");
			l.DiffuseColour = new ColourValue(1.0f, 1.0f, 1.0f);
			l.Position = new Vector3(0, 2, -1) * playerShip.Mesh.BoundingRadius;
			l.CastShadows = true;
			l.Type = Light.LightTypes.LT_POINT;
		}

		/// <summary>
		/// execute the render loop, which
		/// is actually pretty simple at this point
		/// </summary>
		public void Go()
		{
			frameTimer.Reset();
			float frameTime;

			// Basically, there's not much to do here
			// RenderOneFrame returns false when we Ogre
			// is done. Alternatively, we can not have
			// the loop and merely call root.StartRendering();
			bool done = false;
			while (!done)
			{
				frameTime = frameTimer.Milliseconds / 1000.0f;
				frameTimer.Reset();

				input.Update();
				if (input.IsDown(Key.Escape))
					break;

				int mx = input.Mouse.X;
				int my = input.Mouse.Y;

                Entity s = playerShip.Mesh;
                SceneNode sn = s.ParentSceneNode;
				
                if (input.IsDown(Key.Up))
                    playerShip.ThrustRelative(new Vector3(0.0f, 0.0f, s.BoundingRadius * 0.1f));
                if (input.IsDown(Key.Down))
                    playerShip.ThrustRelative(new Vector3(0.0f, 0.0f, s.BoundingRadius * -0.1f));
                
                System.Console.WriteLine(playerShip.Velocity.ToString() + ", " + playerShip.Position.ToString());

                world.update(frameTime);

				mgr.GetCamera("cam").LookAt(sn.Position);

				done = !root.RenderOneFrame();
			}
		}
	}
}