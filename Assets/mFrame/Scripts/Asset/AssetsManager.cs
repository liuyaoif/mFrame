using mFrame.Log;
using mFrame.Singleton;
using mFrame.Timing;
using mFrame.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace mFrame.Asset
{
    public sealed class AssetsManager : SingletonMonoBehaviour<AssetsManager>
    {
        public struct AssetToBundleInfo
        {
            public string bundleName;
            public string assetPath;
        }
        private string m_rootBundleURL;
        private string m_manifestName;
        private string m_globalConfigName;

        private AssetBundleManifest assetBundleManifest = null;
        private List<LoadResourceTask> bundleTaskList = new List<LoadResourceTask>();
        private Dictionary<string, LoadResourceTask> bundleTaskDict = new Dictionary<string, LoadResourceTask>();
        //private List<LoadResourceTask> lastingBundleTaskList = new List<LoadResourceTask>();
        private List<LoadResourceTask> removeList = new List<LoadResourceTask>();
        //private Dictionary<string, string> assetToBundleDict = new Dictionary<string, string>();//key: asset propName; value: bundle propName.
        private Dictionary<string, AssetToBundleInfo> assetToBundleDict = new Dictionary<string, AssetToBundleInfo>();
        private Action onAssetsManagerReady;
        private bool isConfigReady = false;
        private bool isManifestReady = false;
        private bool isGlobalConfigReady = false;

        public string rootBundleURL
        {
            get { return m_rootBundleURL; }
            set { m_rootBundleURL = value; }
        }

        public Action OnAssetsManagerReady
        {
            set { onAssetsManagerReady = value; }
        }

        public bool IsConfigReady
        {
            set
            {
                isConfigReady = value;
                if (isConfigReady && isManifestReady && isGlobalConfigReady)
                {
                    if (onAssetsManagerReady != null)
                    {
                        onAssetsManagerReady();
                        onAssetsManagerReady = null;
                    }
                }
            }
        }

        public bool IsManifestReady
        {
            set
            {
                isManifestReady = value;
                if (isConfigReady && isManifestReady && isGlobalConfigReady)
                {
                    if (onAssetsManagerReady != null)
                    {
                        onAssetsManagerReady();
                        onAssetsManagerReady = null;
                    }
                }
            }
        }


        public bool IsGlobalConfigReady
        {
            set
            {
                isGlobalConfigReady = value;
                if (isConfigReady && isManifestReady && isGlobalConfigReady)
                {
                    if (onAssetsManagerReady != null)
                    {
                        onAssetsManagerReady();
                        onAssetsManagerReady = null;
                    }
                }
            }
        }

        public void InitAssetsManagerFor5()
        {
            SetFileRoot();

            //Load assetconfig file.
            LoadAssetConfig();

            //Load manifest file.
            LoadManifest();

            //Load global config file.
            LoadGlobalConfig();
        }

        private void SetFileRoot()
        {
            //Temp path. When version update is ready, path will be persistentPath and streamingPath.
#if UNITY_EDITOR
            rootBundleURL = UtilTools.CombineString("file:///", Application.streamingAssetsPath, "/",
                AssetFilter.GetPlatformFolderForAssetBundles(UnityEditor.EditorUserBuildSettings.activeBuildTarget), "/");

            m_globalConfigName = UtilTools.CombineString("file:///", Application.streamingAssetsPath, "/", GlobalConfigManager.CONFIG_FILE_NAME);
#elif UNITY_STANDALONE_WIN
                        rootBundleURL = UtilTools.CombineString("file:///", Application.streamingAssetsPath, "/",
                AssetFilter.GetPlatformFolderForAssetBundles(Application.platform), "/");

                        m_globalConfigName = UtilTools.CombineString("file:///", Application.streamingAssetsPath, "/", GlobalConfigManager.CONFIG_FILE_NAME);
#elif UNITY_IPHONE
			rootBundleURL = UtilTools.CombineString("file://", Application.streamingAssetsPath, "/",
                AssetFilter.GetPlatformFolderForAssetBundles(Application.platform), "/");

            			m_globalConfigName = UtilTools.CombineString("file://", Application.streamingAssetsPath, "/", GlobalConfigManager.CONFIG_FILE_NAME);
#elif UNITY_ANDROID
            rootBundleURL = UtilTools.CombineString(Application.streamingAssetsPath, "/",
                AssetFilter.GetPlatformFolderForAssetBundles(Application.platform), "/");

            	m_globalConfigName = UtilTools.CombineString(Application.streamingAssetsPath, "/",
            GlobalConfigManager.CONFIG_FILE_NAME);
#endif

#if UNITY_EDITOR
            m_manifestName =
                 AssetFilter.GetPlatformFolderForAssetBundles(UnityEditor.EditorUserBuildSettings.activeBuildTarget);
#else
            m_manifestName =
			AssetFilter.GetPlatformFolderForAssetBundles(Application.platform);
#endif
        }

        private void LoadAssetConfig()
        {
            LoadResourceTask.OnWwwLoaded onConfigLoaded = (string wwwText) =>
            {
                //XMLParser xmlParser = new XMLParser();
                //XMLNode rootNode = xmlParser.Parse(wwwText);

                ////Parse assets
                //XMLNode assetRootNode = rootNode.GetNode("PackConfig>0>Assets>0");
                //XMLNodeList assetList = assetRootNode.GetNodeList("AssetConfig");

                //for (int i = 0; i < assetList.Count; i++)
                //{
                //    XMLNode assetNode = assetList[i] as XMLNode;
                //    string assetName = assetNode.GetValue("@AssetName");
                //    string bundleName = assetNode.GetValue("@BundleName");
                //    string assetPath = assetNode.GetValue("@AssetPath");
                //    if (!assetToBundleDict.ContainsKey(assetName))
                //    {
                //        AssetToBundleInfo info = new AssetToBundleInfo();
                //        info.assetPath = assetPath;
                //        info.bundleName = bundleName;
                //        assetToBundleDict.Add(assetName, info);
                //    }

                //}
                IsConfigReady = true;

            };

            LoadResourceTask task = new LoadResourceTask(AssetsManager.Instance.rootBundleURL + "assetconfig.text",
                LoadResourceTask.ResourceType.WWW);
            task.OnWWWReady = onConfigLoaded;
            AddBundleTask(task);
        }

        private void LoadManifest()
        {
            LoadResourceTask.OnBundleLoaded OnManifestReady = (AssetBundle bundle) =>
            {
                assetBundleManifest = bundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                IsManifestReady = true;
            };
            AddBundleTask(m_manifestName, OnManifestReady, null, true);
        }

        //For load asset.
        public LoadResourceTask AddAssetTask(string assetName,
            LoadResourceTask.OnAssetLoaded onAssetReady,
            LoadResourceTask.OnLoadError onError = null)//, LoadResourceTask.TaskCacheType chacheType = LoadResourceTask.TaskCacheType.Discharge)
        {
            if (GlobalConfigManager.Instance.isUseBundle)
            {
                return AddAssetTaskForBundle(assetName, onAssetReady, onError);
            }
            else
            {
                return AddAssetTaskForAssetPath(assetName, onAssetReady, onError);
            }
        }

        /// <summary>
        /// 以Bundle方式加载Asset.全平台通用,必须先打bundle
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="onAssetReady"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public LoadResourceTask AddAssetTaskForBundle(string assetName,
           LoadResourceTask.OnAssetLoaded onAssetReady,
           LoadResourceTask.OnLoadError onError = null)
        {
            if (!assetToBundleDict.ContainsKey(assetName))
            {
                LogManager.Instance.LogWarning(assetName + " not find in asset config.");
                return null;
            }

            AssetToBundleInfo bundleInfo = assetToBundleDict[assetName];
            LoadResourceTask task = AddBundleTask(assetName, bundleInfo.bundleName, LoadResourceTask.ResourceType.Asset, null, onError);
            task.AddAssetReadyCallback(onAssetReady);
            return task;
        }

        /// <summary>
        /// 以局部路径方式直接加载asset.仅Editor下可用
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="onAssetReady"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public LoadResourceTask AddAssetTaskForAssetPath(string assetName,
            LoadResourceTask.OnAssetLoaded onAssetReady,
            LoadResourceTask.OnLoadError onError = null)
        {
#if UNITY_EDITOR
            if (!assetToBundleDict.ContainsKey(assetName))
            {
                LogManager.Instance.LogWarning(assetName + " not find in asset config.");
                return null;
            }

            AssetToBundleInfo assetInfo = assetToBundleDict[assetName];
            LoadResourceTask task = new LoadResourceTask(assetName);
            task.AddAssetReadyCallback(onAssetReady);
            TimerManager.Instance.AddTimer(0.01f, delegate ()
            {
                task.onAssetReady(InstantiateResAsset(assetInfo.assetPath, assetName), task.userData);

            });

            return task;
#else
                return AddAssetTaskForBundle(assetName, onAssetReady, onError);
#endif
        }

        private UnityEngine.Object InstantiateResAsset(string path, string objName)
        {
#if UNITY_EDITOR
            UnityEngine.Object obj = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            if (obj is GameObject)
            {
                GameObject go = GameObject.Instantiate<GameObject>((GameObject)obj);
                go.name = objName;
                return go;
            }
            else
            {
                return obj;
            }
#endif
            return null;
        }

        public T GetAssetData<T>(string assetName)
        {
            if (!assetToBundleDict.ContainsKey(assetName))
            {
                LogManager.Instance.Log(assetName, " not find in config.");
            }
            AssetToBundleInfo bundleInfo = assetToBundleDict[assetName];
            if (bundleTaskDict.ContainsKey(bundleInfo.bundleName))
            {
                return (T)(object)(bundleTaskDict[bundleInfo.bundleName].curBundle.LoadAsset(assetName));
            }
            return (T)(object)null;
        }

        private void LoadGlobalConfig()
        {
            LoadResourceTask.OnWwwLoaded onConfigLoaded = (string wwwText) =>
            {
                GlobalConfigManager.Instance.onConfigLoaded(wwwText);
                IsGlobalConfigReady = true;
            };

            LoadResourceTask task = new LoadResourceTask(m_globalConfigName, LoadResourceTask.ResourceType.WWW);
            task.OnWWWReady = onConfigLoaded;
            AddBundleTask(task);
        }

        public LoadResourceTask AddSceneTask(string sceneName,
                        LoadResourceTask.OnSceneLoaded onSceneReady,
            LoadResourceTask.OnLoadError onError = null)
        {
            if (!assetToBundleDict.ContainsKey(sceneName))
            {
                if (onError != null)
                {
                    onError(UtilTools.CombineString(sceneName, " not find in config."));
                }
                else
                {
                    LogManager.Instance.LogError("No such scene: " + sceneName);
                }
            }

            AssetToBundleInfo bundleInfo = assetToBundleDict[sceneName];
            LoadResourceTask task = null;
            //if (GlobalConfigManager.Instance.isUseBundle)
            //{
            task = AddBundleTask(sceneName, bundleInfo.bundleName, LoadResourceTask.ResourceType.Scene, null, onError);
            task.OnSceneReady = onSceneReady;
            //}
            //else
            //{
            //    task = new LoadResourceTask(sceneName);
            //    SceneManager.LoadSceneAsync(sceneName.Split('.')[0]);
            //    task.OnSceneReady = onSceneReady;
            //}

            return task;
        }

        //WWW type.
        public LoadResourceTask AddBundleTask(LoadResourceTask task)
        {
            if (!bundleTaskDict.ContainsValue(task))
            {
                AddTask(task);
            }
            task.ReferenceCount++;
            task.IsEnabled = true;
            return task;
        }

        //Asset type/Scene type.
        public LoadResourceTask AddBundleTask(string assetName,
            string bundleName,
            LoadResourceTask.ResourceType resType,
            LoadResourceTask.OnBundleLoaded onLoaded = null,
             LoadResourceTask.OnLoadError onError = null)
        {
            LoadResourceTask task = null;
            if (bundleTaskDict.ContainsKey(bundleName))
            {
                task = bundleTaskDict[bundleName];
                task.curResourceType = resType;
            }
            else
            {
                task = new LoadResourceTask(assetName, bundleName, resType);
                AddTask(task);
            }

            task.assetName = assetName;
            task.AddBundleReadyCallback(onLoaded);
            task.OnError = onError;
            task.ReferenceCount++;
            task.IsEnabled = true;
            return task;
        }

        //Bundle/Manifest type.
        public LoadResourceTask AddBundleTask(string bundleName,
            LoadResourceTask.OnBundleLoaded onLoaded = null,
             LoadResourceTask.OnLoadError onError = null,
            bool isManifest = false)
        {
            LoadResourceTask task = null;
            if (bundleTaskDict.ContainsKey(bundleName))
            {
                task = bundleTaskDict[bundleName];
            }
            else
            {
                task = new LoadResourceTask(bundleName, isManifest);
                AddTask(task);
            }

            task.AddBundleReadyCallback(onLoaded);
            task.OnError = onError;
            task.ReferenceCount++;
            task.IsEnabled = true;
            return task;
        }

        private void AddTask(LoadResourceTask task)
        {
            bundleTaskDict.Add(task.bundleName, task);
            bundleTaskList.Add(task);
        }

        public void RemoveBundleTask(string bundleName)
        {
            if (string.IsNullOrEmpty(bundleName))
            {
                return;
            }

            if (bundleTaskDict.ContainsKey(bundleName))
            {
                LoadResourceTask task = bundleTaskDict[bundleName];

                task.IsEnabled = false;
                task.ReferenceCount--;

                if (task.ReferenceCount <= 0)
                {
                    bundleTaskDict.Remove(bundleName);
                    bundleTaskList.Remove(task);
                    task.Dispose();
                }
            }
        }

        public LoadResourceTask GetBundleTask(string bundleName)
        {
            if (bundleTaskDict.ContainsKey(bundleName))
            {
                return bundleTaskDict[bundleName];
            }
            return null;
        }

        public string[] GetBundleDependencies(string bundleName)
        {
            if (assetBundleManifest == null)
            {
                return null;
            }

            return assetBundleManifest.GetDirectDependencies(bundleName);
        }

        public bool HasBundle(string bundleName)
        {
            return bundleTaskDict.ContainsKey(bundleName);
        }

        public static WWW CreateWWW(string url)
        {
            // #if UNITY_EDITOR
            //             WWW www = new WWW(url);
            // #else
            //             WWW www = WWW.LoadFromCacheOrDownload(url, 1);
            // #endif
            return new WWW(url);
        }

        //         public static WWW CreateWWW(string url, Hash128 hash, uint crc)
        //         {
        // #if UNITY_EDITOR
        //             WWW www = new WWW(url);
        // #else
        //             WWW www = WWW.LoadFromCacheOrDownload(url, hash, crc);
        // #endif
        //             return www;
        //         }

        //         private IEnumerator DoWWWAsyncLoad(string url, LoadResourceTask.OnWwwLoaded callBack)
        //         {
        //             WWW www = CreateWWW(url);
        //
        //             while (!www.isDone)
        //             {
        //                 yield return www;
        //             }
        //
        //             if (www.error != null)
        //             {
        //                 LogManager.Instance.Log(www.url + "Error");
        //             }
        //
        //             callBack(www);
        //             www.Dispose();
        //             www = null;
        //         }

        protected override void LogicUpdate()
        {
            for (int i = 0; i < bundleTaskList.Count; i++)
            {
                LoadResourceTask task = bundleTaskList[i];

                if (task.IsError())
                {
                    removeList.Add(task);
                    continue;
                }

                if (task.IsDone())
                {
                    if (!task.HasOwner())
                    {
                        removeList.Add(task);
                    }
                }

                task.UpdateLoading();
            }

            if (removeList.Count > 0)
            {
                for (int i = 0; i < removeList.Count; i++)
                {
                    RemoveBundleTask(removeList[i].bundleName);
                }
                removeList.Clear();
            }
        }

        public string GetRelativePath()
        {
            if (Application.isEditor)
            {
                return "file://" + System.Environment.CurrentDirectory.Replace("\\", "/"); // Use the build output folder directly.
            }
            else if (Application.isWebPlayer)
            {
                return System.IO.Path.GetDirectoryName(Application.absoluteURL).Replace("\\", "/") + "/StreamingAssets";
            }
            else if (Application.isMobilePlatform || Application.isConsolePlatform)
            {
                return Application.streamingAssetsPath;
            }
            else // For standalone player.
            {
                return "file://" + Application.streamingAssetsPath;
            }
        }

        public void ClearUnusedBundles()
        {
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }

        public GameObject LoadFromResource(string assetName)
        {
            return Resources.Load(assetName) as GameObject;
        }

        public GameObject InstantiateFromPrefab(GameObject prefab)
        {
            return GameObject.Instantiate(prefab);
        }
        public AsyncOperation LoadScene(string sceneName, bool isSync)
        {
            if (isSync)
            {
                SceneManager.LoadScene(sceneName);
                return null;
            }
            else
            {
                return SceneManager.LoadSceneAsync(sceneName);
            }
        }

        public LoadResourceTaskGroup AddAssetsGroup(params string[] assets)
        {
            return new LoadResourceTaskGroup(assets);
        }

        public LoadResourceTaskGroup AddAssetsGroup(List<string> assets)
        {
            return new LoadResourceTaskGroup(assets.ToArray());
        }

        public LoadResourceTaskGroup AddAssetsGroup(List<LoadResourceTask> taskList)
        {
            return new LoadResourceTaskGroup(taskList);
        }
        public void DeleteObject(UnityEngine.Object obj)
        {
            GameObject.DestroyImmediate(obj);
        }
    }//AssetsManagerFor5
}