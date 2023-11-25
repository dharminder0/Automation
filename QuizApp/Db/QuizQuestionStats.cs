using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QuizApp.Db
{
    public class QuizQuestionStats
    {
        public int Id { get; set; }

        [ForeignKey("QuizAttempts")]
        public int QuizAttemptId { get; set; }
        public virtual QuizAttempts QuizAttempts { get; set; }

        [ForeignKey("QuestionsInQuiz")]
        public int QuestionId { get; set; }
        public virtual QuestionsInQuiz QuestionsInQuiz { get; set; }

        public DateTime StartedOn { get; set; }

        public DateTime? CompletedOn { get; set; }

        public int Status { get; set; }

        public virtual ICollection<QuizAnswerStats> QuizAnswerStats { get; set; }
    }
}