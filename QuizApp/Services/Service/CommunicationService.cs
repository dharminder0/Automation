using Core.Common.Extensions;
using QuizApp.Db;
using QuizApp.Helpers;
using QuizApp.RepositoryPattern;
using QuizApp.Request;
using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Services.Service {
	public partial class CommunicationService : ICommunicationService {

		private ResultEnum status = ResultEnum.Ok;
		private string errormessage = string.Empty;
		public ResultEnum Status { get { return status; } set { status = value; } }
		public string ErrorMessage { get { return errormessage; } set { errormessage = value; } }
		private readonly IQuizAttemptService _quizAttemptService;
		public CommunicationService(IQuizAttemptService quizAttemptService) {
			_quizAttemptService = quizAttemptService;
		}
		public bool UpdateCommunicationStatus(CommunicationRequest communicationRequest) {
			bool updateStatus = false;
			WorkPackageInfo WorkPackageInfoObj = null;
				AddWhatsappLog(communicationRequest);
			
			
			if (string.IsNullOrWhiteSpace(communicationRequest.CommunicationType) || string.IsNullOrWhiteSpace(communicationRequest.ObjectId)) {
				return false; 
			}
			if (!communicationRequest.ModuleName.EqualsCI("automation")) {
				return false;
			}


			try {				
				using (var UOWObj = new AutomationUnitOfWork()) {
					if (communicationRequest != null)
						{
						var id = Int32.Parse(communicationRequest.ObjectId); 
						if(id > 0) {
							WorkPackageInfoObj = UOWObj.WorkPackageInfoRepository.Get(v => v.Id == id).FirstOrDefault();
						}						 
						if (WorkPackageInfoObj != null) {

							var communicationType = (CommunicationTypeEnum)Enum.Parse(typeof(CommunicationTypeEnum), communicationRequest.CommunicationType, ignoreCase: true);
							switch (communicationType) {
								case CommunicationTypeEnum.Whatsapp: {
										WorkPackageInfoObj.WhatsappSentOn = DateTime.UtcNow;
										if (communicationRequest.Status.EqualsCI("rejected") || communicationRequest.Status.EqualsCI("failed")) {
											WorkPackageInfoObj.WhatsappSentOn = null;
											FailedQuiz failedQuiz = new FailedQuiz() {
											LeadUserId = communicationRequest.ContactId,
											CompanyCode = communicationRequest.ClientCode,	
											QuizId = WorkPackageInfoObj.QuizId,
											};
											

											var value = _quizAttemptService.FailedQuiz(failedQuiz);

										}

										UOWObj.WorkPackageInfoRepository.Update(WorkPackageInfoObj);

										updateStatus = true;
										break;
								}
								case CommunicationTypeEnum.Email: {
										WorkPackageInfoObj.EmailSentOn = DateTime.UtcNow;
										if (communicationRequest.Status.EqualsCI("rejected") || communicationRequest.Status.EqualsCI("failed")) {
											WorkPackageInfoObj.EmailSentOn = null;
										}

										UOWObj.WorkPackageInfoRepository.Update(WorkPackageInfoObj);
										updateStatus = true;
										break;
									}
								case CommunicationTypeEnum.SMS: {
										WorkPackageInfoObj.SMSSentOn = DateTime.UtcNow;
										if (communicationRequest.Status.EqualsCI("rejected") || communicationRequest.Status.EqualsCI("failed")) {
											WorkPackageInfoObj.SMSSentOn = null;
										}

										UOWObj.WorkPackageInfoRepository.Update(WorkPackageInfoObj);

										updateStatus = true;
										break;
									}
								default:
									break;
							}
							UOWObj.Save();
							return updateStatus;
						}
					}
					else {
						return updateStatus;
					}
				}
			}
			catch(Exception ex) { }
			return updateStatus;	
		}

		public void AddWhatsappLog(CommunicationRequest communicationRequest) {
			try {
				using (var UOWObj = new AutomationUnitOfWork()) {
					if (communicationRequest != null) {
						var loggingObj = new WhatsappLogging {
							ClientCode = communicationRequest.ClientCode,
							ContactId = communicationRequest.ContactId,
							ContactPhone = communicationRequest.ContactPhone,
							ModuleName = communicationRequest.ModuleName,
							EventType = communicationRequest.ObjectType,
							ObjectId = communicationRequest.ObjectId,
							UniqueCode = communicationRequest.UniqueCode,
							Status = communicationRequest.Status,
							CommunicationType = communicationRequest.CommunicationType,
							ErrorMessage = communicationRequest.Error
						};

						UOWObj.WhatsappLoggingRepository.Insert(loggingObj);
						UOWObj.Save();
					}
				}
			} catch(Exception ex) { }
			
		}

	}
	
}