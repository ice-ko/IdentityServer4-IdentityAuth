using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;

namespace Components.Tools
{
    /// <summary>
    /// http请求
    /// </summary>
    public class Api
    {
        //请求url
        private string _baseUrl = "http://127.0.0.1";


        public Api(string baseUrl)
        {
            _baseUrl = baseUrl;
        }
        /// <summary>
        /// post自定义参数(参数放在url中)
        /// <typeparam name="T">返回接收类</typeparam>
        /// <param name="args">自定义参数</param>
        /// <returns></returns>
        public T ExecutePost<T>(Dictionary<string, Object> args = null)
            where T : class
        {
            return Execute<T>(HttpMethod.POST, args, string.Empty);
        }
        /// <summary>
        /// post自定义参数请求
        /// <typeparam name="T">返回接收类</typeparam>
        /// <param name="body">自定义参数</param>
        /// <returns></returns>
        public T ExecutePost<T>(string body)
            where T : class
        {
            return Execute<T>(HttpMethod.POST, null, body);
        }
        /// <summary>
        /// post请求返回object
        /// </summary>
        /// <param name="args">自定义参数</param>
        /// <returns></returns>
        public object ExecutePost(Dictionary<string, Object> args = null)
        {
            return Execute(HttpMethod.POST, args, string.Empty);
        }
        /// <summary>
        /// post指定参数类型
        /// </summary>
        /// <typeparam name="T">返回接收类</typeparam>
        /// <typeparam name="P">参数类</typeparam>
        /// <param name="postEntity"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public T ExecutePost<T, P>(P postEntity, Dictionary<string, Object> args = null)
            where T : class
            where P : class
        {
            var body = string.Empty;
            if (postEntity != null && typeof(P).Name != "String")
            {
                body = JsonConvert.SerializeObject(postEntity);
            }
            else
            {
                body = postEntity as string;
            }
            return Execute<T>(HttpMethod.POST, args, body);
        }
        /// <summary>
        /// post请求没有返回值
        /// </summary>
        /// <typeparam name="P">参数类</typeparam>
        /// <param name="postEntity">参数类</param>
        /// <param name="args">自定义参数</param>
        public void ExecutePost<P>(P postEntity, Dictionary<string, Object> args = null) where P : class
        {
            var body = "";
            if (postEntity != null)
            {
                body = JsonConvert.SerializeObject(postEntity);
            }
            Execute<NoResponse>(HttpMethod.POST, args, body);
        }
        /// <summary>
        /// get请求自定义参数请求
        /// </summary>
        /// <typeparam name="T">返回接收类</typeparam>
        /// <param name="args">自定义参数</param>
        /// <returns></returns>
        public T ExecuteGet<T>(Dictionary<string, Object> args = null) where T : class
        {
            return Execute<T>(HttpMethod.GET, args, string.Empty);
        }
        /// <summary>
        /// get请求没有返回值
        /// </summary>
        /// <param name="args">自定义参数</param>
        public void ExecuteGet(Dictionary<string, Object> args = null)
        {
            Execute<NoResponse>(HttpMethod.GET, args, string.Empty);
        }

        private T Execute<T>(HttpMethod httpMethod, Dictionary<string, Object> args, string body) where T : class
        {
            var queryString = "";
            if (args != null)
            {
                queryString = ToQueryString(args);
            }
            if (!string.IsNullOrEmpty(queryString))
            {
                queryString = "?" + queryString;
            }
            var uri = new Uri(_baseUrl + queryString);
            var webRequest = (HttpWebRequest)WebRequest.Create(uri);

            webRequest.ContentType = "application/json";
            webRequest.Accept = "application/json";
            webRequest.Method = httpMethod.ToString();

            if (httpMethod == HttpMethod.POST && !string.IsNullOrEmpty(body))
            {
                using (Stream stream = webRequest.GetRequestStream())
                {
                    var jsonByte = Encoding.UTF8.GetBytes(body);
                    stream.Write(jsonByte, 0, jsonByte.Length);
                }
            }
            else
            {
                webRequest.ContentLength = 0;
            }
            try
            {
                var webResponse = webRequest.GetResponse();
                using (var myStreamReader = new StreamReader(webResponse.GetResponseStream(), Encoding.UTF8))
                {
                    var str = myStreamReader.ReadToEnd();
                    return JsonConvert.DeserializeObject<T>(str);
                }
            }
            catch (WebException ex)
            {
                Stream myResponseStream = ((HttpWebResponse)ex.Response).GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                return JsonConvert.DeserializeObject<T>(myStreamReader.ReadToEnd());
            }
        }
        /// <summary>
        ///返回类型object
        /// </summary>
        /// <param name="httpMethod"></param>
        /// <param name="args"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        private object Execute(HttpMethod httpMethod, Dictionary<string, Object> args, string body)
        {
            var queryString = "";
            if (args != null)
            {
                queryString = ToQueryString(args);
            }
            if (!string.IsNullOrEmpty(queryString))
            {
                queryString = "?" + queryString;
            }
            var uri = new Uri(_baseUrl + queryString);
            var webRequest = (HttpWebRequest)WebRequest.Create(uri);

            webRequest.ContentType = "application/json";
            webRequest.Accept = "application/json";
            webRequest.Method = httpMethod.ToString();

            if (httpMethod == HttpMethod.POST && !string.IsNullOrEmpty(body))
            {
                using (Stream stream = webRequest.GetRequestStream())
                {
                    var jsonByte = Encoding.UTF8.GetBytes(body);
                    stream.Write(jsonByte, 0, jsonByte.Length);
                }
            }
            else
            {
                webRequest.ContentLength = 0;
            }
            var webResponse = webRequest.GetResponse();
            using (var myStreamReader = new StreamReader(webResponse.GetResponseStream(), Encoding.UTF8))
            {
                var str = myStreamReader.ReadToEnd();
                if (webResponse.ContentType.Contains("text/html"))
                {
                    return str;
                }
                return JsonConvert.DeserializeObject<object>(str);
            }
        }
        private abstract class NoResponse
        {

        }
        private string ToQueryString(Dictionary<string, Object> args)
        {
            var values = args.Where(a => a.Value != null)
                .Select(a => string.Format("{0}={1}", a.Key, HttpUtility.UrlEncode(a.Value.ToString()))).ToArray();

            return string.Join("&", values);
        }
    }
    public enum HttpMethod
    {
        POST,
        GET,
        PUT,
    }
}