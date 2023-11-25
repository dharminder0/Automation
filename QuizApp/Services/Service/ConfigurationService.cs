using Core.Common.Caching;
using QuizApp.Helpers;
using QuizApp.RepositoryExtensions;
using QuizApp.RepositoryPattern;
using QuizApp.Services.Model;
using QuizApp.Services.Validator;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Transactions;
using System.Web;

namespace QuizApp.Services.Service
{
    public class ConfigurationService
    {
        private ResultEnum status = ResultEnum.Ok;
        private string errormessage = string.Empty;

        public ResultEnum Status { get { return status; } set { status = value; } }
        public string ErrorMessage { get { return errormessage; } set { errormessage = value; } }

        private void AddConfiguration(Db.ConfigurationDetails configurationDetails, AddOrUpdateConfiguration configurationObj)
        {
            bool isNewConfiguration = false;

            if (configurationDetails == null)
            {
                configurationDetails = new Db.ConfigurationDetails()
                {
                    ConfigurationId = configurationObj.ConfigurationId,
                };
                isNewConfiguration = true;
            }

            configurationDetails.QuizId = configurationObj.QuizId;
            configurationDetails.IsUpdatedSend = configurationObj.IsUpdatedSend;
            configurationDetails.Subject = configurationObj.Subject;
            configurationDetails.Body = configurationObj.Body;
            configurationDetails.SMSText = configurationObj.SMSText;
            configurationDetails.SendEmail = configurationObj.SendEmail;
            configurationDetails.SendSms = configurationObj.SendSms;
            configurationDetails.SendWhatsApp = configurationObj.SendWhatsApp;
            configurationDetails.SendFallbackSms = configurationObj.SendFallbackSms;
            configurationDetails.SendMailNotRequired = configurationObj.SendMailNotRequired;
            configurationDetails.SourceId = configurationObj.SourceId;
            configurationDetails.SourceType = configurationObj.SourceType;
            configurationDetails.SourceName = configurationObj.SourceTitle;
            configurationDetails.PrivacyLink = configurationObj.PrivacyLink;
            configurationDetails.ConfigurationType = configurationObj.ConfigurationType;
            configurationDetails.CompanyCode = configurationObj.CompanyCode;
            configurationDetails.LeadFormTitle = configurationObj.LeadFormTitle;
            configurationDetails.UpdatedOn = DateTime.UtcNow;
            configurationDetails.HsmTemplateId = (configurationObj.WhatsApp != null && configurationObj.WhatsApp.HsmTemplateId > 0) ? configurationObj.WhatsApp.HsmTemplateId : default(int?);
            configurationDetails.FollowUpMessage = configurationObj.WhatsApp != null ? configurationObj.WhatsApp.FollowUpMessage : string.Empty;

            using (var UOWObj = new AutomationUnitOfWork())
            {
                if (isNewConfiguration)
                {
                    UOWObj.ConfigurationDetailsRepository.Update(configurationDetails);
                }
                else
                {
                    UOWObj.ConfigurationDetailsRepository.Insert(configurationDetails);
                }

                UOWObj.Save();
            }
        }

        private void AddConfigurationWhatsApp(AddOrUpdateConfiguration configurationObj)
        {
            Db.ConfigurationDetails configurationDetails = null;
            #region insert in WhatsApp
            using (var UOWObj = new AutomationUnitOfWork())
            {
                var configurationDetailsList = UOWObj.ConfigurationDetailsRepository.Get(r => r.ConfigurationId == configurationObj.ConfigurationId);

                if (configurationDetailsList != null && configurationDetailsList.Any())
                {
                    configurationDetails = configurationDetailsList.FirstOrDefault();
                }
                if (configurationDetails.TemplateParameterInConfigurationDetails.Any())
                {
                    foreach (var item in configurationDetails.TemplateParameterInConfigurationDetails.ToList())
                    {
                        UOWObj.TemplateParameterInConfigurationDetailsRepository.Delete(item);
                    }
                    UOWObj.Save();
                }

                if (configurationObj.WhatsApp != null)
                {
                    if (configurationObj.WhatsApp.TemplateParameters != null)
                    {
                        foreach (var item in configurationObj.WhatsApp.TemplateParameters)
                        {
                            var templateParameterInConfigurationDetails = new Db.TemplateParameterInConfigurationDetails()
                            {
                                ConfigurationDetailsId = configurationDetails.Id,
                                ParaName = item.Paraname,
                                Position = item.Position,
                                Value = item.Value
                            };
                            UOWObj.TemplateParameterInConfigurationDetailsRepository.Insert(templateParameterInConfigurationDetails);
                        }
                        UOWObj.Save();
                    }
                }
            }

            #endregion
        }

        public void AddConfigurationQuizDetails(AddOrUpdateConfiguration configurationObj)
        {
            Db.ConfigurationDetails CurrentconfigurationDetails = null;
            using (var UOWObj = new AutomationUnitOfWork())
            {
                var currentDate = DateTime.UtcNow;
                //var configurationDetailsList = UOWObj.ConfigurationDetailsRepository.Get(r => r.ConfigurationId == configurationObj.ConfigurationId);
                var configurationDetailsList = UOWObj.ConfigurationDetailsRepository.GetConfigurationDetailsByConfigurationId(configurationObj.ConfigurationId);

                if (configurationDetailsList != null && configurationDetailsList.Any())
                {
                    CurrentconfigurationDetails = configurationDetailsList.FirstOrDefault();

                }
            }

            Db.Quiz currentQuizDetails = null;
            using (var UOWObj = new AutomationUnitOfWork())
            {
                currentQuizDetails = UOWObj.QuizRepository.GetByID(configurationObj.QuizId);
            }

            if (currentQuizDetails == null)
            {
                Status = ResultEnum.Error;
                ErrorMessage = "Quiz does not exists for QuizId " + configurationObj.QuizId;
                return;
            }


        }






        public void AddOrUpdateConfigurationDetails(AddOrUpdateConfiguration configurationObj)
        {
            Db.ConfigurationDetails CurrentconfigurationDetails = null;
            using (var UOWObj = new AutomationUnitOfWork())
            {
                var currentDate = DateTime.UtcNow;
                //var configurationDetailsList = UOWObj.ConfigurationDetailsRepository.Get(r => r.ConfigurationId == configurationObj.ConfigurationId);
                var configurationDetailsList = UOWObj.ConfigurationDetailsRepository.GetConfigurationDetailsByConfigurationId(configurationObj.ConfigurationId);

                if (configurationDetailsList != null && configurationDetailsList.Any())
                {
                    CurrentconfigurationDetails = configurationDetailsList.FirstOrDefault();

                }
            }

            Db.Quiz currentQuizDetails = null;
            using (var UOWObj = new AutomationUnitOfWork())
            {
                currentQuizDetails = UOWObj.QuizRepository.GetByID(configurationObj.QuizId);
            }

            if (currentQuizDetails == null)
            {
                Status = ResultEnum.Error;
                ErrorMessage = "Quiz does not exists for QuizId " + configurationObj.QuizId;
                return;
            }




            using (var UOWObj = new AutomationUnitOfWork())
            {
                using (var transaction = Utility.CreateTransactionScope())
                {
                    try
                    {
                        var currentDate = DateTime.UtcNow;
                        var configurationDetailsList = UOWObj.ConfigurationDetailsRepository.Get(r => r.ConfigurationId == configurationObj.ConfigurationId);

                        if (configurationDetailsList != null && configurationDetailsList.Any())
                        {
                            var configurationDetails = configurationDetailsList.FirstOrDefault();

                            var quizObj = UOWObj.QuizRepository.GetByID(configurationObj.QuizId);
                            if (quizObj != null && quizObj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active) != null)
                            {
                                var quizDetails = quizObj.QuizDetails.LastOrDefault(r => r.State == (int)QuizStateEnum.PUBLISHED);

                                var quizComponentLogs = quizDetails.QuizComponentLogs;

                                #region update in ConfigurationDetails

                                configurationDetails.QuizId = configurationObj.QuizId;
                                configurationDetails.IsUpdatedSend = configurationObj.IsUpdatedSend;
                                configurationDetails.Subject = configurationObj.Subject;
                                configurationDetails.Body = configurationObj.Body;
                                configurationDetails.SMSText = configurationObj.SMSText;
                                configurationDetails.SendEmail = configurationObj.SendEmail;
                                configurationDetails.SendSms = configurationObj.SendSms;
                                configurationDetails.SendWhatsApp = configurationObj.SendWhatsApp;
                                configurationDetails.SendFallbackSms = configurationObj.SendFallbackSms;
                                configurationDetails.SendMailNotRequired = configurationObj.SendMailNotRequired;
                                configurationDetails.SourceId = configurationObj.SourceId;
                                configurationDetails.SourceType = configurationObj.SourceType;
                                configurationDetails.SourceName = configurationObj.SourceTitle;
                                configurationDetails.PrivacyLink = configurationObj.PrivacyLink;
                                configurationDetails.ConfigurationType = configurationObj.ConfigurationType;
                                configurationDetails.CompanyCode = configurationObj.CompanyCode;
                                configurationDetails.LeadFormTitle = configurationObj.LeadFormTitle;
                                configurationDetails.UpdatedOn = currentDate;
                                configurationDetails.HsmTemplateId = (configurationObj.WhatsApp != null && configurationObj.WhatsApp.HsmTemplateId > 0) ? configurationObj.WhatsApp.HsmTemplateId : default(int?);
                                configurationDetails.FollowUpMessage = configurationObj.WhatsApp != null ? configurationObj.WhatsApp.FollowUpMessage : string.Empty;

                                UOWObj.ConfigurationDetailsRepository.Update(configurationDetails);
                                UOWObj.Save();

                                #endregion

                                #region insert in WhatsApp

                                if (configurationDetails.TemplateParameterInConfigurationDetails.Any())
                                {
                                    foreach (var item in configurationDetails.TemplateParameterInConfigurationDetails.ToList())
                                    {
                                        UOWObj.TemplateParameterInConfigurationDetailsRepository.Delete(item);
                                    }
                                    UOWObj.Save();
                                }

                                if (configurationObj.WhatsApp != null)
                                {
                                    if (configurationObj.WhatsApp.TemplateParameters != null)
                                    {
                                        foreach (var item in configurationObj.WhatsApp.TemplateParameters)
                                        {
                                            var templateParameterInConfigurationDetails = new Db.TemplateParameterInConfigurationDetails()
                                            {
                                                ConfigurationDetailsId = configurationDetails.Id,
                                                ParaName = item.Paraname,
                                                Position = item.Position,
                                                Value = item.Value
                                            };
                                            UOWObj.TemplateParameterInConfigurationDetailsRepository.Insert(templateParameterInConfigurationDetails);
                                        }
                                        UOWObj.Save();
                                    }
                                }

                                #endregion

                                #region update in ResultIdsInConfigurationDetails

                                if (configurationDetails.ResultIdsInConfigurationDetails.Any(r => r.QuizResults.QuizId == quizDetails.Id))
                                {
                                    foreach (var resultIdObj in configurationDetails.ResultIdsInConfigurationDetails.Where(r => r.QuizResults.QuizId == quizDetails.Id).ToList())
                                    {
                                        UOWObj.ResultIdsInConfigurationDetailsRepository.Delete(resultIdObj);
                                        UOWObj.Save();
                                    }
                                }

                                if (configurationObj.ResultIds != null && configurationObj.ResultIds.Any())
                                {
                                    foreach (var obj in configurationObj.ResultIds)
                                    {
                                        var resultIdObj = new Db.ResultIdsInConfigurationDetails();

                                        resultIdObj.ResultId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj && r.ObjectTypeId == (int)BranchingLogicEnum.RESULT).PublishedObjectId;
                                        resultIdObj.ConfigurationDetailsId = configurationDetails.Id;
                                        resultIdObj.FormId = 1;
                                        resultIdObj.FlowOrder = 3;

                                        UOWObj.ResultIdsInConfigurationDetailsRepository.Insert(resultIdObj);

                                        UOWObj.Save();
                                    }
                                }
                                else if (configurationObj.LeadFormDetailofResults != null && configurationObj.LeadFormDetailofResults.Any())
                                {
                                    foreach (var obj in configurationObj.LeadFormDetailofResults.ToList())
                                    {
                                        var resultIdObj = new Db.ResultIdsInConfigurationDetails();

                                        resultIdObj.ResultId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ResultId && r.ObjectTypeId == (int)BranchingLogicEnum.RESULT).PublishedObjectId;
                                        resultIdObj.ConfigurationDetailsId = configurationDetails.Id;
                                        resultIdObj.FormId = obj.FormId;
                                        resultIdObj.FlowOrder = obj.FlowOrder;

                                        UOWObj.ResultIdsInConfigurationDetailsRepository.Insert(resultIdObj);

                                        UOWObj.Save();
                                    }
                                }

                                #endregion

                                #region update in VariablesDetails

                                if (configurationDetails.VariablesDetails.Any())
                                {
                                    foreach (var variablesDetailObj in configurationDetails.VariablesDetails.ToList())
                                    {
                                        UOWObj.VariablesDetailsRepository.Delete(variablesDetailObj);
                                        UOWObj.Save();
                                    }
                                }

                                foreach (var obj in configurationObj.DynamicVariables)
                                {
                                    var VariablesObj = UOWObj.VariablesRepository.Get(r => r.VariableInQuiz.Any(s => s.QuizId == quizDetails.Id) && r.Name.ToLower() == obj.Key.ToLower()).FirstOrDefault();

                                    if (VariablesObj != null)
                                    {
                                        var variablesDetailsObj = new Db.VariablesDetails();

                                        variablesDetailsObj.VariableInQuizId = VariablesObj.VariableInQuiz.FirstOrDefault(r => r.QuizId == quizDetails.Id).Id;
                                        variablesDetailsObj.VariableValue = obj.Value==null ? null :  obj.Value.Trim();
                                        variablesDetailsObj.ConfigurationDetailsId = configurationDetails.Id;

                                        UOWObj.VariablesDetailsRepository.Insert(variablesDetailsObj);

                                        UOWObj.Save();
                                    }
                                }

                                #endregion

                                #region update in LeadDataInAction

                                if (configurationDetails.LeadDataInAction.Any())
                                {
                                    foreach (var leadDataInActionObj in configurationDetails.LeadDataInAction.Where(r => r.ActionsInQuiz.QuizId == quizDetails.Id).ToList())
                                    {
                                        UOWObj.LeadDataInActionRepository.Delete(leadDataInActionObj);
                                        UOWObj.Save();
                                    }
                                }

                                foreach (var obj in configurationObj.LeadDataInActionList)
                                {
                                    var actionId = 0;

                                    if (obj.ParentId > 0)
                                    {
                                        actionId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ParentId && r.ObjectTypeId == (int)BranchingLogicEnum.ACTION).PublishedObjectId;
                                    }
                                    else
                                    {
                                        int parentActionId = quizComponentLogs.LastOrDefault(r => r.PublishedObjectId == obj.ActionId && r.ObjectTypeId == (int)BranchingLogicEnum.ACTION).DraftedObjectId;
                                        actionId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == parentActionId && r.ObjectTypeId == (int)BranchingLogicEnum.ACTION).PublishedObjectId;
                                    }

                                    var actionsInQuiz = quizDetails.ActionsInQuiz.Where(t => t.Id == actionId).FirstOrDefault();

                                    UpdateleadDataInActionValidator emailListValidator = new UpdateleadDataInActionValidator();
                                    var validationEmailResult = emailListValidator.Validate(obj.ReportEmails.Split(',').Select(t => t.Trim()).ToList());

                                    if (actionsInQuiz != null && (((actionsInQuiz.ActionType == (int)ActionTypeEnum.LinkWithLeadDashboardAppointment || actionsInQuiz.ActionType == (int)ActionTypeEnum.Appointment) && obj.AppointmentTypeId > 0) || ((actionsInQuiz.ActionType == (int)ActionTypeEnum.LinkWithLeadDashboardAppointment || actionsInQuiz.ActionType == (int)ActionTypeEnum.ReportEmail) && !string.IsNullOrEmpty(obj.ReportEmails) && validationEmailResult != null && validationEmailResult.IsValid)))
                                    {
                                        var leadDataInActionObj = new Db.LeadDataInAction();

                                        leadDataInActionObj.ActionId = actionId;
                                        leadDataInActionObj.AppointmentTypeId = obj.AppointmentTypeId > 0 ? obj.AppointmentTypeId : default(int?);
                                        leadDataInActionObj.ReportEmails = !string.IsNullOrEmpty(obj.ReportEmails) ? obj.ReportEmails : null;
                                        if (obj.IsUpdatedSend && !obj.SendMailNotRequired && !string.IsNullOrEmpty(obj.Subject) && !string.IsNullOrEmpty(obj.Body) && !string.IsNullOrEmpty(obj.SMSText))
                                        {
                                            leadDataInActionObj.IsUpdatedSend = obj.IsUpdatedSend;
                                            leadDataInActionObj.Subject = obj.Subject;
                                            leadDataInActionObj.Body = obj.Body;
                                            leadDataInActionObj.SMSText = obj.SMSText;
                                        }
                                        leadDataInActionObj.ConfigurationDetailsId = configurationDetails.Id;


                                        if (obj.CalendarIds != null)
                                        {
                                            foreach (var calendarId in obj.CalendarIds.ToList())
                                            {
                                                var leadCalendarDataInActionObj = new Db.LeadCalendarDataInAction();
                                                leadCalendarDataInActionObj.LeadDataInActionId = leadDataInActionObj.Id;
                                                leadCalendarDataInActionObj.CalendarId = calendarId;
                                                UOWObj.LeadCalendarDataInActionRepository.Insert(leadCalendarDataInActionObj);
                                            }
                                        }

                                        UOWObj.LeadDataInActionRepository.Insert(leadDataInActionObj);

                                        UOWObj.Save();
                                    }
                                }

                                #endregion

                                #region update in MediaVariables

                                if (configurationDetails.MediaVariablesDetails.Any(r => r.QuizId == quizDetails.Id))
                                {
                                    foreach (var obj in configurationDetails.MediaVariablesDetails.Where(r => r.QuizId == quizDetails.Id).ToList())
                                    {
                                        UOWObj.MediaVariablesDetailsRepository.Delete(obj);
                                        UOWObj.Save();
                                    }
                                }

                                if (configurationObj.MediaVariableDetails.Questions != null)
                                {
                                    foreach (var obj in configurationObj.MediaVariableDetails.Questions)
                                    {
                                        var MediaObj = new Db.MediaVariablesDetails();

                                        MediaObj.ObjectId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ParentId && r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION).PublishedObjectId;
                                        MediaObj.ObjectTypeId = (int)BranchingLogicEnum.QUESTION;
                                        MediaObj.ObjectValue = obj.MediaUrlValue;
                                        MediaObj.ObjectPublicId = obj.PublicId;
                                        MediaObj.QuizId = quizDetails.Id;
                                        MediaObj.ConfigurationDetailsId = configurationDetails.Id;
                                        MediaObj.Type = (int)ImageTypeEnum.Title;
                                        MediaObj.MediaOwner = obj.MediaOwner;
                                        MediaObj.ProfileMedia = obj.ProfileMedia;

                                        UOWObj.MediaVariablesDetailsRepository.Insert(MediaObj);

                                        UOWObj.Save();

                                        var MediaforDescriptionObj = new Db.MediaVariablesDetails();

                                        MediaforDescriptionObj.ObjectId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ParentId && r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION).PublishedObjectId;
                                        MediaforDescriptionObj.ObjectTypeId = (int)BranchingLogicEnum.QUESTION;
                                        MediaforDescriptionObj.ObjectValue = obj.MediaUrlforDescriptionValue;
                                        MediaforDescriptionObj.ObjectPublicId = obj.PublicIdforDescription;
                                        MediaforDescriptionObj.QuizId = quizDetails.Id;
                                        MediaforDescriptionObj.ConfigurationDetailsId = configurationDetails.Id;
                                        MediaforDescriptionObj.Type = (int)ImageTypeEnum.Description;
                                        MediaforDescriptionObj.MediaOwner = obj.MediaOwnerforDescription;
                                        MediaforDescriptionObj.ProfileMedia = obj.ProfileMediaforDescription;

                                        UOWObj.MediaVariablesDetailsRepository.Insert(MediaforDescriptionObj);

                                        UOWObj.Save();
                                    }
                                }

                                if (configurationObj.MediaVariableDetails.Answers != null)
                                {
                                    foreach (var obj in configurationObj.MediaVariableDetails.Answers)
                                    {
                                        var MediaObj = new Db.MediaVariablesDetails();

                                        MediaObj.ObjectId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ParentId && r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER).PublishedObjectId;
                                        MediaObj.ObjectTypeId = (int)BranchingLogicEnum.ANSWER;
                                        MediaObj.ObjectValue = obj.MediaUrlValue;
                                        MediaObj.ObjectPublicId = obj.PublicId;
                                        MediaObj.QuizId = quizDetails.Id;
                                        MediaObj.ConfigurationDetailsId = configurationDetails.Id;
                                        MediaObj.Type = (int)ImageTypeEnum.Title;
                                        MediaObj.MediaOwner = obj.MediaOwner;
                                        MediaObj.ProfileMedia = obj.ProfileMedia;

                                        UOWObj.MediaVariablesDetailsRepository.Insert(MediaObj);

                                        UOWObj.Save();
                                    }
                                }

                                if (configurationObj.MediaVariableDetails.Results != null)
                                {
                                    foreach (var obj in configurationObj.MediaVariableDetails.Results)
                                    {
                                        var MediaObj = new Db.MediaVariablesDetails();

                                        MediaObj.ObjectId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ParentId && r.ObjectTypeId == (int)BranchingLogicEnum.RESULT).PublishedObjectId;
                                        MediaObj.ObjectTypeId = (int)BranchingLogicEnum.RESULT;
                                        MediaObj.ObjectValue = obj.MediaUrlValue;
                                        MediaObj.ObjectPublicId = obj.PublicId;
                                        MediaObj.QuizId = quizDetails.Id;
                                        MediaObj.ConfigurationDetailsId = configurationDetails.Id;
                                        MediaObj.Type = (int)ImageTypeEnum.Title;
                                        MediaObj.MediaOwner = obj.MediaOwner;
                                        MediaObj.ProfileMedia = obj.ProfileMedia;

                                        UOWObj.MediaVariablesDetailsRepository.Insert(MediaObj);

                                        UOWObj.Save();
                                    }
                                }

                                if (configurationObj.MediaVariableDetails.CoverDetails != null)
                                {
                                    foreach (var obj in configurationObj.MediaVariableDetails.CoverDetails)
                                    {
                                        var MediaObj = new Db.MediaVariablesDetails();

                                        MediaObj.ObjectId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ParentId && r.ObjectTypeId == (int)BranchingLogicEnum.COVERDETAILS).PublishedObjectId;
                                        MediaObj.ObjectTypeId = (int)BranchingLogicEnum.COVERDETAILS;
                                        MediaObj.ObjectValue = obj.MediaUrlValue;
                                        MediaObj.ObjectPublicId = obj.PublicId;
                                        MediaObj.QuizId = quizDetails.Id;
                                        MediaObj.ConfigurationDetailsId = configurationDetails.Id;
                                        MediaObj.Type = (int)ImageTypeEnum.Title;
                                        MediaObj.MediaOwner = obj.MediaOwner;
                                        MediaObj.ProfileMedia = obj.ProfileMedia;

                                        UOWObj.MediaVariablesDetailsRepository.Insert(MediaObj);

                                        UOWObj.Save();
                                    }
                                }

                                if (configurationObj.MediaVariableDetails.Badges != null)
                                {
                                    foreach (var obj in configurationObj.MediaVariableDetails.Badges)
                                    {
                                        var MediaObj = new Db.MediaVariablesDetails();

                                        MediaObj.ObjectId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ParentId && r.ObjectTypeId == (int)BranchingLogicEnum.BADGE).PublishedObjectId;
                                        MediaObj.ObjectTypeId = (int)BranchingLogicEnum.BADGE;
                                        MediaObj.ObjectValue = obj.MediaUrlValue;
                                        MediaObj.ObjectPublicId = obj.PublicId;
                                        MediaObj.QuizId = quizDetails.Id;
                                        MediaObj.ConfigurationDetailsId = configurationDetails.Id;
                                        MediaObj.Type = (int)ImageTypeEnum.Title;
                                        MediaObj.MediaOwner = obj.MediaOwner;
                                        MediaObj.ProfileMedia = obj.ProfileMedia;

                                        UOWObj.MediaVariablesDetailsRepository.Insert(MediaObj);

                                        UOWObj.Save();
                                    }
                                }

                                if (configurationObj.MediaVariableDetails.Content != null)
                                {
                                    foreach (var obj in configurationObj.MediaVariableDetails.Content)
                                    {
                                        var MediaObj = new Db.MediaVariablesDetails();

                                        MediaObj.ObjectId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ParentId && (quizObj.QuesAndContentInSameTable ? r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION : r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT)).PublishedObjectId;
                                        MediaObj.ObjectTypeId = (int)BranchingLogicEnum.CONTENT;
                                        MediaObj.ObjectValue = obj.MediaUrlValue;
                                        MediaObj.ObjectPublicId = obj.PublicId;
                                        MediaObj.QuizId = quizDetails.Id;
                                        MediaObj.ConfigurationDetailsId = configurationDetails.Id;
                                        MediaObj.Type = (int)ImageTypeEnum.Title;
                                        MediaObj.MediaOwner = obj.MediaOwner;
                                        MediaObj.ProfileMedia = obj.ProfileMedia;

                                        UOWObj.MediaVariablesDetailsRepository.Insert(MediaObj);

                                        UOWObj.Save();

                                        var MediaforDescriptionObj = new Db.MediaVariablesDetails();

                                        MediaforDescriptionObj.ObjectId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ParentId && (quizObj.QuesAndContentInSameTable ? r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION : r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT)).PublishedObjectId;
                                        MediaforDescriptionObj.ObjectTypeId = (int)BranchingLogicEnum.CONTENT;
                                        MediaforDescriptionObj.ObjectValue = obj.MediaUrlforDescriptionValue;
                                        MediaforDescriptionObj.ObjectPublicId = obj.PublicIdforDescription;
                                        MediaforDescriptionObj.QuizId = quizDetails.Id;
                                        MediaforDescriptionObj.ConfigurationDetailsId = configurationDetails.Id;
                                        MediaforDescriptionObj.Type = (int)ImageTypeEnum.Description;
                                        MediaforDescriptionObj.MediaOwner = obj.MediaOwnerforDescription;
                                        MediaforDescriptionObj.ProfileMedia = obj.ProfileMediaforDescription;

                                        UOWObj.MediaVariablesDetailsRepository.Insert(MediaforDescriptionObj);

                                        UOWObj.Save();
                                    }
                                }

                                #endregion

                                #region update in AttachmentsInConfiguration

                                if (configurationDetails.AttachmentsInConfiguration.Any())
                                {
                                    foreach (var attachmentsObj in configurationDetails.AttachmentsInConfiguration.ToList())
                                    {
                                        UOWObj.AttachmentsInConfigurationRepository.Delete(attachmentsObj);
                                        UOWObj.Save();
                                    }
                                }

                                foreach (var obj in configurationObj.EmailAttachments)
                                {
                                    var attachmentsInConfigurationObj = new Db.AttachmentsInConfiguration();

                                    attachmentsInConfigurationObj.FileName = obj.FileName;
                                    attachmentsInConfigurationObj.FileIdentifier = obj.FileIdentifier;
                                    attachmentsInConfigurationObj.FileLink = obj.FileLink;
                                    attachmentsInConfigurationObj.ConfigurationDetailsId = configurationDetails.Id;

                                    UOWObj.AttachmentsInConfigurationRepository.Insert(attachmentsInConfigurationObj);

                                    UOWObj.Save();
                                }

                                #endregion

                                    AppLocalCache.Remove("QuizReportList_QuizId_" + quizObj.Id);
                            }
                            else
                            {
                                Status = ResultEnum.Error;
                                ErrorMessage = "Quiz does not exists for QuizId " + configurationDetails.QuizId;
                            }
                        }
                        else
                        {
                            var quizObj = UOWObj.QuizRepository.GetByID(configurationObj.QuizId);
                            if (quizObj != null && quizObj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active) != null)
                            {
                                var quizDetails = quizObj.QuizDetails.LastOrDefault(r => r.State == (int)QuizStateEnum.PUBLISHED);

                                var quizComponentLogs = quizDetails.QuizComponentLogs;

                                #region ConfigurationDetails

                                var configurationDetailsObj = new Db.ConfigurationDetails()
                                {
                                    ConfigurationId = configurationObj.ConfigurationId,
                                    QuizId = configurationObj.QuizId,
                                    IsUpdatedSend = configurationObj.IsUpdatedSend,
                                    Subject = configurationObj.Subject,
                                    Body = configurationObj.Body,
                                    SMSText = configurationObj.SMSText,
                                    SendEmail = configurationObj.SendEmail,
                                    SendSms = configurationObj.SendSms,
                                    SendWhatsApp = configurationObj.SendWhatsApp,
                                    SendFallbackSms = configurationObj.SendFallbackSms,
                                    SendMailNotRequired = configurationObj.SendMailNotRequired,
                                    SourceId = configurationObj.SourceId,
                                    SourceType = configurationObj.SourceType,
                                    SourceName = configurationObj.SourceTitle,
                                    PrivacyLink = configurationObj.PrivacyLink,
                                    ConfigurationType = configurationObj.ConfigurationType,
                                    CompanyCode = configurationObj.CompanyCode,
                                    LeadFormTitle = configurationObj.LeadFormTitle,
                                    CreatedOn = currentDate,
                                    Status = (int)StatusEnum.Active,
                                    HsmTemplateId = (configurationObj.WhatsApp != null && configurationObj.WhatsApp.HsmTemplateId > 0) ? configurationObj.WhatsApp.HsmTemplateId : default(int?),
                                    FollowUpMessage = configurationObj.WhatsApp != null ? configurationObj.WhatsApp.FollowUpMessage : string.Empty
                                };

                                UOWObj.ConfigurationDetailsRepository.Insert(configurationDetailsObj);
                                UOWObj.Save();

                                #endregion

                                #region insert in WhatsApp

                                if (configurationObj.WhatsApp != null && configurationObj.WhatsApp.TemplateParameters != null)
                                {
                                    foreach (var item in configurationObj.WhatsApp.TemplateParameters)
                                    {
                                        var templateParameterInConfigurationDetails = new Db.TemplateParameterInConfigurationDetails()
                                        {
                                            ConfigurationDetailsId = configurationDetailsObj.Id,
                                            ParaName = item.Paraname,
                                            Position = item.Position,
                                            Value = item.Value
                                        };

                                        UOWObj.TemplateParameterInConfigurationDetailsRepository.Insert(templateParameterInConfigurationDetails);
                                    }
                                    UOWObj.Save();
                                }

                                #endregion

                                #region insert in ResultIdsInConfigurationDetails

                                if (configurationObj.ResultIds != null && configurationObj.ResultIds.Any())
                                {
                                    foreach (var obj in configurationObj.ResultIds)
                                    {
                                        var resultIdObj = new Db.ResultIdsInConfigurationDetails();

                                        resultIdObj.ResultId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj && r.ObjectTypeId == (int)BranchingLogicEnum.RESULT).PublishedObjectId;
                                        resultIdObj.ConfigurationDetailsId = configurationDetailsObj.Id;
                                        resultIdObj.FormId = 1;
                                        resultIdObj.FlowOrder = 3;

                                        UOWObj.ResultIdsInConfigurationDetailsRepository.Insert(resultIdObj);

                                        UOWObj.Save();
                                    }
                                }
                                else if (configurationObj.LeadFormDetailofResults != null && configurationObj.LeadFormDetailofResults.Any())
                                {
                                    foreach (var obj in configurationObj.LeadFormDetailofResults.ToList())
                                    {
                                        var resultIdObj = new Db.ResultIdsInConfigurationDetails();

                                        resultIdObj.ResultId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ResultId && r.ObjectTypeId == (int)BranchingLogicEnum.RESULT).PublishedObjectId;
                                        resultIdObj.ConfigurationDetailsId = configurationDetailsObj.Id;
                                        resultIdObj.FormId = obj.FormId;
                                        resultIdObj.FlowOrder = obj.FlowOrder;

                                        UOWObj.ResultIdsInConfigurationDetailsRepository.Insert(resultIdObj);

                                        UOWObj.Save();
                                    }
                                }

                                #endregion

                                #region insert in VariablesDetails

                                foreach (var obj in configurationObj.DynamicVariables)
                                {
                                    var VariablesObj = UOWObj.VariablesRepository.Get(r => r.VariableInQuiz.Any(s => s.QuizId == quizDetails.Id) && r.Name.ToLower() == obj.Key.ToLower()).FirstOrDefault();

                                    if (VariablesObj != null)
                                    {
                                        var variablesDetailsObj = new Db.VariablesDetails();

                                        variablesDetailsObj.VariableInQuizId = VariablesObj.VariableInQuiz.FirstOrDefault(r => r.QuizId == quizDetails.Id).Id;
                                        variablesDetailsObj.VariableValue = obj.Value == null ? null : obj.Value.Trim();
                                        variablesDetailsObj.ConfigurationDetailsId = configurationDetailsObj.Id;

                                        UOWObj.VariablesDetailsRepository.Insert(variablesDetailsObj);

                                        UOWObj.Save();
                                    }
                                }

                                #endregion

                                #region insert in LeadDataInAction

                                foreach (var obj in configurationObj.LeadDataInActionList)
                                {
                                    var actionId = 0;

                                    if (obj.ParentId > 0)
                                    {
                                        actionId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ParentId && r.ObjectTypeId == (int)BranchingLogicEnum.ACTION).PublishedObjectId;
                                    }
                                    else
                                    {
                                        int parentActionId = quizComponentLogs.LastOrDefault(r => r.PublishedObjectId == obj.ActionId && r.ObjectTypeId == (int)BranchingLogicEnum.ACTION).DraftedObjectId;
                                        actionId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == parentActionId && r.ObjectTypeId == (int)BranchingLogicEnum.ACTION).PublishedObjectId;
                                    }

                                    var actionsInQuiz = quizDetails.ActionsInQuiz.Where(t => t.Id == actionId).FirstOrDefault();

                                    UpdateleadDataInActionValidator emailListValidator = new UpdateleadDataInActionValidator();
                                    var validationEmailResult = emailListValidator.Validate(obj.ReportEmails.Split(',').Select(t => t.Trim()).ToList());

                                    if (actionsInQuiz != null && (((actionsInQuiz.ActionType == (int)ActionTypeEnum.LinkWithLeadDashboardAppointment || actionsInQuiz.ActionType == (int)ActionTypeEnum.Appointment) && obj.AppointmentTypeId > 0) || ((actionsInQuiz.ActionType == (int)ActionTypeEnum.LinkWithLeadDashboardAppointment || actionsInQuiz.ActionType == (int)ActionTypeEnum.ReportEmail) && !string.IsNullOrEmpty(obj.ReportEmails) && validationEmailResult != null && validationEmailResult.IsValid)))
                                    {
                                        var leadDataInActionObj = new Db.LeadDataInAction();

                                        leadDataInActionObj.ActionId = actionId;
                                        leadDataInActionObj.AppointmentTypeId = obj.AppointmentTypeId > 0 ? obj.AppointmentTypeId : default(int?);
                                        leadDataInActionObj.ReportEmails = !string.IsNullOrEmpty(obj.ReportEmails) ? obj.ReportEmails : null;
                                        if (obj.IsUpdatedSend && !obj.SendMailNotRequired && !string.IsNullOrEmpty(obj.Subject) && !string.IsNullOrEmpty(obj.Body) && !string.IsNullOrEmpty(obj.SMSText))
                                        {
                                            leadDataInActionObj.IsUpdatedSend = obj.IsUpdatedSend;
                                            leadDataInActionObj.Subject = obj.Subject;
                                            leadDataInActionObj.Body = obj.Body;
                                            leadDataInActionObj.SMSText = obj.SMSText;
                                        }
                                        leadDataInActionObj.ConfigurationDetailsId = configurationDetailsObj.Id;


                                        if (obj.CalendarIds != null)
                                        {
                                            foreach (var calendarId in obj.CalendarIds.ToList())
                                            {
                                                var leadCalendarDataInActionObj = new Db.LeadCalendarDataInAction();
                                                leadCalendarDataInActionObj.LeadDataInActionId = leadDataInActionObj.Id;
                                                leadCalendarDataInActionObj.CalendarId = calendarId;
                                                UOWObj.LeadCalendarDataInActionRepository.Insert(leadCalendarDataInActionObj);
                                            }
                                        }

                                        UOWObj.LeadDataInActionRepository.Insert(leadDataInActionObj);

                                        UOWObj.Save();
                                    }
                                }

                                #endregion

                                #region insert in MediaVariables

                                if (configurationObj.MediaVariableDetails.Questions != null)
                                {
                                    foreach (var obj in configurationObj.MediaVariableDetails.Questions)
                                    {
                                        var MediaObj = new Db.MediaVariablesDetails();

                                        MediaObj.ObjectId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ParentId && r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION).PublishedObjectId;
                                        MediaObj.ObjectTypeId = (int)BranchingLogicEnum.QUESTION;
                                        MediaObj.ObjectValue = obj.MediaUrlValue;
                                        MediaObj.ObjectPublicId = obj.PublicId;
                                        MediaObj.QuizId = quizDetails.Id;
                                        MediaObj.ConfigurationDetailsId = configurationDetailsObj.Id;
                                        MediaObj.Type = (int)ImageTypeEnum.Title;
                                        MediaObj.MediaOwner = obj.MediaOwner;
                                        MediaObj.ProfileMedia = obj.ProfileMedia;

                                        UOWObj.MediaVariablesDetailsRepository.Insert(MediaObj);

                                        UOWObj.Save();

                                        var MediaforDescriptionObj = new Db.MediaVariablesDetails();

                                        MediaforDescriptionObj.ObjectId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ParentId && r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION).PublishedObjectId;
                                        MediaforDescriptionObj.ObjectTypeId = (int)BranchingLogicEnum.QUESTION;
                                        MediaforDescriptionObj.ObjectValue = obj.MediaUrlforDescriptionValue;
                                        MediaforDescriptionObj.ObjectPublicId = obj.PublicIdforDescription;
                                        MediaforDescriptionObj.QuizId = quizDetails.Id;
                                        MediaforDescriptionObj.ConfigurationDetailsId = configurationDetailsObj.Id;
                                        MediaforDescriptionObj.Type = (int)ImageTypeEnum.Description;
                                        MediaforDescriptionObj.MediaOwner = obj.MediaOwnerforDescription;
                                        MediaforDescriptionObj.ProfileMedia = obj.ProfileMediaforDescription;

                                        UOWObj.MediaVariablesDetailsRepository.Insert(MediaforDescriptionObj);

                                        UOWObj.Save();
                                    }
                                }

                                if (configurationObj.MediaVariableDetails.Answers != null)
                                {
                                    foreach (var obj in configurationObj.MediaVariableDetails.Answers)
                                    {
                                        var MediaObj = new Db.MediaVariablesDetails();

                                        MediaObj.ObjectId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ParentId && r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER).PublishedObjectId;
                                        MediaObj.ObjectTypeId = (int)BranchingLogicEnum.ANSWER;
                                        MediaObj.ObjectValue = obj.MediaUrlValue;
                                        MediaObj.ObjectPublicId = obj.PublicId;
                                        MediaObj.QuizId = quizDetails.Id;
                                        MediaObj.ConfigurationDetailsId = configurationDetailsObj.Id;
                                        MediaObj.Type = (int)ImageTypeEnum.Title;
                                        MediaObj.MediaOwner = obj.MediaOwner;
                                        MediaObj.ProfileMedia = obj.ProfileMedia;

                                        UOWObj.MediaVariablesDetailsRepository.Insert(MediaObj);

                                        UOWObj.Save();
                                    }
                                }

                                if (configurationObj.MediaVariableDetails.Results != null)
                                {
                                    foreach (var obj in configurationObj.MediaVariableDetails.Results)
                                    {
                                        var MediaObj = new Db.MediaVariablesDetails();

                                        MediaObj.ObjectId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ParentId && r.ObjectTypeId == (int)BranchingLogicEnum.RESULT).PublishedObjectId;
                                        MediaObj.ObjectTypeId = (int)BranchingLogicEnum.RESULT;
                                        MediaObj.ObjectPublicId = obj.PublicId;
                                        MediaObj.ObjectValue = obj.MediaUrlValue;
                                        MediaObj.QuizId = quizDetails.Id;
                                        MediaObj.ConfigurationDetailsId = configurationDetailsObj.Id;
                                        MediaObj.Type = (int)ImageTypeEnum.Title;
                                        MediaObj.MediaOwner = obj.MediaOwner;
                                        MediaObj.ProfileMedia = obj.ProfileMedia;

                                        UOWObj.MediaVariablesDetailsRepository.Insert(MediaObj);

                                        UOWObj.Save();
                                    }
                                }

                                if (configurationObj.MediaVariableDetails.CoverDetails != null)
                                {
                                    foreach (var obj in configurationObj.MediaVariableDetails.CoverDetails)
                                    {
                                        var MediaObj = new Db.MediaVariablesDetails();

                                        MediaObj.ObjectId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ParentId && r.ObjectTypeId == (int)BranchingLogicEnum.COVERDETAILS).PublishedObjectId;
                                        MediaObj.ObjectTypeId = (int)BranchingLogicEnum.COVERDETAILS;
                                        MediaObj.ObjectValue = obj.MediaUrlValue;
                                        MediaObj.ObjectPublicId = obj.PublicId;
                                        MediaObj.QuizId = quizDetails.Id;
                                        MediaObj.ConfigurationDetailsId = configurationDetailsObj.Id;
                                        MediaObj.Type = (int)ImageTypeEnum.Title;
                                        MediaObj.MediaOwner = obj.MediaOwner;
                                        MediaObj.ProfileMedia = obj.ProfileMedia;

                                        UOWObj.MediaVariablesDetailsRepository.Insert(MediaObj);

                                        UOWObj.Save();
                                    }
                                }

                                if (configurationObj.MediaVariableDetails.Badges != null)
                                {
                                    foreach (var obj in configurationObj.MediaVariableDetails.Badges)
                                    {
                                        var MediaObj = new Db.MediaVariablesDetails();

                                        MediaObj.ObjectId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ParentId && r.ObjectTypeId == (int)BranchingLogicEnum.BADGE).PublishedObjectId;
                                        MediaObj.ObjectTypeId = (int)BranchingLogicEnum.BADGE;
                                        MediaObj.ObjectValue = obj.MediaUrlValue;
                                        MediaObj.ObjectPublicId = obj.PublicId;
                                        MediaObj.QuizId = quizDetails.Id;
                                        MediaObj.ConfigurationDetailsId = configurationDetailsObj.Id;
                                        MediaObj.Type = (int)ImageTypeEnum.Title;
                                        MediaObj.MediaOwner = obj.MediaOwner;
                                        MediaObj.ProfileMedia = obj.ProfileMedia;

                                        UOWObj.MediaVariablesDetailsRepository.Insert(MediaObj);

                                        UOWObj.Save();
                                    }
                                }

                                if (configurationObj.MediaVariableDetails.Content != null)
                                {
                                    foreach (var obj in configurationObj.MediaVariableDetails.Content)
                                    {
                                        var MediaObj = new Db.MediaVariablesDetails();

                                        MediaObj.ObjectId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ParentId && (quizObj.QuesAndContentInSameTable ? r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION : r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT)).PublishedObjectId;
                                        MediaObj.ObjectTypeId = (int)BranchingLogicEnum.CONTENT;
                                        MediaObj.ObjectValue = obj.MediaUrlValue;
                                        MediaObj.ObjectPublicId = obj.PublicId;
                                        MediaObj.QuizId = quizDetails.Id;
                                        MediaObj.ConfigurationDetailsId = configurationDetailsObj.Id;
                                        MediaObj.Type = (int)ImageTypeEnum.Title;
                                        MediaObj.MediaOwner = obj.MediaOwner;
                                        MediaObj.ProfileMedia = obj.ProfileMedia;

                                        UOWObj.MediaVariablesDetailsRepository.Insert(MediaObj);

                                        UOWObj.Save();

                                        var MediaforDescriptionObj = new Db.MediaVariablesDetails();

                                        MediaforDescriptionObj.ObjectId = quizComponentLogs.LastOrDefault(r => r.DraftedObjectId == obj.ParentId && (quizObj.QuesAndContentInSameTable ? r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION : r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT)).PublishedObjectId;
                                        MediaforDescriptionObj.ObjectTypeId = (int)BranchingLogicEnum.CONTENT;
                                        MediaforDescriptionObj.ObjectValue = obj.MediaUrlforDescriptionValue;
                                        MediaforDescriptionObj.ObjectPublicId = obj.PublicIdforDescription;
                                        MediaforDescriptionObj.QuizId = quizDetails.Id;
                                        MediaforDescriptionObj.ConfigurationDetailsId = configurationDetailsObj.Id;
                                        MediaforDescriptionObj.Type = (int)ImageTypeEnum.Description;
                                        MediaforDescriptionObj.MediaOwner = obj.MediaOwnerforDescription;
                                        MediaforDescriptionObj.ProfileMedia = obj.ProfileMediaforDescription;

                                        UOWObj.MediaVariablesDetailsRepository.Insert(MediaforDescriptionObj);

                                        UOWObj.Save();
                                    }
                                }

                                #endregion

                                #region insert in AttachmentsInConfiguration

                                foreach (var obj in configurationObj.EmailAttachments)
                                {
                                    var attachmentsInConfigurationObj = new Db.AttachmentsInConfiguration();

                                    attachmentsInConfigurationObj.FileName = obj.FileName;
                                    attachmentsInConfigurationObj.FileIdentifier = obj.FileIdentifier;
                                    attachmentsInConfigurationObj.FileLink = obj.FileLink;
                                    attachmentsInConfigurationObj.ConfigurationDetailsId = configurationDetailsObj.Id;

                                    UOWObj.AttachmentsInConfigurationRepository.Insert(attachmentsInConfigurationObj);

                                    UOWObj.Save();
                                }

                                #endregion

                            }
                            else
                            {
                                Status = ResultEnum.Error;
                                ErrorMessage = "Quiz does not exists for QuizId " + configurationObj.QuizId;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = ex.Message;
                        throw ex;
                    }
                    transaction.Complete();
                }
            }
        }

    }
}
