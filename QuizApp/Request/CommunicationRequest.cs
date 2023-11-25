using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Request {
	public class CommunicationRequest {
		public string ClientCode { get; set; }
		public string ContactId { get; set; }
		public string ContactPhone { get; set; }
		public string ModuleName { get; set; }
        public string ObjectType { get; set; }
        public string  ObjectId { get; set; }
        public string UniqueCode   { get; set; }
		public string CommunicationType { get; set; }
		public string Status { get; set; }
		public string Error { get; set; }
	}
}
