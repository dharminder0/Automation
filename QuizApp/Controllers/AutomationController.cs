using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using NLog;
using QuizApp.Helpers;
using QuizApp.Response;
using QuizApp.Services.Model;
using QuizApp.Services.Service;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using static QuizApp.Helpers.Models;

namespace QuizApp.Controllers
{
    [RoutePrefix("api/v1/Automation")]
    [SwaggerResponseRemoveDefaults]
    public class AutomationController : BaseController {

        public NLog.Logger Logger = LogManager.GetCurrentClassLogger();
        private IAutomationDetailsService _automationService;
        public AutomationController(IAutomationDetailsService automationService) {
            _automationService = automationService;
        }



        /// <summary>
        /// Get AutomationDetails
        /// </summary>
        /// <param name="ClientCode"></param>
        /// <param name="QuizId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAutomationDetails")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetAutomationDetails(int QuizId, string ClientCode) {
            QuizResponse responseList = null;

            try {
                var quizLst = _automationService.GetAutomationDetail(QuizId, ClientCode);
                if (_automationService.Status == ResultEnum.Ok) {
                    var serializer = new JsonSerializer { ContractResolver = new CamelCasePropertyNamesContractResolver() };
                    var json = JObject.FromObject(quizLst, serializer);
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = json, message = string.Empty }));
                } else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = responseList, message = _automationService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = responseList, message = ex.Message }));
            }
        }

        /// <summary>
        /// Delete Verify Request for Automation
        /// </summary>
        /// <param name="LeadUserId"></param>
        /// <param name="ConfigurationId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("DeleteVerifyRequest")]
        [AuthorizeTokenFilter]
        public IHttpActionResult DeleteVerifyRequest(string LeadUserId, string ConfigurationId) {
            try {
                var isDeleted = _automationService.DeleteAutomationLog(LeadUserId, ConfigurationId);
                if (_automationService.Status == ResultEnum.Ok && isDeleted == false) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { Data = isDeleted, message = "Automation Verify Request Not Deleted Successfully" }));
                }

                if (_automationService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { Data = isDeleted, message = "Automation Verify Request Deleted Successfully" }));
                } else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { Data = string.Empty, message = _automationService.ErrorMessage }));
                }

            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { Data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Delete Verify Request for Automation
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="objectType"></param>
        /// <param name="clientCode"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("DeleteVerifyRequestAutomation")]
        [AuthorizeTokenFilter]
        public IHttpActionResult DeleteVerifyRequest(string requestId, string objectType, string clientCode) {
            try {
                var isDeleted = OWCHelper.DeleteVerifyRequest(requestId, objectType, clientCode);
                if (_automationService.Status == ResultEnum.Ok && isDeleted == false) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { Data = isDeleted, message = "Automation Verify Request Not Deleted Successfully" }));
                }

                if (_automationService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { Data = isDeleted, message = "Automation Verify Request Deleted Successfully" }));
                } else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { Data = string.Empty, message = _automationService.ErrorMessage }));
                }

            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { Data = string.Empty, message = ex.Message }));
            }
        }
    }
}
