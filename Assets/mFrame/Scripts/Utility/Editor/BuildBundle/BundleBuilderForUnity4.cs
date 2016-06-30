using UnityEditor;
using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Collections;
using Utility;

namespace Utility
{
    /*
    public sealed class BundleBuilderForUnity4
    {
        public static string bundlePath = "AssetBundle4/";
        private const string extensionName = ".assetBundle";

        private static List<AssetsPackInfo.AssetInfo> assetList = new List<AssetsPackInfo.AssetInfo>();
        private static List<AssetsPackInfo.BundleInfo> bundleList = new List<AssetsPackInfo.BundleInfo>();

        private static BuildTarget m_target = BuildTarget.StandaloneWindows;

        //[MenuItem("Build bundle/INDEPENDENT from selects")]
        private static void IndependentBuildSelects()
        {
            CheckBundleSavePathExist();
            UnityEngine.Object[] SelectedAsset = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
            for (int i = 0; i < SelectedAsset.Length; i++)
            {
                OneAssetToOneBundle(SelectedAsset[i], bundlePath + SelectedAsset[i].name + extensionName,
                    BuildAssetBundleOptions.CollectDependencies, null);
            }
        }

        //[MenuItem("Build bundle/DEPENDENT UI")]
        private static void DpendentBuildUI()
        {
            CheckBundleSavePathExist();
            string UIAssetsPath = "RawBundles/UI/";
            string dependencePath = "Dependence/";
            string dependenceBundleName = "UIDependence";

            assetList.Clear();
            bundleList.Clear();

            //Independent scene. Font.
            BuildPipeline.PushAssetDependencies();
            UnityEngine.Object[] independentAssets = GetAssetsAtPath(UIAssetsPath + dependencePath);
            string dependentBundleName = bundlePath + dependenceBundleName + extensionName;
            MultiAssetsToOneBundle(independentAssets, dependentBundleName, BuildAssetBundleOptions.CollectDependencies, null);//Build Font

            //Dependent scene. UIPrefab.
            {
                BuildAssetBundleOptions buildOption = BuildAssetBundleOptions.CollectDependencies |
                    BuildAssetBundleOptions.CompleteAssets |
                    BuildAssetBundleOptions.DeterministicAssetBundle;

                UnityEngine.Object[] dependentAssets = GetAssetsAtPath(UIAssetsPath);

                for (int i = 0; i < dependentAssets.Length; i++)
                {
                    BuildPipeline.PushAssetDependencies();
                    OneAssetToOneBundle(dependentAssets[i],
                        bundlePath + dependentAssets[i].name + extensionName, buildOption,
                        new List<string> { dependentBundleName });//Build UI
                    BuildPipeline.PopAssetDependencies();
                }
            }

            BuildPipeline.PopAssetDependencies();

            //Write XML file.

            string path = Directory.GetCurrentDirectory() + "/AssetBundle4/AssetsConfig.xml";
            AssetsPackInfo.WriteInfoToXMLFile(assetList, bundleList, path);
        }//DpendencePackUI

        public static void AutoCollectDepedenceBuild()
        {
            CheckBundleSavePathExist();
            string UIAssetsPath = "RawBundles/UIPrefab/";

            assetList.Clear();
            bundleList.Clear();

            //Get all assets.
            UnityEngine.Object[] allAssets = GetAssetsAtPath(UIAssetsPath);

            List<UnityEngine.Object> fontList = new List<UnityEngine.Object>();
            List<UnityEngine.Object> atlasList = new List<UnityEngine.Object>();

            Dictionary<UnityEngine.Object, List<string>> dependenceDict = new Dictionary<UnityEngine.Object, List<string>>();

            foreach (UnityEngine.Object asset in allAssets)
            {
                List<string> dpdNameList = new List<string>();

                //Get all dependences of all assets.
                UnityEngine.Object[] dependence = EditorUtility.CollectDependencies(new UnityEngine.Object[] { asset });
                UnityEngine.Object[] fonts = AssetFilter.GetAssetsByTypes(dependence, new AssetFilter.AssetType[] { AssetFilter.AssetType.Font });
                foreach (UnityEngine.Object font in fonts)
                {
                    if (!fontList.Contains(font))
                    {
                        fontList.Add(font);
                    }
                    dpdNameList.Add(font.name);
                }

                UnityEngine.Object[] uiAtlases = AssetFilter.GetAssetsByTypes(dependence, new AssetFilter.AssetType[] { AssetFilter.AssetType.UIAtlas });
                foreach (UnityEngine.Object atlas in uiAtlases)
                {
                    if (!atlasList.Contains(atlas))
                    {
                        atlasList.Add(atlas);
                    }
                    dpdNameList.Add(atlas.name);
                }
                dependenceDict.Add(asset, dpdNameList);
            }

            //Build fonts.
            for (int i = 0; i < fontList.Count; i++)
            {
                BuildPipeline.PushAssetDependencies();
                OneAssetToOneBundle(fontList[i], bundlePath + fontList[i].name + extensionName, BuildAssetBundleOptions.CollectDependencies, null);
            }

            //Build Atlas.
            for (int i = 0; i < atlasList.Count; i++)
            {
                BuildPipeline.PushAssetDependencies();
                OneAssetToOneBundle(atlasList[i], bundlePath + atlasList[i].name + extensionName, BuildAssetBundleOptions.CollectDependencies, null);
            }


            //Build assets.
            BuildAssetBundleOptions buildOption = BuildAssetBundleOptions.CollectDependencies |
                BuildAssetBundleOptions.CompleteAssets |
                BuildAssetBundleOptions.DeterministicAssetBundle;
            for (int i = 0; i < allAssets.Length; i++)
            {
                BuildPipeline.PushAssetDependencies();
                List<string> nameList = dependenceDict[allAssets[i]];
                OneAssetToOneBundle(allAssets[i], bundlePath + allAssets[i].name + extensionName, buildOption, nameList);//Build UI
                BuildPipeline.PopAssetDependencies();
            }

            int popCount = fontList.Count + atlasList.Count;
            for (int i = 0; i < popCount; i++)
            {
                BuildPipeline.PopAssetDependencies();
            }

            //Write XML file.
            string path = Directory.GetCurrentDirectory() + "/AssetBundle4/AssetsConfig.xml";
            AssetsPackInfo.WriteInfoToXMLFile(assetList, bundleList, path);
        }

        #region BuildBundle
        private static void OneAssetToOneBundle(UnityEngine.Object asset, string bundleFileName, BuildAssetBundleOptions option, List<string> dependentBundles)
        {
            DeleteExistFile(bundleFileName);

            //create AssetBundle
            if (BuildPipeline.BuildAssetBundle(asset, null, bundleFileName, option, m_target))
            {
                assetList.Add(AssetsPackInfo.CreateAssetInfo(asset.name, bundleFileName));
                bundleList.Add(AssetsPackInfo.CreateBundleInfo(bundleFileName, dependentBundles, GetMD5String(bundleFileName)));
            }
            else
            {
                UtilTools.Assert(false, "Bundle " + bundleFileName + "is error");
            }
        }

        private static void MultiAssetsToOneBundle(UnityEngine.Object[] assets, string bundleFileName, BuildAssetBundleOptions option, List<string> dependentBundles)
        {
            DeleteExistFile(bundleFileName);
            if (BuildPipeline.BuildAssetBundle(assets[0], assets, bundleFileName, option, m_target))
            {
                foreach (UnityEngine.Object asset in assets)
                {
                    assetList.Add(AssetsPackInfo.CreateAssetInfo(asset.name, bundleFileName));
                }
                bundleList.Add(AssetsPackInfo.CreateBundleInfo(bundleFileName, dependentBundles, GetMD5String(bundleFileName)));
            }
            else
            {
                UtilTools.Assert(false, "Bundle " + bundleFileName + "is error");
            }
        }
        #endregion

        #region tools
        private static void CheckBundleSavePathExist()
        {
            if (!Directory.Exists(bundlePath))
            {//create AssetBundle save path
                Directory.CreateDirectory(bundlePath);
            }
        }

        private static void DeleteExistFile(string targetPath)
        {
            if (File.Exists(targetPath))
            {
                File.Delete(targetPath);
            }
        }

        private static string GetMD5String(string filePath)
        {
            MD5 md5 = MD5.Create();

            FileStream fs = new FileStream(filePath, FileMode.Open);
            byte[] result = md5.ComputeHash(fs);
            fs.Close();
            string md5Value = BitConverter.ToString(result).Replace("-", "").ToLower();
            return md5Value;
        }

        private static char[] splits = new char[] { '/', '.' };
        public static string SpliteName(string name)
        {
            string[] splitName = name.Split(splits);

            if (splitName.Length > 2)
            {
                return splitName[splitName.Length - 2];
            }
            else
            {
                return "";
            }
        }

        public static UnityEngine.Object[] GetAssetsAtPath(string path)
        {
            ArrayList assetsArray = new ArrayList();
            string[] fileEntries = Directory.GetFiles(Application.dataPath + "/" + path);
            foreach (string fileName in fileEntries)
            {
                int index = fileName.LastIndexOf("/");
                string localPath = "Assets/" + path;

                if (index > 0)
                {
                    string sub = fileName.Substring(index).Replace("/", "");
                    localPath += sub;
                }

                UnityEngine.Object t = AssetDatabase.LoadMainAssetAtPath(localPath);//Resources.LoadAssetAtPath(localPath, typeof(T));

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
        #endregion
    }//BundleBuilderForUnity4
*/
}