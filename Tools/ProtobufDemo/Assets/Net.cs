using System;
using System.Collections.Generic;

using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using UnityEngine;
// using Thrift.Protocol;
// using Thrift.Transport;
// using protocol;
//using Utility;

public class SenderMessage
{
}
public class NetWaitingForResponseMessage
{
}
public class NetReceiveResponseMessage
{
}

public class Net //: Singleton<Net>
{
	private static readonly int RECEIVE_BUFFER_SIZE = 0x1000;

	public bool WaitingMsg { get { return this.waitingMsg; } }

	private bool waitingMsg;

	// Message Queue
	private Queue<SenderMessage> sendingMessageQueue = new Queue<SenderMessage> ();
	private Queue<SenderMessage> waitingMessageQueue = new Queue<SenderMessage> ();
	// Send and Receive
	private Thread sendThread = null;
	private Thread receiveThread = null;
	// Socket
	private Socket socket = null;
	private BufferedStream socketStream = null;
	private volatile bool isRunning = false;
	private object netLock = new object ();

	private Type[] waitedMsgs = null;
	private NetWaitingForResponseMessage waitingForResponseMessage = null;
	private NetReceiveResponseMessage receiveResponseMessage = null;

	private bool isInitiativeDisconnection = false;
	private int serialNo = 0;
	public bool IsInitiativeDisconnection {
		set { this.isInitiativeDisconnection = value; }
	}

	public Net ()
	{
		this.waitingForResponseMessage = new NetWaitingForResponseMessage ();
		this.receiveResponseMessage = new NetReceiveResponseMessage ();
	}

	public void Start (string host, int port, bool isIp = true)
	{
		if (this.isRunning)
			return;
		IPEndPoint ipEndPoint = null;
		if (isIp) {
			ipEndPoint = new IPEndPoint (IPAddress.Parse (host), port);
		} else {
			IPAddress[] address = Dns.GetHostAddresses (host);
			ipEndPoint = new IPEndPoint (address [0], port);
		}
		this.socket = new Socket (ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
		try {
			this.isRunning = true;
			// 1.Connect
			this.socket.Connect (ipEndPoint);
			// 2.Start send message thread and receive message thread
			// this.sendThread = new Thread(new ThreadStart(Send));
			//this.receiveThread = new Thread(new ThreadStart(Receive));
			//this.sendThread.Start();
			//this.receiveThread.Start();
			this.isInitiativeDisconnection = false;
		} catch (Exception ex) {
			// Socket connect error
			Debug.Log ("Connect to " + ipEndPoint.ToString () + "failed");
			Shutdown ();
			//NetInMainThread.Instance().SendMessageAsync(new NetConnectExceptionMessage(ex.Message));
		}
	}

	public void Shutdown ()
	{
		if (!this.isRunning)
			return;
		try {
			if (this.socket.Available != 0) {
				this.socket.Shutdown (SocketShutdown.Both);
			}
		} catch (Exception ex) {
			//LogModule.Exception(ex.Message, ex.StackTrace);
		} finally {
			this.isRunning = false;
			try {
				this.socket.Close ();
			} catch (Exception) {
			}
		}
	}

	//     // Link message
	//     public void SendMessage(TBase tBase, params Type[] waitedMsgs)
	//     {
	//         if (waitedMsgs != null && waitedMsgs.Length > 0)
	//         {// 需要等待回包
	//             this.waitingMsg = true;
	//             this.waitedMsgs = waitedMsgs;
	//             //MessageDispatcher.Instance().BroadcastMessage(this, this.waitingForResponseMessage);
	//         }
	//         else
	//         {
	//             this.waitingMsg = false;
	//             this.waitedMsgs = null;
	//         }
	// 
	//         SenderMessage senderMessage = new SenderMessage(tBase);
	//         lock (this.netLock)
	//         {
	//             this.waitingMessageQueue.Enqueue(senderMessage);
	//             Monitor.Pulse(this.netLock);
	//         }
	//     }
	// 
	//     // Channel message
	//     public void SendChannelMessage(TBase tBase, int type, params Type[] waitedMsgs)
	//     {
	//         LogModule.Assert(tBase != null, "Message Content is NULL!");
	// 
	//         ClientChannelRequest requestMessage = new ClientChannelRequest();
	//         requestMessage.InternalProtocolType = type;
	//         MemoryStream stream = new MemoryStream();
	//         TProtocol tProtocol = new TCompactProtocol(new TStreamTransport(stream, stream));
	//         tBase.Write(tProtocol);
	//         requestMessage.Content = stream.ToArray();
	//         SendMessage(requestMessage, waitedMsgs);
	// 
	//         LogModule.Log("Send channel message " + tBase.GetType().Name);
	//     }

	public void ClearWaitedMsgTypes ()
	{
		this.waitedMsgs = null;
		this.waitingMsg = false;
	}

	//     private void Send()
	//     {
	//         //LogModule.Log("Socket send thread start!");
	//         while (this.isRunning)
	//         {
	//             if (this.sendingMessageQueue.Count == 0)
	//             {
	//                 lock (this.netLock)
	//                 {
	//                     while (this.waitingMessageQueue.Count == 0)
	//                     {
	//                         Monitor.Wait(this.netLock);
	//                     }
	//                     Queue<SenderMessage> temp = this.sendingMessageQueue;
	//                     this.sendingMessageQueue = this.waitingMessageQueue;
	//                     this.waitingMessageQueue = temp;
	//                 }
	//             }
	//             else
	//             {
	//                 while (this.sendingMessageQueue.Count > 0)
	//                 {
	//                     SenderMessage senderMessage = this.sendingMessageQueue.Dequeue();
	//                     byte[] messageBytes = null;
	//                     try
	//                     {
	//                         ThriftCodec.EncodeTLV(senderMessage.Base, out messageBytes);
	//                         this.socket.Send(messageBytes);
	//                     }
	//                     catch (Exception ex)
	//                     {
	//                         Shutdown();
	//                         if (!this.isInitiativeDisconnection)
	//                         {
	//                             NetInMainThread.Instance().SendMessageAsync(new NetDisconnectMessage(ex.Message));
	//                             LogModule.Exception(ex.Message, ex.StackTrace);
	//                         }
	//                         else
	//                         {
	//                             //Logic.Instance().SendMessage(new InitiativeDisconnectionMessage());
	//                         }
	//                     }
	//                     finally
	//                     {
	//                         //LogModule.Log("Send message: " + ThriftTools.ThriftToString(senderMessage.Base));
	//                     }
	//                 }
	//             }
	//         }
	//         this.sendingMessageQueue.Clear();
	//         this.waitingMessageQueue.Clear();
	//         LogModule.Log("Net send thread over!");
	//     }
	// 
	//     private void Receive()
	//     {
	//         LogModule.Log("Socket receive thread start!");
	//         this.socketStream = new BufferedStream(new NetworkStream(this.socket), RECEIVE_BUFFER_SIZE);
	//         while (this.isRunning)
	//         {
	//             try
	//             {
	//                 TBase tBase = null;
	//                 int type = -1;
	//                 ThriftCodec.DecodeTLV(this.socketStream, out tBase, out type);
	// 
	//                 // Client channel response
	//                 if (type == ClientChannelResponse.GetProtocolType())
	//                 {
	//                     ClientChannelResponse channelResponse = (ClientChannelResponse)tBase;
	//                     MemoryStream stream = new MemoryStream(channelResponse.Content);
	//                     type = channelResponse.InternalProtocolType;
	//                     // Create TBase instance
	//                     tBase = ProtocolFactory.Create(type);
	//                     ThriftCodec.Decode(stream, tBase);
	//                 }
	//                 LogModule.Log("Receive Message : " + tBase);
	//                 ReceiverMessage receiverMessage = new ReceiverMessage(tBase);
	//                 NetInMainThread.Instance().SendMessageAsync(receiverMessage);
	// 
	//                 if (this.waitedMsgs != null)
	//                 {// 有正在等待的消息
	//                     for (int i = 0; i < this.waitedMsgs.Length; ++i)
	//                     {
	//                         if (tBase.GetType() == this.waitedMsgs[i] || tBase.GetType() == typeof(S2CErrorNotify))
	//                         {// 等到回包(或者是S2CErrorNotify) 发送消息并清空等待列表
	//                             NetInMainThread.Instance().SendMessageAsync(this.receiveResponseMessage);
	//                             NetInMainThread.Instance().clearWaitedMsgs = true;
	//                             break;
	//                         }
	//                     }
	//                 }
	//             }
	//             catch (Exception ex)
	//             {
	//                 // Thrift parse error
	//                 Shutdown();
	//                 if (!this.isInitiativeDisconnection)
	//                 {
	//                     NetInMainThread.Instance().SendMessageAsync(new NetDisconnectMessage(ex.Message + " serial:" + serialNo));
	//                     LogModule.Exception(ex.Message + " serial:" + serialNo, ex.StackTrace);
	//                     serialNo++;
	//                 }
	//                 else
	//                 {
	//                     //Logic.Instance().SendMessage(new InitiativeDisconnectionMessage());
	//                 }
	//             }
	//         }
	//         LogModule.Log("Net receive thread over!");
	//     }
	// 
	//     private void HandleNetConnectException(string netExceptionInfo)
	//     {
	//         NetInMainThread.Instance().SendMessageAsync(new NetConnectExceptionMessage(netExceptionInfo));
	//     }
	// 
	//     private void HandleNetDisconnectException(string netExceptionInfo)
	//     {
	//         NetInMainThread.Instance().SendMessageAsync(new NetDisconnectMessage(netExceptionInfo));
	//     }
}
