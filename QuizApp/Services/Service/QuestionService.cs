using Core.Common.Caching;
using QuizApp.Helpers;
using QuizApp.RepositoryPattern;
using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace QuizApp.Services.Service {
    public class QuestionService : IQuestionService
    {
        private ResultEnum status = ResultEnum.Ok;
        private string errormessage = string.Empty;
        public ResultEnum Status { get { return status; } set { status = value; } }
        public string ErrorMessage { get { return errormessage; } set { errormessage = value; } }
        public void UpdateAnswerType(int QuestionId, int AnswerType, int BusinessUserId, int CompanyId, int? answerStructureType, bool isWhatsappEnable = false, bool isMultiRating = false)
        {

            if (isWhatsappEnable)
            {
                UpdateAnswerTypeWhatsApp(QuestionId, AnswerType, BusinessUserId, CompanyId, answerStructureType, true, isMultiRating);
                return;
            }

            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var existingQuestion = UOWObj.QuestionsInQuizRepository.GetByID(QuestionId);


                    if (existingQuestion != null)
                    {
                        var quiz = existingQuestion.QuizDetails.Quiz;

                        if (existingQuestion.AnswerType != AnswerType)
                        {
                            var currentDate = DateTime.UtcNow;

                            var branchingLogic = UOWObj.BranchingLogicRepository.Get(r => r.SourceObjectId == QuestionId && r.SourceTypeId == (int)BranchingLogicEnum.QUESTIONNEXT).FirstOrDefault();

                            if (branchingLogic != null)
                            {
                                UOWObj.BranchingLogicRepository.Delete(branchingLogic);
                            }


                            //other(not multiple) to sigle
                            //other(not sigle) to multiple
                            //other to DrivingLicense,LookingforJobs, FullAddress
                            if ((existingQuestion.AnswerType != (int)AnswerTypeEnum.Multiple && AnswerType == (int)AnswerTypeEnum.Single) || (existingQuestion.AnswerType != (int)AnswerTypeEnum.Single && AnswerType == (int)AnswerTypeEnum.Multiple) || AnswerType == (int)AnswerTypeEnum.DrivingLicense || AnswerType == (int)AnswerTypeEnum.LookingforJobs || AnswerType == (int)AnswerTypeEnum.FullAddress || (AnswerType == (int)AnswerTypeEnum.RatingEmoji && isMultiRating) || (AnswerType == (int)AnswerTypeEnum.RatingStarts && isMultiRating) || (AnswerType == (int)AnswerTypeEnum.Availability) || (AnswerType == (int)AnswerTypeEnum.NPS && isMultiRating))
                            {
                                #region update answer type  to other

                                foreach (var obj in existingQuestion.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active))
                                    RemoveAnswer(obj.Id, BusinessUserId, CompanyId, AnswerType, UOWObj);

                                existingQuestion.EnableComment = false;

                                if (AnswerType == (int)AnswerTypeEnum.DrivingLicense)
                                {
                                    #region to add answer option for driving license

                                    existingQuestion.Question = "Your driving license";
                                    existingQuestion.RevealCorrectAnswer = false;

                                    string[] drivingLicenseOptions = new string[] { "A", "B", "BE", "C", "CE", "D", "THE", "G", "No driving license" };

                                    foreach (var obj in drivingLicenseOptions)
                                    {
                                        var answerObj = new Db.AnswerOptionsInQuizQuestions();
                                        answerObj.QuestionId = QuestionId;
                                        answerObj.QuizId = existingQuestion.QuizId;
                                        answerObj.Option = obj;
                                        answerObj.OptionImage = string.Empty;
                                        answerObj.PublicId = string.Empty;
                                        answerObj.LastUpdatedBy = BusinessUserId;
                                        answerObj.IsCorrectAnswer = true;
                                        answerObj.IsCorrectForMultipleAnswer = false;
                                        if (existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                            answerObj.AssociatedScore = default(int);
                                        answerObj.LastUpdatedOn = currentDate;
                                        answerObj.DisplayOrder = existingQuestion.AnswerOptionsInQuizQuestions.Count(a => a.Status == (int)StatusEnum.Active) + 1;
                                        answerObj.Status = (int)StatusEnum.Active;
                                        answerObj.State = (int)QuizStateEnum.DRAFTED;
                                        answerObj.IsReadOnly = true;

                                        UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);
                                    }

                                    #endregion
                                }
                                else if (AnswerType == (int)AnswerTypeEnum.LookingforJobs)
                                {
                                    #region to add answer option for LookingforJobs

                                    existingQuestion.Question = "Are you still looking for jobs ?";
                                    existingQuestion.RevealCorrectAnswer = false;

                                    string[] LookingforJobsOptions = new string[] { "Yes", "No", "No, but open to good suggestions" };

                                    foreach (var obj in LookingforJobsOptions)
                                    {
                                        var answerObj = new Db.AnswerOptionsInQuizQuestions();
                                        answerObj.QuestionId = QuestionId;
                                        answerObj.QuizId = existingQuestion.QuizId;
                                        answerObj.Option = obj;
                                        answerObj.OptionImage = string.Empty;
                                        answerObj.PublicId = string.Empty;
                                        answerObj.LastUpdatedBy = BusinessUserId;
                                        answerObj.IsCorrectAnswer = true;
                                        answerObj.IsCorrectForMultipleAnswer = false;
                                        if (existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                            answerObj.AssociatedScore = default(int);
                                        answerObj.LastUpdatedOn = currentDate;
                                        answerObj.DisplayOrder = existingQuestion.AnswerOptionsInQuizQuestions.Count(a => a.Status == (int)StatusEnum.Active) + 1;
                                        answerObj.Status = (int)StatusEnum.Active;
                                        answerObj.State = (int)QuizStateEnum.DRAFTED;
                                        answerObj.IsReadOnly = true;

                                        UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);
                                    }

                                    #endregion
                                }
                                else if (AnswerType == (int)AnswerTypeEnum.FullAddress)
                                {
                                    #region to add answer option for FullAddress

                                    existingQuestion.Question = "Your full address";
                                    existingQuestion.RevealCorrectAnswer = false;

                                    string[] fullAddressOptions = new string[] { "Post Code", "House Number" };

                                    foreach (var obj in fullAddressOptions)
                                    {
                                        var answerObj = new Db.AnswerOptionsInQuizQuestions();
                                        answerObj.QuestionId = QuestionId;
                                        answerObj.QuizId = existingQuestion.QuizId;
                                        answerObj.Option = obj;
                                        answerObj.OptionImage = string.Empty;
                                        answerObj.PublicId = string.Empty;
                                        answerObj.LastUpdatedBy = BusinessUserId;
                                        answerObj.IsCorrectAnswer = true;
                                        answerObj.IsCorrectForMultipleAnswer = false;
                                        if (existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                            answerObj.AssociatedScore = default(int);
                                        answerObj.LastUpdatedOn = currentDate;
                                        answerObj.DisplayOrder = existingQuestion.AnswerOptionsInQuizQuestions.Count(a => a.Status == (int)StatusEnum.Active) + 1;
                                        answerObj.Status = (int)StatusEnum.Active;
                                        answerObj.State = (int)QuizStateEnum.DRAFTED;
                                        answerObj.IsReadOnly = false;

                                        UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);
                                    }

                                    #endregion
                                }

                                //else if(AnswerType == (int)AnswerTypeEnum.Availability)
                                //{
                                //    existingQuestion.Question = "Available from date";
                                //}

                                else if (AnswerType == (int)AnswerTypeEnum.RatingEmoji || AnswerType == (int)AnswerTypeEnum.RatingStarts)
                                {
                                    #region to add answer option for RatingEmoji & RatingStarts

                                    if (isMultiRating)
                                    {
                                        existingQuestion.Question = "Are you happy with the way the recruiter contacted you?";
                                        string[] MultiRating = new string[] { "OptionTextforRatingOne", "OptionTextforRatingTwo", "OptionTextforRatingThree", "OptionTextforRatingFour", "OptionTextforRatingFive" };

                                        foreach (var obj in MultiRating)
                                        {
                                            var answerObj = new Db.AnswerOptionsInQuizQuestions();
                                            answerObj.QuestionId = QuestionId;
                                            answerObj.QuizId = existingQuestion.QuizId;
                                            answerObj.Option = null;
                                            answerObj.OptionImage = string.Empty;
                                            answerObj.PublicId = string.Empty;
                                            answerObj.LastUpdatedBy = BusinessUserId;
                                            if (existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                                answerObj.AssociatedScore = default(int);
                                            answerObj.LastUpdatedOn = currentDate;
                                            answerObj.DisplayOrder = existingQuestion.AnswerOptionsInQuizQuestions.Count(a => a.Status == (int)StatusEnum.Active) + 1;
                                            answerObj.Status = (int)StatusEnum.Active;
                                            answerObj.State = (int)QuizStateEnum.DRAFTED;
                                            answerObj.IsReadOnly = false;

                                            UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);
                                        }
                                    }

                                    #endregion
                                }
                                else if (AnswerType == (int)AnswerTypeEnum.Availability)
                                {
                                    var answerObj = new Db.AnswerOptionsInQuizQuestions();
                                    existingQuestion.Question = "Available from date";
                                    answerObj.QuestionId = existingQuestion.Id;
                                    answerObj.QuizId = existingQuestion.QuizId;
                                    answerObj.Option = "";
                                    answerObj.OptionImage = string.Empty;
                                    answerObj.PublicId = string.Empty;
                                    answerObj.LastUpdatedBy = BusinessUserId;
                                    answerObj.LastUpdatedOn = currentDate;

                                    if (quiz.QuizType == (int)QuizTypeEnum.Score || quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                    {
                                        answerObj.AssociatedScore = default(int);
                                        answerObj.IsCorrectAnswer = false;
                                    }
                                    else if (quiz.QuizType == (int)QuizTypeEnum.Personality || quiz.QuizType == (int)QuizTypeEnum.PersonalityTemplate)
                                        answerObj.IsCorrectAnswer = false;
                                    else
                                        answerObj.IsCorrectAnswer = true;

                                    answerObj.DisplayOrder = 1;
                                    answerObj.Status = (int)StatusEnum.Active;
                                    answerObj.State = (int)QuizStateEnum.DRAFTED;
                                    answerObj.AutoPlay = true;
                                    answerObj.SecondsToApply = "0";
                                    answerObj.VideoFrameEnabled = false;

                                    UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);

                                    answerObj = new Db.AnswerOptionsInQuizQuestions();

                                    answerObj.QuestionId = existingQuestion.Id;
                                    answerObj.QuizId = existingQuestion.QuizId;
                                    answerObj.Option = "";
                                    answerObj.OptionImage = string.Empty;
                                    answerObj.PublicId = string.Empty;
                                    answerObj.LastUpdatedBy = BusinessUserId;
                                    answerObj.LastUpdatedOn = currentDate;

                                    if (quiz.QuizType == (int)QuizTypeEnum.Score || quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                    {
                                        answerObj.AssociatedScore = default(int);
                                        answerObj.IsCorrectAnswer = false;
                                    }
                                    else
                                        answerObj.IsCorrectAnswer = false;

                                    answerObj.DisplayOrder = 2;
                                    answerObj.Status = (int)StatusEnum.Active;
                                    answerObj.State = (int)QuizStateEnum.DRAFTED;
                                    answerObj.AutoPlay = true;
                                    answerObj.SecondsToApply = "0";
                                    answerObj.VideoFrameEnabled = false;

                                    UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);

                                    answerObj = new Db.AnswerOptionsInQuizQuestions();

                                    answerObj.QuestionId = existingQuestion.Id;
                                    answerObj.QuizId = existingQuestion.QuizId;
                                    answerObj.Option = "";
                                    answerObj.OptionImage = string.Empty;
                                    answerObj.PublicId = string.Empty;
                                    answerObj.LastUpdatedBy = BusinessUserId;
                                    answerObj.LastUpdatedOn = currentDate;

                                    if (quiz.QuizType == (int)QuizTypeEnum.Score || quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                    {
                                        answerObj.AssociatedScore = default(int);
                                        answerObj.IsCorrectAnswer = false;
                                    }
                                    else if (quiz.QuizType == (int)QuizTypeEnum.Personality || quiz.QuizType == (int)QuizTypeEnum.PersonalityTemplate)
                                        answerObj.IsCorrectAnswer = false;
                                    else
                                        answerObj.IsCorrectAnswer = true;

                                    answerObj.DisplayOrder = 3;
                                    answerObj.Status = (int)StatusEnum.Active;
                                    answerObj.State = (int)QuizStateEnum.DRAFTED;
                                    answerObj.AutoPlay = true;
                                    answerObj.SecondsToApply = "0";
                                    answerObj.VideoFrameEnabled = false;

                                    UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);
                                    UOWObj.Save();
                                }

                                else if(isMultiRating && AnswerType == (int)AnswerTypeEnum.NPS) {
                                    existingQuestion.Question = "NPS";
                                    string[] multiRatingNps = new string[] { "OptionTextforDetractorhigh1", "OptionTextforDetractorhigh2", "OptionTextforDetractorhigh3", "OptionTextforDetractorhigh4", "OptionTextforDetractorlow5",
                                        "OptionTextforDetractorlow6", "OptionTextforNeutral7", "OptionTextforNeutral8", "OptionTextforPromoter9", "OptionTextforPromoter10" };
                                    foreach (var item in multiRatingNps) {
                                        var answerObj = new Db.AnswerOptionsInQuizQuestions();
                                        answerObj.QuestionId = QuestionId;
                                        answerObj.QuizId = existingQuestion.QuizId;
                                        answerObj.Option = null;
                                        answerObj.OptionImage = string.Empty;
                                        answerObj.PublicId = string.Empty;
                                        answerObj.LastUpdatedBy = BusinessUserId;
                                        if (existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                            answerObj.AssociatedScore = default(int);
                                        answerObj.LastUpdatedOn = currentDate;
                                        answerObj.DisplayOrder = existingQuestion.AnswerOptionsInQuizQuestions.Count(a => a.Status == (int)StatusEnum.Active) + 1;
                                        answerObj.Status = (int)StatusEnum.Active;
                                        answerObj.State = (int)QuizStateEnum.DRAFTED;
                                        answerObj.IsReadOnly = false;

                                        UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);
                                    }
								}
                                else
                                {
                                    #region to add answer option for Single and Multiple type

                                    var answerObj = new Db.AnswerOptionsInQuizQuestions();
                                    if (existingQuestion.AnswerType == (int)AnswerTypeEnum.Availability)
                                        existingQuestion.Question = "Question" + (existingQuestion.QuizDetails.QuestionsInQuiz.Count(a => a.Status == (int)StatusEnum.Active && a.Type == (int)BranchingLogicEnum.QUESTION)).ToString();

                                    if (existingQuestion.AnswerType == (int)AnswerTypeEnum.DOB || existingQuestion.AnswerType == (int)AnswerTypeEnum.PostCode || existingQuestion.AnswerType == (int)AnswerTypeEnum.DrivingLicense || existingQuestion.AnswerType == (int)AnswerTypeEnum.LookingforJobs || existingQuestion.AnswerType == (int)AnswerTypeEnum.FullAddress || existingQuestion.AnswerType == (int)AnswerTypeEnum.RatingEmoji || existingQuestion.AnswerType == (int)AnswerTypeEnum.RatingStarts
                                        || existingQuestion.AnswerType == (int)AnswerTypeEnum.FirstName || existingQuestion.AnswerType == (int)AnswerTypeEnum.LastName || existingQuestion.AnswerType == (int)AnswerTypeEnum.Email || existingQuestion.AnswerType == (int)AnswerTypeEnum.PhoneNumber || existingQuestion.AnswerType == (int)AnswerTypeEnum.DatePicker || existingQuestion.AnswerType == (int)AnswerTypeEnum.NPS)
                                        existingQuestion.Question = "Question" + (existingQuestion.QuizDetails.QuestionsInQuiz.Count(a => a.Status == (int)StatusEnum.Active && a.Type == (int)BranchingLogicEnum.QUESTION)).ToString();
                                    answerObj.QuestionId = QuestionId;
                                    answerObj.QuizId = existingQuestion.QuizId;
                                    answerObj.Option = "Answer 1";
                                    answerObj.OptionImage = string.Empty;
                                    answerObj.PublicId = string.Empty;
                                    answerObj.LastUpdatedBy = BusinessUserId;
                                    answerObj.IsCorrectAnswer = true;
                                    answerObj.IsCorrectForMultipleAnswer = false;
                                    if (existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                        answerObj.AssociatedScore = default(int);
                                    answerObj.LastUpdatedOn = currentDate;
                                    answerObj.DisplayOrder = 1;
                                    answerObj.Status = (int)StatusEnum.Active;
                                    answerObj.State = (int)QuizStateEnum.DRAFTED;
                                    answerObj.IsReadOnly = false;

                                    UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);

                                    answerObj = new Db.AnswerOptionsInQuizQuestions();

                                    answerObj.QuestionId = QuestionId;
                                    answerObj.QuizId = existingQuestion.QuizId;
                                    answerObj.Option = "Answer 2";
                                    answerObj.OptionImage = string.Empty;
                                    answerObj.PublicId = string.Empty;
                                    answerObj.LastUpdatedBy = BusinessUserId;
                                    answerObj.LastUpdatedOn = currentDate;
                                    answerObj.DisplayOrder = 2;
                                    answerObj.Status = (int)StatusEnum.Active;
                                    answerObj.State = (int)QuizStateEnum.DRAFTED;
                                    answerObj.IsCorrectAnswer = false;
                                    answerObj.IsCorrectForMultipleAnswer = false;
                                    answerObj.IsReadOnly = false;
                                    if (existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                        answerObj.AssociatedScore = default(int);

                                    UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);

                                    if (AnswerType == (int)AnswerTypeEnum.Single)
                                    {
                                        answerObj = new Db.AnswerOptionsInQuizQuestions();

                                        answerObj.QuestionId = QuestionId;
                                        answerObj.QuizId = existingQuestion.QuizId;
                                        answerObj.Option = "Unanswered";
                                        answerObj.OptionImage = string.Empty;
                                        answerObj.PublicId = string.Empty;
                                        answerObj.LastUpdatedBy = BusinessUserId;
                                        answerObj.LastUpdatedOn = currentDate;
                                        answerObj.DisplayOrder = 0;
                                        answerObj.Status = (int)StatusEnum.Active;
                                        answerObj.State = (int)QuizStateEnum.DRAFTED;
                                        answerObj.IsCorrectAnswer = false;
                                        answerObj.IsCorrectForMultipleAnswer = false;
                                        answerObj.IsReadOnly = false;
                                        answerObj.IsUnansweredType = true;
                                        if (existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                            answerObj.AssociatedScore = default(int);

                                        UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);
                                    }

                                    #endregion
                                }

                                #endregion
                            }


                            //sigle, multiple, DrivingLicense,LookingforJobs, FullAddress to Short, Long, DOB, PostCode, NPS, RatingEmoji, RatingStarts
                            else if ((existingQuestion.AnswerType == (int)AnswerTypeEnum.Single || existingQuestion.AnswerType == (int)AnswerTypeEnum.Multiple || existingQuestion.AnswerType == (int)AnswerTypeEnum.DrivingLicense || existingQuestion.AnswerType == (int)AnswerTypeEnum.LookingforJobs || existingQuestion.AnswerType == (int)AnswerTypeEnum.FullAddress) && (AnswerType == (int)AnswerTypeEnum.Short || AnswerType == (int)AnswerTypeEnum.Long || AnswerType == (int)AnswerTypeEnum.DOB || AnswerType == (int)AnswerTypeEnum.PostCode || AnswerType == (int)AnswerTypeEnum.NPS || AnswerType == (int)AnswerTypeEnum.RatingEmoji || AnswerType == (int)AnswerTypeEnum.RatingStarts || AnswerType == (int)AnswerTypeEnum.Availability
                                || AnswerType == (int)AnswerTypeEnum.FirstName || AnswerType == (int)AnswerTypeEnum.LastName || AnswerType == (int)AnswerTypeEnum.Email || AnswerType == (int)AnswerTypeEnum.PhoneNumber || AnswerType == (int)AnswerTypeEnum.DatePicker))
                            {
                                #region update answer type other to text

                                foreach (var obj in existingQuestion.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active))
                                    RemoveAnswer(obj.Id, BusinessUserId, CompanyId, AnswerType, null);

                                var answerObj = new Db.AnswerOptionsInQuizQuestions();
                                existingQuestion.RevealCorrectAnswer = false;
                                answerObj.QuestionId = QuestionId;
                                answerObj.QuizId = existingQuestion.QuizId;
                                switch (AnswerType)
                                {
                                    case (int)AnswerTypeEnum.Short:
                                        answerObj.Option = "Short answer text...";
                                        if (existingQuestion.AnswerType == (int)AnswerTypeEnum.DrivingLicense || existingQuestion.AnswerType == (int)AnswerTypeEnum.LookingforJobs)
                                            existingQuestion.Question = "Question" + (existingQuestion.QuizDetails.QuestionsInQuiz.Count(a => a.Status == (int)StatusEnum.Active && a.Type == (int)BranchingLogicEnum.QUESTION)).ToString();
                                        answerObj.IsReadOnly = false;
                                        break;
                                    case (int)AnswerTypeEnum.Long:
                                        answerObj.Option = "Long answer text...";
                                        if (existingQuestion.AnswerType == (int)AnswerTypeEnum.DrivingLicense || existingQuestion.AnswerType == (int)AnswerTypeEnum.LookingforJobs)
                                            existingQuestion.Question = "Question" + (existingQuestion.QuizDetails.QuestionsInQuiz.Count(a => a.Status == (int)StatusEnum.Active && a.Type == (int)BranchingLogicEnum.QUESTION)).ToString();
                                        answerObj.IsReadOnly = false;
                                        break;
                                    case (int)AnswerTypeEnum.DOB:
                                        answerObj.Option = null;
                                        answerObj.IsReadOnly = false;
                                        existingQuestion.Question = "Your date of birth";
                                        break;
                                    case (int)AnswerTypeEnum.PostCode:
                                        answerObj.Option = null;
                                        answerObj.IsReadOnly = false;
                                        existingQuestion.Question = "Your post code";
                                        break;
                                    case (int)AnswerTypeEnum.NPS:
                                        answerObj.Option = null;
                                        answerObj.IsReadOnly = false;
                                        existingQuestion.IsMultiRating =isMultiRating;
                                        existingQuestion.Question = (isMultiRating) ? "NPS": "Question" + (existingQuestion.QuizDetails.QuestionsInQuiz.Count(a => a.Status == (int)StatusEnum.Active && a.Type == (int)BranchingLogicEnum.QUESTION)).ToString();
                                        break;
                                    case (int)AnswerTypeEnum.RatingEmoji:
                                    case (int)AnswerTypeEnum.RatingStarts:

                                        answerObj.Option = null;
                                        answerObj.IsReadOnly = true;
                                        existingQuestion.IsMultiRating = isMultiRating;
                                        existingQuestion.Question = "Are you happy with the way the recruiter contacted you?";
                                        break;
                                    case (int)AnswerTypeEnum.Availability:
                                        existingQuestion.Question = "Available from date";                                       
                                        break;
                                    case (int)AnswerTypeEnum.FirstName:
                                        answerObj.Option = null;
                                        answerObj.IsReadOnly = false;
                                        existingQuestion.Question = "Your first name";
                                        break;
                                    case (int)AnswerTypeEnum.LastName:
                                        answerObj.Option = null;
                                        answerObj.IsReadOnly = false;
                                        existingQuestion.Question = "Your last name";
                                        break;
                                    case (int)AnswerTypeEnum.Email:
                                        answerObj.Option = null;
                                        answerObj.IsReadOnly = false;
                                        existingQuestion.Question = "Your email";
                                        break;
                                    case (int)AnswerTypeEnum.PhoneNumber:
                                        answerObj.Option = null;
                                        answerObj.IsReadOnly = false;
                                        existingQuestion.Question = "Your phone number";
                                        break;
                                    case (int)AnswerTypeEnum.DatePicker:
                                        answerObj.Option = null;
                                        answerObj.IsReadOnly = false;
                                        existingQuestion.Question = "Your date picker";
                                        break;
                                }
                                answerObj.OptionImage = string.Empty;
                                answerObj.PublicId = string.Empty;
                                answerObj.IsCorrectAnswer = (AnswerType == (int)AnswerTypeEnum.NPS || AnswerType == (int)AnswerTypeEnum.RatingEmoji || AnswerType == (int)AnswerTypeEnum.RatingStarts) ? false : true;
                                answerObj.IsCorrectForMultipleAnswer = (AnswerType == (int)AnswerTypeEnum.NPS || AnswerType == (int)AnswerTypeEnum.RatingEmoji || AnswerType == (int)AnswerTypeEnum.RatingStarts) ? false : true;
                                answerObj.LastUpdatedBy = BusinessUserId;
                                answerObj.LastUpdatedOn = currentDate;
                                answerObj.DisplayOrder = 1;
                                answerObj.Status = (int)StatusEnum.Active;
                                answerObj.State = (int)QuizStateEnum.DRAFTED;

                                UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);

                                #endregion
                            }

                            //Short, Long, DOB, PostCode, NPS, RatingEmoji, RatingStarts to Short, Long, DOB, PostCode, NPS, RatingEmoji, RatingStarts,NPSQuestion
                            else if ((existingQuestion.AnswerType == (int)AnswerTypeEnum.Short || existingQuestion.AnswerType == (int)AnswerTypeEnum.Long || existingQuestion.AnswerType == (int)AnswerTypeEnum.DOB || existingQuestion.AnswerType == (int)AnswerTypeEnum.PostCode || existingQuestion.AnswerType == (int)AnswerTypeEnum.NPS || existingQuestion.AnswerType == (int)AnswerTypeEnum.RatingEmoji || existingQuestion.AnswerType == (int)AnswerTypeEnum.RatingStarts || existingQuestion.AnswerType == (int)AnswerTypeEnum.Availability || existingQuestion.AnswerType == (int)AnswerTypeEnum.FirstName
                                || existingQuestion.AnswerType == (int)AnswerTypeEnum.LastName || existingQuestion.AnswerType == (int)AnswerTypeEnum.Email || existingQuestion.AnswerType == (int)AnswerTypeEnum.PhoneNumber || existingQuestion.AnswerType == (int)AnswerTypeEnum.DatePicker)
                                      && (AnswerType == (int)AnswerTypeEnum.Short || AnswerType == (int)AnswerTypeEnum.Long || AnswerType == (int)AnswerTypeEnum.DOB || AnswerType == (int)AnswerTypeEnum.PostCode || AnswerType == (int)AnswerTypeEnum.NPS || AnswerType == (int)AnswerTypeEnum.RatingEmoji || AnswerType == (int)AnswerTypeEnum.RatingStarts || AnswerType == (int)AnswerTypeEnum.Availability || AnswerType == (int)AnswerTypeEnum.FirstName || AnswerType == (int)AnswerTypeEnum.LastName || AnswerType == (int)AnswerTypeEnum.Email || AnswerType == (int)AnswerTypeEnum.PhoneNumber
                                      || AnswerType == (int)AnswerTypeEnum.DatePicker))
                            {
                                #region update answer type text to text

                                var answerObj = existingQuestion.AnswerOptionsInQuizQuestions.FirstOrDefault(r => r.Status == (int)StatusEnum.Active);
                                if (existingQuestion.AnswerType == (int)AnswerTypeEnum.RatingEmoji || existingQuestion.AnswerType == (int)AnswerTypeEnum.RatingStarts || existingQuestion.AnswerType == (int)AnswerTypeEnum.Availability || existingQuestion.AnswerType == (int)AnswerTypeEnum.NPS)
                                {
                                    foreach (var obj in existingQuestion.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active))
                                    {
                                        if (obj.Id != answerObj.Id)
                                        {
                                            RemoveAnswer(obj.Id, BusinessUserId, CompanyId, AnswerType, null);
                                        }
                                    }
                                }

                                existingQuestion.RevealCorrectAnswer = false;

                                switch (AnswerType)
                                {
                                    case (int)AnswerTypeEnum.Short:
                                        answerObj.Option = "Short answer text...";
                                        if (existingQuestion.AnswerType == (int)AnswerTypeEnum.DOB || existingQuestion.AnswerType == (int)AnswerTypeEnum.PostCode || existingQuestion.AnswerType == (int)AnswerTypeEnum.RatingEmoji || existingQuestion.AnswerType == (int)AnswerTypeEnum.RatingStarts || existingQuestion.AnswerType == (int)AnswerTypeEnum.Availability
                                            || existingQuestion.AnswerType == (int)AnswerTypeEnum.FirstName || existingQuestion.AnswerType == (int)AnswerTypeEnum.LastName || existingQuestion.AnswerType == (int)AnswerTypeEnum.Email || existingQuestion.AnswerType == (int)AnswerTypeEnum.PhoneNumber || existingQuestion.AnswerType == (int)AnswerTypeEnum.DatePicker)
                                        {
                                            existingQuestion.Question = "Question" + (existingQuestion.QuizDetails.QuestionsInQuiz.Count(a => a.Status == (int)StatusEnum.Active && a.Type == (int)BranchingLogicEnum.QUESTION)).ToString();
                                            existingQuestion.EnableComment = false;
                                        }
                                        break;
                                    case (int)AnswerTypeEnum.Long:
                                        answerObj.Option = "Long answer text...";
                                        if (existingQuestion.AnswerType == (int)AnswerTypeEnum.DOB || existingQuestion.AnswerType == (int)AnswerTypeEnum.PostCode || existingQuestion.AnswerType == (int)AnswerTypeEnum.RatingEmoji || existingQuestion.AnswerType == (int)AnswerTypeEnum.RatingStarts || existingQuestion.AnswerType == (int)AnswerTypeEnum.Availability) {
                                            existingQuestion.Question = "Question" + (existingQuestion.QuizDetails.QuestionsInQuiz.Count(a => a.Status == (int)StatusEnum.Active && a.Type == (int)BranchingLogicEnum.QUESTION)).ToString();
                                            existingQuestion.EnableComment = false;
                                        }
                                        break;
                                    case (int)AnswerTypeEnum.DOB:
                                        answerObj.Option = null;
                                        existingQuestion.Question = "Your date of birth";
                                        existingQuestion.EnableComment = false;
                                        break;
                                    case (int)AnswerTypeEnum.PostCode:
                                        answerObj.Option = null;
                                        existingQuestion.Question = "Your post code";
                                        existingQuestion.EnableComment = false;
                                        break;
                                    case (int)AnswerTypeEnum.NPS:
                                        answerObj.IsCorrectAnswer = false;
                                        answerObj.IsCorrectForMultipleAnswer = false;
                                        answerObj.Option = null;
                                        existingQuestion.RevealCorrectAnswer = false;
                                        existingQuestion.IsMultiRating = isMultiRating;
                                        existingQuestion.Question = (isMultiRating) ? "NPS": "Question" + (existingQuestion.QuizDetails.QuestionsInQuiz.Count(a => a.Status == (int)StatusEnum.Active && a.Type == (int)BranchingLogicEnum.QUESTION)).ToString();
                                        existingQuestion.EnableComment = false;
                                        break;
                                    case (int)AnswerTypeEnum.RatingEmoji:
                                    case (int)AnswerTypeEnum.RatingStarts:
                                        answerObj.IsCorrectAnswer = false;
                                        answerObj.IsCorrectForMultipleAnswer = false;
                                        answerObj.Option = null;
                                        answerObj.IsReadOnly = true;
                                        existingQuestion.Question = "Are you happy with the way the recruiter contacted you?";
                                        break;

                                    case (int)AnswerTypeEnum.FirstName:
                                        answerObj.Option = null;
                                        existingQuestion.Question = "Your first name";
                                        existingQuestion.EnableComment = false;
                                        break;
                                    case (int)AnswerTypeEnum.LastName:
                                        answerObj.Option = null;
                                        existingQuestion.Question = "Your last name";
                                        existingQuestion.EnableComment = false;
                                        break;
                                    case (int)AnswerTypeEnum.Email:
                                        answerObj.Option = null;
                                        existingQuestion.Question = "Your email";
                                        existingQuestion.EnableComment = false;
                                        break;
                                    case (int)AnswerTypeEnum.PhoneNumber:
                                        answerObj.Option = null;
                                        existingQuestion.Question = "Your phone number";
                                        existingQuestion.EnableComment = false;
                                        break;
                                    case (int)AnswerTypeEnum.DatePicker:
                                        answerObj.Option = null;
                                        existingQuestion.Question = "Your date picker";
                                        existingQuestion.EnableComment = false;
                                        break;
                                }
                                UOWObj.AnswerOptionsInQuizQuestionsRepository.Update(answerObj);

                                #endregion
                            }

                            //Single, LookingforJobs to other
                            if ((existingQuestion.AnswerType == (int)AnswerTypeEnum.Single || existingQuestion.AnswerType == (int)AnswerTypeEnum.LookingforJobs) && (AnswerType != (int)AnswerTypeEnum.Single && AnswerType != (int)AnswerTypeEnum.LookingforJobs))
                            {
                                #region update answer type Single to other

                                var ansIds = existingQuestion.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active).ToList().Select(t => t.Id);
                                var branchingLogicList = UOWObj.BranchingLogicRepository.Get(r => ansIds.Any(s => s == r.SourceObjectId) && r.SourceTypeId == (int)BranchingLogicEnum.ANSWER).ToList();

                                if (branchingLogicList != null)
                                {
                                    foreach (var obj in branchingLogicList)
                                    {
                                        UOWObj.BranchingLogicRepository.Delete(obj);
                                    }
                                }

                                #endregion
                            }


                            if (existingQuestion.AnswerType != (int)AnswerTypeEnum.Single || existingQuestion.AnswerType != (int)AnswerTypeEnum.Multiple)
                            {
                                #region update answer type Single to other

                                var ansIds = existingQuestion.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active).ToList().Select(t => t.Id);
                                if(ansIds != null && ansIds.Any())
                                {
                                    var objectFieldsInAnswerList = UOWObj.ObjectFieldsInAnswerRepository.Get(r => ansIds.Contains(r.AnswerOptionsInQuizQuestionsId)).ToList();

                                    if (objectFieldsInAnswerList != null && objectFieldsInAnswerList.Any())
                                    {
                                        foreach (var obj in objectFieldsInAnswerList)
                                        {
                                            UOWObj.ObjectFieldsInAnswerRepository.Delete(obj);
                                        }
                                    }
                                }
                                #endregion
                            }


                            //other to Short, Long, DOB, PostCode, NPS, RatingEmoji, RatingStarts, firstname, lastname, email, phoneNumber,NPSQuestion
                            if ((existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate) && (existingQuestion.AnswerType == (int)AnswerTypeEnum.Multiple || existingQuestion.AnswerType == (int)AnswerTypeEnum.DrivingLicense) && (AnswerType == (int)AnswerTypeEnum.Single || AnswerType == (int)AnswerTypeEnum.DOB || AnswerType == (int)AnswerTypeEnum.PostCode || AnswerType == (int)AnswerTypeEnum.FullAddress || AnswerType == (int)AnswerTypeEnum.LookingforJobs || AnswerType == (int)AnswerTypeEnum.RatingStarts || AnswerType == (int)AnswerTypeEnum.RatingEmoji || AnswerType == (int)AnswerTypeEnum.Availability
                                 || AnswerType == (int)AnswerTypeEnum.FirstName || AnswerType == (int)AnswerTypeEnum.LastName || AnswerType == (int)AnswerTypeEnum.Email || AnswerType == (int)AnswerTypeEnum.PhoneNumber || AnswerType == (int)AnswerTypeEnum.DatePicker || AnswerType == (int)AnswerTypeEnum.NPS))
                            {
                                existingQuestion.MinAnswer = 1;
                                existingQuestion.MaxAnswer = 1;
                            }

                            if ((existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate) && (AnswerType == (int)AnswerTypeEnum.DrivingLicense || AnswerType == (int)AnswerTypeEnum.Multiple))
                            {
                                existingQuestion.MinAnswer = 0;
                                existingQuestion.MaxAnswer = existingQuestion.AnswerOptionsInQuizQuestions.Count(r => r.Status == (int)StatusEnum.Active);
                            }

                            if (AnswerType != (int)AnswerTypeEnum.Single || AnswerType != (int)AnswerTypeEnum.Multiple || AnswerType != (int)AnswerTypeEnum.Short || AnswerType != (int)AnswerTypeEnum.Long)
                            {
                                existingQuestion.TimerRequired = false;
                                existingQuestion.Time = null;
                            }

                            if (AnswerType != (int)AnswerTypeEnum.Single && AnswerType != (int)AnswerTypeEnum.Multiple)
                            {
                                existingQuestion.ShowAnswerImage = false;
                            }
                            existingQuestion.IsMultiRating = isMultiRating;
                            existingQuestion.AnswerType = AnswerType;
                            existingQuestion.LastUpdatedBy = BusinessUserId;
                            existingQuestion.LastUpdatedOn = currentDate;

                            existingQuestion.QuizDetails.LastUpdatedBy = BusinessUserId;
                            existingQuestion.QuizDetails.LastUpdatedOn = currentDate;

                            existingQuestion.QuizDetails.Quiz.State = (int)QuizStateEnum.DRAFTED;
                            existingQuestion.AutoPlay = ((existingQuestion.AnswerType == (int)AnswerTypeEnum.Single || existingQuestion.AnswerType == (int)AnswerTypeEnum.Multiple) && existingQuestion.AutoPlay == true) ? true : existingQuestion.AutoPlay;


                            UOWObj.QuizRepository.Update(existingQuestion.QuizDetails.Quiz);
                            UOWObj.Save();

                            
                            
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
        }


        public void UpdateAnswerTypeWhatsApp(int QuestionId, int AnswerType, int BusinessUserId, int CompanyId, int? answerStructureType, bool isWhatsappEnable, bool isMultiRating = false)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var existingQuestion = UOWObj.QuestionsInQuizRepository.GetByID(QuestionId);
                    var currentDate = DateTime.UtcNow;

                    if (existingQuestion != null)
                    {
                        var quiz = existingQuestion.QuizDetails.Quiz;

                        if (existingQuestion.AnswerType != AnswerType)
                        {

                            var branchingLogic = UOWObj.BranchingLogicRepository.Get(r => r.SourceObjectId == QuestionId && r.SourceTypeId == (int)BranchingLogicEnum.QUESTIONNEXT).FirstOrDefault();

                            if (branchingLogic != null)
                            {
                                UOWObj.BranchingLogicRepository.Delete(branchingLogic);
                            }

                            //if (isWhatsappEnable)
                            //{
                            //    existingQuestion.AnswerStructureType = answerStructureType;
                            //}

                            if ((AnswerType == (int)AnswerTypeEnum.Single || AnswerType == (int)AnswerTypeEnum.Multiple))
                            {
                                existingQuestion.EnableComment = false;
                                

                                SetSingleMultipleAnswerTYpe(QuestionId, AnswerType, answerStructureType , BusinessUserId, CompanyId, UOWObj, existingQuestion, currentDate);
                            }


                            //other(not multiple) to sigle
                            //other(not sigle) to multiple
                            //other to DrivingLicense,LookingforJobs, FullAddress
                            else if (AnswerType == (int)AnswerTypeEnum.DrivingLicense || AnswerType == (int)AnswerTypeEnum.LookingforJobs || AnswerType == (int)AnswerTypeEnum.FullAddress || AnswerType == (int)AnswerTypeEnum.Availability || (AnswerType == (int)AnswerTypeEnum.RatingEmoji && isMultiRating) || (AnswerType == (int)AnswerTypeEnum.RatingStarts && isMultiRating) || (AnswerType == (int)AnswerTypeEnum.NPS && isMultiRating))
                            {
                                #region update answer type  to other

                                foreach (var obj in existingQuestion.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active))
                                    RemoveAnswer(obj.Id, BusinessUserId, CompanyId, AnswerType, UOWObj);

                                existingQuestion.EnableComment = false;

                                if (AnswerType == (int)AnswerTypeEnum.DrivingLicense)
                                {
                                    #region to add answer option for driving license

                                    existingQuestion.Question = "Your driving license";
                                    existingQuestion.RevealCorrectAnswer = false;

                                    string[] drivingLicenseOptions = new string[] { "A", "B", "BE", "C", "CE", "D", "THE", "G", "No driving license" };

                                    foreach (var obj in drivingLicenseOptions)
                                    {
                                        var answerObj = new Db.AnswerOptionsInQuizQuestions();
                                        answerObj.QuestionId = QuestionId;
                                        answerObj.QuizId = existingQuestion.QuizId;
                                        answerObj.Option = obj;
                                        answerObj.OptionImage = string.Empty;
                                        answerObj.PublicId = string.Empty;
                                        answerObj.LastUpdatedBy = BusinessUserId;
                                        answerObj.IsCorrectAnswer = true;
                                        answerObj.IsCorrectForMultipleAnswer = false;
                                        if (existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                            answerObj.AssociatedScore = default(int);
                                        answerObj.LastUpdatedOn = currentDate;
                                        answerObj.DisplayOrder = existingQuestion.AnswerOptionsInQuizQuestions.Count(a => a.Status == (int)StatusEnum.Active) + 1;
                                        answerObj.Status = (int)StatusEnum.Active;
                                        answerObj.State = (int)QuizStateEnum.DRAFTED;
                                        answerObj.IsReadOnly = true;

                                        UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);
                                    }

                                    #endregion
                                }
                                else if (AnswerType == (int)AnswerTypeEnum.LookingforJobs)
                                {
                                    #region to add answer option for LookingforJobs

                                    existingQuestion.Question = "Are you still looking for jobs ?";
                                    existingQuestion.RevealCorrectAnswer = false;

                                    string[] LookingforJobsOptions = new string[] { "Yes", "No", "No, but open to good suggestions" };

                                    foreach (var obj in LookingforJobsOptions)
                                    {
                                        var answerObj = new Db.AnswerOptionsInQuizQuestions();
                                        answerObj.QuestionId = QuestionId;
                                        answerObj.QuizId = existingQuestion.QuizId;
                                        answerObj.Option = obj;
                                        answerObj.OptionImage = string.Empty;
                                        answerObj.PublicId = string.Empty;
                                        answerObj.LastUpdatedBy = BusinessUserId;
                                        answerObj.IsCorrectAnswer = true;
                                        answerObj.IsCorrectForMultipleAnswer = false;
                                        if (existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                            answerObj.AssociatedScore = default(int);
                                        answerObj.LastUpdatedOn = currentDate;
                                        answerObj.DisplayOrder = existingQuestion.AnswerOptionsInQuizQuestions.Count(a => a.Status == (int)StatusEnum.Active) + 1;
                                        answerObj.Status = (int)StatusEnum.Active;
                                        answerObj.State = (int)QuizStateEnum.DRAFTED;
                                        answerObj.IsReadOnly = true;

                                        UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);
                                    }

                                    #endregion
                                }
                                else if (AnswerType == (int)AnswerTypeEnum.FullAddress)
                                {
                                    #region to add answer option for FullAddress

                                    existingQuestion.Question = "Your full address";
                                    existingQuestion.RevealCorrectAnswer = false;

                                    string[] fullAddressOptions = new string[] { "Post Code", "House Number" };

                                    foreach (var obj in fullAddressOptions)
                                    {
                                        var answerObj = new Db.AnswerOptionsInQuizQuestions();
                                        answerObj.QuestionId = QuestionId;
                                        answerObj.QuizId = existingQuestion.QuizId;
                                        answerObj.Option = obj;
                                        answerObj.OptionImage = string.Empty;
                                        answerObj.PublicId = string.Empty;
                                        answerObj.LastUpdatedBy = BusinessUserId;
                                        answerObj.IsCorrectAnswer = true;
                                        answerObj.IsCorrectForMultipleAnswer = false;
                                        if (existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                            answerObj.AssociatedScore = default(int);
                                        answerObj.LastUpdatedOn = currentDate;
                                        answerObj.DisplayOrder = existingQuestion.AnswerOptionsInQuizQuestions.Count(a => a.Status == (int)StatusEnum.Active) + 1;
                                        answerObj.Status = (int)StatusEnum.Active;
                                        answerObj.State = (int)QuizStateEnum.DRAFTED;
                                        answerObj.IsReadOnly = false;

                                        UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);
                                    }

                                    #endregion
                                }
                                else if (AnswerType == (int)AnswerTypeEnum.Availability)
                                {
                                    existingQuestion.Question = "Available from date";

                                    var answerObj = new Db.AnswerOptionsInQuizQuestions();

                                    answerObj.QuestionId = existingQuestion.Id;
                                    answerObj.QuizId = existingQuestion.QuizId;
                                    answerObj.Option = "";
                                    answerObj.OptionImage = string.Empty;
                                    answerObj.PublicId = string.Empty;
                                    answerObj.LastUpdatedBy = BusinessUserId;
                                    answerObj.LastUpdatedOn = currentDate;

                                    if (quiz.QuizType == (int)QuizTypeEnum.Score || quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                    {
                                        answerObj.AssociatedScore = default(int);
                                        answerObj.IsCorrectAnswer = false;
                                    }
                                    else if (quiz.QuizType == (int)QuizTypeEnum.Personality || quiz.QuizType == (int)QuizTypeEnum.PersonalityTemplate)
                                        answerObj.IsCorrectAnswer = false;
                                    else
                                        answerObj.IsCorrectAnswer = true;

                                    answerObj.DisplayOrder = 1;
                                    answerObj.Status = (int)StatusEnum.Active;
                                    answerObj.State = (int)QuizStateEnum.DRAFTED;
                                    answerObj.AutoPlay = true;
                                    answerObj.SecondsToApply = "0";
                                    answerObj.VideoFrameEnabled = false;

                                    UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);

                                    answerObj = new Db.AnswerOptionsInQuizQuestions();

                                    answerObj.QuestionId = existingQuestion.Id;
                                    answerObj.QuizId = existingQuestion.QuizId;
                                    answerObj.Option = "";
                                    answerObj.OptionImage = string.Empty;
                                    answerObj.PublicId = string.Empty;
                                    answerObj.LastUpdatedBy = BusinessUserId;
                                    answerObj.LastUpdatedOn = currentDate;

                                    if (quiz.QuizType == (int)QuizTypeEnum.Score || quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                    {
                                        answerObj.AssociatedScore = default(int);
                                        answerObj.IsCorrectAnswer = false;
                                    }
                                    else
                                        answerObj.IsCorrectAnswer = false;

                                    answerObj.DisplayOrder = 2;
                                    answerObj.Status = (int)StatusEnum.Active;
                                    answerObj.State = (int)QuizStateEnum.DRAFTED;
                                    answerObj.AutoPlay = true;
                                    answerObj.SecondsToApply = "0";
                                    answerObj.VideoFrameEnabled = false;

                                    UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);

                                    answerObj = new Db.AnswerOptionsInQuizQuestions();

                                    answerObj.QuestionId = existingQuestion.Id;
                                    answerObj.QuizId = existingQuestion.QuizId;
                                    answerObj.Option = "";
                                    answerObj.OptionImage = string.Empty;
                                    answerObj.PublicId = string.Empty;
                                    answerObj.LastUpdatedBy = BusinessUserId;
                                    answerObj.LastUpdatedOn = currentDate;

                                    if (quiz.QuizType == (int)QuizTypeEnum.Score || quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                    {
                                        answerObj.AssociatedScore = default(int);
                                        answerObj.IsCorrectAnswer = false;
                                    }
                                    else if (quiz.QuizType == (int)QuizTypeEnum.Personality || quiz.QuizType == (int)QuizTypeEnum.PersonalityTemplate)
                                        answerObj.IsCorrectAnswer = false;
                                    else
                                        answerObj.IsCorrectAnswer = true;

                                    answerObj.DisplayOrder = 3;
                                    answerObj.Status = (int)StatusEnum.Active;
                                    answerObj.State = (int)QuizStateEnum.DRAFTED;
                                    answerObj.AutoPlay = true;
                                    answerObj.SecondsToApply = "0";
                                    answerObj.VideoFrameEnabled = false;

                                    UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);
                                    UOWObj.Save();

                                } 
                                else if (AnswerType == (int)AnswerTypeEnum.RatingEmoji || AnswerType == (int)AnswerTypeEnum.RatingStarts) {
                                    #region to add answer option for RatingEmoji & RatingStarts

                                    if (isMultiRating) {
                                        existingQuestion.Question = "Are you happy with the way the recruiter contacted you?";
                                        string[] MultiRating = new string[] { "OptionTextforRatingOne", "OptionTextforRatingTwo", "OptionTextforRatingThree", "OptionTextforRatingFour", "OptionTextforRatingFive" };

                                        foreach (var obj in MultiRating) {
                                            var answerObj = new Db.AnswerOptionsInQuizQuestions();
                                            answerObj.QuestionId = QuestionId;
                                            answerObj.QuizId = existingQuestion.QuizId;
                                            answerObj.Option = null;
                                            answerObj.OptionImage = string.Empty;
                                            answerObj.PublicId = string.Empty;
                                            answerObj.LastUpdatedBy = BusinessUserId;
                                            if (existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                                answerObj.AssociatedScore = default(int);
                                            answerObj.LastUpdatedOn = currentDate;
                                            answerObj.DisplayOrder = existingQuestion.AnswerOptionsInQuizQuestions.Count(a => a.Status == (int)StatusEnum.Active) + 1;
                                            answerObj.Status = (int)StatusEnum.Active;
                                            answerObj.State = (int)QuizStateEnum.DRAFTED;
                                            answerObj.IsReadOnly = false;

                                            UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);
                                        }
                                    }

                                    #endregion
                                }
                                else if (isMultiRating && AnswerType == (int)AnswerTypeEnum.NPS) {
                                    string[] multiRatingNps = new string[] { "OptionTextforDetractorhigh1", "OptionTextforDetractorhigh2", "OptionTextforDetractorhigh3", "OptionTextforDetractorhigh4", "OptionTextforDetractorlow5",
                                        "OptionTextforDetractorlow6", "OptionTextforNeutral7", "OptionTextforNeutral8", "OptionTextforPromoter9", "OptionTextforPromoter10" };
                                    existingQuestion.Question = "NPS";
                                    foreach (var item in multiRatingNps) {
                                        var answerObj = new Db.AnswerOptionsInQuizQuestions();
                                        answerObj.QuestionId = QuestionId;
                                        answerObj.QuizId = existingQuestion.QuizId;
                                        answerObj.Option = null;
                                        answerObj.OptionImage = string.Empty;
                                        answerObj.PublicId = string.Empty;
                                        answerObj.LastUpdatedBy = BusinessUserId;
                                        if (existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                            answerObj.AssociatedScore = default(int);
                                        answerObj.LastUpdatedOn = currentDate;
                                        answerObj.DisplayOrder = existingQuestion.AnswerOptionsInQuizQuestions.Count(a => a.Status == (int)StatusEnum.Active) + 1;
                                        answerObj.Status = (int)StatusEnum.Active;
                                        answerObj.State = (int)QuizStateEnum.DRAFTED;
                                        answerObj.IsReadOnly = false;

                                        UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);
                                    }
                                }
                                else
                                {
                                    #region to add answer option for Single and Multiple type

                                    var answerObj = new Db.AnswerOptionsInQuizQuestions();

                                    if (existingQuestion.AnswerType == (int)AnswerTypeEnum.DOB || existingQuestion.AnswerType == (int)AnswerTypeEnum.PostCode || existingQuestion.AnswerType == (int)AnswerTypeEnum.DrivingLicense || existingQuestion.AnswerType == (int)AnswerTypeEnum.LookingforJobs || existingQuestion.AnswerType == (int)AnswerTypeEnum.FullAddress || existingQuestion.AnswerType == (int)AnswerTypeEnum.FirstName || existingQuestion.AnswerType == (int)AnswerTypeEnum.LastName
                                        || existingQuestion.AnswerType == (int)AnswerTypeEnum.Email || existingQuestion.AnswerType == (int)AnswerTypeEnum.PhoneNumber || existingQuestion.AnswerType == (int)AnswerTypeEnum.DatePicker)
                                        existingQuestion.Question = "Question" + (existingQuestion.QuizDetails.QuestionsInQuiz.Count(a => a.Status == (int)StatusEnum.Active && a.Type == (int)BranchingLogicEnum.QUESTION)).ToString();
                                    answerObj.QuestionId = QuestionId;
                                    answerObj.QuizId = existingQuestion.QuizId;
                                    answerObj.Option = "Answer 1";
                                    answerObj.OptionImage = string.Empty;
                                    answerObj.PublicId = string.Empty;
                                    answerObj.LastUpdatedBy = BusinessUserId;
                                    answerObj.IsCorrectAnswer = true;
                                    answerObj.IsCorrectForMultipleAnswer = false;
                                    if (existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                        answerObj.AssociatedScore = default(int);
                                    answerObj.LastUpdatedOn = currentDate;
                                    answerObj.DisplayOrder = 1;
                                    answerObj.Status = (int)StatusEnum.Active;
                                    answerObj.State = (int)QuizStateEnum.DRAFTED;
                                    answerObj.IsReadOnly = false;

                                    UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);

                                    answerObj = new Db.AnswerOptionsInQuizQuestions();

                                    answerObj.QuestionId = QuestionId;
                                    answerObj.QuizId = existingQuestion.QuizId;
                                    answerObj.Option = "Answer 2";
                                    answerObj.OptionImage = string.Empty;
                                    answerObj.PublicId = string.Empty;
                                    answerObj.LastUpdatedBy = BusinessUserId;
                                    answerObj.LastUpdatedOn = currentDate;
                                    answerObj.DisplayOrder = 2;
                                    answerObj.Status = (int)StatusEnum.Active;
                                    answerObj.State = (int)QuizStateEnum.DRAFTED;
                                    answerObj.IsCorrectAnswer = false;
                                    answerObj.IsCorrectForMultipleAnswer = false;
                                    answerObj.IsReadOnly = false;
                                    if (existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                        answerObj.AssociatedScore = default(int);

                                    UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);

                                    if (AnswerType == (int)AnswerTypeEnum.Single)
                                    {
                                        answerObj = new Db.AnswerOptionsInQuizQuestions();

                                        answerObj.QuestionId = QuestionId;
                                        answerObj.QuizId = existingQuestion.QuizId;
                                        answerObj.Option = "Unanswered";
                                        answerObj.OptionImage = string.Empty;
                                        answerObj.PublicId = string.Empty;
                                        answerObj.LastUpdatedBy = BusinessUserId;
                                        answerObj.LastUpdatedOn = currentDate;
                                        answerObj.DisplayOrder = 0;
                                        answerObj.Status = (int)StatusEnum.Active;
                                        answerObj.State = (int)QuizStateEnum.DRAFTED;
                                        answerObj.IsCorrectAnswer = false;
                                        answerObj.IsCorrectForMultipleAnswer = false;
                                        answerObj.IsReadOnly = false;
                                        answerObj.IsUnansweredType = true;
                                        if (existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                            answerObj.AssociatedScore = default(int);

                                        UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);
                                    }

                                    #endregion
                                }

                                #endregion
                            }

                            //sigle, multiple, DrivingLicense,LookingforJobs, FullAddress to Short, Long, DOB, PostCode, NPS, RatingEmoji, RatingStarts
                            else if ((existingQuestion.AnswerType == (int)AnswerTypeEnum.Single || existingQuestion.AnswerType == (int)AnswerTypeEnum.Multiple || existingQuestion.AnswerType == (int)AnswerTypeEnum.DrivingLicense || existingQuestion.AnswerType == (int)AnswerTypeEnum.LookingforJobs || existingQuestion.AnswerType == (int)AnswerTypeEnum.FullAddress || existingQuestion.AnswerType == (int)AnswerTypeEnum.Availability) && (AnswerType == (int)AnswerTypeEnum.Short || AnswerType == (int)AnswerTypeEnum.Long || AnswerType == (int)AnswerTypeEnum.DOB || AnswerType == (int)AnswerTypeEnum.PostCode || AnswerType == (int)AnswerTypeEnum.NPS || AnswerType == (int)AnswerTypeEnum.RatingEmoji || AnswerType == (int)AnswerTypeEnum.RatingStarts
                                || AnswerType == (int)AnswerTypeEnum.FirstName || AnswerType == (int)AnswerTypeEnum.LastName || AnswerType == (int)AnswerTypeEnum.Email || AnswerType == (int)AnswerTypeEnum.PhoneNumber || AnswerType == (int)AnswerTypeEnum.DatePicker))
                            {
                                #region update answer type other to text

                                foreach (var obj in existingQuestion.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active))
                                    RemoveAnswer(obj.Id, BusinessUserId, CompanyId, AnswerType, null);

                                var answerObj = new Db.AnswerOptionsInQuizQuestions();
                                existingQuestion.RevealCorrectAnswer = false;
                                answerObj.QuestionId = QuestionId;
                                answerObj.QuizId = existingQuestion.QuizId;
                                switch (AnswerType)
                                {
                                    case (int)AnswerTypeEnum.Short:
                                        answerObj.Option = "Short answer text...";
                                        if (existingQuestion.AnswerType == (int)AnswerTypeEnum.DrivingLicense || existingQuestion.AnswerType == (int)AnswerTypeEnum.LookingforJobs)
                                            existingQuestion.Question = "Question" + (existingQuestion.QuizDetails.QuestionsInQuiz.Count(a => a.Status == (int)StatusEnum.Active && a.Type == (int)BranchingLogicEnum.QUESTION)).ToString();
                                        answerObj.IsReadOnly = false;
                                        break;
                                    case (int)AnswerTypeEnum.Long:
                                        answerObj.Option = "Long answer text...";
                                        if (existingQuestion.AnswerType == (int)AnswerTypeEnum.DrivingLicense || existingQuestion.AnswerType == (int)AnswerTypeEnum.LookingforJobs || existingQuestion.AnswerType == (int)AnswerTypeEnum.Availability)
                                            existingQuestion.Question = "Question" + (existingQuestion.QuizDetails.QuestionsInQuiz.Count(a => a.Status == (int)StatusEnum.Active && a.Type == (int)BranchingLogicEnum.QUESTION)).ToString();
                                        answerObj.IsReadOnly = false;
                                        break;
                                    case (int)AnswerTypeEnum.DOB:
                                        answerObj.Option = null;
                                        answerObj.IsReadOnly = false;
                                        existingQuestion.Question = "Your date of birth";
                                        break;
                                    case (int)AnswerTypeEnum.PostCode:
                                        answerObj.Option = null;
                                        answerObj.IsReadOnly = false;
                                        existingQuestion.Question = "Your post code";
                                        break;
                                    case (int)AnswerTypeEnum.NPS:
                                        answerObj.Option = null;
                                        answerObj.IsReadOnly = false;
                                        existingQuestion.IsMultiRating = isMultiRating;
                                        existingQuestion.Question = isMultiRating ? "NPS" :"Question" + (existingQuestion.QuizDetails.QuestionsInQuiz.Count(a => a.Status == (int)StatusEnum.Active && a.Type == (int)BranchingLogicEnum.QUESTION)).ToString();
                                        break;
                                    case (int)AnswerTypeEnum.RatingEmoji:
                                    case (int)AnswerTypeEnum.RatingStarts:
                                        answerObj.Option = null;
                                        answerObj.IsReadOnly = true;
                                        existingQuestion.IsMultiRating = isMultiRating;
                                        existingQuestion.Question = "Are you happy with the way the recruiter contacted you?";
                                        break;

                                    case (int)AnswerTypeEnum.FirstName:
                                        answerObj.Option = null;
                                        answerObj.IsReadOnly = false;
                                        existingQuestion.Question = "Your first name";
                                        break;
                                    case (int)AnswerTypeEnum.LastName:
                                        answerObj.Option = null;
                                        answerObj.IsReadOnly = false;
                                        existingQuestion.Question = "Your last name";
                                        break;
                                    case (int)AnswerTypeEnum.Email:
                                        answerObj.Option = null;
                                        answerObj.IsReadOnly = false;
                                        existingQuestion.Question = "Your email";
                                        break;
                                    case (int)AnswerTypeEnum.PhoneNumber:
                                        answerObj.Option = null;
                                        answerObj.IsReadOnly = false;
                                        existingQuestion.Question = "Your phone number";
                                        break;

                                    case (int)AnswerTypeEnum.DatePicker:
                                        answerObj.Option = null;
                                        answerObj.IsReadOnly = false;
                                        existingQuestion.Question = "Your date picker";
                                        break;
                                }
                                answerObj.OptionImage = string.Empty;
                                answerObj.PublicId = string.Empty;
                                answerObj.IsCorrectAnswer = (AnswerType == (int)AnswerTypeEnum.NPS || AnswerType == (int)AnswerTypeEnum.RatingEmoji || AnswerType == (int)AnswerTypeEnum.RatingStarts) ? false : true;
                                answerObj.IsCorrectForMultipleAnswer = (AnswerType == (int)AnswerTypeEnum.NPS || AnswerType == (int)AnswerTypeEnum.RatingEmoji || AnswerType == (int)AnswerTypeEnum.RatingStarts) ? false : true;
                                answerObj.LastUpdatedBy = BusinessUserId;
                                answerObj.LastUpdatedOn = currentDate;
                                answerObj.DisplayOrder = 1;
                                answerObj.Status = (int)StatusEnum.Active;
                                answerObj.State = (int)QuizStateEnum.DRAFTED;

                                UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);

                                #endregion
                            }

                            //Short, Long, DOB, PostCode, NPS, RatingEmoji, RatingStarts to Short, Long, DOB, PostCode, NPS, RatingEmoji, RatingStarts, firstname, lastname, email, phonenumber


                            else if ((existingQuestion.AnswerType == (int)AnswerTypeEnum.Short || existingQuestion.AnswerType == (int)AnswerTypeEnum.Long || existingQuestion.AnswerType == (int)AnswerTypeEnum.DOB || existingQuestion.AnswerType == (int)AnswerTypeEnum.PostCode || existingQuestion.AnswerType == (int)AnswerTypeEnum.NPS || existingQuestion.AnswerType == (int)AnswerTypeEnum.RatingEmoji || existingQuestion.AnswerType == (int)AnswerTypeEnum.RatingStarts || existingQuestion.AnswerType == (int)AnswerTypeEnum.Availability || existingQuestion.AnswerType == (int)AnswerTypeEnum.FirstName
                                || existingQuestion.AnswerType == (int)AnswerTypeEnum.LastName || existingQuestion.AnswerType == (int)AnswerTypeEnum.Email || existingQuestion.AnswerType == (int)AnswerTypeEnum.PhoneNumber || existingQuestion.AnswerType == (int)AnswerTypeEnum.DatePicker)

                              && (AnswerType == (int)AnswerTypeEnum.Short || AnswerType == (int)AnswerTypeEnum.Long || AnswerType == (int)AnswerTypeEnum.DOB || AnswerType == (int)AnswerTypeEnum.PostCode || AnswerType == (int)AnswerTypeEnum.NPS || AnswerType == (int)AnswerTypeEnum.RatingEmoji || AnswerType == (int)AnswerTypeEnum.RatingStarts || AnswerType == (int)AnswerTypeEnum.Availability || AnswerType == (int)AnswerTypeEnum.FirstName || AnswerType == (int)AnswerTypeEnum.LastName || AnswerType == (int)AnswerTypeEnum.Email || AnswerType == (int)AnswerTypeEnum.PhoneNumber || AnswerType == (int)AnswerTypeEnum.DatePicker))
                            {
                                #region update answer type text to text

                                var answerObj = existingQuestion.AnswerOptionsInQuizQuestions.FirstOrDefault(r => r.Status == (int)StatusEnum.Active);
                                if (existingQuestion.AnswerType == (int)AnswerTypeEnum.RatingEmoji || existingQuestion.AnswerType == (int)AnswerTypeEnum.RatingStarts || existingQuestion.AnswerType == (int)AnswerTypeEnum.Availability || existingQuestion.AnswerType == (int)AnswerTypeEnum.NPS) {
                                    foreach (var obj in existingQuestion.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active)) {
                                        if (obj.Id != answerObj.Id) {
                                            RemoveAnswer(obj.Id, BusinessUserId, CompanyId, AnswerType, null);
                                        }
                                    }
                                }
                                existingQuestion.RevealCorrectAnswer = false;

                                switch (AnswerType)
                                {
                                    case (int)AnswerTypeEnum.Short:
                                        answerObj.Option = "Short answer text...";
                                        if (existingQuestion.AnswerType == (int)AnswerTypeEnum.DOB || existingQuestion.AnswerType == (int)AnswerTypeEnum.PostCode || existingQuestion.AnswerType == (int)AnswerTypeEnum.RatingEmoji || existingQuestion.AnswerType == (int)AnswerTypeEnum.RatingStarts || existingQuestion.AnswerType == (int)AnswerTypeEnum.FirstName
                                            || existingQuestion.AnswerType == (int)AnswerTypeEnum.LastName || existingQuestion.AnswerType == (int)AnswerTypeEnum.Email || existingQuestion.AnswerType == (int)AnswerTypeEnum.PhoneNumber || existingQuestion.AnswerType == (int)AnswerTypeEnum.DatePicker)
                                        {
                                            existingQuestion.Question = "Question" + (existingQuestion.QuizDetails.QuestionsInQuiz.Count(a => a.Status == (int)StatusEnum.Active && a.Type == (int)BranchingLogicEnum.QUESTION)).ToString();
                                            existingQuestion.EnableComment = false;
                                        }
                                        break;
                                    case (int)AnswerTypeEnum.Long:
                                        answerObj.Option = "Long answer text...";
                                        if (existingQuestion.AnswerType == (int)AnswerTypeEnum.DOB || existingQuestion.AnswerType == (int)AnswerTypeEnum.PostCode || existingQuestion.AnswerType == (int)AnswerTypeEnum.RatingEmoji || existingQuestion.AnswerType == (int)AnswerTypeEnum.RatingStarts)
                                        {
                                            existingQuestion.Question = "Question" + (existingQuestion.QuizDetails.QuestionsInQuiz.Count(a => a.Status == (int)StatusEnum.Active && a.Type == (int)BranchingLogicEnum.QUESTION)).ToString();
                                            existingQuestion.EnableComment = false;
                                        }
                                        break;
                                    case (int)AnswerTypeEnum.DOB:
                                        answerObj.Option = null;
                                        existingQuestion.Question = "Your date of birth";
                                        existingQuestion.EnableComment = false;
                                        break;
                                    case (int)AnswerTypeEnum.PostCode:
                                        answerObj.Option = null;
                                        existingQuestion.Question = "Your post code";
                                        existingQuestion.EnableComment = false;
                                        break;
                                    case (int)AnswerTypeEnum.NPS:
                                        answerObj.IsCorrectAnswer = false;
                                        answerObj.IsCorrectForMultipleAnswer = false;
                                        answerObj.Option = null;
                                        existingQuestion.RevealCorrectAnswer = false;
                                        existingQuestion.IsMultiRating = isMultiRating;
                                        existingQuestion.Question = isMultiRating ? "NPS": "Question" + (existingQuestion.QuizDetails.QuestionsInQuiz.Count(a => a.Status == (int)StatusEnum.Active && a.Type == (int)BranchingLogicEnum.QUESTION)).ToString();
                                        existingQuestion.EnableComment = false;
                                        break;
                                    case (int)AnswerTypeEnum.RatingEmoji:
                                    case (int)AnswerTypeEnum.RatingStarts:
                                        answerObj.IsCorrectAnswer = false;
                                        answerObj.IsCorrectForMultipleAnswer = false;
                                        answerObj.Option = null;
                                        answerObj.IsReadOnly = true;
                                        existingQuestion.IsMultiRating = isMultiRating;
                                        existingQuestion.Question = "Are you happy with the way the recruiter contacted you?";
                                        break;
                                    case (int)AnswerTypeEnum.Availability:
                                        existingQuestion.Question = "Available from date";
                                        break;

                                    case (int)AnswerTypeEnum.FirstName:
                                        answerObj.Option = null;
                                        existingQuestion.Question = "Your first name";
                                        existingQuestion.EnableComment = false;
                                        break;
                                    case (int)AnswerTypeEnum.LastName:
                                        answerObj.Option = null;
                                        existingQuestion.Question = "Your lastname";
                                        existingQuestion.EnableComment = false;
                                        break;
                                    case (int)AnswerTypeEnum.Email:
                                        answerObj.Option = null;
                                        existingQuestion.Question = "Your email";
                                        existingQuestion.EnableComment = false;
                                        break;
                                    case (int)AnswerTypeEnum.PhoneNumber:
                                        answerObj.Option = null;
                                        existingQuestion.Question = "Your phone number";
                                        existingQuestion.EnableComment = false;
                                        break;
                                    case (int)AnswerTypeEnum.DatePicker:
                                        answerObj.Option = null;
                                        existingQuestion.Question = "Your date picker";
                                        existingQuestion.EnableComment = false;
                                        break;                                   
                                }
                                UOWObj.AnswerOptionsInQuizQuestionsRepository.Update(answerObj);

                                #endregion
                            }

                            //Single, LookingforJobs to other
                            if ((existingQuestion.AnswerType == (int)AnswerTypeEnum.Single || existingQuestion.AnswerType == (int)AnswerTypeEnum.LookingforJobs) && (AnswerType != (int)AnswerTypeEnum.Single && AnswerType != (int)AnswerTypeEnum.LookingforJobs))
                            {
                                #region update answer type Single to other

                                var ansIds = existingQuestion.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active).ToList().Select(t => t.Id);
                                var branchingLogicList = UOWObj.BranchingLogicRepository.Get(r => ansIds.Any(s => s == r.SourceObjectId) && r.SourceTypeId == (int)BranchingLogicEnum.ANSWER).ToList();

                                if (branchingLogicList != null)
                                {
                                    foreach (var obj in branchingLogicList)
                                    {
                                        UOWObj.BranchingLogicRepository.Delete(obj);
                                    }
                                }

                                #endregion
                            }

                            if (existingQuestion.AnswerType != (int)AnswerTypeEnum.Single || existingQuestion.AnswerType != (int)AnswerTypeEnum.Multiple)
                            {
                                #region update answer type Single to other

                                var ansIds = existingQuestion.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active).ToList().Select(t => t.Id);
                                if (ansIds != null && ansIds.Any())
                                {
                                    var objectFieldsInAnswerList = UOWObj.ObjectFieldsInAnswerRepository.Get(r => ansIds.Contains(r.AnswerOptionsInQuizQuestionsId)).ToList();

                                    if (objectFieldsInAnswerList != null && objectFieldsInAnswerList.Any())
                                    {
                                        foreach (var obj in objectFieldsInAnswerList)
                                        {
                                            UOWObj.ObjectFieldsInAnswerRepository.Delete(obj);
                                        }
                                    }
                                }
                                #endregion
                            }


                            //other to Short, Long, DOB, PostCode, NPS, RatingEmoji, RatingStarts,NPSQuestion
                            if ((existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate) && (existingQuestion.AnswerType == (int)AnswerTypeEnum.Multiple || existingQuestion.AnswerType == (int)AnswerTypeEnum.DrivingLicense) && (AnswerType == (int)AnswerTypeEnum.Single || AnswerType == (int)AnswerTypeEnum.DOB || AnswerType == (int)AnswerTypeEnum.PostCode || AnswerType == (int)AnswerTypeEnum.FullAddress || AnswerType == (int)AnswerTypeEnum.LookingforJobs || AnswerType == (int)AnswerTypeEnum.RatingStarts || AnswerType == (int)AnswerTypeEnum.RatingEmoji
                                || AnswerType == (int)AnswerTypeEnum.FirstName || AnswerType == (int)AnswerTypeEnum.LastName || AnswerType == (int)AnswerTypeEnum.Email || AnswerType == (int)AnswerTypeEnum.PhoneNumber || AnswerType == (int)AnswerTypeEnum.DatePicker || AnswerType == (int)AnswerTypeEnum.NPS))
                            {
                                existingQuestion.MinAnswer = 1;
                                existingQuestion.MaxAnswer = 1;
                            }

                            if ((existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate) && (AnswerType == (int)AnswerTypeEnum.DrivingLicense || AnswerType == (int)AnswerTypeEnum.Multiple))
                            {
                                existingQuestion.MinAnswer = 0;
                                existingQuestion.MaxAnswer = existingQuestion.AnswerOptionsInQuizQuestions.Count(r => r.Status == (int)StatusEnum.Active);
                            }

                            if (AnswerType != (int)AnswerTypeEnum.Single || AnswerType != (int)AnswerTypeEnum.Multiple || AnswerType != (int)AnswerTypeEnum.Short || AnswerType != (int)AnswerTypeEnum.Long)
                            {
                                existingQuestion.TimerRequired = false;
                                existingQuestion.Time = null;
                            }

                            if (AnswerType != (int)AnswerTypeEnum.Single && AnswerType != (int)AnswerTypeEnum.Multiple)
                            {
                                existingQuestion.ShowAnswerImage = false;
                            }

                            if (isWhatsappEnable)
                            {
                                existingQuestion.AnswerStructureType = answerStructureType;
                            }

                            existingQuestion.IsMultiRating = isMultiRating;
                            existingQuestion.AnswerType = AnswerType;
                            existingQuestion.LastUpdatedBy = BusinessUserId;
                            existingQuestion.LastUpdatedOn = currentDate;

                            existingQuestion.QuizDetails.LastUpdatedBy = BusinessUserId;
                            existingQuestion.QuizDetails.LastUpdatedOn = currentDate;

                            existingQuestion.QuizDetails.Quiz.State = (int)QuizStateEnum.DRAFTED;

                            existingQuestion.AutoPlay = false;
                            if (existingQuestion.AnswerType == (int)AnswerTypeEnum.Single || existingQuestion.AnswerType == (int)AnswerTypeEnum.Multiple)
                            {
                                var autoplay = existingQuestion.AnswerOptionsInQuizQuestions.Any(v => v.AutoPlay) ? true : false;
                                existingQuestion.AutoPlay = autoplay;
                            }



                            UOWObj.QuizRepository.Update(existingQuestion.QuizDetails.Quiz);
                            UOWObj.Save();

                            
                            
                        }
                        else
                        {
                            if (isWhatsappEnable)
                            {
                              
                                if (existingQuestion.AnswerStructureType != answerStructureType && (answerStructureType == (int)AnswerStructureTypeEnum.List || answerStructureType == (int)AnswerStructureTypeEnum.Button))
                                {
                                    if (AnswerType == (int)AnswerTypeEnum.Single)
                                    {
                                        existingQuestion.EnableComment = false;

                                        SetSingleMultipleAnswerTYpe(QuestionId, AnswerType, answerStructureType, BusinessUserId, CompanyId, UOWObj, existingQuestion, currentDate);
                                    }
                                }


                                existingQuestion.AnswerStructureType = answerStructureType != 0 ? answerStructureType : (int)AnswerStructureTypeEnum.Default;
                                UOWObj.QuestionsInQuizRepository.Update(existingQuestion);
                                UOWObj.Save();

                            }
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

        }


        public void RemoveAnswer(int AnswerId, int BusinessUserId, int CompanyId, int? type, AutomationUnitOfWork UOWObj)
        {
            try
            {
                using (UOWObj == null ? UOWObj = new AutomationUnitOfWork() : null)
                {
                    var answerOption = UOWObj.AnswerOptionsInQuizQuestionsRepository.GetByID(AnswerId);

                    var currentDate = DateTime.UtcNow;

                    if (answerOption != null)
                    {
                        var activeAnswersInQuestion = answerOption.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && !r.IsUnansweredType);

                        if (type == null && activeAnswersInQuestion.Count() == 2 && (activeAnswersInQuestion.FirstOrDefault().QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single || activeAnswersInQuestion.FirstOrDefault().QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple))
                        {
                            Status = ResultEnum.Error;
                            ErrorMessage = "There must be minimum 2 active answers in a question.";
                        }
                        else if (type == null && answerOption.IsReadOnly)
                        {
                            Status = ResultEnum.Error;
                            ErrorMessage = "User can not delete this answers.";
                        }
                        else
                        {
                            SaveDynamicVariable(answerOption.Option, string.Empty, answerOption.QuestionsInQuiz.QuizDetails.Id);

                            //if current answer is correct then set first answer as correct
                            if (type == null && answerOption.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single && answerOption.IsCorrectAnswer.HasValue && answerOption.IsCorrectAnswer.Value)
                            {
                                var firstAns = activeAnswersInQuestion.OrderBy(r => r.DisplayOrder).FirstOrDefault(r => r.Id != AnswerId);
                                firstAns.IsCorrectAnswer = true;
                            }

                            if ((answerOption.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple || answerOption.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.DrivingLicense)
                                && (answerOption.QuestionsInQuiz.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score
                                || answerOption.QuestionsInQuiz.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate
                                || answerOption.QuestionsInQuiz.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Personality
                                || answerOption.QuestionsInQuiz.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.PersonalityTemplate)
                                && answerOption.QuestionsInQuiz.MaxAnswer > answerOption.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Count(r => r.Status == (int)StatusEnum.Active && !r.IsUnansweredType) - 1)
                                answerOption.QuestionsInQuiz.MaxAnswer = answerOption.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Count(r => r.Status == (int)StatusEnum.Active && !r.IsUnansweredType) - 1;

                            answerOption.Status = (int)StatusEnum.Deleted;
                            answerOption.LastUpdatedBy = BusinessUserId;
                            answerOption.LastUpdatedOn = currentDate;

                            //Remove Personality Quiz Answer Result Mapping 
                            if (answerOption.PersonalityAnswerResultMapping.Any())
                            {
                                var MappingList = answerOption.PersonalityAnswerResultMapping.ToList();
                                foreach (var data in MappingList)
                                {
                                    UOWObj.PersonalityAnswerResultMappingRepository.Delete(data.Id);
                                }
                            }

                            answerOption.QuestionsInQuiz.QuizDetails.LastUpdatedBy = BusinessUserId;
                            answerOption.QuestionsInQuiz.QuizDetails.LastUpdatedOn = currentDate;

                            answerOption.QuestionsInQuiz.QuizDetails.Quiz.State = (int)QuizStateEnum.DRAFTED;

                            //remove if answer exists in branching logic
                            var branchingLogic = UOWObj.BranchingLogicRepository.Get(r => r.SourceObjectId == AnswerId && r.SourceTypeId == (int)BranchingLogicEnum.ANSWER).FirstOrDefault();

                            if (branchingLogic != null)
                            {
                                UOWObj.BranchingLogicRepository.Delete(branchingLogic);
                            }

                            UOWObj.QuizRepository.Update(answerOption.QuestionsInQuiz.QuizDetails.Quiz);
                            UOWObj.Save();
                            
                            
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Answer not found for the AnswerId " + AnswerId;
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

        private void SetSingleMultipleAnswerTYpe(int QuestionId, int AnswerType, int? answerStructureType, int BusinessUserId, int CompanyId, AutomationUnitOfWork UOWObj, Db.QuestionsInQuiz existingQuestion, DateTime currentDate)
        {
            foreach (var obj in existingQuestion.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active))
                RemoveAnswer(obj.Id, BusinessUserId, CompanyId, AnswerType, UOWObj);
            #region to add answer option for Single and Multiple type

            var answerObj = new Db.AnswerOptionsInQuizQuestions();

            if (existingQuestion.AnswerType == (int)AnswerTypeEnum.DOB || existingQuestion.AnswerType == (int)AnswerTypeEnum.PostCode || existingQuestion.AnswerType == (int)AnswerTypeEnum.DrivingLicense || existingQuestion.AnswerType == (int)AnswerTypeEnum.LookingforJobs || existingQuestion.AnswerType == (int)AnswerTypeEnum.FullAddress || existingQuestion.AnswerType == (int)AnswerTypeEnum.Availability || existingQuestion.AnswerType == (int)AnswerTypeEnum.RatingEmoji || existingQuestion.AnswerType == (int)AnswerTypeEnum.RatingStarts
                || existingQuestion.AnswerType == (int)AnswerTypeEnum.FirstName || existingQuestion.AnswerType == (int)AnswerTypeEnum.LastName || existingQuestion.AnswerType == (int)AnswerTypeEnum.Email || existingQuestion.AnswerType == (int)AnswerTypeEnum.PhoneNumber || existingQuestion.AnswerType == (int)AnswerTypeEnum.DatePicker)
                existingQuestion.Question = "Question" + (existingQuestion.QuizDetails.QuestionsInQuiz.Count(a => a.Status == (int)StatusEnum.Active && a.Type == (int)BranchingLogicEnum.QUESTION)).ToString();
            answerObj.QuestionId = QuestionId;
            answerObj.QuizId = existingQuestion.QuizId;
            answerObj.Option = "Answer 1";
            answerObj.Description = (answerStructureType == 1 && AnswerType == (int)AnswerTypeEnum.Single) ? "" : null;
            answerObj.OptionImage = string.Empty;
            answerObj.PublicId = string.Empty;
            answerObj.LastUpdatedBy = BusinessUserId;
            answerObj.IsCorrectAnswer = true;
            answerObj.IsCorrectForMultipleAnswer = false;
            if (existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                answerObj.AssociatedScore = default(int);
            answerObj.LastUpdatedOn = currentDate;
            answerObj.DisplayOrder = 1;
            answerObj.Status = (int)StatusEnum.Active;
            answerObj.State = (int)QuizStateEnum.DRAFTED;
            answerObj.IsReadOnly = false;

            UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);

            answerObj = new Db.AnswerOptionsInQuizQuestions();

            answerObj.QuestionId = QuestionId;
            answerObj.QuizId = existingQuestion.QuizId;
            answerObj.Option = "Answer 2";
            answerObj.Description = (answerStructureType == 1 && AnswerType == (int)AnswerTypeEnum.Single) ? "" : null;
            answerObj.OptionImage = string.Empty;
            answerObj.PublicId = string.Empty;
            answerObj.LastUpdatedBy = BusinessUserId;
            answerObj.LastUpdatedOn = currentDate;
            answerObj.DisplayOrder = 2;
            answerObj.Status = (int)StatusEnum.Active;
            answerObj.State = (int)QuizStateEnum.DRAFTED;
            answerObj.IsCorrectAnswer = false;
            answerObj.IsCorrectForMultipleAnswer = false;
            answerObj.IsReadOnly = false;
            if (existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                answerObj.AssociatedScore = default(int);

            UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);

            if (AnswerType == (int)AnswerTypeEnum.Single)
            {
                answerObj = new Db.AnswerOptionsInQuizQuestions();

                answerObj.QuestionId = QuestionId;
                answerObj.QuizId = existingQuestion.QuizId;
                answerObj.Option = "Unanswered";
                answerObj.OptionImage = string.Empty;
                answerObj.PublicId = string.Empty;
                answerObj.LastUpdatedBy = BusinessUserId;
                answerObj.LastUpdatedOn = currentDate;
                answerObj.DisplayOrder = 0;
                answerObj.Status = (int)StatusEnum.Active;
                answerObj.State = (int)QuizStateEnum.DRAFTED;
                answerObj.IsCorrectAnswer = false;
                answerObj.Description = "";
                answerObj.IsCorrectForMultipleAnswer = false;
                answerObj.IsReadOnly = false;
                answerObj.IsUnansweredType = true;
                if (existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || existingQuestion.QuizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                    answerObj.AssociatedScore = default(int);

                UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);
            }

            #endregion
        }



        public QuizQuestion AddQuizQuestion(int QuizId, int BusinessUserId, int CompanyId, int Type, bool isWhatsappEnable = false)
        {
            QuizQuestion quizQuestionObj = null;
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    using (var transaction = Utility.CreateTransactionScope())
                    {
                        var quizObj = UOWObj.QuizRepository.GetByID(QuizId);

                        if (quizObj != null && quizObj.QuizDetails.Any())
                        {
                            var quizDetails = quizObj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED);

                            if (quizDetails != null)
                            {
                                #region Adding obj

                                var currentDate = DateTime.UtcNow;

                                var existingQuestionInQuiz = quizDetails.QuestionsInQuiz.FirstOrDefault();

                                var contentsInQuiz = quizDetails.ContentsInQuiz.Where(a => a.Status == (int)StatusEnum.Active);
                                var questionInQuiz = quizDetails.QuestionsInQuiz.Where(a => a.Status == (int)StatusEnum.Active);
                                var contentsInQuizCount = contentsInQuiz.Count();
                                var questionInQuizCount = questionInQuiz.Count();


                                var obj = new Db.QuestionsInQuiz();

                                obj.QuizId = quizDetails.Id;
                                obj.ShowAnswerImage = false;
                                if (Type == (int)BranchingLogicEnum.QUESTION)
                                    obj.Question = "Question " + (questionInQuiz.Count(r => r.Type == (int)BranchingLogicEnum.QUESTION) + 1).ToString();
                                else if (Type == (int)BranchingLogicEnum.CONTENT)
                                    obj.Question = "Content " + (questionInQuiz.Count(r => r.Type == (int)BranchingLogicEnum.CONTENT) + 1).ToString();
                                obj.ShowTitle = quizObj.UsageTypeInQuiz.Any(a => a.UsageType == (int)UsageTypeEnum.WhatsAppChatbot) ? false : true;
                                obj.ShowQuestionImage = true;
                                obj.QuestionImage = string.Empty;
                                obj.PublicId = string.Empty;
                                obj.Status = (int)StatusEnum.Active;
                                obj.State = (int)QuizStateEnum.DRAFTED;
                                obj.AnswerType = (int)AnswerTypeEnum.Single;
                                //obj.NextButtonText = "Next";
                                obj.NextButtonText = "";
                                obj.NextButtonTxtSize = "24px";
                                obj.ViewPreviousQuestion = quizDetails.ViewPreviousQuestion;
                                obj.EditAnswer = quizDetails.EditAnswer;
                                obj.AutoPlay = true;
                                obj.SecondsToApply = "0";
                                obj.VideoFrameEnabled = false;
                                obj.Description = quizObj.UsageTypeInQuiz.Any(a => a.UsageType == (int)UsageTypeEnum.WhatsAppChatbot) ? "Message" : "Description";
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

                                if (existingQuestionInQuiz != null)
                                {
                                    obj.RevealCorrectAnswer = existingQuestionInQuiz.RevealCorrectAnswer;
                                    obj.AliasTextForCorrect = existingQuestionInQuiz.AliasTextForCorrect;
                                    obj.AliasTextForIncorrect = existingQuestionInQuiz.AliasTextForIncorrect;
                                    obj.AliasTextForYourAnswer = existingQuestionInQuiz.AliasTextForYourAnswer;
                                    obj.AliasTextForCorrectAnswer = existingQuestionInQuiz.AliasTextForCorrectAnswer;
                                    obj.AliasTextForExplanation = existingQuestionInQuiz.AliasTextForExplanation;
                                    obj.AliasTextForNextButton = existingQuestionInQuiz.AliasTextForNextButton;
                                }
                                else if (quizObj.QuizType != (int)QuizTypeEnum.Score && quizObj.QuizType != (int)QuizTypeEnum.ScoreTemplate && quizObj.QuizType != (int)QuizTypeEnum.Personality && quizObj.QuizType != (int)QuizTypeEnum.PersonalityTemplate)
                                {
                                    obj.RevealCorrectAnswer = false;
                                    //obj.AliasTextForCorrect = "Correct";
                                    //obj.AliasTextForIncorrect = "Incorrect";
                                    //obj.AliasTextForYourAnswer = "Your Answer";
                                    //obj.AliasTextForCorrectAnswer = "Correct Answer";
                                    //obj.AliasTextForExplanation = "Explanation";
                                    //obj.AliasTextForNextButton = "Next";
                                    obj.AliasTextForCorrect = "";
                                    obj.AliasTextForIncorrect = "";
                                    obj.AliasTextForYourAnswer = "";
                                    obj.AliasTextForCorrectAnswer = "";
                                    obj.AliasTextForExplanation = "";
                                    obj.AliasTextForNextButton = "";
                                }

                                if (isWhatsappEnable)
                                {
                                    if (obj.AnswerType == (int)AnswerTypeEnum.Single)
                                    {
                                        obj.AnswerStructureType = (int)AnswerStructureTypeEnum.Button;
                                    }
                                    else if (obj.AnswerType == (int)AnswerTypeEnum.NPS)
                                    {
                                        obj.AnswerStructureType = (int)AnswerStructureTypeEnum.List;
                                    }
                                    else
                                    {
                                        obj.AnswerStructureType = (int)AnswerStructureTypeEnum.Default;
                                    }
                                }
                                else
                                {
                                    obj.AnswerStructureType = (int)AnswerStructureTypeEnum.Default;
                                }

                                if ((questionInQuizCount + contentsInQuizCount) == 0)
                                    obj.DisplayOrder = 1;
                                else if (questionInQuizCount != 0 && contentsInQuizCount != 0)
                                    obj.DisplayOrder = (questionInQuiz.Max(r => r.DisplayOrder) > contentsInQuiz.Max(r => r.DisplayOrder) ? questionInQuiz.Max(r => r.DisplayOrder) + 1 : contentsInQuiz.Max(r => r.DisplayOrder) + 1);
                                else if (questionInQuizCount != 0)
                                    obj.DisplayOrder = (questionInQuiz.Max(r => r.DisplayOrder) + 1);
                                else if (contentsInQuizCount != 0)
                                    obj.DisplayOrder = (contentsInQuiz.Max(r => r.DisplayOrder) + 1);

                                obj.LastUpdatedBy = BusinessUserId;
                                obj.LastUpdatedOn = currentDate;

                                UOWObj.QuestionsInQuizRepository.Insert(obj);
                                UOWObj.Save();

                                var answerObj = new Db.AnswerOptionsInQuizQuestions();

                                answerObj.QuestionId = obj.Id;
                                answerObj.QuizId = obj.QuizId;
                                answerObj.Option = "Answer 1";
                                answerObj.Description = "";
                                answerObj.OptionImage = string.Empty;
                                answerObj.PublicId = string.Empty;
                                answerObj.LastUpdatedBy = BusinessUserId;
                                answerObj.LastUpdatedOn = currentDate;

                                if (quizObj.QuizType == (int)QuizTypeEnum.Score || quizObj.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                {
                                    answerObj.AssociatedScore = default(int);
                                    answerObj.IsCorrectAnswer = false;
                                }
                                else if (quizObj.QuizType == (int)QuizTypeEnum.Personality || quizObj.QuizType == (int)QuizTypeEnum.PersonalityTemplate)
                                    answerObj.IsCorrectAnswer = false;
                                else
                                    answerObj.IsCorrectAnswer = true;

                                answerObj.DisplayOrder = 1;
                                answerObj.Status = (int)StatusEnum.Active;
                                answerObj.State = (int)QuizStateEnum.DRAFTED;
                                answerObj.AutoPlay = true;
                                answerObj.SecondsToApply = "0";
                                answerObj.VideoFrameEnabled = false;

                                UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);


                                answerObj = new Db.AnswerOptionsInQuizQuestions();

                                answerObj.QuestionId = obj.Id;
                                answerObj.QuizId = obj.QuizId;
                                answerObj.Option = "Answer 2";
                                answerObj.Description = "";
                                answerObj.OptionImage = string.Empty;
                                answerObj.PublicId = string.Empty;
                                answerObj.LastUpdatedBy = BusinessUserId;
                                answerObj.LastUpdatedOn = currentDate;

                                if (quizObj.QuizType == (int)QuizTypeEnum.Score || quizObj.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                {
                                    answerObj.AssociatedScore = default(int);
                                    answerObj.IsCorrectAnswer = false;
                                }
                                else
                                    answerObj.IsCorrectAnswer = false;

                                answerObj.DisplayOrder = 2;
                                answerObj.Status = (int)StatusEnum.Active;
                                answerObj.State = (int)QuizStateEnum.DRAFTED;
                                answerObj.AutoPlay = true;
                                answerObj.SecondsToApply = "0";
                                answerObj.VideoFrameEnabled = false;

                                UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);

                                quizDetails.LastUpdatedBy = BusinessUserId;
                                quizDetails.LastUpdatedOn = currentDate;

                                quizObj.State = (int)QuizStateEnum.DRAFTED;


                                if (quizObj.QuizType == (int)QuizTypeEnum.Score || quizObj.QuizType == (int)QuizTypeEnum.ScoreTemplate || quizObj.QuizType == (int)QuizTypeEnum.Personality || quizObj.QuizType == (int)QuizTypeEnum.PersonalityTemplate)
                                {
                                    obj.MinAnswer = 1;
                                    obj.MaxAnswer = 1;
                                }
                                UOWObj.QuestionsInQuizRepository.Update(obj);


                                #region for unanswered answer option

                                answerObj = new Db.AnswerOptionsInQuizQuestions();

                                answerObj.QuestionId = obj.Id;
                                answerObj.QuizId = obj.QuizId;
                                answerObj.Option = "Unanswered";
                                answerObj.OptionImage = string.Empty;
                                answerObj.PublicId = string.Empty;
                                answerObj.LastUpdatedBy = BusinessUserId;
                                answerObj.LastUpdatedOn = currentDate;

                                if (quizObj.QuizType == (int)QuizTypeEnum.Score || quizObj.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                {
                                    answerObj.AssociatedScore = default(int);
                                    answerObj.IsCorrectAnswer = false;
                                }
                                else
                                    answerObj.IsCorrectAnswer = false;

                                answerObj.DisplayOrder = 0;
                                answerObj.Status = (int)StatusEnum.Active;
                                answerObj.State = (int)QuizStateEnum.DRAFTED;
                                answerObj.IsUnansweredType = true;
                                answerObj.AutoPlay = true;
                                answerObj.SecondsToApply = "0";
                                answerObj.VideoFrameEnabled = false;

                                UOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);

                                #endregion

                                UOWObj.QuizRepository.Update(quizObj);

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
                                //if(isWhatsappEnable)
                                //{
                                //    quizQuestionObj.AnswerStructureType =(int)AnswerStructureType.List;
                                //}
                                //else
                                //{
                                //    quizQuestionObj.AnswerStructureType = (int)AnswerStructureType.Default;
                                //}

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

                                foreach (var answer in obj.AnswerOptionsInQuizQuestions.Where(r => !r.IsUnansweredType).OrderBy(r => r.DisplayOrder))
                                {
                                    quizQuestionObj.AnswerList.Add(new AnswerOptionInQuestion
                                    {
                                        AnswerId = answer.Id,
                                        AnswerText = answer.Option,
                                        AnswerDescription = answer.Description,
                                        AnswerImage = answer.OptionImage,
                                        AssociatedScore = answer.AssociatedScore,
                                        PublicIdForAnswer = answer.PublicId,
                                        IsCorrectAnswer = answer.IsCorrectAnswer,
                                        DisplayOrder = answer.DisplayOrder
                                    });
                                }

                                #endregion
                                
                                
                            }
                        }
                        else
                        {
                            Status = ResultEnum.Error;
                            ErrorMessage = "Quiz not found for the QuizId " + QuizId;
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
            return quizQuestionObj;
        }
        public QuestionMappedValue GetQuestionMappedFieldValues(int QuizId, string LeadUserId) {
            
            var quizQuestionObj = new QuestionMappedValue();
            quizQuestionObj.QuestionDetails = new List<QuestionMappedValue.QuestionDetail>();
            try {
                using (var UOWObj = new AutomationUnitOfWork()) {

                    var quizattemptObj = UOWObj.QuizAttemptsRepository.Get(v => v.LeadUserId == LeadUserId && v.QuizId == QuizId).FirstOrDefault();
                    if (quizattemptObj is null) { return null; }

                    var quesStats = UOWObj.QuizQuestionStatsRepository.Get(v => v.CompletedOn.HasValue && v.QuizAttemptId == quizattemptObj.Id);
                    if(quesStats is null) { return null; }

                    foreach (var stats in quesStats) {
                        var questionObj = UOWObj.QuestionsInQuizRepository.Get(v => v.QuizId == quizattemptObj.QuizId && v.Id == stats.QuestionId);
                        if (questionObj is null) { return null; }

                        foreach (var question in questionObj) {
                            if (question != null) {

                                var quesDetail = new QuestionMappedValue.QuestionDetail();
                                quesDetail.QuestionTitle = question.Question;
                                quesDetail.ShowTitle = question.ShowTitle;
                                quesDetail.QuestionImage = question.QuestionImage;

                                quesDetail.ObjectFieldsInAnswer = new List<QuestionMappedValue.QuestionDetail.ObjectFieldsDetail>();

                                foreach (var ans in question.AnswerOptionsInQuizQuestions) {

                                    if (ans.ObjectFieldsInAnswer != null && ans.ObjectFieldsInAnswer.Any()) {

                                        foreach (var objectFieldsInAnswerObj in ans.ObjectFieldsInAnswer) {
                                            if (objectFieldsInAnswerObj != null) {

                                                quesDetail.ObjectFieldsInAnswer.Add(new QuestionMappedValue.QuestionDetail.ObjectFieldsDetail {
                                                    ObjectName = objectFieldsInAnswerObj.ObjectName,
                                                    FieldName = objectFieldsInAnswerObj.FieldName,
                                                    Value = objectFieldsInAnswerObj.Value,
                                                    IsCommentMapped = objectFieldsInAnswerObj.IsCommentMapped.HasValue ? objectFieldsInAnswerObj.IsCommentMapped.Value : false,
                                                });
                                            }
                                        }
                                    }
                                }
                                quizQuestionObj.QuestionDetails.Add(quesDetail);
                            }
                        }
                    }                 
                }
                return quizQuestionObj;

            } catch (Exception ex) {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
            return quizQuestionObj;

        }

    }
}
