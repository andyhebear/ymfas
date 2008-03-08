using System;
using Mogre;
using MogreNewt;
using System.Collections.Generic;
using System.Threading;

namespace Ymfas
{
	partial class TestEngine : IDisposable
	{
		private Root root;
		private SceneManager sceneMgr;
		private InputSystem input;
		private World world;
		private EventManager eventMgr;
		private YmfasClient netClient;
		private UserInputManager inputMgr;
        private ChatManager chatMgr;

		private Mogre.Timer frameTimer;

		#region Constants
		private string RESOURCE_FILE = "resources.cfg";
		private string SCENE_MANAGER_ID = "default";
		#endregion

		public enum RenderType
		{
			Direct3D9 = 0,
			OpenGL = 1
		};

		private string[] renderTypeStrings =
			{ 
				"Direct3D9 Rendering Subsystem" ,
				"OpenGL Rendering Subsystem" 
			};

		public TestEngine(YmfasClient _client)
		{
			netClient = _client;

			// create the event manager
			eventMgr = new EventManager(netClient);			
		}

		void Singleton_ResourceGroupLoadEnded(string groupName)
		{
			throw new Exception("The method or operation is not implemented.");
		}
		
		public void PrepareGameInstance()
		{
            // create the root object with paths to various configuraion files
            root = new Root();

            // call the various rendering functions, essentially in 
            // the order they are defined
            DefineResources();
            if (!SetupRenderSystem(RenderType.Direct3D9, 1024, 768, false))
                //if (!SetupRenderSystem())
                throw new Exception();

			CreateWindow("YMFAS");
			InitializeResourceGroups();

			// Ogre allows callbacks for lots of different events.
			// For instance, frame listeners can be called at the start
			// or end of the rendering loop.
			root.FrameEnded += new FrameListener.FrameEndedHandler(OnFrameEnd);

			// initialize the input system
			IntPtr hwnd;
			root.AutoCreatedWindow.GetCustomAttribute("Window", out hwnd);
			input = new InputSystem(hwnd);
			inputMgr = new UserInputManager(input, eventMgr, (byte)PlayerId);

			// physics system
			InitializePhysics();            

			// various other things
			frameTimer = new Mogre.Timer();

			// initalize the scene
			InitializeScene();

            //initialize chat manager
            chatMgr = new ChatManager(netClient.GameMode, netClient.PlayerId);
            TextRenderer.AddTextBox("frameCtr","FPS: 0",900,700,100,50, ColourValue.Green, ColourValue.White);
		}

		/// <summary>
		/// Process the config file and add all the proper resource locations
		/// </summary>
		private void DefineResources()
		{
			// load the configuration file
			ConfigFile cf = new ConfigFile();
			cf.Load(RESOURCE_FILE, "\t:=", true);

			// process each section in the configuration file
			ConfigFile.SectionIterator itr = cf.GetSectionIterator();
			while (itr.MoveNext())
			{
				string sectionName = itr.CurrentKey;
				ConfigFile.SettingsMultiMap smm = itr.Current;

				// each section is set of key/value pairs
				// value is the resource type (FileSystem, Zip, etc)
				// key is the location of the resource set
				foreach (KeyValuePair<string, string> kv in smm)
				{
					// add the resource location to Ogre
					ResourceGroupManager.Singleton.AddResourceLocation(kv.Value, kv.Key, sectionName);
				}
			}
		}

		/// <summary>
		/// show a dialog to request render settings
		/// </summary>
		/// <returns>true iff the user picked settings</returns>
		private bool SetupRenderSystem()
		{
			// ShowConfigDialog returns false when the user doesn't
			// specify anything, so we'll handle it later
			return root.ShowConfigDialog();
		}

		/// <summary>
		/// programmer-specified render settings
		/// </summary>
		private bool SetupRenderSystem(RenderType rt, int width, int height, bool fullscreen)
		{
			// get the rendering system plugin
			RenderSystem rs = root.GetRenderSystemByName(renderTypeStrings[(int)rt]);
			root.RenderSystem = rs;

			// set the other values
			// configuration options include: 
			// "Full Screen", which must be "Yes" or "No"
			// "Video Mode", of the form "[width] x [height] @ [bits]-bit colour"
			rs.SetConfigOption("Full Screen", fullscreen ? "Yes" : "No");
			rs.SetConfigOption("Video Mode", width + " x " + height + " @ 32-bit colour");
			rs.SetConfigOption("VSync", "no");

			return true;
		}

		/// <summary>
		/// create the window and display to screen
		/// </summary>
		/// <param name="title">window caption, if not in fullscreen mode</param>
		private void CreateWindow(string title)
		{
			// The first parameter determines whether to create the
			// window automatically. Otherwise, we'd also have to call
			// root.CreateRenderWindow .
			root.Initialise(true, title);
		}

		/// <summary>
		/// initialize all resource groups in the resource configuration file
		/// </summary>
		private void InitializeResourceGroups()
		{
			TextureManager.Singleton.DefaultNumMipmaps = 4;
			MeshManager.Singleton.PrepareAllMeshesForShadowVolumes = true;

			// for now, we just load all of the resource groups
			ResourceGroupManager.Singleton.InitialiseAllResourceGroups();

			// usually, though, we'll need to fine comb it a little more
			// InitializeResourceGroup allows us to load a single resource group,
			// rather than loading the entire set at once

            //load verdana font
            try {
                FontManager fontMgr = FontManager.Singleton;
                ResourcePtr font = fontMgr.Create("Verdana", "General");
                font.SetParameter("type", "truetype");
                font.SetParameter("source", "verdana.ttf");
                font.SetParameter("size", "16");
                font.SetParameter("resolution", "96");
                font.Load();
                Console.Out.WriteLine("loaded, yo!");
            }
            catch (Exception e) {
                Util.Log("Unable to load verdana.ttf");
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

		/// <summary>
		/// event handle for the end of a frame render
		/// </summary>
		/// <returns>false to exit the program</returns>
		private bool OnFrameEnd(FrameEvent e)
		{
			return true;
		}

		public void Dispose()
		{
			// destroy all scene instance-specific information
			DisposeScene();
			netClient.Dispose();
			netClient = null;

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
		public EventManager EventManager
		{
			get { return eventMgr; }
		}
		public int PlayerId
		{
			get { return netClient.PlayerId; }
		}
	}
}