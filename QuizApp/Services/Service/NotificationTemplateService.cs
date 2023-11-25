using Newtonsoft.Json;
using QuizApp.Helpers;
using QuizApp.RepositoryExtensions;
using QuizApp.RepositoryPattern;
using QuizApp.Request;
using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Transactions;
using System.Web;
using static QuizApp.Request.TemplateTypeRequest;

namespace QuizApp.Services.Service
{
    public class NotificationTemplateService : INotificationTemplateService
    {
        private ResultEnum status = ResultEnum.Ok;
        private string errormessage = string.Empty;

        public ResultEnum Status { get { return status; } set { status = value; } }
        public string ErrorMessage { get { return errormessage; } set { errormessage = value; } }
        IQuizService _iQuizService = null;
        public NotificationTemplateService(IQuizService iQuizService)
        {
            _iQuizService = iQuizService;
        }

        public NotificationTemplateModel AddQuizInTemplate(NotificationTemplateModel notificationTemplate, int BusinessUserId, int CompanyId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    using (var transaction = Utility.CreateTransactionScope())
                    {
                        Db.NotificationTemplate obj = new Db.NotificationTemplate
                        {
                            OfficeId = notificationTemplate.OfficeId,
                            Title = notificationTemplate.TemplateTitle,
                            NotificationType = (int)notificationTemplate.NotificationType,
                            Subject = string.Empty,
                            Body = string.Empty,
                            SMSText = string.Empty,
                            WhatsApp = null,
                            CompanyId = CompanyId,
                            Status = (int)StatusEnum.Active,
                            CreatedBy = BusinessUserId,
                            CreatedOn = DateTime.UtcNow
                        };

                        UOWObj.NotificationTemplateRepository.Insert(obj);

                        UOWObj.Save();

                        foreach (var item in notificationTemplate.QuizInTemplateList)
                        {
                            var notificationTemplateInQuizObj = UOWObj.NotificationTemplatesInQuizRepository.Get(r => r.QuizId == item.Id && r.NotificationTemplate.NotificationType == obj.NotificationType).FirstOrDefault();

                            if (notificationTemplateInQuizObj != null)
                            {
                                notificationTemplateInQuizObj.NotificationTemplateId = obj.Id;
                                notificationTemplateInQuizObj.CompanyId = obj.CompanyId;

                                UOWObj.NotificationTemplatesInQuizRepository.Update(notificationTemplateInQuizObj);
                            }
                            else
                            {
                                notificationTemplateInQuizObj = new Db.NotificationTemplatesInQuiz
                                {
                                    QuizId = item.Id,
                                    NotificationTemplateId = obj.Id,
                                    CompanyId = CompanyId
                                };

                                UOWObj.NotificationTemplatesInQuizRepository.Insert(notificationTemplateInQuizObj);
                            }
                        }

                        UOWObj.Save();
                        transaction.Complete();

                        notificationTemplate.Id = obj.Id;
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
            return notificationTemplate;
        }

        public void UpdateQuizInTemplate(NotificationTemplateModel notificationTemplate, int BusinessUserId, int CompanyId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    foreach (var item in notificationTemplate.QuizInTemplateList)
                    {
                        var obj = UOWObj.NotificationTemplatesInQuizRepository.Get(r => r.QuizId == item.Id && r.NotificationTemplate.NotificationType == (int)notificationTemplate.NotificationType).FirstOrDefault();

                        if (obj != null)
                        {
                            obj.NotificationTemplateId = notificationTemplate.Id;
                            obj.CompanyId = CompanyId;

                            UOWObj.NotificationTemplatesInQuizRepository.Update(obj);
                        }
                        else
                        {
                            var templateInQuizObj = new Db.NotificationTemplatesInQuiz
                            {
                                QuizId = item.Id,
                                NotificationTemplateId = notificationTemplate.Id,
                                CompanyId = CompanyId
                            };

                            UOWObj.NotificationTemplatesInQuizRepository.Insert(templateInQuizObj);
                        }
                    }
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

        public void SaveTemplateBody(NotificationTemplateModel notificationTemplate, int BusinessUserId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var obj = UOWObj.NotificationTemplateRepository.GetByID(notificationTemplate.Id);

                    if (obj != null)
                    {
                        obj.Title = notificationTemplate.TemplateTitle;
                        obj.Body = notificationTemplate.Body;
                        obj.Subject = notificationTemplate.Subject;
                        obj.SMSText = notificationTemplate.SMSText;
                        obj.WhatsApp = notificationTemplate.WhatsApp != null ? JsonConvert.SerializeObject(notificationTemplate.WhatsApp) : null;
                        obj.EmailLinkVariable = notificationTemplate.EmailLinkVariable;
                        obj.LastUpdatedBy = BusinessUserId;
                        obj.LastUpdatedOn = DateTime.UtcNow;
                        obj.MsgVariables = notificationTemplate.MsgVariables != null && notificationTemplate.MsgVariables.Any() ? JsonConvert.SerializeObject(notificationTemplate.MsgVariables) : null;

                        UOWObj.NotificationTemplateRepository.Update(obj);

                        foreach (var templateAttachmentObj in obj.AttachmentsInNotificationTemplate.ToList())
                        {
                            UOWObj.AttachmentsInNotificationTemplateRepository.Delete(templateAttachmentObj);
                        }

                        foreach (var templateAttachmentObj in notificationTemplate.TemplateAttachmentList)
                        {
                            var templateAttachment = new Db.AttachmentsInNotificationTemplate()
                            {
                                NotificationTemplateId = obj.Id,
                                Description = templateAttachmentObj.FileUrl,
                                Title = templateAttachmentObj.FileName,
                                FileIdentifier = templateAttachmentObj.FileIdentifier,
                                LastUpdatedBy = BusinessUserId,
                                LastUpdatedOn = DateTime.Now
                            };
                            UOWObj.AttachmentsInNotificationTemplateRepository.Insert(templateAttachment);
                        }

                        UOWObj.Save();
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Notification Template not found for the Id " + notificationTemplate.Id;
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

        public NotificationTemplateModel GetTemplateBody(int notificationTemplateId)
        {
            NotificationTemplateModel notificationTemplateObj = null;
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var obj = UOWObj.NotificationTemplateRepository.GetByID(notificationTemplateId);

                    if (obj != null)
                    {
                        notificationTemplateObj = new NotificationTemplateModel();

                        notificationTemplateObj.Id = obj.Id;
                        notificationTemplateObj.TemplateTitle = obj.Title;
                        notificationTemplateObj.Body = obj.Body;
                        notificationTemplateObj.Subject = obj.Subject;
                        notificationTemplateObj.SMSText = obj.SMSText;
                        notificationTemplateObj.WhatsApp = !string.IsNullOrWhiteSpace(obj.WhatsApp) ? JsonConvert.DeserializeObject<WhatsAppDetails>(obj.WhatsApp) : null;
                        notificationTemplateObj.EmailLinkVariable = obj.EmailLinkVariable;
                        notificationTemplateObj.NotificationType = (NotificationTypeEnum)obj.NotificationType;
                        notificationTemplateObj.CompanyId = obj.CompanyId;
                        notificationTemplateObj.MsgVariables = !string.IsNullOrWhiteSpace(obj.MsgVariables) ? JsonConvert.DeserializeObject<List<string>>(obj.MsgVariables) : null;

                        notificationTemplateObj.TemplateAttachmentList = new List<TemplateAttachment>();

                        foreach (var templateAttachmentObj in obj.AttachmentsInNotificationTemplate)
                        {
                            var templateAttachment = new TemplateAttachment();
                            templateAttachment.FileName = templateAttachmentObj.Title;
                            templateAttachment.FileUrl = templateAttachmentObj.Description;
                            templateAttachment.FileIdentifier = templateAttachmentObj.FileIdentifier;
                            notificationTemplateObj.TemplateAttachmentList.Add(templateAttachment);
                        }

                        notificationTemplateObj.QuizInTemplateList = new List<LocalQuiz>();
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Notification Template not found for the Id " + notificationTemplateId;
                    }

                    return notificationTemplateObj;
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        //public NotificationTemplateTypeV1 GetTemplateTypeDetails(int PageNo, int PageSize, int NotificationType, string OfficeId, bool IncludeSharedWithMe, long OffsetValue, int BusinessUserId, CompanyModel CompanyInfo, int? UserInfoId, string SearchTxt, string QuizId, string QuizTypeId, bool? IsFavorite, bool? IsPublished, int? QuizTagId)
        //{
        //    NotificationTemplateTypeV1 notificationTemplateTypeObj = null;
        //    List<NotificationTemplateModel> notificationTemplateLst = null;

        //    IQuizListService _iQuizListService = new QuizListService();
        //    try
        //    {
        //        using (var UOWObj = new AutomationUnitOfWork())
        //        {
        //            var companyId = CompanyInfo.Id;

        //            var userTokens = UOWObj.UserTokensRepository.Get(r => r.CompanyId == companyId).Select(s => s.BusinessUserId).ToList();

        //            var lstNotificationTemplate = UOWObj.NotificationTemplateRepository.Get(r =>
        //                userTokens.Contains(r.CreatedBy)
        //                && r.Status == (int)StatusEnum.Active
        //                && r.NotificationType == NotificationType
        //                && (IncludeSharedWithMe ? string.IsNullOrEmpty(r.OfficeId) : !string.IsNullOrEmpty(r.OfficeId) && r.OfficeId == OfficeId)
        //                && r.CompanyId == companyId);

        //            var allQuizLst = _iQuizListService.GetAutomationList(PageNo, PageSize, BusinessUserId, !string.IsNullOrEmpty(OfficeId) ? (new List<string> { OfficeId }) : new List<string>(), IncludeSharedWithMe, OffsetValue, SearchTxt,1, string.IsNullOrEmpty(QuizTypeId) ? "1,2,3,4" : QuizTypeId, CompanyInfo, false, false, UserInfoId, QuizId, IsFavorite, IsPublished, false, QuizTagId).Quiz;

        //            var activeQuizIdLst = new List<int>();

        //            if (lstNotificationTemplate != null)
        //            {
        //                notificationTemplateTypeObj = new NotificationTemplateTypeV1();

        //                notificationTemplateLst = new List<NotificationTemplateModel>();

        //                NotificationTemplateModel notificationTemplateObj = null;

        //                foreach (var item in lstNotificationTemplate)
        //                {
        //                    notificationTemplateObj = new NotificationTemplateModel();

        //                    notificationTemplateObj.Id = item.Id;
        //                    notificationTemplateObj.TemplateTitle = item.Title;
        //                    notificationTemplateObj.OfficeId = item.OfficeId;
        //                    notificationTemplateObj.Body = item.Body;
        //                    notificationTemplateObj.Subject = item.Subject;
        //                    notificationTemplateObj.SMSText = item.SMSText;
        //                    notificationTemplateObj.EmailLinkVariable = item.EmailLinkVariable;
        //                    notificationTemplateObj.NotificationType = (NotificationTypeEnum)item.NotificationType;

        //                    var notificationTemplatesInQuiz = item.NotificationTemplatesInQuiz;

        //                    var quizListInTemplate = allQuizLst.Where(r => notificationTemplatesInQuiz.Any(n => n.QuizId == r.Id));


        //                    notificationTemplateObj.TemplateAttachmentList = new List<TemplateAttachment>();

        //                    foreach (var templateAttachmentObj in item.AttachmentsInNotificationTemplate)
        //                    {
        //                        var templateAttachment = new TemplateAttachment();
        //                        templateAttachment.FileName = templateAttachmentObj.Title;
        //                        templateAttachment.FileUrl = templateAttachmentObj.Description;
        //                        templateAttachment.FileIdentifier = templateAttachmentObj.FileIdentifier;
        //                        notificationTemplateObj.TemplateAttachmentList.Add(templateAttachment);
        //                    }

        //                    notificationTemplateLst.Add(notificationTemplateObj);
        //                }

        //                notificationTemplateTypeObj.NotificationTemplateList = new List<NotificationTemplateModel>();

        //                notificationTemplateTypeObj.NotificationTemplateList = notificationTemplateLst;

        //            }

        //            return notificationTemplateTypeObj;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Status = ResultEnum.Error;
        //        ErrorMessage = ex.Message;
        //        throw ex;
        //    }
        //}


        public NotificationTemplateTypeV1 GetTemplateTypeDetails(TemplateTypeRequestModel rquestModel, CompanyModel CompanyInfo)
        {
            NotificationTemplateTypeV1 notificationTemplateTypeObj = null;
            List<NotificationTemplateModel> notificationTemplateLst = null;
            int totalCount = 0;
            IQuizListService _iQuizListService = new QuizListService();
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var companyId = CompanyInfo.Id;

                    var userTokens = UOWObj.UserTokensRepository.Get(r => r.CompanyId == companyId).Select(s => s.BusinessUserId).ToList();

                    var lstNotificationTemplate = UOWObj.NotificationTemplateRepository.GetWithPagination(out totalCount, rquestModel.PageNo, rquestModel.PageSize, filter: (r =>
                        userTokens.Contains(r.CreatedBy)
                        && r.Status == (int)StatusEnum.Active
                        && r.NotificationType == rquestModel.NotificationType
                        && (rquestModel.IncludeSharedWithMe ? string.IsNullOrEmpty(r.OfficeId) : !string.IsNullOrEmpty(r.OfficeId) && r.OfficeId == rquestModel.OfficeId)
                        && r.CompanyId == companyId),
                        orderBy: r => r.OrderByDescending(s => s.Id));

                    var activeQuizIdLst = new List<int>();

                    if (lstNotificationTemplate != null)
                    {
                        notificationTemplateTypeObj = new NotificationTemplateTypeV1();

                        notificationTemplateLst = new List<NotificationTemplateModel>();

                        NotificationTemplateModel notificationTemplateObj = null;

                        foreach (var item in lstNotificationTemplate)
                        {
                            notificationTemplateObj = new NotificationTemplateModel();

                            notificationTemplateObj.Id = item.Id;
                            notificationTemplateObj.TemplateTitle = item.Title;
                            notificationTemplateObj.OfficeId = item.OfficeId;
                            notificationTemplateObj.Body = item.Body;
                            notificationTemplateObj.Subject = item.Subject;
                            notificationTemplateObj.SMSText = item.SMSText;
                            notificationTemplateObj.EmailLinkVariable = item.EmailLinkVariable;
                            notificationTemplateObj.NotificationType = (NotificationTypeEnum)item.NotificationType;
                            notificationTemplateObj.CompanyId = item.CompanyId;

                            var notificationTemplatesInQuiz = item.NotificationTemplatesInQuiz;

                            notificationTemplateObj.TemplateAttachmentList = new List<TemplateAttachment>();

                            foreach (var templateAttachmentObj in item.AttachmentsInNotificationTemplate)
                            {
                                var templateAttachment = new TemplateAttachment();
                                templateAttachment.FileName = templateAttachmentObj.Title;
                                templateAttachment.FileUrl = templateAttachmentObj.Description;
                                templateAttachment.FileIdentifier = templateAttachmentObj.FileIdentifier;
                                notificationTemplateObj.TemplateAttachmentList.Add(templateAttachment);
                            }

                            notificationTemplateLst.Add(notificationTemplateObj);
                        }
                        notificationTemplateTypeObj.TotalRecords = totalCount;
                        notificationTemplateTypeObj.CurrentPageIndex = rquestModel.PageNo;


                        notificationTemplateTypeObj.NotificationTemplateList = new List<NotificationTemplateModel>();

                        notificationTemplateTypeObj.NotificationTemplateList = notificationTemplateLst;

                    }

                    return notificationTemplateTypeObj;
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }
        public void DeleteTemplate(int id)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var notificationTemplate = UOWObj.NotificationTemplateRepository.GetByID(id);

                    if (notificationTemplate != null)
                    {
                        var quizInTemplate = notificationTemplate.NotificationTemplatesInQuiz.Where(r => r.Quiz.QuizDetails.FirstOrDefault(t => t.State == (int)QuizStateEnum.DRAFTED).Status == (int)StatusEnum.Active).ToList();

                        if (quizInTemplate.Count > 0)
                        {
                            Status = ResultEnum.Error;
                            ErrorMessage = "Notification Template is not empty.";
                        }
                        else
                        {
                            notificationTemplate.Status = (int)StatusEnum.Deleted;
                            UOWObj.NotificationTemplateRepository.Update(notificationTemplate);
                            UOWObj.Save();
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Notification Template not found for the Id " + id;
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

        public void SetQuizInactive(NotificationTemplateModel notificationTemplate)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    foreach (var item in notificationTemplate.QuizInTemplateList)
                    {
                        var obj = UOWObj.NotificationTemplatesInQuizRepository.Get(r => r.QuizId == item.Id && r.NotificationTemplate.NotificationType == (int)notificationTemplate.NotificationType).FirstOrDefault();

                        if (obj != null)
                        {
                            UOWObj.NotificationTemplatesInQuizRepository.Delete(obj);
                        }
                    }
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

        public NotificationTemplateModel GetDefaultTemplateByType(int notificationTemplateType)
        {
            NotificationTemplateModel notificationTemplateObj = null;
            try
            {
                notificationTemplateObj = new NotificationTemplateModel();

                var templateType = string.Empty;

                switch (notificationTemplateType)
                {
                    case 2:
                        templateType = "Invitation";
                        notificationTemplateObj.NotificationType = NotificationTypeEnum.INVITATION;
                        break;
                    case 3:
                        templateType = "Reminder";
                        notificationTemplateObj.NotificationType = NotificationTypeEnum.REMINDER;
                        break;
                    case 1:
                        templateType = "Result";
                        notificationTemplateObj.NotificationType = NotificationTypeEnum.RESULT;
                        break;
                }

                notificationTemplateObj.Subject = Utility.NotificationDefaultTemplates[templateType + "_Subject"];
                notificationTemplateObj.Body = Utility.NotificationDefaultTemplates[templateType + "_Body"];
                notificationTemplateObj.SMSText = Utility.NotificationDefaultTemplates[templateType + "_SMSText"];

                return notificationTemplateObj;
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public List<NotificationTemplateModel> GetTemplateBodyWithValues(int QuizId, string SourceName, CompanyModel CompanyInfo)
        {
            List<NotificationTemplateModel> lstTemplates = new List<NotificationTemplateModel>();

            NotificationTemplateModel notificationTemplateObj = null;
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var Quiz = UOWObj.QuizRepository.Get(r => r.Id == QuizId).FirstOrDefault();
                    if (Quiz != null)
                    {
                        lstTemplates = new List<NotificationTemplateModel>();

                        foreach (var template in Quiz.NotificationTemplatesInQuiz)
                        {
                            string body = string.Empty;

                            if (template.NotificationTemplate != null && !String.IsNullOrEmpty(template.NotificationTemplate.Body))
                            {
                                body = template.NotificationTemplate.Body.Replace("%qname%", (Quiz.QuizDetails.Any() && Quiz.QuizDetails.LastOrDefault(r => r.State == (int)QuizStateEnum.PUBLISHED) != null && Quiz.QuizDetails.LastOrDefault(r => r.State == (int)QuizStateEnum.PUBLISHED).QuizTitle != null) ? Quiz.QuizDetails.LastOrDefault(r => r.State == (int)QuizStateEnum.PUBLISHED).QuizTitle : string.Empty)
                                                                         .Replace("%sourcename%", SourceName);
                            }

                            string smsBody = string.Empty;

                            if (template.NotificationTemplate != null && !String.IsNullOrEmpty(template.NotificationTemplate.SMSText))
                            {
                                smsBody = template.NotificationTemplate.SMSText.Replace("%qname%", (Quiz.QuizDetails.Any() && Quiz.QuizDetails.LastOrDefault(r => r.State == (int)QuizStateEnum.PUBLISHED) != null && Quiz.QuizDetails.LastOrDefault(r => r.State == (int)QuizStateEnum.PUBLISHED).QuizTitle != null) ? Quiz.QuizDetails.LastOrDefault(r => r.State == (int)QuizStateEnum.PUBLISHED).QuizTitle : string.Empty)
                                                                         .Replace("%sourcename%", SourceName);
                            }

                            notificationTemplateObj = new NotificationTemplateModel();

                            notificationTemplateObj.Id = template.NotificationTemplateId;
                            notificationTemplateObj.TemplateTitle = template.NotificationTemplate.Title;
                            notificationTemplateObj.Body = body;
                            notificationTemplateObj.Subject = String.IsNullOrEmpty(template.NotificationTemplate.Subject) ? string.Empty : template.NotificationTemplate.Subject.Replace("%sourcename%", SourceName);
                            notificationTemplateObj.SMSText = smsBody;
                            notificationTemplateObj.WhatsApp = (template.NotificationTemplate != null && !String.IsNullOrEmpty(template.NotificationTemplate.WhatsApp)) ? JsonConvert.DeserializeObject<WhatsAppDetails>(template.NotificationTemplate.WhatsApp) : null;
                            notificationTemplateObj.NotificationType = template.NotificationTemplate.NotificationType == 1 ? NotificationTypeEnum.RESULT :
                                                                      template.NotificationTemplate.NotificationType == 2 ? NotificationTypeEnum.INVITATION : NotificationTypeEnum.REMINDER;

                            notificationTemplateObj.MsgVariables = !string.IsNullOrWhiteSpace(template.NotificationTemplate.MsgVariables) ? JsonConvert.DeserializeObject<List<string>>(template.NotificationTemplate.MsgVariables) : null;

                            notificationTemplateObj.TemplateAttachmentList = new List<TemplateAttachment>();

                            foreach (var templateAttachmentObj in template.NotificationTemplate.AttachmentsInNotificationTemplate)
                            {
                                var templateAttachment = new TemplateAttachment();
                                templateAttachment.FileName = templateAttachmentObj.Title;
                                templateAttachment.FileUrl = templateAttachmentObj.Description;
                                templateAttachment.FileIdentifier = templateAttachmentObj.FileIdentifier;
                                notificationTemplateObj.TemplateAttachmentList.Add(templateAttachment);
                            }

                            lstTemplates.Add(notificationTemplateObj);
                        }
                    }

                    return lstTemplates;
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public AutomationDetails GetSearchAndSuggestionByNotificationTemplate(int QuizStatus, int NotificationType, string OfficeId, bool IncludeSharedWithMe, long OffsetValue, CompanyModel CompanyInfo, int? NotificationTemplateId, string SearchTxt, bool? IsPublished, int? UsageType)
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

                    var OfficeIdList = !string.IsNullOrEmpty(OfficeId) ? (new List<string> { OfficeId }) : new List<string>();

                    var quizTypeIds = "1,2,3,4".Split(',').Select(Int32.Parse).ToList();

                    var quizLst = UOWObj.QuizRepository
                                  .Get(r => (quizTypeIds.Contains(r.QuizType))
                                         && (string.IsNullOrEmpty(SearchTxt) ? false : ((string.IsNullOrEmpty(r.QuizDetails.FirstOrDefault(t => t.State == (int)QuizStateEnum.DRAFTED).QuizTitle) ? false : r.QuizDetails.FirstOrDefault(t => t.State == (int)QuizStateEnum.DRAFTED).QuizTitle.ToUpper().IndexOf(SearchTxt) > -1)
                                                                                         || (string.IsNullOrEmpty(r.QuizDetails.FirstOrDefault(t => t.State == (int)QuizStateEnum.DRAFTED).QuizCoverTitle) ? false : r.QuizDetails.FirstOrDefault(t => t.State == (int)QuizStateEnum.DRAFTED).QuizCoverTitle.ToUpper().IndexOf(SearchTxt) > -1)))
                                         && (r.Company.Id == CompanyInfo.Id)
                                         && (r.QuizDetails.FirstOrDefault(q => q.State == (int)QuizStateEnum.DRAFTED && q.Status == (int)StatusEnum.Active) != null)
                                         && (IsPublished.HasValue ? (IsPublished.Value ? (r.State == (int)QuizStateEnum.PUBLISHED) : (r.State != (int)QuizStateEnum.PUBLISHED)) : true)
                                          && (UsageType.HasValue ? r.UsageTypeInQuiz.Any(u => u.UsageType == UsageType.Value) : true)
                                         && (!string.IsNullOrEmpty(r.AccessibleOfficeId) ? (OfficeIdList.Contains(r.AccessibleOfficeId)) : IncludeSharedWithMe),
                                         includeProperties: "NotificationTemplatesInQuiz, QuizDetails, QuizTagDetails");



                    if (quizLst != null && quizLst.Any())
                    {
                        foreach (var Quiz in quizLst)
                        {
                            if ((QuizStatus == (int)StatusEnum.Active && NotificationTemplateId.HasValue && Quiz.NotificationTemplatesInQuiz != null && Quiz.NotificationTemplatesInQuiz.Any() && Quiz.NotificationTemplatesInQuiz.FirstOrDefault().NotificationTemplateId == NotificationTemplateId.Value)
                                || (QuizStatus == (int)StatusEnum.Inactive && ((Quiz.NotificationTemplatesInQuiz == null || !Quiz.NotificationTemplatesInQuiz.Any()) || Quiz.NotificationTemplatesInQuiz.FirstOrDefault().NotificationTemplateId == 0)))
                            {
                                var quizDetailsObj = Quiz.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active);
                                if (quizDetailsObj != null)
                                {
                                    if (string.IsNullOrEmpty(quizDetailsObj.QuizTitle) ? false : quizDetailsObj.QuizTitle.ToUpper().IndexOf(SearchTxt) > -1)
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
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }

            return AutomationDetails;
        }

        public NotificationTemplateModel GetTemplateDetailsWithValues(int NotificationTemplateId, CompanyModel CompanyInfo, int? QuizId)
        {
            NotificationTemplateModel notificationTemplateObj = null;
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var template = UOWObj.NotificationTemplateRepository.GetByID(NotificationTemplateId);

                    if (template != null)
                    {
                        string body = string.Empty;
                        string smsBody = string.Empty;

                        if (!String.IsNullOrEmpty(template.Body))
                        {
                            if (template.Body.Contains("%signature%"))
                            {
                                //var signatureDetails = OWCHelper.GetEmailSignature(CompanyInfo.ClientCode, "group");
                                var signatureDetails = CommonStaticData.GetCachedEmailSignature(CompanyInfo.ClientCode, "group");
                                if (signatureDetails != null)
                                    body = template.Body.Replace("%signature%", signatureDetails.signatureText);
                            }

                            if (QuizId != null && (template.Body.Contains("%qname%") || template.SMSText.Contains("%qname%")))
                            {
                                var quizDetails = UOWObj.QuizDetailsRepository.GetSelectedColoumn(r => new { r.QuizTitle },
                                                  filter: r => r.ParentQuizId == QuizId && r.State == (int)QuizStateEnum.PUBLISHED).LastOrDefault();
                                if (quizDetails != null)
                                {
                                    body = body.Replace("%qname%", quizDetails.QuizTitle ?? string.Empty);
                                    smsBody = template.SMSText.Replace("%qname%", quizDetails.QuizTitle ?? string.Empty);
                                }
                            }
                        }

                        notificationTemplateObj = new NotificationTemplateModel();

                        notificationTemplateObj.Id = template.Id;
                        notificationTemplateObj.TemplateTitle = template.Title;
                        notificationTemplateObj.Body = body;
                        notificationTemplateObj.Subject = template.Subject ?? string.Empty;
                        notificationTemplateObj.SMSText = smsBody;
                        notificationTemplateObj.NotificationType = template.NotificationType == 1 ? NotificationTypeEnum.RESULT :
                                                                  template.NotificationType == 2 ? NotificationTypeEnum.INVITATION : NotificationTypeEnum.REMINDER;
                        notificationTemplateObj.CompanyId = template.CompanyId;

                        notificationTemplateObj.TemplateAttachmentList = new List<TemplateAttachment>();

                        foreach (var templateAttachmentObj in template.AttachmentsInNotificationTemplate)
                        {
                            var templateAttachment = new TemplateAttachment();
                            templateAttachment.FileName = templateAttachmentObj.Title;
                            templateAttachment.FileUrl = templateAttachmentObj.Description;
                            templateAttachment.FileIdentifier = templateAttachmentObj.FileIdentifier;
                            notificationTemplateObj.TemplateAttachmentList.Add(templateAttachment);
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Notification Template not found for the Id " + NotificationTemplateId;
                    }

                    return notificationTemplateObj;
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }



        public InactiveNotificationTemplateTypeResponse InActiveQuizList(NotificationTemplateQuizListRequestModel requestModel, CompanyModel companyInfo, int? userInfoId)
        {
            var notificationTemplateTypeObj = new InactiveNotificationTemplateTypeResponse();
            notificationTemplateTypeObj = new InactiveNotificationTemplateTypeResponse();

            notificationTemplateTypeObj.InactiveQuizList = new List<NotificationTemplateQuizDetails>();

            IQuizListService _iQuizListService = new QuizListService();
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var companyId = companyInfo.Id;

                    var userTokens = UOWObj.UserTokensRepository.Get(r => r.CompanyId == companyId).Select(s => s.BusinessUserId).ToList();

                    var lstNotificationTemplate = UOWObj.NotificationTemplateRepository.Get(r =>
                         userTokens.Contains(r.CreatedBy)
                         && r.Status == (int)StatusEnum.Active
                         && r.NotificationType == requestModel.NotificationType
                         && (requestModel.IncludeSharedWithMe ? string.IsNullOrEmpty(r.OfficeId) : !string.IsNullOrEmpty(r.OfficeId) && r.OfficeId == requestModel.OfficeId)
                         && r.CompanyId == companyId).Select(v => v.Id).ToList();
                    if (lstNotificationTemplate != null && lstNotificationTemplate.Any())
                    {
                        notificationTemplateTypeObj = _iQuizListService.GetInActiveNotificationTemplateList(!string.IsNullOrEmpty(requestModel.OfficeId) ? (new List<string> { requestModel.OfficeId }) : new List<string>(), requestModel.IncludeSharedWithMe, requestModel.PageNo, requestModel.PageSize, requestModel.SearchTxt, requestModel.OrderBy, string.IsNullOrEmpty(requestModel.QuizTypeId) ? "1,2,3,4" : requestModel.QuizTypeId, companyInfo, false, false, userInfoId, requestModel.QuizId, requestModel.IsFavorite, requestModel.QuizTagId, lstNotificationTemplate, requestModel.UsageType);

                        if (notificationTemplateTypeObj != null && notificationTemplateTypeObj.InactiveQuizList != null && notificationTemplateTypeObj.InactiveQuizList.Any())
                        {
                            return notificationTemplateTypeObj;
                        }
                    }
                    return notificationTemplateTypeObj;
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }




        public ActiveNotificationTemplateTypeResponse QuizTemplateList(NotificationTemplateInActiveRequestModel requestModel, CompanyModel companyInfo, int? userInfoId)
        {
            var quizInTemplateList = new ActiveNotificationTemplateTypeResponse();
            quizInTemplateList.QuizInTemplateList = new List<NotificationTemplateQuizDetails>();
            IQuizListService _iQuizListService = new QuizListService();
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var companyId = companyInfo.Id;

                    var userTokens = UOWObj.UserTokensRepository.Get(r => r.CompanyId == companyId).Select(s => s.BusinessUserId).ToList();

                    var lstNotificationTemplate = UOWObj.NotificationTemplateRepository.Get(r =>
                        userTokens.Contains(r.CreatedBy)
                        && r.Status == (int)StatusEnum.Active
                        && r.NotificationType == requestModel.NotificationType
                        && (requestModel.IncludeSharedWithMe ? string.IsNullOrEmpty(r.OfficeId) : !string.IsNullOrEmpty(r.OfficeId) && r.OfficeId == requestModel.OfficeId)
                        && r.CompanyId == companyId
                        && r.Id == requestModel.TemplateId).Select(v => v.Id).ToList();

                    if (lstNotificationTemplate != null && lstNotificationTemplate.Any())
                    {
                        quizInTemplateList = _iQuizListService.GetQuizTemplateList(lstNotificationTemplate, !string.IsNullOrEmpty(requestModel.OfficeId) ? (new List<string> { requestModel.OfficeId }) : new List<string>(), requestModel.IncludeSharedWithMe, requestModel.PageNo, requestModel.PageSize, requestModel.SearchTxt, requestModel.OrderBy, string.IsNullOrEmpty(requestModel.QuizTypeId) ? "1,2,3,4" : requestModel.QuizTypeId, companyInfo, false, false, userInfoId, requestModel.QuizId, requestModel.IsFavorite, false, requestModel.QuizTagId);
                        if (quizInTemplateList != null && quizInTemplateList.QuizInTemplateList != null && quizInTemplateList.QuizInTemplateList.Any())
                        {
                            return quizInTemplateList;
                        }
                    }

                    return quizInTemplateList;
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public void UnLinkAutomation(InactiveNotificationTemplate notificationTemplate)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                        var obj = UOWObj.NotificationTemplatesInQuizRepository.Get(r => r.QuizId == notificationTemplate.QuizId && r.NotificationTemplate.NotificationType == notificationTemplate.NotificationType && r.NotificationTemplateId == notificationTemplate.TemplateId).FirstOrDefault();
                        if (obj != null)
                        {
                            UOWObj.NotificationTemplatesInQuizRepository.Delete(obj);
                        }
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

        public void LinkAutomation(InactiveNotificationTemplate notificationTemplate, CompanyModel CompanyInfo)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                  
                        var obj = UOWObj.NotificationTemplatesInQuizRepository.Get(r => r.QuizId == notificationTemplate.QuizId && r.NotificationTemplate.NotificationType == notificationTemplate.NotificationType && r.NotificationTemplateId == notificationTemplate.TemplateId).FirstOrDefault();

                        if (obj != null)
                        {
                            obj.NotificationTemplateId = notificationTemplate.TemplateId;
                            obj.CompanyId = obj.CompanyId;

                            UOWObj.NotificationTemplatesInQuizRepository.Update(obj);
                        }
                        else
                        {
                            var templateInQuizObj = new Db.NotificationTemplatesInQuiz
                            {
                                QuizId = notificationTemplate.QuizId,
                                NotificationTemplateId = notificationTemplate.TemplateId,
                                CompanyId = CompanyInfo.Id
                            };

                            UOWObj.NotificationTemplatesInQuizRepository.Insert(templateInQuizObj);
                        }
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

    }
}
