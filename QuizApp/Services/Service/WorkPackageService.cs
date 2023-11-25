using Core.Common.Caching;
using Core.Common.Extensions;
using Newtonsoft.Json;
using NLog;
using QuizApp.Helpers;
using QuizApp.RepositoryExtensions;
using QuizApp.RepositoryPattern;
using QuizApp.Response;
using QuizApp.Services.Model;
using QuizApp.Services.Validator;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static QuizApp.Response.ConfigurationDataDetails;

namespace QuizApp.Services.Service {
    public class TempWorkpackagePush
    {
        public int ConfigurationDetailId { get; set; }
        public int workPackageInfoId { get; set; }
        public int quizDetailid { get; set; }
        public int UsageType { get; set; }
        public string LeadUserId { get; set; }

        public string WorkpackageCampaignName { get; set; }
        public int parentquizid { get; set; }
        public CompanyModel companyObj { get; set; }
        public int quizType { get; set; }
        public int quizUsageType { get; set; }
        public string shotenUrlCode { get; set; }
        public string shortUrl { get; set; }
        public string qLinkKey { get; set; }
        public string UserToken { get; set; }
        public Dictionary<string, object> ContactObject { get; set; }

        public ContactBasicDetails leadUserInfo { get; set; }

        public Db.QuizDetails QuizDetails { get; set; }

        public Db.ConfigurationDetails ConfigurationDetails { get; set; }
        public List<OWCUserVariable> RecruiterList { get; set; }

        public class ContactBasicDetails
        {
            public string contactId { get; set; }
            public string firstName { get; set; }
            public string lastName { get; set; }
            public string email { get; set; }
            public string telephone { get; set; }
            public string SourceId { get; set; }
            public string SourceName { get; set; }
            public string ContactOwnerId { get; set; }
            public string SourceOwnerId { get; set; }
            public string LeadOwnerId { get; set; }
        }
    }
    public class BasicDetails {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        
    }


    public class WorkPackageService : IWorkPackageService
    {
        private static bool enableExternalActionQueue = bool.Parse(string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["EnableExternalActionQueue"]) ? "true" : ConfigurationManager.AppSettings["EnableExternalActionQueue"]);
        private ResultEnum status = ResultEnum.Ok;
        private string errormessage = string.Empty;
        public static NLog.Logger Logger = LogManager.GetCurrentClassLogger();
        public ResultEnum Status { get { return status; } set { status = value; } }
        public string ErrorMessage { get { return errormessage; } set { errormessage = value; } }
        public static readonly OWCHelper _owchelper = new OWCHelper();

        public void SaveWorkPackage(WorkPackage workPackageObj)
        {
            WorkPackageValidator workPackageValidator = new WorkPackageValidator();

            var validationResult = workPackageValidator.Validate(workPackageObj);

            if (validationResult != null && validationResult.IsValid)
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    using (var transaction = Utility.CreateTransactionScope())
                    {
                        try
                        {
                            var quizObj = UOWObj.QuizRepository.GetByID(workPackageObj.QuizId);

                            if (quizObj != null && quizObj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active) != null)
                            {
                                var uniqueCode = Guid.NewGuid().ToString();
                                var currentDate = DateTime.UtcNow;

                                var quizDetails = quizObj.QuizDetails.LastOrDefault(r => r.State == (int)QuizStateEnum.PUBLISHED);
                                if (workPackageObj.IsUpdatedSend && ((string.IsNullOrEmpty(workPackageObj.Body) || string.IsNullOrEmpty(workPackageObj.Subject)) && string.IsNullOrEmpty(workPackageObj.SMSText)))
                                {
                                    Status = ResultEnum.Error;
                                    ErrorMessage = "EmailSubject, body or sms text are required";
                                    return;
                                }

                                if (quizDetails != null)
                                {
                                    CompanyModel companyObj;
                                    if (quizObj.Company != null)
                                    {
                                        companyObj = new CompanyModel
                                        {
                                            Id = quizObj.Company.Id,
                                            AlternateClientCodes = quizObj.Company.AlternateClientCodes,
                                            ClientCode = quizObj.Company.ClientCode,
                                            CompanyName = quizObj.Company.CompanyName,
                                            CompanyWebsiteUrl = quizObj.Company.CompanyWebsiteUrl,
                                            JobRocketApiAuthorizationBearer = quizObj.Company.JobRocketApiAuthorizationBearer,
                                            JobRocketApiUrl = quizObj.Company.JobRocketApiUrl,
                                            JobRocketClientUrl = quizObj.Company.JobRocketClientUrl,
                                            LeadDashboardApiAuthorizationBearer = quizObj.Company.LeadDashboardApiAuthorizationBearer,
                                            LeadDashboardApiUrl = quizObj.Company.LeadDashboardApiUrl,
                                            LeadDashboardClientUrl = quizObj.Company.LeadDashboardClientUrl,
                                            LogoUrl = quizObj.Company.LogoUrl,
                                            PrimaryBrandingColor = quizObj.Company.PrimaryBrandingColor,
                                            SecondaryBrandingColor = quizObj.Company.SecondaryBrandingColor

                                        };
                                    }
                                    else
                                    {
                                        companyObj = new CompanyModel();
                                    }

                                    var leadUserInfo = OWCHelper.GetLeadUserInfo(workPackageObj.LeadUserId, companyObj);

                                    if (leadUserInfo != null && !string.IsNullOrEmpty(leadUserInfo.contactId))
                                    {
                                        #region insert in WorkPackageInfo

                                        var workPackageInfoObj = new Db.WorkPackageInfo();

                                        workPackageInfoObj.LeadUserId = workPackageObj.LeadUserId;
                                        workPackageInfoObj.QuizId = workPackageObj.QuizId;
                                        workPackageInfoObj.BusinessUserId = workPackageObj.BusinessUserId;
                                        workPackageInfoObj.CampaignId = workPackageObj.SourceId;
                                        workPackageInfoObj.CampaignName = workPackageObj.SourceName;
                                        workPackageInfoObj.Status = (int)WorkPackageStatusEnum.Pending;
                                        workPackageInfoObj.CreatedOn = currentDate;

                                        UOWObj.WorkPackageInfoRepository.Insert(workPackageInfoObj);
                                        UOWObj.Save();

                                        #endregion

                                        #region insert in VariablesDetails

                                        foreach (var obj in workPackageObj.DynamicVariables)
                                        {
                                            var VariablesObj = UOWObj.VariablesRepository.Get(r => r.VariableInQuiz.Any(s => s.QuizId == quizDetails.Id) && r.Name.ToLower() == obj.Key.ToLower()).FirstOrDefault();

                                            if (VariablesObj != null)
                                            {
                                                var id = VariablesObj.VariableInQuiz.FirstOrDefault(r => r.QuizId == quizDetails.Id).Id;
                                                var KeyExistInVariablesDetails = UOWObj.VariablesDetailsRepository.Get(s => s.VariableInQuizId == id && s.LeadId == workPackageObj.LeadUserId).FirstOrDefault();
                                                if (KeyExistInVariablesDetails == null)
                                                {
                                                    var variablesDetailsObj = new Db.VariablesDetails();

                                                    variablesDetailsObj.LeadId = workPackageObj.LeadUserId;
                                                    variablesDetailsObj.VariableInQuizId = VariablesObj.VariableInQuiz.FirstOrDefault(r => r.QuizId == quizDetails.Id).Id;
                                                    variablesDetailsObj.VariableValue = obj.Value == null ? null : obj.Value.Trim(); 

                                                    UOWObj.VariablesDetailsRepository.Insert(variablesDetailsObj);
                                                }
                                                else
                                                {
                                                    KeyExistInVariablesDetails.VariableValue = obj.Value == null ? null : obj.Value.Trim();
                                                    UOWObj.VariablesDetailsRepository.Update(KeyExistInVariablesDetails);
                                                }
                                                UOWObj.Save();
                                            }
                                        }

                                        #endregion

                                        #region insert in LeadDataInAction

                                        foreach (var obj in workPackageObj.LeadDataInActionList)
                                        {
                                            var leadDataInAction = UOWObj.LeadDataInActionRepository.Get(r => r.ActionId == obj.ActionId && r.LeadUserId == workPackageObj.LeadUserId).FirstOrDefault();

                                            var actionsInQuiz = quizDetails.ActionsInQuiz.Where(t => t.Id == obj.ActionId).FirstOrDefault();

                                            UpdateleadDataInActionValidator emailListValidator = new UpdateleadDataInActionValidator();
                                            var validationEmailResult = emailListValidator.Validate(obj.ReportEmails.Split(',').Select(t => t.Trim()).ToList());

                                            if (actionsInQuiz != null && (((actionsInQuiz.ActionType == (int)ActionTypeEnum.LinkWithLeadDashboardAppointment || actionsInQuiz.ActionType == (int)ActionTypeEnum.Appointment) && obj.AppointmentTypeId > 0) || ((actionsInQuiz.ActionType == (int)ActionTypeEnum.LinkWithLeadDashboardAppointment || actionsInQuiz.ActionType == (int)ActionTypeEnum.ReportEmail) && !string.IsNullOrEmpty(obj.ReportEmails) && validationEmailResult != null && validationEmailResult.IsValid)))
                                            {
                                                if (leadDataInAction == null)
                                                {
                                                    var leadDataInActionObj = new Db.LeadDataInAction();

                                                    leadDataInActionObj.LeadUserId = workPackageObj.LeadUserId;
                                                    leadDataInActionObj.ActionId = obj.ActionId;
                                                    leadDataInActionObj.AppointmentTypeId = obj.AppointmentTypeId > 0 ? obj.AppointmentTypeId : default(int?);
                                                    leadDataInActionObj.ReportEmails = !string.IsNullOrEmpty(obj.ReportEmails) ? obj.ReportEmails : null;
                                                    if (obj.IsUpdatedSend && !obj.SendMailNotRequired && !string.IsNullOrEmpty(obj.Subject) && !string.IsNullOrEmpty(obj.Body) && !string.IsNullOrEmpty(obj.SMSText))
                                                    {
                                                        leadDataInActionObj.IsUpdatedSend = obj.IsUpdatedSend;
                                                        leadDataInActionObj.Subject = obj.Subject;
                                                        leadDataInActionObj.Body = obj.Body;
                                                        leadDataInActionObj.SMSText = obj.SMSText;
                                                    }


                                                    if (obj.CalendarIds != null)
                                                    {
                                                        foreach (var calendarId in obj.CalendarIds.ToList())
                                                        {
                                                            var leadCalendarDataInActionObj = new Db.LeadCalendarDataInAction();
                                                            leadCalendarDataInActionObj.LeadDataInActionId = leadDataInActionObj.Id;
                                                            leadCalendarDataInActionObj.CalendarId = calendarId;
                                                            UOWObj.LeadCalendarDataInActionRepository.Insert(leadCalendarDataInActionObj);
                                                        }
                                                    }

                                                    UOWObj.LeadDataInActionRepository.Insert(leadDataInActionObj);
                                                }
                                                else
                                                {
                                                    leadDataInAction.AppointmentTypeId = obj.AppointmentTypeId > 0 ? obj.AppointmentTypeId : default(int?);
                                                    leadDataInAction.ReportEmails = !string.IsNullOrEmpty(obj.ReportEmails) ? obj.ReportEmails : null;
                                                    if (obj.IsUpdatedSend && !obj.SendMailNotRequired && !string.IsNullOrEmpty(obj.Subject) && !string.IsNullOrEmpty(obj.Body) && !string.IsNullOrEmpty(obj.SMSText))
                                                    {
                                                        leadDataInAction.IsUpdatedSend = obj.IsUpdatedSend;
                                                        leadDataInAction.Subject = obj.Subject;
                                                        leadDataInAction.Body = obj.Body;
                                                        leadDataInAction.SMSText = obj.SMSText;
                                                    }
                                                    else
                                                    {
                                                        leadDataInAction.IsUpdatedSend = false;
                                                        leadDataInAction.Subject = string.Empty;
                                                        leadDataInAction.Body = string.Empty;
                                                        leadDataInAction.SMSText = string.Empty;
                                                    }

                                                    if (leadDataInAction.LeadCalendarDataInAction != null)
                                                    {
                                                        foreach (var leadCalendarDataInActionObj in leadDataInAction.LeadCalendarDataInAction.ToList())
                                                        {
                                                            UOWObj.LeadCalendarDataInActionRepository.Delete(leadCalendarDataInActionObj);
                                                        }
                                                    }

                                                    if (obj.CalendarIds != null)
                                                    {
                                                        foreach (var calendarId in obj.CalendarIds.ToList())
                                                        {
                                                            var leadCalendarDataInActionObj = new Db.LeadCalendarDataInAction();
                                                            leadCalendarDataInActionObj.LeadDataInActionId = leadDataInAction.Id;
                                                            leadCalendarDataInActionObj.CalendarId = calendarId;
                                                            UOWObj.LeadCalendarDataInActionRepository.Insert(leadCalendarDataInActionObj);
                                                        }
                                                    }

                                                    UOWObj.LeadDataInActionRepository.Update(leadDataInAction);
                                                }
                                                UOWObj.Save();
                                            }
                                        }

                                        #endregion

                                        #region Send Email and SMS

                                        var template = quizObj.NotificationTemplatesInQuiz.FirstOrDefault(r => r.NotificationTemplate.Status == (int)StatusEnum.Active && r.NotificationTemplate.NotificationType == (int)NotificationTypeEnum.INVITATION);

                                        if (!string.IsNullOrEmpty(leadUserInfo.email))
                                        {
                                            string body = string.Empty;
                                            var sendMailStatus = false;
                                            var sendSmsStatus = false;

                                            var title = quizDetails.QuizTitle.Replace("%fname%", leadUserInfo.firstName)
                                                                                 .Replace("%lname%", leadUserInfo.lastName)
                                                                                 .Replace("%phone%", leadUserInfo.telephone)
                                                                                 .Replace("%email%", leadUserInfo.email)
                                                                                 .Replace("%leadid%", workPackageObj.LeadUserId)
                                                                                 .Replace("%qname%", string.Empty)
                                                                                 .Replace("%qlink%", string.Empty)
                                                                                 .Replace("%qendresult%", string.Empty)
                                                                                 .Replace("%correctanswerexplanation%", (quizDetails.QuestionsInQuiz != null && quizDetails.QuestionsInQuiz.Any()) ? quizDetails.QuestionsInQuiz.FirstOrDefault().AliasTextForCorrectAnswer : string.Empty);

                                            IEnumerable<Db.VariablesDetails> variablesDetailList = UOWObj.VariablesDetailsRepository.Get(r => r.VariableInQuiz.QuizId == quizDetails.ParentQuizId && r.LeadId == workPackageObj.LeadUserId);

                                            MatchCollection mcol = Regex.Matches(title, @"%\b\S+?\b%");

                                            foreach (Match m in mcol)
                                            {
                                                var variablesDetailObj = variablesDetailList.FirstOrDefault(t => t.VariableInQuiz.Variables.Name == m.ToString().ToLower().Replace("%", string.Empty));
                                                if (variablesDetailObj != null)
                                                {
                                                    title = title.Replace(m.ToString(), variablesDetailObj.VariableValue);
                                                }
                                                else
                                                {
                                                    title = title.Replace(m.ToString(), string.Empty);
                                                }
                                            }

                                            var shotenUrlCode = string.Empty;
                                            var shortUrl = string.Empty;

                                            #region url shortning by custom domain

                                            //var domainList = OWCHelper.GetClientDomains(companyObj);
                                            var domainList = CommonStaticData.GetCachedClientDomains(companyObj);
                                            var domain = (domainList != null && domainList.Any()) ? domainList.FirstOrDefault().Domain : GlobalSettings.defaultDomain.ToString();

                                            if (template != null && !string.IsNullOrEmpty(template.NotificationTemplate.EmailLinkVariable))
                                                shotenUrlCode = template.NotificationTemplate.EmailLinkVariable + "-" + IdGenerator.GetShortCode();
                                            else
                                                shotenUrlCode = IdGenerator.GetShortCode();

                                            #region insert QuizUrlSetting

                                            var quizUrlSettingObj = new Db.QuizUrlSetting();

                                            quizUrlSettingObj.Key = shotenUrlCode;
                                            quizUrlSettingObj.DomainName = domain;
                                            quizUrlSettingObj.CompanyId = quizObj.CompanyId.HasValue ? quizObj.CompanyId.Value : 0;
                                            quizUrlSettingObj.Value = GlobalSettings.webUrl.ToString() + "/quiz?Code=" + quizObj.PublishedCode + "&UserTypeId=2&UserId=" + workPackageObj.LeadUserId + "&WorkPackageInfoId=" + workPackageInfoObj.Id;

                                            UOWObj.QuizUrlSettingRepository.Insert(quizUrlSettingObj);
                                            UOWObj.Save();

                                            shortUrl = "https://" + quizUrlSettingObj.DomainName + "/" + quizUrlSettingObj.Key;

                                            #endregion

                                            #region update ShotenUrlCode in WorkPackageInfo

                                            workPackageInfoObj.ShotenUrlCode = shotenUrlCode;

                                            workPackageInfoObj.IsOurEndLogicForInvitation = true;

                                            if (template != null && !string.IsNullOrEmpty(template.NotificationTemplate.EmailLinkVariable))
                                                workPackageInfoObj.EmailLinkVariableForInvitation = template.NotificationTemplate.EmailLinkVariable;

                                            UOWObj.WorkPackageInfoRepository.Update(workPackageInfoObj);
                                            UOWObj.Save();

                                            #endregion

                                            #endregion

                                            if (!workPackageObj.IsUpdatedSend)
                                            {
                                                if (template != null && !String.IsNullOrEmpty(template.NotificationTemplate.Body))
                                                {
                                                    body = template.NotificationTemplate.Body.Replace("%fname%", leadUserInfo.firstName)
                                                                                             .Replace("%lname%", leadUserInfo.lastName)
                                                                                             .Replace("%phone%", leadUserInfo.telephone)
                                                                                             .Replace("%email%", leadUserInfo.email)
                                                                                             .Replace("%leadid%", workPackageObj.LeadUserId)
                                                                                             .Replace("%qname%", title)
                                                                                             .Replace("%qtype%", ((QuizTypeEnum)quizObj.QuizType).ToString())
                                                                                             .Replace("%qlink%", "<a href=\"" + shortUrl + "\">" + shortUrl + "</a>")
                                                                                             .Replace("%sourcename%", String.IsNullOrEmpty(workPackageInfoObj.CampaignName) ? string.Empty : workPackageInfoObj.CampaignName);


                                                    sendMailStatus = CommunicationHelper.SendMailWithAttachment(leadUserInfo.email, String.IsNullOrEmpty(template.NotificationTemplate.Subject) ? string.Empty : template.NotificationTemplate.Subject.Replace
                                                        ("%qname%", title).Replace("%sourcename%", String.IsNullOrEmpty(workPackageInfoObj.CampaignName) ? string.Empty : workPackageInfoObj.CampaignName), body, companyObj.ClientCode, (template.NotificationTemplate.AttachmentsInNotificationTemplate != null && template.NotificationTemplate.AttachmentsInNotificationTemplate.Any()) ? template.NotificationTemplate.AttachmentsInNotificationTemplate.Select(r => new Models.FileAttachment { FileLink = r.Description, FileName = r.Title }).ToList() : null);
                                                }

                                                string smsBody = string.Empty;

                                                if (template != null && !String.IsNullOrEmpty(template.NotificationTemplate.SMSText))
                                                {
                                                    smsBody = template.NotificationTemplate.SMSText.Replace("%fname%", leadUserInfo.firstName)
                                                                                                   .Replace("%lname%", leadUserInfo.lastName)
                                                                                                   .Replace("%phone%", leadUserInfo.telephone)
                                                                                                   .Replace("%email%", leadUserInfo.email)
                                                                                                   .Replace("%leadid%", workPackageObj.LeadUserId)
                                                                                                   .Replace("%qname%", title)
                                                                                                   .Replace("%qtype%", ((QuizTypeEnum)quizObj.QuizType).ToString())
                                                                                                   .Replace("%qlink%", shortUrl)
                                                                                                   .Replace("%sourcename%", String.IsNullOrEmpty(workPackageInfoObj.CampaignName) ? string.Empty : workPackageInfoObj.CampaignName);

                                                    sendSmsStatus = CommunicationHelper.SendSMS(leadUserInfo.telephone, smsBody, companyObj);
                                                }


                                            }
                                            else
                                            {
                                                if (!string.IsNullOrEmpty(workPackageObj.LeadUserId))
                                                {
                                                    if (!string.IsNullOrEmpty(workPackageObj.Body) && !string.IsNullOrEmpty(workPackageObj.Subject))
                                                    {

                                                        body = workPackageObj.Body.Replace("&lt;", "<")
                                                                          .Replace("&gt;", " >")
                                                                          .Replace("%fname%", leadUserInfo.firstName)
                                                                          .Replace("%lname%", leadUserInfo.lastName)
                                                                          .Replace("%phone%", leadUserInfo.telephone)
                                                                          .Replace("%email%", leadUserInfo.email)
                                                                          .Replace("%leadid%", workPackageObj.LeadUserId)
                                                                          .Replace("%qname%", title)
                                                                          .Replace("%qtype%", ((QuizTypeEnum)quizObj.QuizType).ToString())
                                                                          .Replace("%qlink%", "<a href=\"" + shortUrl + "\">" + shortUrl + "</a>")
                                                                          .Replace("%sourcename%", String.IsNullOrEmpty(workPackageInfoObj.CampaignName) ? string.Empty : workPackageInfoObj.CampaignName);

                                                        sendMailStatus = CommunicationHelper.SendMailWithAttachment(leadUserInfo.email, String.IsNullOrEmpty(workPackageObj.Subject) ? string.Empty : workPackageObj.Subject.Replace("%qname%", title).Replace("%sourcename%", string.IsNullOrEmpty(workPackageInfoObj.CampaignName) ? string.Empty : workPackageInfoObj.CampaignName), body, companyObj.ClientCode, (template != null && template.NotificationTemplate.AttachmentsInNotificationTemplate != null && template.NotificationTemplate.AttachmentsInNotificationTemplate.Any()) ? template.NotificationTemplate.AttachmentsInNotificationTemplate.Select(r => new Models.FileAttachment { FileLink = r.Description, FileName = r.Title }).ToList() : null);
                                                    }

                                                    if (!string.IsNullOrEmpty(workPackageObj.SMSText))
                                                    {
                                                        var smsBody = workPackageObj.SMSText.Replace("%fname%", leadUserInfo.firstName)
                                                                                       .Replace("%lname%", leadUserInfo.lastName)
                                                                                       .Replace("%phone%", leadUserInfo.telephone)
                                                                                       .Replace("%email%", leadUserInfo.email)
                                                                                       .Replace("%leadid%", workPackageObj.LeadUserId)
                                                                                       .Replace("%qname%", title)
                                                                                       .Replace("%qtype%", ((QuizTypeEnum)quizObj.QuizType).ToString())
                                                                                       .Replace("%qlink%", shortUrl)
                                                                                       .Replace("%sourcename%", String.IsNullOrEmpty(workPackageInfoObj.CampaignName) ? string.Empty : workPackageInfoObj.CampaignName);

                                                        sendSmsStatus = CommunicationHelper.SendSMS(leadUserInfo.telephone, smsBody, companyObj);
                                                    }
                                                }
                                            }

                                            workPackageInfoObj.EmailSentOn = sendMailStatus ? currentDate : default(DateTime?);
                                            workPackageInfoObj.SMSSentOn = sendSmsStatus ? currentDate : default(DateTime?);
                                            UOWObj.WorkPackageInfoRepository.Update(workPackageInfoObj);
                                            UOWObj.Save();
                                        }

                                        #endregion

                                        #region insert in reminderQueues

                                        var remindersInQuizObj = UOWObj.RemindersInQuizRepository.Get(a => (string.IsNullOrEmpty(quizObj.AccessibleOfficeId) ? string.IsNullOrEmpty(a.OfficeId) : a.OfficeId == quizObj.AccessibleOfficeId) && a.CompanyId == quizObj.CompanyId).FirstOrDefault();
                                        if (remindersInQuizObj != null)
                                        {
                                            var reminderQueuesObj = new Db.ReminderQueues();
                                            reminderQueuesObj.ReminderInQuizId = remindersInQuizObj.Id;
                                            reminderQueuesObj.Type = (int)ReminderTypeEnum.EMAIL;
                                            reminderQueuesObj.QueuedOn = currentDate;
                                            reminderQueuesObj.WorkPackageInfoId = workPackageInfoObj.Id;

                                            if (remindersInQuizObj.FirstReminder.HasValue)
                                            {
                                                reminderQueuesObj.ReminderLevel = 1;
                                                UOWObj.ReminderQueuesRepository.Insert(reminderQueuesObj);
                                                UOWObj.Save();
                                            }

                                            if (remindersInQuizObj.SecondReminder.HasValue)
                                            {
                                                reminderQueuesObj.ReminderLevel = 2;
                                                UOWObj.ReminderQueuesRepository.Insert(reminderQueuesObj);
                                                UOWObj.Save();
                                            }

                                            if (remindersInQuizObj.ThirdReminder.HasValue)
                                            {
                                                reminderQueuesObj.ReminderLevel = 3;
                                                UOWObj.ReminderQueuesRepository.Insert(reminderQueuesObj);
                                                UOWObj.Save();
                                            }

                                            reminderQueuesObj.Type = (int)ReminderTypeEnum.SMS;

                                            if (remindersInQuizObj.FirstReminder.HasValue)
                                            {
                                                reminderQueuesObj.ReminderLevel = 1;
                                                UOWObj.ReminderQueuesRepository.Insert(reminderQueuesObj);
                                                UOWObj.Save();
                                            }

                                            if (remindersInQuizObj.SecondReminder.HasValue)
                                            {
                                                reminderQueuesObj.ReminderLevel = 2;
                                                UOWObj.ReminderQueuesRepository.Insert(reminderQueuesObj);
                                                UOWObj.Save();
                                            }

                                            if (remindersInQuizObj.ThirdReminder.HasValue)
                                            {
                                                reminderQueuesObj.ReminderLevel = 3;
                                                UOWObj.ReminderQueuesRepository.Insert(reminderQueuesObj);
                                                UOWObj.Save();
                                            }

                                            reminderQueuesObj.Type = (int)ReminderTypeEnum.WHATSAPP;

                                            if (remindersInQuizObj.FirstReminder.HasValue) {
                                                reminderQueuesObj.ReminderLevel = 1;
                                                UOWObj.ReminderQueuesRepository.Insert(reminderQueuesObj);
                                                UOWObj.Save();
                                            }

                                            if (remindersInQuizObj.SecondReminder.HasValue) {
                                                reminderQueuesObj.ReminderLevel = 2;
                                                UOWObj.ReminderQueuesRepository.Insert(reminderQueuesObj);
                                                UOWObj.Save();
                                            }

                                            if (remindersInQuizObj.ThirdReminder.HasValue) {
                                                reminderQueuesObj.ReminderLevel = 3;
                                                UOWObj.ReminderQueuesRepository.Insert(reminderQueuesObj);
                                                UOWObj.Save();
                                            }
                                        }
                                        #endregion
                                    }
                                    else
                                    {
                                        Status = ResultEnum.Error;
                                        ErrorMessage = "Lead Info not found for Id " + workPackageObj.LeadUserId.ToString();
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
                                ErrorMessage = "Quiz does not exists for QuizId " + workPackageObj.QuizId;
                            }
                        }
                        catch (Exception ex)
                        {
                            Status = ResultEnum.Error;
                            ErrorMessage = ex.Message;
                            throw ex;
                        }
                        transaction.Complete();
                    }
                }
            }
            else
            {
                Status = ResultEnum.Error;
                ErrorMessage = "Validation error: " + String.Join(", ", validationResult.Errors.Select(r => r.ErrorMessage));
            }
        }

        public void PushWorkPackage(PushWorkPackage workPackageObj)
        {
            PushWorkPackageValidator workPackageValidator = new PushWorkPackageValidator();
            TempWorkpackagePush.ContactBasicDetails leadUserInfo = null;
            IEnumerable<Db.VariablesDetails> variablesDetailList = null;
            var exObj = new Dictionary<string, object>();
            CompanyModel companyObj = null;
            bool isSendEmail = false;
            int workPackageInfoObjId = 0;
            int quizDetailId = 0;
            Db.ConfigurationDetails configurationDetailsobj = null;
            var validationResult = workPackageValidator.Validate(workPackageObj);
            var uniqueCode = Guid.NewGuid().ToString();
            var currentDate = DateTime.UtcNow;
            Db.QuizDetails quizDetails = null;
            Db.Quiz quizObj = null;
            int? usageType = 1;

            if (!(validationResult != null && validationResult.IsValid))
            {
                Status = ResultEnum.Error;
                ErrorMessage = "Validation error: " + String.Join(", ", validationResult.Errors.Select(r => r.ErrorMessage));
            }

            if (workPackageObj.WorkPackageInfoId != 0)
            {
                Db.WorkPackageInfo workPackageInfoObj = null;
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    workPackageInfoObj = UOWObj.WorkPackageInfoRepository.GetByID(workPackageObj.WorkPackageInfoId);


                    configurationDetailsobj = UOWObj.ConfigurationDetailsRepository.GetByID(workPackageInfoObj.ConfigurationDetailsId);
                    quizObj = UOWObj.QuizRepository.GetByID(configurationDetailsobj.QuizId);

                    if (!(quizObj != null && quizObj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active) != null))
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz does not exists for QuizId " + configurationDetailsobj.QuizId;
                    }

                    quizDetails = quizObj.QuizDetails.LastOrDefault(r => r.State == (int)QuizStateEnum.PUBLISHED);
                    quizDetailId = quizDetails.Id;
                    usageType = quizObj.UsageTypeInQuiz.FirstOrDefault()?.UsageType;
                    if (quizDetails == null)
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz is not yet published.";
                    }

                    companyObj = GetQuizCompany(quizObj.Company);
                    variablesDetailList = UOWObj.VariablesDetailsRepository.Get(r => r.VariableInQuiz.QuizId == configurationDetailsobj.QuizId && r.ConfigurationDetailsId == configurationDetailsobj.Id);

                }

                if (workPackageInfoObj != null)
                {
                    //leadUserInfo = OWCHelper.GetLeadUserInfo(workPackageInfoObj.LeadUserId, companyObj);
                    var domainList = CommonStaticData.GetCachedClientDomains(companyObj);
                    var domain = (domainList != null && domainList.Any()) ? domainList.FirstOrDefault().Domain : GlobalSettings.defaultDomain.ToString();
                    var shortUrl = "https://" + domain + "/" + workPackageInfoObj.ShotenUrlCode;
                    var qlinkKey = workPackageInfoObj.ShotenUrlCode;
                    var commContactInfo = _owchelper.GetCommContactDetails(workPackageInfoObj.LeadUserId, companyObj.ClientCode);

                    if (commContactInfo != null && commContactInfo.Any())
                    {
                        exObj = (JsonConvert.DeserializeObject<Dictionary<string, object>>(commContactInfo.ToString()));
                        leadUserInfo = (JsonConvert.DeserializeObject<TempWorkpackagePush.ContactBasicDetails>(commContactInfo.ToString()));
                    }

                    var tempWorkpackagePush =
                    new TempWorkpackagePush
                    {

                        workPackageInfoId = workPackageInfoObj.Id,
                        quizDetailid = quizDetails.Id,
                        companyObj = companyObj,
                        leadUserInfo = leadUserInfo,
                        shortUrl = shortUrl,
                        shotenUrlCode = workPackageInfoObj.ShotenUrlCode,
                        quizType = quizObj.QuizType,
                        parentquizid = quizObj.Id,
                        WorkpackageCampaignName = workPackageInfoObj.CampaignName,
                        LeadUserId = workPackageInfoObj.LeadUserId,
                        ConfigurationDetailId = workPackageInfoObj.ConfigurationDetailsId.Value,
                        QuizDetails = quizDetails,
                        UserToken = workPackageObj.UserToken,
                        ConfigurationDetails = configurationDetailsobj,
                        ContactObject = exObj,
                        UsageType = usageType.HasValue ? usageType.Value : 1,
                        qLinkKey = qlinkKey
                    };
                  
                        
                        WorkpackageCommunicationService.SendEmailSmsandWhatsappHandlerNew(tempWorkpackagePush);
   
                }
                return;
            }


            try
            {


                using (var UOWObj = new AutomationUnitOfWork())
                {

                    var configurationDetailsList = UOWObj.ConfigurationDetailsRepository.Get(r => r.ConfigurationId == workPackageObj.ConfigurationId);

                    if (configurationDetailsList != null && configurationDetailsList.Any())
                    {
                        var configurationDetail = configurationDetailsList.FirstOrDefault();

                        if (configurationDetail != null)
                        {
                            configurationDetailsobj = configurationDetail;
                        }
                    }
                }

                if (configurationDetailsobj == null)
                {
                    configurationDetailsobj = SaveAutomationconfigurationDetails(workPackageObj);

                }

                if (configurationDetailsobj == null)
                {
                    return;
                }

                List<TempWorkpackagePush> tempWorkpackagePusheslist = new List<TempWorkpackagePush>();
                using (var UOWObj = new AutomationUnitOfWork())
                {

                    quizObj = UOWObj.QuizRepository.GetByID(configurationDetailsobj.QuizId);

                    if (!(quizObj != null && quizObj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active) != null))
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz does not exists for QuizId " + configurationDetailsobj.QuizId;
                    }

                    quizDetails = quizObj.QuizDetails.LastOrDefault(r => r.State == (int)QuizStateEnum.PUBLISHED);
                    quizDetailId = quizDetails.Id;
                    usageType = quizObj.UsageTypeInQuiz.FirstOrDefault()?.UsageType;
                    if (quizDetails == null)
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz is not yet published.";
                    }

                    if (Status == ResultEnum.Error) {
                        return;
                    }

                    var template = UOWObj.NotificationTemplatesInQuizRepository.Get(r => r.NotificationTemplate.Status == (int)StatusEnum.Active && r.QuizId == quizObj.Id && r.NotificationTemplate.NotificationType == (int)NotificationTypeEnum.INVITATION).FirstOrDefault();


                    companyObj = GetQuizCompany(quizObj.Company);

                    var remindersInQuizObj = UOWObj.RemindersInQuizRepository.Get(a => (string.IsNullOrEmpty(quizObj.AccessibleOfficeId) ? string.IsNullOrEmpty(a.OfficeId) : a.OfficeId == quizObj.AccessibleOfficeId) && a.CompanyId == quizObj.CompanyId).FirstOrDefault();

                    variablesDetailList = UOWObj.VariablesDetailsRepository.Get(r => r.VariableInQuiz.QuizId == configurationDetailsobj.QuizId && r.ConfigurationDetailsId == configurationDetailsobj.Id);

                    var domainList = CommonStaticData.GetCachedClientDomains(companyObj);
                    var domain = (domainList != null && domainList.Any()) ? domainList.FirstOrDefault().Domain : GlobalSettings.defaultDomain.ToString();

                    foreach (var LeadUserId in workPackageObj.ContactIds)
                    {

                        // leadUserInfo = OWCHelper.GetLeadUserInfo(LeadUserId, companyObj);
                        //if (string.IsNullOrWhiteSpace(leadUserInfo?.contactId))
                        //{
                        //    Status = ResultEnum.Error;
                        //    ErrorMessage = "Lead Info not found for Id " + LeadUserId.ToString();
                        //}


                        var commContactInfo = _owchelper.GetCommContactDetails(LeadUserId, companyObj.ClientCode);
                        if (commContactInfo != null && commContactInfo.Any())
                        {
                            exObj = (JsonConvert.DeserializeObject<Dictionary<string, object>>(commContactInfo.ToString()));
                            leadUserInfo = (JsonConvert.DeserializeObject<TempWorkpackagePush.ContactBasicDetails>(commContactInfo.ToString()));
                        }

                            if (!exObj.CheckDictionarykeyExistStringObject("contactid"))
                            {
                                Status = ResultEnum.Error;
                                ErrorMessage = "Lead Info not found for Id " + LeadUserId.ToString();
                            }

                        var contactidKeyvalue = exObj.GetDictionarykeyValueStringObject("contactid");
                            if (string.IsNullOrWhiteSpace(contactidKeyvalue))   {
                                Status = ResultEnum.Error;
                                ErrorMessage = "Lead Info not found for Id " + LeadUserId.ToString();
                            }

                            if (Status == ResultEnum.Error) {
                                continue;
                            }

                        var workPackageInfoObj = new Db.WorkPackageInfo();
                            var shotenUrlCode = IdGenerator.GetShortCode();
                            if (template != null && template.NotificationTemplate != null && !string.IsNullOrEmpty(template.NotificationTemplate.EmailLinkVariable))
                            {
                                shotenUrlCode = template.NotificationTemplate.EmailLinkVariable + "-" + IdGenerator.GetShortCode();
                                workPackageInfoObj.EmailLinkVariableForInvitation = template.NotificationTemplate.EmailLinkVariable;

                            }

                            #region insert in WorkPackageInfo

                            workPackageInfoObj.LeadUserId = LeadUserId;
                            workPackageInfoObj.QuizId = configurationDetailsobj.QuizId;
                            workPackageInfoObj.CampaignId = leadUserInfo.SourceId;
                            workPackageInfoObj.CampaignName = leadUserInfo.SourceName;
                            workPackageInfoObj.Status = (int)WorkPackageStatusEnum.Pending;
                            workPackageInfoObj.CreatedOn = currentDate;
                            workPackageInfoObj.ShotenUrlCode = shotenUrlCode;
                            workPackageInfoObj.ConfigurationDetailsId = configurationDetailsobj.Id;
                            workPackageInfoObj.IsOurEndLogicForInvitation = true;
                            workPackageInfoObj.RequestId = workPackageObj.RequestId;
                            UOWObj.WorkPackageInfoRepository.Insert(workPackageInfoObj);
                            UOWObj.Save();
                            #endregion

                            var quizUrlSettingObj = new Db.QuizUrlSetting();
                            quizUrlSettingObj.Key = shotenUrlCode;
                            quizUrlSettingObj.DomainName = domain;
                            quizUrlSettingObj.CompanyId = quizObj.CompanyId.HasValue ? quizObj.CompanyId.Value : 0;
                            quizUrlSettingObj.Value = GlobalSettings.webUrl.ToString() + "/quiz?Code=" + quizObj.PublishedCode + "&UserTypeId=2&UserId=" + workPackageInfoObj.LeadUserId + "&WorkPackageInfoId=" + workPackageInfoObj.Id;
                            var shortUrl = "https://" + quizUrlSettingObj.DomainName + "/" + quizUrlSettingObj.Key;
                            var qlinkKey = quizUrlSettingObj.Key;
                            UOWObj.QuizUrlSettingRepository.Insert(quizUrlSettingObj);
                            UOWObj.Save();


                            #region insert in reminderQueues

                            if (remindersInQuizObj != null)
                            {
                                var reminderQueuesObj = new Db.ReminderQueues();
                                reminderQueuesObj.ReminderInQuizId = remindersInQuizObj.Id;
                                reminderQueuesObj.Type = (int)ReminderTypeEnum.EMAIL;
                                reminderQueuesObj.QueuedOn = currentDate;
                                reminderQueuesObj.WorkPackageInfoId = workPackageInfoObj.Id;

                                if (remindersInQuizObj.FirstReminder.HasValue)
                                {
                                    reminderQueuesObj.ReminderLevel = 1;
                                    UOWObj.ReminderQueuesRepository.Insert(reminderQueuesObj);
                                    UOWObj.Save();
                                }

                                if (remindersInQuizObj.SecondReminder.HasValue)
                                {
                                    reminderQueuesObj.ReminderLevel = 2;
                                    UOWObj.ReminderQueuesRepository.Insert(reminderQueuesObj);
                                    UOWObj.Save();
                                }

                                if (remindersInQuizObj.ThirdReminder.HasValue)
                                {
                                    reminderQueuesObj.ReminderLevel = 3;
                                    UOWObj.ReminderQueuesRepository.Insert(reminderQueuesObj);
                                    UOWObj.Save();
                                }

                                reminderQueuesObj.Type = (int)ReminderTypeEnum.SMS;

                                if (remindersInQuizObj.FirstReminder.HasValue)
                                {
                                    reminderQueuesObj.ReminderLevel = 1;
                                    UOWObj.ReminderQueuesRepository.Insert(reminderQueuesObj);
                                    UOWObj.Save();
                                }

                                if (remindersInQuizObj.SecondReminder.HasValue)
                                {
                                    reminderQueuesObj.ReminderLevel = 2;
                                    UOWObj.ReminderQueuesRepository.Insert(reminderQueuesObj);
                                    UOWObj.Save();
                                }

                                if (remindersInQuizObj.ThirdReminder.HasValue)
                                {
                                    reminderQueuesObj.ReminderLevel = 3;
                                    UOWObj.ReminderQueuesRepository.Insert(reminderQueuesObj);
                                    UOWObj.Save();
                                }
                            reminderQueuesObj.Type = (int)ReminderTypeEnum.WHATSAPP;

                            if (remindersInQuizObj.FirstReminder.HasValue) {
                                reminderQueuesObj.ReminderLevel = 1;
                                UOWObj.ReminderQueuesRepository.Insert(reminderQueuesObj);
                                UOWObj.Save();
                            }

                            if (remindersInQuizObj.SecondReminder.HasValue) {
                                reminderQueuesObj.ReminderLevel = 2;
                                UOWObj.ReminderQueuesRepository.Insert(reminderQueuesObj);
                                UOWObj.Save();
                            }

                            if (remindersInQuizObj.ThirdReminder.HasValue) {
                                reminderQueuesObj.ReminderLevel = 3;
                                UOWObj.ReminderQueuesRepository.Insert(reminderQueuesObj);
                                UOWObj.Save();
                            }

                        }
                            #endregion

                            isSendEmail = true;

                            tempWorkpackagePusheslist.Add(new TempWorkpackagePush
                            {

                                workPackageInfoId = workPackageInfoObj.Id,
                                quizDetailid = quizDetails.Id,
                                companyObj = companyObj,
                                leadUserInfo = leadUserInfo,
                                shortUrl = shortUrl,
                                shotenUrlCode = shotenUrlCode,
                                quizType = quizObj.QuizType,
                                parentquizid = quizObj.Id,
                                WorkpackageCampaignName = workPackageInfoObj.CampaignName,
                                LeadUserId = workPackageInfoObj.LeadUserId,
                                ConfigurationDetailId = workPackageInfoObj.ConfigurationDetailsId.Value,
                                QuizDetails = quizDetails,
                                ConfigurationDetails = configurationDetailsobj,
                                UserToken = workPackageObj.UserToken,
                                ContactObject = exObj,
                                UsageType = usageType.HasValue ? usageType.Value : 1,
                                qLinkKey =  qlinkKey
                            });                        
                    }

                    if (tempWorkpackagePusheslist != null && tempWorkpackagePusheslist.Any())
                    {
                        foreach (var item in tempWorkpackagePusheslist)
                        {
                            WorkpackageCommunicationService.SendEmailSmsandWhatsappHandlerNew(item);                         
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

        //public void PushWorkPackageOLd(PushWorkPackage workPackageObj)
        //{

        //    PushWorkPackageValidator workPackageValidator = new PushWorkPackageValidator();
        //    Response.OWCLeadUserResponse.LeadUserResponse leadUserInfo = null;
        //    IEnumerable<Db.VariablesDetails> variablesDetailList = null; ;
        //    Model.Company companyObj =null;
        //    bool isSendEmail = false;
        //    int workPackageInfoObjId = 0;
        //    int quizDetailId = 0;
        //    var validationResult = workPackageValidator.Validate(workPackageObj);

        //    if (validationResult != null && validationResult.IsValid)
        //    {
        //        using (var UOWObj = new AutomationUnitOfWork())
        //        {
        //            try
        //            {
        //                var configurationDetailsList = UOWObj.ConfigurationDetailsRepository.Get(r => r.ConfigurationId == workPackageObj.ConfigurationId);

        //                if (configurationDetailsList != null && configurationDetailsList.Any())
        //                {
        //                    var configurationDetail = configurationDetailsList.FirstOrDefault();

        //                    if (configurationDetail != null)
        //                    {
        //                        var quizObj = UOWObj.QuizRepository.GetByID(configurationDetail.QuizId);

        //                        if (quizObj != null && quizObj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active) != null)
        //                        {
        //                            var uniqueCode = Guid.NewGuid().ToString();
        //                            var currentDate = DateTime.UtcNow;

        //                            var quizDetails = quizObj.QuizDetails.LastOrDefault(r => r.State == (int)QuizStateEnum.PUBLISHED);
        //                            quizDetailId = quizDetails.Id;
        //                            if (configurationDetail.IsUpdatedSend && (
        //                                (configurationDetail.SendEmail.GetValueOrDefault() == false || string.IsNullOrEmpty(configurationDetail.Body) || string.IsNullOrEmpty(configurationDetail.Subject)) &&
        //                                (configurationDetail.SendSms.GetValueOrDefault() == false || string.IsNullOrEmpty(configurationDetail.SMSText)) &&
        //                                (configurationDetail.SendWhatsApp.GetValueOrDefault() == false || configurationDetail.HsmTemplateId == null || configurationDetail.HsmTemplateId == 0)
        //                                ))
        //                            {
        //                                Status = ResultEnum.Error;
        //                                ErrorMessage = "Email Subject Body, Sms text or WhatsApp are required";
        //                                return;
        //                            }

        //                            if (quizDetails != null)
        //                            {

        //                                if (quizObj.Company != null)
        //                                {
        //                                    companyObj = new Model.Company
        //                                    {
        //                                        Id = quizObj.Company.Id,
        //                                        AlternateClientCodes = quizObj.Company.AlternateClientCodes,
        //                                        ClientCode = quizObj.Company.ClientCode,
        //                                        CompanyName = quizObj.Company.CompanyName,
        //                                        CompanyWebsiteUrl = quizObj.Company.CompanyWebsiteUrl,
        //                                        JobRocketApiAuthorizationBearer = quizObj.Company.JobRocketApiAuthorizationBearer,
        //                                        JobRocketApiUrl = quizObj.Company.JobRocketApiUrl,
        //                                        JobRocketClientUrl = quizObj.Company.JobRocketClientUrl,
        //                                        LeadDashboardApiAuthorizationBearer = quizObj.Company.LeadDashboardApiAuthorizationBearer,
        //                                        LeadDashboardApiUrl = quizObj.Company.LeadDashboardApiUrl,
        //                                        LeadDashboardClientUrl = quizObj.Company.LeadDashboardClientUrl,
        //                                        LogoUrl = quizObj.Company.LogoUrl,
        //                                        PrimaryBrandingColor = quizObj.Company.PrimaryBrandingColor,
        //                                        SecondaryBrandingColor = quizObj.Company.SecondaryBrandingColor

        //                                    };
        //                                }
        //                                else
        //                                {
        //                                    companyObj = new Company();
        //                                }

        //                                var remindersInQuizObj = UOWObj.RemindersInQuizRepository.Get(a => (string.IsNullOrEmpty(quizObj.AccessibleOfficeId) ? string.IsNullOrEmpty(a.OfficeId) : a.OfficeId == quizObj.AccessibleOfficeId) && a.CompanyId == quizObj.CompanyId).FirstOrDefault();


        //                                variablesDetailList = UOWObj.VariablesDetailsRepository.Get(r => r.VariableInQuiz.QuizId == configurationDetail.QuizId && r.ConfigurationDetailsId == configurationDetail.Id);

        //                                foreach (var LeadUserId in workPackageObj.ContactIds)
        //                                {

        //                                    leadUserInfo = OWCHelper.GetLeadUserInfo(LeadUserId, companyObj);

        //                                    if (leadUserInfo != null && !string.IsNullOrEmpty(leadUserInfo.contactId))
        //                                    {
        //                                        var workPackageInfoObj = new Db.WorkPackageInfo();

        //                                        using (var transaction = Utility.CreateTransactionScope())
        //                                        {
        //                                            #region insert in WorkPackageInfo

        //                                            workPackageInfoObj.LeadUserId = LeadUserId;
        //                                            workPackageInfoObj.QuizId = configurationDetail.QuizId;
        //                                            workPackageInfoObj.CampaignId = leadUserInfo.SourceId;
        //                                            workPackageInfoObj.CampaignName = leadUserInfo.SourceName;
        //                                            workPackageInfoObj.Status = (int)WorkPackageStatusEnum.Pending;
        //                                            workPackageInfoObj.CreatedOn = currentDate;
        //                                            workPackageInfoObj.ConfigurationDetailsId = configurationDetail.Id;

        //                                            UOWObj.WorkPackageInfoRepository.Insert(workPackageInfoObj);
        //                                            UOWObj.Save();
        //                                            workPackageInfoObjId = workPackageInfoObj.Id;
        //                                            #endregion

        //                                            #region insert in reminderQueues

        //                                            if (remindersInQuizObj != null)
        //                                            {
        //                                                var reminderQueuesObj = new Db.ReminderQueues();
        //                                                reminderQueuesObj.ReminderInQuizId = remindersInQuizObj.Id;
        //                                                reminderQueuesObj.Type = (int)ReminderTypeEnum.EMAIL;
        //                                                reminderQueuesObj.QueuedOn = currentDate;
        //                                                reminderQueuesObj.WorkPackageInfoId = workPackageInfoObj.Id;

        //                                                if (remindersInQuizObj.FirstReminder.HasValue)
        //                                                {
        //                                                    reminderQueuesObj.ReminderLevel = 1;
        //                                                    UOWObj.ReminderQueuesRepository.Insert(reminderQueuesObj);
        //                                                    UOWObj.Save();
        //                                                }

        //                                                if (remindersInQuizObj.SecondReminder.HasValue)
        //                                                {
        //                                                    reminderQueuesObj.ReminderLevel = 2;
        //                                                    UOWObj.ReminderQueuesRepository.Insert(reminderQueuesObj);
        //                                                    UOWObj.Save();
        //                                                }

        //                                                if (remindersInQuizObj.ThirdReminder.HasValue)
        //                                                {
        //                                                    reminderQueuesObj.ReminderLevel = 3;
        //                                                    UOWObj.ReminderQueuesRepository.Insert(reminderQueuesObj);
        //                                                    UOWObj.Save();
        //                                                }

        //                                                reminderQueuesObj.Type = (int)ReminderTypeEnum.SMS;

        //                                                if (remindersInQuizObj.FirstReminder.HasValue)
        //                                                {
        //                                                    reminderQueuesObj.ReminderLevel = 1;
        //                                                    UOWObj.ReminderQueuesRepository.Insert(reminderQueuesObj);
        //                                                    UOWObj.Save();
        //                                                }

        //                                                if (remindersInQuizObj.SecondReminder.HasValue)
        //                                                {
        //                                                    reminderQueuesObj.ReminderLevel = 2;
        //                                                    UOWObj.ReminderQueuesRepository.Insert(reminderQueuesObj);
        //                                                    UOWObj.Save();
        //                                                }

        //                                                if (remindersInQuizObj.ThirdReminder.HasValue)
        //                                                {
        //                                                    reminderQueuesObj.ReminderLevel = 3;
        //                                                    UOWObj.ReminderQueuesRepository.Insert(reminderQueuesObj);
        //                                                    UOWObj.Save();
        //                                                }
        //                                            }
        //                                            #endregion

        //                                            transaction.Complete();
        //                                        }

        //                                        isSendEmail = true;

        //                                    }
        //                                    else
        //                                    {
        //                                        Status = ResultEnum.Error;
        //                                        ErrorMessage = "Lead Info not found for Id " + LeadUserId.ToString();
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                Status = ResultEnum.Error;
        //                                ErrorMessage = "Quiz is not yet published.";
        //                            }
        //                        }
        //                        else
        //                        {
        //                            Status = ResultEnum.Error;
        //                            ErrorMessage = "Quiz does not exists for QuizId " + configurationDetail.QuizId;
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    SendAutomationWIthNewConfiguration(workPackageObj);
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                Status = ResultEnum.Error;
        //                ErrorMessage = ex.Message;
        //                throw ex;
        //            }
        //        }
        //        if (isSendEmail)
        //        {
        //            Logger.Log(LogLevel.Error, "SendEmailSmsandWhatsappHandler- " + workPackageInfoObjId + " Exception- Send mail head");
        //            SendEmailSmsandWhatsappHandlerNew(workPackageInfoObjId, quizDetailId, companyObj, leadUserInfo, variablesDetailList);
        //            Logger.Log(LogLevel.Error, "SendEmailSmsandWhatsappHandler- " + workPackageInfoObjId + " Exception- Send mail foot");
        //        }
        //    }
        //    else
        //    {
        //        Status = ResultEnum.Error;
        //        ErrorMessage = "Validation error: " + String.Join(", ", validationResult.Errors.Select(r => r.ErrorMessage));
        //    }
        //}

        private Db.ConfigurationDetails SaveConfigurationdetails(Response.ConfigurationDataDetails data)
        {


            var configurationDetailsObj = new Db.ConfigurationDetails()
            {
                ConfigurationId = data.ConfigurationId,
                QuizId = data.QuizId,
                IsUpdatedSend = data.IsUpdatedSend,
                Subject = data.Subject,
                Body = data.Body,
                SMSText = data.SmsText,
                SendEmail = data.SendEmail,
                SendSms = data.SendSms,
                SendWhatsApp = data.SendWhatsApp,
                SendFallbackSms = data.SendFallbackSms,
                SendMailNotRequired = data.SendMailNotRequired,
                CompanyCode = data.CompanyCode,
                ConfigurationType = data.ConfigurationType,
                PrivacyLink = data.PrivacyLink,
                Status = (int)StatusEnum.Active,
                SourceId = data.SourceId,
                SourceType = data.SourceType,
                SourceName = data.SourceTitle,
                LeadFormTitle = data.LeadFormTitle,
                CreatedOn = DateTime.UtcNow,
                HsmTemplateId = (data.WhatsApp != null && data.WhatsApp.HsmTemplateId > 0) ? data.WhatsApp.HsmTemplateId : default(int?),
                FollowUpMessage = data.WhatsApp != null ? data.WhatsApp.FollowUpMessage : string.Empty
            };
            using (var UOWObj = new AutomationUnitOfWork())
            {
                UOWObj.ConfigurationDetailsRepository.Insert(configurationDetailsObj);
                UOWObj.Save();

                return configurationDetailsObj;
            }

            return null;
        }

        private void SaveDynamicVariables(Dictionary<string, string> DynamicVariables, int quizDetailId, int configurationDetailId)
        {
            if (DynamicVariables != null)
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    foreach (var obj in DynamicVariables)
                    {
                        var VariablesObj = UOWObj.VariablesRepository.Get(r => r.VariableInQuiz.Any(s => s.QuizId == quizDetailId) && r.Name.ToLower() == obj.Key.ToLower()).FirstOrDefault();

                        if (VariablesObj != null)
                        {
                            var variablesDetailsObj = new Db.VariablesDetails();

                            variablesDetailsObj.VariableInQuizId = VariablesObj.VariableInQuiz.FirstOrDefault(r => r.QuizId == quizDetailId).Id;
                            variablesDetailsObj.VariableValue = obj.Value == null ? null : obj.Value.Trim();
                            variablesDetailsObj.ConfigurationDetailsId = configurationDetailId;

                            UOWObj.VariablesDetailsRepository.Insert(variablesDetailsObj);

                            UOWObj.Save();
                        }
                    }
                }
            }
        }

        private CompanyModel GetQuizCompany(Db.Company quizObjCompany)
        {
            CompanyModel companyObj = null;
            if (quizObjCompany != null)
            {
                companyObj = new CompanyModel
                {
                    Id = quizObjCompany.Id,
                    AlternateClientCodes = quizObjCompany.AlternateClientCodes,
                    ClientCode = quizObjCompany.ClientCode,
                    CompanyName = quizObjCompany.CompanyName,
                    CompanyWebsiteUrl = quizObjCompany.CompanyWebsiteUrl,
                    JobRocketApiAuthorizationBearer = quizObjCompany.JobRocketApiAuthorizationBearer,
                    JobRocketApiUrl = quizObjCompany.JobRocketApiUrl,
                    JobRocketClientUrl = quizObjCompany.JobRocketClientUrl,
                    LeadDashboardApiAuthorizationBearer = quizObjCompany.LeadDashboardApiAuthorizationBearer,
                    LeadDashboardApiUrl = quizObjCompany.LeadDashboardApiUrl,
                    LeadDashboardClientUrl = quizObjCompany.LeadDashboardClientUrl,
                    LogoUrl = quizObjCompany.LogoUrl,
                    PrimaryBrandingColor = quizObjCompany.PrimaryBrandingColor,
                    SecondaryBrandingColor = quizObjCompany.SecondaryBrandingColor

                };
            }
            else
            {
                companyObj = new CompanyModel();
            }

            return companyObj;
        }

        private void SaveWhatsApp(Response.ConfigurationDataDetails.WhatsAppDetails WhatsApp, int configurationDetailId)
        {
            using (var UOWObj = new AutomationUnitOfWork())
            {
                if (WhatsApp != null && WhatsApp.TemplateParameters != null)
                {
                    foreach (var item in WhatsApp.TemplateParameters)
                    {
                        var templateParameterInConfigurationDetails = new Db.TemplateParameterInConfigurationDetails()
                        {
                            ConfigurationDetailsId = configurationDetailId,
                            ParaName = item.Paraname,
                            Position = item.Position,
                            Value = item.Value
                        };

                        UOWObj.TemplateParameterInConfigurationDetailsRepository.Insert(templateParameterInConfigurationDetails);
                    }

                    UOWObj.Save();
                }
            }
        }
        private void SaveConfigurationLeadDataAction(List<LeadDataInActionResponse> leadDataInActions, ICollection<Db.QuizComponentLogs> quizComponentLogs, ICollection<Db.ActionsInQuiz> actionsInQuizzes, int configurationDetailId)
        {
            if (leadDataInActions != null)
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    foreach (var obj in leadDataInActions.Where(r => r.ActionId > 0 || r.ParentId > 0))
                    {
                        int actionId = 0;

                        if (obj.ParentId > 0)
                        {
                            actionId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ParentId && r.ObjectTypeId == (int)BranchingLogicEnum.ACTION).PublishedObjectId;
                        }
                        else
                        {
                            int parentActionId = quizComponentLogs.LastOrDefault(r => r.PublishedObjectId == obj.ActionId && r.ObjectTypeId == (int)BranchingLogicEnum.ACTION).DraftedObjectId;
                            actionId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == parentActionId && r.ObjectTypeId == (int)BranchingLogicEnum.ACTION).PublishedObjectId;
                        }

                        var actionsInQuiz = actionsInQuizzes.Where(t => t.Id == actionId).FirstOrDefault();

                        UpdateleadDataInActionValidator emailListValidator = new UpdateleadDataInActionValidator();
                        var validationEmailResult = emailListValidator.Validate(obj.ReportEmails.Split(',').Select(t => t.Trim()).ToList());

                        if (actionsInQuiz != null && (((actionsInQuiz.ActionType == (int)ActionTypeEnum.LinkWithLeadDashboardAppointment || actionsInQuiz.ActionType == (int)ActionTypeEnum.Appointment) && obj.AppointmentTypeId > 0) || ((actionsInQuiz.ActionType == (int)ActionTypeEnum.LinkWithLeadDashboardAppointment || actionsInQuiz.ActionType == (int)ActionTypeEnum.ReportEmail) && !string.IsNullOrEmpty(obj.ReportEmails) && validationEmailResult != null && validationEmailResult.IsValid)))
                        {
                            var leadDataInActionObj = new Db.LeadDataInAction();

                            leadDataInActionObj.ActionId = actionId;
                            leadDataInActionObj.AppointmentTypeId = obj.AppointmentTypeId > 0 ? obj.AppointmentTypeId : default(int?);
                            leadDataInActionObj.ReportEmails = !string.IsNullOrEmpty(obj.ReportEmails) ? obj.ReportEmails : null;
                            if (obj.IsUpdatedSend && !obj.SendMailNotRequired && !string.IsNullOrEmpty(obj.Subject) && !string.IsNullOrEmpty(obj.Body) && !string.IsNullOrEmpty(obj.SMSText))
                            {
                                leadDataInActionObj.IsUpdatedSend = obj.IsUpdatedSend;
                                leadDataInActionObj.Subject = obj.Subject;
                                leadDataInActionObj.Body = obj.Body;
                                leadDataInActionObj.SMSText = obj.SMSText;
                            }
                            leadDataInActionObj.ConfigurationDetailsId = configurationDetailId;


                            if (obj.CalendarIds != null)
                            {
                                foreach (var calendarId in obj.CalendarIds.ToList())
                                {
                                    var leadCalendarDataInActionObj = new Db.LeadCalendarDataInAction();
                                    leadCalendarDataInActionObj.LeadDataInActionId = leadDataInActionObj.Id;
                                    leadCalendarDataInActionObj.CalendarId = calendarId;
                                    UOWObj.LeadCalendarDataInActionRepository.Insert(leadCalendarDataInActionObj);
                                }
                            }

                            UOWObj.LeadDataInActionRepository.Insert(leadDataInActionObj);
                            UOWObj.Save();
                        }
                    }
                }
            }
        }

        private void SaveDynamicMediaVariables(MediaVariableDetail mediaVariableDetails, ICollection<Db.QuizComponentLogs> quizComponentLogs, int configurationDetailId, int quizDetailId, bool quesAndContentInSameTable)
        {

            #region insert in MediaVariables

            if (mediaVariableDetails != null)
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    if (mediaVariableDetails.Questions != null)
                    {
                        foreach (var obj in mediaVariableDetails.Questions)
                        {
                            var MediaObj = new Db.MediaVariablesDetails();

                            MediaObj.ObjectId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ParentId && r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION).PublishedObjectId;
                            MediaObj.ObjectTypeId = (int)BranchingLogicEnum.QUESTION;
                            MediaObj.ObjectValue = obj.MediaUrlValue;
                            MediaObj.ObjectPublicId = obj.PublicId;
                            MediaObj.QuizId = quizDetailId;
                            MediaObj.ConfigurationDetailsId = configurationDetailId;
                            MediaObj.Type = (int)ImageTypeEnum.Title;
                            MediaObj.MediaOwner = obj.MediaOwner;
                            MediaObj.ProfileMedia = obj.ProfileMedia;

                            UOWObj.MediaVariablesDetailsRepository.Insert(MediaObj);

                            UOWObj.Save();

                            var MediaforDescriptionObj = new Db.MediaVariablesDetails();

                            MediaforDescriptionObj.ObjectId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ParentId && r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION).PublishedObjectId;
                            MediaforDescriptionObj.ObjectTypeId = (int)BranchingLogicEnum.QUESTION;
                            MediaforDescriptionObj.ObjectValue = obj.MediaUrlforDescriptionValue;
                            MediaforDescriptionObj.ObjectPublicId = obj.PublicIdforDescription;
                            MediaforDescriptionObj.QuizId = quizDetailId;
                            MediaforDescriptionObj.ConfigurationDetailsId = configurationDetailId;
                            MediaforDescriptionObj.Type = (int)ImageTypeEnum.Description;
                            MediaforDescriptionObj.MediaOwner = obj.MediaOwnerforDescription;
                            MediaforDescriptionObj.ProfileMedia = obj.ProfileMediaforDescription;

                            UOWObj.MediaVariablesDetailsRepository.Insert(MediaforDescriptionObj);

                            UOWObj.Save();
                        }
                    }

                    if (mediaVariableDetails.Answers != null)
                    {
                        foreach (var obj in mediaVariableDetails.Answers)
                        {
                            var MediaObj = new Db.MediaVariablesDetails();

                            MediaObj.ObjectId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ParentId && r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER).PublishedObjectId;
                            MediaObj.ObjectTypeId = (int)BranchingLogicEnum.ANSWER;
                            MediaObj.ObjectValue = obj.MediaUrlValue;
                            MediaObj.ObjectPublicId = obj.PublicId;
                            MediaObj.QuizId = quizDetailId;
                            MediaObj.ConfigurationDetailsId = configurationDetailId;
                            MediaObj.Type = (int)ImageTypeEnum.Title;
                            MediaObj.MediaOwner = obj.MediaOwner;
                            MediaObj.ProfileMedia = obj.ProfileMedia;

                            UOWObj.MediaVariablesDetailsRepository.Insert(MediaObj);

                            UOWObj.Save();
                        }
                    }

                    if (mediaVariableDetails.Results != null)
                    {
                        foreach (var obj in mediaVariableDetails.Results)
                        {
                            var MediaObj = new Db.MediaVariablesDetails();

                            MediaObj.ObjectId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ParentId && r.ObjectTypeId == (int)BranchingLogicEnum.RESULT).PublishedObjectId;
                            MediaObj.ObjectTypeId = (int)BranchingLogicEnum.RESULT;
                            MediaObj.ObjectValue = obj.MediaUrlValue;
                            MediaObj.ObjectPublicId = obj.PublicId;
                            MediaObj.QuizId = quizDetailId;
                            MediaObj.ConfigurationDetailsId = configurationDetailId;
                            MediaObj.Type = (int)ImageTypeEnum.Title;
                            MediaObj.MediaOwner = obj.MediaOwner;
                            MediaObj.ProfileMedia = obj.ProfileMedia;

                            UOWObj.MediaVariablesDetailsRepository.Insert(MediaObj);

                            UOWObj.Save();
                        }
                    }

                    if (mediaVariableDetails.CoverDetails != null)
                    {
                        foreach (var obj in mediaVariableDetails.CoverDetails)
                        {
                            var MediaObj = new Db.MediaVariablesDetails();

                            MediaObj.ObjectId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ParentId && r.ObjectTypeId == (int)BranchingLogicEnum.COVERDETAILS).PublishedObjectId;
                            MediaObj.ObjectTypeId = (int)BranchingLogicEnum.COVERDETAILS;
                            MediaObj.ObjectValue = obj.MediaUrlValue;
                            MediaObj.ObjectPublicId = obj.PublicId;
                            MediaObj.QuizId = quizDetailId;
                            MediaObj.ConfigurationDetailsId = configurationDetailId;
                            MediaObj.Type = (int)ImageTypeEnum.Title;
                            MediaObj.MediaOwner = obj.MediaOwner;
                            MediaObj.ProfileMedia = obj.ProfileMedia;

                            UOWObj.MediaVariablesDetailsRepository.Insert(MediaObj);

                            UOWObj.Save();
                        }
                    }

                    if (mediaVariableDetails.Badges != null)
                    {
                        foreach (var obj in mediaVariableDetails.Badges)
                        {
                            var MediaObj = new Db.MediaVariablesDetails();

                            MediaObj.ObjectId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ParentId && r.ObjectTypeId == (int)BranchingLogicEnum.BADGE).PublishedObjectId;
                            MediaObj.ObjectTypeId = (int)BranchingLogicEnum.BADGE;
                            MediaObj.ObjectValue = obj.MediaUrlValue;
                            MediaObj.ObjectPublicId = obj.PublicId;
                            MediaObj.QuizId = quizDetailId;
                            MediaObj.ConfigurationDetailsId = configurationDetailId;
                            MediaObj.Type = (int)ImageTypeEnum.Title;
                            MediaObj.MediaOwner = obj.MediaOwner;
                            MediaObj.ProfileMedia = obj.ProfileMedia;

                            UOWObj.MediaVariablesDetailsRepository.Insert(MediaObj);

                            UOWObj.Save();
                        }
                    }

                    if (mediaVariableDetails.Content != null)
                    {
                        foreach (var obj in mediaVariableDetails.Content)
                        {
                            var MediaObj = new Db.MediaVariablesDetails();

                            MediaObj.ObjectId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ParentId && (quesAndContentInSameTable ? r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION : r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT)).PublishedObjectId;
                            MediaObj.ObjectTypeId = (int)BranchingLogicEnum.CONTENT;
                            MediaObj.ObjectValue = obj.MediaUrlValue;
                            MediaObj.ObjectPublicId = obj.PublicId;
                            MediaObj.QuizId = quizDetailId;
                            MediaObj.ConfigurationDetailsId = configurationDetailId;
                            MediaObj.Type = (int)ImageTypeEnum.Title;
                            MediaObj.MediaOwner = obj.MediaOwner;
                            MediaObj.ProfileMedia = obj.ProfileMedia;

                            UOWObj.MediaVariablesDetailsRepository.Insert(MediaObj);

                            UOWObj.Save();

                            var MediaforDescriptionObj = new Db.MediaVariablesDetails();

                            MediaforDescriptionObj.ObjectId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ParentId && (quesAndContentInSameTable ? r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION : r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT)).PublishedObjectId;
                            MediaforDescriptionObj.ObjectTypeId = (int)BranchingLogicEnum.CONTENT;
                            MediaforDescriptionObj.ObjectValue = obj.MediaUrlforDescriptionValue;
                            MediaforDescriptionObj.ObjectPublicId = obj.PublicIdforDescription;
                            MediaforDescriptionObj.QuizId = quizDetailId;
                            MediaforDescriptionObj.ConfigurationDetailsId = configurationDetailId;
                            MediaforDescriptionObj.Type = (int)ImageTypeEnum.Description;
                            MediaforDescriptionObj.MediaOwner = obj.MediaOwnerforDescription;
                            MediaforDescriptionObj.ProfileMedia = obj.ProfileMediaforDescription;

                            UOWObj.MediaVariablesDetailsRepository.Insert(MediaforDescriptionObj);

                            UOWObj.Save();
                        }
                    }
                }
            }

            #endregion

        }

        private void SaveconfigurationAttachment(List<EmailAttachment> emailAttachments, int configurationDetailId)
        {
            if (emailAttachments != null)
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    foreach (var obj in emailAttachments)
                    {
                        var attachmentsInConfigurationObj = new Db.AttachmentsInConfiguration();

                        attachmentsInConfigurationObj.FileName = obj.FileName;
                        attachmentsInConfigurationObj.FileIdentifier = obj.FileIdentifier;
                        attachmentsInConfigurationObj.FileLink = obj.FileLink;
                        attachmentsInConfigurationObj.ConfigurationDetailsId = configurationDetailId;

                        UOWObj.AttachmentsInConfigurationRepository.Insert(attachmentsInConfigurationObj);

                        UOWObj.Save();
                    }
                }
            }


        }

        private Db.ConfigurationDetails SaveAutomationconfigurationDetails(PushWorkPackage workPackageObj)
        {
            Response.OWCLeadUserResponse.LeadUserResponse leadUserInfo = null;
            IEnumerable<Db.VariablesDetails> variablesDetailList = null; ;
            CompanyModel companyObj = null;
            bool isSendEmail = false;
            bool quesAndContentInSameTable = false;
            int workPackageInfoObjId = 0;
            int quizDetailId = 0;
            Db.QuizDetails quizDetails = null;
            Db.Quiz quizObj = null;
            ICollection<Db.QuizComponentLogs> quizComponentLogs = null;
            ICollection<Db.ActionsInQuiz> quizActionsInQuiz = null;
            Db.ConfigurationDetails savedConfigurationDetails = null;
            var configurationDetail = OWCHelper.GetConfigurationDetails(workPackageObj.ConfigurationId, workPackageObj.CompanyCode);
            if (configurationDetail != null && configurationDetail.Data != null)
            {
                if (configurationDetail.Data.IsUpdatedSend && (
            (configurationDetail.Data.SendEmail.GetValueOrDefault() == false || string.IsNullOrEmpty(configurationDetail.Data.Body) || string.IsNullOrEmpty(configurationDetail.Data.Subject)) &&
            (configurationDetail.Data.SendSms.GetValueOrDefault() == false || string.IsNullOrEmpty(configurationDetail.Data.SmsText)) &&
            (configurationDetail.Data.SendWhatsApp.GetValueOrDefault() == false || configurationDetail.Data.WhatsApp == null || configurationDetail.Data.WhatsApp.HsmTemplateId == 0)
            ))
                {
                    Status = ResultEnum.Error;
                    ErrorMessage = "Email Subject Body, Sms text or WhatsApp are required";
                    return null;
                }
            }

            if (configurationDetail == null)
            {
                Status = ResultEnum.Error;
                ErrorMessage = "configurationDetail does not exists for ConfigurationId " + workPackageObj.ConfigurationId;
                return null;
            }

            savedConfigurationDetails = SaveConfigurationdetails(configurationDetail.Data);
            using (var UOWObj = new AutomationUnitOfWork())
            {
                quizObj = UOWObj.QuizRepository.GetByID(configurationDetail.Data.QuizId);

                if (!(quizObj != null && quizObj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active) != null))
                {
                    Status = ResultEnum.Error;
                    ErrorMessage = "Quiz does not exists for QuizId " + configurationDetail.Data.QuizId;
                    return null;
                }

                quizDetails = quizObj.QuizDetails.LastOrDefault(r => r.State == (int)QuizStateEnum.PUBLISHED);
                if (quizDetails == null)
                {
                    Status = ResultEnum.Error;
                    ErrorMessage = "Quiz is not yet published.";
                    return null;
                }
                quesAndContentInSameTable = quizObj.QuesAndContentInSameTable;
                companyObj = GetQuizCompany(quizObj.Company);
                quizDetailId = quizDetails.Id;
                quizComponentLogs = quizDetails.QuizComponentLogs;
                quizActionsInQuiz = quizDetails.ActionsInQuiz;
            }



            SaveWhatsApp(configurationDetail.Data.WhatsApp, savedConfigurationDetails.Id);
            SaveDynamicVariables(configurationDetail.Data.DynamicVariables, quizDetailId, savedConfigurationDetails.Id);
            SaveConfigurationLeadDataAction(configurationDetail.Data.LeadDataInActionList, quizComponentLogs, quizActionsInQuiz, savedConfigurationDetails.Id);
            SaveDynamicMediaVariables(configurationDetail.Data.MediaVariableDetails, quizComponentLogs, savedConfigurationDetails.Id, quizDetailId, quesAndContentInSameTable);
            SaveconfigurationAttachment(configurationDetail.Data.EmailAttachments, savedConfigurationDetails.Id);

            return savedConfigurationDetails;
        }



        //private void SendAutomationWIthNewConfiguration(PushWorkPackage workPackageObj)
        //{
        //    Response.OWCLeadUserResponse.LeadUserResponse leadUserInfo = null;
        //    IEnumerable<Db.VariablesDetails> variablesDetailList = null; ;
        //    Model.Company companyObj = null;
        //    bool isSendEmail = false;
        //    bool quesAndContentInSameTable = false;
        //    int workPackageInfoObjId = 0;
        //    int quizDetailId = 0;
        //    Db.QuizDetails quizDetails = null;
        //    Db.Quiz quizObj = null;
        //    ICollection<Db.QuizComponentLogs> quizComponentLogs = null;
        //    ICollection<Db.ActionsInQuiz> quizActionsInQuiz = null;

        //    var configurationDetail = OWCHelper.GetConfigurationDetails(workPackageObj.ConfigurationId, workPackageObj.CompanyCode);
        //    if (configurationDetail != null && configurationDetail.Data != null)
        //    {
        //        if (configurationDetail.Data.IsUpdatedSend && (
        //    (configurationDetail.Data.SendEmail.GetValueOrDefault() == false || string.IsNullOrEmpty(configurationDetail.Data.Body) || string.IsNullOrEmpty(configurationDetail.Data.Subject)) &&
        //    (configurationDetail.Data.SendSms.GetValueOrDefault() == false || string.IsNullOrEmpty(configurationDetail.Data.SmsText)) &&
        //    (configurationDetail.Data.SendWhatsApp.GetValueOrDefault() == false || configurationDetail.Data.WhatsApp == null || configurationDetail.Data.WhatsApp.HsmTemplateId == 0)
        //    ))
        //        {
        //            Status = ResultEnum.Error;
        //            ErrorMessage = "Email Subject Body, Sms text or WhatsApp are required";
        //            return;
        //        }
        //    }

        //    if (configurationDetail == null)
        //    {
        //        Status = ResultEnum.Error;
        //        ErrorMessage = "configurationDetail does not exists for ConfigurationId " + workPackageObj.ConfigurationId;
        //        return;
        //    }

        //    var savedConfigurationDetails = SaveConfigurationdetails(configurationDetail.Data);

        //    using (var UOWObj = new AutomationUnitOfWork())
        //    {
        //        quizObj = UOWObj.QuizRepository.GetByID(configurationDetail.Data.QuizId);

        //        if (!(quizObj != null && quizObj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active) != null))
        //        {
        //            Status = ResultEnum.Error;
        //            ErrorMessage = "Quiz does not exists for QuizId " + configurationDetail.Data.QuizId;
        //            return;
        //        }

        //        quizDetails = quizObj.QuizDetails.LastOrDefault(r => r.State == (int)QuizStateEnum.PUBLISHED);
        //        if (quizDetails == null)
        //        {
        //            Status = ResultEnum.Error;
        //            ErrorMessage = "Quiz is not yet published.";
        //            return;
        //        }
        //        quesAndContentInSameTable = quizObj.QuesAndContentInSameTable;
        //        companyObj = GetQuizCompany(quizObj.Company);
        //        quizDetailId = quizDetails.Id;
        //        quizComponentLogs = quizDetails.QuizComponentLogs;
        //        quizActionsInQuiz = quizDetails.ActionsInQuiz;
        //    }



        //    SaveWhatsApp(configurationDetail.Data.WhatsApp, savedConfigurationDetails.Id);
        //    SaveDynamicVariables(configurationDetail.Data.DynamicVariables, quizDetailId, savedConfigurationDetails.Id);
        //    SaveConfigurationLeadDataAction(configurationDetail.Data.LeadDataInActionList, quizComponentLogs, quizActionsInQuiz, savedConfigurationDetails.Id);
        //    SaveDynamicMediaVariables(configurationDetail.Data.MediaVariableDetails, quizComponentLogs, savedConfigurationDetails.Id, quizDetailId, quesAndContentInSameTable);
        //    SaveconfigurationAttachment(configurationDetail.Data.EmailAttachments, savedConfigurationDetails.Id);

        //    var uniqueCode = Guid.NewGuid().ToString();
        //    var currentDate = DateTime.UtcNow;



        //    using (var UOWObj = new AutomationUnitOfWork())
        //    {
        //        var remindersInQuizObj = UOWObj.RemindersInQuizRepository.Get(a => (string.IsNullOrEmpty(quizObj.AccessibleOfficeId) ? string.IsNullOrEmpty(a.OfficeId) : a.OfficeId == quizObj.AccessibleOfficeId) && a.CompanyId == quizObj.CompanyId).FirstOrDefault();

        //        variablesDetailList = UOWObj.VariablesDetailsRepository.Get(r => r.VariableInQuiz.QuizId == configurationDetail.Data.QuizId && r.ConfigurationDetailsId == savedConfigurationDetails.Id);

        //        foreach (var LeadUserId in workPackageObj.ContactIds)
        //        {
        //            leadUserInfo = OWCHelper.GetLeadUserInfo(LeadUserId, companyObj);
        //            if (!string.IsNullOrWhiteSpace(leadUserInfo?.contactId))
        //            {
        //                Status = ResultEnum.Error;
        //                ErrorMessage = "Lead Info not found for Id " + LeadUserId.ToString();
        //            }

        //            var workPackageInfoObj = new Db.WorkPackageInfo();

        //            using (var transaction = Utility.CreateTransactionScope())
        //            {
        //                #region insert in WorkPackageInfo

        //                workPackageInfoObj.LeadUserId = LeadUserId;
        //                workPackageInfoObj.QuizId = configurationDetail.Data.QuizId;
        //                workPackageInfoObj.CampaignId = leadUserInfo.SourceId;
        //                workPackageInfoObj.CampaignName = leadUserInfo.SourceName;
        //                workPackageInfoObj.Status = (int)WorkPackageStatusEnum.Pending;
        //                workPackageInfoObj.CreatedOn = currentDate;
        //                workPackageInfoObj.ConfigurationDetailsId = savedConfigurationDetails.Id;

        //                UOWObj.WorkPackageInfoRepository.Insert(workPackageInfoObj);
        //                UOWObj.Save();
        //                #endregion
        //                #region insert in reminderQueues

        //                if (remindersInQuizObj != null)
        //                {
        //                    var reminderQueuesObj = new Db.ReminderQueues();
        //                    reminderQueuesObj.ReminderInQuizId = remindersInQuizObj.Id;
        //                    reminderQueuesObj.Type = (int)ReminderTypeEnum.EMAIL;
        //                    reminderQueuesObj.QueuedOn = currentDate;
        //                    reminderQueuesObj.WorkPackageInfoId = workPackageInfoObj.Id;

        //                    if (remindersInQuizObj.FirstReminder.HasValue)
        //                    {
        //                        reminderQueuesObj.ReminderLevel = 1;
        //                        UOWObj.ReminderQueuesRepository.Insert(reminderQueuesObj);
        //                        UOWObj.Save();
        //                    }

        //                    if (remindersInQuizObj.SecondReminder.HasValue)
        //                    {
        //                        reminderQueuesObj.ReminderLevel = 2;
        //                        UOWObj.ReminderQueuesRepository.Insert(reminderQueuesObj);
        //                        UOWObj.Save();
        //                    }

        //                    if (remindersInQuizObj.ThirdReminder.HasValue)
        //                    {
        //                        reminderQueuesObj.ReminderLevel = 3;
        //                        UOWObj.ReminderQueuesRepository.Insert(reminderQueuesObj);
        //                        UOWObj.Save();
        //                    }

        //                    reminderQueuesObj.Type = (int)ReminderTypeEnum.SMS;

        //                    if (remindersInQuizObj.FirstReminder.HasValue)
        //                    {
        //                        reminderQueuesObj.ReminderLevel = 1;
        //                        UOWObj.ReminderQueuesRepository.Insert(reminderQueuesObj);
        //                        UOWObj.Save();
        //                    }

        //                    if (remindersInQuizObj.SecondReminder.HasValue)
        //                    {
        //                        reminderQueuesObj.ReminderLevel = 2;
        //                        UOWObj.ReminderQueuesRepository.Insert(reminderQueuesObj);
        //                        UOWObj.Save();
        //                    }

        //                    if (remindersInQuizObj.ThirdReminder.HasValue)
        //                    {
        //                        reminderQueuesObj.ReminderLevel = 3;
        //                        UOWObj.ReminderQueuesRepository.Insert(reminderQueuesObj);
        //                        UOWObj.Save();
        //                    }




        //                }
        //                #endregion
        //                transaction.Complete();
        //            }
        //                isSendEmail = true;
        //                SendEmailSmsandWhatsappHandlerNew(workPackageInfoObj.Id, quizDetails.Id, companyObj, leadUserInfo);
        //        }
        //    }
        //}


        public AutomationDetails GetSearchAndSuggestion(List<string> OfficeIdList, bool IncludeSharedWithMe, string SearchTxt, CompanyModel CompanyInfo, bool IsDataforGlobalOfficeAdmin, bool? IsPublished, int? UsageType, bool? IsWhatsAppChatBotOldVersion)
        {
            var AutomationDetails = new AutomationDetails();

            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    AutomationDetails.Automations = new List<Automation>();
                    AutomationDetails.Tag = new List<Tags>();
                    AutomationDetails.AutomationTypes = new List<AutomationType>();

                    //var automationTagsList = OWCHelper.GetAutomationTagsList(CompanyInfo.ClientCode);
                    var automationTagsList = CommonStaticData.GetCachedAutomationTagsList(CompanyInfo.ClientCode);

                    if (!(string.IsNullOrEmpty(SearchTxt)))
                    {
                        SearchTxt = SearchTxt.ToUpper();

                        if (automationTagsList != null)
                        {
                            foreach (var item in automationTagsList.Where(r => r.tagName.IndexOf(SearchTxt, StringComparison.OrdinalIgnoreCase) > -1))
                            {
                                AutomationDetails.Tag.Add(new Tags() { TagId = item.tagId, TagName = item.tagName, TagCode = item.tagCode });
                            }
                        }

                        if (QuizTypeEnum.Assessment.ToString().IndexOf(SearchTxt, StringComparison.OrdinalIgnoreCase) > -1)
                            AutomationDetails.AutomationTypes.Add(new AutomationType()
                            {
                                AutomationTypeId = (int)QuizTypeEnum.Assessment,
                                AutomationTypeName = QuizTypeEnum.Assessment.ToString()
                            });

                        if (QuizTypeEnum.Score.ToString().IndexOf(SearchTxt, StringComparison.OrdinalIgnoreCase) > -1)
                            AutomationDetails.AutomationTypes.Add(new AutomationType()
                            {
                                AutomationTypeId = (int)QuizTypeEnum.Score,
                                AutomationTypeName = QuizTypeEnum.Score.ToString()
                            });

                        if (QuizTypeEnum.Personality.ToString().IndexOf(SearchTxt, StringComparison.OrdinalIgnoreCase) > -1)
                            AutomationDetails.AutomationTypes.Add(new AutomationType()
                            {
                                AutomationTypeId = (int)QuizTypeEnum.Personality,
                                AutomationTypeName = QuizTypeEnum.Personality.ToString()
                            });

                        if (QuizTypeEnum.NPS.ToString().IndexOf(SearchTxt, StringComparison.OrdinalIgnoreCase) > -1)
                            AutomationDetails.AutomationTypes.Add(new AutomationType()
                            {
                                AutomationTypeId = (int)QuizTypeEnum.NPS,
                                AutomationTypeName = QuizTypeEnum.NPS.ToString()
                            });
                    }


                    var AutomationList = UOWObj.QuizRepository
                                  .Get(r => (string.IsNullOrEmpty(SearchTxt) ? false : (string.IsNullOrEmpty(r.QuizDetails.FirstOrDefault(t => t.State == (int)QuizStateEnum.DRAFTED).QuizTitle) ? false : r.QuizDetails.FirstOrDefault(t => t.State == (int)QuizStateEnum.DRAFTED).QuizTitle.ToUpper().IndexOf(SearchTxt) > -1))
                                         && (r.Company.Id == CompanyInfo.Id)
                                         && (IsWhatsAppChatBotOldVersion.HasValue ? IsWhatsAppChatBotOldVersion.Value == true ? r.IsWhatsAppChatBotOldVersion == true : r.IsWhatsAppChatBotOldVersion != true : true)
                                         && (IsPublished.HasValue ? (IsPublished.Value ? (r.State == (int)QuizStateEnum.PUBLISHED) : (r.State != (int)QuizStateEnum.PUBLISHED)) : true)
                                         && (UsageType.HasValue ? r.UsageTypeInQuiz.Any(u => u.UsageType == UsageType.Value) : true)
                                         && (r.QuizDetails.FirstOrDefault(s => s.State == (int)QuizStateEnum.DRAFTED && s.Status == (int)StatusEnum.Active) != null)
                                         && (!string.IsNullOrEmpty(r.AccessibleOfficeId) ? ((IsDataforGlobalOfficeAdmin && !OfficeIdList.Any()) ? true : OfficeIdList.Contains(r.AccessibleOfficeId))
                                                                           : (IncludeSharedWithMe)),
                                   includeProperties: "QuizDetails, QuizTagDetails");

                    if (AutomationList.Any())
                    {
                        foreach (var Quiz in AutomationList)
                        {
                            var quizDetailsObj = Quiz.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active);
                            if (quizDetailsObj != null)
                            {
                                if (string.IsNullOrEmpty(Quiz.QuizDetails.FirstOrDefault(t => t.State == (int)QuizStateEnum.DRAFTED).QuizTitle) ? false : Quiz.QuizDetails.FirstOrDefault(t => t.State == (int)QuizStateEnum.DRAFTED).QuizTitle.ToUpper().IndexOf(SearchTxt, StringComparison.OrdinalIgnoreCase) > -1)
                                {
                                    AutomationDetails.Automations.Add(new Automation()
                                    {
                                        QuizId = Quiz.Id,
                                        QuizTitle = quizDetailsObj.QuizTitle
                                    });
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

            return AutomationDetails;
        }

        public static async Task RefereshCacheHandler(int? FilterId = null, int? Type = null)
        {
            await Task.Run(() =>
            {
                RefereshCache(FilterId, Type);
            });
        }

        static void RefereshCache(int? FilterId, int? Type)
        {
            using (var UOWObj = new AutomationUnitOfWork())
            {
                try
                {
                    #region Automation - GetList API

                    if (!Type.HasValue || Type.Value == (int)ListTypeEnum.Automation)
                    {

                        AppLocalCache.Remove("AutomationList_CompanyId_" + FilterId);
                        var result = AppLocalCache.GetOrCache("AutomationList_CompanyId_" + FilterId, () =>
                        {
                            return UOWObj.QuizRepository.Get(filter: a => a.CompanyId == FilterId
                        && (a.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active)) != null
                        , includeProperties: "QuizDetails, QuizDetails.QuestionsInQuiz, QuizTagDetails, FavoriteQuizByUser");
                        });
                    }

                    #endregion

                    #region Automation -  v2 GetList API

                    if (!Type.HasValue || Type.Value == (int)ListTypeEnum.AutomationforHub)
                    {

                        AppLocalCache.Remove("AutomationforHubList_CompanyId_" + FilterId);
                        var result = AppLocalCache.GetOrCache("AutomationforHubList_CompanyId_" + FilterId, () =>
                        {
                            return UOWObj.QuizRepository.Get(filter: r => r.CompanyId == FilterId
                            && (r.UserPermissionsInQuiz.Any() ? r.UserPermissionsInQuiz.Any(p => (p.UserTypeId == (int)UserTypeEnum.Lead)) : false)
                            && (r.QuizDetails.FirstOrDefault(s => s.State == (int)QuizStateEnum.DRAFTED && s.Status == (int)StatusEnum.Active) != null),
                            includeProperties: "QuizDetails, QuizDetails.QuestionsInQuiz, QuizTagDetails, FavoriteQuizByUser, Company, UsageTypeInQuiz");
                        });
                    }

                    #endregion

                    #region TechnicalRecruite - TechnicalRecruiterList API

                    if (!Type.HasValue || Type.Value == (int)ListTypeEnum.TechnicalRecruiter)
                    {
                        AppLocalCache.Remove("TechnicalRecruiterList");

                        var result = AppLocalCache.GetOrCache("TechnicalRecruiterList", () =>
                        {
                            return UOWObj.QuizRepository.Get(filter: a => a.QuizDetails.FirstOrDefault().State == (int)QuizStateEnum.DRAFTED
                               && a.QuizDetails.FirstOrDefault().Status == (int)StatusEnum.Active
                               && a.UserPermissionsInQuiz.Any(r => r.UserTypeId == (int)UserTypeEnum.TechnicalRecruiter)
                               && a.ModulePermissionsInQuiz.Any(),
                               includeProperties: "QuizDetails.QuizAttempts.QuizStats, ModulePermissionsInQuiz");
                        });
                    }

                    #endregion

                    #region Elearning - GetDashboardData API

                    if (!Type.HasValue || Type.Value == (int)ListTypeEnum.Elearning)
                    {

                        AppLocalCache.Remove("ElearningList_CompanyId_" + FilterId);
                        var result = AppLocalCache.GetOrCache("ElearningList_CompanyId_" + FilterId, () =>
                        {
                            return UOWObj.QuizRepository.Get(filter: a => a.CompanyId == FilterId && a.UserPermissionsInQuiz.Any(r => r.UserTypeId == (int)UserTypeEnum.Recruiter),
                            includeProperties: "QuizDetails, AttachmentsInQuiz, QuizTagDetails, UserPermissionsInQuiz, Company");
                        });
                    }

                    #endregion

                    #region JobRockAcademy - GetDashboardData API

                    if (!Type.HasValue || Type.Value == (int)ListTypeEnum.JobRockAcademy)
                    {
                        AppLocalCache.Remove("JobRockAcademyList");

                        var result = AppLocalCache.GetOrCache("JobRockAcademyList", () =>
                        {
                            return UOWObj.QuizRepository.Get(filter: (a => a.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active) != null
                                       && a.Company.CreateAcademyCourseEnabled
                                       && a.UserPermissionsInQuiz.Any(r => r.UserTypeId == (int)UserTypeEnum.JobRockAcademy)),
                                       includeProperties: "QuizDetails, AttachmentsInQuiz, QuizTagDetails, UserPermissionsInQuiz, Company");
                        });
                    }

                    #endregion

                    #region TechnicalRecruite - TechnicalRecruiteData API

                    if (!Type.HasValue || Type.Value == (int)ListTypeEnum.TechnicalRecruiteData)
                    {
                        AppLocalCache.Remove("TechnicalRecruiterData");

                        var result = AppLocalCache.GetOrCache("TechnicalRecruiterData", () =>
                        {
                            return UOWObj.QuizRepository.Get(filter: (a => (a.UserPermissionsInQuiz.Any(r => r.UserTypeId == (int)UserTypeEnum.TechnicalRecruiter))
                                   && (a.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active) != null)),
                                   includeProperties: "QuizDetails, AttachmentsInQuiz, QuizTagDetails, UserPermissionsInQuiz, ModulePermissionsInQuiz, Company");
                        });
                    }

                    #endregion

                    #region QuizReportList - GetQuizReport API

                    if (Type.Value == (int)ListTypeEnum.Report)
                    {

                        AppLocalCache.Remove("QuizReportList_QuizId_" + FilterId);
                        var result = AppLocalCache.GetOrCache("QuizReportList_QuizId_" + FilterId, () =>
                        {
                            return UOWObj.QuizAttemptsRepository.Get(filter: r => r.Mode == "AUDIT"
                                               && r.QuizDetails.ParentQuizId == FilterId
                                               && r.RecruiterUserId == null
                           , includeProperties: "ConfigurationDetails, WorkPackageInfo, QuizStats, QuizStats.QuizResults, QuizQuestionStats, QuizQuestionStats.QuestionsInQuiz, QuizQuestionStats.QuizAnswerStats");

                        });



                        AppLocalCache.Remove("QuizQuestionStatsList_QuizId_" + FilterId);
                        var result1 = AppLocalCache.GetOrCache("QuizQuestionStatsList_QuizId_" + FilterId, () =>
                        {
                            return UOWObj.QuizQuestionStatsRepository.Get(filter: r => r.Status == (int)StatusEnum.Active && r.CompletedOn.HasValue && r.QuizAttempts.QuizDetails.ParentQuizId == FilterId
                        && (r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.RatingEmoji || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.RatingStarts)
                           , includeProperties: "QuizAttempts.QuizStats, QuestionsInQuiz, QuizAnswerStats");
                        });


                        AppLocalCache.Remove("QuizDetails_QuizId_" + FilterId);
                        var result2 = AppLocalCache.GetOrCache("QuizDetails_QuizId_" + FilterId, () =>
                        {
                            return UOWObj.QuizDetailsRepository.Get(filter: r => r.Status == (int)StatusEnum.Active && r.State == (int)QuizStateEnum.DRAFTED && r.ParentQuizId == FilterId
                            , includeProperties: "QuestionsInQuiz.AnswerOptionsInQuizQuestions, QuizResults");
                        });
                    }

                    #endregion
                }


                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public void AddOrUpdateConfigurationDetails(AddOrUpdateConfiguration configurationObj)
        {
            using (var UOWObj = new AutomationUnitOfWork())
            {
                using (var transaction = Utility.CreateTransactionScope())
                {
                    try
                    {
                        var currentDate = DateTime.UtcNow;
                        var configurationDetailsList = UOWObj.ConfigurationDetailsRepository.Get(r => r.ConfigurationId == configurationObj.ConfigurationId);

                        if (configurationDetailsList != null && configurationDetailsList.Any())
                        {
                            var configurationDetails = configurationDetailsList.FirstOrDefault();

                            var quizObj = UOWObj.QuizRepository.GetByID(configurationObj.QuizId);
                            if (quizObj != null && quizObj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active) != null)
                            {
                                var quizDetails = quizObj.QuizDetails.LastOrDefault(r => r.State == (int)QuizStateEnum.PUBLISHED);

                                var quizComponentLogs = quizDetails.QuizComponentLogs;

                                #region update in ConfigurationDetails

                                configurationDetails.QuizId = configurationObj.QuizId;
                                configurationDetails.IsUpdatedSend = configurationObj.IsUpdatedSend;
                                configurationDetails.Subject = configurationObj.Subject;
                                configurationDetails.Body = configurationObj.Body;
                                configurationDetails.SMSText = configurationObj.SMSText;
                                configurationDetails.SendEmail = configurationObj.SendEmail;
                                configurationDetails.SendSms = configurationObj.SendSms;
                                configurationDetails.SendWhatsApp = configurationObj.SendWhatsApp;
                                configurationDetails.SendFallbackSms = configurationObj.SendFallbackSms;
                                configurationDetails.SendMailNotRequired = configurationObj.SendMailNotRequired;
                                configurationDetails.SourceId = configurationObj.SourceId;
                                configurationDetails.SourceType = configurationObj.SourceType;
                                configurationDetails.SourceName = configurationObj.SourceTitle;
                                configurationDetails.PrivacyLink = configurationObj.PrivacyLink;
                                configurationDetails.ConfigurationType = configurationObj.ConfigurationType;
                                configurationDetails.CompanyCode = configurationObj.CompanyCode;
                                configurationDetails.LeadFormTitle = configurationObj.LeadFormTitle;
                                configurationDetails.UpdatedOn = currentDate;
                                configurationDetails.HsmTemplateId = (configurationObj.WhatsApp != null && configurationObj.WhatsApp.HsmTemplateId > 0) ? configurationObj.WhatsApp.HsmTemplateId : default(int?);
                                configurationDetails.HsmTemplateLanguageCode = (configurationObj.WhatsApp != null && !string.IsNullOrWhiteSpace(configurationObj.WhatsApp.HsmTemplateLanguageCode)) ? configurationObj.WhatsApp.HsmTemplateLanguageCode : null;
                                configurationDetails.FollowUpMessage = configurationObj.WhatsApp != null ? configurationObj.WhatsApp.FollowUpMessage : string.Empty;
                                configurationDetails.PrivacyJson = configurationObj.PrivacyJson != null ? JsonConvert.SerializeObject(configurationObj.PrivacyJson) : null;
                                configurationDetails.MsgVariables = configurationObj.MsgVariables != null && configurationObj.MsgVariables.Any() ? JsonConvert.SerializeObject(configurationObj.MsgVariables) : null;

                                UOWObj.ConfigurationDetailsRepository.Update(configurationDetails);
                                UOWObj.Save();

                                #endregion

                                #region insert in WhatsApp

                                if (configurationDetails.TemplateParameterInConfigurationDetails.Any())
                                {
                                    foreach (var item in configurationDetails.TemplateParameterInConfigurationDetails.ToList())
                                    {
                                        UOWObj.TemplateParameterInConfigurationDetailsRepository.Delete(item);
                                    }
                                    UOWObj.Save();
                                }

                                if (configurationObj.WhatsApp != null)
                                {
                                    if (configurationObj.WhatsApp.TemplateParameters != null)
                                    {
                                        foreach (var item in configurationObj.WhatsApp.TemplateParameters)
                                        {
                                            var templateParameterInConfigurationDetails = new Db.TemplateParameterInConfigurationDetails()
                                            {
                                                ConfigurationDetailsId = configurationDetails.Id,
                                                ParaName = item.Paraname,
                                                Position = item.Position,
                                                Value = item.Value
                                            };
                                            UOWObj.TemplateParameterInConfigurationDetailsRepository.Insert(templateParameterInConfigurationDetails);
                                        }
                                        UOWObj.Save();
                                    }
                                }

                                #endregion

                                #region update in ResultIdsInConfigurationDetails

                                if (configurationDetails.ResultIdsInConfigurationDetails.Any(r => r.QuizResults.QuizId == quizDetails.Id))
                                {
                                    foreach (var resultIdObj in configurationDetails.ResultIdsInConfigurationDetails.Where(r => r.QuizResults.QuizId == quizDetails.Id).ToList())
                                    {
                                        UOWObj.ResultIdsInConfigurationDetailsRepository.Delete(resultIdObj);
                                        UOWObj.Save();
                                    }
                                }

                                if (configurationObj.ResultIds != null && configurationObj.ResultIds.Any())
                                {
                                    foreach (var obj in configurationObj.ResultIds)
                                    {
                                        var resultIdObj = new Db.ResultIdsInConfigurationDetails();

                                        resultIdObj.ResultId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj && r.ObjectTypeId == (int)BranchingLogicEnum.RESULT).PublishedObjectId;
                                        resultIdObj.ConfigurationDetailsId = configurationDetails.Id;
                                        resultIdObj.FormId = 1;
                                        resultIdObj.FlowOrder = 3;

                                        UOWObj.ResultIdsInConfigurationDetailsRepository.Insert(resultIdObj);

                                        UOWObj.Save();
                                    }
                                }
                                else if (configurationObj.LeadFormDetailofResults != null && configurationObj.LeadFormDetailofResults.Any())
                                {
                                    foreach (var obj in configurationObj.LeadFormDetailofResults.ToList())
                                    {
                                        var resultIdObj = new Db.ResultIdsInConfigurationDetails();

                                        resultIdObj.ResultId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ResultId && r.ObjectTypeId == (int)BranchingLogicEnum.RESULT).PublishedObjectId;
                                        resultIdObj.ConfigurationDetailsId = configurationDetails.Id;
                                        resultIdObj.FormId = obj.FormId;
                                        resultIdObj.FlowOrder = obj.FlowOrder;

                                        UOWObj.ResultIdsInConfigurationDetailsRepository.Insert(resultIdObj);

                                        UOWObj.Save();
                                    }
                                }

                                #endregion

                                #region update in VariablesDetails

                                if (configurationDetails.VariablesDetails.Any())
                                {
                                    foreach (var variablesDetailObj in configurationDetails.VariablesDetails.ToList())
                                    {
                                        UOWObj.VariablesDetailsRepository.Delete(variablesDetailObj);
                                        UOWObj.Save();
                                    }
                                }

                                foreach (var obj in configurationObj.DynamicVariables)
                                {
                                    var VariablesObj = UOWObj.VariablesRepository.Get(r => r.VariableInQuiz.Any(s => s.QuizId == quizDetails.Id) && r.Name.ToLower() == obj.Key.ToLower()).FirstOrDefault();

                                    if (VariablesObj != null)
                                    {
                                        var variablesDetailsObj = new Db.VariablesDetails();

                                        variablesDetailsObj.VariableInQuizId = VariablesObj.VariableInQuiz.FirstOrDefault(r => r.QuizId == quizDetails.Id).Id;
                                        variablesDetailsObj.VariableValue = obj.Value == null ? null : obj.Value.Trim();
                                        variablesDetailsObj.ConfigurationDetailsId = configurationDetails.Id;

                                        UOWObj.VariablesDetailsRepository.Insert(variablesDetailsObj);

                                        UOWObj.Save();
                                    }
                                }

                                #endregion

                                #region update in LeadDataInAction

                                if (configurationDetails.LeadDataInAction.Any())
                                {
                                    foreach (var leadDataInActionObj in configurationDetails.LeadDataInAction.Where(r => r.ActionsInQuiz.QuizId == quizDetails.Id).ToList())
                                    {
                                        UOWObj.LeadDataInActionRepository.Delete(leadDataInActionObj);
                                        UOWObj.Save();
                                    }
                                }

                                foreach (var obj in configurationObj.LeadDataInActionList)
                                {
                                    var actionId = 0;

                                    if (obj.ParentId > 0)
                                    {
                                        actionId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ParentId && r.ObjectTypeId == (int)BranchingLogicEnum.ACTION).PublishedObjectId;
                                    }
                                    else
                                    {
                                        int parentActionId = quizComponentLogs.LastOrDefault(r => r.PublishedObjectId == obj.ActionId && r.ObjectTypeId == (int)BranchingLogicEnum.ACTION).DraftedObjectId;
                                        actionId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == parentActionId && r.ObjectTypeId == (int)BranchingLogicEnum.ACTION).PublishedObjectId;
                                    }

                                    var actionsInQuiz = quizDetails.ActionsInQuiz.Where(t => t.Id == actionId).FirstOrDefault();

                                    UpdateleadDataInActionValidator emailListValidator = new UpdateleadDataInActionValidator();
                                    var validationEmailResult = emailListValidator.Validate(obj.ReportEmails.Split(',').Select(t => t.Trim()).ToList());

                                    if (actionsInQuiz != null && (((actionsInQuiz.ActionType == (int)ActionTypeEnum.LinkWithLeadDashboardAppointment || actionsInQuiz.ActionType == (int)ActionTypeEnum.Appointment) && obj.AppointmentTypeId > 0) || ((actionsInQuiz.ActionType == (int)ActionTypeEnum.LinkWithLeadDashboardAppointment || actionsInQuiz.ActionType == (int)ActionTypeEnum.ReportEmail) && !string.IsNullOrEmpty(obj.ReportEmails) && validationEmailResult != null && validationEmailResult.IsValid)))
                                    {
                                        var leadDataInActionObj = new Db.LeadDataInAction();

                                        leadDataInActionObj.ActionId = actionId;
                                        leadDataInActionObj.AppointmentTypeId = obj.AppointmentTypeId > 0 ? obj.AppointmentTypeId : default(int?);
                                        leadDataInActionObj.ReportEmails = !string.IsNullOrEmpty(obj.ReportEmails) ? obj.ReportEmails : null;
                                        if (obj.IsUpdatedSend && !obj.SendMailNotRequired && !string.IsNullOrEmpty(obj.Subject) && !string.IsNullOrEmpty(obj.Body) && !string.IsNullOrEmpty(obj.SMSText))
                                        {
                                            leadDataInActionObj.IsUpdatedSend = obj.IsUpdatedSend;
                                            leadDataInActionObj.Subject = obj.Subject;
                                            leadDataInActionObj.Body = obj.Body;
                                            leadDataInActionObj.SMSText = obj.SMSText;
                                        }
                                        leadDataInActionObj.ConfigurationDetailsId = configurationDetails.Id;


                                        if (obj.CalendarIds != null)
                                        {
                                            foreach (var calendarId in obj.CalendarIds.ToList())
                                            {
                                                var leadCalendarDataInActionObj = new Db.LeadCalendarDataInAction();
                                                leadCalendarDataInActionObj.LeadDataInActionId = leadDataInActionObj.Id;
                                                leadCalendarDataInActionObj.CalendarId = calendarId;
                                                UOWObj.LeadCalendarDataInActionRepository.Insert(leadCalendarDataInActionObj);
                                            }
                                        }

                                        UOWObj.LeadDataInActionRepository.Insert(leadDataInActionObj);

                                        UOWObj.Save();
                                    }
                                }

                                #endregion

                                #region update in MediaVariables

                                if (configurationDetails.MediaVariablesDetails.Any(r => r.QuizId == quizDetails.Id))
                                {
                                    foreach (var obj in configurationDetails.MediaVariablesDetails.Where(r => r.QuizId == quizDetails.Id).ToList())
                                    {
                                        UOWObj.MediaVariablesDetailsRepository.Delete(obj);
                                        UOWObj.Save();
                                    }
                                }

                                if (configurationObj.MediaVariableDetails.Questions != null)
                                {
                                    foreach (var obj in configurationObj.MediaVariableDetails.Questions)
                                    {
                                        var MediaObj = new Db.MediaVariablesDetails();

                                        MediaObj.ObjectId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ParentId && r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION).PublishedObjectId;
                                        MediaObj.ObjectTypeId = (int)BranchingLogicEnum.QUESTION;
                                        MediaObj.ObjectValue = obj.MediaUrlValue;
                                        MediaObj.ObjectPublicId = obj.PublicId;
                                        MediaObj.QuizId = quizDetails.Id;
                                        MediaObj.ConfigurationDetailsId = configurationDetails.Id;
                                        MediaObj.Type = (int)ImageTypeEnum.Title;
                                        MediaObj.MediaOwner = obj.MediaOwner;
                                        MediaObj.ProfileMedia = obj.ProfileMedia;

                                        UOWObj.MediaVariablesDetailsRepository.Insert(MediaObj);

                                        UOWObj.Save();

                                        var MediaforDescriptionObj = new Db.MediaVariablesDetails();

                                        MediaforDescriptionObj.ObjectId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ParentId && r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION).PublishedObjectId;
                                        MediaforDescriptionObj.ObjectTypeId = (int)BranchingLogicEnum.QUESTION;
                                        MediaforDescriptionObj.ObjectValue = obj.MediaUrlforDescriptionValue;
                                        MediaforDescriptionObj.ObjectPublicId = obj.PublicIdforDescription;
                                        MediaforDescriptionObj.QuizId = quizDetails.Id;
                                        MediaforDescriptionObj.ConfigurationDetailsId = configurationDetails.Id;
                                        MediaforDescriptionObj.Type = (int)ImageTypeEnum.Description;
                                        MediaforDescriptionObj.MediaOwner = obj.MediaOwnerforDescription;
                                        MediaforDescriptionObj.ProfileMedia = obj.ProfileMediaforDescription;

                                        UOWObj.MediaVariablesDetailsRepository.Insert(MediaforDescriptionObj);

                                        UOWObj.Save();
                                    }
                                }

                                if (configurationObj.MediaVariableDetails.Answers != null)
                                {
                                    foreach (var obj in configurationObj.MediaVariableDetails.Answers)
                                    {
                                        var MediaObj = new Db.MediaVariablesDetails();

                                        MediaObj.ObjectId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ParentId && r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER).PublishedObjectId;
                                        MediaObj.ObjectTypeId = (int)BranchingLogicEnum.ANSWER;
                                        MediaObj.ObjectValue = obj.MediaUrlValue;
                                        MediaObj.ObjectPublicId = obj.PublicId;
                                        MediaObj.QuizId = quizDetails.Id;
                                        MediaObj.ConfigurationDetailsId = configurationDetails.Id;
                                        MediaObj.Type = (int)ImageTypeEnum.Title;
                                        MediaObj.MediaOwner = obj.MediaOwner;
                                        MediaObj.ProfileMedia = obj.ProfileMedia;

                                        UOWObj.MediaVariablesDetailsRepository.Insert(MediaObj);

                                        UOWObj.Save();
                                    }
                                }

                                if (configurationObj.MediaVariableDetails.Results != null)
                                {
                                    foreach (var obj in configurationObj.MediaVariableDetails.Results)
                                    {
                                        var MediaObj = new Db.MediaVariablesDetails();

                                        MediaObj.ObjectId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ParentId && r.ObjectTypeId == (int)BranchingLogicEnum.RESULT).PublishedObjectId;
                                        MediaObj.ObjectTypeId = (int)BranchingLogicEnum.RESULT;
                                        MediaObj.ObjectValue = obj.MediaUrlValue;
                                        MediaObj.ObjectPublicId = obj.PublicId;
                                        MediaObj.QuizId = quizDetails.Id;
                                        MediaObj.ConfigurationDetailsId = configurationDetails.Id;
                                        MediaObj.Type = (int)ImageTypeEnum.Title;
                                        MediaObj.MediaOwner = obj.MediaOwner;
                                        MediaObj.ProfileMedia = obj.ProfileMedia;

                                        UOWObj.MediaVariablesDetailsRepository.Insert(MediaObj);

                                        UOWObj.Save();
                                    }
                                }

                                if (configurationObj.MediaVariableDetails.CoverDetails != null)
                                {
                                    foreach (var obj in configurationObj.MediaVariableDetails.CoverDetails)
                                    {
                                        var MediaObj = new Db.MediaVariablesDetails();

                                        MediaObj.ObjectId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ParentId && r.ObjectTypeId == (int)BranchingLogicEnum.COVERDETAILS).PublishedObjectId;
                                        MediaObj.ObjectTypeId = (int)BranchingLogicEnum.COVERDETAILS;
                                        MediaObj.ObjectValue = obj.MediaUrlValue;
                                        MediaObj.ObjectPublicId = obj.PublicId;
                                        MediaObj.QuizId = quizDetails.Id;
                                        MediaObj.ConfigurationDetailsId = configurationDetails.Id;
                                        MediaObj.Type = (int)ImageTypeEnum.Title;
                                        MediaObj.MediaOwner = obj.MediaOwner;
                                        MediaObj.ProfileMedia = obj.ProfileMedia;

                                        UOWObj.MediaVariablesDetailsRepository.Insert(MediaObj);

                                        UOWObj.Save();
                                    }
                                }

                                if (configurationObj.MediaVariableDetails.Badges != null)
                                {
                                    foreach (var obj in configurationObj.MediaVariableDetails.Badges)
                                    {
                                        var MediaObj = new Db.MediaVariablesDetails();

                                        MediaObj.ObjectId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ParentId && r.ObjectTypeId == (int)BranchingLogicEnum.BADGE).PublishedObjectId;
                                        MediaObj.ObjectTypeId = (int)BranchingLogicEnum.BADGE;
                                        MediaObj.ObjectValue = obj.MediaUrlValue;
                                        MediaObj.ObjectPublicId = obj.PublicId;
                                        MediaObj.QuizId = quizDetails.Id;
                                        MediaObj.ConfigurationDetailsId = configurationDetails.Id;
                                        MediaObj.Type = (int)ImageTypeEnum.Title;
                                        MediaObj.MediaOwner = obj.MediaOwner;
                                        MediaObj.ProfileMedia = obj.ProfileMedia;

                                        UOWObj.MediaVariablesDetailsRepository.Insert(MediaObj);

                                        UOWObj.Save();
                                    }
                                }

                                if (configurationObj.MediaVariableDetails.Content != null)
                                {
                                    foreach (var obj in configurationObj.MediaVariableDetails.Content)
                                    {
                                        var MediaObj = new Db.MediaVariablesDetails();

                                        MediaObj.ObjectId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ParentId && (quizObj.QuesAndContentInSameTable ? r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION : r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT)).PublishedObjectId;
                                        MediaObj.ObjectTypeId = (int)BranchingLogicEnum.CONTENT;
                                        MediaObj.ObjectValue = obj.MediaUrlValue;
                                        MediaObj.ObjectPublicId = obj.PublicId;
                                        MediaObj.QuizId = quizDetails.Id;
                                        MediaObj.ConfigurationDetailsId = configurationDetails.Id;
                                        MediaObj.Type = (int)ImageTypeEnum.Title;
                                        MediaObj.MediaOwner = obj.MediaOwner;
                                        MediaObj.ProfileMedia = obj.ProfileMedia;

                                        UOWObj.MediaVariablesDetailsRepository.Insert(MediaObj);

                                        UOWObj.Save();

                                        var MediaforDescriptionObj = new Db.MediaVariablesDetails();

                                        MediaforDescriptionObj.ObjectId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ParentId && (quizObj.QuesAndContentInSameTable ? r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION : r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT)).PublishedObjectId;
                                        MediaforDescriptionObj.ObjectTypeId = (int)BranchingLogicEnum.CONTENT;
                                        MediaforDescriptionObj.ObjectValue = obj.MediaUrlforDescriptionValue;
                                        MediaforDescriptionObj.ObjectPublicId = obj.PublicIdforDescription;
                                        MediaforDescriptionObj.QuizId = quizDetails.Id;
                                        MediaforDescriptionObj.ConfigurationDetailsId = configurationDetails.Id;
                                        MediaforDescriptionObj.Type = (int)ImageTypeEnum.Description;
                                        MediaforDescriptionObj.MediaOwner = obj.MediaOwnerforDescription;
                                        MediaforDescriptionObj.ProfileMedia = obj.ProfileMediaforDescription;

                                        UOWObj.MediaVariablesDetailsRepository.Insert(MediaforDescriptionObj);

                                        UOWObj.Save();
                                    }
                                }

                                #endregion

                                #region update in AttachmentsInConfiguration

                                if (configurationDetails.AttachmentsInConfiguration.Any())
                                {
                                    foreach (var attachmentsObj in configurationDetails.AttachmentsInConfiguration.ToList())
                                    {
                                        UOWObj.AttachmentsInConfigurationRepository.Delete(attachmentsObj);
                                        UOWObj.Save();
                                    }
                                }

                                foreach (var obj in configurationObj.EmailAttachments)
                                {
                                    var attachmentsInConfigurationObj = new Db.AttachmentsInConfiguration();

                                    attachmentsInConfigurationObj.FileName = obj.FileName;
                                    attachmentsInConfigurationObj.FileIdentifier = obj.FileIdentifier;
                                    attachmentsInConfigurationObj.FileLink = obj.FileLink;
                                    attachmentsInConfigurationObj.ConfigurationDetailsId = configurationDetails.Id;

                                    UOWObj.AttachmentsInConfigurationRepository.Insert(attachmentsInConfigurationObj);

                                    UOWObj.Save();
                                }

                                #endregion

                                AppLocalCache.Remove("QuizReportList_QuizId_" + quizObj.Id);
                            }
                            else
                            {
                                Status = ResultEnum.Error;
                                ErrorMessage = "Quiz does not exists for QuizId " + configurationDetails.QuizId;
                            }
                        }
                        else
                        {
                            var quizObj = UOWObj.QuizRepository.GetByID(configurationObj.QuizId);
                            if (quizObj != null && quizObj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active) != null)
                            {
                                var quizDetails = quizObj.QuizDetails.LastOrDefault(r => r.State == (int)QuizStateEnum.PUBLISHED);

                                var quizComponentLogs = quizDetails.QuizComponentLogs;

                                #region ConfigurationDetails

                                var configurationDetailsObj = new Db.ConfigurationDetails()
                                {
                                    ConfigurationId = configurationObj.ConfigurationId,
                                    QuizId = configurationObj.QuizId,
                                    IsUpdatedSend = configurationObj.IsUpdatedSend,
                                    Subject = configurationObj.Subject,
                                    Body = configurationObj.Body,
                                    SMSText = configurationObj.SMSText,
                                    SendEmail = configurationObj.SendEmail,
                                    SendSms = configurationObj.SendSms,
                                    SendWhatsApp = configurationObj.SendWhatsApp,
                                    SendFallbackSms = configurationObj.SendFallbackSms,
                                    SendMailNotRequired = configurationObj.SendMailNotRequired,
                                    SourceId = configurationObj.SourceId,
                                    SourceType = configurationObj.SourceType,
                                    SourceName = configurationObj.SourceTitle,
                                    PrivacyLink = configurationObj.PrivacyLink,
                                    ConfigurationType = configurationObj.ConfigurationType,
                                    CompanyCode = configurationObj.CompanyCode,
                                    LeadFormTitle = configurationObj.LeadFormTitle,
                                    CreatedOn = currentDate,
                                    Status = (int)StatusEnum.Active,
                                    HsmTemplateId = (configurationObj.WhatsApp != null && configurationObj.WhatsApp.HsmTemplateId > 0) ? configurationObj.WhatsApp.HsmTemplateId : default(int?),
                                    HsmTemplateLanguageCode = (configurationObj.WhatsApp != null && !string.IsNullOrWhiteSpace(configurationObj.WhatsApp.HsmTemplateLanguageCode)) ? configurationObj.WhatsApp.HsmTemplateLanguageCode : null,
                                    FollowUpMessage = configurationObj.WhatsApp != null ? configurationObj.WhatsApp.FollowUpMessage : string.Empty,
                                    PrivacyJson = configurationObj.PrivacyJson != null ? JsonConvert.SerializeObject(configurationObj.PrivacyJson) : null,
                                    MsgVariables = configurationObj.MsgVariables != null && configurationObj.MsgVariables.Any() ? JsonConvert.SerializeObject(configurationObj.MsgVariables) : null
                                };

                                UOWObj.ConfigurationDetailsRepository.Insert(configurationDetailsObj);
                                UOWObj.Save();

                                #endregion

                                #region insert in WhatsApp

                                if (configurationObj.WhatsApp != null && configurationObj.WhatsApp.TemplateParameters != null)
                                {
                                    foreach (var item in configurationObj.WhatsApp.TemplateParameters)
                                    {
                                        var templateParameterInConfigurationDetails = new Db.TemplateParameterInConfigurationDetails()
                                        {
                                            ConfigurationDetailsId = configurationDetailsObj.Id,
                                            ParaName = item.Paraname,
                                            Position = item.Position,
                                            Value = item.Value
                                        };

                                        UOWObj.TemplateParameterInConfigurationDetailsRepository.Insert(templateParameterInConfigurationDetails);
                                    }
                                    UOWObj.Save();
                                }

                                #endregion

                                #region insert in ResultIdsInConfigurationDetails

                                if (configurationObj.ResultIds != null && configurationObj.ResultIds.Any())
                                {
                                    foreach (var obj in configurationObj.ResultIds)
                                    {
                                        var resultIdObj = new Db.ResultIdsInConfigurationDetails();

                                        resultIdObj.ResultId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj && r.ObjectTypeId == (int)BranchingLogicEnum.RESULT).PublishedObjectId;
                                        resultIdObj.ConfigurationDetailsId = configurationDetailsObj.Id;
                                        resultIdObj.FormId = 1;
                                        resultIdObj.FlowOrder = 3;

                                        UOWObj.ResultIdsInConfigurationDetailsRepository.Insert(resultIdObj);

                                        UOWObj.Save();
                                    }
                                }
                                else if (configurationObj.LeadFormDetailofResults != null && configurationObj.LeadFormDetailofResults.Any())
                                {
                                    foreach (var obj in configurationObj.LeadFormDetailofResults.ToList())
                                    {
                                        var resultIdObj = new Db.ResultIdsInConfigurationDetails();

                                        resultIdObj.ResultId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ResultId && r.ObjectTypeId == (int)BranchingLogicEnum.RESULT).PublishedObjectId;
                                        resultIdObj.ConfigurationDetailsId = configurationDetailsObj.Id;
                                        resultIdObj.FormId = obj.FormId;
                                        resultIdObj.FlowOrder = obj.FlowOrder;

                                        UOWObj.ResultIdsInConfigurationDetailsRepository.Insert(resultIdObj);

                                        UOWObj.Save();
                                    }
                                }

                                #endregion

                                #region insert in VariablesDetails

                                foreach (var obj in configurationObj.DynamicVariables)
                                {
                                    var VariablesObj = UOWObj.VariablesRepository.Get(r => r.VariableInQuiz.Any(s => s.QuizId == quizDetails.Id) && r.Name.ToLower() == obj.Key.ToLower()).FirstOrDefault();

                                    if (VariablesObj != null)
                                    {
                                        var variablesDetailsObj = new Db.VariablesDetails();

                                        variablesDetailsObj.VariableInQuizId = VariablesObj.VariableInQuiz.FirstOrDefault(r => r.QuizId == quizDetails.Id).Id;
                                        variablesDetailsObj.VariableValue = obj.Value == null ? null : obj.Value.Trim();
                                        variablesDetailsObj.ConfigurationDetailsId = configurationDetailsObj.Id;

                                        UOWObj.VariablesDetailsRepository.Insert(variablesDetailsObj);

                                        UOWObj.Save();
                                    }
                                }

                                #endregion

                                #region insert in LeadDataInAction

                                foreach (var obj in configurationObj.LeadDataInActionList)
                                {
                                    var actionId = 0;

                                    if (obj.ParentId > 0)
                                    {
                                        actionId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ParentId && r.ObjectTypeId == (int)BranchingLogicEnum.ACTION).PublishedObjectId;
                                    }
                                    else
                                    {
                                        int parentActionId = quizComponentLogs.LastOrDefault(r => r.PublishedObjectId == obj.ActionId && r.ObjectTypeId == (int)BranchingLogicEnum.ACTION).DraftedObjectId;
                                        actionId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == parentActionId && r.ObjectTypeId == (int)BranchingLogicEnum.ACTION).PublishedObjectId;
                                    }

                                    var actionsInQuiz = quizDetails.ActionsInQuiz.Where(t => t.Id == actionId).FirstOrDefault();

                                    UpdateleadDataInActionValidator emailListValidator = new UpdateleadDataInActionValidator();
                                    var validationEmailResult = emailListValidator.Validate(obj.ReportEmails.Split(',').Select(t => t.Trim()).ToList());

                                    if (actionsInQuiz != null && (((actionsInQuiz.ActionType == (int)ActionTypeEnum.LinkWithLeadDashboardAppointment || actionsInQuiz.ActionType == (int)ActionTypeEnum.Appointment) && obj.AppointmentTypeId > 0) || ((actionsInQuiz.ActionType == (int)ActionTypeEnum.LinkWithLeadDashboardAppointment || actionsInQuiz.ActionType == (int)ActionTypeEnum.ReportEmail) && !string.IsNullOrEmpty(obj.ReportEmails) && validationEmailResult != null && validationEmailResult.IsValid)))
                                    {
                                        var leadDataInActionObj = new Db.LeadDataInAction();

                                        leadDataInActionObj.ActionId = actionId;
                                        leadDataInActionObj.AppointmentTypeId = obj.AppointmentTypeId > 0 ? obj.AppointmentTypeId : default(int?);
                                        leadDataInActionObj.ReportEmails = !string.IsNullOrEmpty(obj.ReportEmails) ? obj.ReportEmails : null;
                                        if (obj.IsUpdatedSend && !obj.SendMailNotRequired && !string.IsNullOrEmpty(obj.Subject) && !string.IsNullOrEmpty(obj.Body) && !string.IsNullOrEmpty(obj.SMSText))
                                        {
                                            leadDataInActionObj.IsUpdatedSend = obj.IsUpdatedSend;
                                            leadDataInActionObj.Subject = obj.Subject;
                                            leadDataInActionObj.Body = obj.Body;
                                            leadDataInActionObj.SMSText = obj.SMSText;
                                        }
                                        leadDataInActionObj.ConfigurationDetailsId = configurationDetailsObj.Id;


                                        if (obj.CalendarIds != null)
                                        {
                                            foreach (var calendarId in obj.CalendarIds.ToList())
                                            {
                                                var leadCalendarDataInActionObj = new Db.LeadCalendarDataInAction();
                                                leadCalendarDataInActionObj.LeadDataInActionId = leadDataInActionObj.Id;
                                                leadCalendarDataInActionObj.CalendarId = calendarId;
                                                UOWObj.LeadCalendarDataInActionRepository.Insert(leadCalendarDataInActionObj);
                                            }
                                        }

                                        UOWObj.LeadDataInActionRepository.Insert(leadDataInActionObj);

                                        UOWObj.Save();
                                    }
                                }

                                #endregion

                                #region insert in MediaVariables

                                if (configurationObj.MediaVariableDetails.Questions != null)
                                {
                                    foreach (var obj in configurationObj.MediaVariableDetails.Questions)
                                    {
                                        var MediaObj = new Db.MediaVariablesDetails();

                                        MediaObj.ObjectId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ParentId && r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION).PublishedObjectId;
                                        MediaObj.ObjectTypeId = (int)BranchingLogicEnum.QUESTION;
                                        MediaObj.ObjectValue = obj.MediaUrlValue;
                                        MediaObj.ObjectPublicId = obj.PublicId;
                                        MediaObj.QuizId = quizDetails.Id;
                                        MediaObj.ConfigurationDetailsId = configurationDetailsObj.Id;
                                        MediaObj.Type = (int)ImageTypeEnum.Title;
                                        MediaObj.MediaOwner = obj.MediaOwner;
                                        MediaObj.ProfileMedia = obj.ProfileMedia;

                                        UOWObj.MediaVariablesDetailsRepository.Insert(MediaObj);

                                        UOWObj.Save();

                                        var MediaforDescriptionObj = new Db.MediaVariablesDetails();

                                        MediaforDescriptionObj.ObjectId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ParentId && r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION).PublishedObjectId;
                                        MediaforDescriptionObj.ObjectTypeId = (int)BranchingLogicEnum.QUESTION;
                                        MediaforDescriptionObj.ObjectValue = obj.MediaUrlforDescriptionValue;
                                        MediaforDescriptionObj.ObjectPublicId = obj.PublicIdforDescription;
                                        MediaforDescriptionObj.QuizId = quizDetails.Id;
                                        MediaforDescriptionObj.ConfigurationDetailsId = configurationDetailsObj.Id;
                                        MediaforDescriptionObj.Type = (int)ImageTypeEnum.Description;
                                        MediaforDescriptionObj.MediaOwner = obj.MediaOwnerforDescription;
                                        MediaforDescriptionObj.ProfileMedia = obj.ProfileMediaforDescription;

                                        UOWObj.MediaVariablesDetailsRepository.Insert(MediaforDescriptionObj);

                                        UOWObj.Save();
                                    }
                                }

                                if (configurationObj.MediaVariableDetails.Answers != null)
                                {
                                    foreach (var obj in configurationObj.MediaVariableDetails.Answers)
                                    {
                                        var MediaObj = new Db.MediaVariablesDetails();

                                        MediaObj.ObjectId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ParentId && r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER).PublishedObjectId;
                                        MediaObj.ObjectTypeId = (int)BranchingLogicEnum.ANSWER;
                                        MediaObj.ObjectValue = obj.MediaUrlValue;
                                        MediaObj.ObjectPublicId = obj.PublicId;
                                        MediaObj.QuizId = quizDetails.Id;
                                        MediaObj.ConfigurationDetailsId = configurationDetailsObj.Id;
                                        MediaObj.Type = (int)ImageTypeEnum.Title;
                                        MediaObj.MediaOwner = obj.MediaOwner;
                                        MediaObj.ProfileMedia = obj.ProfileMedia;

                                        UOWObj.MediaVariablesDetailsRepository.Insert(MediaObj);

                                        UOWObj.Save();
                                    }
                                }

                                if (configurationObj.MediaVariableDetails.Results != null)
                                {
                                    foreach (var obj in configurationObj.MediaVariableDetails.Results)
                                    {
                                        var MediaObj = new Db.MediaVariablesDetails();

                                        MediaObj.ObjectId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ParentId && r.ObjectTypeId == (int)BranchingLogicEnum.RESULT).PublishedObjectId;
                                        MediaObj.ObjectTypeId = (int)BranchingLogicEnum.RESULT;
                                        MediaObj.ObjectPublicId = obj.PublicId;
                                        MediaObj.ObjectValue = obj.MediaUrlValue;
                                        MediaObj.QuizId = quizDetails.Id;
                                        MediaObj.ConfigurationDetailsId = configurationDetailsObj.Id;
                                        MediaObj.Type = (int)ImageTypeEnum.Title;
                                        MediaObj.MediaOwner = obj.MediaOwner;
                                        MediaObj.ProfileMedia = obj.ProfileMedia;

                                        UOWObj.MediaVariablesDetailsRepository.Insert(MediaObj);

                                        UOWObj.Save();
                                    }
                                }

                                if (configurationObj.MediaVariableDetails.CoverDetails != null)
                                {
                                    foreach (var obj in configurationObj.MediaVariableDetails.CoverDetails)
                                    {
                                        var MediaObj = new Db.MediaVariablesDetails();

                                        MediaObj.ObjectId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ParentId && r.ObjectTypeId == (int)BranchingLogicEnum.COVERDETAILS).PublishedObjectId;
                                        MediaObj.ObjectTypeId = (int)BranchingLogicEnum.COVERDETAILS;
                                        MediaObj.ObjectValue = obj.MediaUrlValue;
                                        MediaObj.ObjectPublicId = obj.PublicId;
                                        MediaObj.QuizId = quizDetails.Id;
                                        MediaObj.ConfigurationDetailsId = configurationDetailsObj.Id;
                                        MediaObj.Type = (int)ImageTypeEnum.Title;
                                        MediaObj.MediaOwner = obj.MediaOwner;
                                        MediaObj.ProfileMedia = obj.ProfileMedia;

                                        UOWObj.MediaVariablesDetailsRepository.Insert(MediaObj);

                                        UOWObj.Save();
                                    }
                                }

                                if (configurationObj.MediaVariableDetails.Badges != null)
                                {
                                    foreach (var obj in configurationObj.MediaVariableDetails.Badges)
                                    {
                                        var MediaObj = new Db.MediaVariablesDetails();

                                        MediaObj.ObjectId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ParentId && r.ObjectTypeId == (int)BranchingLogicEnum.BADGE).PublishedObjectId;
                                        MediaObj.ObjectTypeId = (int)BranchingLogicEnum.BADGE;
                                        MediaObj.ObjectValue = obj.MediaUrlValue;
                                        MediaObj.ObjectPublicId = obj.PublicId;
                                        MediaObj.QuizId = quizDetails.Id;
                                        MediaObj.ConfigurationDetailsId = configurationDetailsObj.Id;
                                        MediaObj.Type = (int)ImageTypeEnum.Title;
                                        MediaObj.MediaOwner = obj.MediaOwner;
                                        MediaObj.ProfileMedia = obj.ProfileMedia;

                                        UOWObj.MediaVariablesDetailsRepository.Insert(MediaObj);

                                        UOWObj.Save();
                                    }
                                }

                                if (configurationObj.MediaVariableDetails.Content != null)
                                {
                                    foreach (var obj in configurationObj.MediaVariableDetails.Content)
                                    {
                                        var MediaObj = new Db.MediaVariablesDetails();

                                        MediaObj.ObjectId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ParentId && (quizObj.QuesAndContentInSameTable ? r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION : r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT)).PublishedObjectId;
                                        MediaObj.ObjectTypeId = (int)BranchingLogicEnum.CONTENT;
                                        MediaObj.ObjectValue = obj.MediaUrlValue;
                                        MediaObj.ObjectPublicId = obj.PublicId;
                                        MediaObj.QuizId = quizDetails.Id;
                                        MediaObj.ConfigurationDetailsId = configurationDetailsObj.Id;
                                        MediaObj.Type = (int)ImageTypeEnum.Title;
                                        MediaObj.MediaOwner = obj.MediaOwner;
                                        MediaObj.ProfileMedia = obj.ProfileMedia;

                                        UOWObj.MediaVariablesDetailsRepository.Insert(MediaObj);

                                        UOWObj.Save();

                                        var MediaforDescriptionObj = new Db.MediaVariablesDetails();

                                        MediaforDescriptionObj.ObjectId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ParentId && (quizObj.QuesAndContentInSameTable ? r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION : r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT)).PublishedObjectId;
                                        MediaforDescriptionObj.ObjectTypeId = (int)BranchingLogicEnum.CONTENT;
                                        MediaforDescriptionObj.ObjectValue = obj.MediaUrlforDescriptionValue;
                                        MediaforDescriptionObj.ObjectPublicId = obj.PublicIdforDescription;
                                        MediaforDescriptionObj.QuizId = quizDetails.Id;
                                        MediaforDescriptionObj.ConfigurationDetailsId = configurationDetailsObj.Id;
                                        MediaforDescriptionObj.Type = (int)ImageTypeEnum.Description;
                                        MediaforDescriptionObj.MediaOwner = obj.MediaOwnerforDescription;
                                        MediaforDescriptionObj.ProfileMedia = obj.ProfileMediaforDescription;

                                        UOWObj.MediaVariablesDetailsRepository.Insert(MediaforDescriptionObj);

                                        UOWObj.Save();
                                    }
                                }

                                #endregion

                                #region insert in AttachmentsInConfiguration

                                foreach (var obj in configurationObj.EmailAttachments)
                                {
                                    var attachmentsInConfigurationObj = new Db.AttachmentsInConfiguration();

                                    attachmentsInConfigurationObj.FileName = obj.FileName;
                                    attachmentsInConfigurationObj.FileIdentifier = obj.FileIdentifier;
                                    attachmentsInConfigurationObj.FileLink = obj.FileLink;
                                    attachmentsInConfigurationObj.ConfigurationDetailsId = configurationDetailsObj.Id;

                                    UOWObj.AttachmentsInConfigurationRepository.Insert(attachmentsInConfigurationObj);

                                    UOWObj.Save();
                                }

                                #endregion

                            }
                            else
                            {
                                Status = ResultEnum.Error;
                                ErrorMessage = "Quiz does not exists for QuizId " + configurationObj.QuizId;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = ex.Message;
                        throw ex;
                    }
                    transaction.Complete();
                }
            }
        }

        public void RemoveConfiguration(string ConfigurationId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var configurationDetailsObj = UOWObj.ConfigurationDetailsRepository.Get(r => r.ConfigurationId == ConfigurationId);

                    if (configurationDetailsObj != null && configurationDetailsObj.Any())
                    {
                        var configurationObj = configurationDetailsObj.FirstOrDefault();

                        if (configurationObj != null)
                        {
                            var currentDate = DateTime.UtcNow;

                            #region update Quiz Details

                            configurationObj.Status = (int)StatusEnum.Deleted;
                            configurationObj.UpdatedOn = currentDate;

                            #endregion

                            UOWObj.ConfigurationDetailsRepository.Update(configurationObj);

                            UOWObj.Save();
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Configuration not found for the ConfigurationId " + ConfigurationId;
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

        public void SaveQuizUrlSetting(string Key, string DomainName, string Value, int companyId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizUrlSettingObj = new Db.QuizUrlSetting();

                    quizUrlSettingObj.Key = Key;
                    quizUrlSettingObj.DomainName = DomainName;
                    quizUrlSettingObj.Value = Value;
                    quizUrlSettingObj.CompanyId = companyId;
                    UOWObj.QuizUrlSettingRepository.Insert(quizUrlSettingObj);
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

        public bool CheckQuizUrlSettingKey(string Key, string DomainName)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    return UOWObj.QuizUrlSettingRepository.Get(t => t.Key.ToLower() == Key.ToLower() && t.DomainName == DomainName).Any();
                }
            }
            catch (Exception ex)
            {
                status = ResultEnum.Error;
                errormessage = ex.Message;
                throw ex;
            }
        }

        //public void SendSmsByWorkPackageId(SendSMS Obj)
        //{
        //    try
        //    {
        //        using (var UOWObj = new AutomationUnitOfWork())
        //        {
        //            var sendSmsStatus = false;

        //            var workPackageInfoObj = UOWObj.WorkPackageInfoRepository.GetByID(Obj.WorkPackageId);

        //            if (workPackageInfoObj != null && workPackageInfoObj.SMSSentOn == null)
        //            {
        //                var configurationDetail = workPackageInfoObj.ConfigurationDetails;
        //                var quizObj = workPackageInfoObj.Quiz;

        //                if (!configurationDetail.SendMailNotRequired)
        //                {
        //                    var company = quizObj.Company;

        //                    CompanyModel companyObj;
        //                    if (quizObj.Company != null)
        //                    {
        //                        companyObj = new CompanyModel
        //                        {
        //                            Id = company.Id,
        //                            AlternateClientCodes = company.AlternateClientCodes,
        //                            ClientCode = company.ClientCode,
        //                            CompanyName = company.CompanyName,
        //                            CompanyWebsiteUrl = company.CompanyWebsiteUrl,
        //                            JobRocketApiAuthorizationBearer = company.JobRocketApiAuthorizationBearer,
        //                            JobRocketApiUrl = company.JobRocketApiUrl,
        //                            JobRocketClientUrl = company.JobRocketClientUrl,
        //                            LeadDashboardApiAuthorizationBearer = company.LeadDashboardApiAuthorizationBearer,
        //                            LeadDashboardApiUrl = company.LeadDashboardApiUrl,
        //                            LeadDashboardClientUrl = quizObj.Company.LeadDashboardClientUrl,
        //                            LogoUrl = company.LogoUrl,
        //                            PrimaryBrandingColor = company.PrimaryBrandingColor,
        //                            SecondaryBrandingColor = company.SecondaryBrandingColor

        //                        };
        //                    }
        //                    else
        //                    {
        //                        companyObj = new CompanyModel();
        //                    }

        //                    var shortUrl = string.Empty;

        //                    var domainList = CommonStaticData.GetCachedClientDomains(companyObj);
        //                    var domain = (domainList != null && domainList.Any()) ? domainList.FirstOrDefault().Domain : GlobalSettings.defaultDomain.ToString();

        //                    var template = UOWObj.NotificationTemplatesInQuizRepository.Get(r => r.NotificationTemplate.Status == (int)StatusEnum.Active && r.QuizId == configurationDetail.QuizId && r.NotificationTemplate.NotificationType == (int)NotificationTypeEnum.INVITATION,
        //                                    includeProperties: "NotificationTemplate").FirstOrDefault();

        //                    var leadUserInfo = OWCHelper.GetLeadUserInfo(workPackageInfoObj.LeadUserId, companyObj);

        //                    if (!string.IsNullOrEmpty(workPackageInfoObj.ShotenUrlCode))
        //                    {
        //                        shortUrl = workPackageInfoObj.IsOurEndLogicForInvitation
        //                            ? "https://" + domain + "/" + workPackageInfoObj.ShotenUrlCode
        //                            : "https://" + GlobalSettings.domainUrl.ToString() + "/" + workPackageInfoObj.ShotenUrlCode;
        //                    }
        //                    else
        //                    {
        //                        #region url shortning

        //                        var shotenUrlCode = string.Empty;

        //                        #region url shortning by custom domain

        //                        if (template != null && !string.IsNullOrEmpty(template.NotificationTemplate.EmailLinkVariable))
        //                            shotenUrlCode = template.NotificationTemplate.EmailLinkVariable + "-" + IdGenerator.GetShortCode();
        //                        else
        //                            shotenUrlCode = IdGenerator.GetShortCode();

        //                        #region insert QuizUrlSetting

        //                        var quizUrlSettingObj = new Db.QuizUrlSetting();

        //                        quizUrlSettingObj.Key = shotenUrlCode;
        //                        quizUrlSettingObj.DomainName = domain;
        //                        quizUrlSettingObj.CompanyId = quizObj.CompanyId.HasValue ? quizObj.CompanyId.Value : 0;
        //                        quizUrlSettingObj.Value = GlobalSettings.webUrl.ToString() + "/quiz?Code=" + quizObj.PublishedCode + "&UserTypeId=2&UserId=" + workPackageInfoObj.LeadUserId + "&WorkPackageInfoId=" + workPackageInfoObj.Id;

        //                        UOWObj.QuizUrlSettingRepository.Insert(quizUrlSettingObj);
        //                        UOWObj.Save();

        //                        shortUrl = "https://" + quizUrlSettingObj.DomainName + "/" + quizUrlSettingObj.Key;

        //                        #endregion

        //                        #region update ShotenUrlCode in WorkPackageInfo

        //                        workPackageInfoObj.ShotenUrlCode = shotenUrlCode;

        //                        workPackageInfoObj.IsOurEndLogicForInvitation = true;

        //                        if (template != null && !string.IsNullOrEmpty(template.NotificationTemplate.EmailLinkVariable))
        //                            workPackageInfoObj.EmailLinkVariableForInvitation = template.NotificationTemplate.EmailLinkVariable;

        //                        UOWObj.WorkPackageInfoRepository.Update(workPackageInfoObj);
        //                        UOWObj.Save();

        //                        #endregion

        //                        #endregion

        //                        #endregion
        //                    }

        //                    var quizDetails = quizObj.QuizDetails.LastOrDefault(r => r.State == (int)QuizStateEnum.PUBLISHED);

        //                    var title = CommonService.SmsDynamicVariableReplace(quizDetails.QuizTitle, leadUserInfo, workPackageInfoObj.LeadUserId, string.Empty, string.Empty, string.Empty, string.Empty, quizDetails);

        //                    IEnumerable<Db.VariablesDetails> variablesDetailList = UOWObj.VariablesDetailsRepository.Get(r => r.VariableInQuiz.QuizId == quizDetails.ParentQuizId && r.LeadId == workPackageInfoObj.LeadUserId);

        //                    MatchCollection mcol = Regex.Matches(title, @"%\b\S+?\b%");

        //                    foreach (Match m in mcol)
        //                    {
        //                        var variablesDetailObj = variablesDetailList.FirstOrDefault(t => t.VariableInQuiz.Variables.Name == m.ToString().ToLower().Replace("%", string.Empty));
        //                        if (variablesDetailObj != null)
        //                        {
        //                            title = title.Replace(m.ToString(), variablesDetailObj.VariableValue);
        //                        }
        //                        else
        //                        {
        //                            title = title.Replace(m.ToString(), string.Empty);
        //                        }
        //                    }


        //                    if (!configurationDetail.IsUpdatedSend)
        //                    {
        //                        if (template != null && template.NotificationTemplate != null && (!string.IsNullOrWhiteSpace(leadUserInfo?.email)))
        //                        {
        //                            if (!string.IsNullOrEmpty(template.NotificationTemplate.SMSText))
        //                            {
        //                                var smsBody = CommonService.SmsDynamicVariableReplace(template.NotificationTemplate.SMSText, leadUserInfo, workPackageInfoObj.LeadUserId, title, ((QuizTypeEnum)quizObj.QuizType).ToString(), shortUrl, String.IsNullOrEmpty(workPackageInfoObj.CampaignName) ? string.Empty : workPackageInfoObj.CampaignName, null);

        //                                sendSmsStatus = CommunicationHelper.SendSMS(leadUserInfo.telephone, smsBody, companyObj);

        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        if (configurationDetail.SendFallbackSms.GetValueOrDefault() == true && !string.IsNullOrEmpty(configurationDetail.SMSText) && leadUserInfo != null)
        //                        {
        //                            var smsBody = CommonService.SmsDynamicVariableReplace(configurationDetail.SMSText, leadUserInfo, workPackageInfoObj.LeadUserId, title, ((QuizTypeEnum)quizObj.QuizType).ToString(), shortUrl, string.IsNullOrEmpty(workPackageInfoObj.CampaignName) ? string.Empty : workPackageInfoObj.CampaignName, null);

        //                            sendSmsStatus = CommunicationHelper.SendSMS(leadUserInfo.telephone, smsBody, companyObj);

        //                            var triggerSMSFallbackEmailNotification = !string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TriggerSMSFallbackEmailNotification"]) ? Convert.ToBoolean(ConfigurationManager.AppSettings["TriggerSMSFallbackEmailNotification"]) : false;

        //                            if (triggerSMSFallbackEmailNotification)
        //                            {
        //                                bool sendMailStatus = CommunicationHelper.SendMail(leadUserInfo.email, "SMS Fallback Notification", smsBody);
        //                                if (sendMailStatus) { sendSmsStatus = true; }
        //                            }
        //                        }
        //                    }

        //                    if (sendSmsStatus)
        //                    {
        //                        workPackageInfoObj.SMSSentOn = DateTime.UtcNow;
        //                        UOWObj.WorkPackageInfoRepository.Update(workPackageInfoObj);
        //                        UOWObj.Save();
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                Status = ResultEnum.Ok;
        //                ErrorMessage = "Record does not exists for WorkPackageId = " + Obj.WorkPackageId;
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

        static void SendEmailSmsandWhatsappHandlerNewWithQueue(TempWorkpackagePush item)
        {
            try
            {
                ExternalActionQueueService.InsertExternalActionQueue(item.companyObj.Id, item.LeadUserId, QueueItemTypes.SendEmailSms, (int)QueueStatusTypes.New, JsonConvert.SerializeObject(item));
                ExternalActionQueueService.InsertExternalActionQueue(item.companyObj.Id, item.LeadUserId, QueueItemTypes.SendWhatsapp, (int)QueueStatusTypes.New, JsonConvert.SerializeObject(item));
            }
            catch (Exception ex)
            {

                return;
            }
        }

        public static void SendEmailSmsandWhatsappHandlerNew(TempWorkpackagePush item)
        {
            try
            {
                if (enableExternalActionQueue)
                {
                    SendEmailSmsandWhatsappHandlerNewWithQueue(item);
                }
                else
                {
                    Task.Run(async () =>
                    {
                        await PrerenderHelper.RecachePrerenderedPage(item.shortUrl, item.workPackageInfoId);
                        await SendEmailSmsNew(item);
                        await SendWhatsappNew(item);
                    });
                }
            }
            catch (Exception ex)
            {

                Logger.Log(LogLevel.Error, "SendEmailSmsandWhatsappHandlerNew inside function error - " + item.workPackageInfoId + " Exception-" + ex.Message);
            }


        }

        public static async Task SendEmailSmsNew(TempWorkpackagePush tempWorkpackagePush)
        {

            int Id = tempWorkpackagePush.workPackageInfoId;
            int quizDetaildId = tempWorkpackagePush.quizDetailid;
            var shotenUrlCode = tempWorkpackagePush.shotenUrlCode;
            var shortUrl = tempWorkpackagePush.shortUrl;

            var title = string.Empty;
            var sendMailStatus = false;
            var sendSmsStatus = false;
            var sendWhatsappStatus = false;
            var currentDate = DateTime.UtcNow;
            string body = string.Empty;

            string emailLinkVariableForInvitation = string.Empty;
            Db.ConfigurationDetails configurationDetail = tempWorkpackagePush.ConfigurationDetails;

            if (configurationDetail.ConfigurationType.EqualsCI("WHATSAPP_CHATBOT") || configurationDetail.ConfigurationType.EqualsCI("WHATSAPP_CHATBOT_TEMPLATE"))
            {

                return;
            }
            if (tempWorkpackagePush.UsageType == (int)UsageTypeEnum.WhatsAppChatbot)
            {
                return;
            }

            Db.QuizDetails quizDetails = tempWorkpackagePush.QuizDetails;
            Db.NotificationTemplate notificationTemplate = null;
            List<Db.VariablesDetails> variablesDetailList = null;
            List<Db.TemplateParameterInConfigurationDetails> templateParameterInConfigurationDetailList = null;
            OWCUserVariable leadRecruiterObj = null;


            var errortrack = "";
            try
            {

                using (var UOWObj = new AutomationUnitOfWork())
                {

                    try
                    {
                        if (configurationDetail != null && templateParameterInConfigurationDetailList != null)
                        {
                            templateParameterInConfigurationDetailList = UOWObj.TemplateParameterInConfigurationDetailsRepository.GetTemplateParameterInConfigurationDetailsByConfigurationId(configurationDetail.Id).ToList();
                        }
                    }
                    catch (Exception ex)
                    {

                        Logger.Log(LogLevel.Error, "SendEmailSmsandWhatsappNew templateParameterInConfigurationDetailList - " + Id + " has issue" + ex.Message);
                    }


                    try
                    {
                        var template = UOWObj.NotificationTemplatesInQuizRepository.Get(r => r.NotificationTemplate.Status == (int)StatusEnum.Active && r.QuizId == tempWorkpackagePush.parentquizid && r.NotificationTemplate.NotificationType == (int)NotificationTypeEnum.INVITATION).FirstOrDefault();

                        if (template != null)
                        {
                            notificationTemplate = template.NotificationTemplate;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, "SendEmailSmsandWhatsappNew notificationTemplate - " + Id + " has issue " + ex.Message);
                    }

                }


                if (!string.IsNullOrEmpty(tempWorkpackagePush.leadUserInfo.email))
                {
                    title = ModifyQuizTitle(quizDetails.QuizTitle, tempWorkpackagePush.leadUserInfo, tempWorkpackagePush.LeadUserId, configurationDetail.QuizId, configurationDetail.Id);
                }

                errortrack += $"#get notificationTemplate #";
                try
                {
                    var userdetails = new List<OWCUserVariable>();
                    long[] Userids = new long[1];

                    if (!tempWorkpackagePush.leadUserInfo.contactId.ContainsCI("SF"))
                    {
                        if (!string.IsNullOrWhiteSpace(tempWorkpackagePush.leadUserInfo.SourceOwnerId))
                        {
                            long sourceOwnerId = Convert.ToInt64(tempWorkpackagePush.leadUserInfo.SourceOwnerId);
                            if (sourceOwnerId > 0)
                            {
                                Userids[0] = sourceOwnerId;
                            }
                            else
                            {
                                long ContactOwnerId = Convert.ToInt64(tempWorkpackagePush.leadUserInfo.ContactOwnerId);
                                if (ContactOwnerId > 0)
                                {
                                    Userids[0] = ContactOwnerId;
                                }


                            }
                        }

                        if (Userids.Any())
                        {
                            userdetails = OWCHelper.GetUserListUsingUserId(Userids, tempWorkpackagePush.companyObj).ToList();
                        }

                        if (userdetails != null && userdetails.Any())
                        {
                            leadRecruiterObj = userdetails.FirstOrDefault();
                        }
                    }

                    else
                    {

                        if (!string.IsNullOrWhiteSpace(tempWorkpackagePush.leadUserInfo?.SourceOwnerId) || !string.IsNullOrWhiteSpace(tempWorkpackagePush.leadUserInfo?.ContactOwnerId))
                        {
                            var externalDetails = _owchelper.GetExternalDetails(tempWorkpackagePush.companyObj.ClientCode, tempWorkpackagePush.leadUserInfo.SourceOwnerId);
                            if (externalDetails != null)
                            {
                                var oWCBusinessUserResponse = JsonConvert.DeserializeObject<OWCUserVariable>(externalDetails);
                                if (oWCBusinessUserResponse !=null)
                                {
                                    leadRecruiterObj = oWCBusinessUserResponse;
                                }
                            }
                            if (leadRecruiterObj == null)
                            {
                                var externalDetails2 = _owchelper.GetExternalDetails(tempWorkpackagePush.companyObj.ClientCode, tempWorkpackagePush.leadUserInfo.ContactOwnerId);
                                if (externalDetails2 != null)
                                {
                                        var oWCBusinessUserResponse = JsonConvert.DeserializeObject<OWCUserVariable>(externalDetails2);
                                        if (oWCBusinessUserResponse != null)
                                        {
                                            leadRecruiterObj = oWCBusinessUserResponse;
                                        }

                                }
                            }

                            List<BasicDetails> basicDetails = new List<BasicDetails>();
                            
                            //if(leadRecruiterObj.Id == )



                        }

                    }
                }

                catch
                {
                    Logger.Log(LogLevel.Error, "SendEmailSmsandWhatsappNew inside function - " + tempWorkpackagePush.leadUserInfo.SourceOwnerId + "Source Owner Details not found ");
                }


                errortrack += $"#send mail  #";
                try
                {
                    if (!string.IsNullOrEmpty(tempWorkpackagePush.leadUserInfo.email))
                    {
                        if (!configurationDetail.IsUpdatedSend)
                        {
                            if (notificationTemplate != null && !String.IsNullOrEmpty(notificationTemplate.Body))
                            {


                                sendMailStatus = await SendEmailTemplate(notificationTemplate.Body, notificationTemplate.Subject, configurationDetail.ConfigurationId, tempWorkpackagePush, title, notificationTemplate.Id, leadRecruiterObj);

                            }

                            string smsBody = string.Empty;

                            if (notificationTemplate != null && !String.IsNullOrEmpty(notificationTemplate.SMSText))
                            {

                                sendSmsStatus = await SendSMS(notificationTemplate.SMSText, configurationDetail.ConfigurationId, tempWorkpackagePush, title, leadRecruiterObj);


                            }

                        }
                        else
                        {

                            if (!string.IsNullOrEmpty(tempWorkpackagePush.LeadUserId))
                            {

                                if (configurationDetail.SendEmail.GetValueOrDefault() == true && !string.IsNullOrEmpty(configurationDetail.Body) && !string.IsNullOrEmpty(configurationDetail.Subject))
                                {

                                    sendMailStatus = await SendEmail(configurationDetail.Body, configurationDetail.Subject, configurationDetail.ConfigurationId, tempWorkpackagePush, title, leadRecruiterObj);
                                }

                                if (configurationDetail.SendSms.GetValueOrDefault() == true && !string.IsNullOrEmpty(configurationDetail.SMSText))
                                {
                                    sendSmsStatus = await SendSMS(configurationDetail.SMSText, configurationDetail.ConfigurationId, tempWorkpackagePush, title, leadRecruiterObj);
                                }
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, "SendEmailSmsNew inside function email Exception " + Id + " Exception-main error inner exception " + ex.InnerException.Message);
                    throw ex;
                }

                try
                {

                    using (var UOWObj = new AutomationUnitOfWork())
                    {
                        var workPackageInfo = UOWObj.WorkPackageInfoRepository.GetByID(Id);
                        workPackageInfo.ShotenUrlCode = string.IsNullOrWhiteSpace(workPackageInfo.ShotenUrlCode) ? shotenUrlCode : workPackageInfo.ShotenUrlCode;
                        workPackageInfo.EmailSentOn = sendMailStatus ? currentDate : default(DateTime?);
                        workPackageInfo.SMSSentOn = sendSmsStatus ? currentDate : default(DateTime?);
                        workPackageInfo.IsOurEndLogicForInvitation = true;
                        UOWObj.WorkPackageInfoRepository.Update(workPackageInfo);
                        UOWObj.Save();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, "SendEmailSmsNew error in updating workPackageInfoObj end Exception " + Id + " Exception-main error inner exception " + ex.Message);
                    Logger.Log(LogLevel.Error, "SendEmailSmsNew error in updating workPackageInfoObj end Exception " + Id + " Exception-main error inner exception " + ex.InnerException.Message);
                }

                errortrack += $"#succeed  #";
            }
            catch (Exception ex)
            {

                Logger.Log(LogLevel.Error, "SendEmailSmsNew outer function Exception #" + Id + "# quizdetailid " + tempWorkpackagePush.quizDetailid + "#Leadinfo :" + JsonConvert.SerializeObject(tempWorkpackagePush.leadUserInfo) + "# Exception-main error " + ex.Message);
                Logger.Log(LogLevel.Error, "SendEmailSmsNew outer function Exception track " + Id + " Exception-main error " + errortrack);
                if (ex.InnerException != null && ex.InnerException.Message != null)
                {
                    Logger.Log(LogLevel.Error, "SendEmailSmsNew inside function Exception " + Id + " Exception-main error inner exception " + ex.InnerException.Message);
                }
            }

        }


        public static async Task SendWhatsappNew(TempWorkpackagePush tempWorkpackagePush)
        {
            int Id = tempWorkpackagePush.workPackageInfoId;
            var shotenUrlCode = tempWorkpackagePush.shotenUrlCode;
            var shortUrl = tempWorkpackagePush.shortUrl;

            var title = string.Empty;
            var sendMailStatus = false;
            var sendSmsStatus = false;
            var sendWhatsappStatus = false;
            var currentDate = DateTime.UtcNow;
            string body = string.Empty;

            string emailLinkVariableForInvitation = string.Empty;
            Db.ConfigurationDetails configurationDetail = tempWorkpackagePush.ConfigurationDetails;
            Db.QuizDetails quizDetails = tempWorkpackagePush.QuizDetails;
            List<Db.VariablesDetails> variablesDetailList = null;
            List<Db.TemplateParameterInConfigurationDetails> templateParameterInConfigurationDetailList = null;
            OWCUserVariable leadRecruiterObj = null;

            var errortrack = "";
            try
            {
                #region Send WhatsappMessage

                if (configurationDetail.SendWhatsApp.GetValueOrDefault() == true && configurationDetail.HsmTemplateId != null && configurationDetail.HsmTemplateId > 0)
                {
                    if (!string.IsNullOrEmpty(tempWorkpackagePush.leadUserInfo.email))
                    {
                        title = ModifyQuizTitle(quizDetails.QuizTitle, tempWorkpackagePush.leadUserInfo, tempWorkpackagePush.LeadUserId, configurationDetail.QuizId, configurationDetail.Id);
                    }

                    await SendWhatsApp(configurationDetail.FollowUpMessage, configurationDetail.HsmTemplateId.Value, configurationDetail.HsmTemplateLanguageCode, configurationDetail.CompanyCode, tempWorkpackagePush, title);
                }
                else
                {
                    if (tempWorkpackagePush.UsageType == (int)UsageTypeEnum.WhatsAppChatbot || configurationDetail.ConfigurationType.EqualsCI("WHATSAPP_CHATBOT") || configurationDetail.ConfigurationType.EqualsCI("WHATSAPP_CHATBOT_TEMPLATE"))
                    {
                        dynamic requestObject = new ExpandoObject();
                        requestObject.ClientCode = tempWorkpackagePush.ConfigurationDetails.CompanyCode;
                        requestObject.ContactPhone = tempWorkpackagePush.leadUserInfo.telephone;
                        requestObject.AutomationConfigId = configurationDetail.ConfigurationId;
                        requestObject.UserId = tempWorkpackagePush.leadUserInfo.contactId;
                        requestObject.WorkPackageInfoId = tempWorkpackagePush.workPackageInfoId;
                        requestObject.ModuleName = "Automation";
                        requestObject.EventType = TaskTypeEnum.Invitation.ToString();
                        requestObject.ObjectId = tempWorkpackagePush.workPackageInfoId.ToString();
                        requestObject.UniqueCode = Guid.NewGuid().ToString();

                        var clientCodeValue = tempWorkpackagePush.ConfigurationDetails?.CompanyCode;

                        if (GlobalSettings.EnableWhatsApptempRedirection) {
                            if (clientCodeValue.ContainsCI("SFMine") || clientCodeValue.ContainsCI("AABHSNTEJH") || clientCodeValue.ContainsCI("AABHUX6DCI") || clientCodeValue.ContainsCI("SFMADEV")) {
                                requestObject.ClientCode = "AABCGFVDCC";
                            }
                        }


                        sendWhatsappStatus = await CommunicationHelper.SendAutomationChatbotStartAsnc(requestObject);

                        if (sendWhatsappStatus)
                        {

                            using (var UOWObj = new AutomationUnitOfWork())
                            {
                                var workPackageInfo = UOWObj.WorkPackageInfoRepository.GetByID(tempWorkpackagePush.workPackageInfoId);
                                workPackageInfo.ShotenUrlCode = string.IsNullOrWhiteSpace(workPackageInfo.ShotenUrlCode) ? tempWorkpackagePush.shotenUrlCode : workPackageInfo.ShotenUrlCode;
                                workPackageInfo.WhatsappSentOn = sendWhatsappStatus ? DateTime.UtcNow : default(DateTime?);
                                UOWObj.WorkPackageInfoRepository.Update(workPackageInfo);
                                UOWObj.Save();
                            }

                        }
                    }
                }
                #endregion


            }
            catch (Exception ex)
            {

                Logger.Log(LogLevel.Error, "SendWhatsappNew inside function Exception #" + Id + "# quizdetailid " + tempWorkpackagePush.quizDetailid + "#Leadinfo :" + JsonConvert.SerializeObject(tempWorkpackagePush.leadUserInfo) + "# Exception-main error " + ex.Message);
                Logger.Log(LogLevel.Error, "SendWhatsappNew inside function Exception track " + Id + " Exception-main error " + errortrack);
                if (ex.InnerException != null && ex.InnerException.Message != null)
                {
                    Logger.Log(LogLevel.Error, "SendWhatsappNew inside function Exception " + Id + " Exception-main error inner exception " + ex.InnerException.Message);
                }
            }

        }

        #region "Oldcode"
        //public static void SendEmailSmsandWhatsappHandler(int Id, Response.OWCLeadUserResponse.LeadUserResponse leadUserInfo, Db.QuizDetails quizDetails, IEnumerable<Db.VariablesDetails> variablesDetailList, Company companyObj)
        //{
        //    try
        //    {
        //        Task.Run(() =>
        //        {
        //            SendEmailSmsandWhatsapp(Id, leadUserInfo, quizDetails, variablesDetailList, companyObj);
        //        });
        //    }
        //    catch (Exception ex)
        //    {

        //        Logger.Log(LogLevel.Error, "SendEmailSmsandWhatsapp inside function error - " + Id + " Exception-" + ex.Message);
        //    }
        //}

        //static void SendEmailSmsandWhatsapp(int Id, Response.OWCLeadUserResponse.LeadUserResponse leadUserInfo, Db.QuizDetails quizDetails, IEnumerable<Db.VariablesDetails> variablesDetailList, Company companyObj)
        //{
        //    try
        //    {
        //        using (var UOWObj = new AutomationUnitOfWork())
        //        {
        //            try
        //            {

        //                var currentDate = DateTime.UtcNow;

        //                var workPackageInfoObj = UOWObj.WorkPackageInfoRepository.GetByID(Id);

        //                var configurationDetail = workPackageInfoObj.ConfigurationDetails;

        //                var quizObj = quizDetails.Quiz;

        //                #region Send Email and SMS

        //                var title = string.Empty;
        //                var shortUrl = string.Empty;
        //                var sendMailStatus = false;
        //                var sendSmsStatus = false;
        //                var sendWhatsappStatus = false;

        //                var template = UOWObj.NotificationTemplatesInQuizRepository.Get(r => r.NotificationTemplate.Status == (int)StatusEnum.Active && r.QuizId == quizObj.Id && r.NotificationTemplate.NotificationType == (int)NotificationTypeEnum.INVITATION).FirstOrDefault();
        //                Logger.Log(LogLevel.Error, "SendEmailSmsandWhatsapp inside function send email- " + leadUserInfo.email + " Exception-" + leadUserInfo.contactId);
        //                if (!string.IsNullOrEmpty(leadUserInfo.email))
        //                {
        //                    string body = string.Empty;

        //                    title = quizDetails.QuizTitle.Replace("%fname%", leadUserInfo.firstName)
        //                                                         .Replace("%lname%", leadUserInfo.lastName)
        //                                                         .Replace("%phone%", leadUserInfo.telephone)
        //                                                         .Replace("%email%", leadUserInfo.email)
        //                                                         .Replace("%leadid%", workPackageInfoObj.LeadUserId)
        //                                                         .Replace("%qname%", string.Empty)
        //                                                         .Replace("%qlink%", string.Empty)
        //                                                         .Replace("%qendresult%", string.Empty)
        //                                                         .Replace("%correctanswerexplanation%", string.Empty)
        //                                                         .Replace("%signature%", string.Empty);

        //                    MatchCollection mcol = Regex.Matches(title, @"%\b\S+?\b%");

        //                    foreach (Match m in mcol)
        //                    {
        //                        var variablesDetailObj = variablesDetailList.FirstOrDefault(t => t.VariableInQuiz.Variables.Name == m.ToString().ToLower().Replace("%", string.Empty));
        //                        if (variablesDetailObj != null)
        //                        {
        //                            title = title.Replace(m.ToString(), variablesDetailObj.VariableValue);
        //                        }
        //                        else
        //                        {
        //                            title = title.Replace(m.ToString(), string.Empty);
        //                        }
        //                    }

        //                    var shotenUrlCode = string.Empty;

        //                    #region url shortning by custom domain

        //                    //var domainList = OWCHelper.GetClientDomains(companyObj);
        //                    var domainList = CommonStaticData.GetCachedClientDomains(companyObj);
        //                    var domain = (domainList != null && domainList.Any()) ? domainList.FirstOrDefault().Domain : GlobalSettings.defaultDomain.ToString();

        //                    if (template != null && !string.IsNullOrEmpty(template.NotificationTemplate.EmailLinkVariable))
        //                        shotenUrlCode = template.NotificationTemplate.EmailLinkVariable + "-" + IdGenerator.GetShortCode();
        //                    else
        //                        shotenUrlCode = IdGenerator.GetShortCode();

        //                    #region insert QuizUrlSetting

        //                    var quizUrlSettingObj = new Db.QuizUrlSetting();

        //                    quizUrlSettingObj.Key = shotenUrlCode;
        //                    quizUrlSettingObj.DomainName = domain;
        //                    quizUrlSettingObj.Value = GlobalSettings.webUrl.ToString() + "/quiz?Code=" + quizObj.PublishedCode + "&UserTypeId=2&UserId=" + workPackageInfoObj.LeadUserId + "&WorkPackageInfoId=" + workPackageInfoObj.Id;

        //                    UOWObj.QuizUrlSettingRepository.Insert(quizUrlSettingObj);
        //                    UOWObj.Save();

        //                    shortUrl = "https://" + quizUrlSettingObj.DomainName + "/" + quizUrlSettingObj.Key;

        //                    #endregion

        //                    #region update ShotenUrlCode in WorkPackageInfo

        //                    workPackageInfoObj.ShotenUrlCode = shotenUrlCode;

        //                    workPackageInfoObj.IsOurEndLogicForInvitation = true;

        //                    if (template != null && !string.IsNullOrEmpty(template.NotificationTemplate.EmailLinkVariable))
        //                        workPackageInfoObj.EmailLinkVariableForInvitation = template.NotificationTemplate.EmailLinkVariable;

        //                    UOWObj.WorkPackageInfoRepository.Update(workPackageInfoObj);
        //                    UOWObj.Save();

        //                    #endregion

        //                    #endregion

        //                    if (!string.IsNullOrWhiteSpace(shortUrl))
        //                    {
        //                        PrerenderHelper.RecachePrerenderedPage(shortUrl);
        //                    }

        //                    if (!configurationDetail.IsUpdatedSend)
        //                    {
        //                        if (template != null && !String.IsNullOrEmpty(template.NotificationTemplate.Body))
        //                        {
        //                            var notificationTemplate = template.NotificationTemplate;
        //                            body = notificationTemplate.Body.Replace("%fname%", leadUserInfo.firstName)
        //                                                                     .Replace("%lname%", leadUserInfo.lastName)
        //                                                                     .Replace("%phone%", leadUserInfo.telephone)
        //                                                                     .Replace("%email%", leadUserInfo.email)
        //                                                                     .Replace("%leadid%", workPackageInfoObj.LeadUserId)
        //                                                                     .Replace("%qname%", title)
        //                                                                     .Replace("%qtype%", ((QuizTypeEnum)quizObj.QuizType).ToString())
        //                                                                     .Replace("%qlink%", "<a href=\"" + shortUrl + "\">" + shortUrl + "</a>")
        //                                                                     .Replace("%sourcename%", String.IsNullOrEmpty(workPackageInfoObj.CampaignName) ? string.Empty : workPackageInfoObj.CampaignName)
        //                                                                     .Replace("%jrleadid%", workPackageInfoObj.LeadUserId)
        //                                                                     .Replace("%jrconfigurationId%", configurationDetail.ConfigurationId)
        //                                                                     .Replace("%jrquizId%", quizObj.Id.ToString());

        //                            if (notificationTemplate.Body.Contains("%signature%"))
        //                            {
        //                                var signatureDetails = CommonStaticData.GetCachedEmailSignature(companyObj.ClientCode, "group");
        //                                body = body.Replace("%signature%", signatureDetails != null ? signatureDetails.signatureText ?? string.Empty : string.Empty);
        //                            }

        //                            Logger.Log(LogLevel.Error, "SendEmailSmsandWhatsapp inside function CommunicationHelper.SendMailWithAttachment start - " + leadUserInfo.email + " Exception-" + workPackageInfoObj.LeadUserId);

        //                                sendMailStatus = CommunicationHelper.SendMailWithAttachment(leadUserInfo.email, String.IsNullOrEmpty(template.NotificationTemplate.Subject) ? string.Empty : template.NotificationTemplate.Subject.Replace
        //         ("%qname%", title).Replace("%sourcename%", String.IsNullOrEmpty(workPackageInfoObj.CampaignName) ? string.Empty : workPackageInfoObj.CampaignName), body, companyObj.ClientCode, (template.NotificationTemplate.AttachmentsInNotificationTemplate != null && template.NotificationTemplate.AttachmentsInNotificationTemplate.Any()) ? template.NotificationTemplate.AttachmentsInNotificationTemplate.Select(r => new Models.FileAttachment { FileLink = r.Description, FileName = r.Title }).ToList() : null);

        //                                Logger.Log(LogLevel.Error, "error in CommunicationHelper.SendMailWithAttachment done - " + leadUserInfo.email + " Exception-" + workPackageInfoObj.LeadUserId);

        //                        }

        //                        string smsBody = string.Empty;

        //                        if (template != null && !String.IsNullOrEmpty(template.NotificationTemplate.SMSText))
        //                        {
        //                            smsBody = template.NotificationTemplate.SMSText.Replace("%fname%", leadUserInfo.firstName)
        //                                                                           .Replace("%lname%", leadUserInfo.lastName)
        //                                                                           .Replace("%phone%", leadUserInfo.telephone)
        //                                                                           .Replace("%email%", leadUserInfo.email)
        //                                                                           .Replace("%leadid%", workPackageInfoObj.LeadUserId)
        //                                                                           .Replace("%qname%", title)
        //                                                                           .Replace("%qtype%", ((QuizTypeEnum)quizObj.QuizType).ToString())
        //                                                                           .Replace("%qlink%", shortUrl)
        //                                                                           .Replace("%sourcename%", String.IsNullOrEmpty(workPackageInfoObj.CampaignName) ? string.Empty : workPackageInfoObj.CampaignName)
        //                                                                           .Replace("%signature%", string.Empty)
        //                                                                           .Replace("%jrleadid%", workPackageInfoObj.LeadUserId)
        //                                                                           .Replace("%jrconfigurationId%", configurationDetail.ConfigurationId)
        //                                                                           .Replace("%jrquizId%", quizObj.Id.ToString());

        //                            sendSmsStatus = CommunicationHelper.SendSMS(leadUserInfo.telephone, smsBody, companyObj);
        //                        }


        //                    }
        //                    else
        //                    {
        //                        if (!string.IsNullOrEmpty(workPackageInfoObj.LeadUserId))
        //                        {
        //                            if (configurationDetail.SendEmail.GetValueOrDefault() == true && !string.IsNullOrEmpty(configurationDetail.Body) && !string.IsNullOrEmpty(configurationDetail.Subject))
        //                            {
        //                                body = configurationDetail.Body.Replace("&lt;", "<")
        //                                                  .Replace("&gt;", " >")
        //                                                  .Replace("%fname%", leadUserInfo.firstName)
        //                                                  .Replace("%lname%", leadUserInfo.lastName)
        //                                                  .Replace("%phone%", leadUserInfo.telephone)
        //                                                  .Replace("%email%", leadUserInfo.email)
        //                                                  .Replace("%leadid%", workPackageInfoObj.LeadUserId)
        //                                                  .Replace("%qname%", title)
        //                                                  .Replace("%qtype%", ((QuizTypeEnum)quizObj.QuizType).ToString())
        //                                                  .Replace("%qlink%", "<a href=\"" + shortUrl + "\">" + shortUrl + "</a>")
        //                                                  .Replace("%sourcename%", String.IsNullOrEmpty(workPackageInfoObj.CampaignName) ? string.Empty : workPackageInfoObj.CampaignName)
        //                                                  .Replace("%jrleadid%", workPackageInfoObj.LeadUserId)
        //                                                  .Replace("%jrconfigurationId%", configurationDetail.ConfigurationId)
        //                                                  .Replace("%jrquizId%", quizObj.Id.ToString());


        //                                if (configurationDetail.Body.Contains("%signature%"))
        //                                {
        //                                    var signatureDetails = CommonStaticData.GetCachedEmailSignature(companyObj.ClientCode, "group");
        //                                    body = body.Replace("%signature%", signatureDetails != null ? signatureDetails.signatureText ?? string.Empty : string.Empty);
        //                                }

        //                                sendMailStatus = CommunicationHelper.SendMailWithAttachment(leadUserInfo.email, String.IsNullOrEmpty(configurationDetail.Subject) ? string.Empty : configurationDetail.Subject.Replace("%qname%", title).Replace("%sourcename%", string.IsNullOrEmpty(workPackageInfoObj.CampaignName) ? string.Empty : workPackageInfoObj.CampaignName), body, companyObj.ClientCode,
        //                                    (configurationDetail.AttachmentsInConfiguration != null && configurationDetail.AttachmentsInConfiguration.Any()) ? configurationDetail.AttachmentsInConfiguration.Select(r => new Models.FileAttachment { FileLink = r.FileLink, FileName = r.FileName }).ToList() : null);
        //                            }

        //                            if (configurationDetail.SendSms.GetValueOrDefault() == true && !string.IsNullOrEmpty(configurationDetail.SMSText))
        //                            {
        //                                var smsBody = configurationDetail.SMSText.Replace("%fname%", leadUserInfo.firstName)
        //                                                               .Replace("%lname%", leadUserInfo.lastName)
        //                                                               .Replace("%phone%", leadUserInfo.telephone)
        //                                                               .Replace("%email%", leadUserInfo.email)
        //                                                               .Replace("%leadid%", workPackageInfoObj.LeadUserId)
        //                                                               .Replace("%qname%", title)
        //                                                               .Replace("%qtype%", ((QuizTypeEnum)quizObj.QuizType).ToString())
        //                                                               .Replace("%qlink%", shortUrl)
        //                                                               .Replace("%sourcename%", String.IsNullOrEmpty(workPackageInfoObj.CampaignName) ? string.Empty : workPackageInfoObj.CampaignName)
        //                                                               .Replace("%signature%", string.Empty)
        //                                                               .Replace("%jrleadid%", workPackageInfoObj.LeadUserId)
        //                                                               .Replace("%jrconfigurationId%", configurationDetail.ConfigurationId)
        //                                                               .Replace("%jrquizId%", quizObj.Id.ToString());

        //                                sendSmsStatus = CommunicationHelper.SendSMS(leadUserInfo.telephone, smsBody, companyObj);
        //                            }
        //                        }
        //                    }
        //                }

        //                Logger.Log(LogLevel.Error, "SendEmailSmsandWhatsapp inside function send email- " + leadUserInfo.email + " Exception-" + leadUserInfo.contactId);

        //                #endregion

        //                #region Send WhatsappMessage

        //                if (configurationDetail.SendWhatsApp.GetValueOrDefault() == true && configurationDetail.HsmTemplateId != null && configurationDetail.HsmTemplateId > 0)
        //                {
        //                    var followUpMessage = configurationDetail.FollowUpMessage.Replace("%fname%", leadUserInfo.firstName)
        //                                                                     .Replace("%lname%", leadUserInfo.lastName)
        //                                                                     .Replace("%phone%", leadUserInfo.telephone)
        //                                                                     .Replace("%email%", leadUserInfo.email)
        //                                                                     .Replace("%leadid%", workPackageInfoObj.LeadUserId)
        //                                                                     .Replace("%qname%", title)
        //                                                                     .Replace("%qtype%", ((QuizTypeEnum)quizObj.QuizType).ToString())
        //                                                                     .Replace("%qlink%", shortUrl)
        //                                                                     .Replace("%sourcename%", String.IsNullOrEmpty(workPackageInfoObj.CampaignName) ? string.Empty : workPackageInfoObj.CampaignName)
        //                                                                     .Replace("%qendresult%", string.Empty)
        //                                                                     .Replace("%correctanswerexplanation%", string.Empty)
        //                                                                     .Replace("%signature%", string.Empty);

        //                    var whatsappMessageObj = new WhatsappMessage();
        //                    whatsappMessageObj.ClientCode = configurationDetail.CompanyCode;
        //                    whatsappMessageObj.ContactPhone = leadUserInfo.telephone;
        //                    whatsappMessageObj.HsmTemplateId = configurationDetail.HsmTemplateId.Value;
        //                    whatsappMessageObj.LanguageCode = configurationDetail.HsmTemplateLanguageCode;
        //                    whatsappMessageObj.FollowUpMessage = followUpMessage;
        //                    whatsappMessageObj.ModuleWorkPackageId = workPackageInfoObj.Id;
        //                    whatsappMessageObj.TemplateParameters = new List<WhatsappMessage.TemplateParameter>();

        //                    foreach (var item in configurationDetail.TemplateParameterInConfigurationDetails.ToList())
        //                    {
        //                        whatsappMessageObj.TemplateParameters.Add(new WhatsappMessage.TemplateParameter()
        //                        {
        //                            paraname = item.ParaName,
        //                            position = item.Position,
        //                            value = item.Value.Replace("%fname%", leadUserInfo.firstName)
        //                                              .Replace("%lname%", leadUserInfo.lastName)
        //                                              .Replace("%phone%", leadUserInfo.telephone)
        //                                              .Replace("%email%", leadUserInfo.email)
        //                                              .Replace("%leadid%", workPackageInfoObj.LeadUserId)
        //                                              .Replace("%qname%", title)
        //                                              .Replace("%qtype%", ((QuizTypeEnum)quizObj.QuizType).ToString())
        //                                              .Replace("%qlink%", shortUrl)
        //                                              .Replace("%sourcename%", String.IsNullOrEmpty(workPackageInfoObj.CampaignName) ? string.Empty : workPackageInfoObj.CampaignName)
        //                                              .Replace("%qendresult%", string.Empty)
        //                                              .Replace("%correctanswerexplanation%", string.Empty)
        //                                              .Replace("%signature%", string.Empty)
        //                        });
        //                    }

        //                    sendWhatsappStatus = CommunicationHelper.SendWhatsappMessage(whatsappMessageObj);

        //                }

        //                #endregion

        //                workPackageInfoObj.EmailSentOn = sendMailStatus ? currentDate : default(DateTime?);
        //                workPackageInfoObj.SMSSentOn = sendSmsStatus ? currentDate : default(DateTime?);
        //                workPackageInfoObj.WhatsappSentOn = sendWhatsappStatus ? currentDate : default(DateTime?);
        //                UOWObj.WorkPackageInfoRepository.Update(workPackageInfoObj);
        //                UOWObj.Save();
        //            }
        //            catch (Exception ex)
        //            {
        //                throw ex;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Log(LogLevel.Error, "SendEmailSmsandWhatsapp inside function Exception " + Id + " Exception-" + ex.Message);
        //    }


        //    Logger.Log(LogLevel.Error, "SendEmailSmsandWhatsapp inside function End " + Id + " Exception-" + leadUserInfo.contactId);
        //}        

        #endregion


        private static async Task<bool> SendSMS(string smsText, string ConfigurationId, TempWorkpackagePush tempWorkpackagePush, string title, OWCUserVariable leadRecruiterObj)
        {
            var sendSmsStatus = false;
            if (!string.IsNullOrEmpty(smsText))
            {
                Dictionary<string, object> staticobjects = new Dictionary<string, object>();

                staticobjects.Add("%leadid%", tempWorkpackagePush.LeadUserId);
                staticobjects.Add("%jrleadid%", tempWorkpackagePush.LeadUserId);
                staticobjects.Add("%qname%", String.IsNullOrEmpty(title) ? string.Empty : title);
                staticobjects.Add("%jrconfigurationId%", String.IsNullOrEmpty(ConfigurationId) ? string.Empty : ConfigurationId);
                staticobjects.Add("%qtype%", ((QuizTypeEnum)tempWorkpackagePush.quizType).ToString());
                staticobjects.Add("%qlink%", tempWorkpackagePush.shortUrl);
                staticobjects.Add("%sourcename%", String.IsNullOrEmpty(tempWorkpackagePush.WorkpackageCampaignName) ? string.Empty : tempWorkpackagePush.WorkpackageCampaignName);
                staticobjects.Add("%qendresult%", string.Empty);
                staticobjects.Add("%correctanswerexplanation%", string.Empty);
                staticobjects.Add("%jrquizId%", tempWorkpackagePush.parentquizid.ToString());
                staticobjects.Add("%rfname%", (leadRecruiterObj != null && !String.IsNullOrEmpty(leadRecruiterObj.firstName)) ? leadRecruiterObj.firstName : string.Empty);
                staticobjects.Add("%rphone%", (leadRecruiterObj != null && !String.IsNullOrEmpty(leadRecruiterObj.phoneNumber)) ? leadRecruiterObj.phoneNumber : string.Empty);
                staticobjects.Add("%recruiter%", (leadRecruiterObj != null && !String.IsNullOrEmpty(leadRecruiterObj.firstName)) ? leadRecruiterObj.firstName : string.Empty);
                staticobjects.Add("%signature%", string.Empty);

                CommonStaticData.VacancyVariableLink(tempWorkpackagePush.ContactObject, smsText, tempWorkpackagePush.companyObj.ClientCode);
                if (!string.IsNullOrWhiteSpace(tempWorkpackagePush.UserToken))
                {
                    CommonStaticData.UserVariableLink(tempWorkpackagePush.ContactObject, smsText, tempWorkpackagePush.UserToken, tempWorkpackagePush.companyObj.ClientCode);
                }

                var smsBody = DynamicExtensions.DynamicVariablesReplace(smsText, tempWorkpackagePush.ContactObject, staticobjects, tempWorkpackagePush.ConfigurationDetails.MsgVariables);
                #region "Oldcode"
                //var smsBody = smsText.Replace("%fname%", tempWorkpackagePush.leadUserInfo.firstName)
                //                               .Replace("%lname%", tempWorkpackagePush.leadUserInfo.lastName)
                //                               .Replace("%phone%", tempWorkpackagePush.leadUserInfo.telephone)
                //                               .Replace("%email%", tempWorkpackagePush.leadUserInfo.email)
                //                               .Replace("%leadid%", tempWorkpackagePush.LeadUserId)
                //                               .Replace("%qname%", title)
                //                               .Replace("%qtype%", ((QuizTypeEnum)tempWorkpackagePush.quizType).ToString())
                //                               .Replace("%qlink%", tempWorkpackagePush.shortUrl)
                //                               .Replace("%sourcename%", String.IsNullOrEmpty(tempWorkpackagePush.WorkpackageCampaignName) ? string.Empty : tempWorkpackagePush.WorkpackageCampaignName)
                //                               .Replace("%signature%", string.Empty)
                //                               .Replace("%jrleadid%", tempWorkpackagePush.LeadUserId)
                //                               .Replace("%jrconfigurationId%", ConfigurationId)
                //                               .Replace("%rfname%", (leadRecruiterObj != null && !String.IsNullOrEmpty(leadRecruiterObj.firstName)) ? leadRecruiterObj.firstName : string.Empty)
                //                               .Replace("%rphone%", (leadRecruiterObj != null && !String.IsNullOrEmpty(leadRecruiterObj.phoneNumber)) ? leadRecruiterObj.phoneNumber : string.Empty)
                //                               .Replace("%jrquizId%", tempWorkpackagePush.parentquizid.ToString());
                #endregion
                sendSmsStatus = await CommunicationHelper.SendSMSAsync(tempWorkpackagePush.leadUserInfo.telephone, smsBody, tempWorkpackagePush.companyObj);
            }

            return sendSmsStatus;
        }

       
        private static async Task<bool> SendWhatsApp(string followUpMessage, int hsmTemplateId, string hsmTemplateLanguageCode, string companyCode, TempWorkpackagePush tempWorkpackagePush, string title)
        {
            var sendWhatsappStatus = false;
            List<Db.TemplateParameterInConfigurationDetails> templateParameterInConfigurationDetails = null;
            OWCUserVariable leadRecruiterObj = new OWCUserVariable();
            try
            {

                if (templateParameterInConfigurationDetails == null)
                {
                    ErrorLog.LogError(new Exception("#configurationDetailForWhatsappParam :" + tempWorkpackagePush.ConfigurationDetailId));
                    using (var UOWObj = new AutomationUnitOfWork())
                    {

                        templateParameterInConfigurationDetails = UOWObj.TemplateParameterInConfigurationDetailsRepository.GetTemplateParameterInConfigurationDetailsByConfigurationId(tempWorkpackagePush.ConfigurationDetailId).ToList();
                    }

                    if (templateParameterInConfigurationDetails == null)
                    {
                        ErrorLog.LogError(new Exception("#configurationDetailForWhatsappParam 22:" + tempWorkpackagePush.ConfigurationDetailId));
                    }
                }


                var userdetails = new List<OWCUserVariable>();
                long[] Userids = new long[1];

                if (!string.IsNullOrWhiteSpace(tempWorkpackagePush.leadUserInfo?.contactId) && !tempWorkpackagePush.leadUserInfo.contactId.ContainsCI("SF-"))
                {
                    if (!string.IsNullOrWhiteSpace(tempWorkpackagePush.leadUserInfo.SourceOwnerId))
                    {
                        long sourceOWnerId = Convert.ToInt64(tempWorkpackagePush.leadUserInfo.SourceOwnerId);
                        if (sourceOWnerId > 0)
                        {
                            Userids[0] = sourceOWnerId;
                        }
                        else
                        {
                            long contactOwnerId = Convert.ToInt64(tempWorkpackagePush.leadUserInfo.ContactOwnerId);
                            if (contactOwnerId > 0)
                            {
                                Userids[0] = contactOwnerId;
                            }


                        }
                    }

                    if (Userids.Any())
                    {
                        userdetails = OWCHelper.GetUserListUsingUserId(Userids, tempWorkpackagePush.companyObj).ToList();
                    }

                    if (userdetails != null && userdetails.Any())
                    {
                        leadRecruiterObj = userdetails.FirstOrDefault();
                    }

                    if (leadRecruiterObj == null)
                    {
                        leadRecruiterObj = new OWCUserVariable();
                    }
                }

                else
                {
                    if (!string.IsNullOrWhiteSpace(tempWorkpackagePush.leadUserInfo?.SourceOwnerId) || !string.IsNullOrWhiteSpace(tempWorkpackagePush.leadUserInfo?.ContactOwnerId) || !string.IsNullOrWhiteSpace(tempWorkpackagePush.leadUserInfo?.LeadOwnerId))
                    {
                        var externalDetails = _owchelper.GetExternalDetails(tempWorkpackagePush.companyObj.ClientCode, tempWorkpackagePush.leadUserInfo.SourceOwnerId);
                        if (externalDetails != null)
                        {
                            var oWCBusinessUserResponse = JsonConvert.DeserializeObject<OWCUserVariable>(externalDetails);
                            if (oWCBusinessUserResponse != null)
                            {
                                leadRecruiterObj = oWCBusinessUserResponse;
                            }
                        }
                        if (leadRecruiterObj == null) {
                            var externalDetails2 = _owchelper.GetExternalDetails(tempWorkpackagePush.companyObj.ClientCode, tempWorkpackagePush.leadUserInfo.LeadOwnerId);
                            if (externalDetails2 != null) {
                                var oWCBusinessUserResponse = JsonConvert.DeserializeObject<OWCUserVariable>(externalDetails2);
                                if (oWCBusinessUserResponse != null) {
                                    leadRecruiterObj = oWCBusinessUserResponse;
                                }
                            }
                        }

                        if (leadRecruiterObj == null)
                        {
                            var externalDetails2 = _owchelper.GetExternalDetails(tempWorkpackagePush.companyObj.ClientCode, tempWorkpackagePush.leadUserInfo.ContactOwnerId);
                            if (externalDetails2 != null)
                            {
                                    var oWCBusinessUserResponse = JsonConvert.DeserializeObject<OWCUserVariable>(externalDetails2);
                                    if (oWCBusinessUserResponse != null)
                                    {
                                        leadRecruiterObj = oWCBusinessUserResponse;
                                    }
                            }
                        }
                    }
                }

               

                #region Send WhatsappMessage

                if (hsmTemplateId > 0)
                {
                    Dictionary<string, object> staticobjects = WhatsAppStaticVariables(tempWorkpackagePush, title, leadRecruiterObj);

                    sendWhatsappStatus = WhatsaAppCommunicationHelper.SendWhatsapp(tempWorkpackagePush.leadUserInfo.telephone, tempWorkpackagePush.ContactObject, staticobjects, followUpMessage, tempWorkpackagePush.ConfigurationDetails.MsgVariables, hsmTemplateId, hsmTemplateLanguageCode, tempWorkpackagePush.workPackageInfoId, tempWorkpackagePush.UserToken, companyCode, templateParameterInConfigurationDetails, tempWorkpackagePush);

                   
                    if (sendWhatsappStatus)
                    {

                        using (var UOWObj = new AutomationUnitOfWork())
                        {
                            var workPackageInfo = UOWObj.WorkPackageInfoRepository.GetByID(tempWorkpackagePush.workPackageInfoId);
                            workPackageInfo.ShotenUrlCode = string.IsNullOrWhiteSpace(workPackageInfo.ShotenUrlCode) ? tempWorkpackagePush.shotenUrlCode : workPackageInfo.ShotenUrlCode;
                            workPackageInfo.WhatsappSentOn = sendWhatsappStatus ? DateTime.UtcNow : default(DateTime?);
                            UOWObj.WorkPackageInfoRepository.Update(workPackageInfo);
                            UOWObj.Save();
                        }

                    }
                }

                #endregion

            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "SendEmailSmsandWhatsapp SendWhatsApp exception  " + tempWorkpackagePush.workPackageInfoId + " Exception-main error inner exception " + ex.Message);
                Logger.Log(LogLevel.Error, "SendEmailSmsandWhatsapp SendWhatsApp inner exception  " + tempWorkpackagePush.workPackageInfoId + " Exception-main error inner exception " + ex.InnerException.Message);
            }


            return sendWhatsappStatus;
        }

        private static Dictionary<string, object> WhatsAppStaticVariables(TempWorkpackagePush tempWorkpackagePush, string title, OWCUserVariable leadRecruiterObj)
        {
            Dictionary<string, object> staticobjects = new Dictionary<string, object>();


            staticobjects.Add("%leadid%", tempWorkpackagePush.LeadUserId);
            staticobjects.Add("%qname%", String.IsNullOrEmpty(title) ? string.Empty : title);
            staticobjects.Add("%qtype%", ((QuizTypeEnum)tempWorkpackagePush.quizType).ToString());
            staticobjects.Add("%qlink%", tempWorkpackagePush.shortUrl);
            staticobjects.Add("%qLinkKey%", tempWorkpackagePush.qLinkKey);
            staticobjects.Add("%sourcename%", String.IsNullOrEmpty(tempWorkpackagePush.WorkpackageCampaignName) ? string.Empty : tempWorkpackagePush.WorkpackageCampaignName);
            staticobjects.Add("%qendresult%", string.Empty);
            staticobjects.Add("%correctanswerexplanation%", string.Empty);
            staticobjects.Add("%rfname%", (leadRecruiterObj != null && !String.IsNullOrEmpty(leadRecruiterObj.firstName)) ? leadRecruiterObj.firstName : string.Empty);
            staticobjects.Add("%rphone%", (leadRecruiterObj != null && !String.IsNullOrEmpty(leadRecruiterObj.phoneNumber)) ? leadRecruiterObj.phoneNumber : string.Empty);
            staticobjects.Add("%recruiter%", (leadRecruiterObj != null && !String.IsNullOrEmpty(leadRecruiterObj.firstName)) ? leadRecruiterObj.firstName : string.Empty);
            staticobjects.Add("%signature%", string.Empty);
            return staticobjects;
        }

        private static async Task<bool> SendEmail(string body, string subject, string configurationId, TempWorkpackagePush tempWorkpackagePush, string title, OWCUserVariable leadRecruiterObj)
        {
            var sendMailStatus = false;
            string userToken = string.Empty;
            if (!string.IsNullOrEmpty(body) && !string.IsNullOrEmpty(subject))
            {

                if (body.Contains("%signature%"))
                {
                    var signatureDetails = CommonStaticData.GetCachedEmailSignature(tempWorkpackagePush.companyObj.ClientCode, "group");
                    body = body.Replace("%signature%", signatureDetails != null ? signatureDetails.signatureText ?? string.Empty : string.Empty);
                }


                if (!String.IsNullOrEmpty(subject))
                {
                    string campaignname = string.Empty;
                    if (!string.IsNullOrEmpty(tempWorkpackagePush.WorkpackageCampaignName))
                    {
                        campaignname = tempWorkpackagePush.WorkpackageCampaignName;
                    }

                    subject = subject.Replace("%qname%", title).Replace("%sourcename%", campaignname);
                }

                Dictionary<string, object> staticobjects = StaticVariablesEmail(configurationId, tempWorkpackagePush, title, leadRecruiterObj);

                CommonStaticData.VacancyVariableLink(tempWorkpackagePush.ContactObject, body, tempWorkpackagePush.companyObj.ClientCode);
                if (!string.IsNullOrEmpty(tempWorkpackagePush.UserToken))
                {
                    userToken = tempWorkpackagePush.UserToken;
                    CommonStaticData.UserVariableLink(tempWorkpackagePush.ContactObject, body, userToken, tempWorkpackagePush.companyObj.ClientCode);
                }

                body = DynamicExtensions.DynamicVariablesReplace(body, tempWorkpackagePush.ContactObject, staticobjects, tempWorkpackagePush.ConfigurationDetails.MsgVariables);
                
                #region "Oldcode"
                //body = body.Replace("&lt;", "<")
                //                  .Replace("&gt;", " >")
                //                  .Replace("%fname%", tempWorkpackagePush.leadUserInfo.firstName)
                //                  .Replace("%lname%", tempWorkpackagePush.leadUserInfo.lastName)
                //                  .Replace("%phone%", tempWorkpackagePush.leadUserInfo.telephone)
                //                  .Replace("%email%", tempWorkpackagePush.leadUserInfo.email)
                //                  .Replace("%leadid%", tempWorkpackagePush.LeadUserId)
                //                  .Replace("%qname%", title)
                //                  .Replace("%qtype%", ((QuizTypeEnum)tempWorkpackagePush.quizType).ToString())
                //                  .Replace("%qlink%", "<a href=\"" + tempWorkpackagePush.shortUrl + "\">" + tempWorkpackagePush.shortUrl + "</a>")
                //                  .Replace("%sourcename%", String.IsNullOrEmpty(tempWorkpackagePush.WorkpackageCampaignName) ? string.Empty : tempWorkpackagePush.WorkpackageCampaignName)
                //                  .Replace("%jrleadid%", tempWorkpackagePush.LeadUserId)
                //                  .Replace("%jrconfigurationId%", configurationId)
                //                  .Replace("%rfname%", (leadRecruiterObj != null && !String.IsNullOrEmpty(leadRecruiterObj.firstName)) ? leadRecruiterObj.firstName : string.Empty)
                //                  .Replace("%rphone%", (leadRecruiterObj != null && !String.IsNullOrEmpty(leadRecruiterObj.phoneNumber)) ? leadRecruiterObj.phoneNumber : string.Empty)
                //                  .Replace("%jrquizId%", tempWorkpackagePush.parentquizid.ToString());
                #endregion

                List<Models.FileAttachment> attachmentlist = null;

                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var configurationAttachment = UOWObj.AttachmentsInConfigurationRepository.GetAttachmentsInConfigurationByConfigurationId(tempWorkpackagePush.ConfigurationDetailId);
                    if (configurationAttachment != null && configurationAttachment.Any())
                    {
                        attachmentlist = configurationAttachment.Select(r => new Models.FileAttachment { FileLink = r.FileLink, FileName = r.FileName }).ToList();
                    }

                }
                sendMailStatus = await CommunicationHelper.SendMailWithAttachmentAsync(tempWorkpackagePush.leadUserInfo.email, subject, body, tempWorkpackagePush.companyObj.ClientCode, attachmentlist);               

            }

            return sendMailStatus;
        }

        private static async Task<bool> SendEmailTemplate(string body, string subject, string configurationId, TempWorkpackagePush tempWorkpackagePush, string title, int notificationTemplateId, OWCUserVariable leadRecruiterObj)
        {
            var sendMailStatus = false;
            string userToken = string.Empty;
            if (!string.IsNullOrEmpty(body) && !string.IsNullOrEmpty(subject))
            {
                if (body.Contains("%signature%"))
                {
                    var signatureDetails = CommonStaticData.GetCachedEmailSignature(tempWorkpackagePush.companyObj.ClientCode, "group");
                    body = body.Replace("%signature%", signatureDetails != null ? signatureDetails.signatureText ?? string.Empty : string.Empty);
                }

                if (!String.IsNullOrEmpty(subject))
                {
                    string campaignname = string.Empty;
                    if (!string.IsNullOrEmpty(tempWorkpackagePush.WorkpackageCampaignName))
                    {
                        campaignname = tempWorkpackagePush.WorkpackageCampaignName;
                    }
                    subject = subject.Replace("%qname%", title).Replace("%sourcename%", campaignname);
                }

                
                Dictionary<string, object> staticobjects = StaticVariablesEmail(configurationId, tempWorkpackagePush, title, leadRecruiterObj);

                CommonStaticData.VacancyVariableLink(tempWorkpackagePush.ContactObject, body, tempWorkpackagePush.companyObj.ClientCode);
                if (!string.IsNullOrEmpty(tempWorkpackagePush.UserToken))
                {
                    userToken = tempWorkpackagePush.UserToken;
                    CommonStaticData.UserVariableLink(tempWorkpackagePush.ContactObject, body, userToken, tempWorkpackagePush.companyObj.ClientCode);
                }

                body = DynamicExtensions.DynamicVariablesReplace(body, tempWorkpackagePush.ContactObject, staticobjects, tempWorkpackagePush.ConfigurationDetails.MsgVariables);

                #region "Oldcode"
                //body = body.Replace("&lt;", "<")
                //              .Replace("&gt;", " >")
                //              .Replace("%fname%", tempWorkpackagePush.leadUserInfo.firstName)
                //              .Replace("%lname%", tempWorkpackagePush.leadUserInfo.lastName)
                //              .Replace("%phone%", tempWorkpackagePush.leadUserInfo.telephone)
                //              .Replace("%email%", tempWorkpackagePush.leadUserInfo.email)
                //              .Replace("%leadid%", tempWorkpackagePush.LeadUserId)
                //              .Replace("%qname%", title)
                //              .Replace("%qtype%", ((QuizTypeEnum)tempWorkpackagePush.quizType).ToString())
                //              .Replace("%qlink%", "<a href=\"" + tempWorkpackagePush.shortUrl + "\">" + tempWorkpackagePush.shortUrl + "</a>")
                //              .Replace("%sourcename%", String.IsNullOrEmpty(tempWorkpackagePush.WorkpackageCampaignName) ? string.Empty : tempWorkpackagePush.WorkpackageCampaignName)
                //              .Replace("%jrleadid%", tempWorkpackagePush.LeadUserId)
                //              .Replace("%jrconfigurationId%", configurationId)
                //              .Replace("%rfname%", (leadRecruiterObj != null && !String.IsNullOrEmpty(leadRecruiterObj.firstName)) ? leadRecruiterObj.firstName : string.Empty)
                //              .Replace("%rphone%", (leadRecruiterObj != null && !String.IsNullOrEmpty(leadRecruiterObj.phoneNumber)) ? leadRecruiterObj.phoneNumber : string.Empty)
                //              .Replace("%jrquizId%", tempWorkpackagePush.parentquizid.ToString());
                #endregion

                List<Models.FileAttachment> attachmentlist = null;
                if (notificationTemplateId > 0)
                {
                    using (var UOWObj = new AutomationUnitOfWork())
                    {
                        var configurationAttachment = UOWObj.AttachmentsInNotificationTemplateRepository.GetAttachmentsInNotificationTemplateByTemplateId(notificationTemplateId);
                        if (configurationAttachment != null && configurationAttachment.Any())
                        {
                            attachmentlist = configurationAttachment.Select(r => new Models.FileAttachment { FileLink = r.Description, FileName = r.Title }).ToList();
                        }

                    }
                }
                sendMailStatus = await CommunicationHelper.SendMailWithAttachmentAsync(tempWorkpackagePush.leadUserInfo.email, subject, body, tempWorkpackagePush.companyObj.ClientCode, attachmentlist);
            }

            return sendMailStatus;


        }


        private static Dictionary<string, object> StaticVariablesEmail(string configurationId, TempWorkpackagePush tempWorkpackagePush, string title, OWCUserVariable leadRecruiterObj)
        {
            Dictionary<string, object> staticobjects = new Dictionary<string, object>();

            staticobjects.Add("&lt;", "<");
            staticobjects.Add("&gt;", " >");
            staticobjects.Add("%qname%", String.IsNullOrEmpty(title) ? string.Empty : title);
            staticobjects.Add("%leadid%", tempWorkpackagePush.LeadUserId);
            staticobjects.Add("%jrleadid%", tempWorkpackagePush.LeadUserId);
            staticobjects.Add("%jrconfigurationId%", String.IsNullOrEmpty(configurationId) ? string.Empty : configurationId);
            staticobjects.Add("%jrquizId%", tempWorkpackagePush.parentquizid.ToString());
            staticobjects.Add("%qtype%", ((QuizTypeEnum)tempWorkpackagePush.quizType).ToString());

            staticobjects.Add("%qlink%", "<a href=\"" + tempWorkpackagePush.shortUrl + "\">" + tempWorkpackagePush.shortUrl + "</a>");
            if (leadRecruiterObj != null) 

            {
                staticobjects.Add("%rfname%", (leadRecruiterObj != null && !String.IsNullOrEmpty(leadRecruiterObj.firstName)) ? leadRecruiterObj.firstName : string.Empty);
                staticobjects.Add("%rphone%", (leadRecruiterObj != null && !String.IsNullOrEmpty(leadRecruiterObj.phoneNumber)) ? leadRecruiterObj.phoneNumber : string.Empty);
                staticobjects.Add("%recruiter%", (leadRecruiterObj != null && !String.IsNullOrEmpty(leadRecruiterObj.firstName)) ? leadRecruiterObj.firstName : string.Empty);
                staticobjects.Add("%sourcename%", String.IsNullOrEmpty(tempWorkpackagePush.WorkpackageCampaignName) ? string.Empty : tempWorkpackagePush.WorkpackageCampaignName);
                staticobjects.Add("%ContactOwnerFirstName%", String.IsNullOrEmpty(leadRecruiterObj?.firstName) ? string.Empty : leadRecruiterObj.firstName);
                staticobjects.Add("%ContactOwnerLastName%", String.IsNullOrEmpty(leadRecruiterObj?.lastName) ? string.Empty : leadRecruiterObj.lastName);
                staticobjects.Add("%ContactOwnerPhone%", String.IsNullOrEmpty(leadRecruiterObj?.phoneNumber) ? string.Empty : leadRecruiterObj.phoneNumber);
                staticobjects.Add("%ContactOwnerEmail%", String.IsNullOrEmpty(tempWorkpackagePush.leadUserInfo.email) ? string.Empty : tempWorkpackagePush.leadUserInfo.email);

            }
            
            return staticobjects;
        }

        private static string ModifyQuizTitle(string quizTitle, TempWorkpackagePush.ContactBasicDetails leadUserInfo, string leadUserId, int quizId, int configurationDetailId)
        {

            string title = quizTitle.Replace("%fname%", leadUserInfo.firstName)
                                                                    .Replace("%lname%", leadUserInfo.lastName)
                                                                    .Replace("%phone%", leadUserInfo.telephone)
                                                                    .Replace("%email%", leadUserInfo.email)
                                                                    .Replace("%leadid%", leadUserId)
                                                                    .Replace("%qname%", string.Empty)
                                                                    .Replace("%qlink%", string.Empty)
                                                                    .Replace("%qendresult%", string.Empty)
                                                                    .Replace("%correctanswerexplanation%", string.Empty)
                                                                    .Replace("%signature%", string.Empty);

            MatchCollection mcol = Regex.Matches(title, @"%\b\S+?\b%");
            using (var UOWObj = new AutomationUnitOfWork())
            {
                if (mcol != null && mcol.Count > 0)
                {
                    var variablesDetailList = UOWObj.VariablesDetailsRepository.Get(r => r.VariableInQuiz.QuizId == quizId && r.ConfigurationDetailsId == configurationDetailId);
                    foreach (Match m in mcol)
                    {
                        var variablesDetailObj = variablesDetailList.FirstOrDefault(t => t.VariableInQuiz.Variables.Name == m.ToString().ToLower().Replace("%", string.Empty));
                        if (variablesDetailObj != null)
                        {
                            title = title.Replace(m.ToString(), variablesDetailObj.VariableValue);
                        }
                        else
                        {
                            title = title.Replace(m.ToString(), string.Empty);
                        }
                    }
                }
            }

            return title;

        }
    }
}
