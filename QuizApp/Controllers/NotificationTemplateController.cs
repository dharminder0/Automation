using NLog;
using QuizApp.Helpers;
using QuizApp.Request;
using QuizApp.Response;
using QuizApp.Services.Model;
using QuizApp.Services.Service;
using Swashbuckle.Examples;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using static QuizApp.Request.TemplateTypeRequest;

namespace QuizApp.Controllers
{
    [RoutePrefix("api/v1/NotificationTemplate")]
    [SwaggerResponseRemoveDefaults]
    public class NotificationTemplateController : BaseController
    {
        public NLog.Logger Logger = LogManager.GetCurrentClassLogger();

        private INotificationTemplateService _iNotificationTemplateService;
        public NotificationTemplateController(INotificationTemplateService iNotificationTemplateService)
        {
            _iNotificationTemplateService = iNotificationTemplateService;
        }

        /// <summary>
        /// Add quiz template
        /// </summary>
        /// <param name="notificationTemplateRequestObj"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddQuizTemplate")]
        [AuthorizeTokenFilter]
        [SwaggerRequestExample(typeof(NotificationTemplateRequest), typeof(NotificationTemplateRequest.NotificationTemplateRequestExample))]
        public IHttpActionResult AddQuizTemplate(NotificationTemplateRequest notificationTemplateRequestObj)
        {
            NotificationTemplateResponse notificationTemplateResponse = null;
            try
            {
                NotificationTemplateRequest notificationTemplateRequest = new NotificationTemplateRequest();

                var notificationTempate = _iNotificationTemplateService.AddQuizInTemplate(notificationTemplateRequest.MapRequestToEntity(notificationTemplateRequestObj), BusinessUserId, CompanyInfo.Id);

                if (_iNotificationTemplateService.Status == ResultEnum.Ok)
                {
                    notificationTemplateResponse = new NotificationTemplateResponse();

                    notificationTemplateResponse = (NotificationTemplateResponse)notificationTemplateResponse.MapEntityToResponse(notificationTempate);

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = notificationTemplateResponse, message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = notificationTemplateResponse, message = _iNotificationTemplateService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = notificationTemplateResponse, message = ex.Message }));
            }
        }

        /// <summary>
        /// Update quiz in template
        /// </summary>
        /// <param name="notificationTemplateRequestObj"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateQuizInTemplate")]
        [AuthorizeTokenFilter]
        [SwaggerRequestExample(typeof(NotificationTemplateRequest), typeof(NotificationTemplateRequest.NotificationTemplateRequestExample))]
        public IHttpActionResult UpdateQuizInTemplate(NotificationTemplateRequest notificationTemplateRequestObj)
        {
            NotificationTemplateResponse notificationTemplateResponse = null;
            try
            {
                NotificationTemplateRequest notificationTemplateRequest = new NotificationTemplateRequest();

                _iNotificationTemplateService.UpdateQuizInTemplate(notificationTemplateRequest.MapRequestToEntity(notificationTemplateRequestObj), BusinessUserId, CompanyInfo.Id);

                if (_iNotificationTemplateService.Status == ResultEnum.Ok)
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = notificationTemplateResponse, message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = notificationTemplateResponse, message = _iNotificationTemplateService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = notificationTemplateResponse, message = ex.Message }));
            }
        }

        /// <summary>
        /// Save Body, Subject and SMS body in the template
        /// </summary>
        /// <param name="notificationTemplateRequestObj"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("SaveTemplateBody")]
        [AuthorizeTokenFilter]
        [SwaggerRequestExample(typeof(NotificationTemplateRequest), typeof(NotificationTemplateRequest.NotificationTemplateRequestExample))]
        public IHttpActionResult SaveTemplateBody(NotificationTemplateRequest notificationTemplateRequestObj)
        {
            NotificationTemplateResponse notificationTemplateResponse = null;
            try
            {
                NotificationTemplateRequest notificationTemplateRequest = new NotificationTemplateRequest();

                _iNotificationTemplateService.SaveTemplateBody(notificationTemplateRequest.MapRequestToEntity(notificationTemplateRequestObj), BusinessUserId);

                if (_iNotificationTemplateService.Status == ResultEnum.Ok)
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = notificationTemplateResponse, message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = notificationTemplateResponse, message = _iNotificationTemplateService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = notificationTemplateResponse, message = ex.Message }));
            }
        }

        /// <summary>
        /// Get template body details.
        /// </summary>
        /// <param name="notificationTemplateId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetTemplateBody")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetTemplateBody(int notificationTemplateId)
        {
            NotificationTemplateResponse notificationTemplateResponse = null;
            try
            {
                var notificationTemplateObj = _iNotificationTemplateService.GetTemplateBody(notificationTemplateId);

                if (_iNotificationTemplateService.Status == ResultEnum.Ok)
                {
                    notificationTemplateResponse = new NotificationTemplateResponse();

                    notificationTemplateResponse = (NotificationTemplateResponse)notificationTemplateResponse.MapEntityToResponse(notificationTemplateObj);

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = notificationTemplateResponse, message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = notificationTemplateResponse, message = _iNotificationTemplateService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = notificationTemplateResponse, message = ex.Message }));
            }
        }

        /// <summary>
        /// Get Template Body With Values
        /// </summary>
        /// <param name="QuizId"></param>
        /// <param name="LeadUserId"></param>
        /// <param name="SourceName"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetTemplateBodyWithValues")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetTemplateBodyWithValues(int QuizId, string SourceName = "")
        {
            List<NotificationTemplateResponse> notificationTemplateResponse = null;
            try
            {
                var notificationTemplateObj = _iNotificationTemplateService.GetTemplateBodyWithValues(QuizId, SourceName, CompanyInfo);

                if (_iNotificationTemplateService.Status == ResultEnum.Ok)
                {
                    notificationTemplateResponse = new List<NotificationTemplateResponse>();

                    foreach (var item in notificationTemplateObj)
                    {
                        var response = new NotificationTemplateResponse();

                        response = (NotificationTemplateResponse)response.MapEntityToResponse(item);

                        notificationTemplateResponse.Add(response);
                    }
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = notificationTemplateResponse, message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = notificationTemplateResponse, message = _iNotificationTemplateService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = notificationTemplateResponse, message = ex.Message }));
            }
        }

        /// <summary>
        /// Get details of template type.
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetTemplateTypeDetails")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetTemplateTypeDetails(TemplateTypeRequestModel rquestModel)
        {
            NotificationTemplateTypeResponseV1 notificationTemplateTypeResponse = null;
            try
            {
                var notificationTemplateObj = _iNotificationTemplateService.GetTemplateTypeDetails(rquestModel, CompanyInfo);

                if (_iNotificationTemplateService.Status == ResultEnum.Ok)
                {
                    notificationTemplateTypeResponse = new NotificationTemplateTypeResponseV1();

                    notificationTemplateTypeResponse = (NotificationTemplateTypeResponseV1)notificationTemplateTypeResponse.MapEntityToResponse(notificationTemplateObj);

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = notificationTemplateTypeResponse, message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = notificationTemplateTypeResponse, message = _iNotificationTemplateService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = notificationTemplateTypeResponse, message = ex.Message }));
            }
        }

        /// <summary>
        /// Delete template.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>        
        [Route("DeleteTemplate")]
        [AuthorizeTokenFilter]
        [HttpDelete]
        public IHttpActionResult DeleteTemplate(int id)
        {
            NotificationTemplateResponse notificationTemplateResponse = null;
            try
            {
                _iNotificationTemplateService.DeleteTemplate(id);

                if (_iNotificationTemplateService.Status == ResultEnum.Ok)
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = notificationTemplateResponse, message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = notificationTemplateResponse, message = _iNotificationTemplateService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = notificationTemplateResponse, message = ex.Message }));
            }
        }

        /// <summary>
        /// Set Quiz inactive in template
        /// </summary>
        /// <param name="notificationTemplateRequestObj"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("SetQuizInactive")]
        [AuthorizeTokenFilter]
        [SwaggerRequestExample(typeof(NotificationTemplateRequest), typeof(NotificationTemplateRequest.NotificationTemplateRequestExample))]
        public IHttpActionResult SetQuizInactive(NotificationTemplateRequest notificationTemplateRequestObj)
        {
            NotificationTemplateResponse notificationTemplateResponse = null;
            try
            {
                NotificationTemplateRequest notificationTemplateRequest = new NotificationTemplateRequest();

                _iNotificationTemplateService.SetQuizInactive(notificationTemplateRequest.MapRequestToEntity(notificationTemplateRequestObj));

                if (_iNotificationTemplateService.Status == ResultEnum.Ok)
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = notificationTemplateResponse, message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = notificationTemplateResponse, message = _iNotificationTemplateService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = notificationTemplateResponse, message = ex.Message }));
            }
        }

        /// <summary>
        /// Get default template by type.
        /// </summary>
        /// <param name="NotificationType"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetDefaultTemplateByType")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetDefaultTemplateByType(int NotificationType)
        {
            NotificationTemplateResponse notificationTemplateResponse = null;
            try
            {
                var notificationTemplateObj = _iNotificationTemplateService.GetDefaultTemplateByType(NotificationType);

                if (_iNotificationTemplateService.Status == ResultEnum.Ok)
                {
                    notificationTemplateResponse = new NotificationTemplateResponse();

                    notificationTemplateResponse = (NotificationTemplateResponse)notificationTemplateResponse.MapEntityToResponse(notificationTemplateObj);

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = notificationTemplateResponse, message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = notificationTemplateResponse, message = _iNotificationTemplateService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = notificationTemplateResponse, message = ex.Message }));
            }
        }

        /// <summary>
        /// search and suggestion API to search active or inactive automation
        /// </summary>
        /// <param name="Status"></param>
        /// <param name="NotificationType"></param>
        /// <param name="IncludeSharedWithMe"></param>
        /// <param name="OffsetValue"></param>
        /// <param name="OfficeId"></param>
        /// <param name="NotificationTemplateId"></param>
        /// <param name="SearchTxt"></param>
        /// <param name="IsPublished"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetSearchAndSuggestionByNotificationTemplate")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetSearchAndSuggestionByNotificationTemplate(int Status, int NotificationType, bool IncludeSharedWithMe, long OffsetValue, int? UsageType, string OfficeId = "", int? NotificationTemplateId = null, string SearchTxt = "", 
             bool? IsPublished = null)
        {
            AutomationListResponse AutomationListResponse = null;
            try
            {
                var QuizDetailList = _iNotificationTemplateService.GetSearchAndSuggestionByNotificationTemplate(Status, NotificationType, OfficeId, IncludeSharedWithMe, OffsetValue, CompanyInfo, NotificationTemplateId, SearchTxt, IsPublished, UsageType);

                if (_iNotificationTemplateService.Status == ResultEnum.Ok)
                {
                    AutomationListResponse = new AutomationListResponse();

                    AutomationListResponse = (AutomationListResponse)AutomationListResponse.MapEntityToResponse(QuizDetailList);

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = AutomationListResponse, message = "" }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = AutomationListResponse, message = _iNotificationTemplateService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = AutomationListResponse, message = ex.Message }));
            }
        }

        /// <summary>
        /// To Get Email Signature
        /// </summary>
        /// <param name="SignatureType"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetEmailSignature")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetEmailSignature(int SignatureType)
        {
            try
            {
                //var response = OWCHelper.GetEmailSignature(CompanyInfo.ClientCode, ((SignatureTypenum)SignatureType).ToString());
                var response = CommonStaticData.GetCachedEmailSignature(CompanyInfo.ClientCode, ((SignatureTypenum)SignatureType).ToString());

                return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = false, message = ex.Message }));
            }
        }

        /// <summary>
        /// GetTemplateDetailsWithValues
        /// </summary>
        /// <param name="NotificationTemplateId"></param>
        /// <param name="QuizId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetTemplateDetailsWithValues")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetTemplateDetailsWithValues(int NotificationTemplateId, int? QuizId = null)
        {
            AutomationListResponse AutomationListResponse = null;
            try
            {
                var item = _iNotificationTemplateService.GetTemplateDetailsWithValues(NotificationTemplateId, CompanyInfo, QuizId);

                if (_iNotificationTemplateService.Status == ResultEnum.Ok)
                {
                    var response = new NotificationTemplateResponse();

                    response = (NotificationTemplateResponse)response.MapEntityToResponse(item);

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = "" }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = AutomationListResponse, message = _iNotificationTemplateService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = AutomationListResponse, message = ex.Message }));
            }
        }

        /// <summary>
        /// Get inactiveList details of template type.
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("InActiveQuizList")]
        [AuthorizeTokenFilter]
        public IHttpActionResult InActiveQuizList(NotificationTemplateQuizListRequestModel requestModel)
        {
            try
            {
                var notificationTemplateObj = _iNotificationTemplateService.InActiveQuizList(requestModel, CompanyInfo, UserInfo.Id);

                if (_iNotificationTemplateService.Status == ResultEnum.Ok)
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = notificationTemplateObj, message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = notificationTemplateObj, message = _iNotificationTemplateService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }



        /// <summary>
        /// Get activeList details of template type.
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("QuizTemplateList")]
        [AuthorizeTokenFilter]
        public IHttpActionResult QuizTemplateList(NotificationTemplateInActiveRequestModel requestModel)
        {
            try
            {
                var notificationTemplateObj = _iNotificationTemplateService.QuizTemplateList(requestModel,CompanyInfo,UserInfo.Id);


                if (_iNotificationTemplateService.Status == ResultEnum.Ok)
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = notificationTemplateObj, message = string.Empty }));
                }
                else
                {

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = notificationTemplateObj, message = _iNotificationTemplateService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Set Quiz inactive in template
        /// </summary>
        /// <param name="notificationTemplateRequestObj"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UnLinkAutomation")]
        [AuthorizeTokenFilter]
        public IHttpActionResult UnLinkAutomation(InactiveNotificationTemplate notificationTemplateRequestObj)
        {
            try
            {

                _iNotificationTemplateService.UnLinkAutomation(notificationTemplateRequestObj);

                if (_iNotificationTemplateService.Status == ResultEnum.Ok)
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = true, message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iNotificationTemplateService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = false, message = ex.Message }));
            }
        }


        /// <summary>
        /// Update quiz in template
        /// </summary>
        /// <param name="notificationTemplateRequestObj"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("LinkAutomation")]
        [AuthorizeTokenFilter]
        public IHttpActionResult LinkAutomation(InactiveNotificationTemplate notificationTemplateRequestObj)
        {
            try
            {
                _iNotificationTemplateService.LinkAutomation(notificationTemplateRequestObj, CompanyInfo);

                if (_iNotificationTemplateService.Status == ResultEnum.Ok)
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = true, message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iNotificationTemplateService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = false, message = ex.Message }));
            }
        }

    }
}
