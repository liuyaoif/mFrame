using System.Collections.Generic;

namespace Utility
{
    public interface IEvent
    {
        int eventId { get; }
    }

    public class EventManager : Singleton<EventManager>
    {
        public EventID currDispatchEventId { get { return m_currDispatchEventId; } }

        public delegate void OnEventCallBack(params object[] obj);

        private Dictionary<int, OnEventCallBack> m_callBackDict =
            new Dictionary<int, OnEventCallBack>();

        private EventID m_currDispatchEventId;

        public void AddEventListener(EventID eventID, OnEventCallBack callBack)
        {
            if (!m_callBackDict.ContainsKey((int)eventID))
            {
                m_callBackDict.Add((int)eventID, callBack);
            }
            else
            {
                m_callBackDict[(int)eventID] -= callBack;
                m_callBackDict[(int)eventID] += callBack;
            }
        }

        public static EventManager GetThis()
        {
            return Instance;
        }

        public void RemoveEventListener(EventID eventID, OnEventCallBack callBack)
        {
            //Delay 1 frame to remove.
            Timer.OnTimerCallBack OnTimer = () =>
                {
                    if (!m_callBackDict.ContainsKey((int)eventID))
                    {
                        return;
                    }
                    else
                    {
                        m_callBackDict[(int)eventID] -= callBack;
                        if (m_callBackDict[(int)eventID] == null)
                        {
                            m_callBackDict.Remove((int)eventID);
                        }
                    }
                };

            TimerManager.Instance.AddTimer(UtilTools.FrameToDuration(1), OnTimer);
        }

        public void DispatchEvent(EventID eventID, params object[] obj)
        {
            if (m_callBackDict.ContainsKey((int)eventID))
            {
                if (m_callBackDict[(int)eventID] != null)
                {
                    m_currDispatchEventId = eventID;
                    m_callBackDict[(int)eventID](obj);
                }
            }
        }

        public override void Dispose()
        {
            m_callBackDict.Clear();
        }
    }
}