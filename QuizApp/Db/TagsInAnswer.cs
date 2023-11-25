using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QuizApp.Db
{
    public class TagsInAnswer
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("AnswerOptionsInQuizQuestions")]
        public int AnswerOptionsId { get; set; }
        public virtual AnswerOptionsInQuizQuestions AnswerOptionsInQuizQuestions { get; set; }

        public int TagId { get; set; }

        public int TagCategoryId { get; set; }
        
    }
}