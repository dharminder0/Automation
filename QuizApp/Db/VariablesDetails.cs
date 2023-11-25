using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QuizApp.Db
{
    public class VariablesDetails
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string LeadId { get; set; }

        [ForeignKey("VariableInQuiz")]
        public int VariableInQuizId { get; set; }
        public virtual VariableInQuiz VariableInQuiz { get; set; }
        public string VariableValue { get; set; }

        [ForeignKey("ConfigurationDetails")]
        public int? ConfigurationDetailsId { get; set; }
        public virtual ConfigurationDetails ConfigurationDetails { get; set; }
    }
}