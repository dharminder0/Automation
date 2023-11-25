using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Request
{
    public class OWCEmailRequest
    {
        public string to { get; set; }
        public string subject { get; set; }
        public string body { get; set; }
    }

    public class OWCSMSRequest
    {
        public string virtualNumber { get; set; }
        public string recipientNumber { get; set; }
        public string message { get; set; }
        public string clientName { get; set; }
        public string clientCode { get; set; }
    }
}