using Core.Common.Extensions;
using Newtonsoft.Json;
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
using System.Web;

namespace QuizApp.Services.Service
{
    public partial class QuizAttemptService : QuizAttemptServiceBase, IQuizAttemptService
    {
        private QuizAnswerSubmit CompleteBadge(PublishQuizTmpModel publishQuizTmpModel)
        {
            QuizAnswerSubmit quizAnswerSubmit = new QuizAnswerSubmit();

            //int quizStatId = 0;
            //if (!publishQuizTmpModel.IsQuizAlreadyStarted)
            //{
            //    UpdateQuizAttemptViewed(publishQuizTmpModel.QuizAttemptId);
            //    quizStatId = UpdateQuizStat(publishQuizTmpModel.QuizAttemptId);
            //    SyncQuizAttemptWithlead(publishQuizTmpModel);
            //}

            var nextQuestionObj = FetchNextQuestion(publishQuizTmpModel.QuizDetailId, publishQuizTmpModel.IsQuesAndContentInSameTable, publishQuizTmpModel.IsBranchingLogicEnabled);
            if (nextQuestionObj == null)
            {
                quizAnswerSubmit = NextNullquestionCompleteQuestionLastResult(publishQuizTmpModel, quizAnswerSubmit);
                return quizAnswerSubmit;
            }
            else
            {

                quizAnswerSubmit = GetNextQuestionObjectDetails(nextQuestionObj, quizAnswerSubmit, publishQuizTmpModel,null, publishQuizTmpModel.RequestType);
                return quizAnswerSubmit;
            }
        }
        private QuizBadge BadgesInQuiz(PublishQuizTmpModel publishQuizTmpModel, Db.BadgesInQuiz badgesInQuiz)
        {

            var quizBadge = new QuizBadge();
            quizBadge.Image = badgesInQuiz.Image ?? string.Empty;
            quizBadge.PublicIdForBadge = badgesInQuiz.PublicId ?? string.Empty;

            if (badgesInQuiz.EnableMediaFile)
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    //var mediaObjList = UOWObj.MediaVariablesDetailsRepository.Get(r => r.QuizId == publishQuizTmpModel.QuizDetailId && r.ConfigurationDetailsId == publishQuizTmpModel.ConfigurationDetailId && r.ObjectTypeId == (int)BranchingLogicEnum.BADGE && r.ObjectId == badgesInQuiz.Id);
                    var mediaVariables = UOWObj.MediaVariablesDetailsRepository.GetMediaVariablesDetailsByQuizId(publishQuizTmpModel.QuizDetailId);
                    if(mediaVariables != null && mediaVariables.Any())
                    {
                        var mediaObjList = mediaVariables.Where(r => r.ConfigurationDetailsId == publishQuizTmpModel.ConfigurationDetailId && r.ObjectTypeId == (int)BranchingLogicEnum.BADGE && r.ObjectId == badgesInQuiz.Id);
                        if (badgesInQuiz.EnableMediaFile && mediaObjList != null && mediaObjList.Any())
                        {
                            var mediaObj = mediaObjList.FirstOrDefault();
                            quizBadge.Image = mediaObj.ObjectValue;
                            quizBadge.PublicIdForBadge = mediaObj.ObjectPublicId ?? string.Empty;

                            var newMedia = ExtractDynamicMedia(mediaObj, publishQuizTmpModel.LeadDetails, publishQuizTmpModel.UserMediaClassifications, publishQuizTmpModel.LeadOwners);
                            if (newMedia != null)
                            {
                                quizBadge.Image = newMedia.MediaUrl;
                                quizBadge.PublicIdForBadge = newMedia.MediaPublicId;
                            }
                        }
                    }
                }
            }

            //if (publishQuizTmpModel.RecruiterUserId && quizDetails.Quiz.Company.BadgesEnabled)
            if (publishQuizTmpModel.RecruiterUserId > 0 && publishQuizTmpModel.CompanyDetails.BadgesEnabled)
            {
                //var badgesInfo = badgesInfoUpdateJson.Replace("{UserId}", publishQuizTmpModel.RecruiterUserId).Replace("{CourseId}", quizDetails.ParentQuizId.ToString()).Replace("{CourseBadgeName}",
                //    (badgesInQuiz.Title).Replace("{CourseBadgeImageUrl}", quizAnswerSubmit.BadgeDetails.Image).Replace("{CourseTitle}", quizDetails.QuizTitle);

                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var badgesInfo = badgesInfoUpdateJson.Replace("{UserId}", publishQuizTmpModel.RecruiterUserId.ToString()).Replace("{CourseId}", publishQuizTmpModel.ParentQuizid.ToString()).Replace("{CourseBadgeName}",
                     badgesInQuiz.Title).Replace("{CourseBadgeImageUrl}", quizBadge.Image).Replace("{CourseTitle}", publishQuizTmpModel.QuizTitle);

                    var user = UOWObj.UserTokensRepository.Get(r => r.BusinessUserId == publishQuizTmpModel.RecruiterUserId).FirstOrDefault();

                    try
                    {
                        var apiSuccess = OWCHelper.UpdateRecruiterCourseBadgesInfos(badgesInfo, publishQuizTmpModel.CompanyDetails);
                        if (!apiSuccess)
                            AddPendingApi(publishQuizTmpModel.CompanyDetails.JobRocketApiUrl + (GlobalSettings.owcAddRecruiterCourseUrl.ToString()).Replace("{ClientCode}", publishQuizTmpModel.CompanyDetails.ClientCode), publishQuizTmpModel.CompanyDetails.JobRocketApiAuthorizationBearer, badgesInfo, "POST");
                    }
                    catch (Exception ex)
                    {
                        AddPendingApi(publishQuizTmpModel.CompanyDetails.JobRocketApiUrl + (GlobalSettings.owcAddRecruiterCourseUrl.ToString()).Replace("{ClientCode}", publishQuizTmpModel.CompanyDetails.ClientCode), publishQuizTmpModel.CompanyDetails.JobRocketApiAuthorizationBearer, badgesInfo, "POST");
                    }
                }

            }
            quizBadge.Id = badgesInQuiz.Id;
            //quizBadge.Title = VariableLinking(badgesInQuiz.Title, quizDetails, quizAttemptsObj, false, false, null);
            quizBadge.Title = VariableLinking(badgesInQuiz.Title, false, false, null, publishQuizTmpModel);;
            quizBadge.ShowTitle = badgesInQuiz.ShowTitle;
            quizBadge.AutoPlay = badgesInQuiz.AutoPlay;
            quizBadge.SecondsToApply = badgesInQuiz.SecondsToApply;
            quizBadge.VideoFrameEnabled = badgesInQuiz.VideoFrameEnabled;
            quizBadge.DisplayOrderForTitleImage = badgesInQuiz.DisplayOrderForTitleImage;
            quizBadge.DisplayOrderForTitle = badgesInQuiz.DisplayOrderForTitle;
            quizBadge.ShowImage = badgesInQuiz.ShowImage;

            return quizBadge;
        }

    }
}