using mFrame.Sound;
using UnityEngine;

namespace mFrame.UI
{
    public class UIButton
    {
        public delegate void OnClickEvent();
        public event OnClickEvent onClick;
    }

    public class UIEventListener : MonoBehaviour
    {
        public delegate void OnClickEvent();
        public event OnClickEvent onClick;
    }

    [DisallowMultipleComponent]
    public class UISoundPlayer : MonoBehaviour
    {
        public string soundAssetName;

        void Awake()
        {
            UIButton button = GetComponent<UIButton>();
            if (button != null)
            {
                button.onClick += OnClickButton;
                return;
            }

            UIEventListener eventListener = GetComponent<UIEventListener>();
            if (eventListener != null)
            {
                eventListener.onClick -= OnClickListener;
                eventListener.onClick += OnClickListener;
                return;
            }


            //UIEventTrigger trigger = GetComponent<UIEventTrigger>();
            //if (trigger != null)
            //{
            //    EventDelegate.Remove(trigger.onClick, OnClickTrigger);
            //    EventDelegate.Add(trigger.onClick, OnClickTrigger);
            //    return;
            //}
        }

        public void OnClickListener()
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
}
