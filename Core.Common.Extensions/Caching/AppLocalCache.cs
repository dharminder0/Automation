using Core.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Common.Caching {
    public static class AppLocalCache {
        private static Dictionary<string, CacheObject> _cache = new Dictionary<string, Caching.CacheObject>();
        private static bool _isCacheEnabled = ConfigurationManager.AppSettings["AppLocalCacheEnabled"]?.ToBool() ?? false;
        private static int _defaultCacheHours = ConfigurationManager.AppSettings["DefaultAppLocalCacheHours"]?.ToInt() ?? 5;

        public static void Add(string key, CacheObject obj) {
            key = key.ToLower();
            lock (_cache){
                if (_cache.ContainsKey(key)) {
                    _cache.Remove(key);
                }
                _cache.Add(key, obj);
            }           
        }

        public static void Add<T>(string key, CacheObject<T> obj) {
            if (!_isCacheEnabled) return;
            key = key.ToLower();
            lock (_cache) {
                if (_cache.ContainsKey(key)) {
                    _cache.Remove(key);
                }
                _cache.Add(key, obj);
            }
        }

        public static CacheObject<T> Get<T>(string key) {
            key = key.ToLower();
            if (!_isCacheEnabled) return null;
            if (!_cache.ContainsKey(key))
                return null;
            if (_cache[key].ExpireDate < DateTime.Now) {
                Remove(key);
                return null;
            }            
            return (CacheObject<T>)_cache[key];
        }

        public static bool KeyExist(string key)
        {
            if (!_isCacheEnabled) return false;
            key = key.ToLower();
            lock (_cache)
            {
                if (_cache.ContainsKey(key))
                    return true;
                
            }
            return false;
        }

        public static CacheObject Get(string key) {
            if (!_isCacheEnabled) return null;
            key = key.ToLower();
            if (!_cache.ContainsKey(key))
                return null;
            if (_cache[key].ExpireDate < DateTime.Now) {
                Remove(key);
                return null;
            }
            return _cache[key];
        }

        public static void RemoveLikeKeys(string key)
        {
            if (!_isCacheEnabled) return;
            key = key.ToLower();
            lock (_cache)
            {
                foreach (var item in _cache.Keys.ToList())
                {
                    if (item.Contains(key))
                    {
                        Remove(item);
                    }
                }
            }
        }

        public static void Remove(string key) {
            if (!_isCacheEnabled) return;
            key = key.ToLower();
            lock (_cache) {
                if(_cache.ContainsKey(key))
                    _cache.Remove(key);
            }
        }

        public static void Remove(IEnumerable<string> keys) {
            if (!_isCacheEnabled) return;
            lock (_cache) {
                foreach (var key in keys) {
                    if (_cache.ContainsKey(key.ToLower()))
                        _cache.Remove(key.ToLower());
                }                
            }
        }

        public static void Clear() {
            if (!_isCacheEnabled) return;
            lock (_cache) {                
                _cache.Clear();
            }
        }

        public static IEnumerable<string> GetAllKeys() {
            return _cache.Keys;
        }

        public static bool ContainsKey(string key) {
            return _cache.ContainsKey(key);
        }

        public static T GetOrCache<T>(string key, Func<T> f) {
            return GetOrCache(key, _defaultCacheHours *60, f);
        }

        public static T GetOrCache<T>(string key, int minutes, Func<T> f) {
            var result = Get<T>(key);
            if(result == null) {
                var data = f();
                if (data != null)
                {
                    Add<T>(key, new CacheObject<T> { ExpireDate = DateTime.Now.AddMinutes(minutes), Data = data });
                }
                return data;
            }
            return result.Data;
        }

        public static T GetCacheOrDirectData<T>(string key, Func<T> f)
        {
            var result = Get<T>(key);
            if (result == null)
            {
                var data = f();
                return data;
            }
            return result.Data;
        }

        public static Dictionary<string, CacheObject> GetAllCahedObjects() {
            return _cache;
        }
    }
}
