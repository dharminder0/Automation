using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QuizApp.Db
{
    public class QuizResults
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("QuizDetails")]
        public int QuizId { get; set; }
        public virtual QuizDetails QuizDetails { get; set; }

        [StringLength(500)]
        public string Title { get; set; }

        public bool ShowExternalTitle { get; set; }

        [StringLength(200)]
        public string InternalTitle { get; set; }

        public bool ShowInternalTitle { get; set; }

        [StringLength(2000)]
        public string Image { get; set; }

        public bool EnableMediaFile { get; set; }

        public string PublicId { get; set; }

        [StringLength(2000)]
        public string Description { get; set; }

        public bool ShowDescription { get; set; }

        public bool ShowResultImage { get; set; }

        public bool? HideCallToAction { get; set; }

        [StringLength(2000)]
        public string ActionButtonURL { get; set; }

        public bool? OpenLinkInNewTab { get; set; }

        [StringLength(20)]
        public string ActionButtonTxtSize { get; set; }

        [StringLength(50)]
        public string ActionButtonTitleColor { get; set; }

        [StringLength(50)]
        public string ActionButtonColor { get; set; }

        [StringLength(50)]
        public string ActionButtonText { get; set; }

        public int State { get; set; }

        public int? Status { get; set; }

        public bool? IsRedirectOn { get; set; }

        public string RedirectResultTo { get; set; }

        public DateTime? LastUpdatedOn { get; set; }

        public int? LastUpdatedBy { get; set; }

        public int? MinScore { get; set; }

        public int? MaxScore { get; set; }
      
        public int DisplayOrder { get; set; }

        public bool IsPersonalityCorrelatedResult { get; set; }

        public bool ShowLeadUserForm { get; set; }

        public bool AutoPlay { get; set; }
        public bool? VideoFrameEnabled { get; set; }

        public string SecondsToApply { get; set; }

        public int DisplayOrderForTitle { get; set; }

        public int DisplayOrderForTitleImage { get; set; }

        public int DisplayOrderForDescription { get; set; }

        public int DisplayOrderForNextButton { get; set; }

        public virtual ICollection<QuizStats> QuizStats { get; set; }

        public virtual ICollection<PersonalityAnswerResultMapping> PersonalityAnswerResultMapping { get; set; }
        public virtual ICollection<ResultIdsInConfigurationDetails> ResultIdsInConfigurationDetails { get; set; }
    }
}