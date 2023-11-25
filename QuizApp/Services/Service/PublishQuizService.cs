using Core.Common.Caching;
using Newtonsoft.Json;
using QuizApp.Helpers;
using QuizApp.RepositoryPattern;
using QuizApp.Services.Model;
using QuizApp.Services.Validator;
using System;
using QuizApp.RepositoryExtensions;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Web;


namespace QuizApp.Services.Service
{
    public class PublishQuizService : IPublishQuizService
    {
        private ResultEnum status = ResultEnum.Ok;
        private string errormessage = string.Empty;
        private string leadInfoUpdateJson = "{'ContactId': '{ContactId}','ClientCode': '{ClientCode}','campaignName': '{campaignName}','appointmentStatus': '{appointmentStatus}','appointmentDate': '{appointmentDate}','appointmentTypeId': {appointmentTypeId},'appointmentTypeTitle': '{appointmentTypeTitle}','calendarId': {calendarId},'calendarTitle': '{calendarTitle}','appointmentBookedDate': '{appointmentBookedDate}','UserToken': '{UserToken}','SourceId': '{SourceId}'}";
        private readonly IQuizVariablesService _quizVariablesService; 

        public ResultEnum Status { get { return status; } set { status = value; } }
        public string ErrorMessage { get { return errormessage; } set { errormessage = value; } }

        public PublishQuizService(IQuizVariablesService quizVariablesService)
        {
            _quizVariablesService = quizVariablesService;
        }

        private int AddPublishQuizDetails(AutomationUnitOfWork UOWObj, Db.QuizDetails draftedQuizDetailsObj, int versionNumber, int businessUserId)
        {
            var publishedQuizDetailsObj = new Db.QuizDetails();
            #region insert in Quiz Details

            publishedQuizDetailsObj.ParentQuizId = draftedQuizDetailsObj.ParentQuizId;
            publishedQuizDetailsObj.QuizTitle = draftedQuizDetailsObj.QuizTitle;
            publishedQuizDetailsObj.QuizCoverTitle = draftedQuizDetailsObj.QuizCoverTitle;
            publishedQuizDetailsObj.ShowQuizCoverTitle = draftedQuizDetailsObj.ShowQuizCoverTitle;
            publishedQuizDetailsObj.QuizCoverImage = draftedQuizDetailsObj.QuizCoverImage;
            publishedQuizDetailsObj.ShowQuizCoverImage = draftedQuizDetailsObj.ShowQuizCoverImage;
            publishedQuizDetailsObj.EnableMediaFile = draftedQuizDetailsObj.EnableMediaFile;
            publishedQuizDetailsObj.PublicId = draftedQuizDetailsObj.PublicId;
            publishedQuizDetailsObj.QuizCoverImgXCoordinate = draftedQuizDetailsObj.QuizCoverImgXCoordinate;
            publishedQuizDetailsObj.QuizCoverImgYCoordinate = draftedQuizDetailsObj.QuizCoverImgYCoordinate;
            publishedQuizDetailsObj.QuizCoverImgHeight = draftedQuizDetailsObj.QuizCoverImgHeight;
            publishedQuizDetailsObj.QuizCoverImgWidth = draftedQuizDetailsObj.QuizCoverImgWidth;
            publishedQuizDetailsObj.QuizCoverImgAttributionLabel = draftedQuizDetailsObj.QuizCoverImgAttributionLabel;
            publishedQuizDetailsObj.QuizCoverImgAltTag = draftedQuizDetailsObj.QuizCoverImgAltTag;
            publishedQuizDetailsObj.QuizDescription = draftedQuizDetailsObj.QuizDescription;
            publishedQuizDetailsObj.ShowDescription = draftedQuizDetailsObj.ShowDescription;
            publishedQuizDetailsObj.StartButtonText = draftedQuizDetailsObj.StartButtonText;
            publishedQuizDetailsObj.EnableNextButton = draftedQuizDetailsObj.EnableNextButton;
            publishedQuizDetailsObj.IsBranchingLogicEnabled = draftedQuizDetailsObj.IsBranchingLogicEnabled;
            publishedQuizDetailsObj.HideSocialShareButtons = draftedQuizDetailsObj.HideSocialShareButtons;
            publishedQuizDetailsObj.EnableFacebookShare = draftedQuizDetailsObj.EnableFacebookShare;
            publishedQuizDetailsObj.EnableTwitterShare = draftedQuizDetailsObj.EnableTwitterShare;
            publishedQuizDetailsObj.EnableLinkedinShare = draftedQuizDetailsObj.EnableLinkedinShare;
            publishedQuizDetailsObj.State = (int)QuizStateEnum.PUBLISHED;
            publishedQuizDetailsObj.Version = versionNumber - 1;
            publishedQuizDetailsObj.Status = (int)StatusEnum.Active;
            publishedQuizDetailsObj.CreatedOn = DateTime.UtcNow;
            publishedQuizDetailsObj.CreatedBy = businessUserId;
            publishedQuizDetailsObj.LastUpdatedOn = DateTime.UtcNow;
            publishedQuizDetailsObj.LastUpdatedBy = businessUserId;
            publishedQuizDetailsObj.ViewPreviousQuestion = draftedQuizDetailsObj.ViewPreviousQuestion;
            publishedQuizDetailsObj.EditAnswer = draftedQuizDetailsObj.EditAnswer;
            publishedQuizDetailsObj.AutoPlay = draftedQuizDetailsObj.AutoPlay;
            publishedQuizDetailsObj.DisplayOrderForTitle = draftedQuizDetailsObj.DisplayOrderForTitle;
            publishedQuizDetailsObj.DisplayOrderForTitleImage = draftedQuizDetailsObj.DisplayOrderForTitleImage;
            publishedQuizDetailsObj.DisplayOrderForDescription = draftedQuizDetailsObj.DisplayOrderForDescription;
            publishedQuizDetailsObj.DisplayOrderForNextButton = draftedQuizDetailsObj.DisplayOrderForNextButton;
            publishedQuizDetailsObj.SecondsToApply = draftedQuizDetailsObj.SecondsToApply;
            publishedQuizDetailsObj.VideoFrameEnabled = draftedQuizDetailsObj.VideoFrameEnabled;
            publishedQuizDetailsObj.CompanyId = draftedQuizDetailsObj.CompanyId;

            UOWObj.QuizDetailsRepository.Insert(publishedQuizDetailsObj);

            UOWObj.Save();
            #endregion
            #region insert in QuizComponentLogs

            var ComponentQuizObj = new Db.QuizComponentLogs();
            ComponentQuizObj.QuizId = publishedQuizDetailsObj.Id;
            ComponentQuizObj.DraftedObjectId = draftedQuizDetailsObj.Id;
            ComponentQuizObj.PublishedObjectId = publishedQuizDetailsObj.Id;
            ComponentQuizObj.ObjectTypeId = (int)BranchingLogicEnum.COVERDETAILS;
            UOWObj.QuizComponentLogsRepository.Insert(ComponentQuizObj);
            UOWObj.Save();

            #endregion

            return publishedQuizDetailsObj.Id;
        }

        private void AddQuizPersonalityResult(AutomationUnitOfWork UOWObj, Db.QuizDetails draftedQuizDetailsObj, int businessUserId, int currentPublishedQuizDetailId, int quizType)
        {
            if (quizType == (int)QuizTypeEnum.Personality || quizType == (int)QuizTypeEnum.PersonalityTemplate)
            {
                var personalityResult = draftedQuizDetailsObj.PersonalityResultSetting.FirstOrDefault();
                var personalityResultObj = new Db.PersonalityResultSetting();

                personalityResultObj.QuizId = currentPublishedQuizDetailId;
                personalityResultObj.Title = personalityResult.Title;
                personalityResultObj.Status = personalityResult.Status;
                personalityResultObj.MaxResult = personalityResult.MaxResult;
                personalityResultObj.GraphColor = personalityResult.GraphColor;
                personalityResultObj.ButtonColor = personalityResult.ButtonColor;
                personalityResultObj.ButtonFontColor = personalityResult.ButtonFontColor;
                personalityResultObj.SideButtonText = personalityResult.SideButtonText;
                personalityResultObj.IsFullWidthEnable = personalityResult.IsFullWidthEnable;
                personalityResultObj.LastUpdatedOn = DateTime.UtcNow;
                personalityResultObj.LastUpdatedBy = businessUserId;
                //  personalityResultObj.ShowLeadUserForm = personalityResult.ShowLeadUserForm;

                UOWObj.PersonalityResultSettingRepository.Insert(personalityResultObj);
                UOWObj.Save();
            }
        }

        private void AddPublishQuizMediaVariables(AutomationUnitOfWork UOWObj, int lastPublishedQuizDetailId, int currentPublishedQuizDetailId, int draftquizDetailid)
        {
            var lastpublishedQuizDetailsObj = UOWObj.QuizDetailsRepository.GetByID(lastPublishedQuizDetailId);
            if (lastpublishedQuizDetailsObj != null && lastpublishedQuizDetailsObj.QuizComponentLogs.Any() && lastpublishedQuizDetailsObj.MediaVariablesDetails.Any())
            {
                var lastPublishedQuizDetailsObjQuestions = lastpublishedQuizDetailsObj.QuizComponentLogs.FirstOrDefault(r => r.ObjectTypeId == (int)BranchingLogicEnum.COVERDETAILS && r.DraftedObjectId == draftquizDetailid);

                if (lastPublishedQuizDetailsObjQuestions != null)
                {
                    var mediaVariablesDetailsList = lastpublishedQuizDetailsObj.MediaVariablesDetails.Where(r => r.ObjectTypeId == (int)BranchingLogicEnum.COVERDETAILS && r.ObjectId == lastPublishedQuizDetailsObjQuestions.PublishedObjectId);

                    if (mediaVariablesDetailsList != null && mediaVariablesDetailsList.Any())
                    {
                        var listmediaVariables = new List<Db.MediaVariablesDetails>();
                        foreach (var mediaVariablesDetails in mediaVariablesDetailsList)
                        {
                            var mediaVariablesDetailsObj = new Db.MediaVariablesDetails();
                            mediaVariablesDetailsObj.QuizId = currentPublishedQuizDetailId;
                            mediaVariablesDetailsObj.ObjectId = currentPublishedQuizDetailId;
                            mediaVariablesDetailsObj.ObjectPublicId = mediaVariablesDetails.ObjectPublicId;
                            mediaVariablesDetailsObj.ObjectTypeId = (int)BranchingLogicEnum.COVERDETAILS;
                            mediaVariablesDetailsObj.ObjectValue = mediaVariablesDetails.ObjectValue;
                            mediaVariablesDetailsObj.Type = mediaVariablesDetails.Type;
                            mediaVariablesDetailsObj.ConfigurationDetailsId = mediaVariablesDetails.ConfigurationDetailsId;
                            mediaVariablesDetailsObj.MediaOwner = mediaVariablesDetails.MediaOwner;
                            mediaVariablesDetailsObj.ProfileMedia = mediaVariablesDetails.ProfileMedia;
                            listmediaVariables.Add(mediaVariablesDetailsObj);
                        }

                        UOWObj.MediaVariablesDetailsRepository.BulKInsert(listmediaVariables);
                        UOWObj.Save();

                    }
                }
            }



        }

        private List<Mappings> AddPublishQuizResultDetails(AutomationUnitOfWork UOWObj, Db.QuizDetails draftedQuizDetailsObj, int businessUserId, int currentPublishedQuizDetailId, int lastPublishedQuizDetailId)
        {
            List<Mappings> mappingLst = new List<Mappings>();
            #region insert in Quiz Results

            var lstResults = draftedQuizDetailsObj.QuizResults.Where(r => r.Status == (int)StatusEnum.Active).ToList();

            foreach (var item in lstResults)

            {
                var quizResultObj = new Db.QuizResults();

                quizResultObj.QuizId = currentPublishedQuizDetailId;
                quizResultObj.Title = item.Title;
                quizResultObj.ShowExternalTitle = item.ShowExternalTitle;
                quizResultObj.InternalTitle = item.InternalTitle;
                quizResultObj.ShowInternalTitle = item.ShowInternalTitle;
                quizResultObj.Image = item.Image;
                quizResultObj.EnableMediaFile = item.EnableMediaFile;
                quizResultObj.PublicId = item.PublicId;
                quizResultObj.Description = item.Description;
                quizResultObj.ShowDescription = item.ShowDescription;
                quizResultObj.ActionButtonURL = item.ActionButtonURL;
                quizResultObj.ActionButtonTxtSize = item.ActionButtonTxtSize;
                quizResultObj.ActionButtonTitleColor = item.ActionButtonTitleColor;
                quizResultObj.LastUpdatedOn = DateTime.UtcNow;
                quizResultObj.LastUpdatedBy = businessUserId;
                quizResultObj.HideCallToAction = item.HideCallToAction;
                quizResultObj.ActionButtonText = item.ActionButtonText;
                quizResultObj.ShowResultImage = item.ShowResultImage;
                quizResultObj.OpenLinkInNewTab = item.OpenLinkInNewTab;
                quizResultObj.ActionButtonColor = item.ActionButtonColor;
                quizResultObj.MinScore = item.MinScore;
                quizResultObj.MaxScore = item.MaxScore;
                quizResultObj.DisplayOrder = item.DisplayOrder;
                quizResultObj.IsPersonalityCorrelatedResult = item.IsPersonalityCorrelatedResult;
                quizResultObj.DisplayOrderForTitle = item.DisplayOrderForTitle;
                quizResultObj.DisplayOrderForTitleImage = item.DisplayOrderForTitleImage;
                quizResultObj.DisplayOrderForDescription = item.DisplayOrderForDescription;
                quizResultObj.DisplayOrderForNextButton = item.DisplayOrderForNextButton;
                quizResultObj.Status = (int)StatusEnum.Active;
                quizResultObj.State = (int)QuizStateEnum.PUBLISHED;
                //   quizResultObj.ShowLeadUserForm = item.ShowLeadUserForm;
                quizResultObj.AutoPlay = item.AutoPlay;
                quizResultObj.SecondsToApply = item.SecondsToApply;
                quizResultObj.VideoFrameEnabled = item.VideoFrameEnabled;

                UOWObj.QuizResultsRepository.Insert(quizResultObj);

                UOWObj.Save();

                mappingLst.Add(new Mappings
                {
                    DraftedId = item.Id,
                    PublishedId = quizResultObj.Id,
                    Type = (int)BranchingLogicEnum.RESULT
                });

                #region insert in QuizComponentLogs

                var ComponentObj = new Db.QuizComponentLogs();
                ComponentObj.QuizId = currentPublishedQuizDetailId;
                ComponentObj.DraftedObjectId = item.Id;
                ComponentObj.PublishedObjectId = quizResultObj.Id;
                ComponentObj.ObjectTypeId = (int)BranchingLogicEnum.RESULT;
                UOWObj.QuizComponentLogsRepository.Insert(ComponentObj);
                UOWObj.Save();

                #endregion
                if (lastPublishedQuizDetailId != 0)
                {
                    #region insert in MediaVariablesDetails

                    var lastpublishedQuizDetailsObj = UOWObj.QuizDetailsRepository.GetByID(lastPublishedQuizDetailId);

                    if (lastpublishedQuizDetailsObj != null && lastpublishedQuizDetailsObj.QuizComponentLogs.Any() && lastpublishedQuizDetailsObj.MediaVariablesDetails.Any())
                    {
                        var lastPublishedQuizDetailsObjQuestions = lastpublishedQuizDetailsObj.QuizComponentLogs.FirstOrDefault(r => r.ObjectTypeId == (int)BranchingLogicEnum.RESULT && r.DraftedObjectId == item.Id);

                        if (lastPublishedQuizDetailsObjQuestions != null)
                        {
                            var mediaVariablesDetailsList = lastpublishedQuizDetailsObj.MediaVariablesDetails.Where(r => r.ObjectTypeId == (int)BranchingLogicEnum.RESULT && r.ObjectId == lastPublishedQuizDetailsObjQuestions.PublishedObjectId);

                            if (mediaVariablesDetailsList != null && mediaVariablesDetailsList.Any())
                            {
                                var listmediaVariables = new List<Db.MediaVariablesDetails>();

                                foreach (var mediaVariablesDetails in mediaVariablesDetailsList)
                                {
                                    var mediaVariablesDetailsObj = new Db.MediaVariablesDetails();
                                    mediaVariablesDetailsObj.QuizId = currentPublishedQuizDetailId;
                                    mediaVariablesDetailsObj.ObjectId = quizResultObj.Id;
                                    mediaVariablesDetailsObj.ObjectPublicId = mediaVariablesDetails.ObjectPublicId;
                                    mediaVariablesDetailsObj.ObjectTypeId = (int)BranchingLogicEnum.RESULT;
                                    mediaVariablesDetailsObj.ObjectValue = mediaVariablesDetails.ObjectValue;
                                    mediaVariablesDetailsObj.Type = mediaVariablesDetails.Type;
                                    mediaVariablesDetailsObj.ConfigurationDetailsId = mediaVariablesDetails.ConfigurationDetailsId;
                                    mediaVariablesDetailsObj.MediaOwner = mediaVariablesDetails.MediaOwner;
                                    mediaVariablesDetailsObj.ProfileMedia = mediaVariablesDetails.ProfileMedia;
                                    listmediaVariables.Add(mediaVariablesDetailsObj);
                                }

                                UOWObj.MediaVariablesDetailsRepository.BulKInsert(listmediaVariables);
                                UOWObj.Save();
                            }
                        }
                    }

                    #endregion

                    #region insert in ResultIdsInConfigurationDetails

                    if (lastpublishedQuizDetailsObj != null && lastpublishedQuizDetailsObj.QuizComponentLogs.Any())
                    {
                        var lastPublishedQuizDetailsObjQuestions = lastpublishedQuizDetailsObj.QuizComponentLogs.FirstOrDefault(r => r.ObjectTypeId == (int)BranchingLogicEnum.RESULT && r.DraftedObjectId == item.Id);

                        if (lastPublishedQuizDetailsObjQuestions != null)
                        {
                            var quizResultsList = lastpublishedQuizDetailsObj.QuizResults.FirstOrDefault(r => r.Id == lastPublishedQuizDetailsObjQuestions.PublishedObjectId).ResultIdsInConfigurationDetails.Where(q => q.ResultId == lastPublishedQuizDetailsObjQuestions.PublishedObjectId);

                            if (quizResultsList != null && quizResultsList.Any())
                            {
                                var resultIdObjList = new List<Db.ResultIdsInConfigurationDetails>();
                                foreach (var quizResultsObj in quizResultsList)
                                {
                                    var resultIdObj = new Db.ResultIdsInConfigurationDetails();
                                    resultIdObj.ResultId = quizResultObj.Id;
                                    resultIdObj.FormId = quizResultsObj.FormId;
                                    resultIdObj.FlowOrder = quizResultsObj.FlowOrder;
                                    resultIdObj.ConfigurationDetailsId = quizResultsObj.ConfigurationDetailsId;
                                    resultIdObjList.Add(resultIdObj);
                                }

                                UOWObj.ResultIdsInConfigurationDetailsRepository.BulKInsert(resultIdObjList);
                                UOWObj.Save();
                            }
                        }
                    }

                    #endregion
                }

                #region insert in Quizvariables for QuizResults

                if (item != null)
                {
                    var oldVar = _quizVariablesService.GetQuizVariables(draftedQuizDetailsObj.Id, item.Id, (int)QuizVariableObjectTypes.RESULT);
                    QuizVariableModel quizVariable = new QuizVariableModel();
                    if (oldVar != null && oldVar.Any())
                    {
                        quizVariable.Variables = String.Join(",", oldVar);
                        quizVariable.QuizDetailsId = currentPublishedQuizDetailId;
                        quizVariable.ObjectId = quizResultObj.Id;
                        quizVariable.ObjectTypes = (int)QuizVariableObjectTypes.RESULT;

                        _quizVariablesService.AddQuizVariables(quizVariable);

                    }
                }

                #endregion



            }

            #endregion


            #region insert in Result Settings

            var draftedResultSetting = draftedQuizDetailsObj.ResultSettings.FirstOrDefault();

            if (draftedResultSetting != null)
            {
                var resultSettingObj = new Db.ResultSettings();

                resultSettingObj.QuizId = currentPublishedQuizDetailId;
                resultSettingObj.ShowScoreValue = draftedResultSetting.ShowScoreValue;
                resultSettingObj.ShowCorrectAnswer = draftedResultSetting.ShowCorrectAnswer;
                resultSettingObj.MinScore = draftedResultSetting.MinScore;
                resultSettingObj.CustomTxtForAnswerKey = draftedResultSetting.CustomTxtForAnswerKey;
                resultSettingObj.CustomTxtForYourAnswer = draftedResultSetting.CustomTxtForYourAnswer;
                resultSettingObj.CustomTxtForCorrectAnswer = draftedResultSetting.CustomTxtForCorrectAnswer;
                resultSettingObj.CustomTxtForExplanation = draftedResultSetting.CustomTxtForExplanation;
                resultSettingObj.CustomTxtForScoreValueInResult = draftedResultSetting.CustomTxtForScoreValueInResult;
                resultSettingObj.LastUpdatedBy = businessUserId;
                resultSettingObj.LastUpdatedOn = DateTime.UtcNow;
                resultSettingObj.State = (int)QuizStateEnum.PUBLISHED;

                UOWObj.ResultSettingsRepository.Insert(resultSettingObj);

                UOWObj.Save();
            }

            #endregion

            #region insert in Quiz BrandingAndStyle

            var draftedBrandingAndStyle = draftedQuizDetailsObj.QuizBrandingAndStyle.FirstOrDefault();

            if (draftedBrandingAndStyle != null)
            {
                var brandingAndStyleObj = new Db.QuizBrandingAndStyle();

                brandingAndStyleObj.QuizId = currentPublishedQuizDetailId;
                brandingAndStyleObj.ImageFileURL = draftedBrandingAndStyle.ImageFileURL;
                brandingAndStyleObj.PublicId = draftedBrandingAndStyle.PublicId;
                brandingAndStyleObj.BackgroundColor = draftedBrandingAndStyle.BackgroundColor;
                brandingAndStyleObj.ButtonColor = draftedBrandingAndStyle.ButtonColor;
                brandingAndStyleObj.OptionColor = draftedBrandingAndStyle.OptionColor;
                brandingAndStyleObj.ButtonFontColor = draftedBrandingAndStyle.ButtonFontColor;
                brandingAndStyleObj.OptionFontColor = draftedBrandingAndStyle.OptionFontColor;
                brandingAndStyleObj.FontColor = draftedBrandingAndStyle.FontColor;
                brandingAndStyleObj.ButtonHoverColor = draftedBrandingAndStyle.ButtonHoverColor;
                brandingAndStyleObj.ButtonHoverTextColor = draftedBrandingAndStyle.ButtonHoverTextColor;
                brandingAndStyleObj.FontType = draftedBrandingAndStyle.FontType;
                brandingAndStyleObj.BackgroundColorofSelectedAnswer = draftedBrandingAndStyle.BackgroundColorofSelectedAnswer;
                brandingAndStyleObj.BackgroundColorofAnsweronHover = draftedBrandingAndStyle.BackgroundColorofAnsweronHover;
                brandingAndStyleObj.AnswerTextColorofSelectedAnswer = draftedBrandingAndStyle.AnswerTextColorofSelectedAnswer;
                brandingAndStyleObj.ApplyToAll = draftedBrandingAndStyle.ApplyToAll;
                brandingAndStyleObj.BackColor = draftedBrandingAndStyle.BackColor;
                brandingAndStyleObj.Opacity = draftedBrandingAndStyle.Opacity;
                brandingAndStyleObj.LogoUrl = draftedBrandingAndStyle.LogoUrl;
                brandingAndStyleObj.LogoPublicId = draftedBrandingAndStyle.LogoPublicId;
                brandingAndStyleObj.BackgroundColorofLogo = draftedBrandingAndStyle.BackgroundColorofLogo;
                brandingAndStyleObj.AutomationAlignment = draftedBrandingAndStyle.AutomationAlignment;
                brandingAndStyleObj.LogoAlignment = draftedBrandingAndStyle.LogoAlignment;
                brandingAndStyleObj.Flip = draftedBrandingAndStyle.Flip;
                brandingAndStyleObj.Language = draftedBrandingAndStyle.Language;
                brandingAndStyleObj.IsBackType = draftedBrandingAndStyle.IsBackType;
                brandingAndStyleObj.BackImageFileURL = draftedBrandingAndStyle.BackImageFileURL;
                brandingAndStyleObj.LastUpdatedBy = businessUserId;
                brandingAndStyleObj.LastUpdatedOn = DateTime.UtcNow;
                brandingAndStyleObj.State = (int)QuizStateEnum.PUBLISHED;

                UOWObj.QuizBrandingAndStyleRepository.Insert(brandingAndStyleObj);

                UOWObj.Save();
            }

            #endregion




            return mappingLst;
        }
        private List<Mappings> AddPublishQuizQuestioncontactAction(AutomationUnitOfWork UOWObj, Db.QuizDetails draftedQuizDetailsObj, int businessUserId, int currentPublishedQuizDetailId, int lastPublishedQuizDetailId, int quizType)
        {
            List<Mappings> mappingLst = new List<Mappings>();
            var lastpublishedQuizDetailsObj = UOWObj.QuizDetailsRepository.GetByID(lastPublishedQuizDetailId);
            #region insert in Questions

            var lstQuestions = draftedQuizDetailsObj.QuestionsInQuiz.Where(r => r.Status == (int)StatusEnum.Active).ToList();

            foreach (var question in lstQuestions)
            {
                var questionObj = new Db.QuestionsInQuiz();

                questionObj.QuizId = currentPublishedQuizDetailId;
                questionObj.Question = question.Question;
                questionObj.ShowTitle = question.ShowTitle;
                questionObj.QuestionImage = question.QuestionImage;
                questionObj.EnableMediaFile = question.EnableMediaFile;
                questionObj.PublicId = question.PublicId;
                questionObj.CorrectAnswerDescription = question.CorrectAnswerDescription;
                questionObj.RevealCorrectAnswer = question.RevealCorrectAnswer;
                questionObj.AliasTextForCorrect = question.AliasTextForCorrect;
                questionObj.AliasTextForIncorrect = question.AliasTextForIncorrect;
                questionObj.AliasTextForYourAnswer = question.AliasTextForYourAnswer;
                questionObj.AliasTextForCorrectAnswer = question.AliasTextForCorrectAnswer;
                questionObj.AliasTextForExplanation = question.AliasTextForExplanation;
                questionObj.AliasTextForNextButton = question.AliasTextForNextButton;
                questionObj.ShowQuestionImage = question.ShowQuestionImage;
                questionObj.LastUpdatedOn = DateTime.UtcNow;
                questionObj.LastUpdatedBy = businessUserId;
                questionObj.DisplayOrder = question.DisplayOrder;
                questionObj.ShowAnswerImage = question.ShowAnswerImage;
                questionObj.Status = (int)StatusEnum.Active;
                questionObj.State = (int)QuizStateEnum.PUBLISHED;
                questionObj.AnswerType = question.AnswerType;
                questionObj.MinAnswer = question.MinAnswer;
                questionObj.MaxAnswer = question.MaxAnswer;
                questionObj.NextButtonColor = question.NextButtonColor;
                questionObj.NextButtonText = question.NextButtonText;
                questionObj.NextButtonTxtColor = question.NextButtonTxtColor;
                questionObj.NextButtonTxtSize = question.NextButtonTxtSize;
                questionObj.EnableNextButton = question.EnableNextButton;
                questionObj.ViewPreviousQuestion = question.ViewPreviousQuestion;
                questionObj.EditAnswer = question.EditAnswer;
                questionObj.TimerRequired = question.TimerRequired;
                questionObj.Time = question.Time;
                questionObj.Description = question.Description;
                questionObj.ShowDescription = question.ShowDescription;
                questionObj.DescriptionImage = question.DescriptionImage;
                questionObj.EnableMediaFileForDescription = question.EnableMediaFileForDescription;
                questionObj.ShowDescriptionImage = question.ShowDescriptionImage;
                questionObj.PublicIdForDescription = question.PublicIdForDescription;
                questionObj.AutoPlay = question.AutoPlay;
                questionObj.AutoPlayForDescription = question.AutoPlayForDescription;
                questionObj.Type = question.Type;
                questionObj.DisplayOrderForTitle = question.DisplayOrderForTitle;
                questionObj.DisplayOrderForTitleImage = question.DisplayOrderForTitleImage;
                questionObj.DisplayOrderForDescription = question.DisplayOrderForDescription;
                questionObj.DisplayOrderForDescriptionImage = question.DisplayOrderForDescriptionImage;
                questionObj.DisplayOrderForAnswer = question.DisplayOrderForAnswer;
                questionObj.DisplayOrderForNextButton = question.DisplayOrderForNextButton;
                questionObj.EnableComment = question.EnableComment;
                questionObj.TopicTitle = question.TopicTitle;
                questionObj.SecondsToApply = question.SecondsToApply;
                questionObj.SecondsToApplyForDescription = question.SecondsToApplyForDescription;
                questionObj.VideoFrameEnabled = question.VideoFrameEnabled;
                questionObj.DescVideoFrameEnabled = question.DescVideoFrameEnabled;
                questionObj.AnswerStructureType = question.AnswerStructureType;
                questionObj.TemplateId = question.TemplateId;
                questionObj.LanguageCode = question.LanguageCode;
                questionObj.IsMultiRating = question.IsMultiRating;
                UOWObj.QuestionsInQuizRepository.Insert(questionObj);

                UOWObj.Save();

                mappingLst.Add(new Mappings
                {
                    DraftedId = question.Id,
                    PublishedId = questionObj.Id,
                    Type = question.Type
                });

                #region insert in QuizComponentLogs

                var ComponentObj = new Db.QuizComponentLogs();
                ComponentObj.QuizId = currentPublishedQuizDetailId;
                ComponentObj.DraftedObjectId = question.Id;
                ComponentObj.PublishedObjectId = questionObj.Id;
                ComponentObj.ObjectTypeId = (int)BranchingLogicEnum.QUESTION;
                UOWObj.QuizComponentLogsRepository.Insert(ComponentObj);
                UOWObj.Save();

                #endregion

                #region insert in MediaVariablesDetails

                if (lastpublishedQuizDetailsObj != null && lastpublishedQuizDetailsObj.QuizComponentLogs.Any() && lastpublishedQuizDetailsObj.MediaVariablesDetails.Any())
                {
                    var lastPublishedQuizDetailsObjQuestions = lastpublishedQuizDetailsObj.QuizComponentLogs.FirstOrDefault(r => (r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION || r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT) && r.DraftedObjectId == question.Id);

                    if (lastPublishedQuizDetailsObjQuestions != null)
                    {
                        var mediaVariablesDetailsList = lastpublishedQuizDetailsObj.MediaVariablesDetails.Where(r => (r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION || r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT) && r.ObjectId == lastPublishedQuizDetailsObjQuestions.PublishedObjectId);

                        if (mediaVariablesDetailsList != null)
                        {

                            var mediaVariablesDetailsObjList = new List<Db.MediaVariablesDetails>();
                            foreach (var mediaVariablesDetails in mediaVariablesDetailsList)
                            {
                                var mediaVariablesDetailsObj = new Db.MediaVariablesDetails();
                                mediaVariablesDetailsObj.QuizId = currentPublishedQuizDetailId;
                                mediaVariablesDetailsObj.ObjectId = questionObj.Id;
                                mediaVariablesDetailsObj.ObjectPublicId = mediaVariablesDetails.ObjectPublicId;
                                mediaVariablesDetailsObj.ObjectTypeId = mediaVariablesDetails.ObjectTypeId;
                                mediaVariablesDetailsObj.ObjectValue = mediaVariablesDetails.ObjectValue;
                                mediaVariablesDetailsObj.Type = mediaVariablesDetails.Type;
                                mediaVariablesDetailsObj.ConfigurationDetailsId = mediaVariablesDetails.ConfigurationDetailsId;
                                mediaVariablesDetailsObj.MediaOwner = mediaVariablesDetails.MediaOwner;
                                mediaVariablesDetailsObj.ProfileMedia = mediaVariablesDetails.ProfileMedia;
                                mediaVariablesDetailsObjList.Add(mediaVariablesDetailsObj);

                            }
                            UOWObj.MediaVariablesDetailsRepository.BulKInsert(mediaVariablesDetailsObjList);
                            UOWObj.Save();

                        }
                    }
                }
                #endregion

                #region insert in Quizvariable for Questions

                if (question != null)
                {
                    var oldVar = _quizVariablesService.GetQuizVariables(draftedQuizDetailsObj.Id, question.Id, (int)QuizVariableObjectTypes.QUESTION);
                    QuizVariableModel quizVariable = new QuizVariableModel();
                    if (oldVar != null && oldVar.Any())
                    {
                        quizVariable.Variables = String.Join(",", oldVar);
                        quizVariable.QuizDetailsId = currentPublishedQuizDetailId;
                        quizVariable.ObjectId = questionObj.Id;
                        quizVariable.ObjectTypes = (int)QuizVariableObjectTypes.QUESTION;


                        _quizVariablesService.AddQuizVariables(quizVariable);

                    }
                }

                #endregion

                #region insert in AnswerOptions

                var lstAnswers = question.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active).ToList();

                foreach (var answer in lstAnswers)
                {
                    var answerObj = new Db.AnswerOptionsInQuizQuestions();

                    answerObj.QuestionId = questionObj.Id;
                    answerObj.QuizId = questionObj.QuizId;
                    answerObj.Option = answer.Option;
                    answerObj.Description = answer.Description;
                    answerObj.OptionImage = answer.OptionImage;
                    answerObj.EnableMediaFile = answer.EnableMediaFile;
                    answerObj.PublicId = answer.PublicId;
                    answerObj.AssociatedScore = answer.AssociatedScore;
                    answerObj.IsCorrectAnswer = answer.IsCorrectAnswer;
                    answerObj.IsCorrectForMultipleAnswer = answer.IsCorrectForMultipleAnswer;
                    answerObj.LastUpdatedOn = DateTime.UtcNow;
                    answerObj.LastUpdatedBy = businessUserId;
                    answerObj.DisplayOrder = answer.DisplayOrder;
                    answerObj.IsReadOnly = answer.IsReadOnly;
                    answerObj.IsUnansweredType = answer.IsUnansweredType;
                    answerObj.AutoPlay = answer.AutoPlay;
                    answerObj.OptionTextforRatingOne = answer.OptionTextforRatingOne;
                    answerObj.OptionTextforRatingTwo = answer.OptionTextforRatingTwo;
                    answerObj.OptionTextforRatingThree = answer.OptionTextforRatingThree;
                    answerObj.OptionTextforRatingFour = answer.OptionTextforRatingFour;
                    answerObj.OptionTextforRatingFive = answer.OptionTextforRatingFive;
                    answerObj.Status = (int)StatusEnum.Active;
                    answerObj.State = (int)QuizStateEnum.PUBLISHED;
                    answerObj.ListValues = answer.ListValues;
                    answerObj.SecondsToApply = answer.SecondsToApply;
                    answerObj.SecondsToApplyForDescription = answer.SecondsToApplyForDescription;
                    answerObj.VideoFrameEnabled = answer.VideoFrameEnabled;
                    answerObj.RefId = answer.RefId;
                    UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);

                    UOWObj.Save();


                    if (question.Type == (int)BranchingLogicEnum.WHATSAPPTEMPLATE)
                    {
                        mappingLst.Add(new Mappings
                        {
                            DraftedId = answer.Id,
                            PublishedId = answerObj.Id,
                            Type = (int)BranchingLogicEnum.WHATSAPPTEMPLATEACTION
                        });
                    }
                    else
                    {
                        mappingLst.Add(new Mappings
                        {
                            DraftedId = answer.Id,
                            PublishedId = answerObj.Id,
                            Type = (int)BranchingLogicEnum.ANSWER
                        });
                    }


                    #region insert in ObjectFieldsInAnswer

                    foreach (var objectFieldsInAnswer in answer.ObjectFieldsInAnswer)
                    {
                        var objectFieldsInAnswerObj = new Db.ObjectFieldsInAnswer();

                        if (objectFieldsInAnswer.IsCommentMapped == true) {
                            objectFieldsInAnswerObj.QuestionId = objectFieldsInAnswer.AnswerOptionsInQuizQuestions.QuestionId;

                        }


                        //if iscomment then add questionid


                        objectFieldsInAnswerObj.AnswerOptionsInQuizQuestionsId = answerObj.Id;
                        objectFieldsInAnswerObj.ObjectName = objectFieldsInAnswer.ObjectName;
                        objectFieldsInAnswerObj.FieldName = objectFieldsInAnswer.FieldName;
                        objectFieldsInAnswerObj.Value = objectFieldsInAnswer.Value;
                        objectFieldsInAnswerObj.CreatedOn = DateTime.UtcNow;
                        objectFieldsInAnswerObj.CreatedBy = businessUserId;
                        objectFieldsInAnswerObj.LastUpdatedOn = DateTime.UtcNow;
                        objectFieldsInAnswerObj.LastUpdatedBy = businessUserId;
                        objectFieldsInAnswerObj.IsExternalSync = objectFieldsInAnswer.IsExternalSync;
                        objectFieldsInAnswerObj.IsCommentMapped = objectFieldsInAnswer.IsCommentMapped;

                        UOWObj.ObjectFieldsInAnswerRepository.Insert(objectFieldsInAnswerObj);

                        UOWObj.Save();
                    }

                    #endregion

                    #region insert in QuizComponentLogs

                    var ComponentAnswerObj = new Db.QuizComponentLogs();
                    ComponentAnswerObj.QuizId = currentPublishedQuizDetailId;
                    ComponentAnswerObj.DraftedObjectId = answer.Id;
                    ComponentAnswerObj.PublishedObjectId = answerObj.Id;
                    ComponentAnswerObj.ObjectTypeId = (int)BranchingLogicEnum.ANSWER;
                    UOWObj.QuizComponentLogsRepository.Insert(ComponentAnswerObj);
                    UOWObj.Save();

                    #endregion

                    #region insert in MediaVariablesDetails

                    if (lastpublishedQuizDetailsObj != null && lastpublishedQuizDetailsObj.QuizComponentLogs.Any() && lastpublishedQuizDetailsObj.MediaVariablesDetails.Any())
                    {
                        var lastPublishedQuizDetailsObjQuestions = lastpublishedQuizDetailsObj.QuizComponentLogs.FirstOrDefault(r => r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.DraftedObjectId == answer.Id);

                        if (lastPublishedQuizDetailsObjQuestions != null)
                        {
                            var mediaVariablesDetailsList = lastpublishedQuizDetailsObj.MediaVariablesDetails.Where(r => r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.ObjectId == lastPublishedQuizDetailsObjQuestions.PublishedObjectId);

                            if (mediaVariablesDetailsList != null)
                            {
                                var mediaVariablesDetailsObjList = new List<Db.MediaVariablesDetails>();
                                foreach (var mediaVariablesDetails in mediaVariablesDetailsList)
                                {
                                    var mediaVariablesDetailsObj = new Db.MediaVariablesDetails();
                                    mediaVariablesDetailsObj.QuizId = currentPublishedQuizDetailId;
                                    mediaVariablesDetailsObj.ObjectId = answerObj.Id;
                                    mediaVariablesDetailsObj.ObjectPublicId = mediaVariablesDetails.ObjectPublicId;
                                    mediaVariablesDetailsObj.ObjectTypeId = (int)BranchingLogicEnum.ANSWER;
                                    mediaVariablesDetailsObj.ObjectValue = mediaVariablesDetails.ObjectValue;
                                    mediaVariablesDetailsObj.Type = mediaVariablesDetails.Type;
                                    mediaVariablesDetailsObj.ConfigurationDetailsId = mediaVariablesDetails.ConfigurationDetailsId;
                                    mediaVariablesDetailsObj.MediaOwner = mediaVariablesDetails.MediaOwner;
                                    mediaVariablesDetailsObj.ProfileMedia = mediaVariablesDetails.ProfileMedia;
                                    mediaVariablesDetailsObjList.Add(mediaVariablesDetailsObj);

                                }
                                UOWObj.MediaVariablesDetailsRepository.BulKInsert(mediaVariablesDetailsObjList);
                                UOWObj.Save();
                            }
                        }
                    }

                    #endregion

                    var lstTags = answer.TagsInAnswer.ToList();

                    foreach (var tag in lstTags)
                    {
                        var TagsObj = new Db.TagsInAnswer();
                        TagsObj.AnswerOptionsId = answerObj.Id;
                        TagsObj.TagCategoryId = tag.TagCategoryId;
                        TagsObj.TagId = tag.TagId;
                        UOWObj.TagsInAnswerRepository.Insert(TagsObj);
                        UOWObj.Save();
                    }

                    #region insert in Correlation
                    if (quizType == (int)QuizTypeEnum.Personality || quizType == (int)QuizTypeEnum.PersonalityTemplate)
                    {
                        var lstCorrelation = answer.PersonalityAnswerResultMapping.Where(r => r.QuizResults.Status == (int)StatusEnum.Active);
                        foreach (var correlation in lstCorrelation)
                        {
                            var resultMappingObj = new Db.PersonalityAnswerResultMapping();
                            resultMappingObj.AnswerId = mappingLst.FirstOrDefault(a => a.DraftedId == correlation.AnswerId && a.Type == (int)BranchingLogicEnum.ANSWER).PublishedId;
                            resultMappingObj.ResultId = mappingLst.FirstOrDefault(a => a.DraftedId == correlation.ResultId && a.Type == (int)BranchingLogicEnum.RESULT).PublishedId;
                            UOWObj.PersonalityAnswerResultMappingRepository.Insert(resultMappingObj);
                            UOWObj.Save();
                        }
                    }
                    #endregion
                }

                #endregion
            }

            #endregion

            #region insert in Content

            var lstContent = draftedQuizDetailsObj.ContentsInQuiz.Where(r => r.Status == (int)StatusEnum.Active).ToList();

            foreach (var content in lstContent)
            {
                var contentObj = new Db.ContentsInQuiz();

                contentObj.QuizId = currentPublishedQuizDetailId;
                contentObj.ContentTitle = content.ContentTitle;
                contentObj.ShowTitle = content.ShowTitle;
                contentObj.ContentTitleImage = content.ContentTitleImage;
                contentObj.EnableMediaFileForTitle = content.EnableMediaFileForTitle;
                contentObj.PublicIdForContentTitle = content.PublicIdForContentTitle;
                contentObj.ContentDescription = content.ContentDescription;
                contentObj.ShowDescription = content.ShowDescription;
                contentObj.ContentDescriptionImage = content.ContentDescriptionImage;
                contentObj.EnableMediaFileForDescription = content.EnableMediaFileForDescription;
                contentObj.PublicIdForContentDescription = content.PublicIdForContentDescription;
                contentObj.DisplayOrder = content.DisplayOrder;
                contentObj.ShowContentDescriptionImage = content.ShowContentDescriptionImage;
                contentObj.ShowContentTitleImage = content.ShowContentTitleImage;
                contentObj.AliasTextForNextButton = content.AliasTextForNextButton;
                contentObj.EnableNextButton = content.EnableNextButton;
                contentObj.LastUpdatedOn = DateTime.UtcNow;
                contentObj.LastUpdatedBy = businessUserId;
                contentObj.Status = (int)StatusEnum.Active;
                contentObj.State = (int)QuizStateEnum.PUBLISHED;
                contentObj.ViewPreviousQuestion = content.ViewPreviousQuestion;
                contentObj.AutoPlay = content.AutoPlay;
                contentObj.AutoPlayForDescription = content.AutoPlayForDescription;
                contentObj.DisplayOrderForTitle = content.DisplayOrderForTitle;
                contentObj.DisplayOrderForTitleImage = content.DisplayOrderForTitleImage;
                contentObj.DisplayOrderForDescription = content.DisplayOrderForDescription;
                contentObj.DisplayOrderForDescriptionImage = content.DisplayOrderForDescriptionImage;
                contentObj.DisplayOrderForNextButton = content.DisplayOrderForNextButton;

                UOWObj.ContentsInQuizRepository.Insert(contentObj);

                UOWObj.Save();

                mappingLst.Add(new Mappings
                {
                    DraftedId = content.Id,
                    PublishedId = contentObj.Id,
                    Type = (int)BranchingLogicEnum.CONTENT
                });

                #region insert in QuizComponentLogs

                var ComponentObj = new Db.QuizComponentLogs();
                ComponentObj.QuizId = currentPublishedQuizDetailId;
                ComponentObj.DraftedObjectId = content.Id;
                ComponentObj.PublishedObjectId = contentObj.Id;
                ComponentObj.ObjectTypeId = (int)BranchingLogicEnum.CONTENT;
                UOWObj.QuizComponentLogsRepository.Insert(ComponentObj);
                UOWObj.Save();

                #endregion

                #region insert in MediaVariablesDetails

                if (lastpublishedQuizDetailsObj != null && lastpublishedQuizDetailsObj.QuizComponentLogs.Any() && lastpublishedQuizDetailsObj.MediaVariablesDetails.Any())
                {
                    var lastPublishedQuizDetailsObjQuestions = lastpublishedQuizDetailsObj.QuizComponentLogs.FirstOrDefault(r => r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT && r.DraftedObjectId == content.Id);

                    if (lastPublishedQuizDetailsObjQuestions != null)
                    {
                        var mediaVariablesDetailsList = lastpublishedQuizDetailsObj.MediaVariablesDetails.Where(r => r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == lastPublishedQuizDetailsObjQuestions.PublishedObjectId);

                        if (mediaVariablesDetailsList != null)
                        {
                            var mediaVariablesDetailsObjList = new List<Db.MediaVariablesDetails>();
                            foreach (var mediaVariablesDetails in mediaVariablesDetailsList)
                            {
                                var mediaVariablesDetailsObj = new Db.MediaVariablesDetails();
                                mediaVariablesDetailsObj.QuizId = currentPublishedQuizDetailId;
                                mediaVariablesDetailsObj.ObjectId = contentObj.Id;
                                mediaVariablesDetailsObj.ObjectPublicId = mediaVariablesDetails.ObjectPublicId;
                                mediaVariablesDetailsObj.ObjectTypeId = (int)BranchingLogicEnum.CONTENT;
                                mediaVariablesDetailsObj.ObjectValue = mediaVariablesDetails.ObjectValue;
                                mediaVariablesDetailsObj.Type = mediaVariablesDetails.Type;
                                mediaVariablesDetailsObj.ConfigurationDetailsId = mediaVariablesDetails.ConfigurationDetailsId;
                                mediaVariablesDetailsObj.MediaOwner = mediaVariablesDetails.MediaOwner;
                                mediaVariablesDetailsObj.ProfileMedia = mediaVariablesDetails.ProfileMedia;
                                mediaVariablesDetailsObjList.Add(mediaVariablesDetailsObj);
                            }
                            UOWObj.MediaVariablesDetailsRepository.BulKInsert(mediaVariablesDetailsObjList);
                            UOWObj.Save();
                        }
                    }
                }

                #endregion
            }

            #endregion

            #region insert in Action

            var lstAction = draftedQuizDetailsObj.ActionsInQuiz.Where(r => r.Status == (int)StatusEnum.Active).ToList();

            foreach (var action in lstAction)
            {
                var actionObj = new Db.ActionsInQuiz();

                actionObj.QuizId = currentPublishedQuizDetailId;
                actionObj.Title = action.Title;
                actionObj.AppointmentId = action.AppointmentId;
                actionObj.ActionType = action.ActionType;
                actionObj.ReportEmails = action.ReportEmails;
                actionObj.AutomationId = action.AutomationId;
                actionObj.LastUpdatedOn = DateTime.UtcNow;
                actionObj.LastUpdatedBy = businessUserId;
                actionObj.Status = (int)StatusEnum.Active;
                actionObj.State = (int)QuizStateEnum.PUBLISHED;

                UOWObj.ActionsInQuizRepository.Insert(actionObj);

                UOWObj.Save();

                mappingLst.Add(new Mappings
                {
                    DraftedId = action.Id,
                    PublishedId = actionObj.Id,
                    Type = (int)BranchingLogicEnum.ACTION
                });

                #region insert in LinkedCalendarInAction

                foreach (var calObj in action.LinkedCalendarInAction)
                {
                    var linkedCalendarInActionObj = new Db.LinkedCalendarInAction();
                    linkedCalendarInActionObj.ActionsInQuizId = actionObj.Id;
                    linkedCalendarInActionObj.CalendarId = calObj.CalendarId;
                    UOWObj.LinkedCalendarInActionRepository.Insert(linkedCalendarInActionObj);
                }

                UOWObj.Save();

                #endregion

                #region insert in QuizComponentLogs

                var ComponentObj = new Db.QuizComponentLogs();
                ComponentObj.QuizId = currentPublishedQuizDetailId;
                ComponentObj.DraftedObjectId = action.Id;
                ComponentObj.PublishedObjectId = actionObj.Id;
                ComponentObj.ObjectTypeId = (int)BranchingLogicEnum.ACTION;
                UOWObj.QuizComponentLogsRepository.Insert(ComponentObj);
                UOWObj.Save();

                var publishedQuizDetails = lastpublishedQuizDetailsObj;
                if (publishedQuizDetails != null && publishedQuizDetails.QuizComponentLogs != null)
                {
                    var quizComponentLogs = publishedQuizDetails.QuizComponentLogs.FirstOrDefault(r => r.DraftedObjectId == action.Id && r.ObjectTypeId == (int)BranchingLogicEnum.ACTION);

                    if (quizComponentLogs != null)
                    {
                        var leadDataInAction = publishedQuizDetails.ActionsInQuiz.FirstOrDefault(r => r.Id == quizComponentLogs.PublishedObjectId).LeadDataInAction;
                        if (leadDataInAction != null && leadDataInAction.Any())
                        {
                            var leadDataObjList = new List<Db.LeadDataInAction>();
                            foreach (var leadDataInActionObj in leadDataInAction)
                            {
                                var leadDataObj = new Db.LeadDataInAction();
                                leadDataObj.ActionId = actionObj.Id;
                                leadDataObj.LeadUserId = leadDataInActionObj.LeadUserId;
                                leadDataObj.ReportEmails = leadDataInActionObj.ReportEmails;
                                leadDataObj.AppointmentTypeId = leadDataInActionObj.AppointmentTypeId;
                                leadDataObj.IsUpdatedSend = leadDataInActionObj.IsUpdatedSend;
                                leadDataObj.Subject = leadDataInActionObj.Subject;
                                leadDataObj.Body = leadDataInActionObj.Body;
                                leadDataObj.SMSText = leadDataInActionObj.SMSText;
                                leadDataObj.ConfigurationDetailsId = leadDataInActionObj.ConfigurationDetailsId;
                                leadDataObjList.Add(leadDataObj);
                                UOWObj.LeadDataInActionRepository.BulKInsert(leadDataObjList);

                                #region insert in LeadCalendarDataInAction

                                var leadCalendarDataInActionObjList = new List<Db.LeadCalendarDataInAction>();
                                foreach (var leadCalObj in leadDataInActionObj.LeadCalendarDataInAction)
                                {
                                    var leadCalendarDataInActionObj = new Db.LeadCalendarDataInAction();
                                    leadCalendarDataInActionObj.LeadDataInActionId = leadDataObj.Id;
                                    leadCalendarDataInActionObj.CalendarId = leadCalObj.CalendarId;
                                    leadCalendarDataInActionObjList.Add(leadCalendarDataInActionObj);
                                }

                                UOWObj.LeadCalendarDataInActionRepository.BulKInsert(leadCalendarDataInActionObjList);
                                UOWObj.Save();

                                #endregion
                            }
                            UOWObj.Save();
                        }
                        UOWObj.Save();
                    }
                }

                #endregion
            }

            #endregion

            #region insert in Badges

            var lstBadge = draftedQuizDetailsObj.BadgesInQuiz.Where(r => r.Status == (int)StatusEnum.Active).ToList();

            foreach (var badge in lstBadge)
            {
                var badgeObj = new Db.BadgesInQuiz();

                badgeObj.QuizId = currentPublishedQuizDetailId;
                badgeObj.Title = badge.Title;
                badgeObj.ShowTitle = badge.ShowTitle;
                badgeObj.Image = badge.Image;
                badgeObj.ShowImage = badge.ShowImage;
                badgeObj.EnableMediaFile = badge.EnableMediaFile;
                badgeObj.PublicId = badge.PublicId;
                badgeObj.DisplayOrderForTitle = badge.DisplayOrderForTitle;
                badgeObj.DisplayOrderForTitleImage = badge.DisplayOrderForTitleImage;
                badgeObj.LastUpdatedOn = DateTime.UtcNow;
                badgeObj.LastUpdatedBy = businessUserId;
                badgeObj.Status = (int)StatusEnum.Active;
                badgeObj.State = (int)QuizStateEnum.PUBLISHED;
                badgeObj.SecondsToApply = badge.SecondsToApply;
                badgeObj.VideoFrameEnabled = badge.VideoFrameEnabled;

                UOWObj.BadgesInQuizRepository.Insert(badgeObj);

                UOWObj.Save();

                mappingLst.Add(new Mappings
                {
                    DraftedId = badge.Id,
                    PublishedId = badgeObj.Id,
                    Type = (int)BranchingLogicEnum.BADGE
                });

                #region insert in QuizComponentLogs

                var ComponentObj = new Db.QuizComponentLogs();
                ComponentObj.QuizId = currentPublishedQuizDetailId;
                ComponentObj.DraftedObjectId = badge.Id;
                ComponentObj.PublishedObjectId = badgeObj.Id;
                ComponentObj.ObjectTypeId = (int)BranchingLogicEnum.BADGE;
                UOWObj.QuizComponentLogsRepository.Insert(ComponentObj);
                UOWObj.Save();

                #endregion

                #region insert in MediaVariablesDetails

                if (lastpublishedQuizDetailsObj != null && lastpublishedQuizDetailsObj.QuizComponentLogs.Any() && lastpublishedQuizDetailsObj.MediaVariablesDetails.Any())
                {
                    var lastPublishedQuizDetailsObjQuestions = lastpublishedQuizDetailsObj.QuizComponentLogs.FirstOrDefault(r => r.ObjectTypeId == (int)BranchingLogicEnum.BADGE && r.DraftedObjectId == badge.Id);

                    if (lastPublishedQuizDetailsObjQuestions != null)
                    {
                        var mediaVariablesDetailsList = lastpublishedQuizDetailsObj.MediaVariablesDetails.Where(r => r.ObjectTypeId == (int)BranchingLogicEnum.BADGE && r.ObjectId == lastPublishedQuizDetailsObjQuestions.PublishedObjectId);

                        if (mediaVariablesDetailsList != null)
                        {
                            var mediaVariablesDetailsObjList = new List<Db.MediaVariablesDetails>();

                            foreach (var mediaVariablesDetails in mediaVariablesDetailsList)
                            {
                                var mediaVariablesDetailsObj = new Db.MediaVariablesDetails();
                                mediaVariablesDetailsObj.QuizId = currentPublishedQuizDetailId;
                                mediaVariablesDetailsObj.ObjectId = badgeObj.Id;
                                mediaVariablesDetailsObj.ObjectPublicId = mediaVariablesDetails.ObjectPublicId;
                                mediaVariablesDetailsObj.ObjectTypeId = (int)BranchingLogicEnum.BADGE;
                                mediaVariablesDetailsObj.ObjectValue = mediaVariablesDetails.ObjectValue;
                                mediaVariablesDetailsObj.Type = mediaVariablesDetails.Type;
                                mediaVariablesDetailsObj.ConfigurationDetailsId = mediaVariablesDetails.ConfigurationDetailsId;
                                mediaVariablesDetailsObj.MediaOwner = mediaVariablesDetails.MediaOwner;
                                mediaVariablesDetailsObj.ProfileMedia = mediaVariablesDetails.ProfileMedia;
                                mediaVariablesDetailsObjList.Add(mediaVariablesDetailsObj);
                            }

                            UOWObj.MediaVariablesDetailsRepository.BulKInsert(mediaVariablesDetailsObjList);
                            UOWObj.Save();
                        }
                    }
                }

                #endregion
            }

            #endregion

            return mappingLst;
        }

        private void AddPublishQuizBranchingLogic(AutomationUnitOfWork UOWObj, Db.QuizDetails draftedQuizDetailsObj, int businessUserId, int currentPublishedQuizDetailId, int lastPublishedQuizDetailId, int quizType, List<Mappings> mappingLst)
        {
            #region insert in Quiz Branching Logic

            var branchingLogic = draftedQuizDetailsObj.BranchingLogic.ToList();
            var branchingLogiclist = new List<Db.BranchingLogic>();
            foreach (var branching in branchingLogic)
            {
                var SourceObjectId = 0;
                var DestinationObjectId = 0;
                if (branching.SourceTypeId == (int)BranchingLogicEnum.ANSWER || branching.SourceTypeId == (int)BranchingLogicEnum.QUESTION)
                    SourceObjectId = mappingLst.FirstOrDefault(a => a.DraftedId == branching.SourceObjectId && a.Type == branching.SourceTypeId).PublishedId;
                else if (branching.SourceTypeId == (int)BranchingLogicEnum.QUESTIONNEXT)
                    SourceObjectId = mappingLst.FirstOrDefault(a => a.DraftedId == branching.SourceObjectId && a.Type == (int)BranchingLogicEnum.QUESTION).PublishedId;
                else if (branching.SourceTypeId == (int)BranchingLogicEnum.ACTION || branching.SourceTypeId == (int)BranchingLogicEnum.ACTIONNEXT)
                    SourceObjectId = mappingLst.FirstOrDefault(a => a.DraftedId == branching.SourceObjectId && a.Type == ((int)BranchingLogicEnum.ACTION)).PublishedId;
                else if (branching.SourceTypeId == (int)BranchingLogicEnum.CONTENT || branching.SourceTypeId == (int)BranchingLogicEnum.CONTENTNEXT)
                    SourceObjectId = mappingLst.FirstOrDefault(a => a.DraftedId == branching.SourceObjectId && a.Type == ((int)BranchingLogicEnum.CONTENT)).PublishedId;
                else if (branching.SourceTypeId == (int)BranchingLogicEnum.RESULT || branching.SourceTypeId == (int)BranchingLogicEnum.RESULTNEXT)
                    SourceObjectId = mappingLst.FirstOrDefault(a => a.DraftedId == branching.SourceObjectId && a.Type == ((int)BranchingLogicEnum.RESULT)).PublishedId;
                else if (branching.SourceTypeId == (int)BranchingLogicEnum.BADGE || branching.SourceTypeId == (int)BranchingLogicEnum.BADGENEXT)
                    SourceObjectId = mappingLst.FirstOrDefault(a => a.DraftedId == branching.SourceObjectId && a.Type == ((int)BranchingLogicEnum.BADGE)).PublishedId;
                else if (branching.SourceTypeId == (int)BranchingLogicEnum.WHATSAPPTEMPLATE)
                    SourceObjectId = mappingLst.FirstOrDefault(a => a.DraftedId == branching.SourceObjectId && a.Type == branching.SourceTypeId).PublishedId;
                else if (branching.SourceTypeId == (int)BranchingLogicEnum.WHATSAPPTEMPLATEACTION)
                    SourceObjectId = mappingLst.FirstOrDefault(a => a.DraftedId == branching.SourceObjectId && a.Type == branching.SourceTypeId).PublishedId;

                if (branching.DestinationTypeId == (int)BranchingLogicEnum.ANSWER || branching.DestinationTypeId == (int)BranchingLogicEnum.QUESTION)
                    DestinationObjectId = mappingLst.FirstOrDefault(a => a.DraftedId == branching.DestinationObjectId && a.Type == branching.DestinationTypeId).PublishedId;
                else if (branching.DestinationTypeId == (int)BranchingLogicEnum.QUESTIONNEXT)
                    DestinationObjectId = mappingLst.FirstOrDefault(a => a.DraftedId == branching.DestinationObjectId && a.Type == (int)BranchingLogicEnum.QUESTION).PublishedId;
                else if (branching.DestinationTypeId == (int)BranchingLogicEnum.ACTION || branching.DestinationTypeId == (int)BranchingLogicEnum.ACTIONNEXT)
                    DestinationObjectId = mappingLst.FirstOrDefault(a => a.DraftedId == branching.DestinationObjectId && a.Type == ((int)BranchingLogicEnum.ACTION)).PublishedId;
                else if (branching.DestinationTypeId == (int)BranchingLogicEnum.CONTENT || branching.DestinationTypeId == (int)BranchingLogicEnum.CONTENTNEXT)
                    DestinationObjectId = mappingLst.FirstOrDefault(a => a.DraftedId == branching.DestinationObjectId && a.Type == ((int)BranchingLogicEnum.CONTENT)).PublishedId;
                else if (branching.DestinationTypeId == (int)BranchingLogicEnum.RESULT || branching.DestinationTypeId == (int)BranchingLogicEnum.RESULTNEXT)
                    DestinationObjectId = mappingLst.FirstOrDefault(a => a.DraftedId == branching.DestinationObjectId && a.Type == ((int)BranchingLogicEnum.RESULT)).PublishedId;
                else if (branching.DestinationTypeId == (int)BranchingLogicEnum.BADGE || branching.DestinationTypeId == (int)BranchingLogicEnum.BADGENEXT)
                    DestinationObjectId = mappingLst.FirstOrDefault(a => a.DraftedId == branching.DestinationObjectId && a.Type == ((int)BranchingLogicEnum.BADGE)).PublishedId;

                if (branching.DestinationTypeId == (int)BranchingLogicEnum.WHATSAPPTEMPLATE || branching.DestinationTypeId == (int)BranchingLogicEnum.WHATSAPPTEMPLATEACTION)
                    DestinationObjectId = mappingLst.FirstOrDefault(a => a.DraftedId == branching.DestinationObjectId && a.Type == branching.DestinationTypeId).PublishedId;

                var branchingObj = new Db.BranchingLogic()
                {
                    QuizId = currentPublishedQuizDetailId,
                    SourceTypeId = branching.SourceTypeId,
                    SourceObjectId = SourceObjectId,
                    DestinationTypeId = branching.DestinationTypeId,
                    DestinationObjectId = DestinationObjectId,
                    IsStartingPoint = branching.IsStartingPoint,
                    IsEndPoint = branching.IsEndPoint
                };

                branchingLogiclist.Add(branchingObj);

            }
            if (branchingLogiclist != null)
            {
                UOWObj.BranchingLogicRepository.BulKInsert(branchingLogiclist);
                UOWObj.Save();
            }

            #endregion

        }


        private void AddPublishQuizVariableInQuiz(AutomationUnitOfWork UOWObj, Db.QuizDetails draftedQuizDetailsObj, int businessUserId, int currentPublishedQuizDetailId, int lastPublishedQuizDetailId)
        {
            #region insert into VariableInQuiz

            var lstVariableInQuiz = draftedQuizDetailsObj.VariableInQuiz.Where(t => t.NumberOfUses > 0);
            var VariableInQuizlist = new List<Db.VariableInQuiz>();
            foreach (var variable in lstVariableInQuiz)
            {
                var variableInQuizObj = new Db.VariableInQuiz();

                variableInQuizObj.QuizId = currentPublishedQuizDetailId;
                variableInQuizObj.NumberOfUses = variable.NumberOfUses;
                variableInQuizObj.VariableId = variable.VariableId;
                VariableInQuizlist.Add(variableInQuizObj);
            }
            if (VariableInQuizlist != null && VariableInQuizlist.Any())
            {
                UOWObj.VariableInQuizRepository.BulKInsert(VariableInQuizlist);
            }

            UOWObj.Save();

            #endregion

        }

        public string PublishQuiz(int QuizId, int BusinessUserId, int CompanyId)
        {
            string publishedCode = string.Empty;
            int draftquizDetailid = 0;
            int lastPublishedquizDetailid = 0;
            int quizType = 0;

            using (var UOWObj = new AutomationUnitOfWork())
            {
                var quizObj = UOWObj.QuizRepository.GetByID(QuizId);
                if (quizObj == null)
                {
                    Status = ResultEnum.Error;
                    ErrorMessage = "Quiz not found for the QuizId " + QuizId;
                    return publishedCode;
                }


                if (quizObj != null)
                {
                    quizType = quizObj.QuizType;
                    var draftedQuizDetailsObj = quizObj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED);
                    if (draftedQuizDetailsObj != null)
                    {
                        if (!(quizObj.QuizType != (int)QuizTypeEnum.NPS || draftedQuizDetailsObj.QuestionsInQuiz.Any(r => r.Status == (int)StatusEnum.Active && r.AnswerType == (int)AnswerTypeEnum.NPS)))
                        {

                            Status = ResultEnum.Error;
                            ErrorMessage = "There should be at least 1 NPS type question";
                            return publishedCode;
                        }


                        draftquizDetailid = draftedQuizDetailsObj.Id;
                        var lastpublishedQuizDetailsObj = quizObj.QuizDetails.Where(r => r.State == (int)QuizStateEnum.PUBLISHED).OrderByDescending(r => r.Version).FirstOrDefault();
                        lastPublishedquizDetailid = lastpublishedQuizDetailsObj != null ? lastpublishedQuizDetailsObj.Id : 0;

                    }

                }
            }

            int versionNumber = 0;
            int currentPublishedQuizDetailId = 0;
            if (draftquizDetailid != 0)
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var draftedQuizDetailsObj = UOWObj.QuizDetailsRepository.GetByID(draftquizDetailid);
                    versionNumber = draftedQuizDetailsObj.Version + 1;
                    draftedQuizDetailsObj.Version = draftedQuizDetailsObj.Version + 1;
                    UOWObj.QuizDetailsRepository.Update(draftedQuizDetailsObj);
                    UOWObj.Save();

                    currentPublishedQuizDetailId = AddPublishQuizDetails(UOWObj, draftedQuizDetailsObj, versionNumber, BusinessUserId);
                    if (currentPublishedQuizDetailId == 0)
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz unable to publish " + QuizId;
                        return publishedCode;
                    }

                    if(draftedQuizDetailsObj != null)
                    {
                        AddQuizVariables(currentPublishedQuizDetailId, UOWObj, draftedQuizDetailsObj);
                    }
                    
                    using (var transaction = Utility.CreateTransactionScope())
                    {
                        AddPublishQuizMediaVariables(UOWObj, lastPublishedquizDetailid, currentPublishedQuizDetailId, draftquizDetailid);

                        AddQuizPersonalityResult(UOWObj, draftedQuizDetailsObj, BusinessUserId, currentPublishedQuizDetailId, quizType);
                        UOWObj.Save();
                        transaction.Complete();

                    }
                }
            }

            if (draftquizDetailid != 0 && currentPublishedQuizDetailId != 0)
            {
                List<Mappings> mappingLst = new List<Mappings>();
                try
                {
                    using (var UOWObj = new AutomationUnitOfWork())
                    {
                        var draftedQuizDetailsObj = UOWObj.QuizDetailsRepository.GetByID(draftquizDetailid);
                        using (var transaction = Utility.CreateTransactionScope())
                        {
                            var mappingLstresult = AddPublishQuizResultDetails(UOWObj, draftedQuizDetailsObj, BusinessUserId, currentPublishedQuizDetailId, lastPublishedquizDetailid);
                            mappingLst.AddRange(mappingLstresult);

                            var mappingLstresultQuestion = AddPublishQuizQuestioncontactAction(UOWObj, draftedQuizDetailsObj, BusinessUserId, currentPublishedQuizDetailId, lastPublishedquizDetailid, quizType);
                            mappingLst.AddRange(mappingLstresultQuestion);

                            AddPublishQuizBranchingLogic(UOWObj, draftedQuizDetailsObj, BusinessUserId, currentPublishedQuizDetailId, lastPublishedquizDetailid, quizType, mappingLst);

                            AddPublishQuizVariableInQuiz(UOWObj, draftedQuizDetailsObj, BusinessUserId, currentPublishedQuizDetailId, lastPublishedquizDetailid);
                            UOWObj.Save();
                            transaction.Complete();

                        }
                    }
                }
                catch (Exception ex)
                {

                    DeleteRollBackTransaction(currentPublishedQuizDetailId);
                    Status = ResultEnum.Error;
                    ErrorMessage = ex.Message;
                    throw ex;
                }

            }


            if (draftquizDetailid != 0 && currentPublishedQuizDetailId != 0)
            {
                try
                {
                    using (var UOWObj = new AutomationUnitOfWork())
                    {

                        var quizObj = UOWObj.QuizRepository.GetByID(QuizId);
                        using (var transaction = Utility.CreateTransactionScope())
                        {

                            quizObj.State = (int)QuizStateEnum.PUBLISHED;

                            if (string.IsNullOrEmpty(quizObj.PublishedCode)) quizObj.PublishedCode = Guid.NewGuid().ToString();

                            UOWObj.QuizRepository.Update(quizObj);
                            UOWObj.Save();
                            transaction.Complete();

                        }

                        publishedCode = quizObj.PublishedCode;

                        
                        
                    }
                }
                catch (Exception ex)
                {
                    DeleteRollBackTransaction(currentPublishedQuizDetailId);
                    Status = ResultEnum.Error;
                    ErrorMessage = ex.Message;
                    throw ex;
                }
            }

            return publishedCode;
            
        }

        private void AddQuizVariables(int currentPublishedQuizDetailId, AutomationUnitOfWork UOWObj, Db.QuizDetails draftedQuizDetailsObj)
        {
            var currentQuizVar = new Db.QuizVariables();
            var draftedQuizVar = UOWObj.QuizVariablesRepository.GetQuizVariablesByQuizId(draftedQuizDetailsObj.Id);
            if (currentPublishedQuizDetailId != 0 && draftedQuizVar != null)
            {
                foreach (var item in draftedQuizVar)
                {
                    if (item.ObjectTypes == (int)QuizVariableObjectTypes.COVER)
                    {
                        currentQuizVar.QuizDetailsId = currentPublishedQuizDetailId;
                        currentQuizVar.ObjectId = currentPublishedQuizDetailId;
                        currentQuizVar.ObjectTypes = item.ObjectTypes;
                        currentQuizVar.CompanyId = item.CompanyId;
                        currentQuizVar.Variables = item.Variables;
                        UOWObj.QuizVariablesRepository.Insert(currentQuizVar);
                        UOWObj.Save();
                    }
                    else if (item.ObjectTypes != (int)QuizVariableObjectTypes.QUESTION && item.ObjectTypes != (int)QuizVariableObjectTypes.RESULT)
                    {
                        currentQuizVar.QuizDetailsId = currentPublishedQuizDetailId;
                        currentQuizVar.ObjectId = item.ObjectId;
                        currentQuizVar.ObjectTypes = item.ObjectTypes;
                        currentQuizVar.CompanyId = item.CompanyId;
                        currentQuizVar.Variables = item.Variables;
                        UOWObj.QuizVariablesRepository.Insert(currentQuizVar);
                        UOWObj.Save();

                    }
                }
            }
        }

        private void DeleteRollBackTransaction(int currentPublishedQuizDetailId)
        {
            using (var UOWObj = new AutomationUnitOfWork())
            {

                string query = $@"delete  from VariableInQuiz where QuizId ={currentPublishedQuizDetailId}";
                UOWObj.VariableInQuizRepository.DeleteRange(query);
                UOWObj.Save();

                query = $@"delete  from MediaVariablesDetails where QuizId ={currentPublishedQuizDetailId}";
                UOWObj.MediaVariablesDetailsRepository.DeleteRange(query);
                UOWObj.Save();

                query = $@"delete  from PersonalityResultSetting where QuizId ={currentPublishedQuizDetailId}";
                UOWObj.PersonalityResultSettingRepository.DeleteRange(query);
                UOWObj.Save();

                query = $@"delete  from QuizResults where QuizId ={currentPublishedQuizDetailId}";
                UOWObj.QuizResultsRepository.DeleteRange(query);
                UOWObj.Save();

                query = $@"delete  from QuizBrandingAndStyle where QuizId ={currentPublishedQuizDetailId}";
                UOWObj.QuizBrandingAndStyleRepository.DeleteRange(query);
                UOWObj.Save();

                query = $@"delete  from QuestionsInQuiz where QuizId ={currentPublishedQuizDetailId}";
                UOWObj.QuestionsInQuizRepository.DeleteRange(query);
                UOWObj.Save();

                query = $@"delete  from BranchingLogic where QuizId ={currentPublishedQuizDetailId}";
                UOWObj.BranchingLogicRepository.DeleteRange(query);
                UOWObj.Save();

                query = $@"delete  from ContentsInQuiz where QuizId ={currentPublishedQuizDetailId}";
                UOWObj.ContentsInQuizRepository.DeleteRange(query);
                UOWObj.Save();

                query = $@"delete  from ActionsInQuiz where QuizId ={currentPublishedQuizDetailId}";
                UOWObj.ActionsInQuizRepository.DeleteRange(query);
                UOWObj.Save();

                query = $@"delete  from BadgesInQuiz where QuizId ={currentPublishedQuizDetailId}";
                UOWObj.BadgesInQuizRepository.DeleteRange(query);
                UOWObj.Save();

                query = $@"delete  from QuizComponentLogs where QuizId ={currentPublishedQuizDetailId}";
                UOWObj.QuizComponentLogsRepository.DeleteRange(query);
                UOWObj.Save();

                query = $@"delete  from QuizDetails where Id ={currentPublishedQuizDetailId}";
                UOWObj.QuizDetailsRepository.DeleteRange(query);
                UOWObj.Save();

                query = $@"delete  from QuizVariables where quizdetailsid ={currentPublishedQuizDetailId}";
                UOWObj.QuizVariablesRepository.DeleteRange(query);
                UOWObj.Save();

            }
        }


    }
}