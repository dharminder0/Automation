using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QuizApp.Db
{
    public class QuizObjectStats
    {
        public int Id { get; set; }

        [ForeignKey("QuizAttempts")]
        public int QuizAttemptId { get; set; }
        public virtual QuizAttempts QuizAttempts { get; set; }

        public int ObjectId { get; set; }
    
     
        public DateTime ViewedOn { get; set; }

        public int TypeId { get; set; }

        public int Status { get; set; }

    }
}