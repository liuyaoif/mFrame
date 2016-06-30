using System;
using UnityEngine;

public class LongPressLogic : MonoBehaviour
{
    /// <summary>
    /// 进入长按委托
    /// </summary>
    public Action onLongerPressEnter;
    private bool isFristEnter = true; //是否第一时间进入长按
    /// <summary>
    /// 退出长按委托
    /// </summary>
    public Action onLongerPressExit;
    /// <summary>
    /// 长按中的Update委托
    /// </summary>
    public Action onLongerPressUpdate;
    /// <summary>
    /// 是否开启长按中的Update事件
    /// </summary>
    public bool isPressUpdate = false;
    /// <summary>
    /// 当前长按了多久时间
    /// </summary>
    private float currentPreesTime = 0;

    private bool isPress = false;

    void OnPress(bool isPressed)
    {

        if (isPressed)
        {
            isPress = true;
            currentPreesTime = 0;
        }
        else
        {
            isPress = false;
            CancelLongPress();
        }

    }

    void OnDrag(Vector2 delta)
    {
        CancelLongPress();
    }

    void OnDisable()
    {
        isPress = false;
        currentPreesTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (isPress)
        {
            currentPreesTime += Time.deltaTime;
            if (currentPreesTime >= 0.3F)
            {
                if (isPressUpdate)
                {
                    if (onLongerPressUpdate != null)
                        onLongerPressUpdate();
                }
                else
                {
                    if (onLongerPressEnter != null && isFristEnter)
                    { onLongerPressEnter(); isFristEnter = false; }
                }


            }
        }

    }



    private void CancelLongPress()
    {
        isPress = false;
        currentPreesTime = 0;

        isFristEnter = true;
        if (onLongerPressExit != null)
            onLongerPressExit();
    }



}
