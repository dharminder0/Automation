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
    [RoutePrefix("api/v1/ReminderSetting")]
    [SwaggerResponseRemoveDefaults]
    public class ReminderSettingController : BaseController
    {
        public NLog.Logger Logger = LogManager.GetCurrentClassLogger();

        private IReminderSettingService _iReminderSettingService;
        public ReminderSettingController(IReminderSettingService iReminderSettingService)
        {
            _iReminderSettingService = iReminderSettingService;
        }

        /// <summary>
        /// Get reminder settings
        /// </summary>
        /// <param name="OfficeId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetReminderSettings")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetReminderSettings(string OfficeId)
        {
            ReminderSettingResponse reminderSettingResponse = null;
            try
            {
                var reminderSetting = _iReminderSettingService.GetReminderSettings(OfficeId, CompanyInfo.Id);

                if (_iReminderSettingService.Status == ResultEnum.Ok)
                {
                    reminderSettingResponse = new ReminderSettingResponse();

                    reminderSettingResponse = (ReminderSettingResponse)reminderSettingResponse.MapEntityToResponse(reminderSetting);

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = reminderSettingResponse, message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = reminderSettingResponse, message = _iReminderSettingService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = reminderSettingResponse, message = ex.Message }));
            }
        }

        /// <summary>
        /// Save reminder settings
        /// </summary>
        /// <param name="ReminderSettingRequestObj"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("SaveReminderSettings")]
        [AuthorizeTokenFilter]
        [SwaggerRequestExample(typeof(ReminderSettingRequest), typeof(ReminderSettingRequest.ReminderSettingRequestExample))]
        public IHttpActionResult SaveReminderSettings(ReminderSettingRequest ReminderSettingRequestObj)
        {
            ReminderSettingResponse reminderSettingResponse = null;
            try
            {
                ReminderSettingRequest reminderSettingRequest = new ReminderSettingRequest();

                _iReminderSettingService.SaveReminderSettings(reminderSettingRequest.MapRequestToEntity(ReminderSettingRequestObj), BusinessUserId , CompanyInfo.Id);

                if (_iReminderSettingService.Status == ResultEnum.Ok)
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Reminder setting updated.", message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = reminderSettingResponse, message = _iReminderSettingService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = reminderSettingResponse, message = ex.Message }));
            }
        }
    }
}
