using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizApp.Db
{
    public class BadgesInQuiz
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("QuizDetails")]
        public int QuizId { get; set; }
        public virtual QuizDetails QuizDetails { get; set; }

        public string Title { get; set; }

        public bool ShowTitle { get; set; }

        public string Image { get; set; }

        public bool ShowImage { get; set; }

        public bool EnableMediaFile { get; set; }

        public string PublicId { get; set; }

        public string AliasTextForNextButton { get; set; }

        public bool AutoPlay { get; set; }

        public string SecondsToApply { get; set; }
        public bool? VideoFrameEnabled { get; set; }

        public int DisplayOrderForTitle { get; set; }

        public int DisplayOrderForTitleImage { get; set; }

        public int State { get; set; }

        public int? Status { get; set; }

        public DateTime? LastUpdatedOn { get; set; }

        public int? LastUpdatedBy { get; set; }        
    }
}