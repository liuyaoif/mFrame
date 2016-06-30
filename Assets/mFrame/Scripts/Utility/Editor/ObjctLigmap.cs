using UnityEditor;
using UnityEngine;

public class ObjctLigmap : EditorWindow
{
    GameObject game;

    [MenuItem("Tools/Rest_ColorQD")]
    static void Rend()
    {
        Renderer[] rends = Object.FindObjectsOfType<Renderer>();
        for (int i = 0; i < rends.Length; i++)
        {
            Material[] maters = rends[i].sharedMaterials;
            for (int j = 0; j < maters.Length; j++)
            {
                if (maters[j].HasProperty("_ColorQD"))
                {
                    maters[j].SetFloat("_ColorQD", 1);
                }
            }
        }
    }


    [MenuItem("Tools/SaveLightMap")]
    static void OpneWind()
    {
        EditorWindow.CreateInstance<ObjctLigmap>().Show();
    }


    void OnGUI()
    {
        game = EditorGUILayout.ObjectField(game, typeof(GameObject), true) as GameObject;
        if (GUILayout.Button("Set"))
        {
            Save(game.GetComponentsInChildren<SaveLightmapToRenderer>());
        }
        if (GUILayout.Button("Res"))
        {
            Rem(game.GetComponentsInChildren<SaveLightmapToRenderer>());
        }

    }
    void Save(SaveLightmapToRenderer[] maps)
    {
        for (int i = 0; i < maps.Length; i++)
        {
            GameObject game = maps[i].gameObject;
            //GameObject.DestroyImmediate(maps[i]);
            maps[i].SaveLmInfo();


            GameObject go = PrefabUtility.GetPrefabParent(game) as GameObject;
            PrefabUtility.ReplacePrefab(game, go);

        }
    }
    void Rem(SaveLightmapToRenderer[] maps)
    {
        for (int i = 0; i < maps.Length; i++)
        {
            GameObject game = maps[i].gameObject;
            GameObject.DestroyImmediate(maps[i]);
            //maps[i].SaveLmInfo();
            //SerializedObject ser = new SerializedObject(game);
            //ser.ApplyModifiedProperties();
            //PrefabUtility.MergeAllPrefabInstances(maps[i].gameObject);
            //PropertyModification[] info=  PrefabUtility.GetPropertyModifications(maps[i].gameObject);
            //PrefabUtility.SetPropertyModifications(PrefabUtility.GetPrefabParent(maps[i].gameObject), info);

            GameObject go = PrefabUtility.GetPrefabParent(game) as GameObject;
            PrefabUtility.ReplacePrefab(game, go);
            //PrefabUtility.ResetToPrefabState(game);
            //Debug.Log(go.GetComponents<MonoBehaviour>().Length);
        }
    }
}
