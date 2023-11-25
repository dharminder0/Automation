using QuizApp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Services.Model
{
    public class PublishQuizTmpModel
    {
     
        public int ParentQuizid { get; set; } = 0;
        public int QuizattemptId { get; set; } = 0;
        public int QuizDetailId { get; set; } = 0;

        public DateTime QuizAttemptCreatedOn { get; set; }
     
        public string QuizTitle { get; set; }
        public int CompanyId { get; set; }
        public int ConfigurationDetailId { get; set; } = 0;
        public int WorkpackageInfoId  { get; set; } = 0;
        public DateTime? WorkpackageInfoCreatedOn { get; set; } 
        public string CompanyCode { get; set; } = string.Empty;
        public string QuizCode { get; set; } = string.Empty;
        public string LeadUserId { get; set; } = string.Empty;
        public int RecruiterUserId { get; set; } = 0;
      
        public string RequestMode { get; set; } = string.Empty;
        public int? RequestedQuestionType { get; set; } = (int)BranchingLogicEnum.QUESTION;

        public bool IsQuizStatNotcompleted { get; set; } = true;
    
        public bool IsQuesAndContentInSameTable { get; set; }
        public bool IsBranchingLogicEnabled { get; set; }
        public bool IsQuizResultinBranchingLogic { get; set; }
        public bool IsQuizAlreadyStarted { get; set; } = false;
        public bool IsLastQuestionAttempted { get; set; } = false;
        public bool IsLastQuestionStarted { get; set; } = false;
        public bool IsrevealScore { get; set; } = true;
        public string  RequestType { get; set; } = string.Empty;
        public List<Db.UserTokens> LeadOwners { get; set; }
        public QuizLeadInfo LeadDetails { get; set; } = new QuizLeadInfo();
        public Dictionary<string, object> ContactObject { get; set; }
        public CompanyModel CompanyDetails { get; set; } = new CompanyModel();

        public QuizDetailInfo QuizDetailInfo { get; set; } = new QuizDetailInfo();
        public int QuizType { get; set; }
        public string QuizAccessibleOfficeId { get; set; }
        public List<Response.UserMediaClassification> UserMediaClassifications { get; set; } =  new List<Response.UserMediaClassification>();
        public int? RequestUsageType { get; set; }

        public int RequestUserTypeId { get; set; }
        public int RequestBusinessUserId { get; set; } = 1;
        public string RequestQuizCode { get; set; }
        public List<Db.QuizVariables> QuizVariables { get; set; }
        public List<TextAnswer> RequestedTextAnswerList { get; set; }
        public int RequestQuestionId { get; set; } = -1;
        public List<int> RequestAnswerId { get; set; }
        public bool IsViewed { get; set; }
        public string  QuizPublishCode { get; set; }
        public string RequestId { get; set; }
        public List<Db.MediaVariablesDetails>  MediaVariableList { get; set; }

    }
    public class ContactBasicDetails
    {
        public string contactId { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string telephone { get; set; }
        public string SourceId { get; set; }
        public string SourceName { get; set; }
        public int ContactOwnerId { get; set; }
        public int SourceOwnerId { get; set; }
    }

    public class QuizDetailInfo
    {
        public string QuizCoverTitle { get; set; }
        public bool ShowQuizCoverTitle { get; set; }
        public bool AutoPlay { get; set; }
        public string  SecondsToApply { get; set; }
        public string QuizDescription { get; set; }
        public bool ShowDescription { get; set; }
        public bool EnableNextButton { get; set; }
        public int DisplayOrderForTitleImage { get; set; }
        public int DisplayOrderForTitle { get; set; }
        public int DisplayOrderForDescription { get; set; }
        public int DisplayOrderForNextButton { get; set; }

        public bool EnableMediaFile { get; set; }

        public string QuizCoverImage { get; set; }
        public bool ShowQuizCoverImage { get; set; }
        public string PublicIdForQuizCover { get; set; }
        public int? QuizCoverImgXCoordinate { get; set; }
        public int? QuizCoverImgYCoordinate { get; set; }
        public int? QuizCoverImgHeight { get; set; }
        public int? QuizCoverImgWidth { get; set; }
        public string QuizCoverImgAttributionLabel { get; set; }
        public string QuizCoverImgAltTag { get; set; }
        public bool? VideoFrameEnabled { get; set; }
        public string QuizStartButtonText { get; set; }
    }
     
    public class QuizLeadInfo
    {
        public string contactId { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string telephone { get; set; }
        public string SourceId { get; set; }
        public string SourceName { get; set; }
        public string  ContactOwnerId { get; set; }
        public string SourceOwnerId { get; set; }
        public string LeadOwnerId { get; set; }

        public int JRContactOwnerId { get; set; }
        public int JRSourceOwnerId { get; set; }
        public int JRLeadOwnerId { get; set; }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Token { get; set; }
        public string AccessToken { get; set; }
        public string ExternalUserId { get; set; }
 
    }
     
    public class QuizVariablesDetails
    {
        public int ObjectTypes { get; set; }
        public string Variables { get; set; }
        public int ObjectId { get; set; }
        public int QuizDetailsId { get; set; }
        public int CompanyId { get; set; }
    }

    public class FailedQuiz {
        public string LeadUserId { get; set; } = string.Empty;
        public string CompanyCode { get; set; } = string.Empty;
        public int QuizId { get; set; } = 0;
    }
}