using NLog;
using QuizApp.Helpers;
using QuizApp.Request;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace QuizApp.Controllers
{
    [RoutePrefix("api/v1/Cron")]
    [SwaggerResponseRemoveDefaults]
    public class CronController : BaseController
    {
        public NLog.Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// SendMailWithAttachment
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("SendMailWithAttachment")]
        [AuthorizeTokenFilter]
        public bool SendMailWithAttachment(CronRequest Obj)
        {
            try
            {
                return CommunicationHelper.SendMailWithAttachment(Obj.ToEmail, Obj.Subject, Obj.Body, Obj.ClientCode, Obj.Attachments);
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return false;
            }
        }
    }
}
