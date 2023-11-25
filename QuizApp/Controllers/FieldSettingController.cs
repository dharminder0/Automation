using QuizApp.Helpers;
using QuizApp.Services.Service;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace QuizApp.Controllers
{
    [RoutePrefix("api/Fields")]
    [SwaggerResponseRemoveDefaults]
    public class FieldSettingController : BaseController
    {
        private readonly FieldSettingService _fieldSettingService;
        public FieldSettingController(FieldSettingService fieldSettingService) {
            _fieldSettingService = fieldSettingService;

        }
        /// <summary>
        /// Get field sync setting
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("SyncSetting")]
        [AuthorizeTokenFilter]
        public IHttpActionResult FieldSyncSetting() {
            try {
                var response = _fieldSettingService.FieldSyncSetting();

                if (_fieldSettingService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
                } else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _fieldSettingService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }
    }
}
