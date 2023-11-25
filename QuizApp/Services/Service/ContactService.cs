using QuizApp.Db;
using QuizApp.Helpers;
using QuizApp.RepositoryPattern;
using QuizApp.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Services.Service {
	public class ContactService : IContactService {
		private ResultEnum status = ResultEnum.Ok;
		private string errormessage = string.Empty;
		public ResultEnum Status { get { return status; } set { status = value; } }
		public string ErrorMessage { get { return errormessage; } set { errormessage = value; } }
		public void DeleteContactData(ContactRequest contactRequest) {
			if(contactRequest == null) {
				return ;
			}
			if (!string.IsNullOrWhiteSpace(contactRequest.ContactId)) {
				try {
					using (var UOWObj = new AutomationUnitOfWork()) {

						if (!string.IsNullOrWhiteSpace(contactRequest.ClientCode)) {
							var companyObj = UOWObj.CompanyRepository.Get(v => v.ClientCode == contactRequest.ClientCode).FirstOrDefault();
							if(companyObj != null && companyObj.CompanyId > 0) {
								var QuizAttemptObj = UOWObj.QuizAttemptsRepository.Get(r => r.CompanyId == companyObj.Id && r.LeadUserId == contactRequest.ContactId);
								if(QuizAttemptObj != null) {
									foreach(var item in QuizAttemptObj) {
										DeleteAttempt(item.Id);
									}

								}
							}
						}
						var workPackaheInfoObj = UOWObj.WorkPackageInfoRepository.Get(r => r.LeadUserId == contactRequest.ContactId);
						if (workPackaheInfoObj != null) {
							foreach (var item in workPackaheInfoObj) {
								DeleteWorkPackageInfo(item.Id);

                            }
                        }

                        if (!string.IsNullOrWhiteSpace(contactRequest.ClientCode)) {

							var whatsappLoggingObj = UOWObj.WhatsappLoggingRepository.Get(r => r.ContactId == contactRequest.ContactId && r.ClientCode == contactRequest.ClientCode);
							if (whatsappLoggingObj != null) {
								foreach(var item in whatsappLoggingObj) {
									UOWObj.WhatsappLoggingRepository.Delete(item);
									UOWObj.Save();
								}
							}
						}

					}

					DeletePedingApiQueueLog(contactRequest.ContactId, contactRequest.Phone, contactRequest.Email);
					DeleteApiUsageLogs(contactRequest.ContactId, contactRequest.Phone, contactRequest.Email);

				} catch (Exception ex) {}

			}			
		}

		private void DeletePedingApiQueueLog(string contactId, string phone, string email) {
			phone = phone ?? contactId;
			phone = phone.Replace("+31", "");
			email = email ?? contactId;
			using (var UOWObj = new AutomationUnitOfWork()) {

				string query = $@"DELETE FROM PendingApiQueue where (RequestData like '%{contactId}%' OR RequestData like '%{phone}%' OR RequestData like '%{email}%')";
				UOWObj.PendingApiQueueRepository.DeleteRange(query);
				UOWObj.Save();
			}
		}

		private void DeleteApiUsageLogs(string contactId, string phone, string email) {
			phone = phone ?? contactId;
			phone = phone.Replace("+31", "");
			email = email ?? contactId;
			using (var UOWObj = new AutomationUnitOfWork()) {

				string query = $@"DELETE FROM ApiUsageLogs where (Body like '%{contactId}%' OR Body like '%{phone}%' OR Body like '%{email}%')";
				UOWObj.ApiUsageLogsRepository.DeleteRange(query);
				UOWObj.Save();
			}
		}

		private void DeleteAttempt(int attemptId) {
			using (var UOWObj = new AutomationUnitOfWork()) {
				var query = $@" 
                delete from QuizQuestionStats where QuizAttemptId = {attemptId};
                delete from QuizObjectStats where QuizAttemptId = {attemptId};
				delete from QuizStats where QuizAttemptId = {attemptId};
				delete from QuizAnswerStats where QuizAttemptId = {attemptId};
				delete from AttemptQuizLog where QuizAttemptId = {attemptId};
                delete from QuizAttempts where Id = {attemptId};
";
				UOWObj.QuizAttemptsRepository.DeleteRange(query);
				UOWObj.Save();
			}
		}

		private void DeleteWorkPackageInfo(int workpackageInfoId) {
			using (var UOWObj = new AutomationUnitOfWork()) {
				var query = $@"delete from WorkPackageInfo where Id = {workpackageInfoId};";
				UOWObj.WorkPackageInfoRepository.DeleteRange(query);
				UOWObj.Save();
			}

		}
	}
}