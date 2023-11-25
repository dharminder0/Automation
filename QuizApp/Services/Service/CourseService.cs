using Core.Common.Caching;
using QuizApp.Helpers;
using QuizApp.RepositoryPattern;
using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace QuizApp.Services.Service
{
    public class CourseService : ICourseService
    {
        private ResultEnum status = ResultEnum.Ok;
        private string errormessage = string.Empty;

        public ResultEnum Status { get { return status; } set { status = value; } }
        public string ErrorMessage { get { return errormessage; } set { errormessage = value; } }
        public List<Model.Course> GetDashboardData(long OffsetValue, bool IncludeSharedWithMe, string BusinessUserEmail, string OfficeIds, int BusinessUserId, int CompanyId, bool IsJobRockAcademy)
        {
            var CourseList = new List<Model.Course>();
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var OfficeIdsLst = new List<string>();
                    if (!string.IsNullOrEmpty(OfficeIds))
                    {
                        OfficeIdsLst = OfficeIds.Split(',').Where(a => !string.IsNullOrEmpty(a)).ToList();

                    }

                    IEnumerable<Db.Quiz> CourseLst = new List<Db.Quiz>();

                    string elearningListcacheKey = "ElearningList_CompanyId_" + CompanyId;
                    string jobRockAcademycacheKey = "JobRockAcademyList";
                    var jobRockAcademyListCacheKey = AppLocalCache.Get<IEnumerable<Db.Quiz>>(jobRockAcademycacheKey);
                    var elearningListCacheKey = AppLocalCache.Get<IEnumerable<Db.Quiz>>(elearningListcacheKey);


                    //if (!IsJobRockAcademy && Utility.GetCacheValue("ElearningList_CompanyId_" + CompanyId) != null && ConfigurationManager.AppSettings["Caching"].ToString() == "true")
                    //{

                    //    CourseLst = ((Utility.GetCacheValue("ElearningList_CompanyId_" + CompanyId)) as List<Db.Quiz>)
                    //        .Where(a => IsJobRockAcademy ? (a.Company.CreateAcademyCourseEnabled && a.UserPermissionsInQuiz.Any(r => r.UserTypeId == (int)UserTypeEnum.JobRockAcademy))
                    //                                            : ((a.Company.Id == CompanyId && a.UserPermissionsInQuiz.Any(r => r.UserTypeId == (int)UserTypeEnum.Recruiter))
                    //                                               && (!string.IsNullOrEmpty(a.AccessibleOfficeId) ? (OfficeIdsLst.Count > 0 && OfficeIdsLst.Contains(a.AccessibleOfficeId))
                    //                                                                                               : (IncludeSharedWithMe))));
                    //}

                    if (!IsJobRockAcademy && elearningListCacheKey != null && elearningListCacheKey.Data != null && elearningListCacheKey.Data.Any())
                    {

                        CourseLst = (elearningListCacheKey.Data).ToList()
                            .Where(a => IsJobRockAcademy ? (a.Company.CreateAcademyCourseEnabled && a.UserPermissionsInQuiz.Any(r => r.UserTypeId == (int)UserTypeEnum.JobRockAcademy))
                                                                : ((a.Company.Id == CompanyId && a.UserPermissionsInQuiz.Any(r => r.UserTypeId == (int)UserTypeEnum.Recruiter))
                                                                   && (!string.IsNullOrEmpty(a.AccessibleOfficeId) ? (OfficeIdsLst.Count > 0 && OfficeIdsLst.Contains(a.AccessibleOfficeId))
                                                                                                                   : (IncludeSharedWithMe))));
                    }



                    //else if (IsJobRockAcademy && Utility.GetCacheValue("JobRockAcademyList") != null && ConfigurationManager.AppSettings["Caching"].ToString() == "true")
                    //{
                    //    CourseLst = ((Utility.GetCacheValue("JobRockAcademyList")) as List<Db.Quiz>);
                    //}
                 
                    else if (IsJobRockAcademy && jobRockAcademyListCacheKey != null && jobRockAcademyListCacheKey.Data != null && jobRockAcademyListCacheKey.Data.Any())
                    {
                        CourseLst = (jobRockAcademyListCacheKey.Data).ToList();
                    }

                    else
                    {
                        CourseLst = UOWObj.QuizRepository
                                    .Get(a => a.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active) != null && 
                                             (IsJobRockAcademy ? (a.Company.CreateAcademyCourseEnabled && a.UserPermissionsInQuiz.Any(r => r.UserTypeId == (int)UserTypeEnum.JobRockAcademy))
                                                                : (a.Company.Id == CompanyId && a.UserPermissionsInQuiz.Any(r => r.UserTypeId == (int)UserTypeEnum.Recruiter))),
                                                            includeProperties: "QuizDetails, AttachmentsInQuiz, QuizTagDetails")
                                    .Where(a => IsJobRockAcademy ? true :
                                    (!string.IsNullOrEmpty(a.AccessibleOfficeId) ? (OfficeIdsLst.Count > 0 && OfficeIdsLst.Contains(a.AccessibleOfficeId))
                                                                                                                   : (IncludeSharedWithMe)));
                    }
                    var quizAttemptsList = UOWObj.QuizAttemptsRepository.Get(r => r.QuizDetails.State == (int)QuizStateEnum.PUBLISHED && r.RecruiterUserId == BusinessUserId,
                        includeProperties: "QuizDetails, QuizStats, QuizObjectStats");

                    foreach (var course in CourseLst)
                    {
                        var QuizAttachmentObj = new QuizAttachment()
                        {
                            QuizId = course.Id,
                            Attachments = new List<QuizAttachment.Attachment>()
                        };

                        if (course != null)
                        {
                            foreach (var attachment in course.AttachmentsInQuiz)
                            {
                                var attachmentObj = new QuizAttachment.Attachment()
                                {
                                    Title = attachment.Title,
                                    Description = attachment.Description
                                };

                                QuizAttachmentObj.Attachments.Add(attachmentObj);
                            }
                        }

                        var courseDetail = course.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active);
                        if (courseDetail != null)
                        {
                            var quizAttemptsObj = quizAttemptsList.Where(r => r.QuizDetails.ParentQuizId == course.Id);
                            var quizStatsObj = quizAttemptsObj.Any() ? quizAttemptsObj.LastOrDefault().QuizStats.FirstOrDefault() : null;
                            var courseObj = new Model.Course();

                            courseObj.Id = course.Id;
                            courseObj.AccessibleOfficeId = course.AccessibleOfficeId;
                            courseObj.NumberOfAttempted = quizAttemptsObj.Where(r => r.QuizStats.Any()).Count();
                            courseObj.PublishedCode = course.PublishedCode;
                            courseObj.IsStarted = quizStatsObj != null && !quizStatsObj.CompletedOn.HasValue;
                            courseObj.QuizAttachment = QuizAttachmentObj;
                            courseObj.Title = courseDetail.QuizTitle;
                            courseObj.CoverImage = courseDetail.QuizCoverImage;
                            courseObj.PublicIdForQuizCover = courseDetail.PublicId;
                            courseObj.Type = IsJobRockAcademy ? (int)UserTypeEnum.JobRockAcademy : (int)UserTypeEnum.Recruiter;
                            courseObj.CreatedOn = Utility.ConvertUTCDateToLocalDate(courseDetail.CreatedOn, OffsetValue);

                            if (quizAttemptsObj.LastOrDefault(r => r.QuizStats.Any()) != null)
                            {
                                courseObj.LastAttempt = Utility.ConvertUTCDateToLocalDate(quizAttemptsObj.LastOrDefault(r => r.QuizStats.Any()).CreatedOn, OffsetValue);
                                var quizAttemptObj = (quizStatsObj == null || courseObj.IsStarted) && quizAttemptsObj.LastOrDefault(a => a.Id != quizAttemptsObj.LastOrDefault().Id) != null ? quizAttemptsObj.LastOrDefault(a => a.Id != quizAttemptsObj.LastOrDefault().Id) : quizAttemptsObj.LastOrDefault();
                                var QuizBadgeStats = quizAttemptObj.QuizObjectStats.Where(r => r.TypeId == (int)BranchingLogicEnum.BADGE).FirstOrDefault();
                                if (QuizBadgeStats != null)
                                {
                                    var badgesInQuizObj = UOWObj.BadgesInQuizRepository.GetByID(QuizBadgeStats.ObjectId);
                                    courseObj.QuizBadge = new QuizBadge
                                    {
                                        Id = QuizBadgeStats.Id,
                                        Image = badgesInQuizObj.Image,
                                        PublicIdForBadge = badgesInQuizObj.PublicId,
                                        Title = badgesInQuizObj.Title
                                    };
                                }
                            }

                            CourseList.Add(courseObj);
                        }
                    }

                    //if (!IsJobRockAcademy && Utility.GetCacheValue("ElearningList_CompanyId_" + CompanyId) == null && ConfigurationManager.AppSettings["Caching"].ToString() == "true")
                    if (!IsJobRockAcademy && AppLocalCache.Get(elearningListcacheKey) == null)
                    {
                        WorkPackageService.RefereshCacheHandler(CompanyId, (int)ListTypeEnum.Elearning);
                    }
                    //else if (IsJobRockAcademy && Utility.GetCacheValue("JobRockAcademyList") == null && ConfigurationManager.AppSettings["Caching"].ToString() == "true")
                    else if (IsJobRockAcademy && AppLocalCache.Get(jobRockAcademycacheKey) == null)
                    {
                        WorkPackageService.RefereshCacheHandler(CompanyId, (int)ListTypeEnum.JobRockAcademy);
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }

            return CourseList;
        }


        public List<Model.Course> GetTechnicalRecruiterData(long OffsetValue, string BusinessUserEmail, int Module, int BusinessUserId, BusinessUser UserInfo)
        {
            var CourseList = new List<Model.Course>();
            try
            {
                if ((Module == (int)ModuleTypeEnum.Automation && UserInfo.IsAutomationTechnicalRecruiterCoursePermission) || (Module == (int)ModuleTypeEnum.Appointment && UserInfo.IsAppointmentTechnicalRecruiterCoursePermission || (Module == (int)ModuleTypeEnum.ELearning && UserInfo.IsELearningTechnicalRecruiterCoursePermission) || (Module == (int)ModuleTypeEnum.Canvas && UserInfo.IsCanvasTechnicalRecruiterCoursePermission) || (Module == (int)ModuleTypeEnum.Vacancies && UserInfo.IsVacanciesTechnicalRecruiterCoursePermission) || (Module == (int)ModuleTypeEnum.Contacts && UserInfo.IsContactsTechnicalRecruiterCoursePermission) || (Module == (int)ModuleTypeEnum.Review && UserInfo.IsReviewTechnicalRecruiterCoursePermission) || (Module == (int)ModuleTypeEnum.Reporting && UserInfo.IsReportingTechnicalRecruiterCoursePermission) || (Module == (int)ModuleTypeEnum.Campaigns && UserInfo.IsCampaignsTechnicalRecruiterCoursePermission)))
                {
                    using (var UOWObj = new AutomationUnitOfWork())
                    {
                        var technicalRecruiterCacheKey = "TechnicalRecruiterData";
                        IEnumerable<Db.Quiz> CourseLst = new List<Db.Quiz>();

                        //if (Utility.GetCacheValue("TechnicalRecruiterData") == null || ConfigurationManager.AppSettings["Caching"].ToString() == "false")


                        if (AppLocalCache.Get(technicalRecruiterCacheKey) == null)
                        {
                            CourseLst = UOWObj.QuizRepository
                                        .Get(a => ((a.UserPermissionsInQuiz.Any(r => r.UserTypeId == (int)UserTypeEnum.TechnicalRecruiter))
                                                && (a.ModulePermissionsInQuiz.Any(m => m.ModuleTypeId == Module))
                                                && (a.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active) != null)),
                                        includeProperties: "QuizDetails, AttachmentsInQuiz, QuizTagDetails");
                        }
                        else
                        {
                            //CourseLst = ((Utility.GetCacheValue(technicalRecruiterCacheKey)) as List<Db.Quiz>)
                            //   .Where(a => a.ModulePermissionsInQuiz.Any(m => m.ModuleTypeId == Module));

                            var technicalRecruiterCache = AppLocalCache.Get<IEnumerable<Db.Quiz>>(technicalRecruiterCacheKey);
                            if(technicalRecruiterCache != null && technicalRecruiterCache.Data != null && technicalRecruiterCache.Data.Any())
                            {
                                CourseLst = technicalRecruiterCache.Data.ToList()
                              .Where(a => a.ModulePermissionsInQuiz.Any(m => m.ModuleTypeId == Module));
                            }
                        }

                        var quizAttemptsList = UOWObj.QuizAttemptsRepository.Get(r => r.QuizDetails.State == (int)QuizStateEnum.PUBLISHED && r.RecruiterUserId == BusinessUserId,
                                                                                                        includeProperties: "QuizDetails, QuizStats, QuizObjectStats");

                        foreach (var course in CourseLst)
                        {
                            var QuizAttachmentObj = new QuizAttachment()
                            {
                                QuizId = course.Id,
                                Attachments = new List<QuizAttachment.Attachment>()
                            };

                            foreach (var attachment in course.AttachmentsInQuiz)
                            {
                                var attachmentObj = new QuizAttachment.Attachment()
                                {
                                    Title = attachment.Title,
                                    Description = attachment.Description
                                };

                                QuizAttachmentObj.Attachments.Add(attachmentObj);
                            }

                            var courseDetail = course.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED && r.Status == (int)StatusEnum.Active);
                            if (courseDetail != null)
                            {
                                var quizAttemptsObj = quizAttemptsList.Where(r => r.QuizDetails.ParentQuizId == course.Id);
                                var quizStatsObj = quizAttemptsObj.Any() ? quizAttemptsObj.LastOrDefault().QuizStats.FirstOrDefault() : null;
                                var courseObj = new Model.Course();

                                courseObj.Id = course.Id;
                                courseObj.AccessibleOfficeId = course.AccessibleOfficeId;
                                courseObj.NumberOfAttempted = quizAttemptsObj.Where(r => r.QuizStats.Any()).Count();
                                courseObj.PublishedCode = course.PublishedCode;
                                courseObj.IsStarted = quizStatsObj != null && !quizStatsObj.CompletedOn.HasValue;
                                courseObj.QuizAttachment = QuizAttachmentObj;
                                courseObj.Title = courseDetail.QuizTitle;
                                courseObj.CoverImage = courseDetail.QuizCoverImage;
                                courseObj.PublicIdForQuizCover = courseDetail.PublicId;
                                courseObj.Type = (int)UserTypeEnum.TechnicalRecruiter;
                                courseObj.CreatedOn = Utility.ConvertUTCDateToLocalDate(courseDetail.CreatedOn, OffsetValue);

                                if (quizAttemptsObj.LastOrDefault(r => r.QuizStats.Any()) != null)
                                {
                                    courseObj.LastAttempt = Utility.ConvertUTCDateToLocalDate(quizAttemptsObj.LastOrDefault(r => r.QuizStats.Any()).CreatedOn, OffsetValue);
                                    var quizAttemptObj = (quizStatsObj == null || courseObj.IsStarted) && quizAttemptsObj.LastOrDefault(a => a.Id != quizAttemptsObj.LastOrDefault().Id) != null ? quizAttemptsObj.LastOrDefault(a => a.Id != quizAttemptsObj.LastOrDefault().Id) : quizAttemptsObj.LastOrDefault();
                                    var QuizBadgeStats = quizAttemptObj.QuizObjectStats.Where(r => r.TypeId == (int)BranchingLogicEnum.BADGE).FirstOrDefault();
                                    if (QuizBadgeStats != null)
                                    {
                                        var badgesInQuizObj = UOWObj.BadgesInQuizRepository.GetByID(QuizBadgeStats.ObjectId);
                                        courseObj.QuizBadge = new QuizBadge
                                        {
                                            Id = QuizBadgeStats.Id,
                                            Image = badgesInQuizObj.Image,
                                            PublicIdForBadge = badgesInQuizObj.PublicId,
                                            Title = badgesInQuizObj.Title
                                        };
                                    }
                                }

                                CourseList.Add(courseObj);
                            }
                        }

                        //if (Utility.GetCacheValue("TechnicalRecruiterData") == null && ConfigurationManager.AppSettings["Caching"].ToString() == "true")
                        if (AppLocalCache.Get(technicalRecruiterCacheKey) == null)
                        {
                            WorkPackageService.RefereshCacheHandler(null, (int)ListTypeEnum.TechnicalRecruiteData);
                        }
                    }
                }
                else
                {
                    Status = ResultEnum.Error;
                    ErrorMessage = "Unauthorized";
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }

            return CourseList;
        }

        public List<Model.Dashboard> GetDashboardList(BusinessUser UserInfo)
        {
            var dashboardList = new List<Model.Dashboard>();

            using (var UOWObj = new AutomationUnitOfWork())
            {
                IEnumerable<Db.Quiz> dashboardLst = new List<Db.Quiz>();
                var technicalRecruiterCacheKey = "TechnicalRecruiterList";
                //if (Utility.GetCacheValue("TechnicalRecruiterList") == null || ConfigurationManager.AppSettings["Caching"].ToString() == "false")
                //if (AppLocalCache.Get(technicalRecruiterCacheKey) == null || ConfigurationManager.AppSettings["Caching"].ToString() == "false")
                //{
                //    dashboardLst = UOWObj.QuizRepository
                //                   .Get(a => a.QuizDetails.FirstOrDefault().State == (int)QuizStateEnum.DRAFTED
                //                   && a.QuizDetails.FirstOrDefault().Status == (int)StatusEnum.Active
                //                   && a.UserPermissionsInQuiz.Any(r => r.UserTypeId == (int)UserTypeEnum.TechnicalRecruiter)
                //                   && a.ModulePermissionsInQuiz.Any(),
                //                   includeProperties: "QuizDetails, QuizDetails.QuizAttempts, QuizDetails.QuizAttempts.QuizStats");
                //}
                //else
                //{
                //    dashboardLst = ((Utility.GetCacheValue(technicalRecruiterCacheKey)) as List<Db.Quiz>);
                    
                //}

                if (AppLocalCache.Get(technicalRecruiterCacheKey) == null)
                {
                    dashboardLst = UOWObj.QuizRepository
                                   .Get(a => a.QuizDetails.FirstOrDefault().State == (int)QuizStateEnum.DRAFTED
                                   && a.QuizDetails.FirstOrDefault().Status == (int)StatusEnum.Active
                                   && a.UserPermissionsInQuiz.Any(r => r.UserTypeId == (int)UserTypeEnum.TechnicalRecruiter)
                                   && a.ModulePermissionsInQuiz.Any(),
                                   includeProperties: "QuizDetails, QuizDetails.QuizAttempts, QuizDetails.QuizAttempts.QuizStats").ToList();
                }
                else
                {
                    var dashboardLstcache = AppLocalCache.Get<IEnumerable<Db.Quiz>>(technicalRecruiterCacheKey);
                    if (dashboardLstcache != null && dashboardLstcache.Data != null && dashboardLstcache.Data.Any())
                    {
                        dashboardLst = dashboardLstcache.Data.ToList();
                    }
                }

                    //var dashboardLst = AppLocalCache.GetCacheOrDirectData(technicalRecruiterCacheKey, () =>
                    //{

                    //    var dashboardinnerLst = UOWObj.GetRepositoryInstance<Db.Quiz>()
                    //                   .Get(a => a.QuizDetails.FirstOrDefault().State == (int)QuizStateEnum.DRAFTED
                    //                   && a.QuizDetails.FirstOrDefault().Status == (int)StatusEnum.Active
                    //                   && a.UserPermissionsInQuiz.Any(r => r.UserTypeId == (int)UserTypeEnum.TechnicalRecruiter)
                    //                   && a.ModulePermissionsInQuiz.Any(),
                    //                   includeProperties: "QuizDetails, QuizDetails.QuizAttempts, QuizDetails.QuizAttempts.QuizStats").ToList();

                    //    return dashboardinnerLst;
                    //});

                var list = from s in dashboardLst
                           group s by s.ModulePermissionsInQuiz;

                List<int> modules = new List<int>();

                if (UserInfo.IsAutomationTechnicalRecruiterCoursePermission)
                    modules.Add((int)ModuleTypeEnum.Automation);
                if (UserInfo.IsAppointmentTechnicalRecruiterCoursePermission)
                    modules.Add((int)ModuleTypeEnum.Appointment);
                if (UserInfo.IsELearningTechnicalRecruiterCoursePermission)
                    modules.Add((int)ModuleTypeEnum.ELearning);
                if (UserInfo.IsCanvasTechnicalRecruiterCoursePermission)
                    modules.Add((int)ModuleTypeEnum.Canvas);
                if (UserInfo.IsVacanciesTechnicalRecruiterCoursePermission)
                    modules.Add((int)ModuleTypeEnum.Vacancies);
                if (UserInfo.IsContactsTechnicalRecruiterCoursePermission)
                    modules.Add((int)ModuleTypeEnum.Contacts);
                if (UserInfo.IsReviewTechnicalRecruiterCoursePermission)
                    modules.Add((int)ModuleTypeEnum.Review);
                if (UserInfo.IsReportingTechnicalRecruiterCoursePermission)
                    modules.Add((int)ModuleTypeEnum.Reporting);
                if (UserInfo.IsCampaignsTechnicalRecruiterCoursePermission)
                    modules.Add((int)ModuleTypeEnum.Campaigns);


                foreach (var module in modules)
                {
                    var obj = list.Where(r => r.Key.FirstOrDefault().ModuleTypeId == module);
                    var dashboard = new Model.Dashboard();
                    dashboard.ModuleId = module;
                    dashboard.ModuleName = Enum.GetName(typeof(ModuleTypeEnum), module);
                    dashboard.NumberOfElearning = obj.Count();
                    dashboard.NumberOfAttempted = obj.Where(r => r.Key.FirstOrDefault().Quiz.QuizDetails.Any(tr => tr.QuizAttempts.Any(q => q.RecruiterUserId == UserInfo.BusinessUserId && q.QuizStats.Any(s => s.CompletedOn != null)))).Count();
                    dashboardList.Add(dashboard);
                }

                if (AppLocalCache.Get(technicalRecruiterCacheKey) == null)
                {
                    WorkPackageService.RefereshCacheHandler(null, (int)ListTypeEnum.TechnicalRecruiter);
                }

            }
            return dashboardList;

        }
    }
}
