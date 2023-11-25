using Core.Common.Extensions;
using Newtonsoft.Json;
using NLog;
using QuizApp.Helpers;
using QuizApp.RepositoryExtensions;
using QuizApp.RepositoryPattern;
using QuizApp.Request;
using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace QuizApp.Services.Service
{
    public class AttemptQuizRequest
    {
        public List<TextAnswerRequest> TextAnswerList { get; set; }
        public string QuizCode { get; set; }
        public string Mode { get; set; }
        public string Type { get; set; }
        public int QuestionId { get; set; } = -1;
        public string AnswerId { get; set; } = string.Empty;
        public int UserTypeId { get; set; } = 0;
        public int? QuestionType { get; set; } = (int)BranchingLogicEnum.QUESTION;
        public int? UsageType { get; set; } = null;
        public int BusinessUserId { get; set; } = 1;
    }

    public abstract class QuizAttemptServiceBase
    {
        private static bool enableExternalActionQueue =  bool.Parse(string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["EnableExternalActionQueue"]) ? "true" : ConfigurationManager.AppSettings["EnableExternalActionQueue"]);

        public static NLog.Logger Logger = LogManager.GetCurrentClassLogger();
      

        public CompanyModel BindCompanyDetails(Db.Company companyInfo)
        {
            CompanyModel companyObj;
            if (companyInfo != null)
            {
                companyObj = new CompanyModel
                {
                    Id = companyInfo.Id,
                    AlternateClientCodes = companyInfo.AlternateClientCodes,
                    ClientCode = companyInfo.ClientCode,
                    CompanyName = companyInfo.CompanyName,
                    CompanyWebsiteUrl = companyInfo.CompanyWebsiteUrl,
                    JobRocketApiAuthorizationBearer = companyInfo.JobRocketApiAuthorizationBearer,
                    JobRocketApiUrl = companyInfo.JobRocketApiUrl,
                    JobRocketClientUrl = companyInfo.JobRocketClientUrl,
                    LeadDashboardApiAuthorizationBearer = companyInfo.LeadDashboardApiAuthorizationBearer,
                    LeadDashboardApiUrl = companyInfo.LeadDashboardApiUrl,
                    LeadDashboardClientUrl = companyInfo.LeadDashboardClientUrl,
                    LogoUrl = companyInfo.LogoUrl,
                    PrimaryBrandingColor = companyInfo.PrimaryBrandingColor,
                    SecondaryBrandingColor = companyInfo.SecondaryBrandingColor

                };
            }
            else
            {
                companyObj = new CompanyModel();
            }

            return companyObj;

        }

        public List<Db.QuizVariables> GetQuizVariablesDetails(int QuizDetailId)
        {
            var VariableDetails = new List<QuizVariablesDetails>();
            if (QuizDetailId != 0)
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizVar = UOWObj.QuizVariablesRepository.Get(v => v.QuizDetailsId == QuizDetailId).ToList();
                    return quizVar;
                }
            }
            return null;
        }


        public void UpdateQuizAttemptViewed(int quizAttemptId,int CompanyId)
        {
            using (var UOWObj = new AutomationUnitOfWork())
            {
                //var quizAttemptsObj = UOWObj.QuizAttemptsRepository.Get(r => r.Id == quizAttemptId).FirstOrDefault();
                var quizAttemptsObj = UOWObj.QuizAttemptsRepository.GetQuizAttemptsById(quizAttemptId).FirstOrDefault();
                if (quizAttemptsObj != null)
                {
                    quizAttemptsObj.IsViewed = true;
                    quizAttemptsObj.CompanyId = CompanyId;
                    UOWObj.QuizAttemptsRepository.Update(quizAttemptsObj);
                    UOWObj.Save();
                }
            }
        }

        public static string VariableLinking(string text, bool isTitle, bool showScore, string resultScore, PublishQuizTmpModel publishQuizTmpModel, string MsgVariables="")
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return text;
            }

            try
            {
                QuizLeadInfo LeadDetails = publishQuizTmpModel.LeadDetails;
                 
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var list = new List<string>();

                    if (!string.IsNullOrWhiteSpace(text) && !string.IsNullOrWhiteSpace(MsgVariables) && publishQuizTmpModel.ContactObject != null)                   
                    {
                        if (!string.IsNullOrWhiteSpace(MsgVariables))
                        {
                            list = MsgVariables != null ? MsgVariables.Split(',').ToList() : new List<string>();

                            if (Regex.Matches(text, @"{{([a-zA-Z0-9.])+}}").Count > 0)
                            {
                                var m1 = Regex.Matches(text, @"{{([a-zA-Z0-9.])+}}");
                                
                                foreach (var match in m1)
                                {
                                    
                                    if (!list.Contains(match.ToString()))
                                    {
                                        list.Add(match.ToString());
                                    }                                   
                                }
                            }
                        }

                            var exObjpair = publishQuizTmpModel.ContactObject.ToDictionary(k => "{{" + k.Key.ToLower() + "}}", k => k.Value);
                            exObjpair.AddDictionarykey("{{fname}}", exObjpair.GetDictionarykeyValueStringObject("{{firstname}}"));
                            exObjpair.AddDictionarykey("{{lname}}", exObjpair.GetDictionarykeyValueStringObject("{{lastname}}"));
                            exObjpair.AddDictionarykey("{{phone}}", exObjpair.GetDictionarykeyValueStringObject("{{telephone}}"));
                            exObjpair.AddDictionarykey("{{email}}", exObjpair.GetDictionarykeyValueStringObject("{{email}}"));

                            foreach (var item in list)
                            {
                                if (exObjpair.CheckDictionarykeyExistStringObject(item))
                                {
                                    text = text.Replace(item, exObjpair.GetDictionarykeyValueStringObject(item));
                                }

                            }                        
                    }

                    if (!string.IsNullOrWhiteSpace(text) && Regex.Matches(text, @"%\b\S+?\b%").Count > 0)
                    {                        
                    
                                text = string.IsNullOrEmpty(text) ? string.Empty : text;
                        StringBuilder correctanswerexplanation = new StringBuilder("Incorrect answer details:<br/>");
                        if (text.Contains("%correctanswerexplanation%") && publishQuizTmpModel.QuizDetailId != 0 && (publishQuizTmpModel.QuizType == (int)QuizTypeEnum.Assessment || publishQuizTmpModel.QuizType == (int)QuizTypeEnum.AssessmentTemplate))
                        {
                            var questList = UOWObj.QuizQuestionStatsRepository.Get(r => r.Id == publishQuizTmpModel.QuizattemptId);
                            if (questList != null && questList.Any())
                            {
                                foreach (var ques in questList.Where(r => r.Status == (int)StatusEnum.Active && r.QuizAnswerStats.Any(k => !k.AnswerOptionsInQuizQuestions.IsUnansweredType)))
                                {
                                    var correctAnswerTxt = string.Empty;
                                    var Incorrectques = false;
                                    if (ques.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && ques.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Any(r => r.Status == (int)StatusEnum.Active && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value))
                                    {
                                        Incorrectques = !((ques.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Select(s => s.Id).OrderBy(s => s)).SequenceEqual(ques.QuizAnswerStats.Select(r => r.AnswerId).OrderBy(s => s)));
                                        correctAnswerTxt = string.Join(",", ques.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Select(t => t.Option).Select(s => "'" + s + "'"));
                                    }
                                    else if (ques.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single)
                                    {
                                        Incorrectques = ques.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer.HasValue && ques.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer.Value ? false : true;
                                        correctAnswerTxt = ques.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(t => t.IsCorrectAnswer.HasValue ? t.IsCorrectAnswer.Value : false).Select(t => t.Option).FirstOrDefault();
                                    }

                                    if (Incorrectques)
                                    {

                                        var questionsInQuiz = UOWObj.QuestionsInQuizRepository.Get(r => r.QuizId == publishQuizTmpModel.QuizDetailId).FirstOrDefault();
                                        correctanswerexplanation.Append("Question : " + ques.QuestionsInQuiz.Question + "<br/>");
                                        correctanswerexplanation.Append("Your Answer : " + string.Join(",", ques.QuizAnswerStats.Select(t => t.AnswerOptionsInQuizQuestions.Option).Select(s => "'" + s + "'")) + "<br/>");
                                        correctanswerexplanation.Append("Correct Answer : " + correctAnswerTxt + "<br/>");
                                        correctanswerexplanation.Append("Explanation for correct answer : " + questionsInQuiz.AliasTextForCorrectAnswer + "<br/>");
                                    }
                                }
                            }
                        }
                        else
                        {
                            text = text.Replace("%correctanswerexplanation%", string.Empty);
                        }


                        IEnumerable<Db.VariablesDetails> variablesDetailList = new List<Db.VariablesDetails>();
                        string Obj = string.IsNullOrEmpty(text) ? string.Empty : text.Replace("%fname%", (LeadDetails != null && !string.IsNullOrEmpty(LeadDetails.firstName)) ? LeadDetails.firstName : string.Empty)
                                                          .Replace("%lname%", (LeadDetails != null && !string.IsNullOrEmpty(LeadDetails.lastName)) ? LeadDetails.lastName : string.Empty)
                                                          .Replace("%phone%", (LeadDetails != null && !string.IsNullOrEmpty(LeadDetails.telephone)) ? LeadDetails.telephone : string.Empty)
                                                          .Replace("%email%", (LeadDetails != null && !string.IsNullOrEmpty(LeadDetails.firstName)) ? LeadDetails.email : string.Empty).Replace("%qendresult%", showScore && !string.IsNullOrEmpty(resultScore) ? resultScore.ToString() : string.Empty).Replace("%correctanswerexplanation%", correctanswerexplanation.ToString() == "Incorrect answer details:<br/>" ? string.Empty : correctanswerexplanation.ToString()).Replace("%leadid%", (LeadDetails != null && !string.IsNullOrEmpty(LeadDetails.contactId)) ? LeadDetails.contactId : string.Empty);
                        if (!string.IsNullOrEmpty(text))
                        {
                            Obj = Obj.Replace("%qname%", !isTitle && publishQuizTmpModel.QuizTitle != null ? publishQuizTmpModel.QuizTitle.Replace("%qname%", string.Empty) : string.Empty);
                            string qlink = string.Empty;
                            if (publishQuizTmpModel.QuizattemptId != 0)
                            {
                               if (publishQuizTmpModel.QuizattemptId >0)
                                {
                                    if (publishQuizTmpModel.WorkpackageInfoId >0)
                                    {
                                        qlink = GlobalSettings.webUrl.ToString() + "/quiz?Code=" + publishQuizTmpModel.QuizPublishCode + "&UserTypeId=2&UserId=" + publishQuizTmpModel.LeadUserId + "&WorkPackageInfoId=" + publishQuizTmpModel.WorkpackageInfoId;
                                    }
                                    else
                                    {

                                        if (publishQuizTmpModel.RecruiterUserId >0)
                                        {

                                            qlink = GlobalSettings.elearningWebURL.ToString() + "/course/" + publishQuizTmpModel.RecruiterUserId + "?Code=" + publishQuizTmpModel.QuizPublishCode;
                                        }
                                        else
                                        {
                                            qlink = GlobalSettings.webUrl.ToString() + "/quiz?Code=" + publishQuizTmpModel.QuizPublishCode;
                                        }
                                    }

                                    if (publishQuizTmpModel.ConfigurationDetailId > 0 && publishQuizTmpModel.QuizDetailId >0)
                                    {
                                        variablesDetailList = UOWObj.VariablesDetailsRepository.Get(r => r.ConfigurationDetailsId == publishQuizTmpModel.ConfigurationDetailId);
                                    }
                                    else if (!string.IsNullOrEmpty(publishQuizTmpModel.LeadUserId) && publishQuizTmpModel.QuizDetailId > 0)
                                    {
                                        variablesDetailList = UOWObj.VariablesDetailsRepository.Get(r => r.VariableInQuiz.QuizId == publishQuizTmpModel.QuizDetailId && r.LeadId == publishQuizTmpModel.LeadUserId);
                                    }
                                }
                            }
                            Obj = Obj.Replace("%qlink%", qlink);
                            MatchCollection mcol = Regex.Matches(Obj, @"%\b\S+?\b%");

                            foreach (Match m in mcol)
                            {
                                var variablesDetailObj = variablesDetailList.FirstOrDefault(t => t.VariableInQuiz.Variables.Name == m.ToString().ToLower().Replace("%", string.Empty));
                                
                                if (variablesDetailObj != null && ! string.IsNullOrWhiteSpace(variablesDetailObj.VariableValue))
                                {
                                    if (variablesDetailObj.VariableValue.ContainsCI("%") && publishQuizTmpModel.ContactObject != null)
                                    {
                                        var runtimeVariable = variablesDetailObj.VariableValue.Replace("%", "").Trim();
                                        CommonStaticData.VacancyVariableLink(publishQuizTmpModel.ContactObject, runtimeVariable, publishQuizTmpModel.CompanyCode);
                                        variablesDetailObj.VariableValue = publishQuizTmpModel.ContactObject.GetDictionarykeyValueStringObject(runtimeVariable);
                                    }                                 

                                    Obj = Obj.Replace(m.ToString(), variablesDetailObj.VariableValue);
                                }
                                else
                                {
                                    Obj = Obj.Replace(m.ToString(), string.Empty);
                                }
                            }
                            return Obj;


                        }

                        else
                        {
                            return text;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return text;
        }

        public static void AddPendingApi(string requestTypeURL, string authorization, string requestData, string requestType)
        {
            using (var UOWObj = new AutomationUnitOfWork())
            {
                var pendingApiQueueObj = new Db.PendingApiQueue
                {
                    CreatedOn = DateTime.UtcNow,
                    RequestTypeURL = requestTypeURL,
                    Authorization = authorization,
                    RequestData = requestData,
                    RequestType = requestType
                };
                UOWObj.PendingApiQueueRepository.Insert(pendingApiQueueObj);
                UOWObj.Save();
            }
        }
        public static void SyncQuizAttemptWithleadTask(PublishQuizTmpModel publishQuizTmpModel)
        {
          //bool enableExternalActionQueue = bool.Parse(string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["EnableExternalActionQueue"]) ? "true" : ConfigurationManager.AppSettings["EnableExternalActionQueue"]);

            try
            {

                Task.Run(() =>
                {
                    SyncQuizAttemptWithlead(publishQuizTmpModel);
                });



                //if (enableExternalActionQueue)
                //{
                //    SyncQuizAttemptWithleadQueue(publishQuizTmpModel);

                //}
                //else
                //{
                //    Task.Run(() =>
                //    {
                //        SyncQuizAttemptWithlead(publishQuizTmpModel);
                //    });

                //}
            }
            catch (Exception ex)
            {

                Logger.Log(LogLevel.Error, "SyncQuizAttemptWithlead inside function error - " + publishQuizTmpModel.QuizattemptId + " Exception-" + ex.Message);
            }
        }

        public static void SyncQuizAttemptWithleadQueue(PublishQuizTmpModel publishQuizTmpModel)
        {
            try
            {
                ExternalActionQueueService.InsertExternalActionQueue(publishQuizTmpModel.CompanyId, publishQuizTmpModel.LeadUserId, QueueItemTypes.InsertQuizAttemptLead, (int)QueueStatusTypes.New, JsonConvert.SerializeObject(publishQuizTmpModel));
            }
            catch (Exception ex)
            {

                return;
            }
        }


        public void SaveLeadTags(PublishQuizTmpModel publishQuizTmpModel)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(publishQuizTmpModel.LeadUserId))
                {
                    return;
                }

                if (publishQuizTmpModel.LeadUserId.ContainsCI("SF-"))
                {
                    return;
                }

                if (enableExternalActionQueue)
                {
                    SaveLeadTagsWithleadQueue(publishQuizTmpModel);

                }
                else
                {
                    Task.Run(() =>
                    {
                        //List<int> tagList = null;
                        //using (var UOWObj = new AutomationUnitOfWork())
                        //{
                        //    var tagListobj = UOWObj.QuizQuestionStatsRepository.Get(r => r.QuizAttemptId == publishQuizTmpModel.QuizattemptId && r.Status == (int)StatusEnum.Active);
                        //    if (tagListobj != null && tagListobj.Any())
                        //    {
                        //        tagList = tagListobj.SelectMany(a => a.QuizAnswerStats.SelectMany(b => b.AnswerOptionsInQuizQuestions.TagsInAnswer.Select(q => q.TagId).ToList())).ToList();
                        //    }
                        //}
                        //if (tagList != null && tagList.Any())
                        //{
                        //    if (!string.IsNullOrEmpty(publishQuizTmpModel.LeadUserId))
                        //    {
                        //        LeadTags leadTags = new LeadTags();
                        //        leadTags.LeadId = publishQuizTmpModel.LeadUserId;
                        //        leadTags.TagsIds = tagList.Distinct().ToArray();

                        //        OWCHelper.SaveLeadTags(leadTags, publishQuizTmpModel.CompanyDetails);
                        //    }
                        //}

                        SaveLeadTagsTask(publishQuizTmpModel);
                    });
                }
            }
            catch (Exception ex)
            {

                Logger.Log(LogLevel.Error, "SyncQuizAttemptWithlead inside function error - " + publishQuizTmpModel.QuizattemptId + " Exception-" + ex.Message);
            }
        }

        //public static void SaveLeadTagsTask(PublishQuizTmpModel publishQuizTmpModel)
        //{
        //    List<int> tagList = null;
        //    using (var UOWObj = new AutomationUnitOfWork())
        //    {
        //        var tagListobj = UOWObj.QuizQuestionStatsRepository.Get(r => r.QuizAttemptId == publishQuizTmpModel.QuizattemptId && r.Status == (int)StatusEnum.Active);
        //        if (tagListobj != null && tagListobj.Any())
        //        {
        //            tagList = tagListobj.SelectMany(a => a.QuizAnswerStats.SelectMany(b => b.AnswerOptionsInQuizQuestions.TagsInAnswer.Select(q => q.TagId).ToList())).ToList();
        //        }
        //    }
        //    if (tagList != null && tagList.Any())
        //    {
        //        if (!string.IsNullOrEmpty(publishQuizTmpModel.LeadUserId))
        //        {
        //            LeadTags leadTags = new LeadTags();
        //            leadTags.LeadId = publishQuizTmpModel.LeadUserId;
        //            leadTags.TagsIds = tagList.Distinct().ToArray();

        //            OWCHelper.SaveLeadTags(leadTags, publishQuizTmpModel.CompanyDetails);
        //        }
        //    }
        //}

        public static void SaveLeadTagsTask(PublishQuizTmpModel publishQuizTmpModel)

        {
            List<int> tagList = null;
            using (var UOWObj = new AutomationUnitOfWork())
            {
                var quizAnswerStatsobj = UOWObj.QuizAnswerStatsRepository.Get(r => r.QuizAttemptId == publishQuizTmpModel.QuizattemptId).Select(v => v.AnswerId).ToList();

                if (quizAnswerStatsobj != null)

                {
                    tagList = UOWObj.TagsInAnswerRepository.GetQueryable(r => (quizAnswerStatsobj.Any(s => s == r.AnswerOptionsId))).Select(v => v.TagId).ToList();

                    if (tagList != null && tagList.Any())
                    {
                        if (!string.IsNullOrEmpty(publishQuizTmpModel.LeadUserId))
                        {
                            LeadTags leadTags = new LeadTags();
                            leadTags.LeadId = publishQuizTmpModel.LeadUserId;
                            leadTags.TagsIds = tagList.Distinct().ToArray();

                            OWCHelper.SaveLeadTags(leadTags, publishQuizTmpModel.CompanyDetails);
                        }
                    }

                }

            }


        }

        public static void SaveLeadTagsWithleadQueue(PublishQuizTmpModel publishQuizTmpModel)
        {
            try
            {
                ExternalActionQueueService.InsertExternalActionQueue(publishQuizTmpModel.CompanyId, publishQuizTmpModel.LeadUserId, QueueItemTypes.InsertSaveLeadTags, (int)QueueStatusTypes.New, JsonConvert.SerializeObject(publishQuizTmpModel));
            }
            catch (Exception ex)
            {

                return;
            }
        }

        public void PushAppointment(PublishQuizTmpModel publishQuizTmpModel, int? resultId)
        {
            if (publishQuizTmpModel.RequestMode == "AUDIT" && publishQuizTmpModel.IsBranchingLogicEnabled && (publishQuizTmpModel.LeadUserId != null || publishQuizTmpModel.RecruiterUserId > 0))
            {

                Db.BranchingLogic branchingLogic = null;
                Db.ActionsInQuiz actionsInQuiz = null;
                List<int> calendarIds = new List<int>();
                int appointmentTypeId = 0;
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    //branchingLogic = UOWObj.BranchingLogicRepository.Get(a => a.QuizId == publishQuizTmpModel.QuizDetailId && a.SourceObjectId == resultId && (a.SourceTypeId == (int)BranchingLogicEnum.RESULT || a.SourceTypeId == (int)BranchingLogicEnum.RESULTNEXT) && a.DestinationTypeId == (int)BranchingLogicEnum.ACTION).FirstOrDefault();
                    branchingLogic = UOWObj.BranchingLogicRepository.GetBranchingLogicByQuizId(publishQuizTmpModel.QuizDetailId).Where(a => a.SourceObjectId == resultId && (a.SourceTypeId == (int)BranchingLogicEnum.RESULT || a.SourceTypeId == (int)BranchingLogicEnum.RESULTNEXT) && a.DestinationTypeId == (int)BranchingLogicEnum.ACTION).FirstOrDefault();
                    if (branchingLogic != null)
                    {
                        actionsInQuiz = UOWObj.ActionsInQuizRepository.Get(a => a.QuizId == publishQuizTmpModel.QuizDetailId && a.Id == branchingLogic.DestinationObjectId).FirstOrDefault();
                        calendarIds = ((actionsInQuiz.LinkedCalendarInAction != null && actionsInQuiz.LinkedCalendarInAction.Any()) ? actionsInQuiz.LinkedCalendarInAction.Select(t => t.CalendarId).ToList() : new List<int>());
                    }

                }

                if (branchingLogic != null && publishQuizTmpModel.LeadUserId != null && actionsInQuiz != null)
                {
                    if (actionsInQuiz.ActionType == (int)ActionTypeEnum.Appointment || actionsInQuiz.ActionType == (int)ActionTypeEnum.LinkWithLeadDashboardAppointment)
                    {
                        if (actionsInQuiz.AppointmentId > 0)
                        {
                            appointmentTypeId = actionsInQuiz.AppointmentId.Value;
                            var sourceId = string.Empty;
                            var sourceName = string.Empty;

                            if (publishQuizTmpModel.ConfigurationDetailId > 0)
                            {

                                using (var UOWObj = new AutomationUnitOfWork())
                                {
                                    var configurationDetails = UOWObj.ConfigurationDetailsRepository.GetByID(publishQuizTmpModel.ConfigurationDetailId);
                                    if (configurationDetails != null && configurationDetails.ConfigurationType == "UNKNOWN_LEADS")
                                    {
                                        sourceId = configurationDetails.SourceId ?? string.Empty;
                                        sourceName = configurationDetails.SourceName ?? string.Empty;
                                    }
                                }
                            }
                            else
                            {
                                if (publishQuizTmpModel.WorkpackageInfoId > 0)
                                {
                                    using (var UOWObj = new AutomationUnitOfWork())
                                    {
                                        //var workPackageInfo = UOWObj.WorkPackageInfoRepository.Get(v => v.Id == publishQuizTmpModel.WorkpackageInfoId).FirstOrDefault();
                                        var workPackageInfo = UOWObj.WorkPackageInfoRepository.GetWorkpackageById(publishQuizTmpModel.WorkpackageInfoId).FirstOrDefault();
                                        if (workPackageInfo != null)
                                        {
                                            sourceId = workPackageInfo.CampaignId;
                                            sourceName = workPackageInfo.CampaignName;
                                        }
                                    }
                                }
                            }


                            var pushWorkPackageObj = new Helpers.Models.AppointmentWorkPackage()
                            {
                                AppointmentTypeId = appointmentTypeId,
                                BusinessUserId = publishQuizTmpModel.RequestBusinessUserId,
                                LeadUserId = publishQuizTmpModel.LeadUserId,
                                CompanyCode = publishQuizTmpModel.CompanyCode,
                                SourceId = sourceId,
                                SourceName = sourceName,
                                CalendarIds = calendarIds,
                                IsUpdatedSend = false,
                                Body = string.Empty,
                                Subject = string.Empty,
                                SMSText = string.Empty
                            };


                            AppointmentHelper.PushWorkPackage(pushWorkPackageObj);


                        }
                    }
                }

            }
        }

        public static void SyncQuizAttemptWithlead(PublishQuizTmpModel publishQuizTmpModel)
        {
            
            try
            {
                var urlhub = GlobalSettings.HubUrl;
                var token = GlobalSettings.HubUrlBearer;
                var urlAutomation = urlhub + "/api/v1/Automations/web-hooks/UpdateAutomationStatus";
                var urlAppointment = urlhub + "/api/v1/appointments/web-hooks/UpdateAppointmentStatus";
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizAttemptsObj = UOWObj.QuizAttemptsRepository.Get(r => r.Id == publishQuizTmpModel.QuizattemptId).FirstOrDefault();
                    var quizStatsObj = quizAttemptsObj.QuizStats.FirstOrDefault();

                    var UpdateQuizStatusObj = new LeadQuizStatus()
                    {
                        AutomationTitle = publishQuizTmpModel.QuizTitle,
                        CreatedDate = quizAttemptsObj.CreatedOn.ToString("yyyy-MM-dd'T'HH:mm:ss") ?? quizAttemptsObj.CreatedOn.ToString("yyyy-MM-dd'T'HH:mm:ss"),
                        StartedDate = quizStatsObj.StartedOn.ToString("yyyy-MM-dd'T'HH:mm:ss") ?? string.Empty,
                        AttemptDate = quizStatsObj.CompletedOn.HasValue? quizStatsObj.CompletedOn.Value.ToString("yyyy-MM-dd'T'HH:mm:ss") :string.Empty,
                        SourceId = quizAttemptsObj.ConfigurationDetails.SourceId ?? string.Empty,
                        ContactId = quizAttemptsObj.LeadUserId,
                        ClientCode = publishQuizTmpModel.CompanyCode ?? string.Empty,
                        QuizId = publishQuizTmpModel.ParentQuizid,
                        ConfigurationId = quizAttemptsObj.ConfigurationDetails.ConfigurationId ?? string.Empty,
                        QuizType = publishQuizTmpModel.QuizType,
                        QuizStatus = "2",
                        RequestId = publishQuizTmpModel.RequestId,
                        Results = new List<Result>()
                    };

                    try
                    {
                        var apiSuccess = false;
                        var res = OWCHelper.UpdateQuizStatus(UpdateQuizStatusObj);
                        if (res != null)
                            apiSuccess = res.status == "true";
                        //ExternalActionQueueService.InsertExternalActionQueue(quizAttemptsObj.CompanyId.Value, quizAttemptsObj.LeadUserId, QueueItemTypes.InsertQuizAttemptLead, (int)QueueStatusTypes.Done, JsonConvert.SerializeObject(UpdateQuizStatusObj));

                        if (!apiSuccess)
                            AddPendingApi(urlAutomation, token, JsonConvert.SerializeObject(UpdateQuizStatusObj), "POST");

                    }

                    catch (Exception)
                    {
                        AddPendingApi(urlAutomation, token, JsonConvert.SerializeObject(UpdateQuizStatusObj), "POST");
                    }
                }
            }
            catch(Exception ex)
            {
               
            }
        }

        public int InsertQuizStat(int quizAttemptId, int? resultId, DateTime startedOn)
        {
            int quizstatId = 0;
            using (var UOWObj = new AutomationUnitOfWork())
            {
                var quizStatsObj = new Db.QuizStats();
                quizStatsObj.QuizAttemptId = quizAttemptId;
                quizStatsObj.StartedOn = startedOn;
                quizStatsObj.ResultId = resultId;
                UOWObj.QuizStatsRepository.Insert(quizStatsObj);
                UOWObj.Save();
                quizstatId = quizStatsObj.Id;
            }

            return quizstatId;
        }


        public List<QuizAnswerSubmit.SubmittedAnswerResult.SubmittedAnswer> CheckQuizAnswerType(PublishQuizTmpModel publishQuizTmpModel, Db.QuestionsInQuiz nextQuestionObj, Db.AnswerOptionsInQuizQuestions submittedAnswerOptionObj, int quizQuestionStatsId)
        {

            var submittedAnswer = new List<QuizAnswerSubmit.SubmittedAnswerResult.SubmittedAnswer>();
            switch (submittedAnswerOptionObj.QuestionsInQuiz.AnswerType)
            {
                case (int)AnswerTypeEnum.Multiple:
                case (int)AnswerTypeEnum.Single:
                    var msgVariables = publishQuizTmpModel.QuizVariables?.Where(v => v.ObjectTypes == (int)QuizVariableObjectTypes.QUESTION && v.ObjectId == nextQuestionObj.Id).FirstOrDefault()?.Variables;
                    CommonStaticData.VacancyVariableReplace(publishQuizTmpModel.ContactObject, msgVariables, publishQuizTmpModel.CompanyCode);
                    submittedAnswer.Add(new QuizAnswerSubmit.SubmittedAnswerResult.SubmittedAnswer
                    {
                        AnswerId = submittedAnswerOptionObj.Id,                        
                    SubmittedAnswerTitle = VariableLinking(submittedAnswerOptionObj.Option, false, false, null, publishQuizTmpModel, msgVariables),
                        SubmittedAnswerImage = nextQuestionObj.ShowAnswerImage.ToBoolValue() ? submittedAnswerOptionObj.OptionImage : string.Empty,
                        PublicIdForSubmittedAnswer = nextQuestionObj.ShowAnswerImage.ToBoolValue() ? submittedAnswerOptionObj.PublicId : string.Empty,
                        AutoPlay = submittedAnswerOptionObj.AutoPlay,
                        SecondsToApply = submittedAnswerOptionObj.SecondsToApply,
                        VideoFrameEnabled = submittedAnswerOptionObj.VideoFrameEnabled
                    });
                    break;
                case (int)AnswerTypeEnum.FullAddress:
                    {
                        using (var UOWObj = new AutomationUnitOfWork())
                        {
                            //var quizAnswerStats = UOWObj.QuizAnswerStatsRepository.Get(v => v.AnswerId == submittedAnswerOptionObj.Id && v.QuizQuestionStatsId == quizQuestionStatsId).FirstOrDefault();
                            var quizAnswerStats = UOWObj.QuizAnswerStatsRepository.GetQuizAnswerStatsByAnswerId(submittedAnswerOptionObj.Id ,quizQuestionStatsId).FirstOrDefault();

                            if (quizAnswerStats.SubAnswerTypeId == (int)SubAnswerTypeEnum.PostCode)
                            {
                                submittedAnswer.Add(new QuizAnswerSubmit.SubmittedAnswerResult.SubmittedAnswer
                                {
                                    AnswerId = submittedAnswerOptionObj.Id,
                                    SubmittedAnswerTitle = quizAnswerStats.AnswerText,
                                    SubmittedSecondaryAnswerTitle = quizAnswerStats.AnswerSecondaryText,
                                    SubmittedAnswerImage = nextQuestionObj.ShowAnswerImage.ToBoolValue() ? submittedAnswerOptionObj.OptionImage : string.Empty,
                                    PublicIdForSubmittedAnswer = nextQuestionObj.ShowAnswerImage.ToBoolValue() ? submittedAnswerOptionObj.PublicId : string.Empty,
                                    AutoPlay = submittedAnswerOptionObj.AutoPlay,
                                    SecondsToApply = submittedAnswerOptionObj.SecondsToApply,
                                    VideoFrameEnabled = submittedAnswerOptionObj.VideoFrameEnabled
                                });
                            }

                            if (quizAnswerStats.SubAnswerTypeId == (int)SubAnswerTypeEnum.HouseNumber)
                            {
                                submittedAnswer.Add(new QuizAnswerSubmit.SubmittedAnswerResult.SubmittedAnswer
                                {
                                    AnswerId = submittedAnswerOptionObj.Id,
                                    SubmittedAnswerTitle = quizAnswerStats.AnswerText,
                                    SubmittedAnswerImage = nextQuestionObj.ShowAnswerImage.ToBoolValue() ? submittedAnswerOptionObj.OptionImage : string.Empty,
                                    PublicIdForSubmittedAnswer = nextQuestionObj.ShowAnswerImage.ToBoolValue() ? submittedAnswerOptionObj.PublicId : string.Empty,
                                    AutoPlay = submittedAnswerOptionObj.AutoPlay,
                                    SecondsToApply = submittedAnswerOptionObj.SecondsToApply,
                                    VideoFrameEnabled = submittedAnswerOptionObj.VideoFrameEnabled
                                });

                                submittedAnswer.Add(new QuizAnswerSubmit.SubmittedAnswerResult.SubmittedAnswer
                                {
                                    AnswerId = submittedAnswerOptionObj.Id,
                                    SubmittedAnswerTitle = UOWObj.QuizAnswerStatsRepository.Get(v => v.AnswerId == submittedAnswerOptionObj.Id && v.QuizQuestionStatsId == quizQuestionStatsId && v.SubAnswerTypeId == (int)SubAnswerTypeEnum.Address).FirstOrDefault()?.AnswerText,
                                    SubmittedAnswerImage = nextQuestionObj.ShowAnswerImage.ToBoolValue() ? submittedAnswerOptionObj.OptionImage : string.Empty,
                                    PublicIdForSubmittedAnswer = nextQuestionObj.ShowAnswerImage.ToBoolValue() ? submittedAnswerOptionObj.PublicId : string.Empty,
                                    AutoPlay = submittedAnswerOptionObj.AutoPlay,
                                    SecondsToApply = submittedAnswerOptionObj.SecondsToApply,
                                    VideoFrameEnabled = submittedAnswerOptionObj.VideoFrameEnabled
                                });

                            }
                        }
                        break;
                    }
                case (int)AnswerTypeEnum.RatingEmoji:
                case (int)AnswerTypeEnum.RatingStarts:
                    {
                        //Todo : dharminder
                        submittedAnswer.Add(new QuizAnswerSubmit.SubmittedAnswerResult.SubmittedAnswer
                        {
                            AnswerId = submittedAnswerOptionObj.Id,
                            SubmittedAnswerTitle = submittedAnswerOptionObj.QuizAnswerStats.FirstOrDefault().AnswerText,
                            SubmittedAnswerImage = nextQuestionObj.ShowAnswerImage.ToBoolValue() ? submittedAnswerOptionObj.OptionImage : string.Empty,
                            PublicIdForSubmittedAnswer = nextQuestionObj.ShowAnswerImage.ToBoolValue() ? submittedAnswerOptionObj.PublicId : string.Empty,
                            AutoPlay = submittedAnswerOptionObj.AutoPlay,
                            SecondsToApply = submittedAnswerOptionObj.SecondsToApply,
                            VideoFrameEnabled = submittedAnswerOptionObj.VideoFrameEnabled,
                            Comment = submittedAnswerOptionObj.QuizAnswerStats.FirstOrDefault().Comment,
                            //for Rating type question
                            OptionTextforRatingOne = submittedAnswerOptionObj.OptionTextforRatingOne,
                            OptionTextforRatingTwo = submittedAnswerOptionObj.OptionTextforRatingTwo,
                            OptionTextforRatingThree = submittedAnswerOptionObj.OptionTextforRatingThree,
                            OptionTextforRatingFour = submittedAnswerOptionObj.OptionTextforRatingFour,
                            OptionTextforRatingFive = submittedAnswerOptionObj.OptionTextforRatingFive
                        });
                        break;
                    }


                default:

                    using (var UOWObj = new AutomationUnitOfWork())
                    {
                        var quizAnswerStats = UOWObj.QuizAnswerStatsRepository.Get(v => v.AnswerId == submittedAnswerOptionObj.Id && v.QuizQuestionStatsId == quizQuestionStatsId).FirstOrDefault();
                        submittedAnswer.Add(new QuizAnswerSubmit.SubmittedAnswerResult.SubmittedAnswer
                        {
                            AnswerId = submittedAnswerOptionObj.Id,
                            SubmittedAnswerTitle = submittedAnswerOptionObj.QuizAnswerStats.FirstOrDefault().AnswerText,
                            SubmittedSecondaryAnswerTitle = quizAnswerStats.AnswerSecondaryText,
                            SubmittedAnswerImage = nextQuestionObj.ShowAnswerImage.ToBoolValue() ? submittedAnswerOptionObj.OptionImage : string.Empty,
                            PublicIdForSubmittedAnswer = nextQuestionObj.ShowAnswerImage.ToBoolValue() ? submittedAnswerOptionObj.PublicId : string.Empty,
                            AutoPlay = submittedAnswerOptionObj.AutoPlay,
                            SecondsToApply = submittedAnswerOptionObj.SecondsToApply,
                            VideoFrameEnabled = submittedAnswerOptionObj.VideoFrameEnabled,
                            Comment = submittedAnswerOptionObj.QuizAnswerStats.FirstOrDefault().Comment
                        });
                    }


                    break;
            }

            return submittedAnswer;
        }

        public Response.UserMediaClassification ExtractDynamicMedia(Db.MediaVariablesDetails mediaObj, QuizLeadInfo leadUserInfo, List<Response.UserMediaClassification> userMediaClassifications, List<Db.UserTokens> dbUsers)
        {

            if (leadUserInfo == null)
            {
                return null;
            }
            if (leadUserInfo != null && string.IsNullOrWhiteSpace(leadUserInfo.contactId))
            {
                return null;
            }

            Response.UserMediaClassification newMedia = null;

            int sourceOwnerId = 0;
            int contactOwnerId = 0;

            if (leadUserInfo.JRSourceOwnerId > 0) {
                sourceOwnerId = leadUserInfo.JRSourceOwnerId;
            }

            if (leadUserInfo.JRContactOwnerId > 0) {
                contactOwnerId = leadUserInfo.JRContactOwnerId;
            }

            if (!string.IsNullOrWhiteSpace(mediaObj.MediaOwner) && !string.IsNullOrWhiteSpace(mediaObj.ProfileMedia) && userMediaClassifications != null && userMediaClassifications.Any())
            {
                if (mediaObj.MediaOwner == "CASE_OWNER" && sourceOwnerId != 0 && userMediaClassifications.Any(a => a.UserToken == dbUsers.FirstOrDefault(b => b.BusinessUserId == sourceOwnerId)?.OWCToken && a.Name == mediaObj.ProfileMedia))
                {
                    newMedia = userMediaClassifications.FirstOrDefault(a => a.UserToken == dbUsers.FirstOrDefault(b => b.BusinessUserId == sourceOwnerId).OWCToken && a.Name == mediaObj.ProfileMedia);
                }
                else if (mediaObj.MediaOwner == "CONTACT_OWNER" && contactOwnerId != 0 && userMediaClassifications.Any(a => a.UserToken == dbUsers.FirstOrDefault(b => b.BusinessUserId == contactOwnerId)?.OWCToken && a.Name == mediaObj.ProfileMedia))
                {
                    newMedia = userMediaClassifications.FirstOrDefault(a => a.UserToken == dbUsers.FirstOrDefault(b => b.BusinessUserId == contactOwnerId).OWCToken && a.Name == mediaObj.ProfileMedia);
                }
            }

            return newMedia;
        }
        public static void InsertQuizQuestionStats(int questionId, int quizAttemptId)
        {
            var quizQuestionStatsObj = new Db.QuizQuestionStats();
            quizQuestionStatsObj.QuizAttemptId = quizAttemptId;
            quizQuestionStatsObj.QuestionId = questionId;
            quizQuestionStatsObj.StartedOn = DateTime.UtcNow;
            quizQuestionStatsObj.Status = (int)StatusEnum.Active;
            using (var UOWObj = new AutomationUnitOfWork())
            {
                UOWObj.QuizQuestionStatsRepository.Insert(quizQuestionStatsObj);
                UOWObj.Save();
            }
        }


        public bool CheckleadForm(PublishQuizTmpModel publishQuizTmpModel, List<QuizAnswerSubmit.PersonalityResult> personalityResultList, IEnumerable<Db.QuizResults> quizresultList, Db.PersonalityResultSetting personalitySetting, out int? formId, out int? flowOrder)
        {


            formId = null;
            flowOrder = null;

            if (publishQuizTmpModel.RequestMode == "PREVIEW" || publishQuizTmpModel.RequestMode == "PREVIEWTEMPLATE")
            {
                return false;
            }

            bool showLeadUserForm = false;
            IEnumerable<Db.ResultIdsInConfigurationDetails> resultIdsInConfigurationDetails = null;
            IEnumerable<Db.QuizComponentLogs> quizComponentLogsList = null;
            var resultIdList = new List<int>();
            using (var UOWObj = new AutomationUnitOfWork())
            {
                if (publishQuizTmpModel.ConfigurationDetailId > 0)
                {
                    //resultIdsInConfigurationDetails = UOWObj.ResultIdsInConfigurationDetailsRepository.Get(v => v.ConfigurationDetailsId == publishQuizTmpModel.ConfigurationDetailId);
                    resultIdsInConfigurationDetails = UOWObj.ResultIdsInConfigurationDetailsRepository.GetResultIdsInConfigurationDetailsByConfigurationDetailsId(publishQuizTmpModel.ConfigurationDetailId);
                    if (resultIdsInConfigurationDetails != null && resultIdsInConfigurationDetails.Any())
                    {
                        resultIdList = UOWObj.ResultIdsInConfigurationDetailsRepository.Get(v => v.ConfigurationDetailsId == publishQuizTmpModel.ConfigurationDetailId).Select(r => r.ResultId).ToList();
                    }
                }

                if (resultIdList.Any())
                {
                    //quizComponentLogsList = UOWObj.QuizComponentLogsRepository.Get(r => r.QuizId == publishQuizTmpModel.QuizDetailId && r.ObjectTypeId == (int)BranchingLogicEnum.RESULT);
                    quizComponentLogsList = UOWObj.QuizComponentLogsRepository.GetQuizComponentLogsByQuizId(publishQuizTmpModel.QuizDetailId , (int)BranchingLogicEnum.RESULT);

                    showLeadUserForm = (resultIdList.Any(r => quizresultList.Any(l => l.Id == r) || quizComponentLogsList.Any(q => q.DraftedObjectId == r && quizresultList.Any(l => l.Id == q.PublishedObjectId))));
                }

                //else
                //{
                //    if (publishQuizTmpModel.LeadUserId == null && publishQuizTmpModel.RecruiterUserId == null)
                //    {
                //        if (publishQuizTmpModel.QuizType == (int)QuizTypeEnum.Personality)
                //        {
                //            showLeadUserForm = personalitySetting.ShowLeadUserForm;
                //        }
                //    }
                //}

                if (resultIdsInConfigurationDetails != null && resultIdsInConfigurationDetails.Any() && showLeadUserForm)
                {
                    if (personalityResultList != null)
                    {
                        var personalityResult = personalityResultList.OrderByDescending(r => r.Percentage.Value).FirstOrDefault();
                        if (resultIdsInConfigurationDetails.Any(r => r.ResultId == personalityResult.ResultId || quizComponentLogsList != null && quizComponentLogsList.Any(q => q.DraftedObjectId == r.ResultId && q.PublishedObjectId == personalityResult.ResultId)))
                        {
                            var leadFormDetailofResultsObj = resultIdsInConfigurationDetails.FirstOrDefault(r => r.ResultId == personalityResult.ResultId || quizComponentLogsList.Any(q => q.DraftedObjectId == r.ResultId && q.PublishedObjectId == personalityResult.ResultId));
                            formId = leadFormDetailofResultsObj.FormId;
                            flowOrder = leadFormDetailofResultsObj.FlowOrder;
                        }
                    }
                }
                else
                {
                    formId = null;
                    flowOrder = null;
                }
            }


            return showLeadUserForm;

        }


        public bool CheckleadForm2(PublishQuizTmpModel publishQuizTmpModel, List<QuizAnswerSubmit.PersonalityResult> personalityResultList, IEnumerable<Db.QuizResults> quizresultList, Db.PersonalityResultSetting personalitySetting, out int? formId, out int? flowOrder)
        {


            formId = null;
            flowOrder = null;

            if (publishQuizTmpModel.RequestMode == "PREVIEW" || publishQuizTmpModel.RequestMode == "PREVIEWTEMPLATE")
            {
                return false;
            }

            bool showLeadUserForm = false;
            IEnumerable<Db.ResultIdsInConfigurationDetails> resultIdsInConfigurationDetails = null;
            IEnumerable<Db.QuizComponentLogs> quizComponentLogsList = null;
            var resultIdList = new List<int>();
            using (var UOWObj = new AutomationUnitOfWork())
            {
                if (publishQuizTmpModel.ConfigurationDetailId > 0)
                {
                    resultIdsInConfigurationDetails = UOWObj.ResultIdsInConfigurationDetailsRepository.Get(v => v.ConfigurationDetailsId == publishQuizTmpModel.ConfigurationDetailId);
                    if (resultIdsInConfigurationDetails != null && resultIdsInConfigurationDetails.Any())
                    {
                        resultIdList = UOWObj.ResultIdsInConfigurationDetailsRepository.Get(v => v.ConfigurationDetailsId == publishQuizTmpModel.ConfigurationDetailId).Select(r => r.ResultId).ToList();
                    }
                }

                if (resultIdList.Any())
                {
                    quizComponentLogsList = UOWObj.QuizComponentLogsRepository.Get(r => r.QuizId == publishQuizTmpModel.QuizDetailId && r.ObjectTypeId == (int)BranchingLogicEnum.RESULT);

                    showLeadUserForm = (resultIdList.Any(r => quizresultList.Any(l => l.Id == r) || quizComponentLogsList.Any(q => q.DraftedObjectId == r && quizresultList.Any(l => l.Id == q.PublishedObjectId))));
                }

                //else
                //{
                //    if (publishQuizTmpModel.LeadUserId == null && publishQuizTmpModel.RecruiterUserId == null)
                //    {
                //        if (publishQuizTmpModel.QuizType == (int)QuizTypeEnum.Personality && personalitySetting.Status == (int)StatusEnum.Active)
                //        {
                //            showLeadUserForm = personalitySetting.ShowLeadUserForm;

                //        }
                //        else
                //        {
                //            showLeadUserForm = quizresultList.Any(r => r.ShowLeadUserForm);
                //        }
                //    }
                //}

                if (showLeadUserForm)
                {
                    if (personalityResultList != null)
                    {
                        var personalityResult = personalityResultList.OrderByDescending(r => r.Percentage.Value).FirstOrDefault();
                        if (resultIdsInConfigurationDetails.Any(r => r.ResultId == personalityResult.ResultId || quizComponentLogsList != null && quizComponentLogsList.Any(q => q.DraftedObjectId == r.ResultId && q.PublishedObjectId == personalityResult.ResultId)))
                        {
                            var leadFormDetailofResultsObj = resultIdsInConfigurationDetails.FirstOrDefault(r => r.ResultId == personalityResult.ResultId || quizComponentLogsList.Any(q => q.DraftedObjectId == r.ResultId && q.PublishedObjectId == personalityResult.ResultId));
                            formId = leadFormDetailofResultsObj.FormId;
                            flowOrder = leadFormDetailofResultsObj.FlowOrder;
                        }
                        else
                        {
                            formId = null;
                            flowOrder = null;
                        }
                    }
                }
            }


            return showLeadUserForm;

        }


        public bool CheckleadFormQuizResult(PublishQuizTmpModel publishQuizTmpModel, Db.QuizResults quizResults, IEnumerable<Db.QuizResults> quizresultList, Db.PersonalityResultSetting personalitySetting, out int? formId, out int? flowOrder)
        {


            formId = null;
            flowOrder = null;

            if (publishQuizTmpModel.RequestMode == "PREVIEW" || publishQuizTmpModel.RequestMode == "PREVIEWTEMPLATE")
            {
                return false;
            }

            bool showLeadUserForm = false;
            IEnumerable<Db.ResultIdsInConfigurationDetails> resultIdsInConfigurationDetails = null;
            IEnumerable<Db.QuizComponentLogs> quizComponentLogsList = null;
            var resultIdList = new List<int>();
            using (var UOWObj = new AutomationUnitOfWork())
            {
                if (publishQuizTmpModel.ConfigurationDetailId > 0)
                {
                    //resultIdsInConfigurationDetails = UOWObj.ResultIdsInConfigurationDetailsRepository.Get(v => v.ConfigurationDetailsId == publishQuizTmpModel.ConfigurationDetailId);
                    resultIdsInConfigurationDetails = UOWObj.ResultIdsInConfigurationDetailsRepository.GetResultIdsInConfigurationDetailsByConfigurationDetailsId(publishQuizTmpModel.ConfigurationDetailId);
                    if (resultIdsInConfigurationDetails != null && resultIdsInConfigurationDetails.Any())
                    {
                        resultIdList = UOWObj.ResultIdsInConfigurationDetailsRepository.Get(v => v.ConfigurationDetailsId == publishQuizTmpModel.ConfigurationDetailId).Select(r => r.ResultId).ToList();
                    }
                }

                if (resultIdList.Any())
                {
                    //quizComponentLogsList = UOWObj.QuizComponentLogsRepository.Get(r => r.QuizId == publishQuizTmpModel.QuizDetailId && r.ObjectTypeId == (int)BranchingLogicEnum.RESULT);
                    quizComponentLogsList = UOWObj.QuizComponentLogsRepository.GetQuizComponentLogsByQuizId(publishQuizTmpModel.QuizDetailId , (int)BranchingLogicEnum.RESULT);

                    showLeadUserForm = (resultIdList.Any(r => quizResults.Id == r || quizComponentLogsList.Any(q => q.DraftedObjectId == r && quizResults.Id == q.PublishedObjectId)));
                }

                //else
                //{
                //    if (publishQuizTmpModel.LeadUserId == null && publishQuizTmpModel.RecruiterUserId == null)
                //    {
                //        if (publishQuizTmpModel.QuizType == (int)QuizTypeEnum.Personality && personalitySetting.Status == (int)StatusEnum.Active)
                //        {
                //            showLeadUserForm = personalitySetting.ShowLeadUserForm;

                //        }
                //        else
                //        {
                //            showLeadUserForm = quizResults.ShowLeadUserForm;
                //        }
                //    }
                //}

                if (showLeadUserForm)
                {

                    if (resultIdsInConfigurationDetails != null)
                    {
                        if (resultIdsInConfigurationDetails.Any(r => r.ResultId == quizResults.Id || quizComponentLogsList != null && quizComponentLogsList.Any(q => q.DraftedObjectId == r.ResultId && q.PublishedObjectId == quizResults.Id)))
                        {
                            var leadFormDetailofResultsObj = resultIdsInConfigurationDetails.FirstOrDefault(r => r.ResultId == quizResults.Id || quizComponentLogsList.Any(q => q.DraftedObjectId == r.ResultId && q.PublishedObjectId == quizResults.Id));
                            formId = leadFormDetailofResultsObj.FormId;
                            flowOrder = leadFormDetailofResultsObj.FlowOrder;
                        }
                        else
                        {
                            formId = null;
                            flowOrder = null;
                        }
                    }
                }
            }


            return showLeadUserForm;

        }

        //public static void ExternalActionQueue(string RequestTypeURL, string Authorization, string RequestData, string RequestType)
        //{
        //    using (var UOWObj = new AutomationUnitOfWork())
        //    {
        //        var externalActionQueueObj = new Db.ExternalActionQueue
        //        {
        //            AddedOn = DateTime.UtcNow,
        //            CompanyCode = RequestTypeURL,
        //            ObjectId = Authorization,
        //            ItemType = RequestData,
        //            ObjectJson = "",
        //            Status = 0,
        //            ModifiedOn = DateTime.UtcNow
        //        };
        //        UOWObj.ExternalActionQueueRepository.Insert(externalActionQueueObj);
        //        UOWObj.Save();
        //    }
        //}
    }
}