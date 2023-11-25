using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Common.Extensions
{
   public class GlobalSettings
   {
      private static NameValueCollection _appSettings = ConfigurationManager.AppSettings;

        public static string HubUrl => GetString("HubURL");
        public static string HubUrlBearer => GetString("HubURLBearer");
        public static string accountsApiUrl => GetString("AccountsApiUrl");
        public static string owcSecret => GetString("OWCSecret");
        public static string coreApiUrl => GetString("CoreApiUrl");
        public static string coreBearer => GetString("CoreBearer");
        public static string owcCompanySecret => GetString("OWCCompanySecret");
        public static string mediaApiUrl => GetString("MediaApiUrl");
        public static string mediaApiBearer => GetString("MediaApiBearer");
        public static string owcAddRecruiterCourseUrl => GetString("OWCAddRecruiterCoursesURL");
        public static string communicationApiUrl => GetString("CommunicationApiURL");
        public static string domainUrl => GetString("DomainURL");
        public static string defaultDomain => GetString("DefaultDomain");
        public static string leadUserToken => GetString("LeadUserToken");
        public static string appointmentApiBaseUrl => GetString("AppointmentAPIBaseURL");
        public static string webUrl => GetString("WebURL"); 
        public static string appointmentAPISecret => GetString("AppointmentAPISecret"); 
        public static string owcSSOSecretKeyForDecrypt => GetString("OWCSSOSecretKeyForDecrypt"); 
        public static string elearningWebURL => GetString("ElearningWebURL"); 
        public static string webLoginURL => GetString("WebLoginURL"); 
        public static string owcEmailCommunicationBearer => GetString("OWCEmailCommunicationBearer"); 
        public static string owcSMSCommunicationVNforOLY => GetString("OWCSMSCommunicationVNforOLY"); 
        public static string owcSMSCommunicationVNforUSG => GetString("OWCSMSCommunicationVNforUSG"); 
        public static string owcSMSCommunicationVNforAethone => GetString("OWCSMSCommunicationVNforAethone"); 
        public static string owcSMSCommunicationVN => GetString("OWCSMSCommunicationVN"); 
        public static string owcSMSCommunicationBearer => GetString("OWCSMSCommunicationBearer"); 
        public static string webApiUrl => GetString("WebApiUrl"); 
        public static string apiSecret => GetString("APISecret"); 
        public static string createAcademyCourseKey => GetString("CreateAcademyCourseKey"); 
        public static string createTechnicalRecruiterSkillsKey => GetString("CreateTechnicalRecruiterSkillsKey"); 
        public static string createTemplateKey => GetString("CreateTemplateKey"); 
        public static string globalOfficeAdminPermissionKey => GetString("GlobalOfficeAdminPermissionKey"); 
        public static string automationTechnicalRecruiterCoursePermissionKey => GetString("AutomationTechnicalRecruiterCoursePermissionKey"); 
        public static string appointmentTechnicalRecruiterCoursePermissionKey => GetString("AppointmentTechnicalRecruiterCoursePermissionKey"); 
        public static string eLearningTechnicalRecruiterCoursePermissionKey => GetString("ELearningTechnicalRecruiterCoursePermissionKey"); 
        public static string canvasTechnicalRecruiterCoursePermissionKey => GetString("CanvasTechnicalRecruiterCoursePermissionKey"); 
        public static string vacanciesTechnicalRecruiterCoursePermissionKey => GetString("VacanciesTechnicalRecruiterCoursePermissionKey"); 
        public static string contactsTechnicalRecruiterCoursePermissionKey => GetString("ContactsTechnicalRecruiterCoursePermissionKey"); 
        public static string reviewTechnicalRecruiterCoursePermissionKey => GetString("ReviewTechnicalRecruiterCoursePermissionKey"); 
        public static string reportingTechnicalRecruiterCoursePermissionKey => GetString("ReportingTechnicalRecruiterCoursePermissionKey"); 
        public static string campaignsTechnicalRecruiterCoursePermissionKey => GetString("CampaignsTechnicalRecruiterCoursePermissionKey");
        public static string whatsAppAsSMSPermissionKey => GetString("WhatsAppAsSMSPermissionKey");
        public static string createStandardAutomationPermissionKey => GetString("CreateStandardAutomationPermissionKey"); 
        public static string manageElearningPermissionKey => GetString("ManageElearningPermissionKey"); 
        public static string webChatbotPermissionKey => GetString("WebChatbotPermissionKey"); 
        public static string cloudinaryReaderPermissionKey => GetString("CloudinaryReaderPermissionKey"); 
        public static string cloudinaryAdminPermissionKey => GetString("CloudinaryAdminPermissionKey"); 
        public static string npsAutomationPermissionKey => GetString("NPSAutomationPermissionKey"); 
        public static string contactAutomationReportPermissionKey => GetString("ContactAutomationReportPermissionKey"); 
        public static string contactTagsPermissionKey => GetString("ContactTagsPermissionKey"); 
        public static string module => GetString("Module");
        public static string LeadApiClientCode => GetString("LeadApiClientCode");
        public static string automationMenu => GetString("AutomationMenu");
        public static string module_Elearning => GetString("Module_Elearning"); 
        public static string elearningMenu => GetString("ElearningMenu"); 
        public static string elmahBasicAuthUserName => GetString("ElmahBasicAuth.UserName"); 
        public static string elmahBasicAuthPassword => GetString("ElmahBasicAuth.Password"); 
        public static string elmahmvcroute => GetString("elmah.mvc.route"); 
        public static string notLogActions => GetString("NotLogActions"); 
        public static string notifiyEmails => GetString("NotifiyEmails");
        public static bool triggerSMSFallbackEmailNotification => GetBool("TriggerSMSFallbackEmailNotification");
        public static bool UseQuizAttemptNew => GetBool("UseQuizAttemptNew");
        public static string isJRSalesforceEnabled => GetString("IsJRSalesforceEnabled");
        public static bool EnableWhatsApptempRedirection = GetBool("EnableWhatsApptempRedirection");

       // public static bool EnableContactObjectList = GetBool("EnableContactObjectList");
        private static string GetString(string key, string defaultVal = null)
        {
            return !string.IsNullOrWhiteSpace(_appSettings[key]) ? _appSettings[key] : defaultVal;
        }

        private static int GetInt(string key, int defaultVal = 0)
        {
            return !string.IsNullOrWhiteSpace(_appSettings[key]) ? Convert.ToInt32(_appSettings[key]) : defaultVal;
        }

        private static bool GetBool(string key, bool defaultVal = false)
        {
            return !string.IsNullOrWhiteSpace(_appSettings[key]) ? Convert.ToBoolean(_appSettings[key]) : defaultVal;
        }
    }
}
