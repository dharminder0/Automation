using QuizApp.Helpers;
using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Response
{
    public class QuizListResponse : IResponse
    {
        public int Id { get; set; }
        public string QuizTitle { get; set; }
        public bool IsPublished { get; set; }
        public bool IsBranchingLogicEnabled { get; set; }
        public DateTime createdOn { get; set; }
        public DateTime? LastEditDate { get; set; }
        public bool IsViewOnly { get; set; }
        public string OfficeName { get; set; }
        public string OfficeId { get; set; }
        public string CreatedByName { get; set; }
        public long CreatedById { get; set; }
        public string PublishedCode { get; set; }
        public int QuizTypeId { get; set; }
        public bool IsCreatedByYou { get; set; }
        public QuizCoverDetails QuizCoverDetail { get; set; }
        public int NoOfQusetions { get; set; }
        public bool IsFavorited { get; set; }
        public List<int> UsageTypes { get; set; }
        public List<Tags> Tag { get; set; }
        public bool? IsWhatsAppChatBotOldVersion { get; set; }

        public IResponse MapEntityToResponse(Base EntityObj)
        {
            QuizListResponse response = new QuizListResponse();

            var quizObj = (LocalQuiz)EntityObj;

            response.Id = quizObj.Id;
            response.IsPublished = quizObj.IsPublished;
            response.QuizTitle = quizObj.QuizTitle == null ? string.Empty : quizObj.QuizTitle;
            response.createdOn = quizObj.CreatedOn;
            response.PublishedCode = quizObj.PublishedCode;
            response.LastEditDate = quizObj.LastEditDate.HasValue ? quizObj.LastEditDate : null;
            response.QuizTypeId = (int)quizObj.QuizType;
            response.QuizCoverDetail = new QuizCoverDetails();
            response.QuizCoverDetail.QuizCoverImage = quizObj.QuizCoverDetails.QuizCoverImage;
            response.QuizCoverDetail.QuizCoverTitle = quizObj.QuizCoverDetails.QuizCoverTitle;
            response.NoOfQusetions = quizObj.NoOfQusetions;
            response.IsCreatedByYou = quizObj.IsCreatedByYou;

            if (!string.IsNullOrEmpty(quizObj.AccessibleOfficeId) || quizObj.IsCreateStandardAutomation)
            {
                response.IsViewOnly = false;
            }
            else
            {
                response.IsViewOnly = true;
            }

            return response;
        }
    }
    public class QuizListResponses : IResponse
    {
        public int Id { get; set; }
        public string QuizTitle { get; set; }
        public bool IsPublished { get; set; }
        public DateTime createdOn { get; set; }
        public bool IsViewOnly { get; set; }
        public string PublishedCode { get; set; }
        public int QuizTypeId { get; set; }
        public bool IsCreatedByYou { get; set; }
        public string OfficeName { get; set; }
        public string CreatedByName { get; set; }
        public string OfficeId { get; set; }
        public long CreatedById { get; set; }
        public DateTime? LastEditDate { get; set; }
        public QuizCoverDetails QuizCoverDetail { get; set; }
        public int NoOfQusetions { get; set; }
        public bool IsFavorited { get; set; }
        public List<Tags> Tag { get; set; }

        public IResponse MapEntityToResponse(Base EntityObj)
        {
            QuizListResponses response = new QuizListResponses();

            var quizObj = (LocalQuiz)EntityObj;

            response.Id = quizObj.Id;
            response.IsPublished = quizObj.IsPublished;
            response.QuizTitle = quizObj.QuizTitle == null ? string.Empty : quizObj.QuizTitle;
            response.createdOn = quizObj.CreatedOn;
            response.PublishedCode = quizObj.PublishedCode;
            response.QuizTypeId = (int)quizObj.QuizType;
            response.QuizCoverDetail = new QuizCoverDetails();
            response.QuizCoverDetail.QuizCoverImage = quizObj.QuizCoverDetails.QuizCoverImage;
            response.QuizCoverDetail.QuizCoverTitle = quizObj.QuizCoverDetails.QuizCoverTitle;
            response.QuizCoverDetail.PublicId = quizObj.QuizCoverDetails.PublicIdForQuizCover;
            response.NoOfQusetions = quizObj.NoOfQusetions;
            response.IsCreatedByYou = quizObj.IsCreatedByYou;
            response.OfficeName = quizObj.OfficeName;
            response.OfficeId = quizObj.AccessibleOfficeId;
            response.CreatedById = quizObj.CreatedByID;
            response.LastEditDate = quizObj.LastEditDate;
            response.IsFavorited = quizObj.IsFavorited;
            response.Tag = quizObj.Tag;

            if (!string.IsNullOrEmpty(quizObj.AccessibleOfficeId) || quizObj.IsCreateStandardAutomation)
            {
                response.IsViewOnly = false;
            }
            else
            {
                response.IsViewOnly = true;
            }

            return response;
        }
    }

    public class QuizBrandingAndStyleResponse : IResponse
    {
        public int QuizId { get; set; }
        public string ImageFileURL { get; set; }
        public string PublicIdForImageFile { get; set; }
        public string BackgroundColor { get; set; }
        public string ButtonColor { get; set; }
        public string OptionColor { get; set; }
        public string ButtonFontColor { get; set; }
        public string OptionFontColor { get; set; }
        public string FontColor { get; set; }
        public string ButtonHoverColor { get; set; }
        public string ButtonHoverTextColor { get; set; }
        public string FontType { get; set; }
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

        public IResponse MapEntityToResponse(Base EntityObj)
        {
            QuizBrandingAndStyleResponse response = new QuizBrandingAndStyleResponse();

            var obj = (QuizBrandingAndStyleModel)EntityObj;

            response.QuizId = obj.QuizId;
            response.ImageFileURL = obj.ImageFileURL ?? string.Empty;
            response.PublicIdForImageFile = obj.PublicIdForFileURL ?? string.Empty;
            response.BackgroundColor = obj.BackgroundColor ?? string.Empty;
            response.ButtonColor = obj.ButtonColor ?? string.Empty;
            response.OptionColor = obj.OptionColor ?? string.Empty;
            response.ButtonFontColor = obj.ButtonFontColor ?? string.Empty;
            response.OptionFontColor = obj.OptionFontColor ?? string.Empty;
            response.FontColor = obj.FontColor ?? string.Empty;
            response.FontType = obj.FontType ?? string.Empty;
            response.ButtonHoverColor = obj.ButtonHoverColor ?? string.Empty;
            response.ButtonHoverTextColor = obj.ButtonHoverTextColor ?? string.Empty;
            response.BackgroundColorofSelectedAnswer = obj.BackgroundColorofSelectedAnswer ?? string.Empty;
            response.BackgroundColorofAnsweronHover = obj.BackgroundColorofAnsweronHover ?? string.Empty;
            response.AnswerTextColorofSelectedAnswer = obj.AnswerTextColorofSelectedAnswer ?? string.Empty;
            response.ApplyToAll = obj.ApplyToAll;
            response.IsBackType = obj.IsBackType;
            response.BackImageFileURL = obj.BackImageFileURL;
            response.BackColor = obj.BackColor;
            response.Opacity = obj.Opacity;
            response.LogoUrl = obj.LogoUrl;
            response.LogoPublicId = obj.LogoPublicId;
            response.BackgroundColorofLogo = obj.BackgroundColorofLogo;
            response.AutomationAlignment = obj.AutomationAlignment;
            response.LogoAlignment = obj.LogoAlignment;
            response.Flip = obj.Flip;
            response.Language = obj.Language;

            return response;
        }
    }

    public class QuizAccessSettingResponse : IResponse
    {
        public int QuizId { get; set; }
        public string AccessibleOfficeId { get; set; }

        public IResponse MapEntityToResponse(Base EntityObj)
        {
            QuizAccessSettingResponse response = new QuizAccessSettingResponse();

            var obj = (LocalQuiz)EntityObj;

            response.QuizId = obj.Id;
            response.AccessibleOfficeId = obj.AccessibleOfficeId;

            return response;
        }
    }

    public class QuizSocialShareSettingResponse : IResponse
    {
        public int QuizId { get; set; }
        public bool? HideSocialShareButtons { get; set; }
        public bool? EnableFacebookShare { get; set; }
        public bool? EnableTwitterShare { get; set; }
        public bool? EnableLinkedinShare { get; set; }

        public IResponse MapEntityToResponse(Base EntityObj)
        {
            QuizSocialShareSettingResponse response = new QuizSocialShareSettingResponse();

            var obj = (QuizSocialShareSetting)EntityObj;

            response.QuizId = obj.QuizId;
            response.HideSocialShareButtons = obj.HideSocialShareButtons ?? false;
            response.EnableFacebookShare = obj.EnableFacebookShare ?? false;
            response.EnableTwitterShare = obj.EnableTwitterShare ?? false;
            response.EnableLinkedinShare = obj.EnableLinkedinShare ?? false;

            return response;
        }
    }

    public class QuizCoverDetailsResponse : IResponse
    {
        public int QuizId { get; set; }
        public string QuizTitle { get; set; }
        public int QuizType { get; set; }
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
        public bool AutoPlay { get; set; }
        public string SecondsToApply { get; set; }
        public bool? VideoFrameEnabled { get; set; }
        public bool EnableNextButton { get; set; }
        public int DisplayOrderForTitle { get; set; }
        public int DisplayOrderForTitleImage { get; set; }
        public int DisplayOrderForDescription { get; set; }
        public int DisplayOrderForNextButton { get; set; }
        public List<string> MsgVariables { get; set; }
        public int QuestionId { get; set; }

        public IResponse MapEntityToResponse(Base EntityObj)
        {
            QuizCoverDetailsResponse response = new QuizCoverDetailsResponse();

            var obj = (QuizCover)EntityObj;

            response.QuizId = obj.QuizId;
            response.QuizTitle = obj.QuizTitle ?? string.Empty;
            response.QuizType = obj.QuizType;
            response.QuizCoverTitle = obj.QuizCoverTitle ?? string.Empty;
            response.ShowQuizCoverTitle = obj.ShowQuizCoverTitle;
            response.QuizCoverImage = obj.QuizCoverImage ?? string.Empty;
            response.ShowQuizCoverImage = obj.ShowQuizCoverImage;
            response.EnableMediaFile = obj.EnableMediaFile;
            response.PublicIdForQuizCover = obj.PublicIdForQuizCover;
            response.QuizCoverImgXCoordinate = obj.QuizCoverImgXCoordinate ?? 0;
            response.QuizCoverImgYCoordinate = obj.QuizCoverImgYCoordinate ?? 0;
            response.QuizCoverImgHeight = obj.QuizCoverImgHeight ?? 0;
            response.QuizCoverImgWidth = obj.QuizCoverImgWidth ?? 0;
            response.QuizCoverImgAttributionLabel = obj.QuizCoverImgAttributionLabel ?? string.Empty;
            response.QuizCoverImgAltTag = obj.QuizCoverImgAltTag ?? string.Empty;
            response.QuizDescription = obj.QuizDescription ?? string.Empty;
            response.ShowDescription = obj.ShowDescription;
            response.QuizStartButtonText = obj.QuizStartButtonText ?? string.Empty;
            response.AutoPlay = obj.AutoPlay;
            response.SecondsToApply = obj.SecondsToApply ?? "0";
            response.VideoFrameEnabled = obj.VideoFrameEnabled ?? false;
            response.EnableNextButton = obj.EnableNextButton;
            response.DisplayOrderForTitleImage = obj.DisplayOrderForTitleImage;
            response.DisplayOrderForTitle = obj.DisplayOrderForTitle;
            response.DisplayOrderForDescription = obj.DisplayOrderForDescription;
            response.DisplayOrderForNextButton = obj.DisplayOrderForNextButton;
            response.MsgVariables = obj.MsgVariables;

            return response;
        }
    }

    public class QuizQuestionDetailsResponse : IResponse
    {
        public int QuizType { get; set; }
        public int QuestionId { get; set; }
        public bool? ShowAnswerImage { get; set; }
        public string QuestionTitle { get; set; }
        public bool ShowTitle { get; set; }
        public string QuestionImage { get; set; }
        public bool EnableMediaFile { get; set; }
        public string PublicIdForQuestion { get; set; }
        public bool? ShowQuestionImage { get; set; }
        public int DisplayOrder { get; set; }
        public List<AnswerOption> AnswerList { get; set; }
        public string CorrectAnswerExplanation { get; set; }
        public bool? RevealCorrectAnswer { get; set; }
        public string AliasTextForCorrect { get; set; }
        public string AliasTextForIncorrect { get; set; }
        public string AliasTextForYourAnswer { get; set; }
        public string AliasTextForCorrectAnswer { get; set; }
        public string AliasTextForExplanation { get; set; }
        public string AliasTextForNextButton { get; set; }
        public string NextButtonText { get; set; }
        public string NextButtonTxtSize { get; set; }
        public string NextButtonTxtColor { get; set; }
        public string NextButtonColor { get; set; }
        public bool EnableNextButton { get; set; }
        public int AnswerType { get; set; }
        public int? MinAnswer { get; set; }
        public int? MaxAnswer { get; set; }
        public bool ViewPreviousQuestion { get; set; }
        public bool EditAnswer { get; set; }
        public bool TimerRequired { get; set; }
        public string Time { get; set; }
        public bool AutoPlay { get; set; }
        public string SecondsToApply { get; set; }
        public bool? VideoFrameEnabled { get; set; }
        public bool? DescVideoFrameEnabled { get; set; }
        public string Description { get; set; }
        public bool ShowDescription { get; set; }
        public string DescriptionImage { get; set; }
        public bool EnableMediaFileForDescription { get; set; }
        public string PublicIdForDescription { get; set; }
        public bool? ShowDescriptionImage { get; set; }
        public bool AutoPlayForDescription { get; set; }
        public string SecondsToApplyForDescription { get; set; }
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
        public int? LanguageId { get; set; }
        public int? TemplateId { get; set; }
        public string LanguageCode { get; set; }
        public bool IsMultiRating { get; set; }
        public List<string> MsgVariables { get; set; }
        public class AnswerOption
        {
            public int AnswerId { get; set; }
            public string AnswerText { get; set; }
            public string AnswerDescription { get; set; }
            public string AnswerImage { get; set; }
            public bool EnableMediaFile { get; set; }
            public string PublicIdForAnswer { get; set; }
            public bool? IsCorrectAnswer { get; set; }
            public int DisplayOrder { get; set; }
            public List<CategoryResponse> Categories { get; set; }
            public int? AssociatedScore { get; set; }
            public bool IsReadOnly { get; set; }
            public bool AutoPlay { get; set; }
            public string SecondsToApply { get; set; }
            public bool? VideoFrameEnabled { get; set; }
            public string OptionTextforRatingOne { get; set; }
            public string OptionTextforRatingTwo { get; set; }
            public string OptionTextforRatingThree { get; set; }
            public string OptionTextforRatingFour { get; set; }
            public string OptionTextforRatingFive { get; set; }
            public List<string> ListValues { get; set; }
            public int? RefId { get; set; }
            public ObjectFieldsDetails ObjectFieldsInAnswer { get; set; }
            public ObjectFieldsDetails ObjectFieldsInAnswerComment { get; set; }
            public class CategoryResponse
            {
                public string CategoryName { get; set; }
                public int CategoryId { get; set; }
                public List<TagDetail> TagDetails { get; set; }

                public class TagDetail
                {
                    public int TagId { get; set; }
                    public string TagName { get; set; }
                }
            }
            public class ObjectFieldsDetails
            {
                public string ObjectName { get; set; }
                public string FieldName { get; set; }
                public string Value { get; set; }
                public bool? IsExternalSync { get; set; }
                public bool IsCommentMapped { get; set; }
            }
        }

        public IResponse MapEntityToResponse(Base EntityObj)
        {
            QuizQuestionDetailsResponse response = new QuizQuestionDetailsResponse();

            var obj = (QuizQuestion)EntityObj;

            response.QuizType = obj.QuizType;
            response.QuestionId = obj.QuestionId;
            response.ShowAnswerImage = obj.ShowAnswerImage ?? false;
            response.QuestionTitle = obj.QuestionTitle ?? string.Empty;
            response.ShowTitle = obj.ShowTitle;
            response.QuestionImage = obj.QuestionImage ?? string.Empty;
            response.EnableMediaFile = obj.EnableMediaFile;
            response.PublicIdForQuestion = obj.PublicIdForQuestion ?? string.Empty;
            response.ShowQuestionImage = obj.ShowQuestionImage ?? false;
            response.DisplayOrder = obj.DisplayOrder;
            response.CorrectAnswerExplanation = obj.QuizCorrectAnswerSetting.CorrectAnswerExplanation ?? string.Empty;
            response.RevealCorrectAnswer = obj.QuizCorrectAnswerSetting.RevealCorrectAnswer ?? false;
            response.AliasTextForCorrect = obj.QuizCorrectAnswerSetting.AliasTextForCorrect ?? string.Empty;
            response.AliasTextForIncorrect = obj.QuizCorrectAnswerSetting.AliasTextForIncorrect ?? string.Empty;
            response.AliasTextForYourAnswer = obj.QuizCorrectAnswerSetting.AliasTextForYourAnswer ?? string.Empty;
            response.AliasTextForCorrectAnswer = obj.QuizCorrectAnswerSetting.AliasTextForCorrectAnswer ?? string.Empty;
            response.AliasTextForExplanation = obj.QuizCorrectAnswerSetting.AliasTextForExplanation ?? string.Empty;
            response.AliasTextForNextButton = obj.QuizCorrectAnswerSetting.AliasTextForNextButton ?? string.Empty;
            response.NextButtonText = obj.NextButtonText ?? string.Empty;
            response.NextButtonColor = obj.NextButtonColor ?? string.Empty;
            response.NextButtonTxtColor = obj.NextButtonTxtColor ?? string.Empty;
            response.NextButtonTxtSize = obj.NextButtonTxtSize ?? string.Empty;
            response.EnableNextButton = obj.EnableNextButton;
            response.AnswerType = obj.AnswerType;
            response.MinAnswer = obj.MinAnswer;
            response.MaxAnswer = obj.MaxAnswer;
            response.ViewPreviousQuestion = obj.ViewPreviousQuestion;
            response.EditAnswer = obj.EditAnswer;
            response.TimerRequired = obj.TimerRequired;
            response.Time = obj.Time;
            response.AutoPlay = obj.AutoPlay;
            response.SecondsToApply = obj.SecondsToApply ?? "0";
            response.VideoFrameEnabled = obj.VideoFrameEnabled ?? false;
            response.Description = obj.Description;
            response.ShowDescription = obj.ShowDescription;
            response.DescriptionImage = obj.DescriptionImage;
            response.EnableMediaFileForDescription = obj.EnableMediaFileForDescription;
            response.PublicIdForDescription = obj.PublicIdForDescription;
            response.ShowDescriptionImage = obj.ShowDescriptionImage;
            response.AutoPlayForDescription = obj.AutoPlayForDescription;
            response.SecondsToApplyForDescription = obj.SecondsToApplyForDescription ?? "0";
            response.DescVideoFrameEnabled = obj.DescVideoFrameEnabled ?? false;
            response.Type = obj.Type;
            response.DisplayOrderForTitle = obj.DisplayOrderForTitle;
            response.DisplayOrderForTitleImage = obj.DisplayOrderForTitleImage;
            response.DisplayOrderForDescription = obj.DisplayOrderForDescription;
            response.DisplayOrderForDescriptionImage = obj.DisplayOrderForDescriptionImage;
            response.DisplayOrderForAnswer = obj.DisplayOrderForAnswer;
            response.DisplayOrderForNextButton = obj.DisplayOrderForNextButton;
            response.EnableComment = obj.EnableComment;
            response.TopicTitle = obj.TopicTitle;
            response.AnswerStructureType = obj.AnswerStructureType.HasValue ? obj.AnswerStructureType.Value : (int)AnswerStructureTypeEnum.Default;
            //response.LanguageId = obj.LanguageId;
            response.LanguageCode = obj.LanguageCode;
            response.TemplateId = obj.TemplateId;
            response.LanguageCode = obj.LanguageCode;
            response.IsMultiRating = obj.IsMultiRating;
            response.MsgVariables = obj.MsgVariables;

            response.AnswerList = new List<AnswerOption>();

            if (obj.AnswerList != null)
            {
                foreach (var ans in obj.AnswerList)
                {
                    var answerOption = new AnswerOption();

                    answerOption.AnswerId = ans.AnswerId;
                    answerOption.AnswerText = ans.AnswerText ?? string.Empty;
                    answerOption.AnswerDescription = ans.AnswerDescription ?? string.Empty;
                    answerOption.AnswerImage = ans.AnswerImage ?? string.Empty;
                    answerOption.EnableMediaFile = ans.EnableMediaFile;
                    answerOption.PublicIdForAnswer = ans.PublicIdForAnswer ?? string.Empty;
                    answerOption.IsCorrectAnswer = ans.IsCorrectAnswer ?? false;
                    answerOption.DisplayOrder = ans.DisplayOrder;
                    answerOption.AssociatedScore = ans.AssociatedScore;
                    answerOption.IsReadOnly = ans.IsReadOnly;
                    answerOption.AutoPlay = ans.AutoPlay;
                    answerOption.SecondsToApply = ans.SecondsToApply ?? "0";
                    answerOption.VideoFrameEnabled = ans.VideoFrameEnabled ?? false;
                    answerOption.OptionTextforRatingOne = ans.OptionTextforRatingOne;
                    answerOption.OptionTextforRatingTwo = ans.OptionTextforRatingTwo;
                    answerOption.OptionTextforRatingThree = ans.OptionTextforRatingThree;
                    answerOption.OptionTextforRatingFour = ans.OptionTextforRatingFour;
                    answerOption.OptionTextforRatingFive = ans.OptionTextforRatingFive;
                    answerOption.ListValues = !string.IsNullOrWhiteSpace(ans.ListValues) ? ans.ListValues.Split(',').ToList() : new List<string>();
                    answerOption.RefId = ans.RefId;
                    answerOption.Categories = new List<AnswerOption.CategoryResponse>();

                    if (ans.Categories != null)
                    {
                        foreach (var category in ans.Categories)
                        {
                            var categoryObj = new AnswerOption.CategoryResponse();

                            categoryObj.CategoryName = category.CategoryName;
                            categoryObj.CategoryId = category.CategoryId;
                            categoryObj.TagDetails = new List<AnswerOption.CategoryResponse.TagDetail>();

                            if (category.TagDetails != null)
                            {
                                foreach (var tag in category.TagDetails)
                                {
                                    categoryObj.TagDetails.Add(new AnswerOption.CategoryResponse.TagDetail
                                    {
                                        TagId = tag.TagId,
                                        TagName = tag.TagName
                                    });
                                }
                            }
                            answerOption.Categories.Add(categoryObj);
                        }
                    }

                    if (ans.ObjectFieldsInAnswer != null)
                    {
                        answerOption.ObjectFieldsInAnswer = new AnswerOption.ObjectFieldsDetails();
                        answerOption.ObjectFieldsInAnswer.ObjectName = ans.ObjectFieldsInAnswer.ObjectName;
                        answerOption.ObjectFieldsInAnswer.FieldName = ans.ObjectFieldsInAnswer.FieldName;
                        answerOption.ObjectFieldsInAnswer.Value = ans.ObjectFieldsInAnswer.Value;
                        answerOption.ObjectFieldsInAnswer.IsExternalSync = ans.ObjectFieldsInAnswer.IsExternalSync ?? false;
                        answerOption.ObjectFieldsInAnswer.IsCommentMapped = ans.ObjectFieldsInAnswer?.IsCommentMapped ?? false;
                    }

                     if (ans.ObjectFieldsInAnswerComment != null) {
                        answerOption.ObjectFieldsInAnswerComment = new AnswerOption.ObjectFieldsDetails();
                        answerOption.ObjectFieldsInAnswerComment.ObjectName = ans.ObjectFieldsInAnswerComment.ObjectName;
                        answerOption.ObjectFieldsInAnswerComment.FieldName = ans.ObjectFieldsInAnswerComment.FieldName;
                        answerOption.ObjectFieldsInAnswerComment.Value = ans.ObjectFieldsInAnswerComment.Value;
                        answerOption.ObjectFieldsInAnswerComment.IsExternalSync = ans.ObjectFieldsInAnswerComment.IsExternalSync ?? false;
                        answerOption.ObjectFieldsInAnswerComment.IsCommentMapped = ans.ObjectFieldsInAnswerComment?.IsCommentMapped ?? false;
                    }

                    response.AnswerList.Add(answerOption);

                }

            }

            return response;
        }
    }

    public class AnswerOptionInQuestionResponse : IResponse
    {
        public int AnswerId { get; set; }
        public string AnswerText { get; set; }
        public string AnswerDescription { get; set; }
        public string AnswerImage { get; set; }
        public string PublicIdForAnswer { get; set; }

        public IResponse MapEntityToResponse(Base EntityObj)
        {
            AnswerOptionInQuestionResponse response = new AnswerOptionInQuestionResponse();

            var obj = (AnswerOptionInQuestion)EntityObj;

            response.AnswerId = obj.AnswerId;
            response.AnswerText = obj.AnswerText ?? string.Empty;
            response.AnswerDescription = obj.AnswerDescription ?? string.Empty;
            response.AnswerImage = obj.AnswerImage ?? string.Empty;
            response.PublicIdForAnswer = obj.PublicIdForAnswer ?? string.Empty;

            return response;
        }
    }

    public class QuizResultResponse : IResponse
    {
        public int ResultId { get; set; }
        public bool ShowResultImage { get; set; }
        public string Title { get; set; }
        public string InternalTitle { get; set; }
        public string Image { get; set; }
        public bool EnableMediaFile { get; set; }
        public string PublicIdForResult { get; set; }
        public string Description { get; set; }
        public bool? HideCallToAction { get; set; }
        public bool EnableCallToActionButton { get; set; }
        public string ActionButtonURL { get; set; }
        public bool? OpenLinkInNewTab { get; set; }
        public string ActionButtonTxtSize { get; set; }
        public string ActionButtonColor { get; set; }
        public string ActionButtonTitleColor { get; set; }
        public string ActionButtonText { get; set; }
        public ResultSetting ResultSettings { get; set; }
        public int? MinScore { get; set; }
        public int? MaxScore { get; set; }
        public bool ShowLeadUserForm { get; set; }
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
        public List<TemplateAttachment> TemplateAttachmentList { get; set; }
        public List<string> MsgVariables { get; set; }

        public class ResultSetting
        {
            public int QuizId { get; set; }
            public bool? ShowScoreValue { get; set; }
            public bool? ShowCorrectAnswer { get; set; }
            public string CustomTxtForScoreValueInResult { get; set; }
            public string CustomTxtForAnswerKey { get; set; }
            public string CustomTxtForYourAnswer { get; set; }
            public string CustomTxtForCorrectAnswer { get; set; }
            public string CustomTxtForExplanation { get; set; }
            public bool ShowLeadUserForm { get; set; }
            public bool AutoPlay { get; set; }

        }

        public IResponse MapEntityToResponse(Base EntityObj)
        {
            QuizResultResponse response = new QuizResultResponse();

            var obj = (QuizResult)EntityObj;

            response.ResultId = obj.ResultId;
            response.ShowResultImage = obj.ShowResultImage;
            response.Title = obj.Title ?? string.Empty;
            response.InternalTitle = obj.InternalTitle ?? string.Empty;
            response.Image = obj.Image ?? string.Empty;
            response.EnableMediaFile = obj.EnableMediaFile;
            response.PublicIdForResult = obj.PublicIdForResult ?? string.Empty;
            response.Description = obj.Description ?? string.Empty;
            response.HideCallToAction = obj.HideCallToAction ?? false;
            response.EnableCallToActionButton = obj.EnableCallToActionButton;
            response.ActionButtonURL = obj.ActionButtonURL ?? string.Empty;
            response.OpenLinkInNewTab = obj.OpenLinkInNewTab ?? true;
            response.ActionButtonTxtSize = obj.ActionButtonTxtSize ?? string.Empty;
            response.ActionButtonColor = obj.ActionButtonColor ?? string.Empty;
            response.ActionButtonTitleColor = obj.ActionButtonTitleColor ?? string.Empty;
            response.ActionButtonText = obj.ActionButtonText ?? string.Empty;
            response.MinScore = obj.MinScore ?? default(int);
            response.MaxScore = obj.MaxScore ?? default(int);
            response.ShowLeadUserForm = obj.ShowLeadUserForm;
            response.AutoPlay = obj.AutoPlay;
            response.SecondsToApply = obj.SecondsToApply ?? "0";
            response.VideoFrameEnabled = obj.VideoFrameEnabled ?? false;
            response.ShowExternalTitle = obj.ShowExternalTitle;
            response.ShowInternalTitle = obj.ShowInternalTitle;
            response.ShowDescription = obj.ShowDescription;
            response.DisplayOrderForTitle = obj.DisplayOrderForTitle;
            response.DisplayOrderForTitleImage = obj.DisplayOrderForTitleImage;
            response.DisplayOrderForDescription = obj.DisplayOrderForDescription;
            response.DisplayOrderForNextButton = obj.DisplayOrderForNextButton;
            response.MsgVariables = obj.MsgVariables;

            response.ResultSettings = new ResultSetting();

            if (obj.ResultSetting != null)
            {
                response.ResultSettings.QuizId = obj.ResultSetting.QuizId;
                response.ResultSettings.ShowScoreValue = obj.ResultSetting.ShowScoreValue ?? true;
                response.ResultSettings.ShowCorrectAnswer = obj.ResultSetting.ShowCorrectAnswer ?? true;
                response.ResultSettings.CustomTxtForScoreValueInResult = obj.ResultSetting.CustomTxtForScoreValueInResult ?? string.Empty;
                response.ResultSettings.CustomTxtForAnswerKey = obj.ResultSetting.CustomTxtForAnswerKey ?? string.Empty;
                response.ResultSettings.CustomTxtForYourAnswer = obj.ResultSetting.CustomTxtForYourAnswer ?? string.Empty;
                response.ResultSettings.CustomTxtForCorrectAnswer = obj.ResultSetting.CustomTxtForCorrectAnswer ?? string.Empty;
                response.ResultSettings.CustomTxtForExplanation = obj.ResultSetting.CustomTxtForExplanation ?? string.Empty;
                response.ResultSettings.ShowLeadUserForm = obj.ResultSetting.ShowLeadUserForm;
                response.ResultSettings.AutoPlay = obj.ResultSetting.AutoPlay;
            }

            return response;
        }
    }

    public class QuizDetailResponse : IResponse
    {
        public int QuizId { get; set; }
        public string PublishedCode { get; set; }
        public bool IsPublished { get; set; }
        public bool IsBranchingLogicEnabled { get; set; }
        public string QuizTitle { get; set; }
        public string QuizCoverTitle { get; set; }
        public bool ShowQuizCoverTitle { get; set; }
        public string QuizCoverImage { get; set; }
        public bool ShowQuizCoverImage { get; set; }
        public bool EnableMediaFile { get; set; }
        public string PublicIdForQuizCover { get; set; }
        public int QuizCoverImgXCoordinate { get; set; }
        public int QuizCoverImgYCoordinate { get; set; }
        public int QuizCoverImgHeight { get; set; }
        public int QuizCoverImgWidth { get; set; }
        public string QuizCoverImgAttributionLabel { get; set; }
        public string QuizCoverImgAltTag { get; set; }
        public string QuizDescription { get; set; }
        public bool ShowDescription { get; set; }
        public string QuizStartButtonText { get; set; }
        public int QuizTypeId { get; set; }
        public int MultipleResultEnabled { get; set; }
        public string OfficeId { get; set; }
        public bool ViewPreviousQuestion { get; set; }
        public bool EditAnswer { get; set; }
        public bool ApplyToAll { get; set; }
        public bool AutoPlay { get; set; }
        public string SecondsToApply { get; set; }
        public bool? VideoFrameEnabled { get; set; }
        public bool EnableNextButton { get; set; }
        public bool IsQuesAndContentInSameTable { get; set; }
        public int DisplayOrderForTitle { get; set; }
        public int DisplayOrderForTitleImage { get; set; }
        public int DisplayOrderForDescription { get; set; }
        public int DisplayOrderForNextButton { get; set; }
        public List<int> usageType { get; set; }
        public QuizBrandingAndStyleResponse QuizBrandingAndStyle { get; set; }
        public List<QuestionListInQuiz> QuestionsInQuiz { get; set; }
        public List<QuizResultsResponse> Results { get; set; }
        public List<QuizAction> Action { get; set; }
        public List<QuizContent> Content { get; set; }
        public List<QuizBadge> Badge { get; set; }
        public List<QuizQuestionAndContent> QuestionAndContent { get; set; }
        public bool? IsWhatsAppChatBotOldVersion { get; set; }
        public bool? EverPublished { get; set; }

        public class QuestionListInQuiz
        {
            public int QuestionId { get; set; }
            public string QuestionTitle { get; set; }
            public int DisplayOrder { get; set; }
            public bool ShowAnswerImage { get; set; }
            public int AnswerType { get; set; }
            public int? MaxAnswer { get; set; }
            public int? MinAnswer { get; set; }
            public bool TimerRequired { get; set; }
            public string Time { get; set; }
            public bool AutoPlay { get; set; }
            public string SecondsToApply { get; set; }
            public bool? VideoFrameEnabled { get; set; }
            public int Type { get; set; }
            //public int LanguageId { get; set; }
            public string LanguageCode { get; set; }
            public int TemplateId { get; set; }
            public bool IsMultiRating { get; set; }
            public List<AnswerListInQuestion> Answers { get; set; }
        }
        public class AnswerListInQuestion
        {
            public int AnswerId { get; set; }
            public string AnswerText { get; set; }
            public int DisplayOrder { get; set; }
            public int? AssociatedScore { get; set; }
            public bool IsReadOnly { get; set; }
            public int? RefId { get; set; }
        }
        public class QuizResultsResponse
        {
            public int ResultId { get; set; }
            public string ResultTitle { get; set; }
            public string InternalTitle { get; set; }
            public int? MinScore { get; set; }
            public int? MaxScore { get; set; }
            public bool AutoPlay { get; set; }
            public string SecondsToApply { get; set; }
            public bool? VideoFrameEnabled { get; set; }
        }

        public class QuizAction
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public int? AppointmentId { get; set; }
            public string ReportEmails { get; set; }
            public int? AutomationId { get; set; }
            public int ActionType { get; set; }
            public List<int> CalendarIds { get; set; }
        }
        public class QuizBadge
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Image { get; set; }
            public bool EnableMediaFile { get; set; }
            public string PublicIdForBadge { get; set; }
        }
        public class QuizQuestionAndContent
        {
            //question
            public int QuestionId { get; set; }
            public string QuestionTitle { get; set; }
            public int DisplayOrder { get; set; }
            public bool ShowAnswerImage { get; set; }
            public int AnswerType { get; set; }
            public int? MaxAnswer { get; set; }
            public int? MinAnswer { get; set; }
            public bool TimerRequired { get; set; }
            public string Time { get; set; }
            public bool AutoPlay { get; set; }
            public string SecondsToApply { get; set; }
            public bool? VideoFrameEnabled { get; set; }
            public bool IsMultiRating { get; set; }
            public List<AnswerListInQuestion> Answers { get; set; }
            //content
            public int Id { get; set; }
            public string ContentTitle { get; set; }
            public string ContentTitleImage { get; set; }
            public bool EnableMediaFileForTitle { get; set; }
            public string PublicIdForContentTitle { get; set; }
            public bool? ShowContentTitleImage { get; set; }
            public string ContentDescription { get; set; }
            public string ContentDescriptionImage { get; set; }
            public bool EnableMediaFileForDescription { get; set; }
            public string PublicIdForContentDescription { get; set; }
            public bool? ShowContentDescriptionImage { get; set; }
            public string AliasTextForNextButton { get; set; }
            public bool AutoPlayForDescription { get; set; }
            public string SecondsToApplyForDescription { get; set; }
            public bool? DescVideoFrameEnabled { get; set; }
            public int AnswerStructureType { get; set; }
            public int Type { get; set; }
        }
        public IResponse MapEntityToResponse(Base EntityObj)
        {
            QuizDetailResponse response = new QuizDetailResponse();

            var obj = (QuizDetailsModel)EntityObj;

            response.QuizId = obj.QuizId;
            response.QuizTypeId = obj.QuizTypeId;
            response.PublishedCode = obj.PublishedCode;
            response.IsPublished = obj.IsPublished;
            response.MultipleResultEnabled = obj.MultipleResultEnabled;
            response.IsBranchingLogicEnabled = obj.IsBranchingLogicEnabled ?? false;
            response.QuizTitle = obj.QuizCoverDetails.QuizTitle ?? string.Empty;
            response.QuizCoverTitle = obj.QuizCoverDetails.QuizCoverTitle ?? string.Empty;
            response.ShowQuizCoverTitle = obj.QuizCoverDetails.ShowQuizCoverTitle;
            response.QuizCoverImage = obj.QuizCoverDetails.QuizCoverImage ?? string.Empty;
            response.ShowQuizCoverImage = obj.QuizCoverDetails.ShowQuizCoverImage;
            response.EnableMediaFile = obj.QuizCoverDetails.EnableMediaFile;
            response.PublicIdForQuizCover = obj.QuizCoverDetails.PublicIdForQuizCover ?? string.Empty;
            response.QuizCoverImgXCoordinate = obj.QuizCoverDetails.QuizCoverImgXCoordinate ?? 0;
            response.QuizCoverImgYCoordinate = obj.QuizCoverDetails.QuizCoverImgYCoordinate ?? 0;
            response.QuizCoverImgHeight = obj.QuizCoverDetails.QuizCoverImgHeight ?? 0;
            response.QuizCoverImgWidth = obj.QuizCoverDetails.QuizCoverImgWidth ?? 0;
            response.QuizCoverImgAttributionLabel = obj.QuizCoverDetails.QuizCoverImgAttributionLabel ?? string.Empty;
            response.QuizCoverImgAltTag = obj.QuizCoverDetails.QuizCoverImgAltTag ?? string.Empty;
            response.QuizDescription = obj.QuizCoverDetails.QuizDescription ?? string.Empty;
            response.ShowDescription = obj.QuizCoverDetails.ShowDescription;
            response.QuizStartButtonText = obj.QuizCoverDetails.QuizStartButtonText ?? string.Empty;
            response.OfficeId = obj.OfficeId;
            response.ViewPreviousQuestion = obj.ViewPreviousQuestion;
            response.EditAnswer = obj.EditAnswer;
            response.ApplyToAll = obj.ApplyToAll;
            response.AutoPlay = obj.QuizCoverDetails.AutoPlay;
            response.SecondsToApply = obj.SecondsToApply ?? "0";
            response.VideoFrameEnabled = obj.VideoFrameEnabled ?? false;
            response.EnableNextButton = obj.QuizCoverDetails.EnableNextButton;
            response.DisplayOrderForTitleImage = obj.QuizCoverDetails.DisplayOrderForTitleImage;
            response.DisplayOrderForTitle = obj.QuizCoverDetails.DisplayOrderForTitle;
            response.DisplayOrderForDescription = obj.QuizCoverDetails.DisplayOrderForDescription;
            response.DisplayOrderForNextButton = obj.QuizCoverDetails.DisplayOrderForNextButton;
            response.IsQuesAndContentInSameTable = obj.IsQuesAndContentInSameTable;
            response.usageType = obj.UsageType;
            response.IsWhatsAppChatBotOldVersion = obj.IsWhatsAppChatBotOldVersion;
            response.EverPublished = obj.EverPublished;

            if (obj.QuizBrandingAndStyle != null)
            {
                response.QuizBrandingAndStyle = new QuizBrandingAndStyleResponse();
                response.QuizBrandingAndStyle.QuizId = obj.QuizBrandingAndStyle.QuizId;
                response.QuizBrandingAndStyle.ImageFileURL = obj.QuizBrandingAndStyle.ImageFileURL ?? string.Empty;
                response.QuizBrandingAndStyle.PublicIdForImageFile = obj.QuizBrandingAndStyle.PublicIdForFileURL ?? string.Empty;
                response.QuizBrandingAndStyle.BackgroundColor = obj.QuizBrandingAndStyle.BackgroundColor ?? string.Empty;
                response.QuizBrandingAndStyle.ButtonColor = obj.QuizBrandingAndStyle.ButtonColor ?? string.Empty;
                response.QuizBrandingAndStyle.OptionColor = obj.QuizBrandingAndStyle.OptionColor ?? string.Empty;
                response.QuizBrandingAndStyle.ButtonFontColor = obj.QuizBrandingAndStyle.ButtonFontColor ?? string.Empty;
                response.QuizBrandingAndStyle.OptionFontColor = obj.QuizBrandingAndStyle.OptionFontColor ?? string.Empty;
                response.QuizBrandingAndStyle.FontColor = obj.QuizBrandingAndStyle.FontColor ?? string.Empty;
                response.QuizBrandingAndStyle.FontType = obj.QuizBrandingAndStyle.FontType ?? string.Empty;
                response.QuizBrandingAndStyle.BackgroundColorofSelectedAnswer = obj.QuizBrandingAndStyle.BackgroundColorofSelectedAnswer ?? string.Empty;
                response.QuizBrandingAndStyle.BackgroundColorofAnsweronHover = obj.QuizBrandingAndStyle.BackgroundColorofAnsweronHover ?? string.Empty;
                response.QuizBrandingAndStyle.AnswerTextColorofSelectedAnswer = obj.QuizBrandingAndStyle.AnswerTextColorofSelectedAnswer ?? string.Empty;
                response.QuizBrandingAndStyle.IsBackType = obj.QuizBrandingAndStyle.IsBackType;
                response.QuizBrandingAndStyle.BackImageFileURL = obj.QuizBrandingAndStyle.BackImageFileURL ?? string.Empty;
                response.QuizBrandingAndStyle.BackColor = obj.QuizBrandingAndStyle.BackColor ?? string.Empty;
                response.QuizBrandingAndStyle.Opacity = obj.QuizBrandingAndStyle.Opacity ?? string.Empty;
                response.QuizBrandingAndStyle.LogoUrl = obj.QuizBrandingAndStyle.LogoUrl ?? string.Empty;
                response.QuizBrandingAndStyle.LogoPublicId = obj.QuizBrandingAndStyle.LogoPublicId ?? string.Empty;
                response.QuizBrandingAndStyle.BackgroundColorofLogo = obj.QuizBrandingAndStyle.BackgroundColorofLogo ?? string.Empty;
                response.QuizBrandingAndStyle.AutomationAlignment = obj.QuizBrandingAndStyle.AutomationAlignment ?? string.Empty;
                response.QuizBrandingAndStyle.LogoAlignment = obj.QuizBrandingAndStyle.LogoAlignment ?? string.Empty;
                response.QuizBrandingAndStyle.Flip = obj.QuizBrandingAndStyle.Flip;
                response.QuizBrandingAndStyle.Language = obj.QuizBrandingAndStyle.Language;
            }
            response.QuestionsInQuiz = new List<QuestionListInQuiz>();

            foreach (var question in obj.QuestionsInQuiz)
            {
                var ques = new QuestionListInQuiz();

                ques.QuestionId = question.QuestionId;
                ques.QuestionTitle = question.QuestionTitle;
                ques.DisplayOrder = question.DisplayOrder;
                ques.ShowAnswerImage = question.ShowAnswerImage ?? false;
                ques.AnswerType = question.AnswerType;
                ques.MinAnswer = question.MinAnswer;
                ques.MaxAnswer = question.MaxAnswer;
                ques.TimerRequired = question.TimerRequired;
                ques.Time = question.Time;
                ques.AutoPlay = question.AutoPlay;
                ques.SecondsToApply = question.SecondsToApply;
                ques.VideoFrameEnabled = question.VideoFrameEnabled;
                ques.Type = question.Type;
                //ques.LanguageId = question.LanguageId ?? 0;
                ques.LanguageCode = question.LanguageCode;
                ques.TemplateId = question.TemplateId ?? 0;
                ques.IsMultiRating = question.IsMultiRating;
                ques.Answers = new List<AnswerListInQuestion>();

                foreach (var answer in question.AnswerList)
                {
                    var ans = new AnswerListInQuestion();

                    ans.AnswerId = answer.AnswerId;
                    ans.AnswerText = answer.AnswerText;
                    ans.DisplayOrder = answer.DisplayOrder;
                    ans.AssociatedScore = answer.AssociatedScore;
                    ans.IsReadOnly = answer.IsReadOnly;
                    ans.RefId = answer.RefId;
                    ques.Answers.Add(ans);
                }

                response.QuestionsInQuiz.Add(ques);
            }

            response.Results = new List<QuizResultsResponse>();

            if (obj.ResultList != null && obj.ResultList.Count > 0)
            {
                foreach (var item in obj.ResultList)
                {
                    response.Results.Add(new QuizResultsResponse
                    {
                        ResultId = item.ResultId,
                        ResultTitle = item.Title,
                        InternalTitle = item.InternalTitle,
                        MinScore = item.MinScore,
                        MaxScore = item.MaxScore,
                        AutoPlay = item.AutoPlay,
                        SecondsToApply = item.SecondsToApply,
                        VideoFrameEnabled = item.VideoFrameEnabled
                    });
                }
            }

            response.Action = new List<QuizAction>();
            if (obj.QuizAction != null && obj.QuizAction.Count > 0)
            {
                foreach (var item in obj.QuizAction)
                {
                    response.Action.Add(new QuizAction
                    {
                        AppointmentId = item.AppointmentId,
                        Id = item.Id,
                        ReportEmails = item.ReportEmails,
                        AutomationId = item.AutomationId,
                        Title = item.Title,
                        ActionType = item.ActionType,
                        CalendarIds = item.CalendarIds
                    });
                }
            }
            response.Content = new List<QuizContent>();
            if (obj.QuizContent != null && obj.QuizContent.Count > 0)
            {
                foreach (var item in obj.QuizContent)
                {
                    response.Content.Add(new QuizContent
                    {
                        Id = item.Id,
                        ContentTitle = item.ContentTitle,
                        ContentTitleImage = item.ContentTitleImage,
                        EnableMediaFileForTitle = item.EnableMediaFileForTitle,
                        PublicIdForContentTitle = item.PublicIdForContentTitle,
                        ShowContentTitleImage = item.ShowContentTitleImage,
                        ContentDescription = item.ContentDescription,
                        ContentDescriptionImage = item.ContentDescriptionImage,
                        EnableMediaFileForDescription = item.EnableMediaFileForDescription,
                        PublicIdForContentDescription = item.PublicIdForContentDescription,
                        ShowContentDescriptionImage = item.ShowContentDescriptionImage,
                        AliasTextForNextButton = item.AliasTextForNextButton,
                        AutoPlay = item.AutoPlay,
                        SecondsToApply = item.SecondsToApply,
                        VideoFrameEnabled = item.VideoFrameEnabled,
                        DisplayOrder = item.DisplayOrder
                    });
                }
            }
            response.Badge = new List<QuizBadge>();
            if (obj.BadgeList != null && obj.BadgeList.Count > 0)
            {
                foreach (var item in obj.BadgeList)
                {
                    response.Badge.Add(new QuizBadge
                    {
                        Id = item.Id,
                        Title = item.Title,
                        Image = item.Image,
                        EnableMediaFile = item.EnableMediaFile,
                        PublicIdForBadge = item.PublicIdForBadge
                    });
                }
            }

            response.QuestionAndContent = new List<QuizQuestionAndContent>();

            foreach (var question in obj.QuestionsAndContent.OrderBy(r => r.DisplayOrder))
            {
                var ques = new QuizQuestionAndContent();

                ques.QuestionId = question.QuestionId;
                ques.QuestionTitle = question.QuestionTitle;
                ques.DisplayOrder = question.DisplayOrder;
                ques.ShowAnswerImage = question.ShowAnswerImage ?? false;
                ques.AnswerType = question.AnswerType;
                ques.MinAnswer = question.MinAnswer;
                ques.MaxAnswer = question.MaxAnswer;
                ques.TimerRequired = question.TimerRequired;
                ques.Time = question.Time;
                ques.AutoPlay = question.AutoPlay;
                ques.SecondsToApply = question.SecondsToApply;
                ques.VideoFrameEnabled = question.VideoFrameEnabled;
                ques.IsMultiRating = question.IsMultiRating;
                ques.Answers = new List<AnswerListInQuestion>();

                if (question.AnswerList != null)
                {
                    foreach (var answer in question.AnswerList)
                    {
                        var ans = new AnswerListInQuestion();

                        ans.AnswerId = answer.AnswerId;
                        ans.AnswerText = answer.AnswerText;
                        ans.DisplayOrder = answer.DisplayOrder;
                        ans.AssociatedScore = answer.AssociatedScore;
                        ans.IsReadOnly = answer.IsReadOnly;
                        ques.Answers.Add(ans);
                    }
                }

                //content 
                ques.Id = question.Id;
                ques.ContentTitle = question.ContentTitle;
                ques.ContentTitleImage = question.ContentTitleImage;
                ques.EnableMediaFileForTitle = question.EnableMediaFileForTitle;
                ques.PublicIdForContentTitle = question.PublicIdForContentTitle;
                ques.ShowContentTitleImage = question.ShowContentTitleImage;
                ques.ContentDescription = question.ContentDescription;
                ques.ContentDescriptionImage = question.ContentDescriptionImage;
                ques.EnableMediaFileForDescription = question.EnableMediaFileForDescription;
                ques.PublicIdForContentDescription = question.PublicIdForContentDescription;
                ques.ShowContentDescriptionImage = question.ShowContentDescriptionImage;
                ques.AliasTextForNextButton = question.AliasTextForNextButton;
                ques.AutoPlayForDescription = question.AutoPlayForDescription;
                ques.SecondsToApplyForDescription = question.SecondsToApplyForDescription;
                ques.DescVideoFrameEnabled = question.DescVideoFrameEnabled;
                ques.IsMultiRating = question.IsMultiRating;
                ques.AnswerStructureType = question.AnswerStructureType.HasValue ? question.AnswerStructureType.Value : (int)AnswerStructureTypeEnum.Default;
                ques.Type = question.Type;
                response.QuestionAndContent.Add(ques);
            }
            return response;
        }
    }

    public class QuizBranchingLogicResponse : IResponse
    {
        public int QuizId { get; set; }
        public int QuizType { get; set; }
        public string QuizTitle { get; set; }
        public string QuizCoverTitle { get; set; }
        public string QuizDescription { get; set; }
        public bool ShowDescription { get; set; }
        public int StartType { get; set; }
        public int StartTypeId { get; set; }
        public string SecondsToApply { get; set; }
        public bool? VideoFrameEnabled { get; set; }
        public List<int> usageType { get; set; }
        public List<ResultBranchingLogic> ResultList { get; set; }
        public List<ActionBranchingLogic> ActionList { get; set; }
        public bool IsQuesAndContentInSameTable { get; set; }
        public List<BadgeBranchingLogic> BadgeList { get; set; }
        public List<QuestionAndContentBranchingLogic> QuestionAndContentList { get; set; }
        public bool? IsWhatsAppChatBotOldVersion { get; set; }


        public class BranchingLogicResponse
        {
            public int QuizId { get; set; }
            public int QuizType { get; set; }
            public string QuestionId { get; set; }
            public string QuestionTxt { get; set; }
            public string QuestionImage { get; set; }
            public string PublicIdForQuestion { get; set; }
            public bool? ShowQuestionImage { get; set; }
            public bool? ShowAnswerImage { get; set; }
            public bool IsDisabled { get; set; }
            public int AnswerType { get; set; }
            public int? MaxAnswer { get; set; }
            public int? MinAnswer { get; set; }
            public List<AnswerInQuestions> AnswerList { get; set; }
        }
        public class ActionBranchingLogic : Base
        {
            public int QuizId { get; set; }
            public int ActionId { get; set; }
            public string ActionTitle { get; set; }
            public int? AppointmentId { get; set; }
            public string ReportEmails { get; set; }
            public int ActionType { get; set; }
            public int? XCordinate { get; set; }
            public int? YCordinate { get; set; }
            public bool IsDisabled { get; set; }
            public string SecondsToApply { get; set; }
            public bool? VideoFrameEnabled { get; set; }
            public string SecondsToApplyForDescription { get; set; }
            public bool? DescVideoFrameEnabled { get; set; }
        }
        public class BadgeBranchingLogic
        {
            public int QuizId { get; set; }
            public int BadgetId { get; set; }
            public string BadgetTitle { get; set; }
            public string BadgetImage { get; set; }
            public string PublicIdForBadget { get; set; }
            public int LinkedToType { get; set; }
            public int? LinkedResultId { get; set; }
            public int? LinkedQuestionId { get; set; }
            public int? LinkedContentId { get; set; }
            public bool IsDisabled { get; set; }
            public int? XCordinate { get; set; }
            public int? YCordinate { get; set; }
        }
        public class ResultBranchingLogic
        {
            public int QuizId { get; set; }
            public int ResultId { get; set; }
            public string ResultTitle { get; set; }
            public string ResultInternalTitle { get; set; }
            public string ResultDescription { get; set; }
            public string ResultImage { get; set; }
            public string SecondsToApply { get; set; }
            public bool? VideoFrameEnabled { get; set; }
            public string PublicIdForResult { get; set; }
            public bool ShowResultImage { get; set; }
            public int LinkedToType { get; set; }
            public int? LinkedActionId { get; set; }
            public int? LinkedContentId { get; set; }
            public bool IsDisabled { get; set; }
            public int? XCordinate { get; set; }
            public int? YCordinate { get; set; }
            public string ActionButtonText { get; set; }
            public bool HideActionButton { get; set; }
            public int? MinScore { get; set; }
            public int? MaxScore { get; set; }
            public bool IsPersonalityCorrelatedResult { get; set; }
            public bool ShowDescription { get; set; }
            public bool ShowExternalTitle { get; set; }
            public bool ShowInternalTitle { get; set; }
        }
        public class ContentBranchingLogic
        {
            public int QuizId { get; set; }
            public int ContentId { get; set; }
            public string ContentTitle { get; set; }
            public string ContentTitleImage { get; set; }
            public string PublicIdForContentTitle { get; set; }
            public bool? ShowContentTitleImage { get; set; }
            public string ContentDescription { get; set; }
            public string ContentDescriptionImage { get; set; }
            public string PublicIdForContentDescription { get; set; }
            public bool? ShowContentDescriptionImage { get; set; }
            public bool IsDisabled { get; set; }
            public int LinkedToType { get; set; }
            public int? LinkedQuestionId { get; set; }
            public int? LinkedResultId { get; set; }
            public int? LinkedContentId { get; set; }
            public int? XCordinate { get; set; }
            public int? YCordinate { get; set; }
            public string ActionButtonText { get; set; }
            public ContentBranchingLogic LinkedTypeObj { get; set; }
        }
        public class AnswerInQuestions
        {
            public int AnswerId { get; set; }
            public string AnswerTxt { get; set; }
            public string AnswerDescription { get; set; }
            public int? AssociatedScore { get; set; }
            public string AnswerOptionImage { get; set; }
            public string PublicIdForAnswerOption { get; set; }
            public string LinkedToType { get; set; }
            public string LinkedQuestionId { get; set; }
            public string LinkedResultId { get; set; }
            public bool? IsCorrect { get; set; }
            public int DisplayOrder { get; set; }
            public string SecondsToApply { get; set; }
            public bool? VideoFrameEnabled { get; set; }
            public int? RefId { get; set; }
        }
        public class QuestionAndContentBranchingLogic : Base
        {
            public int QuizId { get; set; }
            public bool IsDisabled { get; set; }
            public int Type { get; set; }
            // Content
            public int ContentId { get; set; }
            public string ContentTitle { get; set; }
            public string ContentTitleImage { get; set; }
            public string PublicIdForContentTitle { get; set; }
            public bool? ShowContentTitleImage { get; set; }
            public string ContentDescription { get; set; }
            public string ContentDescriptionImage { get; set; }
            public string PublicIdForContentDescription { get; set; }
            public bool? ShowContentDescriptionImage { get; set; }
            public int LinkedToType { get; set; }
            public int? LinkedQuestionId { get; set; }
            public int? LinkedResultId { get; set; }
            public int? LinkedContentId { get; set; }
            public int? XCordinate { get; set; }
            public int? YCordinate { get; set; }
            public string ActionButtonText { get; set; }
            public ContentBranchingLogic LinkedTypeObj { get; set; }
            //Question
            public int QuizType { get; set; }
            public string QuestionId { get; set; }
            public string QuestionTxt { get; set; }
            public string QuestionImage { get; set; }
            public string PublicIdForQuestion { get; set; }
            public bool? ShowQuestionImage { get; set; }
            public bool? ShowAnswerImage { get; set; }
            public int AnswerType { get; set; }
            public int? MaxAnswer { get; set; }
            public int? MinAnswer { get; set; }
            public string SecondsToApply { get; set; }
            public bool? VideoFrameEnabled { get; set; }
            public string SecondsToApplyForDescription { get; set; }
            public bool? DescVideoFrameEnabled { get; set; }
            //public int? LanguageId { get; set; }
            public string LanguageCode { get; set; }
            public int? TemplateId { get; set; }
            public bool ShowTitle { get; set; }
            public bool ShowDescription { get; set; }
            public bool IsMultiRating { get; set; }
            public List<AnswerInQuestions> AnswerList { get; set; }
        }

        public IResponse MapEntityToResponse(Base EntityObj)
        {
            QuizBranchingLogicResponse response = new QuizBranchingLogicResponse();

            var obj = (QuizBranchingLogic)EntityObj;

            response.QuizId = obj.QuizId;
            response.QuizType = obj.QuizType;
            response.QuizTitle = obj.QuizTitle;
            response.QuizCoverTitle = obj.QuizCoverTitle;
            response.QuizDescription = obj.QuizDescription;
            response.ShowDescription = obj.ShowDescription;
            response.SecondsToApply = obj.SecondsToApply;
            response.VideoFrameEnabled = obj.VideoFrameEnabled;
            response.StartType = (int)obj.StartType;
            response.StartTypeId = obj.StartTypeId;
            response.usageType = obj.UsageTypes;
            response.IsWhatsAppChatBotOldVersion = obj.IsWhatsAppChatBotOldVersion;

            response.ResultList = new List<ResultBranchingLogic>();
            if (obj.ResultList != null)
            {
                foreach (var result in obj.ResultList)
                {
                    var resultObj = new ResultBranchingLogic
                    {
                        QuizId = result.QuizId,
                        ResultId = result.ResultId,
                        ResultTitle = result.ResultTitle,
                        ResultInternalTitle = result.ResultInternalTitle,
                        ResultDescription = result.ResultDescription,
                        ResultImage = result.ResultImage,
                        PublicIdForResult = result.PublicIdForResult,
                        ShowResultImage = result.ShowResultImage,
                        LinkedToType = result.LinkedToType,
                        LinkedActionId = result.LinkedActionId,
                        LinkedContentId = result.LinkedContentId,
                        XCordinate = result.XCordinate,
                        YCordinate = result.YCordinate,
                        IsDisabled = result.IsDisabled,
                        ActionButtonText = result.ActionButtonText,
                        HideActionButton = result.HideActionButton,
                        MinScore = result.MinScore,
                        MaxScore = result.MaxScore,
                        SecondsToApply = result.SecondsToApply,
                        VideoFrameEnabled = result.VideoFrameEnabled,
                        IsPersonalityCorrelatedResult = result.IsPersonalityCorrelatedResult,
                        ShowDescription = result.ShowDescription,
                        ShowExternalTitle = result.ShowExternalTitle,
                        ShowInternalTitle = result.ShowInternalTitle
                    };
                    response.ResultList.Add(resultObj);
                }
            }
            response.ActionList = new List<ActionBranchingLogic>();
            if (obj.ActionList != null)
            {
                foreach (var action in obj.ActionList)
                {
                    var resultObj = new ActionBranchingLogic
                    {
                        QuizId = action.QuizId,
                        ActionId = action.ActionId,
                        ActionTitle = action.ActionTitle,
                        ActionType = action.ActionType,
                        AppointmentId = action.AppointmentId,
                        ReportEmails = action.ReportEmails,
                        XCordinate = action.XCordinate,
                        YCordinate = action.YCordinate,
                        IsDisabled = action.IsDisabled,
                        SecondsToApply = action.SecondsToApply,
                        VideoFrameEnabled = action.VideoFrameEnabled,
                        SecondsToApplyForDescription = action.SecondsToApplyForDescription,
                        DescVideoFrameEnabled = action.DescVideoFrameEnabled

                    };
                    response.ActionList.Add(resultObj);
                }
            }
            response.ActionList = new List<ActionBranchingLogic>();
            if (obj.ActionList != null)
            {
                foreach (var action in obj.ActionList)
                {
                    var resultObj = new ActionBranchingLogic
                    {
                        QuizId = action.QuizId,
                        ActionId = action.ActionId,
                        ActionTitle = action.ActionTitle,
                        ActionType = action.ActionType,
                        AppointmentId = action.AppointmentId,
                        ReportEmails = action.ReportEmails,
                        XCordinate = action.XCordinate,
                        YCordinate = action.YCordinate,
                        IsDisabled = action.IsDisabled,
                        SecondsToApply = action.SecondsToApply,
                        VideoFrameEnabled = action.VideoFrameEnabled,
                        SecondsToApplyForDescription = action.SecondsToApplyForDescription,
                        DescVideoFrameEnabled = action.DescVideoFrameEnabled
                    };
                    response.ActionList.Add(resultObj);
                }
            }
            response.IsQuesAndContentInSameTable = obj.IsQuesAndContentInSameTable;
            response.BadgeList = new List<BadgeBranchingLogic>();
            if (obj.ActionList != null)
            {
                foreach (var badge in obj.BadgeList)
                {
                    var badgeObj = new BadgeBranchingLogic
                    {
                        QuizId = badge.QuizId,
                        BadgetId = badge.BadgetId,
                        BadgetImage = badge.BadgetImage,
                        PublicIdForBadget = badge.PublicIdForBadget,
                        BadgetTitle = badge.BadgetTitle,
                        LinkedQuestionId = badge.LinkedQuestionId,
                        LinkedToType = badge.LinkedToType,
                        LinkedResultId = badge.LinkedResultId,
                        LinkedContentId = badge.LinkedContentId,
                        XCordinate = badge.XCordinate,
                        YCordinate = badge.YCordinate,
                        IsDisabled = badge.IsDisabled
                    };
                    response.BadgeList.Add(badgeObj);
                }
            }

            response.QuestionAndContentList = new List<QuestionAndContentBranchingLogic>();
            if (obj.QuestionAndContentList != null)
            {
                foreach (var questionAndContent in obj.QuestionAndContentList.OrderBy(r => r.DisplayOrder))
                {
                    var questionAndContentObj = new QuestionAndContentBranchingLogic();

                    questionAndContentObj.QuizId = questionAndContent.QuizId;
                    questionAndContentObj.IsDisabled = questionAndContent.IsDisabled;
                    questionAndContentObj.Type = questionAndContent.Type;
                    //Content 
                    questionAndContentObj.ContentId = questionAndContent.ContentId;
                    questionAndContentObj.ContentTitle = questionAndContent.ContentTitle;
                    questionAndContentObj.ContentTitleImage = questionAndContent.ContentTitleImage;
                    questionAndContentObj.PublicIdForContentTitle = questionAndContent.PublicIdForContentTitle;
                    questionAndContentObj.ShowContentTitleImage = questionAndContent.ShowContentTitleImage;
                    questionAndContentObj.ContentDescription = questionAndContent.ContentDescription;
                    questionAndContentObj.ContentDescriptionImage = questionAndContent.ContentDescriptionImage;
                    questionAndContentObj.PublicIdForContentDescription = questionAndContent.PublicIdForContentDescription;
                    questionAndContentObj.ShowContentDescriptionImage = questionAndContent.ShowContentDescriptionImage;
                    questionAndContentObj.LinkedToType = questionAndContent.LinkedToType;
                    questionAndContentObj.LinkedQuestionId = questionAndContent.LinkedQuestionId;
                    questionAndContentObj.LinkedResultId = questionAndContent.LinkedResultId;
                    questionAndContentObj.LinkedContentId = questionAndContent.LinkedContentId;
                    questionAndContentObj.XCordinate = questionAndContent.XCordinate;
                    questionAndContentObj.YCordinate = questionAndContent.YCordinate;
                    questionAndContentObj.ActionButtonText = questionAndContent.ActionButtonText;
                    //Question
                    questionAndContentObj.QuizType = questionAndContent.QuizType;
                    questionAndContentObj.QuestionId = questionAndContent.QuestionId.ToString();
                    questionAndContentObj.QuestionTxt = questionAndContent.QuestionTxt;
                    questionAndContentObj.QuestionImage = questionAndContent.QuestionImage;
                    questionAndContentObj.PublicIdForQuestion = questionAndContent.PublicIdForQuestion;
                    questionAndContentObj.ShowQuestionImage = questionAndContent.ShowQuestionImage;
                    questionAndContentObj.ShowAnswerImage = questionAndContent.ShowAnswerImage;
                    questionAndContentObj.AnswerType = questionAndContent.AnswerType;
                    questionAndContentObj.MinAnswer = questionAndContent.MinAnswer;
                    questionAndContentObj.MaxAnswer = questionAndContent.MaxAnswer;
                    questionAndContentObj.SecondsToApply = questionAndContent.SecondsToApply;
                    questionAndContentObj.VideoFrameEnabled = questionAndContent.VideoFrameEnabled;
                    questionAndContentObj.SecondsToApplyForDescription = questionAndContent.SecondsToApplyForDescription;
                    questionAndContentObj.DescVideoFrameEnabled = questionAndContent.DescVideoFrameEnabled;
                    //questionAndContentObj.LanguageId = questionAndContent.LanguageId;
                    questionAndContentObj.LanguageCode = questionAndContent.LanguageCode;
                    questionAndContentObj.TemplateId = questionAndContent.TemplateId;
                    questionAndContentObj.ShowTitle = questionAndContent.ShowTitle;
                    questionAndContentObj.ShowDescription = questionAndContent.ShowDescription;
                    questionAndContentObj.IsMultiRating = questionAndContent.IsMultiRating;
                    questionAndContentObj.AnswerList = new List<AnswerInQuestions>();

                    if (questionAndContent.AnswerList != null)
                    {
                        foreach (var item in questionAndContent.AnswerList)
                        {
                            questionAndContentObj.AnswerList.Add(new AnswerInQuestions
                            {
                                AnswerId = item.AnswerId,
                                AnswerTxt = item.AnswerTxt,
                                AnswerDescription = item.AnswerDescription,
                                AssociatedScore = item.AssociatedScore,
                                AnswerOptionImage = item.AnswerOptionImage,
                                PublicIdForAnswerOption = item.PublicIdForAnswerOption,
                                LinkedToType = questionAndContent.IsDisabled ? ((int)BranchingLogicEnum.NONE).ToString() : item.LinkedToType.ToString(),
                                LinkedQuestionId = item.QuestionId.HasValue ? item.QuestionId.Value.ToString() : string.Empty,
                                LinkedResultId = item.ResultId.HasValue ? item.ResultId.Value.ToString() : string.Empty,
                                IsCorrect = item.IsCorrect,
                                DisplayOrder = item.DisplayOrder,
                                SecondsToApply = item.SecondsToApply ?? "0",
                                VideoFrameEnabled = item.VideoFrameEnabled ?? false,
                                RefId = item.RefId

                            });
                        }
                    }

                    response.QuestionAndContentList.Add(questionAndContentObj);
                }
            }

            return response;
        }
    }

    public class QuizBranchingLogicLinksListResponse : IResponse
    {
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
            public bool IsCorrect { get; set; }
        }
        public IResponse MapEntityToResponse(Base EntityObj)
        {
            QuizBranchingLogicLinksListResponse response = new QuizBranchingLogicLinksListResponse();

            var obj = (QuizBranchingLogicLinksList)EntityObj;


            response.QuizBranchingLogic = new List<QuizBranchingLogicLinks>();

            foreach (var quizBranchingLogic in obj.QuizBranchingLogicLinks)
            {
                var quizObj = new QuizBranchingLogicLinks();

                quizObj.Position = quizBranchingLogic.Position;
                quizObj.Type = (int)quizBranchingLogic.ObjectType;
                quizObj.Id = quizBranchingLogic.ObjectTypeId;
                quizObj.Links = new List<BranchiLinks>();

                if (quizBranchingLogic.Links != null && quizBranchingLogic.Links.Count > 0)
                {
                    foreach (var item in quizBranchingLogic.Links)
                    {
                        if (!string.IsNullOrEmpty(item.ToId))
                        {
                            quizObj.Links.Add(new BranchiLinks
                            {
                                FromId = item.FromId,
                                FromType = (int)item.FromType,
                                ToId = item.ToId,
                                ToType = (int)item.ToType,
                                IsCorrect = item.IsCorrect
                            });
                        }
                        else
                        {
                            quizObj.Links.Add(new BranchiLinks
                            {
                                FromId = "",
                                FromType = (int)BranchingLogicEnum.NONE,
                                ToId = "",
                                ToType = (int)BranchingLogicEnum.NONE,
                            });
                        }
                    }
                }
                else
                {
                    quizObj.Links.Add(new BranchiLinks
                    {
                        FromId = "",
                        FromType = (int)BranchingLogicEnum.NONE,
                        ToId = "",
                        ToType = (int)BranchingLogicEnum.NONE,
                    });
                }

                response.QuizBranchingLogic.Add(quizObj);
            }
            return response;
        }
    }



    public class QuizResultRedirectResponse : IResponse
    {
        public int QuizId { get; set; }
        public int ResultId { get; set; }
        public string ResultTitle { get; set; }
        public bool IsRedirectOn { get; set; }
        public string RedirectResultTo { get; set; }

        public IResponse MapEntityToResponse(Base EntityObj)
        {
            QuizResultRedirectResponse response = new QuizResultRedirectResponse();

            var obj = (QuizResultRedirect)EntityObj;

            response.QuizId = obj.QuizId;
            response.ResultId = obj.ResultId;
            response.ResultTitle = obj.ResultTitle;
            response.IsRedirectOn = obj.IsRedirectOn.HasValue && obj.IsRedirectOn.Value ? obj.IsRedirectOn.Value : false;
            response.RedirectResultTo = !response.IsRedirectOn ? string.Empty : obj.RedirectResultTo;

            return response;
        }
    }

    public class QuizVersionResponse : IResponse
    {
        public int PublishedQuizId { get; set; }
        public int VersionNumber { get; set; }
        public DateTime PublishedOnDate { get; set; }
        public string PublishedOnTime { get; set; }
        public DateTime UntilDate { get; set; }
        public string UntilTime { get; set; }
        public bool IsCurrent { get; set; }

        public IResponse MapEntityToResponse(Base EntityObj)
        {
            QuizVersionResponse response = new QuizVersionResponse();

            var obj = (QuizVersion)EntityObj;

            response.PublishedQuizId = obj.PublishedQuizId;
            response.VersionNumber = obj.VersionNumber;
            response.PublishedOnDate = obj.PublishedOn.Date;
            response.PublishedOnTime = obj.PublishedOn.ToString("HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            response.UntilDate = obj.UntilDate.Date;
            response.UntilTime = obj.UntilDate.ToString("HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            response.IsCurrent = obj.IsCurrent;

            return response;
        }
    }

    public class QuizAnalyticsOverviewResponse : IResponse
    {
        public int PublishedQuizId { get; set; }
        public int Views { get; set; }
        public int QuizStarts { get; set; }
        public int Completion { get; set; }
        public int Leads { get; set; }

        public IResponse MapEntityToResponse(Base EntityObj)
        {
            QuizAnalyticsOverviewResponse response = new QuizAnalyticsOverviewResponse();

            var obj = (QuizAnalyticsOverview)EntityObj;

            response.PublishedQuizId = obj.PublishedQuizId;
            response.Views = obj.Views;
            response.QuizStarts = obj.QuizStarts;
            response.Completion = obj.Completion;
            response.Leads = obj.Leads;

            return response;
        }
    }

    public class QuizAnalyticsStatsResponse : IResponse
    {
        public int PublishedQuizId { get; set; }
        public int VisitorsCount { get; set; }
        public string StartButtonText { get; set; }
        public bool IsBranchingLogicEnabled { get; set; }
        public List<QuestionStats> Questions { get; set; }
        public List<ResultStats> Results { get; set; }
        public int CompletedQuizCount { get; set; }
        public int LeadsCount { get; set; }

        public class QuestionStats
        {
            public string QuestionTitle { get; set; }
            public int QuestionAttempts { get; set; }
            public List<AnswerStats> Answers { get; set; }

            public class AnswerStats
            {
                public string AnswerTitle { get; set; }
                public int AnswerAttempts { get; set; }
            }
        }
        public class ResultStats
        {
            public string ResultTitle { get; set; }
            public int LeadsInResult { get; set; }
        }

        public IResponse MapEntityToResponse(Base EntityObj)
        {
            QuizAnalyticsStatsResponse response = new QuizAnalyticsStatsResponse();

            var obj = (QuizAnalyticsStats)EntityObj;

            response.PublishedQuizId = obj.PublishedQuizId;
            response.VisitorsCount = obj.VisitorsCount;
            response.StartButtonText = obj.StartButtonText;
            response.IsBranchingLogicEnabled = obj.IsBranchingLogicEnabled;

            if (!obj.IsBranchingLogicEnabled)
            {
                if (obj.Questions != null)
                {
                    response.Questions = new List<QuestionStats>();

                    foreach (var ques in obj.Questions)
                    {
                        var questionObj = new QuestionStats();

                        questionObj.QuestionTitle = ques.QuestionTitle;
                        questionObj.QuestionAttempts = ques.QuestionAttempts;

                        questionObj.Answers = new List<QuestionStats.AnswerStats>();

                        foreach (var ans in ques.Answers)
                        {
                            questionObj.Answers.Add(new QuestionStats.AnswerStats
                            {
                                AnswerTitle = ans.AnswerTitle,
                                AnswerAttempts = ans.AnswerAttempts
                            });
                        }

                        response.Questions.Add(questionObj);
                    }
                }
            }

            if (obj.Results != null)
            {
                response.Results = new List<ResultStats>();

                foreach (var item in obj.Results)
                {
                    response.Results.Add(new ResultStats
                    {
                        ResultTitle = item.ResultTitle,
                        LeadsInResult = item.LeadsInResult
                    });
                }
            }

            response.CompletedQuizCount = obj.CompletedQuizCount;
            response.LeadsCount = obj.LeadsCount;

            return response;
        }
    }

    public class QuizLeadCollectionStatsResponse : IResponse
    {
        public string LeadUserEmail { get; set; }
        public DateTime AddedOn { get; set; }
        public string LeadUserName { get; set; }
        public string LeadUserPhone { get; set; }

        public IResponse MapEntityToResponse(Base EntityObj)
        {
            QuizLeadCollectionStatsResponse response = new QuizLeadCollectionStatsResponse();

            var obj = (QuizLeadCollectionStats)EntityObj;

            response.LeadUserEmail = obj.LeadUserEmail;
            response.AddedOn = obj.AddedOn;
            response.LeadUserName = obj.LeadUserName;
            response.LeadUserPhone = obj.LeadUserPhone;

            return response;
        }
    }

    public class QuizActionResponse : IResponse
    {
        public int Id { get; set; }
        public int? AppointmentId { get; set; }
        public string[] ReportEmails { get; set; }
        public int? AutomationId { get; set; }
        public int ActionType { get; set; }
        public string Title { get; set; }
        public List<int> CalendarIds { get; set; }

        public IResponse MapEntityToResponse(Base EntityObj)
        {
            QuizActionResponse response = new QuizActionResponse();

            var obj = (QuizAction)EntityObj;

            response.Id = obj.Id;
            response.AppointmentId = obj.AppointmentId ?? 0;
            response.ReportEmails = string.IsNullOrEmpty(obj.ReportEmails) ? new string[] { } : obj.ReportEmails.Split(',');
            response.AutomationId = obj.AutomationId ?? default(int?);
            response.ActionType = obj.ActionType;
            response.Title = obj.Title;
            response.CalendarIds = obj.CalendarIds;
            return response;
        }
    }

    public class QuizContentResponse : IResponse
    {
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
        public bool ViewPreviousQuestion { get; set; }
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

        public IResponse MapEntityToResponse(Base EntityObj)
        {
            QuizContentResponse response = new QuizContentResponse();

            var obj = (QuizContent)EntityObj;

            response.Id = obj.Id;
            response.ContentTitle = obj.ContentTitle;
            response.ShowTitle = obj.ShowTitle;
            response.ContentTitleImage = obj.ContentTitleImage;
            response.EnableMediaFileForTitle = obj.EnableMediaFileForTitle;
            response.PublicIdForContentTitle = obj.PublicIdForContentTitle;
            response.ShowContentTitleImage = obj.ShowContentTitleImage;
            response.ContentDescription = obj.ContentDescription;
            response.ShowDescription = obj.ShowDescription;
            response.ContentDescriptionImage = obj.ContentDescriptionImage;
            response.EnableMediaFileForDescription = obj.EnableMediaFileForDescription;
            response.PublicIdForContentDescription = obj.PublicIdForContentDescription;
            response.ShowContentDescriptionImage = obj.ShowContentDescriptionImage;
            response.AliasTextForNextButton = obj.AliasTextForNextButton;
            response.EnableNextButton = obj.EnableNextButton;
            response.ViewPreviousQuestion = obj.ViewPreviousQuestion;
            response.AutoPlay = obj.AutoPlay;
            response.SecondsToApply = obj.SecondsToApply;
            response.VideoFrameEnabled = obj.VideoFrameEnabled;
            response.AutoPlayForDescription = obj.AutoPlayForDescription;
            response.SecondsToApplyForDescription = obj.SecondsToApplyForDescription;
            response.DescVideoFrameEnabled = obj.DescVideoFrameEnabled;
            response.DisplayOrderForTitle = obj.DisplayOrderForTitle;
            response.DisplayOrderForTitleImage = obj.DisplayOrderForTitleImage;
            response.DisplayOrderForDescription = obj.DisplayOrderForDescription;
            response.DisplayOrderForDescriptionImage = obj.DisplayOrderForDescriptionImage;
            response.DisplayOrderForNextButton = obj.DisplayOrderForNextButton;
            return response;
        }
    }

    public class QuizAttachmentResponse : IResponse
    {
        public int QuizId { get; set; }
        public List<Attachment> Attachments { get; set; }
        public class Attachment
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string PublicIdForAttachment { get; set; }
        }
        public IResponse MapEntityToResponse(Base EntityObj)
        {
            QuizAttachmentResponse response = new QuizAttachmentResponse();

            var obj = (QuizAttachment)EntityObj;

            response.QuizId = obj.QuizId;
            response.Attachments = new List<Attachment>();
            if (obj.Attachments != null)
            {
                foreach (var attachment in obj.Attachments)
                {
                    response.Attachments.Add(new Attachment
                    {
                        Title = attachment.Title,
                        Description = attachment.Description,
                        PublicIdForAttachment = attachment.PublicIdForAttachment
                    });
                }

            }

            return response;
        }
    }


    public class QuizBadgeResponse : IResponse
    {
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

        public IResponse MapEntityToResponse(Base EntityObj)
        {
            QuizBadgeResponse response = new QuizBadgeResponse();

            var obj = (QuizBadge)EntityObj;

            response.Id = obj.Id;
            response.Title = obj.Title;
            response.ShowTitle = obj.ShowTitle;
            response.Image = obj.Image;
            response.ShowImage = obj.ShowImage;
            response.EnableMediaFile = obj.EnableMediaFile;
            response.PublicIdForBadge = obj.PublicIdForBadge;
            response.AutoPlay = obj.AutoPlay;
            response.SecondsToApply = obj.SecondsToApply;
            response.VideoFrameEnabled = obj.VideoFrameEnabled;
            response.DisplayOrderForTitle = obj.DisplayOrderForTitle;
            response.DisplayOrderForTitleImage = obj.DisplayOrderForTitleImage;
            return response;
        }
    }

    public class ResultCorrelationResponse : IResponse
    {
        public int AnswerId { get; set; }
        public int ResultId { get; set; }

        public IResponse MapEntityToResponse(Base EntityObj)
        {
            ResultCorrelationResponse response = new ResultCorrelationResponse();

            var obj = (Correlation)EntityObj;

            response.AnswerId = obj.AnswerId;
            response.ResultId = obj.ResultId;

            return response;
        }
    }

    public class QuizShareResponse : IResponse
    {
        public int QuizId { get; set; }
        public bool Recruiter { get; set; }
        public bool Lead { get; set; }
        public bool Public { get; set; }
        public bool JobRockAcademy { get; set; }
        public bool TechnicalRecruiter { get; set; }
        public int? Module { get; set; }
        public IResponse MapEntityToResponse(Base EntityObj)
        {
            QuizShareResponse response = new QuizShareResponse();

            var obj = (QuizShare)EntityObj;

            response.QuizId = obj.QuizId;
            var UserTypeIds = obj.UserType.Split(',').Where(a => !string.IsNullOrEmpty(a)).Select(a => int.Parse(a)).ToList();
            foreach (var UserTypeId in UserTypeIds)
            {
                if (UserTypeId == (int)UserTypeEnum.Recruiter)
                {
                    response.Recruiter = true;
                }
                if (UserTypeId == (int)UserTypeEnum.Lead)
                {
                    response.Lead = true;
                }
                if (UserTypeId == (int)UserTypeEnum.Public)
                {
                    response.Public = true;
                }
                if (UserTypeId == (int)UserTypeEnum.JobRockAcademy)
                {
                    response.JobRockAcademy = true;
                }
                if (UserTypeId == (int)UserTypeEnum.TechnicalRecruiter)
                {
                    response.TechnicalRecruiter = true;
                }
            }
            response.Module = obj.ModuleType;
            return response;
        }
    }

    public class AttemptedQuizDetailResponse : IResponse
    {
        public string ConfigurationId { get; set; }
        public QuizDetail QuizDetails { get; set; }
        public List<QuestionDetail> QuestionDetails { get; set; }
        public class QuizDetail
        {
            public int QuizId { get; set; }
            public string QuizTitle { get; set; }
            public string QuizStatus { get; set; }
            public DateTime? QuizDate { get; set; }
            public List<AchievedResult> AchievedResults { get; set; }
            public class AchievedResult
            {
                public int Id { get; set; }
                public string Title { get; set; }
                public string InternalTitle { get; set; }
            }
        }
        public class QuestionDetail
        {
            public int QuiestionId { get; set; }
            public int AnswerType { get; set; }
            public string QuiestionTitle { get; set; }
            public bool? IsCorrect { get; set; }
            public List<QuestionAnswerDetail> QuestionAnswerDetails { get; set; }
            public class QuestionAnswerDetail
            {
                public int? AnswerId { get; set; }
                public string AnswerTitle { get; set; }
                public int? AssociatedScore { get; set; }
                public string Comment { get; set; }
            }
        }


        public IResponse MapEntityToResponse(Base EntityObj)
        {
            AttemptedQuizDetailResponse response = new AttemptedQuizDetailResponse();

            var obj = (AttemptedQuizDetail)EntityObj;
            response.ConfigurationId = obj.ConfigurationId;
            response.QuizDetails = new QuizDetail();
            if (obj.QuizDetails != null)
            {
                response.QuizDetails.QuizId = obj.QuizDetails.QuizId;
                response.QuizDetails.QuizTitle = obj.QuizDetails.QuizTitle;
                response.QuizDetails.QuizStatus = obj.QuizDetails.QuizStatus;
                response.QuizDetails.QuizDate = obj.QuizDetails.QuizDate;

                response.QuizDetails.AchievedResults = new List<QuizDetail.AchievedResult>();

                if (obj.QuizDetails.AchievedResults != null)
                {
                    foreach (var achievedResultsObj in obj.QuizDetails.AchievedResults)
                    {
                        var achievedResults = new QuizDetail.AchievedResult();
                        achievedResults.Id = achievedResultsObj.Id;
                        achievedResults.Title = achievedResultsObj.Title;
                        achievedResults.InternalTitle = achievedResultsObj.InternalTitle;
                        response.QuizDetails.AchievedResults.Add(achievedResults);
                    }
                }
            }

            response.QuestionDetails = new List<QuestionDetail>();
            if (obj.QuestionDetails != null)
            {
                foreach (var questionDetails in obj.QuestionDetails)
                {
                    var questionDetail = new QuestionDetail();
                    questionDetail.QuiestionId = questionDetails.QuiestionId;
                    questionDetail.AnswerType = questionDetails.AnswerType;
                    questionDetail.QuiestionTitle = questionDetails.QuiestionTitle;
                    questionDetail.IsCorrect = questionDetails.IsCorrect;

                    questionDetail.QuestionAnswerDetails = new List<QuestionDetail.QuestionAnswerDetail>();

                    foreach (var quizAnswerStatsObj in questionDetails.QuestionAnswerDetails)
                    {
                        var questionAnswerDetail = new QuestionDetail.QuestionAnswerDetail();
                        questionAnswerDetail.AnswerId = quizAnswerStatsObj.AnswerId;
                        questionAnswerDetail.AnswerTitle = quizAnswerStatsObj.AnswerTitle;
                        questionAnswerDetail.AssociatedScore = quizAnswerStatsObj.AssociatedScore;
                        questionAnswerDetail.Comment = quizAnswerStatsObj.Comment;
                        questionDetail.QuestionAnswerDetails.Add(questionAnswerDetail);
                    }
                    response.QuestionDetails.Add(questionDetail);
                }
            }
            return response;
        }
    }

    public class PersonalityResultSettingResponse : IResponse
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
        public bool ShowLeadUserForm { get; set; }
        public List<ResultDetailResponse> ResultDetails { get; set; }

        public class ResultDetailResponse
        {
            public int ResultId { get; set; }
            public string Title { get; set; }
            public string Image { get; set; }
        }

        public IResponse MapEntityToResponse(Base EntityObj)
        {
            PersonalityResultSettingResponse response = new PersonalityResultSettingResponse();

            var obj = (PersonalityResultSettingModel)EntityObj;
            response.Id = obj.Id;
            response.QuizId = obj.QuizId;
            response.Title = obj.Title;
            response.Status = obj.Status;
            response.MaxResult = obj.MaxResult;
            response.GraphColor = obj.GraphColor;
            response.ButtonColor = obj.ButtonColor;
            response.ButtonFontColor = obj.ButtonFontColor;
            response.SideButtonText = obj.SideButtonText;
            response.IsFullWidthEnable = obj.IsFullWidthEnable;
            response.LastUpdatedOn = obj.LastUpdatedOn;
            response.LastUpdatedBy = obj.LastUpdatedBy;
            response.ShowLeadUserForm = obj.ShowLeadUserForm;

            if (obj.ResultDetails != null && obj.ResultDetails.Count() > 0)
            {
                response.ResultDetails = new List<ResultDetailResponse>();
                foreach (var item in obj.ResultDetails)
                {
                    response.ResultDetails.Add(new ResultDetailResponse
                    {
                        ResultId = item.ResultId,
                        Title = item.Title,
                        Image = item.Image,
                    });
                }
            }
            return response;
        }
    }

    public class AttemptedAutomationResponse : IResponse
    {
        public string LeadId { get; set; }

        public List<quizDetail> quizDetails { get; set; }

        public class quizDetail
        {
            public QuizDetail QuizDetails { get; set; }
            public List<QuestionDetail> QuestionDetails { get; set; }
            public class QuizDetail
            {
                public int QuizId { get; set; }
                public string QuizTitle { get; set; }
                public string QuizStatus { get; set; }
                public DateTime? QuizDate { get; set; }
                public List<AchievedResult> AchievedResults { get; set; }
                public class AchievedResult
                {
                    public int Id { get; set; }
                    public string Title { get; set; }
                    public string InternalTitle { get; set; }
                }
            }
            public class QuestionDetail
            {
                public int QuiestionId { get; set; }
                public int AnswerType { get; set; }
                public string QuiestionTitle { get; set; }
                public bool? IsCorrect { get; set; }
                public List<QuestionAnswerDetail> QuestionAnswerDetails { get; set; }
                public class QuestionAnswerDetail
                {
                    public int? AnswerId { get; set; }
                    public string AnswerTitle { get; set; }
                    public int? AssociatedScore { get; set; }
                    public string Comment { get; set; }
                }
            }
        }


        public IResponse MapEntityToResponse(Base EntityObj)
        {
            AttemptedAutomationResponse response = new AttemptedAutomationResponse();

            var obj = (AttemptedAutomation)EntityObj;
            response.LeadId = obj.LeadId;
            response.quizDetails = new List<quizDetail>();
            if (obj.quizDetails.Any())
            {
                foreach (var quizDetail in obj.quizDetails.OrderByDescending(r => r.QuizDetails.QuizDate))
                {
                    var attemptedAutomationQuizDetail = new quizDetail();

                    attemptedAutomationQuizDetail.QuizDetails = new quizDetail.QuizDetail();
                    if (quizDetail.QuizDetails != null)
                    {
                        attemptedAutomationQuizDetail.QuizDetails.QuizId = quizDetail.QuizDetails.QuizId;
                        attemptedAutomationQuizDetail.QuizDetails.QuizTitle = quizDetail.QuizDetails.QuizTitle;
                        attemptedAutomationQuizDetail.QuizDetails.QuizStatus = quizDetail.QuizDetails.QuizStatus;
                        attemptedAutomationQuizDetail.QuizDetails.QuizDate = quizDetail.QuizDetails.QuizDate;

                        attemptedAutomationQuizDetail.QuizDetails.AchievedResults = new List<quizDetail.QuizDetail.AchievedResult>();

                        if (quizDetail.QuizDetails.AchievedResults != null)
                        {
                            foreach (var achievedResultsObj in quizDetail.QuizDetails.AchievedResults)
                            {
                                var achievedResults = new quizDetail.QuizDetail.AchievedResult();
                                achievedResults.Id = achievedResultsObj.Id;
                                achievedResults.Title = achievedResultsObj.Title;
                                achievedResults.InternalTitle = achievedResultsObj.InternalTitle;
                                attemptedAutomationQuizDetail.QuizDetails.AchievedResults.Add(achievedResults);
                            }
                        }
                    }

                    attemptedAutomationQuizDetail.QuestionDetails = new List<quizDetail.QuestionDetail>();
                    if (quizDetail.QuestionDetails != null)
                    {
                        foreach (var questionDetails in quizDetail.QuestionDetails)
                        {
                            var questionDetail = new quizDetail.QuestionDetail();
                            questionDetail.QuiestionId = questionDetails.QuiestionId;
                            questionDetail.AnswerType = questionDetails.AnswerType;
                            questionDetail.QuiestionTitle = questionDetails.QuiestionTitle;
                            questionDetail.IsCorrect = questionDetails.IsCorrect;

                            questionDetail.QuestionAnswerDetails = new List<quizDetail.QuestionDetail.QuestionAnswerDetail>();

                            foreach (var quizAnswerStatsObj in questionDetails.QuestionAnswerDetails)
                            {
                                var questionAnswerDetail = new quizDetail.QuestionDetail.QuestionAnswerDetail();
                                questionAnswerDetail.AnswerId = quizAnswerStatsObj.AnswerId;
                                questionAnswerDetail.AnswerTitle = quizAnswerStatsObj.AnswerTitle;
                                questionAnswerDetail.AssociatedScore = quizAnswerStatsObj.AssociatedScore;
                                questionAnswerDetail.Comment = quizAnswerStatsObj.Comment;
                                questionDetail.QuestionAnswerDetails.Add(questionAnswerDetail);
                            }
                            attemptedAutomationQuizDetail.QuestionDetails.Add(questionDetail);
                        }
                    }

                    response.quizDetails.Add(attemptedAutomationQuizDetail);
                }
            }

            return response;
        }
    }

    public class QuizPreviousQuestionSettingResponse : IResponse
    {
        public int QuizId { get; set; }
        public bool ViewPreviousQuestion { get; set; }
        public bool EditAnswer { get; set; }
        public bool ApplyToAll { get; set; }

        public IResponse MapEntityToResponse(Base EntityObj)
        {
            QuizPreviousQuestionSettingResponse response = new QuizPreviousQuestionSettingResponse();

            var obj = (QuizDetailsModel)EntityObj;

            response.QuizId = obj.QuizId;
            response.ViewPreviousQuestion = obj.ViewPreviousQuestion;
            response.EditAnswer = obj.EditAnswer;
            response.ApplyToAll = obj.ApplyToAll;

            return response;
        }
    }
    public class QuizResponse : IResponse
    {
        public int CurrentPageIndex { get; set; }
        public int TotalRecords { get; set; }
        public List<QuizListResponse> QuizListResponse { get; set; }

        public IResponse MapEntityToResponse(Base EntityObj)
        {
            QuizResponse response = new QuizResponse();

            var quizObjs = (QuizList)EntityObj;

            response.CurrentPageIndex = quizObjs.CurrentPageIndex;
            response.TotalRecords = quizObjs.TotalRecords;
            response.QuizListResponse = new List<QuizListResponse>();

            foreach (var quizObj in quizObjs.Quiz)
            {
                var responses = new QuizListResponse();
                responses.Id = quizObj.Id;
                responses.IsPublished = quizObj.IsPublished;
                responses.IsBranchingLogicEnabled = quizObj.IsBranchingLogicEnabled ?? false;
                responses.QuizTitle = quizObj.QuizTitle == null ? string.Empty : quizObj.QuizTitle;
                responses.createdOn = quizObj.CreatedOn;
                responses.PublishedCode = quizObj.PublishedCode;
                responses.OfficeId = quizObj.AccessibleOfficeId;
                responses.CreatedById = quizObj.CreatedByID;
                responses.LastEditDate = quizObj.LastEditDate.HasValue ? quizObj.LastEditDate : null;
                responses.QuizTypeId = (int)quizObj.QuizType;
                responses.QuizCoverDetail = new QuizCoverDetails();
                responses.QuizCoverDetail.QuizCoverImage = quizObj.QuizCoverDetails.QuizCoverImage;
                responses.QuizCoverDetail.QuizCoverTitle = quizObj.QuizCoverDetails.QuizCoverTitle;
                responses.QuizCoverDetail.PublicId = quizObj.QuizCoverDetails.PublicIdForQuizCover;
                responses.NoOfQusetions = quizObj.NoOfQusetions;
                responses.IsCreatedByYou = quizObj.IsCreatedByYou;
                responses.IsFavorited = quizObj.IsFavorited;
                responses.Tag = quizObj.Tag;
                responses.IsWhatsAppChatBotOldVersion = quizObj.IsWhatsAppChatBotOldVersion;

                responses.UsageTypes = new List<int>();
                foreach (var item in quizObj.UsageTypes)
                {
                    responses.UsageTypes.Add(item);
                }

                if (!string.IsNullOrEmpty(quizObj.AccessibleOfficeId) || quizObj.IsCreateStandardAutomation)
                {
                    responses.IsViewOnly = false;
                }
                else
                {
                    responses.IsViewOnly = true;
                }

                response.QuizListResponse.Add(responses);
            }

            return response;
        }
    }

    public class AttemptedAutomationAcheivedResultDetailsResponse : IResponse
    {
        public string LeadId { get; set; }

        public List<AchievedResult> AchievedResults { get; set; }
        public class AchievedResult
        {
            public int Id { get; set; }
            public string Title { get; set; }
        }

        public IResponse MapEntityToResponse(Base EntityObj)
        {
            AttemptedAutomationAcheivedResultDetailsResponse response = new AttemptedAutomationAcheivedResultDetailsResponse();

            var obj = (AttemptedAutomationAchievedResultDetails)EntityObj;
            response.LeadId = obj.LeadId;
            if (obj.AchievedResults.Any())
            {
                response.AchievedResults = new List<AchievedResult>();
                foreach (var resultDetail in obj.AchievedResults)
                {
                    var achievedResults = new AchievedResult();
                    achievedResults.Id = resultDetail.Id;
                    achievedResults.Title = resultDetail.Title;
                    response.AchievedResults.Add(achievedResults);

                }
            }

            return response;
        }
    }

    public class AutomationListResponse : IResponse
    {
        public List<Automation> Automations { get; set; }
        public List<AutomationType> AutomationTypes { get; set; }
        public List<Tags> Tag { get; set; }

        public class Automation
        {
            public int QuizId { get; set; }
            public string QuizTitle { get; set; }
        }
        public class AutomationType
        {
            public int AutomationTypeId { get; set; }
            public string AutomationTypeName { get; set; }
        }
        public IResponse MapEntityToResponse(Base EntityObj)
        {
            AutomationListResponse response = new AutomationListResponse();

            var quizObj = (AutomationDetails)EntityObj;
            response.Automations = new List<Automation>();

            foreach (var quiz in quizObj.Automations)
            {
                response.Automations.Add(new Automation()
                {
                    QuizId = quiz.QuizId,
                    QuizTitle = quiz.QuizTitle
                });
            }
            response.AutomationTypes = new List<AutomationType>();

            foreach (var AutomationType in quizObj.AutomationTypes)
            {
                response.AutomationTypes.Add(new AutomationType()
                {
                    AutomationTypeId = AutomationType.AutomationTypeId,
                    AutomationTypeName = AutomationType.AutomationTypeName
                });
            }

            response.Tag = new List<Tags>();

            foreach (var tag in quizObj.Tag)
            {
                response.Tag.Add(new Tags() { TagId = tag.TagId, TagName = tag.TagName, TagCode = tag.TagCode });
            }

            return response;
        }
    }

    public class DynamicFieldDetailsResponse : IResponse
    {
        public List<string> DynamicVariables { get; set; }
        public MediaVariable MediaVariables { get; set; }

        public IResponse MapEntityToResponse(Base EntityObj)
        {
            DynamicFieldDetailsResponse response = new DynamicFieldDetailsResponse();

            var obj = (DynamicFieldDetails)EntityObj;

            response.DynamicVariables = obj.DynamicVariables;
            response.MediaVariables = obj.MediaVariables;

            return response;
        }
    }

    public class TagsResponse : IResponse
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int TagId { get; set; }
        public string TagName { get; set; }
        public string TagCode { get; set; }

        public IResponse MapEntityToResponse(Base obj)
        {
            TagsResponse tagsResponse = new TagsResponse();
            var tagsObj = (AutomationTagsDetails)obj;

            tagsResponse.CategoryId = tagsObj.categoryId;
            tagsResponse.CategoryName = tagsObj.categoryName;
            tagsResponse.TagId = tagsObj.tagId;
            tagsResponse.TagName = tagsObj.tagName;
            tagsResponse.TagCode = tagsObj.tagCode;

            return tagsResponse;
        }
    }

    public class QuizUsageTypeDetailsResponse : IResponse
    {
        public List<int> UsageTypes { get; set; }
        public List<Tags> Tag { get; set; }

        public IResponse MapEntityToResponse(Base EntityObj)
        {
            QuizUsageTypeDetailsResponse response = new QuizUsageTypeDetailsResponse();

            var obj = (QuizUsageTypeDetails)EntityObj;

            response.UsageTypes = obj.UsageTypes;
            response.Tag = obj.Tag;

            return response;
        }
    }
}