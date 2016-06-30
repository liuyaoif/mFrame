using System.Collections.Generic;
using UnityEngine;

namespace Utility
{
    public class UIPanel:MonoBehaviour
    {
        public int depth
        {
            set;get;
        }
    }

    //[RequireComponent(typeof(UIPanel))]
    public class UIPanelDecorator : Entity
    {
        /// <summary>
        /// 名字
        /// </summary>
        protected string m_prefabName;
        public string prefabName
        {
            get { return m_prefabName; }
            set { m_prefabName = value; }
        }
        public PanelOpenType openType
        {
            get { return m_openType; }
            set { m_openType = value; }
        }

        protected UIPanel m_panel;
        protected List<UIPanel> m_childrenPanels;
        protected int m_maxChildDepth;
        private PanelOpenType m_openType;
        public int maxChildDepth
        {
            get { return m_maxChildDepth; }
        }

        protected bool m_enableSoundEffect = true;

        public UIPanel panel
        {
            get
            {
                if (m_panel == null)
                {
                    m_panel = GetComponent<UIPanel>();
                    UIPanel[] childrenPanels = m_panel.GetComponentsInChildren<UIPanel>(true);
                    if (m_childrenPanels == null && childrenPanels != null)
                    {
                        m_childrenPanels = new List<UIPanel>();
                        for (int i = 0; i < childrenPanels.Length; i++)
                        {
                            m_childrenPanels.Add(childrenPanels[i]);
                        }
                    }
                }

                const string assertMsg = "Panel should not be null";
                LogManager.Instance.Assert(m_panel != null, assertMsg);
                return m_panel;
            }
        }

        public int depth
        {
            set
            {
                if (panel != null)
                {
                    m_maxChildDepth = value;
                    if (m_childrenPanels != null)
                    {
                        int lastDepth = panel.depth;
                        for (int i = 0; i < m_childrenPanels.Count; i++)
                        {
                            m_childrenPanels[i].depth = m_childrenPanels[i].depth - lastDepth + value;
                            m_maxChildDepth = Mathf.Max(m_maxChildDepth, m_childrenPanels[i].depth);
                        }
                    }
                    panel.depth = value;
                }
            }

            get
            {
                if (panel != null)
                {
                    return panel.depth;
                }
                return -1;
            }
        }//depth

        public void SetActive(bool isActive)
        {
            if (!gameObject)
            {
                LogManager.Instance.Log(gameObject.name);
                return;
            }

            gameObject.SetActive(isActive);
            if (isActive)
            {
                OnOpen();
                //                 if(m_enableSoundEffect)
                //                 {
                //                     SoundManager.Instance.PlaySoundEffect("PanelOpen", false, true);
                //                 }
            }
            else
            {
                OnHide(PanelHideType.Hide);
                //                 if (m_enableSoundEffect)
                //                 {
                //                     SoundManager.Instance.PlaySoundEffect("PanelHide", false, true);
                //                 }
            }
        }

        protected virtual void OnOpen()
        {
        }

        protected virtual void OnHide(PanelHideType type = PanelHideType.Hide)
        {
            //             switch (type)
            //             {
            //                 case PanelHideType.Hide:
            //                     {
            //                         //Do nothing.
            //                     }
            //                     break;
            //
            //                 case PanelHideType.Destroy:
            //                     {
            //                         Destroy(gameObject);
            //                     }
            //                     break;
            //
            //                 case PanelHideType.DelayDestroy:
            //                     {
            //                         DelayCall(1f, delegate ()
            //                         {
            //                             Destroy(gameObject);
            //                         });
            //                     }
            //                     break;
            //}
        }

        /// <summary>
        /// 如果显示了黑底，点击黑底的回调
        /// </summary>
        public virtual void OnClickBlackBG()
        {
            UIManager.Instance.HidePanel(this);
        }

        public bool IsVisible()
        {
            return gameObject.activeSelf;
        }
    }//UIPanelDecorator
}
