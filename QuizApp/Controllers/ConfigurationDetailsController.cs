using Elmah;
using NLog;
using QuizApp.Services.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;


namespace QuizApp.Controllers
{
    public class ConfigurationDetailsController : BaseController
    {
        public NLog.Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IConfigurationDetailsLogService _iConfigurationDetailsLogService;

        public ConfigurationDetailsController(IConfigurationDetailsLogService iConfigurationDetailsLogService)
        {
            _iConfigurationDetailsLogService = iConfigurationDetailsLogService;
        }

        /// <summary>
        /// To clear Old configurationDetails by ConfigurationDetailsid
        /// </summary>
        /// <param name="quizAttemptId"></param>
        /// <returns></returns>        
        [Route("api/v1/ClearOldConfigurationByQuizAttemptId")]
        [AuthorizeTokenFilter]
        [HttpDelete]
        public IHttpActionResult ClearOldConfigurationByQuizAttemptId(int quizAttemptId)
        {
            try
            {
                _iConfigurationDetailsLogService.ClearOldConfigurationByQuizAttemptId(quizAttemptId);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { message = "logs clear sucessfully." }));

            }
            catch (Exception ex)
            {
                //ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { message = ex.Message }));
            }
        }
    }
}
