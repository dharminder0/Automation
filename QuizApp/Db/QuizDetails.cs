using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QuizApp.Db
{
    public class QuizDetails
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("Quiz")]
        public int ParentQuizId { get; set; }
        public virtual Quiz Quiz { get; set; }

        [StringLength(500)]
        public string QuizCoverTitle { get; set; }

        public bool ShowQuizCoverTitle { get; set; }

        [StringLength(200)]
        public string QuizTitle { get; set; }

        [StringLength(2000)]
        public string QuizCoverImage { get; set; }

        public bool ShowQuizCoverImage { get; set; }

        public bool EnableMediaFile { get; set; }

        public string PublicId { get; set; }

        public int? QuizCoverImgXCoordinate { get; set; }

        public int? QuizCoverImgYCoordinate { get; set; }

        public int? QuizCoverImgHeight { get; set; }

        public int? QuizCoverImgWidth { get; set; }

        public string QuizCoverImgAttributionLabel { get; set; }

        public string QuizCoverImgAltTag { get; set; }

        [StringLength(1000)]
        public string QuizDescription { get; set; }

        public bool ShowDescription { get; set; }

        [StringLength(100)]
        public string StartButtonText { get; set; }

        public bool EnableNextButton { get; set; }

        public bool? IsBranchingLogicEnabled { get; set; }

        public bool? HideSocialShareButtons { get; set; }

        public bool? EnableFacebookShare { get; set; }

        public bool? EnableTwitterShare { get; set; }

        public bool? EnableLinkedinShare { get; set; }

        public int State { get; set; }
            
        public int Status { get; set; }

        [DefaultValue(1)]
        public int Version { get; set; }

        public DateTime CreatedOn { get; set; }

        public int CreatedBy { get; set; }

        public DateTime? LastUpdatedOn { get; set; }

        public int? LastUpdatedBy { get; set; }

        public bool ViewPreviousQuestion { get; set; }

        public bool EditAnswer { get; set; }

        public bool ApplyToAll { get; set; }

        public bool AutoPlay { get; set; }

        public int DisplayOrderForTitle { get; set; }

        public int DisplayOrderForTitleImage { get; set; }

        public int DisplayOrderForDescription { get; set; }

        public int DisplayOrderForNextButton { get; set; }

        public string SecondsToApply { get; set; }

        [DefaultValue(0)]
        public bool? VideoFrameEnabled { get; set; }

        public int? CompanyId { get; set; }
        public virtual ICollection<QuizBrandingAndStyle> QuizBrandingAndStyle { get; set; }

        public virtual ICollection<QuestionsInQuiz> QuestionsInQuiz { get; set; }

        public virtual ICollection<QuizResults> QuizResults { get; set; }

        public virtual ICollection<ResultSettings> ResultSettings { get; set; }

        public virtual ICollection<QuizAttempts> QuizAttempts { get; set; }

        public virtual ICollection<ActionsInQuiz> ActionsInQuiz { get; set; }

        public virtual ICollection<ContentsInQuiz> ContentsInQuiz { get; set; }
        public virtual ICollection<BadgesInQuiz> BadgesInQuiz { get; set; }
        public virtual ICollection<BranchingLogic> BranchingLogic { get; set; }
        public virtual ICollection<QuizComponentLogs> QuizComponentLogs { get; set; }
        public virtual ICollection<VariableInQuiz> VariableInQuiz { get; set; }
        public virtual ICollection<PersonalityResultSetting> PersonalityResultSetting { get; set; }
        public virtual ICollection<MediaVariablesDetails> MediaVariablesDetails { get; set; }
    }
}