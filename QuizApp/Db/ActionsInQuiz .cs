using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizApp.Db
{
    public class ActionsInQuiz
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("QuizDetails")]
        public int QuizId { get; set; }
        public virtual QuizDetails QuizDetails { get; set; }

        public string Title { get; set; }

        public int? AppointmentId { get; set; }

        public string ReportEmails { get; set; }

        public int? AutomationId { get; set; }

        public int ActionType { get; set; }

        public int State { get; set; }    
          
        public int? Status { get; set; }

        public DateTime? LastUpdatedOn { get; set; }

        public int? LastUpdatedBy { get; set; }

        public virtual ICollection<LeadDataInAction> LeadDataInAction { get; set; }

        public virtual ICollection<LinkedCalendarInAction> LinkedCalendarInAction { get; set; }
    }
}