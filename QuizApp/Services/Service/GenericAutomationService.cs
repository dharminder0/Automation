using Core.Common.Caching;
using Newtonsoft.Json;
using QuizApp.Db;
using QuizApp.Helpers;
using QuizApp.RepositoryExtensions;
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
    public class GenericAutomationService : IGenericAutomationService
    {
        private ResultEnum status = ResultEnum.Ok;
        private string errormessage = string.Empty;
        private string leadInfoUpdateJson = "{'ContactId': '{ContactId}','ClientCode': '{ClientCode}','campaignName': '{campaignName}','appointmentStatus': '{appointmentStatus}','appointmentDate': '{appointmentDate}','appointmentTypeId': {appointmentTypeId},'appointmentTypeTitle': '{appointmentTypeTitle}','calendarId': {calendarId},'calendarTitle': '{calendarTitle}','appointmentBookedDate': '{appointmentBookedDate}','UserToken': '{UserToken}','SourceId': '{SourceId}'}";
        private string badgesInfoUpdateJson = "[ {'UserId': {UserId},'CourseId': '{CourseId}','CourseBadgeName': '{CourseBadgeName}','CourseBadgeImageUrl': '{CourseBadgeImageUrl}','CourseTitle': '{CourseTitle}'}]";

        public ResultEnum Status { get { return status; } set { status = value; } }
        public string ErrorMessage { get { return errormessage; } set { errormessage = value; } }

        public int CreateQuiz(LocalQuiz QuizObj, int BusinessUserId, bool IsCreateAcademyCourse, bool IsCreateTechnicalRecruiterCourse, int companyId)
        {
            int quizId = -1; int quizDetailId = -1;int quizType = 1;

            CreateQuizValidator createQuizValidator = new CreateQuizValidator();
            var validationResult = createQuizValidator.Validate(QuizObj);

            if (validationResult != null && validationResult.IsValid)
            {
                try
                {
                    var obj = new Db.Quiz();
                    var currentDate = DateTime.UtcNow;
                    obj.QuizType = (int)QuizObj.QuizType;
                    quizType = (int)QuizObj.QuizType;
                    obj.QuesAndContentInSameTable = true;
                    obj.State = (int)QuizStateEnum.DRAFTED;
                    obj.PublishedCode = Guid.NewGuid().ToString();
                    obj.CompanyId = companyId;


                    var quizDetails = new Db.QuizDetails();

                    switch (QuizObj.QuizType)
                    {
                        case QuizTypeEnum.Assessment:
                        case QuizTypeEnum.AssessmentTemplate:
                            quizDetails.QuizTitle = "Untitled Assessment";
                            //quizDetails.StartButtonText = "Take Quiz";
                            quizDetails.StartButtonText = "";
                            break;
                        case QuizTypeEnum.Score:
                        case QuizTypeEnum.ScoreTemplate:
                            quizDetails.QuizTitle = "Untitled Score";
                            //quizDetails.StartButtonText = "Take Score";
                            quizDetails.StartButtonText = "";
                            break;
                        case QuizTypeEnum.Personality:
                        case QuizTypeEnum.PersonalityTemplate:
                            quizDetails.QuizTitle = "Untitled Personality";
                            //quizDetails.StartButtonText = "Take Personality";
                            quizDetails.StartButtonText = "";
                            break;
                        case QuizTypeEnum.NPS:
                            quizDetails.QuizTitle = "Untitled NPS";
                            //quizDetails.StartButtonText = "Take NPS";
                            quizDetails.StartButtonText = "";
                            break;
                    }

                    quizDetails.QuizCoverTitle = "Untitled Cover";
                    quizDetails.ShowQuizCoverTitle = true;
                    quizDetails.ShowQuizCoverImage = true;
                    quizDetails.QuizDescription = "Type Description Here";
                    quizDetails.ShowDescription = true;
                    quizDetails.EnableNextButton = true;
                    quizDetails.DisplayOrderForTitleImage = 2;
                    quizDetails.DisplayOrderForTitle = 1;
                    quizDetails.DisplayOrderForDescription = 3;

                    quizDetails.DisplayOrderForNextButton = 4;
                    quizDetails.AutoPlay = true;


                    quizDetails.State = (int)QuizStateEnum.DRAFTED;
                    if (QuizObj.QuizType == QuizTypeEnum.AssessmentTemplate || QuizObj.QuizType == QuizTypeEnum.ScoreTemplate || QuizObj.QuizType == QuizTypeEnum.PersonalityTemplate)
                        quizDetails.Status = (int)StatusEnum.Inactive;
                    else
                        quizDetails.Status = (int)StatusEnum.Active;
                    quizDetails.Version = 1;
                    quizDetails.CreatedBy = BusinessUserId;
                    quizDetails.CreatedOn = currentDate;
                    quizDetails.LastUpdatedBy = BusinessUserId;
                    quizDetails.LastUpdatedOn = currentDate;
                    quizDetails.CompanyId = companyId;

                    var brandingAndStyle = new Db.QuizBrandingAndStyle();
                    brandingAndStyle.BackgroundColor = "#ffffff";
                    brandingAndStyle.ButtonColor = "#000000";
                    brandingAndStyle.OptionColor = "#F4F4F4";
                    brandingAndStyle.ButtonFontColor = "#ffffff";
                    brandingAndStyle.OptionFontColor = "#111111";
                    brandingAndStyle.FontColor = "#000000";
                    brandingAndStyle.ButtonHoverColor = "#000000";
                    brandingAndStyle.ButtonHoverTextColor = "#ffffff";
                    brandingAndStyle.BackgroundColorofSelectedAnswer = "#00B7AB";
                    brandingAndStyle.BackgroundColorofAnsweronHover = "#F9F9F9";
                    brandingAndStyle.AnswerTextColorofSelectedAnswer = "#FFFFFF";
                    brandingAndStyle.IsBackType = (int)BackTypeEnum.Color;
                    brandingAndStyle.BackImageFileURL = "";
                    brandingAndStyle.Opacity = "rgba(255, 255, 255, 0)";
                    brandingAndStyle.BackColor = "#ffffff";
                    brandingAndStyle.Language = 1;
                    brandingAndStyle.BackgroundColorofLogo = "#ffffff";
                    brandingAndStyle.AutomationAlignment = "Center";
                    brandingAndStyle.LogoAlignment = "Left";
                    brandingAndStyle.Flip = false;
                    brandingAndStyle.LastUpdatedBy = BusinessUserId;
                    brandingAndStyle.LastUpdatedOn = DateTime.UtcNow;


                    if (QuizObj.QuizType == QuizTypeEnum.AssessmentTemplate || QuizObj.QuizType == QuizTypeEnum.ScoreTemplate || QuizObj.QuizType == QuizTypeEnum.PersonalityTemplate)
                    {
                        using (var AUOWObj = new AutomationUnitOfWork())
                        {
                            var Category = AUOWObj.CategoryRepository.Get(r => r.CategoryId == QuizObj.CategoryId).FirstOrDefault();
                            if (Category != null)
                                obj.CategoryId = Category.Id;
                            else
                            {
                                Status = ResultEnum.Error;
                                ErrorMessage = "CategoryId not exist.";
                                return -1;
                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(QuizObj.AccessibleOfficeId))
                    {
                        obj.AccessibleOfficeId = QuizObj.AccessibleOfficeId;
                    }

                    using (var AUOWObj = new AutomationUnitOfWork())
                    {
                        using (var transaction = Utility.CreateTransactionScope())
                        {


                            #region adding quiz in parent table

                            AUOWObj.QuizRepository.Insert(obj);
                            AUOWObj.Save();
                            quizId = obj.Id;
                            #endregion

                            #region adding in child tables


                            quizDetails.ParentQuizId = obj.Id;
                            AUOWObj.QuizDetailsRepository.Insert(quizDetails);
                            AUOWObj.Save();
                            quizDetailId = quizDetails.Id;

                            #endregion

                            #region add brandingAndStyle

                            brandingAndStyle.QuizId = quizDetails.Id;
                            AUOWObj.QuizBrandingAndStyleRepository.Insert(brandingAndStyle);
                            AUOWObj.Save();

                            #endregion

                            #region add UserPermissionsInQuiz


                            if (string.IsNullOrEmpty(QuizObj.UserType))
                            {
                                var userPermissionsObj = new Db.UserPermissionsInQuiz()
                                {
                                    QuizId = obj.Id,
                                    UserTypeId = (int)UserTypeEnum.Lead
                                };
                                AUOWObj.UserPermissionsInQuizRepository.Insert(userPermissionsObj);
                                AUOWObj.Save();
                            }
                            else
                            {
                                var UserTypes = QuizObj.UserType.Split(',');

                                foreach (var userType in UserTypes.Where(a => !string.IsNullOrEmpty(a)).ToList())
                                {
                                    if (int.Parse(userType) == (int)UserTypeEnum.Lead || int.Parse(userType) == (int)UserTypeEnum.Recruiter || (IsCreateTechnicalRecruiterCourse && int.Parse(userType) == (int)UserTypeEnum.TechnicalRecruiter) || (IsCreateAcademyCourse && int.Parse(userType) == (int)UserTypeEnum.JobRockAcademy))
                                    {
                                        var userPermissionsObj = new Db.UserPermissionsInQuiz()
                                        {
                                            QuizId = obj.Id,
                                            UserTypeId = int.Parse(userType)
                                        };
                                        AUOWObj.UserPermissionsInQuizRepository.Insert(userPermissionsObj);
                                        AUOWObj.Save();
                                    }
                                };

                                if (IsCreateTechnicalRecruiterCourse && QuizObj.ModuleType.HasValue && QuizObj.ModuleType.Value > 0 && UserTypes.Any(r => !string.IsNullOrEmpty(r) && int.Parse(r) == (int)UserTypeEnum.TechnicalRecruiter))
                                {
                                    var modulePermissionsObj = new Db.ModulePermissionsInQuiz()
                                    {
                                        QuizId = obj.Id,
                                        ModuleTypeId = QuizObj.ModuleType.Value
                                    };
                                    AUOWObj.ModulePermissionsInQuizRepository.Insert(modulePermissionsObj);
                                    AUOWObj.Save();
                                };
                            }

                            #endregion

                            transaction.Complete();


                        }
                    }

                    if (quizId > 0 && quizDetailId > 0)
                    {
                        AddQuizQuestion(quizId, quizDetailId, BusinessUserId, companyId, (int)BranchingLogicEnum.QUESTION, quizType);
                        AddQuizResult(quizId, quizDetailId, BusinessUserId, companyId, quizType);

                    }
                        

                            
                            
                            
                            
                            
                            
                }
                catch (Exception ex)
                {
                    Status = ResultEnum.Error;
                    ErrorMessage = ex.Message;
                    throw ex;
                }
            }
            else
            {
                Status = ResultEnum.Error;
                ErrorMessage = "Validation error: " + String.Join(", ", validationResult.Errors.Select(r => r.ErrorMessage));
            }
            return quizId;
        }



        public QuizQuestion AddQuizQuestion(int QuizId, int quizDetailId, int BusinessUserId, int CompanyId, int Type, int quizType)
        {
            QuizQuestion quizQuestionObj = null;
            var obj = new Db.QuestionsInQuiz();
            var answerObj = new Db.AnswerOptionsInQuizQuestions();
            var answerObjNPS = new Db.AnswerOptionsInQuizQuestions();
            var answerObjNPS2 = new Db.AnswerOptionsInQuizQuestions();
            obj.QuizId = quizDetailId;
            obj.ShowAnswerImage = false;

            obj.ShowTitle = true;
            obj.ShowQuestionImage = true;
            obj.QuestionImage = string.Empty;
            obj.PublicId = string.Empty;
            obj.Status = (int)StatusEnum.Active;
            obj.State = (int)QuizStateEnum.DRAFTED;
            obj.AnswerType = quizType == (int)QuizTypeEnum.NPS ? (int)AnswerTypeEnum.NPS : (int)AnswerTypeEnum.Single;
            //obj.NextButtonText = "Next";
            obj.NextButtonText = "";
            obj.NextButtonTxtSize = "24px";

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

            using (var AUOWObj = new AutomationUnitOfWork())
            {
                var quizDetails = AUOWObj.QuizDetailsRepository.GetQuizDetailsbyId(quizDetailId).FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED);
                var contentsInQuiz = AUOWObj.ContentsInQuizRepository.GetContentInQuizRepositoryExtension(quizDetailId).Where(a => a.Status == (int)StatusEnum.Active);
                var questionInQuiz = AUOWObj.QuestionsInQuizRepository.GetExistingQuestioninQuiz(quizDetailId).Where(a => a.Status == (int)StatusEnum.Active);
                var contentsInQuizCount = contentsInQuiz.Count();
                var questionInQuizCount = questionInQuiz.Count();
                var quizDescription = AUOWObj.UsageTypeInQuizRepository.GetUsageTypeInQuizRepositoryExtension(QuizId).Any(a => a.UsageType == (int)UsageTypeEnum.WhatsAppChatbot) ? "Message" : "Description";
                var existingQuestionInQuiz = AUOWObj.QuestionsInQuizRepository.GetExistingQuestioninQuiz(quizDetails.Id).FirstOrDefault();

                if (Type == (int)BranchingLogicEnum.QUESTION)
                    obj.Question = "Question " + (questionInQuiz.Count(r => r.Type == (int)BranchingLogicEnum.QUESTION) + 1).ToString();
                else if (Type == (int)BranchingLogicEnum.CONTENT)
                    obj.Question = "Content " + (questionInQuiz.Count(r => r.Type == (int)BranchingLogicEnum.CONTENT) + 1).ToString();
                obj.ViewPreviousQuestion = quizDetails.ViewPreviousQuestion;
                obj.EditAnswer = quizDetails.EditAnswer;
                obj.Description = quizDescription;

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
                else if (quizType != (int)QuizTypeEnum.NPS && quizType != (int)QuizTypeEnum.Score && quizType != (int)QuizTypeEnum.ScoreTemplate && quizType != (int)QuizTypeEnum.Personality && quizType != (int)QuizTypeEnum.PersonalityTemplate)
                {
                    //obj.AliasTextForCorrect = "Correct";
                    //obj.AliasTextForIncorrect = "Incorrect";
                    //obj.AliasTextForYourAnswer = "Your Answer";
                    //obj.AliasTextForCorrectAnswer = "Correct Answer";
                    //obj.AliasTextForExplanation = "Explanation";
                    //obj.AliasTextForNextButton = "Next";

                    obj.RevealCorrectAnswer = false;
                    obj.AliasTextForCorrect = "";
                    obj.AliasTextForIncorrect = "";
                    obj.AliasTextForYourAnswer = "";
                    obj.AliasTextForCorrectAnswer = "";
                    obj.AliasTextForExplanation = "";
                    obj.AliasTextForNextButton = "";
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
                obj.LastUpdatedOn = DateTime.UtcNow;

                if (quizType == (int)QuizTypeEnum.Score || quizType == (int)QuizTypeEnum.ScoreTemplate || quizType == (int)QuizTypeEnum.Personality || quizType == (int)QuizTypeEnum.PersonalityTemplate)
                {
                    obj.MinAnswer = 1;
                    obj.MaxAnswer = 1;
                }



               
                answerObj.Option = quizType == (int)QuizTypeEnum.NPS ? null : "Answer 1";
                answerObj.Description = "";
                answerObj.OptionImage = string.Empty;
                answerObj.PublicId = string.Empty;
                answerObj.LastUpdatedBy = BusinessUserId;
                answerObj.LastUpdatedOn = DateTime.UtcNow;

                if (quizType == (int)QuizTypeEnum.Score || quizType == (int)QuizTypeEnum.ScoreTemplate)
                {
                    answerObj.AssociatedScore = default(int);
                    answerObj.IsCorrectAnswer = false;
                }
                else if (quizType == (int)QuizTypeEnum.Personality || quizType == (int)QuizTypeEnum.PersonalityTemplate)
                    answerObj.IsCorrectAnswer = false;
                else if (quizType == (int)QuizTypeEnum.NPS)
                {
                    answerObj.IsCorrectAnswer = null;
                    answerObj.IsCorrectForMultipleAnswer = null;
                }
                else
                    answerObj.IsCorrectAnswer = true;

                answerObj.DisplayOrder = 1;
                answerObj.Status = (int)StatusEnum.Active;
                answerObj.State = (int)QuizStateEnum.DRAFTED;
                answerObj.AutoPlay = true;
                answerObj.SecondsToApply = "0";
                answerObj.VideoFrameEnabled = false;
                answerObj.QuizId = obj.QuizId;

                if (quizType != (int)QuizTypeEnum.NPS)
                {
                   

                   answerObjNPS.QuestionId = obj.Id;
                   answerObjNPS.QuizId = obj.QuizId;
                   answerObjNPS.Option = "Answer 2";
                   answerObjNPS.Description = "";
                   answerObjNPS.OptionImage = string.Empty;
                   answerObjNPS.PublicId = string.Empty;
                   answerObjNPS.LastUpdatedBy = BusinessUserId;
                   answerObjNPS.LastUpdatedOn = DateTime.UtcNow;

                    if (quizType == (int)QuizTypeEnum.Score || quizType == (int)QuizTypeEnum.ScoreTemplate)
                    {
                        answerObjNPS.AssociatedScore = default(int);
                        answerObjNPS.IsCorrectAnswer = false;
                    }
                    else
                        answerObjNPS.IsCorrectAnswer = false;

                    answerObjNPS.DisplayOrder = 2;
                    answerObjNPS.Status = (int)StatusEnum.Active;
                    answerObjNPS.State = (int)QuizStateEnum.DRAFTED;
                    answerObjNPS.AutoPlay = true;
                    answerObjNPS.SecondsToApply = "0";
                    answerObjNPS.VideoFrameEnabled = false;

                    


                    #region for unanswered answer option

                   

                    answerObjNPS2.QuestionId = obj.Id;
                    answerObjNPS2.QuizId = obj.QuizId;
                    answerObjNPS2.Option = "Unanswered";
                    answerObjNPS2.OptionImage = string.Empty;
                    answerObjNPS2.PublicId = string.Empty;
                    answerObjNPS2.LastUpdatedBy = BusinessUserId;
                    answerObjNPS2.LastUpdatedOn = DateTime.UtcNow;

                    if (quizType == (int)QuizTypeEnum.Score || quizType == (int)QuizTypeEnum.ScoreTemplate)
                    {
                        answerObjNPS2.AssociatedScore = default(int);
                        answerObjNPS2.IsCorrectAnswer = false;
                    }
                    else
                        answerObjNPS2.IsCorrectAnswer = false;

                    answerObjNPS2.DisplayOrder = 0;
                    answerObjNPS2.Status = (int)StatusEnum.Active;
                    answerObjNPS2.State = (int)QuizStateEnum.DRAFTED;
                    answerObjNPS2.IsUnansweredType = true;
                    answerObjNPS2.AutoPlay = true;
                    answerObjNPS2.SecondsToApply = "0";
                    answerObjNPS2.VideoFrameEnabled = false;
                    #endregion
                }

            }

            try
            {
                int quizObjQuizType = 0;
                using (var AUOWObj = new AutomationUnitOfWork())
                {
                    using (var transaction = Utility.CreateTransactionScope())
                    {
                        AUOWObj.QuestionsInQuizRepository.Insert(obj);
                        AUOWObj.Save();

                        answerObj.QuestionId = obj.Id;
                        AUOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);
                        AUOWObj.Save();

                        if (quizType != (int)QuizTypeEnum.NPS)
                        {
                            answerObjNPS.QuestionId = obj.Id;
                            AUOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObjNPS);


                            #region for unanswered answer option

                            answerObjNPS2.QuestionId = obj.Id;
                            AUOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObjNPS2);

                            #endregion
                        }

                        var quizDetails = AUOWObj.QuizDetailsRepository.GetQuizDetailsbyId(quizDetailId).FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED);
                        quizDetails.LastUpdatedBy = BusinessUserId;
                        quizDetails.LastUpdatedOn = DateTime.UtcNow;
                        AUOWObj.QuizDetailsRepository.Update(quizDetails);

                        var quizObj = AUOWObj.QuizRepository.GetByID(QuizId);
                        quizObj.State = (int)QuizStateEnum.DRAFTED;
                        AUOWObj.QuizRepository.Update(quizObj);
                        AUOWObj.Save();
                        transaction.Complete();

                        quizObjQuizType = quizObj.QuizType;
                    }

                        #region Bind return obj

                        quizQuestionObj = new QuizQuestion();

                        quizQuestionObj.QuizType = quizObjQuizType;
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
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }


                
                

            return quizQuestionObj;

        }


            public QuizQuestion AddQuizQuestionOLd(int QuizId, int BusinessUserId, int CompanyId, int Type)
        {
            QuizQuestion quizQuestionObj = null;
            try
            {
                using (var AUOWObj = new AutomationUnitOfWork())
                {
                    using (var transaction = Utility.CreateTransactionScope())
                    {
                        var quizObj = AUOWObj.QuizRepository.GetByID(QuizId);

                        if (quizObj != null && quizObj.QuizDetails.Any())
                        {
                            var quizDetails = quizObj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED);

                            if (quizDetails != null)
                            {
                                #region Adding obj

                                var currentDate = DateTime.UtcNow;

                                //var existingQuestionInQuiz = quizDetails.QuestionsInQuiz.FirstOrDefault();

                                var existingQuestionInQuiz = AUOWObj.QuestionsInQuizRepository.GetExistingQuestioninQuiz(quizDetails.Id).FirstOrDefault();

                                var contentsInQuiz = AUOWObj.ContentsInQuizRepository.GetContentInQuizRepositoryExtension(quizDetails.Id).Where(a => a.Status == (int)StatusEnum.Active);
                                //var contentsInQuiz = quizDetails.ContentsInQuiz.Where(a => a.Status == (int)StatusEnum.Active);
                                var questionInQuiz = AUOWObj.QuestionsInQuizRepository.GetExistingQuestioninQuiz(quizDetails.Id).Where(a => a.Status == (int)StatusEnum.Active);
                                //var questionInQuiz = quizDetails.QuestionsInQuiz.Where(a => a.Status == (int)StatusEnum.Active);
                                var contentsInQuizCount = contentsInQuiz.Count();
                                var questionInQuizCount = questionInQuiz.Count();


                                var obj = new Db.QuestionsInQuiz();

                                obj.QuizId = quizDetails.Id;
                                obj.ShowAnswerImage = false;
                                if (Type == (int)BranchingLogicEnum.QUESTION)
                                    obj.Question = "Question " + (questionInQuiz.Count(r => r.Type == (int)BranchingLogicEnum.QUESTION) + 1).ToString();
                                else if (Type == (int)BranchingLogicEnum.CONTENT)
                                    obj.Question = "Content " + (questionInQuiz.Count(r => r.Type == (int)BranchingLogicEnum.CONTENT) + 1).ToString();
                                obj.ShowTitle = true;
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
                                obj.Description = AUOWObj.UsageTypeInQuizRepository.GetUsageTypeInQuizRepositoryExtension(quizObj.Id).Any(a => a.UsageType == (int)UsageTypeEnum.WhatsAppChatbot) ? "Message" : "";
                                //obj.Description = quizObj.UsageTypeInQuiz.Any(a => a.UsageType == (int)UsageTypeEnum.WhatsAppChatbot) ? "Message" : "Description";
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
                                else if (quizObj.QuizType != (int)QuizTypeEnum.NPS && quizObj.QuizType != (int)QuizTypeEnum.Score && quizObj.QuizType != (int)QuizTypeEnum.ScoreTemplate && quizObj.QuizType != (int)QuizTypeEnum.Personality && quizObj.QuizType != (int)QuizTypeEnum.PersonalityTemplate)
                                {
                                    obj.RevealCorrectAnswer = false;
                                    obj.AliasTextForCorrect = "Correct";
                                    obj.AliasTextForIncorrect = "Incorrect";
                                    obj.AliasTextForYourAnswer = "Your Answer";
                                    obj.AliasTextForCorrectAnswer = "Correct Answer";
                                    obj.AliasTextForExplanation = "Explanation";
                                    obj.AliasTextForNextButton = "Next";
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

                                AUOWObj.QuestionsInQuizRepository.Insert(obj);
                                AUOWObj.Save();

                                var answerObj = new Db.AnswerOptionsInQuizQuestions();

                                answerObj.QuestionId = obj.Id;
                                answerObj.QuizId = obj.QuizId;
                                answerObj.Option = quizObj.QuizType == (int)QuizTypeEnum.NPS ? null : "Answer 1";
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
                                else if (quizObj.QuizType == (int)QuizTypeEnum.NPS)
                                {
                                    answerObj.IsCorrectAnswer = null;
                                    answerObj.IsCorrectForMultipleAnswer = null;
                                }
                                else
                                    answerObj.IsCorrectAnswer = true;

                                answerObj.DisplayOrder = 1;
                                answerObj.Status = (int)StatusEnum.Active;
                                answerObj.State = (int)QuizStateEnum.DRAFTED;
                                answerObj.AutoPlay = true;
                                answerObj.SecondsToApply = "0";
                                answerObj.VideoFrameEnabled = false;

                                AUOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);

                                if (quizObj.QuizType != (int)QuizTypeEnum.NPS)
                                {
                                    answerObj = new Db.AnswerOptionsInQuizQuestions();

                                    answerObj.QuestionId = obj.Id;
                                    answerObj.QuizId = obj.QuizId;
                                    answerObj.Option = "Answer 2";
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

                                    AUOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);
                                }

                                quizDetails.LastUpdatedBy = BusinessUserId;
                                quizDetails.LastUpdatedOn = currentDate;

                                quizObj.State = (int)QuizStateEnum.DRAFTED;


                                if (quizObj.QuizType == (int)QuizTypeEnum.Score || quizObj.QuizType == (int)QuizTypeEnum.ScoreTemplate || quizObj.QuizType == (int)QuizTypeEnum.Personality || quizObj.QuizType == (int)QuizTypeEnum.PersonalityTemplate)
                                {
                                    obj.MinAnswer = 1;
                                    obj.MaxAnswer = 1;
                                }
                                AUOWObj.QuestionsInQuizRepository.Update(obj);

                                if (quizObj.QuizType != (int)QuizTypeEnum.NPS)
                                {
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

                                    AUOWObj.AnswerOptionsInQuizQuestionsRepository.Insert(answerObj);

                                    #endregion
                                }

                                AUOWObj.QuizRepository.Update(quizObj);

                                AUOWObj.Save();
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


        public QuizResult AddQuizResult(int QuizId, int quizDetailId, int BusinessUserId, int CompanyId, int? quizType = 0)
        {
            QuizResult quizResult = null;
            var resultObj = new Db.QuizResults();
            var resultObjPersonality = new Db.QuizResults();
            var resultObjPersonality2 = new Db.QuizResults();
            var multipleresultPersonality = new Db.PersonalityResultSetting();
            var resultObjNPS = new Db.QuizResults();
            var resultObjNPS2 = new Db.QuizResults();
            Db.ResultSettings resultSetting = null;
            var currentDate = DateTime.UtcNow;

            try
            {
                using (var AUOWObj = new AutomationUnitOfWork())
                {
                    var quizDetails = AUOWObj.QuizDetailsRepository.GetQuizDetailsbyId(quizDetailId).FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED);
                    if (quizDetails != null)
                    {

                        resultObj.QuizId = quizDetails.Id;
                        resultObj.Title = quizType == (int)QuizTypeEnum.NPS ? "Detractor" : "Result " + (AUOWObj.QuizResultsRepository.GetQuizResultsRepositoryExtension(quizDetails.Id).Where(r => r.State == (int)StatusEnum.Active).Count() + 1).ToString();
                        resultObj.InternalTitle = quizType == (int)QuizTypeEnum.NPS ? "Detractor" : "Result " + (AUOWObj.QuizResultsRepository.GetQuizResultsRepositoryExtension(quizDetails.Id).Where(r => r.State == (int)StatusEnum.Active).Count() + 1).ToString();
                        resultObj.ShowResultImage = true;
                        resultObj.Description = "Result Description";
                        resultObj.HideCallToAction = false;
                        resultObj.OpenLinkInNewTab = true;
                        resultObj.ActionButtonTxtSize = "24 px";
                        //resultObj.ActionButtonText = "Call To Action";
                        resultObj.ActionButtonText = "";
                        resultObj.Status = (int)StatusEnum.Active;
                        resultObj.LastUpdatedBy = BusinessUserId;
                        resultObj.LastUpdatedOn = currentDate;
                        resultObj.State = (int)QuizStateEnum.DRAFTED;
                        resultObj.ShowLeadUserForm = false;
                        resultObj.AutoPlay = true;
                        resultObj.SecondsToApply = quizDetails.SecondsToApply;
                        resultObj.VideoFrameEnabled = quizDetails.VideoFrameEnabled;
                        resultObj.ShowExternalTitle = true;
                        resultObj.ShowInternalTitle = true;
                        resultObj.ShowDescription = true;
                        resultObj.EnableMediaFile = false;
                        resultObj.DisplayOrderForTitleImage = 2;
                        resultObj.DisplayOrderForTitle = 1;
                        resultObj.DisplayOrderForDescription = 3;
                        resultObj.DisplayOrderForNextButton = 4;

                        if (quizType == (int)QuizTypeEnum.Assessment || quizType == (int)QuizTypeEnum.AssessmentTemplate)
                        {
                            resultObj.MinScore = 0;
                            resultObj.MaxScore = 1;
                        }
                        else if (quizType == (int)QuizTypeEnum.Personality || quizType == (int)QuizTypeEnum.PersonalityTemplate)
                        {
                            resultObj.DisplayOrder = 1;
                        }
                        else if (quizType == (int)QuizTypeEnum.NPS)
                        {
                            resultObj.DisplayOrder = 1;
                            resultObj.MinScore = 0;
                            resultObj.MaxScore = 6;
                        }
                        else
                        {
                            resultObj.MinScore = 0;
                            resultObj.MaxScore = 0;
                        }

                        if (quizType == (int)QuizTypeEnum.Personality || quizType == (int)QuizTypeEnum.PersonalityTemplate)
                        {

                            resultObjPersonality.QuizId = quizDetails.Id;
                            //resultObjPersonality.Title = "Result " + (AUOWObj.QuizResultsRepository.GetQuizResultsRepositoryExtension(quizDetails.Id).Where(r => r.State == (int)StatusEnum.Active).Count() + 1).ToString();
                            ////resultObj.Title = "Result " + (quizDetails.QuizResults.Where(r => r.State == (int)StatusEnum.Active).Count() + 1).ToString();
                            //resultObjPersonality.InternalTitle = "Result " + (AUOWObj.QuizResultsRepository.GetQuizResultsRepositoryExtension(quizDetails.Id).Where(r => r.State == (int)StatusEnum.Active).Count() + 1).ToString();
                            ////resultObj.InternalTitle = "Result " + (quizDetails.QuizResults.Where(r => r.State == (int)StatusEnum.Active).Count() + 1).ToString();
                            resultObjPersonality.ShowResultImage = true;
                            resultObjPersonality.Description = "Result Description";
                            resultObjPersonality.HideCallToAction = false;
                            resultObjPersonality.OpenLinkInNewTab = true;
                            resultObjPersonality.ActionButtonTxtSize = "24 px";
                            //resultObjPersonality.ActionButtonText = "Call To Action";
                            resultObjPersonality.ActionButtonText = "";
                            resultObjPersonality.Status = (int)StatusEnum.Active;
                            resultObjPersonality.LastUpdatedBy = BusinessUserId;
                            resultObjPersonality.LastUpdatedOn = currentDate;
                            resultObjPersonality.State = (int)QuizStateEnum.DRAFTED;
                            resultObjPersonality.DisplayOrder = 2;
                            resultObjPersonality.MinScore = null;
                            resultObjPersonality.MaxScore = null;
                            resultObjPersonality.ShowLeadUserForm = false;
                            resultObjPersonality.ShowExternalTitle = true;
                            resultObjPersonality.ShowInternalTitle = true;
                            resultObjPersonality.ShowDescription = true;
                            resultObjPersonality.EnableMediaFile = false;
                            resultObjPersonality.DisplayOrderForTitle = 1;
                            resultObjPersonality.DisplayOrderForTitleImage = 2;
                            resultObjPersonality.DisplayOrderForDescription = 3;
                            resultObjPersonality.DisplayOrderForNextButton = 4;

                            resultObjPersonality2.QuizId = quizDetails.Id;
                            resultObjPersonality2.Title = "Based On Correlations";
                            resultObjPersonality2.InternalTitle = "Based On Correlations";
                            resultObjPersonality2.ShowResultImage = false;
                            resultObjPersonality2.Description = "Calculated Result";
                            resultObjPersonality2.HideCallToAction = false;
                            resultObjPersonality2.OpenLinkInNewTab = false;
                            resultObjPersonality2.ActionButtonTxtSize = "24 px";
                           //resultObjPersonality2.ActionButtonText = "Call To Action";
                            resultObjPersonality2.ActionButtonText = "";
                            resultObjPersonality2.Status = (int)StatusEnum.Active;
                            resultObjPersonality2.LastUpdatedBy = BusinessUserId;
                            resultObjPersonality2.LastUpdatedOn = currentDate;
                            resultObjPersonality2.State = (int)QuizStateEnum.DRAFTED;
                            resultObjPersonality2.IsPersonalityCorrelatedResult = true;
                            resultObjPersonality2.DisplayOrder = 3;
                            resultObjPersonality2.MinScore = null;
                            resultObjPersonality2.MaxScore = null;
                            resultObjPersonality2.ShowLeadUserForm = false;
                            resultObjPersonality2.ShowExternalTitle = true;
                            resultObjPersonality2.ShowInternalTitle = true;
                            resultObjPersonality2.ShowDescription = true;
                            resultObjPersonality2.EnableMediaFile = false;
                            resultObjPersonality2.DisplayOrderForTitle = 1;
                            resultObjPersonality2.DisplayOrderForTitleImage = 2;
                            resultObjPersonality2.DisplayOrderForDescription = 3;
                            resultObjPersonality2.DisplayOrderForNextButton = 4;

                            multipleresultPersonality.QuizId = quizDetails.Id;
                            multipleresultPersonality.Title = "Your Top Results";
                            multipleresultPersonality.Status = (int)StatusEnum.Inactive;
                            multipleresultPersonality.GraphColor = "#417341";
                            multipleresultPersonality.MaxResult = 2;
                            multipleresultPersonality.ButtonFontColor = "#ffffff";
                            multipleresultPersonality.SideButtonText = "More Details";
                            multipleresultPersonality.IsFullWidthEnable = false;
                            multipleresultPersonality.LastUpdatedOn = DateTime.UtcNow;
                            multipleresultPersonality.LastUpdatedBy = BusinessUserId;
                            multipleresultPersonality.ShowLeadUserForm = false;


                        }

                        if (quizType == (int)QuizTypeEnum.NPS)
                        {


                            resultObjNPS.QuizId = quizDetails.Id;
                            resultObjNPS.Title = "Passive";
                            resultObjNPS.InternalTitle = "Passive";
                            resultObjNPS.ShowResultImage = true;
                            resultObjNPS.Description = "Result Description";
                            resultObjNPS.HideCallToAction = false;
                            resultObjNPS.OpenLinkInNewTab = true;
                            resultObjNPS.ActionButtonTxtSize = "24 px";
                            //resultObjNPS.ActionButtonText = "Call To Action";
                            resultObjNPS.ActionButtonText = "";
                            resultObjNPS.Status = (int)StatusEnum.Active;
                            resultObjNPS.LastUpdatedBy = BusinessUserId;
                            resultObjNPS.LastUpdatedOn = currentDate;
                            resultObjNPS.State = (int)QuizStateEnum.DRAFTED;
                            resultObjNPS.DisplayOrder = 2;
                            resultObjNPS.MinScore = 7;
                            resultObjNPS.MaxScore = 8;
                            resultObjNPS.ShowLeadUserForm = false;
                            resultObjNPS.DisplayOrderForTitle = 1;
                            resultObjNPS.DisplayOrderForTitleImage = 2;
                            resultObjNPS.DisplayOrderForDescription = 3;
                            resultObjNPS.DisplayOrderForNextButton = 4;
                            resultObjNPS.ShowExternalTitle = true;
                            resultObjNPS.ShowInternalTitle = true;
                            resultObjNPS.ShowDescription = true;
                            resultObjNPS.EnableMediaFile = false;

                            resultObjNPS2.QuizId = quizDetails.Id;
                            resultObjNPS2.Title = "Promoter";
                            resultObjNPS2.InternalTitle = "Promoter";
                            resultObjNPS2.ShowResultImage = true;
                            resultObjNPS2.Description = "Result Description";
                            resultObjNPS2.HideCallToAction = false;
                            resultObjNPS2.OpenLinkInNewTab = true;
                            resultObjNPS2.ActionButtonTxtSize = "24 px";
                            //resultObjNPS2.ActionButtonText = "Call To Action";
                            resultObjNPS2.ActionButtonText = "";
                            resultObjNPS2.Status = (int)StatusEnum.Active;
                            resultObjNPS2.LastUpdatedBy = BusinessUserId;
                            resultObjNPS2.LastUpdatedOn = currentDate;
                            resultObjNPS2.State = (int)QuizStateEnum.DRAFTED;
                            resultObjNPS2.DisplayOrder = 3;
                            resultObjNPS2.MinScore = 9;
                            resultObjNPS2.MaxScore = 10;
                            resultObjNPS2.ShowLeadUserForm = false;
                            resultObjNPS2.DisplayOrderForTitle = 1;
                            resultObjNPS2.DisplayOrderForTitleImage = 2;
                            resultObjNPS2.DisplayOrderForDescription = 3;
                            resultObjNPS2.DisplayOrderForNextButton = 4;
                            resultObjNPS2.ShowExternalTitle = true;
                            resultObjNPS2.ShowInternalTitle = true;
                            resultObjNPS2.ShowDescription = true;
                            resultObjNPS2.EnableMediaFile = false;


                        }
                        //var resultSetting = quizDetails.ResultSettings.FirstOrDefault();
                
                    }
                }


                using (var AUOWObj = new AutomationUnitOfWork())
                {
                    using (var transaction = Utility.CreateTransactionScope())
                    {
                        AUOWObj.QuizResultsRepository.Insert(resultObj);
                        AUOWObj.Save();
                        if (quizType == (int)QuizTypeEnum.Personality || quizType == (int)QuizTypeEnum.PersonalityTemplate)
                        {

                            resultObjPersonality.Title = "Result " + (AUOWObj.QuizResultsRepository.GetQuizResultsRepositoryExtension(quizDetailId).Where(r => r.State == (int)StatusEnum.Active).Count() + 1).ToString();
                            //resultObj.Title = "Result " + (quizDetails.QuizResults.Where(r => r.State == (int)StatusEnum.Active).Count() + 1).ToString();
                            resultObjPersonality.InternalTitle = "Result " + (AUOWObj.QuizResultsRepository.GetQuizResultsRepositoryExtension(quizDetailId).Where(r => r.State == (int)StatusEnum.Active).Count() + 1).ToString();
                            //resultObj.InternalTitle = "Result " + (quizDetails.QuizResults.Where(r => r.State == (int)StatusEnum.Active).Count() + 1).ToString();

                            AUOWObj.QuizResultsRepository.Insert(resultObjPersonality);
                            AUOWObj.Save();

                            AUOWObj.QuizResultsRepository.Insert(resultObjPersonality2);
                            AUOWObj.Save();

                            AUOWObj.PersonalityResultSettingRepository.Insert(multipleresultPersonality);
                            AUOWObj.Save();
                        }
                        if (quizType == (int)QuizTypeEnum.NPS)
                        {
                            AUOWObj.QuizResultsRepository.Insert(resultObjNPS);
                            AUOWObj.Save();

                            AUOWObj.QuizResultsRepository.Insert(resultObjNPS2);
                            AUOWObj.Save();
                        }

                        resultSetting = AUOWObj.ResultSettingsRepository.GetResultSettingsRepositoryExtension(quizDetailId).FirstOrDefault();

                        if (resultSetting == null)
                        {
                            resultSetting = new Db.ResultSettings();

                            resultSetting.QuizId = quizDetailId;
                            resultSetting.ShowScoreValue = false;
                            resultSetting.ShowCorrectAnswer = false;
                            resultSetting.LastUpdatedBy = BusinessUserId;
                            resultSetting.LastUpdatedOn = currentDate;
                            resultSetting.State = (int)QuizStateEnum.DRAFTED;

                            if (quizType == (int)QuizTypeEnum.Score || quizType == (int)QuizTypeEnum.ScoreTemplate)
                                resultSetting.CustomTxtForScoreValueInResult = "You scored a%score%";
                            else if (quizType == (int)QuizTypeEnum.NPS)
                            {
                                resultSetting.CustomTxtForScoreValueInResult = string.Empty;
                                resultSetting.ShowScoreValue = false;
                            }
                            else
                                resultSetting.CustomTxtForScoreValueInResult = "I got%score%out of%total%Correct";

                            resultSetting.CustomTxtForAnswerKey = "Answer Key";
                            resultSetting.CustomTxtForYourAnswer = "Your Answer";
                            resultSetting.CustomTxtForCorrectAnswer = "Correct Answer";
                            resultSetting.CustomTxtForExplanation = "Explanation";

                        }

                        if (resultSetting != null)
                        {
                            AUOWObj.ResultSettingsRepository.Insert(resultSetting);
                            AUOWObj.Save();
                        }

                        var quizDetails = AUOWObj.QuizDetailsRepository.GetQuizDetailsbyId(quizDetailId).FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED);
                        quizDetails.LastUpdatedBy = BusinessUserId;
                        quizDetails.LastUpdatedOn = currentDate;

                        AUOWObj.QuizDetailsRepository.Update(quizDetails);

                        var quizObj = AUOWObj.QuizRepository.GetByID(QuizId);
                        quizObj.State = (int)QuizStateEnum.DRAFTED;
                        AUOWObj.QuizRepository.Update(quizObj);
                        AUOWObj.Save();
                        transaction.Complete();

                        #region Bind return obj


                        quizResult = new QuizResult();

                        quizResult.ResultId = resultObj.Id;
                        quizResult.Title = resultObj.Title;
                        quizResult.InternalTitle = resultObj.InternalTitle;
                        quizResult.ShowResultImage = resultObj.ShowResultImage;
                        quizResult.Description = resultObj.Description;
                        quizResult.HideCallToAction = resultObj.HideCallToAction;
                        quizResult.EnableCallToActionButton = !(resultObj.HideCallToAction ?? false); ;
                        quizResult.ActionButtonURL = resultObj.ActionButtonURL;
                        quizResult.OpenLinkInNewTab = resultObj.OpenLinkInNewTab;
                        quizResult.ActionButtonTxtSize = resultObj.ActionButtonTxtSize;
                        quizResult.ActionButtonColor = resultObj.ActionButtonColor;
                        quizResult.ActionButtonTitleColor = resultObj.ActionButtonTitleColor;
                        quizResult.ActionButtonText = resultObj.ActionButtonText;
                        quizResult.ShowExternalTitle = resultObj.ShowExternalTitle;
                        quizResult.ShowInternalTitle = resultObj.ShowInternalTitle;
                        quizResult.ShowDescription = resultObj.ShowDescription;
                        quizResult.EnableMediaFile = resultObj.EnableMediaFile;
                        quizResult.DisplayOrderForTitleImage = resultObj.DisplayOrderForTitleImage;
                        quizResult.DisplayOrderForTitle = resultObj.DisplayOrderForTitle;
                        quizResult.DisplayOrderForDescription = resultObj.DisplayOrderForDescription;
                        quizResult.DisplayOrderForNextButton = resultObj.DisplayOrderForNextButton;

                        quizResult.ResultSetting = new QuizResultSetting();

                        quizResult.ResultSetting.QuizId = QuizId;
                        quizResult.ResultSetting.ShowScoreValue = resultSetting.ShowScoreValue;
                        quizResult.ResultSetting.ShowCorrectAnswer = resultSetting.ShowCorrectAnswer;
                        quizResult.ResultSetting.CustomTxtForScoreValueInResult = resultSetting.CustomTxtForScoreValueInResult;
                        quizResult.ResultSetting.CustomTxtForAnswerKey = resultSetting.CustomTxtForAnswerKey;
                        quizResult.ResultSetting.CustomTxtForYourAnswer = resultSetting.CustomTxtForYourAnswer;
                        quizResult.ResultSetting.CustomTxtForCorrectAnswer = resultSetting.CustomTxtForCorrectAnswer;
                        quizResult.ResultSetting.CustomTxtForExplanation = resultSetting.CustomTxtForExplanation;

                        #endregion

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


        public QuizResult AddQuizResultOld(int QuizId, int BusinessUserId, int CompanyId, int? quizType = 0)
        {
            QuizResult quizResult = null;
            try
            {
                using (var AUOWObj = new AutomationUnitOfWork())
                {
                    using (var transaction = Utility.CreateTransactionScope())
                    {
                        var quizObj = AUOWObj.QuizRepository.GetByID(QuizId);

                        if (quizObj != null)
                        {
                            var quizDetails = quizObj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED);

                            if (quizDetails != null)
                            {
                                var currentDate = DateTime.UtcNow;

                                #region Adding obj

                                var resultObj = new Db.QuizResults();

                                resultObj.QuizId = quizDetails.Id;
                                resultObj.Title = quizType == (int)QuizTypeEnum.NPS ? "Detractor" : "Result " + (AUOWObj.QuizResultsRepository.GetQuizResultsRepositoryExtension(quizDetails.Id).Where(r => r.State == (int)StatusEnum.Active).Count() + 1).ToString();
                                //resultObj.Title = quizType == (int)QuizTypeEnum.NPS ? "Detractor" : "Result " + (quizDetails.QuizResults.Where(r => r.State == (int)StatusEnum.Active).Count() + 1).ToString();
                                resultObj.InternalTitle = quizType == (int)QuizTypeEnum.NPS ? "Detractor" : "Result " + (AUOWObj.QuizResultsRepository.GetQuizResultsRepositoryExtension(quizDetails.Id).Where(r => r.State == (int)StatusEnum.Active).Count() + 1).ToString();
                                //resultObj.InternalTitle = quizType == (int)QuizTypeEnum.NPS ? "Detractor" : "Result " + (quizDetails.QuizResults.Where(r => r.State == (int)StatusEnum.Active).Count() + 1).ToString();
                                resultObj.ShowResultImage = true;
                                resultObj.Description = "Result Description";
                                resultObj.HideCallToAction = false;
                                resultObj.OpenLinkInNewTab = true;
                                resultObj.ActionButtonTxtSize = "24 px";
                                resultObj.ActionButtonText = "Call To Action";
                                resultObj.Status = (int)StatusEnum.Active;
                                resultObj.LastUpdatedBy = BusinessUserId;
                                resultObj.LastUpdatedOn = currentDate;
                                resultObj.State = (int)QuizStateEnum.DRAFTED;
                                resultObj.ShowLeadUserForm = false;
                                resultObj.AutoPlay = true;
                                resultObj.SecondsToApply = quizDetails.SecondsToApply;
                                resultObj.VideoFrameEnabled = quizDetails.VideoFrameEnabled;
                                resultObj.ShowExternalTitle = true;
                                resultObj.ShowInternalTitle = true;
                                resultObj.ShowDescription = true;
                                resultObj.EnableMediaFile = false;
                                resultObj.DisplayOrderForTitleImage = 2;
                                resultObj.DisplayOrderForTitle = 1;
                                resultObj.DisplayOrderForDescription = 3;
                                resultObj.DisplayOrderForNextButton = 4;

                                if (quizType == (int)QuizTypeEnum.Assessment || quizType == (int)QuizTypeEnum.AssessmentTemplate)
                                {
                                    resultObj.MinScore = 0;
                                    resultObj.MaxScore = 1;
                                }
                                else if (quizType == (int)QuizTypeEnum.Personality || quizType == (int)QuizTypeEnum.PersonalityTemplate)
                                {
                                    resultObj.DisplayOrder = 1;
                                }
                                else if (quizType == (int)QuizTypeEnum.NPS)
                                {
                                    resultObj.DisplayOrder = 1;
                                    resultObj.MinScore = 0;
                                    resultObj.MaxScore = 6;
                                }
                                else
                                {
                                    resultObj.MinScore = 0;
                                    resultObj.MaxScore = 0;
                                }

                                AUOWObj.QuizResultsRepository.Insert(resultObj);
                                AUOWObj.Save();

                                if (quizType == (int)QuizTypeEnum.Personality || quizType == (int)QuizTypeEnum.PersonalityTemplate)
                                {
                                    resultObj = new Db.QuizResults();

                                    resultObj.QuizId = quizDetails.Id; 
                                    resultObj.Title = "Result " + (AUOWObj.QuizResultsRepository.GetQuizResultsRepositoryExtension(quizDetails.Id).Where(r => r.State == (int)StatusEnum.Active).Count() + 1).ToString();
                                    //resultObj.Title = "Result " + (quizDetails.QuizResults.Where(r => r.State == (int)StatusEnum.Active).Count() + 1).ToString();
                                    resultObj.InternalTitle = "Result " + (AUOWObj.QuizResultsRepository.GetQuizResultsRepositoryExtension(quizDetails.Id).Where(r => r.State == (int)StatusEnum.Active).Count() + 1).ToString();
                                    //resultObj.InternalTitle = "Result " + (quizDetails.QuizResults.Where(r => r.State == (int)StatusEnum.Active).Count() + 1).ToString();
                                    resultObj.ShowResultImage = true;
                                    resultObj.Description = "Result Description";
                                    resultObj.HideCallToAction = false;
                                    resultObj.OpenLinkInNewTab = true;
                                    resultObj.ActionButtonTxtSize = "24 px";
                                    resultObj.ActionButtonText = "Call To Action";
                                    resultObj.Status = (int)StatusEnum.Active;
                                    resultObj.LastUpdatedBy = BusinessUserId;
                                    resultObj.LastUpdatedOn = currentDate;
                                    resultObj.State = (int)QuizStateEnum.DRAFTED;
                                    resultObj.DisplayOrder = 2;
                                    resultObj.MinScore = null;
                                    resultObj.MaxScore = null;
                                    resultObj.ShowLeadUserForm = false;
                                    resultObj.ShowExternalTitle = true;
                                    resultObj.ShowInternalTitle = true;
                                    resultObj.ShowDescription = true;
                                    resultObj.EnableMediaFile = false;
                                    resultObj.DisplayOrderForTitle = 1;
                                    resultObj.DisplayOrderForTitleImage = 2;
                                    resultObj.DisplayOrderForDescription = 3;
                                    resultObj.DisplayOrderForNextButton = 4;

                                    AUOWObj.QuizResultsRepository.Insert(resultObj);
                                    AUOWObj.Save();


                                    resultObj = new Db.QuizResults();

                                    resultObj.QuizId = quizDetails.Id;
                                    resultObj.Title = "Based On Correlations";
                                    resultObj.InternalTitle = "Based On Correlations";
                                    resultObj.ShowResultImage = false;
                                    resultObj.Description = "Calculated Result";
                                    resultObj.HideCallToAction = false;
                                    resultObj.OpenLinkInNewTab = false;
                                    resultObj.ActionButtonTxtSize = "24 px";
                                    resultObj.ActionButtonText = "Call To Action";
                                    resultObj.Status = (int)StatusEnum.Active;
                                    resultObj.LastUpdatedBy = BusinessUserId;
                                    resultObj.LastUpdatedOn = currentDate;
                                    resultObj.State = (int)QuizStateEnum.DRAFTED;
                                    resultObj.IsPersonalityCorrelatedResult = true;
                                    resultObj.DisplayOrder = 3;
                                    resultObj.MinScore = null;
                                    resultObj.MaxScore = null;
                                    resultObj.ShowLeadUserForm = false;
                                    resultObj.ShowExternalTitle = true;
                                    resultObj.ShowInternalTitle = true;
                                    resultObj.ShowDescription = true;
                                    resultObj.EnableMediaFile = false;
                                    resultObj.DisplayOrderForTitle = 1;
                                    resultObj.DisplayOrderForTitleImage = 2;
                                    resultObj.DisplayOrderForDescription = 3;
                                    resultObj.DisplayOrderForNextButton = 4;

                                    AUOWObj.QuizResultsRepository.Insert(resultObj);
                                    AUOWObj.Save();

                                    var multipleresult = new Db.PersonalityResultSetting();
                                    multipleresult.QuizId = quizDetails.Id;
                                    multipleresult.Title = "Your Top Results";
                                    multipleresult.Status = (int)StatusEnum.Inactive;
                                    multipleresult.GraphColor = "#417341";
                                    multipleresult.MaxResult = 2;
                                    //multipleresult.ButtonColor = "#bc3939";
                                    multipleresult.ButtonFontColor = "#ffffff";
                                    multipleresult.SideButtonText = "More Details";
                                    multipleresult.IsFullWidthEnable = false;
                                    multipleresult.LastUpdatedOn = DateTime.UtcNow;
                                    multipleresult.LastUpdatedBy = BusinessUserId;
                                    multipleresult.ShowLeadUserForm = false;

                                    AUOWObj.PersonalityResultSettingRepository.Insert(multipleresult);
                                    AUOWObj.Save();
                                }

                                if (quizType == (int)QuizTypeEnum.NPS)
                                {
                                    resultObj = new Db.QuizResults();

                                    resultObj.QuizId = quizDetails.Id;
                                    resultObj.Title = "Passive";
                                    resultObj.InternalTitle = "Passive";
                                    resultObj.ShowResultImage = true;
                                    resultObj.Description = "Result Description";
                                    resultObj.HideCallToAction = false;
                                    resultObj.OpenLinkInNewTab = true;
                                    resultObj.ActionButtonTxtSize = "24 px";
                                    resultObj.ActionButtonText = "Call To Action";
                                    resultObj.Status = (int)StatusEnum.Active;
                                    resultObj.LastUpdatedBy = BusinessUserId;
                                    resultObj.LastUpdatedOn = currentDate;
                                    resultObj.State = (int)QuizStateEnum.DRAFTED;
                                    resultObj.DisplayOrder = 2;
                                    resultObj.MinScore = 7;
                                    resultObj.MaxScore = 8;
                                    resultObj.ShowLeadUserForm = false;
                                    resultObj.DisplayOrderForTitle = 1;
                                    resultObj.DisplayOrderForTitleImage = 2;
                                    resultObj.DisplayOrderForDescription = 3;
                                    resultObj.DisplayOrderForNextButton = 4;
                                    resultObj.ShowExternalTitle = true;
                                    resultObj.ShowInternalTitle = true;
                                    resultObj.ShowDescription = true;
                                    resultObj.EnableMediaFile = false;

                                    AUOWObj.QuizResultsRepository.Insert(resultObj);
                                    AUOWObj.Save();


                                    resultObj = new Db.QuizResults();

                                    resultObj.QuizId = quizDetails.Id;
                                    resultObj.Title = "Promoter";
                                    resultObj.InternalTitle = "Promoter";
                                    resultObj.ShowResultImage = true;
                                    resultObj.Description = "Result Description";
                                    resultObj.HideCallToAction = false;
                                    resultObj.OpenLinkInNewTab = true;
                                    resultObj.ActionButtonTxtSize = "24 px";
                                    resultObj.ActionButtonText = "Call To Action";
                                    resultObj.Status = (int)StatusEnum.Active;
                                    resultObj.LastUpdatedBy = BusinessUserId;
                                    resultObj.LastUpdatedOn = currentDate;
                                    resultObj.State = (int)QuizStateEnum.DRAFTED;
                                    resultObj.DisplayOrder = 3;
                                    resultObj.MinScore = 9;
                                    resultObj.MaxScore = 10;
                                    resultObj.ShowLeadUserForm = false;
                                    resultObj.DisplayOrderForTitle = 1;
                                    resultObj.DisplayOrderForTitleImage = 2;
                                    resultObj.DisplayOrderForDescription = 3;
                                    resultObj.DisplayOrderForNextButton = 4;
                                    resultObj.ShowExternalTitle = true;
                                    resultObj.ShowInternalTitle = true;
                                    resultObj.ShowDescription = true;
                                    resultObj.EnableMediaFile = false;

                                    AUOWObj.QuizResultsRepository.Insert(resultObj);
                                    AUOWObj.Save();
                                }                                
                                //var resultSetting = quizDetails.ResultSettings.FirstOrDefault();
                                var resultSetting = AUOWObj.ResultSettingsRepository.GetResultSettingsRepositoryExtension(quizDetails.Id).FirstOrDefault();

                                if (resultSetting == null)
                                {
                                    resultSetting = new Db.ResultSettings();

                                    resultSetting.QuizId = quizDetails.Id;
                                    resultSetting.ShowScoreValue = true;
                                    resultSetting.ShowCorrectAnswer = false;
                                    resultSetting.LastUpdatedBy = BusinessUserId;
                                    resultSetting.LastUpdatedOn = currentDate;
                                    resultSetting.State = (int)QuizStateEnum.DRAFTED;

                                    if (quizObj.QuizType == (int)QuizTypeEnum.Score || quizObj.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                        resultSetting.CustomTxtForScoreValueInResult = "You scored a%score%";
                                    else if (quizObj.QuizType == (int)QuizTypeEnum.NPS)
                                    {
                                        resultSetting.CustomTxtForScoreValueInResult = string.Empty;
                                        resultSetting.ShowScoreValue = false;
                                    }
                                    else
                                        resultSetting.CustomTxtForScoreValueInResult = "I got%score%out of%total%Correct";

                                    resultSetting.CustomTxtForAnswerKey = "Answer Key";
                                    resultSetting.CustomTxtForYourAnswer = "Your Answer";
                                    resultSetting.CustomTxtForCorrectAnswer = "Correct Answer";
                                    resultSetting.CustomTxtForExplanation = "Explanation";
                                    AUOWObj.ResultSettingsRepository.Insert(resultSetting);
                                }

                                quizDetails.LastUpdatedBy = BusinessUserId;
                                quizDetails.LastUpdatedOn = currentDate;

                                quizObj.State = (int)QuizStateEnum.DRAFTED;

                                AUOWObj.QuizRepository.Update(quizObj);

                                AUOWObj.Save();
                                transaction.Complete();

                                #endregion

                                #region Bind return obj


                                quizResult = new QuizResult();

                                quizResult.ResultId = resultObj.Id;
                                quizResult.Title = resultObj.Title;
                                quizResult.InternalTitle = resultObj.InternalTitle;
                                quizResult.ShowResultImage = resultObj.ShowResultImage;
                                quizResult.Description = resultObj.Description;
                                quizResult.HideCallToAction = resultObj.HideCallToAction;
                                quizResult.EnableCallToActionButton = !(resultObj.HideCallToAction ?? false); ;
                                quizResult.ActionButtonURL = resultObj.ActionButtonURL;
                                quizResult.OpenLinkInNewTab = resultObj.OpenLinkInNewTab;
                                quizResult.ActionButtonTxtSize = resultObj.ActionButtonTxtSize;
                                quizResult.ActionButtonColor = resultObj.ActionButtonColor;
                                quizResult.ActionButtonTitleColor = resultObj.ActionButtonTitleColor;
                                quizResult.ActionButtonText = resultObj.ActionButtonText;
                                quizResult.ShowExternalTitle = resultObj.ShowExternalTitle;
                                quizResult.ShowInternalTitle = resultObj.ShowInternalTitle;
                                quizResult.ShowDescription = resultObj.ShowDescription;
                                quizResult.EnableMediaFile = resultObj.EnableMediaFile;
                                quizResult.DisplayOrderForTitleImage = resultObj.DisplayOrderForTitleImage;
                                quizResult.DisplayOrderForTitle = resultObj.DisplayOrderForTitle;
                                quizResult.DisplayOrderForDescription = resultObj.DisplayOrderForDescription;
                                quizResult.DisplayOrderForNextButton = resultObj.DisplayOrderForNextButton;

                                quizResult.ResultSetting = new QuizResultSetting();

                                quizResult.ResultSetting.QuizId = QuizId;
                                quizResult.ResultSetting.ShowScoreValue = resultSetting.ShowScoreValue;
                                quizResult.ResultSetting.ShowCorrectAnswer = resultSetting.ShowCorrectAnswer;
                                quizResult.ResultSetting.CustomTxtForScoreValueInResult = resultSetting.CustomTxtForScoreValueInResult;
                                quizResult.ResultSetting.CustomTxtForAnswerKey = resultSetting.CustomTxtForAnswerKey;
                                quizResult.ResultSetting.CustomTxtForYourAnswer = resultSetting.CustomTxtForYourAnswer;
                                quizResult.ResultSetting.CustomTxtForCorrectAnswer = resultSetting.CustomTxtForCorrectAnswer;
                                quizResult.ResultSetting.CustomTxtForExplanation = resultSetting.CustomTxtForExplanation;

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
            return quizResult;
        }


    }
}
