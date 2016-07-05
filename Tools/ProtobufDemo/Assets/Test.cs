using UnityEngine;

using System.IO;
using com.pwrd.wuxiaprg;

public class Test : MonoBehaviour
{
	private string content = "";
	void Start ()
	{
		TestProtobuf ();
	}

	void OnGUI ()
	{
		if (GUILayout.Button (content, GUILayout.Width (200f), GUILayout.Height (200f))) {
			TestProtobuf ();
		}
	}

	private void TestProtobuf ()
	{


		LoginRequest req = new LoginRequest ();
		req.user = "UserName";
		ProtobufSerializer serializer = new ProtobufSerializer ();
			
		//Serialize
		byte[] buffer = null;
			
		using (MemoryStream m = new MemoryStream ( )) {
			serializer.Serialize (m, req);
			m.Position = 0;
			int length = (int)m.Length;
			buffer = new byte[length];
			m.Read (buffer, 0, length);
		}
			
		LoginRequest req1;
				
		//Deserialize
		using (MemoryStream m = new MemoryStream ( buffer )) {
			req1 = serializer.Deserialize (m, null, typeof(LoginRequest)) as LoginRequest;
		}
			
			
		Debug.Log ("req1.User=" + req1.user);
		content = req1.user;
	}
	
	
}
