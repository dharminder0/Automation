using QuizApp.Helpers;
using QuizApp.RepositoryExtensions;
using QuizApp.RepositoryPattern;
using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace QuizApp.Services.Service {
    public class UncompleteQuizService : IUncompleteQuizService {
        private ResultEnum status = ResultEnum.Ok;
        private string errormessage = string.Empty;
        private int incompleteQuizDays = System.Configuration.ConfigurationManager.AppSettings["IncompleteQuizDays"] != null ? Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["IncompleteQuizDays"]) : -2;
        public ResultEnum Status { get { return status; } set { status = value; } }
        public string ErrorMessage { get { return errormessage; } set { errormessage = value; } }

        public UncompleteQuizService() { }
        public void UpdateUncompleteService(object state) {
            while (true) {

                try {
                    
                    if (incompleteQuizDays > -1) {
                        return;
                    }
                    var startdate = DateTime.UtcNow.AddDays(incompleteQuizDays);
                    var endDate = DateTime.UtcNow.AddDays(incompleteQuizDays + (-2));
                    List<int> quizAttempList = new List<int>();
                    using (var UOWObj = new AutomationUnitOfWork()) {
                        var quizStatsList = UOWObj.QuizStatsRepository.GetUncompleteQuizStats(startdate, endDate);

                        if (quizStatsList != null && quizStatsList.Any()) {
                            quizAttempList.AddRange(quizStatsList.Select(v => v.QuizAttemptId));
                        }
                    }

                    if (quizAttempList != null && quizAttempList.Any()) {
                        foreach (var item in quizAttempList)
                            MarkQuizIncomplete(item);
                    }

                } catch (Exception ex) { } finally { Thread.Sleep(new TimeSpan(0, 2, 0, 0)); }
            }
        }

        public void MarkQuizIncomplete(int quizAttemptId) {
            using (var UOWObj = new AutomationUnitOfWork()) {
                IQuizAttemptService ObjQuizAttemptService = new QuizAttemptService();
                var quizAttempts = UOWObj.QuizAttemptsRepository.GetByID(quizAttemptId);
                var companyInfo = UOWObj.CompanyRepository.GetByID(quizAttempts.CompanyId);
                var quizDetailsInfo = UOWObj.QuizDetailsRepository.GetByID(quizAttempts.QuizId);
                var quizInfo = UOWObj.QuizRepository.GetByID(quizDetailsInfo.ParentQuizId);

                PublishQuizTmpModel publishQuizTmpModel = new PublishQuizTmpModel {
                    QuizCode = quizAttempts.Code,
                    QuizattemptId = quizAttemptId,
                    QuizTitle = quizDetailsInfo.QuizTitle,
                    LeadUserId = quizAttempts.LeadUserId,
                    QuizDetailId = quizDetailsInfo.Id,
                    QuizAttemptCreatedOn = quizAttempts.CreatedOn,
                    IsViewed = quizAttempts.IsViewed,
                    RequestMode = quizAttempts.Mode,
                    RecruiterUserId = quizAttempts.RecruiterUserId ?? 0,
                    WorkpackageInfoId = quizAttempts.WorkPackageInfoId ?? 0,
                    ConfigurationDetailId = quizAttempts.ConfigurationDetailsId ?? 0,
                    CompanyId = quizAttempts.CompanyId ?? 0,
                    ParentQuizid = quizDetailsInfo.ParentQuizId,
                    CompanyCode = companyInfo.ClientCode,
                    QuizType = quizInfo.QuizType,
                };
                ObjQuizAttemptService.NotCompletedQuiz(publishQuizTmpModel);
            }
        }
    }
}