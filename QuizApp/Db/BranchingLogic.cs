using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QuizApp.Db
{
    public class BranchingLogic
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("QuizDetails")]
        public int QuizId { get; set; }
        public virtual QuizDetails QuizDetails { get; set; }
        public int SourceTypeId { get; set; }
        public int SourceObjectId { get; set; }
        public int? DestinationTypeId { get; set; }
        public int? DestinationObjectId { get; set; }
        public bool IsStartingPoint { get; set; }
        public bool IsEndPoint { get; set; }
    }
}