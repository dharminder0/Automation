using QuizApp.Helpers;
using QuizApp.Request;
using QuizApp.Services.Model;
using System.Collections.Generic;

namespace QuizApp.Services.Service
{
    public interface IQuizListService
    {
        string ErrorMessage { get; set; }
        ResultEnum Status { get; set; }

        //v1
        List<LocalQuiz> GetList(int BusinessUserID, List<string> OfficeIdList, bool IncludeSharedWithMe, long OffsetValue, string SearchTxt, string QuizTypeId, CompanyModel CompanyInfo, bool IsDataforGlobalOfficeAdmin, bool IsGlobalOfficeAdmin, int? UserInfoId = null, string QuizId = "", bool? IsFavorite = null, bool? IsPublished = null, bool IsCreateStandardAutomation = false, int? QuizTagId = null);
        //v2
        QuizList GetListWithPagination(int BusinessUserID, List<string> OfficeIdList, bool IncludeSharedWithMe, long OffsetValue, int? PageNo, int? PageSize, string SearchTxt, int? OrderBy, string QuizTypeId, CompanyModel CompanyInfo, bool IsDataforGlobalOfficeAdmin, bool IsGlobalOfficeAdmin, int? UserInfoId, string QuizId, bool? IsFavorite, bool IsCreateStandardAutomation, string MustIncludeQuizId, int? UsageType, int? QuizTagId,
            bool? IsWhatsAppChatBotOldVersion);
        //v3
        QuizList GetAutomationList(int PageNo, int PageSize, int BusinessUserID, List<string> OfficeIdList, bool IncludeSharedWithMe, long OffsetValue, string SearchTxt, int OrderBy, string QuizTypeId, CompanyModel CompanyInfo, bool IsDataforGlobalOfficeAdmin, bool IsGlobalOfficeAdmin, int? UserInfoId = null, string QuizId = "", bool? IsFavorite = null, bool? IsPublished = null, bool IsCreateStandardAutomation = false, int? QuizTagId = null, int? UsageType = null);

        //v3 new 
        QuizList AutomationReportList(QuizListRequest quizListRequest, int BusinessUserID, CompanyModel companyModel, bool IsGlobalOfficeAdmin, int? UserInfoId = null, bool IsCreateStandardAutomation = false);
        InactiveNotificationTemplateTypeResponse GetInActiveNotificationTemplateList(List<string> officeIdList, bool includeSharedWithMe, int pageNo, int pageSize, string searchTxt, int orderBy, string quizTypeId, CompanyModel companyInfo, bool isDataforGlobalOfficeAdmin, bool isGlobalOfficeAdmin, int? userInfoId, string quizId, bool? isFavorite, int? quizTagId, List<int> lstNotificationTemplate, int ? UsageTypes);

        ActiveNotificationTemplateTypeResponse GetQuizTemplateList(List<int> lstNotificationTemplate, List<string> officeIdList, bool includeSharedWithMe, int pageNo, int pageSize, string searchTxt, int orderBy, string quizTypeId, CompanyModel companyInfo, bool isDataforGlobalOfficeAdmin, bool isGlobalOfficeAdmin, int? userInfoId, string quizId, bool? isFavorite, bool? isPublished, int? quizTagId);

        //v2 new
        QuizList ReportList(AutomationListRequest objV2, int businessUserID, CompanyModel companyInfo, bool isGlobalOfficeAdmin, int? userInfoId = null, bool isCreateStandardAutomation = false);
    }
}