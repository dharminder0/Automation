using Core.Common.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace QuizApp.Controllers
{
    [RoutePrefix("api/v1/cache")]
    public class CacheController : BaseController
    {
        /// <summary>
        /// Gets all cached objects along with their keys from memory cache
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAll")]
        [AuthorizeTokenFilter(IsJWTRequired = false)]
        public IHttpActionResult GetAllCacheObjects()
        {
            var cache = AppLocalCache.GetAllCahedObjects();
            return Json(cache);
        }

        /// <summary>
        /// Gets a cached object by its key
        /// </summary>
        /// <param name="key">key of the cached object</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetByKey/{key}")]
        [AuthorizeTokenFilter(IsJWTRequired = false)]
        public IHttpActionResult GetByKey(string key)
        {
            var cache = AppLocalCache.Get(key);
            return Json(cache);
        }

        /// <summary>
        /// Gets an array of all keys of the cached objects
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAllKeys")]
        [AuthorizeTokenFilter(IsJWTRequired = false)]
        public IHttpActionResult GetAllCacheKeys()
        {
            var cache = AppLocalCache.GetAllKeys();
            return Json(cache);
        }

        /// <summary>
        /// Deletes a cached object from memory cache by its key
        /// </summary>
        /// <param name="key">key of the cached object</param>
        /// <returns></returns>
        [HttpPost]
        [Route("DeleteByKey/{key}")]
        [AuthorizeTokenFilter(IsJWTRequired = false)]
        public IHttpActionResult DeleteByKey(string key)
        {
            try
            {
                AppLocalCache.Remove(key);
                return Json(new { success = true });
            }
            catch (Exception error)
            {
                // return BadRequest(error);
                return null;
            }
        }

        /// <summary>
        /// Clears all cached objects from memory cache
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("DeleteAll")]
        [AuthorizeTokenFilter(IsJWTRequired = false)]
        public IHttpActionResult DeleteAllCacheObjects()
        {
            try
            {
                AppLocalCache.Clear();
                return Json(new { success = true });
            }
            catch (Exception error)
            {
                return null;
                // return BadRequest(error);
            }
        }
    }
}

