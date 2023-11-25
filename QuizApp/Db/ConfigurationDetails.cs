using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QuizApp.Db
{
    public class ConfigurationDetails
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("Quiz")]
        public int QuizId { get; set; }
        public virtual Quiz Quiz { get; set; }

        public string ConfigurationId { get; set; }
        public bool IsUpdatedSend { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string SMSText { get; set; }
        public bool SendMailNotRequired { get; set; }
        public string CompanyCode { get; set; }
        public string SourceType { get; set; }
        public string SourceId { get; set; }
        public string SourceName { get; set; }
        public string PrivacyLink { get; set; }
        public string PrivacyJson { get; set; }
        public int Status { get; set; }
        public string ConfigurationType { get; set; }
        public string LeadFormTitle { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public int? HsmTemplateId { get; set; }
        public string HsmTemplateLanguageCode { get; set; }
        public string FollowUpMessage { get; set; }
        public bool? SendEmail { get; set; }
        public bool? SendSms { get; set; }
        public bool? SendWhatsApp { get; set; }
        public bool? SendFallbackSms { get; set; }
        public string MsgVariables { get; set; }
        public virtual ICollection<QuizAttempts> QuizAttempts { get; set; }
        public virtual ICollection<VariablesDetails> VariablesDetails { get; set; }
        public virtual ICollection<LeadDataInAction> LeadDataInAction { get; set; }
        public virtual ICollection<WorkPackageInfo> WorkPackageInfo { get; set; }
        public virtual ICollection<MediaVariablesDetails> MediaVariablesDetails { get; set; }
        public virtual ICollection<ResultIdsInConfigurationDetails> ResultIdsInConfigurationDetails { get; set; }
        public virtual ICollection<AttachmentsInConfiguration> AttachmentsInConfiguration { get; set; }
        public virtual ICollection<TemplateParameterInConfigurationDetails> TemplateParameterInConfigurationDetails { get; set; }
    }
}