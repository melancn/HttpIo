using System;
using System.Net;
using System.Web;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Collections;

namespace HttpAction
{
    public class HttpIo
    {
        const string DefaultUserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/40.0.2214.115 Safari/537.36";
        const int DefaultTimeout = 5;

        private static string Url = "";
        private static CookieContainer Cookie = new CookieContainer();
        private static CookieCollection OldCookie = new CookieCollection();
        private static HttpWebResponse Response;

        public static string UserAgent { get; set; }
        public static int Timeout { get; set; }

        public HttpIo(){}

        public HttpIo(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("message", nameof(url));
            }
            HttpIo.Url = url;
        }

        public static HttpWebResponse HttpGet()
        {
            if (string.IsNullOrEmpty(HttpIo.Url))
            {
                throw new ArgumentException("message", nameof(HttpIo.Url));
            }

            return HttpIo.HttpGet(HttpIo.Url);
        }

        public static HttpWebResponse HttpGet(string url)
        {
            if (string.IsNullOrEmpty(HttpIo.Url))
            {
                throw new ArgumentException("message", nameof(HttpIo.Url));
            }

            HttpIo.Url = url;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(HttpIo.Url);
            request.Method = "GET";
            request.UserAgent = HttpIo.UserAgent ?? HttpIo.DefaultUserAgent;
            request.CookieContainer = HttpIo.Cookie;
            request.Timeout = HttpIo.Timeout > 0 ? HttpIo.Timeout : HttpIo.DefaultTimeout;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            HttpIo.Cookie = HttpIo.setCookieContainer(response.Cookies);
            response.Close();

            HttpIo.Response = response;
            return response;
        }

        public static HttpWebResponse HttpGet(string url, Dictionary<String, String> queryArr, Encoding encode)
        {

            string query = "";
            Encoding encode_utf8 = System.Text.Encoding.GetEncoding("utf-8");

            foreach (KeyValuePair<string, string> kvp in queryArr)
            {
                query += "&" + HttpUtility.UrlEncode(kvp.Key, encode) + "=" + HttpUtility.UrlEncode(kvp.Value, encode);
            }

            return HttpIo.HttpGet(HttpIo.Url + "?" + query);
        }

        public static string GetContent(Encoding encode)
        {
            Stream receiveStream = HttpIo.Response.GetResponseStream();
            return ReadStream(receiveStream, encode);
        }

        public static string GetContent(HttpWebResponse response, Encoding encode)
        {
            Stream receiveStream = response.GetResponseStream();
            return ReadStream(receiveStream, encode);
        }

        public static void ErasureCookie()
        {
            HttpIo.OldCookie = new CookieCollection();
            HttpIo.Cookie = new CookieContainer();
        }

        private static string ReadStream(Stream receiveStream, Encoding encode)
        {
            string content = "";
            StreamReader readStream = new StreamReader(receiveStream, encode);

            Char[] read = new Char[256];
            int count = readStream.Read(read, 0, 256);
            while (count > 0)
            {
                content += new String(read, 0, count);
                count = readStream.Read(read, 0, 256);
            }
            readStream.Close();

            return content;
        }

        private static CookieContainer setCookieContainer(CookieCollection Cookies)
        {
            CookieContainer Container = new CookieContainer();
            Hashtable hshTable = new Hashtable();
            CookieCollection OldCookie = new CookieCollection();
            //新Cookie
            foreach (Cookie cookie in Cookies)
            {
                hshTable.Add(cookie.Name, 1);
                Container.Add(cookie);
                OldCookie.Add(cookie);
            }

            foreach (Cookie cookie in HttpIo.OldCookie)
            {
                if (hshTable.Contains(cookie.Name)) continue;
                Container.Add(cookie);
                OldCookie.Add(cookie);
            }

            HttpIo.OldCookie = OldCookie;
            return Container;
        }
    }


}