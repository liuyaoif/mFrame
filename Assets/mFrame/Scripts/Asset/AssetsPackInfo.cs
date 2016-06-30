using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;

namespace mFrame.Asset
{
    public class AssetsPackInfo
    {
        public class AssetInfo
        {
            public string name;
            public string bundleName;
            public string assetPath;
        }

        public class BundleInfo
        {
            public string name;
            //public List<string> depedenceBundles;
            public string MD5;
            public long size;//byte
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

        public static AssetInfo CreateAssetInfo(string name, string bundleName)
        {
            AssetInfo asset = new AssetInfo();
            asset.name = name;
            asset.bundleName = SpliteName(bundleName);
            return asset;
        }

        public static BundleInfo CreateBundleInfo(string name, List<string> dependence, string MD5Value)
        {
            BundleInfo bundle = new BundleInfo();
            bundle.name = SpliteName(name);

            if (dependence != null)
            {
                dependence.ForEach(str => SpliteName(str));
                //bundle.depedenceBundles = dependence;
            }

            bundle.MD5 = MD5Value;
            return bundle;
        }

        #region Write Xml
        private static XmlAttribute addXmlAttribute(XmlDocument doc, string name, string val)
        {
            XmlAttribute attr = doc.CreateAttribute(name);
            attr.Value = val;
            return attr;
        }

        private static XmlNode CreateNode(AssetInfo asset, XmlDocument doc)
        {
            XmlNode node = doc.CreateElement("AssetConfig");
            node.Attributes.Append(addXmlAttribute(doc, "AssetName", asset.name));
            node.Attributes.Append(addXmlAttribute(doc, "BundleName", asset.bundleName));
            node.Attributes.Append(addXmlAttribute(doc, "AssetPath", asset.assetPath));
            return node;
        }

        private static XmlNode CreateNode(BundleInfo bundle, XmlDocument doc)
        {
            XmlNode node = doc.CreateElement("BundleConfig");
            node.Attributes.Append(addXmlAttribute(doc, "BundleName", bundle.name));
            node.Attributes.Append(addXmlAttribute(doc, "MD5", bundle.MD5));
            node.Attributes.Append(addXmlAttribute(doc, "Size", bundle.size.ToString()));

            //             if (bundle.depedenceBundles != null)
            //             {
            //                 node.Attributes.Append(addXmlAttribute(doc, "DependenceCount", bundle.depedenceBundles.Count.ToString()));
            //                 for (int i = 0; i < bundle.depedenceBundles.Count; i++)
            //                 {
            //                     node.Attributes.Append(addXmlAttribute(doc, "DependenceBundle" + i, bundle.depedenceBundles [i]));
            //                 }
            //             }

            return node;
        }

        [ExecuteInEditMode]
        public static void WriteInfoToXMLFile(Dictionary<string, AssetInfo> assetList, Dictionary<string, BundleInfo> bundleList, string savePath)
        {
            if (null == assetList ||
                null == bundleList)
            {
                return;
            }

            XmlDocument doc = new XmlDocument();
            XmlDeclaration desc = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            doc.AppendChild(desc);

            XmlElement root = doc.CreateElement("PackConfig");
            doc.AppendChild(root);

            XmlElement assetsRoot = doc.CreateElement("Assets");
            root.AppendChild(assetsRoot);
            foreach (var kvp in assetList)
            {
                XmlNode node = CreateNode(kvp.Value, doc);
                assetsRoot.AppendChild(node);
            }

            XmlElement bundlesRoot = doc.CreateElement("Bundles");
            root.AppendChild(bundlesRoot);
            foreach (var kvp in bundleList)
            {
                XmlNode node = CreateNode(kvp.Value, doc);
                bundlesRoot.AppendChild(node);
            }

            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }

            doc.Save(savePath);
        }
        #endregion
    }//AssetsPackInfo
}