using QuizApp.Helpers;
using QuizApp.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Services.Service {
	public interface IContactService {
		ResultEnum Status { get; set; }
		string ErrorMessage { get; set; }
		void DeleteContactData(ContactRequest contactRequest);
	}
}