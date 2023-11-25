using Core.Common.Caching;
using Core.Common.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using QuizApp.Request;
using QuizApp.Response;
using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using static QuizApp.Helpers.Models;
using static QuizApp.Response.OWCLeadUserResponse;

namespace QuizApp.Helpers
{
    public class OWCHelper
    {
        public static NLog.Logger Logger = LogManager.GetCurrentClassLogger();

        public static BusinessUser MapOWCResponseToEntity(OwcVerificationResponse responseObj)
        {
            BusinessUser businessUser = new BusinessUser();

            businessUser.BusinessUserId = responseObj.OWCBusinessUserResponse.userId;
            businessUser.OWCToken = responseObj.OWCBusinessUserResponse.token;
            businessUser.CreateAcademyCourse = responseObj.OWCBusinessUserResponse.permissions.Any(s => s.Equals(GlobalSettings.createAcademyCourseKey.ToString(), StringComparison.OrdinalIgnoreCase));
            businessUser.CreateTechnicalRecruiterCourse = responseObj.OWCBusinessUserResponse.permissions.Any(s => s.Equals(GlobalSettings.createTechnicalRecruiterSkillsKey.ToString(), StringComparison.OrdinalIgnoreCase));
            businessUser.CreateTemplate = responseObj.OWCBusinessUserResponse.permissions.Any(s => s.Equals(GlobalSettings.createTemplateKey.ToString(), StringComparison.OrdinalIgnoreCase));
            businessUser.IsGlobalOfficeAdmin = responseObj.OWCBusinessUserResponse.permissions.Any(s => s.Equals(GlobalSettings.globalOfficeAdminPermissionKey.ToString(), StringComparison.OrdinalIgnoreCase));
            businessUser.IsAutomationTechnicalRecruiterCoursePermission = responseObj.OWCBusinessUserResponse.permissions.Any(s => s.Equals(GlobalSettings.automationTechnicalRecruiterCoursePermissionKey.ToString(), StringComparison.OrdinalIgnoreCase));
            businessUser.IsAppointmentTechnicalRecruiterCoursePermission = responseObj.OWCBusinessUserResponse.permissions.Any(s => s.Equals(GlobalSettings.appointmentTechnicalRecruiterCoursePermissionKey.ToString(), StringComparison.OrdinalIgnoreCase));
            businessUser.IsELearningTechnicalRecruiterCoursePermission = responseObj.OWCBusinessUserResponse.permissions.Any(s => s.Equals(GlobalSettings.eLearningTechnicalRecruiterCoursePermissionKey.ToString(), StringComparison.OrdinalIgnoreCase));
            businessUser.IsCanvasTechnicalRecruiterCoursePermission = responseObj.OWCBusinessUserResponse.permissions.Any(s => s.Equals(GlobalSettings.canvasTechnicalRecruiterCoursePermissionKey.ToString(), StringComparison.OrdinalIgnoreCase));
            businessUser.IsVacanciesTechnicalRecruiterCoursePermission = responseObj.OWCBusinessUserResponse.permissions.Any(s => s.Equals(GlobalSettings.vacanciesTechnicalRecruiterCoursePermissionKey.ToString(), StringComparison.OrdinalIgnoreCase));
            businessUser.IsContactsTechnicalRecruiterCoursePermission = responseObj.OWCBusinessUserResponse.permissions.Any(s => s.Equals(GlobalSettings.contactsTechnicalRecruiterCoursePermissionKey.ToString(), StringComparison.OrdinalIgnoreCase));
            businessUser.IsReviewTechnicalRecruiterCoursePermission = responseObj.OWCBusinessUserResponse.permissions.Any(s => s.Equals(GlobalSettings.reviewTechnicalRecruiterCoursePermissionKey.ToString(), StringComparison.OrdinalIgnoreCase));
            businessUser.IsReportingTechnicalRecruiterCoursePermission = responseObj.OWCBusinessUserResponse.permissions.Any(s => s.Equals(GlobalSettings.reportingTechnicalRecruiterCoursePermissionKey.ToString(), StringComparison.OrdinalIgnoreCase));
            businessUser.IsCampaignsTechnicalRecruiterCoursePermission = responseObj.OWCBusinessUserResponse.permissions.Any(s => s.Equals(GlobalSettings.campaignsTechnicalRecruiterCoursePermissionKey.ToString(), StringComparison.OrdinalIgnoreCase));
            businessUser.IsCreateStandardAutomationPermission = responseObj.OWCBusinessUserResponse.permissions.Any(s => s.Equals(GlobalSettings.createStandardAutomationPermissionKey.ToString(), StringComparison.OrdinalIgnoreCase));
            businessUser.IsManageElearningPermission = responseObj.OWCBusinessUserResponse.permissions.Any(s => s.Equals(GlobalSettings.manageElearningPermissionKey.ToString(), StringComparison.OrdinalIgnoreCase));
            businessUser.IsWebChatbotPermission = responseObj.OWCBusinessUserResponse.permissions.Any(s => s.Equals(GlobalSettings.webChatbotPermissionKey.ToString(), StringComparison.OrdinalIgnoreCase));
            businessUser.OfficeList = new List<OfficeModel>();

            foreach (var item in responseObj.OWCBusinessUserResponse.offices)
            {
                businessUser.OfficeList.Add(new OfficeModel
                {
                    Id = item.id,
                    Name = item.name
                });
            }
            businessUser.CompanyInfo = new CompanyModel()
            {
                Id = responseObj.OWCCompanyResponse.id,
                AlternateClientCodes = responseObj.OWCCompanyResponse.alternateClientCodes,
                ClientCode = responseObj.OWCCompanyResponse.clientCode,
                CompanyName = responseObj.OWCCompanyResponse.companyName,
                CompanyWebsiteUrl = responseObj.OWCCompanyResponse.companyWebsiteUrl,
                JobRocketApiAuthorizationBearer = responseObj.OWCCompanyResponse.jobRocketApiAuthorizationBearer,
                JobRocketApiUrl = responseObj.OWCCompanyResponse.jobRocketApiUrl,
                JobRocketClientUrl = responseObj.OWCCompanyResponse.jobRocketClientUrl,
                LeadDashboardApiAuthorizationBearer = responseObj.OWCCompanyResponse.leadDashboardApiAuthorizationBearer,
                LeadDashboardApiUrl = responseObj.OWCCompanyResponse.leadDashboardApiUrl,
                LeadDashboardClientUrl = responseObj.OWCCompanyResponse.leadDashboardClientUrl,
                LogoUrl = responseObj.OWCCompanyResponse.logoUrl,
                PrimaryBrandingColor = responseObj.OWCCompanyResponse.primaryBrandingColor,
                SecondaryBrandingColor = responseObj.OWCCompanyResponse.secondaryBrandingColor,
                TertiaryColor = responseObj.OWCCompanyResponse.tertiaryColor,
                CreateAcademyCourseEnabled = responseObj.OWCCompanyResponse.CreateAcademyCourseEnabled,
                CreateTechnicalRecruiterCourseEnabled = responseObj.OWCCompanyResponse.CreateTechnicalRecruiterSkillsEnabled,
                CreateTemplateEnabled = responseObj.OWCCompanyResponse.CreateTemplateEnabled,
                BadgesEnabled = responseObj.OWCCompanyResponse.BadgesEnabled
            };

            businessUser.CompanyInfo.gtmSettings = new List<GtmSetting>();

            foreach (var item in responseObj.OWCCompanyResponse.gtmSettings)
            {
                businessUser.CompanyInfo.gtmSettings.Add(new GtmSetting
                {
                    Brand = item.Brand,
                    GtmCode = item.GtmCode
                });
            }

            return businessUser;
        }

        public static string GetResponse(string requestTypeURL, string authorization, string requestType = "GET", string requestdata = "") {
            string json = string.Empty;
            string url = requestTypeURL;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add(HttpRequestHeader.Authorization, authorization);

            request.Method = requestType;
            request.ContentType = "application/json";
            request.MediaType = "application/json";
            request.Accept = "application/json";

            if (request.Method == "POST") {

                var data = System.Text.Encoding.UTF8.GetBytes(requestdata);

                request.ContentLength = data.Length;

                using (var stream = request.GetRequestStream()) {
                    stream.Write(data, 0, data.Length);
                }
            }
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls |
                                       SecurityProtocolType.Tls11 |
                                       SecurityProtocolType.Tls12;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream)) {
                json = reader.ReadToEnd();
            }

            return json;
        }

        public static string GetResponseWithApiSecret(string requestTypeURL, string authorization, string requestType = "GET", string requestdata = "")
        {
            string json = string.Empty;
            string url = requestTypeURL;

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.TryAddWithoutValidation("ApiSecret", authorization);

            switch (requestType)
            {
                case "GET":
                    var result = client.GetAsync(url).Result.Content;
                    json = result.ReadAsStringAsync().Result;
                    break;

                case "POST":
                    object obj = string.IsNullOrWhiteSpace(requestdata) ? null : JsonConvert.DeserializeObject(requestdata);
                    var resultPost = client.PostAsJsonAsync(url, obj).Result.Content;
                    json = resultPost.ReadAsStringAsync().Result;
                    break;

                default:
                    break;
            }

            return json;
        }

        public static OwcVerificationResponse ValidateUserToken(string module, string token, string companyCode, bool checkModulePermissions = true)
        {
            var url = GlobalSettings.accountsApiUrl.ToString() + "/api/v1/clients/{clientCode}/settings".Replace("{clientCode}", companyCode);
            var secretKey = GlobalSettings.owcCompanySecret.ToString();

            if (string.IsNullOrEmpty(module))
            {
                module = GlobalSettings.module.ToString();
            }
            var companyObj = JsonConvert.DeserializeObject<OWCCompanyResponse>(GetResponse(url, secretKey));
            var owcUserInfoUrl = companyObj.jobRocketApiUrl + "api/integration/userinfo/" + "/" + token + "/" + companyObj.clientCode;
            if (companyObj != null)
            {
                //var isValidToken = JsonConvert.DeserializeObject<bool>(GetResponse(companyObj.jobRocketApiUrl + ConfigurationManager.AppSettings["OWCValidateUserURL"].ToString() + "/" + token, companyObj.jobRocketApiAuthorizationBearer));

                //if (isValidToken)
                // {
                var userInfoObj = JsonConvert.DeserializeObject<OWCBusinessUserResponse>(GetResponse(owcUserInfoUrl, companyObj.jobRocketApiAuthorizationBearer));
                if (userInfoObj != null)
                {
                    if (checkModulePermissions && !userInfoObj.permissions.Any(s => s.Equals(module, StringComparison.OrdinalIgnoreCase)))
                    {
                        return new OwcVerificationResponse() { Status = ResultEnum.Error };
                    }

                    return new OwcVerificationResponse
                    {
                        OWCBusinessUserResponse = userInfoObj,
                        OWCCompanyResponse = companyObj
                    };
                }
                // }
                return new OwcVerificationResponse() { OWCCompanyResponse = companyObj };
            }
            else
            {
                return new OwcVerificationResponse() { Error = "Invalid Company Code" };
            }
            return null;
        }

        public static OWCBusinessUserResponse GetBusinessUserInfo(string Id)
        {
            var owcUserInfoUrl = "api/integration/userinfo/" + "/" + "b134bd51-9ec2-4082-a346-3b007ce61425";
            var owcSecretKey = GlobalSettings.owcSecret.ToString();
            return JsonConvert.DeserializeObject<OWCBusinessUserResponse>(GetResponse(owcUserInfoUrl, owcSecretKey));
        }

        public static List<OWCBusinessUserResponse> GetUserListByOfficeId(string OfficeId, CompanyModel companyObj)
        {
            var OWCOfficeUsersUrl = companyObj.JobRocketApiUrl + ("api/integration/officeusers/{ OfficeId}?clientCode ={ ClientCode}").Replace("{OfficeId}", OfficeId).Replace("{ClientCode}", companyObj.ClientCode);
            return JsonConvert.DeserializeObject<List<OWCBusinessUserResponse>>(GetResponse(OWCOfficeUsersUrl, companyObj.JobRocketApiAuthorizationBearer));
        }
        public static LeadUserResponse GetLeadUserInfo(string Id, CompanyModel conpanyObj)
        {
            //var urlhub = GlobalSettings.HubUrl.ToString();
            var token = GlobalSettings.HubUrlBearer.ToString();
            var urlGetContactInfo = GlobalSettings.HubUrl.ToString() + ("/api/v1/leads/web-hooks/GetContactInfo?clientCode={ClientCode}&contactId=").Replace("{ClientCode}", conpanyObj.ClientCode) + Id;
            try
            {
                return JsonConvert.DeserializeObject<LeadUserResponse>(GetResponse(urlGetContactInfo, token));
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "API- " + urlGetContactInfo + " Exception- " + ex.Message);
                return null;
            }
        }

        public static QuizLeadInfo GetLeadInfo(string Id, string clientCode)
        {
            //var urlhub = ConfigurationManager.AppSettings["HubURL"].ToString();
            var token = GlobalSettings.HubUrlBearer.ToString();
            var urlGetContactInfo = GlobalSettings.HubUrl.ToString() + ("/api/v1/leads/web-hooks/GetContactInfo?clientCode={ClientCode}&contactId=").Replace("{ClientCode}", clientCode) + Id;
            try
            {
                return JsonConvert.DeserializeObject<QuizLeadInfo>(GetResponse(urlGetContactInfo, token));
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "API- " + urlGetContactInfo + " Exception- " + ex.Message);
                return null;
            }
        }
        public static string SaveLeadUserInfo(Lead leadUserRequest, CompanyModel CompanyInfo) {
            string json = string.Empty;
            var leadInsertionUrl = CompanyInfo.LeadDashboardApiUrl + "/api/v2/Integration/LeadInsert";
            var httpReq = (HttpWebRequest)WebRequest.Create(leadInsertionUrl);
            httpReq.Method = "POST";
            httpReq.ContentType = "application/x-www-form-urlencoded";

            using (var stream = httpReq.GetRequestStream())
            using (var sw = new StreamWriter(stream)) {
                sw.Write("data.json=" + HttpUtility.UrlEncode(JsonConvert.SerializeObject(leadUserRequest)));
            }

            using (var response = httpReq.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream)) {
                json = reader.ReadToEnd();
            }
            return JsonConvert.DeserializeObject<string>(json);
        }
        public static string SaveHubLeadUserInfo(Dictionary<string, object> leadUserRequest, CompanyModel CompanyInfo) {
            string json = string.Empty;
            var leadInsertionUrl = $"{GlobalSettings.HubUrl}/api/Integration/v2/LeadInsert?clientCode={CompanyInfo.ClientCode}&isResponseIdRequired=true";

            var httpReq = (HttpWebRequest)WebRequest.Create(leadInsertionUrl);
            httpReq.Method = "POST";
            var searlizeData = JsonConvert.SerializeObject(leadUserRequest);
            var response = GetResponse(leadInsertionUrl, GlobalSettings.HubUrlBearer, "POST", searlizeData);
            JObject  result= JsonConvert.DeserializeObject<JObject>(response);
            if (result != null && result.CheckJObjectkeyExist("content")) {
               return  result.GetJobjectValue("content");
            }
            return null;
        }

        public object GetWhatsAppHSMTemplates(string clientCode, string templatesType, bool replaceParameters = true, string language = null)
        {
            if (GlobalSettings.EnableWhatsApptempRedirection) {
                if (clientCode.EqualsCI("SFMine") || clientCode.EqualsCI("AABHSNTEJH") || clientCode.EqualsCI("AABHUX6DCI") || clientCode.EqualsCI("SFMADEV")) {
                    clientCode = "AABCGFVDCC";
                }
            }

            var token = GlobalSettings.owcEmailCommunicationBearer.ToString();
            var result = $@"{GlobalSettings.communicationApiUrl}/api/v1/{clientCode}/whatsApp/hsmTemplates?type={templatesType}&replaceParameters={replaceParameters}&language={language}";
            try
            {
                return JsonConvert.DeserializeObject<object>(GetResponse(result, token));
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "API- " + result + " Request Data - " + " Exception- " + ex.Message);
                return null;
            }
        }

        public object WhatsAppTemplatesLanguages(string clientCode)
        {

            if (GlobalSettings.EnableWhatsApptempRedirection) {
                if (clientCode.EqualsCI("SFMine") || clientCode.EqualsCI("AABHSNTEJH") || clientCode.EqualsCI("AABHUX6DCI") || clientCode.EqualsCI("SFMADEV")) {
                    clientCode = "AABCGFVDCC";
                }
            }

            var token = GlobalSettings.owcEmailCommunicationBearer.ToString();
            var result = $@"{GlobalSettings.communicationApiUrl}/api/v1/hsmTemplates/{clientCode}/languages";
            try
            {
                return JsonConvert.DeserializeObject<object>(GetResponse(result, token));
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "API- " + result + " Request Data - " + " Exception- " + ex.Message);
                return null;
            }
        }

        public static bool SaveLeadTags(LeadTags leadTags, CompanyModel CompanyInfo)
        {
            var owcInsertLeadTagsUrl = CompanyInfo.LeadDashboardApiUrl + "/api/v2/integration/InsertLeadTags";
            try
            {
                string json = string.Empty;
                var httpReq = (HttpWebRequest)WebRequest.Create(owcInsertLeadTagsUrl);
                httpReq.Method = "POST";
                httpReq.ContentType = "application/json";
                httpReq.MediaType = "application/json";
                httpReq.Accept = "application/json";

                httpReq.Headers.Add(HttpRequestHeader.Authorization, CompanyInfo.LeadDashboardApiAuthorizationBearer);

                //var data = System.Text.Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(leadTags));
                var data = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(leadTags));

                using (var stream = httpReq.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                using (var response = httpReq.GetResponse())
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    json = reader.ReadToEnd();
                }
                return JsonConvert.DeserializeObject<bool>(json);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "API- " + owcInsertLeadTagsUrl + " Request Data - " + JsonConvert.SerializeObject(leadTags).ToString() + " Exception- " + ex.Message);
                return false;
            }
        }

        public static UpdateQuizStatusResponse UpdateQuizStatus(LeadQuizStatus leadQuizStatus)
        {
            //var urlhub = GlobalSettings.HubUrl.ToString();
            var token = GlobalSettings.HubUrlBearer.ToString();
            //var urlAutomation = GlobalSettings.HubUrl.ToString() + "/api/v1/Automations/web-hooks/UpdateAutomationStatus";
            var urlAutomation = GlobalSettings.HubUrl.ToString() + "/api/v1/Automations/web-hooks/UpdateAutomationStatus";
            try
            {
                var response = GetResponse(urlAutomation, token, "POST", JsonConvert.SerializeObject(leadQuizStatus));
                if (response != null)
                    return JsonConvert.DeserializeObject<UpdateQuizStatusResponse>(response);
                return null;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "API- " + urlAutomation + " Request Data - " + JsonConvert.SerializeObject(leadQuizStatus).ToString() + " Exception- " + ex.Message);
                //Logger.Log(LogLevel.Error, "API- " + (ConfigurationManager.AppSettings["OWCUpdateQuizStatus"].ToString()) + " Request Data - " + JsonConvert.SerializeObject(leadQuizStatus).ToString() + " Exception- " + ex.Message);
                return null;
            }
        }

        public static bool UpdateLeadsAppointments(string model)
        {
            //var urlhub = GlobalSettings.HubUrl.ToString();
            var token = GlobalSettings.HubUrlBearer.ToString();
            //var urlAppointment = GlobalSettings.HubUrl.ToString() + "/api/v1/appointments/web-hooks/UpdateAppointmentStatus";
            var urlAppointment = GlobalSettings.HubUrl.ToString() + "/api/v1/appointments/web-hooks/UpdateAppointmentStatus";
            try
            {
                return JsonConvert.DeserializeObject<bool>(GetResponse(urlAppointment, token, "POST", model));
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "API- " + urlAppointment + " Request Data - " + model + " Exception- " + ex.Message);
                return false;
            }
        }

        public static bool UpdateRecruiterCourseBadgesInfo(string model, QuizApp.Db.Company CompanyInfo)
        {
            try
            {
                return JsonConvert.DeserializeObject<bool>(GetResponse(CompanyInfo.JobRocketApiUrl + (GlobalSettings.owcAddRecruiterCourseUrl.ToString()).Replace("{ClientCode}", CompanyInfo.ClientCode), CompanyInfo.JobRocketApiAuthorizationBearer, "POST", model));
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "API- " + (CompanyInfo.JobRocketApiUrl + (GlobalSettings.owcAddRecruiterCourseUrl.ToString()).Replace("{ClientCode}", CompanyInfo.ClientCode)) + " Request Data - " + model + " Exception- " + ex.Message);
                return false;
            }
        }

        public static bool UpdateRecruiterCourseBadgesInfos(string model, CompanyModel CompanyInfo)
        {
            try
            {
                return JsonConvert.DeserializeObject<bool>(GetResponse(CompanyInfo.JobRocketApiUrl + (GlobalSettings.owcAddRecruiterCourseUrl.ToString()).Replace("{ClientCode}", CompanyInfo.ClientCode), CompanyInfo.JobRocketApiAuthorizationBearer, "POST", model));
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "API- " + (CompanyInfo.JobRocketApiUrl + (GlobalSettings.owcAddRecruiterCourseUrl.ToString()).Replace("{ClientCode}", CompanyInfo.ClientCode)) + " Request Data - " + model + " Exception- " + ex.Message);
                return false;
            }
        }
        public static List<OWCBusinessUserResponse> GetUserListOnUserId(long[] UserIds, CompanyModel CompanyInfo)
        {
            var urlowcAllUsers = CompanyInfo.JobRocketApiUrl + "api/integration/usersInfo/";
            try
            {
                var response = GetResponse(urlowcAllUsers, CompanyInfo.JobRocketApiAuthorizationBearer, "POST", JsonConvert.SerializeObject(UserIds));
                if (response != null)
                    return JsonConvert.DeserializeObject<List<OWCBusinessUserResponse>>(response);

                return null;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "API- " + urlowcAllUsers + " Exception- " + ex.Message);
                return null;
            }
        }

        public static List<OWCUserVariable> GetUserListUsingUserId(long[] UserIds, CompanyModel CompanyInfo) {
            var urlowcAllUsers = CompanyInfo.JobRocketApiUrl + "api/integration/usersInfo/";
            try {
                var response = GetResponse(urlowcAllUsers, CompanyInfo.JobRocketApiAuthorizationBearer, "POST", JsonConvert.SerializeObject(UserIds));
                if (response != null)
                    return JsonConvert.DeserializeObject<List<OWCUserVariable>>(response);

                return null;
            } catch (Exception ex) {
                Logger.Log(LogLevel.Error, "API- " + urlowcAllUsers + " Exception- " + ex.Message);
                return null;
            }
        }

        public static List<OWCUserVariable> GetListOfExternalDetails(List<string> externalUserId, CompanyModel CompanyInfo) {
            if (externalUserId != null && externalUserId.Any()) {
                string response = GlobalSettings.accountsApiUrl.ToString() + $"/api/users/GetByExternalUserIdList?clientCode=" + $"{CompanyInfo.ClientCode}";
                var rsponse = GetResponse(response, GlobalSettings.owcCompanySecret.ToString(), "POST", JsonConvert.SerializeObject(externalUserId));
                if (rsponse != null) {
                    return JsonConvert.DeserializeObject<List<OWCUserVariable>>(rsponse);
                }
            }
            return new List<OWCUserVariable>();

        }


        public static List<OWCUserVariable> GetUserListUsingUserId(List<long> UserIds, CompanyModel CompanyInfo) {
            var urlowcAllUsers = CompanyInfo.JobRocketApiUrl + "api/integration/usersInfo/";
            try {
                var response = GetResponse(urlowcAllUsers, CompanyInfo.JobRocketApiAuthorizationBearer, "POST", JsonConvert.SerializeObject(UserIds));
                if (response != null)
                    return JsonConvert.DeserializeObject<List<OWCUserVariable>>(response);

                return null;
            } catch (Exception ex) {
                Logger.Log(LogLevel.Error, "API- " + urlowcAllUsers + " Exception- " + ex.Message);
                return null;
            }
        }


        public static ConfigurationData GetConfigurationDetails(string ConfigurationId, string ClientCode)
        {
            //var urlhubGetPackageById = GlobalSettings.HubUrl.ToString() + (("/api/v1/Automations/web-hooks/GetPackageById?configurationId={configurationId}&clientCode=").Replace("{configurationId}", ConfigurationId.ToString())) + ClientCode;
            var urlhubGetPackageById = GlobalSettings.HubUrl.ToString() + (("/api/v1/Automations/web-hooks/GetPackageById?configurationId={configurationId}&clientCode=").Replace("{configurationId}", ConfigurationId.ToString())) + ClientCode;
            var hubBearer = GlobalSettings.HubUrlBearer.ToString();
            try
            {
                var response = GetResponse(urlhubGetPackageById, hubBearer);
                if (response != null)
                    return JsonConvert.DeserializeObject<ConfigurationData>(response);
                return null;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "API- " + urlhubGetPackageById + " Exception- " + ex.Message);
                return null;
            }
        }

        ////SignatureType = individual or group
        //public static SignatureDetails GetEmailSignature(string ClientCode, string SignatureType)
        //{
        //    var signatureApiUrl = GlobalSettings.accountsApiUrl.ToString() + ("/api/v1/integration/email-signatures/{signatureType}?clientCode={clientCode}").Replace("{signatureType}", SignatureType.ToString()).Replace("{clientCode}", ClientCode);
        //    var owcCompanySecretKey = GlobalSettings.owcCompanySecret.ToString();
        //    try
        //    {               
        //        var response = GetResponse(signatureApiUrl, owcCompanySecretKey);
        //        if (response != null)
        //            return JsonConvert.DeserializeObject<SignatureDetails>(response);
        //        return null;
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Log(LogLevel.Error, "API- " + signatureApiUrl + " Exception- " + ex.Message);
        //        return null;
        //    }
        //}

        public static List<UserMediaClassification> GetUserMediaClassification(string ClientCode, List<string> UserTokens)
        {
            try
            {
                var response = GetResponse((GlobalSettings.mediaApiUrl.ToString()), (GlobalSettings.mediaApiBearer.ToString()), "POST", JsonConvert.SerializeObject(new { ClientCode, UserTokens }));
                if (response != null)
                    return JsonConvert.DeserializeObject<List<UserMediaClassification>>(response);
                return null;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "API- " + (GlobalSettings.mediaApiUrl.ToString()) + " Exception- " + ex.Message);
                return null;
            }
        }

        public static HubTemplateLinkedAutomation GetTemplateLinkedAutomation(string ClientCode, string ConfigurationId)
        {
            //var linkedAutpmationUrl = GlobalSettings.HubUrl.ToString() + "/api/Integration/v1/GetLinkedAutomationConfigurations";
            var linkedAutpmationUrl = GlobalSettings.HubUrl.ToString() + "/api/Integration/v1/GetLinkedAutomationConfigurations";
            var hubBearer = GlobalSettings.HubUrlBearer.ToString();
            try
            {
                var response = GetResponse(linkedAutpmationUrl, hubBearer, "POST", JsonConvert.SerializeObject(new { ClientCode, ConfigurationId }));
                if (response != null)
                    return JsonConvert.DeserializeObject<HubTemplateLinkedAutomation>(response);
                return null;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "API- " + linkedAutpmationUrl + " Exception- " + ex.Message);
                return null;
            }
        }

        public static object GetWhatsAppHSMTemplate(string clientCode, string type = null, bool replaceParameters = true, string language = null, bool? isConsent = null)
        {
            if (GlobalSettings.EnableWhatsApptempRedirection) {
                if (clientCode.EqualsCI("SFMine") || clientCode.EqualsCI("AABHSNTEJH") || clientCode.EqualsCI("AABHUX6DCI") || clientCode.EqualsCI("SFMADEV")) {
                    clientCode = "AABCGFVDCC";
                }
            }

            var urlhub = GlobalSettings.HubUrl.ToString();
            var hubBearer = GlobalSettings.HubUrlBearer.ToString();
            var whatsAppHSMtemplateUrl = urlhub + $@"/api/v1/Common/GetWhatsAppHSMTemplates?clientCode={clientCode}&type={type}&replaceParameters={replaceParameters}&language={language}";

            try
            {
                var response = GetResponse(whatsAppHSMtemplateUrl, hubBearer);
                if (response != null)
                    return JsonConvert.DeserializeObject<object>(response);
                return null;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "API- " + whatsAppHSMtemplateUrl + " Exception- " + ex.Message);
                return null;
            }
        }
        public string GetCommContactDetails(string contactId, string clientCode)
        {

            var hubBearer = GlobalSettings.HubUrlBearer.ToString();
            var url = GlobalSettings.HubUrl.ToString() + $"/api/v1/leads/web-hooks/GetCommContactDetails?" +
                $"clientCode={clientCode}" + $"&contactId={contactId}";

            try
            {
                var response = GetResponse(url, hubBearer);
                return response;
            }

            catch(Exception ex)
            {
                Logger.Log(LogLevel.Error, "API- " + contactId + " Exception- " + ex.Message);
                return null;
            }                
            
        }
        public string GetCoreVacancyDetails(string atsId, string clientCode)
        {

            var coreBearer = GlobalSettings.coreBearer.ToString();
            var url = GlobalSettings.coreApiUrl.ToString()+$"/api/integration/entity?clientCode="+$"{clientCode}&entityId=1&atsId={atsId}";

            try
            {
                var response = GetResponse(url, coreBearer);
                return response;
            }

            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "API- " + atsId + " Exception- " + ex.Message);
                return null;

            }

        }

        public string GetExternalDetails(string clientCode, string externalUserId)
        {
            if(string.IsNullOrEmpty(externalUserId) || string.IsNullOrWhiteSpace(externalUserId))
            {
                return null;
            }

            var accountBearer = GlobalSettings.owcCompanySecret.ToString();
            var url = GlobalSettings.accountsApiUrl.ToString() + $"/api/users/userByExternalUserId?clientCode=" + $"{clientCode}&externalUserId={externalUserId}";

            try
            {
                var response = GetResponse(url, accountBearer);
                return response;
            }

            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "API- " + externalUserId + " Exception- " + ex.Message);
                return null;
            }

        }

        public string GetUserInfoByUserToken(string userToken, string clientCode)
        {
            if (string.IsNullOrEmpty(userToken) )
            {
                return null;
            }
            var accountBearer = GlobalSettings.owcCompanySecret.ToString();
            var url = GlobalSettings.accountsApiUrl.ToString() + $"/api/integration/userinfo/{userToken}/{clientCode}";
            try
            {
                var response = GetResponse(url, accountBearer);
                return response;
            }

            catch (Exception ex)
            {
                return null;
            }
        }


        public object ClientCountries(string clientCode)
        {
            var bearer = GlobalSettings.owcCompanySecret.ToString();
            var url = GlobalSettings.accountsApiUrl.ToString() + $"/api/metadata/clientCountries?clientCode={clientCode}";
            try
            {
                var response = JsonConvert.DeserializeObject<object>(GetResponse(url, bearer));
                return response;
            }
            catch (Exception ex)
            {               
                return null;
            }
        }

        public static object WhatsAppTemplates(int TemplateId)
        {
            var result = $@"{GlobalSettings.communicationApiUrl}/api/v2/whatsApp/hsmTemplates/get/{TemplateId}";
            var communicationBearer = GlobalSettings.owcEmailCommunicationBearer.ToString();
            try
            {
                return JsonConvert.DeserializeObject<object>(GetResponse(result, communicationBearer));
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "API- " + result + " Request Data - " + " Exception- " + ex.Message);
                return null;
            }
        }

        public static IEnumerable<VariablesObjectFieldDto> GetVariableFields(string clientCode, string objectName) {
            string variableUrl = GlobalSettings.HubUrl.ToString() + $"/api/v1/" + $"{clientCode}" + "/variablefields?objects=" + $"{objectName}";
            try {
                return JsonConvert.DeserializeObject<IEnumerable<VariablesObjectFieldDto>>(GetResponse(variableUrl, GlobalSettings.HubUrlBearer.ToString()));
            } catch (Exception ex) {
                Logger.Log(LogLevel.Error, "API- " + variableUrl + " Request Data - " + " Exception- " + ex.Message);
                return null;
            }

        }


        public static bool DeleteVerifyRequest(string requestId, string objectType, string clientCode) 
        {
            string Url = GlobalSettings.HubUrl.ToString() + $"/api/v1/DeleteVerifyRequest?requestId={requestId}&objectType={objectType}&clientCode={clientCode}";
            try 
            {
                return JsonConvert.DeserializeObject<bool>(GetResponse(Url, GlobalSettings.HubUrlBearer.ToString(), "POST"));

            } catch (Exception ex) {
                Logger.Log(LogLevel.Error, "API- " + Url + " Request Data - " + " Exception- " + ex.Message);
                return false;
            }
        }

        public static string GetFollowUpMessageByCode(string clientCode, string appointmentCode) {
            var url = GlobalSettings.HubUrl.ToString() + $"api/v1/Integration/GetFollowUpMessageByCode?clientCode={clientCode}&appointmentCode={appointmentCode}";
            return JsonConvert.DeserializeObject<string>(GetResponse(url, GlobalSettings.HubUrlBearer.ToString()));
        }
     
    }
    
}