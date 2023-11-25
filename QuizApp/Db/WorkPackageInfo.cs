using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QuizApp.Db
{
    public class WorkPackageInfo
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string LeadUserId { get; set; }

        [ForeignKey("Quiz")]
        public int QuizId { get; set; }
        public virtual Quiz Quiz { get; set; }

        public int? BusinessUserId { get; set; }

        public string CampaignId { get; set; }

        [StringLength(1000)]
        public string CampaignName { get; set; }

        public int Status { get; set; }

        public DateTime? CreatedOn { get; set; }

        public string ShotenUrlCode { get; set; }
        
        public string EmailLinkVariableForInvitation { get; set; }

        public bool IsOurEndLogicForInvitation { get; set; }

        public string ShotenUrlCodeForFirstReminder { get; set; }
        
        public string EmailLinkVariableForFirstReminder { get; set; }

        public bool IsOurEndLogicForFirstReminder { get; set; }

        public string ShotenUrlCodeForSecondReminder { get; set; }
        
        public string EmailLinkVariableForSecondReminder { get; set; }

        public bool IsOurEndLogicForSecondReminder { get; set; }

        public string ShotenUrlCodeForThirdReminder { get; set; }
        
        public string EmailLinkVariableForThirdReminder { get; set; }

        public bool IsOurEndLogicForThirdReminder { get; set; }

        public DateTime? EmailSentOn { get; set; }

        public DateTime? SMSSentOn { get; set; }

        public DateTime? WhatsappSentOn { get; set; }
        public string RequestId { get; set; }

        [ForeignKey("ConfigurationDetails")]
        public int? ConfigurationDetailsId { get; set; }
        public virtual ConfigurationDetails ConfigurationDetails { get; set; }

        public virtual ICollection<QuizAttempts> QuizAttempts { get; set; }
        public virtual ICollection<ReminderQueues> ReminderQueues { get; set; }
    }
}