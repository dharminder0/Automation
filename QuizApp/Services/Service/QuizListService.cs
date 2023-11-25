using QuizApp.Helpers;
using QuizApp.RepositoryPattern;
using QuizApp.Request;
using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Services.Service
{
    public class QuizListService : IQuizListService
    {

        private ResultEnum status = ResultEnum.Ok;
        private string errormessage = string.Empty;
        public ResultEnum Status { get { return status; } set { status = value; } }
        public string ErrorMessage { get { return errormessage; } set { errormessage = value; } }

        public List<LocalQuiz> GetList(int businessUserID, List<string> officeIdList, bool includeSharedWithMe, long OffsetValue, string searchTxt, string quizTypeId, CompanyModel companyInfo, bool isDataforGlobalOfficeAdmin, bool isGlobalOfficeAdmin, int? userInfoId, string quizId, bool? isFavorite, bool? isPublished, bool isCreateStandardAutomation, int? quizTagId)
        {
            List<LocalQuiz> lstQuiz = new List<LocalQuiz>();

            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizTypeIds = string.IsNullOrEmpty(quizTypeId) ? new List<int>() : quizTypeId.Split(',').Select(Int32.Parse).ToList();
                    var QuizIds = string.IsNullOrEmpty(quizId) ? new List<int>() : quizId.Split(',').Select(Int32.Parse).ToList();

                    if (!string.IsNullOrEmpty(searchTxt))
                        searchTxt = searchTxt.ToUpper();

                    var companyId = companyInfo.Id;

                    IEnumerable<Db.Quiz> quizList = new List<Db.Quiz>();


                    quizList = UOWObj.QuizRepository
                                .GetQueryable(r => (QuizIds.Any() ? QuizIds.Any(s => s == r.Id) : true)
                                       && (quizTypeIds.Contains(r.QuizType))
                                       && (string.IsNullOrEmpty(searchTxt) ? true : (string.IsNullOrEmpty(r.QuizDetails.FirstOrDefault(t => t.State == (int)QuizStateEnum.DRAFTED).QuizTitle) ? false : r.QuizDetails.FirstOrDefault(t => t.State == (int)QuizStateEnum.DRAFTED).QuizTitle.ToUpper().IndexOf(searchTxt) > -1))
                                       && (r.Company.Id == companyId)
                                       && (isPublished.HasValue ? (isPublished.Value ? (r.State == (int)QuizStateEnum.PUBLISHED) : (r.State != (int)QuizStateEnum.PUBLISHED)) : true)
                                       && (isFavorite.HasValue ? (isFavorite.Value ? r.FavoriteQuizByUser.Any(f => f.QuizId == r.Id && f.UserTokenId == userInfoId) : !r.FavoriteQuizByUser.Any(f => f.QuizId == r.Id && f.UserTokenId == userInfoId)) : true)
                                       && (quizTagId.HasValue ? r.QuizTagDetails.Any(q => q.TagId == quizTagId.Value) : true)
                                       && (!string.IsNullOrEmpty(r.AccessibleOfficeId) ? (((isGlobalOfficeAdmin && isDataforGlobalOfficeAdmin && !officeIdList.Any()) ? true : officeIdList.Contains(r.AccessibleOfficeId)))
                                                                         : (includeSharedWithMe))
                     , includeProperties: "QuizDetails, QuizDetails.QuestionsInQuiz, QuizTagDetails, FavoriteQuizByUser,UsageTypeInQuiz");

                    string[] extensions = new string[] { "MP4", "WEBM", "MPG", "OBG", "MOV", "mp4" };

                    var automationTagsList = CommonStaticData.GetCachedAutomationTagsList(companyInfo.ClientCode);

                    foreach (var quiz in quizList)
                    {
                        var quizObj = new LocalQuiz();

                        var quizDetailsObj = quiz.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active);

                        if (quizDetailsObj != null)
                        {
                            quizObj.Id = quiz.Id;
                            quizObj.QuizTitle = quizDetailsObj.QuizTitle;
                            quizObj.IsPublished = quiz.State == (int)QuizStateEnum.PUBLISHED;
                            quizObj.CreatedByID = quizDetailsObj.CreatedBy;
                            if (quizDetailsObj.LastUpdatedOn.HasValue)
                            {
                                quizObj.LastEditDate = Utility.ConvertUTCDateToLocalDate(quizDetailsObj.LastUpdatedOn.Value, OffsetValue);
                            }
                            quizObj.CreatedOn = Utility.ConvertUTCDateToLocalDate(quizDetailsObj.CreatedOn, OffsetValue);
                            quizObj.QuizCoverDetails = new QuizCover();
                            quizObj.QuizCoverDetails.QuizCoverImage = (quizDetailsObj.QuizCoverImage != null &&
                                                                     quizDetailsObj.QuizCoverImage.Contains("/video/upload/") &&
                                                                     (quizDetailsObj.QuizCoverImage.EndsWith("MP4") || quizDetailsObj.QuizCoverImage.EndsWith("WEBM") || quizDetailsObj.QuizCoverImage.EndsWith("MPG") || quizDetailsObj.QuizCoverImage.EndsWith("OBG") || quizDetailsObj.QuizCoverImage.EndsWith("MOV") || quizDetailsObj.QuizCoverImage.EndsWith("mp4"))
                                                                         ? quizDetailsObj.QuizCoverImage.Split(extensions, System.StringSplitOptions.RemoveEmptyEntries).GetValue(0).ToString() + "jpg"
                                                                         : quizDetailsObj.QuizCoverImage);
                            quizObj.QuizCoverDetails.QuizCoverTitle = quizDetailsObj.QuizCoverTitle;
                            quizObj.QuizCoverDetails.PublicIdForQuizCover = quizDetailsObj.PublicId;
                            quizObj.NoOfQusetions = quizDetailsObj.QuestionsInQuiz.Count(r => r.Status == (int)StatusEnum.Active);

                            quizObj.PublishedCode = quiz.PublishedCode;
                            quizObj.QuizType = (QuizTypeEnum)quiz.QuizType;
                            quizObj.UsageTypes = quiz.UsageTypeInQuiz.Select(v => v.UsageType).Select(c => int.Parse(c.ToString())).ToList();
                            if (!string.IsNullOrEmpty(quiz.AccessibleOfficeId))
                            {
                                quizObj.AccessibleOfficeId = quiz.AccessibleOfficeId;
                            }

                            if (quiz.QuizDetails.FirstOrDefault().CreatedBy == businessUserID)
                                quizObj.IsCreatedByYou = true;

                            quizObj.IsFavorited = quiz.FavoriteQuizByUser.Any(f => f.QuizId == quiz.Id && f.UserTokenId == userInfoId);

                            quizObj.IsCreateStandardAutomation = isCreateStandardAutomation;
                            //if (quiz.UsageTypeInQuiz != null)
                            //{
                            //    quizObj.UsageTypes = new List<int>();
                            //    foreach (var item in quiz.UsageTypeInQuiz.ToList())
                            //    {
                            //        quizObj.UsageTypes.Add(item.UsageType);
                            //    }
                            //}


                            quizObj.Tag = new List<Tags>();
                            foreach (var tag in quiz.QuizTagDetails.ToList())
                            {
                                quizObj.Tag.Add(new Tags()
                                {
                                    TagId = tag.TagId,
                                    TagName = (automationTagsList != null && automationTagsList.Any(r => r.tagId == tag.TagId)) ? automationTagsList.FirstOrDefault(r => r.tagId == tag.TagId).tagName : string.Empty,
                                    TagCode = (automationTagsList != null && automationTagsList.Any(r => r.tagId == tag.TagId)) ? automationTagsList.FirstOrDefault(r => r.tagId == tag.TagId).tagCode : string.Empty
                                });
                            }

                            lstQuiz.Add(quizObj);
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

            return lstQuiz;
        }

        public QuizList GetListWithPagination(int businessUserID, List<string> officeIdList, bool includeSharedWithMe, long OffsetValue, int? pageNo, int? pageSize, string searchTxt, int? orderBy, string quizTypeId, CompanyModel companyInfo, bool isDataforGlobalOfficeAdmin, bool isGlobalOfficeAdmin, int? userInfoId, string quizId, bool? isFavorite, bool isCreateStandardAutomation, string MustIncludeQuizId, int? usageType, int? quizTagId, bool? IsWhatsAppChatBotOldVersion)
        {

            QuizList quizlist = new QuizList();
            List<LocalQuiz> lstQuiz = new List<LocalQuiz>();
            var quizTypeIds = string.IsNullOrEmpty(quizTypeId) ? new List<int>() : quizTypeId.Split(',').Select(Int32.Parse).ToList();
            var QuizIds = string.IsNullOrEmpty(quizId) ? new List<int>() : quizId.Split(',').Select(Int32.Parse).ToList();
            var mustIncludeQuizId = string.IsNullOrEmpty(MustIncludeQuizId) ? new List<int>() : MustIncludeQuizId.Split(',').Select(Int32.Parse).ToList();

            int totalCount = 0;
            var quizs = new List<Db.Quiz>();
            var quizList = new List<Db.Quiz>();
            if (!string.IsNullOrEmpty(searchTxt))
                searchTxt = searchTxt.ToUpper();

            var CompanyId = companyInfo.Id;
            List<int> favouriteQuizId = new List<int>();
            List<int> searchQuizId = new List<int>();

            if (!pageNo.HasValue || !pageSize.HasValue)
            {
                if (!pageNo.HasValue)
                {
                    pageNo = 1;
                }
                if (!pageSize.HasValue)
                {
                    pageSize = 10;
                }
            }
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    if (mustIncludeQuizId.Any())
                    {
                        var mustIncludeQuizIdList = UOWObj.QuizRepository.Get(filter: r => (r.Company.Id == CompanyId) && (mustIncludeQuizId.Any(c => c == r.Id)),
                            orderBy: null,
                            includeProperties: "QuizDetails, QuizDetails.QuestionsInQuiz, QuizTagDetails, FavoriteQuizByUser, UsageTypeInQuiz").ToList();
                        if (mustIncludeQuizIdList != null && mustIncludeQuizIdList.Any())
                            quizList.AddRange(mustIncludeQuizIdList);
                        quizlist.TotalRecords = mustIncludeQuizIdList.Count();
                    }
                    pageSize = ((pageSize - quizlist.TotalRecords) > 0) ? pageSize - quizlist.TotalRecords : 0;
                    if (userInfoId.HasValue && isFavorite.HasValue)
                    {
                        var favouritequiz = UOWObj.FavoriteQuizByUserRepository.GetSelectedColoumn(a => new { a.QuizId }, filter: (a => a.UserTokenId == userInfoId)).ToList();
                        if (favouritequiz != null && favouritequiz.Any())
                        {
                            favouriteQuizId = favouritequiz.Select(r => r.QuizId).ToList();
                        }
                    }
                    if (!string.IsNullOrEmpty(searchTxt))
                    {
                        var searchTxtQuiz = UOWObj.QuizRepository.GetSelectedColoumn(a => new { a.Id }, filter: (r => (r.Company.Id == CompanyId) && (string.IsNullOrEmpty(r.QuizDetails.FirstOrDefault(t => t.State == (int)QuizStateEnum.DRAFTED).QuizTitle) ? false : r.QuizDetails.FirstOrDefault(t => t.State == (int)QuizStateEnum.DRAFTED).QuizTitle.ToUpper().IndexOf(searchTxt) > -1))).ToList();
                        if (searchTxtQuiz != null && searchTxtQuiz.Any())
                        {
                            searchQuizId = searchTxtQuiz.Select(r => r.Id).ToList();
                        }
                    }
                    //if (!string.IsNullOrEmpty(SearchTxt))
                    //{
                    //    var searchTxtQuiz = UOWObj.QuizDetailsRepository.GetSelectedColoumn(a => new { a.ParentQuizId }, filter: (r => r.State == (int)QuizStateEnum.DRAFTED && r.QuizTitle.IndexOf(SearchTxt) > -1)).ToList();
                    //    if (searchTxtQuiz != null && searchTxtQuiz.Any())
                    //    {
                    //        searchQuizId = searchTxtQuiz.Select(r => r.ParentQuizId).ToList();
                    //    }
                    //}

                    System.Linq.Expressions.Expression<Func<Db.Quiz, bool>> filter = (r =>
                                                (QuizIds.Any() ? QuizIds.Any(s => s == r.Id) : true)
                                               && (favouriteQuizId.Any() ? favouriteQuizId.Any(s => s == r.Id) : true)
                                               && (searchQuizId.Any() ? searchQuizId.Any(s => s == r.Id) : true)
                                               && (quizTypeIds.Contains(r.QuizType))
                                               && (usageType.HasValue ? r.UsageTypeInQuiz.Any(u => u.UsageType == usageType.Value) : true)
                                               && (r.Company.Id == CompanyId)
                                               && (IsWhatsAppChatBotOldVersion.HasValue ? IsWhatsAppChatBotOldVersion.Value == true ? r.IsWhatsAppChatBotOldVersion == true : r.IsWhatsAppChatBotOldVersion != true : true)
                                               && (!mustIncludeQuizId.Any(c => c == r.Id))
                                               && (r.QuizDetails.Any(s => s.State == (int)QuizStateEnum.PUBLISHED && s.Status == (int)StatusEnum.Active))
                                               && (quizTagId.HasValue ? r.QuizTagDetails.Any(q => q.TagId == quizTagId.Value) : true)
                                               && (r.UserPermissionsInQuiz.Any() ? r.UserPermissionsInQuiz.Any(p => (p.UserTypeId == (int)UserTypeEnum.Lead)) : false)
                                               && (r.QuizDetails.FirstOrDefault(s => s.State == (int)QuizStateEnum.DRAFTED && s.Status == (int)StatusEnum.Active) != null)
                                               && (!string.IsNullOrEmpty(r.AccessibleOfficeId) ? ((isGlobalOfficeAdmin && isDataforGlobalOfficeAdmin && !officeIdList.Any()) ? true : officeIdList.Contains(r.AccessibleOfficeId))
                                                                          : (includeSharedWithMe)));


                    quizs = UOWObj.QuizRepository.GetWithPagination(out totalCount, pageNo.Value, pageSize.Value, filter: filter,
                                             orderBy: r => orderBy == (int)OrderByEnum.Ascending ? (r.OrderBy(k => k.QuizDetails.OrderBy(s => s.Version).FirstOrDefault(s => s.State == (int)QuizStateEnum.DRAFTED).CreatedOn))
                                                                                                 : (r.OrderByDescending(k => k.QuizDetails.OrderByDescending(s => s.Version).FirstOrDefault(s => s.State == (int)QuizStateEnum.DRAFTED).CreatedOn))
                                                , includeProperties: "QuizDetails, QuizTagDetails,  UsageTypeInQuiz").ToList();

                    if (quizs != null && quizs.Any())
                        quizList.AddRange(quizs);

                    quizlist.CurrentPageIndex = pageNo.Value;
                    quizlist.TotalRecords += totalCount;
                    pageNo = pageNo.HasValue ? pageNo.Value : 1;
                    pageSize = pageSize.HasValue ? pageSize.Value : quizlist.TotalRecords;

                    // var objList = quizList.Skip(PageSize.Value * (PageNo.Value - 1)).Take(PageSize.Value);

                    string[] extensions = new string[] { "MP4", "WEBM", "MPG", "OBG", "MOV", "mp4" };

                    var automationTagsList = CommonStaticData.GetCachedAutomationTagsList(companyInfo.ClientCode);

                    foreach (var quiz in quizList)
                    {
                        var quizObj = new LocalQuiz();

                        var quizDetailsObj = quiz.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active);

                        if (quizDetailsObj != null)
                        {
                            quizObj.Id = quiz.Id;
                            quizObj.QuizTitle = quizDetailsObj.QuizTitle;
                            quizObj.IsWhatsAppChatBotOldVersion = quiz.IsWhatsAppChatBotOldVersion.HasValue ? quiz.IsWhatsAppChatBotOldVersion.Value : false;
                            quizObj.IsPublished = quiz.State == (int)QuizStateEnum.PUBLISHED;
                            quizObj.CreatedByID = quizDetailsObj.CreatedBy;


                            if (quizDetailsObj.LastUpdatedOn.HasValue)
                            {
                                quizObj.LastEditDate = Utility.ConvertUTCDateToLocalDate(quizDetailsObj.LastUpdatedOn.Value, OffsetValue);
                            }

                            quizObj.CreatedOn = Utility.ConvertUTCDateToLocalDate(quizDetailsObj.CreatedOn, OffsetValue);
                            quizObj.QuizCoverDetails = new QuizCover();
                            quizObj.QuizCoverDetails.QuizCoverImage = quizDetailsObj.ShowQuizCoverImage ? (quizDetailsObj.QuizCoverImage != null &&
                                                                     quizDetailsObj.QuizCoverImage.Contains("/video/upload/") &&
                                                                     (quizDetailsObj.QuizCoverImage.EndsWith("MP4") || quizDetailsObj.QuizCoverImage.EndsWith("WEBM") || quizDetailsObj.QuizCoverImage.EndsWith("MPG") || quizDetailsObj.QuizCoverImage.EndsWith("OBG") || quizDetailsObj.QuizCoverImage.EndsWith("MOV") || quizDetailsObj.QuizCoverImage.EndsWith("mp4"))
                                                                         ? quizDetailsObj.QuizCoverImage.Split(extensions, System.StringSplitOptions.RemoveEmptyEntries).GetValue(0).ToString() + "jpg"
                                                                         : quizDetailsObj.QuizCoverImage) : string.Empty;

                            quizObj.QuizCoverDetails.QuizCoverTitle = quizDetailsObj.ShowQuizCoverTitle ? quizDetailsObj.QuizCoverTitle : string.Empty;
                            quizObj.QuizCoverDetails.PublicIdForQuizCover = quizDetailsObj.PublicId;


                            var quizquestionlist = UOWObj.QuestionsInQuizRepository.GetSelectedColoumn(a => new { a.Id }, filter: (a => a.QuizId == quizDetailsObj.Id && a.Status == (int)StatusEnum.Active)).ToList();
                            if (quizquestionlist != null && quizquestionlist.Any())
                            {
                                quizObj.NoOfQusetions = quizquestionlist.Count();
                            }

                            quizObj.PublishedCode = quiz.PublishedCode;
                            quizObj.QuizType = (QuizTypeEnum)quiz.QuizType;
                            quizObj.UsageTypes = quiz.UsageTypeInQuiz.Select(v => v.UsageType).Select(c => int.Parse(c.ToString())).ToList();
                            if (!string.IsNullOrEmpty(quiz.AccessibleOfficeId))
                            {
                                quizObj.AccessibleOfficeId = quiz.AccessibleOfficeId;
                            }

                            if (quiz.Company != null && quiz.QuizDetails.FirstOrDefault().CreatedBy == businessUserID)
                                quizObj.IsCreatedByYou = true;

                            if (isFavorite.HasValue && favouriteQuizId != null && favouriteQuizId.Any())
                            {
                                if (favouriteQuizId.Contains(quizObj.Id))
                                {
                                    quizObj.IsFavorited = true;
                                }
                                else
                                { quizObj.IsFavorited = false; }
                            }

                            quizObj.IsCreateStandardAutomation = isCreateStandardAutomation;

                            //quizObj.UsageTypes = new List<int>();

                            //foreach (var item in quiz.UsageTypeInQuiz.ToList())
                            //{
                            //    quizObj.UsageTypes.Add(item.UsageType);
                            //}

                            quizObj.Tag = new List<Tags>();
                            foreach (var tag in quiz.QuizTagDetails.ToList())
                            {
                                quizObj.Tag.Add(new Tags()
                                {
                                    TagId = tag.TagId,
                                    TagName = (automationTagsList != null && automationTagsList.Any(r => r.tagId == tag.TagId)) ? automationTagsList.FirstOrDefault(r => r.tagId == tag.TagId).tagName : string.Empty,
                                    TagCode = (automationTagsList != null && automationTagsList.Any(r => r.tagId == tag.TagId)) ? automationTagsList.FirstOrDefault(r => r.tagId == tag.TagId).tagCode : string.Empty
                                });
                            }

                            lstQuiz.Add(quizObj);
                        }
                    }
                    quizlist.Quiz = lstQuiz;
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }

            return quizlist;
        }


        public QuizList GetAutomationList(int pageNo, int pageSize, int businessUserID, List<string> officeIdList, bool includeSharedWithMe, long offsetValue, string searchTxt, int orderBy, string quizTypeId, CompanyModel companyInfo, bool isDataforGlobalOfficeAdmin, bool isGlobalOfficeAdmin, int? userInfoId, string quizId, bool? isFavorite, bool? isPublished, bool isCreateStandardAutomation, int? quizTagId, int? usageType)
        {
            QuizList quizlist = new QuizList();
            List<LocalQuiz> lstQuiz = new List<LocalQuiz>();
            int totalCount = 0;
            var quizTypeIds = string.IsNullOrEmpty(quizTypeId) ? new List<int>() : quizTypeId.Split(',').Select(Int32.Parse).ToList();
            var QuizIds = string.IsNullOrEmpty(quizId) ? new List<int>() : quizId.Split(',').Select(Int32.Parse).ToList();

            if (!string.IsNullOrEmpty(searchTxt))
                searchTxt = searchTxt.ToUpper();

            var companyId = companyInfo.Id;
            string[] extensions = new string[] { "MP4", "WEBM", "MPG", "OBG", "MOV", "mp4" };
            var automationTagsList = CommonStaticData.GetCachedAutomationTagsList(companyInfo.ClientCode);



            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {


                    IEnumerable<Db.Quiz> quizs = new List<Db.Quiz>();
                    var includeProperties = "QuizDetails, QuizDetails.QuestionsInQuiz, QuizTagDetails, FavoriteQuizByUser";
                    quizs = UOWObj.QuizRepository.GetWithPagination(out totalCount, pageNo, pageSize,
                        filter: (r =>
                                        (QuizIds.Any() ? QuizIds.Any(s => s == r.Id) : true) && (quizTypeIds.Contains(r.QuizType)) && (r.Company.Id == companyId)
                                           && (isPublished.HasValue ? (isPublished.Value ? (r.State == (int)QuizStateEnum.PUBLISHED) : (r.State != (int)QuizStateEnum.PUBLISHED)) : true)
                                           && (!string.IsNullOrEmpty(r.AccessibleOfficeId) ? (((isGlobalOfficeAdmin && isDataforGlobalOfficeAdmin && !officeIdList.Any()) ? true : officeIdList.Contains(r.AccessibleOfficeId))) : (includeSharedWithMe))
                                           // && (IsWhatsAppChatBotOldVersion.HasValue ? IsWhatsAppChatBotOldVersion.Value == true ? r.IsWhatsAppChatBotOldVersion == true : r.IsWhatsAppChatBotOldVersion != true : true)
                                           && (isFavorite.HasValue ? (isFavorite.Value ? r.FavoriteQuizByUser.Any(f => f.QuizId == r.Id && f.UserTokenId == userInfoId) : !r.FavoriteQuizByUser.Any(f => f.QuizId == r.Id && f.UserTokenId == userInfoId)) : true)
                                           && (quizTagId.HasValue ? r.QuizTagDetails.Any(q => q.TagId == quizTagId.Value) : true)
                                           && (string.IsNullOrEmpty(searchTxt) ? true : (string.IsNullOrEmpty(r.QuizDetails.FirstOrDefault(t => t.State == (int)QuizStateEnum.DRAFTED).QuizTitle) ? false : r.QuizDetails.FirstOrDefault(t => t.State == (int)QuizStateEnum.DRAFTED).QuizTitle.ToUpper().IndexOf(searchTxt) > -1))
                                           && ((r.QuizDetails.FirstOrDefault(s => s.State == (int)QuizStateEnum.DRAFTED && s.Status == (int)StatusEnum.Active)) != null)
                                            && (usageType.HasValue ? (r.UsageTypeInQuiz.Any(s => s.UsageType == usageType.Value)) : true)
                                           ),
                         orderBy: r => orderBy == (int)OrderByEnum.Ascending ? (r.OrderBy(k => k.QuizDetails.OrderBy(s => s.Version).FirstOrDefault(s => s.State == (int)QuizStateEnum.DRAFTED).CreatedOn))
                                           : (r.OrderByDescending(k => k.QuizDetails.OrderByDescending(s => s.Version).FirstOrDefault(s => s.State == (int)QuizStateEnum.DRAFTED).CreatedOn)),
                         includeProperties: includeProperties);

                    quizlist.TotalRecords = totalCount;
                    quizlist.CurrentPageIndex = pageNo;



                    foreach (var quiz in quizs)
                    {
                        var quizObj = new LocalQuiz();

                        var quizDetailsObj = quiz.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active);

                        if (quizDetailsObj != null)
                        {
                            quizObj.Id = quiz.Id;
                            quizObj.QuizTitle = quizDetailsObj.QuizTitle;
                            //quizObj.IsWhatsAppChatBotOldVersion = quiz.IsWhatsAppChatBotOldVersion.HasValue ? quiz.IsWhatsAppChatBotOldVersion.Value : false;
                            quizObj.IsPublished = quiz.State == (int)QuizStateEnum.PUBLISHED;
                            quizObj.IsBranchingLogicEnabled = quizDetailsObj.IsBranchingLogicEnabled;
                            quizObj.CreatedByID = quizDetailsObj.CreatedBy;
                            quizObj.UsageTypes = quiz.UsageTypeInQuiz.Select(v => v.UsageType).Select(c => int.Parse(c.ToString())).ToList();
                            if (quizDetailsObj.LastUpdatedOn.HasValue)
                            {
                                quizObj.LastEditDate = Utility.ConvertUTCDateToLocalDate(quizDetailsObj.LastUpdatedOn.Value, offsetValue);
                            }
                            quizObj.CreatedOn = Utility.ConvertUTCDateToLocalDate(quizDetailsObj.CreatedOn, offsetValue);
                            quizObj.QuizCoverDetails = new QuizCover();
                            quizObj.QuizCoverDetails.QuizCoverImage = (quizDetailsObj.QuizCoverImage != null &&
                                                                     quizDetailsObj.QuizCoverImage.Contains("/video/upload/") &&
                                                                     (quizDetailsObj.QuizCoverImage.EndsWith("MP4") || quizDetailsObj.QuizCoverImage.EndsWith("WEBM") || quizDetailsObj.QuizCoverImage.EndsWith("MPG") || quizDetailsObj.QuizCoverImage.EndsWith("OBG") || quizDetailsObj.QuizCoverImage.EndsWith("MOV") || quizDetailsObj.QuizCoverImage.EndsWith("mp4"))
                                                                         ? quizDetailsObj.QuizCoverImage.Split(extensions, System.StringSplitOptions.RemoveEmptyEntries).GetValue(0).ToString() + "jpg"
                                                                         : quizDetailsObj.QuizCoverImage);
                            quizObj.QuizCoverDetails.QuizCoverTitle = quizDetailsObj.QuizCoverTitle;
                            quizObj.QuizCoverDetails.PublicIdForQuizCover = quizDetailsObj.PublicId;
                            quizObj.NoOfQusetions = quizDetailsObj.QuestionsInQuiz.Count(r => r.Status == (int)StatusEnum.Active);

                            quizObj.PublishedCode = quiz.PublishedCode;
                            quizObj.QuizType = (QuizTypeEnum)quiz.QuizType;
                            quizObj.UsageTypes = quiz.UsageTypeInQuiz.Select(v => v.UsageType).Select(c => int.Parse(c.ToString())).ToList();
                            if (!string.IsNullOrEmpty(quiz.AccessibleOfficeId))
                            {
                                quizObj.AccessibleOfficeId = quiz.AccessibleOfficeId;
                            }

                            if (quiz.QuizDetails.FirstOrDefault().CreatedBy == businessUserID)
                                quizObj.IsCreatedByYou = true;


                            quizObj.IsFavorited = quiz.FavoriteQuizByUser.Any(f => f.QuizId == quiz.Id && f.UserTokenId == userInfoId);

                            quizObj.IsCreateStandardAutomation = isCreateStandardAutomation;

                            quizObj.Tag = new List<Tags>();
                            foreach (var tag in quiz.QuizTagDetails.ToList())
                            {
                                if (automationTagsList != null && automationTagsList.Any(r => r.tagId == tag.TagId))
                                {
                                    var tagDetails = automationTagsList.First(r => r.tagId == tag.TagId);
                                    quizObj.Tag.Add(new Tags()
                                    {
                                        TagId = tag.TagId,
                                        TagName = tagDetails.tagName,
                                        TagCode = tagDetails.tagCode
                                    });
                                }
                            }

                            lstQuiz.Add(quizObj);
                        }
                    }

                    quizlist.Quiz = lstQuiz;

                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }

            return quizlist;
        }

        public InactiveNotificationTemplateTypeResponse GetInActiveNotificationTemplateList(List<string> officeIdList, bool includeSharedWithMe, int pageNo, int pageSize, string searchTxt, int orderBy, string quizTypeId, CompanyModel companyInfo, bool isDataforGlobalOfficeAdmin, bool isGlobalOfficeAdmin, int? userInfoId, string quizId, bool? isFavorite, int? quizTagId, List<int> lstNotificationTemplate, int? UsageTypes)
        {
            InactiveNotificationTemplateTypeResponse lstQuiz = new InactiveNotificationTemplateTypeResponse();
            string[] extensions = new string[] { "MP4", "WEBM", "MPG", "OBG", "MOV", "mp4" };
            try
            {

                var quizTypeIds = string.IsNullOrEmpty(quizTypeId) ? new List<int>() : quizTypeId.Split(',').Select(Int32.Parse).ToList();
                var QuizIds = string.IsNullOrEmpty(quizId) ? new List<int>() : quizId.Split(',').Select(Int32.Parse).ToList();

                if (!string.IsNullOrEmpty(searchTxt))
                    searchTxt = searchTxt.ToUpper();

                var companyId = companyInfo.Id;

                IEnumerable<Db.Quiz> quizList = new List<Db.Quiz>();
                int totalCount = 0;


                System.Linq.Expressions.Expression<Func<Db.Quiz, bool>> filter = (r =>
                                            (QuizIds.Any() ? QuizIds.Any(s => s == r.Id) : true)
                                        && (quizTypeIds.Contains(r.QuizType)) && (r.Company.Id == companyInfo.Id)
                                        && (!r.NotificationTemplatesInQuiz.Any(n => lstNotificationTemplate.Contains(n.NotificationTemplateId) && n.QuizId == r.Id))
                                        && (isFavorite.HasValue ? (isFavorite.Value ? r.FavoriteQuizByUser.Any(f => f.QuizId == r.Id && f.UserTokenId == userInfoId) : !r.FavoriteQuizByUser.Any(f => f.QuizId == r.Id && f.UserTokenId == userInfoId)) : true)
                                        && (quizTagId.HasValue ? r.QuizTagDetails.Any(q => q.TagId == quizTagId.Value) : true)
                                        && (string.IsNullOrEmpty(searchTxt) ? true : (string.IsNullOrEmpty(r.QuizDetails.FirstOrDefault(t => t.State == (int)QuizStateEnum.DRAFTED).QuizTitle) ? false : r.QuizDetails.FirstOrDefault(t => t.State == (int)QuizStateEnum.DRAFTED).QuizTitle.ToUpper().IndexOf(searchTxt) > -1))
                                        && ((r.QuizDetails.FirstOrDefault(s => s.State == (int)QuizStateEnum.DRAFTED && s.Status == (int)StatusEnum.Active)) != null)
                                        && (UsageTypes.HasValue ? (r.UsageTypeInQuiz.Any(s => s.UsageType == UsageTypes.Value)) : true)
                                        && (!string.IsNullOrEmpty(r.AccessibleOfficeId) ? (officeIdList.Contains(r.AccessibleOfficeId)) : includeSharedWithMe)
                                        );

                Func<IQueryable<Db.Quiz>, IOrderedQueryable<Db.Quiz>> orderBycolumn = (r => orderBy == (int)OrderByEnum.Ascending ? (r.OrderBy(k => k.QuizDetails.OrderBy(s => s.Version).FirstOrDefault(s => s.State == (int)QuizStateEnum.DRAFTED).CreatedOn))
                                            : (r.OrderByDescending(k => k.QuizDetails.OrderByDescending(s => s.Version).FirstOrDefault(s => s.State == (int)QuizStateEnum.DRAFTED).CreatedOn)));


                var includeProperties = "QuizDetails, QuizDetails.QuestionsInQuiz, QuizTagDetails, FavoriteQuizByUser, NotificationTemplatesInQuiz";
                var automationTagsList = CommonStaticData.GetCachedAutomationTagsList(companyInfo.ClientCode);

                using (var UOWObj = new AutomationUnitOfWork())
                {

                    quizList = UOWObj.QuizRepository.GetWithPagination(out totalCount, pageNo, pageSize, filter, orderBy: orderBycolumn,
                         includeProperties: includeProperties);

                    lstQuiz.TotalRecords = totalCount;
                    lstQuiz.CurrentPageIndex = pageNo;
                    List<NotificationTemplateQuizDetails> quizInTemplatesList = new List<NotificationTemplateQuizDetails>();
                    foreach (var quiz in quizList)
                    {
                        var quizObj = new NotificationTemplateQuizDetails();

                        var quizDetailsObj = quiz.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active);

                        if (quizDetailsObj != null)
                        {
                            quizObj.Id = quiz.Id;
                            quizObj.QuizTitle = quizDetailsObj.QuizTitle;
                            quizObj.IsPublished = quiz.State == (int)QuizStateEnum.PUBLISHED;
                            quizObj.QuizCoverDetails = new NotificationTemplateQuizDetails.NotificationTemplateQuizCover();
                            quizObj.QuizCoverDetails.QuizCoverImage = (quizDetailsObj.QuizCoverImage != null &&
                                                                     quizDetailsObj.QuizCoverImage.Contains("/video/upload/") &&
                                                                     (quizDetailsObj.QuizCoverImage.EndsWith("MP4") || quizDetailsObj.QuizCoverImage.EndsWith("WEBM") || quizDetailsObj.QuizCoverImage.EndsWith("MPG") || quizDetailsObj.QuizCoverImage.EndsWith("OBG") || quizDetailsObj.QuizCoverImage.EndsWith("MOV") || quizDetailsObj.QuizCoverImage.EndsWith("mp4"))
                                                                         ? quizDetailsObj.QuizCoverImage.Split(extensions, System.StringSplitOptions.RemoveEmptyEntries).GetValue(0).ToString() + "jpg"
                                                                         : quizDetailsObj.QuizCoverImage);
                            quizObj.QuizCoverDetails.QuizCoverTitle = quizDetailsObj.QuizCoverTitle;
                            quizObj.QuizCoverDetails.PublicId = quizDetailsObj.PublicId;
                            quizObj.NoOfQusetions = quizDetailsObj.QuestionsInQuiz.Count(r => r.Status == (int)StatusEnum.Active);
                            quizObj.PublishedCode = quiz.PublishedCode;
                            quizObj.QuizType = (QuizTypeEnum)quiz.QuizType;
                            quizObj.UsageTypes = quiz.UsageTypeInQuiz.Select(v => v.UsageType).Select(c => int.Parse(c.ToString())).ToList();
                            quizObj.IsFavorited = quiz.FavoriteQuizByUser.Any(f => f.QuizId == quiz.Id && f.UserTokenId == userInfoId);

                            quizObj.Tag = new List<Tags>();
                            foreach (var tag in quiz.QuizTagDetails.ToList())
                            {
                                quizObj.Tag.Add(new Tags()
                                {
                                    TagId = tag.TagId,
                                    TagName = (automationTagsList != null && automationTagsList.Any(r => r.tagId == tag.TagId)) ? automationTagsList.FirstOrDefault(r => r.tagId == tag.TagId).tagName : string.Empty,
                                    TagCode = (automationTagsList != null && automationTagsList.Any(r => r.tagId == tag.TagId)) ? automationTagsList.FirstOrDefault(r => r.tagId == tag.TagId).tagCode : string.Empty
                                });
                            }

                            quizInTemplatesList.Add(quizObj);
                        }
                    }
                    lstQuiz.InactiveQuizList = quizInTemplatesList;
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }

            return lstQuiz;
        }

        public ActiveNotificationTemplateTypeResponse GetQuizTemplateList(List<int> lstNotificationTemplate, List<string> officeIdList, bool includeSharedWithMe, int pageNo, int pageSize, string searchTxt, int orderBy, string quizTypeId, CompanyModel companyInfo, bool isDataforGlobalOfficeAdmin, bool isGlobalOfficeAdmin, int? userInfoId, string quizId, bool? isFavorite, bool? isPublished, int? quizTagId)
        {
            ActiveNotificationTemplateTypeResponse lstQuiz = new ActiveNotificationTemplateTypeResponse();
            string[] extensions = new string[] { "MP4", "WEBM", "MPG", "OBG", "MOV", "mp4" };

            try
            {
                var quizTypeIds = string.IsNullOrEmpty(quizTypeId) ? new List<int>() : quizTypeId.Split(',').Select(Int32.Parse).ToList();
                var QuizIds = string.IsNullOrEmpty(quizId) ? new List<int>() : quizId.Split(',').Select(Int32.Parse).ToList();

                if (!string.IsNullOrEmpty(searchTxt))
                    searchTxt = searchTxt.ToUpper();

                var companyId = companyInfo.Id;

                IEnumerable<Db.Quiz> quizList = new List<Db.Quiz>();
                int totalCount = 0;
                System.Linq.Expressions.Expression<Func<Db.Quiz, bool>> filter = (r =>
                                        (QuizIds.Any() ? QuizIds.Any(s => s == r.Id) : true)
                                      && (quizTypeIds.Contains(r.QuizType)) && (r.Company.Id == companyInfo.Id)
                                      && (r.NotificationTemplatesInQuiz.Any(n => n.QuizId == r.Id && lstNotificationTemplate.Contains(n.NotificationTemplateId)))
                                      && (isFavorite.HasValue ? (isFavorite.Value ? r.FavoriteQuizByUser.Any(f => f.QuizId == r.Id && f.UserTokenId == userInfoId) : !r.FavoriteQuizByUser.Any(f => f.QuizId == r.Id && f.UserTokenId == userInfoId)) : true)
                                      && (quizTagId.HasValue ? r.QuizTagDetails.Any(q => q.TagId == quizTagId.Value) : true)
                                      && (string.IsNullOrEmpty(searchTxt) ? true : (string.IsNullOrEmpty(r.QuizDetails.FirstOrDefault(t => t.State == (int)QuizStateEnum.DRAFTED).QuizTitle) ? false : r.QuizDetails.FirstOrDefault(t => t.State == (int)QuizStateEnum.DRAFTED).QuizTitle.ToUpper().IndexOf(searchTxt) > -1))
                                      && ((r.QuizDetails.FirstOrDefault(s => s.State == (int)QuizStateEnum.DRAFTED && s.Status == (int)StatusEnum.Active)) != null)
                                      && (!string.IsNullOrEmpty(r.AccessibleOfficeId) ? (officeIdList.Contains(r.AccessibleOfficeId)) : includeSharedWithMe)
                                      );
                Func<IQueryable<Db.Quiz>, IOrderedQueryable<Db.Quiz>> orderBycolumn = (r => orderBy == (int)OrderByEnum.Ascending ? (r.OrderBy(k => k.QuizDetails.OrderBy(s => s.Version).FirstOrDefault(s => s.State == (int)QuizStateEnum.DRAFTED).CreatedOn))
                                           : (r.OrderByDescending(k => k.QuizDetails.OrderByDescending(s => s.Version).FirstOrDefault(s => s.State == (int)QuizStateEnum.DRAFTED).CreatedOn)));
                var includeProperties = "QuizDetails, QuizDetails.QuestionsInQuiz, QuizTagDetails, FavoriteQuizByUser, NotificationTemplatesInQuiz";
                var automationTagsList = CommonStaticData.GetCachedAutomationTagsList(companyInfo.ClientCode);

                using (var UOWObj = new AutomationUnitOfWork())
                {

                    quizList = UOWObj.QuizRepository.GetWithPagination(out totalCount, pageNo, pageSize, filter, orderBy: orderBycolumn, includeProperties: includeProperties);

                    lstQuiz.TotalRecords = totalCount;
                    lstQuiz.CurrentPageIndex = pageNo;

                    List<NotificationTemplateQuizDetails> quizInTemplatesList = new List<NotificationTemplateQuizDetails>();

                    foreach (var quiz in quizList)
                    {
                        var quizObj = new NotificationTemplateQuizDetails();

                        var quizDetailsObj = quiz.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active);

                        if (quizDetailsObj != null)
                        {
                            quizObj.Id = quiz.Id;
                            quizObj.QuizTitle = quizDetailsObj.QuizTitle;
                            quizObj.IsPublished = quiz.State == (int)QuizStateEnum.PUBLISHED;
                            quizObj.QuizCoverDetails = new NotificationTemplateQuizDetails.NotificationTemplateQuizCover();
                            quizObj.QuizCoverDetails.QuizCoverImage = (quizDetailsObj.QuizCoverImage != null &&
                                                                     quizDetailsObj.QuizCoverImage.Contains("/video/upload/") &&
                                                                     (quizDetailsObj.QuizCoverImage.EndsWith("MP4") || quizDetailsObj.QuizCoverImage.EndsWith("WEBM") || quizDetailsObj.QuizCoverImage.EndsWith("MPG") || quizDetailsObj.QuizCoverImage.EndsWith("OBG") || quizDetailsObj.QuizCoverImage.EndsWith("MOV") || quizDetailsObj.QuizCoverImage.EndsWith("mp4"))
                                                                         ? quizDetailsObj.QuizCoverImage.Split(extensions, System.StringSplitOptions.RemoveEmptyEntries).GetValue(0).ToString() + "jpg"
                                                                         : quizDetailsObj.QuizCoverImage);
                            quizObj.QuizCoverDetails.QuizCoverTitle = quizDetailsObj.QuizCoverTitle;
                            quizObj.QuizCoverDetails.PublicId = quizDetailsObj.PublicId;
                            quizObj.NoOfQusetions = quizDetailsObj.QuestionsInQuiz.Count(r => r.Status == (int)StatusEnum.Active);

                            quizObj.PublishedCode = quiz.PublishedCode;
                            quizObj.QuizType = (QuizTypeEnum)quiz.QuizType;
                            quizObj.UsageTypes = quiz.UsageTypeInQuiz.Select(v => v.UsageType).Select(c => int.Parse(c.ToString())).ToList();

                            quizObj.IsFavorited = quiz.FavoriteQuizByUser.Any(f => f.QuizId == quiz.Id && f.UserTokenId == userInfoId);

                            quizObj.Tag = new List<Tags>();
                            foreach (var tag in quiz.QuizTagDetails.ToList())
                            {
                                quizObj.Tag.Add(new Tags()
                                {
                                    TagId = tag.TagId,
                                    TagName = (automationTagsList != null && automationTagsList.Any(r => r.tagId == tag.TagId)) ? automationTagsList.FirstOrDefault(r => r.tagId == tag.TagId).tagName : string.Empty,
                                    TagCode = (automationTagsList != null && automationTagsList.Any(r => r.tagId == tag.TagId)) ? automationTagsList.FirstOrDefault(r => r.tagId == tag.TagId).tagCode : string.Empty
                                });
                            }

                            quizInTemplatesList.Add(quizObj);
                        }
                    }
                    lstQuiz.QuizInTemplateList = quizInTemplatesList;
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }

            return lstQuiz;
        }

        public QuizList AutomationReportList(QuizListRequest quizListRequest, int businessUserID, CompanyModel companyModel, bool isGlobalOfficeAdmin, int? userInfoId = null, bool isCreateStandardAutomation = false)
        {
            if (quizListRequest == null)
            {
                return null;
            }

            if (quizListRequest.OfficeIdList == null)
            {
                quizListRequest.OfficeIdList = new List<string>();
            }

            if (quizListRequest.UsageType == 0)
            {
                quizListRequest.UsageType = null;
            }

            if (quizListRequest.QuizTagId == 0)
            {
                quizListRequest.QuizTagId = null;
            }


            QuizList quizlist = new QuizList();
            List<LocalQuiz> lstQuiz = new List<LocalQuiz>();
            int totalCount = 0;
            var quizTypeIds = string.IsNullOrEmpty(quizListRequest.QuizTypeId) ? new List<int>() : quizListRequest.QuizTypeId.Split(',').Select(Int32.Parse).ToList();
            var QuizIds = string.IsNullOrEmpty(quizListRequest.QuizId) ? new List<int>() : quizListRequest.QuizId.Split(',').Select(Int32.Parse).ToList();

            if (!string.IsNullOrEmpty(quizListRequest.SearchTxt))
                quizListRequest.SearchTxt = quizListRequest.SearchTxt.ToUpper();

            var companyId = companyModel.Id;
            string[] extensions = new string[] { "MP4", "WEBM", "MPG", "OBG", "MOV", "mp4" };
            var automationTagsList = CommonStaticData.GetCachedAutomationTagsList(companyModel.ClientCode);

            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {

                    IEnumerable<Db.Quiz> quizs = new List<Db.Quiz>();
                    var includeProperties = "QuizDetails, QuizDetails.QuestionsInQuiz, QuizTagDetails, FavoriteQuizByUser";
                    quizs = UOWObj.QuizRepository.GetWithPagination(out totalCount, quizListRequest.PageNo, quizListRequest.PageSize,
                      filter: (r =>
                                      (QuizIds.Any() ? QuizIds.Any(s => s == r.Id) : true) && (quizTypeIds.Contains(r.QuizType)) && (r.Company.Id == companyId)
                                         && (quizListRequest.IsPublished.HasValue ? (quizListRequest.IsPublished.Value ? (r.State == (int)QuizStateEnum.PUBLISHED) : (r.State != (int)QuizStateEnum.PUBLISHED)) : true)
                                         && (!string.IsNullOrEmpty(r.AccessibleOfficeId) ? (((isGlobalOfficeAdmin && quizListRequest.IsDataforGlobalOfficeAdmin && !quizListRequest.OfficeIdList.Any()) ? true : quizListRequest.OfficeIdList.Contains(r.AccessibleOfficeId))) : (quizListRequest.IncludeSharedWithMe))
                                         && (quizListRequest.IsWhatsAppChatBotOldVersion.HasValue ? quizListRequest.IsWhatsAppChatBotOldVersion.Value == true ? r.IsWhatsAppChatBotOldVersion == true : r.IsWhatsAppChatBotOldVersion != true : true)
                                         && (quizListRequest.IsFavorite.HasValue ? (quizListRequest.IsFavorite.Value ? r.FavoriteQuizByUser.Any(f => f.QuizId == r.Id && f.UserTokenId == userInfoId) : !r.FavoriteQuizByUser.Any(f => f.QuizId == r.Id && f.UserTokenId == userInfoId)) : true)
                                         && (quizListRequest.QuizTagId.HasValue ? r.QuizTagDetails.Any(q => q.TagId == quizListRequest.QuizTagId.Value) : true)
                                         && (string.IsNullOrEmpty(quizListRequest.SearchTxt) ? true : (string.IsNullOrEmpty(r.QuizDetails.FirstOrDefault(t => t.State == (int)QuizStateEnum.DRAFTED).QuizTitle) ? false : r.QuizDetails.FirstOrDefault(t => t.State == (int)QuizStateEnum.DRAFTED).QuizTitle.ToUpper().IndexOf(quizListRequest.SearchTxt) > -1))
                                         && ((r.QuizDetails.FirstOrDefault(s => s.State == (int)QuizStateEnum.DRAFTED && s.Status == (int)StatusEnum.Active)) != null)
                                          && (quizListRequest.UsageType.HasValue ? (r.UsageTypeInQuiz.Any(s => s.UsageType == quizListRequest.UsageType.Value)) : true)
                                         ),
                       orderBy: r => quizListRequest.OrderBy == (int)OrderByEnum.Ascending ? (r.OrderBy(k => k.QuizDetails.OrderBy(s => s.Version).FirstOrDefault(s => s.State == (int)QuizStateEnum.DRAFTED).CreatedOn))
                                         : (r.OrderByDescending(k => k.QuizDetails.OrderByDescending(s => s.Version).FirstOrDefault(s => s.State == (int)QuizStateEnum.DRAFTED).CreatedOn)),
                       includeProperties: includeProperties);

                    quizlist.TotalRecords = totalCount;
                    quizlist.CurrentPageIndex = quizListRequest.PageNo;

                    foreach (var quiz in quizs)
                    {
                        var quizObj = new LocalQuiz();

                        var quizDetailsObj = quiz.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active);

                        if (quizDetailsObj != null)
                        {
                            quizObj.Id = quiz.Id;
                            quizObj.QuizTitle = quizDetailsObj.QuizTitle;
                            quizObj.IsWhatsAppChatBotOldVersion = quiz.IsWhatsAppChatBotOldVersion.HasValue ? quiz.IsWhatsAppChatBotOldVersion.Value : false;
                            quizObj.IsPublished = quiz.State == (int)QuizStateEnum.PUBLISHED;
                            quizObj.IsBranchingLogicEnabled = quizDetailsObj.IsBranchingLogicEnabled;
                            quizObj.CreatedByID = quizDetailsObj.CreatedBy;
                            quizObj.UsageTypes = quiz.UsageTypeInQuiz.Select(v => v.UsageType).Select(c => int.Parse(c.ToString())).ToList();
                            if (quizDetailsObj.LastUpdatedOn.HasValue)
                            {
                                quizObj.LastEditDate = Utility.ConvertUTCDateToLocalDate(quizDetailsObj.LastUpdatedOn.Value, quizListRequest.OffsetValue);
                            }
                            quizObj.CreatedOn = Utility.ConvertUTCDateToLocalDate(quizDetailsObj.CreatedOn, quizListRequest.OffsetValue);
                            quizObj.QuizCoverDetails = new QuizCover();
                            quizObj.QuizCoverDetails.QuizCoverImage = (quizDetailsObj.QuizCoverImage != null &&
                                                                     quizDetailsObj.QuizCoverImage.Contains("/video/upload/") &&
                                                                     (quizDetailsObj.QuizCoverImage.EndsWith("MP4") || quizDetailsObj.QuizCoverImage.EndsWith("WEBM") || quizDetailsObj.QuizCoverImage.EndsWith("MPG") || quizDetailsObj.QuizCoverImage.EndsWith("OBG") || quizDetailsObj.QuizCoverImage.EndsWith("MOV") || quizDetailsObj.QuizCoverImage.EndsWith("mp4"))
                                                                         ? quizDetailsObj.QuizCoverImage.Split(extensions, System.StringSplitOptions.RemoveEmptyEntries).GetValue(0).ToString() + "jpg"
                                                                         : quizDetailsObj.QuizCoverImage);
                            quizObj.QuizCoverDetails.QuizCoverTitle = quizDetailsObj.QuizCoverTitle;
                            quizObj.QuizCoverDetails.PublicIdForQuizCover = quizDetailsObj.PublicId;
                            quizObj.NoOfQusetions = quizDetailsObj.QuestionsInQuiz.Count(r => r.Status == (int)StatusEnum.Active);

                            quizObj.PublishedCode = quiz.PublishedCode;
                            quizObj.QuizType = (QuizTypeEnum)quiz.QuizType;
                            quizObj.UsageTypes = quiz.UsageTypeInQuiz.Select(v => v.UsageType).Select(c => int.Parse(c.ToString())).ToList();
                            if (!string.IsNullOrEmpty(quiz.AccessibleOfficeId))
                            {
                                quizObj.AccessibleOfficeId = quiz.AccessibleOfficeId;
                            }

                            if (quiz.QuizDetails.FirstOrDefault().CreatedBy == businessUserID)
                                quizObj.IsCreatedByYou = true;


                            quizObj.IsFavorited = quiz.FavoriteQuizByUser.Any(f => f.QuizId == quiz.Id && f.UserTokenId == userInfoId);

                            quizObj.IsCreateStandardAutomation = isCreateStandardAutomation;

                            quizObj.Tag = new List<Tags>();
                            foreach (var tag in quiz.QuizTagDetails.ToList())
                            {
                                if (automationTagsList != null && automationTagsList.Any(r => r.tagId == tag.TagId))
                                {
                                    var tagDetails = automationTagsList.First(r => r.tagId == tag.TagId);
                                    quizObj.Tag.Add(new Tags()
                                    {
                                        TagId = tag.TagId,
                                        TagName = tagDetails.tagName,
                                        TagCode = tagDetails.tagCode
                                    });
                                }
                            }
                            lstQuiz.Add(quizObj);
                        }
                    }
                    quizlist.Quiz = lstQuiz;
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
            return quizlist;
        }

        public QuizList ReportList(AutomationListRequest objV2, int businessUserID, CompanyModel companyInfo, bool isGlobalOfficeAdmin, int? userInfoId = null, bool isCreateStandardAutomation = false)
        {

            List<string> officeIdList = !String.IsNullOrEmpty(objV2.OfficeIdList) ? objV2.OfficeIdList.Split(',').ToList() : new List<string>();
            bool includeSharedWithMe = objV2.IncludeSharedWithMe, isDataforGlobalOfficeAdmin = objV2.IsDataforGlobalOfficeAdmin;
            long OffsetValue = objV2.OffsetValue;
            int? pageNo = objV2.PageNo, pageSize = objV2.PageSize, orderBy = objV2.OrderBy, usageType = objV2.UsageType, quizTagId = objV2.QuizTagId;
            string searchTxt = objV2.SearchTxt, quizTypeId = objV2.QuizTypeId, quizId = objV2.QuizId, MustIncludeQuizId = objV2.MustIncludeQuizId;
            bool? isFavorite = objV2.IsFavorite, IsWhatsAppChatBotOldVersion = objV2.IsWhatsAppChatBotOldVersion;



            QuizList quizlist = new QuizList();
            List<LocalQuiz> lstQuiz = new List<LocalQuiz>();
            var quizTypeIds = string.IsNullOrEmpty(quizTypeId) ? new List<int>() : quizTypeId.Split(',').Select(Int32.Parse).ToList();
            var QuizIds = string.IsNullOrEmpty(quizId) ? new List<int>() : quizId.Split(',').Select(Int32.Parse).ToList();
            var mustIncludeQuizId = string.IsNullOrEmpty(MustIncludeQuizId) ? new List<int>() : MustIncludeQuizId.Split(',').Select(Int32.Parse).ToList();

            int totalCount = 0;
            var quizs = new List<Db.Quiz>();
            var quizList = new List<Db.Quiz>();
            if (!string.IsNullOrEmpty(searchTxt))
                searchTxt = searchTxt.ToUpper();

            var CompanyId = companyInfo.Id;
            List<int> favouriteQuizId = new List<int>();
            List<int> searchQuizId = new List<int>();

            if (!pageNo.HasValue || !pageSize.HasValue)
            {
                if (!pageNo.HasValue)
                {
                    pageNo = 1;
                }
                if (!pageSize.HasValue)
                {
                    pageSize = 10;
                }
            }
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    if (mustIncludeQuizId.Any())
                    {
                        var mustIncludeQuizIdList = UOWObj.QuizRepository.Get(filter: r => (r.Company.Id == CompanyId) && (mustIncludeQuizId.Any(c => c == r.Id)),
                            orderBy: null,
                            includeProperties: "QuizDetails, QuizDetails.QuestionsInQuiz, QuizTagDetails, FavoriteQuizByUser, UsageTypeInQuiz").ToList();
                        if (mustIncludeQuizIdList != null && mustIncludeQuizIdList.Any())
                            quizList.AddRange(mustIncludeQuizIdList);
                        quizlist.TotalRecords = mustIncludeQuizIdList.Count();
                    }
                    pageSize = ((pageSize - quizlist.TotalRecords) > 0) ? pageSize - quizlist.TotalRecords : 0;
                    if (userInfoId.HasValue && isFavorite.HasValue)
                    {
                        var favouritequiz = UOWObj.FavoriteQuizByUserRepository.GetSelectedColoumn(a => new { a.QuizId }, filter: (a => a.UserTokenId == userInfoId)).ToList();
                        if (favouritequiz != null && favouritequiz.Any())
                        {
                            favouriteQuizId = favouritequiz.Select(r => r.QuizId).ToList();
                        }
                    }
                    if (!string.IsNullOrEmpty(searchTxt))
                    {
                        var searchTxtQuiz = UOWObj.QuizRepository.GetSelectedColoumn(a => new { a.Id }, filter: (r => (r.Company.Id == CompanyId) && (string.IsNullOrEmpty(r.QuizDetails.FirstOrDefault(t => t.State == (int)QuizStateEnum.DRAFTED).QuizTitle) ? false : r.QuizDetails.FirstOrDefault(t => t.State == (int)QuizStateEnum.DRAFTED).QuizTitle.ToUpper().IndexOf(searchTxt) > -1))).ToList();
                        if (searchTxtQuiz != null && searchTxtQuiz.Any())
                        {
                            searchQuizId = searchTxtQuiz.Select(r => r.Id).ToList();
                        }
                    }
                    //if (!string.IsNullOrEmpty(SearchTxt))
                    //{
                    //    var searchTxtQuiz = UOWObj.QuizDetailsRepository.GetSelectedColoumn(a => new { a.ParentQuizId }, filter: (r => r.State == (int)QuizStateEnum.DRAFTED && r.QuizTitle.IndexOf(SearchTxt) > -1)).ToList();
                    //    if (searchTxtQuiz != null && searchTxtQuiz.Any())
                    //    {
                    //        searchQuizId = searchTxtQuiz.Select(r => r.ParentQuizId).ToList();
                    //    }
                    //}

                    System.Linq.Expressions.Expression<Func<Db.Quiz, bool>> filter = (r =>
                                                (QuizIds.Any() ? QuizIds.Any(s => s == r.Id) : true)
                                               && (favouriteQuizId.Any() ? favouriteQuizId.Any(s => s == r.Id) : true)
                                               && (searchQuizId.Any() ? searchQuizId.Any(s => s == r.Id) : true)
                                               && (quizTypeIds.Contains(r.QuizType))
                                               && (usageType.HasValue ? r.UsageTypeInQuiz.Any(u => u.UsageType == usageType.Value) : true)
                                               && (r.Company.Id == CompanyId)
                                               && (IsWhatsAppChatBotOldVersion.HasValue ? IsWhatsAppChatBotOldVersion.Value == true ? r.IsWhatsAppChatBotOldVersion == true : r.IsWhatsAppChatBotOldVersion != true : true)
                                               && (!mustIncludeQuizId.Any(c => c == r.Id))
                                               && (r.QuizDetails.Any(s => s.State == (int)QuizStateEnum.PUBLISHED && s.Status == (int)StatusEnum.Active))
                                               && (quizTagId.HasValue ? r.QuizTagDetails.Any(q => q.TagId == quizTagId.Value) : true)
                                               && (r.UserPermissionsInQuiz.Any() ? r.UserPermissionsInQuiz.Any(p => (p.UserTypeId == (int)UserTypeEnum.Lead)) : false)
                                               && (r.QuizDetails.FirstOrDefault(s => s.State == (int)QuizStateEnum.DRAFTED && s.Status == (int)StatusEnum.Active) != null)
                                               && (!string.IsNullOrEmpty(r.AccessibleOfficeId) ? ((isGlobalOfficeAdmin && isDataforGlobalOfficeAdmin && !officeIdList.Any()) ? true : officeIdList.Contains(r.AccessibleOfficeId))
                                                                          : (includeSharedWithMe)));


                    quizs = UOWObj.QuizRepository.GetWithPagination(out totalCount, pageNo.Value, pageSize.Value, filter: filter,
                                             orderBy: r => orderBy == (int)OrderByEnum.Ascending ? (r.OrderBy(k => k.QuizDetails.OrderBy(s => s.Version).FirstOrDefault(s => s.State == (int)QuizStateEnum.DRAFTED).CreatedOn))
                                                                                                 : (r.OrderByDescending(k => k.QuizDetails.OrderByDescending(s => s.Version).FirstOrDefault(s => s.State == (int)QuizStateEnum.DRAFTED).CreatedOn))
                                                , includeProperties: "QuizDetails, QuizTagDetails,  UsageTypeInQuiz").ToList();

                    if (quizs != null && quizs.Any())
                        quizList.AddRange(quizs);

                    quizlist.CurrentPageIndex = pageNo.Value;
                    quizlist.TotalRecords += totalCount;
                    pageNo = pageNo.HasValue ? pageNo.Value : 1;
                    pageSize = pageSize.HasValue ? pageSize.Value : quizlist.TotalRecords;

                    // var objList = quizList.Skip(PageSize.Value * (PageNo.Value - 1)).Take(PageSize.Value);

                    string[] extensions = new string[] { "MP4", "WEBM", "MPG", "OBG", "MOV", "mp4" };

                    var automationTagsList = CommonStaticData.GetCachedAutomationTagsList(companyInfo.ClientCode);

                    foreach (var quiz in quizList)
                    {
                        var quizObj = new LocalQuiz();

                        var quizDetailsObj = quiz.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active);

                        if (quizDetailsObj != null)
                        {
                            quizObj.Id = quiz.Id;
                            quizObj.QuizTitle = quizDetailsObj.QuizTitle;
                            quizObj.IsWhatsAppChatBotOldVersion = quiz.IsWhatsAppChatBotOldVersion.HasValue ? quiz.IsWhatsAppChatBotOldVersion.Value : false;
                            quizObj.IsPublished = quiz.State == (int)QuizStateEnum.PUBLISHED;
                            quizObj.CreatedByID = quizDetailsObj.CreatedBy;


                            if (quizDetailsObj.LastUpdatedOn.HasValue)
                            {
                                quizObj.LastEditDate = Utility.ConvertUTCDateToLocalDate(quizDetailsObj.LastUpdatedOn.Value, OffsetValue);
                            }

                            quizObj.CreatedOn = Utility.ConvertUTCDateToLocalDate(quizDetailsObj.CreatedOn, OffsetValue);
                            quizObj.QuizCoverDetails = new QuizCover();
                            quizObj.QuizCoverDetails.QuizCoverImage = quizDetailsObj.ShowQuizCoverImage ? (quizDetailsObj.QuizCoverImage != null &&
                                                                     quizDetailsObj.QuizCoverImage.Contains("/video/upload/") &&
                                                                     (quizDetailsObj.QuizCoverImage.EndsWith("MP4") || quizDetailsObj.QuizCoverImage.EndsWith("WEBM") || quizDetailsObj.QuizCoverImage.EndsWith("MPG") || quizDetailsObj.QuizCoverImage.EndsWith("OBG") || quizDetailsObj.QuizCoverImage.EndsWith("MOV") || quizDetailsObj.QuizCoverImage.EndsWith("mp4"))
                                                                         ? quizDetailsObj.QuizCoverImage.Split(extensions, System.StringSplitOptions.RemoveEmptyEntries).GetValue(0).ToString() + "jpg"
                                                                         : quizDetailsObj.QuizCoverImage) : string.Empty;

                            quizObj.QuizCoverDetails.QuizCoverTitle = quizDetailsObj.ShowQuizCoverTitle ? quizDetailsObj.QuizCoverTitle : string.Empty;
                            quizObj.QuizCoverDetails.PublicIdForQuizCover = quizDetailsObj.PublicId;


                            var quizquestionlist = UOWObj.QuestionsInQuizRepository.GetSelectedColoumn(a => new { a.Id }, filter: (a => a.QuizId == quizDetailsObj.Id && a.Status == (int)StatusEnum.Active)).ToList();
                            if (quizquestionlist != null && quizquestionlist.Any())
                            {
                                quizObj.NoOfQusetions = quizquestionlist.Count();
                            }

                            quizObj.PublishedCode = quiz.PublishedCode;
                            quizObj.QuizType = (QuizTypeEnum)quiz.QuizType;
                            quizObj.UsageTypes = quiz.UsageTypeInQuiz.Select(v => v.UsageType).Select(c => int.Parse(c.ToString())).ToList();
                            if (!string.IsNullOrEmpty(quiz.AccessibleOfficeId))
                            {
                                quizObj.AccessibleOfficeId = quiz.AccessibleOfficeId;
                            }

                            if (quiz.Company != null && quiz.QuizDetails.FirstOrDefault().CreatedBy == businessUserID)
                                quizObj.IsCreatedByYou = true;

                            if (isFavorite.HasValue && favouriteQuizId != null && favouriteQuizId.Any())
                            {
                                if (favouriteQuizId.Contains(quizObj.Id))
                                {
                                    quizObj.IsFavorited = true;
                                }
                                else
                                { quizObj.IsFavorited = false; }
                            }

                            quizObj.IsCreateStandardAutomation = isCreateStandardAutomation;

                            //quizObj.UsageTypes = new List<int>();

                            //foreach (var item in quiz.UsageTypeInQuiz.ToList())
                            //{
                            //    quizObj.UsageTypes.Add(item.UsageType);
                            //}

                            quizObj.Tag = new List<Tags>();
                            foreach (var tag in quiz.QuizTagDetails.ToList())
                            {
                                quizObj.Tag.Add(new Tags()
                                {
                                    TagId = tag.TagId,
                                    TagName = (automationTagsList != null && automationTagsList.Any(r => r.tagId == tag.TagId)) ? automationTagsList.FirstOrDefault(r => r.tagId == tag.TagId).tagName : string.Empty,
                                    TagCode = (automationTagsList != null && automationTagsList.Any(r => r.tagId == tag.TagId)) ? automationTagsList.FirstOrDefault(r => r.tagId == tag.TagId).tagCode : string.Empty
                                });
                            }

                            lstQuiz.Add(quizObj);
                        }
                    }
                    quizlist.Quiz = lstQuiz;
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }

            return quizlist;
        }
    }
}
