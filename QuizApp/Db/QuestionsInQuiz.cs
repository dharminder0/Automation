using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QuizApp.Db
{
    public class QuestionsInQuiz
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("QuizDetails")]
        public int QuizId { get; set; }
        public virtual QuizDetails QuizDetails { get; set; }

        public string Question { get; set; }

        public bool ShowTitle { get; set; }

        public string QuestionImage { get; set; }

        public bool EnableMediaFile { get; set; }

        public string PublicId { get; set; }

        public bool? ShowAnswerImage { get; set; }

        public string CorrectAnswerDescription { get; set; }

        public bool? RevealCorrectAnswer { get; set; }

        public string AliasTextForCorrect { get; set; }

        public string AliasTextForIncorrect { get; set; }

        public string AliasTextForYourAnswer { get; set; }

        public string AliasTextForCorrectAnswer { get; set; }

        public string AliasTextForExplanation { get; set; }

        public string AliasTextForNextButton { get; set; }

        [StringLength(50)]
        public string NextButtonText { get; set; }

        [StringLength(50)]
        public string NextButtonTxtSize { get; set; }

        [StringLength(50)]
        public string NextButtonTxtColor { get; set; }

        [StringLength(50)]
        public string NextButtonColor { get; set; }

        public bool EnableNextButton { get; set; }

        public bool? ShowQuestionImage { get; set; }

        public int State { get; set; }
        
        public int DisplayOrder { get; set; }

        public int? Status { get; set; }

        public DateTime? LastUpdatedOn { get; set; }

        public int? LastUpdatedBy { get; set; }

        public int AnswerType { get; set; }

        public int? MaxAnswer { get; set; }

        public int? MinAnswer { get; set; }

        public bool ViewPreviousQuestion { get; set; }

        public bool EditAnswer { get; set; }
        
        public bool TimerRequired { get; set; }
        
        public string Time{ get; set; }

        public bool AutoPlay { get; set; }

        public bool? VideoFrameEnabled { get; set; }

        public bool? DescVideoFrameEnabled { get; set; }
        public int? AnswerStructureType { get; set; }
        public string SecondsToApply { get; set; }

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

        public virtual ICollection<AnswerOptionsInQuizQuestions> AnswerOptionsInQuizQuestions { get; set; }      

        public virtual ICollection<QuizQuestionStats> QuizQuestionStats { get; set; }
        //public int? LanguageId { get; set; }
        public string LanguageCode { get; set; }
        public int? TemplateId { get; set; }
        public bool IsMultiRating { get; set; }
    }
}