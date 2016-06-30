using mFrame.Timing;
using mFrame.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace mFrame.MonoWrap
{
    public class Entity : MonoBehaviour, IDisposable
    {
        private GameObject m_cachedGameObject;
        private Transform m_cachedTransform;

        public GameObject cachedGameObject
        {
            get
            {
                if (null == m_cachedGameObject)
                {
                    m_cachedGameObject = gameObject;
                }
                return m_cachedGameObject;
            }
        }

        public Transform cachedTransform
        {
            get
            {
                if (null == m_cachedTransform)
                {
                    m_cachedTransform = transform;
                }
                return m_cachedTransform;
            }
        }

        private int m_hashCode;

        public int HashCode
        {
            get { return m_hashCode; }
            set { m_hashCode = value; }
        }

        protected void DelayCall(float duration, Timer.OnTimerCallBack callBack)
        {
            TimerManager.Instance.AddTimer(duration, callBack);
        }

        protected void DelayCall(int frame, Timer.OnTimerCallBack callBack)
        {
            float duration = UtilTools.FrameToDuration(frame);
            TimerManager.Instance.AddTimer(duration, callBack);
        }


        #region Invalid
        private List<string> m_invalidList;
        public const string INVALID_ALL = "All";

        protected void Invalidate(string invalidString)
        {
            if (null == m_invalidList)
            {
                m_invalidList = new List<string>();
            }
            m_invalidList.Add(invalidString);
        }

        protected bool IsValid(string invalidString)
        {
            if (m_invalidList == null ||
                m_invalidList.Count == 0)
            {
                return false;
            }

            if (m_invalidList.Contains(INVALID_ALL))
            {
                return true;
            }

            return m_invalidList.Contains(invalidString);
        }
        #endregion

        #region Entity life cycle
        virtual protected void PreInit() { }
        virtual protected void Init() { }
        virtual protected void RenderUpdate() { }
        virtual protected void LogicUpdate() { }
        virtual protected void LateRenderUpdate() { }
        virtual protected void Draw() { }
        virtual protected void OnDestruct() { }
        virtual public void Dispose() { }

        private void FixStepUpdate()
        {
            if (null != m_invalidList && m_invalidList.Count > 0)
            {
                Draw();
                m_invalidList.Clear();
            }
            LogicUpdate();
        }

        #endregion

        #region MonoBehaviour life cycle
        void Awake()
        {
            PreInit();
            StaticUpdater.Instance.AddRenderUpdateCallBack(RenderUpdate);
            StaticUpdater.Instance.AddFixedUpdateCallBack(FixStepUpdate);
            StaticUpdater.Instance.AddLateUpdateCallBack(LateRenderUpdate);
        }

        // Use this for initialization
        void Start()
        {
            Init();
        }

        void OnDestroy()
        {
            StaticUpdater.Instance.RemoveRenderUpdateCallBack(RenderUpdate);
            StaticUpdater.Instance.RemoveFixedUpdateCallBack(FixStepUpdate);
            StaticUpdater.Instance.RemoveLateUpdateCallBack(LateRenderUpdate);
            OnDestruct();
        }
        #endregion
    }//Entity
}