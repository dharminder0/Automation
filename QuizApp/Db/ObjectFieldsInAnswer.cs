using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizApp.Db
{
    public class ObjectFieldsInAnswer
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string ObjectName { get; set; }

        public string FieldName { get; set; }

        public string Value { get; set; }

        [ForeignKey("AnswerOptionsInQuizQuestions")]
        public int AnswerOptionsInQuizQuestionsId { get; set; }
        public virtual AnswerOptionsInQuizQuestions AnswerOptionsInQuizQuestions { get; set; }

        public DateTime CreatedOn { get; set; }

        public int CreatedBy { get; set; }

        public DateTime? LastUpdatedOn { get; set; }

        public int? LastUpdatedBy { get; set; }
        public bool? IsExternalSync { get; set; }
        public bool? IsCommentMapped { get; set; }
        public int QuestionId { get; set; }
    }
}