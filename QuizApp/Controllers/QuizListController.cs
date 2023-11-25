using Core.Common.Extensions;
using NLog;
using QuizApp.Helpers;
using QuizApp.Request;
using QuizApp.Response;
using QuizApp.Services.Model;
using QuizApp.Services.Service;
using Swashbuckle.Examples;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using static QuizApp.Helpers.Models;

namespace QuizApp.Controllers
{
    [SwaggerResponseRemoveDefaults]
    public class QuizListController : BaseController
    {
        public NLog.Logger Logger = LogManager.GetCurrentClassLogger();
        private IQuizListService _quizListService;
        public QuizListController(IQuizListService quizListService)

        {
            _quizListService = quizListService;
        }

        /// <summary>
        /// Get Quiz List
        /// </summary>
        /// <param name="IncludeSharedWithMe"></param>
        /// <param name="OffsetValue"></param>
        /// <param name="SearchTxt"></param>
        /// <param name="OrderBy"></param>
        /// <param name="QuizTypeId"></param>
        /// <param name="IsDataforGlobalOfficeAdmin"></param>
        /// <param name="OfficeIdList"></param>
        /// <param name="QuizId"></param>
        /// <param name="IsFavorite"></param>
        /// <param name="IsPublished"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/api/v1/Quiz/GetList")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetList(bool IncludeSharedWithMe, long OffsetValue, string SearchTxt = "", string OrderBy = "", string QuizTypeId = "", bool IsDataforGlobalOfficeAdmin = false, string OfficeIdList = null, string QuizId = "", bool? IsFavorite = null, bool? IsPublished = null, int? QuizTagId = null)
        {
            List<QuizListResponses> responseList = null;
            try
            {
                var quizLst = _quizListService.GetList(BusinessUserId, !String.IsNullOrEmpty(OfficeIdList) ? OfficeIdList.Split(',').ToList() : new List<string>(), IncludeSharedWithMe, OffsetValue, SearchTxt, QuizTypeId, CompanyInfo, IsDataforGlobalOfficeAdmin, IsGlobalOfficeAdmin, UserInfo.Id, QuizId, IsFavorite, IsPublished, UserInfo.IsCreateStandardAutomationPermission, QuizTagId);

                if (_quizListService.Status == ResultEnum.Ok)
                {
                    responseList = new List<QuizListResponses>();
                    foreach (var quizObj in quizLst)
                    {
                        QuizListResponses res = new QuizListResponses();
                        responseList.Add((QuizListResponses)res.MapEntityToResponse(quizObj));
                    }

                    List<OWCBusinessUserResponse> userdetails = new List<OWCBusinessUserResponse>();
                    List<Offices> officedetails = new List<Offices>();
                    long[] Userids = new long[quizLst.Count];

                    for (int i = 0; i < quizLst.Count; i++)
                    {
                        Userids[i] = quizLst[i].CreatedByID;
                    }
                    if (Userids.Any() && (CompanyInfo.ClientCode != "JobRock" && CompanyInfo.ClientCode != "HEMA"))
                    {
                        userdetails = OWCHelper.GetUserListOnUserId(Userids, CompanyInfo).ToList();
                    }
                    //officedetails = OWCHelper.GetOfficeInfo(CompanyInfo).ToList();
                    officedetails = CommonStaticData.GetCachedOfficeInfo(CompanyInfo).ToList();

                    foreach (var response in responseList)
                    {
                        response.CreatedByName = (userdetails != null && userdetails.Any(r => r.userId == response.CreatedById)) ? userdetails.FirstOrDefault(r => r.userId == response.CreatedById).firstName : string.Empty;
                        response.OfficeName = (officedetails != null && officedetails.Any(r => r.id == response.OfficeId)) ? officedetails.FirstOrDefault(r => r.id == response.OfficeId).name : string.Empty;
                    }

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = responseList.OrderByDescending(r => r.createdOn), message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = responseList, message = _quizListService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = responseList, message = ex.Message }));
            }
        }

        /// <summary>
        /// Get Quiz List
        /// </summary>
        /// <param name="IncludeSharedWithMe"></param>
        /// <param name="OffsetValue"></param>
        /// <param name="SearchTxt"></param>
        /// <param name="OrderBy"></param>
        /// <param name="QuizTypeId"></param>
        /// <param name="IsDataforGlobalOfficeAdmin"></param>
        /// <param name="OfficeIdList"></param>
        /// <param name="PageNo"></param>
        /// <param name="PageSize"></param>
        /// <param name="QuizId"></param>
        /// <param name="IsFavorite"></param>
        /// <param name="MustIncludeQuizId"></param>
        /// <param name="UsageType"></param>
        /// <param name="QuizTagId"></param>
        /// <param name="IsWhatsAppChatBotOldVersion"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/api/v2/Quiz/GetList")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetList(bool IncludeSharedWithMe, long OffsetValue, string SearchTxt = "", int? OrderBy = 1, string QuizTypeId = "", bool IsDataforGlobalOfficeAdmin = false, string OfficeIdList = null, int? PageNo = null, int? PageSize = null, string QuizId = "", bool? IsFavorite = null, string MustIncludeQuizId = null, int? UsageType = null, int? QuizTagId = null, bool? IsWhatsAppChatBotOldVersion = null)
        {
            QuizResponse responseList = null;

            try
            {
                var quizLst = _quizListService.GetListWithPagination(BusinessUserId, !String.IsNullOrEmpty(OfficeIdList) ? OfficeIdList.Split(',').ToList() : new List<string>(), IncludeSharedWithMe, OffsetValue, PageNo, PageSize, SearchTxt, OrderBy, QuizTypeId, CompanyInfo, IsDataforGlobalOfficeAdmin, IsGlobalOfficeAdmin, UserInfo.Id, QuizId, IsFavorite, UserInfo.IsCreateStandardAutomationPermission, MustIncludeQuizId, UsageType, QuizTagId, IsWhatsAppChatBotOldVersion);

                if (_quizListService.Status == ResultEnum.Ok)
                {
                    responseList = new QuizResponse();
                    responseList = (QuizResponse)responseList.MapEntityToResponse(quizLst);

                    List<OWCBusinessUserResponse> userdetails = new List<OWCBusinessUserResponse>();
                    List<Offices> officedetails = new List<Offices>();
                    long[] Userids = new long[quizLst.Quiz.Count];

                    for (int i = 0; i < quizLst.Quiz.Count; i++)
                    {
                        Userids[i] = quizLst.Quiz[i].CreatedByID;
                    }
                    if (Userids.Any() && (CompanyInfo.ClientCode != "JobRock" && CompanyInfo.ClientCode != "HEMA"))
                    {
                        userdetails = OWCHelper.GetUserListOnUserId(Userids, CompanyInfo).ToList();
                    }
                    //officedetails = OWCHelper.GetOfficeInfo(CompanyInfo).ToList();
                    officedetails = CommonStaticData.GetCachedOfficeInfo(CompanyInfo).ToList();

                    foreach (var Response in responseList.QuizListResponse)
                    {
                        Response.CreatedByName = (userdetails != null && userdetails.Any(r => r.userId == Response.CreatedById)) ? userdetails.FirstOrDefault(r => r.userId == Response.CreatedById).firstName : string.Empty;
                        Response.OfficeName = (officedetails != null && officedetails.Any(r => r.id == Response.OfficeId)) ? officedetails.FirstOrDefault(r => r.id == Response.OfficeId).name : string.Empty;
                    }

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = responseList, message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = responseList, message = _quizListService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = responseList, message = ex.Message }));
            }
        }

        /// <summary>
        /// Get Quiz List with Pagination
        /// </summary>
        /// <param name="PageNo"></param>
        /// <param name="PageSize"></param>
        /// <param name="IncludeSharedWithMe"></param>
        /// <param name="OffsetValue"></param>
        /// <param name="SearchTxt"></param>
        /// <param name="OrderBy"></param>
        /// <param name="QuizTypeId"></param>
        /// <param name="IsDataforGlobalOfficeAdmin"></param>
        /// <param name="OfficeIdList"></param>
        /// <param name="QuizId"></param>
        /// <param name="IsFavorite"></param>
        /// <param name="IsPublished"></param>
        /// <param name="QuizTagId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/api/v3/Quiz/GetList")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetList(int PageNo, int PageSize, bool IncludeSharedWithMe, long OffsetValue, string SearchTxt = "", int OrderBy = 0, string QuizTypeId = "", bool IsDataforGlobalOfficeAdmin = false, string OfficeIdList = null, string QuizId = "", bool? IsFavorite = null, bool? IsPublished = null, int? QuizTagId = null, int? UsageType = null)
        {
            QuizResponse responseList = null;

            try
            {
                var quizLst = _quizListService.GetAutomationList(PageNo, PageSize, BusinessUserId, !string.IsNullOrEmpty(OfficeIdList) ? OfficeIdList.Split(',').ToList() : new List<string>(), IncludeSharedWithMe, OffsetValue, SearchTxt, OrderBy, QuizTypeId, CompanyInfo, IsDataforGlobalOfficeAdmin, IsGlobalOfficeAdmin, UserInfo.Id, QuizId, IsFavorite, IsPublished, UserInfo.IsCreateStandardAutomationPermission, QuizTagId, UsageType);

                if (_quizListService.Status == ResultEnum.Ok)
                {
                    responseList = new QuizResponse();
                    responseList = (QuizResponse)responseList.MapEntityToResponse(quizLst);

                    List<OWCBusinessUserResponse> userdetails = new List<OWCBusinessUserResponse>();
                    List<Offices> officedetails = new List<Offices>();
                    long[] Userids = new long[quizLst.Quiz.Count];

                    for (int i = 0; i < quizLst.Quiz.Count; i++)
                    {
                        Userids[i] = quizLst.Quiz[i].CreatedByID;
                    }
                    if (Userids.Any() && (CompanyInfo.ClientCode != "JobRock" && CompanyInfo.ClientCode != "HEMA"))
                    {
                        userdetails = OWCHelper.GetUserListOnUserId(Userids, CompanyInfo).ToList();
                    }
                    //officedetails = OWCHelper.GetOfficeInfo(CompanyInfo).ToList();
                    officedetails = CommonStaticData.GetCachedOfficeInfo(CompanyInfo).ToList();

                    foreach (var response in responseList.QuizListResponse)
                    {
                        response.CreatedByName = (userdetails != null && userdetails.Any(r => r.userId == response.CreatedById)) ? userdetails.FirstOrDefault(r => r.userId == response.CreatedById).firstName : string.Empty;
                        response.OfficeName = (officedetails != null && officedetails.Any(r => r.id == response.OfficeId)) ? officedetails.FirstOrDefault(r => r.id == response.OfficeId).name : string.Empty;
                    }

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = responseList, message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = responseList, message = _quizListService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = responseList, message = ex.Message }));
            }
        }

        /// <summary>
        /// Get Quiz List in AutomationReportList 
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/v3/Quiz/AutomationReportList")]
        [AuthorizeTokenFilter]
        public IHttpActionResult AutomationReportList(QuizListRequest requestModel)
        {
            QuizResponse responseList = null;
            try
            {
                var quizLst = _quizListService.AutomationReportList(requestModel, BusinessUserId, CompanyInfo, IsGlobalOfficeAdmin, UserInfo.Id, UserInfo.IsCreateStandardAutomationPermission);
                if (_quizListService.Status == ResultEnum.Ok)
                {
                    responseList = new QuizResponse();
                    responseList = (QuizResponse)responseList.MapEntityToResponse(quizLst);

                    List<OWCBusinessUserResponse> userdetails = new List<OWCBusinessUserResponse>();
                    List<Offices> officedetails = new List<Offices>();
                    long[] Userids = new long[quizLst.Quiz.Count];

                    for (int i = 0; i < quizLst.Quiz.Count; i++)
                    {
                        Userids[i] = quizLst.Quiz[i].CreatedByID;
                    }
                    if (Userids.Any() && (CompanyInfo.ClientCode != "JobRock" && CompanyInfo.ClientCode != "HEMA"))
                    {
                        userdetails = OWCHelper.GetUserListOnUserId(Userids, CompanyInfo).ToList();
                    }
                    officedetails = CommonStaticData.GetCachedOfficeInfo(CompanyInfo).ToList();

                    foreach (var response in responseList.QuizListResponse)
                    {
                        response.CreatedByName = (userdetails != null && userdetails.Any(r => r.userId == response.CreatedById)) ? userdetails.FirstOrDefault(r => r.userId == response.CreatedById).firstName : string.Empty;
                        response.OfficeName = (officedetails != null && officedetails.Any(r => r.id == response.OfficeId)) ? officedetails.FirstOrDefault(r => r.id == response.OfficeId).name : string.Empty;
                    }

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = responseList, message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = responseList, message = _quizListService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = responseList, message = ex.Message }));
            }
        }

        /// <summary>
        /// Get Quiz List in ReportList
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/v1/Automation/ReportList")]
        [AuthorizeTokenFilter]
        public IHttpActionResult ReportList(AutomationListRequest requestModel)
        {
            QuizResponse responseList = null;

            try
            {
                var quizLst = _quizListService.ReportList(requestModel, BusinessUserId, CompanyInfo, IsGlobalOfficeAdmin, UserInfo.Id, UserInfo.IsCreateStandardAutomationPermission);

                if (_quizListService.Status == ResultEnum.Ok)
                {
                    responseList = new QuizResponse();
                    responseList = (QuizResponse)responseList.MapEntityToResponse(quizLst);

                    List<OWCBusinessUserResponse> userdetails = new List<OWCBusinessUserResponse>();
                    List<Offices> officedetails = new List<Offices>();
                    long[] Userids = new long[quizLst.Quiz.Count];

                    for (int i = 0; i < quizLst.Quiz.Count; i++)
                    {
                        Userids[i] = quizLst.Quiz[i].CreatedByID;
                    }
                    if (Userids.Any() && (CompanyInfo.ClientCode != "JobRock" && CompanyInfo.ClientCode != "HEMA"))
                    {
                        userdetails = OWCHelper.GetUserListOnUserId(Userids, CompanyInfo).ToList();
                    }
                    //officedetails = OWCHelper.GetOfficeInfo(CompanyInfo).ToList();
                    officedetails = CommonStaticData.GetCachedOfficeInfo(CompanyInfo).ToList();

                    foreach (var Response in responseList.QuizListResponse)
                    {
                        Response.CreatedByName = (userdetails != null && userdetails.Any(r => r.userId == Response.CreatedById)) ? userdetails.FirstOrDefault(r => r.userId == Response.CreatedById).firstName : string.Empty;
                        Response.OfficeName = (officedetails != null && officedetails.Any(r => r.id == Response.OfficeId)) ? officedetails.FirstOrDefault(r => r.id == Response.OfficeId).name : string.Empty;
                    }

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = responseList, message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = responseList, message = _quizListService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = responseList, message = ex.Message }));
            }
        }
    }
}