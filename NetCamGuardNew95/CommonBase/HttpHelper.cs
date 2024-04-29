using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class HttpHelper
    {
        private readonly HttpClient _httpClient;
        private string _url;
        private string _content;

        public HttpHelper()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        public HttpHelper Url(string url)
        {
            _url = url;
            return this;
        }
        public HttpHelper Content(string content)
        {
            _content = content;
            return this;
        }
        public HttpHelper HeaderKeyValue(string key, string value)
        {
            if (_httpClient.DefaultRequestHeaders.Contains(key))
                _httpClient.DefaultRequestHeaders.Remove(key);
            _httpClient.DefaultRequestHeaders.Add(key, value);
            return this;
        }
        public string Post()
        {
            var content = new StringContent(_content);
            var response = _httpClient.PostAsync(_url, content).Result;
            return response.IsSuccessStatusCode ? response.Content.ReadAsStringAsync().Result : null;
        }
        public string Get()
        {
            var response = _httpClient.GetAsync(_url).Result;
            return response.IsSuccessStatusCode ? response.Content.ReadAsStringAsync().Result : null;
        }

        #region 推荐的模拟POST请求,需要.net4.5,并引用System.Net.Http.dll;
        public static string Post(string Url, List<KeyValuePair<string, string>> paramList)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.MaxResponseContentBufferSize = 256000;
            /*httpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/36.0.1985.143 Safari/537.36");*/
            HttpResponseMessage response = httpClient.PostAsync(new Uri(Url), new FormUrlEncodedContent(paramList)).Result;
            var result = response.Content.ReadAsStringAsync().Result;
            return result;
        }
        #endregion
          
    }
}