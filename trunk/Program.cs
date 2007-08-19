using System;
using System.Collections.Generic;
using Mogre;
using MogreNewt;
using Microsoft.DirectX.DirectInput;
using System.Threading;

namespace Ymfas
{
	static class Program
	{
		static YmfasClient client;
		static YmfasServer server;

		static void Main()
		{
			System.Windows.Forms.Application.EnableVisualStyles();
			System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
			
			// run the form to create a networking game
			if (!ConfigureNetwork())
				return;

			if (server != null)
			{
				Thread serverThread = new Thread(new ThreadStart(RunServer));
				serverThread.Start();
			}

			RunClient();
		}

		public static void RunClient()
		{
			TestEngine test = new TestEngine(client);

			// now, initialize the engine
			test.PrepareGameInstance();

			// run the game
			test.Go();

			test.Dispose();
			test = null;
		}

		public static void RunServer()
		{
			TestEngineServer engineServer = new TestEngineServer(server);

			engineServer.PrepareGameInstance();

			engineServer.Go();

			engineServer.Dispose();
			engineServer = null;
		}

		/// <summary>
		/// Launches the form to configure networking 
		/// upon return, the network engine should be fully initialized
		/// </summary>
		public static bool ConfigureNetwork()
		{
			// launch the main splash window
			frmMainSplash networkForm = new frmMainSplash();
			Console.Out.WriteLine("Running the form");
			networkForm.ShowDialog();
			Console.Out.WriteLine("Form done running");

			System.Console.Write(networkForm.DialogResult);

			if (networkForm.DialogResult == System.Windows.Forms.DialogResult.Cancel)
				return false;

			// check to see if we have a host as well
			client = networkForm.Client;
			server = networkForm.Server;

			return true;
		}
	}
}
