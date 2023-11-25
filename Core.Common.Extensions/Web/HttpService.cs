using Core.Common.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

namespace Core.Common.Web
{
    public class HttpService {
        public HttpContext CurrentHttpContext;
        private string _userName;
        private string _password;
        private Dictionary<string, string> _headers = new Dictionary<string, string>();
        private JsonSerializerSettings _jsonSerializerSettings;

        public string RootUrl { get; set; }

        public HttpService(string url, string userName = null, string password = null) {
            RootUrl = url;
            _userName = userName;
            _password = password;
            _jsonSerializerSettings = new JsonSerializerSettings {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }
        public object Get(string route) {
            var url = string.Format("{0}/{1}", RootUrl.TrimEnd('/'), route).TrimEnd('/');
            var client = GetClient();
            var response = client.GetAsync(url).Result;

            if (response.IsSuccessStatusCode && response.Content != null) {
                var data = response.Content.ReadAsStringAsync().Result;
                try {
                    return JsonConvert.DeserializeObject(data, _jsonSerializerSettings);
                }
                catch {
                    return data;
                }
            }
            else {
                if (response.Content != null) {
                    var result = response.Content.ReadAsStringAsync().Result;
                    throw new Exception(result);
                }
                throw new Exception("Unknown error");
            }
        }

        public T Get<T>(string route) {
            var url = string.Format("{0}/{1}", RootUrl.TrimEnd('/'), route).TrimEnd('/');
            var client = GetClient();
            var response = client.GetAsync(url).Result;

            if (response.IsSuccessStatusCode && response.Content != null) {
                var data = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<T>(data, _jsonSerializerSettings);
            }
            else {
                if (response.Content != null) {
                    var result = response.Content.ReadAsStringAsync().Result;
                    throw new Exception(result);
                }
                throw new Exception("Unknown error");
            }
        }

        //public object Post(string route, object body) {
        //    var url = string.Format("{0}/{1}", RootUrl.TrimEnd('/'), route).TrimEnd('/');
        //    var client = GetClient();
        //    var response = client.PostAsJsonAsync(url, body).Result;
        //    if (response.IsSuccessStatusCode && response.Content != null) {
        //        var data = response.Content.ReadAsStringAsync().Result;
        //        return JsonConvert.DeserializeObject(data, _jsonSerializerSettings);
        //    }
        //    else {
        //        if (response.Content != null) {
        //            var result = response.Content.ReadAsStringAsync().Result;
        //            throw new Exception(result);
        //        }
        //        throw new Exception("Unknown error");
        //    }
        //}

        //public T Post<T>(string route, object body) {
        //    var url = string.Format("{0}/{1}", RootUrl.TrimEnd('/'), route).TrimEnd('/');
        //    var client = GetClient();
        //    var response = client.PostAsJsonAsync(url, body).Result;
        //    if (response.IsSuccessStatusCode && response.Content != null) {
        //        var data = response.Content.ReadAsStringAsync().Result;
        //        return JsonConvert.DeserializeObject<T>(data, _jsonSerializerSettings);
        //    }
        //    else {
        //        if (response.Content != null) {
        //            var result = response.Content.ReadAsStringAsync().Result;
        //            throw new Exception(result);
        //        }
        //        throw new Exception("Unknown error");
        //    }
        //}

        //public object Put(string route, object body) {
        //    var url = string.Format("{0}/{1}", RootUrl.TrimEnd('/'), route).TrimEnd('/');
        //    var client = GetClient();
        //    var response = client.PutAsJsonAsync(url, body).Result;
        //    if (response.IsSuccessStatusCode && response.Content != null) {
        //        var data = response.Content.ReadAsStringAsync().Result;
        //        return JsonConvert.DeserializeObject(data, _jsonSerializerSettings);
        //    }
        //    else {
        //        if (response.Content != null) {
        //            var result = response.Content.ReadAsStringAsync().Result;
        //            throw new Exception(result);
        //        }
        //        throw new Exception("Unknown error");
        //    }
        //}

        //public T Put<T>(string route, object body) {
        //    var url = string.Format("{0}/{1}", RootUrl.TrimEnd('/'), route).TrimEnd('/');
        //    var client = GetClient();
        //    var response = client.PutAsJsonAsync(url, body).Result;
        //    if (response.IsSuccessStatusCode && response.Content != null) {
        //        var data = response.Content.ReadAsStringAsync().Result;
        //        return JsonConvert.DeserializeObject<T>(data, _jsonSerializerSettings);
        //    }
        //    else {
        //        if (response.Content != null) {
        //            var result = response.Content.ReadAsStringAsync().Result;
        //            throw new Exception(result);
        //        }
        //        throw new Exception("Unknown error");
        //    }
        //}

        public T Patch<T>(string route, object body)
        {
            var url = string.Format("{0}/{1}", RootUrl.TrimEnd('/'), route);
            var client = GetClient();
            var response = client.PatchAsync(url, body).Result;
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var data = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<T>(data);
            }
            else
            {
                if (response.Content != null)
                {
                    var result = response.Content.ReadAsStringAsync().Result;
                    throw new Exception(result);
                }
                throw new Exception("Unknown error");
            }
        }

        public object Delete(string route, object body) {
            var url = string.Format("{0}/{1}", RootUrl.TrimEnd('/'), route).TrimEnd('/');
            var client = GetClient();
            var response = client.DeleteAsync(url).Result;
            if (response.IsSuccessStatusCode && response.Content != null) {
                var data = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject(data);
            }
            else {
                if (response.Content != null) {
                    var result = response.Content.ReadAsStringAsync().Result;
                    throw new Exception(result);
                }
                throw new Exception("Unknown error");
            }
        }

        public T Delete<T>(string route) {
            var url = string.Format("{0}/{1}", RootUrl.TrimEnd('/'), route).TrimEnd('/');
            var client = GetClient();
            var response = client.DeleteAsync(url).Result;
            if (response.IsSuccessStatusCode && response.Content != null) {
                var data = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<T>(data);
            }
            else {
                if (response.Content != null) {
                    var result = response.Content.ReadAsStringAsync().Result;
                    throw new Exception(result);
                }
                throw new Exception("Unknown error");
            }
        }

        public void AddHeader(string key, string value) {
            if (_headers.ContainsKey(key))
                _headers[key] = value;
            else
                _headers.Add(key, value);
        }

        private HttpClient GetClient() {
            var httpClient = new HttpClient();
            if (!string.IsNullOrWhiteSpace(_userName) && !string.IsNullOrWhiteSpace(_password)) {
                var basicParams = string.Format("{0}:{1}", _userName, _password);
                var basicAuthorization = string.Format("Basic {0}", Base64Encode(basicParams));
                httpClient.DefaultRequestHeaders.Add("Authorization", basicAuthorization);
            }
            foreach (var header in _headers) {
                httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return httpClient;
        }

        private static string Base64Encode(string plainText) {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        private HttpClient GetFilledHttpClient(string apiUrl = "") {
            var client = new HttpClient();
            var proxyEnabled = bool.Parse(ConfigurationManager.AppSettings["ProxyEnabled"]);
            if (proxyEnabled) {
                // Proxy Server Info
                var proxyHost = ConfigurationManager.AppSettings["ProxyHost"];
                var proxyPort = !string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["ProxyPort"])
                    ? int.Parse(ConfigurationManager.AppSettings["ProxyPort"])
                    : 8080;
                var proxyUserName = ConfigurationManager.AppSettings["ProxyUserName"];
                var proxyPassword = ConfigurationManager.AppSettings["ProxyPassword"];

                var proxyServer = new WebProxy(proxyHost, proxyPort);
                proxyServer.Credentials = new NetworkCredential { UserName = proxyUserName, Password = proxyPassword };
                //client.Proxy = proxyServer;

                var httpClientHandler = new HttpClientHandler() {
                    Proxy = proxyServer,
                    PreAuthenticate = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential { UserName = proxyUserName, Password = proxyPassword }
                };
                client = new HttpClient(httpClientHandler);

            }

            var bearerKey = !string.IsNullOrWhiteSpace(apiUrl) ? ConfigurationManager.AppSettings["ProductionApiBearer"] : ConfigurationManager.AppSettings["ApiBearer"];
            client.DefaultRequestHeaders.Add("Authorization", bearerKey);
            if (HttpContext.Current != null) {
                CurrentHttpContext = HttpContext.Current;
            }

            return client;
        }
    }
}
