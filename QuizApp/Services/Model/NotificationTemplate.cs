using QuizApp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Services.Model
{
    public class NotificationTemplateModel : Base
    {
        public int Id { get; set; }
        public string TemplateTitle { get; set; }
        public string OfficeId { get; set; }
        public NotificationTypeEnum NotificationType { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string SMSText { get; set; }
        public WhatsAppDetails WhatsApp { get; set; }
        public string EmailLinkVariable { get; set; }
        public int? CompanyId { get; set; }
        public List<string> MsgVariables { get; set; }
        public List<LocalQuiz> QuizInTemplateList { get; set; }
        public List<TemplateAttachment> TemplateAttachmentList { get; set; }
    }

    public class InactiveNotificationTemplateTypeResponse
    {
        public List<NotificationTemplateQuizDetails> InactiveQuizList { get; set; }
        public int CurrentPageIndex { get; set; }
        public int TotalRecords { get; set; }
    }

    public class ActiveNotificationTemplateTypeResponse
    {
        public List<NotificationTemplateQuizDetails> QuizInTemplateList { get; set; }
        public int CurrentPageIndex { get; set; }
        public int TotalRecords { get; set; }
    }
    public class NotificationTemplateQuizDetails
    {
        public int Id { get; set; }
        public string QuizTitle { get; set; }
        public QuizTypeEnum QuizType { get; set; }
        public string PublishedCode { get; set; }
        public NotificationTemplateQuizCover QuizCoverDetails { get; set; }
        public int NoOfQusetions { get; set; }
        public bool IsFavorited { get; set; }
        public bool IsPublished { get; set; }
        public List<Tags> Tag { get; set; }
        public List<int> UsageTypes { get; set; }
        public class NotificationTemplateQuizCover
        {
            public string QuizCoverTitle { get; set; }
            public string QuizCoverImage { get; set; }
            public string PublicId { get; set; }
        }
    }

    public class InactiveNotificationTemplate
    {
        public int NotificationType { get; set; }
        public int QuizId { get; set; }
        public int TemplateId { get; set; }
    }

}