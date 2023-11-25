using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QuizApp.Db
{
    public class QuizStats
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("QuizAttempts")]
        public int QuizAttemptId { get; set; }
        public virtual QuizAttempts QuizAttempts { get; set; }

        public DateTime StartedOn { get; set; }

        public DateTime? CompletedOn { get; set; }

        [ForeignKey("QuizResults")]
        public int? ResultId { get; set; }
        public byte? AttemptStatus { get; set; }
        public virtual QuizResults QuizResults { get; set; }
    }
}