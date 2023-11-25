using Core.Common.Extensions;
using Newtonsoft.Json;
using NLog;
using QuizApp.Helpers;
using QuizApp.RepositoryExtensions;
using QuizApp.RepositoryPattern;
using QuizApp.Request;
using QuizApp.Response;
using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using static QuizApp.Request.TextAnswerRequest;
using static QuizApp.Response.QuizTemplateResponse;

namespace QuizApp.Services.Service
{


    public partial class QuizAttemptService : QuizAttemptServiceBase, IQuizAttemptService
    {
        private string badgesInfoUpdateJson = "[ {'UserId': {UserId},'CourseId': '{CourseId}','CourseBadgeName': '{CourseBadgeName}','CourseBadgeImageUrl': '{CourseBadgeImageUrl}','CourseTitle': '{CourseTitle}'}]";
        private bool enableAttemptQuizLogging = System.Configuration.ConfigurationManager.AppSettings["EnableAttemptQuizLogging"] != null ? Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["EnableAttemptQuizLogging"]) : false;
        public ResultEnum Status { get { return status; } set { status = value; } }
        public string ErrorMessage { get { return errormessage; } set { errormessage = value; } }
        private ResultEnum status = ResultEnum.Ok;
        private string errormessage = string.Empty;
        private readonly OWCHelper _owchelper = new OWCHelper();
        public static readonly WorkpackageCommunicationService _workpackageComm = new WorkpackageCommunicationService();
        public string notAttemptedQuesText = "You haven't attempted the question.";

        public QuizAnswerSubmit AttemptQuiz(AttemptQuizRequest attemptQuizRequest)
        {
            PublishQuizTmpModel publishQuizTmpModel = new PublishQuizTmpModel();
            QuizAnswerSubmit quizAnswerSubmit = new QuizAnswerSubmit();
            try
            {                
                publishQuizTmpModel.RequestUsageType = attemptQuizRequest.UsageType;
                publishQuizTmpModel.RequestUserTypeId = attemptQuizRequest.UserTypeId;
                publishQuizTmpModel.RequestQuestionId = attemptQuizRequest.QuestionId;

                publishQuizTmpModel.RequestMode = attemptQuizRequest.Mode;
                publishQuizTmpModel.RequestType = attemptQuizRequest.Type;
                publishQuizTmpModel.RequestBusinessUserId = attemptQuizRequest.BusinessUserId;
                publishQuizTmpModel.RequestQuizCode = attemptQuizRequest.QuizCode;
                publishQuizTmpModel.RequestedQuestionType = attemptQuizRequest.QuestionType;

                if (attemptQuizRequest.TextAnswerList != null && attemptQuizRequest.TextAnswerList.Any())
                {
                    QuizApp.Request.TextAnswerRequest request = new QuizApp.Request.TextAnswerRequest();
                    publishQuizTmpModel.RequestedTextAnswerList = request.MapRequestToEntity(attemptQuizRequest.TextAnswerList);
                }

                publishQuizTmpModel.RequestAnswerId = string.IsNullOrEmpty(attemptQuizRequest.AnswerId) ? new List<int>() : attemptQuizRequest.AnswerId.Split(',').Select(Int32.Parse).ToList();

                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizAttemptsObj = UOWObj.QuizAttemptsRepository.GetQuizAttemptsBasicDetailsByCode(attemptQuizRequest.QuizCode).FirstOrDefault();

                    if (quizAttemptsObj == null)
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz not found for the QuizCode " + attemptQuizRequest.QuizCode;
                        return quizAnswerSubmit;
                    }

                    publishQuizTmpModel.QuizattemptId = quizAttemptsObj.Id;
                    publishQuizTmpModel.QuizDetailId = quizAttemptsObj.QuizId;
                    publishQuizTmpModel.LeadUserId = quizAttemptsObj.LeadUserId;                   
                    publishQuizTmpModel.ConfigurationDetailId = quizAttemptsObj.ConfigurationDetailsId.ToIntValue();
                    publishQuizTmpModel.RecruiterUserId = quizAttemptsObj.RecruiterUserId.ToIntValue();
                    publishQuizTmpModel.QuizAttemptCreatedOn = quizAttemptsObj.CreatedOn;
                    publishQuizTmpModel.IsViewed = quizAttemptsObj.IsViewed;
                    if (quizAttemptsObj.WorkPackageInfoId.HasValue && quizAttemptsObj.WorkPackageInfoId.Value > 0)
                    {
                        publishQuizTmpModel.WorkpackageInfoId = quizAttemptsObj.WorkPackageInfoId.Value;
                        publishQuizTmpModel.WorkpackageInfoCreatedOn = quizAttemptsObj.WorkPackageInfo.CreatedOn;

                        //var workPackageInfo = UOWObj.WorkPackageInfoRepository.Get(r => r.Id == publishQuizTmpModel.WorkpackageInfoId).FirstOrDefault();
                        var workPackageInfo = UOWObj.WorkPackageInfoRepository.GetWorkpackageById(publishQuizTmpModel.WorkpackageInfoId).FirstOrDefault();
                        if (workPackageInfo != null)
                        {
                            publishQuizTmpModel.WorkpackageInfoCreatedOn = workPackageInfo.CreatedOn;
                            publishQuizTmpModel.RequestId = workPackageInfo.RequestId;
                        }

                    }
                    if (quizAttemptsObj.QuizId != 0)
                    {
                        publishQuizTmpModel.QuizVariables = GetQuizVariablesDetails(quizAttemptsObj.QuizId);
                    }

                  

                    var quizDetails = UOWObj.QuizDetailsRepository.Get(r => r.Id == publishQuizTmpModel.QuizDetailId).FirstOrDefault();
                    publishQuizTmpModel.QuizTitle = quizDetails.QuizTitle;
                    publishQuizTmpModel.ParentQuizid = quizDetails.ParentQuizId;
                    publishQuizTmpModel.IsBranchingLogicEnabled = quizDetails.IsBranchingLogicEnabled.ToBoolValue();
                    if (publishQuizTmpModel.IsBranchingLogicEnabled)
                    {
                        publishQuizTmpModel.IsQuizResultinBranchingLogic = quizDetails.BranchingLogic.Where(r => r.DestinationTypeId == (int)BranchingLogicEnum.RESULT).Any() ? true : false;
                    }
                    var quiz = UOWObj.QuizRepository.Get(r => r.Id == publishQuizTmpModel.ParentQuizid).FirstOrDefault();
                    if (publishQuizTmpModel.RequestUsageType == null) {
                        var usageTyep = quiz.UsageTypeInQuiz.Where(v => v.QuizId == quiz.Id).Select(v => v.UsageType).FirstOrDefault();
                        if(usageTyep != null && usageTyep > 0) {
                            publishQuizTmpModel.RequestUsageType = usageTyep;
                        }
                        
                    }
                    publishQuizTmpModel.CompanyId = quiz.CompanyId.Value;
                    publishQuizTmpModel.CompanyDetails = BindCompanyDetails(quiz.Company);
                    publishQuizTmpModel.CompanyCode = publishQuizTmpModel.CompanyDetails.ClientCode;
                    publishQuizTmpModel.QuizType = quiz.QuizType;
                    publishQuizTmpModel.QuizAccessibleOfficeId = quiz.AccessibleOfficeId;
                    publishQuizTmpModel.IsQuesAndContentInSameTable = quiz.QuesAndContentInSameTable;
                    publishQuizTmpModel.QuizPublishCode = quiz.PublishedCode;

                    //publishQuizTmpModel.MediaVariableList = UOWObj.MediaVariablesDetailsRepository.Get(r => r.QuizId == publishQuizTmpModel.QuizDetailId && r.ConfigurationDetailsId == publishQuizTmpModel.ConfigurationDetailId).ToList();
                    publishQuizTmpModel.MediaVariableList = UOWObj.MediaVariablesDetailsRepository.GetMediaVariablesDetailsByQuizId(publishQuizTmpModel.QuizDetailId).Where(r => r.ConfigurationDetailsId == publishQuizTmpModel.ConfigurationDetailId).ToList();
                }

                AddAttemptQuizDefaults(publishQuizTmpModel);

                if (publishQuizTmpModel.RequestType.Equals("fetch_quiz"))
                {
                    quizAnswerSubmit.LoadQuizDetails = true;
                    return quizAnswerSubmit;
                }

                if (!string.IsNullOrWhiteSpace(publishQuizTmpModel.LeadUserId) && publishQuizTmpModel.LeadUserId != "0")
                {
                    //publishQuizTmpModel.LeadDetails = OWCHelper.GetLeadInfo(publishQuizTmpModel.LeadUserId, publishQuizTmpModel.CompanyDetails.ClientCode);

                    var commContactInfo = _owchelper.GetCommContactDetails(publishQuizTmpModel.LeadUserId, publishQuizTmpModel.CompanyDetails.ClientCode);

                    if (commContactInfo != null && commContactInfo.Any())
                    {
                        publishQuizTmpModel.ContactObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(commContactInfo.ToString());
                        publishQuizTmpModel.LeadDetails = JsonConvert.DeserializeObject<QuizLeadInfo>(commContactInfo.ToString());
                    }

                    using (var UOWObj = new AutomationUnitOfWork())
                    {
                        //var mediaVariablesDetails = UOWObj.MediaVariablesDetailsRepository.Get(r => r.QuizId == publishQuizTmpModel.QuizDetailId);
                        var mediaVariablesDetails = UOWObj.MediaVariablesDetailsRepository.GetMediaVariablesDetailsByQuizId(publishQuizTmpModel.QuizDetailId);

                        if (mediaVariablesDetails != null && mediaVariablesDetails.Any(a => !string.IsNullOrWhiteSpace(a.MediaOwner) && !string.IsNullOrWhiteSpace(a.ProfileMedia)) && !string.IsNullOrWhiteSpace(publishQuizTmpModel.LeadUserId))
                        {
                            try
                            {
                                List<int> businessUserIds = new List<int>();
                                if (!publishQuizTmpModel.LeadDetails.contactId.ContainsCI("SF"))
                                {
                                    if (!string.IsNullOrWhiteSpace(publishQuizTmpModel.LeadDetails.SourceOwnerId))
                                    {
                                        int sourceowwnerId = int.Parse(publishQuizTmpModel.LeadDetails.SourceOwnerId);
                                        if (sourceowwnerId != 0)
                                        {
                                            businessUserIds.Add(sourceowwnerId);
                                        }
                                    }

                                    if (!string.IsNullOrWhiteSpace(publishQuizTmpModel.LeadDetails.ContactOwnerId))
                                    {
                                        int contactOwnerId = int.Parse(publishQuizTmpModel.LeadDetails.ContactOwnerId);
                                        if (contactOwnerId != 0)
                                        {
                                            businessUserIds.Add(contactOwnerId);
                                        }
                                    }

                                    var dbUsers = UOWObj.UserTokensRepository.Get(r => r.CompanyId == publishQuizTmpModel.CompanyId && businessUserIds.Distinct().Contains(r.BusinessUserId));

                                    publishQuizTmpModel.LeadOwners = dbUsers.ToList();
                                    publishQuizTmpModel.UserMediaClassifications = dbUsers.Any() ? OWCHelper.GetUserMediaClassification(publishQuizTmpModel.CompanyDetails.ClientCode, dbUsers.Select(a => a.OWCToken).ToList()) : new List<Response.UserMediaClassification>();

                                    
                                }
                                else
                                {
                                    if (!string.IsNullOrWhiteSpace(publishQuizTmpModel.LeadDetails.SourceOwnerId) || !string.IsNullOrWhiteSpace(publishQuizTmpModel.LeadDetails.ContactOwnerId))
                                    {
                                        var externalDetails = _owchelper.GetExternalDetails(publishQuizTmpModel.CompanyDetails.ClientCode, publishQuizTmpModel.LeadDetails.SourceOwnerId);
                                        if (externalDetails != null)
                                        {
                                            var userDetails = JsonConvert.DeserializeObject<UserDto>(externalDetails);
                                            if (userDetails?.Id != 0)
                                            {
                                                businessUserIds.Add(userDetails.Id);
                                                publishQuizTmpModel.LeadDetails.JRSourceOwnerId = userDetails.Id;
                                            }

                                        }

                                        var externalDetails2 = _owchelper.GetExternalDetails(publishQuizTmpModel.CompanyDetails.ClientCode, publishQuizTmpModel.LeadDetails.ContactOwnerId);
                                        if (externalDetails2 != null)
                                        {
                                            var userDetails = JsonConvert.DeserializeObject<UserDto>(externalDetails2);
                                            if (userDetails?.Id != 0)
                                            {
                                                businessUserIds.Add(userDetails.Id);
                                                publishQuizTmpModel.LeadDetails.JRContactOwnerId = userDetails.Id;
                                            }
                                        }

                                        if (businessUserIds.Any() && businessUserIds != null)
                                        {
                                            var dbUsers = UOWObj.UserTokensRepository.Get(r => r.CompanyId == publishQuizTmpModel.CompanyId && businessUserIds.Distinct().Contains(r.BusinessUserId));

                                            publishQuizTmpModel.LeadOwners = dbUsers.ToList();
                                            publishQuizTmpModel.UserMediaClassifications = dbUsers.Any() ? OWCHelper.GetUserMediaClassification(publishQuizTmpModel.CompanyDetails.ClientCode, dbUsers.Select(a => a.OWCToken).ToList()) : new List<Response.UserMediaClassification>();
                                        }

                                    }

                                }

                            }
                            catch (Exception ex)
                            {

                            }
                        }
                    }
                }
               
                    FetchRecruiterVariables(publishQuizTmpModel);
                
                if (publishQuizTmpModel.RequestType.Equals("start_quiz"))
                {
                    quizAnswerSubmit = StartQuiz(publishQuizTmpModel, quizAnswerSubmit);
                }
                else
                {
                    if (publishQuizTmpModel.RequestUsageType != null && publishQuizTmpModel.RequestUsageType == (int)UsageTypeEnum.Chatbot && !publishQuizTmpModel.IsViewed)
                    {
                        UpdateQuizAttemptViewed(publishQuizTmpModel.QuizattemptId, publishQuizTmpModel.CompanyId);
                    }

                    if (publishQuizTmpModel.RequestType.Equals("complete_quiz"))
                    {
                        quizAnswerSubmit = CompleteQuiz(publishQuizTmpModel);
                    }
                    else if (publishQuizTmpModel.RequestType.Equals("previous_question"))
                    {
                        quizAnswerSubmit = FetchPeviousQuestionType(publishQuizTmpModel);
                    }
                    else if (publishQuizTmpModel.RequestType.Equals("complete_content"))
                    {
                        quizAnswerSubmit = CompleteContent(publishQuizTmpModel, quizAnswerSubmit);
                    }
                    else if (publishQuizTmpModel.RequestType.Equals("start_question"))
                    {
                        quizAnswerSubmit = StartQuestion(publishQuizTmpModel);
                    }
                    else if (publishQuizTmpModel.RequestType.Equals("complete_question"))
                    {
                        quizAnswerSubmit = CompleteQuestion(publishQuizTmpModel, quizAnswerSubmit);
                    }
                    else if (publishQuizTmpModel.RequestType.Equals("complete_badge"))
                    {
                        quizAnswerSubmit = CompleteBadge(publishQuizTmpModel);
                    }
                    if (publishQuizTmpModel.RequestType.Equals("not_completed")) 
                    {
                        quizAnswerSubmit = NotCompletedQuiz(publishQuizTmpModel);
                    }
                }

                quizAnswerSubmit.PrimaryBrandingColor = publishQuizTmpModel.CompanyDetails.PrimaryBrandingColor;
                quizAnswerSubmit.SecondaryBrandingColor = publishQuizTmpModel.CompanyDetails.SecondaryBrandingColor;
                quizAnswerSubmit.OfficeId = publishQuizTmpModel.QuizAccessibleOfficeId;
                quizAnswerSubmit.CompanyCode = publishQuizTmpModel.CompanyCode;

                if (publishQuizTmpModel.RequestUsageType != null && publishQuizTmpModel.RequestUsageType == 3) {
                    if (quizAnswerSubmit.QuestionDetails != null && quizAnswerSubmit.QuestionDetails.TemplateId == null) {
                        var languageCode = GetLanguageCode(publishQuizTmpModel.QuizDetailId);
                        quizAnswerSubmit.QuestionDetails.LanguageCode = languageCode;
                    }
                }

                if (quizAnswerSubmit.QuestionDetails != null)
                {

                    //if (!String.IsNullOrWhiteSpace(publishQuizTmpModel.LeadUserId) && publishQuizTmpModel.LeadUserId.StartsWith("UNK") && !string.IsNullOrWhiteSpace(publishQuizTmpModel.RequestType) && publishQuizTmpModel.RequestType.Equals("start_quiz")) {

                    //    return TriggerWhatsappTemplateUnknownFlow(publishQuizTmpModel, quizAnswerSubmit);
                    //}
                    //else {
                        TriggerWhatsappTemplate(publishQuizTmpModel, quizAnswerSubmit);
                    //}
                                       

                }
            }

            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $@"AttemptQuiz main exception #### " + JsonConvert.SerializeObject(attemptQuizRequest) + " #### Exception-main " + ex.Message + " ##### inner exception " + ex.InnerException.Message
                    + " ##### stack trace " + ex.StackTrace + " ##### source " + ex.Source
                    );

                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;

                //if (enableAttemptQuizLogging && attemptQuizLogId != 0)
                //{
                //    AttemptQuizLog.UpdateResponseJson(attemptQuizLogId, ErrorMessage);
                //}

                throw ex;
            }

            return quizAnswerSubmit;
        }
        private string GetLanguageCode(int quizDetailId) {
			string languageCode = string.Empty;
			try {
                using (var UOWObj = new AutomationUnitOfWork()) {
                    var brandingAndStyle = UOWObj.QuizBrandingAndStyleRepository.Get(v => v.QuizId == quizDetailId).FirstOrDefault();
                    if (brandingAndStyle != null && brandingAndStyle.Language > 0) {
                        var language = UOWObj.LanguagesRepository.GetByID(brandingAndStyle.Language);
                        if (language != null) {
                            languageCode = language.Culture;
                        }
                    }
                }
            } catch (Exception) {
			}
            
			return languageCode;
		}

        private QuizAnswerSubmit TriggerWhatsappTemplateUnknownFlow(PublishQuizTmpModel publishQuizTmpModel, QuizAnswerSubmit quizAnswerSubmit) {
            var tempId = quizAnswerSubmit.QuestionDetails.TemplateId;
            var tempQuestionId = quizAnswerSubmit.QuestionDetails.QuestionId;
            var templanguageCode = quizAnswerSubmit.QuestionDetails.LanguageCode;
            var leadDetails = publishQuizTmpModel.LeadDetails;
            var clientCode = publishQuizTmpModel.CompanyDetails.ClientCode;
            var listheaderParams = new List<WhatsappMessage.HeaderParameter>();


            if (tempId != null && tempQuestionId != 0 && templanguageCode != null) {

                //var whatsAppHSMTemplates = _owchelper.GetWhatsAppHSMTemplates(quizAnswerSubmit.CompanyCode, "Chatbot", true, templanguageCode);
                //var templateInfo = JsonConvert.DeserializeObject<List<WhatsAppTemplateDto>>(whatsAppHSMTemplates.ToString()).Where(v => v.Id == tempId).FirstOrDefault();

                var whatsAppHSMTemplates = OWCHelper.WhatsAppTemplates(tempId.ToIntValue());
                var templateInfo = JsonConvert.DeserializeObject<WhatsappTemplate>(whatsAppHSMTemplates.ToString());
                var paraamms = new List<WhatsappParam>();

                if (templateInfo.Params != null) {
                    foreach (var paras in templateInfo.Params) {
                        if (paras.ModuleCode.EqualsCI("Chatbot")) {
                            foreach (var paramms in paras.Params) {

                                paraamms.Add(new WhatsappParam {
                                    Paraname = paramms.Paraname,
                                    Position = paramms.Position,
                                    Value = paramms.Value
                                });
                            }
                        }
                    }
                }

                templateInfo.Paramms = paraamms;

                foreach (var item in templateInfo?.TemplateBody) {
                    quizAnswerSubmit.QuestionDetails.Description = item.TempBody;
                }
                quizAnswerSubmit.QuestionDetails.ShowTitle = true;
                quizAnswerSubmit.QuestionDetails.ViewPreviousQuestion = false;

                var whatsappDetail = new WhatsappDetails() {
                    LanguageCode = templanguageCode,
                    TemplateId = tempId
                };

                List<WhatsappParam> newList = templateInfo.Paramms;

                if (publishQuizTmpModel.LeadDetails != null) {

                    var exobj = publishQuizTmpModel.ContactObject;

                    if (exobj != null) {

                        var exObjpair = exobj.ToDictionary(k => k.Key.ToLower(), k => k.Value);

                        var atsvacancyId = exObjpair.GetDictionarykeyValueStringObject("Lead.SourceId");

                        if (newList != null && newList.Any() && !string.IsNullOrWhiteSpace(atsvacancyId)) {
                            if (newList.Any(v => v.Paraname.ContainsCI("vacancy."))) {
                                var vacancyDetails = _owchelper.GetCoreVacancyDetails(atsvacancyId, clientCode);

                                if (!string.IsNullOrWhiteSpace(vacancyDetails)) {
                                    var vacancyObj = (JsonConvert.DeserializeObject<Dictionary<string, object>>(vacancyDetails.ToString()));
                                    // var vacancyObjPair = vacancyObj.ToDictionary(k => k.Key.ToLower(), k => k.Value);
                                    foreach (var item in vacancyObj) {
                                        exObjpair.Add("vacancy." + item.Key.ToLower(), item.Value);
                                    }
                                }
                            }
                        }

                        var templateBody = templateInfo.TemplateBody.FirstOrDefault(v => v.LangCode.EqualsCI(templanguageCode));
                        if (templateBody != null && templateBody.IsEnabledDynamicMedia && publishQuizTmpModel.LeadDetails != null) {
                            UpdateDynamicMedia(publishQuizTmpModel, clientCode, listheaderParams, templateBody);
                        }

                        if (newList != null && newList.Any() && !string.IsNullOrWhiteSpace(leadDetails.contactId)) {

                                if (newList.Any(v => v.Paraname.EqualsCI("recruiter") || v.Paraname.EqualsCI("SourceOwner") || v.Paraname.EqualsCI("LeadOwner"))) {
                                    _workpackageComm.AddRecruiterDetails(exObjpair, publishQuizTmpModel.CompanyDetails);
                                }
                         
                            foreach (var item in newList) {
                                if (exObjpair.ContainsKey(item.Paraname.ToLower())) {
                                    item.Value = exObjpair.GetDictionarykeyValueStringObject(item.Paraname);
                                } else {

                                    item.Value = item.Paraname;
                                }

                                quizAnswerSubmit.QuestionDetails.Description = quizAnswerSubmit.QuestionDetails.Description.Replace(item.Paraname, item.Value?.ToString()).Replace("\n", " ");
                            }

                            quizAnswerSubmit.QuestionDetails.Description = RemoveSpecialChars(quizAnswerSubmit.QuestionDetails.Description);
                            whatsappDetail.WhatsappParam = newList;

                        }
                    }
                }
                quizAnswerSubmit.QuestionDetails.WhatsappDetails = whatsappDetail;

                quizAnswerSubmit.QuestionDetails.WhatsappDetails.HeaderParams = listheaderParams;                


                if (publishQuizTmpModel.LeadUserId.StartsWith("UNK") && publishQuizTmpModel.RequestType.Equals("start_quiz")) {
                    var answerId = quizAnswerSubmit.QuestionDetails.AnswerList.FirstOrDefault().AnswerId;
                    quizAnswerSubmit.PreviousQuestionSubmittedAnswer = null;
                    var textAnswerList = new List<TextAnswerRequest>();
                    var answer = new List<Answer>();

                    answer.Add(new Answer {
                        AnswerText = quizAnswerSubmit.QuestionDetails.AnswerList.FirstOrDefault().AnswerText,
                    });

                    textAnswerList.Add(new TextAnswerRequest {
                        AnswerId = answerId,
                        Answers = answer
                    });


                    return AttemptQuiz(new AttemptQuizRequest {
                        TextAnswerList = textAnswerList,
                        Type = "complete_question",
                        QuizCode = publishQuizTmpModel.RequestQuizCode,
                        QuestionId = quizAnswerSubmit.QuestionDetails.QuestionId,
                        AnswerId = answerId.ToString(),
                        Mode = "AUDIT",
                        UserTypeId = 0,
                        QuestionType = quizAnswerSubmit.QuestionType,
                        UsageType = null
                    }); ;
                }

            }            
            return quizAnswerSubmit;
        }

        private void TriggerWhatsappTemplate(PublishQuizTmpModel publishQuizTmpModel, QuizAnswerSubmit quizAnswerSubmit) {
            var tempId = quizAnswerSubmit.QuestionDetails.TemplateId;
            var tempQuestionId = quizAnswerSubmit.QuestionDetails.QuestionId;
            var templanguageCode = quizAnswerSubmit.QuestionDetails.LanguageCode;
            var leadDetails = publishQuizTmpModel.LeadDetails;
            var clientCode = publishQuizTmpModel.CompanyDetails.ClientCode;           
            var listheaderParams = new List<WhatsappMessage.HeaderParameter>();
           

            if (tempId != null && tempQuestionId != 0 && templanguageCode != null) {
                //var whatsAppHSMTemplates = _owchelper.GetWhatsAppHSMTemplates(quizAnswerSubmit.CompanyCode, "Chatbot", true, templanguageCode);
                //var templateInfo = JsonConvert.DeserializeObject<List<WhatsAppTemplateDto>>(whatsAppHSMTemplates.ToString()).Where(v => v.Id == tempId).FirstOrDefault();

                var whatsAppHSMTemplates = OWCHelper.WhatsAppTemplates(tempId.ToIntValue());
                var templateInfo = JsonConvert.DeserializeObject<WhatsappTemplate>(whatsAppHSMTemplates.ToString());
                var paraamms = new List<WhatsappParam>();

                if (templateInfo.Params != null) {
                    foreach (var paras in templateInfo.Params) {
                        if (paras.ModuleCode.EqualsCI("Chatbot")) {
                            foreach (var paramms in paras.Params) {

                                paraamms.Add(new WhatsappParam {
                                    Paraname = paramms.Paraname,
                                    Position = paramms.Position,
                                    Value = paramms.Value
                                });
                            }
                        }
                    }
                }

                templateInfo.Paramms = paraamms;

                if (templateInfo != null && templateInfo.HeaderParams != null && templateInfo.HeaderParams.Count != 0) {

                    foreach (var itemHeaderParams in templateInfo.HeaderParams) {
                        if (itemHeaderParams.moduleCode == "Chatbot") {
                            listheaderParams = new List<WhatsappMessage.HeaderParameter>();
                            if (itemHeaderParams.Params != null) {
                                foreach (var Headerparams in itemHeaderParams.Params) {
                                listheaderParams.Add(
                                    new WhatsappMessage.HeaderParameter {
                                        paraname = Headerparams.Paraname,
                                        position = Headerparams.Position,
                                        value = Headerparams.Value
                                    });
                                }
                            }
                        }
                    }
                }


                foreach (var item in templateInfo?.TemplateBody) {
                    quizAnswerSubmit.QuestionDetails.Description = item.TempBody;
                }
                quizAnswerSubmit.QuestionDetails.ShowTitle = true;
                quizAnswerSubmit.QuestionDetails.ViewPreviousQuestion = false;

                var whatsappDetail = new WhatsappDetails() {
                    LanguageCode = templanguageCode,
                    TemplateId = tempId
                };

                List<WhatsappParam> newList = templateInfo.Paramms;

                if (publishQuizTmpModel.LeadDetails != null) {

                    var exobj = publishQuizTmpModel.ContactObject;

                    if (exobj != null) {

                        var exObjpair = exobj.ToDictionary(k => k.Key.ToLower(), k => k.Value);
                        if(listheaderParams != null) {
                            foreach (var item in listheaderParams) {
                                if (exObjpair.ContainsKey(item.paraname.ToLower())) {
                                    item.value = exObjpair.GetDictionarykeyValueStringObject(item.paraname);
                                } else {

                                    item.value = item.paraname;
                                }
                            }
                        }
                      
                            var atsvacancyId = exObjpair.GetDictionarykeyValueStringObject("Lead.SourceId");

                        if (newList != null && newList.Any() && !string.IsNullOrWhiteSpace(atsvacancyId)) {
                            if (newList.Any(v => v.Paraname.ContainsCI("vacancy."))) {
                                var vacancyDetails = _owchelper.GetCoreVacancyDetails(atsvacancyId, clientCode);

                                if (!string.IsNullOrWhiteSpace(vacancyDetails)) {
                                    var vacancyObj = (JsonConvert.DeserializeObject<Dictionary<string, object>>(vacancyDetails.ToString()));
                                    // var vacancyObjPair = vacancyObj.ToDictionary(k => k.Key.ToLower(), k => k.Value);
                                    foreach (var item in vacancyObj) {
                                        exObjpair.Add("vacancy." + item.Key.ToLower(), item.Value);
                                    }
                                }
                            }
                        }

                        var templateBody = templateInfo.TemplateBody.FirstOrDefault(v => v.LangCode.EqualsCI(templanguageCode));
                        if(templateBody != null && templateBody.IsEnabledDynamicMedia && publishQuizTmpModel.LeadDetails != null) {
                            UpdateDynamicMedia(publishQuizTmpModel, clientCode, listheaderParams, templateBody);
                        }

                        if (newList != null && newList.Any() && !string.IsNullOrWhiteSpace(leadDetails.contactId)) {
                          
                          
                                if (newList.Any(v => v.Paraname.EqualsCI("recruiter") || v.Paraname.EqualsCI("SourceOwner") || v.Paraname.EqualsCI("LeadOwner"))) {
                                    _workpackageComm.AddRecruiterDetails(exObjpair, publishQuizTmpModel.CompanyDetails);
                                }

                            foreach (var item in newList) {
                                if (exObjpair.ContainsKey(item.Paraname.ToLower())) {
                                    item.Value = exObjpair.GetDictionarykeyValueStringObject(item.Paraname);
                                } else {

                                    item.Value = item.Paraname;
                                }

                                quizAnswerSubmit.QuestionDetails.Description = quizAnswerSubmit.QuestionDetails.Description.Replace(item.Paraname, item.Value?.ToString()).Replace("\n", " ");
                            }

                            quizAnswerSubmit.QuestionDetails.Description = RemoveSpecialChars(quizAnswerSubmit.QuestionDetails.Description);
                            whatsappDetail.WhatsappParam = newList;

                        }
                    }
                }
                quizAnswerSubmit.QuestionDetails.WhatsappDetails = whatsappDetail;
                quizAnswerSubmit.QuestionDetails.WhatsappDetails.HeaderParams = listheaderParams;
            }
        }

        private void UpdateDynamicMedia(PublishQuizTmpModel publishQuizTmpModel, string clientCode, List<WhatsappMessage.HeaderParameter> listheaderParams, TemplateBody templateBody) {
            string ownerExternalId = string.Empty;
            string token = null;
            var media = new List<UserMediaClassification>();

            if (templateBody.MediaVariable.MediaOwner == "CASE_OWNER") {
                ownerExternalId = publishQuizTmpModel.LeadDetails.SourceOwnerId;
            } else if (templateBody.MediaVariable.MediaOwner == "LEAD_OWNER") {
                ownerExternalId = publishQuizTmpModel.LeadDetails.LeadOwnerId;
            } else if (templateBody.MediaVariable.MediaOwner == "CONTACT_OWNER") {
                ownerExternalId = publishQuizTmpModel.LeadDetails.ContactOwnerId;
            }
            if (!string.IsNullOrWhiteSpace(ownerExternalId)) {
                var externalDetails = _owchelper.GetExternalDetails(clientCode, ownerExternalId);
                if (externalDetails != null) {
                    var oWCBusinessUserResponse = JsonConvert.DeserializeObject<OWCBusinessUserResponse>(externalDetails);
                    if (oWCBusinessUserResponse != null) {
                        token = oWCBusinessUserResponse.token;
                    }
                    if (!string.IsNullOrWhiteSpace(token)) {
                        var userTokens = new List<string>();
                        userTokens.Add(token);
                        media = OWCHelper.GetUserMediaClassification(clientCode, userTokens);
                    }
                }
            }

            var tempHeaderExample = templateBody?.HeaderExample.ToString();
            var tempProfileMedia = templateBody?.MediaVariable?.ProfileMedia;
            var tempHeaderMediaPublicUrl = templateBody?.HeaderMediaPublicId;

            if (media != null && media.Any()) {
                foreach (var item in media.Where(v => v.Name == tempProfileMedia)) {
                    if (!string.IsNullOrWhiteSpace(item.MediaUrl)) {
                        if (!item.MediaUrl.EqualsCI(tempHeaderExample)) {
                            templateBody.HeaderExample = item.MediaUrl;

                            listheaderParams.Add(new WhatsappMessage.HeaderParameter {
                                paraname = "",
                                position = 1,
                                value = item.MediaUrl

                            });

                        }
                        if (!string.IsNullOrWhiteSpace(item.MediaPublicId)) {
                            if (!item.MediaPublicId.EqualsCI(tempHeaderMediaPublicUrl)) {
                                templateBody.HeaderMediaPublicId = item.MediaPublicId;
                            }
                        }

                    }

                }

            }
        }

        private void AddRecruiterDetails(Dictionary<string, object> exObjpair, CompanyModel CompanyObj) {
            int ownerId = 0;
            if (!(exObjpair.ContainsKey("recruiter") && exObjpair["recruiter"] != null && Convert.ToString(exObjpair["recruiter"]) != null)) {
                var contactOwnerId = exObjpair.ContainsKey("contactownerid") && exObjpair["contactownerid"] != null ? Convert.ToString(exObjpair["contactownerid"]) : null;
                var sourceOwnerId = exObjpair.ContainsKey("sourceownerid") && exObjpair["sourceownerid"] != null ? Convert.ToString(exObjpair["sourceownerid"]) : null;
                if (!string.IsNullOrWhiteSpace(contactOwnerId)) {
                    ownerId = int.Parse(contactOwnerId);
                }

                if (ownerId == 0) {
                    if (!string.IsNullOrWhiteSpace(sourceOwnerId)) {
                        ownerId = int.Parse(sourceOwnerId);
                    }
                }
            }

            var userdetails = OWCHelper.GetUserListOnUserId(new long[] { ownerId }, CompanyObj).ToList();

            if (userdetails != null && userdetails.Any()) {
                var leadRecruiterObj = userdetails.FirstOrDefault();
                if (leadRecruiterObj != null) {
                    exObjpair.Add("recruiter", leadRecruiterObj.firstName + " " + leadRecruiterObj.lastName);
                }
            }
        }

        public string RemoveSpecialChars(string input)
        {
            return Regex.Replace(input, "[}{]", "");
        }

        private QuizAnswerSubmit CompleteContent(PublishQuizTmpModel publishQuizTmpModel, QuizAnswerSubmit quizAnswerSubmit)
        {
            #region fetch next content details
            using (var UOWObj = new AutomationUnitOfWork())
            {
                if (publishQuizTmpModel.IsQuesAndContentInSameTable && !publishQuizTmpModel.IsLastQuestionAttempted)
                {

                    var quizQuestionStatsObj = UOWObj.QuizQuestionStatsRepository.GetQuizQuestionStatsByQuizAttemptId(publishQuizTmpModel.QuizattemptId).Where(r => r.QuestionId == publishQuizTmpModel.RequestQuestionId && r.Status == (int)StatusEnum.Active).FirstOrDefault();
                    if (quizQuestionStatsObj != null)
                    {
                        quizQuestionStatsObj.CompletedOn = DateTime.UtcNow;
                        quizQuestionStatsObj.Status = (int)StatusEnum.Active;

                        UOWObj.QuizQuestionStatsRepository.Update(quizQuestionStatsObj);
                        UOWObj.Save();
                    }

                }
                else
                {

                    //var quizObjectStatsList = UOWObj.QuizObjectStatsRepository.GetQuizObjectStatsByQuizAttemptId(publishQuizTmpModel.QuizattemptId, (int)BranchingLogicEnum.CONTENT, publishQuizTmpModel.RequestQuestionId, (int)StatusEnum.Active);
                    var quizObjectStatsList = UOWObj.QuizObjectStatsRepository.GetQuizObjectStatsByQuizAttemptId(publishQuizTmpModel.QuizattemptId).Where(r => r.TypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == publishQuizTmpModel.RequestQuestionId && r.Status == (int)StatusEnum.Active);

                    if (!quizObjectStatsList.Any() || quizObjectStatsList == null)
                    {
                        var quizObjectStats = new Db.QuizObjectStats();

                        quizObjectStats.QuizAttemptId = publishQuizTmpModel.QuizattemptId;
                        quizObjectStats.ObjectId = publishQuizTmpModel.RequestQuestionId;
                        quizObjectStats.ViewedOn = DateTime.UtcNow;
                        quizObjectStats.TypeId = (int)BranchingLogicEnum.CONTENT;
                        quizObjectStats.Status = (int)StatusEnum.Active;

                        UOWObj.QuizObjectStatsRepository.Insert(quizObjectStats);
                        UOWObj.Save();

                    }

                }
            }

            var nextQuestionObj = FetchNextQuestion(publishQuizTmpModel.QuizDetailId, publishQuizTmpModel.IsQuesAndContentInSameTable, publishQuizTmpModel.IsBranchingLogicEnabled, publishQuizTmpModel.RequestQuestionId, (int)BranchingLogicEnum.CONTENTNEXT);
            if (nextQuestionObj != null)
            {

                quizAnswerSubmit = GetNextQuestionObjectDetails(nextQuestionObj, quizAnswerSubmit, publishQuizTmpModel, null, "complete_content");
            }
            else
            {
                quizAnswerSubmit = NextNullquestionCompleteQuestionLastResult(publishQuizTmpModel, quizAnswerSubmit);
            }
            #endregion

            return quizAnswerSubmit;
        }

        private QuizAnswerSubmit StartQuestion(PublishQuizTmpModel publishQuizTmpModel)
        {
            var currentDate = DateTime.UtcNow;
            QuizAnswerSubmit quizAnswerSubmit = new QuizAnswerSubmit();
            quizAnswerSubmit.QuestionType = (int)BranchingLogicEnum.QUESTION;
            InsertQuizQuestionStats(publishQuizTmpModel.RequestQuestionId, publishQuizTmpModel.QuizattemptId);

            return quizAnswerSubmit;
        }

        private PublishQuizTmpModel AddAttemptQuizDefaults(PublishQuizTmpModel publishQuizTmpModel)
        {
            if (string.IsNullOrEmpty(publishQuizTmpModel.RequestType))
            {

                using (var UOWObj = new AutomationUnitOfWork())
                {
                    //var quizStatsObj = UOWObj.QuizStatsRepository.Get(r => r.QuizAttemptId == publishQuizTmpModel.QuizattemptId).FirstOrDefault();
                    var quizStatsObj = UOWObj.QuizStatsRepository.GetQuizStatsQuizAttemptId(publishQuizTmpModel.QuizattemptId).FirstOrDefault();
                    if (quizStatsObj == null)
                    {
                        publishQuizTmpModel.RequestType = "fetch_quiz";
                        publishQuizTmpModel.RequestQuestionId = -1;
                        publishQuizTmpModel.RequestAnswerId = null;
                    }
                    else
                    {
                        if (quizStatsObj.CompletedOn.HasValue)
                        {
                            Status = ResultEnum.Error;
                            ErrorMessage = "Quiz has already been completed.";
                            return publishQuizTmpModel;
                        }
                        else
                        {

                            //var questionStats = UOWObj.QuizQuestionStatsRepository.Get(r => r.QuizAttemptId == publishQuizTmpModel.QuizattemptId && r.Status == (int)StatusEnum.Active).ToList();
                            //var questionStats = UOWObj.QuizQuestionStatsRepository.Get(r => r.QuizAttemptId == publishQuizTmpModel.QuizattemptId).Where(r=> r.Status == (int)StatusEnum.Active).ToList();
                            var questionStats = UOWObj.QuizQuestionStatsRepository.GetQuizQuestionStatsByQuizAttemptId(publishQuizTmpModel.QuizattemptId).Where(r => r.Status == (int)StatusEnum.Active).ToList();

                            if (questionStats.Count == 0)
                            {
                                publishQuizTmpModel.IsQuizAlreadyStarted = true;
                                publishQuizTmpModel.RequestType = "start_quiz";
                            }
                            else
                            {
                                var lastAttemptedQuestion = questionStats.OrderByDescending(r => r.StartedOn).FirstOrDefault();

                                if (lastAttemptedQuestion != null && lastAttemptedQuestion.CompletedOn.HasValue)
                                {
                                    publishQuizTmpModel.RequestType = "complete_question";
                                    publishQuizTmpModel.IsLastQuestionAttempted = true;
                                    publishQuizTmpModel.RequestQuestionId = lastAttemptedQuestion.QuestionId;
                                    foreach (var obj in lastAttemptedQuestion.QuizAnswerStats)
                                    {
                                        publishQuizTmpModel.RequestAnswerId.Add(obj.AnswerId);
                                    }
                                }
                                else
                                {
                                    if (questionStats.Count == 1)
                                    {
                                        publishQuizTmpModel.IsQuizAlreadyStarted = true;
                                        publishQuizTmpModel.IsLastQuestionStarted = true;
                                        publishQuizTmpModel.RequestType = "start_quiz";
                                    }
                                    else
                                    {
                                        publishQuizTmpModel.IsLastQuestionAttempted = true;
                                        publishQuizTmpModel.IsLastQuestionStarted = true;
                                        publishQuizTmpModel.IsrevealScore = false;

                                        lastAttemptedQuestion = questionStats.OrderByDescending(r => r.StartedOn).Skip(1).Take(1).FirstOrDefault();
                                        publishQuizTmpModel.RequestQuestionId = lastAttemptedQuestion.QuestionId;

                                        if (lastAttemptedQuestion.QuestionsInQuiz.Type == (int)BranchingLogicEnum.QUESTION)
                                            publishQuizTmpModel.RequestType = "complete_question";
                                        else
                                            publishQuizTmpModel.RequestType = "complete_content";

                                        foreach (var obj in lastAttemptedQuestion.QuizAnswerStats)
                                        {
                                            publishQuizTmpModel.RequestAnswerId.Add(obj.AnswerId);
                                        }
                                    }
                                }

                            }
                        }
                    }
                }
            }

            return publishQuizTmpModel;
        }

        private QuizAnswerSubmit StartQuiz(PublishQuizTmpModel publishQuizTmpModel, QuizAnswerSubmit quizAnswerSubmit)
        {
            int quizstatId = 0;
            if (!publishQuizTmpModel.IsQuizAlreadyStarted)
            {
                if (!(publishQuizTmpModel.RequestUsageType != null && publishQuizTmpModel.RequestUsageType == (int)UsageTypeEnum.Chatbot))
                {
                    UpdateQuizAttemptViewed(publishQuizTmpModel.QuizattemptId, publishQuizTmpModel.CompanyId);
                }
                quizstatId = InsertQuizStat(publishQuizTmpModel.QuizattemptId, null, DateTime.UtcNow);
                if (!string.IsNullOrWhiteSpace(publishQuizTmpModel.LeadUserId))
                {
                    SyncQuizAttemptWithleadTask(publishQuizTmpModel);
                }
            }

            var nextQuestionObj = FetchNextQuestion(publishQuizTmpModel.QuizDetailId, publishQuizTmpModel.IsQuesAndContentInSameTable, publishQuizTmpModel.IsBranchingLogicEnabled);
            if (nextQuestionObj == null)
            {
                #region update complete quiz status

                using (var UOWObj = new AutomationUnitOfWork())
                {
                    //var quizStatsObj = UOWObj.QuizStatsRepository.Get(r => r.QuizAttemptId == publishQuizTmpModel.QuizattemptId).FirstOrDefault();
                    var quizStatsObj = UOWObj.QuizStatsRepository.GetQuizStatsQuizAttemptId(publishQuizTmpModel.QuizattemptId).FirstOrDefault();
                    if (quizStatsObj != null)
                    {
                        quizStatsObj.CompletedOn = DateTime.UtcNow;
                        UOWObj.QuizStatsRepository.Update(quizStatsObj);
                        UOWObj.Save();
                    }
                }

                #endregion

                return quizAnswerSubmit;
            }
            else
            {

                quizAnswerSubmit = GetNextQuestionObjectDetails(nextQuestionObj, quizAnswerSubmit, publishQuizTmpModel, null, publishQuizTmpModel.RequestType);
                return quizAnswerSubmit;
            }
        }



        private QuizAnswerSubmit GetNextQuestionObjectDetails(object nextQuestionObj, QuizAnswerSubmit quizAnswerSubmit, PublishQuizTmpModel publishQuizTmpModel, Db.QuestionsInQuiz questionsInQuizStat, string requestType)
        {

            var nextQuestionObjectName = nextQuestionObj.GetType().BaseType.Name;
            if (nextQuestionObjectName == "QuestionsInQuiz")
            {
                var questionsInQuizObject = (Db.QuestionsInQuiz)nextQuestionObj;
                if (questionsInQuizObject.Type == (int)BranchingLogicEnum.QUESTION || questionsInQuizObject.Type == (int)BranchingLogicEnum.WHATSAPPTEMPLATE)
                {
                    quizAnswerSubmit.QuestionType = (int)BranchingLogicEnum.QUESTION;
                    var quizQuestion = NextQuesTypeQues(publishQuizTmpModel, questionsInQuizObject, false);
                    if (quizQuestion != null)
                        quizAnswerSubmit.QuestionDetails = quizQuestion;

                    quizAnswerSubmit.IsBackButtonEnable = false;

                    if (requestType.EqualsCI("complete_question"))
                    {
                        if (questionsInQuizStat != null)
                        {
                            quizAnswerSubmit.IsBackButtonEnable = (questionsInQuizStat.RevealCorrectAnswer.HasValue && questionsInQuizStat.RevealCorrectAnswer.Value) ? false : (questionsInQuizStat.ViewPreviousQuestion || questionsInQuizStat.EditAnswer);
                        }
                    }
                    else if (requestType.EqualsCI("complete_content"))
                    {
                        if (publishQuizTmpModel.IsQuesAndContentInSameTable)
                        {
                            using (var UOWObj = new AutomationUnitOfWork())
                            {
                                //todo : need extension menthod
                                //var questionpreviousExist = UOWObj.QuestionsInQuizRepository.Get(v => v.QuizId == publishQuizTmpModel.QuizDetailId && v.Id == publishQuizTmpModel.RequestQuestionId).FirstOrDefault()?.ViewPreviousQuestion;
                                var questionpreviousExist = UOWObj.QuestionsInQuizRepository.GetExistingQuestioninQuiz(publishQuizTmpModel.QuizDetailId).Where(v => v.Id == publishQuizTmpModel.RequestQuestionId).FirstOrDefault()?.ViewPreviousQuestion;
                                if (questionpreviousExist.HasValue)
                                {
                                    quizAnswerSubmit.IsBackButtonEnable = questionpreviousExist.Value;
                                }
                            }
                        }
                        else
                        {
                            using (var UOWObj = new AutomationUnitOfWork())
                            {
                                //todo : need extension menthod
                                //var questionpreviousExist = UOWObj.ContentsInQuizRepository.Get(v => v.QuizId == publishQuizTmpModel.QuizDetailId && v.Id == publishQuizTmpModel.RequestQuestionId).FirstOrDefault()?.ViewPreviousQuestion;
                                var questionpreviousExist = UOWObj.ContentsInQuizRepository.GetContentInQuizRepositoryExtension(publishQuizTmpModel.QuizDetailId).Where(v => v.Id == publishQuizTmpModel.RequestQuestionId).FirstOrDefault()?.ViewPreviousQuestion;

                                if (questionpreviousExist.HasValue)
                                {
                                    quizAnswerSubmit.IsBackButtonEnable = questionpreviousExist.Value;
                                }
                            }
                        }
                    }

                    var previousQuestionAnswerDeatils = PreviousQuestion(publishQuizTmpModel, questionsInQuizObject);
                    if (previousQuestionAnswerDeatils != null)
                        quizAnswerSubmit.PreviousQuestionSubmittedAnswer = previousQuestionAnswerDeatils;

                    if (!publishQuizTmpModel.IsLastQuestionStarted && !requestType.Equals("complete_question"))
                    {
                        InsertQuizQuestionStats(questionsInQuizObject.Id, publishQuizTmpModel.QuizattemptId);
                    }

                    if (!publishQuizTmpModel.IsLastQuestionStarted && requestType.Equals("complete_question") && questionsInQuizStat != null)
                    {
                        if ((publishQuizTmpModel.RequestUsageType.HasValue && (publishQuizTmpModel.RequestUsageType.Value == (int)UsageTypeEnum.Chatbot || publishQuizTmpModel.RequestUsageType.Value == (int)UsageTypeEnum.WhatsAppChatbot)) || (!(questionsInQuizStat.RevealCorrectAnswer.HasValue && questionsInQuizStat.RevealCorrectAnswer.Value)))
                        {
                            InsertQuizQuestionStats(questionsInQuizObject.Id, publishQuizTmpModel.QuizattemptId);
                        }
                    }
                }

                else if (questionsInQuizObject.Type == (int)BranchingLogicEnum.CONTENT)
                {

                    if (!publishQuizTmpModel.IsLastQuestionStarted)
                    {
                        InsertQuizQuestionStats(questionsInQuizObject.Id, publishQuizTmpModel.QuizattemptId);
                    }


                    quizAnswerSubmit.QuestionType = (int)BranchingLogicEnum.CONTENT;
                    var quizContent = NextQuesTypeContent(publishQuizTmpModel, questionsInQuizObject);
                    if (quizContent != null)
                        quizAnswerSubmit.ContentDetails = quizContent;


                    if (requestType.EqualsCI("complete_question") && questionsInQuizStat != null)
                    {
                        quizAnswerSubmit.IsBackButtonEnable = (questionsInQuizStat.RevealCorrectAnswer.HasValue && questionsInQuizStat.RevealCorrectAnswer.Value) ? false : (questionsInQuizStat.ViewPreviousQuestion || questionsInQuizStat.EditAnswer);
                    }
                    else if (requestType.EqualsCI("complete_content"))
                    {
                        if (publishQuizTmpModel.IsQuesAndContentInSameTable)
                        {

                            using (var UOWObj = new AutomationUnitOfWork())
                            {
                                //var content = UOWObj.QuestionsInQuizRepository.Get(v => v.QuizId == publishQuizTmpModel.QuizDetailId && v.Id == publishQuizTmpModel.RequestQuestionId).FirstOrDefault();
                                var content = UOWObj.QuestionsInQuizRepository.GetExistingQuestioninQuiz(publishQuizTmpModel.QuizDetailId).Where(v => v.Id == publishQuizTmpModel.RequestQuestionId).FirstOrDefault();
                                if (content != null)
                                {
                                    quizAnswerSubmit.IsBackButtonEnable = content.ViewPreviousQuestion;
                                }
                            }
                        }
                    }
                }
            }

            if (nextQuestionObjectName == "QuizResults")
            {
                var questionsInQuizObject = (Db.QuizResults)nextQuestionObj;
                quizAnswerSubmit = QuestionObjectQuizResult(quizAnswerSubmit, publishQuizTmpModel, questionsInQuizObject);
                quizAnswerSubmit.QuestionType = (int)BranchingLogicEnum.RESULT;

            }

            if (nextQuestionObjectName == "ContentsInQuiz")
            {
                quizAnswerSubmit.QuestionType = (int)BranchingLogicEnum.CONTENT;
                var contentsInQuizObject = (Db.ContentsInQuiz)nextQuestionObj;

                bool isCompleteContentRedirection = false;
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    //var quizstattype = UOWObj.QuizObjectStatsRepository.Get(a => a.QuizAttemptId == publishQuizTmpModel.QuizattemptId && a.Status == (int)StatusEnum.Active && a.TypeId == (int)BranchingLogicEnum.CONTENT && a.ObjectId == ((Db.ContentsInQuiz)nextQuestionObj).Id).FirstOrDefault();
                    var quizstattype = UOWObj.QuizObjectStatsRepository.GetQuizObjectStatsByQuizAttemptId(publishQuizTmpModel.QuizattemptId).Where(a => a.Status == (int)StatusEnum.Active && a.TypeId == (int)BranchingLogicEnum.CONTENT && a.ObjectId == ((Db.ContentsInQuiz)nextQuestionObj).Id).FirstOrDefault();
                    if (quizstattype != null)
                    {
                        isCompleteContentRedirection = true;
                    }

                    if (requestType.EqualsCI("complete_question") && questionsInQuizStat != null)
                    {
                        quizAnswerSubmit.IsBackButtonEnable = (questionsInQuizStat.RevealCorrectAnswer.HasValue && questionsInQuizStat.RevealCorrectAnswer.Value) ? false : (questionsInQuizStat.ViewPreviousQuestion || questionsInQuizStat.EditAnswer);
                    }
                    else if (requestType.EqualsCI("complete_content"))
                    {
                        //var content = UOWObj.ContentsInQuizRepository.Get(v => v.QuizId == publishQuizTmpModel.QuizDetailId && v.Id == publishQuizTmpModel.RequestQuestionId).FirstOrDefault();
                        var content = UOWObj.ContentsInQuizRepository.GetContentInQuizRepositoryExtension(publishQuizTmpModel.QuizDetailId).Where(v => v.Id == publishQuizTmpModel.RequestQuestionId).FirstOrDefault();
                        if (content != null)
                        {
                            quizAnswerSubmit.IsBackButtonEnable = content.ViewPreviousQuestion;

                        }
                    }
                }

                if (isCompleteContentRedirection)
                {
                    publishQuizTmpModel.RequestQuestionId = contentsInQuizObject.Id;
                    quizAnswerSubmit = CompleteContent(publishQuizTmpModel, quizAnswerSubmit);
                    return quizAnswerSubmit;
                }

                quizAnswerSubmit.ContentDetails = ContentInQuiz(publishQuizTmpModel, contentsInQuizObject);


            }

            if (publishQuizTmpModel.RecruiterUserId > 0)
            {
                if (nextQuestionObj.GetType().BaseType.Name == "BadgesInQuiz")
                {
                    quizAnswerSubmit.QuestionType = (int)BranchingLogicEnum.BADGE;
                    var badgesInQuizObject = (Db.BadgesInQuiz)nextQuestionObj;
                    quizAnswerSubmit.BadgeDetails = BadgesInQuiz(publishQuizTmpModel, badgesInQuizObject);
                }
            }

            return quizAnswerSubmit;
        }

        public QuizAnswerSubmit NotCompletedQuiz(PublishQuizTmpModel publishQuizTmpModel) 
        {
            Db.QuizStats quizStatsObj = null;
            List<Db.QuizStats> quizStatsObjList = null;
            QuizAnswerSubmit quizAnswerSubmit = new QuizAnswerSubmit();
            int? resultId = null;
            using (var UOWObj = new AutomationUnitOfWork()) 
            {
                var quizstats = UOWObj.QuizStatsRepository.GetQuizStatsQuizAttemptId(publishQuizTmpModel.QuizattemptId);
                quizStatsObj = quizstats.FirstOrDefault();
                if (quizStatsObj != null) 
                {
                    resultId = quizStatsObj.ResultId;
                    quizStatsObjList = quizstats.ToList();
                    if (quizStatsObjList != null && quizStatsObjList.Any()) 
                    {
                        foreach (var obj in quizStatsObjList) 
                        {
                            obj.CompletedOn = DateTime.UtcNow;
                            obj.AttemptStatus = 4;
                            UOWObj.QuizStatsRepository.Update(obj);
                        }
                        UOWObj.Save();
                    }
                }
            }

            var UpdateQuizStatusObj = new LeadQuizStatus();
            try {
                UpdateQuizStatusObj.AutomationTitle = publishQuizTmpModel.QuizTitle;

                UpdateQuizStatusObj.StartedDate = quizStatsObj.StartedOn != null ? quizStatsObj.StartedOn.ToString("yyyy-MM-dd'T'HH:mm:ss") : string.Empty;
                UpdateQuizStatusObj.AttemptDate = quizStatsObj.CompletedOn != null ? quizStatsObj.CompletedOn.Value.ToString("yyyy-MM-dd'T'HH:mm:ss") : string.Empty;

                UpdateQuizStatusObj.ContactId = publishQuizTmpModel.LeadUserId;
                UpdateQuizStatusObj.ClientCode = publishQuizTmpModel.CompanyCode;
                UpdateQuizStatusObj.QuizId = publishQuizTmpModel.ParentQuizid;
                UpdateQuizStatusObj.QuizStatus = "4";
                UpdateQuizStatusObj.CreatedDate = publishQuizTmpModel.WorkpackageInfoCreatedOn.HasValue ? publishQuizTmpModel.WorkpackageInfoCreatedOn.Value.ToString() : publishQuizTmpModel.QuizAttemptCreatedOn.ToString();
                UpdateQuizStatusObj.QuizType = publishQuizTmpModel.QuizType;
                UpdateQuizStatusObj.RequestId = publishQuizTmpModel.RequestId;
                UpdateQuizStatusObj.Results = new List<Result>();
                UpdateQuizStatusObj.FieldsToUpdate = new List<Fields>();

                using (var UOWObj = new AutomationUnitOfWork()) {
                var attemptedQuestions = UOWObj.QuizQuestionStatsRepository.Get(t => t.QuizAttemptId == publishQuizTmpModel.QuizattemptId);
                foreach (var questionObj in attemptedQuestions) {
                    var ansType = questionObj.QuestionsInQuiz.AnswerType;
                    if (ansType == (int)AnswerTypeEnum.FullAddress) {
                        var answerObjForPostCode = questionObj.QuizAnswerStats.FirstOrDefault(r => r.SubAnswerTypeId == (int)SubAnswerTypeEnum.PostCode);

                        if (answerObjForPostCode != null && answerObjForPostCode.AnswerOptionsInQuizQuestions.ObjectFieldsInAnswer.Any()) {
                            var objectFieldsInAnswer = answerObjForPostCode.AnswerOptionsInQuizQuestions.ObjectFieldsInAnswer.FirstOrDefault();
                            UpdateQuizStatusObj.FieldsToUpdate.Add(new Fields {
                                ObjectName = objectFieldsInAnswer.ObjectName,
                                FieldName = objectFieldsInAnswer.FieldName,
                                Value = answerObjForPostCode?.AnswerText.Trim()
                            });
                        }

                        var answerObjForHouseNumber = questionObj.QuizAnswerStats.FirstOrDefault(r => r.SubAnswerTypeId == (int)SubAnswerTypeEnum.HouseNumber);

                        if (answerObjForHouseNumber != null && answerObjForHouseNumber.AnswerOptionsInQuizQuestions.ObjectFieldsInAnswer.Any()) {
                            var objectFieldsInAnswer = answerObjForHouseNumber.AnswerOptionsInQuizQuestions.ObjectFieldsInAnswer.FirstOrDefault();
                            UpdateQuizStatusObj.FieldsToUpdate.Add(new Fields {
                                ObjectName = objectFieldsInAnswer.ObjectName,
                                FieldName = objectFieldsInAnswer.FieldName,
                                Value = string.Join(",", questionObj.QuizAnswerStats.Where(r => r.SubAnswerTypeId == (int)SubAnswerTypeEnum.HouseNumber || r.SubAnswerTypeId == (int)SubAnswerTypeEnum.Address).Select(r => r.AnswerText))
                            });
                        }
                    }
                    else if (ansType == (int)AnswerTypeEnum.Availability) {
                        var answerObjForAvailability = questionObj.QuizAnswerStats.FirstOrDefault(r => r.QuizQuestionStatsId == questionObj.Id);

                        if (answerObjForAvailability != null && answerObjForAvailability.AnswerOptionsInQuizQuestions.ObjectFieldsInAnswer.Any()) {
                            var objectFieldsInAnswer = answerObjForAvailability.AnswerOptionsInQuizQuestions.ObjectFieldsInAnswer.FirstOrDefault();
                            UpdateQuizStatusObj.FieldsToUpdate.Add(new Fields {
                                ObjectName = objectFieldsInAnswer.ObjectName,
                                FieldName = objectFieldsInAnswer.FieldName,
                                Value = answerObjForAvailability?.Comment.Trim()
                            });
                        }
                    }

                    else {
                        foreach (var answerObj in questionObj.QuizAnswerStats.ToList()) {
                            var templateId = questionObj.QuestionsInQuiz.TemplateId;
                            if (templateId.HasValue && templateId.Value > 0) {
                                try {
                                    var whatsAppHSMTemplates = _owchelper.GetWhatsAppHSMTemplates(publishQuizTmpModel.CompanyCode, "chatbot", language: questionObj.QuestionsInQuiz.LanguageCode);
                                    if (whatsAppHSMTemplates != null) {
                                        var hsmTemplatesList = JsonConvert.DeserializeObject<List<WhatsappHSMTemplateModel>>(whatsAppHSMTemplates.ToString());
                                        if (hsmTemplatesList != null && hsmTemplatesList.Any(v => v.id == templateId)) {
                                            var customComponents = hsmTemplatesList.Where(v => v.id == templateId && v.customComponents != null).Select(v => v.customComponents).FirstOrDefault();
                                            if (customComponents != null) {
                                                foreach (var item in customComponents) {
                                                    if (item.type.EqualsCI("buttons") && item.items != null) {
                                                        var mappedfields = item.items.Where(v => v.id == answerObj.AnswerOptionsInQuizQuestions.RefId).Select(v => v.mappedFields).FirstOrDefault();
                                                        if (mappedfields != null && mappedfields.Any()) {
                                                            foreach (var mapped in mappedfields) {
                                                                UpdateQuizStatusObj.FieldsToUpdate.Add(new Fields {
                                                                    ObjectName = mapped.objectName,
                                                                    FieldName = mapped.fieldName,
                                                                    Value = mapped?.value.Trim()
                                                                });
                                                            }
                                                        }

                                                    }
                                                }

                                            }

                                        }
                                    }
                                } catch (Exception ex) {
                                }
                            }
                            else {
                                if (answerObj.AnswerOptionsInQuizQuestions.ObjectFieldsInAnswer.Any()) {
                                    var objectFieldsInAnswer = answerObj.AnswerOptionsInQuizQuestions.ObjectFieldsInAnswer.FirstOrDefault();

                                    if (objectFieldsInAnswer.IsExternalSync.HasValue && objectFieldsInAnswer.IsExternalSync.Value) {

                                        var valueitem = !string.IsNullOrWhiteSpace(objectFieldsInAnswer.Value) ? objectFieldsInAnswer.Value : answerObj.AnswerText;
                                        UpdateQuizStatusObj.FieldsToUpdate.Add(new Fields {
                                            ObjectName = objectFieldsInAnswer.ObjectName,
                                            FieldName = objectFieldsInAnswer.FieldName,
                                            Value = valueitem?.Trim()
                                        });
                                    }

                                    //var valueitem = (ansType == (int)AnswerTypeEnum.Single || ansType == (int)AnswerTypeEnum.Multiple || ansType == (int)AnswerTypeEnum.DrivingLicense || ansType == (int)AnswerTypeEnum.LookingforJobs)
                                    //  ? objectFieldsInAnswer.Value : answerObj.AnswerText;

                                }
                            }
                        }
                    }
                }
            }


            } catch (Exception) {
            }

            if (quizStatsObj == null) 
            {
                return null;
            }

            if (!string.IsNullOrEmpty(publishQuizTmpModel.LeadUserId)) 
            {
                UpdateQuizStatusObj.AutomationTitle = publishQuizTmpModel.QuizTitle;

                UpdateQuizStatusObj.StartedDate = quizStatsObj.StartedOn != null ? quizStatsObj.StartedOn.ToString("yyyy-MM-dd'T'HH:mm:ss") : string.Empty;
                UpdateQuizStatusObj.AttemptDate = quizStatsObj.CompletedOn != null ? quizStatsObj.CompletedOn.Value.ToString("yyyy-MM-dd'T'HH:mm:ss") : string.Empty;

                UpdateQuizStatusObj.ContactId = publishQuizTmpModel.LeadUserId;
                UpdateQuizStatusObj.ClientCode = publishQuizTmpModel.CompanyCode;
                UpdateQuizStatusObj.QuizId = publishQuizTmpModel.ParentQuizid;
                UpdateQuizStatusObj.QuizStatus = "4";
                UpdateQuizStatusObj.CreatedDate = publishQuizTmpModel.WorkpackageInfoCreatedOn.HasValue ? publishQuizTmpModel.WorkpackageInfoCreatedOn.Value.ToString("yyyy-MM-dd'T'HH:mm:ss") : publishQuizTmpModel.QuizAttemptCreatedOn.ToString("yyyy-MM-dd'T'HH:mm:ss");
                UpdateQuizStatusObj.QuizType = publishQuizTmpModel.QuizType;
                UpdateQuizStatusObj.Results = new List<Result>();
                 using (var UOWObj = new AutomationUnitOfWork()) 
                {
                    var configurationDetails = UOWObj.ConfigurationDetailsRepository.GetByID(publishQuizTmpModel.ConfigurationDetailId);
                    if (configurationDetails != null) 
                    {
                        UpdateQuizStatusObj.SourceId = configurationDetails.SourceId;
                        UpdateQuizStatusObj.ConfigurationId = configurationDetails.ConfigurationId;

                    }
                }

                var urlhub = GlobalSettings.HubUrl.ToString();
                var token = GlobalSettings.HubUrlBearer.ToString();
                var urlAutomation = urlhub + "/api/v1/Automations/web-hooks/UpdateAutomationStatus";
                var urlAppointment = urlhub + "/api/v1/appointments/web-hooks/UpdateAppointmentStatus";

                try {
                    var apiSuccess = false;
                    var res = OWCHelper.UpdateQuizStatus(UpdateQuizStatusObj);
                    if (res != null) {
                        apiSuccess = res.status == "true";
                        quizAnswerSubmit.AppointmentCode = res.appointmentCode;
                        quizAnswerSubmit.AppointmentLink = res.appointmentLink;
                        quizAnswerSubmit.AppointmentErrorMessage = res.message;
                    }

                    if (!apiSuccess)
                        AddPendingApi(urlAutomation, token, JsonConvert.SerializeObject(UpdateQuizStatusObj), "POST");
                } catch (Exception) {
                    AddPendingApi(urlAutomation, token, JsonConvert.SerializeObject(UpdateQuizStatusObj), "POST");
                }
            }
            return quizAnswerSubmit;
        }

        public QuizAnswerSubmit FailedQuiz(FailedQuiz failedQuiz) {
            QuizAnswerSubmit quizAnswerSubmit = new QuizAnswerSubmit();
            int? resultId = null;

            var UpdateQuizStatusObj = new LeadQuizStatus();          
            if (!string.IsNullOrEmpty(failedQuiz.LeadUserId)) {

                UpdateQuizStatusObj.ContactId = failedQuiz.LeadUserId;
                UpdateQuizStatusObj.ClientCode = failedQuiz.CompanyCode;
                UpdateQuizStatusObj.QuizStatus = "5";
                UpdateQuizStatusObj.QuizId = failedQuiz.QuizId;
                UpdateQuizStatusObj.Results = new List<Result>();

                var urlhub = GlobalSettings.HubUrl.ToString();
                var token = GlobalSettings.HubUrlBearer.ToString();
                var urlAutomation = urlhub + "/api/v1/Automations/web-hooks/UpdateAutomationStatus";
                
                try {
                    var apiSuccess = false;
                    var res = OWCHelper.UpdateQuizStatus(UpdateQuizStatusObj);
                    if (res != null) {
                        apiSuccess = res.status == "true";
                        quizAnswerSubmit.AppointmentCode = res.appointmentCode;
                        quizAnswerSubmit.AppointmentLink = res.appointmentLink;
                        quizAnswerSubmit.AppointmentErrorMessage = res.message;

                        if (res != null && !string.IsNullOrWhiteSpace(res.appointmentCode)) {

                            var followUpMessage = OWCHelper.GetFollowUpMessageByCode(failedQuiz.CompanyCode, res.appointmentCode);
                        }

                    }

                    if (!apiSuccess)
                        AddPendingApi(urlAutomation, token, JsonConvert.SerializeObject(UpdateQuizStatusObj), "POST");
                } catch (Exception) {
                    AddPendingApi(urlAutomation, token, JsonConvert.SerializeObject(UpdateQuizStatusObj), "POST");
                }
            }
            return quizAnswerSubmit;
        }

        
        private QuizAnswerSubmit CompleteQuiz(PublishQuizTmpModel publishQuizTmpModel)
        {
            Db.QuizStats quizStatsObj = null;
            List<Db.QuizStats> quizStatsObjList = null;
            QuizAnswerSubmit quizAnswerSubmit = new QuizAnswerSubmit();
            int? resultId = null;
            using (var UOWObj = new AutomationUnitOfWork())
            {
                //var  quizstats = UOWObj.QuizStatsRepository.Get(r => r.QuizAttemptId == publishQuizTmpModel.QuizattemptId);
                var quizstats = UOWObj.QuizStatsRepository.GetQuizStatsQuizAttemptId(publishQuizTmpModel.QuizattemptId);
                quizStatsObj = quizstats.FirstOrDefault();
                if (quizStatsObj != null)
                {
                    resultId = quizStatsObj.ResultId;
                    quizStatsObjList = quizstats.ToList();
                    if (quizStatsObjList != null && quizStatsObjList.Any())
                    {
                        foreach (var obj in quizStatsObjList)
                        {
                            obj.CompletedOn = DateTime.UtcNow;
                            UOWObj.QuizStatsRepository.Update(obj);
                        }
                        UOWObj.Save();
                    }
                }
            }

            if (quizStatsObj == null)
            {
                return null;
            }

            PushAppointment(publishQuizTmpModel, resultId);
           
            SaveLeadTags(publishQuizTmpModel);


            if (!string.IsNullOrEmpty(publishQuizTmpModel.LeadUserId))
            {
                var UpdateQuizStatusObj = new LeadQuizStatus();

                UpdateQuizStatusObj.AutomationTitle = publishQuizTmpModel.QuizTitle;

                UpdateQuizStatusObj.StartedDate = quizStatsObj.StartedOn != null ? quizStatsObj.StartedOn.ToString("yyyy-MM-dd'T'HH:mm:ss") : string.Empty;
                UpdateQuizStatusObj.AttemptDate = quizStatsObj.CompletedOn != null ? quizStatsObj.CompletedOn.Value.ToString("yyyy-MM-dd'T'HH:mm:ss") : string.Empty;

                UpdateQuizStatusObj.ContactId = publishQuizTmpModel.LeadUserId;
                UpdateQuizStatusObj.ClientCode = publishQuizTmpModel.CompanyCode;
                UpdateQuizStatusObj.QuizId = publishQuizTmpModel.ParentQuizid;
                UpdateQuizStatusObj.QuizStatus = "3";
                UpdateQuizStatusObj.CreatedDate = publishQuizTmpModel.WorkpackageInfoCreatedOn.HasValue ? publishQuizTmpModel.WorkpackageInfoCreatedOn.Value.ToString("yyyy-MM-dd'T'HH:mm:ss") : publishQuizTmpModel.QuizAttemptCreatedOn.ToString("yyyy-MM-dd'T'HH:mm:ss");
                UpdateQuizStatusObj.QuizType = publishQuizTmpModel.QuizType;
                UpdateQuizStatusObj.RequestId = publishQuizTmpModel.RequestId;
                UpdateQuizStatusObj.Results = new List<Result>();
                UpdateQuizStatusObj.FieldsToUpdate = new List<Fields>();
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var configurationDetails = UOWObj.ConfigurationDetailsRepository.GetByID(publishQuizTmpModel.ConfigurationDetailId);
                    if (configurationDetails != null)
                    {

                        UpdateQuizStatusObj.SourceId = configurationDetails.SourceId;
                        UpdateQuizStatusObj.ConfigurationId = configurationDetails.ConfigurationId;

                    }


                    //var quizComponentLogsList = UOWObj.QuizComponentLogsRepository.Get(r =>r.QuizId == publishQuizTmpModel.QuizDetailId &&  r.ObjectTypeId == (int)BranchingLogicEnum.RESULT);
                    var quizComponentLogsList = UOWObj.QuizComponentLogsRepository.GetQuizComponentLogsByQuizId(publishQuizTmpModel.QuizDetailId, (int)BranchingLogicEnum.RESULT);

                    foreach (var resultObj in quizStatsObjList.Where(r => r.ResultId != null))
                    {
                        string resultTitle = string.Empty;
                        if (resultObj.ResultId.HasValue && resultObj.ResultId.Value > 0)
                        {
                            var quizResultsObj = UOWObj.QuizResultsRepository.GetByID(resultObj.ResultId);
                            if (quizResultsObj != null)
                            {
                                resultTitle = !string.IsNullOrWhiteSpace(quizResultsObj.InternalTitle) ? quizResultsObj.InternalTitle : quizResultsObj.Title;
                            }

                        }


                        UpdateQuizStatusObj.Results.Add(new Result
                        {
                            ParentResultId = quizComponentLogsList.Any(r => r.PublishedObjectId == resultObj.ResultId.Value) ? quizComponentLogsList.FirstOrDefault(r => r.PublishedObjectId == resultObj.ResultId.Value).DraftedObjectId : 0,
                            ResultId = resultObj.ResultId.ToIntValue(),
                            ResultTitle = resultTitle
                        });
                    }
                }

                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var attemptedQuestions = UOWObj.QuizQuestionStatsRepository.Get(t => t.QuizAttemptId == publishQuizTmpModel.QuizattemptId).Where(v => v.Status == (int)StatusEnum.Active);
                    foreach (var questionObj in attemptedQuestions)
                    {
                        var ansType = questionObj.QuestionsInQuiz.AnswerType;
                        if (ansType == (int)AnswerTypeEnum.FullAddress)
                        {
                            var answerObjForPostCode = questionObj.QuizAnswerStats.FirstOrDefault(r => r.SubAnswerTypeId == (int)SubAnswerTypeEnum.PostCode);

                            if (answerObjForPostCode != null && answerObjForPostCode.AnswerOptionsInQuizQuestions.ObjectFieldsInAnswer.Any())
                            {
                                var objectFieldsInAnswer = answerObjForPostCode.AnswerOptionsInQuizQuestions.ObjectFieldsInAnswer.FirstOrDefault();
                                UpdateQuizStatusObj.FieldsToUpdate.Add(new Fields
                                {
                                    ObjectName = objectFieldsInAnswer.ObjectName,
                                    FieldName = objectFieldsInAnswer.FieldName,
                                    Value = answerObjForPostCode?.AnswerText.Trim()
                                });
                            }

                            var answerObjForHouseNumber = questionObj.QuizAnswerStats.FirstOrDefault(r => r.SubAnswerTypeId == (int)SubAnswerTypeEnum.HouseNumber);

                            if (answerObjForHouseNumber != null && answerObjForHouseNumber.AnswerOptionsInQuizQuestions.ObjectFieldsInAnswer.Any())
                            {
                                var objectFieldsInAnswer = answerObjForHouseNumber.AnswerOptionsInQuizQuestions.ObjectFieldsInAnswer.FirstOrDefault();
                                UpdateQuizStatusObj.FieldsToUpdate.Add(new Fields
                                {
                                    ObjectName = objectFieldsInAnswer.ObjectName,
                                    FieldName = objectFieldsInAnswer.FieldName,
                                    Value = string.Join(",", questionObj.QuizAnswerStats.Where(r => r.SubAnswerTypeId == (int)SubAnswerTypeEnum.HouseNumber || r.SubAnswerTypeId == (int)SubAnswerTypeEnum.Address).Select(r => r.AnswerText))
                                });
                            }
                        }
                        else if (ansType == (int)AnswerTypeEnum.Availability)
                        {
                            var answerObjForAvailability = questionObj.QuizAnswerStats.FirstOrDefault(r => r.QuizQuestionStatsId == questionObj.Id);
                            
                            if(answerObjForAvailability != null && answerObjForAvailability.AnswerOptionsInQuizQuestions.ObjectFieldsInAnswer.Any())
                            {
                                var objectFieldsInAnswer = answerObjForAvailability.AnswerOptionsInQuizQuestions.ObjectFieldsInAnswer.FirstOrDefault();
                                UpdateQuizStatusObj.FieldsToUpdate.Add(new Fields
                                {
                                    ObjectName = objectFieldsInAnswer.ObjectName,
                                    FieldName = objectFieldsInAnswer.FieldName,
                                    Value = answerObjForAvailability?.Comment.Trim()
                                });
                            }
                        }

                        else
                        {
                            foreach (var answerObj in questionObj.QuizAnswerStats.ToList())
                            {
                                var templateId = questionObj.QuestionsInQuiz.TemplateId;
                                if (templateId.HasValue && templateId.Value > 0)
                                {
                                    try
                                    {
                                        var whatsAppHSMTemplates = _owchelper.GetWhatsAppHSMTemplates(publishQuizTmpModel.CompanyCode, "chatbot", language: questionObj.QuestionsInQuiz.LanguageCode);
                                        if (whatsAppHSMTemplates != null)
                                        {
                                            var hsmTemplatesList = JsonConvert.DeserializeObject<List<WhatsappHSMTemplateModel>>(whatsAppHSMTemplates.ToString());
                                            if (hsmTemplatesList != null && hsmTemplatesList.Any(v => v.id == templateId))
                                            {
                                                var customComponents = hsmTemplatesList.Where(v => v.id == templateId && v.customComponents != null ).Select(v =>v.customComponents).FirstOrDefault();
                                                if (customComponents != null)
                                                {
                                                    foreach (var item in customComponents)
                                                    {
                                                        if (item.type.EqualsCI("buttons") && item.items !=null)
                                                        {
                                                            var mappedfields  = item.items.Where(v => v.id == answerObj.AnswerOptionsInQuizQuestions.RefId).Select(v => v.mappedFields).FirstOrDefault();
                                                            if (mappedfields != null && mappedfields.Any())
                                                            {
                                                                foreach (var mapped in mappedfields)
                                                                {
                                                                    UpdateQuizStatusObj.FieldsToUpdate.Add(new Fields
                                                                    {
                                                                        ObjectName = mapped.objectName,
                                                                        FieldName = mapped.fieldName,
                                                                        Value = mapped?.value.Trim()
                                                                    });
                                                                }
                                                            }

                                                        }
                                                    }
                                                
                                                }
                                               
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                    }
                                }
                                else
                                {
                                    if (answerObj.AnswerOptionsInQuizQuestions.ObjectFieldsInAnswer.Any())
                                    {
                                        foreach (var objectFieldsInAnswer in answerObj.AnswerOptionsInQuizQuestions.ObjectFieldsInAnswer) {
                                            if (objectFieldsInAnswer.IsExternalSync.HasValue && objectFieldsInAnswer.IsExternalSync.Value) {
                                                var valueitem = !string.IsNullOrWhiteSpace(objectFieldsInAnswer.Value) ? objectFieldsInAnswer.Value : answerObj.AnswerText;
                                                if (objectFieldsInAnswer.IsCommentMapped.HasValue && objectFieldsInAnswer.IsCommentMapped.Value && (objectFieldsInAnswer.IsExternalSync.HasValue && objectFieldsInAnswer.IsExternalSync.Value)) {
                                                    valueitem = !string.IsNullOrWhiteSpace(objectFieldsInAnswer.Value) ? objectFieldsInAnswer.Value : answerObj.Comment;
                                                }
                                                
                                                if (!string.IsNullOrWhiteSpace(valueitem)) {
                                                    UpdateQuizStatusObj.FieldsToUpdate.Add(new Fields {
                                                        ObjectName = objectFieldsInAnswer.ObjectName,
                                                        FieldName = objectFieldsInAnswer.FieldName,
                                                        Value = valueitem?.Trim()
                                                    });
                                                }
                                            }
                                        }

                                        //var valueitem = (ansType == (int)AnswerTypeEnum.Single || ansType == (int)AnswerTypeEnum.Multiple || ansType == (int)AnswerTypeEnum.DrivingLicense || ansType == (int)AnswerTypeEnum.LookingforJobs)
                                        //  ? objectFieldsInAnswer.Value : answerObj.AnswerText;
                                       
                                    }
                                }
                            }
                        }
                    }
                }


                var urlhub = GlobalSettings.HubUrl.ToString();
                var token = GlobalSettings.HubUrlBearer.ToString();
                var urlAutomation = urlhub + "/api/v1/Automations/web-hooks/UpdateAutomationStatus";
                var urlAppointment = urlhub + "/api/v1/appointments/web-hooks/UpdateAppointmentStatus";

                try
                {
                    var apiSuccess = false;
                    var res = OWCHelper.UpdateQuizStatus(UpdateQuizStatusObj);
                    if (res != null)
                    {
                        apiSuccess = res.status == "true";
                        quizAnswerSubmit.AppointmentCode = res.appointmentCode;
                        quizAnswerSubmit.AppointmentLink = res.appointmentLink;
                        quizAnswerSubmit.AppointmentErrorMessage = res.message;
                    }

                    if (!apiSuccess)
                        AddPendingApi(urlAutomation, token, JsonConvert.SerializeObject(UpdateQuizStatusObj), "POST");
                }
                catch (Exception)
                {
                    AddPendingApi(urlAutomation, token, JsonConvert.SerializeObject(UpdateQuizStatusObj), "POST");
                }

            }


            return quizAnswerSubmit;
        }



        private QuizContent ContentInQuiz(PublishQuizTmpModel publishQuizTmpModel, Db.ContentsInQuiz contentsInQuiz)
        {
            QuizContent quizContent = new QuizContent();
            quizContent.Id = contentsInQuiz.Id;
            quizContent.ContentTitle = VariableLinking(contentsInQuiz.ContentTitle, false, false, null, publishQuizTmpModel);
            quizContent.ContentDescription = VariableLinking(contentsInQuiz.ContentDescription, false, false, null, publishQuizTmpModel);
            quizContent.ShowDescription = contentsInQuiz.ShowDescription;
            quizContent.ShowContentTitleImage = contentsInQuiz.ShowContentTitleImage.ToBoolValue();
            quizContent.AliasTextForNextButton = contentsInQuiz.AliasTextForNextButton;
            quizContent.EnableNextButton = contentsInQuiz.EnableNextButton;

            quizContent.ShowContentDescriptionImage = contentsInQuiz.ShowContentDescriptionImage.ToBoolValue();
            quizContent.ViewPreviousQuestion = contentsInQuiz.ViewPreviousQuestion;
            quizContent.AutoPlay = contentsInQuiz.AutoPlay;
            quizContent.SecondsToApply = contentsInQuiz.SecondsToApply ?? "0";
            quizContent.VideoFrameEnabled = contentsInQuiz.VideoFrameEnabled ?? false;
            quizContent.AutoPlayForDescription = contentsInQuiz.AutoPlayForDescription;
            quizContent.SecondsToApplyForDescription = contentsInQuiz.SecondsToApplyForDescription ?? "0";
            quizContent.DescVideoFrameEnabled = contentsInQuiz.DescVideoFrameEnabled ?? false;
            quizContent.ShowTitle = contentsInQuiz.ShowTitle;
            quizContent.DisplayOrderForTitle = contentsInQuiz.DisplayOrderForTitle;
            quizContent.DisplayOrderForTitleImage = contentsInQuiz.DisplayOrderForTitleImage;
            quizContent.DisplayOrderForDescription = contentsInQuiz.DisplayOrderForDescription;
            quizContent.DisplayOrderForDescriptionImage = contentsInQuiz.DisplayOrderForDescriptionImage;
            quizContent.DisplayOrderForNextButton = contentsInQuiz.DisplayOrderForNextButton;

            quizContent.ContentTitleImage = contentsInQuiz.ContentTitleImage ?? string.Empty;
            quizContent.PublicIdForContentTitle = contentsInQuiz.PublicIdForContentTitle ?? string.Empty;
            quizContent.ContentDescriptionImage = contentsInQuiz.ContentDescriptionImage ?? string.Empty;
            quizContent.PublicIdForContentDescription = contentsInQuiz.PublicIdForContentDescription ?? string.Empty;

            if (contentsInQuiz.EnableMediaFileForDescription || contentsInQuiz.EnableMediaFileForTitle)
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var mediaObjList = publishQuizTmpModel.MediaVariableList.Where(r => r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == contentsInQuiz.Id);
                    //var mediaObjList = UOWObj.MediaVariablesDetailsRepository.Get(r => r.QuizId == publishQuizTmpModel.QuizDetailId &&
                    //r.ConfigurationDetailsId == publishQuizTmpModel.ConfigurationDetailId && r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == contentsInQuiz.Id);

                    if (mediaObjList != null)
                    {
                        if (contentsInQuiz.EnableMediaFileForTitle)
                        {
                            var mediaObj = mediaObjList.FirstOrDefault(v => v.Type == (int)ImageTypeEnum.Title);
                            if (mediaObj != null)
                            {
                                quizContent.ContentTitleImage = mediaObj.ObjectValue;
                                quizContent.PublicIdForContentTitle = mediaObj.ObjectPublicId ?? string.Empty;

                                var newMedia = ExtractDynamicMedia(mediaObj, publishQuizTmpModel.LeadDetails, publishQuizTmpModel.UserMediaClassifications, publishQuizTmpModel.LeadOwners);
                                if (newMedia != null)
                                {
                                    quizContent.ContentTitleImage = newMedia.MediaUrl;
                                    quizContent.PublicIdForContentTitle = newMedia.MediaPublicId;
                                }
                            }
                        }

                        if (contentsInQuiz.EnableMediaFileForDescription)
                        {
                            var mediaObj = mediaObjList.FirstOrDefault(v => v.Type == (int)ImageTypeEnum.Description);
                            if (mediaObj != null)
                            {
                                quizContent.ContentDescriptionImage = mediaObj.ObjectValue ?? string.Empty;
                                quizContent.PublicIdForContentDescription = mediaObj.ObjectPublicId ?? string.Empty;

                                var newMedia = ExtractDynamicMedia(mediaObj, publishQuizTmpModel.LeadDetails, publishQuizTmpModel.UserMediaClassifications, publishQuizTmpModel.LeadOwners);
                                if (newMedia != null)
                                {
                                    quizContent.ContentDescriptionImage = newMedia.MediaUrl;
                                    quizContent.PublicIdForContentDescription = newMedia.MediaPublicId;
                                }
                            }
                        }

                    }
                }
            }
            return quizContent;
        }

        private QuizAnswerSubmit QuestionObjectQuizResult(QuizAnswerSubmit quizAnswerSubmit, PublishQuizTmpModel publishQuizTmpModel, Db.QuizResults result)
        {
            if (publishQuizTmpModel.LeadUserId == null && publishQuizTmpModel.RecruiterUserId == 0)
            {
                var resultIdList = new List<int>();

                int? formId = null;
                int? flowOrder = null;

                using (var UOWObj = new AutomationUnitOfWork())
                {
                    if (publishQuizTmpModel.ConfigurationDetailId > 0)
                    {
                        //var resultList = UOWObj.ResultIdsInConfigurationDetailsRepository.Get(v => v.ConfigurationDetailsId == publishQuizTmpModel.ConfigurationDetailId);
                        var resultList = UOWObj.ResultIdsInConfigurationDetailsRepository.GetResultIdsInConfigurationDetailsByConfigurationDetailsId(publishQuizTmpModel.ConfigurationDetailId);
                        if (resultList != null && resultList.Any())
                        {
                            resultIdList = resultList.Select(r => r.ResultId).ToList();
                        }

                        var quizConfigurationResult = resultList.Where(v => v.ResultId.Equals(result.Id)).FirstOrDefault();
                        if (quizConfigurationResult != null)
                        {
                            formId = quizConfigurationResult.FormId;
                            flowOrder = quizConfigurationResult.FlowOrder;
                        }
                    }

                    quizAnswerSubmit.ShowLeadUserForm = false;
                    if (!publishQuizTmpModel.RequestMode.Equals("PREVIEW") && !publishQuizTmpModel.RequestMode.Equals("PREVIEWTEMPLATE"))
                    {

                        if (resultIdList.Any())
                        {
                            //var quizComponentLogsList = UOWObj.QuizComponentLogsRepository.Get(r => r.QuizId == publishQuizTmpModel.QuizDetailId &&  r.ObjectTypeId == (int)BranchingLogicEnum.RESULT);
                            var quizComponentLogsList = UOWObj.QuizComponentLogsRepository.GetQuizComponentLogsByQuizId(publishQuizTmpModel.QuizDetailId, (int)BranchingLogicEnum.RESULT);
                            if (resultIdList.Any(r => r == result.Id || quizComponentLogsList != null && quizComponentLogsList.Any(q => q.DraftedObjectId == r && q.PublishedObjectId == result.Id)))
                            {
                                quizAnswerSubmit.ShowLeadUserForm = true;
                            }
                        }
                        //else
                        //{
                        //    quizAnswerSubmit.ShowLeadUserForm = result.ShowLeadUserForm;
                        //    if (publishQuizTmpModel.LeadUserId == null && publishQuizTmpModel.RecruiterUserId == null && publishQuizTmpModel.QuizType == (int)QuizTypeEnum.Personality)
                        //    {
                        //        //var personalityResultSetting = UOWObj.PersonalityResultSettingRepository.Get(v => v.QuizId == publishQuizTmpModel.QuizDetailId).FirstOrDefault();
                        //        var personalityResultSetting = UOWObj.PersonalityResultSettingRepository.GetPersonalityResultSettingByQuizId(publishQuizTmpModel.QuizDetailId).FirstOrDefault();
                        //        if (personalityResultSetting != null && personalityResultSetting.Status == (int) StatusEnum.Active)
                        //        {
                        //            quizAnswerSubmit.ShowLeadUserForm = personalityResultSetting.ShowLeadUserForm;
                        //        }
                        //    }
                        //}
                    }
                }
                if (quizAnswerSubmit.ShowLeadUserForm)
                {
                    quizAnswerSubmit.FormId = formId;
                    quizAnswerSubmit.FlowOrder = flowOrder;
                }
            }

            if (publishQuizTmpModel.QuizType != (int)QuizTypeEnum.Personality && publishQuizTmpModel.QuizType != (int)QuizTypeEnum.PersonalityTemplate)
            {
                var quizAnswerSubmitResult = QuizResultAssessment(publishQuizTmpModel, result);
                quizAnswerSubmit.ResultScore = quizAnswerSubmitResult;
            }
            else
            {
                quizAnswerSubmit = QuizResultpersonalityResultSetting(publishQuizTmpModel, result, quizAnswerSubmit);
            }

            if (publishQuizTmpModel.RecruiterUserId > 0)
            {
                var nextQuestionObj = FetchNextQuestion(publishQuizTmpModel.QuizDetailId, publishQuizTmpModel.IsQuesAndContentInSameTable, publishQuizTmpModel.IsBranchingLogicEnabled, result.Id, (int)BranchingLogicEnum.RESULTNEXT);
                if (nextQuestionObj != null)
                {
                    if (nextQuestionObj.GetType().BaseType.Name == "BadgesInQuiz")
                    {
                        var badgesInQuizObject = (Db.BadgesInQuiz)nextQuestionObj;
                        quizAnswerSubmit.BadgeDetails = BadgesInQuiz(publishQuizTmpModel, badgesInQuizObject);
                    }
                }
            }

            return quizAnswerSubmit;
        }

        private QuizAnswerSubmit.Result QuizResultAssessment(PublishQuizTmpModel publishQuizTmpModel, Db.QuizResults result)
        {
            var quizAnswerSubmitResultScore = new QuizAnswerSubmit.Result();
            #region non personality result Setting


            quizAnswerSubmitResultScore.Image = result.ShowResultImage ? result.Image : string.Empty;
            quizAnswerSubmitResultScore.PublicIdForResult = result.ShowResultImage ? result.PublicId : string.Empty;
            if (result.EnableMediaFile && result.ShowResultImage)
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var mediaObjList = publishQuizTmpModel.MediaVariableList.Where(r => r.ObjectTypeId == (int)BranchingLogicEnum.RESULT && r.ObjectId == result.Id);
                    //var mediaObjList = UOWObj.MediaVariablesDetailsRepository.Get(r => r.QuizId == publishQuizTmpModel.QuizDetailId && r.ConfigurationDetailsId == publishQuizTmpModel.ConfigurationDetailId && r.ObjectTypeId == (int)BranchingLogicEnum.RESULT && r.ObjectId == result.Id);
                    if (mediaObjList != null)
                    {
                        var mediaObj = mediaObjList.FirstOrDefault();
                        if (mediaObj != null)
                        {
                            quizAnswerSubmitResultScore.Image = result.ShowResultImage ? mediaObj.ObjectValue : string.Empty;
                            quizAnswerSubmitResultScore.PublicIdForResult = result.ShowResultImage ? mediaObj.ObjectPublicId : string.Empty;

                            var newMedia = ExtractDynamicMedia(mediaObj, publishQuizTmpModel.LeadDetails, publishQuizTmpModel.UserMediaClassifications, publishQuizTmpModel.LeadOwners);
                            if (newMedia != null)
                            {
                                quizAnswerSubmitResultScore.Image = newMedia.MediaUrl;
                                quizAnswerSubmitResultScore.PublicIdForResult = newMedia.MediaPublicId;
                            }
                        }
                    }
                }
            }

            quizAnswerSubmitResultScore.HideActionButton = result.HideCallToAction.ToBoolValue();
            quizAnswerSubmitResultScore.ActionButtonURL = quizAnswerSubmitResultScore.HideActionButton ? string.Empty : result.ActionButtonURL;
            quizAnswerSubmitResultScore.OpenLinkInNewTab = quizAnswerSubmitResultScore.HideActionButton ? false : result.OpenLinkInNewTab;
            quizAnswerSubmitResultScore.ActionButtonTxtSize = quizAnswerSubmitResultScore.HideActionButton ? string.Empty : result.ActionButtonTxtSize;
            quizAnswerSubmitResultScore.ActionButtonColor = quizAnswerSubmitResultScore.HideActionButton ? string.Empty : result.ActionButtonColor;
            quizAnswerSubmitResultScore.ActionButtonTitleColor = quizAnswerSubmitResultScore.HideActionButton ? string.Empty : result.ActionButtonTitleColor;
            quizAnswerSubmitResultScore.ActionButtonText = quizAnswerSubmitResultScore.HideActionButton ? string.Empty : result.ActionButtonText;

            quizAnswerSubmitResultScore.DisplayOrderForTitle = quizAnswerSubmitResultScore.DisplayOrderForTitle;
            quizAnswerSubmitResultScore.DisplayOrderForTitleImage = quizAnswerSubmitResultScore.DisplayOrderForTitleImage;
            quizAnswerSubmitResultScore.DisplayOrderForDescription = quizAnswerSubmitResultScore.DisplayOrderForDescription;
            quizAnswerSubmitResultScore.DisplayOrderForNextButton = quizAnswerSubmitResultScore.DisplayOrderForNextButton;
            var correctAnsCount = 0;
            string scoreValueTxt = string.Empty;
            using (var UOWObj = new AutomationUnitOfWork())
            {
                //var quizResultSetting = UOWObj.ResultSettingsRepository.Get(v => v.QuizId == publishQuizTmpModel.QuizDetailId).FirstOrDefault();
                var quizResultSetting = UOWObj.ResultSettingsRepository.GetResultSettingsRepositoryExtension(publishQuizTmpModel.QuizDetailId).FirstOrDefault();
                if (quizResultSetting != null)
                {
                    bool resultSettingShowScoreValue = quizResultSetting.ShowScoreValue.ToBoolValue();
                    //TODO:Dharminder

                    //var attemptedQuestions = UOWObj.QuizQuestionStatsRepository.Get(t => t.QuizAttemptId == publishQuizTmpModel.QuizattemptId && t.Status == (int)StatusEnum.Active && t.CompletedOn != null && (t.QuestionsInQuiz.TimerRequired ? true : t.QuizAnswerStats.Any(r => !r.AnswerOptionsInQuizQuestions.IsUnansweredType)));

                    var attemptedQuestions = UOWObj.QuizQuestionStatsRepository.Get(t => t.QuizAttemptId == publishQuizTmpModel.QuizattemptId && t.Status == (int)StatusEnum.Active && t.CompletedOn != null
                    && t.QuestionsInQuiz.TemplateId == null && (t.QuestionsInQuiz.TimerRequired ? true : t.QuizAnswerStats.Any(r => !r.AnswerOptionsInQuizQuestions.IsUnansweredType)));

                    var validattemptedQuestions = attemptedQuestions.Any(r => r.QuestionsInQuiz.Status == (int)StatusEnum.Active && (r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple));

                    var validattemptedQuestions2 = attemptedQuestions.Any(r => (r.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Any(t => t.Status == (int)StatusEnum.Active && t.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && t.IsCorrectForMultipleAnswer.HasValue && t.IsCorrectForMultipleAnswer.Value)) || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single);

                    quizAnswerSubmitResultScore.ResultScoreValueTxt = string.Empty;
                    quizAnswerSubmitResultScore.ShowScoreValue = false;

                    if (resultSettingShowScoreValue && (((publishQuizTmpModel.QuizType == (int)QuizTypeEnum.Score || publishQuizTmpModel.QuizType == (int)QuizTypeEnum.ScoreTemplate) && validattemptedQuestions) || validattemptedQuestions2))
                    {
                        scoreValueTxt = quizResultSetting.CustomTxtForScoreValueInResult;
                        if (publishQuizTmpModel.QuizType == (int)QuizTypeEnum.Score || publishQuizTmpModel.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                            correctAnsCount = attemptedQuestions.Sum(a => a.QuizAnswerStats.Sum(x => x.AnswerOptionsInQuizQuestions.AssociatedScore)).Value;
                        else
                            correctAnsCount = attemptedQuestions.Count(a => (a.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && ((a.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Select(s => s.Id).OrderBy(s => s)).SequenceEqual(a.QuizAnswerStats.Select(r => r.AnswerId).OrderBy(s => s)))) || (a.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single && a.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer == true));

                        scoreValueTxt = scoreValueTxt == null ? string.Empty : scoreValueTxt.Replace("%score%", ' ' + correctAnsCount.ToString() + ' ').Replace("%total%", ' ' + attemptedQuestions.Count(t => (t.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Any(r => r.Status == (int)StatusEnum.Active && r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value)) || t.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single).ToString() + ' ');

                        quizAnswerSubmitResultScore.ResultScoreValueTxt = scoreValueTxt;
                        quizAnswerSubmitResultScore.ShowScoreValue = true;

                    }

                    scoreValueTxt = scoreValueTxt == null ? string.Empty : scoreValueTxt.Replace("%score%", ' ' + correctAnsCount.ToString() + ' ').Replace("%total%", ' ' + attemptedQuestions.Where(t => (t.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Any()) || t.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single).Count().ToString() + ' ');


                    if (quizResultSetting.ShowCorrectAnswer.HasValue && quizResultSetting.ShowCorrectAnswer.Value)
                    {
                        quizAnswerSubmitResultScore.AnswerKeyCustomTxt = quizResultSetting.CustomTxtForAnswerKey;
                        quizAnswerSubmitResultScore.YourAnswerCustomTxt = quizResultSetting.CustomTxtForYourAnswer;
                        quizAnswerSubmitResultScore.CorrectAnswerCustomTxt = quizResultSetting.CustomTxtForCorrectAnswer;
                        quizAnswerSubmitResultScore.ExplanationCustomTxt = quizResultSetting.CustomTxtForExplanation;
                        quizAnswerSubmitResultScore.ShowCorrectAnswer = true;

                        quizAnswerSubmitResultScore.AttemptedQuestionAnswerDetails = new List<QuizAnswerSubmit.AnswerResult>();

                        foreach (var item in attemptedQuestions.Where(r => r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Short || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Long))
                        {
                            var correctAnswerTxt = string.Empty;
                            int associatedScore = default(int);
                            bool? IsCorrectValue = null;
                            var yourAnswer = string.Empty;
                            

                            if (item.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Any())
                            {
                                var variabletext = string.Join(",", item.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Select(t => t.Option).Select(s => "'" + s + "'"));
                                correctAnswerTxt = VariableLinking(variabletext, false, quizAnswerSubmitResultScore.ShowScoreValue, scoreValueTxt, publishQuizTmpModel);
                                if (publishQuizTmpModel.QuizType == (int)QuizTypeEnum.Score || publishQuizTmpModel.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                    associatedScore = item.QuizAnswerStats.Sum(x => x.AnswerOptionsInQuizQuestions.AssociatedScore).Value;
                                else
                                    IsCorrectValue = (item.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Select(s => s.Id).OrderBy(s => s)).SequenceEqual(item.QuizAnswerStats.Select(r => r.AnswerId).OrderBy(s => s));
                            }
                            else if (item.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single)
                            {
                                var variabletext = item.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(t => t.IsCorrectAnswer.HasValue ? t.IsCorrectAnswer.Value : false).Select(t => t.Option).FirstOrDefault();

                                correctAnswerTxt = VariableLinking(variabletext, false, quizAnswerSubmitResultScore.ShowScoreValue, scoreValueTxt, publishQuizTmpModel);


                                if (publishQuizTmpModel.QuizType == (int)QuizTypeEnum.Score || publishQuizTmpModel.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                    associatedScore = item.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.AssociatedScore.Value;
                                else
                                    IsCorrectValue = item.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer.HasValue && item.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer.Value;
                            }

                            if (item.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple || item.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single)
                            {
                                var variableText = string.Join(",", item.QuizAnswerStats.Where(t => !t.AnswerOptionsInQuizQuestions.IsUnansweredType).Select(t => t.AnswerOptionsInQuizQuestions.Option).Select(s => "'" + s + "'"));
                                yourAnswer = VariableLinking(variableText, false, quizAnswerSubmitResultScore.ShowScoreValue, scoreValueTxt, publishQuizTmpModel);
                            }
                            else
                                yourAnswer = item.QuizAnswerStats.Select(t => t.AnswerText).FirstOrDefault();

                            quizAnswerSubmitResultScore.AttemptedQuestionAnswerDetails.Add(new QuizAnswerSubmit.AnswerResult
                            {
                                Question = VariableLinking(item.QuestionsInQuiz.Question, false, quizAnswerSubmitResultScore.ShowScoreValue, scoreValueTxt, publishQuizTmpModel),
                                YourAnswer = string.IsNullOrEmpty(yourAnswer) ? notAttemptedQuesText : yourAnswer,
                                CorrectAnswer = (publishQuizTmpModel.QuizType == (int)QuizTypeEnum.Score || publishQuizTmpModel.QuizType == (int)QuizTypeEnum.ScoreTemplate) ? string.Empty : correctAnswerTxt,
                                IsCorrect = IsCorrectValue,
                                AnswerExplanation = item.QuestionsInQuiz.CorrectAnswerDescription,
                                AssociatedScore = (publishQuizTmpModel.QuizType == (int)QuizTypeEnum.Score || publishQuizTmpModel.QuizType == (int)QuizTypeEnum.ScoreTemplate) ? associatedScore : default(int)
                            });
                        }
                    }
                    else
                    {
                        quizAnswerSubmitResultScore.ShowCorrectAnswer = false;
                    }
                }
            }

            quizAnswerSubmitResultScore.ShowInternalTitle = result.ShowInternalTitle;
            quizAnswerSubmitResultScore.ShowExternalTitle = result.ShowExternalTitle;
            quizAnswerSubmitResultScore.ShowDescription = result.ShowDescription;
            var msgVariables = publishQuizTmpModel.QuizVariables?.Where(v => v.ObjectTypes == (int)QuizVariableObjectTypes.RESULT && v.ObjectId == result.Id).FirstOrDefault()?.Variables;
            CommonStaticData.VacancyVariableReplace(publishQuizTmpModel.ContactObject, msgVariables, publishQuizTmpModel.CompanyCode);
            quizAnswerSubmitResultScore.Title = VariableLinking(result.Title, false, quizAnswerSubmitResultScore.ShowScoreValue, scoreValueTxt, publishQuizTmpModel, msgVariables);
            quizAnswerSubmitResultScore.InternalTitle = VariableLinking(result.InternalTitle, false, quizAnswerSubmitResultScore.ShowScoreValue, scoreValueTxt, publishQuizTmpModel, msgVariables);
            quizAnswerSubmitResultScore.Description = VariableLinking(result.Description, false, quizAnswerSubmitResultScore.ShowScoreValue, scoreValueTxt, publishQuizTmpModel, msgVariables);
            quizAnswerSubmitResultScore.AutoPlay = result.AutoPlay;
            quizAnswerSubmitResultScore.SecondsToApply = result.SecondsToApply;
            quizAnswerSubmitResultScore.VideoFrameEnabled = result.VideoFrameEnabled;
            quizAnswerSubmitResultScore.DisplayOrderForTitle = result.DisplayOrderForTitle;
            quizAnswerSubmitResultScore.DisplayOrderForTitleImage = result.DisplayOrderForTitleImage;
            quizAnswerSubmitResultScore.DisplayOrderForDescription = result.DisplayOrderForDescription;
            quizAnswerSubmitResultScore.DisplayOrderForNextButton = result.DisplayOrderForNextButton;

            using (var UOWObj = new AutomationUnitOfWork())
            {
                //var quizStatsObj = UOWObj.QuizStatsRepository.Get(v => v.QuizAttemptId == publishQuizTmpModel.QuizattemptId).FirstOrDefault();
                var quizStatsObj = UOWObj.QuizStatsRepository.GetQuizStatsQuizAttemptId(publishQuizTmpModel.QuizattemptId).FirstOrDefault();
                if (quizStatsObj != null)
                {
                    quizStatsObj.ResultId = result.Id;

                    UOWObj.QuizStatsRepository.Update(quizStatsObj);
                    UOWObj.Save();

                }
            }
            #endregion

            return quizAnswerSubmitResultScore;
        }

        private QuizAnswerSubmit FetchPeviousQuestionType(PublishQuizTmpModel publishQuizTmpModel)
        {
            QuizAnswerSubmit quizAnswerSubmit = new QuizAnswerSubmit();
            object previousQuestionObj = FetchPreviousQuestion(publishQuizTmpModel, publishQuizTmpModel.RequestQuestionId, publishQuizTmpModel.RequestedQuestionType.Value);
            if (previousQuestionObj == null)
            {
                Status = ResultEnum.Error;
                ErrorMessage = "You can not see the previous question.";
                return null;
            }

            if (previousQuestionObj != null)
            {
                PreviousQuestion_UpdateQuizQuestionStat(publishQuizTmpModel);

                #region if previous is question type
                var objectBasename = previousQuestionObj.GetType().BaseType.Name;
                if (objectBasename == "QuestionsInQuiz")
                {
                    var questionObject = (Db.QuestionsInQuiz)previousQuestionObj;
                    if (questionObject.ViewPreviousQuestion || questionObject.EditAnswer)
                    {
                        quizAnswerSubmit = PreviousTypeQuestion(publishQuizTmpModel, questionObject);
                    }
                }

                #endregion

                #region if previous is content type

                else if (objectBasename == "ContentsInQuiz" && ((Db.ContentsInQuiz)previousQuestionObj).ViewPreviousQuestion)
                {
                    var contentsInQuizObject = (Db.ContentsInQuiz)previousQuestionObj;
                    quizAnswerSubmit = PreviousTypeContent(publishQuizTmpModel, contentsInQuizObject);
                }
                #endregion
            }

            return quizAnswerSubmit;
        }

        private QuizAnswerSubmit PreviousTypeQuestion(PublishQuizTmpModel publishQuizTmpModel, Db.QuestionsInQuiz previousQuestionObj)
        {

            var quizQuestionStatsObj = new Db.QuizQuestionStats();
            quizQuestionStatsObj.QuizAttemptId = publishQuizTmpModel.QuizattemptId;
            quizQuestionStatsObj.QuestionId = previousQuestionObj.Id;
            quizQuestionStatsObj.StartedOn = DateTime.Now;
            quizQuestionStatsObj.Status = (int)StatusEnum.Active;

            using (var UOWObj = new AutomationUnitOfWork())
            {
                //var previouszQuestionStats = UOWObj.QuizQuestionStatsRepository.Get(v => v.QuizAttemptId == publishQuizTmpModel.QuizattemptId).FirstOrDefault(r => r.QuestionId == previousQuestionObj.Id && r.Status == (int)StatusEnum.Active);
                var previouszQuestionStats = UOWObj.QuizQuestionStatsRepository.GetQuizQuestionStatsByQuizAttemptId(publishQuizTmpModel.QuizattemptId).FirstOrDefault(r => r.QuestionId == previousQuestionObj.Id && r.Status == (int)StatusEnum.Active);
                if (previouszQuestionStats != null)
                {
                    previouszQuestionStats.Status = (int)StatusEnum.Inactive;
                    UOWObj.QuizQuestionStatsRepository.Update(previouszQuestionStats);
                    UOWObj.Save();
                }
            }

            InsertQuizQuestionStats(previousQuestionObj.Id, publishQuizTmpModel.QuizattemptId);

            QuizAnswerSubmit quizAnswerSubmit = new QuizAnswerSubmit();
            var QuestionDetails = new QuizQuestion();
            if (previousQuestionObj.Type == (int)BranchingLogicEnum.QUESTION)
            {
                quizAnswerSubmit.QuestionType = (int)BranchingLogicEnum.QUESTION;
                var quizQuestion = NextQuesTypeQues(publishQuizTmpModel, previousQuestionObj, true);
                quizAnswerSubmit.QuestionDetails = quizQuestion;
                quizAnswerSubmit.PreviousQuestionSubmittedAnswer = PreviousQuestion(publishQuizTmpModel, previousQuestionObj);
            }
            else if (previousQuestionObj.Type == (int)BranchingLogicEnum.CONTENT)
            {
                quizAnswerSubmit.QuestionType = (int)BranchingLogicEnum.CONTENT;
                var quizContent = NextQuesTypeContent(publishQuizTmpModel, previousQuestionObj);
                quizAnswerSubmit.ContentDetails = quizContent;
            }



            object previous = FetchPreviousQuestion(publishQuizTmpModel, previousQuestionObj.Id, previousQuestionObj.Type);

            if (previous != null)
            {

                if (previous.GetType().BaseType.Name == "QuestionsInQuiz")
                {
                    var previousQuestion = (Db.QuestionsInQuiz)previous;
                    if (previousQuestion.Type == (int)BranchingLogicEnum.QUESTION)
                        quizAnswerSubmit.IsBackButtonEnable = (previousQuestion.RevealCorrectAnswer.HasValue && previousQuestion.RevealCorrectAnswer.Value) ? false : (previousQuestion.ViewPreviousQuestion || previousQuestion.EditAnswer);
                    else if (previousQuestion.Type == (int)BranchingLogicEnum.CONTENT)
                        quizAnswerSubmit.IsBackButtonEnable = previousQuestion.ViewPreviousQuestion;

                }
                if (previous.GetType().BaseType.Name == "ContentsInQuiz")
                {
                    var previousQuestion = (Db.ContentsInQuiz)previous;
                    quizAnswerSubmit.IsBackButtonEnable = previousQuestion.ViewPreviousQuestion;
                }
            }

            return quizAnswerSubmit;
        }

        private QuizAnswerSubmit PreviousTypeContent(PublishQuizTmpModel publishQuizTmpModel, Db.ContentsInQuiz previousQuestionObject)
        {

            using (var UOWObj = new AutomationUnitOfWork())
            {
                //var previouszQuestionStats = UOWObj.QuizObjectStatsRepository.Get(v => v.QuizAttemptId == publishQuizTmpModel.QuizattemptId).FirstOrDefault(r => r.ObjectId == previousQuestionObject.Id && r.Status == (int)StatusEnum.Active);
                var previouszQuestionStats = UOWObj.QuizObjectStatsRepository.GetQuizObjectStatsByQuizAttemptId(publishQuizTmpModel.QuizattemptId).FirstOrDefault(r => r.ObjectId == previousQuestionObject.Id && r.Status == (int)StatusEnum.Active);
                if (previouszQuestionStats != null)
                {
                    previouszQuestionStats.Status = (int)StatusEnum.Inactive;
                    UOWObj.QuizObjectStatsRepository.Update(previouszQuestionStats);
                    UOWObj.Save();
                }
            }

            QuizAnswerSubmit quizAnswerSubmit = new QuizAnswerSubmit();
            quizAnswerSubmit.ContentDetails = ContentInQuiz(publishQuizTmpModel, previousQuestionObject);
            //     object previous = FetchPreviousQuestion(publishQuizTmpModel);
            object previous = FetchPreviousQuestion(publishQuizTmpModel, previousQuestionObject.Id, (int)BranchingLogicEnum.CONTENT);

            if (previous != null)
            {

                if (previous.GetType().BaseType.Name == "QuestionsInQuiz")
                {
                    var previousQuestion = (Db.QuestionsInQuiz)previous;
                    if (previousQuestion.Type == (int)BranchingLogicEnum.QUESTION)
                        quizAnswerSubmit.IsBackButtonEnable = (previousQuestion.RevealCorrectAnswer.HasValue && previousQuestion.RevealCorrectAnswer.Value) ? false : (previousQuestion.ViewPreviousQuestion || previousQuestion.EditAnswer);
                    else if (previousQuestion.Type == (int)BranchingLogicEnum.CONTENT)
                        quizAnswerSubmit.IsBackButtonEnable = previousQuestion.ViewPreviousQuestion;

                }
                if (previous.GetType().BaseType.Name == "ContentsInQuiz")
                {
                    var previousQuestion = (Db.ContentsInQuiz)previous;
                    quizAnswerSubmit.IsBackButtonEnable = previousQuestion.ViewPreviousQuestion;
                }

            }

            return quizAnswerSubmit;
        }

        public object FetchPreviousQuestion(PublishQuizTmpModel publishQuizTmpModel, int AnswerId, int TypeId)
        {

            if (AnswerId == -1)
            {
                return null;
            }
            if (publishQuizTmpModel.IsBranchingLogicEnabled)
            {
                var quizBranching = new Db.BranchingLogic();
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    //var quizQuestionStatsList = UOWObj.QuizQuestionStatsRepository.Get(v => v.QuizAttemptId == publishQuizTmpModel.QuizattemptId);
                    var quizQuestionStatsList = UOWObj.QuizQuestionStatsRepository.Get(v => v.QuizAttemptId == publishQuizTmpModel.QuizattemptId).OrderByDescending(v => v.Id);
                    var quizAnswerStatsList = UOWObj.QuizAnswerStatsRepository.Get(v => v.QuizAttemptId == publishQuizTmpModel.QuizattemptId);
                    //var quizBranchingList = UOWObj.BranchingLogicRepository.Get(v => v.QuizId == publishQuizTmpModel.QuizDetailId && v.DestinationObjectId == AnswerId && v.DestinationTypeId == TypeId);
                    var quizBranchingList = UOWObj.BranchingLogicRepository.GetBranchingLogicByQuizId(publishQuizTmpModel.QuizDetailId).Where(v => v.DestinationObjectId == AnswerId && v.DestinationTypeId == TypeId);
                    if (quizBranchingList != null && quizBranchingList.Any())
                    {



                        foreach (var quizBranchingObj in quizBranchingList)
                        {
                            if (quizBranchingObj.SourceTypeId == (int)BranchingLogicEnum.ANSWER)
                            {
                                //if (quizBranchingObj.SourceTypeId == (int)BranchingLogicEnum.ANSWER)
                                //{
                                //    var answerOptionsInQuizQuestionsObj = UOWObj.QuestionsInQuizRepository.Get(a => a.Status == (int)StatusEnum.Active && a.AnswerOptionsInQuizQuestions.Any(q => q.Id == quizBranchingObj.SourceObjectId)).FirstOrDefault();

                                //    if (quizQuestionStatsList != null && quizQuestionStatsList.Any(r => r.Status == (int)StatusEnum.Active && answerOptionsInQuizQuestionsObj != null && r.QuestionId == answerOptionsInQuizQuestionsObj.Id))
                                //    {
                                //        quizBranching = quizBranchingObj;
                                //        return answerOptionsInQuizQuestionsObj;
                                //    }
                                //}

                                Db.QuestionsInQuiz answerOptionsInQuizQuestionsObj = null;
                                var answerattempt = quizAnswerStatsList.FirstOrDefault(v => v.AnswerId == quizBranchingObj.SourceObjectId);
                                if (answerattempt != null)
                                {
                                    var quizQuestionStat = quizQuestionStatsList.Where(v => v.Id == answerattempt.QuizQuestionStatsId).FirstOrDefault();
                                    if (quizQuestionStat != null)
                                    {
                                        answerOptionsInQuizQuestionsObj = UOWObj.QuestionsInQuizRepository.Get(a => a.Status == (int)StatusEnum.Active && a.Id == quizQuestionStat.QuestionId).FirstOrDefault();

                                    }


                                }

                                if (quizQuestionStatsList != null && quizQuestionStatsList.Any(r => r.Status == (int)StatusEnum.Active && answerOptionsInQuizQuestionsObj != null && r.QuestionId == answerOptionsInQuizQuestionsObj.Id))
                                {
                                    quizBranching = quizBranchingObj;
                                    return answerOptionsInQuizQuestionsObj;
                                }

                            }

                            if (quizBranchingObj.SourceTypeId == (int)BranchingLogicEnum.QUESTIONNEXT)
                            {
                                //var answerOptionsInQuizQuestionsObj = UOWObj.QuestionsInQuizRepository.Get(a => a.Status == (int)StatusEnum.Active && a.Id == quizBranchingObj.SourceObjectId).FirstOrDefault();
                                var answerOptionsInQuizQuestionsObj = UOWObj.QuestionsInQuizRepository.GetQuestioninQuizById((int)StatusEnum.Active, quizBranchingObj.SourceObjectId).FirstOrDefault();

                                if (quizQuestionStatsList != null && quizQuestionStatsList.Any(r => r.Status == (int)StatusEnum.Active && r.QuestionId == quizBranchingObj.SourceObjectId))
                                {
                                    quizBranching = quizBranchingObj;
                                    return answerOptionsInQuizQuestionsObj;
                                }
                            }

                            if (quizBranchingObj.SourceTypeId == (int)BranchingLogicEnum.CONTENTNEXT)
                            {
                                if (publishQuizTmpModel.IsQuesAndContentInSameTable)
                                {
                                    //var answerOptionsInQuizQuestionsObj = UOWObj.QuestionsInQuizRepository.Get(a => a.Status == (int)StatusEnum.Active && a.Id == quizBranchingObj.SourceObjectId).FirstOrDefault();
                                    var answerOptionsInQuizQuestionsObj = UOWObj.QuestionsInQuizRepository.GetQuestioninQuizById((int)StatusEnum.Active, quizBranchingObj.SourceObjectId).FirstOrDefault();

                                    if (quizQuestionStatsList != null && quizQuestionStatsList.Any(r => r.Status == (int)StatusEnum.Active && r.QuestionId == quizBranchingObj.SourceObjectId))
                                    {
                                        quizBranching = quizBranchingObj;
                                        return answerOptionsInQuizQuestionsObj;
                                    }
                                }
                                else
                                {
                                    //var contentsInQuizObj = UOWObj.ContentsInQuizRepository.Get(a => a.Status == (int)StatusEnum.Active && a.Id == quizBranchingObj.SourceObjectId).FirstOrDefault();
                                    var contentsInQuizObj = UOWObj.ContentsInQuizRepository.GetContentInQuizById((int)StatusEnum.Active, quizBranchingObj.SourceObjectId).FirstOrDefault();
                                    //var quizObjectStats = UOWObj.QuizObjectStatsRepository.Get(r => r.QuizAttemptId == publishQuizTmpModel.QuizattemptId);
                                    var quizObjectStats = UOWObj.QuizObjectStatsRepository.GetQuizObjectStatsByQuizAttemptId(publishQuizTmpModel.QuizattemptId);
                                    if (quizObjectStats != null && quizObjectStats.Any(r => r.TypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == quizBranchingObj.SourceObjectId))
                                    {
                                        quizBranching = quizBranchingObj;
                                        return contentsInQuizObj;
                                    }
                                }
                            }


                        }

                        if (quizBranching == null) return null;

                        return null;
                    }
                }

            }
            else
            {
                int currentQuesDisplayOrder = 0;

                using (var UOWObj = new AutomationUnitOfWork())
                {
                    if (TypeId == (int)BranchingLogicEnum.CONTENT && !publishQuizTmpModel.IsQuesAndContentInSameTable)
                    {
                        //currentQuesDisplayOrder = UOWObj.ContentsInQuizRepository.Get(r => r.QuizId == publishQuizTmpModel.QuizDetailId && r.Id == AnswerId).Select(r => r.DisplayOrder).FirstOrDefault();
                        currentQuesDisplayOrder = UOWObj.ContentsInQuizRepository.GetContentInQuizRepositoryExtension(publishQuizTmpModel.QuizDetailId).Where(r => r.Id == AnswerId).Select(r => r.DisplayOrder).FirstOrDefault();
                    }
                    else
                    {
                        //currentQuesDisplayOrder = UOWObj.QuestionsInQuizRepository.Get(r => r.QuizId == publishQuizTmpModel.QuizDetailId && r.Id == AnswerId).Select(r => r.DisplayOrder).FirstOrDefault();
                        currentQuesDisplayOrder = UOWObj.QuestionsInQuizRepository.GetExistingQuestioninQuiz(publishQuizTmpModel.QuizDetailId).Where(r => r.Id == AnswerId).Select(r => r.DisplayOrder).FirstOrDefault();
                    }

                    //var question = UOWObj.QuestionsInQuizRepository.Get(r => r.QuizId == publishQuizTmpModel.QuizDetailId).OrderByDescending(r => r.DisplayOrder).FirstOrDefault(r => r.Status == (int)StatusEnum.Active && r.DisplayOrder < currentQuesDisplayOrder);
                    var question = UOWObj.QuestionsInQuizRepository.GetExistingQuestioninQuiz(publishQuizTmpModel.QuizDetailId).OrderByDescending(r => r.DisplayOrder).FirstOrDefault(r => r.Status == (int)StatusEnum.Active && r.DisplayOrder < currentQuesDisplayOrder);

                    //var content = UOWObj.ContentsInQuizRepository.Get(r => r.QuizId == publishQuizTmpModel.QuizDetailId).OrderByDescending(r => r.DisplayOrder).FirstOrDefault(r => r.Status == (int)StatusEnum.Active && r.DisplayOrder < currentQuesDisplayOrder);
                    var content = UOWObj.ContentsInQuizRepository.GetContentInQuizRepositoryExtension(publishQuizTmpModel.QuizDetailId).OrderByDescending(r => r.DisplayOrder).FirstOrDefault(r => r.Status == (int)StatusEnum.Active && r.DisplayOrder < currentQuesDisplayOrder);

                    if (question == null && content == null)
                        return null;
                    else if (question == null)
                        return content;
                    else if (content == null)
                        return question;
                    else if (question.DisplayOrder < content.DisplayOrder)
                        return content;
                    else
                        return question;
                }

            }

            return null;
        }


        private void PreviousQuestion_UpdateQuizQuestionStat(PublishQuizTmpModel publishQuizTmpModel)
        {
            if (publishQuizTmpModel.RequestedQuestionType == (int)BranchingLogicEnum.QUESTION || publishQuizTmpModel.IsQuesAndContentInSameTable)
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    //var currentQuestionStats = UOWObj.QuizQuestionStatsRepository.Get(v =>v.QuizAttemptId == publishQuizTmpModel.QuizattemptId).FirstOrDefault(r => r.QuestionId == publishQuizTmpModel.RequestQuestionId && r.Status == (int)StatusEnum.Active);
                    var currentQuestionStats = UOWObj.QuizQuestionStatsRepository.GetQuizQuestionStatsByQuizAttemptId(publishQuizTmpModel.QuizattemptId).FirstOrDefault(r => r.QuestionId == publishQuizTmpModel.RequestQuestionId && r.Status == (int)StatusEnum.Active);
                    if (currentQuestionStats != null)
                    {
                        currentQuestionStats.Status = (int)StatusEnum.Inactive;
                        UOWObj.QuizQuestionStatsRepository.Update(currentQuestionStats);
                        UOWObj.Save();
                    }
                }
            }
            else
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    //var currentQuestionStats = UOWObj.QuizObjectStatsRepository.Get(v => v.QuizAttemptId == publishQuizTmpModel.QuizattemptId).FirstOrDefault(r => r.ObjectId == publishQuizTmpModel.RequestQuestionId && r.Status == (int)StatusEnum.Active);
                    var currentQuestionStats = UOWObj.QuizObjectStatsRepository.GetQuizObjectStatsByQuizAttemptId(publishQuizTmpModel.QuizattemptId).FirstOrDefault(r => r.ObjectId == publishQuizTmpModel.RequestQuestionId && r.Status == (int)StatusEnum.Active);
                    if (currentQuestionStats != null)
                    {
                        currentQuestionStats.Status = (int)StatusEnum.Inactive;
                        UOWObj.QuizObjectStatsRepository.Update(currentQuestionStats);
                        UOWObj.Save();
                    }
                }
            }
        }




        private QuizAnswerSubmit.SubmittedAnswerResult PreviousQuestion(PublishQuizTmpModel publishQuizTmpModel, Db.QuestionsInQuiz nextQuestionObj)
        {
            QuizAnswerSubmit.SubmittedAnswerResult previousQuestionSubmittedAnswer = null;
            #region PreviousQuestionSubmittedAnswer

            using (var UOWObj = new AutomationUnitOfWork())
            {
                int quizQuestionStatsId = 0;
                //var QuizQuestionStats = UOWObj.QuizQuestionStatsRepository.Get(v => v.QuizAttemptId == publishQuizTmpModel.QuizattemptId && v.QuestionId == nextQuestionObj.Id);
                var QuizQuestionStats = UOWObj.QuizQuestionStatsRepository.GetQuizQuestionStatsByQuestionId(publishQuizTmpModel.QuizattemptId, nextQuestionObj.Id);
                if (QuizQuestionStats != null && QuizQuestionStats.Any(v => v.CompletedOn.HasValue))
                {
                    quizQuestionStatsId = QuizQuestionStats.LastOrDefault(v => v.CompletedOn.HasValue).Id;
                }
                else if (QuizQuestionStats != null && QuizQuestionStats.Any())
                {
                    quizQuestionStatsId = QuizQuestionStats.LastOrDefault().Id;
                }

                var attemptedAnswerIds = UOWObj.QuizAnswerStatsRepository.Get(v => v.QuizQuestionStatsId == quizQuestionStatsId).Select(r => r.AnswerId).ToList();
                if (attemptedAnswerIds != null && attemptedAnswerIds.Any())
                {
                    previousQuestionSubmittedAnswer = new QuizAnswerSubmit.SubmittedAnswerResult();
                    var submittedAnswerOption = UOWObj.AnswerOptionsInQuizQuestionsRepository.Get(v => v.QuestionId == nextQuestionObj.Id && attemptedAnswerIds.Contains(v.Id));
                    if (submittedAnswerOption.Any())
                    {
                        previousQuestionSubmittedAnswer.AliasTextForCorrect = nextQuestionObj.AliasTextForCorrect;
                        previousQuestionSubmittedAnswer.AliasTextForIncorrect = nextQuestionObj.AliasTextForIncorrect;
                        previousQuestionSubmittedAnswer.AliasTextForYourAnswer = nextQuestionObj.AliasTextForYourAnswer;
                        previousQuestionSubmittedAnswer.AliasTextForCorrectAnswer = nextQuestionObj.AliasTextForCorrectAnswer;
                        previousQuestionSubmittedAnswer.AliasTextForExplanation = nextQuestionObj.AliasTextForExplanation;
                        previousQuestionSubmittedAnswer.AliasTextForNextButton = nextQuestionObj.AliasTextForNextButton;
                        previousQuestionSubmittedAnswer.CorrectAnswerDescription = nextQuestionObj.CorrectAnswerDescription;
                        previousQuestionSubmittedAnswer.SubmittedAnswerDetails = new List<QuizAnswerSubmit.SubmittedAnswerResult.SubmittedAnswer>();

                        foreach (var submittedAnswerOptionObj in submittedAnswerOption)
                        {
                            var list = CheckQuizAnswerType(publishQuizTmpModel, nextQuestionObj, submittedAnswerOptionObj, quizQuestionStatsId);
                            if (list != null && list.Any())
                            {
                                previousQuestionSubmittedAnswer.SubmittedAnswerDetails.AddRange(list);
                            }

                        }


                    }
                }
            }
            #endregion

            return previousQuestionSubmittedAnswer;
        }


        private QuizContent NextQuesTypeContent(PublishQuizTmpModel publishQuizTmpModel, Db.QuestionsInQuiz nextQuestionObj)
        {

            var quizAnswerSubmitContentDetails = new QuizContent();

            quizAnswerSubmitContentDetails.Id = nextQuestionObj.Id;
            var msgVariable = publishQuizTmpModel.QuizVariables?.Where(v => v.ObjectTypes == (int)QuizVariableObjectTypes.QUESTION && v.ObjectId == nextQuestionObj.Id).FirstOrDefault()?.Variables;
            CommonStaticData.VacancyVariableReplace(publishQuizTmpModel.ContactObject, msgVariable, publishQuizTmpModel.CompanyCode);
            quizAnswerSubmitContentDetails.ContentTitle = VariableLinking(nextQuestionObj.Question, false, false, null, publishQuizTmpModel, msgVariable);
            quizAnswerSubmitContentDetails.ContentDescription = VariableLinking(nextQuestionObj.Description, false, false, null, publishQuizTmpModel, msgVariable);
            quizAnswerSubmitContentDetails.ShowDescription = nextQuestionObj.ShowDescription;

            quizAnswerSubmitContentDetails.ContentTitleImage = nextQuestionObj.QuestionImage ?? string.Empty;
            quizAnswerSubmitContentDetails.PublicIdForContentTitle = nextQuestionObj.PublicId ?? string.Empty;
            quizAnswerSubmitContentDetails.ContentDescriptionImage = nextQuestionObj.DescriptionImage ?? string.Empty;
            quizAnswerSubmitContentDetails.PublicIdForContentDescription = nextQuestionObj.PublicIdForDescription ?? string.Empty;

            if (nextQuestionObj.ShowQuestionImage.ToBoolValue() && (nextQuestionObj.EnableMediaFile || nextQuestionObj.EnableMediaFileForDescription))
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var mediaObjList = publishQuizTmpModel.MediaVariableList.Where(r => r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT && r.ObjectId == nextQuestionObj.Id);
                    //var mediaObjList = UOWObj.MediaVariablesDetailsRepository.Get(r => r.QuizId == publishQuizTmpModel.QuizDetailId && r.ConfigurationDetailsId == publishQuizTmpModel.ConfigurationDetailId && r.ObjectTypeId == (int)BranchingLogicEnum.CONTENT &&  r.ObjectId == nextQuestionObj.Id);
                    if (mediaObjList != null && mediaObjList.Any(r => r.Type == (int)ImageTypeEnum.Title) && nextQuestionObj.EnableMediaFile)
                    {
                        var mediaObj = mediaObjList.FirstOrDefault(r => r.Type == (int)ImageTypeEnum.Title);
                        if (mediaObj != null)
                        {
                            quizAnswerSubmitContentDetails.ContentTitleImage = mediaObj.ObjectValue;
                            quizAnswerSubmitContentDetails.PublicIdForContentTitle = mediaObj.ObjectPublicId ?? string.Empty;

                            var newMedia = ExtractDynamicMedia(mediaObj, publishQuizTmpModel.LeadDetails, publishQuizTmpModel.UserMediaClassifications, publishQuizTmpModel.LeadOwners);
                            if (newMedia != null)
                            {
                                quizAnswerSubmitContentDetails.ContentTitleImage = newMedia.MediaUrl;
                                quizAnswerSubmitContentDetails.PublicIdForContentTitle = newMedia.MediaPublicId;
                            }
                        }
                    }

                    if (mediaObjList != null && mediaObjList.Any(r => r.Type == (int)ImageTypeEnum.Description) && nextQuestionObj.EnableMediaFile)
                    {
                        var mediaObj = mediaObjList.FirstOrDefault(r => r.Type == (int)ImageTypeEnum.Description);
                        if (mediaObj != null)
                        {
                            quizAnswerSubmitContentDetails.ContentDescriptionImage = mediaObj.ObjectValue ?? string.Empty;
                            quizAnswerSubmitContentDetails.PublicIdForContentDescription = mediaObj.ObjectPublicId ?? string.Empty;

                            var newMedia = ExtractDynamicMedia(mediaObj, publishQuizTmpModel.LeadDetails, publishQuizTmpModel.UserMediaClassifications, publishQuizTmpModel.LeadOwners);
                            if (newMedia != null)
                            {
                                quizAnswerSubmitContentDetails.ContentDescriptionImage = newMedia.MediaUrl;
                                quizAnswerSubmitContentDetails.PublicIdForContentDescription = newMedia.MediaPublicId;
                            }
                        }
                    }
                }
            }

            quizAnswerSubmitContentDetails.ShowContentTitleImage = nextQuestionObj.ShowQuestionImage.ToBoolValue();
            quizAnswerSubmitContentDetails.AliasTextForNextButton = nextQuestionObj.NextButtonText;
            quizAnswerSubmitContentDetails.EnableNextButton = nextQuestionObj.EnableNextButton;

            quizAnswerSubmitContentDetails.ShowContentDescriptionImage = nextQuestionObj.ShowDescriptionImage.ToBoolValue();
            quizAnswerSubmitContentDetails.ViewPreviousQuestion = nextQuestionObj.ViewPreviousQuestion;
            quizAnswerSubmitContentDetails.AutoPlay = nextQuestionObj.AutoPlay;
            quizAnswerSubmitContentDetails.SecondsToApply = nextQuestionObj.SecondsToApply ?? "0";
            quizAnswerSubmitContentDetails.VideoFrameEnabled = nextQuestionObj.VideoFrameEnabled ?? false;
            quizAnswerSubmitContentDetails.AutoPlayForDescription = nextQuestionObj.AutoPlayForDescription;
            quizAnswerSubmitContentDetails.SecondsToApplyForDescription = nextQuestionObj.SecondsToApplyForDescription ?? "0";
            quizAnswerSubmitContentDetails.DescVideoFrameEnabled = nextQuestionObj.DescVideoFrameEnabled ?? false;
            quizAnswerSubmitContentDetails.ShowTitle = nextQuestionObj.ShowTitle;
            quizAnswerSubmitContentDetails.DisplayOrderForTitle = nextQuestionObj.DisplayOrderForTitle;
            quizAnswerSubmitContentDetails.DisplayOrderForTitleImage = nextQuestionObj.DisplayOrderForTitleImage;
            quizAnswerSubmitContentDetails.DisplayOrderForDescription = nextQuestionObj.DisplayOrderForDescription;
            quizAnswerSubmitContentDetails.DisplayOrderForDescriptionImage = nextQuestionObj.DisplayOrderForDescriptionImage;
            quizAnswerSubmitContentDetails.DisplayOrderForNextButton = nextQuestionObj.DisplayOrderForNextButton;

            return quizAnswerSubmitContentDetails;

        }

        private QuizQuestion NextQuesTypeQues(PublishQuizTmpModel publishQuizTmpModel, Db.QuestionsInQuiz nextQuestionObj, bool isPreviousQuestion)
        {
            var QuestionDetails = new QuizQuestion();
            QuestionDetails.QuestionId = nextQuestionObj.Id;
            QuestionDetails.TemplateId = nextQuestionObj.TemplateId;
            QuestionDetails.LanguageCode = nextQuestionObj.LanguageCode;
            var msgVariable = publishQuizTmpModel.QuizVariables?.Where(v => v.ObjectTypes == (int)QuizVariableObjectTypes.QUESTION && v.ObjectId == nextQuestionObj.Id).FirstOrDefault()?.Variables;
            CommonStaticData.VacancyVariableReplace(publishQuizTmpModel.ContactObject, msgVariable, publishQuizTmpModel.CompanyCode);
            QuestionDetails.QuestionTitle = VariableLinking(nextQuestionObj.Question, false, false, null, publishQuizTmpModel, msgVariable);
            QuestionDetails.ShowTitle = nextQuestionObj.ShowTitle;
            QuestionDetails.QuestionImage = nextQuestionObj.ShowQuestionImage.ToBoolValue() ? nextQuestionObj.QuestionImage : string.Empty;
            QuestionDetails.PublicIdForQuestion = nextQuestionObj.ShowQuestionImage.ToBoolValue() ? nextQuestionObj.PublicId : string.Empty;
            QuestionDetails.DescriptionImage = nextQuestionObj.DescriptionImage ?? string.Empty;
            QuestionDetails.PublicIdForDescription = nextQuestionObj.PublicIdForDescription ?? string.Empty;

            if (nextQuestionObj.ShowQuestionImage.ToBoolValue() && (nextQuestionObj.EnableMediaFile || nextQuestionObj.EnableMediaFileForDescription))
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var mediaObjList = publishQuizTmpModel.MediaVariableList.Where(r => r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION && r.ObjectId == nextQuestionObj.Id).ToList();
                    //var mediaObjList = UOWObj.MediaVariablesDetailsRepository.Get(r => r.QuizId == publishQuizTmpModel.QuizDetailId && r.ConfigurationDetailsId == publishQuizTmpModel.ConfigurationDetailId && r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION && r.ObjectId == nextQuestionObj.Id);
                    if (mediaObjList != null && mediaObjList.Any() && nextQuestionObj.EnableMediaFile)
                    {
                        var mediaObj = mediaObjList.FirstOrDefault();
                        QuestionDetails.QuestionImage = mediaObj.ObjectValue;
                        QuestionDetails.PublicIdForQuestion = mediaObj.ObjectPublicId;
                        var newMedia = ExtractDynamicMedia(mediaObj, publishQuizTmpModel.LeadDetails, publishQuizTmpModel.UserMediaClassifications, publishQuizTmpModel.LeadOwners);
                        if (newMedia != null)
                        {
                            QuestionDetails.QuestionImage = newMedia.MediaUrl;
                            QuestionDetails.PublicIdForQuestion = newMedia.MediaPublicId;
                        }
                    }
                    if (mediaObjList != null && mediaObjList.Any() && nextQuestionObj.EnableMediaFileForDescription)
                    {
                        var mediaObj = mediaObjList.FirstOrDefault(r => r.Type == (int)ImageTypeEnum.Description);
                        if (mediaObj != null)
                        {
                            QuestionDetails.DescriptionImage = mediaObj.ObjectValue ?? string.Empty;
                            QuestionDetails.PublicIdForDescription = mediaObj.ObjectPublicId ?? string.Empty;

                        }
                        var newMedia = ExtractDynamicMedia(mediaObj, publishQuizTmpModel.LeadDetails, publishQuizTmpModel.UserMediaClassifications, publishQuizTmpModel.LeadOwners);
                        if (newMedia != null)
                        {
                            QuestionDetails.DescriptionImage = newMedia.MediaUrl;
                            QuestionDetails.PublicIdForDescription = newMedia.MediaPublicId;
                        }
                    }
                }
            }

            QuestionDetails.ShowQuestionImage = nextQuestionObj.ShowQuestionImage;
            QuestionDetails.ShowAnswerImage = nextQuestionObj.ShowAnswerImage;
            QuestionDetails.AnswerType = nextQuestionObj.AnswerType;
            QuestionDetails.MinAnswer = nextQuestionObj.MinAnswer;
            QuestionDetails.MaxAnswer = nextQuestionObj.MaxAnswer;
            QuestionDetails.NextButtonColor = nextQuestionObj.NextButtonColor;
            QuestionDetails.NextButtonText = nextQuestionObj.NextButtonText;
            QuestionDetails.NextButtonTxtColor = nextQuestionObj.NextButtonTxtColor;
            QuestionDetails.NextButtonTxtSize = nextQuestionObj.NextButtonTxtSize;
            QuestionDetails.EnableNextButton = nextQuestionObj.EnableNextButton;
            QuestionDetails.ViewPreviousQuestion = nextQuestionObj.ViewPreviousQuestion;
            QuestionDetails.EditAnswer = nextQuestionObj.EditAnswer;
            QuestionDetails.TimerRequired = nextQuestionObj.TimerRequired;
            QuestionDetails.Time = nextQuestionObj.Time;
            QuestionDetails.AutoPlay = nextQuestionObj.AutoPlay;
            QuestionDetails.SecondsToApply = nextQuestionObj.SecondsToApply ?? "0";
            QuestionDetails.VideoFrameEnabled = nextQuestionObj.VideoFrameEnabled ?? false;
            QuestionDetails.DisplayOrderForTitle = nextQuestionObj.DisplayOrderForTitle;
            QuestionDetails.DisplayOrderForTitleImage = nextQuestionObj.DisplayOrderForTitleImage;
            QuestionDetails.DisplayOrderForDescription = nextQuestionObj.DisplayOrderForDescription;
            QuestionDetails.DisplayOrderForDescriptionImage = nextQuestionObj.DisplayOrderForDescriptionImage;
            QuestionDetails.DisplayOrderForAnswer = nextQuestionObj.DisplayOrderForAnswer;
            QuestionDetails.DisplayOrderForNextButton = nextQuestionObj.DisplayOrderForNextButton;
            QuestionDetails.Description = VariableLinking(nextQuestionObj.Description, false, false, null, publishQuizTmpModel, msgVariable);
            QuestionDetails.ShowDescription = nextQuestionObj.ShowDescription;
            QuestionDetails.ShowDescriptionImage = nextQuestionObj.ShowDescriptionImage ?? false;
            QuestionDetails.AutoPlayForDescription = nextQuestionObj.AutoPlayForDescription;
            QuestionDetails.SecondsToApplyForDescription = nextQuestionObj.SecondsToApplyForDescription ?? "0";
            QuestionDetails.DescVideoFrameEnabled = nextQuestionObj.DescVideoFrameEnabled ?? false;
            QuestionDetails.EnableComment = nextQuestionObj.EnableComment;
            QuestionDetails.StartedOn = default(DateTime?);
            QuestionDetails.IsMultiRating = nextQuestionObj.IsMultiRating;
            //QuestionDetails.LanguageCode = (publishQuizTmpModel.RequestUserTypeId == 3 && !string.IsNullOrWhiteSpace(nextQuestionObj.LanguageCode)) ? nextQuestionObj.LanguageCode : null; 

            QuestionDetails.AnswerList = new List<AnswerOptionInQuestion>();
            using (var UOWObj = new AutomationUnitOfWork())
            {
                if (publishQuizTmpModel.IsLastQuestionStarted)
                {
                    //var quizQuestionStat = UOWObj.QuizQuestionStatsRepository.Get(r => r.QuestionId == nextQuestionObj.Id && r.Status == (int)StatusEnum.Active);
                    var quizQuestionStat = UOWObj.QuizQuestionStatsRepository.GetQuizQuestionStatsByQuizAttemptId(publishQuizTmpModel.QuizattemptId).Where(r => r.QuestionId == nextQuestionObj.Id && r.Status == (int)StatusEnum.Active);
                    if (quizQuestionStat != null && quizQuestionStat.Any())
                    {
                        QuestionDetails.StartedOn = quizQuestionStat.FirstOrDefault().StartedOn;
                    }
                }

                //var ansOptionList = UOWObj.AnswerOptionsInQuizQuestionsRepository.Get(r => r.QuestionId == nextQuestionObj.Id && r.Status == (int)StatusEnum.Active);
                var ansOptionList = UOWObj.AnswerOptionsInQuizQuestionsRepository.GetAnswerOptionsInQuizQuestionsByQuestionId(nextQuestionObj.Id, (int)StatusEnum.Active);

                if (!isPreviousQuestion)
                {
                    ansOptionList = ansOptionList.Where(r => !r.IsUnansweredType);
                }
                if (ansOptionList != null && ansOptionList.Any())
                {
                    List<Db.MediaVariablesDetails> listAnswermedia = new List<Db.MediaVariablesDetails>();
                    if (nextQuestionObj.ShowAnswerImage.ToBoolValue())
                    {
                        listAnswermedia = publishQuizTmpModel.MediaVariableList.Where(r => r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER).ToList();

                        //listAnswermedia = UOWObj.MediaVariablesDetailsRepository.Get(r => r.QuizId == publishQuizTmpModel.QuizDetailId && r.ConfigurationDetailsId == publishQuizTmpModel.ConfigurationDetailId && r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER).ToList();
                    }

                    foreach (var ans in ansOptionList.OrderBy(r => r.DisplayOrder))
                    {
                        var answerImage = string.Empty;
                        var publicIdForAnswer = string.Empty;
                        if (ans.EnableMediaFile && listAnswermedia != null && listAnswermedia.Any())
                        {
                            var mediaObj = listAnswermedia.FirstOrDefault(r => r.ObjectId == ans.Id);
                            answerImage = mediaObj.ObjectValue;
                            publicIdForAnswer = mediaObj.ObjectPublicId;

                            var newMedia = ExtractDynamicMedia(mediaObj, publishQuizTmpModel.LeadDetails, publishQuizTmpModel.UserMediaClassifications, publishQuizTmpModel.LeadOwners);
                            if (newMedia != null)
                            {
                                answerImage = newMedia.MediaUrl;
                                publicIdForAnswer = newMedia.MediaPublicId;
                            }
                        }
                        else
                        {

                            answerImage = nextQuestionObj.ShowAnswerImage.ToBoolValue() ? ans.OptionImage : string.Empty;
                            publicIdForAnswer = nextQuestionObj.ShowAnswerImage.ToBoolValue() ? ans.PublicId : string.Empty;
                        }

                        QuestionDetails.AnswerList.Add(new AnswerOptionInQuestion
                        {
                            AnswerId = ans.Id,
                            AssociatedScore = ans.AssociatedScore,
                            AnswerText = VariableLinking(ans.Option, false, false, null, publishQuizTmpModel, msgVariable),
                            AnswerDescription = VariableLinking(ans.Description, false, false, null, publishQuizTmpModel, msgVariable),
                            AnswerImage = answerImage,
                            PublicIdForAnswer = publicIdForAnswer,
                            IsCorrectAnswer = false,
                            DisplayOrder = ans.DisplayOrder,
                            IsUnansweredType = ans.IsUnansweredType,
                            AutoPlay = ans.AutoPlay,
                            SecondsToApply = ans.SecondsToApply,
                            VideoFrameEnabled = ans.VideoFrameEnabled,
                            ListValues = ans.ListValues,
                            RefId = ans.RefId,
                            //for Rating type question
                            OptionTextforRatingOne = ans.OptionTextforRatingOne,
                            OptionTextforRatingTwo = ans.OptionTextforRatingTwo,
                            OptionTextforRatingThree = ans.OptionTextforRatingThree,
                            OptionTextforRatingFour = ans.OptionTextforRatingFour,
                            OptionTextforRatingFive = ans.OptionTextforRatingFive
                        });
                    }
                }
                QuestionDetails.AnswerStructureType = nextQuestionObj.AnswerStructureType;
            }

            return QuestionDetails;
        }


        public object FetchNextQuestion(int quizdetailId, bool IsQuesAndContentInSameTable, bool isBranchingLogicEnabled, int AnswerId = -1, int TypeId = (int)BranchingLogicEnum.QUESTION)
        {
            using (var UOWObj = new AutomationUnitOfWork())
            {
                Db.BranchingLogic quizBranching = null;
                var quizBranchingLogic = UOWObj.BranchingLogicRepository.GetBranchingLogicByQuizId(quizdetailId);

                if (quizBranchingLogic == null && !quizBranchingLogic.Any()) return null;

                if (isBranchingLogicEnabled)
                {

                    if (AnswerId == -1)
                    {
                        quizBranching = quizBranchingLogic.FirstOrDefault(a => a.IsStartingPoint);
                        if (quizBranching.SourceTypeId == (int)BranchingLogicEnum.QUESTION)
                        {
                            //var questionsInQuizObj = UOWObj.QuestionsInQuizRepository.Get(a => a.Status == (int)StatusEnum.Active && a.Id == quizBranching.SourceObjectId).FirstOrDefault();
                            var questionsInQuizObj = UOWObj.QuestionsInQuizRepository.GetQuestioninQuizById((int)StatusEnum.Active, quizBranching.SourceObjectId).FirstOrDefault();
                            return questionsInQuizObj;
                        }

                        if (quizBranching.SourceTypeId == (int)BranchingLogicEnum.WHATSAPPTEMPLATE)
                        {
                            var templateInQuizObj = UOWObj.QuestionsInQuizRepository.GetQuestioninQuizById((int)StatusEnum.Active, quizBranching.SourceObjectId).FirstOrDefault();
                            return templateInQuizObj;
                        }

                        if (quizBranching.SourceTypeId == (int)BranchingLogicEnum.ANSWER)
                        {
                            var answerOptionsInQuizQuestionsObj = UOWObj.AnswerOptionsInQuizQuestionsRepository.Get(a => a.Status == (int)StatusEnum.Active && a.Id == quizBranching.SourceObjectId).FirstOrDefault().QuestionsInQuiz;
                            return answerOptionsInQuizQuestionsObj;
                        }
                        if (quizBranching.SourceTypeId == (int)BranchingLogicEnum.CONTENT && IsQuesAndContentInSameTable)
                        {
                            //var questionsInQuizObj = UOWObj.QuestionsInQuizRepository.Get(a => a.Status == (int)StatusEnum.Active && a.Id == quizBranching.SourceObjectId).FirstOrDefault();
                            var questionsInQuizObj = UOWObj.QuestionsInQuizRepository.GetQuestioninQuizById((int)StatusEnum.Active, quizBranching.SourceObjectId).FirstOrDefault();
                            return questionsInQuizObj;
                        }
                        if (quizBranching.SourceTypeId == (int)BranchingLogicEnum.CONTENT && !IsQuesAndContentInSameTable)
                        {
                            //var contentsInQuizObj = UOWObj.ContentsInQuizRepository.Get(a => a.Status == (int)StatusEnum.Active && a.Id == quizBranching.SourceObjectId).FirstOrDefault();
                            var contentsInQuizObj = UOWObj.ContentsInQuizRepository.GetContentInQuizById((int)StatusEnum.Active, quizBranching.SourceObjectId).FirstOrDefault();
                            return contentsInQuizObj;
                        }
                        if (quizBranching.SourceTypeId == (int)BranchingLogicEnum.RESULT)
                        {
                            //var quizResultsObj = UOWObj.QuizResultsRepository.Get(a => a.Status == (int)StatusEnum.Active && a.Id == quizBranching.SourceObjectId).FirstOrDefault();
                            var quizResultsObj = UOWObj.QuizResultsRepository.GetQuizResultsById((int)StatusEnum.Active, quizBranching.SourceObjectId).FirstOrDefault();
                            return quizResultsObj;
                        }
                        if (quizBranching.SourceTypeId == (int)BranchingLogicEnum.BADGE && quizBranching.SourceTypeId == (int)BranchingLogicEnum.BADGENEXT)
                        {
                            //var badgesInQuizObj = UOWObj.BadgesInQuizRepository.Get(a => a.Status == (int)StatusEnum.Active && a.Id == quizBranching.SourceObjectId).FirstOrDefault();
                            var badgesInQuizObj = UOWObj.BadgesInQuizRepository.GetBadgesInQuizById((int)StatusEnum.Active, quizBranching.SourceObjectId).FirstOrDefault();
                            return badgesInQuizObj;
                        }

                        return null;
                    }
                    else
                    {
                        quizBranching = quizBranchingLogic.FirstOrDefault(a => a.SourceObjectId == AnswerId && a.SourceTypeId == TypeId);
                        if (quizBranching == null) return null;
                        if (quizBranching.DestinationTypeId == (int)BranchingLogicEnum.QUESTION)
                        {
                            //var answerOptionsInQuizQuestionsObj = UOWObj.QuestionsInQuizRepository.Get(a => a.Status == (int)StatusEnum.Active && a.Id == quizBranching.DestinationObjectId).FirstOrDefault();
                            var answerOptionsInQuizQuestionsObj = UOWObj.QuestionsInQuizRepository.GetQuestioninQuizById((int)StatusEnum.Active, quizBranching.DestinationObjectId.Value).FirstOrDefault();
                            return answerOptionsInQuizQuestionsObj;
                        }

                        if (quizBranching.DestinationTypeId == (int)BranchingLogicEnum.WHATSAPPTEMPLATE)
                        {
                            var answerOptionsInTemplateQuestionsObj = UOWObj.QuestionsInQuizRepository.GetQuestioninQuizById((int)StatusEnum.Active, quizBranching.DestinationObjectId.Value).FirstOrDefault();
                            return answerOptionsInTemplateQuestionsObj;
                        }

                        //Todo: whatsapptemplate
                        if (quizBranching.DestinationTypeId == (int)BranchingLogicEnum.CONTENT && IsQuesAndContentInSameTable)
                        {
                            //var answerOptionsInQuizQuestionsObj = UOWObj.QuestionsInQuizRepository.Get(a => a.Status == (int)StatusEnum.Active && a.Id == quizBranching.DestinationObjectId).FirstOrDefault();
                            var answerOptionsInQuizQuestionsObj = UOWObj.QuestionsInQuizRepository.GetQuestioninQuizById((int)StatusEnum.Active, quizBranching.DestinationObjectId.Value).FirstOrDefault();
                            return answerOptionsInQuizQuestionsObj;
                        }
                        if (quizBranching.DestinationTypeId == (int)BranchingLogicEnum.CONTENT && !IsQuesAndContentInSameTable)
                        {
                            //var contentsInQuizObj = UOWObj.ContentsInQuizRepository.Get(a => a.Status == (int)StatusEnum.Active && a.Id == quizBranching.DestinationObjectId).FirstOrDefault();
                            var contentsInQuizObj = UOWObj.ContentsInQuizRepository.GetContentInQuizById((int)StatusEnum.Active, quizBranching.DestinationObjectId.Value).FirstOrDefault();
                            return contentsInQuizObj;
                        }
                        if (quizBranching.DestinationTypeId == (int)BranchingLogicEnum.RESULT)
                        {
                            //var quizResultsObj = UOWObj.QuizResultsRepository.Get(a => a.Status == (int)StatusEnum.Active && a.Id == quizBranching.DestinationObjectId).FirstOrDefault();
                            var quizResultsObj = UOWObj.QuizResultsRepository.GetQuizResultsById((int)StatusEnum.Active, quizBranching.DestinationObjectId.Value).FirstOrDefault();
                            return quizResultsObj;
                        }
                        if (quizBranching.DestinationTypeId == (int)BranchingLogicEnum.BADGE)
                        {
                            //var badgesInQuizObj = UOWObj.BadgesInQuizRepository.Get(a => a.Status == (int)StatusEnum.Active && a.Id == quizBranching.DestinationObjectId).FirstOrDefault();
                            var badgesInQuizObj = UOWObj.BadgesInQuizRepository.GetBadgesInQuizById((int)StatusEnum.Active, quizBranching.DestinationObjectId.Value).FirstOrDefault();
                            return badgesInQuizObj;
                        }
                        return null;
                    }
                }
                else
                {
                    int currentQuesDisplayOrder = 0;
                    var quizDetails = UOWObj.QuizDetailsRepository.Get(a => a.Id == quizdetailId).FirstOrDefault();
                    if (TypeId == (int)BranchingLogicEnum.QUESTIONNEXT)
                        currentQuesDisplayOrder = quizDetails.QuestionsInQuiz.Where(r => r.Id == AnswerId).Select(r => r.DisplayOrder).FirstOrDefault();
                    else if (TypeId == (int)BranchingLogicEnum.CONTENTNEXT && IsQuesAndContentInSameTable)
                        currentQuesDisplayOrder = quizDetails.QuestionsInQuiz.Where(r => r.Id == AnswerId).Select(r => r.DisplayOrder).FirstOrDefault();
                    else if (TypeId == (int)BranchingLogicEnum.CONTENTNEXT && !IsQuesAndContentInSameTable)
                        currentQuesDisplayOrder = quizDetails.ContentsInQuiz.Where(r => r.Id == AnswerId).Select(r => r.DisplayOrder).FirstOrDefault();
                    else
                        currentQuesDisplayOrder = quizDetails.QuestionsInQuiz.Where(r => r.AnswerOptionsInQuizQuestions.Any(t => t.Id == AnswerId)).Select(r => r.DisplayOrder).FirstOrDefault();

                    //var question = UOWObj.QuestionsInQuizRepository.Get(a => a.QuizId == quizdetailId).OrderBy(r => r.DisplayOrder).FirstOrDefault(r => r.Status == (int)StatusEnum.Active && r.DisplayOrder > currentQuesDisplayOrder);
                    var question = UOWObj.QuestionsInQuizRepository.GetExistingQuestioninQuiz(quizdetailId).OrderBy(r => r.DisplayOrder).FirstOrDefault(r => r.Status == (int)StatusEnum.Active && r.DisplayOrder > currentQuesDisplayOrder);

                    //var content = UOWObj.ContentsInQuizRepository.Get(a => a.QuizId == quizdetailId).OrderBy(r => r.DisplayOrder).FirstOrDefault(r => r.Status == (int)StatusEnum.Active && r.DisplayOrder > currentQuesDisplayOrder);
                    var content = UOWObj.ContentsInQuizRepository.GetContentInQuizRepositoryExtension(quizdetailId).OrderBy(r => r.DisplayOrder).FirstOrDefault(r => r.Status == (int)StatusEnum.Active && r.DisplayOrder > currentQuesDisplayOrder);

                    if (question == null && content == null)
                        return null;
                    else if (question == null)
                        return content;
                    else if (content == null)
                        return question;
                    else if (question.DisplayOrder < content.DisplayOrder)
                        return question;
                    else
                        return content;
                }
            }
        }

        public QuizAnswerSubmit CompleteQuestion(PublishQuizTmpModel publishQuizTmpModel, QuizAnswerSubmit quizAnswerSubmit)
        {

            var currentDate = DateTime.UtcNow;
            var quizAnswerStatsObj = new Db.QuizAnswerStats();
            Db.QuestionsInQuiz questionsInQuiz = null;
            Db.QuizQuestionStats quizQuestionStatsObj = null;
            List<Db.MediaVariablesDetails> mediaObjectList = null;
            List<Db.AnswerOptionsInQuizQuestions> answerOptionsInQuizQuestionsList = new List<Db.AnswerOptionsInQuizQuestions>();
            List<Db.QuizAnswerStats> quizAnswerStatsList = null;

            int answerType = -1;
            using (var UOWObj = new AutomationUnitOfWork())
            {
                //quizQuestionStatsObj = UOWObj.QuizQuestionStatsRepository.GetQuizQuestionStatsByQuizAttemptId(publishQuizTmpModel.QuizattemptId, publishQuizTmpModel.RequestQuestionId, (int)StatusEnum.Active).FirstOrDefault();
                quizQuestionStatsObj = UOWObj.QuizQuestionStatsRepository.Get(r => r.QuizAttemptId == publishQuizTmpModel.QuizattemptId && r.QuestionId == publishQuizTmpModel.RequestQuestionId && r.Status == (int)StatusEnum.Active).FirstOrDefault();

                if (quizQuestionStatsObj != null)
                {
                    questionsInQuiz = quizQuestionStatsObj.QuestionsInQuiz;
                    answerOptionsInQuizQuestionsList = questionsInQuiz.AnswerOptionsInQuizQuestions != null ? questionsInQuiz.AnswerOptionsInQuizQuestions.ToList() : new List<Db.AnswerOptionsInQuizQuestions>();
                    answerType = questionsInQuiz.AnswerType;
                    if (!publishQuizTmpModel.IsLastQuestionAttempted)
                    {

                        if(quizQuestionStatsObj.QuizAnswerStats != null && quizQuestionStatsObj.QuizAnswerStats.Any()) 
                        {

                            quizAnswerStatsList = quizQuestionStatsObj.QuizAnswerStats.ToList();
                            foreach (var quizQuestStats in quizAnswerStatsList) {
                                UOWObj.QuizAnswerStatsRepository.Delete(quizQuestStats);
                            }

                        }
                        



                        if (!publishQuizTmpModel.RequestAnswerId.Any())
                        {
                            if (questionsInQuiz != null && questionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single
                                   && questionsInQuiz.TimerRequired && answerOptionsInQuizQuestionsList != null)
                            {
                                publishQuizTmpModel.RequestAnswerId.Add(answerOptionsInQuizQuestionsList.FirstOrDefault(r => r.IsUnansweredType).Id);
                            }
                        }

                        if (answerType == (int)AnswerTypeEnum.Single || answerType == (int)AnswerTypeEnum.Multiple || answerType == (int)AnswerTypeEnum.DrivingLicense || answerType == (int)AnswerTypeEnum.LookingforJobs)
                        {
                            foreach (var obj in publishQuizTmpModel.RequestAnswerId)
                            {
                                var quizAnswerStats = new Db.QuizAnswerStats();
                                quizAnswerStats.QuizQuestionStatsId = quizQuestionStatsObj.Id;
                                quizAnswerStats.AnswerId = obj;
                                quizAnswerStats.AnswerText = null;
                                quizAnswerStats.AnswerOptionsInQuizQuestions = answerOptionsInQuizQuestionsList.FirstOrDefault(a => a.Id == obj);
                                quizAnswerStats.AnswerSecondaryText = null;
                                quizAnswerStats.CompanyId = publishQuizTmpModel.CompanyId;
                                quizAnswerStats.QuizAttemptId = publishQuizTmpModel.QuizattemptId;
                                UOWObj.QuizAnswerStatsRepository.Insert(quizAnswerStats);
                                UOWObj.Save();
                            }
                        }
                        else if (answerType == (int)AnswerTypeEnum.Availability)
                        {
                            if (publishQuizTmpModel.RequestedTextAnswerList != null && publishQuizTmpModel.RequestedTextAnswerList.Any())
                            {
                                foreach (var textAnswerObj in publishQuizTmpModel.RequestedTextAnswerList)
                                {
                                    foreach (var answerObj in textAnswerObj.Answers)
                                    {
                                        var quizAnswerStats = new Db.QuizAnswerStats();
                                        quizAnswerStats.QuizQuestionStatsId = quizQuestionStatsObj.Id;
                                        quizAnswerStats.AnswerId = textAnswerObj.AnswerId;
                                        quizAnswerStats.AnswerText = answerObj.AnswerText;
                                        quizAnswerStats.SubAnswerTypeId = answerObj.SubAnswerTypeId;
                                        quizAnswerStats.AnswerSecondaryText = answerObj.AnswerSecondaryText;
                                        quizAnswerStats.AnswerOptionsInQuizQuestions = answerOptionsInQuizQuestionsList.FirstOrDefault(a => a.Id == textAnswerObj.AnswerId);
                                        quizAnswerStats.Comment = answerObj.Comment;
                                        quizAnswerStats.CompanyId = publishQuizTmpModel.CompanyId;
                                        quizAnswerStats.QuizAttemptId = publishQuizTmpModel.QuizattemptId;
                                        UOWObj.QuizAnswerStatsRepository.Insert(quizAnswerStats);
                                        UOWObj.Save();
                                    }
                                }
                            }
                        }

                        else
                        {
                            if (publishQuizTmpModel.RequestedTextAnswerList != null && publishQuizTmpModel.RequestedTextAnswerList.Any())
                            {
                                foreach (var textAnswerObj in publishQuizTmpModel.RequestedTextAnswerList)
                                {
                                    foreach (var answerObj in textAnswerObj.Answers)
                                    {
                                        var quizAnswerStats = new Db.QuizAnswerStats();
                                        quizAnswerStats.QuizQuestionStatsId = quizQuestionStatsObj.Id;
                                        quizAnswerStats.AnswerId = textAnswerObj.AnswerId;
                                        quizAnswerStats.AnswerText = answerObj.AnswerText;
                                        quizAnswerStats.SubAnswerTypeId = answerObj.SubAnswerTypeId;
                                        quizAnswerStats.AnswerSecondaryText = answerObj.AnswerSecondaryText;
                                        quizAnswerStats.AnswerOptionsInQuizQuestions = answerOptionsInQuizQuestionsList.FirstOrDefault(a => a.Id == textAnswerObj.AnswerId);
                                        if (questionsInQuiz.EnableComment)
                                            quizAnswerStats.Comment = answerObj.Comment;
                                        quizAnswerStats.CompanyId = publishQuizTmpModel.CompanyId;
                                        quizAnswerStats.QuizAttemptId = publishQuizTmpModel.QuizattemptId;
                                        UOWObj.QuizAnswerStatsRepository.Insert(quizAnswerStats);
                                        UOWObj.Save();
                                    }
                                }
                            }
                        }

                        if (quizQuestionStatsObj != null)
                        {
                            quizQuestionStatsObj.CompletedOn = currentDate;
                            quizQuestionStatsObj.Status = (int)StatusEnum.Active;

                            UOWObj.QuizQuestionStatsRepository.Update(quizQuestionStatsObj);
                            UOWObj.Save();
                        }

                        var mediaList = publishQuizTmpModel.MediaVariableList.Where(r => r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER);
                        //var mediaList = UOWObj.MediaVariablesDetailsRepository.Get(r => r.QuizId == publishQuizTmpModel.QuizDetailId && r.ConfigurationDetailsId == publishQuizTmpModel.ConfigurationDetailId && r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER);
                        if (mediaList != null && mediaList.Any())
                        {
                            mediaObjectList = mediaList.ToList();
                        }

                    }

                }
            }

            if (questionsInQuiz != null)
            {

                quizAnswerSubmit.IsBackButtonEnable = (questionsInQuiz.RevealCorrectAnswer.HasValue && questionsInQuiz.RevealCorrectAnswer.Value) ? false : (questionsInQuiz.ViewPreviousQuestion || questionsInQuiz.EditAnswer);
            }


            #region SubmittedAnswer
            QuizAnswerSubmit.SubmittedAnswerResult submittedAnswer = null;
            if (publishQuizTmpModel.QuizType == (int)QuizTypeEnum.NPS || publishQuizTmpModel.QuizType == (int)QuizTypeEnum.Assessment || publishQuizTmpModel.QuizType == (int)QuizTypeEnum.AssessmentTemplate)
            {
                if (!(publishQuizTmpModel.RequestUsageType.HasValue && (publishQuizTmpModel.RequestUsageType.Value == (int)UsageTypeEnum.Chatbot || publishQuizTmpModel.RequestUsageType.Value == (int)UsageTypeEnum.WhatsAppChatbot)) && publishQuizTmpModel.IsrevealScore)
                {
                    if (questionsInQuiz.RevealCorrectAnswer.HasValue && questionsInQuiz.RevealCorrectAnswer.Value && (questionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple || questionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single))
                    {
                        submittedAnswer = new QuizAnswerSubmit.SubmittedAnswerResult();
                        submittedAnswer.AliasTextForCorrect = questionsInQuiz.AliasTextForCorrect;
                        submittedAnswer.AliasTextForIncorrect = questionsInQuiz.AliasTextForIncorrect;
                        submittedAnswer.AliasTextForYourAnswer = questionsInQuiz.AliasTextForYourAnswer;
                        submittedAnswer.AliasTextForCorrectAnswer = questionsInQuiz.AliasTextForCorrectAnswer;
                        submittedAnswer.AliasTextForExplanation = questionsInQuiz.AliasTextForExplanation;
                        submittedAnswer.AliasTextForNextButton = questionsInQuiz.AliasTextForNextButton;
                        submittedAnswer.CorrectAnswerDescription = questionsInQuiz.CorrectAnswerDescription;
                        submittedAnswer.ShowAnswerImage = questionsInQuiz.ShowAnswerImage;

                        submittedAnswer.SubmittedAnswerDetails = new List<QuizAnswerSubmit.SubmittedAnswerResult.SubmittedAnswer> {
                        new QuizAnswerSubmit.SubmittedAnswerResult.SubmittedAnswer
                        {
                            SubmittedAnswerTitle = notAttemptedQuesText,
                            SubmittedAnswerImage = string.Empty,
                            PublicIdForSubmittedAnswer = string.Empty
                        }
                        };
                        submittedAnswer.IsCorrect = false;

                        var submittedAnswerOption = answerOptionsInQuizQuestionsList.Where(r => !r.IsUnansweredType && publishQuizTmpModel.RequestAnswerId.Contains(r.Id));
                        if (submittedAnswerOption.Any())
                        {
                            submittedAnswer.SubmittedAnswerDetails = new List<QuizAnswerSubmit.SubmittedAnswerResult.SubmittedAnswer>();
                            foreach (var submittedAnswerOptionObj in submittedAnswerOption)
                            {
                                var submittedAnswerImage = submittedAnswerOptionObj.OptionImage;
                                var publicIdForSubmittedAnswer = submittedAnswerOptionObj.PublicId;
                                if (mediaObjectList != null && mediaObjectList.Any() && questionsInQuiz.ShowAnswerImage.HasValue && questionsInQuiz.ShowAnswerImage.Value)
                                {
                                    var mediaObj = mediaObjectList.FirstOrDefault(v => v.ObjectId == submittedAnswerOptionObj.Id);
                                    if (submittedAnswerOptionObj.EnableMediaFile && mediaObj != null)
                                    {
                                        submittedAnswerImage = mediaObj.ObjectValue;
                                        publicIdForSubmittedAnswer = mediaObj.ObjectPublicId;

                                        var newMedia = ExtractDynamicMedia(mediaObj, publishQuizTmpModel.LeadDetails, publishQuizTmpModel.UserMediaClassifications, publishQuizTmpModel.LeadOwners);
                                        if (newMedia != null)
                                        {
                                            submittedAnswerImage = newMedia.MediaUrl;
                                            publicIdForSubmittedAnswer = newMedia.MediaPublicId;
                                        }
                                    }
                                }


                                submittedAnswer.SubmittedAnswerDetails.Add(new QuizAnswerSubmit.SubmittedAnswerResult.SubmittedAnswer
                                {
                                    SubmittedAnswerTitle = VariableLinking(submittedAnswerOptionObj.Option, false, false, null, publishQuizTmpModel),
                                    SubmittedAnswerImage = submittedAnswerImage,
                                    PublicIdForSubmittedAnswer = publicIdForSubmittedAnswer,
                                    AutoPlay = submittedAnswerOptionObj.AutoPlay,
                                    SecondsToApply = submittedAnswerOptionObj.SecondsToApply,
                                    VideoFrameEnabled = submittedAnswerOptionObj.VideoFrameEnabled
                                });
                            }

                            if (questionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single)
                            {
                                submittedAnswer.IsCorrect = submittedAnswerOption.FirstOrDefault().IsCorrectAnswer.HasValue ? submittedAnswerOption.FirstOrDefault().IsCorrectAnswer.Value : false;
                            }
                            if (questionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && answerOptionsInQuizQuestionsList != null)
                            {
                                var filteranswerOptionsInQuizQuestionsList = answerOptionsInQuizQuestionsList.Where(r => r.Status == (int)StatusEnum.Active && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value);
                                if (filteranswerOptionsInQuizQuestionsList.Any())
                                {

                                    submittedAnswer.IsCorrect = filteranswerOptionsInQuizQuestionsList.Select(s => s.Id).OrderBy(s => s).SequenceEqual(quizQuestionStatsObj.QuizAnswerStats.Select(r => r.AnswerId).OrderBy(s => s));
                                }
                            }
                        }

                        submittedAnswer.CorrectAnswerDetails = new List<QuizAnswerSubmit.SubmittedAnswerResult.CorrectAnswer>();
                        if (!(submittedAnswer.IsCorrect.HasValue && submittedAnswer.IsCorrect.Value))
                        {
                            var correctAnswerOption = answerOptionsInQuizQuestionsList.Where(r => r.Status == (int)StatusEnum.Active && ((questionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value) || (questionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single && r.IsCorrectAnswer.HasValue && r.IsCorrectAnswer.Value)));

                            if (correctAnswerOption != null)
                            {
                                foreach (var correctAnswerOptionObj in correctAnswerOption)
                                {
                                    var correctAnswerImage = correctAnswerOptionObj.OptionImage;
                                    var publicIdForCorrectAnswer = correctAnswerOptionObj.PublicId;
                                    if (mediaObjectList != null && mediaObjectList.Any() && questionsInQuiz.ShowAnswerImage.HasValue && questionsInQuiz.ShowAnswerImage.Value)
                                    {
                                        var mediaObj = mediaObjectList.FirstOrDefault(v => v.ObjectId == correctAnswerOptionObj.Id);
                                        correctAnswerImage = mediaObj.ObjectValue;
                                        publicIdForCorrectAnswer = mediaObj.ObjectPublicId;

                                        var newMedia = ExtractDynamicMedia(mediaObj, publishQuizTmpModel.LeadDetails, publishQuizTmpModel.UserMediaClassifications, publishQuizTmpModel.LeadOwners);
                                        if (newMedia != null)
                                        {
                                            correctAnswerImage = newMedia.MediaUrl;
                                            publicIdForCorrectAnswer = newMedia.MediaPublicId;
                                        }

                                    }

                                    submittedAnswer.CorrectAnswerDetails.Add(new QuizAnswerSubmit.SubmittedAnswerResult.CorrectAnswer()
                                    {
                                        CorrectAnswerTitle = VariableLinking(correctAnswerOptionObj.Option, false, false, null, publishQuizTmpModel),
                                        CorrectAnswerImage = correctAnswerImage,
                                        PublicIdForCorrectAnswer = publicIdForCorrectAnswer,
                                        AutoPlay = correctAnswerOptionObj.AutoPlay,
                                        SecondsToApply = correctAnswerOptionObj.SecondsToApply,
                                        VideoFrameEnabled = correctAnswerOptionObj.VideoFrameEnabled
                                    });
                                }
                            }
                        }
                    }

                }
            }

            quizAnswerSubmit.SubmittedAnswer = submittedAnswer;


            #endregion

            object nextQuestionObj = null;
            if (questionsInQuiz != null)
            {
                if (questionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single || questionsInQuiz.AnswerType == (int)AnswerTypeEnum.LookingforJobs || questionsInQuiz.AnswerType == (int)AnswerTypeEnum.Availability || (questionsInQuiz.IsMultiRating && (questionsInQuiz.AnswerType == (int)AnswerTypeEnum.RatingEmoji || questionsInQuiz.AnswerType == (int)AnswerTypeEnum.RatingStarts)) || (questionsInQuiz.IsMultiRating && questionsInQuiz.AnswerType == (int)AnswerTypeEnum.NPS))
                {
                    int QuestionAnswertype = (int)BranchingLogicEnum.ANSWER;
                    if (questionsInQuiz.TemplateId != 0 && questionsInQuiz.TemplateId != null)
                    {
                        QuestionAnswertype = (int)BranchingLogicEnum.WHATSAPPTEMPLATEACTION;
                    }

                    nextQuestionObj = FetchNextQuestion(publishQuizTmpModel.QuizDetailId, publishQuizTmpModel.IsQuesAndContentInSameTable, publishQuizTmpModel.IsBranchingLogicEnabled, publishQuizTmpModel.RequestAnswerId.FirstOrDefault(), QuestionAnswertype);
                }
                else
                    nextQuestionObj = FetchNextQuestion(publishQuizTmpModel.QuizDetailId, publishQuizTmpModel.IsQuesAndContentInSameTable, publishQuizTmpModel.IsBranchingLogicEnabled, questionsInQuiz.Id, (int)BranchingLogicEnum.QUESTIONNEXT);
            }
            if (nextQuestionObj != null)
            {
                quizAnswerSubmit = GetNextQuestionObjectDetails(nextQuestionObj, quizAnswerSubmit, publishQuizTmpModel, questionsInQuiz, publishQuizTmpModel.RequestType);
            }
            else
            {
                quizAnswerSubmit = NextNullquestionCompleteQuestionLastResult(publishQuizTmpModel, quizAnswerSubmit);
            }

            return quizAnswerSubmit;
        }

        private QuizAnswerSubmit NextNullquestionCompleteQuestionLastResult(PublishQuizTmpModel publishQuizTmpModel, QuizAnswerSubmit quizAnswerSubmit)
        {
            quizAnswerSubmit.IsBackButtonEnable = false;
            quizAnswerSubmit.CompanyCode = publishQuizTmpModel.CompanyCode;
            if (publishQuizTmpModel.IsQuizResultinBranchingLogic)
            {
                quizAnswerSubmit = CompleteQuestionLastResultBranchingLogic(publishQuizTmpModel, quizAnswerSubmit);
            }
            else
            {
                if (publishQuizTmpModel.QuizType != (int)QuizTypeEnum.Personality && publishQuizTmpModel.QuizType != (int)QuizTypeEnum.PersonalityTemplate)
                {
                    quizAnswerSubmit = CompleteQuestionLastResult(publishQuizTmpModel, quizAnswerSubmit);
                }
                else
                {
                    quizAnswerSubmit = CompleteQuestionLastResultBranchingPersonality(publishQuizTmpModel, quizAnswerSubmit);
                }
            }
            return quizAnswerSubmit;
        }

        private QuizAnswerSubmit CompleteQuestionLastResultBranchingLogic(PublishQuizTmpModel publishQuizTmpModel, QuizAnswerSubmit quizAnswerSubmit)
        {
            Db.ResultSettings resultSetting = null;
            var quizAnswerSubmitResultScore = new QuizAnswerSubmit.Result();
            #region resultSetting is null
            string scoreValueTxt = string.Empty;
            float correctAnsCount = 0;
            var showScoreValue = false;
            using (var UOWObj = new AutomationUnitOfWork())
            {
                //resultSetting = UOWObj.ResultSettingsRepository.Get(r => r.QuizId == publishQuizTmpModel.QuizDetailId).FirstOrDefault();
                resultSetting = UOWObj.ResultSettingsRepository.GetResultSettingsRepositoryExtension(publishQuizTmpModel.QuizDetailId).FirstOrDefault();

                //var attemptedQuestions = UOWObj.QuizQuestionStatsRepository.Get(t => t.QuizAttemptId == publishQuizTmpModel.QuizattemptId && t.Status == (int)StatusEnum.Active && t.CompletedOn != null && (t.QuestionsInQuiz.TimerRequired ? true : t.QuizAnswerStats.Any(r => !r.AnswerOptionsInQuizQuestions.IsUnansweredType)));

                var attemptedQuestions = UOWObj.QuizQuestionStatsRepository.Get(t => t.QuizAttemptId == publishQuizTmpModel.QuizattemptId && t.Status == (int)StatusEnum.Active && t.CompletedOn != null && t.QuestionsInQuiz.TemplateId == null && (t.QuestionsInQuiz.TimerRequired ? true : t.QuizAnswerStats.Any(r => !r.AnswerOptionsInQuizQuestions.IsUnansweredType)));

                if ((publishQuizTmpModel.QuizType == (int)QuizTypeEnum.Score || publishQuizTmpModel.QuizType == (int)QuizTypeEnum.ScoreTemplate) && attemptedQuestions.Any(r => r.QuestionsInQuiz.Status == (int)StatusEnum.Active && (r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple)))
                {
                    scoreValueTxt = resultSetting.CustomTxtForScoreValueInResult ?? "You scored a%score%";
                    correctAnsCount = attemptedQuestions.Sum(a => a.QuizAnswerStats.Sum(x => x.AnswerOptionsInQuizQuestions.AssociatedScore)).Value;
                    showScoreValue = true;
                }
                else if ((publishQuizTmpModel.QuizType == (int)QuizTypeEnum.Assessment || publishQuizTmpModel.QuizType == (int)QuizTypeEnum.AssessmentTemplate) && attemptedQuestions.Any(r => (r.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Any(t => t.Status == (int)StatusEnum.Active && t.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && t.IsCorrectForMultipleAnswer.HasValue && t.IsCorrectForMultipleAnswer.Value)) || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single))
                {
                    scoreValueTxt = resultSetting.CustomTxtForScoreValueInResult ?? "YOU GOT%score%OUT OF%total%CORRECT";
                    correctAnsCount = attemptedQuestions.Count(a => (a.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && ((a.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Select(s => s.Id).OrderBy(s => s)).SequenceEqual(a.QuizAnswerStats.Select(r => r.AnswerId).OrderBy(s => s)))) || (a.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single && a.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer == true));
                    showScoreValue = true;
                }

                scoreValueTxt = scoreValueTxt == null ? string.Empty : scoreValueTxt.Replace("%score%", ' ' + correctAnsCount.ToString() + ' ').Replace("%total%", ' ' + attemptedQuestions.Count(t => (t.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Any(r => r.Status == (int)StatusEnum.Active && r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value)) || t.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single).ToString() + ' ');
            }
            quizAnswerSubmitResultScore.ResultScoreValueTxt = scoreValueTxt;
            quizAnswerSubmitResultScore.ShowScoreValue = showScoreValue;
            quizAnswerSubmit.ShowLeadUserForm = false;
            quizAnswerSubmit.ResultScore = quizAnswerSubmitResultScore;

            #endregion
            return quizAnswerSubmit;
        }

        private QuizAnswerSubmit CompleteQuestionLastResult(PublishQuizTmpModel publishQuizTmpModel, QuizAnswerSubmit quizAnswerSubmit)
        {
            Db.ResultSettings resultSetting = null;
            var IsResultInquizBranching = false;
            List<Db.QuizResults> quizresultList = null;
            var quizAnswerSubmitResultScore = new QuizAnswerSubmit.Result();
            #region non personality result Setting

            string scoreValueTxt = string.Empty;
            float correctAnsCount = 0;
            using (var UOWObj = new AutomationUnitOfWork())
            {
                //resultSetting = UOWObj.ResultSettingsRepository.Get(r => r.QuizId == publishQuizTmpModel.QuizDetailId).FirstOrDefault();
                resultSetting = UOWObj.ResultSettingsRepository.GetResultSettingsRepositoryExtension(publishQuizTmpModel.QuizDetailId).FirstOrDefault();
                //quizresultList = UOWObj.QuizResultsRepository.Get(r => r.QuizId == publishQuizTmpModel.QuizDetailId).ToList();
                quizresultList = UOWObj.QuizResultsRepository.GetQuizResultsRepositoryExtension(publishQuizTmpModel.QuizDetailId).ToList();

                //var attemptedQuestions = UOWObj.QuizQuestionStatsRepository.Get(t => t.QuizAttemptId == publishQuizTmpModel.QuizattemptId && t.Status == (int)StatusEnum.Active && t.CompletedOn != null && (t.QuestionsInQuiz.TimerRequired ? true : t.QuizAnswerStats.Any(r => !r.AnswerOptionsInQuizQuestions.IsUnansweredType)));

                var attemptedQuestions = UOWObj.QuizQuestionStatsRepository.Get(t => t.QuizAttemptId == publishQuizTmpModel.QuizattemptId && t.Status == (int)StatusEnum.Active && t.CompletedOn != null && t.QuestionsInQuiz.TemplateId == null &&
                     (t.QuestionsInQuiz.TimerRequired ? true : t.QuizAnswerStats.Any(r => !r.AnswerOptionsInQuizQuestions.IsUnansweredType)));

                if (publishQuizTmpModel.QuizType == (int)QuizTypeEnum.Score || publishQuizTmpModel.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                {
                    scoreValueTxt = resultSetting.CustomTxtForScoreValueInResult ?? "You scored a%score%";
                    correctAnsCount = attemptedQuestions.Sum(a => a.QuizAnswerStats.Sum(x => x.AnswerOptionsInQuizQuestions.AssociatedScore)).Value;
                }
                else if (publishQuizTmpModel.QuizType == (int)QuizTypeEnum.NPS)
                {
                    scoreValueTxt = resultSetting.CustomTxtForScoreValueInResult ?? string.Empty;
                    var attemptedQuestionsofNPSType = attemptedQuestions.Where(a => a.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.NPS);
                    if (attemptedQuestionsofNPSType != null && attemptedQuestionsofNPSType.Any())
                    {
                        correctAnsCount = Convert.ToInt32(attemptedQuestionsofNPSType.Sum(a => a.QuizAnswerStats.Sum(x => Convert.ToInt32(x.AnswerText))) / attemptedQuestionsofNPSType.Count());
                    }
                }
                else
                {
                    scoreValueTxt = resultSetting.CustomTxtForScoreValueInResult ?? "YOU GOT%score%OUT OF%total%CORRECT";
                    correctAnsCount = attemptedQuestions.Count(a => (a.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && ((a.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Select(s => s.Id).OrderBy(s => s)).SequenceEqual(a.QuizAnswerStats.Select(r => r.AnswerId).OrderBy(s => s)))) || (a.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single && a.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer == true));
                }

                scoreValueTxt = string.IsNullOrEmpty(scoreValueTxt) ? string.Empty : scoreValueTxt.Replace("%score%", ' ' + correctAnsCount.ToString() + ' ').Replace("%total%", ' ' + attemptedQuestions.Count(t => (t.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Any(r => r.Status == (int)StatusEnum.Active && r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value)) || t.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single).ToString() + ' ');


                if (resultSetting.ShowScoreValue.HasValue && resultSetting.ShowScoreValue.Value && (((publishQuizTmpModel.QuizType == (int)QuizTypeEnum.Score || publishQuizTmpModel.QuizType == (int)QuizTypeEnum.ScoreTemplate) && attemptedQuestions.Any(r => r.QuestionsInQuiz.Status == (int)StatusEnum.Active && (r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple))) || attemptedQuestions.Any(r => (r.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Any(t => t.Status == (int)StatusEnum.Active && t.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && t.IsCorrectForMultipleAnswer.HasValue && t.IsCorrectForMultipleAnswer.Value)) || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single)))
                {
                    quizAnswerSubmitResultScore.ResultScoreValueTxt = scoreValueTxt;
                    quizAnswerSubmitResultScore.ShowScoreValue = true;
                }
                else
                {
                    quizAnswerSubmitResultScore.ResultScoreValueTxt = string.Empty;
                    quizAnswerSubmitResultScore.ShowScoreValue = false;
                }

                var resultIdList = new List<int>();
                var result = quizresultList.Where(r => r.Status == (int)StatusEnum.Active && r.MinScore <= correctAnsCount && correctAnsCount <= r.MaxScore).FirstOrDefault();
                Db.MediaVariablesDetails mediaObj = null;
                //mediaObj = UOWObj.MediaVariablesDetailsRepository.Get(i => i.QuizId == publishQuizTmpModel.QuizDetailId).FirstOrDefault(r => r.ConfigurationDetailsId == publishQuizTmpModel.ConfigurationDetailId && r.ObjectTypeId == (int)BranchingLogicEnum.RESULT && r.ObjectId == result.Id);
                mediaObj = publishQuizTmpModel.MediaVariableList.Where(r => r.ObjectTypeId == (int)BranchingLogicEnum.RESULT && r.ObjectId == result.Id).FirstOrDefault();
                quizAnswerSubmit.ShowLeadUserForm = false;

                if (publishQuizTmpModel.LeadUserId == null && publishQuizTmpModel.RecruiterUserId == 0)
                {
                    int? formId;
                    int? flowOrder;
                    //var personalityResultSetting = UOWObj.PersonalityResultSettingRepository.Get(v => v.QuizId == publishQuizTmpModel.QuizDetailId).FirstOrDefault();
                    var personalityResultSetting = UOWObj.PersonalityResultSettingRepository.GetPersonalityResultSettingByQuizId(publishQuizTmpModel.QuizDetailId).FirstOrDefault();
                    quizAnswerSubmit.ShowLeadUserForm = CheckleadFormQuizResult(publishQuizTmpModel, result, quizresultList, personalityResultSetting, out formId, out flowOrder);
                    quizAnswerSubmit.FormId = formId;
                    quizAnswerSubmit.FlowOrder = flowOrder;
                }

                quizAnswerSubmitResultScore.ShowInternalTitle = result.ShowInternalTitle;
                quizAnswerSubmitResultScore.ShowExternalTitle = result.ShowExternalTitle;
                quizAnswerSubmitResultScore.ShowDescription = result.ShowDescription;
                var msgVariables = publishQuizTmpModel.QuizVariables?.Where(v => v.ObjectTypes == (int)QuizVariableObjectTypes.RESULT && v.ObjectId == result.Id).FirstOrDefault()?.Variables;
                CommonStaticData.VacancyVariableReplace(publishQuizTmpModel.ContactObject, msgVariables, publishQuizTmpModel.CompanyCode);
                quizAnswerSubmitResultScore.Title = VariableLinking(result.Title, false, (resultSetting.ShowScoreValue.HasValue && resultSetting.ShowScoreValue.Value), scoreValueTxt, publishQuizTmpModel, msgVariables);
                quizAnswerSubmitResultScore.InternalTitle = VariableLinking(result.InternalTitle, false, (resultSetting.ShowScoreValue.HasValue && resultSetting.ShowScoreValue.Value), scoreValueTxt, publishQuizTmpModel, msgVariables);

                if (result.EnableMediaFile && mediaObj != null)
                {
                    if (mediaObj != null)
                    {
                        quizAnswerSubmitResultScore.Image = result.ShowResultImage ? mediaObj.ObjectValue : string.Empty;
                        quizAnswerSubmitResultScore.PublicIdForResult = result.ShowResultImage ? mediaObj.ObjectPublicId : string.Empty;

                        var newMedia = result.ShowResultImage ? ExtractDynamicMedia(mediaObj, publishQuizTmpModel.LeadDetails, publishQuizTmpModel.UserMediaClassifications, publishQuizTmpModel.LeadOwners) : null;
                        if (newMedia != null)
                        {
                            quizAnswerSubmitResultScore.Image = newMedia.MediaUrl;
                            quizAnswerSubmitResultScore.PublicIdForResult = newMedia.MediaPublicId;
                        }
                    }
                }
                else
                {
                    quizAnswerSubmitResultScore.Image = result.ShowResultImage ? result.Image : string.Empty;
                    quizAnswerSubmitResultScore.PublicIdForResult = result.ShowResultImage ? result.PublicId : string.Empty;
                }

                quizAnswerSubmitResultScore.Description = VariableLinking(result.Description, false, (resultSetting.ShowScoreValue.HasValue && resultSetting.ShowScoreValue.Value), scoreValueTxt, publishQuizTmpModel, msgVariables);
                quizAnswerSubmitResultScore.HideActionButton = result.HideCallToAction.ToBoolValue();
                quizAnswerSubmitResultScore.ActionButtonURL = quizAnswerSubmitResultScore.HideActionButton ? string.Empty : result.ActionButtonURL;
                quizAnswerSubmitResultScore.OpenLinkInNewTab = quizAnswerSubmitResultScore.HideActionButton ? false : result.OpenLinkInNewTab;
                quizAnswerSubmitResultScore.ActionButtonTxtSize = quizAnswerSubmitResultScore.HideActionButton ? string.Empty : result.ActionButtonTxtSize;
                quizAnswerSubmitResultScore.ActionButtonColor = quizAnswerSubmitResultScore.HideActionButton ? string.Empty : result.ActionButtonColor;
                quizAnswerSubmitResultScore.ActionButtonTitleColor = quizAnswerSubmitResultScore.HideActionButton ? string.Empty : result.ActionButtonTitleColor;
                quizAnswerSubmitResultScore.ActionButtonText = quizAnswerSubmitResultScore.HideActionButton ? string.Empty : result.ActionButtonText;
                quizAnswerSubmitResultScore.AutoPlay = result.AutoPlay;
                quizAnswerSubmitResultScore.SecondsToApply = result.SecondsToApply ?? "0";
                quizAnswerSubmitResultScore.VideoFrameEnabled = result.VideoFrameEnabled ?? false;
                quizAnswerSubmitResultScore.DisplayOrderForTitle = result.DisplayOrderForTitle;
                quizAnswerSubmitResultScore.DisplayOrderForTitleImage = result.DisplayOrderForTitleImage;
                quizAnswerSubmitResultScore.DisplayOrderForDescription = result.DisplayOrderForDescription;
                quizAnswerSubmitResultScore.DisplayOrderForNextButton = result.DisplayOrderForNextButton;



                //var quizStatsObj = UOWObj.QuizStatsRepository.Get(v => v.QuizAttemptId == publishQuizTmpModel.QuizattemptId).FirstOrDefault();
                var quizStatsObj = UOWObj.QuizStatsRepository.GetQuizStatsQuizAttemptId(publishQuizTmpModel.QuizattemptId).FirstOrDefault();
                if (quizStatsObj != null)
                {
                    quizStatsObj.ResultId = result.Id;
                    UOWObj.QuizStatsRepository.Update(quizStatsObj);
                    UOWObj.Save();
                }
                if (quizStatsObj != null)
                {
                    quizStatsObj.ResultId = result.Id;
                    UOWObj.QuizStatsRepository.Update(quizStatsObj);
                    UOWObj.Save();
                }


                if (resultSetting.ShowCorrectAnswer.ToBoolValue())
                {
                    quizAnswerSubmitResultScore.AnswerKeyCustomTxt = resultSetting.CustomTxtForAnswerKey;
                    quizAnswerSubmitResultScore.YourAnswerCustomTxt = resultSetting.CustomTxtForYourAnswer;
                    quizAnswerSubmitResultScore.CorrectAnswerCustomTxt = resultSetting.CustomTxtForCorrectAnswer;
                    quizAnswerSubmitResultScore.ExplanationCustomTxt = resultSetting.CustomTxtForExplanation;
                    quizAnswerSubmitResultScore.ShowCorrectAnswer = true;

                    quizAnswerSubmitResultScore.AttemptedQuestionAnswerDetails = new List<QuizAnswerSubmit.AnswerResult>();

                    #region attempted question

                    foreach (var item in attemptedQuestions.Where(r => r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Short || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Long))
                    {
                        var correctAnswerTxt = string.Empty;
                        bool? IsCorrectValue = null;
                        int associatedScore = default(int);
                        var yourAnswer = string.Empty;

                        if (item.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Any())
                        {
                            if (publishQuizTmpModel.QuizType == (int)QuizTypeEnum.Score || publishQuizTmpModel.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                associatedScore = item.QuizAnswerStats.Sum(x => x.AnswerOptionsInQuizQuestions.AssociatedScore).Value;
                            else
                                IsCorrectValue = (item.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Select(s => s.Id).OrderBy(s => s)).SequenceEqual(item.QuizAnswerStats.Select(r => r.AnswerId).OrderBy(s => s));
                            correctAnswerTxt = VariableLinking(string.Join(",", item.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Select(t => t.Option).Select(s => "'" + s + "'")), false, quizAnswerSubmitResultScore.ShowScoreValue, scoreValueTxt, publishQuizTmpModel);
                        }
                        else if (item.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single)
                        {
                            correctAnswerTxt = VariableLinking(item.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(t => t.IsCorrectAnswer.HasValue ? t.IsCorrectAnswer.Value : false).Select(t => t.Option).FirstOrDefault(), false, quizAnswerSubmitResultScore.ShowScoreValue, scoreValueTxt, publishQuizTmpModel);
                            if (publishQuizTmpModel.QuizType == (int)QuizTypeEnum.Score || publishQuizTmpModel.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                associatedScore = item.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.AssociatedScore.Value;
                            else
                                IsCorrectValue = item.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer.HasValue && item.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer.Value;
                        }

                        if (item.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple || item.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single)
                            yourAnswer = VariableLinking(string.Join(",", item.QuizAnswerStats.Where(t => !t.AnswerOptionsInQuizQuestions.IsUnansweredType).Select(t => t.AnswerOptionsInQuizQuestions.Option).Select(s => "'" + s + "'")), false, quizAnswerSubmitResultScore.ShowScoreValue, scoreValueTxt, publishQuizTmpModel);

                        else
                            yourAnswer = item.QuizAnswerStats.Select(t => t.AnswerText).FirstOrDefault();

                        quizAnswerSubmitResultScore.AttemptedQuestionAnswerDetails.Add(new QuizAnswerSubmit.AnswerResult
                        {
                            Question = VariableLinking(item.QuestionsInQuiz.Question, false, quizAnswerSubmitResultScore.ShowScoreValue, scoreValueTxt, publishQuizTmpModel),
                            YourAnswer = string.IsNullOrEmpty(yourAnswer) ? notAttemptedQuesText : yourAnswer,
                            CorrectAnswer = (publishQuizTmpModel.QuizType == (int)QuizTypeEnum.Score || publishQuizTmpModel.QuizType == (int)QuizTypeEnum.ScoreTemplate) ? string.Empty : correctAnswerTxt,
                            IsCorrect = IsCorrectValue,
                            AnswerExplanation = item.QuestionsInQuiz.CorrectAnswerDescription,
                            AssociatedScore = (publishQuizTmpModel.QuizType == (int)QuizTypeEnum.Score || publishQuizTmpModel.QuizType == (int)QuizTypeEnum.ScoreTemplate) ? associatedScore : default(int),
                        });
                    }

                    #endregion
                }
                else
                {
                    quizAnswerSubmitResultScore.ShowCorrectAnswer = false;
                }
            }
            #endregion


            quizAnswerSubmit.ResultScore = quizAnswerSubmitResultScore;
            quizAnswerSubmit.QuestionType = (int)BranchingLogicEnum.RESULT;
            return quizAnswerSubmit;


        }


        private QuizAnswerSubmit QuizResultpersonalityResultSetting(PublishQuizTmpModel publishQuizTmpModel, Db.QuizResults result, QuizAnswerSubmit quizAnswerSubmit)
        {
            #region Personality result Setting
            bool showLeadUserForm = false;


            using (var UOWObj = new AutomationUnitOfWork())
            {
                var attemptedQuestions = UOWObj.QuizQuestionStatsRepository.Get(t => t.QuizAttemptId == publishQuizTmpModel.QuizattemptId && t.Status == (int)StatusEnum.Active && t.CompletedOn != null && (t.QuestionsInQuiz.TimerRequired ? true : t.QuizAnswerStats.Any(r => !r.AnswerOptionsInQuizQuestions.IsUnansweredType)));

                var personalityAnswerResultList = attemptedQuestions.SelectMany(x => x.QuizAnswerStats.SelectMany(r => r.AnswerOptionsInQuizQuestions.PersonalityAnswerResultMapping));
                var personalitySetting = UOWObj.PersonalityResultSettingRepository.Get(v => v.QuizId == publishQuizTmpModel.QuizDetailId).FirstOrDefault();
                var quizStatsObj = UOWObj.QuizStatsRepository.Get(v => v.QuizAttemptId == publishQuizTmpModel.QuizattemptId).FirstOrDefault();

                if (personalitySetting != null && personalitySetting.Status == (int)StatusEnum.Active)
                {
                    #region personality Setting Active

                    var personalityResultList = new List<QuizAnswerSubmit.PersonalityResult>();
                    var countResult = 0;

                    if (personalityAnswerResultList.Any())
                    {
                        #region correlation is available
                        var mappedResultList = personalityAnswerResultList.OrderBy(x => x.QuizResults.DisplayOrder).GroupBy(r => r.ResultId).OrderByDescending(i => i.Count()).Take(personalitySetting.MaxResult).Select(x => x.Key).ToList();

                        //var quizresultList = UOWObj.QuizResultsRepository.Get(r => r.QuizId == publishQuizTmpModel.QuizDetailId && !r.IsPersonalityCorrelatedResult && mappedResultList.Contains(r.Id)).OrderBy(x => mappedResultList.IndexOf(x.Id));
                        var quizresultList = UOWObj.QuizResultsRepository.GetQuizResultsRepositoryExtension(publishQuizTmpModel.QuizDetailId).Where(r => !r.IsPersonalityCorrelatedResult && mappedResultList.Contains(r.Id)).OrderBy(x => mappedResultList.IndexOf(x.Id));

                        foreach (var quizresult in quizresultList)
                        {
                            QuizAnswerSubmit.PersonalityResult personalityresult = new QuizAnswerSubmit.PersonalityResult();
                            personalityresult.Title = VariableLinking(quizresult.Title, false, false, null, publishQuizTmpModel);
                            personalityresult.InternalTitle = VariableLinking(quizresult.InternalTitle, false, false, null, publishQuizTmpModel);
                            personalityresult.Description = VariableLinking(quizresult.Description, false, false, null, publishQuizTmpModel);
                            personalityresult.Image = quizresult.Image;
                            personalityresult.ResultId = quizresult.Id;
                            personalityresult.GraphColor = personalitySetting.GraphColor;
                            personalityresult.ButtonColor = personalitySetting.ButtonColor;
                            personalityresult.ButtonFontColor = personalitySetting.ButtonFontColor;
                            personalityresult.MaxResult = personalitySetting.MaxResult;
                            personalityresult.SideButtonText = personalitySetting.SideButtonText;
                            personalityresult.IsFullWidthEnable = personalitySetting.IsFullWidthEnable;
                            int count = personalityAnswerResultList.Where(k => k.ResultId == quizresult.Id).Count();
                            personalityresult.Percentage = (int)Math.Round((double)(100 * count) / personalityAnswerResultList.Count());
                            personalityresult.ShowInternalTitle = quizresult.ShowInternalTitle;
                            personalityresult.ShowExternalTitle = quizresult.ShowExternalTitle;
                            personalityresult.ShowDescription = quizresult.ShowDescription;
                            personalityResultList.Add(personalityresult);

                            if (quizStatsObj != null)
                            {
                                if (countResult == 0)
                                {
                                    quizStatsObj.ResultId = quizresult.Id;
                                    UOWObj.QuizStatsRepository.Update(quizStatsObj);
                                }
                                else
                                {
                                    InsertQuizStat(quizStatsObj.QuizAttemptId, quizresult.Id, quizStatsObj.StartedOn);
                                }
                                UOWObj.Save();
                                countResult++;
                            }
                        }

                        if (publishQuizTmpModel.LeadUserId == null && publishQuizTmpModel.RecruiterUserId == 0)
                        {
                            //todo :check
                            int? formId;
                            int? flowOrder;
                            CheckleadForm(publishQuizTmpModel, personalityResultList, quizresultList, personalitySetting, out formId, out flowOrder);
                            quizAnswerSubmit.FormId = formId;
                            quizAnswerSubmit.FlowOrder = flowOrder;
                        }

                        #endregion
                    }
                    else
                    {
                        #region correlation is not available

                        //var quizresultList = UOWObj.QuizResultsRepository.Get(r => r.QuizId == publishQuizTmpModel.QuizDetailId).OrderBy(x => x.DisplayOrder).Take(personalitySetting.MaxResult);
                        var quizresultList = UOWObj.QuizResultsRepository.GetQuizResultsRepositoryExtension(publishQuizTmpModel.QuizDetailId).OrderBy(x => x.DisplayOrder).Take(personalitySetting.MaxResult);

                        foreach (var quizresult in quizresultList)
                        {
                            QuizAnswerSubmit.PersonalityResult personalityresult = new QuizAnswerSubmit.PersonalityResult();
                            personalityresult.Title =
                            VariableLinking(quizresult.Title, false, false, null, publishQuizTmpModel);
                            personalityresult.InternalTitle =
                            VariableLinking(quizresult.InternalTitle, false, false, null, publishQuizTmpModel);
                            personalityresult.Image = quizresult.Image;
                            personalityresult.ResultId = quizresult.Id;
                            personalityresult.GraphColor = personalitySetting.GraphColor;
                            personalityresult.ButtonColor = personalitySetting.ButtonColor;
                            personalityresult.ButtonFontColor = personalitySetting.ButtonFontColor;
                            personalityresult.MaxResult = personalitySetting.MaxResult;
                            personalityresult.SideButtonText = personalitySetting.SideButtonText;
                            personalityresult.IsFullWidthEnable = personalitySetting.IsFullWidthEnable;
                            personalityresult.Description = VariableLinking(quizresult.Description, false, false, null, publishQuizTmpModel);
                            personalityresult.Percentage = null;
                            personalityresult.ShowInternalTitle = quizresult.ShowInternalTitle;
                            personalityresult.ShowExternalTitle = quizresult.ShowExternalTitle;
                            personalityresult.ShowDescription = quizresult.ShowDescription;
                            personalityResultList.Add(personalityresult);

                            if (quizStatsObj != null)
                            {
                                if (countResult == 0)
                                {
                                    quizStatsObj.ResultId = quizresult.Id;
                                    UOWObj.QuizStatsRepository.Update(quizStatsObj);
                                }
                                else
                                {
                                    InsertQuizStat(quizStatsObj.QuizAttemptId, quizresult.Id, quizStatsObj.StartedOn);
                                }
                                UOWObj.Save();
                                countResult++;
                            }
                        }

                        if (publishQuizTmpModel.LeadUserId == null && publishQuizTmpModel.RecruiterUserId == 0)
                        {

                            int? formId; int? flowOrder;
                            CheckleadForm(publishQuizTmpModel, personalityResultList, quizresultList, personalitySetting, out formId, out flowOrder);
                            quizAnswerSubmit.FormId = formId;
                            quizAnswerSubmit.FlowOrder = flowOrder;
                        }


                        #endregion
                    }


                    var quizAnswerSubmitResultScore = quizAnswerSubmit.ResultScore ?? new QuizAnswerSubmit.Result();
                    quizAnswerSubmitResultScore.ShowInternalTitle = true;
                    quizAnswerSubmitResultScore.ShowExternalTitle = true;
                    quizAnswerSubmitResultScore.ShowDescription = true;
                    quizAnswerSubmitResultScore.Title = personalitySetting.Title;
                    quizAnswerSubmitResultScore.PersonalityResultList = personalityResultList.OrderByDescending(x => x.Percentage).ToList();
                    quizAnswerSubmit.ResultScore = quizAnswerSubmitResultScore;
                    #endregion
                }
                else
                {
                    #region if there is correlation available but personalitySetting is disabled

                    if (result.IsPersonalityCorrelatedResult && personalityAnswerResultList.Any())
                    {
                        result = UOWObj.QuizResultsRepository.Get(v => v.QuizId == publishQuizTmpModel.QuizDetailId).FirstOrDefault(x => x.Id == personalityAnswerResultList.OrderBy(r => r.QuizResults.DisplayOrder).GroupBy(r => r.ResultId).OrderByDescending(i => i.Count()).First().Key);
                    }

                    //TOdo: Dharminder
                    //int? formId,  int? flowOrder;
                    //CheckleadForm(publishQuizTmpModel, personalityResultList, quizresultList, personalitySetting, out formId, out flowOrder);
                    //quizAnswerSubmit.FormId = formId;
                    //quizAnswerSubmit.FlowOrder = flowOrder;

                    //quizAnswerSubmit.ShowLeadUserForm = (Mode == "PREVIEW" || Mode == "PREVIEWTEMPLATE") ? false : (resultIdList.Any() ? (resultIdList.Any(r => r == result.Id || quizComponentLogsList.Any(q => q.DraftedObjectId == r && q.PublishedObjectId == result.Id))) : (quizAttemptsObj.LeadUserId == null && quizAttemptsObj.RecruiterUserId == null && ((quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Personality && quizDetails.PersonalityResultSetting.FirstOrDefault().Status == (int)StatusEnum.Active) ? quizDetails.PersonalityResultSetting.FirstOrDefault().ShowLeadUserForm : result.ShowLeadUserForm)));



                    //if (leadFormDetailofResultsLst != null && leadFormDetailofResultsLst.Any() && quizAnswerSubmit.ShowLeadUserForm)
                    //{
                    //    var leadFormDetailofResultsObj = leadFormDetailofResultsLst.FirstOrDefault(r => r.ResultId == result.Id || quizComponentLogsList.Any(q => q.DraftedObjectId == r.ResultId && q.PublishedObjectId == result.Id));
                    //    quizAnswerSubmit.FormId = leadFormDetailofResultsObj.FormId;
                    //    quizAnswerSubmit.FlowOrder = leadFormDetailofResultsObj.FlowOrder;
                    //}

                    var quizAnswerSubmitResultScore = new QuizAnswerSubmit.Result();
                    quizAnswerSubmitResultScore.Image = result.ShowResultImage ? result.Image : string.Empty;
                    quizAnswerSubmitResultScore.PublicIdForResult = result.ShowResultImage ? result.PublicId : string.Empty;
                    if (result.EnableMediaFile)
                    {
                        var mediaObjList = publishQuizTmpModel.MediaVariableList.Where(r => r.ObjectTypeId == (int)BranchingLogicEnum.RESULT && r.ObjectId == result.Id);
                        //var mediaObjList = UOWObj.MediaVariablesDetailsRepository.Get(r => r.QuizId == publishQuizTmpModel.QuizDetailId && r.ConfigurationDetailsId == publishQuizTmpModel.ConfigurationDetailId && r.ObjectTypeId == (int)BranchingLogicEnum.RESULT && r.ObjectId == result.Id);
                        if (mediaObjList != null && mediaObjList.Any())
                        {
                            var mediaObj = mediaObjList.FirstOrDefault();
                            if (mediaObj != null)
                            {
                                quizAnswerSubmitResultScore.Image = result.ShowResultImage ? mediaObj.ObjectValue : string.Empty;
                                quizAnswerSubmitResultScore.PublicIdForResult = result.ShowResultImage ? mediaObj.ObjectPublicId : string.Empty;

                                var newMedia = ExtractDynamicMedia(mediaObj, publishQuizTmpModel.LeadDetails, publishQuizTmpModel.UserMediaClassifications, publishQuizTmpModel.LeadOwners);
                                if (newMedia != null)
                                {
                                    quizAnswerSubmitResultScore.Image = newMedia.MediaUrl;
                                    quizAnswerSubmitResultScore.PublicIdForResult = newMedia.MediaPublicId;
                                }
                            }
                        }
                    }

                    //quizAnswerSubmitResultScore.HideActionButton = result.HideCallToAction.HasValue ? result.HideCallToAction.Value : false;
                    quizAnswerSubmitResultScore.HideActionButton = result.HideCallToAction.ToBoolValue();
                    quizAnswerSubmitResultScore.ActionButtonURL = quizAnswerSubmitResultScore.HideActionButton ? string.Empty : result.ActionButtonURL;
                    quizAnswerSubmitResultScore.OpenLinkInNewTab = quizAnswerSubmitResultScore.HideActionButton ? false : result.OpenLinkInNewTab;
                    quizAnswerSubmitResultScore.ActionButtonTxtSize = quizAnswerSubmitResultScore.HideActionButton ? string.Empty : result.ActionButtonTxtSize;
                    quizAnswerSubmitResultScore.ActionButtonColor = quizAnswerSubmitResultScore.HideActionButton ? string.Empty : result.ActionButtonColor;
                    quizAnswerSubmitResultScore.ActionButtonTitleColor = quizAnswerSubmitResultScore.HideActionButton ? string.Empty : result.ActionButtonTitleColor;
                    quizAnswerSubmitResultScore.ActionButtonText = quizAnswerSubmitResultScore.HideActionButton ? string.Empty : result.ActionButtonText;

                    quizAnswerSubmitResultScore.DisplayOrderForTitle = quizAnswerSubmitResultScore.DisplayOrderForTitle;
                    quizAnswerSubmitResultScore.DisplayOrderForTitleImage = quizAnswerSubmitResultScore.DisplayOrderForTitleImage;
                    quizAnswerSubmitResultScore.DisplayOrderForDescription = quizAnswerSubmitResultScore.DisplayOrderForDescription;
                    quizAnswerSubmitResultScore.DisplayOrderForNextButton = quizAnswerSubmitResultScore.DisplayOrderForNextButton;

                    var resultSetting = UOWObj.ResultSettingsRepository.Get(v => v.QuizId == publishQuizTmpModel.QuizDetailId).FirstOrDefault();
                    if (resultSetting != null)
                    {
                        quizAnswerSubmitResultScore.ResultScoreValueTxt = string.Empty;
                        quizAnswerSubmitResultScore.ShowScoreValue = false;

                        if (resultSetting.ShowCorrectAnswer.HasValue && resultSetting.ShowCorrectAnswer.Value)
                        {
                            quizAnswerSubmitResultScore.AnswerKeyCustomTxt = resultSetting.CustomTxtForAnswerKey;
                            quizAnswerSubmitResultScore.YourAnswerCustomTxt = resultSetting.CustomTxtForYourAnswer;
                            quizAnswerSubmitResultScore.CorrectAnswerCustomTxt = resultSetting.CustomTxtForCorrectAnswer;
                            quizAnswerSubmitResultScore.ExplanationCustomTxt = resultSetting.CustomTxtForExplanation;
                            quizAnswerSubmitResultScore.ShowCorrectAnswer = true;
                            quizAnswerSubmitResultScore.AttemptedQuestionAnswerDetails = new List<QuizAnswerSubmit.AnswerResult>();

                            foreach (var item in attemptedQuestions.Where(r => r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single))
                            {
                                string yurAnswer = notAttemptedQuesText;
                                if (item.QuizAnswerStats != null && item.QuizAnswerStats.Any(r => !r.AnswerOptionsInQuizQuestions.IsUnansweredType))
                                {
                                    yurAnswer = VariableLinking(string.Join(",", item.QuizAnswerStats.Where(t => !t.AnswerOptionsInQuizQuestions.IsUnansweredType).Select(t => t.AnswerOptionsInQuizQuestions.Option).Select(s => "'" + s + "'")), false, quizAnswerSubmitResultScore.ShowScoreValue, string.Empty, publishQuizTmpModel);
                                }



                                quizAnswerSubmitResultScore.AttemptedQuestionAnswerDetails.Add(new QuizAnswerSubmit.AnswerResult
                                {
                                    Question = VariableLinking(item.QuestionsInQuiz.Question, false, quizAnswerSubmitResultScore.ShowScoreValue, string.Empty, publishQuizTmpModel),
                                    YourAnswer = yurAnswer,
                                    CorrectAnswer = string.Empty,
                                    IsCorrect = null,
                                    AnswerExplanation = item.QuestionsInQuiz.CorrectAnswerDescription,
                                    AssociatedScore = default(int)
                                });
                            }
                        }
                        else
                        {
                            quizAnswerSubmitResultScore.ShowCorrectAnswer = false;
                        }
                    }

                    quizAnswerSubmitResultScore.ShowInternalTitle = result.ShowInternalTitle;
                    quizAnswerSubmitResultScore.ShowExternalTitle = result.ShowExternalTitle;
                    quizAnswerSubmitResultScore.ShowDescription = result.ShowDescription;
                    var msgVariables = publishQuizTmpModel.QuizVariables?.Where(v => v.ObjectTypes == (int)QuizVariableObjectTypes.RESULT && v.ObjectId == result.Id).FirstOrDefault()?.Variables;
                    CommonStaticData.VacancyVariableReplace(publishQuizTmpModel.ContactObject, msgVariables, publishQuizTmpModel.CompanyCode);
                    quizAnswerSubmitResultScore.Title = VariableLinking(result.Title, false, quizAnswerSubmitResultScore.ShowScoreValue, string.Empty, publishQuizTmpModel, msgVariables);
                    quizAnswerSubmitResultScore.InternalTitle = VariableLinking(result.InternalTitle, false, quizAnswerSubmitResultScore.ShowScoreValue, string.Empty, publishQuizTmpModel, msgVariables);
                    quizAnswerSubmitResultScore.Description = VariableLinking(result.Description, false, quizAnswerSubmitResultScore.ShowScoreValue, string.Empty, publishQuizTmpModel, msgVariables);
                    quizAnswerSubmitResultScore.AutoPlay = result.AutoPlay;
                    quizAnswerSubmitResultScore.SecondsToApply = result.SecondsToApply;
                    quizAnswerSubmitResultScore.VideoFrameEnabled = result.VideoFrameEnabled;
                    quizAnswerSubmitResultScore.DisplayOrderForTitle = result.DisplayOrderForTitle;
                    quizAnswerSubmitResultScore.DisplayOrderForTitleImage = result.DisplayOrderForTitleImage;
                    quizAnswerSubmitResultScore.DisplayOrderForDescription = result.DisplayOrderForDescription;
                    quizAnswerSubmitResultScore.DisplayOrderForNextButton = result.DisplayOrderForNextButton;
                    quizAnswerSubmit.ResultScore = quizAnswerSubmitResultScore;
                    if (quizStatsObj != null)
                    {
                        quizStatsObj.ResultId = result.Id;
                        UOWObj.QuizStatsRepository.Update(quizStatsObj);
                        UOWObj.Save();
                    }
                    #endregion
                }
            }
            #endregion

            return quizAnswerSubmit;
        }


        private QuizAnswerSubmit CompleteQuestionLastResultBranchingPersonality(PublishQuizTmpModel publishQuizTmpModel, QuizAnswerSubmit quizAnswerSubmit)
        {
            Db.ResultSettings resultSetting = null;
            var IsResultInquizBranching = false;
            List<Db.QuizResults> quizresultList = null;
            var quizAnswerSubmitResultScore = new QuizAnswerSubmit.Result();
            List<Db.MediaVariablesDetails> mediaObjectList = null;

            #region Personality result Setting
            Db.PersonalityResultSetting personalitySetting = null;
            Db.QuizStats quizStatsObj = null;
            List<Db.PersonalityAnswerResultMapping> personalityAnswerResultList = null;
            quizAnswerSubmit.ResultScore = new QuizAnswerSubmit.Result();

            using (var UOWObj = new AutomationUnitOfWork())
            {

                //resultSetting = UOWObj.ResultSettingsRepository.Get(r => r.QuizId == publishQuizTmpModel.QuizDetailId).FirstOrDefault();
                resultSetting = UOWObj.ResultSettingsRepository.GetResultSettingsRepositoryExtension(publishQuizTmpModel.QuizDetailId).FirstOrDefault();
                //quizresultList = UOWObj.QuizResultsRepository.Get(r => r.QuizId == publishQuizTmpModel.QuizDetailId).ToList();
                quizresultList = UOWObj.QuizResultsRepository.GetQuizResultsRepositoryExtension(publishQuizTmpModel.QuizDetailId).ToList();
                //quizStatsObj = UOWObj.QuizStatsRepository.Get(v => v.QuizAttemptId == publishQuizTmpModel.QuizattemptId).FirstOrDefault();
                quizStatsObj = UOWObj.QuizStatsRepository.GetQuizStatsQuizAttemptId(publishQuizTmpModel.QuizattemptId).FirstOrDefault();

                var attemptedQuestions = UOWObj.QuizQuestionStatsRepository.Get(t => t.QuizAttemptId == publishQuizTmpModel.QuizattemptId && t.Status == (int)StatusEnum.Active && t.CompletedOn != null && (t.QuestionsInQuiz.TimerRequired ? true : t.QuizAnswerStats.Any(r => !r.AnswerOptionsInQuizQuestions.IsUnansweredType))).ToList();
                if (attemptedQuestions != null && attemptedQuestions.Any())
                {
                    personalityAnswerResultList = attemptedQuestions.SelectMany(x => x.QuizAnswerStats.SelectMany(r => r.AnswerOptionsInQuizQuestions.PersonalityAnswerResultMapping)).ToList();

                }

                //personalitySetting = UOWObj.PersonalityResultSettingRepository.Get(v => v.QuizId == publishQuizTmpModel.QuizDetailId).FirstOrDefault();
                personalitySetting = UOWObj.PersonalityResultSettingRepository.GetPersonalityResultSettingByQuizId(publishQuizTmpModel.QuizDetailId).FirstOrDefault();

                if (personalitySetting != null && personalitySetting.Status == (int)StatusEnum.Active)
                {
                    #region personality Setting Active
                    var personalityResultList = new List<QuizAnswerSubmit.PersonalityResult>();
                    var countResult = 0;


                    if (personalityAnswerResultList != null && personalityAnswerResultList.Any())
                    {
                        #region correlation is available
                        var mappedResultList = personalityAnswerResultList.OrderBy(x => x.QuizResults.DisplayOrder).GroupBy(r => r.ResultId).OrderByDescending(i => i.Count()).Take(personalitySetting.MaxResult).Select(x => x.Key).ToList();
                        var quizresultFilterList = quizresultList.Where(r => !r.IsPersonalityCorrelatedResult && mappedResultList.Contains(r.Id)).OrderBy(x => mappedResultList.IndexOf(x.Id));
                        foreach (var quizresult in quizresultFilterList)
                        {
                            QuizAnswerSubmit.PersonalityResult personalityresult = new QuizAnswerSubmit.PersonalityResult();
                            personalityresult.Title = VariableLinking(quizresult.Title, false, false, null, publishQuizTmpModel);
                            personalityresult.InternalTitle = VariableLinking(quizresult.InternalTitle, false, false, null, publishQuizTmpModel);


                            personalityresult.Image = quizresult.Image;
                            personalityresult.ResultId = quizresult.Id;
                            personalityresult.GraphColor = personalitySetting.GraphColor;
                            personalityresult.ButtonColor = personalitySetting.ButtonColor;
                            personalityresult.ButtonFontColor = personalitySetting.ButtonFontColor;
                            personalityresult.MaxResult = personalitySetting.MaxResult;
                            personalityresult.SideButtonText = personalitySetting.SideButtonText;
                            personalityresult.IsFullWidthEnable = personalitySetting.IsFullWidthEnable;

                            personalityresult.Description = VariableLinking(quizresult.Description, false, false, null, publishQuizTmpModel);

                            int count = personalityAnswerResultList.Where(k => k.ResultId == quizresult.Id).Count();
                            personalityresult.Percentage = (int)Math.Round((double)(100 * count) / personalityAnswerResultList.Count());
                            personalityresult.ShowInternalTitle = quizresult.ShowInternalTitle;
                            personalityresult.ShowExternalTitle = quizresult.ShowExternalTitle;
                            personalityresult.ShowDescription = quizresult.ShowDescription;
                            personalityResultList.Add(personalityresult);

                            if (quizStatsObj != null)
                            {

                                if (countResult == 0)
                                {
                                    quizStatsObj.ResultId = quizresult.Id;
                                    UOWObj.QuizStatsRepository.Update(quizStatsObj);
                                }
                                else
                                {
                                    InsertQuizStat(quizStatsObj.QuizAttemptId, quizresult.Id, quizStatsObj.StartedOn);
                                }
                                UOWObj.Save();

                                countResult++;
                            }
                        }

                        if (publishQuizTmpModel.LeadUserId == null && publishQuizTmpModel.RecruiterUserId == 0)
                        {
                            int? formId;
                            int? flowOrder;
                            quizAnswerSubmit.ShowLeadUserForm = CheckleadForm2(publishQuizTmpModel, personalityResultList, quizresultList, personalitySetting, out formId, out flowOrder);
                            quizAnswerSubmit.FormId = formId;
                            quizAnswerSubmit.FlowOrder = flowOrder;
                        }

                        #endregion
                    }
                    else
                    {
                        #region correlation is not available
                        var quizresultListfiltered = quizresultList.OrderBy(x => x.DisplayOrder).Take(personalitySetting.MaxResult).ToList();
                        foreach (var quizresult in quizresultListfiltered)
                        {
                            QuizAnswerSubmit.PersonalityResult personalityresult = new QuizAnswerSubmit.PersonalityResult();
                            personalityresult.Title = VariableLinking(quizresult.Title, false, false, null, publishQuizTmpModel);
                            personalityresult.InternalTitle = VariableLinking(quizresult.InternalTitle, false, false, null, publishQuizTmpModel); ;
                            personalityresult.Description = VariableLinking(quizresult.Description, false, false, null, publishQuizTmpModel);

                            personalityresult.Image = quizresult.Image;
                            personalityresult.ResultId = quizresult.Id;
                            personalityresult.GraphColor = personalitySetting.GraphColor;
                            personalityresult.ButtonColor = personalitySetting.ButtonColor;
                            personalityresult.ButtonFontColor = personalitySetting.ButtonFontColor;
                            personalityresult.MaxResult = personalitySetting.MaxResult;
                            personalityresult.SideButtonText = personalitySetting.SideButtonText;
                            personalityresult.IsFullWidthEnable = personalitySetting.IsFullWidthEnable;

                            personalityresult.Percentage = null;
                            personalityresult.ShowInternalTitle = quizresult.ShowInternalTitle;
                            personalityresult.ShowExternalTitle = quizresult.ShowExternalTitle;
                            personalityresult.ShowDescription = quizresult.ShowDescription;
                            personalityResultList.Add(personalityresult);

                            if (quizStatsObj != null)
                            {

                                if (countResult == 0)
                                {
                                    quizStatsObj.ResultId = quizresult.Id;
                                    UOWObj.QuizStatsRepository.Update(quizStatsObj);
                                }
                                else
                                {
                                    InsertQuizStat(quizStatsObj.QuizAttemptId, quizresult.Id, quizStatsObj.StartedOn);
                                }
                                UOWObj.Save();

                                countResult++;
                            }


                        }

                        if (publishQuizTmpModel.LeadUserId == null && publishQuizTmpModel.RecruiterUserId == 0)
                        {
                            int? formId;
                            int? flowOrder;
                            quizAnswerSubmit.ShowLeadUserForm = CheckleadForm2(publishQuizTmpModel, personalityResultList, quizresultListfiltered, personalitySetting, out formId, out flowOrder);
                            quizAnswerSubmit.FormId = formId;
                            quizAnswerSubmit.FlowOrder = flowOrder;
                        }

                        #endregion
                    }
                    quizAnswerSubmitResultScore.ShowInternalTitle = true;
                    quizAnswerSubmitResultScore.ShowExternalTitle = true;
                    quizAnswerSubmitResultScore.ShowDescription = true;
                    quizAnswerSubmitResultScore.Title = personalitySetting.Title;
                    quizAnswerSubmitResultScore.PersonalityResultList = personalityResultList.OrderByDescending(x => x.Percentage).ToList();
                    #endregion
                }
                else
                {
                    #region If there is correlation available but personalitySetting is disabled

                    var result = new Db.QuizResults();
                    if (personalityAnswerResultList != null && personalityAnswerResultList.Any())
                        result = quizresultList.FirstOrDefault(x => x.Id == personalityAnswerResultList.OrderBy(r => r.QuizResults.DisplayOrder).GroupBy(r => r.ResultId).OrderByDescending(i => i.Count()).First().Key);
                    else
                        result = quizresultList.FirstOrDefault(x => x.DisplayOrder == 1);

                    if (publishQuizTmpModel.LeadUserId == null && publishQuizTmpModel.RecruiterUserId == 0)
                    {
                        int? formId;
                        int? flowOrder;
                        quizAnswerSubmit.ShowLeadUserForm = CheckleadFormQuizResult(publishQuizTmpModel, result, quizresultList, personalitySetting, out formId, out flowOrder);
                        quizAnswerSubmit.FormId = formId;
                        quizAnswerSubmit.FlowOrder = flowOrder;
                    }


                    quizAnswerSubmitResultScore.ShowInternalTitle = result.ShowInternalTitle;
                    quizAnswerSubmitResultScore.ShowExternalTitle = result.ShowExternalTitle;
                    quizAnswerSubmitResultScore.ShowDescription = result.ShowDescription;
                    var msgVariables = publishQuizTmpModel.QuizVariables?.Where(v => v.ObjectTypes == (int)QuizVariableObjectTypes.RESULT && v.ObjectId == result.Id).FirstOrDefault()?.Variables;
                    CommonStaticData.VacancyVariableReplace(publishQuizTmpModel.ContactObject, msgVariables, publishQuizTmpModel.CompanyCode);
                    quizAnswerSubmitResultScore.Title = VariableLinking(result.Title, false, (resultSetting.ShowScoreValue.HasValue && resultSetting.ShowScoreValue.Value), string.Empty, publishQuizTmpModel, msgVariables);
                    quizAnswerSubmitResultScore.InternalTitle = VariableLinking(result.InternalTitle, false, (resultSetting.ShowScoreValue.HasValue && resultSetting.ShowScoreValue.Value), string.Empty, publishQuizTmpModel, msgVariables);


                    quizAnswerSubmitResultScore.Image = result.ShowResultImage ? result.Image : string.Empty;
                    quizAnswerSubmitResultScore.PublicIdForResult = result.ShowResultImage ? result.PublicId : string.Empty;

                    if (result.EnableMediaFile)
                    {
                        var mediaList = publishQuizTmpModel.MediaVariableList.Where(r => r.ObjectTypeId == (int)BranchingLogicEnum.RESULT && r.ObjectId == result.Id);
                        //var mediaList = UOWObj.MediaVariablesDetailsRepository.Get(r => r.QuizId == publishQuizTmpModel.QuizDetailId && r.ConfigurationDetailsId == publishQuizTmpModel.ConfigurationDetailId && r.ObjectTypeId == (int)BranchingLogicEnum.RESULT && r.ObjectId == result.Id);
                        if (result.EnableMediaFile && mediaList != null)
                        {
                            var mediaObj = mediaList.FirstOrDefault();
                            if (mediaList != null)
                            {
                                quizAnswerSubmitResultScore.Image = result.ShowResultImage ? mediaObj.ObjectValue : string.Empty;
                                quizAnswerSubmitResultScore.PublicIdForResult = result.ShowResultImage ? mediaObj.ObjectPublicId : string.Empty;

                                var newMedia = result.ShowResultImage ? ExtractDynamicMedia(mediaObj, publishQuizTmpModel.LeadDetails, publishQuizTmpModel.UserMediaClassifications, publishQuizTmpModel.LeadOwners) : null;
                                if (newMedia != null)
                                {
                                    quizAnswerSubmitResultScore.Image = newMedia.MediaUrl;
                                    quizAnswerSubmitResultScore.PublicIdForResult = newMedia.MediaPublicId;
                                }
                            }
                        }
                    }


                    quizAnswerSubmitResultScore.Description = VariableLinking(result.Description, false, (resultSetting.ShowScoreValue.HasValue && resultSetting.ShowScoreValue.Value), string.Empty, publishQuizTmpModel, msgVariables);
                    quizAnswerSubmitResultScore.HideActionButton = result.HideCallToAction.HasValue ? result.HideCallToAction.Value : false;
                    quizAnswerSubmitResultScore.ActionButtonURL = quizAnswerSubmitResultScore.HideActionButton ? string.Empty : result.ActionButtonURL;
                    quizAnswerSubmitResultScore.OpenLinkInNewTab = quizAnswerSubmitResultScore.HideActionButton ? false : result.OpenLinkInNewTab;
                    quizAnswerSubmitResultScore.ActionButtonTxtSize = quizAnswerSubmitResultScore.HideActionButton ? string.Empty : result.ActionButtonTxtSize;
                    quizAnswerSubmitResultScore.ActionButtonColor = quizAnswerSubmitResultScore.HideActionButton ? string.Empty : result.ActionButtonColor;
                    quizAnswerSubmitResultScore.ActionButtonTitleColor = quizAnswerSubmitResultScore.HideActionButton ? string.Empty : result.ActionButtonTitleColor;
                    quizAnswerSubmitResultScore.ActionButtonText = quizAnswerSubmitResultScore.HideActionButton ? string.Empty : result.ActionButtonText;
                    quizAnswerSubmitResultScore.AutoPlay = result.AutoPlay;
                    quizAnswerSubmitResultScore.SecondsToApply = result.SecondsToApply ?? "0";
                    quizAnswerSubmitResultScore.VideoFrameEnabled = result.VideoFrameEnabled ?? false;
                    quizAnswerSubmitResultScore.DisplayOrderForTitle = result.DisplayOrderForTitle;
                    quizAnswerSubmitResultScore.DisplayOrderForTitleImage = result.DisplayOrderForTitleImage;
                    quizAnswerSubmitResultScore.DisplayOrderForDescription = result.DisplayOrderForDescription;
                    quizAnswerSubmitResultScore.DisplayOrderForNextButton = result.DisplayOrderForNextButton;



                    if (quizStatsObj != null)
                    {
                        quizStatsObj.ResultId = result.Id;
                        UOWObj.QuizStatsRepository.Update(quizStatsObj);
                        UOWObj.Save();
                    }


                    quizAnswerSubmitResultScore.ResultScoreValueTxt = string.Empty;
                    quizAnswerSubmitResultScore.ShowScoreValue = false;

                    if (resultSetting.ShowCorrectAnswer.HasValue && resultSetting.ShowCorrectAnswer.Value)
                    {
                        quizAnswerSubmitResultScore.AnswerKeyCustomTxt = resultSetting.CustomTxtForAnswerKey;
                        quizAnswerSubmitResultScore.YourAnswerCustomTxt = resultSetting.CustomTxtForYourAnswer;
                        quizAnswerSubmitResultScore.CorrectAnswerCustomTxt = resultSetting.CustomTxtForCorrectAnswer;
                        quizAnswerSubmitResultScore.ExplanationCustomTxt = resultSetting.CustomTxtForExplanation;
                        quizAnswerSubmitResultScore.ShowCorrectAnswer = true;
                        quizAnswerSubmitResultScore.AttemptedQuestionAnswerDetails = new List<QuizAnswerSubmit.AnswerResult>();

                        #region attempted question

                        foreach (var item in attemptedQuestions.Where(r => r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single))
                        {
                            var question = VariableLinking(item.QuestionsInQuiz.Question, false, quizAnswerSubmitResultScore.ShowScoreValue, string.Empty, publishQuizTmpModel);
                            var yourAnswer = notAttemptedQuesText;
                            if (item.QuizAnswerStats != null && item.QuizAnswerStats.Any(r => !r.AnswerOptionsInQuizQuestions.IsUnansweredType))
                            {
                                var answertitle = string.Join(",", item.QuizAnswerStats.Where(t => !t.AnswerOptionsInQuizQuestions.IsUnansweredType).Select(t => t.AnswerOptionsInQuizQuestions.Option).Select(s => "'" + s + "'"));
                                yourAnswer = VariableLinking(answertitle, false, quizAnswerSubmitResultScore.ShowScoreValue, string.Empty, publishQuizTmpModel);
                            }

                            quizAnswerSubmitResultScore.AttemptedQuestionAnswerDetails.Add(new QuizAnswerSubmit.AnswerResult
                            {
                                YourAnswer = yourAnswer,
                                Question = question,
                                CorrectAnswer = string.Empty,
                                IsCorrect = null,
                                AnswerExplanation = item.QuestionsInQuiz.CorrectAnswerDescription,
                                AssociatedScore = default(int)
                            });
                        }

                        #endregion
                    }
                    else
                    {

                        quizAnswerSubmitResultScore.ShowCorrectAnswer = false;
                    }
                    #endregion

                }
            }
            #endregion
            quizAnswerSubmit.ResultScore = quizAnswerSubmitResultScore;
            return quizAnswerSubmit;
        }

        public static void FetchRecruiterVariables(PublishQuizTmpModel publishQuizTmpModel) {
            TempWorkpackagePush temp = new TempWorkpackagePush();
            temp.leadUserInfo = new TempWorkpackagePush.ContactBasicDetails();
            temp.leadUserInfo.contactId = publishQuizTmpModel.LeadUserId;
            temp.leadUserInfo.LeadOwnerId = publishQuizTmpModel.LeadDetails.LeadOwnerId;
            temp.leadUserInfo.SourceOwnerId = publishQuizTmpModel.LeadDetails.SourceOwnerId;
            temp.leadUserInfo.ContactOwnerId = publishQuizTmpModel.LeadDetails.ContactOwnerId;
            temp.companyObj = publishQuizTmpModel.CompanyDetails;

            temp.RecruiterList = WorkpackageCommunicationService.GetOwnerList(temp);
            if(temp.RecruiterList != null && temp.RecruiterList.Any()) {
                foreach (var item in temp.RecruiterList) {
                    publishQuizTmpModel.ContactObject.Add($@"{item.ObjectUserOwnerType}.firstname", String.IsNullOrEmpty(item.firstName) ? string.Empty : item.firstName);
                    publishQuizTmpModel.ContactObject.Add($@"{item.ObjectUserOwnerType}.lastName", String.IsNullOrEmpty(item.lastName) ? string.Empty : item.lastName);
                    publishQuizTmpModel.ContactObject.Add($@"{item.ObjectUserOwnerType}.phoneNumber", String.IsNullOrEmpty(item.phoneNumber) ? string.Empty : item.phoneNumber);
                    publishQuizTmpModel.ContactObject.Add($@"{item.ObjectUserOwnerType}.phone", String.IsNullOrEmpty(item.phoneNumber) ? string.Empty : item.phoneNumber);
                    publishQuizTmpModel.ContactObject.Add($@"{item.ObjectUserOwnerType}.email", String.IsNullOrEmpty(item.Mail) ? string.Empty : item.Mail);
                    if (item.ObjectUserOwnerType.EqualsCI("SourceOwner")) {
                        publishQuizTmpModel.ContactObject.Add("rfname", (item != null && !String.IsNullOrEmpty(item.firstName)) ? item.firstName : string.Empty);
                        publishQuizTmpModel.ContactObject.Add("rphone", (item != null && !String.IsNullOrEmpty(item.phoneNumber)) ? item.phoneNumber : string.Empty);
                        publishQuizTmpModel.ContactObject.Add("recruiter", (item != null && !String.IsNullOrEmpty(item.firstName)) ? item.firstName : string.Empty);

                    }
                }

            }
            
        }

    }
}
