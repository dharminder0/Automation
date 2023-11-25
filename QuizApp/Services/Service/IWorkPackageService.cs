using QuizApp.Helpers;
using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizApp.Services.Service
{
    public interface IWorkPackageService
    {
        ResultEnum Status { get; set; }
        string ErrorMessage { get; set; }
        void PushWorkPackage(PushWorkPackage workPackageObj);
        AutomationDetails GetSearchAndSuggestion(List<string> OfficeIdList, bool IncludeSharedWithMe, string SearchTxt, CompanyModel CompanyInfo, bool IsDataforGlobalOfficeAdmin, bool? IsPublished, int? UsageType, bool? IsWhatsAppChatBotOldVersion);
        void AddOrUpdateConfigurationDetails(AddOrUpdateConfiguration configurationObj);
        void RemoveConfiguration(string ConfigurationId);
        void SaveQuizUrlSetting(string Key, string DomainName, string Value, int companyId);
        bool CheckQuizUrlSettingKey(string Key, string DomainName);
        // void SendSmsByWorkPackageId(SendSMS Obj);  , bool? IsWhatsAppChatBotOldVersion
    }
}
