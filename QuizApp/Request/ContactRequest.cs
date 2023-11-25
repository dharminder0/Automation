using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Request {
	public class ContactRequest {
		public string ClientCode { get; set; }
		public string ContactId { get; set; }
		public string Phone { get; set; }
		public string Email { get; set; }
	}
}