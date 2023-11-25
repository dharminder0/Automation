using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QuizApp.Db
{
    public class ResultSettings
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [ForeignKey("QuizDetails")]
        public int QuizId { get; set; }        
        public virtual QuizDetails QuizDetails { get; set; }

        public bool? ShowScoreValue { get; set; }

        public bool? ShowCorrectAnswer { get; set; }

        public int? MinScore { get; set; }

        public int? MaxScore { get; set; }

        public string CustomTxtForScoreValueInResult { get; set; }

        [StringLength(100)]
        public string CustomTxtForAnswerKey { get; set; }

        [StringLength(100)]
        public string CustomTxtForYourAnswer { get; set; }

        [StringLength(100)]
        public string CustomTxtForCorrectAnswer { get; set; }

        [StringLength(100)]
        public string CustomTxtForExplanation { get; set; }

        public int State { get; set; }

        public DateTime? LastUpdatedOn { get; set; }

        public int? LastUpdatedBy { get; set; }
    }
}