using UnityEngine;
using System.Collections;

namespace Utility
{
    public class UILabel
    {
        public string text { set; get; }
    }

    [DisallowMultipleComponent]
    public class UILabelAdapter : MonoBehaviour
    {
        public int m_textId;
        void Awake()
        {
            if (!TextManager.Instance.enableAdapter)
            {
                return;
            }

            UILabel label = GetComponent<UILabel>();
            label.text = TextManager.Instance.GetContent(m_textId);
        }

    }
}