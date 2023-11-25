using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QuizApp.Db
{
    public class ApiUsageLogs
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string Body { get; set; }
        public string Url { get; set; }
        public DateTime RequestDate { get; set; }
        public string Response { get; set; }
    }
}