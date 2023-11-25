using Core.Common.Caching;
using Newtonsoft.Json;
using QuizApp.Helpers;
using QuizApp.RepositoryPattern;
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
    public partial class QuizCoverService : IQuizCoverService
    {
        private ResultEnum status = ResultEnum.Ok;
        private string errormessage = string.Empty;
        public ResultEnum Status { get { return status; } set { status = value; } }
        public string ErrorMessage { get { return errormessage; } set { errormessage = value; } }

        private readonly IQuizVariablesService _quizVariablesService;
        public QuizCoverService(IQuizVariablesService quizVariablesService)
        {
            _quizVariablesService = quizVariablesService;
        }

        public QuizCover GetQuizCoverDetails(int QuizId)
        {
            QuizCover quizCoverObj = null;
            int quizDetailId = 0;
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var obj = UOWObj.QuizRepository.GetByID(QuizId);

                    if (obj != null)
                    {
                        var quizDetails = obj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED);

                        if (quizDetails != null)
                        {
                            quizCoverObj = new QuizCover();
                            quizDetailId = quizDetails.Id;
                            quizCoverObj.QuizId = QuizId;
                            quizCoverObj.QuizTitle = quizDetails.QuizTitle;
                            quizCoverObj.QuizType = obj.QuizType;
                            quizCoverObj.QuizCoverTitle = quizDetails.QuizCoverTitle;
                            quizCoverObj.ShowQuizCoverTitle = quizDetails.ShowQuizCoverTitle;
                            quizCoverObj.QuizCoverImage = quizDetails.QuizCoverImage;
                            quizCoverObj.ShowQuizCoverImage = quizDetails.ShowQuizCoverImage;
                            quizCoverObj.EnableMediaFile = quizDetails.EnableMediaFile;
                            quizCoverObj.PublicIdForQuizCover = quizDetails.PublicId;
                            quizCoverObj.QuizCoverImgXCoordinate = quizDetails.QuizCoverImgXCoordinate;
                            quizCoverObj.QuizCoverImgYCoordinate = quizDetails.QuizCoverImgYCoordinate;
                            quizCoverObj.QuizCoverImgHeight = quizDetails.QuizCoverImgHeight;
                            quizCoverObj.QuizCoverImgWidth = quizDetails.QuizCoverImgWidth;
                            quizCoverObj.QuizCoverImgAttributionLabel = quizDetails.QuizCoverImgAttributionLabel;
                            quizCoverObj.QuizCoverImgAltTag = quizDetails.QuizCoverImgAltTag;
                            quizCoverObj.QuizDescription = quizDetails.QuizDescription;
                            quizCoverObj.ShowDescription = quizDetails.ShowDescription;
                            quizCoverObj.QuizStartButtonText = quizDetails.StartButtonText;
                            quizCoverObj.AutoPlay = quizDetails.AutoPlay;
                            quizCoverObj.SecondsToApply = quizDetails.SecondsToApply;
                            quizCoverObj.VideoFrameEnabled = quizDetails.VideoFrameEnabled;
                            quizCoverObj.EnableNextButton = quizDetails.EnableNextButton;
                            quizCoverObj.DisplayOrderForTitleImage = quizDetails.DisplayOrderForTitleImage;
                            quizCoverObj.DisplayOrderForTitle = quizDetails.DisplayOrderForTitle;
                            quizCoverObj.DisplayOrderForDescription = quizDetails.DisplayOrderForDescription;
                            quizCoverObj.DisplayOrderForNextButton = quizDetails.DisplayOrderForNextButton;
                          
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz not found for the QuizId " + QuizId;
                    }
                }
                if (quizCoverObj != null)
                {
                    quizCoverObj.MsgVariables = _quizVariablesService.GetQuizVariables(quizDetailId, quizDetailId, (int)QuizVariableObjectTypes.COVER);
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
            return quizCoverObj;
        }

        public void UpdateQuizCoverDetails(QuizCover QuizCoverObj, int BusinessUserId, int CompanyId)
        {
            try
            {
                int quizDetailId = 0;
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    if (!string.IsNullOrEmpty(QuizCoverObj.QuizCoverTitle))
                    {
                        var obj = UOWObj.QuizRepository.GetByID(QuizCoverObj.QuizId);

                        if (obj != null)
                        {
                            var quizDetails = obj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED);

                            if (quizDetails != null)
                            {
                                quizDetailId = quizDetails.Id;
                                SaveDynamicVariable(quizDetails.QuizTitle, QuizCoverObj.QuizTitle, quizDetails.Id);
                                SaveDynamicVariable(quizDetails.QuizCoverTitle, QuizCoverObj.QuizCoverTitle, quizDetails.Id);
                                SaveDynamicVariable(quizDetails.QuizDescription, QuizCoverObj.QuizDescription, quizDetails.Id);

                                quizDetails.QuizTitle = QuizCoverObj.QuizTitle;
                                quizDetails.QuizCoverTitle = StringExtension.DecodeHtml(QuizCoverObj.QuizCoverTitle);
                                quizDetails.ShowQuizCoverTitle = QuizCoverObj.ShowQuizCoverTitle;
                                quizDetails.ShowQuizCoverImage = QuizCoverObj.ShowQuizCoverImage;
                                quizDetails.QuizDescription = QuizCoverObj.QuizDescription;
                                quizDetails.ShowDescription = QuizCoverObj.ShowDescription;
                                quizDetails.StartButtonText = QuizCoverObj.QuizStartButtonText;
                                quizDetails.EnableMediaFile = QuizCoverObj.EnableMediaFile;
                                quizDetails.AutoPlay = QuizCoverObj.AutoPlay;
                                quizDetails.SecondsToApply = QuizCoverObj.SecondsToApply;
                                quizDetails.VideoFrameEnabled = QuizCoverObj.VideoFrameEnabled;
                                quizDetails.EnableNextButton = QuizCoverObj.EnableNextButton;
                                quizDetails.DisplayOrderForTitle = QuizCoverObj.DisplayOrderForTitle;
                                quizDetails.DisplayOrderForTitleImage = QuizCoverObj.DisplayOrderForTitleImage;
                                quizDetails.DisplayOrderForDescription = QuizCoverObj.DisplayOrderForDescription;
                                quizDetails.DisplayOrderForNextButton = QuizCoverObj.DisplayOrderForNextButton;

                                quizDetails.LastUpdatedBy = BusinessUserId;
                                quizDetails.LastUpdatedOn = DateTime.UtcNow;

                                obj.State = (int)QuizStateEnum.DRAFTED;

                                UOWObj.QuizRepository.Update(obj);
                                UOWObj.Save();

                            }
                        }
                        else
                        {
                            Status = ResultEnum.Error;
                            ErrorMessage = "Quiz not found for the QuizId " + QuizCoverObj.QuizId;
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz cover title is required";
                    }
                }

                if (QuizCoverObj != null && QuizCoverObj.MsgVariables != null && QuizCoverObj.MsgVariables.Any())
                {
                    QuizVariableModel quizVariable = new QuizVariableModel();

                    quizVariable.Variables = String.Join(",", QuizCoverObj.MsgVariables.ToList());
                    quizVariable.QuizDetailsId = quizDetailId;
                    quizVariable.ObjectId = quizDetailId;
                    quizVariable.ObjectTypes = (int)QuizVariableObjectTypes.COVER;

                    _quizVariablesService.AddQuizVariables(quizVariable);
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public void UpdateQuizCoverImage(QuizCover QuizCoverObj, int BusinessUserId, int CompanyId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {

                    var obj = UOWObj.QuizRepository.GetByID(QuizCoverObj.QuizId);

                    if (obj != null)
                    {
                        var quizDetails = obj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED);

                        if (quizDetails != null)
                        {
                            quizDetails.QuizCoverImage = QuizCoverObj.QuizCoverImage;
                            quizDetails.PublicId = QuizCoverObj.PublicIdForQuizCover;
                            quizDetails.QuizCoverImgXCoordinate = QuizCoverObj.QuizCoverImgXCoordinate;
                            quizDetails.QuizCoverImgYCoordinate = QuizCoverObj.QuizCoverImgYCoordinate;
                            quizDetails.QuizCoverImgHeight = QuizCoverObj.QuizCoverImgHeight;
                            quizDetails.QuizCoverImgWidth = QuizCoverObj.QuizCoverImgWidth;
                            quizDetails.QuizCoverImgAttributionLabel = QuizCoverObj.QuizCoverImgAttributionLabel;
                            quizDetails.QuizCoverImgAltTag = QuizCoverObj.QuizCoverImgAltTag;

                            quizDetails.LastUpdatedBy = BusinessUserId;
                            quizDetails.LastUpdatedOn = DateTime.UtcNow;

                            obj.State = (int)QuizStateEnum.DRAFTED;

                            UOWObj.QuizRepository.Update(obj);
                            UOWObj.Save();

                            
                            
                            
                            
                            
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz not found for the QuizId " + QuizCoverObj.QuizId;
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
        public void UpdateCoverSettingtInQuiz(QuizCover CoverObj, int BusinessUserId, int CompanyId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizCoverObj = UOWObj.QuizDetailsRepository.Get(r => r.ParentQuizId == CoverObj.QuizId && r.Status == (int)StatusEnum.Active).FirstOrDefault();

                    if (quizCoverObj != null)
                    {
                        var currentDate = DateTime.UtcNow;

                        quizCoverObj.AutoPlay = CoverObj.AutoPlay;
                        quizCoverObj.LastUpdatedBy = BusinessUserId;
                        quizCoverObj.LastUpdatedOn = currentDate;

                        UOWObj.QuizDetailsRepository.Update(quizCoverObj);
                        UOWObj.Save();

                        
                        
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Cover not found for the QuizId " + CoverObj.QuizId;
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

    }
}