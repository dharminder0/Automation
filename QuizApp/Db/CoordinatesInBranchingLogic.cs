using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QuizApp.Db
{
    public class CoordinatesInBranchingLogic
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int ObjectTypeId { get; set; }
        public int ObjectId { get; set; }
        public string XCoordinate { get; set; }
        public string YCoordinate { get; set; }
        public int? QuizId { get; set; }
        public int? CompanyId { get; set; }
    }
}