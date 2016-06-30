using System;
using System.Net;

namespace Utility
{
    public class WXMessage
    {
        public int msgCode;
        public int seqId;
    }

    public interface IResponseManager
    {
        void AddResponseCallBack(int seqId, SocketManager.OnResponse response);
        void OnSocketRecieve(WXMessage msg);
    }

    public class SocketManager : Singleton<SocketManager>
    {
        public delegate void HandleNetExceptionCallBack(Exception ex);
        public delegate void OnConnectedCallBack(bool isConnected);
        public delegate void OnResponse(WXMessage msg);

        //private SocketMessageDispatcher m_dispatcher;
        private OnConnectedCallBack m_onConnectedCallBack;
        private TcpSocket m_tcpSocket;
        private UdpSocket m_udpSocket;

        private const float HEART_BEAT_INTERVAL = 60;//sec
        private Timer m_heartBeatTimer;

        private IResponseManager m_rspMgr;

        private bool m_connectResult = false;

        private void Init()
        {
            StaticUpdater.Instance.AddFixedUpdateCallBack(Update);
            m_tcpSocket = new TcpSocket();
            m_udpSocket = new UdpSocket();
        }

        #region TCP
        public void TcpConnect(IPEndPoint point, OnConnectedCallBack callback, bool isAsync)
        {
            Init();
            m_onConnectedCallBack = callback;
            if (isAsync)
            {
                m_tcpSocket.AsyncConnect(point, OnTcpConnected);
            }
            else
            {
                bool isConnected = m_tcpSocket.Connect(point);
                if (m_onConnectedCallBack != null)
                {
                    m_onConnectedCallBack(isConnected);
                    SetHeartBeatEnable(true);
                    m_onConnectedCallBack = null;
                }
            }
        }

        private void OnTcpConnected(bool result)
        {
            m_connectResult = result;
        }

        //         public void AddMessageHandler(int id, SocketMessageDispatcher.HandleCallBack handle)
        //         {
        //             if (m_dispatcher != null)
        //             {
        //                 m_dispatcher.AddMessageHandler(id, handle);
        //             }
        //         }
        //
        //         public void RemoveMessageHandler(int id, SocketMessageDispatcher.HandleCallBack handle)
        //         {
        //             if (m_dispatcher != null)
        //             {
        //                 m_dispatcher.RemoveMessageHandler(id, handle);
        //             }
        //         }


        private void OnTcpReceivedMessage()
        {
            SocketMessage msg = m_tcpSocket.GetReceivedMessage();
            while (msg != null)
            {
                //m_dispatcher.ReceiveNetMsg(msg);
                m_rspMgr.OnSocketRecieve(msg.wxMsg);
                msg = m_tcpSocket.GetReceivedMessage();
            }
        }

        private void ProcessTcpException()
        {
            if (m_tcpSocket.curState == TcpSocket.SockState.Error)
            {
                Exception exception = m_tcpSocket.Exception;
                if (null != exception)
                {
                    LogManager.Instance.LogError(exception.ToString());
                }

                ShutDown();
                LogManager.Instance.LogWarning("Please connect again later");
                SetHeartBeatEnable(false);
            }
        }

        public bool IsTcpConnected()
        {
            return m_tcpSocket.curState == TcpSocket.SockState.Connected;
        }

        private void SetHeartBeatEnable(bool isEnable)
        {
            if (isEnable)
            {
                SendHeartBeatReq();
                m_heartBeatTimer = TimerManager.Instance.AddTimer(HEART_BEAT_INTERVAL, SendHeartBeatReq,
                    Timer.TimerType.LifeCycle, int.MaxValue);
            }
            else
            {
                if (m_heartBeatTimer != null)
                {
                    TimerManager.Instance.RemoveTimer(m_heartBeatTimer);
                }
            }
        }

        private void SendHeartBeatReq()
        {
            //SocketMessage socketMsg = ProtobufUtility.CreateLinkRequest(LINK_MSG_CODE.HEART_BEAT_REQUEST);
            //Send(socketMsg, null);
        }
        #endregion

        public void Send(SocketMessage socketMsg, OnResponse response, bool isTcp = true)
        {
            LogManager.Instance.Log("send message code" + socketMsg.wxMsg.msgCode);
            if (isTcp)
            {
                m_tcpSocket.SendMsg(socketMsg);
            }
            else
            {
                m_udpSocket.Send(socketMsg);
            }
            m_rspMgr.AddResponseCallBack(socketMsg.wxMsg.seqId, response);
        }

        private void Update()
        {
            if (m_onConnectedCallBack != null)
            {
                m_onConnectedCallBack(m_connectResult);
                m_onConnectedCallBack = null;
                if (m_connectResult)
                {
                    SetHeartBeatEnable(true);
                }
            }

            OnTcpReceivedMessage();
            ProcessTcpException();
        }

        public override void Dispose()
        {
            ShutDown();
        }

        public void ShutDown()
        {
            if (m_tcpSocket != null)
            {
                m_tcpSocket.ShutDown();
            }
        }

        public void SetResponseManager(IResponseManager rspMgr)
        {
            m_rspMgr = rspMgr;
        }
    }//TcpSocketManager
}