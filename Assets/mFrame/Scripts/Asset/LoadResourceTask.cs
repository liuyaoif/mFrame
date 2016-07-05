using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Utility
{
    public class LoadResourceTask
    {
        public enum ResourceType
        {
            /// <summary>
            /// Unity5的Bundles声明文件
            /// </summary>
            Manifest,

            /// <summary>
            /// 资源:Prefab, Texture, ogg...
            /// </summary>
            Asset,

            /// <summary>
            /// Bundle:被依赖的bundle
            /// </summary>
            Bundle,

            /// <summary>
            /// WWW文本
            /// </summary>
            WWW,

            /// <summary>
            /// 场景
            /// </summary>
            Scene,

            Count,
            Invalid,
        }

        public enum LoadingState
        {
            /// <summary>
            ///Task刚刚实例化
            /// </summary>
            Init,

            /// <summary>
            ///加载依赖bundle
            /// </summary>
            LoadDep,

            /// <summary>
            ///自己的Loader创建了
            /// </summary>
            LoaderCreated,

            /// <summary>
            ///Loader加载完
            /// </summary>
            LoadFinish,

            /// <summary>
            ///回调执行
            /// </summary>
            Done,

            /// <summary>
            ///已经清理了
            /// </summary>
            Disposed,

            /// <summary>
            ///出错
            /// </summary>
            Error,

            Count,
            Invalid,
        }

        public delegate void OnBundleLoaded(AssetBundle bundle);
        public delegate void OnAssetLoaded(UnityEngine.Object obj, object userData = null);//obj可能是GameObject也可能是Asset
        public delegate void OnWwwLoaded(string wwwText);
        public delegate void OnSceneLoaded(string sceneName);
        public delegate void OnLoadError(string error);

        public OnAssetLoaded onAssetReady
        {
            get { return m_onAssetReady; }
        }
        private event OnBundleLoaded m_onBundleReady;
        private event OnAssetLoaded m_onAssetReady;
        private OnWwwLoaded m_onWwwReady;
        private OnSceneLoaded m_onSceneReady;
        private OnLoadError m_onError;

        private AsyncOperation m_loadSceneAsync;

        private string m_assetName;
        private string m_bundleName;
        private WWW m_loader;

        private int m_referenceCount = 0;
        private bool m_isEnabled = false;
        private ResourceType m_curResourceType = ResourceType.Invalid;
        private List<string> m_dependenceTasks;

        //依赖这个Task的其他Task
        private List<string> m_onwerBundleList = new List<string>();

        //依赖这个Task的其他GameObject
        private List<WeakReference> m_ownerObjList = new List<WeakReference>();
        private List<WeakReference> m_removeList = new List<WeakReference>();

        //User defined param.
        private object m_userData;

        public object userData
        {
            get { return m_userData; }
            set { m_userData = value; }
        }

        private LoadingState m_curState = LoadingState.Invalid;

        public LoadingState curState
        {
            get { return m_curState; }

            private set
            {
                if (m_curState != value)
                {
                    m_curState = value;
                    OnStateChanged();
                }
            }
        }

        private AssetBundle m_curBundle = null;
        public AssetBundle curBundle
        {
            get { return m_curBundle; }
        }

        public string assetName
        {
            set { m_assetName = value; }
            get { return m_assetName; }
        }

        public string bundleName
        {
            set { m_bundleName = value; }
            get { return m_bundleName; }
        }

        public OnLoadError OnError
        {
            get { return m_onError; }
            set { m_onError = value; }
        }

        public int ReferenceCount
        {
            get { return m_referenceCount; }
            set { m_referenceCount = value; }
        }

        public bool IsEnabled
        {
            get { return m_isEnabled; }
            set { m_isEnabled = value; }
        }

        public OnWwwLoaded OnWWWReady
        {
            get { return m_onWwwReady; }
            set { m_onWwwReady = value; }
        }

        public OnSceneLoaded OnSceneReady
        {
            get { return m_onSceneReady; }
            set { m_onSceneReady = value; }
        }

        /// <summary>
        ///true:是prefab类型的Asset;false:是纹理、音效类型的Asset
        /// </summary>
        private bool m_isPrefab = false;

        private bool m_allowMultiPrefabInstances = false;

        /// <summary>
        /// 允许这个Task产生多个Asset的实例，必然是m_isPrefab为true
        /// </summary>
        public bool allowMultiPrefabInstances
        {
            get { return m_allowMultiPrefabInstances; }
            set { m_allowMultiPrefabInstances = value; }
        }

        public ResourceType curResourceType
        {
            get { return m_curResourceType; }
            set { m_curResourceType = value; }
        }

        private float m_progress;

        /// <summary>
        /// 加载进度
        /// </summary>
        public float progress
        {
            get { return m_progress; }
        }

        //Bundle and manifest.
        public LoadResourceTask(string bundle, bool isManifestTask = false)
        {
            m_curResourceType = (isManifestTask == true) ? ResourceType.Manifest : ResourceType.Bundle;
            m_bundleName = bundle;
            Init();
        }

        //Asset and scene bundle.
        public LoadResourceTask(string asset, string bundle, ResourceType loadType)
        {
            m_curResourceType = loadType;
            m_assetName = asset;
            m_bundleName = bundle;
            Init();
        }

#if UNITY_EDITOR
        /// <summary>
        /// EDITOR用
        /// </summary>
        public LoadResourceTask()
        {
        }
#endif

        //WWW
        public LoadResourceTask(string fileName, ResourceType type)
        {
            m_curResourceType = type;
            m_bundleName = fileName;
            Init();
        }

        private void Init()
        {
            curState = LoadingState.Init;
            if (m_curResourceType == ResourceType.Asset ||
                m_curResourceType == ResourceType.Bundle ||
                m_curResourceType == ResourceType.Scene)
            {
                if (!GlobalConfigManager.Instance.isUseBundle && m_curResourceType == ResourceType.Scene)
                {
                    curState = LoadingState.LoadFinish;
                    return;
                }
                string[] dependencies = AssetsManagerFor5.Instance.GetBundleDependencies(m_bundleName);
                if (dependencies != null && dependencies.Length > 0)
                {
                    //Load dependences.
                    LoadDepBundles(dependencies);
                }
                else
                {
                    string url = UtilTools.CombineString(AssetsManagerFor5.Instance.rootBundleURL, m_bundleName);
                    m_loader = AssetsManagerFor5.CreateWWW(url);
                    curState = LoadingState.LoaderCreated;
                }
            }
            else if (m_curResourceType == ResourceType.Manifest)//Manifest
            {
                string url = UtilTools.CombineString(AssetsManagerFor5.Instance.rootBundleURL, m_bundleName);
                m_loader = AssetsManagerFor5.CreateWWW(url);
                curState = LoadingState.LoaderCreated;
            }
            else//WWW
            {
                string url = m_bundleName;
                m_loader = AssetsManagerFor5.CreateWWW(url);
                curState = LoadingState.LoaderCreated;
            }
        }

        private void OnStateChanged()
        {
            switch (curState)
            {
                case LoadingState.Error:
                    {
                        if (m_onError != null)
                        {
                            m_onError(m_loader.error);
                        }
                        LogManager.Instance.LogError(m_loader.error);
                        ClearLoader();
                        AssetsManagerFor5.Instance.OnTaskError(this);
                    }
                    break;

                case LoadingState.LoaderCreated:
                    {
                        AssetsManagerFor5.Instance.StartCoroutine(Loading());
                    }
                    break;

                case LoadingState.Done:
                    {
                        AssetsManagerFor5.Instance.OnTaskDone(this);
                    }
                    break;
            }
        }

        private IEnumerator Loading()
        {
            //未启动
            if (!IsEnabled)
            {
                yield return 0;
            }

            //加载中
            while (!m_loader.isDone)
            {
                m_progress = m_loader.progress;
                yield return 0;
            }

            //加载错误
            if (m_loader.error != null)
            {
                curState = LoadingState.Error;
                yield return 0;
            }

            //加载完了
            m_curBundle = m_loader.assetBundle;
            switch (m_curResourceType)
            {
                case ResourceType.WWW:
                    {
                        if (m_onWwwReady != null)
                        {
                            m_onWwwReady(m_loader.text);
                        }
                    }
                    break;

                case ResourceType.Manifest:
                    {
                        if (m_onBundleReady != null)
                        {
                            m_onBundleReady(curBundle);
                        }
                    }
                    break;

                case ResourceType.Bundle:
                    {
                        TouchAllDepBundles();
                        if (m_onBundleReady != null)
                        {
                            m_onBundleReady(curBundle);
                        }
                        RemoveAllDepBundles();
                    }
                    break;

                case ResourceType.Asset:
                    {
                        TouchAllDepBundles();
                        CallAssetReady();
                        RemoveAllDepBundles();
                    }
                    break;

                case ResourceType.Scene:
                    {
                        TouchAllDepBundles();

                        if (m_loadSceneAsync == null)
                        {
                            m_loadSceneAsync = SceneManager.LoadSceneAsync(m_assetName.Replace(".unity", ""));
                        }

                        while (!m_loadSceneAsync.isDone)
                        {
                            yield return 0;
                        }

                        m_onSceneReady(m_assetName.Replace(".unity", ""));
                        RestoreSkyBoxForScene();
                        RemoveAllDepBundles();
                        if (curBundle != null)
                        {
                            curBundle.Unload(false);
                        }
                    }
                    break;
            }
            curState = LoadingState.Done;
        }

        public bool IsError()
        {
            return curState == LoadingState.Error;
        }

        public bool IsDone()
        {
            return curState == LoadingState.Done;
        }

        public void Dispose()
        {
            ClearLoader();

            if (m_curBundle != null)
            {
                if (m_curResourceType == ResourceType.Manifest)
                {
                    m_curBundle.Unload(false);
                }
                else
                {
                    m_curBundle.Unload(m_isPrefab);//是Prefab时，才全部卸载；是资源时，不能全部卸载
                }
                m_curBundle = null;
            }

            if (m_dependenceTasks != null)
            {
                m_dependenceTasks = null;
            }

            m_onwerBundleList = null;
            m_onBundleReady = null;
            m_onError = null;
            m_onAssetReady = null;
            m_onWwwReady = null;
            m_onSceneReady = null;

            m_assetName = null;
            m_bundleName = null;
            IsEnabled = false;
            curState = LoadingState.Disposed;
        }

        private void LoadDepBundles(string[] dependencies)
        {
            m_dependenceTasks = new List<string>();
            for (int i = 0; i < dependencies.Length; i++)
            {
                string depBundleName = dependencies[i];
                LoadResourceTask task = AssetsManagerFor5.Instance.AddBundleTask(depBundleName, null, null, false);
                task.AddBundleOwner(this.m_bundleName);
                m_dependenceTasks.Add(depBundleName);
            }
            curState = LoadingState.LoadDep;
        }

        private void OnDepBundlesLoaded(LoadResourceTask task)
        {
        }

        public bool CheckDepBundles()
        {
            int doneBundleCount = 0;
            bool ret = false;
            if (m_dependenceTasks == null ||
                m_dependenceTasks.Count == 0)
            {
                ret = true;
            }
            else
            {
                ret = true;
                for (int i = 0; i < m_dependenceTasks.Count; i++)
                {
                    string depTaskName = m_dependenceTasks[i];
                    LoadResourceTask depTask = AssetsManagerFor5.Instance.GetBundleTask(depTaskName);
                    if (depTask != null && !depTask.IsDone())
                    {
                        ret = false;
                    }
                    doneBundleCount++;
                }
            }

            //All dependence ready.
            if (ret && m_loader == null)
            {
                string url = UtilTools.CombineString(AssetsManagerFor5.Instance.rootBundleURL, m_bundleName);
                m_loader = AssetsManagerFor5.CreateWWW(url);
                curState = LoadingState.LoaderCreated;
            }
            UpdateProgress(doneBundleCount);
            return ret;
        }

        private void ClearLoader()
        {
            if (m_loader != null)
            {
                m_loader.Dispose();
                m_loader = null;
            }
        }

        private void TouchAllDepBundles()
        {
            if (m_dependenceTasks != null)
            {
                for (int i = 0; i < m_dependenceTasks.Count; i++)
                {
                    LoadResourceTask depTask = AssetsManagerFor5.Instance.GetBundleTask(m_dependenceTasks[i]);
                    AssetBundle bundle = depTask.curBundle;
                    bundle = null;
                }
            }
        }

        public void RemoveAllDepBundles()
        {
            if (HasOwner())
            {
                return;
            }

            if (m_dependenceTasks != null)
            {
                for (int i = 0; i < m_dependenceTasks.Count; i++)
                {
                    LoadResourceTask depTask = AssetsManagerFor5.Instance.GetBundleTask(m_dependenceTasks[i]);
                    depTask.RemoveBundleOwner(this.m_bundleName);
                    depTask.RemoveAllDepBundles();
                }
            }
        }

        public void AddObjectOwner(GameObject go)
        {
            m_ownerObjList.Add(new WeakReference(go));
        }

        public void AddBundleOwner(string ownerName)
        {
            m_onwerBundleList.Add(ownerName);
        }

        public void RemoveBundleOwner(string ownerName)
        {
            m_onwerBundleList.Remove(ownerName);
        }

        public bool HasOwner()
        {
            m_removeList.Clear();
            for (int i = 0; i < m_ownerObjList.Count; i++)
            {
                GameObject target = (GameObject)m_ownerObjList[i].Target;
                if (target == null)
                {
                    m_removeList.Add(m_ownerObjList[i]);
                }
            }

            for (int i = 0; i < m_removeList.Count; i++)
            {
                m_ownerObjList.Remove(m_removeList[i]);
            }

            if (m_ownerObjList.Count != 0 || m_onwerBundleList.Count != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override string ToString()
        {
            return UtilTools.CombineString("B:", m_bundleName, ". A: ", m_assetName, ". T: ", m_curResourceType.ToString());
        }

        public void AddBundleReadyCallback(OnBundleLoaded bundleReady)
        {
            m_onBundleReady += bundleReady;
        }

        public void AddAssetReadyCallback(OnAssetLoaded assetReady)
        {
            m_onAssetReady += assetReady;
            if (IsDone())
            {
                curState = LoadingState.LoadFinish;

                //延迟1帧调用.当前帧后面还要设置userData
                TimerManager.Instance.AddTimer(0.01f,
                    CallAssetReady);

                //CallAssetReady();
            }
        }

        private void CallAssetReady()
        {
            if (m_onAssetReady != null)
            {
                UnityEngine.Object goAsset = curBundle.LoadAsset(m_assetName);
                if (goAsset.GetType() == typeof(GameObject))//Prefab类型
                {
                    if (allowMultiPrefabInstances)
                    {
                        Delegate[] delegateArr = m_onAssetReady.GetInvocationList();

                        for (int i = 0; i < delegateArr.Length; i++)
                        {
                            try
                            {
                                GameObject go = GameObject.Instantiate(curBundle.LoadAsset(m_assetName) as GameObject);
                                go.name = assetName;
                                (delegateArr[i] as OnAssetLoaded)(go, m_userData);
                                AddObjectOwner(go);
                            }
                            catch (Exception e)
                            {
                                LogManager.Instance.LogError(curBundle.LoadAsset(m_assetName).GetType()
                                    + "_" + m_assetName + "_" + e.ToString());
                            }
                        }
                    }
                    else
                    {
                        GameObject go = GameObject.Instantiate(curBundle.LoadAsset(m_assetName) as GameObject);
                        go.name = assetName;
                        m_onAssetReady(go, m_userData);
                        AddObjectOwner(go);
                    }

                    m_isPrefab = true;
                }
                else//资源类型. 纹理、音效等
                {
                    m_onAssetReady(curBundle.LoadAsset(m_assetName), m_userData);
                    m_isPrefab = false;
                }
                m_onAssetReady = null;
            }
        }

        /// <summary>
        /// 为场景类型的Bundle处理天空盒
        /// </summary>
        private void RestoreSkyBoxForScene()
        {
            if (RenderSettings.skybox != null)
            {
                RenderSettings.skybox.shader = Shader.Find(RenderSettings.skybox.shader.name);
            }
        }

        /// <summary>
        /// 计算加载进度
        /// </summary>
        private void UpdateProgress(int doneDepBundleCount)
        {
            if (m_dependenceTasks != null &&
                m_dependenceTasks.Count > 0)//有依赖的bundle. 进度是完成的bundle个数
            {
                m_progress = (float)doneDepBundleCount / (float)(m_dependenceTasks.Count + 1);
            }
            else//没有依赖的bundle. 进度是loader的进度
            {
                if (curState == LoadingState.LoaderCreated)
                {
                    m_progress = m_loader.progress;
                }
                else if (curState == LoadingState.LoadFinish)
                {
                    m_progress = 1f;
                }
                else
                {
                    m_progress = 0f;
                }
            }
        }
    }//LoadResourceTask
}
