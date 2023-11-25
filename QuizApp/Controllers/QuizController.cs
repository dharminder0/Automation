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

namespace QuizApp.Controllers {
    [RoutePrefix("api/v1/Quiz")]
    [SwaggerResponseRemoveDefaults]
    public class QuizController : BaseController {
        public NLog.Logger Logger = LogManager.GetCurrentClassLogger();
        private IQuizService _iQuizService;
        private IPublishQuizService _publishQuizService = null;
        private IAutomationService _automationService;
        private IGenericAutomationService _genericAutomationService;
        private IQuizAttemptService _quizAttemptService;
        private IBranchingLogicService _branchingLogicService;
        private IQuizCoverService _quizCoverService;
        private IUpdateBrandingService _updateBrandingService;
        private IQuizListService _quizListService;
        public QuizController(IQuizListService quizListService, IQuizService iQuizService, IAutomationService automationService,
            IQuizCoverService quizCoverService, IBranchingLogicService branchingLogicService, IGenericAutomationService genericAutomationService, IQuizAttemptService quizAttemptService, IUpdateBrandingService updateBrandingService, IPublishQuizService publishQuizService) {
            _iQuizService = iQuizService;
            _automationService = automationService;
            _genericAutomationService = genericAutomationService;
            _quizAttemptService = quizAttemptService;
            _quizListService = quizListService;
            _branchingLogicService = branchingLogicService;
            _quizCoverService = quizCoverService;
            _updateBrandingService = updateBrandingService;
            _publishQuizService = publishQuizService;
        }

        /// <summary>
        /// Create a new Quiz
        /// </summary>
        /// <returns></returns>
        /// <param name="Obj"></param>
        [HttpPost]
        [Route("CreateQuiz")]
        [AuthorizeTokenFilter]
        [SwaggerRequestExample(typeof(AddQuizRequest), typeof(AddQuizRequest.AddQuizRequestExample))]
        public IHttpActionResult CreateQuiz(AddQuizRequest Obj) {
            try {
                AddQuizRequest addRequest = new AddQuizRequest();

                if ((Obj.QuizType == (int)QuizTypeEnum.Assessment || Obj.QuizType == (int)QuizTypeEnum.Score || Obj.QuizType == (int)QuizTypeEnum.Personality || Obj.QuizType == (int)QuizTypeEnum.NPS) || (CompanyInfo.CreateTemplateEnabled && CreateTemplate)) {
                    var quizId = 0;

                    quizId = _genericAutomationService.CreateQuiz(addRequest.MapRequestToEntity(Obj, UserInfo.IsCreateStandardAutomationPermission), BusinessUserId, (CompanyInfo.CreateAcademyCourseEnabled && CreateAcademyCourse), (CompanyInfo.CreateTechnicalRecruiterCourseEnabled && UserInfo.CreateTechnicalRecruiterCourse), CompanyInfo.Id);

                    if (_genericAutomationService.Status == ResultEnum.Ok) {
                        return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = quizId, message = string.Empty }));
                    }
                    else {
                        return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = -1, message = _iQuizService.ErrorMessage }));
                    }
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.Unauthorized, new { data = -1, message = "Unauthorized" }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = -1, message = ex.Message }));
            }
        }

        /// <summary>
        /// Update Quiz
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateQuiz")]
        [SwaggerRequestExample(typeof(UpdateQuizRequest), typeof(UpdateQuizRequest.UpdateQuizRequestExample))]
        [AuthorizeTokenFilter]
        public IHttpActionResult UpdateQuiz(UpdateQuizRequest Obj) {
            try {
                UpdateQuizRequest quizRequest = new UpdateQuizRequest();

                _iQuizService.UpdateQuiz(quizRequest.MapRequestToEntity(Obj), BusinessUserId, CompanyInfo.Id);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Quiz updated", message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        ///// <summary>
        ///// Get Quiz List
        ///// </summary>
        ///// <param name="IncludeSharedWithMe"></param>
        ///// <param name="OffsetValue"></param>
        ///// <param name="SearchTxt"></param>
        ///// <param name="OrderBy"></param>
        ///// <param name="QuizTypeId"></param>
        ///// <param name="IsDataforGlobalOfficeAdmin"></param>
        ///// <param name="OfficeIdList"></param>
        ///// <param name="QuizId"></param>
        ///// <param name="IsFavorite"></param>
        ///// <param name="IsPublished"></param>
        ///// <returns></returns>
        //[HttpPost]
        //[Route("GetList")]
        //[AuthorizeTokenFilter]
        //public IHttpActionResult GetList(bool IncludeSharedWithMe, long OffsetValue, string SearchTxt = "", string OrderBy = "", string QuizTypeId = "", bool IsDataforGlobalOfficeAdmin = false, string OfficeIdList = null, string QuizId = "", bool? IsFavorite = null, bool? IsPublished = null, int? QuizTagId = null)
        //{
        //    List<QuizListResponses> responseList = null;

        //    try
        //    {
        //        var quizLst = _iQuizService.GetList(BusinessUserId, !String.IsNullOrEmpty(OfficeIdList) ? OfficeIdList.Split(',').ToList() : new List<string>(), IncludeSharedWithMe, OffsetValue, SearchTxt, QuizTypeId, CompanyInfo, IsDataforGlobalOfficeAdmin, IsGlobalOfficeAdmin, UserInfo.Id, QuizId, IsFavorite, IsPublished, UserInfo.IsCreateStandardAutomationPermission, QuizTagId);

        //        if (_iQuizService.Status == ResultEnum.Ok)
        //        {
        //            responseList = new List<QuizListResponses>();
        //            foreach (var quizObj in quizLst)
        //            {
        //                QuizListResponses res = new QuizListResponses();
        //                responseList.Add((QuizListResponses)res.MapEntityToResponse(quizObj));
        //            }

        //            List<OWCBusinessUserResponse> userdetails = new List<OWCBusinessUserResponse>();
        //            List<Offices> officedetails = new List<Offices>();
        //            long[] Userids = new long[quizLst.Count];

        //            for (int i = 0; i < quizLst.Count; i++)
        //            {
        //                Userids[i] = quizLst[i].CreatedByID;
        //            }
        //            if (Userids.Any() && (CompanyInfo.ClientCode != "JobRock" && CompanyInfo.ClientCode != "HEMA"))
        //            {
        //                userdetails = OWCHelper.GetUserListOnUserId(Userids, CompanyInfo).ToList();
        //            }
        //            //officedetails = OWCHelper.GetOfficeInfo(CompanyInfo).ToList();
        //            officedetails = CommonStaticData.GetCachedOfficeInfo(CompanyInfo).ToList();

        //            foreach (var response in responseList)
        //            {
        //                response.CreatedByName = (userdetails != null && userdetails.Any(r => r.userId == response.CreatedById)) ? userdetails.FirstOrDefault(r => r.userId == response.CreatedById).firstName : string.Empty;
        //                response.OfficeName = (officedetails != null && officedetails.Any(r => r.id == response.OfficeId)) ? officedetails.FirstOrDefault(r => r.id == response.OfficeId).name : string.Empty;
        //            }

        //            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = responseList.OrderByDescending(r => r.createdOn), message = string.Empty }));
        //        }
        //        else
        //        {
        //            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = responseList, message = _iQuizService.ErrorMessage }));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLog.LogError(ex);
        //        return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = responseList, message = ex.Message }));
        //    }
        //}

        ///// <summary>
        ///// Get Quiz List with Pagination
        ///// </summary>
        ///// <param name="PageNo"></param>
        ///// <param name="PageSize"></param>
        ///// <param name="IncludeSharedWithMe"></param>
        ///// <param name="OffsetValue"></param>
        ///// <param name="SearchTxt"></param>
        ///// <param name="OrderBy"></param>
        ///// <param name="QuizTypeId"></param>
        ///// <param name="IsDataforGlobalOfficeAdmin"></param>
        ///// <param name="OfficeIdList"></param>
        ///// <param name="QuizId"></param>
        ///// <param name="IsFavorite"></param>
        ///// <param name="IsPublished"></param>
        ///// <param name="QuizTagId"></param>
        ///// <returns></returns>
        //[HttpPost]
        //[Route("~/api/v3/Quiz/GetList")]
        //[AuthorizeTokenFilter]
        //public IHttpActionResult GetList(int PageNo, int PageSize, bool IncludeSharedWithMe, long OffsetValue, string SearchTxt = "", int OrderBy = 0, string QuizTypeId = "", bool IsDataforGlobalOfficeAdmin = false, string OfficeIdList = null, string QuizId = "", bool? IsFavorite = null, bool? IsPublished = null, int? QuizTagId = null, int? UsageType = null)
        //{
        //    QuizResponse responseList = null;

        //    try
        //    {
        //        var quizLst = _iQuizService.GetAutomationList(PageNo, PageSize, BusinessUserId, !string.IsNullOrEmpty(OfficeIdList) ? OfficeIdList.Split(',').ToList() : new List<string>(), IncludeSharedWithMe, OffsetValue, SearchTxt, OrderBy, QuizTypeId, CompanyInfo, IsDataforGlobalOfficeAdmin, IsGlobalOfficeAdmin, UserInfo.Id, QuizId, IsFavorite, IsPublished, UserInfo.IsCreateStandardAutomationPermission, QuizTagId, UsageType);

        //        if (_iQuizService.Status == ResultEnum.Ok)
        //        {
        //            responseList = new QuizResponse();
        //            responseList = (QuizResponse)responseList.MapEntityToResponse(quizLst);

        //            List<OWCBusinessUserResponse> userdetails = new List<OWCBusinessUserResponse>();
        //            List<Offices> officedetails = new List<Offices>();
        //            long[] Userids = new long[quizLst.Quiz.Count];

        //            for (int i = 0; i < quizLst.Quiz.Count; i++)
        //            {
        //                Userids[i] = quizLst.Quiz[i].CreatedByID;
        //            }
        //            if (Userids.Any() && (CompanyInfo.ClientCode != "JobRock" && CompanyInfo.ClientCode != "HEMA"))
        //            {
        //                userdetails = OWCHelper.GetUserListOnUserId(Userids, CompanyInfo).ToList();
        //            }
        //            //officedetails = OWCHelper.GetOfficeInfo(CompanyInfo).ToList();
        //            officedetails = CommonStaticData.GetCachedOfficeInfo(CompanyInfo).ToList();

        //            foreach (var response in responseList.QuizListResponse)
        //            {
        //                response.CreatedByName = (userdetails != null && userdetails.Any(r => r.userId == response.CreatedById)) ? userdetails.FirstOrDefault(r => r.userId == response.CreatedById).firstName : string.Empty;
        //                response.OfficeName = (officedetails != null && officedetails.Any(r => r.id == response.OfficeId)) ? officedetails.FirstOrDefault(r => r.id == response.OfficeId).name : string.Empty;
        //            }

        //            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = responseList, message = string.Empty }));
        //        }
        //        else
        //        {
        //            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = responseList, message = _iQuizService.ErrorMessage }));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLog.LogError(ex);
        //        return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = responseList, message = ex.Message }));
        //    }
        //}


        /// <summary>
        /// Update Branding and Style
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateBrandingAndStyle")]
        [AuthorizeTokenFilter]
        [SwaggerRequestExample(typeof(BrandingAndStyleRequest), typeof(BrandingAndStyleRequest.BrandingAndStyleRequestExample))]
        public IHttpActionResult UpdateBrandingAndStyle(BrandingAndStyleRequest Obj) {
            try {
                BrandingAndStyleRequest brandingAndStyleRequestRequest = new BrandingAndStyleRequest();

                //_iQuizService.UpdateBrandingAndStyle(brandingAndStyleRequestRequest.MapRequestToEntity(Obj), BusinessUserId, CompanyInfo.Id);
                _updateBrandingService.UpdateBrandingAndStyle(brandingAndStyleRequestRequest.MapRequestToEntity(Obj), BusinessUserId, CompanyInfo.Id);

                //if (_iQuizService.Status == ResultEnum.Ok)
                if (_updateBrandingService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Branding and style updated", message = string.Empty }));
                }
                else {
                    //return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _updateBrandingService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Get Quiz Branding and Style
        /// </summary>
        /// <param name="QuizId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetQuizBrandingAndStyle")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetQuizBrandingAndStyle(int QuizId) {
            QuizBrandingAndStyleResponse response = null;
            try {
                var brandingAndStyleObj = _iQuizService.GetQuizBrandingAndStyle(QuizId);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    response = new QuizBrandingAndStyleResponse();

                    response = (QuizBrandingAndStyleResponse)response.MapEntityToResponse(brandingAndStyleObj);

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = response, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = response, message = ex.Message }));
            }
        }

        /// <summary>
        /// Update Quiz Access Setting
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateQuizAccessSetttings")]
        [AuthorizeTokenFilter]
        [SwaggerRequestExample(typeof(QuizAccessSettingRequest), typeof(QuizAccessSettingRequest.QuizAccessSettingRequestExample))]
        public IHttpActionResult UpdateQuizAccessSetttings(QuizAccessSettingRequest Obj) {
            try {
                QuizAccessSettingRequest quizAccessSettingRequestRequest = new QuizAccessSettingRequest();

                _iQuizService.UpdateQuizAccessSetting(quizAccessSettingRequestRequest.MapRequestToEntity(Obj), BusinessUserId, CompanyInfo.Id);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Quiz access setting updated", message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Get Quiz Access Setting
        /// </summary>
        /// <param name="QuizId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetQuizAccessSetting")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetQuizAccessSetting(int QuizId) {
            QuizAccessSettingResponse response = null;
            try {
                var accessSettingObj = _iQuizService.GetQuizAccessSetting(QuizId);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    response = new QuizAccessSettingResponse();

                    response = (QuizAccessSettingResponse)response.MapEntityToResponse(accessSettingObj);

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = response, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = response, message = ex.Message }));
            }
        }

        /// <summary>
        /// Update Quiz Social Share Setting
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateQuizSocialShareSetting")]
        [AuthorizeTokenFilter]
        [SwaggerRequestExample(typeof(QuizSocialShareSettingRequest), typeof(QuizSocialShareSettingRequest.QuizSocialShareSettingRequestExample))]
        public IHttpActionResult UpdateQuizSocialShareSetting(QuizSocialShareSettingRequest Obj) {
            try {
                QuizSocialShareSettingRequest quizSocialShareSettingRequest = new QuizSocialShareSettingRequest();

                _iQuizService.UpdateQuizSocialShareSetting(quizSocialShareSettingRequest.MapRequestToEntity(Obj), BusinessUserId);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Quiz social share setting updated", message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Get Quiz Social Share Setting
        /// </summary>
        /// <param name="QuizId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetQuizSocialShareSetting")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetQuizSocialShareSetting(int QuizId) {
            QuizSocialShareSettingResponse response = null;
            try {
                var socialShareSettingObj = _iQuizService.GetQuizSocialShareSetting(QuizId);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    response = new QuizSocialShareSettingResponse();

                    response = (QuizSocialShareSettingResponse)response.MapEntityToResponse(socialShareSettingObj);

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = response, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = response, message = ex.Message }));
            }
        }

        /// <summary>
        /// Get Quiz Cover Details
        /// </summary>
        /// <param name="QuizId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetQuizCoverDetails")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetQuizCoverDetails(int QuizId) {
            QuizCoverDetailsResponse response = null;
            try {
                var coverDetailsObj = _quizCoverService.GetQuizCoverDetails(QuizId);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    response = new QuizCoverDetailsResponse();

                    response = (QuizCoverDetailsResponse)response.MapEntityToResponse(coverDetailsObj);

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = response, message = _quizCoverService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = response, message = ex.Message }));
            }
        }

        /// <summary>
        /// Update Quiz Cover Details
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateQuizCoverDetails")]
        [SwaggerRequestExample(typeof(QuizCoverDetailRequest), typeof(QuizCoverDetailRequest.QuizCoverDetailRequestExample))]
        [AuthorizeTokenFilter]
        public IHttpActionResult UpdateQuizCoverDetails(QuizCoverDetailRequest Obj) {
            try {
                QuizCoverDetailRequest quizCoverDetailsRequest = new QuizCoverDetailRequest();

                _quizCoverService.UpdateQuizCoverDetails(quizCoverDetailsRequest.MapRequestToEntity(Obj), BusinessUserId, CompanyInfo.Id);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Quiz cover details updated", message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _quizCoverService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Update Quiz Cover Image
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateQuizCoverImage")]
        [SwaggerRequestExample(typeof(QuizCoverImageRequest), typeof(QuizCoverImageRequest.QuizCoverImageRequestExample))]
        [AuthorizeTokenFilter]
        public IHttpActionResult UpdateQuizCoverImage(QuizCoverImageRequest Obj) {
            try {
                QuizCoverImageRequest quizCoverImageRequest = new QuizCoverImageRequest();

                _quizCoverService.UpdateQuizCoverImage(quizCoverImageRequest.MapRequestToEntity(Obj), BusinessUserId, CompanyInfo.Id);

                if (_quizCoverService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Quiz cover details updated", message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _quizCoverService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        ///// <summary>
        ///// Upload Image
        ///// </summary>
        ///// <returns></returns>
        //[HttpPost]
        //[Route("UploadImage")]
        //public Task<HttpResponseMessage> UploadImage()
        //{
        //    // Check if the request contains multipart/form-data.
        //    if (!Request.Content.IsMimeMultipartContent())
        //    {
        //        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
        //    }

        //    var folderName = Guid.NewGuid().ToString();
        //    var destinationFolderPath = HttpContext.Current.Server.MapPath("~/UploadedFiles/" + folderName);

        //    if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/UploadedFiles/" + folderName)))  // if it doesn't exist, create
        //        Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/UploadedFiles/" + folderName));            

        //    var returnUrlPath = this.Request.RequestUri.Scheme + "://" + this.Request.RequestUri.Authority + "/UploadedFiles/" + folderName + "/";

        //    string root = HttpContext.Current.Server.MapPath("~/App_Data");
        //    var provider = new MultipartFormDataStreamProvider(root);

        //    // Read the form data and return an async task.
        //    var task = Request.Content.ReadAsMultipartAsync(provider).
        //        ContinueWith<HttpResponseMessage>(t =>
        //        {
        //            if (t.IsFaulted || t.IsCanceled)
        //            {
        //                Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
        //            }

        //            // This illustrates how to get the file names.
        //            foreach (MultipartFileData file in provider.FileData)
        //            {
        //                foreach (MultipartFileData fileData in provider.FileData)
        //                {
        //                    string fileName = fileData.Headers.ContentDisposition.FileName;

        //                    if (fileName.StartsWith("\"") && fileName.EndsWith("\""))
        //                    {
        //                        fileName = fileName.Trim('"');
        //                    }
        //                    if (fileName.Contains(@"/") || fileName.Contains(@"\"))
        //                    {
        //                        fileName = Path.GetFileName(fileName);
        //                    }

        //                    File.Move(fileData.LocalFileName, Path.Combine(destinationFolderPath, fileName));
        //                }
        //            }

        //            string[] files = Directory.GetFiles(destinationFolderPath);

        //            return Request.CreateResponse(HttpStatusCode.OK, new { data = returnUrlPath + Path.GetFileName(files[0]), message = string.Empty });
        //        });

        //    return task;
        //}

        /// <summary>
        /// Upload Image
        /// </summary>
        /// <param name="ImageString"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UploadImage")]
        [AuthorizeTokenFilter]
        public IHttpActionResult UploadImage(string ImageString) {
            try {
                string path = HttpContext.Current.Server.MapPath("~/UploadedFiles"); //Path

                string ImgName = Guid.NewGuid().ToString();

                //Check if directory exist
                if (!System.IO.Directory.Exists(path)) {
                    System.IO.Directory.CreateDirectory(path); //Create directory if it doesn't exist
                }

                string imageName = ImgName + ".jpg";

                string imgPath = Path.Combine(path, imageName);

                byte[] imageBytes = Convert.FromBase64String(ImageString.Split(',')[1]);

                File.WriteAllBytes(imgPath, imageBytes);

                var returnUrlPath = this.Request.RequestUri.Scheme + "://" + this.Request.RequestUri.Authority + "/UploadedFiles/" + imageName;

                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = returnUrlPath, message = string.Empty }));

            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Get Quiz Question Details
        /// </summary>
        /// <param name="QuestionId"></param>
        /// <param name="QuizId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetQuizQuestionDetails")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetQuizQuestionDetails(int QuestionId, int QuizId) {
            QuizQuestionDetailsResponse response = null;
            try {
                var quizQuestionDetailsObj = _iQuizService.GetQuizQuestionDetails(QuestionId, QuizId);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    response = new QuizQuestionDetailsResponse();

                    response = (QuizQuestionDetailsResponse)response.MapEntityToResponse(quizQuestionDetailsObj);

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = response, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = response, message = ex.Message }));
            }
        }

        ///// <summary>
        ///// Add Quiz Question
        ///// </summary>
        ///// <param name="QuizId"></param>
        ///// <returns></returns>
        //[HttpPost]
        //[Route("AddQuizQuestion")]
        //[AuthorizeTokenFilter]
        //public IHttpActionResult AddQuizQuestion(int QuizId, int? Type = (int)BranchingLogicEnum.QUESTION, bool isWhatsappEnable = false)
        //{
        //    QuizQuestionDetailsResponse response = null;
        //    try
        //    {
        //        //var quizQuestionDetailsObj = _iQuizService.AddQuizQuestion(QuizId, BusinessUserId, CompanyInfo.Id, Type.Value, isWhatsappEnable);
        //        var quizQuestionDetailsObj = _questionService.AddQuizQuestion(QuizId, BusinessUserId, CompanyInfo.Id, Type.Value, isWhatsappEnable);

        //        if (_questionService.Status == ResultEnum.Ok)
        //        {
        //            response = new QuizQuestionDetailsResponse();

        //            response = (QuizQuestionDetailsResponse)response.MapEntityToResponse(quizQuestionDetailsObj);

        //            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
        //        }
        //        else
        //        {
        //            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = response, message = _iQuizService.ErrorMessage }));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLog.LogError(ex);
        //        return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = response, message = ex.Message }));
        //    }
        //}

        ///// <summary>
        ///// Add Quiz Question
        ///// </summary>
        ///// <param name="QuizId"></param>
        ///// <param name="templateId"></param>
        ///// <param name="language"></param>
        ///// <returns></returns>
        //[HttpPost]
        //[Route("AddQuizWhatsAppTemplate")]
        //[AuthorizeTokenFilter]
        //public IHttpActionResult AddQuizWhatsAppTemplate(int QuizId, int templateId, string language)
        //{
        //    QuizQuestionDetailsResponse response = null;
        //    try
        //    {
        //        var quizQuestionDetailsObj = _iQuizService.AddQuizWhatsAppTemplate(QuizId, templateId, language, BusinessUserId, CompanyInfo.Id, CompanyInfo.ClientCode, (int)BranchingLogicEnum.WHATSAPPTEMPLATE);

        //        if (_iQuizService.Status == ResultEnum.Ok)
        //        {
        //            response = new QuizQuestionDetailsResponse();

        //            response = (QuizQuestionDetailsResponse)response.MapEntityToResponse(quizQuestionDetailsObj);

        //            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
        //        }
        //        else
        //        {
        //            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = response, message = _iQuizService.ErrorMessage }));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLog.LogError(ex);
        //        return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = response, message = ex.Message }));
        //    }
        //}

        /// <summary>
        /// Update Quiz Correct Answer Setting
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateQuizCorrectAnswerSetting")]
        [AuthorizeTokenFilter]
        [SwaggerRequestExample(typeof(QuizCorrectAnswerSettingRequest), typeof(QuizCorrectAnswerSettingRequest.QuizCorrectAnswerSettingRequestExample))]
        public IHttpActionResult UpdateQuizCorrectAnswerSetting(QuizCorrectAnswerSettingRequest Obj) {
            try {
                QuizCorrectAnswerSettingRequest quizCorrectAnswerSettingRequest = new QuizCorrectAnswerSettingRequest();

                _iQuizService.UpdateQuizCorrectAnswerSetting(quizCorrectAnswerSettingRequest.MapRequestToEntity(Obj), BusinessUserId, CompanyInfo.Id);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Quiz correct answer setting updated", message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Update Quiz Question Details
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateQuizQuestionDetails")]
        [AuthorizeTokenFilter]
        [SwaggerRequestExample(typeof(QuizQuestionDetailsRequest), typeof(QuizQuestionDetailsRequest.QuizQuestionDetailsRequestExample))]
        public IHttpActionResult UpdateQuizQuestionDetails(QuizQuestionDetailsRequest Obj, bool isWhatsappEnable = false) {
            try {
                QuizQuestionDetailsRequest quizQuestionDetailsRequest = new QuizQuestionDetailsRequest();

                _iQuizService.UpdateQuizQuestionDetails(quizQuestionDetailsRequest.MapRequestToEntity(Obj), BusinessUserId, CompanyInfo.Id, isWhatsappEnable);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Quiz question details updated", message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Add Answer Option in Question
        /// </summary>
        /// <param name="QuestionId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddAnswerOptionInQuestion")]
        [AuthorizeTokenFilter]
        public IHttpActionResult AddAnswerOptionInQuestion(int QuestionId) {
            AnswerOptionInQuestionResponse response = null;
            try {
                var answerOptionInQuestionObj = _iQuizService.AddAnswerOptionInQuestion(QuestionId, BusinessUserId, CompanyInfo.Id);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    response = new AnswerOptionInQuestionResponse();

                    response = (AnswerOptionInQuestionResponse)response.MapEntityToResponse(answerOptionInQuestionObj);

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = response, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = response, message = ex.Message }));
            }
        }

        /// <summary>
        /// Remove Answer
        /// </summary>
        /// <param name="AnswerId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("RemoveAnswer")]
        [AuthorizeTokenFilter]
        public IHttpActionResult RemoveAnswer(int AnswerId) {
            try {
                _iQuizService.RemoveAnswer(AnswerId, BusinessUserId, CompanyInfo.Id);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Answer removed.", message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Remove Question
        /// </summary>
        /// <param name="QuestionId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("RemoveQuestion")]
        [AuthorizeTokenFilter]
        public IHttpActionResult RemoveQuestion(int QuestionId) {
            try {
                _iQuizService.RemoveQuestion(QuestionId, BusinessUserId, CompanyInfo.Id);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Question removed.", message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Reorder Question Answer
        /// </summary>
        /// <param name="IsQuesAndContentInSameTable"></param>
        /// <param name="Obj"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ReorderQuestionAnswerAndContent")]
        [AuthorizeTokenFilter]
        [SwaggerRequestExample(typeof(QuizReorderQuestionAnswerRequest), typeof(QuizReorderQuestionAnswerRequest.QuizReorderQuestionAnswerRequestExample))]
        public IHttpActionResult ReorderQuestionAnswerAndContent(bool IsQuesAndContentInSameTable, List<QuizReorderQuestionAnswerRequest> Obj) {
            try {
                QuizReorderQuestionAnswerRequest requestObj = new QuizReorderQuestionAnswerRequest();

                _iQuizService.ReorderQuestionAnswerAndContent(IsQuesAndContentInSameTable, requestObj.MapRequestToEntity(Obj), BusinessUserId, CompanyInfo.Id);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Order updated.", message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Reorder Answer
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ReorderAnswer")]
        [AuthorizeTokenFilter]
        [SwaggerRequestExample(typeof(QuizReorderAnswerRequest), typeof(QuizReorderAnswerRequest.QuizReorderAnswerRequestExample))]
        public IHttpActionResult ReorderAnswer(QuizReorderAnswerRequest Obj) {
            try {
                QuizReorderAnswerRequest requestObj = new QuizReorderAnswerRequest();

                _iQuizService.ReorderAnswer(requestObj.MapRequestToEntity(Obj), BusinessUserId, CompanyInfo.Id);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Order updated.", message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Add Quiz Result
        /// </summary>
        /// <param name="QuizId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddQuizResult")]
        [AuthorizeTokenFilter]
        public IHttpActionResult AddQuizResult(int QuizId) {
            QuizResultResponse response = null;

            try {
                QuizResult quizResult = null;

                quizResult = _automationService.AddQuizResult(QuizId, BusinessUserId, CompanyInfo.Id);

                if (_automationService.Status == ResultEnum.Ok) {
                    response = new QuizResultResponse();

                    response = (QuizResultResponse)response.MapEntityToResponse(quizResult);

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = response, message = string.Empty }));
                }

            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Update Quiz Result
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateQuizResult")]
        [AuthorizeTokenFilter]
        [SwaggerRequestExample(typeof(QuizResultRequest), typeof(QuizResultRequest.QuizResultRequestExample))]
        public IHttpActionResult UpdateQuizResult(QuizResultRequest Obj) {
            try {
                QuizResultRequest requestObj = new QuizResultRequest();

                _iQuizService.UpdateQuizResult(requestObj.MapRequestToEntity(Obj), BusinessUserId, CompanyInfo.Id);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Quiz result updated.", message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Update Quiz Result Setting
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateResultSetting")]
        [AuthorizeTokenFilter]
        [SwaggerRequestExample(typeof(QuizResultSettingRequest), typeof(QuizResultSettingRequest.QuizResultSettingRequesttExample))]
        public IHttpActionResult UpdateResultSetting(QuizResultSettingRequest Obj) {
            try {
                QuizResultSettingRequest requestObj = new QuizResultSettingRequest();

                _iQuizService.UpdateResultSetting(requestObj.MapRequestToEntity(Obj), BusinessUserId, CompanyInfo.Id);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Quiz result setting updated.", message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Get Quiz Result Details
        /// </summary>
        /// <param name="ResultId"></param>
        /// <param name="QuizId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetQuizResultDetails")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetQuizResultDetails(int ResultId, int QuizId) {
            QuizResultResponse response = null;

            try {
                var quizResult = _iQuizService.GetQuizResultDetails(ResultId, QuizId);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    response = new QuizResultResponse();

                    response = (QuizResultResponse)response.MapEntityToResponse(quizResult);

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = response, message = _iQuizService.ErrorMessage }));
                }

            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Remove Quiz Result
        /// </summary>
        /// <param name="ResultId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("RemoveQuizResult")]
        [AuthorizeTokenFilter]
        public IHttpActionResult RemoveQuizResult(int ResultId) {
            try {
                _iQuizService.RemoveQuizResult(ResultId, BusinessUserId, CompanyInfo.Id);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Quiz result removed.", message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Get Quiz Details
        /// </summary>
        /// <param name="QuizId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetQuizDetails")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetQuizDetails(int QuizId) {
            QuizDetailResponse response = null;

            try {
                QuizDetailsModel quizDetails = null;

                quizDetails = _automationService.GetQuizDetails(QuizId);

                if (_automationService.Status == ResultEnum.Ok) {
                    response = new QuizDetailResponse();

                    response = (QuizDetailResponse)response.MapEntityToResponse(quizDetails);

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = response, message = _automationService.ErrorMessage }));
                }

            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        ///// <summary>
        ///// Get Quiz BranchingLogic Details
        ///// </summary>
        ///// <param name="QuizId"></param>
        ///// <returns></returns>
        //[HttpGet]
        //[Route("GetQuizBranchingLogicDetails")]
        //[AuthorizeTokenFilter]
        //public IHttpActionResult GetQuizBranchingLogicDetails(int QuizId)
        //{
        //    QuizBranchingLogicResponse response = null;

        //    try
        //    {
        //        var quizBranchingLogicDetails = _branchingLogicService.GetQuizBranchingLogic(QuizId);

        //        if (_branchingLogicService.Status == ResultEnum.Ok)
        //        {
        //            response = new QuizBranchingLogicResponse();

        //            response = (QuizBranchingLogicResponse)response.MapEntityToResponse(quizBranchingLogicDetails);

        //            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
        //        }
        //        else
        //        {
        //            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = response, message = _branchingLogicService.ErrorMessage }));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLog.LogError(ex);
        //        return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
        //    }
        //}

        /// <summary>
        /// Get Quiz BranchingLogic Data
        /// </summary>
        /// <param name="QuizId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetQuizBranchingLogicData")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetQuizBranchingLogicData(int QuizId) {
            QuizBranchingLogicLinksListResponse response = null;

            try {
                var quizBranchingLogicDetails = _iQuizService.GetQuizBranchingLogicData(QuizId);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    response = new QuizBranchingLogicLinksListResponse();

                    response = (QuizBranchingLogicLinksListResponse)response.MapEntityToResponse(quizBranchingLogicDetails);

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = response, message = _iQuizService.ErrorMessage }));
                }

            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }


        /// <summary>
        /// Get Quiz Results
        /// </summary>
        /// <param name="QuizId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetQuizResults")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetQuizResults(int QuizId) {
            List<QuizResultResponse> responseLst = null;

            try {
                var quizResults = _iQuizService.GetQuizResults(QuizId);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    responseLst = new List<QuizResultResponse>();

                    foreach (var item in quizResults) {
                        var response = new QuizResultResponse();

                        responseLst.Add((QuizResultResponse)response.MapEntityToResponse(item));
                    }

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = responseLst, message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = responseLst, message = _iQuizService.ErrorMessage }));
                }

            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Update Quiz BranchingLogic
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateQuizBranchingLogic")]
        [AuthorizeTokenFilter]
        [SwaggerRequestExample(typeof(QuizBranchingLogicLinksListRequest), typeof(QuizBranchingLogicLinksListRequest.QuizBranchingLogicLinksListRequestExample))]
        public IHttpActionResult UpdateQuizBranchingLogic(QuizBranchingLogicLinksListRequest Obj) {
            try {
                Logger.Log(LogLevel.Error, Newtonsoft.Json.JsonConvert.SerializeObject(Obj).ToString());

                QuizBranchingLogicLinksListRequest requestObj = new QuizBranchingLogicLinksListRequest();

                _iQuizService.UpdateQuizBranchingLogicDetails(requestObj.MapRequestToEntity(Obj), BusinessUserId, CompanyInfo.Id);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Quiz branching setting updated.", message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Publish Quiz
        /// </summary>
        /// <param name="QuizId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("PublishQuiz")]
        [AuthorizeTokenFilter]
        public IHttpActionResult PublishQuiz(int QuizId) {
            try {
                string publishedCode = _publishQuizService.PublishQuiz(QuizId, BusinessUserId, CompanyInfo.Id);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = publishedCode, message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Duplicate Quiz
        /// </summary>
        /// <param name="QuizId"></param>
        /// <param name="AccessibleOfficeId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("DuplicateQuiz")]
        [AuthorizeTokenFilter]
        public IHttpActionResult DuplicateQuiz(int QuizId, string AccessibleOfficeId = null, string UserType = null, int? ModuleType = null) {
            try {
                var quizId = _iQuizService.DuplicateQuiz(QuizId, AccessibleOfficeId, UserType, ModuleType, BusinessUserId, CompanyInfo, (CompanyInfo.CreateAcademyCourseEnabled && CreateAcademyCourse), (CompanyInfo.CreateTechnicalRecruiterCourseEnabled && UserInfo.CreateTechnicalRecruiterCourse));

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = quizId, message = string.Empty }));
                }
                else if (_iQuizService.Status == ResultEnum.OkWithMessage) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Remove Quiz
        /// </summary>
        /// <param name="QuizId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("RemoveQuiz")]
        [AuthorizeTokenFilter]
        public IHttpActionResult RemoveQuiz(int QuizId) {
            try {
                _iQuizService.RemoveQuiz(QuizId, BusinessUserId, CompanyInfo.Id);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Quiz removed.", message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }


        /// <summary>
        /// Attempt Quiz
        /// </summary>
        /// <param name="Obj"></param>
        /// <param name="QuizCode"></param>
        /// <param name="Mode"></param>
        /// <param name="Type"></param>
        /// <param name="QuestionId"></param>
        /// <param name="AnswerId"></param>
        /// <param name="UserTypeId"></param>
        /// <param name="QuestionType"></param>
        /// <param name="UsageType"></param>
        /// <param name="OptimizeCode"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AttemptQuiz")]
        [AuthorizeTokenFilter]

        public IHttpActionResult AttemptQuiz(List<TextAnswerRequest> Obj, string QuizCode, string Mode, string Type = "", int QuestionId = -1, string AnswerId = "", int UserTypeId = 0, int? QuestionType = (int)BranchingLogicEnum.QUESTION, int? UsageType = null, bool OptimizeCode = true) {

            if (GlobalSettings.UseQuizAttemptNew && OptimizeCode) {

                try {

                    AttemptQuizRequest attemptQuizRequest = new AttemptQuizRequest {
                        TextAnswerList = Obj,
                        QuizCode = QuizCode,
                        Mode = Mode,
                        Type = Type,
                        QuestionId = QuestionId,
                        AnswerId = AnswerId,
                        QuestionType = QuestionType,
                        UsageType = UsageType
                    };
                    return AttemptQuiz(attemptQuizRequest);
                } catch (Exception ex) {
                    ErrorLog.LogError(ex);
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
                }

            }

            AttemptQuizResponse response = null;
            try {
                TextAnswerRequest requestObj = new TextAnswerRequest();
                QuizAnswerSubmit quizDetails = null;
                quizDetails = _automationService.AttemptQuiz(requestObj.MapRequestToEntity(Obj), QuizCode, Mode, Type, QuestionId, AnswerId, BusinessUserId, UserTypeId, QuestionType, UsageType);


                if (_automationService.Status == ResultEnum.Ok) {
                    response = new AttemptQuizResponse();

                    response = (AttemptQuizResponse)response.MapEntityToResponse(quizDetails);

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = response, message = _automationService.ErrorMessage }));
                }

            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }

        }

        private IHttpActionResult AttemptQuiz(AttemptQuizRequest attemptQuizRequest) {
            AttemptQuizV2Response response = null;
            try {
                QuizAnswerSubmit quizDetails = null;
                quizDetails = _quizAttemptService.AttemptQuiz(attemptQuizRequest);


                if (_quizAttemptService.Status == ResultEnum.Ok) {
                    response = new AttemptQuizV2Response();

                    response = (AttemptQuizV2Response)response.MapEntityToResponse(quizDetails);

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = response, message = _quizAttemptService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Get Quiz Redirect Result
        /// </summary>
        /// <param name="QuizId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetQuizRedirectResult")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetQuizRedirectResult(int QuizId) {
            List<QuizResultRedirectResponse> responseLst = null;

            try {
                var quizRedirectResult = _iQuizService.GetQuizRedirectResult(QuizId);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    responseLst = new List<QuizResultRedirectResponse>();

                    foreach (var item in quizRedirectResult) {
                        var response = new QuizResultRedirectResponse();

                        responseLst.Add((QuizResultRedirectResponse)response.MapEntityToResponse(item));
                    }

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = responseLst, message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = responseLst, message = _iQuizService.ErrorMessage }));
                }

            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }


        /// <summary>
        /// Update Quiz Redirect Result
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateQuizRedirectResult")]
        [AuthorizeTokenFilter]
        [SwaggerRequestExample(typeof(QuizResultRedirectRequest), typeof(QuizResultRedirectRequest.QuizResultRedirectRequestExample))]
        public IHttpActionResult UpdateQuizRedirectResult(List<QuizResultRedirectRequest> Obj) {
            try {
                QuizResultRedirectRequest requestObj = new QuizResultRedirectRequest();

                _iQuizService.UpdateQuizRedirectResult(requestObj.MapRequestToEntity(Obj), BusinessUserId, CompanyInfo.Id);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Quiz redirect result updated.", message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        ///  Update Branching Logic State
        /// </summary>
        /// <param name="QuizId"></param>
        /// <param name="IsEnabled"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateBranchingLogicState")]
        [AuthorizeTokenFilter]
        public IHttpActionResult UpdateBranchingLogicState(int QuizId, bool IsEnabled) {
            try {
                _iQuizService.UpdateBranchingLogicState(QuizId, BusinessUserId, IsEnabled, CompanyInfo.Id);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Quiz branching logic state updated.", message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Get quiz list by versions
        /// </summary>
        /// <param name="QuizId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetQuizListByVersions")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetQuizListByVersions(int QuizId, long OffsetValue) {
            List<QuizVersionResponse> responseLst = null;

            try {
                var quizList = _iQuizService.GetQuizListByVersions(QuizId, OffsetValue);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    responseLst = new List<QuizVersionResponse>();

                    foreach (var item in quizList) {
                        var response = new QuizVersionResponse();

                        responseLst.Add((QuizVersionResponse)response.MapEntityToResponse(item));
                    }
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = responseLst, message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = responseLst, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = responseLst, message = ex.Message }));
            }
        }

        /// <summary>
        /// Get quiz analytics overview
        /// </summary>
        /// <param name="PublishedQuizId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetQuizAnalyticsOverview")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetQuizAnalyticsOverview(int PublishedQuizId) {
            QuizAnalyticsOverviewResponse response = null;

            try {
                var quizAnalyticsOverviewResponse = _iQuizService.GetQuizAnalyticsOverview(PublishedQuizId);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    response = new QuizAnalyticsOverviewResponse();

                    response = (QuizAnalyticsOverviewResponse)response.MapEntityToResponse(quizAnalyticsOverviewResponse);

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = response, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = response, message = ex.Message }));
            }
        }

        /// <summary>
        /// Get quiz analytics stats
        /// </summary>
        /// <param name="PublishedQuizId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetQuizAnalyticsStats")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetQuizAnalyticsStats(int PublishedQuizId) {
            QuizAnalyticsStatsResponse response = null;

            try {
                var quizAnalyticsStatsResponse = _iQuizService.GetQuizAnalyticsStats(PublishedQuizId);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    response = new QuizAnalyticsStatsResponse();

                    response = (QuizAnalyticsStatsResponse)response.MapEntityToResponse(quizAnalyticsStatsResponse);

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = response, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = response, message = ex.Message }));
            }
        }

        /// <summary>
        /// Get quiz attempt code
        /// </summary>
        /// <param name="PublishedCode"></param>
        /// <param name="Mode"></param>
        /// <param name="UserTypeId"></param>
        /// <param name="UserId"></param>
        /// <param name="WorkPackageInfoId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetQuizAttemptCode")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetQuizAttemptCode(string Mode, int UserTypeId = (int)UserTypeEnum.Public, string UserId = null, int WorkPackageInfoId = 0, string SourceId = "", string ConfigurationId = null, string PublishedCode = null, string CompanyCode = "") {
            try {
                if ((UserTypeId == (int)UserTypeEnum.Recruiter || UserTypeId == (int)UserTypeEnum.JobRockAcademy || UserTypeId == (int)UserTypeEnum.TechnicalRecruiter) && string.IsNullOrEmpty(UserId)) {
                    if (UserInfo == null)
                        return ResponseMessage(Request.CreateResponse(HttpStatusCode.Unauthorized, new { data = string.Empty, message = "Unauthorized" }));
                }

                var quizAttemptCode = _iQuizService.GetQuizAttemptCode(PublishedCode, Mode, UserTypeId, UserId, WorkPackageInfoId, UserInfo, SourceId, ConfigurationId, CompanyCode);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = quizAttemptCode, message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Save lead user info
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("SaveLeadUserInfo")]
        [AuthorizeTokenFilter]
        [SwaggerRequestExample(typeof(LeadUserRequest), typeof(LeadUserRequest.LeadUserRequestExample))]
        public IHttpActionResult SaveLeadUserInfo(LeadUserRequest Obj) {
            try {
                if (string.IsNullOrEmpty(Obj.CompanyCode)) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { message = "Company Code is required" }));
                }
                //var response = OWCHelper.GetCompanyInfo(Obj.CompanyCode);
                var response = CommonStaticData.GetCachedCompanyInfo(Obj.CompanyCode);
                var responseId = "";
                var quizId = _iQuizService.GetQuizIdByAttemptCode(Obj.PublishedCode);

                if (response != null && quizId > 0) {
                    var companyInfo = Obj.MapCompanyResponseToEntity(response);
                    string leadApiClientCode = GlobalSettings.LeadApiClientCode;
                    if (!string.IsNullOrEmpty(leadApiClientCode)) {

                        string[] values = leadApiClientCode.Split(',');
                        List<string> clientCodes = new List<string>(values);
                        if (clientCodes.Any(item => item.ContainsCI(companyInfo.ClientCode))) {
                            responseId = OWCHelper.SaveLeadUserInfo(
                                Obj.MapRequestToEntity(Obj, companyInfo.CompanyName, response.clientCode, quizId),
                                companyInfo
                            );
                        }
                        else {
                            responseId = OWCHelper.SaveHubLeadUserInfo(
                                Obj.MapRequestToHubEntity(Obj, companyInfo.CompanyName, response.clientCode, quizId),
                                companyInfo
                            );
                            if (!string.IsNullOrEmpty(responseId)) {
                                responseId = "SF-" + responseId;
                            }
                        }

                        if (!string.IsNullOrEmpty(responseId)) {
                            _iQuizService.UpdateQuizCompleteStatus(Obj.PublishedCode, responseId);

                            if (_iQuizService.Status == ResultEnum.Ok) {
                                return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Quiz completed.", message = string.Empty }));
                            }
                            else {
                                return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                            }
                        }
                        else {
                            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = "Error creating lead" }));

                        }

                    }
                    else {
                        responseId = OWCHelper.SaveLeadUserInfo(Obj.MapRequestToEntity(Obj, companyInfo.CompanyName, response.clientCode, quizId), companyInfo);

                        if (!string.IsNullOrEmpty(responseId)) {
                            _iQuizService.UpdateQuizCompleteStatus(Obj.PublishedCode, responseId);

                            if (_iQuizService.Status == ResultEnum.Ok) {
                                return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Quiz completed.", message = string.Empty }));
                            }
                            else {
                                return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                            }
                        }
                        else {
                            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = "Error creating lead" }));
                        }
                    }
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { message = "Invalid Company Code" }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }




           


            /// <summary>
            /// Get quiz lead collection stats
            /// </summary>
            /// <param name="PublishedQuizId"></param>
            /// <returns></returns>
            [HttpGet]
        [Route("GetQuizLeadCollectionStats")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetQuizLeadCollectionStats(int PublishedQuizId) {
            List<QuizLeadCollectionStatsResponse> responseList = null;

            try {
                var quizLeadCollectionStatsResponseList = _iQuizService.GetQuizLeadCollectionStats(PublishedQuizId);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    responseList = new List<QuizLeadCollectionStatsResponse>();

                    foreach (var item in quizLeadCollectionStatsResponseList) {
                        var response = new QuizLeadCollectionStatsResponse();

                        responseList.Add((QuizLeadCollectionStatsResponse)response.MapEntityToResponse(item));
                    }

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = responseList, message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = responseList, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = responseList, message = ex.Message }));
            }
        }

        /// <summary>
        /// Add Action in Quiz
        /// </summary>
        /// <param name="quizId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddActionInQuiz")]
        [AuthorizeTokenFilter]
        public IHttpActionResult AddActionInQuiz(int quizId) {
            try {
                var actionResult = _iQuizService.AddActionInQuiz(quizId, BusinessUserId, CompanyInfo.Id);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    var response = new QuizActionResponse();

                    response = (QuizActionResponse)response.MapEntityToResponse(actionResult);

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// update Action in Quiz
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateActionInQuiz")]
        [AuthorizeTokenFilter]
        [SwaggerRequestExample(typeof(QuizActionRequest), typeof(QuizActionRequest.QuizActionRequestExample))]
        public IHttpActionResult UpdateActionInQuiz(QuizActionRequest obj) {
            try {
                _iQuizService.UpdateActionInQuiz(obj.MapRequestToEntity(obj), BusinessUserId, CompanyInfo.Id);

                if (_iQuizService.Status == ResultEnum.Ok) {

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Quiz Action Updated", message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Remove Action in Quiz
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("RemoveActionInQuiz")]
        [AuthorizeTokenFilter]
        public IHttpActionResult RemoveActionInQuiz(int Id) {
            try {
                _iQuizService.RemoveActionInQuiz(Id, BusinessUserId, CompanyInfo.Id);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Quiz Action Removed", message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Get Action by Id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetQuizAction")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetQuizActionDetail(int Id) {
            try {
                var actionResult = _iQuizService.GetQuizAction(Id);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    var response = new QuizActionResponse();

                    response = (QuizActionResponse)response.MapEntityToResponse(actionResult);


                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = actionResult, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Add Content in Quiz
        /// </summary>
        /// <param name="quizId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddContentInQuiz")]
        [AuthorizeTokenFilter]
        public IHttpActionResult AddContentInQuiz(int quizId) {
            try {
                var data = _iQuizService.AddContentInQuiz(quizId, BusinessUserId, CompanyInfo.Id);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    var response = new QuizContentResponse();

                    response = (QuizContentResponse)response.MapEntityToResponse(data);

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Update Content in Quiz
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateContentInQuiz")]
        [AuthorizeTokenFilter]
        [SwaggerRequestExample(typeof(QuizContentRequest), typeof(QuizContentRequest.QuizContentRequestExample))]
        public IHttpActionResult UpdateContentInQuiz(QuizContentRequest obj) {
            try {

                _iQuizService.UpdateContentInQuiz(obj.MapRequestToEntity(obj), BusinessUserId, CompanyInfo.Id);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Content Updated", message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Remove Content in Quiz
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("RemoveContentInQuiz")]
        [AuthorizeTokenFilter]
        public IHttpActionResult RemoveContentInQuiz(int Id) {
            try {
                _iQuizService.RemoveContentInQuiz(Id, BusinessUserId, CompanyInfo.Id);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Quiz Action Removed", message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Get Content by Id
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="QuizId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetQuizContentDetail")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetQuizContentDetail(int Id, int QuizId) {
            try {
                var actionResult = _iQuizService.GetQuizContent(Id, QuizId);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    var response = new QuizContentResponse();

                    response = (QuizContentResponse)response.MapEntityToResponse(actionResult);


                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = actionResult, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }


        /// <summary>
        /// Add Attachment in Quiz
        /// </summary>
        /// <param name="Quiz"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddAttachmentInQuiz")]
        [AuthorizeTokenFilter]
        [SwaggerRequestExample(typeof(QuizAttachmentRequest), typeof(QuizAttachmentRequest.QuizAttachmentRequestExample))]
        public IHttpActionResult AddAttachmentInQuiz(QuizAttachmentRequest Quiz) {
            try {
                _iQuizService.AddAttachmentInQuiz(Quiz.MapRequestToEntity(Quiz), BusinessUserId, CompanyInfo.Id);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Added", message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Get All Attachments in Quiz
        /// </summary>
        /// <param name="QuizId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAttachmentsInQuiz")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetAttachmentsInQuiz(int QuizId) {
            try {
                var attachmentLst = _iQuizService.GetAttachmentsInQuiz(QuizId);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    var response = new List<QuizAttachmentResponse>();

                    QuizAttachmentResponse res = new QuizAttachmentResponse();
                    response.Add((QuizAttachmentResponse)res.MapEntityToResponse(attachmentLst));


                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Quiz Share Setting
        /// </summary>
        /// <param name="Quiz"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("QuizShareSetting")]
        [AuthorizeTokenFilter]
        [SwaggerRequestExample(typeof(QuizShareRequest), typeof(QuizShareRequest.QuizShareRequestExample))]
        public IHttpActionResult QuizShareSetting(QuizShareRequest Quiz) {
            try {
                if ((Quiz.JobRockAcademy == null || (CompanyInfo.CreateAcademyCourseEnabled && CreateAcademyCourse)) && (Quiz.TechnicalRecruiter == null || (CompanyInfo.CreateTechnicalRecruiterCourseEnabled && UserInfo.CreateTechnicalRecruiterCourse))) {
                    _iQuizService.QuizShareSetting(Quiz.MapRequestToEntity(Quiz), BusinessUserId, (CompanyInfo.CreateAcademyCourseEnabled && CreateAcademyCourse), (CompanyInfo.CreateTechnicalRecruiterCourseEnabled && UserInfo.CreateTechnicalRecruiterCourse), CompanyInfo.Id);

                    if (_iQuizService.Status == ResultEnum.Ok) {
                        return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Updated", message = string.Empty }));
                    }
                    else {
                        return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                    }
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.Unauthorized, new { data = string.Empty, message = "Unauthorized" }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Get Quiz Share Setting
        /// </summary>
        /// <param name="QuizId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetQuizShareSetting")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetQuizShareSetting(int QuizId) {
            try {
                var data = _iQuizService.GetQuizShareSetting(QuizId);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    QuizShareResponse res = new QuizShareResponse();
                    var response = (QuizShareResponse)res.MapEntityToResponse(data);

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Add Badge in Quiz
        /// </summary>
        /// <param name="Quiz"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddBadgeInQuiz")]
        [AuthorizeTokenFilter]
        public IHttpActionResult AddBadgeInQuiz(int quizId) {
            try {
                var data = _iQuizService.AddBadgeInQuiz(quizId, BusinessUserId, CompanyInfo.Id);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    var response = new QuizBadgeResponse();

                    response = (QuizBadgeResponse)response.MapEntityToResponse(data);
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Remove Badge in Quiz
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("RemoveBadgeInQuiz")]
        [AuthorizeTokenFilter]
        public IHttpActionResult RemoveBadgeInQuiz(int Id) {
            try {
                _iQuizService.RemoveBadgeInQuiz(Id, BusinessUserId, CompanyInfo.Id);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Removed", message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Update Badge in Quiz
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateBadgeInQuiz")]
        [AuthorizeTokenFilter]
        [SwaggerRequestExample(typeof(QuizBadgeRequest), typeof(QuizBadgeRequest.QuizBadgeRequestExample))]
        public IHttpActionResult UpdateBadgeInQuiz(QuizBadgeRequest Obj) {
            try {
                _iQuizService.UpdateBadgeInQuiz(Obj.MapRequestToEntity(Obj), BusinessUserId, CompanyInfo.Id);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Updated", message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Get Badge in Quiz
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="QuizId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetBadgeInQuiz")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetBadgeInQuiz(int Id, int QuizId) {
            try {
                var data = _iQuizService.GetBadgeInQuiz(Id, QuizId);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    var response = new QuizBadgeResponse();

                    response = (QuizBadgeResponse)response.MapEntityToResponse(data);
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// To get tags by category
        /// </summary>
        /// <param name="tagCategoryId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetTagsByCategory")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetTagsByCategory(string tagCategoryId) {
            try {
                //var response = OWCHelper.GetTagsByCategory(tagCategoryId, CompanyInfo);
                var response = CommonStaticData.GetCachedTagsByCategory(tagCategoryId, CompanyInfo);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// To get all categories
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAllCategory")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetAllCategory() {
            try {
                //var response = OWCHelper.GetAllCategory(CompanyInfo);
                var response = CommonStaticData.GetCachedAllCategory(CompanyInfo);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// To Remove Tag from answer
        /// </summary>
        /// <param name="TagId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("RemoveTag")]
        [AuthorizeTokenFilter]
        public IHttpActionResult RemoveTag(int TagId) {
            try {
                _iQuizService.RemoveTag(TagId, CompanyInfo.Id);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Tag removed.", message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }


        /// <summary>
        /// To UpdateAnswerTagAndCategory
        /// </summary>
        /// <param name="quizAnsweTags"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateAnswerTagAndCategory")]
        [AuthorizeTokenFilter]
        [SwaggerRequestExample(typeof(QuizAnsweTagsRequest), typeof(QuizAnsweTagsRequest.QuizAnsweTagsRequestExample))]
        public IHttpActionResult UpdateAnswerTagAndCategory(List<QuizAnsweTagsRequest> quizAnsweTags) {
            try {
                QuizAnsweTagsRequest obj = new QuizAnsweTagsRequest();

                _iQuizService.UpdateAnswerTagAndCategory(obj.MapRequestToEntity(quizAnsweTags), CompanyInfo.Id);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Tags and Category added.", message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// To get details of Attempted Quiz by a lead
        /// </summary>
        /// <param name="LeadId"></param>
        /// <param name="QuizId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAttemptedQuizDetailByLead")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetAttemptedQuizDetailByLead(string LeadId, int QuizId = 0, string ConfigurationId = null) {
            try {
                var data = _iQuizService.GetAttemptedQuizDetailByLead(LeadId, QuizId, ConfigurationId);
                List<AttemptedQuizDetailResponse> responseList = null;
                if (_iQuizService.Status == ResultEnum.Ok) {
                    responseList = new List<AttemptedQuizDetailResponse>();
                    if (data.Count() > 0) {
                        foreach (var quizObj in data.OrderByDescending(r => r.QuizDetails.QuizDate)) {
                            AttemptedQuizDetailResponse res = new AttemptedQuizDetailResponse();
                            responseList.Add((AttemptedQuizDetailResponse)res.MapEntityToResponse(quizObj));
                        }
                    }
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = responseList, message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// To Remove Category
        /// </summary>
        /// <param name="CategoryId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("RemoveCategory")]
        [AuthorizeTokenFilter]
        public IHttpActionResult RemoveCategory(int CategoryId) {
            try {
                _iQuizService.RemoveCategory(CategoryId, CompanyInfo.Id);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Category removed.", message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// to update result range
        /// </summary>
        /// <param name="quizResultRangeObj"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateResultRange")]
        [AuthorizeTokenFilter]
        public IHttpActionResult UpdateResultRange(QuizResultRangeRequest quizResultRangeObj) {
            try {
                QuizResultRangeRequest obj = new QuizResultRangeRequest();

                _iQuizService.UpdateResultRange(obj.MapRequestToEntity(quizResultRangeObj), BusinessUserId, CompanyInfo.Id);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Result score Updated.", message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }


        /// <summary>
        /// To Get Variables By Quiz Id
        /// </summary>
        /// <param name="QuizId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetVariablesByQuizId")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetVariablesByQuizId(int QuizId) {
            try {
                var data = _iQuizService.GetVariablesByQuizId(QuizId);
                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = data, message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// GetQuizResultAndAction
        /// </summary>
        /// <param name="QuizId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetQuizResultAndAction")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetQuizResultAndAction(int QuizId) {
            try {
                var data = _iQuizService.GetQuizResultAndAction(QuizId, false, false);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = data, message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// GetQuizAllResultAndAction
        /// </summary>
        /// <param name="QuizId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("~/api/v2/Quiz/GetQuizResultAndAction")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetQuizAllResultAndAction(int QuizId) {
            try {
                var data = _iQuizService.GetQuizAllResultAndAction(QuizId);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = data, message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }


        ///// <summary>
        ///// Update Quiz Answer type
        ///// </summary>
        ///// <param name="QuestionId"></param>
        ///// <param name="AnswerType"></param>
        ///// <returns></returns>
        //[HttpPost]
        //[Route("UpdateAnswerType")]
        //[AuthorizeTokenFilter]
        //public IHttpActionResult UpdateAnswerType(int QuestionId, int AnswerType, int? answerStructureType = null, bool isWhatsappEnable = false, bool isMultiRating = false)
        //{
        //    try
        //    {
        //        //_iQuizService.UpdateAnswerType(QuestionId, AnswerType, BusinessUserId, CompanyInfo.Id, answerStructureType, isWhatsappEnable, isMultiRating);
        //        _questionService.UpdateAnswerType(QuestionId, AnswerType, BusinessUserId, CompanyInfo.Id, answerStructureType, isWhatsappEnable, isMultiRating);

        //        if(_questionService.Status == ResultEnum.Ok)
        //        {
        //            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Quiz answer type updated.", message = string.Empty }));
        //        }
        //        else
        //        {
        //            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLog.LogError(ex);
        //        return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
        //    }
        //}


        /// <summary>
        /// Get relation based on question Id
        /// </summary>
        /// <param name="questionId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetResultCorrelation")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetResultCorrelation(int questionId) {
            List<ResultCorrelationResponse> responseList = null;
            try {
                var correlationList = _iQuizService.GetResultCorrelation(questionId);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    responseList = new List<ResultCorrelationResponse>();
                    foreach (var correlation in correlationList) {
                        ResultCorrelationResponse res = new ResultCorrelationResponse();
                        responseList.Add((ResultCorrelationResponse)res.MapEntityToResponse(correlation));
                    }
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = responseList, message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Update answer and result relationship
        /// </summary>
        /// <param name="resultCorrelationRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateResultCorrelation")]
        [AuthorizeTokenFilter]
        [SwaggerRequestExample(typeof(ResultCorrelationRequest), typeof(ResultCorrelationRequest.ResultCorrelationRequestExample))]
        public IHttpActionResult UpdateResultCorrelation(ResultCorrelationRequest resultCorrelationRequest) {
            try {
                _iQuizService.UpdateResultCorrelation(resultCorrelationRequest.MapRequestToEntity(resultCorrelationRequest), BusinessUserId, CompanyInfo.Id);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Result correlation updated.", message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// reorder result
        /// </summary>
        /// <param name="quizResultReorderRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ReorderResult")]
        [AuthorizeTokenFilter]
        [SwaggerRequestExample(typeof(QuizResultReorderRequest), typeof(QuizResultReorderRequest.QuizResultReorderRequestExample))]
        public IHttpActionResult ReorderResult(QuizResultReorderRequest quizResultReorderRequest) {
            try {
                _iQuizService.ReorderResultQuestionAnswer(quizResultReorderRequest.MapRequestToEntity(quizResultReorderRequest), BusinessUserId, CompanyInfo.Id);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Order updated.", message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Get personality result setting
        /// </summary>
        /// <param name="QuizId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetPersonalityResultSetting")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetPersonalityResultSetting(int QuizId) {
            try {
                var resultSettingLst = _iQuizService.GetPersonalityResultSetting(QuizId);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    var response = new List<PersonalityResultSettingResponse>();

                    PersonalityResultSettingResponse res = new PersonalityResultSettingResponse();
                    response.Add((PersonalityResultSettingResponse)res.MapEntityToResponse(resultSettingLst));

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Update personality result setting
        /// </summary>
        /// <param name="personalityResultSettingRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdatePersonalityResultSetting")]
        [AuthorizeTokenFilter]
        [SwaggerRequestExample(typeof(PersonalityResultSettingRequest), typeof(PersonalityResultSettingRequest.PersonalityResultSettingRequestExample))]
        public IHttpActionResult UpdatePersonalityResultSetting(PersonalityResultSettingRequest personalityResultSettingRequest) {
            try {
                _iQuizService.UpdatePersonalityResultSetting(personalityResultSettingRequest.MapRequestToEntity(personalityResultSettingRequest), BusinessUserId, CompanyInfo.Id);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Personality setting updated.", message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Update personality result status
        /// </summary>
        /// <param name="QuizId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdatePersonalityResultStatus")]
        [AuthorizeTokenFilter]
        public IHttpActionResult UpdatePersonalityResultStatus(int QuizId, int status) {
            try {
                _iQuizService.UpdatePersonalityResultStatus(QuizId, status, BusinessUserId, CompanyInfo.Id);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Personality status updated.", message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Update personality full width stting
        /// </summary>
        /// <param name="QuizId"></param>
        /// <param name="IsFullWidthEnable"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdatePersonalityWidthSetting")]
        [AuthorizeTokenFilter]
        public IHttpActionResult UpdatePersonalityWidthSetting(int QuizId, bool IsFullWidthEnable) {
            try {
                _iQuizService.UpdatePersonalityWidthSetting(QuizId, IsFullWidthEnable, BusinessUserId, CompanyInfo.Id);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Personality full width updated.", message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Update personality max result
        /// </summary>
        /// <param name="QuizId"></param>
        /// <param name="maxResult"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdatePersonalityMaxResult")]
        [AuthorizeTokenFilter]
        public IHttpActionResult UpdatePersonalityMaxResult(int QuizId, int maxResult, bool ShowLeadUserForm) {
            try {
                _iQuizService.UpdatePersonalityMaxResult(QuizId, maxResult, ShowLeadUserForm, BusinessUserId, CompanyInfo.Id);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Personality max result updated.", message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// To get details of Attempted Quiz by a lead
        /// </summary>
        /// <param name="LeadId"></param>
        /// <param name="QuizId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAttemptedAutomationByLeads")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetAttemptedAutomationByLeads(string LeadId, int QuizId = 0) {
            try {
                if (string.IsNullOrEmpty(LeadId)) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { message = "LeadId is required" }));
                }
                var data = _iQuizService.GetAttemptedAutomationByLeads(LeadId, QuizId);
                List<AttemptedAutomationResponse> responseList = null;
                if (_iQuizService.Status == ResultEnum.Ok) {
                    responseList = new List<AttemptedAutomationResponse>();
                    if (data.Count() > 0) {
                        foreach (var quizObj in data) {
                            AttemptedAutomationResponse res = new AttemptedAutomationResponse();
                            responseList.Add((AttemptedAutomationResponse)res.MapEntityToResponse(quizObj));
                        }
                    }
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = responseList, message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }
        /// <summary>
        /// To Get Attempted Automation AcheivedResult Details By Leads
        /// </summary>
        /// <param name="LeadID"></param>
        /// <param name="QuizId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/api/v2/Quiz/GetAttemptedAutomationByLeads")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetAttemptedAutomationAcheivedResultDetailsByLeads(List<string> LeadID, int QuizId = 0) {
            try {
                if (LeadID == null || !LeadID.Any()) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { message = "LeadId is required" }));
                }
                var data = _iQuizService.GetAttemptedAutomationAcheivedResultDetailsByLeads(LeadID, QuizId);
                List<AttemptedAutomationAcheivedResultDetailsResponse> responseList = null;
                if (_iQuizService.Status == ResultEnum.Ok) {
                    responseList = new List<AttemptedAutomationAcheivedResultDetailsResponse>();
                    if (data.Count() > 0) {
                        foreach (var quizObj in data) {
                            AttemptedAutomationAcheivedResultDetailsResponse res = new AttemptedAutomationAcheivedResultDetailsResponse();
                            responseList.Add((AttemptedAutomationAcheivedResultDetailsResponse)res.MapEntityToResponse(quizObj));
                        }
                    }
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = responseList, message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /////// <summary>
        /////// Upload attachment
        /////// </summary>
        /////// <returns></returns>
        //[HttpPost]
        //[Route("UploadAttachment")]
        //[AuthorizeTokenFilter]
        //public Task<HttpResponseMessage> UploadAttachment()
        //{
        //    // Check if the request contains multipart/form-data.
        //    if (!Request.Content.IsMimeMultipartContent())
        //    {
        //        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
        //    }

        //    var folderName = Guid.NewGuid().ToString();
        //    var destinationFolderPath = HttpContext.Current.Server.MapPath("~/UploadedFiles/" + folderName);

        //    if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/UploadedFiles/" + folderName)))  // if it doesn't exist, create
        //        Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/UploadedFiles/" + folderName));

        //    if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/App_Data")))  // if it doesn't exist, create
        //        Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/App_Data"));

        //    var returnUrlPath = "/UploadedFiles/" + folderName + "/";

        //    string root = HttpContext.Current.Server.MapPath("~/App_Data");
        //    var provider = new MultipartFormDataStreamProvider(root);

        //    // Read the form data and return an async task.
        //    var task = Request.Content.ReadAsMultipartAsync(provider).
        //        ContinueWith<HttpResponseMessage>(t =>
        //        {
        //            if (t.IsFaulted || t.IsCanceled)
        //            {
        //                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { message = t.Exception.Message });
        //            }

        //            // This illustrates how to get the file names.
        //            foreach (MultipartFileData file in provider.FileData)
        //            {
        //                foreach (MultipartFileData fileData in provider.FileData)
        //                {
        //                    string fileName = fileData.Headers.ContentDisposition.FileName;

        //                    if (fileName.StartsWith("\"") && fileName.EndsWith("\""))
        //                    {
        //                        fileName = fileName.Trim('"');
        //                    }
        //                    if (fileName.Contains(@"/") || fileName.Contains(@"\"))
        //                    {
        //                        fileName = Path.GetFileName(fileName);
        //                    }

        //                    File.Move(fileData.LocalFileName, Path.Combine(destinationFolderPath, fileName));
        //                }
        //            }

        //            string[] files = Directory.GetFiles(destinationFolderPath);

        //            return Request.CreateResponse(HttpStatusCode.OK, new { FileUrl = returnUrlPath + Path.GetFileName(files[0]), message = "File uploaded sucessfully" });
        //        });

        //    return task;
        //}

        /// <summary>
        /// Upload
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("UploadAttachment")]
        [AuthorizeTokenFilter]
        public async Task<IHttpActionResult> UploadAttachment() {
            var fileUploadResponseModelList = new List<FileUploadResponseModel>();
            // Check if the request contains multipart/form-data.            
            if (!Request.Content.IsMimeMultipartContent())
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);
            foreach (var file in provider.Contents) {
                var filename = file.Headers.ContentDisposition.FileName.Trim('\"');
                var buffer = await file.ReadAsByteArrayAsync();
                var fileUploadResponseModel = BlobStorageHelper.UploadFileToBlob(filename, buffer);
                fileUploadResponseModelList.Add(fileUploadResponseModel);
            }

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = fileUploadResponseModelList, message = string.Empty }));
        }

        /// <summary>
        /// RemoveAttachment
        /// </summary>
        /// <param name="FileIdentifier"></param>
        /// <returns></returns>
        [Route("RemoveAttachment")]
        [AuthorizeTokenFilter]
        public IHttpActionResult RemoveAttachment(string FileIdentifier) {
            try {
                BlobStorageHelper.DeleteBlobFile(FileIdentifier);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "File Deleted", message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Update Quiz Question Setting
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateQuizQuestionSetting")]
        [AuthorizeTokenFilter]
        [SwaggerRequestExample(typeof(QuizQuestionSettingRequest), typeof(QuizQuestionSettingRequest.QuizQuestionSettingRequestExample))]
        public IHttpActionResult UpdateQuizQuestionSetting(QuizQuestionSettingRequest Obj) {
            try {
                QuizQuestionSettingRequest quizQuestionSettingRequest = new QuizQuestionSettingRequest();

                _iQuizService.UpdateQuizQuestionSetting(quizQuestionSettingRequest.MapRequestToEntity(Obj), BusinessUserId, CompanyInfo.Id);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Quiz question setting updated", message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Update Quiz Question Setting
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateQuizSetting")]
        [AuthorizeTokenFilter]
        [SwaggerRequestExample(typeof(UpdateQuizSettingRequest), typeof(UpdateQuizSettingRequest.UpdateQuizSettingRequestExample))]
        public IHttpActionResult UpdateQuizSetting(UpdateQuizSettingRequest Obj) {
            try {
                UpdateQuizSettingRequest quizQuizSettingRequest = new UpdateQuizSettingRequest();

                _iQuizService.UpdateQuizSetting(quizQuizSettingRequest.MapRequestToEntity(Obj), BusinessUserId, CompanyInfo.Id);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Quiz question setting updated", message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Update Content in Quiz
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateContenSettingtInQuiz")]
        [AuthorizeTokenFilter]
        [SwaggerRequestExample(typeof(QuizContentSettingRequest), typeof(QuizContentSettingRequest.QuizContentSettingRequestExample))]
        public IHttpActionResult UpdateContenSettingtInQuiz(QuizContentSettingRequest obj) {
            try {
                _iQuizService.UpdateContenSettingtInQuiz(obj.MapRequestToEntity(obj), BusinessUserId, CompanyInfo.Id);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Content Setting Updated", message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }
        /// <summary>
        /// Update CoverSetting in Quiz
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateCoverSettingtInQuiz")]
        [AuthorizeTokenFilter]
        [SwaggerRequestExample(typeof(QuizCoverSettingRequest), typeof(QuizCoverSettingRequest.QuizCoverSettingRequestExample))]
        public IHttpActionResult UpdateCoverSettingtInQuiz(QuizCoverSettingRequest obj) {
            try {
                _quizCoverService.UpdateCoverSettingtInQuiz(obj.MapRequestToEntity(obj), BusinessUserId, CompanyInfo.Id);

                if (_quizCoverService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Cover Setting Updated", message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _quizCoverService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Get Quiz Previous Question Setting
        /// </summary>
        /// <param name="QuizId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetQuizPreviousQuestionSetting")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetQuizPreviousQuestionSetting(int QuizId) {
            QuizPreviousQuestionSettingResponse response = null;

            try {
                var quizDetails = _iQuizService.GetQuizPreviousQuestionSetting(QuizId);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    response = new QuizPreviousQuestionSettingResponse();

                    response = (QuizPreviousQuestionSettingResponse)response.MapEntityToResponse(quizDetails);

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = response, message = _iQuizService.ErrorMessage }));
                }

            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }


        ///// <summary>
        ///// Get Quiz List
        ///// </summary>
        ///// <param name="IncludeSharedWithMe"></param>
        ///// <param name="OffsetValue"></param>
        ///// <param name="SearchTxt"></param>
        ///// <param name="OrderBy"></param>
        ///// <param name="QuizTypeId"></param>
        ///// <param name="IsDataforGlobalOfficeAdmin"></param>
        ///// <param name="OfficeIdList"></param>
        ///// <param name="PageNo"></param>
        ///// <param name="PageSize"></param>
        ///// <param name="QuizId"></param>
        ///// <param name="IsFavorite"></param>
        ///// <param name="MustIncludeQuizId"></param>
        ///// <param name="UsageType"></param>
        ///// <param name="QuizTagId"></param>
        ///// <returns></returns>
        //[HttpPost]
        //[Route("~/api/v2/Quiz/GetList")]
        //[AuthorizeTokenFilter]
        //public IHttpActionResult GetList(bool IncludeSharedWithMe, long OffsetValue, string SearchTxt = "", int? OrderBy = 1, string QuizTypeId = "", bool IsDataforGlobalOfficeAdmin = false, string OfficeIdList = null, int? PageNo = null, int? PageSize = null, string QuizId = "", bool? IsFavorite = null, string MustIncludeQuizId = null, int? UsageType = null, int? QuizTagId = null)
        //{
        //    QuizResponse responseList = null;

        //    try
        //    {
        //        var quizLst = _quizListService.GetListWithPagination(BusinessUserId, !String.IsNullOrEmpty(OfficeIdList) ? OfficeIdList.Split(',').ToList() : new List<string>(), IncludeSharedWithMe, OffsetValue, PageNo, PageSize, SearchTxt, OrderBy, QuizTypeId, CompanyInfo, IsDataforGlobalOfficeAdmin, IsGlobalOfficeAdmin, UserInfo.Id, QuizId, IsFavorite, UserInfo.IsCreateStandardAutomationPermission, MustIncludeQuizId, UsageType, QuizTagId);

        //        if (_iQuizService.Status == ResultEnum.Ok)
        //        {
        //            responseList = new QuizResponse();
        //            responseList = (QuizResponse)responseList.MapEntityToResponse(quizLst);

        //            List<OWCBusinessUserResponse> userdetails = new List<OWCBusinessUserResponse>();
        //            List<Offices> officedetails = new List<Offices>();
        //            long[] Userids = new long[quizLst.Quiz.Count];

        //            for (int i = 0; i < quizLst.Quiz.Count; i++)
        //            {
        //                Userids[i] = quizLst.Quiz[i].CreatedByID;
        //            }
        //            if (Userids.Any() && (CompanyInfo.ClientCode != "JobRock" && CompanyInfo.ClientCode != "HEMA"))
        //            {
        //                userdetails = OWCHelper.GetUserListOnUserId(Userids, CompanyInfo).ToList();
        //            }
        //            //officedetails = OWCHelper.GetOfficeInfo(CompanyInfo).ToList();
        //            officedetails = CommonStaticData.GetCachedOfficeInfo(CompanyInfo).ToList();

        //            foreach (var Response in responseList.QuizListResponse)
        //            {
        //                Response.CreatedByName = (userdetails != null && userdetails.Any(r => r.userId == Response.CreatedById)) ? userdetails.FirstOrDefault(r => r.userId == Response.CreatedById).firstName : string.Empty;
        //                Response.OfficeName = (officedetails != null && officedetails.Any(r => r.id == Response.OfficeId)) ? officedetails.FirstOrDefault(r => r.id == Response.OfficeId).name : string.Empty;
        //            }

        //            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = responseList, message = string.Empty }));
        //        }
        //        else
        //        {
        //            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = responseList, message = _iQuizService.ErrorMessage }));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLog.LogError(ex);
        //        return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = responseList, message = ex.Message }));
        //    }
        //}

        [HttpPost]
        [Route("UpdateQuizFavoriteStatus")]
        [AuthorizeTokenFilter]
        public IHttpActionResult UpdateQuizFavoriteStatus(int QuizId, bool IsFavorite) {
            try {
                _iQuizService.UpdateQuizFavoriteStatus(QuizId, IsFavorite, UserInfo);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    if (string.IsNullOrEmpty(_iQuizService.ErrorMessage))
                        return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Favorite Status Updated", message = string.Empty }));
                    else
                        return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = -1, message = _iQuizService.ErrorMessage }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = -1, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = -1, message = ex.Message }));
            }
        }

        /// <summary>
        /// GetDynamicFieldByQuizId
        /// </summary>
        /// <param name="QuizId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetDynamicFieldByQuizId")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetDynamicFieldByQuizId(int QuizId) {
            DynamicFieldDetailsResponse response = null;
            try {
                var data = _iQuizService.GetDynamicFieldByQuizId(QuizId);
                if (_iQuizService.Status == ResultEnum.Ok) {
                    response = new DynamicFieldDetailsResponse();

                    response = (DynamicFieldDetailsResponse)response.MapEntityToResponse(data);

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// To Get Url Value by Key and Domain
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="DomainName"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetUrlValueByKey")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetUrlValueByKey(string Key, string DomainName) {
            try {
                var data = _iQuizService.GetUrlValueByKey(Key, DomainName);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = data, message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// UpdateAnswerObjectFieldsDetails
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateAnswerObjectFieldsDetails")]
        [AuthorizeTokenFilter]
        [SwaggerRequestExample(typeof(AnswerObjectFieldsRequest), typeof(AnswerObjectFieldsRequest.AnswerObjectFieldsRequestExample))]
        public IHttpActionResult UpdateAnswerObjectFieldsDetails(List<AnswerObjectFieldsRequest> obj) {
            try {
                AnswerObjectFieldsRequest request = new AnswerObjectFieldsRequest();
                _iQuizService.UpdateAnswerObjectFieldsDetails(request.MapRequestToEntity(obj), CompanyInfo.Id, UserInfo.BusinessUserId);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Answer Object Fields Updated", message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// RemoveAnswerObjectFieldsDetails
        /// </summary>
        /// <param name="AnswerId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("RemoveAnswerObjectFieldsDetails")]
        [AuthorizeTokenFilter]
        public IHttpActionResult RemoveAnswerObjectFieldsDetails(int AnswerId) {
            try {
                _iQuizService.RemoveAnswerObjectFieldsDetails(AnswerId, CompanyInfo.Id, UserInfo.BusinessUserId);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Details removed", message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// UpdateQuizUsageType
        /// </summary>
        /// <param name="QuizId"></param>
        /// <param name="UsageType"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateQuizUsageTypeandTagDetails")]
        [AuthorizeTokenFilter]
        [SwaggerRequestExample(typeof(QuizUsageTypeandTagDetailsRequest), typeof(QuizUsageTypeandTagDetailsRequest.QuizUsageTypeandTagDetailsRequestExample))]
        public IHttpActionResult UpdateQuizUsageTypeandTagDetails(QuizUsageTypeandTagDetailsRequest Obj) {
            QuizUsageTypeandTagDetailsRequest request = new QuizUsageTypeandTagDetailsRequest();
            try {
                _iQuizService.UpdateQuizUsageTypeandTagDetails(request.MapRequestToEntity(Obj), UserInfo);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Quiz usagetype updated.", message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Quiz UsageType Details Response
        /// </summary>
        /// <param name="QuizId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetQuizUsageTypeDetails")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetQuizUsageTypeDetails(int QuizId) {
            QuizUsageTypeDetailsResponse response = null;
            try {
                var data = _iQuizService.GetQuizUsageTypeDetails(QuizId, CompanyInfo.Id);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    response = new QuizUsageTypeDetailsResponse();

                    response = (QuizUsageTypeDetailsResponse)response.MapEntityToResponse(data);

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Get Automation Tags List
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAutomationTagsList")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetAutomationTagsList() {
            List<TagsResponse> tagResponseLst = null;

            try {
                //var tagObj = OWCHelper.GetAutomationTagsList(CompanyInfo.ClientCode);
                var tagObj = CommonStaticData.GetCachedAutomationTagsList(CompanyInfo.ClientCode);

                tagResponseLst = new List<TagsResponse>();

                if (tagObj != null) {
                    foreach (var item in tagObj) {
                        var tagsResponse = new TagsResponse();

                        tagsResponse = (TagsResponse)tagsResponse.MapEntityToResponse(item);

                        tagResponseLst.Add(tagsResponse);
                    }
                }

                return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = tagResponseLst, message = string.Empty }));
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = tagResponseLst, message = ex.Message }));
            }
        }

        /// <summary>
        /// Update Answer Option Values
        /// </summary>
        /// <param name="AnswerId"></param>
        /// <param name="ListValues"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateAnswerOptionValues")]
        [AuthorizeTokenFilter]
        public IHttpActionResult UpdateAnswerOptionValues(int AnswerId, List<string> ListValues) {
            try {
                _iQuizService.UpdateAnswerOptionValues(AnswerId, ListValues, BusinessUserId, CompanyInfo.Id);

                if (_iQuizService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = "Answer Option Values Updated", message = string.Empty }));
                }
                else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = _iQuizService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

    }
}


