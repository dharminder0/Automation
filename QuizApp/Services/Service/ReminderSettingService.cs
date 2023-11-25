using Core.Common.Extensions;
using Newtonsoft.Json;
using QuizApp.Helpers;
using QuizApp.RepositoryPattern;
using QuizApp.Response;
using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web;
using static QuizApp.Helpers.Models;

namespace QuizApp.Services.Service
{
    public class ReminderSettingService : IReminderSettingService
    {
        private ResultEnum status = ResultEnum.Ok;
        private string errormessage = string.Empty;
        private readonly OWCHelper _owchelper = new OWCHelper();

        public ResultEnum Status { get { return status; } set { status = value; } }
        public string ErrorMessage { get { return errormessage; } set { errormessage = value; } }

        public ReminderSetting GetReminderSettings(string OfficeId, int CompanyId)
        {
            ReminderSetting reminderSetting = null;

            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var reminderSettingObj = UOWObj.RemindersInQuizRepository.Get(r => (string.IsNullOrEmpty(OfficeId) ? string.IsNullOrEmpty(r.OfficeId) : r.OfficeId == OfficeId) && r.CompanyId == CompanyId).FirstOrDefault();

                    if (reminderSettingObj != null)
                    {
                        reminderSetting = new ReminderSetting();

                        reminderSetting.OfficeId = OfficeId;
                        reminderSetting.FirstReminder = reminderSettingObj.FirstReminder;
                        reminderSetting.SecondReminder = reminderSettingObj.SecondReminder;
                        reminderSetting.ThirdReminder = reminderSettingObj.ThirdReminder;
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }

            return reminderSetting;
        }

        public void SaveReminderSettings(ReminderSetting reminderSetting, int BusinessUserId, int CompanyId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var obj = UOWObj.RemindersInQuizRepository.Get(r => (string.IsNullOrEmpty(reminderSetting.OfficeId) ? string.IsNullOrEmpty(r.OfficeId) : r.OfficeId == reminderSetting.OfficeId) && r.CompanyId == CompanyId).FirstOrDefault();

                    if (obj != null)
                    {
                        obj.FirstReminder = reminderSetting.FirstReminder;
                        obj.SecondReminder = reminderSetting.SecondReminder;
                        obj.ThirdReminder = reminderSetting.ThirdReminder;
                        obj.LastUpdatedBy = BusinessUserId;
                        obj.LastUpdatedOn = DateTime.UtcNow;

                        UOWObj.RemindersInQuizRepository.Update(obj);
                    }
                    else
                    {
                        obj = new Db.RemindersInQuiz();

                        obj.CompanyId = CompanyId;
                        obj.OfficeId = reminderSetting.OfficeId;
                        obj.FirstReminder = reminderSetting.FirstReminder;
                        obj.SecondReminder = reminderSetting.SecondReminder;
                        obj.ThirdReminder = reminderSetting.ThirdReminder;
                        obj.CreatedBy = BusinessUserId;
                        obj.CreatedOn = DateTime.UtcNow;

                        UOWObj.RemindersInQuizRepository.Insert(obj);
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

        public List<ReminderQueuesModel> GetReminderQueue()
        {
            TempWorkpackagePush.ContactBasicDetails leadUserInfo = null;
            List<ReminderQueuesModel> ReminderQueueList = new List<ReminderQueuesModel>();
            var exObj = new Dictionary<string, object>();
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var reminderQueue = UOWObj.ReminderQueuesRepository.Get(r => r.Sent == false && !r.WorkPackageInfo.QuizAttempts.Any());
                    var CurrentDate = DateTime.UtcNow;

                    foreach (var item in reminderQueue)
                    {
                        bool addInQueue = false;

                        switch (item.ReminderLevel)
                        {
                            case 1:
                                if (item.RemindersInQuiz.FirstReminder.HasValue)
                                {
                                    var dateTimeDiff = Math.Floor((item.QueuedOn.AddDays(item.RemindersInQuiz.FirstReminder.Value)).Subtract(CurrentDate).TotalMinutes);

                                    if (dateTimeDiff >= 0 && dateTimeDiff <= 5)
                                    {
                                        addInQueue = true;
                                    }
                                }
                                break;
                            case 2:
                                if (item.RemindersInQuiz.SecondReminder.HasValue)
                                {
                                    var dateTimeDiff = Math.Floor((item.QueuedOn.AddDays(item.RemindersInQuiz.SecondReminder.Value)).Subtract(CurrentDate).TotalMinutes);
                                    if (dateTimeDiff >= 0 && dateTimeDiff <= 5)
                                    {
                                        addInQueue = true;
                                    }
                                }
                                break;
                            case 3:
                                if (item.RemindersInQuiz.ThirdReminder.HasValue)
                                {
                                    var dateTimeDiff = Math.Floor((item.QueuedOn.AddDays(item.RemindersInQuiz.ThirdReminder.Value)).Subtract(CurrentDate).TotalMinutes);
                                    if (dateTimeDiff >= 0 && dateTimeDiff <= 5)
                                    {
                                        addInQueue = true;
                                    }
                                }
                                break;
                        }

                        if (addInQueue)
                        {
                            var quiz = item.WorkPackageInfo.Quiz;
                            var quizDetails = quiz.QuizDetails.LastOrDefault(r => r.State == (int)QuizStateEnum.PUBLISHED);

                            CompanyModel companyObj;
                            if (quiz.Company != null)
                            {
                                companyObj = new CompanyModel
                                {
                                    Id = item.WorkPackageInfo.Quiz.Company.Id,
                                    AlternateClientCodes = item.WorkPackageInfo.Quiz.Company.AlternateClientCodes,
                                    ClientCode = item.WorkPackageInfo.Quiz.Company.ClientCode,
                                    CompanyName = item.WorkPackageInfo.Quiz.Company.CompanyName,
                                    CompanyWebsiteUrl = item.WorkPackageInfo.Quiz.Company.CompanyWebsiteUrl,
                                    JobRocketApiAuthorizationBearer = item.WorkPackageInfo.Quiz.Company.JobRocketApiAuthorizationBearer,
                                    JobRocketApiUrl = item.WorkPackageInfo.Quiz.Company.JobRocketApiUrl,
                                    JobRocketClientUrl = item.WorkPackageInfo.Quiz.Company.JobRocketClientUrl,
                                    LeadDashboardApiAuthorizationBearer = item.WorkPackageInfo.Quiz.Company.LeadDashboardApiAuthorizationBearer,
                                    LeadDashboardApiUrl = item.WorkPackageInfo.Quiz.Company.LeadDashboardApiUrl,
                                    LeadDashboardClientUrl = item.WorkPackageInfo.Quiz.Company.LeadDashboardClientUrl,
                                    LogoUrl = item.WorkPackageInfo.Quiz.Company.LogoUrl,
                                    PrimaryBrandingColor = item.WorkPackageInfo.Quiz.Company.PrimaryBrandingColor,
                                    SecondaryBrandingColor = item.WorkPackageInfo.Quiz.Company.SecondaryBrandingColor

                                };
                            }
                            else
                            {
                                companyObj = new CompanyModel();
                            }
                           // var leadUserInfo = OWCHelper.GetLeadUserInfo(item.WorkPackageInfo.LeadUserId.ToString(), companyObj);
                            
                            var commContactInfo = _owchelper.GetCommContactDetails(item.WorkPackageInfo.LeadUserId.ToString(), companyObj.ClientCode);

                            if (commContactInfo != null && commContactInfo.Any())
                            {
                                exObj = (JsonConvert.DeserializeObject<Dictionary<string, object>>(commContactInfo.ToString()));
                                leadUserInfo = (JsonConvert.DeserializeObject<TempWorkpackagePush.ContactBasicDetails>(commContactInfo.ToString()));
                            }


                            var template = UOWObj.NotificationTemplatesInQuizRepository.Get(r => r.NotificationTemplate.Status == (int)StatusEnum.Active && r.QuizId == item.WorkPackageInfo.QuizId && r.NotificationTemplate.NotificationType == (int)NotificationTypeEnum.REMINDER).FirstOrDefault();

                            if (template != null && leadUserInfo != null && !string.IsNullOrEmpty(leadUserInfo.email))
                            {
                                #region url shortning

                                var shortUrl = string.Empty;
                                var shotenUrlCode = string.Empty;

                                var notificationTemplate = template.NotificationTemplate;

                                if (!string.IsNullOrEmpty(notificationTemplate.Body) || !string.IsNullOrEmpty(notificationTemplate.SMSText))
                                {
                                    //var domainList = OWCHelper.GetClientDomains(companyObj);
                                    var domainList = CommonStaticData.GetCachedClientDomains(companyObj);
                                    var domain = (domainList != null && domainList.Any()) ? domainList.FirstOrDefault().Domain : GlobalSettings.defaultDomain.ToString();

                                    if (notificationTemplate.EmailLinkVariable == item.WorkPackageInfo.EmailLinkVariableForInvitation && !string.IsNullOrEmpty(item.WorkPackageInfo.ShotenUrlCode))
                                    {
                                        shortUrl = "https://" + (item.WorkPackageInfo.IsOurEndLogicForInvitation ? domain : GlobalSettings.domainUrl.ToString()) + "/" + item.WorkPackageInfo.ShotenUrlCode;
                                    }

                                    else if ((item.ReminderLevel == 2 || item.ReminderLevel == 3) && notificationTemplate.EmailLinkVariable == item.WorkPackageInfo.EmailLinkVariableForFirstReminder && !string.IsNullOrEmpty(item.WorkPackageInfo.ShotenUrlCodeForFirstReminder))
                                    {
                                        shortUrl = "https://" + (item.WorkPackageInfo.IsOurEndLogicForFirstReminder ? domain : GlobalSettings.domainUrl.ToString()) + "/" + item.WorkPackageInfo.ShotenUrlCodeForFirstReminder;
                                    }

                                    else if (item.ReminderLevel == 3 && notificationTemplate.EmailLinkVariable == item.WorkPackageInfo.EmailLinkVariableForSecondReminder && !string.IsNullOrEmpty(item.WorkPackageInfo.ShotenUrlCodeForSecondReminder))
                                    {
                                        shortUrl = "https://" + (item.WorkPackageInfo.IsOurEndLogicForSecondReminder ? domain : GlobalSettings.domainUrl.ToString()) + "/" + item.WorkPackageInfo.ShotenUrlCodeForSecondReminder;
                                    }
                                    else
                                    {
                                        //var shotenUrlCode = string.Empty;

                                        if (!string.IsNullOrEmpty(notificationTemplate.EmailLinkVariable))
                                            shotenUrlCode = notificationTemplate.EmailLinkVariable + "-" + IdGenerator.GetShortCode();
                                        else
                                            shotenUrlCode = IdGenerator.GetShortCode();

                                        #region insert QuizUrlSetting

                                        var quizUrlSettingObj = new Db.QuizUrlSetting();

                                        quizUrlSettingObj.Key = shotenUrlCode;
                                        quizUrlSettingObj.CompanyId = companyObj.Id;
                                        quizUrlSettingObj.DomainName = domainList.Any() ? domainList.FirstOrDefault().Domain : GlobalSettings.defaultDomain.ToString();
                                        //quizUrlSettingObj.Value = GlobalSettings.webUrl.ToString() + "/quiz?Code=" + quiz.PublishedCode + "%26UserTypeId=2%26UserId=" + item.WorkPackageInfo.LeadUserId + "%26ampWorkPackageInfoId=" + item.WorkPackageInfo.Id;
                                        quizUrlSettingObj.Value = GlobalSettings.webUrl.ToString() + "/quiz?Code=" + quiz.PublishedCode + "&UserTypeId=2&UserId=" + item.WorkPackageInfo.LeadUserId + "&WorkPackageInfoId=" + item.WorkPackageInfo.Id;


                                        UOWObj.QuizUrlSettingRepository.Insert(quizUrlSettingObj);
                                        UOWObj.Save();

                                        shortUrl = "https://" + quizUrlSettingObj.DomainName + "/" + quizUrlSettingObj.Key;

                                        #endregion

                                        #region update ShotenUrlCode in WorkPackageInfo

                                        if (item.ReminderLevel == 1)
                                        {
                                            item.WorkPackageInfo.IsOurEndLogicForFirstReminder = true;
                                            item.WorkPackageInfo.ShotenUrlCodeForFirstReminder = shotenUrlCode;
                                            if (!string.IsNullOrEmpty(notificationTemplate.EmailLinkVariable))
                                                item.WorkPackageInfo.EmailLinkVariableForFirstReminder = notificationTemplate.EmailLinkVariable;
                                        }
                                        else if (item.ReminderLevel == 2)
                                        {
                                            item.WorkPackageInfo.IsOurEndLogicForSecondReminder = true;
                                            item.WorkPackageInfo.ShotenUrlCodeForSecondReminder = shotenUrlCode;
                                            if (!string.IsNullOrEmpty(notificationTemplate.EmailLinkVariable))
                                                item.WorkPackageInfo.EmailLinkVariableForSecondReminder = notificationTemplate.EmailLinkVariable;
                                        }
                                        else if (item.ReminderLevel == 3)
                                        {
                                            item.WorkPackageInfo.IsOurEndLogicForThirdReminder = true;
                                            item.WorkPackageInfo.ShotenUrlCodeForThirdReminder = shotenUrlCode;
                                            if (!string.IsNullOrEmpty(notificationTemplate.EmailLinkVariable))
                                                item.WorkPackageInfo.EmailLinkVariableForThirdReminder = notificationTemplate.EmailLinkVariable;
                                        }

                                        UOWObj.WorkPackageInfoRepository.Update(item.WorkPackageInfo);
                                        UOWObj.Save();

                                        #endregion
                                    }
                                }

                                #endregion


                                string body = string.Empty;
                                List<FileAttachment> attachments = null;
                                Dictionary<string, object> staticobjects = new Dictionary<string, object>();                                
                                var Temp = new TempWorkpackagePush();
                                Temp.leadUserInfo = leadUserInfo;
                                Temp.companyObj = companyObj;
                                Temp.RecruiterList = WorkpackageCommunicationService.GetOwnerList(Temp);
                                staticobjects = StaticVariablesEmail(Temp.RecruiterList, staticobjects);
                                if (!String.IsNullOrEmpty(notificationTemplate.Body))
                                {
                             
                                    if (notificationTemplate.Body.Contains("%signature%"))
                                    {
                                        var signatureDetails = CommonStaticData.GetCachedEmailSignature(companyObj.ClientCode, "group");
                                    
                                        staticobjects.Add("%signature%", signatureDetails != null ? signatureDetails.signatureText ?? string.Empty : string.Empty);
                                    }


                                    staticobjects.Add("%qname%", string.IsNullOrEmpty(quizDetails.QuizTitle) ? string.Empty : quizDetails.QuizTitle);
                                    staticobjects.Add("%leadid%", item.WorkPackageInfo.LeadUserId);
                                    staticobjects.Add("%qtype%", ((QuizTypeEnum)quiz.QuizType).ToString());
                                    staticobjects.Add("%qlink%", "<a href=\"" + shortUrl + "\">" + shortUrl + "</a>");
                                    staticobjects.Add("%sourcename%", string.IsNullOrEmpty(item.WorkPackageInfo.CampaignName) ? string.Empty : item.WorkPackageInfo.CampaignName);
                                   
                                    
                                    
                                    CommonStaticData.VacancyVariableLink(exObj, notificationTemplate.Body, companyObj.ClientCode);

                                    body = DynamicExtensions.DynamicVariablesReplace(notificationTemplate.Body, exObj, staticobjects, notificationTemplate.MsgVariables);
                                    #region "Oldcode"
                                    //body = notificationTemplate.Body.Replace("%fname%", leadUserInfo.firstName)
                                    //                                         .Replace("%lname%", leadUserInfo.lastName)
                                    //                                         .Replace("%phone%", leadUserInfo.telephone)
                                    //                                         .Replace("%email%", leadUserInfo.email)
                                    //                                         .Replace("%leadid%", item.WorkPackageInfo.LeadUserId)
                                    //                                         .Replace("%qname%", quizDetails.QuizTitle)
                                    //                                         .Replace("%qtype%", ((QuizTypeEnum)quiz.QuizType).ToString())
                                    //                                         .Replace("%qlink%", "<a href=\"" + shortUrl + "\">" + shortUrl + "</a>")
                                    //                                         .Replace("%sourcename%", string.IsNullOrEmpty(item.WorkPackageInfo.CampaignName) ? string.Empty : item.WorkPackageInfo.CampaignName);
                                    #endregion


                                    attachments = notificationTemplate.AttachmentsInNotificationTemplate.Any() ? notificationTemplate.AttachmentsInNotificationTemplate.Select(r => new Models.FileAttachment { FileLink = r.Description, FileName = r.Title }).ToList() : null;
                                }

                                
                                string smsBody = string.Empty;

                                if (!string.IsNullOrEmpty(notificationTemplate.SMSText))
                                {
                                    //shortUrl = "https://" + quizUrlSettingObj.DomainName + "/" + quizUrlSettingObj.Key;
                                    staticobjects.AddORUpdateDictionarykey("%qtype%", ((QuizTypeEnum)item.WorkPackageInfo.Quiz.QuizType).ToString());
                                    staticobjects.AddORUpdateDictionarykey("%qlink%", shortUrl);
                                    staticobjects.AddORUpdateDictionarykey("%signature%", string.Empty);
                                    CommonStaticData.VacancyVariableLink(exObj, notificationTemplate.SMSText, companyObj.ClientCode);
                                    smsBody = DynamicExtensions.DynamicVariablesReplace(notificationTemplate.SMSText, exObj, staticobjects, notificationTemplate.MsgVariables);
                                    #region "Oldcode"
                                    //smsBody = notificationTemplate.SMSText.Replace("%fname%", leadUserInfo.firstName)
                                    //                                               .Replace("%lname%", leadUserInfo.lastName)
                                    //                                               .Replace("%phone%", leadUserInfo.telephone)
                                    //                                               .Replace("%email%", leadUserInfo.email)
                                    //                                               .Replace("%leadid%", item.WorkPackageInfo.LeadUserId)
                                    //                                               .Replace("%qname%", quizDetails.QuizTitle)
                                    //                                               .Replace("%qtype%", ((QuizTypeEnum)item.WorkPackageInfo.Quiz.QuizType).ToString())
                                    //                                               .Replace("%qlink%", shortUrl)
                                    //                                               .Replace("%sourcename%", String.IsNullOrEmpty(item.WorkPackageInfo.CampaignName) ? string.Empty : item.WorkPackageInfo.CampaignName)
                                    //                                               .Replace("%signature%", string.Empty);
                                    #endregion
                                }
                                if (!string.IsNullOrEmpty(notificationTemplate.EmailLinkVariable))
                                    shotenUrlCode = notificationTemplate.EmailLinkVariable + "-" + IdGenerator.GetShortCode();
                                else
                                    shotenUrlCode = IdGenerator.GetShortCode();

                                if (string.IsNullOrWhiteSpace(shortUrl)) {
                                    var quizUrlSettingObj = new Db.QuizUrlSetting();
                                    var domainList = CommonStaticData.GetCachedClientDomains(companyObj);
                                    var domain = (domainList != null && domainList.Any()) ? domainList.FirstOrDefault().Domain : GlobalSettings.defaultDomain.ToString();
                                    quizUrlSettingObj.Key = shotenUrlCode;
                                    quizUrlSettingObj.CompanyId = companyObj.Id;
                                    quizUrlSettingObj.DomainName = domainList.Any() ? domainList.FirstOrDefault().Domain : GlobalSettings.defaultDomain.ToString();
                                    //quizUrlSettingObj.Value = GlobalSettings.webUrl.ToString() + "/quiz?Code=" + quiz.PublishedCode + "%26UserTypeId=2%26UserId=" + item.WorkPackageInfo.LeadUserId + "%26ampWorkPackageInfoId=" + item.WorkPackageInfo.Id;
                                    quizUrlSettingObj.Value = GlobalSettings.webUrl.ToString() + "/quiz?Code=" + quiz.PublishedCode + "&UserTypeId=2&UserId=" + item.WorkPackageInfo.LeadUserId + "&WorkPackageInfoId=" + item.WorkPackageInfo.Id;


                                    UOWObj.QuizUrlSettingRepository.Insert(quizUrlSettingObj);
                                    UOWObj.Save();

                                    shortUrl = "https://" + quizUrlSettingObj.DomainName + "/" + quizUrlSettingObj.Key;

                                }
                                

                                string whatsappBody = string.Empty;
                                var whatsapp = new WhatsappBodyDetails();
                                var Urlkey = !string.IsNullOrWhiteSpace(shortUrl) ? shortUrl.Split('/').Last() : null;
                                if (!string.IsNullOrEmpty(notificationTemplate.WhatsApp)) {
                                    
                                    staticobjects.Add("%qLinkKey%", string.IsNullOrWhiteSpace(shotenUrlCode) ? Urlkey : shotenUrlCode);
                                    var whatsappdetail = JsonConvert.DeserializeObject<WhatsAppBody>(notificationTemplate.WhatsApp);

                                    whatsapp.Clientcode = companyObj.ClientCode;
                                    whatsapp.ContactPhone = leadUserInfo.telephone;
                                    whatsapp.FollowUpMessage = whatsappdetail.FollowUpMessage;
                                    whatsapp.ObjectFields = exObj;
                                    whatsapp.HsmTemplateId = whatsappdetail.HsmTemplateId;
                                    whatsapp.LanguageCode = whatsappdetail.HsmTemplateLanguageCode;
                                    whatsapp.MsgVariables = notificationTemplate.MsgVariables;
                                    whatsapp.StaticObjects = staticobjects;
                                    whatsapp.WorkPackageInfoId = item.WorkPackageInfoId;
                                    whatsapp.LeadUserId = leadUserInfo.contactId;
                                    whatsapp.ContactOwnerId = leadUserInfo.ContactOwnerId;
                                    whatsapp.SourceOwnerId = leadUserInfo.SourceOwnerId;
                                    whatsapp.LeadOwnerId = leadUserInfo.LeadOwnerId;
                                };
                                                         
                                ReminderQueueList.Add(new ReminderQueuesModel
                                {
                                    Id = item.Id,
                                    ToEmail = leadUserInfo.email,
                                    Subject = string.IsNullOrEmpty(notificationTemplate.Subject) ? string.Empty : notificationTemplate.Subject.Replace("%qname%", quizDetails.QuizTitle).Replace("%sourcename%", string.IsNullOrEmpty(item.WorkPackageInfo.CampaignName) ? string.Empty :item.WorkPackageInfo.CampaignName),
                                    Body = body,
                                    ToPhone = leadUserInfo.telephone,
                                    SMSText = smsBody,
                                    WhatsappBody = whatsapp,
                                    Sent = false,
                                    Type = item.Type,
                                    Attachments = attachments,
                                    Company = companyObj,
                                    LeadUserInfo = leadUserInfo,
                                    MsgVariables = notificationTemplate.MsgVariables
                                });
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
            return ReminderQueueList;
        }

        public void UpdateReminderQueueStatus(ReminderQueuesModel reminderQueue)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var reminderQueueObj = UOWObj.ReminderQueuesRepository.GetByID(reminderQueue.Id);

                    if (reminderQueueObj != null)
                    {
                        reminderQueueObj.SentOn = reminderQueue.SentOn;
                        reminderQueueObj.Sent = reminderQueue.Sent;

                        UOWObj.ReminderQueuesRepository.Update(reminderQueueObj);
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

        public static Dictionary<string, object> StaticVariablesEmail(List<OWCUserVariable> leadRecruiterObjList, Dictionary<string, object> staticobjects) {


            //staticobjects.Add("&lt;", "<");
            //staticobjects.Add("&gt;", " >");
            //staticobjects.Add("%qname%", String.IsNullOrEmpty(title) ? string.Empty : title);
            //staticobjects.Add("%leadid%", tempWorkpackagePush.LeadUserId);
            //staticobjects.Add("%jrleadid%", tempWorkpackagePush.LeadUserId);
            //staticobjects.Add("%jrconfigurationId%", String.IsNullOrEmpty(configurationId) ? string.Empty : configurationId);
            //staticobjects.Add("%jrquizId%", tempWorkpackagePush.parentquizid.ToString());
            //staticobjects.Add("%qtype%", ((QuizTypeEnum)tempWorkpackagePush.quizType).ToString());
            //staticobjects.Add("%qlink%", "<a href=\"" + tempWorkpackagePush.shortUrl + "\">" + tempWorkpackagePush.shortUrl + "</a>");
            //staticobjects.Add("%sourcename%", String.IsNullOrEmpty(tempWorkpackagePush.WorkpackageCampaignName) ? string.Empty : tempWorkpackagePush.WorkpackageCampaignName);

            foreach (var leadRecruiterObj in leadRecruiterObjList) {
                staticobjects.Add($@"%{leadRecruiterObj.ObjectUserOwnerType}.firstname%", String.IsNullOrEmpty(leadRecruiterObj.firstName) ? string.Empty : leadRecruiterObj.firstName);
                staticobjects.Add($@"%{leadRecruiterObj.ObjectUserOwnerType}.lastName%", String.IsNullOrEmpty(leadRecruiterObj.lastName) ? string.Empty : leadRecruiterObj.lastName);
                staticobjects.Add($@"%{leadRecruiterObj.ObjectUserOwnerType}.phoneNumber%", String.IsNullOrEmpty(leadRecruiterObj.phoneNumber) ? string.Empty : leadRecruiterObj.phoneNumber);
                staticobjects.Add($@"%{leadRecruiterObj.ObjectUserOwnerType}.phone%", String.IsNullOrEmpty(leadRecruiterObj.phoneNumber) ? string.Empty : leadRecruiterObj.phoneNumber);
                staticobjects.Add($@"%{leadRecruiterObj.ObjectUserOwnerType}.email%", String.IsNullOrEmpty(leadRecruiterObj.Mail) ? string.Empty : leadRecruiterObj.Mail);
                if (leadRecruiterObj.ObjectUserOwnerType.EqualsCI("SourceOwner")) {
                    staticobjects.Add("%rfname%", (leadRecruiterObj != null && !String.IsNullOrEmpty(leadRecruiterObj.firstName)) ? leadRecruiterObj.firstName : string.Empty);
                    staticobjects.Add("%rphone%", (leadRecruiterObj != null && !String.IsNullOrEmpty(leadRecruiterObj.phoneNumber)) ? leadRecruiterObj.phoneNumber : string.Empty);
                    staticobjects.Add("%recruiter%", (leadRecruiterObj != null && !String.IsNullOrEmpty(leadRecruiterObj.firstName)) ? leadRecruiterObj.firstName : string.Empty);

                }
            }

            return staticobjects;
        }
    }
}