using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QuizApp.Db
{
    public class LeadCalendarDataInAction
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("LeadDataInAction")]
        public int LeadDataInActionId { get; set; }
        public virtual LeadDataInAction LeadDataInAction { get; set; }

        public int CalendarId { get; set; }
    }
}