using mFrame.MonoWrap;
using System;
using System.Collections.Generic;

namespace mFrame.Asset
{
    public class LoadResourceTaskGroup : IDisposable
    {
        private Dictionary<string, LoadResourceTask> m_taskDict;
        private bool m_isDone = false;
        private Action m_onGroupReady;
        private int m_count;
        private bool isLoadEnd;

        public Action OnGroupReady
        {
            get { return m_onGroupReady; }
            set { m_onGroupReady = value; }
        }

        public Dictionary<string, LoadResourceTask> taskDict
        {
            get { return m_taskDict; }
        }

        public bool isDone
        {
            get { return m_isDone; }
        }

        public int count
        {
            get { return m_count; }
        }

        public LoadResourceTaskGroup(params string[] assetNames)
        {
            m_isDone = false;
            m_taskDict = new Dictionary<string, LoadResourceTask>();

            for (int i = 0; i < assetNames.Length; i++)
            {
                m_taskDict.Add(assetNames[i], AssetsManager.Instance.
                    AddAssetTask(assetNames[i], null, null));
            }
            isLoadEnd = false;
            m_count = m_taskDict.Count;
            StaticUpdater.Instance.AddRenderUpdateCallBack(Update);
        }

        public LoadResourceTaskGroup(List<LoadResourceTask> taskList)
        {
            m_isDone = false;
            m_taskDict = new Dictionary<string, LoadResourceTask>();

            for (int i = 0; i < taskList.Count; i++)
            {
                m_taskDict.Add(taskList[i].assetName,
                    taskList[i]);
            }
            isLoadEnd = false;
            m_count = m_taskDict.Count;
            StaticUpdater.Instance.AddRenderUpdateCallBack(Update);
        }

        private void Update()
        {
            if (isLoadEnd)
            {
                return;
            }

            foreach (var kvp in m_taskDict)
            {
                if (!kvp.Value.IsDone())
                {
                    m_isDone = false;
                    return;
                }
            }

            isLoadEnd = true;
            StaticUpdater.Instance.RemoveRenderUpdateCallBack(Update);

            m_isDone = true;
            if (m_onGroupReady != null)
            {
                m_onGroupReady();
            }
        }

        public void Dispose()
        {
            //             foreach (var kvp in m_taskDict)
            //             {
            //                 kvp.Value.Dispose();
            //             }
            m_taskDict = null;
            m_onGroupReady = null;
        }
    }//LoadResourceTaskGroup
}
