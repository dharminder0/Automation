using Core.Common.Extensions;
using Core.Common.Caching;
using Newtonsoft.Json;
using QuizApp.Db;
using QuizApp.Helpers;
using QuizApp.RepositoryExtensions;
using QuizApp.RepositoryPattern;
using QuizApp.Services.Model;
using QuizApp.Services.Validator;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Web;
using QuizApp.Services.Model;

namespace QuizApp.Services.Service
{
    public class AutomationService : IAutomationService
    {
        private ResultEnum status = ResultEnum.Ok;
        private string errormessage = string.Empty;
        private string leadInfoUpdateJson = "{'ContactId': '{ContactId}','ClientCode': '{ClientCode}','campaignName': '{campaignName}','appointmentStatus': '{appointmentStatus}','appointmentDate': '{appointmentDate}','appointmentTypeId': {appointmentTypeId},'appointmentTypeTitle': '{appointmentTypeTitle}','calendarId': {calendarId},'calendarTitle': '{calendarTitle}','appointmentBookedDate': '{appointmentBookedDate}','UserToken': '{UserToken}','SourceId': '{SourceId}'}";
        private string badgesInfoUpdateJson = "[ {'UserId': {UserId},'CourseId': '{CourseId}','CourseBadgeName': '{CourseBadgeName}','CourseBadgeImageUrl': '{CourseBadgeImageUrl}','CourseTitle': '{CourseTitle}'}]";
        private bool enableAttemptQuizLogging = System.Configuration.ConfigurationManager.AppSettings["EnableAttemptQuizLogging"] != null ? Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["EnableAttemptQuizLogging"]) : false;

        public ResultEnum Status { get { return status; } set { status = value; } }
        public string ErrorMessage { get { return errormessage; } set { errormessage = value; } }

        public QuizDetailsModel GetQuizDetails(int QuizId)
        {
            QuizDetailsModel quizDetails = null;
            bool EverPublished = false;
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizObj = UOWObj.QuizRepository.GetByID(QuizId);


                    if (quizObj != null)
                    {
                        var publishedQuizDetailObj = UOWObj.QuizDetailsRepository.Get(v => v.ParentQuizId == QuizId && v.State == (int)QuizStateEnum.PUBLISHED).FirstOrDefault();
                        if (publishedQuizDetailObj != null) { EverPublished = true; }

                        //var quizDetailsObj = UOWObj.QuizDetailsRepository.GetQuizDetailsbyParentQuizIdRepositoryExtension(quizObj.Id).FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED);
                        var quizDetailsObj = quizObj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED);

                        if (quizDetailsObj != null)
                        {
                            quizDetails = BrandingStylingObject(QuizId, quizObj, quizDetailsObj);

                            if ((quizDetailsObj.Quiz.QuizType == (int)QuizTypeEnum.Personality || quizDetailsObj.Quiz.QuizType == (int)QuizTypeEnum.PersonalityTemplate) && quizDetailsObj.PersonalityResultSetting.Any())
                                quizDetails.MultipleResultEnabled = quizDetailsObj.PersonalityResultSetting.FirstOrDefault().Status;

                            foreach (var action in quizDetailsObj.ActionsInQuiz.Where(r => r.Status == (int)StatusEnum.Active))
                            {
                                quizDetails.QuizAction.Add(new QuizAction()
                                {
                                    AppointmentId = action.AppointmentId,
                                    Id = action.Id,
                                    ReportEmails = action.ReportEmails,
                                    AutomationId = action.AutomationId,
                                    Title = action.Title,
                                    ActionType = action.ActionType,
                                    CalendarIds = (action.ActionType == (int)ActionTypeEnum.Appointment && action.LinkedCalendarInAction.Any()) ? action.LinkedCalendarInAction.Select(t => t.CalendarId).ToList() : new List<int>() { }
                                });
                            }

                            foreach (var content in quizDetailsObj.ContentsInQuiz.Where(r => r.Status == (int)StatusEnum.Active))
                            {
                                quizDetails.QuizContent.Add(new QuizContent()
                                {
                                    Id = content.Id,
                                    ContentTitle = content.ContentTitle,
                                    ContentTitleImage = content.ContentTitleImage,
                                    EnableMediaFileForTitle = content.EnableMediaFileForTitle,
                                    PublicIdForContentTitle = content.PublicIdForContentTitle,
                                    ShowContentTitleImage = content.ShowContentTitleImage,
                                    ContentDescription = content.ContentDescription,
                                    ContentDescriptionImage = content.ContentDescriptionImage,
                                    EnableMediaFileForDescription = content.EnableMediaFileForDescription,
                                    PublicIdForContentDescription = content.PublicIdForContentDescription,
                                    ShowContentDescriptionImage = content.ShowContentDescriptionImage,
                                    AliasTextForNextButton = content.AliasTextForNextButton,
                                    AutoPlay = content.AutoPlay,
                                    SecondsToApply = content.SecondsToApply,
                                    VideoFrameEnabled = content.VideoFrameEnabled,
                                    DisplayOrder = content.DisplayOrder
                                });
                            }

                            foreach (var question in quizDetailsObj.QuestionsInQuiz.Where(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active).OrderBy(r => r.DisplayOrder))
                            {
                                QuizQuestion questionInQuiz = QuesAnsMapping(question);

                                quizDetails.QuestionsInQuiz.Add(questionInQuiz);
                            }

                            quizDetails.ResultList = new List<QuizResult>();

                            foreach (var result in quizDetailsObj.QuizResults.Where(r => r.State == (int)QuizStateEnum.DRAFTED && r.IsPersonalityCorrelatedResult == false && r.Status == (int)StatusEnum.Active))
                            {
                                var quizResult = new QuizResult();

                                quizResult.ResultId = result.Id;
                                quizResult.Title = result.Title;
                                quizResult.InternalTitle = result.InternalTitle ?? string.Empty;
                                quizResult.MinScore = result.MinScore;
                                quizResult.MaxScore = result.MaxScore;
                                quizResult.AutoPlay = result.AutoPlay;

                                quizDetails.ResultList.Add(quizResult);
                            }

                            quizDetails.BadgeList = new List<QuizBadge>();

                            foreach (var badge in quizDetailsObj.BadgesInQuiz.Where(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active))
                            {
                                var quizBadge = new QuizBadge();

                                quizBadge.Id = badge.Id;
                                quizBadge.Title = badge.Title;
                                quizBadge.Image = badge.Image;
                                quizBadge.EnableMediaFile = badge.EnableMediaFile;
                                quizBadge.PublicIdForBadge = badge.PublicId;

                                quizDetails.BadgeList.Add(quizBadge);
                            }

                            quizDetails.QuestionsAndContent = new List<QuizQuestionAndContent>();
                            foreach (var question in quizDetailsObj.QuestionsInQuiz.Where(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active).OrderBy(r => r.DisplayOrder))
                            {
                                QuizQuestionAndContent questionInQuiz = QuesContentMapping(question);

                                quizDetails.QuestionsAndContent.Add(questionInQuiz);
                            }

                            foreach (var content in quizDetailsObj.ContentsInQuiz.Where(r => r.Status == (int)StatusEnum.Active))
                            {
                                quizDetails.QuestionsAndContent.Add(new QuizQuestionAndContent()
                                {
                                    Id = content.Id,
                                    ContentTitle = content.ContentTitle,
                                    ContentTitleImage = content.ContentTitleImage,
                                    EnableMediaFileForTitle = content.EnableMediaFileForTitle,
                                    PublicIdForContentTitle = content.PublicIdForContentTitle,
                                    ShowContentTitleImage = content.ShowContentTitleImage,
                                    ContentDescription = content.ContentDescription,
                                    ContentDescriptionImage = content.ContentDescriptionImage,
                                    EnableMediaFileForDescription = content.EnableMediaFileForDescription,
                                    PublicIdForContentDescription = content.PublicIdForContentDescription,
                                    ShowContentDescriptionImage = content.ShowContentDescriptionImage,
                                    AliasTextForNextButton = content.AliasTextForNextButton,
                                    AutoPlay = content.AutoPlay,
                                    SecondsToApply = content.SecondsToApply,
                                    VideoFrameEnabled = content.VideoFrameEnabled,
                                    AutoPlayForDescription = content.AutoPlayForDescription,
                                    SecondsToApplyForDescription = content.SecondsToApplyForDescription,
                                    DescVideoFrameEnabled = content.DescVideoFrameEnabled,
                                    DisplayOrder = content.DisplayOrder,
                                    Type = (int)BranchingLogicEnum.CONTENT
                                });
                            }

                            quizDetails.UsageType = new List<int>();
                            var usageTypeInQuiz = quizObj.UsageTypeInQuiz.Where(r => r.QuizId == quizObj.Id).Select(v => v.UsageType);
                            if (usageTypeInQuiz != null && usageTypeInQuiz.Any())
                            {
                                foreach (var item in usageTypeInQuiz.ToList())
                                {
                                    quizDetails.UsageType.Add(item);
                                }
                            }
                            quizDetails.IsWhatsAppChatBotOldVersion = quizObj.IsWhatsAppChatBotOldVersion.HasValue ? quizObj.IsWhatsAppChatBotOldVersion.Value : false;

                        }
                        quizDetails.EverPublished = EverPublished;
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz not found for the QuizId " + QuizId;
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }

            return quizDetails;

        }

        public QuizResult AddQuizResult(int QuizId, int BusinessUserId, int CompanyId, int? quizType = 0)
        {
            QuizResult quizResult = null;
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {

                    using (var transaction = Utility.CreateTransactionScope())
                    {
                        var quizObj = UOWObj.QuizRepository.GetByID(QuizId);

                        if (quizObj != null)
                        {
                            var quizDetails = quizObj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED);

                            if (quizDetails != null)
                            {
                                var currentDate = DateTime.UtcNow;

                                #region Adding obj

                                Db.QuizResults resultObj = AddingResultObj(BusinessUserId, quizType, quizDetails, currentDate);

                                UOWObj.QuizResultsRepository.Insert(resultObj);
                                UOWObj.Save();

                                if (quizType == (int)QuizTypeEnum.Personality || quizType == (int)QuizTypeEnum.PersonalityTemplate)
                                {
                                    QuizTypeEnum quizTypeEnum = QuizTypeEnum.Personality;
                                    if (quizType == (int)QuizTypeEnum.Personality)
                                    {
                                        quizTypeEnum = QuizTypeEnum.Personality;
                                    }
                                    if (quizType == (int)QuizTypeEnum.PersonalityTemplate)
                                    {
                                        quizTypeEnum = QuizTypeEnum.PersonalityTemplate;
                                    }

                                    CreateResultObj(UOWObj, quizTypeEnum, BusinessUserId, currentDate, quizDetails, "Result");
                                    CreateResultObj(UOWObj, quizTypeEnum, BusinessUserId, currentDate, quizDetails, "Based On Correlations");

                                    //resultObj = CreateResultObj(quizDetails.Id, BusinessUserId, currentDate, quizDetails, quizType);

                                    //resultObj.QuizId = quizDetails.Id;
                                    //resultObj.Title = "Result " + (quizDetails.QuizResults.Where(r => r.State == (int)StatusEnum.Active).Count() + 1).ToString();
                                    //resultObj.InternalTitle = "Result " + (quizDetails.QuizResults.Where(r => r.State == (int)StatusEnum.Active).Count() + 1).ToString();
                                    //resultObj.ShowResultImage = true;
                                    //resultObj.Description = "Result Description";
                                    //resultObj.HideCallToAction = false;
                                    //resultObj.OpenLinkInNewTab = true;
                                    //resultObj.ActionButtonTxtSize = "24 px";
                                    //resultObj.ActionButtonText = "Call To Action";
                                    //resultObj.Status = (int)StatusEnum.Active;
                                    //resultObj.LastUpdatedBy = BusinessUserId;
                                    //resultObj.LastUpdatedOn = currentDate;
                                    //resultObj.State = (int)QuizStateEnum.DRAFTED;
                                    //resultObj.DisplayOrder = 2;
                                    //resultObj.MinScore = null;
                                    //resultObj.MaxScore = null;
                                    //resultObj.ShowLeadUserForm = false;
                                    //resultObj.ShowExternalTitle = true;
                                    //resultObj.ShowInternalTitle = true;
                                    //resultObj.ShowDescription = true;
                                    //resultObj.EnableMediaFile = false;
                                    //resultObj.DisplayOrderForTitle = 1;
                                    //resultObj.DisplayOrderForTitleImage = 2;
                                    //resultObj.DisplayOrderForDescription = 3;
                                    //resultObj.DisplayOrderForNextButton = 4;

                                    //UOWObj.QuizResultsRepository.Insert(resultObj);
                                    //UOWObj.Save();


                                    //resultObj = new Db.QuizResults();

                                    //resultObj.QuizId = quizDetails.Id;
                                    //resultObj.Title = "Based On Correlations";
                                    //resultObj.InternalTitle = "Based On Correlations";
                                    //resultObj.ShowResultImage = false;
                                    //resultObj.Description = "Calculated Result";
                                    //resultObj.HideCallToAction = false;
                                    //resultObj.OpenLinkInNewTab = false;
                                    //resultObj.ActionButtonTxtSize = "24 px";
                                    //resultObj.ActionButtonText = "Call To Action";
                                    //resultObj.Status = (int)StatusEnum.Active;
                                    //resultObj.LastUpdatedBy = BusinessUserId;
                                    //resultObj.LastUpdatedOn = currentDate;
                                    //resultObj.State = (int)QuizStateEnum.DRAFTED;
                                    //resultObj.IsPersonalityCorrelatedResult = true;
                                    //resultObj.DisplayOrder = 3;
                                    //resultObj.MinScore = null;
                                    //resultObj.MaxScore = null;
                                    //resultObj.ShowLeadUserForm = false;
                                    //resultObj.ShowExternalTitle = true;
                                    //resultObj.ShowInternalTitle = true;
                                    //resultObj.ShowDescription = true;
                                    //resultObj.EnableMediaFile = false;
                                    //resultObj.DisplayOrderForTitle = 1;
                                    //resultObj.DisplayOrderForTitleImage = 2;
                                    //resultObj.DisplayOrderForDescription = 3;
                                    //resultObj.DisplayOrderForNextButton = 4;

                                    //UOWObj.QuizResultsRepository.Insert(resultObj);
                                    //UOWObj.Save();

                                    var multipleresult = new Db.PersonalityResultSetting();
                                    multipleresult.QuizId = quizDetails.Id;
                                    multipleresult.Title = "Your Top Results";
                                    multipleresult.Status = (int)StatusEnum.Inactive;
                                    multipleresult.GraphColor = "#417341";
                                    multipleresult.MaxResult = 2;
                                    //multipleresult.ButtonColor = "#bc3939";
                                    multipleresult.ButtonFontColor = "#ffffff";
                                    multipleresult.SideButtonText = "More Details";
                                    multipleresult.IsFullWidthEnable = false;
                                    multipleresult.LastUpdatedOn = DateTime.UtcNow;
                                    multipleresult.LastUpdatedBy = BusinessUserId;
                                    multipleresult.ShowLeadUserForm = false;

                                    UOWObj.PersonalityResultSettingRepository.Insert(multipleresult);
                                    UOWObj.Save();
                                }

                                if (quizType == (int)QuizTypeEnum.NPS)
                                {
                                    QuizTypeEnum quizTypeEnum = QuizTypeEnum.NPS;
                                    if (quizType == (int)QuizTypeEnum.NPS)
                                    {
                                        quizTypeEnum = QuizTypeEnum.NPS;
                                    }

                                    CreateResultObj(UOWObj, quizTypeEnum, BusinessUserId, currentDate, quizDetails, "Passive");
                                    CreateResultObj(UOWObj, quizTypeEnum, BusinessUserId, currentDate, quizDetails, "Promoter");

                                    //resultObj.QuizId = quizDetails.Id;
                                    //resultObj.Title = "Passive";
                                    //resultObj.InternalTitle = "Passive";
                                    //resultObj.ShowResultImage = true;
                                    //resultObj.Description = "Result Description";
                                    //resultObj.HideCallToAction = false;
                                    //resultObj.OpenLinkInNewTab = true;
                                    //resultObj.ActionButtonTxtSize = "24 px";
                                    //resultObj.ActionButtonText = "Call To Action";
                                    //resultObj.Status = (int)StatusEnum.Active;
                                    //resultObj.LastUpdatedBy = BusinessUserId;
                                    //resultObj.LastUpdatedOn = currentDate;
                                    //resultObj.State = (int)QuizStateEnum.DRAFTED;
                                    //resultObj.DisplayOrder = 2;
                                    //resultObj.MinScore = 7;
                                    //resultObj.MaxScore = 8;
                                    //resultObj.ShowLeadUserForm = false;
                                    //resultObj.DisplayOrderForTitle = 1;
                                    //resultObj.DisplayOrderForTitleImage = 2;
                                    //resultObj.DisplayOrderForDescription = 3;
                                    //resultObj.DisplayOrderForNextButton = 4;
                                    //resultObj.ShowExternalTitle = true;
                                    //resultObj.ShowInternalTitle = true;
                                    //resultObj.ShowDescription = true;
                                    //resultObj.EnableMediaFile = false;

                                    //UOWObj.QuizResultsRepository.Insert(resultObj);
                                    //UOWObj.Save();


                                    //resultObj = new Db.QuizResults();

                                    //resultObj.QuizId = quizDetails.Id;
                                    //resultObj.Title = "Promoter";
                                    //resultObj.InternalTitle = "Promoter";
                                    //resultObj.ShowResultImage = true;
                                    //resultObj.Description = "Result Description";
                                    //resultObj.HideCallToAction = false;
                                    //resultObj.OpenLinkInNewTab = true;
                                    //resultObj.ActionButtonTxtSize = "24 px";
                                    //resultObj.ActionButtonText = "Call To Action";
                                    //resultObj.Status = (int)StatusEnum.Active;
                                    //resultObj.LastUpdatedBy = BusinessUserId;
                                    //resultObj.LastUpdatedOn = currentDate;
                                    //resultObj.State = (int)QuizStateEnum.DRAFTED;
                                    //resultObj.DisplayOrder = 3;
                                    //resultObj.MinScore = 9;
                                    //resultObj.MaxScore = 10;
                                    //resultObj.ShowLeadUserForm = false;
                                    //resultObj.DisplayOrderForTitle = 1;
                                    //resultObj.DisplayOrderForTitleImage = 2;
                                    //resultObj.DisplayOrderForDescription = 3;
                                    //resultObj.DisplayOrderForNextButton = 4;
                                    //resultObj.ShowExternalTitle = true;
                                    //resultObj.ShowInternalTitle = true;
                                    //resultObj.ShowDescription = true;
                                    //resultObj.EnableMediaFile = false;

                                    //UOWObj.QuizResultsRepository.Insert(resultObj);
                                    //UOWObj.Save();
                                }

                                var resultSetting = quizDetails.ResultSettings.FirstOrDefault();

                                if (resultSetting == null)
                                {
                                    resultSetting = new Db.ResultSettings();

                                    resultSetting.QuizId = quizDetails.Id;
                                    resultSetting.ShowScoreValue = false;
                                    resultSetting.ShowCorrectAnswer = false;
                                    resultSetting.LastUpdatedBy = BusinessUserId;
                                    resultSetting.LastUpdatedOn = currentDate;
                                    resultSetting.State = (int)QuizStateEnum.DRAFTED;

                                    if (quizObj.QuizType == (int)QuizTypeEnum.Score || quizObj.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                        resultSetting.CustomTxtForScoreValueInResult = "You scored a%score%";
                                    else if (quizObj.QuizType == (int)QuizTypeEnum.NPS)
                                    {
                                        resultSetting.CustomTxtForScoreValueInResult = string.Empty;
                                        resultSetting.ShowScoreValue = false;
                                    }
                                    else
                                        resultSetting.CustomTxtForScoreValueInResult = "I got%score%out of%total%Correct";

                                    resultSetting.CustomTxtForAnswerKey = "Answer Key";
                                    resultSetting.CustomTxtForYourAnswer = "Your Answer";
                                    resultSetting.CustomTxtForCorrectAnswer = "Correct Answer";
                                    resultSetting.CustomTxtForExplanation = "Explanation";
                                    UOWObj.ResultSettingsRepository.Insert(resultSetting);
                                }

                                quizDetails.LastUpdatedBy = BusinessUserId;
                                quizDetails.LastUpdatedOn = currentDate;

                                quizObj.State = (int)QuizStateEnum.DRAFTED;

                                UOWObj.QuizRepository.Update(quizObj);

                                UOWObj.Save();
                                transaction.Complete();

                                #endregion

                                #region Bind return obj


                                quizResult = BindingObj (QuizId, resultObj, resultSetting);

                                #endregion

                                
                                
                            }
                        }
                        else
                        {
                            Status = ResultEnum.Error;
                            ErrorMessage = "Quiz not found for the QuizId " + QuizId;
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
            return quizResult;

        }

        private void CreateResultObj(AutomationUnitOfWork UOWObj, QuizTypeEnum quizTypeEnum, int businessUserId, DateTime currentDate, Db.QuizDetails quizDetails, string resultType)
        {
            Db.QuizResults resultObj = new Db.QuizResults();

            resultObj.QuizId = quizDetails.Id;
            resultObj.Title = "Passive";
            resultObj.InternalTitle = "Passive";
            resultObj.ShowResultImage = true;
            resultObj.Description = "Result Description";
            resultObj.HideCallToAction = false;
            resultObj.OpenLinkInNewTab = true;
            resultObj.ActionButtonTxtSize = "24 px";
            //resultObj.ActionButtonText = "Call To Action";
            resultObj.ActionButtonText = "";
            resultObj.Status = (int)StatusEnum.Active;
            resultObj.LastUpdatedBy = businessUserId;
            resultObj.LastUpdatedOn = currentDate;
            resultObj.State = (int)QuizStateEnum.DRAFTED;
            resultObj.DisplayOrder = 2;
            resultObj.MinScore = null;
            resultObj.MaxScore = null;
            resultObj.ShowLeadUserForm = false;
            resultObj.DisplayOrderForTitle = 1;
            resultObj.DisplayOrderForTitleImage = 2;
            resultObj.DisplayOrderForDescription = 3;
            resultObj.DisplayOrderForNextButton = 4;
            resultObj.ShowExternalTitle = true;
            resultObj.ShowInternalTitle = true;
            resultObj.ShowDescription = true;
            resultObj.EnableMediaFile = false;


            switch (quizTypeEnum)
            {
                case QuizTypeEnum.NPS:
                    {
                        if (resultType == "Passive")
                        {
                            resultObj.Title = "Passive";
                            resultObj.InternalTitle = "Passive";
                            resultObj.DisplayOrder = 2;
                            resultObj.MaxScore = 8;
                            resultObj.MinScore = 7;
                        }
                        else
                        {
                            resultObj.Title = "Promoter";
                            resultObj.InternalTitle = "Promoter";
                            resultObj.DisplayOrder = 3;
                            resultObj.MaxScore = 9;
                            resultObj.MinScore = 10;
                        }
                    }

                    break;

                case QuizTypeEnum.Personality:
                case QuizTypeEnum.PersonalityTemplate:
                    {

                        if (resultType == "Based On Correlations")
                        {
                            resultObj.Title = "Based On Correlations";
                            resultObj.InternalTitle = "Based On Correlations";
                            resultObj.ShowResultImage = false;
                            resultObj.Description = "Calculated Result";
                            resultObj.HideCallToAction = false;
                            resultObj.OpenLinkInNewTab = false;
                            resultObj.IsPersonalityCorrelatedResult = true;
                            resultObj.DisplayOrder = 3;
                            resultObj.MinScore = null;
                            resultObj.MaxScore = null;
                            resultObj.ShowLeadUserForm = false;
                        }
                        else
                        {
                            resultObj.Title = "Result " + (quizDetails.QuizResults.Where(r => r.State == (int)StatusEnum.Active).Count() + 1).ToString();
                            resultObj.InternalTitle = "Result " + (quizDetails.QuizResults.Where(r => r.State == (int)StatusEnum.Active).Count() + 1).ToString();
                            resultObj.MinScore = null;
                            resultObj.MaxScore = null;
                            resultObj.Description = "Result Description";
                            resultObj.DisplayOrder = 2;
                            resultObj.ShowResultImage = true;
                        }

                    }
                    break;
                default:
                    break;
            }

            UOWObj.QuizResultsRepository.Insert(resultObj);
            UOWObj.Save();
        }

        private QuizResult BindingObj(int QuizId, Db.QuizResults resultObj, Db.ResultSettings resultSetting)
        {
            QuizResult quizResult = new QuizResult();
            quizResult.ResultId = resultObj.Id;
            quizResult.Title = resultObj.Title;
            quizResult.InternalTitle = resultObj.InternalTitle;
            quizResult.ShowResultImage = resultObj.ShowResultImage;
            quizResult.Description = resultObj.Description;
            quizResult.HideCallToAction = resultObj.HideCallToAction;
            quizResult.EnableCallToActionButton = !(resultObj.HideCallToAction ?? false); ;
            quizResult.ActionButtonURL = resultObj.ActionButtonURL;
            quizResult.OpenLinkInNewTab = resultObj.OpenLinkInNewTab;
            quizResult.ActionButtonTxtSize = resultObj.ActionButtonTxtSize;
            quizResult.ActionButtonColor = resultObj.ActionButtonColor;
            quizResult.ActionButtonTitleColor = resultObj.ActionButtonTitleColor;
            quizResult.ActionButtonText = resultObj.ActionButtonText;
            quizResult.ShowExternalTitle = resultObj.ShowExternalTitle;
            quizResult.ShowInternalTitle = resultObj.ShowInternalTitle;
            quizResult.ShowDescription = resultObj.ShowDescription;
            quizResult.EnableMediaFile = resultObj.EnableMediaFile;
            quizResult.DisplayOrderForTitleImage = resultObj.DisplayOrderForTitleImage;
            quizResult.DisplayOrderForTitle = resultObj.DisplayOrderForTitle;
            quizResult.DisplayOrderForDescription = resultObj.DisplayOrderForDescription;
            quizResult.DisplayOrderForNextButton = resultObj.DisplayOrderForNextButton;

            quizResult.ResultSetting = new QuizResultSetting();

            quizResult.ResultSetting.QuizId = QuizId;
            quizResult.ResultSetting.ShowScoreValue = resultSetting.ShowScoreValue;
            quizResult.ResultSetting.ShowCorrectAnswer = resultSetting.ShowCorrectAnswer;
            quizResult.ResultSetting.CustomTxtForScoreValueInResult = resultSetting.CustomTxtForScoreValueInResult;
            quizResult.ResultSetting.CustomTxtForAnswerKey = resultSetting.CustomTxtForAnswerKey;
            quizResult.ResultSetting.CustomTxtForYourAnswer = resultSetting.CustomTxtForYourAnswer;
            quizResult.ResultSetting.CustomTxtForCorrectAnswer = resultSetting.CustomTxtForCorrectAnswer;
            quizResult.ResultSetting.CustomTxtForExplanation = resultSetting.CustomTxtForExplanation;
            return quizResult;
        }

        private Db.QuizResults AddingResultObj(int BusinessUserId, int? quizType, Db.QuizDetails quizDetails, DateTime currentDate)
        {
            var resultObj = new Db.QuizResults();

            resultObj.QuizId = quizDetails.Id;
            resultObj.Title = quizType == (int)QuizTypeEnum.NPS ? "Detractor" : "Result " + (quizDetails.QuizResults.Where(r => r.State == (int)StatusEnum.Active).Count() + 1).ToString();
            resultObj.InternalTitle = quizType == (int)QuizTypeEnum.NPS ? "Detractor" : "Result " + (quizDetails.QuizResults.Where(r => r.State == (int)StatusEnum.Active).Count() + 1).ToString();
            resultObj.ShowResultImage = true;
            resultObj.Description = "Result Description";
            resultObj.HideCallToAction = false;
            resultObj.OpenLinkInNewTab = true;
            resultObj.ActionButtonTxtSize = "24 px";
            //resultObj.ActionButtonText = "Call To Action";
            resultObj.ActionButtonText = "";
            resultObj.Status = (int)StatusEnum.Active;
            resultObj.LastUpdatedBy = BusinessUserId;
            resultObj.LastUpdatedOn = currentDate;
            resultObj.State = (int)QuizStateEnum.DRAFTED;
            resultObj.ShowLeadUserForm = false;
            resultObj.AutoPlay = true;
            resultObj.SecondsToApply = quizDetails.SecondsToApply;
            //resultObj.VideoFrameEnabled = quizDetails.VideoFrameEnabled;
            resultObj.ShowExternalTitle = true;
            resultObj.ShowInternalTitle = true;
            resultObj.ShowDescription = true;
            resultObj.EnableMediaFile = false;
            resultObj.DisplayOrderForTitleImage = 2;
            resultObj.DisplayOrderForTitle = 1;
            resultObj.DisplayOrderForDescription = 3;
            resultObj.DisplayOrderForNextButton = 4;

            if (quizType == (int)QuizTypeEnum.Assessment || quizType == (int)QuizTypeEnum.AssessmentTemplate)
            {
                resultObj.MinScore = 0;
                resultObj.MaxScore = 1;
            }
            else if (quizType == (int)QuizTypeEnum.Personality || quizType == (int)QuizTypeEnum.PersonalityTemplate)
            {
                resultObj.DisplayOrder = 1;
            }
            else if (quizType == (int)QuizTypeEnum.NPS)
            {
                resultObj.DisplayOrder = 1;
                resultObj.MinScore = 0;
                resultObj.MaxScore = 6;
            }
            else
            {
                resultObj.MinScore = 0;
                resultObj.MaxScore = 0;
            }

            return resultObj;
        }

        private QuizQuestionAndContent QuesContentMapping(Db.QuestionsInQuiz question)
        {
            var questionInQuiz = new QuizQuestionAndContent();

            questionInQuiz.QuestionId = question.Id;
            questionInQuiz.QuestionTitle = question.Question;
            questionInQuiz.DisplayOrder = question.DisplayOrder;
            questionInQuiz.ShowAnswerImage = question.ShowAnswerImage;
            questionInQuiz.AnswerType = question.AnswerType;
            questionInQuiz.MinAnswer = question.MinAnswer;
            questionInQuiz.MaxAnswer = question.MaxAnswer;
            questionInQuiz.TimerRequired = question.TimerRequired;
            questionInQuiz.Time = question.Time;
            questionInQuiz.AutoPlay = question.AutoPlay;
            questionInQuiz.SecondsToApply = question.SecondsToApply;
            questionInQuiz.VideoFrameEnabled = question.VideoFrameEnabled;
            questionInQuiz.Type = question.Type;
            questionInQuiz.ContentDescription = question.Description;
            questionInQuiz.ContentDescriptionImage = question.DescriptionImage;
            questionInQuiz.EnableMediaFileForDescription = question.EnableMediaFileForDescription;
            questionInQuiz.PublicIdForContentDescription = question.PublicIdForDescription;
            questionInQuiz.ShowContentDescriptionImage = question.ShowDescriptionImage;
            questionInQuiz.AutoPlayForDescription = question.AutoPlayForDescription;
            questionInQuiz.SecondsToApplyForDescription = question.SecondsToApplyForDescription;
            questionInQuiz.DescVideoFrameEnabled = question.DescVideoFrameEnabled;
            questionInQuiz.AnswerStructureType = question.AnswerStructureType.HasValue ? question.AnswerStructureType.Value : (int)AnswerStructureTypeEnum.Default;
            //questionInQuiz.LanguageId = question.LanguageId;
            questionInQuiz.LanguageCode = question.LanguageCode;
            questionInQuiz.TemplateId = question.TemplateId;
            questionInQuiz.IsMultiRating = question.IsMultiRating;

            questionInQuiz.AnswerList = new List<AnswerOptionInQuestion>();

            foreach (var answer in question.AnswerOptionsInQuizQuestions.Where(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active && !r.IsUnansweredType).OrderBy(r => r.DisplayOrder))
            {
                var answerOption = new AnswerOptionInQuestion();

                answerOption.AnswerId = answer.Id;
                answerOption.AnswerText = answer.Option;
                answerOption.DisplayOrder = answer.DisplayOrder;
                answerOption.AssociatedScore = answer.AssociatedScore;
                answerOption.IsReadOnly = answer.IsReadOnly;
                answerOption.RefId = answer.RefId;
                questionInQuiz.AnswerList.Add(answerOption);
            }

            return questionInQuiz;
        }

        private QuizQuestion QuesAnsMapping(Db.QuestionsInQuiz question)
        {
            var questionInQuiz = new QuizQuestion();

            questionInQuiz.QuestionId = question.Id;
            questionInQuiz.QuestionTitle = question.Question;
            questionInQuiz.DisplayOrder = question.DisplayOrder;
            questionInQuiz.ShowAnswerImage = question.ShowAnswerImage;
            questionInQuiz.AnswerType = question.AnswerType;
            questionInQuiz.MinAnswer = question.MinAnswer;
            questionInQuiz.MaxAnswer = question.MaxAnswer;
            questionInQuiz.TimerRequired = question.TimerRequired;
            questionInQuiz.Time = question.Time;
            questionInQuiz.AutoPlay = question.AutoPlay;
            questionInQuiz.SecondsToApply = question.SecondsToApply;
            questionInQuiz.VideoFrameEnabled = question.VideoFrameEnabled;
            questionInQuiz.Type = question.Type;
            questionInQuiz.AnswerStructureType = question.AnswerStructureType;
            questionInQuiz.TemplateId = question.TemplateId;
            //questionInQuiz.LanguageId = question.LanguageId;
            questionInQuiz.LanguageCode = question.LanguageCode;
            questionInQuiz.IsMultiRating = question.IsMultiRating;
            questionInQuiz.AnswerList = new List<AnswerOptionInQuestion>();

            foreach (var answer in question.AnswerOptionsInQuizQuestions.Where(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active && !r.IsUnansweredType).OrderBy(r => r.DisplayOrder))
            {
                var answerOption = new AnswerOptionInQuestion();

                answerOption.AnswerId = answer.Id;
                answerOption.AnswerText = answer.Option;
                answerOption.DisplayOrder = answer.DisplayOrder;
                answerOption.AssociatedScore = answer.AssociatedScore;
                answerOption.IsReadOnly = answer.IsReadOnly;
                answerOption.RefId = answer.RefId;
                questionInQuiz.AnswerList.Add(answerOption);
            }

            return questionInQuiz;
        }

        private QuizDetailsModel BrandingStylingObject(int QuizId, Db.Quiz quizObj, Db.QuizDetails quizDetailsObj)
        {
            QuizDetailsModel quizDetails;
            QuizBrandingAndStyleModel BrandingAndStyleObj = null;

            var brandingAndStyle = quizDetailsObj.QuizBrandingAndStyle.FirstOrDefault();

            BrandingAndStyleObj = new QuizBrandingAndStyleModel();
            BrandingAndStyleObj.QuizId = QuizId;

            if (brandingAndStyle != null)
            {
                BrandingAndStyleObj.ImageFileURL = brandingAndStyle.ImageFileURL;
                BrandingAndStyleObj.PublicIdForFileURL = brandingAndStyle.PublicId;
                BrandingAndStyleObj.BackgroundColor = brandingAndStyle.BackgroundColor;
                BrandingAndStyleObj.ButtonColor = brandingAndStyle.ButtonColor;
                BrandingAndStyleObj.OptionColor = brandingAndStyle.OptionColor;
                BrandingAndStyleObj.ButtonFontColor = brandingAndStyle.ButtonFontColor;
                BrandingAndStyleObj.OptionFontColor = brandingAndStyle.OptionFontColor;
                BrandingAndStyleObj.FontColor = brandingAndStyle.FontColor;
                BrandingAndStyleObj.ButtonHoverColor = brandingAndStyle.ButtonHoverColor;
                BrandingAndStyleObj.ButtonHoverTextColor = brandingAndStyle.ButtonHoverTextColor;
                BrandingAndStyleObj.FontType = brandingAndStyle.FontType;
                BrandingAndStyleObj.BackgroundColorofSelectedAnswer = brandingAndStyle.BackgroundColorofSelectedAnswer;
                BrandingAndStyleObj.BackgroundColorofAnsweronHover = brandingAndStyle.BackgroundColorofAnsweronHover;
                BrandingAndStyleObj.AnswerTextColorofSelectedAnswer = brandingAndStyle.AnswerTextColorofSelectedAnswer;
                BrandingAndStyleObj.IsBackType = brandingAndStyle.IsBackType;
                BrandingAndStyleObj.BackImageFileURL = brandingAndStyle.BackImageFileURL;
                BrandingAndStyleObj.Opacity = brandingAndStyle.Opacity;
                BrandingAndStyleObj.BackColor = brandingAndStyle.BackColor;
                BrandingAndStyleObj.LogoUrl = brandingAndStyle.LogoUrl;
                BrandingAndStyleObj.LogoPublicId = brandingAndStyle.LogoPublicId;
                BrandingAndStyleObj.BackgroundColorofLogo = brandingAndStyle.BackgroundColorofLogo;
                BrandingAndStyleObj.AutomationAlignment = brandingAndStyle.AutomationAlignment;
                BrandingAndStyleObj.LogoAlignment = brandingAndStyle.LogoAlignment;
                BrandingAndStyleObj.Flip = brandingAndStyle.Flip;
                BrandingAndStyleObj.Language = brandingAndStyle.Language;
            }
            quizDetails = new QuizDetailsModel();

            quizDetails.QuizId = quizDetailsObj.ParentQuizId;
            quizDetails.IsBranchingLogicEnabled = quizDetailsObj.IsBranchingLogicEnabled;
            quizDetails.IsPublished = quizObj.State == (int)QuizStateEnum.PUBLISHED;
            quizDetails.PublishedCode = quizObj.PublishedCode;
            quizDetails.QuizTypeId = quizObj.QuizType;
            quizDetails.OfficeId = quizObj.AccessibleOfficeId;
            quizDetails.ViewPreviousQuestion = quizDetailsObj.ViewPreviousQuestion;
            quizDetails.EditAnswer = quizDetailsObj.EditAnswer;
            quizDetails.ApplyToAll = quizDetailsObj.ApplyToAll;
            quizDetails.SecondsToApply = quizDetailsObj.SecondsToApply;
            quizDetails.VideoFrameEnabled = quizDetailsObj.VideoFrameEnabled;
            quizDetails.QuizCoverDetails = new QuizCover();
            quizDetails.QuizCoverDetails.QuizTitle = quizDetailsObj.QuizTitle;
            quizDetails.QuizCoverDetails.QuizCoverTitle = quizDetailsObj.QuizCoverTitle;
            quizDetails.QuizCoverDetails.ShowQuizCoverTitle = quizDetailsObj.ShowQuizCoverTitle;
            quizDetails.QuizCoverDetails.QuizCoverImage = quizDetailsObj.QuizCoverImage;
            quizDetails.QuizCoverDetails.ShowQuizCoverImage = quizDetailsObj.ShowQuizCoverImage;
            quizDetails.QuizCoverDetails.EnableMediaFile = quizDetailsObj.EnableMediaFile;
            quizDetails.QuizCoverDetails.PublicIdForQuizCover = quizDetailsObj.PublicId;
            quizDetails.QuizCoverDetails.QuizCoverImgXCoordinate = quizDetailsObj.QuizCoverImgXCoordinate;
            quizDetails.QuizCoverDetails.QuizCoverImgYCoordinate = quizDetailsObj.QuizCoverImgYCoordinate;
            quizDetails.QuizCoverDetails.QuizCoverImgHeight = quizDetailsObj.QuizCoverImgHeight;
            quizDetails.QuizCoverDetails.QuizCoverImgWidth = quizDetailsObj.QuizCoverImgWidth;
            quizDetails.QuizCoverDetails.QuizCoverImgAttributionLabel = quizDetailsObj.QuizCoverImgAttributionLabel;
            quizDetails.QuizCoverDetails.QuizCoverImgAltTag = quizDetailsObj.QuizCoverImgAltTag;
            quizDetails.QuizCoverDetails.QuizDescription = quizDetailsObj.QuizDescription;
            quizDetails.QuizCoverDetails.ShowDescription = quizDetailsObj.ShowDescription;
            quizDetails.QuizCoverDetails.QuizStartButtonText = quizDetailsObj.StartButtonText;
            quizDetails.QuizCoverDetails.AutoPlay = quizDetailsObj.AutoPlay;
            quizDetails.QuizCoverDetails.SecondsToApply = quizDetailsObj.SecondsToApply;
            quizDetails.QuizCoverDetails.VideoFrameEnabled = quizDetailsObj.VideoFrameEnabled;
            quizDetails.QuizCoverDetails.EnableNextButton = quizDetailsObj.EnableNextButton;
            quizDetails.QuizCoverDetails.DisplayOrderForTitleImage = quizDetailsObj.DisplayOrderForTitleImage;
            quizDetails.QuizCoverDetails.DisplayOrderForTitle = quizDetailsObj.DisplayOrderForTitle;
            quizDetails.QuizCoverDetails.DisplayOrderForDescription = quizDetailsObj.DisplayOrderForDescription;
            quizDetails.QuizCoverDetails.DisplayOrderForNextButton = quizDetailsObj.DisplayOrderForNextButton;
            quizDetails.IsQuesAndContentInSameTable = quizObj.QuesAndContentInSameTable;
            quizDetails.QuizBrandingAndStyle = BrandingAndStyleObj;
            quizDetails.QuestionsInQuiz = new List<QuizQuestion>();
            quizDetails.QuizAction = new List<QuizAction>();
            quizDetails.QuizContent = new List<QuizContent>();
            return quizDetails;
        }


        #region Old Method BindCompanyDetails
        private CompanyModel BindCompanyDetails(Db.Company companyInfo)
        {
            CompanyModel companyObj;
            if (companyInfo != null)
            {
                companyObj = new CompanyModel
                {
                    Id = companyInfo.Id,
                    AlternateClientCodes = companyInfo.AlternateClientCodes,
                    ClientCode = companyInfo.ClientCode,
                    CompanyName = companyInfo.CompanyName,
                    CompanyWebsiteUrl = companyInfo.CompanyWebsiteUrl,
                    JobRocketApiAuthorizationBearer = companyInfo.JobRocketApiAuthorizationBearer,
                    JobRocketApiUrl = companyInfo.JobRocketApiUrl,
                    JobRocketClientUrl = companyInfo.JobRocketClientUrl,
                    LeadDashboardApiAuthorizationBearer = companyInfo.LeadDashboardApiAuthorizationBearer,
                    LeadDashboardApiUrl = companyInfo.LeadDashboardApiUrl,
                    LeadDashboardClientUrl = companyInfo.LeadDashboardClientUrl,
                    LogoUrl = companyInfo.LogoUrl,
                    PrimaryBrandingColor = companyInfo.PrimaryBrandingColor,
                    SecondaryBrandingColor = companyInfo.SecondaryBrandingColor

                };
            }
            else
            {
                companyObj = new CompanyModel();
            }

            return companyObj;

        }
        #endregion

        public QuizAnswerSubmit AttemptQuiz(List<TextAnswer> TextAnswerList, string QuizCode, string Mode, string Type, int QuestionId = -1, string AnswerIdList = "", int BusinessUserId = 1, int UserTypeId = 0, int? QuestionType = (int)BranchingLogicEnum.QUESTION, int? UsageType = null)
        {
            QuizAnswerSubmit quizAnswerSubmit = new QuizAnswerSubmit();
            int attemptQuizLogId = 0;


            try
            {
                Db.QuizAttempts quizAttemptsObj;
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    List<Db.ResultIdsInConfigurationDetails> leadFormDetailofResultsLst = null;
                    quizAttemptsObj = UOWObj.QuizAttemptsRepository.Get(r => r.Code == QuizCode).FirstOrDefault();
                    var resultIdList = quizAttemptsObj.ConfigurationDetailsId > 0 ? quizAttemptsObj.ConfigurationDetails.ResultIdsInConfigurationDetails.Select(r => r.ResultId).ToList() : new List<int>();

                    if (Mode != "PREVIEW" && Mode != "PREVIEWTEMPLATE" && resultIdList.Any())
                    {
                        leadFormDetailofResultsLst = quizAttemptsObj.ConfigurationDetails.ResultIdsInConfigurationDetails.ToList();
                    }

                    var AnswerId = string.IsNullOrEmpty(AnswerIdList) ? new List<int>() : AnswerIdList.Split(',').Select(Int32.Parse).ToList();
                    var notAttemptedQuesText = "You haven't attempted the question.";

                    if (quizAttemptsObj != null)
                    {
                        var currentDate = DateTime.UtcNow;

                        bool isLastQuestionAttempted = false, isQuizAlreadyStarted = false, isLastQuestionStarted = false, revealScore = true;

                        var quizStatsObj = quizAttemptsObj.QuizStats.FirstOrDefault();

                        var quizDetails = quizAttemptsObj.QuizDetails;

                        var quizObj = quizDetails.Quiz;

                        var isQuesAndContentInSameTable = quizObj.QuesAndContentInSameTable;

                        CompanyModel companyObj = BindCompanyDetails(quizObj.Company);
                        // Model.Company companyObj = UOWObj.CompanyRepository.BindCompanyDetails(quizObj.Company.CompanyId);


                        var quizComponentLogsList = quizDetails.QuizComponentLogs.Where(r => r.ObjectTypeId == (int)BranchingLogicEnum.RESULT);

                        SetDefaulQuestionAnswer(ref Type, ref QuestionId, quizAttemptsObj, ref AnswerId, ref isLastQuestionAttempted, ref isQuizAlreadyStarted, ref isLastQuestionStarted, ref revealScore, quizStatsObj);

                        var quizQuestionStatsObj = new Db.QuizQuestionStats();

                        QuizBrandingAndStyleModel BrandingAndStyleObj = null;
                        BrandingAndStyleObj = BrandingStyleObj(quizDetails);
                        quizAnswerSubmit.QuizBrandingAndStyle = BrandingAndStyleObj;

                        quizAnswerSubmit.QuizCoverDetails = new QuizCover();
                        quizAnswerSubmit.QuizCoverDetails.QuizType = quizObj.QuizType;
                        quizAnswerSubmit.QuizCoverDetails.QuizTitle = VariableLinking(quizDetails.QuizTitle, quizDetails, quizAttemptsObj, true, false, null);
                        quizAnswerSubmit.QuizCoverDetails.QuizCoverTitle = VariableLinking(quizDetails.QuizCoverTitle, quizDetails, quizAttemptsObj, true, false, null);
                        quizAnswerSubmit.QuizCoverDetails.ShowQuizCoverTitle = quizDetails.ShowQuizCoverTitle;
                        quizAnswerSubmit.QuizCoverDetails.AutoPlay = quizDetails.AutoPlay;
                        quizAnswerSubmit.QuizCoverDetails.SecondsToApply = quizDetails.SecondsToApply ?? "0";
                        quizAnswerSubmit.QuizCoverDetails.QuizDescription = quizDetails.QuizDescription;
                        quizAnswerSubmit.QuizCoverDetails.ShowDescription = quizDetails.ShowDescription;
                        quizAnswerSubmit.QuizCoverDetails.EnableNextButton = quizDetails.EnableNextButton;
                        quizAnswerSubmit.QuizCoverDetails.DisplayOrderForTitleImage = quizDetails.DisplayOrderForTitleImage;
                        quizAnswerSubmit.QuizCoverDetails.DisplayOrderForTitle = quizDetails.DisplayOrderForTitle;
                        quizAnswerSubmit.QuizCoverDetails.DisplayOrderForDescription = quizDetails.DisplayOrderForDescription;
                        quizAnswerSubmit.QuizCoverDetails.DisplayOrderForNextButton = quizDetails.DisplayOrderForNextButton;
                        quizAnswerSubmit.PrimaryBrandingColor = companyObj.PrimaryBrandingColor;
                        quizAnswerSubmit.SecondaryBrandingColor = companyObj.SecondaryBrandingColor;
                        quizAnswerSubmit.TertiaryColor = companyObj.TertiaryColor;
                        quizAnswerSubmit.SourceId = quizAttemptsObj.ConfigurationDetailsId > 0 ? quizAttemptsObj.ConfigurationDetails.SourceId : string.Empty;
                        quizAnswerSubmit.SourceName = quizAttemptsObj.ConfigurationDetailsId > 0 ? quizAttemptsObj.ConfigurationDetails.SourceName : string.Empty;
                        quizAnswerSubmit.SourceType = quizAttemptsObj.ConfigurationDetailsId > 0 ? quizAttemptsObj.ConfigurationDetails.SourceType : string.Empty;
                        quizAnswerSubmit.PrivacyLink = quizAttemptsObj.ConfigurationDetailsId > 0 ? quizAttemptsObj.ConfigurationDetails.PrivacyLink : string.Empty;
                        quizAnswerSubmit.ConfigurationType = quizAttemptsObj.ConfigurationDetailsId > 0 ? quizAttemptsObj.ConfigurationDetails.ConfigurationType : string.Empty;
                        quizAnswerSubmit.ConfigurationId = quizAttemptsObj.ConfigurationDetailsId > 0 ? quizAttemptsObj.ConfigurationDetails.ConfigurationId : string.Empty;
                        quizAnswerSubmit.LeadFormTitle = quizAttemptsObj.ConfigurationDetailsId > 0 ? quizAttemptsObj.ConfigurationDetails.LeadFormTitle : string.Empty;
                        quizAnswerSubmit.OfficeId = quizObj.AccessibleOfficeId;
                        quizAnswerSubmit.PrivacyJson = quizAttemptsObj.ConfigurationDetailsId > 0 ? (!string.IsNullOrWhiteSpace(quizAttemptsObj.ConfigurationDetails.PrivacyJson) ? JsonConvert.DeserializeObject<PrivacyDto>(quizAttemptsObj.ConfigurationDetails.PrivacyJson) : null) : null;

                        var domainList = CommonStaticData.GetCachedClientDomains(companyObj);
                        if (domainList != null && domainList.Any())
                        {
                            quizAnswerSubmit.GtmCode = domainList.FirstOrDefault().GtmCode;
                            quizAnswerSubmit.FavoriteIconUrl = domainList.FirstOrDefault().FavoriteIconUrl;
                        }

                        quizAnswerSubmit.Tag = SetAnswerTags(quizAnswerSubmit, quizObj, companyObj.ClientCode);

                        //Fetch UserMedia Classification files
                        Response.OWCLeadUserResponse.LeadUserResponse leadUserInfo = null;
                        List<Response.UserMediaClassification> userMediaClassifications = new List<Response.UserMediaClassification>();
                        IEnumerable<Db.UserTokens> dbUsers = null;
                        if (!string.IsNullOrEmpty(quizAttemptsObj.LeadUserId) && quizDetails.MediaVariablesDetails.Any(a => !string.IsNullOrWhiteSpace(a.MediaOwner) && !string.IsNullOrWhiteSpace(a.ProfileMedia)))
                        {
                            leadUserInfo = OWCHelper.GetLeadUserInfo(quizAttemptsObj.LeadUserId, companyObj);
                            if (leadUserInfo.SourceOwnerId != 0 || leadUserInfo.ContactOwnerId != 0)
                            {
                                List<int> businessUserIds = new List<int>();
                                if (leadUserInfo.SourceOwnerId != 0) businessUserIds.Add(leadUserInfo.SourceOwnerId);
                                if (leadUserInfo.ContactOwnerId != 0) businessUserIds.Add(leadUserInfo.ContactOwnerId);

                                dbUsers = UOWObj.UserTokensRepository.Get(r => r.CompanyId == companyObj.Id && businessUserIds.Distinct().Contains(r.BusinessUserId));
                                userMediaClassifications = dbUsers.Any() ? OWCHelper.GetUserMediaClassification(companyObj.ClientCode, dbUsers.Select(a => a.OWCToken).ToList()) : new List<Response.UserMediaClassification>();
                            }
                        }


                        #region Logging Insertion For AttemptQuiz

                        if (enableAttemptQuizLogging)
                        {
                            var resultjson = new RequestJson()
                            {
                                TextAnswerList = TextAnswerList,
                                QuizCode = QuizCode,
                                Mode = Mode,
                                Type = Type,
                                QuestionId = QuestionId,
                                AnswerIdList = AnswerIdList,
                                BusinessUserId = BusinessUserId,
                                UserTypeId = UserTypeId,
                                QuestionType = QuestionType,
                                UsageType = UsageType
                            };

                            attemptQuizLogId = AttemptQuizLog.InsertLogging(quizAttemptsObj.QuizId, quizAttemptsObj.LeadUserId, quizAttemptsObj.Id, currentDate, JsonConvert.SerializeObject(resultjson));

                        }
                        #endregion




                        var urlhub = GlobalSettings.HubUrl.ToString();
                        var token = GlobalSettings.HubUrlBearer.ToString();
                        var urlAutomation = urlhub + "/api/v1/Automations/web-hooks/UpdateAutomationStatus";
                        var urlAppointment = urlhub + "/api/v1/appointments/web-hooks/UpdateAppointmentStatus";
                    switchcase:
                        switch (Type)
                        {
                            case "previous_question":

                                #region fetch next question details                                

                                object previousQuestionObj = FetchPreviousQuestion(quizAttemptsObj, QuestionId, QuestionType.Value, UOWObj, isQuesAndContentInSameTable);

                                if (previousQuestionObj != null)
                                {
                                    QuestionTypeQuestion(QuestionId, QuestionType, quizAttemptsObj, UOWObj, quizObj);

                                    #region if previous is question type

                                    if (previousQuestionObj.GetType().BaseType.Name == "QuestionsInQuiz" && (((Db.QuestionsInQuiz)previousQuestionObj).ViewPreviousQuestion || ((Db.QuestionsInQuiz)previousQuestionObj).EditAnswer))
                                    {
                                        quizQuestionStatsObj = PreviousTypeQuestion(quizAnswerSubmit, quizAttemptsObj, UOWObj, currentDate, isLastQuestionStarted, quizDetails, isQuesAndContentInSameTable, quizQuestionStatsObj, leadUserInfo, userMediaClassifications, dbUsers, previousQuestionObj);
                                    }

                                    #endregion                                   

                                    #region if previous is content type

                                    else if (previousQuestionObj.GetType().BaseType.Name == "ContentsInQuiz" && ((Db.ContentsInQuiz)previousQuestionObj).ViewPreviousQuestion)

                                        PreviousTypeContent(quizAnswerSubmit, quizAttemptsObj, UOWObj, quizDetails, isQuesAndContentInSameTable, leadUserInfo, userMediaClassifications, dbUsers, previousQuestionObj);

                                    #endregion
                                }
                                else
                                {
                                    Status = ResultEnum.Error;
                                    ErrorMessage = "You can not see the previous question.";
                                }


                                #endregion
                                break;

                            case "fetch_quiz":

                                #region fetch quiz cover details

                                FetchCoverDetails(quizAnswerSubmit, quizAttemptsObj, UOWObj, quizDetails, leadUserInfo, userMediaClassifications, dbUsers);

                                #endregion

                                break;

                            case "start_quiz":

                                #region fetch first question / result

                                if (!isQuizAlreadyStarted)
                                {
                                    quizStatsObj = FetchingFirstQues(quizAttemptsObj, UOWObj, currentDate, quizDetails, quizObj, companyObj, token, urlAutomation);
                                }

                                quizAnswerSubmit.IsBackButtonEnable = false;

                                var nextQuestionObj = FetchNextQuestion(quizDetails, UOWObj: UOWObj, IsQuesAndContentInSameTable: isQuesAndContentInSameTable);

                                if (nextQuestionObj != null)
                                {
                                    #region if next is question type

                                    if (nextQuestionObj.GetType().BaseType.Name == "QuestionsInQuiz")
                                    {
                                        if (((Db.QuestionsInQuiz)nextQuestionObj).Type == (int)BranchingLogicEnum.QUESTION)
                                        {
                                            NextQuesTypeQues(quizAnswerSubmit, quizAttemptsObj, isLastQuestionStarted, quizDetails, leadUserInfo, userMediaClassifications, dbUsers, nextQuestionObj);

                                            if (((Db.QuestionsInQuiz)nextQuestionObj).EnableMediaFileForDescription && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Description && r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION && r.ObjectId == ((Db.QuestionsInQuiz)nextQuestionObj).Id))
                                            {
                                                var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Description && r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION && r.ObjectId == ((Db.QuestionsInQuiz)nextQuestionObj).Id);
                                                quizAnswerSubmit.QuestionDetails.DescriptionImage = mediaObj.ObjectValue ?? string.Empty;
                                                quizAnswerSubmit.QuestionDetails.PublicIdForDescription = mediaObj.ObjectPublicId ?? string.Empty;

                                                var newMedia = ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers);
                                                if (newMedia != null)
                                                {
                                                    quizAnswerSubmit.QuestionDetails.DescriptionImage = newMedia.MediaUrl;
                                                    quizAnswerSubmit.QuestionDetails.PublicIdForDescription = newMedia.MediaPublicId;
                                                }
                                            }
                                            else
                                            {
                                                quizAnswerSubmit.QuestionDetails.DescriptionImage = ((Db.QuestionsInQuiz)nextQuestionObj).DescriptionImage ?? string.Empty;
                                                quizAnswerSubmit.QuestionDetails.PublicIdForDescription = ((Db.QuestionsInQuiz)nextQuestionObj).PublicIdForDescription ?? string.Empty;
                                            }

                                            quizAnswerSubmit.QuestionType = (int)BranchingLogicEnum.QUESTION;

                                            quizAnswerSubmit.QuestionDetails.AnswerList = new List<AnswerOptionInQuestion>();

                                            foreach (var ans in ((Db.QuestionsInQuiz)nextQuestionObj).AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && !r.IsUnansweredType).OrderBy(r => r.DisplayOrder))
                                            {
                                                var answerImage = string.Empty;
                                                var publicIdForAnswer = string.Empty;

                                                if (((Db.QuestionsInQuiz)nextQuestionObj).ShowAnswerImage.HasValue && ((Db.QuestionsInQuiz)nextQuestionObj).ShowAnswerImage.Value && ans.EnableMediaFile && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.ObjectId == ans.Id))
                                                {
                                                    var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.ObjectId == ans.Id);
                                                    answerImage = mediaObj.ObjectValue;
                                                    publicIdForAnswer = mediaObj.ObjectPublicId;

                                                    var newMedia = ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers);
                                                    if (newMedia != null)
                                                    {
                                                        answerImage = newMedia.MediaUrl;
                                                        publicIdForAnswer = newMedia.MediaPublicId;
                                                    }
                                                }
                                                else
                                                {
                                                    answerImage = ((Db.QuestionsInQuiz)nextQuestionObj).ShowAnswerImage.HasValue && ((Db.QuestionsInQuiz)nextQuestionObj).ShowAnswerImage.Value ? ans.OptionImage : string.Empty;
                                                    publicIdForAnswer = ((Db.QuestionsInQuiz)nextQuestionObj).ShowAnswerImage.HasValue && ((Db.QuestionsInQuiz)nextQuestionObj).ShowAnswerImage.Value ? ans.PublicId : string.Empty;
                                                }

                                                quizAnswerSubmit.QuestionDetails.AnswerList.Add(new AnswerOptionInQuestion
                                                {
                                                    AnswerId = ans.Id,
                                                    AssociatedScore = ans.AssociatedScore,
                                                    AnswerText = VariableLinking(ans.Option, quizDetails, quizAttemptsObj, false, false, null),
                                                    AnswerImage = answerImage,
                                                    PublicIdForAnswer = publicIdForAnswer,
                                                    IsCorrectAnswer = false,
                                                    DisplayOrder = ans.DisplayOrder,
                                                    IsUnansweredType = ans.IsUnansweredType,
                                                    AutoPlay = ans.AutoPlay,
                                                    SecondsToApply = ans.SecondsToApply,
                                                    VideoFrameEnabled = ans.VideoFrameEnabled,
                                                    ListValues = ans.ListValues,
                                                    //for Rating type question
                                                    OptionTextforRatingOne = ans.OptionTextforRatingOne,
                                                    OptionTextforRatingTwo = ans.OptionTextforRatingTwo,
                                                    OptionTextforRatingThree = ans.OptionTextforRatingThree,
                                                    OptionTextforRatingFour = ans.OptionTextforRatingFour,
                                                    OptionTextforRatingFive = ans.OptionTextforRatingFive
                                                });
                                            }


                                            #region PreviousQuestionSubmittedAnswer

                                            if (quizAttemptsObj.QuizQuestionStats.FirstOrDefault(r => r.QuestionId == ((Db.QuestionsInQuiz)nextQuestionObj).Id) != null && (quizAttemptsObj.QuizQuestionStats.Any(r => r.QuestionId == ((Db.QuestionsInQuiz)nextQuestionObj).Id && r.CompletedOn.HasValue) ?
                                                quizAttemptsObj.QuizQuestionStats.LastOrDefault(r => r.QuestionId == ((Db.QuestionsInQuiz)nextQuestionObj).Id && r.CompletedOn.HasValue).QuizAnswerStats != null
                                                : quizAttemptsObj.QuizQuestionStats.LastOrDefault(r => r.QuestionId == ((Db.QuestionsInQuiz)nextQuestionObj).Id).QuizAnswerStats != null))
                                            {
                                                var attemptedAnswerIds = quizAttemptsObj.QuizQuestionStats.Any(r => r.QuestionId == ((Db.QuestionsInQuiz)nextQuestionObj).Id && r.CompletedOn.HasValue) ?
                                                    quizAttemptsObj.QuizQuestionStats.LastOrDefault(r => r.QuestionId == ((Db.QuestionsInQuiz)nextQuestionObj).Id && r.CompletedOn.HasValue).QuizAnswerStats.Select(r => r.AnswerId).ToList() :
                                                    quizAttemptsObj.QuizQuestionStats.LastOrDefault(r => r.QuestionId == ((Db.QuestionsInQuiz)nextQuestionObj).Id).QuizAnswerStats.Select(r => r.AnswerId).ToList();

                                                var submittedAnswerOption = ((Db.QuestionsInQuiz)nextQuestionObj).AnswerOptionsInQuizQuestions.Where(r => attemptedAnswerIds.Contains(r.Id));
                                                if (submittedAnswerOption.Any())
                                                {
                                                    PreviousQuestionSubmittedAnswerMapping(quizAnswerSubmit, nextQuestionObj);

                                                    foreach (var submittedAnswerOptionObj in submittedAnswerOption)
                                                        CheckQuizAnswerType(quizAnswerSubmit, quizAttemptsObj, quizDetails, nextQuestionObj, submittedAnswerOptionObj);
                                                }
                                            }
                                            #endregion

                                            if (!isLastQuestionStarted)
                                            {
                                                quizQuestionStatsObj.QuizAttemptId = quizAttemptsObj.Id;
                                                quizQuestionStatsObj.QuestionId = ((Db.QuestionsInQuiz)nextQuestionObj).Id;
                                                quizQuestionStatsObj.StartedOn = currentDate;
                                                quizQuestionStatsObj.Status = (int)StatusEnum.Active;

                                                UOWObj.QuizQuestionStatsRepository.Insert(quizQuestionStatsObj);
                                                UOWObj.Save();
                                            }
                                        }
                                        else if (((Db.QuestionsInQuiz)nextQuestionObj).Type == (int)BranchingLogicEnum.CONTENT)
                                        {
                                            quizAnswerSubmit.QuestionType = (int)BranchingLogicEnum.CONTENT;

                                            if (!isLastQuestionStarted)
                                            {
                                                quizQuestionStatsObj = new Db.QuizQuestionStats();

                                                quizQuestionStatsObj.QuizAttemptId = quizAttemptsObj.Id;
                                                quizQuestionStatsObj.QuestionId = ((Db.QuestionsInQuiz)nextQuestionObj).Id;
                                                quizQuestionStatsObj.StartedOn = currentDate;
                                                quizQuestionStatsObj.Status = (int)StatusEnum.Active;

                                                UOWObj.QuizQuestionStatsRepository.Insert(quizQuestionStatsObj);
                                                UOWObj.Save();
                                            }

                                            quizAnswerSubmit.ContentDetails = new QuizContent();

                                            quizAnswerSubmit.ContentDetails.Id = ((Db.QuestionsInQuiz)nextQuestionObj).Id;
                                            quizAnswerSubmit.ContentDetails.ContentTitle = VariableLinking(((Db.QuestionsInQuiz)nextQuestionObj).Question, quizDetails, quizAttemptsObj, false, false, null);
                                            quizAnswerSubmit.ContentDetails.ContentDescription = VariableLinking(((Db.QuestionsInQuiz)nextQuestionObj).Description, quizDetails, quizAttemptsObj, false, false, null);
                                            quizAnswerSubmit.ContentDetails.ShowDescription = ((Db.QuestionsInQuiz)nextQuestionObj).ShowDescription;

                                            if ((((Db.QuestionsInQuiz)nextQuestionObj).EnableMediaFile && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Title && r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == ((Db.QuestionsInQuiz)nextQuestionObj).Id)))
                                            {
                                                var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Title && r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == ((Db.QuestionsInQuiz)nextQuestionObj).Id);
                                                quizAnswerSubmit.ContentDetails.ContentTitleImage = mediaObj.ObjectValue;
                                                quizAnswerSubmit.ContentDetails.PublicIdForContentTitle = mediaObj.ObjectPublicId ?? string.Empty;

                                                var newMedia = ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers);
                                                if (newMedia != null)
                                                {
                                                    quizAnswerSubmit.ContentDetails.ContentTitleImage = newMedia.MediaUrl;
                                                    quizAnswerSubmit.ContentDetails.PublicIdForContentTitle = newMedia.MediaPublicId;
                                                }
                                            }
                                            else
                                            {
                                                quizAnswerSubmit.ContentDetails.ContentTitleImage = ((Db.QuestionsInQuiz)nextQuestionObj).QuestionImage ?? string.Empty;
                                                quizAnswerSubmit.ContentDetails.PublicIdForContentTitle = ((Db.QuestionsInQuiz)nextQuestionObj).PublicId ?? string.Empty;
                                            }

                                            quizAnswerSubmit.ContentDetails.ShowContentTitleImage = ((Db.QuestionsInQuiz)nextQuestionObj).ShowQuestionImage.HasValue && ((Db.QuestionsInQuiz)nextQuestionObj).ShowQuestionImage.Value ? ((Db.QuestionsInQuiz)nextQuestionObj).ShowQuestionImage.Value : false;
                                            quizAnswerSubmit.ContentDetails.AliasTextForNextButton = ((Db.QuestionsInQuiz)nextQuestionObj).NextButtonText;
                                            quizAnswerSubmit.ContentDetails.EnableNextButton = ((Db.QuestionsInQuiz)nextQuestionObj).EnableNextButton;

                                            if (((Db.QuestionsInQuiz)nextQuestionObj).EnableMediaFileForDescription && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Description && r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == ((Db.QuestionsInQuiz)nextQuestionObj).Id))
                                            {
                                                var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Description && r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == ((Db.QuestionsInQuiz)nextQuestionObj).Id);
                                                quizAnswerSubmit.ContentDetails.ContentDescriptionImage = mediaObj.ObjectValue ?? string.Empty;
                                                quizAnswerSubmit.ContentDetails.PublicIdForContentDescription = mediaObj.ObjectPublicId ?? string.Empty;

                                                var newMedia = ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers);
                                                if (newMedia != null)
                                                {
                                                    quizAnswerSubmit.ContentDetails.ContentDescriptionImage = newMedia.MediaUrl;
                                                    quizAnswerSubmit.ContentDetails.PublicIdForContentDescription = newMedia.MediaPublicId;
                                                }
                                            }
                                            else
                                            {
                                                quizAnswerSubmit.ContentDetails.ContentDescriptionImage = ((Db.QuestionsInQuiz)nextQuestionObj).DescriptionImage ?? string.Empty;
                                                quizAnswerSubmit.ContentDetails.PublicIdForContentDescription = ((Db.QuestionsInQuiz)nextQuestionObj).PublicIdForDescription ?? string.Empty;
                                            }

                                            quizAnswerSubmit.ContentDetails.ShowContentDescriptionImage = ((Db.QuestionsInQuiz)nextQuestionObj).ShowDescriptionImage.HasValue && ((Db.QuestionsInQuiz)nextQuestionObj).ShowDescriptionImage.Value ? ((Db.QuestionsInQuiz)nextQuestionObj).ShowDescriptionImage.Value : false;
                                            quizAnswerSubmit.ContentDetails.ViewPreviousQuestion = ((Db.QuestionsInQuiz)nextQuestionObj).ViewPreviousQuestion;
                                            quizAnswerSubmit.ContentDetails.AutoPlay = ((Db.QuestionsInQuiz)nextQuestionObj).AutoPlay;
                                            quizAnswerSubmit.ContentDetails.SecondsToApply = ((Db.QuestionsInQuiz)nextQuestionObj).SecondsToApply ?? "0";
                                            quizAnswerSubmit.ContentDetails.VideoFrameEnabled = ((Db.QuestionsInQuiz)nextQuestionObj).VideoFrameEnabled ?? false;
                                            quizAnswerSubmit.ContentDetails.AutoPlayForDescription = ((Db.QuestionsInQuiz)nextQuestionObj).AutoPlayForDescription;
                                            quizAnswerSubmit.ContentDetails.SecondsToApplyForDescription = ((Db.QuestionsInQuiz)nextQuestionObj).SecondsToApplyForDescription ?? "0";
                                            quizAnswerSubmit.ContentDetails.DescVideoFrameEnabled = ((Db.QuestionsInQuiz)nextQuestionObj).DescVideoFrameEnabled ?? false;
                                            quizAnswerSubmit.ContentDetails.ShowTitle = ((Db.QuestionsInQuiz)nextQuestionObj).ShowTitle;
                                            quizAnswerSubmit.ContentDetails.DisplayOrderForTitle = ((Db.QuestionsInQuiz)nextQuestionObj).DisplayOrderForTitle;
                                            quizAnswerSubmit.ContentDetails.DisplayOrderForTitleImage = ((Db.QuestionsInQuiz)nextQuestionObj).DisplayOrderForTitleImage;
                                            quizAnswerSubmit.ContentDetails.DisplayOrderForDescription = ((Db.QuestionsInQuiz)nextQuestionObj).DisplayOrderForDescription;
                                            quizAnswerSubmit.ContentDetails.DisplayOrderForDescriptionImage = ((Db.QuestionsInQuiz)nextQuestionObj).DisplayOrderForDescriptionImage;
                                            quizAnswerSubmit.ContentDetails.DisplayOrderForNextButton = ((Db.QuestionsInQuiz)nextQuestionObj).DisplayOrderForNextButton;
                                        }
                                    }

                                    #endregion

                                    #region if next is result type

                                    if (nextQuestionObj.GetType().BaseType.Name == "QuizResults")
                                    {
                                        quizAnswerSubmit.QuestionType = (int)BranchingLogicEnum.RESULT;
                                        var attemptedQuestions = quizAttemptsObj.QuizQuestionStats.Where(t => t.Status == (int)StatusEnum.Active && t.CompletedOn != null && (t.QuestionsInQuiz.TimerRequired ? true : t.QuizAnswerStats.Any(r => !r.AnswerOptionsInQuizQuestions.IsUnansweredType)));
                                        var result = ((Db.QuizResults)nextQuestionObj);
                                        quizAnswerSubmit.ShowLeadUserForm = (Mode == "PREVIEW" || Mode == "PREVIEWTEMPLATE") ? false : (resultIdList.Any() ? (resultIdList.Any(r => r == result.Id || quizComponentLogsList.Any(q => q.DraftedObjectId == r && q.PublishedObjectId == result.Id))) : (quizAttemptsObj.LeadUserId == null && quizAttemptsObj.RecruiterUserId == null && ((quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Personality && quizDetails.PersonalityResultSetting.FirstOrDefault().Status == (int)StatusEnum.Active) ? quizDetails.PersonalityResultSetting.FirstOrDefault().ShowLeadUserForm : result.ShowLeadUserForm)));
                                        quizAnswerSubmit.CompanyCode = quizDetails.Quiz.Company.ClientCode;

                                        if (leadFormDetailofResultsLst != null && leadFormDetailofResultsLst.Any() && quizAnswerSubmit.ShowLeadUserForm)
                                        {
                                            var leadFormDetailofResultsObj = leadFormDetailofResultsLst.FirstOrDefault(r => r.ResultId == result.Id || quizComponentLogsList.Any(q => q.DraftedObjectId == r.ResultId && q.PublishedObjectId == result.Id));
                                            quizAnswerSubmit.FormId = leadFormDetailofResultsObj.FormId;
                                            quizAnswerSubmit.FlowOrder = leadFormDetailofResultsObj.FlowOrder;
                                        }

                                        if (quizDetails.Quiz.QuizType != (int)QuizTypeEnum.Personality && quizDetails.Quiz.QuizType != (int)QuizTypeEnum.PersonalityTemplate)
                                        {
                                            #region non personality result Setting

                                            quizAnswerSubmit.ResultScore = new QuizAnswerSubmit.Result();

                                            if (result.EnableMediaFile && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.RESULT && r.ObjectId == result.Id))
                                            {
                                                var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.RESULT && r.ObjectId == result.Id);
                                                quizAnswerSubmit.ResultScore.Image = result.ShowResultImage ? mediaObj.ObjectValue : string.Empty;
                                                quizAnswerSubmit.ResultScore.PublicIdForResult = result.ShowResultImage ? mediaObj.ObjectPublicId : string.Empty;

                                                var newMedia = result.ShowResultImage ? ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers) : null;
                                                if (newMedia != null)
                                                {
                                                    quizAnswerSubmit.ResultScore.Image = newMedia.MediaUrl;
                                                    quizAnswerSubmit.ResultScore.PublicIdForResult = newMedia.MediaPublicId;
                                                }
                                            }
                                            else
                                            {
                                                quizAnswerSubmit.ResultScore.Image = result.ShowResultImage ? result.Image : string.Empty;
                                                quizAnswerSubmit.ResultScore.PublicIdForResult = result.ShowResultImage ? result.PublicId : string.Empty;
                                            }
                                            quizAnswerSubmit.ResultScore.HideActionButton = result.HideCallToAction.HasValue ? result.HideCallToAction.Value : false;
                                            quizAnswerSubmit.ResultScore.ActionButtonURL = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonURL;
                                            quizAnswerSubmit.ResultScore.OpenLinkInNewTab = quizAnswerSubmit.ResultScore.HideActionButton ? false : result.OpenLinkInNewTab;
                                            quizAnswerSubmit.ResultScore.ActionButtonTxtSize = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonTxtSize;
                                            quizAnswerSubmit.ResultScore.ActionButtonColor = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonColor;
                                            quizAnswerSubmit.ResultScore.ActionButtonTitleColor = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonTitleColor;
                                            quizAnswerSubmit.ResultScore.ActionButtonText = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonText;

                                            quizAnswerSubmit.ResultScore.HideSocialShareButtons = quizDetails.HideSocialShareButtons.HasValue && quizDetails.HideSocialShareButtons.Value ? quizDetails.HideSocialShareButtons.Value : false;
                                            quizAnswerSubmit.ResultScore.ShowFacebookBtn = quizAnswerSubmit.ResultScore.HideSocialShareButtons && quizDetails.EnableFacebookShare.HasValue ? quizDetails.EnableFacebookShare.Value : false;
                                            quizAnswerSubmit.ResultScore.ShowTwitterBtn = quizAnswerSubmit.ResultScore.HideSocialShareButtons && quizDetails.EnableTwitterShare.HasValue ? quizDetails.EnableTwitterShare.Value : false;
                                            quizAnswerSubmit.ResultScore.ShowLinkedinBtn = quizAnswerSubmit.ResultScore.HideSocialShareButtons && quizDetails.EnableLinkedinShare.HasValue ? quizDetails.EnableLinkedinShare.Value : false;
                                            quizAnswerSubmit.ResultScore.DisplayOrderForTitle = quizAnswerSubmit.ResultScore.DisplayOrderForTitle;
                                            quizAnswerSubmit.ResultScore.DisplayOrderForTitleImage = quizAnswerSubmit.ResultScore.DisplayOrderForTitleImage;
                                            quizAnswerSubmit.ResultScore.DisplayOrderForDescription = quizAnswerSubmit.ResultScore.DisplayOrderForDescription;
                                            quizAnswerSubmit.ResultScore.DisplayOrderForNextButton = quizAnswerSubmit.ResultScore.DisplayOrderForNextButton;

                                            var resultSetting = quizDetails.ResultSettings.FirstOrDefault();
                                            var correctAnsCount = 0;
                                            string scoreValueTxt = string.Empty;

                                            if (resultSetting != null)
                                            {
                                                if (resultSetting.ShowScoreValue.HasValue && resultSetting.ShowScoreValue.Value && (((quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate) && attemptedQuestions.Any(r => r.QuestionsInQuiz.Status == (int)StatusEnum.Active && (r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple))) || attemptedQuestions.Any(r => (r.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Any(t => t.Status == (int)StatusEnum.Active && t.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && t.IsCorrectForMultipleAnswer.HasValue && t.IsCorrectForMultipleAnswer.Value)) || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single)))
                                                {
                                                    scoreValueTxt = resultSetting.CustomTxtForScoreValueInResult;

                                                    if (quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                                        correctAnsCount = attemptedQuestions.Sum(a => a.QuizAnswerStats.Sum(x => x.AnswerOptionsInQuizQuestions.AssociatedScore)).Value;
                                                    else
                                                        correctAnsCount = attemptedQuestions.Count(a => (a.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && ((a.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Select(s => s.Id).OrderBy(s => s)).SequenceEqual(a.QuizAnswerStats.Select(r => r.AnswerId).OrderBy(s => s)))) || (a.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single && a.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer == true));

                                                    scoreValueTxt = scoreValueTxt == null ? string.Empty : scoreValueTxt.Replace("%score%", ' ' + correctAnsCount.ToString() + ' ').Replace("%total%", ' ' + attemptedQuestions.Count(t => (t.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Any(r => r.Status == (int)StatusEnum.Active && r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value)) || t.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single).ToString() + ' ');

                                                    quizAnswerSubmit.ResultScore.ResultScoreValueTxt = scoreValueTxt;
                                                    quizAnswerSubmit.ResultScore.ShowScoreValue = true;
                                                }
                                                else
                                                {
                                                    quizAnswerSubmit.ResultScore.ResultScoreValueTxt = string.Empty;
                                                    quizAnswerSubmit.ResultScore.ShowScoreValue = false;
                                                }

                                                scoreValueTxt = scoreValueTxt == null ? string.Empty : scoreValueTxt.Replace("%score%", ' ' + correctAnsCount.ToString() + ' ').Replace("%total%", ' ' + attemptedQuestions.Where(t => (t.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Any()) || t.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single).Count().ToString() + ' ');

                                                if (resultSetting.ShowCorrectAnswer.HasValue && resultSetting.ShowCorrectAnswer.Value)
                                                {
                                                    quizAnswerSubmit.ResultScore.AnswerKeyCustomTxt = resultSetting.CustomTxtForAnswerKey;
                                                    quizAnswerSubmit.ResultScore.YourAnswerCustomTxt = resultSetting.CustomTxtForYourAnswer;
                                                    quizAnswerSubmit.ResultScore.CorrectAnswerCustomTxt = resultSetting.CustomTxtForCorrectAnswer;
                                                    quizAnswerSubmit.ResultScore.ExplanationCustomTxt = resultSetting.CustomTxtForExplanation;
                                                    quizAnswerSubmit.ResultScore.ShowCorrectAnswer = true;

                                                    quizAnswerSubmit.ResultScore.AttemptedQuestionAnswerDetails = new List<QuizAnswerSubmit.AnswerResult>();

                                                    foreach (var item in attemptedQuestions.Where(r => r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Short || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Long))
                                                    {
                                                        var correctAnswerTxt = string.Empty;
                                                        int associatedScore = default(int);
                                                        bool? IsCorrectValue = null;
                                                        var yourAnswer = string.Empty;

                                                        if (item.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Any())
                                                        {
                                                            correctAnswerTxt = VariableLinking(string.Join(",", item.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Select(t => t.Option).Select(s => "'" + s + "'")), quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt);
                                                            if (quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                                                associatedScore = item.QuizAnswerStats.Sum(x => x.AnswerOptionsInQuizQuestions.AssociatedScore).Value;
                                                            else
                                                                IsCorrectValue = (item.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Select(s => s.Id).OrderBy(s => s)).SequenceEqual(item.QuizAnswerStats.Select(r => r.AnswerId).OrderBy(s => s));
                                                        }
                                                        else if (item.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single)
                                                        {
                                                            correctAnswerTxt = VariableLinking(item.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(t => t.IsCorrectAnswer.HasValue ? t.IsCorrectAnswer.Value : false).Select(t => t.Option).FirstOrDefault(), quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt);
                                                            if (quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                                                associatedScore = item.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.AssociatedScore.Value;
                                                            else
                                                                IsCorrectValue = item.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer.HasValue && item.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer.Value;
                                                        }

                                                        if (item.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple || item.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single)
                                                            yourAnswer = VariableLinking(string.Join(",", item.QuizAnswerStats.Where(t => !t.AnswerOptionsInQuizQuestions.IsUnansweredType).Select(t => t.AnswerOptionsInQuizQuestions.Option).Select(s => "'" + s + "'")), quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt);
                                                        else
                                                            yourAnswer = item.QuizAnswerStats.Select(t => t.AnswerText).FirstOrDefault();

                                                        quizAnswerSubmit.ResultScore.AttemptedQuestionAnswerDetails.Add(new QuizAnswerSubmit.AnswerResult
                                                        {
                                                            Question = VariableLinking(item.QuestionsInQuiz.Question, quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt),
                                                            YourAnswer = string.IsNullOrEmpty(yourAnswer) ? notAttemptedQuesText : yourAnswer,
                                                            CorrectAnswer = (quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate) ? string.Empty : correctAnswerTxt,
                                                            IsCorrect = IsCorrectValue,
                                                            AnswerExplanation = item.QuestionsInQuiz.CorrectAnswerDescription,
                                                            AssociatedScore = (quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate) ? associatedScore : default(int)
                                                        });
                                                    }
                                                }
                                                else
                                                {
                                                    quizAnswerSubmit.ResultScore.ShowCorrectAnswer = false;
                                                }
                                            }

                                            quizAnswerSubmit.ResultScore.ShowInternalTitle = result.ShowInternalTitle;
                                            quizAnswerSubmit.ResultScore.ShowExternalTitle = result.ShowExternalTitle;
                                            quizAnswerSubmit.ResultScore.ShowDescription = result.ShowDescription;
                                            quizAnswerSubmit.ResultScore.Title = VariableLinking(result.Title, quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt);
                                            quizAnswerSubmit.ResultScore.InternalTitle = VariableLinking(result.InternalTitle, quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt);
                                            quizAnswerSubmit.ResultScore.Description = VariableLinking(result.Description, quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt);
                                            quizAnswerSubmit.ResultScore.AutoPlay = result.AutoPlay;
                                            quizAnswerSubmit.ResultScore.SecondsToApply = result.SecondsToApply;
                                            quizAnswerSubmit.ResultScore.VideoFrameEnabled = result.VideoFrameEnabled;
                                            quizAnswerSubmit.ResultScore.DisplayOrderForTitle = result.DisplayOrderForTitle;
                                            quizAnswerSubmit.ResultScore.DisplayOrderForTitleImage = result.DisplayOrderForTitleImage;
                                            quizAnswerSubmit.ResultScore.DisplayOrderForDescription = result.DisplayOrderForDescription;
                                            quizAnswerSubmit.ResultScore.DisplayOrderForNextButton = result.DisplayOrderForNextButton;

                                            quizStatsObj = quizAttemptsObj.QuizStats.FirstOrDefault();

                                            if (quizStatsObj != null)
                                            {
                                                quizStatsObj.ResultId = result.Id;

                                                UOWObj.QuizStatsRepository.Update(quizStatsObj);
                                                UOWObj.Save();
                                            }
                                            #endregion
                                        }
                                        else
                                        {
                                            #region Personality result Setting
                                            var personalityAnswerResultList = attemptedQuestions.SelectMany(x => x.QuizAnswerStats.SelectMany(r => r.AnswerOptionsInQuizQuestions.PersonalityAnswerResultMapping));
                                            var personalitySetting = quizDetails.PersonalityResultSetting.FirstOrDefault();
                                            if (personalitySetting != null && personalitySetting.Status == (int)StatusEnum.Active)
                                            {
                                                #region personality Setting Active

                                                var personalityResultList = new List<QuizAnswerSubmit.PersonalityResult>();
                                                var countResult = 0;
                                                quizStatsObj = quizAttemptsObj.QuizStats.FirstOrDefault();

                                                if (personalityAnswerResultList.Any())
                                                {
                                                    #region correlation is available
                                                    var mappedResultList = personalityAnswerResultList.OrderBy(x => x.QuizResults.DisplayOrder).GroupBy(r => r.ResultId).OrderByDescending(i => i.Count()).Take(personalitySetting.MaxResult).Select(x => x.Key).ToList();

                                                    var quizresultList = quizDetails.QuizResults.Where(r => !r.IsPersonalityCorrelatedResult && mappedResultList.Contains(r.Id)).OrderBy(x => mappedResultList.IndexOf(x.Id));

                                                    foreach (var quizresult in quizresultList)
                                                    {
                                                        QuizAnswerSubmit.PersonalityResult personalityresult = new QuizAnswerSubmit.PersonalityResult();
                                                        personalityresult.Title = VariableLinking(quizresult.Title, quizDetails, quizAttemptsObj, false, false, null);
                                                        personalityresult.InternalTitle = VariableLinking(quizresult.InternalTitle, quizDetails, quizAttemptsObj, false, false, null);
                                                        personalityresult.Image = quizresult.Image;
                                                        personalityresult.ResultId = quizresult.Id;
                                                        personalityresult.GraphColor = personalitySetting.GraphColor;
                                                        personalityresult.ButtonColor = personalitySetting.ButtonColor;
                                                        personalityresult.ButtonFontColor = personalitySetting.ButtonFontColor;
                                                        personalityresult.MaxResult = personalitySetting.MaxResult;
                                                        personalityresult.SideButtonText = personalitySetting.SideButtonText;
                                                        personalityresult.IsFullWidthEnable = personalitySetting.IsFullWidthEnable;
                                                        personalityresult.Description = VariableLinking(quizresult.Description, quizDetails, quizAttemptsObj, false, false, null);
                                                        int count = personalityAnswerResultList.Where(k => k.ResultId == quizresult.Id).Count();
                                                        personalityresult.Percentage = (int)Math.Round((double)(100 * count) / personalityAnswerResultList.Count());
                                                        personalityresult.ShowInternalTitle = quizresult.ShowInternalTitle;
                                                        personalityresult.ShowExternalTitle = quizresult.ShowExternalTitle;
                                                        personalityresult.ShowDescription = quizresult.ShowDescription;
                                                        personalityResultList.Add(personalityresult);

                                                        if (quizStatsObj != null)
                                                        {
                                                            if (countResult == 0)
                                                            {
                                                                quizStatsObj.ResultId = quizresult.Id;
                                                                UOWObj.QuizStatsRepository.Update(quizStatsObj);
                                                            }
                                                            else
                                                            {
                                                                var resultQuizStats = new Db.QuizStats();
                                                                resultQuizStats.StartedOn = quizStatsObj.StartedOn;
                                                                resultQuizStats.QuizAttemptId = quizStatsObj.QuizAttemptId;
                                                                resultQuizStats.ResultId = quizresult.Id;
                                                                UOWObj.QuizStatsRepository.Insert(resultQuizStats);
                                                            }
                                                            UOWObj.Save();
                                                            countResult++;
                                                        }
                                                    }

                                                    quizAnswerSubmit.ShowLeadUserForm = (Mode == "PREVIEW" || Mode == "PREVIEWTEMPLATE") ? false : (resultIdList.Any() ? (resultIdList.Any(r => quizresultList.Any(l => l.Id == r) || quizComponentLogsList.Any(q => q.DraftedObjectId == r && quizresultList.Any(l => l.Id == q.PublishedObjectId)))) : (quizAttemptsObj.LeadUserId == null && quizAttemptsObj.RecruiterUserId == null && ((quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Personality && quizDetails.PersonalityResultSetting.FirstOrDefault().Status == (int)StatusEnum.Active) ? quizDetails.PersonalityResultSetting.FirstOrDefault().ShowLeadUserForm : quizresultList.Any(r => r.ShowLeadUserForm))));

                                                    if (leadFormDetailofResultsLst != null && leadFormDetailofResultsLst.Any() && quizAnswerSubmit.ShowLeadUserForm)
                                                    {
                                                        var personalityResult = personalityResultList.OrderByDescending(r => r.Percentage.Value).FirstOrDefault();
                                                        if (leadFormDetailofResultsLst.Any(r => r.ResultId == personalityResult.ResultId || quizComponentLogsList.Any(q => q.DraftedObjectId == r.ResultId && q.PublishedObjectId == personalityResult.ResultId)))
                                                        {
                                                            var leadFormDetailofResultsObj = leadFormDetailofResultsLst.FirstOrDefault(r => r.ResultId == personalityResult.ResultId || quizComponentLogsList.Any(q => q.DraftedObjectId == r.ResultId && q.PublishedObjectId == personalityResult.ResultId));
                                                            quizAnswerSubmit.FormId = leadFormDetailofResultsObj.FormId;
                                                            quizAnswerSubmit.FlowOrder = leadFormDetailofResultsObj.FlowOrder;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        quizAnswerSubmit.FormId = null;
                                                        quizAnswerSubmit.FlowOrder = null;
                                                    }

                                                    #endregion
                                                }
                                                else
                                                {
                                                    #region correlation is not available

                                                    var quizresultList = quizDetails.QuizResults.OrderBy(x => x.DisplayOrder).Take(personalitySetting.MaxResult);

                                                    foreach (var quizresult in quizresultList)
                                                    {
                                                        QuizAnswerSubmit.PersonalityResult personalityresult = new QuizAnswerSubmit.PersonalityResult();
                                                        personalityresult.Title = VariableLinking(quizresult.Title, quizDetails, quizAttemptsObj, false, false, null);
                                                        personalityresult.InternalTitle = VariableLinking(quizresult.InternalTitle, quizDetails, quizAttemptsObj, false, false, null);
                                                        personalityresult.Image = quizresult.Image;
                                                        personalityresult.ResultId = quizresult.Id;
                                                        personalityresult.GraphColor = personalitySetting.GraphColor;
                                                        personalityresult.ButtonColor = personalitySetting.ButtonColor;
                                                        personalityresult.ButtonFontColor = personalitySetting.ButtonFontColor;
                                                        personalityresult.MaxResult = personalitySetting.MaxResult;
                                                        personalityresult.SideButtonText = personalitySetting.SideButtonText;
                                                        personalityresult.IsFullWidthEnable = personalitySetting.IsFullWidthEnable;
                                                        personalityresult.Description = VariableLinking(quizresult.Description, quizDetails, quizAttemptsObj, false, false, null);
                                                        personalityresult.Percentage = null;
                                                        personalityresult.ShowInternalTitle = quizresult.ShowInternalTitle;
                                                        personalityresult.ShowExternalTitle = quizresult.ShowExternalTitle;
                                                        personalityresult.ShowDescription = quizresult.ShowDescription;
                                                        personalityResultList.Add(personalityresult);

                                                        if (quizStatsObj != null)
                                                        {
                                                            if (countResult == 0)
                                                            {
                                                                quizStatsObj.ResultId = quizresult.Id;
                                                                UOWObj.QuizStatsRepository.Update(quizStatsObj);
                                                            }
                                                            else
                                                            {
                                                                var resultQuizStats = new Db.QuizStats();
                                                                resultQuizStats.StartedOn = quizStatsObj.StartedOn;
                                                                resultQuizStats.QuizAttemptId = quizStatsObj.QuizAttemptId;
                                                                resultQuizStats.ResultId = quizresult.Id;
                                                                UOWObj.QuizStatsRepository.Insert(resultQuizStats);
                                                            }
                                                            UOWObj.Save();
                                                            countResult++;
                                                        }
                                                    }

                                                    quizAnswerSubmit.ShowLeadUserForm = (Mode == "PREVIEW" || Mode == "PREVIEWTEMPLATE") ? false : (resultIdList.Any() ? (resultIdList.Any(r => quizresultList.Any(l => l.Id == r) || quizComponentLogsList.Any(q => q.DraftedObjectId == r && quizresultList.Any(l => l.Id == q.PublishedObjectId)))) : (quizAttemptsObj.LeadUserId == null && quizAttemptsObj.RecruiterUserId == null && ((quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Personality && quizDetails.PersonalityResultSetting.FirstOrDefault().Status == (int)StatusEnum.Active) ? quizDetails.PersonalityResultSetting.FirstOrDefault().ShowLeadUserForm : quizresultList.Any(r => r.ShowLeadUserForm))));





                                                    if (leadFormDetailofResultsLst != null && leadFormDetailofResultsLst.Any() && quizAnswerSubmit.ShowLeadUserForm)
                                                    {
                                                        var personalityResult = personalityResultList.FirstOrDefault();
                                                        if (leadFormDetailofResultsLst.Any(r => r.ResultId == personalityResult.ResultId || quizComponentLogsList.Any(q => q.DraftedObjectId == r.ResultId && q.PublishedObjectId == personalityResult.ResultId)))
                                                        {
                                                            var leadFormDetailofResultsObj = leadFormDetailofResultsLst.FirstOrDefault(r => r.ResultId == personalityResult.ResultId || quizComponentLogsList.Any(q => q.DraftedObjectId == r.ResultId && q.PublishedObjectId == personalityResult.ResultId));
                                                            quizAnswerSubmit.FormId = leadFormDetailofResultsObj.FormId;
                                                            quizAnswerSubmit.FlowOrder = leadFormDetailofResultsObj.FlowOrder;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        quizAnswerSubmit.FormId = null;
                                                        quizAnswerSubmit.FlowOrder = null;
                                                    }

                                                    #endregion
                                                }
                                                quizAnswerSubmit.ResultScore = quizAnswerSubmit.ResultScore ?? new QuizAnswerSubmit.Result();
                                                quizAnswerSubmit.ResultScore.ShowInternalTitle = true;
                                                quizAnswerSubmit.ResultScore.ShowExternalTitle = true;
                                                quizAnswerSubmit.ResultScore.ShowDescription = true;
                                                quizAnswerSubmit.ResultScore.Title = personalitySetting.Title;
                                                quizAnswerSubmit.ResultScore.PersonalityResultList = personalityResultList.OrderByDescending(x => x.Percentage).ToList();
                                                #endregion
                                            }
                                            else
                                            {
                                                #region if there is correlation available but personalitySetting is disabled

                                                if (result.IsPersonalityCorrelatedResult && personalityAnswerResultList.Any())
                                                    result = quizDetails.QuizResults.FirstOrDefault(x => x.Id == personalityAnswerResultList.OrderBy(r => r.QuizResults.DisplayOrder).GroupBy(r => r.ResultId).OrderByDescending(i => i.Count()).First().Key);

                                                quizAnswerSubmit.ShowLeadUserForm = (Mode == "PREVIEW" || Mode == "PREVIEWTEMPLATE") ? false : (resultIdList.Any() ? (resultIdList.Any(r => r == result.Id || quizComponentLogsList.Any(q => q.DraftedObjectId == r && q.PublishedObjectId == result.Id))) : (quizAttemptsObj.LeadUserId == null && quizAttemptsObj.RecruiterUserId == null && ((quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Personality && quizDetails.PersonalityResultSetting.FirstOrDefault().Status == (int)StatusEnum.Active) ? quizDetails.PersonalityResultSetting.FirstOrDefault().ShowLeadUserForm : result.ShowLeadUserForm)));

                                                if (leadFormDetailofResultsLst != null && leadFormDetailofResultsLst.Any() && quizAnswerSubmit.ShowLeadUserForm)
                                                {
                                                    var leadFormDetailofResultsObj = leadFormDetailofResultsLst.FirstOrDefault(r => r.ResultId == result.Id || quizComponentLogsList.Any(q => q.DraftedObjectId == r.ResultId && q.PublishedObjectId == result.Id));
                                                    quizAnswerSubmit.FormId = leadFormDetailofResultsObj.FormId;
                                                    quizAnswerSubmit.FlowOrder = leadFormDetailofResultsObj.FlowOrder;
                                                }

                                                quizAnswerSubmit.ResultScore = new QuizAnswerSubmit.Result();

                                                if (result.EnableMediaFile && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.RESULT && r.ObjectId == result.Id))
                                                {
                                                    var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.RESULT && r.ObjectId == result.Id);
                                                    quizAnswerSubmit.ResultScore.Image = result.ShowResultImage ? mediaObj.ObjectValue : string.Empty;
                                                    quizAnswerSubmit.ResultScore.PublicIdForResult = result.ShowResultImage ? mediaObj.ObjectPublicId : string.Empty;

                                                    var newMedia = result.ShowResultImage ? ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers) : null;
                                                    if (newMedia != null)
                                                    {
                                                        quizAnswerSubmit.ResultScore.Image = newMedia.MediaUrl;
                                                        quizAnswerSubmit.ResultScore.PublicIdForResult = newMedia.MediaPublicId;
                                                    }
                                                }
                                                else
                                                {
                                                    quizAnswerSubmit.ResultScore.Image = result.ShowResultImage ? result.Image : string.Empty;
                                                    quizAnswerSubmit.ResultScore.PublicIdForResult = result.ShowResultImage ? result.PublicId : string.Empty;
                                                }
                                                quizAnswerSubmit.ResultScore.HideActionButton = result.HideCallToAction.HasValue ? result.HideCallToAction.Value : false;
                                                quizAnswerSubmit.ResultScore.ActionButtonURL = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonURL;
                                                quizAnswerSubmit.ResultScore.OpenLinkInNewTab = quizAnswerSubmit.ResultScore.HideActionButton ? false : result.OpenLinkInNewTab;
                                                quizAnswerSubmit.ResultScore.ActionButtonTxtSize = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonTxtSize;
                                                quizAnswerSubmit.ResultScore.ActionButtonColor = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonColor;
                                                quizAnswerSubmit.ResultScore.ActionButtonTitleColor = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonTitleColor;
                                                quizAnswerSubmit.ResultScore.ActionButtonText = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonText;

                                                quizAnswerSubmit.ResultScore.HideSocialShareButtons = quizDetails.HideSocialShareButtons.HasValue && quizDetails.HideSocialShareButtons.Value ? quizDetails.HideSocialShareButtons.Value : false;
                                                quizAnswerSubmit.ResultScore.ShowFacebookBtn = quizAnswerSubmit.ResultScore.HideSocialShareButtons && quizDetails.EnableFacebookShare.HasValue ? quizDetails.EnableFacebookShare.Value : false;
                                                quizAnswerSubmit.ResultScore.ShowTwitterBtn = quizAnswerSubmit.ResultScore.HideSocialShareButtons && quizDetails.EnableTwitterShare.HasValue ? quizDetails.EnableTwitterShare.Value : false;
                                                quizAnswerSubmit.ResultScore.ShowLinkedinBtn = quizAnswerSubmit.ResultScore.HideSocialShareButtons && quizDetails.EnableLinkedinShare.HasValue ? quizDetails.EnableLinkedinShare.Value : false;
                                                quizAnswerSubmit.ResultScore.DisplayOrderForTitle = quizAnswerSubmit.ResultScore.DisplayOrderForTitle;
                                                quizAnswerSubmit.ResultScore.DisplayOrderForTitleImage = quizAnswerSubmit.ResultScore.DisplayOrderForTitleImage;
                                                quizAnswerSubmit.ResultScore.DisplayOrderForDescription = quizAnswerSubmit.ResultScore.DisplayOrderForDescription;
                                                quizAnswerSubmit.ResultScore.DisplayOrderForNextButton = quizAnswerSubmit.ResultScore.DisplayOrderForNextButton;

                                                var resultSetting = quizDetails.ResultSettings.FirstOrDefault();
                                                if (resultSetting != null)
                                                {
                                                    quizAnswerSubmit.ResultScore.ResultScoreValueTxt = string.Empty;
                                                    quizAnswerSubmit.ResultScore.ShowScoreValue = false;

                                                    if (resultSetting.ShowCorrectAnswer.HasValue && resultSetting.ShowCorrectAnswer.Value)
                                                    {
                                                        quizAnswerSubmit.ResultScore.AnswerKeyCustomTxt = resultSetting.CustomTxtForAnswerKey;
                                                        quizAnswerSubmit.ResultScore.YourAnswerCustomTxt = resultSetting.CustomTxtForYourAnswer;
                                                        quizAnswerSubmit.ResultScore.CorrectAnswerCustomTxt = resultSetting.CustomTxtForCorrectAnswer;
                                                        quizAnswerSubmit.ResultScore.ExplanationCustomTxt = resultSetting.CustomTxtForExplanation;
                                                        quizAnswerSubmit.ResultScore.ShowCorrectAnswer = true;
                                                        quizAnswerSubmit.ResultScore.AttemptedQuestionAnswerDetails = new List<QuizAnswerSubmit.AnswerResult>();

                                                        foreach (var item in attemptedQuestions.Where(r => r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single))
                                                        {
                                                            quizAnswerSubmit.ResultScore.AttemptedQuestionAnswerDetails.Add(new QuizAnswerSubmit.AnswerResult
                                                            {
                                                                Question = VariableLinking(item.QuestionsInQuiz.Question, quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, string.Empty),
                                                                YourAnswer = (item.QuizAnswerStats != null && item.QuizAnswerStats.Any(r => !r.AnswerOptionsInQuizQuestions.IsUnansweredType)) ? VariableLinking(string.Join(",", item.QuizAnswerStats.Where(t => !t.AnswerOptionsInQuizQuestions.IsUnansweredType).Select(t => t.AnswerOptionsInQuizQuestions.Option).Select(s => "'" + s + "'")), quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, string.Empty) : notAttemptedQuesText,
                                                                CorrectAnswer = string.Empty,
                                                                IsCorrect = null,
                                                                AnswerExplanation = item.QuestionsInQuiz.CorrectAnswerDescription,
                                                                AssociatedScore = default(int)
                                                            });
                                                        }
                                                    }
                                                    else
                                                    {
                                                        quizAnswerSubmit.ResultScore.ShowCorrectAnswer = false;
                                                    }
                                                }

                                                quizAnswerSubmit.ResultScore.ShowInternalTitle = result.ShowInternalTitle;
                                                quizAnswerSubmit.ResultScore.ShowExternalTitle = result.ShowExternalTitle;
                                                quizAnswerSubmit.ResultScore.ShowDescription = result.ShowDescription;
                                                quizAnswerSubmit.ResultScore.Title = VariableLinking(result.Title, quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, string.Empty);
                                                quizAnswerSubmit.ResultScore.InternalTitle = VariableLinking(result.InternalTitle, quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, string.Empty); ;
                                                quizAnswerSubmit.ResultScore.Description = VariableLinking(result.Description, quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, string.Empty);
                                                quizAnswerSubmit.ResultScore.AutoPlay = result.AutoPlay;
                                                quizAnswerSubmit.ResultScore.SecondsToApply = result.SecondsToApply;
                                                quizAnswerSubmit.ResultScore.VideoFrameEnabled = result.VideoFrameEnabled;
                                                quizAnswerSubmit.ResultScore.DisplayOrderForTitle = result.DisplayOrderForTitle;
                                                quizAnswerSubmit.ResultScore.DisplayOrderForTitleImage = result.DisplayOrderForTitleImage;
                                                quizAnswerSubmit.ResultScore.DisplayOrderForDescription = result.DisplayOrderForDescription;
                                                quizAnswerSubmit.ResultScore.DisplayOrderForNextButton = result.DisplayOrderForNextButton;

                                                quizStatsObj = quizAttemptsObj.QuizStats.FirstOrDefault();

                                                if (quizStatsObj != null)
                                                {
                                                    quizStatsObj.ResultId = result.Id;
                                                    UOWObj.QuizStatsRepository.Update(quizStatsObj);
                                                    UOWObj.Save();
                                                }
                                                #endregion
                                            }

                                            #endregion
                                        }

                                        var nextObj = FetchNextQuestion(quizDetails, result.Id, (int)BranchingLogicEnum.RESULTNEXT, UOWObj, isQuesAndContentInSameTable);

                                        #region if next is Badge type

                                        if (nextObj != null && nextObj.GetType().BaseType.Name == "BadgesInQuiz")
                                        {
                                            quizAnswerSubmit.BadgeDetails = new QuizBadge();

                                            quizAnswerSubmit.BadgeDetails.Id = ((Db.BadgesInQuiz)nextObj).Id;
                                            quizAnswerSubmit.BadgeDetails.Title = VariableLinking(((Db.BadgesInQuiz)nextObj).Title, quizDetails, quizAttemptsObj, false, false, null);
                                            quizAnswerSubmit.BadgeDetails.ShowTitle = ((Db.BadgesInQuiz)nextObj).ShowTitle;
                                            quizAnswerSubmit.BadgeDetails.AutoPlay = ((Db.BadgesInQuiz)nextObj).AutoPlay;
                                            quizAnswerSubmit.BadgeDetails.SecondsToApply = ((Db.BadgesInQuiz)nextObj).SecondsToApply;
                                            quizAnswerSubmit.BadgeDetails.VideoFrameEnabled = ((Db.BadgesInQuiz)nextObj).VideoFrameEnabled;
                                            quizAnswerSubmit.BadgeDetails.DisplayOrderForTitleImage = ((Db.BadgesInQuiz)nextObj).DisplayOrderForTitleImage;
                                            quizAnswerSubmit.BadgeDetails.DisplayOrderForTitle = ((Db.BadgesInQuiz)nextObj).DisplayOrderForTitle;
                                            quizAnswerSubmit.BadgeDetails.ShowImage = ((Db.BadgesInQuiz)nextObj).ShowImage;

                                            if (((Db.BadgesInQuiz)nextObj).EnableMediaFile && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.BADGE && r.ObjectId == ((Db.BadgesInQuiz)nextObj).Id))
                                            {
                                                var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.BADGE && r.ObjectId == ((Db.BadgesInQuiz)nextObj).Id);
                                                quizAnswerSubmit.BadgeDetails.Image = mediaObj.ObjectValue;
                                                quizAnswerSubmit.BadgeDetails.PublicIdForBadge = mediaObj.ObjectPublicId ?? string.Empty;

                                                var newMedia = ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers);
                                                if (newMedia != null)
                                                {
                                                    quizAnswerSubmit.BadgeDetails.Image = newMedia.MediaUrl;
                                                    quizAnswerSubmit.BadgeDetails.PublicIdForBadge = newMedia.MediaPublicId;
                                                }
                                            }
                                            else
                                            {
                                                quizAnswerSubmit.BadgeDetails.Image = ((Db.BadgesInQuiz)nextObj).Image ?? string.Empty;
                                                quizAnswerSubmit.BadgeDetails.PublicIdForBadge = ((Db.BadgesInQuiz)nextObj).PublicId ?? string.Empty;
                                            }

                                            var BadgeStatsObj = new Db.QuizObjectStats();

                                            BadgeStatsObj.QuizAttemptId = quizAttemptsObj.Id;
                                            BadgeStatsObj.ObjectId = ((Db.BadgesInQuiz)nextObj).Id;
                                            BadgeStatsObj.ViewedOn = currentDate;
                                            BadgeStatsObj.TypeId = (int)BranchingLogicEnum.BADGE;
                                            BadgeStatsObj.Status = (int)StatusEnum.Active;

                                            UOWObj.QuizObjectStatsRepository.Insert(BadgeStatsObj);
                                            UOWObj.Save();

                                            if (quizAttemptsObj.RecruiterUserId.HasValue && quizDetails.Quiz.Company.BadgesEnabled)
                                            {
                                                var badgesInfo = badgesInfoUpdateJson.Replace("{UserId}", quizAttemptsObj.RecruiterUserId.Value.ToString()).Replace("{CourseId}", quizDetails.ParentQuizId.ToString()).Replace("{CourseBadgeName}", ((Db.BadgesInQuiz)nextObj).Title).Replace("{CourseBadgeImageUrl}", quizAnswerSubmit.BadgeDetails.Image).Replace("{CourseTitle}", quizDetails.QuizTitle);
                                                var user = UOWObj.UserTokensRepository.Get(r => r.BusinessUserId == quizAttemptsObj.RecruiterUserId.Value).FirstOrDefault();
                                                var company = quizAttemptsObj.CompanyId > 0 ? quizAttemptsObj.Company : user.Company;
                                                try
                                                {
                                                    var apiSuccess = OWCHelper.UpdateRecruiterCourseBadgesInfo(badgesInfo, company);
                                                    if (!apiSuccess)
                                                        AddPendingApi(company.JobRocketApiUrl + (GlobalSettings.owcAddRecruiterCourseUrl.ToString()).Replace("{ClientCode}", company.ClientCode), company.JobRocketApiAuthorizationBearer, badgesInfo, "POST");
                                                }
                                                catch (Exception ex)
                                                {
                                                    AddPendingApi(company.JobRocketApiUrl + (GlobalSettings.owcAddRecruiterCourseUrl.ToString()).Replace("{ClientCode}", company.ClientCode), company.JobRocketApiAuthorizationBearer, badgesInfo, "POST");
                                                }
                                            }
                                        }
                                        #endregion
                                    }
                                    #endregion

                                    #region if next is content type

                                    if (nextQuestionObj.GetType().BaseType.Name == "ContentsInQuiz")
                                    {
                                        quizAnswerSubmit.QuestionType = (int)BranchingLogicEnum.CONTENT;

                                        if (quizAttemptsObj.QuizObjectStats.Any(a => a.Status == (int)StatusEnum.Active && a.TypeId == (int)BranchingLogicEnum.CONTENT && a.ObjectId == ((Db.ContentsInQuiz)nextQuestionObj).Id))
                                        {
                                            QuestionId = ((Db.ContentsInQuiz)nextQuestionObj).Id;
                                            Type = "complete_content";
                                            goto switchcase;
                                        }

                                        quizAnswerSubmit.ContentDetails = new QuizContent();

                                        quizAnswerSubmit.ContentDetails.Id = ((Db.ContentsInQuiz)nextQuestionObj).Id;
                                        quizAnswerSubmit.ContentDetails.ContentTitle = VariableLinking(((Db.ContentsInQuiz)nextQuestionObj).ContentTitle, quizDetails, quizAttemptsObj, false, false, null);
                                        quizAnswerSubmit.ContentDetails.ContentDescription = VariableLinking(((Db.ContentsInQuiz)nextQuestionObj).ContentDescription, quizDetails, quizAttemptsObj, false, false, null);
                                        quizAnswerSubmit.ContentDetails.ShowDescription = ((Db.ContentsInQuiz)nextQuestionObj).ShowDescription;

                                        if ((((Db.ContentsInQuiz)nextQuestionObj).EnableMediaFileForTitle && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Title && r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == ((Db.ContentsInQuiz)nextQuestionObj).Id)))
                                        {
                                            var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Title && r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == ((Db.ContentsInQuiz)nextQuestionObj).Id);
                                            quizAnswerSubmit.ContentDetails.ContentTitleImage = mediaObj.ObjectValue;
                                            quizAnswerSubmit.ContentDetails.PublicIdForContentTitle = mediaObj.ObjectPublicId ?? string.Empty;

                                            var newMedia = ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers);
                                            if (newMedia != null)
                                            {
                                                quizAnswerSubmit.ContentDetails.ContentTitleImage = newMedia.MediaUrl;
                                                quizAnswerSubmit.ContentDetails.PublicIdForContentTitle = newMedia.MediaPublicId;
                                            }
                                        }
                                        else
                                        {
                                            quizAnswerSubmit.ContentDetails.ContentTitleImage = ((Db.ContentsInQuiz)nextQuestionObj).ContentTitleImage ?? string.Empty;
                                            quizAnswerSubmit.ContentDetails.PublicIdForContentTitle = ((Db.ContentsInQuiz)nextQuestionObj).PublicIdForContentTitle ?? string.Empty;
                                        }

                                        quizAnswerSubmit.ContentDetails.ShowContentTitleImage = ((Db.ContentsInQuiz)nextQuestionObj).ShowContentTitleImage.HasValue && ((Db.ContentsInQuiz)nextQuestionObj).ShowContentTitleImage.Value ? ((Db.ContentsInQuiz)nextQuestionObj).ShowContentTitleImage.Value : false;
                                        quizAnswerSubmit.ContentDetails.AliasTextForNextButton = ((Db.ContentsInQuiz)nextQuestionObj).AliasTextForNextButton;
                                        quizAnswerSubmit.ContentDetails.EnableNextButton = ((Db.ContentsInQuiz)nextQuestionObj).EnableNextButton;

                                        if (((Db.ContentsInQuiz)nextQuestionObj).EnableMediaFileForDescription && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Description && r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == ((Db.ContentsInQuiz)nextQuestionObj).Id))
                                        {
                                            var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Description && r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == ((Db.ContentsInQuiz)nextQuestionObj).Id);
                                            quizAnswerSubmit.ContentDetails.ContentDescriptionImage = mediaObj.ObjectValue ?? string.Empty;
                                            quizAnswerSubmit.ContentDetails.PublicIdForContentDescription = mediaObj.ObjectPublicId ?? string.Empty;

                                            var newMedia = ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers);
                                            if (newMedia != null)
                                            {
                                                quizAnswerSubmit.ContentDetails.ContentDescriptionImage = newMedia.MediaUrl;
                                                quizAnswerSubmit.ContentDetails.PublicIdForContentDescription = newMedia.MediaPublicId;
                                            }
                                        }
                                        else
                                        {
                                            quizAnswerSubmit.ContentDetails.ContentDescriptionImage = ((Db.ContentsInQuiz)nextQuestionObj).ContentDescriptionImage ?? string.Empty;
                                            quizAnswerSubmit.ContentDetails.PublicIdForContentDescription = ((Db.ContentsInQuiz)nextQuestionObj).PublicIdForContentDescription ?? string.Empty;
                                        }

                                        quizAnswerSubmit.ContentDetails.ShowContentDescriptionImage = ((Db.ContentsInQuiz)nextQuestionObj).ShowContentDescriptionImage.HasValue && ((Db.ContentsInQuiz)nextQuestionObj).ShowContentDescriptionImage.Value ? ((Db.ContentsInQuiz)nextQuestionObj).ShowContentDescriptionImage.Value : false;
                                        quizAnswerSubmit.ContentDetails.ViewPreviousQuestion = ((Db.ContentsInQuiz)nextQuestionObj).ViewPreviousQuestion;
                                        quizAnswerSubmit.ContentDetails.AutoPlay = ((Db.ContentsInQuiz)nextQuestionObj).AutoPlay;
                                        quizAnswerSubmit.ContentDetails.SecondsToApply = ((Db.ContentsInQuiz)nextQuestionObj).SecondsToApply ?? "0";
                                        quizAnswerSubmit.ContentDetails.VideoFrameEnabled = ((Db.ContentsInQuiz)nextQuestionObj).VideoFrameEnabled ?? false;
                                        quizAnswerSubmit.ContentDetails.AutoPlayForDescription = ((Db.ContentsInQuiz)nextQuestionObj).AutoPlayForDescription;
                                        quizAnswerSubmit.ContentDetails.SecondsToApplyForDescription = ((Db.ContentsInQuiz)nextQuestionObj).SecondsToApplyForDescription ?? "0";
                                        quizAnswerSubmit.ContentDetails.DescVideoFrameEnabled = ((Db.ContentsInQuiz)nextQuestionObj).DescVideoFrameEnabled ?? false;
                                        quizAnswerSubmit.ContentDetails.ShowTitle = ((Db.ContentsInQuiz)nextQuestionObj).ShowTitle;
                                        quizAnswerSubmit.ContentDetails.DisplayOrderForTitle = ((Db.ContentsInQuiz)nextQuestionObj).DisplayOrderForTitle;
                                        quizAnswerSubmit.ContentDetails.DisplayOrderForTitleImage = ((Db.ContentsInQuiz)nextQuestionObj).DisplayOrderForTitleImage;
                                        quizAnswerSubmit.ContentDetails.DisplayOrderForDescription = ((Db.ContentsInQuiz)nextQuestionObj).DisplayOrderForDescription;
                                        quizAnswerSubmit.ContentDetails.DisplayOrderForDescriptionImage = ((Db.ContentsInQuiz)nextQuestionObj).DisplayOrderForDescriptionImage;
                                        quizAnswerSubmit.ContentDetails.DisplayOrderForNextButton = ((Db.ContentsInQuiz)nextQuestionObj).DisplayOrderForNextButton;
                                    }

                                    #endregion

                                    #region if next is Badge type

                                    if (nextQuestionObj.GetType().BaseType.Name == "BadgesInQuiz")
                                    {
                                        quizAnswerSubmit.QuestionType = (int)BranchingLogicEnum.BADGE;

                                        quizAnswerSubmit.BadgeDetails = new QuizBadge();

                                        quizAnswerSubmit.BadgeDetails.Id = ((Db.BadgesInQuiz)nextQuestionObj).Id;
                                        quizAnswerSubmit.BadgeDetails.Title = VariableLinking(((Db.BadgesInQuiz)nextQuestionObj).Title, quizDetails, quizAttemptsObj, false, false, null);
                                        quizAnswerSubmit.BadgeDetails.ShowTitle = ((Db.BadgesInQuiz)nextQuestionObj).ShowTitle;
                                        quizAnswerSubmit.BadgeDetails.AutoPlay = ((Db.BadgesInQuiz)nextQuestionObj).AutoPlay;
                                        quizAnswerSubmit.BadgeDetails.SecondsToApply = ((Db.BadgesInQuiz)nextQuestionObj).SecondsToApply;
                                        quizAnswerSubmit.BadgeDetails.VideoFrameEnabled = ((Db.BadgesInQuiz)nextQuestionObj).VideoFrameEnabled;
                                        quizAnswerSubmit.BadgeDetails.DisplayOrderForTitleImage = ((Db.BadgesInQuiz)nextQuestionObj).DisplayOrderForTitleImage;
                                        quizAnswerSubmit.BadgeDetails.DisplayOrderForTitle = ((Db.BadgesInQuiz)nextQuestionObj).DisplayOrderForTitle;
                                        quizAnswerSubmit.BadgeDetails.ShowImage = ((Db.BadgesInQuiz)nextQuestionObj).ShowImage;

                                        if (((Db.BadgesInQuiz)nextQuestionObj).EnableMediaFile && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.BADGE && r.ObjectId == ((Db.BadgesInQuiz)nextQuestionObj).Id))
                                        {
                                            var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.BADGE && r.ObjectId == ((Db.BadgesInQuiz)nextQuestionObj).Id);
                                            quizAnswerSubmit.BadgeDetails.Image = mediaObj.ObjectValue;
                                            quizAnswerSubmit.BadgeDetails.PublicIdForBadge = mediaObj.ObjectPublicId ?? string.Empty;

                                            var newMedia = ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers);
                                            if (newMedia != null)
                                            {
                                                quizAnswerSubmit.BadgeDetails.Image = newMedia.MediaUrl;
                                                quizAnswerSubmit.BadgeDetails.PublicIdForBadge = newMedia.MediaPublicId;
                                            }
                                        }
                                        else
                                        {
                                            quizAnswerSubmit.BadgeDetails.Image = ((Db.BadgesInQuiz)nextQuestionObj).Image ?? string.Empty;
                                            quizAnswerSubmit.BadgeDetails.PublicIdForBadge = ((Db.BadgesInQuiz)nextQuestionObj).PublicId ?? string.Empty;
                                        }

                                        if (quizAttemptsObj.RecruiterUserId.HasValue && quizDetails.Quiz.Company.BadgesEnabled)
                                        {
                                            var badgesInfo = badgesInfoUpdateJson.Replace("{UserId}", quizAttemptsObj.RecruiterUserId.Value.ToString()).Replace("{CourseId}", quizDetails.ParentQuizId.ToString()).Replace("{CourseBadgeName}", ((Db.BadgesInQuiz)nextQuestionObj).Title).Replace("{CourseBadgeImageUrl}", quizAnswerSubmit.BadgeDetails.Image).Replace("{CourseTitle}", quizDetails.QuizTitle);
                                            var user = UOWObj.UserTokensRepository.Get(r => r.BusinessUserId == quizAttemptsObj.RecruiterUserId.Value).FirstOrDefault();
                                            var company = quizAttemptsObj.CompanyId > 0 ? quizAttemptsObj.Company : user.Company;
                                            try
                                            {
                                                var apiSuccess = OWCHelper.UpdateRecruiterCourseBadgesInfo(badgesInfo, company);
                                                if (!apiSuccess)
                                                    AddPendingApi(company.JobRocketApiUrl + (GlobalSettings.owcAddRecruiterCourseUrl.ToString()).Replace("{ClientCode}", company.ClientCode), company.JobRocketApiAuthorizationBearer, badgesInfo, "POST");
                                            }
                                            catch (Exception ex)
                                            {
                                                AddPendingApi(company.JobRocketApiUrl + (GlobalSettings.owcAddRecruiterCourseUrl.ToString()).Replace("{ClientCode}", company.ClientCode), company.JobRocketApiAuthorizationBearer, badgesInfo, "POST");
                                            }
                                        }
                                    }

                                    #endregion
                                }

                                else
                                {
                                    #region update complete quiz status

                                    quizStatsObj = quizAttemptsObj.QuizStats.FirstOrDefault();

                                    if (quizStatsObj != null)
                                    {
                                        quizStatsObj.CompletedOn = currentDate;
                                        UOWObj.QuizStatsRepository.Update(quizStatsObj);
                                        UOWObj.Save();
                                    }

                                    #endregion
                                }

                                #endregion

                                break;

                            case "complete_quiz":

                                #region update complete quiz status

                                quizStatsObj = quizAttemptsObj.QuizStats.FirstOrDefault();

                                if (quizStatsObj != null)
                                {

                                    var resultSetting = quizDetails.ResultSettings.FirstOrDefault();

                                    var correctAnsCount = 0;
                                    var ShowScoreValue = false;
                                    string scoreValueTxt = string.Empty;
                                    if (resultSetting != null)
                                    {
                                        var attemptedQuestions = quizAttemptsObj.QuizQuestionStats.Where(t => t.Status == (int)StatusEnum.Active && t.CompletedOn != null && (t.QuestionsInQuiz.TimerRequired ? true : t.QuizAnswerStats.Any(r => !r.AnswerOptionsInQuizQuestions.IsUnansweredType)));

                                        if (resultSetting.ShowScoreValue.HasValue && resultSetting.ShowScoreValue.Value && (((quizObj.QuizType == (int)QuizTypeEnum.Score || quizObj.QuizType == (int)QuizTypeEnum.ScoreTemplate) && attemptedQuestions.Any(r => r.QuestionsInQuiz.Status == (int)StatusEnum.Active && (r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple))) || attemptedQuestions.Any(r => (r.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Any(t => t.Status == (int)StatusEnum.Active && t.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && t.IsCorrectForMultipleAnswer.HasValue && t.IsCorrectForMultipleAnswer.Value)) || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single)))
                                        {
                                            if (quizObj.QuizType == (int)QuizTypeEnum.Score || quizObj.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                                correctAnsCount = attemptedQuestions.Sum(a => a.QuizAnswerStats.Sum(x => x.AnswerOptionsInQuizQuestions.AssociatedScore)).Value;
                                            else
                                                correctAnsCount = attemptedQuestions.Count(a => (a.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && ((a.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Select(s => s.Id).OrderBy(s => s)).SequenceEqual(a.QuizAnswerStats.Select(r => r.AnswerId).OrderBy(s => s)))) || (a.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single && a.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer == true));

                                            ShowScoreValue = true;
                                            scoreValueTxt = scoreValueTxt == null ? string.Empty : scoreValueTxt.Replace("%score%", ' ' + correctAnsCount.ToString() + ' ').Replace("%total%", ' ' + attemptedQuestions.Count(t => (t.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Any(r => r.Status == (int)StatusEnum.Active && r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value)) || t.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single).ToString() + ' ');
                                        }
                                    }

                                    #region push work package or send mail in case of branching logic enable and result link to action

                                    if (Mode == "AUDIT" && (quizDetails.IsBranchingLogicEnabled.HasValue && quizDetails.IsBranchingLogicEnabled.Value) && (quizAttemptsObj.LeadUserId != null || (quizAttemptsObj.RecruiterUserId.HasValue && quizAttemptsObj.RecruiterUserId != null)))
                                    {
                                        if (quizDetails.BranchingLogic.FirstOrDefault(a => a.SourceObjectId == quizStatsObj.ResultId && (a.SourceTypeId == (int)BranchingLogicEnum.RESULT || a.SourceTypeId == (int)BranchingLogicEnum.RESULTNEXT) && a.DestinationTypeId == (int)BranchingLogicEnum.ACTION) != null)
                                        {
                                            var actionsInQuiz = quizDetails.ActionsInQuiz.FirstOrDefault(a => a.Id == quizDetails.BranchingLogic.FirstOrDefault(r => r.SourceObjectId == quizStatsObj.ResultId && (r.SourceTypeId == (int)BranchingLogicEnum.RESULT || r.SourceTypeId == (int)BranchingLogicEnum.RESULTNEXT) && r.DestinationTypeId == (int)BranchingLogicEnum.ACTION).DestinationObjectId);
                                            var leadDataInAction = (quizAttemptsObj.ConfigurationDetails != null)
                                                ? actionsInQuiz.LeadDataInAction.FirstOrDefault(t => t.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId)
                                                : actionsInQuiz.LeadDataInAction.FirstOrDefault(t => t.LeadUserId == quizAttemptsObj.LeadUserId);

                                            if (!string.IsNullOrEmpty(quizAttemptsObj.LeadUserId) && ((actionsInQuiz.ActionType == (int)ActionTypeEnum.Appointment && (actionsInQuiz.AppointmentId > 0 || (leadDataInAction != null && leadDataInAction.AppointmentTypeId > 0))) || (actionsInQuiz.ActionType == (int)ActionTypeEnum.LinkWithLeadDashboardAppointment && leadDataInAction != null && leadDataInAction.AppointmentTypeId > 0)))
                                            {
                                                var appointmentTypeId = actionsInQuiz.AppointmentId.Value;

                                                var sourceId = string.Empty;
                                                var sourceName = string.Empty;

                                                if (quizAttemptsObj.ConfigurationDetailsId > 0 && quizAttemptsObj.ConfigurationDetails.ConfigurationType == "UNKNOWN_LEADS")
                                                {
                                                    sourceId = quizAttemptsObj.ConfigurationDetails.SourceId ?? string.Empty;
                                                    sourceName = quizAttemptsObj.ConfigurationDetails.SourceName ?? string.Empty;
                                                }
                                                else
                                                {
                                                    sourceId = quizAttemptsObj.WorkPackageInfoId != null ? quizAttemptsObj.WorkPackageInfo.CampaignId : string.Empty;
                                                    sourceName = quizAttemptsObj.WorkPackageInfoId != null ? quizAttemptsObj.WorkPackageInfo.CampaignName : string.Empty;
                                                }

                                                var pushWorkPackageObj = new Helpers.Models.AppointmentWorkPackage()
                                                {
                                                    AppointmentTypeId = (leadDataInAction != null && leadDataInAction.AppointmentTypeId > 0) ? leadDataInAction.AppointmentTypeId.Value : appointmentTypeId,
                                                    BusinessUserId = BusinessUserId,
                                                    LeadUserId = quizAttemptsObj.LeadUserId,
                                                    CompanyCode = UOWObj.CompanyRepository.Get(r => r.Id == quizObj.CompanyId).FirstOrDefault().ClientCode,
                                                    SourceId = sourceId,
                                                    SourceName = sourceName,
                                                    CalendarIds = (leadDataInAction != null && leadDataInAction.AppointmentTypeId > 0)
                                                                  ? ((leadDataInAction.LeadCalendarDataInAction != null && leadDataInAction.LeadCalendarDataInAction.Any()) ? leadDataInAction.LeadCalendarDataInAction.Select(t => t.CalendarId).ToList() : new List<int>())
                                                                  : ((actionsInQuiz.LinkedCalendarInAction != null && actionsInQuiz.LinkedCalendarInAction.Any()) ? actionsInQuiz.LinkedCalendarInAction.Select(t => t.CalendarId).ToList() : new List<int>()),
                                                    IsUpdatedSend = (leadDataInAction != null && leadDataInAction.AppointmentTypeId > 0) ? leadDataInAction.IsUpdatedSend : false,
                                                    Body = (leadDataInAction != null && leadDataInAction.AppointmentTypeId > 0) ? leadDataInAction.Body : string.Empty,
                                                    Subject = (leadDataInAction != null && leadDataInAction.AppointmentTypeId > 0) ? leadDataInAction.Subject : string.Empty,
                                                    SMSText = (leadDataInAction != null && leadDataInAction.AppointmentTypeId > 0) ? leadDataInAction.SMSText : string.Empty
                                                };

                                                AppointmentHelper.PushWorkPackage(pushWorkPackageObj);

                                                #region send info to OWC

                                                SenqQuizinfotoOWC(quizAttemptsObj, companyObj, token, urlAppointment, leadDataInAction, appointmentTypeId, sourceId, sourceName);

                                                #endregion
                                            }
                                            else if (!string.IsNullOrEmpty(quizAttemptsObj.LeadUserId) && actionsInQuiz.AutomationId > 0)
                                            {
                                                WorkPackageService workPackageServiceObj = new WorkPackageService();
                                                WorkPackage workPackageObj = new WorkPackage();
                                                workPackageObj.LeadUserId = quizAttemptsObj.LeadUserId;
                                                workPackageObj.QuizId = actionsInQuiz.AutomationId.Value;
                                                workPackageObj.DynamicVariables = new Dictionary<string, string>();
                                                workPackageObj.LeadDataInActionList = new List<WorkPackage.LeadDataInActionModel>();
                                                try
                                                {
                                                    workPackageServiceObj.SaveWorkPackage(workPackageObj);
                                                }
                                                catch (Exception ex) { }
                                            }
                                            else if ((!string.IsNullOrEmpty(quizAttemptsObj.LeadUserId) || (quizAttemptsObj.RecruiterUserId.HasValue && quizAttemptsObj.RecruiterUserId != null)) && (actionsInQuiz.ActionType == (int)ActionTypeEnum.ReportEmail || actionsInQuiz.ActionType == (int)ActionTypeEnum.LinkWithLeadDashboardAppointment))
                                            {
                                                StringBuilder body = new StringBuilder("Hello,<br/>Please find the questions and answers below:<br/>");
                                                foreach (var ques in quizAttemptsObj.QuizQuestionStats.Where(t => t.Status == (int)StatusEnum.Active && t.CompletedOn != null && (t.QuestionsInQuiz.TimerRequired ? true : t.QuizAnswerStats.Any(r => !r.AnswerOptionsInQuizQuestions.IsUnansweredType))).ToList())
                                                {
                                                    body.Append("Question : " + VariableLinking(ques.QuestionsInQuiz.Question, quizDetails, quizAttemptsObj, false, ShowScoreValue, scoreValueTxt) + "<br/>");
                                                    if (ques.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single || ques.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple || ques.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.LookingforJobs || ques.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.DrivingLicense)
                                                        body.Append("Answer : " + ques.QuizAnswerStats != null && ques.QuizAnswerStats.Any() ? VariableLinking(string.Join(",", ques.QuizAnswerStats.Select(t => t.AnswerOptionsInQuizQuestions.Option).Select(s => "'" + s + "'")), quizDetails, quizAttemptsObj, false, ShowScoreValue, scoreValueTxt) : notAttemptedQuesText + "<br/>");
                                                    else if (ques.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.FullAddress)
                                                        body.Append("Answer : " + ques.QuizAnswerStats != null && ques.QuizAnswerStats.Any() ? string.Join(",", ques.QuizAnswerStats.Select(t => t.AnswerText).Select(s => "'" + s + "'")) : notAttemptedQuesText + "<br/>");
                                                    else
                                                        body.Append("Answer : " + ques.QuizAnswerStats != null && ques.QuizAnswerStats.Any() ? ques.QuizAnswerStats.FirstOrDefault().AnswerText : notAttemptedQuesText + "<br/>");
                                                }

                                                var reportEmails = (leadDataInAction != null && !string.IsNullOrEmpty(leadDataInAction.ReportEmails)) ? leadDataInAction.ReportEmails : quizDetails.ActionsInQuiz.FirstOrDefault(a => a.Id == quizDetails.BranchingLogic.FirstOrDefault(r => r.SourceObjectId == quizStatsObj.ResultId && (r.SourceTypeId == (int)BranchingLogicEnum.RESULT || r.SourceTypeId == (int)BranchingLogicEnum.RESULTNEXT) && r.DestinationTypeId == (int)BranchingLogicEnum.ACTION).DestinationObjectId).ReportEmails;
                                                if (!string.IsNullOrEmpty(quizAttemptsObj.LeadUserId))
                                                {
                                                    if (leadUserInfo == null)
                                                    { leadUserInfo = OWCHelper.GetLeadUserInfo(quizAttemptsObj.LeadUserId, companyObj); }
                                                    if (!string.IsNullOrEmpty(reportEmails) && leadUserInfo != null && !string.IsNullOrEmpty(leadUserInfo.contactId))
                                                        CommunicationHelper.SendMail(reportEmails, "Results of " + leadUserInfo.firstName + "for" + VariableLinking(quizDetails.QuizTitle, quizDetails, quizAttemptsObj, false, ShowScoreValue, scoreValueTxt), body.ToString());

                                                }


                                                else if (quizAttemptsObj.RecruiterUserId.HasValue && quizAttemptsObj.RecruiterUserId != null)
                                                {
                                                    if (companyObj.ClientCode != "JobRock" && companyObj.ClientCode != "HEMA")
                                                    {
                                                        long[] userids = { quizAttemptsObj.RecruiterUserId.Value };
                                                        var userList = OWCHelper.GetUserListOnUserId(userids, companyObj);
                                                        var response = (userList != null && userList.Any()) ? userList.FirstOrDefault() : null;

                                                        if (response != null && !string.IsNullOrEmpty(response.userName) && !string.IsNullOrEmpty(reportEmails))
                                                        {
                                                            CommunicationHelper.SendMail(reportEmails, "Results of " + response.firstName + "for" + VariableLinking(quizDetails.QuizTitle, quizDetails, quizAttemptsObj, false, ShowScoreValue, scoreValueTxt), body.ToString());
                                                        }
                                                    }

                                                }

                                            }
                                        }
                                    }

                                    #endregion

                                    foreach (var obj in quizAttemptsObj.QuizStats.ToList())
                                    {
                                        obj.CompletedOn = currentDate;
                                        UOWObj.QuizStatsRepository.Update(obj);
                                    }
                                    UOWObj.Save();

                                    var TagList = quizAttemptsObj.QuizQuestionStats.Where(r => r.Status == (int)StatusEnum.Active).SelectMany(a => a.QuizAnswerStats.SelectMany(b => b.AnswerOptionsInQuizQuestions.TagsInAnswer.Select(q => q.TagId).ToList()));

                                    if (!string.IsNullOrEmpty(quizAttemptsObj.LeadUserId) && TagList.Any())
                                    {
                                        LeadTags leadTags = new LeadTags();
                                        leadTags.LeadId = quizAttemptsObj.LeadUserId;
                                        leadTags.TagsIds = TagList.Distinct().ToArray();

                                        OWCHelper.SaveLeadTags(leadTags, companyObj);
                                    }

                                    if (quizDetails != null)
                                    {
                                        quizAnswerSubmit.QuizSocialShare = new QuizSocialShareSetting();

                                        quizAnswerSubmit.QuizSocialShare.QuizId = quizDetails.ParentQuizId;
                                        quizAnswerSubmit.QuizSocialShare.HideSocialShareButtons = quizDetails.HideSocialShareButtons;
                                        quizAnswerSubmit.QuizSocialShare.EnableFacebookShare = quizDetails.EnableFacebookShare;
                                        quizAnswerSubmit.QuizSocialShare.EnableTwitterShare = quizDetails.EnableTwitterShare;
                                        quizAnswerSubmit.QuizSocialShare.EnableLinkedinShare = quizDetails.EnableLinkedinShare;
                                    }

                                    if (!string.IsNullOrEmpty(quizAttemptsObj.LeadUserId))
                                    {
                                        var UpdateQuizStatusObj = new LeadQuizStatus();

                                        UpdateQuizStatusObj.AutomationTitle = quizDetails.QuizTitle;
                                        UpdateQuizStatusObj.CreatedDate = quizAttemptsObj.WorkPackageInfoId != null ? quizAttemptsObj.WorkPackageInfo.CreatedOn.Value.ToString("yyyy-MM-dd'T'HH:mm:ss") : quizAttemptsObj.CreatedOn.ToString("yyyy-MM-dd'T'HH:mm:ss");
                                        UpdateQuizStatusObj.StartedDate = (quizAttemptsObj.QuizStats.Any() && quizAttemptsObj.QuizStats.FirstOrDefault().StartedOn != null) ? quizAttemptsObj.QuizStats.FirstOrDefault().StartedOn.ToString("yyyy-MM-dd'T'HH:mm:ss") : string.Empty;
                                        UpdateQuizStatusObj.AttemptDate = (quizAttemptsObj.QuizStats.Any() && quizAttemptsObj.QuizStats.FirstOrDefault().CompletedOn != null) ? quizAttemptsObj.QuizStats.FirstOrDefault().CompletedOn.Value.ToString("yyyy-MM-dd'T'HH:mm:ss") : string.Empty;
                                        UpdateQuizStatusObj.SourceId = quizAttemptsObj.ConfigurationDetailsId != null ? quizAttemptsObj.ConfigurationDetails.SourceId : string.Empty;
                                        UpdateQuizStatusObj.ContactId = quizAttemptsObj.LeadUserId;
                                        UpdateQuizStatusObj.ClientCode = quizObj.CompanyId != null ? companyObj.ClientCode : string.Empty;
                                        UpdateQuizStatusObj.QuizId = quizDetails.ParentQuizId;
                                        UpdateQuizStatusObj.QuizStatus = "3";
                                        UpdateQuizStatusObj.ConfigurationId = quizAttemptsObj.ConfigurationDetails != null ? quizAttemptsObj.ConfigurationDetails.ConfigurationId : string.Empty;
                                        UpdateQuizStatusObj.QuizType = quizObj.QuizType;
                                        UpdateQuizStatusObj.Results = new List<Result>();
                                        UpdateQuizStatusObj.FieldsToUpdate = new List<Fields>();

                                        foreach (var resultObj in quizAttemptsObj.QuizStats.Where(r => r.ResultId != null).ToList())
                                        {
                                            UpdateQuizStatusObj.Results.Add(new Result
                                            {
                                                ParentResultId = quizComponentLogsList.Any(r => r.PublishedObjectId == resultObj.ResultId.Value) ? quizComponentLogsList.FirstOrDefault(r => r.PublishedObjectId == resultObj.ResultId.Value).DraftedObjectId : 0,
                                                ResultId = resultObj.ResultId.HasValue ? resultObj.ResultId.Value : 0,
                                                ResultTitle = resultObj.ResultId.HasValue ? (!string.IsNullOrWhiteSpace(resultObj.QuizResults.InternalTitle) ? resultObj.QuizResults.InternalTitle : resultObj.QuizResults.Title) : string.Empty
                                            });
                                        }

                                        foreach (var questionObj in quizAttemptsObj.QuizQuestionStats.ToList())
                                        {
                                            var ansType = questionObj.QuestionsInQuiz.AnswerType;

                                            if (ansType == (int)AnswerTypeEnum.FullAddress)
                                            {
                                                var answerObjForPostCode = questionObj.QuizAnswerStats.FirstOrDefault(r => r.SubAnswerTypeId == (int)SubAnswerTypeEnum.PostCode);

                                                if (answerObjForPostCode.AnswerOptionsInQuizQuestions.ObjectFieldsInAnswer.Any())
                                                {
                                                    var objectFieldsInAnswer = answerObjForPostCode.AnswerOptionsInQuizQuestions.ObjectFieldsInAnswer.FirstOrDefault();
                                                    UpdateQuizStatusObj.FieldsToUpdate.Add(new Fields
                                                    {
                                                        ObjectName = objectFieldsInAnswer.ObjectName,
                                                        FieldName = objectFieldsInAnswer.FieldName,
                                                        Value = answerObjForPostCode.AnswerText
                                                    });
                                                }

                                                var answerObjForHouseNumber = questionObj.QuizAnswerStats.FirstOrDefault(r => r.SubAnswerTypeId == (int)SubAnswerTypeEnum.HouseNumber);

                                                if (answerObjForHouseNumber.AnswerOptionsInQuizQuestions.ObjectFieldsInAnswer.Any())
                                                {
                                                    var objectFieldsInAnswer = answerObjForHouseNumber.AnswerOptionsInQuizQuestions.ObjectFieldsInAnswer.FirstOrDefault();
                                                    UpdateQuizStatusObj.FieldsToUpdate.Add(new Fields
                                                    {
                                                        ObjectName = objectFieldsInAnswer.ObjectName,
                                                        FieldName = objectFieldsInAnswer.FieldName,
                                                        Value = string.Join(",", questionObj.QuizAnswerStats.Where(r => r.SubAnswerTypeId == (int)SubAnswerTypeEnum.HouseNumber || r.SubAnswerTypeId == (int)SubAnswerTypeEnum.Address).Select(r => r.AnswerText))
                                                    });
                                                }
                                            }
                                            else
                                            {
                                                foreach (var answerObj in questionObj.QuizAnswerStats.ToList())
                                                {
                                                    if (answerObj.AnswerOptionsInQuizQuestions.ObjectFieldsInAnswer.Any())
                                                    {
                                                        var objectFieldsInAnswer = answerObj.AnswerOptionsInQuizQuestions.ObjectFieldsInAnswer.FirstOrDefault();
                                                        UpdateQuizStatusObj.FieldsToUpdate.Add(new Fields
                                                        {
                                                            ObjectName = objectFieldsInAnswer.ObjectName,
                                                            FieldName = objectFieldsInAnswer.FieldName,
                                                            Value = (ansType == (int)AnswerTypeEnum.Single || ansType == (int)AnswerTypeEnum.Multiple || ansType == (int)AnswerTypeEnum.DrivingLicense || ansType == (int)AnswerTypeEnum.LookingforJobs)
                                                            ? objectFieldsInAnswer.Value : answerObj.AnswerText
                                                        });
                                                    }
                                                }
                                            }
                                        }

                                        try
                                        {
                                            var apiSuccess = false;
                                            var res = OWCHelper.UpdateQuizStatus(UpdateQuizStatusObj);
                                            if (res != null)
                                            {
                                                apiSuccess = res.status == "true";
                                                quizAnswerSubmit.AppointmentCode = res.appointmentCode;
                                                quizAnswerSubmit.AppointmentLink = res.appointmentLink;
                                                quizAnswerSubmit.AppointmentErrorMessage = res.message;
                                            }

                                            if (!apiSuccess)
                                                AddPendingApi(urlAutomation, token, JsonConvert.SerializeObject(UpdateQuizStatusObj), "POST");
                                        }
                                        catch (Exception)
                                        {
                                            AddPendingApi(urlAutomation, token, JsonConvert.SerializeObject(UpdateQuizStatusObj), "POST");
                                        }
                                    }
                                }

                                #endregion

                                break;

                            case "start_question":

                                quizAnswerSubmit.QuestionType = (int)BranchingLogicEnum.QUESTION;

                                #region start quiz question

                                quizQuestionStatsObj = new Db.QuizQuestionStats();

                                quizQuestionStatsObj.QuizAttemptId = quizAttemptsObj.Id;
                                quizQuestionStatsObj.QuestionId = QuestionId;
                                quizQuestionStatsObj.StartedOn = currentDate;
                                quizQuestionStatsObj.Status = (int)StatusEnum.Active;

                                UOWObj.QuizQuestionStatsRepository.Insert(quizQuestionStatsObj);
                                UOWObj.Save();

                                #endregion

                                break;

                            case "complete_question":

                                #region fetch next question details

                                quizQuestionStatsObj = quizAttemptsObj.QuizQuestionStats.Where(r => r.QuestionId == QuestionId && r.Status == (int)StatusEnum.Active).FirstOrDefault();

                                if (quizQuestionStatsObj != null)
                                {
                                    var question = quizQuestionStatsObj.QuestionsInQuiz;

                                    if (!isLastQuestionAttempted)
                                    {
                                        if (!AnswerId.Any() && quizQuestionStatsObj.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single && quizQuestionStatsObj.QuestionsInQuiz.TimerRequired && quizQuestionStatsObj.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Any(r => r.IsUnansweredType))
                                            AnswerId.Add(quizQuestionStatsObj.QuestionsInQuiz.AnswerOptionsInQuizQuestions.FirstOrDefault(r => r.IsUnansweredType).Id);

                                        quizQuestionStatsObj.CompletedOn = currentDate;
                                        quizQuestionStatsObj.Status = (int)StatusEnum.Active;

                                        var quizQuestStatsList = quizQuestionStatsObj.QuizAnswerStats.ToList();

                                        foreach (var quizQuestStats in quizQuestStatsList)
                                        {
                                            UOWObj.QuizAnswerStatsRepository.Delete(quizQuestStats);
                                        }

                                        var answerType = question.AnswerType;

                                        if (answerType == (int)AnswerTypeEnum.Short || answerType == (int)AnswerTypeEnum.Long || answerType == (int)AnswerTypeEnum.DOB || answerType == (int)AnswerTypeEnum.PostCode || answerType == (int)AnswerTypeEnum.FullAddress || answerType == (int)AnswerTypeEnum.NPS || answerType == (int)AnswerTypeEnum.RatingEmoji || answerType == (int)AnswerTypeEnum.RatingStarts)
                                        {
                                            foreach (var textAnswerObj in TextAnswerList)
                                            {
                                                foreach (var answerObj in textAnswerObj.Answers)
                                                {
                                                    var quizAnswerStatsObj = new Db.QuizAnswerStats();
                                                    quizAnswerStatsObj.QuizQuestionStatsId = quizQuestionStatsObj.Id;
                                                    quizAnswerStatsObj.AnswerId = textAnswerObj.AnswerId;
                                                    quizAnswerStatsObj.AnswerText = answerObj.AnswerText;
                                                    quizAnswerStatsObj.SubAnswerTypeId = answerObj.SubAnswerTypeId;
                                                    quizAnswerStatsObj.AnswerSecondaryText = answerObj.AnswerSecondaryText;
                                                    quizAnswerStatsObj.CompanyId = quizAttemptsObj.CompanyId.HasValue ? quizAttemptsObj.CompanyId.Value : 0;
                                                    quizAnswerStatsObj.QuizAttemptId = quizQuestionStatsObj.QuizAttemptId;
                                                    quizAnswerStatsObj.AnswerOptionsInQuizQuestions = question.AnswerOptionsInQuizQuestions.FirstOrDefault(a => a.Id == textAnswerObj.AnswerId);
                                                    if (question.EnableComment)
                                                        quizAnswerStatsObj.Comment = answerObj.Comment;
                                                    UOWObj.QuizAnswerStatsRepository.Insert(quizAnswerStatsObj);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            foreach (var obj in AnswerId)
                                            {
                                                var quizAnswerStatsObj = new Db.QuizAnswerStats();
                                                quizAnswerStatsObj.QuizQuestionStatsId = quizQuestionStatsObj.Id;
                                                quizAnswerStatsObj.AnswerId = obj;
                                                quizAnswerStatsObj.AnswerText = null;
                                                quizAnswerStatsObj.AnswerOptionsInQuizQuestions = question.AnswerOptionsInQuizQuestions.FirstOrDefault(a => a.Id == obj);
                                                quizAnswerStatsObj.AnswerSecondaryText = null;
                                                quizAnswerStatsObj.CompanyId = quizAttemptsObj.CompanyId.HasValue ? quizAttemptsObj.CompanyId.Value : 0;
                                                quizAnswerStatsObj.QuizAttemptId = quizQuestionStatsObj.QuizAttemptId;
                                                UOWObj.QuizAnswerStatsRepository.Insert(quizAnswerStatsObj);
                                            }
                                        }

                                        UOWObj.QuizQuestionStatsRepository.Update(quizQuestionStatsObj);
                                        UOWObj.Save();
                                    }

                                    #region SubmittedAnswer

                                    if ((!UsageType.HasValue || UsageType.Value != (int)UsageTypeEnum.Chatbot) && revealScore && question.RevealCorrectAnswer.HasValue && question.RevealCorrectAnswer.Value
                                        && (question.AnswerType == (int)AnswerTypeEnum.Multiple || question.AnswerType == (int)AnswerTypeEnum.Single)
                                        && quizDetails.Quiz.QuizType != (int)QuizTypeEnum.Score && quizDetails.Quiz.QuizType != (int)QuizTypeEnum.ScoreTemplate
                                        && quizDetails.Quiz.QuizType != (int)QuizTypeEnum.Personality && quizDetails.Quiz.QuizType != (int)QuizTypeEnum.PersonalityTemplate)
                                    {
                                        var submittedAnswerOption = question.AnswerOptionsInQuizQuestions.Where(r => !r.IsUnansweredType && AnswerId.Contains(r.Id));

                                        quizAnswerSubmit.SubmittedAnswer = new QuizAnswerSubmit.SubmittedAnswerResult();

                                        quizAnswerSubmit.SubmittedAnswer.AliasTextForCorrect = question.AliasTextForCorrect;
                                        quizAnswerSubmit.SubmittedAnswer.AliasTextForIncorrect = question.AliasTextForIncorrect;
                                        quizAnswerSubmit.SubmittedAnswer.AliasTextForYourAnswer = question.AliasTextForYourAnswer;
                                        quizAnswerSubmit.SubmittedAnswer.AliasTextForCorrectAnswer = question.AliasTextForCorrectAnswer;
                                        quizAnswerSubmit.SubmittedAnswer.AliasTextForExplanation = question.AliasTextForExplanation;
                                        quizAnswerSubmit.SubmittedAnswer.AliasTextForNextButton = question.AliasTextForNextButton;
                                        quizAnswerSubmit.SubmittedAnswer.CorrectAnswerDescription = question.CorrectAnswerDescription;
                                        quizAnswerSubmit.SubmittedAnswer.ShowAnswerImage = question.ShowAnswerImage;

                                        quizAnswerSubmit.SubmittedAnswer.SubmittedAnswerDetails = new List<QuizAnswerSubmit.SubmittedAnswerResult.SubmittedAnswer>();

                                        if (submittedAnswerOption.Any())
                                        {
                                            foreach (var submittedAnswerOptionObj in submittedAnswerOption)
                                            {
                                                var submittedAnswerImage = string.Empty;
                                                var publicIdForSubmittedAnswer = string.Empty;

                                                if (question.ShowAnswerImage.HasValue && question.ShowAnswerImage.Value && submittedAnswerOptionObj.EnableMediaFile && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.ObjectId == submittedAnswerOptionObj.Id))
                                                {
                                                    var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.ObjectId == submittedAnswerOptionObj.Id);
                                                    submittedAnswerImage = mediaObj.ObjectValue;
                                                    publicIdForSubmittedAnswer = mediaObj.ObjectPublicId;

                                                    var newMedia = ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers);
                                                    if (newMedia != null)
                                                    {
                                                        submittedAnswerImage = newMedia.MediaUrl;
                                                        publicIdForSubmittedAnswer = newMedia.MediaPublicId;
                                                    }
                                                }
                                                else
                                                {
                                                    submittedAnswerImage = question.ShowAnswerImage.HasValue && question.ShowAnswerImage.Value ? submittedAnswerOptionObj.OptionImage : string.Empty;
                                                    publicIdForSubmittedAnswer = question.ShowAnswerImage.HasValue && question.ShowAnswerImage.Value ? submittedAnswerOptionObj.PublicId : string.Empty;
                                                }

                                                quizAnswerSubmit.SubmittedAnswer.SubmittedAnswerDetails.Add(new QuizAnswerSubmit.SubmittedAnswerResult.SubmittedAnswer
                                                {
                                                    SubmittedAnswerTitle = VariableLinking(submittedAnswerOptionObj.Option, quizDetails, quizAttemptsObj, false, false, null),
                                                    SubmittedAnswerImage = submittedAnswerImage,
                                                    PublicIdForSubmittedAnswer = publicIdForSubmittedAnswer,
                                                    AutoPlay = submittedAnswerOptionObj.AutoPlay,
                                                    SecondsToApply = submittedAnswerOptionObj.SecondsToApply,
                                                    VideoFrameEnabled = submittedAnswerOptionObj.VideoFrameEnabled
                                                });
                                            }


                                            if (question.AnswerType == (int)AnswerTypeEnum.Multiple && question.AnswerOptionsInQuizQuestions.Any(r => r.Status == (int)StatusEnum.Active && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value))
                                                quizAnswerSubmit.SubmittedAnswer.IsCorrect = (question.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Select(s => s.Id).OrderBy(s => s)).SequenceEqual(quizQuestionStatsObj.QuizAnswerStats.Select(r => r.AnswerId).OrderBy(s => s));
                                            else if (question.AnswerType == (int)AnswerTypeEnum.Single)
                                                quizAnswerSubmit.SubmittedAnswer.IsCorrect = submittedAnswerOption.FirstOrDefault().IsCorrectAnswer.HasValue ? submittedAnswerOption.FirstOrDefault().IsCorrectAnswer.Value : false;
                                        }
                                        else
                                        {
                                            quizAnswerSubmit.SubmittedAnswer.SubmittedAnswerDetails.Add(new QuizAnswerSubmit.SubmittedAnswerResult.SubmittedAnswer
                                            {
                                                SubmittedAnswerTitle = notAttemptedQuesText,
                                                SubmittedAnswerImage = string.Empty,
                                                PublicIdForSubmittedAnswer = string.Empty
                                            });

                                            quizAnswerSubmit.SubmittedAnswer.IsCorrect = false;
                                        }
                                        quizAnswerSubmit.SubmittedAnswer.CorrectAnswerDetails = new List<QuizAnswerSubmit.SubmittedAnswerResult.CorrectAnswer>();
                                        if (!(quizAnswerSubmit.SubmittedAnswer.IsCorrect.HasValue && quizAnswerSubmit.SubmittedAnswer.IsCorrect.Value))
                                        {
                                            var correctAnswerOption = question.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && ((question.AnswerType == (int)AnswerTypeEnum.Multiple && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value) || (question.AnswerType == (int)AnswerTypeEnum.Single && r.IsCorrectAnswer.HasValue && r.IsCorrectAnswer.Value)));

                                            if (correctAnswerOption != null)
                                            {
                                                foreach (var correctAnswerOptionObj in correctAnswerOption)
                                                {
                                                    var correctAnswerImage = string.Empty;
                                                    var publicIdForCorrectAnswer = string.Empty;

                                                    if (question.ShowAnswerImage.HasValue && question.ShowAnswerImage.Value && correctAnswerOptionObj.EnableMediaFile && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.ObjectId == correctAnswerOptionObj.Id))
                                                    {
                                                        var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.ObjectId == correctAnswerOptionObj.Id);
                                                        correctAnswerImage = mediaObj.ObjectValue;
                                                        publicIdForCorrectAnswer = mediaObj.ObjectPublicId;

                                                        var newMedia = ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers);
                                                        if (newMedia != null)
                                                        {
                                                            correctAnswerImage = newMedia.MediaUrl;
                                                            publicIdForCorrectAnswer = newMedia.MediaPublicId;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        correctAnswerImage = question.ShowAnswerImage.HasValue && question.ShowAnswerImage.Value ? correctAnswerOptionObj.OptionImage : string.Empty;
                                                        publicIdForCorrectAnswer = question.ShowAnswerImage.HasValue && question.ShowAnswerImage.Value ? correctAnswerOptionObj.PublicId : string.Empty;
                                                    }

                                                    quizAnswerSubmit.SubmittedAnswer.CorrectAnswerDetails.Add(new QuizAnswerSubmit.SubmittedAnswerResult.CorrectAnswer()
                                                    {
                                                        CorrectAnswerTitle = VariableLinking(correctAnswerOptionObj.Option, quizDetails, quizAttemptsObj, false, false, null),
                                                        CorrectAnswerImage = correctAnswerImage,
                                                        PublicIdForCorrectAnswer = publicIdForCorrectAnswer,
                                                        AutoPlay = correctAnswerOptionObj.AutoPlay,
                                                        SecondsToApply = correctAnswerOptionObj.SecondsToApply,
                                                        VideoFrameEnabled = correctAnswerOptionObj.VideoFrameEnabled
                                                    });
                                                }
                                            }
                                        }
                                    }
                                    #endregion

                                    if (question.AnswerType == (int)AnswerTypeEnum.Single || question.AnswerType == (int)AnswerTypeEnum.LookingforJobs)

                                        nextQuestionObj = FetchNextQuestion(quizDetails, AnswerId.FirstOrDefault(), (int)BranchingLogicEnum.ANSWER, UOWObj, isQuesAndContentInSameTable);
                                    else
                                        nextQuestionObj = FetchNextQuestion(quizDetails, question.Id, (int)BranchingLogicEnum.QUESTIONNEXT, UOWObj, isQuesAndContentInSameTable);

                                    if (nextQuestionObj != null)
                                    {
                                        #region if next is question type

                                        if (nextQuestionObj.GetType().BaseType.Name == "QuestionsInQuiz")
                                        {
                                            if (((Db.QuestionsInQuiz)nextQuestionObj).Type == (int)BranchingLogicEnum.QUESTION)
                                            {
                                                quizAnswerSubmit.QuestionType = (int)BranchingLogicEnum.QUESTION;

                                                quizAnswerSubmit.QuestionDetails = new QuizQuestion();

                                                quizAnswerSubmit.QuestionDetails.QuestionId = ((Db.QuestionsInQuiz)nextQuestionObj).Id;
                                                quizAnswerSubmit.QuestionDetails.QuestionTitle = VariableLinking(((Db.QuestionsInQuiz)nextQuestionObj).Question, quizDetails, quizAttemptsObj, false, false, null);
                                                quizAnswerSubmit.QuestionDetails.ShowTitle = ((Db.QuestionsInQuiz)nextQuestionObj).ShowTitle;

                                                if (((Db.QuestionsInQuiz)nextQuestionObj).ShowQuestionImage.HasValue && ((Db.QuestionsInQuiz)nextQuestionObj).ShowQuestionImage.Value && ((Db.QuestionsInQuiz)nextQuestionObj).EnableMediaFile && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION && r.ObjectId == ((Db.QuestionsInQuiz)nextQuestionObj).Id))
                                                {
                                                    var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION && r.ObjectId == ((Db.QuestionsInQuiz)nextQuestionObj).Id);
                                                    quizAnswerSubmit.QuestionDetails.QuestionImage = mediaObj.ObjectValue;
                                                    quizAnswerSubmit.QuestionDetails.PublicIdForQuestion = mediaObj.ObjectPublicId;

                                                    var newMedia = ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers);
                                                    if (newMedia != null)
                                                    {
                                                        quizAnswerSubmit.QuestionDetails.QuestionImage = newMedia.MediaUrl;
                                                        quizAnswerSubmit.QuestionDetails.PublicIdForQuestion = newMedia.MediaPublicId;
                                                    }
                                                }
                                                else
                                                {
                                                    quizAnswerSubmit.QuestionDetails.QuestionImage = ((Db.QuestionsInQuiz)nextQuestionObj).ShowQuestionImage.HasValue && ((Db.QuestionsInQuiz)nextQuestionObj).ShowQuestionImage.Value ? ((Db.QuestionsInQuiz)nextQuestionObj).QuestionImage : string.Empty;
                                                    quizAnswerSubmit.QuestionDetails.PublicIdForQuestion = ((Db.QuestionsInQuiz)nextQuestionObj).ShowQuestionImage.HasValue && ((Db.QuestionsInQuiz)nextQuestionObj).ShowQuestionImage.Value ? ((Db.QuestionsInQuiz)nextQuestionObj).PublicId : string.Empty;
                                                }

                                                quizAnswerSubmit.QuestionDetails.ShowQuestionImage = ((Db.QuestionsInQuiz)nextQuestionObj).ShowQuestionImage;
                                                quizAnswerSubmit.QuestionDetails.ShowAnswerImage = ((Db.QuestionsInQuiz)nextQuestionObj).ShowAnswerImage;
                                                quizAnswerSubmit.QuestionDetails.AnswerType = ((Db.QuestionsInQuiz)nextQuestionObj).AnswerType;
                                                quizAnswerSubmit.QuestionDetails.NextButtonColor = ((Db.QuestionsInQuiz)nextQuestionObj).NextButtonColor;
                                                quizAnswerSubmit.QuestionDetails.NextButtonText = ((Db.QuestionsInQuiz)nextQuestionObj).NextButtonText;
                                                quizAnswerSubmit.QuestionDetails.NextButtonTxtColor = ((Db.QuestionsInQuiz)nextQuestionObj).NextButtonTxtColor;
                                                quizAnswerSubmit.QuestionDetails.NextButtonTxtSize = ((Db.QuestionsInQuiz)nextQuestionObj).NextButtonTxtSize;
                                                quizAnswerSubmit.QuestionDetails.EnableNextButton = ((Db.QuestionsInQuiz)nextQuestionObj).EnableNextButton;
                                                quizAnswerSubmit.QuestionDetails.MinAnswer = ((Db.QuestionsInQuiz)nextQuestionObj).MinAnswer;
                                                quizAnswerSubmit.QuestionDetails.MaxAnswer = ((Db.QuestionsInQuiz)nextQuestionObj).MaxAnswer;
                                                quizAnswerSubmit.QuestionDetails.ViewPreviousQuestion = ((Db.QuestionsInQuiz)nextQuestionObj).ViewPreviousQuestion;
                                                quizAnswerSubmit.QuestionDetails.EditAnswer = ((Db.QuestionsInQuiz)nextQuestionObj).EditAnswer;
                                                quizAnswerSubmit.IsBackButtonEnable = (question.RevealCorrectAnswer.HasValue && question.RevealCorrectAnswer.Value) ? false : (question.ViewPreviousQuestion || question.EditAnswer);
                                                quizAnswerSubmit.QuestionDetails.StartedOn = (isLastQuestionStarted && ((Db.QuestionsInQuiz)nextQuestionObj).QuizQuestionStats != null && ((Db.QuestionsInQuiz)nextQuestionObj).QuizQuestionStats.Any(r => r.Status == (int)StatusEnum.Active)) ? ((Db.QuestionsInQuiz)nextQuestionObj).QuizQuestionStats.FirstOrDefault(r => r.Status == (int)StatusEnum.Active).StartedOn : default(DateTime?);
                                                quizAnswerSubmit.QuestionDetails.TimerRequired = ((Db.QuestionsInQuiz)nextQuestionObj).TimerRequired;
                                                quizAnswerSubmit.QuestionDetails.Time = ((Db.QuestionsInQuiz)nextQuestionObj).Time;
                                                quizAnswerSubmit.QuestionDetails.AutoPlay = ((Db.QuestionsInQuiz)nextQuestionObj).AutoPlay;
                                                quizAnswerSubmit.QuestionDetails.SecondsToApply = ((Db.QuestionsInQuiz)nextQuestionObj).SecondsToApply ?? "0";
                                                quizAnswerSubmit.QuestionDetails.VideoFrameEnabled = ((Db.QuestionsInQuiz)nextQuestionObj).VideoFrameEnabled ?? false;
                                                quizAnswerSubmit.QuestionDetails.DisplayOrderForTitle = ((Db.QuestionsInQuiz)nextQuestionObj).DisplayOrderForTitle;
                                                quizAnswerSubmit.QuestionDetails.DisplayOrderForTitleImage = ((Db.QuestionsInQuiz)nextQuestionObj).DisplayOrderForTitleImage;
                                                quizAnswerSubmit.QuestionDetails.DisplayOrderForDescription = ((Db.QuestionsInQuiz)nextQuestionObj).DisplayOrderForDescription;
                                                quizAnswerSubmit.QuestionDetails.DisplayOrderForDescriptionImage = ((Db.QuestionsInQuiz)nextQuestionObj).DisplayOrderForDescriptionImage;
                                                quizAnswerSubmit.QuestionDetails.DisplayOrderForAnswer = ((Db.QuestionsInQuiz)nextQuestionObj).DisplayOrderForAnswer;
                                                quizAnswerSubmit.QuestionDetails.DisplayOrderForNextButton = ((Db.QuestionsInQuiz)nextQuestionObj).DisplayOrderForNextButton;
                                                quizAnswerSubmit.QuestionDetails.Description = ((Db.QuestionsInQuiz)nextQuestionObj).Description;
                                                quizAnswerSubmit.QuestionDetails.ShowDescription = ((Db.QuestionsInQuiz)nextQuestionObj).ShowDescription;
                                                quizAnswerSubmit.QuestionDetails.ShowDescriptionImage = ((Db.QuestionsInQuiz)nextQuestionObj).ShowDescriptionImage ?? false;
                                                quizAnswerSubmit.QuestionDetails.AutoPlayForDescription = ((Db.QuestionsInQuiz)nextQuestionObj).AutoPlayForDescription;
                                                quizAnswerSubmit.QuestionDetails.SecondsToApplyForDescription = ((Db.QuestionsInQuiz)nextQuestionObj).SecondsToApplyForDescription ?? "0";
                                                quizAnswerSubmit.QuestionDetails.DescVideoFrameEnabled = ((Db.QuestionsInQuiz)nextQuestionObj).DescVideoFrameEnabled ?? false;
                                                quizAnswerSubmit.QuestionDetails.EnableComment = ((Db.QuestionsInQuiz)nextQuestionObj).EnableComment;
                                                quizAnswerSubmit.QuestionDetails.AnswerStructureType = ((Db.QuestionsInQuiz)nextQuestionObj).AnswerStructureType;

                                                if (((Db.QuestionsInQuiz)nextQuestionObj).EnableMediaFileForDescription && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Description && r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION && r.ObjectId == ((Db.QuestionsInQuiz)nextQuestionObj).Id))
                                                {
                                                    var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Description && r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION && r.ObjectId == ((Db.QuestionsInQuiz)nextQuestionObj).Id);
                                                    quizAnswerSubmit.QuestionDetails.DescriptionImage = mediaObj.ObjectValue ?? string.Empty;
                                                    quizAnswerSubmit.QuestionDetails.PublicIdForDescription = mediaObj.ObjectPublicId ?? string.Empty;

                                                    var newMedia = ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers);
                                                    if (newMedia != null)
                                                    {
                                                        quizAnswerSubmit.QuestionDetails.DescriptionImage = newMedia.MediaUrl;
                                                        quizAnswerSubmit.QuestionDetails.PublicIdForDescription = newMedia.MediaPublicId;
                                                    }
                                                }
                                                else
                                                {
                                                    quizAnswerSubmit.QuestionDetails.DescriptionImage = ((Db.QuestionsInQuiz)nextQuestionObj).DescriptionImage ?? string.Empty;
                                                    quizAnswerSubmit.QuestionDetails.PublicIdForDescription = ((Db.QuestionsInQuiz)nextQuestionObj).PublicIdForDescription ?? string.Empty;
                                                }
                                                quizAnswerSubmit.QuestionDetails.AnswerList = new List<AnswerOptionInQuestion>();

                                                foreach (var ans in ((Db.QuestionsInQuiz)nextQuestionObj).AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && !r.IsUnansweredType).OrderBy(r => r.DisplayOrder))
                                                {
                                                    var answerImage = string.Empty;
                                                    var publicIdForAnswer = string.Empty;

                                                    if (((Db.QuestionsInQuiz)nextQuestionObj).ShowAnswerImage.HasValue && ((Db.QuestionsInQuiz)nextQuestionObj).ShowAnswerImage.Value && ans.EnableMediaFile && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.ObjectId == ans.Id))
                                                    {
                                                        var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.ObjectId == ans.Id);
                                                        answerImage = mediaObj.ObjectValue;
                                                        publicIdForAnswer = mediaObj.ObjectPublicId;

                                                        var newMedia = ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers);
                                                        if (newMedia != null)
                                                        {
                                                            answerImage = newMedia.MediaUrl;
                                                            publicIdForAnswer = newMedia.MediaPublicId;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        answerImage = ((Db.QuestionsInQuiz)nextQuestionObj).ShowAnswerImage.HasValue && ((Db.QuestionsInQuiz)nextQuestionObj).ShowAnswerImage.Value ? ans.OptionImage : string.Empty;
                                                        publicIdForAnswer = ((Db.QuestionsInQuiz)nextQuestionObj).ShowAnswerImage.HasValue && ((Db.QuestionsInQuiz)nextQuestionObj).ShowAnswerImage.Value ? ans.PublicId : string.Empty;
                                                    }

                                                    quizAnswerSubmit.QuestionDetails.AnswerList.Add(new AnswerOptionInQuestion
                                                    {
                                                        AnswerId = ans.Id,
                                                        AssociatedScore = ans.AssociatedScore,
                                                        AnswerText = VariableLinking(ans.Option, quizDetails, quizAttemptsObj, false, false, null),
                                                        AnswerImage = answerImage,
                                                        PublicIdForAnswer = publicIdForAnswer,
                                                        IsCorrectAnswer = false,
                                                        DisplayOrder = ans.DisplayOrder,
                                                        IsUnansweredType = ans.IsUnansweredType,
                                                        AutoPlay = ans.AutoPlay,
                                                        SecondsToApply = ans.SecondsToApply,
                                                        VideoFrameEnabled = ans.VideoFrameEnabled,
                                                        ListValues = ans.ListValues,
                                                        //for Rating type question
                                                        OptionTextforRatingOne = ans.OptionTextforRatingOne,
                                                        OptionTextforRatingTwo = ans.OptionTextforRatingTwo,
                                                        OptionTextforRatingThree = ans.OptionTextforRatingThree,
                                                        OptionTextforRatingFour = ans.OptionTextforRatingFour,
                                                        OptionTextforRatingFive = ans.OptionTextforRatingFive
                                                    });
                                                }

                                                if (!isLastQuestionStarted && ((UsageType.HasValue && UsageType.Value == (int)UsageTypeEnum.Chatbot) || (!(question.RevealCorrectAnswer.HasValue && question.RevealCorrectAnswer.Value))))
                                                {
                                                    quizQuestionStatsObj = new Db.QuizQuestionStats();

                                                    quizQuestionStatsObj.QuizAttemptId = quizAttemptsObj.Id;
                                                    quizQuestionStatsObj.QuestionId = ((Db.QuestionsInQuiz)nextQuestionObj).Id;
                                                    quizQuestionStatsObj.StartedOn = currentDate;
                                                    quizQuestionStatsObj.Status = (int)StatusEnum.Active;

                                                    UOWObj.QuizQuestionStatsRepository.Insert(quizQuestionStatsObj);
                                                    UOWObj.Save();
                                                }

                                                #region PreviousQuestionSubmittedAnswer

                                                if (quizAttemptsObj.QuizQuestionStats.FirstOrDefault(r => r.QuestionId == ((Db.QuestionsInQuiz)nextQuestionObj).Id) != null && (quizAttemptsObj.QuizQuestionStats.Any(r => r.QuestionId == ((Db.QuestionsInQuiz)nextQuestionObj).Id && r.CompletedOn.HasValue) ?
                                                    quizAttemptsObj.QuizQuestionStats.LastOrDefault(r => r.QuestionId == ((Db.QuestionsInQuiz)nextQuestionObj).Id && r.CompletedOn.HasValue).QuizAnswerStats != null
                                                    : quizAttemptsObj.QuizQuestionStats.LastOrDefault(r => r.QuestionId == ((Db.QuestionsInQuiz)nextQuestionObj).Id).QuizAnswerStats != null))
                                                {
                                                    var attemptedAnswerIds = quizAttemptsObj.QuizQuestionStats.Any(r => r.QuestionId == ((Db.QuestionsInQuiz)nextQuestionObj).Id && r.CompletedOn.HasValue) ?
                                                        quizAttemptsObj.QuizQuestionStats.LastOrDefault(r => r.QuestionId == ((Db.QuestionsInQuiz)nextQuestionObj).Id && r.CompletedOn.HasValue).QuizAnswerStats.Select(r => r.AnswerId).ToList() :
                                                        quizAttemptsObj.QuizQuestionStats.LastOrDefault(r => r.QuestionId == ((Db.QuestionsInQuiz)nextQuestionObj).Id).QuizAnswerStats.Select(r => r.AnswerId).ToList();

                                                    var submittedAnswerOption = ((Db.QuestionsInQuiz)nextQuestionObj).AnswerOptionsInQuizQuestions.Where(r => attemptedAnswerIds.Contains(r.Id));
                                                    if (submittedAnswerOption.Any())
                                                    {
                                                        PreviousQuestionSubmittedAnswerMapping(quizAnswerSubmit, nextQuestionObj);

                                                        foreach (var submittedAnswerOptionObj in submittedAnswerOption)
                                                            CheckQuizAnswerType(quizAnswerSubmit, quizAttemptsObj, quizDetails, nextQuestionObj, submittedAnswerOptionObj);
                                                    }
                                                }
                                                #endregion
                                            }
                                            else if (((Db.QuestionsInQuiz)nextQuestionObj).Type == (int)BranchingLogicEnum.CONTENT)
                                            {
                                                quizAnswerSubmit.QuestionType = (int)BranchingLogicEnum.CONTENT;

                                                if (!isLastQuestionStarted)
                                                {
                                                    quizQuestionStatsObj = new Db.QuizQuestionStats();

                                                    quizQuestionStatsObj.QuizAttemptId = quizAttemptsObj.Id;
                                                    quizQuestionStatsObj.QuestionId = ((Db.QuestionsInQuiz)nextQuestionObj).Id;
                                                    quizQuestionStatsObj.StartedOn = currentDate;
                                                    quizQuestionStatsObj.Status = (int)StatusEnum.Active;

                                                    UOWObj.QuizQuestionStatsRepository.Insert(quizQuestionStatsObj);
                                                    UOWObj.Save();
                                                }

                                                quizAnswerSubmit.ContentDetails = new QuizContent();

                                                quizAnswerSubmit.ContentDetails.Id = ((Db.QuestionsInQuiz)nextQuestionObj).Id;
                                                quizAnswerSubmit.ContentDetails.ContentTitle = VariableLinking(((Db.QuestionsInQuiz)nextQuestionObj).Question, quizDetails, quizAttemptsObj, false, false, null);
                                                quizAnswerSubmit.ContentDetails.ContentDescription = VariableLinking(((Db.QuestionsInQuiz)nextQuestionObj).Description, quizDetails, quizAttemptsObj, false, false, null);
                                                quizAnswerSubmit.ContentDetails.ShowDescription = ((Db.QuestionsInQuiz)nextQuestionObj).ShowDescription;

                                                if ((((Db.QuestionsInQuiz)nextQuestionObj).EnableMediaFile && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Title && r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == ((Db.QuestionsInQuiz)nextQuestionObj).Id)))
                                                {
                                                    var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Title && r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == ((Db.QuestionsInQuiz)nextQuestionObj).Id);
                                                    quizAnswerSubmit.ContentDetails.ContentTitleImage = mediaObj.ObjectValue;
                                                    quizAnswerSubmit.ContentDetails.PublicIdForContentTitle = mediaObj.ObjectPublicId ?? string.Empty;

                                                    var newMedia = ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers);
                                                    if (newMedia != null)
                                                    {
                                                        quizAnswerSubmit.ContentDetails.ContentTitleImage = newMedia.MediaUrl;
                                                        quizAnswerSubmit.ContentDetails.PublicIdForContentTitle = newMedia.MediaPublicId;
                                                    }
                                                }
                                                else
                                                {
                                                    quizAnswerSubmit.ContentDetails.ContentTitleImage = ((Db.QuestionsInQuiz)nextQuestionObj).QuestionImage ?? string.Empty;
                                                    quizAnswerSubmit.ContentDetails.PublicIdForContentTitle = ((Db.QuestionsInQuiz)nextQuestionObj).PublicId ?? string.Empty;
                                                }

                                                quizAnswerSubmit.ContentDetails.ShowContentTitleImage = ((Db.QuestionsInQuiz)nextQuestionObj).ShowQuestionImage.HasValue && ((Db.QuestionsInQuiz)nextQuestionObj).ShowQuestionImage.Value ? ((Db.QuestionsInQuiz)nextQuestionObj).ShowQuestionImage.Value : false;
                                                quizAnswerSubmit.ContentDetails.AliasTextForNextButton = ((Db.QuestionsInQuiz)nextQuestionObj).NextButtonText;
                                                quizAnswerSubmit.ContentDetails.EnableNextButton = ((Db.QuestionsInQuiz)nextQuestionObj).EnableNextButton;

                                                if (((Db.QuestionsInQuiz)nextQuestionObj).EnableMediaFileForDescription && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Description && r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == ((Db.QuestionsInQuiz)nextQuestionObj).Id))
                                                {
                                                    var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Description && r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == ((Db.QuestionsInQuiz)nextQuestionObj).Id);
                                                    quizAnswerSubmit.ContentDetails.ContentDescriptionImage = mediaObj.ObjectValue ?? string.Empty;
                                                    quizAnswerSubmit.ContentDetails.PublicIdForContentDescription = mediaObj.ObjectPublicId ?? string.Empty;

                                                    var newMedia = ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers);
                                                    if (newMedia != null)
                                                    {
                                                        quizAnswerSubmit.ContentDetails.ContentDescriptionImage = newMedia.MediaUrl;
                                                        quizAnswerSubmit.ContentDetails.PublicIdForContentDescription = newMedia.MediaPublicId;
                                                    }
                                                }
                                                else
                                                {
                                                    quizAnswerSubmit.ContentDetails.ContentDescriptionImage = ((Db.QuestionsInQuiz)nextQuestionObj).DescriptionImage ?? string.Empty;
                                                    quizAnswerSubmit.ContentDetails.PublicIdForContentDescription = ((Db.QuestionsInQuiz)nextQuestionObj).PublicIdForDescription ?? string.Empty;
                                                }

                                                quizAnswerSubmit.ContentDetails.ShowContentDescriptionImage = ((Db.QuestionsInQuiz)nextQuestionObj).ShowDescriptionImage.HasValue && ((Db.QuestionsInQuiz)nextQuestionObj).ShowDescriptionImage.Value ? ((Db.QuestionsInQuiz)nextQuestionObj).ShowDescriptionImage.Value : false;
                                                quizAnswerSubmit.ContentDetails.ViewPreviousQuestion = ((Db.QuestionsInQuiz)nextQuestionObj).ViewPreviousQuestion;
                                                quizAnswerSubmit.ContentDetails.AutoPlay = ((Db.QuestionsInQuiz)nextQuestionObj).AutoPlay;
                                                quizAnswerSubmit.ContentDetails.SecondsToApply = ((Db.QuestionsInQuiz)nextQuestionObj).SecondsToApply ?? "0";
                                                quizAnswerSubmit.ContentDetails.VideoFrameEnabled = ((Db.QuestionsInQuiz)nextQuestionObj).VideoFrameEnabled ?? false;
                                                quizAnswerSubmit.ContentDetails.AutoPlayForDescription = ((Db.QuestionsInQuiz)nextQuestionObj).AutoPlayForDescription;
                                                quizAnswerSubmit.ContentDetails.SecondsToApplyForDescription = ((Db.QuestionsInQuiz)nextQuestionObj).SecondsToApplyForDescription ?? "0";
                                                quizAnswerSubmit.ContentDetails.DescVideoFrameEnabled = ((Db.QuestionsInQuiz)nextQuestionObj).DescVideoFrameEnabled ?? false;
                                                quizAnswerSubmit.ContentDetails.ShowTitle = ((Db.QuestionsInQuiz)nextQuestionObj).ShowTitle;
                                                quizAnswerSubmit.ContentDetails.DisplayOrderForTitle = ((Db.QuestionsInQuiz)nextQuestionObj).DisplayOrderForTitle;
                                                quizAnswerSubmit.ContentDetails.DisplayOrderForTitleImage = ((Db.QuestionsInQuiz)nextQuestionObj).DisplayOrderForTitleImage;
                                                quizAnswerSubmit.ContentDetails.DisplayOrderForDescription = ((Db.QuestionsInQuiz)nextQuestionObj).DisplayOrderForDescription;
                                                quizAnswerSubmit.ContentDetails.DisplayOrderForDescriptionImage = ((Db.QuestionsInQuiz)nextQuestionObj).DisplayOrderForDescriptionImage;
                                                quizAnswerSubmit.ContentDetails.DisplayOrderForNextButton = ((Db.QuestionsInQuiz)nextQuestionObj).DisplayOrderForNextButton;
                                                quizAnswerSubmit.IsBackButtonEnable = (question.RevealCorrectAnswer.HasValue && question.RevealCorrectAnswer.Value) ? false : (question.ViewPreviousQuestion || question.EditAnswer);
                                            }
                                        }

                                        #endregion

                                        #region if next is result type

                                        if (nextQuestionObj.GetType().BaseType.Name == "QuizResults")
                                        {
                                            quizAnswerSubmit.QuestionType = (int)BranchingLogicEnum.RESULT;

                                            var result = ((Db.QuizResults)nextQuestionObj);
                                            var attemptedQuestions = quizAttemptsObj.QuizQuestionStats.Where(t => t.Status == (int)StatusEnum.Active && t.CompletedOn != null && (t.QuestionsInQuiz.TimerRequired ? true : t.QuizAnswerStats.Any(r => !r.AnswerOptionsInQuizQuestions.IsUnansweredType)));
                                            quizAnswerSubmit.ShowLeadUserForm = (Mode == "PREVIEW" || Mode == "PREVIEWTEMPLATE") ? false : (resultIdList.Any() ? (resultIdList.Any(r => r == result.Id || quizComponentLogsList.Any(q => q.DraftedObjectId == r && q.PublishedObjectId == result.Id))) : (quizAttemptsObj.LeadUserId == null && quizAttemptsObj.RecruiterUserId == null && ((quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Personality && quizDetails.PersonalityResultSetting.FirstOrDefault().Status == (int)StatusEnum.Active) ? quizDetails.PersonalityResultSetting.FirstOrDefault().ShowLeadUserForm : result.ShowLeadUserForm)));
                                            quizAnswerSubmit.CompanyCode = quizDetails.Quiz.Company.ClientCode;
                                            quizAnswerSubmit.IsBackButtonEnable = false;

                                            if (leadFormDetailofResultsLst != null && leadFormDetailofResultsLst.Any() && quizAnswerSubmit.ShowLeadUserForm)
                                            {
                                                var leadFormDetailofResultsObj = leadFormDetailofResultsLst.FirstOrDefault(r => r.ResultId == result.Id || quizComponentLogsList.Any(q => q.DraftedObjectId == r.ResultId && q.PublishedObjectId == result.Id));
                                                quizAnswerSubmit.FormId = leadFormDetailofResultsObj.FormId;
                                                quizAnswerSubmit.FlowOrder = leadFormDetailofResultsObj.FlowOrder;
                                            }

                                            if (quizDetails.Quiz.QuizType != (int)QuizTypeEnum.Personality && quizDetails.Quiz.QuizType != (int)QuizTypeEnum.PersonalityTemplate)
                                            {
                                                #region non Personality type quiz

                                                quizAnswerSubmit.ResultScore = new QuizAnswerSubmit.Result();

                                                if (result.EnableMediaFile && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.RESULT && r.ObjectId == result.Id))
                                                {
                                                    var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.RESULT && r.ObjectId == result.Id);
                                                    quizAnswerSubmit.ResultScore.Image = result.ShowResultImage ? mediaObj.ObjectValue : string.Empty;
                                                    quizAnswerSubmit.ResultScore.PublicIdForResult = result.ShowResultImage ? mediaObj.ObjectPublicId : string.Empty;

                                                    var newMedia = result.ShowResultImage ? ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers) : null;
                                                    if (newMedia != null)
                                                    {
                                                        quizAnswerSubmit.ResultScore.Image = newMedia.MediaUrl;
                                                        quizAnswerSubmit.ResultScore.PublicIdForResult = newMedia.MediaPublicId;
                                                    }
                                                }
                                                else
                                                {
                                                    quizAnswerSubmit.ResultScore.Image = result.ShowResultImage ? result.Image : string.Empty;
                                                    quizAnswerSubmit.ResultScore.PublicIdForResult = result.ShowResultImage ? result.PublicId : string.Empty;
                                                }
                                                quizAnswerSubmit.ResultScore.HideActionButton = result.HideCallToAction.HasValue ? result.HideCallToAction.Value : false;
                                                quizAnswerSubmit.ResultScore.ActionButtonURL = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonURL;
                                                quizAnswerSubmit.ResultScore.OpenLinkInNewTab = quizAnswerSubmit.ResultScore.HideActionButton ? false : result.OpenLinkInNewTab;
                                                quizAnswerSubmit.ResultScore.ActionButtonTxtSize = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonTxtSize;
                                                quizAnswerSubmit.ResultScore.ActionButtonColor = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonColor;
                                                quizAnswerSubmit.ResultScore.ActionButtonTitleColor = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonTitleColor;
                                                quizAnswerSubmit.ResultScore.ActionButtonText = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonText;

                                                var resultSetting = quizDetails.ResultSettings.FirstOrDefault();
                                                var correctAnsCount = 0;
                                                string scoreValueTxt = string.Empty;
                                                if (resultSetting != null)
                                                {
                                                    if (resultSetting.ShowScoreValue.HasValue && resultSetting.ShowScoreValue.Value && (((quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate) && attemptedQuestions.Any(r => r.QuestionsInQuiz.Status == (int)StatusEnum.Active && (r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple))) || attemptedQuestions.Any(r => (r.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Any(t => t.Status == (int)StatusEnum.Active && t.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && t.IsCorrectForMultipleAnswer.HasValue && t.IsCorrectForMultipleAnswer.Value)) || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single)))
                                                    {
                                                        scoreValueTxt = resultSetting.CustomTxtForScoreValueInResult;

                                                        if (quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                                            correctAnsCount = attemptedQuestions.Sum(a => a.QuizAnswerStats.Sum(x => x.AnswerOptionsInQuizQuestions.AssociatedScore)).Value;
                                                        else
                                                            correctAnsCount = attemptedQuestions.Count(a => (a.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && ((a.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Select(s => s.Id).OrderBy(s => s)).SequenceEqual(a.QuizAnswerStats.Select(r => r.AnswerId).OrderBy(s => s)))) || (a.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single && a.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer == true));

                                                        scoreValueTxt = scoreValueTxt == null ? string.Empty : scoreValueTxt.Replace("%score%", ' ' + correctAnsCount.ToString() + ' ').Replace("%total%", ' ' + attemptedQuestions.Count(t => (t.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Any(r => r.Status == (int)StatusEnum.Active && r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value)) || t.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single).ToString() + ' ');

                                                        quizAnswerSubmit.ResultScore.ResultScoreValueTxt = scoreValueTxt;
                                                        quizAnswerSubmit.ResultScore.ShowScoreValue = true;
                                                    }
                                                    else
                                                    {
                                                        quizAnswerSubmit.ResultScore.ResultScoreValueTxt = string.Empty;
                                                        quizAnswerSubmit.ResultScore.ShowScoreValue = false;
                                                    }

                                                    if (resultSetting.ShowCorrectAnswer.HasValue && resultSetting.ShowCorrectAnswer.Value)
                                                    {
                                                        quizAnswerSubmit.ResultScore.AnswerKeyCustomTxt = resultSetting.CustomTxtForAnswerKey;
                                                        quizAnswerSubmit.ResultScore.YourAnswerCustomTxt = resultSetting.CustomTxtForYourAnswer;
                                                        quizAnswerSubmit.ResultScore.CorrectAnswerCustomTxt = resultSetting.CustomTxtForCorrectAnswer;
                                                        quizAnswerSubmit.ResultScore.ExplanationCustomTxt = resultSetting.CustomTxtForExplanation;
                                                        quizAnswerSubmit.ResultScore.ShowCorrectAnswer = true;

                                                        quizAnswerSubmit.ResultScore.AttemptedQuestionAnswerDetails = new List<QuizAnswerSubmit.AnswerResult>();

                                                        foreach (var item in attemptedQuestions.Where(r => r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Short || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Long))
                                                        {
                                                            var correctAnswerTxt = string.Empty;
                                                            bool? IsCorrectValue = null;
                                                            int associatedScore = default(int);
                                                            var yourAnswer = string.Empty;

                                                            if (item.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Any())
                                                            {
                                                                if (quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                                                    associatedScore = item.QuizAnswerStats.Sum(x => x.AnswerOptionsInQuizQuestions.AssociatedScore).Value;
                                                                else
                                                                    IsCorrectValue = (item.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Select(s => s.Id).OrderBy(s => s)).SequenceEqual(item.QuizAnswerStats.Select(r => r.AnswerId).OrderBy(s => s));

                                                                correctAnswerTxt = VariableLinking(string.Join(",", item.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Select(t => t.Option).Select(s => "'" + s + "'")), quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt);
                                                            }
                                                            else if (item.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single)
                                                            {
                                                                correctAnswerTxt = VariableLinking(item.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(t => t.IsCorrectAnswer.HasValue ? t.IsCorrectAnswer.Value : false).Select(t => t.Option).FirstOrDefault(), quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt);
                                                                if (quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                                                    associatedScore = item.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.AssociatedScore.Value;
                                                                else
                                                                    IsCorrectValue = item.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer.HasValue && item.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer.Value;
                                                            }

                                                            if (item.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple || item.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single)
                                                                yourAnswer = VariableLinking(string.Join(",", item.QuizAnswerStats.Where(t => !t.AnswerOptionsInQuizQuestions.IsUnansweredType).Select(t => t.AnswerOptionsInQuizQuestions.Option).Select(s => "'" + s + "'")), quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt);
                                                            else
                                                                yourAnswer = item.QuizAnswerStats.Select(t => t.AnswerText).FirstOrDefault();

                                                            quizAnswerSubmit.ResultScore.AttemptedQuestionAnswerDetails.Add(new QuizAnswerSubmit.AnswerResult
                                                            {
                                                                Question = VariableLinking(item.QuestionsInQuiz.Question, quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt),
                                                                YourAnswer = string.IsNullOrEmpty(yourAnswer) ? notAttemptedQuesText : yourAnswer,
                                                                CorrectAnswer = (quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate) ? string.Empty : correctAnswerTxt,
                                                                IsCorrect = IsCorrectValue,
                                                                AnswerExplanation = item.QuestionsInQuiz.CorrectAnswerDescription,
                                                                AssociatedScore = (quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate) ? associatedScore : default(int),
                                                            });
                                                        }
                                                    }
                                                    else
                                                    {
                                                        quizAnswerSubmit.ResultScore.ShowCorrectAnswer = false;
                                                    }
                                                }

                                                quizAnswerSubmit.ResultScore.ShowInternalTitle = result.ShowInternalTitle;
                                                quizAnswerSubmit.ResultScore.ShowExternalTitle = result.ShowExternalTitle;
                                                quizAnswerSubmit.ResultScore.ShowDescription = result.ShowDescription;
                                                quizAnswerSubmit.ResultScore.Title = VariableLinking(result.Title, quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt);
                                                quizAnswerSubmit.ResultScore.InternalTitle = VariableLinking(result.InternalTitle, quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt);
                                                quizAnswerSubmit.ResultScore.Description = VariableLinking(result.Description, quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt);
                                                quizAnswerSubmit.ResultScore.AutoPlay = result.AutoPlay;
                                                quizAnswerSubmit.ResultScore.SecondsToApply = result.SecondsToApply;
                                                quizAnswerSubmit.ResultScore.VideoFrameEnabled = result.VideoFrameEnabled;
                                                quizAnswerSubmit.ResultScore.DisplayOrderForTitle = result.DisplayOrderForTitle;
                                                quizAnswerSubmit.ResultScore.DisplayOrderForTitleImage = result.DisplayOrderForTitleImage;
                                                quizAnswerSubmit.ResultScore.DisplayOrderForDescription = result.DisplayOrderForDescription;
                                                quizAnswerSubmit.ResultScore.DisplayOrderForNextButton = result.DisplayOrderForNextButton;


                                                quizStatsObj = quizAttemptsObj.QuizStats.FirstOrDefault();
                                                if (quizStatsObj != null)
                                                {
                                                    quizStatsObj.ResultId = result.Id;
                                                    UOWObj.QuizStatsRepository.Update(quizStatsObj);
                                                    UOWObj.Save();
                                                }

                                                #endregion
                                            }
                                            else
                                            {
                                                #region Personality result Setting
                                                var personalityAnswerResultList = attemptedQuestions.SelectMany(x => x.QuizAnswerStats.SelectMany(r => r.AnswerOptionsInQuizQuestions.PersonalityAnswerResultMapping));
                                                var personalitySetting = quizDetails.PersonalityResultSetting.FirstOrDefault();
                                                if (personalitySetting != null && personalitySetting.Status == (int)StatusEnum.Active)
                                                {
                                                    #region personality Setting Active
                                                    var personalityResultList = new List<QuizAnswerSubmit.PersonalityResult>();
                                                    var countResult = 0;
                                                    quizStatsObj = quizAttemptsObj.QuizStats.FirstOrDefault();

                                                    if (personalityAnswerResultList.Any())
                                                    {
                                                        #region correlation is available
                                                        var mappedResultList = personalityAnswerResultList.OrderBy(x => x.QuizResults.DisplayOrder).GroupBy(r => r.ResultId).OrderByDescending(i => i.Count()).Take(personalitySetting.MaxResult).Select(x => x.Key).ToList();

                                                        var quizresultList = quizDetails.QuizResults.Where(r => !r.IsPersonalityCorrelatedResult && mappedResultList.Contains(r.Id)).OrderBy(x => mappedResultList.IndexOf(x.Id));

                                                        foreach (var quizresult in quizresultList)
                                                        {
                                                            QuizAnswerSubmit.PersonalityResult personalityresult = new QuizAnswerSubmit.PersonalityResult();
                                                            personalityresult.Title = VariableLinking(quizresult.Title, quizDetails, quizAttemptsObj, false, false, null);
                                                            personalityresult.InternalTitle = VariableLinking(quizresult.InternalTitle, quizDetails, quizAttemptsObj, false, false, null);
                                                            personalityresult.Image = quizresult.Image;
                                                            personalityresult.ResultId = quizresult.Id;
                                                            personalityresult.GraphColor = personalitySetting.GraphColor;
                                                            personalityresult.ButtonColor = personalitySetting.ButtonColor;
                                                            personalityresult.ButtonFontColor = personalitySetting.ButtonFontColor;
                                                            personalityresult.MaxResult = personalitySetting.MaxResult;
                                                            personalityresult.SideButtonText = personalitySetting.SideButtonText;
                                                            personalityresult.IsFullWidthEnable = personalitySetting.IsFullWidthEnable;
                                                            personalityresult.Description = VariableLinking(quizresult.Description, quizDetails, quizAttemptsObj, false, false, null);
                                                            int count = personalityAnswerResultList.Where(k => k.ResultId == quizresult.Id).Count();
                                                            personalityresult.Percentage = (int)Math.Round((double)(100 * count) / personalityAnswerResultList.Count());
                                                            personalityresult.ShowInternalTitle = quizresult.ShowInternalTitle;
                                                            personalityresult.ShowExternalTitle = quizresult.ShowExternalTitle;
                                                            personalityresult.ShowDescription = quizresult.ShowDescription;
                                                            personalityResultList.Add(personalityresult);

                                                            if (quizStatsObj != null)
                                                            {
                                                                if (countResult == 0)
                                                                {
                                                                    quizStatsObj.ResultId = quizresult.Id;
                                                                    UOWObj.QuizStatsRepository.Update(quizStatsObj);
                                                                }
                                                                else
                                                                {
                                                                    var resultQuizStats = new Db.QuizStats();
                                                                    resultQuizStats.StartedOn = quizStatsObj.StartedOn;
                                                                    resultQuizStats.QuizAttemptId = quizStatsObj.QuizAttemptId;
                                                                    resultQuizStats.ResultId = quizresult.Id;
                                                                    UOWObj.QuizStatsRepository.Insert(resultQuizStats);
                                                                }
                                                                UOWObj.Save();
                                                                countResult++;
                                                            }
                                                        }

                                                        quizAnswerSubmit.ShowLeadUserForm = (Mode == "PREVIEW" || Mode == "PREVIEWTEMPLATE") ? false : (resultIdList.Any() ? (resultIdList.Any(r => quizresultList.Any(l => l.Id == r) || quizComponentLogsList.Any(q => q.DraftedObjectId == r && quizresultList.Any(l => l.Id == q.PublishedObjectId)))) : (quizAttemptsObj.LeadUserId == null && quizAttemptsObj.RecruiterUserId == null && ((quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Personality && quizDetails.PersonalityResultSetting.FirstOrDefault().Status == (int)StatusEnum.Active) ? quizDetails.PersonalityResultSetting.FirstOrDefault().ShowLeadUserForm : quizresultList.Any(r => r.ShowLeadUserForm))));

                                                        if (leadFormDetailofResultsLst != null && leadFormDetailofResultsLst.Any() && quizAnswerSubmit.ShowLeadUserForm)
                                                        {
                                                            var personalityResult = personalityResultList.OrderByDescending(r => r.Percentage.Value).FirstOrDefault();
                                                            if (leadFormDetailofResultsLst.Any(r => r.ResultId == personalityResult.ResultId || quizComponentLogsList.Any(q => q.DraftedObjectId == r.ResultId && q.PublishedObjectId == personalityResult.ResultId)))
                                                            {
                                                                var leadFormDetailofResultsObj = leadFormDetailofResultsLst.FirstOrDefault(r => r.ResultId == personalityResult.ResultId || quizComponentLogsList.Any(q => q.DraftedObjectId == r.ResultId && q.PublishedObjectId == personalityResult.ResultId));
                                                                quizAnswerSubmit.FormId = leadFormDetailofResultsObj.FormId;
                                                                quizAnswerSubmit.FlowOrder = leadFormDetailofResultsObj.FlowOrder;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            quizAnswerSubmit.FormId = null;
                                                            quizAnswerSubmit.FlowOrder = null;
                                                        }

                                                        #endregion
                                                    }
                                                    else
                                                    {
                                                        #region correlation is not available

                                                        var quizresultList = quizDetails.QuizResults.OrderBy(x => x.DisplayOrder).Take(personalitySetting.MaxResult);
                                                        foreach (var quizresult in quizresultList)
                                                        {
                                                            QuizAnswerSubmit.PersonalityResult personalityresult = new QuizAnswerSubmit.PersonalityResult();
                                                            personalityresult.Title = VariableLinking(quizresult.Title, quizDetails, quizAttemptsObj, false, false, null);
                                                            personalityresult.InternalTitle = VariableLinking(quizresult.InternalTitle, quizDetails, quizAttemptsObj, false, false, null);
                                                            personalityresult.Image = quizresult.Image;
                                                            personalityresult.ResultId = quizresult.Id;
                                                            personalityresult.GraphColor = personalitySetting.GraphColor;
                                                            personalityresult.ButtonColor = personalitySetting.ButtonColor;
                                                            personalityresult.ButtonFontColor = personalitySetting.ButtonFontColor;
                                                            personalityresult.MaxResult = personalitySetting.MaxResult;
                                                            personalityresult.SideButtonText = personalitySetting.SideButtonText;
                                                            personalityresult.IsFullWidthEnable = personalitySetting.IsFullWidthEnable;
                                                            personalityresult.Description = VariableLinking(quizresult.Description, quizDetails, quizAttemptsObj, false, false, null);
                                                            personalityresult.Percentage = null;
                                                            personalityresult.ShowInternalTitle = quizresult.ShowInternalTitle;
                                                            personalityresult.ShowExternalTitle = quizresult.ShowExternalTitle;
                                                            personalityresult.ShowDescription = quizresult.ShowDescription;
                                                            personalityResultList.Add(personalityresult);

                                                            if (quizStatsObj != null)
                                                            {
                                                                if (countResult == 0)
                                                                {
                                                                    quizStatsObj.ResultId = quizresult.Id;
                                                                    UOWObj.QuizStatsRepository.Update(quizStatsObj);
                                                                }
                                                                else
                                                                {
                                                                    var resultQuizStats = new Db.QuizStats();
                                                                    resultQuizStats.StartedOn = quizStatsObj.StartedOn;
                                                                    resultQuizStats.QuizAttemptId = quizStatsObj.QuizAttemptId;
                                                                    resultQuizStats.ResultId = quizresult.Id;
                                                                    UOWObj.QuizStatsRepository.Insert(resultQuizStats);
                                                                }
                                                                UOWObj.Save();
                                                                countResult++;
                                                            }
                                                        }

                                                        quizAnswerSubmit.ShowLeadUserForm = (Mode == "PREVIEW" || Mode == "PREVIEWTEMPLATE") ? false : (resultIdList.Any() ? (resultIdList.Any(r => quizresultList.Any(l => l.Id == r) || quizComponentLogsList.Any(q => q.DraftedObjectId == r && quizresultList.Any(l => l.Id == q.PublishedObjectId)))) : (quizAttemptsObj.LeadUserId == null && quizAttemptsObj.RecruiterUserId == null && ((quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Personality && quizDetails.PersonalityResultSetting.FirstOrDefault().Status == (int)StatusEnum.Active) ? quizDetails.PersonalityResultSetting.FirstOrDefault().ShowLeadUserForm : quizresultList.Any(r => r.ShowLeadUserForm))));

                                                        if (leadFormDetailofResultsLst != null && leadFormDetailofResultsLst.Any() && quizAnswerSubmit.ShowLeadUserForm)
                                                        {
                                                            var personalityResult = personalityResultList.FirstOrDefault();
                                                            if (leadFormDetailofResultsLst.Any(r => r.ResultId == personalityResult.ResultId || quizComponentLogsList.Any(q => q.DraftedObjectId == r.ResultId && q.PublishedObjectId == personalityResult.ResultId)))
                                                            {
                                                                var leadFormDetailofResultsObj = leadFormDetailofResultsLst.FirstOrDefault(r => r.ResultId == personalityResult.ResultId || quizComponentLogsList.Any(q => q.DraftedObjectId == r.ResultId && q.PublishedObjectId == personalityResult.ResultId));
                                                                quizAnswerSubmit.FormId = leadFormDetailofResultsObj.FormId;
                                                                quizAnswerSubmit.FlowOrder = leadFormDetailofResultsObj.FlowOrder;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            quizAnswerSubmit.FormId = null;
                                                            quizAnswerSubmit.FlowOrder = null;
                                                        }

                                                        #endregion
                                                    }
                                                    quizAnswerSubmit.ResultScore = quizAnswerSubmit.ResultScore ?? new QuizAnswerSubmit.Result();
                                                    quizAnswerSubmit.ResultScore.ShowInternalTitle = true;
                                                    quizAnswerSubmit.ResultScore.ShowExternalTitle = true;
                                                    quizAnswerSubmit.ResultScore.ShowDescription = true;
                                                    quizAnswerSubmit.ResultScore.Title = personalitySetting.Title;
                                                    quizAnswerSubmit.ResultScore.PersonalityResultList = personalityResultList.OrderByDescending(x => x.Percentage).ToList();
                                                    #endregion
                                                }
                                                else
                                                {
                                                    #region if there is correlation available but personalitySetting is disabled

                                                    if (result.IsPersonalityCorrelatedResult && personalityAnswerResultList.Any())
                                                        result = quizDetails.QuizResults.FirstOrDefault(x => x.Id == personalityAnswerResultList.OrderBy(r => r.QuizResults.DisplayOrder).GroupBy(r => r.ResultId).OrderByDescending(i => i.Count()).First().Key);

                                                    quizAnswerSubmit.ShowLeadUserForm = (Mode == "PREVIEW" || Mode == "PREVIEWTEMPLATE") ? false : (resultIdList.Any() ? (resultIdList.Any(r => r == result.Id || quizComponentLogsList.Any(q => q.DraftedObjectId == r && q.PublishedObjectId == result.Id))) : (quizAttemptsObj.LeadUserId == null && quizAttemptsObj.RecruiterUserId == null && ((quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Personality && quizDetails.PersonalityResultSetting.FirstOrDefault().Status == (int)StatusEnum.Active) ? quizDetails.PersonalityResultSetting.FirstOrDefault().ShowLeadUserForm : result.ShowLeadUserForm)));

                                                    if (leadFormDetailofResultsLst != null && leadFormDetailofResultsLst.Any() && quizAnswerSubmit.ShowLeadUserForm)
                                                    {
                                                        var leadFormDetailofResultsObj = leadFormDetailofResultsLst.FirstOrDefault(r => r.ResultId == result.Id || quizComponentLogsList.Any(q => q.DraftedObjectId == r.ResultId && q.PublishedObjectId == result.Id));
                                                        quizAnswerSubmit.FormId = leadFormDetailofResultsObj.FormId;
                                                        quizAnswerSubmit.FlowOrder = leadFormDetailofResultsObj.FlowOrder;
                                                    }

                                                    quizAnswerSubmit.ResultScore = new QuizAnswerSubmit.Result();

                                                    if (result.EnableMediaFile && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.RESULT && r.ObjectId == result.Id))
                                                    {
                                                        var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.RESULT && r.ObjectId == result.Id);
                                                        quizAnswerSubmit.ResultScore.Image = result.ShowResultImage ? mediaObj.ObjectValue : string.Empty;
                                                        quizAnswerSubmit.ResultScore.PublicIdForResult = result.ShowResultImage ? mediaObj.ObjectPublicId : string.Empty;

                                                        var newMedia = result.ShowResultImage ? ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers) : null;
                                                        if (newMedia != null)
                                                        {
                                                            quizAnswerSubmit.ResultScore.Image = newMedia.MediaUrl;
                                                            quizAnswerSubmit.ResultScore.PublicIdForResult = newMedia.MediaPublicId;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        quizAnswerSubmit.ResultScore.Image = result.ShowResultImage ? result.Image : string.Empty;
                                                        quizAnswerSubmit.ResultScore.PublicIdForResult = result.ShowResultImage ? result.PublicId : string.Empty;
                                                    }
                                                    quizAnswerSubmit.ResultScore.HideActionButton = result.HideCallToAction.HasValue ? result.HideCallToAction.Value : false;
                                                    quizAnswerSubmit.ResultScore.ActionButtonURL = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonURL;
                                                    quizAnswerSubmit.ResultScore.OpenLinkInNewTab = quizAnswerSubmit.ResultScore.HideActionButton ? false : result.OpenLinkInNewTab;
                                                    quizAnswerSubmit.ResultScore.ActionButtonTxtSize = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonTxtSize;
                                                    quizAnswerSubmit.ResultScore.ActionButtonColor = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonColor;
                                                    quizAnswerSubmit.ResultScore.ActionButtonTitleColor = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonTitleColor;
                                                    quizAnswerSubmit.ResultScore.ActionButtonText = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonText;

                                                    var resultSetting = quizDetails.ResultSettings.FirstOrDefault();
                                                    if (resultSetting != null)
                                                    {
                                                        quizAnswerSubmit.ResultScore.ResultScoreValueTxt = string.Empty;
                                                        quizAnswerSubmit.ResultScore.ShowScoreValue = false;
                                                        if (resultSetting.ShowCorrectAnswer.HasValue && resultSetting.ShowCorrectAnswer.Value)
                                                        {
                                                            quizAnswerSubmit.ResultScore.AnswerKeyCustomTxt = resultSetting.CustomTxtForAnswerKey;
                                                            quizAnswerSubmit.ResultScore.YourAnswerCustomTxt = resultSetting.CustomTxtForYourAnswer;
                                                            quizAnswerSubmit.ResultScore.CorrectAnswerCustomTxt = resultSetting.CustomTxtForCorrectAnswer;
                                                            quizAnswerSubmit.ResultScore.ExplanationCustomTxt = resultSetting.CustomTxtForExplanation;
                                                            quizAnswerSubmit.ResultScore.ShowCorrectAnswer = true;
                                                            quizAnswerSubmit.ResultScore.AttemptedQuestionAnswerDetails = new List<QuizAnswerSubmit.AnswerResult>();

                                                            foreach (var item in attemptedQuestions)
                                                            {
                                                                quizAnswerSubmit.ResultScore.AttemptedQuestionAnswerDetails.Add(new QuizAnswerSubmit.AnswerResult
                                                                {
                                                                    Question = VariableLinking(item.QuestionsInQuiz.Question, quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, string.Empty),
                                                                    YourAnswer = (item.QuizAnswerStats != null && item.QuizAnswerStats.Any(r => !r.AnswerOptionsInQuizQuestions.IsUnansweredType)) ? VariableLinking(string.Join(",", item.QuizAnswerStats.Where(t => !t.AnswerOptionsInQuizQuestions.IsUnansweredType).Select(t => t.AnswerOptionsInQuizQuestions.Option).Select(s => "'" + s + "'")), quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, string.Empty) : notAttemptedQuesText,
                                                                    CorrectAnswer = string.Empty,
                                                                    IsCorrect = null,
                                                                    AnswerExplanation = item.QuestionsInQuiz.CorrectAnswerDescription,
                                                                    AssociatedScore = default(int)
                                                                });
                                                            }
                                                        }
                                                        else
                                                        {
                                                            quizAnswerSubmit.ResultScore.ShowCorrectAnswer = false;
                                                        }
                                                    }

                                                    quizAnswerSubmit.ResultScore.ShowInternalTitle = result.ShowInternalTitle;
                                                    quizAnswerSubmit.ResultScore.ShowExternalTitle = result.ShowExternalTitle;
                                                    quizAnswerSubmit.ResultScore.ShowDescription = result.ShowDescription;
                                                    quizAnswerSubmit.ResultScore.Title = VariableLinking(result.Title, quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, string.Empty);
                                                    quizAnswerSubmit.ResultScore.InternalTitle = VariableLinking(result.InternalTitle, quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, string.Empty);
                                                    quizAnswerSubmit.ResultScore.Description = VariableLinking(result.Description, quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, string.Empty);
                                                    quizAnswerSubmit.ResultScore.AutoPlay = result.AutoPlay;
                                                    quizAnswerSubmit.ResultScore.SecondsToApply = result.SecondsToApply;
                                                    quizAnswerSubmit.ResultScore.VideoFrameEnabled = result.VideoFrameEnabled;
                                                    quizAnswerSubmit.ResultScore.DisplayOrderForTitle = result.DisplayOrderForTitle;
                                                    quizAnswerSubmit.ResultScore.DisplayOrderForTitleImage = result.DisplayOrderForTitleImage;
                                                    quizAnswerSubmit.ResultScore.DisplayOrderForDescription = result.DisplayOrderForDescription;
                                                    quizAnswerSubmit.ResultScore.DisplayOrderForNextButton = result.DisplayOrderForNextButton;

                                                    quizStatsObj = quizAttemptsObj.QuizStats.FirstOrDefault();
                                                    if (quizStatsObj != null)
                                                    {
                                                        quizStatsObj.ResultId = result.Id;
                                                        UOWObj.QuizStatsRepository.Update(quizStatsObj);
                                                        UOWObj.Save();
                                                    }

                                                    #endregion
                                                }
                                                #endregion
                                            }

                                            var nextObj = FetchNextQuestion(quizDetails, result.Id, (int)BranchingLogicEnum.RESULTNEXT, UOWObj, isQuesAndContentInSameTable);

                                            #region if next is Badge type

                                            if (nextObj != null && nextObj.GetType().BaseType.Name == "BadgesInQuiz")
                                            {
                                                quizAnswerSubmit.BadgeDetails = new QuizBadge();

                                                quizAnswerSubmit.BadgeDetails.Id = ((Db.BadgesInQuiz)nextObj).Id;
                                                quizAnswerSubmit.BadgeDetails.Title = VariableLinking(((Db.BadgesInQuiz)nextObj).Title, quizDetails, quizAttemptsObj, false, false, null);
                                                quizAnswerSubmit.BadgeDetails.ShowTitle = ((Db.BadgesInQuiz)nextObj).ShowTitle;
                                                quizAnswerSubmit.BadgeDetails.AutoPlay = ((Db.BadgesInQuiz)nextObj).AutoPlay;
                                                quizAnswerSubmit.BadgeDetails.SecondsToApply = ((Db.BadgesInQuiz)nextObj).SecondsToApply;
                                                quizAnswerSubmit.BadgeDetails.VideoFrameEnabled = ((Db.BadgesInQuiz)nextObj).VideoFrameEnabled;
                                                quizAnswerSubmit.BadgeDetails.DisplayOrderForTitleImage = ((Db.BadgesInQuiz)nextObj).DisplayOrderForTitleImage;
                                                quizAnswerSubmit.BadgeDetails.DisplayOrderForTitle = ((Db.BadgesInQuiz)nextObj).DisplayOrderForTitle;
                                                quizAnswerSubmit.BadgeDetails.ShowImage = ((Db.BadgesInQuiz)nextObj).ShowImage;

                                                if (((Db.BadgesInQuiz)nextObj).EnableMediaFile && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.BADGE && r.ObjectId == ((Db.BadgesInQuiz)nextObj).Id))
                                                {
                                                    var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.BADGE && r.ObjectId == ((Db.BadgesInQuiz)nextObj).Id);
                                                    quizAnswerSubmit.BadgeDetails.Image = mediaObj.ObjectValue;
                                                    quizAnswerSubmit.BadgeDetails.PublicIdForBadge = mediaObj.ObjectPublicId ?? string.Empty;

                                                    var newMedia = ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers);
                                                    if (newMedia != null)
                                                    {
                                                        quizAnswerSubmit.BadgeDetails.Image = newMedia.MediaUrl;
                                                        quizAnswerSubmit.BadgeDetails.PublicIdForBadge = newMedia.MediaPublicId;
                                                    }
                                                }
                                                else
                                                {
                                                    quizAnswerSubmit.BadgeDetails.Image = ((Db.BadgesInQuiz)nextObj).Image ?? string.Empty;
                                                    quizAnswerSubmit.BadgeDetails.PublicIdForBadge = ((Db.BadgesInQuiz)nextObj).PublicId ?? string.Empty;
                                                }

                                                var BadgeStatsObj = new Db.QuizObjectStats();

                                                BadgeStatsObj.QuizAttemptId = quizAttemptsObj.Id;
                                                BadgeStatsObj.ObjectId = ((Db.BadgesInQuiz)nextObj).Id;
                                                BadgeStatsObj.ViewedOn = currentDate;
                                                BadgeStatsObj.TypeId = (int)BranchingLogicEnum.BADGE;
                                                BadgeStatsObj.Status = (int)StatusEnum.Active;

                                                UOWObj.QuizObjectStatsRepository.Insert(BadgeStatsObj);
                                                UOWObj.Save();

                                                if (quizAttemptsObj.RecruiterUserId.HasValue && quizDetails.Quiz.Company.BadgesEnabled)
                                                {
                                                    var badgesInfo = badgesInfoUpdateJson.Replace("{UserId}", quizAttemptsObj.RecruiterUserId.Value.ToString()).Replace("{CourseId}", quizDetails.ParentQuizId.ToString()).Replace("{CourseBadgeName}", ((Db.BadgesInQuiz)nextObj).Title).Replace("{CourseBadgeImageUrl}", quizAnswerSubmit.BadgeDetails.Image).Replace("{CourseTitle}", quizDetails.QuizTitle);
                                                    var user = UOWObj.UserTokensRepository.Get(r => r.BusinessUserId == quizAttemptsObj.RecruiterUserId.Value).FirstOrDefault();
                                                    var company = quizAttemptsObj.CompanyId > 0 ? quizAttemptsObj.Company : user.Company;
                                                    try
                                                    {
                                                        var apiSuccess = OWCHelper.UpdateRecruiterCourseBadgesInfo(badgesInfo, company);
                                                        if (!apiSuccess)
                                                            AddPendingApi(company.JobRocketApiUrl + (GlobalSettings.owcAddRecruiterCourseUrl.ToString()).Replace("{ClientCode}", company.ClientCode), company.JobRocketApiAuthorizationBearer, badgesInfo, "POST");
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        AddPendingApi(company.JobRocketApiUrl + (GlobalSettings.owcAddRecruiterCourseUrl.ToString()).Replace("{ClientCode}", company.ClientCode), company.JobRocketApiAuthorizationBearer, badgesInfo, "POST");
                                                    }
                                                }
                                            }
                                            #endregion
                                        }

                                        #endregion

                                        #region if next is content type

                                        if (nextQuestionObj.GetType().BaseType.Name == "ContentsInQuiz")
                                        {
                                            quizAnswerSubmit.QuestionType = (int)BranchingLogicEnum.CONTENT;

                                            quizAnswerSubmit.IsBackButtonEnable = (question.RevealCorrectAnswer.HasValue && question.RevealCorrectAnswer.Value) ? false : (question.ViewPreviousQuestion || question.EditAnswer);

                                            if (quizAttemptsObj.QuizObjectStats.Any(a => a.Status == (int)StatusEnum.Active && a.TypeId == (int)BranchingLogicEnum.CONTENT && a.ObjectId == ((Db.ContentsInQuiz)nextQuestionObj).Id))
                                            {
                                                QuestionId = ((Db.ContentsInQuiz)nextQuestionObj).Id;
                                                Type = "complete_content";
                                                goto switchcase;
                                            };


                                            quizAnswerSubmit.ContentDetails = new QuizContent();

                                            quizAnswerSubmit.ContentDetails.Id = ((Db.ContentsInQuiz)nextQuestionObj).Id;
                                            quizAnswerSubmit.ContentDetails.ContentTitle = VariableLinking(((Db.ContentsInQuiz)nextQuestionObj).ContentTitle, quizDetails, quizAttemptsObj, false, false, null);
                                            quizAnswerSubmit.ContentDetails.ContentDescription = VariableLinking(((Db.ContentsInQuiz)nextQuestionObj).ContentDescription, quizDetails, quizAttemptsObj, false, false, null);
                                            quizAnswerSubmit.ContentDetails.ShowDescription = ((Db.ContentsInQuiz)nextQuestionObj).ShowDescription;

                                            if ((((Db.ContentsInQuiz)nextQuestionObj).EnableMediaFileForTitle && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Title && r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == ((Db.ContentsInQuiz)nextQuestionObj).Id)))
                                            {
                                                var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Title && r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == ((Db.ContentsInQuiz)nextQuestionObj).Id);
                                                quizAnswerSubmit.ContentDetails.ContentTitleImage = mediaObj.ObjectValue;
                                                quizAnswerSubmit.ContentDetails.PublicIdForContentTitle = mediaObj.ObjectPublicId ?? string.Empty;

                                                var newMedia = ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers);
                                                if (newMedia != null)
                                                {
                                                    quizAnswerSubmit.ContentDetails.ContentTitleImage = newMedia.MediaUrl;
                                                    quizAnswerSubmit.ContentDetails.PublicIdForContentTitle = newMedia.MediaPublicId;
                                                }
                                            }
                                            else
                                            {
                                                quizAnswerSubmit.ContentDetails.ContentTitleImage = ((Db.ContentsInQuiz)nextQuestionObj).ContentTitleImage ?? string.Empty;
                                                quizAnswerSubmit.ContentDetails.PublicIdForContentTitle = ((Db.ContentsInQuiz)nextQuestionObj).PublicIdForContentTitle ?? string.Empty;
                                            }

                                            quizAnswerSubmit.ContentDetails.ShowContentTitleImage = ((Db.ContentsInQuiz)nextQuestionObj).ShowContentTitleImage.HasValue && ((Db.ContentsInQuiz)nextQuestionObj).ShowContentTitleImage.Value ? ((Db.ContentsInQuiz)nextQuestionObj).ShowContentTitleImage.Value : false;
                                            quizAnswerSubmit.ContentDetails.AliasTextForNextButton = ((Db.ContentsInQuiz)nextQuestionObj).AliasTextForNextButton;
                                            quizAnswerSubmit.ContentDetails.EnableNextButton = ((Db.ContentsInQuiz)nextQuestionObj).EnableNextButton;

                                            if (((Db.ContentsInQuiz)nextQuestionObj).EnableMediaFileForDescription && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Description && r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == ((Db.ContentsInQuiz)nextQuestionObj).Id))
                                            {
                                                var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Description && r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == ((Db.ContentsInQuiz)nextQuestionObj).Id);
                                                quizAnswerSubmit.ContentDetails.ContentDescriptionImage = mediaObj.ObjectValue ?? string.Empty;
                                                quizAnswerSubmit.ContentDetails.PublicIdForContentDescription = mediaObj.ObjectPublicId ?? string.Empty;

                                                var newMedia = ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers);
                                                if (newMedia != null)
                                                {
                                                    quizAnswerSubmit.ContentDetails.ContentDescriptionImage = newMedia.MediaUrl;
                                                    quizAnswerSubmit.ContentDetails.PublicIdForContentDescription = newMedia.MediaPublicId;
                                                }
                                            }
                                            else
                                            {
                                                quizAnswerSubmit.ContentDetails.ContentDescriptionImage = ((Db.ContentsInQuiz)nextQuestionObj).ContentDescriptionImage ?? string.Empty;
                                                quizAnswerSubmit.ContentDetails.PublicIdForContentDescription = ((Db.ContentsInQuiz)nextQuestionObj).PublicIdForContentDescription ?? string.Empty;
                                            }

                                            quizAnswerSubmit.ContentDetails.PublicIdForContentDescription = ((Db.ContentsInQuiz)nextQuestionObj).PublicIdForContentDescription ?? string.Empty;
                                            quizAnswerSubmit.ContentDetails.ShowContentDescriptionImage = ((Db.ContentsInQuiz)nextQuestionObj).ShowContentDescriptionImage.HasValue && ((Db.ContentsInQuiz)nextQuestionObj).ShowContentDescriptionImage.Value ? ((Db.ContentsInQuiz)nextQuestionObj).ShowContentDescriptionImage.Value : false;
                                            quizAnswerSubmit.ContentDetails.ViewPreviousQuestion = ((Db.ContentsInQuiz)nextQuestionObj).ViewPreviousQuestion;
                                            quizAnswerSubmit.ContentDetails.AutoPlay = ((Db.ContentsInQuiz)nextQuestionObj).AutoPlay;
                                            quizAnswerSubmit.ContentDetails.SecondsToApply = ((Db.ContentsInQuiz)nextQuestionObj).SecondsToApply ?? "0";
                                            quizAnswerSubmit.ContentDetails.VideoFrameEnabled = ((Db.ContentsInQuiz)nextQuestionObj).VideoFrameEnabled ?? false;
                                            quizAnswerSubmit.ContentDetails.AutoPlayForDescription = ((Db.ContentsInQuiz)nextQuestionObj).AutoPlayForDescription;
                                            quizAnswerSubmit.ContentDetails.SecondsToApplyForDescription = ((Db.ContentsInQuiz)nextQuestionObj).SecondsToApplyForDescription ?? "0";
                                            quizAnswerSubmit.ContentDetails.DescVideoFrameEnabled = ((Db.ContentsInQuiz)nextQuestionObj).DescVideoFrameEnabled ?? false;
                                            quizAnswerSubmit.ContentDetails.ShowTitle = ((Db.ContentsInQuiz)nextQuestionObj).ShowTitle;
                                            quizAnswerSubmit.ContentDetails.DisplayOrderForTitle = ((Db.ContentsInQuiz)nextQuestionObj).DisplayOrderForTitle;
                                            quizAnswerSubmit.ContentDetails.DisplayOrderForTitleImage = ((Db.ContentsInQuiz)nextQuestionObj).DisplayOrderForTitleImage;
                                            quizAnswerSubmit.ContentDetails.DisplayOrderForDescription = ((Db.ContentsInQuiz)nextQuestionObj).DisplayOrderForDescription;
                                            quizAnswerSubmit.ContentDetails.DisplayOrderForDescriptionImage = ((Db.ContentsInQuiz)nextQuestionObj).DisplayOrderForDescriptionImage;
                                            quizAnswerSubmit.ContentDetails.DisplayOrderForNextButton = ((Db.ContentsInQuiz)nextQuestionObj).DisplayOrderForNextButton;
                                        }
                                        #endregion

                                        #region if next is Badge type

                                        if (nextQuestionObj.GetType().BaseType.Name == "BadgesInQuiz")
                                        {
                                            quizAnswerSubmit.QuestionType = (int)BranchingLogicEnum.BADGE;

                                            quizAnswerSubmit.IsBackButtonEnable = false;

                                            quizAnswerSubmit.BadgeDetails = new QuizBadge();

                                            quizAnswerSubmit.BadgeDetails.Id = ((Db.BadgesInQuiz)nextQuestionObj).Id;
                                            quizAnswerSubmit.BadgeDetails.Title = VariableLinking(((Db.BadgesInQuiz)nextQuestionObj).Title, quizDetails, quizAttemptsObj, false, false, null);
                                            quizAnswerSubmit.BadgeDetails.ShowTitle = ((Db.BadgesInQuiz)nextQuestionObj).ShowTitle;
                                            quizAnswerSubmit.BadgeDetails.AutoPlay = ((Db.BadgesInQuiz)nextQuestionObj).AutoPlay;
                                            quizAnswerSubmit.BadgeDetails.SecondsToApply = ((Db.BadgesInQuiz)nextQuestionObj).SecondsToApply;
                                            quizAnswerSubmit.BadgeDetails.VideoFrameEnabled = ((Db.BadgesInQuiz)nextQuestionObj).VideoFrameEnabled;
                                            quizAnswerSubmit.BadgeDetails.DisplayOrderForTitleImage = ((Db.BadgesInQuiz)nextQuestionObj).DisplayOrderForTitleImage;
                                            quizAnswerSubmit.BadgeDetails.DisplayOrderForTitle = ((Db.BadgesInQuiz)nextQuestionObj).DisplayOrderForTitle;
                                            quizAnswerSubmit.BadgeDetails.ShowImage = ((Db.BadgesInQuiz)nextQuestionObj).ShowImage;

                                            if (((Db.BadgesInQuiz)nextQuestionObj).EnableMediaFile && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.BADGE && r.ObjectId == ((Db.BadgesInQuiz)nextQuestionObj).Id))
                                            {
                                                var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.BADGE && r.ObjectId == ((Db.BadgesInQuiz)nextQuestionObj).Id);
                                                quizAnswerSubmit.BadgeDetails.Image = mediaObj.ObjectValue;
                                                quizAnswerSubmit.BadgeDetails.PublicIdForBadge = mediaObj.ObjectPublicId ?? string.Empty;

                                                var newMedia = ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers);
                                                if (newMedia != null)
                                                {
                                                    quizAnswerSubmit.BadgeDetails.Image = newMedia.MediaUrl;
                                                    quizAnswerSubmit.BadgeDetails.PublicIdForBadge = newMedia.MediaPublicId;
                                                }
                                            }
                                            else
                                            {
                                                quizAnswerSubmit.BadgeDetails.Image = ((Db.BadgesInQuiz)nextQuestionObj).Image ?? string.Empty;
                                                quizAnswerSubmit.BadgeDetails.PublicIdForBadge = ((Db.BadgesInQuiz)nextQuestionObj).PublicId ?? string.Empty;
                                            }

                                            var BadgeStatsObj = new Db.QuizObjectStats();

                                            BadgeStatsObj.QuizAttemptId = quizAttemptsObj.Id;
                                            BadgeStatsObj.ObjectId = ((Db.BadgesInQuiz)nextQuestionObj).Id;
                                            BadgeStatsObj.ViewedOn = currentDate;
                                            BadgeStatsObj.TypeId = (int)BranchingLogicEnum.BADGE;
                                            BadgeStatsObj.Status = (int)StatusEnum.Active;

                                            UOWObj.QuizObjectStatsRepository.Insert(BadgeStatsObj);
                                            UOWObj.Save();

                                            if (quizAttemptsObj.RecruiterUserId.HasValue && quizDetails.Quiz.Company.BadgesEnabled)
                                            {
                                                var badgesInfo = badgesInfoUpdateJson.Replace("{UserId}", quizAttemptsObj.RecruiterUserId.Value.ToString()).Replace("{CourseId}", quizDetails.ParentQuizId.ToString()).Replace("{CourseBadgeName}", ((Db.BadgesInQuiz)nextQuestionObj).Title).Replace("{CourseBadgeImageUrl}", quizAnswerSubmit.BadgeDetails.Image).Replace("{CourseTitle}", quizDetails.QuizTitle);
                                                var user = UOWObj.UserTokensRepository.Get(r => r.BusinessUserId == quizAttemptsObj.RecruiterUserId.Value).FirstOrDefault();
                                                var company = quizAttemptsObj.CompanyId > 0 ? quizAttemptsObj.Company : user.Company;
                                                try
                                                {
                                                    var apiSuccess = OWCHelper.UpdateRecruiterCourseBadgesInfo(badgesInfo, company);
                                                    if (!apiSuccess)
                                                        AddPendingApi(company.JobRocketApiUrl + (GlobalSettings.owcAddRecruiterCourseUrl.ToString()).Replace("{ClientCode}", company.ClientCode), company.JobRocketApiAuthorizationBearer, badgesInfo, "POST");
                                                }
                                                catch (Exception ex)
                                                {
                                                    AddPendingApi(company.JobRocketApiUrl + (GlobalSettings.owcAddRecruiterCourseUrl.ToString()).Replace("{ClientCode}", company.ClientCode), company.JobRocketApiAuthorizationBearer, badgesInfo, "POST");
                                                }
                                            }

                                        }
                                        #endregion
                                    }
                                    else
                                    {
                                        #region show last result

                                        quizAnswerSubmit.IsBackButtonEnable = false;
                                        quizAnswerSubmit.CompanyCode = quizDetails.Quiz.Company.ClientCode;
                                        var IsResultInquizBranching = false;
                                        if (quizDetails.IsBranchingLogicEnabled.HasValue && quizDetails.IsBranchingLogicEnabled.Value)
                                            IsResultInquizBranching = quizDetails.BranchingLogic.Where(r => r.DestinationTypeId == (int)BranchingLogicEnum.RESULT).Any() ? true : false;

                                        quizAnswerSubmit.ResultScore = new QuizAnswerSubmit.Result();

                                        var resultSetting = quizDetails.ResultSettings.FirstOrDefault();

                                        if (resultSetting != null && !IsResultInquizBranching)
                                        {
                                            #region result Setting is not null                                           
                                            var attemptedQuestions = quizAttemptsObj.QuizQuestionStats.Where(t => t.Status == (int)StatusEnum.Active && t.CompletedOn != null && (t.QuestionsInQuiz.TimerRequired ? true : t.QuizAnswerStats.Any(r => !r.AnswerOptionsInQuizQuestions.IsUnansweredType)));

                                            if (quizDetails.Quiz.QuizType != (int)QuizTypeEnum.Personality && quizDetails.Quiz.QuizType != (int)QuizTypeEnum.PersonalityTemplate)
                                            {
                                                #region non personality result Setting

                                                string scoreValueTxt = string.Empty;
                                                float correctAnsCount = 0;
                                                if (quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                                {
                                                    scoreValueTxt = resultSetting.CustomTxtForScoreValueInResult ?? "You scored a%score%";
                                                    correctAnsCount = attemptedQuestions.Sum(a => a.QuizAnswerStats.Sum(x => x.AnswerOptionsInQuizQuestions.AssociatedScore)).Value;
                                                }
                                                else if (quizDetails.Quiz.QuizType == (int)QuizTypeEnum.NPS)
                                                {
                                                    scoreValueTxt = resultSetting.CustomTxtForScoreValueInResult ?? string.Empty;
                                                    var attemptedQuestionsofNPSType = attemptedQuestions.Where(a => a.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.NPS);
                                                    if (attemptedQuestionsofNPSType != null && attemptedQuestionsofNPSType.Any())
                                                    {
                                                        correctAnsCount = Convert.ToInt32(attemptedQuestionsofNPSType.Sum(a => a.QuizAnswerStats.Sum(x => Convert.ToInt32(x.AnswerText))) / attemptedQuestionsofNPSType.Count());
                                                    }
                                                }
                                                else
                                                {
                                                    scoreValueTxt = resultSetting.CustomTxtForScoreValueInResult ?? "YOU GOT%score%OUT OF%total%CORRECT";
                                                    correctAnsCount = attemptedQuestions.Count(a => (a.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && ((a.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Select(s => s.Id).OrderBy(s => s)).SequenceEqual(a.QuizAnswerStats.Select(r => r.AnswerId).OrderBy(s => s)))) || (a.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single && a.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer == true));
                                                }

                                                scoreValueTxt = string.IsNullOrEmpty(scoreValueTxt) ? string.Empty : scoreValueTxt.Replace("%score%", ' ' + correctAnsCount.ToString() + ' ').Replace("%total%", ' ' + attemptedQuestions.Count(t => (t.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Any(r => r.Status == (int)StatusEnum.Active && r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value)) || t.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single).ToString() + ' ');

                                                var result = quizDetails.QuizResults.Where(r => r.Status == (int)StatusEnum.Active && r.MinScore <= correctAnsCount && correctAnsCount <= r.MaxScore).FirstOrDefault();
                                                quizAnswerSubmit.ShowLeadUserForm = (Mode == "PREVIEW" || Mode == "PREVIEWTEMPLATE") ? false : (resultIdList.Any() ? (resultIdList.Any(r => r == result.Id || quizComponentLogsList.Any(q => q.DraftedObjectId == r && q.PublishedObjectId == result.Id))) : (quizAttemptsObj.LeadUserId == null && quizAttemptsObj.RecruiterUserId == null && ((quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Personality && quizDetails.PersonalityResultSetting.FirstOrDefault().Status == (int)StatusEnum.Active) ? quizDetails.PersonalityResultSetting.FirstOrDefault().ShowLeadUserForm : result.ShowLeadUserForm)));
                                                quizAnswerSubmit.ResultScore.ShowInternalTitle = result.ShowInternalTitle;

                                                if (leadFormDetailofResultsLst != null && leadFormDetailofResultsLst.Any() && quizAnswerSubmit.ShowLeadUserForm)
                                                {
                                                    var leadFormDetailofResultsObj = leadFormDetailofResultsLst.FirstOrDefault(r => r.ResultId == result.Id || quizComponentLogsList.Any(q => q.DraftedObjectId == r.ResultId && q.PublishedObjectId == result.Id));
                                                    quizAnswerSubmit.FormId = leadFormDetailofResultsObj.FormId;
                                                    quizAnswerSubmit.FlowOrder = leadFormDetailofResultsObj.FlowOrder;
                                                }

                                                quizAnswerSubmit.ResultScore.ShowExternalTitle = result.ShowExternalTitle;
                                                quizAnswerSubmit.ResultScore.ShowDescription = result.ShowDescription;
                                                quizAnswerSubmit.ResultScore.Title = VariableLinking(result.Title, quizDetails, quizAttemptsObj, false, (resultSetting.ShowScoreValue.HasValue && resultSetting.ShowScoreValue.Value), scoreValueTxt);
                                                quizAnswerSubmit.ResultScore.InternalTitle = VariableLinking(result.InternalTitle, quizDetails, quizAttemptsObj, false, (resultSetting.ShowScoreValue.HasValue && resultSetting.ShowScoreValue.Value), scoreValueTxt);
                                                if (result.EnableMediaFile && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.RESULT && r.ObjectId == result.Id))
                                                {
                                                    var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.RESULT && r.ObjectId == result.Id);
                                                    quizAnswerSubmit.ResultScore.Image = result.ShowResultImage ? mediaObj.ObjectValue : string.Empty;
                                                    quizAnswerSubmit.ResultScore.PublicIdForResult = result.ShowResultImage ? mediaObj.ObjectPublicId : string.Empty;

                                                    var newMedia = result.ShowResultImage ? ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers) : null;
                                                    if (newMedia != null)
                                                    {
                                                        quizAnswerSubmit.ResultScore.Image = newMedia.MediaUrl;
                                                        quizAnswerSubmit.ResultScore.PublicIdForResult = newMedia.MediaPublicId;
                                                    }
                                                }
                                                else
                                                {
                                                    quizAnswerSubmit.ResultScore.Image = result.ShowResultImage ? result.Image : string.Empty;
                                                    quizAnswerSubmit.ResultScore.PublicIdForResult = result.ShowResultImage ? result.PublicId : string.Empty;
                                                }
                                                quizAnswerSubmit.ResultScore.Description = VariableLinking(result.Description, quizDetails, quizAttemptsObj, false, (resultSetting.ShowScoreValue.HasValue && resultSetting.ShowScoreValue.Value), scoreValueTxt);
                                                quizAnswerSubmit.ResultScore.HideActionButton = result.HideCallToAction.HasValue ? result.HideCallToAction.Value : false;
                                                quizAnswerSubmit.ResultScore.ActionButtonURL = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonURL;
                                                quizAnswerSubmit.ResultScore.OpenLinkInNewTab = quizAnswerSubmit.ResultScore.HideActionButton ? false : result.OpenLinkInNewTab;
                                                quizAnswerSubmit.ResultScore.ActionButtonTxtSize = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonTxtSize;
                                                quizAnswerSubmit.ResultScore.ActionButtonColor = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonColor;
                                                quizAnswerSubmit.ResultScore.ActionButtonTitleColor = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonTitleColor;
                                                quizAnswerSubmit.ResultScore.ActionButtonText = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonText;
                                                quizAnswerSubmit.ResultScore.AutoPlay = result.AutoPlay;
                                                quizAnswerSubmit.ResultScore.SecondsToApply = result.SecondsToApply ?? "0";
                                                quizAnswerSubmit.ResultScore.VideoFrameEnabled = result.VideoFrameEnabled ?? false;
                                                quizAnswerSubmit.ResultScore.DisplayOrderForTitle = result.DisplayOrderForTitle;
                                                quizAnswerSubmit.ResultScore.DisplayOrderForTitleImage = result.DisplayOrderForTitleImage;
                                                quizAnswerSubmit.ResultScore.DisplayOrderForDescription = result.DisplayOrderForDescription;
                                                quizAnswerSubmit.ResultScore.DisplayOrderForNextButton = result.DisplayOrderForNextButton;

                                                if (quizStatsObj != null)
                                                {
                                                    quizStatsObj.ResultId = result.Id;
                                                    UOWObj.QuizStatsRepository.Update(quizStatsObj);
                                                    UOWObj.Save();
                                                }
                                                if (resultSetting.ShowScoreValue.HasValue && resultSetting.ShowScoreValue.Value && (((quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate) && attemptedQuestions.Any(r => r.QuestionsInQuiz.Status == (int)StatusEnum.Active && (r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple))) || attemptedQuestions.Any(r => (r.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Any(t => t.Status == (int)StatusEnum.Active && t.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && t.IsCorrectForMultipleAnswer.HasValue && t.IsCorrectForMultipleAnswer.Value)) || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single)))
                                                {
                                                    quizAnswerSubmit.ResultScore.ResultScoreValueTxt = scoreValueTxt;
                                                    quizAnswerSubmit.ResultScore.ShowScoreValue = true;
                                                }
                                                else
                                                {
                                                    quizAnswerSubmit.ResultScore.ResultScoreValueTxt = string.Empty;
                                                    quizAnswerSubmit.ResultScore.ShowScoreValue = false;
                                                }
                                                quizAnswerSubmit.ResultScore.HideSocialShareButtons = quizDetails.HideSocialShareButtons.HasValue && quizDetails.HideSocialShareButtons.Value ? quizDetails.HideSocialShareButtons.Value : false;
                                                quizAnswerSubmit.ResultScore.ShowFacebookBtn = !quizAnswerSubmit.ResultScore.HideSocialShareButtons && quizDetails.EnableFacebookShare.HasValue ? quizDetails.EnableFacebookShare.Value : false;
                                                quizAnswerSubmit.ResultScore.ShowTwitterBtn = !quizAnswerSubmit.ResultScore.HideSocialShareButtons && quizDetails.EnableTwitterShare.HasValue ? quizDetails.EnableTwitterShare.Value : false;
                                                quizAnswerSubmit.ResultScore.ShowLinkedinBtn = !quizAnswerSubmit.ResultScore.HideSocialShareButtons && quizDetails.EnableLinkedinShare.HasValue ? quizDetails.EnableLinkedinShare.Value : false;

                                                if (resultSetting.ShowCorrectAnswer.HasValue && resultSetting.ShowCorrectAnswer.Value)
                                                {
                                                    quizAnswerSubmit.ResultScore.AnswerKeyCustomTxt = resultSetting.CustomTxtForAnswerKey;
                                                    quizAnswerSubmit.ResultScore.YourAnswerCustomTxt = resultSetting.CustomTxtForYourAnswer;
                                                    quizAnswerSubmit.ResultScore.CorrectAnswerCustomTxt = resultSetting.CustomTxtForCorrectAnswer;
                                                    quizAnswerSubmit.ResultScore.ExplanationCustomTxt = resultSetting.CustomTxtForExplanation;
                                                    quizAnswerSubmit.ResultScore.ShowCorrectAnswer = true;

                                                    quizAnswerSubmit.ResultScore.AttemptedQuestionAnswerDetails = new List<QuizAnswerSubmit.AnswerResult>();

                                                    #region attempted question

                                                    foreach (var item in attemptedQuestions.Where(r => r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Short || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Long))
                                                    {
                                                        var correctAnswerTxt = string.Empty;
                                                        bool? IsCorrectValue = null;
                                                        int associatedScore = default(int);
                                                        var yourAnswer = string.Empty;

                                                        if (item.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Any())
                                                        {
                                                            if (quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                                                associatedScore = item.QuizAnswerStats.Sum(x => x.AnswerOptionsInQuizQuestions.AssociatedScore).Value;
                                                            else
                                                                IsCorrectValue = (item.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Select(s => s.Id).OrderBy(s => s)).SequenceEqual(item.QuizAnswerStats.Select(r => r.AnswerId).OrderBy(s => s));
                                                            correctAnswerTxt = VariableLinking(string.Join(",", item.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Select(t => t.Option).Select(s => "'" + s + "'")), quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt);
                                                        }
                                                        else if (item.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single)
                                                        {
                                                            correctAnswerTxt = VariableLinking(item.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(t => t.IsCorrectAnswer.HasValue ? t.IsCorrectAnswer.Value : false).Select(t => t.Option).FirstOrDefault(), quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt);
                                                            if (quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                                                associatedScore = item.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.AssociatedScore.Value;
                                                            else
                                                                IsCorrectValue = item.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer.HasValue && item.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer.Value;
                                                        }

                                                        if (item.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple || item.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single)
                                                            yourAnswer = VariableLinking(string.Join(",", item.QuizAnswerStats.Where(t => !t.AnswerOptionsInQuizQuestions.IsUnansweredType).Select(t => t.AnswerOptionsInQuizQuestions.Option).Select(s => "'" + s + "'")), quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt);
                                                        else
                                                            yourAnswer = item.QuizAnswerStats.Select(t => t.AnswerText).FirstOrDefault();

                                                        quizAnswerSubmit.ResultScore.AttemptedQuestionAnswerDetails.Add(new QuizAnswerSubmit.AnswerResult
                                                        {
                                                            Question = VariableLinking(item.QuestionsInQuiz.Question, quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt),
                                                            YourAnswer = string.IsNullOrEmpty(yourAnswer) ? notAttemptedQuesText : yourAnswer,
                                                            CorrectAnswer = (quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate) ? string.Empty : correctAnswerTxt,
                                                            IsCorrect = IsCorrectValue,
                                                            AnswerExplanation = item.QuestionsInQuiz.CorrectAnswerDescription,
                                                            AssociatedScore = (quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate) ? associatedScore : default(int),
                                                        });
                                                    }

                                                    #endregion
                                                }
                                                else
                                                {
                                                    quizAnswerSubmit.ResultScore.ShowCorrectAnswer = false;
                                                }
                                                #endregion
                                            }
                                            else
                                            {
                                                #region Personality result Setting
                                                var personalityAnswerResultList = attemptedQuestions.SelectMany(x => x.QuizAnswerStats.SelectMany(r => r.AnswerOptionsInQuizQuestions.PersonalityAnswerResultMapping));
                                                var personalitySetting = quizDetails.PersonalityResultSetting.FirstOrDefault();
                                                if (personalitySetting != null && personalitySetting.Status == (int)StatusEnum.Active)
                                                {
                                                    #region personality Setting Active
                                                    var personalityResultList = new List<QuizAnswerSubmit.PersonalityResult>();
                                                    var countResult = 0;
                                                    quizStatsObj = quizAttemptsObj.QuizStats.FirstOrDefault();

                                                    if (personalityAnswerResultList.Any())
                                                    {
                                                        #region correlation is available
                                                        var mappedResultList = personalityAnswerResultList.OrderBy(x => x.QuizResults.DisplayOrder).GroupBy(r => r.ResultId).OrderByDescending(i => i.Count()).Take(personalitySetting.MaxResult).Select(x => x.Key).ToList();
                                                        var quizresultList = quizDetails.QuizResults.Where(r => !r.IsPersonalityCorrelatedResult && mappedResultList.Contains(r.Id)).OrderBy(x => mappedResultList.IndexOf(x.Id));
                                                        foreach (var quizresult in quizresultList)
                                                        {
                                                            QuizAnswerSubmit.PersonalityResult personalityresult = new QuizAnswerSubmit.PersonalityResult();
                                                            personalityresult.Title = VariableLinking(quizresult.Title, quizDetails, quizAttemptsObj, false, false, null);
                                                            personalityresult.InternalTitle = VariableLinking(quizresult.InternalTitle, quizDetails, quizAttemptsObj, false, false, null);
                                                            personalityresult.Image = quizresult.Image;
                                                            personalityresult.ResultId = quizresult.Id;
                                                            personalityresult.GraphColor = personalitySetting.GraphColor;
                                                            personalityresult.ButtonColor = personalitySetting.ButtonColor;
                                                            personalityresult.ButtonFontColor = personalitySetting.ButtonFontColor;
                                                            personalityresult.MaxResult = personalitySetting.MaxResult;
                                                            personalityresult.SideButtonText = personalitySetting.SideButtonText;
                                                            personalityresult.IsFullWidthEnable = personalitySetting.IsFullWidthEnable;
                                                            personalityresult.Description = VariableLinking(quizresult.Description, quizDetails, quizAttemptsObj, false, false, null);
                                                            int count = personalityAnswerResultList.Where(k => k.ResultId == quizresult.Id).Count();
                                                            personalityresult.Percentage = (int)Math.Round((double)(100 * count) / personalityAnswerResultList.Count());
                                                            personalityresult.ShowInternalTitle = quizresult.ShowInternalTitle;
                                                            personalityresult.ShowExternalTitle = quizresult.ShowExternalTitle;
                                                            personalityresult.ShowDescription = quizresult.ShowDescription;
                                                            personalityResultList.Add(personalityresult);

                                                            if (quizStatsObj != null)
                                                            {
                                                                if (countResult == 0)
                                                                {
                                                                    quizStatsObj.ResultId = quizresult.Id;
                                                                    UOWObj.QuizStatsRepository.Update(quizStatsObj);
                                                                }
                                                                else
                                                                {
                                                                    var resultQuizStats = new Db.QuizStats();
                                                                    resultQuizStats.StartedOn = quizStatsObj.StartedOn;
                                                                    resultQuizStats.QuizAttemptId = quizStatsObj.QuizAttemptId;
                                                                    resultQuizStats.ResultId = quizresult.Id;
                                                                    UOWObj.QuizStatsRepository.Insert(resultQuizStats);
                                                                }
                                                                UOWObj.Save();
                                                                countResult++;
                                                            }
                                                        }

                                                        quizAnswerSubmit.ShowLeadUserForm = (Mode == "PREVIEW" || Mode == "PREVIEWTEMPLATE") ? false : (resultIdList.Any() ? (resultIdList.Any(r => quizresultList.Any(l => l.Id == r) || quizComponentLogsList.Any(q => q.DraftedObjectId == r && quizresultList.Any(l => l.Id == q.PublishedObjectId)))) : (quizAttemptsObj.LeadUserId == null && quizAttemptsObj.RecruiterUserId == null && ((quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Personality && quizDetails.PersonalityResultSetting.FirstOrDefault().Status == (int)StatusEnum.Active) ? quizDetails.PersonalityResultSetting.FirstOrDefault().ShowLeadUserForm : quizresultList.Any(r => r.ShowLeadUserForm))));

                                                        if (leadFormDetailofResultsLst != null && leadFormDetailofResultsLst.Any() && quizAnswerSubmit.ShowLeadUserForm)
                                                        {
                                                            var personalityResult = personalityResultList.OrderByDescending(r => r.Percentage.Value).FirstOrDefault();
                                                            if (leadFormDetailofResultsLst.Any(r => r.ResultId == personalityResult.ResultId || quizComponentLogsList.Any(q => q.DraftedObjectId == r.ResultId && q.PublishedObjectId == personalityResult.ResultId)))
                                                            {
                                                                var leadFormDetailofResultsObj = leadFormDetailofResultsLst.FirstOrDefault(r => r.ResultId == personalityResult.ResultId || quizComponentLogsList.Any(q => q.DraftedObjectId == r.ResultId && q.PublishedObjectId == personalityResult.ResultId));
                                                                quizAnswerSubmit.FormId = leadFormDetailofResultsObj.FormId;
                                                                quizAnswerSubmit.FlowOrder = leadFormDetailofResultsObj.FlowOrder;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            quizAnswerSubmit.FormId = null;
                                                            quizAnswerSubmit.FlowOrder = null;
                                                        }

                                                        #endregion
                                                    }
                                                    else
                                                    {
                                                        #region correlation is not available
                                                        var quizresultList = quizDetails.QuizResults.OrderBy(x => x.DisplayOrder).Take(personalitySetting.MaxResult);
                                                        foreach (var quizresult in quizresultList)
                                                        {
                                                            QuizAnswerSubmit.PersonalityResult personalityresult = new QuizAnswerSubmit.PersonalityResult();
                                                            personalityresult.Title = VariableLinking(quizresult.Title, quizDetails, quizAttemptsObj, false, false, null);
                                                            personalityresult.InternalTitle = VariableLinking(quizresult.InternalTitle, quizDetails, quizAttemptsObj, false, false, null);
                                                            personalityresult.Image = quizresult.Image;
                                                            personalityresult.ResultId = quizresult.Id;
                                                            personalityresult.GraphColor = personalitySetting.GraphColor;
                                                            personalityresult.ButtonColor = personalitySetting.ButtonColor;
                                                            personalityresult.ButtonFontColor = personalitySetting.ButtonFontColor;
                                                            personalityresult.MaxResult = personalitySetting.MaxResult;
                                                            personalityresult.SideButtonText = personalitySetting.SideButtonText;
                                                            personalityresult.IsFullWidthEnable = personalitySetting.IsFullWidthEnable;
                                                            personalityresult.Description = VariableLinking(quizresult.Description, quizDetails, quizAttemptsObj, false, false, null);
                                                            personalityresult.Percentage = null;
                                                            personalityresult.ShowInternalTitle = quizresult.ShowInternalTitle;
                                                            personalityresult.ShowExternalTitle = quizresult.ShowExternalTitle;
                                                            personalityresult.ShowDescription = quizresult.ShowDescription;
                                                            personalityResultList.Add(personalityresult);

                                                            if (quizStatsObj != null)
                                                            {
                                                                if (countResult == 0)
                                                                {
                                                                    quizStatsObj.ResultId = quizresult.Id;
                                                                    UOWObj.QuizStatsRepository.Update(quizStatsObj);
                                                                }
                                                                else
                                                                {
                                                                    var resultQuizStats = new Db.QuizStats();
                                                                    resultQuizStats.StartedOn = quizStatsObj.StartedOn;
                                                                    resultQuizStats.QuizAttemptId = quizStatsObj.QuizAttemptId;
                                                                    resultQuizStats.ResultId = quizresult.Id;
                                                                    UOWObj.QuizStatsRepository.Insert(resultQuizStats);
                                                                }
                                                                UOWObj.Save();
                                                                countResult++;
                                                            }
                                                        }

                                                        quizAnswerSubmit.ShowLeadUserForm = (Mode == "PREVIEW" || Mode == "PREVIEWTEMPLATE") ? false : (resultIdList.Any() ? (resultIdList.Any(r => quizresultList.Any(l => l.Id == r) || quizComponentLogsList.Any(q => q.DraftedObjectId == r && quizresultList.Any(l => l.Id == q.PublishedObjectId)))) : (quizAttemptsObj.LeadUserId == null && quizAttemptsObj.RecruiterUserId == null && ((quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Personality && quizDetails.PersonalityResultSetting.FirstOrDefault().Status == (int)StatusEnum.Active) ? quizDetails.PersonalityResultSetting.FirstOrDefault().ShowLeadUserForm : quizresultList.Any(r => r.ShowLeadUserForm))));

                                                        if (leadFormDetailofResultsLst != null && leadFormDetailofResultsLst.Any() && quizAnswerSubmit.ShowLeadUserForm)
                                                        {
                                                            var personalityResult = personalityResultList.FirstOrDefault();
                                                            if (leadFormDetailofResultsLst.Any(r => r.ResultId == personalityResult.ResultId || quizComponentLogsList.Any(q => q.DraftedObjectId == r.ResultId && q.PublishedObjectId == personalityResult.ResultId)))
                                                            {
                                                                var leadFormDetailofResultsObj = leadFormDetailofResultsLst.FirstOrDefault(r => r.ResultId == personalityResult.ResultId || quizComponentLogsList.Any(q => q.DraftedObjectId == r.ResultId && q.PublishedObjectId == personalityResult.ResultId));
                                                                quizAnswerSubmit.FormId = leadFormDetailofResultsObj.FormId;
                                                                quizAnswerSubmit.FlowOrder = leadFormDetailofResultsObj.FlowOrder;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            quizAnswerSubmit.FormId = null;
                                                            quizAnswerSubmit.FlowOrder = null;
                                                        }

                                                        #endregion
                                                    }
                                                    quizAnswerSubmit.ResultScore.ShowInternalTitle = true;
                                                    quizAnswerSubmit.ResultScore.ShowExternalTitle = true;
                                                    quizAnswerSubmit.ResultScore.ShowDescription = true;
                                                    quizAnswerSubmit.ResultScore.Title = personalitySetting.Title;
                                                    quizAnswerSubmit.ResultScore.PersonalityResultList = personalityResultList.OrderByDescending(x => x.Percentage).ToList();
                                                    #endregion
                                                }
                                                else
                                                {
                                                    #region If there is correlation available but personalitySetting is disabled

                                                    var result = new Db.QuizResults();
                                                    if (personalityAnswerResultList.Any())
                                                        result = quizDetails.QuizResults.FirstOrDefault(x => x.Id == personalityAnswerResultList.OrderBy(r => r.QuizResults.DisplayOrder).GroupBy(r => r.ResultId).OrderByDescending(i => i.Count()).First().Key);
                                                    else
                                                        result = quizDetails.QuizResults.FirstOrDefault(x => x.DisplayOrder == 1);

                                                    quizAnswerSubmit.ShowLeadUserForm = (Mode == "PREVIEW" || Mode == "PREVIEWTEMPLATE") ? false : (resultIdList.Any() ? (resultIdList.Any(r => r == result.Id || quizComponentLogsList.Any(q => q.DraftedObjectId == r && q.PublishedObjectId == result.Id))) : (quizAttemptsObj.LeadUserId == null && quizAttemptsObj.RecruiterUserId == null && ((quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Personality && quizDetails.PersonalityResultSetting.FirstOrDefault().Status == (int)StatusEnum.Active) ? quizDetails.PersonalityResultSetting.FirstOrDefault().ShowLeadUserForm : result.ShowLeadUserForm)));

                                                    if (leadFormDetailofResultsLst != null && leadFormDetailofResultsLst.Any() && quizAnswerSubmit.ShowLeadUserForm)
                                                    {
                                                        var leadFormDetailofResultsObj = leadFormDetailofResultsLst.FirstOrDefault(r => r.ResultId == result.Id || quizComponentLogsList.Any(q => q.DraftedObjectId == r.ResultId && q.PublishedObjectId == result.Id));
                                                        quizAnswerSubmit.FormId = leadFormDetailofResultsObj.FormId;
                                                        quizAnswerSubmit.FlowOrder = leadFormDetailofResultsObj.FlowOrder;
                                                    }

                                                    quizAnswerSubmit.ResultScore.ShowInternalTitle = result.ShowInternalTitle;
                                                    quizAnswerSubmit.ResultScore.ShowExternalTitle = result.ShowExternalTitle;
                                                    quizAnswerSubmit.ResultScore.ShowDescription = result.ShowDescription;
                                                    quizAnswerSubmit.ResultScore.Title = VariableLinking(result.Title, quizDetails, quizAttemptsObj, false, (resultSetting.ShowScoreValue.HasValue && resultSetting.ShowScoreValue.Value), string.Empty);
                                                    quizAnswerSubmit.ResultScore.InternalTitle = VariableLinking(result.InternalTitle, quizDetails, quizAttemptsObj, false, (resultSetting.ShowScoreValue.HasValue && resultSetting.ShowScoreValue.Value), string.Empty);
                                                    if (result.EnableMediaFile && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.RESULT && r.ObjectId == result.Id))
                                                    {
                                                        var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.RESULT && r.ObjectId == result.Id);
                                                        quizAnswerSubmit.ResultScore.Image = result.ShowResultImage ? mediaObj.ObjectValue : string.Empty;
                                                        quizAnswerSubmit.ResultScore.PublicIdForResult = result.ShowResultImage ? mediaObj.ObjectPublicId : string.Empty;

                                                        var newMedia = result.ShowResultImage ? ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers) : null;
                                                        if (newMedia != null)
                                                        {
                                                            quizAnswerSubmit.ResultScore.Image = newMedia.MediaUrl;
                                                            quizAnswerSubmit.ResultScore.PublicIdForResult = newMedia.MediaPublicId;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        quizAnswerSubmit.ResultScore.Image = result.ShowResultImage ? result.Image : string.Empty;
                                                        quizAnswerSubmit.ResultScore.PublicIdForResult = result.ShowResultImage ? result.PublicId : string.Empty;
                                                    }
                                                    quizAnswerSubmit.ResultScore.Description = VariableLinking(result.Description, quizDetails, quizAttemptsObj, false, (resultSetting.ShowScoreValue.HasValue && resultSetting.ShowScoreValue.Value), string.Empty);
                                                    quizAnswerSubmit.ResultScore.HideActionButton = result.HideCallToAction.HasValue ? result.HideCallToAction.Value : false;
                                                    quizAnswerSubmit.ResultScore.ActionButtonURL = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonURL;
                                                    quizAnswerSubmit.ResultScore.OpenLinkInNewTab = quizAnswerSubmit.ResultScore.HideActionButton ? false : result.OpenLinkInNewTab;
                                                    quizAnswerSubmit.ResultScore.ActionButtonTxtSize = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonTxtSize;
                                                    quizAnswerSubmit.ResultScore.ActionButtonColor = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonColor;
                                                    quizAnswerSubmit.ResultScore.ActionButtonTitleColor = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonTitleColor;
                                                    quizAnswerSubmit.ResultScore.ActionButtonText = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonText;
                                                    quizAnswerSubmit.ResultScore.AutoPlay = result.AutoPlay;
                                                    quizAnswerSubmit.ResultScore.SecondsToApply = result.SecondsToApply ?? "0";
                                                    quizAnswerSubmit.ResultScore.VideoFrameEnabled = result.VideoFrameEnabled ?? false;
                                                    quizAnswerSubmit.ResultScore.DisplayOrderForTitle = result.DisplayOrderForTitle;
                                                    quizAnswerSubmit.ResultScore.DisplayOrderForTitleImage = result.DisplayOrderForTitleImage;
                                                    quizAnswerSubmit.ResultScore.DisplayOrderForDescription = result.DisplayOrderForDescription;
                                                    quizAnswerSubmit.ResultScore.DisplayOrderForNextButton = result.DisplayOrderForNextButton;

                                                    if (quizStatsObj != null)
                                                    {
                                                        quizStatsObj.ResultId = result.Id;
                                                        UOWObj.QuizStatsRepository.Update(quizStatsObj);
                                                        UOWObj.Save();
                                                    }

                                                    quizAnswerSubmit.ResultScore.ResultScoreValueTxt = string.Empty;
                                                    quizAnswerSubmit.ResultScore.ShowScoreValue = false;
                                                    quizAnswerSubmit.ResultScore.HideSocialShareButtons = quizDetails.HideSocialShareButtons.HasValue && quizDetails.HideSocialShareButtons.Value ? quizDetails.HideSocialShareButtons.Value : false;
                                                    quizAnswerSubmit.ResultScore.ShowFacebookBtn = !quizAnswerSubmit.ResultScore.HideSocialShareButtons && quizDetails.EnableFacebookShare.HasValue ? quizDetails.EnableFacebookShare.Value : false;
                                                    quizAnswerSubmit.ResultScore.ShowTwitterBtn = !quizAnswerSubmit.ResultScore.HideSocialShareButtons && quizDetails.EnableTwitterShare.HasValue ? quizDetails.EnableTwitterShare.Value : false;
                                                    quizAnswerSubmit.ResultScore.ShowLinkedinBtn = !quizAnswerSubmit.ResultScore.HideSocialShareButtons && quizDetails.EnableLinkedinShare.HasValue ? quizDetails.EnableLinkedinShare.Value : false;

                                                    if (resultSetting.ShowCorrectAnswer.HasValue && resultSetting.ShowCorrectAnswer.Value)
                                                    {
                                                        quizAnswerSubmit.ResultScore.AnswerKeyCustomTxt = resultSetting.CustomTxtForAnswerKey;
                                                        quizAnswerSubmit.ResultScore.YourAnswerCustomTxt = resultSetting.CustomTxtForYourAnswer;
                                                        quizAnswerSubmit.ResultScore.CorrectAnswerCustomTxt = resultSetting.CustomTxtForCorrectAnswer;
                                                        quizAnswerSubmit.ResultScore.ExplanationCustomTxt = resultSetting.CustomTxtForExplanation;
                                                        quizAnswerSubmit.ResultScore.ShowCorrectAnswer = true;
                                                        quizAnswerSubmit.ResultScore.AttemptedQuestionAnswerDetails = new List<QuizAnswerSubmit.AnswerResult>();

                                                        #region attempted question

                                                        foreach (var item in attemptedQuestions.Where(r => r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single))
                                                        {
                                                            quizAnswerSubmit.ResultScore.AttemptedQuestionAnswerDetails.Add(new QuizAnswerSubmit.AnswerResult
                                                            {
                                                                Question = VariableLinking(item.QuestionsInQuiz.Question, quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, string.Empty),
                                                                YourAnswer = (item.QuizAnswerStats != null && item.QuizAnswerStats.Any(r => !r.AnswerOptionsInQuizQuestions.IsUnansweredType)) ? VariableLinking(string.Join(",", item.QuizAnswerStats.Where(t => !t.AnswerOptionsInQuizQuestions.IsUnansweredType).Select(t => t.AnswerOptionsInQuizQuestions.Option).Select(s => "'" + s + "'")), quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, string.Empty) : notAttemptedQuesText,
                                                                CorrectAnswer = string.Empty,
                                                                IsCorrect = null,
                                                                AnswerExplanation = item.QuestionsInQuiz.CorrectAnswerDescription,
                                                                AssociatedScore = default(int)
                                                            });
                                                        }

                                                        #endregion
                                                    }
                                                    else
                                                    {

                                                        quizAnswerSubmit.ResultScore.ShowCorrectAnswer = false;
                                                    }
                                                    #endregion

                                                }
                                                #endregion
                                            }
                                            #endregion
                                        }
                                        else
                                        {
                                            #region resultSetting is null

                                            var attemptedQuestions = quizAttemptsObj.QuizQuestionStats.Where(t => t.Status == (int)StatusEnum.Active && t.CompletedOn != null && (t.QuestionsInQuiz.TimerRequired ? true : t.QuizAnswerStats.Any(r => !r.AnswerOptionsInQuizQuestions.IsUnansweredType)));

                                            string scoreValueTxt = string.Empty;
                                            var correctAnsCount = 0;
                                            var showScoreValue = false;

                                            if ((quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate) && attemptedQuestions.Any(r => r.QuestionsInQuiz.Status == (int)StatusEnum.Active && (r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple)))
                                            {
                                                scoreValueTxt = resultSetting.CustomTxtForScoreValueInResult ?? "You scored a%score%";
                                                correctAnsCount = attemptedQuestions.Sum(a => a.QuizAnswerStats.Sum(x => x.AnswerOptionsInQuizQuestions.AssociatedScore)).Value;
                                                showScoreValue = true;
                                            }
                                            else if ((quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Assessment || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.AssessmentTemplate) && attemptedQuestions.Any(r => (r.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Any(t => t.Status == (int)StatusEnum.Active && t.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && t.IsCorrectForMultipleAnswer.HasValue && t.IsCorrectForMultipleAnswer.Value)) || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single))
                                            {
                                                scoreValueTxt = resultSetting.CustomTxtForScoreValueInResult ?? "YOU GOT%score%OUT OF%total%CORRECT";
                                                correctAnsCount = attemptedQuestions.Count(a => (a.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && ((a.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Select(s => s.Id).OrderBy(s => s)).SequenceEqual(a.QuizAnswerStats.Select(r => r.AnswerId).OrderBy(s => s)))) || (a.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single && a.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer == true));
                                                showScoreValue = true;
                                            }

                                            scoreValueTxt = scoreValueTxt == null ? string.Empty : scoreValueTxt.Replace("%score%", ' ' + correctAnsCount.ToString() + ' ').Replace("%total%", ' ' + attemptedQuestions.Count(t => (t.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Any(r => r.Status == (int)StatusEnum.Active && r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value)) || t.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single).ToString() + ' ');

                                            quizAnswerSubmit.ShowLeadUserForm = false;
                                            quizAnswerSubmit.ResultScore.ResultScoreValueTxt = scoreValueTxt;
                                            quizAnswerSubmit.ResultScore.ShowScoreValue = showScoreValue;

                                            #endregion
                                        }
                                        #endregion
                                    }
                                }
                                #endregion
                                break;

                            case "complete_badge":

                                #region fetch next badge details

                                nextQuestionObj = FetchNextQuestion(quizDetails, QuestionId, (int)BranchingLogicEnum.BADGENEXT, UOWObj, isQuesAndContentInSameTable);

                                if (nextQuestionObj != null)
                                {
                                    #region if next is question type

                                    if (nextQuestionObj.GetType().BaseType.Name == "QuestionsInQuiz")
                                    {
                                        if (((Db.QuestionsInQuiz)nextQuestionObj).Type == (int)BranchingLogicEnum.QUESTION)
                                        {
                                            quizAnswerSubmit.QuestionType = (int)BranchingLogicEnum.QUESTION;

                                            quizAnswerSubmit.QuestionDetails = new QuizQuestion();

                                            quizAnswerSubmit.QuestionDetails.QuestionId = ((Db.QuestionsInQuiz)nextQuestionObj).Id;
                                            quizAnswerSubmit.QuestionDetails.QuestionTitle = VariableLinking(((Db.QuestionsInQuiz)nextQuestionObj).Question, quizDetails, quizAttemptsObj, false, false, null);
                                            quizAnswerSubmit.QuestionDetails.ShowTitle = ((Db.QuestionsInQuiz)nextQuestionObj).ShowTitle;

                                            if (((Db.QuestionsInQuiz)nextQuestionObj).ShowQuestionImage.HasValue && ((Db.QuestionsInQuiz)nextQuestionObj).ShowQuestionImage.Value && ((Db.QuestionsInQuiz)nextQuestionObj).EnableMediaFile && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION && r.ObjectId == ((Db.QuestionsInQuiz)nextQuestionObj).Id))
                                            {
                                                var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION && r.ObjectId == ((Db.QuestionsInQuiz)nextQuestionObj).Id);
                                                quizAnswerSubmit.QuestionDetails.QuestionImage = mediaObj.ObjectValue;
                                                quizAnswerSubmit.QuestionDetails.PublicIdForQuestion = mediaObj.ObjectPublicId;

                                                var newMedia = ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers);
                                                if (newMedia != null)
                                                {
                                                    quizAnswerSubmit.QuestionDetails.QuestionImage = newMedia.MediaUrl;
                                                    quizAnswerSubmit.QuestionDetails.PublicIdForQuestion = newMedia.MediaPublicId;
                                                }
                                            }
                                            else
                                            {
                                                quizAnswerSubmit.QuestionDetails.QuestionImage = ((Db.QuestionsInQuiz)nextQuestionObj).ShowQuestionImage.HasValue && ((Db.QuestionsInQuiz)nextQuestionObj).ShowQuestionImage.Value ? ((Db.QuestionsInQuiz)nextQuestionObj).QuestionImage : string.Empty;
                                                quizAnswerSubmit.QuestionDetails.PublicIdForQuestion = ((Db.QuestionsInQuiz)nextQuestionObj).ShowQuestionImage.HasValue && ((Db.QuestionsInQuiz)nextQuestionObj).ShowQuestionImage.Value ? ((Db.QuestionsInQuiz)nextQuestionObj).PublicId : string.Empty;
                                            }

                                            quizAnswerSubmit.QuestionDetails.ShowQuestionImage = ((Db.QuestionsInQuiz)nextQuestionObj).ShowQuestionImage;
                                            quizAnswerSubmit.QuestionDetails.ShowAnswerImage = ((Db.QuestionsInQuiz)nextQuestionObj).ShowAnswerImage;
                                            quizAnswerSubmit.QuestionDetails.AnswerType = ((Db.QuestionsInQuiz)nextQuestionObj).AnswerType;
                                            quizAnswerSubmit.QuestionDetails.NextButtonColor = ((Db.QuestionsInQuiz)nextQuestionObj).NextButtonColor;
                                            quizAnswerSubmit.QuestionDetails.NextButtonText = ((Db.QuestionsInQuiz)nextQuestionObj).NextButtonText;
                                            quizAnswerSubmit.QuestionDetails.NextButtonTxtColor = ((Db.QuestionsInQuiz)nextQuestionObj).NextButtonTxtColor;
                                            quizAnswerSubmit.QuestionDetails.NextButtonTxtSize = ((Db.QuestionsInQuiz)nextQuestionObj).NextButtonTxtSize;
                                            quizAnswerSubmit.QuestionDetails.EnableNextButton = ((Db.QuestionsInQuiz)nextQuestionObj).EnableNextButton;
                                            quizAnswerSubmit.QuestionDetails.MinAnswer = ((Db.QuestionsInQuiz)nextQuestionObj).MinAnswer;
                                            quizAnswerSubmit.QuestionDetails.MaxAnswer = ((Db.QuestionsInQuiz)nextQuestionObj).MaxAnswer;
                                            quizAnswerSubmit.QuestionDetails.ViewPreviousQuestion = ((Db.QuestionsInQuiz)nextQuestionObj).ViewPreviousQuestion;
                                            quizAnswerSubmit.QuestionDetails.EditAnswer = ((Db.QuestionsInQuiz)nextQuestionObj).EditAnswer;
                                            quizAnswerSubmit.QuestionDetails.StartedOn = (isLastQuestionStarted && ((Db.QuestionsInQuiz)nextQuestionObj).QuizQuestionStats != null && ((Db.QuestionsInQuiz)nextQuestionObj).QuizQuestionStats.Any(r => r.Status == (int)StatusEnum.Active)) ? ((Db.QuestionsInQuiz)nextQuestionObj).QuizQuestionStats.FirstOrDefault(r => r.Status == (int)StatusEnum.Active).StartedOn : default(DateTime?);
                                            quizAnswerSubmit.QuestionDetails.TimerRequired = ((Db.QuestionsInQuiz)nextQuestionObj).TimerRequired;
                                            quizAnswerSubmit.QuestionDetails.Time = ((Db.QuestionsInQuiz)nextQuestionObj).Time;
                                            quizAnswerSubmit.QuestionDetails.AutoPlay = ((Db.QuestionsInQuiz)nextQuestionObj).AutoPlay;
                                            quizAnswerSubmit.QuestionDetails.SecondsToApply = ((Db.QuestionsInQuiz)nextQuestionObj).SecondsToApply ?? "0";
                                            quizAnswerSubmit.QuestionDetails.VideoFrameEnabled = ((Db.QuestionsInQuiz)nextQuestionObj).VideoFrameEnabled ?? false;
                                            quizAnswerSubmit.QuestionDetails.DisplayOrderForTitle = ((Db.QuestionsInQuiz)nextQuestionObj).DisplayOrderForTitle;
                                            quizAnswerSubmit.QuestionDetails.DisplayOrderForTitleImage = ((Db.QuestionsInQuiz)nextQuestionObj).DisplayOrderForTitleImage;
                                            quizAnswerSubmit.QuestionDetails.DisplayOrderForDescription = ((Db.QuestionsInQuiz)nextQuestionObj).DisplayOrderForDescription;
                                            quizAnswerSubmit.QuestionDetails.DisplayOrderForDescriptionImage = ((Db.QuestionsInQuiz)nextQuestionObj).DisplayOrderForDescriptionImage;
                                            quizAnswerSubmit.QuestionDetails.DisplayOrderForAnswer = ((Db.QuestionsInQuiz)nextQuestionObj).DisplayOrderForAnswer;
                                            quizAnswerSubmit.QuestionDetails.DisplayOrderForNextButton = ((Db.QuestionsInQuiz)nextQuestionObj).DisplayOrderForNextButton;
                                            quizAnswerSubmit.QuestionDetails.Description = ((Db.QuestionsInQuiz)nextQuestionObj).Description;
                                            quizAnswerSubmit.QuestionDetails.ShowDescription = ((Db.QuestionsInQuiz)nextQuestionObj).ShowDescription;
                                            quizAnswerSubmit.QuestionDetails.ShowDescriptionImage = ((Db.QuestionsInQuiz)nextQuestionObj).ShowDescriptionImage ?? false;
                                            quizAnswerSubmit.QuestionDetails.AutoPlayForDescription = ((Db.QuestionsInQuiz)nextQuestionObj).AutoPlayForDescription;
                                            quizAnswerSubmit.QuestionDetails.SecondsToApplyForDescription = ((Db.QuestionsInQuiz)nextQuestionObj).SecondsToApplyForDescription ?? "0";
                                            quizAnswerSubmit.QuestionDetails.DescVideoFrameEnabled = ((Db.QuestionsInQuiz)nextQuestionObj).DescVideoFrameEnabled ?? false;
                                            quizAnswerSubmit.QuestionDetails.EnableComment = ((Db.QuestionsInQuiz)nextQuestionObj).EnableComment;
                                            quizAnswerSubmit.QuestionDetails.AnswerStructureType = ((Db.QuestionsInQuiz)nextQuestionObj).AnswerStructureType;

                                            if (((Db.QuestionsInQuiz)nextQuestionObj).EnableMediaFileForDescription && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Description && r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION && r.ObjectId == ((Db.QuestionsInQuiz)nextQuestionObj).Id))
                                            {
                                                var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Description && r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION && r.ObjectId == ((Db.QuestionsInQuiz)nextQuestionObj).Id);
                                                quizAnswerSubmit.QuestionDetails.DescriptionImage = mediaObj.ObjectValue ?? string.Empty;
                                                quizAnswerSubmit.QuestionDetails.PublicIdForDescription = mediaObj.ObjectPublicId ?? string.Empty;

                                                var newMedia = ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers);
                                                if (newMedia != null)
                                                {
                                                    quizAnswerSubmit.QuestionDetails.DescriptionImage = newMedia.MediaUrl;
                                                    quizAnswerSubmit.QuestionDetails.PublicIdForDescription = newMedia.MediaPublicId;
                                                }
                                            }
                                            else
                                            {
                                                quizAnswerSubmit.QuestionDetails.DescriptionImage = ((Db.QuestionsInQuiz)nextQuestionObj).DescriptionImage ?? string.Empty;
                                                quizAnswerSubmit.QuestionDetails.PublicIdForDescription = ((Db.QuestionsInQuiz)nextQuestionObj).PublicIdForDescription ?? string.Empty;
                                            }
                                            quizAnswerSubmit.QuestionDetails.AnswerList = new List<AnswerOptionInQuestion>();

                                            foreach (var ans in ((Db.QuestionsInQuiz)nextQuestionObj).AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && !r.IsUnansweredType).OrderBy(r => r.DisplayOrder))
                                            {
                                                var answerImage = string.Empty;
                                                var publicIdForAnswer = string.Empty;

                                                if (((Db.QuestionsInQuiz)nextQuestionObj).ShowAnswerImage.HasValue && ((Db.QuestionsInQuiz)nextQuestionObj).ShowAnswerImage.Value && ans.EnableMediaFile && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.ObjectId == ans.Id))
                                                {
                                                    var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.ObjectId == ans.Id);
                                                    answerImage = mediaObj.ObjectValue;
                                                    publicIdForAnswer = mediaObj.ObjectPublicId;

                                                    var newMedia = ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers);
                                                    if (newMedia != null)
                                                    {
                                                        answerImage = newMedia.MediaUrl;
                                                        publicIdForAnswer = newMedia.MediaPublicId;
                                                    }
                                                }
                                                else
                                                {
                                                    answerImage = ((Db.QuestionsInQuiz)nextQuestionObj).ShowAnswerImage.HasValue && ((Db.QuestionsInQuiz)nextQuestionObj).ShowAnswerImage.Value ? ans.OptionImage : string.Empty;
                                                    publicIdForAnswer = ((Db.QuestionsInQuiz)nextQuestionObj).ShowAnswerImage.HasValue && ((Db.QuestionsInQuiz)nextQuestionObj).ShowAnswerImage.Value ? ans.PublicId : string.Empty;
                                                }

                                                quizAnswerSubmit.QuestionDetails.AnswerList.Add(new AnswerOptionInQuestion
                                                {
                                                    AnswerId = ans.Id,
                                                    AssociatedScore = ans.AssociatedScore,
                                                    AnswerText = VariableLinking(ans.Option, quizDetails, quizAttemptsObj, false, false, null),
                                                    AnswerImage = answerImage,
                                                    PublicIdForAnswer = publicIdForAnswer,
                                                    IsCorrectAnswer = false,
                                                    DisplayOrder = ans.DisplayOrder,
                                                    AutoPlay = ans.AutoPlay,
                                                    SecondsToApply = ans.SecondsToApply,
                                                    VideoFrameEnabled = ans.VideoFrameEnabled,
                                                    ListValues = ans.ListValues,
                                                    //for Rating type question
                                                    OptionTextforRatingOne = ans.OptionTextforRatingOne,
                                                    OptionTextforRatingTwo = ans.OptionTextforRatingTwo,
                                                    OptionTextforRatingThree = ans.OptionTextforRatingThree,
                                                    OptionTextforRatingFour = ans.OptionTextforRatingFour,
                                                    OptionTextforRatingFive = ans.OptionTextforRatingFive
                                                });
                                            }

                                            if (!isLastQuestionStarted)
                                            {
                                                quizQuestionStatsObj = new Db.QuizQuestionStats();

                                                quizQuestionStatsObj.QuizAttemptId = quizAttemptsObj.Id;
                                                quizQuestionStatsObj.QuestionId = ((Db.QuestionsInQuiz)nextQuestionObj).Id;
                                                quizQuestionStatsObj.StartedOn = currentDate;
                                                quizQuestionStatsObj.Status = (int)StatusEnum.Active;

                                                UOWObj.QuizQuestionStatsRepository.Insert(quizQuestionStatsObj);
                                                UOWObj.Save();
                                            }
                                        }
                                        else if (((Db.QuestionsInQuiz)nextQuestionObj).Type == (int)BranchingLogicEnum.CONTENT)
                                        {
                                            quizAnswerSubmit.QuestionType = (int)BranchingLogicEnum.CONTENT;

                                            if (!isLastQuestionStarted)
                                            {
                                                quizQuestionStatsObj = new Db.QuizQuestionStats();

                                                quizQuestionStatsObj.QuizAttemptId = quizAttemptsObj.Id;
                                                quizQuestionStatsObj.QuestionId = ((Db.QuestionsInQuiz)nextQuestionObj).Id;
                                                quizQuestionStatsObj.StartedOn = currentDate;
                                                quizQuestionStatsObj.Status = (int)StatusEnum.Active;

                                                UOWObj.QuizQuestionStatsRepository.Insert(quizQuestionStatsObj);
                                                UOWObj.Save();
                                            }

                                            quizAnswerSubmit.ContentDetails = new QuizContent();

                                            quizAnswerSubmit.ContentDetails.Id = ((Db.QuestionsInQuiz)nextQuestionObj).Id;
                                            quizAnswerSubmit.ContentDetails.ContentTitle = VariableLinking(((Db.QuestionsInQuiz)nextQuestionObj).Question, quizDetails, quizAttemptsObj, false, false, null);
                                            quizAnswerSubmit.ContentDetails.ContentDescription = VariableLinking(((Db.QuestionsInQuiz)nextQuestionObj).Description, quizDetails, quizAttemptsObj, false, false, null);
                                            quizAnswerSubmit.ContentDetails.ShowDescription = ((Db.QuestionsInQuiz)nextQuestionObj).ShowDescription;

                                            if ((((Db.QuestionsInQuiz)nextQuestionObj).EnableMediaFile && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Title && r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == ((Db.QuestionsInQuiz)nextQuestionObj).Id)))
                                            {
                                                var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Title && r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == ((Db.QuestionsInQuiz)nextQuestionObj).Id);
                                                quizAnswerSubmit.ContentDetails.ContentTitleImage = mediaObj.ObjectValue;
                                                quizAnswerSubmit.ContentDetails.PublicIdForContentTitle = mediaObj.ObjectPublicId ?? string.Empty;

                                                var newMedia = ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers);
                                                if (newMedia != null)
                                                {
                                                    quizAnswerSubmit.ContentDetails.ContentTitleImage = newMedia.MediaUrl;
                                                    quizAnswerSubmit.ContentDetails.PublicIdForContentTitle = newMedia.MediaPublicId;
                                                }
                                            }
                                            else
                                            {
                                                quizAnswerSubmit.ContentDetails.ContentTitleImage = ((Db.QuestionsInQuiz)nextQuestionObj).QuestionImage ?? string.Empty;
                                                quizAnswerSubmit.ContentDetails.PublicIdForContentTitle = ((Db.QuestionsInQuiz)nextQuestionObj).PublicId ?? string.Empty;
                                            }

                                            quizAnswerSubmit.ContentDetails.ShowContentTitleImage = ((Db.QuestionsInQuiz)nextQuestionObj).ShowQuestionImage.HasValue && ((Db.QuestionsInQuiz)nextQuestionObj).ShowQuestionImage.Value ? ((Db.QuestionsInQuiz)nextQuestionObj).ShowQuestionImage.Value : false;
                                            quizAnswerSubmit.ContentDetails.AliasTextForNextButton = ((Db.QuestionsInQuiz)nextQuestionObj).NextButtonText;
                                            quizAnswerSubmit.ContentDetails.EnableNextButton = ((Db.QuestionsInQuiz)nextQuestionObj).EnableNextButton;

                                            if (((Db.QuestionsInQuiz)nextQuestionObj).EnableMediaFileForDescription && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Description && r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == ((Db.QuestionsInQuiz)nextQuestionObj).Id))
                                            {
                                                var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Description && r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == ((Db.QuestionsInQuiz)nextQuestionObj).Id);
                                                quizAnswerSubmit.ContentDetails.ContentDescriptionImage = mediaObj.ObjectValue ?? string.Empty;
                                                quizAnswerSubmit.ContentDetails.PublicIdForContentDescription = mediaObj.ObjectPublicId ?? string.Empty;

                                                var newMedia = ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers);
                                                if (newMedia != null)
                                                {
                                                    quizAnswerSubmit.ContentDetails.ContentDescriptionImage = newMedia.MediaUrl;
                                                    quizAnswerSubmit.ContentDetails.PublicIdForContentDescription = newMedia.MediaPublicId;
                                                }
                                            }
                                            else
                                            {
                                                quizAnswerSubmit.ContentDetails.ContentDescriptionImage = ((Db.QuestionsInQuiz)nextQuestionObj).DescriptionImage ?? string.Empty;
                                                quizAnswerSubmit.ContentDetails.PublicIdForContentDescription = ((Db.QuestionsInQuiz)nextQuestionObj).PublicIdForDescription ?? string.Empty;
                                            }

                                            quizAnswerSubmit.ContentDetails.ShowContentDescriptionImage = ((Db.QuestionsInQuiz)nextQuestionObj).ShowDescriptionImage.HasValue && ((Db.QuestionsInQuiz)nextQuestionObj).ShowDescriptionImage.Value ? ((Db.QuestionsInQuiz)nextQuestionObj).ShowDescriptionImage.Value : false;
                                            quizAnswerSubmit.ContentDetails.ViewPreviousQuestion = ((Db.QuestionsInQuiz)nextQuestionObj).ViewPreviousQuestion;
                                            quizAnswerSubmit.ContentDetails.AutoPlay = ((Db.QuestionsInQuiz)nextQuestionObj).AutoPlay;
                                            quizAnswerSubmit.ContentDetails.SecondsToApply = ((Db.QuestionsInQuiz)nextQuestionObj).SecondsToApply ?? "0";
                                            quizAnswerSubmit.ContentDetails.VideoFrameEnabled = ((Db.QuestionsInQuiz)nextQuestionObj).VideoFrameEnabled ?? false;
                                            quizAnswerSubmit.ContentDetails.AutoPlayForDescription = ((Db.QuestionsInQuiz)nextQuestionObj).AutoPlayForDescription;
                                            quizAnswerSubmit.ContentDetails.SecondsToApplyForDescription = ((Db.QuestionsInQuiz)nextQuestionObj).SecondsToApplyForDescription ?? "0";
                                            quizAnswerSubmit.ContentDetails.DescVideoFrameEnabled = ((Db.QuestionsInQuiz)nextQuestionObj).DescVideoFrameEnabled ?? false;
                                            quizAnswerSubmit.ContentDetails.ShowTitle = ((Db.QuestionsInQuiz)nextQuestionObj).ShowTitle;
                                            quizAnswerSubmit.ContentDetails.DisplayOrderForTitle = ((Db.QuestionsInQuiz)nextQuestionObj).DisplayOrderForTitle;
                                            quizAnswerSubmit.ContentDetails.DisplayOrderForTitleImage = ((Db.QuestionsInQuiz)nextQuestionObj).DisplayOrderForTitleImage;
                                            quizAnswerSubmit.ContentDetails.DisplayOrderForDescription = ((Db.QuestionsInQuiz)nextQuestionObj).DisplayOrderForDescription;
                                            quizAnswerSubmit.ContentDetails.DisplayOrderForDescriptionImage = ((Db.QuestionsInQuiz)nextQuestionObj).DisplayOrderForDescriptionImage;
                                            quizAnswerSubmit.ContentDetails.DisplayOrderForNextButton = ((Db.QuestionsInQuiz)nextQuestionObj).DisplayOrderForNextButton;
                                        }
                                    }

                                    #endregion

                                    #region if next is result type

                                    if (nextQuestionObj.GetType().BaseType.Name == "QuizResults")
                                    {
                                        quizAnswerSubmit.QuestionType = (int)BranchingLogicEnum.RESULT;

                                        var result = ((Db.QuizResults)nextQuestionObj);
                                        var attemptedQuestions = quizAttemptsObj.QuizQuestionStats.Where(t => t.Status == (int)StatusEnum.Active && t.CompletedOn != null && (t.QuestionsInQuiz.TimerRequired ? true : t.QuizAnswerStats.Any(r => !r.AnswerOptionsInQuizQuestions.IsUnansweredType)));

                                        quizAnswerSubmit.ShowLeadUserForm = (Mode == "PREVIEW" || Mode == "PREVIEWTEMPLATE") ? false : (resultIdList.Any() ? (resultIdList.Any(r => r == result.Id || quizComponentLogsList.Any(q => q.DraftedObjectId == r && q.PublishedObjectId == result.Id))) : (quizAttemptsObj.LeadUserId == null && quizAttemptsObj.RecruiterUserId == null && ((quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Personality && quizDetails.PersonalityResultSetting.FirstOrDefault().Status == (int)StatusEnum.Active) ? quizDetails.PersonalityResultSetting.FirstOrDefault().ShowLeadUserForm : result.ShowLeadUserForm)));
                                        quizAnswerSubmit.CompanyCode = quizDetails.Quiz.Company.ClientCode;

                                        if (leadFormDetailofResultsLst != null && leadFormDetailofResultsLst.Any() && quizAnswerSubmit.ShowLeadUserForm)
                                        {
                                            var leadFormDetailofResultsObj = leadFormDetailofResultsLst.FirstOrDefault(r => r.ResultId == result.Id || quizComponentLogsList.Any(q => q.DraftedObjectId == r.ResultId && q.PublishedObjectId == result.Id));
                                            quizAnswerSubmit.FormId = leadFormDetailofResultsObj.FormId;
                                            quizAnswerSubmit.FlowOrder = leadFormDetailofResultsObj.FlowOrder;
                                        }

                                        string scoreValueTxt = string.Empty;
                                        if (quizDetails.Quiz.QuizType != (int)QuizTypeEnum.Personality && quizDetails.Quiz.QuizType != (int)QuizTypeEnum.PersonalityTemplate)
                                        {
                                            #region non personality result Setting

                                            quizAnswerSubmit.ResultScore = new QuizAnswerSubmit.Result();

                                            if (result.EnableMediaFile && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.RESULT && r.ObjectId == result.Id))
                                            {
                                                var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.RESULT && r.ObjectId == result.Id);
                                                quizAnswerSubmit.ResultScore.Image = result.ShowResultImage ? mediaObj.ObjectValue : string.Empty;
                                                quizAnswerSubmit.ResultScore.PublicIdForResult = result.ShowResultImage ? mediaObj.ObjectPublicId : string.Empty;

                                                var newMedia = result.ShowResultImage ? ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers) : null;
                                                if (newMedia != null)
                                                {
                                                    quizAnswerSubmit.ResultScore.Image = newMedia.MediaUrl;
                                                    quizAnswerSubmit.ResultScore.PublicIdForResult = newMedia.MediaPublicId;
                                                }
                                            }
                                            else
                                            {
                                                quizAnswerSubmit.ResultScore.Image = result.ShowResultImage ? result.Image : string.Empty;
                                                quizAnswerSubmit.ResultScore.PublicIdForResult = result.ShowResultImage ? result.PublicId : string.Empty;
                                            }
                                            quizAnswerSubmit.ResultScore.HideActionButton = result.HideCallToAction.HasValue ? result.HideCallToAction.Value : false;
                                            quizAnswerSubmit.ResultScore.ActionButtonURL = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonURL;
                                            quizAnswerSubmit.ResultScore.OpenLinkInNewTab = quizAnswerSubmit.ResultScore.HideActionButton ? false : result.OpenLinkInNewTab;
                                            quizAnswerSubmit.ResultScore.ActionButtonTxtSize = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonTxtSize;
                                            quizAnswerSubmit.ResultScore.ActionButtonColor = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonColor;
                                            quizAnswerSubmit.ResultScore.ActionButtonTitleColor = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonTitleColor;
                                            quizAnswerSubmit.ResultScore.ActionButtonText = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonText;

                                            var resultSetting = quizDetails.ResultSettings.FirstOrDefault();
                                            var correctAnsCount = 0;

                                            if (resultSetting != null)
                                            {
                                                if (resultSetting.ShowScoreValue.HasValue && resultSetting.ShowScoreValue.Value && (((quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate) && attemptedQuestions.Any(r => r.QuestionsInQuiz.Status == (int)StatusEnum.Active && (r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple))) || attemptedQuestions.Any(r => (r.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Any(t => t.Status == (int)StatusEnum.Active && t.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && t.IsCorrectForMultipleAnswer.HasValue && t.IsCorrectForMultipleAnswer.Value)) || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single)))
                                                {
                                                    scoreValueTxt = resultSetting.CustomTxtForScoreValueInResult;

                                                    if (quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                                        correctAnsCount = attemptedQuestions.Sum(a => a.QuizAnswerStats.Sum(x => x.AnswerOptionsInQuizQuestions.AssociatedScore)).Value;
                                                    else
                                                        correctAnsCount = attemptedQuestions.Count(a => (a.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && ((a.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Select(s => s.Id).OrderBy(s => s)).SequenceEqual(a.QuizAnswerStats.Select(r => r.AnswerId).OrderBy(s => s)))) || (a.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single && a.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer == true));

                                                    scoreValueTxt = scoreValueTxt == null ? string.Empty : scoreValueTxt.Replace("%score%", ' ' + correctAnsCount.ToString() + ' ').Replace("%total%", ' ' + attemptedQuestions.Count(t => (t.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Any(r => r.Status == (int)StatusEnum.Active && r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value)) || t.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single).ToString() + ' ');


                                                    quizAnswerSubmit.ResultScore.ResultScoreValueTxt = scoreValueTxt;
                                                    quizAnswerSubmit.ResultScore.ShowScoreValue = true;
                                                }
                                                else
                                                {
                                                    quizAnswerSubmit.ResultScore.ResultScoreValueTxt = string.Empty;
                                                    quizAnswerSubmit.ResultScore.ShowScoreValue = false;
                                                }

                                                if (resultSetting.ShowCorrectAnswer.HasValue && resultSetting.ShowCorrectAnswer.Value)
                                                {
                                                    quizAnswerSubmit.ResultScore.AnswerKeyCustomTxt = resultSetting.CustomTxtForAnswerKey;
                                                    quizAnswerSubmit.ResultScore.YourAnswerCustomTxt = resultSetting.CustomTxtForYourAnswer;
                                                    quizAnswerSubmit.ResultScore.CorrectAnswerCustomTxt = resultSetting.CustomTxtForCorrectAnswer;
                                                    quizAnswerSubmit.ResultScore.ExplanationCustomTxt = resultSetting.CustomTxtForExplanation;
                                                    quizAnswerSubmit.ResultScore.ShowCorrectAnswer = true;

                                                    quizAnswerSubmit.ResultScore.AttemptedQuestionAnswerDetails = new List<QuizAnswerSubmit.AnswerResult>();

                                                    foreach (var item in attemptedQuestions.Where(r => r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Short || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Long))
                                                    {
                                                        var correctAnswerTxt = string.Empty;
                                                        bool? IsCorrectValue = null;
                                                        int associatedScore = default(int);
                                                        var yourAnswer = string.Empty;

                                                        if (item.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Any())
                                                        {
                                                            if (quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                                                associatedScore = item.QuizAnswerStats.Sum(x => x.AnswerOptionsInQuizQuestions.AssociatedScore).Value;
                                                            else
                                                                IsCorrectValue = (item.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Select(s => s.Id).OrderBy(s => s)).SequenceEqual(item.QuizAnswerStats.Select(r => r.AnswerId).OrderBy(s => s));
                                                            correctAnswerTxt = VariableLinking(string.Join(",", item.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Select(t => t.Option).Select(s => "'" + s + "'")), quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt);
                                                        }
                                                        else if (item.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single)
                                                        {
                                                            correctAnswerTxt = VariableLinking(item.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(t => t.IsCorrectAnswer.HasValue ? t.IsCorrectAnswer.Value : false).Select(t => t.Option).FirstOrDefault(), quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt);
                                                            if (quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                                                associatedScore = item.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.AssociatedScore.Value;
                                                            else
                                                                IsCorrectValue = item.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer.HasValue && item.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer.Value;
                                                        }

                                                        if (item.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple || item.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single)
                                                            yourAnswer = VariableLinking(string.Join(",", item.QuizAnswerStats.Where(t => !t.AnswerOptionsInQuizQuestions.IsUnansweredType).Select(t => t.AnswerOptionsInQuizQuestions.Option).Select(s => "'" + s + "'")), quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt);
                                                        else
                                                            yourAnswer = item.QuizAnswerStats.Select(t => t.AnswerText).FirstOrDefault();

                                                        quizAnswerSubmit.ResultScore.AttemptedQuestionAnswerDetails.Add(new QuizAnswerSubmit.AnswerResult
                                                        {
                                                            Question = VariableLinking(item.QuestionsInQuiz.Question, quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt),
                                                            YourAnswer = string.IsNullOrEmpty(yourAnswer) ? notAttemptedQuesText : yourAnswer,
                                                            CorrectAnswer = (quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate) ? string.Empty : correctAnswerTxt,
                                                            IsCorrect = IsCorrectValue,
                                                            AnswerExplanation = item.QuestionsInQuiz.CorrectAnswerDescription,
                                                            AssociatedScore = (quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate) ? associatedScore : default(int),
                                                        });
                                                    }
                                                }
                                                else
                                                {
                                                    quizAnswerSubmit.ResultScore.ShowCorrectAnswer = false;
                                                }
                                            }

                                            quizAnswerSubmit.ResultScore.ShowInternalTitle = result.ShowInternalTitle;
                                            quizAnswerSubmit.ResultScore.ShowExternalTitle = result.ShowExternalTitle;
                                            quizAnswerSubmit.ResultScore.ShowDescription = result.ShowDescription;
                                            quizAnswerSubmit.ResultScore.Title = VariableLinking(result.Title, quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt);
                                            quizAnswerSubmit.ResultScore.InternalTitle = VariableLinking(result.InternalTitle, quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt);
                                            quizAnswerSubmit.ResultScore.Description = VariableLinking(result.Description, quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt);
                                            quizAnswerSubmit.ResultScore.AutoPlay = result.AutoPlay;
                                            quizAnswerSubmit.ResultScore.SecondsToApply = result.SecondsToApply;
                                            quizAnswerSubmit.ResultScore.VideoFrameEnabled = result.VideoFrameEnabled;
                                            quizAnswerSubmit.ResultScore.DisplayOrderForTitle = result.DisplayOrderForTitle;
                                            quizAnswerSubmit.ResultScore.DisplayOrderForTitleImage = result.DisplayOrderForTitleImage;
                                            quizAnswerSubmit.ResultScore.DisplayOrderForDescription = result.DisplayOrderForDescription;
                                            quizAnswerSubmit.ResultScore.DisplayOrderForNextButton = result.DisplayOrderForNextButton;

                                            quizStatsObj = quizAttemptsObj.QuizStats.FirstOrDefault();

                                            if (quizStatsObj != null)
                                            {
                                                quizStatsObj.ResultId = result.Id;

                                                UOWObj.QuizStatsRepository.Update(quizStatsObj);
                                                UOWObj.Save();
                                            }
                                            #endregion
                                        }
                                        else
                                        {
                                            #region Personality result Setting

                                            var personalitySetting = quizDetails.PersonalityResultSetting.FirstOrDefault();
                                            var personalityAnswerResultList = attemptedQuestions.SelectMany(x => x.QuizAnswerStats.SelectMany(r => r.AnswerOptionsInQuizQuestions.PersonalityAnswerResultMapping));
                                            if (personalitySetting != null && personalitySetting.Status == (int)StatusEnum.Active)
                                            {
                                                #region personality Setting Active
                                                var personalityResultList = new List<QuizAnswerSubmit.PersonalityResult>();
                                                var countResult = 0;
                                                quizStatsObj = quizAttemptsObj.QuizStats.FirstOrDefault();

                                                if (personalityAnswerResultList.Any())
                                                {
                                                    #region correlation is available
                                                    var mappedResultList = personalityAnswerResultList.OrderBy(x => x.QuizResults.DisplayOrder).GroupBy(r => r.ResultId).OrderByDescending(i => i.Count()).Take(personalitySetting.MaxResult).Select(x => x.Key).ToList();

                                                    foreach (var quizresult in quizDetails.QuizResults.Where(r => !r.IsPersonalityCorrelatedResult && mappedResultList.Contains(r.Id)).OrderBy(x => mappedResultList.IndexOf(x.Id)))
                                                    {
                                                        QuizAnswerSubmit.PersonalityResult personalityresult = new QuizAnswerSubmit.PersonalityResult();
                                                        personalityresult.Title = VariableLinking(quizresult.Title, quizDetails, quizAttemptsObj, false, false, null);
                                                        personalityresult.InternalTitle = VariableLinking(quizresult.InternalTitle, quizDetails, quizAttemptsObj, false, false, null);
                                                        personalityresult.Image = quizresult.Image;
                                                        personalityresult.ResultId = quizresult.Id;
                                                        personalityresult.GraphColor = personalitySetting.GraphColor;
                                                        personalityresult.ButtonColor = personalitySetting.ButtonColor;
                                                        personalityresult.ButtonFontColor = personalitySetting.ButtonFontColor;
                                                        personalityresult.MaxResult = personalitySetting.MaxResult;
                                                        personalityresult.SideButtonText = personalitySetting.SideButtonText;
                                                        personalityresult.IsFullWidthEnable = personalitySetting.IsFullWidthEnable;
                                                        personalityresult.Description = VariableLinking(quizresult.Description, quizDetails, quizAttemptsObj, false, false, null);
                                                        int count = personalityAnswerResultList.Where(k => k.ResultId == quizresult.Id).Count();
                                                        personalityresult.Percentage = (int)Math.Round((double)(100 * count) / personalityAnswerResultList.Count());
                                                        personalityresult.ShowInternalTitle = quizresult.ShowInternalTitle;
                                                        personalityresult.ShowExternalTitle = quizresult.ShowExternalTitle;
                                                        personalityresult.ShowDescription = quizresult.ShowDescription;
                                                        personalityResultList.Add(personalityresult);

                                                        if (quizStatsObj != null)
                                                        {
                                                            if (countResult == 0)
                                                            {
                                                                quizStatsObj.ResultId = quizresult.Id;
                                                                UOWObj.QuizStatsRepository.Update(quizStatsObj);
                                                            }
                                                            else
                                                            {
                                                                var resultQuizStats = new Db.QuizStats();
                                                                resultQuizStats.StartedOn = quizStatsObj.StartedOn;
                                                                resultQuizStats.QuizAttemptId = quizStatsObj.QuizAttemptId;
                                                                resultQuizStats.ResultId = quizresult.Id;
                                                                UOWObj.QuizStatsRepository.Insert(resultQuizStats);
                                                            }
                                                            UOWObj.Save();
                                                            countResult++;
                                                        }
                                                    }
                                                    #endregion
                                                }
                                                else
                                                {
                                                    #region correlation is not available
                                                    foreach (var quizresult in quizDetails.QuizResults.OrderBy(x => x.DisplayOrder).Take(personalitySetting.MaxResult))
                                                    {
                                                        QuizAnswerSubmit.PersonalityResult personalityresult = new QuizAnswerSubmit.PersonalityResult();
                                                        personalityresult.Title = VariableLinking(quizresult.Title, quizDetails, quizAttemptsObj, false, false, null);
                                                        personalityresult.InternalTitle = VariableLinking(quizresult.InternalTitle, quizDetails, quizAttemptsObj, false, false, null);
                                                        personalityresult.Image = quizresult.Image;
                                                        personalityresult.ResultId = quizresult.Id;
                                                        personalityresult.GraphColor = personalitySetting.GraphColor;
                                                        personalityresult.ButtonColor = personalitySetting.ButtonColor;
                                                        personalityresult.ButtonFontColor = personalitySetting.ButtonFontColor;
                                                        personalityresult.MaxResult = personalitySetting.MaxResult;
                                                        personalityresult.SideButtonText = personalitySetting.SideButtonText;
                                                        personalityresult.IsFullWidthEnable = personalitySetting.IsFullWidthEnable;
                                                        personalityresult.Description = VariableLinking(quizresult.Description, quizDetails, quizAttemptsObj, false, false, null);
                                                        personalityresult.Percentage = null;
                                                        personalityresult.ShowInternalTitle = quizresult.ShowInternalTitle;
                                                        personalityresult.ShowExternalTitle = quizresult.ShowExternalTitle;
                                                        personalityresult.ShowDescription = quizresult.ShowDescription;
                                                        personalityResultList.Add(personalityresult);

                                                        if (quizStatsObj != null)
                                                        {
                                                            if (countResult == 0)
                                                            {
                                                                quizStatsObj.ResultId = quizresult.Id;
                                                                UOWObj.QuizStatsRepository.Update(quizStatsObj);
                                                            }
                                                            else
                                                            {
                                                                var resultQuizStats = new Db.QuizStats();
                                                                resultQuizStats.StartedOn = quizStatsObj.StartedOn;
                                                                resultQuizStats.QuizAttemptId = quizStatsObj.QuizAttemptId;
                                                                resultQuizStats.ResultId = quizresult.Id;
                                                                UOWObj.QuizStatsRepository.Insert(resultQuizStats);
                                                            }
                                                            UOWObj.Save();
                                                            countResult++;
                                                        }
                                                    }
                                                    #endregion
                                                }

                                                quizAnswerSubmit.ResultScore = quizAnswerSubmit.ResultScore ?? new QuizAnswerSubmit.Result();

                                                quizAnswerSubmit.ResultScore.ShowInternalTitle = true;
                                                quizAnswerSubmit.ResultScore.ShowExternalTitle = true;
                                                quizAnswerSubmit.ResultScore.ShowDescription = true;
                                                quizAnswerSubmit.ResultScore.Title = personalitySetting.Title;
                                                quizAnswerSubmit.ResultScore.PersonalityResultList = personalityResultList.OrderByDescending(x => x.Percentage).ToList();
                                                #endregion
                                            }
                                            else
                                            {
                                                #region if there is correlation available but personalitySetting is disabled

                                                if (result.IsPersonalityCorrelatedResult && personalityAnswerResultList.Any())
                                                    result = quizDetails.QuizResults.FirstOrDefault(x => x.Id == personalityAnswerResultList.OrderBy(r => r.QuizResults.DisplayOrder).GroupBy(r => r.ResultId).OrderByDescending(i => i.Count()).First().Key);

                                                quizAnswerSubmit.ResultScore = new QuizAnswerSubmit.Result();

                                                if (result.EnableMediaFile && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.RESULT && r.ObjectId == result.Id))
                                                {
                                                    var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.RESULT && r.ObjectId == result.Id);
                                                    quizAnswerSubmit.ResultScore.Image = result.ShowResultImage ? mediaObj.ObjectValue : string.Empty;
                                                    quizAnswerSubmit.ResultScore.PublicIdForResult = result.ShowResultImage ? mediaObj.ObjectPublicId : string.Empty;

                                                    var newMedia = result.ShowResultImage ? ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers) : null;
                                                    if (newMedia != null)
                                                    {
                                                        quizAnswerSubmit.ResultScore.Image = newMedia.MediaUrl;
                                                        quizAnswerSubmit.ResultScore.PublicIdForResult = newMedia.MediaPublicId;
                                                    }
                                                }
                                                else
                                                {
                                                    quizAnswerSubmit.ResultScore.Image = result.ShowResultImage ? result.Image : string.Empty;
                                                    quizAnswerSubmit.ResultScore.PublicIdForResult = result.ShowResultImage ? result.PublicId : string.Empty;
                                                }
                                                quizAnswerSubmit.ResultScore.HideActionButton = result.HideCallToAction.HasValue ? result.HideCallToAction.Value : false;
                                                quizAnswerSubmit.ResultScore.ActionButtonURL = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonURL;
                                                quizAnswerSubmit.ResultScore.OpenLinkInNewTab = quizAnswerSubmit.ResultScore.HideActionButton ? false : result.OpenLinkInNewTab;
                                                quizAnswerSubmit.ResultScore.ActionButtonTxtSize = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonTxtSize;
                                                quizAnswerSubmit.ResultScore.ActionButtonColor = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonColor;
                                                quizAnswerSubmit.ResultScore.ActionButtonTitleColor = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonTitleColor;
                                                quizAnswerSubmit.ResultScore.ActionButtonText = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonText;

                                                var resultSetting = quizDetails.ResultSettings.FirstOrDefault();

                                                if (resultSetting != null)
                                                {
                                                    quizAnswerSubmit.ResultScore.ResultScoreValueTxt = string.Empty;
                                                    quizAnswerSubmit.ResultScore.ShowScoreValue = false;
                                                    if (resultSetting.ShowCorrectAnswer.HasValue && resultSetting.ShowCorrectAnswer.Value)
                                                    {
                                                        quizAnswerSubmit.ResultScore.AnswerKeyCustomTxt = resultSetting.CustomTxtForAnswerKey;
                                                        quizAnswerSubmit.ResultScore.YourAnswerCustomTxt = resultSetting.CustomTxtForYourAnswer;
                                                        quizAnswerSubmit.ResultScore.CorrectAnswerCustomTxt = resultSetting.CustomTxtForCorrectAnswer;
                                                        quizAnswerSubmit.ResultScore.ExplanationCustomTxt = resultSetting.CustomTxtForExplanation;
                                                        quizAnswerSubmit.ResultScore.ShowCorrectAnswer = true;
                                                        quizAnswerSubmit.ResultScore.AttemptedQuestionAnswerDetails = new List<QuizAnswerSubmit.AnswerResult>();

                                                        foreach (var item in attemptedQuestions.Where(r => r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single))
                                                        {
                                                            quizAnswerSubmit.ResultScore.AttemptedQuestionAnswerDetails.Add(new QuizAnswerSubmit.AnswerResult
                                                            {
                                                                Question = VariableLinking(item.QuestionsInQuiz.Question, quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt),
                                                                YourAnswer = VariableLinking(string.Join(",", item.QuizAnswerStats.Where(t => !t.AnswerOptionsInQuizQuestions.IsUnansweredType).Select(t => t.AnswerOptionsInQuizQuestions.Option).Select(s => "'" + s + "'")), quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt),
                                                                CorrectAnswer = string.Empty,
                                                                IsCorrect = null,
                                                                AnswerExplanation = item.QuestionsInQuiz.CorrectAnswerDescription,
                                                                AssociatedScore = default(int)
                                                            });
                                                        }
                                                    }
                                                    else
                                                    {
                                                        quizAnswerSubmit.ResultScore.ShowCorrectAnswer = false;
                                                    }
                                                }

                                                quizAnswerSubmit.ResultScore.ShowInternalTitle = result.ShowInternalTitle;
                                                quizAnswerSubmit.ResultScore.ShowExternalTitle = result.ShowExternalTitle;
                                                quizAnswerSubmit.ResultScore.ShowDescription = result.ShowDescription;
                                                quizAnswerSubmit.ResultScore.Title = VariableLinking(result.Title, quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt);
                                                quizAnswerSubmit.ResultScore.InternalTitle = VariableLinking(result.InternalTitle, quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt);
                                                quizAnswerSubmit.ResultScore.Description = VariableLinking(result.Description, quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt);
                                                quizAnswerSubmit.ResultScore.AutoPlay = result.AutoPlay;
                                                quizAnswerSubmit.ResultScore.SecondsToApply = result.SecondsToApply;
                                                quizAnswerSubmit.ResultScore.VideoFrameEnabled = result.VideoFrameEnabled;
                                                quizAnswerSubmit.ResultScore.DisplayOrderForTitle = result.DisplayOrderForTitle;
                                                quizAnswerSubmit.ResultScore.DisplayOrderForTitleImage = result.DisplayOrderForTitleImage;
                                                quizAnswerSubmit.ResultScore.DisplayOrderForDescription = result.DisplayOrderForDescription;
                                                quizAnswerSubmit.ResultScore.DisplayOrderForNextButton = result.DisplayOrderForNextButton;

                                                quizStatsObj = quizAttemptsObj.QuizStats.FirstOrDefault();

                                                if (quizStatsObj != null)
                                                {
                                                    quizStatsObj.ResultId = result.Id;
                                                    UOWObj.QuizStatsRepository.Update(quizStatsObj);
                                                    UOWObj.Save();
                                                }
                                                #endregion
                                            }

                                            #endregion
                                        }
                                        var nextObj = FetchNextQuestion(quizDetails, result.Id, (int)BranchingLogicEnum.RESULTNEXT, UOWObj, isQuesAndContentInSameTable);

                                        #region if next is Badge type

                                        if (nextObj != null && nextObj.GetType().BaseType.Name == "BadgesInQuiz")
                                        {
                                            quizAnswerSubmit.BadgeDetails = new QuizBadge();

                                            quizAnswerSubmit.BadgeDetails.Id = ((Db.BadgesInQuiz)nextObj).Id;
                                            quizAnswerSubmit.BadgeDetails.Title = VariableLinking(((Db.BadgesInQuiz)nextObj).Title, quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt);
                                            quizAnswerSubmit.BadgeDetails.ShowTitle = ((Db.BadgesInQuiz)nextObj).ShowTitle;
                                            quizAnswerSubmit.BadgeDetails.AutoPlay = ((Db.BadgesInQuiz)nextObj).AutoPlay;
                                            quizAnswerSubmit.BadgeDetails.SecondsToApply = ((Db.BadgesInQuiz)nextObj).SecondsToApply;
                                            quizAnswerSubmit.BadgeDetails.VideoFrameEnabled = ((Db.BadgesInQuiz)nextObj).VideoFrameEnabled;
                                            quizAnswerSubmit.BadgeDetails.DisplayOrderForTitleImage = ((Db.BadgesInQuiz)nextObj).DisplayOrderForTitleImage;
                                            quizAnswerSubmit.BadgeDetails.DisplayOrderForTitle = ((Db.BadgesInQuiz)nextObj).DisplayOrderForTitle;
                                            quizAnswerSubmit.BadgeDetails.ShowImage = ((Db.BadgesInQuiz)nextObj).ShowImage;

                                            if (((Db.BadgesInQuiz)nextObj).EnableMediaFile && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.BADGE && r.ObjectId == ((Db.BadgesInQuiz)nextObj).Id))
                                            {
                                                var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.BADGE && r.ObjectId == ((Db.BadgesInQuiz)nextObj).Id);
                                                quizAnswerSubmit.BadgeDetails.Image = mediaObj.ObjectValue;
                                                quizAnswerSubmit.BadgeDetails.PublicIdForBadge = mediaObj.ObjectPublicId ?? string.Empty;

                                                var newMedia = ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers);
                                                if (newMedia != null)
                                                {
                                                    quizAnswerSubmit.BadgeDetails.Image = newMedia.MediaUrl;
                                                    quizAnswerSubmit.BadgeDetails.PublicIdForBadge = newMedia.MediaPublicId;
                                                }
                                            }
                                            else
                                            {
                                                quizAnswerSubmit.BadgeDetails.Image = ((Db.BadgesInQuiz)nextObj).Image ?? string.Empty;
                                                quizAnswerSubmit.BadgeDetails.PublicIdForBadge = ((Db.BadgesInQuiz)nextObj).PublicId ?? string.Empty;
                                            }

                                            var BadgeStatsObj = new Db.QuizObjectStats();

                                            BadgeStatsObj.QuizAttemptId = quizAttemptsObj.Id;
                                            BadgeStatsObj.ObjectId = ((Db.BadgesInQuiz)nextObj).Id;
                                            BadgeStatsObj.ViewedOn = currentDate;
                                            BadgeStatsObj.TypeId = (int)BranchingLogicEnum.BADGE;
                                            BadgeStatsObj.Status = (int)StatusEnum.Active;

                                            UOWObj.QuizObjectStatsRepository.Insert(BadgeStatsObj);
                                            UOWObj.Save();

                                        }
                                        #endregion
                                    }

                                    #endregion

                                    #region if next is content type

                                    if (nextQuestionObj.GetType().BaseType.Name == "ContentsInQuiz")
                                    {
                                        quizAnswerSubmit.QuestionType = (int)BranchingLogicEnum.CONTENT;

                                        if (quizAttemptsObj.QuizObjectStats.Any(a => a.Status == (int)StatusEnum.Active && a.TypeId == (int)BranchingLogicEnum.CONTENT && a.ObjectId == ((Db.ContentsInQuiz)nextQuestionObj).Id))
                                        {
                                            QuestionId = ((Db.ContentsInQuiz)nextQuestionObj).Id;
                                            Type = "complete_content";
                                            goto switchcase;
                                        };

                                        quizAnswerSubmit.ContentDetails = new QuizContent();

                                        quizAnswerSubmit.ContentDetails.Id = ((Db.ContentsInQuiz)nextQuestionObj).Id;
                                        quizAnswerSubmit.ContentDetails.ContentTitle = VariableLinking(((Db.ContentsInQuiz)nextQuestionObj).ContentTitle, quizDetails, quizAttemptsObj, false, false, null);
                                        quizAnswerSubmit.ContentDetails.ContentDescription = VariableLinking(((Db.ContentsInQuiz)nextQuestionObj).ContentDescription, quizDetails, quizAttemptsObj, false, false, null);
                                        quizAnswerSubmit.ContentDetails.ShowDescription = ((Db.ContentsInQuiz)nextQuestionObj).ShowDescription;

                                        if ((((Db.ContentsInQuiz)nextQuestionObj).EnableMediaFileForTitle && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Title && r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == ((Db.ContentsInQuiz)nextQuestionObj).Id)))
                                        {
                                            var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Title && r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == ((Db.ContentsInQuiz)nextQuestionObj).Id);
                                            quizAnswerSubmit.ContentDetails.ContentTitleImage = mediaObj.ObjectValue;
                                            quizAnswerSubmit.ContentDetails.PublicIdForContentTitle = mediaObj.ObjectPublicId ?? string.Empty;

                                            var newMedia = ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers);
                                            if (newMedia != null)
                                            {
                                                quizAnswerSubmit.ContentDetails.ContentTitleImage = newMedia.MediaUrl;
                                                quizAnswerSubmit.ContentDetails.PublicIdForContentTitle = newMedia.MediaPublicId;
                                            }
                                        }
                                        else
                                        {
                                            quizAnswerSubmit.ContentDetails.ContentTitleImage = ((Db.ContentsInQuiz)nextQuestionObj).ContentTitleImage ?? string.Empty;
                                            quizAnswerSubmit.ContentDetails.PublicIdForContentTitle = ((Db.ContentsInQuiz)nextQuestionObj).PublicIdForContentTitle ?? string.Empty;
                                        }

                                        quizAnswerSubmit.ContentDetails.ShowContentTitleImage = ((Db.ContentsInQuiz)nextQuestionObj).ShowContentTitleImage.HasValue && ((Db.ContentsInQuiz)nextQuestionObj).ShowContentTitleImage.Value ? ((Db.ContentsInQuiz)nextQuestionObj).ShowContentTitleImage.Value : false;
                                        quizAnswerSubmit.ContentDetails.AliasTextForNextButton = ((Db.ContentsInQuiz)nextQuestionObj).AliasTextForNextButton;
                                        quizAnswerSubmit.ContentDetails.EnableNextButton = ((Db.ContentsInQuiz)nextQuestionObj).EnableNextButton;

                                        if (((Db.ContentsInQuiz)nextQuestionObj).EnableMediaFileForDescription && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Description && r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == ((Db.ContentsInQuiz)nextQuestionObj).Id))
                                        {
                                            var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Description && r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == ((Db.ContentsInQuiz)nextQuestionObj).Id);
                                            quizAnswerSubmit.ContentDetails.ContentDescriptionImage = mediaObj.ObjectValue ?? string.Empty;
                                            quizAnswerSubmit.ContentDetails.PublicIdForContentDescription = mediaObj.ObjectPublicId ?? string.Empty;

                                            var newMedia = ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers);
                                            if (newMedia != null)
                                            {
                                                quizAnswerSubmit.ContentDetails.ContentDescriptionImage = newMedia.MediaUrl;
                                                quizAnswerSubmit.ContentDetails.PublicIdForContentDescription = newMedia.MediaPublicId;
                                            }
                                        }
                                        else
                                        {
                                            quizAnswerSubmit.ContentDetails.ContentDescriptionImage = ((Db.ContentsInQuiz)nextQuestionObj).ContentDescriptionImage ?? string.Empty;
                                            quizAnswerSubmit.ContentDetails.PublicIdForContentDescription = ((Db.ContentsInQuiz)nextQuestionObj).PublicIdForContentDescription ?? string.Empty;
                                        }

                                        //quizAnswerSubmit.ContentDetails.PublicIdForContentDescription = ((Db.ContentsInQuiz)nextQuestionObj).PublicIdForContentDescription ?? string.Empty;
                                        quizAnswerSubmit.ContentDetails.ShowContentDescriptionImage = ((Db.ContentsInQuiz)nextQuestionObj).ShowContentDescriptionImage.HasValue && ((Db.ContentsInQuiz)nextQuestionObj).ShowContentDescriptionImage.Value ? ((Db.ContentsInQuiz)nextQuestionObj).ShowContentDescriptionImage.Value : false;
                                        quizAnswerSubmit.ContentDetails.ViewPreviousQuestion = ((Db.ContentsInQuiz)nextQuestionObj).ViewPreviousQuestion;
                                        quizAnswerSubmit.ContentDetails.AutoPlay = ((Db.ContentsInQuiz)nextQuestionObj).AutoPlay;
                                        quizAnswerSubmit.ContentDetails.SecondsToApply = ((Db.ContentsInQuiz)nextQuestionObj).SecondsToApply ?? "0";
                                        quizAnswerSubmit.ContentDetails.VideoFrameEnabled = ((Db.ContentsInQuiz)nextQuestionObj).VideoFrameEnabled ?? false;
                                        quizAnswerSubmit.ContentDetails.AutoPlayForDescription = ((Db.ContentsInQuiz)nextQuestionObj).AutoPlayForDescription;
                                        quizAnswerSubmit.ContentDetails.SecondsToApplyForDescription = ((Db.ContentsInQuiz)nextQuestionObj).SecondsToApplyForDescription ?? "0";
                                        quizAnswerSubmit.ContentDetails.DescVideoFrameEnabled = ((Db.ContentsInQuiz)nextQuestionObj).DescVideoFrameEnabled ?? false;
                                        quizAnswerSubmit.ContentDetails.ShowTitle = ((Db.ContentsInQuiz)nextQuestionObj).ShowTitle;
                                        quizAnswerSubmit.ContentDetails.DisplayOrderForTitle = ((Db.ContentsInQuiz)nextQuestionObj).DisplayOrderForTitle;
                                        quizAnswerSubmit.ContentDetails.DisplayOrderForTitleImage = ((Db.ContentsInQuiz)nextQuestionObj).DisplayOrderForTitleImage;
                                        quizAnswerSubmit.ContentDetails.DisplayOrderForDescription = ((Db.ContentsInQuiz)nextQuestionObj).DisplayOrderForDescription;
                                        quizAnswerSubmit.ContentDetails.DisplayOrderForDescriptionImage = ((Db.ContentsInQuiz)nextQuestionObj).DisplayOrderForDescriptionImage;
                                        quizAnswerSubmit.ContentDetails.DisplayOrderForNextButton = ((Db.ContentsInQuiz)nextQuestionObj).DisplayOrderForNextButton;
                                    }
                                    #endregion

                                    #region if next is Badge type

                                    if (nextQuestionObj.GetType().BaseType.Name == "BadgesInQuiz")
                                    {
                                        quizAnswerSubmit.QuestionType = (int)BranchingLogicEnum.BADGE;
                                        quizAnswerSubmit.BadgeDetails = new QuizBadge();

                                        quizAnswerSubmit.BadgeDetails.Id = ((Db.BadgesInQuiz)nextQuestionObj).Id;
                                        quizAnswerSubmit.BadgeDetails.Title = VariableLinking(((Db.BadgesInQuiz)nextQuestionObj).Title, quizDetails, quizAttemptsObj, false, false, null);
                                        quizAnswerSubmit.BadgeDetails.ShowTitle = ((Db.BadgesInQuiz)nextQuestionObj).ShowTitle;
                                        quizAnswerSubmit.BadgeDetails.AutoPlay = ((Db.BadgesInQuiz)nextQuestionObj).AutoPlay;
                                        quizAnswerSubmit.BadgeDetails.SecondsToApply = ((Db.BadgesInQuiz)nextQuestionObj).SecondsToApply;
                                        quizAnswerSubmit.BadgeDetails.VideoFrameEnabled = ((Db.BadgesInQuiz)nextQuestionObj).VideoFrameEnabled;
                                        quizAnswerSubmit.BadgeDetails.DisplayOrderForTitleImage = ((Db.BadgesInQuiz)nextQuestionObj).DisplayOrderForTitleImage;
                                        quizAnswerSubmit.BadgeDetails.DisplayOrderForTitle = ((Db.BadgesInQuiz)nextQuestionObj).DisplayOrderForTitle;
                                        quizAnswerSubmit.BadgeDetails.ShowImage = ((Db.BadgesInQuiz)nextQuestionObj).ShowImage;

                                        if (((Db.BadgesInQuiz)nextQuestionObj).EnableMediaFile && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.BADGE && r.ObjectId == ((Db.BadgesInQuiz)nextQuestionObj).Id))
                                        {
                                            var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.BADGE && r.ObjectId == ((Db.BadgesInQuiz)nextQuestionObj).Id);
                                            quizAnswerSubmit.BadgeDetails.Image = mediaObj.ObjectValue;
                                            quizAnswerSubmit.BadgeDetails.PublicIdForBadge = mediaObj.ObjectPublicId ?? string.Empty;

                                            var newMedia = ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers);
                                            if (newMedia != null)
                                            {
                                                quizAnswerSubmit.BadgeDetails.Image = newMedia.MediaUrl;
                                                quizAnswerSubmit.BadgeDetails.PublicIdForBadge = newMedia.MediaPublicId;
                                            }
                                        }
                                        else
                                        {
                                            quizAnswerSubmit.BadgeDetails.Image = ((Db.BadgesInQuiz)nextQuestionObj).Image ?? string.Empty;
                                            quizAnswerSubmit.BadgeDetails.PublicIdForBadge = ((Db.BadgesInQuiz)nextQuestionObj).PublicId ?? string.Empty;
                                        }
                                    }
                                    #endregion
                                }
                                else
                                {
                                    #region show last result

                                    quizAnswerSubmit.ShowLeadUserForm = (Mode == "PREVIEW" || Mode == "PREVIEWTEMPLATE") ? false : (quizAttemptsObj.LeadUserId == null && quizAttemptsObj.RecruiterUserId == null);
                                    quizAnswerSubmit.CompanyCode = quizDetails.Quiz.Company.ClientCode;
                                    quizAnswerSubmit.ResultScore = new QuizAnswerSubmit.Result();

                                    var resultSetting = quizDetails.ResultSettings.FirstOrDefault();

                                    if (resultSetting != null)
                                    {
                                        var attemptedQuestions = quizAttemptsObj.QuizQuestionStats.Where(t => t.Status == (int)StatusEnum.Active && t.CompletedOn != null && (t.QuestionsInQuiz.TimerRequired ? true : t.QuizAnswerStats.Any(r => !r.AnswerOptionsInQuizQuestions.IsUnansweredType)));
                                        var correctAnsCount = 0;
                                        string scoreValueTxt = string.Empty;

                                        if (quizDetails.Quiz.QuizType != (int)QuizTypeEnum.Personality && quizDetails.Quiz.QuizType != (int)QuizTypeEnum.PersonalityTemplate)
                                        {
                                            #region non personality result Setting

                                            if (resultSetting.ShowScoreValue.HasValue && resultSetting.ShowScoreValue.Value && (((quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate) && attemptedQuestions.Any(r => r.QuestionsInQuiz.Status == (int)StatusEnum.Active && (r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple))) || attemptedQuestions.Any(r => (r.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Any(t => t.Status == (int)StatusEnum.Active && t.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && t.IsCorrectForMultipleAnswer.HasValue && t.IsCorrectForMultipleAnswer.Value)) || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single)))
                                            {
                                                if (quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                                {
                                                    scoreValueTxt = resultSetting.CustomTxtForScoreValueInResult ?? "YOU GOT%score%";
                                                    correctAnsCount = attemptedQuestions.Sum(a => a.QuizAnswerStats.Sum(x => x.AnswerOptionsInQuizQuestions.AssociatedScore)).Value;
                                                }
                                                else
                                                {
                                                    scoreValueTxt = resultSetting.CustomTxtForScoreValueInResult ?? "YOU GOT%score%OUT OF%total%CORRECT";
                                                    correctAnsCount = attemptedQuestions.Count(a => (a.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && ((a.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Select(s => s.Id).OrderBy(s => s)).SequenceEqual(a.QuizAnswerStats.Select(r => r.AnswerId).OrderBy(s => s)))) || (a.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single && a.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer == true));
                                                }

                                                scoreValueTxt = scoreValueTxt == null ? string.Empty : scoreValueTxt.Replace("%score%", ' ' + correctAnsCount.ToString() + ' ').Replace("%total%", ' ' + attemptedQuestions.Count(t => (t.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Any(r => r.Status == (int)StatusEnum.Active && r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value)) || t.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single).ToString() + ' ');

                                                quizAnswerSubmit.ResultScore.ResultScoreValueTxt = scoreValueTxt;
                                                quizAnswerSubmit.ResultScore.ShowScoreValue = true;
                                            }
                                            else
                                            {
                                                quizAnswerSubmit.ResultScore.ResultScoreValueTxt = string.Empty;
                                                quizAnswerSubmit.ResultScore.ShowScoreValue = false;
                                            }
                                            quizAnswerSubmit.ResultScore.HideSocialShareButtons = quizDetails.HideSocialShareButtons.HasValue && quizDetails.HideSocialShareButtons.Value ? quizDetails.HideSocialShareButtons.Value : false;
                                            quizAnswerSubmit.ResultScore.ShowFacebookBtn = !quizAnswerSubmit.ResultScore.HideSocialShareButtons && quizDetails.EnableFacebookShare.HasValue ? quizDetails.EnableFacebookShare.Value : false;
                                            quizAnswerSubmit.ResultScore.ShowTwitterBtn = !quizAnswerSubmit.ResultScore.HideSocialShareButtons && quizDetails.EnableTwitterShare.HasValue ? quizDetails.EnableTwitterShare.Value : false;
                                            quizAnswerSubmit.ResultScore.ShowLinkedinBtn = !quizAnswerSubmit.ResultScore.HideSocialShareButtons && quizDetails.EnableLinkedinShare.HasValue ? quizDetails.EnableLinkedinShare.Value : false;
                                            quizAnswerSubmit.ResultScore.DisplayOrderForTitle = quizAnswerSubmit.ResultScore.DisplayOrderForTitle;
                                            quizAnswerSubmit.ResultScore.DisplayOrderForTitleImage = quizAnswerSubmit.ResultScore.DisplayOrderForTitleImage;
                                            quizAnswerSubmit.ResultScore.DisplayOrderForDescription = quizAnswerSubmit.ResultScore.DisplayOrderForDescription;
                                            quizAnswerSubmit.ResultScore.DisplayOrderForNextButton = quizAnswerSubmit.ResultScore.DisplayOrderForNextButton;

                                            if (resultSetting.ShowCorrectAnswer.HasValue && resultSetting.ShowCorrectAnswer.Value)
                                            {
                                                quizAnswerSubmit.ResultScore.AnswerKeyCustomTxt = resultSetting.CustomTxtForAnswerKey;
                                                quizAnswerSubmit.ResultScore.YourAnswerCustomTxt = resultSetting.CustomTxtForYourAnswer;
                                                quizAnswerSubmit.ResultScore.CorrectAnswerCustomTxt = resultSetting.CustomTxtForCorrectAnswer;
                                                quizAnswerSubmit.ResultScore.ExplanationCustomTxt = resultSetting.CustomTxtForExplanation;
                                                quizAnswerSubmit.ResultScore.ShowCorrectAnswer = true;

                                                quizAnswerSubmit.ResultScore.AttemptedQuestionAnswerDetails = new List<QuizAnswerSubmit.AnswerResult>();

                                                foreach (var item in attemptedQuestions.Where(r => r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Short || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Long))
                                                {
                                                    var correctAnswerTxt = string.Empty;
                                                    bool? IsCorrectValue = null;
                                                    int associatedScore = default(int);
                                                    var yourAnswer = string.Empty;

                                                    if (item.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Any())
                                                    {
                                                        if (quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                                            associatedScore = item.QuizAnswerStats.Sum(x => x.AnswerOptionsInQuizQuestions.AssociatedScore).Value;
                                                        else
                                                            IsCorrectValue = (item.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Select(s => s.Id).OrderBy(s => s)).SequenceEqual(item.QuizAnswerStats.Select(r => r.AnswerId).OrderBy(s => s));
                                                        correctAnswerTxt = VariableLinking(string.Join(",", item.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Select(t => t.Option).Select(s => "'" + s + "'")), quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt);
                                                    }
                                                    else if (item.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single)
                                                    {
                                                        correctAnswerTxt = VariableLinking(item.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(t => t.IsCorrectAnswer.HasValue ? t.IsCorrectAnswer.Value : false).Select(t => t.Option).FirstOrDefault(), quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt);
                                                        if (quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                                            associatedScore = item.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.AssociatedScore.Value;
                                                        else
                                                            IsCorrectValue = item.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer.HasValue && item.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer.Value;
                                                    }


                                                    if (item.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple || item.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single)
                                                        yourAnswer = VariableLinking(string.Join(",", item.QuizAnswerStats.Where(t => !t.AnswerOptionsInQuizQuestions.IsUnansweredType).Select(t => t.AnswerOptionsInQuizQuestions.Option).Select(s => "'" + s + "'")), quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt);
                                                    else
                                                        yourAnswer = item.QuizAnswerStats.Select(t => t.AnswerText).FirstOrDefault();

                                                    quizAnswerSubmit.ResultScore.AttemptedQuestionAnswerDetails.Add(new QuizAnswerSubmit.AnswerResult
                                                    {
                                                        Question = VariableLinking(item.QuestionsInQuiz.Question, quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt),
                                                        YourAnswer = string.IsNullOrEmpty(yourAnswer) ? notAttemptedQuesText : yourAnswer,
                                                        CorrectAnswer = (quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate) ? string.Empty : correctAnswerTxt,
                                                        IsCorrect = IsCorrectValue,
                                                        AnswerExplanation = item.QuestionsInQuiz.CorrectAnswerDescription,
                                                        AssociatedScore = (quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate) ? associatedScore : default(int),
                                                    });
                                                }
                                            }
                                            else
                                            {
                                                quizAnswerSubmit.ResultScore.ShowCorrectAnswer = false;
                                            }
                                            #endregion
                                        }
                                        else
                                        {
                                            #region Personality result Setting
                                            attemptedQuestions = quizAttemptsObj.QuizQuestionStats.Where(t => t.Status == (int)StatusEnum.Active && t.QuizAnswerStats.Any(r => !r.AnswerOptionsInQuizQuestions.IsUnansweredType));

                                            var personalitySetting = quizDetails.PersonalityResultSetting.FirstOrDefault();
                                            var personalityAnswerResultList = attemptedQuestions.SelectMany(x => x.QuizAnswerStats.SelectMany(r => r.AnswerOptionsInQuizQuestions.PersonalityAnswerResultMapping));
                                            if (personalitySetting != null && personalitySetting.Status == (int)StatusEnum.Active)
                                            {
                                                #region personality Setting Active
                                                var personalityResultList = new List<QuizAnswerSubmit.PersonalityResult>();
                                                var countResult = 0;
                                                quizStatsObj = quizAttemptsObj.QuizStats.FirstOrDefault();

                                                if (personalityAnswerResultList.Any())
                                                {
                                                    #region correlation is available
                                                    var mappedResultList = personalityAnswerResultList.OrderBy(x => x.QuizResults.DisplayOrder).GroupBy(r => r.ResultId).OrderByDescending(i => i.Count()).Take(personalitySetting.MaxResult).Select(x => x.Key).ToList();
                                                    var quizresultList = quizDetails.QuizResults.Where(r => !r.IsPersonalityCorrelatedResult && mappedResultList.Contains(r.Id)).OrderBy(x => mappedResultList.IndexOf(x.Id));
                                                    foreach (var quizresult in quizresultList)
                                                    {
                                                        QuizAnswerSubmit.PersonalityResult personalityresult = new QuizAnswerSubmit.PersonalityResult();
                                                        personalityresult.Title = VariableLinking(quizresult.Title, quizDetails, quizAttemptsObj, false, false, null);
                                                        personalityresult.InternalTitle = VariableLinking(quizresult.InternalTitle, quizDetails, quizAttemptsObj, false, false, null);
                                                        personalityresult.Image = quizresult.Image;
                                                        personalityresult.ResultId = quizresult.Id;
                                                        personalityresult.GraphColor = personalitySetting.GraphColor;
                                                        personalityresult.ButtonColor = personalitySetting.ButtonColor;
                                                        personalityresult.ButtonFontColor = personalitySetting.ButtonFontColor;
                                                        personalityresult.MaxResult = personalitySetting.MaxResult;
                                                        personalityresult.SideButtonText = personalitySetting.SideButtonText;
                                                        personalityresult.IsFullWidthEnable = personalitySetting.IsFullWidthEnable;
                                                        personalityresult.Description = VariableLinking(quizresult.Description, quizDetails, quizAttemptsObj, false, false, null);
                                                        int count = personalityAnswerResultList.Where(k => k.ResultId == quizresult.Id).Count();
                                                        personalityresult.Percentage = (int)Math.Round((double)(100 * count) / personalityAnswerResultList.Count());
                                                        personalityresult.ShowInternalTitle = quizresult.ShowInternalTitle;
                                                        personalityresult.ShowExternalTitle = quizresult.ShowExternalTitle;
                                                        personalityresult.ShowDescription = quizresult.ShowDescription;
                                                        personalityResultList.Add(personalityresult);

                                                        if (quizStatsObj != null)
                                                        {
                                                            if (countResult == 0)
                                                            {
                                                                quizStatsObj.ResultId = quizresult.Id;
                                                                UOWObj.QuizStatsRepository.Update(quizStatsObj);
                                                            }
                                                            else
                                                            {
                                                                var resultQuizStats = new Db.QuizStats();
                                                                resultQuizStats.StartedOn = quizStatsObj.StartedOn;
                                                                resultQuizStats.QuizAttemptId = quizStatsObj.QuizAttemptId;
                                                                resultQuizStats.ResultId = quizresult.Id;
                                                                UOWObj.QuizStatsRepository.Insert(resultQuizStats);
                                                            }
                                                            UOWObj.Save();
                                                            countResult++;
                                                        }
                                                    }

                                                    quizAnswerSubmit.ShowLeadUserForm = (Mode == "PREVIEW" || Mode == "PREVIEWTEMPLATE") ? false : (resultIdList.Any() ? (resultIdList.Any(r => quizresultList.Any(l => l.Id == r) || quizComponentLogsList.Any(q => q.DraftedObjectId == r && quizresultList.Any(l => l.Id == q.PublishedObjectId)))) : (quizAttemptsObj.LeadUserId == null && quizAttemptsObj.RecruiterUserId == null && ((quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Personality && quizDetails.PersonalityResultSetting.FirstOrDefault().Status == (int)StatusEnum.Active) ? quizDetails.PersonalityResultSetting.FirstOrDefault().ShowLeadUserForm : quizresultList.Any(r => r.ShowLeadUserForm))));

                                                    if (leadFormDetailofResultsLst != null && leadFormDetailofResultsLst.Any() && quizAnswerSubmit.ShowLeadUserForm)
                                                    {
                                                        var personalityResult = personalityResultList.OrderByDescending(r => r.Percentage.Value).FirstOrDefault();
                                                        if (leadFormDetailofResultsLst.Any(r => r.ResultId == personalityResult.ResultId || quizComponentLogsList.Any(q => q.DraftedObjectId == r.ResultId && q.PublishedObjectId == personalityResult.ResultId)))
                                                        {
                                                            var leadFormDetailofResultsObj = leadFormDetailofResultsLst.FirstOrDefault(r => r.ResultId == personalityResult.ResultId || quizComponentLogsList.Any(q => q.DraftedObjectId == r.ResultId && q.PublishedObjectId == personalityResult.ResultId));
                                                            quizAnswerSubmit.FormId = leadFormDetailofResultsObj.FormId;
                                                            quizAnswerSubmit.FlowOrder = leadFormDetailofResultsObj.FlowOrder;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        quizAnswerSubmit.FormId = null;
                                                        quizAnswerSubmit.FlowOrder = null;
                                                    }
                                                    #endregion
                                                }
                                                else
                                                {
                                                    #region correlation is not available

                                                    var quizresultList = quizDetails.QuizResults.OrderBy(x => x.DisplayOrder).Take(personalitySetting.MaxResult);
                                                    foreach (var quizresult in quizresultList)
                                                    {
                                                        QuizAnswerSubmit.PersonalityResult personalityresult = new QuizAnswerSubmit.PersonalityResult();
                                                        personalityresult.Title = VariableLinking(quizresult.Title, quizDetails, quizAttemptsObj, false, false, null);
                                                        personalityresult.InternalTitle = VariableLinking(quizresult.InternalTitle, quizDetails, quizAttemptsObj, false, false, null);
                                                        personalityresult.Image = quizresult.Image;
                                                        personalityresult.ResultId = quizresult.Id;
                                                        personalityresult.GraphColor = personalitySetting.GraphColor;
                                                        personalityresult.ButtonColor = personalitySetting.ButtonColor;
                                                        personalityresult.ButtonFontColor = personalitySetting.ButtonFontColor;
                                                        personalityresult.MaxResult = personalitySetting.MaxResult;
                                                        personalityresult.SideButtonText = personalitySetting.SideButtonText;
                                                        personalityresult.IsFullWidthEnable = personalitySetting.IsFullWidthEnable;
                                                        personalityresult.Description = VariableLinking(quizresult.Description, quizDetails, quizAttemptsObj, false, false, null);
                                                        personalityresult.Percentage = null;
                                                        personalityresult.ShowInternalTitle = quizresult.ShowInternalTitle;
                                                        personalityresult.ShowExternalTitle = quizresult.ShowExternalTitle;
                                                        personalityresult.ShowDescription = quizresult.ShowDescription;
                                                        personalityResultList.Add(personalityresult);

                                                        if (quizStatsObj != null)
                                                        {
                                                            if (countResult == 0)
                                                            {
                                                                quizStatsObj.ResultId = quizresult.Id;
                                                                UOWObj.QuizStatsRepository.Update(quizStatsObj);
                                                            }
                                                            else
                                                            {
                                                                var resultQuizStats = new Db.QuizStats();
                                                                resultQuizStats.StartedOn = quizStatsObj.StartedOn;
                                                                resultQuizStats.QuizAttemptId = quizStatsObj.QuizAttemptId;
                                                                resultQuizStats.ResultId = quizresult.Id;
                                                                UOWObj.QuizStatsRepository.Insert(resultQuizStats);
                                                            }
                                                            UOWObj.Save();
                                                            countResult++;
                                                        }
                                                    }

                                                    quizAnswerSubmit.ShowLeadUserForm = (Mode == "PREVIEW" || Mode == "PREVIEWTEMPLATE") ? false : (resultIdList.Any() ? (resultIdList.Any(r => quizresultList.Any(l => l.Id == r) || quizComponentLogsList.Any(q => q.DraftedObjectId == r && quizresultList.Any(l => l.Id == q.PublishedObjectId)))) : (quizAttemptsObj.LeadUserId == null && quizAttemptsObj.RecruiterUserId == null && ((quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Personality && quizDetails.PersonalityResultSetting.FirstOrDefault().Status == (int)StatusEnum.Active) ? quizDetails.PersonalityResultSetting.FirstOrDefault().ShowLeadUserForm : quizresultList.Any(r => r.ShowLeadUserForm))));

                                                    if (leadFormDetailofResultsLst != null && leadFormDetailofResultsLst.Any() && quizAnswerSubmit.ShowLeadUserForm)
                                                    {
                                                        var personalityResult = personalityResultList.FirstOrDefault();
                                                        if (leadFormDetailofResultsLst.Any(r => r.ResultId == personalityResult.ResultId || quizComponentLogsList.Any(q => q.DraftedObjectId == r.ResultId && q.PublishedObjectId == personalityResult.ResultId)))
                                                        {
                                                            var leadFormDetailofResultsObj = leadFormDetailofResultsLst.FirstOrDefault(r => r.ResultId == personalityResult.ResultId || quizComponentLogsList.Any(q => q.DraftedObjectId == r.ResultId && q.PublishedObjectId == personalityResult.ResultId));
                                                            quizAnswerSubmit.FormId = leadFormDetailofResultsObj.FormId;
                                                            quizAnswerSubmit.FlowOrder = leadFormDetailofResultsObj.FlowOrder;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        quizAnswerSubmit.FormId = null;
                                                        quizAnswerSubmit.FlowOrder = null;
                                                    }
                                                    #endregion
                                                }

                                                quizAnswerSubmit.ResultScore.ShowInternalTitle = true;
                                                quizAnswerSubmit.ResultScore.ShowExternalTitle = true;
                                                quizAnswerSubmit.ResultScore.ShowDescription = true;
                                                quizAnswerSubmit.ResultScore.Title = personalitySetting.Title;
                                                quizAnswerSubmit.ResultScore.PersonalityResultList = personalityResultList.OrderByDescending(x => x.Percentage).ToList();
                                                #endregion
                                            }
                                            else
                                            {
                                                #region if there is correlation available but personalitySetting is disabled

                                                quizAnswerSubmit.ResultScore.ResultScoreValueTxt = string.Empty;
                                                quizAnswerSubmit.ResultScore.ShowScoreValue = false;

                                                quizAnswerSubmit.ResultScore.HideSocialShareButtons = quizDetails.HideSocialShareButtons.HasValue && quizDetails.HideSocialShareButtons.Value ? quizDetails.HideSocialShareButtons.Value : false;
                                                quizAnswerSubmit.ResultScore.ShowFacebookBtn = !quizAnswerSubmit.ResultScore.HideSocialShareButtons && quizDetails.EnableFacebookShare.HasValue ? quizDetails.EnableFacebookShare.Value : false;
                                                quizAnswerSubmit.ResultScore.ShowTwitterBtn = !quizAnswerSubmit.ResultScore.HideSocialShareButtons && quizDetails.EnableTwitterShare.HasValue ? quizDetails.EnableTwitterShare.Value : false;
                                                quizAnswerSubmit.ResultScore.ShowLinkedinBtn = !quizAnswerSubmit.ResultScore.HideSocialShareButtons && quizDetails.EnableLinkedinShare.HasValue ? quizDetails.EnableLinkedinShare.Value : false;
                                                quizAnswerSubmit.ResultScore.DisplayOrderForTitle = quizAnswerSubmit.ResultScore.DisplayOrderForTitle;
                                                quizAnswerSubmit.ResultScore.DisplayOrderForTitleImage = quizAnswerSubmit.ResultScore.DisplayOrderForTitleImage;
                                                quizAnswerSubmit.ResultScore.DisplayOrderForDescription = quizAnswerSubmit.ResultScore.DisplayOrderForDescription;
                                                quizAnswerSubmit.ResultScore.DisplayOrderForNextButton = quizAnswerSubmit.ResultScore.DisplayOrderForNextButton;

                                                if (resultSetting.ShowCorrectAnswer.HasValue && resultSetting.ShowCorrectAnswer.Value)
                                                {
                                                    quizAnswerSubmit.ResultScore.AnswerKeyCustomTxt = resultSetting.CustomTxtForAnswerKey;
                                                    quizAnswerSubmit.ResultScore.YourAnswerCustomTxt = resultSetting.CustomTxtForYourAnswer;
                                                    quizAnswerSubmit.ResultScore.CorrectAnswerCustomTxt = resultSetting.CustomTxtForCorrectAnswer;
                                                    quizAnswerSubmit.ResultScore.ExplanationCustomTxt = resultSetting.CustomTxtForExplanation;
                                                    quizAnswerSubmit.ResultScore.ShowCorrectAnswer = true;
                                                    quizAnswerSubmit.ResultScore.AttemptedQuestionAnswerDetails = new List<QuizAnswerSubmit.AnswerResult>();


                                                    foreach (var item in attemptedQuestions.Where(r => r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single))
                                                    {
                                                        quizAnswerSubmit.ResultScore.AttemptedQuestionAnswerDetails.Add(new QuizAnswerSubmit.AnswerResult
                                                        {
                                                            Question = VariableLinking(item.QuestionsInQuiz.Question, quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt),
                                                            YourAnswer = VariableLinking(string.Join(",", item.QuizAnswerStats.Where(t => !t.AnswerOptionsInQuizQuestions.IsUnansweredType).Select(t => t.AnswerOptionsInQuizQuestions.Option).Select(s => "'" + s + "'")), quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt),
                                                            CorrectAnswer = string.Empty,
                                                            IsCorrect = null,
                                                            AnswerExplanation = item.QuestionsInQuiz.CorrectAnswerDescription,
                                                            AssociatedScore = default(int)
                                                        });
                                                    }
                                                }
                                                else
                                                {
                                                    quizAnswerSubmit.ResultScore.ShowCorrectAnswer = false;
                                                }
                                                #endregion
                                            }
                                            #endregion
                                        }
                                    }
                                    else
                                    {
                                        #region resultSetting is null

                                        var attemptedQuestions = quizAttemptsObj.QuizQuestionStats.Where(t => t.Status == (int)StatusEnum.Active && t.CompletedOn != null && (t.QuestionsInQuiz.TimerRequired ? true : t.QuizAnswerStats.Any(r => !r.AnswerOptionsInQuizQuestions.IsUnansweredType)));

                                        string scoreValueTxt = string.Empty;
                                        var correctAnsCount = 0;
                                        var showScoreValue = false;

                                        if ((quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate) && attemptedQuestions.Any(r => r.QuestionsInQuiz.Status == (int)StatusEnum.Active && (r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple)))
                                        {
                                            scoreValueTxt = resultSetting.CustomTxtForScoreValueInResult ?? "You scored a%score%";
                                            correctAnsCount = attemptedQuestions.Sum(a => a.QuizAnswerStats.Sum(x => x.AnswerOptionsInQuizQuestions.AssociatedScore)).Value;
                                            showScoreValue = true;
                                        }
                                        else if ((quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Assessment || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.AssessmentTemplate) && attemptedQuestions.Any(r => (r.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Any(t => t.Status == (int)StatusEnum.Active && t.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && t.IsCorrectForMultipleAnswer.HasValue && t.IsCorrectForMultipleAnswer.Value)) || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single))
                                        {
                                            scoreValueTxt = resultSetting.CustomTxtForScoreValueInResult ?? "YOU GOT%score%OUT OF%total%CORRECT";
                                            correctAnsCount = attemptedQuestions.Count(a => (a.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && ((a.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Select(s => s.Id).OrderBy(s => s)).SequenceEqual(a.QuizAnswerStats.Select(r => r.AnswerId).OrderBy(s => s)))) || (a.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single && a.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer == true));
                                            showScoreValue = true;
                                        }

                                        scoreValueTxt = scoreValueTxt == null ? string.Empty : scoreValueTxt.Replace("%score%", ' ' + correctAnsCount.ToString() + ' ').Replace("%total%", ' ' + attemptedQuestions.Count(t => (t.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Any(r => r.Status == (int)StatusEnum.Active && r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value)) || t.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single).ToString() + ' ');

                                        quizAnswerSubmit.ShowLeadUserForm = false;
                                        quizAnswerSubmit.ResultScore.ResultScoreValueTxt = scoreValueTxt;
                                        quizAnswerSubmit.ResultScore.ShowScoreValue = showScoreValue;

                                        #endregion
                                    }
                                    #endregion
                                }

                                #endregion

                                break;
                            case "complete_content":

                                #region fetch next content details

                                if (isQuesAndContentInSameTable)
                                {
                                    if (!isLastQuestionAttempted)
                                    {
                                        quizQuestionStatsObj = quizAttemptsObj.QuizQuestionStats.Where(r => r.QuestionId == QuestionId && r.Status == (int)StatusEnum.Active).FirstOrDefault();

                                        quizQuestionStatsObj.CompletedOn = currentDate;
                                        quizQuestionStatsObj.Status = (int)StatusEnum.Active;

                                        UOWObj.QuizQuestionStatsRepository.Update(quizQuestionStatsObj);
                                        UOWObj.Save();
                                    }
                                }
                                else
                                {
                                    if (!quizAttemptsObj.QuizObjectStats.Any(a => a.TypeId == (int)BranchingLogicEnum.CONTENT && a.ObjectId == QuestionId && a.Status == (int)StatusEnum.Active))
                                    {
                                        var quizBadgeStatsObj = new Db.QuizObjectStats();

                                        quizBadgeStatsObj.QuizAttemptId = quizAttemptsObj.Id;
                                        quizBadgeStatsObj.ObjectId = QuestionId;
                                        quizBadgeStatsObj.ViewedOn = currentDate;
                                        quizBadgeStatsObj.TypeId = (int)BranchingLogicEnum.CONTENT;
                                        quizBadgeStatsObj.Status = (int)StatusEnum.Active;

                                        UOWObj.QuizObjectStatsRepository.Insert(quizBadgeStatsObj);
                                        UOWObj.Save();
                                    }
                                }

                                nextQuestionObj = FetchNextQuestion(quizDetails, QuestionId, (int)BranchingLogicEnum.CONTENTNEXT, UOWObj, isQuesAndContentInSameTable);

                                if (nextQuestionObj != null)
                                {
                                    #region if next is question type

                                    if (nextQuestionObj.GetType().BaseType.Name == "QuestionsInQuiz")
                                    {
                                        if (((Db.QuestionsInQuiz)nextQuestionObj).Type == (int)BranchingLogicEnum.QUESTION)
                                        {
                                            quizAnswerSubmit.QuestionType = (int)BranchingLogicEnum.QUESTION;

                                            quizAnswerSubmit.QuestionDetails = new QuizQuestion();

                                            quizAnswerSubmit.QuestionDetails.QuestionId = ((Db.QuestionsInQuiz)nextQuestionObj).Id;
                                            quizAnswerSubmit.QuestionDetails.QuestionTitle = VariableLinking(((Db.QuestionsInQuiz)nextQuestionObj).Question, quizDetails, quizAttemptsObj, false, false, null);
                                            quizAnswerSubmit.QuestionDetails.ShowTitle = ((Db.QuestionsInQuiz)nextQuestionObj).ShowTitle;

                                            if (((Db.QuestionsInQuiz)nextQuestionObj).ShowQuestionImage.HasValue && ((Db.QuestionsInQuiz)nextQuestionObj).ShowQuestionImage.Value && ((Db.QuestionsInQuiz)nextQuestionObj).EnableMediaFile && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION && r.ObjectId == ((Db.QuestionsInQuiz)nextQuestionObj).Id))
                                            {
                                                var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION && r.ObjectId == ((Db.QuestionsInQuiz)nextQuestionObj).Id);
                                                quizAnswerSubmit.QuestionDetails.QuestionImage = mediaObj.ObjectValue;
                                                quizAnswerSubmit.QuestionDetails.PublicIdForQuestion = mediaObj.ObjectPublicId;

                                                var newMedia = ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers);
                                                if (newMedia != null)
                                                {
                                                    quizAnswerSubmit.QuestionDetails.QuestionImage = newMedia.MediaUrl;
                                                    quizAnswerSubmit.QuestionDetails.PublicIdForQuestion = newMedia.MediaPublicId;
                                                }
                                            }
                                            else
                                            {
                                                quizAnswerSubmit.QuestionDetails.QuestionImage = ((Db.QuestionsInQuiz)nextQuestionObj).ShowQuestionImage.HasValue && ((Db.QuestionsInQuiz)nextQuestionObj).ShowQuestionImage.Value ? ((Db.QuestionsInQuiz)nextQuestionObj).QuestionImage : string.Empty;
                                                quizAnswerSubmit.QuestionDetails.PublicIdForQuestion = ((Db.QuestionsInQuiz)nextQuestionObj).ShowQuestionImage.HasValue && ((Db.QuestionsInQuiz)nextQuestionObj).ShowQuestionImage.Value ? ((Db.QuestionsInQuiz)nextQuestionObj).PublicId : string.Empty;
                                            }

                                            quizAnswerSubmit.QuestionDetails.ShowQuestionImage = ((Db.QuestionsInQuiz)nextQuestionObj).ShowQuestionImage;
                                            quizAnswerSubmit.QuestionDetails.ShowAnswerImage = ((Db.QuestionsInQuiz)nextQuestionObj).ShowAnswerImage;
                                            quizAnswerSubmit.QuestionDetails.AnswerType = ((Db.QuestionsInQuiz)nextQuestionObj).AnswerType;
                                            quizAnswerSubmit.QuestionDetails.NextButtonColor = ((Db.QuestionsInQuiz)nextQuestionObj).NextButtonColor;
                                            quizAnswerSubmit.QuestionDetails.NextButtonText = ((Db.QuestionsInQuiz)nextQuestionObj).NextButtonText;
                                            quizAnswerSubmit.QuestionDetails.NextButtonTxtColor = ((Db.QuestionsInQuiz)nextQuestionObj).NextButtonTxtColor;
                                            quizAnswerSubmit.QuestionDetails.NextButtonTxtSize = ((Db.QuestionsInQuiz)nextQuestionObj).NextButtonTxtSize;
                                            quizAnswerSubmit.QuestionDetails.EnableNextButton = ((Db.QuestionsInQuiz)nextQuestionObj).EnableNextButton;
                                            quizAnswerSubmit.QuestionDetails.MinAnswer = ((Db.QuestionsInQuiz)nextQuestionObj).MinAnswer;
                                            quizAnswerSubmit.QuestionDetails.MaxAnswer = ((Db.QuestionsInQuiz)nextQuestionObj).MaxAnswer;
                                            quizAnswerSubmit.QuestionDetails.ViewPreviousQuestion = ((Db.QuestionsInQuiz)nextQuestionObj).ViewPreviousQuestion;
                                            quizAnswerSubmit.QuestionDetails.EditAnswer = ((Db.QuestionsInQuiz)nextQuestionObj).EditAnswer;
                                            quizAnswerSubmit.IsBackButtonEnable = isQuesAndContentInSameTable ? quizDetails.QuestionsInQuiz.FirstOrDefault(r => r.Id == QuestionId).ViewPreviousQuestion : quizDetails.ContentsInQuiz.FirstOrDefault(r => r.Id == QuestionId).ViewPreviousQuestion;
                                            quizAnswerSubmit.QuestionDetails.StartedOn = (isLastQuestionStarted && ((Db.QuestionsInQuiz)nextQuestionObj).QuizQuestionStats != null && ((Db.QuestionsInQuiz)nextQuestionObj).QuizQuestionStats.Any(r => r.Status == (int)StatusEnum.Active)) ? ((Db.QuestionsInQuiz)nextQuestionObj).QuizQuestionStats.FirstOrDefault(r => r.Status == (int)StatusEnum.Active).StartedOn : default(DateTime?);
                                            quizAnswerSubmit.QuestionDetails.TimerRequired = ((Db.QuestionsInQuiz)nextQuestionObj).TimerRequired;
                                            quizAnswerSubmit.QuestionDetails.Time = ((Db.QuestionsInQuiz)nextQuestionObj).Time;
                                            quizAnswerSubmit.QuestionDetails.AutoPlay = ((Db.QuestionsInQuiz)nextQuestionObj).AutoPlay;
                                            quizAnswerSubmit.QuestionDetails.SecondsToApply = ((Db.QuestionsInQuiz)nextQuestionObj).SecondsToApply ?? "0";
                                            quizAnswerSubmit.QuestionDetails.VideoFrameEnabled = ((Db.QuestionsInQuiz)nextQuestionObj).VideoFrameEnabled ?? false;
                                            quizAnswerSubmit.QuestionDetails.DisplayOrderForTitle = ((Db.QuestionsInQuiz)nextQuestionObj).DisplayOrderForTitle;
                                            quizAnswerSubmit.QuestionDetails.DisplayOrderForTitleImage = ((Db.QuestionsInQuiz)nextQuestionObj).DisplayOrderForTitleImage;
                                            quizAnswerSubmit.QuestionDetails.DisplayOrderForDescription = ((Db.QuestionsInQuiz)nextQuestionObj).DisplayOrderForDescription;
                                            quizAnswerSubmit.QuestionDetails.DisplayOrderForDescriptionImage = ((Db.QuestionsInQuiz)nextQuestionObj).DisplayOrderForDescriptionImage;
                                            quizAnswerSubmit.QuestionDetails.DisplayOrderForAnswer = ((Db.QuestionsInQuiz)nextQuestionObj).DisplayOrderForAnswer;
                                            quizAnswerSubmit.QuestionDetails.DisplayOrderForNextButton = ((Db.QuestionsInQuiz)nextQuestionObj).DisplayOrderForNextButton;
                                            quizAnswerSubmit.QuestionDetails.Description = ((Db.QuestionsInQuiz)nextQuestionObj).Description;
                                            quizAnswerSubmit.QuestionDetails.ShowDescription = ((Db.QuestionsInQuiz)nextQuestionObj).ShowDescription;
                                            quizAnswerSubmit.QuestionDetails.ShowDescriptionImage = ((Db.QuestionsInQuiz)nextQuestionObj).ShowDescriptionImage ?? false;
                                            quizAnswerSubmit.QuestionDetails.AutoPlayForDescription = ((Db.QuestionsInQuiz)nextQuestionObj).AutoPlayForDescription;
                                            quizAnswerSubmit.QuestionDetails.SecondsToApplyForDescription = ((Db.QuestionsInQuiz)nextQuestionObj).SecondsToApplyForDescription ?? "0";
                                            quizAnswerSubmit.QuestionDetails.DescVideoFrameEnabled = ((Db.QuestionsInQuiz)nextQuestionObj).DescVideoFrameEnabled ?? false;
                                            quizAnswerSubmit.QuestionDetails.EnableComment = ((Db.QuestionsInQuiz)nextQuestionObj).EnableComment;
                                            quizAnswerSubmit.QuestionDetails.AnswerStructureType = ((Db.QuestionsInQuiz)nextQuestionObj).AnswerStructureType;

                                            if (((Db.QuestionsInQuiz)nextQuestionObj).EnableMediaFileForDescription && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Description && r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION && r.ObjectId == ((Db.QuestionsInQuiz)nextQuestionObj).Id))
                                            {
                                                var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Description && r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION && r.ObjectId == ((Db.QuestionsInQuiz)nextQuestionObj).Id);
                                                quizAnswerSubmit.QuestionDetails.DescriptionImage = mediaObj.ObjectValue ?? string.Empty;
                                                quizAnswerSubmit.QuestionDetails.PublicIdForDescription = mediaObj.ObjectPublicId ?? string.Empty;

                                                var newMedia = ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers);
                                                if (newMedia != null)
                                                {
                                                    quizAnswerSubmit.QuestionDetails.DescriptionImage = newMedia.MediaUrl;
                                                    quizAnswerSubmit.QuestionDetails.PublicIdForDescription = newMedia.MediaPublicId;
                                                }
                                            }
                                            else
                                            {
                                                quizAnswerSubmit.QuestionDetails.DescriptionImage = ((Db.QuestionsInQuiz)nextQuestionObj).DescriptionImage ?? string.Empty;
                                                quizAnswerSubmit.QuestionDetails.PublicIdForDescription = ((Db.QuestionsInQuiz)nextQuestionObj).PublicIdForDescription ?? string.Empty;
                                            }

                                            #region Previous question SubmittedAnswer

                                            if (quizAttemptsObj.QuizQuestionStats.Any(r => r.QuestionId == ((Db.QuestionsInQuiz)nextQuestionObj).Id && r.CompletedOn.HasValue))
                                            {
                                                var attemptedAnswerIds = quizAttemptsObj.QuizQuestionStats.Any(r => r.QuestionId == ((Db.QuestionsInQuiz)nextQuestionObj).Id && r.CompletedOn.HasValue)
                                                ? quizAttemptsObj.QuizQuestionStats.LastOrDefault(r => r.QuestionId == ((Db.QuestionsInQuiz)nextQuestionObj).Id && r.CompletedOn.HasValue).QuizAnswerStats.Select(r => r.AnswerId).ToList()
                                                : quizAttemptsObj.QuizQuestionStats.LastOrDefault(r => r.QuestionId == ((Db.QuestionsInQuiz)nextQuestionObj).Id).QuizAnswerStats.Select(r => r.AnswerId).ToList();

                                                var submittedAnswerOption = ((Db.QuestionsInQuiz)nextQuestionObj).AnswerOptionsInQuizQuestions.Where(r => attemptedAnswerIds.Contains(r.Id));
                                                PreviousQuestionSubmittedAnswerMapping(quizAnswerSubmit, nextQuestionObj);

                                                foreach (var submittedAnswerOptionObj in submittedAnswerOption.Where(r => r.Status == (int)StatusEnum.Active))
                                                    CheckQuizAnswerType(quizAnswerSubmit, quizAttemptsObj, quizDetails, nextQuestionObj, submittedAnswerOptionObj);

                                                quizAnswerSubmit.PreviousQuestionSubmittedAnswer.CorrectAnswerDetails = new List<QuizAnswerSubmit.SubmittedAnswerResult.CorrectAnswer>();
                                            }

                                            #endregion

                                            quizAnswerSubmit.QuestionDetails.AnswerList = new List<AnswerOptionInQuestion>();

                                            foreach (var ans in ((Db.QuestionsInQuiz)nextQuestionObj).AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && !r.IsUnansweredType).OrderBy(r => r.DisplayOrder))
                                            {
                                                var answerImage = string.Empty;
                                                var publicIdForAnswer = string.Empty;

                                                if (((Db.QuestionsInQuiz)nextQuestionObj).ShowAnswerImage.HasValue && ((Db.QuestionsInQuiz)nextQuestionObj).ShowAnswerImage.Value && ans.EnableMediaFile && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.ObjectId == ans.Id))
                                                {
                                                    var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.ObjectId == ans.Id);
                                                    answerImage = mediaObj.ObjectValue;
                                                    publicIdForAnswer = mediaObj.ObjectPublicId;

                                                    var newMedia = ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers);
                                                    if (newMedia != null)
                                                    {
                                                        answerImage = newMedia.MediaUrl;
                                                        publicIdForAnswer = newMedia.MediaPublicId;
                                                    }
                                                }
                                                else
                                                {
                                                    answerImage = ((Db.QuestionsInQuiz)nextQuestionObj).ShowAnswerImage.HasValue && ((Db.QuestionsInQuiz)nextQuestionObj).ShowAnswerImage.Value ? ans.OptionImage : string.Empty;
                                                    publicIdForAnswer = ((Db.QuestionsInQuiz)nextQuestionObj).ShowAnswerImage.HasValue && ((Db.QuestionsInQuiz)nextQuestionObj).ShowAnswerImage.Value ? ans.PublicId : string.Empty;
                                                }

                                                quizAnswerSubmit.QuestionDetails.AnswerList.Add(new AnswerOptionInQuestion
                                                {
                                                    AnswerId = ans.Id,
                                                    AssociatedScore = ans.AssociatedScore,
                                                    AnswerText = VariableLinking(ans.Option, quizDetails, quizAttemptsObj, false, false, null),
                                                    AnswerImage = answerImage,
                                                    PublicIdForAnswer = publicIdForAnswer,
                                                    IsCorrectAnswer = false,
                                                    DisplayOrder = ans.DisplayOrder,
                                                    IsUnansweredType = ans.IsUnansweredType,
                                                    AutoPlay = ans.AutoPlay,
                                                    SecondsToApply = ans.SecondsToApply,
                                                    VideoFrameEnabled = ans.VideoFrameEnabled,
                                                    ListValues = ans.ListValues,
                                                    //for Rating type question
                                                    OptionTextforRatingOne = ans.OptionTextforRatingOne,
                                                    OptionTextforRatingTwo = ans.OptionTextforRatingTwo,
                                                    OptionTextforRatingThree = ans.OptionTextforRatingThree,
                                                    OptionTextforRatingFour = ans.OptionTextforRatingFour,
                                                    OptionTextforRatingFive = ans.OptionTextforRatingFive
                                                });
                                            }

                                            if (!isLastQuestionStarted)
                                            {
                                                quizQuestionStatsObj = new Db.QuizQuestionStats();

                                                quizQuestionStatsObj.QuizAttemptId = quizAttemptsObj.Id;
                                                quizQuestionStatsObj.QuestionId = ((Db.QuestionsInQuiz)nextQuestionObj).Id;
                                                quizQuestionStatsObj.StartedOn = currentDate;
                                                quizQuestionStatsObj.Status = (int)StatusEnum.Active;

                                                UOWObj.QuizQuestionStatsRepository.Insert(quizQuestionStatsObj);
                                                UOWObj.Save();
                                            }
                                        }
                                        else if (((Db.QuestionsInQuiz)nextQuestionObj).Type == (int)BranchingLogicEnum.CONTENT)
                                        {
                                            quizAnswerSubmit.QuestionType = (int)BranchingLogicEnum.CONTENT;

                                            if (!isLastQuestionStarted)
                                            {
                                                quizQuestionStatsObj = new Db.QuizQuestionStats();

                                                quizQuestionStatsObj.QuizAttemptId = quizAttemptsObj.Id;
                                                quizQuestionStatsObj.QuestionId = ((Db.QuestionsInQuiz)nextQuestionObj).Id;
                                                quizQuestionStatsObj.StartedOn = currentDate;
                                                quizQuestionStatsObj.Status = (int)StatusEnum.Active;

                                                UOWObj.QuizQuestionStatsRepository.Insert(quizQuestionStatsObj);
                                                UOWObj.Save();
                                            }

                                            quizAnswerSubmit.ContentDetails = new QuizContent();

                                            quizAnswerSubmit.ContentDetails.Id = ((Db.QuestionsInQuiz)nextQuestionObj).Id;
                                            quizAnswerSubmit.ContentDetails.ContentTitle = VariableLinking(((Db.QuestionsInQuiz)nextQuestionObj).Question, quizDetails, quizAttemptsObj, false, false, null);
                                            quizAnswerSubmit.ContentDetails.ContentDescription = VariableLinking(((Db.QuestionsInQuiz)nextQuestionObj).Description, quizDetails, quizAttemptsObj, false, false, null);
                                            quizAnswerSubmit.ContentDetails.ShowDescription = ((Db.QuestionsInQuiz)nextQuestionObj).ShowDescription;

                                            if ((((Db.QuestionsInQuiz)nextQuestionObj).EnableMediaFile && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Title && r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == ((Db.QuestionsInQuiz)nextQuestionObj).Id)))
                                            {
                                                var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Title && r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == ((Db.QuestionsInQuiz)nextQuestionObj).Id);
                                                quizAnswerSubmit.ContentDetails.ContentTitleImage = mediaObj.ObjectValue;
                                                quizAnswerSubmit.ContentDetails.PublicIdForContentTitle = mediaObj.ObjectPublicId ?? string.Empty;

                                                var newMedia = ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers);
                                                if (newMedia != null)
                                                {
                                                    quizAnswerSubmit.ContentDetails.ContentTitleImage = newMedia.MediaUrl;
                                                    quizAnswerSubmit.ContentDetails.PublicIdForContentTitle = newMedia.MediaPublicId;
                                                }
                                            }
                                            else
                                            {
                                                quizAnswerSubmit.ContentDetails.ContentTitleImage = ((Db.QuestionsInQuiz)nextQuestionObj).QuestionImage ?? string.Empty;
                                                quizAnswerSubmit.ContentDetails.PublicIdForContentTitle = ((Db.QuestionsInQuiz)nextQuestionObj).PublicId ?? string.Empty;
                                            }

                                            quizAnswerSubmit.ContentDetails.ShowContentTitleImage = ((Db.QuestionsInQuiz)nextQuestionObj).ShowQuestionImage.HasValue && ((Db.QuestionsInQuiz)nextQuestionObj).ShowQuestionImage.Value ? ((Db.QuestionsInQuiz)nextQuestionObj).ShowQuestionImage.Value : false;
                                            quizAnswerSubmit.ContentDetails.AliasTextForNextButton = ((Db.QuestionsInQuiz)nextQuestionObj).NextButtonText;
                                            quizAnswerSubmit.ContentDetails.EnableNextButton = ((Db.QuestionsInQuiz)nextQuestionObj).EnableNextButton;

                                            if (((Db.QuestionsInQuiz)nextQuestionObj).EnableMediaFileForDescription && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Description && r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == ((Db.QuestionsInQuiz)nextQuestionObj).Id))
                                            {
                                                var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Description && r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == ((Db.QuestionsInQuiz)nextQuestionObj).Id);
                                                quizAnswerSubmit.ContentDetails.ContentDescriptionImage = mediaObj.ObjectValue ?? string.Empty;
                                                quizAnswerSubmit.ContentDetails.PublicIdForContentDescription = mediaObj.ObjectPublicId ?? string.Empty;

                                                var newMedia = ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers);
                                                if (newMedia != null)
                                                {
                                                    quizAnswerSubmit.ContentDetails.ContentDescriptionImage = newMedia.MediaUrl;
                                                    quizAnswerSubmit.ContentDetails.PublicIdForContentDescription = newMedia.MediaPublicId;
                                                }
                                            }
                                            else
                                            {
                                                quizAnswerSubmit.ContentDetails.ContentDescriptionImage = ((Db.QuestionsInQuiz)nextQuestionObj).DescriptionImage ?? string.Empty;
                                                quizAnswerSubmit.ContentDetails.PublicIdForContentDescription = ((Db.QuestionsInQuiz)nextQuestionObj).PublicIdForDescription ?? string.Empty;
                                            }

                                            quizAnswerSubmit.ContentDetails.ShowContentDescriptionImage = ((Db.QuestionsInQuiz)nextQuestionObj).ShowDescriptionImage.HasValue && ((Db.QuestionsInQuiz)nextQuestionObj).ShowDescriptionImage.Value ? ((Db.QuestionsInQuiz)nextQuestionObj).ShowDescriptionImage.Value : false;
                                            quizAnswerSubmit.ContentDetails.ViewPreviousQuestion = ((Db.QuestionsInQuiz)nextQuestionObj).ViewPreviousQuestion;
                                            quizAnswerSubmit.ContentDetails.AutoPlay = ((Db.QuestionsInQuiz)nextQuestionObj).AutoPlay;
                                            quizAnswerSubmit.ContentDetails.SecondsToApply = ((Db.QuestionsInQuiz)nextQuestionObj).SecondsToApply ?? "0";
                                            quizAnswerSubmit.ContentDetails.VideoFrameEnabled = ((Db.QuestionsInQuiz)nextQuestionObj).VideoFrameEnabled ?? false;
                                            quizAnswerSubmit.ContentDetails.AutoPlayForDescription = ((Db.QuestionsInQuiz)nextQuestionObj).AutoPlayForDescription;
                                            quizAnswerSubmit.ContentDetails.SecondsToApplyForDescription = ((Db.QuestionsInQuiz)nextQuestionObj).SecondsToApplyForDescription ?? "0";
                                            quizAnswerSubmit.ContentDetails.DescVideoFrameEnabled = ((Db.QuestionsInQuiz)nextQuestionObj).DescVideoFrameEnabled ?? false;
                                            quizAnswerSubmit.ContentDetails.ShowTitle = ((Db.QuestionsInQuiz)nextQuestionObj).ShowTitle;
                                            quizAnswerSubmit.ContentDetails.DisplayOrderForTitle = ((Db.QuestionsInQuiz)nextQuestionObj).DisplayOrderForTitle;
                                            quizAnswerSubmit.ContentDetails.DisplayOrderForTitleImage = ((Db.QuestionsInQuiz)nextQuestionObj).DisplayOrderForTitleImage;
                                            quizAnswerSubmit.ContentDetails.DisplayOrderForDescription = ((Db.QuestionsInQuiz)nextQuestionObj).DisplayOrderForDescription;
                                            quizAnswerSubmit.ContentDetails.DisplayOrderForDescriptionImage = ((Db.QuestionsInQuiz)nextQuestionObj).DisplayOrderForDescriptionImage;
                                            quizAnswerSubmit.ContentDetails.DisplayOrderForNextButton = ((Db.QuestionsInQuiz)nextQuestionObj).DisplayOrderForNextButton;
                                            quizAnswerSubmit.IsBackButtonEnable = isQuesAndContentInSameTable ? quizDetails.QuestionsInQuiz.FirstOrDefault(r => r.Id == QuestionId).ViewPreviousQuestion : quizDetails.ContentsInQuiz.FirstOrDefault(r => r.Id == QuestionId).ViewPreviousQuestion;
                                        }
                                    }

                                    #endregion

                                    #region if next is result type
                                    if (nextQuestionObj.GetType().BaseType.Name == "QuizResults")
                                    {
                                        quizAnswerSubmit.QuestionType = (int)BranchingLogicEnum.RESULT;

                                        quizAnswerSubmit.IsBackButtonEnable = false;

                                        var result = ((Db.QuizResults)nextQuestionObj);
                                        var attemptedQuestions = quizAttemptsObj.QuizQuestionStats.Where(t => t.Status == (int)StatusEnum.Active && t.CompletedOn != null && (t.QuestionsInQuiz.TimerRequired ? true : t.QuizAnswerStats.Any(r => !r.AnswerOptionsInQuizQuestions.IsUnansweredType)));

                                        quizAnswerSubmit.ShowLeadUserForm = (Mode == "PREVIEW" || Mode == "PREVIEWTEMPLATE") ? false : (resultIdList.Any() ? (resultIdList.Any(r => r == result.Id || quizComponentLogsList.Any(q => q.DraftedObjectId == r && q.PublishedObjectId == result.Id))) : (quizAttemptsObj.LeadUserId == null && quizAttemptsObj.RecruiterUserId == null && ((quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Personality && quizDetails.PersonalityResultSetting.FirstOrDefault().Status == (int)StatusEnum.Active) ? quizDetails.PersonalityResultSetting.FirstOrDefault().ShowLeadUserForm : result.ShowLeadUserForm)));
                                        quizAnswerSubmit.CompanyCode = quizDetails.Quiz.Company.ClientCode;

                                        if (leadFormDetailofResultsLst != null && leadFormDetailofResultsLst.Any() && quizAnswerSubmit.ShowLeadUserForm)
                                        {
                                            var leadFormDetailofResultsObj = leadFormDetailofResultsLst.FirstOrDefault(r => r.ResultId == result.Id || quizComponentLogsList.Any(q => q.DraftedObjectId == r.ResultId && q.PublishedObjectId == result.Id));
                                            quizAnswerSubmit.FormId = leadFormDetailofResultsObj.FormId;
                                            quizAnswerSubmit.FlowOrder = leadFormDetailofResultsObj.FlowOrder;
                                        }

                                        string scoreValueTxt = string.Empty;

                                        if (quizDetails.Quiz.QuizType != (int)QuizTypeEnum.Personality && quizDetails.Quiz.QuizType != (int)QuizTypeEnum.PersonalityTemplate)
                                        {
                                            #region non personality result Setting

                                            quizAnswerSubmit.ResultScore = new QuizAnswerSubmit.Result();

                                            if (result.EnableMediaFile && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.RESULT && r.ObjectId == result.Id))
                                            {
                                                var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.RESULT && r.ObjectId == result.Id);
                                                quizAnswerSubmit.ResultScore.Image = result.ShowResultImage ? mediaObj.ObjectValue : string.Empty;
                                                quizAnswerSubmit.ResultScore.PublicIdForResult = result.ShowResultImage ? mediaObj.ObjectPublicId : string.Empty;

                                                var newMedia = result.ShowResultImage ? ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers) : null;
                                                if (newMedia != null)
                                                {
                                                    quizAnswerSubmit.ResultScore.Image = newMedia.MediaUrl;
                                                    quizAnswerSubmit.ResultScore.PublicIdForResult = newMedia.MediaPublicId;
                                                }
                                            }
                                            else
                                            {
                                                quizAnswerSubmit.ResultScore.Image = result.ShowResultImage ? result.Image : string.Empty;
                                                quizAnswerSubmit.ResultScore.PublicIdForResult = result.ShowResultImage ? result.PublicId : string.Empty;
                                            }
                                            quizAnswerSubmit.ResultScore.HideActionButton = result.HideCallToAction.HasValue ? result.HideCallToAction.Value : false;
                                            quizAnswerSubmit.ResultScore.ActionButtonURL = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonURL;
                                            quizAnswerSubmit.ResultScore.OpenLinkInNewTab = quizAnswerSubmit.ResultScore.HideActionButton ? false : result.OpenLinkInNewTab;
                                            quizAnswerSubmit.ResultScore.ActionButtonTxtSize = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonTxtSize;
                                            quizAnswerSubmit.ResultScore.ActionButtonColor = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonColor;
                                            quizAnswerSubmit.ResultScore.ActionButtonTitleColor = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonTitleColor;
                                            quizAnswerSubmit.ResultScore.ActionButtonText = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonText;

                                            var resultSetting = quizDetails.ResultSettings.FirstOrDefault();
                                            var correctAnsCount = 0;
                                            if (resultSetting != null)
                                            {

                                                if (resultSetting.ShowScoreValue.HasValue && resultSetting.ShowScoreValue.Value && (((quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate) && attemptedQuestions.Any(r => r.QuestionsInQuiz.Status == (int)StatusEnum.Active && (r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple))) || attemptedQuestions.Any(r => (r.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Any(t => t.Status == (int)StatusEnum.Active && t.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && t.IsCorrectForMultipleAnswer.HasValue && t.IsCorrectForMultipleAnswer.Value)) || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single)))
                                                {
                                                    scoreValueTxt = resultSetting.CustomTxtForScoreValueInResult;

                                                    if (quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                                        correctAnsCount = attemptedQuestions.Sum(a => a.QuizAnswerStats.Sum(x => x.AnswerOptionsInQuizQuestions.AssociatedScore)).Value;
                                                    else
                                                        correctAnsCount = attemptedQuestions.Count(a => (a.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && ((a.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Select(s => s.Id).OrderBy(s => s)).SequenceEqual(a.QuizAnswerStats.Select(r => r.AnswerId).OrderBy(s => s)))) || (a.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single && a.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer == true));

                                                    scoreValueTxt = scoreValueTxt == null ? string.Empty : scoreValueTxt.Replace("%score%", ' ' + correctAnsCount.ToString() + ' ').Replace("%total%", ' ' + attemptedQuestions.Count(t => (t.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Any(r => r.Status == (int)StatusEnum.Active && r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value)) || t.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single).ToString() + ' ');
                                                }
                                                else
                                                {
                                                    quizAnswerSubmit.ResultScore.ResultScoreValueTxt = string.Empty;
                                                    quizAnswerSubmit.ResultScore.ShowScoreValue = false;
                                                }

                                                if (resultSetting.ShowCorrectAnswer.HasValue && resultSetting.ShowCorrectAnswer.Value)
                                                {
                                                    quizAnswerSubmit.ResultScore.AnswerKeyCustomTxt = resultSetting.CustomTxtForAnswerKey;
                                                    quizAnswerSubmit.ResultScore.YourAnswerCustomTxt = resultSetting.CustomTxtForYourAnswer;
                                                    quizAnswerSubmit.ResultScore.CorrectAnswerCustomTxt = resultSetting.CustomTxtForCorrectAnswer;
                                                    quizAnswerSubmit.ResultScore.ExplanationCustomTxt = resultSetting.CustomTxtForExplanation;
                                                    quizAnswerSubmit.ResultScore.ShowCorrectAnswer = true;

                                                    quizAnswerSubmit.ResultScore.AttemptedQuestionAnswerDetails = new List<QuizAnswerSubmit.AnswerResult>();

                                                    foreach (var item in attemptedQuestions.Where(r => r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Short || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Long))
                                                    {
                                                        var correctAnswerTxt = string.Empty;
                                                        bool? IsCorrectValue = null;
                                                        int associatedScore = default(int);
                                                        var yourAnswer = string.Empty;

                                                        if (item.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Any())
                                                        {
                                                            if (quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                                                associatedScore = item.QuizAnswerStats.Sum(x => x.AnswerOptionsInQuizQuestions.AssociatedScore).Value;
                                                            else
                                                                IsCorrectValue = (item.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Select(s => s.Id).OrderBy(s => s)).SequenceEqual(item.QuizAnswerStats.Select(r => r.AnswerId).OrderBy(s => s));
                                                            correctAnswerTxt = VariableLinking(string.Join(",", item.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Select(t => t.Option).Select(s => "'" + s + "'")), quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt);
                                                        }
                                                        else if (item.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single)
                                                        {
                                                            correctAnswerTxt = VariableLinking(item.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(t => t.IsCorrectAnswer.HasValue ? t.IsCorrectAnswer.Value : false).Select(t => t.Option).FirstOrDefault(), quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt);
                                                            if (quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                                                associatedScore = item.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.AssociatedScore.Value;
                                                            else
                                                                IsCorrectValue = item.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer.HasValue && item.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer.Value;
                                                        }

                                                        if (item.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple || item.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single)
                                                            yourAnswer = VariableLinking(string.Join(",", item.QuizAnswerStats.Where(t => !t.AnswerOptionsInQuizQuestions.IsUnansweredType).Select(t => t.AnswerOptionsInQuizQuestions.Option).Select(s => "'" + s + "'")), quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt);
                                                        else
                                                            yourAnswer = item.QuizAnswerStats.Select(t => t.AnswerText).FirstOrDefault();

                                                        quizAnswerSubmit.ResultScore.AttemptedQuestionAnswerDetails.Add(new QuizAnswerSubmit.AnswerResult
                                                        {
                                                            Question = VariableLinking(item.QuestionsInQuiz.Question, quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt),
                                                            YourAnswer = string.IsNullOrEmpty(yourAnswer) ? notAttemptedQuesText : yourAnswer,
                                                            CorrectAnswer = (quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate) ? string.Empty : correctAnswerTxt,
                                                            IsCorrect = IsCorrectValue,
                                                            AnswerExplanation = item.QuestionsInQuiz.CorrectAnswerDescription,
                                                            AssociatedScore = (quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate) ? associatedScore : default(int),
                                                        });
                                                    }
                                                }
                                                else
                                                {
                                                    quizAnswerSubmit.ResultScore.ShowCorrectAnswer = false;
                                                }
                                            }

                                            quizAnswerSubmit.ResultScore.ShowInternalTitle = result.ShowInternalTitle;
                                            quizAnswerSubmit.ResultScore.ShowExternalTitle = result.ShowExternalTitle;
                                            quizAnswerSubmit.ResultScore.ShowDescription = result.ShowDescription;
                                            quizAnswerSubmit.ResultScore.Title = VariableLinking(result.Title, quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt);
                                            quizAnswerSubmit.ResultScore.InternalTitle = VariableLinking(result.InternalTitle, quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt);
                                            quizAnswerSubmit.ResultScore.Description = VariableLinking(result.Description, quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt);
                                            quizAnswerSubmit.ResultScore.AutoPlay = result.AutoPlay;
                                            quizAnswerSubmit.ResultScore.SecondsToApply = result.SecondsToApply;
                                            quizAnswerSubmit.ResultScore.VideoFrameEnabled = result.VideoFrameEnabled;
                                            quizAnswerSubmit.ResultScore.DisplayOrderForTitle = result.DisplayOrderForTitle;
                                            quizAnswerSubmit.ResultScore.DisplayOrderForTitleImage = result.DisplayOrderForTitleImage;
                                            quizAnswerSubmit.ResultScore.DisplayOrderForDescription = result.DisplayOrderForDescription;
                                            quizAnswerSubmit.ResultScore.DisplayOrderForNextButton = result.DisplayOrderForNextButton;

                                            quizStatsObj = quizAttemptsObj.QuizStats.FirstOrDefault();

                                            if (quizStatsObj != null)
                                            {
                                                quizStatsObj.ResultId = result.Id;

                                                UOWObj.QuizStatsRepository.Update(quizStatsObj);
                                                UOWObj.Save();
                                            }
                                            #endregion
                                        }
                                        else
                                        {
                                            #region Personality result Setting
                                            var personalityAnswerResultList = attemptedQuestions.SelectMany(x => x.QuizAnswerStats.SelectMany(r => r.AnswerOptionsInQuizQuestions.PersonalityAnswerResultMapping));
                                            var personalitySetting = quizDetails.PersonalityResultSetting.FirstOrDefault();
                                            if (personalitySetting != null && personalitySetting.Status == (int)StatusEnum.Active)
                                            {
                                                #region personality Setting Active
                                                var personalityResultList = new List<QuizAnswerSubmit.PersonalityResult>();
                                                var countResult = 0;
                                                quizStatsObj = quizAttemptsObj.QuizStats.FirstOrDefault();

                                                if (personalityAnswerResultList.Any())
                                                {
                                                    #region correlation is available
                                                    var mappedResultList = personalityAnswerResultList.OrderBy(x => x.QuizResults.DisplayOrder).GroupBy(r => r.ResultId).OrderByDescending(i => i.Count()).Take(personalitySetting.MaxResult).Select(x => x.Key).ToList();

                                                    var quizresultList = quizDetails.QuizResults.Where(r => !r.IsPersonalityCorrelatedResult && mappedResultList.Contains(r.Id)).OrderBy(x => mappedResultList.IndexOf(x.Id));

                                                    foreach (var quizresult in quizresultList)
                                                    {
                                                        QuizAnswerSubmit.PersonalityResult personalityresult = new QuizAnswerSubmit.PersonalityResult();
                                                        personalityresult.Title = VariableLinking(quizresult.Title, quizDetails, quizAttemptsObj, false, false, null);
                                                        personalityresult.InternalTitle = VariableLinking(quizresult.InternalTitle, quizDetails, quizAttemptsObj, false, false, null);
                                                        personalityresult.Image = quizresult.Image;
                                                        personalityresult.ResultId = quizresult.Id;
                                                        personalityresult.GraphColor = personalitySetting.GraphColor;
                                                        personalityresult.ButtonColor = personalitySetting.ButtonColor;
                                                        personalityresult.ButtonFontColor = personalitySetting.ButtonFontColor;
                                                        personalityresult.MaxResult = personalitySetting.MaxResult;
                                                        personalityresult.SideButtonText = personalitySetting.SideButtonText;
                                                        personalityresult.IsFullWidthEnable = personalitySetting.IsFullWidthEnable;
                                                        personalityresult.Description = VariableLinking(quizresult.Description, quizDetails, quizAttemptsObj, false, false, null);
                                                        int count = personalityAnswerResultList.Where(k => k.ResultId == quizresult.Id).Count();
                                                        personalityresult.Percentage = (int)Math.Round((double)(100 * count) / personalityAnswerResultList.Count());
                                                        personalityresult.ShowInternalTitle = quizresult.ShowInternalTitle;
                                                        personalityresult.ShowExternalTitle = quizresult.ShowExternalTitle;
                                                        personalityresult.ShowDescription = quizresult.ShowDescription;
                                                        personalityResultList.Add(personalityresult);

                                                        if (quizStatsObj != null)
                                                        {
                                                            if (countResult == 0)
                                                            {
                                                                quizStatsObj.ResultId = quizresult.Id;
                                                                UOWObj.QuizStatsRepository.Update(quizStatsObj);
                                                            }
                                                            else
                                                            {
                                                                var resultQuizStats = new Db.QuizStats();
                                                                resultQuizStats.StartedOn = quizStatsObj.StartedOn;
                                                                resultQuizStats.QuizAttemptId = quizStatsObj.QuizAttemptId;
                                                                resultQuizStats.ResultId = quizresult.Id;
                                                                UOWObj.QuizStatsRepository.Insert(resultQuizStats);
                                                            }
                                                            UOWObj.Save();
                                                            countResult++;
                                                        }
                                                    }

                                                    quizAnswerSubmit.ShowLeadUserForm = (Mode == "PREVIEW" || Mode == "PREVIEWTEMPLATE") ? false : (resultIdList.Any() ? (resultIdList.Any(r => quizresultList.Any(l => l.Id == r) || quizComponentLogsList.Any(q => q.DraftedObjectId == r && quizresultList.Any(l => l.Id == q.PublishedObjectId)))) : (quizAttemptsObj.LeadUserId == null && quizAttemptsObj.RecruiterUserId == null && ((quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Personality && quizDetails.PersonalityResultSetting.FirstOrDefault().Status == (int)StatusEnum.Active) ? quizDetails.PersonalityResultSetting.FirstOrDefault().ShowLeadUserForm : quizresultList.Any(r => r.ShowLeadUserForm))));

                                                    if (leadFormDetailofResultsLst != null && leadFormDetailofResultsLst.Any() && quizAnswerSubmit.ShowLeadUserForm)
                                                    {
                                                        var personalityResult = personalityResultList.OrderByDescending(r => r.Percentage.Value).FirstOrDefault();
                                                        if (leadFormDetailofResultsLst.Any(r => r.ResultId == personalityResult.ResultId || quizComponentLogsList.Any(q => q.DraftedObjectId == r.ResultId && q.PublishedObjectId == personalityResult.ResultId)))
                                                        {
                                                            var leadFormDetailofResultsObj = leadFormDetailofResultsLst.FirstOrDefault(r => r.ResultId == personalityResult.ResultId || quizComponentLogsList.Any(q => q.DraftedObjectId == r.ResultId && q.PublishedObjectId == personalityResult.ResultId));
                                                            quizAnswerSubmit.FormId = leadFormDetailofResultsObj.FormId;
                                                            quizAnswerSubmit.FlowOrder = leadFormDetailofResultsObj.FlowOrder;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        quizAnswerSubmit.FormId = null;
                                                        quizAnswerSubmit.FlowOrder = null;
                                                    }

                                                    #endregion
                                                }
                                                else
                                                {
                                                    #region correlation is not available

                                                    var quizresultList = quizDetails.QuizResults.OrderBy(x => x.DisplayOrder).Take(personalitySetting.MaxResult);

                                                    foreach (var quizresult in quizresultList)
                                                    {
                                                        QuizAnswerSubmit.PersonalityResult personalityresult = new QuizAnswerSubmit.PersonalityResult();
                                                        personalityresult.Title = VariableLinking(quizresult.Title, quizDetails, quizAttemptsObj, false, false, null);
                                                        personalityresult.InternalTitle = VariableLinking(quizresult.InternalTitle, quizDetails, quizAttemptsObj, false, false, null);
                                                        personalityresult.Image = quizresult.Image;
                                                        personalityresult.ResultId = quizresult.Id;
                                                        personalityresult.GraphColor = personalitySetting.GraphColor;
                                                        personalityresult.ButtonColor = personalitySetting.ButtonColor;
                                                        personalityresult.ButtonFontColor = personalitySetting.ButtonFontColor;
                                                        personalityresult.MaxResult = personalitySetting.MaxResult;
                                                        personalityresult.SideButtonText = personalitySetting.SideButtonText;
                                                        personalityresult.IsFullWidthEnable = personalitySetting.IsFullWidthEnable;
                                                        personalityresult.Description = VariableLinking(quizresult.Description, quizDetails, quizAttemptsObj, false, false, null);
                                                        personalityresult.Percentage = null;
                                                        personalityresult.ShowInternalTitle = quizresult.ShowInternalTitle;
                                                        personalityresult.ShowExternalTitle = quizresult.ShowExternalTitle;
                                                        personalityresult.ShowDescription = quizresult.ShowDescription;
                                                        personalityResultList.Add(personalityresult);

                                                        if (quizStatsObj != null)
                                                        {
                                                            if (countResult == 0)
                                                            {
                                                                quizStatsObj.ResultId = quizresult.Id;
                                                                UOWObj.QuizStatsRepository.Update(quizStatsObj);
                                                            }
                                                            else
                                                            {
                                                                var resultQuizStats = new Db.QuizStats();
                                                                resultQuizStats.StartedOn = quizStatsObj.StartedOn;
                                                                resultQuizStats.QuizAttemptId = quizStatsObj.QuizAttemptId;
                                                                resultQuizStats.ResultId = quizresult.Id;
                                                                UOWObj.QuizStatsRepository.Insert(resultQuizStats);
                                                            }
                                                            UOWObj.Save();
                                                            countResult++;
                                                        }
                                                    }

                                                    quizAnswerSubmit.ShowLeadUserForm = (Mode == "PREVIEW" || Mode == "PREVIEWTEMPLATE") ? false : (resultIdList.Any() ? (resultIdList.Any(r => quizresultList.Any(l => l.Id == r) || quizComponentLogsList.Any(q => q.DraftedObjectId == r && quizresultList.Any(l => l.Id == q.PublishedObjectId)))) : (quizAttemptsObj.LeadUserId == null && quizAttemptsObj.RecruiterUserId == null && ((quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Personality && quizDetails.PersonalityResultSetting.FirstOrDefault().Status == (int)StatusEnum.Active) ? quizDetails.PersonalityResultSetting.FirstOrDefault().ShowLeadUserForm : quizresultList.Any(r => r.ShowLeadUserForm))));

                                                    if (leadFormDetailofResultsLst != null && leadFormDetailofResultsLst.Any() && quizAnswerSubmit.ShowLeadUserForm)
                                                    {
                                                        var personalityResult = personalityResultList.FirstOrDefault();
                                                        if (leadFormDetailofResultsLst.Any(r => r.ResultId == personalityResult.ResultId || quizComponentLogsList.Any(q => q.DraftedObjectId == r.ResultId && q.PublishedObjectId == personalityResult.ResultId)))
                                                        {
                                                            var leadFormDetailofResultsObj = leadFormDetailofResultsLst.FirstOrDefault(r => r.ResultId == personalityResult.ResultId || quizComponentLogsList.Any(q => q.DraftedObjectId == r.ResultId && q.PublishedObjectId == personalityResult.ResultId));
                                                            quizAnswerSubmit.FormId = leadFormDetailofResultsObj.FormId;
                                                            quizAnswerSubmit.FlowOrder = leadFormDetailofResultsObj.FlowOrder;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        quizAnswerSubmit.FormId = null;
                                                        quizAnswerSubmit.FlowOrder = null;
                                                    }

                                                    #endregion
                                                }
                                                quizAnswerSubmit.ResultScore = quizAnswerSubmit.ResultScore ?? new QuizAnswerSubmit.Result();
                                                quizAnswerSubmit.ResultScore.ShowInternalTitle = true;
                                                quizAnswerSubmit.ResultScore.ShowExternalTitle = true;
                                                quizAnswerSubmit.ResultScore.ShowDescription = true;
                                                quizAnswerSubmit.ResultScore.Title = personalitySetting.Title;
                                                quizAnswerSubmit.ResultScore.PersonalityResultList = personalityResultList.OrderByDescending(x => x.Percentage).ToList();
                                                #endregion
                                            }
                                            else
                                            {
                                                #region if there is correlation available but personalitySetting is disabled

                                                if (result.IsPersonalityCorrelatedResult && personalityAnswerResultList.Any())
                                                    result = quizDetails.QuizResults.FirstOrDefault(x => x.Id == personalityAnswerResultList.OrderBy(r => r.QuizResults.DisplayOrder).GroupBy(r => r.ResultId).OrderByDescending(i => i.Count()).First().Key);

                                                quizAnswerSubmit.ShowLeadUserForm = (Mode == "PREVIEW" || Mode == "PREVIEWTEMPLATE") ? false : (resultIdList.Any() ? (resultIdList.Any(r => r == result.Id || quizComponentLogsList.Any(q => q.DraftedObjectId == r && q.PublishedObjectId == result.Id))) : (quizAttemptsObj.LeadUserId == null && quizAttemptsObj.RecruiterUserId == null && ((quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Personality && quizDetails.PersonalityResultSetting.FirstOrDefault().Status == (int)StatusEnum.Active) ? quizDetails.PersonalityResultSetting.FirstOrDefault().ShowLeadUserForm : result.ShowLeadUserForm)));
                                                quizAnswerSubmit.CompanyCode = quizDetails.Quiz.Company.ClientCode;

                                                if (leadFormDetailofResultsLst != null && leadFormDetailofResultsLst.Any() && quizAnswerSubmit.ShowLeadUserForm)
                                                {
                                                    var leadFormDetailofResultsObj = leadFormDetailofResultsLst.FirstOrDefault(r => r.ResultId == result.Id || quizComponentLogsList.Any(q => q.DraftedObjectId == r.ResultId && q.PublishedObjectId == result.Id));
                                                    quizAnswerSubmit.FormId = leadFormDetailofResultsObj.FormId;
                                                    quizAnswerSubmit.FlowOrder = leadFormDetailofResultsObj.FlowOrder;
                                                }

                                                quizAnswerSubmit.ResultScore = new QuizAnswerSubmit.Result();

                                                if (result.EnableMediaFile && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.RESULT && r.ObjectId == result.Id))
                                                {
                                                    var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.RESULT && r.ObjectId == result.Id);
                                                    quizAnswerSubmit.ResultScore.Image = result.ShowResultImage ? mediaObj.ObjectValue : string.Empty;
                                                    quizAnswerSubmit.ResultScore.PublicIdForResult = result.ShowResultImage ? mediaObj.ObjectPublicId : string.Empty;

                                                    var newMedia = result.ShowResultImage ? ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers) : null;
                                                    if (newMedia != null)
                                                    {
                                                        quizAnswerSubmit.ResultScore.Image = newMedia.MediaUrl;
                                                        quizAnswerSubmit.ResultScore.PublicIdForResult = newMedia.MediaPublicId;
                                                    }
                                                }
                                                else
                                                {
                                                    quizAnswerSubmit.ResultScore.Image = result.ShowResultImage ? result.Image : string.Empty;
                                                    quizAnswerSubmit.ResultScore.PublicIdForResult = result.ShowResultImage ? result.PublicId : string.Empty;
                                                }
                                                quizAnswerSubmit.ResultScore.HideActionButton = result.HideCallToAction.HasValue ? result.HideCallToAction.Value : false;
                                                quizAnswerSubmit.ResultScore.ActionButtonURL = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonURL;
                                                quizAnswerSubmit.ResultScore.OpenLinkInNewTab = quizAnswerSubmit.ResultScore.HideActionButton ? false : result.OpenLinkInNewTab;
                                                quizAnswerSubmit.ResultScore.ActionButtonTxtSize = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonTxtSize;
                                                quizAnswerSubmit.ResultScore.ActionButtonColor = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonColor;
                                                quizAnswerSubmit.ResultScore.ActionButtonTitleColor = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonTitleColor;
                                                quizAnswerSubmit.ResultScore.ActionButtonText = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonText;

                                                var resultSetting = quizDetails.ResultSettings.FirstOrDefault();

                                                if (resultSetting != null)
                                                {
                                                    quizAnswerSubmit.ResultScore.ResultScoreValueTxt = string.Empty;
                                                    quizAnswerSubmit.ResultScore.ShowScoreValue = false;
                                                    if (resultSetting.ShowCorrectAnswer.HasValue && resultSetting.ShowCorrectAnswer.Value)
                                                    {
                                                        quizAnswerSubmit.ResultScore.AnswerKeyCustomTxt = resultSetting.CustomTxtForAnswerKey;
                                                        quizAnswerSubmit.ResultScore.YourAnswerCustomTxt = resultSetting.CustomTxtForYourAnswer;
                                                        quizAnswerSubmit.ResultScore.CorrectAnswerCustomTxt = resultSetting.CustomTxtForCorrectAnswer;
                                                        quizAnswerSubmit.ResultScore.ExplanationCustomTxt = resultSetting.CustomTxtForExplanation;
                                                        quizAnswerSubmit.ResultScore.ShowCorrectAnswer = true;
                                                        quizAnswerSubmit.ResultScore.AttemptedQuestionAnswerDetails = new List<QuizAnswerSubmit.AnswerResult>();

                                                        foreach (var item in attemptedQuestions.Where(r => r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single))
                                                        {
                                                            quizAnswerSubmit.ResultScore.AttemptedQuestionAnswerDetails.Add(new QuizAnswerSubmit.AnswerResult
                                                            {
                                                                Question = VariableLinking(item.QuestionsInQuiz.Question, quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt),
                                                                YourAnswer = VariableLinking(string.Join(",", item.QuizAnswerStats.Where(t => !t.AnswerOptionsInQuizQuestions.IsUnansweredType).Select(t => t.AnswerOptionsInQuizQuestions.Option).Select(s => "'" + s + "'")), quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt),
                                                                CorrectAnswer = string.Empty,
                                                                IsCorrect = null,
                                                                AnswerExplanation = item.QuestionsInQuiz.CorrectAnswerDescription,
                                                                AssociatedScore = default(int)
                                                            });
                                                        }
                                                    }
                                                    else
                                                    {
                                                        quizAnswerSubmit.ResultScore.ShowCorrectAnswer = false;
                                                    }
                                                }

                                                quizAnswerSubmit.ResultScore.ShowInternalTitle = result.ShowInternalTitle;
                                                quizAnswerSubmit.ResultScore.ShowExternalTitle = result.ShowExternalTitle;
                                                quizAnswerSubmit.ResultScore.ShowDescription = result.ShowDescription;
                                                quizAnswerSubmit.ResultScore.Title = VariableLinking(result.Title, quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt);
                                                quizAnswerSubmit.ResultScore.InternalTitle = VariableLinking(result.InternalTitle, quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt);
                                                quizAnswerSubmit.ResultScore.Description = VariableLinking(result.Description, quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt);
                                                quizAnswerSubmit.ResultScore.AutoPlay = result.AutoPlay;
                                                quizAnswerSubmit.ResultScore.SecondsToApply = result.SecondsToApply;
                                                quizAnswerSubmit.ResultScore.VideoFrameEnabled = result.VideoFrameEnabled;
                                                quizAnswerSubmit.ResultScore.DisplayOrderForTitle = result.DisplayOrderForTitle;
                                                quizAnswerSubmit.ResultScore.DisplayOrderForTitleImage = result.DisplayOrderForTitleImage;
                                                quizAnswerSubmit.ResultScore.DisplayOrderForDescription = result.DisplayOrderForDescription;
                                                quizAnswerSubmit.ResultScore.DisplayOrderForNextButton = result.DisplayOrderForNextButton;

                                                quizStatsObj = quizAttemptsObj.QuizStats.FirstOrDefault();

                                                if (quizStatsObj != null)
                                                {
                                                    quizStatsObj.ResultId = result.Id;

                                                    UOWObj.QuizStatsRepository.Update(quizStatsObj);
                                                    UOWObj.Save();
                                                }
                                                #endregion
                                            }
                                            #endregion
                                        }
                                        var nextObj = FetchNextQuestion(quizDetails, result.Id, (int)BranchingLogicEnum.RESULTNEXT, UOWObj, isQuesAndContentInSameTable);

                                        #region if next is Badge type

                                        if (nextObj != null && nextObj.GetType().BaseType.Name == "BadgesInQuiz")
                                        {
                                            quizAnswerSubmit.BadgeDetails = new QuizBadge();

                                            quizAnswerSubmit.BadgeDetails.Id = ((Db.BadgesInQuiz)nextObj).Id;
                                            quizAnswerSubmit.BadgeDetails.Title = VariableLinking(((Db.BadgesInQuiz)nextObj).Title, quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt);
                                            quizAnswerSubmit.BadgeDetails.ShowTitle = ((Db.BadgesInQuiz)nextObj).ShowTitle;
                                            quizAnswerSubmit.BadgeDetails.AutoPlay = ((Db.BadgesInQuiz)nextObj).AutoPlay;
                                            quizAnswerSubmit.BadgeDetails.SecondsToApply = ((Db.BadgesInQuiz)nextObj).SecondsToApply;
                                            quizAnswerSubmit.BadgeDetails.VideoFrameEnabled = ((Db.BadgesInQuiz)nextObj).VideoFrameEnabled;
                                            quizAnswerSubmit.BadgeDetails.DisplayOrderForTitleImage = ((Db.BadgesInQuiz)nextObj).DisplayOrderForTitleImage;
                                            quizAnswerSubmit.BadgeDetails.DisplayOrderForTitle = ((Db.BadgesInQuiz)nextObj).DisplayOrderForTitle;
                                            quizAnswerSubmit.BadgeDetails.ShowImage = ((Db.BadgesInQuiz)nextObj).ShowImage;

                                            if (((Db.BadgesInQuiz)nextObj).EnableMediaFile && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.BADGE && r.ObjectId == ((Db.BadgesInQuiz)nextObj).Id))
                                            {
                                                var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.BADGE && r.ObjectId == ((Db.BadgesInQuiz)nextObj).Id);
                                                quizAnswerSubmit.BadgeDetails.Image = mediaObj.ObjectValue;
                                                quizAnswerSubmit.BadgeDetails.PublicIdForBadge = mediaObj.ObjectPublicId ?? string.Empty;

                                                var newMedia = ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers);
                                                if (newMedia != null)
                                                {
                                                    quizAnswerSubmit.BadgeDetails.Image = newMedia.MediaUrl;
                                                    quizAnswerSubmit.BadgeDetails.PublicIdForBadge = newMedia.MediaPublicId;
                                                }
                                            }
                                            else
                                            {
                                                quizAnswerSubmit.BadgeDetails.Image = ((Db.BadgesInQuiz)nextObj).Image ?? string.Empty;
                                                quizAnswerSubmit.BadgeDetails.PublicIdForBadge = ((Db.BadgesInQuiz)nextObj).PublicId ?? string.Empty;
                                            }

                                            var BadgeStatsObj = new Db.QuizObjectStats();

                                            BadgeStatsObj.QuizAttemptId = quizAttemptsObj.Id;
                                            BadgeStatsObj.ObjectId = ((Db.BadgesInQuiz)nextObj).Id;
                                            BadgeStatsObj.ViewedOn = currentDate;
                                            BadgeStatsObj.TypeId = (int)BranchingLogicEnum.BADGE;
                                            BadgeStatsObj.Status = (int)StatusEnum.Active;

                                            UOWObj.QuizObjectStatsRepository.Insert(BadgeStatsObj);
                                            UOWObj.Save();

                                            if (quizAttemptsObj.RecruiterUserId.HasValue && quizDetails.Quiz.Company.BadgesEnabled)
                                            {
                                                var badgesInfo = badgesInfoUpdateJson.Replace("{UserId}", quizAttemptsObj.RecruiterUserId.Value.ToString()).Replace("{CourseId}", quizDetails.ParentQuizId.ToString()).Replace("{CourseBadgeName}", ((Db.BadgesInQuiz)nextObj).Title).Replace("{CourseBadgeImageUrl}", quizAnswerSubmit.BadgeDetails.Image).Replace("{CourseTitle}", quizDetails.QuizTitle);
                                                var user = UOWObj.UserTokensRepository.Get(r => r.BusinessUserId == quizAttemptsObj.RecruiterUserId.Value).FirstOrDefault();
                                                var company = quizAttemptsObj.CompanyId > 0 ? quizAttemptsObj.Company : user.Company;
                                                try
                                                {
                                                    var apiSuccess = OWCHelper.UpdateRecruiterCourseBadgesInfo(badgesInfo, company);
                                                    if (!apiSuccess)
                                                        AddPendingApi(company.JobRocketApiUrl + (GlobalSettings.owcAddRecruiterCourseUrl.ToString()).Replace("{ClientCode}", company.ClientCode), company.JobRocketApiAuthorizationBearer, badgesInfo, "POST");
                                                }
                                                catch (Exception ex)
                                                {
                                                    AddPendingApi(company.JobRocketApiUrl + (GlobalSettings.owcAddRecruiterCourseUrl.ToString()).Replace("{ClientCode}", company.ClientCode), company.JobRocketApiAuthorizationBearer, badgesInfo, "POST");
                                                }
                                            }
                                        }
                                        #endregion
                                    }
                                    #endregion

                                    #region if next is content type

                                    if (nextQuestionObj.GetType().BaseType.Name == "ContentsInQuiz")
                                    {
                                        var content = quizDetails.ContentsInQuiz.FirstOrDefault(r => r.Id == QuestionId);

                                        quizAnswerSubmit.QuestionType = (int)BranchingLogicEnum.CONTENT;

                                        quizAnswerSubmit.IsBackButtonEnable = content.ViewPreviousQuestion;

                                        if (quizAttemptsObj.QuizObjectStats.Any(a => a.Status == (int)StatusEnum.Active && a.TypeId == (int)BranchingLogicEnum.CONTENT && a.ObjectId == ((Db.ContentsInQuiz)nextQuestionObj).Id))
                                        {
                                            QuestionId = ((Db.ContentsInQuiz)nextQuestionObj).Id;
                                            Type = "complete_content";
                                            goto switchcase;
                                        }

                                        quizAnswerSubmit.ContentDetails = new QuizContent();

                                        quizAnswerSubmit.ContentDetails.Id = ((Db.ContentsInQuiz)nextQuestionObj).Id;
                                        quizAnswerSubmit.ContentDetails.ContentTitle = VariableLinking(((Db.ContentsInQuiz)nextQuestionObj).ContentTitle, quizDetails, quizAttemptsObj, false, false, null);
                                        quizAnswerSubmit.ContentDetails.ContentDescription = VariableLinking(((Db.ContentsInQuiz)nextQuestionObj).ContentDescription, quizDetails, quizAttemptsObj, false, false, null);
                                        quizAnswerSubmit.ContentDetails.ShowDescription = ((Db.ContentsInQuiz)nextQuestionObj).ShowDescription;

                                        if (((Db.ContentsInQuiz)nextQuestionObj).EnableMediaFileForTitle && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Title && r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == ((Db.ContentsInQuiz)nextQuestionObj).Id))
                                        {
                                            var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Title && r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == ((Db.ContentsInQuiz)nextQuestionObj).Id);
                                            quizAnswerSubmit.ContentDetails.ContentTitleImage = mediaObj.ObjectValue ?? string.Empty;
                                            quizAnswerSubmit.ContentDetails.PublicIdForContentTitle = mediaObj.ObjectPublicId ?? string.Empty;

                                            var newMedia = ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers);
                                            if (newMedia != null)
                                            {
                                                quizAnswerSubmit.ContentDetails.ContentTitleImage = newMedia.MediaUrl;
                                                quizAnswerSubmit.ContentDetails.PublicIdForContentTitle = newMedia.MediaPublicId;
                                            }
                                        }
                                        else
                                        {
                                            quizAnswerSubmit.ContentDetails.ContentTitleImage = ((Db.ContentsInQuiz)nextQuestionObj).ContentTitleImage ?? string.Empty;
                                            quizAnswerSubmit.ContentDetails.PublicIdForContentTitle = ((Db.ContentsInQuiz)nextQuestionObj).PublicIdForContentTitle ?? string.Empty;
                                        }

                                        quizAnswerSubmit.ContentDetails.ShowContentTitleImage = ((Db.ContentsInQuiz)nextQuestionObj).ShowContentTitleImage.HasValue && ((Db.ContentsInQuiz)nextQuestionObj).ShowContentTitleImage.Value ? ((Db.ContentsInQuiz)nextQuestionObj).ShowContentTitleImage.Value : false;
                                        quizAnswerSubmit.ContentDetails.AliasTextForNextButton = ((Db.ContentsInQuiz)nextQuestionObj).AliasTextForNextButton;
                                        quizAnswerSubmit.ContentDetails.EnableNextButton = ((Db.ContentsInQuiz)nextQuestionObj).EnableNextButton;

                                        if (((Db.ContentsInQuiz)nextQuestionObj).EnableMediaFileForDescription && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Description && r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == ((Db.ContentsInQuiz)nextQuestionObj).Id))
                                        {
                                            var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Description && r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == ((Db.ContentsInQuiz)nextQuestionObj).Id);
                                            quizAnswerSubmit.ContentDetails.ContentDescriptionImage = mediaObj.ObjectValue ?? string.Empty;
                                            quizAnswerSubmit.ContentDetails.PublicIdForContentDescription = mediaObj.ObjectPublicId ?? string.Empty;

                                            var newMedia = ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers);
                                            if (newMedia != null)
                                            {
                                                quizAnswerSubmit.ContentDetails.ContentDescriptionImage = newMedia.MediaUrl;
                                                quizAnswerSubmit.ContentDetails.PublicIdForContentDescription = newMedia.MediaPublicId;
                                            }
                                        }
                                        else
                                        {
                                            quizAnswerSubmit.ContentDetails.ContentDescriptionImage = ((Db.ContentsInQuiz)nextQuestionObj).ContentDescriptionImage ?? string.Empty;
                                            quizAnswerSubmit.ContentDetails.PublicIdForContentDescription = ((Db.ContentsInQuiz)nextQuestionObj).PublicIdForContentDescription ?? string.Empty;
                                        }

                                        //quizAnswerSubmit.ContentDetails.PublicIdForContentDescription = ((Db.ContentsInQuiz)nextQuestionObj).PublicIdForContentDescription ?? string.Empty;
                                        quizAnswerSubmit.ContentDetails.ShowContentDescriptionImage = ((Db.ContentsInQuiz)nextQuestionObj).ShowContentDescriptionImage.HasValue && ((Db.ContentsInQuiz)nextQuestionObj).ShowContentDescriptionImage.Value ? ((Db.ContentsInQuiz)nextQuestionObj).ShowContentDescriptionImage.Value : false;
                                        quizAnswerSubmit.ContentDetails.ViewPreviousQuestion = ((Db.ContentsInQuiz)nextQuestionObj).ViewPreviousQuestion;
                                        quizAnswerSubmit.ContentDetails.AutoPlay = ((Db.ContentsInQuiz)nextQuestionObj).AutoPlay;
                                        quizAnswerSubmit.ContentDetails.SecondsToApply = ((Db.ContentsInQuiz)nextQuestionObj).SecondsToApply ?? "0";
                                        quizAnswerSubmit.ContentDetails.VideoFrameEnabled = ((Db.ContentsInQuiz)nextQuestionObj).VideoFrameEnabled ?? false;
                                        quizAnswerSubmit.ContentDetails.AutoPlayForDescription = ((Db.ContentsInQuiz)nextQuestionObj).AutoPlayForDescription;
                                        quizAnswerSubmit.ContentDetails.SecondsToApplyForDescription = ((Db.ContentsInQuiz)nextQuestionObj).SecondsToApplyForDescription ?? "0";
                                        quizAnswerSubmit.ContentDetails.DescVideoFrameEnabled = ((Db.ContentsInQuiz)nextQuestionObj).DescVideoFrameEnabled ?? false;
                                        quizAnswerSubmit.ContentDetails.ShowTitle = ((Db.ContentsInQuiz)nextQuestionObj).ShowTitle;
                                        quizAnswerSubmit.ContentDetails.DisplayOrderForTitle = ((Db.ContentsInQuiz)nextQuestionObj).DisplayOrderForTitle;
                                        quizAnswerSubmit.ContentDetails.DisplayOrderForTitleImage = ((Db.ContentsInQuiz)nextQuestionObj).DisplayOrderForTitleImage;
                                        quizAnswerSubmit.ContentDetails.DisplayOrderForDescription = ((Db.ContentsInQuiz)nextQuestionObj).DisplayOrderForDescription;
                                        quizAnswerSubmit.ContentDetails.DisplayOrderForDescriptionImage = ((Db.ContentsInQuiz)nextQuestionObj).DisplayOrderForDescriptionImage;
                                        quizAnswerSubmit.ContentDetails.DisplayOrderForNextButton = ((Db.ContentsInQuiz)nextQuestionObj).DisplayOrderForNextButton;
                                    }
                                    #endregion

                                    #region if next is Badge type

                                    if (nextQuestionObj.GetType().BaseType.Name == "BadgesInQuiz")
                                    {
                                        quizAnswerSubmit.QuestionType = (int)BranchingLogicEnum.BADGE;

                                        quizAnswerSubmit.IsBackButtonEnable = false;

                                        var BadgeStatsObj = new Db.QuizObjectStats();

                                        BadgeStatsObj.QuizAttemptId = quizAttemptsObj.Id;
                                        BadgeStatsObj.ObjectId = ((Db.BadgesInQuiz)nextQuestionObj).Id;
                                        BadgeStatsObj.ViewedOn = currentDate;
                                        BadgeStatsObj.TypeId = (int)BranchingLogicEnum.BADGE;
                                        BadgeStatsObj.Status = (int)StatusEnum.Active;

                                        UOWObj.QuizObjectStatsRepository.Insert(BadgeStatsObj);
                                        UOWObj.Save();
                                        quizAnswerSubmit.BadgeDetails = new QuizBadge();

                                        quizAnswerSubmit.BadgeDetails.Id = ((Db.BadgesInQuiz)nextQuestionObj).Id;
                                        quizAnswerSubmit.BadgeDetails.Title = VariableLinking(((Db.BadgesInQuiz)nextQuestionObj).Title, quizDetails, quizAttemptsObj, false, false, null);
                                        quizAnswerSubmit.BadgeDetails.ShowTitle = ((Db.BadgesInQuiz)nextQuestionObj).ShowTitle;
                                        quizAnswerSubmit.BadgeDetails.AutoPlay = ((Db.BadgesInQuiz)nextQuestionObj).AutoPlay;
                                        quizAnswerSubmit.BadgeDetails.SecondsToApply = ((Db.BadgesInQuiz)nextQuestionObj).SecondsToApply;
                                        quizAnswerSubmit.BadgeDetails.VideoFrameEnabled = ((Db.BadgesInQuiz)nextQuestionObj).VideoFrameEnabled;
                                        quizAnswerSubmit.BadgeDetails.DisplayOrderForTitleImage = ((Db.BadgesInQuiz)nextQuestionObj).DisplayOrderForTitleImage;
                                        quizAnswerSubmit.BadgeDetails.DisplayOrderForTitle = ((Db.BadgesInQuiz)nextQuestionObj).DisplayOrderForTitle;
                                        quizAnswerSubmit.BadgeDetails.ShowImage = ((Db.BadgesInQuiz)nextQuestionObj).ShowImage;

                                        if (((Db.BadgesInQuiz)nextQuestionObj).EnableMediaFile && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.BADGE && r.ObjectId == ((Db.BadgesInQuiz)nextQuestionObj).Id))
                                        {
                                            var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.BADGE && r.ObjectId == ((Db.BadgesInQuiz)nextQuestionObj).Id);
                                            quizAnswerSubmit.BadgeDetails.Image = mediaObj.ObjectValue;
                                            quizAnswerSubmit.BadgeDetails.PublicIdForBadge = mediaObj.ObjectPublicId ?? string.Empty;

                                            var newMedia = ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers);
                                            if (newMedia != null)
                                            {
                                                quizAnswerSubmit.BadgeDetails.Image = newMedia.MediaUrl;
                                                quizAnswerSubmit.BadgeDetails.PublicIdForBadge = newMedia.MediaPublicId;
                                            }
                                        }
                                        else
                                        {
                                            quizAnswerSubmit.BadgeDetails.Image = ((Db.BadgesInQuiz)nextQuestionObj).Image ?? string.Empty;
                                            quizAnswerSubmit.BadgeDetails.PublicIdForBadge = ((Db.BadgesInQuiz)nextQuestionObj).PublicId ?? string.Empty;
                                        }

                                        if (quizAttemptsObj.RecruiterUserId.HasValue && quizDetails.Quiz.Company.BadgesEnabled)
                                        {
                                            var badgesInfo = badgesInfoUpdateJson.Replace("{UserId}", quizAttemptsObj.RecruiterUserId.Value.ToString()).Replace("{CourseId}", quizDetails.ParentQuizId.ToString()).Replace("{CourseBadgeName}", ((Db.BadgesInQuiz)nextQuestionObj).Title).Replace("{CourseBadgeImageUrl}", quizAnswerSubmit.BadgeDetails.Image).Replace("{CourseTitle}", quizDetails.QuizTitle);
                                            var user = UOWObj.UserTokensRepository.Get(r => r.BusinessUserId == quizAttemptsObj.RecruiterUserId.Value).FirstOrDefault();
                                            var company = quizAttemptsObj.CompanyId > 0 ? quizAttemptsObj.Company : user.Company;
                                            try
                                            {
                                                var apiSuccess = OWCHelper.UpdateRecruiterCourseBadgesInfo(badgesInfo, company);
                                                if (!apiSuccess)
                                                    AddPendingApi(company.JobRocketApiUrl + (GlobalSettings.owcAddRecruiterCourseUrl.ToString()).Replace("{ClientCode}", company.ClientCode), company.JobRocketApiAuthorizationBearer, badgesInfo, "POST");
                                            }
                                            catch (Exception ex)
                                            {
                                                AddPendingApi(company.JobRocketApiUrl + (GlobalSettings.owcAddRecruiterCourseUrl.ToString()).Replace("{ClientCode}", company.ClientCode), company.JobRocketApiAuthorizationBearer, badgesInfo, "POST");
                                            }
                                        }

                                    }
                                    #endregion
                                }
                                else
                                {
                                    #region show last result

                                    quizAnswerSubmit.IsBackButtonEnable = false;
                                    quizAnswerSubmit.CompanyCode = quizDetails.Quiz.Company.ClientCode;
                                    var IsResultInquizBranching = false;
                                    if (quizDetails.IsBranchingLogicEnabled.HasValue && quizDetails.IsBranchingLogicEnabled.Value)
                                        IsResultInquizBranching = quizDetails.BranchingLogic.Where(r => r.DestinationTypeId == (int)BranchingLogicEnum.RESULT).Any() ? true : false;

                                    quizAnswerSubmit.ResultScore = new QuizAnswerSubmit.Result();

                                    var resultSetting = quizDetails.ResultSettings.FirstOrDefault();

                                    if (resultSetting != null && !IsResultInquizBranching)
                                    {
                                        #region result Setting is not null                                           
                                        var attemptedQuestions = quizAttemptsObj.QuizQuestionStats.Where(t => t.Status == (int)StatusEnum.Active && t.CompletedOn != null && (t.QuestionsInQuiz.TimerRequired ? true : t.QuizAnswerStats.Any(r => !r.AnswerOptionsInQuizQuestions.IsUnansweredType)));

                                        if (quizDetails.Quiz.QuizType != (int)QuizTypeEnum.Personality && quizDetails.Quiz.QuizType != (int)QuizTypeEnum.PersonalityTemplate)
                                        {
                                            #region non personality result Setting

                                            string scoreValueTxt = string.Empty;
                                            float correctAnsCount = 0;
                                            if (quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                            {
                                                scoreValueTxt = resultSetting.CustomTxtForScoreValueInResult ?? "You scored a%score%";
                                                correctAnsCount = attemptedQuestions.Sum(a => a.QuizAnswerStats.Sum(x => x.AnswerOptionsInQuizQuestions.AssociatedScore)).Value;
                                            }
                                            else if (quizDetails.Quiz.QuizType == (int)QuizTypeEnum.NPS)
                                            {
                                                scoreValueTxt = resultSetting.CustomTxtForScoreValueInResult ?? string.Empty;
                                                var attemptedQuestionsofNPSType = attemptedQuestions.Where(a => a.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.NPS);
                                                correctAnsCount = Convert.ToInt32(attemptedQuestionsofNPSType.Sum(a => a.QuizAnswerStats.Sum(x => Convert.ToInt32(x.AnswerText))) / attemptedQuestionsofNPSType.Count());
                                            }
                                            else
                                            {
                                                scoreValueTxt = resultSetting.CustomTxtForScoreValueInResult ?? "YOU GOT%score%OUT OF%total%CORRECT";
                                                correctAnsCount = attemptedQuestions.Count(a => (a.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && ((a.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Select(s => s.Id).OrderBy(s => s)).SequenceEqual(a.QuizAnswerStats.Select(r => r.AnswerId).OrderBy(s => s)))) || (a.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single && a.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer == true));
                                            }

                                            scoreValueTxt = string.IsNullOrEmpty(scoreValueTxt) ? string.Empty : scoreValueTxt.Replace("%score%", ' ' + correctAnsCount.ToString() + ' ').Replace("%total%", ' ' + attemptedQuestions.Count(t => (t.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Any(r => r.Status == (int)StatusEnum.Active && r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value)) || t.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single).ToString() + ' ');

                                            var result = quizDetails.QuizResults.Where(r => r.Status == (int)StatusEnum.Active && r.MinScore <= correctAnsCount && correctAnsCount <= r.MaxScore).FirstOrDefault();
                                            quizAnswerSubmit.ShowLeadUserForm = (Mode == "PREVIEW" || Mode == "PREVIEWTEMPLATE") ? false : (resultIdList.Any() ? (resultIdList.Any(r => r == result.Id || quizComponentLogsList.Any(q => q.DraftedObjectId == r && q.PublishedObjectId == result.Id))) : (quizAttemptsObj.LeadUserId == null && quizAttemptsObj.RecruiterUserId == null && ((quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Personality && quizDetails.PersonalityResultSetting.FirstOrDefault().Status == (int)StatusEnum.Active) ? quizDetails.PersonalityResultSetting.FirstOrDefault().ShowLeadUserForm : result.ShowLeadUserForm)));

                                            if (leadFormDetailofResultsLst != null && leadFormDetailofResultsLst.Any() && quizAnswerSubmit.ShowLeadUserForm)
                                            {
                                                var leadFormDetailofResultsObj = leadFormDetailofResultsLst.FirstOrDefault(r => r.ResultId == result.Id || quizComponentLogsList.Any(q => q.DraftedObjectId == r.ResultId && q.PublishedObjectId == result.Id));
                                                quizAnswerSubmit.FormId = leadFormDetailofResultsObj.FormId;
                                                quizAnswerSubmit.FlowOrder = leadFormDetailofResultsObj.FlowOrder;
                                            }

                                            quizAnswerSubmit.ResultScore.ShowInternalTitle = result.ShowInternalTitle;
                                            quizAnswerSubmit.ResultScore.ShowExternalTitle = result.ShowExternalTitle;
                                            quizAnswerSubmit.ResultScore.ShowDescription = result.ShowDescription;
                                            quizAnswerSubmit.ResultScore.Title = VariableLinking(result.Title, quizDetails, quizAttemptsObj, false, (resultSetting.ShowScoreValue.HasValue && resultSetting.ShowScoreValue.Value), scoreValueTxt);
                                            quizAnswerSubmit.ResultScore.InternalTitle = VariableLinking(result.InternalTitle, quizDetails, quizAttemptsObj, false, (resultSetting.ShowScoreValue.HasValue && resultSetting.ShowScoreValue.Value), scoreValueTxt);
                                            if (result.EnableMediaFile && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.RESULT && r.ObjectId == result.Id))
                                            {
                                                var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.RESULT && r.ObjectId == result.Id);
                                                quizAnswerSubmit.ResultScore.Image = result.ShowResultImage ? mediaObj.ObjectValue : string.Empty;
                                                quizAnswerSubmit.ResultScore.PublicIdForResult = result.ShowResultImage ? mediaObj.ObjectPublicId : string.Empty;

                                                var newMedia = result.ShowResultImage ? ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers) : null;
                                                if (newMedia != null)
                                                {
                                                    quizAnswerSubmit.ResultScore.Image = newMedia.MediaUrl;
                                                    quizAnswerSubmit.ResultScore.PublicIdForResult = newMedia.MediaPublicId;
                                                }
                                            }
                                            else
                                            {
                                                quizAnswerSubmit.ResultScore.Image = result.ShowResultImage ? result.Image : string.Empty;
                                                quizAnswerSubmit.ResultScore.PublicIdForResult = result.ShowResultImage ? result.PublicId : string.Empty;
                                            }
                                            quizAnswerSubmit.ResultScore.Description = VariableLinking(result.Description, quizDetails, quizAttemptsObj, false, (resultSetting.ShowScoreValue.HasValue && resultSetting.ShowScoreValue.Value), scoreValueTxt);
                                            quizAnswerSubmit.ResultScore.HideActionButton = result.HideCallToAction.HasValue ? result.HideCallToAction.Value : false;
                                            quizAnswerSubmit.ResultScore.ActionButtonURL = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonURL;
                                            quizAnswerSubmit.ResultScore.OpenLinkInNewTab = quizAnswerSubmit.ResultScore.HideActionButton ? false : result.OpenLinkInNewTab;
                                            quizAnswerSubmit.ResultScore.ActionButtonTxtSize = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonTxtSize;
                                            quizAnswerSubmit.ResultScore.ActionButtonColor = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonColor;
                                            quizAnswerSubmit.ResultScore.ActionButtonTitleColor = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonTitleColor;
                                            quizAnswerSubmit.ResultScore.ActionButtonText = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonText;
                                            quizAnswerSubmit.ResultScore.AutoPlay = result.AutoPlay;
                                            quizAnswerSubmit.ResultScore.SecondsToApply = result.SecondsToApply ?? "0";
                                            quizAnswerSubmit.ResultScore.VideoFrameEnabled = result.VideoFrameEnabled ?? false;
                                            quizAnswerSubmit.ResultScore.DisplayOrderForTitle = result.DisplayOrderForTitle;
                                            quizAnswerSubmit.ResultScore.DisplayOrderForTitleImage = result.DisplayOrderForTitleImage;
                                            quizAnswerSubmit.ResultScore.DisplayOrderForDescription = result.DisplayOrderForDescription;
                                            quizAnswerSubmit.ResultScore.DisplayOrderForNextButton = result.DisplayOrderForNextButton;

                                            if (quizStatsObj != null)
                                            {
                                                quizStatsObj.ResultId = result.Id;
                                                UOWObj.QuizStatsRepository.Update(quizStatsObj);
                                                UOWObj.Save();
                                            }
                                            if (resultSetting.ShowScoreValue.HasValue && resultSetting.ShowScoreValue.Value && (((quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate) && attemptedQuestions.Any(r => r.QuestionsInQuiz.Status == (int)StatusEnum.Active && (r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple))) || attemptedQuestions.Any(r => (r.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Any(t => t.Status == (int)StatusEnum.Active && t.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && t.IsCorrectForMultipleAnswer.HasValue && t.IsCorrectForMultipleAnswer.Value)) || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single)))
                                            {
                                                quizAnswerSubmit.ResultScore.ResultScoreValueTxt = scoreValueTxt;
                                                quizAnswerSubmit.ResultScore.ShowScoreValue = true;
                                            }
                                            else
                                            {
                                                quizAnswerSubmit.ResultScore.ResultScoreValueTxt = string.Empty;
                                                quizAnswerSubmit.ResultScore.ShowScoreValue = false;
                                            }
                                            quizAnswerSubmit.ResultScore.HideSocialShareButtons = quizDetails.HideSocialShareButtons.HasValue && quizDetails.HideSocialShareButtons.Value ? quizDetails.HideSocialShareButtons.Value : false;
                                            quizAnswerSubmit.ResultScore.ShowFacebookBtn = !quizAnswerSubmit.ResultScore.HideSocialShareButtons && quizDetails.EnableFacebookShare.HasValue ? quizDetails.EnableFacebookShare.Value : false;
                                            quizAnswerSubmit.ResultScore.ShowTwitterBtn = !quizAnswerSubmit.ResultScore.HideSocialShareButtons && quizDetails.EnableTwitterShare.HasValue ? quizDetails.EnableTwitterShare.Value : false;
                                            quizAnswerSubmit.ResultScore.ShowLinkedinBtn = !quizAnswerSubmit.ResultScore.HideSocialShareButtons && quizDetails.EnableLinkedinShare.HasValue ? quizDetails.EnableLinkedinShare.Value : false;

                                            if (resultSetting.ShowCorrectAnswer.HasValue && resultSetting.ShowCorrectAnswer.Value)
                                            {
                                                quizAnswerSubmit.ResultScore.AnswerKeyCustomTxt = resultSetting.CustomTxtForAnswerKey;
                                                quizAnswerSubmit.ResultScore.YourAnswerCustomTxt = resultSetting.CustomTxtForYourAnswer;
                                                quizAnswerSubmit.ResultScore.CorrectAnswerCustomTxt = resultSetting.CustomTxtForCorrectAnswer;
                                                quizAnswerSubmit.ResultScore.ExplanationCustomTxt = resultSetting.CustomTxtForExplanation;
                                                quizAnswerSubmit.ResultScore.ShowCorrectAnswer = true;

                                                quizAnswerSubmit.ResultScore.AttemptedQuestionAnswerDetails = new List<QuizAnswerSubmit.AnswerResult>();

                                                #region attempted question

                                                foreach (var item in attemptedQuestions.Where(r => r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Short || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Long))
                                                {
                                                    var correctAnswerTxt = string.Empty;
                                                    bool? IsCorrectValue = null;
                                                    int associatedScore = default(int);
                                                    var yourAnswer = string.Empty;

                                                    if (item.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Any())
                                                    {
                                                        if (quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                                            associatedScore = item.QuizAnswerStats.Sum(x => x.AnswerOptionsInQuizQuestions.AssociatedScore).Value;
                                                        else
                                                            IsCorrectValue = (item.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Select(s => s.Id).OrderBy(s => s)).SequenceEqual(item.QuizAnswerStats.Select(r => r.AnswerId).OrderBy(s => s));
                                                        correctAnswerTxt = VariableLinking(string.Join(",", item.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Select(t => t.Option).Select(s => "'" + s + "'")), quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt);
                                                    }
                                                    else if (item.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single)
                                                    {
                                                        correctAnswerTxt = VariableLinking(item.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(t => t.IsCorrectAnswer.HasValue ? t.IsCorrectAnswer.Value : false).Select(t => t.Option).FirstOrDefault(), quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt);
                                                        if (quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                                            associatedScore = item.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.AssociatedScore.Value;
                                                        else
                                                            IsCorrectValue = item.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer.HasValue && item.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer.Value;
                                                    }

                                                    if (item.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple || item.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single)
                                                        yourAnswer = VariableLinking(string.Join(",", item.QuizAnswerStats.Where(t => !t.AnswerOptionsInQuizQuestions.IsUnansweredType).Select(t => t.AnswerOptionsInQuizQuestions.Option).Select(s => "'" + s + "'")), quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt);
                                                    else
                                                        yourAnswer = item.QuizAnswerStats.Select(t => t.AnswerText).FirstOrDefault();

                                                    quizAnswerSubmit.ResultScore.AttemptedQuestionAnswerDetails.Add(new QuizAnswerSubmit.AnswerResult
                                                    {
                                                        Question = VariableLinking(item.QuestionsInQuiz.Question, quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, scoreValueTxt),
                                                        YourAnswer = string.IsNullOrEmpty(yourAnswer) ? notAttemptedQuesText : yourAnswer,
                                                        CorrectAnswer = (quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate) ? string.Empty : correctAnswerTxt,
                                                        IsCorrect = IsCorrectValue,
                                                        AnswerExplanation = item.QuestionsInQuiz.CorrectAnswerDescription,
                                                        AssociatedScore = (quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate) ? associatedScore : default(int),
                                                    });
                                                }

                                                #endregion
                                            }
                                            else
                                            {
                                                quizAnswerSubmit.ResultScore.ShowCorrectAnswer = false;
                                            }
                                            #endregion
                                        }
                                        else
                                        {
                                            #region Personality result Setting
                                            var personalityAnswerResultList = attemptedQuestions.SelectMany(x => x.QuizAnswerStats.SelectMany(r => r.AnswerOptionsInQuizQuestions.PersonalityAnswerResultMapping));
                                            var personalitySetting = quizDetails.PersonalityResultSetting.FirstOrDefault();
                                            if (personalitySetting != null && personalitySetting.Status == (int)StatusEnum.Active)
                                            {
                                                #region personality Setting Active
                                                var personalityResultList = new List<QuizAnswerSubmit.PersonalityResult>();
                                                var countResult = 0;
                                                quizStatsObj = quizAttemptsObj.QuizStats.FirstOrDefault();

                                                if (personalityAnswerResultList.Any())
                                                {
                                                    #region correlation is available
                                                    var mappedResultList = personalityAnswerResultList.OrderBy(x => x.QuizResults.DisplayOrder).GroupBy(r => r.ResultId).OrderByDescending(i => i.Count()).Take(personalitySetting.MaxResult).Select(x => x.Key).ToList();
                                                    var quizresultList = quizDetails.QuizResults.Where(r => !r.IsPersonalityCorrelatedResult && mappedResultList.Contains(r.Id)).OrderBy(x => mappedResultList.IndexOf(x.Id));
                                                    foreach (var quizresult in quizresultList)
                                                    {
                                                        QuizAnswerSubmit.PersonalityResult personalityresult = new QuizAnswerSubmit.PersonalityResult();
                                                        personalityresult.Title = VariableLinking(quizresult.Title, quizDetails, quizAttemptsObj, false, false, null);
                                                        personalityresult.InternalTitle = VariableLinking(quizresult.InternalTitle, quizDetails, quizAttemptsObj, false, false, null);
                                                        personalityresult.Image = quizresult.Image;
                                                        personalityresult.ResultId = quizresult.Id;
                                                        personalityresult.GraphColor = personalitySetting.GraphColor;
                                                        personalityresult.ButtonColor = personalitySetting.ButtonColor;
                                                        personalityresult.ButtonFontColor = personalitySetting.ButtonFontColor;
                                                        personalityresult.MaxResult = personalitySetting.MaxResult;
                                                        personalityresult.SideButtonText = personalitySetting.SideButtonText;
                                                        personalityresult.IsFullWidthEnable = personalitySetting.IsFullWidthEnable;
                                                        personalityresult.Description = VariableLinking(quizresult.Description, quizDetails, quizAttemptsObj, false, false, null);
                                                        int count = personalityAnswerResultList.Where(k => k.ResultId == quizresult.Id).Count();
                                                        personalityresult.Percentage = (int)Math.Round((double)(100 * count) / personalityAnswerResultList.Count());
                                                        personalityresult.ShowInternalTitle = quizresult.ShowInternalTitle;
                                                        personalityresult.ShowExternalTitle = quizresult.ShowExternalTitle;
                                                        personalityresult.ShowDescription = quizresult.ShowDescription;
                                                        personalityResultList.Add(personalityresult);

                                                        if (quizStatsObj != null)
                                                        {
                                                            if (countResult == 0)
                                                            {
                                                                quizStatsObj.ResultId = quizresult.Id;
                                                                UOWObj.QuizStatsRepository.Update(quizStatsObj);
                                                            }
                                                            else
                                                            {
                                                                var resultQuizStats = new Db.QuizStats();
                                                                resultQuizStats.StartedOn = quizStatsObj.StartedOn;
                                                                resultQuizStats.QuizAttemptId = quizStatsObj.QuizAttemptId;
                                                                resultQuizStats.ResultId = quizresult.Id;
                                                                UOWObj.QuizStatsRepository.Insert(resultQuizStats);
                                                            }
                                                            UOWObj.Save();
                                                            countResult++;
                                                        }
                                                    }

                                                    quizAnswerSubmit.ShowLeadUserForm = (Mode == "PREVIEW" || Mode == "PREVIEWTEMPLATE") ? false : (resultIdList.Any() ? (resultIdList.Any(r => quizresultList.Any(l => l.Id == r) || quizComponentLogsList.Any(q => q.DraftedObjectId == r && quizresultList.Any(l => l.Id == q.PublishedObjectId)))) : (quizAttemptsObj.LeadUserId == null && quizAttemptsObj.RecruiterUserId == null && ((quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Personality && quizDetails.PersonalityResultSetting.FirstOrDefault().Status == (int)StatusEnum.Active) ? quizDetails.PersonalityResultSetting.FirstOrDefault().ShowLeadUserForm : quizresultList.Any(r => r.ShowLeadUserForm))));

                                                    if (leadFormDetailofResultsLst != null && leadFormDetailofResultsLst.Any() && quizAnswerSubmit.ShowLeadUserForm)
                                                    {
                                                        var personalityResult = personalityResultList.OrderByDescending(r => r.Percentage.Value).FirstOrDefault();
                                                        if (leadFormDetailofResultsLst.Any(r => r.ResultId == personalityResult.ResultId || quizComponentLogsList.Any(q => q.DraftedObjectId == r.ResultId && q.PublishedObjectId == personalityResult.ResultId)))
                                                        {
                                                            var leadFormDetailofResultsObj = leadFormDetailofResultsLst.FirstOrDefault(r => r.ResultId == personalityResult.ResultId || quizComponentLogsList.Any(q => q.DraftedObjectId == r.ResultId && q.PublishedObjectId == personalityResult.ResultId));
                                                            quizAnswerSubmit.FormId = leadFormDetailofResultsObj.FormId;
                                                            quizAnswerSubmit.FlowOrder = leadFormDetailofResultsObj.FlowOrder;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        quizAnswerSubmit.FormId = null;
                                                        quizAnswerSubmit.FlowOrder = null;
                                                    }

                                                    #endregion
                                                }
                                                else
                                                {
                                                    #region correlation is not available
                                                    var quizresultList = quizDetails.QuizResults.OrderBy(x => x.DisplayOrder).Take(personalitySetting.MaxResult);
                                                    foreach (var quizresult in quizresultList)
                                                    {
                                                        QuizAnswerSubmit.PersonalityResult personalityresult = new QuizAnswerSubmit.PersonalityResult();
                                                        personalityresult.Title = VariableLinking(quizresult.Title, quizDetails, quizAttemptsObj, false, false, null);
                                                        personalityresult.InternalTitle = VariableLinking(quizresult.InternalTitle, quizDetails, quizAttemptsObj, false, false, null);
                                                        personalityresult.Image = quizresult.Image;
                                                        personalityresult.ResultId = quizresult.Id;
                                                        personalityresult.GraphColor = personalitySetting.GraphColor;
                                                        personalityresult.ButtonColor = personalitySetting.ButtonColor;
                                                        personalityresult.ButtonFontColor = personalitySetting.ButtonFontColor;
                                                        personalityresult.MaxResult = personalitySetting.MaxResult;
                                                        personalityresult.SideButtonText = personalitySetting.SideButtonText;
                                                        personalityresult.IsFullWidthEnable = personalitySetting.IsFullWidthEnable;
                                                        personalityresult.Description = VariableLinking(quizresult.Description, quizDetails, quizAttemptsObj, false, false, null);
                                                        personalityresult.Percentage = null;
                                                        personalityresult.ShowInternalTitle = quizresult.ShowInternalTitle;
                                                        personalityresult.ShowExternalTitle = quizresult.ShowExternalTitle;
                                                        personalityresult.ShowDescription = quizresult.ShowDescription;
                                                        personalityResultList.Add(personalityresult);

                                                        if (quizStatsObj != null)
                                                        {
                                                            if (countResult == 0)
                                                            {
                                                                quizStatsObj.ResultId = quizresult.Id;
                                                                UOWObj.QuizStatsRepository.Update(quizStatsObj);
                                                            }
                                                            else
                                                            {
                                                                var resultQuizStats = new Db.QuizStats();
                                                                resultQuizStats.StartedOn = quizStatsObj.StartedOn;
                                                                resultQuizStats.QuizAttemptId = quizStatsObj.QuizAttemptId;
                                                                resultQuizStats.ResultId = quizresult.Id;
                                                                UOWObj.QuizStatsRepository.Insert(resultQuizStats);
                                                            }
                                                            UOWObj.Save();
                                                            countResult++;
                                                        }
                                                    }

                                                    quizAnswerSubmit.ShowLeadUserForm = (Mode == "PREVIEW" || Mode == "PREVIEWTEMPLATE") ? false : (resultIdList.Any() ? (resultIdList.Any(r => quizresultList.Any(l => l.Id == r) || quizComponentLogsList.Any(q => q.DraftedObjectId == r && quizresultList.Any(l => l.Id == q.PublishedObjectId)))) : (quizAttemptsObj.LeadUserId == null && quizAttemptsObj.RecruiterUserId == null && ((quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Personality && quizDetails.PersonalityResultSetting.FirstOrDefault().Status == (int)StatusEnum.Active) ? quizDetails.PersonalityResultSetting.FirstOrDefault().ShowLeadUserForm : quizresultList.Any(r => r.ShowLeadUserForm))));

                                                    if (leadFormDetailofResultsLst != null && leadFormDetailofResultsLst.Any() && quizAnswerSubmit.ShowLeadUserForm)
                                                    {
                                                        var personalityResult = personalityResultList.FirstOrDefault();
                                                        if (leadFormDetailofResultsLst.Any(r => r.ResultId == personalityResult.ResultId || quizComponentLogsList.Any(q => q.DraftedObjectId == r.ResultId && q.PublishedObjectId == personalityResult.ResultId)))
                                                        {
                                                            var leadFormDetailofResultsObj = leadFormDetailofResultsLst.FirstOrDefault(r => r.ResultId == personalityResult.ResultId || quizComponentLogsList.Any(q => q.DraftedObjectId == r.ResultId && q.PublishedObjectId == personalityResult.ResultId));
                                                            quizAnswerSubmit.FormId = leadFormDetailofResultsObj.FormId;
                                                            quizAnswerSubmit.FlowOrder = leadFormDetailofResultsObj.FlowOrder;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        quizAnswerSubmit.FormId = null;
                                                        quizAnswerSubmit.FlowOrder = null;
                                                    }

                                                    #endregion
                                                }
                                                quizAnswerSubmit.ResultScore.ShowInternalTitle = true;
                                                quizAnswerSubmit.ResultScore.ShowExternalTitle = true;
                                                quizAnswerSubmit.ResultScore.ShowDescription = true;
                                                quizAnswerSubmit.ResultScore.Title = personalitySetting.Title;
                                                quizAnswerSubmit.ResultScore.PersonalityResultList = personalityResultList.OrderByDescending(x => x.Percentage).ToList();
                                                #endregion
                                            }
                                            else
                                            {
                                                #region If there is correlation available but personalitySetting is disabled

                                                var result = new Db.QuizResults();
                                                if (personalityAnswerResultList.Any())
                                                    result = quizDetails.QuizResults.FirstOrDefault(x => x.Id == personalityAnswerResultList.OrderBy(r => r.QuizResults.DisplayOrder).GroupBy(r => r.ResultId).OrderByDescending(i => i.Count()).First().Key);
                                                else
                                                    result = quizDetails.QuizResults.FirstOrDefault(x => x.DisplayOrder == 1);

                                                quizAnswerSubmit.ShowLeadUserForm = (Mode == "PREVIEW" || Mode == "PREVIEWTEMPLATE") ? false : (resultIdList.Any() ? (resultIdList.Any(r => r == result.Id || quizComponentLogsList.Any(q => q.DraftedObjectId == r && q.PublishedObjectId == result.Id))) : (quizAttemptsObj.LeadUserId == null && quizAttemptsObj.RecruiterUserId == null && ((quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Personality && quizDetails.PersonalityResultSetting.FirstOrDefault().Status == (int)StatusEnum.Active) ? quizDetails.PersonalityResultSetting.FirstOrDefault().ShowLeadUserForm : result.ShowLeadUserForm)));

                                                if (leadFormDetailofResultsLst != null && leadFormDetailofResultsLst.Any() && quizAnswerSubmit.ShowLeadUserForm)
                                                {
                                                    var leadFormDetailofResultsObj = leadFormDetailofResultsLst.FirstOrDefault(r => r.ResultId == result.Id || quizComponentLogsList.Any(q => q.DraftedObjectId == r.ResultId && q.PublishedObjectId == result.Id));
                                                    quizAnswerSubmit.FormId = leadFormDetailofResultsObj.FormId;
                                                    quizAnswerSubmit.FlowOrder = leadFormDetailofResultsObj.FlowOrder;
                                                }

                                                quizAnswerSubmit.ResultScore.ShowInternalTitle = result.ShowInternalTitle;
                                                quizAnswerSubmit.ResultScore.ShowExternalTitle = result.ShowExternalTitle;
                                                quizAnswerSubmit.ResultScore.ShowDescription = result.ShowDescription;
                                                quizAnswerSubmit.ResultScore.Title = VariableLinking(result.Title, quizDetails, quizAttemptsObj, false, (resultSetting.ShowScoreValue.HasValue && resultSetting.ShowScoreValue.Value), string.Empty);
                                                quizAnswerSubmit.ResultScore.InternalTitle = VariableLinking(result.InternalTitle, quizDetails, quizAttemptsObj, false, (resultSetting.ShowScoreValue.HasValue && resultSetting.ShowScoreValue.Value), string.Empty);
                                                if (result.EnableMediaFile && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.RESULT && r.ObjectId == result.Id))
                                                {
                                                    var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.RESULT && r.ObjectId == result.Id);
                                                    quizAnswerSubmit.ResultScore.Image = result.ShowResultImage ? mediaObj.ObjectValue : string.Empty;
                                                    quizAnswerSubmit.ResultScore.PublicIdForResult = result.ShowResultImage ? mediaObj.ObjectPublicId : string.Empty;

                                                    var newMedia = result.ShowResultImage ? ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers) : null;
                                                    if (newMedia != null)
                                                    {
                                                        quizAnswerSubmit.ResultScore.Image = newMedia.MediaUrl;
                                                        quizAnswerSubmit.ResultScore.PublicIdForResult = newMedia.MediaPublicId;
                                                    }
                                                }
                                                else
                                                {
                                                    quizAnswerSubmit.ResultScore.Image = result.ShowResultImage ? result.Image : string.Empty;
                                                    quizAnswerSubmit.ResultScore.PublicIdForResult = result.ShowResultImage ? result.PublicId : string.Empty;
                                                }
                                                quizAnswerSubmit.ResultScore.Description = VariableLinking(result.Description, quizDetails, quizAttemptsObj, false, (resultSetting.ShowScoreValue.HasValue && resultSetting.ShowScoreValue.Value), string.Empty);
                                                quizAnswerSubmit.ResultScore.HideActionButton = result.HideCallToAction.HasValue ? result.HideCallToAction.Value : false;
                                                quizAnswerSubmit.ResultScore.ActionButtonURL = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonURL;
                                                quizAnswerSubmit.ResultScore.OpenLinkInNewTab = quizAnswerSubmit.ResultScore.HideActionButton ? false : result.OpenLinkInNewTab;
                                                quizAnswerSubmit.ResultScore.ActionButtonTxtSize = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonTxtSize;
                                                quizAnswerSubmit.ResultScore.ActionButtonColor = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonColor;
                                                quizAnswerSubmit.ResultScore.ActionButtonTitleColor = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonTitleColor;
                                                quizAnswerSubmit.ResultScore.ActionButtonText = quizAnswerSubmit.ResultScore.HideActionButton ? string.Empty : result.ActionButtonText;
                                                quizAnswerSubmit.ResultScore.AutoPlay = result.AutoPlay;
                                                quizAnswerSubmit.ResultScore.SecondsToApply = result.SecondsToApply ?? "0";
                                                quizAnswerSubmit.ResultScore.VideoFrameEnabled = result.VideoFrameEnabled ?? false;
                                                quizAnswerSubmit.ResultScore.DisplayOrderForTitle = result.DisplayOrderForTitle;
                                                quizAnswerSubmit.ResultScore.DisplayOrderForTitleImage = result.DisplayOrderForTitleImage;
                                                quizAnswerSubmit.ResultScore.DisplayOrderForDescription = result.DisplayOrderForDescription;
                                                quizAnswerSubmit.ResultScore.DisplayOrderForNextButton = result.DisplayOrderForNextButton;

                                                if (quizStatsObj != null)
                                                {
                                                    quizStatsObj.ResultId = result.Id;
                                                    UOWObj.QuizStatsRepository.Update(quizStatsObj);
                                                    UOWObj.Save();
                                                }

                                                quizAnswerSubmit.ResultScore.ResultScoreValueTxt = string.Empty;
                                                quizAnswerSubmit.ResultScore.ShowScoreValue = false;
                                                quizAnswerSubmit.ResultScore.HideSocialShareButtons = quizDetails.HideSocialShareButtons.HasValue && quizDetails.HideSocialShareButtons.Value ? quizDetails.HideSocialShareButtons.Value : false;
                                                quizAnswerSubmit.ResultScore.ShowFacebookBtn = !quizAnswerSubmit.ResultScore.HideSocialShareButtons && quizDetails.EnableFacebookShare.HasValue ? quizDetails.EnableFacebookShare.Value : false;
                                                quizAnswerSubmit.ResultScore.ShowTwitterBtn = !quizAnswerSubmit.ResultScore.HideSocialShareButtons && quizDetails.EnableTwitterShare.HasValue ? quizDetails.EnableTwitterShare.Value : false;
                                                quizAnswerSubmit.ResultScore.ShowLinkedinBtn = !quizAnswerSubmit.ResultScore.HideSocialShareButtons && quizDetails.EnableLinkedinShare.HasValue ? quizDetails.EnableLinkedinShare.Value : false;

                                                if (resultSetting.ShowCorrectAnswer.HasValue && resultSetting.ShowCorrectAnswer.Value)
                                                {
                                                    quizAnswerSubmit.ResultScore.AnswerKeyCustomTxt = resultSetting.CustomTxtForAnswerKey;
                                                    quizAnswerSubmit.ResultScore.YourAnswerCustomTxt = resultSetting.CustomTxtForYourAnswer;
                                                    quizAnswerSubmit.ResultScore.CorrectAnswerCustomTxt = resultSetting.CustomTxtForCorrectAnswer;
                                                    quizAnswerSubmit.ResultScore.ExplanationCustomTxt = resultSetting.CustomTxtForExplanation;
                                                    quizAnswerSubmit.ResultScore.ShowCorrectAnswer = true;
                                                    quizAnswerSubmit.ResultScore.AttemptedQuestionAnswerDetails = new List<QuizAnswerSubmit.AnswerResult>();

                                                    #region attempted question

                                                    foreach (var item in attemptedQuestions.Where(r => r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single))
                                                    {
                                                        quizAnswerSubmit.ResultScore.AttemptedQuestionAnswerDetails.Add(new QuizAnswerSubmit.AnswerResult
                                                        {
                                                            Question = VariableLinking(item.QuestionsInQuiz.Question, quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, string.Empty),
                                                            YourAnswer = (item.QuizAnswerStats != null && item.QuizAnswerStats.Any(r => !r.AnswerOptionsInQuizQuestions.IsUnansweredType)) ? VariableLinking(string.Join(",", item.QuizAnswerStats.Where(t => !t.AnswerOptionsInQuizQuestions.IsUnansweredType).Select(t => t.AnswerOptionsInQuizQuestions.Option).Select(s => "'" + s + "'")), quizDetails, quizAttemptsObj, false, quizAnswerSubmit.ResultScore.ShowScoreValue, string.Empty) : notAttemptedQuesText,
                                                            CorrectAnswer = string.Empty,
                                                            IsCorrect = null,
                                                            AnswerExplanation = item.QuestionsInQuiz.CorrectAnswerDescription,
                                                            AssociatedScore = default(int)
                                                        });
                                                    }

                                                    #endregion
                                                }
                                                else
                                                {

                                                    quizAnswerSubmit.ResultScore.ShowCorrectAnswer = false;
                                                }
                                                #endregion

                                            }
                                            #endregion
                                        }
                                        #endregion
                                    }
                                    else
                                    {
                                        #region resultSetting is null

                                        var attemptedQuestions = quizAttemptsObj.QuizQuestionStats.Where(t => t.Status == (int)StatusEnum.Active && t.CompletedOn != null && (t.QuestionsInQuiz.TimerRequired ? true : t.QuizAnswerStats.Any(r => !r.AnswerOptionsInQuizQuestions.IsUnansweredType)));

                                        string scoreValueTxt = string.Empty;
                                        var correctAnsCount = 0;
                                        var showScoreValue = false;

                                        if ((quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate) && attemptedQuestions.Any(r => r.QuestionsInQuiz.Status == (int)StatusEnum.Active && (r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple)))
                                        {
                                            scoreValueTxt = resultSetting.CustomTxtForScoreValueInResult ?? "You scored a%score%";
                                            correctAnsCount = attemptedQuestions.Sum(a => a.QuizAnswerStats.Sum(x => x.AnswerOptionsInQuizQuestions.AssociatedScore)).Value;
                                            showScoreValue = true;
                                        }
                                        else if ((quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Assessment || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.AssessmentTemplate) && attemptedQuestions.Any(r => (r.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Any(t => t.Status == (int)StatusEnum.Active && t.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && t.IsCorrectForMultipleAnswer.HasValue && t.IsCorrectForMultipleAnswer.Value)) || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single))
                                        {
                                            scoreValueTxt = resultSetting.CustomTxtForScoreValueInResult ?? "YOU GOT%score%OUT OF%total%CORRECT";
                                            correctAnsCount = attemptedQuestions.Count(a => (a.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && ((a.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Select(s => s.Id).OrderBy(s => s)).SequenceEqual(a.QuizAnswerStats.Select(r => r.AnswerId).OrderBy(s => s)))) || (a.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single && a.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer == true));
                                            showScoreValue = true;
                                        }

                                        scoreValueTxt = scoreValueTxt == null ? string.Empty : scoreValueTxt.Replace("%score%", ' ' + correctAnsCount.ToString() + ' ').Replace("%total%", ' ' + attemptedQuestions.Count(t => (t.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Any(r => r.Status == (int)StatusEnum.Active && r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value)) || t.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single).ToString() + ' ');

                                        quizAnswerSubmit.ShowLeadUserForm = false;
                                        quizAnswerSubmit.ResultScore.ResultScoreValueTxt = scoreValueTxt;
                                        quizAnswerSubmit.ResultScore.ShowScoreValue = showScoreValue;

                                        #endregion
                                    }
                                    #endregion
                                }
                                #endregion

                                break;
                        }

                        #region Updating ResponseJson for AttemptQuizLog 

                        if (enableAttemptQuizLogging && attemptQuizLogId != 0)
                        {
                            AttemptQuizLog.UpdateResponseJson(attemptQuizLogId, JsonConvert.SerializeObject(quizAnswerSubmit));

                        }
                        #endregion

                        if (Mode == "AUDIT")
                        {
                            DeleteQuizCache(quizObj.Id);
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz not found for the QuizCode " + QuizCode;
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;

                if (enableAttemptQuizLogging && attemptQuizLogId != 0)
                {
                    AttemptQuizLog.UpdateResponseJson(attemptQuizLogId, ErrorMessage);
                }


                throw ex;
            }

            return quizAnswerSubmit;

        }


        #region Old Unused Method
        private static void PreviousQuestionSubmittedAnswerMapping(QuizAnswerSubmit quizAnswerSubmit, object nextQuestionObj)
        {
            quizAnswerSubmit.PreviousQuestionSubmittedAnswer = new QuizAnswerSubmit.SubmittedAnswerResult();

            quizAnswerSubmit.PreviousQuestionSubmittedAnswer.AliasTextForCorrect = ((Db.QuestionsInQuiz)nextQuestionObj).AliasTextForCorrect;
            quizAnswerSubmit.PreviousQuestionSubmittedAnswer.AliasTextForIncorrect = ((Db.QuestionsInQuiz)nextQuestionObj).AliasTextForIncorrect;
            quizAnswerSubmit.PreviousQuestionSubmittedAnswer.AliasTextForYourAnswer = ((Db.QuestionsInQuiz)nextQuestionObj).AliasTextForYourAnswer;
            quizAnswerSubmit.PreviousQuestionSubmittedAnswer.AliasTextForCorrectAnswer = ((Db.QuestionsInQuiz)nextQuestionObj).AliasTextForCorrectAnswer;
            quizAnswerSubmit.PreviousQuestionSubmittedAnswer.AliasTextForExplanation = ((Db.QuestionsInQuiz)nextQuestionObj).AliasTextForExplanation;
            quizAnswerSubmit.PreviousQuestionSubmittedAnswer.AliasTextForNextButton = ((Db.QuestionsInQuiz)nextQuestionObj).AliasTextForNextButton;
            quizAnswerSubmit.PreviousQuestionSubmittedAnswer.CorrectAnswerDescription = ((Db.QuestionsInQuiz)nextQuestionObj).CorrectAnswerDescription;

            quizAnswerSubmit.PreviousQuestionSubmittedAnswer.SubmittedAnswerDetails = new List<QuizAnswerSubmit.SubmittedAnswerResult.SubmittedAnswer>();
        }

        private static void CheckQuizAnswerType(QuizAnswerSubmit quizAnswerSubmit, QuizAttempts quizAttemptsObj, Db.QuizDetails quizDetails, object nextQuestionObj, AnswerOptionsInQuizQuestions submittedAnswerOptionObj)
        {
            switch (submittedAnswerOptionObj.QuestionsInQuiz.AnswerType)
            {
                case (int)AnswerTypeEnum.Multiple:
                case (int)AnswerTypeEnum.Single:
                    quizAnswerSubmit.PreviousQuestionSubmittedAnswer.SubmittedAnswerDetails.Add(new QuizAnswerSubmit.SubmittedAnswerResult.SubmittedAnswer
                    {
                        AnswerId = submittedAnswerOptionObj.Id,
                        SubmittedAnswerTitle = VariableLinking(submittedAnswerOptionObj.Option, quizDetails, quizAttemptsObj, false, false, null),
                        SubmittedAnswerImage = ((QuestionsInQuiz)nextQuestionObj).ShowAnswerImage.HasValue && ((QuestionsInQuiz)nextQuestionObj).ShowAnswerImage.Value ? submittedAnswerOptionObj.OptionImage : string.Empty,
                        PublicIdForSubmittedAnswer = ((QuestionsInQuiz)nextQuestionObj).ShowAnswerImage.HasValue && ((QuestionsInQuiz)nextQuestionObj).ShowAnswerImage.Value ? submittedAnswerOptionObj.PublicId : string.Empty,
                        AutoPlay = submittedAnswerOptionObj.AutoPlay,
                        SecondsToApply = submittedAnswerOptionObj.SecondsToApply,
                        VideoFrameEnabled = submittedAnswerOptionObj.VideoFrameEnabled
                    });
                    break;
                case (int)AnswerTypeEnum.FullAddress:
                    {
                        if (submittedAnswerOptionObj.QuizAnswerStats.FirstOrDefault().SubAnswerTypeId == (int)SubAnswerTypeEnum.PostCode)
                        {
                            quizAnswerSubmit.PreviousQuestionSubmittedAnswer.SubmittedAnswerDetails.Add(new QuizAnswerSubmit.SubmittedAnswerResult.SubmittedAnswer
                            {
                                AnswerId = submittedAnswerOptionObj.Id,
                                SubmittedAnswerTitle = submittedAnswerOptionObj.QuizAnswerStats.FirstOrDefault().AnswerText,
                                SubmittedSecondaryAnswerTitle = submittedAnswerOptionObj.QuizAnswerStats.FirstOrDefault().AnswerSecondaryText,
                                SubmittedAnswerImage = ((QuestionsInQuiz)nextQuestionObj).ShowAnswerImage.HasValue && ((QuestionsInQuiz)nextQuestionObj).ShowAnswerImage.Value ? submittedAnswerOptionObj.OptionImage : string.Empty,
                                PublicIdForSubmittedAnswer = ((QuestionsInQuiz)nextQuestionObj).ShowAnswerImage.HasValue && ((QuestionsInQuiz)nextQuestionObj).ShowAnswerImage.Value ? submittedAnswerOptionObj.PublicId : string.Empty,
                                AutoPlay = submittedAnswerOptionObj.AutoPlay,
                                SecondsToApply = submittedAnswerOptionObj.SecondsToApply,
                                VideoFrameEnabled = submittedAnswerOptionObj.VideoFrameEnabled
                            });
                        }

                        if (submittedAnswerOptionObj.QuizAnswerStats.FirstOrDefault().SubAnswerTypeId == (int)SubAnswerTypeEnum.HouseNumber)
                        {
                            quizAnswerSubmit.PreviousQuestionSubmittedAnswer.SubmittedAnswerDetails.Add(new QuizAnswerSubmit.SubmittedAnswerResult.SubmittedAnswer
                            {
                                AnswerId = submittedAnswerOptionObj.Id,
                                SubmittedAnswerTitle = submittedAnswerOptionObj.QuizAnswerStats.FirstOrDefault(r => r.SubAnswerTypeId == (int)SubAnswerTypeEnum.HouseNumber).AnswerText,
                                SubmittedAnswerImage = ((QuestionsInQuiz)nextQuestionObj).ShowAnswerImage.HasValue && ((QuestionsInQuiz)nextQuestionObj).ShowAnswerImage.Value ? submittedAnswerOptionObj.OptionImage : string.Empty,
                                PublicIdForSubmittedAnswer = ((QuestionsInQuiz)nextQuestionObj).ShowAnswerImage.HasValue && ((QuestionsInQuiz)nextQuestionObj).ShowAnswerImage.Value ? submittedAnswerOptionObj.PublicId : string.Empty,
                                AutoPlay = submittedAnswerOptionObj.AutoPlay,
                                SecondsToApply = submittedAnswerOptionObj.SecondsToApply,
                                VideoFrameEnabled = submittedAnswerOptionObj.VideoFrameEnabled
                            });

                            quizAnswerSubmit.PreviousQuestionSubmittedAnswer.SubmittedAnswerDetails.Add(new QuizAnswerSubmit.SubmittedAnswerResult.SubmittedAnswer
                            {
                                AnswerId = submittedAnswerOptionObj.Id,
                                SubmittedAnswerTitle = submittedAnswerOptionObj.QuizAnswerStats.FirstOrDefault(r => r.SubAnswerTypeId == (int)SubAnswerTypeEnum.Address).AnswerText,
                                SubmittedAnswerImage = ((QuestionsInQuiz)nextQuestionObj).ShowAnswerImage.HasValue && ((QuestionsInQuiz)nextQuestionObj).ShowAnswerImage.Value ? submittedAnswerOptionObj.OptionImage : string.Empty,
                                PublicIdForSubmittedAnswer = ((QuestionsInQuiz)nextQuestionObj).ShowAnswerImage.HasValue && ((QuestionsInQuiz)nextQuestionObj).ShowAnswerImage.Value ? submittedAnswerOptionObj.PublicId : string.Empty,
                                AutoPlay = submittedAnswerOptionObj.AutoPlay,
                                SecondsToApply = submittedAnswerOptionObj.SecondsToApply,
                                VideoFrameEnabled = submittedAnswerOptionObj.VideoFrameEnabled
                            });
                        }

                        break;
                    }

                default:
                    quizAnswerSubmit.PreviousQuestionSubmittedAnswer.SubmittedAnswerDetails.Add(new QuizAnswerSubmit.SubmittedAnswerResult.SubmittedAnswer
                    {
                        AnswerId = submittedAnswerOptionObj.Id,
                        SubmittedAnswerTitle = submittedAnswerOptionObj.QuizAnswerStats.FirstOrDefault().AnswerText,
                        SubmittedSecondaryAnswerTitle = submittedAnswerOptionObj.QuizAnswerStats.FirstOrDefault().AnswerSecondaryText,
                        SubmittedAnswerImage = ((QuestionsInQuiz)nextQuestionObj).ShowAnswerImage.HasValue && ((QuestionsInQuiz)nextQuestionObj).ShowAnswerImage.Value ? submittedAnswerOptionObj.OptionImage : string.Empty,
                        PublicIdForSubmittedAnswer = ((QuestionsInQuiz)nextQuestionObj).ShowAnswerImage.HasValue && ((QuestionsInQuiz)nextQuestionObj).ShowAnswerImage.Value ? submittedAnswerOptionObj.PublicId : string.Empty,
                        AutoPlay = submittedAnswerOptionObj.AutoPlay,
                        SecondsToApply = submittedAnswerOptionObj.SecondsToApply,
                        VideoFrameEnabled = submittedAnswerOptionObj.VideoFrameEnabled
                    });
                    break;
            }
        }

        private void SenqQuizinfotoOWC(QuizAttempts quizAttemptsObj, CompanyModel companyObj, string token, string urlAppointment, LeadDataInAction leadDataInAction, int appointmentTypeId, string sourceId, string sourceName)
        {
            var leadsAppointments = leadInfoUpdateJson.Replace("{ContactId}", quizAttemptsObj.LeadUserId.ToString())
            .Replace("{ClientCode}", companyObj.ClientCode.ToString())
            .Replace("{campaignName}", sourceName)
            .Replace("{calendarId}", "'" + string.Empty + "'")
            .Replace("{calendarTitle}", string.Empty)
            .Replace("{appointmentStatus}", "AppointmentInvitation")
            .Replace("{appointmentDate}", string.Empty)
            .Replace("{appointmentBookedDate}", string.Empty)
            .Replace("{appointmentTypeId}", leadDataInAction != null ? leadDataInAction.AppointmentTypeId.ToString() : appointmentTypeId.ToString())
            .Replace("{appointmentTypeTitle}", string.Empty)
            .Replace("{UserToken}", (GlobalSettings.leadUserToken).ToString())
            .Replace("{SourceId}", sourceId);

            OWCHelper.UpdateLeadsAppointments(leadsAppointments);
            try
            {
                var apiSuccess = OWCHelper.UpdateLeadsAppointments(leadsAppointments);

                if (!apiSuccess)
                    AddPendingApi(urlAppointment, token, leadsAppointments, "POST");
            }
            catch (Exception)
            {
                AddPendingApi(urlAppointment, token, leadsAppointments, "POST");

            }
        }

        private List<Tags> SetAnswerTags(QuizAnswerSubmit quizAnswerSubmit, Db.Quiz quizObj, string clientcode)
        {
            List<Tags> listTags = new List<Tags>();
            if (quizObj.QuizTagDetails.Any())
            {
                //var automationTagsList = OWCHelper.GetAutomationTagsList(clientcode);
                var automationTagsList = CommonStaticData.GetCachedAutomationTagsList(clientcode);

                foreach (var tag in quizObj.QuizTagDetails.ToList())
                {
                    listTags.Add(new Tags()
                    {
                        TagId = tag.TagId,
                        TagName = (automationTagsList != null && automationTagsList.Any(r => r.tagId == tag.TagId)) ? automationTagsList.FirstOrDefault(r => r.tagId == tag.TagId).tagName : string.Empty,
                        TagCode = (automationTagsList != null && automationTagsList.Any(r => r.tagId == tag.TagId)) ? automationTagsList.FirstOrDefault(r => r.tagId == tag.TagId).tagCode : string.Empty
                    });
                }
            }

            return listTags;
        }

        public AttemptedQuizDetails GetAttemptedQuizDetails(string LeadUserId, int QuizId, string ConfigurationId) {
            AttemptedQuizDetails attemptedQuizDetails = new AttemptedQuizDetails();
            attemptedQuizDetails.Question = new List<AttemptedQuizDetails.Questions>();
            var attemptedAns = new List<QuizAnswerStats>();
            var quizstatus = new QuizStats();
            var quizDetails = new QuizDetails();
            var quizAttempt = new QuizAttempts();
            var quiz = new Quiz();

            try {

                using (var UOWObj = new AutomationUnitOfWork()) {
                    quizDetails = UOWObj.QuizDetailsRepository.Get(v => v.ParentQuizId == QuizId).LastOrDefault();
                    var configurationDetails = UOWObj.ConfigurationDetailsRepository.Get(v => v.ConfigurationId == ConfigurationId).FirstOrDefault();
                    attemptedQuizDetails.QuizDetailId = quizDetails.Id;
                    var workPackageInfo = UOWObj.WorkPackageInfoRepository.Get(v => v.LeadUserId == LeadUserId && v.QuizId == QuizId).LastOrDefault();
                    if (workPackageInfo == null) { quizAttempt = UOWObj.QuizAttemptsRepository.Get(v => v.LeadUserId == LeadUserId && v.ConfigurationDetailsId == configurationDetails.Id).FirstOrDefault(); }
                    else {
                        quizAttempt = UOWObj.QuizAttemptsRepository.Get(v => v.LeadUserId == LeadUserId && v.WorkPackageInfoId == workPackageInfo.Id && v.ConfigurationDetailsId == configurationDetails.Id).FirstOrDefault();
                    }

                    if (quizAttempt != null) {
                        attemptedQuizDetails.QuizDetailId = quizAttempt.QuizId;
                    }


                    attemptedQuizDetails.QuizAttemptId = quizAttempt?.Id ?? 0;
                    if (attemptedQuizDetails.QuizAttemptId != 0) {
                        quizstatus = UOWObj.QuizStatsRepository.Get(v => v.QuizAttemptId == attemptedQuizDetails.QuizAttemptId).Where(v => (v.CompletedOn != null && v.ResultId != null) || (v.AttemptStatus == 4 && v.CompletedOn != null)).FirstOrDefault();
                    }

                    if (quizstatus == null) {
                        return attemptedQuizDetails;

                    }
                    else {

                        if (quizDetails != null && quizDetails.ParentQuizId != 0) {
                            attemptedQuizDetails.QuizCoverTitle = quizDetails.QuizCoverTitle;
                            attemptedQuizDetails.QuizTitle = quizDetails.QuizTitle;
                            quiz = UOWObj.QuizRepository.GetQuizById(quizDetails.ParentQuizId).FirstOrDefault();
                        }

                        var quesStatList = UOWObj.QuizQuestionStatsRepository.GetQuizQuestionStatsByQuizAttemptId(attemptedQuizDetails.QuizAttemptId);

                        if (quesStatList == null) {

                            return attemptedQuizDetails;
                        }


                        var quesList = UOWObj.QuestionsInQuizRepository.GetExistingQuestioninQuiz(attemptedQuizDetails.QuizDetailId).Where(a => a.Status == (int)StatusEnum.Active).OrderBy(v => v.DisplayOrder);

                        attemptedQuizDetails.QuizType = quiz.QuizType;

                        foreach (var ques in quesList) {

                            var questionstat = quesStatList.Where(v => v.QuestionId.Equals(ques.Id)).FirstOrDefault();
                            if (questionstat == null) {
                                continue;
                            }

                            var question = new AttemptedQuizDetails.Questions {
                                QuestionId = ques.Id,
                                QuestionTitle = ques.Question,
                                AnswerType = ques.AnswerType,
                                AnswerList = new List<AttemptedQuizDetails.Questions.Answers>()
                            };

                            attemptedAns = UOWObj.QuizAnswerStatsRepository.Get(v => v.QuizQuestionStatsId == questionstat.Id).ToList();

                            foreach (var attempt in attemptedAns) {
                                var ansDetails = UOWObj.AnswerOptionsInQuizQuestionsRepository.Get(v => v.Id == attempt.AnswerId).FirstOrDefault();

                                var questionAnswer = new AttemptedQuizDetails.Questions.Answers {
                                    AnswerId = ansDetails.Id,
                                    Title = attempt.AnswerText ?? ansDetails.Option,
                                    AnswerDescription = ansDetails.Description,
                                    Comment = attempt.Comment
                                };

                                if (ques.AnswerType == (int)AnswerTypeEnum.Availability) {
                                    questionAnswer.RatingOptionTitle = GetAvailabilityTitle(attempt.AnswerText.ToInt());
                                }
                                else if (ques.AnswerType == (int)AnswerTypeEnum.RatingEmoji) {
                                    questionAnswer.RatingOptionTitle = GetEmojiTitle(attempt.AnswerText.ToInt(), ansDetails);
                                }
                                else if (ques.AnswerType == (int)AnswerTypeEnum.RatingStarts) {
                                    questionAnswer.RatingOptionTitle = GetStarRatingTitle(attempt.AnswerText.ToInt());
                                }

                                question.AnswerList.Add(questionAnswer);
                            }

                            attemptedQuizDetails.Question.Add(question);
                        }
                    }
                    return attemptedQuizDetails;
                }

            } catch (Exception ex) {

            }
            return null;
        }
        public AttemptedQuizDetails GetAttemptedQuizDetailsV2(string LeadUserId, int QuizId, string ConfigurationId) {
            AttemptedQuizDetails attemptedQuizDetails = new AttemptedQuizDetails();
            attemptedQuizDetails.Question = new List<AttemptedQuizDetails.Questions>();
            var attemptedAns = new List<QuizAnswerStats>();
            var quizstatus = new QuizStats();
            var quizDetails = new QuizDetails();
            var quizAttempt = new QuizAttempts();
            var quiz = new Quiz();

            try {

                using (var UOWObj = new AutomationUnitOfWork()) {
                    quizDetails = UOWObj.QuizDetailsRepository.Get(v => v.ParentQuizId == QuizId).LastOrDefault();
                    var configurationDetails = UOWObj.ConfigurationDetailsRepository.Get(v => v.ConfigurationId == ConfigurationId).FirstOrDefault();
                    attemptedQuizDetails.QuizDetailId = quizDetails.Id;
                    var workPackageInfo = UOWObj.WorkPackageInfoRepository.Get(v => v.LeadUserId == LeadUserId && v.QuizId == QuizId).LastOrDefault();
                    if (workPackageInfo == null) { quizAttempt = UOWObj.QuizAttemptsRepository.Get(v => v.LeadUserId == LeadUserId && v.ConfigurationDetailsId == configurationDetails.Id).FirstOrDefault(); }
                    else {
                        quizAttempt = UOWObj.QuizAttemptsRepository.Get(v => v.LeadUserId == LeadUserId && v.WorkPackageInfoId == workPackageInfo.Id && v.ConfigurationDetailsId == configurationDetails.Id).FirstOrDefault();
                    }

                    if (quizAttempt != null) {
                        attemptedQuizDetails.QuizDetailId = quizAttempt.QuizId;
                    }


                    attemptedQuizDetails.QuizAttemptId = quizAttempt?.Id ?? 0;
                    if (attemptedQuizDetails.QuizAttemptId != 0) {
                        quizstatus = UOWObj.QuizStatsRepository.Get(v => v.QuizAttemptId == attemptedQuizDetails.QuizAttemptId).Where(v => (v.CompletedOn != null && v.ResultId != null) || (v.AttemptStatus == 4 && v.CompletedOn != null)).FirstOrDefault();
                    }

                    if (quizstatus == null) {
                        return attemptedQuizDetails;

                    }
                    else {

                        if (quizDetails != null && quizDetails.ParentQuizId != 0) {
                            attemptedQuizDetails.QuizCoverTitle = quizDetails.QuizCoverTitle;
                            attemptedQuizDetails.QuizTitle = quizDetails.QuizTitle;
                            quiz = UOWObj.QuizRepository.GetQuizById(quizDetails.ParentQuizId).FirstOrDefault();
                        }

                        var quesStatList = UOWObj.QuizQuestionStatsRepository.GetQuizQuestionStatsByQuizAttemptId(attemptedQuizDetails.QuizAttemptId).Where(a => a.Status == (int)StatusEnum.Active);

                        if (quesStatList == null) {

                            return attemptedQuizDetails;
                        }


                        var quesList = UOWObj.QuestionsInQuizRepository.GetExistingQuestioninQuiz(attemptedQuizDetails.QuizDetailId).Where(a => a.Status == (int)StatusEnum.Active).OrderBy(v => v.DisplayOrder);

                        attemptedQuizDetails.QuizType = quiz.QuizType;

                        foreach (var ques in quesStatList) {
                            var questionstats = quesList.Where(v => v.Id.Equals(ques.QuestionId)).FirstOrDefault();
                            if (questionstats == null) {
                                continue;
                            }
                            //var quesDetail = UOWObj.QuestionsInQuizRepository.Get(v => v.Id == ques.QuestionId).Where(a => a.Status == (int)StatusEnum.Active).FirstOrDefault();
                            var question = new AttemptedQuizDetails.Questions {
                                QuestionId = questionstats.Id,
                                QuestionTitle = questionstats.Question,
                                AnswerType = questionstats.AnswerType,
                                AnswerList = new List<AttemptedQuizDetails.Questions.Answers>()
                            };
                        

                        attemptedAns = UOWObj.QuizAnswerStatsRepository.Get(v => v.QuizQuestionStatsId == ques.Id).ToList();

                            foreach (var attempt in attemptedAns) {
                                var ansDetails = UOWObj.AnswerOptionsInQuizQuestionsRepository.Get(v => v.Id == attempt.AnswerId).FirstOrDefault();

                                var questionAnswer = new AttemptedQuizDetails.Questions.Answers {
                                    AnswerId = ansDetails.Id,
                                    Title = attempt.AnswerText ?? ansDetails.Option,
                                    AnswerDescription = ansDetails.Description,
                                    Comment = attempt.Comment
                                };

                                if (questionstats.AnswerType == (int)AnswerTypeEnum.Availability) {
                                    questionAnswer.RatingOptionTitle = GetAvailabilityTitle(attempt.AnswerText.ToInt());
                                }
                                else if (questionstats.AnswerType == (int)AnswerTypeEnum.RatingEmoji) {
                                    questionAnswer.RatingOptionTitle = GetEmojiTitle(attempt.AnswerText.ToInt(), ansDetails);
                                }
                                else if (questionstats.AnswerType == (int)AnswerTypeEnum.RatingStarts) {
                                    questionAnswer.RatingOptionTitle = GetStarRatingTitle(attempt.AnswerText.ToInt());
                                }

                                question.AnswerList.Add(questionAnswer);
                            }

                            attemptedQuizDetails.Question.Add(question);
                        }
                    }
                    return attemptedQuizDetails;
                }

            } catch (Exception ex) {

            }
            return null;
        }


        //public AttemptedQuizDetails GetAttemptedQuizDetailsOld(string LeadUserId, int QuizId, string ConfigurationId) {
        //    AttemptedQuizDetails attemptedQuizDetails = new AttemptedQuizDetails();
        //    attemptedQuizDetails.Question = new List<AttemptedQuizDetails.Questions>();
        //    var attemptedAns = new List<QuizAnswerStats>();
        //    var quizstatus = new QuizStats();
        //    var quizDetails = new QuizDetails();
        //    var quizAttempt = new QuizAttempts();
        //    var quiz = new Quiz();

        //    try {

        //        using (var UOWObj = new AutomationUnitOfWork()) {
        //            quizDetails = UOWObj.QuizDetailsRepository.Get(v => v.ParentQuizId == QuizId).LastOrDefault();
        //            var configurationDetails = UOWObj.ConfigurationDetailsRepository.Get(v => v.ConfigurationId == ConfigurationId).FirstOrDefault();
        //            attemptedQuizDetails.QuizDetailId = quizDetails.Id;
        //            var workPackageInfo = UOWObj.WorkPackageInfoRepository.Get(v => v.LeadUserId == LeadUserId && v.QuizId == QuizId).LastOrDefault();
        //            if(workPackageInfo == null) {  quizAttempt = UOWObj.QuizAttemptsRepository.Get(v => v.LeadUserId == LeadUserId &&  v.ConfigurationDetailsId == configurationDetails.Id).FirstOrDefault(); } 
        //            else {
        //                quizAttempt = UOWObj.QuizAttemptsRepository.Get(v => v.LeadUserId == LeadUserId && v.WorkPackageInfoId == workPackageInfo.Id && v.ConfigurationDetailsId == configurationDetails.Id).FirstOrDefault();
        //            }

        //            if (quizAttempt != null) {
        //                attemptedQuizDetails.QuizDetailId = quizAttempt.QuizId;
        //            }


        //            attemptedQuizDetails.QuizAttemptId = quizAttempt?.Id ?? 0;
        //            if (attemptedQuizDetails.QuizAttemptId != 0) {
        //                quizstatus = UOWObj.QuizStatsRepository.Get(v => v.QuizAttemptId == attemptedQuizDetails.QuizAttemptId).Where(v => (v.CompletedOn != null && v.ResultId != null) || (v.AttemptStatus == 4 && v.CompletedOn != null)).FirstOrDefault();
        //            }

        //            if (quizstatus == null) {
        //                return attemptedQuizDetails;

        //            } else {

        //                if (quizDetails != null && quizDetails.ParentQuizId != 0) {
        //                    attemptedQuizDetails.QuizCoverTitle = quizDetails.QuizCoverTitle;
        //                    attemptedQuizDetails.QuizTitle = quizDetails.QuizTitle;
        //                    quiz = UOWObj.QuizRepository.GetQuizById(quizDetails.ParentQuizId).FirstOrDefault();
        //                }

        //                var quesStatList = UOWObj.QuizQuestionStatsRepository.GetQuizQuestionStatsByQuizAttemptId(attemptedQuizDetails.QuizAttemptId);

        //                if (quesStatList == null) {

        //                    return attemptedQuizDetails;
        //                }


        //                var quesList = UOWObj.QuestionsInQuizRepository.GetExistingQuestioninQuiz(attemptedQuizDetails.QuizDetailId).Where(a => a.Status == (int)StatusEnum.Active).OrderBy(v => v.DisplayOrder);

        //                attemptedQuizDetails.QuizType = quiz.QuizType;

        //                foreach (var ques in quesList) {

        //                    var questionstat = quesStatList.Where(v => v.QuestionId.Equals(ques.Id)).FirstOrDefault();
        //                    if (questionstat == null) {
        //                        continue;
        //                    }

        //                    var question = new AttemptedQuizDetails.Questions {
        //                        QuestionId = ques.Id,
        //                        QuestionTitle = ques.Question,
        //                        AnswerType = ques.AnswerType,
        //                        AnswerList = new List<AttemptedQuizDetails.Questions.Answers>()
        //                    };

        //                    attemptedAns = UOWObj.QuizAnswerStatsRepository.Get(v => v.QuizQuestionStatsId == questionstat.Id).ToList();

        //                    foreach (var attempt in attemptedAns) {
        //                        var ansDetails = UOWObj.AnswerOptionsInQuizQuestionsRepository.Get(v => v.Id == attempt.AnswerId).FirstOrDefault();
        //                        if(ques.AnswerType == (int)AnswerTypeEnum.Availability) {
        //                            string title = string.Empty;
        //                            switch(attempt.AnswerText.ToInt()) {
        //                                case 1:
        //                                    title = AvailableDateOptionsEnglish.RatingOne;
        //                                    break;
        //                                case 2: 
        //                                    title = AvailableDateOptionsEnglish.RatingTwo;
        //                                    break;
        //                                case 3:
        //                                    title = AvailableDateOptionsEnglish.RatingThree;
        //                                    break;
        //                            }

        //                            question.AnswerList.Add(new AttemptedQuizDetails.Questions.Answers {
        //                                AnswerId = ansDetails.Id,
        //                                Title = attempt.AnswerText ?? ansDetails.Option,
        //                                RatingOptionTitle = title,
        //                                AnswerDescription = ansDetails.Description,
        //                                Comment = attempt.Comment
        //                            });

        //                        }

        //                        else if (ques.AnswerType == (int)AnswerTypeEnum.RatingEmoji) {
        //                            string title = string.Empty;
        //                            switch (attempt.AnswerText.ToInt()) {
        //                                case 1:
        //                                    title = !string.IsNullOrWhiteSpace(ansDetails.OptionTextforRatingOne) ? ansDetails.OptionTextforRatingOne : EmojiQuestionOptionsEnglish.RatingOne;
        //                                    break;
        //                                case 2:
        //                                    title = !string.IsNullOrWhiteSpace(ansDetails.OptionTextforRatingTwo) ? ansDetails.OptionTextforRatingTwo : EmojiQuestionOptionsEnglish.RatingTwo;
        //                                    break;
        //                                case 3:
        //                                    title = !string.IsNullOrWhiteSpace(ansDetails.OptionTextforRatingThree) ? ansDetails.OptionTextforRatingThree : EmojiQuestionOptionsEnglish.RatingThree;
        //                                    break;
        //                                case 4:
        //                                    title = !string.IsNullOrWhiteSpace(ansDetails.OptionTextforRatingFour) ? ansDetails.OptionTextforRatingFour : EmojiQuestionOptionsEnglish.RatingFour;
        //                                    break;
        //                                case 5:
        //                                    title = !string.IsNullOrWhiteSpace(ansDetails.OptionTextforRatingFive) ? ansDetails.OptionTextforRatingFive : EmojiQuestionOptionsEnglish.RatingFive;
        //                                    break;
        //                            }

        //                            question.AnswerList.Add(new AttemptedQuizDetails.Questions.Answers {
        //                                AnswerId = ansDetails.Id, 
        //                                Title = attempt.AnswerText ?? ansDetails.Option,
        //                                RatingOptionTitle = title,
        //                                AnswerDescription = ansDetails.Description,
        //                                Comment = attempt.Comment
        //                            });

        //                        }

        //                        else if (ques.AnswerType == (int)AnswerTypeEnum.RatingStarts) {
        //                            string title = string.Empty;
        //                            switch (attempt.AnswerText.ToInt()) {
        //                                case 1:
        //                                    title = StarQuestionOptionsEnglish.RatingOne;
        //                                    break;
        //                                case 2:
        //                                    title = StarQuestionOptionsEnglish.RatingTwo;
        //                                    break;
        //                                case 3:
        //                                    title = StarQuestionOptionsEnglish.RatingThree;
        //                                    break;
        //                                case 4:
        //                                    title = StarQuestionOptionsEnglish.RatingFour;
        //                                    break;
        //                                case 5:
        //                                    title = StarQuestionOptionsEnglish.RatingFive;
        //                                    break;
        //                            }

        //                            question.AnswerList.Add(new AttemptedQuizDetails.Questions.Answers {
        //                                AnswerId = ansDetails.Id,
        //                                Title = attempt.AnswerText ?? ansDetails.Option,
        //                                RatingOptionTitle = title,
        //                                AnswerDescription = ansDetails.Description,
        //                                Comment = attempt.Comment
        //                            });

        //                        }

        //                        else {
        //                            question.AnswerList.Add(new AttemptedQuizDetails.Questions.Answers {
        //                                AnswerId = ansDetails.Id,
        //                                Title = attempt.AnswerText ?? ansDetails.Option,
        //                                AnswerDescription = ansDetails.Description,
        //                                Comment = attempt.Comment
        //                            });


        //                        }



        //                    }

        //                    attemptedQuizDetails.Question.Add(question);
        //                }
        //            }
        //            return attemptedQuizDetails;
        //        }

        //    } catch (Exception ex) {

        //    }
        //    return null;
        //}
        private string GetAvailabilityTitle(int rating) {
            switch (rating) {
                case 1:
                    return AvailableDateOptionsEnglish.RatingOne;
                case 2:
                    return AvailableDateOptionsEnglish.RatingTwo;
                case 3:
                    return AvailableDateOptionsEnglish.RatingThree;
                default:
                    return string.Empty;
            }
        }
        private string GetEmojiTitle(int rating, AnswerOptionsInQuizQuestions ansDetails) {
            switch (rating) {
                case 1:
                    return !string.IsNullOrWhiteSpace(ansDetails.OptionTextforRatingOne) ? ansDetails.OptionTextforRatingOne : EmojiQuestionOptionsEnglish.RatingOne;
                case 2:
                    return !string.IsNullOrWhiteSpace(ansDetails.OptionTextforRatingTwo) ? ansDetails.OptionTextforRatingTwo : EmojiQuestionOptionsEnglish.RatingTwo;
                case 3:
                    return !string.IsNullOrWhiteSpace(ansDetails.OptionTextforRatingThree) ? ansDetails.OptionTextforRatingThree : EmojiQuestionOptionsEnglish.RatingThree;
                case 4:
                    return !string.IsNullOrWhiteSpace(ansDetails.OptionTextforRatingFour) ? ansDetails.OptionTextforRatingFour : EmojiQuestionOptionsEnglish.RatingFour;
                case 5:
                    return !string.IsNullOrWhiteSpace(ansDetails.OptionTextforRatingFive) ? ansDetails.OptionTextforRatingFive : EmojiQuestionOptionsEnglish.RatingFive;
                default:
                    return string.Empty;
            }
        }
        private string GetStarRatingTitle(int rating) {
            switch (rating) {
                case 1:
                    return StarQuestionOptionsEnglish.RatingOne;
                case 2:
                    return StarQuestionOptionsEnglish.RatingTwo;
                case 3:
                    return StarQuestionOptionsEnglish.RatingThree;
                case 4:
                    return StarQuestionOptionsEnglish.RatingFour;
                case 5:
                    return StarQuestionOptionsEnglish.RatingFive;
                default:
                    return string.Empty;
            }
        }

        #endregion

        #region Old Method SetDefaulQuestionAnswer
        void SetDefaulQuestionAnswer(ref string Type, ref int QuestionId, Db.QuizAttempts quizAttemptsObj, ref List<int> AnswerId, ref bool isLastQuestionAttempted, ref bool isQuizAlreadyStarted, ref bool isLastQuestionStarted, ref bool revealScore, Db.QuizStats quizStatsObj)
        {
            if (string.IsNullOrEmpty(Type))
            {
                if (quizStatsObj == null)
                {
                    Type = "fetch_quiz";
                    QuestionId = -1;
                    AnswerId = null;
                }
                else
                {
                    if (quizStatsObj.CompletedOn.HasValue)
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz has already been completed.";
                    }
                    else
                    {
                        var questionStats = quizAttemptsObj.QuizQuestionStats.Where(r => r.Status == (int)StatusEnum.Active).ToList();

                        //if no question was attempted in the past
                        if (questionStats.Count == 0)
                        {
                            isQuizAlreadyStarted = true;
                            Type = "start_quiz";
                        }

                        //if any question was attempted in the past
                        else
                        {
                            var lastAttemptedQuestion = questionStats.OrderByDescending(r => r.StartedOn).FirstOrDefault();

                            if (lastAttemptedQuestion.CompletedOn.HasValue)
                            {
                                Type = "complete_question";
                                isLastQuestionAttempted = true;
                                QuestionId = lastAttemptedQuestion.QuestionId;
                                foreach (var obj in lastAttemptedQuestion.QuizAnswerStats)
                                {
                                    AnswerId.Add(obj.AnswerId);
                                }
                            }
                            else
                            {
                                if (questionStats.Count == 1)
                                {
                                    isQuizAlreadyStarted = true;
                                    isLastQuestionStarted = true;
                                    Type = "start_quiz";
                                }
                                else
                                {
                                    isLastQuestionAttempted = true;
                                    isLastQuestionStarted = true;
                                    revealScore = false;

                                    lastAttemptedQuestion = questionStats.OrderByDescending(r => r.StartedOn).Skip(1).Take(1).FirstOrDefault();
                                    QuestionId = lastAttemptedQuestion.QuestionId;

                                    if (lastAttemptedQuestion.QuestionsInQuiz.Type == (int)BranchingLogicEnum.QUESTION)
                                        Type = "complete_question";
                                    else
                                        Type = "complete_content";

                                    foreach (var obj in lastAttemptedQuestion.QuizAnswerStats)
                                    {
                                        AnswerId.Add(obj.AnswerId);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Old Method DeleteQuizCache
        private void DeleteQuizCache(int quizId)
        {
            //AppLocalCache.Remove("QuizReportList_QuizId_" + quizId);
            //AppLocalCache.Remove("QuizQuestionStatsList_QuizId_" + quizId);
            //AppLocalCache.Remove("QuizDetails_QuizId_" + quizId);
        }
        #endregion

        #region Old Method FetchPreviousQuestion
        public object FetchPreviousQuestion(Db.QuizAttempts quizAttemptsObj, int AnswerId = -1, int TypeId = (int)BranchingLogicEnum.QUESTION, AutomationUnitOfWork UOWObj = null, bool IsQuesAndContentInSameTable = false)
        {
            using (UOWObj == null ? UOWObj = new AutomationUnitOfWork() : null)
            {
                var quizDetails = quizAttemptsObj.QuizDetails;
                var quizStats = quizAttemptsObj.QuizStats.FirstOrDefault();

                if (quizDetails.IsBranchingLogicEnabled.HasValue && quizDetails.IsBranchingLogicEnabled.Value)
                {
                    if (AnswerId == -1)
                    {
                        return null;
                    }
                    else
                    {
                        var quizBranchingList = quizDetails.BranchingLogic.Where(a => a.DestinationObjectId == AnswerId && a.DestinationTypeId == TypeId);
                        var quizBranching = new Db.BranchingLogic();

                        foreach (var quizBranchingObj in quizBranchingList)
                        {
                            if (quizBranchingObj.SourceTypeId == (int)BranchingLogicEnum.ANSWER)
                            {
                                var answerOptionsInQuizQuestionsObj = UOWObj.QuestionsInQuizRepository.Get(a => a.Status == (int)StatusEnum.Active && a.AnswerOptionsInQuizQuestions.Any(q => q.Id == quizBranchingObj.SourceObjectId)).FirstOrDefault();

                                if (quizAttemptsObj.QuizQuestionStats.Any(r => r.Status == (int)StatusEnum.Active && r.QuestionId == answerOptionsInQuizQuestionsObj.Id))
                                {
                                    quizBranching = quizBranchingObj;
                                    return answerOptionsInQuizQuestionsObj;
                                }
                            }

                            if (quizBranchingObj.SourceTypeId == (int)BranchingLogicEnum.QUESTIONNEXT)
                            {
                                var answerOptionsInQuizQuestionsObj = UOWObj.QuestionsInQuizRepository.Get(a => a.Status == (int)StatusEnum.Active && a.Id == quizBranchingObj.SourceObjectId).FirstOrDefault();

                                if (quizAttemptsObj.QuizQuestionStats.Any(r => r.Status == (int)StatusEnum.Active && r.QuestionId == quizBranchingObj.SourceObjectId))
                                {
                                    quizBranching = quizBranchingObj;
                                    return answerOptionsInQuizQuestionsObj;
                                }
                            }

                            if (quizBranchingObj.SourceTypeId == (int)BranchingLogicEnum.CONTENTNEXT && IsQuesAndContentInSameTable)
                            {
                                var answerOptionsInQuizQuestionsObj = UOWObj.QuestionsInQuizRepository.Get(a => a.Status == (int)StatusEnum.Active && a.Id == quizBranchingObj.SourceObjectId).FirstOrDefault();

                                if (quizAttemptsObj.QuizQuestionStats.Any(r => r.Status == (int)StatusEnum.Active && r.QuestionId == quizBranchingObj.SourceObjectId))
                                {
                                    quizBranching = quizBranchingObj;
                                    return answerOptionsInQuizQuestionsObj;
                                }
                            }

                            if (quizBranchingObj.SourceTypeId == (int)BranchingLogicEnum.CONTENTNEXT && !IsQuesAndContentInSameTable)
                            {
                                var contentsInQuizObj = UOWObj.ContentsInQuizRepository.Get(a => a.Status == (int)StatusEnum.Active && a.Id == quizBranchingObj.SourceObjectId).FirstOrDefault();
                                if (quizAttemptsObj.QuizObjectStats.Any(r => r.TypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == quizBranchingObj.SourceObjectId))
                                {
                                    quizBranching = quizBranchingObj;
                                    return contentsInQuizObj;
                                }
                            }
                        }

                        if (quizBranching == null) return null;

                        return null;
                    }
                }
                else
                {
                    int currentQuesDisplayOrder = 0;

                    if (TypeId == (int)BranchingLogicEnum.CONTENT && !IsQuesAndContentInSameTable)
                        currentQuesDisplayOrder = quizDetails.ContentsInQuiz.Where(r => r.Id == AnswerId).Select(r => r.DisplayOrder).FirstOrDefault();
                    else
                        currentQuesDisplayOrder = quizDetails.QuestionsInQuiz.Where(r => r.Id == AnswerId).Select(r => r.DisplayOrder).FirstOrDefault();

                    var question = quizDetails.QuestionsInQuiz.OrderByDescending(r => r.DisplayOrder).FirstOrDefault(r => r.Status == (int)StatusEnum.Active && r.DisplayOrder < currentQuesDisplayOrder);

                    var content = quizDetails.ContentsInQuiz.OrderByDescending(r => r.DisplayOrder).FirstOrDefault(r => r.Status == (int)StatusEnum.Active && r.DisplayOrder < currentQuesDisplayOrder);

                    if (question == null && content == null)
                        return null;
                    else if (question == null)
                        return content;
                    else if (content == null)
                        return question;
                    else if (question.DisplayOrder < content.DisplayOrder)
                        return content;
                    else
                        return question;
                }
            }
        }
        #endregion


        #region Old Method FetchNextQuestion
        public object FetchNextQuestion(Db.QuizDetails quizDetails, int AnswerId = -1, int TypeId = (int)BranchingLogicEnum.QUESTION, AutomationUnitOfWork UOWObj = null, bool IsQuesAndContentInSameTable = false)
        {
            using (UOWObj == null ? UOWObj = new AutomationUnitOfWork() : null)
            {
                if (quizDetails.IsBranchingLogicEnabled.HasValue && quizDetails.IsBranchingLogicEnabled.Value)
                {
                    if (AnswerId == -1)
                    {
                        var quizBranching = quizDetails.BranchingLogic.FirstOrDefault(a => a.IsStartingPoint);
                        if (quizBranching == null) return null;
                        if (quizBranching.SourceTypeId == (int)BranchingLogicEnum.QUESTION)
                        {
                            var questionsInQuizObj = UOWObj.QuestionsInQuizRepository.Get(a => a.Status == (int)StatusEnum.Active && a.Id == quizBranching.SourceObjectId).FirstOrDefault();
                            return questionsInQuizObj;
                        }
                        if (quizBranching.SourceTypeId == (int)BranchingLogicEnum.ANSWER)
                        {
                            var answerOptionsInQuizQuestionsObj = UOWObj.AnswerOptionsInQuizQuestionsRepository.Get(a => a.Status == (int)StatusEnum.Active && a.Id == quizBranching.SourceObjectId).FirstOrDefault().QuestionsInQuiz;
                            return answerOptionsInQuizQuestionsObj;
                        }
                        if (quizBranching.SourceTypeId == (int)BranchingLogicEnum.CONTENT && IsQuesAndContentInSameTable)
                        {
                            var questionsInQuizObj = UOWObj.QuestionsInQuizRepository.Get(a => a.Status == (int)StatusEnum.Active && a.Id == quizBranching.SourceObjectId).FirstOrDefault();
                            return questionsInQuizObj;
                        }
                        if (quizBranching.SourceTypeId == (int)BranchingLogicEnum.CONTENT && !IsQuesAndContentInSameTable)
                        {
                            var contentsInQuizObj = UOWObj.ContentsInQuizRepository.Get(a => a.Status == (int)StatusEnum.Active && a.Id == quizBranching.SourceObjectId).FirstOrDefault();
                            return contentsInQuizObj;
                        }
                        if (quizBranching.SourceTypeId == (int)BranchingLogicEnum.RESULT)
                        {
                            var quizResultsObj = UOWObj.QuizResultsRepository.Get(a => a.Status == (int)StatusEnum.Active && a.Id == quizBranching.SourceObjectId).FirstOrDefault();
                            return quizResultsObj;
                        }
                        if (quizBranching.SourceTypeId == (int)BranchingLogicEnum.BADGE && quizBranching.SourceTypeId == (int)BranchingLogicEnum.BADGENEXT)
                        {
                            var badgesInQuizObj = UOWObj.BadgesInQuizRepository.Get(a => a.Status == (int)StatusEnum.Active && a.Id == quizBranching.SourceObjectId).FirstOrDefault();
                            return badgesInQuizObj;
                        }

                        return null;
                    }
                    else
                    {
                        var quizBranching = quizDetails.BranchingLogic.FirstOrDefault(a => a.SourceObjectId == AnswerId && a.SourceTypeId == TypeId);
                        if (quizBranching == null) return null;
                        if (quizBranching.DestinationTypeId == (int)BranchingLogicEnum.QUESTION)
                        {
                            var answerOptionsInQuizQuestionsObj = UOWObj.QuestionsInQuizRepository.Get(a => a.Status == (int)StatusEnum.Active && a.Id == quizBranching.DestinationObjectId).FirstOrDefault();
                            return answerOptionsInQuizQuestionsObj;
                        }
                        if (quizBranching.DestinationTypeId == (int)BranchingLogicEnum.CONTENT && IsQuesAndContentInSameTable)
                        {
                            var answerOptionsInQuizQuestionsObj = UOWObj.QuestionsInQuizRepository.Get(a => a.Status == (int)StatusEnum.Active && a.Id == quizBranching.DestinationObjectId).FirstOrDefault();
                            return answerOptionsInQuizQuestionsObj;
                        }
                        if (quizBranching.DestinationTypeId == (int)BranchingLogicEnum.CONTENT && !IsQuesAndContentInSameTable)
                        {
                            var contentsInQuizObj = UOWObj.ContentsInQuizRepository.Get(a => a.Status == (int)StatusEnum.Active && a.Id == quizBranching.DestinationObjectId).FirstOrDefault();
                            return contentsInQuizObj;
                        }
                        if (quizBranching.DestinationTypeId == (int)BranchingLogicEnum.RESULT)
                        {
                            var quizResultsObj = UOWObj.QuizResultsRepository.Get(a => a.Status == (int)StatusEnum.Active && a.Id == quizBranching.DestinationObjectId).FirstOrDefault();
                            return quizResultsObj;
                        }
                        if (quizBranching.DestinationTypeId == (int)BranchingLogicEnum.BADGE)
                        {
                            var badgesInQuizObj = UOWObj.BadgesInQuizRepository.Get(a => a.Status == (int)StatusEnum.Active && a.Id == quizBranching.DestinationObjectId).FirstOrDefault();
                            return badgesInQuizObj;
                        }
                        return null;
                    }
                }
                else
                {
                    int currentQuesDisplayOrder = 0;

                    if (TypeId == (int)BranchingLogicEnum.QUESTIONNEXT)
                        currentQuesDisplayOrder = quizDetails.QuestionsInQuiz.Where(r => r.Id == AnswerId).Select(r => r.DisplayOrder).FirstOrDefault();
                    else if (TypeId == (int)BranchingLogicEnum.CONTENTNEXT && IsQuesAndContentInSameTable)
                        currentQuesDisplayOrder = quizDetails.QuestionsInQuiz.Where(r => r.Id == AnswerId).Select(r => r.DisplayOrder).FirstOrDefault();
                    else if (TypeId == (int)BranchingLogicEnum.CONTENTNEXT && !IsQuesAndContentInSameTable)
                        currentQuesDisplayOrder = quizDetails.ContentsInQuiz.Where(r => r.Id == AnswerId).Select(r => r.DisplayOrder).FirstOrDefault();
                    else
                        currentQuesDisplayOrder = quizDetails.QuestionsInQuiz.Where(r => r.AnswerOptionsInQuizQuestions.Any(t => t.Id == AnswerId)).Select(r => r.DisplayOrder).FirstOrDefault();

                    var question = quizDetails.QuestionsInQuiz.OrderBy(r => r.DisplayOrder).FirstOrDefault(r => r.Status == (int)StatusEnum.Active && r.DisplayOrder > currentQuesDisplayOrder);

                    var content = quizDetails.ContentsInQuiz.OrderBy(r => r.DisplayOrder).FirstOrDefault(r => r.Status == (int)StatusEnum.Active && r.DisplayOrder > currentQuesDisplayOrder);

                    if (question == null && content == null)
                        return null;
                    else if (question == null)
                        return content;
                    else if (content == null)
                        return question;
                    else if (question.DisplayOrder < content.DisplayOrder)
                        return question;
                    else
                        return content;
                }
            }
        }
        #endregion

        #region Old Unused Methods
        public static string VariableLinking(string Text, QuizApp.Db.QuizDetails QuizDetail, QuizApp.Db.QuizAttempts QuizAttempt, bool IsTitle, bool ShowScore, string ResultScore)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    if (!string.IsNullOrEmpty(Text) && Regex.Matches(Text, @"%\b\S+?\b%").Count > 0)
                    {
                        var LeadInfo = new QuizApp.Response.OWCLeadUserResponse.LeadUserResponse();

                        CompanyModel compObj;

                        if (QuizDetail != null && QuizDetail.Quiz.Company != null)
                        {
                            compObj = new CompanyModel
                            {
                                LeadDashboardApiAuthorizationBearer = QuizDetail.Quiz.Company.LeadDashboardApiAuthorizationBearer,
                                LeadDashboardApiUrl = QuizDetail.Quiz.Company.LeadDashboardApiUrl,
                                ClientCode = QuizDetail.Quiz.Company.ClientCode
                            };
                        }
                        else
                            compObj = new CompanyModel();


                        if (QuizAttempt != null && !string.IsNullOrEmpty(QuizAttempt.LeadUserId))
                            LeadInfo = OWCHelper.GetLeadUserInfo(QuizAttempt.LeadUserId, compObj);

                        Text = string.IsNullOrEmpty(Text) ? string.Empty : Text;

                        StringBuilder correctanswerexplanation = new StringBuilder("Incorrect answer details:<br/>");
                        if (Text.Contains("%correctanswerexplanation%") && QuizDetail != null && (QuizDetail.Quiz.QuizType == (int)QuizTypeEnum.Assessment || QuizDetail.Quiz.QuizType == (int)QuizTypeEnum.AssessmentTemplate))
                        {
                            if (QuizAttempt != null)
                            {
                                foreach (var ques in QuizAttempt.QuizQuestionStats.Where(r => r.Status == (int)StatusEnum.Active && r.QuizAnswerStats.Any(k => !k.AnswerOptionsInQuizQuestions.IsUnansweredType)).ToList())
                                {
                                    var correctAnswerTxt = string.Empty;
                                    var Incorrectques = false;
                                    if (ques.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && ques.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Any(r => r.Status == (int)StatusEnum.Active && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value))
                                    {
                                        Incorrectques = !((ques.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Select(s => s.Id).OrderBy(s => s)).SequenceEqual(ques.QuizAnswerStats.Select(r => r.AnswerId).OrderBy(s => s)));
                                        correctAnswerTxt = string.Join(",", ques.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Select(t => t.Option).Select(s => "'" + s + "'"));
                                    }
                                    else if (ques.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single)
                                    {
                                        Incorrectques = ques.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer.HasValue && ques.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer.Value ? false : true;
                                        correctAnswerTxt = ques.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(t => t.IsCorrectAnswer.HasValue ? t.IsCorrectAnswer.Value : false).Select(t => t.Option).FirstOrDefault();
                                    }

                                    if (Incorrectques)
                                    {
                                        correctanswerexplanation.Append("Question : " + ques.QuestionsInQuiz.Question + "<br/>");
                                        correctanswerexplanation.Append("Your Answer : " + string.Join(",", ques.QuizAnswerStats.Select(t => t.AnswerOptionsInQuizQuestions.Option).Select(s => "'" + s + "'")) + "<br/>");
                                        correctanswerexplanation.Append("Correct Answer : " + correctAnswerTxt + "<br/>");
                                        correctanswerexplanation.Append("Explanation for correct answer : " + QuizDetail.QuestionsInQuiz.FirstOrDefault().AliasTextForCorrectAnswer + "<br/>");
                                    }
                                }
                            }
                            else
                            {
                                Text = Text.Replace("%correctanswerexplanation%", string.Empty);
                            }
                        }

                        string Obj = string.IsNullOrEmpty(Text) ? string.Empty : Text.Replace("%fname%", (LeadInfo != null && !string.IsNullOrEmpty(LeadInfo.firstName)) ? LeadInfo.firstName : string.Empty)
                                                         .Replace("%lname%", (LeadInfo != null && !string.IsNullOrEmpty(LeadInfo.lastName)) ? LeadInfo.lastName : string.Empty)
                                                         .Replace("%phone%", (LeadInfo != null && !string.IsNullOrEmpty(LeadInfo.telephone)) ? LeadInfo.telephone : string.Empty)
                                                         .Replace("%email%", (LeadInfo != null && !string.IsNullOrEmpty(LeadInfo.firstName)) ? LeadInfo.email : string.Empty)
                                                         .Replace("%qname%", !IsTitle && QuizDetail != null ? QuizDetail.QuizTitle.Replace("%qname%", string.Empty) : string.Empty)
                                                         .Replace("%qlink%", QuizAttempt != null ? ((QuizAttempt.LeadUserId != null && QuizAttempt.WorkPackageInfo != null) ? GlobalSettings.webUrl.ToString() + "/quiz?Code=" + QuizAttempt.WorkPackageInfo.Quiz.PublishedCode + "&UserTypeId=2&UserId=" + QuizAttempt.WorkPackageInfo.LeadUserId + "&WorkPackageInfoId=" + QuizAttempt.WorkPackageInfo.Id
                                                                                                                                          : (QuizAttempt.RecruiterUserId != null ? GlobalSettings.elearningWebURL.ToString() + "/course/" + QuizAttempt.RecruiterUserId + "?Code=" + QuizDetail.Quiz.PublishedCode : GlobalSettings.webUrl.ToString() + "/quiz?Code=" + QuizDetail.Quiz.PublishedCode)) : string.Empty)
                                                         .Replace("%qendresult%", ShowScore && !string.IsNullOrEmpty(ResultScore) ? ResultScore.ToString() : string.Empty)
                                                         .Replace("%correctanswerexplanation%", correctanswerexplanation.ToString() == "Incorrect answer details:<br/>" ? string.Empty : correctanswerexplanation.ToString())
                                                         .Replace("%leadid%", (LeadInfo != null && !string.IsNullOrEmpty(LeadInfo.contactId)) ? LeadInfo.contactId : string.Empty);

                        IEnumerable<Db.VariablesDetails> variablesDetailList = new List<Db.VariablesDetails>();
                        if (QuizAttempt != null && QuizAttempt.ConfigurationDetailsId > 0 && QuizDetail != null)
                            variablesDetailList = UOWObj.VariablesDetailsRepository.Get(r => r.ConfigurationDetailsId == QuizAttempt.ConfigurationDetailsId);
                        else if (QuizAttempt != null && !string.IsNullOrEmpty(QuizAttempt.LeadUserId) && QuizDetail != null)
                            variablesDetailList = UOWObj.VariablesDetailsRepository.Get(r => r.VariableInQuiz.QuizId == QuizDetail.Id && r.LeadId == QuizAttempt.LeadUserId);

                        MatchCollection mcol = Regex.Matches(Obj, @"%\b\S+?\b%");


                        foreach (Match m in mcol)
                        {
                            var variablesDetailObj = variablesDetailList.FirstOrDefault(t => t.VariableInQuiz.Variables.Name == m.ToString().ToLower().Replace("%", string.Empty));
                            if (variablesDetailObj != null)
                            {
                                Obj = Obj.Replace(m.ToString(), variablesDetailObj.VariableValue);
                            }
                            else
                            {
                                Obj = Obj.Replace(m.ToString(), string.Empty);
                            }
                        }
                        return Obj;
                    }
                    else
                    {
                        return Text;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void AddPendingApi(string RequestTypeURL, string Authorization, string RequestData, string RequestType)
        {
            using (var UOWObj = new AutomationUnitOfWork())
            {
                var pendingApiQueueObj = new Db.PendingApiQueue
                {
                    CreatedOn = DateTime.UtcNow,
                    RequestTypeURL = RequestTypeURL,
                    Authorization = Authorization,
                    RequestData = RequestData,
                    RequestType = RequestType
                };
                UOWObj.PendingApiQueueRepository.Insert(pendingApiQueueObj);
                UOWObj.Save();
            }
        }

        #endregion

        #region Old Method ExtractDynamicMedia
        private Response.UserMediaClassification ExtractDynamicMedia(Db.MediaVariablesDetails mediaObj, Response.OWCLeadUserResponse.LeadUserResponse leadUserInfo, List<Response.UserMediaClassification> userMediaClassifications, IEnumerable<Db.UserTokens> dbUsers)
        {
            Response.UserMediaClassification newMedia = null;
            if (!string.IsNullOrWhiteSpace(mediaObj.MediaOwner) && !string.IsNullOrWhiteSpace(mediaObj.ProfileMedia) && userMediaClassifications != null && userMediaClassifications.Any())
            {
                if (mediaObj.MediaOwner == "CASE_OWNER" && leadUserInfo?.SourceOwnerId != 0 && userMediaClassifications.Any(a => a.UserToken == dbUsers.FirstOrDefault(b => b.BusinessUserId == leadUserInfo?.SourceOwnerId).OWCToken && a.Name == mediaObj.ProfileMedia))
                {
                    newMedia = userMediaClassifications.FirstOrDefault(a => a.UserToken == dbUsers.FirstOrDefault(b => b.BusinessUserId == leadUserInfo?.SourceOwnerId).OWCToken && a.Name == mediaObj.ProfileMedia);
                }
                else if (mediaObj.MediaOwner == "CONTACT_OWNER" && leadUserInfo?.ContactOwnerId != 0 && userMediaClassifications.Any(a => a.UserToken == dbUsers.FirstOrDefault(b => b.BusinessUserId == leadUserInfo?.ContactOwnerId).OWCToken && a.Name == mediaObj.ProfileMedia))
                {
                    newMedia = userMediaClassifications.FirstOrDefault(a => a.UserToken == dbUsers.FirstOrDefault(b => b.BusinessUserId == leadUserInfo?.ContactOwnerId).OWCToken && a.Name == mediaObj.ProfileMedia);
                }
            }
            return newMedia;
        }
        #endregion

        #region Old Method FetchingFirstQues
        private Db.QuizStats FetchingFirstQues(Db.QuizAttempts quizAttemptsObj, AutomationUnitOfWork UOWObj, DateTime currentDate, Db.QuizDetails quizDetails, Db.Quiz quizObj, CompanyModel companyInfo, string token, string urlAutomation)
        {
            Db.QuizStats quizStatsObj;
            quizAttemptsObj.IsViewed = true;
            quizAttemptsObj.CompanyId = quizDetails.CompanyId;

            UOWObj.QuizAttemptsRepository.Update(quizAttemptsObj);

            quizStatsObj = new Db.QuizStats();
            quizStatsObj.QuizAttemptId = quizAttemptsObj.Id;
            quizStatsObj.StartedOn = currentDate;

            UOWObj.QuizStatsRepository.Insert(quizStatsObj);
            UOWObj.Save();

            if (!string.IsNullOrEmpty(quizAttemptsObj.LeadUserId))
            {
                var UpdateQuizStatusObj = new LeadQuizStatus()
                {
                    AutomationTitle = quizDetails.QuizTitle,
                    CreatedDate = quizAttemptsObj.WorkPackageInfoId != null ? quizAttemptsObj.WorkPackageInfo.CreatedOn.Value.ToString("yyyy-MM-dd'T'HH:mm:ss") : quizAttemptsObj.CreatedOn.ToString("yyyy-MM-dd'T'HH:mm:ss"),
                    StartedDate = (quizAttemptsObj.QuizStats.Any() && quizAttemptsObj.QuizStats.FirstOrDefault().StartedOn != null) ? quizAttemptsObj.QuizStats.FirstOrDefault().StartedOn.ToString("yyyy-MM-dd'T'HH:mm:ss") : string.Empty,
                    AttemptDate = (quizAttemptsObj.QuizStats.Any() && quizAttemptsObj.QuizStats.FirstOrDefault().CompletedOn != null) ? quizAttemptsObj.QuizStats.FirstOrDefault().CompletedOn.Value.ToString("yyyy-MM-dd'T'HH:mm:ss") : string.Empty,
                    SourceId = quizAttemptsObj.ConfigurationDetailsId != null ? quizAttemptsObj.ConfigurationDetails.SourceId : string.Empty,
                    ContactId = quizAttemptsObj.LeadUserId,
                    ClientCode = quizObj.CompanyId != null ? companyInfo.ClientCode : string.Empty,
                    QuizId = quizDetails.ParentQuizId,
                    ConfigurationId = quizAttemptsObj.ConfigurationDetails != null ? quizAttemptsObj.ConfigurationDetails.ConfigurationId : string.Empty,
                    QuizType = quizObj.QuizType,
                    QuizStatus = "2",
                    RequestId = quizAttemptsObj.WorkPackageInfo.RequestId,
                    Results = new List<Result>()
                };
                try
                {
                    var apiSuccess = false;
                    var res = OWCHelper.UpdateQuizStatus(UpdateQuizStatusObj);
                    if (res != null)
                        apiSuccess = res.status == "true";

                    if (!apiSuccess)
                        AddPendingApi(urlAutomation, token, JsonConvert.SerializeObject(UpdateQuizStatusObj), "POST");
                }
                catch (Exception)
                {
                    AddPendingApi(urlAutomation, token, JsonConvert.SerializeObject(UpdateQuizStatusObj), "POST");
                }
            }

            return quizStatsObj;
        }
        #endregion

        #region Old Method  FetchCoverDetails
        private void FetchCoverDetails(QuizAnswerSubmit quizAnswerSubmit, Db.QuizAttempts quizAttemptsObj, AutomationUnitOfWork UOWObj, Db.QuizDetails quizDetails, Response.OWCLeadUserResponse.LeadUserResponse leadUserInfo, List<Response.UserMediaClassification> userMediaClassifications, IEnumerable<Db.UserTokens> dbUsers)
        {
            quizAnswerSubmit.LoadQuizDetails = true;
            var QuizAnsCoverDetails = quizAnswerSubmit.QuizCoverDetails;

            QuizAnsCoverDetails.QuizId = quizDetails.ParentQuizId;
            if ((quizDetails.EnableMediaFile && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.COVERDETAILS && r.ObjectId == quizDetails.Id)))
            {
                var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.COVERDETAILS && r.ObjectId == quizDetails.Id);
                QuizAnsCoverDetails.QuizCoverImage = mediaObj.ObjectValue;
                QuizAnsCoverDetails.ShowQuizCoverImage = true;
                QuizAnsCoverDetails.PublicIdForQuizCover = mediaObj.ObjectPublicId;

                var newMedia = ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers);
                if (newMedia != null)
                {
                    QuizAnsCoverDetails.QuizCoverImage = newMedia.MediaUrl;
                    QuizAnsCoverDetails.PublicIdForQuizCover = newMedia.MediaPublicId;
                }
            }
            else
            {
                QuizAnsCoverDetails.QuizCoverImage = quizDetails.QuizCoverImage;
                QuizAnsCoverDetails.ShowQuizCoverImage = quizDetails.ShowQuizCoverImage;
                QuizAnsCoverDetails.PublicIdForQuizCover = quizDetails.PublicId;
            }
            QuizAnsCoverDetails.QuizCoverImgXCoordinate = quizDetails.QuizCoverImgXCoordinate;
            QuizAnsCoverDetails.QuizCoverImgYCoordinate = quizDetails.QuizCoverImgYCoordinate;
            QuizAnsCoverDetails.QuizCoverImgHeight = quizDetails.QuizCoverImgHeight;
            QuizAnsCoverDetails.QuizCoverImgWidth = quizDetails.QuizCoverImgWidth;
            QuizAnsCoverDetails.QuizCoverImgAttributionLabel = quizDetails.QuizCoverImgAttributionLabel;
            QuizAnsCoverDetails.QuizCoverImgAltTag = quizDetails.QuizCoverImgAltTag;
            QuizAnsCoverDetails.QuizDescription = VariableLinking(quizDetails.QuizDescription, quizDetails, quizAttemptsObj, false, false, null);
            QuizAnsCoverDetails.QuizStartButtonText = quizDetails.StartButtonText;
            QuizAnsCoverDetails.AutoPlay = quizDetails.AutoPlay;
            QuizAnsCoverDetails.SecondsToApply = quizDetails.SecondsToApply ?? "0";
            QuizAnsCoverDetails.VideoFrameEnabled = quizDetails.VideoFrameEnabled ?? false;

            quizAttemptsObj.IsViewed = true;
            quizAttemptsObj.CompanyId = quizDetails.CompanyId;

            UOWObj.QuizAttemptsRepository.Update(quizAttemptsObj);
            UOWObj.Save();
        }
        #endregion

        #region Old Method NextQuesTypeQues
        private void NextQuesTypeQues(QuizAnswerSubmit quizAnswerSubmit, Db.QuizAttempts quizAttemptsObj, bool isLastQuestionStarted, Db.QuizDetails quizDetails, Response.OWCLeadUserResponse.LeadUserResponse leadUserInfo, List<Response.UserMediaClassification> userMediaClassifications, IEnumerable<Db.UserTokens> dbUsers, object nextQuestionObject)
        {

            var nextQuestionObj = (Db.QuestionsInQuiz)nextQuestionObject;
            quizAnswerSubmit.QuestionType = (int)BranchingLogicEnum.QUESTION;

            quizAnswerSubmit.QuestionDetails = new QuizQuestion();

            quizAnswerSubmit.QuestionDetails.QuestionId = nextQuestionObj.Id;
            quizAnswerSubmit.QuestionDetails.QuestionTitle = VariableLinking(nextQuestionObj.Question, quizDetails, quizAttemptsObj, false, false, null);
            quizAnswerSubmit.QuestionDetails.ShowTitle = nextQuestionObj.ShowTitle;

            if (nextQuestionObj.ShowQuestionImage.HasValue && nextQuestionObj.ShowQuestionImage.Value && nextQuestionObj.EnableMediaFile && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION && r.ObjectId == nextQuestionObj.Id))
            {
                var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION && r.ObjectId == nextQuestionObj.Id);
                quizAnswerSubmit.QuestionDetails.QuestionImage = mediaObj.ObjectValue;
                quizAnswerSubmit.QuestionDetails.PublicIdForQuestion = mediaObj.ObjectPublicId;

                var newMedia = ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers);
                if (newMedia != null)
                {
                    quizAnswerSubmit.QuestionDetails.QuestionImage = newMedia.MediaUrl;
                    quizAnswerSubmit.QuestionDetails.PublicIdForQuestion = newMedia.MediaPublicId;
                }
            }
            else
            {
                quizAnswerSubmit.QuestionDetails.QuestionImage = nextQuestionObj.ShowQuestionImage.HasValue && nextQuestionObj.ShowQuestionImage.Value ? nextQuestionObj.QuestionImage : string.Empty;
                quizAnswerSubmit.QuestionDetails.PublicIdForQuestion = nextQuestionObj.ShowQuestionImage.HasValue && nextQuestionObj.ShowQuestionImage.Value ? nextQuestionObj.PublicId : string.Empty;
            }

            quizAnswerSubmit.QuestionDetails.ShowQuestionImage = nextQuestionObj.ShowQuestionImage;
            quizAnswerSubmit.QuestionDetails.ShowAnswerImage = nextQuestionObj.ShowAnswerImage;
            quizAnswerSubmit.QuestionDetails.AnswerType = nextQuestionObj.AnswerType;
            quizAnswerSubmit.QuestionDetails.MinAnswer = nextQuestionObj.MinAnswer;
            quizAnswerSubmit.QuestionDetails.MaxAnswer = nextQuestionObj.MaxAnswer;
            quizAnswerSubmit.QuestionDetails.NextButtonColor = nextQuestionObj.NextButtonColor;
            quizAnswerSubmit.QuestionDetails.NextButtonText = nextQuestionObj.NextButtonText;
            quizAnswerSubmit.QuestionDetails.NextButtonTxtColor = nextQuestionObj.NextButtonTxtColor;
            quizAnswerSubmit.QuestionDetails.NextButtonTxtSize = nextQuestionObj.NextButtonTxtSize;
            quizAnswerSubmit.QuestionDetails.EnableNextButton = nextQuestionObj.EnableNextButton;
            quizAnswerSubmit.QuestionDetails.ViewPreviousQuestion = nextQuestionObj.ViewPreviousQuestion;
            quizAnswerSubmit.QuestionDetails.EditAnswer = nextQuestionObj.EditAnswer;
            quizAnswerSubmit.QuestionDetails.StartedOn = (isLastQuestionStarted && nextQuestionObj.QuizQuestionStats != null && nextQuestionObj.QuizQuestionStats.Any(r => r.Status == (int)StatusEnum.Active)) ? nextQuestionObj.QuizQuestionStats.FirstOrDefault(r => r.Status == (int)StatusEnum.Active).StartedOn : default(DateTime?);
            quizAnswerSubmit.QuestionDetails.TimerRequired = nextQuestionObj.TimerRequired;
            quizAnswerSubmit.QuestionDetails.Time = nextQuestionObj.Time;
            quizAnswerSubmit.QuestionDetails.AutoPlay = nextQuestionObj.AutoPlay;
            quizAnswerSubmit.QuestionDetails.SecondsToApply = nextQuestionObj.SecondsToApply ?? "0";
            quizAnswerSubmit.QuestionDetails.VideoFrameEnabled = nextQuestionObj.VideoFrameEnabled ?? false;
            quizAnswerSubmit.QuestionDetails.DisplayOrderForTitle = nextQuestionObj.DisplayOrderForTitle;
            quizAnswerSubmit.QuestionDetails.DisplayOrderForTitleImage = nextQuestionObj.DisplayOrderForTitleImage;
            quizAnswerSubmit.QuestionDetails.DisplayOrderForDescription = nextQuestionObj.DisplayOrderForDescription;
            quizAnswerSubmit.QuestionDetails.DisplayOrderForDescriptionImage = nextQuestionObj.DisplayOrderForDescriptionImage;
            quizAnswerSubmit.QuestionDetails.DisplayOrderForAnswer = nextQuestionObj.DisplayOrderForAnswer;
            quizAnswerSubmit.QuestionDetails.DisplayOrderForNextButton = nextQuestionObj.DisplayOrderForNextButton;
            quizAnswerSubmit.QuestionDetails.Description = nextQuestionObj.Description;
            quizAnswerSubmit.QuestionDetails.ShowDescription = nextQuestionObj.ShowDescription;
            quizAnswerSubmit.QuestionDetails.ShowDescriptionImage = nextQuestionObj.ShowDescriptionImage ?? false;
            quizAnswerSubmit.QuestionDetails.AutoPlayForDescription = nextQuestionObj.AutoPlayForDescription;
            quizAnswerSubmit.QuestionDetails.SecondsToApplyForDescription = nextQuestionObj.SecondsToApplyForDescription ?? "0";
            quizAnswerSubmit.QuestionDetails.DescVideoFrameEnabled = nextQuestionObj.DescVideoFrameEnabled ?? false;
            quizAnswerSubmit.QuestionDetails.EnableComment = nextQuestionObj.EnableComment;
            quizAnswerSubmit.QuestionDetails.AnswerStructureType = nextQuestionObj.AnswerStructureType;
        }
        #endregion

        #region Old Method PreviousTypeContent
        private void PreviousTypeContent(QuizAnswerSubmit quizAnswerSubmit, Db.QuizAttempts quizAttemptsObj, AutomationUnitOfWork UOWObj, Db.QuizDetails quizDetails, bool isQuesAndContentInSameTable, Response.OWCLeadUserResponse.LeadUserResponse leadUserInfo, List<Response.UserMediaClassification> userMediaClassifications, IEnumerable<Db.UserTokens> dbUsers, object previousQuestionObject)
        {
            var previousQuestionObj = (Db.ContentsInQuiz)previousQuestionObject;
            object previous = FetchPreviousQuestion(quizAttemptsObj, previousQuestionObj.Id, (int)BranchingLogicEnum.CONTENT, UOWObj, isQuesAndContentInSameTable);

            var previousContentStats = quizAttemptsObj.QuizObjectStats.FirstOrDefault(r => r.ObjectId == previousQuestionObj.Id && r.Status == (int)StatusEnum.Active);
            previousContentStats.Status = (int)StatusEnum.Inactive;
            UOWObj.QuizObjectStatsRepository.Update(previousContentStats);
            UOWObj.Save();

            quizAnswerSubmit.ContentDetails = new QuizContent();

            quizAnswerSubmit.ContentDetails.Id = previousQuestionObj.Id;
            quizAnswerSubmit.ContentDetails.ContentTitle = VariableLinking(previousQuestionObj.ContentTitle, quizDetails, quizAttemptsObj, false, false, null);
            quizAnswerSubmit.ContentDetails.ContentDescription = VariableLinking(previousQuestionObj.ContentDescription, quizDetails, quizAttemptsObj, false, false, null);
            quizAnswerSubmit.ContentDetails.ShowDescription = previousQuestionObj.ShowDescription;

            if ((previousQuestionObj.EnableMediaFileForTitle && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Title && r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == previousQuestionObj.Id)))
            {
                var mediaVariablesObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Title && r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == previousQuestionObj.Id);
                quizAnswerSubmit.ContentDetails.ContentTitleImage = mediaVariablesObj.ObjectValue;
                quizAnswerSubmit.ContentDetails.PublicIdForContentTitle = mediaVariablesObj.ObjectPublicId ?? string.Empty;

                var newMedia = ExtractDynamicMedia(mediaVariablesObj, leadUserInfo, userMediaClassifications, dbUsers);
                if (newMedia != null)
                {
                    quizAnswerSubmit.ContentDetails.ContentTitleImage = newMedia.MediaUrl;
                    quizAnswerSubmit.ContentDetails.PublicIdForContentTitle = newMedia.MediaPublicId;
                }
            }
            else
            {
                quizAnswerSubmit.ContentDetails.ContentTitleImage = previousQuestionObj.ContentTitleImage ?? string.Empty;
                quizAnswerSubmit.ContentDetails.PublicIdForContentTitle = previousQuestionObj.PublicIdForContentTitle ?? string.Empty;
            }

            quizAnswerSubmit.ContentDetails.ShowContentTitleImage = previousQuestionObj.ShowContentTitleImage.HasValue && previousQuestionObj.ShowContentTitleImage.Value ? previousQuestionObj.ShowContentTitleImage.Value : false;
            quizAnswerSubmit.ContentDetails.AliasTextForNextButton = previousQuestionObj.AliasTextForNextButton;
            quizAnswerSubmit.ContentDetails.EnableNextButton = previousQuestionObj.EnableNextButton;

            if (previousQuestionObj.EnableMediaFileForDescription && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Description && r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == previousQuestionObj.Id))
            {
                var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Description && r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == previousQuestionObj.Id);
                quizAnswerSubmit.ContentDetails.ContentDescriptionImage = mediaObj.ObjectValue ?? string.Empty;
                quizAnswerSubmit.ContentDetails.PublicIdForContentDescription = mediaObj.ObjectPublicId ?? string.Empty;

                var newMedia = ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers);
                if (newMedia != null)
                {
                    quizAnswerSubmit.ContentDetails.ContentDescriptionImage = newMedia.MediaUrl;
                    quizAnswerSubmit.ContentDetails.PublicIdForContentDescription = newMedia.MediaPublicId;
                }
            }
            else
            {
                quizAnswerSubmit.ContentDetails.ContentDescriptionImage = previousQuestionObj.ContentDescriptionImage ?? string.Empty;
                quizAnswerSubmit.ContentDetails.PublicIdForContentDescription = previousQuestionObj.PublicIdForContentDescription ?? string.Empty;
            }

            quizAnswerSubmit.ContentDetails.ShowContentDescriptionImage = previousQuestionObj.ShowContentDescriptionImage.HasValue && previousQuestionObj.ShowContentDescriptionImage.Value ? previousQuestionObj.ShowContentDescriptionImage.Value : false;
            quizAnswerSubmit.ContentDetails.ViewPreviousQuestion = previousQuestionObj.ViewPreviousQuestion;
            quizAnswerSubmit.ContentDetails.AutoPlay = previousQuestionObj.AutoPlay;
            quizAnswerSubmit.ContentDetails.SecondsToApply = previousQuestionObj.SecondsToApply ?? "0";
            quizAnswerSubmit.ContentDetails.VideoFrameEnabled = previousQuestionObj.VideoFrameEnabled ?? false;
            quizAnswerSubmit.ContentDetails.AutoPlayForDescription = previousQuestionObj.AutoPlayForDescription;
            quizAnswerSubmit.ContentDetails.SecondsToApplyForDescription = previousQuestionObj.SecondsToApplyForDescription ?? "0";
            quizAnswerSubmit.ContentDetails.DescVideoFrameEnabled = previousQuestionObj.DescVideoFrameEnabled ?? false;
            quizAnswerSubmit.ContentDetails.ShowTitle = previousQuestionObj.ShowTitle;
            quizAnswerSubmit.ContentDetails.DisplayOrderForTitle = previousQuestionObj.DisplayOrderForTitle;
            quizAnswerSubmit.ContentDetails.DisplayOrderForTitleImage = previousQuestionObj.DisplayOrderForTitleImage;
            quizAnswerSubmit.ContentDetails.DisplayOrderForDescription = previousQuestionObj.DisplayOrderForDescription;
            quizAnswerSubmit.ContentDetails.DisplayOrderForDescriptionImage = previousQuestionObj.DisplayOrderForDescriptionImage;
            quizAnswerSubmit.ContentDetails.DisplayOrderForNextButton = previousQuestionObj.DisplayOrderForNextButton;

            if (previous != null)
            {
                if (previous.GetType().BaseType.Name == "QuestionsInQuiz")
                {
                    if (((Db.QuestionsInQuiz)previous).Type == (int)BranchingLogicEnum.QUESTION)
                        quizAnswerSubmit.IsBackButtonEnable = (((Db.QuestionsInQuiz)previous).RevealCorrectAnswer.HasValue && ((Db.QuestionsInQuiz)previous).RevealCorrectAnswer.Value) ? false : (((Db.QuestionsInQuiz)previous).ViewPreviousQuestion || ((Db.QuestionsInQuiz)previous).EditAnswer);
                    if (((Db.QuestionsInQuiz)previous).Type == (int)BranchingLogicEnum.CONTENT)
                        quizAnswerSubmit.IsBackButtonEnable = ((Db.QuestionsInQuiz)previous).ViewPreviousQuestion;
                }
                if (previous.GetType().BaseType.Name == "ContentsInQuiz")
                    quizAnswerSubmit.IsBackButtonEnable = ((Db.ContentsInQuiz)previous).ViewPreviousQuestion;
            }
        }
        #endregion

        #region Old Unused methods
        private Db.QuizQuestionStats PreviousTypeQuestion(QuizAnswerSubmit quizAnswerSubmit, Db.QuizAttempts quizAttemptsObj, AutomationUnitOfWork UOWObj, DateTime currentDate, bool isLastQuestionStarted, Db.QuizDetails quizDetails, bool isQuesAndContentInSameTable, Db.QuizQuestionStats quizQuestionStatsObj, Response.OWCLeadUserResponse.LeadUserResponse leadUserInfo, List<Response.UserMediaClassification> userMediaClassifications, IEnumerable<Db.UserTokens> dbUsers, object previousQuestionObj)
        {
            if (((Db.QuestionsInQuiz)previousQuestionObj).Type == (int)BranchingLogicEnum.QUESTION)
            {
                object previous = FetchPreviousQuestion(quizAttemptsObj, ((Db.QuestionsInQuiz)previousQuestionObj).Id, (int)BranchingLogicEnum.QUESTION, UOWObj, isQuesAndContentInSameTable);

                var previouszQuestionStats = quizAttemptsObj.QuizQuestionStats.FirstOrDefault(r => r.QuestionId == ((Db.QuestionsInQuiz)previousQuestionObj).Id && r.Status == (int)StatusEnum.Active);

                previouszQuestionStats.Status = (int)StatusEnum.Inactive;
                UOWObj.QuizQuestionStatsRepository.Update(previouszQuestionStats);
                UOWObj.Save();

                quizQuestionStatsObj = new Db.QuizQuestionStats();
                quizQuestionStatsObj.QuizAttemptId = quizAttemptsObj.Id;
                quizQuestionStatsObj.QuestionId = ((Db.QuestionsInQuiz)previousQuestionObj).Id;
                quizQuestionStatsObj.StartedOn = currentDate;
                quizQuestionStatsObj.Status = (int)StatusEnum.Active;

                UOWObj.QuizQuestionStatsRepository.Insert(quizQuestionStatsObj);
                UOWObj.Save();

                quizAnswerSubmit.QuestionDetails = new QuizQuestion();

                quizAnswerSubmit.QuestionDetails.QuestionId = ((Db.QuestionsInQuiz)previousQuestionObj).Id;
                quizAnswerSubmit.QuestionDetails.QuestionTitle = VariableLinking(((Db.QuestionsInQuiz)previousQuestionObj).Question, quizDetails, quizAttemptsObj, false, false, null);
                quizAnswerSubmit.QuestionDetails.ShowTitle = ((Db.QuestionsInQuiz)previousQuestionObj).ShowTitle;

                if (((Db.QuestionsInQuiz)previousQuestionObj).ShowQuestionImage.HasValue && ((Db.QuestionsInQuiz)previousQuestionObj).ShowQuestionImage.Value && ((Db.QuestionsInQuiz)previousQuestionObj).EnableMediaFile && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION && r.ObjectId == ((Db.QuestionsInQuiz)previousQuestionObj).Id))
                {
                    var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION && r.ObjectId == ((Db.QuestionsInQuiz)previousQuestionObj).Id);
                    quizAnswerSubmit.QuestionDetails.QuestionImage = mediaObj.ObjectValue;
                    quizAnswerSubmit.QuestionDetails.PublicIdForQuestion = mediaObj.ObjectPublicId;

                    var newMedia = ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers);
                    if (newMedia != null)
                    {
                        quizAnswerSubmit.QuestionDetails.QuestionImage = newMedia.MediaUrl;
                        quizAnswerSubmit.QuestionDetails.PublicIdForQuestion = newMedia.MediaPublicId;
                    }
                }
                else
                {
                    quizAnswerSubmit.QuestionDetails.QuestionImage = ((Db.QuestionsInQuiz)previousQuestionObj).ShowQuestionImage.HasValue && ((Db.QuestionsInQuiz)previousQuestionObj).ShowQuestionImage.Value ? ((Db.QuestionsInQuiz)previousQuestionObj).QuestionImage : string.Empty;
                    quizAnswerSubmit.QuestionDetails.PublicIdForQuestion = ((Db.QuestionsInQuiz)previousQuestionObj).ShowQuestionImage.HasValue && ((Db.QuestionsInQuiz)previousQuestionObj).ShowQuestionImage.Value ? ((Db.QuestionsInQuiz)previousQuestionObj).PublicId : string.Empty;
                }

                quizAnswerSubmit.QuestionDetails.ShowQuestionImage = ((Db.QuestionsInQuiz)previousQuestionObj).ShowQuestionImage;
                quizAnswerSubmit.QuestionDetails.ShowAnswerImage = ((Db.QuestionsInQuiz)previousQuestionObj).ShowAnswerImage;
                quizAnswerSubmit.QuestionDetails.AnswerType = ((Db.QuestionsInQuiz)previousQuestionObj).AnswerType;
                quizAnswerSubmit.QuestionDetails.NextButtonColor = ((Db.QuestionsInQuiz)previousQuestionObj).NextButtonColor;
                quizAnswerSubmit.QuestionDetails.NextButtonText = ((Db.QuestionsInQuiz)previousQuestionObj).NextButtonText;
                quizAnswerSubmit.QuestionDetails.NextButtonTxtColor = ((Db.QuestionsInQuiz)previousQuestionObj).NextButtonTxtColor;
                quizAnswerSubmit.QuestionDetails.NextButtonTxtSize = ((Db.QuestionsInQuiz)previousQuestionObj).NextButtonTxtSize;
                quizAnswerSubmit.QuestionDetails.EnableNextButton = ((Db.QuestionsInQuiz)previousQuestionObj).EnableNextButton;
                quizAnswerSubmit.QuestionDetails.MinAnswer = ((Db.QuestionsInQuiz)previousQuestionObj).MinAnswer;
                quizAnswerSubmit.QuestionDetails.MaxAnswer = ((Db.QuestionsInQuiz)previousQuestionObj).MaxAnswer;
                quizAnswerSubmit.QuestionDetails.ViewPreviousQuestion = ((Db.QuestionsInQuiz)previousQuestionObj).ViewPreviousQuestion;
                quizAnswerSubmit.QuestionDetails.EditAnswer = ((Db.QuestionsInQuiz)previousQuestionObj).EditAnswer;
                quizAnswerSubmit.QuestionDetails.StartedOn = (isLastQuestionStarted && ((Db.QuestionsInQuiz)previousQuestionObj).QuizQuestionStats != null && ((Db.QuestionsInQuiz)previousQuestionObj).QuizQuestionStats.Any(r => r.Status == (int)StatusEnum.Active)) ? ((Db.QuestionsInQuiz)previousQuestionObj).QuizQuestionStats.FirstOrDefault(r => r.Status == (int)StatusEnum.Active).StartedOn : default(DateTime?);
                quizAnswerSubmit.QuestionDetails.TimerRequired = ((Db.QuestionsInQuiz)previousQuestionObj).TimerRequired;
                quizAnswerSubmit.QuestionDetails.Time = ((Db.QuestionsInQuiz)previousQuestionObj).Time;
                quizAnswerSubmit.QuestionDetails.AutoPlay = ((Db.QuestionsInQuiz)previousQuestionObj).AutoPlay;
                quizAnswerSubmit.QuestionDetails.SecondsToApply = ((Db.QuestionsInQuiz)previousQuestionObj).SecondsToApply ?? "0";
                quizAnswerSubmit.QuestionDetails.VideoFrameEnabled = ((Db.QuestionsInQuiz)previousQuestionObj).VideoFrameEnabled ?? false;
                quizAnswerSubmit.QuestionDetails.DisplayOrderForTitle = ((Db.QuestionsInQuiz)previousQuestionObj).DisplayOrderForTitle;
                quizAnswerSubmit.QuestionDetails.DisplayOrderForTitleImage = ((Db.QuestionsInQuiz)previousQuestionObj).DisplayOrderForTitleImage;
                quizAnswerSubmit.QuestionDetails.DisplayOrderForDescription = ((Db.QuestionsInQuiz)previousQuestionObj).DisplayOrderForDescription;
                quizAnswerSubmit.QuestionDetails.DisplayOrderForDescriptionImage = ((Db.QuestionsInQuiz)previousQuestionObj).DisplayOrderForDescriptionImage;
                quizAnswerSubmit.QuestionDetails.DisplayOrderForAnswer = ((Db.QuestionsInQuiz)previousQuestionObj).DisplayOrderForAnswer;
                quizAnswerSubmit.QuestionDetails.DisplayOrderForNextButton = ((Db.QuestionsInQuiz)previousQuestionObj).DisplayOrderForNextButton;
                quizAnswerSubmit.QuestionDetails.Description = ((Db.QuestionsInQuiz)previousQuestionObj).Description;
                quizAnswerSubmit.QuestionDetails.ShowDescription = ((Db.QuestionsInQuiz)previousQuestionObj).ShowDescription;
                quizAnswerSubmit.QuestionDetails.ShowDescriptionImage = ((Db.QuestionsInQuiz)previousQuestionObj).ShowDescriptionImage ?? false;
                quizAnswerSubmit.QuestionDetails.AutoPlayForDescription = ((Db.QuestionsInQuiz)previousQuestionObj).AutoPlayForDescription;
                quizAnswerSubmit.QuestionDetails.SecondsToApplyForDescription = ((Db.QuestionsInQuiz)previousQuestionObj).SecondsToApplyForDescription ?? "0";
                quizAnswerSubmit.QuestionDetails.DescVideoFrameEnabled = ((Db.QuestionsInQuiz)previousQuestionObj).DescVideoFrameEnabled ?? false;
                quizAnswerSubmit.QuestionDetails.EnableComment = ((Db.QuestionsInQuiz)previousQuestionObj).EnableComment;
                quizAnswerSubmit.QuestionDetails.AnswerStructureType = ((Db.QuestionsInQuiz)previousQuestionObj).AnswerStructureType;

                if (((Db.QuestionsInQuiz)previousQuestionObj).EnableMediaFileForDescription && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Description && r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION && r.ObjectId == ((Db.QuestionsInQuiz)previousQuestionObj).Id))
                {
                    var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Description && r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION && r.ObjectId == ((Db.QuestionsInQuiz)previousQuestionObj).Id);
                    quizAnswerSubmit.QuestionDetails.DescriptionImage = mediaObj.ObjectValue ?? string.Empty;
                    quizAnswerSubmit.QuestionDetails.PublicIdForDescription = mediaObj.ObjectPublicId ?? string.Empty;

                    var newMedia = ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers);
                    if (newMedia != null)
                    {
                        quizAnswerSubmit.QuestionDetails.DescriptionImage = newMedia.MediaUrl;
                        quizAnswerSubmit.QuestionDetails.PublicIdForDescription = newMedia.MediaPublicId;
                    }
                }
                else
                {
                    quizAnswerSubmit.QuestionDetails.DescriptionImage = ((Db.QuestionsInQuiz)previousQuestionObj).DescriptionImage ?? string.Empty;
                    quizAnswerSubmit.QuestionDetails.PublicIdForDescription = ((Db.QuestionsInQuiz)previousQuestionObj).PublicIdForDescription ?? string.Empty;
                }

                if (previous != null)
                {
                    if (previous.GetType().BaseType.Name == "ContentsInQuiz")
                        quizAnswerSubmit.IsBackButtonEnable = ((Db.ContentsInQuiz)previous).ViewPreviousQuestion;

                    else if (previous.GetType().BaseType.Name == "QuestionsInQuiz")
                    {
                        if (((Db.QuestionsInQuiz)previous).Type == (int)BranchingLogicEnum.QUESTION)
                            quizAnswerSubmit.IsBackButtonEnable = (((Db.QuestionsInQuiz)previous).RevealCorrectAnswer.HasValue && ((Db.QuestionsInQuiz)previous).RevealCorrectAnswer.Value) ? false : (((Db.QuestionsInQuiz)previous).ViewPreviousQuestion || ((Db.QuestionsInQuiz)previous).EditAnswer);
                        else if (((Db.QuestionsInQuiz)previous).Type == (int)BranchingLogicEnum.CONTENT)
                            quizAnswerSubmit.IsBackButtonEnable = ((Db.QuestionsInQuiz)previous).ViewPreviousQuestion;
                    }
                }

                #region Previous question SubmittedAnswer

                var attemptedAnswerIds = quizAttemptsObj.QuizQuestionStats.Any(r => r.QuestionId == ((Db.QuestionsInQuiz)previousQuestionObj).Id && r.CompletedOn.HasValue)
                    ? quizAttemptsObj.QuizQuestionStats.LastOrDefault(r => r.QuestionId == ((Db.QuestionsInQuiz)previousQuestionObj).Id && r.CompletedOn.HasValue).QuizAnswerStats.Select(r => r.AnswerId).ToList()
                    : quizAttemptsObj.QuizQuestionStats.LastOrDefault(r => r.QuestionId == ((Db.QuestionsInQuiz)previousQuestionObj).Id).QuizAnswerStats.Select(r => r.AnswerId).ToList();

                var submittedAnswerOption = ((Db.QuestionsInQuiz)previousQuestionObj).AnswerOptionsInQuizQuestions.Where(r => attemptedAnswerIds.Contains(r.Id));

                quizAnswerSubmit.PreviousQuestionSubmittedAnswer = new QuizAnswerSubmit.SubmittedAnswerResult();

                quizAnswerSubmit.PreviousQuestionSubmittedAnswer.AliasTextForCorrect = ((Db.QuestionsInQuiz)previousQuestionObj).AliasTextForCorrect;
                quizAnswerSubmit.PreviousQuestionSubmittedAnswer.AliasTextForIncorrect = ((Db.QuestionsInQuiz)previousQuestionObj).AliasTextForIncorrect;
                quizAnswerSubmit.PreviousQuestionSubmittedAnswer.AliasTextForYourAnswer = ((Db.QuestionsInQuiz)previousQuestionObj).AliasTextForYourAnswer;
                quizAnswerSubmit.PreviousQuestionSubmittedAnswer.AliasTextForCorrectAnswer = ((Db.QuestionsInQuiz)previousQuestionObj).AliasTextForCorrectAnswer;
                quizAnswerSubmit.PreviousQuestionSubmittedAnswer.AliasTextForExplanation = ((Db.QuestionsInQuiz)previousQuestionObj).AliasTextForExplanation;
                quizAnswerSubmit.PreviousQuestionSubmittedAnswer.AliasTextForNextButton = ((Db.QuestionsInQuiz)previousQuestionObj).AliasTextForNextButton;
                quizAnswerSubmit.PreviousQuestionSubmittedAnswer.CorrectAnswerDescription = ((Db.QuestionsInQuiz)previousQuestionObj).CorrectAnswerDescription;

                quizAnswerSubmit.PreviousQuestionSubmittedAnswer.SubmittedAnswerDetails = new List<QuizAnswerSubmit.SubmittedAnswerResult.SubmittedAnswer>();

                foreach (var submittedAnswerOptionObj in submittedAnswerOption.Where(r => r.Status == (int)StatusEnum.Active))
                {
                    if (submittedAnswerOptionObj.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple || submittedAnswerOptionObj.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single)
                    {
                        quizAnswerSubmit.PreviousQuestionSubmittedAnswer.SubmittedAnswerDetails.Add(new QuizAnswerSubmit.SubmittedAnswerResult.SubmittedAnswer
                        {
                            AnswerId = submittedAnswerOptionObj.Id,
                            SubmittedAnswerTitle = VariableLinking(submittedAnswerOptionObj.Option, quizDetails, quizAttemptsObj, false, false, null),
                            SubmittedAnswerImage = ((Db.QuestionsInQuiz)previousQuestionObj).ShowAnswerImage.HasValue && ((Db.QuestionsInQuiz)previousQuestionObj).ShowAnswerImage.Value ? submittedAnswerOptionObj.OptionImage : string.Empty,
                            PublicIdForSubmittedAnswer = ((Db.QuestionsInQuiz)previousQuestionObj).ShowAnswerImage.HasValue && ((Db.QuestionsInQuiz)previousQuestionObj).ShowAnswerImage.Value ? submittedAnswerOptionObj.PublicId : string.Empty,
                            AutoPlay = submittedAnswerOptionObj.AutoPlay,
                            SecondsToApply = submittedAnswerOptionObj.SecondsToApply,
                            VideoFrameEnabled = submittedAnswerOptionObj.VideoFrameEnabled
                        });
                    }
                    else if (submittedAnswerOptionObj.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.FullAddress)
                    {
                        if (submittedAnswerOptionObj.QuizAnswerStats.FirstOrDefault().SubAnswerTypeId == (int)SubAnswerTypeEnum.PostCode)
                        {
                            quizAnswerSubmit.PreviousQuestionSubmittedAnswer.SubmittedAnswerDetails.Add(new QuizAnswerSubmit.SubmittedAnswerResult.SubmittedAnswer
                            {
                                AnswerId = submittedAnswerOptionObj.Id,
                                SubmittedAnswerTitle = submittedAnswerOptionObj.QuizAnswerStats.FirstOrDefault().AnswerText,
                                SubmittedSecondaryAnswerTitle = submittedAnswerOptionObj.QuizAnswerStats.FirstOrDefault().AnswerSecondaryText,
                                SubmittedAnswerImage = ((Db.QuestionsInQuiz)previousQuestionObj).ShowAnswerImage.HasValue && ((Db.QuestionsInQuiz)previousQuestionObj).ShowAnswerImage.Value ? submittedAnswerOptionObj.OptionImage : string.Empty,
                                PublicIdForSubmittedAnswer = ((Db.QuestionsInQuiz)previousQuestionObj).ShowAnswerImage.HasValue && ((Db.QuestionsInQuiz)previousQuestionObj).ShowAnswerImage.Value ? submittedAnswerOptionObj.PublicId : string.Empty,
                                AutoPlay = submittedAnswerOptionObj.AutoPlay,
                                SecondsToApply = submittedAnswerOptionObj.SecondsToApply,
                                VideoFrameEnabled = submittedAnswerOptionObj.VideoFrameEnabled
                            });
                        }

                        if (submittedAnswerOptionObj.QuizAnswerStats.FirstOrDefault().SubAnswerTypeId == (int)SubAnswerTypeEnum.HouseNumber)
                        {
                            quizAnswerSubmit.PreviousQuestionSubmittedAnswer.SubmittedAnswerDetails.Add(new QuizAnswerSubmit.SubmittedAnswerResult.SubmittedAnswer
                            {
                                AnswerId = submittedAnswerOptionObj.Id,
                                SubmittedAnswerTitle = submittedAnswerOptionObj.QuizAnswerStats.FirstOrDefault(r => r.SubAnswerTypeId == (int)SubAnswerTypeEnum.HouseNumber).AnswerText,
                                SubmittedAnswerImage = ((Db.QuestionsInQuiz)previousQuestionObj).ShowAnswerImage.HasValue && ((Db.QuestionsInQuiz)previousQuestionObj).ShowAnswerImage.Value ? submittedAnswerOptionObj.OptionImage : string.Empty,
                                PublicIdForSubmittedAnswer = ((Db.QuestionsInQuiz)previousQuestionObj).ShowAnswerImage.HasValue && ((Db.QuestionsInQuiz)previousQuestionObj).ShowAnswerImage.Value ? submittedAnswerOptionObj.PublicId : string.Empty,
                                AutoPlay = submittedAnswerOptionObj.AutoPlay,
                                SecondsToApply = submittedAnswerOptionObj.SecondsToApply,
                                VideoFrameEnabled = submittedAnswerOptionObj.VideoFrameEnabled
                            });

                            quizAnswerSubmit.PreviousQuestionSubmittedAnswer.SubmittedAnswerDetails.Add(new QuizAnswerSubmit.SubmittedAnswerResult.SubmittedAnswer
                            {
                                AnswerId = submittedAnswerOptionObj.Id,
                                SubmittedAnswerTitle = submittedAnswerOptionObj.QuizAnswerStats.FirstOrDefault(r => r.SubAnswerTypeId == (int)SubAnswerTypeEnum.Address).AnswerText,
                                SubmittedAnswerImage = ((Db.QuestionsInQuiz)previousQuestionObj).ShowAnswerImage.HasValue && ((Db.QuestionsInQuiz)previousQuestionObj).ShowAnswerImage.Value ? submittedAnswerOptionObj.OptionImage : string.Empty,
                                PublicIdForSubmittedAnswer = ((Db.QuestionsInQuiz)previousQuestionObj).ShowAnswerImage.HasValue && ((Db.QuestionsInQuiz)previousQuestionObj).ShowAnswerImage.Value ? submittedAnswerOptionObj.PublicId : string.Empty,
                                AutoPlay = submittedAnswerOptionObj.AutoPlay,
                                SecondsToApply = submittedAnswerOptionObj.SecondsToApply,
                                VideoFrameEnabled = submittedAnswerOptionObj.VideoFrameEnabled
                            });
                        }
                    }
                    else if (submittedAnswerOptionObj.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.RatingEmoji || submittedAnswerOptionObj.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.RatingStarts)
                    {
                        quizAnswerSubmit.PreviousQuestionSubmittedAnswer.SubmittedAnswerDetails.Add(new QuizAnswerSubmit.SubmittedAnswerResult.SubmittedAnswer
                        {
                            AnswerId = submittedAnswerOptionObj.Id,
                            SubmittedAnswerTitle = submittedAnswerOptionObj.QuizAnswerStats.FirstOrDefault().AnswerText,
                            SubmittedAnswerImage = ((Db.QuestionsInQuiz)previousQuestionObj).ShowAnswerImage.HasValue && ((Db.QuestionsInQuiz)previousQuestionObj).ShowAnswerImage.Value ? submittedAnswerOptionObj.OptionImage : string.Empty,
                            PublicIdForSubmittedAnswer = ((Db.QuestionsInQuiz)previousQuestionObj).ShowAnswerImage.HasValue && ((Db.QuestionsInQuiz)previousQuestionObj).ShowAnswerImage.Value ? submittedAnswerOptionObj.PublicId : string.Empty,
                            AutoPlay = submittedAnswerOptionObj.AutoPlay,
                            SecondsToApply = submittedAnswerOptionObj.SecondsToApply,
                            VideoFrameEnabled = submittedAnswerOptionObj.VideoFrameEnabled,
                            Comment = submittedAnswerOptionObj.QuizAnswerStats.FirstOrDefault().Comment,
                            //for Rating type question
                            OptionTextforRatingOne = submittedAnswerOptionObj.OptionTextforRatingOne,
                            OptionTextforRatingTwo = submittedAnswerOptionObj.OptionTextforRatingTwo,
                            OptionTextforRatingThree = submittedAnswerOptionObj.OptionTextforRatingThree,
                            OptionTextforRatingFour = submittedAnswerOptionObj.OptionTextforRatingFour,
                            OptionTextforRatingFive = submittedAnswerOptionObj.OptionTextforRatingFive
                        });
                    }
                    else
                    {
                        quizAnswerSubmit.PreviousQuestionSubmittedAnswer.SubmittedAnswerDetails.Add(new QuizAnswerSubmit.SubmittedAnswerResult.SubmittedAnswer
                        {
                            AnswerId = submittedAnswerOptionObj.Id,
                            SubmittedAnswerTitle = submittedAnswerOptionObj.QuizAnswerStats.FirstOrDefault().AnswerText,
                            SubmittedSecondaryAnswerTitle = submittedAnswerOptionObj.QuizAnswerStats.FirstOrDefault().AnswerSecondaryText,
                            SubmittedAnswerImage = ((Db.QuestionsInQuiz)previousQuestionObj).ShowAnswerImage.HasValue && ((Db.QuestionsInQuiz)previousQuestionObj).ShowAnswerImage.Value ? submittedAnswerOptionObj.OptionImage : string.Empty,
                            PublicIdForSubmittedAnswer = ((Db.QuestionsInQuiz)previousQuestionObj).ShowAnswerImage.HasValue && ((Db.QuestionsInQuiz)previousQuestionObj).ShowAnswerImage.Value ? submittedAnswerOptionObj.PublicId : string.Empty,
                            AutoPlay = submittedAnswerOptionObj.AutoPlay,
                            SecondsToApply = submittedAnswerOptionObj.SecondsToApply,
                            VideoFrameEnabled = submittedAnswerOptionObj.VideoFrameEnabled,
                            Comment = submittedAnswerOptionObj.QuizAnswerStats.FirstOrDefault().Comment
                        });
                    }
                }

                quizAnswerSubmit.PreviousQuestionSubmittedAnswer.CorrectAnswerDetails = new List<QuizAnswerSubmit.SubmittedAnswerResult.CorrectAnswer>();

                #endregion

                quizAnswerSubmit.QuestionDetails.AnswerList = new List<AnswerOptionInQuestion>();

                foreach (var ans in ((Db.QuestionsInQuiz)previousQuestionObj).AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active).OrderBy(r => r.DisplayOrder))
                {
                    var answerImage = string.Empty;
                    var publicIdForAnswer = string.Empty;

                    if (((Db.QuestionsInQuiz)previousQuestionObj).ShowAnswerImage.HasValue && ((Db.QuestionsInQuiz)previousQuestionObj).ShowAnswerImage.Value && ans.EnableMediaFile && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.ObjectId == ans.Id))
                    {
                        var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.ObjectId == ans.Id);
                        answerImage = mediaObj.ObjectValue;
                        publicIdForAnswer = mediaObj.ObjectPublicId;

                        var newMedia = ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers);
                        if (newMedia != null)
                        {
                            answerImage = newMedia.MediaUrl;
                            publicIdForAnswer = newMedia.MediaPublicId;
                        }
                    }
                    else
                    {
                        answerImage = ((Db.QuestionsInQuiz)previousQuestionObj).ShowAnswerImage.HasValue && ((Db.QuestionsInQuiz)previousQuestionObj).ShowAnswerImage.Value ? ans.OptionImage : string.Empty;
                        publicIdForAnswer = ((Db.QuestionsInQuiz)previousQuestionObj).ShowAnswerImage.HasValue && ((Db.QuestionsInQuiz)previousQuestionObj).ShowAnswerImage.Value ? ans.PublicId : string.Empty;
                    }

                    quizAnswerSubmit.QuestionDetails.AnswerList.Add(new AnswerOptionInQuestion
                    {
                        AnswerId = ans.Id,
                        AssociatedScore = ans.AssociatedScore,
                        AnswerText = VariableLinking(ans.Option, quizDetails, quizAttemptsObj, false, false, null),
                        AnswerImage = answerImage,
                        PublicIdForAnswer = publicIdForAnswer,
                        IsCorrectAnswer = false,
                        DisplayOrder = ans.DisplayOrder,
                        IsUnansweredType = ans.IsUnansweredType,
                        AutoPlay = ans.AutoPlay,
                        SecondsToApply = ans.SecondsToApply,
                        VideoFrameEnabled = ans.VideoFrameEnabled,
                        ListValues = ans.ListValues,
                        //for Rating type question
                        OptionTextforRatingOne = ans.OptionTextforRatingOne,
                        OptionTextforRatingTwo = ans.OptionTextforRatingTwo,
                        OptionTextforRatingThree = ans.OptionTextforRatingThree,
                        OptionTextforRatingFour = ans.OptionTextforRatingFour,
                        OptionTextforRatingFive = ans.OptionTextforRatingFive
                    });
                }
            }
            else if (((Db.QuestionsInQuiz)previousQuestionObj).Type == (int)BranchingLogicEnum.CONTENT)
            {
                object previous = FetchPreviousQuestion(quizAttemptsObj, ((Db.QuestionsInQuiz)previousQuestionObj).Id, (int)BranchingLogicEnum.CONTENT, UOWObj, isQuesAndContentInSameTable);

                var previouszQuestionStats = quizAttemptsObj.QuizQuestionStats.FirstOrDefault(r => r.QuestionId == ((Db.QuestionsInQuiz)previousQuestionObj).Id && r.Status == (int)StatusEnum.Active);

                previouszQuestionStats.Status = (int)StatusEnum.Inactive;
                UOWObj.QuizQuestionStatsRepository.Update(previouszQuestionStats);
                UOWObj.Save();

                quizQuestionStatsObj = new Db.QuizQuestionStats();
                quizQuestionStatsObj.QuizAttemptId = quizAttemptsObj.Id;
                quizQuestionStatsObj.QuestionId = ((Db.QuestionsInQuiz)previousQuestionObj).Id;
                quizQuestionStatsObj.StartedOn = currentDate;
                quizQuestionStatsObj.Status = (int)StatusEnum.Active;

                UOWObj.QuizQuestionStatsRepository.Insert(quizQuestionStatsObj);
                UOWObj.Save();

                quizAnswerSubmit.ContentDetails = new QuizContent();

                quizAnswerSubmit.ContentDetails.Id = ((Db.QuestionsInQuiz)previousQuestionObj).Id;
                quizAnswerSubmit.ContentDetails.ContentTitle = VariableLinking(((Db.QuestionsInQuiz)previousQuestionObj).Question, quizDetails, quizAttemptsObj, false, false, null);
                quizAnswerSubmit.ContentDetails.ContentDescription = VariableLinking(((Db.QuestionsInQuiz)previousQuestionObj).Description, quizDetails, quizAttemptsObj, false, false, null);
                quizAnswerSubmit.ContentDetails.ShowDescription = ((Db.QuestionsInQuiz)previousQuestionObj).ShowDescription;

                if ((((Db.QuestionsInQuiz)previousQuestionObj).EnableMediaFile && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Title && r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == ((Db.QuestionsInQuiz)previousQuestionObj).Id)))
                {
                    var mediaVariablesObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Title && r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == ((Db.QuestionsInQuiz)previousQuestionObj).Id);
                    quizAnswerSubmit.ContentDetails.ContentTitleImage = mediaVariablesObj.ObjectValue;
                    quizAnswerSubmit.ContentDetails.PublicIdForContentTitle = mediaVariablesObj.ObjectPublicId ?? string.Empty;

                    var newMedia = ExtractDynamicMedia(mediaVariablesObj, leadUserInfo, userMediaClassifications, dbUsers);
                    if (newMedia != null)
                    {
                        quizAnswerSubmit.ContentDetails.ContentTitleImage = newMedia.MediaUrl;
                        quizAnswerSubmit.ContentDetails.PublicIdForContentTitle = newMedia.MediaPublicId;
                    }
                }
                else
                {
                    quizAnswerSubmit.ContentDetails.ContentTitleImage = ((Db.QuestionsInQuiz)previousQuestionObj).QuestionImage ?? string.Empty;
                    quizAnswerSubmit.ContentDetails.PublicIdForContentTitle = ((Db.QuestionsInQuiz)previousQuestionObj).PublicId ?? string.Empty;
                }

                quizAnswerSubmit.ContentDetails.ShowContentTitleImage = ((Db.QuestionsInQuiz)previousQuestionObj).ShowQuestionImage.HasValue && ((Db.QuestionsInQuiz)previousQuestionObj).ShowQuestionImage.Value ? ((Db.QuestionsInQuiz)previousQuestionObj).ShowQuestionImage.Value : false;
                quizAnswerSubmit.ContentDetails.AliasTextForNextButton = ((Db.QuestionsInQuiz)previousQuestionObj).NextButtonText;
                quizAnswerSubmit.ContentDetails.EnableNextButton = ((Db.QuestionsInQuiz)previousQuestionObj).EnableNextButton;

                if (((Db.QuestionsInQuiz)previousQuestionObj).EnableMediaFileForDescription && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Description && r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == ((Db.QuestionsInQuiz)previousQuestionObj).Id))
                {
                    var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttemptsObj.ConfigurationDetailsId && r.Type == (int)ImageTypeEnum.Description && r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == ((Db.QuestionsInQuiz)previousQuestionObj).Id);
                    quizAnswerSubmit.ContentDetails.ContentDescriptionImage = mediaObj.ObjectValue ?? string.Empty;
                    quizAnswerSubmit.ContentDetails.PublicIdForContentDescription = mediaObj.ObjectPublicId ?? string.Empty;

                    var newMedia = ExtractDynamicMedia(mediaObj, leadUserInfo, userMediaClassifications, dbUsers);
                    if (newMedia != null)
                    {
                        quizAnswerSubmit.ContentDetails.ContentDescriptionImage = newMedia.MediaUrl;
                        quizAnswerSubmit.ContentDetails.PublicIdForContentDescription = newMedia.MediaPublicId;
                    }
                }
                else
                {
                    quizAnswerSubmit.ContentDetails.ContentDescriptionImage = ((Db.QuestionsInQuiz)previousQuestionObj).DescriptionImage ?? string.Empty;
                    quizAnswerSubmit.ContentDetails.PublicIdForContentDescription = ((Db.QuestionsInQuiz)previousQuestionObj).PublicIdForDescription ?? string.Empty;
                }

                quizAnswerSubmit.ContentDetails.ShowContentDescriptionImage = ((Db.QuestionsInQuiz)previousQuestionObj).ShowDescriptionImage.HasValue && ((Db.QuestionsInQuiz)previousQuestionObj).ShowDescriptionImage.Value ? ((Db.QuestionsInQuiz)previousQuestionObj).ShowDescriptionImage.Value : false;
                quizAnswerSubmit.ContentDetails.ViewPreviousQuestion = ((Db.QuestionsInQuiz)previousQuestionObj).ViewPreviousQuestion;
                quizAnswerSubmit.ContentDetails.AutoPlay = ((Db.QuestionsInQuiz)previousQuestionObj).AutoPlay;
                quizAnswerSubmit.ContentDetails.SecondsToApply = ((Db.QuestionsInQuiz)previousQuestionObj).SecondsToApply ?? "0";
                quizAnswerSubmit.ContentDetails.VideoFrameEnabled = ((Db.QuestionsInQuiz)previousQuestionObj).VideoFrameEnabled ?? false;
                quizAnswerSubmit.ContentDetails.AutoPlayForDescription = ((Db.QuestionsInQuiz)previousQuestionObj).AutoPlayForDescription;
                quizAnswerSubmit.ContentDetails.SecondsToApplyForDescription = ((Db.QuestionsInQuiz)previousQuestionObj).SecondsToApplyForDescription ?? "0";
                quizAnswerSubmit.ContentDetails.DescVideoFrameEnabled = ((Db.QuestionsInQuiz)previousQuestionObj).DescVideoFrameEnabled ?? false;
                quizAnswerSubmit.ContentDetails.ShowTitle = ((Db.QuestionsInQuiz)previousQuestionObj).ShowTitle;
                quizAnswerSubmit.ContentDetails.DisplayOrderForTitle = ((Db.QuestionsInQuiz)previousQuestionObj).DisplayOrderForTitle;
                quizAnswerSubmit.ContentDetails.DisplayOrderForTitleImage = ((Db.QuestionsInQuiz)previousQuestionObj).DisplayOrderForTitleImage;
                quizAnswerSubmit.ContentDetails.DisplayOrderForDescription = ((Db.QuestionsInQuiz)previousQuestionObj).DisplayOrderForDescription;
                quizAnswerSubmit.ContentDetails.DisplayOrderForDescriptionImage = ((Db.QuestionsInQuiz)previousQuestionObj).DisplayOrderForDescriptionImage;
                quizAnswerSubmit.ContentDetails.DisplayOrderForNextButton = ((Db.QuestionsInQuiz)previousQuestionObj).DisplayOrderForNextButton;

                if (previous != null)
                {
                    if (previous.GetType().BaseType.Name == "QuestionsInQuiz")
                    {
                        if (((Db.QuestionsInQuiz)previous).Type == (int)BranchingLogicEnum.QUESTION)
                            quizAnswerSubmit.IsBackButtonEnable = (((Db.QuestionsInQuiz)previous).RevealCorrectAnswer.HasValue && ((Db.QuestionsInQuiz)previous).RevealCorrectAnswer.Value) ? false : (((Db.QuestionsInQuiz)previous).ViewPreviousQuestion || ((Db.QuestionsInQuiz)previous).EditAnswer);
                        else if (((Db.QuestionsInQuiz)previous).Type == (int)BranchingLogicEnum.CONTENT)
                            quizAnswerSubmit.IsBackButtonEnable = ((Db.QuestionsInQuiz)previous).ViewPreviousQuestion;

                    }
                    if (previous.GetType().BaseType.Name == "ContentsInQuiz")
                        quizAnswerSubmit.IsBackButtonEnable = ((Db.ContentsInQuiz)previous).ViewPreviousQuestion;
                }
            }

            return quizQuestionStatsObj;
        }

        private void QuestionTypeQuestion(int QuestionId, int? QuestionType, Db.QuizAttempts quizAttemptsObj, AutomationUnitOfWork UOWObj, Db.Quiz quizObj)
        {
            if (QuestionType == (int)BranchingLogicEnum.QUESTION || quizObj.QuesAndContentInSameTable)
            {
                var currentQuestionStats = quizAttemptsObj.QuizQuestionStats.FirstOrDefault(r => r.QuestionId == QuestionId && r.Status == (int)StatusEnum.Active);
                if (currentQuestionStats != null)
                {
                    currentQuestionStats.Status = (int)StatusEnum.Inactive;
                    UOWObj.QuizQuestionStatsRepository.Update(currentQuestionStats);
                    UOWObj.Save();
                }
            }
            else
            {
                var currentQuestionStats = quizAttemptsObj.QuizObjectStats.FirstOrDefault(r => r.ObjectId == QuestionId && r.Status == (int)StatusEnum.Active);
                if (currentQuestionStats != null)
                {
                    currentQuestionStats.Status = (int)StatusEnum.Inactive;
                    UOWObj.QuizObjectStatsRepository.Update(currentQuestionStats);
                    UOWObj.Save();
                }
            }
        }

        private QuizBrandingAndStyleModel BrandingStyleObj(Db.QuizDetails quizDetails)
        {
            QuizBrandingAndStyleModel BrandingAndStyleObj;
            var brandingAndStyle = quizDetails.QuizBrandingAndStyle.FirstOrDefault();

            BrandingAndStyleObj = new QuizBrandingAndStyleModel();

            if (brandingAndStyle != null)
            {
                BrandingAndStyleObj.QuizId = quizDetails.Quiz.Id;
                BrandingAndStyleObj.ImageFileURL = brandingAndStyle.ImageFileURL;
                BrandingAndStyleObj.PublicIdForFileURL = brandingAndStyle.PublicId;
                BrandingAndStyleObj.BackgroundColor = brandingAndStyle.BackgroundColor;
                BrandingAndStyleObj.ButtonColor = brandingAndStyle.ButtonColor;
                BrandingAndStyleObj.OptionColor = brandingAndStyle.OptionColor;
                BrandingAndStyleObj.ButtonFontColor = brandingAndStyle.ButtonFontColor;
                BrandingAndStyleObj.OptionFontColor = brandingAndStyle.OptionFontColor;
                BrandingAndStyleObj.FontColor = brandingAndStyle.FontColor;
                BrandingAndStyleObj.ButtonHoverColor = brandingAndStyle.ButtonHoverColor;
                BrandingAndStyleObj.ButtonHoverTextColor = brandingAndStyle.ButtonHoverTextColor;
                BrandingAndStyleObj.FontType = brandingAndStyle.FontType;
                BrandingAndStyleObj.BackgroundColorofSelectedAnswer = brandingAndStyle.BackgroundColorofSelectedAnswer;
                BrandingAndStyleObj.BackgroundColorofAnsweronHover = brandingAndStyle.BackgroundColorofAnsweronHover;
                BrandingAndStyleObj.AnswerTextColorofSelectedAnswer = brandingAndStyle.AnswerTextColorofSelectedAnswer;
                BrandingAndStyleObj.IsBackType = brandingAndStyle.IsBackType;
                BrandingAndStyleObj.BackImageFileURL = brandingAndStyle.BackImageFileURL;
                BrandingAndStyleObj.BackColor = brandingAndStyle.BackColor;
                BrandingAndStyleObj.Opacity = brandingAndStyle.Opacity;
                BrandingAndStyleObj.LogoUrl = brandingAndStyle.LogoUrl ?? string.Empty;
                BrandingAndStyleObj.LogoPublicId = brandingAndStyle.LogoPublicId;
                BrandingAndStyleObj.Language = brandingAndStyle.Language;
                BrandingAndStyleObj.BackgroundColorofLogo = brandingAndStyle.BackgroundColorofLogo;
                BrandingAndStyleObj.AutomationAlignment = brandingAndStyle.AutomationAlignment;
                BrandingAndStyleObj.LogoAlignment = brandingAndStyle.LogoAlignment;
                BrandingAndStyleObj.Flip = brandingAndStyle.Flip;
            }

            return BrandingAndStyleObj;
        }
        #endregion

    }
}
