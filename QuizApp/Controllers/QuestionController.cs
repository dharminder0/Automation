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
    [RoutePrefix("api/v1/Question")]
    [SwaggerResponseRemoveDefaults]
    public class QuestionController : BaseController
    {
        public NLog.Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IQuestionService _questionService;
        public QuestionController(IQuestionService questionService)
        {
            _questionService = questionService;
        }
        /// <summary>
        /// Add Quiz Question
        /// </summary>
        /// <param name="QuizId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/api/v1/Quiz/AddQuizQuestion")]
        [AuthorizeTokenFilter]
        public IHttpActionResult AddQuizQuestion(int QuizId, int? Type = (int)BranchingLogicEnum.QUESTION, bool isWhatsappEnable = false)
        {
            QuizQuestionDetailsResponse response = null;
            try
            {
                var quizQuestionDetailsObj = _questionService.AddQuizQuestion(QuizId, BusinessUserId, CompanyInfo.Id, Type.Value, isWhatsappEnable);

                if (_questionService.Status == ResultEnum.Ok)
                {
                    response = new QuizQuestionDetailsResponse();

                    response = (QuizQuestionDetailsResponse)response.MapEntityToResponse(quizQuestionDetailsObj);

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = response, message = _questionService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = response, message = ex.Message }));
            }
        }

        /// <summary>
        /// Update Quiz Answer type
        /// </summary>
        /// <param name="QuestionId"></param>
        /// <param name="AnswerType"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/api/v1/Quiz/UpdateAnswerType")]
        [AuthorizeTokenFilter]
        public IHttpActionResult UpdateAnswerType(int QuestionId, int AnswerType, int? answerStructureType = null, bool isWhatsappEnable = false, bool isMultiRating = false)
        {
            try
            {
                _questionService.UpdateAnswerType(QuestionId, AnswerType, BusinessUserId, CompanyInfo.Id, answerStructureType, isWhatsappEnable, isMultiRating);

                if (_questionService.Status == ResultEnum.Ok)
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Quiz answer type updated.", message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _questionService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Getting Mapped Object for Questions of a Quiz
        /// </summary>
        /// <param name="QuizId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("QuestionMappedFieldValues")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetQuestionMappedFieldValues(int QuizId, string LeadUserId) {
            QuizQuestionDetailsResponse response = null;
            try {
                var quizQuestionMappedDetails = _questionService.GetQuestionMappedFieldValues(QuizId, LeadUserId);

                if (_questionService.Status == ResultEnum.Ok) {

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = quizQuestionMappedDetails, message = string.Empty }));
                } else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = response, message = _questionService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = response, message = ex.Message }));
            }
        }
    }
}
