using UnityEditor;
using UnityEngine;
using Utility;

public class BundleMenuItems
{
    //     [MenuItem("AssetBundle/Build Bundle For Unity4.x")]
    //     private static void BuildBundle4()
    //     {
    //         BundleBuilderForUnity4.AutoCollectDepedenceBuild();
    //     }

    [MenuItem("AssetBundle/Build Bundles/All")]
    private static void BuildAllBundle()
    {
        BundleBuilderForUnity5.MarkAndBuildBundles(EditorUserBuildSettings.activeBuildTarget, true, true, true);
    }
    [MenuItem("AssetBundle/Build Bundles/EditorPlay")]
    private static void BuildAllBundleEditorPlay()
    {
        BundleBuilderForUnity5.MarkAndBuildBundles(EditorUserBuildSettings.activeBuildTarget, true, false, true, true);
    }
    [MenuItem("AssetBundle/Build Bundles/Prefabs")]
    private static void BuildPrefabBundle()
    {
        BundleBuilderForUnity5.MarkAndBuildBundles(EditorUserBuildSettings.activeBuildTarget, true, false, false);
    }

    [MenuItem("AssetBundle/Build Bundles/Lua")]
    private static void BuildLuaBundle()
    {
        BundleBuilderForUnity5.MarkAndBuildBundles(EditorUserBuildSettings.activeBuildTarget, false, true, false);
    }

    [MenuItem("AssetBundle/Build Bundles/Scene")]
    private static void BuildSceneBundle()
    {
        BundleBuilderForUnity5.MarkAndBuildBundles(EditorUserBuildSettings.activeBuildTarget, false, false, true);
    }

    [MenuItem("AssetBundle/Clean all bundles")]
    private static void CleanAllBundles()
    {
        BundleBuilderForUnity5.CleanAllBundles();
    }

    [MenuItem("AssetBundle/Unmark all assets")]
    private static void UnMarkAllAssets()
    {
        BundleBuilderForUnity5.UnMarkAllAssets();
    }

    [MenuItem("AssetBundle/Build player/iOS")]
    private static void BuildPlayerIOS()
    {
        BuildPlayer.Build(BuildTarget.iOS);
    }

    [MenuItem("AssetBundle/Build player/Android apk")]
    private static void BuildPlayerAndroid()
    {
        //BundleBuilderForUnity5.RunExcel2JsonBat();
        BuildPlayer.Build(BuildTarget.Android);
    }

    [MenuItem("AssetBundle/Build player/Google android project")]
    private static void BuildPlayerGoogleProject()
    {
        //BundleBuilderForUnity5.RunExcel2JsonBat();
        BuildPlayer.Build(BuildTarget.Android, false);
    }

    [MenuItem("Assets/CreateAnimData")]
    static void CreateAnimData()
    {
        string path = EditorUtility.SaveFilePanel("Save Resource", "", "New Resource", "asset");
        if (path.Length != 0)
        {
            ScriptableObject testda = ScriptableObject.CreateInstance(Selection.activeObject.name);
            if (testda == null)
                return;
            path = path.Replace(Application.dataPath, "Assets");
            AssetDatabase.CreateAsset(testda, path);
        }
    }
}