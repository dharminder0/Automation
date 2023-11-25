using NLog;
using QuizApp.Db;
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
    [RoutePrefix("api/v1/Reporting")]
    [SwaggerResponseRemoveDefaults]
    public class ReportingController : BaseController
    {
        public NLog.Logger Logger = LogManager.GetCurrentClassLogger();
        private IReportingService _iReportingService;
        private IAutomationService _automationService;

        public ReportingController(IReportingService iReportingService, IAutomationService automationService)
        {
            _iReportingService = iReportingService;
            _automationService = automationService;
        }

        /// <summary>
        /// Get quiz report details
        /// </summary>
        /// <param name="QuizIdCSV"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="numerator"></param>
        /// <param name="denominator"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetQuizReportDetails")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetQuizReportDetails(string QuizIdCSV, DateTime fromDate, DateTime toDate, string numerator, string denominator)
        {
            List<QuizReportResponse> responseDataList = null;

            try
            {
                var quizReportLst = _iReportingService.GetQuizReportDetails(QuizIdCSV, fromDate, toDate, numerator, denominator);

                if (_iReportingService.Status == ResultEnum.Ok)
                {
                    responseDataList = new List<QuizReportResponse>();

                    foreach (var quizReportObj in quizReportLst)
                    {
                        QuizReportResponse res = new QuizReportResponse();
                        responseDataList.Add((QuizReportResponse)res.MapEntityToResponse(quizReportObj));
                    }

                    var aggregrateReportingData = new List<QuizReportResponse.ReportAttribute>();
                    var individualReportingData = new List<QuizReportResponse>();

                    aggregrateReportingData = responseDataList.LastOrDefault().data;

                    if (responseDataList.Count > 1) individualReportingData = responseDataList.Take(responseDataList.Count - 1).ToList();

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = new { AggregatedReporting = aggregrateReportingData, IndividualReporting = individualReportingData }, message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iReportingService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// To Get Quiz Report
        /// </summary>
        /// <param name="QuizId"></param>
        /// <param name="SourceId"></param>
        /// <param name="FromDate"></param>
        /// <param name="ToDate"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetQuizReport")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetQuizReport(int QuizId, string SourceId = null, DateTime? FromDate = null, DateTime? ToDate = null, int? ResultId = null)
        {
            ReportResponse response = null;
            try
            {
                var quizReportLst = _iReportingService.GetQuizReport(QuizId, SourceId, FromDate, ToDate, ResultId, CompanyInfo.Id);

                if (_iReportingService.Status == ResultEnum.Ok)
                {
                    response = new ReportResponse();
                    response = (ReportResponse)response.MapEntityToResponse(quizReportLst);

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iReportingService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// GetQuizLeadReport
        /// </summary>
        /// <param name="QuizId"></param>
        /// <param name="LeadUserId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetQuizLeadReport")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetQuizLeadReport(int QuizId, string LeadUserId)
        {
            try
            {
                var quizReportLst = _iReportingService.GetQuizLeadReport(QuizId, LeadUserId, CompanyInfo.Id);

                if (_iReportingService.Status == ResultEnum.Ok)
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = quizReportLst, message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iReportingService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// To Get NPS Quiz Report
        /// </summary>
        /// <param name="QuizId"></param>
        /// <param name="SourceId"></param>
        /// <param name="ChartView"></param>
        /// <param name="FromDate"></param>
        /// <param name="ToDate"></param>
        /// <param name="ResultId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetNPSAutomationReport")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetNPSAutomationReport(int QuizId, string SourceId = null, int? ChartView = 1, DateTime? FromDate = null, DateTime? ToDate = null, int? ResultId = null)
        {
            NPSAutomationReportResponse response = null; 
            try
            {
                var quizReportLst = _iReportingService.GetNPSAutomationReport(QuizId, SourceId, ChartView.Value, FromDate, ToDate, ResultId, CompanyInfo.Id);

                if (_iReportingService.Status == ResultEnum.Ok)
                {
                    response = new NPSAutomationReportResponse();
                    response = (NPSAutomationReportResponse)response.MapEntityToResponse(quizReportLst);

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iReportingService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// To Get Quiz Template Report
        /// </summary>
        /// <param name="TemplateId"></param>
        /// <param name="FromDate"></param>
        /// <param name="ToDate"></param>
        /// <param name="ResultId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetQuizTemplateReport")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetQuizTemplateReport(string TemplateId, DateTime? FromDate = null, DateTime? ToDate = null, int? ResultId = null)
        {
            ReportResponse response = null;
            try
            {
                var quizReportLst = _iReportingService.GetQuizTemplateReport(TemplateId, FromDate, ToDate, ResultId, CompanyInfo);

                if (_iReportingService.Status == ResultEnum.Ok)
                {
                    response = new ReportResponse();
                    response = (ReportResponse)response.MapEntityToResponse(quizReportLst);

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iReportingService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }
        /// <summary>
        /// To Get Quiz Template Lead Report
        /// </summary>
        /// <param name="TemplateId"></param>
        /// <param name="LeadUserId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetQuizTemplateLeadReport")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetTemplateLeadReport(string TemplateId, string LeadUserId)
        {
            try
            {
                var quizReportLst = _iReportingService.GetQuizTemplateLeadReport(TemplateId, LeadUserId, CompanyInfo);

                if (_iReportingService.Status == ResultEnum.Ok)
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = quizReportLst, message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iReportingService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// To Get NPS Template Automation Report
        /// </summary>
        /// <param name="TemplateId"></param>
        /// <param name="ChartView"></param>
        /// <param name="FromDate"></param>
        /// <param name="ToDate"></param>
        /// <param name="ResultId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetNPSTemplateAutomationReport")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetNPSTemplateAutomationReport(string TemplateId, int? ChartView = 1, DateTime? FromDate = null, DateTime? ToDate = null, int? ResultId = null)
        {
            NPSAutomationReportResponse response = null;
            try
            {
                var quizReportLst = _iReportingService.GetNPSTemplateAutomationReport(TemplateId, ChartView.Value, FromDate, ToDate, ResultId, CompanyInfo);

                if (_iReportingService.Status == ResultEnum.Ok)
                {
                    response = new NPSAutomationReportResponse();
                    response = (NPSAutomationReportResponse)response.MapEntityToResponse(quizReportLst);

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iReportingService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// To Get Template Quiz Details
        /// </summary>
        /// <param name="TemplateId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetTemplateQuizDetails")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetTemplateQuizDetails(string TemplateId)
        {
            try
            {
                var quizReportLst = _iReportingService.GetTemplateQuizDetails(TemplateId, CompanyInfo);

                if (_iReportingService.Status == ResultEnum.Ok)
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = quizReportLst, message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iReportingService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Get Questions and Answers of AttemptedQuiz by QuizAttemptId
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAttemptedQuizDetails")]
        public IHttpActionResult GetAttemptedQuizDetails(string LeadUserId, int QuizId, string ConfigurationId) {
            try {

                var data = _automationService.GetAttemptedQuizDetailsV2(LeadUserId, QuizId, ConfigurationId);
                if (_automationService.Status == ResultEnum.Ok) {
                    
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { AttemptedQuizDetails = data, message = "" }));
                } else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { AttemptedQuizDetails = string.Empty, message = _automationService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { AttemptedQuizDetails = string.Empty, message = ex.Message }));
            }
        }
    }
}
