using UnityEngine;
using System.Collections;
using System.IO;
using com.pwrd.wuxiaprg;

public class ProtobufUtility
{
	private static ProtobufSerializer _serializer = null;
	public static ProtobufSerializer serializer {
		get {
			if (_serializer == null) {
				_serializer = new ProtobufSerializer ();
			}
			return _serializer;
		}
	}

	public static byte[] GetByteFromProtoBuf (object protobufData_)
	{
		byte[] buffer = null;
		using (MemoryStream m = new MemoryStream ( )) {
			serializer.Serialize (m, protobufData_);
			m.Position = 0;
			int length = (int)m.Length;
			buffer = new byte[length];
			m.Read (buffer, 0, length);
		}
		return buffer;
	}
	
	public static T DeserializeProtobuf<T> (byte[] buffer) where T : class
	{
		
		using (MemoryStream m = new MemoryStream(buffer)) {
			return serializer.Deserialize (m, null, typeof(T)) as T;
		}
		
	}
	
	public static object DeserializeProtobuf (byte[] buffer, System.Type type)
	{
		using (MemoryStream m = new MemoryStream(buffer)) {
			return serializer.Deserialize (m, null, type);
		}
	}
}
