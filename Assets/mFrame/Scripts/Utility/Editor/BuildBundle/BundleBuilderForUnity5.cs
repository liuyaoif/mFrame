using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Utility
{
    public class BundleBuilderForUnity5
    {
        const string BundlesOutputPath = "AssetBundle5";
        const string extensionName = ".bundle";
        private static string assetConfigFileName = "assetconfig.text";

        private static Dictionary<string, AssetsPackInfo.AssetInfo> assetInfoDict = new Dictionary<string, AssetsPackInfo.AssetInfo>();
        private static Dictionary<string, AssetsPackInfo.BundleInfo> bundleInfoDict = new Dictionary<string, AssetsPackInfo.BundleInfo>();
        private static List<string> m_tempLuaFolder = new List<string>();
        private static bool isEditorPlay;
        public static void MarkAndBuildBundles(BuildTarget target, bool isBuildPrefab, bool isBuildLua, bool isBuildScene, bool editorPlay = false)
        {
            try
            {
                isEditorPlay = editorPlay;
                assetInfoDict.Clear();
                bundleInfoDict.Clear();

                if (isBuildPrefab)
                {
                    MarkBehaviorTree();
                    MarkConfigFiles();
                    MarkBuildings();
                    MarkUITextures();
                    MarkUIPrefab();
                    MarkEffect();
                    MarkCharacters();
                    MarkMapParts();
                    MarkSound();
                    MarkVillageInfoConfig();
                    MarkStory();
                }

                if (isBuildLua)
                {
                    MarkLuaFiles();
                }

                if (isBuildScene)
                {
                    MarkScene();
                }

                //Build bundles
                if (!editorPlay)
                {
                    BuildAssetBundles(target);
                }

                //Write XML, copy bundles
                PostBuild();
                AssetDatabase.Refresh(ImportAssetOptions.Default);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e);
            }
            finally
            {
                RemoveTempLuaFolders();
            }

        }
        private static void MarkStory()
        {
            const string stroyPath = "RawBundles/StoryData/";
            string[] luaFolders = Directory.GetDirectories(Application.dataPath + "/" + stroyPath);
            foreach (var folder in luaFolders)
            {
                string[] datas = folder.Split('/');
                string dataPath = "Assets/" + stroyPath + datas[datas.Length - 1] + "/" + datas[datas.Length - 1] + "story.asset";
                UnityEngine.Debug.Log(dataPath);
                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(dataPath);
                if (obj != null)
                {
                    MarkAssetToBundle(dataPath, obj.name);
                    UnityEngine.Debug.Log(dataPath);
                }
            }
        }
        private static void MarkMapParts()
        {
            const string uiPrefabPath = "RawBundles/MapParts/";
            UnityEngine.Object[] allPrefabs = GetAssetsAtPath(uiPrefabPath);

            string assetPath = "";
            foreach (UnityEngine.Object prefab in allPrefabs)
            {
                try
                {
                    assetPath = AssetDatabase.GetAssetPath(prefab);
                    MarkAssetToBundle(assetPath, prefab.name + extensionName);
                }
                catch (Exception exp)
                {
                    LogManager.Instance.LogError("Pack " + prefab.name + " error. " + exp.ToString());
                }
            }


        }
        private static void MarkUITextures()
        {
            const string uiPrefabPath = "RawBundles/UITextures/";
            UnityEngine.Object[] allUITextures = GetAssetsAtPath(uiPrefabPath);

            string assetPath = "";
            foreach (UnityEngine.Object tempTexture in allUITextures)
            {
                assetPath = AssetDatabase.GetAssetPath(tempTexture);
                MarkAssetToBundle(assetPath, tempTexture.name + extensionName);
            }
        }

        //This function is for NGUI. UGUI do not need to mark atlas.
        private static void MarkUIPrefab()
        {
            const string uiPrefabPath = "RawBundles/UIPrefab/";
            UnityEngine.Object[] allPrefabs = GetAssetsAtPath(uiPrefabPath);

            List<UnityEngine.Object> fontList = new List<UnityEngine.Object>();
            List<UnityEngine.Object> atlasList = new List<UnityEngine.Object>();

            //Get all dependences of all assets.
            foreach (UnityEngine.Object asset in allPrefabs)
            {
                UnityEngine.Object[] dependence = EditorUtility.CollectDependencies(new UnityEngine.Object[] { asset });
                UnityEngine.Object[] fonts = AssetFilter.GetAssetsByTypes(dependence, new AssetFilter.AssetType[] { AssetFilter.AssetType.Font });
                foreach (UnityEngine.Object font in fonts)
                {
                    if (!fontList.Contains(font))
                    {
                        fontList.Add(font);
                    }
                }

                UnityEngine.Object[] uiAtlases = AssetFilter.GetAssetsByTypes(dependence, new AssetFilter.AssetType[] { AssetFilter.AssetType.UIAtlas });
                foreach (UnityEngine.Object atlas in uiAtlases)
                {
                    if (!atlasList.Contains(atlas))
                    {
                        atlasList.Add(atlas);
                    }
                }
            }

            string assetPath = "";
            foreach (UnityEngine.Object font in fontList)
            {
                assetPath = AssetDatabase.GetAssetPath(font);
                MarkAssetToBundle(assetPath, font.name + extensionName);
            }

            foreach (UnityEngine.Object atlas in atlasList)
            {
                assetPath = AssetDatabase.GetAssetPath(atlas);
                MarkAssetToBundle(assetPath, atlas.name + extensionName);
            }

            foreach (UnityEngine.Object prefab in allPrefabs)
            {
                assetPath = AssetDatabase.GetAssetPath(prefab);
                MarkAssetToBundle(assetPath, prefab.name + extensionName);
            }
        }
        private static void MarkConfigFiles()
        {
            const string bundleName = "DesignConfig";
            const string jsonFileRoot = "RawBundles/" + bundleName + "/";
            UnityEngine.Object[] allAssets = GetAssetsAtPath(jsonFileRoot);

            //All json files to one bundle.
            foreach (UnityEngine.Object asset in allAssets)
            {
                MarkAssetToBundle(asset, bundleName + extensionName);
            }
        }

        private static void MarkScene()
        {
            const string sceneFileRoot = "RawBundles/Scenes/";
            string[] fileEntries = Directory.GetFiles(Application.dataPath + "/" + sceneFileRoot, "*.unity",
                SearchOption.AllDirectories);
            EditorBuildSettingsScene[] seces = null;
            EditorBuildSettingsScene mainSec = new EditorBuildSettingsScene(Application.dataPath + "/Scenes/Main.unity", true);

            if (isEditorPlay)
            {
                seces = new EditorBuildSettingsScene[fileEntries.Length + 1];
            }
            else
            {
                seces = new EditorBuildSettingsScene[1];
            }
            seces[0] = mainSec;

            int idex = 1;
            foreach (string scenePath in fileEntries)
            {
                string localPath = "Assets" + scenePath.Replace("\\", "/").Replace(Application.dataPath, "");
                if (isEditorPlay)
                    seces[idex] = new EditorBuildSettingsScene(localPath, true);
                UnityEngine.Object scene = AssetDatabase.LoadMainAssetAtPath(localPath);
                idex++;
                MarkAssetToBundle(scene, scene.name + extensionName);
            }

            EditorBuildSettings.scenes = seces;
        }

        private static void MarkSound()
        {
            const string path = "RawBundles/Sound/";
            UnityEngine.Object[] allSound = GetAssetsAtPath(path);

            string assetPath = "";
            foreach (UnityEngine.Object tempSound in allSound)
            {
                assetPath = AssetDatabase.GetAssetPath(tempSound);
                MarkAssetToBundle(assetPath, tempSound.name + extensionName);
            }
        }

        private static void MarkBehaviorTree()
        {
            const string uiPrefabPath = "RawBundles/BehaviorTree/";
            UnityEngine.Object[] allPrefabs = GetAssetsAtPath(uiPrefabPath);

            string assetPath = "";
            foreach (UnityEngine.Object prefab in allPrefabs)
            {
                try
                {
                    assetPath = AssetDatabase.GetAssetPath(prefab);
                    MarkAssetToBundle(assetPath, prefab.name + extensionName);
                }
                catch (Exception exp)
                {
                    LogManager.Instance.LogError("Pack " + prefab.name + " error. " + exp.ToString());
                }
            }
        }

        private static void MarkCharacters()
        {
            const string uiPrefabPath = "RawBundles/Character/";
            UnityEngine.Object[] allPrefabs = GetAssetsAtPath(uiPrefabPath);

            string assetPath = "";
            foreach (UnityEngine.Object prefab in allPrefabs)
            {
                try
                {
                    assetPath = AssetDatabase.GetAssetPath(prefab);
                    MarkAssetToBundle(assetPath, prefab.name + extensionName);
                }
                catch (Exception exp)
                {
                    LogManager.Instance.LogError("Pack " + prefab.name + " error. " + exp.ToString());
                }
            }
        }

        private static void MarkBuildings()
        {
            const string path = "RawBundles/Village/";
            UnityEngine.Object[] allPrefabs = GetAssetsAtPath(path);

            string assetPath = "";
            foreach (UnityEngine.Object prefab in allPrefabs)
            {
                assetPath = AssetDatabase.GetAssetPath(prefab);
                MarkAssetToBundle(assetPath, prefab.name + extensionName);
            }
        }

        private static void MarkEffect()
        {
            const string path = "RawBundles/Effect/";
            UnityEngine.Object[] allPrefabs = GetAssetsAtPath(path);

            List<UnityEngine.Object> textureList = new List<UnityEngine.Object>();
            List<UnityEngine.Object> matList = new List<UnityEngine.Object>();

            //筛选纹理和材质
            foreach (UnityEngine.Object prefab in allPrefabs)
            {
                GameObject gm = prefab as GameObject;
                if (gm.GetComponent<BundleFindShader>() == null)
                    gm.AddComponent<BundleFindShader>();
                UnityEngine.Object[] dependence = EditorUtility.CollectDependencies(new UnityEngine.Object[] { prefab });
                UnityEngine.Object[] textures = AssetFilter.GetAssetsByTypes(dependence, new AssetFilter.AssetType[] { AssetFilter.AssetType.Texture });
                foreach (UnityEngine.Object tex in textures)
                {
                    if (!textureList.Contains(tex))
                    {
                        textureList.Add(tex);
                    }
                }

                UnityEngine.Object[] materials = AssetFilter.GetAssetsByTypes(dependence, new AssetFilter.AssetType[] { AssetFilter.AssetType.Material });
                foreach (UnityEngine.Object mat in materials)
                {
                    if (!matList.Contains(mat))
                    {
                        matList.Add(mat);
                    }
                }
            }

            string assetPath = "";

            //打包纹理
            foreach (UnityEngine.Object font in textureList)
            {
                assetPath = AssetDatabase.GetAssetPath(font);
                MarkAssetToBundle(assetPath, font.name + extensionName);
            }

            //打包材质
            foreach (UnityEngine.Object atlas in matList)
            {
                assetPath = AssetDatabase.GetAssetPath(atlas);
                MarkAssetToBundle(assetPath, atlas.name + extensionName);
            }


            //打包prefab
            foreach (UnityEngine.Object prefab in allPrefabs)
            {
                assetPath = AssetDatabase.GetAssetPath(prefab);
                MarkAssetToBundle(assetPath, prefab.name + extensionName);
            }
        }

        private static void MarkLuaFiles()
        {
            m_tempLuaFolder.Clear();

            const string luaFileRoot = "RawBundles/Lua/";
            string[] luaFolders = Directory.GetDirectories(Application.dataPath + "/" + luaFileRoot);
            foreach (var folder in luaFolders)
            {
                //Create luatxt folder.
                string txtLuaFolder = folder + "txt";
                if (Directory.Exists(txtLuaFolder))
                {
                    Directory.Delete(txtLuaFolder, true);
                }
                Directory.CreateDirectory(txtLuaFolder);

                //Copy .lua files to luatxt folder as .txt files.
                string[] allLuaFiles = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories);
                foreach (var luaFile in allLuaFiles)
                {
                    if (luaFile.IndexOf(".meta") != -1)
                    {
                        continue;
                    }
                    string luaFileName = Path.GetFileName(luaFile);
                    FileUtil.CopyFileOrDirectory(luaFile, txtLuaFolder + "/" + luaFileName + ".txt");
                }

                //Force refresh AssetDatabase.
                AssetDatabase.Refresh();

                //Pack lua txt files.
                UnityEngine.Object[] allLuaTxtFiles = GetAssetsAtPath(txtLuaFolder.Replace(Application.dataPath + "/", "") + "/");
                string folderName = Path.GetFileName(folder);
                foreach (var luaTxtFile in allLuaTxtFiles)
                {
                    MarkAssetToBundle(luaTxtFile, "Lua" + folderName + extensionName);
                }
                //Cache luatxt folder.
                m_tempLuaFolder.Add(txtLuaFolder);
            }
        }

        private static void MarkVillageInfoConfig()
        {
            const string path = "Assets/RawBundles/VillageInfoConfig.json";
            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset)) as TextAsset;
            string assetPath = AssetDatabase.GetAssetPath(asset);
            MarkAssetToBundle(assetPath, asset.name + extensionName);
        }

        private static void RemoveTempLuaFolders()
        {
            foreach (var path in m_tempLuaFolder)
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
                FileUtil.DeleteFileOrDirectory(path + ".meta");
            }
            AssetDatabase.Refresh();
        }

        public static void BuildAssetBundles(BuildTarget target)
        {
            // Choose the output path according to the build renderer.
            string outputPath = Path.Combine(BundlesOutputPath,
                AssetFilter.GetPlatformFolderForAssetBundles(target));

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            try
            {
                BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.None, target);
            }
            catch (Exception exp)
            {
                LogManager.Instance.LogError(exp.ToString());
            }
        }

        private static void PostBuild()
        {
            AssetDatabase.RemoveUnusedAssetBundleNames();
            string outputPath = Path.Combine(BundlesOutputPath,
                     AssetFilter.GetPlatformFolderForAssetBundles(EditorUserBuildSettings.activeBuildTarget));

            string configFilePath = outputPath + "/" + assetConfigFileName;
            if (Directory.Exists(configFilePath))
            {
                Directory.Delete(configFilePath);
            }

            foreach (var kvp in bundleInfoDict)
            {
                string bundleFileName = outputPath + "/" + kvp.Value.name;
                kvp.Value.MD5 = GetMD5HashFromFile(bundleFileName);
                kvp.Value.size = GetFileSize(bundleFileName);
            }

            AssetsPackInfo.WriteInfoToXMLFile(assetInfoDict, bundleInfoDict, configFilePath);

            assetInfoDict.Clear();
            bundleInfoDict.Clear();

            //Copy
            string copyPath = Application.streamingAssetsPath;
            CopyAssetBundlesTo(copyPath);
            AssetDatabase.Refresh();
        }

        static void CopyAssetBundlesTo(string outputPath)
        {
            // Clear streaming assets folder.
            string[] folders = Directory.GetDirectories(Application.streamingAssetsPath);
            foreach (var folder in folders)
            {
                if (Directory.Exists(folder))
                {
                    FileUtil.DeleteFileOrDirectory(folder);
                }
            }

            string outputFolder = AssetFilter.GetPlatformFolderForAssetBundles(EditorUserBuildSettings.activeBuildTarget);

            // Setup the source folder for assetbundles.
            var source = Path.Combine(Path.Combine(System.Environment.CurrentDirectory, BundlesOutputPath), outputFolder);
            if (!System.IO.Directory.Exists(source))
            {
                LogManager.Instance.Log("No assetBundle output folder, try to build the assetBundles first.");
            }

            // Setup the destination folder for assetbundles.
            var destination = System.IO.Path.Combine(outputPath, outputFolder);
            if (System.IO.Directory.Exists(destination))
                FileUtil.DeleteFileOrDirectory(destination);

            FileUtil.CopyFileOrDirectory(source, destination);
        }

        private static void MarkAssetToBundle(string assetFilePath, string bundleName)
        {
            AssetImporter importer = AssetImporter.GetAtPath(assetFilePath);
            if (importer == null)
            {
                return;
            }
            //importer.obj
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(assetFilePath);
            if (go != null)
            {
                Renderer rend = go.transform.GetComponentInChildren<Renderer>();
                if (rend != null)
                {
                    if (go.transform.GetComponentInChildren<BundleFindShader>() == null)
                    {
                        go.AddComponent<BundleFindShader>();
                    }
                }
            }
            bundleName = ValidBundleName(bundleName);
            importer.assetBundleName = bundleName;

            int index = assetFilePath.LastIndexOf("/");
            string assetName = assetFilePath.Substring(index).Replace("/", "");

            if (!assetInfoDict.ContainsKey(assetName))
            {
                AssetsPackInfo.AssetInfo assetInfo = new AssetsPackInfo.AssetInfo();
                assetInfo.name = assetName;
                assetInfo.bundleName = importer.assetBundleName;
                assetInfo.assetPath = importer.assetPath;
                assetInfoDict.Add(assetInfo.name, assetInfo);
            }
            else
            {
                LogManager.Instance.LogError("Error!. Multiple prefabs have same name : " + assetName);
            }

            if (!bundleInfoDict.ContainsKey(importer.assetBundleName))
            {
                AssetsPackInfo.BundleInfo bundleInfo = new AssetsPackInfo.BundleInfo();
                bundleInfo.name = importer.assetBundleName;
                bundleInfoDict.Add(bundleInfo.name, bundleInfo);
            }
        }

        private static void MarkAssetToBundle(UnityEngine.Object asset, string bundleName)
        {
            string path = AssetDatabase.GetAssetPath(asset);
            MarkAssetToBundle(path, bundleName);
        }

        public static void CleanAllBundles()
        {
            if (Directory.Exists(BundlesOutputPath))
            {
                Directory.Delete(BundlesOutputPath, true);
            }
        }

        public static void UnMarkAllAssets()
        {
            //const string uiPrefabPath = "RawBundles/UIPrefab/";
            //UnityEngine.Object[] allPrefabs = GetAssetsAtPath(uiPrefabPath);

            List<string> allFiles = ProjectReference.GetFilesRecursively(Application.dataPath);
            foreach (string file in allFiles)
            {
                if (file.IndexOf(".meta") != -1 ||
                    file.IndexOf(".cs") != -1 ||
                    file.IndexOf(".js") != -1)
                {
                    continue;
                }

                int index = file.IndexOf("Assets");
                string sub = file.Substring(index);
                string assetPath = sub.Replace("\\", "/");

                AssetImporter importer = AssetImporter.GetAtPath(assetPath);
                if (importer == null)
                {
                    continue;
                }

                importer.assetBundleName = null;
            }
            AssetDatabase.RemoveUnusedAssetBundleNames();
        }

        public static UnityEngine.Object[] GetAssetsAtPath(string path)
        {
            ArrayList assetsArray = new ArrayList();
            string[] fileEntries = Directory.GetFiles(Application.dataPath + "/" + path, "*.*", SearchOption.AllDirectories);
            foreach (string fileName in fileEntries)
            {
                string localPath = "Assets" + fileName.Replace("\\", "/").Replace(Application.dataPath, "");
                UnityEngine.Object t = AssetDatabase.LoadMainAssetAtPath(localPath);

                if (t != null)
                {
                    assetsArray.Add(t);
                }
            }

            UnityEngine.Object[] result = new UnityEngine.Object[assetsArray.Count];
            for (int i = 0; i < assetsArray.Count; i++)
            {
                result[i] = (UnityEngine.Object)assetsArray[i];
            }

            return result;
        }

        private static string GetMD5HashFromFile(string fileName)
        {

            if (isEditorPlay)
                return "";
            try
            {
                FileStream file = new FileStream(fileName, FileMode.Open);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
            }
        }

        public static long GetFileSize(string fileName)
        {
            if (isEditorPlay)
                return 0;
            try
            {
                FileInfo fileInfo = new FileInfo(fileName);
                return fileInfo.Length;
            }
            catch (Exception ex)
            {
                throw new Exception("GetFileSize() fail,error:" + ex.Message);
            }
        }

        public static void RunExcel2JsonBat()
        {
            string batPath = Application.dataPath + "/../excel2json/转换Excel为Json和C#.bat";

            Process pro = new Process();
            FileInfo file = new FileInfo(batPath);
            pro.StartInfo.WorkingDirectory = file.Directory.FullName;
            pro.StartInfo.FileName = batPath;
            pro.StartInfo.CreateNoWindow = false;
            pro.Start();
            pro.WaitForExit();
        }

        private static string ValidBundleName(string bundleName)
        {
            if (bundleName.Contains(" "))
            {
                LogManager.Instance.LogWarning(bundleName + " contains space");
                return bundleName.Replace(" ", "_");
            }
            return bundleName;
        }
    }//BundleBuilderForUnity5
}
