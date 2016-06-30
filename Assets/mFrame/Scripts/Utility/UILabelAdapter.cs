using UnityEngine;
using System.Collections;

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
