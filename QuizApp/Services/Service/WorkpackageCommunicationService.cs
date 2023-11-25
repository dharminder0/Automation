using Core.Common.Extensions;
using NLog;
using QuizApp.Helpers;
using QuizApp.RepositoryPattern;
using QuizApp.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using QuizApp.RepositoryExtensions;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Dynamic;
using QuizApp.Services.Model;

namespace QuizApp.Services.Service {
    public class WorkpackageCommunicationService {
        private static bool enableExternalActionQueue = bool.Parse(string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["EnableExternalActionQueue"]) ? "true" : ConfigurationManager.AppSettings["EnableExternalActionQueue"]);
        private ResultEnum status = ResultEnum.Ok;
        private string errormessage = string.Empty;
        public static NLog.Logger Logger = LogManager.GetCurrentClassLogger();
        public ResultEnum Status { get { return status; } set { status = value; } }
        public string ErrorMessage { get { return errormessage; } set { errormessage = value; } }
        public static readonly OWCHelper _owchelper = new OWCHelper();

        public static async Task SendEmailSmsNew(TempWorkpackagePush tempWorkpackagePush) {

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

            if (configurationDetail.ConfigurationType.EqualsCI("WHATSAPP_CHATBOT") || configurationDetail.ConfigurationType.EqualsCI("WHATSAPP_CHATBOT_TEMPLATE")) {

                return;
            }
            if (tempWorkpackagePush.UsageType == (int)UsageTypeEnum.WhatsAppChatbot) {
                return;
            }

            Db.QuizDetails quizDetails = tempWorkpackagePush.QuizDetails;
            Db.NotificationTemplate notificationTemplate = null;
            List<Db.VariablesDetails> variablesDetailList = null;
            List<Db.TemplateParameterInConfigurationDetails> templateParameterInConfigurationDetailList = null;
         
            var errortrack = "";
            try {

                using (var UOWObj = new AutomationUnitOfWork()) {

                    try {
                        if (configurationDetail != null && templateParameterInConfigurationDetailList != null) {
                            templateParameterInConfigurationDetailList = UOWObj.TemplateParameterInConfigurationDetailsRepository.GetTemplateParameterInConfigurationDetailsByConfigurationId(configurationDetail.Id).ToList();
                        }
                    } catch (Exception ex) {

                        Logger.Log(LogLevel.Error, "SendEmailSmsandWhatsappNew templateParameterInConfigurationDetailList - " + Id + " has issue" + ex.Message);
                    }


                    try {
                        var template = UOWObj.NotificationTemplatesInQuizRepository.Get(r => r.NotificationTemplate.Status == (int)StatusEnum.Active && r.QuizId == tempWorkpackagePush.parentquizid && r.NotificationTemplate.NotificationType == (int)NotificationTypeEnum.INVITATION).FirstOrDefault();

                        if (template != null) {
                            notificationTemplate = template.NotificationTemplate;
                        }
                    } catch (Exception ex) {
                        Logger.Log(LogLevel.Error, "SendEmailSmsandWhatsappNew notificationTemplate - " + Id + " has issue " + ex.Message);
                    }

                }


                if (!string.IsNullOrEmpty(tempWorkpackagePush.leadUserInfo.email)) {
                    title = quizDetails.QuizTitle;
                }

                try {
                    if (!string.IsNullOrEmpty(tempWorkpackagePush.leadUserInfo.email)) {
                        if (!configurationDetail.IsUpdatedSend) {
                            if (notificationTemplate != null && !String.IsNullOrEmpty(notificationTemplate.Body)) {


                                sendMailStatus = await SendEmailTemplate(notificationTemplate.Body, notificationTemplate.Subject, configurationDetail.ConfigurationId, tempWorkpackagePush, title, notificationTemplate.Id, tempWorkpackagePush.RecruiterList);

                            }

                            string smsBody = string.Empty;

                            if (notificationTemplate != null && !String.IsNullOrEmpty(notificationTemplate.SMSText)) {

                                sendSmsStatus = await SendSMS(notificationTemplate.SMSText, configurationDetail.ConfigurationId, tempWorkpackagePush, title, tempWorkpackagePush.RecruiterList);


                            }

                        } else {

                            if (!string.IsNullOrEmpty(tempWorkpackagePush.LeadUserId)) {

                                if (configurationDetail.SendEmail.GetValueOrDefault() == true && !string.IsNullOrEmpty(configurationDetail.Body) && !string.IsNullOrEmpty(configurationDetail.Subject)) {

                                    sendMailStatus = await SendEmail(configurationDetail.Body, configurationDetail.Subject, configurationDetail.ConfigurationId, tempWorkpackagePush, title, tempWorkpackagePush.RecruiterList);
                                }

                                if (configurationDetail.SendSms.GetValueOrDefault() == true && !string.IsNullOrEmpty(configurationDetail.SMSText)) {
                                    sendSmsStatus = await SendSMS(configurationDetail.SMSText, configurationDetail.ConfigurationId, tempWorkpackagePush, title, tempWorkpackagePush.RecruiterList);
                                }
                            }

                        }
                    }
                } catch (Exception ex) {
                    Logger.Log(LogLevel.Error, "SendEmailSmsNew inside function email Exception " + Id + " Exception-main error inner exception " + ex.InnerException.Message);
                    throw ex;
                }

                try {

                    using (var UOWObj = new AutomationUnitOfWork()) {
                        var workPackageInfo = UOWObj.WorkPackageInfoRepository.GetByID(Id);
                        workPackageInfo.ShotenUrlCode = string.IsNullOrWhiteSpace(workPackageInfo.ShotenUrlCode) ? shotenUrlCode : workPackageInfo.ShotenUrlCode;
                        workPackageInfo.EmailSentOn = sendMailStatus ? currentDate : default(DateTime?);
                        workPackageInfo.SMSSentOn = sendSmsStatus ? currentDate : default(DateTime?);
                        workPackageInfo.IsOurEndLogicForInvitation = true;
                        UOWObj.WorkPackageInfoRepository.Update(workPackageInfo);
                        UOWObj.Save();
                    }
                } catch (Exception ex) {
                    Logger.Log(LogLevel.Error, "SendEmailSmsNew error in updating workPackageInfoObj end Exception " + Id + " Exception-main error inner exception " + ex.Message);
                    Logger.Log(LogLevel.Error, "SendEmailSmsNew error in updating workPackageInfoObj end Exception " + Id + " Exception-main error inner exception " + ex.InnerException.Message);
                }

                errortrack += $"#succeed  #";
            } catch (Exception ex) {

                Logger.Log(LogLevel.Error, "SendEmailSmsNew outer function Exception #" + Id + "# quizdetailid " + tempWorkpackagePush.quizDetailid + "#Leadinfo :" + JsonConvert.SerializeObject(tempWorkpackagePush.leadUserInfo) + "# Exception-main error " + ex.Message);
                Logger.Log(LogLevel.Error, "SendEmailSmsNew outer function Exception track " + Id + " Exception-main error " + errortrack);
                if (ex.InnerException != null && ex.InnerException.Message != null) {
                    Logger.Log(LogLevel.Error, "SendEmailSmsNew inside function Exception " + Id + " Exception-main error inner exception " + ex.InnerException.Message);
                }
            }

        }

        private static async Task<bool> SendEmail(string body, string subject, string configurationId, TempWorkpackagePush tempWorkpackagePush, string title, List<OWCUserVariable> leadRecruiterObj) {
            var sendMailStatus = false;
            string userToken = string.Empty;
            if (!string.IsNullOrEmpty(body) && !string.IsNullOrEmpty(subject)) {

                if (body.Contains("%signature%")) {
                    var signatureDetails = CommonStaticData.GetCachedEmailSignature(tempWorkpackagePush.companyObj.ClientCode, "group");
                    body = body.Replace("%signature%", signatureDetails != null ? signatureDetails.signatureText ?? string.Empty : string.Empty);
                }


                if (!String.IsNullOrEmpty(subject)) {
                    string campaignname = string.Empty;
                    if (!string.IsNullOrEmpty(tempWorkpackagePush.WorkpackageCampaignName)) {
                        campaignname = tempWorkpackagePush.WorkpackageCampaignName;
                    }

                    subject = subject.Replace("%qname%", title).Replace("%sourcename%", campaignname);
                }

                Dictionary<string, object> staticobjects = StaticVariablesEmail(configurationId, tempWorkpackagePush, title, leadRecruiterObj);

                CommonStaticData.VacancyVariableLink(tempWorkpackagePush.ContactObject, body, tempWorkpackagePush.companyObj.ClientCode);
                if (!string.IsNullOrEmpty(tempWorkpackagePush.UserToken)) {
                    userToken = tempWorkpackagePush.UserToken;
                    CommonStaticData.UserVariableLink(tempWorkpackagePush.ContactObject, body, userToken, tempWorkpackagePush.companyObj.ClientCode);
                }

                body = DynamicExtensions.DynamicVariablesReplace(body, tempWorkpackagePush.ContactObject, staticobjects, tempWorkpackagePush.ConfigurationDetails.MsgVariables);

                

                List<Models.FileAttachment> attachmentlist = null;

                using (var UOWObj = new AutomationUnitOfWork()) {
                    var configurationAttachment = UOWObj.AttachmentsInConfigurationRepository.GetAttachmentsInConfigurationByConfigurationId(tempWorkpackagePush.ConfigurationDetailId);
                    if (configurationAttachment != null && configurationAttachment.Any()) {
                        attachmentlist = configurationAttachment.Select(r => new Models.FileAttachment { FileLink = r.FileLink, FileName = r.FileName }).ToList();
                    }

                }
                sendMailStatus = await CommunicationHelper.SendMailWithAttachmentAsync(tempWorkpackagePush.leadUserInfo.email, subject, body, tempWorkpackagePush.companyObj.ClientCode, attachmentlist);

            }

            return sendMailStatus;
        }

        private static async Task<bool> SendEmailTemplate(string body, string subject, string configurationId, TempWorkpackagePush tempWorkpackagePush, string title, int notificationTemplateId, List<OWCUserVariable> leadRecruiterObjList) {
            var sendMailStatus = false;
            string userToken = string.Empty;
            if (!string.IsNullOrEmpty(body) && !string.IsNullOrEmpty(subject)) {
                if (body.Contains("%signature%")) {
                    var signatureDetails = CommonStaticData.GetCachedEmailSignature(tempWorkpackagePush.companyObj.ClientCode, "group");
                    body = body.Replace("%signature%", signatureDetails != null ? signatureDetails.signatureText ?? string.Empty : string.Empty);
                }

                if (!String.IsNullOrEmpty(subject)) {
                    string campaignname = string.Empty;
                    if (!string.IsNullOrEmpty(tempWorkpackagePush.WorkpackageCampaignName)) {
                        campaignname = tempWorkpackagePush.WorkpackageCampaignName;
                    }
                    subject = subject.Replace("%qname%", title).Replace("%sourcename%", campaignname);
                }


                Dictionary<string, object> staticobjects = StaticVariablesEmail(configurationId, tempWorkpackagePush, title, leadRecruiterObjList);

                CommonStaticData.VacancyVariableLink(tempWorkpackagePush.ContactObject, body, tempWorkpackagePush.companyObj.ClientCode);
                if (!string.IsNullOrEmpty(tempWorkpackagePush.UserToken)) {
                    userToken = tempWorkpackagePush.UserToken;
                    CommonStaticData.UserVariableLink(tempWorkpackagePush.ContactObject, body, userToken, tempWorkpackagePush.companyObj.ClientCode);
                }

                body = DynamicExtensions.DynamicVariablesReplace(body, tempWorkpackagePush.ContactObject, staticobjects, tempWorkpackagePush.ConfigurationDetails.MsgVariables);

                List<Models.FileAttachment> attachmentlist = null;
                if (notificationTemplateId > 0) {
                    using (var UOWObj = new AutomationUnitOfWork()) {
                        var configurationAttachment = UOWObj.AttachmentsInNotificationTemplateRepository.GetAttachmentsInNotificationTemplateByTemplateId(notificationTemplateId);
                        if (configurationAttachment != null && configurationAttachment.Any()) {
                            attachmentlist = configurationAttachment.Select(r => new Models.FileAttachment { FileLink = r.Description, FileName = r.Title }).ToList();
                        }

                    }
                }
                sendMailStatus = await CommunicationHelper.SendMailWithAttachmentAsync(tempWorkpackagePush.leadUserInfo.email, subject, body, tempWorkpackagePush.companyObj.ClientCode, attachmentlist);
            }

            return sendMailStatus;


        }


        private static Dictionary<string, object> StaticVariablesEmail(string configurationId, TempWorkpackagePush tempWorkpackagePush, string title, List<OWCUserVariable> leadRecruiterObjList) {
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
            staticobjects.Add("%sourcename%", String.IsNullOrEmpty(tempWorkpackagePush.WorkpackageCampaignName) ? string.Empty : tempWorkpackagePush.WorkpackageCampaignName);

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


        private static async Task<bool> SendSMS(string smsText, string ConfigurationId, TempWorkpackagePush tempWorkpackagePush, string title, List<OWCUserVariable> leadRecruiterObjList) {
            var sendSmsStatus = false;
            if (!string.IsNullOrEmpty(smsText)) {
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
                //staticobjects.Add("%rfname%", (leadRecruiterObj != null && !String.IsNullOrEmpty(leadRecruiterObj.firstName)) ? leadRecruiterObj.firstName : string.Empty);
                //staticobjects.Add("%rphone%", (leadRecruiterObj != null && !String.IsNullOrEmpty(leadRecruiterObj.phoneNumber)) ? leadRecruiterObj.phoneNumber : string.Empty);
                //staticobjects.Add("%recruiter%", (leadRecruiterObj != null && !String.IsNullOrEmpty(leadRecruiterObj.firstName)) ? leadRecruiterObj.firstName : string.Empty);

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



                staticobjects.Add("%signature%", string.Empty);

                CommonStaticData.VacancyVariableLink(tempWorkpackagePush.ContactObject, smsText, tempWorkpackagePush.companyObj.ClientCode);
                if (!string.IsNullOrWhiteSpace(tempWorkpackagePush.UserToken)) {
                    CommonStaticData.UserVariableLink(tempWorkpackagePush.ContactObject, smsText, tempWorkpackagePush.UserToken, tempWorkpackagePush.companyObj.ClientCode);
                }

                var smsBody = DynamicExtensions.DynamicVariablesReplace(smsText, tempWorkpackagePush.ContactObject, staticobjects, tempWorkpackagePush.ConfigurationDetails.MsgVariables);
                sendSmsStatus = await CommunicationHelper.SendSMSAsync(tempWorkpackagePush.leadUserInfo.telephone, smsBody, tempWorkpackagePush.companyObj);
            }

            return sendSmsStatus;
        }


        static void SendEmailSmsandWhatsappHandlerNewWithQueue(TempWorkpackagePush item) {
            try {
                ExternalActionQueueService.InsertExternalActionQueue(item.companyObj.Id, item.LeadUserId, QueueItemTypes.SendEmailSms, (int)QueueStatusTypes.New, JsonConvert.SerializeObject(item));
                ExternalActionQueueService.InsertExternalActionQueue(item.companyObj.Id, item.LeadUserId, QueueItemTypes.SendWhatsapp, (int)QueueStatusTypes.New, JsonConvert.SerializeObject(item));
            } catch (Exception ex) {

                return;
            }
        }

        public static void SendEmailSmsandWhatsappHandlerNew(TempWorkpackagePush item) {
            try {
                if (enableExternalActionQueue) {
                    SendEmailSmsandWhatsappHandlerNewWithQueue(item);
                } else {
                    Task.Run(async () => {

                        await PrerenderHelper.RecachePrerenderedPage(item.shortUrl, item.workPackageInfoId);
                        item.RecruiterList = GetOwnerList(item);
                        if (!string.IsNullOrEmpty(item.leadUserInfo?.email)) {
                            item.QuizDetails.QuizTitle = ModifyQuizTitle(item.QuizDetails?.QuizTitle, item.leadUserInfo, item.LeadUserId, item.ConfigurationDetails.QuizId, item.ConfigurationDetails.Id);
                        }
                        await SendEmailSmsNew(item);
                        await SendWhatsappNew(item);
                    });
                }
            } catch (Exception ex) {

                Logger.Log(LogLevel.Error, "SendEmailSmsandWhatsappHandlerNew inside function error - " + item.workPackageInfoId + " Exception-" + ex.Message);
            }


        }


        private static string ModifyQuizTitle(string quizTitle, TempWorkpackagePush.ContactBasicDetails leadUserInfo, string leadUserId, int quizId, int configurationDetailId) {

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
            using (var UOWObj = new AutomationUnitOfWork()) {
                if (mcol != null && mcol.Count > 0) {
                    var variablesDetailList = UOWObj.VariablesDetailsRepository.Get(r => r.VariableInQuiz.QuizId == quizId && r.ConfigurationDetailsId == configurationDetailId);
                    foreach (Match m in mcol) {
                        var variablesDetailObj = variablesDetailList.FirstOrDefault(t => t.VariableInQuiz.Variables.Name == m.ToString().ToLower().Replace("%", string.Empty));
                        if (variablesDetailObj != null) {
                            title = title.Replace(m.ToString(), variablesDetailObj.VariableValue);
                        } else {
                            title = title.Replace(m.ToString(), string.Empty);
                        }
                    }
                }
            }

            return title;

        }



        public static List<OWCUserVariable> GetOwnerList(TempWorkpackagePush tempWorkpackagePush) {

            string sourceOwnerId = null;
            string contactOwnerId = null;
            string leadOwnerId = null;
            var userdetails = new List<OWCUserVariable>();
            try {

                long[] Userids = new long[1];

                if (tempWorkpackagePush.leadUserInfo.contactId.ContainsCI("SF")) {
                    List<long> UseridList = new List<long>();

                    if (tempWorkpackagePush.leadUserInfo.contactId.ContainsCI("SF")) {
                        if (!string.IsNullOrWhiteSpace(tempWorkpackagePush.leadUserInfo.SourceOwnerId)) {
                            sourceOwnerId = tempWorkpackagePush.leadUserInfo.SourceOwnerId;
                        }
                    }

                    if (tempWorkpackagePush.leadUserInfo.contactId.ContainsCI("SF")) {
                        if (!string.IsNullOrWhiteSpace(tempWorkpackagePush.leadUserInfo.ContactOwnerId)) {
                            contactOwnerId = tempWorkpackagePush.leadUserInfo.ContactOwnerId;

                        }
                    }

                    if (tempWorkpackagePush.leadUserInfo.contactId.ContainsCI("SF")) {
                        if (!string.IsNullOrWhiteSpace(tempWorkpackagePush.leadUserInfo.LeadOwnerId)) {
                            leadOwnerId = tempWorkpackagePush.leadUserInfo.LeadOwnerId;
                        }
                    }

                    List<string> userids = new List<string>();
                    if (!string.IsNullOrWhiteSpace(sourceOwnerId)) {
                        userids.Add(sourceOwnerId);
                    }

                    if (!string.IsNullOrWhiteSpace(contactOwnerId)) {
                        userids.Add(contactOwnerId);
                    }

                    if (!string.IsNullOrWhiteSpace(leadOwnerId)) {
                        userids.Add(leadOwnerId);
                    }

                    var userdetailslist = OWCHelper.GetListOfExternalDetails(userids, tempWorkpackagePush.companyObj);
                    if (userdetailslist != null && userdetailslist.Any()) {
                        if (!string.IsNullOrWhiteSpace(sourceOwnerId)) {
                            var owcResponseObj = userdetailslist.Where(v => v.ExternalUserId.EqualsCI(sourceOwnerId)).FirstOrDefault();
                            if (owcResponseObj != null) {
                                userdetails.Add(new OWCUserVariable {

                                    firstName = owcResponseObj.firstName,
                                    lastName = owcResponseObj.lastName,
                                    userName = owcResponseObj.userName,
                                    Mail = owcResponseObj.Mail,
                                    Email = owcResponseObj.Email,
                                    nickName = owcResponseObj.nickName,
                                    phoneNumber = owcResponseObj.phoneNumber,
                                    Phone = owcResponseObj.phoneNumber,
                                    ExternalUserId = owcResponseObj.ExternalUserId,
                                    ObjectUserOwnerType = "SourceOwner"

                                });

                            }
                        }

                        if (!string.IsNullOrWhiteSpace(contactOwnerId)) {
                            var owcResponseObj = userdetailslist.Where(v => v.ExternalUserId.EqualsCI(contactOwnerId)).FirstOrDefault();
                            if (owcResponseObj != null) {
                                userdetails.Add(new OWCUserVariable {

                                    firstName = owcResponseObj.firstName,
                                    lastName = owcResponseObj.lastName,
                                    userName = owcResponseObj.userName,
                                    Mail = owcResponseObj.Mail,
                                    Email = owcResponseObj.Email,
                                    nickName = owcResponseObj.nickName,
                                    phoneNumber = owcResponseObj.phoneNumber,
                                    Phone = owcResponseObj.phoneNumber,
                                    ExternalUserId = owcResponseObj.ExternalUserId,
                                    ObjectUserOwnerType = "ContactOwner"

                                });

                            }
                        }

                        if (!string.IsNullOrWhiteSpace(leadOwnerId)) {
                            var owcResponseObj = userdetailslist.Where(v => v.ExternalUserId.EqualsCI(leadOwnerId)).FirstOrDefault();
                            if (owcResponseObj != null) {
                                userdetails.Add(new OWCUserVariable {
                                    firstName = owcResponseObj.firstName,
                                    lastName = owcResponseObj.lastName,
                                    userName = owcResponseObj.userName,
                                    Mail = owcResponseObj.Mail,
                                    Email = owcResponseObj.Email,
                                    nickName = owcResponseObj.nickName,
                                    phoneNumber = owcResponseObj.phoneNumber,
                                    Phone = owcResponseObj.phoneNumber,
                                    ExternalUserId = owcResponseObj.ExternalUserId,
                                    ObjectUserOwnerType = "LeadOwner"

                                });

                            }

                        }
                    }
                    //if (userdetails != null && userdetails.Any())
                    //{
                    //    leadRecruiterObj = userdetails.FirstOrDefault();
                    //}
                } else {


                    if (!string.IsNullOrWhiteSpace(tempWorkpackagePush.leadUserInfo?.SourceOwnerId)) {
                        var externalDetails = _owchelper.GetExternalDetails(tempWorkpackagePush.companyObj.ClientCode, tempWorkpackagePush.leadUserInfo.SourceOwnerId);
                        if (externalDetails != null) {
                            var oWCBusinessUserResponse = JsonConvert.DeserializeObject<OWCUserVariable>(externalDetails);
                            if (oWCBusinessUserResponse != null) {
                                oWCBusinessUserResponse.Phone = oWCBusinessUserResponse.phoneNumber;
                                oWCBusinessUserResponse.Email = oWCBusinessUserResponse.Mail;
                                oWCBusinessUserResponse.ObjectUserOwnerType = "SourceOwner";
                                userdetails.Add(oWCBusinessUserResponse);
                                //leadRecruiterObj = oWCBusinessUserResponse;
                            }
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(tempWorkpackagePush.leadUserInfo?.ContactOwnerId)) {
                        var externalDetailsContact = _owchelper.GetExternalDetails(tempWorkpackagePush.companyObj.ClientCode, tempWorkpackagePush.leadUserInfo.ContactOwnerId);
                        if (externalDetailsContact != null) {
                            var oWCBusinessUserResponse = JsonConvert.DeserializeObject<OWCUserVariable>(externalDetailsContact);
                            if (oWCBusinessUserResponse != null) {
                                oWCBusinessUserResponse.Phone = oWCBusinessUserResponse.phoneNumber;
                                oWCBusinessUserResponse.Email = oWCBusinessUserResponse.Mail;
                                oWCBusinessUserResponse.ObjectUserOwnerType = "ContactOwner";
                                userdetails.Add(oWCBusinessUserResponse);
                            }

                        }
                    }

                    if (!string.IsNullOrWhiteSpace(tempWorkpackagePush.leadUserInfo?.LeadOwnerId)) {
                        var externalDetailsLead = _owchelper.GetExternalDetails(tempWorkpackagePush.companyObj.ClientCode, tempWorkpackagePush.leadUserInfo.LeadOwnerId);
                        if (externalDetailsLead != null) {
                            var oWCBusinessUserResponse = JsonConvert.DeserializeObject<OWCUserVariable>(externalDetailsLead);
                            if (oWCBusinessUserResponse != null) {
                                oWCBusinessUserResponse.Phone = oWCBusinessUserResponse.phoneNumber;
                                oWCBusinessUserResponse.Email = oWCBusinessUserResponse.Mail;
                                oWCBusinessUserResponse.ObjectUserOwnerType = "LeadOwner";
                                userdetails.Add(oWCBusinessUserResponse);
                            }

                        }
                    }
                }
            } catch (Exception ex) {
                Logger.Log(LogLevel.Error, "SendEmailSmsandWhatsappNew inside function - " + tempWorkpackagePush.leadUserInfo.SourceOwnerId + "Source Owner Details not found ");
            }



            return userdetails;

        }

        private static Dictionary<string, object> WhatsAppStaticVariables(TempWorkpackagePush tempWorkpackagePush, string title) {
            Dictionary<string, object> staticobjects = new Dictionary<string, object>();


            staticobjects.Add("%leadid%", tempWorkpackagePush.LeadUserId);
            staticobjects.Add("%qname%", String.IsNullOrEmpty(title) ? string.Empty : title);
            staticobjects.Add("%qtype%", ((QuizTypeEnum)tempWorkpackagePush.quizType).ToString());
            staticobjects.Add("%qlink%", tempWorkpackagePush.shortUrl);
            staticobjects.Add("%qLinkKey%", tempWorkpackagePush.qLinkKey);
            staticobjects.Add("%sourcename%", String.IsNullOrEmpty(tempWorkpackagePush.WorkpackageCampaignName) ? string.Empty : tempWorkpackagePush.WorkpackageCampaignName);
            staticobjects.Add("%qendresult%", string.Empty);
            staticobjects.Add("%correctanswerexplanation%", string.Empty);
            //staticobjects.Add("%rfname%", (leadRecruiterObj != null && !String.IsNullOrEmpty(leadRecruiterObj.firstName)) ? leadRecruiterObj.firstName : string.Empty);
            //staticobjects.Add("%rphone%", (leadRecruiterObj != null && !String.IsNullOrEmpty(leadRecruiterObj.phoneNumber)) ? leadRecruiterObj.phoneNumber : string.Empty);
            //staticobjects.Add("%recruiter%", (leadRecruiterObj != null && !String.IsNullOrEmpty(leadRecruiterObj.firstName)) ? leadRecruiterObj.firstName : string.Empty);
            staticobjects.Add("%signature%", string.Empty);


            if (tempWorkpackagePush.RecruiterList != null) {
                foreach (var leadRecruiterObj in tempWorkpackagePush.RecruiterList) {
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
            }

            return staticobjects;
        }


        private static async Task<bool> SendWhatsApp(string followUpMessage, int hsmTemplateId, string hsmTemplateLanguageCode, string companyCode, TempWorkpackagePush tempWorkpackagePush, string title) {
            var sendWhatsappStatus = false;
            List<Db.TemplateParameterInConfigurationDetails> templateParameterInConfigurationDetails = null;
            OWCUserVariable leadRecruiterObj = new OWCUserVariable();
            try {

                if (templateParameterInConfigurationDetails == null) {
                    ErrorLog.LogError(new Exception("#configurationDetailForWhatsappParam :" + tempWorkpackagePush.ConfigurationDetailId));
                    using (var UOWObj = new AutomationUnitOfWork()) {

                        templateParameterInConfigurationDetails = UOWObj.TemplateParameterInConfigurationDetailsRepository.GetTemplateParameterInConfigurationDetailsByConfigurationId(tempWorkpackagePush.ConfigurationDetailId).ToList();
                    }

                    if (templateParameterInConfigurationDetails == null) {
                        ErrorLog.LogError(new Exception("#configurationDetailForWhatsappParam 22:" + tempWorkpackagePush.ConfigurationDetailId));
                    }
                }


                var userdetails = new List<OWCUserVariable>();
                long[] Userids = new long[1];





                #region Send WhatsappMessage

                if (hsmTemplateId > 0) {
                    Dictionary<string, object> staticobjects = WhatsAppStaticVariables(tempWorkpackagePush, title);

                    sendWhatsappStatus = WhatsaAppCommunicationHelper.SendWhatsapp(tempWorkpackagePush.leadUserInfo.telephone, tempWorkpackagePush.ContactObject, staticobjects, followUpMessage, tempWorkpackagePush.ConfigurationDetails.MsgVariables, hsmTemplateId, hsmTemplateLanguageCode, tempWorkpackagePush.workPackageInfoId, tempWorkpackagePush.UserToken, companyCode, templateParameterInConfigurationDetails, tempWorkpackagePush);


                    if (sendWhatsappStatus) {

                        using (var UOWObj = new AutomationUnitOfWork()) {
                            var workPackageInfo = UOWObj.WorkPackageInfoRepository.GetByID(tempWorkpackagePush.workPackageInfoId);
                            workPackageInfo.ShotenUrlCode = string.IsNullOrWhiteSpace(workPackageInfo.ShotenUrlCode) ? tempWorkpackagePush.shotenUrlCode : workPackageInfo.ShotenUrlCode;
                            workPackageInfo.WhatsappSentOn = sendWhatsappStatus ? DateTime.UtcNow : default(DateTime?);
                            UOWObj.WorkPackageInfoRepository.Update(workPackageInfo);
                            UOWObj.Save();
                        }

                    }
                }

                #endregion

            } catch (Exception ex) {
                Logger.Log(LogLevel.Error, "SendEmailSmsandWhatsapp SendWhatsApp exception  " + tempWorkpackagePush.workPackageInfoId + " Exception-main error inner exception " + ex.Message);
                Logger.Log(LogLevel.Error, "SendEmailSmsandWhatsapp SendWhatsApp inner exception  " + tempWorkpackagePush.workPackageInfoId + " Exception-main error inner exception " + ex.InnerException.Message);
            }


            return sendWhatsappStatus;
        }

        public static async Task SendWhatsappNew(TempWorkpackagePush tempWorkpackagePush) {
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
            try {
                #region Send WhatsappMessage

                if (configurationDetail.SendWhatsApp.GetValueOrDefault() == true && configurationDetail.HsmTemplateId != null && configurationDetail.HsmTemplateId > 0) {
                    if (!string.IsNullOrEmpty(tempWorkpackagePush.leadUserInfo.email)) {
                        title = quizDetails.QuizTitle;
                    }

                    await SendWhatsApp(configurationDetail.FollowUpMessage, configurationDetail.HsmTemplateId.Value, configurationDetail.HsmTemplateLanguageCode, configurationDetail.CompanyCode, tempWorkpackagePush, title);
                } else {
                    if (tempWorkpackagePush.UsageType == (int)UsageTypeEnum.WhatsAppChatbot || configurationDetail.ConfigurationType.EqualsCI("WHATSAPP_CHATBOT") || configurationDetail.ConfigurationType.EqualsCI("WHATSAPP_CHATBOT_TEMPLATE")) {
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

                        if (sendWhatsappStatus) {

                            using (var UOWObj = new AutomationUnitOfWork()) {
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


            } catch (Exception ex) {

                Logger.Log(LogLevel.Error, "SendWhatsappNew inside function Exception #" + Id + "# quizdetailid " + tempWorkpackagePush.quizDetailid + "#Leadinfo :" + JsonConvert.SerializeObject(tempWorkpackagePush.leadUserInfo) + "# Exception-main error " + ex.Message);
                Logger.Log(LogLevel.Error, "SendWhatsappNew inside function Exception track " + Id + " Exception-main error " + errortrack);
                if (ex.InnerException != null && ex.InnerException.Message != null) {
                    Logger.Log(LogLevel.Error, "SendWhatsappNew inside function Exception " + Id + " Exception-main error inner exception " + ex.InnerException.Message);
                }
            }

        }

        public void AddRecruiterDetails(Dictionary<string, object> exObjpair, CompanyModel CompanyObj) {
            long SourceOwnerId = 0;
            long ContactOwnerId = 0;
            long LeadOwnerId = 0;
            List<long> userids = new List<long>();
            if (!(exObjpair.ContainsKey("recruiter") && exObjpair["recruiter"] != null && Convert.ToString(exObjpair["recruiter"]) != null)) {
                var ContactId = exObjpair.ContainsKey("contactid") && exObjpair["contactid"] != null ? Convert.ToString(exObjpair["contactid"]) : null;
                var contactOwnerId = exObjpair.ContainsKey("contactownerid") && exObjpair["contactownerid"] != null ? Convert.ToString(exObjpair["contactownerid"]) : null;
                var sourceOwnerId = exObjpair.ContainsKey("sourceownerid") && exObjpair["sourceownerid"] != null ? Convert.ToString(exObjpair["sourceownerid"]) : null;
                var leadOwnerId = exObjpair.ContainsKey("leadownerid") && exObjpair["leadownerid"] != null ? Convert.ToString(exObjpair["leadownerid"]) : null;

                if (ContactId.ContainsCI("SF")) {
                        if (!string.IsNullOrWhiteSpace(sourceOwnerId)) {
                            SourceOwnerId = Convert.ToInt64(sourceOwnerId); 
                        }
                }

                    if (ContactId.ContainsCI("SF")) {
                        if (!string.IsNullOrWhiteSpace(contactOwnerId)) {
                            ContactOwnerId = Convert.ToInt64(contactOwnerId);
                        }
                    }

                if (ContactId.ContainsCI("SF")) {
                    if (!string.IsNullOrWhiteSpace(leadOwnerId)) {
                        LeadOwnerId = Convert.ToInt64(leadOwnerId);

                    }
                }



                if (SourceOwnerId > 0) {
                    userids.Add(SourceOwnerId);
                }
                if (ContactOwnerId > 0) {
                    userids.Add(ContactOwnerId);
                }
                if (LeadOwnerId > 0) {
                    userids.Add(LeadOwnerId);
                }

            }

            var userdetails = OWCHelper.GetUserListUsingUserId(userids, CompanyObj).ToList();

            if (userdetails != null && userdetails.Any()) {
                var leadRecruiterObj = userdetails.FirstOrDefault();
                if (leadRecruiterObj != null) {
                    exObjpair.Add("recruiter", leadRecruiterObj.firstName + " " + leadRecruiterObj.lastName);
                }

                foreach (var leadRecruiter in userdetails) {
                    exObjpair.Add($@"%{leadRecruiter.ObjectUserOwnerType}.firstname%", String.IsNullOrEmpty(leadRecruiter.firstName) ? string.Empty : leadRecruiter.firstName);
                    exObjpair.Add($@"%{leadRecruiter.ObjectUserOwnerType}.lastName%", String.IsNullOrEmpty(leadRecruiter.lastName) ? string.Empty : leadRecruiter.lastName);
                    exObjpair.Add($@"%{leadRecruiter.ObjectUserOwnerType}.phoneNumber%", String.IsNullOrEmpty(leadRecruiter.phoneNumber) ? string.Empty : leadRecruiter.phoneNumber);
                    exObjpair.Add($@"%{leadRecruiter.ObjectUserOwnerType}.phone%", String.IsNullOrEmpty(leadRecruiter.phoneNumber) ? string.Empty : leadRecruiter.phoneNumber);
                    exObjpair.Add($@"%{leadRecruiter.ObjectUserOwnerType}.email%", String.IsNullOrEmpty(leadRecruiter.Mail) ? string.Empty : leadRecruiter.Mail);
                    if (leadRecruiter.ObjectUserOwnerType.EqualsCI("SourceOwner")) {
                        exObjpair.Add("%rfname%", (leadRecruiter != null && !String.IsNullOrEmpty(leadRecruiter.firstName)) ? leadRecruiter.firstName : string.Empty);
                        exObjpair.Add("%rphone%", (leadRecruiter != null && !String.IsNullOrEmpty(leadRecruiter.phoneNumber)) ? leadRecruiter.phoneNumber : string.Empty);
                    }
                }
            }
        }

    }
}