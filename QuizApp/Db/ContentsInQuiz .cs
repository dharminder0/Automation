using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QuizApp.Db
{
    public class ContentsInQuiz
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("QuizDetails")]
        public int QuizId { get; set; }
        public virtual QuizDetails QuizDetails { get; set; }

        public string ContentTitle { get; set; }

        public bool ShowTitle { get; set; }

        public string ContentTitleImage { get; set; }

        public bool EnableMediaFileForTitle { get; set; }

        public string PublicIdForContentTitle { get; set; }

        public bool? ShowContentTitleImage { get; set; }

        public string ContentDescription { get; set; }

        public bool ShowDescription { get; set; }

        public string ContentDescriptionImage { get; set; }

        public bool EnableMediaFileForDescription { get; set; }

        public string PublicIdForContentDescription { get; set; }

        public bool? ShowContentDescriptionImage { get; set; }

        public string AliasTextForNextButton { get; set; }

        public bool EnableNextButton { get; set; }

        public int State { get; set; }

        public int DisplayOrder { get; set; }

        public int? Status { get; set; }

        public bool ViewPreviousQuestion { get; set; }

        public bool AutoPlay { get; set; }

        public string SecondsToApply { get; set; }
        public bool? VideoFrameEnabled { get; set; }

        public bool AutoPlayForDescription { get; set; }

        public string SecondsToApplyForDescription { get; set; }
        public bool? DescVideoFrameEnabled { get; set; }

        public int DisplayOrderForTitle { get; set; }

        public int DisplayOrderForTitleImage { get; set; }

        public int DisplayOrderForDescription { get; set; }

        public int DisplayOrderForDescriptionImage { get; set; }

        public int DisplayOrderForNextButton { get; set; }

        public DateTime? LastUpdatedOn { get; set; }

        public int? LastUpdatedBy { get; set; }
    }
}