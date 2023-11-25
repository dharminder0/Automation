using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Services.Model
{
    public class QuizReport : Base
    {
        public int QuizId { get; set; }
        public string QuizTitle { get; set; }
        public DateTime QuizCreatedOn { get; set; }
        public string QuizTag { get; set; }
        public List<ReportAttribute> ReportAttributeList { get; set; }

        public class ReportAttribute
        {
            public float TotalCount { get; set; }
            public string AttributeName { get; set; }
            public List<ReportAttributeSeries> SeriesDataList { get; set; }

            public class ReportAttributeSeries
            {
                public DateTime Date { get; set; }
                public float Value { get; set; }
            }
        }
    }

    public class Report : Base
    {
        public int QuizId { get; set; }
        public string QuizTitle { get; set; }
        public int QuizType { get; set; }
        public string TemplateName { get; set; }
        public string TemplateId { get; set; }

        public List<Stage> Stages { get; set; }
        public List<Result> Results { get; set; }
        public List<Question> Questions { get; set; }
        public List<TopRecordsDetail> TopRecordsDetails { get; set; }

        public class Stage
        {
            public int Id { get; set; }
            public int Value { get; set; }
        }
        public class Result
        {
            public int ParentResultId { get; set; }
            public string InternalResultTitle { get; set; }
            public string ExternalResultTitle { get; set; }
            public int Value { get; set; }
        }
        public class Question
        {
            public int ParentQuestionId { get; set; }
            public string QuestionTitle { get; set; }
            public int QuestionType { get; set; }
            public int LeadCountForQuestion { get; set; }
            public List<Answer> Answers { get; set; }
            public List<CommentDetails> Comments { get; set; }
            public OptionTextforRatingTypeQuestion OptionTextforRatingTypeQuestions { get; set; }
            public class Answer
            {
                public int ParentAnswerid { get; set; }
                public string AnswerText { get; set; }
                public string LeadId { get; set; }
                public int LeadCount { get; set; }
                public DateTime? CompletedOn { get; set; }
            }
            public class CommentDetails
            {
                public string AnswerText { get; set; }
                public string LeadId { get; set; }
                public DateTime? CompletedOn { get; set; }
                public string Comment { get; set; }
            }
            public class OptionTextforRatingTypeQuestion
            {
                public string OptionTextforRatingOne { get; set; }
                public string OptionTextforRatingTwo { get; set; }
                public string OptionTextforRatingThree { get; set; }
                public string OptionTextforRatingFour { get; set; }
                public string OptionTextforRatingFive { get; set; }
            }
        }
        public class TopRecordsDetail
        {
            public int ParentResultId { get; set; }
            public string InternalResultTitle { get; set; }
            public string ExternalResultTitle { get; set; }
            public int NumberofLead { get; set; }
            public List<Thing> PositiveThings { get; set; }
            public List<Thing> NegativeThings { get; set; }

            public class Thing
            {
                public string TopicTitle { get; set; }
                public int Rating { get; set; }
            }
        }

    }

    public class LeadReportDetails
    {
        public int QuizId { get; set; }
        public int QuizType { get; set; }
        public string TemplateName { get; set; }
        public string TemplateId { get; set; }
        public Response.OWCLeadUserResponse.LeadUserResponse LeadUserInfo { get; set; }
        public List<LeadReport> leadReports { get; set; }
    }

    public class LeadReport
    {
        public string QuizTitle { get; set; }
        public DateTime? SentDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? CompleteDate { get; set; }
        public List<Result> Results { get; set; }
        public List<Question> Questions { get; set; }
        public class Result
        {
            public int ResultId { get; set; }
            public string InternalResultTitle { get; set; }
            public string ExternalResultTitle { get; set; }
            public DateTime? CompleteDate { get; set; }
        }
        public class Question
        {
            public int QuestionId { get; set; }
            public string QuestionTitle { get; set; }
            public int QuestionType { get; set; }
            public string QuestionImage { get; set; }
            public List<Answer> Answers { get; set; }
            public List<CommentDetails> Comments { get; set; }
            public OptionTextforRatingTypeQuestion OptionTextforRatingTypeQuestions { get; set; }
            public class Answer
            {
                public int AnswerId { get; set; }
                public string AnswerText { get; set; }
                public string OptionImage { get; set; }
                public string PublicId { get; set; }
            }
            public class CommentDetails
            {
                public string AnswerText { get; set; }
                public DateTime? CompletedOn { get; set; }
                public string Comment { get; set; }
            }
            public class OptionTextforRatingTypeQuestion
            {
                public string OptionTextforRatingOne { get; set; }
                public string OptionTextforRatingTwo { get; set; }
                public string OptionTextforRatingThree { get; set; }
                public string OptionTextforRatingFour { get; set; }
                public string OptionTextforRatingFive { get; set; }
            }
        }
    }

    public class NPSReport : Base
    {
        public int QuizId { get; set; }
        public string QuizTitle { get; set; }
        public int QuizType { get; set; }
        public float? NPSScore { get; set; }
        public float? DetractorResultCount { get; set; }
        public float? PassiveResultCount { get; set; }
        public float? PromoterResultCount { get; set; }
        public string TemplateName { get; set; }
        public string TemplateId { get; set; }
        public List<Stage> Stages { get; set; }
        public List<Result> Results { get; set; }
        public List<Question> Questions { get; set; }
        public List<NPSScoreDetail> NPSScoreDetails { get; set; }
        public List<TopRecordsDetail> TopRecordsDetails { get; set; }

        public class Stage
        {
            public int Id { get; set; }
            public int Value { get; set; }
        }
        public class Result
        {
            public int ParentResultId { get; set; }
            public string InternalResultTitle { get; set; }
            public string ExternalResultTitle { get; set; }
            public int Value { get; set; }
            public int MinScore { get; set; }
            public int MaxScore { get; set; }
        }
        public class Question
        {
            public int ParentQuestionId { get; set; }
            public string QuestionTitle { get; set; }
            public int QuestionType { get; set; }
            public int LeadCountForQuestion { get; set; }
            public List<Answer> Answers { get; set; }
            public List<CommentDetails> Comments { get; set; }
            public OptionTextforRatingTypeQuestion OptionTextforRatingTypeQuestions { get; set; }
            public class Answer
            {
                public int ParentAnswerid { get; set; }
                public string AnswerText { get; set; }
                public string LeadId { get; set; }
                public int LeadCount { get; set; }
                public DateTime? CompletedOn { get; set; }
            }
            public class CommentDetails
            {
                public string AnswerText { get; set; }
                public string LeadId { get; set; }
                public DateTime? CompletedOn { get; set; }
                public string Comment { get; set; }
            }
            public class OptionTextforRatingTypeQuestion
            {
                public string OptionTextforRatingOne { get; set; }
                public string OptionTextforRatingTwo { get; set; }
                public string OptionTextforRatingThree { get; set; }
                public string OptionTextforRatingFour { get; set; }
                public string OptionTextforRatingFive { get; set; }
            }
        }
        public class NPSScoreDetail
        {
            //day
            public DateTime Date { get; set; }
            public DayOfWeek Day { get; set; }
            public int DayNumber { get; set; }
            //week
            public int Week { get; set; }
            // month
            public int MonthNumber { get; set; }
            public string MonthName { get; set; }
            //year
            public int Year { get; set; }
            public float NPSScore { get; set; }
        }
        public class TopRecordsDetail
        {
            public int ParentResultId { get; set; }
            public string InternalResultTitle { get; set; }
            public string ExternalResultTitle { get; set; }
            public int NumberofLead { get; set; }
            public List<Thing> PositiveThings { get; set; }
            public List<Thing> NegativeThings { get; set; }

            public class Thing
            {
                public string TopicTitle { get; set; }
                public int Rating { get; set; }
            }
        }
    }

    public class TemplatateDetail
    {
        public int QuizId { get; set; }
        public string QuizTitle { get; set; }
        public int QuizType { get; set; }
        public string TemplateName { get; set; }
        public string TemplateId { get; set; }
        public List<string> AutomationConfigurationIds { get; set; }
    }
}