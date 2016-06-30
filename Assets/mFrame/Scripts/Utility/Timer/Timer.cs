﻿using UnityEngine;

namespace Utility
{
    public class Timer
    {
        public enum TimerType
        {
            LifeCycle,//Cannot be paused.
            Suspendable,//Can be paused.
            Count,
            Invalid,
        }

        public delegate void OnTimerCallBack();
        private float m_maxDuration;

        public float maxDuration
        {
            get { return m_maxDuration; }
            set { m_maxDuration = value; }
        }

        private int m_curHitCount;
        private uint m_maxHitCount;

        private OnTimerCallBack m_callBack;

        private TimerType m_type = TimerType.Invalid;
        public TimerType Type
        {
            get { return m_type; }
        }

        private int m_timerGuid;

        public int TimerGuid
        {
            get { return m_timerGuid; }
        }

        private bool m_isEnabled = true;
        public bool IsEnabled
        {
            get { return m_isEnabled; }

            set
            {
                if (m_type == TimerType.Suspendable)
                {
                    m_isEnabled = value;
                }
                else
                {
                    m_isEnabled = true;
                }
            }
        }

        public string timerName
        {
            set;
            get;
        }

        private float m_startTime;

        public Timer(int frame, OnTimerCallBack callBack, TimerType type = TimerType.LifeCycle, uint maxHitCount = 1)
        {
            m_maxDuration = UtilTools.FrameToDuration(frame);
            m_callBack = callBack;
            m_type = type;
            m_maxHitCount = maxHitCount;
            m_timerGuid = UtilTools.GenerateSeed();
        }

        public Timer(float duration, OnTimerCallBack callBack, TimerType type = TimerType.LifeCycle, uint maxHitCount = 1)
        {
            m_maxDuration = duration;
            m_callBack = callBack;
            m_type = type;
            m_maxHitCount = maxHitCount;
            m_timerGuid = UtilTools.GenerateSeed();
        }

        public void Begin()
        {
            m_startTime = Time.realtimeSinceStartup;
        }

        public void DeltaUpdate(float nowTime)//Executed by FixedUpdate
        {
            if (!IsEnabled)
            {
                return;
            }

            float duration = nowTime - m_startTime;
            if (duration >= m_maxDuration)
            {
                m_callBack();
                m_curHitCount++;

                if (m_curHitCount >= m_maxHitCount)
                {
                    TimerManager.Instance.RemoveTimer(this);
                }
                else
                {
                    m_startTime = nowTime;
                }
            }
        }

        public void Dispose()
        {
            m_maxDuration = 0;
            m_callBack = null;
            m_type = TimerType.Invalid;
            m_curHitCount = 0;
            m_maxHitCount = 0;
        }
    }//Timer
}//Timer


