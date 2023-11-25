using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QuizApp.Db
{
    public class AnswerOptionsInQuizQuestions
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("QuestionsInQuiz")]
        public int QuestionId { get; set; }
        public virtual QuestionsInQuiz QuestionsInQuiz { get; set; }

        [StringLength(2000)]
        public string Option { get; set; }

        [StringLength(2000)]
        public string OptionImage { get; set; }

        public bool EnableMediaFile { get; set; }

        public string PublicId { get; set; }
   
        public int? AssociatedScore { get; set; }

        public bool? IsCorrectAnswer { get; set; }

        public bool? IsCorrectForMultipleAnswer { get; set; }

        public int DisplayOrder { get; set; }

        public int State { get; set; }

        public DateTime? LastUpdatedOn { get; set; }

        public int? LastUpdatedBy { get; set; }

        public int? Status { get; set; }

        public bool IsReadOnly { get; set; }

        public bool IsUnansweredType { get; set; }

        public bool AutoPlay { get; set; }

        public string SecondsToApply { get; set; }
        public string SecondsToApplyForDescription { get; set; }
        public bool? VideoFrameEnabled { get; set; }
        public bool? DescVideoFrameEnabled { get; set; }
        public string OptionTextforRatingOne { get; set; }

        public string OptionTextforRatingTwo { get; set; }

        public string OptionTextforRatingThree { get; set; }

        public string OptionTextforRatingFour { get; set; }

        public string OptionTextforRatingFive { get; set; }
        public string ListValues { get; set; }
        public int? QuizId { get; set; }
        public string Description { get; set; }

        public virtual ICollection<QuizAnswerStats> QuizAnswerStats { get; set; }

        public virtual ICollection<TagsInAnswer> TagsInAnswer { get; set; }

        public virtual ICollection<PersonalityAnswerResultMapping> PersonalityAnswerResultMapping { get; set; }

        public virtual ICollection<ObjectFieldsInAnswer> ObjectFieldsInAnswer { get; set; }
        public int? RefId { get; set; }

    }
}