using UnityEngine;
using System.Collections;
using Utility;

public class PersistObject : MonoBehaviour
{
    void Awake()
    {
        UtilTools.SetDontDestroyOnLoad(gameObject);
        //DontDestroyOnLoad(gameObject);
    }
}
