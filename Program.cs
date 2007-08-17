using System;
using System.Collections.Generic;
using Mogre;
using MogreNewt;
using Microsoft.DirectX.DirectInput;

namespace Ymfas
{
	static class Program
	{
		static void Main()
		{
			System.Windows.Forms.Application.EnableVisualStyles();
			System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

			TestEngine test = new TestEngine();

			// run the form to create a networking game
			if (!test.ConfigureNetwork())
				return;

			// now, initialize the engine
			test.PrepareGameInstance();

			// run the game
			test.Go();

			test.Dispose();
			test = null;
		}
	}
}
