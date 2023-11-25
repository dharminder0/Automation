using QuizApp.Helpers;
using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Response
{
    public class QuizInTemplate
    {
        public int Id { get; set; }
        public string QuizTitle { get; set; }
        public QuizTypeEnum QuizType { get; set; }
        public string PublishedCode { get; set; }
        public QuizCover QuizCoverDetails { get; set; }
        public int NoOfQusetions { get; set; }
        public bool IsFavorited { get; set; }
        public bool IsPublished { get; set; }
        public List<Tags> Tag { get; set; }
        public List<int> UsageTypes { get; set; }
        public class QuizCover
        {
            public string QuizCoverTitle { get; set; }
            public string QuizCoverImage { get; set; }
            public string PublicId { get; set; }
        }
    }

    public class QuizInTemplateList
    {
        public List<QuizInTemplate> quiz { get; set; }
        public int CurrentPageIndex { get; set; }
        public int TotalRecords { get; set; }

    }

    public class NotificationTemplateResponse : IResponse
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
        public List<QuizInTemplate> QuizInTemplateList { get; set; }
        public List<TemplateAttachment> TemplateAttachmentList { get; set; }

        public IResponse MapEntityToResponse(Base obj)
        {
            NotificationTemplateResponse notificationTemplateResponse = new NotificationTemplateResponse();
            var notificationTemplateObj = (NotificationTemplateModel)obj;

            notificationTemplateResponse.Id = notificationTemplateObj.Id;
            notificationTemplateResponse.TemplateTitle = notificationTemplateObj.TemplateTitle;
            notificationTemplateResponse.NotificationType = (int)notificationTemplateObj.NotificationType;
            notificationTemplateResponse.OfficeId = notificationTemplateObj.OfficeId;
            notificationTemplateResponse.Subject = notificationTemplateObj.Subject;
            notificationTemplateResponse.Body = notificationTemplateObj.Body;
            notificationTemplateResponse.SMSText = notificationTemplateObj.SMSText;
            notificationTemplateResponse.WhatsApp = notificationTemplateObj.WhatsApp;
            notificationTemplateResponse.EmailLinkVariable = notificationTemplateObj.EmailLinkVariable;
            notificationTemplateResponse.MsgVariables = notificationTemplateObj.MsgVariables;


            notificationTemplateResponse.QuizInTemplateList = new List<QuizInTemplate>();

            if (notificationTemplateObj.QuizInTemplateList != null)
            {
                foreach (var item in notificationTemplateObj.QuizInTemplateList)
                {
                    notificationTemplateResponse.QuizInTemplateList.Add(new QuizInTemplate
                    {
                        Id = item.Id,
                        QuizTitle = item.QuizTitle
                    });
                }
            }

            notificationTemplateResponse.TemplateAttachmentList = new List<TemplateAttachment>();

            if (notificationTemplateObj.TemplateAttachmentList != null)
            {
                foreach (var item in notificationTemplateObj.TemplateAttachmentList)
                {
                    notificationTemplateResponse.TemplateAttachmentList.Add(new TemplateAttachment
                    {
                        FileName = item.FileName,
                        FileUrl = item.FileUrl,
                        FileIdentifier = item.FileIdentifier
                    });
                }
            }

            return notificationTemplateResponse;
        }
    }

    public class NotificationTemplateTypeResponse : IResponse
    {
        public List<NotificationTemplateResponse> NotificationTemplateList { get; set; }
        public List<QuizInTemplate> InactiveQuizList { get; set; }

        public IResponse MapEntityToResponse(Base obj)
        {
            NotificationTemplateTypeResponse notificationTemplateTypeResponse = new NotificationTemplateTypeResponse();
            var notificationTemplateTypeObj = (NotificationTemplateType)obj;

            notificationTemplateTypeResponse.NotificationTemplateList = new List<NotificationTemplateResponse>();

            foreach (var item in notificationTemplateTypeObj.NotificationTemplateList)
            {
                var notificationTemplateResponse = new NotificationTemplateResponse();

                notificationTemplateResponse.Id = item.Id;
                notificationTemplateResponse.TemplateTitle = item.TemplateTitle;
                notificationTemplateResponse.OfficeId = item.OfficeId;
                notificationTemplateResponse.NotificationType = (int)item.NotificationType;
                notificationTemplateResponse.Body = item.Body;
                notificationTemplateResponse.Subject = item.Subject;
                notificationTemplateResponse.SMSText = item.SMSText;
                notificationTemplateResponse.EmailLinkVariable = item.EmailLinkVariable;

                notificationTemplateResponse.QuizInTemplateList = new List<QuizInTemplate>();

                foreach (var quiz in item.QuizInTemplateList)
                {
                    var quizObj = new QuizInTemplate();

                    quizObj.Id = quiz.Id;
                    quizObj.QuizTitle = quiz.QuizTitle;
                    quizObj.QuizType = quiz.QuizType;
                    quizObj.PublishedCode = quiz.PublishedCode;
                    quizObj.QuizCoverDetails = new QuizInTemplate.QuizCover();
                    quizObj.QuizCoverDetails.QuizCoverTitle = quiz.QuizCoverDetails.QuizCoverTitle;
                    quizObj.QuizCoverDetails.QuizCoverImage = quiz.QuizCoverDetails.QuizCoverImage;
                    quizObj.QuizCoverDetails.PublicId = quiz.QuizCoverDetails.PublicIdForQuizCover;
                    quizObj.NoOfQusetions = quiz.NoOfQusetions;
                    quizObj.IsFavorited = quiz.IsFavorited;
                    quizObj.IsPublished = quiz.IsPublished;
                    quizObj.Tag = quiz.Tag;
                    quizObj.UsageTypes = quiz.UsageTypes;
                    notificationTemplateResponse.QuizInTemplateList.Add(quizObj);
                }

                notificationTemplateResponse.TemplateAttachmentList = new List<TemplateAttachment>();

                foreach (var attachment in item.TemplateAttachmentList)
                {
                    notificationTemplateResponse.TemplateAttachmentList.Add(new TemplateAttachment
                    {
                        FileName = attachment.FileName,
                        FileUrl = attachment.FileUrl,
                        FileIdentifier = attachment.FileIdentifier
                    });
                }

                notificationTemplateTypeResponse.NotificationTemplateList.Add(notificationTemplateResponse);
            }

            notificationTemplateTypeResponse.InactiveQuizList = new List<QuizInTemplate>();

            foreach (var item in notificationTemplateTypeObj.InactiveQuizList)
            {
                var quizObj = new QuizInTemplate();

                quizObj.Id = item.Id;
                quizObj.QuizTitle = item.QuizTitle;
                quizObj.QuizType = item.QuizType;
                quizObj.PublishedCode = item.PublishedCode;
                quizObj.QuizCoverDetails = new QuizInTemplate.QuizCover();
                quizObj.QuizCoverDetails.QuizCoverTitle = item.QuizCoverDetails.QuizCoverTitle;
                quizObj.QuizCoverDetails.QuizCoverImage = item.QuizCoverDetails.QuizCoverImage;
                quizObj.QuizCoverDetails.PublicId = item.QuizCoverDetails.PublicIdForQuizCover;
                quizObj.NoOfQusetions = item.NoOfQusetions;
                quizObj.IsFavorited = item.IsFavorited;
                quizObj.IsPublished = item.IsPublished;
                quizObj.Tag = item.Tag;
                quizObj.UsageTypes = item.UsageTypes;
                notificationTemplateTypeResponse.InactiveQuizList.Add(quizObj);
            }

            return notificationTemplateTypeResponse;
        }
    }


    public class NotificationTemplateTypeResponseV1 : IResponse
    {
        public List<NotificationTemplateResponse> NotificationTemplateList { get; set; }
        public int CurrentPageIndex { get; set; }
        public int TotalRecords { get; set; }
        public IResponse MapEntityToResponse(Base obj)
        {
            NotificationTemplateTypeResponseV1 notificationTemplateTypeResponse = new NotificationTemplateTypeResponseV1();
            var notificationTemplateTypeObj = (NotificationTemplateTypeV1)obj;

            notificationTemplateTypeResponse.NotificationTemplateList = new List<NotificationTemplateResponse>();

            foreach (var item in notificationTemplateTypeObj.NotificationTemplateList)
            {
                var notificationTemplateResponse = new NotificationTemplateResponse();

                notificationTemplateResponse.Id = item.Id;
                notificationTemplateResponse.TemplateTitle = item.TemplateTitle;
                notificationTemplateResponse.OfficeId = item.OfficeId;
                notificationTemplateResponse.NotificationType = (int)item.NotificationType;
                notificationTemplateResponse.Body = item.Body;
                notificationTemplateResponse.Subject = item.Subject;
                notificationTemplateResponse.SMSText = item.SMSText;
                notificationTemplateResponse.EmailLinkVariable = item.EmailLinkVariable;

                notificationTemplateResponse.TemplateAttachmentList = new List<TemplateAttachment>();

                foreach (var attachment in item.TemplateAttachmentList)
                {
                    notificationTemplateResponse.TemplateAttachmentList.Add(new TemplateAttachment
                    {
                        FileName = attachment.FileName,
                        FileUrl = attachment.FileUrl,
                        FileIdentifier = attachment.FileIdentifier
                    });
                }

                notificationTemplateTypeResponse.NotificationTemplateList.Add(notificationTemplateResponse);
                //notificationTemplateTypeObj.TotalRecords = PageNo;
                //notificationTemplateTypeObj.CurrentPageIndex = totalCount;

                notificationTemplateTypeResponse.CurrentPageIndex = notificationTemplateTypeObj.CurrentPageIndex;
                notificationTemplateTypeResponse.TotalRecords = notificationTemplateTypeObj.TotalRecords;
            }

            return notificationTemplateTypeResponse;
        }
    }
}