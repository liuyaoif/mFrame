using mFrame.Log;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace mFrame.Net
{
    public class TcpSocket
    {
        public enum SockState
        {
            Initial,
            Connecting,
            Connected,
            Closed,
            Error,
        };

        public delegate void ErrorHandleCallback(Exception exception, string ip, int port);
        public delegate void ConnectCallBack(bool isConnected);

        public const int HEAD_LENGTH = 2;//2 bytes contain msg length.
        public const int SEND_BUFFER_MAX_SIZE = 1024 * 2;
        public const int RECV_BUFFER_MAX_SIZE = 1024 * 4;
        //private const int MAX_SEND
        private const float PACKAGE_COUNT_INTERVAL = 1f;//发包个数统计时间间隔. 1sec
        private const int MAX_PACKAGE_COUNT = 25;//间隔时间中, 最大发包个数

        private long m_lastMoment;//上次发包统计启示时刻
        private int m_curSentCount;//当前间隔中发包个数

        private SockState m_curState = SockState.Initial;

        public SockState curState
        {
            set { m_curState = value; }
            get { return m_curState; }
        }

        private Socket m_socket;
        private Exception m_exception = null;

        private Queue<SocketMessage> m_sendQueue = new Queue<SocketMessage>();
        private Queue<SocketMessage> m_recvQueue = new Queue<SocketMessage>();

        private IPEndPoint m_point;

        private bool m_isRecvHead = true;

        private Thread m_sendingThread;
        private Thread m_recvThread;

        public bool Connect(IPEndPoint endPoint)
        {
            if (m_curState == SockState.Connected)
            {
                LogManager.Instance.LogError("Is connected!");
                return true;
            }

            m_point = endPoint;
            InitSocket();

            try
            {
                m_socket.Connect(m_point);
            }
            catch (Exception ex)
            {
                LogManager.Instance.LogError(ex.ToString());
                curState = SockState.Error;
                HandleException(ex);
                return false;
            }

            if (!m_socket.Connected)
            {
                LogManager.Instance.LogError("Connect failed: " + m_point.ToString());
                curState = SockState.Error;
                return false;
            }
            OnConnectSuccess();
            return true;
        }

        public void AsyncConnect(IPEndPoint endPoint, TcpSocket.ConnectCallBack callback)
        {
            if (m_curState == SockState.Connected || m_curState == SockState.Connecting)
            {
                LogManager.Instance.LogError("Is connected!");
                return;
            }

            InitSocket();

            try
            {
                m_socket.BeginConnect(endPoint, AsyncConnectCallBack, callback);
            }
            catch (Exception ex)
            {
                LogManager.Instance.LogError(ex.ToString());
                m_curState = SockState.Error;
                HandleException(ex);
                return;
            }
        }

        private void AsyncConnectCallBack(IAsyncResult asyncResult)
        {
            if (m_socket.Connected)
            {
                OnConnectSuccess();
            }
            else
            {
                curState = SockState.Closed;
            }

            ConnectCallBack connectCallBack = (ConnectCallBack)asyncResult.AsyncState;
            connectCallBack(m_socket.Connected);
        }

        private void InitSocket()
        {
            m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            m_socket.Blocking = true;
            m_socket.NoDelay = true;
            curState = SockState.Connecting;
        }

        private void OnConnectSuccess()
        {
            m_sendQueue.Clear();
            m_recvQueue.Clear();
            curState = SockState.Connected;

            m_sendingThread = new Thread(RunSending);
            m_sendingThread.Start();
            m_recvThread = new Thread(RunRecieving);
            m_recvThread.Start();
        }

        public void ShutDown()
        {
            m_curState = SockState.Closed;
            if (m_sendingThread != null)
                m_sendingThread.Abort();
            m_recvThread.Abort();
            if (m_sendingThread != null)
                m_sendingThread.Join();
            m_recvThread.Join();

            m_socket.Shutdown(SocketShutdown.Both);
            m_socket.Close();
            m_socket = null;

            m_sendQueue.Clear();
            m_recvQueue.Clear();
        }

        public void SendMsg(SocketMessage msg)
        {
            if (m_curState == SockState.Connected)
            {
                lock (m_sendQueue)
                {
                    m_sendQueue.Enqueue(msg);
                }
            }
            else
            {
                LogManager.Instance.LogWarning("server not connected");
            }
        }

        public SocketMessage GetReceivedMessage()
        {
            SocketMessage msg = null;
            if (m_recvQueue.Count > 0)
            {
                lock (m_recvQueue)
                {
                    msg = m_recvQueue.Dequeue();
                }
            }
            return msg;
        }

        public Exception Exception { get { return m_exception; } }

        private void RunSending(object param)
        {
            while (SockState.Connected == m_curState)
            {
                try
                {
                    if (!m_socket.Poll(-1, SelectMode.SelectWrite))
                    {
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    HandleException(ex);
                    continue;
                }

                byte[] sendBuffer = new byte[SEND_BUFFER_MAX_SIZE];
                int sendSize = 0;

                lock (m_sendQueue)
                {
                    long now = DateTime.Now.Ticks / 10000;//nanosec->ms
                    if (now - m_lastMoment < 1000)//1sec
                    {

                        while (m_sendQueue.Count > 0)
                        {
                            if (m_curSentCount < MAX_PACKAGE_COUNT)
                            {
                                SocketMessage sendNetMsg;

                                if (sendSize == 0)
                                {
                                    sendNetMsg = m_sendQueue.Dequeue();

                                    byte[] msgBytes = CreateSendingBuffer(sendNetMsg);
                                    Array.Copy(msgBytes, sendBuffer, msgBytes.Length);
                                    sendSize += msgBytes.Length;
                                    m_curSentCount++;
                                }
                                else
                                {
                                    sendNetMsg = m_sendQueue.Peek();
                                    byte[] msgBytes = CreateSendingBuffer(sendNetMsg);
                                    if (sendSize + msgBytes.Length > SEND_BUFFER_MAX_SIZE)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        sendNetMsg = m_sendQueue.Dequeue();

                                        Array.Copy(msgBytes, 0, sendBuffer, sendSize, msgBytes.Length);
                                        sendSize += msgBytes.Length;
                                        m_curSentCount++;
                                    }

                                }
                            }
                            else
                            {
                                Debug.Log("超包" + m_sendQueue.Count);
                                break;
                            }
                        }
                    }
                    else
                    {
                        m_lastMoment = now;
                        m_curSentCount = 0;
                    }
                }

                if (sendSize > 0)
                {
                    try
                    {
                        int sendedTotalSize = 0;
                        while (sendedTotalSize < sendSize)
                        {
                            int size = m_socket.Send(sendBuffer, sendedTotalSize, sendSize - sendedTotalSize, SocketFlags.None);
                            sendedTotalSize += size;
                        }
                    }
                    catch (Exception ex)
                    {
                        HandleException(ex);
                    }
                }
            }
        }

        private void RunRecieving(object param)
        {
            while (SockState.Connected == m_curState)
            {
                try
                {
                    if (!m_socket.Poll(-1, SelectMode.SelectRead))
                    {
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    HandleException(ex);
                    continue;
                }

                try
                {
                    int recvSize = 0;

                    ushort bodyLength = 0;
                    if (m_isRecvHead)//Receive head buffer.
                    {
                        if (m_socket == null || !m_socket.Connected || m_socket.Available < HEAD_LENGTH)
                        {
                            continue;
                        }
                        byte[] headBuffer = new byte[HEAD_LENGTH];
                        recvSize = m_socket.Receive(headBuffer, 0, HEAD_LENGTH, SocketFlags.None);
                        bodyLength = MsgHeadToBodyLength(headBuffer);
                        m_isRecvHead = false;
                    }

                    if (!m_isRecvHead)//Receive body buffer.
                    {
                        if (m_socket.Available < bodyLength)
                        {
                            continue;
                        }
                        byte[] bodyBuffer = new byte[bodyLength];
                        recvSize = m_socket.Receive(bodyBuffer, 0, bodyLength, SocketFlags.None);
                        m_isRecvHead = true;

                        SocketMessage newMsg = new SocketMessage(bodyBuffer);
                        lock (m_recvQueue)
                        {
                            m_recvQueue.Enqueue(newMsg);
                        }
                    }
                }
                catch (Exception ex)
                {
                    HandleException(ex);
                }
            }
        }

        public static byte[] CreateSendingBuffer(SocketMessage msg)
        {
            msg.Pack();
            byte[] buffers = new byte[HEAD_LENGTH + msg.buffer.Length];

            //Copy msg head.
            byte[] headBuffer = BodyLengthToMsgHead((ushort)msg.buffer.Length);
            Array.Copy(headBuffer, buffers, headBuffer.Length);

            //Copy msg body.
            Array.Copy(msg.buffer, 0, buffers, headBuffer.Length, msg.buffer.Length);
            return buffers;
        }

        public static byte[] BodyLengthToMsgHead(ushort bodyLength)
        {
            //See IPAddress.HostToNetworkOrder. To bigEndian.
            ushort bigEndianNumber = (ushort)IPAddress.HostToNetworkOrder((short)bodyLength);
            return BitConverter.GetBytes(bigEndianNumber);
        }

        public static ushort MsgHeadToBodyLength(byte[] headByte)
        {
            short length = BitConverter.ToInt16(headByte, 0);
            return (ushort)IPAddress.NetworkToHostOrder(length);
        }

        public static void HandleException(Exception ex)
        {
            LogManager.Instance.LogError(ex.StackTrace);
            if (ex is ObjectDisposedException)
            {
                LogManager.Instance.LogError("The Socket has been closed");
            }
            else if (ex is SocketException)
            {
                SocketException sockException = ex as SocketException;
                LogManager.Instance.LogError("SocketException ErrorCode = " + sockException.ErrorCode);
            }
        }
    }
}