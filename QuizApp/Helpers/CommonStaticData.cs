using Core.Common.Caching;
using Core.Common.Extensions;
using Newtonsoft.Json;
using NLog;
using QuizApp.Response;
using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using static QuizApp.Helpers.Models;

namespace QuizApp.Helpers
{
    public  class CommonStaticData
    {
        public static readonly OWCHelper _owchelper = new OWCHelper();
        public static NLog.Logger Logger = LogManager.GetCurrentClassLogger();
        public static OWCCompanyResponse GetCachedCompanyInfo(string companyCode)
        {
            var key = $"Common-{companyCode.ToUpper()}-GetCompanyInfo";
            var result = AppLocalCache.GetOrCache(key, () =>
            {
                return GetCompanyInfo(companyCode);
            });
            return result;

        }
        private static OWCCompanyResponse GetCompanyInfo(string companyCode)
        {
            //var accountUrl = GlobalSettings.accountsApiUrl.ToString();
            var url = GlobalSettings.accountsApiUrl.ToString() + "/api/v1/clients/{clientCode}/settings";
            var secretKey = GlobalSettings.owcCompanySecret.ToString();
            return JsonConvert.DeserializeObject<OWCCompanyResponse>(OWCHelper.GetResponse(url.Replace("{clientCode}", companyCode), secretKey));

            //return JsonConvert.DeserializeObject<OWCCompanyResponse>(GetResponse((ConfigurationManager.AppSettings["OWCCompanyInfoURL"].ToString()).Replace("{clientCode}", companyCode), GlobalSettings.owcCompanySecret.ToString()));
        }

        public static List<OWCLeadTagsResponse> GetCachedTagsByCategory(string tagCategoryId, CompanyModel companyObj)
        {
            //var key = $"Common-{companyObj.ClientCode.ToUpper()}-GetTagsByCategory";
            //if (!string.IsNullOrWhiteSpace(tagCategoryId))
            //{
            //    key += $"-{tagCategoryId}";
            //}
            //var result = AppLocalCache.GetOrCache(key, () =>
            //{
            //    return GetTagsByCategory(tagCategoryId, companyObj);
            //});
            //return result;
            return GetTagsByCategory(tagCategoryId, companyObj);
        }

        private static List<OWCLeadTagsResponse> GetTagsByCategory(string tagCategoryId, CompanyModel companyObj)
        {
            try
            {
                var owcLeadGetTagsUrl = companyObj.LeadDashboardApiUrl + ("/api/v2/integration/GetTags?tagCategoryId={tagCategory}&clientCode=").Replace("{tagCategory}", tagCategoryId) + companyObj.ClientCode;
                return JsonConvert.DeserializeObject<List<OWCLeadTagsResponse>>(OWCHelper.GetResponse(owcLeadGetTagsUrl, companyObj.LeadDashboardApiAuthorizationBearer));
            }
            catch (Exception ex)
            {

                return new List<OWCLeadTagsResponse>();
            }

        }

        public static List<OWCLeadTagCategoriesResponse> GetCachedAllCategory(CompanyModel CompanyInfo)
        {
            //var key = $"Common-{CompanyInfo.ClientCode.ToUpper()}-GetAllCategory";
            //var result = AppLocalCache.GetOrCache(key, 5, () =>
            //{
            //    return GetAllCategory(CompanyInfo);
            //});
            //return result;
            return GetAllCategory(CompanyInfo);

        }
        private static List<OWCLeadTagCategoriesResponse> GetAllCategory(CompanyModel companyObj)
        {
            var owcGetTagsCategoriesUrl = companyObj.LeadDashboardApiUrl + "/api/v2/integration/GetTagsCategories?clientCode=" + companyObj.ClientCode;
            try
            {
                var response = OWCHelper.GetResponse(owcGetTagsCategoriesUrl, companyObj.LeadDashboardApiAuthorizationBearer);
                if (response != null)
                    return JsonConvert.DeserializeObject<List<OWCLeadTagCategoriesResponse>>(response);
                return null;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "API- " + owcGetTagsCategoriesUrl + " Exception- " + ex.Message);
                return null;
            }
        }

        public static List<Offices> GetCachedOfficeInfo(CompanyModel CompanyInfo)
        {
            var key = $"Common-{CompanyInfo.ClientCode.ToUpper()}-GetOfficeInfo";
            var result = AppLocalCache.GetOrCache(key, () =>
            {
                return GetOfficeInfo(CompanyInfo);
            });
            return result;

        }
        private static List<Offices> GetOfficeInfo(CompanyModel CompanyInfo)
        {
            var owcOfficesInfoURL = CompanyInfo.JobRocketApiUrl + ("api/getOfficeList?clientCode={ClientCode}").Replace("{ClientCode}", CompanyInfo.ClientCode);
            try
            {
                var response = OWCHelper.GetResponse(owcOfficesInfoURL, CompanyInfo.JobRocketApiAuthorizationBearer);
                if (response != null)
                    return JsonConvert.DeserializeObject<List<Offices>>(response);
                return null;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "API- " + owcOfficesInfoURL + " Exception- " + ex.Message);
                return null;
            }
        }

        public static List<ClientDomainsResponse> GetCachedClientDomains(CompanyModel CompanyInfo)
        {
            var key = $"Common-{CompanyInfo.ClientCode.ToUpper()}-GetClientDomainsList";
            var result = AppLocalCache.GetOrCache(key, () =>
            {
                return GetClientDomains(CompanyInfo);
            });
            return result;

        }
        private static List<ClientDomainsResponse> GetClientDomains(CompanyModel CompanyInfo)
        {
            //< add key = "OWCClientDomains" value = "/api/v2/integration/clientdomains?clientCode={ClientCode}&amp;module={Module}" />
            var moduleName = ConfigurationManager.AppSettings["Module_Name"].ToString();
            var urlowcClientDomains = CompanyInfo.JobRocketApiUrl + ("/api/v2/integration/clientdomains?clientCode={ClientCode}&module={Module}").Replace("{ClientCode}", CompanyInfo.ClientCode).Replace("{Module}", moduleName);
            try
            {
                var response = OWCHelper.GetResponse(urlowcClientDomains, CompanyInfo.JobRocketApiAuthorizationBearer);
                if (response != null)
                    return JsonConvert.DeserializeObject<List<ClientDomainsResponse>>(response);
                return null;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "API- " + urlowcClientDomains + " Exception- " + ex.Message);
                return null;
            }
        }

        public static ObjectFieldsData GetCachedObjectFieldsList(string clientCode)
        {
            //var key = $"Common-{clientCode.ToUpper()}-GetObjectFieldsList";
            //var result = AppLocalCache.GetOrCache(key, () =>
            //{
            //    return GetObjectFieldsList(clientCode);
            //});
            //return result;

            return GetObjectFieldsList(clientCode);
        }

        public static ObjectFieldsData GetObjectFieldsList(string ClientCode)
        {
            //var urlfieldList = GlobalSettings.HubUrl.ToString() + ("/api/Integration/v1/Webhook/GetObjectFieldsList?clientCode={ClientCode}").Replace("{ClientCode}", ClientCode);
            var urlfieldList = GlobalSettings.HubUrl.ToString() + ("/api/Integration/v1/Webhook/GetObjectFieldsList?clientCode={ClientCode}").Replace("{ClientCode}", ClientCode);
            var hubURLBearer = GlobalSettings.HubUrlBearer.ToString();
            try
            {
                var response = OWCHelper.GetResponse(urlfieldList, hubURLBearer);
                if (response != null)
                    return JsonConvert.DeserializeObject<ObjectFieldsData>(response);
                return null;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "API- " + urlfieldList + " Exception- " + ex.Message);
                return null;
            }
        }

        public static List<AutomationTagsDetails> GetCachedAutomationTagsList(string clientCode)
        {
            //var key = $"Common-{clientCode.ToUpper()}-GetAutomationTagsList";
            //var result = AppLocalCache.GetOrCache(key, 5, () =>
            //{
            //    return GetAutomationTagsList(clientCode);
            //});
            //return result;
            return GetAutomationTagsList(clientCode);
        }
        private static List<AutomationTagsDetails> GetAutomationTagsList(string ClientCode)
        {
            var url = GlobalSettings.accountsApiUrl.ToString() + ("/api/v1/integration/tags?clientCode={clientCode}&moduleName=Automation").Replace("{clientCode}", ClientCode);
            var secretKey = GlobalSettings.owcCompanySecret.ToString();

            try
            {

                var response = OWCHelper.GetResponse(url, secretKey);
                //var response = GetResponse((ConfigurationManager.AppSettings["TagsApiUrl"].ToString()).Replace("{clientCode}", ClientCode), (GlobalSettings.owcCompanySecret.ToString()));
                if (response != null)
                    return JsonConvert.DeserializeObject<List<AutomationTagsDetails>>(response);
                return null;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "API- " + url + " Exception- " + ex.Message);
                return null;
            }
        }


        public static SignatureDetails GetCachedEmailSignature(string clientCode, string SignatureType)
        {
            var key = $"Common-{clientCode.ToUpper()}-GetEmailSignature-{SignatureType}";
            var result = AppLocalCache.GetOrCache(key, () =>
            {
                return GetEmailSignature(clientCode, SignatureType);
            });
            return result;

        }
        //SignatureType = individual or group
        private static SignatureDetails GetEmailSignature(string ClientCode, string SignatureType)
        {
            var signatureApiUrl = GlobalSettings.accountsApiUrl.ToString() + ("/api/v1/integration/email-signatures/{signatureType}?clientCode={clientCode}").Replace("{signatureType}", SignatureType.ToString()).Replace("{clientCode}", ClientCode);
            var owcCompanySecretKey = GlobalSettings.owcCompanySecret.ToString();
            try
            {
                var response = OWCHelper.GetResponse(signatureApiUrl, owcCompanySecretKey);
                if (response != null)
                    return JsonConvert.DeserializeObject<SignatureDetails>(response);
                return null;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "API- " + signatureApiUrl + " Exception- " + ex.Message);
                return null;
            }
        }

        public static void VacancyVariableLink(Dictionary<string, object> ContactObject, string Message, string ClientCode)
        {
            if (string.IsNullOrEmpty(Message))
            {
                return;
            }

            if(ContactObject != null && !string.IsNullOrWhiteSpace(ClientCode))
            {
                var vacancyrefid = ContactObject.GetDictionarykeyValueStringObject("vacancy.atsrefid1");
                if (!string.IsNullOrWhiteSpace(vacancyrefid))
                {
                    return;
                }
                var atsvacancyId = ContactObject.GetDictionarykeyValueStringObject("Lead.SourceId");
                if (!string.IsNullOrWhiteSpace(atsvacancyId) && Message.ContainsCI("vacancy.") && !ContactObject.CheckDictionarykeyExistStringObject("vacancy."))
                {
                    var vacancyDetails = _owchelper.GetCoreVacancyDetails(atsvacancyId, ClientCode);
                    if (!string.IsNullOrWhiteSpace(vacancyDetails))
                    {
                        var vacancyObj = (JsonConvert.DeserializeObject<Dictionary<string, object>>(vacancyDetails.ToString()));
                        foreach (var item in vacancyObj)
                        {
                            if (item.Value != null)
                            {
                                ContactObject.Add("vacancy." + item.Key.ToLower(), item.Value);
                            }
                        }
                    }
                }
            }
           
        }

        public static void VacancyVariableReplace(Dictionary<string, object> ContactObject, string QuizVariables, string ClientCode)
        {
            if (ContactObject != null && !string.IsNullOrWhiteSpace(ClientCode) && !string.IsNullOrWhiteSpace(QuizVariables))
            {
                var vacancyrefid = ContactObject.GetDictionarykeyValueStringObject("vacancy.atsrefid1");
                if (!string.IsNullOrWhiteSpace(vacancyrefid))
                {
                    return;
                }
                var atsvacancyId = ContactObject.GetDictionarykeyValueStringObject("Lead.SourceId");               
                if (QuizVariables.Contains("Vacancy.") && !string.IsNullOrWhiteSpace(atsvacancyId) && !ContactObject.CheckDictionarykeyExistStringObject("vacancy."))
                {

                    var vacancyDetails = _owchelper.GetCoreVacancyDetails(atsvacancyId, ClientCode);
                    if (!string.IsNullOrWhiteSpace(vacancyDetails))
                    {
                        var vacancyObj = (JsonConvert.DeserializeObject<Dictionary<string, object>>(vacancyDetails.ToString()));
                        foreach (var item in vacancyObj)
                        {
                            if (item.Value != null)
                            {
                                ContactObject.Add("vacancy." + item.Key.ToLower(), item.Value);
                            }
                        }
                    }
                }
            }

        }

        public static void UserVariableLink(Dictionary<string, object> ContactObject, string Message , string UserToken, string ClientCode)
        {
            if (string.IsNullOrWhiteSpace(Message))
            {
                return;
            }
            if (ContactObject != null)
            {
                var userId = ContactObject.GetDictionarykeyValueStringObject("user.userId");
                if (!string.IsNullOrWhiteSpace(userId))
                {
                    return;
                }

                if (Message.ContainsCI("user.") || Message.ContainsCI("LoggedInUser."))
                {
                    var userDetails = _owchelper.GetUserInfoByUserToken(UserToken, ClientCode);
                    if (!string.IsNullOrWhiteSpace(userDetails))
                    {
                        var userObj = (JsonConvert.DeserializeObject<Dictionary<string, object>>(userDetails.ToString()));
                        //var obj = (JsonConvert.DeserializeObject<LoggedInUser>(userDetails.ToString()));
                        //var json = JsonConvert.SerializeObject(obj);
                        //var userObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                        foreach (var item in userObj)
                        {
                            if (item.Value != null)
                            {
                                ContactObject.Add("user." + item.Key.ToLower(), item.Value);
                                ContactObject.Add("LoggedInUser." + item.Key.ToLower(), item.Value);
                                //if (item.Key.EqualsCI("mail"))
                                //{
                                //    ContactObject.Add("user.Email", item.Value);
                                //    ContactObject.Add("LoggedInUser.Email", item.Value);
                                //}
                                if (item.Key.EqualsCI("mail")) {
                                    if (!ContactObject.ContainsKey("user.Email")) {
                                        ContactObject.Add("user.Email", item.Value);
                                    }
                                    if (!ContactObject.ContainsKey("LoggedInUser.Email")) {
                                        ContactObject.Add("LoggedInUser.Email", item.Value);
                                    }
                                }
                                if (item.Key.EqualsCI("phonenumber")) {
                                    if (!ContactObject.ContainsKey("user.phone")) {
                                        ContactObject.Add("user.phone", item.Value);
                                    }
                                    if (!ContactObject.ContainsKey("LoggedInUser.phone")) {
                                        ContactObject.Add("LoggedInUser.phone", item.Value);
                                    }
                                }
                            }
                        }
                    }
                }
            }

        }

    }
}