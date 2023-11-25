using QuizApp.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QuizApp.Db
{
    public class QuizVariables
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int ObjectTypes { get; set; }
        [StringLength(4000)]
        public string Variables { get; set; }
        public int ObjectId { get; set; }
        public int QuizDetailsId { get; set; }
        public int CompanyId { get; set; } 
    }
}