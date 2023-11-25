using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QuizApp.Db
{
    public class QuizAttempts
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string LeadUserId { get; set; }

        public int? RecruiterUserId { get; set; }

        [ForeignKey("QuizDetails")]
        public int QuizId { get; set; }
        public virtual QuizDetails QuizDetails { get; set; }

        public DateTime Date { get; set; }

        public string Code { get; set; }

        public string CampaignName { get; set; }

        public string Mode { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? LastUpdatedOn { get; set; }

        [DefaultValue(false)]
        public bool IsViewed { get; set; }

        public DateTime? LeadConvertedOn { get; set; }

        [ForeignKey("WorkPackageInfo")]
        public int? WorkPackageInfoId { get; set; }
        public virtual WorkPackageInfo WorkPackageInfo { get; set; }

        [ForeignKey("ConfigurationDetails")]
        public int? ConfigurationDetailsId { get; set; }
        public virtual ConfigurationDetails ConfigurationDetails { get; set; }

        [ForeignKey("Company")]
        public int? CompanyId { get; set; }
        public string SourceId { get; set; }
        public virtual Company Company { get; set; }

        public virtual ICollection<QuizStats> QuizStats { get; set; }

        public virtual ICollection<QuizQuestionStats> QuizQuestionStats { get; set; }
        public virtual ICollection<QuizObjectStats> QuizObjectStats { get; set; }
    }
}