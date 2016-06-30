using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
[InitializeOnLoad]
public class NGUIDepthSeting
{
    private class PanelPosInfo
    {
        public Rect rect;
        public UIPanel panel;
    }
    private static List<UIWidget> wigdets = new List<UIWidget>();
    private static List<UIPanel> panels = new List<UIPanel>();
    private static List<PanelPosInfo> panelPos = new List<PanelPosInfo>();
    static NGUIDepthSeting()
    {
        EditorApplication.playmodeStateChanged += HierarchyWindowChanged;
        //Debug.Log("asdas");
        EditorApplication.hierarchyWindowChanged += HierarchyWindowChanged;
        EditorApplication.hierarchyWindowItemOnGUI += HierarchyGUI;
        FindWidget();
    }

    private static void FindWidget()
    {
        wigdets = MapEditorTool.GetAllSceneObj<UIWidget>();
        panels = MapEditorTool.GetAllSceneObj<UIPanel>();
    }
    private static void HierarchyWindowChanged()
    {
        //Debug.Log("gaibian");
        FindWidget();
    }
    private static UIWidget GetWidget(int id)
    {
        for (int i = 0; i < wigdets.Count; i++)
        {
            if (!wigdets[i])
                continue;
            if (!wigdets[i].gameObject)
                continue;
            if (wigdets[i].gameObject.GetInstanceID() == id)
                return wigdets[i];
        }
        return null;
    }
    private static UIPanel GetPanel(int id)
    {
        for (int i = 0; i < panels.Count; i++)
        {
            if (!panels[i])
                continue;
            if (!panels[i].gameObject)
                continue;
            if (panels[i].gameObject.GetInstanceID() == id)
                return panels[i];
        }
        return null;
    }
    private static void AddPanelInfo(int id, UIPanel panel, Rect selectionRect)
    {
        PanelPosInfo info = GetPanelPosInfo(id);
        if (info != null)
        {
            //info = panelPos[id];
            info.panel = panel;
            info.rect = selectionRect;
        }
        else
        {
            info = new PanelPosInfo();
            info.panel = panel;
            info.rect = selectionRect;
            panelPos.Add(info);
        }
    }
    private static PanelPosInfo GetPanelPosInfo(int id)
    {
        for (int i = 0; i < panelPos.Count; i++)
        {
            if (!panelPos[i].panel)
                continue;
            if (panelPos[i].panel.GetInstanceID() == id)
                return panelPos[i];
        }
        return null;
    }
    private static void HierarchyGUI(int instanceID, Rect selectionRect)
    {

        DrawWidget(instanceID, selectionRect);
        DrawPanel(instanceID, selectionRect);


    }
    private static void DrawPanel(int instanceID, Rect selectionRect)
    {
        UIPanel panel = GetPanel(instanceID);
        if (panel == null)
        {
            return;
        }
        GUI.color = Color.magenta;
        Rect newRect = new Rect(selectionRect.x + selectionRect.width - 100, selectionRect.y, 25, selectionRect.height - 2);
        if (GUI.Button(newRect, "P" + panel.depth))
        {
        }
        GUI.color = Color.white;
        //GUI.Box(newRect, );
        //if (GUI.Button(newRect, widget.depth.ToString()))
        //AddPanelInfo(instanceID, panel, selectionRect);
    }
    private static void DrawWidget(int instanceID, Rect selectionRect)
    {
        //GUILayout.Button("asda");
        UIWidget widget = GetWidget(instanceID);
        if (widget == null)
        {
            return;
        }
        //if (widget.panel != null)
        //{
        //    PanelPosInfo panel = GetPanelPosInfo(widget.panel.GetInstanceID());
        //    if (panel != null)
        //    {
        //        Rect lien1 = new Rect(selectionRect.x + selectionRect.width - 33, selectionRect.y + selectionRect.height / 2, 3, 3);
        //        GUI.Box(lien1, "");
        //        Rect lien2 = new Rect(selectionRect.x + selectionRect.width - 33, panel.rect.y + panel.rect.height / 2, 3, selectionRect.y - panel.rect.y);
        //        GUI.Box(lien2, "");
        //        Rect lien3 = new Rect(panel.rect.x, panel.rect.y + panel.rect.height / 2, selectionRect.x + selectionRect.width - 33 - panel.rect.x, 3);
        //        GUI.Box(lien3, "");
        //    }
        //    else
        //    {
        //        Debug.Log("kong" + widget.name);
        //    }
        //}
        GUI.color = Color.grey;
        if (widget as UILabel)
            GUI.color = Color.white;
        if (widget as UISprite)
            GUI.color = Color.green;
        if (widget as UITexture)
            GUI.color = Color.yellow;
        Rect newRect = new Rect(selectionRect.x + selectionRect.width - 100, selectionRect.y, 25, selectionRect.height - 2);
        if (GUI.Button(newRect, widget.depth.ToString()))
        {
            if (Event.current.button == 0)
            {
                widget.depth++;
            }
            if (Event.current.button == 1)
            {
                widget.depth--;
            }

        }
        GUI.color = Color.white;

    }
}
