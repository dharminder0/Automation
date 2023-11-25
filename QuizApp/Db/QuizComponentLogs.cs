using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QuizApp.Db
{
    public class QuizComponentLogs
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("QuizDetails")]
        public int QuizId { get; set; }
        public virtual QuizDetails QuizDetails { get; set; }
        public int DraftedObjectId { get; set; }
        public int PublishedObjectId { get; set; }
        public int ObjectTypeId { get; set; }
    }
}