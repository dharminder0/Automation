using QuizApp.Helpers;
using QuizApp.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizApp.Services.Service {
	public interface ICommunicationService {
		ResultEnum Status { get; set; }
		string ErrorMessage { get; set; }
		bool UpdateCommunicationStatus(CommunicationRequest communicationRequest);
	}
}
