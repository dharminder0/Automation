using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Request
{
    public class DuplicateQuizRequest
    {
        public int CurrentQuizId { get; set; }
        public string DestinationCompanyCode { get; set; }
        public string DestinationOfficeId { get; set; } = "";
        public int DestinationBusinessUserId { get; set; }

    }
}