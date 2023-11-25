using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QuizApp.Db
{
    public class ReminderQueues
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ReminderLevel { get; set; }

        [ForeignKey("WorkPackageInfo")]
        public int WorkPackageInfoId { get; set; }
        public virtual WorkPackageInfo WorkPackageInfo { get; set; }

        [ForeignKey("RemindersInQuiz")]
        public int? ReminderInQuizId { get; set; }
        public virtual RemindersInQuiz RemindersInQuiz { get; set; }

        public int Type { get; set; }

        public DateTime QueuedOn { get; set; }

        public DateTime? SentOn { get; set; }

        public bool Sent { get; set; }
    }
}