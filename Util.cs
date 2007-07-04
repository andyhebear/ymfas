using System;

namespace MogreSimple
{
	static class Util
	{
		public static void RecordException(Exception e)
		{
			System.Console.WriteLine(e.GetType().Name);
			System.Console.WriteLine(e.Source);
			System.Console.WriteLine(e.Message);
			System.Console.WriteLine(e.StackTrace);
		}
	}
}
