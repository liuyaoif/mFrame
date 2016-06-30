using UnityEngine;
using System.Collections;
using Utility;
public class GestureManager : SingletonMonoBehaviour<GestureManager>
{
    public delegate void Gesture1(Vector2 toch1);
    public delegate void Gesture2(Vector2 toch1, Vector2 toch2);
    public delegate void Gesture2Slide(bool orientation, Touch toch1, Touch toch2);
    public Gesture1 gesture1OnStart;
    public Gesture1 gesture1OnEnd;
    public Gesture1 gesture1OnSlide;

    public Gesture2Slide gesture2OnSlide;
    public Gesture2 gesture2OnStart;
    public Gesture2 gesture2OnEnd;
    private Vector2 pos1;
    private Vector2 pos2;

    private bool m_isPickedUpUI = false;
    private float defaultDpi = 120;
    private float m_blDpi;
    public GestureManager()
    {

        m_blDpi = defaultDpi / Screen.dpi;
    }
    protected override void RenderUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            m_isPickedUpUI = UIManager.Instance.IsPickUpUIObject(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(0))
        {
            m_isPickedUpUI = false;
        }

        if (m_isPickedUpUI)
        {
            return;
        }

        if (Application.platform != RuntimePlatform.WindowsEditor)
        {
            MobileTouch();
        }
        else
        {
            PC();
        }
    }

    public void PC()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (gesture1OnStart != null)
                gesture1OnStart(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (gesture1OnEnd != null)
                gesture1OnEnd(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0))
        {
            float xhd = Input.GetAxis("Mouse X");
            float yhd = Input.GetAxis("Mouse Y");
            if (xhd != 0 || yhd != 0)
            {
                // "滑动"
                if (gesture1OnSlide != null)
                    gesture1OnSlide(new Vector2(xhd, yhd) * m_blDpi);
            }
        }
    }
    public void MobileTouch()
    {
        if (Input.touchCount == 2)
        {
            Touch toc1 = Input.GetTouch(0);
            Touch toc2 = Input.GetTouch(1);

            if (toc1.phase == TouchPhase.Began || toc2.phase == TouchPhase.Began)
            {
                if (gesture2OnStart != null)
                    gesture2OnStart(toc1.position, toc2.position);
            }
            if (toc1.phase == TouchPhase.Ended || toc2.phase == TouchPhase.Ended)
            {
                if (gesture2OnEnd != null)
                    gesture2OnEnd(toc1.position, toc2.position);
            }

            if (pos1 == Vector2.zero)
            {
                pos1 = toc1.position;
                pos2 = toc2.position;
            }
            if (toc1.phase == TouchPhase.Moved && toc2.phase == TouchPhase.Moved)
            {
                //"进来";

                // mess += toc1.deltaPosition + " " + toc1.deltaTime + "   " + toc2.deltaTime;
                if (Vector2.Dot(toc1.deltaPosition, toc2.deltaPosition) < -0.2f)
                {
                    // mess2 = "滑动";
                    float posD1 = Vector2.Distance(pos1, pos2);
                    float posD2 = Vector2.Distance(toc1.position, toc2.position);
                    if (posD1 < posD2)
                    {
                        if (gesture2OnSlide != null)
                            gesture2OnSlide(true, toc1, toc2);
                    }
                    else
                    {
                        //"向外";
                        if (gesture2OnSlide != null)
                            gesture2OnSlide(false, toc1, toc2);
                    }
                }
                else
                {
                    pos1 = toc1.position;
                    pos2 = toc2.position;
                }
            }
        }
        else if (Input.touchCount == 1)
        {
            Touch toc1 = Input.GetTouch(0);

            if (toc1.phase == TouchPhase.Moved)
            {

                // "滑动"
                if (gesture1OnSlide != null)
                    gesture1OnSlide(toc1.deltaPosition * m_blDpi);
            }
            else if (toc1.phase == TouchPhase.Began)
            {
                if (gesture1OnStart != null)
                    gesture1OnStart(toc1.position);
            }
            else if (toc1.phase == TouchPhase.Ended)
            {
                if (gesture1OnEnd != null)
                    gesture1OnEnd(toc1.position);
            }
        }
        else
        {
            pos1 = Vector2.zero;
            pos2 = Vector2.zero;
        }
    }
}
