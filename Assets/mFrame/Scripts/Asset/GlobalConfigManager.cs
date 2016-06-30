using mFrame.Singleton;
using System.Collections.Generic;
using System.Net;

public class GlobalConfigManager : Singleton<GlobalConfigManager>
{
    public class VirtualServerConfig
    {
        public string id;
        public List<IPEndPoint> links;
    }

    public const string CONFIG_FILE_NAME = "GlobalConfig.text";
    private string m_cdnRoot;
    private string m_clientVersion;
    private Dictionary<string, IPEndPoint> m_loginDict;
    public Dictionary<string, IPEndPoint> loginDict
    {
        get { return m_loginDict; }
        set { m_loginDict = value; }
    }

    private Dictionary<string, VirtualServerConfig> m_virtualServerDict;

    public Dictionary<string, VirtualServerConfig> virtualServerDict
    {
        get { return m_virtualServerDict; }
    }

    public string ClientVersion
    {
        get { return m_clientVersion; }
    }

    public string CdnRoot
    {
        get { return m_cdnRoot; }
    }

    private bool m_isEnableOffLine = true;
    public bool isEnableOffLine
    {
        get { return m_isEnableOffLine; }
    }
    private bool m_isUseBundle;
    public bool isUseBundle
    {
        get { return m_isUseBundle; }
    }
    public void onConfigLoaded(string content)
    {      
        /*
        try
        {
            XMLParser xmlParser = new XMLParser();
            XMLNode rootNode = null;
            try
            {
                rootNode = xmlParser.Parse(content);
            }
            catch (Exception exp)
            {
                Debug.LogError(exp);
                return;
            }
            XMLNode wxNode = rootNode.GetNode("WuxiaRPG>0");

            //Version, CDN
            XMLNode node = wxNode.GetNode("Version>0");
            m_clientVersion = node.GetValue("@value");

            node = wxNode.GetNode("CDNRootURL>0");
            m_cdnRoot = node.GetValue("@url");

            //Login
            m_loginDict = new Dictionary<string, IPEndPoint>();
            XMLNodeList loginList = wxNode.GetNodeList("LoginServer>0>Login");
            while (loginList.Count != 0)
            {
                XMLNode loginNode = loginList.Pop();
                m_loginDict.Add(loginNode.GetValue("@id"),
                    new IPEndPoint(IPAddress.Parse(loginNode.GetValue("@host")),
                    int.Parse(loginNode.GetValue("@port"))));
            }

            //Virtual server
            m_virtualServerDict = new Dictionary<string, VirtualServerConfig>();
            XMLNodeList vsConfigList = wxNode.GetNodeList("VirtualServers>0>VirtualServer");
            while (vsConfigList.Count != 0)
            {
                XMLNode vsConfigNode = vsConfigList.Pop();
                VirtualServerConfig config = new VirtualServerConfig();
                config.id = vsConfigNode.GetValue("@ID");
                config.links = new List<IPEndPoint>();
                XMLNodeList linkList = vsConfigNode.GetNodeList("Link");
                while (linkList.Count != 0)
                {
                    XMLNode linkNode = linkList.Pop();
                    config.links.Add(new IPEndPoint(IPAddress.Parse(linkNode.GetValue("@host")),
                    int.Parse(linkNode.GetValue("@port"))));
                }
                m_virtualServerDict.Add(config.id, config);
            }

            //Lua
            //XMLNode luaNode = wxNode.GetNode("DebugConfig>0>LuaDebug>0");

            //OffLine
            m_isEnableOffLine = Boolean.Parse(wxNode.GetNode("DebugConfig>0>OffLine>0").GetValue("@value"));

            //UserBundle
#if UNITY_EDITOR
            m_isUseBundle = Boolean.Parse(wxNode.GetNode("DebugConfig>0>UseBundle>0").GetValue("@value"));
#else
            m_isUseBundle = true;
#endif

            //Log
            XMLNode logTypeNode = wxNode.GetNode("DebugConfig>0>LogDeviceType>0");
            LogManager.LogDeviceType selectType = (LogManager.LogDeviceType)Enum.Parse(typeof(LogManager.LogDeviceType),
                 logTypeNode.GetValue("@value"));

            XMLNodeList LogList = wxNode.GetNodeList("DebugConfig>0>LogConfig");
            while (LogList.Count != 0)
            {
                XMLNode logConfig = LogList.Pop();
                LogManager.LogDeviceType curType = (LogManager.LogDeviceType)Enum.Parse(typeof(LogManager.LogDeviceType),
                    logConfig.GetValue("@DeviceType"));
                if (selectType == curType)
                {
                    bool isFile = Boolean.Parse(logConfig.GetValue("@File"));
                    bool isUI = Boolean.Parse(logConfig.GetValue("@UI"));
                    LogManager.Instance.Init(isFile, isUI, curType);
                    return;
                }
            }
        }
        catch (Exception exp)
        {
            LogManager.Instance.LogError(exp.ToString());
            Application.Quit();
        }
        */
    }
}
