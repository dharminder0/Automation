using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Swashbuckle.Swagger.Annotations;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using NLog;
using QuizApp.Services.Service;
using QuizApp.Helpers;
using QuizApp.Response;

namespace QuizApp.Controllers
{
    [RoutePrefix("api/v1/Template")]
    [SwaggerResponseRemoveDefaults]
    public class TemplateController : BaseController
    {
        public NLog.Logger Logger = LogManager.GetCurrentClassLogger();
        private ITemplateService _iTemplateService;

        public TemplateController(ITemplateService iTemplateService)
        {
            _iTemplateService = iTemplateService;
        }
        /// <summary>
        /// To Get Templates By category and quiz type
        /// </summary>
        /// <param name="QuizType"></param>
        /// <param name="CategoryId"></param>
        /// <param name="PageNo"></param>
        /// <param name="PageSize"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetTemplateList")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetTemplateList(string QuizType, string CategoryId, int PageNo, int PageSize)
        {
            try
            {
                QuizTemplateResponse responseList = null;

                var quizTemplates = _iTemplateService.GetTemplateList(QuizType, CategoryId, PageNo, PageSize);

                if (_iTemplateService.Status == ResultEnum.Ok)
                {
                    responseList = new QuizTemplateResponse();

                    responseList = (QuizTemplateResponse)responseList.MapEntityToResponse(quizTemplates);

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = responseList, message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = responseList, message = _iTemplateService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// to get All Templates
        /// </summary>
        /// <param name="OffsetValue"></param>
        /// <param name="QuizTypeId"></param>
        /// <param name="CategoryId"></param>
        /// <param name="PageNo"></param>
        /// <param name="PageSize"></param>
        /// <param name="SearchTxt"></param>
        /// <param name="OrderByDate"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetDashboardTemplates")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetDashboardTemplates(long OffsetValue, string QuizTypeId, string CategoryId, int PageNo, int PageSize, string SearchTxt = "", int OrderByDate = (int)OrderByEnum.Descending)
        {
            QuizDashboardTemplateResponse responseList = null;

            try
            {
                var quizTemplates = _iTemplateService.GetDashboardTemplates(OffsetValue, QuizTypeId, CategoryId, SearchTxt, PageNo, PageSize, OrderByDate, CompanyInfo.Id, CompanyInfo.ClientCode);

                if (_iTemplateService.Status == ResultEnum.Ok)
                {
                    responseList = new QuizDashboardTemplateResponse();

                    responseList = (QuizDashboardTemplateResponse)responseList.MapEntityToResponse(quizTemplates);

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = responseList, message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = responseList, message = _iTemplateService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = responseList, message = ex.Message }));
            }
        }

        /// <summary>
        /// To Set Template status
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="TemplateStatus"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("SetTemplateStatus")]
        [AuthorizeTokenFilter]
        public IHttpActionResult SetTemplateActive(int Id, int TemplateStatus)
        {
            try
            {
                _iTemplateService.SetTemplateStatus(Id, TemplateStatus, BusinessUserId);

                if (_iTemplateService.Status == ResultEnum.Ok)
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Template status updated.", message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iTemplateService.ErrorMessage }));
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
