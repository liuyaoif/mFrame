using mFrame.Asset;
using mFrame.Singleton;
using System.Collections.Generic;
using UnityEngine;

namespace mFrame.Sound
{
    public class SoundManager : SingletonMonoBehaviour<SoundManager>
    {
        private class SoundEffectInfo
        {
            public string m_assetName;
            public bool m_isLoop;
            public bool m_isMultiPlay;
            public AudioSource m_source;

            private bool m_isLoading;
            public bool isLoading
            {
                get { return m_isLoading; }
                private set { m_isLoading = value; }
            }

            public void Play()
            {
                if (m_source != null)
                {
                    m_source.volume = SoundManager.Instance.soundEffectVolume;
                    m_source.Play();
                }
            }

            public void Stop()
            {
                m_assetName = null;

                if (m_source != null)
                {
                    m_source.Stop();
                    m_source.clip = null;
                    m_source = null;
                }
            }

            public void LoadSoundEffect()
            {
                if (m_source == null)
                {
                    return;
                }

                AssetsManager.Instance.AddAssetTask(m_assetName + ".ogg",
                     OnSoundEffectReady);
                isLoading = true;
            }

            private void OnSoundEffectReady(UnityEngine.Object asset, object userData)
            {
                m_source.clip = null;
                m_source.clip = asset as AudioClip;
                m_source.loop = m_isLoop;
                Play();
                isLoading = false;
            }
        }

        private string m_curBgmName;
        public AudioSource m_bgmSource;
        private bool m_enableBgm = true;
        public bool enableBgm
        {
            get { return m_enableBgm; }
            set
            {
                if (m_enableBgm == value)
                {
                    return;
                }

                m_enableBgm = value;
                m_bgmSource.mute = !m_enableBgm;

                if (!m_bgmSource.isPlaying)
                {
                    PlayBgm(m_curBgmName);
                }
            }
        }

        private float m_bgmVolume = 0.5f;
        public float bgmVolume
        {
            get { return m_bgmVolume; }
            set { m_bgmVolume = value; }
        }

        private const int MAX_EFFECT_SOURCE_COUNT = 6;
        private List<SoundEffectInfo> m_soundEffectList = new List<SoundEffectInfo>();

        /// <summary>
        /// key: AudioSource
        /// value: Available
        /// </summary>
        private Dictionary<AudioSource, bool> m_audioSourceDict = new Dictionary<AudioSource, bool>();

        private bool m_enableSoundEffect = true;
        public bool enableSoundEffect
        {
            get { return m_enableSoundEffect; }
            set
            {
                if (m_enableSoundEffect == value)
                {
                    return;
                }

                m_enableSoundEffect = value;
                foreach (var kvp in m_audioSourceDict)
                {
                    kvp.Key.mute = !m_enableSoundEffect;
                }
            }
        }

        private float m_soundEffectVolume = 0.5f;
        public float soundEffectVolume
        {
            get { return m_soundEffectVolume; }
            set { m_soundEffectVolume = value; }
        }

        #region BGM
        public void PlayBgm(string bgmName)
        {
            if (m_curBgmName == bgmName)//已经在播放了，BGM不打断
            {
                return;
            }

            m_curBgmName = bgmName;

            if (!m_enableBgm)
            {
                return;
            }

            AssetsManager.Instance.AddAssetTask(bgmName + ".mp3",
                OnBgmAssetReady);
        }

        private void OnBgmAssetReady(UnityEngine.Object asset, object userData)
        {
            m_curBgmName = asset.name;

            m_bgmSource.Stop();
            m_bgmSource.clip = null;
            m_bgmSource.clip = asset as AudioClip;
            m_bgmSource.loop = true;
            m_bgmSource.volume = m_bgmVolume;
            m_bgmSource.Play();
        }
        #endregion

        #region Effect
        private void CheckIsSoundEffectPlaying()
        {
            List<SoundEffectInfo> removeIndex = new List<SoundEffectInfo>();
            for (int i = 0; i < m_soundEffectList.Count; i++)
            {
                if (m_soundEffectList[i] == null)
                {
                    continue;
                }

                if (!m_soundEffectList[i].isLoading &&
                    !m_soundEffectList[i].m_source.isPlaying ||
                    m_soundEffectList[i].m_source == null)
                {
                    removeIndex.Add(m_soundEffectList[i]);
                }
            }

            for (int i = 0; i < removeIndex.Count; i++)
            {
                m_audioSourceDict[removeIndex[i].m_source] = true;
                removeIndex[i].Stop();
                m_soundEffectList.Remove(removeIndex[i]);
            }
        }

        private AudioSource AcquireSource()
        {
            CheckIsSoundEffectPlaying();

            foreach (var kvp in m_audioSourceDict)
            {
                if (kvp.Value)//可用
                {
                    m_audioSourceDict[kvp.Key] = false;
                    return kvp.Key;
                }
            }

            if (m_audioSourceDict.Count < MAX_EFFECT_SOURCE_COUNT)
            {
                AudioSource newSource = gameObject.AddComponent<AudioSource>();
                m_audioSourceDict.Add(newSource, false);
                return newSource;
            }

            return null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="soundEffectName"></param>
        /// <param name="循环播放"></param>
        /// <param name="允许相同音效并行播放"></param>
        public void PlaySoundEffect(string soundEffectName, bool isLoop = false, bool isMultiPlay = false)
        {
            SoundEffectInfo effectInfo = null;
            for (int i = 0; i < m_soundEffectList.Count; i++)
            {
                if (m_soundEffectList[i].m_assetName == soundEffectName)
                {
                    effectInfo = m_soundEffectList[i];
                    break;
                }
            }

            //当前缓存了这个音效
            if (effectInfo != null)
            {
                if (isMultiPlay)//允许多个实例播放
                {
                    SoundEffectInfo newInfo = new SoundEffectInfo();
                    newInfo.m_assetName = soundEffectName;
                    newInfo.m_isLoop = isLoop;
                    newInfo.m_isMultiPlay = isMultiPlay;
                    newInfo.m_source = AcquireSource();
                    if (newInfo.m_source != null)
                    {
                        if (effectInfo.m_source != null &&
                            effectInfo.m_source.clip != null)
                        {
                            AudioClip clip = effectInfo.m_source.clip;
                            newInfo.m_source.clip = clip;
                            newInfo.Play();
                        }
                        else
                        {
                            newInfo.LoadSoundEffect();
                        }
                        m_soundEffectList.Add(newInfo);
                    }
                }
                else//仅单个
                {
                    effectInfo.Play();
                }
            }
            else//没有缓存音效，需要加载
            {
                SoundEffectInfo newInfo = new SoundEffectInfo();
                newInfo.m_assetName = soundEffectName;
                newInfo.m_isLoop = isLoop;
                newInfo.m_isMultiPlay = isMultiPlay;
                newInfo.m_source = AcquireSource();
                newInfo.LoadSoundEffect();
                m_soundEffectList.Add(newInfo);
            }
        }

        public void StopSoundEffect(string soundEffectName)
        {
            for (int i = 0; i < m_soundEffectList.Count; i++)
            {
                if (m_soundEffectList[i].m_assetName == soundEffectName)
                {
                    m_audioSourceDict[m_soundEffectList[i].m_source] = true;
                    m_soundEffectList[i].Stop();
                }
            }
        }

        #endregion
    }
}
