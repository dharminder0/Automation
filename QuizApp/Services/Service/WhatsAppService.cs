using Core.Common.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuizApp.Helpers;
using QuizApp.RepositoryPattern;
using QuizApp.Response;
using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using static QuizApp.Response.QuizTemplateResponse;

namespace QuizApp.Services.Service {
    public class WhatsAppService : IWhatsAppService {
        IQuizService _quizService = null;
        public WhatsAppService(IQuizService quizService) {
            _quizService = quizService;
        }

        private readonly OWCHelper _owchelper = new OWCHelper();
        private ResultEnum status = ResultEnum.Ok;
        private string errormessage = string.Empty;
        public ResultEnum Status { get { return status; } set { status = value; } }
        public string ErrorMessage { get { return errormessage; } set { errormessage = value; } }
        public object GetWhatsAppHSMTemplates(string clientCode, string templatesType, bool replaceParameters = true, string language = null) {
            var list = _owchelper.GetWhatsAppHSMTemplates(clientCode, templatesType, replaceParameters, language);
            return list;
        }
        public object WhatsAppTemplatesLanguages(string clientCode, string type = null, bool replaceParameters = true, string language = null) {
            var list = _owchelper.WhatsAppTemplatesLanguages(clientCode);
            return list;
        }
        public QuizQuestion AddQuizWhatsAppTemplateOld(int QuizId, int templateId, string language, int BusinessUserId, int CompanyId, string clientCode, int Type) {
            var whatsApptemplateDetails = OWCHelper.GetWhatsAppHSMTemplate(clientCode, null, false, language, null);
            if (whatsApptemplateDetails == null) {
                return null;
            }
            var whatsAppTemplate = JsonConvert.DeserializeObject<List<WhatsAppTemplateDto>>(whatsApptemplateDetails.ToString()).Where(v => v.Id == templateId && v.TemplateLanguage.Equals(language, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (whatsAppTemplate == null) {
                return null;
            }

            int existingtemplateId = 0;
            int existingTemplateQuestionId = 0;
            QuizQuestion quizQuestionObj = null;
            try {
                using (var UOWObj = new AutomationUnitOfWork()) {
                    using (var transaction = Utility.CreateTransactionScope()) {
                        var quizObj = UOWObj.QuizRepository.GetByID(QuizId);
                        var languages = UOWObj.LanguagesRepository.Get(r => r.Culture.Equals(whatsAppTemplate.TemplateLanguage, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();


                        if (quizObj != null && quizObj.QuizDetails.Any()) {
                            var quizDetails = quizObj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED);

                            if (quizDetails != null) {
                                #region Adding obj

                                var currentDate = DateTime.UtcNow;

                                var existingtemplateQuestionlist = quizDetails.QuestionsInQuiz.Where(v => v.TemplateId.HasValue);
                                if (existingtemplateQuestionlist != null && existingtemplateQuestionlist.Any()) {
                                    foreach (var item in existingtemplateQuestionlist.ToList()) {
                                        existingtemplateId = item.TemplateId.Value;
                                        existingTemplateQuestionId = item.Id;

                                        UOWObj.QuestionsInQuizRepository.Delete(item);
                                    }
                                }


                                var obj = new Db.QuestionsInQuiz();

                                obj.QuizId = quizDetails.Id;
                                obj.ShowAnswerImage = false;
                                obj.Question = whatsAppTemplate.TemplateName;
                                obj.TemplateId = whatsAppTemplate.Id;
                                obj.Description = "Message";
                                obj.LanguageCode = language;

                                obj.ShowTitle = quizObj.UsageTypeInQuiz.Any(a => a.UsageType == (int)UsageTypeEnum.WhatsAppChatbot) ? false : true;
                                obj.ShowQuestionImage = true;
                                obj.QuestionImage = string.Empty;
                                obj.PublicId = string.Empty;
                                obj.Status = (int)StatusEnum.Active;
                                obj.State = (int)QuizStateEnum.DRAFTED;
                                obj.AnswerType = quizObj.QuizType == (int)QuizTypeEnum.NPS ? (int)AnswerTypeEnum.NPS : (int)AnswerTypeEnum.Single;
                                obj.NextButtonText = "Next";
                                obj.NextButtonTxtSize = "24px";
                                obj.ViewPreviousQuestion = quizDetails.ViewPreviousQuestion;
                                obj.EditAnswer = quizDetails.EditAnswer;
                                obj.AutoPlay = true;
                                obj.SecondsToApply = "0";
                                obj.VideoFrameEnabled = false;

                                obj.DescriptionImage = string.Empty;
                                obj.EnableMediaFileForDescription = false;
                                obj.PublicIdForDescription = string.Empty;
                                obj.ShowDescriptionImage = false;
                                obj.AutoPlayForDescription = true;
                                obj.SecondsToApplyForDescription = "0";
                                obj.DescVideoFrameEnabled = false;
                                obj.Type = Type;
                                obj.DisplayOrderForTitleImage = 2;
                                obj.DisplayOrderForTitle = 1;
                                obj.DisplayOrderForDescriptionImage = 4;
                                obj.DisplayOrderForDescription = 3;
                                obj.DisplayOrderForAnswer = 5;
                                obj.DisplayOrderForNextButton = 6;
                                obj.EnableNextButton = true;
                                obj.ShowDescription = true;
                                obj.EnableComment = false;
                                obj.TopicTitle = string.Empty;
                                obj.CorrectAnswerDescription = string.Empty;
                                obj.AnswerStructureType = (int)AnswerStructureTypeEnum.Button;
                                obj.DisplayOrder = 1;
                                obj.LastUpdatedBy = BusinessUserId;
                                obj.LastUpdatedOn = currentDate;

                                UOWObj.QuestionsInQuizRepository.Insert(obj);
                                UOWObj.Save();

                                if (whatsAppTemplate.CustomComponents != null && whatsAppTemplate.CustomComponents.Any()) {
                                    foreach (var cstomComponents in whatsAppTemplate.CustomComponents) {
                                        if (!string.IsNullOrWhiteSpace(cstomComponents.Type) && cstomComponents.Type.EqualsCI("buttons") && (cstomComponents.Items != null && cstomComponents.Items.Any())) {
                                            foreach (var item in cstomComponents.Items) {
                                                var answerObj = new Db.AnswerOptionsInQuizQuestions();

                                                answerObj.QuestionId = obj.Id;
                                                answerObj.QuizId = obj.QuizId;
                                                answerObj.Option = item.Text;
                                                answerObj.RefId = item.Id;
                                                answerObj.OptionImage = string.Empty;
                                                answerObj.PublicId = string.Empty;
                                                answerObj.LastUpdatedBy = BusinessUserId;
                                                answerObj.LastUpdatedOn = currentDate;

                                                if (quizObj.QuizType == (int)QuizTypeEnum.Score || quizObj.QuizType == (int)QuizTypeEnum.ScoreTemplate) {
                                                    answerObj.AssociatedScore = default(int);
                                                    answerObj.IsCorrectAnswer = false;
                                                } else if (quizObj.QuizType == (int)QuizTypeEnum.Personality || quizObj.QuizType == (int)QuizTypeEnum.PersonalityTemplate)
                                                    answerObj.IsCorrectAnswer = false;
                                                else if (quizObj.QuizType == (int)QuizTypeEnum.NPS) {
                                                    answerObj.IsCorrectAnswer = null;
                                                    answerObj.IsCorrectForMultipleAnswer = null;
                                                } else
                                                    answerObj.IsCorrectAnswer = true;

                                                answerObj.DisplayOrder = 1;
                                                answerObj.Status = (int)StatusEnum.Active;
                                                answerObj.State = (int)QuizStateEnum.DRAFTED;
                                                answerObj.AutoPlay = true;
                                                answerObj.SecondsToApply = "0";
                                                answerObj.VideoFrameEnabled = false;

                                                UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);

                                                if (item.MappedFields != null && item.MappedFields.Any()) {
                                                    List<ObjectFieldsDetails> listObjectFields = new List<ObjectFieldsDetails>();
                                                    foreach (var itemmapped in item.MappedFields) {
                                                        listObjectFields.Add(new ObjectFieldsDetails {
                                                            AnswerId = answerObj.Id,
                                                            ObjectName = itemmapped.ObjectName,
                                                            FieldName = itemmapped.FieldName,
                                                            Value = itemmapped.Value

                                                        });
                                                    }
                                                    _quizService.UpdateAnswerObjectFieldsDetails(listObjectFields, CompanyId, BusinessUserId);
                                                }
                                            }

                                        }
                                    }
                                }


                                quizDetails.LastUpdatedBy = BusinessUserId;
                                quizDetails.LastUpdatedOn = currentDate;

                                quizObj.State = (int)QuizStateEnum.DRAFTED;


                                if (quizObj.QuizType == (int)QuizTypeEnum.Score || quizObj.QuizType == (int)QuizTypeEnum.ScoreTemplate || quizObj.QuizType == (int)QuizTypeEnum.Personality || quizObj.QuizType == (int)QuizTypeEnum.PersonalityTemplate) {
                                    obj.MinAnswer = 1;
                                    obj.MaxAnswer = 1;
                                }
                                UOWObj.QuestionsInQuizRepository.Update(obj);
                                UOWObj.QuizRepository.Update(quizObj);

                                var BranchingLogicObj = UOWObj.BranchingLogicRepository.Get(a => a.QuizId == quizDetails.Id);
                                foreach (var BranchingLogic in BranchingLogicObj) {
                                    if (BranchingLogic.SourceTypeId == (int)BranchingLogicEnum.WHATSAPPTEMPLATE) {
                                        BranchingLogic.SourceObjectId = obj.Id;
                                        UOWObj.BranchingLogicRepository.Update(BranchingLogic);
                                    }
                                    if (BranchingLogic.SourceTypeId == (int)BranchingLogicEnum.WHATSAPPTEMPLATEACTION) {
                                        UOWObj.BranchingLogicRepository.Delete(BranchingLogic);
                                    }
                                }


                                var cordinatesInBranchingLogicObj = UOWObj.CoordinatesInBranchingLogicRepository.Get(a => a.ObjectId == existingTemplateQuestionId && a.ObjectTypeId == (int)BranchingLogicEnum.WHATSAPPTEMPLATE).FirstOrDefault();
                                if (cordinatesInBranchingLogicObj != null) {
                                    cordinatesInBranchingLogicObj.ObjectId = obj.Id;
                                    cordinatesInBranchingLogicObj.CompanyId = CompanyId;
                                    cordinatesInBranchingLogicObj.QuizId = QuizId;
                                    UOWObj.CoordinatesInBranchingLogicRepository.Update(cordinatesInBranchingLogicObj);
                                }

                                UOWObj.Save();
                                transaction.Complete();

                                #endregion

                                #region Bind return obj

                                quizQuestionObj = new QuizQuestion();

                                quizQuestionObj.QuizType = quizObj.QuizType;
                                quizQuestionObj.QuestionId = obj.Id;
                                quizQuestionObj.ShowAnswerImage = obj.ShowAnswerImage;
                                quizQuestionObj.QuestionTitle = obj.Question;
                                quizQuestionObj.QuestionImage = obj.QuestionImage;
                                quizQuestionObj.PublicIdForQuestion = obj.PublicId;
                                quizQuestionObj.ShowQuestionImage = obj.ShowQuestionImage;
                                quizQuestionObj.DisplayOrder = obj.DisplayOrder;
                                quizQuestionObj.AnswerType = obj.AnswerType;
                                quizQuestionObj.MinAnswer = obj.MinAnswer;
                                quizQuestionObj.MaxAnswer = obj.MaxAnswer;
                                quizQuestionObj.AutoPlay = obj.AutoPlay;
                                quizQuestionObj.SecondsToApply = obj.SecondsToApply ?? "0";
                                quizQuestionObj.VideoFrameEnabled = obj.VideoFrameEnabled ?? false;
                                quizQuestionObj.Description = obj.Description;
                                quizQuestionObj.ShowDescription = obj.ShowDescription;
                                quizQuestionObj.DescriptionImage = obj.DescriptionImage;
                                quizQuestionObj.EnableMediaFileForDescription = obj.EnableMediaFileForDescription;
                                quizQuestionObj.PublicIdForDescription = obj.PublicIdForDescription;
                                quizQuestionObj.ShowDescriptionImage = obj.ShowDescriptionImage ?? false;
                                quizQuestionObj.AutoPlayForDescription = obj.AutoPlayForDescription;
                                quizQuestionObj.SecondsToApplyForDescription = obj.SecondsToApplyForDescription ?? "0";
                                quizQuestionObj.DescVideoFrameEnabled = obj.DescVideoFrameEnabled ?? false;
                                quizQuestionObj.Type = obj.Type;
                                quizQuestionObj.EnableNextButton = obj.EnableNextButton;
                                quizQuestionObj.DisplayOrderForTitleImage = obj.DisplayOrderForTitleImage;
                                quizQuestionObj.DisplayOrderForTitle = obj.DisplayOrderForTitle;
                                quizQuestionObj.DisplayOrderForDescriptionImage = obj.DisplayOrderForDescriptionImage;
                                quizQuestionObj.DisplayOrderForDescription = obj.DisplayOrderForDescription;
                                quizQuestionObj.DisplayOrderForAnswer = obj.DisplayOrderForAnswer;
                                quizQuestionObj.DisplayOrderForNextButton = obj.DisplayOrderForNextButton;
                                quizQuestionObj.AnswerStructureType = obj.AnswerStructureType;
                                //quizQuestionObj.LanguageId = obj.LanguageId;
                                quizQuestionObj.LanguageCode = obj.LanguageCode;
                                quizQuestionObj.TemplateId = obj.TemplateId;
                                quizQuestionObj.LanguageCode = whatsAppTemplate.TemplateLanguage;

                                quizQuestionObj.QuizCorrectAnswerSetting = new QuizCorrectAnswerSetting();

                                quizQuestionObj.QuizCorrectAnswerSetting.CorrectAnswerExplanation = obj.CorrectAnswerDescription;
                                quizQuestionObj.QuizCorrectAnswerSetting.RevealCorrectAnswer = obj.RevealCorrectAnswer;
                                quizQuestionObj.QuizCorrectAnswerSetting.AliasTextForCorrect = obj.AliasTextForCorrect;
                                quizQuestionObj.QuizCorrectAnswerSetting.AliasTextForIncorrect = obj.AliasTextForIncorrect;
                                quizQuestionObj.QuizCorrectAnswerSetting.AliasTextForYourAnswer = obj.AliasTextForYourAnswer;
                                quizQuestionObj.QuizCorrectAnswerSetting.AliasTextForCorrectAnswer = obj.AliasTextForCorrectAnswer;
                                quizQuestionObj.QuizCorrectAnswerSetting.AliasTextForExplanation = obj.AliasTextForExplanation;
                                quizQuestionObj.QuizCorrectAnswerSetting.AliasTextForNextButton = obj.AliasTextForNextButton;

                                quizQuestionObj.AnswerList = new List<AnswerOptionInQuestion>();

                                if (obj.AnswerOptionsInQuizQuestions != null && obj.AnswerOptionsInQuizQuestions.Any()) {
                                    foreach (var answer in obj.AnswerOptionsInQuizQuestions.Where(r => !r.IsUnansweredType).OrderBy(r => r.DisplayOrder)) {
                                        quizQuestionObj.AnswerList.Add(new AnswerOptionInQuestion {
                                            AnswerId = answer.Id,
                                            AnswerText = answer.Option,
                                            AnswerImage = answer.OptionImage,
                                            AssociatedScore = answer.AssociatedScore,
                                            PublicIdForAnswer = answer.PublicId,
                                            IsCorrectAnswer = answer.IsCorrectAnswer,
                                            DisplayOrder = answer.DisplayOrder,
                                            RefId = answer.RefId
                                        });
                                    }
                                }

                                #endregion

                            }
                        } else {
                            Status = ResultEnum.Error;
                            ErrorMessage = "Quiz not found for the QuizId " + QuizId;
                        }
                    }
                }
            } catch (Exception ex) {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
            return quizQuestionObj;
        }
        public QuizQuestion AddQuizWhatsAppTemplate(int QuizId, int templateId, string language, int BusinessUserId, int CompanyId, string clientCode, int Type) {
            //var whatsApptemplateDetails = OWCHelper.GetWhatsAppHSMTemplate(clientCode, null, false, language, null);           
            var whatsApptemplateDetails = OWCHelper.WhatsAppTemplates(templateId);
            if (whatsApptemplateDetails == null) {
                return null;
            }
            var whatsAppTemplate = JsonConvert.DeserializeObject<WhatsappTemplate>(whatsApptemplateDetails.ToString());
            if (whatsAppTemplate == null) {
                return null;
            }
            string tempLang = null;
            var customComp = new List<CustomComponent>();

            foreach(var custom in whatsAppTemplate.TemplateBody.Where(v => v.LangCode.EqualsCI(language))) 
            {
                tempLang = custom.LangCode;
                if(custom.CustomComponents != null) 
                {
                    foreach (var value in custom.CustomComponents) 
                        {
                        customComp.Add(new CustomComponent {
                            Items = value.Items,
                            Type = value.Type
                        });

                    }

                }
                                        
            }

            int existingtemplateId = 0;
            int existingTemplateQuestionId = 0;
            QuizQuestion quizQuestionObj = null;
            try {
                using (var UOWObj = new AutomationUnitOfWork()) {
                    using (var transaction = Utility.CreateTransactionScope()) {
                        var quizObj = UOWObj.QuizRepository.GetByID(QuizId);
                        var languages = UOWObj.LanguagesRepository.Get(r => r.Culture.Equals(tempLang, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();


                        if (quizObj != null && quizObj.QuizDetails.Any()) {
                            var quizDetails = quizObj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED);

                            if (quizDetails != null) {
                                #region Adding obj

                                var currentDate = DateTime.UtcNow;

                                var existingtemplateQuestionlist = quizDetails.QuestionsInQuiz.Where(v => v.TemplateId.HasValue);
                                if (existingtemplateQuestionlist != null && existingtemplateQuestionlist.Any()) {
                                    foreach (var item in existingtemplateQuestionlist.ToList()) {
                                        existingtemplateId = item.TemplateId.Value;
                                        existingTemplateQuestionId = item.Id;

                                        UOWObj.QuestionsInQuizRepository.Delete(item);
                                    }
                                }


                                var obj = new Db.QuestionsInQuiz();

                                obj.QuizId = quizDetails.Id;
                                obj.ShowAnswerImage = false;
                                obj.Question = whatsAppTemplate.TemplateName;
                                obj.TemplateId = whatsAppTemplate.Id;
                                obj.Description = "Message";
                                obj.LanguageCode = language;

                                obj.ShowTitle = quizObj.UsageTypeInQuiz.Any(a => a.UsageType == (int)UsageTypeEnum.WhatsAppChatbot) ? false : true;
                                obj.ShowQuestionImage = true;
                                obj.QuestionImage = string.Empty;
                                obj.PublicId = string.Empty;
                                obj.Status = (int)StatusEnum.Active;
                                obj.State = (int)QuizStateEnum.DRAFTED;
                                obj.AnswerType = quizObj.QuizType == (int)QuizTypeEnum.NPS ? (int)AnswerTypeEnum.NPS : (int)AnswerTypeEnum.Single;
                                obj.NextButtonText = "Next";
                                obj.NextButtonTxtSize = "24px";
                                obj.ViewPreviousQuestion = quizDetails.ViewPreviousQuestion;
                                obj.EditAnswer = quizDetails.EditAnswer;
                                obj.AutoPlay = true;
                                obj.SecondsToApply = "0";
                                obj.VideoFrameEnabled = false;

                                obj.DescriptionImage = string.Empty;
                                obj.EnableMediaFileForDescription = false;
                                obj.PublicIdForDescription = string.Empty;
                                obj.ShowDescriptionImage = false;
                                obj.AutoPlayForDescription = true;
                                obj.SecondsToApplyForDescription = "0";
                                obj.DescVideoFrameEnabled = false;
                                obj.Type = Type;
                                obj.DisplayOrderForTitleImage = 2;
                                obj.DisplayOrderForTitle = 1;
                                obj.DisplayOrderForDescriptionImage = 4;
                                obj.DisplayOrderForDescription = 3;
                                obj.DisplayOrderForAnswer = 5;
                                obj.DisplayOrderForNextButton = 6;
                                obj.EnableNextButton = true;
                                obj.ShowDescription = true;
                                obj.EnableComment = false;
                                obj.TopicTitle = string.Empty;
                                obj.CorrectAnswerDescription = string.Empty;
                                obj.AnswerStructureType = (int)AnswerStructureTypeEnum.Button;
                                obj.DisplayOrder = 1;
                                obj.LastUpdatedBy = BusinessUserId;
                                obj.LastUpdatedOn = currentDate;

                                UOWObj.QuestionsInQuizRepository.Insert(obj);
                                UOWObj.Save();

                                if (customComp != null && customComp.Any()) {
                                    foreach (var cstomComponents in customComp) {
                                        if (!string.IsNullOrWhiteSpace(cstomComponents.Type) && cstomComponents.Type.EqualsCI("buttons") && (cstomComponents.Items != null && cstomComponents.Items.Any())) {
                                            foreach (var item in cstomComponents.Items) {
                                                var answerObj = new Db.AnswerOptionsInQuizQuestions();

                                                answerObj.QuestionId = obj.Id;
                                                answerObj.QuizId = obj.QuizId;
                                                answerObj.Option = item.Text;
                                                answerObj.RefId = item.Id;
                                                answerObj.OptionImage = string.Empty;
                                                answerObj.PublicId = string.Empty;
                                                answerObj.LastUpdatedBy = BusinessUserId;
                                                answerObj.LastUpdatedOn = currentDate;

                                                if (quizObj.QuizType == (int)QuizTypeEnum.Score || quizObj.QuizType == (int)QuizTypeEnum.ScoreTemplate) {
                                                    answerObj.AssociatedScore = default(int);
                                                    answerObj.IsCorrectAnswer = false;
                                                } else if (quizObj.QuizType == (int)QuizTypeEnum.Personality || quizObj.QuizType == (int)QuizTypeEnum.PersonalityTemplate)
                                                    answerObj.IsCorrectAnswer = false;
                                                else if (quizObj.QuizType == (int)QuizTypeEnum.NPS) {
                                                    answerObj.IsCorrectAnswer = null;
                                                    answerObj.IsCorrectForMultipleAnswer = null;
                                                } else
                                                    answerObj.IsCorrectAnswer = true;

                                                answerObj.DisplayOrder = 1;
                                                answerObj.Status = (int)StatusEnum.Active;
                                                answerObj.State = (int)QuizStateEnum.DRAFTED;
                                                answerObj.AutoPlay = true;
                                                answerObj.SecondsToApply = "0";
                                                answerObj.VideoFrameEnabled = false;

                                                UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);

                                                //if (item.MappedFields != null && item.MappedFields.Any()) {
                                                //    List<ObjectFieldsDetails> listObjectFields = new List<ObjectFieldsDetails>();
                                                //    foreach (var itemmapped in item.MappedFields) {
                                                //        listObjectFields.Add(new ObjectFieldsDetails {
                                                //            AnswerId = answerObj.Id,
                                                //            ObjectName = itemmapped.ObjectName,
                                                //            FieldName = itemmapped.FieldName,
                                                //            Value = itemmapped.Value

                                                //        });
                                                //    }
                                                //    _quizService.UpdateAnswerObjectFieldsDetails(listObjectFields, CompanyId, BusinessUserId);
                                                //}
                                            }

                                        }
                                    }
                                }


                                quizDetails.LastUpdatedBy = BusinessUserId;
                                quizDetails.LastUpdatedOn = currentDate;

                                quizObj.State = (int)QuizStateEnum.DRAFTED;


                                if (quizObj.QuizType == (int)QuizTypeEnum.Score || quizObj.QuizType == (int)QuizTypeEnum.ScoreTemplate || quizObj.QuizType == (int)QuizTypeEnum.Personality || quizObj.QuizType == (int)QuizTypeEnum.PersonalityTemplate) {
                                    obj.MinAnswer = 1;
                                    obj.MaxAnswer = 1;
                                }
                                UOWObj.QuestionsInQuizRepository.Update(obj);
                                UOWObj.QuizRepository.Update(quizObj);

                                var BranchingLogicObj = UOWObj.BranchingLogicRepository.Get(a => a.QuizId == quizDetails.Id);
                                foreach (var BranchingLogic in BranchingLogicObj) {
                                    if (BranchingLogic.SourceTypeId == (int)BranchingLogicEnum.WHATSAPPTEMPLATE) {
                                        BranchingLogic.SourceObjectId = obj.Id;
                                        UOWObj.BranchingLogicRepository.Update(BranchingLogic);
                                    }
                                    if (BranchingLogic.SourceTypeId == (int)BranchingLogicEnum.WHATSAPPTEMPLATEACTION) {
                                        UOWObj.BranchingLogicRepository.Delete(BranchingLogic);
                                    }
                                }


                                var cordinatesInBranchingLogicObj = UOWObj.CoordinatesInBranchingLogicRepository.Get(a => a.ObjectId == existingTemplateQuestionId && a.ObjectTypeId == (int)BranchingLogicEnum.WHATSAPPTEMPLATE).FirstOrDefault();
                                if (cordinatesInBranchingLogicObj != null) {
                                    cordinatesInBranchingLogicObj.ObjectId = obj.Id;
                                    cordinatesInBranchingLogicObj.CompanyId = CompanyId;
                                    cordinatesInBranchingLogicObj.QuizId = QuizId;
                                    UOWObj.CoordinatesInBranchingLogicRepository.Update(cordinatesInBranchingLogicObj);
                                }

                                UOWObj.Save();
                                transaction.Complete();

                                #endregion

                                #region Bind return obj

                                quizQuestionObj = new QuizQuestion();

                                quizQuestionObj.QuizType = quizObj.QuizType;
                                quizQuestionObj.QuestionId = obj.Id;
                                quizQuestionObj.ShowAnswerImage = obj.ShowAnswerImage;
                                quizQuestionObj.QuestionTitle = obj.Question;
                                quizQuestionObj.QuestionImage = obj.QuestionImage;
                                quizQuestionObj.PublicIdForQuestion = obj.PublicId;
                                quizQuestionObj.ShowQuestionImage = obj.ShowQuestionImage;
                                quizQuestionObj.DisplayOrder = obj.DisplayOrder;
                                quizQuestionObj.AnswerType = obj.AnswerType;
                                quizQuestionObj.MinAnswer = obj.MinAnswer;
                                quizQuestionObj.MaxAnswer = obj.MaxAnswer;
                                quizQuestionObj.AutoPlay = obj.AutoPlay;
                                quizQuestionObj.SecondsToApply = obj.SecondsToApply ?? "0";
                                quizQuestionObj.VideoFrameEnabled = obj.VideoFrameEnabled ?? false;
                                quizQuestionObj.Description = obj.Description;
                                quizQuestionObj.ShowDescription = obj.ShowDescription;
                                quizQuestionObj.DescriptionImage = obj.DescriptionImage;
                                quizQuestionObj.EnableMediaFileForDescription = obj.EnableMediaFileForDescription;
                                quizQuestionObj.PublicIdForDescription = obj.PublicIdForDescription;
                                quizQuestionObj.ShowDescriptionImage = obj.ShowDescriptionImage ?? false;
                                quizQuestionObj.AutoPlayForDescription = obj.AutoPlayForDescription;
                                quizQuestionObj.SecondsToApplyForDescription = obj.SecondsToApplyForDescription ?? "0";
                                quizQuestionObj.DescVideoFrameEnabled = obj.DescVideoFrameEnabled ?? false;
                                quizQuestionObj.Type = obj.Type;
                                quizQuestionObj.EnableNextButton = obj.EnableNextButton;
                                quizQuestionObj.DisplayOrderForTitleImage = obj.DisplayOrderForTitleImage;
                                quizQuestionObj.DisplayOrderForTitle = obj.DisplayOrderForTitle;
                                quizQuestionObj.DisplayOrderForDescriptionImage = obj.DisplayOrderForDescriptionImage;
                                quizQuestionObj.DisplayOrderForDescription = obj.DisplayOrderForDescription;
                                quizQuestionObj.DisplayOrderForAnswer = obj.DisplayOrderForAnswer;
                                quizQuestionObj.DisplayOrderForNextButton = obj.DisplayOrderForNextButton;
                                quizQuestionObj.AnswerStructureType = obj.AnswerStructureType;
                                //quizQuestionObj.LanguageId = obj.LanguageId;
                                quizQuestionObj.LanguageCode = obj.LanguageCode;
                                quizQuestionObj.TemplateId = obj.TemplateId;
                                quizQuestionObj.LanguageCode = tempLang;

                                quizQuestionObj.QuizCorrectAnswerSetting = new QuizCorrectAnswerSetting();

                                quizQuestionObj.QuizCorrectAnswerSetting.CorrectAnswerExplanation = obj.CorrectAnswerDescription;
                                quizQuestionObj.QuizCorrectAnswerSetting.RevealCorrectAnswer = obj.RevealCorrectAnswer;
                                quizQuestionObj.QuizCorrectAnswerSetting.AliasTextForCorrect = obj.AliasTextForCorrect;
                                quizQuestionObj.QuizCorrectAnswerSetting.AliasTextForIncorrect = obj.AliasTextForIncorrect;
                                quizQuestionObj.QuizCorrectAnswerSetting.AliasTextForYourAnswer = obj.AliasTextForYourAnswer;
                                quizQuestionObj.QuizCorrectAnswerSetting.AliasTextForCorrectAnswer = obj.AliasTextForCorrectAnswer;
                                quizQuestionObj.QuizCorrectAnswerSetting.AliasTextForExplanation = obj.AliasTextForExplanation;
                                quizQuestionObj.QuizCorrectAnswerSetting.AliasTextForNextButton = obj.AliasTextForNextButton;

                                quizQuestionObj.AnswerList = new List<AnswerOptionInQuestion>();

                                if (obj.AnswerOptionsInQuizQuestions != null && obj.AnswerOptionsInQuizQuestions.Any()) {
                                    foreach (var answer in obj.AnswerOptionsInQuizQuestions.Where(r => !r.IsUnansweredType).OrderBy(r => r.DisplayOrder)) {
                                        quizQuestionObj.AnswerList.Add(new AnswerOptionInQuestion {
                                            AnswerId = answer.Id,
                                            AnswerText = answer.Option,
                                            AnswerImage = answer.OptionImage,
                                            AssociatedScore = answer.AssociatedScore,
                                            PublicIdForAnswer = answer.PublicId,
                                            IsCorrectAnswer = answer.IsCorrectAnswer,
                                            DisplayOrder = answer.DisplayOrder,
                                            RefId = answer.RefId
                                        });
                                    }
                                }

                                #endregion

                            }
                        } else {
                            Status = ResultEnum.Error;
                            ErrorMessage = "Quiz not found for the QuizId " + QuizId;
                        }
                    }
                }
            } catch (Exception ex) {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
            return quizQuestionObj;
        }

        public IDictionary<string, object> GetCommContactDetails(string contactId, string clientCode) {
            var contactDetails = _owchelper.GetCommContactDetails(contactId, clientCode);

            if (string.IsNullOrWhiteSpace(contactDetails)) {
                return null;
            }

            var exObjtemp = (JsonConvert.DeserializeObject<Dictionary<string, object>>(contactDetails.ToString()));

            if (exObjtemp == null) {
                return null;
            }

            return exObjtemp;

        }




        public WhatsappTemplateV3.GetWhatsappTemplateV3 HSMTemplateDetails(string clientCode, string moduleType, string languageCode, int templateId) {
            try {
                WhatsappTemplateV3.GetWhatsappTemplateV3 getWhatsappTemplateV3 = new WhatsappTemplateV3.GetWhatsappTemplateV3();
                if (templateId == null || clientCode == null) { return getWhatsappTemplateV3; }
                var templateobj = OWCHelper.WhatsAppTemplates(templateId);
                if (templateobj == null) { return getWhatsappTemplateV3; }

                var templateJson = JsonConvert.DeserializeObject<WhatsappTemplateV3.GetWhatsappTemplateV3>(templateobj.ToString());

                if (templateJson != null && templateJson.ClientCode.EqualsCI(clientCode)) {
                    getWhatsappTemplateV3.TemplateBody = new List<WhatsappTemplateV3.TemplateBody>();
                    if (templateJson.TemplateBody != null && templateJson.TemplateBody.Any()) {
                        foreach (var item in templateJson.TemplateBody) {
                            if (templateJson.TemplateBody != null) {
                                if (item.LangCode.EqualsCI(languageCode)) {
                                    getWhatsappTemplateV3.TemplateBody.Add(item);
                                }
                            }
                        }
                    }

                    getWhatsappTemplateV3.Params = new List<WhatsappTemplateV3.ParamItem>();
                    if (templateJson.Params != null && templateJson.Params.Any()) {
                        foreach (var item in templateJson.Params) {
                            if (templateJson.Params != null) {
                                if (item.ModuleCode.EqualsCI(moduleType)) {
                                    getWhatsappTemplateV3.Params.Add(item);
                                }
                            }
                        }

                    }

                    getWhatsappTemplateV3.ButtonParams = new List<WhatsappTemplateV3.ButtonParam>();
                    if (templateJson.ButtonParams != null && templateJson.ButtonParams.Any()) {
                        foreach (var item in templateJson.ButtonParams) {
                            if (templateJson.ButtonParams != null) {
                                if (item.ModuleCode.EqualsCI(moduleType)) {
                                    getWhatsappTemplateV3.ButtonParams.Add(item);
                                }
                            }
                        }
                    }


                    getWhatsappTemplateV3.HeaderParams = new List<WhatsappTemplateV3.ParamItem>();
                    if (templateJson.HeaderParams != null && templateJson.HeaderParams.Any()) {
                        foreach (var item in templateJson.HeaderParams) {
                            if (templateJson.HeaderParams != null) {
                                if (item.ModuleCode.EqualsCI(moduleType)) {
                                    getWhatsappTemplateV3.HeaderParams.Add(item);
                                }
                            }
                        }
                    }


                    getWhatsappTemplateV3.TemplateTypes = new List<string>();
                    if (templateJson.TemplateTypes != null && templateJson.TemplateTypes.Any()) {
                        foreach (var item in templateJson.TemplateTypes) {
                            if (templateJson.TemplateBody != null) {
                                if (item.EqualsCI(moduleType)) {
                                    getWhatsappTemplateV3.TemplateTypes.Add(item);
                                }
                            }
                        }
                    }

                    getWhatsappTemplateV3.Id = templateJson.Id;
                    getWhatsappTemplateV3.TemplateName = templateJson.TemplateName;
                    getWhatsappTemplateV3.DisplayName = templateJson.DisplayName;
                    getWhatsappTemplateV3.TemplateNamespace = templateJson.TemplateNamespace;
                    getWhatsappTemplateV3.CategoryId = templateJson.CategoryId;
                    getWhatsappTemplateV3.CategoryName = templateJson.CategoryName;
                    getWhatsappTemplateV3.TemplateLanguage = templateJson.TemplateLanguage;
                    getWhatsappTemplateV3.ClientCode = templateJson.ClientCode;
                    getWhatsappTemplateV3.SortOrder = templateJson.SortOrder;
                    getWhatsappTemplateV3.IsActive = templateJson.IsActive;
                    getWhatsappTemplateV3.Provider = templateJson.Provider;
                    getWhatsappTemplateV3.AudienceType = templateJson.AudienceType;
                }


                return getWhatsappTemplateV3;
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }
    }
}