using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QuizApp.Db
{
    public class ExternalActionQueue
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public DateTime AddedOn { get; set; }
        public int CompanyId { get; set; }
        public string ObjectId { get; set; }
        public string ItemType { get; set; }
        public string ObjectJson { get; set; }
        public int Status { get; set; }
        public DateTime? ModifiedOn { get; set; }

    }
}