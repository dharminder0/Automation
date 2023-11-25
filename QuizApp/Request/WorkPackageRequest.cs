using QuizApp.Helpers;
using QuizApp.Response;
using QuizApp.Services.Model;
using Swashbuckle.Examples;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Request
{
    public class WorkPackageRequest
    {       
        public string CompanyCode { get; set; }
        public List<string> ContactIds { get; set; }
        public string ConfigurationId { get; set; }
        public string UserToken { get; set; } = "";

        public int WorkPackageInfoId { get; set; } = 0;
        public string RequestId { get; set; }

        public PushWorkPackage MapRequestToEntityWithConfiguration(WorkPackageRequest workPackageObj)
        {
            PushWorkPackage obj = new PushWorkPackage();

            obj.CompanyCode = workPackageObj.CompanyCode;
            obj.ConfigurationId = workPackageObj.ConfigurationId;
            obj.ContactIds = workPackageObj.ContactIds;
            obj.WorkPackageInfoId = workPackageObj.WorkPackageInfoId;
            obj.UserToken = workPackageObj.UserToken;
            obj.RequestId = workPackageObj.RequestId;
            return obj;
        }

        public class WorkPackageRequestExample : IExamplesProvider
        {
            public object GetExamples()
            {
                return new WorkPackageRequest
                {
                    CompanyCode = string.Empty,
                    ContactIds = new List<string>() { },
                    ConfigurationId = string.Empty,
                    RequestId = string.Empty,
                };
            }
        }
    }

    public class DataInAction
    {
        public int ActionId { get; set; }
        public int ParentId { get; set; }
        public int AppointmentTypeId { get; set; }
        public string ReportEmails { get; set; }
        public List<int> CalendarIds { get; set; }
        public bool IsUpdatedSend { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string SMSText { get; set; }
        public bool SendMailNotRequired { get; set; }
    }

    public class MediaVariableDetail
    {
        public List<Data> CoverDetails { get; set; }
        public List<ContentData> Questions { get; set; }
        public List<Data> Answers { get; set; }
        public List<Data> Results { get; set; }
        public List<ContentData> Content { get; set; }
        public List<Data> Badges { get; set; }
    }

    public class Data
    {
        public int ParentId { get; set; }
        public string MediaUrlValue { get; set; }
        public string PublicId { get; set; }
        public string MediaOwner { get; set; }
        public string ProfileMedia { get; set; }
    }

    public class ContentData
    {
        public int ParentId { get; set; }
        public string MediaUrlValue { get; set; }
        public string PublicId { get; set; }
        public string MediaUrlforDescriptionValue { get; set; }
        public string PublicIdforDescription { get; set; }
        public string MediaOwner { get; set; }
        public string ProfileMedia { get; set; }
        public string MediaOwnerforDescription { get; set; }
        public string ProfileMediaforDescription { get; set; }
    }

    public class AddOrUpdateConfigurationDetailsRequest
    {
        public int QuizId { get; set; }
        public bool IsUpdatedSend { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string SMSText { get; set; }
        public bool? SendEmail { get; set; }
        public bool? SendSms { get; set; }
        public bool? SendWhatsApp { get; set; }
        public bool? SendFallbackSms { get; set; }
        public List<string> MsgVariables { get; set; }
        public List<EmailAttachment> EmailAttachments { get; set; }
        public bool SendMailNotRequired { get; set; }
        public string CompanyCode { get; set; }
        public string SourceType { get; set; }
        public string SourceId { get; set; }
        public string SourceTitle { get; set; }
        public string PrivacyLink { get; set; }
        public List<int> ResultIds { get; set; }
        public List<LeadFormDetailofResult> LeadFormDetailofResults { get; set; }
        public string ConfigurationType { get; set; }
        public string LeadFormTitle { get; set; }
        public Dictionary<string, string> DynamicVariables { set; get; }
        public List<DataInAction> LeadDataInActionList { get; set; }
        public string ConfigurationId { get; set; }
        public MediaVariableDetail MediaVariableDetails { get; set; }
        public WhatsAppDetails WhatsApp { get; set; }
        public PrivacyDto PrivacyJson { get; set; }

        public class WhatsAppDetails
        {
            public int HsmTemplateId { get; set; }
            public string HsmTemplateLanguageCode { get; set; }
            public List<TemplateParameter> TemplateParameters { get; set; }
            public string FollowUpMessage { get; set; }
        }

        public class TemplateParameter
        {
            public string Paraname { get; set; }
            public int Position { get; set; }
            public string Value { get; set; }
        }

        public class LeadFormDetailofResult
        {
            public int ResultId { get; set; }
            public int FormId { get; set; }
            public int FlowOrder { get; set; }
        }

        public AddOrUpdateConfiguration MapRequestToEntity(AddOrUpdateConfigurationDetailsRequest workPackageObj)
        {
            AddOrUpdateConfiguration obj = new AddOrUpdateConfiguration();

            obj.QuizId = workPackageObj.QuizId;
            obj.IsUpdatedSend = workPackageObj.IsUpdatedSend;
            obj.Subject = workPackageObj.Subject;
            obj.Body = workPackageObj.Body;
            obj.SMSText = workPackageObj.SMSText;
            obj.SendEmail = workPackageObj.SendEmail;
            obj.SendSms = workPackageObj.SendSms;
            obj.SendWhatsApp = workPackageObj.SendWhatsApp;
            obj.SendFallbackSms = workPackageObj.SendFallbackSms;
            obj.MsgVariables = workPackageObj.MsgVariables;
            obj.SendMailNotRequired = workPackageObj.SendMailNotRequired;
            obj.SourceType = workPackageObj.SourceType;
            obj.SourceId = workPackageObj.SourceId;
            obj.SourceTitle = workPackageObj.SourceTitle;
            obj.PrivacyLink = workPackageObj.PrivacyLink;
            obj.ResultIds = workPackageObj.ResultIds;
            obj.ConfigurationType = workPackageObj.ConfigurationType;
            obj.CompanyCode = workPackageObj.CompanyCode;
            obj.ConfigurationId = workPackageObj.ConfigurationId;
            obj.LeadFormTitle = workPackageObj.LeadFormTitle;
            obj.PrivacyJson = workPackageObj.PrivacyJson;

            obj.WhatsApp = new AddOrUpdateConfiguration.WhatsAppDetails();

            if (workPackageObj.WhatsApp != null)
            {
                obj.WhatsApp.HsmTemplateId = workPackageObj.WhatsApp.HsmTemplateId;
                obj.WhatsApp.HsmTemplateLanguageCode = workPackageObj.WhatsApp.HsmTemplateLanguageCode;
                obj.WhatsApp.FollowUpMessage = workPackageObj.WhatsApp.FollowUpMessage;
                obj.WhatsApp.TemplateParameters = new List<AddOrUpdateConfiguration.TemplateParameter>();
                if (workPackageObj.WhatsApp.TemplateParameters != null)
                {
                    foreach (var templateParametersObj in workPackageObj.WhatsApp.TemplateParameters)
                    {
                        obj.WhatsApp.TemplateParameters.Add(new AddOrUpdateConfiguration.TemplateParameter()
                        {
                            Paraname = templateParametersObj.Paraname,
                            Position = templateParametersObj.Position,
                            Value = templateParametersObj.Value
                        });
                    }
                }
            }

            obj.DynamicVariables = new Dictionary<string, string>();
            if (workPackageObj.DynamicVariables != null)
            {
                foreach (var dynamicVariablesObj in workPackageObj.DynamicVariables.Where(t => !string.IsNullOrEmpty(t.Key) && !string.IsNullOrEmpty(t.Value)))
                {
                    obj.DynamicVariables.Add(dynamicVariablesObj.Key, dynamicVariablesObj.Value);
                }
            }
            obj.LeadDataInActionList = new List<AddOrUpdateConfiguration.DataInAction>();

            foreach (var linkingObj in workPackageObj.LeadDataInActionList.Where(t => t.ParentId > 0 || t.ActionId > 0))
            {
                var AppointmentAndEmailLinkingObj = new AddOrUpdateConfiguration.DataInAction();
                AppointmentAndEmailLinkingObj.ActionId = linkingObj.ActionId;
                AppointmentAndEmailLinkingObj.ParentId = linkingObj.ParentId;
                AppointmentAndEmailLinkingObj.AppointmentTypeId = linkingObj.AppointmentTypeId;
                AppointmentAndEmailLinkingObj.ReportEmails = linkingObj.ReportEmails;
                AppointmentAndEmailLinkingObj.CalendarIds = linkingObj.CalendarIds;
                AppointmentAndEmailLinkingObj.IsUpdatedSend = linkingObj.IsUpdatedSend;
                AppointmentAndEmailLinkingObj.Subject = linkingObj.Subject;
                AppointmentAndEmailLinkingObj.Body = linkingObj.Body;
                AppointmentAndEmailLinkingObj.SMSText = linkingObj.SMSText;
                AppointmentAndEmailLinkingObj.SendMailNotRequired = linkingObj.SendMailNotRequired;
                obj.LeadDataInActionList.Add(AppointmentAndEmailLinkingObj);
            }

            obj.MediaVariableDetails = new AddOrUpdateConfiguration.MediaVariableDetail();

            if (workPackageObj.MediaVariableDetails != null)
            {
                obj.MediaVariableDetails.Questions = new List<AddOrUpdateConfiguration.ContentData>();

                if (workPackageObj.MediaVariableDetails.Questions != null)
                {
                    foreach (var mediaVariablesObj in workPackageObj.MediaVariableDetails.Questions.Where(r => r.ParentId > 0))
                    {
                        var dataObj = new AddOrUpdateConfiguration.ContentData();
                        dataObj.ParentId = mediaVariablesObj.ParentId;
                        dataObj.MediaUrlValue = mediaVariablesObj.MediaUrlValue;
                        dataObj.PublicId = mediaVariablesObj.PublicId;
                        dataObj.MediaUrlforDescriptionValue = mediaVariablesObj.MediaUrlforDescriptionValue;
                        dataObj.PublicIdforDescription = mediaVariablesObj.PublicIdforDescription;
                        dataObj.MediaOwner = mediaVariablesObj.MediaOwner;
                        dataObj.ProfileMedia = mediaVariablesObj.ProfileMedia;
                        dataObj.MediaOwnerforDescription = mediaVariablesObj.MediaOwnerforDescription;
                        dataObj.ProfileMediaforDescription = mediaVariablesObj.ProfileMediaforDescription;

                        obj.MediaVariableDetails.Questions.Add(dataObj);
                    }
                }

                obj.MediaVariableDetails.Answers = new List<AddOrUpdateConfiguration.Data>();

                if (workPackageObj.MediaVariableDetails.Answers != null)
                {
                    foreach (var mediaVariablesObj in workPackageObj.MediaVariableDetails.Answers.Where(r => r.ParentId > 0))
                    {
                        var dataObj = new AddOrUpdateConfiguration.Data();
                        dataObj.ParentId = mediaVariablesObj.ParentId;
                        dataObj.MediaUrlValue = mediaVariablesObj.MediaUrlValue;
                        dataObj.PublicId = mediaVariablesObj.PublicId;
                        dataObj.MediaOwner = mediaVariablesObj.MediaOwner;
                        dataObj.ProfileMedia = mediaVariablesObj.ProfileMedia;

                        obj.MediaVariableDetails.Answers.Add(dataObj);
                    }
                }

                obj.MediaVariableDetails.Results = new List<AddOrUpdateConfiguration.Data>();

                if (workPackageObj.MediaVariableDetails.Results != null)
                {
                    foreach (var mediaVariablesObj in workPackageObj.MediaVariableDetails.Results.Where(r => r.ParentId > 0))
                    {
                        var dataObj = new AddOrUpdateConfiguration.Data();
                        dataObj.ParentId = mediaVariablesObj.ParentId;
                        dataObj.MediaUrlValue = mediaVariablesObj.MediaUrlValue;
                        dataObj.PublicId = mediaVariablesObj.PublicId;
                        dataObj.MediaOwner = mediaVariablesObj.MediaOwner;
                        dataObj.ProfileMedia = mediaVariablesObj.ProfileMedia;

                        obj.MediaVariableDetails.Results.Add(dataObj);
                    }
                }

                obj.MediaVariableDetails.CoverDetails = new List<AddOrUpdateConfiguration.Data>();

                if (workPackageObj.MediaVariableDetails.CoverDetails != null)
                {
                    foreach (var mediaVariablesObj in workPackageObj.MediaVariableDetails.CoverDetails.Where(r => r.ParentId > 0))
                    {
                        var dataObj = new AddOrUpdateConfiguration.Data();
                        dataObj.ParentId = mediaVariablesObj.ParentId;
                        dataObj.MediaUrlValue = mediaVariablesObj.MediaUrlValue;
                        dataObj.PublicId = mediaVariablesObj.PublicId;
                        dataObj.MediaOwner = mediaVariablesObj.MediaOwner;
                        dataObj.ProfileMedia = mediaVariablesObj.ProfileMedia;

                        obj.MediaVariableDetails.CoverDetails.Add(dataObj);
                    }
                }

                obj.MediaVariableDetails.Badges = new List<AddOrUpdateConfiguration.Data>();

                if (workPackageObj.MediaVariableDetails.Badges != null)
                {
                    foreach (var mediaVariablesObj in workPackageObj.MediaVariableDetails.Badges.Where(r => r.ParentId > 0))
                    {
                        var dataObj = new AddOrUpdateConfiguration.Data();
                        dataObj.ParentId = mediaVariablesObj.ParentId;
                        dataObj.MediaUrlValue = mediaVariablesObj.MediaUrlValue;
                        dataObj.PublicId = mediaVariablesObj.PublicId;
                        dataObj.MediaOwner = mediaVariablesObj.MediaOwner;
                        dataObj.ProfileMedia = mediaVariablesObj.ProfileMedia;

                        obj.MediaVariableDetails.Badges.Add(dataObj);
                    }
                }

                obj.MediaVariableDetails.Content = new List<AddOrUpdateConfiguration.ContentData>();

                if (workPackageObj.MediaVariableDetails.Content != null)
                {
                    foreach (var mediaVariablesObj in workPackageObj.MediaVariableDetails.Content.Where(r => r.ParentId > 0))
                    {
                        var dataObj = new AddOrUpdateConfiguration.ContentData();
                        dataObj.ParentId = mediaVariablesObj.ParentId;
                        dataObj.MediaUrlValue = mediaVariablesObj.MediaUrlValue;
                        dataObj.PublicId = mediaVariablesObj.PublicId;
                        dataObj.MediaUrlforDescriptionValue = mediaVariablesObj.MediaUrlforDescriptionValue;
                        dataObj.PublicIdforDescription = mediaVariablesObj.PublicIdforDescription;
                        dataObj.MediaOwner = mediaVariablesObj.MediaOwner;
                        dataObj.ProfileMedia = mediaVariablesObj.ProfileMedia;
                        dataObj.MediaOwnerforDescription = mediaVariablesObj.MediaOwnerforDescription;
                        dataObj.ProfileMediaforDescription = mediaVariablesObj.ProfileMediaforDescription;

                        obj.MediaVariableDetails.Content.Add(dataObj);
                    }
                }
            }

            obj.EmailAttachments = new List<AddOrUpdateConfiguration.EmailAttachment>();

            if (workPackageObj.EmailAttachments != null)
            {
                foreach (var emailAttachmentsObj in workPackageObj.EmailAttachments.Where(t => !string.IsNullOrEmpty(t.FileIdentifier) && !string.IsNullOrEmpty(t.FileLink)))
                {
                    obj.EmailAttachments.Add(new AddOrUpdateConfiguration.EmailAttachment()
                    {
                        FileIdentifier = emailAttachmentsObj.FileIdentifier,
                        FileLink = emailAttachmentsObj.FileLink,
                        FileName = emailAttachmentsObj.FileName
                    });
                }
            }

            obj.LeadFormDetailofResults = new List<AddOrUpdateConfiguration.LeadFormDetailofResult>();

            if (workPackageObj.LeadFormDetailofResults != null)
            {
                foreach (var emailAttachmentsObj in workPackageObj.LeadFormDetailofResults)
                {
                    obj.LeadFormDetailofResults.Add(new AddOrUpdateConfiguration.LeadFormDetailofResult()
                    {
                        ResultId = emailAttachmentsObj.ResultId,
                        FormId = emailAttachmentsObj.FormId,
                        FlowOrder = emailAttachmentsObj.FlowOrder
                    });
                }
            }

            return obj;
        }

        public class AddOrUpdateConfigurationDetailsRequestExample : IExamplesProvider
        {
            public object GetExamples()
            {
                var dictionary = new Dictionary<string, string>();
                dictionary.Add(string.Empty, string.Empty);

                var lst = new List<DataInAction>();
                lst.Add(new DataInAction()
                {
                    ActionId = 0,
                    ParentId = 0,
                    AppointmentTypeId = 0,
                    ReportEmails = string.Empty,
                    CalendarIds = new List<int>() { },
                    IsUpdatedSend = false,
                    Subject = string.Empty,
                    Body = string.Empty,
                    SMSText = string.Empty,
                    SendMailNotRequired = false,
                });

                var LeadFormDetailofResultsLst = new List<LeadFormDetailofResult>();
                LeadFormDetailofResultsLst.Add(new LeadFormDetailofResult() { ResultId = 1, FormId = 1, FlowOrder = 1});

                var mediaLst = new MediaVariableDetail();
                mediaLst.Questions = new List<ContentData>() { new ContentData() { PublicId = string.Empty, MediaUrlValue = string.Empty, PublicIdforDescription = string.Empty, MediaUrlforDescriptionValue = string.Empty, ParentId = 0 } };
                mediaLst.Answers = new List<Data>() { new Data() { PublicId = string.Empty, MediaUrlValue = string.Empty, ParentId = 0 } };
                mediaLst.Results = new List<Data>() { new Data() { PublicId = string.Empty, MediaUrlValue = string.Empty, ParentId = 0 } };
                mediaLst.CoverDetails = new List<Data>() { new Data() { PublicId = string.Empty, MediaUrlValue = string.Empty, ParentId = 0} };
                mediaLst.Badges = new List<Data>() { new Data() { PublicId = string.Empty, MediaUrlValue = string.Empty, ParentId = 0 } };
                mediaLst.Content = new List<ContentData>() { new ContentData() { PublicId = string.Empty, MediaUrlValue = string.Empty, PublicIdforDescription = string.Empty, MediaUrlforDescriptionValue = string.Empty, ParentId = 0 } };

                var emailAttachments = new List<EmailAttachment>();
                emailAttachments.Add(new EmailAttachment()
                {
                    FileName = string.Empty,
                    FileIdentifier = string.Empty,
                    FileLink = string.Empty
                });

                var whatsapp = new WhatsAppDetails();
                whatsapp.HsmTemplateId = 0;
                whatsapp.FollowUpMessage = string.Empty;
                whatsapp.TemplateParameters = new List<TemplateParameter>();

                var templateparameter = new TemplateParameter();
                templateparameter.Paraname = string.Empty;
                templateparameter.Position = 0;
                templateparameter.Value = string.Empty;

                whatsapp.TemplateParameters.Add(templateparameter);

                return new AddOrUpdateConfigurationDetailsRequest
                {
                    QuizId = 1,
                    IsUpdatedSend = false,
                    Subject = string.Empty,
                    Body = string.Empty,
                    SMSText = string.Empty,
                    SendEmail = null,
                    SendSms = null,
                    SendWhatsApp = null,
                    SendMailNotRequired = false,
                    SourceType = string.Empty,
                    SourceId = string.Empty,
                    SourceTitle = string.Empty,
                    PrivacyLink = string.Empty,
                    ResultIds = new List<int>(),
                    LeadFormDetailofResults = LeadFormDetailofResultsLst,
                    ConfigurationType = string.Empty,
                    CompanyCode = string.Empty,
                    LeadFormTitle = string.Empty,
                    DynamicVariables = dictionary,
                    LeadDataInActionList = lst,
                    ConfigurationId = string.Empty,
                    MediaVariableDetails = mediaLst,
                    EmailAttachments = emailAttachments,
                    WhatsApp = whatsapp
                };
            }
        }
    }

    public class SendSMSRequest
    {
        public string ModuleName { get; set; }
        public int WorkPackageId { get; set; }
        public string ClientCode { get; set; }

        public SendSMS MapRequestToEntity(SendSMSRequest sendSMSObj)
        {
            SendSMS obj = new SendSMS();

            obj.ModuleName = sendSMSObj.ModuleName;
            obj.WorkPackageId = sendSMSObj.WorkPackageId;
            obj.ClientCode = sendSMSObj.ClientCode;

            return obj;
        }

        public class SendSMSRequestExample : IExamplesProvider
        {
            public object GetExamples()
            {

                return new SendSMSRequest
                {
                    ModuleName = "Automation",
                    WorkPackageId = 1,
                    ClientCode = string.Empty,
                };
            }
        }
    }
}
