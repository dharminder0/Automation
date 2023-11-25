using QuizApp.Helpers;
using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Response
{
    public class QuizReportResponse : IResponse
    {
        public string title { get; set; }
        public DateTime createdOn { get; set; }        
        public List<ReportAttribute> data { get; set; }
       
        public class ReportAttribute
        {
            public string total { get; set; }
            public string name { get; set; }
            public List<ReportAttributeSeries> series { get; set; }

            public class ReportAttributeSeries
            {
                public DateTime name { get; set; }
                public string value { get; set; }
            }
        }

        public IResponse MapEntityToResponse(Base EntityObj)
        {
            QuizReportResponse response = new QuizReportResponse();

            var obj = (QuizReport)EntityObj;

            response.title = obj.QuizTitle ?? string.Empty;
            response.createdOn = obj.QuizCreatedOn;

            response.data = new List<ReportAttribute>();

            foreach (var item in obj.ReportAttributeList)
            {
                var reportAttributeObj = new ReportAttribute();

                reportAttributeObj.name = item.AttributeName;
                reportAttributeObj.total = item.AttributeName == ((QuizReportingAttributeEnum)5).ToString() ? Math.Round(item.TotalCount, 1).ToString() : item.TotalCount.ToString();

                reportAttributeObj.series = new List<ReportAttribute.ReportAttributeSeries>();

                foreach (var seriesData in item.SeriesDataList)
                {
                    reportAttributeObj.series.Add(new ReportAttribute.ReportAttributeSeries
                    {
                        name = seriesData.Date,
                        value = item.AttributeName == ((QuizReportingAttributeEnum)5).ToString() ? Math.Round(seriesData.Value, 1).ToString() : seriesData.Value.ToString()
                    });
                }
                response.data.Add(reportAttributeObj);
            }

            return response;
        }
    }

    public class ReportResponse : IResponse
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
            public float? AVGScore { get; set; }
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

        public IResponse MapEntityToResponse(Base EntityObj)
        {
            ReportResponse response = new ReportResponse();

            var obj = (Report)EntityObj;

            response.QuizId = obj.QuizId;
            response.QuizTitle = obj.QuizTitle;
            response.QuizType = obj.QuizType;
            response.TemplateName = obj.TemplateName;
            response.TemplateId = obj.TemplateId;

            response.Stages = new List<Stage>();

            if (obj.Stages != null)
            {
                foreach (var item in obj.Stages)
                {
                    response.Stages.Add(new Stage()
                    {
                        Id = item.Id,
                        Value = item.Value
                    });
                }
            }

            response.Results = new List<Result>();

            if (obj.Results != null)
            {
                foreach (var item in obj.Results)
                {
                    response.Results.Add(new Result()
                    {
                        ParentResultId = item.ParentResultId,
                        ExternalResultTitle = item.ExternalResultTitle,
                        InternalResultTitle = item.InternalResultTitle,
                        Value = item.Value
                    });
                }
            }

            response.Questions = new List<Question>();

            if (obj.Questions != null)
            {
                foreach (var item in obj.Questions)
                {
                    float totalRating = 0;
                    var totalCount = 0;
                    float? avgScore = null;
                    var ansList = new List<Question.Answer>();
                    if (item.Answers != null)
                    {
                        foreach (var ans in item.Answers)
                        {
                            ansList.Add(new Question.Answer()
                            {
                                ParentAnswerid = ans.ParentAnswerid,
                                AnswerText = ans.AnswerText,
                                CompletedOn = ans.CompletedOn,
                                LeadCount = ans.LeadCount,
                                LeadId = ans.LeadId
                            });

                            float res;

                            if (float.TryParse(ans.AnswerText, out res) && (item.QuestionType == (int)AnswerTypeEnum.NPS || item.QuestionType == (int)AnswerTypeEnum.RatingEmoji || item.QuestionType == (int)AnswerTypeEnum.RatingStarts))
                            {
                                totalRating = totalRating + (res * ans.LeadCount);
                                totalCount = totalCount + ans.LeadCount;
                            }
                        }
                    }

                    var commentList = new List<Question.CommentDetails>();
                    if (item.Comments != null)
                    {
                        foreach (var ans in item.Comments)
                        {
                            commentList.Add(new Question.CommentDetails()
                            {
                                AnswerText = ans.AnswerText,
                                CompletedOn = ans.CompletedOn,
                                Comment = ans.Comment,
                                LeadId = ans.LeadId
                            });
                        }
                    }

                    if (item.QuestionType == (int)AnswerTypeEnum.NPS || item.QuestionType == (int)AnswerTypeEnum.RatingEmoji || item.QuestionType == (int)AnswerTypeEnum.RatingStarts)
                        avgScore = totalRating / totalCount;

                    Question.OptionTextforRatingTypeQuestion optionTextforRatingTypeQuestions = null;

                    if (item.OptionTextforRatingTypeQuestions != null)
                    {
                        optionTextforRatingTypeQuestions = new Question.OptionTextforRatingTypeQuestion();
                        optionTextforRatingTypeQuestions.OptionTextforRatingOne = item.OptionTextforRatingTypeQuestions.OptionTextforRatingOne;
                        optionTextforRatingTypeQuestions.OptionTextforRatingTwo = item.OptionTextforRatingTypeQuestions.OptionTextforRatingTwo;
                        optionTextforRatingTypeQuestions.OptionTextforRatingThree = item.OptionTextforRatingTypeQuestions.OptionTextforRatingThree;
                        optionTextforRatingTypeQuestions.OptionTextforRatingFour = item.OptionTextforRatingTypeQuestions.OptionTextforRatingFour;
                        optionTextforRatingTypeQuestions.OptionTextforRatingFive = item.OptionTextforRatingTypeQuestions.OptionTextforRatingFive;
                    }

                    response.Questions.Add(new Question()
                    {
                        ParentQuestionId = item.ParentQuestionId,
                        QuestionTitle = item.QuestionTitle,
                        QuestionType = item.QuestionType,
                        LeadCountForQuestion = item.LeadCountForQuestion,
                        AVGScore = avgScore,
                        Answers = ansList,
                        Comments = commentList,
                        OptionTextforRatingTypeQuestions = optionTextforRatingTypeQuestions
                    });
                }
            }

            response.TopRecordsDetails = new List<TopRecordsDetail>();

            if (obj.TopRecordsDetails != null)
            {
                foreach (var item in obj.TopRecordsDetails)
                {
                    var topRecordsDetailObj = new TopRecordsDetail();
                    topRecordsDetailObj.ParentResultId = item.ParentResultId;
                    topRecordsDetailObj.InternalResultTitle = item.InternalResultTitle;
                    topRecordsDetailObj.ExternalResultTitle = item.ExternalResultTitle;
                    topRecordsDetailObj.NumberofLead = item.NumberofLead;
                    topRecordsDetailObj.PositiveThings = new List<TopRecordsDetail.Thing>();

                    if (item.PositiveThings != null)
                    {
                        foreach (var positiveThings in item.PositiveThings)
                        {
                            topRecordsDetailObj.PositiveThings.Add(new TopRecordsDetail.Thing()
                            {
                                Rating = positiveThings.Rating,
                                TopicTitle = positiveThings.TopicTitle
                            });
                        }
                    }

                    topRecordsDetailObj.NegativeThings = new List<TopRecordsDetail.Thing>();

                    if (item.NegativeThings != null)
                    {
                        foreach (var negativeThings in item.NegativeThings)
                        {
                            topRecordsDetailObj.NegativeThings.Add(new TopRecordsDetail.Thing()
                            {
                                Rating = negativeThings.Rating,
                                TopicTitle = negativeThings.TopicTitle
                            });
                        }
                    }

                    response.TopRecordsDetails.Add(topRecordsDetailObj);
                }
            }

            return response;
        }
    }

    public class NPSAutomationReportResponse : IResponse
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
        }
        public class Question
        {
            public int ParentQuestionId { get; set; }
            public string QuestionTitle { get; set; }
            public int QuestionType { get; set; }
            public int LeadCountForQuestion { get; set; }
            public float? NPSScore { get; set; }
            public float? DetractorResultCount { get; set; }
            public float? PassiveResultCount { get; set; }
            public float? PromoterResultCount { get; set; }
            public float? AVGScore { get; set; }
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

        public IResponse MapEntityToResponse(Base EntityObj)
        {
            NPSAutomationReportResponse response = new NPSAutomationReportResponse();

            var obj = (NPSReport)EntityObj;

            response.QuizId = obj.QuizId;
            response.QuizTitle = obj.QuizTitle;
            response.QuizType = obj.QuizType;
            response.NPSScore = obj.NPSScore;
            response.DetractorResultCount = obj.DetractorResultCount;
            response.PassiveResultCount = obj.PassiveResultCount;
            response.PromoterResultCount = obj.PromoterResultCount;
            response.TemplateId = obj.TemplateId;
            response.TemplateName = obj.TemplateName;

            response.Stages = new List<Stage>();

            if (obj.Stages != null)
            {
                foreach (var item in obj.Stages)
                {
                    response.Stages.Add(new Stage()
                    {
                        Id = item.Id,
                        Value = item.Value
                    });
                }
            }

            response.Results = new List<Result>();

            if (obj.Results != null)
            {
                foreach (var item in obj.Results)
                {
                    response.Results.Add(new Result()
                    {
                        ParentResultId = item.ParentResultId,
                        ExternalResultTitle = item.ExternalResultTitle,
                        InternalResultTitle = item.InternalResultTitle,
                        Value = item.Value
                    });
                }
            }

            response.Questions = new List<Question>();

            if (obj.Questions != null)
            {
                foreach (var item in obj.Questions)
                {
                    float? score = default(float?);
                    float? detractorResultCount = default(float?);
                    float? passiveResultCount = default(float?);
                    float? promoterResultCount = default(float?);

                    var totalCount = 0;
                    float totalRating = 0;
                    float? avgScore = null;
                    var ansList = new List<Question.Answer>();
                    if (item.Answers != null)
                    {
                        foreach (var ans in item.Answers)
                        {
                            ansList.Add(new Question.Answer()
                            {
                                ParentAnswerid = ans.ParentAnswerid,
                                AnswerText = ans.AnswerText,
                                CompletedOn = ans.CompletedOn,
                                LeadCount = ans.LeadCount,
                                LeadId = ans.LeadId
                            });


                            float res;

                            if (float.TryParse(ans.AnswerText, out res) && (item.QuestionType == (int)AnswerTypeEnum.NPS || item.QuestionType == (int)AnswerTypeEnum.RatingEmoji || item.QuestionType == (int)AnswerTypeEnum.RatingStarts))
                            {
                                totalRating = totalRating + (res * ans.LeadCount);
                                totalCount = totalCount + ans.LeadCount;
                            }
                        }

                        if (item.QuestionType == (int)AnswerTypeEnum.NPS)
                        {
                            detractorResultCount = item.Answers.Where(r => Convert.ToInt32(r.AnswerText) >= 0 && Convert.ToInt32(r.AnswerText) <= 6).Sum(r => r.LeadCount);
                            passiveResultCount = item.Answers.Where(r => Convert.ToInt32(r.AnswerText) >= 7 && Convert.ToInt32(r.AnswerText) <= 8).Sum(r => r.LeadCount);
                            promoterResultCount = item.Answers.Where(r => Convert.ToInt32(r.AnswerText) >= 9 && Convert.ToInt32(r.AnswerText) <= 10).Sum(r => r.LeadCount);

                            var total = detractorResultCount + passiveResultCount + promoterResultCount;
                            if (total > 0)
                                score = ((promoterResultCount / total) * 100) - ((detractorResultCount / total) * 100);
                        }
                    }

                    var commentList = new List<Question.CommentDetails>();
                    if (item.Comments != null)
                    {
                        foreach (var ans in item.Comments)
                        {
                            commentList.Add(new Question.CommentDetails()
                            {
                                AnswerText = ans.AnswerText,
                                CompletedOn = ans.CompletedOn,
                                Comment = ans.Comment,
                                LeadId = ans.LeadId
                            });
                        }
                    }

                    if (item.QuestionType == (int)AnswerTypeEnum.NPS || item.QuestionType == (int)AnswerTypeEnum.RatingEmoji || item.QuestionType == (int)AnswerTypeEnum.RatingStarts)
                        avgScore = totalRating / totalCount;

                    Question.OptionTextforRatingTypeQuestion optionTextforRatingTypeQuestions = null;

                    if (item.OptionTextforRatingTypeQuestions != null)
                    {
                        optionTextforRatingTypeQuestions = new Question.OptionTextforRatingTypeQuestion();
                        optionTextforRatingTypeQuestions.OptionTextforRatingOne = item.OptionTextforRatingTypeQuestions.OptionTextforRatingOne;
                        optionTextforRatingTypeQuestions.OptionTextforRatingTwo = item.OptionTextforRatingTypeQuestions.OptionTextforRatingTwo;
                        optionTextforRatingTypeQuestions.OptionTextforRatingThree = item.OptionTextforRatingTypeQuestions.OptionTextforRatingThree;
                        optionTextforRatingTypeQuestions.OptionTextforRatingFour = item.OptionTextforRatingTypeQuestions.OptionTextforRatingFour;
                        optionTextforRatingTypeQuestions.OptionTextforRatingFive = item.OptionTextforRatingTypeQuestions.OptionTextforRatingFive;
                    }

                    response.Questions.Add(new Question()
                    {
                        ParentQuestionId = item.ParentQuestionId,
                        QuestionTitle = item.QuestionTitle,
                        QuestionType = item.QuestionType,
                        LeadCountForQuestion = item.LeadCountForQuestion,
                        NPSScore = score,
                        DetractorResultCount = detractorResultCount,
                        PassiveResultCount = passiveResultCount,
                        PromoterResultCount = promoterResultCount,
                        AVGScore = avgScore,
                        Answers = ansList,
                        Comments = commentList,
                        OptionTextforRatingTypeQuestions = optionTextforRatingTypeQuestions
                    });
                }
            }

            response.NPSScoreDetails = new List<NPSScoreDetail>();

            if (obj.NPSScoreDetails != null)
            {
                foreach (var item in obj.NPSScoreDetails)
                {
                    response.NPSScoreDetails.Add(new NPSScoreDetail()
                    {
                        Date = item.Date,
                        Day =item.Day,
                        DayNumber = item.DayNumber,
                        Week = item.Week,
                        MonthNumber = item.MonthNumber,
                        MonthName = item.MonthName,
                        Year = item.Year,
                        NPSScore = item.NPSScore
                    });
                }
            }

            response.TopRecordsDetails = new List<TopRecordsDetail>();

            if (obj.TopRecordsDetails != null)
            {
                foreach (var item in obj.TopRecordsDetails)
                {
                    var topRecordsDetailObj = new TopRecordsDetail();
                    topRecordsDetailObj.ParentResultId = item.ParentResultId;
                    topRecordsDetailObj.InternalResultTitle = item.InternalResultTitle;
                    topRecordsDetailObj.ExternalResultTitle = item.ExternalResultTitle;
                    topRecordsDetailObj.NumberofLead = item.NumberofLead;
                    topRecordsDetailObj.PositiveThings = new List<TopRecordsDetail.Thing>();

                    if (item.PositiveThings != null)
                    {
                        foreach (var positiveThings in item.PositiveThings)
                        {
                            topRecordsDetailObj.PositiveThings.Add(new TopRecordsDetail.Thing()
                            {
                                Rating = positiveThings.Rating,
                                TopicTitle = positiveThings.TopicTitle
                            });
                        }
                    }

                    topRecordsDetailObj.NegativeThings = new List<TopRecordsDetail.Thing>();

                    if (item.NegativeThings != null)
                    {
                        foreach (var negativeThings in item.NegativeThings)
                        {
                            topRecordsDetailObj.NegativeThings.Add(new TopRecordsDetail.Thing()
                            {
                                Rating = negativeThings.Rating,
                                TopicTitle = negativeThings.TopicTitle
                            });
                        }
                    }

                    response.TopRecordsDetails.Add(topRecordsDetailObj);
                }
            }

            return response;
        }
    }
}