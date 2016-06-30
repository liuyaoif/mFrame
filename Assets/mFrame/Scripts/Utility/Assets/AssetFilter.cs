using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Utility
{
    public class AssetFilter
    {
        public enum AssetType
        {
            Rigidbody,
            Transform,
            MonoBehaviour,
            Text,
            Material,
            Shader,
            Texture,
            GameObject,
            UIAtlas,
            Font,
            Count,
            Invalid,
        }

        #region Filter
        public static UnityEngine.Object[] RemoveAssetsByTypes(UnityEngine.Object[] inArray, AssetType[] filterTypes)
        {
            List<UnityEngine.Object> outList = new List<UnityEngine.Object>();
            foreach (UnityEngine.Object obj in inArray)
            {
                AssetType curType = GetTypeOfAsset(obj);

                bool isInFilterType = false;
                foreach (AssetType type in filterTypes)
                {
                    if (curType == type)
                    {
                        isInFilterType = true;
                        break;
                    }
                }

                if (!isInFilterType)
                {
                    outList.Add(obj);
                }
            }
            return outList.ToArray();
        }

        public static UnityEngine.Object[] GetAssetsByTypes(UnityEngine.Object[] inArray, AssetType[] types)
        {
            List<UnityEngine.Object> outList = new List<UnityEngine.Object>();
            foreach (UnityEngine.Object obj in inArray)
            {
                AssetType curType = GetTypeOfAsset(obj);

                bool isType = false;
                foreach (AssetType type in types)
                {
                    if (curType == type)
                    {
                        isType = true;
                        break;
                    }
                }

                if (isType)
                {
                    outList.Add(obj);
                }
            }
            return outList.ToArray();
        }


        public static AssetType GetTypeOfAsset(UnityEngine.Object asset)
        {
            AssetType type;
            if (asset is Texture)
            {
                type = AssetType.Texture;
            }
            else if (asset is TextAsset)//Scripts
            {
                type = AssetType.Text;
            }
            else if (asset is Material)
            {
                type = AssetType.Material;
            }
            else if (asset is Shader)
            {
                type = AssetType.Shader;
            }
            else if (asset is Rigidbody)
            {
                type = AssetType.Rigidbody;
            }
            else if (asset is Transform)
            {
                type = AssetType.Transform;
            }
            else if (asset is MonoBehaviour)
            {
                type = AssetType.MonoBehaviour;
            }
            else if (asset is GameObject)
            {
                type = AssetType.GameObject;
                GameObject go = asset as GameObject;
                if (go.GetComponent<UIAtlas>())
                {
                    type = AssetType.UIAtlas;
                }
            }
            else if (asset is Font)
            {
                type = AssetType.Font;
            }
            else
            {
                type = AssetType.Invalid;
            }

            return type;
        }

        public static string PrintAssets(UnityEngine.Object[] objects)
        {
            string ret = "";
            foreach (UnityEngine.Object obj in objects)
            {
                ret += obj.name + "~" + GetTypeOfAsset(obj).ToString() + "\n";
            }
            return ret;
        }
        #endregion

        #region Children
        public static UnityEngine.Object[] RemoveChildrenGameObjects(UnityEngine.Object[] children, UnityEngine.Object[] parents)
        {
            UnityEngine.Object[] parentsList = GetAssetsByTypes(parents, new AssetType[] { AssetType.GameObject });
            List<UnityEngine.Object> outList = new List<UnityEngine.Object>();

            foreach (var child in children)
            {
                if (AssetType.GameObject != GetTypeOfAsset(child))
                {
                    outList.Add(child);
                    continue;
                }

                GameObject childObj = child as GameObject;
                bool isChild = false;

                foreach (var parent in parentsList)
                {
                    GameObject parentObj = parent as GameObject;
                    isChild = IsChild(childObj, parentObj);
                    if (isChild)
                    {
                        break;
                    }
                }

                if (!isChild)
                {
                    outList.Add(child);
                }
            }

            return outList.ToArray();
        }


        private static List<GameObject> m_children = new List<GameObject>();
        private static List<GameObject> GetChildrenRecursively(GameObject root)
        {
            List<GameObject> temp = new List<GameObject>();
            foreach (Transform trans in root.transform)
            {
                //Debug.Log (trans.propName);
                GetChildrenRecursively(trans.gameObject);
                m_children.Add(trans.gameObject);
            }
            return temp;
        }

        public static bool IsChild(GameObject child, GameObject parent)
        {
            if (child == parent)
            {
                return true;
            }
            else
            {
                m_children.Clear();
                GetChildrenRecursively(parent);

                if (m_children.Contains(child))
                {
                    return true;
                }
            }

            return false;
        }
        #endregion

        public static string GetPlatformFolderForAssetBundles(RuntimePlatform platform)
        {
            switch (platform)
            {
                case RuntimePlatform.Android:
                    return "Android";
                case RuntimePlatform.IPhonePlayer:
                    return "iOS";
                case RuntimePlatform.WindowsWebPlayer:
                case RuntimePlatform.OSXWebPlayer:
                    return "WebPlayer";
                case RuntimePlatform.WindowsPlayer:
                    return "Windows";
                case RuntimePlatform.OSXPlayer:
                    return "OSX";

                default:
                    return "Unknown";
            }
        }

#if UNITY_EDITOR
        public static string GetPlatformFolderForAssetBundles(UnityEditor.BuildTarget target)
        {
            switch (target)
            {
                case UnityEditor.BuildTarget.Android:
                    return "Android";
                case UnityEditor.BuildTarget.iOS:
                    return "iOS";
                case UnityEditor.BuildTarget.WebPlayer:
                    return "WebPlayer";
                case UnityEditor.BuildTarget.StandaloneWindows:
                case UnityEditor.BuildTarget.StandaloneWindows64:
                    return "Windows";
                case UnityEditor.BuildTarget.StandaloneOSXIntel:
                case UnityEditor.BuildTarget.StandaloneOSXIntel64:
                case UnityEditor.BuildTarget.StandaloneOSXUniversal:
                    return "OSX";

                default:
                    return "Unknown";
            }
        }
#endif
    }//AssetFilter
}