using LitJson;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Utility;

public interface IConfigData
{
    int GetId();
}

public sealed class ConfigDataManager : Singleton<ConfigDataManager>
{
    private const string configDataBundleName = "designconfig.bundle";

    private Action onAllConfigDataReady;

    private Dictionary<Type, Dictionary<int, IConfigData>> allConfigDataDict =
        new Dictionary<Type, Dictionary<int, IConfigData>>();

    public T GetConfigData<T>(int id) where T : IConfigData
    {
        if (allConfigDataDict.ContainsKey(typeof(T)))
        {
            Dictionary<int, IConfigData> configDict = allConfigDataDict[typeof(T)];

            if (configDict.ContainsKey(id))
            {
                return (T)configDict[id];
            }
            else
            {
                LogManager.Instance.LogWarning(typeof(T).ToString() + " ID: " + id.ToString() + " not exist.");
            }
        }
        return default(T);
    }

    public Dictionary<int, IConfigData> GetConfigDataDictionary(Type type)
    {
        Dictionary<int, IConfigData> configDict = null;
        allConfigDataDict.TryGetValue(type, out configDict);
        return configDict;
    }

    public void Init(Action onReady = null)
    {
        onAllConfigDataReady = onReady;
        allConfigDataDict.Clear();
        TextManager.Instance.Init();

#if UNITY_EDITOR
        if (GlobalConfigManager.Instance.isUseBundle)
        {
            AssetsManagerFor5.Instance.AddBundleTask(configDataBundleName, OnConfigLoaded, OnError);
        }
        else
        {
            EditorLoaded();
        }
#else
            AssetsManagerFor5.Instance.AddBundleTask(configDataBundleName, OnConfigLoaded, OnError);
#endif
    }

#if UNITY_EDITOR
    private void EditorLoaded()
    {
        string[] fileEntries = System.IO.Directory.GetFiles(Application.dataPath + "/RawBundles/DesignConfig/", "*.json",
            System.IO.SearchOption.TopDirectoryOnly);

        for (int i = 0; i < fileEntries.Length; i++)
        {
            int statrID = fileEntries[i].IndexOf("Assets");
            string path = fileEntries[i].Substring(statrID);
            //ParseJson(UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(path));

            TextAsset textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            ParseJsonByUtility(textAsset);
            Resources.UnloadAsset(textAsset);
        }

        Ready();
    }
#endif

    private void ParseJson(TextAsset textAsset)
    {
        JsonData rootNode = JsonMapper.ToObject(textAsset.text);

        Dictionary<int, IConfigData> singleConfigDict = new Dictionary<int, IConfigData>();

        Type configDataType = Type.GetType(UtilTools.CombineString("Excel2Json.", textAsset.name));
        PropertyInfo[] propertyInfo = configDataType.GetProperties();

        //Debug.Log(textAsset.name);
        //Every line.
        for (int lineIdx = 0; lineIdx < rootNode.Count; lineIdx++)
        {
            System.Object configDataInstance = Activator.CreateInstance(configDataType);
            JsonData curLineJson = rootNode[lineIdx];

            //Every property
            for (int propIdx = 0; propIdx < propertyInfo.Length; propIdx++)
            {
                string propName = propertyInfo[propIdx].ToString().Split(" ".ToCharArray())[1];
                try
                {
                    if (curLineJson == null || propName == null)
                    {
                        Debug.Log("propName" + propName);
                    }
                    Type type = propertyInfo[propIdx].PropertyType;
                    string data = null;
                    if (type == typeof(string) || type.IsArray)
                    {
                        JsonData json =curLineJson[propName];
                        data = json.ToJson();
                    }
                    else
                    {
                        data = curLineJson[propName].ToString();
                    }

                    if (!String.IsNullOrEmpty(data))
                    {
                        try
                        {
                            if (propertyInfo[propIdx].PropertyType == typeof(bool))
                            {
                                if (int.Parse(data) == 0)
                                {
                                    propertyInfo[propIdx].SetValue(configDataInstance, false, null);
                                }
                                else
                                {
                                    propertyInfo[propIdx].SetValue(configDataInstance, true, null);
                                }
                            }
                            else if (propertyInfo[propIdx].PropertyType.IsArray)
                            {
                                string[] strArr = data.Split(";".ToCharArray());
                                Type elementType = propertyInfo[propIdx].PropertyType.GetElementType();
                                Array arr = Array.CreateInstance(elementType, strArr.Length);
                                for (int i = 0; i < strArr.Length; i++)
                                {
                                    arr.SetValue(Convert.ChangeType(strArr[i], elementType), i);
                                }
                                propertyInfo[propIdx].SetValue(configDataInstance, arr, null);
                            }
                            else
                            {
                                object obj = Convert.ChangeType(data, propertyInfo[propIdx].PropertyType);
                                if (obj != null)
                                {
                                    propertyInfo[propIdx].SetValue(configDataInstance, obj, null);
                                }
                            }
                        }
                        catch (Exception exp)
                        {
                            LogManager.Instance.LogError(textAsset.name + "    " + configDataType.ToString() + " " + propertyInfo[propIdx].ToString()
                                + "    " + data +
                                exp.ToString());
                            Application.Quit();
                        }
                    }
                    else
                    {
                    }
                }
                catch (Exception e)
                {
                    LogManager.Instance.LogError(textAsset.name + "    " + propName + e);
                }

            }//Every property

            string idproperty = (configDataInstance as IConfigData).GetId().ToString();
            //Debug.Log(idproperty + configDataInstance);
            singleConfigDict.Add(int.Parse(idproperty), configDataInstance as IConfigData);
            if (!allConfigDataDict.ContainsKey(configDataType))
            {
                allConfigDataDict.Add(configDataType, singleConfigDict);
            }
        }
    }

    private void ParseJsonByUtility(TextAsset textAsset)
    {
        JsonData rootNode = JsonMapper.ToObject(textAsset.text);
        Dictionary<int, IConfigData> singleConfigDict = new Dictionary<int, IConfigData>();
        Type configDataType = null;// ConfigMapper.GetMapperDict()[textAsset.name];

        //Every line.
        for (int lineIdx = 0; lineIdx < rootNode.Count; lineIdx++)
        {
            JsonData curLineJson = rootNode[lineIdx];
            object instance = JsonUtility.FromJson(curLineJson.ToJson(), configDataType);

            IConfigData configData = instance as IConfigData;
            singleConfigDict.Add(configData.GetId(), configData);
            if (!allConfigDataDict.ContainsKey(configDataType))
            {
                allConfigDataDict.Add(configDataType, singleConfigDict);
            }
        }
    }

    private void OnConfigLoaded(AssetBundle bundle)
    {

        try
        {
            UnityEngine.Object[] objs = bundle.LoadAllAssets();

            //Every config files.
            for (int confIdx = 0; confIdx < objs.Length; confIdx++)
            {
                TextAsset textAsset = objs[confIdx] as TextAsset;
                //ParseJson(textAsset);
                ParseJsonByUtility(textAsset);
            } //Every config files.

            bundle.Unload(true);
            Ready();
        }
        catch (Exception exp)
        {
            LogManager.Instance.LogError(exp.ToString() + "Application will quit.");
            Application.Quit();
        }

    }

    private void Ready()
    {
        if (onAllConfigDataReady != null)
        {
            onAllConfigDataReady();
            onAllConfigDataReady = null;
            EventManager.Instance.DispatchEvent(EventID.ConfigDataReady, null);
        }
    }
    private void OnError(string error)
    {
        Ready();
    }

    private static bool IsType(Type type, string typeName)
    {
        if (type.ToString() == typeName)
            return true;
        if (type.ToString() == "System.Object")
            return false;

        return IsType(type.BaseType, typeName);
    }
}
