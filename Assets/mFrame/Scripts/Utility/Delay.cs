using UnityEngine;

public class Delay : MonoBehaviour
{
    public float delayTime = 1.0f;
    private ParticleSystem[] par;

    void Awake()
    {
        par = transform.GetComponentsInChildren<ParticleSystem>();
        for (int i = 0; i < par.Length; i++)
        {
            par[i].startDelay = delayTime;
        }
    }
}
