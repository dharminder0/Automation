using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QuizApp.Db
{
    public class LinkedCalendarInAction
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("ActionsInQuiz")]
        public int ActionsInQuizId { get; set; }
        public virtual ActionsInQuiz ActionsInQuiz { get; set; }

        public int CalendarId { get; set; }
    }
}