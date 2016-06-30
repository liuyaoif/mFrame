using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Utility;

namespace Utility
{
    public class LogManager : Singleton<LogManager>
    {
        public enum LogDeviceType
        {
            Editor = 0,
            DeviceDebug = 1,
            DeviceRelease = 2,
            Count,
            Invalid,
        }

        public class LogItem
        {
            LogType type;
            string content;

            public LogItem(LogType logType, string logContent)
            {
                type = logType;
                content = logContent;
            }

            public string ToUIString()
            {
                string ret = null;
                string color = "";
                switch (type)
                {
                    case LogType.Log:
                        {
                            color = "[FFFFFF]";//white
                        }
                        break;

                    case LogType.Error:
                    case LogType.Exception:
                        {
                            color = "[FF0000]";//red
                        }
                        break;

                    case LogType.Warning:
                        {
                            color = "[FFFF00]";//yellow
                        }
                        break;

                    default:
                        {
                            color = "[FFFFFF]";//white
                        }
                        break;
                }
                ret = UtilTools.CombineString(color, content, "[-]\n");
                return ret;
            }
        }

        private bool m_isConsoleLog = true;
        private bool m_isFileLog = false;
        private bool m_isUiLog = false;
        private bool m_isEnableDeviceDebug = false;

        private const int MAX_LOG_COUNT = 50;
        private List<LogItem> m_itemList;


        private const string LOG_FILE_NAME = "MG002";
        private string m_curLogFileName = null;

        private Action m_showUILogCallBack;

        public Action showUILogCallBack
        {
            set { m_showUILogCallBack = value; }
        }

        public List<LogItem> itemList
        {
            get { return m_itemList; }
        }


        public void Init(bool isFile, bool isUI, LogDeviceType deviceType)
        {
#if UNITY_EDITOR
            m_isConsoleLog = true;
#else
        m_isConsoleLog = false;
#endif
            m_isFileLog = isFile;
            m_isUiLog = isUI;
            m_isEnableDeviceDebug = (deviceType == LogDeviceType.DeviceDebug) ? true : false;

            Application.logMessageReceivedThreaded += OnUnityLogMessage;
            m_itemList = new List<LogItem>();

            DateTime timeNow = DateTime.Now;
            string time = UtilTools.CombineString(timeNow.Year, "-",
                timeNow.Month, "-",
                timeNow.Day, "-",
                timeNow.Hour, "-",
                timeNow.Minute, "-",
                timeNow.Second);

            m_curLogFileName = UtilTools.CombineString(Application.persistentDataPath, "/",
                LOG_FILE_NAME, "-", time, ".log");

            Log("FileName: " + m_curLogFileName);
        }

        public void Log(object message)
        {
            if (m_isConsoleLog)
            {
                Debug.Log(message);
            }
            else
            {
                if (m_isFileLog)
                {
                    LogToFile(message.ToString(), "", LogType.Log);
                }

                if (m_isUiLog)
                {
                    LogToUI(message.ToString(), "", LogType.Log);
                }
            }
        }

        public void Log(params object[] strs)
        {
            if (m_isConsoleLog)
            {
                Debug.Log(UtilTools.CombineString(strs));
            }
            else
            {
                if (m_isFileLog)
                {
                    LogToFile(UtilTools.CombineString(strs), " ", LogType.Log);
                }

                if (m_isUiLog)
                {
                    LogToUI(UtilTools.CombineString(strs), " ", LogType.Log);
                }
            }
        }

        public void LogWarning(object message, Exception obj = null)
        {
            //ObjectPoolManager
            if (m_isConsoleLog)
            {
                Debug.LogWarning(message);
            }
            else
            {
                if (m_isFileLog)
                {
                    LogToFile(message.ToString(), (obj == null) ? " " : obj.ToString(), LogType.Warning);
                }

                if (m_isUiLog)
                {
                    LogToUI(message.ToString(), (obj == null) ? " " : obj.ToString(), LogType.Warning);
                }
            }
        }
        public void LogError(object message, Exception obj = null)
        {
            if (m_isConsoleLog)
            {
                Debug.LogError(message);
            }
            else
            {
                if (m_isFileLog)
                {
                    LogToFile(message.ToString(), (obj == null) ? " " : obj.ToString(), LogType.Error);
                }

                if (m_isUiLog)
                {
                    LogToUI(message.ToString(), (obj == null) ? " " : obj.ToString(), LogType.Error);
                }
            }
        }

        public void Assert(bool expression, System.Object content)
        {
#if UNITY_EDITOR
            if (!expression)
            {
                throw new Exception(content.ToString());
            }
#endif
        }

        private void OnUnityLogMessage(string condition, string stackTrace, LogType type)
        {
            //Only system exceptions.
            if (m_isFileLog)
            {
                LogToFile(condition, stackTrace, type);
            }

            if (m_isUiLog)
            {
                LogToUI(condition, stackTrace, type);
            }
        }

        private void LogToFile(string condition, string stackTrace, LogType type)
        {
            DateTime timeNow;
            string timeStr;
            string content;

            if (type == LogType.Exception ||
                type == LogType.Error)
            {
                timeNow = DateTime.Now;
                timeStr = UtilTools.CombineString(
                   timeNow.Hour, ":",
                   timeNow.Minute, ":",
                   timeNow.Second);
                content = UtilTools.CombineString(type, " ", timeStr, " ", condition, " ", stackTrace, "\n");
                WriteFile(content);
                return;
            }

            if (!m_isEnableDeviceDebug)//Device release do not write none-exception logs.
            {
                return;
            }

            timeNow = DateTime.Now;
            timeStr = UtilTools.CombineString(
                timeNow.Hour, ":",
                timeNow.Minute, ":",
                timeNow.Second);
            content = UtilTools.CombineString(type, " ", timeStr, " ", condition, " ", stackTrace, "\n");
            WriteFile(content);
        }

        private void WriteFile(string content)
        {
            StreamWriter sw;
            FileInfo t = new FileInfo(m_curLogFileName);
            if (!t.Exists)
            {
                sw = t.CreateText();
            }
            else
            {
                sw = t.AppendText();
            }
            sw.WriteLine(content);
            sw.Close();
            sw.Dispose();
        }

        private void LogToUI(string condition, string stackTrace, LogType type)
        {
            m_itemList.Add(new LogItem(type, UtilTools.CombineString(type, ": ", condition, " ", stackTrace)));

            if (m_itemList.Count > MAX_LOG_COUNT)
            {
                m_itemList.RemoveAt(0);
            }

            if (type == LogType.Exception ||
                type == LogType.Error)
            {
                if (m_showUILogCallBack != null)
                {
                    m_showUILogCallBack();
                }
            }
        }
    }
}
