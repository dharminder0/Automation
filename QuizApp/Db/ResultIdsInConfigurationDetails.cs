using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QuizApp.Db
{
    public class ResultIdsInConfigurationDetails
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("QuizResults")]
        public int ResultId { get; set; }
        public virtual QuizResults QuizResults { get; set; }

        [ForeignKey("ConfigurationDetails")]
        public int? ConfigurationDetailsId { get; set; }
        public virtual ConfigurationDetails ConfigurationDetails { get; set; }

        public int FormId { get; set; }
        public int FlowOrder { get; set; }
    }
}