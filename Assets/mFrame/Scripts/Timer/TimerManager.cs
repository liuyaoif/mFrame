using mFrame.Event;
using mFrame.MonoWrap;
using mFrame.Singleton;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace mFrame.Timing
{
    public sealed class TimerManager : Singleton<TimerManager>
    {
        private List<Timer> m_LifeList = new List<Timer>();
        private List<Timer> m_suspendableList = new List<Timer>();

        private bool m_isPause = false;
        public bool IsPause
        {
            get { return m_isPause; }
            set { m_isPause = value; }
        }

        private bool m_isFirstRemoteServerTime = true;
        private long m_remoteServerTimerStart;
        private long m_localServerTimerStart;

        private long m_lastRecieveLocalTime = -1;
        private long m_lastRecieveRemoteTime = -1;

        private long m_serverBeginTick = new DateTime(1970, 1, 1, 0, 0, 0).Ticks; //1970年1月1日刻度
        private const int UTC_OFFSET_HOUR = 8;//东8区.北京时间

        public TimerManager()
        {
            StaticUpdater.Instance.AddRenderUpdateCallBack(Adavance);
        }

        private void PreSceneChange(params object[] obj)
        {
            m_LifeList.Clear();
            m_suspendableList.Clear();
        }

        public override void Dispose()
        {
            for (int i = 0; i < m_LifeList.Count; i++)
            {
                m_LifeList[i].Dispose();
            }

            for (int i = 0; i < m_suspendableList.Count; i++)
            {
                m_suspendableList[i].Dispose();
            }

            m_LifeList.Clear();
            m_suspendableList.Clear();
        }

        public void AddTimer(Timer timer)
        {
            if (timer != null)
            {
                if (timer.Type == Timer.TimerType.LifeCycle)
                {
                    m_LifeList.Add(timer);
                }
                else if (timer.Type == Timer.TimerType.Suspendable)
                {
                    m_suspendableList.Add(timer);
                }
                timer.Begin();
            }
        }

        public Timer AddTimer(float duration, Timer.OnTimerCallBack callBack, Timer.TimerType type = Timer.TimerType.LifeCycle, uint maxHitCount = 1)
        {
            Timer timer = new Timer(duration, callBack, type, maxHitCount);
            AddTimer(timer);
            return timer;
        }

        public void RemoveTimer(Timer timer)
        {
            if (timer != null)
            {
                if (timer.Type == Timer.TimerType.LifeCycle)
                {
                    m_LifeList.Remove(timer);
                }
                else if (timer.Type == Timer.TimerType.Suspendable)
                {
                    m_suspendableList.Remove(timer);
                }
                timer.Dispose();
                timer = null;
            }
        }

        private void Adavance()
        {
            AdvanceLifeTime();

            if (!IsPause)
            {
                AdvanceSuspendable();
            }
        }

        private void AdvanceLifeTime()
        {
            float curTime = Time.realtimeSinceStartup;
            for (int i = 0; i < m_LifeList.Count; i++)
            {
                m_LifeList[i].DeltaUpdate(curTime);
            }
        }

        private void AdvanceSuspendable()
        {
            float curTime = Time.realtimeSinceStartup;
            for (int i = 0; i < m_suspendableList.Count; i++)
            {
                m_suspendableList[i].DeltaUpdate(curTime);
            }
        }

        /// <summary>
        /// Time in second
        /// </summary>
        /// <returns></returns>
        public float TimeSinceStartUp()
        {
            return Time.realtimeSinceStartup;
        }

        #region REMOTE_TIME
        public void OnRecieveServerTime(long serverTime)
        {
            long clientSpan = (DateTime.Now.Ticks - m_lastRecieveLocalTime) / 10000;
            m_lastRecieveLocalTime = DateTime.Now.Ticks;

            long serverSpan = serverTime - m_lastRecieveRemoteTime;
            m_lastRecieveRemoteTime = serverTime;

            //             Debug.Log("ClientSpan: " + clientSpan.ToString() + ". ServerSpan: " + serverSpan.ToString() +
            //                 ". Differ: " + (clientSpan - serverSpan).ToString());

            long clientTime = GetRemoteTime();

            //             Debug.Log("ServerTime: " + serverTime + ". ClientTime: " + clientTime
            //                 + " Differ: " + (clientTime - serverTime).ToString());

            if (clientTime < 0)//第一次
            {
                SetRemoteTime(serverTime);
                EventManager.Instance.DispatchEvent(EventID.ServerTimeReady);
                return;
            }
            else
            {
                if (clientSpan < serverSpan || clientTime > serverTime)
                {
                    SetRemoteTime(serverTime);
                }
            }
        }

        /// <summary>
        /// 更新当前远程时间
        /// </summary>
        /// <param name="remoteTime"></param>
        public void SetRemoteTime(long remoteTime)
        {
            if (m_isFirstRemoteServerTime)
            {
                m_isFirstRemoteServerTime = false;
            }

            m_remoteServerTimerStart = remoteTime;
            m_localServerTimerStart = DateTime.Now.Ticks;

            InitHourlyChime();
        }

        /// <summary>
        /// 获取当前远程时间.ms
        /// </summary>
        /// <returns></returns>
        public long GetRemoteTime()
        {
            if (m_isFirstRemoteServerTime)//远程时间尚未初始化
            {
                return -1;
            }
            long elapsedTime = (DateTime.Now.Ticks - m_localServerTimerStart) / 10000;
            long ret = m_remoteServerTimerStart + elapsedTime;
            return ret;
        }

        public DateTime GetRemoteTimeAsDate()
        {
            long serverTimeMs = GetRemoteTime();//java长整型日期，毫秒为单位
            if (serverTimeMs < 0)
            {
                return default(DateTime);
            }
            long time_tricks = m_serverBeginTick + serverTimeMs * 10000;//日志日期刻度
            DateTime ret = new DateTime(time_tricks).AddHours(UTC_OFFSET_HOUR);//转化为DateTime.服务器时间是UTC时间
            return ret;
        }

        public DateTime GetRemoteTimeAsDate(long timeMillisecond)
        {
            if (timeMillisecond < 0)
            {
                return default(DateTime);
            }
            long serverTimeMs = timeMillisecond;
            DateTime fileTime = DateTime.FromFileTimeUtc(timeMillisecond);
            long time_tricks = m_serverBeginTick + serverTimeMs * 10000;//日志日期刻度
            DateTime ret = new DateTime(time_tricks).AddHours(UTC_OFFSET_HOUR); ;//转化为DateTime.服务器时间已经是北京时间
            return ret;
        }

        /// <summary>
        /// 一个远程时间是否过期
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public bool IsRmoteTimeUp(long time)
        {
            if (m_isFirstRemoteServerTime)//远程时间尚未初始化
            {
                return false;
            }

            return GetRemoteTime() > time;
        }
        #endregion

        public DateTime TimeToDate(long time)
        {
            return new DateTime(time * 10000);
        }

        public TimeSpan SecondToSpan(int second)
        {
            return new TimeSpan(0, 0, second);
        }

        #region 整点报时
        //初始化整点报时
        private void InitHourlyChime()
        {
            long curMillSec = GetRemoteTime() * 10000;
            DateTime date = new DateTime(curMillSec);
            float leftMinitue = (float)(60 - date.Minute);
            AddTimer(leftMinitue * 60f, OnReachHour, Timer.TimerType.LifeCycle, 1);
        }

        private void OnReachHour()
        {
            long curMillSec = GetRemoteTime() * 10000;
            DateTime date = new DateTime(curMillSec);
            EventManager.Instance.DispatchEvent(EventID.HourlyChime, date.Hour);
            AddTimer(60f * 60f, OnReachHour, Timer.TimerType.LifeCycle, 1);
        }
        #endregion
    }//TimerManager
}