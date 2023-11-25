using Core.Common.Caching;
using Core.Common.Extensions;
using Newtonsoft.Json;
using QuizApp.Helpers;
using QuizApp.RepositoryExtensions;
using QuizApp.RepositoryPattern;
using QuizApp.Response;
using QuizApp.Services.Model;
using QuizApp.Services.Validator;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Web;

namespace QuizApp.Services.Service
{
    public class QuizService : IQuizService
    {
        private ResultEnum status = ResultEnum.Ok;
        private string errormessage = string.Empty;
        private string leadInfoUpdateJson = "{'ContactId': '{ContactId}','ClientCode': '{ClientCode}','campaignName': '{campaignName}','appointmentStatus': '{appointmentStatus}','appointmentDate': '{appointmentDate}','appointmentTypeId': {appointmentTypeId},'appointmentTypeTitle': '{appointmentTypeTitle}','calendarId': {calendarId},'calendarTitle': '{calendarTitle}','appointmentBookedDate': '{appointmentBookedDate}','UserToken': '{UserToken}','SourceId': '{SourceId}'}";
        private readonly IQuizVariablesService _quizVariablesService;
        public QuizService(IQuizVariablesService quizVariablesService)
        {
            _quizVariablesService = quizVariablesService;
        }

        private string badgesInfoUpdateJson = "[ {'UserId': {UserId},'CourseId': '{CourseId}','CourseBadgeName': '{CourseBadgeName}','CourseBadgeImageUrl': '{CourseBadgeImageUrl}','CourseTitle': '{CourseTitle}'}]";

        public ResultEnum Status { get { return status; } set { status = value; } }
        public string ErrorMessage { get { return errormessage; } set { errormessage = value; } }
        public void UpdateQuiz(QuizDto QuizObj, int BusinessUserId, int CompanyId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quiz = UOWObj.QuizRepository.GetByID(QuizObj.QuizId);

                    if (quiz != null)
                    {
                        var quizDetails = quiz.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED);

                        if (quizDetails != null)
                        {
                            SaveDynamicVariable(quizDetails.QuizTitle, QuizObj.QuizTitle, quizDetails.Id);

                            quizDetails.QuizTitle = QuizObj.QuizTitle;

                            quizDetails.LastUpdatedBy = BusinessUserId;
                            quizDetails.LastUpdatedOn = DateTime.UtcNow;

                            quiz.State = (int)QuizStateEnum.DRAFTED;

                            UOWObj.QuizRepository.Update(quiz);
                            UOWObj.Save();


                            
                            
                            
                            
                            
                            AppLocalCache.Remove("QuizDetails_QuizId_" + quiz.Id);
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz not found for the QuizId " + QuizObj.QuizId;
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }


        //public List<LocalQuiz> GetList(int BusinessUserID, List<string> OfficeIdList, bool IncludeSharedWithMe, long OffsetValue, string SearchTxt, string QuizTypeId, CompanyModel CompanyInfo, bool IsDataforGlobalOfficeAdmin, bool IsGlobalOfficeAdmin, int? UserInfoId, string QuizId, bool? IsFavorite, bool? IsPublished, bool IsCreateStandardAutomation, int? QuizTagId)
        //{
        //    List<LocalQuiz> lstQuiz = new List<LocalQuiz>();

        //    try
        //    {
        //        using (var UOWObj = new AutomationUnitOfWork())
        //        {
        //            var quizTypeIds = string.IsNullOrEmpty(QuizTypeId) ? new List<int>() : QuizTypeId.Split(',').Select(Int32.Parse).ToList();
        //            var QuizIds = string.IsNullOrEmpty(QuizId) ? new List<int>() : QuizId.Split(',').Select(Int32.Parse).ToList();

        //            if (!string.IsNullOrEmpty(SearchTxt))
        //                SearchTxt = SearchTxt.ToUpper();

        //            var companyId = CompanyInfo.Id;

        //            IEnumerable<Db.Quiz> quizList = new List<Db.Quiz>();


        //            quizList = UOWObj.QuizRepository
        //                        .GetQueryable(r => (QuizIds.Any() ? QuizIds.Any(s => s == r.Id) : true)
        //                               && (quizTypeIds.Contains(r.QuizType))
        //                               && (string.IsNullOrEmpty(SearchTxt) ? true : (string.IsNullOrEmpty(r.QuizDetails.FirstOrDefault(t => t.State == (int)QuizStateEnum.DRAFTED).QuizTitle) ? false : r.QuizDetails.FirstOrDefault(t => t.State == (int)QuizStateEnum.DRAFTED).QuizTitle.ToUpper().IndexOf(SearchTxt) > -1))
        //                               && (r.Company.Id == companyId)
        //                               && (IsPublished.HasValue ? (IsPublished.Value ? (r.State == (int)QuizStateEnum.PUBLISHED) : (r.State != (int)QuizStateEnum.PUBLISHED)) : true)
        //                               && (IsFavorite.HasValue ? (IsFavorite.Value ? r.FavoriteQuizByUser.Any(f => f.QuizId == r.Id && f.UserTokenId == UserInfoId) : !r.FavoriteQuizByUser.Any(f => f.QuizId == r.Id && f.UserTokenId == UserInfoId)) : true)
        //                               && (QuizTagId.HasValue ? r.QuizTagDetails.Any(q => q.TagId == QuizTagId.Value) : true)
        //                               && (!string.IsNullOrEmpty(r.AccessibleOfficeId) ? (((IsGlobalOfficeAdmin && IsDataforGlobalOfficeAdmin && !OfficeIdList.Any()) ? true : OfficeIdList.Contains(r.AccessibleOfficeId)))
        //                                                                 : (IncludeSharedWithMe))
        //             , includeProperties: "QuizDetails, QuizDetails.QuestionsInQuiz, QuizTagDetails, FavoriteQuizByUser,UsageTypeInQuiz");

        //            string[] extensions = new string[] { "MP4", "WEBM", "MPG", "OBG", "MOV", "mp4" };

        //            var automationTagsList = CommonStaticData.GetCachedAutomationTagsList(CompanyInfo.ClientCode);

        //            foreach (var quiz in quizList)
        //            {
        //                var quizObj = new LocalQuiz();

        //                var quizDetailsObj = quiz.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active);

        //                if (quizDetailsObj != null)
        //                {
        //                    quizObj.Id = quiz.Id;
        //                    quizObj.QuizTitle = quizDetailsObj.QuizTitle;
        //                    quizObj.IsPublished = quiz.State == (int)QuizStateEnum.PUBLISHED;
        //                    quizObj.CreatedByID = quizDetailsObj.CreatedBy;
        //                    if (quizDetailsObj.LastUpdatedOn.HasValue)
        //                    {
        //                        quizObj.LastEditDate = Utility.ConvertUTCDateToLocalDate(quizDetailsObj.LastUpdatedOn.Value, OffsetValue);
        //                    }
        //                    quizObj.CreatedOn = Utility.ConvertUTCDateToLocalDate(quizDetailsObj.CreatedOn, OffsetValue);
        //                    quizObj.QuizCoverDetails = new QuizCover();
        //                    quizObj.QuizCoverDetails.QuizCoverImage = (quizDetailsObj.QuizCoverImage != null &&
        //                                                             quizDetailsObj.QuizCoverImage.Contains("/video/upload/") &&
        //                                                             (quizDetailsObj.QuizCoverImage.EndsWith("MP4") || quizDetailsObj.QuizCoverImage.EndsWith("WEBM") || quizDetailsObj.QuizCoverImage.EndsWith("MPG") || quizDetailsObj.QuizCoverImage.EndsWith("OBG") || quizDetailsObj.QuizCoverImage.EndsWith("MOV") || quizDetailsObj.QuizCoverImage.EndsWith("mp4"))
        //                                                                 ? quizDetailsObj.QuizCoverImage.Split(extensions, System.StringSplitOptions.RemoveEmptyEntries).GetValue(0).ToString() + "jpg"
        //                                                                 : quizDetailsObj.QuizCoverImage);
        //                    quizObj.QuizCoverDetails.QuizCoverTitle = quizDetailsObj.QuizCoverTitle;
        //                    quizObj.QuizCoverDetails.PublicIdForQuizCover = quizDetailsObj.PublicId;
        //                    quizObj.NoOfQusetions = quizDetailsObj.QuestionsInQuiz.Count(r => r.Status == (int)StatusEnum.Active);

        //                    quizObj.PublishedCode = quiz.PublishedCode;
        //                    quizObj.QuizType = (QuizTypeEnum)quiz.QuizType;

        //                    if (!string.IsNullOrEmpty(quiz.AccessibleOfficeId))
        //                    {
        //                        quizObj.AccessibleOfficeId = quiz.AccessibleOfficeId;
        //                    }

        //                    if (quiz.QuizDetails.FirstOrDefault().CreatedBy == BusinessUserID)
        //                        quizObj.IsCreatedByYou = true;

        //                    quizObj.IsFavorited = quiz.FavoriteQuizByUser.Any(f => f.QuizId == quiz.Id && f.UserTokenId == UserInfoId);

        //                    quizObj.IsCreateStandardAutomation = IsCreateStandardAutomation;
        //                    if (quiz.UsageTypeInQuiz != null)
        //                    {
        //                        quizObj.UsageTypes = new List<int>();
        //                        foreach (var item in quiz.UsageTypeInQuiz.ToList())
        //                        {
        //                            quizObj.UsageTypes.Add(item.UsageType);
        //                        }
        //                    }

        //                    quizObj.Tag = new List<Tags>();
        //                    foreach (var tag in quiz.QuizTagDetails.ToList())
        //                    {
        //                        quizObj.Tag.Add(new Tags()
        //                        {
        //                            TagId = tag.TagId,
        //                            TagName = (automationTagsList != null && automationTagsList.Any(r => r.tagId == tag.TagId)) ? automationTagsList.FirstOrDefault(r => r.tagId == tag.TagId).tagName : string.Empty,
        //                            TagCode = (automationTagsList != null && automationTagsList.Any(r => r.tagId == tag.TagId)) ? automationTagsList.FirstOrDefault(r => r.tagId == tag.TagId).tagCode : string.Empty
        //                        });
        //                    }

        //                    lstQuiz.Add(quizObj);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Status = ResultEnum.Error;
        //        ErrorMessage = ex.Message;
        //        throw ex;
        //    }

        //    return lstQuiz;
        //}

        //public QuizList GetAutomationList(int PageNo, int PageSize, int BusinessUserID, List<string> OfficeIdList, bool IncludeSharedWithMe, long OffsetValue, string SearchTxt, int OrderBy, string QuizTypeId, CompanyModel CompanyInfo, bool IsDataforGlobalOfficeAdmin, bool IsGlobalOfficeAdmin, int? UserInfoId, string QuizId, bool? IsFavorite, bool? IsPublished, bool IsCreateStandardAutomation, int? QuizTagId, int? UsageType )
        //{
        //    QuizList quizlist = new QuizList();
        //    List<LocalQuiz> lstQuiz = new List<LocalQuiz>();
        //    int totalCount = 0;
        //    var quizTypeIds = string.IsNullOrEmpty(QuizTypeId) ? new List<int>() : QuizTypeId.Split(',').Select(Int32.Parse).ToList();
        //    var QuizIds = string.IsNullOrEmpty(QuizId) ? new List<int>() : QuizId.Split(',').Select(Int32.Parse).ToList();

        //            if (!string.IsNullOrEmpty(SearchTxt))
        //                SearchTxt = SearchTxt.ToUpper();

        //    var companyId = CompanyInfo.Id;
        //    string[] extensions = new string[] { "MP4", "WEBM", "MPG", "OBG", "MOV", "mp4" };
        //    var automationTagsList = CommonStaticData.GetCachedAutomationTagsList(CompanyInfo.ClientCode);



        //    try
        //    {
        //        using (var UOWObj = new AutomationUnitOfWork())
        //        {


        //            IEnumerable<Db.Quiz> quizs = new List<Db.Quiz>();
        //            var includeProperties = "QuizDetails, QuizDetails.QuestionsInQuiz, QuizTagDetails, FavoriteQuizByUser";
        //            quizs = UOWObj.QuizRepository.GetWithPagination(out totalCount, PageNo, PageSize,
        //                filter: (r =>
        //                                (QuizIds.Any() ? QuizIds.Any(s => s == r.Id) : true) && (quizTypeIds.Contains(r.QuizType)) && (r.Company.Id == companyId)
        //                                   && (IsPublished.HasValue ? (IsPublished.Value ? (r.State == (int)QuizStateEnum.PUBLISHED) : (r.State != (int)QuizStateEnum.PUBLISHED)) : true)
        //                                   && (!string.IsNullOrEmpty(r.AccessibleOfficeId) ? (((IsGlobalOfficeAdmin && IsDataforGlobalOfficeAdmin && !OfficeIdList.Any()) ? true : OfficeIdList.Contains(r.AccessibleOfficeId))) : (IncludeSharedWithMe))
        //                                   && (IsFavorite.HasValue ? (IsFavorite.Value ? r.FavoriteQuizByUser.Any(f => f.QuizId == r.Id && f.UserTokenId == UserInfoId) : !r.FavoriteQuizByUser.Any(f => f.QuizId == r.Id && f.UserTokenId == UserInfoId)) : true)
        //                                   && (QuizTagId.HasValue ? r.QuizTagDetails.Any(q => q.TagId == QuizTagId.Value) : true)
        //                                   && (string.IsNullOrEmpty(SearchTxt) ? true : (string.IsNullOrEmpty(r.QuizDetails.FirstOrDefault(t => t.State == (int)QuizStateEnum.DRAFTED).QuizTitle) ? false : r.QuizDetails.FirstOrDefault(t => t.State == (int)QuizStateEnum.DRAFTED).QuizTitle.ToUpper().IndexOf(SearchTxt) > -1))
        //                                   && ((r.QuizDetails.FirstOrDefault(s => s.State == (int)QuizStateEnum.DRAFTED && s.Status == (int)StatusEnum.Active)) != null)
        //                                    && (UsageType.HasValue  ? (r.UsageTypeInQuiz.Any(s => s.UsageType == UsageType.Value)) : true)
        //                                   ),
        //                 orderBy: r => OrderBy == (int)OrderByEnum.Ascending ? (r.OrderBy(k => k.QuizDetails.OrderBy(s => s.Version).FirstOrDefault(s => s.State == (int)QuizStateEnum.DRAFTED).CreatedOn))
        //                                   : (r.OrderByDescending(k => k.QuizDetails.OrderByDescending(s => s.Version).FirstOrDefault(s => s.State == (int)QuizStateEnum.DRAFTED).CreatedOn)),
        //                 includeProperties: includeProperties);

        //            quizlist.TotalRecords = totalCount;
        //            quizlist.CurrentPageIndex = PageNo;



        //            foreach (var quiz in quizs)
        //            {
        //                var quizObj = new LocalQuiz();

        //                var quizDetailsObj = quiz.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active);

        //                if (quizDetailsObj != null)
        //                {
        //                    quizObj.Id = quiz.Id;
        //                    quizObj.QuizTitle = quizDetailsObj.QuizTitle;
        //                    quizObj.IsPublished = quiz.State == (int)QuizStateEnum.PUBLISHED;
        //                    quizObj.IsBranchingLogicEnabled = quizDetailsObj.IsBranchingLogicEnabled;
        //                    quizObj.CreatedByID = quizDetailsObj.CreatedBy;
        //                    //quizObj.UsageTypes = (List<int>)quiz.UsageTypeInQuiz.Select(v => v.UsageType);
        //                    //quizObj.UsageTypes = quiz.UsageTypeInQuiz.Select(v => v.UsageType).Select(c => int.Parse(c.ToString())).ToList();
        //                    if (quizDetailsObj.LastUpdatedOn.HasValue)
        //                    {
        //                        quizObj.LastEditDate = Utility.ConvertUTCDateToLocalDate(quizDetailsObj.LastUpdatedOn.Value, OffsetValue);
        //                    }
        //                    quizObj.CreatedOn = Utility.ConvertUTCDateToLocalDate(quizDetailsObj.CreatedOn, OffsetValue);
        //                    quizObj.QuizCoverDetails = new QuizCover();
        //                    quizObj.QuizCoverDetails.QuizCoverImage = (quizDetailsObj.QuizCoverImage != null &&
        //                                                             quizDetailsObj.QuizCoverImage.Contains("/video/upload/") &&
        //                                                             (quizDetailsObj.QuizCoverImage.EndsWith("MP4") || quizDetailsObj.QuizCoverImage.EndsWith("WEBM") || quizDetailsObj.QuizCoverImage.EndsWith("MPG") || quizDetailsObj.QuizCoverImage.EndsWith("OBG") || quizDetailsObj.QuizCoverImage.EndsWith("MOV") || quizDetailsObj.QuizCoverImage.EndsWith("mp4"))
        //                                                                 ? quizDetailsObj.QuizCoverImage.Split(extensions, System.StringSplitOptions.RemoveEmptyEntries).GetValue(0).ToString() + "jpg"
        //                                                                 : quizDetailsObj.QuizCoverImage);
        //                    quizObj.QuizCoverDetails.QuizCoverTitle = quizDetailsObj.QuizCoverTitle;
        //                    quizObj.QuizCoverDetails.PublicIdForQuizCover = quizDetailsObj.PublicId;
        //                    quizObj.NoOfQusetions = quizDetailsObj.QuestionsInQuiz.Count(r => r.Status == (int)StatusEnum.Active);

        //                    quizObj.PublishedCode = quiz.PublishedCode;
        //                    quizObj.QuizType = (QuizTypeEnum)quiz.QuizType;

        //                    if (!string.IsNullOrEmpty(quiz.AccessibleOfficeId))
        //                    {
        //                        quizObj.AccessibleOfficeId = quiz.AccessibleOfficeId;
        //                    }

        //                    if (quiz.QuizDetails.FirstOrDefault().CreatedBy == BusinessUserID)
        //                        quizObj.IsCreatedByYou = true;

        //                    quizObj.IsFavorited = quiz.FavoriteQuizByUser.Any(f => f.QuizId == quiz.Id && f.UserTokenId == UserInfoId);
        //                    var usageTypeInQuiz = quiz.UsageTypeInQuiz.Where(r => r.QuizId == quiz.Id).Select(v => v.UsageType);
        //                    //var usageTypeInQuiz = quiz.UsageTypeInQuiz.Where(r => r.QuizId == quiz.Id).Select(v => v.UsageType);

        //                    quizObj.IsCreateStandardAutomation = IsCreateStandardAutomation;


        //                    quizObj.UsageTypes = new List<int>();
        //                    foreach (var item in usageTypeInQuiz)
        //                    {
        //                        quizObj.UsageTypes.Add(item);
        //                    }



        //                    quizObj.Tag = new List<Tags>();
        //                    foreach (var tag in quiz.QuizTagDetails.ToList())
        //                    {
        //                        if (automationTagsList != null && automationTagsList.Any(r => r.tagId == tag.TagId))
        //                        {
        //                            var tagDetails = automationTagsList.First(r => r.tagId == tag.TagId);
        //                            quizObj.Tag.Add(new Tags()
        //                            {
        //                                TagId = tag.TagId,
        //                                TagName = tagDetails.tagName,
        //                                TagCode = tagDetails.tagCode
        //                            });
        //                        }
        //                    }

        //                    lstQuiz.Add(quizObj);
        //                }
        //            }

        //            quizlist.Quiz = lstQuiz;

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Status = ResultEnum.Error;
        //        ErrorMessage = ex.Message;
        //        throw ex;
        //    }

        //    return quizlist;
        //}

        //public void UpdateBrandingAndStyle(QuizBrandingAndStyleModel BrandingAndStyleObj, int BusinessUserId, int CompanyId)
        //{
        //    try
        //    {
        //        using (var UOWObj = new AutomationUnitOfWork())
        //        {
        //            var quizObj = UOWObj.QuizRepository.GetByID(BrandingAndStyleObj.QuizId);

        //            if (quizObj != null)
        //            {
        //                var automationListCacheKey = "AutomationList_CompanyId_" + CompanyId;
        //                var quizDetailsObj = quizObj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED);

        //                if (quizDetailsObj != null)
        //                {
        //                    var brandingAndStyle = quizDetailsObj.QuizBrandingAndStyle.FirstOrDefault();

        //                    if (brandingAndStyle != null)
        //                    {
        //                        brandingAndStyle.ImageFileURL = BrandingAndStyleObj.ImageFileURL;
        //                        brandingAndStyle.PublicId = BrandingAndStyleObj.PublicIdForFileURL;
        //                        brandingAndStyle.BackgroundColor = BrandingAndStyleObj.BackgroundColor;
        //                        brandingAndStyle.ButtonColor = BrandingAndStyleObj.ButtonColor;
        //                        brandingAndStyle.OptionColor = BrandingAndStyleObj.OptionColor;
        //                        brandingAndStyle.ButtonFontColor = BrandingAndStyleObj.ButtonFontColor;
        //                        brandingAndStyle.OptionFontColor = BrandingAndStyleObj.OptionFontColor;
        //                        brandingAndStyle.FontColor = BrandingAndStyleObj.FontColor;
        //                        brandingAndStyle.BackgroundColorofSelectedAnswer = BrandingAndStyleObj.BackgroundColorofSelectedAnswer;
        //                        brandingAndStyle.BackgroundColorofAnsweronHover = BrandingAndStyleObj.BackgroundColorofAnsweronHover;
        //                        brandingAndStyle.AnswerTextColorofSelectedAnswer = BrandingAndStyleObj.AnswerTextColorofSelectedAnswer;
        //                        brandingAndStyle.FontType = BrandingAndStyleObj.FontType;
        //                        brandingAndStyle.ApplyToAll = BrandingAndStyleObj.ApplyToAll;
        //                        brandingAndStyle.LogoUrl = BrandingAndStyleObj.LogoUrl;
        //                        brandingAndStyle.LogoPublicId = BrandingAndStyleObj.LogoPublicId;
        //                        brandingAndStyle.BackgroundColorofLogo = BrandingAndStyleObj.BackgroundColorofLogo;
        //                        brandingAndStyle.AutomationAlignment = BrandingAndStyleObj.AutomationAlignment;
        //                        brandingAndStyle.LogoAlignment = BrandingAndStyleObj.LogoAlignment;
        //                        brandingAndStyle.Flip = BrandingAndStyleObj.Flip;
        //                        brandingAndStyle.Language = BrandingAndStyleObj.Language.HasValue ? BrandingAndStyleObj.Language : 1;

        //                        if (BrandingAndStyleObj.IsBackType == (int)BackTypeEnum.Image)
        //                        {
        //                            brandingAndStyle.IsBackType = (int)BackTypeEnum.Image;
        //                            brandingAndStyle.BackImageFileURL = BrandingAndStyleObj.BackImageFileURL;
        //                            brandingAndStyle.Opacity = BrandingAndStyleObj.Opacity;
        //                        }
        //                        else if (BrandingAndStyleObj.IsBackType == (int)BackTypeEnum.Color)
        //                        {
        //                            brandingAndStyle.IsBackType = (int)BackTypeEnum.Color;
        //                            brandingAndStyle.BackColor = BrandingAndStyleObj.BackColor;
        //                            brandingAndStyle.Opacity = BrandingAndStyleObj.Opacity;
        //                        }
        //                        brandingAndStyle.LastUpdatedBy = BusinessUserId;
        //                        brandingAndStyle.LastUpdatedOn = DateTime.UtcNow;

        //                        UOWObj.QuizBrandingAndStyleRepository.Update(brandingAndStyle);
        //                    }
        //                    else
        //                    {
        //                        brandingAndStyle = new Db.QuizBrandingAndStyle();

        //                        brandingAndStyle.QuizId = quizDetailsObj.Id;
        //                        brandingAndStyle.ImageFileURL = BrandingAndStyleObj.ImageFileURL;
        //                        brandingAndStyle.PublicId = BrandingAndStyleObj.PublicIdForFileURL;
        //                        brandingAndStyle.BackgroundColor = BrandingAndStyleObj.BackgroundColor;
        //                        brandingAndStyle.ButtonColor = BrandingAndStyleObj.ButtonColor;
        //                        brandingAndStyle.OptionColor = BrandingAndStyleObj.OptionColor;
        //                        brandingAndStyle.ButtonFontColor = BrandingAndStyleObj.ButtonFontColor;
        //                        brandingAndStyle.OptionFontColor = BrandingAndStyleObj.OptionFontColor;
        //                        brandingAndStyle.FontColor = BrandingAndStyleObj.FontColor;
        //                        brandingAndStyle.FontType = BrandingAndStyleObj.FontType;
        //                        brandingAndStyle.BackgroundColorofSelectedAnswer = BrandingAndStyleObj.BackgroundColorofSelectedAnswer;
        //                        brandingAndStyle.BackgroundColorofAnsweronHover = BrandingAndStyleObj.BackgroundColorofAnsweronHover;
        //                        brandingAndStyle.AnswerTextColorofSelectedAnswer = BrandingAndStyleObj.AnswerTextColorofSelectedAnswer;
        //                        brandingAndStyle.ApplyToAll = BrandingAndStyleObj.ApplyToAll;
        //                        brandingAndStyle.LogoUrl = BrandingAndStyleObj.LogoUrl;
        //                        brandingAndStyle.LogoPublicId = BrandingAndStyleObj.LogoPublicId;
        //                        brandingAndStyle.BackgroundColorofLogo = BrandingAndStyleObj.BackgroundColorofLogo;
        //                        brandingAndStyle.AutomationAlignment = BrandingAndStyleObj.AutomationAlignment;
        //                        brandingAndStyle.LogoAlignment = BrandingAndStyleObj.LogoAlignment;
        //                        brandingAndStyle.Flip = BrandingAndStyleObj.Flip;
        //                        brandingAndStyle.Language = BrandingAndStyleObj.Language.HasValue ? BrandingAndStyleObj.Language : 1;

        //                        if (BrandingAndStyleObj.IsBackType == (int)BackTypeEnum.Image)
        //                        {
        //                            brandingAndStyle.IsBackType = (int)BackTypeEnum.Image;
        //                            brandingAndStyle.BackImageFileURL = BrandingAndStyleObj.BackImageFileURL;
        //                            brandingAndStyle.Opacity = BrandingAndStyleObj.Opacity;
        //                        }
        //                        else if (BrandingAndStyleObj.IsBackType == (int)BackTypeEnum.Color)
        //                        {
        //                            brandingAndStyle.IsBackType = (int)BackTypeEnum.Color;
        //                            brandingAndStyle.BackColor = BrandingAndStyleObj.BackColor;
        //                            brandingAndStyle.Opacity = BrandingAndStyleObj.Opacity;
        //                        }
        //                        brandingAndStyle.LastUpdatedBy = BusinessUserId;
        //                        brandingAndStyle.LastUpdatedOn = DateTime.UtcNow;

        //                        UOWObj.QuizBrandingAndStyleRepository.Insert(brandingAndStyle);
        //                    }

        //                    if (BrandingAndStyleObj.ApplyToAll)
        //                    {
        //                        var questionList = quizDetailsObj.QuestionsInQuiz.Where(r => r.Status == (int)StatusEnum.Active);
        //                        foreach (var ques in questionList)
        //                        {
        //                            ques.NextButtonColor = BrandingAndStyleObj.ButtonColor;
        //                            ques.NextButtonTxtColor = BrandingAndStyleObj.ButtonFontColor;
        //                        }

        //                        var resultList = quizDetailsObj.QuizResults.Where(r => r.Status == (int)StatusEnum.Active);
        //                        foreach (var result in resultList)
        //                        {
        //                            result.ActionButtonColor = BrandingAndStyleObj.ButtonColor;
        //                            result.ActionButtonTitleColor = BrandingAndStyleObj.ButtonFontColor;
        //                        }

        //                        var personalityResultSettingList = quizDetailsObj.PersonalityResultSetting;
        //                        foreach (var personalityResultSetting in personalityResultSettingList)
        //                        {
        //                            personalityResultSetting.ButtonColor = BrandingAndStyleObj.ButtonColor;
        //                            personalityResultSetting.ButtonFontColor = BrandingAndStyleObj.ButtonFontColor;
        //                        }
        //                    }

        //                    quizDetailsObj.LastUpdatedBy = BusinessUserId;
        //                    quizDetailsObj.LastUpdatedOn = DateTime.UtcNow;

        //                    quizObj.State = (int)QuizStateEnum.DRAFTED;

        //                    UOWObj.QuizRepository.Update(quizObj);

        //                    UOWObj.Save();

        //                    AppLocalCache.Remove(automationListCacheKey);
        //                    AppLocalCache.Remove(automationListCacheKey);
        //                }
        //            }
        //            else
        //            {
        //                Status = ResultEnum.Error;
        //                ErrorMessage = "Quiz not found for the QuizId " + BrandingAndStyleObj.QuizId;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Status = ResultEnum.Error;
        //        ErrorMessage = ex.Message;
        //        throw ex;
        //    }
        //}

        public QuizBrandingAndStyleModel GetQuizBrandingAndStyle(int QuizId)
        {
            QuizBrandingAndStyleModel BrandingAndStyleObj = null;

            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizObj = UOWObj.QuizRepository.GetByID(QuizId);

                    if (quizObj != null)
                    {
                        var quizDetailsObj = quizObj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED);

                        if (quizDetailsObj != null)
                        {
                            var brandingAndStyle = quizDetailsObj.QuizBrandingAndStyle.FirstOrDefault();

                            BrandingAndStyleObj = new QuizBrandingAndStyleModel();
                            BrandingAndStyleObj.QuizId = QuizId;

                            if (brandingAndStyle != null)
                            {
                                BrandingAndStyleObj.ImageFileURL = brandingAndStyle.ImageFileURL;
                                BrandingAndStyleObj.PublicIdForFileURL = brandingAndStyle.PublicId;
                                BrandingAndStyleObj.BackgroundColor = brandingAndStyle.BackgroundColor;
                                BrandingAndStyleObj.ButtonColor = brandingAndStyle.ButtonColor;
                                BrandingAndStyleObj.OptionColor = brandingAndStyle.OptionColor;
                                BrandingAndStyleObj.ButtonFontColor = brandingAndStyle.ButtonFontColor;
                                BrandingAndStyleObj.OptionFontColor = brandingAndStyle.OptionFontColor;
                                BrandingAndStyleObj.FontColor = brandingAndStyle.FontColor;
                                BrandingAndStyleObj.FontType = brandingAndStyle.FontType;
                                BrandingAndStyleObj.ButtonHoverColor = brandingAndStyle.ButtonHoverColor;
                                BrandingAndStyleObj.ButtonHoverTextColor = brandingAndStyle.ButtonHoverTextColor;
                                BrandingAndStyleObj.BackgroundColorofSelectedAnswer = brandingAndStyle.BackgroundColorofSelectedAnswer;
                                BrandingAndStyleObj.BackgroundColorofAnsweronHover = brandingAndStyle.BackgroundColorofAnsweronHover;
                                BrandingAndStyleObj.AnswerTextColorofSelectedAnswer = brandingAndStyle.AnswerTextColorofSelectedAnswer;
                                BrandingAndStyleObj.ApplyToAll = brandingAndStyle.ApplyToAll;
                                BrandingAndStyleObj.IsBackType = brandingAndStyle.IsBackType;
                                BrandingAndStyleObj.BackColor = brandingAndStyle.BackColor;
                                BrandingAndStyleObj.Opacity = brandingAndStyle.Opacity;
                                BrandingAndStyleObj.BackImageFileURL = brandingAndStyle.BackImageFileURL;
                                BrandingAndStyleObj.LogoUrl = brandingAndStyle.LogoUrl ?? string.Empty;
                                BrandingAndStyleObj.LogoPublicId = brandingAndStyle.LogoPublicId;
                                BrandingAndStyleObj.BackgroundColorofLogo = brandingAndStyle.BackgroundColorofLogo ?? string.Empty;
                                BrandingAndStyleObj.AutomationAlignment = brandingAndStyle.AutomationAlignment ?? string.Empty;
                                BrandingAndStyleObj.LogoAlignment = brandingAndStyle.LogoAlignment ?? string.Empty;
                                BrandingAndStyleObj.Flip = brandingAndStyle.Flip;
                                BrandingAndStyleObj.Language = brandingAndStyle.Language;
                            }
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz not found for the QuizId " + QuizId;
                    }
                    return BrandingAndStyleObj;
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public QuizDetailsModel GetQuizPreviousQuestionSetting(int QuizId)
        {
            QuizDetailsModel QuizDetails = null;

            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizObj = UOWObj.QuizRepository.GetByID(QuizId);

                    if (quizObj != null)
                    {
                        var quizDetailsObj = quizObj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED);

                        if (quizDetailsObj != null)
                        {
                            QuizDetails = new QuizDetailsModel();
                            QuizDetails.QuizId = quizDetailsObj.ParentQuizId;
                            QuizDetails.ViewPreviousQuestion = quizDetailsObj.ViewPreviousQuestion;
                            QuizDetails.EditAnswer = quizDetailsObj.EditAnswer;
                            QuizDetails.ApplyToAll = quizDetailsObj.ApplyToAll;
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz not found for the QuizId " + QuizId;
                    }
                    return QuizDetails;
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public void UpdateQuizAccessSetting(LocalQuiz QuizObj, int BusinessUserId, int CompanyId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    using (var transaction = Utility.CreateTransactionScope())
                    {
                        var obj = UOWObj.QuizRepository.GetByID(QuizObj.Id);

                        if (obj != null)
                        {
                            var currentDate = DateTime.UtcNow;

                            var NotificationTemplatesInQuiz = obj.NotificationTemplatesInQuiz.Where(r => r.Quiz.AccessibleOfficeId != QuizObj.AccessibleOfficeId).ToList();// office id of quiz not equal to updating office id

                            foreach (var Obj in NotificationTemplatesInQuiz)
                            {
                                UOWObj.NotificationTemplatesInQuizRepository.Delete(Obj);
                            }
                            UOWObj.Save();

                            if (!string.IsNullOrEmpty(QuizObj.AccessibleOfficeId))
                            {
                                obj.AccessibleOfficeId = QuizObj.AccessibleOfficeId;
                            }
                            else
                            {
                                obj.AccessibleOfficeId = null;
                            }

                            var quizDetails = obj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED);

                            if (quizDetails != null)
                            {
                                quizDetails.LastUpdatedBy = BusinessUserId;
                                quizDetails.LastUpdatedOn = currentDate;
                            }

                            UOWObj.QuizRepository.Update(obj);

                            UOWObj.Save();

                            var accessibleUserList = obj.UserAccessInQuiz.ToList();

                            foreach (var user in accessibleUserList)
                            {
                                UOWObj.UserAccessInQuizRepository.Delete(user);
                            }

                            UOWObj.Save();

                            transaction.Complete();

                            
                            
                            
                            
                            
                        }
                        else
                        {
                            Status = ResultEnum.Error;
                            ErrorMessage = "Quiz not found for the QuizId " + QuizObj.Id;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public LocalQuiz GetQuizAccessSetting(int QuizId)
        {
            LocalQuiz quizObj = null;
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {

                    var obj = UOWObj.QuizRepository.GetByID(QuizId);

                    if (obj != null)
                    {
                        quizObj = new LocalQuiz();

                        quizObj.Id = QuizId;

                        if (!string.IsNullOrEmpty(obj.AccessibleOfficeId))
                        {
                            quizObj.AccessibleOfficeId = obj.AccessibleOfficeId;
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz not found for the QuizId " + QuizId;
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
            return quizObj;
        }

        public void UpdateQuizSocialShareSetting(QuizSocialShareSetting QuizSocialShareSettingObj, int BusinessUserId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var obj = UOWObj.QuizRepository.GetByID(QuizSocialShareSettingObj.QuizId);

                    if (obj != null)
                    {
                        var quizDetails = obj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED);

                        if (quizDetails != null)
                        {
                            quizDetails.HideSocialShareButtons = QuizSocialShareSettingObj.HideSocialShareButtons;
                            quizDetails.EnableFacebookShare = QuizSocialShareSettingObj.EnableFacebookShare;
                            quizDetails.EnableTwitterShare = QuizSocialShareSettingObj.EnableTwitterShare;
                            quizDetails.EnableLinkedinShare = QuizSocialShareSettingObj.EnableLinkedinShare;
                            quizDetails.LastUpdatedBy = BusinessUserId;
                            quizDetails.LastUpdatedOn = DateTime.UtcNow;

                            obj.State = (int)QuizStateEnum.DRAFTED;

                            UOWObj.QuizRepository.Update(obj);
                            UOWObj.Save();
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz not found for the QuizId " + QuizSocialShareSettingObj.QuizId;
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public QuizSocialShareSetting GetQuizSocialShareSetting(int QuizId)
        {
            QuizSocialShareSetting quizSocialShareSettingObj = null;
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {

                    var obj = UOWObj.QuizRepository.GetByID(QuizId);

                    if (obj != null)
                    {
                        var quizDetails = obj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED);

                        if (quizDetails != null)
                        {
                            quizSocialShareSettingObj = new QuizSocialShareSetting();

                            quizSocialShareSettingObj.QuizId = QuizId;
                            quizSocialShareSettingObj.HideSocialShareButtons = quizDetails.HideSocialShareButtons;
                            quizSocialShareSettingObj.EnableFacebookShare = quizDetails.EnableFacebookShare;
                            quizSocialShareSettingObj.EnableTwitterShare = quizDetails.EnableTwitterShare;
                            quizSocialShareSettingObj.EnableLinkedinShare = quizDetails.EnableLinkedinShare;
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz not found for the QuizId " + QuizId;
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
            return quizSocialShareSettingObj;
        }

        //public QuizQuestion AddQuizQuestion(int QuizId, int BusinessUserId, int CompanyId, int Type, bool isWhatsappEnable = false)
        //{
        //    QuizQuestion quizQuestionObj = null;
        //    try
        //    {
        //        using (var UOWObj = new AutomationUnitOfWork())
        //        {
        //            using (var transaction = Utility.CreateTransactionScope())
        //            {
        //                var quizObj = UOWObj.QuizRepository.GetByID(QuizId);

        //                if (quizObj != null && quizObj.QuizDetails.Any())
        //                {
        //                    var quizDetails = quizObj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED);

        //                    if (quizDetails != null)
        //                    {
        //                        #region Adding obj

        //                        var currentDate = DateTime.UtcNow;

        //                        var existingQuestionInQuiz = quizDetails.QuestionsInQuiz.FirstOrDefault();

        //                        var contentsInQuiz = quizDetails.ContentsInQuiz.Where(a => a.Status == (int)StatusEnum.Active);
        //                        var questionInQuiz = quizDetails.QuestionsInQuiz.Where(a => a.Status == (int)StatusEnum.Active);
        //                        var contentsInQuizCount = contentsInQuiz.Count();
        //                        var questionInQuizCount = questionInQuiz.Count();


        //                        var obj = new Db.QuestionsInQuiz();

        //                        obj.QuizId = quizDetails.Id;
        //                        obj.ShowAnswerImage = false;
        //                        if (Type == (int)BranchingLogicEnum.QUESTION)
        //                            obj.Question = "Question " + (questionInQuiz.Count(r => r.Type == (int)BranchingLogicEnum.QUESTION) + 1).ToString();
        //                        else if (Type == (int)BranchingLogicEnum.CONTENT)
        //                            obj.Question = "Content " + (questionInQuiz.Count(r => r.Type == (int)BranchingLogicEnum.CONTENT) + 1).ToString();
        //                        obj.ShowTitle = quizObj.UsageTypeInQuiz.Any(a => a.UsageType == (int)UsageTypeEnum.WhatsAppChatbot) ? false : true;
        //                        obj.ShowQuestionImage = true;
        //                        obj.QuestionImage = string.Empty;
        //                        obj.PublicId = string.Empty;
        //                        obj.Status = (int)StatusEnum.Active;
        //                        obj.State = (int)QuizStateEnum.DRAFTED;
        //                        obj.AnswerType = (int)AnswerTypeEnum.Single;
        //                        obj.NextButtonText = "Next";
        //                        obj.NextButtonTxtSize = "24px";
        //                        obj.ViewPreviousQuestion = quizDetails.ViewPreviousQuestion;
        //                        obj.EditAnswer = quizDetails.EditAnswer;
        //                        obj.AutoPlay = true;
        //                        obj.SecondsToApply = "0";
        //                        obj.VideoFrameEnabled = false;
        //                        obj.Description = quizObj.UsageTypeInQuiz.Any(a => a.UsageType == (int)UsageTypeEnum.WhatsAppChatbot) ? "Message" : "Description";
        //                        obj.DescriptionImage = string.Empty;
        //                        obj.EnableMediaFileForDescription = false;
        //                        obj.PublicIdForDescription = string.Empty;
        //                        obj.ShowDescriptionImage = false;
        //                        obj.AutoPlayForDescription = true;
        //                        obj.SecondsToApplyForDescription = "0";
        //                        obj.DescVideoFrameEnabled = false;
        //                        obj.Type = Type;
        //                        obj.DisplayOrderForTitleImage = 2;
        //                        obj.DisplayOrderForTitle = 1;
        //                        obj.DisplayOrderForDescriptionImage = 4;
        //                        obj.DisplayOrderForDescription = 3;
        //                        obj.DisplayOrderForAnswer = 5;
        //                        obj.DisplayOrderForNextButton = 6;
        //                        obj.EnableNextButton = true;
        //                        obj.ShowDescription = true;
        //                        obj.EnableComment = false;
        //                        obj.TopicTitle = string.Empty;

        //                        obj.CorrectAnswerDescription = string.Empty;

        //                        if (existingQuestionInQuiz != null)
        //                        {
        //                            obj.RevealCorrectAnswer = existingQuestionInQuiz.RevealCorrectAnswer;
        //                            obj.AliasTextForCorrect = existingQuestionInQuiz.AliasTextForCorrect;
        //                            obj.AliasTextForIncorrect = existingQuestionInQuiz.AliasTextForIncorrect;
        //                            obj.AliasTextForYourAnswer = existingQuestionInQuiz.AliasTextForYourAnswer;
        //                            obj.AliasTextForCorrectAnswer = existingQuestionInQuiz.AliasTextForCorrectAnswer;
        //                            obj.AliasTextForExplanation = existingQuestionInQuiz.AliasTextForExplanation;
        //                            obj.AliasTextForNextButton = existingQuestionInQuiz.AliasTextForNextButton;
        //                        }
        //                        else if (quizObj.QuizType != (int)QuizTypeEnum.Score && quizObj.QuizType != (int)QuizTypeEnum.ScoreTemplate && quizObj.QuizType != (int)QuizTypeEnum.Personality && quizObj.QuizType != (int)QuizTypeEnum.PersonalityTemplate)
        //                        {
        //                            obj.RevealCorrectAnswer = false;
        //                            obj.AliasTextForCorrect = "Correct";
        //                            obj.AliasTextForIncorrect = "Incorrect";
        //                            obj.AliasTextForYourAnswer = "Your Answer";
        //                            obj.AliasTextForCorrectAnswer = "Correct Answer";
        //                            obj.AliasTextForExplanation = "Explanation";
        //                            obj.AliasTextForNextButton = "Next";
        //                        }

        //                        if (isWhatsappEnable)
        //                        {
        //                            if (obj.AnswerType == (int)AnswerTypeEnum.Single)
        //                            {
        //                                obj.AnswerStructureType = (int)AnswerStructureTypeEnum.Button;
        //                            }
        //                            else if (obj.AnswerType == (int)AnswerTypeEnum.NPS)
        //                            {
        //                                obj.AnswerStructureType = (int)AnswerStructureTypeEnum.List;
        //                            }
        //                            else
        //                            {
        //                                obj.AnswerStructureType = (int)AnswerStructureTypeEnum.Default;
        //                            }
        //                        }
        //                        else
        //                        {
        //                            obj.AnswerStructureType = (int)AnswerStructureTypeEnum.Default;
        //                        }

        //                        if ((questionInQuizCount + contentsInQuizCount) == 0)
        //                            obj.DisplayOrder = 1;
        //                        else if (questionInQuizCount != 0 && contentsInQuizCount != 0)
        //                            obj.DisplayOrder = (questionInQuiz.Max(r => r.DisplayOrder) > contentsInQuiz.Max(r => r.DisplayOrder) ? questionInQuiz.Max(r => r.DisplayOrder) + 1 : contentsInQuiz.Max(r => r.DisplayOrder) + 1);
        //                        else if (questionInQuizCount != 0)
        //                            obj.DisplayOrder = (questionInQuiz.Max(r => r.DisplayOrder) + 1);
        //                        else if (contentsInQuizCount != 0)
        //                            obj.DisplayOrder = (contentsInQuiz.Max(r => r.DisplayOrder) + 1);

        //                        obj.LastUpdatedBy = BusinessUserId;
        //                        obj.LastUpdatedOn = currentDate;

        //                        UOWObj.QuestionsInQuizRepository.Insert(obj);
        //                        UOWObj.Save();

        //                        var answerObj = new Db.AnswerOptionsInQuizQuestions();

        //                        answerObj.QuestionId = obj.Id;
        //                        answerObj.QuizId = obj.QuizId;
        //                        answerObj.Option = "Answer 1";
        //                        answerObj.OptionImage = string.Empty;
        //                        answerObj.PublicId = string.Empty;
        //                        answerObj.LastUpdatedBy = BusinessUserId;
        //                        answerObj.LastUpdatedOn = currentDate;

        //                        if (quizObj.QuizType == (int)QuizTypeEnum.Score || quizObj.QuizType == (int)QuizTypeEnum.ScoreTemplate)
        //                        {
        //                            answerObj.AssociatedScore = default(int);
        //                            answerObj.IsCorrectAnswer = false;
        //                        }
        //                        else if (quizObj.QuizType == (int)QuizTypeEnum.Personality || quizObj.QuizType == (int)QuizTypeEnum.PersonalityTemplate)
        //                            answerObj.IsCorrectAnswer = false;
        //                        else
        //                            answerObj.IsCorrectAnswer = true;

        //                        answerObj.DisplayOrder = 1;
        //                        answerObj.Status = (int)StatusEnum.Active;
        //                        answerObj.State = (int)QuizStateEnum.DRAFTED;
        //                        answerObj.AutoPlay = true;
        //                        answerObj.SecondsToApply = "0";
        //                        answerObj.VideoFrameEnabled = false;

        //                        UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);


        //                        answerObj = new Db.AnswerOptionsInQuizQuestions();

        //                        answerObj.QuestionId = obj.Id;
        //                        answerObj.QuizId = obj.QuizId;
        //                        answerObj.Option = "Answer 2";
        //                        answerObj.OptionImage = string.Empty;
        //                        answerObj.PublicId = string.Empty;
        //                        answerObj.LastUpdatedBy = BusinessUserId;
        //                        answerObj.LastUpdatedOn = currentDate;

        //                        if (quizObj.QuizType == (int)QuizTypeEnum.Score || quizObj.QuizType == (int)QuizTypeEnum.ScoreTemplate)
        //                        {
        //                            answerObj.AssociatedScore = default(int);
        //                            answerObj.IsCorrectAnswer = false;
        //                        }
        //                        else
        //                            answerObj.IsCorrectAnswer = false;

        //                        answerObj.DisplayOrder = 2;
        //                        answerObj.Status = (int)StatusEnum.Active;
        //                        answerObj.State = (int)QuizStateEnum.DRAFTED;
        //                        answerObj.AutoPlay = true;
        //                        answerObj.SecondsToApply = "0";
        //                        answerObj.VideoFrameEnabled = false;

        //                        UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);

        //                        quizDetails.LastUpdatedBy = BusinessUserId;
        //                        quizDetails.LastUpdatedOn = currentDate;

        //                        quizObj.State = (int)QuizStateEnum.DRAFTED;


        //                        if (quizObj.QuizType == (int)QuizTypeEnum.Score || quizObj.QuizType == (int)QuizTypeEnum.ScoreTemplate || quizObj.QuizType == (int)QuizTypeEnum.Personality || quizObj.QuizType == (int)QuizTypeEnum.PersonalityTemplate)
        //                        {
        //                            obj.MinAnswer = 1;
        //                            obj.MaxAnswer = 1;
        //                        }
        //                        UOWObj.QuestionsInQuizRepository.Update(obj);


        //                        #region for unanswered answer option

        //                        answerObj = new Db.AnswerOptionsInQuizQuestions();

        //                        answerObj.QuestionId = obj.Id;
        //                        answerObj.QuizId = obj.QuizId;
        //                        answerObj.Option = "Unanswered";
        //                        answerObj.OptionImage = string.Empty;
        //                        answerObj.PublicId = string.Empty;
        //                        answerObj.LastUpdatedBy = BusinessUserId;
        //                        answerObj.LastUpdatedOn = currentDate;

        //                        if (quizObj.QuizType == (int)QuizTypeEnum.Score || quizObj.QuizType == (int)QuizTypeEnum.ScoreTemplate)
        //                        {
        //                            answerObj.AssociatedScore = default(int);
        //                            answerObj.IsCorrectAnswer = false;
        //                        }
        //                        else
        //                            answerObj.IsCorrectAnswer = false;

        //                        answerObj.DisplayOrder = 0;
        //                        answerObj.Status = (int)StatusEnum.Active;
        //                        answerObj.State = (int)QuizStateEnum.DRAFTED;
        //                        answerObj.IsUnansweredType = true;
        //                        answerObj.AutoPlay = true;
        //                        answerObj.SecondsToApply = "0";
        //                        answerObj.VideoFrameEnabled = false;

        //                        UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);

        //                        #endregion

        //                        UOWObj.QuizRepository.Update(quizObj);

        //                        UOWObj.Save();
        //                        transaction.Complete();

        //                        #endregion

        //                        #region Bind return obj

        //                        quizQuestionObj = new QuizQuestion();

        //                        quizQuestionObj.QuizType = quizObj.QuizType;
        //                        quizQuestionObj.QuestionId = obj.Id;
        //                        quizQuestionObj.ShowAnswerImage = obj.ShowAnswerImage;
        //                        quizQuestionObj.QuestionTitle = obj.Question;
        //                        quizQuestionObj.QuestionImage = obj.QuestionImage;
        //                        quizQuestionObj.PublicIdForQuestion = obj.PublicId;
        //                        quizQuestionObj.ShowQuestionImage = obj.ShowQuestionImage;
        //                        quizQuestionObj.DisplayOrder = obj.DisplayOrder;
        //                        quizQuestionObj.AnswerType = obj.AnswerType;
        //                        quizQuestionObj.MinAnswer = obj.MinAnswer;
        //                        quizQuestionObj.MaxAnswer = obj.MaxAnswer;
        //                        quizQuestionObj.AutoPlay = obj.AutoPlay;
        //                        quizQuestionObj.SecondsToApply = obj.SecondsToApply ?? "0";
        //                        quizQuestionObj.VideoFrameEnabled = obj.VideoFrameEnabled ?? false;
        //                        quizQuestionObj.Description = obj.Description;
        //                        quizQuestionObj.ShowDescription = obj.ShowDescription;
        //                        quizQuestionObj.DescriptionImage = obj.DescriptionImage;
        //                        quizQuestionObj.EnableMediaFileForDescription = obj.EnableMediaFileForDescription;
        //                        quizQuestionObj.PublicIdForDescription = obj.PublicIdForDescription;
        //                        quizQuestionObj.ShowDescriptionImage = obj.ShowDescriptionImage ?? false;
        //                        quizQuestionObj.AutoPlayForDescription = obj.AutoPlayForDescription;
        //                        quizQuestionObj.SecondsToApplyForDescription = obj.SecondsToApplyForDescription ?? "0";
        //                        quizQuestionObj.DescVideoFrameEnabled = obj.DescVideoFrameEnabled ?? false;
        //                        quizQuestionObj.Type = obj.Type;
        //                        quizQuestionObj.EnableNextButton = obj.EnableNextButton;
        //                        quizQuestionObj.DisplayOrderForTitleImage = obj.DisplayOrderForTitleImage;
        //                        quizQuestionObj.DisplayOrderForTitle = obj.DisplayOrderForTitle;
        //                        quizQuestionObj.DisplayOrderForDescriptionImage = obj.DisplayOrderForDescriptionImage;
        //                        quizQuestionObj.DisplayOrderForDescription = obj.DisplayOrderForDescription;
        //                        quizQuestionObj.DisplayOrderForAnswer = obj.DisplayOrderForAnswer;
        //                        quizQuestionObj.DisplayOrderForNextButton = obj.DisplayOrderForNextButton;
        //                        quizQuestionObj.AnswerStructureType = obj.AnswerStructureType;
        //                        //if(isWhatsappEnable)
        //                        //{
        //                        //    quizQuestionObj.AnswerStructureType =(int)AnswerStructureType.List;
        //                        //}
        //                        //else
        //                        //{
        //                        //    quizQuestionObj.AnswerStructureType = (int)AnswerStructureType.Default;
        //                        //}

        //                        quizQuestionObj.QuizCorrectAnswerSetting = new QuizCorrectAnswerSetting();

        //                        quizQuestionObj.QuizCorrectAnswerSetting.CorrectAnswerExplanation = obj.CorrectAnswerDescription;
        //                        quizQuestionObj.QuizCorrectAnswerSetting.RevealCorrectAnswer = obj.RevealCorrectAnswer;
        //                        quizQuestionObj.QuizCorrectAnswerSetting.AliasTextForCorrect = obj.AliasTextForCorrect;
        //                        quizQuestionObj.QuizCorrectAnswerSetting.AliasTextForIncorrect = obj.AliasTextForIncorrect;
        //                        quizQuestionObj.QuizCorrectAnswerSetting.AliasTextForYourAnswer = obj.AliasTextForYourAnswer;
        //                        quizQuestionObj.QuizCorrectAnswerSetting.AliasTextForCorrectAnswer = obj.AliasTextForCorrectAnswer;
        //                        quizQuestionObj.QuizCorrectAnswerSetting.AliasTextForExplanation = obj.AliasTextForExplanation;
        //                        quizQuestionObj.QuizCorrectAnswerSetting.AliasTextForNextButton = obj.AliasTextForNextButton;

        //                        quizQuestionObj.AnswerList = new List<AnswerOptionInQuestion>();

        //                        foreach (var answer in obj.AnswerOptionsInQuizQuestions.Where(r => !r.IsUnansweredType).OrderBy(r => r.DisplayOrder))
        //                        {
        //                            quizQuestionObj.AnswerList.Add(new AnswerOptionInQuestion
        //                            {
        //                                AnswerId = answer.Id,
        //                                AnswerText = answer.Option,
        //                                AnswerImage = answer.OptionImage,
        //                                AssociatedScore = answer.AssociatedScore,
        //                                PublicIdForAnswer = answer.PublicId,
        //                                IsCorrectAnswer = answer.IsCorrectAnswer,
        //                                DisplayOrder = answer.DisplayOrder
        //                            });
        //                        }

        //                        #endregion
        //                        
        //                        
        //                    }
        //                }
        //                else
        //                {
        //                    Status = ResultEnum.Error;
        //                    ErrorMessage = "Quiz not found for the QuizId " + QuizId;
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Status = ResultEnum.Error;
        //        ErrorMessage = ex.Message;
        //        throw ex;
        //    }
        //    return quizQuestionObj;
        //}


        //public QuizQuestion AddQuizWhatsAppTemplate(int QuizId, int templateId, string language , int BusinessUserId, int CompanyId, string clientCode, int Type)
        //{
        //    var whatsApptemplateDetails = OWCHelper.GetWhatsAppHSMTemplate(clientCode, "Automation", false, language, null);
        //    if (whatsApptemplateDetails == null)
        //    {
        //        return null;
        //    }
        //    var whatsAppTemplate  = JsonConvert.DeserializeObject<List<WhatsAppTemplateDto>>(whatsApptemplateDetails.ToString()).Where(v => v.Id == templateId && v.TemplateLanguage.Equals(language, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        //    if(whatsAppTemplate == null)
        //    {
        //        return null;
        //    }


        //    QuizQuestion quizQuestionObj = null;
        //    try
        //    {
        //        using (var UOWObj = new AutomationUnitOfWork())
        //        {
        //            using (var transaction = Utility.CreateTransactionScope())
        //            {
        //                var quizObj = UOWObj.QuizRepository.GetByID(QuizId);
        //                var languages = UOWObj.LanguagesRepository.Get(r => r.Culture.Equals(whatsAppTemplate.TemplateLanguage, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();


        //                if (quizObj != null && quizObj.QuizDetails.Any())
        //                {
        //                    var quizDetails = quizObj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED);

        //                    if (quizDetails != null)
        //                    {
        //                        #region Adding obj

        //                        var currentDate = DateTime.UtcNow;

        //                        var existingtemplateQuestionlist = quizDetails.QuestionsInQuiz.Where(v => v.TemplateId.HasValue);
        //                        if(existingtemplateQuestionlist != null &&  existingtemplateQuestionlist.Any())
        //                        {
        //                            foreach (var item in existingtemplateQuestionlist.ToList())
        //                            {
        //                                UOWObj.QuestionsInQuizRepository.Delete(item);
        //                            }
        //                        }


        //                        var obj = new Db.QuestionsInQuiz();

        //                        obj.QuizId = quizDetails.Id;
        //                        obj.ShowAnswerImage = false;
        //                        obj.Question = whatsAppTemplate.TemplateName;
        //                        obj.TemplateId = whatsAppTemplate.Id;
        //                        obj.Description = "Message";
        //                        obj.LanguageId = languages != null ? languages.Id : 0;

        //                        obj.ShowTitle = quizObj.UsageTypeInQuiz.Any(a => a.UsageType == (int)UsageTypeEnum.WhatsAppChatbot) ? false : true; 
        //                        obj.ShowQuestionImage = true;
        //                        obj.QuestionImage = string.Empty;
        //                        obj.PublicId = string.Empty;
        //                        obj.Status = (int)StatusEnum.Active;
        //                        obj.State = (int)QuizStateEnum.DRAFTED;
        //                        obj.AnswerType = quizObj.QuizType == (int)QuizTypeEnum.NPS ? (int)AnswerTypeEnum.NPS : (int)AnswerTypeEnum.Single;
        //                        obj.NextButtonText = "Next";
        //                        obj.NextButtonTxtSize = "24px";
        //                        obj.ViewPreviousQuestion = quizDetails.ViewPreviousQuestion;
        //                        obj.EditAnswer = quizDetails.EditAnswer;
        //                        obj.AutoPlay = true;
        //                        obj.SecondsToApply = "0";
        //                        obj.VideoFrameEnabled = false;

        //                        obj.DescriptionImage = string.Empty;
        //                        obj.EnableMediaFileForDescription = false;
        //                        obj.PublicIdForDescription = string.Empty;
        //                        obj.ShowDescriptionImage = false;
        //                        obj.AutoPlayForDescription = true;
        //                        obj.SecondsToApplyForDescription = "0";
        //                        obj.DescVideoFrameEnabled = false;
        //                        obj.Type = Type;
        //                        obj.DisplayOrderForTitleImage = 2;
        //                        obj.DisplayOrderForTitle = 1;
        //                        obj.DisplayOrderForDescriptionImage = 4;
        //                        obj.DisplayOrderForDescription = 3;
        //                        obj.DisplayOrderForAnswer = 5;
        //                        obj.DisplayOrderForNextButton = 6;
        //                        obj.EnableNextButton = true;
        //                        obj.ShowDescription = true;
        //                        obj.EnableComment = false;
        //                        obj.TopicTitle = string.Empty;
        //                        obj.CorrectAnswerDescription = string.Empty;
        //                        obj.AnswerStructureType = (int)AnswerStructureTypeEnum.Button;
        //                        obj.DisplayOrder = 1;
        //                        obj.LastUpdatedBy = BusinessUserId;
        //                        obj.LastUpdatedOn = currentDate;

        //                        UOWObj.QuestionsInQuizRepository.Insert(obj);
        //                        UOWObj.Save();


        //                        foreach (var cstomComponents in whatsAppTemplate.CustomComponents)
        //                        {
        //                            if (!string.IsNullOrWhiteSpace(cstomComponents.Type) && cstomComponents.Type.EqualsCI("buttons") && (cstomComponents.Items != null && cstomComponents.Items.Any()))
        //                            {
        //                                foreach (var item in cstomComponents.Items)
        //                                {
        //                                    var answerObj = new Db.AnswerOptionsInQuizQuestions();

        //                                    answerObj.QuestionId = obj.Id;
        //                                    answerObj.QuizId = obj.QuizId;
        //                                    answerObj.Option = item.Text;
        //                                    answerObj.RefId = item.Id;
        //                                    answerObj.OptionImage = string.Empty;
        //                                    answerObj.PublicId = string.Empty;
        //                                    answerObj.LastUpdatedBy = BusinessUserId;
        //                                    answerObj.LastUpdatedOn = currentDate;

        //                                    if (quizObj.QuizType == (int)QuizTypeEnum.Score || quizObj.QuizType == (int)QuizTypeEnum.ScoreTemplate)
        //                                    {
        //                                        answerObj.AssociatedScore = default(int);
        //                                        answerObj.IsCorrectAnswer = false;
        //                                    }
        //                                    else if (quizObj.QuizType == (int)QuizTypeEnum.Personality || quizObj.QuizType == (int)QuizTypeEnum.PersonalityTemplate)
        //                                        answerObj.IsCorrectAnswer = false;
        //                                    else if (quizObj.QuizType == (int)QuizTypeEnum.NPS)
        //                                    {
        //                                        answerObj.IsCorrectAnswer = null;
        //                                        answerObj.IsCorrectForMultipleAnswer = null;
        //                                    }
        //                                    else
        //                                        answerObj.IsCorrectAnswer = true;

        //                                    answerObj.DisplayOrder = 1;
        //                                    answerObj.Status = (int)StatusEnum.Active;
        //                                    answerObj.State = (int)QuizStateEnum.DRAFTED;
        //                                    answerObj.AutoPlay = true;
        //                                    answerObj.SecondsToApply = "0";
        //                                    answerObj.VideoFrameEnabled = false;

        //                                    UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);
        //                                }

        //                            }
        //                        }


        //                        quizDetails.LastUpdatedBy = BusinessUserId;
        //                        quizDetails.LastUpdatedOn = currentDate;

        //                        quizObj.State = (int)QuizStateEnum.DRAFTED;


        //                        if (quizObj.QuizType == (int)QuizTypeEnum.Score || quizObj.QuizType == (int)QuizTypeEnum.ScoreTemplate || quizObj.QuizType == (int)QuizTypeEnum.Personality || quizObj.QuizType == (int)QuizTypeEnum.PersonalityTemplate)
        //                        {
        //                            obj.MinAnswer = 1;
        //                            obj.MaxAnswer = 1;
        //                        }
        //                        UOWObj.QuestionsInQuizRepository.Update(obj);
        //                        UOWObj.QuizRepository.Update(quizObj);

        //                        UOWObj.Save();
        //                        transaction.Complete();

        //                        #endregion

        //                        #region Bind return obj

        //                        quizQuestionObj = new QuizQuestion();

        //                        quizQuestionObj.QuizType = quizObj.QuizType;
        //                        quizQuestionObj.QuestionId = obj.Id;
        //                        quizQuestionObj.ShowAnswerImage = obj.ShowAnswerImage;
        //                        quizQuestionObj.QuestionTitle = obj.Question;
        //                        quizQuestionObj.QuestionImage = obj.QuestionImage;
        //                        quizQuestionObj.PublicIdForQuestion = obj.PublicId;
        //                        quizQuestionObj.ShowQuestionImage = obj.ShowQuestionImage;
        //                        quizQuestionObj.DisplayOrder = obj.DisplayOrder;
        //                        quizQuestionObj.AnswerType = obj.AnswerType;
        //                        quizQuestionObj.MinAnswer = obj.MinAnswer;
        //                        quizQuestionObj.MaxAnswer = obj.MaxAnswer;
        //                        quizQuestionObj.AutoPlay = obj.AutoPlay;
        //                        quizQuestionObj.SecondsToApply = obj.SecondsToApply ?? "0";
        //                        quizQuestionObj.VideoFrameEnabled = obj.VideoFrameEnabled ?? false;
        //                        quizQuestionObj.Description = obj.Description;
        //                        quizQuestionObj.ShowDescription = obj.ShowDescription;
        //                        quizQuestionObj.DescriptionImage = obj.DescriptionImage;
        //                        quizQuestionObj.EnableMediaFileForDescription = obj.EnableMediaFileForDescription;
        //                        quizQuestionObj.PublicIdForDescription = obj.PublicIdForDescription;
        //                        quizQuestionObj.ShowDescriptionImage = obj.ShowDescriptionImage ?? false;
        //                        quizQuestionObj.AutoPlayForDescription = obj.AutoPlayForDescription;
        //                        quizQuestionObj.SecondsToApplyForDescription = obj.SecondsToApplyForDescription ?? "0";
        //                        quizQuestionObj.DescVideoFrameEnabled = obj.DescVideoFrameEnabled ?? false;
        //                        quizQuestionObj.Type = obj.Type;
        //                        quizQuestionObj.EnableNextButton = obj.EnableNextButton;
        //                        quizQuestionObj.DisplayOrderForTitleImage = obj.DisplayOrderForTitleImage;
        //                        quizQuestionObj.DisplayOrderForTitle = obj.DisplayOrderForTitle;
        //                        quizQuestionObj.DisplayOrderForDescriptionImage = obj.DisplayOrderForDescriptionImage;
        //                        quizQuestionObj.DisplayOrderForDescription = obj.DisplayOrderForDescription;
        //                        quizQuestionObj.DisplayOrderForAnswer = obj.DisplayOrderForAnswer;
        //                        quizQuestionObj.DisplayOrderForNextButton = obj.DisplayOrderForNextButton;
        //                        quizQuestionObj.AnswerStructureType = obj.AnswerStructureType;
        //                        quizQuestionObj.LanguageId = obj.LanguageId;
        //                        quizQuestionObj.TemplateId = obj.TemplateId;
        //                        quizQuestionObj.LanguageCode = whatsAppTemplate.TemplateLanguage;

        //                        quizQuestionObj.QuizCorrectAnswerSetting = new QuizCorrectAnswerSetting();

        //                        quizQuestionObj.QuizCorrectAnswerSetting.CorrectAnswerExplanation = obj.CorrectAnswerDescription;
        //                        quizQuestionObj.QuizCorrectAnswerSetting.RevealCorrectAnswer = obj.RevealCorrectAnswer;
        //                        quizQuestionObj.QuizCorrectAnswerSetting.AliasTextForCorrect = obj.AliasTextForCorrect;
        //                        quizQuestionObj.QuizCorrectAnswerSetting.AliasTextForIncorrect = obj.AliasTextForIncorrect;
        //                        quizQuestionObj.QuizCorrectAnswerSetting.AliasTextForYourAnswer = obj.AliasTextForYourAnswer;
        //                        quizQuestionObj.QuizCorrectAnswerSetting.AliasTextForCorrectAnswer = obj.AliasTextForCorrectAnswer;
        //                        quizQuestionObj.QuizCorrectAnswerSetting.AliasTextForExplanation = obj.AliasTextForExplanation;
        //                        quizQuestionObj.QuizCorrectAnswerSetting.AliasTextForNextButton = obj.AliasTextForNextButton;

        //                        quizQuestionObj.AnswerList = new List<AnswerOptionInQuestion>();

        //                        if(obj.AnswerOptionsInQuizQuestions != null && obj.AnswerOptionsInQuizQuestions.Any())
        //                        {
        //                            foreach (var answer in obj.AnswerOptionsInQuizQuestions.Where(r => !r.IsUnansweredType).OrderBy(r => r.DisplayOrder))
        //                            {
        //                                quizQuestionObj.AnswerList.Add(new AnswerOptionInQuestion
        //                                {
        //                                    AnswerId = answer.Id,
        //                                    AnswerText = answer.Option,
        //                                    AnswerImage = answer.OptionImage,
        //                                    AssociatedScore = answer.AssociatedScore,
        //                                    PublicIdForAnswer = answer.PublicId,
        //                                    IsCorrectAnswer = answer.IsCorrectAnswer,
        //                                    DisplayOrder = answer.DisplayOrder,
        //                                    RefId = answer.RefId
        //                                });
        //                            }
        //                        }

        //                        #endregion

        //                    }
        //                }
        //                else
        //                {
        //                    Status = ResultEnum.Error;
        //                    ErrorMessage = "Quiz not found for the QuizId " + QuizId;
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Status = ResultEnum.Error;
        //        ErrorMessage = ex.Message;
        //        throw ex;
        //    }
        //    return quizQuestionObj;
        //}

        //public CompanyModel BindCompanyDetails(Db.Company companyInfo)
        //{
        //    CompanyModel companyObj;
        //    if (companyInfo != null)
        //    {
        //        companyObj = new CompanyModel
        //        {
        //            Id = companyInfo.Id,
        //            AlternateClientCodes = companyInfo.AlternateClientCodes,
        //            ClientCode = companyInfo.ClientCode,
        //            CompanyName = companyInfo.CompanyName,
        //            CompanyWebsiteUrl = companyInfo.CompanyWebsiteUrl,
        //            JobRocketApiAuthorizationBearer = companyInfo.JobRocketApiAuthorizationBearer,
        //            JobRocketApiUrl = companyInfo.JobRocketApiUrl,
        //            JobRocketClientUrl = companyInfo.JobRocketClientUrl,
        //            LeadDashboardApiAuthorizationBearer = companyInfo.LeadDashboardApiAuthorizationBearer,
        //            LeadDashboardApiUrl = companyInfo.LeadDashboardApiUrl,
        //            LeadDashboardClientUrl = companyInfo.LeadDashboardClientUrl,
        //            LogoUrl = companyInfo.LogoUrl,
        //            PrimaryBrandingColor = companyInfo.PrimaryBrandingColor,
        //            SecondaryBrandingColor = companyInfo.SecondaryBrandingColor

        //        };
        //    }
        //    else
        //    {
        //        companyObj = new CompanyModel();
        //    }

        //    return companyObj;

        //}


        public QuizQuestion GetQuizQuestionDetails(int QuestionId, int QuizId)
        {
            int quizDetailId = 0;
            QuizQuestion quizQuestionObj = null;
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var questionObj = UOWObj.QuestionsInQuizRepository.GetByID(QuestionId);

                    if (questionObj != null && questionObj.QuizDetails.ParentQuizId == QuizId)
                    {
                        quizQuestionObj = new QuizQuestion();
                        quizDetailId = questionObj.QuizDetails.Id;
                        quizQuestionObj.QuizType = questionObj.QuizDetails.Quiz.QuizType;
                        quizQuestionObj.QuestionId = QuestionId;
                        quizQuestionObj.ShowAnswerImage = questionObj.ShowAnswerImage;
                        quizQuestionObj.QuestionTitle = questionObj.Question;
                        quizQuestionObj.ShowTitle = questionObj.ShowTitle;
                        quizQuestionObj.QuestionImage = questionObj.QuestionImage;
                        quizQuestionObj.EnableMediaFile = questionObj.EnableMediaFile;
                        quizQuestionObj.PublicIdForQuestion = questionObj.PublicId;
                        quizQuestionObj.ShowQuestionImage = questionObj.ShowQuestionImage;
                        quizQuestionObj.AnswerType = questionObj.AnswerType;
                        quizQuestionObj.MaxAnswer = questionObj.MaxAnswer;
                        quizQuestionObj.MinAnswer = questionObj.MinAnswer;
                        quizQuestionObj.NextButtonColor = questionObj.NextButtonColor;
                        quizQuestionObj.NextButtonText = questionObj.NextButtonText;
                        quizQuestionObj.NextButtonTxtColor = questionObj.NextButtonTxtColor;
                        quizQuestionObj.NextButtonTxtSize = questionObj.NextButtonTxtSize;
                        quizQuestionObj.EnableNextButton = questionObj.EnableNextButton;
                        quizQuestionObj.ViewPreviousQuestion = questionObj.ViewPreviousQuestion;
                        quizQuestionObj.EditAnswer = questionObj.EditAnswer;
                        quizQuestionObj.TimerRequired = questionObj.TimerRequired;
                        quizQuestionObj.Time = questionObj.Time;
                        quizQuestionObj.AutoPlay = questionObj.AutoPlay;
                        quizQuestionObj.SecondsToApply = questionObj.SecondsToApply;
                        quizQuestionObj.VideoFrameEnabled = questionObj.VideoFrameEnabled;
                        quizQuestionObj.Description = questionObj.Description;
                        quizQuestionObj.ShowDescription = questionObj.ShowDescription;
                        quizQuestionObj.DescriptionImage = questionObj.DescriptionImage;
                        quizQuestionObj.EnableMediaFileForDescription = questionObj.EnableMediaFileForDescription;
                        quizQuestionObj.PublicIdForDescription = questionObj.PublicIdForDescription;
                        quizQuestionObj.ShowDescriptionImage = questionObj.ShowDescriptionImage ?? false;
                        quizQuestionObj.AutoPlayForDescription = questionObj.AutoPlayForDescription;
                        quizQuestionObj.SecondsToApplyForDescription = questionObj.SecondsToApplyForDescription;
                        quizQuestionObj.DescVideoFrameEnabled = questionObj.DescVideoFrameEnabled;
                        quizQuestionObj.Type = questionObj.Type;
                        quizQuestionObj.DisplayOrderForTitle = questionObj.DisplayOrderForTitle;
                        quizQuestionObj.DisplayOrderForTitleImage = questionObj.DisplayOrderForTitleImage;
                        quizQuestionObj.DisplayOrderForDescription = questionObj.DisplayOrderForDescription;
                        quizQuestionObj.DisplayOrderForDescriptionImage = questionObj.DisplayOrderForDescriptionImage;
                        quizQuestionObj.DisplayOrderForAnswer = questionObj.DisplayOrderForAnswer;
                        quizQuestionObj.DisplayOrderForNextButton = questionObj.DisplayOrderForNextButton;
                        quizQuestionObj.EnableComment = questionObj.EnableComment;
                        quizQuestionObj.TopicTitle = questionObj.TopicTitle;
                        quizQuestionObj.IsMultiRating = questionObj.IsMultiRating;
                        quizQuestionObj.AnswerStructureType = questionObj.AnswerStructureType.HasValue ? questionObj.AnswerStructureType.Value : (int)AnswerStructureTypeEnum.Default;

                        quizQuestionObj.QuizCorrectAnswerSetting = new QuizCorrectAnswerSetting();

                        quizQuestionObj.QuizCorrectAnswerSetting.CorrectAnswerExplanation = questionObj.CorrectAnswerDescription;
                        quizQuestionObj.QuizCorrectAnswerSetting.RevealCorrectAnswer = questionObj.RevealCorrectAnswer;
                        quizQuestionObj.QuizCorrectAnswerSetting.AliasTextForCorrect = questionObj.AliasTextForCorrect;
                        quizQuestionObj.QuizCorrectAnswerSetting.AliasTextForIncorrect = questionObj.AliasTextForIncorrect;
                        quizQuestionObj.QuizCorrectAnswerSetting.AliasTextForYourAnswer = questionObj.AliasTextForYourAnswer;
                        quizQuestionObj.QuizCorrectAnswerSetting.AliasTextForCorrectAnswer = questionObj.AliasTextForCorrectAnswer;
                        quizQuestionObj.QuizCorrectAnswerSetting.AliasTextForExplanation = questionObj.AliasTextForExplanation;
                        quizQuestionObj.QuizCorrectAnswerSetting.AliasTextForNextButton = questionObj.AliasTextForNextButton;

                        quizQuestionObj.AnswerList = new List<AnswerOptionInQuestion>();

                        var company = questionObj.QuizDetails.Quiz.Company;

                        var companyObj = new CompanyModel()
                        {
                            LeadDashboardApiUrl = company.LeadDashboardApiUrl,
                            CompanyName = company.CompanyName,
                            ClientCode = company.ClientCode,
                            LeadDashboardApiAuthorizationBearer = company.LeadDashboardApiAuthorizationBearer
                        };

                        var TagDetails = CommonStaticData.GetCachedTagsByCategory(String.Join(", ", questionObj.AnswerOptionsInQuizQuestions.SelectMany(a => a.TagsInAnswer.Select(b => b.TagCategoryId))), companyObj);

                        foreach (var answer in questionObj.AnswerOptionsInQuizQuestions.Where(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active && !r.IsUnansweredType).OrderBy(r => r.DisplayOrder)) {
                            var answerOptionInQuestion = new AnswerOptionInQuestion();

                            answerOptionInQuestion.AnswerId = answer.Id;
                            answerOptionInQuestion.AssociatedScore = answer.AssociatedScore;
                            answerOptionInQuestion.AnswerText = answer.Option;
                            answerOptionInQuestion.AnswerDescription = answer.Description;
                            answerOptionInQuestion.AnswerImage = answer.OptionImage;
                            answerOptionInQuestion.EnableMediaFile = answer.EnableMediaFile;
                            answerOptionInQuestion.PublicIdForAnswer = answer.PublicId;
                            answerOptionInQuestion.IsCorrectAnswer = questionObj.AnswerType == (int)AnswerTypeEnum.Multiple ? answer.IsCorrectForMultipleAnswer : (questionObj.AnswerType == (int)AnswerTypeEnum.Single) ? answer.IsCorrectAnswer : null;
                            answerOptionInQuestion.DisplayOrder = answer.DisplayOrder;
                            answerOptionInQuestion.IsReadOnly = answer.IsReadOnly;
                            answerOptionInQuestion.AutoPlay = false;
                            answerOptionInQuestion.AutoPlay = (questionObj.AnswerType == (int)AnswerTypeEnum.Single || questionObj.AnswerType == (int)AnswerTypeEnum.Multiple) ? true : false;
                            answerOptionInQuestion.SecondsToApply = answer.SecondsToApply;
                            answerOptionInQuestion.VideoFrameEnabled = answer.VideoFrameEnabled;
                            answerOptionInQuestion.OptionTextforRatingOne = answer.OptionTextforRatingOne;
                            answerOptionInQuestion.OptionTextforRatingTwo = answer.OptionTextforRatingTwo;
                            answerOptionInQuestion.OptionTextforRatingThree = answer.OptionTextforRatingThree;
                            answerOptionInQuestion.OptionTextforRatingFour = answer.OptionTextforRatingFour;
                            answerOptionInQuestion.OptionTextforRatingFive = answer.OptionTextforRatingFive;
                            answerOptionInQuestion.ListValues = answer.ListValues;
                            answerOptionInQuestion.Categories = new List<AnswerOptionInQuestion.CategoryModel>();

                            if (answer.TagsInAnswer != null) {
                                foreach (var tagDetail in TagDetails.Where(a => answer.TagsInAnswer.Select(b => b.TagCategoryId).Contains(a.tagCategoryId)).GroupBy(a => a.tagCategoryId).Select(x => x.FirstOrDefault())) {
                                    var categoryObj = new AnswerOptionInQuestion.CategoryModel();

                                    categoryObj.CategoryName = tagDetail.tagCategory;
                                    categoryObj.CategoryId = tagDetail.tagCategoryId;

                                    categoryObj.TagDetails = new List<AnswerOptionInQuestion.CategoryModel.TagDetail>();

                                    foreach (var tag in TagDetails.Where(a => a.tagCategoryId == tagDetail.tagCategoryId && answer.TagsInAnswer.Select(b => b.TagId).Contains(a.id))) {
                                        categoryObj.TagDetails.Add(new AnswerOptionInQuestion.CategoryModel.TagDetail {
                                            TagId = tag.id,
                                            TagName = tag.tagName
                                        });
                                    }

                                    answerOptionInQuestion.Categories.Add(categoryObj);
                                }
                            }
                            quizQuestionObj.AnswerList.Add(answerOptionInQuestion);

                            if (answer.ObjectFieldsInAnswer.Any()) {
                                foreach (var item in answer.ObjectFieldsInAnswer) {
                                    if (item.IsCommentMapped.HasValue && item.IsCommentMapped.Value == true) {
                                        answerOptionInQuestion.ObjectFieldsInAnswerComment = new AnswerOptionInQuestion.ObjectFieldsDetails();
                                        answerOptionInQuestion.ObjectFieldsInAnswerComment.ObjectName = item.ObjectName;
                                        answerOptionInQuestion.ObjectFieldsInAnswerComment.FieldName = item.FieldName;
                                        answerOptionInQuestion.ObjectFieldsInAnswerComment.Value = item.Value;
                                        answerOptionInQuestion.ObjectFieldsInAnswerComment.IsExternalSync = item.IsExternalSync;
                                        answerOptionInQuestion.ObjectFieldsInAnswerComment.IsCommentMapped = true;
                                    }
                                    else {
                                        answerOptionInQuestion.ObjectFieldsInAnswer = new AnswerOptionInQuestion.ObjectFieldsDetails();
                                        answerOptionInQuestion.ObjectFieldsInAnswer.ObjectName = item.ObjectName;
                                        answerOptionInQuestion.ObjectFieldsInAnswer.FieldName = item.FieldName;
                                        answerOptionInQuestion.ObjectFieldsInAnswer.Value = item.Value;
                                        answerOptionInQuestion.ObjectFieldsInAnswer.IsExternalSync = item.IsExternalSync;
                                        answerOptionInQuestion.ObjectFieldsInAnswer.IsCommentMapped = false;
                                    }

                                }
                            }
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Question not found for the QuestionId " + QuestionId;
                    }
                }

                if (quizQuestionObj != null)
                {
                    quizQuestionObj.MsgVariables = _quizVariablesService.GetQuizVariables(quizDetailId, quizQuestionObj.QuestionId, (int)QuizVariableObjectTypes.QUESTION);
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
            return quizQuestionObj;
        }

        public void UpdateQuizQuestionDetails(QuizQuestion QuizQuestionObj, int BusinessUserId, int CompanyId, bool isWhatsappEnable = false)
        {

            int quizDetailid = 0;
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    if (!string.IsNullOrEmpty(QuizQuestionObj.QuestionTitle) || (QuizQuestionObj.ShowTitle == false && string.IsNullOrEmpty(QuizQuestionObj.QuestionTitle) && isWhatsappEnable))
                    {
                        var questionObj = UOWObj.QuestionsInQuizRepository.GetByID(QuizQuestionObj.QuestionId);

                        if (questionObj != null && questionObj.QuizDetails.ParentQuizId == QuizQuestionObj.QuizId)
                        {
                            var currentDate = DateTime.UtcNow;
                            quizDetailid = questionObj.QuizDetails.Id;
                            var quizObj = questionObj.QuizDetails.Quiz;

                            if (questionObj.Type != QuizQuestionObj.Type)
                            {
                                if (!quizObj.QuesAndContentInSameTable)
                                {
                                    Status = ResultEnum.Error;
                                    ErrorMessage = "User can not switch the question type for Quiz " + quizObj.Id;
                                    return;
                                }

                                var CoordinatesInBranchingLogic = UOWObj.CoordinatesInBranchingLogicRepository.Get(r => r.ObjectId == questionObj.Id && r.ObjectTypeId == questionObj.Type);
                                if (QuizQuestionObj.Type == (int)BranchingLogicEnum.QUESTION)
                                {
                                    var branchingLogic = questionObj.QuizDetails.BranchingLogic;
                                    var itemforDeltion = branchingLogic.Where(r => r.SourceObjectId == questionObj.Id && r.SourceTypeId == (int)BranchingLogicEnum.CONTENTNEXT).FirstOrDefault();

                                    if (itemforDeltion != null)
                                    {
                                        UOWObj.BranchingLogicRepository.Delete(itemforDeltion);
                                    }

                                    var itemForupdate = branchingLogic.Where(r => (r.SourceObjectId == questionObj.Id && r.SourceTypeId == (int)BranchingLogicEnum.CONTENT)
                                    || (r.DestinationObjectId == questionObj.Id && r.DestinationTypeId == (int)BranchingLogicEnum.CONTENT));

                                    if (itemForupdate != null)
                                    {
                                        foreach (var item in itemForupdate.ToList())
                                        {
                                            if (item.SourceObjectId == questionObj.Id && item.SourceTypeId == (int)BranchingLogicEnum.CONTENT)
                                            {
                                                item.SourceTypeId = (int)BranchingLogicEnum.QUESTION;
                                                UOWObj.BranchingLogicRepository.Update(item);
                                            }
                                            else if (item.DestinationObjectId == questionObj.Id && item.DestinationTypeId == (int)BranchingLogicEnum.CONTENT)
                                            {
                                                item.DestinationTypeId = (int)BranchingLogicEnum.QUESTION;
                                                UOWObj.BranchingLogicRepository.Update(item);
                                            }
                                        }
                                    }
                                }

                                if (QuizQuestionObj.Type == (int)BranchingLogicEnum.CONTENT)
                                {
                                    var branchingLogic = questionObj.QuizDetails.BranchingLogic;
                                    var itemforDeltion = branchingLogic.FirstOrDefault(r => r.SourceObjectId == questionObj.Id && r.SourceTypeId == (int)BranchingLogicEnum.QUESTIONNEXT);

                                    if (itemforDeltion != null)
                                    {
                                        UOWObj.BranchingLogicRepository.Delete(itemforDeltion);
                                    }

                                    var ansIds = questionObj.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active).ToList().Select(t => t.Id);
                                    var branchingLogicList = branchingLogic.Where(r => ansIds.Any(s => s == r.SourceObjectId) && r.SourceTypeId == (int)BranchingLogicEnum.ANSWER).ToList();

                                    if (branchingLogicList != null)
                                    {
                                        foreach (var obj in branchingLogicList)
                                        {
                                            UOWObj.BranchingLogicRepository.Delete(obj);
                                        }
                                    }

                                    var itemForupdate = branchingLogic.Where(r => (r.SourceObjectId == questionObj.Id && r.SourceTypeId == (int)BranchingLogicEnum.QUESTION)
                                    || (r.DestinationObjectId == questionObj.Id && r.DestinationTypeId == (int)BranchingLogicEnum.QUESTION));

                                    if (itemForupdate != null)
                                    {
                                        foreach (var item in itemForupdate.ToList())
                                        {
                                            if (item.SourceObjectId == questionObj.Id && item.SourceTypeId == (int)BranchingLogicEnum.QUESTION)
                                            {
                                                item.SourceTypeId = (int)BranchingLogicEnum.CONTENT;
                                                UOWObj.BranchingLogicRepository.Update(item);
                                            }
                                            else if (item.DestinationObjectId == questionObj.Id && item.DestinationTypeId == (int)BranchingLogicEnum.QUESTION)
                                            {
                                                item.DestinationTypeId = (int)BranchingLogicEnum.CONTENT;
                                                UOWObj.BranchingLogicRepository.Update(item);
                                            }
                                        }
                                    }
                                }

                                if (CoordinatesInBranchingLogic.Any())
                                {
                                    foreach (var item in CoordinatesInBranchingLogic)
                                    {
                                        item.ObjectTypeId = QuizQuestionObj.Type;
                                        item.CompanyId = CompanyId;
                                        item.QuizId = QuizQuestionObj.QuizId;
                                        UOWObj.CoordinatesInBranchingLogicRepository.Update(item);
                                    }
                                }
                            }

                            SaveDynamicVariable(questionObj.Question, QuizQuestionObj.QuestionTitle, questionObj.QuizDetails.Id);
                            SaveDynamicVariable(questionObj.Description, QuizQuestionObj.Description, questionObj.QuizDetails.Id);

                            questionObj.ShowAnswerImage = QuizQuestionObj.ShowAnswerImage;
                            questionObj.Question = QuizQuestionObj.QuestionTitle;
                            questionObj.ShowTitle = QuizQuestionObj.ShowTitle;
                            questionObj.QuestionImage = QuizQuestionObj.QuestionImage;
                            questionObj.EnableMediaFile = QuizQuestionObj.EnableMediaFile;
                            questionObj.PublicId = QuizQuestionObj.PublicIdForQuestion;
                            questionObj.ShowQuestionImage = QuizQuestionObj.ShowQuestionImage;
                            questionObj.NextButtonColor = QuizQuestionObj.NextButtonColor;
                            questionObj.NextButtonText = QuizQuestionObj.NextButtonText;
                            questionObj.NextButtonTxtColor = QuizQuestionObj.NextButtonTxtColor;
                            questionObj.NextButtonTxtSize = QuizQuestionObj.NextButtonTxtSize;
                            questionObj.EnableNextButton = QuizQuestionObj.EnableNextButton;
                            questionObj.AutoPlay = QuizQuestionObj.AutoPlay;
                            questionObj.SecondsToApply = QuizQuestionObj.SecondsToApply;
                            questionObj.VideoFrameEnabled = QuizQuestionObj.VideoFrameEnabled;
                            questionObj.Description = QuizQuestionObj.Description;
                            questionObj.ShowDescription = QuizQuestionObj.ShowDescription;
                            questionObj.DescriptionImage = QuizQuestionObj.DescriptionImage;
                            questionObj.EnableMediaFileForDescription = QuizQuestionObj.EnableMediaFileForDescription;
                            questionObj.PublicIdForDescription = QuizQuestionObj.PublicIdForDescription;
                            questionObj.ShowDescriptionImage = QuizQuestionObj.ShowDescriptionImage;
                            questionObj.AutoPlayForDescription = QuizQuestionObj.AutoPlayForDescription;
                            questionObj.SecondsToApplyForDescription = QuizQuestionObj.SecondsToApplyForDescription;
                            questionObj.DescVideoFrameEnabled = QuizQuestionObj.DescVideoFrameEnabled;
                            questionObj.Type = QuizQuestionObj.Type;
                            questionObj.DisplayOrderForTitle = QuizQuestionObj.DisplayOrderForTitle;
                            questionObj.DisplayOrderForTitleImage = QuizQuestionObj.DisplayOrderForTitleImage;
                            questionObj.DisplayOrderForDescription = QuizQuestionObj.DisplayOrderForDescription;
                            questionObj.DisplayOrderForDescriptionImage = QuizQuestionObj.DisplayOrderForDescriptionImage;
                            questionObj.DisplayOrderForAnswer = QuizQuestionObj.DisplayOrderForAnswer;
                            questionObj.DisplayOrderForNextButton = QuizQuestionObj.DisplayOrderForNextButton;
                            questionObj.EnableComment = QuizQuestionObj.EnableComment;
                            questionObj.TopicTitle = QuizQuestionObj.TopicTitle;
                            //questionObj.IsMultiRating = QuizQuestionObj.IsMultiRating;
                            if (isWhatsappEnable)
                            {
                                questionObj.AnswerStructureType = QuizQuestionObj.AnswerStructureType.HasValue ? QuizQuestionObj.AnswerStructureType.Value : (int)AnswerStructureTypeEnum.Default;
                            }


                            foreach (var answer in QuizQuestionObj.AnswerList)
                            {
                                var answerObj = questionObj.AnswerOptionsInQuizQuestions.FirstOrDefault(r => r.Id == answer.AnswerId);

                                if (answerObj != null)
                                {
                                    SaveDynamicVariable(answerObj.Option, answer.AnswerText, questionObj.QuizDetails.Id);
                                    SaveDynamicVariable(answerObj.Description, answer.AnswerDescription, questionObj.QuizDetails.Id);

                                    answerObj.Option = answer.AnswerText;
                                    answerObj.Description = answer.AnswerDescription;
                                    answerObj.OptionImage = answer.AnswerImage;
                                    answerObj.EnableMediaFile = answer.EnableMediaFile;
                                    answerObj.PublicId = answer.PublicIdForAnswer;
                                    answerObj.AutoPlay = answer.AutoPlay;
                                    answerObj.SecondsToApply = answer.SecondsToApply;
                                    answerObj.VideoFrameEnabled = answer.VideoFrameEnabled;
                                    answerObj.ListValues = answer.ListValues != null && answer.ListValues.Any() ? string.Join(",", answer.ListValues) : null;
                                    answerObj.OptionTextforRatingOne = answer.OptionTextforRatingOne;
                                    answerObj.OptionTextforRatingTwo = answer.OptionTextforRatingTwo;
                                    answerObj.OptionTextforRatingThree = answer.OptionTextforRatingThree;
                                    answerObj.OptionTextforRatingFour = answer.OptionTextforRatingFour;
                                    answerObj.OptionTextforRatingFive = answer.OptionTextforRatingFive;
                                    answerObj.LastUpdatedBy = BusinessUserId;
                                    answerObj.LastUpdatedOn = currentDate;
                                    if (questionObj.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || questionObj.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                        answerObj.AssociatedScore = answer.AssociatedScore;
                                }
                            }

                            if (questionObj.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || questionObj.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate || questionObj.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Personality || questionObj.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.PersonalityTemplate)
                            {
                                questionObj.MinAnswer = QuizQuestionObj.MinAnswer;
                                questionObj.MaxAnswer = QuizQuestionObj.MaxAnswer;
                            }

                            questionObj.LastUpdatedBy = BusinessUserId;
                            questionObj.LastUpdatedOn = currentDate;

                            questionObj.QuizDetails.LastUpdatedBy = BusinessUserId;
                            questionObj.QuizDetails.LastUpdatedOn = currentDate;

                            questionObj.QuizDetails.Quiz.State = (int)QuizStateEnum.DRAFTED;

                            UOWObj.QuizRepository.Update(questionObj.QuizDetails.Quiz);

                            UOWObj.Save();


                        }
                        else
                        {
                            Status = ResultEnum.Error;
                            ErrorMessage = "Question not found for the QuestionId " + QuizQuestionObj.QuestionId;
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Question title is required";
                    }
                }

                if (QuizQuestionObj != null && QuizQuestionObj.MsgVariables != null && QuizQuestionObj.MsgVariables.Any())
                {
                    QuizVariableModel quizVariable = new QuizVariableModel();

                    quizVariable.Variables = String.Join(",", QuizQuestionObj.MsgVariables.ToList());
                    quizVariable.QuizDetailsId = quizDetailid;
                    quizVariable.ObjectId = QuizQuestionObj.QuestionId;
                    quizVariable.ObjectTypes = (int)QuizVariableObjectTypes.QUESTION;

                    _quizVariablesService.AddQuizVariables(quizVariable);
                }
                //else
                //{
                //    Status = ResultEnum.Error;
                //    ErrorMessage = "MsgVariables title is required";
                //}

            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public void UpdateQuizCorrectAnswerSetting(QuizCorrectAnswerSetting QuizCorrectAnswerSettingObj, int BusinessUserId, int CompanyId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var existingQuestion = UOWObj.QuestionsInQuizRepository.GetByID(QuizCorrectAnswerSettingObj.QuestionId);

                    if (existingQuestion != null)
                    {
                        var currentDate = DateTime.UtcNow;

                        foreach (var question in existingQuestion.QuizDetails.QuestionsInQuiz)
                        {
                            if (question.AnswerType == (int)AnswerTypeEnum.Single || question.AnswerType == (int)AnswerTypeEnum.Multiple)
                                question.RevealCorrectAnswer = QuizCorrectAnswerSettingObj.RevealCorrectAnswer;
                            question.AliasTextForCorrect = QuizCorrectAnswerSettingObj.AliasTextForCorrect;
                            question.AliasTextForIncorrect = QuizCorrectAnswerSettingObj.AliasTextForIncorrect;
                            question.AliasTextForYourAnswer = QuizCorrectAnswerSettingObj.AliasTextForYourAnswer;
                            question.AliasTextForCorrectAnswer = QuizCorrectAnswerSettingObj.AliasTextForCorrectAnswer;
                            question.AliasTextForExplanation = QuizCorrectAnswerSettingObj.AliasTextForExplanation;
                            question.AliasTextForNextButton = QuizCorrectAnswerSettingObj.AliasTextForNextButton;

                            if (question.Id == QuizCorrectAnswerSettingObj.QuestionId)
                            {
                                question.AnswerType = QuizCorrectAnswerSettingObj.AnswerType;
                                question.CorrectAnswerDescription = QuizCorrectAnswerSettingObj.CorrectAnswerExplanation;
                                foreach (var answerObj in question.AnswerOptionsInQuizQuestions)
                                {
                                    switch (question.QuizDetails.Quiz.QuizType)
                                    {
                                        case (int)QuizTypeEnum.Score:
                                        case (int)QuizTypeEnum.ScoreTemplate:
                                            answerObj.IsCorrectForMultipleAnswer = false;
                                            answerObj.IsCorrectAnswer = false;
                                            if (QuizCorrectAnswerSettingObj.AnswerScoreData != null)
                                            {
                                                var answerScoreData = QuizCorrectAnswerSettingObj.AnswerScoreData.FirstOrDefault(x => x.AnswerId == answerObj.Id);
                                                if (answerScoreData != null)
                                                    answerObj.AssociatedScore = answerScoreData.AssociatedScore;
                                                else
                                                    answerObj.AssociatedScore = default(int);
                                            }

                                            if (QuizCorrectAnswerSettingObj.AnswerType == (int)AnswerTypeEnum.Multiple)
                                            {
                                                question.MinAnswer = QuizCorrectAnswerSettingObj.MinAnswer;
                                                question.MaxAnswer = QuizCorrectAnswerSettingObj.MaxAnswer;
                                            }
                                            break;
                                        case (int)QuizTypeEnum.Personality:
                                        case (int)QuizTypeEnum.PersonalityTemplate:

                                            answerObj.IsCorrectForMultipleAnswer = false;
                                            answerObj.IsCorrectAnswer = false;
                                            if (QuizCorrectAnswerSettingObj.AnswerType == (int)AnswerTypeEnum.Multiple)
                                            {
                                                question.MinAnswer = QuizCorrectAnswerSettingObj.MinAnswer;
                                                question.MaxAnswer = QuizCorrectAnswerSettingObj.MaxAnswer;
                                            }
                                            break;
                                        default:

                                            if (QuizCorrectAnswerSettingObj.CorrectAnswerId.Contains(answerObj.Id))
                                            {
                                                if (QuizCorrectAnswerSettingObj.AnswerType == (int)AnswerTypeEnum.Multiple)
                                                    answerObj.IsCorrectForMultipleAnswer = true;
                                                else
                                                    answerObj.IsCorrectAnswer = true;
                                            }
                                            else
                                            {
                                                if (QuizCorrectAnswerSettingObj.AnswerType == (int)AnswerTypeEnum.Multiple)
                                                    answerObj.IsCorrectForMultipleAnswer = false;
                                                else
                                                    answerObj.IsCorrectAnswer = false;
                                            }
                                            break;

                                    }
                                    answerObj.LastUpdatedBy = BusinessUserId;
                                    answerObj.LastUpdatedOn = currentDate;
                                }
                            }

                            question.LastUpdatedBy = BusinessUserId;
                            question.LastUpdatedOn = currentDate;

                            existingQuestion.QuizDetails.LastUpdatedBy = BusinessUserId;
                            existingQuestion.QuizDetails.LastUpdatedOn = currentDate;
                            existingQuestion.QuizDetails.Quiz.State = (int)QuizStateEnum.DRAFTED;

                            UOWObj.QuizRepository.Update(existingQuestion.QuizDetails.Quiz);

                            UOWObj.Save();

                            
                            
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Question not found for the QuestionId " + QuizCorrectAnswerSettingObj.QuestionId;
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public AnswerOptionInQuestion AddAnswerOptionInQuestion(int QuestionId, int BusinessUserId, int CompanyId)
        {
            AnswerOptionInQuestion anserOptionInQuestionObj = null;
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    using (var transaction = Utility.CreateTransactionScope())
                    {
                        var existingQuestionInQuiz = UOWObj.QuestionsInQuizRepository.GetByID(QuestionId);

                        if (existingQuestionInQuiz != null)
                        {
                            var currentDate = DateTime.UtcNow;

                            if (existingQuestionInQuiz != null)
                            {
                                var existingAnsCount = existingQuestionInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active && !r.IsUnansweredType).Count();

                                var answerObj = new Db.AnswerOptionsInQuizQuestions();

                                answerObj.QuestionId = existingQuestionInQuiz.Id;
                                answerObj.QuizId = existingQuestionInQuiz.QuizId;
                                answerObj.Option = "Answer " + (existingAnsCount + 1).ToString();
                                answerObj.Description = "";
                                answerObj.OptionImage = string.Empty;
                                answerObj.PublicId = string.Empty;
                                answerObj.IsCorrectAnswer = false;
                                answerObj.IsCorrectForMultipleAnswer = false;
                                if (existingQuestionInQuiz.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || existingQuestionInQuiz.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                    answerObj.AssociatedScore = default(int);
                                answerObj.LastUpdatedBy = BusinessUserId;
                                answerObj.LastUpdatedOn = currentDate;
                                answerObj.DisplayOrder = existingAnsCount + 1;
                                answerObj.Status = (int)StatusEnum.Active;
                                answerObj.State = (int)QuizStateEnum.DRAFTED;
                                answerObj.AutoPlay = true;

                                UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);

                                existingQuestionInQuiz.LastUpdatedBy = BusinessUserId;
                                existingQuestionInQuiz.LastUpdatedOn = currentDate;

                                existingQuestionInQuiz.QuizDetails.LastUpdatedBy = BusinessUserId;
                                existingQuestionInQuiz.QuizDetails.LastUpdatedOn = currentDate;

                                existingQuestionInQuiz.QuizDetails.Quiz.State = (int)QuizStateEnum.DRAFTED;

                                UOWObj.QuizRepository.Update(existingQuestionInQuiz.QuizDetails.Quiz);

                                UOWObj.Save();
                                transaction.Complete();

                                anserOptionInQuestionObj = new AnswerOptionInQuestion();

                                anserOptionInQuestionObj.AnswerId = answerObj.Id;
                                anserOptionInQuestionObj.AnswerText = answerObj.Option;
                                anserOptionInQuestionObj.AnswerDescription = answerObj.Description;
                                anserOptionInQuestionObj.AnswerImage = answerObj.OptionImage;
                                anserOptionInQuestionObj.PublicIdForAnswer = answerObj.PublicId;

                                
                                
                            }
                        }
                        else
                        {
                            Status = ResultEnum.Error;
                            ErrorMessage = "Question not found for the QuestionId " + QuestionId;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
            return anserOptionInQuestionObj;
        }

        public void RemoveAnswer(int AnswerId, int BusinessUserId, int CompanyId, int? type, AutomationUnitOfWork UOWObj)
        {
            try
            {
                using (UOWObj == null ? UOWObj = new AutomationUnitOfWork() : null)
                {
                    var answerOption = UOWObj.AnswerOptionsInQuizQuestionsRepository.GetByID(AnswerId);

                    var currentDate = DateTime.UtcNow;

                    if (answerOption != null)
                    {
                        var activeAnswersInQuestion = answerOption.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && !r.IsUnansweredType);

                        if (type == null && activeAnswersInQuestion.Count() == 2 && (activeAnswersInQuestion.FirstOrDefault().QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single || activeAnswersInQuestion.FirstOrDefault().QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple))
                        {
                            Status = ResultEnum.Error;
                            ErrorMessage = "There must be minimum 2 active answers in a question.";
                        }
                        else if (type == null && answerOption.IsReadOnly)
                        {
                            Status = ResultEnum.Error;
                            ErrorMessage = "User can not delete this answers.";
                        }
                        else
                        {
                            SaveDynamicVariable(answerOption.Option, string.Empty, answerOption.QuestionsInQuiz.QuizDetails.Id);

                            //if current answer is correct then set first answer as correct
                            if (type == null && answerOption.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single && answerOption.IsCorrectAnswer.HasValue && answerOption.IsCorrectAnswer.Value)
                            {
                                var firstAns = activeAnswersInQuestion.OrderBy(r => r.DisplayOrder).FirstOrDefault(r => r.Id != AnswerId);
                                firstAns.IsCorrectAnswer = true;
                            }

                            if ((answerOption.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple || answerOption.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.DrivingLicense)
                                && (answerOption.QuestionsInQuiz.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score
                                || answerOption.QuestionsInQuiz.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate
                                || answerOption.QuestionsInQuiz.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Personality
                                || answerOption.QuestionsInQuiz.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.PersonalityTemplate)
                                && answerOption.QuestionsInQuiz.MaxAnswer > answerOption.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Count(r => r.Status == (int)StatusEnum.Active && !r.IsUnansweredType) - 1)
                                answerOption.QuestionsInQuiz.MaxAnswer = answerOption.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Count(r => r.Status == (int)StatusEnum.Active && !r.IsUnansweredType) - 1;

                            answerOption.Status = (int)StatusEnum.Deleted;
                            answerOption.LastUpdatedBy = BusinessUserId;
                            answerOption.LastUpdatedOn = currentDate;

                            //Remove Personality Quiz Answer Result Mapping 
                            if (answerOption.PersonalityAnswerResultMapping.Any())
                            {
                                var MappingList = answerOption.PersonalityAnswerResultMapping.ToList();
                                foreach (var data in MappingList)
                                {
                                    UOWObj.PersonalityAnswerResultMappingRepository.Delete(data.Id);
                                }
                            }

                            answerOption.QuestionsInQuiz.QuizDetails.LastUpdatedBy = BusinessUserId;
                            answerOption.QuestionsInQuiz.QuizDetails.LastUpdatedOn = currentDate;

                            answerOption.QuestionsInQuiz.QuizDetails.Quiz.State = (int)QuizStateEnum.DRAFTED;

                            //remove if answer exists in branching logic
                            var branchingLogic = UOWObj.BranchingLogicRepository.Get(r => r.SourceObjectId == AnswerId && r.SourceTypeId == (int)BranchingLogicEnum.ANSWER).FirstOrDefault();

                            if (branchingLogic != null)
                            {
                                UOWObj.BranchingLogicRepository.Delete(branchingLogic);
                            }

                            UOWObj.QuizRepository.Update(answerOption.QuestionsInQuiz.QuizDetails.Quiz);
                            UOWObj.Save();
                            
                            
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Answer not found for the AnswerId " + AnswerId;
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public void RemoveQuestion(int QuestionId, int BusinessUserId, int CompanyId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var question = UOWObj.QuestionsInQuizRepository.GetByID(QuestionId);

                    //var branchingLogic = UOWObj.BranchingLogicRepository.Get();

                    var currentDate = DateTime.UtcNow;

                    if (question != null)
                    {
                        IEnumerable<Db.BranchingLogic> branchingLogicQues = null;

                        if (question.QuizDetails.Quiz.QuesAndContentInSameTable)
                        {
                            //branchingLogicQues = branchingLogic.Where(r => (r.SourceObjectId == QuestionId && (r.SourceTypeId == (int)BranchingLogicEnum.QUESTION || r.SourceTypeId == (int)BranchingLogicEnum.QUESTIONNEXT || r.SourceTypeId == (int)BranchingLogicEnum.CONTENT || r.SourceTypeId == (int)BranchingLogicEnum.CONTENTNEXT))
                            //|| (r.DestinationObjectId == QuestionId && (r.DestinationTypeId == (int)BranchingLogicEnum.CONTENT || r.DestinationTypeId == (int)BranchingLogicEnum.QUESTION)));


                            branchingLogicQues = UOWObj.BranchingLogicRepository.Get(r => (r.SourceObjectId == QuestionId && (r.SourceTypeId == (int)BranchingLogicEnum.QUESTION || r.SourceTypeId == (int)BranchingLogicEnum.QUESTIONNEXT || r.SourceTypeId == (int)BranchingLogicEnum.CONTENT || r.SourceTypeId == (int)BranchingLogicEnum.CONTENTNEXT))
                            || (r.DestinationObjectId == QuestionId && (r.DestinationTypeId == (int)BranchingLogicEnum.CONTENT || r.DestinationTypeId == (int)BranchingLogicEnum.QUESTION)));
                        }
                        else
                        {
                            //branchingLogicQues = branchingLogic.Where(r => (r.SourceObjectId == QuestionId && (r.SourceTypeId == (int)BranchingLogicEnum.QUESTION || r.SourceTypeId == (int)BranchingLogicEnum.QUESTIONNEXT))
                            //|| (r.DestinationObjectId == QuestionId && r.DestinationTypeId == (int)BranchingLogicEnum.QUESTION));


                            branchingLogicQues = UOWObj.BranchingLogicRepository.Get(r => (r.SourceObjectId == QuestionId && (r.SourceTypeId == (int)BranchingLogicEnum.QUESTION || r.SourceTypeId == (int)BranchingLogicEnum.QUESTIONNEXT))
                           || (r.DestinationObjectId == QuestionId && r.DestinationTypeId == (int)BranchingLogicEnum.QUESTION));
                        }

                        if (branchingLogicQues.Any())
                        {
                            foreach (var obj in branchingLogicQues)
                            {
                                UOWObj.BranchingLogicRepository.Delete(obj);
                            }
                        }

                        question.Status = (int)StatusEnum.Deleted;
                        question.LastUpdatedBy = BusinessUserId;
                        question.LastUpdatedOn = DateTime.UtcNow;

                        SaveDynamicVariable(question.Question, string.Empty, question.QuizDetails.Id);

                        foreach (var answer in question.AnswerOptionsInQuizQuestions)
                        {
                            SaveDynamicVariable(answer.Option, string.Empty, question.QuizDetails.Id);

                            answer.Status = (int)StatusEnum.Deleted;
                            answer.LastUpdatedBy = BusinessUserId;
                            answer.LastUpdatedOn = currentDate;

                            //var branchingLogicQuesAns = branchingLogic.Where(r => r.SourceObjectId == answer.Id && r.SourceTypeId == (int)BranchingLogicEnum.ANSWER).FirstOrDefault();

                            var branchingLogicQuesAns = UOWObj.BranchingLogicRepository.Get(r => r.SourceObjectId == answer.Id && r.SourceTypeId == (int)BranchingLogicEnum.ANSWER).FirstOrDefault();

                            if (branchingLogicQuesAns != null)
                            {
                                UOWObj.BranchingLogicRepository.Delete(branchingLogicQuesAns);
                            }
                        }

                        question.QuizDetails.LastUpdatedBy = BusinessUserId;
                        question.QuizDetails.LastUpdatedOn = currentDate;
                        question.QuizDetails.Quiz.State = (int)QuizStateEnum.DRAFTED;

                        UOWObj.QuizRepository.Update(question.QuizDetails.Quiz);
                        UOWObj.Save();
                        
                        
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Question not found for the QuestionId " + QuestionId;
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public void ReorderQuestionAnswerAndContent(bool IsQuesAndContentInSameTable, List<QuizQuestion> ReorderedList, int BusinessUserId, int CompanyId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var currentDate = DateTime.UtcNow;

                    for (int i = 0; i < ReorderedList.Count(); i++)
                    {
                        var questionObj = ReorderedList[i];

                        if (questionObj.Type == (int)BranchingLogicEnum.QUESTION || (IsQuesAndContentInSameTable && questionObj.Type == (int)BranchingLogicEnum.CONTENT))
                        {
                            var question = UOWObj.QuestionsInQuizRepository.GetByID(questionObj.QuestionId);

                            if (question != null)
                            {
                                question.DisplayOrder = i + 1;
                                question.LastUpdatedBy = BusinessUserId;
                                question.LastUpdatedOn = currentDate;

                                for (int j = 0; j < questionObj.AnswerList.Count(); j++)
                                {
                                    var answerObj = questionObj.AnswerList[j];
                                    var answer = question.AnswerOptionsInQuizQuestions.FirstOrDefault(r => r.Id == answerObj.AnswerId);

                                    if (answer != null)
                                    {
                                        answer.DisplayOrder = j + 1;
                                        answer.LastUpdatedBy = BusinessUserId;
                                        answer.LastUpdatedOn = currentDate;
                                    }
                                }

                                question.QuizDetails.LastUpdatedBy = BusinessUserId;
                                question.QuizDetails.LastUpdatedOn = currentDate;

                                question.QuizDetails.Quiz.State = (int)QuizStateEnum.DRAFTED;

                                UOWObj.QuizRepository.Update(question.QuizDetails.Quiz);
                            }
                        }
                        else if (questionObj.Type == (int)BranchingLogicEnum.CONTENT)
                        {
                            var content = UOWObj.ContentsInQuizRepository.GetByID(questionObj.QuestionId);

                            if (content != null)
                            {
                                content.DisplayOrder = i + 1;
                                content.LastUpdatedBy = BusinessUserId;
                                content.LastUpdatedOn = currentDate;

                                content.QuizDetails.LastUpdatedBy = BusinessUserId;
                                content.QuizDetails.LastUpdatedOn = currentDate;

                                content.QuizDetails.Quiz.State = (int)QuizStateEnum.DRAFTED;

                                UOWObj.QuizRepository.Update(content.QuizDetails.Quiz);
                            }
                        }
                    }

                    UOWObj.Save();

                    
                    
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public void ReorderAnswer(QuizQuestion questionObj, int BusinessUserId, int CompanyId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var currentDate = DateTime.UtcNow;

                    var question = UOWObj.QuestionsInQuizRepository.GetByID(questionObj.QuestionId);

                    if (question != null && question.Type == (int)BranchingLogicEnum.QUESTION)
                    {
                        question.LastUpdatedBy = BusinessUserId;
                        question.LastUpdatedOn = currentDate;

                        for (int j = 0; j < questionObj.AnswerList.Count(); j++)
                        {
                            var answerObj = questionObj.AnswerList[j];
                            var answer = question.AnswerOptionsInQuizQuestions.FirstOrDefault(r => r.Id == answerObj.AnswerId);

                            if (answer != null)
                            {
                                answer.DisplayOrder = j + 1;
                                answer.LastUpdatedBy = BusinessUserId;
                                answer.LastUpdatedOn = currentDate;

                                UOWObj.AnswerOptionsInQuizQuestionsRepository.Update(answer);
                            }
                        }

                        question.QuizDetails.LastUpdatedBy = BusinessUserId;
                        question.QuizDetails.LastUpdatedOn = currentDate;

                        question.QuizDetails.Quiz.State = (int)QuizStateEnum.DRAFTED;

                        UOWObj.QuizRepository.Update(question.QuizDetails.Quiz);
                        UOWObj.Save();
                    }


                    
                    
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public void UpdateResultSetting(QuizResultSetting QuizResultSettingObj, int BusinessUserId, int CompanyId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizObj = UOWObj.QuizRepository.GetByID(QuizResultSettingObj.QuizId);

                    if (quizObj != null)
                    {
                        var quizDetails = quizObj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED);

                        if (quizDetails != null)
                        {
                            var currentDate = DateTime.UtcNow;

                            var quizResult = quizDetails.QuizResults.FirstOrDefault(r => r.Id == QuizResultSettingObj.ResultId && r.Status == (int)StatusEnum.Active);

                            var resultSetting = quizDetails.ResultSettings.FirstOrDefault();

                            if (resultSetting != null)
                            {
                                resultSetting.ShowScoreValue = QuizResultSettingObj.ShowScoreValue;
                                resultSetting.ShowCorrectAnswer = QuizResultSettingObj.ShowCorrectAnswer;
                                resultSetting.CustomTxtForScoreValueInResult = QuizResultSettingObj.CustomTxtForScoreValueInResult;
                                resultSetting.CustomTxtForAnswerKey = QuizResultSettingObj.CustomTxtForAnswerKey;
                                resultSetting.CustomTxtForYourAnswer = QuizResultSettingObj.CustomTxtForYourAnswer;
                                resultSetting.CustomTxtForCorrectAnswer = QuizResultSettingObj.CustomTxtForCorrectAnswer;
                                resultSetting.CustomTxtForExplanation = QuizResultSettingObj.CustomTxtForExplanation;
                                resultSetting.LastUpdatedBy = BusinessUserId;
                                resultSetting.LastUpdatedOn = currentDate;

                                if (quizResult != null)
                                {
                                    //   quizResult.ShowLeadUserForm = QuizResultSettingObj.ShowLeadUserForm;
                                    quizResult.AutoPlay = QuizResultSettingObj.AutoPlay;
                                }

                                quizDetails.LastUpdatedBy = BusinessUserId;
                                quizDetails.LastUpdatedOn = currentDate;

                                quizObj.State = (int)QuizStateEnum.DRAFTED;

                                UOWObj.QuizRepository.Update(quizObj);

                                UOWObj.Save();

                                
                                
                            }
                            else
                            {
                                Status = ResultEnum.Error;
                                ErrorMessage = "Result Setting not found for the QuizId " + QuizResultSettingObj.QuizId;
                            }
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz not found for the QuizId " + QuizResultSettingObj.QuizId;
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        private void DeleteQuizUnusedVariables(int QuizDetailsId)
        {
            using (var UOWObj = new AutomationUnitOfWork())
            {
                var VariableInQuizObj = UOWObj.VariableInQuizRepository.Get().Where(t => t.QuizId == QuizDetailsId && t.NumberOfUses < 1);
                foreach (var item in VariableInQuizObj)
                {
                    UOWObj.VariableInQuizRepository.Delete(item);

                }

                UOWObj.Save();
            }
        }

        public void UpdateQuizResult(QuizResult QuizResultObj, int BusinessUserId, int CompanyId)
        {
            try
            {
                int quizDetailsId = 0;
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    if (!string.IsNullOrEmpty(QuizResultObj.Title))
                    {
                        var quizResult = UOWObj.QuizResultsRepository.GetByID(QuizResultObj.ResultId);

                        if (quizResult != null && quizResult.QuizDetails.ParentQuizId == QuizResultObj.QuizId)
                        {
                            var currentDate = DateTime.UtcNow;
                            SaveDynamicVariable(quizResult.Title, QuizResultObj.Title, quizResult.QuizDetails.Id);
                            SaveDynamicVariable(quizResult.InternalTitle, QuizResultObj.InternalTitle, quizResult.QuizDetails.Id);
                            SaveDynamicVariable(quizResult.Description, QuizResultObj.Description, quizResult.QuizDetails.Id);
                            DeleteQuizUnusedVariables(quizResult.QuizDetails.Id);
                            quizDetailsId = quizResult.QuizDetails.Id;
                            quizResult.Title = StringExtension.DecodeHtml(QuizResultObj.Title);
                            quizResult.InternalTitle = QuizResultObj.InternalTitle;
                            quizResult.Image = QuizResultObj.Image;
                            quizResult.EnableMediaFile = QuizResultObj.EnableMediaFile;
                            quizResult.PublicId = QuizResultObj.PublicIdForResult;
                            quizResult.ShowResultImage = QuizResultObj.ShowResultImage;
                            quizResult.Description = QuizResultObj.Description;
                            quizResult.HideCallToAction = !QuizResultObj.EnableCallToActionButton;
                            quizResult.ActionButtonURL = QuizResultObj.ActionButtonURL;
                            quizResult.ActionButtonTxtSize = QuizResultObj.ActionButtonTxtSize;
                            quizResult.ActionButtonColor = QuizResultObj.ActionButtonColor;
                            quizResult.ActionButtonTitleColor = QuizResultObj.ActionButtonTitleColor;
                            quizResult.ActionButtonText = QuizResultObj.ActionButtonText;
                            quizResult.AutoPlay = QuizResultObj.AutoPlay;
                            quizResult.SecondsToApply = QuizResultObj.SecondsToApply;
                            quizResult.VideoFrameEnabled = QuizResultObj.VideoFrameEnabled;
                            quizResult.ShowExternalTitle = QuizResultObj.ShowExternalTitle;
                            quizResult.ShowInternalTitle = QuizResultObj.ShowInternalTitle;
                            quizResult.ShowDescription = QuizResultObj.ShowDescription;
                            quizResult.DisplayOrderForTitle = QuizResultObj.DisplayOrderForTitle;
                            quizResult.DisplayOrderForTitleImage = QuizResultObj.DisplayOrderForTitleImage;
                            quizResult.DisplayOrderForDescription = QuizResultObj.DisplayOrderForDescription;
                            quizResult.DisplayOrderForNextButton = QuizResultObj.DisplayOrderForNextButton;
                            quizResult.LastUpdatedBy = BusinessUserId;
                            quizResult.LastUpdatedOn = currentDate;

                            quizResult.QuizDetails.LastUpdatedBy = BusinessUserId;
                            quizResult.QuizDetails.LastUpdatedOn = currentDate;

                            quizResult.QuizDetails.Quiz.State = (int)QuizStateEnum.DRAFTED;

                            UOWObj.QuizRepository.Update(quizResult.QuizDetails.Quiz);

                            UOWObj.Save();

                            
                            
                            AppLocalCache.Remove("QuizDetails_QuizId_" + quizResult.QuizDetails.ParentQuizId);
                        }
                        else
                        {
                            Status = ResultEnum.Error;
                            ErrorMessage = "Result not found for the ResultId " + QuizResultObj.ResultId;
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Title is required";
                    }
                }

                if (QuizResultObj != null && QuizResultObj.MsgVariables != null && QuizResultObj.MsgVariables.Any())
                {
                    QuizVariableModel quizVariable = new QuizVariableModel();

                    quizVariable.Variables = String.Join(",", QuizResultObj.MsgVariables.ToList());
                    quizVariable.QuizDetailsId = quizDetailsId;
                    quizVariable.ObjectId = QuizResultObj.ResultId;
                    quizVariable.ObjectTypes = (int)QuizVariableObjectTypes.RESULT;

                    _quizVariablesService.AddQuizVariables(quizVariable);
                }

            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public void RemoveQuizResult(int ResultId, int BusinessUserId, int CompanyId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizResult = UOWObj.QuizResultsRepository.GetByID(ResultId);

                    if (quizResult != null)
                    {
                        SaveDynamicVariable(quizResult.Title, string.Empty, quizResult.QuizDetails.Id);
                        SaveDynamicVariable(quizResult.Description, string.Empty, quizResult.QuizDetails.Id);

                        var currentDate = DateTime.UtcNow;

                        quizResult.Status = (int)StatusEnum.Deleted;
                        quizResult.LastUpdatedBy = BusinessUserId;
                        quizResult.LastUpdatedOn = currentDate;

                        //remove if answer exists in branching logic
                        var branchingLogic = UOWObj.BranchingLogicRepository.Get(r => (r.SourceObjectId == ResultId && r.SourceTypeId == (int)BranchingLogicEnum.RESULTNEXT) || (r.DestinationObjectId == ResultId && r.DestinationTypeId == (int)BranchingLogicEnum.RESULT));

                        if (branchingLogic.Any())
                        {
                            foreach (var obj in branchingLogic)
                            {
                                UOWObj.BranchingLogicRepository.Delete(obj);
                            }
                        }

                        //Remove Personality Quiz Answer Result Mapping 
                        if (quizResult.PersonalityAnswerResultMapping.Any())
                        {
                            var MappingList = quizResult.PersonalityAnswerResultMapping.ToList();
                            foreach (var data in MappingList)
                            {
                                UOWObj.PersonalityAnswerResultMappingRepository.Delete(data.Id);
                            }
                        }

                        quizResult.QuizDetails.LastUpdatedBy = BusinessUserId;
                        quizResult.QuizDetails.LastUpdatedOn = currentDate;

                        quizResult.QuizDetails.Quiz.State = (int)QuizStateEnum.DRAFTED;

                        UOWObj.QuizRepository.Update(quizResult.QuizDetails.Quiz);

                        UOWObj.Save();
                        
                        
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Result not found in any quiz for the ResultId " + ResultId.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public QuizResult GetQuizResultDetails(int ResultId, int QuizId)
        {
            QuizResult quizResult = null;
            int quizDetailId = 0;
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var resultObj = UOWObj.QuizResultsRepository.GetByID(ResultId);

                    if (resultObj != null && resultObj.QuizDetails.ParentQuizId == QuizId)
                    {
                        #region Bind return obj

                        quizResult = new QuizResult();
                        quizDetailId = resultObj.QuizDetails.Id;
                        quizResult.ResultId = resultObj.Id;
                        quizResult.Title = resultObj.Title;
                        quizResult.InternalTitle = resultObj.InternalTitle ?? string.Empty;
                        quizResult.ShowResultImage = resultObj.ShowResultImage;
                        quizResult.Image = resultObj.Image;
                        quizResult.EnableMediaFile = resultObj.EnableMediaFile;
                        quizResult.PublicIdForResult = resultObj.PublicId;
                        quizResult.Description = resultObj.Description;
                        quizResult.HideCallToAction = resultObj.HideCallToAction;
                        quizResult.EnableCallToActionButton = !(resultObj.HideCallToAction ?? false);
                        quizResult.ActionButtonURL = resultObj.ActionButtonURL;
                        quizResult.OpenLinkInNewTab = resultObj.OpenLinkInNewTab;
                        quizResult.ActionButtonTxtSize = resultObj.ActionButtonTxtSize;
                        quizResult.ActionButtonColor = resultObj.ActionButtonColor;
                        quizResult.ActionButtonTitleColor = resultObj.ActionButtonTitleColor;
                        quizResult.ActionButtonText = resultObj.ActionButtonText;
                        quizResult.MinScore = resultObj.MinScore;
                        quizResult.MaxScore = resultObj.MaxScore;
                        // quizResult.ShowLeadUserForm = resultObj.ShowLeadUserForm;
                        quizResult.AutoPlay = resultObj.AutoPlay;
                        quizResult.VideoFrameEnabled = resultObj.VideoFrameEnabled;
                        quizResult.ShowExternalTitle = resultObj.ShowExternalTitle;
                        quizResult.ShowInternalTitle = resultObj.ShowInternalTitle;
                        quizResult.ShowDescription = resultObj.ShowDescription;
                        quizResult.DisplayOrderForTitle = resultObj.DisplayOrderForTitle;
                        quizResult.DisplayOrderForTitleImage = resultObj.DisplayOrderForTitleImage;
                        quizResult.DisplayOrderForDescription = resultObj.DisplayOrderForDescription;
                        quizResult.DisplayOrderForNextButton = resultObj.DisplayOrderForNextButton;
                        quizResult.SecondsToApply = resultObj.SecondsToApply;

                        quizResult.ResultSetting = new QuizResultSetting();

                        var resultSetting = resultObj.QuizDetails.ResultSettings.FirstOrDefault();

                        if (resultSetting != null)
                        {
                            quizResult.ResultSetting.QuizId = resultSetting.QuizDetails.ParentQuizId;
                            quizResult.ResultSetting.ShowScoreValue = resultSetting.ShowScoreValue;
                            quizResult.ResultSetting.ShowCorrectAnswer = resultSetting.ShowCorrectAnswer;
                            quizResult.ResultSetting.CustomTxtForScoreValueInResult = resultSetting.CustomTxtForScoreValueInResult;
                            quizResult.ResultSetting.CustomTxtForAnswerKey = resultSetting.CustomTxtForAnswerKey;
                            quizResult.ResultSetting.CustomTxtForYourAnswer = resultSetting.CustomTxtForYourAnswer;
                            quizResult.ResultSetting.CustomTxtForCorrectAnswer = resultSetting.CustomTxtForCorrectAnswer;
                            quizResult.ResultSetting.CustomTxtForExplanation = resultSetting.CustomTxtForExplanation;
                            // quizResult.ResultSetting.ShowLeadUserForm = resultObj.ShowLeadUserForm;
                            quizResult.ResultSetting.AutoPlay = resultObj.AutoPlay;
                        }

                        #endregion
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Result not found for the ResultId " + ResultId.ToString();
                    }
                }

                if (quizResult != null)
                {
                    quizResult.MsgVariables = _quizVariablesService.GetQuizVariables(quizDetailId, quizResult.ResultId, (int)QuizVariableObjectTypes.RESULT);
                }

            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
            return quizResult;
        }

        public void UpdateQuizBranchingLogicDetails(QuizBranchingLogicLinksList QuizBranchingLogicObj, int BusinessUserId, int CompanyId)
        {
            try
            {
                bool IsCyclic = false;

                #region check if branching logic is cyclic

                foreach (var links in QuizBranchingLogicObj.QuizBranchingLogicLinks)
                {
                    foreach (var link in links.Links.Where(a => !string.IsNullOrEmpty(a.FromId)))
                    {
                        if (!CheckIfValid((int)link.FromType))
                        {
                            var FromType = link.FromType;
                            var FromId = link.FromId;
                            link.FromId = link.ToId;
                            link.FromType = link.ToType;
                            link.ToType = FromType;
                            link.ToId = FromId;
                        }

                        link.LinkedTypeObj = QuizBranchingLogicObj.QuizBranchingLogicLinks.Select(a => a.Links.FirstOrDefault(w => w.FromId == link.ToId && w.FromType == link.ToType)).Where(a => a != null).FirstOrDefault();
                    }
                }
                foreach (var links in QuizBranchingLogicObj.QuizBranchingLogicLinks)
                {
                    if (IsCyclic) break;
                    foreach (var link in links.Links.Where(a => !string.IsNullOrEmpty(a.FromId)))
                    {
                        IsCyclic = CheckIfBranchLoopExists(link, link.FromId, (int)link.FromType);
                    }
                }

                #endregion

                #region if not cyclic then update branching logic


                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizObj = UOWObj.QuizRepository.GetByID(QuizBranchingLogicObj.QuizId);

                    if (quizObj != null)
                    {
                        var quizDetailsObj = quizObj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED);

                        if (quizDetailsObj != null)
                        {
                            quizDetailsObj.Quiz.State = (int)QuizStateEnum.DRAFTED;

                            UOWObj.QuizRepository.Update(quizDetailsObj.Quiz);

                            UOWObj.Save();
                        }
                        if (!IsCyclic)
                        {
                            var BranchingLogicObj = UOWObj.BranchingLogicRepository.Get(a => a.QuizId == quizDetailsObj.Id);
                            foreach (var BranchingLogic in BranchingLogicObj)
                            {
                                UOWObj.BranchingLogicRepository.Delete(BranchingLogic);
                            }
                            foreach (var QuizBranchingLogicLink in QuizBranchingLogicObj.QuizBranchingLogicLinks)
                            {
                                if (QuizBranchingLogicLink.ObjectType != BranchingLogicEnum.START && QuizBranchingLogicLink.ObjectType != BranchingLogicEnum.NONE)
                                {
                                    var ParsedObjectTypeId = int.Parse(QuizBranchingLogicLink.ObjectTypeId);
                                    switch (QuizBranchingLogicLink.ObjectType)
                                    {
                                        case BranchingLogicEnum.QUESTION:
                                            if (quizObj.QuizDetails.FirstOrDefault(a => a.QuestionsInQuiz.Any(v => v.Id == ParsedObjectTypeId)) == null)
                                            {
                                                Status = ResultEnum.Error;
                                                ErrorMessage = "Question not found for the QuestionId " + ParsedObjectTypeId;
                                            }
                                            break;
                                        case BranchingLogicEnum.CONTENT:
                                            if (quizObj.QuesAndContentInSameTable)
                                            {
                                                if (quizObj.QuizDetails.FirstOrDefault(a => a.QuestionsInQuiz.Any(v => v.Id == ParsedObjectTypeId)) == null)
                                                {
                                                    Status = ResultEnum.Error;
                                                    ErrorMessage = "Question not found for the QuestionId " + ParsedObjectTypeId;
                                                }
                                            }
                                            else
                                            {
                                                if (quizObj.QuizDetails.FirstOrDefault(a => a.ContentsInQuiz.Any(v => v.Id == ParsedObjectTypeId)) == null)
                                                {
                                                    Status = ResultEnum.Error;
                                                    ErrorMessage = "Content not found for the ContentId " + ParsedObjectTypeId;
                                                }
                                            }
                                            break;
                                        case BranchingLogicEnum.RESULT:
                                            if (quizObj.QuizDetails.FirstOrDefault(a => a.QuizResults.Any(v => v.Id == ParsedObjectTypeId)) == null)
                                            {
                                                Status = ResultEnum.Error;
                                                ErrorMessage = "Result not found for the ResultId " + ParsedObjectTypeId;
                                            }
                                            break;
                                        case BranchingLogicEnum.ACTION:
                                            if (quizObj.QuizDetails.FirstOrDefault(a => a.ActionsInQuiz.Any(v => v.Id == ParsedObjectTypeId)) == null)
                                            {
                                                Status = ResultEnum.Error;
                                                ErrorMessage = "Action not found for the ActionId " + ParsedObjectTypeId;
                                            }
                                            break;
                                        case BranchingLogicEnum.WHATSAPPTEMPLATE:
                                            if (quizObj.QuizDetails.FirstOrDefault(a => a.QuestionsInQuiz.Any(v => v.Id == ParsedObjectTypeId)) == null)
                                            {
                                                Status = ResultEnum.Error;
                                                ErrorMessage = "Template not found for the QuestionId " + ParsedObjectTypeId;
                                            }
                                            break;
                                        case BranchingLogicEnum.WHATSAPPTEMPLATEACTION:
                                            if (quizObj.QuizDetails.FirstOrDefault(a => a.QuestionsInQuiz.Any(v => v.Id == ParsedObjectTypeId)) == null)
                                            {
                                                Status = ResultEnum.Error;
                                                ErrorMessage = "Template not found for the QuestionId " + ParsedObjectTypeId;
                                            }
                                            break;


                                    }
                                }
                                foreach (var link in QuizBranchingLogicLink.Links.Where(a => !string.IsNullOrEmpty(a.FromId)))
                                {

                                    if (link.FromType != BranchingLogicEnum.START)
                                    {

                                        var branchingLogic = new Db.BranchingLogic
                                        {
                                            QuizId = quizDetailsObj.Id,
                                            SourceObjectId = int.Parse(link.FromId),
                                            SourceTypeId = (int)link.FromType,
                                            DestinationObjectId = int.Parse(link.ToId),
                                            DestinationTypeId = (int)link.ToType,
                                            IsStartingPoint = QuizBranchingLogicObj.QuizBranchingLogicLinks.Any(a => a.Links.Any(r => r.FromType == BranchingLogicEnum.START && r.ToId == link.FromId && r.ToType == link.FromType))
                                        };
                                        UOWObj.BranchingLogicRepository.Insert(branchingLogic);
                                    }
                                    else
                                    {
                                        if (!QuizBranchingLogicObj.QuizBranchingLogicLinks.Any(a => a.Links.Any(r => r.FromType == link.ToType && r.FromId == link.ToId)) && !string.IsNullOrEmpty(link.ToId))
                                        {
                                            var branchingLogic = new Db.BranchingLogic
                                            {
                                                QuizId = quizDetailsObj.Id,
                                                SourceObjectId = int.Parse(link.ToId),
                                                SourceTypeId = (int)link.ToType,
                                                IsStartingPoint = true
                                            };
                                            UOWObj.BranchingLogicRepository.Insert(branchingLogic);
                                        }
                                    }

                                }
                                if (QuizBranchingLogicLink.ObjectType != BranchingLogicEnum.START)
                                {
                                    var parsedObjectId = int.Parse(QuizBranchingLogicLink.ObjectTypeId);
                                    var cordinatesInBranchingLogicObj = UOWObj.CoordinatesInBranchingLogicRepository.Get(a => a.ObjectId == parsedObjectId && a.ObjectTypeId == (int)QuizBranchingLogicLink.ObjectType).FirstOrDefault();
                                    if (cordinatesInBranchingLogicObj == null)
                                    {
                                        var cordinatesInBranchingLogic = new Db.CoordinatesInBranchingLogic
                                        {
                                            ObjectId = parsedObjectId,
                                            ObjectTypeId = (int)QuizBranchingLogicLink.ObjectType,
                                            XCoordinate = QuizBranchingLogicLink.Position.Count() > 1 ? QuizBranchingLogicLink.Position[0] : string.Empty,
                                            YCoordinate = QuizBranchingLogicLink.Position.Count() > 1 ? QuizBranchingLogicLink.Position[1] : string.Empty,
                                            CompanyId = CompanyId,
                                            QuizId = QuizBranchingLogicObj.QuizId
                                        };
                                        UOWObj.CoordinatesInBranchingLogicRepository.Insert(cordinatesInBranchingLogic);
                                    }
                                    else
                                    {
                                        cordinatesInBranchingLogicObj.XCoordinate = QuizBranchingLogicLink.Position.Count() > 1 ? QuizBranchingLogicLink.Position[0] : string.Empty;
                                        cordinatesInBranchingLogicObj.YCoordinate = QuizBranchingLogicLink.Position.Count() > 1 ? QuizBranchingLogicLink.Position[1] : string.Empty;
                                        cordinatesInBranchingLogicObj.CompanyId = CompanyId;
                                        cordinatesInBranchingLogicObj.QuizId = QuizBranchingLogicObj.QuizId;

                                        UOWObj.CoordinatesInBranchingLogicRepository.Update(cordinatesInBranchingLogicObj);
                                    }
                                }
                                else
                                {
                                    var cordinatesInBranchingLogicObj = UOWObj.CoordinatesInBranchingLogicRepository.Get(a => a.ObjectId == QuizBranchingLogicObj.QuizId && a.ObjectTypeId == (int)QuizBranchingLogicLink.ObjectType).FirstOrDefault();
                                    if (cordinatesInBranchingLogicObj == null)
                                    {
                                        var cordinatesInBranchingLogic = new Db.CoordinatesInBranchingLogic
                                        {
                                            ObjectId = QuizBranchingLogicObj.QuizId,
                                            ObjectTypeId = (int)QuizBranchingLogicLink.ObjectType,
                                            XCoordinate = QuizBranchingLogicLink.Position.Count() > 1 ? QuizBranchingLogicLink.Position[0] : string.Empty,
                                            YCoordinate = QuizBranchingLogicLink.Position.Count() > 1 ? QuizBranchingLogicLink.Position[1] : string.Empty,
                                            CompanyId = CompanyId,
                                            QuizId = QuizBranchingLogicObj.QuizId
                                        };
                                        UOWObj.CoordinatesInBranchingLogicRepository.Insert(cordinatesInBranchingLogic);
                                    }
                                    else
                                    {
                                        cordinatesInBranchingLogicObj.XCoordinate = QuizBranchingLogicLink.Position.Count() > 1 ? QuizBranchingLogicLink.Position[0] : string.Empty;
                                        cordinatesInBranchingLogicObj.YCoordinate = QuizBranchingLogicLink.Position.Count() > 1 ? QuizBranchingLogicLink.Position[1] : string.Empty;
                                        cordinatesInBranchingLogicObj.CompanyId = CompanyId;
                                        cordinatesInBranchingLogicObj.QuizId = QuizBranchingLogicObj.QuizId;

                                        UOWObj.CoordinatesInBranchingLogicRepository.Update(cordinatesInBranchingLogicObj);
                                    }
                                }
                            }
                            UOWObj.Save();

                        }

                        
                        
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz not found for the QuizId " + QuizBranchingLogicObj.QuizId;
                    }
                }

                #endregion
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        private bool CheckIfValid(int FromType)
        {
            switch (FromType)
            {
                case (int)BranchingLogicEnum.QUESTION:
                    return false;
                case (int)BranchingLogicEnum.CONTENT:
                    return false;
                case (int)BranchingLogicEnum.RESULT:
                    return false;
                case (int)BranchingLogicEnum.ACTION:
                    return false;
                case (int)BranchingLogicEnum.BADGE:
                    return false;
                case (int)BranchingLogicEnum.WHATSAPPTEMPLATE:
                    return false;
            }
            return true;
        }

        static BranchingLogicModel GetQuestion(List<BranchingLogicModel> QuestionList, List<ContentBranchingLogic> ContentList, int? ContentId)
        {

            var content = ContentList.FirstOrDefault(a => a.ContentId == ContentId);

            if (content.LinkedToType == (int)BranchingLogicEnum.QUESTION)
            {
                return QuestionList.FirstOrDefault(a => a.QuestionId == content.LinkedQuestionId);
            }
            else if (content.LinkedToType == (int)BranchingLogicEnum.CONTENT)
            {
                return GetQuestion(QuestionList, ContentList, content.LinkedContentId);
            }

            return null;
        }

        static bool CheckIfBranchLoopExists(BranchingLinks node, string ObjectId, int objectType)
        {
            bool isExitsts = false;
            isExitsts = (node.ToId.Equals(ObjectId.ToString()) && (int)node.ToType == objectType);
            if (isExitsts) return isExitsts;
            if (node.LinkedTypeObj == null) return isExitsts;
            isExitsts = CheckIfBranchLoopExists(node.LinkedTypeObj, ObjectId, objectType);
            return isExitsts;
        }

        public List<QuizResult> GetQuizResults(int QuizId)
        {
            List<QuizResult> quizResultLst = new List<QuizResult>();

            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizObj = UOWObj.QuizRepository.GetByID(QuizId);

                    if (quizObj != null)
                    {
                        var quizDetails = quizObj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED);

                        if (quizDetails != null)
                        {
                            var quizResults = quizDetails.QuizResults.Where(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active);

                            foreach (var result in quizResults)
                            {
                                var quizResult = new QuizResult();
                                quizResult.ResultId = result.Id;
                                quizResult.Title = result.Title;
                                quizResult.ShowExternalTitle = result.ShowExternalTitle;
                                quizResult.InternalTitle = result.InternalTitle ?? string.Empty;
                                quizResult.ShowInternalTitle = result.ShowInternalTitle;
                                quizResult.ShowResultImage = result.ShowResultImage;
                                quizResult.Image = result.Image;
                                quizResult.EnableMediaFile = result.EnableMediaFile;
                                quizResult.PublicIdForResult = result.PublicId;
                                quizResult.Description = result.Description;
                                quizResult.ShowDescription = result.ShowDescription;
                                quizResult.HideCallToAction = result.HideCallToAction;
                                quizResult.ActionButtonURL = result.ActionButtonURL;
                                quizResult.OpenLinkInNewTab = result.OpenLinkInNewTab;
                                quizResult.ActionButtonTxtSize = result.ActionButtonTxtSize;
                                quizResult.ActionButtonColor = result.ActionButtonColor;
                                quizResult.ActionButtonTitleColor = result.ActionButtonTitleColor;
                                quizResult.ActionButtonText = result.ActionButtonText;

                                quizResult.ResultSetting = new QuizResultSetting();

                                var resultSetting = result.QuizDetails.ResultSettings.FirstOrDefault();

                                if (resultSetting != null)
                                {
                                    quizResult.ResultSetting.QuizId = resultSetting.QuizDetails.ParentQuizId;
                                    quizResult.ResultSetting.ShowScoreValue = resultSetting.ShowScoreValue;
                                    quizResult.ResultSetting.ShowCorrectAnswer = resultSetting.ShowCorrectAnswer;
                                    quizResult.ResultSetting.CustomTxtForScoreValueInResult = resultSetting.CustomTxtForScoreValueInResult;
                                    quizResult.ResultSetting.CustomTxtForAnswerKey = resultSetting.CustomTxtForAnswerKey;
                                    quizResult.ResultSetting.CustomTxtForYourAnswer = resultSetting.CustomTxtForYourAnswer;
                                    quizResult.ResultSetting.CustomTxtForCorrectAnswer = resultSetting.CustomTxtForCorrectAnswer;
                                    quizResult.ResultSetting.CustomTxtForExplanation = resultSetting.CustomTxtForExplanation;
                                }

                                quizResultLst.Add(quizResult);
                            }
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz not found for the QuizId " + QuizId;
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
            return quizResultLst;
        }

        public int DuplicateQuiz(int QuizId, string AccessibleOfficeId, string UserType, int? ModuleType, int BusinessUserId, CompanyModel CompanyInfo, bool IsCreateAcademyCourse, bool IsCreateTechnicalRecruiterCourse)
        {
            int newQuizId = -1;
            var currentQuizVar = new Db.QuizVariables();


            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    using (var transaction = Utility.CreateTransactionScope())
                    {
                        var quizObj = UOWObj.QuizRepository.GetByID(QuizId);

                        if (quizObj != null)
                        {
                            if (quizObj.State != (int)QuizStateEnum.PUBLISHED && ((quizObj.QuizType == (int)QuizTypeEnum.AssessmentTemplate || quizObj.QuizType == (int)QuizTypeEnum.ScoreTemplate || quizObj.QuizType == (int)QuizTypeEnum.PersonalityTemplate) && !quizObj.QuizDetails.Any(r => r.State == (int)QuizStateEnum.PUBLISHED)))
                            {
                                Status = ResultEnum.OkWithMessage;
                                ErrorMessage = "Quiz cannot be duplicated because changes are not yet published for the QuizId " + QuizId;
                            }
                            else //duplicate only if quiz is published
                            {
                                var currentDate = DateTime.UtcNow;

                                #region insert in Quiz

                                var nQuizObj = new Db.Quiz();

                                nQuizObj.PublishedCode = Guid.NewGuid().ToString();

                                switch (quizObj.QuizType)
                                {
                                    case (int)QuizTypeEnum.AssessmentTemplate:
                                        nQuizObj.QuizType = (int)QuizTypeEnum.Assessment;
                                        break;
                                    case (int)QuizTypeEnum.ScoreTemplate:
                                        nQuizObj.QuizType = (int)QuizTypeEnum.Score;
                                        break;
                                    case (int)QuizTypeEnum.PersonalityTemplate:
                                        nQuizObj.QuizType = (int)QuizTypeEnum.Personality;
                                        break;
                                    default:
                                        nQuizObj.QuizType = quizObj.QuizType;
                                        break;
                                }

                                var publishedQuizDetails = new Db.QuizDetails();

                                if (quizObj.QuizType == (int)QuizTypeEnum.AssessmentTemplate || quizObj.QuizType == (int)QuizTypeEnum.ScoreTemplate || quizObj.QuizType == (int)QuizTypeEnum.PersonalityTemplate)
                                {
                                    publishedQuizDetails = quizObj.QuizDetails.Where(r => r.State == (int)QuizStateEnum.PUBLISHED).OrderByDescending(r => r.Version).FirstOrDefault();

                                    if (!string.IsNullOrEmpty(AccessibleOfficeId))
                                    {
                                        nQuizObj.AccessibleOfficeId = AccessibleOfficeId;
                                    }
                                }
                                else
                                {
                                    publishedQuizDetails = quizObj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED);

                                    if (!string.IsNullOrEmpty(quizObj.AccessibleOfficeId))
                                    {
                                        nQuizObj.AccessibleOfficeId = quizObj.AccessibleOfficeId;
                                    }
                                }

                                nQuizObj.State = (int)QuizStateEnum.DRAFTED;
                                nQuizObj.CompanyId = CompanyInfo.Id;
                                nQuizObj.QuesAndContentInSameTable = quizObj.QuesAndContentInSameTable;

                                UOWObj.QuizRepository.Insert(nQuizObj);

                                UOWObj.Save();

                                newQuizId = nQuizObj.Id;

                                #endregion

                                if (publishedQuizDetails != null)
                                {
                                    List<Mappings> mappingLst = new List<Mappings>();
                                    List<int> branchingLogicAnsIds = new List<int>();

                                    #region insert in Quiz Details

                                    var publishedQuizDetailsObj = new Db.QuizDetails();

                                    publishedQuizDetailsObj.ParentQuizId = nQuizObj.Id;
                                    publishedQuizDetailsObj.QuizTitle = publishedQuizDetails.QuizTitle;
                                    publishedQuizDetailsObj.QuizCoverTitle = publishedQuizDetails.QuizCoverTitle;
                                    publishedQuizDetailsObj.ShowQuizCoverTitle = publishedQuizDetails.ShowQuizCoverTitle;
                                    publishedQuizDetailsObj.QuizCoverImage = publishedQuizDetails.QuizCoverImage;
                                    publishedQuizDetailsObj.ShowQuizCoverImage = publishedQuizDetails.ShowQuizCoverImage;
                                    publishedQuizDetailsObj.PublicId = publishedQuizDetails.PublicId;
                                    publishedQuizDetailsObj.QuizCoverImgXCoordinate = publishedQuizDetails.QuizCoverImgXCoordinate;
                                    publishedQuizDetailsObj.QuizCoverImgYCoordinate = publishedQuizDetails.QuizCoverImgYCoordinate;
                                    publishedQuizDetailsObj.QuizCoverImgHeight = publishedQuizDetails.QuizCoverImgHeight;
                                    publishedQuizDetailsObj.QuizCoverImgWidth = publishedQuizDetails.QuizCoverImgWidth;
                                    publishedQuizDetailsObj.QuizCoverImgAttributionLabel = publishedQuizDetails.QuizCoverImgAttributionLabel;
                                    publishedQuizDetailsObj.QuizCoverImgAltTag = publishedQuizDetails.QuizCoverImgAltTag;
                                    publishedQuizDetailsObj.QuizDescription = publishedQuizDetails.QuizDescription;
                                    publishedQuizDetailsObj.ShowDescription = publishedQuizDetails.ShowDescription;
                                    publishedQuizDetailsObj.StartButtonText = publishedQuizDetails.StartButtonText;
                                    publishedQuizDetailsObj.EnableNextButton = publishedQuizDetails.EnableNextButton;
                                    publishedQuizDetailsObj.IsBranchingLogicEnabled = publishedQuizDetails.IsBranchingLogicEnabled;
                                    publishedQuizDetailsObj.HideSocialShareButtons = publishedQuizDetails.HideSocialShareButtons;
                                    publishedQuizDetailsObj.EnableFacebookShare = publishedQuizDetails.EnableFacebookShare;
                                    publishedQuizDetailsObj.EnableTwitterShare = publishedQuizDetails.EnableTwitterShare;
                                    publishedQuizDetailsObj.EnableLinkedinShare = publishedQuizDetails.EnableLinkedinShare;
                                    publishedQuizDetailsObj.State = (int)QuizStateEnum.DRAFTED;
                                    publishedQuizDetailsObj.Version = 1;
                                    publishedQuizDetailsObj.Status = (int)StatusEnum.Active;
                                    publishedQuizDetailsObj.CreatedOn = currentDate;
                                    publishedQuizDetailsObj.CreatedBy = BusinessUserId;
                                    publishedQuizDetailsObj.LastUpdatedOn = currentDate;
                                    publishedQuizDetailsObj.LastUpdatedBy = BusinessUserId;
                                    publishedQuizDetailsObj.ViewPreviousQuestion = publishedQuizDetails.ViewPreviousQuestion;
                                    publishedQuizDetailsObj.EditAnswer = publishedQuizDetails.EditAnswer;
                                    publishedQuizDetailsObj.AutoPlay = publishedQuizDetails.AutoPlay;
                                    publishedQuizDetailsObj.DisplayOrderForTitle = publishedQuizDetails.DisplayOrderForTitle;
                                    publishedQuizDetailsObj.DisplayOrderForTitleImage = publishedQuizDetails.DisplayOrderForTitleImage;
                                    publishedQuizDetailsObj.DisplayOrderForDescription = publishedQuizDetails.DisplayOrderForDescription;
                                    publishedQuizDetailsObj.DisplayOrderForNextButton = publishedQuizDetails.DisplayOrderForNextButton;
                                    publishedQuizDetailsObj.CompanyId = publishedQuizDetails.CompanyId;

                                    UOWObj.QuizDetailsRepository.Insert(publishedQuizDetailsObj);

                                    UOWObj.Save();

                                    var oldvarDetails = UOWObj.QuizVariablesRepository.Get(v => v.QuizDetailsId == publishedQuizDetails.Id && v.ObjectId == publishedQuizDetails.Id && v.ObjectTypes == (int)QuizVariableObjectTypes.COVER);

                                    if (oldvarDetails != null && oldvarDetails.Any())
                                    {
                                        foreach (var details in oldvarDetails)
                                        {
                                            currentQuizVar.QuizDetailsId = publishedQuizDetailsObj.Id;
                                            currentQuizVar.ObjectTypes = details.ObjectTypes;
                                            currentQuizVar.ObjectId = publishedQuizDetailsObj.Id;
                                            currentQuizVar.CompanyId = details.CompanyId;
                                            currentQuizVar.Variables = details.Variables;
                                            UOWObj.QuizVariablesRepository.Insert(currentQuizVar);
                                            UOWObj.Save();
                                        }

                                    }


                                    #endregion

                                    #region insert in Quiz Results

                                    var lstResults = publishedQuizDetails.QuizResults.Where(r => r.Status == (int)StatusEnum.Active).ToList();

                                    foreach (var item in lstResults)
                                    {
                                        var quizResultObj = new Db.QuizResults();

                                        quizResultObj.QuizId = publishedQuizDetailsObj.Id;
                                        quizResultObj.Title = item.Title;
                                        quizResultObj.ShowExternalTitle = item.ShowExternalTitle;
                                        quizResultObj.InternalTitle = item.InternalTitle;
                                        quizResultObj.ShowInternalTitle = item.ShowInternalTitle;
                                        quizResultObj.Image = item.Image;
                                        quizResultObj.PublicId = item.PublicId;
                                        quizResultObj.Description = item.Description;
                                        quizResultObj.ShowDescription = item.ShowDescription;
                                        quizResultObj.ActionButtonURL = item.ActionButtonURL;
                                        quizResultObj.ActionButtonTxtSize = item.ActionButtonTxtSize;
                                        quizResultObj.ActionButtonTitleColor = item.ActionButtonTitleColor;
                                        quizResultObj.LastUpdatedOn = currentDate;
                                        quizResultObj.LastUpdatedBy = BusinessUserId;
                                        quizResultObj.HideCallToAction = item.HideCallToAction;
                                        quizResultObj.ActionButtonText = item.ActionButtonText;
                                        quizResultObj.ShowResultImage = item.ShowResultImage;
                                        quizResultObj.OpenLinkInNewTab = item.OpenLinkInNewTab;
                                        quizResultObj.ActionButtonColor = item.ActionButtonColor;
                                        quizResultObj.MinScore = item.MinScore;
                                        quizResultObj.MaxScore = item.MaxScore;
                                        quizResultObj.DisplayOrder = item.DisplayOrder;
                                        quizResultObj.IsPersonalityCorrelatedResult = item.IsPersonalityCorrelatedResult;
                                        quizResultObj.DisplayOrderForTitle = item.DisplayOrderForTitle;
                                        quizResultObj.DisplayOrderForTitleImage = item.DisplayOrderForTitleImage;
                                        quizResultObj.DisplayOrderForDescription = item.DisplayOrderForDescription;
                                        quizResultObj.DisplayOrderForNextButton = item.DisplayOrderForNextButton;
                                        quizResultObj.Status = (int)StatusEnum.Active;
                                        quizResultObj.State = (int)QuizStateEnum.DRAFTED;
                                        //  quizResultObj.ShowLeadUserForm = item.ShowLeadUserForm;
                                        quizResultObj.AutoPlay = item.AutoPlay;

                                        UOWObj.QuizResultsRepository.Insert(quizResultObj);

                                        UOWObj.Save();

                                        mappingLst.Add(new Mappings
                                        {
                                            DraftedId = item.Id,
                                            PublishedId = quizResultObj.Id,
                                            Type = (int)BranchingLogicEnum.RESULT
                                        });

                                        var oldvarResultDetails = UOWObj.QuizVariablesRepository.Get(v => v.QuizDetailsId == publishedQuizDetails.Id && v.ObjectId == item.Id && v.ObjectTypes == (int)QuizVariableObjectTypes.RESULT);
                                        if (oldvarResultDetails != null && oldvarResultDetails.Any())
                                        {
                                            foreach (var result in oldvarResultDetails)
                                            {
                                                currentQuizVar.QuizDetailsId = publishedQuizDetailsObj.Id;
                                                currentQuizVar.ObjectTypes = result.ObjectTypes;
                                                currentQuizVar.ObjectId = quizResultObj.Id;
                                                currentQuizVar.CompanyId = result.CompanyId;
                                                currentQuizVar.Variables = result.Variables;
                                                UOWObj.QuizVariablesRepository.Insert(currentQuizVar);
                                                UOWObj.Save();
                                            }

                                        }

                                    }

                                    #endregion

                                    #region insert in Result Settings

                                    var draftedResultSetting = publishedQuizDetails.ResultSettings.FirstOrDefault();

                                    if (draftedResultSetting != null)
                                    {
                                        var resultSettingObj = new Db.ResultSettings();

                                        resultSettingObj.QuizId = publishedQuizDetailsObj.Id;
                                        resultSettingObj.ShowScoreValue = draftedResultSetting.ShowScoreValue;
                                        resultSettingObj.ShowCorrectAnswer = draftedResultSetting.ShowCorrectAnswer;
                                        resultSettingObj.MinScore = draftedResultSetting.MinScore;
                                        resultSettingObj.CustomTxtForAnswerKey = draftedResultSetting.CustomTxtForAnswerKey;
                                        resultSettingObj.CustomTxtForYourAnswer = draftedResultSetting.CustomTxtForYourAnswer;
                                        resultSettingObj.CustomTxtForCorrectAnswer = draftedResultSetting.CustomTxtForCorrectAnswer;
                                        resultSettingObj.CustomTxtForExplanation = draftedResultSetting.CustomTxtForExplanation;
                                        resultSettingObj.CustomTxtForScoreValueInResult = draftedResultSetting.CustomTxtForScoreValueInResult;
                                        resultSettingObj.LastUpdatedBy = BusinessUserId;
                                        resultSettingObj.LastUpdatedOn = currentDate;
                                        resultSettingObj.State = (int)QuizStateEnum.DRAFTED;

                                        UOWObj.ResultSettingsRepository.Insert(resultSettingObj);

                                        UOWObj.Save();
                                    }

                                    #endregion

                                    #region insert in Quiz personality setting
                                    if (quizObj.QuizType == (int)QuizTypeEnum.Personality || quizObj.QuizType == (int)QuizTypeEnum.PersonalityTemplate)
                                    {
                                        var personalityResult = publishedQuizDetails.PersonalityResultSetting.FirstOrDefault();
                                        var personalityResultObj = new Db.PersonalityResultSetting();

                                        personalityResultObj.QuizId = publishedQuizDetailsObj.Id;
                                        personalityResultObj.Title = personalityResult.Title;
                                        personalityResultObj.Status = personalityResult.Status;
                                        personalityResultObj.MaxResult = personalityResult.MaxResult;
                                        personalityResultObj.GraphColor = personalityResult.GraphColor;
                                        personalityResultObj.ButtonColor = personalityResult.ButtonColor;
                                        personalityResultObj.ButtonFontColor = personalityResult.ButtonFontColor;
                                        personalityResultObj.SideButtonText = personalityResult.SideButtonText;
                                        personalityResultObj.IsFullWidthEnable = personalityResult.IsFullWidthEnable;
                                        personalityResultObj.LastUpdatedOn = currentDate;
                                        personalityResultObj.LastUpdatedBy = BusinessUserId;
                                        //personalityResultObj.ShowLeadUserForm = personalityResult.ShowLeadUserForm;

                                        UOWObj.PersonalityResultSettingRepository.Insert(personalityResultObj);
                                        UOWObj.Save();
                                    }
                                    #endregion

                                    #region insert in Quiz BrandingAndStyle

                                    var draftedBrandingAndStyle = publishedQuizDetails.QuizBrandingAndStyle.FirstOrDefault();

                                    if (draftedBrandingAndStyle != null)
                                    {
                                        var brandingAndStyleObj = new Db.QuizBrandingAndStyle();

                                        brandingAndStyleObj.QuizId = publishedQuizDetailsObj.Id;
                                        brandingAndStyleObj.ImageFileURL = draftedBrandingAndStyle.ImageFileURL;
                                        brandingAndStyleObj.PublicId = draftedBrandingAndStyle.PublicId;
                                        brandingAndStyleObj.BackgroundColor = draftedBrandingAndStyle.BackgroundColor;
                                        brandingAndStyleObj.ButtonColor = draftedBrandingAndStyle.ButtonColor;
                                        brandingAndStyleObj.OptionColor = draftedBrandingAndStyle.OptionColor;
                                        brandingAndStyleObj.ButtonFontColor = draftedBrandingAndStyle.ButtonFontColor;
                                        brandingAndStyleObj.OptionFontColor = draftedBrandingAndStyle.OptionFontColor;
                                        brandingAndStyleObj.FontColor = draftedBrandingAndStyle.FontColor;
                                        brandingAndStyleObj.ButtonHoverColor = draftedBrandingAndStyle.ButtonHoverColor;
                                        brandingAndStyleObj.ButtonHoverTextColor = draftedBrandingAndStyle.ButtonHoverTextColor;
                                        brandingAndStyleObj.FontType = draftedBrandingAndStyle.FontType;
                                        brandingAndStyleObj.BackgroundColorofSelectedAnswer = draftedBrandingAndStyle.BackgroundColorofSelectedAnswer;
                                        brandingAndStyleObj.BackgroundColorofAnsweronHover = draftedBrandingAndStyle.BackgroundColorofAnsweronHover;
                                        brandingAndStyleObj.AnswerTextColorofSelectedAnswer = draftedBrandingAndStyle.AnswerTextColorofSelectedAnswer;
                                        brandingAndStyleObj.ApplyToAll = draftedBrandingAndStyle.ApplyToAll;
                                        brandingAndStyleObj.IsBackType = draftedBrandingAndStyle.IsBackType;
                                        brandingAndStyleObj.BackColor = draftedBrandingAndStyle.BackColor;
                                        brandingAndStyleObj.Opacity = draftedBrandingAndStyle.Opacity;
                                        brandingAndStyleObj.LogoUrl = draftedBrandingAndStyle.LogoUrl;
                                        brandingAndStyleObj.LogoPublicId = draftedBrandingAndStyle.LogoPublicId;
                                        brandingAndStyleObj.BackgroundColorofLogo = draftedBrandingAndStyle.BackgroundColorofLogo;
                                        brandingAndStyleObj.AutomationAlignment = draftedBrandingAndStyle.AutomationAlignment;
                                        brandingAndStyleObj.LogoAlignment = draftedBrandingAndStyle.LogoAlignment;
                                        brandingAndStyleObj.Flip = draftedBrandingAndStyle.Flip;
                                        brandingAndStyleObj.Language = draftedBrandingAndStyle.Language;
                                        brandingAndStyleObj.BackImageFileURL = draftedBrandingAndStyle.BackImageFileURL;
                                        brandingAndStyleObj.LastUpdatedBy = BusinessUserId;
                                        brandingAndStyleObj.LastUpdatedOn = currentDate;
                                        brandingAndStyleObj.State = (int)QuizStateEnum.DRAFTED;

                                        UOWObj.QuizBrandingAndStyleRepository.Insert(brandingAndStyleObj);

                                        UOWObj.Save();
                                    }

                                    #endregion

                                    #region insert in Questions

                                    var lstQuestions = publishedQuizDetails.QuestionsInQuiz.Where(r => r.Status == (int)StatusEnum.Active).ToList();

                                    foreach (var question in lstQuestions)
                                    {
                                        var questionObj = new Db.QuestionsInQuiz();

                                        questionObj.QuizId = publishedQuizDetailsObj.Id;
                                        questionObj.Question = question.Question;
                                        questionObj.ShowTitle = question.ShowTitle;
                                        questionObj.QuestionImage = question.QuestionImage;
                                        questionObj.PublicId = question.PublicId;
                                        questionObj.CorrectAnswerDescription = question.CorrectAnswerDescription;
                                        questionObj.RevealCorrectAnswer = question.RevealCorrectAnswer;
                                        questionObj.AliasTextForCorrect = question.AliasTextForCorrect;
                                        questionObj.AliasTextForIncorrect = question.AliasTextForIncorrect;
                                        questionObj.AliasTextForYourAnswer = question.AliasTextForYourAnswer;
                                        questionObj.AliasTextForCorrectAnswer = question.AliasTextForCorrectAnswer;
                                        questionObj.AliasTextForExplanation = question.AliasTextForExplanation;
                                        questionObj.AliasTextForNextButton = question.AliasTextForNextButton;
                                        questionObj.EnableNextButton = question.EnableNextButton;
                                        questionObj.ShowQuestionImage = question.ShowQuestionImage;
                                        questionObj.LastUpdatedOn = currentDate;
                                        questionObj.LastUpdatedBy = BusinessUserId;
                                        questionObj.DisplayOrder = question.DisplayOrder;
                                        questionObj.ShowAnswerImage = question.ShowAnswerImage;
                                        questionObj.Status = (int)StatusEnum.Active;
                                        questionObj.State = (int)QuizStateEnum.DRAFTED;
                                        questionObj.AnswerType = question.AnswerType;
                                        questionObj.MinAnswer = question.MinAnswer;
                                        questionObj.MaxAnswer = question.MaxAnswer;
                                        questionObj.NextButtonColor = question.NextButtonColor;
                                        questionObj.NextButtonText = question.NextButtonText;
                                        questionObj.NextButtonTxtColor = question.NextButtonTxtColor;
                                        questionObj.NextButtonTxtSize = question.NextButtonTxtSize;
                                        questionObj.ViewPreviousQuestion = question.ViewPreviousQuestion;
                                        questionObj.EditAnswer = question.EditAnswer;
                                        questionObj.TimerRequired = question.TimerRequired;
                                        questionObj.Time = question.Time;
                                        questionObj.AutoPlay = question.AutoPlay;
                                        questionObj.Description = question.Description;
                                        questionObj.ShowDescription = question.ShowDescription;
                                        questionObj.DescriptionImage = question.DescriptionImage;
                                        questionObj.EnableMediaFileForDescription = question.EnableMediaFileForDescription;
                                        questionObj.ShowDescriptionImage = question.ShowDescriptionImage;
                                        questionObj.PublicIdForDescription = question.PublicIdForDescription;
                                        questionObj.AutoPlayForDescription = question.AutoPlayForDescription;
                                        questionObj.Type = question.Type;
                                        questionObj.DisplayOrderForTitle = question.DisplayOrderForTitle;
                                        questionObj.DisplayOrderForTitleImage = question.DisplayOrderForTitleImage;
                                        questionObj.DisplayOrderForDescription = question.DisplayOrderForDescription;
                                        questionObj.DisplayOrderForDescriptionImage = question.DisplayOrderForDescriptionImage;
                                        questionObj.DisplayOrderForAnswer = question.DisplayOrderForAnswer;
                                        questionObj.DisplayOrderForNextButton = question.DisplayOrderForNextButton;
                                        questionObj.EnableComment = question.EnableComment;
                                        questionObj.TopicTitle = question.TopicTitle;
                                        questionObj.AnswerStructureType = question.AnswerStructureType;
                                        questionObj.TemplateId = question.TemplateId;
                                        questionObj.LanguageCode = question.LanguageCode;
                                        questionObj.IsMultiRating = question.IsMultiRating;

                                        UOWObj.QuestionsInQuizRepository.Insert(questionObj);

                                        UOWObj.Save();

                                        mappingLst.Add(new Mappings
                                        {
                                            DraftedId = question.Id,
                                            PublishedId = questionObj.Id,
                                            Type = question.Type
                                        });

                                        var oldvarQuesDetails = UOWObj.QuizVariablesRepository.Get(v => v.QuizDetailsId == publishedQuizDetails.Id && v.ObjectId == question.Id && v.ObjectTypes == (int)QuizVariableObjectTypes.QUESTION);
                                        if (oldvarQuesDetails != null && oldvarQuesDetails.Any())
                                        {
                                            foreach (var ques in oldvarQuesDetails)
                                            {
                                                currentQuizVar.QuizDetailsId = publishedQuizDetailsObj.Id;
                                                currentQuizVar.ObjectTypes = ques.ObjectTypes;
                                                currentQuizVar.ObjectId = questionObj.Id;
                                                currentQuizVar.CompanyId = ques.CompanyId;
                                                currentQuizVar.Variables = ques.Variables;
                                                UOWObj.QuizVariablesRepository.Insert(currentQuizVar);
                                                UOWObj.Save();
                                            }

                                        }

                                        #region insert in AnswerOptions

                                        var lstAnswers = question.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active).ToList();

                                        foreach (var answer in lstAnswers)
                                        {
                                            var answerObj = new Db.AnswerOptionsInQuizQuestions();

                                            answerObj.QuestionId = questionObj.Id;
                                            answerObj.QuizId = questionObj.QuizId;
                                            answerObj.Option = answer.Option;
                                            answerObj.Description = answer.Description;
                                            answerObj.OptionImage = answer.OptionImage;
                                            answerObj.PublicId = answer.PublicId;
                                            answerObj.AssociatedScore = answer.AssociatedScore;
                                            answerObj.IsCorrectAnswer = answer.IsCorrectAnswer;
                                            answerObj.IsCorrectForMultipleAnswer = answer.IsCorrectForMultipleAnswer;
                                            answerObj.LastUpdatedOn = currentDate;
                                            answerObj.LastUpdatedBy = BusinessUserId;
                                            answerObj.DisplayOrder = answer.DisplayOrder;
                                            answerObj.IsReadOnly = answer.IsReadOnly;
                                            answerObj.IsUnansweredType = answer.IsUnansweredType;
                                            answerObj.AutoPlay = answer.AutoPlay;
                                            answerObj.OptionTextforRatingOne = answer.OptionTextforRatingOne;
                                            answerObj.OptionTextforRatingTwo = answer.OptionTextforRatingTwo;
                                            answerObj.OptionTextforRatingThree = answer.OptionTextforRatingThree;
                                            answerObj.OptionTextforRatingFour = answer.OptionTextforRatingFour;
                                            answerObj.OptionTextforRatingFive = answer.OptionTextforRatingFive;
                                            answerObj.Status = (int)StatusEnum.Active;
                                            answerObj.State = (int)QuizStateEnum.DRAFTED;
                                            answerObj.ListValues = answer.ListValues;
                                            answerObj.RefId = answer.RefId;

                                            UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);

                                            UOWObj.Save();
                                            if (question.Type == (int)BranchingLogicEnum.WHATSAPPTEMPLATE)
                                            {
                                                mappingLst.Add(new Mappings
                                                {
                                                    DraftedId = answer.Id,
                                                    PublishedId = answerObj.Id,
                                                    Type = (int)BranchingLogicEnum.WHATSAPPTEMPLATEACTION
                                                });
                                            }
                                            else
                                            {
                                                mappingLst.Add(new Mappings
                                                {
                                                    DraftedId = answer.Id,
                                                    PublishedId = answerObj.Id,
                                                    Type = (int)BranchingLogicEnum.ANSWER
                                                });
                                            }

                                            #region insert in ObjectFieldsInAnswer

                                            foreach (var objectFieldsInAnswer in answer.ObjectFieldsInAnswer)
                                            {
                                                var objectFieldsInAnswerObj = new Db.ObjectFieldsInAnswer();

                                                objectFieldsInAnswerObj.AnswerOptionsInQuizQuestionsId = answerObj.Id;
                                                objectFieldsInAnswerObj.ObjectName = objectFieldsInAnswer.ObjectName;
                                                objectFieldsInAnswerObj.FieldName = objectFieldsInAnswer.FieldName;
                                                objectFieldsInAnswerObj.Value = objectFieldsInAnswer.Value;
                                                objectFieldsInAnswerObj.CreatedOn = currentDate;
                                                objectFieldsInAnswerObj.CreatedBy = BusinessUserId;
                                                objectFieldsInAnswerObj.LastUpdatedOn = currentDate;
                                                objectFieldsInAnswerObj.LastUpdatedBy = BusinessUserId;
                                                objectFieldsInAnswerObj.IsExternalSync = objectFieldsInAnswer.IsExternalSync;
                                                objectFieldsInAnswerObj.IsCommentMapped = objectFieldsInAnswer.IsCommentMapped;

                                                UOWObj.ObjectFieldsInAnswerRepository.Insert(objectFieldsInAnswerObj);

                                                UOWObj.Save();
                                            }

                                            #endregion

                                            var lstTags = answer.TagsInAnswer.ToList();

                                            foreach (var tag in lstTags)
                                            {
                                                var TagsObj = new Db.TagsInAnswer();
                                                TagsObj.AnswerOptionsId = answerObj.Id;
                                                TagsObj.TagCategoryId = tag.TagCategoryId;
                                                TagsObj.TagId = tag.TagId;
                                                UOWObj.TagsInAnswerRepository.Insert(TagsObj);
                                                UOWObj.Save();
                                            }

                                            #region insert in Correlation
                                            if (quizObj.QuizType == (int)QuizTypeEnum.Personality || quizObj.QuizType == (int)QuizTypeEnum.PersonalityTemplate)
                                            {
                                                var lstCorrelation = answer.PersonalityAnswerResultMapping.Where(r => r.QuizResults.Status == (int)StatusEnum.Active);
                                                foreach (var correlation in lstCorrelation)
                                                {
                                                    var resultMappingObj = new Db.PersonalityAnswerResultMapping();
                                                    resultMappingObj.AnswerId = answerObj.Id;
                                                    resultMappingObj.ResultId = publishedQuizDetailsObj.QuizResults.FirstOrDefault(r => lstResults.FirstOrDefault(x => x.Id == correlation.ResultId).DisplayOrder.Equals(r.DisplayOrder)).Id;
                                                    UOWObj.PersonalityAnswerResultMappingRepository.Insert(resultMappingObj);
                                                    UOWObj.Save();
                                                }
                                            }
                                            #endregion
                                        }
                                        #endregion
                                    }

                                    #endregion

                                    #region insert in Content

                                    var lstContent = publishedQuizDetails.ContentsInQuiz.Where(r => r.Status == (int)StatusEnum.Active).ToList();

                                    foreach (var content in lstContent)
                                    {
                                        var contentObj = new Db.ContentsInQuiz();

                                        contentObj.QuizId = publishedQuizDetailsObj.Id;
                                        contentObj.ContentTitle = content.ContentTitle;
                                        contentObj.ShowTitle = content.ShowTitle;
                                        contentObj.ContentTitleImage = content.ContentTitleImage;
                                        contentObj.PublicIdForContentTitle = content.PublicIdForContentTitle;
                                        contentObj.ContentDescription = content.ContentDescription;
                                        contentObj.ShowDescription = content.ShowDescription;
                                        contentObj.ContentDescriptionImage = content.ContentDescriptionImage;
                                        contentObj.PublicIdForContentDescription = content.PublicIdForContentDescription;
                                        contentObj.ShowContentDescriptionImage = content.ShowContentDescriptionImage;
                                        contentObj.AliasTextForNextButton = content.AliasTextForNextButton;
                                        contentObj.EnableNextButton = content.EnableNextButton;
                                        contentObj.DisplayOrder = content.DisplayOrder;
                                        contentObj.LastUpdatedOn = currentDate;
                                        contentObj.LastUpdatedBy = BusinessUserId;
                                        contentObj.ViewPreviousQuestion = content.ViewPreviousQuestion;
                                        contentObj.AutoPlay = content.AutoPlay;
                                        contentObj.AutoPlayForDescription = content.AutoPlayForDescription;
                                        contentObj.DisplayOrderForTitle = content.DisplayOrderForTitle;
                                        contentObj.DisplayOrderForTitleImage = content.DisplayOrderForTitleImage;
                                        contentObj.DisplayOrderForDescription = content.DisplayOrderForDescription;
                                        contentObj.DisplayOrderForDescriptionImage = content.DisplayOrderForDescriptionImage;
                                        contentObj.DisplayOrderForNextButton = content.DisplayOrderForNextButton;

                                        contentObj.Status = (int)StatusEnum.Active;
                                        contentObj.State = (int)QuizStateEnum.DRAFTED;

                                        UOWObj.ContentsInQuizRepository.Insert(contentObj);

                                        UOWObj.Save();

                                        mappingLst.Add(new Mappings
                                        {
                                            DraftedId = content.Id,
                                            PublishedId = contentObj.Id,
                                            Type = (int)BranchingLogicEnum.CONTENT
                                        });
                                    }

                                    #endregion

                                    #region insert in Action

                                    var lstAction = publishedQuizDetails.ActionsInQuiz.Where(r => r.Status == (int)StatusEnum.Active).ToList();

                                    foreach (var action in lstAction)
                                    {
                                        var actionObj = new Db.ActionsInQuiz();

                                        actionObj.QuizId = publishedQuizDetailsObj.Id;
                                        actionObj.Title = action.Title;
                                        actionObj.ReportEmails = action.ReportEmails;
                                        actionObj.AppointmentId = action.AppointmentId;
                                        actionObj.AutomationId = action.AutomationId;
                                        actionObj.ActionType = action.ActionType;
                                        actionObj.LastUpdatedOn = currentDate;
                                        actionObj.LastUpdatedBy = BusinessUserId;
                                        actionObj.Status = (int)StatusEnum.Active;
                                        actionObj.State = (int)QuizStateEnum.DRAFTED;

                                        UOWObj.ActionsInQuizRepository.Insert(actionObj);

                                        UOWObj.Save();

                                        mappingLst.Add(new Mappings
                                        {
                                            DraftedId = action.Id,
                                            PublishedId = actionObj.Id,
                                            Type = (int)BranchingLogicEnum.ACTION
                                        });

                                        #region insert in LinkedCalendarInAction

                                        foreach (var calObj in action.LinkedCalendarInAction)
                                        {
                                            var linkedCalendarInActionObj = new Db.LinkedCalendarInAction();
                                            linkedCalendarInActionObj.ActionsInQuizId = actionObj.Id;
                                            linkedCalendarInActionObj.CalendarId = calObj.CalendarId;
                                            UOWObj.LinkedCalendarInActionRepository.Insert(linkedCalendarInActionObj);
                                        }

                                        UOWObj.Save();

                                        #endregion
                                    }

                                    #endregion

                                    #region insert in Badge

                                    var lstBadge = publishedQuizDetails.BadgesInQuiz.Where(r => r.Status == (int)StatusEnum.Active).ToList();

                                    foreach (var badge in lstBadge)
                                    {
                                        var badgeObj = new Db.BadgesInQuiz();

                                        badgeObj.QuizId = publishedQuizDetailsObj.Id;
                                        badgeObj.Title = badge.Title;
                                        badgeObj.ShowTitle = badge.ShowTitle;
                                        badgeObj.Image = badge.Image;
                                        badgeObj.ShowImage = badge.ShowImage;
                                        badgeObj.PublicId = badge.PublicId;
                                        badgeObj.DisplayOrderForTitle = badge.DisplayOrderForTitle;
                                        badgeObj.DisplayOrderForTitleImage = badge.DisplayOrderForTitleImage;
                                        badgeObj.LastUpdatedOn = currentDate;
                                        badgeObj.LastUpdatedBy = BusinessUserId;
                                        badgeObj.Status = (int)StatusEnum.Active;
                                        badgeObj.State = (int)QuizStateEnum.DRAFTED;

                                        UOWObj.BadgesInQuizRepository.Insert(badgeObj);

                                        UOWObj.Save();

                                        mappingLst.Add(new Mappings
                                        {
                                            DraftedId = badge.Id,
                                            PublishedId = badgeObj.Id,
                                            Type = (int)BranchingLogicEnum.BADGE
                                        });
                                    }

                                    #endregion

                                    #region insert into VariableInQuiz

                                    var lstVariableInQuiz = publishedQuizDetails.VariableInQuiz.Where(t => t.NumberOfUses > 0);

                                    foreach (var variable in lstVariableInQuiz)
                                    {
                                        var variableInQuizObj = new Db.VariableInQuiz();

                                        variableInQuizObj.QuizId = publishedQuizDetailsObj.Id;
                                        variableInQuizObj.NumberOfUses = variable.NumberOfUses;
                                        variableInQuizObj.VariableId = variable.VariableId;
                                        UOWObj.VariableInQuizRepository.Insert(variableInQuizObj);
                                        UOWObj.Save();
                                    }

                                    #endregion                            

                                    #region insert in Quiz Branching Logic

                                    var branchingLogic = quizObj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED).BranchingLogic.ToList();
                                    List<Db.CoordinatesInBranchingLogic> coordinatesInBranchingLogic = UOWObj.CoordinatesInBranchingLogicRepository.Get().ToList();

                                    foreach (var branching in branchingLogic)
                                    {
                                        var newBranching = new Db.BranchingLogic();
                                        if (branching.SourceTypeId == (int)BranchingLogicEnum.ANSWER || branching.SourceTypeId == (int)BranchingLogicEnum.QUESTION)
                                            newBranching.SourceObjectId = mappingLst.FirstOrDefault(a => a.DraftedId == branching.SourceObjectId && a.Type == branching.SourceTypeId).PublishedId;
                                        else if (branching.SourceTypeId == (int)BranchingLogicEnum.QUESTIONNEXT)
                                            newBranching.SourceObjectId = mappingLst.FirstOrDefault(a => a.DraftedId == branching.SourceObjectId && a.Type == (int)BranchingLogicEnum.QUESTION).PublishedId;
                                        else if (branching.SourceTypeId == (int)BranchingLogicEnum.ACTION || branching.SourceTypeId == (int)BranchingLogicEnum.ACTIONNEXT)

                                            newBranching.SourceObjectId = mappingLst.FirstOrDefault(a => a.DraftedId == branching.SourceObjectId && a.Type == ((int)BranchingLogicEnum.ACTION)).PublishedId;
                                        else if (branching.SourceTypeId == (int)BranchingLogicEnum.CONTENT || branching.SourceTypeId == (int)BranchingLogicEnum.CONTENTNEXT)
                                            newBranching.SourceObjectId = mappingLst.FirstOrDefault(a => a.DraftedId == branching.SourceObjectId && a.Type == ((int)BranchingLogicEnum.CONTENT)).PublishedId;
                                        else if (branching.SourceTypeId == (int)BranchingLogicEnum.RESULT || branching.SourceTypeId == (int)BranchingLogicEnum.RESULTNEXT)
                                            newBranching.SourceObjectId = mappingLst.FirstOrDefault(a => a.DraftedId == branching.SourceObjectId && a.Type == ((int)BranchingLogicEnum.RESULT)).PublishedId;
                                        else if (branching.SourceTypeId == (int)BranchingLogicEnum.BADGE || branching.SourceTypeId == (int)BranchingLogicEnum.BADGENEXT)
                                            newBranching.SourceObjectId = mappingLst.FirstOrDefault(a => a.DraftedId == branching.SourceObjectId && a.Type == ((int)BranchingLogicEnum.BADGE)).PublishedId;

                                        else if (branching.SourceTypeId == (int)BranchingLogicEnum.WHATSAPPTEMPLATE || branching.SourceTypeId == (int)BranchingLogicEnum.WHATSAPPTEMPLATEACTION)
                                            newBranching.SourceObjectId = mappingLst.FirstOrDefault(a => a.DraftedId == branching.SourceObjectId && a.Type == branching.SourceTypeId).PublishedId;

                                        if (branching.DestinationTypeId == (int)BranchingLogicEnum.ANSWER || branching.DestinationTypeId == (int)BranchingLogicEnum.QUESTION)
                                            newBranching.DestinationObjectId = mappingLst.FirstOrDefault(a => a.DraftedId == branching.DestinationObjectId && a.Type == branching.DestinationTypeId).PublishedId;
                                        else if (branching.DestinationTypeId == (int)BranchingLogicEnum.QUESTIONNEXT)
                                            newBranching.DestinationObjectId = mappingLst.FirstOrDefault(a => a.DraftedId == branching.DestinationObjectId && a.Type == (int)BranchingLogicEnum.QUESTION).PublishedId;
                                        else if (branching.DestinationTypeId == (int)BranchingLogicEnum.ACTION || branching.DestinationTypeId == (int)BranchingLogicEnum.ACTIONNEXT)
                                            newBranching.DestinationObjectId = mappingLst.FirstOrDefault(a => a.DraftedId == branching.DestinationObjectId && a.Type == ((int)BranchingLogicEnum.ACTION)).PublishedId;
                                        else if (branching.DestinationTypeId == (int)BranchingLogicEnum.CONTENT || branching.DestinationTypeId == (int)BranchingLogicEnum.CONTENTNEXT)
                                            newBranching.DestinationObjectId = mappingLst.FirstOrDefault(a => a.DraftedId == branching.DestinationObjectId && a.Type == ((int)BranchingLogicEnum.CONTENT)).PublishedId;
                                        else if (branching.DestinationTypeId == (int)BranchingLogicEnum.RESULT || branching.DestinationTypeId == (int)BranchingLogicEnum.RESULTNEXT)
                                            newBranching.DestinationObjectId = mappingLst.FirstOrDefault(a => a.DraftedId == branching.DestinationObjectId && a.Type == ((int)BranchingLogicEnum.RESULT)).PublishedId;
                                        else if (branching.DestinationTypeId == (int)BranchingLogicEnum.BADGE || branching.DestinationTypeId == (int)BranchingLogicEnum.BADGENEXT)
                                            newBranching.DestinationObjectId = mappingLst.FirstOrDefault(a => a.DraftedId == branching.DestinationObjectId && a.Type == ((int)BranchingLogicEnum.BADGE)).PublishedId;

                                        else if (branching.DestinationTypeId == (int)BranchingLogicEnum.WHATSAPPTEMPLATE || branching.DestinationTypeId == (int)BranchingLogicEnum.WHATSAPPTEMPLATEACTION)
                                            newBranching.DestinationObjectId = mappingLst.FirstOrDefault(a => a.DraftedId == branching.DestinationObjectId && a.Type == branching.DestinationTypeId).PublishedId;

                                        var branchingObj = new Db.BranchingLogic()
                                        {
                                            QuizId = publishedQuizDetailsObj.Id,
                                            SourceTypeId = branching.SourceTypeId,
                                            SourceObjectId = newBranching.SourceObjectId,
                                            DestinationTypeId = branching.DestinationTypeId,
                                            DestinationObjectId = newBranching.DestinationObjectId,
                                            IsStartingPoint = branching.IsStartingPoint,
                                            IsEndPoint = branching.IsEndPoint
                                        };
                                        UOWObj.BranchingLogicRepository.Insert(branchingObj);

                                        #region insert the coordinates in branchinglogic

                                        var element = coordinatesInBranchingLogic.FirstOrDefault(r => (r.ObjectId == branching.SourceObjectId && r.ObjectTypeId == branching.SourceTypeId) || (r.ObjectId == branching.DestinationObjectId && r.ObjectTypeId == branching.DestinationTypeId));
                                        if (element != null)
                                        {
                                            if (branching.IsStartingPoint)
                                            {
                                                var CoordinatesInBranchingLogic = new Db.CoordinatesInBranchingLogic()
                                                {
                                                    ObjectId = newQuizId,
                                                    ObjectTypeId = (int)BranchingLogicEnum.START,
                                                    XCoordinate = "0",
                                                    YCoordinate = "0",
                                                    CompanyId = quizObj.CompanyId,
                                                    QuizId = newQuizId
                                                };
                                                UOWObj.CoordinatesInBranchingLogicRepository.Insert(CoordinatesInBranchingLogic);
                                            };
                                            var CoordinatesInBranchingLogicObj = new Db.CoordinatesInBranchingLogic()
                                            {
                                                ObjectId = branching.IsStartingPoint ? newBranching.SourceObjectId : (newBranching.DestinationObjectId.HasValue ? newBranching.DestinationObjectId.Value : default(int)),
                                                ObjectTypeId = element.ObjectTypeId,
                                                XCoordinate = element.XCoordinate,
                                                YCoordinate = element.YCoordinate,
                                                //,
                                                CompanyId = quizObj.CompanyId,
                                                QuizId = newQuizId
                                            };

                                            if (!coordinatesInBranchingLogic.Any(r => r.ObjectId == CoordinatesInBranchingLogicObj.ObjectId && r.ObjectTypeId == CoordinatesInBranchingLogicObj.ObjectTypeId))
                                            {
                                                UOWObj.CoordinatesInBranchingLogicRepository.Insert(CoordinatesInBranchingLogicObj);
                                            }

                                            CoordinatesInBranchingLogicObj.Id = coordinatesInBranchingLogic.Count() + 1;

                                            coordinatesInBranchingLogic.Add(CoordinatesInBranchingLogicObj);

                                            #region copy coordinatesInBranchingLogic which is not present in branching logic

                                            if (branching.SourceTypeId == (int)BranchingLogicEnum.ANSWER || branching.SourceTypeId == (int)BranchingLogicEnum.QUESTIONNEXT || branching.SourceTypeId == (int)BranchingLogicEnum.CONTENTNEXT || branching.SourceTypeId == (int)BranchingLogicEnum.RESULTNEXT)
                                            {
                                                var objectId = 0; //object startPoint Id
                                                var objectType = 0; //object startPoint Type

                                                if (branching.SourceTypeId == (int)BranchingLogicEnum.ANSWER)
                                                {
                                                    var obj = publishedQuizDetails.QuestionsInQuiz.FirstOrDefault(r => r.AnswerOptionsInQuizQuestions.Any(a => a.Id == branching.SourceObjectId));
                                                    if (obj != null)
                                                        objectId = obj.Id;
                                                }
                                                else
                                                    objectId = branching.SourceObjectId;

                                                if (branching.SourceTypeId == (int)BranchingLogicEnum.ANSWER || branching.SourceTypeId == (int)BranchingLogicEnum.QUESTIONNEXT)
                                                    objectType = (int)BranchingLogicEnum.QUESTION;
                                                else if (branching.SourceTypeId == (int)BranchingLogicEnum.RESULTNEXT)
                                                    objectType = (int)BranchingLogicEnum.RESULT;
                                                else if (branching.SourceTypeId == (int)BranchingLogicEnum.CONTENTNEXT)
                                                    objectType = (int)BranchingLogicEnum.CONTENT;

                                                if (objectId > 0 && !branchingLogic.Any(r => (r.SourceObjectId == objectId && r.SourceTypeId == objectType) || (r.DestinationObjectId == objectId && r.DestinationTypeId == objectType)))
                                                {
                                                    var coordinate = coordinatesInBranchingLogic.FirstOrDefault(r => r.ObjectId == objectId && r.ObjectTypeId == objectType);
                                                    if (coordinate != null)
                                                    {
                                                        var publishedDetails = mappingLst.FirstOrDefault(a => a.DraftedId == objectId && a.Type == objectType);

                                                        if (publishedDetails != null && !coordinatesInBranchingLogic.Any(r => r.ObjectId == publishedDetails.PublishedId && r.ObjectTypeId == coordinate.ObjectTypeId))
                                                        {
                                                            CoordinatesInBranchingLogicObj = new Db.CoordinatesInBranchingLogic()
                                                            {
                                                                ObjectId = publishedDetails.PublishedId,
                                                                ObjectTypeId = coordinate.ObjectTypeId,
                                                                XCoordinate = coordinate.XCoordinate,
                                                                YCoordinate = coordinate.YCoordinate,
                                                                //,
                                                                CompanyId = quizObj.CompanyId,
                                                                QuizId = newQuizId
                                                            };

                                                            UOWObj.CoordinatesInBranchingLogicRepository.Insert(CoordinatesInBranchingLogicObj);

                                                            CoordinatesInBranchingLogicObj.Id = coordinatesInBranchingLogic.Count() + 1;

                                                            coordinatesInBranchingLogic.Add(CoordinatesInBranchingLogicObj);
                                                        }
                                                    }
                                                }
                                            }
                                            #endregion
                                        }

                                        #endregion
                                    }
                                    #endregion

                                    #region insert usageType in UsageTypeInQuiz

                                    var usageTypeInQuiz = quizObj.UsageTypeInQuiz.Where(r => r.QuizId == quizObj.Id);

                                    if (usageTypeInQuiz != null && usageTypeInQuiz.Any())
                                    {

                                        foreach (var item in usageTypeInQuiz.ToList())
                                        {
                                            var usageTypeObj = new Db.UsageTypeInQuiz();
                                            usageTypeObj.QuizId = newQuizId;
                                            usageTypeObj.UsageType = item.UsageType;
                                            UOWObj.UsageTypeInQuizRepository.Insert(usageTypeObj);
                                            UOWObj.Save();
                                        }
                                    }

                                    #endregion

                                    #region insert into UserPermissionsInQuiz

                                    if (quizObj.QuizType == (int)QuizTypeEnum.AssessmentTemplate || quizObj.QuizType == (int)QuizTypeEnum.ScoreTemplate || quizObj.QuizType == (int)QuizTypeEnum.PersonalityTemplate)
                                    {
                                        if (string.IsNullOrEmpty(UserType))
                                        {
                                            var userPermissionsObj = new Db.UserPermissionsInQuiz()
                                            {
                                                QuizId = newQuizId,
                                                UserTypeId = (int)UserTypeEnum.Lead
                                            };
                                            UOWObj.UserPermissionsInQuizRepository.Insert(userPermissionsObj);
                                            UOWObj.Save();
                                        }
                                        else
                                        {
                                            var UserTypes = UserType.Split(',');

                                            foreach (var userType in UserTypes.Where(a => !string.IsNullOrEmpty(a)).ToList())
                                            {
                                                if (int.Parse(userType) == (int)UserTypeEnum.Lead || int.Parse(userType) == (int)UserTypeEnum.Recruiter || (IsCreateTechnicalRecruiterCourse && int.Parse(userType) == (int)UserTypeEnum.TechnicalRecruiter) || (IsCreateAcademyCourse && int.Parse(userType) == (int)UserTypeEnum.JobRockAcademy))
                                                {
                                                    var userPermissionsObj = new Db.UserPermissionsInQuiz()
                                                    {
                                                        QuizId = newQuizId,
                                                        UserTypeId = int.Parse(userType)
                                                    };
                                                    UOWObj.UserPermissionsInQuizRepository.Insert(userPermissionsObj);
                                                    UOWObj.Save();
                                                }
                                            };

                                            if (IsCreateTechnicalRecruiterCourse && ModuleType.HasValue && ModuleType.Value > 0 && UserTypes.Any(r => !string.IsNullOrEmpty(r) && int.Parse(r) == (int)UserTypeEnum.TechnicalRecruiter))
                                            {
                                                var modulePermissionsObj = new Db.ModulePermissionsInQuiz()
                                                {
                                                    QuizId = newQuizId,
                                                    ModuleTypeId = ModuleType.Value
                                                };
                                                UOWObj.ModulePermissionsInQuizRepository.Insert(modulePermissionsObj);
                                                UOWObj.Save();
                                            };
                                        }
                                    }
                                    else
                                    {
                                        foreach (var userType in quizObj.UserPermissionsInQuiz.ToList())
                                        {
                                            if (userType.UserTypeId == (int)UserTypeEnum.Lead || userType.UserTypeId == (int)UserTypeEnum.Recruiter || (IsCreateTechnicalRecruiterCourse && userType.UserTypeId == (int)UserTypeEnum.TechnicalRecruiter) || (IsCreateAcademyCourse && userType.UserTypeId == (int)UserTypeEnum.JobRockAcademy))
                                            {
                                                var userPermissionsObj = new Db.UserPermissionsInQuiz()
                                                {
                                                    QuizId = newQuizId,
                                                    UserTypeId = userType.UserTypeId
                                                };
                                                UOWObj.UserPermissionsInQuizRepository.Insert(userPermissionsObj);

                                                if (IsCreateTechnicalRecruiterCourse && userType.UserTypeId == (int)UserTypeEnum.TechnicalRecruiter)
                                                {
                                                    foreach (var module in quizObj.ModulePermissionsInQuiz.ToList())
                                                    {
                                                        var modulePermissionsObj = new Db.ModulePermissionsInQuiz()
                                                        {
                                                            QuizId = newQuizId,
                                                            ModuleTypeId = module.ModuleTypeId
                                                        };
                                                        UOWObj.ModulePermissionsInQuizRepository.Insert(modulePermissionsObj);
                                                    }
                                                };

                                                UOWObj.Save();
                                            }
                                        };
                                    }

                                    #endregion

                                }
                                UOWObj.Save();
                                transaction.Complete();

                            }
                        }
                        else
                        {
                            Status = ResultEnum.Error;
                            ErrorMessage = "Quiz not found for the QuizId " + QuizId;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
            return newQuizId;
        }

        public void RemoveQuiz(int QuizId, int BusinessUserId, int CompanyId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizObj = UOWObj.QuizRepository.GetByID(QuizId);

                    if (quizObj != null)
                    {
                        var quizDetailsObj = quizObj.QuizDetails.FirstOrDefault();

                        if (quizDetailsObj != null)
                        {
                            var currentDate = DateTime.UtcNow;

                            #region update Quiz Details

                            quizDetailsObj.Status = (int)StatusEnum.Deleted;
                            quizDetailsObj.LastUpdatedOn = currentDate;
                            quizDetailsObj.LastUpdatedBy = BusinessUserId;

                            #endregion

                            #region update Quiz Results

                            var lstResults = quizDetailsObj.QuizResults.Where(r => r.Status == (int)StatusEnum.Active).ToList();

                            foreach (var item in lstResults)
                            {
                                item.Status = (int)StatusEnum.Deleted;
                            }

                            #endregion

                            #region update Questions

                            var lstQuestions = quizDetailsObj.QuestionsInQuiz.Where(r => r.Status == (int)StatusEnum.Active).ToList();

                            foreach (var question in lstQuestions)
                            {
                                question.Status = (int)StatusEnum.Deleted;

                                #region update AnswerOptions

                                var lstAnswers = question.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active).ToList();

                                foreach (var answer in lstAnswers)
                                {
                                    answer.Status = (int)StatusEnum.Deleted;
                                }

                                #endregion
                            }

                            #endregion

                            #region update Content

                            var lstContents = quizDetailsObj.ContentsInQuiz.Where(r => r.Status == (int)StatusEnum.Active).ToList();

                            foreach (var content in lstContents)
                            {
                                content.Status = (int)StatusEnum.Deleted;
                            }
                            #endregion

                            #region update Action

                            var lstAction = quizDetailsObj.ActionsInQuiz.Where(r => r.Status == (int)StatusEnum.Active).ToList();

                            foreach (var action in lstAction)
                            {
                                action.Status = (int)StatusEnum.Deleted;
                            }
                            #endregion

                            #region update Badge

                            var lstBadge = quizDetailsObj.BadgesInQuiz.Where(r => r.Status == (int)StatusEnum.Active).ToList();

                            foreach (var badge in lstBadge)
                            {
                                badge.Status = (int)StatusEnum.Deleted;
                            }
                            #endregion

                            UOWObj.QuizDetailsRepository.Update(quizDetailsObj);

                            UOWObj.Save();
                            
                            
                            
                            
                            
                            
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz not found for the QuizId " + QuizId;
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public List<QuizResultRedirect> GetQuizRedirectResult(int QuizId)
        {
            List<QuizResultRedirect> QuizResultRedirectList = new List<QuizResultRedirect>();
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizObj = UOWObj.QuizRepository.GetByID(QuizId);

                    if (quizObj != null)
                    {
                        var quizDetailsObj = quizObj.QuizDetails.Where(r => r.State == (int)QuizStateEnum.DRAFTED).FirstOrDefault();

                        if (quizDetailsObj != null)
                        {
                            var quizResults = quizDetailsObj.QuizResults.Where(r => r.Status == (int)StatusEnum.Active);

                            foreach (var item in quizResults)
                            {
                                QuizResultRedirectList.Add(new QuizResultRedirect
                                {
                                    QuizId = QuizId,
                                    ResultId = item.Id,
                                    IsRedirectOn = item.IsRedirectOn,
                                    ResultTitle = item.Title,
                                    RedirectResultTo = item.RedirectResultTo
                                });
                            }
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz not found for the Id " + QuizId;
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
            return QuizResultRedirectList;
        }

        public void UpdateQuizRedirectResult(List<QuizResultRedirect> RedirectResultList, int BusinessUserId, int CompanyId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    if (RedirectResultList != null && RedirectResultList.Any())
                    {
                        var currentDate = DateTime.UtcNow;

                        foreach (var item in RedirectResultList)
                        {
                            var resultObj = UOWObj.QuizResultsRepository.GetByID(item.ResultId);

                            resultObj.Title = item.ResultTitle;
                            resultObj.IsRedirectOn = item.IsRedirectOn;
                            resultObj.RedirectResultTo = item.RedirectResultTo;
                            resultObj.LastUpdatedBy = BusinessUserId;
                            resultObj.LastUpdatedOn = currentDate;

                            resultObj.QuizDetails.LastUpdatedOn = currentDate;
                            resultObj.QuizDetails.LastUpdatedBy = BusinessUserId;

                            resultObj.QuizDetails.Quiz.State = (int)QuizStateEnum.DRAFTED;

                            UOWObj.QuizRepository.Update(resultObj.QuizDetails.Quiz);
                        }

                        UOWObj.Save();
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public void UpdateBranchingLogicState(int QuizId, int BusinessUserId, bool IsEnabled, int CompanyId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizObj = UOWObj.QuizRepository.GetByID(QuizId);

                    if (quizObj != null)
                    {
                        var currentDate = DateTime.UtcNow;

                        var quizDetailsObj = quizObj.QuizDetails.Where(r => r.State == (int)QuizStateEnum.DRAFTED).FirstOrDefault();

                        if (quizDetailsObj != null)
                        {
                            quizDetailsObj.IsBranchingLogicEnabled = IsEnabled;

                            quizDetailsObj.LastUpdatedOn = currentDate;
                            quizDetailsObj.LastUpdatedBy = BusinessUserId;

                            quizDetailsObj.Quiz.State = (int)QuizStateEnum.DRAFTED;

                            UOWObj.QuizRepository.Update(quizDetailsObj.Quiz);

                            UOWObj.Save();

                            
                            
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz not found for the Id " + QuizId;
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public List<QuizVersion> GetQuizListByVersions(int QuizId, long OffsetValue)
        {
            List<QuizVersion> QuizVersionList = new List<QuizVersion>();
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quiz = UOWObj.QuizRepository.GetByID(QuizId);

                    if (quiz != null)
                    {
                        QuizVersion quizVersionObj = new QuizVersion();

                        foreach (var item in quiz.QuizDetails.Where(r => r.State == (int)QuizStateEnum.PUBLISHED).OrderByDescending(r => r.Version))
                        {
                            if (QuizVersionList.Count == 0)
                            {
                                quizVersionObj.PublishedQuizId = item.Id;
                                quizVersionObj.VersionNumber = item.Version;
                                quizVersionObj.PublishedOn = Utility.ConvertUTCDateToLocalDate(item.CreatedOn, OffsetValue);
                                quizVersionObj.UntilDate = Utility.ConvertUTCDateToLocalDate(DateTime.UtcNow, OffsetValue);
                                quizVersionObj.IsCurrent = true;

                                QuizVersionList.Add(quizVersionObj);
                            }
                            else
                            {
                                var nextQuizVersionObj = new QuizVersion();

                                nextQuizVersionObj.PublishedQuizId = item.Id;
                                nextQuizVersionObj.VersionNumber = item.Version;
                                nextQuizVersionObj.PublishedOn = Utility.ConvertUTCDateToLocalDate(item.CreatedOn, OffsetValue);
                                nextQuizVersionObj.UntilDate = quizVersionObj.PublishedOn;
                                nextQuizVersionObj.IsCurrent = false;

                                QuizVersionList.Add(nextQuizVersionObj);

                                quizVersionObj = nextQuizVersionObj;
                            }
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz not found for the QuizId " + QuizId;
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
            return QuizVersionList;
        }

        public QuizAnalyticsOverview GetQuizAnalyticsOverview(int PublishedQuizId)
        {
            QuizAnalyticsOverview quizAnalyticsOverview = new QuizAnalyticsOverview();
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizDetails = UOWObj.QuizDetailsRepository.GetByID(PublishedQuizId);

                    if (quizDetails != null)
                    {
                        quizAnalyticsOverview.PublishedQuizId = PublishedQuizId;

                        int viewCount = 0, startCount = 0, completionCount = 0;

                        var quizAttempts = quizDetails.QuizAttempts.Where(r => r.Mode == "AUDIT");

                        foreach (var item in quizAttempts)
                        {
                            var quizStatsObj = item.QuizStats.FirstOrDefault();

                            viewCount += item.IsViewed ? 1 : 0;
                            startCount += quizStatsObj != null ? 1 : 0;
                            completionCount += quizStatsObj != null && quizStatsObj.CompletedOn.HasValue ? 1 : 0;
                        }

                        quizAnalyticsOverview.Views = viewCount;
                        quizAnalyticsOverview.QuizStarts = startCount;
                        quizAnalyticsOverview.Completion = completionCount;
                        quizAnalyticsOverview.Leads = quizAttempts.Where(r => r.LeadConvertedOn.HasValue && !string.IsNullOrEmpty(r.LeadUserId)).Count();
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz not found for the QuizId " + PublishedQuizId;
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
            return quizAnalyticsOverview;
        }

        public QuizAnalyticsStats GetQuizAnalyticsStats(int PublishedQuizId)
        {
            QuizAnalyticsStats quizAnalyticsStats = new QuizAnalyticsStats();

            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizDetails = UOWObj.QuizDetailsRepository.GetByID(PublishedQuizId);

                    if (quizDetails != null)
                    {
                        var quizAttempts = quizDetails.QuizAttempts.Where(r => r.Mode == "AUDIT");

                        int visitorsCount = 0, completedQuizCount = 0;

                        quizAnalyticsStats.PublishedQuizId = PublishedQuizId;
                        quizAnalyticsStats.StartButtonText = quizDetails.StartButtonText;
                        quizAnalyticsStats.IsBranchingLogicEnabled = quizDetails.IsBranchingLogicEnabled.HasValue ? quizDetails.IsBranchingLogicEnabled.Value : false;
                        quizAnalyticsStats.LeadsCount = quizAttempts.Where(r => r.LeadConvertedOn.HasValue && !string.IsNullOrEmpty(r.LeadUserId)).Count();

                        #region binding question and result object

                        if (!quizAnalyticsStats.IsBranchingLogicEnabled)
                        {
                            quizAnalyticsStats.Questions = new List<QuizAnalyticsStats.QuestionStats>();

                            foreach (var ques in quizDetails.QuestionsInQuiz)
                            {
                                var questionObj = new QuizAnalyticsStats.QuestionStats();

                                questionObj.QuestionId = ques.Id;
                                questionObj.QuestionTitle = ques.Question;

                                questionObj.Answers = new List<QuizAnalyticsStats.QuestionStats.AnswerStats>();

                                foreach (var answer in ques.AnswerOptionsInQuizQuestions.Where(r => !r.IsUnansweredType))
                                {
                                    questionObj.Answers.Add(new QuizAnalyticsStats.QuestionStats.AnswerStats
                                    {
                                        AnswerId = answer.Id,
                                        AnswerTitle = answer.Option
                                    });
                                }

                                quizAnalyticsStats.Questions.Add(questionObj);
                            }
                        }

                        quizAnalyticsStats.Results = new List<QuizAnalyticsStats.ResultStats>();

                        foreach (var res in quizDetails.QuizResults)
                        {
                            var resultObj = new QuizAnalyticsStats.ResultStats();

                            resultObj.ResultId = res.Id;
                            resultObj.ResultTitle = res.Title;

                            quizAnalyticsStats.Results.Add(resultObj);
                        }

                        #endregion

                        foreach (var item in quizAttempts)
                        {
                            var quizStatsObj = item.QuizStats.FirstOrDefault();

                            visitorsCount += quizStatsObj != null ? 1 : 0;

                            if (!quizAnalyticsStats.IsBranchingLogicEnabled)
                            {
                                var quizQuestionStatsList = item.QuizQuestionStats.Where(t => t.Status == (int)StatusEnum.Active && t.QuizAnswerStats.Any(r => !r.AnswerOptionsInQuizQuestions.IsUnansweredType));

                                foreach (var quesStat in quizQuestionStatsList)
                                {
                                    var quesStatObj = quizAnalyticsStats.Questions.FirstOrDefault(r => r.QuestionId == quesStat.QuestionId);
                                    quesStatObj.QuestionAttempts += 1;

                                    var answerStatObj = quesStatObj.Answers.FirstOrDefault(r => r.AnswerId == quesStat.QuizAnswerStats.FirstOrDefault().AnswerId);
                                    answerStatObj.AnswerAttempts += 1;
                                }
                            }

                            var quizStatsList = item.QuizStats;

                            completedQuizCount += quizStatsList.Where(r => r.CompletedOn.HasValue).Count();

                            foreach (var stats in quizStatsList.Where(r => r.ResultId.HasValue))
                            {
                                var resultAnalyticsObj = quizAnalyticsStats.Results.FirstOrDefault(r => r.ResultId == stats.ResultId.Value);

                                if (resultAnalyticsObj != null)
                                {
                                    resultAnalyticsObj.LeadsInResult += 1;
                                }
                                else
                                {
                                    quizAnalyticsStats.Results.Add(new QuizAnalyticsStats.ResultStats
                                    {
                                        ResultId = stats.ResultId.Value,
                                        ResultTitle = stats.QuizResults.Title,
                                        LeadsInResult = 1
                                    });
                                }
                            }
                        }

                        quizAnalyticsStats.CompletedQuizCount = completedQuizCount;
                        quizAnalyticsStats.VisitorsCount = visitorsCount;
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz not found for the QuizId " + PublishedQuizId;
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
            return quizAnalyticsStats;
        }

        public string GetQuizAttemptCode(string PublishedCode, string Mode, int UserTypeId, string UserId, int WorkPackageInfoId, BusinessUser UserInfo, string SourceId = "", string ConfigurationId = null, string CompanyCode = "")
        {
            string quizAttemptCode = string.Empty;
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    Db.ConfigurationDetails ConfigurationDetailsObj = null;
                    if (!string.IsNullOrEmpty(ConfigurationId))
                    {
                        var ConfigurationDetailsList = UOWObj.ConfigurationDetailsRepository.Get(r => r.ConfigurationId == ConfigurationId);
                        if (ConfigurationDetailsList != null && ConfigurationDetailsList.Any())
                        {
                            ConfigurationDetailsObj = ConfigurationDetailsList.FirstOrDefault();
                            PublishedCode = ConfigurationDetailsObj.Quiz.PublishedCode;
                        }
                    }
                    var quizObj = UOWObj.QuizRepository.Get(r => r.PublishedCode == PublishedCode).FirstOrDefault();

                    if (quizObj != null)
                    {
                        if (UserTypeId == (int)UserTypeEnum.Recruiter && UserInfo != null && quizObj.CompanyId != UserInfo.CompanyInfo.Id)
                        {
                            Status = ResultEnum.Error;
                            ErrorMessage = "Quiz not found for the Code " + PublishedCode;
                            return "";
                        }

                        var uniqueCode = Guid.NewGuid().ToString();
                        var currentDate = DateTime.UtcNow;

                        var latestPublishedQuiz = quizObj.QuizDetails.OrderByDescending(r => r.Version).FirstOrDefault(r => r.Status == (int)StatusEnum.Active && r.State == (Mode == "PREVIEW" ? (int)QuizStateEnum.DRAFTED : (int)QuizStateEnum.PUBLISHED));

                        if (latestPublishedQuiz != null)
                        {
                            int companyId = 0;
                            if (!string.IsNullOrEmpty(CompanyCode))
                            {
                                var companyObj = UOWObj.CompanyRepository.Get(r => r.ClientCode.Equals(CompanyCode, StringComparison.OrdinalIgnoreCase));
                                if (companyObj != null && companyObj.Any())
                                {
                                    companyId = companyObj.FirstOrDefault().Id;
                                }
                            }

                            Db.QuizAttempts quizAttemptsObj = null;

                            if ((!string.IsNullOrEmpty(UserId) || UserInfo != null) && (UserTypeId == (int)UserTypeEnum.JobRockAcademy || UserTypeId == (int)UserTypeEnum.TechnicalRecruiter || UserTypeId == (int)UserTypeEnum.Recruiter || UserTypeId == (int)UserTypeEnum.Lead))
                            {
                                if (quizObj.UserPermissionsInQuiz.FirstOrDefault() != null && (quizObj.UserPermissionsInQuiz.FirstOrDefault(a => a.UserTypeId == UserTypeId) != null))
                                {
                                    if (!string.IsNullOrEmpty(UserId))
                                    {
                                        if (UserTypeId == (int)UserTypeEnum.Recruiter || UserTypeId == (int)UserTypeEnum.JobRockAcademy || UserTypeId == (int)UserTypeEnum.TechnicalRecruiter)
                                        {
                                            var recruiterUserId = Convert.ToInt32(UserId);
                                            quizAttemptsObj = UOWObj.QuizAttemptsRepository.Get(r => r.QuizId == latestPublishedQuiz.Id && r.RecruiterUserId == recruiterUserId).LastOrDefault();
                                        }

                                        if (UserTypeId == (int)UserTypeEnum.Lead)
                                            quizAttemptsObj = UOWObj.QuizAttemptsRepository.Get(r => r.WorkPackageInfoId == WorkPackageInfoId && r.LeadUserId == UserId).LastOrDefault();

                                    }

                                    if (quizAttemptsObj == null || (UserTypeId != (int)UserTypeEnum.Lead && quizAttemptsObj != null && quizAttemptsObj.QuizStats.FirstOrDefault() != null && quizAttemptsObj.QuizStats.FirstOrDefault().CompletedOn.HasValue))
                                    {
                                        quizAttemptsObj = new Db.QuizAttempts();

                                        quizAttemptsObj.QuizId = latestPublishedQuiz.Id;
                                        quizAttemptsObj.Date = currentDate;
                                        quizAttemptsObj.Code = uniqueCode;
                                        quizAttemptsObj.CreatedOn = currentDate;
                                        quizAttemptsObj.LastUpdatedOn = currentDate;
                                        if (WorkPackageInfoId > 0)
                                            quizAttemptsObj.WorkPackageInfoId = WorkPackageInfoId;

                                        if (UserTypeId == (int)UserTypeEnum.Recruiter || UserTypeId == (int)UserTypeEnum.JobRockAcademy || UserTypeId == (int)UserTypeEnum.TechnicalRecruiter)
                                        {
                                            var recruiterUserId = 0;
                                            if (!string.IsNullOrEmpty(UserId))
                                                recruiterUserId = Convert.ToInt32(UserId);
                                            else
                                                recruiterUserId = UserInfo.BusinessUserId;
                                            quizAttemptsObj.RecruiterUserId = recruiterUserId;
                                        }

                                        if (UserTypeId == (int)UserTypeEnum.Lead)
                                            quizAttemptsObj.LeadUserId = UserId;

                                        quizAttemptsObj.IsViewed = false;
                                        quizAttemptsObj.Mode = Mode;
                                        quizAttemptsObj.CompanyId = quizObj.CompanyId;
                                        quizAttemptsObj.SourceId = !string.IsNullOrWhiteSpace(SourceId) ? SourceId : null;
                                        quizAttemptCode = quizAttemptsObj.Code; 
                                        
                                        UOWObj.QuizAttemptsRepository.Insert(quizAttemptsObj);
                                        UOWObj.Save();
                                    }
                                }
                                else
                                {
                                    Status = ResultEnum.Error;

                                    if (UserTypeId == (int)UserTypeEnum.Recruiter || UserTypeId == (int)UserTypeEnum.JobRockAcademy || UserTypeId == (int)UserTypeEnum.TechnicalRecruiter)
                                        ErrorMessage = "Quiz not Permitted to Recruiter";

                                    if (UserTypeId == (int)UserTypeEnum.Lead)
                                        ErrorMessage = "Quiz not Permitted to Lead ";

                                    return "";
                                }
                            }

                            else
                            {
                                quizAttemptsObj = new Db.QuizAttempts();

                                quizAttemptsObj.QuizId = latestPublishedQuiz.Id;
                                quizAttemptsObj.Date = currentDate;
                                quizAttemptsObj.Code = uniqueCode;
                                quizAttemptsObj.CreatedOn = currentDate;
                                quizAttemptsObj.LastUpdatedOn = currentDate;
                                quizAttemptsObj.IsViewed = false;
                                quizAttemptsObj.Mode = Mode;
                                quizAttemptsObj.SourceId = !string.IsNullOrWhiteSpace(SourceId) ? SourceId : null;
                                quizAttemptCode = quizAttemptsObj.Code;
                                quizAttemptsObj.CompanyId = UserInfo == null ? (!string.IsNullOrEmpty(CompanyCode) ? companyId : default(int?)) : UserInfo.CompanyInfo.Id;
                                UOWObj.QuizAttemptsRepository.Insert(quizAttemptsObj);
                                UOWObj.Save();
                            }

                            quizAttemptCode = quizAttemptsObj != null ? quizAttemptsObj.Code : string.Empty;

                            var configurationDetailsObj = new Db.ConfigurationDetails();

                            #region ConfigurationDetails

                            if (!string.IsNullOrEmpty(ConfigurationId))
                            {
                                if (ConfigurationDetailsObj != null)
                                {
                                    quizAttemptsObj.ConfigurationDetailsId = ConfigurationDetailsObj.Id;
                                    quizAttemptsObj.CompanyId = quizObj.CompanyId;
                                    UOWObj.QuizAttemptsRepository.Update(quizAttemptsObj);
                                    UOWObj.Save();
                                }
                            }
                            else if (WorkPackageInfoId > 0)
                            {
                                var WorkPackageInfoObj = UOWObj.WorkPackageInfoRepository.GetByID(WorkPackageInfoId);
                                if (WorkPackageInfoObj != null && WorkPackageInfoObj.ConfigurationDetailsId > 0)
                                {
                                    quizAttemptsObj.ConfigurationDetailsId = WorkPackageInfoObj.ConfigurationDetailsId;
                                    quizAttemptsObj.CompanyId = quizObj.CompanyId;
                                    UOWObj.QuizAttemptsRepository.Update(quizAttemptsObj);
                                    UOWObj.Save();
                                }
                            }
                            #endregion
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz not found for the Code " + PublishedCode;
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
            return quizAttemptCode;
        }

        public void UpdateQuizCompleteStatus(string QuizCode, string LeadUserId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizAttemptsObj = UOWObj.QuizAttemptsRepository.Get(r => r.Code == QuizCode).FirstOrDefault();

                    if (quizAttemptsObj != null)
                    {
                        var currentDate = DateTime.UtcNow;
                        var quizStatsObj = quizAttemptsObj.QuizStats.FirstOrDefault();

                        if (quizStatsObj != null)
                        {
                            quizStatsObj.CompletedOn = currentDate;

                            quizAttemptsObj.LeadUserId = LeadUserId;
                            quizAttemptsObj.LeadConvertedOn = currentDate;
                            quizAttemptsObj.CompanyId = quizAttemptsObj.QuizDetails.CompanyId;

                            UOWObj.QuizAttemptsRepository.Update(quizAttemptsObj);
                            UOWObj.Save();
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz not found for the QuizCode " + QuizCode;
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public List<QuizLeadCollectionStats> GetQuizLeadCollectionStats(int PublishedQuizId)
        {
            List<QuizLeadCollectionStats> statsList = new List<QuizLeadCollectionStats>();

            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizDetails = UOWObj.QuizDetailsRepository.GetByID(PublishedQuizId);

                    if (quizDetails != null)
                    {
                        CompanyModel companyObj;
                        if (quizDetails.Quiz.Company != null)
                        {
                            companyObj = new CompanyModel
                            {
                                Id = quizDetails.Quiz.Company.Id,
                                AlternateClientCodes = quizDetails.Quiz.Company.AlternateClientCodes,
                                ClientCode = quizDetails.Quiz.Company.ClientCode,
                                CompanyName = quizDetails.Quiz.Company.CompanyName,
                                CompanyWebsiteUrl = quizDetails.Quiz.Company.CompanyWebsiteUrl,
                                JobRocketApiAuthorizationBearer = quizDetails.Quiz.Company.JobRocketApiAuthorizationBearer,
                                JobRocketApiUrl = quizDetails.Quiz.Company.JobRocketApiUrl,
                                JobRocketClientUrl = quizDetails.Quiz.Company.JobRocketClientUrl,
                                LeadDashboardApiAuthorizationBearer = quizDetails.Quiz.Company.LeadDashboardApiAuthorizationBearer,
                                LeadDashboardApiUrl = quizDetails.Quiz.Company.LeadDashboardApiUrl,
                                LeadDashboardClientUrl = quizDetails.Quiz.Company.LeadDashboardClientUrl,
                                LogoUrl = quizDetails.Quiz.Company.LogoUrl,
                                PrimaryBrandingColor = quizDetails.Quiz.Company.PrimaryBrandingColor,
                                SecondaryBrandingColor = quizDetails.Quiz.Company.SecondaryBrandingColor

                            };
                        }
                        else
                        {
                            companyObj = new CompanyModel();
                        }
                        foreach (var item in quizDetails.QuizAttempts.Where(r => r.LeadConvertedOn.HasValue && !string.IsNullOrEmpty(r.LeadUserId)))
                        {
                            var leadUserInfo = OWCHelper.GetLeadUserInfo(item.LeadUserId, companyObj);

                            if (leadUserInfo != null && !string.IsNullOrEmpty(leadUserInfo.contactId))
                            {
                                statsList.Add(new QuizLeadCollectionStats
                                {
                                    LeadUserName = leadUserInfo.firstName + " " + (leadUserInfo.lastName ?? string.Empty),
                                    LeadUserEmail = leadUserInfo.email,
                                    LeadUserPhone = leadUserInfo.telephone,
                                    AddedOn = item.LastUpdatedOn.Value
                                });
                            }
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz not found for the QuizId " + PublishedQuizId;
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
            return statsList;
        }

        public QuizAction AddActionInQuiz(int QuizId, int BusinessUserId, int CompanyId)
        {
            try
            {
                QuizAction quizAction = null;
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizObj = UOWObj.QuizRepository.GetByID(QuizId);

                    if (quizObj != null)
                    {
                        var quizDetails = quizObj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED);
                        if (quizDetails != null)
                        {
                            var quizActionsObj = new Db.ActionsInQuiz();
                            var currentDate = DateTime.UtcNow;
                            quizActionsObj.Title = "Action " + (quizDetails.ActionsInQuiz.Where(r => r.State == (int)StatusEnum.Active).Count() + 1).ToString();
                            quizActionsObj.LastUpdatedBy = BusinessUserId;
                            quizActionsObj.LastUpdatedOn = currentDate;
                            quizActionsObj.ReportEmails = string.Empty;
                            quizActionsObj.AppointmentId = 0;
                            quizActionsObj.QuizId = quizDetails.Id;
                            quizActionsObj.ActionType = (int)ActionTypeEnum.Appointment;
                            quizActionsObj.Status = (int)StatusEnum.Active;
                            quizActionsObj.State = (int)QuizStateEnum.DRAFTED;
                            UOWObj.ActionsInQuizRepository.Insert(quizActionsObj);
                            UOWObj.Save();
                            quizAction = new QuizAction()
                            {
                                Id = quizActionsObj.Id,
                                AppointmentId = quizActionsObj.AppointmentId ?? 0,
                                Title = quizActionsObj.Title,
                                ReportEmails = quizActionsObj.ReportEmails ?? string.Empty,
                                ActionType = quizActionsObj.ActionType
                            };

                            
                            
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz not found for the QuizId " + QuizId;
                    }
                }
                return quizAction;
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public void UpdateActionInQuiz(QuizAction Actionbj, int BusinessUserId, int CompanyId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizActionsObj = UOWObj.ActionsInQuizRepository.Get(r => r.Id == Actionbj.Id && r.Status == (int)StatusEnum.Active).FirstOrDefault();

                    if (quizActionsObj != null)
                    {
                        var currentDate = DateTime.UtcNow;

                        SaveDynamicVariable(quizActionsObj.Title, string.IsNullOrEmpty(Actionbj.Title) ? quizActionsObj.Title : Actionbj.Title, quizActionsObj.QuizDetails.Id);

                        foreach (var linkedCalendarInActionObj in quizActionsObj.LinkedCalendarInAction.ToList())
                            UOWObj.LinkedCalendarInActionRepository.Delete(linkedCalendarInActionObj);

                        if (Actionbj.ActionType == (int)ActionTypeEnum.Appointment)
                        {

                            quizActionsObj.AppointmentId = Actionbj.AppointmentId;
                            quizActionsObj.ReportEmails = string.Empty;
                            quizActionsObj.AutomationId = null;

                            if (Actionbj.CalendarIds != null)
                            {
                                foreach (var calendarId in Actionbj.CalendarIds)
                                {
                                    UOWObj.LinkedCalendarInActionRepository.Insert(new Db.LinkedCalendarInAction()
                                    {
                                        CalendarId = calendarId,
                                        ActionsInQuizId = quizActionsObj.Id
                                    });
                                }
                            }
                        }
                        else if (Actionbj.ActionType == (int)ActionTypeEnum.ReportEmail)
                        {
                            quizActionsObj.ReportEmails = Actionbj.ReportEmails;
                            quizActionsObj.AppointmentId = 0;
                            quizActionsObj.AutomationId = null;
                        }
                        else if (Actionbj.ActionType == (int)ActionTypeEnum.Automation)
                        {
                            quizActionsObj.AutomationId = Actionbj.AutomationId;
                            quizActionsObj.AppointmentId = 0;
                            quizActionsObj.ReportEmails = string.Empty;
                        }
                        else if (Actionbj.ActionType == (int)ActionTypeEnum.LinkWithLeadDashboardAppointment)
                        {
                            quizActionsObj.AutomationId = null;
                            quizActionsObj.AppointmentId = 0;
                            quizActionsObj.ReportEmails = string.Empty;
                        }
                        if (!string.IsNullOrEmpty(Actionbj.Title))
                            quizActionsObj.Title = Actionbj.Title;
                        quizActionsObj.LastUpdatedBy = BusinessUserId;
                        quizActionsObj.LastUpdatedOn = currentDate;
                        quizActionsObj.ActionType = Actionbj.ActionType;

                        UOWObj.ActionsInQuizRepository.Update(quizActionsObj);
                        UOWObj.Save();

                        
                        
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Action not found for the ActionId " + Actionbj.Id;
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public QuizAction GetQuizAction(int Id)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizActionsObj = UOWObj.ActionsInQuizRepository.Get(r => r.Id == Id && r.Status == (int)StatusEnum.Active).FirstOrDefault();
                    var currentDate = DateTime.UtcNow;

                    if (quizActionsObj != null)
                    {
                        return new QuizAction()
                        {
                            AppointmentId = quizActionsObj.AppointmentId,
                            Id = quizActionsObj.Id,
                            ReportEmails = quizActionsObj.ReportEmails,
                            AutomationId = quizActionsObj.AutomationId,
                            QuizId = quizActionsObj.QuizId,
                            Title = quizActionsObj.Title,
                            ActionType = quizActionsObj.ActionType,
                            CalendarIds = (quizActionsObj.ActionType == (int)ActionTypeEnum.Appointment && quizActionsObj.LinkedCalendarInAction.Any()) ? quizActionsObj.LinkedCalendarInAction.Select(t => t.CalendarId).ToList() : new List<int>() { }
                        };
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Action not found for the Id " + Id;
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public void RemoveActionInQuiz(int Id, int BusinessUserId, int CompanyId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizActionsObj = UOWObj.ActionsInQuizRepository.Get(r => r.Id == Id && r.Status == (int)StatusEnum.Active).FirstOrDefault();
                    var currentDate = DateTime.UtcNow;

                    if (quizActionsObj != null)
                    {
                        SaveDynamicVariable(quizActionsObj.Title, string.Empty, quizActionsObj.QuizDetails.Id);

                        quizActionsObj.Status = (int)StatusEnum.Deleted;
                        quizActionsObj.LastUpdatedBy = BusinessUserId;
                        quizActionsObj.LastUpdatedOn = currentDate;

                        UOWObj.ActionsInQuizRepository.Update(quizActionsObj);

                        var branchingLogic = UOWObj.BranchingLogicRepository.Get(r => r.DestinationObjectId == Id && r.DestinationTypeId == (int)BranchingLogicEnum.ACTION);

                        if (branchingLogic.Any())
                        {
                            foreach (var obj in branchingLogic)
                            {
                                UOWObj.BranchingLogicRepository.Delete(obj);
                            }
                        }
                        UOWObj.Save();

                        
                        
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Action not found for the Id " + Id;
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public QuizContent AddContentInQuiz(int QuizId, int BusinessUserId, int CompanyId)
        {
            try
            {
                QuizContent quizContent = null;
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizObj = UOWObj.QuizRepository.GetByID(QuizId);

                    if (quizObj != null)
                    {
                        var quizDetails = UOWObj.QuizDetailsRepository.GetQuizDetailsbyParentQuizIdRepositoryExtension(QuizId).FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED);
                        if (quizDetails != null)
                        {
                            var contentsInQuiz = UOWObj.ContentsInQuizRepository.GetContentInQuizRepositoryExtension(QuizId).Where(r => r.State == (int)StatusEnum.Active);
                            var questionInQuiz = UOWObj.QuestionsInQuizRepository.GetExistingQuestioninQuiz(QuizId).Where(r => r.State == (int)StatusEnum.Active);

                            var contentsInQuizCount = contentsInQuiz.Count();
                            var questionInQuizCount = questionInQuiz.Count();

                            var quizContentObj = new Db.ContentsInQuiz()
                            {
                                ContentTitle = "Content " + (contentsInQuizCount + 1).ToString(),
                                ShowTitle = true,
                                ContentTitleImage = string.Empty,
                                PublicIdForContentTitle = string.Empty,
                                ContentDescription = "Description",
                                ShowDescription = true,
                                ContentDescriptionImage = string.Empty,
                                PublicIdForContentDescription = string.Empty,
                                ShowContentTitleImage = true,
                                ShowContentDescriptionImage = false,
                                LastUpdatedBy = BusinessUserId,
                                LastUpdatedOn = DateTime.UtcNow,
                                QuizId = quizDetails.Id,
                                Status = (int)StatusEnum.Active,
                                State = (int)QuizStateEnum.DRAFTED,
                                AliasTextForNextButton = "Next",
                                EnableNextButton = true,
                                ViewPreviousQuestion = quizDetails.ViewPreviousQuestion,
                                AutoPlay = true,
                                SecondsToApply = "0",
                                VideoFrameEnabled = false,
                                AutoPlayForDescription = true,
                                SecondsToApplyForDescription = "0",
                                DescVideoFrameEnabled = false,
                                DisplayOrderForTitleImage = 2,
                                DisplayOrderForTitle = 1,
                                DisplayOrderForDescriptionImage = 4,
                                DisplayOrderForDescription = 3,
                                DisplayOrderForNextButton = 5

                            };


                            switch (questionInQuizCount + contentsInQuizCount)
                            {
                                case 0:
                                    quizContentObj.DisplayOrder = 1;
                                    break;
                                default:
                                    if (questionInQuizCount != 0 && contentsInQuizCount != 0)
                                        quizContentObj.DisplayOrder = (questionInQuiz.Max(r => r.DisplayOrder) > contentsInQuiz.Max(r => r.DisplayOrder) ? questionInQuiz.Max(r => r.DisplayOrder) + 1 : contentsInQuiz.Max(r => r.DisplayOrder) + 1);
                                    else if (questionInQuizCount != 0)
                                        quizContentObj.DisplayOrder = (questionInQuiz.Max(r => r.DisplayOrder) + 1);
                                    else if (contentsInQuizCount != 0)
                                        quizContentObj.DisplayOrder = (contentsInQuiz.Max(r => r.DisplayOrder) + 1);
                                    break;
                            }

                            UOWObj.ContentsInQuizRepository.Insert(quizContentObj);
                            UOWObj.Save();
                            quizContent = new QuizContent()
                            {
                                Id = quizContentObj.Id,
                                ContentTitle = quizContentObj.ContentTitle,
                                ContentTitleImage = quizContentObj.ContentTitleImage,
                                PublicIdForContentTitle = quizContentObj.PublicIdForContentTitle,
                                ShowContentTitleImage = quizContentObj.ShowContentTitleImage,
                                ContentDescription = quizContentObj.ContentDescription,
                                ContentDescriptionImage = quizContentObj.ContentDescriptionImage,
                                PublicIdForContentDescription = quizContentObj.PublicIdForContentDescription,
                                ShowContentDescriptionImage = quizContentObj.ShowContentDescriptionImage,
                                AliasTextForNextButton = quizContentObj.AliasTextForNextButton,
                                EnableNextButton = quizContentObj.EnableNextButton,
                                DisplayOrderForTitle = quizContentObj.DisplayOrderForTitle,
                                DisplayOrderForTitleImage = quizContentObj.DisplayOrderForTitleImage,
                                DisplayOrderForDescription = quizContentObj.DisplayOrderForDescription,
                                DisplayOrderForDescriptionImage = quizContentObj.DisplayOrderForDescriptionImage,
                                DisplayOrderForNextButton = quizContentObj.DisplayOrderForNextButton,
                                AutoPlay = quizContentObj.AutoPlay,
                                SecondsToApply = quizContentObj.SecondsToApply,
                                VideoFrameEnabled = quizContentObj.VideoFrameEnabled,
                                AutoPlayForDescription = quizContentObj.AutoPlayForDescription,
                                SecondsToApplyForDescription = quizContentObj.SecondsToApplyForDescription,
                                DescVideoFrameEnabled = quizContentObj.DescVideoFrameEnabled
                            };

                            
                            
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz not found for the QuizId " + QuizId;
                    }
                }
                return quizContent;
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public void UpdateContentInQuiz(QuizContent ContentObj, int BusinessUserId, int CompanyId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    if (!string.IsNullOrEmpty(ContentObj.ContentTitle))
                    {
                        var quizContentObj = UOWObj.ContentsInQuizRepository.Get(r => r.Id == ContentObj.Id && r.Status == (int)StatusEnum.Active).FirstOrDefault();

                        if (quizContentObj != null && quizContentObj.QuizDetails.ParentQuizId == ContentObj.QuizId)
                        {
                            var currentDate = DateTime.UtcNow;

                            SaveDynamicVariable(quizContentObj.ContentTitle, ContentObj.ContentTitle, quizContentObj.QuizDetails.Id);
                            SaveDynamicVariable(quizContentObj.ContentDescription, ContentObj.ContentDescription, quizContentObj.QuizDetails.Id);

                            UpdateQuizContent(ContentObj, BusinessUserId, quizContentObj, currentDate);

                            UOWObj.ContentsInQuizRepository.Update(quizContentObj);
                            UOWObj.Save();

                            
                            
                        }
                        else
                        {
                            Status = ResultEnum.Error;
                            ErrorMessage = "Content not found for the ContentId " + ContentObj.Id;
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Content title is required";
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        private void UpdateQuizContent(QuizContent ContentObj, int BusinessUserId, Db.ContentsInQuiz quizContentObj, DateTime currentDate)
        {
            quizContentObj.ContentTitle = ContentObj.ContentTitle;
            quizContentObj.ShowTitle = ContentObj.ShowTitle;
            quizContentObj.ContentTitleImage = ContentObj.ContentTitleImage;
            quizContentObj.EnableMediaFileForTitle = ContentObj.EnableMediaFileForTitle;
            quizContentObj.PublicIdForContentTitle = ContentObj.PublicIdForContentTitle;
            quizContentObj.ShowContentTitleImage = ContentObj.ShowContentTitleImage;
            quizContentObj.ContentDescription = ContentObj.ContentDescription;
            quizContentObj.ShowDescription = ContentObj.ShowDescription;
            quizContentObj.ContentDescriptionImage = ContentObj.ContentDescriptionImage;
            quizContentObj.EnableMediaFileForDescription = ContentObj.EnableMediaFileForDescription;
            quizContentObj.PublicIdForContentDescription = ContentObj.PublicIdForContentDescription;
            quizContentObj.ShowContentDescriptionImage = ContentObj.ShowContentDescriptionImage;
            quizContentObj.AliasTextForNextButton = ContentObj.AliasTextForNextButton;
            quizContentObj.EnableNextButton = ContentObj.EnableNextButton;
            quizContentObj.AutoPlay = ContentObj.AutoPlay;
            quizContentObj.SecondsToApply = ContentObj.SecondsToApply;
            quizContentObj.VideoFrameEnabled = ContentObj.VideoFrameEnabled;
            quizContentObj.AutoPlayForDescription = ContentObj.AutoPlayForDescription;
            quizContentObj.SecondsToApplyForDescription = ContentObj.SecondsToApplyForDescription;
            quizContentObj.DescVideoFrameEnabled = ContentObj.DescVideoFrameEnabled;
            quizContentObj.DisplayOrderForTitle = ContentObj.DisplayOrderForTitle;
            quizContentObj.DisplayOrderForTitleImage = ContentObj.DisplayOrderForTitleImage;
            quizContentObj.DisplayOrderForDescription = ContentObj.DisplayOrderForDescription;
            quizContentObj.DisplayOrderForDescriptionImage = ContentObj.DisplayOrderForDescriptionImage;
            quizContentObj.DisplayOrderForNextButton = ContentObj.DisplayOrderForNextButton;
            quizContentObj.LastUpdatedBy = BusinessUserId;
            quizContentObj.LastUpdatedOn = currentDate;
        }

        public QuizContent GetQuizContent(int Id, int QuizId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizContentObj = UOWObj.ContentsInQuizRepository.Get(r => r.Id == Id && r.Status == (int)StatusEnum.Active).FirstOrDefault();
                    var currentDate = DateTime.UtcNow;

                    if (quizContentObj != null && quizContentObj.QuizDetails.ParentQuizId == QuizId)
                    {
                        return new QuizContent()
                        {
                            Id = quizContentObj.Id,
                            ContentTitle = quizContentObj.ContentTitle,
                            ShowTitle = quizContentObj.ShowTitle,
                            ContentTitleImage = quizContentObj.ContentTitleImage,
                            EnableMediaFileForTitle = quizContentObj.EnableMediaFileForTitle,
                            PublicIdForContentTitle = quizContentObj.PublicIdForContentTitle,
                            ShowContentTitleImage = quizContentObj.ShowContentTitleImage,
                            ContentDescription = quizContentObj.ContentDescription,
                            ShowDescription = quizContentObj.ShowDescription,
                            EnableMediaFileForDescription = quizContentObj.EnableMediaFileForDescription,
                            ContentDescriptionImage = quizContentObj.ContentDescriptionImage,
                            ShowContentDescriptionImage = quizContentObj.ShowContentDescriptionImage,
                            PublicIdForContentDescription = quizContentObj.PublicIdForContentDescription,
                            ViewPreviousQuestion = quizContentObj.ViewPreviousQuestion,
                            AliasTextForNextButton = quizContentObj.AliasTextForNextButton,
                            EnableNextButton = quizContentObj.EnableNextButton,
                            AutoPlay = quizContentObj.AutoPlay,
                            SecondsToApply = quizContentObj.SecondsToApply,
                            VideoFrameEnabled = quizContentObj.VideoFrameEnabled,
                            AutoPlayForDescription = quizContentObj.AutoPlayForDescription,
                            SecondsToApplyForDescription = quizContentObj.SecondsToApplyForDescription,
                            DescVideoFrameEnabled = quizContentObj.DescVideoFrameEnabled,
                            DisplayOrderForTitle = quizContentObj.DisplayOrderForTitle,
                            DisplayOrderForTitleImage = quizContentObj.DisplayOrderForTitleImage,
                            DisplayOrderForDescription = quizContentObj.DisplayOrderForDescription,
                            DisplayOrderForDescriptionImage = quizContentObj.DisplayOrderForDescriptionImage,
                            DisplayOrderForNextButton = quizContentObj.DisplayOrderForNextButton
                        };
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "COntent not found for the Id " + Id;
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public void RemoveContentInQuiz(int Id, int BusinessUserId, int CompanyId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizContentObj = UOWObj.ContentsInQuizRepository.Get(r => r.Id == Id && r.Status == (int)StatusEnum.Active).FirstOrDefault();
                    var currentDate = DateTime.UtcNow;

                    if (quizContentObj != null)
                    {
                        SaveDynamicVariable(quizContentObj.ContentTitle, string.Empty, quizContentObj.QuizDetails.Id);
                        SaveDynamicVariable(quizContentObj.ContentDescription, string.Empty, quizContentObj.QuizDetails.Id);

                        quizContentObj.Status = (int)StatusEnum.Deleted;
                        quizContentObj.LastUpdatedBy = BusinessUserId;
                        quizContentObj.LastUpdatedOn = currentDate;

                        UOWObj.ContentsInQuizRepository.Update(quizContentObj);

                        var branchingLogic = UOWObj.BranchingLogicRepository.Get(r => (r.SourceObjectId == Id && r.SourceTypeId == (int)BranchingLogicEnum.CONTENTNEXT) || (r.SourceObjectId == Id && r.SourceTypeId == (int)BranchingLogicEnum.CONTENT) || (r.DestinationObjectId == Id && r.DestinationTypeId == (int)BranchingLogicEnum.CONTENT));

                        if (branchingLogic.Any())
                        {
                            foreach (var obj in branchingLogic)
                            {
                                UOWObj.BranchingLogicRepository.Delete(obj);
                            }
                        }
                        UOWObj.Save();

                        
                        
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Content not found for the Id " + Id;
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public QuizBranchingLogicLinksList GetQuizBranchingLogicData(int QuizId)
        {
            var QuizBranchingLogicLinksLstObj = new QuizBranchingLogicLinksList();
            QuizBranchingLogicLinksLstObj.QuizBranchingLogicLinks = new List<QuizBranchingLogicLinks>();
            QuizBranchingLogicLinks quizBranchingLogic = null;

            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizObj = UOWObj.QuizRepository.GetByID(QuizId);

                    if (quizObj != null)
                    {
                        var quizDetailsObj = quizObj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED);

                        if (quizDetailsObj != null)
                        {
                            if (quizDetailsObj.IsBranchingLogicEnabled.HasValue && quizDetailsObj.IsBranchingLogicEnabled.Value)
                            {
                                foreach (var branchingLogic in quizDetailsObj.BranchingLogic)
                                {
                                    if (branchingLogic.IsStartingPoint)
                                    {
                                        quizBranchingLogic = new QuizBranchingLogicLinks()
                                        {
                                            ObjectType = BranchingLogicEnum.START,
                                            ObjectTypeId = "start",
                                            Links = new List<BranchingLinks>()
                                        };
                                        var link = new BranchingLinks()
                                        {
                                            FromId = "start",
                                            FromType = BranchingLogicEnum.START,
                                            ToId = branchingLogic.SourceTypeId == (int)BranchingLogicEnum.QUESTION ? "q_" + branchingLogic.SourceObjectId.ToString() : branchingLogic.SourceTypeId == (int)BranchingLogicEnum.ANSWER ? "a_" + branchingLogic.SourceObjectId.ToString() : (branchingLogic.SourceTypeId == (int)BranchingLogicEnum.CONTENT) ? "c_" + branchingLogic.SourceObjectId.ToString() : (branchingLogic.SourceTypeId == (int)BranchingLogicEnum.CONTENTNEXT) ? "cb_" + branchingLogic.SourceObjectId.ToString() : (branchingLogic.SourceTypeId == (int)BranchingLogicEnum.RESULT) ? "r_" + branchingLogic.SourceObjectId.ToString() : (branchingLogic.SourceTypeId == (int)BranchingLogicEnum.RESULTNEXT) ? "rb_" + branchingLogic.SourceObjectId.ToString() : (branchingLogic.SourceTypeId == (int)BranchingLogicEnum.ACTION) ? "ac_" + branchingLogic.SourceObjectId.ToString() : (branchingLogic.SourceTypeId == (int)BranchingLogicEnum.ACTIONNEXT) ? "acb_" + branchingLogic.SourceObjectId.ToString() : (branchingLogic.SourceTypeId == (int)BranchingLogicEnum.BADGE) ? "b_" + branchingLogic.SourceObjectId.ToString() : (branchingLogic.SourceTypeId == (int)BranchingLogicEnum.BADGENEXT) ? "bb_" + branchingLogic.SourceObjectId.ToString() : string.Empty,
                                            ToType = (BranchingLogicEnum)branchingLogic.SourceTypeId
                                        };


                                        var CoordinatesInBranchingLogicObj = UOWObj.CoordinatesInBranchingLogicRepository.Get(a => a.ObjectId == QuizId && a.ObjectTypeId == (int)quizBranchingLogic.ObjectType).FirstOrDefault();
                                        if (CoordinatesInBranchingLogicObj != null)
                                        {
                                            quizBranchingLogic.Position = new string[] { CoordinatesInBranchingLogicObj.XCoordinate, CoordinatesInBranchingLogicObj.YCoordinate };
                                        }
                                        quizBranchingLogic.Links.Add(link);
                                        QuizBranchingLogicLinksLstObj.QuizBranchingLogicLinks.Add(quizBranchingLogic);
                                    }

                                    if (branchingLogic.DestinationObjectId > 0 && (!QuizBranchingLogicLinksLstObj.QuizBranchingLogicLinks.Any(a => a.ObjectTypeId == branchingLogic.DestinationObjectId.ToString() && a.ObjectType == (BranchingLogicEnum)branchingLogic.DestinationTypeId)) && !quizDetailsObj.BranchingLogic.Any(a => a.SourceObjectId == branchingLogic.DestinationObjectId && a.SourceTypeId == branchingLogic.DestinationTypeId))
                                    {
                                        quizBranchingLogic = new QuizBranchingLogicLinks()
                                        {
                                            ObjectType = (BranchingLogicEnum)branchingLogic.DestinationTypeId,
                                            ObjectTypeId = branchingLogic.DestinationObjectId.ToString(),
                                            Links = new List<BranchingLinks>()
                                        };
                                        var link = new BranchingLinks()
                                        {
                                            FromId = string.Empty,
                                            FromType = BranchingLogicEnum.NONE,
                                            ToId = string.Empty,
                                            ToType = BranchingLogicEnum.NONE
                                        };
                                        var quizBranchingLogicObj = QuizBranchingLogicLinksLstObj.QuizBranchingLogicLinks.FirstOrDefault(a => a.ObjectTypeId == quizBranchingLogic.ObjectTypeId && a.ObjectType == quizBranchingLogic.ObjectType);
                                        if (quizBranchingLogicObj != null)
                                        {
                                            quizBranchingLogic = quizBranchingLogicObj;
                                            quizBranchingLogic.Links.Add(link);
                                        }
                                        else
                                        {
                                            var CoordinatesInBranchingLogicObj = UOWObj.CoordinatesInBranchingLogicRepository.Get(a => a.ObjectId == branchingLogic.DestinationObjectId && a.ObjectTypeId == (int)quizBranchingLogic.ObjectType).FirstOrDefault();
                                            if (CoordinatesInBranchingLogicObj != null)
                                            {
                                                quizBranchingLogic.Position = new string[] { CoordinatesInBranchingLogicObj.XCoordinate, CoordinatesInBranchingLogicObj.YCoordinate };
                                            }
                                            quizBranchingLogic.Links.Add(link);
                                            QuizBranchingLogicLinksLstObj.QuizBranchingLogicLinks.Add(quizBranchingLogic);
                                        }

                                    }

                                    if (branchingLogic.SourceTypeId == (int)BranchingLogicEnum.ANSWER)
                                    {
                                        var asd = UOWObj.QuestionsInQuizRepository.Get(a => a.AnswerOptionsInQuizQuestions.Any(r => r.Id == branchingLogic.SourceObjectId)).FirstOrDefault();

                                        quizBranchingLogic = new QuizBranchingLogicLinks()
                                        {
                                            ObjectType = BranchingLogicEnum.QUESTION,
                                            ObjectTypeId = asd.Id.ToString(),
                                            Links = new List<BranchingLinks>()
                                        };
                                    }
                                    else if (branchingLogic.SourceTypeId == (int)BranchingLogicEnum.QUESTIONNEXT)
                                    {
                                        quizBranchingLogic = new QuizBranchingLogicLinks()
                                        {
                                            ObjectType = BranchingLogicEnum.QUESTION,
                                            ObjectTypeId = branchingLogic.SourceObjectId.ToString(),
                                            Links = new List<BranchingLinks>()
                                        };
                                    }
                                    else
                                    {
                                        quizBranchingLogic = new QuizBranchingLogicLinks()
                                        {
                                            ObjectType = branchingLogic.SourceTypeId == (int)BranchingLogicEnum.ACTIONNEXT ? BranchingLogicEnum.ACTION : branchingLogic.SourceTypeId == (int)BranchingLogicEnum.RESULTNEXT ? BranchingLogicEnum.RESULT : branchingLogic.SourceTypeId == (int)BranchingLogicEnum.CONTENTNEXT ? BranchingLogicEnum.CONTENT : branchingLogic.SourceTypeId == (int)BranchingLogicEnum.ANSWER ? BranchingLogicEnum.QUESTION : (BranchingLogicEnum)branchingLogic.SourceTypeId,
                                            ObjectTypeId = branchingLogic.SourceObjectId.ToString(),
                                            Links = new List<BranchingLinks>()
                                        };

                                    }
                                    var links = new BranchingLinks()
                                    {
                                        FromId = branchingLogic.SourceTypeId == (int)BranchingLogicEnum.ANSWER ? "a_" + branchingLogic.SourceObjectId.ToString() : branchingLogic.SourceTypeId == (int)BranchingLogicEnum.CONTENTNEXT ? "cb_" + branchingLogic.SourceObjectId.ToString() : branchingLogic.SourceTypeId == (int)BranchingLogicEnum.RESULTNEXT ? "rb_" + branchingLogic.SourceObjectId.ToString() : branchingLogic.SourceTypeId == (int)BranchingLogicEnum.ACTIONNEXT ? "acb_" + branchingLogic.SourceObjectId.ToString() : branchingLogic.SourceTypeId == (int)BranchingLogicEnum.QUESTIONNEXT ? "qn_" + branchingLogic.SourceObjectId.ToString() : string.Empty,
                                        FromType = (BranchingLogicEnum)branchingLogic.SourceTypeId,
                                        ToId = branchingLogic.DestinationTypeId == (int)BranchingLogicEnum.QUESTION ? "q_" + branchingLogic.DestinationObjectId.ToString() : branchingLogic.DestinationTypeId == (int)BranchingLogicEnum.CONTENT ? "c_" + branchingLogic.DestinationObjectId.ToString() : branchingLogic.DestinationTypeId == (int)BranchingLogicEnum.RESULT ? "r_" + branchingLogic.DestinationObjectId.ToString() : branchingLogic.DestinationTypeId == (int)BranchingLogicEnum.ACTION ? "ac_" + branchingLogic.DestinationObjectId.ToString() : (branchingLogic.DestinationTypeId == (int)BranchingLogicEnum.BADGE) ? "b_" + branchingLogic.DestinationObjectId.ToString() : (branchingLogic.DestinationTypeId == (int)BranchingLogicEnum.BADGENEXT) ? "bb_" + branchingLogic.DestinationObjectId.ToString() : string.Empty,
                                        ToType = branchingLogic.DestinationTypeId != null ? (BranchingLogicEnum)branchingLogic.DestinationTypeId : BranchingLogicEnum.NONE,
                                        IsCorrect = branchingLogic.SourceTypeId == (int)BranchingLogicEnum.ANSWER && UOWObj.AnswerOptionsInQuizQuestionsRepository.Get(r => r.Id == branchingLogic.SourceObjectId && r.IsCorrectAnswer.HasValue && r.IsCorrectAnswer.Value).Any()

                                    };
                                    if (branchingLogic.SourceTypeId == (int)BranchingLogicEnum.QUESTION)
                                    {
                                        var questionId = branchingLogic.SourceObjectId;
                                        quizBranchingLogic.ObjectTypeId = questionId.ToString();
                                        var quizBranchingLogicObj = QuizBranchingLogicLinksLstObj.QuizBranchingLogicLinks.FirstOrDefault(a => a.ObjectType == quizBranchingLogic.ObjectType && a.ObjectTypeId == quizBranchingLogic.ObjectTypeId);
                                        if (quizBranchingLogicObj != null)
                                        {
                                            if (quizBranchingLogicObj.Links.FirstOrDefault(a => string.IsNullOrEmpty(a.FromId) && string.IsNullOrEmpty(a.ToId)) != null)
                                            {
                                                quizBranchingLogicObj.Links.Remove(quizBranchingLogicObj.Links.FirstOrDefault(a => string.IsNullOrEmpty(a.FromId) && string.IsNullOrEmpty(a.ToId)));
                                            }
                                            quizBranchingLogic = quizBranchingLogicObj;
                                            quizBranchingLogic.Links.Add(links);
                                            if (quizBranchingLogic.Position == null)
                                            {
                                                var CoordinatesInBranchingLogicObj = UOWObj.CoordinatesInBranchingLogicRepository.Get(a => a.ObjectId == questionId && a.ObjectTypeId == (int)BranchingLogicEnum.QUESTION).FirstOrDefault();
                                                if (CoordinatesInBranchingLogicObj != null)
                                                {
                                                    quizBranchingLogic.Position = new string[] { CoordinatesInBranchingLogicObj.XCoordinate, CoordinatesInBranchingLogicObj.YCoordinate };
                                                }
                                            }
                                        }
                                        else
                                        {
                                            var CoordinatesInBranchingLogicObj = UOWObj.CoordinatesInBranchingLogicRepository.Get(a => a.ObjectId == questionId && a.ObjectTypeId == (int)BranchingLogicEnum.QUESTION).FirstOrDefault();
                                            if (CoordinatesInBranchingLogicObj != null)
                                            {
                                                quizBranchingLogic.Position = new string[] { CoordinatesInBranchingLogicObj.XCoordinate, CoordinatesInBranchingLogicObj.YCoordinate };
                                            }
                                            quizBranchingLogic.Links.Add(links);
                                            QuizBranchingLogicLinksLstObj.QuizBranchingLogicLinks.Add(quizBranchingLogic);

                                        }
                                    }
                                    else if (branchingLogic.SourceTypeId == (int)BranchingLogicEnum.ANSWER || branchingLogic.SourceTypeId == (int)BranchingLogicEnum.QUESTIONNEXT)
                                    {

                                        var quizBranchingLogicObj = QuizBranchingLogicLinksLstObj.QuizBranchingLogicLinks.FirstOrDefault(a => a.ObjectTypeId == quizBranchingLogic.ObjectTypeId);
                                        if (quizBranchingLogicObj != null)
                                        {
                                            if (quizBranchingLogicObj.Links.FirstOrDefault(a => string.IsNullOrEmpty(a.FromId) && string.IsNullOrEmpty(a.ToId)) != null)
                                            {
                                                quizBranchingLogicObj.Links.Remove(quizBranchingLogicObj.Links.FirstOrDefault(a => string.IsNullOrEmpty(a.FromId) && string.IsNullOrEmpty(a.ToId)));
                                            }
                                            quizBranchingLogic = quizBranchingLogicObj;
                                            quizBranchingLogic.Links.Add(links);
                                            if (quizBranchingLogic.Position == null)
                                            {
                                                var questionsInQuizObj = UOWObj.QuestionsInQuizRepository.Get(q => q.AnswerOptionsInQuizQuestions.Any(r => r.Id == branchingLogic.SourceObjectId)).FirstOrDefault();
                                                var CoordinatesInBranchingLogicObj = UOWObj.CoordinatesInBranchingLogicRepository.Get(a => a.ObjectId == questionsInQuizObj.Id && a.ObjectTypeId == (int)BranchingLogicEnum.QUESTION).FirstOrDefault();
                                                if (CoordinatesInBranchingLogicObj != null)
                                                {
                                                    quizBranchingLogic.Position = new string[] { CoordinatesInBranchingLogicObj.XCoordinate, CoordinatesInBranchingLogicObj.YCoordinate };
                                                }
                                            }
                                        }
                                        else
                                        {
                                            var questionsInQuizObj = UOWObj.QuestionsInQuizRepository.Get(q => q.AnswerOptionsInQuizQuestions.Any(r => r.Id == branchingLogic.SourceObjectId)).FirstOrDefault();
                                            var CoordinatesInBranchingLogicObj = new Db.CoordinatesInBranchingLogic();
                                            if (branchingLogic.SourceTypeId == (int)BranchingLogicEnum.QUESTIONNEXT)
                                                CoordinatesInBranchingLogicObj = UOWObj.CoordinatesInBranchingLogicRepository.Get(a => a.ObjectId == branchingLogic.SourceObjectId && a.ObjectTypeId == (int)BranchingLogicEnum.QUESTION).FirstOrDefault();
                                            else
                                                CoordinatesInBranchingLogicObj = UOWObj.CoordinatesInBranchingLogicRepository.Get(a => a.ObjectId == questionsInQuizObj.Id && a.ObjectTypeId == (int)BranchingLogicEnum.QUESTION).FirstOrDefault();

                                            if (CoordinatesInBranchingLogicObj != null)
                                            {
                                                quizBranchingLogic.Position = new string[] { CoordinatesInBranchingLogicObj.XCoordinate, CoordinatesInBranchingLogicObj.YCoordinate };
                                            }
                                            quizBranchingLogic.Links.Add(links);
                                            QuizBranchingLogicLinksLstObj.QuizBranchingLogicLinks.Add(quizBranchingLogic);
                                        }
                                    }
                                    else
                                    {
                                        var quizBranchingLogicObj = QuizBranchingLogicLinksLstObj.QuizBranchingLogicLinks.FirstOrDefault(a => a.ObjectTypeId == quizBranchingLogic.ObjectTypeId && a.ObjectType == quizBranchingLogic.ObjectType);
                                        if (quizBranchingLogicObj != null)
                                        {
                                            if (quizBranchingLogicObj.Links.FirstOrDefault(a => string.IsNullOrEmpty(a.FromId) && string.IsNullOrEmpty(a.ToId)) != null)
                                            {
                                                quizBranchingLogicObj.Links.Remove(quizBranchingLogicObj.Links.FirstOrDefault(a => string.IsNullOrEmpty(a.FromId) && string.IsNullOrEmpty(a.ToId)));
                                            }
                                            quizBranchingLogic = quizBranchingLogicObj;
                                            quizBranchingLogic.Links.Add(links);
                                            if (quizBranchingLogic.Position == null)
                                            {
                                                var CoordinatesInBranchingLogicObj = UOWObj.CoordinatesInBranchingLogicRepository.Get(a => a.ObjectId == branchingLogic.SourceObjectId && (a.ObjectTypeId == (int)quizBranchingLogic.ObjectType || a.ObjectTypeId == (int)quizBranchingLogic.ObjectType)).FirstOrDefault();
                                                if (CoordinatesInBranchingLogicObj != null)
                                                {
                                                    quizBranchingLogic.Position = new string[] { CoordinatesInBranchingLogicObj.XCoordinate, CoordinatesInBranchingLogicObj.YCoordinate };
                                                }
                                            }
                                        }
                                        else
                                        {
                                            var CoordinatesInBranchingLogicObj = UOWObj.CoordinatesInBranchingLogicRepository.Get(a => a.ObjectId == branchingLogic.SourceObjectId && (a.ObjectTypeId == (int)quizBranchingLogic.ObjectType || a.ObjectTypeId == (int)quizBranchingLogic.ObjectType)).FirstOrDefault();
                                            if (CoordinatesInBranchingLogicObj != null)
                                            {
                                                quizBranchingLogic.Position = new string[] { CoordinatesInBranchingLogicObj.XCoordinate, CoordinatesInBranchingLogicObj.YCoordinate };
                                            }
                                            quizBranchingLogic.Links.Add(links);
                                            QuizBranchingLogicLinksLstObj.QuizBranchingLogicLinks.Add(quizBranchingLogic);
                                        }
                                    }
                                }
                                if (QuizBranchingLogicLinksLstObj.QuizBranchingLogicLinks.Where(a => a.ObjectType == BranchingLogicEnum.START).Count() == 0)
                                {
                                    quizBranchingLogic = new QuizBranchingLogicLinks()
                                    {
                                        ObjectType = BranchingLogicEnum.START,
                                        ObjectTypeId = "start",
                                        Links = new List<BranchingLinks>()
                                    };
                                    var link = new BranchingLinks()
                                    {
                                        FromId = "start",
                                        FromType = BranchingLogicEnum.START,
                                        ToId = string.Empty,
                                        ToType = BranchingLogicEnum.NONE
                                    };

                                    var CoordinatesInBranchingLogicObj = UOWObj.CoordinatesInBranchingLogicRepository.Get(a => a.ObjectId == QuizId && a.ObjectTypeId == (int)quizBranchingLogic.ObjectType).FirstOrDefault();
                                    if (CoordinatesInBranchingLogicObj != null)
                                    {
                                        quizBranchingLogic.Position = new string[] { CoordinatesInBranchingLogicObj.XCoordinate, CoordinatesInBranchingLogicObj.YCoordinate };
                                    }
                                    else
                                        quizBranchingLogic.Position = new string[] { "0", "0" };
                                    quizBranchingLogic.Links.Add(link);
                                    QuizBranchingLogicLinksLstObj.QuizBranchingLogicLinks.Add(quizBranchingLogic);
                                }
                                if (quizObj.QuizType == (int)QuizTypeEnum.Personality || quizObj.QuizType == (int)QuizTypeEnum.PersonalityTemplate)
                                {
                                    var correlatedResult = quizDetailsObj.QuizResults.FirstOrDefault(b => b.IsPersonalityCorrelatedResult == true && b.Status == (int)StatusEnum.Active);
                                    if (correlatedResult != null)
                                    {
                                        if (QuizBranchingLogicLinksLstObj.QuizBranchingLogicLinks.Where(a => a.ObjectType == BranchingLogicEnum.RESULT && a.ObjectTypeId == correlatedResult.Id.ToString()).Count() == 0)
                                        {
                                            quizBranchingLogic = new QuizBranchingLogicLinks()
                                            {
                                                ObjectType = BranchingLogicEnum.RESULT,
                                                ObjectTypeId = correlatedResult.Id.ToString(),
                                                Links = new List<BranchingLinks>()
                                            };

                                            var destinationObjectIds = QuizBranchingLogicLinksLstObj.QuizBranchingLogicLinks.Where(a => !string.IsNullOrWhiteSpace(a.ObjectTypeId)).Select(b => b.ObjectTypeId).Distinct();
                                            var MaxYCoordinate = UOWObj.CoordinatesInBranchingLogicRepository.Get(a => destinationObjectIds.Contains(a.ObjectId.ToString())).Max(b => b.YCoordinate);
                                            if (!string.IsNullOrWhiteSpace(MaxYCoordinate) && double.Parse(MaxYCoordinate) > 350)
                                            {
                                                quizBranchingLogic.Position = new string[] { "400", (double.Parse(MaxYCoordinate) + 150).ToString() };
                                            }
                                            else
                                                quizBranchingLogic.Position = new string[] { "400", "400" };
                                            QuizBranchingLogicLinksLstObj.QuizBranchingLogicLinks.Add(quizBranchingLogic);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Status = ResultEnum.Error;
                                ErrorMessage = "Quiz branching logic not enabled for the QuizId " + QuizId;
                            }
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz not found for the QuizId " + QuizId;
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }

            return QuizBranchingLogicLinksLstObj;
        }

        public void AddAttachmentInQuiz(QuizAttachment Obj, int BusinessUserId, int CompanyId)
        {
            using (var UOWObj = new AutomationUnitOfWork())
            {
                var quizObj = UOWObj.QuizRepository.GetByID(Obj.QuizId);

                if (quizObj != null)
                {
                    var attachmentLst = quizObj.AttachmentsInQuiz.ToList();
                    foreach (var attachment in attachmentLst)
                    {
                        UOWObj.AttachmentsInQuizRepository.Delete(attachment);
                    }
                    if (Obj.Attachments != null)
                        foreach (var attachment in Obj.Attachments)
                        {
                            var attachmentObj = new Db.AttachmentsInQuiz()
                            {
                                QuizId = Obj.QuizId,
                                Title = attachment.Title,
                                Description = attachment.Description,
                                PublicId = attachment.PublicIdForAttachment,
                                LastUpdatedBy = BusinessUserId,
                                LastUpdatedOn = DateTime.UtcNow
                            };
                            UOWObj.AttachmentsInQuizRepository.Insert(attachmentObj);
                        }
                    UOWObj.Save();

                    
                    
                    
                    
                    
                }
                else
                {
                    Status = ResultEnum.Error;
                    ErrorMessage = "Quiz not found for the QuizId " + Obj.QuizId;
                }
            }
        }

        public QuizAttachment GetAttachmentsInQuiz(int QuizId)
        {
            var QuizAttachmentObj = new QuizAttachment()
            {
                QuizId = QuizId,
                Attachments = new List<QuizAttachment.Attachment>()
            };

            using (var UOWObj = new AutomationUnitOfWork())
            {
                var quizObj = UOWObj.QuizRepository.GetByID(QuizId);

                if (quizObj != null)
                {
                    foreach (var attachment in quizObj.AttachmentsInQuiz)
                    {
                        var attachmentObj = new QuizAttachment.Attachment()
                        {
                            Title = attachment.Title,
                            Description = attachment.Description,
                            PublicIdForAttachment = attachment.PublicId,
                        };
                        QuizAttachmentObj.Attachments.Add(attachmentObj);
                    }
                }
                else
                {
                    Status = ResultEnum.Error;
                    ErrorMessage = "Quiz not found for the QuizId " + QuizId;
                }
                return QuizAttachmentObj;
            }
        }

        public void QuizShareSetting(QuizShare Obj, int BusinessUserId, bool IsCreateAcademyCourse, bool IsCreateTechnicalRecruiterCourse, int CompanyId)
        {
            using (var UOWObj = new AutomationUnitOfWork())
            {
                var quizObj = UOWObj.QuizRepository.GetByID(Obj.QuizId);

                if (quizObj != null)
                {
                    var userPermissionsInQuizObj = quizObj.UserPermissionsInQuiz;
                    var UserTypes = Obj.UserType.Split(',');
                    if (userPermissionsInQuizObj != null)
                    {
                        if (IsCreateTechnicalRecruiterCourse && userPermissionsInQuizObj.ToList().Any(r => r.UserTypeId == (int)UserTypeEnum.TechnicalRecruiter) && quizObj.ModulePermissionsInQuiz.Any())
                        {
                            foreach (var modulePermission in quizObj.ModulePermissionsInQuiz.ToList())
                                UOWObj.ModulePermissionsInQuizRepository.Delete(modulePermission);
                        }
                        foreach (var userPermissionsInQuiz in userPermissionsInQuizObj.ToList().Where(r => (IsCreateAcademyCourse || r.UserTypeId != (int)UserTypeEnum.JobRockAcademy) && (IsCreateTechnicalRecruiterCourse || r.UserTypeId != (int)UserTypeEnum.TechnicalRecruiter)))
                        {
                            UOWObj.UserPermissionsInQuizRepository.Delete(userPermissionsInQuiz);
                        }
                    }

                    foreach (var userType in UserTypes.Where(a => !string.IsNullOrEmpty(a)).ToList())
                    {
                        var userPermissionsObj = new Db.UserPermissionsInQuiz()
                        {
                            QuizId = Obj.QuizId,
                            UserTypeId = int.Parse(userType)
                        };
                        UOWObj.UserPermissionsInQuizRepository.Insert(userPermissionsObj);
                    };

                    if (IsCreateTechnicalRecruiterCourse && Obj.ModuleType.HasValue && UserTypes.Any(r => !string.IsNullOrEmpty(r) && int.Parse(r) == (int)UserTypeEnum.TechnicalRecruiter))
                    {
                        var modulePermissionsObj = new Db.ModulePermissionsInQuiz()
                        {
                            QuizId = Obj.QuizId,
                            ModuleTypeId = Obj.ModuleType.Value
                        };
                        UOWObj.ModulePermissionsInQuizRepository.Insert(modulePermissionsObj);
                    };

                    UOWObj.Save();

                    
                    
                    
                    
                    
                }
                else
                {
                    Status = ResultEnum.Error;
                    ErrorMessage = "Quiz not found for the QuizId " + Obj.QuizId;
                }
            }
        }

        public QuizShare GetQuizShareSetting(int QuizId)
        {
            using (var UOWObj = new AutomationUnitOfWork())
            {
                var quizObj = UOWObj.QuizRepository.GetByID(QuizId);

                if (quizObj != null)
                {
                    var userPermissionsInQuizObj = quizObj.UserPermissionsInQuiz;

                    if (userPermissionsInQuizObj != null)
                    {
                        var userPermissionsObj = new QuizShare()
                        {
                            QuizId = QuizId,
                            UserType = string.Empty
                        };
                        foreach (var userPermissionsInQuiz in userPermissionsInQuizObj)
                        {
                            userPermissionsObj.UserType = string.IsNullOrEmpty(userPermissionsObj.UserType) ? userPermissionsInQuiz.UserTypeId.ToString() : userPermissionsObj.UserType + "," + userPermissionsInQuiz.UserTypeId;
                        };

                        if (userPermissionsInQuizObj.Any(r => r.UserTypeId == (int)UserTypeEnum.TechnicalRecruiter) && quizObj.ModulePermissionsInQuiz.Any())
                            userPermissionsObj.ModuleType = quizObj.ModulePermissionsInQuiz.FirstOrDefault().ModuleTypeId;

                        return userPermissionsObj;
                    }
                }
                else
                {
                    Status = ResultEnum.Error;
                    ErrorMessage = "Quiz not found for the QuizId " + QuizId;
                }
                return null;
            }
        }

        public QuizBadge AddBadgeInQuiz(int QuizId, int BusinessUserId, int CompanyId)
        {
            using (var UOWObj = new AutomationUnitOfWork())
            {
                var quizObj = UOWObj.QuizRepository.GetByID(QuizId);
                QuizBadge quizBadge = null;
                if (quizObj != null)
                {
                    var quizDetails = quizObj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED);
                    if (quizDetails != null)
                    {
                        var QuizBadgeObj = new Db.BadgesInQuiz()
                        {
                            QuizId = quizDetails.Id,
                            Image = string.Empty,
                            ShowImage = true,
                            PublicId = string.Empty,
                            Title = "Badge " + (quizDetails.BadgesInQuiz.Where(r => r.State == (int)StatusEnum.Active).Count() + 1).ToString(),
                            ShowTitle = true,
                            EnableMediaFile = false,
                            DisplayOrderForTitleImage = 2,
                            DisplayOrderForTitle = 1,
                            AutoPlay = true,
                            SecondsToApply = "0",
                            VideoFrameEnabled = false,
                            LastUpdatedBy = BusinessUserId,
                            LastUpdatedOn = DateTime.UtcNow,
                            Status = (int)StatusEnum.Active,
                            State = (int)QuizStateEnum.DRAFTED
                        };

                        UOWObj.BadgesInQuizRepository.Insert(QuizBadgeObj);
                        UOWObj.Save();
                        quizBadge = new QuizBadge()
                        {
                            Title = QuizBadgeObj.Title,
                            ShowTitle = QuizBadgeObj.ShowTitle,
                            Id = QuizBadgeObj.Id,
                            Image = QuizBadgeObj.Image,
                            ShowImage = QuizBadgeObj.ShowImage,
                            EnableMediaFile = QuizBadgeObj.EnableMediaFile,
                            PublicIdForBadge = QuizBadgeObj.PublicId,
                            DisplayOrderForTitleImage = QuizBadgeObj.DisplayOrderForTitleImage,
                            DisplayOrderForTitle = QuizBadgeObj.DisplayOrderForTitle,
                            AutoPlay = QuizBadgeObj.AutoPlay,
                            SecondsToApply = QuizBadgeObj.SecondsToApply,
                            VideoFrameEnabled = QuizBadgeObj.VideoFrameEnabled
                        };

                        
                        
                    }
                }
                else
                {
                    Status = ResultEnum.Error;
                    ErrorMessage = "Quiz not found for the QuizId " + QuizId;
                }
                return quizBadge;
            }

        }

        public void RemoveBadgeInQuiz(int Id, int BusinessUserId, int CompanyId)
        {
            using (var UOWObj = new AutomationUnitOfWork())
            {
                var badgesInQuizObj = UOWObj.BadgesInQuizRepository.GetByID(Id);

                if (badgesInQuizObj != null)
                {
                    SaveDynamicVariable(badgesInQuizObj.Title, string.Empty, badgesInQuizObj.QuizDetails.Id);

                    badgesInQuizObj.Status = (int)StatusEnum.Deleted;
                    badgesInQuizObj.LastUpdatedBy = BusinessUserId;
                    badgesInQuizObj.LastUpdatedOn = DateTime.UtcNow;

                    UOWObj.BadgesInQuizRepository.Update(badgesInQuizObj);

                    var branchingLogic = UOWObj.BranchingLogicRepository.Get(r => (r.SourceObjectId == Id && r.SourceTypeId == (int)BranchingLogicEnum.BADGENEXT) || (r.DestinationObjectId == Id && r.DestinationTypeId == (int)BranchingLogicEnum.BADGE));

                    if (branchingLogic.Any())
                    {
                        foreach (var obj in branchingLogic)
                        {
                            UOWObj.BranchingLogicRepository.Delete(obj);
                        }
                    }

                    branchingLogic = UOWObj.BranchingLogicRepository.Get(r => r.DestinationObjectId == Id && r.DestinationTypeId == (int)BranchingLogicEnum.BADGE);

                    UOWObj.Save();

                    
                    
                }
                else
                {
                    Status = ResultEnum.Error;
                    ErrorMessage = "Badge not found for the BadgeId " + Id;
                }
            }
        }

        public void UpdateBadgeInQuiz(QuizBadge BadgeObj, int BusinessUserId, int CompanyId)
        {
            using (var UOWObj = new AutomationUnitOfWork())
            {
                if (!string.IsNullOrEmpty(BadgeObj.Title))
                {
                    var badgesInQuizObj = UOWObj.BadgesInQuizRepository.GetByID(BadgeObj.Id);

                    if (badgesInQuizObj != null && badgesInQuizObj.QuizDetails.ParentQuizId == BadgeObj.QuizId)
                    {
                        SaveDynamicVariable(badgesInQuizObj.Title, BadgeObj.Title, badgesInQuizObj.QuizDetails.Id);

                        badgesInQuizObj.Title = BadgeObj.Title;
                        badgesInQuizObj.ShowTitle = BadgeObj.ShowTitle;
                        badgesInQuizObj.Image = BadgeObj.Image;
                        badgesInQuizObj.ShowImage = BadgeObj.ShowImage;
                        badgesInQuizObj.EnableMediaFile = BadgeObj.EnableMediaFile;
                        badgesInQuizObj.PublicId = BadgeObj.PublicIdForBadge;
                        badgesInQuizObj.AutoPlay = BadgeObj.AutoPlay;
                        badgesInQuizObj.SecondsToApply = BadgeObj.SecondsToApply;
                        badgesInQuizObj.VideoFrameEnabled = BadgeObj.VideoFrameEnabled;
                        badgesInQuizObj.DisplayOrderForTitle = BadgeObj.DisplayOrderForTitle;
                        badgesInQuizObj.DisplayOrderForTitleImage = BadgeObj.DisplayOrderForTitleImage;
                        badgesInQuizObj.LastUpdatedBy = BusinessUserId;
                        badgesInQuizObj.LastUpdatedOn = DateTime.UtcNow;

                        UOWObj.BadgesInQuizRepository.Update(badgesInQuizObj);
                        UOWObj.Save();

                        
                        
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Badge not found for the BadgeId " + BadgeObj.Id;
                    }
                }
                else
                {
                    Status = ResultEnum.Error;
                    ErrorMessage = "Title is required";
                }
            }
        }

        public QuizBadge GetBadgeInQuiz(int Id, int QuizId)
        {
            using (var UOWObj = new AutomationUnitOfWork())
            {
                var badgesInQuizObj = UOWObj.BadgesInQuizRepository.GetByID(Id);

                if (badgesInQuizObj != null && badgesInQuizObj.QuizDetails.ParentQuizId == QuizId)
                {
                    return new QuizBadge()
                    {
                        Title = badgesInQuizObj.Title,
                        ShowTitle = badgesInQuizObj.ShowTitle,
                        Id = badgesInQuizObj.Id,
                        Image = badgesInQuizObj.Image,
                        ShowImage = badgesInQuizObj.ShowImage,
                        EnableMediaFile = badgesInQuizObj.EnableMediaFile,
                        PublicIdForBadge = badgesInQuizObj.PublicId,
                        AutoPlay = badgesInQuizObj.AutoPlay,
                        SecondsToApply = badgesInQuizObj.SecondsToApply,
                        VideoFrameEnabled = badgesInQuizObj.VideoFrameEnabled,
                        DisplayOrderForTitle = badgesInQuizObj.DisplayOrderForTitle,
                        DisplayOrderForTitleImage = badgesInQuizObj.DisplayOrderForTitleImage
                    };
                }
                else
                {
                    Status = ResultEnum.Error;
                    ErrorMessage = "Badge not found for the BadgeId " + Id;
                }
            }
            return null;
        }

        public void RemoveTag(int TagId, int CompanyId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var tagsInAnswer = UOWObj.TagsInAnswerRepository.Get(r => r.TagId == TagId);

                    foreach (var obj in tagsInAnswer.ToList())
                    {
                        UOWObj.TagsInAnswerRepository.Delete(obj);
                    }
                    UOWObj.Save();

                    
                    
                    
                    
                    
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public void UpdateAnswerTagAndCategory(List<QuizAnsweTags> quizAnsweTags, int CompanyId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    foreach (var obj in quizAnsweTags)
                    {
                        var ans = UOWObj.AnswerOptionsInQuizQuestionsRepository.Get(t => t.Id == obj.answerId);

                        if (ans.Any())
                        {
                            var ExistquizAnsweTags = UOWObj.TagsInAnswerRepository.Get(t => t.AnswerOptionsId == obj.answerId);

                            foreach (var ExistquizAnsweTagsObj in ExistquizAnsweTags)
                            {
                                UOWObj.TagsInAnswerRepository.Delete(ExistquizAnsweTagsObj);
                            }
                            UOWObj.Save();
                            foreach (var categoryObj in obj.Categories.Where(a => a.CategoryId != 0))
                            {
                                foreach (var tagDetail in categoryObj.TagDetails)
                                {
                                    Db.TagsInAnswer TagsInAnswerObj = new Db.TagsInAnswer();
                                    TagsInAnswerObj.TagCategoryId = categoryObj.CategoryId;
                                    TagsInAnswerObj.TagId = tagDetail.TagId;
                                    TagsInAnswerObj.AnswerOptionsId = obj.answerId;

                                    UOWObj.TagsInAnswerRepository.Insert(TagsInAnswerObj);
                                    UOWObj.Save();
                                }
                            }

                            
                            
                            
                            
                            
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public List<AttemptedQuizDetail> GetAttemptedQuizDetailByLead(string LeadId, int QuizId, string ConfigurationId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var workPackageInfoObj = UOWObj.WorkPackageInfoRepository.Get(r => (string.IsNullOrEmpty(ConfigurationId) ? true : (r.ConfigurationDetails != null && r.ConfigurationDetails.ConfigurationId == ConfigurationId)) && ((r.QuizAttempts.Count() == 0 || r.QuizAttempts.FirstOrDefault().QuizStats.Count() == 0) && ((QuizId > 0 && r.QuizId == QuizId && r.LeadUserId == LeadId) || (QuizId == 0 && r.LeadUserId == LeadId))));
                    var quizAttemptsObj = UOWObj.QuizAttemptsRepository.Get(r => (string.IsNullOrEmpty(ConfigurationId) ? true : (r.ConfigurationDetails != null && r.ConfigurationDetails.ConfigurationId == ConfigurationId)) && (r.QuizStats.Any()) && ((QuizId > 0 && r.QuizDetails.ParentQuizId == QuizId && r.LeadUserId == LeadId) || (QuizId == 0 && r.LeadUserId == LeadId)));

                    var attemptedQuizDetailList = new List<AttemptedQuizDetail>();


                    foreach (var obj in quizAttemptsObj)
                    {
                        var correctAnsCount = 0;
                        var ShowScoreValue = false;
                        var scoreValueTxt = string.Empty;
                        var resultSetting = obj.QuizDetails.ResultSettings.FirstOrDefault();

                        if (resultSetting != null)
                        {
                            var attemptedQuestions = obj.QuizQuestionStats.Where(t => t.Status == (int)StatusEnum.Active && t.CompletedOn != null && (t.QuestionsInQuiz.TimerRequired ? true : t.QuizAnswerStats.Any(r => !r.AnswerOptionsInQuizQuestions.IsUnansweredType)));

                            if (resultSetting.ShowScoreValue.HasValue && resultSetting.ShowScoreValue.Value && (obj.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || obj.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate || attemptedQuestions.Any(r => (r.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Any(t => t.Status == (int)StatusEnum.Active && t.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && t.IsCorrectForMultipleAnswer.HasValue && t.IsCorrectForMultipleAnswer.Value)) || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single)))
                            {
                                if (obj.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || obj.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                    correctAnsCount = attemptedQuestions.Sum(a => a.QuizAnswerStats.Sum(x => x.AnswerOptionsInQuizQuestions.AssociatedScore)).Value;
                                else
                                    correctAnsCount = attemptedQuestions.Count(a => (a.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && ((a.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Select(s => s.Id).OrderBy(s => s)).SequenceEqual(a.QuizAnswerStats.Select(r => r.AnswerId).OrderBy(s => s)))) || (a.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single && a.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer == true));

                                ShowScoreValue = true;
                                scoreValueTxt = string.IsNullOrEmpty(scoreValueTxt) ? string.Empty : scoreValueTxt.Replace("%score%", ' ' + correctAnsCount.ToString() + ' ').Replace("%total%", ' ' + attemptedQuestions.Count(t => (t.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Any(r => r.Status == (int)StatusEnum.Active && r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value)) || t.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single).ToString() + ' ');
                            }
                        }

                        var attemptedQuizDetail = new AttemptedQuizDetail();
                        attemptedQuizDetail.ConfigurationId = obj.ConfigurationDetails != null ? obj.ConfigurationDetails.ConfigurationId : string.Empty;
                        attemptedQuizDetail.QuizDetails = new AttemptedQuizDetail.QuizDetail();
                        attemptedQuizDetail.QuizDetails.QuizId = obj.QuizDetails.ParentQuizId;
                        attemptedQuizDetail.QuizDetails.QuizTitle = HttpUtility.HtmlDecode(Regex.Replace(VariableLinking(obj.QuizDetails.QuizTitle, obj.QuizDetails, obj, true, ShowScoreValue, scoreValueTxt), "<.*?>", string.Empty));

                        if (obj.QuizStats.FirstOrDefault().CompletedOn != null)
                        {
                            attemptedQuizDetail.QuizDetails.QuizStatus = (QuizStatusEnum.Completed).ToString();
                            attemptedQuizDetail.QuizDetails.QuizDate = obj.QuizStats.FirstOrDefault().CompletedOn;
                        }
                        else
                        {
                            attemptedQuizDetail.QuizDetails.QuizStatus = (QuizStatusEnum.Started).ToString();
                            attemptedQuizDetail.QuizDetails.QuizDate = obj.QuizStats.FirstOrDefault().StartedOn;
                        }

                        var quizResults = obj.QuizStats.Where(r => r.CompletedOn != null && r.ResultId != null);
                        attemptedQuizDetail.QuizDetails.AchievedResults = new List<AttemptedQuizDetail.QuizDetail.AchievedResult>();

                        foreach (var quizResultsobj in quizResults)
                        {
                            var achievedResultDetails = new AttemptedQuizDetail.QuizDetail.AchievedResult();
                            achievedResultDetails.Id = quizResultsobj.ResultId.Value;
                            achievedResultDetails.Title = HttpUtility.HtmlDecode(Regex.Replace(VariableLinking(quizResultsobj.QuizResults.Title, obj.QuizDetails, obj, false, ShowScoreValue, scoreValueTxt), "<.*?>", string.Empty));
                            achievedResultDetails.InternalTitle = HttpUtility.HtmlDecode(Regex.Replace(VariableLinking(quizResultsobj.QuizResults.InternalTitle, obj.QuizDetails, obj, false, ShowScoreValue, scoreValueTxt), "<.*?>", string.Empty));
                            attemptedQuizDetail.QuizDetails.AchievedResults.Add(achievedResultDetails);
                        }

                        var quizQuestions = obj.QuizQuestionStats.Where(r => r.QuestionsInQuiz.Type == (int)BranchingLogicEnum.QUESTION && r.Status == (int)StatusEnum.Active && r.CompletedOn != null && (r.QuestionsInQuiz.TimerRequired ? true : r.QuizAnswerStats.Any(t => !t.AnswerOptionsInQuizQuestions.IsUnansweredType)));
                        attemptedQuizDetail.QuestionDetails = new List<AttemptedQuizDetail.QuestionDetail>();
                        foreach (var quizQuestionsobj in quizQuestions)
                        {
                            var question = quizQuestionsobj.QuestionsInQuiz;
                            var questionDetail = new AttemptedQuizDetail.QuestionDetail();
                            questionDetail.QuiestionId = quizQuestionsobj.QuestionId;
                            questionDetail.AnswerType = question.AnswerType;
                            questionDetail.QuiestionTitle = HttpUtility.HtmlDecode(Regex.Replace(VariableLinking(question.Question, obj.QuizDetails, obj, false, ShowScoreValue, scoreValueTxt), "<.*?>", string.Empty));
                            questionDetail.QuestionAnswerDetails = new List<AttemptedQuizDetail.QuestionDetail.QuestionAnswerDetail>();
                            if (question.AnswerType == (int)AnswerTypeEnum.Single || question.AnswerType == (int)AnswerTypeEnum.Multiple || question.AnswerType == (int)AnswerTypeEnum.DrivingLicense || question.AnswerType == (int)AnswerTypeEnum.LookingforJobs)
                            {
                                int associatedScore = default(int);
                                bool? IsCorrectValue = null;
                                if (question.AnswerType == (int)AnswerTypeEnum.Multiple && question.AnswerOptionsInQuizQuestions.Any(r => r.Status == (int)StatusEnum.Active && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value))
                                {
                                    if (obj.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || obj.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                        associatedScore = quizQuestionsobj.QuizAnswerStats.Sum(x => x.AnswerOptionsInQuizQuestions.AssociatedScore).Value;
                                    else if (obj.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Assessment || obj.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.AssessmentTemplate)
                                        IsCorrectValue = (question.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Select(s => s.Id).OrderBy(s => s)).SequenceEqual(quizQuestionsobj.QuizAnswerStats.Select(r => r.AnswerId).OrderBy(s => s));
                                }
                                else if (question.AnswerType == (int)AnswerTypeEnum.Single)
                                {
                                    if (obj.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || obj.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                        associatedScore = quizQuestionsobj.QuizAnswerStats.Sum(x => x.AnswerOptionsInQuizQuestions.AssociatedScore).Value;
                                    else if (obj.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Assessment || obj.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.AssessmentTemplate)
                                        IsCorrectValue = quizQuestionsobj.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer.HasValue && quizQuestionsobj.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer.Value;

                                }
                                questionDetail.IsCorrect = IsCorrectValue;
                                if (quizQuestionsobj.QuizAnswerStats.Any(r => !r.AnswerOptionsInQuizQuestions.IsUnansweredType))
                                {
                                    foreach (var quizAnswerStatsObj in quizQuestionsobj.QuizAnswerStats)
                                    {
                                        var questionAnswerDetail = new AttemptedQuizDetail.QuestionDetail.QuestionAnswerDetail();
                                        questionAnswerDetail.AnswerId = quizAnswerStatsObj.AnswerId;
                                        questionAnswerDetail.AnswerTitle = HttpUtility.HtmlDecode(Regex.Replace(VariableLinking(quizAnswerStatsObj.AnswerOptionsInQuizQuestions.Option, obj.QuizDetails, obj, false, ShowScoreValue, scoreValueTxt), "<.*?>", string.Empty));
                                        questionAnswerDetail.AssociatedScore = (obj.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || obj.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate) ? default(int) : associatedScore;
                                        questionDetail.QuestionAnswerDetails.Add(questionAnswerDetail);
                                    }
                                }
                                else
                                {
                                    var questionAnswerDetail = new AttemptedQuizDetail.QuestionDetail.QuestionAnswerDetail();
                                    questionAnswerDetail.AnswerId = null;
                                    questionAnswerDetail.AnswerTitle = null;
                                    questionAnswerDetail.AssociatedScore = null;
                                    questionDetail.QuestionAnswerDetails.Add(questionAnswerDetail);
                                }

                            }
                            else if (question.AnswerType == (int)AnswerTypeEnum.Short || question.AnswerType == (int)AnswerTypeEnum.Long || quizQuestionsobj.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.DOB || quizQuestionsobj.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.PostCode)
                            {
                                questionDetail.QuestionAnswerDetails.Add(new AttemptedQuizDetail.QuestionDetail.QuestionAnswerDetail()
                                {
                                    AnswerId = quizQuestionsobj.QuizAnswerStats.FirstOrDefault().AnswerId,
                                    AnswerTitle = quizQuestionsobj.QuizAnswerStats.FirstOrDefault().AnswerText
                                });
                            }
                            else if (question.AnswerType == (int)AnswerTypeEnum.FullAddress)
                            {
                                foreach (var quizAnswerStatsObj in quizQuestionsobj.QuizAnswerStats)
                                {
                                    questionDetail.QuestionAnswerDetails.Add(new AttemptedQuizDetail.QuestionDetail.QuestionAnswerDetail()
                                    {
                                        AnswerId = quizAnswerStatsObj.AnswerId,
                                        AnswerTitle = quizAnswerStatsObj.AnswerText
                                    });
                                }
                            }
                            else if (question.AnswerType == (int)AnswerTypeEnum.NPS || question.AnswerType == (int)AnswerTypeEnum.RatingEmoji || question.AnswerType == (int)AnswerTypeEnum.RatingStarts || question.AnswerType == (int)AnswerTypeEnum.Availability)
                            {
                                questionDetail.QuestionAnswerDetails.Add(new AttemptedQuizDetail.QuestionDetail.QuestionAnswerDetail()
                                {
                                    AnswerId = quizQuestionsobj.QuizAnswerStats.FirstOrDefault().AnswerId,
                                    AnswerTitle = quizQuestionsobj.QuizAnswerStats.FirstOrDefault().AnswerText,
                                    Comment = quizQuestionsobj.QuizAnswerStats.FirstOrDefault().Comment
                                });
                            }

                            attemptedQuizDetail.QuestionDetails.Add(questionDetail);
                        }
                        attemptedQuizDetailList.Add(attemptedQuizDetail);
                    }


                    foreach (var obj in workPackageInfoObj)
                    {
                        if (!quizAttemptsObj.Any(v => v.LeadUserId == obj.LeadUserId && v.QuizDetails.ParentQuizId == obj.QuizId && v.ConfigurationDetailsId == obj.ConfigurationDetailsId))
                        {
                            var attemptedQuizDetail = new AttemptedQuizDetail();
                            attemptedQuizDetail.ConfigurationId = obj.ConfigurationDetails != null ? obj.ConfigurationDetails.ConfigurationId : string.Empty;
                            attemptedQuizDetail.QuizDetails = new AttemptedQuizDetail.QuizDetail();
                            attemptedQuizDetail.QuizDetails.QuizId = obj.QuizId;



                            if (obj.QuizAttempts.Any())
                                attemptedQuizDetail.QuizDetails.QuizTitle = HttpUtility.HtmlDecode(Regex.Replace(VariableLinking(obj.QuizAttempts.LastOrDefault().QuizDetails.QuizTitle, obj.QuizAttempts.LastOrDefault().QuizDetails, obj.QuizAttempts.LastOrDefault(), true, false, null), "<.*?>", string.Empty));
                            else
                            {
                                var quizAttempt = new QuizApp.Db.QuizAttempts() { LeadUserId = obj.LeadUserId };
                                attemptedQuizDetail.QuizDetails.QuizTitle = HttpUtility.HtmlDecode(Regex.Replace(VariableLinking(obj.Quiz.QuizDetails.LastOrDefault().QuizTitle, obj.Quiz.QuizDetails.LastOrDefault(), quizAttempt, true, false, null), "<.*?>", string.Empty));
                            }

                            attemptedQuizDetail.QuizDetails.QuizStatus = (QuizStatusEnum.Sent).ToString();
                            attemptedQuizDetail.QuizDetails.QuizDate = obj.CreatedOn;
                            attemptedQuizDetailList.Add(attemptedQuizDetail);
                        }
                    }


                    return attemptedQuizDetailList;
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public void RemoveCategory(int CategoryId, int CompanyId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var tagsInAnswer = UOWObj.TagsInAnswerRepository.Get(r => r.TagCategoryId == CategoryId);

                    foreach (var obj in tagsInAnswer.ToList())
                    {
                        UOWObj.TagsInAnswerRepository.Delete(obj);
                    }
                    UOWObj.Save();

                    
                    
                    
                    
                    
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public void UpdateResultRange(QuizResultRange quizResultRangeObj, int BusinessUserId, int CompanyId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizObj = UOWObj.QuizRepository.GetByID(quizResultRangeObj.QuizId);

                    if (quizObj != null)
                    {
                        var currentDate = DateTime.UtcNow;

                        foreach (var obj in quizResultRangeObj.Results)
                        {
                            var Quizresult = quizObj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED).QuizResults.Where(r => r.State == (int)StatusEnum.Active && r.Id == obj.ResultId);
                            if (Quizresult.Any())
                            {
                                var quizResult = UOWObj.QuizResultsRepository.GetByID(obj.ResultId);

                                if (quizResult != null)
                                {
                                    quizResult.MinScore = obj.MinScore;
                                    quizResult.MaxScore = obj.MaxScore;
                                    quizResult.QuizDetails.LastUpdatedBy = BusinessUserId;
                                    quizResult.QuizDetails.LastUpdatedOn = currentDate;
                                    quizResult.QuizDetails.Quiz.State = (int)QuizStateEnum.DRAFTED;

                                    UOWObj.QuizRepository.Update(quizResult.QuizDetails.Quiz);
                                    UOWObj.Save();

                                    
                                    
                                }
                            }
                            else
                            {
                                Status = ResultEnum.Error;
                                ErrorMessage = "There is no result found for id " + obj.ResultId + " for this quiz.";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public static string VariableLinking(string Text, QuizApp.Db.QuizDetails QuizDetail, QuizApp.Db.QuizAttempts QuizAttempt, bool IsTitle, bool ShowScore, string ResultScore)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    if (!string.IsNullOrEmpty(Text) && Regex.Matches(Text, @"%\b\S+?\b%").Count > 0)
                    {
                        var LeadInfo = new QuizApp.Response.OWCLeadUserResponse.LeadUserResponse();

                        CompanyModel compObj;

                        if (QuizDetail != null && QuizDetail.Quiz.Company != null)
                        {
                            compObj = new CompanyModel
                            {
                                LeadDashboardApiAuthorizationBearer = QuizDetail.Quiz.Company.LeadDashboardApiAuthorizationBearer,
                                LeadDashboardApiUrl = QuizDetail.Quiz.Company.LeadDashboardApiUrl,
                                ClientCode = QuizDetail.Quiz.Company.ClientCode
                            };
                        }
                        else
                            compObj = new CompanyModel();


                        if (QuizAttempt != null && !string.IsNullOrEmpty(QuizAttempt.LeadUserId))
                            LeadInfo = OWCHelper.GetLeadUserInfo(QuizAttempt.LeadUserId, compObj);

                        Text = string.IsNullOrEmpty(Text) ? string.Empty : Text;

                        StringBuilder correctanswerexplanation = new StringBuilder("Incorrect answer details:<br/>");
                        if (Text.Contains("%correctanswerexplanation%") && QuizDetail != null && (QuizDetail.Quiz.QuizType == (int)QuizTypeEnum.Assessment || QuizDetail.Quiz.QuizType == (int)QuizTypeEnum.AssessmentTemplate))
                        {
                            if (QuizAttempt != null)
                            {
                                foreach (var ques in QuizAttempt.QuizQuestionStats.Where(r => r.Status == (int)StatusEnum.Active && r.QuizAnswerStats.Any(k => !k.AnswerOptionsInQuizQuestions.IsUnansweredType)).ToList())
                                {
                                    var correctAnswerTxt = string.Empty;
                                    var Incorrectques = false;
                                    if (ques.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && ques.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Any(r => r.Status == (int)StatusEnum.Active && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value))
                                    {
                                        Incorrectques = !((ques.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Select(s => s.Id).OrderBy(s => s)).SequenceEqual(ques.QuizAnswerStats.Select(r => r.AnswerId).OrderBy(s => s)));
                                        correctAnswerTxt = string.Join(",", ques.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Select(t => t.Option).Select(s => "'" + s + "'"));
                                    }
                                    else if (ques.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single)
                                    {
                                        Incorrectques = ques.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer.HasValue && ques.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer.Value ? false : true;
                                        correctAnswerTxt = ques.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(t => t.IsCorrectAnswer.HasValue ? t.IsCorrectAnswer.Value : false).Select(t => t.Option).FirstOrDefault();
                                    }

                                    if (Incorrectques)
                                    {
                                        correctanswerexplanation.Append("Question : " + ques.QuestionsInQuiz.Question + "<br/>");
                                        correctanswerexplanation.Append("Your Answer : " + string.Join(",", ques.QuizAnswerStats.Select(t => t.AnswerOptionsInQuizQuestions.Option).Select(s => "'" + s + "'")) + "<br/>");
                                        correctanswerexplanation.Append("Correct Answer : " + correctAnswerTxt + "<br/>");
                                        correctanswerexplanation.Append("Explanation for correct answer : " + QuizDetail.QuestionsInQuiz.FirstOrDefault().AliasTextForCorrectAnswer + "<br/>");
                                    }
                                }
                            }
                            else
                            {
                                Text = Text.Replace("%correctanswerexplanation%", string.Empty);
                            }
                        }

                        string Obj = string.IsNullOrEmpty(Text) ? string.Empty : Text.Replace("%fname%", (LeadInfo != null && !string.IsNullOrEmpty(LeadInfo.firstName)) ? LeadInfo.firstName : string.Empty)
                                                         .Replace("%lname%", (LeadInfo != null && !string.IsNullOrEmpty(LeadInfo.lastName)) ? LeadInfo.lastName : string.Empty)
                                                         .Replace("%phone%", (LeadInfo != null && !string.IsNullOrEmpty(LeadInfo.telephone)) ? LeadInfo.telephone : string.Empty)
                                                         .Replace("%email%", (LeadInfo != null && !string.IsNullOrEmpty(LeadInfo.firstName)) ? LeadInfo.email : string.Empty)
                                                         .Replace("%qname%", !IsTitle && QuizDetail != null ? QuizDetail.QuizTitle.Replace("%qname%", string.Empty) : string.Empty)
                                                         .Replace("%qlink%", QuizAttempt != null ? ((QuizAttempt.LeadUserId != null && QuizAttempt.WorkPackageInfo != null) ? GlobalSettings.webUrl.ToString() + "/quiz?Code=" + QuizAttempt.WorkPackageInfo.Quiz.PublishedCode + "&UserTypeId=2&UserId=" + QuizAttempt.WorkPackageInfo.LeadUserId + "&WorkPackageInfoId=" + QuizAttempt.WorkPackageInfo.Id
                                                                                                                                          : (QuizAttempt.RecruiterUserId != null ? GlobalSettings.elearningWebURL.ToString() + "/course/" + QuizAttempt.RecruiterUserId + "?Code=" + QuizDetail.Quiz.PublishedCode : GlobalSettings.webUrl.ToString() + "/quiz?Code=" + QuizDetail.Quiz.PublishedCode)) : string.Empty)
                                                         .Replace("%qendresult%", ShowScore && !string.IsNullOrEmpty(ResultScore) ? ResultScore.ToString() : string.Empty)
                                                         .Replace("%correctanswerexplanation%", correctanswerexplanation.ToString() == "Incorrect answer details:<br/>" ? string.Empty : correctanswerexplanation.ToString())
                                                         .Replace("%leadid%", (LeadInfo != null && !string.IsNullOrEmpty(LeadInfo.contactId)) ? LeadInfo.contactId : string.Empty);

                        IEnumerable<Db.VariablesDetails> variablesDetailList = new List<Db.VariablesDetails>();
                        if (QuizAttempt != null && QuizAttempt.ConfigurationDetailsId > 0 && QuizDetail != null)
                            variablesDetailList = UOWObj.VariablesDetailsRepository.Get(r => r.ConfigurationDetailsId == QuizAttempt.ConfigurationDetailsId);
                        else if (QuizAttempt != null && !string.IsNullOrEmpty(QuizAttempt.LeadUserId) && QuizDetail != null)
                            variablesDetailList = UOWObj.VariablesDetailsRepository.Get(r => r.VariableInQuiz.QuizId == QuizDetail.Id && r.LeadId == QuizAttempt.LeadUserId);

                        MatchCollection mcol = Regex.Matches(Obj, @"%\b\S+?\b%");


                        foreach (Match m in mcol)
                        {
                            var variablesDetailObj = variablesDetailList.FirstOrDefault(t => t.VariableInQuiz.Variables.Name == m.ToString().ToLower().Replace("%", string.Empty));
                            if (variablesDetailObj != null)
                            {
                                Obj = Obj.Replace(m.ToString(), variablesDetailObj.VariableValue);
                            }
                            else
                            {
                                Obj = Obj.Replace(m.ToString(), string.Empty);
                            }
                        }
                        return Obj;
                    }
                    else
                    {
                        return Text;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SaveDynamicVariable(string OldText, string NewText, int QuizId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var VariableObj = UOWObj.VariablesRepository.Get();

                    OldText = string.IsNullOrEmpty(OldText) ? string.Empty : OldText.ToLower().Replace("%leadid%", string.Empty).Replace("%fname%", string.Empty).Replace("%lname%", string.Empty).Replace("%phone%", string.Empty).Replace("%email%", string.Empty).Replace("%qname%", string.Empty).Replace("%qlink%", string.Empty).Replace("%qendresult%", string.Empty).Replace("%correctanswerexplanation%", string.Empty);

                    NewText = string.IsNullOrEmpty(NewText) ? string.Empty : NewText.ToLower().Replace("%leadid%", string.Empty).Replace("%fname%", string.Empty).Replace("%lname%", string.Empty).Replace("%phone%", string.Empty).Replace("%email%", string.Empty).Replace("%qname%", string.Empty).Replace("%qlink%", string.Empty).Replace("%qendresult%", string.Empty).Replace("%correctanswerexplanation%", string.Empty);

                    if (OldText.Equals(NewText, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return;
                    }


                    var NewTextDynamicVariables = Regex.Matches(NewText, @"%\b\S+?\b%").Cast<Match>().Select(m => m.Value).ToList();

                    var OldTextDynamicVariables = Regex.Matches(OldText, @"%\b\S+?\b%").Cast<Match>().Select(m => m.Value).ToList();

                    var DeletedElements = OldTextDynamicVariables.Except(NewTextDynamicVariables);

                    var AddedElements = NewTextDynamicVariables.Except(OldTextDynamicVariables);

                    var VariableInQuizObj = UOWObj.VariableInQuizRepository.Get();



                    foreach (var deletedElementsObj in DeletedElements)
                    {
                        var elements = VariableInQuizObj.FirstOrDefault(t => t.QuizId == QuizId && t.Variables != null && t.Variables.Name == deletedElementsObj.Replace("%", string.Empty));
                        if (elements != null)
                        {
                            elements.NumberOfUses = elements.NumberOfUses <= 0 ? 0 : elements.NumberOfUses - 1;
                            UOWObj.VariableInQuizRepository.Update(elements);

                            if (elements.NumberOfUses == 0 && VariableInQuizObj.Count(t => t.VariableId == elements.VariableId & t.NumberOfUses != 0) == 0)
                                UOWObj.VariablesRepository.Delete(elements.Variables);
                        }
                    }
                    UOWObj.Save();

                    foreach (var AddedElementsObj in AddedElements)
                    {
                        var variables = VariableObj.FirstOrDefault(t => t.Name == AddedElementsObj.Replace("%", string.Empty));
                        if (variables == null)
                        {
                            variables = new Db.Variables();
                            variables.Name = AddedElementsObj.Replace("%", string.Empty);
                            UOWObj.VariablesRepository.Insert(variables);
                        }
                        var variableInQuiz = variables.VariableInQuiz == null ? null : variables.VariableInQuiz.FirstOrDefault(r => r.QuizId == QuizId && r.VariableId == variables.Id);
                        if (variableInQuiz == null)
                        {
                            variableInQuiz = new Db.VariableInQuiz();
                            variableInQuiz.VariableId = variables.Id;
                            variableInQuiz.NumberOfUses = 1;
                            variableInQuiz.QuizId = QuizId;
                            UOWObj.VariableInQuizRepository.Insert(variableInQuiz);
                        }
                        else
                        {
                            variableInQuiz.NumberOfUses = variableInQuiz.NumberOfUses + 1;
                            UOWObj.VariableInQuizRepository.Update(variableInQuiz);
                        }
                        UOWObj.Save();
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public List<string> GetVariablesByQuizId(int QuizId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quiz = UOWObj.QuizRepository.GetByID(QuizId);
                    List<string> variableList = new List<string>();
                    if (quiz != null)
                    {
                        var quizDetails = quiz.QuizDetails.LastOrDefault(r => r.State == (int)QuizStateEnum.PUBLISHED);
                        if (quizDetails != null)
                        {
                            var VariableObj = UOWObj.VariablesRepository.Get(r => r.VariableInQuiz.Any(s => s.QuizId == quizDetails.Id && s.NumberOfUses > 0));

                            foreach (var obj in VariableObj)
                            {
                                variableList.Add(obj.Name);
                            }
                        }
                        else
                        {
                            Status = ResultEnum.Error;
                            ErrorMessage = "Quiz is not yet published.";
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz not found for the Quiz Id " + QuizId;
                    }
                    return variableList;
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public QuizResultAndAction GetQuizResultAndAction(int QuizId, bool AllResultRequired = false, bool AllActionRequired = false)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizActionResults = new QuizResultAndAction();

                    var quiz = UOWObj.QuizRepository.GetByID(QuizId);

                    if (quiz != null)
                    {
                        var resultLst = new List<QuizResultAndAction.Result>();

                        var quizDetails = quiz.QuizDetails.LastOrDefault(r => r.State == (int)QuizStateEnum.PUBLISHED);
                        if (quizDetails != null)
                        {

                            var quizResults = quizDetails.QuizResults.Where(r => r.State == (int)QuizStateEnum.PUBLISHED && r.Status == (int)StatusEnum.Active);

                            var ActionsInQuizIds = String.Join(",", quizDetails.ActionsInQuiz.Where(t => t.Status == (int)StatusEnum.Active && t.AppointmentId != 0).Select(s => s.AppointmentId).ToList());

                            QuizApp.Response.AppointmentTypeList appointmentTypeDetails = null;

                            if (!string.IsNullOrEmpty(ActionsInQuizIds))
                                appointmentTypeDetails = AppointmentHelper.GetAppointmentTypeDetailsList(ActionsInQuizIds);

                            if (AllResultRequired || (quizDetails.IsBranchingLogicEnabled.HasValue && quizDetails.IsBranchingLogicEnabled.Value))
                            {
                                var quizComponentLogsList = quizDetails.QuizComponentLogs.Where(r => r.ObjectTypeId == (int)BranchingLogicEnum.RESULT || r.ObjectTypeId == (int)BranchingLogicEnum.ACTION);

                                foreach (var obj in quizResults)
                                {
                                    var quizComponentLogs = quizComponentLogsList.Where(r => r.PublishedObjectId == obj.Id);

                                    var result = new QuizResultAndAction.Result();
                                    result.ParentResultId = quizComponentLogs.Any(r => r.ObjectTypeId == (int)BranchingLogicEnum.RESULT) ? quizComponentLogs.FirstOrDefault(r => r.ObjectTypeId == (int)BranchingLogicEnum.RESULT).DraftedObjectId : 0;
                                    result.Id = obj.Id;
                                    result.Title = obj.Title;
                                    result.InternalTitle = obj.InternalTitle ?? string.Empty;
                                    result.Description = obj.Description;

                                    if (quizDetails.IsBranchingLogicEnabled.HasValue && quizDetails.IsBranchingLogicEnabled.Value)
                                    {
                                        var branchingLogic = quizDetails.BranchingLogic.FirstOrDefault(r => r.SourceTypeId == (int)BranchingLogicEnum.RESULTNEXT && r.SourceObjectId == obj.Id && r.DestinationTypeId == (int)BranchingLogicEnum.ACTION);
                                        if (branchingLogic != null)
                                        {
                                            var quizAction = quizDetails.ActionsInQuiz.FirstOrDefault(r => r.Id == branchingLogic.DestinationObjectId);
                                            if (quizAction != null)
                                            {
                                                var action = new QuizResultAndAction.Action();
                                                action.ParentActionId = quizComponentLogsList.Any(r => r.PublishedObjectId == quizAction.Id && r.ObjectTypeId == (int)BranchingLogicEnum.ACTION) ? quizComponentLogsList.FirstOrDefault(r => r.PublishedObjectId == quizAction.Id && r.ObjectTypeId == (int)BranchingLogicEnum.ACTION).DraftedObjectId : 0;
                                                action.Id = quizAction.Id;
                                                action.ActionType = Enum.GetName(typeof(ActionTypeEnum), quizAction.ActionType);
                                                action.Title = quizAction.Title;
                                                action.AppointmentId = (appointmentTypeDetails == null || quizAction.AppointmentId == 0) ? null : quizAction.AppointmentId;
                                                action.AppointmentName = (appointmentTypeDetails == null || quizAction.AppointmentId == 0) ? null : appointmentTypeDetails.Data.FirstOrDefault(t => t.Id == quizAction.AppointmentId).AppointmentTypeName;
                                                action.ReportEmails = quizAction.ReportEmails;
                                                result.ActionDetail = action;
                                            }
                                        }
                                    }

                                    if (AllResultRequired || (result.ActionDetail != null))
                                        resultLst.Add(result);
                                }
                            }


                            quizActionResults.Results = resultLst;

                            return quizActionResults;
                        }
                        else
                        {
                            Status = ResultEnum.Error;
                            ErrorMessage = "Quiz is not yet published.";
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz not found for the Quiz Id " + QuizId;
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = $@"GetQuizResultAndAction {ex.Message}"; ;
                throw ex;
            }
            return null;
        }

        public QuizResultAndAction GetQuizAllResultAndAction(int QuizId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizActionResults = new QuizResultAndAction();

                    var quiz = UOWObj.QuizRepository.GetByID(QuizId);

                    if (quiz != null)
                    {
                        var resultLst = new List<QuizResultAndAction.Result>();

                        var quizDetails = quiz.QuizDetails.LastOrDefault(r => r.State == (int)QuizStateEnum.PUBLISHED);
                        if (quizDetails != null)
                        {

                            var quizResults = quizDetails.QuizResults.Where(r => r.State == (int)QuizStateEnum.PUBLISHED && r.Status == (int)StatusEnum.Active);

                            var ActionsInQuizIds = String.Join(",", quizDetails.ActionsInQuiz.Where(t => t.Status == (int)StatusEnum.Active && t.AppointmentId != 0).Select(s => s.AppointmentId).ToList());

                            QuizApp.Response.AppointmentTypeList appointmentTypeDetails = null;

                            if (!string.IsNullOrWhiteSpace(ActionsInQuizIds))
                                appointmentTypeDetails = AppointmentHelper.GetAppointmentTypeDetailsList(ActionsInQuizIds);

                            resultLst.Add(new QuizResultAndAction.Result {
                                Id = -1,
                                Title = "Not Completed",
                                InternalTitle = "Not Completed"
                            });

                            var quizComponentLogsList = quizDetails.QuizComponentLogs.Where(r => r.ObjectTypeId == (int)BranchingLogicEnum.RESULT || r.ObjectTypeId == (int)BranchingLogicEnum.ACTION);
                            var usageType = quiz.UsageTypeInQuiz.Where(v => v.QuizId == QuizId && v.UsageType == (int)UsageTypeEnum.WhatsAppChatbot);
                            if (usageType != null && usageType.Any()) {
                                resultLst.Add(new QuizResultAndAction.Result {
                                    Id = -2,
                                    Title = "Whatsapp not delivered",
                                    InternalTitle = "Whatsapp not delivered"
                                });
                            }
                           

                            foreach (var obj in quizResults)
                            {
                                var quizComponentLogs = quizComponentLogsList.Where(r => r.PublishedObjectId == obj.Id);
                                var result = new QuizResultAndAction.Result();
                              
                                result.ParentResultId = quizComponentLogs.Any(r => r.ObjectTypeId == (int)BranchingLogicEnum.RESULT) ? quizComponentLogs.FirstOrDefault(r => r.ObjectTypeId == (int)BranchingLogicEnum.RESULT).DraftedObjectId : 0;
                                result.Id = obj.Id;
                                result.Title = obj.Title;
                                result.InternalTitle = obj.InternalTitle ?? string.Empty;
                                result.Description = obj.Description;
                                result.IsLinkedResult = false;

                                if (quizDetails.IsBranchingLogicEnabled.HasValue && quizDetails.IsBranchingLogicEnabled.Value)
                                {
                                    var resultsInBranchingLogic = quizDetails.BranchingLogic.FirstOrDefault(r => r.DestinationTypeId == (int)BranchingLogicEnum.RESULT && r.DestinationObjectId == obj.Id);
                                    result.IsLinkedResult = resultsInBranchingLogic != null ? true : false;

                                    var branchingLogic = quizDetails.BranchingLogic.FirstOrDefault(r => r.SourceTypeId == (int)BranchingLogicEnum.RESULTNEXT && r.SourceObjectId == obj.Id && r.DestinationTypeId == (int)BranchingLogicEnum.ACTION);
                                    if (branchingLogic != null)
                                    {
                                        var quizAction = quizDetails.ActionsInQuiz.FirstOrDefault(r => r.Id == branchingLogic.DestinationObjectId);
                                        if (quizAction != null)
                                        {
                                            var action = new QuizResultAndAction.Action();
                                            action.Id = quizAction.Id;
                                            action.ParentActionId = quizComponentLogsList.Any(r => r.PublishedObjectId == quizAction.Id && r.ObjectTypeId == (int)BranchingLogicEnum.ACTION) ? quizComponentLogsList.FirstOrDefault(r => r.PublishedObjectId == quizAction.Id && r.ObjectTypeId == (int)BranchingLogicEnum.ACTION).DraftedObjectId : 0;
                                            action.ActionType = Enum.GetName(typeof(ActionTypeEnum), quizAction.ActionType);
                                            action.Title = quizAction.Title;
                                            action.AppointmentId = (appointmentTypeDetails == null || quizAction.AppointmentId == 0) ? null : quizAction.AppointmentId;
                                            action.AppointmentName = (appointmentTypeDetails == null || quizAction.AppointmentId == 0) ? null : appointmentTypeDetails.Data.FirstOrDefault(t => t.Id == quizAction.AppointmentId).AppointmentTypeName;
                                            action.ReportEmails = quizAction.ReportEmails;
                                            result.ActionDetail = action;
                                        }
                                    }
                                }

                                resultLst.Add(result);
                            }
                            quizActionResults.Results = resultLst;

                            return quizActionResults;
                        }
                        else
                        {
                            Status = ResultEnum.Error;
                            ErrorMessage = "Quiz is not yet published.";
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz not found for the Quiz Id " + QuizId;
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
            return null;
        }

        //public void UpdateAnswerType(int QuestionId, int AnswerType, int BusinessUserId, int CompanyId, int? answerStructureType, bool isWhatsappEnable = false, bool isMultiRating = false)
        //{

        //    if (isWhatsappEnable)
        //    {
        //        UpdateAnswerTypeWhatsApp(QuestionId, AnswerType, BusinessUserId, CompanyId, answerStructureType, true);
        //        return;
        //    }

        //    try
        //    {
        //        using (var UOWObj = new AutomationUnitOfWork())
        //        {
        //            var existingQuestion = UOWObj.QuestionsInQuizRepository.GetByID(QuestionId);


        //            if (existingQuestion != null)
        //            {
        //                var quiz = existingQuestion.QuizDetails.Quiz;

        //                if (existingQuestion.AnswerType != AnswerType)
        //                {
        //                    var currentDate = DateTime.UtcNow;

        //                    var branchingLogic = UOWObj.BranchingLogicRepository.Get(r => r.SourceObjectId == QuestionId && r.SourceTypeId == (int)BranchingLogicEnum.QUESTIONNEXT).FirstOrDefault();

        //                    if (branchingLogic != null)
        //                    {
        //                        UOWObj.BranchingLogicRepository.Delete(branchingLogic);
        //                    }


        //                    //other(not multiple) to sigle
        //                    //other(not sigle) to multiple
        //                    //other to DrivingLicense,LookingforJobs, FullAddress
        //                    if ((existingQuestion.AnswerType != (int)AnswerTypeEnum.Multiple && AnswerType == (int)AnswerTypeEnum.Single) || (existingQuestion.AnswerType != (int)AnswerTypeEnum.Single && AnswerType == (int)AnswerTypeEnum.Multiple) || AnswerType == (int)AnswerTypeEnum.DrivingLicense || AnswerType == (int)AnswerTypeEnum.LookingforJobs || AnswerType == (int)AnswerTypeEnum.FullAddress || (AnswerType == (int)AnswerTypeEnum.RatingEmoji && isMultiRating) || (AnswerType == (int)AnswerTypeEnum.RatingStarts && isMultiRating) || (AnswerType == (int)AnswerTypeEnum.Availability))
        //                    {
        //                        #region update answer type  to other

        //                        foreach (var obj in existingQuestion.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active))
        //                            RemoveAnswer(obj.Id, BusinessUserId, CompanyId, AnswerType, UOWObj);

        //                        existingQuestion.EnableComment = false;

        //                        if (AnswerType == (int)AnswerTypeEnum.DrivingLicense)
        //                        {
        //                            #region to add answer option for driving license

        //                            existingQuestion.Question = "Your driving license";
        //                            existingQuestion.RevealCorrectAnswer = false;

        //                            string[] drivingLicenseOptions = new string[] { "A", "B", "BE", "C", "CE", "D", "THE", "G", "No driving license" };

        //                            foreach (var obj in drivingLicenseOptions)
        //                            {
        //                                var answerObj = new Db.AnswerOptionsInQuizQuestions();
        //                                answerObj.QuestionId = QuestionId;
        //                                answerObj.QuizId = existingQuestion.QuizId;
        //                                answerObj.Option = obj;
        //                                answerObj.OptionImage = string.Empty;
        //                                answerObj.PublicId = string.Empty;
        //                                answerObj.LastUpdatedBy = BusinessUserId;
        //                                answerObj.IsCorrectAnswer = true;
        //                                answerObj.IsCorrectForMultipleAnswer = false;
        //                                if (existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
        //                                    answerObj.AssociatedScore = default(int);
        //                                answerObj.LastUpdatedOn = currentDate;
        //                                answerObj.DisplayOrder = existingQuestion.AnswerOptionsInQuizQuestions.Count(a => a.Status == (int)StatusEnum.Active) + 1;
        //                                answerObj.Status = (int)StatusEnum.Active;
        //                                answerObj.State = (int)QuizStateEnum.DRAFTED;
        //                                answerObj.IsReadOnly = true;

        //                                UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);
        //                            }

        //                            #endregion
        //                        }
        //                        else if (AnswerType == (int)AnswerTypeEnum.LookingforJobs)
        //                        {
        //                            #region to add answer option for LookingforJobs

        //                            existingQuestion.Question = "Are you still looking for jobs ?";
        //                            existingQuestion.RevealCorrectAnswer = false;

        //                            string[] LookingforJobsOptions = new string[] { "Yes", "No", "No, but open to good suggestions" };

        //                            foreach (var obj in LookingforJobsOptions)
        //                            {
        //                                var answerObj = new Db.AnswerOptionsInQuizQuestions();
        //                                answerObj.QuestionId = QuestionId;
        //                                answerObj.QuizId = existingQuestion.QuizId;
        //                                answerObj.Option = obj;
        //                                answerObj.OptionImage = string.Empty;
        //                                answerObj.PublicId = string.Empty;
        //                                answerObj.LastUpdatedBy = BusinessUserId;
        //                                answerObj.IsCorrectAnswer = true;
        //                                answerObj.IsCorrectForMultipleAnswer = false;
        //                                if (existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
        //                                    answerObj.AssociatedScore = default(int);
        //                                answerObj.LastUpdatedOn = currentDate;
        //                                answerObj.DisplayOrder = existingQuestion.AnswerOptionsInQuizQuestions.Count(a => a.Status == (int)StatusEnum.Active) + 1;
        //                                answerObj.Status = (int)StatusEnum.Active;
        //                                answerObj.State = (int)QuizStateEnum.DRAFTED;
        //                                answerObj.IsReadOnly = true;

        //                                UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);
        //                            }

        //                            #endregion
        //                        }
        //                        else if (AnswerType == (int)AnswerTypeEnum.FullAddress)
        //                        {
        //                            #region to add answer option for FullAddress

        //                            existingQuestion.Question = "Your full address";
        //                            existingQuestion.RevealCorrectAnswer = false;

        //                            string[] fullAddressOptions = new string[] { "Post Code", "House Number" };

        //                            foreach (var obj in fullAddressOptions)
        //                            {
        //                                var answerObj = new Db.AnswerOptionsInQuizQuestions();
        //                                answerObj.QuestionId = QuestionId;
        //                                answerObj.QuizId = existingQuestion.QuizId;
        //                                answerObj.Option = obj;
        //                                answerObj.OptionImage = string.Empty;
        //                                answerObj.PublicId = string.Empty;
        //                                answerObj.LastUpdatedBy = BusinessUserId;
        //                                answerObj.IsCorrectAnswer = true;
        //                                answerObj.IsCorrectForMultipleAnswer = false;
        //                                if (existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
        //                                    answerObj.AssociatedScore = default(int);
        //                                answerObj.LastUpdatedOn = currentDate;
        //                                answerObj.DisplayOrder = existingQuestion.AnswerOptionsInQuizQuestions.Count(a => a.Status == (int)StatusEnum.Active) + 1;
        //                                answerObj.Status = (int)StatusEnum.Active;
        //                                answerObj.State = (int)QuizStateEnum.DRAFTED;
        //                                answerObj.IsReadOnly = false;

        //                                UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);
        //                            }

        //                            #endregion
        //                        }

        //                        else if (AnswerType == (int)AnswerTypeEnum.RatingEmoji || AnswerType == (int)AnswerTypeEnum.RatingStarts)
        //                        {
        //                            #region to add answer option for RatingEmoji & RatingStarts

        //                            if (isMultiRating)
        //                            {

        //                                string[] MultiRating = new string[] { "OptionTextforRatingOne", "OptionTextforRatingTwo", "OptionTextforRatingThree", "OptionTextforRatingFour", "OptionTextforRatingFive" };

        //                                foreach (var obj in MultiRating)
        //                                {
        //                                    var answerObj = new Db.AnswerOptionsInQuizQuestions();
        //                                    answerObj.QuestionId = QuestionId;
        //                                    answerObj.QuizId = existingQuestion.QuizId;
        //                                    answerObj.Option = null;
        //                                    answerObj.OptionImage = string.Empty;
        //                                    answerObj.PublicId = string.Empty;
        //                                    answerObj.LastUpdatedBy = BusinessUserId;
        //                                    if (existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
        //                                        answerObj.AssociatedScore = default(int);
        //                                    answerObj.LastUpdatedOn = currentDate;
        //                                    answerObj.DisplayOrder = existingQuestion.AnswerOptionsInQuizQuestions.Count(a => a.Status == (int)StatusEnum.Active) + 1;
        //                                    answerObj.Status = (int)StatusEnum.Active;
        //                                    answerObj.State = (int)QuizStateEnum.DRAFTED;
        //                                    answerObj.IsReadOnly = false;

        //                                    UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);
        //                                }
        //                            }

        //                            #endregion
        //                        }
        //                        else if(AnswerType == (int)AnswerTypeEnum.Availability)
        //                        {
        //                            var answerObj = new Db.AnswerOptionsInQuizQuestions();

        //                            answerObj.QuestionId = existingQuestion.Id;
        //                            answerObj.QuizId = existingQuestion.QuizId;
        //                            answerObj.Option = "";
        //                            answerObj.OptionImage = string.Empty;
        //                            answerObj.PublicId = string.Empty;
        //                            answerObj.LastUpdatedBy = BusinessUserId;
        //                            answerObj.LastUpdatedOn = currentDate;

        //                            if (quiz.QuizType == (int)QuizTypeEnum.Score || quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
        //                            {
        //                                answerObj.AssociatedScore = default(int);
        //                                answerObj.IsCorrectAnswer = false;
        //                            }
        //                            else if (quiz.QuizType == (int)QuizTypeEnum.Personality || quiz.QuizType == (int)QuizTypeEnum.PersonalityTemplate)
        //                                answerObj.IsCorrectAnswer = false;
        //                            else
        //                                answerObj.IsCorrectAnswer = true;

        //                            answerObj.DisplayOrder = 1;
        //                            answerObj.Status = (int)StatusEnum.Active;
        //                            answerObj.State = (int)QuizStateEnum.DRAFTED;
        //                            answerObj.AutoPlay = true;
        //                            answerObj.SecondsToApply = "0";
        //                            answerObj.VideoFrameEnabled = false;

        //                            UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);

        //                            answerObj = new Db.AnswerOptionsInQuizQuestions();

        //                            answerObj.QuestionId = existingQuestion.Id;
        //                            answerObj.QuizId = existingQuestion.QuizId;
        //                            answerObj.Option = "";
        //                            answerObj.OptionImage = string.Empty;
        //                            answerObj.PublicId = string.Empty;
        //                            answerObj.LastUpdatedBy = BusinessUserId;
        //                            answerObj.LastUpdatedOn = currentDate;

        //                            if (quiz.QuizType == (int)QuizTypeEnum.Score || quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
        //                            {
        //                                answerObj.AssociatedScore = default(int);
        //                                answerObj.IsCorrectAnswer = false;
        //                            }
        //                            else
        //                                answerObj.IsCorrectAnswer = false;

        //                            answerObj.DisplayOrder = 2;
        //                            answerObj.Status = (int)StatusEnum.Active;
        //                            answerObj.State = (int)QuizStateEnum.DRAFTED;
        //                            answerObj.AutoPlay = true;
        //                            answerObj.SecondsToApply = "0";
        //                            answerObj.VideoFrameEnabled = false;

        //                            UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);

        //                            answerObj = new Db.AnswerOptionsInQuizQuestions();

        //                            answerObj.QuestionId = existingQuestion.Id;
        //                            answerObj.QuizId = existingQuestion.QuizId;
        //                            answerObj.Option = "";
        //                            answerObj.OptionImage = string.Empty;
        //                            answerObj.PublicId = string.Empty;
        //                            answerObj.LastUpdatedBy = BusinessUserId;
        //                            answerObj.LastUpdatedOn = currentDate;

        //                            if (quiz.QuizType == (int)QuizTypeEnum.Score || quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
        //                            {
        //                                answerObj.AssociatedScore = default(int);
        //                                answerObj.IsCorrectAnswer = false;
        //                            }
        //                            else if (quiz.QuizType == (int)QuizTypeEnum.Personality || quiz.QuizType == (int)QuizTypeEnum.PersonalityTemplate)
        //                                answerObj.IsCorrectAnswer = false;
        //                            else
        //                                answerObj.IsCorrectAnswer = true;

        //                            answerObj.DisplayOrder = 1;
        //                            answerObj.Status = (int)StatusEnum.Active;
        //                            answerObj.State = (int)QuizStateEnum.DRAFTED;
        //                            answerObj.AutoPlay = true;
        //                            answerObj.SecondsToApply = "0";
        //                            answerObj.VideoFrameEnabled = false;

        //                            UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);
        //                            UOWObj.Save();
        //                        }
        //                        else
        //                        {
        //                            #region to add answer option for Single and Multiple type

        //                            var answerObj = new Db.AnswerOptionsInQuizQuestions();

        //                            if (existingQuestion.AnswerType == (int)AnswerTypeEnum.DOB || existingQuestion.AnswerType == (int)AnswerTypeEnum.PostCode || existingQuestion.AnswerType == (int)AnswerTypeEnum.DrivingLicense || existingQuestion.AnswerType == (int)AnswerTypeEnum.LookingforJobs || existingQuestion.AnswerType == (int)AnswerTypeEnum.FullAddress)
        //                                existingQuestion.Question = "Question" + (existingQuestion.QuizDetails.QuestionsInQuiz.Count(a => a.Status == (int)StatusEnum.Active && a.Type == (int)BranchingLogicEnum.QUESTION)).ToString();
        //                            answerObj.QuestionId = QuestionId;
        //                            answerObj.QuizId = existingQuestion.QuizId;
        //                            answerObj.Option = "Answer 1";
        //                            answerObj.OptionImage = string.Empty;
        //                            answerObj.PublicId = string.Empty;
        //                            answerObj.LastUpdatedBy = BusinessUserId;
        //                            answerObj.IsCorrectAnswer = true;
        //                            answerObj.IsCorrectForMultipleAnswer = false;
        //                            if (existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
        //                                answerObj.AssociatedScore = default(int);
        //                            answerObj.LastUpdatedOn = currentDate;
        //                            answerObj.DisplayOrder = 1;
        //                            answerObj.Status = (int)StatusEnum.Active;
        //                            answerObj.State = (int)QuizStateEnum.DRAFTED;
        //                            answerObj.IsReadOnly = false;

        //                            UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);

        //                            answerObj = new Db.AnswerOptionsInQuizQuestions();

        //                            answerObj.QuestionId = QuestionId;
        //                            answerObj.QuizId = existingQuestion.QuizId;
        //                            answerObj.Option = "Answer 2";
        //                            answerObj.OptionImage = string.Empty;
        //                            answerObj.PublicId = string.Empty;
        //                            answerObj.LastUpdatedBy = BusinessUserId;
        //                            answerObj.LastUpdatedOn = currentDate;
        //                            answerObj.DisplayOrder = 2;
        //                            answerObj.Status = (int)StatusEnum.Active;
        //                            answerObj.State = (int)QuizStateEnum.DRAFTED;
        //                            answerObj.IsCorrectAnswer = false;
        //                            answerObj.IsCorrectForMultipleAnswer = false;
        //                            answerObj.IsReadOnly = false;
        //                            if (existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
        //                                answerObj.AssociatedScore = default(int);

        //                            UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);

        //                            if (AnswerType == (int)AnswerTypeEnum.Single)
        //                            {
        //                                answerObj = new Db.AnswerOptionsInQuizQuestions();

        //                                answerObj.QuestionId = QuestionId;
        //                                answerObj.QuizId = existingQuestion.QuizId;
        //                                answerObj.Option = "Unanswered";
        //                                answerObj.OptionImage = string.Empty;
        //                                answerObj.PublicId = string.Empty;
        //                                answerObj.LastUpdatedBy = BusinessUserId;
        //                                answerObj.LastUpdatedOn = currentDate;
        //                                answerObj.DisplayOrder = 0;
        //                                answerObj.Status = (int)StatusEnum.Active;
        //                                answerObj.State = (int)QuizStateEnum.DRAFTED;
        //                                answerObj.IsCorrectAnswer = false;
        //                                answerObj.IsCorrectForMultipleAnswer = false;
        //                                answerObj.IsReadOnly = false;
        //                                answerObj.IsUnansweredType = true;
        //                                if (existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
        //                                    answerObj.AssociatedScore = default(int);

        //                                UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);
        //                            }

        //                            #endregion
        //                        }

        //                        #endregion
        //                    }


        //                    //sigle, multiple, DrivingLicense,LookingforJobs, FullAddress to Short, Long, DOB, PostCode, NPS, RatingEmoji, RatingStarts
        //                    else if ((existingQuestion.AnswerType == (int)AnswerTypeEnum.Single || existingQuestion.AnswerType == (int)AnswerTypeEnum.Multiple || existingQuestion.AnswerType == (int)AnswerTypeEnum.DrivingLicense || existingQuestion.AnswerType == (int)AnswerTypeEnum.LookingforJobs || existingQuestion.AnswerType == (int)AnswerTypeEnum.FullAddress) && (AnswerType == (int)AnswerTypeEnum.Short || AnswerType == (int)AnswerTypeEnum.Long || AnswerType == (int)AnswerTypeEnum.DOB || AnswerType == (int)AnswerTypeEnum.PostCode || AnswerType == (int)AnswerTypeEnum.NPS || AnswerType == (int)AnswerTypeEnum.RatingEmoji || AnswerType == (int)AnswerTypeEnum.RatingStarts))
        //                    {
        //                        #region update answer type other to text

        //                        foreach (var obj in existingQuestion.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active))
        //                            RemoveAnswer(obj.Id, BusinessUserId, CompanyId, AnswerType, null);

        //                        var answerObj = new Db.AnswerOptionsInQuizQuestions();
        //                        existingQuestion.RevealCorrectAnswer = false;
        //                        answerObj.QuestionId = QuestionId;
        //                        answerObj.QuizId = existingQuestion.QuizId;
        //                        switch (AnswerType)
        //                        {
        //                            case (int)AnswerTypeEnum.Short:
        //                                answerObj.Option = "Short answer text...";
        //                                if (existingQuestion.AnswerType == (int)AnswerTypeEnum.DrivingLicense || existingQuestion.AnswerType == (int)AnswerTypeEnum.LookingforJobs)
        //                                    existingQuestion.Question = "Question" + (existingQuestion.QuizDetails.QuestionsInQuiz.Count(a => a.Status == (int)StatusEnum.Active && a.Type == (int)BranchingLogicEnum.QUESTION)).ToString();
        //                                answerObj.IsReadOnly = false;
        //                                break;
        //                            case (int)AnswerTypeEnum.Long:
        //                                answerObj.Option = "Long answer text...";
        //                                if (existingQuestion.AnswerType == (int)AnswerTypeEnum.DrivingLicense || existingQuestion.AnswerType == (int)AnswerTypeEnum.LookingforJobs)
        //                                    existingQuestion.Question = "Question" + (existingQuestion.QuizDetails.QuestionsInQuiz.Count(a => a.Status == (int)StatusEnum.Active && a.Type == (int)BranchingLogicEnum.QUESTION)).ToString();
        //                                answerObj.IsReadOnly = false;
        //                                break;
        //                            case (int)AnswerTypeEnum.DOB:
        //                                answerObj.Option = null;
        //                                answerObj.IsReadOnly = false;
        //                                existingQuestion.Question = "Your date of birth";
        //                                break;
        //                            case (int)AnswerTypeEnum.PostCode:
        //                                answerObj.Option = null;
        //                                answerObj.IsReadOnly = false;
        //                                existingQuestion.Question = "Your post code";
        //                                break;
        //                            case (int)AnswerTypeEnum.NPS:
        //                                answerObj.Option = null;
        //                                answerObj.IsReadOnly = false;
        //                                existingQuestion.Question = "Question" + (existingQuestion.QuizDetails.QuestionsInQuiz.Count(a => a.Status == (int)StatusEnum.Active && a.Type == (int)BranchingLogicEnum.QUESTION)).ToString();
        //                                break;
        //                            case (int)AnswerTypeEnum.RatingEmoji:
        //                            case (int)AnswerTypeEnum.RatingStarts:

        //                                answerObj.Option = null;
        //                                answerObj.IsReadOnly = true;
        //                                existingQuestion.IsMultiRating = isMultiRating;
        //                                existingQuestion.Question = "Are you happy with the way the recruiter contacted you?";
        //                                break;
        //                        }
        //                        answerObj.OptionImage = string.Empty;
        //                        answerObj.PublicId = string.Empty;
        //                        answerObj.IsCorrectAnswer = (AnswerType == (int)AnswerTypeEnum.NPS || AnswerType == (int)AnswerTypeEnum.RatingEmoji || AnswerType == (int)AnswerTypeEnum.RatingStarts) ? false : true;
        //                        answerObj.IsCorrectForMultipleAnswer = (AnswerType == (int)AnswerTypeEnum.NPS || AnswerType == (int)AnswerTypeEnum.RatingEmoji || AnswerType == (int)AnswerTypeEnum.RatingStarts) ? false : true;
        //                        answerObj.LastUpdatedBy = BusinessUserId;
        //                        answerObj.LastUpdatedOn = currentDate;
        //                        answerObj.DisplayOrder = 1;
        //                        answerObj.Status = (int)StatusEnum.Active;
        //                        answerObj.State = (int)QuizStateEnum.DRAFTED;

        //                        UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);

        //                        #endregion
        //                    }

        //                    //Short, Long, DOB, PostCode, NPS, RatingEmoji, RatingStarts to Short, Long, DOB, PostCode, NPS, RatingEmoji, RatingStarts
        //                    else if ((existingQuestion.AnswerType == (int)AnswerTypeEnum.Short || existingQuestion.AnswerType == (int)AnswerTypeEnum.Long || existingQuestion.AnswerType == (int)AnswerTypeEnum.DOB || existingQuestion.AnswerType == (int)AnswerTypeEnum.PostCode || existingQuestion.AnswerType == (int)AnswerTypeEnum.NPS || existingQuestion.AnswerType == (int)AnswerTypeEnum.RatingEmoji || existingQuestion.AnswerType == (int)AnswerTypeEnum.RatingStarts)
        //                              && (AnswerType == (int)AnswerTypeEnum.Short || AnswerType == (int)AnswerTypeEnum.Long || AnswerType == (int)AnswerTypeEnum.DOB || AnswerType == (int)AnswerTypeEnum.PostCode || AnswerType == (int)AnswerTypeEnum.NPS || AnswerType == (int)AnswerTypeEnum.RatingEmoji || AnswerType == (int)AnswerTypeEnum.RatingStarts))
        //                    {
        //                        #region update answer type text to text

        //                        var answerObj = existingQuestion.AnswerOptionsInQuizQuestions.FirstOrDefault(r => r.Status == (int)StatusEnum.Active);
        //                        if (existingQuestion.AnswerType == (int)AnswerTypeEnum.RatingEmoji || existingQuestion.AnswerType == (int)AnswerTypeEnum.RatingStarts)
        //                        {
        //                            foreach (var obj in existingQuestion.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active))
        //                            {
        //                                if (obj.Id != answerObj.Id)
        //                                {
        //                                    RemoveAnswer(obj.Id, BusinessUserId, CompanyId, AnswerType, null);
        //                                }
        //                            }
        //                        }

        //                        existingQuestion.RevealCorrectAnswer = false;

        //                        switch (AnswerType)
        //                        {
        //                            case (int)AnswerTypeEnum.Short:
        //                                answerObj.Option = "Short answer text...";
        //                                if (existingQuestion.AnswerType == (int)AnswerTypeEnum.DOB || existingQuestion.AnswerType == (int)AnswerTypeEnum.PostCode || existingQuestion.AnswerType == (int)AnswerTypeEnum.RatingEmoji || existingQuestion.AnswerType == (int)AnswerTypeEnum.RatingStarts)
        //                                {
        //                                    existingQuestion.Question = "Question" + (existingQuestion.QuizDetails.QuestionsInQuiz.Count(a => a.Status == (int)StatusEnum.Active && a.Type == (int)BranchingLogicEnum.QUESTION)).ToString();
        //                                    existingQuestion.EnableComment = false;
        //                                }
        //                                break;
        //                            case (int)AnswerTypeEnum.Long:
        //                                answerObj.Option = "Long answer text...";
        //                                if (existingQuestion.AnswerType == (int)AnswerTypeEnum.DOB || existingQuestion.AnswerType == (int)AnswerTypeEnum.PostCode || existingQuestion.AnswerType == (int)AnswerTypeEnum.RatingEmoji || existingQuestion.AnswerType == (int)AnswerTypeEnum.RatingStarts)
        //                                {
        //                                    existingQuestion.Question = "Question" + (existingQuestion.QuizDetails.QuestionsInQuiz.Count(a => a.Status == (int)StatusEnum.Active && a.Type == (int)BranchingLogicEnum.QUESTION)).ToString();
        //                                    existingQuestion.EnableComment = false;
        //                                }
        //                                break;
        //                            case (int)AnswerTypeEnum.DOB:
        //                                answerObj.Option = null;
        //                                existingQuestion.Question = "Your date of birth";
        //                                existingQuestion.EnableComment = false;
        //                                break;
        //                            case (int)AnswerTypeEnum.PostCode:
        //                                answerObj.Option = null;
        //                                existingQuestion.Question = "Your post code";
        //                                existingQuestion.EnableComment = false;
        //                                break;
        //                            case (int)AnswerTypeEnum.NPS:
        //                                answerObj.IsCorrectAnswer = false;
        //                                answerObj.IsCorrectForMultipleAnswer = false;
        //                                answerObj.Option = null;
        //                                existingQuestion.RevealCorrectAnswer = false;
        //                                existingQuestion.Question = "Question" + (existingQuestion.QuizDetails.QuestionsInQuiz.Count(a => a.Status == (int)StatusEnum.Active && a.Type == (int)BranchingLogicEnum.QUESTION)).ToString();
        //                                existingQuestion.EnableComment = false;
        //                                break;
        //                            case (int)AnswerTypeEnum.RatingEmoji:
        //                            case (int)AnswerTypeEnum.RatingStarts:
        //                                answerObj.IsCorrectAnswer = false;
        //                                answerObj.IsCorrectForMultipleAnswer = false;
        //                                answerObj.Option = null;
        //                                answerObj.IsReadOnly = true;
        //                                existingQuestion.Question = "Are you happy with the way the recruiter contacted you?";
        //                                break;
        //                        }
        //                        UOWObj.AnswerOptionsInQuizQuestionsRepository.Update(answerObj);

        //                        #endregion
        //                    }

        //                    //Single, LookingforJobs to other
        //                    if ((existingQuestion.AnswerType == (int)AnswerTypeEnum.Single || existingQuestion.AnswerType == (int)AnswerTypeEnum.LookingforJobs) && (AnswerType != (int)AnswerTypeEnum.Single && AnswerType != (int)AnswerTypeEnum.LookingforJobs))
        //                    {
        //                        #region update answer type Single to other

        //                        var ansIds = existingQuestion.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active).ToList().Select(t => t.Id);
        //                        var branchingLogicList = UOWObj.BranchingLogicRepository.Get(r => ansIds.Any(s => s == r.SourceObjectId) && r.SourceTypeId == (int)BranchingLogicEnum.ANSWER).ToList();

        //                        if (branchingLogicList != null)
        //                        {
        //                            foreach (var obj in branchingLogicList)
        //                            {
        //                                UOWObj.BranchingLogicRepository.Delete(obj);
        //                            }
        //                        }

        //                        #endregion
        //                    }

        //                    //other to Short, Long, DOB, PostCode, NPS, RatingEmoji, RatingStarts
        //                    if ((existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate) && (existingQuestion.AnswerType == (int)AnswerTypeEnum.Multiple || existingQuestion.AnswerType == (int)AnswerTypeEnum.DrivingLicense) && (AnswerType == (int)AnswerTypeEnum.Single || AnswerType == (int)AnswerTypeEnum.DOB || AnswerType == (int)AnswerTypeEnum.PostCode || AnswerType == (int)AnswerTypeEnum.FullAddress || AnswerType == (int)AnswerTypeEnum.LookingforJobs || AnswerType == (int)AnswerTypeEnum.RatingStarts || AnswerType == (int)AnswerTypeEnum.RatingEmoji))
        //                    {
        //                        existingQuestion.MinAnswer = 1;
        //                        existingQuestion.MaxAnswer = 1;
        //                    }

        //                    if ((existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate) && (AnswerType == (int)AnswerTypeEnum.DrivingLicense || AnswerType == (int)AnswerTypeEnum.Multiple))
        //                    {
        //                        existingQuestion.MinAnswer = 0;
        //                        existingQuestion.MaxAnswer = existingQuestion.AnswerOptionsInQuizQuestions.Count(r => r.Status == (int)StatusEnum.Active);
        //                    }

        //                    if (AnswerType != (int)AnswerTypeEnum.Single || AnswerType != (int)AnswerTypeEnum.Multiple || AnswerType != (int)AnswerTypeEnum.Short || AnswerType != (int)AnswerTypeEnum.Long)
        //                    {
        //                        existingQuestion.TimerRequired = false;
        //                        existingQuestion.Time = null;
        //                    }

        //                    if (AnswerType != (int)AnswerTypeEnum.Single && AnswerType != (int)AnswerTypeEnum.Multiple)
        //                    {
        //                        existingQuestion.ShowAnswerImage = false;
        //                    }
        //                    existingQuestion.IsMultiRating = isMultiRating;
        //                    existingQuestion.AnswerType = AnswerType;
        //                    existingQuestion.LastUpdatedBy = BusinessUserId;
        //                    existingQuestion.LastUpdatedOn = currentDate;

        //                    existingQuestion.QuizDetails.LastUpdatedBy = BusinessUserId;
        //                    existingQuestion.QuizDetails.LastUpdatedOn = currentDate;

        //                    existingQuestion.QuizDetails.Quiz.State = (int)QuizStateEnum.DRAFTED;
        //                    existingQuestion.AutoPlay = ((existingQuestion.AnswerType == (int)AnswerTypeEnum.Single || existingQuestion.AnswerType == (int)AnswerTypeEnum.Multiple) && existingQuestion.AutoPlay == true) ? true : existingQuestion.AutoPlay;


        //                    UOWObj.QuizRepository.Update(existingQuestion.QuizDetails.Quiz);
        //                    UOWObj.Save();

        //                    
        //                    
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Status = ResultEnum.Error;
        //        ErrorMessage = ex.Message;
        //        throw ex;
        //    }
        //}

        //public void UpdateAnswerTypeWhatsApp(int QuestionId, int AnswerType, int BusinessUserId, int CompanyId, int? answerStructureType, bool isWhatsappEnable)
        //{
        //    try
        //    {
        //        using (var UOWObj = new AutomationUnitOfWork())
        //        {
        //            var existingQuestion = UOWObj.QuestionsInQuizRepository.GetByID(QuestionId);
        //            var currentDate = DateTime.UtcNow;

        //            if (existingQuestion != null)
        //            {
        //                var quiz = existingQuestion.QuizDetails.Quiz;

        //                if (existingQuestion.AnswerType != AnswerType)
        //                {

        //                    var branchingLogic = UOWObj.BranchingLogicRepository.Get(r => r.SourceObjectId == QuestionId && r.SourceTypeId == (int)BranchingLogicEnum.QUESTIONNEXT).FirstOrDefault();

        //                    if (branchingLogic != null)
        //                    {
        //                        UOWObj.BranchingLogicRepository.Delete(branchingLogic);
        //                    }

        //                    if ((AnswerType == (int)AnswerTypeEnum.Single || AnswerType == (int)AnswerTypeEnum.Multiple))
        //                    {
        //                        existingQuestion.EnableComment = false;

        //                        SetSingleMultipleAnswerTYpe(QuestionId, AnswerType, BusinessUserId, CompanyId, UOWObj, existingQuestion, currentDate);
        //                    }


        //                    //other(not multiple) to sigle
        //                    //other(not sigle) to multiple
        //                    //other to DrivingLicense,LookingforJobs, FullAddress
        //                    else if (AnswerType == (int)AnswerTypeEnum.DrivingLicense || AnswerType == (int)AnswerTypeEnum.LookingforJobs || AnswerType == (int)AnswerTypeEnum.FullAddress)
        //                    {
        //                        #region update answer type  to other

        //                        foreach (var obj in existingQuestion.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active))
        //                            RemoveAnswer(obj.Id, BusinessUserId, CompanyId, AnswerType, UOWObj);

        //                        existingQuestion.EnableComment = false;

        //                        if (AnswerType == (int)AnswerTypeEnum.DrivingLicense)
        //                        {
        //                            #region to add answer option for driving license

        //                            existingQuestion.Question = "Your driving license";
        //                            existingQuestion.RevealCorrectAnswer = false;

        //                            string[] drivingLicenseOptions = new string[] { "A", "B", "BE", "C", "CE", "D", "THE", "G", "No driving license" };

        //                            foreach (var obj in drivingLicenseOptions)
        //                            {
        //                                var answerObj = new Db.AnswerOptionsInQuizQuestions();
        //                                answerObj.QuestionId = QuestionId;
        //                                answerObj.QuizId = existingQuestion.QuizId;
        //                                answerObj.Option = obj;
        //                                answerObj.OptionImage = string.Empty;
        //                                answerObj.PublicId = string.Empty;
        //                                answerObj.LastUpdatedBy = BusinessUserId;
        //                                answerObj.IsCorrectAnswer = true;
        //                                answerObj.IsCorrectForMultipleAnswer = false;
        //                                if (existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
        //                                    answerObj.AssociatedScore = default(int);
        //                                answerObj.LastUpdatedOn = currentDate;
        //                                answerObj.DisplayOrder = existingQuestion.AnswerOptionsInQuizQuestions.Count(a => a.Status == (int)StatusEnum.Active) + 1;
        //                                answerObj.Status = (int)StatusEnum.Active;
        //                                answerObj.State = (int)QuizStateEnum.DRAFTED;
        //                                answerObj.IsReadOnly = true;

        //                                UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);
        //                            }

        //                            #endregion
        //                        }
        //                        else if (AnswerType == (int)AnswerTypeEnum.LookingforJobs)
        //                        {
        //                            #region to add answer option for LookingforJobs

        //                            existingQuestion.Question = "Are you still looking for jobs ?";
        //                            existingQuestion.RevealCorrectAnswer = false;

        //                            string[] LookingforJobsOptions = new string[] { "Yes", "No", "No, but open to good suggestions" };

        //                            foreach (var obj in LookingforJobsOptions)
        //                            {
        //                                var answerObj = new Db.AnswerOptionsInQuizQuestions();
        //                                answerObj.QuestionId = QuestionId;
        //                                answerObj.QuizId = existingQuestion.QuizId;
        //                                answerObj.Option = obj;
        //                                answerObj.OptionImage = string.Empty;
        //                                answerObj.PublicId = string.Empty;
        //                                answerObj.LastUpdatedBy = BusinessUserId;
        //                                answerObj.IsCorrectAnswer = true;
        //                                answerObj.IsCorrectForMultipleAnswer = false;
        //                                if (existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
        //                                    answerObj.AssociatedScore = default(int);
        //                                answerObj.LastUpdatedOn = currentDate;
        //                                answerObj.DisplayOrder = existingQuestion.AnswerOptionsInQuizQuestions.Count(a => a.Status == (int)StatusEnum.Active) + 1;
        //                                answerObj.Status = (int)StatusEnum.Active;
        //                                answerObj.State = (int)QuizStateEnum.DRAFTED;
        //                                answerObj.IsReadOnly = true;

        //                                UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);
        //                            }

        //                            #endregion
        //                        }
        //                        else if (AnswerType == (int)AnswerTypeEnum.FullAddress)
        //                        {
        //                            #region to add answer option for FullAddress

        //                            existingQuestion.Question = "Your full address";
        //                            existingQuestion.RevealCorrectAnswer = false;

        //                            string[] fullAddressOptions = new string[] { "Post Code", "House Number" };

        //                            foreach (var obj in fullAddressOptions)
        //                            {
        //                                var answerObj = new Db.AnswerOptionsInQuizQuestions();
        //                                answerObj.QuestionId = QuestionId;
        //                                answerObj.QuizId = existingQuestion.QuizId;
        //                                answerObj.Option = obj;
        //                                answerObj.OptionImage = string.Empty;
        //                                answerObj.PublicId = string.Empty;
        //                                answerObj.LastUpdatedBy = BusinessUserId;
        //                                answerObj.IsCorrectAnswer = true;
        //                                answerObj.IsCorrectForMultipleAnswer = false;
        //                                if (existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
        //                                    answerObj.AssociatedScore = default(int);
        //                                answerObj.LastUpdatedOn = currentDate;
        //                                answerObj.DisplayOrder = existingQuestion.AnswerOptionsInQuizQuestions.Count(a => a.Status == (int)StatusEnum.Active) + 1;
        //                                answerObj.Status = (int)StatusEnum.Active;
        //                                answerObj.State = (int)QuizStateEnum.DRAFTED;
        //                                answerObj.IsReadOnly = false;

        //                                UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);
        //                            }

        //                            #endregion
        //                        }
        //                        else if (AnswerType == (int)AnswerTypeEnum.Availability)
        //                        {
        //                            var answerObj = new Db.AnswerOptionsInQuizQuestions();

        //                            answerObj.QuestionId = existingQuestion.Id;
        //                            answerObj.QuizId = existingQuestion.QuizId;
        //                            answerObj.Option = "";
        //                            answerObj.OptionImage = string.Empty;
        //                            answerObj.PublicId = string.Empty;
        //                            answerObj.LastUpdatedBy = BusinessUserId;
        //                            answerObj.LastUpdatedOn = currentDate;

        //                            if (quiz.QuizType == (int)QuizTypeEnum.Score || quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
        //                            {
        //                                answerObj.AssociatedScore = default(int);
        //                                answerObj.IsCorrectAnswer = false;
        //                            }
        //                            else if (quiz.QuizType == (int)QuizTypeEnum.Personality || quiz.QuizType == (int)QuizTypeEnum.PersonalityTemplate)
        //                                answerObj.IsCorrectAnswer = false;
        //                            else
        //                                answerObj.IsCorrectAnswer = true;

        //                            answerObj.DisplayOrder = 1;
        //                            answerObj.Status = (int)StatusEnum.Active;
        //                            answerObj.State = (int)QuizStateEnum.DRAFTED;
        //                            answerObj.AutoPlay = true;
        //                            answerObj.SecondsToApply = "0";
        //                            answerObj.VideoFrameEnabled = false;

        //                            UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);

        //                            answerObj = new Db.AnswerOptionsInQuizQuestions();

        //                            answerObj.QuestionId = existingQuestion.Id;
        //                            answerObj.QuizId = existingQuestion.QuizId;
        //                            answerObj.Option = "";
        //                            answerObj.OptionImage = string.Empty;
        //                            answerObj.PublicId = string.Empty;
        //                            answerObj.LastUpdatedBy = BusinessUserId;
        //                            answerObj.LastUpdatedOn = currentDate;

        //                            if (quiz.QuizType == (int)QuizTypeEnum.Score || quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
        //                            {
        //                                answerObj.AssociatedScore = default(int);
        //                                answerObj.IsCorrectAnswer = false;
        //                            }
        //                            else
        //                                answerObj.IsCorrectAnswer = false;

        //                            answerObj.DisplayOrder = 2;
        //                            answerObj.Status = (int)StatusEnum.Active;
        //                            answerObj.State = (int)QuizStateEnum.DRAFTED;
        //                            answerObj.AutoPlay = true;
        //                            answerObj.SecondsToApply = "0";
        //                            answerObj.VideoFrameEnabled = false;

        //                            UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);

        //                            answerObj = new Db.AnswerOptionsInQuizQuestions();

        //                            answerObj.QuestionId = existingQuestion.Id;
        //                            answerObj.QuizId = existingQuestion.QuizId;
        //                            answerObj.Option = "";
        //                            answerObj.OptionImage = string.Empty;
        //                            answerObj.PublicId = string.Empty;
        //                            answerObj.LastUpdatedBy = BusinessUserId;
        //                            answerObj.LastUpdatedOn = currentDate;

        //                            if (quiz.QuizType == (int)QuizTypeEnum.Score || quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
        //                            {
        //                                answerObj.AssociatedScore = default(int);
        //                                answerObj.IsCorrectAnswer = false;
        //                            }
        //                            else if (quiz.QuizType == (int)QuizTypeEnum.Personality || quiz.QuizType == (int)QuizTypeEnum.PersonalityTemplate)
        //                                answerObj.IsCorrectAnswer = false;
        //                            else
        //                                answerObj.IsCorrectAnswer = true;

        //                            answerObj.DisplayOrder = 1;
        //                            answerObj.Status = (int)StatusEnum.Active;
        //                            answerObj.State = (int)QuizStateEnum.DRAFTED;
        //                            answerObj.AutoPlay = true;
        //                            answerObj.SecondsToApply = "0";
        //                            answerObj.VideoFrameEnabled = false;

        //                            UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);
        //                            UOWObj.Save();
        //                        }
        //                        else
        //                        {
        //                            #region to add answer option for Single and Multiple type

        //                            var answerObj = new Db.AnswerOptionsInQuizQuestions();

        //                            if (existingQuestion.AnswerType == (int)AnswerTypeEnum.DOB || existingQuestion.AnswerType == (int)AnswerTypeEnum.PostCode || existingQuestion.AnswerType == (int)AnswerTypeEnum.DrivingLicense || existingQuestion.AnswerType == (int)AnswerTypeEnum.LookingforJobs || existingQuestion.AnswerType == (int)AnswerTypeEnum.FullAddress)
        //                                existingQuestion.Question = "Question" + (existingQuestion.QuizDetails.QuestionsInQuiz.Count(a => a.Status == (int)StatusEnum.Active && a.Type == (int)BranchingLogicEnum.QUESTION)).ToString();
        //                            answerObj.QuestionId = QuestionId;
        //                            answerObj.QuizId = existingQuestion.QuizId;
        //                            answerObj.Option = "Answer 1";
        //                            answerObj.OptionImage = string.Empty;
        //                            answerObj.PublicId = string.Empty;
        //                            answerObj.LastUpdatedBy = BusinessUserId;
        //                            answerObj.IsCorrectAnswer = true;
        //                            answerObj.IsCorrectForMultipleAnswer = false;
        //                            if (existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
        //                                answerObj.AssociatedScore = default(int);
        //                            answerObj.LastUpdatedOn = currentDate;
        //                            answerObj.DisplayOrder = 1;
        //                            answerObj.Status = (int)StatusEnum.Active;
        //                            answerObj.State = (int)QuizStateEnum.DRAFTED;
        //                            answerObj.IsReadOnly = false;

        //                            UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);

        //                            answerObj = new Db.AnswerOptionsInQuizQuestions();

        //                            answerObj.QuestionId = QuestionId;
        //                            answerObj.QuizId = existingQuestion.QuizId;
        //                            answerObj.Option = "Answer 2";
        //                            answerObj.OptionImage = string.Empty;
        //                            answerObj.PublicId = string.Empty;
        //                            answerObj.LastUpdatedBy = BusinessUserId;
        //                            answerObj.LastUpdatedOn = currentDate;
        //                            answerObj.DisplayOrder = 2;
        //                            answerObj.Status = (int)StatusEnum.Active;
        //                            answerObj.State = (int)QuizStateEnum.DRAFTED;
        //                            answerObj.IsCorrectAnswer = false;
        //                            answerObj.IsCorrectForMultipleAnswer = false;
        //                            answerObj.IsReadOnly = false;
        //                            if (existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
        //                                answerObj.AssociatedScore = default(int);

        //                            UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);

        //                            if (AnswerType == (int)AnswerTypeEnum.Single)
        //                            {
        //                                answerObj = new Db.AnswerOptionsInQuizQuestions();

        //                                answerObj.QuestionId = QuestionId;
        //                                answerObj.QuizId = existingQuestion.QuizId;
        //                                answerObj.Option = "Unanswered";
        //                                answerObj.OptionImage = string.Empty;
        //                                answerObj.PublicId = string.Empty;
        //                                answerObj.LastUpdatedBy = BusinessUserId;
        //                                answerObj.LastUpdatedOn = currentDate;
        //                                answerObj.DisplayOrder = 0;
        //                                answerObj.Status = (int)StatusEnum.Active;
        //                                answerObj.State = (int)QuizStateEnum.DRAFTED;
        //                                answerObj.IsCorrectAnswer = false;
        //                                answerObj.IsCorrectForMultipleAnswer = false;
        //                                answerObj.IsReadOnly = false;
        //                                answerObj.IsUnansweredType = true;
        //                                if (existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
        //                                    answerObj.AssociatedScore = default(int);

        //                                UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);
        //                            }

        //                            #endregion
        //                        }

        //                        #endregion
        //                    }

        //                    //sigle, multiple, DrivingLicense,LookingforJobs, FullAddress to Short, Long, DOB, PostCode, NPS, RatingEmoji, RatingStarts
        //                    else if ((existingQuestion.AnswerType == (int)AnswerTypeEnum.Single || existingQuestion.AnswerType == (int)AnswerTypeEnum.Multiple || existingQuestion.AnswerType == (int)AnswerTypeEnum.DrivingLicense || existingQuestion.AnswerType == (int)AnswerTypeEnum.LookingforJobs || existingQuestion.AnswerType == (int)AnswerTypeEnum.FullAddress) && (AnswerType == (int)AnswerTypeEnum.Short || AnswerType == (int)AnswerTypeEnum.Long || AnswerType == (int)AnswerTypeEnum.DOB || AnswerType == (int)AnswerTypeEnum.PostCode || AnswerType == (int)AnswerTypeEnum.NPS || AnswerType == (int)AnswerTypeEnum.RatingEmoji || AnswerType == (int)AnswerTypeEnum.RatingStarts))
        //                    {
        //                        #region update answer type other to text

        //                        foreach (var obj in existingQuestion.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active))
        //                            RemoveAnswer(obj.Id, BusinessUserId, CompanyId, AnswerType, null);

        //                        var answerObj = new Db.AnswerOptionsInQuizQuestions();
        //                        existingQuestion.RevealCorrectAnswer = false;
        //                        answerObj.QuestionId = QuestionId;
        //                        answerObj.QuizId = existingQuestion.QuizId;
        //                        switch (AnswerType)
        //                        {
        //                            case (int)AnswerTypeEnum.Short:
        //                                answerObj.Option = "Short answer text...";
        //                                if (existingQuestion.AnswerType == (int)AnswerTypeEnum.DrivingLicense || existingQuestion.AnswerType == (int)AnswerTypeEnum.LookingforJobs)
        //                                    existingQuestion.Question = "Question" + (existingQuestion.QuizDetails.QuestionsInQuiz.Count(a => a.Status == (int)StatusEnum.Active && a.Type == (int)BranchingLogicEnum.QUESTION)).ToString();
        //                                answerObj.IsReadOnly = false;
        //                                break;
        //                            case (int)AnswerTypeEnum.Long:
        //                                answerObj.Option = "Long answer text...";
        //                                if (existingQuestion.AnswerType == (int)AnswerTypeEnum.DrivingLicense || existingQuestion.AnswerType == (int)AnswerTypeEnum.LookingforJobs)
        //                                    existingQuestion.Question = "Question" + (existingQuestion.QuizDetails.QuestionsInQuiz.Count(a => a.Status == (int)StatusEnum.Active && a.Type == (int)BranchingLogicEnum.QUESTION)).ToString();
        //                                answerObj.IsReadOnly = false;
        //                                break;
        //                            case (int)AnswerTypeEnum.DOB:
        //                                answerObj.Option = null;
        //                                answerObj.IsReadOnly = false;
        //                                existingQuestion.Question = "Your date of birth";
        //                                break;
        //                            case (int)AnswerTypeEnum.PostCode:
        //                                answerObj.Option = null;
        //                                answerObj.IsReadOnly = false;
        //                                existingQuestion.Question = "Your post code";
        //                                break;
        //                            case (int)AnswerTypeEnum.NPS:
        //                                answerObj.Option = null;
        //                                answerObj.IsReadOnly = false;
        //                                existingQuestion.Question = "Question" + (existingQuestion.QuizDetails.QuestionsInQuiz.Count(a => a.Status == (int)StatusEnum.Active && a.Type == (int)BranchingLogicEnum.QUESTION)).ToString();
        //                                break;
        //                            case (int)AnswerTypeEnum.RatingEmoji:
        //                            case (int)AnswerTypeEnum.RatingStarts:
        //                                answerObj.Option = null;
        //                                answerObj.IsReadOnly = true;
        //                                existingQuestion.Question = "Are you happy with the way the recruiter contacted you?";
        //                                break;
        //                        }
        //                        answerObj.OptionImage = string.Empty;
        //                        answerObj.PublicId = string.Empty;
        //                        answerObj.IsCorrectAnswer = (AnswerType == (int)AnswerTypeEnum.NPS || AnswerType == (int)AnswerTypeEnum.RatingEmoji || AnswerType == (int)AnswerTypeEnum.RatingStarts) ? false : true;
        //                        answerObj.IsCorrectForMultipleAnswer = (AnswerType == (int)AnswerTypeEnum.NPS || AnswerType == (int)AnswerTypeEnum.RatingEmoji || AnswerType == (int)AnswerTypeEnum.RatingStarts) ? false : true;
        //                        answerObj.LastUpdatedBy = BusinessUserId;
        //                        answerObj.LastUpdatedOn = currentDate;
        //                        answerObj.DisplayOrder = 1;
        //                        answerObj.Status = (int)StatusEnum.Active;
        //                        answerObj.State = (int)QuizStateEnum.DRAFTED;

        //                        UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);

        //                        #endregion
        //                    }

        //                    //Short, Long, DOB, PostCode, NPS, RatingEmoji, RatingStarts to Short, Long, DOB, PostCode, NPS, RatingEmoji, RatingStarts
        //                    else if ((existingQuestion.AnswerType == (int)AnswerTypeEnum.Short || existingQuestion.AnswerType == (int)AnswerTypeEnum.Long || existingQuestion.AnswerType == (int)AnswerTypeEnum.DOB || existingQuestion.AnswerType == (int)AnswerTypeEnum.PostCode || existingQuestion.AnswerType == (int)AnswerTypeEnum.NPS || existingQuestion.AnswerType == (int)AnswerTypeEnum.RatingEmoji || existingQuestion.AnswerType == (int)AnswerTypeEnum.RatingStarts)
        //                              && (AnswerType == (int)AnswerTypeEnum.Short || AnswerType == (int)AnswerTypeEnum.Long || AnswerType == (int)AnswerTypeEnum.DOB || AnswerType == (int)AnswerTypeEnum.PostCode || AnswerType == (int)AnswerTypeEnum.NPS || AnswerType == (int)AnswerTypeEnum.RatingEmoji || AnswerType == (int)AnswerTypeEnum.RatingStarts))
        //                    {
        //                        #region update answer type text to text

        //                        var answerObj = existingQuestion.AnswerOptionsInQuizQuestions.FirstOrDefault(r => r.Status == (int)StatusEnum.Active);
        //                        existingQuestion.RevealCorrectAnswer = false;

        //                        switch (AnswerType)
        //                        {
        //                            case (int)AnswerTypeEnum.Short:
        //                                answerObj.Option = "Short answer text...";
        //                                if (existingQuestion.AnswerType == (int)AnswerTypeEnum.DOB || existingQuestion.AnswerType == (int)AnswerTypeEnum.PostCode || existingQuestion.AnswerType == (int)AnswerTypeEnum.RatingEmoji || existingQuestion.AnswerType == (int)AnswerTypeEnum.RatingStarts)
        //                                {
        //                                    existingQuestion.Question = "Question" + (existingQuestion.QuizDetails.QuestionsInQuiz.Count(a => a.Status == (int)StatusEnum.Active && a.Type == (int)BranchingLogicEnum.QUESTION)).ToString();
        //                                    existingQuestion.EnableComment = false;
        //                                }
        //                                break;
        //                            case (int)AnswerTypeEnum.Long:
        //                                answerObj.Option = "Long answer text...";
        //                                if (existingQuestion.AnswerType == (int)AnswerTypeEnum.DOB || existingQuestion.AnswerType == (int)AnswerTypeEnum.PostCode || existingQuestion.AnswerType == (int)AnswerTypeEnum.RatingEmoji || existingQuestion.AnswerType == (int)AnswerTypeEnum.RatingStarts)
        //                                {
        //                                    existingQuestion.Question = "Question" + (existingQuestion.QuizDetails.QuestionsInQuiz.Count(a => a.Status == (int)StatusEnum.Active && a.Type == (int)BranchingLogicEnum.QUESTION)).ToString();
        //                                    existingQuestion.EnableComment = false;
        //                                }
        //                                break;
        //                            case (int)AnswerTypeEnum.DOB:
        //                                answerObj.Option = null;
        //                                existingQuestion.Question = "Your date of birth";
        //                                existingQuestion.EnableComment = false;
        //                                break;
        //                            case (int)AnswerTypeEnum.PostCode:
        //                                answerObj.Option = null;
        //                                existingQuestion.Question = "Your post code";
        //                                existingQuestion.EnableComment = false;
        //                                break;
        //                            case (int)AnswerTypeEnum.NPS:
        //                                answerObj.IsCorrectAnswer = false;
        //                                answerObj.IsCorrectForMultipleAnswer = false;
        //                                answerObj.Option = null;
        //                                existingQuestion.RevealCorrectAnswer = false;
        //                                existingQuestion.Question = "Question" + (existingQuestion.QuizDetails.QuestionsInQuiz.Count(a => a.Status == (int)StatusEnum.Active && a.Type == (int)BranchingLogicEnum.QUESTION)).ToString();
        //                                existingQuestion.EnableComment = false;
        //                                break;
        //                            case (int)AnswerTypeEnum.RatingEmoji:
        //                            case (int)AnswerTypeEnum.RatingStarts:
        //                                answerObj.IsCorrectAnswer = false;
        //                                answerObj.IsCorrectForMultipleAnswer = false;
        //                                answerObj.Option = null;
        //                                answerObj.IsReadOnly = true;
        //                                existingQuestion.Question = "Are you happy with the way the recruiter contacted you?";
        //                                break;
        //                        }
        //                        UOWObj.AnswerOptionsInQuizQuestionsRepository.Update(answerObj);

        //                        #endregion
        //                    }

        //                    //Single, LookingforJobs to other
        //                    if ((existingQuestion.AnswerType == (int)AnswerTypeEnum.Single || existingQuestion.AnswerType == (int)AnswerTypeEnum.LookingforJobs) && (AnswerType != (int)AnswerTypeEnum.Single && AnswerType != (int)AnswerTypeEnum.LookingforJobs))
        //                    {
        //                        #region update answer type Single to other

        //                        var ansIds = existingQuestion.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active).ToList().Select(t => t.Id);
        //                        var branchingLogicList = UOWObj.BranchingLogicRepository.Get(r => ansIds.Any(s => s == r.SourceObjectId) && r.SourceTypeId == (int)BranchingLogicEnum.ANSWER).ToList();

        //                        if (branchingLogicList != null)
        //                        {
        //                            foreach (var obj in branchingLogicList)
        //                            {
        //                                UOWObj.BranchingLogicRepository.Delete(obj);
        //                            }
        //                        }

        //                        #endregion
        //                    }

        //                    //other to Short, Long, DOB, PostCode, NPS, RatingEmoji, RatingStarts
        //                    if ((existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate) && (existingQuestion.AnswerType == (int)AnswerTypeEnum.Multiple || existingQuestion.AnswerType == (int)AnswerTypeEnum.DrivingLicense) && (AnswerType == (int)AnswerTypeEnum.Single || AnswerType == (int)AnswerTypeEnum.DOB || AnswerType == (int)AnswerTypeEnum.PostCode || AnswerType == (int)AnswerTypeEnum.FullAddress || AnswerType == (int)AnswerTypeEnum.LookingforJobs || AnswerType == (int)AnswerTypeEnum.RatingStarts || AnswerType == (int)AnswerTypeEnum.RatingEmoji))
        //                    {
        //                        existingQuestion.MinAnswer = 1;
        //                        existingQuestion.MaxAnswer = 1;
        //                    }

        //                    if ((existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate) && (AnswerType == (int)AnswerTypeEnum.DrivingLicense || AnswerType == (int)AnswerTypeEnum.Multiple))
        //                    {
        //                        existingQuestion.MinAnswer = 0;
        //                        existingQuestion.MaxAnswer = existingQuestion.AnswerOptionsInQuizQuestions.Count(r => r.Status == (int)StatusEnum.Active);
        //                    }

        //                    if (AnswerType != (int)AnswerTypeEnum.Single || AnswerType != (int)AnswerTypeEnum.Multiple || AnswerType != (int)AnswerTypeEnum.Short || AnswerType != (int)AnswerTypeEnum.Long)
        //                    {
        //                        existingQuestion.TimerRequired = false;
        //                        existingQuestion.Time = null;
        //                    }

        //                    if (AnswerType != (int)AnswerTypeEnum.Single && AnswerType != (int)AnswerTypeEnum.Multiple)
        //                    {
        //                        existingQuestion.ShowAnswerImage = false;
        //                    }

        //                    if (isWhatsappEnable)
        //                    {
        //                        existingQuestion.AnswerStructureType = answerStructureType;
        //                    }

        //                    existingQuestion.AnswerType = AnswerType;
        //                    existingQuestion.LastUpdatedBy = BusinessUserId;
        //                    existingQuestion.LastUpdatedOn = currentDate;

        //                    existingQuestion.QuizDetails.LastUpdatedBy = BusinessUserId;
        //                    existingQuestion.QuizDetails.LastUpdatedOn = currentDate;

        //                    existingQuestion.QuizDetails.Quiz.State = (int)QuizStateEnum.DRAFTED;

        //                    existingQuestion.AutoPlay = false;
        //                    if (existingQuestion.AnswerType == (int)AnswerTypeEnum.Single || existingQuestion.AnswerType == (int)AnswerTypeEnum.Multiple)
        //                    {
        //                        var autoplay = existingQuestion.AnswerOptionsInQuizQuestions.Any(v => v.AutoPlay) ? true : false;
        //                        existingQuestion.AutoPlay = autoplay;
        //                    }



        //                    UOWObj.QuizRepository.Update(existingQuestion.QuizDetails.Quiz);
        //                    UOWObj.Save();

        //                    
        //                    
        //                }
        //                else
        //                {
        //                    if (isWhatsappEnable)
        //                    {
        //                        if (existingQuestion.AnswerStructureType != answerStructureType && (answerStructureType == (int)AnswerStructureTypeEnum.List || answerStructureType == (int)AnswerStructureTypeEnum.Button))
        //                        {
        //                            if (AnswerType == (int)AnswerTypeEnum.Single)
        //                            {
        //                                existingQuestion.EnableComment = false;

        //                                SetSingleMultipleAnswerTYpe(QuestionId, AnswerType, BusinessUserId, CompanyId, UOWObj, existingQuestion, currentDate);
        //                            }
        //                        }

        //                        existingQuestion.AnswerStructureType = answerStructureType != 0 ? answerStructureType : (int)AnswerStructureTypeEnum.Default;

        //                        UOWObj.QuestionsInQuizRepository.Update(existingQuestion);
        //                        UOWObj.Save();

        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Status = ResultEnum.Error;
        //        ErrorMessage = ex.Message;
        //        throw ex;
        //    }

        //}


        //private void SetSingleMultipleAnswerTYpe(int QuestionId, int AnswerType, int BusinessUserId, int CompanyId, AutomationUnitOfWork UOWObj, Db.QuestionsInQuiz existingQuestion, DateTime currentDate)
        //{
        //    foreach (var obj in existingQuestion.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active))
        //        RemoveAnswer(obj.Id, BusinessUserId, CompanyId, AnswerType, UOWObj);
        //    #region to add answer option for Single and Multiple type

        //    var answerObj = new Db.AnswerOptionsInQuizQuestions();

        //    if (existingQuestion.AnswerType == (int)AnswerTypeEnum.DOB || existingQuestion.AnswerType == (int)AnswerTypeEnum.PostCode || existingQuestion.AnswerType == (int)AnswerTypeEnum.DrivingLicense || existingQuestion.AnswerType == (int)AnswerTypeEnum.LookingforJobs || existingQuestion.AnswerType == (int)AnswerTypeEnum.FullAddress)
        //        existingQuestion.Question = "Question" + (existingQuestion.QuizDetails.QuestionsInQuiz.Count(a => a.Status == (int)StatusEnum.Active && a.Type == (int)BranchingLogicEnum.QUESTION)).ToString();
        //    answerObj.QuestionId = QuestionId;
        //    answerObj.QuizId = existingQuestion.QuizId;
        //    answerObj.Option = "Answer 1";
        //    answerObj.OptionImage = string.Empty;
        //    answerObj.PublicId = string.Empty;
        //    answerObj.LastUpdatedBy = BusinessUserId;
        //    answerObj.IsCorrectAnswer = true;
        //    answerObj.IsCorrectForMultipleAnswer = false;
        //    if (existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
        //        answerObj.AssociatedScore = default(int);
        //    answerObj.LastUpdatedOn = currentDate;
        //    answerObj.DisplayOrder = 1;
        //    answerObj.Status = (int)StatusEnum.Active;
        //    answerObj.State = (int)QuizStateEnum.DRAFTED;
        //    answerObj.IsReadOnly = false;

        //    UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);

        //    answerObj = new Db.AnswerOptionsInQuizQuestions();

        //    answerObj.QuestionId = QuestionId;
        //    answerObj.QuizId = existingQuestion.QuizId;
        //    answerObj.Option = "Answer 2";
        //    answerObj.OptionImage = string.Empty;
        //    answerObj.PublicId = string.Empty;
        //    answerObj.LastUpdatedBy = BusinessUserId;
        //    answerObj.LastUpdatedOn = currentDate;
        //    answerObj.DisplayOrder = 2;
        //    answerObj.Status = (int)StatusEnum.Active;
        //    answerObj.State = (int)QuizStateEnum.DRAFTED;
        //    answerObj.IsCorrectAnswer = false;
        //    answerObj.IsCorrectForMultipleAnswer = false;
        //    answerObj.IsReadOnly = false;
        //    if (existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
        //        answerObj.AssociatedScore = default(int);

        //    UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);

        //    if (AnswerType == (int)AnswerTypeEnum.Single)
        //    {
        //        answerObj = new Db.AnswerOptionsInQuizQuestions();

        //        answerObj.QuestionId = QuestionId;
        //        answerObj.QuizId = existingQuestion.QuizId;
        //        answerObj.Option = "Unanswered";
        //        answerObj.OptionImage = string.Empty;
        //        answerObj.PublicId = string.Empty;
        //        answerObj.LastUpdatedBy = BusinessUserId;
        //        answerObj.LastUpdatedOn = currentDate;
        //        answerObj.DisplayOrder = 0;
        //        answerObj.Status = (int)StatusEnum.Active;
        //        answerObj.State = (int)QuizStateEnum.DRAFTED;
        //        answerObj.IsCorrectAnswer = false;
        //        answerObj.IsCorrectForMultipleAnswer = false;
        //        answerObj.IsReadOnly = false;
        //        answerObj.IsUnansweredType = true;
        //        if (existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
        //            answerObj.AssociatedScore = default(int);

        //        UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);
        //    }

        //    #endregion
        //}

        public List<Correlation> GetResultCorrelation(int questionId)
        {
            try
            {
                List<Correlation> correlationList = new List<Correlation>();
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var personalityAnswerResultMappingList = UOWObj.PersonalityAnswerResultMappingRepository.Get(x => x.AnswerOptionsInQuizQuestions.QuestionId == questionId);
                    if (personalityAnswerResultMappingList.Any())
                    {
                        foreach (var mapping in personalityAnswerResultMappingList)
                        {
                            Correlation relation = new Correlation();
                            relation.AnswerId = mapping.AnswerId;
                            relation.ResultId = mapping.ResultId;
                            correlationList.Add(relation);
                        }
                    }
                }
                return correlationList;
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public void UpdateResultCorrelation(ResultCorrelation resultCorrelation, int BusinessUserId, int CompanyId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {

                    var questionsDetails = UOWObj.QuestionsInQuizRepository.GetByID(resultCorrelation.QuestionId);
                    if (questionsDetails != null)
                    {
                        questionsDetails.MinAnswer = resultCorrelation.MinAnswer;
                        questionsDetails.MaxAnswer = resultCorrelation.MaxAnswer;
                        UOWObj.QuestionsInQuizRepository.Update(questionsDetails);
                        UOWObj.Save();

                        var quizDetails = UOWObj.QuizDetailsRepository.GetByID(questionsDetails.QuizId);
                        if (quizDetails != null)
                        {
                            foreach (var relation in questionsDetails.AnswerOptionsInQuizQuestions.Where(r => !r.IsUnansweredType))
                            {
                                var MappingList = relation.PersonalityAnswerResultMapping.ToList();
                                foreach (var data in MappingList)
                                {
                                    UOWObj.PersonalityAnswerResultMappingRepository.Delete(data.Id);
                                }
                                UOWObj.Save();
                            }

                            if (resultCorrelation.CorrelationList.Any())
                            {
                                foreach (var relation in resultCorrelation.CorrelationList)
                                {
                                    if (relation.AnswerId != default(int) && relation.ResultId != default(int))
                                    {
                                        var resultRelation = new Db.PersonalityAnswerResultMapping();
                                        resultRelation.AnswerId = relation.AnswerId;
                                        resultRelation.ResultId = relation.ResultId;
                                        UOWObj.PersonalityAnswerResultMappingRepository.Insert(resultRelation);
                                        UOWObj.Save();
                                    }
                                }
                            }

                            quizDetails.LastUpdatedBy = BusinessUserId;
                            quizDetails.LastUpdatedOn = DateTime.UtcNow;
                            quizDetails.Quiz.State = (int)QuizStateEnum.DRAFTED;

                            UOWObj.QuizRepository.Update(quizDetails.Quiz);
                            UOWObj.Save();

                            
                            
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public void ReorderResultQuestionAnswer(QuizReorderResult reorderedList, int BusinessUserId, int CompanyId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var currentDate = DateTime.UtcNow;

                    for (int i = 0; i < reorderedList.QuizResultList.Count(); i++)
                    {
                        var resultObj = reorderedList.QuizResultList[i];
                        var result = UOWObj.QuizResultsRepository.GetByID(resultObj.ResultId);

                        if (result != null)
                        {
                            result.DisplayOrder = i + 1;
                            result.LastUpdatedBy = BusinessUserId;
                            result.LastUpdatedOn = currentDate;

                            result.QuizDetails.LastUpdatedBy = BusinessUserId;
                            result.QuizDetails.LastUpdatedOn = currentDate;
                            result.QuizDetails.Quiz.State = (int)QuizStateEnum.DRAFTED;

                            UOWObj.QuizResultsRepository.Update(result);

                            UOWObj.QuizRepository.Update(result.QuizDetails.Quiz);
                        }
                    }

                    for (int i = 0; i < reorderedList.QuizQuestionList.Count(); i++)
                    {
                        var questionObj = reorderedList.QuizQuestionList[i];
                        var question = UOWObj.QuestionsInQuizRepository.GetByID(questionObj.QuestionId);

                        if (question != null)
                        {
                            question.DisplayOrder = i + 1;
                            question.LastUpdatedBy = BusinessUserId;
                            question.LastUpdatedOn = currentDate;

                            for (int j = 0; j < questionObj.AnswerList.Count(); j++)
                            {
                                var answerObj = questionObj.AnswerList[j];
                                var answer = question.AnswerOptionsInQuizQuestions.FirstOrDefault(r => r.Id == answerObj.AnswerId);

                                if (answer != null)
                                {
                                    answer.DisplayOrder = j;
                                    answer.LastUpdatedBy = BusinessUserId;
                                    answer.LastUpdatedOn = currentDate;
                                }
                            }

                            question.QuizDetails.LastUpdatedBy = BusinessUserId;
                            question.QuizDetails.LastUpdatedOn = currentDate;

                            question.QuizDetails.Quiz.State = (int)QuizStateEnum.DRAFTED;

                            UOWObj.QuizRepository.Update(question.QuizDetails.Quiz);
                        }
                    }

                    UOWObj.Save();

                    
                    
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public PersonalityResultSettingModel GetPersonalityResultSetting(int quizId)
        {
            try
            {
                PersonalityResultSettingModel personalityResult = new PersonalityResultSettingModel();
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quiz = UOWObj.QuizRepository.GetByID(quizId);
                    if (quiz != null)
                    {
                        var quizDetails = quiz.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED);
                        if (quizDetails != null)
                        {
                            var personalityResultObj = quizDetails.PersonalityResultSetting.FirstOrDefault();

                            if (personalityResultObj != null)
                            {
                                personalityResult.Id = personalityResultObj.Id;
                                personalityResult.QuizId = personalityResultObj.QuizId;
                                personalityResult.Title = personalityResultObj.Title;
                                personalityResult.Status = personalityResultObj.Status;
                                personalityResult.MaxResult = personalityResultObj.MaxResult;
                                personalityResult.GraphColor = personalityResultObj.GraphColor;
                                personalityResult.ButtonColor = personalityResultObj.ButtonColor;
                                personalityResult.ButtonFontColor = personalityResultObj.ButtonFontColor;
                                personalityResult.SideButtonText = personalityResultObj.SideButtonText;
                                personalityResult.IsFullWidthEnable = personalityResultObj.IsFullWidthEnable;
                                personalityResult.LastUpdatedOn = personalityResultObj.LastUpdatedOn;
                                personalityResult.LastUpdatedBy = personalityResultObj.LastUpdatedBy;
                                //  personalityResult.ShowLeadUserForm = personalityResultObj.ShowLeadUserForm;

                                var quizResults = quizDetails.QuizResults.Where(r => r.IsPersonalityCorrelatedResult == false && r.Status == (int)StatusEnum.Active);

                                if (quizResults.Any())
                                {
                                    personalityResult.ResultDetails = new List<ResultDetail>();
                                    foreach (var item in quizResults)
                                    {
                                        personalityResult.ResultDetails.Add(new ResultDetail
                                        {
                                            ResultId = item.Id,
                                            Title = item.Title,
                                            Image = item.Image,
                                        });
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz not found for the QuizId " + quizId;
                    }
                }
                return personalityResult;
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public void UpdatePersonalityResultSetting(PersonalityResultSettingModel personalityResult, int BusinessUserId, int CompanyId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizDetails = UOWObj.QuizDetailsRepository.GetByID(personalityResult.QuizId);
                    if (quizDetails != null)
                    {
                        var personalityResultObj = quizDetails.PersonalityResultSetting.FirstOrDefault();

                        if (personalityResultObj != null)
                        {
                            personalityResultObj.Title = personalityResult.Title;
                            personalityResultObj.Status = personalityResult.Status;
                            personalityResultObj.MaxResult = personalityResult.MaxResult;
                            personalityResultObj.GraphColor = personalityResult.GraphColor;
                            personalityResultObj.ButtonColor = personalityResult.ButtonColor;
                            personalityResultObj.ButtonFontColor = personalityResult.ButtonFontColor;
                            personalityResultObj.SideButtonText = personalityResult.SideButtonText;
                            personalityResultObj.IsFullWidthEnable = personalityResult.IsFullWidthEnable;
                            personalityResultObj.LastUpdatedOn = DateTime.UtcNow;
                            personalityResultObj.LastUpdatedBy = BusinessUserId;

                            UOWObj.PersonalityResultSettingRepository.Update(personalityResultObj);
                            UOWObj.Save();
                        }

                        quizDetails.LastUpdatedBy = BusinessUserId;
                        quizDetails.LastUpdatedOn = DateTime.UtcNow;
                        quizDetails.Quiz.State = (int)QuizStateEnum.DRAFTED;

                        UOWObj.QuizRepository.Update(quizDetails.Quiz);
                        UOWObj.Save();
                        
                        
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public void UpdatePersonalityResultStatus(int quizId, int status, int BusinessUserId, int CompanyId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizObj = UOWObj.QuizRepository.GetByID(quizId);
                    if (quizObj != null)
                    {
                        var quizDetails = quizObj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED);

                        if (quizDetails != null)
                        {
                            var personalityQuiz = quizDetails.PersonalityResultSetting.FirstOrDefault();

                            if (personalityQuiz != null)
                            {
                                personalityQuiz.Status = status;

                                personalityQuiz.LastUpdatedOn = DateTime.UtcNow;
                                personalityQuiz.LastUpdatedBy = BusinessUserId;

                                UOWObj.PersonalityResultSettingRepository.Update(personalityQuiz);
                                UOWObj.Save();
                            }
                            quizDetails.LastUpdatedBy = BusinessUserId;
                            quizDetails.LastUpdatedOn = DateTime.UtcNow;
                            quizObj.State = (int)QuizStateEnum.DRAFTED;

                            UOWObj.QuizRepository.Update(quizObj);
                            UOWObj.Save();

                            
                            
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz not found for the QuizId " + quizId;
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public void UpdatePersonalityWidthSetting(int quizId, bool isFullWidthEnable, int BusinessUserId, int CompanyId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizObj = UOWObj.QuizRepository.GetByID(quizId);
                    if (quizObj != null)
                    {
                        var quizDetails = quizObj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED);

                        if (quizDetails != null)
                        {
                            var personalityQuiz = quizDetails.PersonalityResultSetting.FirstOrDefault();

                            if (personalityQuiz != null)
                            {
                                personalityQuiz.IsFullWidthEnable = isFullWidthEnable;

                                personalityQuiz.LastUpdatedOn = DateTime.UtcNow;
                                personalityQuiz.LastUpdatedBy = BusinessUserId;

                                UOWObj.PersonalityResultSettingRepository.Update(personalityQuiz);
                                UOWObj.Save();
                            }
                            quizDetails.LastUpdatedBy = BusinessUserId;
                            quizDetails.LastUpdatedOn = DateTime.UtcNow;
                            quizObj.State = (int)QuizStateEnum.DRAFTED;

                            UOWObj.QuizRepository.Update(quizObj);
                            UOWObj.Save();

                            
                            
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz not found for the QuizId " + quizId;
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public void UpdatePersonalityMaxResult(int quizId, int maxResult, bool ShowLeadUserForm, int BusinessUserId, int CompanyId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizObj = UOWObj.QuizRepository.GetByID(quizId);
                    if (quizObj != null)
                    {
                        var quizDetails = quizObj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED);

                        if (quizDetails != null)
                        {
                            var personalityQuiz = quizDetails.PersonalityResultSetting.FirstOrDefault();

                            if (personalityQuiz != null)
                            {
                                personalityQuiz.MaxResult = maxResult;
                                //  personalityQuiz.ShowLeadUserForm = ShowLeadUserForm;

                                personalityQuiz.LastUpdatedOn = DateTime.UtcNow;
                                personalityQuiz.LastUpdatedBy = BusinessUserId;

                                UOWObj.PersonalityResultSettingRepository.Update(personalityQuiz);
                                UOWObj.Save();
                            }
                            quizDetails.LastUpdatedBy = BusinessUserId;
                            quizDetails.LastUpdatedOn = DateTime.UtcNow;
                            quizObj.State = (int)QuizStateEnum.DRAFTED;

                            UOWObj.QuizRepository.Update(quizObj);
                            UOWObj.Save();

                            
                            
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz not found for the QuizId " + quizId;
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public List<PendingApiQueueModel> PendingApiList()
        {
            var communicationUrl = GlobalSettings.communicationApiUrl;
            var communicationSMSUrl = communicationUrl + "/api/v1/sms/messages/send";
            var pendingApiList = new List<PendingApiQueueModel>();

            using (var UOWObj = new AutomationUnitOfWork())
            {
                var pendingApiLst = UOWObj.PendingApiQueueRepository.Get();
                try
                {
                    foreach (var pendingApiObj in pendingApiLst.ToList())
                    {
                        try
                        {
                            var urlhub = GlobalSettings.HubUrl.ToString();
                            var url = urlhub + "/api/v1/Automations/web-hooks/UpdateAutomationStatus";
                            if (pendingApiObj.RequestTypeURL == url)
                            {
                                var res = JsonConvert.DeserializeObject<QuizApp.Response.UpdateQuizStatusResponse>(OWCHelper.GetResponse(pendingApiObj.RequestTypeURL, pendingApiObj.Authorization, pendingApiObj.RequestType, pendingApiObj.RequestData));

                                //if (res.status == "true")
                                    UOWObj.PendingApiQueueRepository.Delete(pendingApiObj);
                            }
                            else if (pendingApiObj.RequestTypeURL == communicationSMSUrl)
                            {
                                var res = JsonConvert.DeserializeObject<Dictionary<string, object>>(OWCHelper.GetResponse(pendingApiObj.RequestTypeURL, pendingApiObj.Authorization, pendingApiObj.RequestType, pendingApiObj.RequestData));

                                //if (res["status"].ToString().ToLower() == "sent")
                                    UOWObj.PendingApiQueueRepository.Delete(pendingApiObj);
                            }
                            else if (pendingApiObj.RequestTypeURL == ConfigurationManager.AppSettings["OWCWhatsappCommunicationURL"].ToString())
                            {
                                var res = OWCHelper.GetResponse(pendingApiObj.RequestTypeURL, pendingApiObj.Authorization, pendingApiObj.RequestType, pendingApiObj.RequestData);

                                //if (res == "{\"success\":true}")
                                    UOWObj.PendingApiQueueRepository.Delete(pendingApiObj);
                            }
                            else if (pendingApiObj.Authorization == GlobalSettings.apiSecret.ToString())
                            {
                                var apiSuccess = JsonConvert.DeserializeObject<bool>(OWCHelper.GetResponseWithApiSecret(pendingApiObj.RequestTypeURL, pendingApiObj.Authorization, pendingApiObj.RequestType, pendingApiObj.RequestData));
                                //if (apiSuccess)
                                    UOWObj.PendingApiQueueRepository.Delete(pendingApiObj);
                            }
                            else
                            {
                                var apiSuccess = OWCHelper.GetResponse(pendingApiObj.RequestTypeURL, pendingApiObj.Authorization, pendingApiObj.RequestType, pendingApiObj.RequestData);
                                //if (apiSuccess == "true")
                                    UOWObj.PendingApiQueueRepository.Delete(pendingApiObj);
                            }
                        }
                        catch (Exception)
                        {

                        }
                    }
                    UOWObj.Save();
                }
                catch (Exception)
                {

                }
            }
            return pendingApiList;
        }

        public static void AddPendingApi(string RequestTypeURL, string Authorization, string RequestData, string RequestType)
        {
            using (var UOWObj = new AutomationUnitOfWork())
            {
                var pendingApiQueueObj = new Db.PendingApiQueue
                {
                    CreatedOn = DateTime.UtcNow,
                    RequestTypeURL = RequestTypeURL,
                    Authorization = Authorization,
                    RequestData = RequestData,
                    RequestType = RequestType
                };
                UOWObj.PendingApiQueueRepository.Insert(pendingApiQueueObj);
                UOWObj.Save();
            }
        }

        public List<AttemptedAutomation> GetAttemptedAutomationByLeads(string LeadIds, int QuizId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var leadIds = string.IsNullOrEmpty(LeadIds) ? new List<string>() : LeadIds.Split(',').ToList();
                    var attemptedAutomationList = new List<AttemptedAutomation>();
                    foreach (var LeadId in leadIds)
                    {
                        var attemptedAutomation = new AttemptedAutomation();

                        var workPackageInfoObj = UOWObj.WorkPackageInfoRepository.Get(r => (r.QuizAttempts.Count() == 0 || r.QuizAttempts.FirstOrDefault().QuizStats.Count() == 0) && ((QuizId > 0 && r.QuizId == QuizId && r.LeadUserId == LeadId) || (QuizId == 0 && r.LeadUserId == LeadId)));
                        var quizAttemptsObj = UOWObj.QuizAttemptsRepository.Get(r => (r.QuizStats.Any()) && ((QuizId > 0 && r.QuizDetails.ParentQuizId == QuizId && r.LeadUserId == LeadId) || (QuizId == 0 && r.LeadUserId == LeadId)));
                        attemptedAutomation.LeadId = LeadId;

                        attemptedAutomation.quizDetails = new List<AttemptedAutomation.quizDetail>();

                        foreach (var obj in workPackageInfoObj)
                        {
                            var attemptedQuizDetail = new AttemptedAutomation.quizDetail();
                            attemptedQuizDetail.QuizDetails = new AttemptedAutomation.quizDetail.QuizDetail();
                            attemptedQuizDetail.QuizDetails.QuizId = obj.QuizId;

                            if (obj.QuizAttempts.Any())
                                attemptedQuizDetail.QuizDetails.QuizTitle = HttpUtility.HtmlDecode(Regex.Replace(VariableLinking(obj.QuizAttempts.LastOrDefault().QuizDetails.QuizTitle, obj.QuizAttempts.LastOrDefault().QuizDetails, obj.QuizAttempts.LastOrDefault(), true, false, null), "<.*?>", string.Empty));
                            else
                            {
                                var quizAttempt = new QuizApp.Db.QuizAttempts() { LeadUserId = obj.LeadUserId };
                                attemptedQuizDetail.QuizDetails.QuizTitle = HttpUtility.HtmlDecode(Regex.Replace(VariableLinking(obj.Quiz.QuizDetails.LastOrDefault().QuizTitle, obj.Quiz.QuizDetails.LastOrDefault(), quizAttempt, true, false, null), "<.*?>", string.Empty));
                            }

                            attemptedQuizDetail.QuizDetails.QuizStatus = (QuizStatusEnum.Sent).ToString();
                            attemptedQuizDetail.QuizDetails.QuizDate = obj.CreatedOn;

                            attemptedAutomation.quizDetails.Add(attemptedQuizDetail);
                        }

                        foreach (var obj in quizAttemptsObj)
                        {
                            var correctAnsCount = 0;
                            var ShowScoreValue = false;
                            var scoreValueTxt = string.Empty;
                            var resultSetting = obj.QuizDetails.ResultSettings.FirstOrDefault();

                            if (resultSetting != null)
                            {
                                var attemptedQuestions = obj.QuizQuestionStats.Where(t => t.Status == (int)StatusEnum.Active && t.CompletedOn != null && (t.QuestionsInQuiz.TimerRequired ? true : t.QuizAnswerStats.Any(r => !r.AnswerOptionsInQuizQuestions.IsUnansweredType)));

                                if (resultSetting.ShowScoreValue.HasValue && resultSetting.ShowScoreValue.Value && (obj.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || obj.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate || attemptedQuestions.Any(r => (r.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Any(t => t.Status == (int)StatusEnum.Active && t.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && t.IsCorrectForMultipleAnswer.HasValue && t.IsCorrectForMultipleAnswer.Value)) || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single)))
                                {
                                    if (obj.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || obj.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                        correctAnsCount = attemptedQuestions.Sum(a => a.QuizAnswerStats.Sum(x => x.AnswerOptionsInQuizQuestions.AssociatedScore)).Value;
                                    else
                                        correctAnsCount = attemptedQuestions.Count(a => (a.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && ((a.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Select(s => s.Id).OrderBy(s => s)).SequenceEqual(a.QuizAnswerStats.Select(r => r.AnswerId).OrderBy(s => s)))) || (a.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single && a.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer == true));

                                    ShowScoreValue = true;
                                    scoreValueTxt = string.IsNullOrEmpty(scoreValueTxt) ? string.Empty : scoreValueTxt.Replace("%score%", ' ' + correctAnsCount.ToString() + ' ').Replace("%total%", ' ' + attemptedQuestions.Count(t => (t.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Any(r => r.Status == (int)StatusEnum.Active && r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value)) || t.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single).ToString() + ' ');
                                }
                            }

                            var attemptedQuizDetail = new AttemptedAutomation.quizDetail();
                            attemptedQuizDetail.QuizDetails = new AttemptedAutomation.quizDetail.QuizDetail();
                            attemptedQuizDetail.QuizDetails.QuizId = obj.QuizDetails.ParentQuizId;
                            attemptedQuizDetail.QuizDetails.QuizTitle = HttpUtility.HtmlDecode(Regex.Replace(VariableLinking(obj.QuizDetails.QuizTitle, obj.QuizDetails, obj, true, ShowScoreValue, scoreValueTxt), "<.*?>", string.Empty));

                            if (obj.QuizStats.FirstOrDefault().CompletedOn != null)
                            {
                                attemptedQuizDetail.QuizDetails.QuizStatus = (QuizStatusEnum.Completed).ToString();
                                attemptedQuizDetail.QuizDetails.QuizDate = obj.QuizStats.FirstOrDefault().CompletedOn;
                            }
                            else
                            {
                                attemptedQuizDetail.QuizDetails.QuizStatus = (QuizStatusEnum.Started).ToString();
                                attemptedQuizDetail.QuizDetails.QuizDate = obj.QuizStats.FirstOrDefault().StartedOn;
                            }

                            var quizResults = obj.QuizStats.Where(r => r.CompletedOn != null && r.ResultId != null);

                            attemptedQuizDetail.QuizDetails.AchievedResults = new List<AttemptedAutomation.quizDetail.QuizDetail.AchievedResult>();

                            foreach (var quizResultsobj in quizResults)
                            {
                                var achievedResultDetails = new AttemptedAutomation.quizDetail.QuizDetail.AchievedResult();
                                achievedResultDetails.Id = quizResultsobj.ResultId.Value;
                                achievedResultDetails.Title = HttpUtility.HtmlDecode(Regex.Replace(VariableLinking(quizResultsobj.QuizResults.Title, obj.QuizDetails, obj, false, ShowScoreValue, scoreValueTxt), "<.*?>", string.Empty));
                                achievedResultDetails.InternalTitle = HttpUtility.HtmlDecode(Regex.Replace(VariableLinking(quizResultsobj.QuizResults.InternalTitle, obj.QuizDetails, obj, false, ShowScoreValue, scoreValueTxt), "<.*?>", string.Empty));
                                attemptedQuizDetail.QuizDetails.AchievedResults.Add(achievedResultDetails);
                            }

                            var quizQuestions = obj.QuizQuestionStats.Where(r => r.QuestionsInQuiz.Type == (int)BranchingLogicEnum.QUESTION && r.Status == (int)StatusEnum.Active && r.CompletedOn != null && (r.QuestionsInQuiz.TimerRequired ? true : r.QuizAnswerStats.Any(t => !t.AnswerOptionsInQuizQuestions.IsUnansweredType)));

                            attemptedQuizDetail.QuestionDetails = new List<AttemptedAutomation.quizDetail.QuestionDetail>();
                            foreach (var quizQuestionsobj in quizQuestions)
                            {
                                var question = quizQuestionsobj.QuestionsInQuiz;
                                var questionDetail = new AttemptedAutomation.quizDetail.QuestionDetail();
                                questionDetail.QuiestionId = quizQuestionsobj.QuestionId;
                                questionDetail.AnswerType = question.AnswerType;
                                questionDetail.QuiestionTitle = HttpUtility.HtmlDecode(Regex.Replace(VariableLinking(question.Question, obj.QuizDetails, obj, false, ShowScoreValue, scoreValueTxt), "<.*?>", string.Empty));
                                questionDetail.QuestionAnswerDetails = new List<AttemptedAutomation.quizDetail.QuestionDetail.QuestionAnswerDetail>();

                                if (question.AnswerType == (int)AnswerTypeEnum.Single || question.AnswerType == (int)AnswerTypeEnum.Multiple || question.AnswerType == (int)AnswerTypeEnum.LookingforJobs || question.AnswerType == (int)AnswerTypeEnum.DrivingLicense)
                                {
                                    int associatedScore = default(int);
                                    bool? IsCorrectValue = null;
                                    if (question.AnswerType == (int)AnswerTypeEnum.Multiple && question.AnswerOptionsInQuizQuestions.Any(r => r.Status == (int)StatusEnum.Active && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value))
                                    {
                                        if (obj.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || obj.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                            associatedScore = quizQuestionsobj.QuizAnswerStats.Sum(x => x.AnswerOptionsInQuizQuestions.AssociatedScore).Value;
                                        else if (obj.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Assessment || obj.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.AssessmentTemplate)
                                            IsCorrectValue = (question.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Select(s => s.Id).OrderBy(s => s)).SequenceEqual(quizQuestionsobj.QuizAnswerStats.Select(r => r.AnswerId).OrderBy(s => s));
                                    }
                                    else if (question.AnswerType == (int)AnswerTypeEnum.Single)
                                    {
                                        if (obj.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || obj.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                            associatedScore = quizQuestionsobj.QuizAnswerStats.Sum(x => x.AnswerOptionsInQuizQuestions.AssociatedScore).Value;
                                        else if (obj.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Assessment || obj.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.AssessmentTemplate)
                                            IsCorrectValue = quizQuestionsobj.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer.HasValue && quizQuestionsobj.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer.Value;
                                    }
                                    questionDetail.IsCorrect = IsCorrectValue;

                                    if (quizQuestionsobj.QuizAnswerStats.Any(r => !r.AnswerOptionsInQuizQuestions.IsUnansweredType))
                                    {
                                        foreach (var quizAnswerStatsObj in quizQuestionsobj.QuizAnswerStats.Where(r => !r.AnswerOptionsInQuizQuestions.IsUnansweredType))
                                        {
                                            var questionAnswerDetail = new AttemptedAutomation.quizDetail.QuestionDetail.QuestionAnswerDetail();
                                            questionAnswerDetail.AnswerId = quizAnswerStatsObj.AnswerId;
                                            questionAnswerDetail.AnswerTitle = HttpUtility.HtmlDecode(Regex.Replace(VariableLinking(quizAnswerStatsObj.AnswerOptionsInQuizQuestions.Option, obj.QuizDetails, obj, false, ShowScoreValue, scoreValueTxt), "<.*?>", string.Empty));
                                            questionAnswerDetail.AssociatedScore = (obj.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || obj.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate) ? default(int) : associatedScore;
                                            questionDetail.QuestionAnswerDetails.Add(questionAnswerDetail);
                                        }
                                    }
                                    else
                                    {
                                        var questionAnswerDetail = new AttemptedAutomation.quizDetail.QuestionDetail.QuestionAnswerDetail();
                                        questionAnswerDetail.AnswerId = null;
                                        questionAnswerDetail.AnswerTitle = null;
                                        questionAnswerDetail.AssociatedScore = null;
                                        questionDetail.QuestionAnswerDetails.Add(questionAnswerDetail);
                                    }

                                }
                                else if (question.AnswerType == (int)AnswerTypeEnum.Short || question.AnswerType == (int)AnswerTypeEnum.Long || question.AnswerType == (int)AnswerTypeEnum.DOB || question.AnswerType == (int)AnswerTypeEnum.PostCode)
                                {
                                    questionDetail.QuestionAnswerDetails.Add(new AttemptedAutomation.quizDetail.QuestionDetail.QuestionAnswerDetail()
                                    {
                                        AnswerId = (quizQuestionsobj.QuizAnswerStats != null && quizQuestionsobj.QuizAnswerStats.Any(r => !r.AnswerOptionsInQuizQuestions.IsUnansweredType)) ? quizQuestionsobj.QuizAnswerStats.FirstOrDefault().AnswerId : default(int?),
                                        AnswerTitle = (quizQuestionsobj.QuizAnswerStats != null && quizQuestionsobj.QuizAnswerStats.Any(r => !r.AnswerOptionsInQuizQuestions.IsUnansweredType)) ? quizQuestionsobj.QuizAnswerStats.FirstOrDefault().AnswerText : null
                                    });
                                }
                                else if (question.AnswerType == (int)AnswerTypeEnum.FullAddress)
                                {
                                    foreach (var quizAnswerStatsObj in quizQuestionsobj.QuizAnswerStats)
                                    {
                                        questionDetail.QuestionAnswerDetails.Add(new AttemptedAutomation.quizDetail.QuestionDetail.QuestionAnswerDetail()
                                        {
                                            AnswerId = quizAnswerStatsObj.AnswerId,
                                            AnswerTitle = quizAnswerStatsObj.AnswerText
                                        });
                                    }
                                }
                                else if (question.AnswerType == (int)AnswerTypeEnum.NPS || question.AnswerType == (int)AnswerTypeEnum.RatingEmoji || question.AnswerType == (int)AnswerTypeEnum.RatingStarts)
                                {
                                    questionDetail.QuestionAnswerDetails.Add(new AttemptedAutomation.quizDetail.QuestionDetail.QuestionAnswerDetail()
                                    {
                                        AnswerId = (quizQuestionsobj.QuizAnswerStats != null && quizQuestionsobj.QuizAnswerStats.Any(r => !r.AnswerOptionsInQuizQuestions.IsUnansweredType)) ? quizQuestionsobj.QuizAnswerStats.FirstOrDefault().AnswerId : default(int?),
                                        AnswerTitle = (quizQuestionsobj.QuizAnswerStats != null && quizQuestionsobj.QuizAnswerStats.Any(r => !r.AnswerOptionsInQuizQuestions.IsUnansweredType)) ? quizQuestionsobj.QuizAnswerStats.FirstOrDefault().AnswerText : null,
                                        Comment = (quizQuestionsobj.QuizAnswerStats != null && quizQuestionsobj.QuizAnswerStats.Any(r => !r.AnswerOptionsInQuizQuestions.IsUnansweredType)) ? quizQuestionsobj.QuizAnswerStats.FirstOrDefault().Comment : null,
                                    });
                                }

                                attemptedQuizDetail.QuestionDetails.Add(questionDetail);
                            }
                            attemptedAutomation.quizDetails.Add(attemptedQuizDetail);
                        }

                        attemptedAutomationList.Add(attemptedAutomation);
                    }

                    return attemptedAutomationList;
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public List<AttemptedAutomationAchievedResultDetails> GetAttemptedAutomationAcheivedResultDetailsByLeads(List<string> LeadIds, int QuizId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var attemptedAutomationList = new List<AttemptedAutomationAchievedResultDetails>();

                    var quizAttempts = UOWObj.QuizAttemptsRepository.Get(r => (r.QuizStats.Any() && LeadIds.Contains(r.LeadUserId)));

                    var quizResultList = UOWObj.QuizResultsRepository.GetSelectedColoumn(select: (a => new { a.Id, a.Title }), filter: (a => a.Status == (int)StatusEnum.Active)).ToList();

                    var quizStatsList = UOWObj.QuizStatsRepository.GetSelectedColoumn(select: (a => new { a.Id, a.ResultId, a.QuizAttemptId }), filter: (a => a.CompletedOn != null && a.ResultId != null)).ToList();

                    foreach (var LeadId in LeadIds)
                    {
                        var attemptedAutomation = new AttemptedAutomationAchievedResultDetails();

                        var quizAttemptsObj = quizAttempts.Where(r => ((QuizId > 0 && r.QuizDetails.ParentQuizId == QuizId && r.LeadUserId == LeadId) || (QuizId == 0 && r.LeadUserId == LeadId)));

                        attemptedAutomation.LeadId = LeadId;
                        attemptedAutomation.AchievedResults = new List<AttemptedAutomationAchievedResultDetails.AchievedResult>();

                        foreach (var obj in quizAttemptsObj)
                        {
                            var quizResults = quizStatsList.Where(r => r.QuizAttemptId == obj.Id);

                            if (quizResults.Any(r => r.ResultId != null && r.ResultId > 0 && Regex.Matches(quizResultList.FirstOrDefault(q => q.Id == r.ResultId).Title, @"%\b\S+?\b%").Count > 0))
                            {
                                var correctAnsCount = 0;
                                var ShowScoreValue = false;
                                var scoreValueTxt = string.Empty;
                                var resultSetting = obj.QuizDetails.ResultSettings.FirstOrDefault();
                                if (resultSetting != null)
                                {
                                    var attemptedQuestions = obj.QuizQuestionStats.Where(t => t.Status == (int)StatusEnum.Active && t.CompletedOn != null && (t.QuestionsInQuiz.TimerRequired ? true : t.QuizAnswerStats.Any(r => !r.AnswerOptionsInQuizQuestions.IsUnansweredType)));

                                    if (resultSetting.ShowScoreValue.HasValue && resultSetting.ShowScoreValue.Value && (obj.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || obj.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate || attemptedQuestions.Any(r => (r.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Any(t => t.Status == (int)StatusEnum.Active && t.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && t.IsCorrectForMultipleAnswer.HasValue && t.IsCorrectForMultipleAnswer.Value)) || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single)))
                                    {
                                        if (obj.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || obj.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                            correctAnsCount = attemptedQuestions.Sum(a => a.QuizAnswerStats.Sum(x => x.AnswerOptionsInQuizQuestions.AssociatedScore)).Value;
                                        else
                                            correctAnsCount = attemptedQuestions.Count(a => (a.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && ((a.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Select(s => s.Id).OrderBy(s => s)).SequenceEqual(a.QuizAnswerStats.Select(r => r.AnswerId).OrderBy(s => s)))) || (a.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single && a.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer == true));

                                        ShowScoreValue = true;
                                        scoreValueTxt = scoreValueTxt == null ? string.Empty : scoreValueTxt.Replace("%score%", ' ' + correctAnsCount.ToString() + ' ').Replace("%total%", ' ' + attemptedQuestions.Count(t => (t.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Any(r => r.Status == (int)StatusEnum.Active && r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value)) || t.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single).ToString() + ' ');
                                    }


                                    foreach (var quizResultsobj in quizResults)
                                    {
                                        var achievedResultDetails = new AttemptedAutomationAchievedResultDetails.AchievedResult();
                                        achievedResultDetails.Id = quizResultsobj.ResultId.Value;
                                        achievedResultDetails.Title = HttpUtility.HtmlDecode(Regex.Replace(VariableLinking(quizResultList.FirstOrDefault(r => r.Id == achievedResultDetails.Id).Title, obj.QuizDetails, obj, false, ShowScoreValue, scoreValueTxt), "<.*?>", string.Empty));
                                        attemptedAutomation.AchievedResults.Add(achievedResultDetails);
                                    }
                                }
                            }
                            else
                            {
                                foreach (var quizResultsobj in quizResults)
                                {
                                    var achievedResultDetails = new AttemptedAutomationAchievedResultDetails.AchievedResult();
                                    achievedResultDetails.Id = quizResultsobj.ResultId.Value;
                                    achievedResultDetails.Title = HttpUtility.HtmlDecode(Regex.Replace(quizResultList.FirstOrDefault(r => r.Id == achievedResultDetails.Id).Title, "<.*?>", string.Empty));
                                    attemptedAutomation.AchievedResults.Add(achievedResultDetails);
                                }
                            }
                        }
                        attemptedAutomationList.Add(attemptedAutomation);
                    }
                    return attemptedAutomationList;
                }


            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public void UpdateQuizQuestionSetting(QuizCorrectAnswerSetting QuizCorrectAnswerSettingObj, int BusinessUserId, int CompanyId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var existingQuestion = UOWObj.QuestionsInQuizRepository.GetByID(QuizCorrectAnswerSettingObj.QuestionId);

                    if (existingQuestion != null)
                    {
                        var currentDate = DateTime.UtcNow;
                        if (QuizCorrectAnswerSettingObj.TimerRequired && (existingQuestion.AnswerType == (int)AnswerTypeEnum.Single || existingQuestion.AnswerType == (int)AnswerTypeEnum.Multiple
                            || existingQuestion.AnswerType == (int)AnswerTypeEnum.Short || existingQuestion.AnswerType == (int)AnswerTypeEnum.Long))
                        {
                            if (QuizCorrectAnswerSettingObj.Time == null)
                            {
                                Status = ResultEnum.Error;
                                ErrorMessage = "Time should be in between 1 second to 90 minute.";
                                return;
                            }
                            else
                            {
                                existingQuestion.ViewPreviousQuestion = false;
                                existingQuestion.EditAnswer = false;
                                existingQuestion.TimerRequired = QuizCorrectAnswerSettingObj.TimerRequired;
                                existingQuestion.Time = QuizCorrectAnswerSettingObj.Time;
                                existingQuestion.AutoPlay = QuizCorrectAnswerSettingObj.AutoPlay;
                            }
                        }
                        else
                        {
                            existingQuestion.ViewPreviousQuestion = QuizCorrectAnswerSettingObj.ViewPreviousQuestion;
                            existingQuestion.EditAnswer = QuizCorrectAnswerSettingObj.EditAnswer;
                            existingQuestion.TimerRequired = false;
                            existingQuestion.Time = null;
                            existingQuestion.AutoPlay = QuizCorrectAnswerSettingObj.AutoPlay;
                        }

                        existingQuestion.LastUpdatedBy = BusinessUserId;
                        existingQuestion.LastUpdatedOn = currentDate;
                        existingQuestion.QuizDetails.LastUpdatedBy = BusinessUserId;
                        existingQuestion.QuizDetails.LastUpdatedOn = currentDate;
                        existingQuestion.QuizDetails.Quiz.State = (int)QuizStateEnum.DRAFTED;

                        UOWObj.QuizRepository.Update(existingQuestion.QuizDetails.Quiz);
                        UOWObj.Save();

                        
                        
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Question not found for the QuestionId " + QuizCorrectAnswerSettingObj.QuestionId;
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public void UpdateContenSettingtInQuiz(QuizContent ContentObj, int BusinessUserId, int CompanyId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizContentObj = UOWObj.ContentsInQuizRepository.Get(r => r.Id == ContentObj.Id && r.Status == (int)StatusEnum.Active).FirstOrDefault();

                    if (quizContentObj != null)
                    {
                        var currentDate = DateTime.UtcNow;

                        quizContentObj.ViewPreviousQuestion = ContentObj.ViewPreviousQuestion;
                        quizContentObj.AutoPlay = ContentObj.AutoPlay;
                        quizContentObj.LastUpdatedBy = BusinessUserId;
                        quizContentObj.LastUpdatedOn = currentDate;
                        quizContentObj.QuizDetails.Quiz.State = (int)QuizStateEnum.DRAFTED;
                        quizContentObj.QuizDetails.LastUpdatedBy = BusinessUserId;
                        quizContentObj.QuizDetails.LastUpdatedOn = currentDate;

                        UOWObj.ContentsInQuizRepository.Update(quizContentObj);
                        UOWObj.Save();

                        
                        
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Content not found for the ContentId " + ContentObj.Id;
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public void UpdateQuizSetting(QuizSetting QuizSettingObj, int BusinessUserId, int CompanyId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizObj = UOWObj.QuizRepository.GetByID(QuizSettingObj.QuizId);

                    if (quizObj != null)
                    {
                        var currentDate = DateTime.UtcNow;
                        var quizDetailsObj = quizObj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED);
                        if (quizDetailsObj != null)
                        {
                            quizDetailsObj.ViewPreviousQuestion = QuizSettingObj.ViewPreviousQuestion;
                            quizDetailsObj.EditAnswer = QuizSettingObj.EditAnswer;
                            quizDetailsObj.ApplyToAll = QuizSettingObj.ApplyToAll;

                            if (QuizSettingObj.ApplyToAll)
                            {
                                var quesList = quizDetailsObj.QuestionsInQuiz.Where(r => r.Status == (int)StatusEnum.Active);
                                foreach (var quesObj in quesList)
                                {
                                    quesObj.ViewPreviousQuestion = QuizSettingObj.ViewPreviousQuestion;
                                    quesObj.EditAnswer = QuizSettingObj.EditAnswer;
                                    quesObj.TimerRequired = false;
                                    quesObj.Time = null;
                                }

                                var contentList = quizDetailsObj.ContentsInQuiz.Where(r => r.Status == (int)StatusEnum.Active);
                                foreach (var contentObj in contentList)
                                {
                                    contentObj.ViewPreviousQuestion = QuizSettingObj.ViewPreviousQuestion;
                                }
                            }

                            quizDetailsObj.LastUpdatedBy = BusinessUserId;
                            quizDetailsObj.LastUpdatedOn = currentDate;
                            quizDetailsObj.LastUpdatedBy = BusinessUserId;
                            quizDetailsObj.Quiz.State = (int)QuizStateEnum.DRAFTED;

                            UOWObj.QuizRepository.Update(quizDetailsObj.Quiz);
                            UOWObj.Save();

                            
                            
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz not found for the QuizId " + QuizSettingObj.QuizId;
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public void UpdateQuizFavoriteStatus(int QuizId, bool IsFavorite, BusinessUser UserInfo)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizObj = UOWObj.QuizRepository.GetByID(QuizId);

                    if (quizObj != null && quizObj.CompanyId == UserInfo.CompanyInfo.Id)
                    {
                        var favoriteQuizByUserObj = quizObj.FavoriteQuizByUser.Where(r => r.UserTokenId == UserInfo.Id && r.QuizId == quizObj.Id);
                        if (!favoriteQuizByUserObj.Any() && IsFavorite)
                        {
                            UOWObj.FavoriteQuizByUserRepository.Insert(new Db.FavoriteQuizByUser()
                            {
                                UserTokenId = UserInfo.Id,
                                QuizId = quizObj.Id
                            });
                        }
                        else if (favoriteQuizByUserObj.Any() && !IsFavorite)
                        {
                            foreach (var obj in favoriteQuizByUserObj.ToList())
                                UOWObj.FavoriteQuizByUserRepository.Delete(obj);
                        }

                        UOWObj.QuizRepository.Update(quizObj);
                        UOWObj.Save();

                       
                    }
                    else
                    {
                        Status = ResultEnum.Ok;
                        ErrorMessage = "Quiz not found for the QuizId " + QuizId;
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public DynamicFieldDetails GetDynamicFieldByQuizId(int QuizId)
        {
            DynamicFieldDetails dynamicFieldDetails = new DynamicFieldDetails();
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quiz = UOWObj.QuizRepository.GetByID(QuizId);
                    dynamicFieldDetails.DynamicVariables = new List<string>();
                    dynamicFieldDetails.MediaVariables = new MediaVariable();
                    if (quiz != null)
                    {
                        var quizDetails = quiz.QuizDetails.LastOrDefault(r => r.State == (int)QuizStateEnum.PUBLISHED);
                        if (quizDetails != null)
                        {
                            var VariableObj = UOWObj.VariablesRepository.Get(r => r.VariableInQuiz.Any(s => s.QuizId == quizDetails.Id && s.NumberOfUses > 0));

                            foreach (var obj in VariableObj)
                            {
                                dynamicFieldDetails.DynamicVariables.Add(obj.Name);
                            }
                            dynamicFieldDetails.MediaVariables.CoverDetails = new List<Details>();
                            dynamicFieldDetails.MediaVariables.Questions = new List<ContentDetails>();
                            dynamicFieldDetails.MediaVariables.Answers = new List<Details>();
                            dynamicFieldDetails.MediaVariables.Results = new List<Details>();
                            dynamicFieldDetails.MediaVariables.Content = new List<ContentDetails>();
                            dynamicFieldDetails.MediaVariables.Badges = new List<Details>();

                            if (quizDetails.QuizComponentLogs.Any())
                            {
                                var quizComponentLogsList = quizDetails.QuizComponentLogs;

                                #region cover

                                if (quizDetails.EnableMediaFile && quizComponentLogsList.Any(r => r.PublishedObjectId == quizDetails.Id && r.ObjectTypeId == (int)BranchingLogicEnum.COVERDETAILS))
                                {
                                    var coverQuizComponentLogs = quizComponentLogsList.Where(r => r.PublishedObjectId == quizDetails.Id && r.ObjectTypeId == (int)BranchingLogicEnum.COVERDETAILS);
                                    var cover = new Details();
                                    cover.Id = coverQuizComponentLogs.FirstOrDefault().PublishedObjectId;
                                    cover.ParentId = coverQuizComponentLogs.FirstOrDefault().DraftedObjectId;
                                    cover.Title = quizDetails.QuizCoverTitle;
                                    cover.MediaUrl = quizDetails.QuizCoverImage ?? string.Empty;
                                    cover.PublicId = quizDetails.PublicId ?? string.Empty;
                                    cover.TypeId = (int)BranchingLogicEnum.COVERDETAILS;

                                    dynamicFieldDetails.MediaVariables.CoverDetails.Add(cover);
                                }

                                #endregion

                                #region question

                                var questionList = quizDetails.QuestionsInQuiz;

                                if (quizComponentLogsList.Any(r => questionList.Any(q => (q.EnableMediaFile || q.EnableMediaFileForDescription) && q.Id == r.PublishedObjectId) && r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION))
                                {
                                    var quizComponentLogs = quizComponentLogsList.Where(r => questionList.Any(q => (q.EnableMediaFile || q.EnableMediaFileForDescription) && q.Id == r.PublishedObjectId) && r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION);

                                    foreach (var obj in quizComponentLogs)
                                    {
                                        var question = questionList.FirstOrDefault(r => r.Id == obj.PublishedObjectId);

                                        var details = new ContentDetails();
                                        details.Id = obj.PublishedObjectId;
                                        details.ParentId = obj.DraftedObjectId;
                                        if (question.EnableMediaFile)
                                        {
                                            details.Title = question.Question;
                                            details.MediaUrl = question.QuestionImage;
                                            details.PublicId = question.PublicId ?? string.Empty;
                                        }
                                        details.TypeId = question.Type;
                                        if (question.EnableMediaFileForDescription)
                                        {
                                            details.Description = question.Description ?? string.Empty;
                                            details.MediaUrlforDescription = question.DescriptionImage ?? string.Empty;
                                            details.PublicIdforDescription = question.PublicIdForDescription ?? string.Empty;
                                        }

                                        if (question.Type == (int)BranchingLogicEnum.QUESTION)
                                            dynamicFieldDetails.MediaVariables.Questions.Add(details);
                                        else if (question.Type == (int)BranchingLogicEnum.CONTENT)
                                            dynamicFieldDetails.MediaVariables.Content.Add(details);
                                    }
                                }

                                #endregion

                                #region Answer

                                foreach (var ques in questionList)
                                {
                                    var answerList = ques.AnswerOptionsInQuizQuestions;

                                    if (quizComponentLogsList.Any(r => answerList.Any(q => q.EnableMediaFile && q.Id == r.PublishedObjectId) && r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER))
                                    {
                                        var quizComponentLogs = quizComponentLogsList.Where(r => answerList.Any(q => q.EnableMediaFile && q.Id == r.PublishedObjectId) && r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER);

                                        foreach (var obj in quizComponentLogs)
                                        {
                                            var answer = answerList.FirstOrDefault(r => r.Id == obj.PublishedObjectId);

                                            var details = new Details();
                                            details.Id = obj.PublishedObjectId;
                                            details.ParentId = obj.DraftedObjectId;
                                            details.Title = answer.Option;
                                            details.AnswerDescription = answer.Description;
                                            details.MediaUrl = answer.OptionImage;
                                            details.PublicId = answer.PublicId ?? string.Empty;
                                            details.TypeId = (int)BranchingLogicEnum.ANSWER;

                                            dynamicFieldDetails.MediaVariables.Answers.Add(details);
                                        }
                                    }

                                }

                                #endregion

                                #region Result

                                var resultList = quizDetails.QuizResults;

                                if (quizComponentLogsList.Any(r => resultList.Any(q => q.EnableMediaFile && q.Id == r.PublishedObjectId) && r.ObjectTypeId == (int)BranchingLogicEnum.RESULT))
                                {
                                    var quizComponentLogs = quizComponentLogsList.Where(r => resultList.Any(q => q.EnableMediaFile && q.Id == r.PublishedObjectId) && r.ObjectTypeId == (int)BranchingLogicEnum.RESULT);

                                    foreach (var obj in quizComponentLogs)
                                    {
                                        var result = resultList.FirstOrDefault(r => r.Id == obj.PublishedObjectId);

                                        var details = new Details();
                                        details.Id = obj.PublishedObjectId;
                                        details.ParentId = obj.DraftedObjectId;
                                        details.Title = result.Title;
                                        details.MediaUrl = result.Image;
                                        details.PublicId = result.PublicId ?? string.Empty;
                                        details.TypeId = (int)BranchingLogicEnum.RESULT;

                                        dynamicFieldDetails.MediaVariables.Results.Add(details);
                                    }
                                }

                                #endregion

                                #region Content

                                var contentList = quizDetails.ContentsInQuiz;

                                if (quizComponentLogsList.Any(r => contentList.Any(q => (q.EnableMediaFileForTitle || q.EnableMediaFileForDescription) && q.Id == r.PublishedObjectId) && r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT))
                                {
                                    var quizComponentLogs = quizComponentLogsList.Where(r => contentList.Any(q => (q.EnableMediaFileForTitle || q.EnableMediaFileForDescription) && q.Id == r.PublishedObjectId) && r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT);

                                    foreach (var obj in quizComponentLogs)
                                    {
                                        var content = contentList.FirstOrDefault(r => r.Id == obj.PublishedObjectId);

                                        var details = new ContentDetails();
                                        details.Id = obj.PublishedObjectId;
                                        details.ParentId = obj.DraftedObjectId;
                                        if (content.EnableMediaFileForTitle)
                                        {
                                            details.Title = content.ContentTitle;
                                            details.MediaUrl = content.ContentTitleImage;
                                            details.PublicId = content.PublicIdForContentTitle ?? string.Empty;
                                        }
                                        if (content.EnableMediaFileForDescription)
                                        {
                                            details.Description = content.ContentDescription;
                                            details.MediaUrlforDescription = content.ContentDescriptionImage;
                                            details.PublicIdforDescription = content.PublicIdForContentDescription ?? string.Empty;
                                        }

                                        details.TypeId = (int)BranchingLogicEnum.CONTENT;

                                        dynamicFieldDetails.MediaVariables.Content.Add(details);
                                    }
                                }

                                #endregion

                                #region Badges

                                var badgesList = quizDetails.BadgesInQuiz;

                                if (quizComponentLogsList.Any(r => badgesList.Any(q => q.EnableMediaFile && q.Id == r.PublishedObjectId) && r.ObjectTypeId == (int)BranchingLogicEnum.BADGE))
                                {
                                    var quizComponentLogs = quizComponentLogsList.Where(r => badgesList.Any(q => q.EnableMediaFile && q.Id == r.PublishedObjectId) && r.ObjectTypeId == (int)BranchingLogicEnum.BADGE);

                                    foreach (var obj in quizComponentLogs)
                                    {
                                        var badges = badgesList.FirstOrDefault(r => r.Id == obj.PublishedObjectId);

                                        var details = new Details();
                                        details.Id = obj.PublishedObjectId;
                                        details.ParentId = obj.DraftedObjectId;
                                        details.Title = badges.Title;
                                        details.MediaUrl = badges.Image;
                                        details.PublicId = badges.PublicId ?? string.Empty;
                                        details.TypeId = (int)BranchingLogicEnum.BADGE;

                                        dynamicFieldDetails.MediaVariables.Badges.Add(details);
                                    }
                                }

                                #endregion


                            }
                        }
                        else
                        {
                            Status = ResultEnum.Error;
                            ErrorMessage = "Quiz is not yet published.";
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz not found for the Quiz Id " + QuizId;
                    }
                    return dynamicFieldDetails;
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public string GetUrlValueByKey(string Key, string DomainName)
        {
            try
            {
                string urlValue = string.Empty;
                string configId = string.Empty;
                string sourceId = string.Empty;
                if (!string.IsNullOrWhiteSpace(Key) && (Key.Contains('$'))) {
                    var keylist = Key.Split('$').ToList();
                    if (keylist.Count > 1) {
                        Key = keylist[0];
                        sourceId = keylist[1];
                    }
                }
                else if (!string.IsNullOrWhiteSpace(Key) && (Key.Contains('#'))) {
                    var keylist = Key.Split('#').ToList();
                    if (keylist.Count > 1) {
                        Key = keylist[0];
                        sourceId = keylist[1];
                    }
                }
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizUrlSettingObj = UOWObj.QuizUrlSettingRepository.Get(r => r.Key == Key && r.DomainName == DomainName);

                    if (quizUrlSettingObj != null && quizUrlSettingObj.Any())
                    {
                        urlValue = quizUrlSettingObj.FirstOrDefault().Value;
                        if (urlValue.ContainsCI("Code")) {

                            return urlValue;
                        }
                        else {
                            if (string.IsNullOrWhiteSpace(sourceId)) {
                                int index = urlValue.IndexOf("=");
                                configId = urlValue.Substring(index + 1);
                                sourceId = UOWObj.ConfigurationDetailsRepository.Get(v => v.ConfigurationId == configId).FirstOrDefault()?.SourceId;
                            }
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Ok;
                        ErrorMessage = "Data not found for the key = " + Key + " and domain = " + DomainName + " and SourceId = "+ sourceId;
                    }
                }
                
                return urlValue+"&sourceId="+sourceId;
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public void UpdateAnswerObjectFieldsDetails(List<ObjectFieldsDetails> objectFieldsDetailList, int CompanyId, int BusinessUserId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    foreach (var objectFieldsDetails in objectFieldsDetailList)
                    {
                           var answerOptionsInQuizQuestions = UOWObj.AnswerOptionsInQuizQuestionsRepository.GetByID(objectFieldsDetails.AnswerId);
                        if (answerOptionsInQuizQuestions != null && answerOptionsInQuizQuestions.ObjectFieldsInAnswer != null && answerOptionsInQuizQuestions.ObjectFieldsInAnswer.Any()) {
                            var item = answerOptionsInQuizQuestions.ObjectFieldsInAnswer.Where(v => v.AnswerOptionsInQuizQuestionsId == objectFieldsDetails.AnswerId && v.IsCommentMapped == objectFieldsDetails.IsCommentMapped).FirstOrDefault();
                            if (item != null && string.IsNullOrWhiteSpace(objectFieldsDetails.FieldName) || string.IsNullOrWhiteSpace(objectFieldsDetails.ObjectName)) {
                                UOWObj.ObjectFieldsInAnswerRepository.Delete(item);
                            } else if (item != null && !string.IsNullOrWhiteSpace(objectFieldsDetails.FieldName) && !string.IsNullOrWhiteSpace(objectFieldsDetails.ObjectName)) {
                                item.ObjectName = objectFieldsDetails.ObjectName;
                                item.FieldName = objectFieldsDetails.FieldName;
                                item.Value = objectFieldsDetails.Value;
                                item.LastUpdatedBy = BusinessUserId;
                                item.LastUpdatedOn = DateTime.UtcNow;
                                item.IsExternalSync = objectFieldsDetails.IsExternalSync;
                                item.IsCommentMapped = objectFieldsDetails.IsCommentMapped;

                                UOWObj.ObjectFieldsInAnswerRepository.Update(item);
                            } else {
                                if (!string.IsNullOrWhiteSpace(objectFieldsDetails.FieldName) && !string.IsNullOrWhiteSpace(objectFieldsDetails.ObjectName)) {
                                    var obj = new Db.ObjectFieldsInAnswer();
                                    obj.AnswerOptionsInQuizQuestionsId = objectFieldsDetails.AnswerId;
                                    obj.ObjectName = objectFieldsDetails.ObjectName;
                                    obj.FieldName = objectFieldsDetails.FieldName;
                                    obj.Value = objectFieldsDetails.Value;
                                    obj.IsExternalSync = objectFieldsDetails.IsExternalSync;
                                    obj.CreatedBy = BusinessUserId;
                                    obj.CreatedOn = DateTime.UtcNow;
                                    obj.IsCommentMapped = objectFieldsDetails.IsCommentMapped;
                                    UOWObj.ObjectFieldsInAnswerRepository.Insert(obj);
                                }
                            }


                            var quizDetailsObj = answerOptionsInQuizQuestions.QuestionsInQuiz.QuizDetails;

                            quizDetailsObj.LastUpdatedBy = BusinessUserId;
                            quizDetailsObj.LastUpdatedOn = DateTime.UtcNow;

                            quizDetailsObj.Quiz.State = (int)QuizStateEnum.DRAFTED;

                            UOWObj.QuizRepository.Update(quizDetailsObj.Quiz);

                            UOWObj.Save();
						} 
                        else {
                            if (!string.IsNullOrWhiteSpace(objectFieldsDetails.FieldName) && !string.IsNullOrWhiteSpace(objectFieldsDetails.ObjectName) && objectFieldsDetails.IsExternalSync == true) {
                                var obj = new Db.ObjectFieldsInAnswer();
                                obj.AnswerOptionsInQuizQuestionsId = objectFieldsDetails.AnswerId;
                                obj.ObjectName = objectFieldsDetails.ObjectName;
                                obj.FieldName = objectFieldsDetails.FieldName;
                                obj.Value = objectFieldsDetails.Value;
                                obj.IsExternalSync = objectFieldsDetails.IsExternalSync;
                                obj.CreatedBy = BusinessUserId;
                                obj.CreatedOn = DateTime.UtcNow;
                                obj.IsCommentMapped = objectFieldsDetails.IsCommentMapped;
                                UOWObj.ObjectFieldsInAnswerRepository.Insert(obj);
                                UOWObj.Save();
                            }
                        }
                    }
                    
                }
            }


            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }

        }

        public void RemoveAnswerObjectFieldsDetails(int AnswerId, int CompanyId, int BusinessUserId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var answerOptionsInQuizQuestions = UOWObj.AnswerOptionsInQuizQuestionsRepository.GetByID(AnswerId);
                    if (answerOptionsInQuizQuestions != null)
                    {
                        if (answerOptionsInQuizQuestions.ObjectFieldsInAnswer.Any())
                        {
                            foreach (var objectFieldsInAnswerObj in answerOptionsInQuizQuestions.ObjectFieldsInAnswer.ToList())
                                UOWObj.ObjectFieldsInAnswerRepository.Delete(objectFieldsInAnswerObj);

                            var quizDetailsObj = answerOptionsInQuizQuestions.QuestionsInQuiz.QuizDetails;

                            quizDetailsObj.LastUpdatedBy = BusinessUserId;
                            quizDetailsObj.LastUpdatedOn = DateTime.UtcNow;

                            quizDetailsObj.Quiz.State = (int)QuizStateEnum.DRAFTED;

                            UOWObj.QuizRepository.Update(quizDetailsObj.Quiz);

                            UOWObj.Save();
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public void UpdateQuizUsageTypeandTagDetails(QuizUsageTypeandTagDetails Obj, BusinessUser UserInfo)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizObj = UOWObj.QuizRepository.GetByID(Obj.QuizId);

                    var companyId = UserInfo.CompanyInfo.Id;

                    if (quizObj != null && quizObj.CompanyId == companyId)
                    {
                        bool usageTypeEntryExist = false;
                        foreach (var item in quizObj.QuizTagDetails.ToList())
                        {
                            UOWObj.QuizTagDetailsRepository.Delete(item);
                        }

                        foreach (var item in Obj.TagIds)
                        {
                            var quizTagDetails = new Db.QuizTagDetails();
                            quizTagDetails.QuizId = Obj.QuizId;
                            quizTagDetails.TagId = item;
                            quizTagDetails.CompanyId = quizObj.CompanyId.HasValue ? quizObj.CompanyId.Value : 0;
                            UOWObj.QuizTagDetailsRepository.Insert(quizTagDetails);
                        }

                        if (quizObj.UsageTypeInQuiz != null && quizObj.UsageTypeInQuiz.Any())
                        {
                            usageTypeEntryExist = true;
                        }

                        foreach (var item in quizObj.UsageTypeInQuiz.ToList())
                        {
                            UOWObj.UsageTypeInQuizRepository.Delete(item);
                            UOWObj.Save();
                        }

                        foreach (var item in Obj.UsageTypes.ToList())
                        {
                            var usageTypeInQuizObj = new Db.UsageTypeInQuiz();
                            usageTypeInQuizObj.QuizId = Obj.QuizId;
                            usageTypeInQuizObj.UsageType = item;
                            usageTypeInQuizObj.LastUpdatedBy = UserInfo.BusinessUserId;
                            usageTypeInQuizObj.LastUpdatedOn = DateTime.UtcNow;
                            UOWObj.UsageTypeInQuizRepository.Insert(usageTypeInQuizObj);
                        }

                        if (Obj.UsageTypes.Contains((int)UsageTypeEnum.WhatsAppChatbot))
                        {
                            var quizDetailObj = quizObj.QuizDetails.FirstOrDefault(a => a.State == (int)QuizStateEnum.DRAFTED);
                            foreach (var question in quizDetailObj.QuestionsInQuiz)
                            {
                                if (question.AnswerStructureType == 0 || question.AnswerStructureType == null)
                                {
                                    if (question.AnswerType == (int)AnswerTypeEnum.Single || question.AnswerType == (int)AnswerTypeEnum.LookingforJobs)
                                    {
                                        question.AnswerStructureType = (int)AnswerStructureTypeEnum.Button;
                                    }
                                    else if (question.AnswerType == (int)AnswerTypeEnum.NPS || question.AnswerType == (int)AnswerTypeEnum.RatingStarts || question.AnswerType == (int)AnswerTypeEnum.RatingEmoji)
                                    {
                                        question.AnswerStructureType = (int)AnswerStructureTypeEnum.List;
                                    }

                                }

                                if (!usageTypeEntryExist)
                                {
                                    question.ShowTitle = false;
                                }

                                question.Description = "Message";
                                UOWObj.QuestionsInQuizRepository.Update(question);
                            }

                        }

                        UOWObj.Save();

                        
                        
                    }
                    else
                    {
                        Status = ResultEnum.Ok;
                        ErrorMessage = "Quiz not found for the QuizId " + Obj.QuizId;
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public QuizUsageTypeDetails GetQuizUsageTypeDetails(int QuizId, int CompanyId)
        {
            QuizUsageTypeDetails quizUsageTypeDetails = null;

            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizObj = UOWObj.QuizRepository.GetByID(QuizId);

                    if (quizObj != null && quizObj.CompanyId == CompanyId)
                    {
                        quizUsageTypeDetails = new QuizUsageTypeDetails();

                        quizUsageTypeDetails.UsageTypes = new List<int>();

                        foreach (var item in quizObj.UsageTypeInQuiz)
                        {
                            quizUsageTypeDetails.UsageTypes.Add(item.UsageType);
                        }

                        quizUsageTypeDetails.Tag = new List<Tags>();

                        var quizTagDetailsList = quizObj.QuizTagDetails.ToList();

                        if (quizTagDetailsList.Any())
                        {
                            var automationTagsList = CommonStaticData.GetCachedAutomationTagsList(quizObj.Company.ClientCode);

                            foreach (var item in quizTagDetailsList)
                            {
                                if (automationTagsList != null && automationTagsList.Any(r => r.tagId == item.TagId))
                                {
                                    var tagDetails = automationTagsList.First(r => r.tagId == item.TagId);
                                    quizUsageTypeDetails.Tag.Add(new Tags()
                                    {
                                        TagId = item.TagId,
                                        TagName = tagDetails.tagName,
                                        TagCode = tagDetails.tagCode
                                    });
                                }
                            }
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Ok;
                        ErrorMessage = "Quiz not found for the QuizId " + QuizId;
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }

            return quizUsageTypeDetails;
        }

        public int GetQuizIdByAttemptCode(string AttemptCode)
        {
            int quizId = 0;

            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizAttemptsObj = UOWObj.QuizAttemptsRepository.Get(r => r.Code == AttemptCode);
                    if (quizAttemptsObj != null && quizAttemptsObj.Any())
                    {
                        return quizAttemptsObj.FirstOrDefault().QuizDetails.ParentQuizId;
                    }
                    else
                    {
                        Status = ResultEnum.Ok;
                        ErrorMessage = "Quiz not found for the AttemptCode " + AttemptCode;
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }

            return quizId;
        }

        public void UpdateAnswerOptionValues(int AnswerId, List<string> ListValues, int BusinessUserId, int CompanyId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var answerObj = UOWObj.AnswerOptionsInQuizQuestionsRepository.GetByID(AnswerId);

                    if (answerObj != null)
                    {
                        var currentDate = DateTime.UtcNow;
                        answerObj.ListValues = ListValues != null && ListValues.Any() ? string.Join(",", ListValues) : null;
                        answerObj.LastUpdatedBy = BusinessUserId;
                        answerObj.LastUpdatedOn = currentDate;

                        UOWObj.AnswerOptionsInQuizQuestionsRepository.Update(answerObj);
                        UOWObj.Save();

                        
                        
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Answer not found for the AnswerId " + AnswerId;
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }
    }
}
