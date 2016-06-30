using System.Collections.Generic;
using UnityEngine;

namespace Utility
{
    public enum UICameraType
    {
        Normal,
        TopMost,
    }

    public enum PanelOpenType
    {
        /// <summary>
        ///Normal layer
        /// </summary>
        StandAlone,

        PopUp,

        /// <summary>
        /// TopMost layer
        /// </summary>
        TopMost,

        /// <summary>
        /// PopUp layer, but hide.
        /// </summary>
        PopUpHide,

        Count,
        Invalid,
    }

    public enum PanelHideType
    {
        /// <summary>
        /// 隐藏不删除
        /// </summary>
        Hide,

        /// <summary>
        /// 删除
        /// </summary>
        Destroy,

        /// <summary>
        /// 延迟删除.10秒
        /// </summary>
        DelayDestroy,
    }

    public enum UILayer
    {
        UI,
        UITopMost,
    }

    public class UIManager : SingletonMonoBehaviour<UIManager>
    {
        /// <summary>
        /// 面板异步打开缓存的信息
        /// </summary>
        private class PanelOpenInfo
        {
            public PanelOpenType m_openType = PanelOpenType.Invalid;
            public bool m_isShowBlackBG = false;
        }

        public Transform rootNormal;
        public Camera rootNormalCamera;

        public Transform rootTopMost;
        public Camera rootTopMostCamera;

        public Transform root3D;
        public Camera root3DCamera;

        private VersionPanelDecorator m_versionPanel;
        private LoadingSceneDecorator m_loadingPanel;
        private DebugWindowDecorator m_debugWindowPanel;
        private BlackBackgroundDecorator m_blackBGPanel;
        private LoadingRotatePanel m_loadingRotatePanel;
        private BundleLoadingDecorator m_bundleLoadingPanel;

        private Dictionary<string, UIPanelDecorator> m_visibleDict = new Dictionary<string, UIPanelDecorator>();
        private Dictionary<string, UIPanelDecorator> m_hideDict = new Dictionary<string, UIPanelDecorator>();
        private Dictionary<string, LoadResourceTask> m_loadingDict = new Dictionary<string, LoadResourceTask>();

        /// <summary>
        /// 用于缓存DelayDestroy的定时器
        /// 所有加入m_delayDestroyDict的Panel，必然也在HideDict中
        /// </summary>
        private Dictionary<string, Timer> m_delayDestroyDict = new Dictionary<string, Timer>();

        private List<string> m_tempHidePanelQueue = new List<string>();

        private string m_lastOpenedPanel;
        public string lastOpenedPanel
        {
            get { return m_lastOpenedPanel; }
        }

        private string m_curOpenPanel;
        public string curOpenPanel
        {
            get { return m_curOpenPanel; }

            set
            {
                m_lastOpenedPanel = m_curOpenPanel;
                m_curOpenPanel = value;
            }
        }

        public Vector2 uiScreenSize
        {
            get
            {
                return m_uiScreenSize;
            }

            private set
            {
                m_uiScreenSize = value;
            }
        }

        private Vector2 m_uiScreenSize;

        protected override void PreInit()
        {
            UtilTools.SetDontDestroyOnLoad(gameObject);
            //DontDestroyOnLoad(gameObject);
            UICamera.uiRigidboy = GetComponent<Rigidbody>();
            //uiScreenSize = new Vector2(rootNormal.manualWidth, rootNormal.manualHeight);
            EventManager.Instance.AddEventListener(EventID.ConfigDataReady, OnConfigDataReady);

            EventManager.Instance.AddEventListener(EventID.SocketMessage, OnSocketMsgEvent);
        }

        private void OnConfigDataReady(params object[] param)
        {
            EventManager.Instance.RemoveEventListener(EventID.ConfigDataReady, OnConfigDataReady);
        }

        private void OnSocketMsgEvent(params object[] param)
        {
            ShowNetWaitingPanel((bool)param[0]);
        }

        /// <summary>
        /// 隐藏当前显示的panel_Lua
        /// </summary>
        public void HideCurrPanel()
        {
            //临时保存隐藏的面板名字
            m_tempHidePanelQueue.Clear();

            //hide other panels.
            foreach (var kvp in m_visibleDict)
            {
                m_tempHidePanelQueue.Add(kvp.Key);
            }
            //把隐藏的面板从显示面板列表中移除
            for (int i = 0; i < m_tempHidePanelQueue.Count; i++)
            {
                UIPanelDecorator panel = m_visibleDict[m_tempHidePanelQueue[i]];
                panel.SetActive(false);
            }
        }

        /// <summary>
        /// 显示上次隐藏的panel_Lua
        /// </summary>
        public void ShowLastHidePanel()
        {
            for (int i = m_tempHidePanelQueue.Count - 1; i >= 0; i--)
            {
                if (m_visibleDict.ContainsKey(m_tempHidePanelQueue[i]))
                {
                    UIPanelDecorator panel = m_visibleDict[m_tempHidePanelQueue[i]];
                    panel.SetActive(true);
                    if (panel.openType == PanelOpenType.StandAlone)
                    {
                        return;
                    }
                }
            }
        }

        /// </summary>
        /// <param name="需要显示的面板名称"></param>
        /// <param name="打开方式"></param>
        /// <returns></returns>
        //         public GameObject OpenPanelFromResouce(string prefabName,
        //             PanelOpenType openType = PanelOpenType.PopUp,
        //             bool isShowBlackBG = false)
        //         {
        //             UIPanelDecorator panelDecorator = null;
        //
        //             string[] names = prefabName.Split('/');
        //             string name = names[names.Length - 1];
        //
        //             //Is visible
        //             if (m_visibleDict.ContainsKey(name))
        //             {
        //                 panelDecorator = m_visibleDict[name];
        //                 SetPanelOpenType(panelDecorator, openType, isShowBlackBG);
        //                 return panelDecorator.gameObject;
        //             }
        //
        //             //Is hide
        //             if (m_hideDict.ContainsKey(name))
        //             {
        //                 panelDecorator = m_hideDict[name];
        //                 if (panelDecorator.gameObject != null)
        //                 {
        //                     m_hideDict.Remove(name);
        //                     m_visibleDict.Add(name, panelDecorator);
        //                 }
        //             }
        //
        //             //panelDecorator exists, reset depth.
        //             curOpenPanel = prefabName;
        //             if (panelDecorator == null)
        //             {
        //                 GameObject prefab = AssetsManagerFor5.Instance.LoadFromResource(prefabName);
        //                 GameObject go = InstantiateUIPrefab(prefab, openType);
        //                 panelDecorator = go.GetComponent<UIPanelDecorator>();
        //             }
        //
        //             SetPanelOpenType(panelDecorator, openType, isShowBlackBG);
        //
        //             if(openType == PanelOpenType.PopUpHide)
        //             {
        //                 panelDecorator.SetActive(false);
        //             }
        //
        //             return panelDecorator.gameObject;
        //         }

        /// <summary>
        /// 从bundle中加载UI prefab
        /// </summary>
        /// <param name="panelName"></param>
        /// <param name="openType"></param>
        public LoadResourceTask OpenPanelFromBundle(string panelName,
            PanelOpenType openType = PanelOpenType.PopUp,
            bool isShowBlackBG = false)
        {
            UIPanelDecorator panelDecorator = null;
            //string assetName = panelName + ".prefab";
            panelName += ".prefab";

            //Is visible
            if (m_visibleDict.ContainsKey(panelName))
            {
                panelDecorator = m_visibleDict[panelName];
            }

            //Is hide
            if (m_hideDict.ContainsKey(panelName))
            {
                panelDecorator = m_hideDict[panelName];
                if (panelDecorator.gameObject != null)
                {
                    m_hideDict.Remove(panelName);
                    m_visibleDict.Add(panelName, panelDecorator);
                }

                //正在delayDestroy中
                if (m_delayDestroyDict.ContainsKey(panelName))
                {
                    TimerManager.Instance.RemoveTimer(m_delayDestroyDict[panelName]);
                    m_delayDestroyDict.Remove(panelName);
                }
            }

            //panelDecorator exists, reset depth.
            if (panelDecorator != null)
            {
                curOpenPanel = panelName;

                SetPanelOpenType(panelDecorator, openType, isShowBlackBG);
                panelDecorator.SetActive(true);
                return null;
            }

            //正在加载
            if (!m_loadingDict.ContainsKey(panelName))
            {
                LoadResourceTask task = AssetsManagerFor5.Instance.AddAssetTask(panelName, OnUIBundleLoaded);
                if (task != null)
                {
                    PanelOpenInfo info = new PanelOpenInfo();
                    info.m_openType = openType;
                    info.m_isShowBlackBG = isShowBlackBG;
                    task.userData = info;
                    m_loadingDict.Add(panelName, task);
                    ShowBundleLoading(true, panelName);
                }
                else
                {
                    return null;
                }
            }
            return m_loadingDict[panelName];
        }

        /// <summary>
        /// UI Bundle. 加载完成了
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="userData"></param>
        private void OnUIBundleLoaded(UnityEngine.Object asset, object userData)
        {
            ShowBundleLoading(false);

            GameObject uiObj = asset as GameObject;
            UIPanelDecorator panelDecorator = uiObj.GetComponent<UIPanelDecorator>();

            if (panelDecorator == null)
            {
                LogManager.Instance.LogError("UI prefab" + asset.name + "must have UIPanelDecorator");
                return;
            }

            panelDecorator.prefabName = asset.name;

            if (!m_visibleDict.ContainsKey(panelDecorator.prefabName))
            {
                m_visibleDict.Add(panelDecorator.prefabName, panelDecorator);
            }

            if (m_loadingDict.ContainsKey(asset.name))
            {
                LoadResourceTask task = m_loadingDict[asset.name];
                m_loadingDict.Remove(asset.name);
                PanelOpenInfo info = (PanelOpenInfo)task.userData;
                SetPanelOpenType(panelDecorator, info.m_openType, info.m_isShowBlackBG);
            }
            else
            {

                PanelOpenInfo info = (PanelOpenInfo)userData;
                SetPanelOpenType(panelDecorator, info.m_openType, info.m_isShowBlackBG);
            }

            curOpenPanel = panelDecorator.prefabName;
            panelDecorator.SetActive(true);
        }

        public void HidePanel(string prefabName, PanelHideType type = PanelHideType.Hide)
        {
            HidePanel(GetPanelDecorator(prefabName + ".prefab", true), type);
        }

        public void HidePanel(UIPanelDecorator panelDecorator, PanelHideType type = PanelHideType.Hide)
        {
            if (panelDecorator == null)
            {
                return;
            }

            //在可见列表中
            if (m_visibleDict.ContainsKey(panelDecorator.prefabName))
            {
                m_visibleDict.Remove(panelDecorator.prefabName);
                panelDecorator.SetActive(false);

                //类型为Hide并可以放入HideDict中就放入
                if (type == PanelHideType.Hide)
                {
                    m_hideDict.Add(panelDecorator.prefabName, panelDecorator);
                }
                else if (type == PanelHideType.Destroy)
                {
                    UtilTools.DestroyGameObject(panelDecorator.gameObject);
                }
                else if (type == PanelHideType.DelayDestroy)
                {
                    Timer delayTimer = TimerManager.Instance.AddTimer(10f,
                        delegate ()
                        {
                            m_delayDestroyDict.Remove(panelDecorator.prefabName);
                            m_hideDict.Remove(panelDecorator.prefabName);
                            UtilTools.DestroyGameObject(panelDecorator.gameObject);
                        });

                    m_hideDict.Add(panelDecorator.prefabName, panelDecorator);
                    m_delayDestroyDict.Add(panelDecorator.prefabName, delayTimer);
                }
            }
            else
            {
                LogManager.Instance.LogError("Hiding invisible panel: " + panelDecorator.prefabName);
                return;
            }

            //是独占式打开的面板，关闭时还原其他面板
            if (panelDecorator.openType == PanelOpenType.StandAlone)
            {
                for (int i = m_tempHidePanelQueue.Count - 1; i >= 0; i--)
                {
                    UIPanelDecorator panel = null;
                    m_visibleDict.TryGetValue(m_tempHidePanelQueue[i], out panel);
                    if (panel != null)
                    {
                        panel.SetActive(true);
                        curOpenPanel = panel.prefabName;
                        if (panel.openType == PanelOpenType.StandAlone)
                        {
                            return;
                        }
                    }
                }
            }
            else
            {
                curOpenPanel = lastOpenedPanel;
            }

            ShowBlackBackground(false);
        }

        public void DestroyPanel(string prefabName)
        {
            string[] names = prefabName.Split('/');
            prefabName = names[names.Length - 1];
            UIPanelDecorator panel = null;
            m_visibleDict.TryGetValue(prefabName, out panel);
            if (panel != null)
            {
                m_visibleDict.Remove(prefabName);
                panel.SetActive(false);
                UtilTools.DestroyGameObject(panel.gameObject);
                return;
            }

            m_hideDict.TryGetValue(prefabName, out panel);
            if (panel != null)
            {
                m_hideDict.Remove(prefabName);
                GameObject.DestroyImmediate(panel.gameObject);
            }
        }

        /// <summary>
        /// 克隆显示面板
        /// </summary>
        /// <param name="当前面板资源"></param>
        /// <param name="显示类型"></param>
        /// <param name="重哪个面板跳转过来"></param>
        /// <returns></returns>
        //         private GameObject InstantiateUIPrefab(UnityEngine.Object asset, object userData)
        //         {
        //             GameObject uiObj = GameObject.Instantiate(asset) as GameObject;
        //
        //             UIPanelDecorator panelDecorator = uiObj.GetComponent<UIPanelDecorator>();
        //
        //             if (panelDecorator == null)
        //             {
        //                 LogManager.Instance.LogError("UI prefab must have UIPanelDecorator");
        //                 return uiObj;
        //             }
        //
        //             panelDecorator.prefabName = asset.name;
        //
        //             if (!m_visibleDict.ContainsKey(panelDecorator.prefabName))
        //                 m_visibleDict.Add(panelDecorator.prefabName, panelDecorator);
        //
        //             if (m_loadingDict.ContainsKey(asset.name))
        //             {
        //                 LoadResourceTask task = m_loadingDict[asset.name];
        //                 m_loadingDict.Remove(asset.name);
        //                 SetPanelOpenType(panelDecorator, (PanelOpenType)task.userData);
        //             }
        //             else
        //             {
        //                 PanelOpenType openType = (PanelOpenType)userData;
        //                 SetPanelOpenType(panelDecorator, openType);
        //             }
        //             return uiObj;
        //         }
        //加载各种初始化资源

        private void SetPanelOpenType(UIPanelDecorator panelDecorator, PanelOpenType type, bool isShowBlackBG)
        {
            panelDecorator.openType = type;
            if (panelDecorator != null)
            {
                switch (type)
                {
                    case PanelOpenType.StandAlone:
                        {
                            //临时保存隐藏的面板名字
                            m_tempHidePanelQueue.Clear();

                            //hide other panels.
                            foreach (var kvp in m_visibleDict)
                            {
                                if (kvp.Key != panelDecorator.prefabName)
                                {
                                    m_tempHidePanelQueue.Add(kvp.Key);
                                }
                            }

                            //把隐藏的面板从显示面板列表中移除
                            for (int i = 0; i < m_tempHidePanelQueue.Count; i++)
                            {
                                UIPanelDecorator panel = m_visibleDict[m_tempHidePanelQueue[i]];
                                panel.SetActive(false);
                            }

                            SetPanelLayer(panelDecorator, UILayer.UI);
                            panelDecorator.depth = m_visibleDict.Count;
                            panelDecorator.transform.SetParent(rootNormal.transform);
                            panelDecorator.transform.localPosition = Vector3.zero;
                            panelDecorator.transform.localScale = Vector3.one;
                        }
                        break;

                    case PanelOpenType.PopUp:
                    case PanelOpenType.PopUpHide:
                        {
                            SetPanelLayer(panelDecorator, UILayer.UI);

                            int maxDepth = 0;
                            foreach (var kvp in m_visibleDict)
                            {
                                maxDepth = Mathf.Max(maxDepth, kvp.Value.maxChildDepth);
                            }

                            //显示黑底
                            if (isShowBlackBG)
                            {
                                panelDecorator.depth = maxDepth + 1;
                                ShowBlackBackground(true, panelDecorator);
                            }
                            else
                            {
                                panelDecorator.depth = maxDepth + 1;
                            }

                            panelDecorator.transform.SetParent(rootNormal.transform);
                            panelDecorator.transform.localPosition = Vector3.zero;
                            panelDecorator.transform.localScale = Vector3.one;
                        }
                        break;

                    case PanelOpenType.TopMost:
                        {
                            SetPanelLayer(panelDecorator, UILayer.UITopMost);
                            panelDecorator.transform.SetParent(rootTopMost.transform);
                            panelDecorator.transform.localPosition = Vector3.zero;
                            panelDecorator.transform.localScale = Vector3.one;
                        }
                        break;
                }

                return;
            }
        }

        private void SetPanelLayer(UIPanelDecorator panelDecorator, UILayer layer)
        {
            int curLayerIdx = panelDecorator.gameObject.layer;
            int targetLayerIdx = LayerMask.NameToLayer(layer.ToString());
            if (targetLayerIdx != curLayerIdx)
            {
                UtilTools.SetGameObjctLayer(layer.ToString(), panelDecorator.gameObject, true);
            }
        }

        public UIPanelDecorator GetPanelDecorator(string panelPrefabName, bool isVisible)
        {
            Dictionary<string, UIPanelDecorator> sourceDict =
                (isVisible) ? m_visibleDict : m_hideDict;

            UIPanelDecorator ret = null;
            sourceDict.TryGetValue(GetPanelName(panelPrefabName), out ret);
            return ret;
        }

        public T GetPanelDecorator<T>(bool isVisible) where T : UIPanelDecorator
        {
            Dictionary<string, UIPanelDecorator> sourceDict =
                (isVisible) ? m_visibleDict : m_hideDict;

            foreach (var kvp in sourceDict)
            {
                if (kvp.Value is T)
                {
                    return kvp.Value as T;
                }
            }

            return null;
        }
        /// <summary>
        /// 清除定时timer
        /// </summary>
        public void CloseDelayDestroyDict()
        {
            foreach (Timer t in m_delayDestroyDict.Values)
            {
                TimerManager.Instance.RemoveTimer(t);
            }
            m_delayDestroyDict.Clear();
        }

        public void DestroyAllPanels()
        {
            List<string> tempHideKeys = new List<string>(m_hideDict.Keys);
            for (int i = 0; i < tempHideKeys.Count; i++)
            {
                Destroy(m_hideDict[tempHideKeys[i]].gameObject);
                m_hideDict.Remove(tempHideKeys[i]);
            }

            List<string> tempVisibleKeys = new List<string>(m_visibleDict.Keys);
            for (int i = 0; i < tempVisibleKeys.Count; i++)
            {
                Destroy(m_visibleDict[tempVisibleKeys[i]].gameObject);
                m_visibleDict.Remove(tempVisibleKeys[i]);
            }
        }

        public Vector3 WorldPosToUI(Vector3 worldPos, UICameraType cameraType = UICameraType.Normal)
        {
            Vector3 scrnPos = CurrMainCamera.mainCamera.WorldToScreenPoint(worldPos);
            Vector3 uiPos = rootNormalCamera.ScreenToWorldPoint(scrnPos);
            uiPos.z = 0f;
            return uiPos;
        }

        public bool IsPickUpUIObject(Vector3 mousePos)
        {
            UICamera.Raycast(mousePos);
            if (UICamera.lastHit.collider == null)
            {
                return false;
            }
            else
            {
                if (UICamera.lastHit.collider.gameObject.layer == LayerMask.NameToLayer("UI"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private string GetPanelName(string rawPanelName)
        {
            string[] nameArr = rawPanelName.Split('/');
            if (nameArr.Length > 0)
            {
                return nameArr[nameArr.Length - 1];
            }
            return rawPanelName;
        }

        //加载各种初始化资源
        public void OnAssetsManagerReady()
        {
            AssetsManagerFor5.Instance.AddAssetTask("VersionPanel.prefab",
           delegate (UnityEngine.Object asset, object userData)
           {
               GameObject go = asset as GameObject;
               go.transform.SetParentAndScale(rootTopMost.transform, false);
               m_versionPanel = go.GetComponent<VersionPanelDecorator>();
               m_versionPanel.SetActive(true);
           });

            AssetsManagerFor5.Instance.AddAssetTask("LoadingPanel.prefab",
                delegate (UnityEngine.Object asset, object userData)
                {
                    GameObject go = asset as GameObject;
                    go.transform.SetParentAndScale(rootNormal.transform, false);
                    m_loadingPanel = go.GetComponent<LoadingSceneDecorator>();
                    m_loadingPanel.SetActive(false);
                });

            AssetsManagerFor5.Instance.AddAssetTask("DebugWindowPanel.prefab",
                delegate (UnityEngine.Object asset, object userData)
                {
                    GameObject go = asset as GameObject;
                    go.transform.SetParentAndScale(rootTopMost.transform, false);
                    m_debugWindowPanel = go.GetComponent<DebugWindowDecorator>();
                    m_debugWindowPanel.SetActive(false);
                    LogManager.Instance.showUILogCallBack = m_debugWindowPanel.LogCallBack;
                });

            AssetsManagerFor5.Instance.AddAssetTask("NoticeBox.prefab",
                delegate (UnityEngine.Object asset, object userData)
                {
                    GameObject go = asset as GameObject;
                    go.transform.SetParentAndScale(rootTopMost.transform, true);
                    NoticeBoxManger.Instance.noticePanel = go.GetComponent<NoticePanel>();
                });

            AssetsManagerFor5.Instance.AddAssetTask("AwaitingPanel.prefab",
                delegate (UnityEngine.Object asset, object userData)
                {
                    GameObject go = asset as GameObject;
                    go.transform.SetParentAndScale(rootTopMost.transform, false);
                    NoticeBoxManger.Instance.awaitingPanel = go.GetComponent<AwaitingPanel>();
                });

            AssetsManagerFor5.Instance.AddAssetTask("LoadingRotatePanel.prefab",
                delegate (UnityEngine.Object asset, object userData)
                {
                    GameObject go = asset as GameObject;
                    go.transform.SetParentAndScale(rootTopMost.transform, false);
                    m_loadingRotatePanel = go.GetComponent<LoadingRotatePanel>();
                });

            AssetsManagerFor5.Instance.AddAssetTask("BlackBackground.prefab",
                delegate (UnityEngine.Object asset, object userData)
                {
                    GameObject go = asset as GameObject;
                    go.transform.SetParentAndScale(rootNormal.transform, false);
                    m_blackBGPanel = go.GetComponent<BlackBackgroundDecorator>();
                });

            AssetsManagerFor5.Instance.AddAssetTask("BundleLodingPanel.prefab",
                delegate (UnityEngine.Object asset, object userData)
                {
                    GameObject go = asset as GameObject;
                    go.transform.SetParentAndScale(rootTopMost.transform, false);
                    m_bundleLoadingPanel = go.GetComponent<BundleLoadingDecorator>();
                });
        }

        public void ShowDebugWindow()
        {
            m_debugWindowPanel.SetActive(true);
        }

        //进入loading场景，销毁所有缓存的面板
        public void OnEnterLoading()
        {
            List<string> allPanels = new List<string>();
            foreach (var kvp in m_visibleDict)
            {
                allPanels.Add(kvp.Value.prefabName);
            }

            foreach (var kvp in m_hideDict)
            {
                allPanels.Add(kvp.Value.prefabName);
            }

            for (int i = 0; i < allPanels.Count; i++)
            {
                DestroyPanel(allPanels[i]);
            }

            m_visibleDict.Clear();
            m_hideDict.Clear();

            AssetsManagerFor5.Instance.ClearUnusedBundles();
        }

        public void ShowBlackBackground(bool isShow, UIPanelDecorator panelDecorator = null)
        {
            if (m_blackBGPanel == null)
            {
                return;
            }

            if (isShow && panelDecorator != null)
            {
                m_blackBGPanel.SetVisible(true, panelDecorator);
                m_blackBGPanel.depth = panelDecorator.depth;
                panelDecorator.depth++;
            }
            else
            {
                m_blackBGPanel.SetVisible(false, panelDecorator);
            }
        }

        /// <summary>
        /// widget依附到了UIGrid下，需要调整UIDragScrollView的UIScrollView
        /// </summary>
        public static void AdapteNewUIGrid(GameObject widget, UIGrid grid)
        {
            UIDragScrollView dragComponent = widget.GetComponent<UIDragScrollView>();
            if (dragComponent != null)
            {
                dragComponent.scrollView = grid.GetScrollView();
            }
        }

        /// <summary>
        /// 场景加载UI进度条
        /// </summary>
        /// <param name="isShow"></param>
        /// <returns></returns>
        public LoadingSceneDecorator ShowLoadingScenePanel(bool isShow)
        {
            m_loadingPanel.SetActive(isShow);
            return m_loadingPanel;
        }

        /// <summary>
        /// 现在从bundle加载的UI的过程
        /// </summary>
        /// <param name="isShow"></param>
        /// <param name="content"></param>
        private void ShowBundleLoading(bool isShow, string content = null)
        {
            if (m_bundleLoadingPanel == null)
            {
                return;
            }

            m_bundleLoadingPanel.SetActive(isShow);
            if (isShow && !string.IsNullOrEmpty(content))
            {
                m_bundleLoadingPanel.m_label.text = content;
            }
        }

        /// <summary>
        /// 网络等待panel.req已发出，rsp还没有收到
        /// </summary>
        /// <param name="isShow"></param>
        public void ShowNetWaitingPanel(bool isShow)
        {
            if (m_loadingRotatePanel != null)
            {
                m_loadingRotatePanel.SetActive(isShow);
            }
        }

        public BlackBackgroundDecorator GetBlackBackGround()
        {
            return m_blackBGPanel;
        }
    }//UIManager
}