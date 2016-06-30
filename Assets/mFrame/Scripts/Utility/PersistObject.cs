using UnityEngine;

namespace mFrame.Utility
{
    public class PersistObject : MonoBehaviour
    {
        void Awake()
        {
            UtilTools.SetDontDestroyOnLoad(gameObject);
        }
    }
}
