using Core.Common.Caching;
using NLog;
using QuizApp.Helpers;
using QuizApp.Request;
using QuizApp.Response;
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
    [RoutePrefix("api/v1/Integration")]
    [SwaggerResponseRemoveDefaults]
    public class IntegrationController : BaseController
    {
        private IWorkPackageService _iWorkPackageService;
        private readonly IApiUsageLogsService _apiUsageLogsService;
        public IntegrationController(IWorkPackageService iWorkPackageService, IApiUsageLogsService apiUsageLogsService)
        {
            _iWorkPackageService = iWorkPackageService;
            _apiUsageLogsService = apiUsageLogsService;
        }


        /// <summary>
        /// Push Work Package
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("PushWorkPackage")]
        [AuthorizeTokenFilter]
        [SwaggerRequestExample(typeof(WorkPackageRequest), typeof(WorkPackageRequest.WorkPackageRequestExample))]
        public IHttpActionResult PushWorkPackage(WorkPackageRequest Obj)
        {
            try
            {
                _iWorkPackageService.PushWorkPackage(Obj.MapRequestToEntityWithConfiguration(Obj));

                if (_iWorkPackageService.Status == ResultEnum.Ok)
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { message = "Work Package added." }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { message = _iWorkPackageService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { message = ex.Message }));
            }
        }

        /// <summary>
        /// To Get SHA-256 Generator Value
        /// </summary>
        /// <param name="CloudName"></param>
        /// <param name="Timestamp"></param>
        /// <param name="Username"></param>
        /// <param name="SecretKey"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetSHA256GeneratorValue")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetSHA256GeneratorValue(string CloudName, string Timestamp, string Username, string SecretKey)
        {
            try
            {
                var data = Utility.GetSHA256GeneratorValue(CloudName, Timestamp, Username, SecretKey);

                if (data != null)
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = data, message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iWorkPackageService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Get Search And Suggestion
        /// </summary>
        /// <param name="IncludeSharedWithMe"></param>
        /// <param name="IsDataforGlobalOfficeAdmin"></param>
        /// <param name="SearchTxt"></param>
        /// <param name="OfficeIdList"></param>
        /// <param name="IsPublished"></param>
        /// <param name="UsageType"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetSearchAndSuggestion")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetSearchAndSuggestion(bool IncludeSharedWithMe, bool IsDataforGlobalOfficeAdmin = false, string SearchTxt = "" , string OfficeIdList = null, bool? IsPublished = null, int? UsageType = null, bool? IsWhatsAppChatBotOldVersion = null)
        {
            AutomationListResponse AutomationListResponse = null;
            try
            {
                var QuizDetailList = _iWorkPackageService.GetSearchAndSuggestion(!string.IsNullOrEmpty(OfficeIdList) ? OfficeIdList.Split(',').ToList() : new List<string>(), IncludeSharedWithMe, SearchTxt, CompanyInfo, IsDataforGlobalOfficeAdmin, IsPublished, UsageType, IsWhatsAppChatBotOldVersion);
                //

                if (_iWorkPackageService.Status == ResultEnum.Ok)
                {
                    AutomationListResponse = new AutomationListResponse();

                    AutomationListResponse = (AutomationListResponse)AutomationListResponse.MapEntityToResponse(QuizDetailList);

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = AutomationListResponse, message = "" }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = AutomationListResponse, message = _iWorkPackageService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = AutomationListResponse, message = ex.Message }));
            }
        }

        /// <summary>
        /// To clear cache
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("ClearCache")]
        [AuthorizeTokenFilter]
        public IHttpActionResult ClearCache()
        {
            try
            {
                //Utility.ClearCache();
                AppLocalCache.Clear();
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { message = "Cache clear sucessfully." }));

            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = -1, message = ex.Message }));
            }
        }

        /// <summary>
        /// AddOrUpdateConfigurationDetails
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddOrUpdateConfigurationDetails")]
        [AuthorizeTokenFilter]
        [SwaggerRequestExample(typeof(AddOrUpdateConfigurationDetailsRequest), typeof(AddOrUpdateConfigurationDetailsRequest.AddOrUpdateConfigurationDetailsRequestExample))]
        public IHttpActionResult AddOrUpdateConfigurationDetails(AddOrUpdateConfigurationDetailsRequest Obj)
        {
            try
            {
                _iWorkPackageService.AddOrUpdateConfigurationDetails(Obj.MapRequestToEntity(Obj));

                if (_iWorkPackageService.Status == ResultEnum.Ok)
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { message = "Configuration Details Saved." }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { message = _iWorkPackageService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { message = ex.Message }));
            }
        }

        /// <summary>
        /// Remove Configuration
        /// </summary>
        /// <param name="ConfigurationId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("RemoveConfiguration")]
        [AuthorizeTokenFilter]
        public IHttpActionResult RemoveConfiguration(string ConfigurationId)
        {
            try
            {
                _iWorkPackageService.RemoveConfiguration(ConfigurationId);

                if (_iWorkPackageService.Status == ResultEnum.Ok)
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Configuration removed.", message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iWorkPackageService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// To Save QuizUrlSetting
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="DomainName"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("SaveQuizUrlSetting")]
        [AuthorizeTokenFilter]
        public IHttpActionResult SaveQuizUrlSetting(string Key, string DomainName, string Value,int companyId = 0)
        {
            try
            {
                _iWorkPackageService.SaveQuizUrlSetting(Key, DomainName, Value, companyId);

                if (_iWorkPackageService.Status == ResultEnum.Ok)
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "QuizUrlSetting saved.", message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iWorkPackageService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// To Check QuizUrlSettingKey
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="DomainName"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("CheckQuizUrlSettingKey")]
        [AuthorizeTokenFilter]
        public IHttpActionResult CheckQuizUrlSettingKey(string Key, string DomainName)
        {
            try
            {
                var response = _iWorkPackageService.CheckQuizUrlSettingKey(Key, DomainName);

                if (_iWorkPackageService.Status == ResultEnum.Ok)
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iWorkPackageService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// GetObjectFieldsList
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetObjectFieldsList")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetObjectFieldsList()
        {
            try
            {
                //var data = OWCHelper.GetObjectFieldsList(CompanyInfo.ClientCode);
                var data = CommonStaticData.GetCachedObjectFieldsList(CompanyInfo.ClientCode);

                if (_iWorkPackageService.Status == ResultEnum.Ok)
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = data, message = "" }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iWorkPackageService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        ///// <summary>
        ///// To Send Sms By WorkPackageId
        ///// </summary>
        ///// <param name="Obj"></param>
        ///// <returns></returns>
        //[HttpPost]
        //[Route("SendSmsByWorkPackageId")]
        //[AuthorizeTokenFilter]
        //[SwaggerRequestExample(typeof(SendSMSRequest), typeof(SendSMSRequest.SendSMSRequestExample))]
        //public IHttpActionResult SendSmsByWorkPackageId(SendSMSRequest Obj)
        //{
        //    try
        //    {
        //        SendSMSRequest request = new SendSMSRequest();

        //        _iWorkPackageService.SendSmsByWorkPackageId(request.MapRequestToEntity(Obj));

        //        if (_iWorkPackageService.Status == ResultEnum.Ok)
        //        {
        //            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = true, message = _iWorkPackageService.ErrorMessage }));
        //        }
        //        else
        //        {
        //            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iWorkPackageService.ErrorMessage }));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLog.LogError(ex);
        //        return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = false, message = ex.Message }));
        //    }
        //}


        /// <summary>
        /// To clear old logs
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("ClearOldLogs")]
        [AuthorizeTokenFilter]
        public IHttpActionResult ClearOldLogs()
        {
            try
            {
                _apiUsageLogsService.ClearApiUsageLogs();
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { message = "logs clear sucessfully." }));

            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = -1, message = ex.Message }));
            }
        }

        /// <summary>
        /// To clear Preview Quiz Id
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("ClearPreviewQuiz")]
        [AuthorizeTokenFilter]
        public IHttpActionResult ClearPreviewQuiz(int attemptId)
        {
            try
            {
                _apiUsageLogsService.DeletePerivewQuizAttempts(attemptId);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { message = "logs clear sucessfully." }));

            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new {message = ex.Message }));
            }
        }

    }
}
