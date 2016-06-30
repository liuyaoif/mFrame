using UnityEngine;
using System.Collections;
using Utility;
public class CurrMainCamera : SingletonMonoBehaviour<CurrMainCamera>
{
    public static Camera mainCamera;
    public Camera came { get { return myCame; } }
    private Camera myCame;

    void Awake()
    {
        myCame = GetComponent<Camera>();
        mainCamera = myCame;
        //Camera.SetupCurrent(myCame);
    }
}
