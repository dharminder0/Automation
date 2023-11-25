using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static QuizApp.Response.QuizAttachmentResponse;

namespace QuizApp.Response
{
    public class CourseResponse : IResponse
    {
    
        public int Id { get; set; }
        public string AccessibleOfficeId { get; set; }
        public DateTime? LastAttempt { get; set; }
        public DateTime CreatedOn { get; set; }
        public int NumberOfAttempted { get; set; }
        public QuizAttachmentResponse QuizAttachment { get; set; }
        public bool IsStarted { get; set; }
        public string PublishedCode { get; set; }
        public string Title { get; set; }
        public string CoverImage { get; set; }
        public string PublicIdForQuizCover { get; set; }
        public int Type { get; set; }
        public QuizBadgeResponse QuizBadge { get; set; }

        public IResponse MapEntityToResponse(Services.Model.Base EntityObj)
        {
            CourseResponse response = new CourseResponse();

            var courseObj = (Services.Model.Course)EntityObj;

            response.Id = courseObj.Id;
            response.AccessibleOfficeId = courseObj.AccessibleOfficeId;
            response.IsStarted = courseObj.IsStarted;
            response.PublishedCode = courseObj.PublishedCode;
            response.NumberOfAttempted = courseObj.NumberOfAttempted;
            response.LastAttempt = courseObj.LastAttempt;
            response.Title = courseObj.Title;
            response.CoverImage = courseObj.CoverImage;
            response.CreatedOn = courseObj.CreatedOn;
            response.PublicIdForQuizCover = courseObj.PublicIdForQuizCover;
            response.Type = courseObj.Type;
            response.QuizAttachment = new QuizAttachmentResponse();
            response.QuizBadge = new QuizBadgeResponse();
            if (courseObj.QuizAttachment != null)
            {
                response.QuizAttachment.QuizId = courseObj.QuizAttachment.QuizId;
                response.QuizAttachment.Attachments = new List<Attachment>();
                if (courseObj.QuizAttachment.Attachments != null)
                {
                    foreach (var attachment in courseObj.QuizAttachment.Attachments)
                    {
                        response.QuizAttachment.Attachments.Add(new Attachment
                        {
                            Title = attachment.Title,
                            Description = attachment.Description
                        });
                    }
                }
            }
            if (courseObj.QuizBadge != null)
            {
                response.QuizBadge.Id = courseObj.QuizBadge.Id;
                response.QuizBadge.Title = courseObj.QuizBadge.Title;
                response.QuizBadge.Image = courseObj.QuizBadge.Image;
            }

            return response;
        }
    }

    public class DashboardResponse : IResponse
    {
        public int ModuleId { get; set; }
        public string ModuleName { get; set; }
        public int NumberOfElearning { get; set; }
        public int NumberOfAttempted { get; set; }

        public IResponse MapEntityToResponse(Services.Model.Base EntityObj)
        {
            DashboardResponse response = new DashboardResponse();

            var dashboardObj = (Services.Model.Dashboard)EntityObj;

            response.ModuleId = dashboardObj.ModuleId;
            response.ModuleName = dashboardObj.ModuleName;
            response.NumberOfElearning = dashboardObj.NumberOfElearning;
            response.NumberOfAttempted = dashboardObj.NumberOfAttempted;

            return response;
        }
    }
}
