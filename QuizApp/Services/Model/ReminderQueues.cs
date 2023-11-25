using QuizApp.Services.Service;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net.Mail;
using System.Web;
using static QuizApp.Helpers.Models;

namespace QuizApp.Services.Model
{
    public class ReminderQueuesModel : Base
    {
        public int Id { get; set; }
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string ToPhone { get; set; }
        public string SMSText { get; set; }
        public WhatsappBodyDetails WhatsappBody { get; set; }
        public TempWorkpackagePush.ContactBasicDetails LeadUserInfo { get; set; }
        public DateTime? SentOn { get; set; }
        public int Type { get; set; }
        public bool Sent { get; set; }
        public string MsgVariables { get; set; }  
        public List<FileAttachment> Attachments { get; set; }
        public CompanyModel Company { get; set; }
    }
    
    public class WhatsappBodyDetails 
    {
    public string ContactPhone { get; set; }
        public Dictionary<string, object> ObjectFields { get; set; }
        public Dictionary<string, object> StaticObjects { get; set; }
        public string FollowUpMessage { get; set; }
        public string MsgVariables { get; set; }
        public int HsmTemplateId { get; set; }
        public string LanguageCode { get; set; }
        public int WorkPackageInfoId { get; set; }
        public string UserToken { get; set; }
        public string LeadUserId { get; set; }
        public string ContactOwnerId { get; set; }
        public string SourceOwnerId { get; set; }
        public string LeadOwnerId { get; set; }
        public string Clientcode { get; set; }
        public List<Db.TemplateParameterInConfigurationDetails> TemplateParameterInConfigurationDetails { get; set; }
        public TempWorkpackagePush TempWorkpackagePush { get; set; }
    }
}