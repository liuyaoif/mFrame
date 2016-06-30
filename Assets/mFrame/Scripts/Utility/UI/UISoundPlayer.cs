using UnityEngine;

[DisallowMultipleComponent]
public class UISoundPlayer : MonoBehaviour
{
    public string soundAssetName;

    void Awake()
    {
        UIButton button = GetComponent<UIButton>();
        if (button != null)
        {
            EventDelegate.Remove(button.onClick, OnClickButton);
            EventDelegate.Add(button.onClick, OnClickButton);
            return;
        }

        UIEventListener eventListener = GetComponent<UIEventListener>();
        if (eventListener != null)
        {
            eventListener.onClick -= OnClickListener;
            eventListener.onClick += OnClickListener;
            return;
        }


        UIEventTrigger trigger = GetComponent<UIEventTrigger>();
        if (trigger != null)
        {
            EventDelegate.Remove(trigger.onClick, OnClickTrigger);
            EventDelegate.Add(trigger.onClick, OnClickTrigger);
            return;
        }
    }

    public void OnClickListener(GameObject go)
    {
        PlaySound();
    }

    public void OnClickButton()
    {
        PlaySound();
    }

    public void OnClickTrigger()
    {
        PlaySound();
    }

    private void PlaySound()
    {
        if (string.IsNullOrEmpty(soundAssetName))
        {
            SoundManager.Instance.PlaySoundEffect("ClickButton");
        }
        else
        {
            SoundManager.Instance.PlaySoundEffect(soundAssetName);
        }
    }
}
