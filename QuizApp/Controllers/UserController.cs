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
    [RoutePrefix("api/v1/User")]
    [SwaggerResponseRemoveDefaults]
    public class UserController : BaseController
    {
        public NLog.Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Get users related to specific office
        /// </summary>
        /// <param name="OfficeId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetUserListByOfficeId")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetUserListByOfficeId(string OfficeId)
        {
            List<BusinessUserResponseForOffice> businessUserResponseList = null;
            BusinessUserResponseForOffice businessUserResponse = null;

            try
            {
                var response = OWCHelper.GetUserListByOfficeId(OfficeId,CompanyInfo);

                if (response != null)
                {
                    businessUserResponseList = new List<BusinessUserResponseForOffice>();

                    foreach (var obj in response)
                    {
                        businessUserResponse = new BusinessUserResponseForOffice();

                        businessUserResponseList.Add(businessUserResponse.MapEntityToResponse(obj, obj.token));
                    }

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = businessUserResponseList, message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.Unauthorized, new { data = businessUserResponseList, message = "Invalid token" }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = businessUserResponseList, message = ex.Message }));
            }
        }
    }
}
