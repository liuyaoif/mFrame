using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Utility
{
    public class UtilTools
    {
        public static List<GameObject> allDontDestroyOnLoad = new List<GameObject>();
        private static int m_seed = 0;

        public static int GenerateSeed()
        {
            m_seed++;
            return m_seed;
        }

        public static float FrameToDuration(int frame)
        {
            return frame * Time.fixedDeltaTime;
        }

        public static void SetDontDestroyOnLoad(GameObject go)
        {
            if (allDontDestroyOnLoad.Contains(go))
            {
                return;
            }
            GameObject.DontDestroyOnLoad(go);
            allDontDestroyOnLoad.Add(go);
        }

        public static int DurationToFrame(float duration)
        {
            return (int)(duration / Time.fixedDeltaTime);
        }

        public static GameObject Instantiate(GameObject prefab, Transform parent = null, bool isActive = true)
        {
            GameObject ret = GameObject.Instantiate(prefab) as GameObject;
            ret.SetActive(isActive);
            if (parent != null)
            {
                ret.gameObject.transform.SetParent(parent);
                //ret.transform.lo
                ret.transform.localScale = Vector3.one;
            }

            return ret;
        }

        public static void DestroyGameObject(GameObject go, bool isImmediate = false)
        {
            if (go != null)
            {
                if (!isImmediate)
                {
                    GameObject.Destroy(go);
                }
                else
                {
                    GameObject.DestroyImmediate(go);
                }
            }
        }

        public static string CombineString(params string[] strs)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < strs.Length; i++)
            {
                sb.Append(strs[i]);
            }
            return sb.ToString();
        }

        public static string TimeLabel(int time)
        {

            int fen = time / 60;
            int miao = time - (fen * 60);
            int xiaoShi = fen / 60;
            fen = fen - (xiaoShi * 60);
            return string.Format("{0}:{1}:{2}", xiaoShi.ToString("00"), fen.ToString("00"), miao.ToString("00"));
        }

        public static string CombineString(params object[] strs)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < strs.Length; i++)
            {
                sb.Append(strs[i].ToString());
            }
            return sb.ToString();
        }
        /// <summary>
        /// 正则匹配
        /// </summary>
        /// <param name="str"></param>
        /// <param name="data">"mydata|mydata|"</param>
        public static void MatchingMouna(ref string str, string data)
        {

            MatchCollection m = Regex.Matches(str, "(" + data + ")", RegexOptions.Multiline);
            for (int i = 0; i < m.Count; i++)
            {

                string m1 = Regex.Replace(m[i].ToString(), "\\$", "\\$");
                m1 = Regex.Replace(m1, "\\^", "\\^");
                string xingxing = "";
                for (int j = 0; j < m1.Length; j++)
                    xingxing += "*";
                str = Regex.Replace(str, m1, xingxing);
            }
            LogManager.Instance.Log(str + "   " + data);
        }
        /// <summary>
        /// 设置对象的层
        /// </summary>
        /// <param name="layerName"></param>
        /// <param name="obj"></param>
        /// <param name="isChild"></param>
        public static void SetGameObjctLayer(string layerName, GameObject obj, bool isChild = true)
        {
            int layerIdx = LayerMask.NameToLayer(layerName);
            if (isChild)
                SetChildLayer(layerIdx, obj.transform);
            else
            {
                obj.layer = layerIdx;
            }
        }
        /// <summary>
        /// 设置对象的层
        /// </summary>
        /// <param name="layerName"></param>
        /// <param name="obj"></param>
        /// <param name="isChild"></param>
        public static void SetGameObjctLayer(int layer, GameObject obj, bool isChild = true)
        {
            //int layerIdx = LayerMask.NameToLayer(layerName);
            if (isChild)
                SetChildLayer(layer, obj.transform);
            else
            {
                obj.layer = layer;
            }
        }
        private static void SetChildLayer(int layerIdx, Transform obj)
        {
            obj.gameObject.layer = layerIdx;
            for (int i = 0; i < obj.childCount; i++)
            {
                SetChildLayer(layerIdx, obj.GetChild(i));
            }
        }

        public static IPEndPoint CombineIPAddress(string host, int port)
        {
            return new IPEndPoint(IPAddress.Parse(host), port);
        }

        public static bool IsStringLetterAndNumber(string content)
        {
            if (content.Length == 0)
            {
                return false;
            }
            int asciiCount = Regex.Matches(content, "[a-zA-Z0-9]").Count;
            return (content.Length > asciiCount) ? false : true;
        }

        public static bool IsArrayValid(object array)
        {
            if (array == null || !(array is Array) || (array as Array).Length == 0)
            {
                return false;
            }
            return true;
        }

        private static string m_bannedWorlds;
        public static bool IsBannedWord(string rolename)
        {
            //if (string.IsNullOrEmpty(m_bannedWorlds))
            //{
            //    Dictionary<int, IConfigData> gameBanWord = ConfigDataManager.Instance.GetConfigDataDictionary(typeof(Excel2Json.BanWordListConfig));
            //    foreach (var key in gameBanWord)
            //    {
            //        Excel2Json.BanWordListConfig data = (Excel2Json.BanWordListConfig)key.Value;
            //        m_bannedWorlds += "|" + data.banWord;
            //    }

            //    m_bannedWorlds = m_bannedWorlds.Substring(1);
            //    m_bannedWorlds = m_bannedWorlds.ToLower();
            //}

            rolename = rolename.Replace(" ", "");
            MatchCollection chinesStr = Regex.Matches(rolename, @"[\u4e00-\u9fa5]");
            MatchCollection englistStr = Regex.Matches(rolename, @"[a-zA-Z]*");

            string tempChinese = "";
            for (int i = 0; i < chinesStr.Count; i++)
            {
                tempChinese += chinesStr[i].ToString();
            }

            string tempEnglish = "";
            for (int i = 0; i < englistStr.Count; i++)
            {
                tempEnglish += englistStr[i].ToString();
            }
            tempEnglish = tempEnglish.ToLower();

            if (!Regex.IsMatch(tempChinese, m_bannedWorlds) && !Regex.IsMatch(tempEnglish, m_bannedWorlds))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }//UtilTools
}