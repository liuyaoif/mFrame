using mFrame.Asset;
using mFrame.Log;
using mFrame.Singleton;
using mFrame.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace mFrame.Pool
{
    public interface IPoolObject
    {
        string poolObjName { get; }

        bool isUsing { get; set; }

        int PrefabHash { get; set; }

        Transform transform { get; set; }

        /// <summary>
        /// 重置
        /// </summary>
        void Reset();

        /// <summary>
        /// 激活对象
        /// </summary>
        void OnEnable();

        /// <summary>
        /// 休眠
        /// </summary>
        void OnDisable();

        /// <summary>
        /// 销毁
        /// </summary>
        void OnDestroy();
    }

    //*---------------------------------------------
    //ObjectPool can only manage GameObject with Entity component.
    //---------------------------------------------
    public sealed class ObjectPoolManager : Singleton<ObjectPoolManager>
    {
        private class ObjectPool
        {
            /// <summary>
            /// 池的最小值。如果归还时个数超出最小值，则不会加入List，直接删除。
            /// </summary>
            public int minCount;

            /// <summary>
            /// 池得最大值。如果创建时没有超出最大值，则创建，否则不创建。
            /// </summary>
            public int maxCount;

            List<IPoolObject> obj = new List<IPoolObject>();

            public ObjectPool(int min, int max = int.MaxValue)
            {
                minCount = min;
                maxCount = max;
            }

            public int GetAvailableCount()
            {
                int ret = 0;
                for (int i = 0; i < obj.Count; i++)
                {
                    if (!obj[i].isUsing)
                    {
                        ret++;
                    }
                }
                return ret + maxCount - obj.Count;
            }

            public bool Add(IPoolObject ir)
            {
                if (obj.Count >= maxCount)
                {
                    return false;
                }

                obj.Add(ir);
                return true;
            }

            public IPoolObject GetObject()
            {
                for (int i = 0; i < obj.Count; i++)
                {
                    if (!obj[i].isUsing)
                    {
                        obj[i].Reset();
                        obj[i].OnEnable();
                        obj[i].isUsing = true;
                        return obj[i];
                    }
                }
                return null;
            }

            /// <summary>
            /// 关闭一个池的对象
            /// </summary>
            /// <param name="ir"></param>
            public void CloseObj(IPoolObject ir)
            {
                if (obj.Count > maxCount)
                {
                    if (ir != null)
                        ir.OnDestroy();
                    obj.Remove(ir);
                    return;
                }

                ir.transform.gameObject.SetActive(false);
                ir.OnDisable();
                ir.isUsing = false;
            }

            public void CloseObj(int hashId)
            {
                for (int i = 0; i < obj.Count; i++)
                {
                    if (obj[i].PrefabHash == hashId)
                    {
                        CloseObj(obj[i]);
                    }
                }
            }
            public void DormancyAllObj()
            {
                for (int i = 0; i < obj.Count; i++)
                {

                    CloseObj(obj[i]);

                }
            }
            public void Clear()
            {
                for (int i = 0; i < obj.Count; i++)
                {
                    if (obj[i] != null)
                    {
                        obj[i].OnDestroy();
                    }
                }
                obj.Clear();
            }
        }

        //Key: prefab's hash code.
        //Value: Queue of GameObjects that created from the same prefab.
        private Dictionary<string, ObjectPool> m_poolDict = new Dictionary<string, ObjectPool>();
        /// <summary>
        /// 预加载对象池
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public IEnumerator PreCreate(GameObject prefab, bool isCreate, int minCount = 1, int maxCount = int.MaxValue, Transform parent = null)
        {
            if (prefab == null)
            {
                yield break;
            }
            LogManager.Instance.Log(prefab.name);

            ObjectPool objList;
            m_poolDict.TryGetValue(prefab.name, out objList);

            if (null == objList)//No object pool for this prefab.
            {
                objList = new ObjectPool(minCount, maxCount);
                m_poolDict.Add(prefab.name, objList);
            }
            if (isCreate)
            {
                for (int i = 0; i < minCount; i++)
                {
                    IPoolObject ir = CreateInstance(prefab, parent, false);
                    PreCreateObj(prefab.name, ir.transform.gameObject, objList, parent);
                    yield return 0;
                }
            }
        }
        /// <summary>
        /// 预加载对象池
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public void PreCreate(string prefabName, bool isCreate, int maxCount = int.MaxValue, Transform parent = null, bool isResource = false)
        {
            ObjectPool objList;
            m_poolDict.TryGetValue(prefabName, out objList);

            if (null == objList)//No object pool for this prefab.
            {
                objList = new ObjectPool(1, maxCount);
                m_poolDict.Add(prefabName, objList);
            }
            if (isCreate)
            {
                if (!isResource)
                {
                    AssetsManager.Instance.AddAssetTask(prefabName + ".prefab", delegate(UnityEngine.Object obj, object data)
                    {
                        PreCreateObj(prefabName, obj, objList, parent);
                    }).allowMultiPrefabInstances = true;
                }
                else
                {
                    try
                    {
                        AssetsManager.Instance.AddAssetTaskForAssetPath(prefabName + ".prefab", delegate(UnityEngine.Object obj, object data)
                        {
                            PreCreateObj(prefabName, obj, objList, parent);
                        }).allowMultiPrefabInstances = true;

                    }
                    catch (Exception e)
                    {
                        LogManager.Instance.LogError("没有找到预制件" + prefabName);
                        //callBack(null);
                    }
                }
            }

        }
        private void PreCreateObj(string prefabName, UnityEngine.Object obj, ObjectPool objList, Transform parent)
        {
            GameObject go = obj as GameObject;
            if (null != parent)
            {
                go.transform.parent = parent;
            }
            go.name = prefabName;
            IPoolObject ir = go.GetComponent<IPoolObject>();
            objList.Add(ir);
            ir.isUsing = false;
            go.SetActive(false);
        }

        private IPoolObject CreateInstance(GameObject prefab, Transform parent, bool isActive)
        {
            GameObject retObj = UtilTools.Instantiate(prefab, parent, isActive);//GameObject.Instantiate(prefab) as GameObject;
            retObj.name = prefab.name;

            IPoolObject ir = retObj.GetComponent<IPoolObject>();

            try
            {
                ir.transform = retObj.transform;
                ir.PrefabHash = prefab.GetHashCode();
                retObj.SetActive(isActive);
                if (null != parent)
                {
                    retObj.transform.parent = parent;
                }
                return ir;
            }
            catch (Exception exp)
            {
                retObj.SetActive(false);
                LogManager.Instance.LogError(prefab.name + exp.ToString());
            }
            return null;
        }

        /// <summary>
        /// Get an instance of prefab.
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="parent"></param>
        /// <param name="forceNew"></param>
        /// <param name="isActive"></param>
        /// <returns></returns>
        public IPoolObject Acquire(GameObject prefab, Transform parent = null)
        {
            ObjectPool objList = null;
            IPoolObject ret = GetPoolObject(prefab.name, ref objList);
            if (ret != null)
            {
                if (null != parent)
                {
                    ret.transform.parent = parent;
                }
                ret.transform.gameObject.SetActive(true);
                return ret;
            }

            //No available object.
            if (objList.GetAvailableCount() > 1)
            {
                IPoolObject ir = CreateInstance(prefab, parent, true);
                AddObjToPool(ir.transform.gameObject, objList);
                return ir;
            }
            return null;
        }

        public void Acquire(string prefabName, Action<IPoolObject> callBack, Transform parent = null, bool isResource = false)
        {
            //int hash = prefab.GetHashCode();

            ObjectPool objList = null;
            IPoolObject ret = GetPoolObject(prefabName, ref objList);
            if (ret != null)
            {
                callBack(ret);
                return;
            }

            //No available object.
            if (objList.GetAvailableCount() > 1)
            {
                if (!isResource)
                {
                    AssetsManager.Instance.AddAssetTask(prefabName + ".prefab", delegate(UnityEngine.Object obj, object data)
                    {
                        GameObject go = obj as GameObject;
                        if (null != parent)
                        {
                            go.transform.parent = parent;
                        }
                        go.name = prefabName;
                        AddObjToPool(go, objList);
                        ret = go.GetComponent<IPoolObject>();
                        callBack(ret);
                    });
                }
                else
                {
                    try
                    {
                        AssetsManager.Instance.AddAssetTaskForAssetPath(prefabName + ".prefab", delegate(UnityEngine.Object obj, object data)
                        {
                            GameObject go = obj as GameObject;
                            if (null != parent)
                            {
                                go.transform.parent = parent;
                            }
                            go.name = prefabName;
                            AddObjToPool(go, objList);
                            ret = go.GetComponent<IPoolObject>();
                            ret.PrefabHash = go.GetHashCode();
                            callBack(ret);
                        }).allowMultiPrefabInstances = true;

                    }
                    catch (Exception e)
                    {
                        LogManager.Instance.LogError("没有找到预制件" + prefabName);
                        callBack(null);
                    }
                }
            }
        }

        private IPoolObject GetPoolObject(string prefabName, ref ObjectPool objList)
        {
            //ObjectPool objList;
            m_poolDict.TryGetValue(prefabName, out objList);
            if (null == objList)//No object pool for this prefab.
            {
                objList = new ObjectPool(1, 30);
                m_poolDict.Add(prefabName, objList);
                return null;
            }

            IPoolObject ret = objList.GetObject();
            if (ret != null)
            {
                return ret;
            }
            return null;
        }

        private void AddObjToPool(GameObject go, ObjectPool objList)
        {
            IPoolObject ir = go.GetComponent<IPoolObject>();
            if (ir == null)
            {
                LogManager.Instance.LogWarning("该对象不是池对象");
            }
            ir.OnEnable();
            objList.Add(ir);
            ir.isUsing = true;
        }

        public void ClosePool(string prefabName)
        {
            ObjectPool poolObj;
            m_poolDict.TryGetValue(prefabName, out poolObj);
            if (poolObj != null)
            {
                poolObj.Clear();
            }
        }
        public void DormancyPool(string prefabName)
        {
            ObjectPool poolObj;
            m_poolDict.TryGetValue(prefabName, out poolObj);
            if (poolObj != null)
            {
                poolObj.DormancyAllObj();
            }
        }
        /// <summary>
        /// 情况全部对象池,如果有需要，在切场景的时候记到调用
        /// </summary>
        public void CloseAllPools()
        {
            foreach (KeyValuePair<string, ObjectPool> kvp in m_poolDict)
            {
                kvp.Value.Clear();
            }

            m_poolDict.Clear();
        }

        public bool Recycle(string poolObjName, int hashId)
        {
            ObjectPool objList;
            m_poolDict.TryGetValue(poolObjName, out objList);
            if (null == objList)
            {
                return false;
            }
            objList.CloseObj(hashId);
            return true;
        }

        public bool Recycle(IPoolObject go)
        {
            return Recycle(go.poolObjName, go.PrefabHash);
        }//Recycle

    }
}