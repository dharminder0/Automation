using QuizApp.Helpers;
using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static QuizApp.Helpers.Models;

namespace QuizApp.Response
{
    public class OWCUserBasic {
        public int Id { get; set; }
        public string ExternalUserId { get; set; }
        public int userId { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string userName { get; set; }
        public string Mail { get; set; }
        public string Email { get; set; }
        public string nickName { get; set; }
        public string token { get; set; }
        public string avatar { get; set; }
        public string avatarPublicId { get; set; }
        public string activeLanguage { get; set; }
        public string timeZone { get; set; }
        public string phoneNumber { get; set; }
        public string Phone { get; set; }
    }

        public class OWCBusinessUserResponse  : OWCUserBasic {
       
        public List<Offices> offices { get; set; }
        public List<Offices> officesParentChild { get; set; }
        public List<Offices> preferredOffices { get; set; }
        public List<HeaderMenus> headerMenus { get; set; }
        public List<HeaderMenusNew> headerMenusNew { get; set; }
        public float offset { get; set; }
        public string cloudinaryKey { get; set; }
        public List<string> permissions { get; set; }
        public CloudinarySetting cloudinarySettings { get; set; }
        public string accountLoginType { get; set; }
    }
    
    public class OWCUserVariable : OWCUserBasic {
        public string ObjectUserOwnerType { get; set; }
    }

        public class OWCCompanyResponse
    {
        public int id { get; set; }
        public string clientCode { get; set; }
        public string jobRocketApiUrl { get; set; }
        public string jobRocketApiAuthorizationBearer { get; set; }
        public string jobRocketClientUrl { get; set; }
        public string leadDashboardApiUrl { get; set; }
        public string leadDashboardApiAuthorizationBearer { get; set; }
        public string leadDashboardClientUrl { get; set; }
        public string primaryBrandingColor { get; set; }
        public string secondaryBrandingColor { get; set; }
        public string tertiaryColor { get; set; }
        public string logoUrl { get; set; }
        public string companyName { get; set; }
        public string alternateClientCodes { get; set; }
        public string companyWebsiteUrl { get; set; }
        public string cloudinaryCompanyName { get; set; }
        public string cloudinaryApikey { get; set; }
        public string cloudinaryUsername { get; set; }
        public string cloudinaryBaseFolder { get; set; }
        public string uiFooter { get; set; }
        public bool CreateAcademyCourseEnabled { get; set; }
        public bool CreateTechnicalRecruiterSkillsEnabled { get; set; }
        public bool CreateTemplateEnabled { get; set; }
        public bool BadgesEnabled { get; set; }
        public List<GtmSetting> gtmSettings { get; set; }
    }

    public class OwcVerificationResponse
    {
        public ResultEnum Status { get; set; }
        public string Error { get; set; }
        public OWCCompanyResponse OWCCompanyResponse { get; set; }
        public OWCBusinessUserResponse OWCBusinessUserResponse { get; set; }
    }

    public class OWCLeadUserResponse
    {
        public int count { get; set; }
        public List<LeadUserResponse> contacts { get; set; }

        public class LeadUserResponse
        {
            public string contactId { get; set; }
            public string firstName { get; set; }
            public string lastName { get; set; }
            public string email { get; set; }
            public string telephone { get; set; }
            public string SourceId { get; set; }
            public string SourceName { get; set; }
            public int ContactOwnerId { get; set; }
            public int SourceOwnerId { get; set; }
        }
    }

    public class OWCLeadTagsResponse
    {
        public int id { get; set; }
        public string tagName { get; set; }
        public string tagCategory { get; set; }
        public int tagCategoryId { get; set; }
    }

    public class OWCLeadTagCategoriesResponse
    {
        public string tagCategoryName { get; set; }
        public int tagCategoryId { get; set; }
    }

    public class AppointmentTypeList
    {
        public List<AppointmentTypeDetail> Data { get; set; }
    }
    public class AppointmentTypeDetail
    {
        public int Id { get; set; }
        public string AppointmentTypeName { get; set; }
    }


    public class ConfigurationData
    {
        public ConfigurationDataDetails Data { get; set; }
    }

    public class ConfigurationDataDetails
    {
        public string ConfigurationId { get; set; }
        public int QuizId { get; set; }
        public Dictionary<string, string> DynamicVariables { set; get; }
        public List<LeadDataInActionResponse> LeadDataInActionList { get; set; }
        public bool IsUpdatedSend { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string SmsText { get; set; }
        public bool? SendEmail { get; set; }
        public bool? SendSms { get; set; }
        public bool? SendWhatsApp { get; set; }
        public bool? SendFallbackSms { get; set; }
        public List<EmailAttachment> EmailAttachments { get; set; }
        public bool SendMailNotRequired { get; set; }
        public string CompanyCode { get; set; }
        public string SourceType { get; set; }
        public string SourceId { get; set; }
        public string SourceTitle { get; set; }
        public string PrivacyLink { get; set; }
        public string ConfigurationType { get; set; }
        public string LeadFormTitle { get; set; }
        public MediaVariableDetail MediaVariableDetails { get; set; }
        public WhatsAppDetails WhatsApp { get; set; }

        public class LeadDataInActionResponse
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

        public class WhatsAppDetails
        {
            public int HsmTemplateId { get; set; }
            public List<TemplateParameter> TemplateParameters { get; set; }
            public string FollowUpMessage { get; set; }
        }

        public class TemplateParameter
        {
            public string Paraname { get; set; }
            public int Position { get; set; }
            public string Value { get; set; }
        }
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

    public class ObjectFieldsData
    {
        public List<ObjectFields> data { get; set; }
    }

    public class ObjectFields
    {
        public string ObjectName { get; set; }
        public string ObjectDisplayName { get; set; }
        public List<field> Fields { get; set; }

        public class field
        {
            public string Name { get; set; }
            public string DisplayName { get; set; }
            public string DataType { get; set; }
            public string AtsFieldName { get; set; }
            public List<StringList> FieldListValues { get; set; }
        }
        public class StringList
        {
            public string Label { get; set; }
            public string Value { get; set; }
        }
    }

    public class UpdateQuizStatusResponse
    {
        public string status { get; set; }
        public string appointmentCode { get; set; }
        public string appointmentLink { get; set; }
        public string message { get; set; }
    }

    public class ClientDomainsResponse
    {
        public string Domain { get; set; }
        public string GtmCode { get; set; }
        public string FavoriteIconUrl { get; set; }
    }

    public class SignatureDetails
    {
        public string clientCode { get; set; }
        public object brandName { get; set; }
        public string signatureType { get; set; }
        public string signatureText { get; set; }
    }

    public class UserMediaClassification
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ClientCode { get; set; }
        public string UserToken { get; set; }
        public int? MediaFileId { get; set; }
        public string MediaTitle { get; set; }
        public string MediaPublicId { get; set; }
        public string MediaUrl { get; set; }
        public string ResourceType { get; set; }
    }

    public class HubTemplateLinkedAutomation
    {
        public string ConfigurationId { get; set; }
        public string ConfigurationType { get; set; }
        public string TemplateName { get; set; }
        public int? QuizId { get; set; }
        public List<string> AutomationConfigurationIds { get; set; }
    }

    public class WhatsAppTemplateDtos
    {
        public int HsmTemplateId { get; set; }
        public int HsmTemplateLanguageCode { get; set; }
        public List<ParamDto> TemplateParameters { get; set; }
        public string FollowUpMessage { get; set; }
        public string TemplateName { get; set; }

        public class ParamDto
        {
            public string paraname { get; set; }
            public int position { get; set; }
            public string value { get; set; }
        }
    }


    public class WhatsAppTemplateDto
    {
        public int Id { get; set; }
        public string TemplateName { get; set; }
        public string DisplayName { get; set; }
        public string TemplateLanguage { get; set; }
        public string TemplateBody { get; set; }  
        public string Provider { get; set; }
        public Customcomponent[] CustomComponents { get; set; }
        public List<WhatsappParam> Params { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public string[] TemplateTypes { get; set; }
        public bool? IsConsentTemplate { get; set; }
        public object CategoryId { get; set; }
        public object CategoryName { get; set; }
        public string Status { get; set; }
    }

    public class Customcomponent
    {
        public string Type { get; set; }
        public Item[] Items { get; set; }
    }

    public class Item
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public ItemMappedField[] MappedFields { get; set; }
    }

    public class ItemMappedField
    {
        public string ObjectName { get; set; }
        public string FieldName { get; set; }
        public string Value { get; set; }
    }
    public class VariablesObjectFieldDto {
        public int ObjectId { get; set; }
        public string ObjectName { get; set; }
        public IEnumerable<VariablefieldListDto> Fields { get; set; }

        public class VariablefieldListDto {
            public string FieldName { get; set; }
            public string FieldLabel { get; set; }
            public string FieldType { get; set; }
        }
    }

    public class  LoggedInUser {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NickName { get; set; }
        public string Gender { get; set; }
        public object PhoneNumber { get; set; }
        public string Mail { get; set; }
        public string BirthDate { get; set; }
    }

    public class TemplateParameters {
        public string Paraname { get; set; }
        public int Position { get; set; }
        public string Value { get; set; }

    }
    public class WhatsAppBody {
        public int HsmTemplateId { get; set; }
        public string HsmTemplateLanguageCode { get; set; }
        public List<TemplateParameters> TemplateParameters { get; set; }
        public string FollowUpMessage { get; set; }

    }


}