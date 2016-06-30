using UnityEngine;
using System.Collections;
using UnityEditor;

public class AssetAftercure : UnityEditor.AssetModificationProcessor
{
    private static string effectOrdinaryPath = "Assets/RawBundles/Effect/fx_cj_effect";
    private static string effectPrefabPath = "Assets/RawBundles/Effect/fx_prefab";
    //public static void OnWillCreateAsset(string str)
    //{
    //    //AssetDatabase.f
    //    //Debug.Log("MoveAsset" + str);
    //}
    //    public static AssetDeleteResult OnWillDeleteAsset(string str, RemoveAssetOptions op)
    //    {
    //        if (EditorUtility.DisplayDialog("文件删除", str + "  文件将会被删除并找不回来","确定", "取消"))
    //        {
    //            return AssetDeleteResult.DidNotDelete;
    //        }
    //
    //        //Debug.Log("MoveAsset" + str);
    //        return AssetDeleteResult.FailedDelete;
    //    }

    private static void AddPool(string str, string str2)
    {
        if (str2.IndexOf(effectOrdinaryPath) >= 0)
        {
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(str);
            if (go != null)
            {
                if (go.GetComponent<BattleObjectPool>() != null)
                {
                    GameObject.Destroy(go.GetComponent<BattleObjectPool>());
                }

                go.AddComponent<NCMParticleSystemPoolObject>();
            }
            AssetDatabase.SaveAssets();
        }
        if (str2.IndexOf(effectPrefabPath) >= 0)
        {
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(str);
            if (go != null)
            {
                if (go.GetComponent<BattleObjectPool>() != null)
                {
                    GameObject.Destroy(go.GetComponent<BattleObjectPool>());
                }
                //if (go.GetComponent<NCMAttachPrefabPoolObject>() == null)
                //{
                go.AddComponent<NCMAttachPrefabPoolObject>();
                //}
            }
            AssetDatabase.SaveAssets();
        }
    }
    public static AssetMoveResult OnWillMoveAsset(string str, string str2)
    {
        //Debug.Log("MoveAsset" + str);
        if (EditorUtility.DisplayDialog("文件移动", str + "  移动到   " + str2, "确定", "取消"))
        {

            AddPool(str, str2);

            return AssetMoveResult.DidNotMove;
        }

        return AssetMoveResult.FailedMove;
    }
}
