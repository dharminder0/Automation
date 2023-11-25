using NLog;
using QuizApp.Helpers;
using QuizApp.Request;
using QuizApp.Response;
using QuizApp.Services.Model;
using QuizApp.Services.Service;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace QuizApp.Controllers
{
    [RoutePrefix("api/v1/QuizDuplicate")]
    [SwaggerResponseRemoveDefaults]
    public class QuizDuplicateController : BaseController
    {
        public NLog.Logger Logger = LogManager.GetCurrentClassLogger();
        private IQuizDuplicateService _iQuizDuplicateService;

        public QuizDuplicateController(IQuizDuplicateService iQuizDuplicateService)
        {
            _iQuizDuplicateService = iQuizDuplicateService;
        }


        /// <summary>
        /// Add Quiz Duplicate Question
        /// </summary>
        /// <param name="quizId"></param>
        /// <param name="questionId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddQuestion")]
        [AuthorizeTokenFilter]
        public IHttpActionResult AddQuestion(int quizId, int questionId)
        {
            QuizQuestionDetailsResponse response = null;
            try
            {
                var quizQuestionDetailsObj = _iQuizDuplicateService.AddQuizDuplicateQuestion(quizId, questionId, BusinessUserId, CompanyInfo.Id);

                if (_iQuizDuplicateService.Status == ResultEnum.Ok)
                {
                    response = new QuizQuestionDetailsResponse();

                    response = (QuizQuestionDetailsResponse)response.MapEntityToResponse(quizQuestionDetailsObj);

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = response, message = _iQuizDuplicateService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = response, message = ex.Message }));
            }
        }



        /// <summary>
        /// Add Quiz Duplicate Result
        /// </summary>
        /// <param name="QuizId"></param>
        /// <param name="quizResultId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddQuizResult")]
        [AuthorizeTokenFilter]
        public IHttpActionResult AddQuizResult(int quizId,int quizResultId)
        {
            QuizResultResponse response = null;

            try
            {
                QuizResult quizResult = null;

                quizResult = _iQuizDuplicateService.AddQuizDuplicateResult(quizId, quizResultId, BusinessUserId, CompanyInfo.Id);

                if (_iQuizDuplicateService.Status == ResultEnum.Ok)
                {
                    response = new QuizResultResponse();

                    response = (QuizResultResponse)response.MapEntityToResponse(quizResult);

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = response, message = string.Empty }));
                }

            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Duplicate Quiz By CompanyId
        /// </summary>
        /// <param name="duplicateQuizRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("OtherCompany")]
        [AuthorizeTokenFilter(IsJWTRequired = false)]
        public IHttpActionResult OtherCompany(DuplicateQuizRequest duplicateQuizRequest)
        {
            try
            {
                if(duplicateQuizRequest == null)
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = "Invalid Request" }));
                }
                var duplicatequizId = _iQuizDuplicateService.DuplicateQuizByCompanyId(duplicateQuizRequest);

                if (_iQuizDuplicateService.Status == ResultEnum.Ok)
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = duplicatequizId, message = string.Empty }));
                }
                else if (_iQuizDuplicateService.Status == ResultEnum.OkWithMessage)
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = string.Empty, message = _iQuizDuplicateService.ErrorMessage }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizDuplicateService.ErrorMessage }));
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
