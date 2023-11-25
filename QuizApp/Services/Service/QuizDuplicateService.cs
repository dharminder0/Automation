using Core.Common.Caching;
using Core.Common.Extensions;
using Newtonsoft.Json;
using QuizApp.Helpers;
using QuizApp.RepositoryExtensions;
using QuizApp.RepositoryPattern;
using QuizApp.Request;
using QuizApp.Response;
using QuizApp.Services.Model;
using QuizApp.Services.Validator;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Web;


namespace QuizApp.Services.Service
{
    public partial class QuizDuplicateService : IQuizDuplicateService
    {
        private ResultEnum status = ResultEnum.Ok;
        private string errormessage = string.Empty;
        private readonly IQuizVariablesService _quizVariablesService;
        public ResultEnum Status { get { return status; } set { status = value; } }
        public string ErrorMessage { get { return errormessage; } set { errormessage = value; } }
        public QuizDuplicateService(IQuizVariablesService quizVariablesService)
        {
            _quizVariablesService = quizVariablesService;
        }

        public QuizQuestion AddQuizDuplicateQuestion(int parentquizId, int questionId, int businessUserId, int companyId)
        {
            QuizQuestion quizQuestionObj = null;
            int newQuestionId = 0;
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizDetails = UOWObj.QuizDetailsRepository.GetQuizDetailsbyParentQuizIdRepositoryExtension(parentquizId).FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED);

                    if (quizDetails != null)
                    {
                        //var currentDate = DateTime.UtcNow;

                        var contentsInQuiz = quizDetails.ContentsInQuiz.Where(a => a.Status == (int)StatusEnum.Active);
                        var questionInQuiz = quizDetails.QuestionsInQuiz.Where(a => a.Status == (int)StatusEnum.Active);
                        var contentsInQuizCount = contentsInQuiz.Count();
                        var questionInQuizCount = questionInQuiz.Count();

                        var questionobj = UOWObj.QuestionsInQuizRepository.GetByID(questionId);
                        var obj = new Db.QuestionsInQuiz();
                        using (var transaction = Utility.CreateTransactionScope())
                        {
                            #region Duplicate obj

                            obj.QuizId = questionobj.QuizId;
                            obj.ShowAnswerImage = questionobj.ShowAnswerImage;               
                            obj.Question = questionobj.Question;
                            obj.ShowTitle = questionobj.ShowTitle;
                            obj.ShowQuestionImage = questionobj.ShowQuestionImage;
                            obj.QuestionImage = questionobj.QuestionImage;
                            obj.PublicId = questionobj.PublicId;
                            obj.Status = questionobj.Status;
                            obj.State = questionobj.State;
                            obj.AnswerType = questionobj.AnswerType;
                            obj.NextButtonColor = questionobj.NextButtonColor;
                            obj.NextButtonText = questionobj.NextButtonText;
                            obj.NextButtonTxtColor = questionobj.NextButtonTxtColor;
                            obj.NextButtonTxtSize = questionobj.NextButtonTxtSize;
                            obj.EnableNextButton = questionobj.EnableNextButton;
                            obj.ViewPreviousQuestion = questionobj.ViewPreviousQuestion;
                            obj.EditAnswer = questionobj.EditAnswer;
                            obj.Time = questionobj.Time;
                            obj.TimerRequired = questionobj.TimerRequired;
                            obj.AutoPlay = questionobj.AutoPlay;
                            obj.SecondsToApply = questionobj.SecondsToApply;
                            obj.VideoFrameEnabled = questionobj.VideoFrameEnabled;
                            obj.Description = questionobj.Description;
                            obj.DescriptionImage = questionobj.DescriptionImage;
                            obj.EnableMediaFileForDescription = questionobj.EnableMediaFileForDescription;
                            obj.PublicIdForDescription = questionobj.PublicIdForDescription;
                            obj.ShowDescriptionImage = questionobj.ShowDescriptionImage;
                            obj.AutoPlayForDescription = questionobj.AutoPlayForDescription;
                            obj.SecondsToApplyForDescription = questionobj.SecondsToApplyForDescription;
                            obj.DescVideoFrameEnabled = questionobj.DescVideoFrameEnabled;
                            obj.Type = questionobj.Type;
                            obj.DisplayOrderForTitleImage = questionobj.DisplayOrderForTitleImage;
                            obj.DisplayOrderForTitle = questionobj.DisplayOrderForTitle;
                            obj.DisplayOrderForDescriptionImage = questionobj.DisplayOrderForDescriptionImage;
                            obj.DisplayOrderForDescription = questionobj.DisplayOrderForDescription;
                            obj.DisplayOrderForAnswer = questionobj.DisplayOrderForAnswer;
                            obj.DisplayOrderForNextButton = questionobj.DisplayOrderForNextButton;
                            obj.EnableNextButton = questionobj.EnableNextButton;
                            obj.ShowDescription = questionobj.ShowDescription;
                            obj.EnableComment = questionobj.EnableComment;
                            obj.TopicTitle = questionobj.TopicTitle;
                            obj.CorrectAnswerDescription = questionobj.CorrectAnswerDescription;
                            obj.RevealCorrectAnswer = questionobj.RevealCorrectAnswer;
                            obj.AliasTextForCorrect = questionobj.AliasTextForCorrect;
                            obj.AliasTextForIncorrect = questionobj.AliasTextForIncorrect;
                            obj.AliasTextForYourAnswer = questionobj.AliasTextForYourAnswer;
                            obj.AliasTextForCorrectAnswer = questionobj.AliasTextForCorrectAnswer;
                            obj.AliasTextForExplanation = questionobj.AliasTextForExplanation;
                            obj.AliasTextForNextButton = questionobj.AliasTextForNextButton;
                            obj.AnswerStructureType = questionobj.AnswerStructureType;
                            obj.DisplayOrder = questionobj.DisplayOrder;
                            obj.LastUpdatedBy = questionobj.LastUpdatedBy;
                            obj.LastUpdatedOn = questionobj.LastUpdatedOn;
                            obj.MinAnswer = questionobj.MinAnswer;
                            obj.MaxAnswer = questionobj.MaxAnswer;
                            obj.EnableMediaFile = questionobj.EnableMediaFile;
                            obj.IsMultiRating = questionobj.IsMultiRating;

                            if ((questionInQuizCount + contentsInQuizCount) == 0)
                                obj.DisplayOrder = 1;
                            else if (questionInQuizCount != 0 && contentsInQuizCount != 0)
                                obj.DisplayOrder = (questionInQuiz.Max(r => r.DisplayOrder) > contentsInQuiz.Max(r => r.DisplayOrder) ? questionInQuiz.Max(r => r.DisplayOrder) + 1 : contentsInQuiz.Max(r => r.DisplayOrder) + 1);
                            else if (questionInQuizCount != 0)
                                obj.DisplayOrder = (questionInQuiz.Max(r => r.DisplayOrder) + 1);
                            else if (contentsInQuizCount != 0)
                                obj.DisplayOrder = (contentsInQuiz.Max(r => r.DisplayOrder) + 1);

                            UOWObj.QuestionsInQuizRepository.Insert(obj);
                            UOWObj.Save();

                            #region Insert into QuizVariables

                            if (questionobj != null)
                            {
                                var oldQuizVar = _quizVariablesService.GetQuizVariables(quizDetails.Id, questionobj.Id, (int)QuizVariableObjectTypes.QUESTION);                                
                                
                                QuizVariableModel quizVariable = new QuizVariableModel();
                                if (oldQuizVar != null && oldQuizVar.Any())
                                {
                                    quizVariable.Variables = String.Join(",", oldQuizVar);
                                    quizVariable.QuizDetailsId = questionobj.QuizId;
                                    quizVariable.ObjectId = obj.Id;
                                    quizVariable.ObjectTypes = (int)QuizVariableObjectTypes.QUESTION;

                                    _quizVariablesService.AddQuizVariables(quizVariable);
                                }
                            }

                            #endregion

                            SaveDynamicVariable(string.Empty, questionobj.Question, quizDetails.Id);
                            SaveDynamicVariable(string.Empty, questionobj.Description, quizDetails.Id);

                            var answerobjs = questionobj.AnswerOptionsInQuizQuestions;
                            foreach (var item in answerobjs)
                            {
                                var answerObj = new Db.AnswerOptionsInQuizQuestions();

                                answerObj.QuestionId = obj.Id;
                                answerObj.QuizId = obj.QuizId;
                                answerObj.Option = item.Option;
                                answerObj.OptionImage = item.OptionImage;
                                answerObj.PublicId = item.PublicId;
                                answerObj.LastUpdatedBy = item.LastUpdatedBy;
                                answerObj.LastUpdatedOn = item.LastUpdatedOn;
                                answerObj.AssociatedScore = item.AssociatedScore;
                                answerObj.IsCorrectAnswer = item.IsCorrectAnswer;
                                answerObj.IsCorrectForMultipleAnswer = item.IsCorrectForMultipleAnswer;
                                answerObj.DisplayOrder = item.DisplayOrder;
                                answerObj.Status = item.Status;
                                answerObj.State = item.State;
                                answerObj.AutoPlay = item.AutoPlay;
                                answerObj.SecondsToApply = item.SecondsToApply;
                                answerObj.VideoFrameEnabled = item.VideoFrameEnabled;
                                answerObj.IsUnansweredType = item.IsUnansweredType;
                                answerObj.OptionTextforRatingOne = item.OptionTextforRatingOne;
                                answerObj.OptionTextforRatingTwo = item.OptionTextforRatingTwo;
                                answerObj.OptionTextforRatingThree = item.OptionTextforRatingThree;
                                answerObj.OptionTextforRatingFour = item.OptionTextforRatingFour;
                                answerObj.OptionTextforRatingFive = item.OptionTextforRatingFive;
                                answerObj.ListValues = item.ListValues;

                                UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);
                                UOWObj.Save();

                                SaveDynamicVariable(string.Empty, answerObj.Option, quizDetails.Id);


                                if (item.ObjectFieldsInAnswer != null)
                                {
                                    #region insert in ObjectFieldsInAnswer

                                    foreach (var objectFieldsInAnswer in item.ObjectFieldsInAnswer)
                                    {
                                        var objectFieldsInAnswerObj = new Db.ObjectFieldsInAnswer();

                                        objectFieldsInAnswerObj.AnswerOptionsInQuizQuestionsId = answerObj.Id;
                                        objectFieldsInAnswerObj.ObjectName = objectFieldsInAnswer.ObjectName;
                                        objectFieldsInAnswerObj.FieldName = objectFieldsInAnswer.FieldName;
                                        objectFieldsInAnswerObj.Value = objectFieldsInAnswer.Value;
                                        objectFieldsInAnswerObj.CreatedOn = DateTime.UtcNow;
                                        objectFieldsInAnswerObj.CreatedBy = businessUserId;
                                        objectFieldsInAnswerObj.LastUpdatedOn = DateTime.UtcNow;
                                        objectFieldsInAnswerObj.LastUpdatedBy = businessUserId;
                                        objectFieldsInAnswerObj.IsExternalSync = objectFieldsInAnswer.IsExternalSync;
                                        objectFieldsInAnswerObj.IsCommentMapped = objectFieldsInAnswer.IsCommentMapped;

                                        UOWObj.ObjectFieldsInAnswerRepository.Insert(objectFieldsInAnswerObj);

                                        UOWObj.Save();
                                    }

                                    #endregion
                                }


                                var lstTags = item.TagsInAnswer.ToList();

                                foreach (var tag in lstTags)
                                {
                                    var TagsObj = new Db.TagsInAnswer();
                                    TagsObj.AnswerOptionsId = answerObj.Id;
                                    TagsObj.TagCategoryId = tag.TagCategoryId;
                                    TagsObj.TagId = tag.TagId;
                                    UOWObj.TagsInAnswerRepository.Insert(TagsObj);
                                    UOWObj.Save();
                                }
                            }
                            newQuestionId = obj.Id;
                            UOWObj.Save();
                            transaction.Complete();
                            #endregion
                        }

                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Question not found   " + questionId;
                    }
                }


                return GetQuizQuestionDetails(newQuestionId, parentquizId);
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
            return quizQuestionObj;
        }

        private QuizQuestion GetQuizQuestionDetails(int questionId, int quizId)
        {
            QuizQuestion quizQuestionObj = null;
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var questionObj = UOWObj.QuestionsInQuizRepository.GetByID(questionId);

                    if (questionObj != null && questionObj.QuizDetails.ParentQuizId == quizId)
                    {
                        quizQuestionObj = new QuizQuestion();

                        quizQuestionObj.QuizType = questionObj.QuizDetails.Quiz.QuizType;
                        quizQuestionObj.QuestionId = questionId;
                        quizQuestionObj.ShowAnswerImage = questionObj.ShowAnswerImage;
                        quizQuestionObj.QuestionTitle = questionObj.Question;
                        quizQuestionObj.ShowTitle = questionObj.ShowTitle;
                        quizQuestionObj.QuestionImage = questionObj.QuestionImage;
                        quizQuestionObj.EnableMediaFile = questionObj.EnableMediaFile;
                        quizQuestionObj.PublicIdForQuestion = questionObj.PublicId;
                        quizQuestionObj.ShowQuestionImage = questionObj.ShowQuestionImage;
                        quizQuestionObj.AnswerType = questionObj.AnswerType;
                        quizQuestionObj.MaxAnswer = questionObj.MaxAnswer;
                        quizQuestionObj.MinAnswer = questionObj.MinAnswer;
                        quizQuestionObj.NextButtonColor = questionObj.NextButtonColor;
                        quizQuestionObj.NextButtonText = questionObj.NextButtonText;
                        quizQuestionObj.NextButtonTxtColor = questionObj.NextButtonTxtColor;
                        quizQuestionObj.NextButtonTxtSize = questionObj.NextButtonTxtSize;
                        quizQuestionObj.EnableNextButton = questionObj.EnableNextButton;
                        quizQuestionObj.ViewPreviousQuestion = questionObj.ViewPreviousQuestion;
                        quizQuestionObj.EditAnswer = questionObj.EditAnswer;
                        quizQuestionObj.TimerRequired = questionObj.TimerRequired;
                        quizQuestionObj.Time = questionObj.Time;
                        quizQuestionObj.AutoPlay = questionObj.AutoPlay;
                        quizQuestionObj.SecondsToApply = questionObj.SecondsToApply;
                        quizQuestionObj.VideoFrameEnabled = questionObj.VideoFrameEnabled;
                        quizQuestionObj.Description = questionObj.Description;
                        quizQuestionObj.ShowDescription = questionObj.ShowDescription;
                        quizQuestionObj.DescriptionImage = questionObj.DescriptionImage;
                        quizQuestionObj.EnableMediaFileForDescription = questionObj.EnableMediaFileForDescription;
                        quizQuestionObj.PublicIdForDescription = questionObj.PublicIdForDescription;
                        quizQuestionObj.ShowDescriptionImage = questionObj.ShowDescriptionImage ?? false;
                        quizQuestionObj.AutoPlayForDescription = questionObj.AutoPlayForDescription;
                        quizQuestionObj.SecondsToApplyForDescription = questionObj.SecondsToApplyForDescription;
                        quizQuestionObj.DescVideoFrameEnabled = questionObj.DescVideoFrameEnabled;
                        quizQuestionObj.Type = questionObj.Type;
                        quizQuestionObj.DisplayOrderForTitle = questionObj.DisplayOrderForTitle;
                        quizQuestionObj.DisplayOrderForTitleImage = questionObj.DisplayOrderForTitleImage;
                        quizQuestionObj.DisplayOrderForDescription = questionObj.DisplayOrderForDescription;
                        quizQuestionObj.DisplayOrderForDescriptionImage = questionObj.DisplayOrderForDescriptionImage;
                        quizQuestionObj.DisplayOrderForAnswer = questionObj.DisplayOrderForAnswer;
                        quizQuestionObj.DisplayOrderForNextButton = questionObj.DisplayOrderForNextButton;
                        quizQuestionObj.EnableComment = questionObj.EnableComment;
                        quizQuestionObj.TopicTitle = questionObj.TopicTitle;
                        quizQuestionObj.AnswerStructureType = questionObj.AnswerStructureType.HasValue ? questionObj.AnswerStructureType.Value : (int)AnswerStructureTypeEnum.Default;
                        quizQuestionObj.IsMultiRating = questionObj.IsMultiRating;

                        quizQuestionObj.QuizCorrectAnswerSetting = new QuizCorrectAnswerSetting();

                        quizQuestionObj.QuizCorrectAnswerSetting.CorrectAnswerExplanation = questionObj.CorrectAnswerDescription;
                        quizQuestionObj.QuizCorrectAnswerSetting.RevealCorrectAnswer = questionObj.RevealCorrectAnswer;
                        quizQuestionObj.QuizCorrectAnswerSetting.AliasTextForCorrect = questionObj.AliasTextForCorrect;
                        quizQuestionObj.QuizCorrectAnswerSetting.AliasTextForIncorrect = questionObj.AliasTextForIncorrect;
                        quizQuestionObj.QuizCorrectAnswerSetting.AliasTextForYourAnswer = questionObj.AliasTextForYourAnswer;
                        quizQuestionObj.QuizCorrectAnswerSetting.AliasTextForCorrectAnswer = questionObj.AliasTextForCorrectAnswer;
                        quizQuestionObj.QuizCorrectAnswerSetting.AliasTextForExplanation = questionObj.AliasTextForExplanation;
                        quizQuestionObj.QuizCorrectAnswerSetting.AliasTextForNextButton = questionObj.AliasTextForNextButton;

                        quizQuestionObj.AnswerList = new List<AnswerOptionInQuestion>();

                        var company = questionObj.QuizDetails.Quiz.Company;

                        var companyObj = new CompanyModel()
                        {
                            LeadDashboardApiUrl = company.LeadDashboardApiUrl,
                            CompanyName = company.CompanyName,
                            ClientCode = company.ClientCode,
                            LeadDashboardApiAuthorizationBearer = company.LeadDashboardApiAuthorizationBearer
                        };

                        string tagcategoryId = null;
                        var tagCategordListObject = questionObj.AnswerOptionsInQuizQuestions.Where(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active && !r.IsUnansweredType).Where(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active && !r.IsUnansweredType);
                        if (tagCategordListObject != null && tagCategordListObject.Any())
                        {
                            var answeridList =  tagCategordListObject.Select(v => v.Id).ToList();
                           var tagList = UOWObj.TagsInAnswerRepository.GetQueryable(r => (answeridList.Any(s => s == r.AnswerOptionsId)));
                            if (tagList != null && tagList.Any())
                            {
                                var tagCategoryIdList = tagList.Select(b => b.TagCategoryId).ToList();
                                if (tagCategoryIdList != null && tagCategoryIdList.Any())
                                {
                                    tagcategoryId = String.Join(", ", tagCategoryIdList);
                                }
                            }
                        }
                        List<OWCLeadTagsResponse> tagDetails = new List<OWCLeadTagsResponse>();
                        if (tagcategoryId != null)
                        {
                            tagDetails = CommonStaticData.GetCachedTagsByCategory(tagcategoryId, companyObj);
                        }                      

                        foreach (var answer in questionObj.AnswerOptionsInQuizQuestions.Where(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active && !r.IsUnansweredType).OrderBy(r => r.DisplayOrder))
                        {
                            var answerOptionInQuestion = new AnswerOptionInQuestion();

                            answerOptionInQuestion.AnswerId = answer.Id;
                            answerOptionInQuestion.AssociatedScore = answer.AssociatedScore;
                            answerOptionInQuestion.AnswerText = answer.Option;
                            answerOptionInQuestion.AnswerImage = answer.OptionImage;
                            answerOptionInQuestion.EnableMediaFile = answer.EnableMediaFile;
                            answerOptionInQuestion.PublicIdForAnswer = answer.PublicId;
                            answerOptionInQuestion.IsCorrectAnswer = questionObj.AnswerType == (int)AnswerTypeEnum.Multiple ? answer.IsCorrectForMultipleAnswer : (questionObj.AnswerType == (int)AnswerTypeEnum.Single) ? answer.IsCorrectAnswer : null;
                            answerOptionInQuestion.DisplayOrder = answer.DisplayOrder;
                            answerOptionInQuestion.IsReadOnly = answer.IsReadOnly;
                            answerOptionInQuestion.AutoPlay = false;
                            answerOptionInQuestion.AutoPlay = (questionObj.AnswerType == (int)AnswerTypeEnum.Single || questionObj.AnswerType == (int)AnswerTypeEnum.Multiple) ? true : false;
                            answerOptionInQuestion.SecondsToApply = answer.SecondsToApply;
                            answerOptionInQuestion.VideoFrameEnabled = answer.VideoFrameEnabled;
                            answerOptionInQuestion.OptionTextforRatingOne = answer.OptionTextforRatingOne;
                            answerOptionInQuestion.OptionTextforRatingTwo = answer.OptionTextforRatingTwo;
                            answerOptionInQuestion.OptionTextforRatingThree = answer.OptionTextforRatingThree;
                            answerOptionInQuestion.OptionTextforRatingFour = answer.OptionTextforRatingFour;
                            answerOptionInQuestion.OptionTextforRatingFive = answer.OptionTextforRatingFive;
                            answerOptionInQuestion.ListValues = answer.ListValues;
                            answerOptionInQuestion.Categories = new List<AnswerOptionInQuestion.CategoryModel>();

                            if (answer.TagsInAnswer != null)
                            {
                                foreach (var tagDetail in tagDetails.Where(a => answer.TagsInAnswer.Select(b => b.TagCategoryId).Contains(a.tagCategoryId)).GroupBy(a => a.tagCategoryId).Select(x => x.FirstOrDefault()))
                                {
                                    var categoryObj = new AnswerOptionInQuestion.CategoryModel();

                                    categoryObj.CategoryName = tagDetail.tagCategory;
                                    categoryObj.CategoryId = tagDetail.tagCategoryId;

                                    categoryObj.TagDetails = new List<AnswerOptionInQuestion.CategoryModel.TagDetail>();

                                    foreach (var tag in tagDetails.Where(a => a.tagCategoryId == tagDetail.tagCategoryId && answer.TagsInAnswer.Select(b => b.TagId).Contains(a.id)))
                                    {
                                        categoryObj.TagDetails.Add(new AnswerOptionInQuestion.CategoryModel.TagDetail
                                        {
                                            TagId = tag.id,
                                            TagName = tag.tagName
                                        });
                                    }

                                    answerOptionInQuestion.Categories.Add(categoryObj);
                                }
                            }
                            
                            quizQuestionObj.AnswerList.Add(answerOptionInQuestion);

                            if (answer.ObjectFieldsInAnswer!=null && answer.ObjectFieldsInAnswer.Any())
                            {
                               foreach (var item in answer.ObjectFieldsInAnswer) {
                                    if (item.IsCommentMapped.HasValue && item.IsCommentMapped.Value == true) {
                                        answerOptionInQuestion.ObjectFieldsInAnswerComment = new AnswerOptionInQuestion.ObjectFieldsDetails();
                                        answerOptionInQuestion.ObjectFieldsInAnswerComment.ObjectName = item.ObjectName;
                                        answerOptionInQuestion.ObjectFieldsInAnswerComment.FieldName = item.FieldName;
                                        answerOptionInQuestion.ObjectFieldsInAnswerComment.Value = item.Value;
                                        answerOptionInQuestion.ObjectFieldsInAnswerComment.IsExternalSync = item.IsExternalSync;
                                        answerOptionInQuestion.ObjectFieldsInAnswerComment.IsCommentMapped = true;
                                    }
                                    else {
                                        answerOptionInQuestion.ObjectFieldsInAnswer = new AnswerOptionInQuestion.ObjectFieldsDetails();
                                        answerOptionInQuestion.ObjectFieldsInAnswer.ObjectName = item.ObjectName;
                                        answerOptionInQuestion.ObjectFieldsInAnswer.FieldName = item.FieldName;
                                        answerOptionInQuestion.ObjectFieldsInAnswer.Value = item.Value;
                                        answerOptionInQuestion.ObjectFieldsInAnswer.IsExternalSync = item.IsExternalSync;
                                        answerOptionInQuestion.ObjectFieldsInAnswer.IsCommentMapped = false;
                                    }

                                }
                            }
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Question not found for the QuestionId " + questionId;
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
               // throw ex;
            }
            return quizQuestionObj;
        }

        private void SaveDynamicVariable(string OldText, string NewText, int QuizId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var VariableObj = UOWObj.VariablesRepository.Get();

                    OldText = string.IsNullOrEmpty(OldText) ? string.Empty : OldText.ToLower().Replace("%leadid%", string.Empty).Replace("%fname%", string.Empty).Replace("%lname%", string.Empty).Replace("%phone%", string.Empty).Replace("%email%", string.Empty).Replace("%qname%", string.Empty).Replace("%qlink%", string.Empty).Replace("%qendresult%", string.Empty).Replace("%correctanswerexplanation%", string.Empty);

                    NewText = string.IsNullOrEmpty(NewText) ? string.Empty : NewText.ToLower().Replace("%leadid%", string.Empty).Replace("%fname%", string.Empty).Replace("%lname%", string.Empty).Replace("%phone%", string.Empty).Replace("%email%", string.Empty).Replace("%qname%", string.Empty).Replace("%qlink%", string.Empty).Replace("%qendresult%", string.Empty).Replace("%correctanswerexplanation%", string.Empty);

                    if (OldText.Equals(NewText, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return;
                    }


                    var NewTextDynamicVariables = Regex.Matches(NewText, @"%\b\S+?\b%").Cast<Match>().Select(m => m.Value).ToList();

                    var OldTextDynamicVariables = Regex.Matches(OldText, @"%\b\S+?\b%").Cast<Match>().Select(m => m.Value).ToList();

                    var DeletedElements = OldTextDynamicVariables.Except(NewTextDynamicVariables);

                    var AddedElements = NewTextDynamicVariables.Except(OldTextDynamicVariables);

                    var VariableInQuizObj = UOWObj.VariableInQuizRepository.Get();



                    foreach (var deletedElementsObj in DeletedElements)
                    {
                        var elements = VariableInQuizObj.FirstOrDefault(t => t.QuizId == QuizId && t.Variables != null && t.Variables.Name == deletedElementsObj.Replace("%", string.Empty));
                        if (elements != null)
                        {
                            elements.NumberOfUses = elements.NumberOfUses <= 0 ? 0 : elements.NumberOfUses - 1;
                            UOWObj.VariableInQuizRepository.Update(elements);

                            if (elements.NumberOfUses == 0 && VariableInQuizObj.Count(t => t.VariableId == elements.VariableId & t.NumberOfUses != 0) == 0)
                                UOWObj.VariablesRepository.Delete(elements.Variables);
                        }
                    }
                    UOWObj.Save();

                    foreach (var AddedElementsObj in AddedElements)
                    {
                        var variables = VariableObj.FirstOrDefault(t => t.Name == AddedElementsObj.Replace("%", string.Empty));
                        if (variables == null)
                        {
                            variables = new Db.Variables();
                            variables.Name = AddedElementsObj.Replace("%", string.Empty);
                            UOWObj.VariablesRepository.Insert(variables);
                        }
                        var variableInQuiz = variables.VariableInQuiz == null ? null : variables.VariableInQuiz.FirstOrDefault(r => r.QuizId == QuizId && r.VariableId == variables.Id);
                        if (variableInQuiz == null)
                        {
                            variableInQuiz = new Db.VariableInQuiz();
                            variableInQuiz.VariableId = variables.Id;
                            variableInQuiz.NumberOfUses = 1;
                            variableInQuiz.QuizId = QuizId;
                            UOWObj.VariableInQuizRepository.Insert(variableInQuiz);
                        }
                        else
                        {
                            variableInQuiz.NumberOfUses = variableInQuiz.NumberOfUses + 1;
                            UOWObj.VariableInQuizRepository.Update(variableInQuiz);
                        }
                        UOWObj.Save();
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        private void DeleteQuizUnusedVariables(int QuizDetailsId)
        {
            using (var UOWObj = new AutomationUnitOfWork())
            {
                var VariableInQuizObj = UOWObj.VariableInQuizRepository.Get().Where(t => t.QuizId == QuizDetailsId && t.NumberOfUses < 1);
                foreach (var item in VariableInQuizObj)
                {
                    UOWObj.VariableInQuizRepository.Delete(item);

                }

                UOWObj.Save();
            }
        }

        public QuizResult AddQuizDuplicateResult(int quizId, int quizResultId, int businessUserId, int companyId, int? quizType = 0)
        {
            QuizResult quizResult = null;
            int newResultId = 0;
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizObj = UOWObj.QuizRepository.GetByID(quizId);

                    if (quizObj != null)
                    {
                        var quizDetails = quizObj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED);

                        if (quizDetails != null)
                        {
                            var QuizResultobj = UOWObj.QuizResultsRepository.GetByID(quizResultId);
                          
                            var resultObj = new Db.QuizResults();
                            using (var transaction = Utility.CreateTransactionScope())
                            {
                               

                                resultObj.QuizId = QuizResultobj.QuizId;
                                resultObj.Title = QuizResultobj.Title;
                                resultObj.InternalTitle = QuizResultobj.InternalTitle;
                                resultObj.ShowResultImage = QuizResultobj.ShowResultImage;
                                resultObj.Description = QuizResultobj.Description;
                                resultObj.HideCallToAction = QuizResultobj.HideCallToAction;
                                resultObj.OpenLinkInNewTab = QuizResultobj.OpenLinkInNewTab;
                                resultObj.ActionButtonTxtSize = QuizResultobj.ActionButtonTxtSize;
                                resultObj.ActionButtonText = QuizResultobj.ActionButtonText;
                                resultObj.ActionButtonURL = QuizResultobj.ActionButtonURL;
                                resultObj.ActionButtonColor = QuizResultobj.ActionButtonColor;
                                resultObj.ActionButtonTitleColor = QuizResultobj.ActionButtonTitleColor;
                                resultObj.Status = QuizResultobj.Status;
                                resultObj.LastUpdatedBy = QuizResultobj.LastUpdatedBy;
                                resultObj.LastUpdatedOn = QuizResultobj.LastUpdatedOn;
                                resultObj.State = QuizResultobj.State;
                                resultObj.ShowLeadUserForm = QuizResultobj.ShowLeadUserForm;
                                resultObj.AutoPlay = QuizResultobj.AutoPlay;
                                resultObj.SecondsToApply = QuizResultobj.SecondsToApply;
                                resultObj.VideoFrameEnabled = QuizResultobj.VideoFrameEnabled;
                                resultObj.ShowExternalTitle = QuizResultobj.ShowExternalTitle;
                                resultObj.ShowInternalTitle = QuizResultobj.ShowInternalTitle;
                                resultObj.ShowDescription = QuizResultobj.ShowDescription;
                                resultObj.EnableMediaFile = QuizResultobj.EnableMediaFile;
                                resultObj.DisplayOrderForTitleImage = QuizResultobj.DisplayOrderForTitleImage;
                                resultObj.DisplayOrderForTitle = QuizResultobj.DisplayOrderForTitle;
                                resultObj.DisplayOrderForDescription = QuizResultobj.DisplayOrderForDescription;
                                resultObj.DisplayOrderForNextButton = QuizResultobj.DisplayOrderForNextButton;
                                resultObj.MinScore = QuizResultobj.MinScore;
                                resultObj.MaxScore = QuizResultobj.MaxScore;
                                resultObj.DisplayOrder = QuizResultobj.DisplayOrder;
                                resultObj.IsPersonalityCorrelatedResult = QuizResultobj.IsPersonalityCorrelatedResult;
                                resultObj.Image = QuizResultobj.Image;
                                resultObj.PublicId = QuizResultobj.PublicId;

                                UOWObj.QuizResultsRepository.Insert(resultObj);                          
                                //UOWObj.QuizRepository.Update(quizObj);
                               
                                UOWObj.Save();
                                newResultId = resultObj.Id;
                                transaction.Complete();
                            }

                            if (QuizResultobj != null)
                            {
                                var oldQuizVar = _quizVariablesService.GetQuizVariables(quizDetails.Id, QuizResultobj.Id, (int)QuizVariableObjectTypes.RESULT);

                                QuizVariableModel quizVariable = new QuizVariableModel();
                                if (oldQuizVar != null && oldQuizVar.Any())
                                {
                                    quizVariable.Variables = String.Join(",", oldQuizVar);
                                    quizVariable.QuizDetailsId = QuizResultobj.QuizId;
                                    quizVariable.ObjectId = resultObj.Id;
                                    quizVariable.ObjectTypes = (int)QuizVariableObjectTypes.RESULT;

                                    _quizVariablesService.AddQuizVariables(quizVariable);
                                }
                            }

                            SaveDynamicVariable(string.Empty, resultObj.Title, quizDetails.Id);
                            SaveDynamicVariable(string.Empty, resultObj.InternalTitle, quizDetails.Id);
                            SaveDynamicVariable(string.Empty, resultObj.Description, quizDetails.Id);
                            DeleteQuizUnusedVariables(quizDetails.Id);

                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "quizResuls not found " + quizResultId;
                    }

                }
                return GetQuizResultDetails(newResultId, quizId);
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
            return quizResult;
        }

        private QuizResult GetQuizResultDetails(int resultId, int quizId)
        {
            QuizResult quizResult = null;
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var resultObj = UOWObj.QuizResultsRepository.GetByID(resultId);

                    if (resultObj != null && resultObj.QuizDetails.ParentQuizId == quizId)
                    {
                        #region Bind return obj

                        quizResult = new QuizResult();

                        quizResult.ResultId = resultObj.Id;
                        quizResult.Title = resultObj.Title;
                        quizResult.InternalTitle = resultObj.InternalTitle ?? string.Empty;
                        quizResult.ShowResultImage = resultObj.ShowResultImage;
                        quizResult.Image = resultObj.Image;
                        quizResult.EnableMediaFile = resultObj.EnableMediaFile;
                        quizResult.PublicIdForResult = resultObj.PublicId;
                        quizResult.Description = resultObj.Description;
                        quizResult.HideCallToAction = resultObj.HideCallToAction;
                        quizResult.EnableCallToActionButton = !(resultObj.HideCallToAction ?? false);
                        quizResult.ActionButtonURL = resultObj.ActionButtonURL;
                        quizResult.OpenLinkInNewTab = resultObj.OpenLinkInNewTab;
                        quizResult.ActionButtonTxtSize = resultObj.ActionButtonTxtSize;
                        quizResult.ActionButtonColor = resultObj.ActionButtonColor;
                        quizResult.ActionButtonTitleColor = resultObj.ActionButtonTitleColor;
                        quizResult.ActionButtonText = resultObj.ActionButtonText;
                        quizResult.MinScore = resultObj.MinScore;
                        quizResult.MaxScore = resultObj.MaxScore;
                        // quizResult.ShowLeadUserForm = resultObj.ShowLeadUserForm;
                        quizResult.AutoPlay = resultObj.AutoPlay;
                        quizResult.VideoFrameEnabled = resultObj.VideoFrameEnabled;
                        quizResult.ShowExternalTitle = resultObj.ShowExternalTitle;
                        quizResult.ShowInternalTitle = resultObj.ShowInternalTitle;
                        quizResult.ShowDescription = resultObj.ShowDescription;
                        quizResult.DisplayOrderForTitle = resultObj.DisplayOrderForTitle;
                        quizResult.DisplayOrderForTitleImage = resultObj.DisplayOrderForTitleImage;
                        quizResult.DisplayOrderForDescription = resultObj.DisplayOrderForDescription;
                        quizResult.DisplayOrderForNextButton = resultObj.DisplayOrderForNextButton;
                        quizResult.SecondsToApply = resultObj.SecondsToApply;

                        quizResult.ResultSetting = new QuizResultSetting();

                        var resultSetting = resultObj.QuizDetails.ResultSettings.FirstOrDefault();

                        if (resultSetting != null)
                        {
                            quizResult.ResultSetting.QuizId = resultSetting.QuizDetails.ParentQuizId;
                            quizResult.ResultSetting.ShowScoreValue = resultSetting.ShowScoreValue;
                            quizResult.ResultSetting.ShowCorrectAnswer = resultSetting.ShowCorrectAnswer;
                            quizResult.ResultSetting.CustomTxtForScoreValueInResult = resultSetting.CustomTxtForScoreValueInResult;
                            quizResult.ResultSetting.CustomTxtForAnswerKey = resultSetting.CustomTxtForAnswerKey;
                            quizResult.ResultSetting.CustomTxtForYourAnswer = resultSetting.CustomTxtForYourAnswer;
                            quizResult.ResultSetting.CustomTxtForCorrectAnswer = resultSetting.CustomTxtForCorrectAnswer;
                            quizResult.ResultSetting.CustomTxtForExplanation = resultSetting.CustomTxtForExplanation;
                            // quizResult.ResultSetting.ShowLeadUserForm = resultObj.ShowLeadUserForm;
                            quizResult.ResultSetting.AutoPlay = resultObj.AutoPlay;
                        }

                        #endregion
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Result not found for the ResultId " + resultId.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
            return quizResult;
        }

        public int DuplicateQuizByCompanyId(DuplicateQuizRequest duplicateQuizRequest)
        {
            if(duplicateQuizRequest == null)
            { return 0; }
            int newQuizId = -1;
            var currentQuizVar = new Db.QuizVariables();


            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    using (var transaction = Utility.CreateTransactionScope())
                    {
                        var quizObj = UOWObj.QuizRepository.GetByID(duplicateQuizRequest.CurrentQuizId);
                        var newCompanyId = UOWObj.CompanyRepository.Get(r => r.ClientCode == duplicateQuizRequest.DestinationCompanyCode).Select(v => v.Id).FirstOrDefault();

                        if (quizObj != null && (newCompanyId != null && newCompanyId != 0))
                        {
                            if (quizObj.State != (int)QuizStateEnum.PUBLISHED && ((quizObj.QuizType == (int)QuizTypeEnum.AssessmentTemplate || quizObj.QuizType == (int)QuizTypeEnum.ScoreTemplate || quizObj.QuizType == (int)QuizTypeEnum.PersonalityTemplate) && !quizObj.QuizDetails.Any(r => r.State == (int)QuizStateEnum.PUBLISHED)))
                            {
                                Status = ResultEnum.OkWithMessage;
                                ErrorMessage = "Quiz cannot be duplicated because changes are not yet published for the QuizId " + duplicateQuizRequest.CurrentQuizId + "and CompanyCode" + duplicateQuizRequest.DestinationCompanyCode;
                            }
                            else //duplicate only if quiz is published
                            {
                                var currentDate = DateTime.UtcNow;

                                #region insert in Quiz

                                var nQuizObj = new Db.Quiz();

                                nQuizObj.PublishedCode = Guid.NewGuid().ToString();

                                switch (quizObj.QuizType)
                                {
                                    case (int)QuizTypeEnum.AssessmentTemplate:
                                        nQuizObj.QuizType = (int)QuizTypeEnum.Assessment;
                                        break;
                                    case (int)QuizTypeEnum.ScoreTemplate:
                                        nQuizObj.QuizType = (int)QuizTypeEnum.Score;
                                        break;
                                    case (int)QuizTypeEnum.PersonalityTemplate:
                                        nQuizObj.QuizType = (int)QuizTypeEnum.Personality;
                                        break;
                                    default:
                                        nQuizObj.QuizType = quizObj.QuizType;
                                        break;
                                }

                                var publishedQuizDetails = new Db.QuizDetails();

                                if (quizObj.QuizType == (int)QuizTypeEnum.AssessmentTemplate || quizObj.QuizType == (int)QuizTypeEnum.ScoreTemplate || quizObj.QuizType == (int)QuizTypeEnum.PersonalityTemplate)
                                {
                                    publishedQuizDetails = quizObj.QuizDetails.Where(r => r.State == (int)QuizStateEnum.PUBLISHED).OrderByDescending(r => r.Version).FirstOrDefault();

                                    if (!string.IsNullOrEmpty(duplicateQuizRequest.DestinationOfficeId))
                                    {
                                        nQuizObj.AccessibleOfficeId = duplicateQuizRequest.DestinationOfficeId;
                                    }
                                }
                                else
                                {
                                    publishedQuizDetails = quizObj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED);

                                    if (!string.IsNullOrEmpty(duplicateQuizRequest.DestinationOfficeId))
                                    {
                                        //nQuizObj.AccessibleOfficeId = quizObj.AccessibleOfficeId;
                                        nQuizObj.AccessibleOfficeId = duplicateQuizRequest.DestinationOfficeId;
                                    }
                                }

                                nQuizObj.State = (int)QuizStateEnum.DRAFTED;
                                //nQuizObj.CompanyId = CompanyInfo.Id;
                                nQuizObj.CompanyId = newCompanyId;
                                nQuizObj.QuesAndContentInSameTable = quizObj.QuesAndContentInSameTable;

                                UOWObj.QuizRepository.Insert(nQuizObj);

                                UOWObj.Save();

                                newQuizId = nQuizObj.Id;

                                #endregion

                                if (publishedQuizDetails != null)
                                {
                                    List<Mappings> mappingLst = new List<Mappings>();
                                    List<int> branchingLogicAnsIds = new List<int>();

                                    #region insert in Quiz Details

                                    var publishedQuizDetailsObj = new Db.QuizDetails();

                                    publishedQuizDetailsObj.ParentQuizId = nQuizObj.Id;
                                    publishedQuizDetailsObj.QuizTitle = publishedQuizDetails.QuizTitle;
                                    publishedQuizDetailsObj.QuizCoverTitle = publishedQuizDetails.QuizCoverTitle;
                                    publishedQuizDetailsObj.ShowQuizCoverTitle = publishedQuizDetails.ShowQuizCoverTitle;
                                    publishedQuizDetailsObj.QuizCoverImage = publishedQuizDetails.QuizCoverImage;
                                    publishedQuizDetailsObj.ShowQuizCoverImage = publishedQuizDetails.ShowQuizCoverImage;
                                    publishedQuizDetailsObj.PublicId = publishedQuizDetails.PublicId;
                                    publishedQuizDetailsObj.QuizCoverImgXCoordinate = publishedQuizDetails.QuizCoverImgXCoordinate;
                                    publishedQuizDetailsObj.QuizCoverImgYCoordinate = publishedQuizDetails.QuizCoverImgYCoordinate;
                                    publishedQuizDetailsObj.QuizCoverImgHeight = publishedQuizDetails.QuizCoverImgHeight;
                                    publishedQuizDetailsObj.QuizCoverImgWidth = publishedQuizDetails.QuizCoverImgWidth;
                                    publishedQuizDetailsObj.QuizCoverImgAttributionLabel = publishedQuizDetails.QuizCoverImgAttributionLabel;
                                    publishedQuizDetailsObj.QuizCoverImgAltTag = publishedQuizDetails.QuizCoverImgAltTag;
                                    publishedQuizDetailsObj.QuizDescription = publishedQuizDetails.QuizDescription;
                                    publishedQuizDetailsObj.ShowDescription = publishedQuizDetails.ShowDescription;
                                    publishedQuizDetailsObj.StartButtonText = publishedQuizDetails.StartButtonText;
                                    publishedQuizDetailsObj.EnableNextButton = publishedQuizDetails.EnableNextButton;
                                    publishedQuizDetailsObj.IsBranchingLogicEnabled = publishedQuizDetails.IsBranchingLogicEnabled;
                                    publishedQuizDetailsObj.HideSocialShareButtons = publishedQuizDetails.HideSocialShareButtons;
                                    publishedQuizDetailsObj.EnableFacebookShare = publishedQuizDetails.EnableFacebookShare;
                                    publishedQuizDetailsObj.EnableTwitterShare = publishedQuizDetails.EnableTwitterShare;
                                    publishedQuizDetailsObj.EnableLinkedinShare = publishedQuizDetails.EnableLinkedinShare;
                                    publishedQuizDetailsObj.State = (int)QuizStateEnum.DRAFTED;
                                    publishedQuizDetailsObj.Version = 1;
                                    publishedQuizDetailsObj.Status = (int)StatusEnum.Active;
                                    publishedQuizDetailsObj.CreatedOn = currentDate;
                                    publishedQuizDetailsObj.CreatedBy = duplicateQuizRequest.DestinationBusinessUserId;
                                    publishedQuizDetailsObj.LastUpdatedOn = currentDate;
                                    publishedQuizDetailsObj.LastUpdatedBy = duplicateQuizRequest.DestinationBusinessUserId;
                                    publishedQuizDetailsObj.ViewPreviousQuestion = publishedQuizDetails.ViewPreviousQuestion;
                                    publishedQuizDetailsObj.EditAnswer = publishedQuizDetails.EditAnswer;
                                    publishedQuizDetailsObj.AutoPlay = publishedQuizDetails.AutoPlay;
                                    publishedQuizDetailsObj.DisplayOrderForTitle = publishedQuizDetails.DisplayOrderForTitle;
                                    publishedQuizDetailsObj.DisplayOrderForTitleImage = publishedQuizDetails.DisplayOrderForTitleImage;
                                    publishedQuizDetailsObj.DisplayOrderForDescription = publishedQuizDetails.DisplayOrderForDescription;
                                    publishedQuizDetailsObj.DisplayOrderForNextButton = publishedQuizDetails.DisplayOrderForNextButton;
                                    //publishedQuizDetailsObj.CompanyId = publishedQuizDetails.CompanyId;
                                    publishedQuizDetailsObj.CompanyId = newCompanyId;

                                    UOWObj.QuizDetailsRepository.Insert(publishedQuizDetailsObj);

                                    UOWObj.Save();

                                    var oldvarDetails = UOWObj.QuizVariablesRepository.Get(v => v.QuizDetailsId == publishedQuizDetails.Id && v.ObjectId == publishedQuizDetails.Id && v.ObjectTypes == (int)QuizVariableObjectTypes.COVER);

                                    if (oldvarDetails != null && oldvarDetails.Any())
                                    {
                                        foreach (var details in oldvarDetails)
                                        {
                                            currentQuizVar.QuizDetailsId = publishedQuizDetailsObj.Id;
                                            currentQuizVar.ObjectTypes = details.ObjectTypes;
                                            currentQuizVar.ObjectId = publishedQuizDetailsObj.Id;
                                            //currentQuizVar.CompanyId = details.CompanyId;
                                            currentQuizVar.CompanyId = newCompanyId;
                                            currentQuizVar.Variables = details.Variables;
                                            UOWObj.QuizVariablesRepository.Insert(currentQuizVar);
                                            UOWObj.Save();
                                        }

                                    }


                                    #endregion

                                    #region insert in Quiz Results

                                    var lstResults = publishedQuizDetails.QuizResults.Where(r => r.Status == (int)StatusEnum.Active).ToList();

                                    foreach (var item in lstResults)
                                    {
                                        var quizResultObj = new Db.QuizResults();

                                        quizResultObj.QuizId = publishedQuizDetailsObj.Id;
                                        quizResultObj.Title = item.Title;
                                        quizResultObj.ShowExternalTitle = item.ShowExternalTitle;
                                        quizResultObj.InternalTitle = item.InternalTitle;
                                        quizResultObj.ShowInternalTitle = item.ShowInternalTitle;
                                        quizResultObj.Image = item.Image;
                                        quizResultObj.PublicId = item.PublicId;
                                        quizResultObj.Description = item.Description;
                                        quizResultObj.ShowDescription = item.ShowDescription;
                                        quizResultObj.ActionButtonURL = item.ActionButtonURL;
                                        quizResultObj.ActionButtonTxtSize = item.ActionButtonTxtSize;
                                        quizResultObj.ActionButtonTitleColor = item.ActionButtonTitleColor;
                                        quizResultObj.LastUpdatedOn = currentDate;
                                        quizResultObj.LastUpdatedBy = duplicateQuizRequest.DestinationBusinessUserId;
                                        quizResultObj.HideCallToAction = item.HideCallToAction;
                                        quizResultObj.ActionButtonText = item.ActionButtonText;
                                        quizResultObj.ShowResultImage = item.ShowResultImage;
                                        quizResultObj.OpenLinkInNewTab = item.OpenLinkInNewTab;
                                        quizResultObj.ActionButtonColor = item.ActionButtonColor;
                                        quizResultObj.MinScore = item.MinScore;
                                        quizResultObj.MaxScore = item.MaxScore;
                                        quizResultObj.DisplayOrder = item.DisplayOrder;
                                        quizResultObj.IsPersonalityCorrelatedResult = item.IsPersonalityCorrelatedResult;
                                        quizResultObj.DisplayOrderForTitle = item.DisplayOrderForTitle;
                                        quizResultObj.DisplayOrderForTitleImage = item.DisplayOrderForTitleImage;
                                        quizResultObj.DisplayOrderForDescription = item.DisplayOrderForDescription;
                                        quizResultObj.DisplayOrderForNextButton = item.DisplayOrderForNextButton;
                                        quizResultObj.Status = (int)StatusEnum.Active;
                                        quizResultObj.State = (int)QuizStateEnum.DRAFTED;
                                        //  quizResultObj.ShowLeadUserForm = item.ShowLeadUserForm;
                                        quizResultObj.AutoPlay = item.AutoPlay;

                                        UOWObj.QuizResultsRepository.Insert(quizResultObj);

                                        UOWObj.Save();

                                        mappingLst.Add(new Mappings
                                        {
                                            DraftedId = item.Id,
                                            PublishedId = quizResultObj.Id,
                                            Type = (int)BranchingLogicEnum.RESULT
                                        });

                                        var oldvarResultDetails = UOWObj.QuizVariablesRepository.Get(v => v.QuizDetailsId == publishedQuizDetails.Id && v.ObjectId == item.Id && v.ObjectTypes == (int)QuizVariableObjectTypes.RESULT);
                                        if (oldvarResultDetails != null && oldvarResultDetails.Any())
                                        {
                                            foreach (var result in oldvarResultDetails)
                                            {
                                                currentQuizVar.QuizDetailsId = publishedQuizDetailsObj.Id;
                                                currentQuizVar.ObjectTypes = result.ObjectTypes;
                                                currentQuizVar.ObjectId = quizResultObj.Id;
                                                //currentQuizVar.CompanyId = result.CompanyId;
                                                currentQuizVar.CompanyId = newCompanyId;
                                                currentQuizVar.Variables = result.Variables;
                                                UOWObj.QuizVariablesRepository.Insert(currentQuizVar);
                                                UOWObj.Save();
                                            }

                                        }

                                    }

                                    #endregion

                                    #region insert in Result Settings

                                    var draftedResultSetting = publishedQuizDetails.ResultSettings.FirstOrDefault();

                                    if (draftedResultSetting != null)
                                    {
                                        var resultSettingObj = new Db.ResultSettings();

                                        resultSettingObj.QuizId = publishedQuizDetailsObj.Id;
                                        resultSettingObj.ShowScoreValue = draftedResultSetting.ShowScoreValue;
                                        resultSettingObj.ShowCorrectAnswer = draftedResultSetting.ShowCorrectAnswer;
                                        resultSettingObj.MinScore = draftedResultSetting.MinScore;
                                        resultSettingObj.CustomTxtForAnswerKey = draftedResultSetting.CustomTxtForAnswerKey;
                                        resultSettingObj.CustomTxtForYourAnswer = draftedResultSetting.CustomTxtForYourAnswer;
                                        resultSettingObj.CustomTxtForCorrectAnswer = draftedResultSetting.CustomTxtForCorrectAnswer;
                                        resultSettingObj.CustomTxtForExplanation = draftedResultSetting.CustomTxtForExplanation;
                                        resultSettingObj.CustomTxtForScoreValueInResult = draftedResultSetting.CustomTxtForScoreValueInResult;
                                        resultSettingObj.LastUpdatedBy = duplicateQuizRequest.DestinationBusinessUserId;
                                        resultSettingObj.LastUpdatedOn = currentDate;
                                        resultSettingObj.State = (int)QuizStateEnum.DRAFTED;

                                        UOWObj.ResultSettingsRepository.Insert(resultSettingObj);

                                        UOWObj.Save();
                                    }

                                    #endregion

                                    #region insert in Quiz personality setting
                                    if (quizObj.QuizType == (int)QuizTypeEnum.Personality || quizObj.QuizType == (int)QuizTypeEnum.PersonalityTemplate)
                                    {
                                        var personalityResult = publishedQuizDetails.PersonalityResultSetting.FirstOrDefault();
                                        var personalityResultObj = new Db.PersonalityResultSetting();

                                        personalityResultObj.QuizId = publishedQuizDetailsObj.Id;
                                        personalityResultObj.Title = personalityResult.Title;
                                        personalityResultObj.Status = personalityResult.Status;
                                        personalityResultObj.MaxResult = personalityResult.MaxResult;
                                        personalityResultObj.GraphColor = personalityResult.GraphColor;
                                        personalityResultObj.ButtonColor = personalityResult.ButtonColor;
                                        personalityResultObj.ButtonFontColor = personalityResult.ButtonFontColor;
                                        personalityResultObj.SideButtonText = personalityResult.SideButtonText;
                                        personalityResultObj.IsFullWidthEnable = personalityResult.IsFullWidthEnable;
                                        personalityResultObj.LastUpdatedOn = currentDate;
                                        personalityResultObj.LastUpdatedBy = duplicateQuizRequest.DestinationBusinessUserId;
                                        //personalityResultObj.ShowLeadUserForm = personalityResult.ShowLeadUserForm;

                                        UOWObj.PersonalityResultSettingRepository.Insert(personalityResultObj);
                                        UOWObj.Save();
                                    }
                                    #endregion

                                    #region insert in Quiz BrandingAndStyle

                                    var draftedBrandingAndStyle = publishedQuizDetails.QuizBrandingAndStyle.FirstOrDefault();

                                    if (draftedBrandingAndStyle != null)
                                    {
                                        var brandingAndStyleObj = new Db.QuizBrandingAndStyle();

                                        brandingAndStyleObj.QuizId = publishedQuizDetailsObj.Id;
                                        brandingAndStyleObj.ImageFileURL = draftedBrandingAndStyle.ImageFileURL;
                                        brandingAndStyleObj.PublicId = draftedBrandingAndStyle.PublicId;
                                        brandingAndStyleObj.BackgroundColor = draftedBrandingAndStyle.BackgroundColor;
                                        brandingAndStyleObj.ButtonColor = draftedBrandingAndStyle.ButtonColor;
                                        brandingAndStyleObj.OptionColor = draftedBrandingAndStyle.OptionColor;
                                        brandingAndStyleObj.ButtonFontColor = draftedBrandingAndStyle.ButtonFontColor;
                                        brandingAndStyleObj.OptionFontColor = draftedBrandingAndStyle.OptionFontColor;
                                        brandingAndStyleObj.FontColor = draftedBrandingAndStyle.FontColor;
                                        brandingAndStyleObj.ButtonHoverColor = draftedBrandingAndStyle.ButtonHoverColor;
                                        brandingAndStyleObj.ButtonHoverTextColor = draftedBrandingAndStyle.ButtonHoverTextColor;
                                        brandingAndStyleObj.FontType = draftedBrandingAndStyle.FontType;
                                        brandingAndStyleObj.BackgroundColorofSelectedAnswer = draftedBrandingAndStyle.BackgroundColorofSelectedAnswer;
                                        brandingAndStyleObj.BackgroundColorofAnsweronHover = draftedBrandingAndStyle.BackgroundColorofAnsweronHover;
                                        brandingAndStyleObj.AnswerTextColorofSelectedAnswer = draftedBrandingAndStyle.AnswerTextColorofSelectedAnswer;
                                        brandingAndStyleObj.ApplyToAll = draftedBrandingAndStyle.ApplyToAll;
                                        brandingAndStyleObj.IsBackType = draftedBrandingAndStyle.IsBackType;
                                        brandingAndStyleObj.BackColor = draftedBrandingAndStyle.BackColor;
                                        brandingAndStyleObj.Opacity = draftedBrandingAndStyle.Opacity;
                                        brandingAndStyleObj.LogoUrl = draftedBrandingAndStyle.LogoUrl;
                                        brandingAndStyleObj.LogoPublicId = draftedBrandingAndStyle.LogoPublicId;
                                        brandingAndStyleObj.BackgroundColorofLogo = draftedBrandingAndStyle.BackgroundColorofLogo;
                                        brandingAndStyleObj.AutomationAlignment = draftedBrandingAndStyle.AutomationAlignment;
                                        brandingAndStyleObj.LogoAlignment = draftedBrandingAndStyle.LogoAlignment;
                                        brandingAndStyleObj.Flip = draftedBrandingAndStyle.Flip;
                                        brandingAndStyleObj.Language = draftedBrandingAndStyle.Language;
                                        brandingAndStyleObj.BackImageFileURL = draftedBrandingAndStyle.BackImageFileURL;
                                        brandingAndStyleObj.LastUpdatedBy = duplicateQuizRequest.DestinationBusinessUserId;
                                        brandingAndStyleObj.LastUpdatedOn = currentDate;
                                        brandingAndStyleObj.State = (int)QuizStateEnum.DRAFTED;

                                        UOWObj.QuizBrandingAndStyleRepository.Insert(brandingAndStyleObj);

                                        UOWObj.Save();
                                    }

                                    #endregion

                                    #region insert in Questions

                                    var lstQuestions = publishedQuizDetails.QuestionsInQuiz.Where(r => r.Status == (int)StatusEnum.Active).ToList();

                                    foreach (var question in lstQuestions)
                                    {
                                        var questionObj = new Db.QuestionsInQuiz();

                                        questionObj.QuizId = publishedQuizDetailsObj.Id;
                                        questionObj.Question = question.Question;
                                        questionObj.ShowTitle = question.ShowTitle;
                                        questionObj.QuestionImage = question.QuestionImage;
                                        questionObj.PublicId = question.PublicId;
                                        questionObj.CorrectAnswerDescription = question.CorrectAnswerDescription;
                                        questionObj.RevealCorrectAnswer = question.RevealCorrectAnswer;
                                        questionObj.AliasTextForCorrect = question.AliasTextForCorrect;
                                        questionObj.AliasTextForIncorrect = question.AliasTextForIncorrect;
                                        questionObj.AliasTextForYourAnswer = question.AliasTextForYourAnswer;
                                        questionObj.AliasTextForCorrectAnswer = question.AliasTextForCorrectAnswer;
                                        questionObj.AliasTextForExplanation = question.AliasTextForExplanation;
                                        questionObj.AliasTextForNextButton = question.AliasTextForNextButton;
                                        questionObj.EnableNextButton = question.EnableNextButton;
                                        questionObj.ShowQuestionImage = question.ShowQuestionImage;
                                        questionObj.LastUpdatedOn = currentDate;
                                        questionObj.LastUpdatedBy = duplicateQuizRequest.DestinationBusinessUserId;
                                        questionObj.DisplayOrder = question.DisplayOrder;
                                        questionObj.ShowAnswerImage = question.ShowAnswerImage;
                                        questionObj.Status = (int)StatusEnum.Active;
                                        questionObj.State = (int)QuizStateEnum.DRAFTED;
                                        questionObj.AnswerType = question.AnswerType;
                                        questionObj.MinAnswer = question.MinAnswer;
                                        questionObj.MaxAnswer = question.MaxAnswer;
                                        questionObj.NextButtonColor = question.NextButtonColor;
                                        questionObj.NextButtonText = question.NextButtonText;
                                        questionObj.NextButtonTxtColor = question.NextButtonTxtColor;
                                        questionObj.NextButtonTxtSize = question.NextButtonTxtSize;
                                        questionObj.ViewPreviousQuestion = question.ViewPreviousQuestion;
                                        questionObj.EditAnswer = question.EditAnswer;
                                        questionObj.TimerRequired = question.TimerRequired;
                                        questionObj.Time = question.Time;
                                        questionObj.AutoPlay = question.AutoPlay;
                                        questionObj.Description = question.Description;
                                        questionObj.ShowDescription = question.ShowDescription;
                                        questionObj.DescriptionImage = question.DescriptionImage;
                                        questionObj.EnableMediaFileForDescription = question.EnableMediaFileForDescription;
                                        questionObj.ShowDescriptionImage = question.ShowDescriptionImage;
                                        questionObj.PublicIdForDescription = question.PublicIdForDescription;
                                        questionObj.AutoPlayForDescription = question.AutoPlayForDescription;
                                        questionObj.Type = question.Type;
                                        questionObj.DisplayOrderForTitle = question.DisplayOrderForTitle;
                                        questionObj.DisplayOrderForTitleImage = question.DisplayOrderForTitleImage;
                                        questionObj.DisplayOrderForDescription = question.DisplayOrderForDescription;
                                        questionObj.DisplayOrderForDescriptionImage = question.DisplayOrderForDescriptionImage;
                                        questionObj.DisplayOrderForAnswer = question.DisplayOrderForAnswer;
                                        questionObj.DisplayOrderForNextButton = question.DisplayOrderForNextButton;
                                        questionObj.EnableComment = question.EnableComment;
                                        questionObj.TopicTitle = question.TopicTitle;
                                        questionObj.AnswerStructureType = question.AnswerStructureType;
                                        questionObj.TemplateId = question.TemplateId;
                                        questionObj.LanguageCode = question.LanguageCode;

                                        UOWObj.QuestionsInQuizRepository.Insert(questionObj);

                                        UOWObj.Save();

                                        mappingLst.Add(new Mappings
                                        {
                                            DraftedId = question.Id,
                                            PublishedId = questionObj.Id,
                                            Type = question.Type
                                        });

                                        var oldvarQuesDetails = UOWObj.QuizVariablesRepository.Get(v => v.QuizDetailsId == publishedQuizDetails.Id && v.ObjectId == question.Id && v.ObjectTypes == (int)QuizVariableObjectTypes.QUESTION);
                                        if (oldvarQuesDetails != null && oldvarQuesDetails.Any())
                                        {
                                            foreach (var ques in oldvarQuesDetails)
                                            {
                                                currentQuizVar.QuizDetailsId = publishedQuizDetailsObj.Id;
                                                currentQuizVar.ObjectTypes = ques.ObjectTypes;
                                                currentQuizVar.ObjectId = questionObj.Id;
                                                //currentQuizVar.CompanyId = ques.CompanyId;
                                                currentQuizVar.CompanyId = newCompanyId;
                                                currentQuizVar.Variables = ques.Variables;
                                                UOWObj.QuizVariablesRepository.Insert(currentQuizVar);
                                                UOWObj.Save();
                                            }

                                        }

                                        #region insert in AnswerOptions

                                        var lstAnswers = question.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active).ToList();

                                        foreach (var answer in lstAnswers)
                                        {
                                            var answerObj = new Db.AnswerOptionsInQuizQuestions();

                                            answerObj.QuestionId = questionObj.Id;
                                            answerObj.QuizId = questionObj.QuizId;
                                            answerObj.Option = answer.Option;
                                            answerObj.Description = answer.Description;
                                            answerObj.OptionImage = answer.OptionImage;
                                            answerObj.PublicId = answer.PublicId;
                                            answerObj.AssociatedScore = answer.AssociatedScore;
                                            answerObj.IsCorrectAnswer = answer.IsCorrectAnswer;
                                            answerObj.IsCorrectForMultipleAnswer = answer.IsCorrectForMultipleAnswer;
                                            answerObj.LastUpdatedOn = currentDate;
                                            answerObj.LastUpdatedBy = duplicateQuizRequest.DestinationBusinessUserId;
                                            answerObj.DisplayOrder = answer.DisplayOrder;
                                            answerObj.IsReadOnly = answer.IsReadOnly;
                                            answerObj.IsUnansweredType = answer.IsUnansweredType;
                                            answerObj.AutoPlay = answer.AutoPlay;
                                            answerObj.OptionTextforRatingOne = answer.OptionTextforRatingOne;
                                            answerObj.OptionTextforRatingTwo = answer.OptionTextforRatingTwo;
                                            answerObj.OptionTextforRatingThree = answer.OptionTextforRatingThree;
                                            answerObj.OptionTextforRatingFour = answer.OptionTextforRatingFour;
                                            answerObj.OptionTextforRatingFive = answer.OptionTextforRatingFive;
                                            answerObj.Status = (int)StatusEnum.Active;
                                            answerObj.State = (int)QuizStateEnum.DRAFTED;
                                            answerObj.ListValues = answer.ListValues;
                                            answerObj.RefId = answer.RefId;

                                            UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);

                                            UOWObj.Save();
                                            if (question.Type == (int)BranchingLogicEnum.WHATSAPPTEMPLATE)
                                            {
                                                mappingLst.Add(new Mappings
                                                {
                                                    DraftedId = answer.Id,
                                                    PublishedId = answerObj.Id,
                                                    Type = (int)BranchingLogicEnum.WHATSAPPTEMPLATEACTION
                                                });
                                            }
                                            else
                                            {
                                                mappingLst.Add(new Mappings
                                                {
                                                    DraftedId = answer.Id,
                                                    PublishedId = answerObj.Id,
                                                    Type = (int)BranchingLogicEnum.ANSWER
                                                });
                                            }

                                            #region insert in ObjectFieldsInAnswer

                                            foreach (var objectFieldsInAnswer in answer.ObjectFieldsInAnswer)
                                            {
                                                var objectFieldsInAnswerObj = new Db.ObjectFieldsInAnswer();

                                                objectFieldsInAnswerObj.AnswerOptionsInQuizQuestionsId = answerObj.Id;
                                                objectFieldsInAnswerObj.ObjectName = objectFieldsInAnswer.ObjectName;
                                                objectFieldsInAnswerObj.FieldName = objectFieldsInAnswer.FieldName;
                                                objectFieldsInAnswerObj.Value = objectFieldsInAnswer.Value;
                                                objectFieldsInAnswerObj.CreatedOn = currentDate;
                                                objectFieldsInAnswerObj.CreatedBy = duplicateQuizRequest.DestinationBusinessUserId;
                                                objectFieldsInAnswerObj.LastUpdatedOn = currentDate;
                                                objectFieldsInAnswerObj.LastUpdatedBy = duplicateQuizRequest.DestinationBusinessUserId;
                                                objectFieldsInAnswerObj.IsExternalSync = objectFieldsInAnswer.IsExternalSync;
                                                objectFieldsInAnswerObj.IsCommentMapped= objectFieldsInAnswer.IsCommentMapped;

                                                UOWObj.ObjectFieldsInAnswerRepository.Insert(objectFieldsInAnswerObj);

                                                UOWObj.Save();
                                            }

                                            #endregion

                                            var lstTags = answer.TagsInAnswer.ToList();

                                            foreach (var tag in lstTags)
                                            {
                                                var TagsObj = new Db.TagsInAnswer();
                                                TagsObj.AnswerOptionsId = answerObj.Id;
                                                TagsObj.TagCategoryId = tag.TagCategoryId;
                                                TagsObj.TagId = tag.TagId;
                                                UOWObj.TagsInAnswerRepository.Insert(TagsObj);
                                                UOWObj.Save();
                                            }

                                            #region insert in Correlation
                                            if (quizObj.QuizType == (int)QuizTypeEnum.Personality || quizObj.QuizType == (int)QuizTypeEnum.PersonalityTemplate)
                                            {
                                                var lstCorrelation = answer.PersonalityAnswerResultMapping.Where(r => r.QuizResults.Status == (int)StatusEnum.Active);
                                                foreach (var correlation in lstCorrelation)
                                                {
                                                    var resultMappingObj = new Db.PersonalityAnswerResultMapping();
                                                    resultMappingObj.AnswerId = answerObj.Id;
                                                    resultMappingObj.ResultId = publishedQuizDetailsObj.QuizResults.FirstOrDefault(r => lstResults.FirstOrDefault(x => x.Id == correlation.ResultId).DisplayOrder.Equals(r.DisplayOrder)).Id;
                                                    UOWObj.PersonalityAnswerResultMappingRepository.Insert(resultMappingObj);
                                                    UOWObj.Save();
                                                }
                                            }
                                            #endregion
                                        }
                                        #endregion
                                    }

                                    #endregion

                                    #region insert in Content

                                    var lstContent = publishedQuizDetails.ContentsInQuiz.Where(r => r.Status == (int)StatusEnum.Active).ToList();

                                    foreach (var content in lstContent)
                                    {
                                        var contentObj = new Db.ContentsInQuiz();

                                        contentObj.QuizId = publishedQuizDetailsObj.Id;
                                        contentObj.ContentTitle = content.ContentTitle;
                                        contentObj.ShowTitle = content.ShowTitle;
                                        contentObj.ContentTitleImage = content.ContentTitleImage;
                                        contentObj.PublicIdForContentTitle = content.PublicIdForContentTitle;
                                        contentObj.ContentDescription = content.ContentDescription;
                                        contentObj.ShowDescription = content.ShowDescription;
                                        contentObj.ContentDescriptionImage = content.ContentDescriptionImage;
                                        contentObj.PublicIdForContentDescription = content.PublicIdForContentDescription;
                                        contentObj.ShowContentDescriptionImage = content.ShowContentDescriptionImage;
                                        contentObj.AliasTextForNextButton = content.AliasTextForNextButton;
                                        contentObj.EnableNextButton = content.EnableNextButton;
                                        contentObj.DisplayOrder = content.DisplayOrder;
                                        contentObj.LastUpdatedOn = currentDate;
                                        contentObj.LastUpdatedBy = duplicateQuizRequest.DestinationBusinessUserId;
                                        contentObj.ViewPreviousQuestion = content.ViewPreviousQuestion;
                                        contentObj.AutoPlay = content.AutoPlay;
                                        contentObj.AutoPlayForDescription = content.AutoPlayForDescription;
                                        contentObj.DisplayOrderForTitle = content.DisplayOrderForTitle;
                                        contentObj.DisplayOrderForTitleImage = content.DisplayOrderForTitleImage;
                                        contentObj.DisplayOrderForDescription = content.DisplayOrderForDescription;
                                        contentObj.DisplayOrderForDescriptionImage = content.DisplayOrderForDescriptionImage;
                                        contentObj.DisplayOrderForNextButton = content.DisplayOrderForNextButton;

                                        contentObj.Status = (int)StatusEnum.Active;
                                        contentObj.State = (int)QuizStateEnum.DRAFTED;

                                        UOWObj.ContentsInQuizRepository.Insert(contentObj);

                                        UOWObj.Save();

                                        mappingLst.Add(new Mappings
                                        {
                                            DraftedId = content.Id,
                                            PublishedId = contentObj.Id,
                                            Type = (int)BranchingLogicEnum.CONTENT
                                        });
                                    }

                                    #endregion

                                    #region insert in Action

                                    var lstAction = publishedQuizDetails.ActionsInQuiz.Where(r => r.Status == (int)StatusEnum.Active).ToList();

                                    foreach (var action in lstAction)
                                    {
                                        var actionObj = new Db.ActionsInQuiz();

                                        actionObj.QuizId = publishedQuizDetailsObj.Id;
                                        actionObj.Title = action.Title;
                                        actionObj.ReportEmails = action.ReportEmails;
                                        actionObj.AppointmentId = action.AppointmentId;
                                        actionObj.AutomationId = action.AutomationId;
                                        actionObj.ActionType = action.ActionType;
                                        actionObj.LastUpdatedOn = currentDate;
                                        actionObj.LastUpdatedBy = duplicateQuizRequest.DestinationBusinessUserId;
                                        actionObj.Status = (int)StatusEnum.Active;
                                        actionObj.State = (int)QuizStateEnum.DRAFTED;

                                        UOWObj.ActionsInQuizRepository.Insert(actionObj);

                                        UOWObj.Save();

                                        mappingLst.Add(new Mappings
                                        {
                                            DraftedId = action.Id,
                                            PublishedId = actionObj.Id,
                                            Type = (int)BranchingLogicEnum.ACTION
                                        });

                                        #region insert in LinkedCalendarInAction

                                        foreach (var calObj in action.LinkedCalendarInAction)
                                        {
                                            var linkedCalendarInActionObj = new Db.LinkedCalendarInAction();
                                            linkedCalendarInActionObj.ActionsInQuizId = actionObj.Id;
                                            linkedCalendarInActionObj.CalendarId = calObj.CalendarId;
                                            UOWObj.LinkedCalendarInActionRepository.Insert(linkedCalendarInActionObj);
                                        }

                                        UOWObj.Save();

                                        #endregion
                                    }

                                    #endregion

                                    #region insert in Badge

                                    var lstBadge = publishedQuizDetails.BadgesInQuiz.Where(r => r.Status == (int)StatusEnum.Active).ToList();

                                    foreach (var badge in lstBadge)
                                    {
                                        var badgeObj = new Db.BadgesInQuiz();

                                        badgeObj.QuizId = publishedQuizDetailsObj.Id;
                                        badgeObj.Title = badge.Title;
                                        badgeObj.ShowTitle = badge.ShowTitle;
                                        badgeObj.Image = badge.Image;
                                        badgeObj.ShowImage = badge.ShowImage;
                                        badgeObj.PublicId = badge.PublicId;
                                        badgeObj.DisplayOrderForTitle = badge.DisplayOrderForTitle;
                                        badgeObj.DisplayOrderForTitleImage = badge.DisplayOrderForTitleImage;
                                        badgeObj.LastUpdatedOn = currentDate;
                                        badgeObj.LastUpdatedBy = duplicateQuizRequest.DestinationBusinessUserId;
                                        badgeObj.Status = (int)StatusEnum.Active;
                                        badgeObj.State = (int)QuizStateEnum.DRAFTED;

                                        UOWObj.BadgesInQuizRepository.Insert(badgeObj);

                                        UOWObj.Save();

                                        mappingLst.Add(new Mappings
                                        {
                                            DraftedId = badge.Id,
                                            PublishedId = badgeObj.Id,
                                            Type = (int)BranchingLogicEnum.BADGE
                                        });
                                    }

                                    #endregion

                                    #region insert into VariableInQuiz

                                    var lstVariableInQuiz = publishedQuizDetails.VariableInQuiz.Where(t => t.NumberOfUses > 0);

                                    foreach (var variable in lstVariableInQuiz)
                                    {
                                        var variableInQuizObj = new Db.VariableInQuiz();

                                        variableInQuizObj.QuizId = publishedQuizDetailsObj.Id;
                                        variableInQuizObj.NumberOfUses = variable.NumberOfUses;
                                        variableInQuizObj.VariableId = variable.VariableId;
                                        UOWObj.VariableInQuizRepository.Insert(variableInQuizObj);
                                        UOWObj.Save();
                                    }

                                    #endregion                            

                                    #region insert in Quiz Branching Logic

                                    var branchingLogic = quizObj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED).BranchingLogic.ToList();
                                    List<Db.CoordinatesInBranchingLogic> coordinatesInBranchingLogic = UOWObj.CoordinatesInBranchingLogicRepository.Get().ToList();

                                    foreach (var branching in branchingLogic)
                                    {
                                        var newBranching = new Db.BranchingLogic();
                                        if (branching.SourceTypeId == (int)BranchingLogicEnum.ANSWER || branching.SourceTypeId == (int)BranchingLogicEnum.QUESTION)
                                            newBranching.SourceObjectId = mappingLst.FirstOrDefault(a => a.DraftedId == branching.SourceObjectId && a.Type == branching.SourceTypeId).PublishedId;
                                        else if (branching.SourceTypeId == (int)BranchingLogicEnum.QUESTIONNEXT)
                                            newBranching.SourceObjectId = mappingLst.FirstOrDefault(a => a.DraftedId == branching.SourceObjectId && a.Type == (int)BranchingLogicEnum.QUESTION).PublishedId;
                                        else if (branching.SourceTypeId == (int)BranchingLogicEnum.ACTION || branching.SourceTypeId == (int)BranchingLogicEnum.ACTIONNEXT)

                                            newBranching.SourceObjectId = mappingLst.FirstOrDefault(a => a.DraftedId == branching.SourceObjectId && a.Type == ((int)BranchingLogicEnum.ACTION)).PublishedId;
                                        else if (branching.SourceTypeId == (int)BranchingLogicEnum.CONTENT || branching.SourceTypeId == (int)BranchingLogicEnum.CONTENTNEXT)
                                            newBranching.SourceObjectId = mappingLst.FirstOrDefault(a => a.DraftedId == branching.SourceObjectId && a.Type == ((int)BranchingLogicEnum.CONTENT)).PublishedId;
                                        else if (branching.SourceTypeId == (int)BranchingLogicEnum.RESULT || branching.SourceTypeId == (int)BranchingLogicEnum.RESULTNEXT)
                                            newBranching.SourceObjectId = mappingLst.FirstOrDefault(a => a.DraftedId == branching.SourceObjectId && a.Type == ((int)BranchingLogicEnum.RESULT)).PublishedId;
                                        else if (branching.SourceTypeId == (int)BranchingLogicEnum.BADGE || branching.SourceTypeId == (int)BranchingLogicEnum.BADGENEXT)
                                            newBranching.SourceObjectId = mappingLst.FirstOrDefault(a => a.DraftedId == branching.SourceObjectId && a.Type == ((int)BranchingLogicEnum.BADGE)).PublishedId;

                                        else if (branching.SourceTypeId == (int)BranchingLogicEnum.WHATSAPPTEMPLATE || branching.SourceTypeId == (int)BranchingLogicEnum.WHATSAPPTEMPLATEACTION)
                                            newBranching.SourceObjectId = mappingLst.FirstOrDefault(a => a.DraftedId == branching.SourceObjectId && a.Type == branching.SourceTypeId).PublishedId;

                                        if (branching.DestinationTypeId == (int)BranchingLogicEnum.ANSWER || branching.DestinationTypeId == (int)BranchingLogicEnum.QUESTION)
                                            newBranching.DestinationObjectId = mappingLst.FirstOrDefault(a => a.DraftedId == branching.DestinationObjectId && a.Type == branching.DestinationTypeId).PublishedId;
                                        else if (branching.DestinationTypeId == (int)BranchingLogicEnum.QUESTIONNEXT)
                                            newBranching.DestinationObjectId = mappingLst.FirstOrDefault(a => a.DraftedId == branching.DestinationObjectId && a.Type == (int)BranchingLogicEnum.QUESTION).PublishedId;
                                        else if (branching.DestinationTypeId == (int)BranchingLogicEnum.ACTION || branching.DestinationTypeId == (int)BranchingLogicEnum.ACTIONNEXT)
                                            newBranching.DestinationObjectId = mappingLst.FirstOrDefault(a => a.DraftedId == branching.DestinationObjectId && a.Type == ((int)BranchingLogicEnum.ACTION)).PublishedId;
                                        else if (branching.DestinationTypeId == (int)BranchingLogicEnum.CONTENT || branching.DestinationTypeId == (int)BranchingLogicEnum.CONTENTNEXT)
                                            newBranching.DestinationObjectId = mappingLst.FirstOrDefault(a => a.DraftedId == branching.DestinationObjectId && a.Type == ((int)BranchingLogicEnum.CONTENT)).PublishedId;
                                        else if (branching.DestinationTypeId == (int)BranchingLogicEnum.RESULT || branching.DestinationTypeId == (int)BranchingLogicEnum.RESULTNEXT)
                                            newBranching.DestinationObjectId = mappingLst.FirstOrDefault(a => a.DraftedId == branching.DestinationObjectId && a.Type == ((int)BranchingLogicEnum.RESULT)).PublishedId;
                                        else if (branching.DestinationTypeId == (int)BranchingLogicEnum.BADGE || branching.DestinationTypeId == (int)BranchingLogicEnum.BADGENEXT)
                                            newBranching.DestinationObjectId = mappingLst.FirstOrDefault(a => a.DraftedId == branching.DestinationObjectId && a.Type == ((int)BranchingLogicEnum.BADGE)).PublishedId;

                                        else if (branching.DestinationTypeId == (int)BranchingLogicEnum.WHATSAPPTEMPLATE || branching.DestinationTypeId == (int)BranchingLogicEnum.WHATSAPPTEMPLATEACTION)
                                            newBranching.DestinationObjectId = mappingLst.FirstOrDefault(a => a.DraftedId == branching.DestinationObjectId && a.Type == branching.DestinationTypeId).PublishedId;

                                        var branchingObj = new Db.BranchingLogic()
                                        {
                                            QuizId = publishedQuizDetailsObj.Id,
                                            SourceTypeId = branching.SourceTypeId,
                                            SourceObjectId = newBranching.SourceObjectId,
                                            DestinationTypeId = branching.DestinationTypeId,
                                            DestinationObjectId = newBranching.DestinationObjectId,
                                            IsStartingPoint = branching.IsStartingPoint,
                                            IsEndPoint = branching.IsEndPoint
                                        };
                                        UOWObj.BranchingLogicRepository.Insert(branchingObj);

                                        #region insert the coordinates in branchinglogic

                                        var element = coordinatesInBranchingLogic.FirstOrDefault(r => (r.ObjectId == branching.SourceObjectId && r.ObjectTypeId == branching.SourceTypeId) || (r.ObjectId == branching.DestinationObjectId && r.ObjectTypeId == branching.DestinationTypeId));
                                        if (element != null)
                                        {
                                            if (branching.IsStartingPoint)
                                            {
                                                var CoordinatesInBranchingLogic = new Db.CoordinatesInBranchingLogic()
                                                {
                                                    ObjectId = newQuizId,
                                                    ObjectTypeId = (int)BranchingLogicEnum.START,
                                                    XCoordinate = "0",
                                                    YCoordinate = "0",
                                                    CompanyId = quizObj.CompanyId,
                                                    QuizId = newQuizId
                                                };
                                                UOWObj.CoordinatesInBranchingLogicRepository.Insert(CoordinatesInBranchingLogic);
                                            };
                                            var CoordinatesInBranchingLogicObj = new Db.CoordinatesInBranchingLogic()
                                            {
                                                ObjectId = branching.IsStartingPoint ? newBranching.SourceObjectId : (newBranching.DestinationObjectId.HasValue ? newBranching.DestinationObjectId.Value : default(int)),
                                                ObjectTypeId = element.ObjectTypeId,
                                                XCoordinate = element.XCoordinate,
                                                YCoordinate = element.YCoordinate,
                                                //,
                                                CompanyId = quizObj.CompanyId,
                                                QuizId = newQuizId
                                            };

                                            if (!coordinatesInBranchingLogic.Any(r => r.ObjectId == CoordinatesInBranchingLogicObj.ObjectId && r.ObjectTypeId == CoordinatesInBranchingLogicObj.ObjectTypeId))
                                            {
                                                UOWObj.CoordinatesInBranchingLogicRepository.Insert(CoordinatesInBranchingLogicObj);
                                            }

                                            CoordinatesInBranchingLogicObj.Id = coordinatesInBranchingLogic.Count() + 1;

                                            coordinatesInBranchingLogic.Add(CoordinatesInBranchingLogicObj);

                                            #region copy coordinatesInBranchingLogic which is not present in branching logic

                                            if (branching.SourceTypeId == (int)BranchingLogicEnum.ANSWER || branching.SourceTypeId == (int)BranchingLogicEnum.QUESTIONNEXT || branching.SourceTypeId == (int)BranchingLogicEnum.CONTENTNEXT || branching.SourceTypeId == (int)BranchingLogicEnum.RESULTNEXT)
                                            {
                                                var objectId = 0; //object startPoint Id
                                                var objectType = 0; //object startPoint Type

                                                if (branching.SourceTypeId == (int)BranchingLogicEnum.ANSWER)
                                                {
                                                    var obj = publishedQuizDetails.QuestionsInQuiz.FirstOrDefault(r => r.AnswerOptionsInQuizQuestions.Any(a => a.Id == branching.SourceObjectId));
                                                    if (obj != null)
                                                        objectId = obj.Id;
                                                }
                                                else
                                                    objectId = branching.SourceObjectId;

                                                if (branching.SourceTypeId == (int)BranchingLogicEnum.ANSWER || branching.SourceTypeId == (int)BranchingLogicEnum.QUESTIONNEXT)
                                                    objectType = (int)BranchingLogicEnum.QUESTION;
                                                else if (branching.SourceTypeId == (int)BranchingLogicEnum.RESULTNEXT)
                                                    objectType = (int)BranchingLogicEnum.RESULT;
                                                else if (branching.SourceTypeId == (int)BranchingLogicEnum.CONTENTNEXT)
                                                    objectType = (int)BranchingLogicEnum.CONTENT;

                                                if (objectId > 0 && !branchingLogic.Any(r => (r.SourceObjectId == objectId && r.SourceTypeId == objectType) || (r.DestinationObjectId == objectId && r.DestinationTypeId == objectType)))
                                                {
                                                    var coordinate = coordinatesInBranchingLogic.FirstOrDefault(r => r.ObjectId == objectId && r.ObjectTypeId == objectType);
                                                    if (coordinate != null)
                                                    {
                                                        var publishedDetails = mappingLst.FirstOrDefault(a => a.DraftedId == objectId && a.Type == objectType);

                                                        if (publishedDetails != null && !coordinatesInBranchingLogic.Any(r => r.ObjectId == publishedDetails.PublishedId && r.ObjectTypeId == coordinate.ObjectTypeId))
                                                        {
                                                            CoordinatesInBranchingLogicObj = new Db.CoordinatesInBranchingLogic()
                                                            {
                                                                ObjectId = publishedDetails.PublishedId,
                                                                ObjectTypeId = coordinate.ObjectTypeId,
                                                                XCoordinate = coordinate.XCoordinate,
                                                                YCoordinate = coordinate.YCoordinate,
                                                                //,
                                                                //CompanyId = quizObj.CompanyId,
                                                                CompanyId = newCompanyId,
                                                                QuizId = newQuizId
                                                            };

                                                            UOWObj.CoordinatesInBranchingLogicRepository.Insert(CoordinatesInBranchingLogicObj);

                                                            CoordinatesInBranchingLogicObj.Id = coordinatesInBranchingLogic.Count() + 1;

                                                            coordinatesInBranchingLogic.Add(CoordinatesInBranchingLogicObj);
                                                        }
                                                    }
                                                }
                                            }
                                            #endregion
                                        }

                                        #endregion
                                    }
                                    #endregion

                                    #region insert usageType in UsageTypeInQuiz

                                    var usageTypeInQuiz = quizObj.UsageTypeInQuiz.Where(r => r.QuizId == quizObj.Id);

                                    if (usageTypeInQuiz != null && usageTypeInQuiz.Any())
                                    {

                                        foreach (var item in usageTypeInQuiz.ToList())
                                        {
                                            var usageTypeObj = new Db.UsageTypeInQuiz();
                                            usageTypeObj.QuizId = newQuizId;
                                            usageTypeObj.UsageType = item.UsageType;
                                            UOWObj.UsageTypeInQuizRepository.Insert(usageTypeObj);
                                            UOWObj.Save();
                                        }
                                    }

                                    #endregion

                                    #region insert into UserPermissionsInQuiz

                                    if (quizObj.QuizType == (int)QuizTypeEnum.AssessmentTemplate || quizObj.QuizType == (int)QuizTypeEnum.ScoreTemplate || quizObj.QuizType == (int)QuizTypeEnum.PersonalityTemplate)
                                    {

                                        var userPermissionsObj = new Db.UserPermissionsInQuiz()
                                        {
                                            QuizId = newQuizId,
                                            UserTypeId = (int)UserTypeEnum.Lead
                                        };
                                        UOWObj.UserPermissionsInQuizRepository.Insert(userPermissionsObj);
                                        UOWObj.Save();
                                    }
                                    else
                                    {
                                        foreach (var userType in quizObj.UserPermissionsInQuiz.ToList())
                                        {
                                            if (userType.UserTypeId == (int)UserTypeEnum.Lead || userType.UserTypeId == (int)UserTypeEnum.Recruiter )
                                            {
                                                var userPermissionsObj = new Db.UserPermissionsInQuiz()
                                                {
                                                    QuizId = newQuizId,
                                                    UserTypeId = userType.UserTypeId
                                                };
                                                UOWObj.UserPermissionsInQuizRepository.Insert(userPermissionsObj);
                                                UOWObj.Save();
                                            }
                                        };
                                    }

                                    #endregion

                                }
                                UOWObj.Save();
                                transaction.Complete();
                            }
                        }
                        else
                        {
                            Status = ResultEnum.Error;
                            ErrorMessage = "Quiz not found for the QuizId " + duplicateQuizRequest.CurrentQuizId;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
            return newQuizId;
        }
    }
}