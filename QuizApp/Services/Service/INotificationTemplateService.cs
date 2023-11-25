using QuizApp.Helpers;
using QuizApp.Request;
using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static QuizApp.Request.TemplateTypeRequest;

namespace QuizApp.Services.Service
{
    public interface INotificationTemplateService
    {
        ResultEnum Status { get; set; }
        string ErrorMessage { get; set; }

        NotificationTemplateModel AddQuizInTemplate(NotificationTemplateModel notificationTemplate, int BusinessUserId, int CompanyId);
        void UpdateQuizInTemplate(NotificationTemplateModel notificationTemplate, int BusinessUserId, int CompanyId);
        void SaveTemplateBody(NotificationTemplateModel notificationTemplate, int BusinessUserId);
        NotificationTemplateModel GetTemplateBody(int notificationTemplateId);
        NotificationTemplateTypeV1 GetTemplateTypeDetails(TemplateTypeRequestModel rquestModel, CompanyModel CompanyInfo);
        void DeleteTemplate(int id);
        void SetQuizInactive(NotificationTemplateModel notificationTemplate);
        NotificationTemplateModel GetDefaultTemplateByType(int notificationTemplateType);
        List<NotificationTemplateModel> GetTemplateBodyWithValues(int QuizId, string SourceName, CompanyModel CompanyInfo);
        AutomationDetails GetSearchAndSuggestionByNotificationTemplate(int QuizStatus, int NotificationType, string OfficeId, bool IncludeSharedWithMe, long OffsetValue, CompanyModel CompanyInfo, int? NotificationTemplateId, string SearchTxt, bool? IsPublished, int? UsageType);
        Model.NotificationTemplateModel GetTemplateDetailsWithValues(int NotificationTemplateId, CompanyModel CompanyInfo, int? QuizId);
        InactiveNotificationTemplateTypeResponse InActiveQuizList(NotificationTemplateQuizListRequestModel requestModel, CompanyModel companyInfo, int? userInfoId);
        ActiveNotificationTemplateTypeResponse QuizTemplateList(NotificationTemplateInActiveRequestModel requestModel, CompanyModel companyInfo, int? userInfoId);
        void UnLinkAutomation(InactiveNotificationTemplate notificationTemplate);
        void LinkAutomation(InactiveNotificationTemplate notificationTemplate, CompanyModel CompanyInfo);
    }
}
