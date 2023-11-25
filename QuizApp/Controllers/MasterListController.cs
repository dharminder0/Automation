using NLog;
using QuizApp.Helpers;
using QuizApp.Response;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace QuizApp.Controllers
{
    [RoutePrefix("api/v1/MasterList")]
    [SwaggerResponseRemoveDefaults]
    public class MasterListController : BaseController
    {
        /// <summary>
        /// Get List Map fields
        /// used in variable popup for automation/appointment
        /// </summary>
        /// <param name="clientCode"></param>
        /// <param name="objects"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{clientCode}/variablefields")]
        public IHttpActionResult GetVariablesFields(string clientCode, string objects) {
            if (string.IsNullOrWhiteSpace(clientCode) || string.IsNullOrWhiteSpace(objects)) {
                return null;
            } else {
                try {
                    var Fileds = OWCHelper.GetVariableFields(clientCode, objects);
                    return Json(Fileds);
                } catch (Exception ex) {
                    ErrorLog.LogError(ex);
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { message = ex.Message }));
                }
            }
        }
    }
}
