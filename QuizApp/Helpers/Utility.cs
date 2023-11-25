using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Xml.Linq;
using System.Runtime.Caching;
using Newtonsoft.Json;
using System.Transactions;

namespace QuizApp.Helpers
{
    public class Utility
    {
        public static Dictionary<string, string> NotificationDefaultTemplates;

        public static DateTime ConvertDateToUTC(DateTime date, long offset)
        {
            return date.AddMilliseconds(offset);
        }

        public static DateTime ConvertUTCDateToLocalDate(DateTime date, long offset)
        {
            return date.AddMilliseconds(-1 * offset);
        }

        public static void LoadTemplatesMessages()
        {
            NotificationDefaultTemplates = new Dictionary<string, string>();

            string virtualPath = String.Format("~/");
            string serverPath = HttpContext.Current.Server.MapPath(virtualPath);

            var path = Path.Combine(serverPath, "NotificationDefaultTemplate.xml");

            if (File.Exists(path))
            {
                var xdoc = XDocument.Load(path);
                foreach (XElement xelement in xdoc.Root.Elements("message"))
                {
                    NotificationDefaultTemplates.Add(xelement.Attribute("name").Value, xelement.Value);
                }
            }
        }

        public static string GetSHA256GeneratorValue(string CloudName, string Timestamp, string Username, string SecretKey)
        {
            var inputString = "cloud_name=" + CloudName + "&timestamp=" + Timestamp + "&username=" + Username + "_" + SecretKey;

            SHA256 sha256 = SHA256Managed.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(inputString);
            byte[] hash = sha256.ComputeHash(bytes);

            StringBuilder result = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
            {
                result.Append(hash[i].ToString("X2"));
            }
            return result.ToString();
        }

        public static TransactionScope CreateTransactionScope()
        {
            return new TransactionScope(TransactionScopeOption.Required,
                                        new TransactionOptions
                                        {
                                            IsolationLevel = IsolationLevel.ReadCommitted,
                                            Timeout = new TimeSpan(0, 15, 0)
                                        }
                                       );
        }

        //public static object GetCacheValue(string key)
        //{
        //    MemoryCache memoryCache = MemoryCache.Default;
        //    return memoryCache.Get(key);
        //}

        //public static bool AddCache(string key, object value, DateTimeOffset absExpiration)
        //{
        //    MemoryCache memoryCache = MemoryCache.Default;
        //    return memoryCache.Add(key, value, absExpiration);
        //}

        //public static void DeleteCache(string key)
        //{
        //    MemoryCache memoryCache = MemoryCache.Default;
        //    if (memoryCache.Contains(key))
        //    {
        //        memoryCache.Remove(key);
        //    }
        //}

        //public static void ClearCache()
        //{
        //    MemoryCache memoryCache = MemoryCache.Default;

        //    foreach (KeyValuePair<String, Object> a in memoryCache.ToList())
        //    {
        //        memoryCache.Remove(a.Key);
        //    }
        //}
    }
}
