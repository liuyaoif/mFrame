#if UNITY_EDITOR
# define PROFILER_STATIC_UPDATOR
#endif

//----------------------------------------------
//Use StaticUpdater to fixed-update non-MonoBehaviour classes' function.
//----------------------------------------------

using mFrame.Singleton;
using mFrame.Utility;
using System.Collections;
#if PROFILER_STATIC_UPDATOR
using System.Collections.Generic;
#endif
using UnityEngine;

namespace mFrame.MonoWrap
{
    public sealed class StaticUpdater : Singleton<StaticUpdater>
    {
        public delegate void UpdateCallBack();
        public delegate IEnumerator CoroutineCallBack();

#if !PROFILER_STATIC_UPDATOR
        private event UpdateCallBack m_fixedCallBack;
        private event UpdateCallBack m_renderCallBack;
        private event UpdateCallBack m_lateCallBack;
#else
        private List<UpdateCallBack> m_fixedCallBack = new List<UpdateCallBack>();
        private List<UpdateCallBack> m_renderCallBack = new List<UpdateCallBack>();
        private List<UpdateCallBack> m_lateCallBack = new List<UpdateCallBack>();
#endif

        private static MonoUpdater m_updater;

        //         [RuntimeInitializeOnLoadMethod]
        //         private static void Main()
        //         {
        //             object obj = StaticUpdater.Instance;
        //         }

        public StaticUpdater()
        {
            if (m_updater == null)
            {
                GameObject go = new GameObject("staticUpdater");
                m_updater = go.AddComponent<MonoUpdater>();
                m_updater.owner = this;
            }
        }

        public void AddFixedUpdateCallBack(UpdateCallBack callBack)
        {
#if !PROFILER_STATIC_UPDATOR
            m_fixedCallBack -= callBack;
            m_fixedCallBack += callBack;
#else
            m_fixedCallBack.Add(callBack);
#endif
        }

        public void RemoveFixedUpdateCallBack(UpdateCallBack callBack)
        {
#if !PROFILER_STATIC_UPDATOR
            m_fixedCallBack -= callBack;
#else
            m_fixedCallBack.Remove(callBack);
#endif
        }

        public void AddRenderUpdateCallBack(UpdateCallBack callBack)
        {
#if !PROFILER_STATIC_UPDATOR
            m_renderCallBack -= callBack;
            m_renderCallBack += callBack;
#else
            m_renderCallBack.Add(callBack);
#endif
        }

        public void RemoveRenderUpdateCallBack(UpdateCallBack callBack)
        {
#if !PROFILER_STATIC_UPDATOR
            m_renderCallBack -= callBack;
#else
            m_renderCallBack.Remove(callBack);
#endif
        }

        public void AddLateUpdateCallBack(UpdateCallBack callBack)
        {
#if !PROFILER_STATIC_UPDATOR
            m_lateCallBack += callBack;
#else
            m_lateCallBack.Add(callBack);
#endif
        }

        public void RemoveLateUpdateCallBack(UpdateCallBack callBack)
        {
#if !PROFILER_STATIC_UPDATOR
            m_lateCallBack -= callBack;
#else
            m_lateCallBack.Remove(callBack);
#endif
        }

        public void StartCoroutine(CoroutineCallBack callBack)
        {
            m_updater.StartCoroutine(callBack());
        }

        private sealed class MonoUpdater : MonoBehaviour
        {
            private StaticUpdater m_owner;

            public StaticUpdater owner
            {
                set
                {
                    m_owner = value;
                }
            }

            void Awake()
            {
                UtilTools.SetDontDestroyOnLoad(gameObject);
                //DontDestroyOnLoad(gameObject);
            }

            void Update()
            {
#if !PROFILER_STATIC_UPDATOR
                m_owner.m_renderCallBack();
#else
                for (int i = 0; i < m_owner.m_renderCallBack.Count; i++)
                {
                    m_owner.m_renderCallBack[i]();
                }
#endif
            }

            void FixedUpdate()
            {
#if !PROFILER_STATIC_UPDATOR
                m_owner.m_fixedCallBack();
#else
                for (int i = 0; i < m_owner.m_fixedCallBack.Count; i++)
                {
                    m_owner.m_fixedCallBack[i]();
                }
#endif
            }

            void LateUpdate()
            {
#if !PROFILER_STATIC_UPDATOR
                m_owner.m_lateCallBack();
#else
                for (int i = 0; i < m_owner.m_lateCallBack.Count; i++)
                {
                    m_owner.m_lateCallBack[i]();
                }
#endif
            }
        }
    }//StaticUpdater
}