using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QuizApp.Db {
	public class FieldSyncSetting {
		 [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string FieldType { get; set; }

        public string FieldOptionName { get; set; }

        public string FieldOptionTitle { get; set; }

        public string FieldFormula { get; set; }      
    }
}