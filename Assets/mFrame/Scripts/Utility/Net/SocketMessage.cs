using System;

namespace Utility
{
    public enum LINK_MSG_CODE
    {

    }
    public enum BACKEND_SYSTEM
    {

    }

    public class SocketMessage
    {
        private byte[] m_buffer;
        public byte[] buffer
        {
            get { return m_buffer; }
        }

        private WXMessage m_wxMsg;
        public WXMessage wxMsg
        {
            get { return m_wxMsg; }
        }

        private LINK_MSG_CODE m_linkMsgCode;
        public LINK_MSG_CODE linkMsgCode
        {
            get { return m_linkMsgCode; }
        }

        private BACKEND_SYSTEM m_backEndSystemId;
        public BACKEND_SYSTEM backEndSystemId
        {
            get { return m_backEndSystemId; }
        }

        private static int SEQUENCE_COUNTER = 0;

        public SocketMessage(LINK_MSG_CODE linkCode, BACKEND_SYSTEM backEndSystemId,
            WXMessage protoMessage)
        {
            SEQUENCE_COUNTER++;
            m_linkMsgCode = linkCode;
            m_backEndSystemId = backEndSystemId;
            m_wxMsg = protoMessage;
            m_wxMsg.seqId = SEQUENCE_COUNTER;
        }

        public void Pack()
        {
            byte[] wxBuffer = null;// ProtobufUtility.GetByteFromProtoBuf(m_wxMsg);

            m_buffer = new byte[1 + 1 + wxBuffer.Length];

            //LinkMsgCode转为一个byte写入
            byte[] linkMsgCodeBuffer = BitConverter.GetBytes((char)m_linkMsgCode);
            Array.Copy(linkMsgCodeBuffer, 0, m_buffer, 0, linkMsgCodeBuffer.Length);

            //backendSystemCode转为一个byte写入
            byte[] backendSystemCodeBuffer = BitConverter.GetBytes((char)m_backEndSystemId);
            Array.Copy(backendSystemCodeBuffer, 0, m_buffer, 1, backendSystemCodeBuffer.Length);

            Array.Copy(wxBuffer, 0, m_buffer, 2, wxBuffer.Length);
            //Debug.Log("Sending msg. Id: " + m_wxMsg.msgCode.ToString() + ". size: " + m_buffer.Length);
        }

        public SocketMessage(byte[] cotent)
        {
            if (cotent == null)
            {
                m_buffer = new byte[0];
            }
            else
            {
                m_buffer = cotent;
                //m_wxMsg = (WXMessage)ProtobufUtility.DeserializeProtobuf(m_buffer, typeof(WXMessage));
                //Debug.Log("Receiving msg. Id: " + m_wxMsg.msgCode.ToString() + ". size: " + m_buffer.Length);
            }
        }
    }
}