using UnityEngine;
using Utility;

public class UdpTester : MonoBehaviour
{

    private UdpSocket m_udpSocket;

    // Use this for initialization
    void Start()
    {
        m_udpSocket = new UdpSocket();
        m_udpSocket.Init("10.238.6.24", 6000, 6001);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            Send();
        }

        //         TestResponse rsp = m_udpSocket.GetRespone();
        //         if(rsp != null)
        //         {
        //             Debug.Log("Msg recv:" + rsp.content);
        //         }
    }

    //private int counter = 0;
    private void Send()
    {
        //         TestRequest req = new TestRequest();
        //         req.serverId = "liuzuyao";
        //         req.roleName = "liuyao";
        //         req.headId ="5";
        //         req.content = "Chat message" + counter.ToString();
        //
        //         SocketMessage msg = new SocketMessage(req);
        //         m_udpSocket.SendMessage(msg);
        //         counter++;
        //         Debug.Log("Msg send " + counter);
    }

    void OnDisable()
    {
        m_udpSocket.Dispose();
    }
}
