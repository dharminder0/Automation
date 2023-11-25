using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QuizApp.Db
{
    public class QuizAnswerStats
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("AnswerOptionsInQuizQuestions")]
        public int AnswerId { get; set; }
        public virtual AnswerOptionsInQuizQuestions AnswerOptionsInQuizQuestions { get; set; }

        [ForeignKey("QuizQuestionStats")]
        public int QuizQuestionStatsId { get; set; }
        public virtual QuizQuestionStats QuizQuestionStats { get; set; }

        public string AnswerText { get; set; }

        public int? SubAnswerTypeId { get; set; }

        public string Comment { get; set; }
        public string AnswerSecondaryText { get; set; }
        public int CompanyId { get; set; }
        public int QuizAttemptId { get; set; }
    }
}