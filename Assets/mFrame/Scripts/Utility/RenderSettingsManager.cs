﻿using UnityEngine;
using System.Collections;

using Utility;

public class RenderSettingsManager : SingletonMonoBehaviour<RenderSettingsManager>
{
    //     public bool fog;
    //     public Color fogColor;
    //     public float fogDensity;
    //     public Color ambientLight;
    //     public float haloStrength;
    //     public float flareStrength;
    public Material skyboxMaterial;

    public void RestoreSettings(Skybox skyboxCom)
    {
        skyboxCom.material = skyboxMaterial;
    }
}