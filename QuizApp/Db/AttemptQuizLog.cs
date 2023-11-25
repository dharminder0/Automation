using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QuizApp.Db
{
    public class AttemptQuizLog
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int QuizId { get; set; }
        public string LeadId { get; set; }
        public int QuizAttemptId { get; set; }
        public string RequestJson { get; set; }
        public string ResponseJson { get; set; }
        public DateTime CreatedOn { get; set; }

    }
}