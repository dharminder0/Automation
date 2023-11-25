using QuizApp.Helpers;
using QuizApp.Request;
using QuizApp.Services.Service;
using Swashbuckle.Examples;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace QuizApp.Controllers
{
    [RoutePrefix("api/v1/Communication")]
    [SwaggerResponseRemoveDefaults]
    public class CommunicationController : BaseController {
        private readonly ICommunicationService _communicationService;
        public CommunicationController(ICommunicationService communicationService) {
            _communicationService = communicationService;
        }

        /// <summary>
        ///Update Communication Status
        /// </summary>
        /// <param name="communicationRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Webhook/UpdateCommunicationStatus")]
        [AuthorizeTokenFilter]
        public IHttpActionResult UpdateCommunicationStatus(CommunicationRequest communicationRequest) {
            try {
              var updateStatus =   _communicationService.UpdateCommunicationStatus(communicationRequest);

                if (_communicationService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new {data = updateStatus, message = string.Empty }));
                } else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new {data = updateStatus, message = _communicationService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new {data = string.Empty, message = ex.Message }));
            }
        }
    }
}
