using System;
using System.Net;
using System.Net.Sockets;

namespace mFrame.Net
{
    public class TcpSocketClient
    {
        public delegate void ConnectCallBack(bool isConnected);

        private TcpClient m_tcpClient;
        private bool m_isRecvMsgHead = true;
        private int len;
        private IPEndPoint m_serverPoint;

        public TcpSocketClient()
        {
            m_tcpClient = new TcpClient();
        }

        public void Connect(IPEndPoint endPoint, TcpSocketClient.ConnectCallBack callback, bool sync = true)
        {
            m_serverPoint = endPoint;
            if (sync)
            {
                m_tcpClient.Connect(m_serverPoint);

                m_tcpClient.NoDelay = true;

                if (callback != null)
                {
                    callback(m_tcpClient.Connected);
                }
            }
            else
            {
                m_tcpClient.BeginConnect(m_serverPoint.Address, m_serverPoint.Port, AsyncConnectCallBack, callback);
            }
        }

        private void AsyncConnectCallBack(IAsyncResult asyncResult)
        {
            if (m_tcpClient.Connected)
            {
            }

            TcpSocket.ConnectCallBack connectCallBack = (TcpSocket.ConnectCallBack)asyncResult.AsyncState;
            connectCallBack(m_tcpClient.Connected);
        }


        /// <summary>
        /// 发送消息
        /// </summary>
        public void SendMsg(byte[] msg)
        {
            //消息体结构：消息体长度+消息体
            byte[] data = new byte[4 + msg.Length];
            IntToBytes(msg.Length).CopyTo(data, 0);
            msg.CopyTo(data, 4);
            m_tcpClient.GetStream().Write(data, 0, data.Length);
        }

        /// <summary>
        /// 接收消息
        /// </summary>
        public void ReceiveMsg()
        {
            NetworkStream stream = m_tcpClient.GetStream();
            if (!stream.CanRead)
            {
                return;
            }
            //读取消息体的长度
            if (m_isRecvMsgHead)
            {
                if (m_tcpClient.Available < 4)
                {
                    return;
                }
                byte[] lenByte = new byte[4];
                stream.Read(lenByte, 0, 4);
                len = BytesToInt(lenByte, 0);
                m_isRecvMsgHead = false;
            }
            //读取消息体内容
            if (!m_isRecvMsgHead)
            {
                if (m_tcpClient.Available < len)
                {
                    return;
                }
                byte[] msgByte = new byte[len];
                stream.Read(msgByte, 0, len);
                m_isRecvMsgHead = true;
                len = 0;
                if (onRecMsg != null)
                {
                    //处理消息
                    onRecMsg(msgByte);
                }
            }

        }

        /// <summary>
        /// bytes转int
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static int BytesToInt(byte[] data, int offset)
        {
            int num = 0;
            for (int i = offset; i < offset + 4; i++)
            {
                num <<= 8;
                num |= (data[i] & 0xff);
            }
            return num;
        }

        /// <summary>
        /// int 转 bytes
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static byte[] IntToBytes(int num)
        {
            byte[] bytes = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                bytes[i] = (byte)(num >> (24 - i * 8));
            }
            return bytes;
        }

        public delegate void OnRevMsg(byte[] msg);

        public OnRevMsg onRecMsg;
    }
}

