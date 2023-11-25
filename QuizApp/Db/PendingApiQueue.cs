using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QuizApp.Db
{
    public class PendingApiQueue
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string RequestTypeURL { get; set; }

        public string Authorization { get; set; }

        public string RequestType { get; set; }

        public string RequestData { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}