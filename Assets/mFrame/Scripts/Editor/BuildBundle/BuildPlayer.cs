using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Utility;

public class BuildPlayer
{
    public class XMLNode
    {
        public XMLNode GetNode(string name)
        {
            return null;
        }

        public string GetValue(string name)
        {
            return "";
        }
    }

    //得到工程中所有场景名称
    static string[] SCENES = FindEnabledEditorScenes();

    private static XMLNode GetAppConfig()
    {
        FileStream stream = File.OpenRead(Application.streamingAssetsPath + "/GlobalConfig.text");
        TextReader reader = new StreamReader(stream);

        string content = "";
        string line = null;
        while ((line = reader.ReadLine()) != null)
        {
            content += line;
        }

        //XMLParser xmlParser = new XMLParser();
        //XMLNode rootNode = xmlParser.Parse(content);
        return new XMLNode();
    }

    public static void Build(BuildTarget target, bool isApk = true)
    {
        XMLNode rootNode = GetAppConfig();

        //Is debug build.
        //bool isDebug = Boolean.Parse(rootNode.GetNode("WuxiaRPG>0>DebugConfig>0>DebugBuild>0").GetValue("@value"));

        //product name.
        XMLNode node = rootNode.GetNode("WuxiaRPG>0>ProductName>0");
        string productName = node.GetValue("@value");

        //version
        node = rootNode.GetNode("WuxiaRPG>0>Version>0");
        string version = node.GetValue("@value");

        //output name
        node = rootNode.GetNode("WuxiaRPG>0>OutputName>0");
        string outputName = node.GetValue("@value");

        //Player settings.

        node = rootNode.GetNode("WuxiaRPG>0>BundleName>0");
        PlayerSettings.bundleIdentifier = node.GetValue("@value");
        PlayerSettings.bundleVersion = version;

        //Build bundle and build player.
        BundleBuilderForUnity5.MarkAndBuildBundles(target, true, true, true);
        string target_dir = Application.dataPath.Replace("/Assets", "");
        string target_name = (target == BuildTarget.Android) ? outputName + version + ".apk" : outputName + version;

        BuildOptions options = BuildOptions.None;

        if (target == BuildTarget.iOS)
        {
            PlayerSettings.productName = productName;
            target_name = outputName + version;
        }
        else if (target == BuildTarget.Android)
        {
            if (!isApk)
            {
                options |= BuildOptions.AcceptExternalModificationsToPlayer;
                PlayerSettings.productName = outputName;//project的名称只能是ASCII的
                target_name = outputName + version;
            }
            else
            {
                PlayerSettings.productName = productName;
                target_name = outputName + version + ".apk";
            }
        }

        if (File.Exists(target_name))
        {
            File.Delete(target_name);
        }

        if (Directory.Exists(target_name))
        {
            Directory.Delete(target_name);
        }

        //         switch (name)
        //         {
        //             case "QQ":
        //                 PlayerSettings.bundleIdentifier = "com.game.qq";
        //                 PlayerSettings.bundleVersion = "v0.0.1";
        //                 PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, "QQ");
        //                 break;
        //             case "UC":
        //                 PlayerSettings.bundleIdentifier = "com.game.uc";
        //                 PlayerSettings.bundleVersion = "v0.0.1";
        //                 PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, "UC");
        //                 break;
        //             case "CMCC":
        //                 PlayerSettings.bundleIdentifier = "com.game.cmcc";
        //                 PlayerSettings.bundleVersion = "v0.0.1";
        //                 PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, "CMCC");
        //                 break;
        //         }

        //==================这里是比较重要的东西=======================

        GenericBuild(SCENES, target_dir + "/" + target_name, target, options);
    }

    private static string[] FindEnabledEditorScenes()
    {
        List<string> EditorScenes = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled) continue;
            EditorScenes.Add(scene.path);
        }
        return EditorScenes.ToArray();
    }

    static void GenericBuild(string[] scenes, string target_dir, BuildTarget build_target, BuildOptions build_options)
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(build_target);
        string res = BuildPipeline.BuildPlayer(scenes, target_dir, build_target, build_options);

        if (res.Length > 0)
        {
            throw new Exception("BuildPlayer failure: " + res);
        }
    }
}