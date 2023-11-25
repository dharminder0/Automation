using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QuizApp.Db {
	public class WhatsappLogging {
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public string ClientCode { get; set; }
		public string ContactId { get; set; }
		public string ContactPhone { get; set; }
		public string ModuleName { get; set; }
		public string EventType { get; set; }
		public string ObjectId { get; set; }
		public string UniqueCode { get; set; }
		public string Status { get; set; }
		public string CommunicationType { get; set; }
		public string ErrorMessage { get; set; }
	}
}