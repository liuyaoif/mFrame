using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Utility
{
    public class UdpSocket
    {
        //Send
        private IPEndPoint m_remoteEndPoint;
        private UdpClient m_sendClient;
        private Thread m_sendingThread;
        private Queue<SocketMessage> m_sendQueue = new Queue<SocketMessage>();

        //Receive
        private int m_recvPort;
        private UdpClient m_recvClient;
        private Thread m_receiveThread;
        private Queue<SocketMessage> m_recvQueue = new Queue<SocketMessage>();

        private bool m_isRunning = false;

        public void Init(string remoteIP, int remotePort, int recvPort)
        {
            m_isRunning = true;

            m_remoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteIP), remotePort);
            m_sendClient = new UdpClient();
            m_sendingThread = new Thread(new ThreadStart(SendingThread));
            m_sendingThread.Start();

            m_recvPort = recvPort;
            m_recvClient = new UdpClient(m_recvPort);
            m_receiveThread = new Thread(new ThreadStart(RecievingThread));
            m_receiveThread.IsBackground = true;
            m_receiveThread.Start();
        }

        public void Send(SocketMessage message)
        {
            lock (m_sendQueue)
            {
                m_sendQueue.Enqueue(message);
            }
        }

        private void SendingThread()
        {
            while (true)
            {
                if (!m_isRunning)
                {
                    Thread.CurrentThread.Abort();
                    return;
                }

                if (m_sendQueue.Count == 0)
                {
                    continue;
                }

                lock (m_sendQueue)
                {
                    while (m_sendQueue.Count > 0)
                    {
                        SocketMessage sendNetMsg;
                        sendNetMsg = m_sendQueue.Dequeue();
                        byte[] msgBytes = sendNetMsg.buffer;// TcpSocket.CreateSendingBuffer(sendNetMsg);
                        m_sendClient.Send(msgBytes, msgBytes.Length, m_remoteEndPoint);
                    }
                }
            }
        }

        private void RecievingThread()
        {
            while (true)
            {
                if (!m_isRunning)
                {
                    Thread.CurrentThread.Abort();
                    return;
                }

                try
                {
                    IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                    byte[] buffer = m_recvClient.Receive(ref anyIP);

                    //buffer to protocol
                    SocketMessage newMsg = new SocketMessage(buffer);
                    lock (m_recvQueue)
                    {
                        m_recvQueue.Enqueue(newMsg);
                    }

                    lock (m_recvQueue)
                    {
                        m_recvQueue.Enqueue(newMsg);
                    }
                }
                catch (Exception err)
                {
                    LogManager.Instance.Log(err.ToString());
                }
            }
        }

        public SocketMessage GetRespone()
        {
            SocketMessage ret = null;

            if (m_recvQueue.Count > 0)
            {
                lock (m_recvQueue)
                {
                    ret = m_recvQueue.Dequeue();
                }
            }
            return ret;
        }

        public void Dispose()
        {
            if (m_receiveThread != null)
            {
                m_receiveThread.Abort();
            }

            m_recvClient.Close();

            if (m_sendingThread != null)
            {
                m_sendingThread.Abort();
            }

            m_sendClient.Close();
            m_isRunning = false;
        }
    }
}
