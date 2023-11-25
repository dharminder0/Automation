using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QuizApp.Db
{
    public class PersonalityResultSetting
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("QuizDetails")]
        public int QuizId { get; set; }
        public virtual QuizDetails QuizDetails { get; set; }

        public string Title { get; set; }
        public int Status { get; set; }
        public int MaxResult { get; set; }
        public string GraphColor { get; set; }
        public string ButtonColor { get; set; }
        public string ButtonFontColor { get; set; }
        public string SideButtonText { get; set; }
        public bool IsFullWidthEnable { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public int? LastUpdatedBy { get; set; }
        public bool ShowLeadUserForm { get; set; }
    }
}