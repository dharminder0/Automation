using NLog;
using QuizApp.Helpers;
using QuizApp.Response;
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
    [RoutePrefix("api/v1/Course")]
    [SwaggerResponseRemoveDefaults]
    public class CourseController : BaseController
    {
        public NLog.Logger Logger = LogManager.GetCurrentClassLogger();
        private ICourseService _iCourseService;

        public CourseController(ICourseService iCourseService)
        {
            _iCourseService = iCourseService;
        }

        /// <summary>
        /// This will return dashboard data
        /// </summary>
        /// <param name="OffsetValue"></param>
        /// <param name="IncludeSharedWithMe"></param>
        /// <param name="BusinessUserEmail"></param>
        /// <param name="OfficeIds"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetDashboardData")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetDashboardData(long OffsetValue, bool IncludeSharedWithMe, string BusinessUserEmail, string OfficeIds = null, bool IsJobRockAcademy = false)
        {
            try
            {
                var data = _iCourseService.GetDashboardData(OffsetValue, IncludeSharedWithMe, BusinessUserEmail, OfficeIds, BusinessUserId, CompanyInfo.Id, IsJobRockAcademy);
                if (_iCourseService.Status == ResultEnum.Ok)
                {
                    var response = new List<CourseResponse>();

                    CourseResponse res = new CourseResponse();
                    foreach (var course in data)
                    {
                        response.Add((CourseResponse)res.MapEntityToResponse(course));
                    }
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iCourseService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }


        /// <summary>
        /// To Get TechnicalRecruiterData
        /// </summary>
        /// <param name="OffsetValue"></param>
        /// <param name="BusinessUserEmail"></param>
        /// <param name="Module"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetTechnicalRecruiterData")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetTechnicalRecruiterData(long OffsetValue, string BusinessUserEmail, int Module)
        {
            try
            {
                var data = _iCourseService.GetTechnicalRecruiterData(OffsetValue, BusinessUserEmail, Module, BusinessUserId, UserInfo);
                if (_iCourseService.Status == ResultEnum.Ok)
                {
                    var response = new List<CourseResponse>();

                    CourseResponse res = new CourseResponse();
                    foreach (var course in data)
                    {
                        response.Add((CourseResponse)res.MapEntityToResponse(course));
                    }
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iCourseService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        [HttpGet]
        [Route("GetDashboardList")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetDashboardList()
        {
            try
            {
                var data = _iCourseService.GetDashboardList(UserInfo);
                if (_iCourseService.Status == ResultEnum.Ok)
                {
                    var response = new List<DashboardResponse>();

                    DashboardResponse res = new DashboardResponse();
                    foreach (var dashboard in data)
                    {
                        response.Add((DashboardResponse)res.MapEntityToResponse(dashboard));
                    }
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iCourseService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }
    }
}
