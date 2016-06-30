//using DynamicShadowProjector;
using UnityEngine;

[DisallowMultipleComponent]
public class BundleFindShader : MonoBehaviour
{
    //public bool checkProjector = false;

    void Awake()
    {
        LoadShaderForRenderer();

        //         if (checkProjector)
        //         {
        //             LoadShaderForProjector();
        //         }
    }

    private void LoadShaderForRenderer()
    {
        Renderer[] rends = transform.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < rends.Length; i++)
        {
            for (int j = 0; j < rends[i].sharedMaterials.Length; j++)
            {
                if (rends[i].sharedMaterials[j] == null)
                    continue;
                //  Debug.Log(rends[i].sharedMaterials[j].shader.name);
                rends[i].sharedMaterials[j].shader = Shader.Find(rends[i].sharedMaterials[j].shader.name);

            }
        }
    }

    //     private void LoadShaderForProjector()
    //     {
    //         Projector[] projectors = transform.GetComponentsInChildren<Projector>(true);
    //         for (int i = 0; i < projectors.Length; i++)
    //         {
    //             //Debug.Log(projectors[i].material.shader.name);
    //             projectors[i].material.shader = Shader.Find(projectors[i].material.shader.name);
    //             //ShadowTextureRenderer shadowTex = projectors[i].GetComponent<ShadowTextureRenderer>();
    //             //Debug.Log(shadowTex.blurShader.shader.name);
    //             shadowTex.blurShader.shader = Shader.Find(shadowTex.blurShader.shader.name);
    //             //Debug.Log(shadowTex.downsampleShader.shader.name);
    //             shadowTex.downsampleShader.shader = Shader.Find(shadowTex.downsampleShader.shader.name);
    //             //Debug.Log(shadowTex.copyMipmapShader.shader.name);
    //             shadowTex.copyMipmapShader.shader = Shader.Find(shadowTex.copyMipmapShader.shader.name);
    //             //Debug.Log(shadowTex.eraseShadowShader.shader.name);
    //             shadowTex.eraseShadowShader.shader = Shader.Find(shadowTex.eraseShadowShader.shader.name);
    //         }
    //     }
}//BundleFindShader
