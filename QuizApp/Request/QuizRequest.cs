using QuizApp.Services.Model;
using Swashbuckle.Examples;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Request
{
    public class AddQuizRequest
    {
        public int QuizType { get; set; }
        public string AccessibleOfficeId { get; set; }
        public int? CategoryId { get; set; }
        public string UserType { get; set; }
        public int? ModuleType { get; set; }

        public LocalQuiz MapRequestToEntity(AddQuizRequest AddQuizRequestObj, bool IsCreateStandardAutomation)
        {
            LocalQuiz obj = new LocalQuiz();

            obj.QuizType = (Helpers.QuizTypeEnum)AddQuizRequestObj.QuizType;
            obj.AccessibleOfficeId = AddQuizRequestObj.AccessibleOfficeId;
            obj.IsCreateStandardAutomation = IsCreateStandardAutomation;
            obj.CategoryId = AddQuizRequestObj.CategoryId;
            obj.UserType = AddQuizRequestObj.UserType;
            obj.ModuleType = AddQuizRequestObj.ModuleType;

            return obj;
        }

        public class AddQuizRequestExample : IExamplesProvider
        {
            public object GetExamples()
            {
                return new
                {
                    QuizType = 1,
                    AccessibleOfficeId = string.Empty,
                    CategoryId = 0,
                    UserType = string.Empty,
                    ModuleType = 1
                };
            }
        }
    }

    public class UpdateQuizRequest
    {
        public int QuizId { get; set; }
        public string QuizTitle { get; set; }

        public QuizDto MapRequestToEntity(UpdateQuizRequest QuizCoverDetailRequestObj)
        {
            QuizDto obj = new QuizDto();

            obj.QuizId = QuizCoverDetailRequestObj.QuizId;
            obj.QuizTitle = QuizCoverDetailRequestObj.QuizTitle;

            return obj;
        }

        public class UpdateQuizRequestExample : IExamplesProvider
        {
            public object GetExamples()
            {
                return new
                {
                    QuizId = 1,
                    QuizTitle = string.Empty
                };
            }
        }
    }

    public class BrandingAndStyleRequest
    {
        public int QuizId { get; set; }
        public string ImageFileURL { get; set; }
        public string PublicIdForFileURL { get; set; }
        public string BackgroundColor { get; set; }
        public string ButtonColor { get; set; }
        public string OptionColor { get; set; }
        public string ButtonFontColor { get; set; }
        public string OptionFontColor { get; set; }
        public string FontColor { get; set; }
        public string FontType { get; set; }
        public string ButtonHoverColor { get; set; }
        public string ButtonHoverTextColor { get; set; }
        public string BackgroundColorofSelectedAnswer { get; set; }
        public string BackgroundColorofAnsweronHover { get; set; }
        public string AnswerTextColorofSelectedAnswer { get; set; }
        public bool ApplyToAll { get; set; }
        public int IsBackType { get; set; }
        public string BackImageFileURL { get; set; }
        public string BackColor { get; set; }
        public string Opacity { get; set; }
        public string LogoUrl { get; set; }
        public string LogoPublicId { get; set; }
        public string BackgroundColorofLogo { get; set; }
        public string AutomationAlignment { get; set; }
        public string LogoAlignment { get; set; }
        public bool Flip { get; set; }
        public int? Language { get; set; }

        public QuizBrandingAndStyleModel MapRequestToEntity(BrandingAndStyleRequest BrandingAndStyleRequestObj)
        {
            QuizBrandingAndStyleModel obj = new QuizBrandingAndStyleModel();

            obj.QuizId = BrandingAndStyleRequestObj.QuizId;
            obj.ImageFileURL = BrandingAndStyleRequestObj.ImageFileURL;
            obj.PublicIdForFileURL = BrandingAndStyleRequestObj.PublicIdForFileURL;
            obj.BackgroundColor = BrandingAndStyleRequestObj.BackgroundColor;
            obj.ButtonColor = BrandingAndStyleRequestObj.ButtonColor;
            obj.OptionColor = BrandingAndStyleRequestObj.OptionColor;
            obj.ButtonFontColor = BrandingAndStyleRequestObj.ButtonFontColor;
            obj.OptionFontColor = BrandingAndStyleRequestObj.OptionFontColor;
            obj.FontColor = BrandingAndStyleRequestObj.FontColor;
            obj.FontType = BrandingAndStyleRequestObj.FontType;
            obj.ButtonHoverColor = BrandingAndStyleRequestObj.ButtonHoverColor;
            obj.ButtonHoverTextColor = BrandingAndStyleRequestObj.ButtonHoverTextColor;
            obj.BackgroundColorofSelectedAnswer = BrandingAndStyleRequestObj.BackgroundColorofSelectedAnswer;
            obj.BackgroundColorofAnsweronHover = BrandingAndStyleRequestObj.BackgroundColorofAnsweronHover;
            obj.AnswerTextColorofSelectedAnswer = BrandingAndStyleRequestObj.AnswerTextColorofSelectedAnswer;
            obj.ApplyToAll = BrandingAndStyleRequestObj.ApplyToAll;
            obj.IsBackType = BrandingAndStyleRequestObj.IsBackType;
            obj.BackImageFileURL = BrandingAndStyleRequestObj.BackImageFileURL;
            obj.BackColor = BrandingAndStyleRequestObj.BackColor;
            obj.Opacity = BrandingAndStyleRequestObj.Opacity;
            obj.LogoUrl = BrandingAndStyleRequestObj.LogoUrl;
            obj.LogoPublicId = BrandingAndStyleRequestObj.LogoPublicId;
            obj.BackgroundColorofLogo = BrandingAndStyleRequestObj.BackgroundColorofLogo;
            obj.AutomationAlignment = BrandingAndStyleRequestObj.AutomationAlignment;
            obj.LogoAlignment = BrandingAndStyleRequestObj.LogoAlignment;
            obj.Flip = BrandingAndStyleRequestObj.Flip;
            obj.Language = BrandingAndStyleRequestObj.Language;
            return obj;
        }

        public class BrandingAndStyleRequestExample : IExamplesProvider
        {
            public object GetExamples()
            {
                return new
                {
                    QuizId = 1,
                    ImageFileURL = string.Empty,
                    PublicIdForFileURL = string.Empty,
                    BackgroundColor = "#ffffff",
                    ButtonColor = "#000000",
                    OptionColor = "#F4F4F4",
                    ButtonHoverColor = "#F4F4F4",
                    ButtonFontColor = "#000000",
                    OptionFontColor = "#111111",
                    FontColor = "#000000",
                    FontType = "Default",
                    BackgroundColorofSelectedAnswer = "#00B7AB",
                    BackgroundColorofAnsweronHover = "#F9F9F9",
                    AnswerTextColorofSelectedAnswer = "#FFFFFF",
                    ApplyToAll = false,
                    IsBackType = 1,
                    BackImageFileURL = string.Empty,
                    BackColor = string.Empty,
                    LogoUrl = string.Empty,
                    LogoPublicId = string.Empty,
                    BackgroundColorofLogo = "#FFFFFF",
                    AutomationAlignment = "Center",
                    LogoAlignment = "Left",
                    Flip = false,
                    Language = 1
                };
            }
        }
    }

    public class QuizAccessSettingRequest
    {
        public int QuizId { get; set; }
        public string AccessibleOfficeId { get; set; }

        public LocalQuiz MapRequestToEntity(QuizAccessSettingRequest QuizAccessSettingRequestObj)
        {
            LocalQuiz obj = new LocalQuiz();

            obj.Id = QuizAccessSettingRequestObj.QuizId;
            obj.AccessibleOfficeId = QuizAccessSettingRequestObj.AccessibleOfficeId;

            return obj;
        }

        public class QuizAccessSettingRequestExample : IExamplesProvider
        {
            public object GetExamples()
            {
                return new
                {
                    QuizId = 1,
                    AccessibleOfficeId = string.Empty
                };
            }
        }
    }

    public class QuizSocialShareSettingRequest
    {
        public int QuizId { get; set; }
        public bool? HideSocialShareButtons { get; set; }
        public bool? EnableFacebookShare { get; set; }
        public bool? EnableTwitterShare { get; set; }
        public bool? EnableLinkedinShare { get; set; }

        public QuizSocialShareSetting MapRequestToEntity(QuizSocialShareSettingRequest QuizSocialShareSettingRequestObj)
        {
            QuizSocialShareSetting obj = new QuizSocialShareSetting();

            obj.QuizId = QuizSocialShareSettingRequestObj.QuizId;
            obj.HideSocialShareButtons = QuizSocialShareSettingRequestObj.HideSocialShareButtons;
            obj.EnableFacebookShare = QuizSocialShareSettingRequestObj.EnableFacebookShare;
            obj.EnableTwitterShare = QuizSocialShareSettingRequestObj.EnableTwitterShare;
            obj.EnableLinkedinShare = QuizSocialShareSettingRequestObj.EnableLinkedinShare;

            return obj;
        }

        public class QuizSocialShareSettingRequestExample : IExamplesProvider
        {
            public object GetExamples()
            {
                return new
                {
                    QuizId = 1,
                    HideSocialShareButtons = false,
                    EnableFacebookShare = true,
                    EnableTwitterShare = false,
                    EnableLinkedinShare = false
                };
            }
        }
    }

    public class QuizCoverDetailRequest
    {
        public int QuizId { get; set; }
        public string QuizTitle { get; set; }
        public string QuizCoverTitle { get; set; }
        public bool ShowQuizCoverTitle { get; set; }
        public string QuizCoverImage { get; set; }
        public bool ShowQuizCoverImage { get; set; }
        public bool EnableMediaFile { get; set; }
        public string PublicIdForQuizCover { get; set; }
        public int? QuizCoverImgXCoordinate { get; set; }
        public int? QuizCoverImgYCoordinate { get; set; }
        public int? QuizCoverImgHeight { get; set; }
        public int? QuizCoverImgWidth { get; set; }
        public string QuizCoverImgAttributionLabel { get; set; }
        public string QuizCoverImgAltTag { get; set; }
        public string QuizDescription { get; set; }
        public bool ShowDescription { get; set; }
        public string QuizStartButtonText { get; set; }
        public bool  AutoPlay{ get; set; }
        public string SecondsToApply { get; set; }
        public bool? VideoFrameEnabled { get; set; }
        public bool EnableNextButton { get; set; }
        public int DisplayOrderForTitle { get; set; }
        public int DisplayOrderForTitleImage { get; set; }
        public int DisplayOrderForDescription { get; set; }
        public int DisplayOrderForNextButton { get; set; }
        public List<string> MsgVariables { get; set; }

        public QuizCover MapRequestToEntity(QuizCoverDetailRequest QuizCoverDetailRequestObj)
        {
            QuizCover obj = new QuizCover();

            obj.QuizId = QuizCoverDetailRequestObj.QuizId;
            obj.QuizTitle = QuizCoverDetailRequestObj.QuizTitle;
            obj.QuizCoverTitle = QuizCoverDetailRequestObj.QuizCoverTitle;
            obj.ShowQuizCoverTitle = QuizCoverDetailRequestObj.ShowQuizCoverTitle;
            obj.QuizCoverImage = QuizCoverDetailRequestObj.QuizCoverImage;
            obj.ShowQuizCoverImage = QuizCoverDetailRequestObj.ShowQuizCoverImage;
            obj.EnableMediaFile = QuizCoverDetailRequestObj.EnableMediaFile;
            obj.PublicIdForQuizCover = QuizCoverDetailRequestObj.PublicIdForQuizCover;
            obj.QuizCoverImgXCoordinate = QuizCoverDetailRequestObj.QuizCoverImgXCoordinate;
            obj.QuizCoverImgYCoordinate = QuizCoverDetailRequestObj.QuizCoverImgYCoordinate;
            obj.QuizCoverImgHeight = QuizCoverDetailRequestObj.QuizCoverImgHeight;
            obj.QuizCoverImgWidth = QuizCoverDetailRequestObj.QuizCoverImgWidth;
            obj.QuizCoverImgAttributionLabel = QuizCoverDetailRequestObj.QuizCoverImgAttributionLabel;
            obj.QuizCoverImgAltTag = QuizCoverDetailRequestObj.QuizCoverImgAltTag;
            obj.QuizDescription = QuizCoverDetailRequestObj.QuizDescription;
            obj.ShowDescription = QuizCoverDetailRequestObj.ShowDescription;
            obj.QuizStartButtonText = QuizCoverDetailRequestObj.QuizStartButtonText;
            obj.AutoPlay = QuizCoverDetailRequestObj.AutoPlay;
            obj.SecondsToApply = QuizCoverDetailRequestObj.SecondsToApply;
            obj.VideoFrameEnabled = QuizCoverDetailRequestObj.VideoFrameEnabled;
            obj.EnableNextButton = QuizCoverDetailRequestObj.EnableNextButton;
            obj.DisplayOrderForTitleImage = QuizCoverDetailRequestObj.DisplayOrderForTitleImage;
            obj.DisplayOrderForTitle = QuizCoverDetailRequestObj.DisplayOrderForTitle;
            obj.DisplayOrderForDescription = QuizCoverDetailRequestObj.DisplayOrderForDescription;
            obj.DisplayOrderForNextButton = QuizCoverDetailRequestObj.DisplayOrderForNextButton;
            obj.MsgVariables = QuizCoverDetailRequestObj.MsgVariables;

            return obj;
        }

        public class QuizCoverDetailRequestExample : IExamplesProvider
        {
            public object GetExamples()
            {
                return new
                {
                    QuizId = 1,
                    QuizTitle = string.Empty,
                    QuizCoverTitle = string.Empty,
                    ShowQuizCoverTitle = true,
                    QuizCoverImage = string.Empty,
                    ShowQuizCoverImage = true,
                    EnableMediaFile = false,
                    PublicIdForQuizCover = string.Empty,
                    QuizCoverImgXCoordinate = 0,
                    QuizCoverImgYCoordinate = 0,
                    QuizCoverImgHeight = 0,
                    QuizCoverImgWidth = 0,
                    QuizCoverImgAttributionLabel = string.Empty,
                    QuizCoverImgAltTag = string.Empty,
                    QuizDescription = string.Empty,
                    ShowDescription = true,
                    QuizStartButtonText = string.Empty,
                    AutoPlay = false,
                    EnableNextButton = true,
                    DisplayOrderForTitleImage= 1,
                    DisplayOrderForTitle = 2,
                    DisplayOrderForDescription = 3,
                    DisplayOrderForNextButton = 4
                };
            }
        }
    }

    public class QuizCoverImageRequest
    {
        public int QuizId { get; set; }
        public string QuizCoverImage { get; set; }
        public string PublicIdForQuizCover { get; set; }
        public int? QuizCoverImgXCoordinate { get; set; }
        public int? QuizCoverImgYCoordinate { get; set; }
        public int? QuizCoverImgHeight { get; set; }
        public int? QuizCoverImgWidth { get; set; }
        public string QuizCoverImgAttributionLabel { get; set; }
        public string QuizCoverImgAltTag { get; set; }

        public QuizCover MapRequestToEntity(QuizCoverImageRequest QuizCoverDetailRequestObj)
        {
            QuizCover obj = new QuizCover();

            obj.QuizId = QuizCoverDetailRequestObj.QuizId;
            obj.QuizCoverImage = QuizCoverDetailRequestObj.QuizCoverImage;
            obj.PublicIdForQuizCover = QuizCoverDetailRequestObj.PublicIdForQuizCover;
            obj.QuizCoverImgXCoordinate = QuizCoverDetailRequestObj.QuizCoverImgXCoordinate;
            obj.QuizCoverImgYCoordinate = QuizCoverDetailRequestObj.QuizCoverImgYCoordinate;
            obj.QuizCoverImgHeight = QuizCoverDetailRequestObj.QuizCoverImgHeight;
            obj.QuizCoverImgWidth = QuizCoverDetailRequestObj.QuizCoverImgWidth;
            obj.QuizCoverImgAttributionLabel = QuizCoverDetailRequestObj.QuizCoverImgAttributionLabel;
            obj.QuizCoverImgAltTag = QuizCoverDetailRequestObj.QuizCoverImgAltTag;

            return obj;
        }

        public class QuizCoverImageRequestExample : IExamplesProvider
        {
            public object GetExamples()
            {
                return new
                {
                    QuizId = 1,
                    QuizCoverImage = string.Empty,
                    PublicIdForQuizCover = string.Empty,
                    QuizCoverImgXCoordinate = 0,
                    QuizCoverImgYCoordinate = 0,
                    QuizCoverImgHeight = 0,
                    QuizCoverImgWidth = 0,
                    QuizCoverImgAttributionLabel = string.Empty,
                    QuizCoverImgAltTag = string.Empty
                };
            }
        }
    }

    public class QuizCorrectAnswerSettingRequest
    {
        public int QuestionId { get; set; }
        public List<int> CorrectAnswerId { get; set; }
        public string CorrectAnswerExplanation { get; set; }
        public bool RevealCorrectAnswer { get; set; }
        public string AliasTextForCorrect { get; set; }
        public string AliasTextForIncorrect { get; set; }
        public string AliasTextForYourAnswer { get; set; }
        public string AliasTextForCorrectAnswer { get; set; }
        public string AliasTextForExplanation { get; set; }
        public string AliasTextForNextButton { get; set; }
        public int AnswerType { get; set; }
        public int MinAnswer { get; set; }
        public int MaxAnswer { get; set; }
        public List<AnswerScoreRequest> AnswerScoreRequestData { get; set; }

        public QuizCorrectAnswerSetting MapRequestToEntity(QuizCorrectAnswerSettingRequest QuizCorrectAnswerSettingRequestObj)
        {
            QuizCorrectAnswerSetting obj = new QuizCorrectAnswerSetting();

            obj.QuestionId = QuizCorrectAnswerSettingRequestObj.QuestionId;
            obj.CorrectAnswerId = QuizCorrectAnswerSettingRequestObj.CorrectAnswerId;
            obj.CorrectAnswerExplanation = QuizCorrectAnswerSettingRequestObj.CorrectAnswerExplanation;
            obj.RevealCorrectAnswer = QuizCorrectAnswerSettingRequestObj.RevealCorrectAnswer;
            obj.AliasTextForCorrect = QuizCorrectAnswerSettingRequestObj.AliasTextForCorrect;
            obj.AliasTextForIncorrect = QuizCorrectAnswerSettingRequestObj.AliasTextForIncorrect;
            obj.AliasTextForYourAnswer = QuizCorrectAnswerSettingRequestObj.AliasTextForYourAnswer;
            obj.AliasTextForCorrectAnswer = QuizCorrectAnswerSettingRequestObj.AliasTextForCorrectAnswer;
            obj.AliasTextForExplanation = QuizCorrectAnswerSettingRequestObj.AliasTextForExplanation;
            obj.AliasTextForNextButton = QuizCorrectAnswerSettingRequestObj.AliasTextForNextButton;
            obj.AnswerType = QuizCorrectAnswerSettingRequestObj.AnswerType;
            obj.MinAnswer = QuizCorrectAnswerSettingRequestObj.MinAnswer;
            obj.MaxAnswer = QuizCorrectAnswerSettingRequestObj.MaxAnswer;

            if (QuizCorrectAnswerSettingRequestObj.AnswerScoreRequestData != null)
            {
                obj.AnswerScoreData = new List<AnswerScore>();
                foreach (var answer in QuizCorrectAnswerSettingRequestObj.AnswerScoreRequestData)
                {
                    obj.AnswerScoreData.Add(new AnswerScore
                    {
                        AnswerId = answer.AnswerId,
                        AssociatedScore = answer.AssociatedScore,
                    });
                }
            }
            return obj;
        }

        public class AnswerScoreRequest
        {
            public int AnswerId { get; set; }
            public int AssociatedScore { get; set; }
        }

        public class QuizCorrectAnswerSettingRequestExample : IExamplesProvider
        {
            public object GetExamples()
            {
                return new
                {
                    QuestionId = 1,
                    CorrectAnswerId = new List<int>() { },
                    CorrectAnswerExplanation = string.Empty,
                    RevealCorrectAnswer = true,
                    AliasTextForCorrect = "Correct",
                    AliasTextForIncorrect = "Incorrect",
                    AliasTextForYourAnswer = "Your answer",
                    AliasTextForCorrectAnswer = "Correct answer",
                    AliasTextForExplanation = "Explanation",
                    AliasTextForNextButton = "Next",
                    AnswerType = 0,
                    MinAnswer = 0,
                    MaxAnswer = 0,
                    AnswerScoreRequestData = new List<AnswerScoreRequest>() { new AnswerScoreRequest() { AnswerId = 0, AssociatedScore = 0 } }
                };
            }
        }
    }

    public class QuizQuestionDetailsRequest
    {
        public int QuizId { get; set; }
        public int QuestionId { get; set; }
        public bool ShowAnswerImage { get; set; }
        public string QuestionTitle { get; set; }
        public bool ShowTitle { get; set; }
        public string QuestionImage { get; set; }
        public bool EnableMediaFile { get; set; }
        public string PublicIdForQuestion { get; set; }
        public bool ShowQuestionImage { get; set; }
        public int MaxAnswer { get; set; }
        public int MinAnswer { get; set; }
        public string NextButtonText { get; set; }
        public string NextButtonTxtSize { get; set; }
        public string NextButtonTxtColor { get; set; }
        public string NextButtonColor { get; set; }
        public bool EnableNextButton { get; set; }
        public bool AutoPlay { get; set; }
        public string SecondsToApply { get; set; }
        public bool? VideoFrameEnabled { get; set; }
        public string Description { get; set; }
        public bool ShowDescription { get; set; }
        public string DescriptionImage { get; set; }
        public bool EnableMediaFileForDescription { get; set; }
        public string PublicIdForDescription { get; set; }
        public bool ShowDescriptionImage { get; set; }
        public bool AutoPlayForDescription { get; set; }
        public string SecondsToApplyForDescription { get; set; }
        public bool? DescVideoFrameEnabled { get; set; }
        public int Type { get; set; }
        public int DisplayOrderForTitle { get; set; }
        public int DisplayOrderForTitleImage { get; set; }
        public int DisplayOrderForDescription { get; set; }
        public int DisplayOrderForDescriptionImage { get; set; }
        public int DisplayOrderForAnswer { get; set; }
        public int DisplayOrderForNextButton { get; set; }
        public bool EnableComment { get; set; }
        public string TopicTitle { get; set; }
        public int AnswerStructureType { get; set; }
        public bool isMultiRating { get; set; }
        public List<AnswerOptionInQuestionRequest> AnswerList { get; set; }
        public List<string> MsgVariables { get; set; }

        public class AnswerOptionInQuestionRequest
        {
            public int AnswerId { get; set; }
            public string AnswerText { get; set; }
            public string AnswerDescription { get; set; }
            public string AnswerImage { get; set; }
            public bool EnableMediaFile { get; set; }
            public int? AssociatedScore { get; set; }
            public string PublicIdForAnswer { get; set; }
            public bool AutoPlay { get; set; }
            public string SecondsToApply { get; set; }
            public bool? VideoFrameEnabled { get; set; }
            public List<string> ListValues { get; set; }
            public string OptionTextforRatingOne { get; set; }
            public string OptionTextforRatingTwo { get; set; }
            public string OptionTextforRatingThree { get; set; }
            public string OptionTextforRatingFour { get; set; }
            public string OptionTextforRatingFive { get; set; }
        }

        public QuizQuestion MapRequestToEntity(QuizQuestionDetailsRequest QuizCorrectAnswerSettingRequestObj)
        {
            QuizQuestion obj = new QuizQuestion();

            obj.QuizId = QuizCorrectAnswerSettingRequestObj.QuizId;
            obj.QuestionId = QuizCorrectAnswerSettingRequestObj.QuestionId;
            obj.ShowAnswerImage = QuizCorrectAnswerSettingRequestObj.ShowAnswerImage;
            obj.QuestionTitle = QuizCorrectAnswerSettingRequestObj.QuestionTitle;
            obj.ShowTitle = QuizCorrectAnswerSettingRequestObj.ShowTitle;
            obj.QuestionImage = QuizCorrectAnswerSettingRequestObj.QuestionImage;
            obj.EnableMediaFile = QuizCorrectAnswerSettingRequestObj.EnableMediaFile;
            obj.PublicIdForQuestion = QuizCorrectAnswerSettingRequestObj.PublicIdForQuestion;
            obj.ShowQuestionImage = QuizCorrectAnswerSettingRequestObj.ShowQuestionImage;
            obj.NextButtonText = QuizCorrectAnswerSettingRequestObj.NextButtonText;
            obj.NextButtonColor = QuizCorrectAnswerSettingRequestObj.NextButtonColor;
            obj.NextButtonTxtColor = QuizCorrectAnswerSettingRequestObj.NextButtonTxtColor;
            obj.NextButtonTxtSize = QuizCorrectAnswerSettingRequestObj.NextButtonTxtSize;
            obj.EnableNextButton = QuizCorrectAnswerSettingRequestObj.EnableNextButton;
            obj.MinAnswer = QuizCorrectAnswerSettingRequestObj.MinAnswer;
            obj.MaxAnswer = QuizCorrectAnswerSettingRequestObj.MaxAnswer;
            obj.AutoPlay = QuizCorrectAnswerSettingRequestObj.AutoPlay;
            obj.SecondsToApply = QuizCorrectAnswerSettingRequestObj.SecondsToApply;
            obj.VideoFrameEnabled = QuizCorrectAnswerSettingRequestObj.VideoFrameEnabled;
            obj.Description = QuizCorrectAnswerSettingRequestObj.Description;
            obj.ShowDescription = QuizCorrectAnswerSettingRequestObj.ShowDescription;
            obj.DescriptionImage = QuizCorrectAnswerSettingRequestObj.DescriptionImage;
            obj.EnableMediaFileForDescription = QuizCorrectAnswerSettingRequestObj.EnableMediaFileForDescription;
            obj.PublicIdForDescription = QuizCorrectAnswerSettingRequestObj.PublicIdForDescription;
            obj.ShowDescriptionImage = QuizCorrectAnswerSettingRequestObj.ShowDescriptionImage;
            obj.AutoPlayForDescription = QuizCorrectAnswerSettingRequestObj.AutoPlayForDescription;
            obj.SecondsToApplyForDescription = QuizCorrectAnswerSettingRequestObj.SecondsToApplyForDescription;
            obj.DescVideoFrameEnabled = QuizCorrectAnswerSettingRequestObj.DescVideoFrameEnabled;
            obj.Type = QuizCorrectAnswerSettingRequestObj.Type == 0 ? (int)Helpers.BranchingLogicEnum.QUESTION : QuizCorrectAnswerSettingRequestObj.Type;
            obj.DisplayOrderForTitle = QuizCorrectAnswerSettingRequestObj.DisplayOrderForTitle;
            obj.DisplayOrderForTitleImage = QuizCorrectAnswerSettingRequestObj.DisplayOrderForTitleImage;
            obj.DisplayOrderForDescription = QuizCorrectAnswerSettingRequestObj.DisplayOrderForDescription;
            obj.DisplayOrderForDescriptionImage = QuizCorrectAnswerSettingRequestObj.DisplayOrderForDescriptionImage;
            obj.DisplayOrderForAnswer = QuizCorrectAnswerSettingRequestObj.DisplayOrderForAnswer;
            obj.DisplayOrderForNextButton = QuizCorrectAnswerSettingRequestObj.DisplayOrderForNextButton;
            obj.EnableComment = QuizCorrectAnswerSettingRequestObj.EnableComment;
            obj.TopicTitle = QuizCorrectAnswerSettingRequestObj.TopicTitle;
            obj.AnswerStructureType = QuizCorrectAnswerSettingRequestObj.AnswerStructureType;
            obj.MsgVariables = QuizCorrectAnswerSettingRequestObj.MsgVariables;
            //obj.IsMultiRating = QuizCorrectAnswerSettingRequestObj.isMultiRating;

            obj.AnswerList = new List<AnswerOptionInQuestion>();

            if (QuizCorrectAnswerSettingRequestObj.AnswerList != null)
            {
                foreach (var answer in QuizCorrectAnswerSettingRequestObj.AnswerList)
                {
                    obj.AnswerList.Add(new AnswerOptionInQuestion
                    {
                        AnswerId = answer.AnswerId,
                        AnswerText = answer.AnswerText,
                        AnswerDescription = answer.AnswerDescription,
                        AnswerImage = answer.AnswerImage,
                        EnableMediaFile = answer.EnableMediaFile,
                        AssociatedScore = answer.AssociatedScore,
                        PublicIdForAnswer = answer.PublicIdForAnswer,
                        AutoPlay = answer.AutoPlay,
                        SecondsToApply = answer.SecondsToApply,
                        VideoFrameEnabled = answer.VideoFrameEnabled,
                        ListValues = answer.ListValues != null && answer.ListValues.Any() ? string.Join(",", answer.ListValues) : null,
                        OptionTextforRatingOne = answer.OptionTextforRatingOne,
                        OptionTextforRatingTwo = answer.OptionTextforRatingTwo,
                        OptionTextforRatingThree = answer.OptionTextforRatingThree,
                        OptionTextforRatingFour = answer.OptionTextforRatingFour,
                        OptionTextforRatingFive = answer.OptionTextforRatingFive
                    });
                }
            }

            return obj;
        }

        public class QuizQuestionDetailsRequestExample : IExamplesProvider
        {
            public object GetExamples()
            {
                var list = new List<AnswerOptionInQuestionRequest>();

                list.Add(new AnswerOptionInQuestionRequest
                {
                    AnswerId = 1,
                    AnswerText = string.Empty,
                    AssociatedScore = 0,
                    AnswerImage = string.Empty,
                    EnableMediaFile = false,
                    PublicIdForAnswer = string.Empty,
                    AutoPlay = false,
                    ListValues = null,
                    OptionTextforRatingOne = string.Empty,
                    OptionTextforRatingTwo = string.Empty,
                    OptionTextforRatingThree = string.Empty,
                    OptionTextforRatingFour = string.Empty,
                    OptionTextforRatingFive = string.Empty
                });

                return new
                {
                    QuizId = 1,
                    QuestionId = 1,
                    ShowAnswerImage = false,
                    QuestionTitle = string.Empty,
                    ShowTitle = true,
                    QuestionImage = string.Empty,
                    EnableMediaFile = false,
                    PublicIdForQuestion = string.Empty,
                    ShowQuestionImage = false,
                    NextButtonText = string.Empty,
                    NextButtonColor = string.Empty,
                    NextButtonTxtColor = string.Empty,
                    NextButtonTxtSize = string.Empty,
                    EnableNextButton = true,
                    MinAnswer = 0,
                    MaxAnswer = 0,
                    AutoPlay = false,
                    Description = string.Empty,
                    ShowDescription = true,
                    DescriptionImage = string.Empty,
                    EnableMediaFileForDescription = false,
                    PublicIdForDescription = string.Empty,
                    ShowDescriptionImage = false,
                    AutoPlayForDescription = false,
                    Type = 2,
                    DisplayOrderForTitleImage = 1,
                    DisplayOrderForTitle = 2,
                    DisplayOrderForDescriptionImage = 3,
                    DisplayOrderForDescription = 4,
                    DisplayOrderForAnswer = 5,
                    DisplayOrderForNextButton = 6,
                    EnableComment = false,
                    TopicTitle = string.Empty,
                    AnswerList = list
                };
            }
        }
    }

    public class QuizReorderQuestionAnswerRequest
    {
        public int QuestionId { get; set; }
        public int DisplayOrder { get; set; }
        public int Type { get; set; }
        public List<AnswerReorder> Answers { get; set; }

        public class AnswerReorder
        {
            public int AnswerId { get; set; }
            public int DisplayOrder { get; set; }
        }

        public List<QuizQuestion> MapRequestToEntity(List<QuizReorderQuestionAnswerRequest> QuizReorderQuestionAnswerRequestObjList)
        {
            List<QuizQuestion> objList = new List<QuizQuestion>();

            foreach (var QuizReorderQuestionAnswerRequestObj in QuizReorderQuestionAnswerRequestObjList)
            {
                QuizQuestion obj = new QuizQuestion();

                obj.QuestionId = QuizReorderQuestionAnswerRequestObj.QuestionId;
                obj.DisplayOrder = QuizReorderQuestionAnswerRequestObj.DisplayOrder;
                obj.Type = QuizReorderQuestionAnswerRequestObj.Type;

                obj.AnswerList = new List<AnswerOptionInQuestion>();

                if (QuizReorderQuestionAnswerRequestObj.Answers != null)
                {
                    foreach (var answer in QuizReorderQuestionAnswerRequestObj.Answers)
                    {
                        obj.AnswerList.Add(new AnswerOptionInQuestion
                        {
                            AnswerId = answer.AnswerId,
                            DisplayOrder = answer.DisplayOrder
                        });
                    }
                }
                objList.Add(obj);
            }
            return objList;
        }

        public class QuizReorderQuestionAnswerRequestExample : IExamplesProvider
        {
            public object GetExamples()
            {
                List<QuizReorderQuestionAnswerRequest> returnList = new List<QuizReorderQuestionAnswerRequest>();

                var ansList = new List<AnswerReorder>();

                ansList.Add(new AnswerReorder
                {
                    AnswerId = 1,
                    DisplayOrder = 1
                });

                returnList.Add(new QuizReorderQuestionAnswerRequest
                {
                    QuestionId = 1,
                    DisplayOrder = 1,
                    Type = 2,
                    Answers = ansList
                });

                return returnList;
            }
        }
    }

    public class QuizReorderAnswerRequest
    {
        public int QuestionId { get; set; }
        public List<AnswerReorderDto> Answers { get; set; }

        public class AnswerReorderDto
        {
            public int AnswerId { get; set; }
            public int DisplayOrder { get; set; }
        }

        public QuizQuestion MapRequestToEntity(QuizReorderAnswerRequest QuizReorderQuestionAnswerRequestObj)
        {
            QuizQuestion obj = new QuizQuestion();

            obj.QuestionId = QuizReorderQuestionAnswerRequestObj.QuestionId;

            obj.AnswerList = new List<AnswerOptionInQuestion>();

            if (QuizReorderQuestionAnswerRequestObj.Answers != null)
            {
                foreach (var answer in QuizReorderQuestionAnswerRequestObj.Answers)
                {
                    obj.AnswerList.Add(new AnswerOptionInQuestion
                    {
                        AnswerId = answer.AnswerId,
                        DisplayOrder = answer.DisplayOrder
                    });
                }
            }
            return obj;
        }

        public class QuizReorderAnswerRequestExample : IExamplesProvider
        {
            public object GetExamples()
            {
                var ansList = new List<AnswerReorderDto>();
                ansList.Add(new AnswerReorderDto
                {
                    AnswerId = 1,
                    DisplayOrder = 1
                });

                return new QuizReorderAnswerRequest
                {
                    QuestionId = 1,
                    Answers = ansList
                };
            }
        }
    }

    public class QuizResultReorderRequest
    {
        public List<ResultReorderRequest> ResultReorderRequestList { get; set; }
        public List<QuestionAnswerReorder> QuizReorderQuestionAnswerRequest { get; set; }

        public class ResultReorderRequest
        {
            public int ResultId { get; set; }
            public int DisplayOrder { get; set; }
        }

        public class QuestionAnswerReorder
        {
            public int QuestionId { get; set; }
            public int DisplayOrder { get; set; }
            public List<QuizAnswerReorder> Answers { get; set; }

            public class QuizAnswerReorder
            {
                public int AnswerId { get; set; }
                public int DisplayOrder { get; set; }
            }
        }

        public QuizReorderResult MapRequestToEntity(QuizResultReorderRequest quizResultReorderRequestObjList)
        {
            QuizReorderResult reorderResult = new QuizReorderResult();

            List<QuizResult> resultList = new List<QuizResult>();

            foreach (var resultReorderRequestObj in quizResultReorderRequestObjList.ResultReorderRequestList)
            {
                QuizResult quizResult = new QuizResult();
                quizResult.ResultId = resultReorderRequestObj.ResultId;
                quizResult.DisplayOrder = resultReorderRequestObj.DisplayOrder;
                resultList.Add(quizResult);
            }

            List<QuizQuestion> questionList = new List<QuizQuestion>();
            foreach (var QuizReorderQuestionAnswerRequestObj in quizResultReorderRequestObjList.QuizReorderQuestionAnswerRequest)
            {
                QuizQuestion obj = new QuizQuestion();

                obj.QuestionId = QuizReorderQuestionAnswerRequestObj.QuestionId;
                obj.DisplayOrder = QuizReorderQuestionAnswerRequestObj.DisplayOrder;

                obj.AnswerList = new List<AnswerOptionInQuestion>();

                if (QuizReorderQuestionAnswerRequestObj.Answers != null)
                {
                    foreach (var answer in QuizReorderQuestionAnswerRequestObj.Answers)
                    {
                        obj.AnswerList.Add(new AnswerOptionInQuestion
                        {
                            AnswerId = answer.AnswerId,
                            DisplayOrder = answer.DisplayOrder
                        });
                    }
                }
                questionList.Add(obj);
            }

            reorderResult.QuizResultList = resultList;
            reorderResult.QuizQuestionList = questionList;

            return reorderResult;
        }

        public class QuizResultReorderRequestExample : IExamplesProvider
        {
            public object GetExamples()
            {
                QuizResultReorderRequest returnList = new QuizResultReorderRequest();

                var resultList = new List<ResultReorderRequest>();

                resultList.Add(new ResultReorderRequest
                {
                    ResultId = 1,
                    DisplayOrder = 1
                });

                var quizAnswerReorderList = new List<QuestionAnswerReorder.QuizAnswerReorder>();

                quizAnswerReorderList.Add(new QuestionAnswerReorder.QuizAnswerReorder
                {
                    AnswerId = 1,
                    DisplayOrder = 1
                });

                var questionAnswerReorderList = new List<QuestionAnswerReorder>();

                questionAnswerReorderList.Add(new QuestionAnswerReorder
                {
                    QuestionId = 1,
                    DisplayOrder = 1,
                    Answers = quizAnswerReorderList
                });

                return new
                {
                    ResultReorderRequestList = resultList,
                    QuizReorderQuestionAnswerRequest = questionAnswerReorderList
                };
            }
        }
    }

    public class QuizResultRequest
    {
        public int QuizId { get; set; }
        public int ResultId { get; set; }
        public bool ShowResultImage { get; set; }
        public string Title { get; set; }
        public string InternalTitle { get; set; }
        public string Image { get; set; }
        public bool EnableMediaFile { get; set; }
        public string PublicIdForResult { get; set; }
        public string Description { get; set; }
        //public bool? HideCallToAction { get; set; }
        public bool EnableCallToActionButton { get; set; }
        public string ActionButtonURL { get; set; }
        public bool? OpenLinkInNewTab { get; set; }
        public string ActionButtonTxtSize { get; set; }
        public string ActionButtonColor { get; set; }
        public string ActionButtonTitleColor { get; set; }
        public string ActionButtonText { get; set; }
        public bool AutoPlay { get; set; }
        public string SecondsToApply { get; set; }
        public bool? VideoFrameEnabled { get; set; }
        public bool ShowExternalTitle { get; set; }
        public bool ShowInternalTitle { get; set; }
        public bool ShowDescription { get; set; }
        public int DisplayOrderForTitle { get; set; }
        public int DisplayOrderForTitleImage { get; set; }
        public int DisplayOrderForDescription { get; set; }
        public int DisplayOrderForNextButton { get; set; }
        public List<string> MsgVariables { get; set; }

        public QuizResult MapRequestToEntity(QuizResultRequest QuizResultRequestObj)
        {
            QuizResult obj = new QuizResult();

            obj.QuizId = QuizResultRequestObj.QuizId;
            obj.ResultId = QuizResultRequestObj.ResultId;
            obj.ShowResultImage = QuizResultRequestObj.ShowResultImage;
            obj.Title = QuizResultRequestObj.Title;
            obj.InternalTitle = QuizResultRequestObj.InternalTitle;
            obj.Image = QuizResultRequestObj.Image;
            obj.EnableMediaFile = QuizResultRequestObj.EnableMediaFile;
            obj.PublicIdForResult = QuizResultRequestObj.PublicIdForResult;
            obj.Description = QuizResultRequestObj.Description;
            obj.EnableCallToActionButton = QuizResultRequestObj.EnableCallToActionButton;
            obj.ActionButtonURL = QuizResultRequestObj.ActionButtonURL;
            obj.OpenLinkInNewTab = QuizResultRequestObj.OpenLinkInNewTab;
            obj.ActionButtonTxtSize = QuizResultRequestObj.ActionButtonTxtSize;
            obj.ActionButtonColor = QuizResultRequestObj.ActionButtonColor;
            obj.ActionButtonTitleColor = QuizResultRequestObj.ActionButtonTitleColor;
            obj.ActionButtonText = QuizResultRequestObj.ActionButtonText;
            obj.AutoPlay = QuizResultRequestObj.AutoPlay;
            obj.SecondsToApply = QuizResultRequestObj.SecondsToApply;
            obj.VideoFrameEnabled = QuizResultRequestObj.VideoFrameEnabled;
            obj.ShowExternalTitle = QuizResultRequestObj.ShowExternalTitle;
            obj.ShowInternalTitle = QuizResultRequestObj.ShowInternalTitle;
            obj.ShowDescription = QuizResultRequestObj.ShowDescription;
            obj.DisplayOrderForTitle = QuizResultRequestObj.DisplayOrderForTitle;
            obj.DisplayOrderForTitleImage = QuizResultRequestObj.DisplayOrderForTitleImage;
            obj.DisplayOrderForDescription = QuizResultRequestObj.DisplayOrderForDescription;
            obj.DisplayOrderForNextButton = QuizResultRequestObj.DisplayOrderForNextButton;
            obj.MsgVariables = QuizResultRequestObj.MsgVariables;

            return obj;
        }

        public class QuizResultRequestExample : IExamplesProvider
        {
            public object GetExamples()
            {
                var templateAttachmentList = new List<TemplateAttachment>();

                templateAttachmentList.Add(new TemplateAttachment()
                {
                    FileName = string.Empty,
                    FileUrl = string.Empty,
                    FileIdentifier = string.Empty
                });

                return new
                {
                    QuizId = 1,
                    ResultId = 1,
                    ShowResultImage = false,
                    Title = string.Empty,
                    InternalTitle = string.Empty,
                    Image = string.Empty,
                    EnableMediaFile = false,
                    PublicIdForResult = string.Empty,
                    Description = string.Empty,
                    EnableCallToActionButton = true,
                    ActionButtonURL = string.Empty,
                    OpenLinkInNewTab = true,
                    ActionButtonTxtSize = "24 px",
                    ActionButtonColor = "#62afe0",
                    ActionButtonTitleColor = "#ffffff",
                    ActionButtonText = string.Empty,
                    AutoPlay= false,
                    ShowExternalTitle = true,
                    ShowInternalTitle = true,
                    ShowDescription = true,
                    DisplayOrderForTitleImage = 1,
                    DisplayOrderForTitle = 2,
                    DisplayOrderForDescription = 3,
                    DisplayOrderForNextButton = 4,
                    TemplateAttachmentList = templateAttachmentList,
                };
            }
        }
    }

    public class QuizResultSettingRequest
    {
        public int QuizId { get; set; }
        public bool? ShowScoreValue { get; set; }
        public bool? ShowCorrectAnswer { get; set; }
        public string CustomTxtForScoreValueInResult { get; set; }
        public string CustomTxtForAnswerKey { get; set; }
        public string CustomTxtForYourAnswer { get; set; }
        public string CustomTxtForCorrectAnswer { get; set; }
        public string CustomTxtForExplanation { get; set; }
        public int ResultId { get; set; }
        public bool ShowLeadUserForm { get; set; }
        public bool AutoPlay { get; set; }

        public QuizResultSetting MapRequestToEntity(QuizResultSettingRequest QuizResultSettingRequestObj)
        {
            QuizResultSetting obj = new QuizResultSetting();

            obj.QuizId = QuizResultSettingRequestObj.QuizId;
            obj.ShowScoreValue = QuizResultSettingRequestObj.ShowScoreValue;
            obj.ShowCorrectAnswer = QuizResultSettingRequestObj.ShowCorrectAnswer;
            obj.CustomTxtForScoreValueInResult = QuizResultSettingRequestObj.CustomTxtForScoreValueInResult;
            obj.CustomTxtForAnswerKey = QuizResultSettingRequestObj.CustomTxtForAnswerKey;
            obj.CustomTxtForYourAnswer = QuizResultSettingRequestObj.CustomTxtForYourAnswer;
            obj.CustomTxtForCorrectAnswer = QuizResultSettingRequestObj.CustomTxtForCorrectAnswer;
            obj.CustomTxtForExplanation = QuizResultSettingRequestObj.CustomTxtForExplanation;
            obj.ResultId = QuizResultSettingRequestObj.ResultId;
            obj.ShowLeadUserForm = QuizResultSettingRequestObj.ShowLeadUserForm;
            obj.AutoPlay = QuizResultSettingRequestObj.AutoPlay;

            return obj;
        }

        public class QuizResultSettingRequesttExample : IExamplesProvider
        {
            public object GetExamples()
            {
                return new
                {
                    QuizId = 1,
                    ShowScoreValue = true,
                    ShowCorrectAnswer = true,
                    CustomTxtForScoreValueInResult = string.Empty,
                    CustomTxtForAnswerKey = string.Empty,
                    CustomTxtForYourAnswer = string.Empty,
                    CustomTxtForCorrectAnswer = true,
                    CustomTxtForExplanation = string.Empty,
                    ResultId = 0,
                    ShowLeadUserForm = true,
                    AutoPlay = true
                };
            }
        }
    }

    public class QuizBranchingLogicRequest
    {
        public int StartType { get; set; }
        public int StartTypeId { get; set; }
        public List<BranchingLogicRequest> QuestionList { get; set; }
        public List<ResultBranchingLogicRequest> ResultList { get; set; }
        public List<ContentBranchingLogicRequest> ContentList { get; set; }
    }

    public class BranchingLogicRequest
    {
        public int QuizId { get; set; }
        public string QuestionId { get; set; }
        public bool IsDisabled { get; set; }
        public List<AnswerInQuestions> AnswerList { get; set; }

        public class AnswerInQuestions
        {
            public string AnswerId { get; set; }
            public string LinkedToType { get; set; }
            public string LinkedQuestionId { get; set; }
            public string LinkedResultId { get; set; }
            public string LinkedContentId { get; set; }
            public int? XCordinate { get; set; }
            public int? YCordinate { get; set; }

        }

        public QuizBranchingLogic MapRequestToEntity(QuizBranchingLogicRequest BranchingLogicObj)
        {
            QuizBranchingLogic lst = new QuizBranchingLogic();

            lst.QuestionList = new List<BranchingLogicModel>();
            lst.StartType = (Helpers.BranchingLogicEnum)BranchingLogicObj.StartType;
            lst.StartTypeId = BranchingLogicObj.StartTypeId;
            lst.ContentList = new List<ContentBranchingLogic>();
            lst.ResultList = new List<ResultBranchingLogic>();

            foreach (var QuizBranchingLogicRequestObj in BranchingLogicObj.QuestionList)
            {
                BranchingLogicModel obj = new BranchingLogicModel();

                obj.QuizId = QuizBranchingLogicRequestObj.QuizId;
                obj.QuestionId = Convert.ToInt32(QuizBranchingLogicRequestObj.QuestionId);
                obj.IsDisabled = QuizBranchingLogicRequestObj.IsDisabled;
                obj.AnswerList = new List<BranchingLogicModel.AnswerInQuestions>();

                if (QuizBranchingLogicRequestObj.AnswerList != null)
                {
                    foreach (var answer in QuizBranchingLogicRequestObj.AnswerList)
                    {
                        obj.AnswerList.Add(new BranchingLogicModel.AnswerInQuestions
                        {
                            AnswerId = Convert.ToInt32(answer.AnswerId),
                            LinkedToType = Convert.ToInt32(answer.LinkedToType),
                            QuestionId = Convert.ToInt32(answer.LinkedQuestionId == string.Empty ? "0" : answer.LinkedQuestionId),
                            ResultId = Convert.ToInt32(answer.LinkedResultId == string.Empty ? "0" : answer.LinkedResultId),
                            XCordinate = answer.XCordinate,
                            YCordinate = answer.YCordinate
                        });
                    }
                }
                lst.QuestionList.Add(obj);
            }
            if (BranchingLogicObj.ContentList != null)
            {
                foreach (var ContentBranchingLogicRequestObj in BranchingLogicObj.ContentList)
                {
                    lst.ContentList.Add(new ContentBranchingLogic()
                    {
                        ContentId = ContentBranchingLogicRequestObj.ContentId,
                        IsDisabled = ContentBranchingLogicRequestObj.IsDisabled,
                        LinkedContentId = ContentBranchingLogicRequestObj.LinkedContentId,
                        LinkedQuestionId = ContentBranchingLogicRequestObj.LinkedQuestionId,
                        LinkedResultId = ContentBranchingLogicRequestObj.LinkedResultId,
                        LinkedToType = ContentBranchingLogicRequestObj.LinkedToType,
                        QuizId = ContentBranchingLogicRequestObj.QuizId,
                        XCordinate = ContentBranchingLogicRequestObj.XCordinate,
                        YCordinate = ContentBranchingLogicRequestObj.YCordinate
                    });
                }
            }
            if (BranchingLogicObj.ResultList != null)
            {
                foreach (var ResultBranchingLogicRequestObj in BranchingLogicObj.ResultList)
                {
                    lst.ResultList.Add(new ResultBranchingLogic()
                    {
                        LinkedActionId = ResultBranchingLogicRequestObj.LinkedActionId,
                        LinkedContentId = ResultBranchingLogicRequestObj.LinkedContentId,
                        LinkedToType = ResultBranchingLogicRequestObj.LinkedToType,
                        IsDisabled = ResultBranchingLogicRequestObj.IsDisabled,
                        ResultId = ResultBranchingLogicRequestObj.ResultId,
                        XCordinate = ResultBranchingLogicRequestObj.XCordinate,
                        YCordinate = ResultBranchingLogicRequestObj.YCordinate
                    });
                }
            }

            return lst;
        }

        public class QuizBranchingLogicRequestExample : IExamplesProvider
        {
            public object GetExamples()
            {
                var answList = new List<AnswerInQuestions>();

                answList.Add(new AnswerInQuestions
                {
                    AnswerId = "1",
                    LinkedToType = "1",
                    LinkedQuestionId = "1",
                    LinkedResultId = string.Empty,
                    LinkedContentId = string.Empty,
                    XCordinate = 0,
                    YCordinate = 0
                });

                var quesList = new List<BranchingLogicRequest>();

                quesList.Add(new BranchingLogicRequest
                {
                    QuizId = 1,
                    QuestionId = "2",
                    IsDisabled = false,
                    AnswerList = answList
                });
                var contList = new List<ContentBranchingLogicRequest>();

                contList.Add(new ContentBranchingLogicRequest
                {
                    QuizId = 1,
                    ContentId = 0,
                    IsDisabled = false,
                    LinkedToType = 1,
                    LinkedQuestionId = 1,
                    LinkedResultId = 0,
                    LinkedContentId = 0,
                    XCordinate = 0,
                    YCordinate = 0
                });
                var resultList = new List<ResultBranchingLogicRequest>();

                resultList.Add(new ResultBranchingLogicRequest
                {
                    QuizId = 1,
                    ResultId = 0,
                    IsDisabled = false,
                    LinkedToType = 2,
                    LinkedActionId = 0,
                    LinkedContentId = 0,
                    XCordinate = 0,
                    YCordinate = 0
                });
                return new
                {
                    startType = 1,
                    startTypeID = 1,
                    QuestionList = quesList,
                    ResultList = resultList,
                    ContentList = contList
                };
            }
        }
    }

    public class ResultBranchingLogicRequest
    {
        public int QuizId { get; set; }
        public int ResultId { get; set; }
        public int LinkedToType { get; set; }
        public int? LinkedActionId { get; set; }
        public int? LinkedContentId { get; set; }
        public bool IsDisabled { get; set; }
        public int? XCordinate { get; set; }
        public int? YCordinate { get; set; }
    }
    public class ContentBranchingLogicRequest
    {
        public int QuizId { get; set; }
        public int ContentId { get; set; }
        public bool IsDisabled { get; set; }
        public int LinkedToType { get; set; }
        public int? LinkedQuestionId { get; set; }
        public int? LinkedResultId { get; set; }
        public int? LinkedContentId { get; set; }
        public int? XCordinate { get; set; }
        public int? YCordinate { get; set; }
    }

    public class QuizResultRedirectRequest
    {
        public int QuizId { get; set; }
        public int ResultId { get; set; }
        public string ResultTitle { get; set; }
        public bool IsRedirectOn { get; set; }
        public string RedirectResultTo { get; set; }

        public List<QuizResultRedirect> MapRequestToEntity(List<QuizResultRedirectRequest> QuizResultRedirectRequestObjList)
        {
            List<QuizResultRedirect> obj = new List<QuizResultRedirect>();

            foreach (var item in QuizResultRedirectRequestObjList)
            {
                obj.Add(new QuizResultRedirect
                {
                    QuizId = item.QuizId,
                    ResultId = item.ResultId,
                    ResultTitle = item.ResultTitle,
                    IsRedirectOn = item.IsRedirectOn,
                    RedirectResultTo = !item.IsRedirectOn ? string.Empty : item.RedirectResultTo
                });
            }

            return obj;
        }

        public class QuizResultRedirectRequestExample : IExamplesProvider
        {
            public object GetExamples()
            {
                return new
                {
                    QuizId = 1,
                    ResultId = 1,
                    ResultTitle = string.Empty,
                    IsRedirectOn = false,
                    RedirectResultTo = string.Empty
                };
            }
        }
    }

    public class QuizActionRequest
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public int? AppointmentId { get; set; }
        public int? AutomationId { get; set; }
        public int ActionType { get; set; }
        public string ReportEmails { get; set; }
        public string Title { get; set; }
        public List<int> CalendarIds { get; set; }

        public QuizAction MapRequestToEntity(QuizActionRequest QuizResultRequestObj)
        {
            QuizAction obj = new QuizAction();

            obj.Id = QuizResultRequestObj.Id;
            obj.QuizId = QuizResultRequestObj.QuizId;
            obj.AppointmentId = QuizResultRequestObj.AppointmentId;
            obj.ReportEmails = QuizResultRequestObj.ReportEmails;
            obj.AutomationId = QuizResultRequestObj.AutomationId;
            obj.Title = QuizResultRequestObj.Title;
            obj.ActionType = QuizResultRequestObj.ActionType;
            obj.CalendarIds = QuizResultRequestObj.CalendarIds;

            return obj;
        }

        public class QuizActionRequestExample : IExamplesProvider
        {
            public object GetExamples()
            {
                return new
                {
                    Id = 1,
                    QuizId = 1,
                    AppointmentId = 0,
                    ReportEmails = string.Empty,
                    AutomationId = 0,
                    Title = string.Empty,
                    ActionType = 1,
                    CalendarIds = new List<int>()
                };
            }
        }
    }

    public class QuizContentRequest
    {
        public int QuizId { get; set; }
        public int Id { get; set; }
        public string ContentTitle { get; set; }
        public bool ShowTitle { get; set; }
        public string ContentTitleImage { get; set; }
        public bool EnableMediaFileForTitle { get; set; }
        public string PublicIdForContentTitle { get; set; }
        public bool? ShowContentTitleImage { get; set; }
        public string ContentDescription { get; set; }
        public bool ShowDescription { get; set; }
        public string ContentDescriptionImage { get; set; }
        public bool EnableMediaFileForDescription { get; set; }
        public string PublicIdForContentDescription { get; set; }
        public bool? ShowContentDescriptionImage { get; set; }
        public string AliasTextForNextButton { get; set; }
        public bool EnableNextButton { get; set; }
        public bool AutoPlay { get; set; }
        public string SecondsToApply { get; set; }
        public bool? VideoFrameEnabled { get; set; }
        public bool AutoPlayForDescription { get; set; }
        public string SecondsToApplyForDescription { get; set; }
        public bool? DescVideoFrameEnabled { get; set; }
        public int DisplayOrderForTitle { get; set; }
        public int DisplayOrderForTitleImage { get; set; }
        public int DisplayOrderForDescription { get; set; }
        public int DisplayOrderForDescriptionImage { get; set; }
        public int DisplayOrderForNextButton { get; set; }

        public QuizContent MapRequestToEntity(QuizContentRequest QuizContentRequestObj)
        {
            QuizContent obj = new QuizContent();

            obj.QuizId = QuizContentRequestObj.QuizId;
            obj.Id = QuizContentRequestObj.Id;
            obj.ContentTitle = QuizContentRequestObj.ContentTitle;
            obj.ShowTitle = QuizContentRequestObj.ShowTitle;
            obj.ContentTitleImage = QuizContentRequestObj.ContentTitleImage;
            obj.EnableMediaFileForTitle = QuizContentRequestObj.EnableMediaFileForTitle;
            obj.PublicIdForContentTitle = QuizContentRequestObj.PublicIdForContentTitle;
            obj.ShowContentTitleImage = QuizContentRequestObj.ShowContentTitleImage;
            obj.ContentDescription = QuizContentRequestObj.ContentDescription;
            obj.ShowDescription = QuizContentRequestObj.ShowDescription;
            obj.ContentDescriptionImage = QuizContentRequestObj.ContentDescriptionImage;
            obj.EnableMediaFileForDescription = QuizContentRequestObj.EnableMediaFileForDescription;
            obj.PublicIdForContentDescription = QuizContentRequestObj.PublicIdForContentDescription;
            obj.ShowContentDescriptionImage = QuizContentRequestObj.ShowContentDescriptionImage;
            obj.AliasTextForNextButton = QuizContentRequestObj.AliasTextForNextButton;
            obj.EnableNextButton = QuizContentRequestObj.EnableNextButton;
            obj.AutoPlay = QuizContentRequestObj.AutoPlay;
            obj.SecondsToApply = QuizContentRequestObj.SecondsToApply;
            obj.VideoFrameEnabled = QuizContentRequestObj.VideoFrameEnabled;
            obj.AutoPlayForDescription = QuizContentRequestObj.AutoPlayForDescription;
            obj.SecondsToApplyForDescription = QuizContentRequestObj.SecondsToApplyForDescription;
            obj.DescVideoFrameEnabled = QuizContentRequestObj.DescVideoFrameEnabled;
            obj.DisplayOrderForTitle = QuizContentRequestObj.DisplayOrderForTitle;
            obj.DisplayOrderForTitleImage = QuizContentRequestObj.DisplayOrderForTitleImage;
            obj.DisplayOrderForDescription = QuizContentRequestObj.DisplayOrderForDescription;
            obj.DisplayOrderForDescriptionImage = QuizContentRequestObj.DisplayOrderForDescriptionImage;
            obj.DisplayOrderForNextButton = QuizContentRequestObj.DisplayOrderForNextButton;

            return obj;
        }

        public class QuizContentRequestExample : IExamplesProvider
        {
            public object GetExamples()
            {
                return new
                {
                    QuizId = 1,
                    Id = 1,
                    ContentTitle = string.Empty,
                    ShowTitle = true,
                    ContentTitleImage = string.Empty,
                    EnableMediaFileForTitle = false,
                    PublicIdForContentTitle = string.Empty,
                    ShowContentTitleImage = false,
                    ContentDescription = string.Empty,
                    ShowDescription = true,
                    ContentDescriptionImage = string.Empty,
                    EnableMediaFileForDescription = false,
                    PublicIdForContentDescription = string.Empty,
                    ShowContentDescriptionImage = false,
                    AliasTextForNextButton = string.Empty,
                    EnableNextButton = true,
                    AutoPlay = false,
                    AutoPlayForDescription = false,
                    DisplayOrderForTitleImage = 1,
                    DisplayOrderForTitle = 2,
                    DisplayOrderForDescriptionImage = 3,
                    DisplayOrderForDescription = 4,
                    DisplayOrderForNextButton = 5
                };
            }
        }
    }

    public class QuizBranchingLogicLinksListRequest
    {
        public int QuizId { get; set; }

        public List<QuizBranchingLogicLinks> QuizBranchingLogic { get; set; }
        public class QuizBranchingLogicLinks
        {
            public int Type { get; set; }
            public string Id { get; set; }
            public List<BranchiLinks> Links { get; set; }
            public string[] Position { get; set; }
        }

        public class BranchiLinks
        {
            public string FromId { get; set; }
            public string ToId { get; set; }
            public int FromType { get; set; }
            public int ToType { get; set; }
        }
        public QuizBranchingLogicLinksList MapRequestToEntity(QuizBranchingLogicLinksListRequest QuizBranchingLogicLinksListRequestObj)
        {
            QuizBranchingLogicLinksList response = new QuizBranchingLogicLinksList();

            response.QuizId = QuizBranchingLogicLinksListRequestObj.QuizId;

            response.QuizBranchingLogicLinks = new List<Services.Model.QuizBranchingLogicLinks>();

            foreach (var quizBranchingLogic in QuizBranchingLogicLinksListRequestObj.QuizBranchingLogic)
            {
                var quizObj = new Services.Model.QuizBranchingLogicLinks();

                quizObj.Position = quizBranchingLogic.Position;
                quizObj.ObjectType = (Helpers.BranchingLogicEnum)quizBranchingLogic.Type;
                quizObj.ObjectTypeId = quizBranchingLogic.Id;
                quizObj.Links = new List<Services.Model.BranchingLinks>();

                foreach (var item in quizBranchingLogic.Links)
                {
                    quizObj.Links.Add(new Services.Model.BranchingLinks
                    {
                        FromId = item.FromId,
                        FromType = (Helpers.BranchingLogicEnum)item.FromType,
                        ToId = item.ToId,
                        ToType = (Helpers.BranchingLogicEnum)item.ToType,
                    });
                }

                response.QuizBranchingLogicLinks.Add(quizObj);
            }
            return response;
        }
        public class QuizBranchingLogicLinksListRequestExample : IExamplesProvider
        {
            public object GetExamples()
            {
                var quizBranchingLogicLinksList = new List<QuizBranchingLogicLinks>();

                var BranchiLinksList = new List<BranchiLinks>();

                BranchiLinksList.Add(new BranchiLinks
                {
                    FromId = "1",
                    ToId = "2",
                    FromType = 1,
                    ToType = 1,


                });
                quizBranchingLogicLinksList.Add(new QuizBranchingLogicLinks
                {
                    Type = 1,
                    Id = "1",
                    Links = BranchiLinksList,
                    Position = new string[] { "0", "0" }
                });
                return new
                {
                    QuizId = "1",
                    QuizBranchingLogic = quizBranchingLogicLinksList
                };
            }
        }
    }


    public class QuizAttachmentRequest
    {
        public int QuizId { get; set; }
        public List<Attachment> Attachments { get; set; }
        public class Attachment
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string PublicIdForAttachment { get; set; }
        }

        public QuizAttachment MapRequestToEntity(QuizAttachmentRequest QuizAttachmentRequestObj)
        {
            QuizAttachment obj = new QuizAttachment();

            obj.QuizId = QuizAttachmentRequestObj.QuizId;
            obj.Attachments = new List<QuizAttachment.Attachment>();
            if (QuizAttachmentRequestObj.Attachments != null)
                foreach (var attachment in QuizAttachmentRequestObj.Attachments)
                {
                    obj.Attachments.Add(new QuizAttachment.Attachment
                    {
                        Title = attachment.Title,
                        Description = attachment.Description,
                        PublicIdForAttachment = attachment.PublicIdForAttachment
                    });

                }
            return obj;
        }

        public class QuizAttachmentRequestExample : IExamplesProvider
        {
            public object GetExamples()
            {
                var attachment = new List<Attachment>();
                attachment.Add(new Attachment
                {
                    Description = string.Empty,
                    Title = string.Empty,
                    PublicIdForAttachment = string.Empty,
                });
                return new
                {
                    QuizId = 1,
                    Attachments = attachment
                };
            }
        }
    }

    public class QuizShareRequest
    {
        public int QuizId { get; set; }
        public bool Recruiter { get; set; }
        public bool Lead { get; set; }
        public bool? JobRockAcademy { get; set; }
        public bool? TechnicalRecruiter { get; set; }
        public int? Module { get; set; }

        public QuizShare MapRequestToEntity(QuizShareRequest QuizShareRequestObj)
        {
            QuizShare obj = new QuizShare();

            obj.QuizId = QuizShareRequestObj.QuizId;
            obj.UserType = QuizShareRequestObj.Recruiter ? ((int)Helpers.UserTypeEnum.Recruiter).ToString() : string.Empty;
            if (QuizShareRequestObj.Lead) obj.UserType = obj.UserType + "," + ((int)Helpers.UserTypeEnum.Lead).ToString();
            if (QuizShareRequestObj.JobRockAcademy.HasValue && QuizShareRequestObj.JobRockAcademy.Value) obj.UserType = obj.UserType + "," + ((int)Helpers.UserTypeEnum.JobRockAcademy).ToString();
            if (QuizShareRequestObj.TechnicalRecruiter.HasValue && QuizShareRequestObj.TechnicalRecruiter.Value) obj.UserType = obj.UserType + "," + ((int)Helpers.UserTypeEnum.TechnicalRecruiter).ToString();
            obj.ModuleType = QuizShareRequestObj.Module;
            return obj;
        }

        public class QuizShareRequestExample : IExamplesProvider
        {
            public object GetExamples()
            {
                return new
                {
                    QuizId = 1,
                    Recruiter = false,
                    Lead = false,
                    JobRockAcademy = false,
                    TechnicalRecruiter = false,
                    Module = 0
                };
            }
        }
    }

    public class QuizBadgeRequest
    {
        public int QuizId { get; set; }
        public int Id { get; set; }
        public string Title { get; set; }
        public bool ShowTitle { get; set; }
        public string Image { get; set; }
        public bool ShowImage { get; set; }
        public bool EnableMediaFile { get; set; }
        public string PublicIdForBadge { get; set; }
        public bool AutoPlay { get; set; }
        public string SecondsToApply { get; set; }
        public bool? VideoFrameEnabled { get; set; }
        public int DisplayOrderForTitle { get; set; }
        public int DisplayOrderForTitleImage { get; set; }

        public QuizBadge MapRequestToEntity(QuizBadgeRequest QuizResultRequestObj)
        {
            QuizBadge obj = new QuizBadge();

            obj.QuizId = QuizResultRequestObj.QuizId;
            obj.Id = QuizResultRequestObj.Id;
            obj.Title = QuizResultRequestObj.Title;
            obj.ShowTitle = QuizResultRequestObj.ShowTitle;
            obj.Image = QuizResultRequestObj.Image;
            obj.ShowImage = QuizResultRequestObj.ShowImage;
            obj.EnableMediaFile = QuizResultRequestObj.EnableMediaFile;
            obj.PublicIdForBadge = QuizResultRequestObj.PublicIdForBadge;
            obj.AutoPlay = QuizResultRequestObj.AutoPlay;
            obj.SecondsToApply = QuizResultRequestObj.SecondsToApply;
            obj.VideoFrameEnabled = QuizResultRequestObj.VideoFrameEnabled;
            obj.DisplayOrderForTitle = QuizResultRequestObj.DisplayOrderForTitle;
            obj.DisplayOrderForTitleImage = QuizResultRequestObj.DisplayOrderForTitleImage;

            return obj;
        }

        public class QuizBadgeRequestExample : IExamplesProvider
        {
            public object GetExamples()
            {
                return new
                {
                    QuizId = 1,
                    Id = 1,
                    Title = string.Empty,
                    ShowTitle = true,
                    Image = string.Empty,
                    ShowImage = true,
                    EnableMediaFileForTitle = false,
                    PublicIdForBadge = string.Empty,
                    AutoPlay = false,
                    DisplayOrderForTitleImage = 1,
                    DisplayOrderForTitle = 2
                };
            }
        }
    }


    public class QuizAnsweTagsRequest
    {
        public int answerId { get; set; }
        public List<Category> Categories { get; set; }
        public class Category
        {
            public int CategoryId { get; set; }
            public string CategoryName { get; set; }
            public List<TagDetail> TagDetails { get; set; }

            public class TagDetail
            {
                public int TagId { get; set; }
                public string TagName { get; set; }
            }
        }


        public List<QuizAnsweTags> MapRequestToEntity(List<QuizAnsweTagsRequest> QuizAnsweTagsRequestObj)
        {
            List<QuizAnsweTags> objList = new List<QuizAnsweTags>();

            foreach (var QuizAnsweTagsRequestListObj in QuizAnsweTagsRequestObj)
            {

                QuizAnsweTags obj = new QuizAnsweTags();
                obj.answerId = QuizAnsweTagsRequestListObj.answerId;
                obj.Categories = new List<QuizAnsweTags.CategoryModel>();

                foreach (var category in QuizAnsweTagsRequestListObj.Categories)
                {
                    var categoryObj = new QuizAnsweTags.CategoryModel();

                    categoryObj.CategoryId = category.CategoryId;

                    categoryObj.CategoryName = category.CategoryName;

                    categoryObj.TagDetails = new List<QuizAnsweTags.CategoryModel.TagDetail>();

                    if (category.TagDetails != null)
                    {
                        foreach (var tag in category.TagDetails)
                        {
                            categoryObj.TagDetails.Add(new QuizAnsweTags.CategoryModel.TagDetail
                            {
                                TagId = tag.TagId,
                                TagName = tag.TagName
                            });
                        }
                    }

                    obj.Categories.Add(categoryObj);
                }

                objList.Add(obj);

            }
            return objList;
        }

        public class QuizAnsweTagsRequestExample : IExamplesProvider
        {
            public object GetExamples()
            {
                var categorylist = new List<QuizAnsweTags.CategoryModel>();

                var tagDetaillist = new List<QuizAnsweTags.CategoryModel.TagDetail>();
                tagDetaillist.Add(new QuizAnsweTags.CategoryModel.TagDetail
                {
                    TagId = 1,
                    TagName = string.Empty
                });

                categorylist.Add(new QuizAnsweTags.CategoryModel
                {
                    CategoryId = 0,
                    CategoryName = string.Empty,
                    TagDetails = tagDetaillist
                });

                return new
                {
                    answerId = 1,
                    Categories = categorylist,
                };
            }
        }
    }

    public class QuizResultRangeRequest
    {
        public int QuizId { get; set; }
        public List<Result> Results { get; set; }
        public class Result
        {
            public int ResultId { get; set; }
            public int? MinScore { get; set; }
            public int? MaxScore { get; set; }
        }

        public QuizResultRange MapRequestToEntity(QuizResultRangeRequest QuizResultRangeRequestObj)
        {
            QuizResultRange obj = new QuizResultRange();

            obj.QuizId = QuizResultRangeRequestObj.QuizId;
            obj.Results = new List<QuizResultRange.Result>();

            foreach (var QuizResultObj in QuizResultRangeRequestObj.Results)
            {
                QuizResultRange.Result QuizResultRangeResultobj = new QuizResultRange.Result();
                QuizResultRangeResultobj.ResultId = QuizResultObj.ResultId;
                QuizResultRangeResultobj.MinScore = QuizResultObj.MinScore;
                QuizResultRangeResultobj.MaxScore = QuizResultObj.MaxScore;
                obj.Results.Add(QuizResultRangeResultobj);
            }
            return obj;
        }
        public class QuizResultRangeRequestExample : IExamplesProvider
        {
            public object GetExamples()
            {
                var ResultList = new List<QuizResultRange.Result>();
                ResultList.Add(new QuizResultRange.Result
                {
                    ResultId = 0,
                    MinScore = 0,
                    MaxScore = 0
                });
                return new
                {
                    QuizId = 0,
                    Results = ResultList
                };
            }
        }
    }


    public class ResultCorrelationRequest
    {
        public int QuestionId { get; set; }
        public List<CorrelationRequest> CorrelationList { get; set; }

        public class CorrelationRequest
        {
            public int AnswerId { get; set; }
            public int ResultId { get; set; }
        }

        public int MinAnswer { get; set; }

        public int MaxAnswer { get; set; }

        public ResultCorrelation MapRequestToEntity(ResultCorrelationRequest resultCorrelationRequestObj)
        {
            ResultCorrelation objList = new ResultCorrelation();
            objList.QuestionId = resultCorrelationRequestObj.QuestionId;
            objList.MinAnswer = resultCorrelationRequestObj.MinAnswer;
            objList.MaxAnswer = resultCorrelationRequestObj.MaxAnswer;

            objList.CorrelationList = new List<Correlation>();
            foreach (var resultCorrelation in resultCorrelationRequestObj.CorrelationList)
            {
                objList.CorrelationList.Add(new Correlation
                {
                    AnswerId = resultCorrelation.AnswerId,
                    ResultId = resultCorrelation.ResultId
                });
            }
            return objList;
        }
        public class ResultCorrelationRequestExample : IExamplesProvider
        {
            public object GetExamples()
            {
                return new
                {
                    QuestionId = 0,
                    CorrelationList = new List<CorrelationRequest>() { new CorrelationRequest() { AnswerId = 0, ResultId = 0 } }
                };
            }
        }
    }

    public class PersonalityResultSettingRequest
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public string Title { get; set; }
        public int Status { get; set; }
        public int MaxResult { get; set; }
        public string GraphColor { get; set; }
        public string ButtonColor { get; set; }
        public string ButtonFontColor { get; set; }
        public string SideButtonText { get; set; }
        public bool IsFullWidthEnable { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public int? LastUpdatedBy { get; set; }

        public PersonalityResultSettingModel MapRequestToEntity(PersonalityResultSettingRequest personalityResultSettingRequestObj)
        {
            PersonalityResultSettingModel personalityResult = new PersonalityResultSettingModel();
            personalityResult.Id = personalityResultSettingRequestObj.Id;
            personalityResult.QuizId = personalityResultSettingRequestObj.QuizId;
            personalityResult.Title = personalityResultSettingRequestObj.Title;
            personalityResult.Status = personalityResultSettingRequestObj.Status;
            personalityResult.GraphColor = personalityResultSettingRequestObj.GraphColor;
            personalityResult.ButtonColor = personalityResultSettingRequestObj.ButtonColor;
            personalityResult.ButtonFontColor = personalityResultSettingRequestObj.ButtonFontColor;
            personalityResult.SideButtonText = personalityResultSettingRequestObj.SideButtonText;
            personalityResult.IsFullWidthEnable = personalityResultSettingRequestObj.IsFullWidthEnable;
            personalityResult.MaxResult = personalityResultSettingRequestObj.MaxResult;

            return personalityResult;
        }
        public class PersonalityResultSettingRequestExample : IExamplesProvider
        {
            public object GetExamples()
            {
                return new
                {
                    Id = 0,
                    QuizId = 0,
                    Title = string.Empty,
                    Status = 0,
                    GraphColor = string.Empty,
                    ButtonColor = string.Empty,
                    ButtonFontColor = string.Empty,
                    SideButtonText = string.Empty,
                    IsFullWidthEnable = default(bool)
                };
            }
        }
    }

    public class TextAnswerRequest
    {
        public int AnswerId { get; set; }
        public List<Answer> Answers { get; set; }
        public class Answer
        {
            public string AnswerText { get; set; }
            public string AnswerDescription { get; set; }
            public int? SubAnswerTypeId { get; set; }
            public string Comment { get; set; }
            public string AnswerSecondaryText { get; set; }
        }

        
        public List<TextAnswer> MapRequestToEntity(List<TextAnswerRequest> TextAnswerList)
        {
            List<TextAnswer> list = new List<TextAnswer>();
            if (TextAnswerList != null)
            {
                foreach (var textAnswerObj in TextAnswerList)
                {
                    TextAnswer obj = new TextAnswer();
                    obj.AnswerId = textAnswerObj.AnswerId;
                    obj.Answers = new List<TextAnswer.Answer>();
                    foreach (var answerObj in textAnswerObj.Answers)
                    {
                        TextAnswer.Answer answer = new TextAnswer.Answer();
                        answer.AnswerText = answerObj.AnswerText;
                        answer.AnswerDescription = answerObj.AnswerDescription;
                        answer.SubAnswerTypeId = answerObj.SubAnswerTypeId;
                        answer.Comment = answerObj.Comment;
                        answer.AnswerSecondaryText = answerObj.AnswerSecondaryText;
                        obj.Answers.Add(answer);
                    }
                    list.Add(obj);
                }
            }
            return list;
        }

        public class TextAnswerRequestExample : IExamplesProvider
        {
            public object GetExamples()
            {
                var AnswerList = new List<Answer>();
                AnswerList.Add(new Answer
                {
                    AnswerText = string.Empty,
                    SubAnswerTypeId = 0,
                    Comment = string.Empty,
                    AnswerSecondaryText = string.Empty
                });
                var TextAnswerList = new List<TextAnswerRequest>();
                TextAnswerList.Add(new TextAnswerRequest
                {
                    AnswerId = 0,
                    Answers = AnswerList

                });
                return new
                {
                    TextAnswerList
                };
            }
        }
    }

    public class QuizQuestionSettingRequest
    {
        public int QuestionId { get; set; }
        public bool ViewPreviousQuestion { get; set; }
        public bool EditAnswer { get; set; }
        public bool TimerRequired { get; set; }
        public string Time { get; set; }
        public bool AutoPlay { get; set; }

        public QuizCorrectAnswerSetting MapRequestToEntity(QuizQuestionSettingRequest QuizCorrectAnswerSettingRequestObj)
        {
            QuizCorrectAnswerSetting obj = new QuizCorrectAnswerSetting();

            obj.QuestionId = QuizCorrectAnswerSettingRequestObj.QuestionId;
            obj.ViewPreviousQuestion = QuizCorrectAnswerSettingRequestObj.ViewPreviousQuestion;
            obj.EditAnswer = QuizCorrectAnswerSettingRequestObj.EditAnswer;
            obj.TimerRequired = QuizCorrectAnswerSettingRequestObj.TimerRequired;
            obj.Time = QuizCorrectAnswerSettingRequestObj.Time;
            obj.AutoPlay = QuizCorrectAnswerSettingRequestObj.AutoPlay;
            return obj;
        }

        public class QuizQuestionSettingRequestExample : IExamplesProvider
        {
            public object GetExamples()
            {
                return new
                {
                    QuestionId = 1,
                    ViewPreviousQuestion = false,
                    EditAnswer = false,
                    TimerRequired = false,
                    Time = string.Empty,
                    AutoPlay = true
                };
            }
        }
    }

    public class QuizContentSettingRequest
    {
        public int Id { get; set; }
        public bool ViewPreviousQuestion { get; set; }
        public bool AutoPlay { get; set; }

        public QuizContent MapRequestToEntity(QuizContentSettingRequest QuizContentRequestObj)
        {
            QuizContent obj = new QuizContent();

            obj.Id = QuizContentRequestObj.Id;
            obj.ViewPreviousQuestion = QuizContentRequestObj.ViewPreviousQuestion;
            obj.AutoPlay = QuizContentRequestObj.AutoPlay;

            return obj;
        }

        public class QuizContentSettingRequestExample : IExamplesProvider
        {
            public object GetExamples()
            {
                return new
                {
                    Id = 1,
                    ViewPreviousQuestion = false
                };
            }
        }
    }

    public class QuizCoverSettingRequest
    {
        public int Id { get; set; }
        public bool AutoPlay { get; set; }

        public QuizCover MapRequestToEntity(QuizCoverSettingRequest QuizContentRequestObj)
        {
            QuizCover obj = new QuizCover();
            obj.QuizId = QuizContentRequestObj.Id;
            obj.AutoPlay = QuizContentRequestObj.AutoPlay;
            return obj;
        }

        public class QuizCoverSettingRequestExample : IExamplesProvider
        {
            public object GetExamples()
            {
                return new
                {
                    Id = 1,
                    AutoPlay = true
                };
            }
        }
    }
    public class UpdateQuizSettingRequest
    {
        public int QuizId { get; set; }
        public bool ViewPreviousQuestion { get; set; }
        public bool EditAnswer { get; set; }
        public bool ApplyToAll { get; set; }

        public QuizSetting MapRequestToEntity(UpdateQuizSettingRequest QuizCorrectAnswerSettingRequestObj)
        {
            QuizSetting obj = new QuizSetting();

            obj.QuizId = QuizCorrectAnswerSettingRequestObj.QuizId;
            obj.ViewPreviousQuestion = QuizCorrectAnswerSettingRequestObj.ViewPreviousQuestion;
            obj.EditAnswer = QuizCorrectAnswerSettingRequestObj.EditAnswer;
            obj.ApplyToAll = QuizCorrectAnswerSettingRequestObj.ApplyToAll;
            return obj;
        }

        public class UpdateQuizSettingRequestExample : IExamplesProvider
        {
            public object GetExamples()
            {
                return new
                {
                    QuizId = 1,
                    ViewPreviousQuestion = false,
                    EditAnswer = false,
                    ApplyToAll = false
                };
            }
        }
    }

    public class AnswerObjectFieldsRequest
    {
        public int AnswerId { get; set; }
        public string ObjectName { get; set; }
        public string FieldName { get; set; }
        public string Value { get; set; }
        public bool IsExternalSync { get; set; } = true;
        public bool IsCommentMapped { get; set; } = false;

        public List<ObjectFieldsDetails> MapRequestToEntity(List<AnswerObjectFieldsRequest> answerObjectFieldsRequest)
        {
            List<ObjectFieldsDetails> lst = new List<ObjectFieldsDetails>();

            foreach (var request in answerObjectFieldsRequest)
            {
                ObjectFieldsDetails obj = new ObjectFieldsDetails();
                obj.AnswerId = request.AnswerId;
                obj.ObjectName = request.ObjectName;
                obj.FieldName = request.FieldName;
                obj.Value = request.Value;
                obj.IsExternalSync = request.IsExternalSync;
                obj.IsCommentMapped= request.IsCommentMapped;
                lst.Add(obj);
            }
            return lst;
        }

        public class AnswerObjectFieldsRequestExample : IExamplesProvider
        {
            public object GetExamples()
            {
                return new
                {
                    AnswerId = 1,
                    ObjectName = string.Empty,
                    FieldName = string.Empty,
                    Value = string.Empty,
                    IsExternalSync = true,
                    IsCommentMapped = false
                };
            }
        }
    }

    public class QuizUsageTypeandTagDetailsRequest
    {
        public int QuizId { get; set; }
        public List<int> UsageTypes { get; set; }
        public List<int> TagIds { get; set; }

        public QuizUsageTypeandTagDetails MapRequestToEntity(QuizUsageTypeandTagDetailsRequest request)
        {
            QuizUsageTypeandTagDetails obj = new QuizUsageTypeandTagDetails();
            obj.QuizId = request.QuizId;
            obj.UsageTypes = request.UsageTypes;
            obj.TagIds = request.TagIds;

            return obj;
        }

        public class QuizUsageTypeandTagDetailsRequestExample : IExamplesProvider
        {
            public object GetExamples()
            {
                return new
                {
                    QuizId = 1,
                    UsageTypes = new List<int>(),
                    TagIds = new List<int>()
                };
            }
        }
    }
}