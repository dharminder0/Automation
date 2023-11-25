using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizApp.Db
{
    public class TemplateParameterInConfigurationDetails
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string ParaName { get; set; }
        public int Position { get; set; }
        public string Value { get; set; }

        [ForeignKey("ConfigurationDetails")]
        public int ConfigurationDetailsId { get; set; }
        public virtual ConfigurationDetails ConfigurationDetails { get; set; }
    }
}