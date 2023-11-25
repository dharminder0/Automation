using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Services.Model
{
    public class QuizVariableModel 
    {
        public int Id { get; set; }
        public int ObjectTypes { get; set; }
        public string Variables { get; set; }
        public int ObjectId { get; set; }
        public int QuizDetailsId { get; set; }
        public int CompanyId { get; set; }

    }
}