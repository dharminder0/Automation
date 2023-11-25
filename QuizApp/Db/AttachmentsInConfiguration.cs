using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizApp.Db
{
    public class AttachmentsInConfiguration
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string FileName { get; set; }

        public string FileIdentifier { get; set; }

        public string FileLink { get; set; }

        [ForeignKey("ConfigurationDetails")]
        public int? ConfigurationDetailsId { get; set; }
        public virtual ConfigurationDetails ConfigurationDetails { get; set; }
    }
}