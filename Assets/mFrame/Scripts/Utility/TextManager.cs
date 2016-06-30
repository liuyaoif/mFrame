using System;
using System.Collections.Generic;
using System.Text;
using Utility;

public class TextManager : Singleton<TextManager>
{
    public enum Language
    {
        ChnSimp,
        ChnTrad,
        English,
        Count,
        Invalid,
    }

    private Language m_curLanguage = Language.ChnSimp;
    public Language CurLanguage
    {
        get { return m_curLanguage; }
        set { m_curLanguage = value; }
    }

    private struct TextContent
    {
        public int id;
        public string simpChn;
        public string tradChn;
        public string english;
    }

    private const int MAX_TEXT_FILE_COUNT = 6;

    private Dictionary<int, TextContent> m_textContentDict = new Dictionary<int, TextContent>();

    /// <summary>
    /// 是否启用文本适配器
    /// 启用后，所有的UILabelAdapter会替换
    /// UILabel的文本内容
    /// </summary>
    private bool m_enableAdapter = false;

    public bool enableAdapter
    {
        get { return m_enableAdapter; }
    }

    public void Init()
    {
        EventManager.Instance.AddEventListener(EventID.ConfigDataReady, OnConfigDataReady);
    }

    public void OnConfigDataReady(params object[] param)
    {
        EventManager.Instance.RemoveEventListener(EventID.ConfigDataReady, OnConfigDataReady);
        for (int i = 0; i < MAX_TEXT_FILE_COUNT; i++)
        {
            Type configDataType = Type.GetType(UtilTools.CombineString("Excel2Json.", "TextConfig", i));
            Dictionary<int, IConfigData> dict = ConfigDataManager.Instance.GetConfigDataDictionary(configDataType);

            if (dict != null)
            {
                foreach (var kvp in dict)
                {
                    TextContent content = new TextContent();
                    content.id = kvp.Value.GetId();

                    try
                    {
                        content.simpChn = configDataType.GetField("ChnSimp").GetValue(kvp.Value) as string;
                        content.tradChn = configDataType.GetField("ChnTrad").GetValue(kvp.Value) as string;
                        content.english = configDataType.GetField("English").GetValue(kvp.Value) as string;
                        if (!m_textContentDict.ContainsKey(content.id))
                        {
                            m_textContentDict.Add(content.id, content);
                        }
                        else
                        {
                            LogManager.Instance.LogError("Text 表中有ID相同的，重复ID：" + content.id);
                        }

                    }
                    catch (Exception exp)
                    {
                        LogManager.Instance.LogError(content.id + exp.StackTrace);
                    }
                }
            }
        }
    }

    public string GetContent(int id)
    {
        if (!m_textContentDict.ContainsKey(id))
        {
            return "[ff00ff]Text" + id.ToString() + "[-]";
        }

        TextContent content;
        m_textContentDict.TryGetValue(id, out content);

        switch (CurLanguage)
        {
            case Language.ChnSimp:
                {
                    return content.simpChn;
                }

            case Language.ChnTrad:
                {
                    return content.tradChn;
                }

            case Language.English:
                {
                    return content.english;
                }

            default:
                {
                    return content.english;
                }
        }
    }

    public string GetContent(bool isMultiLine, params int[] idArray)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < idArray.Length; i++)
        {
            string content = GetContent(idArray[i]);
            content += isMultiLine ? "\n" : "";
            sb.Append(content);
        }
        return sb.ToString();
    }

    /// <summary>
    /// 获取并替换
    /// </summary>
    /// <param name="id"></param>
    /// <param name="replace"></param>
    /// <returns></returns>
    public string GetContent(int id, params object[] replace)
    {
        return string.Format(GetContent(id), replace);
    }
}