using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QuizApp.Db
{
    public class MediaVariablesDetails
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ObjectId { get; set; }

        public int ObjectTypeId { get; set; }

        public string ObjectValue { get; set; }

        public string ObjectPublicId { get; set; }

        public int Type { get; set; }

        public string MediaOwner { get; set; }

        public string ProfileMedia { get; set; }

        [ForeignKey("ConfigurationDetails")]
        public int? ConfigurationDetailsId { get; set; }
        public virtual ConfigurationDetails ConfigurationDetails { get; set; }

        [ForeignKey("QuizDetails")]
        public int QuizId { get; set; }
        public virtual QuizDetails QuizDetails { get; set; }
    }
}