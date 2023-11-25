using NLog;
using QuizApp.Db;
using QuizApp.Helpers;
using QuizApp.RepositoryPattern;
using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Web;

namespace QuizApp.Services.Service
{
    public class AutomationDetailsService : IAutomationDetailsService {
        private ResultEnum status = ResultEnum.Ok;
        private string errormessage = string.Empty;
        public ResultEnum Status { get { return status; } set { status = value; } }
        public string ErrorMessage { get { return errormessage; } set { errormessage = value; } }
        public AutomationDetail GetAutomationDetail(int quizId,string ClientCode) {

            var quizObj = new AutomationDetail();
            List<AutomationDetail> lstQuiz = new List<AutomationDetail>();
            var OffsetValue = -3600000;                
            try {
                using (var UOWObj = new AutomationUnitOfWork()) {

                    var quiz = UOWObj.QuizRepository.GetByID(quizId);
                    string[] extensions = new string[] { "MP4", "WEBM", "MPG", "OBG", "MOV", "mp4" };

                    var automationTagsList = CommonStaticData.GetCachedAutomationTagsList(ClientCode);
                     

                        var quizDetailsObj = quiz.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active);

                        if (quizDetailsObj != null) {
                            quizObj.Id = quiz.Id;
                            quizObj.QuizTitle = quizDetailsObj.QuizTitle;
                            quizObj.IsWhatsAppChatBotOldVersion = quiz.IsWhatsAppChatBotOldVersion.HasValue ? quiz.IsWhatsAppChatBotOldVersion.Value : false;
                            quizObj.IsPublished = quiz.State == (int)QuizStateEnum.PUBLISHED;
                            quizObj.CreatedByID = quizDetailsObj.CreatedBy;


                            if (quizDetailsObj.LastUpdatedOn.HasValue) {
                                quizObj.LastEditDate = Utility.ConvertUTCDateToLocalDate(quizDetailsObj.LastUpdatedOn.Value, OffsetValue);
                            }

                            quizObj.CreatedOn = Utility.ConvertUTCDateToLocalDate(quizDetailsObj.CreatedOn, OffsetValue);
                            quizObj.QuizCoverDetail = new QuizCover();
                            quizObj.QuizCoverDetail.QuizCoverImage = quizDetailsObj.ShowQuizCoverImage ? (quizDetailsObj.QuizCoverImage != null &&
                                                                     quizDetailsObj.QuizCoverImage.Contains("/video/upload/") &&
                                                                     (quizDetailsObj.QuizCoverImage.EndsWith("MP4") || quizDetailsObj.QuizCoverImage.EndsWith("WEBM") || quizDetailsObj.QuizCoverImage.EndsWith("MPG") || quizDetailsObj.QuizCoverImage.EndsWith("OBG") || quizDetailsObj.QuizCoverImage.EndsWith("MOV") || quizDetailsObj.QuizCoverImage.EndsWith("mp4"))
                                                                         ? quizDetailsObj.QuizCoverImage.Split(extensions, System.StringSplitOptions.RemoveEmptyEntries).GetValue(0).ToString() + "jpg"
                                                                         : quizDetailsObj.QuizCoverImage) : string.Empty;

                            quizObj.QuizCoverDetail.QuizCoverTitle = quizDetailsObj.ShowQuizCoverTitle ? quizDetailsObj.QuizCoverTitle : string.Empty;
                            quizObj.QuizCoverDetail.PublicIdForQuizCover = quizDetailsObj.PublicId;


                            var quizquestionlist = UOWObj.QuestionsInQuizRepository.GetSelectedColoumn(a => new { a.Id }, filter: (a => a.QuizId == quizDetailsObj.Id && a.Status == (int)StatusEnum.Active)).ToList();
                            if (quizquestionlist != null && quizquestionlist.Any()) {
                                quizObj.NoOfQusetions = quizquestionlist.Count();
                            }

                            quizObj.PublishedCode = quiz.PublishedCode;
                            quizObj.QuizTypeId = (QuizTypeEnum)quiz.QuizType;
                            quizObj.UsageTypes = quiz.UsageTypeInQuiz.Select(v => v.UsageType).Select(c => int.Parse(c.ToString())).ToList();
                            if (!string.IsNullOrEmpty(quiz.AccessibleOfficeId)) {
                                quizObj.AccessibleOfficeId = quiz.AccessibleOfficeId;
                            }
                        }

                    return quizObj;
                }
            } catch (Exception ex) {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }

            return quizObj;
        }
        public bool DeleteAutomationLog(string LeadUserId, string ConfigurationId) {
            
            int quizId = 0;
            int configurationDetailId = 0;

            try {
                using (var UOWObj = new AutomationUnitOfWork()) {
                    var configurationDetailsList = UOWObj.ConfigurationDetailsRepository.Get(r => r.ConfigurationId == ConfigurationId);

                    if (configurationDetailsList != null && configurationDetailsList.Any()) {

                        var configurationDetail = configurationDetailsList.FirstOrDefault();
                        quizId = configurationDetail.QuizId;
                        configurationDetailId = configurationDetail.Id;
                    } else {
                        return false;
                    }

                }

                using (var UOWObj = new AutomationUnitOfWork()) {

                    if (configurationDetailId != 0) {
                        var workpackageinfo = UOWObj.WorkPackageInfoRepository.Get(v => v.LeadUserId == LeadUserId && v.ConfigurationDetailsId == configurationDetailId).FirstOrDefault();
                        var quizAttempts = UOWObj.QuizAttemptsRepository.Get(v => v.LeadUserId == LeadUserId && v.ConfigurationDetailsId == configurationDetailId).FirstOrDefault();

                        if (quizAttempts != null && workpackageinfo != null) {
                            var quizStat = UOWObj.QuizStatsRepository.Get(v => v.QuizAttemptId == quizAttempts.Id).FirstOrDefault();
                            var quizQuesStats = UOWObj.QuizQuestionStatsRepository.Get(v => v.QuizAttemptId == quizAttempts.Id);

                            if (workpackageinfo != null && quizAttempts != null && quizStat != null && quizQuesStats != null) {
                                UOWObj.WorkPackageInfoRepository.Delete(workpackageinfo.Id);
                                UOWObj.QuizStatsRepository.Delete(quizStat.Id);
                                foreach (var item in quizQuesStats) {
                                    UOWObj.QuizQuestionStatsRepository.Delete(item.Id);
                                }
                                UOWObj.QuizAttemptsRepository.Delete(quizAttempts.Id);
                                UOWObj.Save();
                            }
                            else {
                                return false;
                            }
                        }

                        if (quizAttempts == null && workpackageinfo != null) {
                            UOWObj.WorkPackageInfoRepository.Delete(workpackageinfo.Id);
                        }
                    }

                    return true;
                }
            } catch (Exception ex) {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }
         
    }
}