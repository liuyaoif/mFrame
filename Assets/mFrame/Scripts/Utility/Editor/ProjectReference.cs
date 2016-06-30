using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


public class ProjectReference : EditorWindow
{
    [MenuItem("Assets/Find Reference In Project", false, 20)]
    private static void FindReferenceInProject()
    {
        SerachAssetRefByGuid(Selection.objects);
    }

    private static void CheckAssetsInFolderRef(string folderPath, string fileExtName = "")
    {
        if (fileExtName == "")
        {
            fileExtName = ".prefab";
        }

        GetFilesRecursively(folderPath);

        List<UnityEngine.Object> objList = new List<UnityEngine.Object>();

        for (int i = 0; i < fileList.Count; i++)
        {
            string file = fileList[i];
            if (file.IndexOf(fileExtName) != -1)
            {
                int index = file.IndexOf("Assets");
                string sub = file.Substring(index);
                string assetPath = sub.Replace("\\", "/");
                UnityEngine.Object prefabAsset = AssetDatabase.LoadMainAssetAtPath(assetPath);

                if (null != prefabAsset)
                {
                    objList.Add(prefabAsset);
                }
            }
        }

        SerachAssetRefByGuid(objList.ToArray());
    }

    private static void SerachAssetRefByGuid(UnityEngine.Object[] assets)
    {
        if (null == assets || assets.Length == 0)
        {
            return;
        }

        //Collect all assets.
        GetFilesRecursively(Application.dataPath);

        foreach (UnityEngine.Object asset in assets)
        {
            //Find scene guid.
            int iid = asset.GetInstanceID();
            string selectName = AssetDatabase.GetAssetPath(iid);
            if (AssetDatabase.IsMainAsset(iid))
            {
                FileStream stream = File.OpenRead(selectName + ".meta");
                TextReader reader = new StreamReader(stream);

                string line = null;
                string assetGUID = "";
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.IndexOf("guid") != -1)
                    {
                        assetGUID = line.Replace("guid: ", "");
                        break;
                    }
                }

                reader.Close();
                stream.Close();

                List<string> matches = new List<string>();
                //Loop all files in project.
                for (int i = 0; i < fileList.Count; i++)
                {
                    string path = fileList[i];
                    try
                    {
                        if (String.IsNullOrEmpty(path))
                        {
                            continue;
                        }

                        if (path.IndexOf(".prefab") == -1 &&
                            path.IndexOf(".mat") == -1 &&
                            path.IndexOf("cs.meta") == -1 &&
                            path.IndexOf(".anim") == -1 &&
                            path.IndexOf(".controller") == -1 &&
                            path.IndexOf(".renderTexture") == -1 &&
                            path.IndexOf(".physicMaterial") == -1 &&
                            path.IndexOf(".physicsMaterial2D") == -1 &&
                            path.IndexOf(".overrideController") == -1 &&
                            path.IndexOf(".mask") == -1 &&
                            path.IndexOf(".cubemap") == -1 &&
                            path.IndexOf(".flare") == -1 &&
                            path.IndexOf(".fontsettings") == -1 &&
                            path.IndexOf(".guiskin") == -1 &&
                            path.IndexOf(".unity") == -1)//If not file that contains guid(static reference), continue.
                        {
                            continue;
                        }

                        string content = File.ReadAllText(path);
                        if (!String.IsNullOrEmpty(content) && content.Contains(assetGUID))
                        {
                            matches.Add(path.Replace("\\", "/").Replace(Application.dataPath, ""));
                        }
                    }
                    catch (Exception exp)
                    {
                        LogManager.Instance.Log(exp.ToString());
                    }
                }

                string[] splits = selectName.Split("/".ToCharArray());
                string outPut = "~~~Asset: " + splits[splits.Length - 1] + " is referred by " + matches.Count + " asset(s): \n";
                foreach (var path in matches)
                {
                    outPut += path + "\n";
                }
                outPut += "~~~End\n";
                LogManager.Instance.Log(outPut);
                matches.Clear(); // clear the list
            }
        }
    }

    private static List<string> dirList = new List<string>();
    private static List<string> fileList = new List<string>();
    public static List<string> GetFilesRecursively(string root)
    {
        dirList.Clear();
        fileList.Clear();

        DoGetDir(root);

        foreach (string path in dirList)
        {
            DirectoryInfo Dir = new DirectoryInfo(path);
            foreach (FileInfo f in Dir.GetFiles("*.*"))
            {
                fileList.Add(f.ToString());
            }
        }
        return fileList;
    }

    private static void DoGetDir(string root) //参数dirPath为指定的目录
    {
        if (!dirList.Contains(root))
        {
            dirList.Add(root);
        }

        DirectoryInfo Dir = new DirectoryInfo(root);
        try
        {
            foreach (DirectoryInfo d in Dir.GetDirectories())
            {
                dirList.Add(d.ToString() + "\\");
                DoGetDir(d.ToString() + "\\");
            }
        }
        catch (Exception e)
        {
            LogManager.Instance.Log(e);
        }
    }
}
