
namespace mFrame.Net
{
    public class ProtobufUtility
    {
        /*
        private static ProtobufSerializer _serializer = null;
        public static ProtobufSerializer serializer
        {
            get
            {
                if (_serializer == null)
                {
                    _serializer = new ProtobufSerializer();
                }
                return _serializer;
            }
        }

        public static SocketMessage CreateLinkRequest(LINK_MSG_CODE linkMsgCode)
        {
            WXMessage wxMsg = new WXMessage();
            wxMsg.msgCode = (int)linkMsgCode;
            wxMsg.request = new Request();
            SocketMessage socketMsg = new SocketMessage(linkMsgCode, BACKEND_SYSTEM.LINK_SERVER, wxMsg);
            return socketMsg;
        }

        public static SocketMessage CreateWorldRequest(WORLD_MSG_CODE worldMsgCode)
        {
            WXMessage wxMsg = new WXMessage();
            wxMsg.msgCode = (int)worldMsgCode;
            wxMsg.request = new Request();
            SocketMessage socketMsg = new SocketMessage(LINK_MSG_CODE.TRANSFER_REQUEST,
                BACKEND_SYSTEM.WORLD_SERVER, wxMsg);
            return socketMsg;
        }

        public static SocketMessage CreateChatRequest(CHAT_MSG_CODE chatMsgCode)
        {
            WXMessage wxMsg = new WXMessage();
            wxMsg.msgCode = (int)chatMsgCode;
            wxMsg.request = new Request();
            SocketMessage socketMsg = new SocketMessage(LINK_MSG_CODE.TRANSFER_REQUEST,
                BACKEND_SYSTEM.WORLD_SERVER, wxMsg);
            return socketMsg;
        }

        public static WXMessage CreateTestWxMessge(WORLD_MSG_CODE msgCode)
        {
    #if UNITY_EDITOR
            WXMessage wxMsg = new WXMessage();
            wxMsg.msgCode = (int)msgCode;
            wxMsg.response = new Response();
            wxMsg.response.result = (int)RESULT.SUCCESSED;
            return wxMsg;
    #else
            return null;
    #endif
        }

        public static byte[] GetByteFromProtoBuf(object protobufData_)
        {
            byte[] buffer = null;
            using (MemoryStream m = new MemoryStream())
            {
                serializer.Serialize(m, protobufData_);
                m.Position = 0;
                int length = (int)m.Length;
                buffer = new byte[length];
                m.Read(buffer, 0, length);
            }
            return buffer;
        }

        public static T DeserializeProtobuf<T>(byte[] buffer) where T : class
        {

            using (MemoryStream m = new MemoryStream(buffer))
            {
                return serializer.Deserialize(m, null, typeof(T)) as T;
            }
        }

        public static object DeserializeProtobuf(byte[] buffer, System.Type type)
        {
            using (MemoryStream m = new MemoryStream(buffer))
            {
                return serializer.Deserialize(m, null, type);
            }
        }
        */
    }
}
