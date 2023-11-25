using NLog;
using QuizApp.Helpers;
using QuizApp.Response;
using QuizApp.Services.Model;
using QuizApp.Services.Service;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace QuizApp.Controllers {
    [RoutePrefix("api/v1/QuizAttempt")]
    [SwaggerResponseRemoveDefaults]
    public class QuizAttemptController : BaseController {

        public NLog.Logger Logger = LogManager.GetCurrentClassLogger();
        private IQuizAttemptService _quizAttemptService;
        private IUncompleteQuizService _uncompleteQuizService;
        public QuizAttemptController(IQuizAttemptService quizAttemptService, IUncompleteQuizService uncompleteQuizService) {
            _quizAttemptService = quizAttemptService;
            _uncompleteQuizService = uncompleteQuizService;
        }


        [HttpPost]
        [Route("AttemptQuizSettings")]
        [AuthorizeTokenFilter]
        public IHttpActionResult AttemptQuizSettings(string QuizCode) {
            AttemptQuizSettingResponse response = null;

            try {

                var quizDetails = _quizAttemptService.AttemptQuizSettings(QuizCode);

                if (_quizAttemptService.Status == ResultEnum.Ok) {
                    //response = new AttemptQuizSettingResponse();

                    //response = (AttemptQuizSettingResponse)response.MapEntityToResponse(quizDetails);

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = quizDetails, message = string.Empty }));
                } else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = quizDetails, message = _quizAttemptService.ErrorMessage }));
                }

            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Attempt Quiz
        /// <returns></returns>
        [HttpPost]
        [Route("AttemptQuiz")]
        [AuthorizeTokenFilter]
        public IHttpActionResult AttemptQuiz(AttemptQuizRequest attemptQuizRequest) {
            AttemptQuizV2Response response = null;
            try {
                QuizAnswerSubmit quizDetails = null;
                quizDetails = _quizAttemptService.AttemptQuiz(attemptQuizRequest);


                if (_quizAttemptService.Status == ResultEnum.Ok) {
                    response = new AttemptQuizV2Response();

                    response = (AttemptQuizV2Response)response.MapEntityToResponse(quizDetails);

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
                } else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = response, message = _quizAttemptService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }



        /// <summary>
        /// Uncomplete Quiz stats
        /// <returns></returns>
        [HttpPost]
        [Route("MarkQuizIncomplete")]
        [AuthorizeTokenFilter]
        public IHttpActionResult MarkQuizIncomplete(int quizAttemptId) {
            try {
                _uncompleteQuizService.MarkQuizIncomplete(quizAttemptId);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { message = string.Empty }));
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

    }
}
