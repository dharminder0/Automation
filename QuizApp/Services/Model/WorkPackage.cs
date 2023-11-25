using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Services.Model
{
    public class WorkPackage : Base
    {
        public string LeadUserId { get; set; }
        public int QuizId { get; set; }
        public int? BusinessUserId { get; set; }
        public string SourceId { get; set; }
        public string SourceName { get; set; }
        public bool IsUpdatedSend { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string SMSText { get; set; }
        public bool? SendEmail { get; set; }
        public bool? SendSms { get; set; }
        public bool? SendWhatsApp { get; set; }
        public bool SendMailNotRequired { get; set; }
        public Dictionary<string, string> DynamicVariables { set; get; }
        public List<LeadDataInActionModel> LeadDataInActionList { get; set; }
        public string CompanyCode { get; set; }
        public List<string> ContactIds { get; set; }
        public int? ConfigurationId { get; set; }

        public class LeadDataInActionModel
        {
            public int ActionId { get; set; }
            public int AppointmentTypeId { get; set; }
            public string ReportEmails { get; set; }
            public List<int> CalendarIds { get; set; }
            public bool IsUpdatedSend { get; set; }
            public string Subject { get; set; }
            public string Body { get; set; }
            public string SMSText { get; set; }
            public bool SendMailNotRequired { get; set; }
        }
    }

    public class PushWorkPackage : Base
    {
        public string CompanyCode { get; set; }
        public int WorkPackageInfoId { get; set; } = 0;
        public List<string> ContactIds { get; set; }
        public string ConfigurationId { get; set; }
        public string UserToken { get; set; } = "";
        public string RequestId { get; set; }
    }
    public class QuizUrl
    {
        public string ShortUrl { get; set; }
    }

    public class AddOrUpdateConfiguration : Base
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

        public class EmailAttachment
        {
            public string FileName { get; set; }
            public string FileIdentifier { get; set; }
            public string FileLink { get; set; }
        }

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
    }

    public class SendSMS
    {
        public string ModuleName { get; set; }
        public int WorkPackageId { get; set; }
        public string ClientCode { get; set; }
    }

    public class PrivacyDto
    {
        public bool IsMandatory { get; set; }
        public bool IsCVMandatory { get; set; }
        public string PrivacyLabel { get; set; }
        public string PrivacyLink { get; set; }
        public string CountryCode { get; set; }
    }
}