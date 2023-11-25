using Core.Common.Extensions;
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
using QuizApp.Response;

namespace QuizApp.Services.Service
{
    public class BranchingLogicService : IBranchingLogicService
    {
        private ResultEnum status = ResultEnum.Ok;
        private string errormessage = string.Empty;
        public ResultEnum Status { get { return status; } set { status = value; } }
        public string ErrorMessage { get { return errormessage; } set { errormessage = value; } }



        public QuizBranchingLogic GetQuizBranchingLogic(int QuizId)
        {
            QuizBranchingLogic quizBranchingLogic = null;

            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizObj = UOWObj.QuizRepository.GetByID(QuizId);

                    if (quizObj == null)
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz not found for the QuizId " + QuizId;
                    }

                    else
                    {
                        var quizDetailsObj = quizObj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED);

                        if (quizDetailsObj != null)
                        {
                            if (!(quizDetailsObj.IsBranchingLogicEnabled.HasValue && quizDetailsObj.IsBranchingLogicEnabled.Value))
                            {
                                Status = ResultEnum.Error;
                                ErrorMessage = "Quiz branching logic not enabled for the QuizId " + QuizId;
                            }

                            else
                            {
                                quizBranchingLogic = new QuizBranchingLogic();

                                quizBranchingLogic.QuizId = QuizId;
                                quizBranchingLogic.QuizType = quizObj.QuizType;
                                quizBranchingLogic.IsWhatsAppChatBotOldVersion = quizObj.IsWhatsAppChatBotOldVersion.HasValue ? quizObj.IsWhatsAppChatBotOldVersion.Value : false;

                                quizBranchingLogic.QuizTitle = quizDetailsObj.QuizTitle;
                                quizBranchingLogic.QuizCoverTitle = quizDetailsObj.QuizCoverTitle;
                                quizBranchingLogic.QuizDescription = quizDetailsObj.QuizDescription;
                                quizBranchingLogic.ShowDescription = quizDetailsObj.ShowDescription;
                                quizBranchingLogic.SecondsToApply = quizDetailsObj.SecondsToApply;
                                quizBranchingLogic.VideoFrameEnabled = quizDetailsObj.VideoFrameEnabled;
                                quizBranchingLogic.StartType = quizDetailsObj.BranchingLogic.FirstOrDefault(a => a.IsStartingPoint) != null ? (BranchingLogicEnum)quizDetailsObj.BranchingLogic.FirstOrDefault(a => a.IsStartingPoint).SourceTypeId : BranchingLogicEnum.NONE;
                                quizBranchingLogic.StartTypeId = quizDetailsObj.BranchingLogic.FirstOrDefault(a => a.IsStartingPoint) != null ? quizDetailsObj.BranchingLogic.FirstOrDefault(a => a.IsStartingPoint).SourceObjectId : 0;

                                var quesAndContentBranchingLogic = new List<QuestionAndContentBranchingLogic>();

                                foreach (var question in quizDetailsObj.QuestionsInQuiz.Where(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active).OrderBy(r => r.DisplayOrder))
                                {
                                    switch (question.Type)
                                    {
                                        case (int)BranchingLogicEnum.QUESTION:
                                        case (int)BranchingLogicEnum.WHATSAPPTEMPLATE:
                                            {
                                                QuestionAndContentBranchingLogic branchingLogic = QuesTypeBranchingLogic(QuizId, quizObj, quizDetailsObj, question);

                                                quesAndContentBranchingLogic.Add(branchingLogic);
                                                break;
                                            }

                                        case (int)BranchingLogicEnum.CONTENT:
                                            {
                                                QuestionAndContentBranchingLogic branchingLogic = ContentTypeBranchingLogic(QuizId, quizDetailsObj, question);

                                                quesAndContentBranchingLogic.Add(branchingLogic);
                                                break;
                                            }

                                    }
                                }
                                quizBranchingLogic.QuestionAndContentList = quesAndContentBranchingLogic;

                                foreach (var content in quizDetailsObj.ContentsInQuiz.Where(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active).OrderBy(r => r.DisplayOrder))
                                {
                                    QuestionAndContentBranchingLogic branchingLogic = ContentInQuizMapping(QuizId, quizDetailsObj, content);
                                    quesAndContentBranchingLogic.Add(branchingLogic);
                                }
                                quizBranchingLogic.QuestionAndContentList = quesAndContentBranchingLogic;

                                var resultBranchingLogicLst = new List<ResultBranchingLogic>();

                                foreach (var result in quizDetailsObj.QuizResults.Where(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active))
                                {
                                    var branchingLogic = new ResultBranchingLogic();
                                    var linkedBranchObj = quizDetailsObj.BranchingLogic.FirstOrDefault(a => a.SourceObjectId == result.Id && a.SourceTypeId == (int)BranchingLogicEnum.RESULTNEXT);
                                    branchingLogic.QuizId = QuizId;
                                    branchingLogic.ResultId = result.Id;
                                    branchingLogic.ResultTitle = result.Title;
                                    branchingLogic.ResultInternalTitle = result.InternalTitle;
                                    branchingLogic.ResultDescription = result.Description;
                                    branchingLogic.SecondsToApply = result.SecondsToApply ?? "0";
                                    branchingLogic.VideoFrameEnabled = result.VideoFrameEnabled ?? false;
                                    branchingLogic.ResultImage = result.Image ?? string.Empty;
                                    branchingLogic.PublicIdForResult = result.PublicId ?? string.Empty;
                                    branchingLogic.ShowResultImage = result.ShowResultImage;
                                    branchingLogic.ActionButtonText = result.ActionButtonText;
                                    branchingLogic.ShowDescription = result.ShowDescription;
                                    branchingLogic.ShowExternalTitle = result.ShowExternalTitle;
                                    branchingLogic.ShowInternalTitle = result.ShowInternalTitle;
                                    branchingLogic.IsDisabled = true;
                                    if (linkedBranchObj != null)
                                    {
                                        branchingLogic.LinkedActionId = linkedBranchObj.DestinationTypeId.HasValue && linkedBranchObj.DestinationTypeId == (int)BranchingLogicEnum.ACTION ? linkedBranchObj.DestinationObjectId : null;
                                        branchingLogic.LinkedContentId = linkedBranchObj.DestinationTypeId.HasValue && linkedBranchObj.DestinationTypeId == (int)BranchingLogicEnum.CONTENT ? linkedBranchObj.DestinationObjectId : null;
                                        branchingLogic.LinkedToType = linkedBranchObj.DestinationTypeId.HasValue ? linkedBranchObj.DestinationTypeId.Value : (int)BranchingLogicEnum.NONE;
                                        branchingLogic.IsDisabled = false;
                                    }
                                    branchingLogic.HideActionButton = result.HideCallToAction.HasValue ? result.HideCallToAction.Value : false;
                                    branchingLogic.MinScore = result.MinScore;
                                    branchingLogic.MaxScore = result.MaxScore;
                                    branchingLogic.IsPersonalityCorrelatedResult = result.IsPersonalityCorrelatedResult;

                                    resultBranchingLogicLst.Add(branchingLogic);
                                }
                                quizBranchingLogic.ResultList = resultBranchingLogicLst;

                                var actionBranchingLogicLst = new List<ActionBranchingLogic>();

                                foreach (var action in quizDetailsObj.ActionsInQuiz.Where(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active))
                                {
                                    var branchingLogic = new ActionBranchingLogic();
                                    branchingLogic.QuizId = QuizId;
                                    branchingLogic.ActionId = action.Id;
                                    branchingLogic.ActionTitle = action.Title;
                                    branchingLogic.ActionType = action.ActionType;
                                    branchingLogic.AppointmentId = action.AppointmentId;
                                    branchingLogic.ReportEmails = action.ReportEmails;
                                    branchingLogic.IsDisabled = true;

                                    actionBranchingLogicLst.Add(branchingLogic);
                                }
                                quizBranchingLogic.ActionList = actionBranchingLogicLst;
                                quizBranchingLogic.IsQuesAndContentInSameTable = quizObj.QuesAndContentInSameTable;

                                quizBranchingLogic.UsageTypes = new List<int>();
                                var usageTypeInQuiz = quizObj.UsageTypeInQuiz.Where(r => r.QuizId == quizObj.Id).Select(v => v.UsageType);
                                if (usageTypeInQuiz != null && usageTypeInQuiz.Any())
                                {
                                    foreach (var item in usageTypeInQuiz.ToList())
                                    {
                                        quizBranchingLogic.UsageTypes.Add(item);
                                    }
                                }

                                var badgeBranchingLogicLst = new List<BadgeBranchingLogic>();

                                foreach (var badge in quizDetailsObj.BadgesInQuiz.Where(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active))
                                {
                                    BadgeBranchingLogic branchingLogic = BadgesInQuizMapping(QuizId, quizDetailsObj, badge);

                                    badgeBranchingLogicLst.Add(branchingLogic);
                                }
                                quizBranchingLogic.BadgeList = badgeBranchingLogicLst;

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

            return quizBranchingLogic;

        }

        private QuestionAndContentBranchingLogic ContentTypeBranchingLogic(int QuizId, Db.QuizDetails quizDetailsObj, QuestionsInQuiz question)
        {

            var branchingLogic = new QuestionAndContentBranchingLogic();
            var linkedBranchObj = quizDetailsObj.BranchingLogic.FirstOrDefault(a => a.SourceObjectId == question.Id && a.SourceTypeId == (int)BranchingLogicEnum.CONTENTNEXT);

            branchingLogic.QuizId = QuizId;
            branchingLogic.ContentId = question.Id;
            branchingLogic.ContentTitle = question.Question;
            branchingLogic.ShowContentTitleImage = question.ShowQuestionImage;
            branchingLogic.ContentTitleImage = question.QuestionImage ?? string.Empty;
            branchingLogic.PublicIdForContentTitle = question.PublicId ?? string.Empty;
            branchingLogic.ContentDescription = question.Description;
            branchingLogic.ContentDescriptionImage = question.DescriptionImage ?? string.Empty;
            branchingLogic.PublicIdForContentDescription = question.PublicIdForDescription ?? string.Empty;
            branchingLogic.ShowContentDescriptionImage = question.ShowDescriptionImage;
            branchingLogic.ActionButtonText = question.AliasTextForNextButton;
            branchingLogic.IsDisabled = true;
            branchingLogic.SecondsToApply = question.SecondsToApply ?? "0";
            branchingLogic.VideoFrameEnabled = question.VideoFrameEnabled ?? false;
            branchingLogic.SecondsToApplyForDescription = question.SecondsToApplyForDescription ?? "0";
            branchingLogic.DescVideoFrameEnabled = question.DescVideoFrameEnabled ?? false;
            branchingLogic.Type = question.Type;
            branchingLogic.DisplayOrder = question.DisplayOrder;
            //branchingLogic.LanguageId = question.LanguageId;
            branchingLogic.LanguageCode = question.LanguageCode;
            branchingLogic.TemplateId = question.TemplateId;
            branchingLogic.ShowTitle = question.ShowTitle;
            branchingLogic.ShowDescription = question.ShowDescription;
            if (linkedBranchObj != null)
            {
                branchingLogic.LinkedQuestionId = linkedBranchObj.DestinationTypeId.HasValue && linkedBranchObj.DestinationTypeId == (int)BranchingLogicEnum.QUESTION ? linkedBranchObj.DestinationObjectId : null;
                branchingLogic.LinkedResultId = linkedBranchObj.DestinationTypeId.HasValue && linkedBranchObj.DestinationTypeId == (int)BranchingLogicEnum.RESULT ? linkedBranchObj.DestinationObjectId : null;
                branchingLogic.LinkedContentId = linkedBranchObj.DestinationTypeId.HasValue && linkedBranchObj.DestinationTypeId == (int)BranchingLogicEnum.CONTENT ? linkedBranchObj.DestinationObjectId : null;
                branchingLogic.LinkedToType = linkedBranchObj.DestinationTypeId.HasValue ? linkedBranchObj.DestinationTypeId.Value : (int)BranchingLogicEnum.NONE;

                branchingLogic.IsDisabled = false;
            }

            return branchingLogic;
        }

        private QuestionAndContentBranchingLogic QuesTypeBranchingLogic(int QuizId, Quiz quizObj, Db.QuizDetails quizDetailsObj, QuestionsInQuiz question)
        {
            var branchingLogic = new QuestionAndContentBranchingLogic();

            branchingLogic.QuizId = QuizId;
            branchingLogic.QuizType = quizObj.QuizType;
            branchingLogic.QuestionId = question.Id;
            branchingLogic.QuestionTxt = question.Question;
            branchingLogic.ShowQuestionImage = question.ShowQuestionImage;
            branchingLogic.ShowAnswerImage = question.ShowAnswerImage;
            branchingLogic.QuestionImage = question.QuestionImage ?? string.Empty;
            branchingLogic.PublicIdForQuestion = question.PublicId ?? string.Empty;
            branchingLogic.IsDisabled = true;
            branchingLogic.SecondsToApply = question.SecondsToApply ?? "0";
            branchingLogic.VideoFrameEnabled = question.VideoFrameEnabled ?? false;
            branchingLogic.SecondsToApplyForDescription = question.SecondsToApplyForDescription ?? "0";
            branchingLogic.DescVideoFrameEnabled = question.DescVideoFrameEnabled ?? false;
            branchingLogic.MinAnswer = question.MinAnswer;
            branchingLogic.MaxAnswer = question.MaxAnswer;
            branchingLogic.AnswerType = question.AnswerType;
            branchingLogic.DisplayOrder = question.DisplayOrder;
            //branchingLogic.LanguageId = question.LanguageId;
            branchingLogic.LanguageCode = question.LanguageCode;
            branchingLogic.TemplateId = question.TemplateId;
            branchingLogic.ShowTitle = question.ShowTitle;
            branchingLogic.ShowDescription = question.ShowDescription;
            branchingLogic.IsMultiRating = question.IsMultiRating;
            branchingLogic.ContentDescription = question.Description ?? string.Empty;
            if (question.Type == (int)BranchingLogicEnum.QUESTION)
            {
                branchingLogic.Type = (int)BranchingLogicEnum.QUESTION;
            }
            else
            {
                branchingLogic.Type = (int)BranchingLogicEnum.WHATSAPPTEMPLATE;
            }

            branchingLogic.AnswerList = new List<QuestionAndContentBranchingLogic.AnswerInQuestions>();

            foreach (var answer in question.AnswerOptionsInQuizQuestions.Where(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active && (r.QuestionsInQuiz.TimerRequired ? true : !r.IsUnansweredType)).OrderBy(r => r.DisplayOrder))
            {
                var answerObj = new QuestionAndContentBranchingLogic.AnswerInQuestions();

                answerObj.AnswerId = answer.Id;
                answerObj.AnswerTxt = answer.Option;
                answerObj.AnswerDescription = answer.Description;
                answerObj.AssociatedScore = answer.AssociatedScore;
                answerObj.AnswerOptionImage = answer.OptionImage ?? string.Empty;
                answerObj.PublicIdForAnswerOption = answer.PublicId ?? string.Empty;
                if(answer.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Availability)
                {
                    answerObj.IsCorrect = false;
                }
                else
                {
                    answerObj.IsCorrect = answer.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.LookingforJobs ? default(bool?) : answer.IsCorrectAnswer.HasValue && answer.IsCorrectAnswer.Value;
                }
                answerObj.DisplayOrder = answer.DisplayOrder;
                answerObj.SecondsToApply = answer.SecondsToApply ?? "0";
                answerObj.VideoFrameEnabled = answer.VideoFrameEnabled ?? false;
                answerObj.RefId = answer.RefId;
                var linkedBranchObj = quizDetailsObj.BranchingLogic.FirstOrDefault(a => a.SourceObjectId == answer.Id && a.SourceTypeId == (int)BranchingLogicEnum.ANSWER);

                if (linkedBranchObj != null)
                {
                    answerObj.QuestionId = linkedBranchObj.DestinationTypeId.HasValue && linkedBranchObj.DestinationTypeId == (int)BranchingLogicEnum.QUESTION ? linkedBranchObj.DestinationObjectId : null;
                    answerObj.ResultId = linkedBranchObj.DestinationTypeId.HasValue && linkedBranchObj.DestinationTypeId == (int)BranchingLogicEnum.RESULT ? linkedBranchObj.DestinationObjectId : null;
                    answerObj.ContentId = linkedBranchObj.DestinationTypeId.HasValue && linkedBranchObj.DestinationTypeId == (int)BranchingLogicEnum.CONTENT ? linkedBranchObj.DestinationObjectId : null;
                    answerObj.LinkedToType = linkedBranchObj.DestinationTypeId.HasValue ? linkedBranchObj.DestinationTypeId.Value : (int)BranchingLogicEnum.NONE;

                    branchingLogic.IsDisabled = false;
                }

                branchingLogic.AnswerList.Add(answerObj);
            }

            return branchingLogic;
        }
        private QuestionAndContentBranchingLogic ContentInQuizMapping(int QuizId, Db.QuizDetails quizDetailsObj, ContentsInQuiz content)
        {
            var branchingLogic = new QuestionAndContentBranchingLogic();
            var linkedBranchObj = quizDetailsObj.BranchingLogic.FirstOrDefault(a => a.SourceObjectId == content.Id && a.SourceTypeId == (int)BranchingLogicEnum.CONTENTNEXT);

            branchingLogic.QuizId = QuizId;
            branchingLogic.ContentId = content.Id;
            branchingLogic.ContentTitle = content.ContentTitle;
            branchingLogic.ShowContentTitleImage = content.ShowContentTitleImage;
            branchingLogic.ContentTitleImage = content.ContentTitleImage ?? string.Empty;
            branchingLogic.PublicIdForContentTitle = content.PublicIdForContentTitle ?? string.Empty;
            branchingLogic.ContentDescription = content.ContentDescription;
            branchingLogic.ContentDescriptionImage = content.ContentDescriptionImage ?? string.Empty;
            branchingLogic.PublicIdForContentDescription = content.PublicIdForContentDescription ?? string.Empty;
            branchingLogic.ShowContentDescriptionImage = content.ShowContentDescriptionImage;
            branchingLogic.SecondsToApply = content.SecondsToApply ?? "0";
            branchingLogic.VideoFrameEnabled = content.VideoFrameEnabled ?? false;
            branchingLogic.SecondsToApplyForDescription = content.SecondsToApplyForDescription ?? "0";
            branchingLogic.DescVideoFrameEnabled = content.DescVideoFrameEnabled ?? false;
            branchingLogic.ActionButtonText = content.AliasTextForNextButton;
            branchingLogic.IsDisabled = true;
            branchingLogic.Type = (int)BranchingLogicEnum.CONTENT;
            branchingLogic.DisplayOrder = content.DisplayOrder;
            if (linkedBranchObj != null)
            {
                branchingLogic.LinkedQuestionId = linkedBranchObj.DestinationTypeId.HasValue && linkedBranchObj.DestinationTypeId == (int)BranchingLogicEnum.QUESTION ? linkedBranchObj.DestinationObjectId : null;
                branchingLogic.LinkedResultId = linkedBranchObj.DestinationTypeId.HasValue && linkedBranchObj.DestinationTypeId == (int)BranchingLogicEnum.RESULT ? linkedBranchObj.DestinationObjectId : null;
                branchingLogic.LinkedContentId = linkedBranchObj.DestinationTypeId.HasValue && linkedBranchObj.DestinationTypeId == (int)BranchingLogicEnum.CONTENT ? linkedBranchObj.DestinationObjectId : null;
                branchingLogic.LinkedToType = linkedBranchObj.DestinationTypeId.HasValue ? linkedBranchObj.DestinationTypeId.Value : (int)BranchingLogicEnum.NONE;

                branchingLogic.IsDisabled = false;
            }

            return branchingLogic;
        }
        private BadgeBranchingLogic BadgesInQuizMapping(int QuizId, Db.QuizDetails quizDetailsObj, BadgesInQuiz badge)
        {
            var branchingLogic = new BadgeBranchingLogic();
            var linkedBranchObj = quizDetailsObj.BranchingLogic.FirstOrDefault(a => a.SourceObjectId == badge.Id && a.SourceTypeId == (int)BranchingLogicEnum.BADGENEXT);
            branchingLogic.QuizId = QuizId;
            branchingLogic.BadgetId = badge.Id;
            branchingLogic.BadgetTitle = badge.Title;
            branchingLogic.SecondsToApply = badge.SecondsToApply ?? "0";
            branchingLogic.VideoFrameEnabled = badge.VideoFrameEnabled ?? false;
            branchingLogic.BadgetImage = badge.Image ?? string.Empty;
            branchingLogic.PublicIdForBadget = badge.PublicId ?? string.Empty;
            if (linkedBranchObj != null)
            {
                branchingLogic.LinkedQuestionId = linkedBranchObj.DestinationTypeId.HasValue && linkedBranchObj.DestinationTypeId == (int)BranchingLogicEnum.QUESTION ? linkedBranchObj.DestinationObjectId : null;
                branchingLogic.LinkedContentId = linkedBranchObj.DestinationTypeId.HasValue && linkedBranchObj.DestinationTypeId == (int)BranchingLogicEnum.CONTENT ? linkedBranchObj.DestinationObjectId : null;
                branchingLogic.LinkedToType = linkedBranchObj.DestinationTypeId.HasValue ? linkedBranchObj.DestinationTypeId.Value : (int)BranchingLogicEnum.NONE;
                branchingLogic.LinkedResultId = linkedBranchObj.DestinationTypeId.HasValue && linkedBranchObj.DestinationTypeId == (int)BranchingLogicEnum.RESULT ? linkedBranchObj.DestinationObjectId : null;
                branchingLogic.IsDisabled = false;
            }

            return branchingLogic;
        }
        public QuizBranchingLogic GetQuizBranchingLogicOld(int QuizId)
        {
            QuizBranchingLogic quizBranchingLogic = null;

            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizObj = UOWObj.QuizRepository.GetByID(QuizId);

                    if (quizObj != null)
                    {
                        var quizDetailsObj = quizObj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED);

                        if (quizDetailsObj != null)
                        {
                            if (quizDetailsObj.IsBranchingLogicEnabled.HasValue && quizDetailsObj.IsBranchingLogicEnabled.Value)
                            {
                                quizBranchingLogic = new QuizBranchingLogic();

                                quizBranchingLogic.QuizId = QuizId;
                                quizBranchingLogic.QuizType = quizObj.QuizType;
                                quizBranchingLogic.QuizTitle = quizDetailsObj.QuizTitle;
                                quizBranchingLogic.QuizCoverTitle = quizDetailsObj.QuizCoverTitle;
                                quizBranchingLogic.QuizDescription = quizDetailsObj.QuizDescription;
                                quizBranchingLogic.SecondsToApply = quizDetailsObj.SecondsToApply;
                                quizBranchingLogic.VideoFrameEnabled = quizDetailsObj.VideoFrameEnabled;
                                quizBranchingLogic.StartType = quizDetailsObj.BranchingLogic.FirstOrDefault(a => a.IsStartingPoint) != null ? (BranchingLogicEnum)quizDetailsObj.BranchingLogic.FirstOrDefault(a => a.IsStartingPoint).SourceTypeId : BranchingLogicEnum.NONE;
                                quizBranchingLogic.StartTypeId = quizDetailsObj.BranchingLogic.FirstOrDefault(a => a.IsStartingPoint) != null ? quizDetailsObj.BranchingLogic.FirstOrDefault(a => a.IsStartingPoint).SourceObjectId : 0;

                                var quesAndContentBranchingLogic = new List<QuestionAndContentBranchingLogic>();

                                foreach (var question in quizDetailsObj.QuestionsInQuiz.Where(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active).OrderBy(r => r.DisplayOrder))
                                {
                                    if (question.Type == (int)BranchingLogicEnum.QUESTION)
                                    {
                                        var branchingLogic = new QuestionAndContentBranchingLogic();

                                        branchingLogic.QuizId = QuizId;
                                        branchingLogic.QuizType = quizObj.QuizType;
                                        branchingLogic.QuestionId = question.Id;
                                        branchingLogic.QuestionTxt = question.Question;
                                        branchingLogic.ShowQuestionImage = question.ShowQuestionImage;
                                        branchingLogic.ShowAnswerImage = question.ShowAnswerImage;
                                        branchingLogic.QuestionImage = question.QuestionImage ?? string.Empty;
                                        branchingLogic.PublicIdForQuestion = question.PublicId ?? string.Empty;
                                        branchingLogic.IsDisabled = true;
                                        branchingLogic.SecondsToApply = question.SecondsToApply ?? "0";
                                        branchingLogic.VideoFrameEnabled = question.VideoFrameEnabled ?? false;
                                        branchingLogic.SecondsToApplyForDescription = question.SecondsToApplyForDescription ?? "0";
                                        branchingLogic.DescVideoFrameEnabled = question.DescVideoFrameEnabled ?? false;
                                        branchingLogic.MinAnswer = question.MinAnswer;
                                        branchingLogic.MaxAnswer = question.MaxAnswer;
                                        branchingLogic.Type = (int)BranchingLogicEnum.QUESTION;
                                        branchingLogic.AnswerType = question.AnswerType;
                                        branchingLogic.DisplayOrder = question.DisplayOrder;
                                        branchingLogic.AnswerList = new List<QuestionAndContentBranchingLogic.AnswerInQuestions>();

                                        foreach (var answer in question.AnswerOptionsInQuizQuestions.Where(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active && (r.QuestionsInQuiz.TimerRequired ? true : !r.IsUnansweredType)).OrderBy(r => r.DisplayOrder))
                                        {
                                            var answerObj = new QuestionAndContentBranchingLogic.AnswerInQuestions();

                                            answerObj.AnswerId = answer.Id;
                                            answerObj.AnswerTxt = answer.Option;
                                            answerObj.AssociatedScore = answer.AssociatedScore;
                                            answerObj.AnswerOptionImage = answer.OptionImage ?? string.Empty;
                                            answerObj.PublicIdForAnswerOption = answer.PublicId ?? string.Empty;
                                            answerObj.IsCorrect = answer.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.LookingforJobs ? default(bool?) : answer.IsCorrectAnswer.HasValue && answer.IsCorrectAnswer.Value;
                                            answerObj.DisplayOrder = answer.DisplayOrder;
                                            answerObj.SecondsToApply = answer.SecondsToApply ?? "0";
                                            answerObj.VideoFrameEnabled = answer.VideoFrameEnabled ?? false;
                                            var linkedBranchObj = quizDetailsObj.BranchingLogic.FirstOrDefault(a => a.SourceObjectId == answer.Id && a.SourceTypeId == (int)BranchingLogicEnum.ANSWER);

                                            if (linkedBranchObj != null)
                                            {
                                                answerObj.QuestionId = linkedBranchObj.DestinationTypeId.HasValue && linkedBranchObj.DestinationTypeId == (int)BranchingLogicEnum.QUESTION ? linkedBranchObj.DestinationObjectId : null;
                                                answerObj.ResultId = linkedBranchObj.DestinationTypeId.HasValue && linkedBranchObj.DestinationTypeId == (int)BranchingLogicEnum.RESULT ? linkedBranchObj.DestinationObjectId : null;
                                                answerObj.ContentId = linkedBranchObj.DestinationTypeId.HasValue && linkedBranchObj.DestinationTypeId == (int)BranchingLogicEnum.CONTENT ? linkedBranchObj.DestinationObjectId : null;
                                                answerObj.LinkedToType = linkedBranchObj.DestinationTypeId.HasValue ? linkedBranchObj.DestinationTypeId.Value : (int)BranchingLogicEnum.NONE;

                                                branchingLogic.IsDisabled = false;
                                            }

                                            branchingLogic.AnswerList.Add(answerObj);
                                        }
                                        quesAndContentBranchingLogic.Add(branchingLogic);
                                    }
                                    else if (question.Type == (int)BranchingLogicEnum.CONTENT)
                                    {
                                        var branchingLogic = new QuestionAndContentBranchingLogic();
                                        var linkedBranchObj = quizDetailsObj.BranchingLogic.FirstOrDefault(a => a.SourceObjectId == question.Id && a.SourceTypeId == (int)BranchingLogicEnum.CONTENTNEXT);

                                        branchingLogic.QuizId = QuizId;
                                        branchingLogic.ContentId = question.Id;
                                        branchingLogic.ContentTitle = question.Question;
                                        branchingLogic.ShowContentTitleImage = question.ShowQuestionImage;
                                        branchingLogic.ContentTitleImage = question.QuestionImage ?? string.Empty;
                                        branchingLogic.PublicIdForContentTitle = question.PublicId ?? string.Empty;
                                        branchingLogic.ContentDescription = question.Description;
                                        branchingLogic.ContentDescriptionImage = question.DescriptionImage ?? string.Empty;
                                        branchingLogic.PublicIdForContentDescription = question.PublicIdForDescription ?? string.Empty;
                                        branchingLogic.ShowContentDescriptionImage = question.ShowDescriptionImage;
                                        branchingLogic.ActionButtonText = question.AliasTextForNextButton;
                                        branchingLogic.IsDisabled = true;
                                        branchingLogic.SecondsToApply = question.SecondsToApply ?? "0";
                                        branchingLogic.VideoFrameEnabled = question.VideoFrameEnabled ?? false;
                                        branchingLogic.SecondsToApplyForDescription = question.SecondsToApplyForDescription ?? "0";
                                        branchingLogic.DescVideoFrameEnabled = question.DescVideoFrameEnabled ?? false;
                                        branchingLogic.Type = question.Type;
                                        branchingLogic.DisplayOrder = question.DisplayOrder;
                                        if (linkedBranchObj != null)
                                        {
                                            branchingLogic.LinkedQuestionId = linkedBranchObj.DestinationTypeId.HasValue && linkedBranchObj.DestinationTypeId == (int)BranchingLogicEnum.QUESTION ? linkedBranchObj.DestinationObjectId : null;
                                            branchingLogic.LinkedResultId = linkedBranchObj.DestinationTypeId.HasValue && linkedBranchObj.DestinationTypeId == (int)BranchingLogicEnum.RESULT ? linkedBranchObj.DestinationObjectId : null;
                                            branchingLogic.LinkedContentId = linkedBranchObj.DestinationTypeId.HasValue && linkedBranchObj.DestinationTypeId == (int)BranchingLogicEnum.CONTENT ? linkedBranchObj.DestinationObjectId : null;
                                            branchingLogic.LinkedToType = linkedBranchObj.DestinationTypeId.HasValue ? linkedBranchObj.DestinationTypeId.Value : (int)BranchingLogicEnum.NONE;

                                            branchingLogic.IsDisabled = false;
                                        }
                                        quesAndContentBranchingLogic.Add(branchingLogic);
                                    }
                                }
                                quizBranchingLogic.QuestionAndContentList = quesAndContentBranchingLogic;

                                foreach (var content in quizDetailsObj.ContentsInQuiz.Where(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active).OrderBy(r => r.DisplayOrder))
                                {
                                    var branchingLogic = new QuestionAndContentBranchingLogic();
                                    var linkedBranchObj = quizDetailsObj.BranchingLogic.FirstOrDefault(a => a.SourceObjectId == content.Id && a.SourceTypeId == (int)BranchingLogicEnum.CONTENTNEXT);

                                    branchingLogic.QuizId = QuizId;
                                    branchingLogic.ContentId = content.Id;
                                    branchingLogic.ContentTitle = content.ContentTitle;
                                    branchingLogic.ShowContentTitleImage = content.ShowContentTitleImage;
                                    branchingLogic.ContentTitleImage = content.ContentTitleImage ?? string.Empty;
                                    branchingLogic.PublicIdForContentTitle = content.PublicIdForContentTitle ?? string.Empty;
                                    branchingLogic.ContentDescription = content.ContentDescription;
                                    branchingLogic.ContentDescriptionImage = content.ContentDescriptionImage ?? string.Empty;
                                    branchingLogic.PublicIdForContentDescription = content.PublicIdForContentDescription ?? string.Empty;
                                    branchingLogic.ShowContentDescriptionImage = content.ShowContentDescriptionImage;
                                    branchingLogic.SecondsToApply = content.SecondsToApply ?? "0";
                                    branchingLogic.VideoFrameEnabled = content.VideoFrameEnabled ?? false;
                                    branchingLogic.SecondsToApplyForDescription = content.SecondsToApplyForDescription ?? "0";
                                    branchingLogic.DescVideoFrameEnabled = content.DescVideoFrameEnabled ?? false;
                                    branchingLogic.ActionButtonText = content.AliasTextForNextButton;
                                    branchingLogic.IsDisabled = true;
                                    branchingLogic.Type = (int)BranchingLogicEnum.CONTENT;
                                    branchingLogic.DisplayOrder = content.DisplayOrder;
                                    if (linkedBranchObj != null)
                                    {
                                        branchingLogic.LinkedQuestionId = linkedBranchObj.DestinationTypeId.HasValue && linkedBranchObj.DestinationTypeId == (int)BranchingLogicEnum.QUESTION ? linkedBranchObj.DestinationObjectId : null;
                                        branchingLogic.LinkedResultId = linkedBranchObj.DestinationTypeId.HasValue && linkedBranchObj.DestinationTypeId == (int)BranchingLogicEnum.RESULT ? linkedBranchObj.DestinationObjectId : null;
                                        branchingLogic.LinkedContentId = linkedBranchObj.DestinationTypeId.HasValue && linkedBranchObj.DestinationTypeId == (int)BranchingLogicEnum.CONTENT ? linkedBranchObj.DestinationObjectId : null;
                                        branchingLogic.LinkedToType = linkedBranchObj.DestinationTypeId.HasValue ? linkedBranchObj.DestinationTypeId.Value : (int)BranchingLogicEnum.NONE;

                                        branchingLogic.IsDisabled = false;
                                    }
                                    quesAndContentBranchingLogic.Add(branchingLogic);
                                }
                                quizBranchingLogic.QuestionAndContentList = quesAndContentBranchingLogic;

                                var resultBranchingLogicLst = new List<ResultBranchingLogic>();

                                foreach (var result in quizDetailsObj.QuizResults.Where(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active))
                                {
                                    var branchingLogic = new ResultBranchingLogic();
                                    var linkedBranchObj = quizDetailsObj.BranchingLogic.FirstOrDefault(a => a.SourceObjectId == result.Id && a.SourceTypeId == (int)BranchingLogicEnum.RESULTNEXT);
                                    branchingLogic.QuizId = QuizId;
                                    branchingLogic.ResultId = result.Id;
                                    branchingLogic.ResultTitle = result.Title;
                                    branchingLogic.ResultInternalTitle = result.InternalTitle;
                                    branchingLogic.ResultDescription = result.Description;
                                    branchingLogic.SecondsToApply = result.SecondsToApply ?? "0";
                                    branchingLogic.VideoFrameEnabled = result.VideoFrameEnabled ?? false;
                                    branchingLogic.ResultImage = result.Image ?? string.Empty;
                                    branchingLogic.PublicIdForResult = result.PublicId ?? string.Empty;
                                    branchingLogic.ShowResultImage = result.ShowResultImage;
                                    branchingLogic.ActionButtonText = result.ActionButtonText;
                                    branchingLogic.IsDisabled = true;
                                    if (linkedBranchObj != null)
                                    {
                                        branchingLogic.LinkedActionId = linkedBranchObj.DestinationTypeId.HasValue && linkedBranchObj.DestinationTypeId == (int)BranchingLogicEnum.ACTION ? linkedBranchObj.DestinationObjectId : null;
                                        branchingLogic.LinkedContentId = linkedBranchObj.DestinationTypeId.HasValue && linkedBranchObj.DestinationTypeId == (int)BranchingLogicEnum.CONTENT ? linkedBranchObj.DestinationObjectId : null;
                                        branchingLogic.LinkedToType = linkedBranchObj.DestinationTypeId.HasValue ? linkedBranchObj.DestinationTypeId.Value : (int)BranchingLogicEnum.NONE;
                                        branchingLogic.IsDisabled = false;
                                    }
                                    branchingLogic.HideActionButton = result.HideCallToAction.HasValue ? result.HideCallToAction.Value : false;
                                    branchingLogic.MinScore = result.MinScore;
                                    branchingLogic.MaxScore = result.MaxScore;
                                    branchingLogic.IsPersonalityCorrelatedResult = result.IsPersonalityCorrelatedResult;

                                    resultBranchingLogicLst.Add(branchingLogic);
                                }
                                quizBranchingLogic.ResultList = resultBranchingLogicLst;

                                var actionBranchingLogicLst = new List<ActionBranchingLogic>();

                                foreach (var action in quizDetailsObj.ActionsInQuiz.Where(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active))
                                {
                                    var branchingLogic = new ActionBranchingLogic();
                                    branchingLogic.QuizId = QuizId;
                                    branchingLogic.ActionId = action.Id;
                                    branchingLogic.ActionTitle = action.Title;
                                    branchingLogic.ActionType = action.ActionType;
                                    branchingLogic.AppointmentId = action.AppointmentId;
                                    branchingLogic.ReportEmails = action.ReportEmails;
                                    branchingLogic.IsDisabled = true;

                                    actionBranchingLogicLst.Add(branchingLogic);
                                }
                                quizBranchingLogic.ActionList = actionBranchingLogicLst;
                                quizBranchingLogic.IsQuesAndContentInSameTable = quizObj.QuesAndContentInSameTable;

                                quizBranchingLogic.UsageTypes = new List<int>();
                                var usageTypeInQuiz = quizObj.UsageTypeInQuiz.Where(r => r.QuizId == quizObj.Id).Select(v => v.UsageType);
                                if (usageTypeInQuiz != null && usageTypeInQuiz.Any())
                                {
                                    foreach (var item in usageTypeInQuiz.ToList())
                                    {
                                        quizBranchingLogic.UsageTypes.Add(item);
                                    }
                                }

                                var badgeBranchingLogicLst = new List<BadgeBranchingLogic>();

                                foreach (var badge in quizDetailsObj.BadgesInQuiz.Where(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active))
                                {
                                    var branchingLogic = new BadgeBranchingLogic();
                                    var linkedBranchObj = quizDetailsObj.BranchingLogic.FirstOrDefault(a => a.SourceObjectId == badge.Id && a.SourceTypeId == (int)BranchingLogicEnum.BADGENEXT);
                                    branchingLogic.QuizId = QuizId;
                                    branchingLogic.BadgetId = badge.Id;
                                    branchingLogic.BadgetTitle = badge.Title;
                                    branchingLogic.SecondsToApply = badge.SecondsToApply ?? "0";
                                    branchingLogic.VideoFrameEnabled = badge.VideoFrameEnabled ?? false;
                                    branchingLogic.BadgetImage = badge.Image ?? string.Empty;
                                    branchingLogic.PublicIdForBadget = badge.PublicId ?? string.Empty;
                                    if (linkedBranchObj != null)
                                    {
                                        branchingLogic.LinkedQuestionId = linkedBranchObj.DestinationTypeId.HasValue && linkedBranchObj.DestinationTypeId == (int)BranchingLogicEnum.QUESTION ? linkedBranchObj.DestinationObjectId : null;
                                        branchingLogic.LinkedContentId = linkedBranchObj.DestinationTypeId.HasValue && linkedBranchObj.DestinationTypeId == (int)BranchingLogicEnum.CONTENT ? linkedBranchObj.DestinationObjectId : null;
                                        branchingLogic.LinkedToType = linkedBranchObj.DestinationTypeId.HasValue ? linkedBranchObj.DestinationTypeId.Value : (int)BranchingLogicEnum.NONE;
                                        branchingLogic.LinkedResultId = linkedBranchObj.DestinationTypeId.HasValue && linkedBranchObj.DestinationTypeId == (int)BranchingLogicEnum.RESULT ? linkedBranchObj.DestinationObjectId : null;
                                        branchingLogic.IsDisabled = false;
                                    }
                                    badgeBranchingLogicLst.Add(branchingLogic);
                                }
                                quizBranchingLogic.BadgeList = badgeBranchingLogicLst;

                            }
                            else
                            {
                                Status = ResultEnum.Error;
                                ErrorMessage = "Quiz branching logic not enabled for the QuizId " + QuizId;
                            }
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz not found for the QuizId " + QuizId;
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }

            return quizBranchingLogic;
        }


        public QuizBranchingLogicLinksList GetQuizBranchingLogicData(int QuizId)
        {
            var QuizBranchingLogicLinksLstObj = new QuizBranchingLogicLinksList();
            QuizBranchingLogicLinksLstObj.QuizBranchingLogicLinks = new List<QuizBranchingLogicLinks>();
            QuizBranchingLogicLinks quizBranchingLogic = null;

            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizObj = UOWObj.QuizRepository.GetByID(QuizId);

                    if (quizObj != null)
                    {
                        var quizDetailsObj = quizObj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED);

                        if (quizDetailsObj != null)
                        {
                            if (quizDetailsObj.IsBranchingLogicEnabled.HasValue && quizDetailsObj.IsBranchingLogicEnabled.Value)
                            {
                                foreach (var branchingLogic in quizDetailsObj.BranchingLogic)
                                {
                                    if (branchingLogic.IsStartingPoint)
                                    {
                                        quizBranchingLogic = new QuizBranchingLogicLinks()
                                        {
                                            ObjectType = BranchingLogicEnum.START,
                                            ObjectTypeId = "start",
                                            Links = new List<BranchingLinks>()
                                        };
                                        var link = new BranchingLinks()
                                        {
                                            FromId = "start",
                                            FromType = BranchingLogicEnum.START,
                                            ToId = branchingLogic.SourceTypeId == (int)BranchingLogicEnum.QUESTION
                                            ? "q_" + branchingLogic.SourceObjectId.ToString() : branchingLogic.SourceTypeId == (int)BranchingLogicEnum.ANSWER
                                            ? "a_" + branchingLogic.SourceObjectId.ToString() : (branchingLogic.SourceTypeId == (int)BranchingLogicEnum.CONTENT)
                                            ? "c_" + branchingLogic.SourceObjectId.ToString() : (branchingLogic.SourceTypeId == (int)BranchingLogicEnum.CONTENTNEXT)
                                            ? "cb_" + branchingLogic.SourceObjectId.ToString() : (branchingLogic.SourceTypeId == (int)BranchingLogicEnum.RESULT)
                                            ? "r_" + branchingLogic.SourceObjectId.ToString() : (branchingLogic.SourceTypeId == (int)BranchingLogicEnum.RESULTNEXT)
                                            ? "rb_" + branchingLogic.SourceObjectId.ToString() : (branchingLogic.SourceTypeId == (int)BranchingLogicEnum.ACTION)
                                            ? "ac_" + branchingLogic.SourceObjectId.ToString() : (branchingLogic.SourceTypeId == (int)BranchingLogicEnum.ACTIONNEXT)
                                            ? "acb_" + branchingLogic.SourceObjectId.ToString() : (branchingLogic.SourceTypeId == (int)BranchingLogicEnum.BADGE)
                                            ? "b_" + branchingLogic.SourceObjectId.ToString() : (branchingLogic.SourceTypeId == (int)BranchingLogicEnum.BADGENEXT)
                                            ? "bb_" + branchingLogic.SourceObjectId.ToString() : branchingLogic.SourceTypeId == (int)BranchingLogicEnum.WHATSAPPTEMPLATE
                                            ? "t_" + branchingLogic.SourceObjectId.ToString() : branchingLogic.SourceTypeId == (int)BranchingLogicEnum.WHATSAPPTEMPLATEACTION
                                            ? "ta_" + branchingLogic.SourceObjectId.ToString() : string.Empty,
                                            ToType = (BranchingLogicEnum)branchingLogic.SourceTypeId
                                        };


                                        var CoordinatesInBranchingLogicObj = UOWObj.CoordinatesInBranchingLogicRepository.Get(a => a.ObjectId == QuizId && a.ObjectTypeId == (int)quizBranchingLogic.ObjectType).FirstOrDefault();
                                        if (CoordinatesInBranchingLogicObj != null)
                                        {
                                            quizBranchingLogic.Position = new string[] { CoordinatesInBranchingLogicObj.XCoordinate, CoordinatesInBranchingLogicObj.YCoordinate };
                                        }
                                        quizBranchingLogic.Links.Add(link);
                                        QuizBranchingLogicLinksLstObj.QuizBranchingLogicLinks.Add(quizBranchingLogic);
                                    }

                                    if (branchingLogic.DestinationObjectId > 0
                                        && (!QuizBranchingLogicLinksLstObj.QuizBranchingLogicLinks.Any(a => a.ObjectTypeId == branchingLogic.DestinationObjectId.ToString()
                                        && a.ObjectType == (BranchingLogicEnum)branchingLogic.DestinationTypeId))
                                        && !quizDetailsObj.BranchingLogic.Any(a => a.SourceObjectId == branchingLogic.DestinationObjectId && a.SourceTypeId == branchingLogic.DestinationTypeId))
                                    {
                                        quizBranchingLogic = new QuizBranchingLogicLinks()
                                        {
                                            ObjectType = (BranchingLogicEnum)branchingLogic.DestinationTypeId,
                                            ObjectTypeId = branchingLogic.DestinationObjectId.ToString(),
                                            Links = new List<BranchingLinks>()
                                        };
                                        var link = new BranchingLinks()
                                        {
                                            FromId = string.Empty,
                                            FromType = BranchingLogicEnum.NONE,
                                            ToId = string.Empty,
                                            ToType = BranchingLogicEnum.NONE
                                        };
                                        var quizBranchingLogicObj = QuizBranchingLogicLinksLstObj.QuizBranchingLogicLinks.FirstOrDefault(a => a.ObjectTypeId == quizBranchingLogic.ObjectTypeId && a.ObjectType == quizBranchingLogic.ObjectType);
                                        if (quizBranchingLogicObj != null)
                                        {
                                            quizBranchingLogic = quizBranchingLogicObj;
                                            quizBranchingLogic.Links.Add(link);
                                        }
                                        else
                                        {
                                            var CoordinatesInBranchingLogicObj = UOWObj.CoordinatesInBranchingLogicRepository.Get(a => a.ObjectId == branchingLogic.DestinationObjectId && a.ObjectTypeId == (int)quizBranchingLogic.ObjectType).FirstOrDefault();
                                            if (CoordinatesInBranchingLogicObj != null)
                                            {
                                                quizBranchingLogic.Position = new string[] { CoordinatesInBranchingLogicObj.XCoordinate, CoordinatesInBranchingLogicObj.YCoordinate };
                                            }
                                            quizBranchingLogic.Links.Add(link);
                                            QuizBranchingLogicLinksLstObj.QuizBranchingLogicLinks.Add(quizBranchingLogic);
                                        }

                                    }

                                    if (branchingLogic.SourceTypeId == (int)BranchingLogicEnum.ANSWER)
                                    {
                                        var asd = UOWObj.QuestionsInQuizRepository.Get(a => a.AnswerOptionsInQuizQuestions.Any(r => r.Id == branchingLogic.SourceObjectId)).FirstOrDefault();

                                        quizBranchingLogic = new QuizBranchingLogicLinks()
                                        {
                                            ObjectType = BranchingLogicEnum.QUESTION,
                                            ObjectTypeId = asd.Id.ToString(),
                                            Links = new List<BranchingLinks>()
                                        };
                                    }
                                    else if (branchingLogic.SourceTypeId == (int)BranchingLogicEnum.WHATSAPPTEMPLATEACTION)
                                    {
                                        var asd = UOWObj.QuestionsInQuizRepository.Get(a => a.AnswerOptionsInQuizQuestions.Any(r => r.Id == branchingLogic.SourceObjectId)).FirstOrDefault();
                                        if (asd != null)
                                        {
                                            quizBranchingLogic = new QuizBranchingLogicLinks()
                                            {
                                                ObjectType = BranchingLogicEnum.WHATSAPPTEMPLATE,
                                                ObjectTypeId = asd.Id.ToString(),
                                                Links = new List<BranchingLinks>()
                                            };
                                        }
                                    }
                                    else if (branchingLogic.SourceTypeId == (int)BranchingLogicEnum.QUESTIONNEXT)
                                    {
                                        quizBranchingLogic = new QuizBranchingLogicLinks()
                                        {
                                            ObjectType = BranchingLogicEnum.QUESTION,
                                            ObjectTypeId = branchingLogic.SourceObjectId.ToString(),
                                            Links = new List<BranchingLinks>()
                                        };
                                    }
                                    else
                                    {
                                        quizBranchingLogic = new QuizBranchingLogicLinks()
                                        {
                                            ObjectType = branchingLogic.SourceTypeId == (int)BranchingLogicEnum.ACTIONNEXT ? BranchingLogicEnum.ACTION
                                            : branchingLogic.SourceTypeId == (int)BranchingLogicEnum.RESULTNEXT ? BranchingLogicEnum.RESULT
                                            : branchingLogic.SourceTypeId == (int)BranchingLogicEnum.CONTENTNEXT ? BranchingLogicEnum.CONTENT
                                            : branchingLogic.SourceTypeId == (int)BranchingLogicEnum.ANSWER ? BranchingLogicEnum.QUESTION
                                            : (BranchingLogicEnum)branchingLogic.SourceTypeId,
                                            ObjectTypeId = branchingLogic.SourceObjectId.ToString(),
                                            Links = new List<BranchingLinks>()
                                        };

                                    }

                                    var links = new BranchingLinks()
                                    {
                                        FromId = branchingLogic.SourceTypeId == (int)BranchingLogicEnum.ANSWER ? "a_" + branchingLogic.SourceObjectId.ToString()
                                        : branchingLogic.SourceTypeId == (int)BranchingLogicEnum.CONTENTNEXT ? "cb_" + branchingLogic.SourceObjectId.ToString()
                                        : branchingLogic.SourceTypeId == (int)BranchingLogicEnum.RESULTNEXT ? "rb_" + branchingLogic.SourceObjectId.ToString()
                                        : branchingLogic.SourceTypeId == (int)BranchingLogicEnum.ACTIONNEXT ? "acb_" + branchingLogic.SourceObjectId.ToString()
                                        : branchingLogic.SourceTypeId == (int)BranchingLogicEnum.QUESTIONNEXT ? "qn_" + branchingLogic.SourceObjectId.ToString()
                                        : branchingLogic.SourceTypeId == (int)BranchingLogicEnum.WHATSAPPTEMPLATEACTION ? "ta_" + branchingLogic.SourceObjectId.ToString()
                                        : string.Empty,
                                        FromType = (BranchingLogicEnum)branchingLogic.SourceTypeId,
                                        ToId = branchingLogic.DestinationTypeId == (int)BranchingLogicEnum.QUESTION ? "q_" + branchingLogic.DestinationObjectId.ToString()
                                        : branchingLogic.DestinationTypeId == (int)BranchingLogicEnum.CONTENT ? "c_" + branchingLogic.DestinationObjectId.ToString()
                                        : branchingLogic.DestinationTypeId == (int)BranchingLogicEnum.RESULT ? "r_" + branchingLogic.DestinationObjectId.ToString()
                                        : branchingLogic.DestinationTypeId == (int)BranchingLogicEnum.ACTION ? "ac_" + branchingLogic.DestinationObjectId.ToString()
                                        : (branchingLogic.DestinationTypeId == (int)BranchingLogicEnum.BADGE) ? "b_" + branchingLogic.DestinationObjectId.ToString()
                                        : (branchingLogic.DestinationTypeId == (int)BranchingLogicEnum.BADGENEXT) ? "bb_" + branchingLogic.DestinationObjectId.ToString()
                                        : branchingLogic.DestinationTypeId == (int)BranchingLogicEnum.WHATSAPPTEMPLATE ? "t_" + branchingLogic.DestinationObjectId.ToString()
                                        : string.Empty,
                                        ToType = branchingLogic.DestinationTypeId != null ? (BranchingLogicEnum)branchingLogic.DestinationTypeId : BranchingLogicEnum.NONE,
                                        IsCorrect = branchingLogic.SourceTypeId == (int)BranchingLogicEnum.ANSWER && UOWObj.AnswerOptionsInQuizQuestionsRepository.Get(r => r.Id == branchingLogic.SourceObjectId && r.IsCorrectAnswer.HasValue && r.IsCorrectAnswer.Value).Any()

                                    };
                                    if (branchingLogic.SourceTypeId == (int)BranchingLogicEnum.QUESTION || branchingLogic.SourceTypeId == (int)BranchingLogicEnum.WHATSAPPTEMPLATE)
                                    {
                                        var questionId = branchingLogic.SourceObjectId;
                                        quizBranchingLogic.ObjectTypeId = questionId.ToString();
                                        var quizBranchingLogicObj = QuizBranchingLogicLinksLstObj.QuizBranchingLogicLinks.FirstOrDefault(a => a.ObjectType == quizBranchingLogic.ObjectType && a.ObjectTypeId == quizBranchingLogic.ObjectTypeId);
                                        if (quizBranchingLogicObj != null)
                                        {
                                            if (quizBranchingLogicObj.Links.FirstOrDefault(a => string.IsNullOrEmpty(a.FromId) && string.IsNullOrEmpty(a.ToId)) != null)
                                            {
                                                quizBranchingLogicObj.Links.Remove(quizBranchingLogicObj.Links.FirstOrDefault(a => string.IsNullOrEmpty(a.FromId) && string.IsNullOrEmpty(a.ToId)));
                                            }
                                            quizBranchingLogic = quizBranchingLogicObj;
                                            quizBranchingLogic.Links.Add(links);
                                            if (quizBranchingLogic.Position == null)
                                            {
                                                var CoordinatesInBranchingLogicObj = UOWObj.CoordinatesInBranchingLogicRepository.Get(a => a.ObjectId == questionId && (a.ObjectTypeId == (int)BranchingLogicEnum.QUESTION || a.ObjectTypeId == (int)BranchingLogicEnum.WHATSAPPTEMPLATE)).FirstOrDefault();
                                                if (CoordinatesInBranchingLogicObj != null)
                                                {
                                                    quizBranchingLogic.Position = new string[] { CoordinatesInBranchingLogicObj.XCoordinate, CoordinatesInBranchingLogicObj.YCoordinate };
                                                }
                                            }
                                        }
                                        else
                                        {
                                            var CoordinatesInBranchingLogicObj = UOWObj.CoordinatesInBranchingLogicRepository.Get(a => a.ObjectId == questionId && (a.ObjectTypeId == (int)BranchingLogicEnum.QUESTION || a.ObjectTypeId == (int)BranchingLogicEnum.WHATSAPPTEMPLATE)).FirstOrDefault();
                                            if (CoordinatesInBranchingLogicObj != null)
                                            {
                                                quizBranchingLogic.Position = new string[] { CoordinatesInBranchingLogicObj.XCoordinate, CoordinatesInBranchingLogicObj.YCoordinate };
                                            }
                                            quizBranchingLogic.Links.Add(links);
                                            QuizBranchingLogicLinksLstObj.QuizBranchingLogicLinks.Add(quizBranchingLogic);

                                        }
                                    }
                                    else if (branchingLogic.SourceTypeId == (int)BranchingLogicEnum.ANSWER || branchingLogic.SourceTypeId == (int)BranchingLogicEnum.QUESTIONNEXT || branchingLogic.SourceTypeId == (int)BranchingLogicEnum.WHATSAPPTEMPLATEACTION)
                                    {

                                        var quizBranchingLogicObj = QuizBranchingLogicLinksLstObj.QuizBranchingLogicLinks.FirstOrDefault(a => a.ObjectTypeId == quizBranchingLogic.ObjectTypeId);
                                        if (quizBranchingLogicObj != null)
                                        {
                                            if (quizBranchingLogicObj.Links.FirstOrDefault(a => string.IsNullOrEmpty(a.FromId) && string.IsNullOrEmpty(a.ToId)) != null)
                                            {
                                                quizBranchingLogicObj.Links.Remove(quizBranchingLogicObj.Links.FirstOrDefault(a => string.IsNullOrEmpty(a.FromId) && string.IsNullOrEmpty(a.ToId)));
                                            }
                                            quizBranchingLogic = quizBranchingLogicObj;
                                            quizBranchingLogic.Links.Add(links);
                                            if (quizBranchingLogic.Position == null)
                                            {
                                                var questionsInQuizObj = UOWObj.QuestionsInQuizRepository.Get(q => q.AnswerOptionsInQuizQuestions.Any(r => r.Id == branchingLogic.SourceObjectId)).FirstOrDefault();
                                                var CoordinatesInBranchingLogicObj = UOWObj.CoordinatesInBranchingLogicRepository.Get(a => a.ObjectId == questionsInQuizObj.Id && (a.ObjectTypeId == (int)BranchingLogicEnum.QUESTION || a.ObjectTypeId == (int)BranchingLogicEnum.WHATSAPPTEMPLATE)).FirstOrDefault();
                                                if (CoordinatesInBranchingLogicObj != null)
                                                {
                                                    quizBranchingLogic.Position = new string[] { CoordinatesInBranchingLogicObj.XCoordinate, CoordinatesInBranchingLogicObj.YCoordinate };
                                                }
                                            }
                                        }
                                        else
                                        {
                                            var questionsInQuizObj = UOWObj.QuestionsInQuizRepository.Get(q => q.AnswerOptionsInQuizQuestions.Any(r => r.Id == branchingLogic.SourceObjectId)).FirstOrDefault();
                                            var CoordinatesInBranchingLogicObj = new Db.CoordinatesInBranchingLogic();
                                            if (branchingLogic.SourceTypeId == (int)BranchingLogicEnum.QUESTIONNEXT)
                                                CoordinatesInBranchingLogicObj = UOWObj.CoordinatesInBranchingLogicRepository.Get(a => a.ObjectId == branchingLogic.SourceObjectId && (a.ObjectTypeId == (int)BranchingLogicEnum.QUESTION || a.ObjectTypeId == (int)BranchingLogicEnum.WHATSAPPTEMPLATE)).FirstOrDefault();
                                            else
                                                CoordinatesInBranchingLogicObj = UOWObj.CoordinatesInBranchingLogicRepository.Get(a => a.ObjectId == questionsInQuizObj.Id && (a.ObjectTypeId == (int)BranchingLogicEnum.QUESTION || a.ObjectTypeId == (int)BranchingLogicEnum.WHATSAPPTEMPLATE)).FirstOrDefault();

                                            if (CoordinatesInBranchingLogicObj != null)
                                            {
                                                quizBranchingLogic.Position = new string[] { CoordinatesInBranchingLogicObj.XCoordinate, CoordinatesInBranchingLogicObj.YCoordinate };
                                            }
                                            quizBranchingLogic.Links.Add(links);
                                            QuizBranchingLogicLinksLstObj.QuizBranchingLogicLinks.Add(quizBranchingLogic);
                                        }
                                    }
                                    else
                                    {
                                        var quizBranchingLogicObj = QuizBranchingLogicLinksLstObj.QuizBranchingLogicLinks.FirstOrDefault(a => a.ObjectTypeId == quizBranchingLogic.ObjectTypeId && a.ObjectType == quizBranchingLogic.ObjectType);
                                        if (quizBranchingLogicObj != null)
                                        {
                                            if (quizBranchingLogicObj.Links.FirstOrDefault(a => string.IsNullOrEmpty(a.FromId) && string.IsNullOrEmpty(a.ToId)) != null)
                                            {
                                                quizBranchingLogicObj.Links.Remove(quizBranchingLogicObj.Links.FirstOrDefault(a => string.IsNullOrEmpty(a.FromId) && string.IsNullOrEmpty(a.ToId)));
                                            }
                                            quizBranchingLogic = quizBranchingLogicObj;
                                            quizBranchingLogic.Links.Add(links);
                                            if (quizBranchingLogic.Position == null)
                                            {
                                                var CoordinatesInBranchingLogicObj = UOWObj.CoordinatesInBranchingLogicRepository.Get(a => a.ObjectId == branchingLogic.SourceObjectId && (a.ObjectTypeId == (int)quizBranchingLogic.ObjectType || a.ObjectTypeId == (int)quizBranchingLogic.ObjectType)).FirstOrDefault();
                                                if (CoordinatesInBranchingLogicObj != null)
                                                {
                                                    quizBranchingLogic.Position = new string[] { CoordinatesInBranchingLogicObj.XCoordinate, CoordinatesInBranchingLogicObj.YCoordinate };
                                                }
                                            }
                                        }
                                        else
                                        {
                                            var CoordinatesInBranchingLogicObj = UOWObj.CoordinatesInBranchingLogicRepository.Get(a => a.ObjectId == branchingLogic.SourceObjectId && (a.ObjectTypeId == (int)quizBranchingLogic.ObjectType || a.ObjectTypeId == (int)quizBranchingLogic.ObjectType)).FirstOrDefault();
                                            if (CoordinatesInBranchingLogicObj != null)
                                            {
                                                quizBranchingLogic.Position = new string[] { CoordinatesInBranchingLogicObj.XCoordinate, CoordinatesInBranchingLogicObj.YCoordinate };
                                            }
                                            quizBranchingLogic.Links.Add(links);
                                            QuizBranchingLogicLinksLstObj.QuizBranchingLogicLinks.Add(quizBranchingLogic);
                                        }
                                    }
                                }
                                if (QuizBranchingLogicLinksLstObj.QuizBranchingLogicLinks.Where(a => a.ObjectType == BranchingLogicEnum.START).Count() == 0)
                                {
                                    quizBranchingLogic = new QuizBranchingLogicLinks()
                                    {
                                        ObjectType = BranchingLogicEnum.START,
                                        ObjectTypeId = "start",
                                        Links = new List<BranchingLinks>()
                                    };
                                    var link = new BranchingLinks()
                                    {
                                        FromId = "start",
                                        FromType = BranchingLogicEnum.START,
                                        ToId = string.Empty,
                                        ToType = BranchingLogicEnum.NONE
                                    };

                                    var CoordinatesInBranchingLogicObj = UOWObj.CoordinatesInBranchingLogicRepository.Get(a => a.ObjectId == QuizId && a.ObjectTypeId == (int)quizBranchingLogic.ObjectType).FirstOrDefault();
                                    if (CoordinatesInBranchingLogicObj != null)
                                    {
                                        quizBranchingLogic.Position = new string[] { CoordinatesInBranchingLogicObj.XCoordinate, CoordinatesInBranchingLogicObj.YCoordinate };
                                    }
                                    else
                                        quizBranchingLogic.Position = new string[] { "0", "0" };
                                    quizBranchingLogic.Links.Add(link);
                                    QuizBranchingLogicLinksLstObj.QuizBranchingLogicLinks.Add(quizBranchingLogic);
                                }
                                if (quizObj.QuizType == (int)QuizTypeEnum.Personality || quizObj.QuizType == (int)QuizTypeEnum.PersonalityTemplate)
                                {
                                    var correlatedResult = quizDetailsObj.QuizResults.FirstOrDefault(b => b.IsPersonalityCorrelatedResult == true && b.Status == (int)StatusEnum.Active);
                                    if (correlatedResult != null)
                                    {
                                        if (QuizBranchingLogicLinksLstObj.QuizBranchingLogicLinks.Where(a => a.ObjectType == BranchingLogicEnum.RESULT && a.ObjectTypeId == correlatedResult.Id.ToString()).Count() == 0)
                                        {
                                            quizBranchingLogic = new QuizBranchingLogicLinks()
                                            {
                                                ObjectType = BranchingLogicEnum.RESULT,
                                                ObjectTypeId = correlatedResult.Id.ToString(),
                                                Links = new List<BranchingLinks>()
                                            };

                                            var destinationObjectIds = QuizBranchingLogicLinksLstObj.QuizBranchingLogicLinks.Where(a => !string.IsNullOrWhiteSpace(a.ObjectTypeId)).Select(b => b.ObjectTypeId).Distinct();
                                            var MaxYCoordinate = UOWObj.CoordinatesInBranchingLogicRepository.Get(a => destinationObjectIds.Contains(a.ObjectId.ToString())).Max(b => b.YCoordinate);
                                            if (!string.IsNullOrWhiteSpace(MaxYCoordinate) && double.Parse(MaxYCoordinate) > 350)
                                            {
                                                quizBranchingLogic.Position = new string[] { "400", (double.Parse(MaxYCoordinate) + 150).ToString() };
                                            }
                                            else
                                                quizBranchingLogic.Position = new string[] { "400", "400" };
                                            QuizBranchingLogicLinksLstObj.QuizBranchingLogicLinks.Add(quizBranchingLogic);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Status = ResultEnum.Error;
                                ErrorMessage = "Quiz branching logic not enabled for the QuizId " + QuizId;
                            }
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz not found for the QuizId " + QuizId;
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }

            return QuizBranchingLogicLinksLstObj;
        }



    }
}
