using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QuizApp.Db
{
    public class PersonalityAnswerResultMapping
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("AnswerOptionsInQuizQuestions")]
        public int AnswerId { get; set; }
        public virtual AnswerOptionsInQuizQuestions AnswerOptionsInQuizQuestions { get; set; }

        [ForeignKey("QuizResults")]
        public int ResultId { get; set; }
        public virtual QuizResults QuizResults { get; set; }
    }
}