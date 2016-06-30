using System;

namespace mFrame.Singleton
{
    public class Singleton<T> : IDisposable where T : new()
    {
        private static T m_instance;
        static object m_lock = new object();
        public static T Instance
        {
            get
            {
                if (m_instance == null)
                {
                    lock (m_lock)
                    {
                        if (m_instance == null)
                        {

                            m_instance = new T();
                        }
                    }
                }
                return m_instance;
            }
            set { m_instance = value; }
        }

        /// <summary>
        /// 切换场景的时候会调用这个方法
        /// </summary>
        public virtual void Dispose()
        {
            Instance = (T)(object)null;
        }
    }
}
