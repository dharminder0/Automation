using QuizApp.Helpers;
using QuizApp.RepositoryPattern;
using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Configuration;

namespace QuizApp.Services.Service
{
    public class TemplateService : ITemplateService
    {
        private ResultEnum status = ResultEnum.Ok;
        private string errormessage = string.Empty;
        public ResultEnum Status { get { return status; } set { status = value; } }
        public string ErrorMessage { get { return errormessage; } set { errormessage = value; } }
        public QuizTemplate GetTemplateList(string QuizType, string CategoryId, int PageNo, int PageSize)
        {
            QuizTemplate quizTemplate = new QuizTemplate();
            List<Template> lstTemplate = new List<Template>();

            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizTypeIds = string.IsNullOrEmpty(QuizType) ? new List<int>() : QuizType.Split(',').Select(Int32.Parse).ToList();
                    var categoryIds = string.IsNullOrEmpty(CategoryId) ? new List<int>() : CategoryId.Split(',').Select(Int32.Parse).ToList();

                    int totalCount = 0;

                    var templatesWithPagination = UOWObj.QuizRepository
                                                .GetWithPagination(out totalCount, PageNo, PageSize, filter: (r => r.QuizDetails.FirstOrDefault(s => s.State == (int)QuizStateEnum.DRAFTED && s.Status == (int)StatusEnum.Active) != null
                                                          && r.QuizDetails.OrderByDescending(s => s.Version).FirstOrDefault(s => s.State == (int)QuizStateEnum.PUBLISHED) != null
                                                          && quizTypeIds.Contains(r.QuizType)
                                                          && categoryIds.Contains(r.Category.CategoryId)),
                                                          orderBy: r => r.OrderBy(k => k.QuizDetails.OrderByDescending(s => s.Version).FirstOrDefault(s => s.State == (int)QuizStateEnum.PUBLISHED).QuizTitle));

                    quizTemplate.CurrentPageIndex = PageNo;
                    quizTemplate.TotalRecords = totalCount;

                    foreach (var quiz in templatesWithPagination)
                    {
                        var templateObj = new Template();

                        var quizDetailsObj = quiz.QuizDetails.OrderByDescending(s => s.Version).FirstOrDefault(r => r.State == (int)QuizStateEnum.PUBLISHED);

                        if (quizDetailsObj != null)
                        {
                            templateObj.Id = quiz.Id;
                            templateObj.QuizTitle = quizDetailsObj.QuizTitle;
                            templateObj.CoverTitle = quizDetailsObj.QuizCoverTitle;
                            templateObj.CoverImage = quizDetailsObj.QuizCoverImage;
                            templateObj.PublicIdForQuizCover = quizDetailsObj.PublicId;
                            templateObj.TotalQuestion = quizDetailsObj.QuestionsInQuiz.Where(r => r.Status == (int)StatusEnum.Active).Count();
                            templateObj.QuizType = (QuizTypeEnum)quiz.QuizType;
                            templateObj.PublishedCode = quiz.PublishedCode;
                            templateObj.QuizDescription = quizDetailsObj.QuizDescription;

                            lstTemplate.Add(templateObj);
                        }
                    }

                    quizTemplate.Templates = lstTemplate;
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }

            return quizTemplate;
        }

        public QuizTemplate GetDashboardTemplates(long OffsetValue, string QuizTypeId, string CategoryId, string SearchTxt, int PageNo, int PageSize, int OrderByDate, int companyId, string ClientCode)
        {
            QuizTemplate quizDashboardTemplate = new QuizTemplate();
            List<Template> lstTemplate = new List<Template>();

            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizTypeIds = string.IsNullOrEmpty(QuizTypeId) ? new List<int>() : QuizTypeId.Split(',').Select(Int32.Parse).ToList();

                    var categoryIds = string.IsNullOrEmpty(CategoryId) ? new List<int>() : CategoryId.Split(',').Select(Int32.Parse).ToList();

                    int totalCount = 0;

                    var templatesWithPagination = UOWObj.QuizRepository
                        .GetWithPagination(out totalCount, PageNo, PageSize, filter : (r => (r.QuizDetails.FirstOrDefault(s => s.State == (int)QuizStateEnum.DRAFTED).Status != (int)StatusEnum.Deleted)
                                && (quizTypeIds.Contains(r.QuizType))
                                && (string.IsNullOrEmpty(SearchTxt) ? true : (string.IsNullOrEmpty(r.QuizDetails.FirstOrDefault(t => t.State == (int)QuizStateEnum.DRAFTED).QuizTitle) ? false : r.QuizDetails.FirstOrDefault(t => t.State == (int)QuizStateEnum.DRAFTED).QuizTitle.IndexOf(SearchTxt) > -1))
                                && (r.CategoryId.HasValue && categoryIds.Contains(r.Category.CategoryId))
                                && (r.Company.Id == companyId)), orderBy: r => OrderByDate == (int)OrderByEnum.Ascending ? r.OrderBy(s => s.QuizDetails.FirstOrDefault().CreatedOn) : r.OrderByDescending(s => s.QuizDetails.FirstOrDefault().CreatedOn));


                    quizDashboardTemplate.CurrentPageIndex = PageNo;
                    quizDashboardTemplate.TotalRecords = totalCount;

                    foreach (var quiz in templatesWithPagination)
                    {
                        var templateObj = new Template();

                        var quizDetailsObj = quiz.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED);

                        if (quizDetailsObj != null)
                        {
                            templateObj.Id = quiz.Id;
                            templateObj.QuizTitle = quizDetailsObj.QuizTitle;
                            templateObj.IsPublished = quiz.State == (int)QuizStateEnum.PUBLISHED;
                            templateObj.CreatedOn = Utility.ConvertUTCDateToLocalDate(quizDetailsObj.CreatedOn, OffsetValue);
                            templateObj.PublishedCode = quiz.PublishedCode;
                            templateObj.QuizType = (QuizTypeEnum)quiz.QuizType;
                            templateObj.Status = (StatusEnum)quizDetailsObj.Status;
                            templateObj.CategoryId = quiz.Category.CategoryId;

                            lstTemplate.Add(templateObj);
                        }
                    }
                    quizDashboardTemplate.Templates = lstTemplate;
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }

            return quizDashboardTemplate;
        }

        public void SetTemplateStatus(int Id, int TemplateStatus, int BusinessUserId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizObj = UOWObj.QuizRepository.GetByID(Id);

                    if (quizObj != null)
                    {
                        var quizDetailsObj = quizObj.QuizDetails.FirstOrDefault(s => s.State == (int)QuizStateEnum.DRAFTED);

                        if (quizDetailsObj != null && TemplateStatus != (int)StatusEnum.Deleted)
                        {
                            var currentDate = DateTime.UtcNow;

                            quizDetailsObj.Status = TemplateStatus;
                            quizDetailsObj.LastUpdatedOn = currentDate;
                            quizDetailsObj.LastUpdatedBy = BusinessUserId;

                            UOWObj.QuizDetailsRepository.Update(quizDetailsObj);

                            UOWObj.Save();
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz not found for the QuizId " + Id;
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