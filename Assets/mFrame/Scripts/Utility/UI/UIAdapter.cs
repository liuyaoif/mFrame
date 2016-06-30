using Utility;
using UnityEngine;

public class UIAdapter : Entity
{
    public UIWidget m_targetWidget;
    private bool m_isDirty = false;

    public bool IsDirty
    {
        get { return m_isDirty; }
        set { m_isDirty = value; }
    }

    void OnEnable()
    {
        IsDirty = true;
    }

    void Awake()
    {
        IsDirty = true;
    }

    //void Update()
    protected override void RenderUpdate()
    {
        if (!IsDirty || m_targetWidget == null)
        {
            return;
        }

        if (m_targetWidget == null)
        {
            AdaptRenderQueue(30000);//UI top.
        }
        else if (m_targetWidget.drawCall != null)
        {
            AdaptRenderQueue(m_targetWidget.drawCall.finalRenderQueue);
        }
    }

    private void AdaptRenderQueue(int queue)
    {
        if (GetComponent<Renderer>() != null && GetComponent<Renderer>().sharedMaterial != null)
        {
            GetComponent<Renderer>().sharedMaterial.renderQueue = queue;
        }

        Renderer[] rendererList = gameObject.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < rendererList.Length; i++)
        {
            Renderer curRenderer = rendererList[i];
            curRenderer.sharedMaterial.renderQueue = queue;
        }

        float zOffset = m_targetWidget.transform.position.z - 1;
        Vector3 tempPos = transform.position;
        tempPos.z = zOffset;
        transform.position = tempPos;

        m_isDirty = false;
    }
}//RendererAdapter
