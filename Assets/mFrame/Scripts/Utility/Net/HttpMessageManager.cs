using LitJson;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Networking;

namespace Utility
{
    public enum HttpResponseCode
    {
        SUCCESSED = 1,      // 登录成功
        ACCOUNT_ERROR = 2,  // 账号错误
        INTERNAL_ERROR = 3,     // 服务器内部错误
        AUTH_ERROR = 4,         // 身份认证错误
        PLATFORM_ERROR = 5, 	// 未知平台
    }

    public class HttpPostRequest
    {
        public string url;
        public Dictionary<string, string> formDict;
        public JsonData json;
        public HttpMessageManager.OnPostCallBack onPost;
        public Action onTimeOut;
    }

    public class HttpMessageManager : SingletonMonoBehaviour<HttpMessageManager>
    {
        public delegate void OnPostCallBack(string param);

        //private UnityWebRequest curReq;
        private WWW curWWW;
        private HttpPostRequest curRequest;
        private Queue<HttpPostRequest> requestQueue = new Queue<HttpPostRequest>();

        public void Post(string url, Dictionary<string, string> formDict, OnPostCallBack callBack, Action timeOut = null)
        {
            HttpPostRequest request = new HttpPostRequest();
            request.url = url;
            request.onPost = callBack;
            request.onTimeOut = timeOut;
            request.formDict = formDict;
            Post(request);
        }

        public void Post(HttpPostRequest request)
        {
            if (request != null)
            {
                requestQueue.Enqueue(request);
            }
        }

        protected override void LogicUpdate()
        {
            if (curWWW == null)//WWW available.
            {
                if (requestQueue.Count == 0)
                {
                    return;
                }

                curRequest = requestQueue.Dequeue();

                if (curRequest.formDict != null)
                {
                    WWWForm form = new WWWForm();
                    foreach (KeyValuePair<string, string> kvp in curRequest.formDict)
                    {
                        form.headers.Add(kvp.Key, kvp.Value);
                    }
                    curWWW = new WWW("http://" + curRequest.url, form);
                    LogManager.Instance.Log("WWW url: ", curWWW.url);
                }
                else if (curRequest.json != null)
                {
                    string content = curRequest.json.ToJson();
                    byte[] btyes = System.Text.Encoding.ASCII.GetBytes(content);
                    curWWW = new WWW("http://" + curRequest.url, btyes);
                }
            }
            else if (curWWW.isDone)//Is posting.
            {
                if (curWWW.error != null)//Http error. Time out.
                {
                    LogManager.Instance.LogWarning("WWW error: " + curWWW.error);
                    if (curRequest.onTimeOut != null)
                    {
                        curRequest.onTimeOut();
                    }
                    else
                    {
                        LogManager.Instance.LogWarning("Http post no response.");
                    }

                    DisposeRequest();
                }
                else//Http success.
                {
                    curRequest.onPost(curWWW.text);

                    DisposeRequest();
                }
            }
        }

        private void DisposeRequest()
        {
            if (curWWW != null)
            {
                curWWW.Dispose();
                curWWW = null;
            }
            curRequest = null;
        }
    }
}