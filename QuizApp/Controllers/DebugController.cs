using System;
using System.Web.Http;
using System.Web.Http.Description;

namespace QuizApp.Controllers {
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class DebugController : ApiController {
        /// <summary>
        /// This will be used as maintanance point for the project 
        /// This must not deleted
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        [Route("api/debug/tm")]
        [HttpGet]
        public IHttpActionResult DebugTm() {
            return Json(true);
        }
    }
}
