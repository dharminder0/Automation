using NLog;
using QuizApp.Db;
using QuizApp.Helpers;
using QuizApp.RepositoryPattern;
using System;
using System.Configuration;
using System.Linq;
using System.Threading;

namespace QuizApp.Services.Service {
    public class ApiUsageLogsService : IApiUsageLogsService {
        private ResultEnum status = ResultEnum.Ok;
        private string errormessage = string.Empty;
        public NLog.Logger Logger = LogManager.GetCurrentClassLogger();
      
        public ResultEnum Status { get { return status; } set { status = value; } }
        public string ErrorMessage { get { return errormessage; } set { errormessage = value; } }

        public void Save(string controller, string action, string url, string body, DateTime requestDate, string response) {
            try {
                using (var UOWObj = new AutomationUnitOfWork()) {
                    var utcDate = DateTime.UtcNow.Date;
                    var obj = new ApiUsageLogs {
                        Controller = controller,
                        Action = action,
                        Url = url,
                        Body = body,
                        RequestDate = requestDate,
                        Response = response
                    };

                    UOWObj.ApiUsageLogsRepository.Insert(obj);

                    UOWObj.Save();
                }
            } catch (Exception ex) {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }


        public void RunDeleteLogsService(object state) {
            while (true) {
                if (DayOfWeek.Saturday == DateTime.UtcNow.DayOfWeek) {
                    try {
                        ClearApiUsageLogs();
                    } catch (Exception ex) { } finally { Thread.Sleep(new TimeSpan(1, 0, 0, 0)); }
                } else {
                    try {
                        DeletePreviewQuiz();
                    } catch (Exception ex) { } finally { Thread.Sleep(new TimeSpan(1, 0, 0, 0)); }
                }

            }
        }
        public void DeletePreviewQuiz() {
            IApiUsageLogsService ObjApiUsageLogsService = new ApiUsageLogsService();
            ObjApiUsageLogsService.DeletePerivewQuizAttempts(0);
        }

        public int ClearApiUsageLogs() {
            var deletedRows = 0;
            try {
                var apiUsageLogsRetentionDays = !string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["ApiUsageLogsRetentionDays"])
                    ? Convert.ToInt32(ConfigurationManager.AppSettings["ApiUsageLogsRetentionDays"])
                    : 15;
                var deletingDate = DateTime.Now.AddDays(apiUsageLogsRetentionDays * -1);
                using (var UOWObj = new AutomationUnitOfWork()) {
                    string query = $@"delete top(1000) from ApiUsageLogs where RequestDate <getdate()- {apiUsageLogsRetentionDays}";
                    UOWObj.ApiUsageLogsRepository.DeleteRange(query);
                    UOWObj.Save();
                    return deletedRows;
                }
            } catch (Exception e) {
                ErrorLog.LogError(e);
                return deletedRows;
            }
        }

        public void DeletePerivewQuizAttempts(int attemptId) {
            var deletedRecord = 0;
            try {
                using (var UOWObj = new AutomationUnitOfWork()) {
                    if (attemptId != 0) {
                        DeleteAttempt(attemptId, UOWObj);
                    } else {

                        var previewdate = DateTime.UtcNow.AddDays(-2);

                        var attemptList = UOWObj.QuizAttemptsRepository.GetSelectedColoumnV2(r => new { r.Id },
                                                         filter: r => r.Mode == "PREVIEW" && r.LastUpdatedOn < previewdate, null, null, 500);
                        if (attemptList != null && attemptList.Any()) {
                            foreach (var item in attemptList) {
                                DeleteAttempt(item.Id, UOWObj);
                            }

                        }
                    }
                }
            } catch (Exception ex) {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        private void DeleteAttempt(int attemptId, AutomationUnitOfWork UOWObj) {
            var query = $@" 
if exists (select 1 from QuizAttempts where Id = {attemptId} and  Mode = 'PREVIEW')
begin
delete from QuizAnswerStats where QuizQuestionStatsId in (select Id from QuizQuestionStats where QuizAttemptId = {attemptId});
                delete from QuizQuestionStats where QuizAttemptId = {attemptId};
                delete from QuizObjectStats where QuizAttemptId = {attemptId};
                delete from QuizAttempts where Id = {attemptId};

end
";

            UOWObj.QuizAttemptsRepository.DeleteRange(query);
            UOWObj.Save();
        }
    }
}