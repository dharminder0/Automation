using QuizApp.Helpers;
using QuizApp.Response;
using QuizApp.Services.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Swashbuckle.Swagger.Annotations;

namespace QuizApp.Controllers
{
    public class BranchingLogicController : BaseController
    {
        private IBranchingLogicService _BranchingLogicService;

        public BranchingLogicController(IBranchingLogicService iBranchingLogicService)
        {
            _BranchingLogicService = iBranchingLogicService;
        }

        /// <summary>
        /// Get Quiz BranchingLogic Data
        /// </summary>
        /// <param name="QuizId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/v1/BranchingLogic/GetData")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetQuizBranchingLogicData(int QuizId)
        {
            QuizBranchingLogicLinksListResponse response = null;

            try
            {
                var quizBranchingLogicDetails = _BranchingLogicService.GetQuizBranchingLogicData(QuizId);

                if (_BranchingLogicService.Status == ResultEnum.Ok)
                {
                    response = new QuizBranchingLogicLinksListResponse();

                    response = (QuizBranchingLogicLinksListResponse)response.MapEntityToResponse(quizBranchingLogicDetails);

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = response, message = _BranchingLogicService.ErrorMessage }));
                }

            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Get Quiz BranchingLogic Details
        /// </summary>
        /// <param name="QuizId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("~/api/v1/Quiz/GetQuizBranchingLogicDetails")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetQuizBranchingLogicDetails(int QuizId)
        {
            QuizBranchingLogicResponse response = null;

            try
            {
                var quizBranchingLogicDetails = _BranchingLogicService.GetQuizBranchingLogic(QuizId);

                if (_BranchingLogicService.Status == ResultEnum.Ok)
                {
                    response = new QuizBranchingLogicResponse();

                    response = (QuizBranchingLogicResponse)response.MapEntityToResponse(quizBranchingLogicDetails);

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = response, message = _BranchingLogicService.ErrorMessage }));
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
