using UnityEngine;

namespace Utility
{
    public class SingletonMonoBehaviour<T> : Entity where T : Entity
    {
        private static T m_instance;

        private static object m_lock = new object();

        private static bool m_applicationIsQuitting = false;

        override protected void PreInit()
        {
            m_applicationIsQuitting = false;
        }

        public static T Instance
        {
            get
            {
                if (m_applicationIsQuitting)
                {
                    //                     Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
                    //                         "' already destroyed on application quit." +
                    //                         " Won't create again - returning null.");
                    return null;
                }

                lock (m_lock)
                {
                    if (m_instance == null)
                    {
                        m_instance = (T)FindObjectOfType(typeof(T));

                        if (FindObjectsOfType(typeof(T)).Length > 1)
                        {
                            LogManager.Instance.LogError("[Singleton] Something went really wrong " +
                                " - there should never be more than 1 singleton!" +
                                " Reopenning the scene might fix it.");
                            return m_instance;
                        }

                        if (m_instance == null)
                        {
                            GameObject singleton = new GameObject();
                            m_instance = singleton.AddComponent<T>();
                            singleton.name = "(singleton) " + typeof(T).ToString();

                            //DontDestroyOnLoad(singleton);

                            //                             Debug.Log("[Singleton] An instance of " + typeof(T) +
                            //                                 " is needed in the scene, so '" + singleton +
                            //                                 "' was created with DontDestroyOnLoad.");
                        }
                        else
                        {
                            //                             Debug.Log("[Singleton] Using instance already created: " +
                            //                                 _instance.gameObject.name);
                        }
                    }

                    return m_instance;
                }
            }
        }

        /// <summary>
        /// When Unity quits, it destroys objects in a random order.
        /// In principle, a Singleton is only destroyed when application quits.
        /// If any script calls Instance after it have been destroyed,
        ///   it will create a buggy ghost object that will stay on the Editor scene
        ///   even after stopping playing the Application. Really bad!
        /// So, this was made to be sure we're not creating that buggy ghost object.
        /// </summary>
        void OnApplicationQuit()
        {
            m_applicationIsQuitting = true;
            DestroyImmediate(gameObject);
        }
    }
}
