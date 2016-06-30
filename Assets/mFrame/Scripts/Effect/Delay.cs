using UnityEngine;

namespace mFrame.Effect
{
    public class Delay : MonoBehaviour
    {
        public float delayTime = 1.0f;
        private ParticleSystem[] m_psArr;

        void Awake()
        {
            m_psArr = transform.GetComponentsInChildren<ParticleSystem>();
            for (int i = 0; i < m_psArr.Length; i++)
            {
                m_psArr[i].startDelay = delayTime;
            }
        }
    }
}
