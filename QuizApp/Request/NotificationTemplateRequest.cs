using QuizApp.Helpers;
using QuizApp.Services.Model;
using Swashbuckle.Examples;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Request
{
    public class NotificationTemplateRequest
    {
        public int Id { get; set; }
        public string TemplateTitle { get; set; }
        public string OfficeId { get; set; }
        public int NotificationType { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string SMSText { get; set; }
        public WhatsAppDetails WhatsApp { get; set; }
        public string EmailLinkVariable { get; set; }
        public List<string> MsgVariables { get; set; }
        public List<Quiz> QuizInTemplateList { get; set; }
        public List<TemplateAttachment> TemplateAttachmentList { get; set; }

        public class Quiz
        {
            public int Id { get; set; }
            public string QuizTitle { get; set; }
        }

        public NotificationTemplateModel MapRequestToEntity(NotificationTemplateRequest notificationTemplateRequestObj)
        {
            NotificationTemplateModel notificationTemplate = new NotificationTemplateModel();

            notificationTemplate.Id = notificationTemplateRequestObj.Id;
            notificationTemplate.TemplateTitle = notificationTemplateRequestObj.TemplateTitle;
            notificationTemplate.OfficeId = notificationTemplateRequestObj.OfficeId;

            switch (notificationTemplateRequestObj.NotificationType)
            {
                case 1:
                    notificationTemplate.NotificationType = NotificationTypeEnum.RESULT;
                    break;
                case 2:
                    notificationTemplate.NotificationType = NotificationTypeEnum.INVITATION;
                    break;
                case 3:
                    notificationTemplate.NotificationType = NotificationTypeEnum.REMINDER;
                    break;
            }

            notificationTemplate.Subject = notificationTemplateRequestObj.Subject;
            notificationTemplate.Body = notificationTemplateRequestObj.Body;
            notificationTemplate.SMSText = notificationTemplateRequestObj.SMSText;
            notificationTemplate.WhatsApp = notificationTemplateRequestObj.WhatsApp;
            notificationTemplate.EmailLinkVariable = notificationTemplateRequestObj.EmailLinkVariable;
            notificationTemplate.MsgVariables = notificationTemplateRequestObj.MsgVariables;

            notificationTemplate.QuizInTemplateList = new List<Services.Model.LocalQuiz>();

            if (notificationTemplateRequestObj.QuizInTemplateList != null && notificationTemplateRequestObj.QuizInTemplateList.Count > 0)
            {
                foreach (var item in notificationTemplateRequestObj.QuizInTemplateList)
                {
                    notificationTemplate.QuizInTemplateList.Add(new Services.Model.LocalQuiz
                    {
                        Id = item.Id,
                        QuizTitle = item.QuizTitle
                    });
                }
            }

            notificationTemplate.TemplateAttachmentList = new List<TemplateAttachment>();

            if (notificationTemplateRequestObj.TemplateAttachmentList != null)
            {
                foreach (var item in notificationTemplateRequestObj.TemplateAttachmentList)
                {
                    notificationTemplate.TemplateAttachmentList.Add(new TemplateAttachment
                    {
                        FileName = item.FileName,
                        FileUrl = item.FileUrl,
                        FileIdentifier = item.FileIdentifier
                    });
                }
            }

            return notificationTemplate;
        }
        public class NotificationTemplateRequestExample : IExamplesProvider
        {
            public object GetExamples()
            {
                return new
                {
                    Id = 0,
                    TemplateTitle = string.Empty,
                    OfficeId = string.Empty,
                    NotificationType = 1,
                    Subject = string.Empty,
                    Body = string.Empty,
                    SMSText = string.Empty,
                    EmailLinkVariable = string.Empty,
                    QuizInTemplateList = new List<Quiz>() { new Quiz() { Id = 1, QuizTitle = string.Empty } },
                    TemplateAttachmentList = new List<TemplateAttachment>() { new TemplateAttachment() { FileName = string.Empty, FileUrl = string.Empty , FileIdentifier= string.Empty} }
                };
            }
        }
    }
}