using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QuizApp.Db
{
    public class LeadDataInAction
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string LeadUserId { get; set; }

        [ForeignKey("ActionsInQuiz")]
        public int ActionId { get; set; }
        public virtual ActionsInQuiz ActionsInQuiz { get; set; }
        public int? AppointmentTypeId { get; set; }
        public string ReportEmails { get; set; }
        public bool IsUpdatedSend { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string SMSText { get; set; }

        [ForeignKey("ConfigurationDetails")]
        public int? ConfigurationDetailsId { get; set; }
        public virtual ConfigurationDetails ConfigurationDetails { get; set; }

        public virtual ICollection<LeadCalendarDataInAction> LeadCalendarDataInAction { get; set; }
    }
}