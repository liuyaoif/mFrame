using UnityEngine;
using System.Collections.Generic;
using System;


//In Unity5, lightmap is not save to each renderer.
//When instantiate from a prefab, renderers won't have lightmaps.
//This script save lightmap info to prefab, and apply to renderer when awake.
//By desmondliu.

public class SaveLightmapToRenderer : MonoBehaviour
{
    [Serializable]
    public struct LightmapInfo
    {
        public Renderer target;
        public Terrain terr;
        public bool isTerr;
        public int lightmapIndex;
        public Vector4 offsetScale;
    }

    public List<LightmapInfo> lmList = new List<LightmapInfo>();

    public int idx = 0;

    void Awake()
    {
        ApplyLightMapToRenderers();
    }

    public void SaveLmInfo()
    {
        lmList.Clear();
        Terrain ter = null;
        if ((ter = gameObject.GetComponent<Terrain>()) != null)
        {
            SaveLightmapToRenderer.LightmapInfo info = new SaveLightmapToRenderer.LightmapInfo();
            //info.target = renders[i];
            info.lightmapIndex = ter.lightmapIndex;
            info.isTerr = true;
            info.terr = ter;
            //info.offsetScale = renders[i].lightmapScaleOffset;
            lmList.Add(info);
            idx = ter.lightmapIndex;
        }

        Renderer[] renders = gameObject.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renders.Length; i++)
        {
            SaveLightmapToRenderer.LightmapInfo info = new SaveLightmapToRenderer.LightmapInfo();
            info.target = renders[i];
            info.lightmapIndex = renders[i].lightmapIndex;
            info.offsetScale = renders[i].lightmapScaleOffset;
            lmList.Add(info);
            idx = renders[i].lightmapIndex;
        }
    }
    private void ApplyLightMapToRenderers()
    {
        for (int i = 0; i < lmList.Count; i++)
        {
            if (lmList[i].isTerr)
            {
                lmList[i].terr.lightmapIndex = lmList[i].lightmapIndex;
            }
            else
            {

                lmList[i].target.lightmapIndex = lmList[i].lightmapIndex;
                lmList[i].target.lightmapScaleOffset = lmList[i].offsetScale;
            }
        }
    }
}


