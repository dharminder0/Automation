using Core.Common.Extensions;
using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Response
{

    public class AttemptQuizSettingResponse : IResponse
    {
        public string SourceType { get; set; }
        public string SourceId { get; set; }
        public string SourceName { get; set; }
        public string PrivacyLink { get; set; }
        public PrivacyDto PrivacyJson { get; set; }
        public string ConfigurationType { get; set; }
        public string ConfigurationId { get; set; }
        public string PrimaryBrandingColor { get; set; }
        public string SecondaryBrandingColor { get; set; }
        public string LeadFormTitle { get; set; }
        public string GtmCode { get; set; }
        public string FavoriteIconUrl { get; set; }
        public string OfficeId { get; set; }
        public string CompanyCode { get; set; }
        public List<Tags> Tag { get; set; }
        public QuizBrandingAndStyleResponse QuizBrandingAndStyle { get; set; }
        public AttemptQuizCover QuizCoverDetails { get; set; }
        public bool LoadQuizDetails { get; set; }
        public List<int> UsageType { get; set; }
        public IResponse MapEntityToResponse(Base EntityObj)
        {
            AttemptQuizSettingResponse response = new AttemptQuizSettingResponse();

            var quizAnswerSubmitObj = (QuizAnswerSubmit)EntityObj;
            response.CompanyCode = quizAnswerSubmitObj.CompanyCode;
            response.PrimaryBrandingColor = quizAnswerSubmitObj.PrimaryBrandingColor;
            response.SecondaryBrandingColor = quizAnswerSubmitObj.SecondaryBrandingColor;
            response.ConfigurationType = quizAnswerSubmitObj.ConfigurationType;
            response.ConfigurationId = quizAnswerSubmitObj.ConfigurationId;
            response.GtmCode = quizAnswerSubmitObj.GtmCode;
            response.FavoriteIconUrl = quizAnswerSubmitObj.FavoriteIconUrl;
            response.OfficeId = quizAnswerSubmitObj.OfficeId;

            response.SourceId = quizAnswerSubmitObj.SourceId;
            response.SourceName = quizAnswerSubmitObj.SourceName;
            response.SourceType = quizAnswerSubmitObj.SourceType;
            response.PrivacyLink = quizAnswerSubmitObj.PrivacyLink;
            response.PrivacyJson = quizAnswerSubmitObj.PrivacyJson;
            response.LeadFormTitle = quizAnswerSubmitObj.LeadFormTitle;
            response.LoadQuizDetails = quizAnswerSubmitObj.LoadQuizDetails;
            response.UsageType = quizAnswerSubmitObj.UsageType;
            if (quizAnswerSubmitObj.Tag != null)
            {
                response.Tag = new List<Tags>();
                foreach (var tag in quizAnswerSubmitObj.Tag)
                {
                    response.Tag.Add(tag);
                }
            }

            if (quizAnswerSubmitObj.QuizCoverDetails != null)
            {
                response.QuizCoverDetails = new AttemptQuizCover();

                response.QuizCoverDetails.QuizTitle = quizAnswerSubmitObj.QuizCoverDetails.QuizTitle;
                response.QuizCoverDetails.QuizType = quizAnswerSubmitObj.QuizCoverDetails.QuizType;
                response.QuizCoverDetails.QuizCoverTitle = quizAnswerSubmitObj.QuizCoverDetails.QuizCoverTitle;
                response.QuizCoverDetails.ShowQuizCoverTitle = quizAnswerSubmitObj.QuizCoverDetails.ShowQuizCoverTitle;
                response.QuizCoverDetails.QuizCoverImage = quizAnswerSubmitObj.QuizCoverDetails.QuizCoverImage;
                response.QuizCoverDetails.ShowQuizCoverImage = quizAnswerSubmitObj.QuizCoverDetails.ShowQuizCoverImage;
                response.QuizCoverDetails.PublicIdForQuizCover = quizAnswerSubmitObj.QuizCoverDetails.PublicIdForQuizCover;
                response.QuizCoverDetails.QuizCoverImgXCoordinate = quizAnswerSubmitObj.QuizCoverDetails.QuizCoverImgXCoordinate;
                response.QuizCoverDetails.QuizCoverImgYCoordinate = quizAnswerSubmitObj.QuizCoverDetails.QuizCoverImgYCoordinate;
                response.QuizCoverDetails.QuizCoverImgHeight = quizAnswerSubmitObj.QuizCoverDetails.QuizCoverImgHeight;
                response.QuizCoverDetails.QuizCoverImgWidth = quizAnswerSubmitObj.QuizCoverDetails.QuizCoverImgWidth;
                response.QuizCoverDetails.QuizCoverImgAttributionLabel = quizAnswerSubmitObj.QuizCoverDetails.QuizCoverImgAttributionLabel;
                response.QuizCoverDetails.QuizCoverImgAltTag = quizAnswerSubmitObj.QuizCoverDetails.QuizCoverImgAltTag;
                response.QuizCoverDetails.QuizDescription = quizAnswerSubmitObj.QuizCoverDetails.QuizDescription;
                response.QuizCoverDetails.ShowDescription = quizAnswerSubmitObj.QuizCoverDetails.ShowDescription;
                response.QuizCoverDetails.QuizStartButtonText = quizAnswerSubmitObj.QuizCoverDetails.QuizStartButtonText;
                response.QuizCoverDetails.AutoPlay = quizAnswerSubmitObj.QuizCoverDetails.AutoPlay;
                response.QuizCoverDetails.SecondsToApply = quizAnswerSubmitObj.QuizCoverDetails.SecondsToApply;
                response.QuizCoverDetails.VideoFrameEnabled = quizAnswerSubmitObj.QuizCoverDetails.VideoFrameEnabled;
                response.QuizCoverDetails.EnableNextButton = quizAnswerSubmitObj.QuizCoverDetails.EnableNextButton;
                response.QuizCoverDetails.DisplayOrderForTitleImage = quizAnswerSubmitObj.QuizCoverDetails.DisplayOrderForTitleImage;
                response.QuizCoverDetails.DisplayOrderForTitle = quizAnswerSubmitObj.QuizCoverDetails.DisplayOrderForTitle;
                response.QuizCoverDetails.DisplayOrderForDescription = quizAnswerSubmitObj.QuizCoverDetails.DisplayOrderForDescription;
                response.QuizCoverDetails.DisplayOrderForNextButton = quizAnswerSubmitObj.QuizCoverDetails.DisplayOrderForNextButton;
            }

            if (quizAnswerSubmitObj.QuizBrandingAndStyle != null)
            {
                response.QuizBrandingAndStyle = new QuizBrandingAndStyleResponse();
                response.QuizBrandingAndStyle.QuizId = quizAnswerSubmitObj.QuizBrandingAndStyle.QuizId;
                response.QuizBrandingAndStyle.ImageFileURL = quizAnswerSubmitObj.QuizBrandingAndStyle.ImageFileURL ?? string.Empty;
                response.QuizBrandingAndStyle.PublicIdForImageFile = quizAnswerSubmitObj.QuizBrandingAndStyle.PublicIdForFileURL ?? string.Empty;
                response.QuizBrandingAndStyle.BackgroundColor = quizAnswerSubmitObj.QuizBrandingAndStyle.BackgroundColor ?? string.Empty;
                response.QuizBrandingAndStyle.ButtonColor = quizAnswerSubmitObj.QuizBrandingAndStyle.ButtonColor ?? string.Empty;
                response.QuizBrandingAndStyle.OptionColor = quizAnswerSubmitObj.QuizBrandingAndStyle.OptionColor ?? string.Empty;
                response.QuizBrandingAndStyle.ButtonFontColor = quizAnswerSubmitObj.QuizBrandingAndStyle.ButtonFontColor ?? string.Empty;
                response.QuizBrandingAndStyle.OptionFontColor = quizAnswerSubmitObj.QuizBrandingAndStyle.OptionFontColor ?? string.Empty;
                response.QuizBrandingAndStyle.FontColor = quizAnswerSubmitObj.QuizBrandingAndStyle.FontColor ?? string.Empty;
                response.QuizBrandingAndStyle.FontType = quizAnswerSubmitObj.QuizBrandingAndStyle.FontType ?? string.Empty;
                response.QuizBrandingAndStyle.BackgroundColorofSelectedAnswer = quizAnswerSubmitObj.QuizBrandingAndStyle.BackgroundColorofSelectedAnswer ?? string.Empty;
                response.QuizBrandingAndStyle.BackgroundColorofAnsweronHover = quizAnswerSubmitObj.QuizBrandingAndStyle.BackgroundColorofAnsweronHover ?? string.Empty;
                response.QuizBrandingAndStyle.AnswerTextColorofSelectedAnswer = quizAnswerSubmitObj.QuizBrandingAndStyle.AnswerTextColorofSelectedAnswer ?? string.Empty;
                response.QuizBrandingAndStyle.IsBackType = quizAnswerSubmitObj.QuizBrandingAndStyle.IsBackType;
                response.QuizBrandingAndStyle.BackImageFileURL = quizAnswerSubmitObj.QuizBrandingAndStyle.BackImageFileURL ?? string.Empty;
                response.QuizBrandingAndStyle.BackColor = quizAnswerSubmitObj.QuizBrandingAndStyle.BackColor ?? string.Empty;
                response.QuizBrandingAndStyle.Opacity = quizAnswerSubmitObj.QuizBrandingAndStyle.Opacity ?? string.Empty;
                response.QuizBrandingAndStyle.LogoUrl = quizAnswerSubmitObj.QuizBrandingAndStyle.LogoUrl ?? string.Empty;
                response.QuizBrandingAndStyle.LogoPublicId = quizAnswerSubmitObj.QuizBrandingAndStyle.LogoPublicId ?? string.Empty;
                response.QuizBrandingAndStyle.BackgroundColorofLogo = quizAnswerSubmitObj.QuizBrandingAndStyle.BackgroundColorofLogo ?? string.Empty;
                response.QuizBrandingAndStyle.AutomationAlignment = quizAnswerSubmitObj.QuizBrandingAndStyle.AutomationAlignment ?? string.Empty;
                response.QuizBrandingAndStyle.LogoAlignment = quizAnswerSubmitObj.QuizBrandingAndStyle.LogoAlignment ?? string.Empty;
                response.QuizBrandingAndStyle.Flip = quizAnswerSubmitObj.QuizBrandingAndStyle.Flip;
                response.QuizBrandingAndStyle.Language = quizAnswerSubmitObj.QuizBrandingAndStyle.Language;
            }

           return response;
        }
    }

    public class AttemptQuizV2Response : IResponse
    {
        public string AppointmentCode { get; set; }
        public string AppointmentLink { get; set; }
        public string AppointmentErrorMessage { get; set; }
        public int? QuestionType { get; set; }
        public bool IsBackButtonEnable { get; set; }
        public int? FormId { get; set; }
        public int? FlowOrder { get; set; }
        public QuizApp.Response.AttemptQuizResponse.Question QuestionDetails { get; set; }
        public QuizApp.Response.AttemptQuizResponse.QuizContent ContentDetails { get; set; }
        public QuizApp.Response.AttemptQuizResponse.QuizBadge BadgeDetails { get; set; }
        public QuizApp.Response.AttemptQuizResponse.SubmittedAnswerResult SubmittedAnswer { get; set; }
        public QuizApp.Response.AttemptQuizResponse.SubmittedAnswerResult PreviousQuestionSubmittedAnswer { get; set; }
        public QuizApp.Response.AttemptQuizResponse.Result ResultScore { get; set; }
        public bool ShowLeadUserForm { get; set; }
        public string CompanyCode { get; set; }
        public bool LoadQuizDetails { get; set; }

        public IResponse MapEntityToResponse(Base EntityObj)
        {
            AttemptQuizV2Response response = new AttemptQuizV2Response();
            var quizAnswerSubmitObj = (QuizAnswerSubmit)EntityObj;
            response.AppointmentCode = quizAnswerSubmitObj.AppointmentCode;
            response.AppointmentLink = quizAnswerSubmitObj.AppointmentLink;
            response.AppointmentErrorMessage = quizAnswerSubmitObj.AppointmentErrorMessage;
            response.ShowLeadUserForm = quizAnswerSubmitObj.ShowLeadUserForm;
            response.CompanyCode = quizAnswerSubmitObj.CompanyCode;
            response.IsBackButtonEnable = quizAnswerSubmitObj.IsBackButtonEnable;
            response.QuestionType = quizAnswerSubmitObj.QuestionType;
            response.FormId = quizAnswerSubmitObj.FormId;
            response.FlowOrder = quizAnswerSubmitObj.FlowOrder;
            response.LoadQuizDetails = quizAnswerSubmitObj.LoadQuizDetails;

            if (quizAnswerSubmitObj.QuestionDetails != null)
            {
                response.QuestionDetails = new QuizApp.Response.AttemptQuizResponse.Question();

                response.QuestionDetails.QuestionId = quizAnswerSubmitObj.QuestionDetails.QuestionId;
                response.QuestionDetails.QuestionTitle = quizAnswerSubmitObj.QuestionDetails.QuestionTitle;
                response.QuestionDetails.ShowTitle = quizAnswerSubmitObj.QuestionDetails.ShowTitle;
                response.QuestionDetails.QuestionImage = quizAnswerSubmitObj.QuestionDetails.QuestionImage;
                response.QuestionDetails.PublicIdForQuestion = quizAnswerSubmitObj.QuestionDetails.PublicIdForQuestion;
                response.QuestionDetails.ShowQuestionImage = quizAnswerSubmitObj.QuestionDetails.ShowQuestionImage.ToBoolValue();
                response.QuestionDetails.ShowAnswerImage = quizAnswerSubmitObj.QuestionDetails.ShowAnswerImage.ToBoolValue();
                response.QuestionDetails.MinAnswer = quizAnswerSubmitObj.QuestionDetails.MinAnswer;
                response.QuestionDetails.MaxAnswer = quizAnswerSubmitObj.QuestionDetails.MaxAnswer;
                response.QuestionDetails.AnswerType = quizAnswerSubmitObj.QuestionDetails.AnswerType;
                response.QuestionDetails.NextButtonColor = quizAnswerSubmitObj.QuestionDetails.NextButtonColor;
                response.QuestionDetails.NextButtonText = quizAnswerSubmitObj.QuestionDetails.NextButtonText;
                response.QuestionDetails.NextButtonTxtColor = quizAnswerSubmitObj.QuestionDetails.NextButtonTxtColor;
                response.QuestionDetails.NextButtonTxtSize = quizAnswerSubmitObj.QuestionDetails.NextButtonTxtSize;
                response.QuestionDetails.EnableNextButton = quizAnswerSubmitObj.QuestionDetails.EnableNextButton;
                response.QuestionDetails.ViewPreviousQuestion = quizAnswerSubmitObj.QuestionDetails.ViewPreviousQuestion;
                response.QuestionDetails.EditAnswer = quizAnswerSubmitObj.QuestionDetails.EditAnswer;
                response.QuestionDetails.StartedOn = quizAnswerSubmitObj.QuestionDetails.StartedOn;
                response.QuestionDetails.TimerRequired = quizAnswerSubmitObj.QuestionDetails.TimerRequired;
                response.QuestionDetails.Time = quizAnswerSubmitObj.QuestionDetails.Time;
                response.QuestionDetails.AutoPlay = quizAnswerSubmitObj.QuestionDetails.AutoPlay;
                response.QuestionDetails.DisplayOrderForTitle = quizAnswerSubmitObj.QuestionDetails.DisplayOrderForTitle;
                response.QuestionDetails.DisplayOrderForTitleImage = quizAnswerSubmitObj.QuestionDetails.DisplayOrderForTitleImage;
                response.QuestionDetails.DisplayOrderForDescription = quizAnswerSubmitObj.QuestionDetails.DisplayOrderForDescription;
                response.QuestionDetails.DisplayOrderForDescriptionImage = quizAnswerSubmitObj.QuestionDetails.DisplayOrderForDescriptionImage;
                response.QuestionDetails.DisplayOrderForAnswer = quizAnswerSubmitObj.QuestionDetails.DisplayOrderForAnswer;
                response.QuestionDetails.DisplayOrderForNextButton = quizAnswerSubmitObj.QuestionDetails.DisplayOrderForNextButton;
                response.QuestionDetails.Description = quizAnswerSubmitObj.QuestionDetails.Description;
                response.QuestionDetails.ShowDescription = quizAnswerSubmitObj.QuestionDetails.ShowDescription;
                response.QuestionDetails.DescriptionImage = quizAnswerSubmitObj.QuestionDetails.DescriptionImage;
                response.QuestionDetails.ShowDescriptionImage = quizAnswerSubmitObj.QuestionDetails.ShowDescriptionImage;
                response.QuestionDetails.PublicIdForDescription = quizAnswerSubmitObj.QuestionDetails.PublicIdForDescription;
                response.QuestionDetails.AutoPlayForDescription = quizAnswerSubmitObj.QuestionDetails.AutoPlayForDescription;
                response.QuestionDetails.EnableComment = quizAnswerSubmitObj.QuestionDetails.EnableComment;
                response.QuestionDetails.SecondsToApply = quizAnswerSubmitObj.QuestionDetails.SecondsToApply;
                response.QuestionDetails.VideoFrameEnabled = quizAnswerSubmitObj.QuestionDetails.VideoFrameEnabled;
                response.QuestionDetails.SecondsToApplyForDescription = quizAnswerSubmitObj.QuestionDetails.SecondsToApplyForDescription;
                response.QuestionDetails.DescVideoFrameEnabled = quizAnswerSubmitObj.QuestionDetails.DescVideoFrameEnabled;
                response.QuestionDetails.AnswerStructureType = quizAnswerSubmitObj.QuestionDetails.AnswerStructureType.HasValue ? quizAnswerSubmitObj.QuestionDetails.AnswerStructureType : 0;
                response.QuestionDetails.WhatsappDetails = quizAnswerSubmitObj.QuestionDetails.WhatsappDetails;
                response.QuestionDetails.IsMultiRating = quizAnswerSubmitObj.QuestionDetails.IsMultiRating;
                response.QuestionDetails.LanguageCode = quizAnswerSubmitObj.QuestionDetails.LanguageCode;

                if (quizAnswerSubmitObj.QuestionDetails.AnswerList != null)
                {
                    response.QuestionDetails.AnswerOption = new List<QuizApp.Response.AttemptQuizResponse.Answer>();

                    foreach (var item in quizAnswerSubmitObj.QuestionDetails.AnswerList)
                    {
                        response.QuestionDetails.AnswerOption.Add(new QuizApp.Response.AttemptQuizResponse.Answer
                        {
                            AnswerId = item.AnswerId,
                            ImageURL = item.AnswerImage,
                            PublicIdForAnswer = item.PublicIdForAnswer,
                            Title = item.AnswerText,
                            AnswerDescription = item.AnswerDescription,
                            IsCorrectAnswer = item.IsCorrectAnswer.HasValue && item.IsCorrectAnswer.Value,
                            DisplayOrder = item.DisplayOrder,
                            IsUnansweredType = item.IsUnansweredType,
                            AutoPlay = item.AutoPlay,
                            ListValues = !string.IsNullOrWhiteSpace(item.ListValues) ? item.ListValues.Split(',').ToList() : new List<string>(),
                            //for Rating type question
                            OptionTextforRatingOne = item.OptionTextforRatingOne,
                            OptionTextforRatingTwo = item.OptionTextforRatingTwo,
                            OptionTextforRatingThree = item.OptionTextforRatingThree,
                            OptionTextforRatingFour = item.OptionTextforRatingFour,
                            OptionTextforRatingFive = item.OptionTextforRatingFive,
                            OptionTextforRating1 = item.OptionTextforRatingOne,
                            OptionTextforRating2 = item.OptionTextforRatingTwo,
                            OptionTextforRating3 = item.OptionTextforRatingThree,
                            OptionTextforRating4 = item.OptionTextforRatingFour,
                            OptionTextforRating5 = item.OptionTextforRatingFive,
                            SecondsToApply = item.SecondsToApply,
                            VideoFrameEnabled = item.VideoFrameEnabled
                        });
                    }
                }
            }

            if (quizAnswerSubmitObj.ContentDetails != null)
            {
                response.ContentDetails = new QuizApp.Response.AttemptQuizResponse.QuizContent()
                {
                    AliasTextForNextButton = quizAnswerSubmitObj.ContentDetails.AliasTextForNextButton,
                    EnableNextButton = quizAnswerSubmitObj.ContentDetails.EnableNextButton,
                    ContentDescription = quizAnswerSubmitObj.ContentDetails.ContentDescription,
                    ShowDescription = quizAnswerSubmitObj.ContentDetails.ShowDescription,
                    ContentDescriptionImage = quizAnswerSubmitObj.ContentDetails.ContentDescriptionImage,
                    PublicIdForContentDescription = quizAnswerSubmitObj.ContentDetails.PublicIdForContentDescription,
                    ContentTitle = quizAnswerSubmitObj.ContentDetails.ContentTitle,
                    ShowTitle = quizAnswerSubmitObj.ContentDetails.ShowTitle,
                    ContentTitleImage = quizAnswerSubmitObj.ContentDetails.ContentTitleImage,
                    PublicIdForContentTitle = quizAnswerSubmitObj.ContentDetails.PublicIdForContentTitle,
                    Id = quizAnswerSubmitObj.ContentDetails.Id,
                    ShowContentDescriptionImage = quizAnswerSubmitObj.ContentDetails.ShowContentDescriptionImage,
                    ShowContentTitleImage = quizAnswerSubmitObj.ContentDetails.ShowContentTitleImage,
                    ViewPreviousQuestion = quizAnswerSubmitObj.ContentDetails.ViewPreviousQuestion,
                    AutoPlay = quizAnswerSubmitObj.ContentDetails.AutoPlay,
                    AutoPlayForDescription = quizAnswerSubmitObj.ContentDetails.AutoPlayForDescription,
                    DisplayOrderForTitle = quizAnswerSubmitObj.ContentDetails.DisplayOrderForTitle,
                    DisplayOrderForTitleImage = quizAnswerSubmitObj.ContentDetails.DisplayOrderForTitleImage,
                    DisplayOrderForDescription = quizAnswerSubmitObj.ContentDetails.DisplayOrderForDescription,
                    DisplayOrderForDescriptionImage = quizAnswerSubmitObj.ContentDetails.DisplayOrderForDescriptionImage,
                    DisplayOrderForNextButton = quizAnswerSubmitObj.ContentDetails.DisplayOrderForNextButton,
                    SecondsToApply = quizAnswerSubmitObj.ContentDetails.SecondsToApply,
                    VideoFrameEnabled = quizAnswerSubmitObj.ContentDetails.VideoFrameEnabled,
                    SecondsToApplyForDescription = quizAnswerSubmitObj.ContentDetails.SecondsToApplyForDescription,
                    DescVideoFrameEnabled = quizAnswerSubmitObj.ContentDetails.DescVideoFrameEnabled
                };
            }
            if (quizAnswerSubmitObj.BadgeDetails != null)
            {
                response.BadgeDetails = new QuizApp.Response.AttemptQuizResponse.QuizBadge()
                {
                    Id = quizAnswerSubmitObj.BadgeDetails.Id,
                    Title = quizAnswerSubmitObj.BadgeDetails.Title,
                    ShowTitle = quizAnswerSubmitObj.BadgeDetails.ShowTitle,
                    Image = quizAnswerSubmitObj.BadgeDetails.Image,
                    ShowImage = quizAnswerSubmitObj.BadgeDetails.ShowImage,
                    PublicIdForBadge = quizAnswerSubmitObj.BadgeDetails.PublicIdForBadge,
                    AutoPlay = quizAnswerSubmitObj.BadgeDetails.AutoPlay,
                    SecondsToApply = quizAnswerSubmitObj.BadgeDetails.SecondsToApply,
                    VideoFrameEnabled = quizAnswerSubmitObj.BadgeDetails.VideoFrameEnabled
                };
            }

            if (quizAnswerSubmitObj.SubmittedAnswer != null)
            {
                response.SubmittedAnswer = new QuizApp.Response.AttemptQuizResponse.SubmittedAnswerResult();

                response.SubmittedAnswer.IsCorrect = quizAnswerSubmitObj.SubmittedAnswer.IsCorrect;
                response.SubmittedAnswer.AliasTextForCorrect = quizAnswerSubmitObj.SubmittedAnswer.AliasTextForCorrect;
                response.SubmittedAnswer.AliasTextForIncorrect = quizAnswerSubmitObj.SubmittedAnswer.AliasTextForIncorrect;
                response.SubmittedAnswer.AliasTextForYourAnswer = quizAnswerSubmitObj.SubmittedAnswer.AliasTextForYourAnswer;
                response.SubmittedAnswer.AliasTextForCorrectAnswer = quizAnswerSubmitObj.SubmittedAnswer.AliasTextForCorrectAnswer;
                response.SubmittedAnswer.AliasTextForExplanation = quizAnswerSubmitObj.SubmittedAnswer.AliasTextForExplanation;
                response.SubmittedAnswer.AliasTextForNextButton = quizAnswerSubmitObj.SubmittedAnswer.AliasTextForNextButton;
                response.SubmittedAnswer.CorrectAnswerDescription = quizAnswerSubmitObj.SubmittedAnswer.CorrectAnswerDescription;
                response.SubmittedAnswer.ShowAnswerImage = quizAnswerSubmitObj.SubmittedAnswer.ShowAnswerImage;

                response.SubmittedAnswer.SubmittedAnswerDetails = new List<QuizApp.Response.AttemptQuizResponse.SubmittedAnswerResult.SubmittedAnswer>();
                if (quizAnswerSubmitObj.SubmittedAnswer.SubmittedAnswerDetails != null)
                {
                    foreach (var obj in quizAnswerSubmitObj.SubmittedAnswer.SubmittedAnswerDetails)
                    {
                        response.SubmittedAnswer.SubmittedAnswerDetails.Add(new QuizApp.Response.AttemptQuizResponse.SubmittedAnswerResult.SubmittedAnswer
                        {
                            AnswerId = obj.AnswerId,
                            SubmittedAnswerTitle = obj.SubmittedAnswerTitle,
                            SubmittedSecondaryAnswerTitle = obj.SubmittedSecondaryAnswerTitle,
                            SubmittedAnswerImage = obj.SubmittedAnswerImage,
                            PublicIdForSubmittedAnswer = obj.PublicIdForSubmittedAnswer,
                            AutoPlay = obj.AutoPlay,
                            SecondsToApply = obj.SecondsToApply,
                            VideoFrameEnabled = obj.VideoFrameEnabled
                        });
                    }
                }

                response.SubmittedAnswer.CorrectAnswerDetails = new List<QuizApp.Response.AttemptQuizResponse.SubmittedAnswerResult.CorrectAnswer>();
                if (quizAnswerSubmitObj.SubmittedAnswer.CorrectAnswerDetails != null)
                {
                    foreach (var obj in quizAnswerSubmitObj.SubmittedAnswer.CorrectAnswerDetails)
                    {
                        response.SubmittedAnswer.CorrectAnswerDetails.Add(new QuizApp.Response.AttemptQuizResponse.SubmittedAnswerResult.CorrectAnswer
                        {
                            CorrectAnswerTitle = obj.CorrectAnswerTitle,
                            CorrectAnswerImage = obj.CorrectAnswerImage,
                            PublicIdForCorrectAnswer = obj.PublicIdForCorrectAnswer,
                            AutoPlay = obj.AutoPlay,
                            SecondsToApply = obj.SecondsToApply,
                            VideoFrameEnabled = obj.VideoFrameEnabled
                        });
                    }
                }
            }

            //
            if (quizAnswerSubmitObj.PreviousQuestionSubmittedAnswer != null)
            {
                response.PreviousQuestionSubmittedAnswer = new QuizApp.Response.AttemptQuizResponse.SubmittedAnswerResult();

                response.PreviousQuestionSubmittedAnswer.IsCorrect = quizAnswerSubmitObj.PreviousQuestionSubmittedAnswer.IsCorrect;
                response.PreviousQuestionSubmittedAnswer.AliasTextForCorrect = quizAnswerSubmitObj.PreviousQuestionSubmittedAnswer.AliasTextForCorrect;
                response.PreviousQuestionSubmittedAnswer.AliasTextForIncorrect = quizAnswerSubmitObj.PreviousQuestionSubmittedAnswer.AliasTextForIncorrect;
                response.PreviousQuestionSubmittedAnswer.AliasTextForYourAnswer = quizAnswerSubmitObj.PreviousQuestionSubmittedAnswer.AliasTextForYourAnswer;
                response.PreviousQuestionSubmittedAnswer.AliasTextForCorrectAnswer = quizAnswerSubmitObj.PreviousQuestionSubmittedAnswer.AliasTextForCorrectAnswer;
                response.PreviousQuestionSubmittedAnswer.AliasTextForExplanation = quizAnswerSubmitObj.PreviousQuestionSubmittedAnswer.AliasTextForExplanation;
                response.PreviousQuestionSubmittedAnswer.AliasTextForNextButton = quizAnswerSubmitObj.PreviousQuestionSubmittedAnswer.AliasTextForNextButton;
                response.PreviousQuestionSubmittedAnswer.CorrectAnswerDescription = quizAnswerSubmitObj.PreviousQuestionSubmittedAnswer.CorrectAnswerDescription;

                response.PreviousQuestionSubmittedAnswer.SubmittedAnswerDetails = new List<QuizApp.Response.AttemptQuizResponse.SubmittedAnswerResult.SubmittedAnswer>();
                if (quizAnswerSubmitObj.PreviousQuestionSubmittedAnswer.SubmittedAnswerDetails != null)
                {
                    foreach (var obj in quizAnswerSubmitObj.PreviousQuestionSubmittedAnswer.SubmittedAnswerDetails)
                    {
                        response.PreviousQuestionSubmittedAnswer.SubmittedAnswerDetails.Add(new QuizApp.Response.AttemptQuizResponse.SubmittedAnswerResult.SubmittedAnswer
                        {
                            AnswerId = obj.AnswerId,
                            SubmittedAnswerTitle = obj.SubmittedAnswerTitle,
                            SubmittedSecondaryAnswerTitle = obj.SubmittedSecondaryAnswerTitle,
                            SubmittedAnswerImage = obj.SubmittedAnswerImage,
                            PublicIdForSubmittedAnswer = obj.PublicIdForSubmittedAnswer,
                            SubAnswerTypeId = obj.SubAnswerTypeId,
                            AutoPlay = obj.AutoPlay,
                            Comment = obj.Comment,
                            //for Rating type question
                            OptionTextforRatingOne = obj.OptionTextforRatingOne,
                            OptionTextforRatingTwo = obj.OptionTextforRatingTwo,
                            OptionTextforRatingThree = obj.OptionTextforRatingThree,
                            OptionTextforRatingFour = obj.OptionTextforRatingFour,
                            OptionTextforRatingFive = obj.OptionTextforRatingFive,
                            OptionTextforRating1 = obj.OptionTextforRatingOne,
                            OptionTextforRating2 = obj.OptionTextforRatingTwo,
                            OptionTextforRating3 = obj.OptionTextforRatingThree,
                            OptionTextforRating4 = obj.OptionTextforRatingFour,
                            OptionTextforRating5 = obj.OptionTextforRatingFive,
                            SecondsToApply = obj.SecondsToApply,
                            VideoFrameEnabled = obj.VideoFrameEnabled
                        });
                    }
                }

                response.PreviousQuestionSubmittedAnswer.CorrectAnswerDetails = new List<QuizApp.Response.AttemptQuizResponse.SubmittedAnswerResult.CorrectAnswer>();
                if (quizAnswerSubmitObj.PreviousQuestionSubmittedAnswer.CorrectAnswerDetails != null)
                {
                    foreach (var obj in quizAnswerSubmitObj.PreviousQuestionSubmittedAnswer.CorrectAnswerDetails)
                    {
                        response.PreviousQuestionSubmittedAnswer.CorrectAnswerDetails.Add(new QuizApp.Response.AttemptQuizResponse.SubmittedAnswerResult.CorrectAnswer
                        {
                            CorrectAnswerTitle = obj.CorrectAnswerTitle,
                            CorrectAnswerImage = obj.CorrectAnswerImage,
                            PublicIdForCorrectAnswer = obj.PublicIdForCorrectAnswer,
                            AutoPlay = obj.AutoPlay,
                            SecondsToApply = obj.SecondsToApply,
                            VideoFrameEnabled = obj.VideoFrameEnabled
                        });
                    }
                }
            }

            if (quizAnswerSubmitObj.ResultScore != null)
            {
                response.ResultScore = new QuizApp.Response.AttemptQuizResponse.Result();

                response.ResultScore.Title = quizAnswerSubmitObj.ResultScore.Title;
                response.ResultScore.InternalTitle = quizAnswerSubmitObj.ResultScore.InternalTitle;
                response.ResultScore.Image = quizAnswerSubmitObj.ResultScore.Image;
                response.ResultScore.PublicIdForResult = quizAnswerSubmitObj.ResultScore.PublicIdForResult;
                response.ResultScore.Description = quizAnswerSubmitObj.ResultScore.Description;
                response.ResultScore.HideActionButton = quizAnswerSubmitObj.ResultScore.HideActionButton;
                response.ResultScore.EnableCallToActionButton = !quizAnswerSubmitObj.ResultScore.HideActionButton;
                response.ResultScore.ActionButtonURL = quizAnswerSubmitObj.ResultScore.ActionButtonURL;
                response.ResultScore.OpenLinkInNewTab = quizAnswerSubmitObj.ResultScore.OpenLinkInNewTab;
                response.ResultScore.ActionButtonTxtSize = quizAnswerSubmitObj.ResultScore.ActionButtonTxtSize;
                response.ResultScore.ActionButtonColor = quizAnswerSubmitObj.ResultScore.ActionButtonColor;
                response.ResultScore.ActionButtonTitleColor = quizAnswerSubmitObj.ResultScore.ActionButtonTitleColor;
                response.ResultScore.ActionButtonText = quizAnswerSubmitObj.ResultScore.ActionButtonText;
                response.ResultScore.ResultScoreValueTxt = quizAnswerSubmitObj.ResultScore.ResultScoreValueTxt;
                response.ResultScore.AnswerKeyCustomTxt = quizAnswerSubmitObj.ResultScore.AnswerKeyCustomTxt;
                response.ResultScore.YourAnswerCustomTxt = quizAnswerSubmitObj.ResultScore.YourAnswerCustomTxt;
                response.ResultScore.CorrectAnswerCustomTxt = quizAnswerSubmitObj.ResultScore.CorrectAnswerCustomTxt;
                response.ResultScore.ExplanationCustomTxt = quizAnswerSubmitObj.ResultScore.ExplanationCustomTxt;
                response.ResultScore.ShowScoreValue = quizAnswerSubmitObj.ResultScore.ShowScoreValue;
                response.ResultScore.ShowCorrectAnswer = quizAnswerSubmitObj.ResultScore.ShowCorrectAnswer;

                response.ResultScore.ShowFacebookBtn = quizAnswerSubmitObj.ResultScore.ShowFacebookBtn;
                response.ResultScore.ShowTwitterBtn = quizAnswerSubmitObj.ResultScore.ShowTwitterBtn;
                response.ResultScore.ShowLinkedinBtn = quizAnswerSubmitObj.ResultScore.ShowLinkedinBtn;
                response.ResultScore.AutoPlay = quizAnswerSubmitObj.ResultScore.AutoPlay;
                response.ResultScore.ShowExternalTitle = quizAnswerSubmitObj.ResultScore.ShowExternalTitle;
                response.ResultScore.ShowInternalTitle = quizAnswerSubmitObj.ResultScore.ShowInternalTitle;
                response.ResultScore.ShowDescription = quizAnswerSubmitObj.ResultScore.ShowDescription;
                response.ResultScore.DisplayOrderForTitle = quizAnswerSubmitObj.ResultScore.DisplayOrderForTitle;
                response.ResultScore.DisplayOrderForTitleImage = quizAnswerSubmitObj.ResultScore.DisplayOrderForTitleImage;
                response.ResultScore.DisplayOrderForDescription = quizAnswerSubmitObj.ResultScore.DisplayOrderForDescription;
                response.ResultScore.DisplayOrderForNextButton = quizAnswerSubmitObj.ResultScore.DisplayOrderForNextButton;
                response.ResultScore.SecondsToApply = quizAnswerSubmitObj.ResultScore.SecondsToApply;
                response.ResultScore.VideoFrameEnabled = quizAnswerSubmitObj.ResultScore.VideoFrameEnabled;

                if (quizAnswerSubmitObj.ResultScore.AttemptedQuestionAnswerDetails != null)
                {
                    response.ResultScore.AttemptedQuestionAnswerDetails = new List<QuizApp.Response.AttemptQuizResponse.AnswerResult>();

                    foreach (var item in quizAnswerSubmitObj.ResultScore.AttemptedQuestionAnswerDetails)
                    {
                        response.ResultScore.AttemptedQuestionAnswerDetails.Add(new QuizApp.Response.AttemptQuizResponse.AnswerResult
                        {
                            Question = item.Question,
                            YourAnswer = item.YourAnswer,
                            CorrectAnswer = item.CorrectAnswer,
                            IsCorrect = item.IsCorrect,
                            AnswerExplanation = item.AnswerExplanation,
                            AssociatedScore = item.AssociatedScore,
                        });
                    }
                }

                if (quizAnswerSubmitObj.ResultScore.PersonalityResultList != null)
                {
                    response.ResultScore.PersonalityResultList = new List<QuizApp.Response.AttemptQuizResponse.PersonalityResult>();
                    foreach (var item in quizAnswerSubmitObj.ResultScore.PersonalityResultList)
                    {
                        response.ResultScore.PersonalityResultList.Add(new QuizApp.Response.AttemptQuizResponse.PersonalityResult
                        {
                            ResultId = item.ResultId,
                            Title = item.Title,
                            InternalTitle = item.InternalTitle,
                            Image = item.Image,
                            GraphColor = item.GraphColor,
                            ButtonColor = item.ButtonColor,
                            ButtonFontColor = item.ButtonFontColor,
                            MaxResult = item.MaxResult,
                            SideButtonText = item.SideButtonText,
                            Percentage = item.Percentage,
                            IsFullWidthEnable = item.IsFullWidthEnable,
                            Description = item.Description,
                            ShowExternalTitle = item.ShowExternalTitle,
                            ShowInternalTitle = item.ShowInternalTitle,
                            ShowDescription = item.ShowDescription
                        });
                    }
                }

            }

            return response;
        }
    }
}