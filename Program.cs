using System;
using System.Text;
using IrrlichtNETCP;
using Tao.Ode;

namespace Ymfas
{
    class Program : IDisposable
    {
        // main rendering device
        private IrrlichtDevice irrDevice;

        // input device
        private IrrInputSystem irrInput;

        // physics handle
        private IntPtr physWorld;
        
        // rendering properties from the Irrlicht device
        private VideoDriver driver;
        private SceneManager sMgr;     

        // rendering objects
        private GUIStaticText fpsLabel;
        private LightSceneNode light;
        private CameraSceneNode cam;
        private Ship playerShip;
        private SceneNode[,] cubeGrid;
        private Texture sphereTex;

        public const int GRID_WIDTH = 2;

        static void Main(string[] args)
        {
            Program p = null;
            try
            {
                p = new Program(800, 600, false, DriverType.OpenGL);

                p.InitScene();
                p.MainLoop();
                p.Dispose();
                p = null;
            }
            catch (Exception e)
            {
            	System.Console.Write("Exception thrown by " +  e.Source + ": " 
            		+ e.ToString());
                if (p != null)
                {
                    p.Dispose();
                    p = null;
                }
            }
        }

        Program(int width, int height, bool fullscreen, DriverType type)
        {
            // create the Irrlicht rendering device
            irrDevice = new IrrlichtDevice(type, new Dimension2D(width, height), 32,
                false, false, false, false);

            if (irrDevice.Null())
                throw new Exception("Couldn't initialize Irrlicht.");

            // retrieve the rendering properties
            this.driver = this.irrDevice.VideoDriver;
            this.sMgr = this.irrDevice.SceneManager;

            // initialize the input system
            irrInput = new IrrInputSystem();
            irrDevice.OnEvent += new OnEventDelegate(irrInput.OnEvent);

            // initialize the ODE physics engine
            physWorld = Ode.dWorldCreate();
            if (physWorld.ToInt32() == 0)
            	throw new Exception();
        }

        public void InitScene()
        {
            fpsLabel = irrDevice.GUIEnvironment.AddStaticText("FPS: 0", 
                new Rect(new Position2D(400, 300), new Dimension2D(100, 50)), 
                false, true, irrDevice.GUIEnvironment.RootElement,
                -1, false);

            // add a light to the scene, which will be fixed at 
            // the camera
            light = sMgr.AddLightSceneNode(null, new Vector3D(0.0f, 0.0f, 0.0f),
                new Colorf(1.0f, 0.4f, 0.4f, 0.4f), 50.0f, -1);

            // the camera will be attached to the only ship
            cam = sMgr.AddCameraSceneNode(null);
            cam.Position = new Vector3D(-10, 5, 0);
            cam.Target = new Vector3D(0, 0, 0);
            cam.AddChild(light);

            playerShip = new Ship(sMgr, physWorld);

            // initialize the background "grid"
            cubeGrid = new SceneNode[GRID_WIDTH, GRID_WIDTH];
            sphereTex = driver.GetTexture("../../data/rock.png");
            for (int i = 0; i < GRID_WIDTH; ++i)
                for (int j = 0; j < GRID_WIDTH; ++j)
                {
                    cubeGrid[i, j] = sMgr.AddCubeSceneNode(0.2f, sMgr.RootSceneNode, -1);
                    cubeGrid[i, j].Position = new Vector3D(2.0f * (float)i, 0.0f, -2.0f * (float)j);
                    cubeGrid[i, j].SetMaterialFlag(MaterialFlag.Lighting, false);
                    cubeGrid[i, j].SetMaterialTexture(0, sphereTex);
                }
        }

        public Boolean MainLoop()
        {
            int prevTime = (int)irrDevice.Timer.Time;
            int timeDelta;
            // start the main rendering loop
            while (irrDevice.Run())
            {
                // update timer
                timeDelta = (int)irrDevice.Timer.Time - prevTime;
                prevTime += timeDelta;
                irrInput.OnLoopStart();

                fpsLabel.Text = "FPS: " + driver.FPS;
                
                // handle user input
                if (irrInput.IsKeyDown(KeyCode.Up))
                    playerShip.ApplyThruster(true);
                if (irrInput.IsKeyDown(KeyCode.Down))
                    playerShip.ApplyThruster(false);
                if (irrInput.IsKeyPressed(KeyCode.Left))
                    playerShip.MaximumSpeed = playerShip.MaximumSpeed * 0.5f;
                if (irrInput.IsKeyPressed(KeyCode.Right))
                    playerShip.MaximumSpeed = playerShip.MaximumSpeed * 2.0f;

                playerShip.ApplyEnvironmentalForces();

                // update physics world
                if (timeDelta > 0)
                    Ode.dWorldQuickStep(physWorld, (float)timeDelta / 1000.0f);
                
                // update all necessary objects
                playerShip.Update();

                playerShip.MoveCameraToPosition(cam);
                driver.BeginScene(true, true, Color.From(255, 30, 30, 224));
                
                sMgr.DrawAll();
                irrDevice.GUIEnvironment.DrawAll();

                driver.EndScene();

                irrInput.OnLoopEnd();
            }

            return true;
        }

        public void Dispose()
        {
            // dispose of all scene objects
            playerShip.Dispose();
            playerShip = null;

            // destroy the rendering device
            if (irrDevice != null)
            {
                this.irrDevice.Dispose();
                this.irrDevice = null;
            }

            // input system resources are all tied to the Irrlicht device

            // dispose of the physics system
            Ode.dWorldDestroy(physWorld);
            Ode.dCloseODE();
        }
    }
}
