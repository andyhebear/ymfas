using System;
using System.Collections;
using System.Collections.Generic;

namespace Ymfas
{
	static class Util
	{
		public static void RecordException(Exception e)
		{
			Log(e.GetType().Name);
			Log(e.Source);
			Log(e.Message);
			Log(e.StackTrace);
		}
        public static void Log(String message) {
            Mogre.LogManager.Singleton.DefaultLog.LogMessage(message);
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

    /// <summary>
    /// Assists with serialization of information over the network
    /// </summary>
    class Serializer {
        private List<byte> contents;

        public Serializer() {
            contents = new List<byte>();
        }

        public void Add(int value) {
            byte[] bytes = BitConverter.GetBytes(value);
            Add(bytes);
        }
        public void Add(float value) {
            byte[] bytes = BitConverter.GetBytes(value);
            Add(bytes);
        }
        public void Add(char value) {
            byte[] bytes = BitConverter.GetBytes(value);
            Add(bytes);
        }
        public void Add(Mogre.Vector3 value) {
            Add(value.x);
            Add(value.y);
            Add(value.z);
        }
        public void Add(Mogre.Quaternion value) {
            Add(value.w);
            Add(value.x);
            Add(value.y);
            Add(value.z);
        }
        public void Add(String value) {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(value.ToCharArray());
            Add(bytes);
        }
        public void Add(byte value) {
            contents.Add(value);
        }
        public void Add(byte[] value) {
            for (int i = 0; i < value.Length; i++) {
                contents.Add(value[i]);
            }
        }

        public byte[] GetBytes() {
            return contents.ToArray();
        }
    }

    /// <summary>
    /// Assists with deserialization of information over the network
    /// </summary>
    public class Deserializer{
        byte [] contents;
        int offset;

        public Deserializer(byte[] bytes){
            contents = bytes;
            offset = 0;
        }

        public int GetNumBytesRemaining() {
            return contents.Length - offset;
        }

        public int GetNextInt(){
            int retval = BitConverter.ToInt32(contents, offset);
            offset += sizeof(int);
            return retval;
        }

        public float GetNextFloat(){
            float retval = BitConverter.ToSingle(contents, offset);
            offset += sizeof(float);
            return retval;
        }

        public char GetNextChar(){
            char retval = BitConverter.ToChar(contents, offset);
            offset += sizeof(char);
            return retval;
        }

        public Mogre.Vector3 GetNextVector3(){
            return new Mogre.Vector3(GetNextFloat(), GetNextFloat(), GetNextFloat());
        }

        public Mogre.Quaternion GetNextQuaternion(){
            return new Mogre.Quaternion(GetNextFloat(), GetNextFloat(), GetNextFloat(), GetNextFloat());
        }

        /// <summary>
        /// Gets the remainder of the data as a string
        /// </summary>
        /// <returns>The remaining data as a UTF-8 encoded string</returns>
        public String GetNextString(){
            return GetNextString(contents.Length-offset);
        }

        /// <summary>
        /// Gets the next data block as a string
        /// </summary>
        /// <param name="length">The number of bytes to get</param>
        /// <returns>A UTF-8 encoded string of size bytes</returns>
        public String GetNextString(int size){
            byte[] bytes = new byte[size];
            for(int i=0;i<size;i++){
                bytes[i] = contents[i+offset];
            }
            offset += size;

            return System.Text.Encoding.UTF8.GetString(bytes);
        }
    }
}
