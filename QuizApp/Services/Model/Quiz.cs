using QuizApp.Helpers;
using QuizApp.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static QuizApp.Response.AttemptQuizResponse;
using static QuizApp.Response.AttemptQuizResponse.SubmittedAnswerResult;
using static QuizApp.Services.Model.WhatsappMessage;

namespace QuizApp.Services.Model
{

    public class QuizList : Base
    {
        public int CurrentPageIndex { get; set; }
        public int TotalRecords { get; set; }
        public List<LocalQuiz> Quiz { get; set; }
    }
    public class LocalQuiz : Base
    {
        public int Id { get; set; }
        public string QuizTitle { get; set; }
        public QuizTypeEnum QuizType { get; set; }
        public bool IsPublished { get; set; }
        public bool? IsBranchingLogicEnabled { get; set; }
        public string AccessibleOfficeId { get; set; }
        public long CreatedByID { get; set; }
        public long OfficeId { get; set; }
        public string OfficeName { get; set; }
        public string CreatedByName { get; set; }
        public DateTime? LastEditDate { get; set; }
        public DateTime CreatedOn { get; set; }
        public string PublishedCode { get; set; }
        public bool IsCreatedByYou { get; set; }
        public int? CategoryId { get; set; }
        public QuizCover QuizCoverDetails { get; set; }
        public int NoOfQusetions { get; set; }
        public bool IsFavorited { get; set; }
        public bool IsCreateStandardAutomation { get; set; }
        public string UserType { get; set; }
        public int? ModuleType { get; set; }
        public List<int> UsageTypes { get; set; }
        public List<Tags> Tag { get; set; }
        public int CurrentPageIndex { get; set; }
        public int TotalRecords { get; set; }
        public bool? IsWhatsAppChatBotOldVersion { get; set; }
    }
    public class AutomationDetail : Base {
        public int Id { get; set; }
        public string QuizTitle { get; set; }
        public QuizTypeEnum QuizTypeId { get; set; }
        public bool IsPublished { get; set; }
        public bool? IsBranchingLogicEnabled { get; set; }
        public string AccessibleOfficeId { get; set; }
        public long CreatedByID { get; set; }
        public long OfficeId { get; set; }
        public string OfficeName { get; set; }
        public string CreatedByName { get; set; }
        public DateTime? LastEditDate { get; set; }
        public DateTime CreatedOn { get; set; }
        public string PublishedCode { get; set; }
        public bool IsCreatedByYou { get; set; }
        public int? CategoryId { get; set; }
        public QuizCover QuizCoverDetail { get; set; }
        public int NoOfQusetions { get; set; }
        public bool IsFavorited { get; set; }
        public bool IsCreateStandardAutomation { get; set; }
        public string UserType { get; set; }
        public int? ModuleType { get; set; }
        public List<int> UsageTypes { get; set; }
        public List<Tags> Tag { get; set; }
        public int CurrentPageIndex { get; set; }
        public int TotalRecords { get; set; }
        public bool? IsWhatsAppChatBotOldVersion { get; set; }
    }
    public class QuizBrandingAndStyleModel : Base
    {
        public int QuizId { get; set; }
        public string ImageFileURL { get; set; }
        public string PublicIdForFileURL { get; set; }
        public string PublicIdForImageFile { get; set; }
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
    }

    public class QuizSocialShareSetting : Base
    {
        public int QuizId { get; set; }
        public bool? HideSocialShareButtons { get; set; }
        public bool? EnableFacebookShare { get; set; }
        public bool? EnableTwitterShare { get; set; }
        public bool? EnableLinkedinShare { get; set; }
    }

    public class QuizCover : Base
    {
        public int QuizId { get; set; }
        public int QuizType { get; set; }
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
    }

    public class QuizDto : Base
    {
        public int QuizId { get; set; }
        public string QuizTitle { get; set; }
    }

    public class QuizCoverDetails
    {
        public string QuizCoverTitle { get; set; }
        public string QuizCoverImage { get; set; }
        public string PublicId { get; set; }
    }

    public class QuizQuestion : Base
    {
        public int QuizType { get; set; }
        public int QuizId { get; set; }
        public int QuestionId { get; set; }
        public bool? ShowAnswerImage { get; set; }
        public string QuestionTitle { get; set; }
        public bool ShowTitle { get; set; }
        public string QuestionImage { get; set; }
        public bool EnableMediaFile { get; set; }
        public string PublicIdForQuestion { get; set; }
        public bool? ShowQuestionImage { get; set; }
        public int DisplayOrder { get; set; }
        public int? MaxAnswer { get; set; }
        public int? MinAnswer { get; set; }
        public int AnswerType { get; set; }
        public string NextButtonText { get; set; }
        public string NextButtonTxtSize { get; set; }
        public string NextButtonTxtColor { get; set; }
        public string NextButtonColor { get; set; }
        public bool EnableNextButton { get; set; }
        public bool ViewPreviousQuestion { get; set; }
        public bool EditAnswer { get; set; }
        public DateTime? StartedOn { get; set; }
        public bool TimerRequired { get; set; }
        public string Time { get; set; }
        public bool AutoPlay { get; set; }
        public string SecondsToApply { get; set; }
        public bool? VideoFrameEnabled { get; set; }
        public bool? DescVideoFrameEnabled { get; set; }
        public int? AnswerStructureType { get; set; }
        public string Description { get; set; }
        public bool ShowDescription { get; set; }
        public string DescriptionImage { get; set; }
        public bool EnableMediaFileForDescription { get; set; }
        public string PublicIdForDescription { get; set; }
        public bool ShowDescriptionImage { get; set; }
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
        public int? TemplateId { get; set; }
        public string LanguageCode { get; set; }
        public bool IsBackButtonEnable { get; set; }
        public bool IsMultiRating { get; set; }
        public List<AnswerOptionInQuestion> AnswerList { get; set; }
        public QuizCorrectAnswerSetting QuizCorrectAnswerSetting { get; set; }
        public WhatsAppTemplateDto WhatsAppTemplateDtos { get; set; }
        public WhatsappDetails WhatsappDetails { get; set; }     
        public List<string> MsgVariables { get; set; }
    }

    
  

    public class QuestionMappedValue {
        public List<QuestionDetail> QuestionDetails { get; set; }
        public class QuestionDetail {
            public string QuestionTitle { get; set; }
            public bool ShowTitle { get; set; }
            public string QuestionImage { get; set; }
            public int AnswerId { get; set; }
            public string ObjectName { get; set; }
            public string FieldName { get; set; }
            public string Value { get; set; }
            public bool? IsExternalSync { get; set; }
            public List<ObjectFieldsDetail> ObjectFieldsInAnswer { get; set; }
            public class ObjectFieldsDetail {
                public int AnswerId { get; set; }
                public string ObjectName { get; set; }
                public string FieldName { get; set; }
                public string Value { get; set; }
                public bool? IsExternalSync { get; set; }
                public bool IsCommentMapped { get; set; }
            }
        }
    }

        public class WhatsappDetails
    {
        public int? TemplateId { get; set; }
        public string LanguageCode { get; set; }
        public List<HeaderParameter> HeaderParams { get; set; }
        public List<WhatsappParam> WhatsappParam { get; set; }

    }

    public class WhatsappParam
    {
        public string Paraname { get; set; }
        public int Position { get; set; }
        public object Value { get; set; }
    }

    public class QuizQuestionAndContent : Base
    {
        //question
        public int QuestionId { get; set; }
        public string QuestionTitle { get; set; }
        public int DisplayOrder { get; set; }
        public bool? ShowAnswerImage { get; set; }
        public int AnswerType { get; set; }
        public int? MaxAnswer { get; set; }
        public int? MinAnswer { get; set; }
        public bool TimerRequired { get; set; }
        public string Time { get; set; }
        public bool AutoPlay { get; set; }
        public string SecondsToApply { get; set; }
        public bool? VideoFrameEnabled { get; set; }
        public List<AnswerOptionInQuestion> AnswerList { get; set; }
        //public int? LanguageId { get; set; }
        public string LanguageCode { get; set; }

        public int? TemplateId { get; set; }
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
        public int? AnswerStructureType { get; set; }
        public int Type { get; set; }
        public bool IsMultiRating { get; set; }
    }

    public class QuizReorderResult : Base
    {
        public List<QuizQuestion> QuizQuestionList { get; set; }
        public List<QuizResult> QuizResultList { get; set; }
    }
    public class QuizCorrectAnswerSetting : Base
    {
        public int QuestionId { get; set; }
        public List<int> CorrectAnswerId { get; set; }
        public string CorrectAnswerExplanation { get; set; }
        public bool? RevealCorrectAnswer { get; set; }
        public string AliasTextForCorrect { get; set; }
        public string AliasTextForIncorrect { get; set; }
        public string AliasTextForYourAnswer { get; set; }
        public string AliasTextForCorrectAnswer { get; set; }
        public string AliasTextForExplanation { get; set; }
        public string AliasTextForNextButton { get; set; }
        public int AnswerType { get; set; }
        public int MinAnswer { get; set; }
        public int MaxAnswer { get; set; }
        public bool ViewPreviousQuestion { get; set; }
        public bool EditAnswer { get; set; }
        public bool TimerRequired { get; set; }
        public string Time { get; set; }
        public bool AutoPlay { get; set; }
        public List<AnswerScore> AnswerScoreData { get; set; }
    }

    public class AnswerScore
    {
        public int AnswerId { get; set; }
        public int AssociatedScore { get; set; }

    }

    public class AnswerOptionInQuestion : Base
    {
        public int AnswerId { get; set; }
        public string AnswerText { get; set; }
        public string AnswerDescription { get; set; }
        public string AnswerImage { get; set; }
        public bool EnableMediaFile { get; set; }
        public string PublicIdForAnswer { get; set; }
        public bool? IsCorrectAnswer { get; set; }
        public int DisplayOrder { get; set; }
        public int? AssociatedScore { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsUnansweredType { get; set; }
        public bool AutoPlay { get; set; }
        public string SecondsToApply { get; set; }
        public bool? VideoFrameEnabled { get; set; }
        public string OptionTextforRatingOne { get; set; }
        public string OptionTextforRatingTwo { get; set; }
        public string OptionTextforRatingThree { get; set; }
        public string OptionTextforRatingFour { get; set; }
        public string OptionTextforRatingFive { get; set; }
        public string ListValues { get; set; }
        public List<CategoryModel> Categories { get; set; }
        public ObjectFieldsDetails ObjectFieldsInAnswer { get; set; }
        public ObjectFieldsDetails ObjectFieldsInAnswerComment { get; set; }
        public int? RefId { get; set; }

        public class CategoryModel
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
        
        public class ObjectFieldsDetails
        {
            public string ObjectName { get; set; }
            public string FieldName { get; set; }
            public string Value { get; set; }
            public bool? IsExternalSync { get; set; }
            public bool IsCommentMapped { get; set; }
        }
    }
    public class QuizResult : Base
    {
        public int QuizId { get; set; }
        public int ResultId { get; set; }
        public string Title { get; set; }
        public bool ShowExternalTitle { get; set; }
        public string InternalTitle { get; set; }
        public bool ShowInternalTitle { get; set; }
        public string Image { get; set; }
        public bool EnableMediaFile { get; set; }
        public string PublicIdForResult { get; set; }
        public bool ShowResultImage { get; set; }
        public string Description { get; set; }
        public bool ShowDescription { get; set; }
        public bool? HideCallToAction { get; set; }
        public string ActionButtonURL { get; set; }
        public bool? OpenLinkInNewTab { get; set; }
        public string ActionButtonTxtSize { get; set; }
        public string ActionButtonColor { get; set; }
        public string ActionButtonTitleColor { get; set; }
        public string ActionButtonText { get; set; }
        public QuizResultSetting ResultSetting { get; set; }
        public int? MinScore { get; set; }
        public int? MaxScore { get; set; }
        public int DisplayOrder { get; set; }
        public bool ShowLeadUserForm { get; set; }
        public bool AutoPlay { get; set; }
        public string SecondsToApply { get; set; }
        public bool? VideoFrameEnabled { get; set; }
        public int DisplayOrderForTitle { get; set; }
        public int DisplayOrderForTitleImage { get; set; }
        public int DisplayOrderForDescription { get; set; }
        public int DisplayOrderForNextButton { get; set; }
        public bool EnableCallToActionButton { get; set; }
        public List<string> MsgVariables { get; set; }
    }

    public class QuizResultSetting : Base
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
    }

    public class QuizDetailsModel : Base
    {
        public int QuizId { get; set; }
        public string PublishedCode { get; set; }
        public bool IsPublished { get; set; }
        public bool? IsBranchingLogicEnabled { get; set; }
        public int MultipleResultEnabled { get; set; }
        public int QuizTypeId { get; set; }
        public List<int> UsageType { get; set; }
        public string OfficeId { get; set; }
        public bool ViewPreviousQuestion { get; set; }
        public bool EditAnswer { get; set; }
        public bool ApplyToAll { get; set; }
        public bool AutoPlay { get; set; }
        public string SecondsToApply { get; set; }
        public bool? VideoFrameEnabled { get; set; }
        public bool IsQuesAndContentInSameTable { get; set; }
        public QuizCover QuizCoverDetails { get; set; }
        public List<QuizQuestion> QuestionsInQuiz { get; set; }
        public List<QuizResult> ResultList { get; set; }
        public QuizBrandingAndStyleModel QuizBrandingAndStyle { get; set; }
        public List<QuizAction> QuizAction { get; set; }
        public List<QuizContent> QuizContent { get; set; }
        public List<QuizBadge> BadgeList { get; set; }
        public List<QuizQuestionAndContent> QuestionsAndContent { get; set; }
        public bool? IsWhatsAppChatBotOldVersion { get; set; }
        public bool?  EverPublished { get; set; }
    }

    public class QuizBranchingLogicData : Base
    {
        public BranchingLogicStartTypeEnum StartType { get; set; }
        public int StartTypeId { get; set; }
        public List<BranchingLogicModel> QuestionList { get; set; }
        public List<ResultBranchingLogic> ResultList { get; set; }
        public List<ContentBranchingLogic> ContentList { get; set; }
    }

    public class QuizBranchingLogicLinksList : Base
    {
        public int QuizId { get; set; }
        public List<QuizBranchingLogicLinks> QuizBranchingLogicLinks { get; set; }
    }

    public class QuizBranchingLogicLinks
    {
        public BranchingLogicEnum ObjectType { get; set; }
        public string ObjectTypeId { get; set; }
        public List<BranchingLinks> Links { get; set; }
        public string[] Position { get; set; }

    }

    public class BranchingLinks
    {
        public string FromId { get; set; }
        public string ToId { get; set; }
        public BranchingLogicEnum FromType { get; set; }
        public BranchingLogicEnum ToType { get; set; }
        public bool IsCorrect { get; set; }
        public BranchingLinks LinkedTypeObj { get; set; }
    }
    public class QuizBranchingLogic : Base
    {
        public int QuizId { get; set; }
        public int QuizType { get; set; }
        public string QuizTitle { get; set; }
        public string QuizCoverTitle { get; set; }
        public bool ShowDescription { get; set; }
        public string QuizDescription { get; set; }
        public BranchingLogicEnum StartType { get; set; }
        public int StartTypeId { get; set; }
        public string SecondsToApply { get; set; }
        public bool? VideoFrameEnabled { get; set; }
        public List<int> UsageTypes { get; set; }
        public List<BranchingLogicModel> QuestionList { get; set; }
        public List<ResultBranchingLogic> ResultList { get; set; }
        public List<ContentBranchingLogic> ContentList { get; set; }
        public List<ActionBranchingLogic> ActionList { get; set; }
        public bool IsQuesAndContentInSameTable { get; set; }
        public List<BadgeBranchingLogic> BadgeList { get; set; }
        public List<QuestionAndContentBranchingLogic> QuestionAndContentList { get; set; }
        public bool? IsWhatsAppChatBotOldVersion { get; set; }

    }

    public class QuestionAndContentBranchingLogic : Base
    {
        public int QuizId { get; set; }
        public bool IsDisabled { get; set; }
        public int Type { get; set; }
        public int DisplayOrder { get; set; }
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
        public ContentBranchingLogic LinkedTypeObj { get; set; }
        public string ActionButtonText { get; set; }
        //Question
        public int QuizType { get; set; }
        public int QuestionId { get; set; }
        public string QuestionTxt { get; set; }
        public string QuestionImage { get; set; }
        public string PublicIdForQuestion { get; set; }
        public bool? ShowQuestionImage { get; set; }
        public bool? ShowAnswerImage { get; set; }
        public int? MaxAnswer { get; set; }
        public int? MinAnswer { get; set; }
        public int AnswerType { get; set; }
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

        public class AnswerInQuestions
        {
            public int AnswerId { get; set; }
            public string AnswerTxt { get; set; }
            public string AnswerDescription { get; set; }
            public int? AssociatedScore { get; set; }
            public string AnswerOptionImage { get; set; }
            public string PublicIdForAnswerOption { get; set; }
            public int LinkedToType { get; set; }
            public int? QuestionId { get; set; }
            public int? ResultId { get; set; }
            public int? ContentId { get; set; }
            public string SecondsToApply { get; set; }
            public bool? VideoFrameEnabled { get; set; }
            public BranchingLogicModel LinkedTypeObj { get; set; }
            public int? XCordinate { get; set; }
            public int? YCordinate { get; set; }
            public bool? IsCorrect { get; set; }
            public int DisplayOrder { get; set; }
            public int? RefId { get; set; }
        }
    }

    public class BranchingLogicModel : Base
    {
        public int QuizId { get; set; }
        public int QuizType { get; set; }
        public int QuestionId { get; set; }
        public string QuestionTxt { get; set; }
        public string QuestionImage { get; set; }
        public string PublicIdForQuestion { get; set; }
        public bool? ShowQuestionImage { get; set; }
        public int? MaxAnswer { get; set; }
        public int? MinAnswer { get; set; }
        public bool IsDisabled { get; set; }
        public int AnswerType { get; set; }
        public List<AnswerInQuestions> AnswerList { get; set; }

        public class AnswerInQuestions
        {
            public int AnswerId { get; set; }
            public string AnswerTxt { get; set; }
            public int? AssociatedScore { get; set; }
            public string AnswerOptionImage { get; set; }
            public string PublicIdForAnswerOption { get; set; }
            public int LinkedToType { get; set; }
            public int? QuestionId { get; set; }
            public int? ResultId { get; set; }
            public int? ContentId { get; set; }
            public BranchingLogicModel LinkedTypeObj { get; set; }
            public int? XCordinate { get; set; }
            public int? YCordinate { get; set; }
            public bool? IsCorrect { get; set; }
            public string SecondsToApply { get; set; }
        }
    }
    public class ResultBranchingLogic : Base
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
    public class ContentBranchingLogic : Base
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
        public string SecondsToApply { get; set; }
        public string SecondsToApplyForDescription { get; set; }
        public ContentBranchingLogic LinkedTypeObj { get; set; }
        public string ActionButtonText { get; set; }

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
        public string ActionButtonText { get; set; }
        public string SecondsToApply { get; set; }
        public bool? VideoFrameEnabled { get; set; }
        public string SecondsToApplyForDescription { get; set; }
        public bool? DescVideoFrameEnabled { get; set; }
    }
    public class BadgeBranchingLogic : Base
    {
        public int QuizId { get; set; }
        public int BadgetId { get; set; }
        public string BadgetTitle { get; set; }
        public string BadgetImage { get; set; }
        public string PublicIdForBadget { get; set; }
        public int LinkedToType { get; set; }
        public int? LinkedContentId { get; set; }
        public int? LinkedResultId { get; set; }
        public int? LinkedQuestionId { get; set; }
        public bool IsDisabled { get; set; }
        public int? XCordinate { get; set; }
        public int? YCordinate { get; set; }
        public string SecondsToApply { get; set; }
        public bool? VideoFrameEnabled { get; set; }
    }
    
    public class QuizAnswerSubmit : Base
    {
        public string AppointmentCode { get; set; }
        public string AppointmentLink { get; set; }
        public string AppointmentErrorMessage { get; set; }
        public int? QuestionType { get; set; }
        public bool IsBackButtonEnable { get; set; }
        public bool LoadQuizDetails { get; set; }
        public string PrimaryBrandingColor { get; set; }
        public string SecondaryBrandingColor { get; set; }
        public string TertiaryColor { get; set; }
        public string SourceType { get; set; }
        public string SourceId { get; set; }
        public string SourceName { get; set; }
        public string PrivacyLink { get; set; }
        public string ConfigurationType { get; set; }
        public string ConfigurationId { get; set; }
        public string LeadFormTitle { get; set; }
        public string GtmCode { get; set; }
        public string FavoriteIconUrl { get; set; }
        public string OfficeId { get; set; }
        public int? FormId { get; set; }
        public int? FlowOrder { get; set; }
        public List<int> UsageType { get; set; }
        public List<Tags> Tag { get; set; }
        public QuizCover QuizCoverDetails { get; set; }
        public QuizQuestion QuestionDetails { get; set; }
        public QuizContent ContentDetails { get; set; }
        public QuizBadge BadgeDetails { get; set; }
        public SubmittedAnswerResult SubmittedAnswer { get; set; }
        public SubmittedAnswerResult PreviousQuestionSubmittedAnswer { get; set; }
        public Result ResultScore { get; set; }
        public bool ShowLeadUserForm { get; set; }
        public string CompanyCode { get; set; }
        public QuizBrandingAndStyleModel QuizBrandingAndStyle { get; set; }
        public QuizSocialShareSetting QuizSocialShare { get; set; }
        public PrivacyDto PrivacyJson { get; set; }
        public List<WhatsappParam> WhatsappParams { get; set; }

        public class Result
        {
            public string Title { get; set; }
            public string InternalTitle { get; set; }
            public string Image { get; set; }
            public string PublicIdForResult { get; set; }
            public string Description { get; set; }
            public bool HideActionButton { get; set; }
            public string ActionButtonURL { get; set; }
            public bool? OpenLinkInNewTab { get; set; }
            public string ActionButtonTxtSize { get; set; }
            public string ActionButtonColor { get; set; }
            public string ActionButtonTitleColor { get; set; }
            public string ActionButtonText { get; set; }
            public string ResultScoreValueTxt { get; set; }
            public string AnswerKeyCustomTxt { get; set; }
            public string YourAnswerCustomTxt { get; set; }
            public string CorrectAnswerCustomTxt { get; set; }
            public string ExplanationCustomTxt { get; set; }
            public bool ShowScoreValue { get; set; }
            public bool ShowCorrectAnswer { get; set; }
            public bool HideSocialShareButtons { get; set; }
            public bool ShowFacebookBtn { get; set; }
            public bool ShowTwitterBtn { get; set; }
            public bool ShowLinkedinBtn { get; set; }
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
            public List<AnswerResult> AttemptedQuestionAnswerDetails { get; set; }
            public List<PersonalityResult> PersonalityResultList { get; set; }
        }

        public class AnswerResult
        {
            public string Question { get; set; }
            public string YourAnswer { get; set; }
            public string CorrectAnswer { get; set; }
            public string AnswerExplanation { get; set; }
            public int? AssociatedScore { get; set; }
            public bool? IsCorrect { get; set; }
        }

        public class PersonalityResult
        {
            public string Title { get; set; }
            public string InternalTitle { get; set; }
            public string Image { get; set; }
            public int ResultId { get; set; }
            public string GraphColor { get; set; }
            public string ButtonColor { get; set; }
            public string ButtonFontColor { get; set; }
            public int MaxResult { get; set; }
            public string SideButtonText { get; set; }
            public int? Percentage { get; set; }
            public bool IsFullWidthEnable { get; set; }
            public string Description { get; set; }
            public bool ShowExternalTitle { get; set; }
            public bool ShowInternalTitle { get; set; }
            public bool ShowDescription { get; set; }

        }

        public class SubmittedAnswerResult
        {
            public bool? IsCorrect { get; set; }
            public string AliasTextForCorrect { get; set; }
            public string AliasTextForIncorrect { get; set; }
            public string AliasTextForYourAnswer { get; set; }
            public string AliasTextForCorrectAnswer { get; set; }
            public string AliasTextForExplanation { get; set; }
            public string AliasTextForNextButton { get; set; }
            public string CorrectAnswerDescription { get; set; }
            public bool? ShowAnswerImage { get; set; }
            public List<SubmittedAnswer> SubmittedAnswerDetails { get; set; }
            public class SubmittedAnswer
            {
                public int AnswerId { get; set; }
                public string SubmittedAnswerTitle { get; set; }
                public string SubmittedSecondaryAnswerTitle { get; set; }
                public string SubmittedAnswerImage { get; set; }
                public string PublicIdForSubmittedAnswer { get; set; }
                public int? SubAnswerTypeId { get; set; }
                public bool AutoPlay { get; set; }
                public string SecondsToApply { get; set; }
                public bool? VideoFrameEnabled { get; set; }
                public string Comment { get; set; }
                // for Rating RatingEmoji type question
                public string OptionTextforRatingOne { get; set; }
                public string OptionTextforRatingTwo { get; set; }
                public string OptionTextforRatingThree { get; set; }
                public string OptionTextforRatingFour { get; set; }
                public string OptionTextforRatingFive { get; set; }
            }
            public List<CorrectAnswer> CorrectAnswerDetails { get; set; }
            public class CorrectAnswer
            {
                public string CorrectAnswerTitle { get; set; }
                public string CorrectAnswerImage { get; set; }
                public string PublicIdForCorrectAnswer { get; set; }
                public bool AutoPlay { get; set; }
                public string SecondsToApply { get; set; }
                public bool? VideoFrameEnabled { get; set; }
            }
        }
    }

    public class QuizResultRedirect : Base
    {
        public int QuizId { get; set; }
        public int ResultId { get; set; }
        public string ResultTitle { get; set; }
        public bool? IsRedirectOn { get; set; }
        public string RedirectResultTo { get; set; }
    }

    public class QuizVersion : Base
    {
        public int PublishedQuizId { get; set; }
        public int VersionNumber { get; set; }
        public DateTime PublishedOn { get; set; }
        public DateTime UntilDate { get; set; }
        public bool IsCurrent { get; set; }
    }

    public class QuizAnalyticsOverview : Base
    {
        public int PublishedQuizId { get; set; }
        public int Views { get; set; }
        public int QuizStarts { get; set; }
        public int Completion { get; set; }
        public int Leads { get; set; }
    }

    public class QuizAnalyticsStats : Base
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
            public int QuestionId { get; set; }
            public string QuestionTitle { get; set; }
            public int QuestionAttempts { get; set; }
            public List<AnswerStats> Answers { get; set; }

            public class AnswerStats
            {
                public int AnswerId { get; set; }
                public string AnswerTitle { get; set; }
                public int AnswerAttempts { get; set; }
            }
        }
        public class ResultStats
        {
            public int ResultId { get; set; }
            public string ResultTitle { get; set; }
            public int LeadsInResult { get; set; }
        }
    }

    public class QuizLeadCollectionStats : Base
    {
        public string LeadUserName { get; set; }
        public string LeadUserEmail { get; set; }
        public string LeadUserPhone { get; set; }
        public DateTime AddedOn { get; set; }
    }

    public class QuizAction : Base
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public string Title { get; set; }
        public int? AppointmentId { get; set; }
        public string ReportEmails { get; set; }
        public int ActionType { get; set; }
        public List<int> CalendarIds { get; set; }
        public int? AutomationId { get; set; }
    }

    public class QuizContent : Base
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
        public bool ViewPreviousQuestion { get; set; }
        public bool AutoPlay { get; set; }
        public string SecondsToApply { get; set; }
        public bool? VideoFrameEnabled { get; set; }
        public bool AutoPlayForDescription { get; set; }
        public string SecondsToApplyForDescription { get; set; }
        public bool? DescVideoFrameEnabled { get; set; }
        public int DisplayOrder { get; set; }
        public int DisplayOrderForTitle { get; set; }
        public int DisplayOrderForTitleImage { get; set; }
        public int DisplayOrderForDescription { get; set; }
        public int DisplayOrderForDescriptionImage { get; set; }
        public int DisplayOrderForNextButton { get; set; }
    }

    public class QuizAttachment : Base
    {
        public int QuizId { get; set; }
        public List<Attachment> Attachments { get; set; }
        public class Attachment
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string PublicIdForAttachment { get; set; }
        }

    }

    public class QuizShare : Base
    {
        public int QuizId { get; set; }
        public string UserType { get; set; }
        public int? ModuleType { get; set; }
    }

    public class QuizBadge : Base
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
    }

    public class QuizAnsweTags : Base
    {
        public int answerId { get; set; }
        public List<CategoryModel> Categories { get; set; }
        public class CategoryModel
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

    }

    public class ResultCorrelation : Base
    {
        public int QuestionId { get; set; }
        public int MinAnswer { get; set; }
        public int MaxAnswer { get; set; }
        public List<Correlation> CorrelationList { get; set; }
    }
    public class Correlation : Base
    {
        public int ResultId { get; set; }
        public int AnswerId { get; set; }
    }


    public class AttemptedQuizDetail : Base
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
            public string QuiestionTitle { get; set; }
            public bool? IsCorrect { get; set; }
            public int AnswerType { get; set; }
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
    public class QuizResultRange : Base
    {
        public int QuizId { get; set; }
        public List<Result> Results { get; set; }
        public class Result
        {
            public int ResultId { get; set; }
            public int? MinScore { get; set; }
            public int? MaxScore { get; set; }
        }

    }

    public class QuizResultAndAction : Base
    {
        public List<Result> Results { get; set; }
        //public List<Action> Actions { get; set; }
        public class Result
        {
            public int Id { get; set; }
            public int ParentResultId { get; set; }
            public string Title { get; set; }
            public string InternalTitle { get; set; }
            public string Description { get; set; }
            public bool IsLinkedResult { get; set; }
            public Action ActionDetail { get; set; }
        }
        public class Action
        {
            public int Id { get; set; }
            public int ParentActionId { get; set; }
            public string ActionType { get; set; }
            public string Title { get; set; }
            public int? AppointmentId { get; set; }
            public string AppointmentName { get; set; }
            public string ReportEmails { get; set; }
        }
    }

    public class PersonalityResultSettingModel : Base
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
        public List<ResultDetail> ResultDetails { get; set; }
    }

    public class ResultDetail
    {
        public int ResultId { get; set; }
        public string Title { get; set; }
        public string Image { get; set; }
    }

    public class TextAnswer : Base
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
    }

    public class AttemptedAutomation : Base
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
    }

    public class TagDetail
    {
        public int TagId { get; set; }
        public string LabelText { get; set; }
    }

    public class TemplateAttachment
    {
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public string FileIdentifier { get; set; }
    }

    public class QuizSetting : Base
    {
        public int QuizId { get; set; }
        public bool ViewPreviousQuestion { get; set; }
        public bool EditAnswer { get; set; }
        public bool ApplyToAll { get; set; }
    }

    public class AttemptedAutomationAchievedResultDetails : Base
    {
        public string LeadId { get; set; }

        public List<AchievedResult> AchievedResults { get; set; }
        public class AchievedResult
        {
            public int Id { get; set; }
            public string Title { get; set; }
        }
    }

    public class AutomationDetails : Base
    {
        public List<Automation> Automations { get; set; }
        public List<AutomationType> AutomationTypes { get; set; }
        public List<Tags> Tag { get; set; }
    }
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
    public class Tags
    {
        public int? TagId { get; set; }
        public string TagName { get; set; }
        public string TagCode { get; set; }
    }

    public class DynamicFieldDetails : Base
    {
        public List<string> DynamicVariables { get; set; }
        public MediaVariable MediaVariables { get; set; }
    }

    public class MediaVariable
    {
        public List<Details> CoverDetails { get; set; }
        public List<ContentDetails> Questions { get; set; }
        public List<Details> Answers { get; set; }
        public List<Details> Results { get; set; }
        public List<ContentDetails> Content { get; set; }
        public List<Details> Badges { get; set; }
    }

    public class Details
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public string Title { get; set; }
        public string AnswerDescription { get; set; }
        public string MediaUrl { get; set; }
        public string PublicId { get; set; }
        public int TypeId { get; set; }
    }

    public class ContentDetails
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public string Title { get; set; }
        public string MediaUrl { get; set; }
        public string PublicId { get; set; }
        public string Description { get; set; }
        public string MediaUrlforDescription { get; set; }
        public string PublicIdforDescription { get; set; }
        public int TypeId { get; set; }
    }
    public class FileUploadResponseModel
    {
        public string FileName { get; set; }
        public string FileIdentifier { get; set; }
        public string FileLink { get; set; }
    }

    public class ActionMessageResponseModel
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Content { get; set; }
    }

    public class ObjectFieldsDetails
    {
        public int AnswerId { get; set; }
        public string ObjectName { get; set; }
        public string FieldName { get; set; }
        public string Value { get; set; }
        public bool? IsExternalSync { get; set; }
        public bool IsCommentMapped { get; set; } = false;
    }

    public class QuizUsageTypeDetails : Base
    {
        public List<int> UsageTypes { get; set; }
        public List<Tags> Tag { get; set; }
    }

    public class QuizUsageTypeandTagDetails
    {
        public int QuizId { get; set; }
        public List<int> UsageTypes { get; set; }
        public List<int> TagIds { get; set; }
    }

    public class WhatsAppDetails
    {
        public int HsmTemplateId { get; set; }
        public string HsmTemplateLanguageCode { get; set; }
        public List<TemplateParameter> TemplateParameters { get; set; }
        public string FollowUpMessage { get; set; }
    }

    public class TemplateParameter
    {
        public string Paraname { get; set; }
        public int Position { get; set; }
        public string Value { get; set; }
    }


    public class AttemptedQuizDetails 
    {
        public int QuizDetailId { get; set; }
        public int QuizType { get; set; }
        public int QuizAttemptId { get; set; }
        public string QuizTitle { get; set; }
        public string QuizCoverTitle { get; set; }
        public List<Questions> Question { get; set; }
        public class Questions {
            public int QuestionId { get; set; }
            public string QuestionTitle { get; set; }
            public int AnswerType { get; set; }
            public List<Answers> AnswerList { get; set; }
            public class Answers {
                public int AnswerId { get; set; }
                public string Title { get; set; }
                public string AnswerDescription { get; set; }
                public string Comment { get; set; }
                public string RatingOptionTitle { get; set; }
            }

        }
    }        

}