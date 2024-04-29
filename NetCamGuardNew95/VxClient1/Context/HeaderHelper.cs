using LanguageResource;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

namespace VxGuardClient.Context
{
    public class HeaderHelper
    {
        public HeaderHelper()
        { 
        }  

        public static HttpClient HeaderAttachment()
        {
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Accept-Language", LangUtilities.LanguageCode);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json")); 
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", WebCookie.AccessToken); 
            return httpClient;
        }
    }
}