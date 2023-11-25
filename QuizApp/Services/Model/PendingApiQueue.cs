using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Services.Model
{
    public class PendingApiQueueModel
    {
        public int Id { get; set; }
        public string RequestType { get; set; }
        public string RequestData { get; set; }
        public int? CompanyId { get; set; }
    }
}