using System;

namespace Ymfas
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

	class YmfasException : Exception
	{
		public YmfasException(string s) : base(s)
		{
		}
	}

	/// <summary>
	/// Simple exception for passing improper parameters
	/// </summary>
	/// <typeparam name="T">type of parameter passed improperly</typeparam>
	class InvalidParameterException<T> : YmfasException
	{
		private T param;
		private string paramName;

		public T Param
		{
			get { return param;}
			set { param = value;}
		}
	
		public string ParamName
		{
			get { return paramName; }
			set { paramName = value; }
		}

		public InvalidParameterException(string _msg, T _param, string _paramName) :
			this(_msg, _param)
		{
			paramName = _paramName;
		}

		public InvalidParameterException(string s, T _param)
			: this(s)
		{
			param = _param;
		}

		public InvalidParameterException(string s) : base(s) { }
	}
}