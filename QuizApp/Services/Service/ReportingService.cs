using Core.Common.Caching;
using QuizApp.Helpers;
using QuizApp.RepositoryPattern;
using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using static QuizApp.Services.Model.Report;

namespace QuizApp.Services.Service
{
    public class ReportingService : IReportingService
    {
        private ResultEnum status = ResultEnum.Ok;
        private string errormessage = string.Empty;

        public ResultEnum Status { get { return status; } set { status = value; } }
        public string ErrorMessage { get { return errormessage; } set { errormessage = value; } }

        public List<QuizReport> GetQuizReportDetails(string QuizIdCSV, DateTime fromDate, DateTime toDate, string numerator, string denominator)
        {
            List<QuizReport> quizReportList = new List<QuizReport>();

            try
            {
                #region creating reporting data for each quiz in csv

                if (!String.IsNullOrEmpty(QuizIdCSV))
                {
                    foreach (var id in QuizIdCSV.Split(',').Select(Int32.Parse).ToList())
                    {
                        using (var UOWObj = new AutomationUnitOfWork())
                        {
                            var quizObj = UOWObj.QuizRepository.GetByID(id);

                            if (quizObj != null)
                            {
                                var quizReport = new QuizReport();

                                var quizDetailsObj = quizObj.QuizDetails.FirstOrDefault(r => r.Status == (int)StatusEnum.Active && r.State == (int)QuizStateEnum.DRAFTED);

                                if (quizDetailsObj != null)
                                {
                                    quizReport.QuizId = quizObj.Id;
                                    quizReport.QuizTitle = quizDetailsObj.QuizTitle;
                                    quizReport.QuizCreatedOn = quizDetailsObj.CreatedOn;

                                    quizReport.ReportAttributeList = new List<QuizReport.ReportAttribute>();

                                    quizReport.ReportAttributeList = CreateQuizReportAttributeList(fromDate, toDate);

                                    var viewsReportAttr = quizReport.ReportAttributeList.FirstOrDefault(r => r.AttributeName == ((QuizReportingAttributeEnum)1).ToString());
                                    var startsReportAttr = quizReport.ReportAttributeList.FirstOrDefault(r => r.AttributeName == ((QuizReportingAttributeEnum)2).ToString());
                                    var completionsReportAttr = quizReport.ReportAttributeList.FirstOrDefault(r => r.AttributeName == ((QuizReportingAttributeEnum)3).ToString());
                                    var leadsReportAttr = quizReport.ReportAttributeList.FirstOrDefault(r => r.AttributeName == ((QuizReportingAttributeEnum)4).ToString());
                                    var conversionReportAttr = quizReport.ReportAttributeList.FirstOrDefault(r => r.AttributeName == ((QuizReportingAttributeEnum)5).ToString());

                                    #region fetching count values

                                    foreach (var item in quizObj.QuizDetails.Where(r => r.Status == (int)StatusEnum.Active && r.State == (int)QuizStateEnum.PUBLISHED && r.CreatedOn >= fromDate))
                                    {
                                        foreach (var attempt in item.QuizAttempts.Where(r => r.Mode == "AUDIT"))
                                        {
                                            //check for views
                                            if (attempt.LastUpdatedOn.HasValue && attempt.LastUpdatedOn.Value.Date <= toDate && attempt.IsViewed)
                                            {
                                                var viewsReportAttrObj = viewsReportAttr.SeriesDataList.FirstOrDefault(r => r.Date == attempt.LastUpdatedOn.Value.Date);

                                                if (viewsReportAttrObj != null)
                                                {
                                                    viewsReportAttrObj.Value += 1;

                                                    if (denominator == "Views")
                                                    {
                                                        var leadsReportAttrObj = leadsReportAttr.SeriesDataList.FirstOrDefault(r => r.Date == attempt.LastUpdatedOn.Value.Date);
                                                        var startsReportAttrObj = startsReportAttr.SeriesDataList.FirstOrDefault(r => r.Date == attempt.LastUpdatedOn.Value.Date);
                                                        var completionsReportAttrObj = completionsReportAttr.SeriesDataList.FirstOrDefault(r => r.Date == attempt.LastUpdatedOn.Value.Date);
                                                        var convsertionReportAttrObj = conversionReportAttr.SeriesDataList.FirstOrDefault(r => r.Date == attempt.LastUpdatedOn.Value.Date);

                                                        convsertionReportAttrObj.Value = GetPercentValue(numerator, denominator, viewsReportAttrObj, leadsReportAttrObj, startsReportAttrObj, completionsReportAttrObj);
                                                    }
                                                }
                                            }

                                            //check for leads
                                            if (attempt.LeadConvertedOn.HasValue && attempt.LeadConvertedOn.Value.Date <= toDate && !string.IsNullOrEmpty(attempt.LeadUserId))
                                            {
                                                var leadsReportAttrObj = leadsReportAttr.SeriesDataList.FirstOrDefault(r => r.Date == attempt.LeadConvertedOn.Value.Date);

                                                if (leadsReportAttrObj != null)
                                                {
                                                    leadsReportAttrObj.Value += 1;

                                                    if (numerator == "Leads")
                                                    {
                                                        var viewsReportAttrObj = viewsReportAttr.SeriesDataList.FirstOrDefault(r => r.Date == attempt.LeadConvertedOn.Value.Date);
                                                        var startsReportAttrObj = startsReportAttr.SeriesDataList.FirstOrDefault(r => r.Date == attempt.LeadConvertedOn.Value.Date);
                                                        var completionsReportAttrObj = completionsReportAttr.SeriesDataList.FirstOrDefault(r => r.Date == attempt.LeadConvertedOn.Value.Date);
                                                        var convsertionReportAttrObj = conversionReportAttr.SeriesDataList.FirstOrDefault(r => r.Date == attempt.LeadConvertedOn.Value.Date);

                                                        convsertionReportAttrObj.Value = GetPercentValue(numerator, denominator, viewsReportAttrObj, leadsReportAttrObj, startsReportAttrObj, completionsReportAttrObj);
                                                    }
                                                }
                                            }

                                            if (attempt.QuizStats.Count > 0)
                                            {
                                                //check for starts
                                                if (attempt.QuizStats.FirstOrDefault().StartedOn.Date <= toDate)
                                                {
                                                    var startsReportAttrObj = startsReportAttr.SeriesDataList.FirstOrDefault(r => r.Date == attempt.QuizStats.FirstOrDefault().StartedOn.Date);

                                                    if (startsReportAttrObj != null)
                                                    {
                                                        startsReportAttrObj.Value += 1;

                                                        if (numerator == "Starts" || denominator == "Starts")
                                                        {
                                                            var viewsReportAttrObj = viewsReportAttr.SeriesDataList.FirstOrDefault(r => r.Date == attempt.QuizStats.FirstOrDefault().StartedOn.Date);
                                                            var leadsReportAttrObj = leadsReportAttr.SeriesDataList.FirstOrDefault(r => r.Date == attempt.QuizStats.FirstOrDefault().StartedOn.Date);
                                                            var completionsReportAttrObj = completionsReportAttr.SeriesDataList.FirstOrDefault(r => r.Date == attempt.QuizStats.FirstOrDefault().StartedOn.Date);
                                                            var convsertionReportAttrObj = conversionReportAttr.SeriesDataList.FirstOrDefault(r => r.Date == attempt.QuizStats.FirstOrDefault().StartedOn.Date);

                                                            convsertionReportAttrObj.Value = GetPercentValue(numerator, denominator, viewsReportAttrObj, leadsReportAttrObj, startsReportAttrObj, completionsReportAttrObj);
                                                        }
                                                    }
                                                }

                                                //check for completion
                                                if (attempt.QuizStats.FirstOrDefault().CompletedOn.HasValue && attempt.QuizStats.FirstOrDefault().CompletedOn.Value.Date <= toDate)
                                                {
                                                    var completionsReportAttrObj = completionsReportAttr.SeriesDataList.FirstOrDefault(r => r.Date == attempt.QuizStats.FirstOrDefault().CompletedOn.Value.Date);

                                                    if (completionsReportAttrObj != null)
                                                    {
                                                        completionsReportAttrObj.Value += 1;

                                                        if (numerator == "Completions" || denominator == "Completions")
                                                        {
                                                            var viewsReportAttrObj = viewsReportAttr.SeriesDataList.FirstOrDefault(r => r.Date == attempt.QuizStats.FirstOrDefault().StartedOn.Date);
                                                            var leadsReportAttrObj = leadsReportAttr.SeriesDataList.FirstOrDefault(r => r.Date == attempt.QuizStats.FirstOrDefault().StartedOn.Date);
                                                            var startsReportAttrObj = startsReportAttr.SeriesDataList.FirstOrDefault(r => r.Date == attempt.QuizStats.FirstOrDefault().StartedOn.Date);
                                                            var convsertionReportAttrObj = conversionReportAttr.SeriesDataList.FirstOrDefault(r => r.Date == attempt.QuizStats.FirstOrDefault().StartedOn.Date);

                                                            convsertionReportAttrObj.Value = GetPercentValue(numerator, denominator, viewsReportAttrObj, leadsReportAttrObj, startsReportAttrObj, completionsReportAttrObj);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    #endregion

                                    viewsReportAttr.TotalCount = viewsReportAttr.SeriesDataList.Sum(r => r.Value);
                                    startsReportAttr.TotalCount = startsReportAttr.SeriesDataList.Sum(r => r.Value);
                                    completionsReportAttr.TotalCount = completionsReportAttr.SeriesDataList.Sum(r => r.Value);
                                    leadsReportAttr.TotalCount = leadsReportAttr.SeriesDataList.Sum(r => r.Value);

                                    switch (denominator)
                                    {
                                        case "Views":
                                            switch (numerator)
                                            {
                                                case "Leads":
                                                    conversionReportAttr.TotalCount = viewsReportAttr.TotalCount == 0 ? 0.0f : (leadsReportAttr.TotalCount / viewsReportAttr.TotalCount) * 100;
                                                    break;
                                                case "Completions":
                                                    conversionReportAttr.TotalCount = viewsReportAttr.TotalCount == 0 ? 0.0f : (completionsReportAttr.TotalCount / viewsReportAttr.TotalCount) * 100;
                                                    break;
                                                case "Starts":
                                                    conversionReportAttr.TotalCount = viewsReportAttr.TotalCount == 0 ? 0.0f : (startsReportAttr.TotalCount / viewsReportAttr.TotalCount) * 100;
                                                    break;
                                            }
                                            break;
                                        case "Starts":
                                            switch (numerator)
                                            {
                                                case "Leads":
                                                    conversionReportAttr.TotalCount = startsReportAttr.TotalCount == 0 ? 0.0f : (leadsReportAttr.TotalCount / startsReportAttr.TotalCount) * 100;
                                                    break;
                                                case "Completions":
                                                    conversionReportAttr.TotalCount = startsReportAttr.TotalCount == 0 ? 0.0f : (completionsReportAttr.TotalCount / startsReportAttr.TotalCount) * 100;
                                                    break;
                                                case "Starts":
                                                    conversionReportAttr.TotalCount = startsReportAttr.TotalCount == 0 ? 0.0f : (startsReportAttr.TotalCount / startsReportAttr.TotalCount) * 100;
                                                    break;
                                            }
                                            break;
                                        case "Completions":
                                            switch (numerator)
                                            {
                                                case "Leads":
                                                    conversionReportAttr.TotalCount = completionsReportAttr.TotalCount == 0 ? 0.0f : (leadsReportAttr.TotalCount / completionsReportAttr.TotalCount) * 100;
                                                    break;
                                                case "Completions":
                                                    conversionReportAttr.TotalCount = completionsReportAttr.TotalCount == 0 ? 0.0f : (completionsReportAttr.TotalCount / completionsReportAttr.TotalCount) * 100;
                                                    break;
                                                case "Starts":
                                                    conversionReportAttr.TotalCount = completionsReportAttr.TotalCount == 0 ? 0.0f : (startsReportAttr.TotalCount / completionsReportAttr.TotalCount) * 100;
                                                    break;
                                            }
                                            break;
                                    }

                                    quizReportList.Add(quizReport);
                                }
                            }
                        }
                    }
                }

                #endregion

                #region creating aggregate data 

                List<QuizReport.ReportAttribute> aggregateReportingData = new List<QuizReport.ReportAttribute>();

                if (quizReportList.Count == 0)
                {
                    aggregateReportingData = CreateQuizReportAttributeList(fromDate, toDate, false);
                }
                else
                {
                    aggregateReportingData = (from rows in quizReportList.SelectMany(r => r.ReportAttributeList)
                                              where rows.AttributeName != "Conversion"
                                              group rows by rows.AttributeName into gr
                                              let rs = (from r in gr.SelectMany(t => t.SeriesDataList)
                                                        group r by r.Date into nGr
                                                        select new QuizReport.ReportAttribute.ReportAttributeSeries
                                                        {
                                                            Date = nGr.Key,
                                                            Value = nGr.Sum(k => k.Value)
                                                        }).ToList()
                                              select new QuizReport.ReportAttribute
                                              {
                                                  AttributeName = gr.Key,
                                                  TotalCount = rs.Sum(tr => tr.Value),
                                                  SeriesDataList = rs
                                              }).ToList();
                }
                #region calculating for conversion data

                aggregateReportingData.Add(new QuizReport.ReportAttribute
                {
                    AttributeName = ((QuizReportingAttributeEnum)5).ToString(),
                    TotalCount = 0,
                    SeriesDataList = new List<QuizReport.ReportAttribute.ReportAttributeSeries>()
                });

                foreach (var item in aggregateReportingData.Select(r => r.SeriesDataList).FirstOrDefault())
                {
                    var viewsReportAttrObj = item;
                    var leadsReportAttrObj = aggregateReportingData.FirstOrDefault(r => r.AttributeName == ((QuizReportingAttributeEnum)4).ToString()).SeriesDataList.FirstOrDefault(r => r.Date == item.Date);
                    var startsReportAttrObj = aggregateReportingData.FirstOrDefault(r => r.AttributeName == ((QuizReportingAttributeEnum)2).ToString()).SeriesDataList.FirstOrDefault(r => r.Date == item.Date);
                    var completionsReportAttrObj = aggregateReportingData.FirstOrDefault(r => r.AttributeName == ((QuizReportingAttributeEnum)3).ToString()).SeriesDataList.FirstOrDefault(r => r.Date == item.Date);

                    var conversionReportAttrObj = new QuizReport.ReportAttribute.ReportAttributeSeries();

                    conversionReportAttrObj.Date = item.Date;

                    switch (denominator)
                    {
                        case "Views":
                            switch (numerator)
                            {
                                case "Leads":
                                    conversionReportAttrObj.Value = viewsReportAttrObj.Value == 0 ? 0.0f : (leadsReportAttrObj.Value / viewsReportAttrObj.Value) * 100;
                                    break;
                                case "Completions":
                                    conversionReportAttrObj.Value = viewsReportAttrObj.Value == 0 ? 0.0f : (completionsReportAttrObj.Value / viewsReportAttrObj.Value) * 100;
                                    break;
                                case "Starts":
                                    conversionReportAttrObj.Value = viewsReportAttrObj.Value == 0 ? 0.0f : (startsReportAttrObj.Value / viewsReportAttrObj.Value) * 100;
                                    break;
                            }
                            break;
                        case "Starts":
                            switch (numerator)
                            {
                                case "Leads":
                                    conversionReportAttrObj.Value = startsReportAttrObj.Value == 0 ? 0.0f : (leadsReportAttrObj.Value / startsReportAttrObj.Value) * 100;
                                    break;
                                case "Completions":
                                    conversionReportAttrObj.Value = startsReportAttrObj.Value == 0 ? 0.0f : (completionsReportAttrObj.Value / startsReportAttrObj.Value) * 100;
                                    break;
                                case "Starts":
                                    conversionReportAttrObj.Value = startsReportAttrObj.Value == 0 ? 0.0f : (startsReportAttrObj.Value / startsReportAttrObj.Value) * 100;
                                    break;
                            }
                            break;
                        case "Completions":
                            switch (numerator)
                            {
                                case "Leads":
                                    conversionReportAttrObj.Value = completionsReportAttrObj.Value == 0 ? 0.0f : (leadsReportAttrObj.Value / completionsReportAttrObj.Value) * 100;
                                    break;
                                case "Completions":
                                    conversionReportAttrObj.Value = completionsReportAttrObj.Value == 0 ? 0.0f : (completionsReportAttrObj.Value / completionsReportAttrObj.Value) * 100;
                                    break;
                                case "Starts":
                                    conversionReportAttrObj.Value = completionsReportAttrObj.Value == 0 ? 0.0f : (startsReportAttrObj.Value / completionsReportAttrObj.Value) * 100;
                                    break;
                            }
                            break;
                    }

                    aggregateReportingData.FirstOrDefault(r => r.AttributeName == ((QuizReportingAttributeEnum)5).ToString()).SeriesDataList.Add(conversionReportAttrObj);
                }

                #endregion

                var viewsReportAttrTotal = aggregateReportingData.FirstOrDefault(r => r.AttributeName == ((QuizReportingAttributeEnum)1).ToString()).TotalCount;
                var leadsReportAttrTotal = aggregateReportingData.FirstOrDefault(r => r.AttributeName == ((QuizReportingAttributeEnum)4).ToString()).TotalCount;
                var startsReportAttrTotal = aggregateReportingData.FirstOrDefault(r => r.AttributeName == ((QuizReportingAttributeEnum)2).ToString()).TotalCount;
                var completionsReportAttrTotal = aggregateReportingData.FirstOrDefault(r => r.AttributeName == ((QuizReportingAttributeEnum)3).ToString()).TotalCount;

                var conversionReportAttribute = aggregateReportingData.FirstOrDefault(r => r.AttributeName == ((QuizReportingAttributeEnum)5).ToString());

                switch (denominator)
                {
                    case "Views":
                        switch (numerator)
                        {
                            case "Leads":
                                conversionReportAttribute.TotalCount = viewsReportAttrTotal == 0 ? 0.0f : (leadsReportAttrTotal / viewsReportAttrTotal) * 100;
                                break;
                            case "Completions":
                                conversionReportAttribute.TotalCount = viewsReportAttrTotal == 0 ? 0.0f : (completionsReportAttrTotal / viewsReportAttrTotal) * 100;
                                break;
                            case "Starts":
                                conversionReportAttribute.TotalCount = viewsReportAttrTotal == 0 ? 0.0f : (startsReportAttrTotal / viewsReportAttrTotal) * 100;
                                break;
                        }
                        break;
                    case "Starts":
                        switch (numerator)
                        {
                            case "Leads":
                                conversionReportAttribute.TotalCount = startsReportAttrTotal == 0 ? 0.0f : (leadsReportAttrTotal / startsReportAttrTotal) * 100;
                                break;
                            case "Completions":
                                conversionReportAttribute.TotalCount = startsReportAttrTotal == 0 ? 0.0f : (completionsReportAttrTotal / startsReportAttrTotal) * 100;
                                break;
                            case "Starts":
                                conversionReportAttribute.TotalCount = startsReportAttrTotal == 0 ? 0.0f : (startsReportAttrTotal / startsReportAttrTotal) * 100;
                                break;
                        }
                        break;
                    case "Completions":
                        switch (numerator)
                        {
                            case "Leads":
                                conversionReportAttribute.TotalCount = completionsReportAttrTotal == 0 ? 0.0f : (leadsReportAttrTotal / completionsReportAttrTotal) * 100;
                                break;
                            case "Completions":
                                conversionReportAttribute.TotalCount = completionsReportAttrTotal == 0 ? 0.0f : (completionsReportAttrTotal / completionsReportAttrTotal) * 100;
                                break;
                            case "Starts":
                                conversionReportAttribute.TotalCount = completionsReportAttrTotal == 0 ? 0.0f : (startsReportAttrTotal / completionsReportAttrTotal) * 100;
                                break;
                        }
                        break;
                }

                quizReportList.Add(new QuizReport
                {
                    ReportAttributeList = aggregateReportingData
                });

                #endregion
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
            return quizReportList;
        }

        public List<QuizReport.ReportAttribute> CreateQuizReportAttributeList(DateTime fromDate, DateTime toDate, bool IncludeConversionList = true)
        {
            var lst = new List<QuizReport.ReportAttribute>();

            var lstViewsSeries = new List<QuizReport.ReportAttribute.ReportAttributeSeries>();

            DateTime tfromDate = fromDate;

            while (fromDate <= toDate)
            {
                lstViewsSeries.Add(new QuizReport.ReportAttribute.ReportAttributeSeries
                {
                    Date = fromDate.Date,
                    Value = 0
                });

                fromDate = fromDate.AddDays(1);
            }

            lst.Add(new QuizReport.ReportAttribute
            {
                AttributeName = ((QuizReportingAttributeEnum)1).ToString(),
                SeriesDataList = lstViewsSeries
            });

            fromDate = tfromDate;

            var lstStartsSeries = new List<QuizReport.ReportAttribute.ReportAttributeSeries>();

            while (fromDate <= toDate)
            {
                lstStartsSeries.Add(new QuizReport.ReportAttribute.ReportAttributeSeries
                {
                    Date = fromDate.Date,
                    Value = 0
                });

                fromDate = fromDate.AddDays(1);
            }

            lst.Add(new QuizReport.ReportAttribute
            {
                AttributeName = ((QuizReportingAttributeEnum)2).ToString(),
                SeriesDataList = lstStartsSeries
            });

            fromDate = tfromDate;

            var lstCompletionsSeries = new List<QuizReport.ReportAttribute.ReportAttributeSeries>();

            while (fromDate <= toDate)
            {
                lstCompletionsSeries.Add(new QuizReport.ReportAttribute.ReportAttributeSeries
                {
                    Date = fromDate.Date,
                    Value = 0
                });

                fromDate = fromDate.AddDays(1);
            }

            lst.Add(new QuizReport.ReportAttribute
            {
                AttributeName = ((QuizReportingAttributeEnum)3).ToString(),
                SeriesDataList = lstCompletionsSeries
            });

            fromDate = tfromDate;

            var lstLeadsSeries = new List<QuizReport.ReportAttribute.ReportAttributeSeries>();

            while (fromDate <= toDate)
            {
                lstLeadsSeries.Add(new QuizReport.ReportAttribute.ReportAttributeSeries
                {
                    Date = fromDate.Date,
                    Value = 0
                });

                fromDate = fromDate.AddDays(1);
            }

            lst.Add(new QuizReport.ReportAttribute
            {
                AttributeName = ((QuizReportingAttributeEnum)4).ToString(),
                SeriesDataList = lstLeadsSeries
            });

            if (IncludeConversionList)
            {
                fromDate = tfromDate;

                var lstConversionSeries = new List<QuizReport.ReportAttribute.ReportAttributeSeries>();

                while (fromDate <= toDate)
                {
                    lstConversionSeries.Add(new QuizReport.ReportAttribute.ReportAttributeSeries
                    {
                        Date = fromDate.Date,
                        Value = 0
                    });

                    fromDate = fromDate.AddDays(1);
                }

                lst.Add(new QuizReport.ReportAttribute
                {
                    AttributeName = ((QuizReportingAttributeEnum)5).ToString(),
                    SeriesDataList = lstConversionSeries
                });
            }

            return lst;
        }

        public float GetPercentValue(string numerator, string denominator, QuizReport.ReportAttribute.ReportAttributeSeries viewsReportAttrObj, QuizReport.ReportAttribute.ReportAttributeSeries leadsReportAttrObj, QuizReport.ReportAttribute.ReportAttributeSeries startsReportAttrObj, QuizReport.ReportAttribute.ReportAttributeSeries completionsReportAttrObj)
        {
            float percentValue = 0.0f;

            switch (denominator)
            {
                case "Views":
                    switch (numerator)
                    {
                        case "Leads":
                            percentValue = viewsReportAttrObj.Value == 0 ? 0.0f : (leadsReportAttrObj.Value / viewsReportAttrObj.Value) * 100;
                            break;
                        case "Completions":
                            percentValue = viewsReportAttrObj.Value == 0 ? 0.0f : (completionsReportAttrObj.Value / viewsReportAttrObj.Value) * 100;
                            break;
                        case "Starts":
                            percentValue = viewsReportAttrObj.Value == 0 ? 0.0f : (startsReportAttrObj.Value / viewsReportAttrObj.Value) * 100;
                            break;
                    }
                    break;
                case "Starts":
                    switch (numerator)
                    {
                        case "Leads":
                            percentValue = startsReportAttrObj.Value == 0 ? 0.0f : (leadsReportAttrObj.Value / startsReportAttrObj.Value) * 100;
                            break;
                        case "Completions":
                            percentValue = startsReportAttrObj.Value == 0 ? 0.0f : (completionsReportAttrObj.Value / startsReportAttrObj.Value) * 100;
                            break;
                        case "Starts":
                            percentValue = startsReportAttrObj.Value == 0 ? 0.0f : (startsReportAttrObj.Value / startsReportAttrObj.Value) * 100;
                            break;
                    }
                    break;
                case "Completions":
                    switch (numerator)
                    {
                        case "Leads":
                            percentValue = completionsReportAttrObj.Value == 0 ? 0.0f : (leadsReportAttrObj.Value / completionsReportAttrObj.Value) * 100;
                            break;
                        case "Completions":
                            percentValue = completionsReportAttrObj.Value == 0 ? 0.0f : (completionsReportAttrObj.Value / completionsReportAttrObj.Value) * 100;
                            break;
                        case "Starts":
                            percentValue = completionsReportAttrObj.Value == 0 ? 0.0f : (startsReportAttrObj.Value / completionsReportAttrObj.Value) * 100;
                            break;
                    }
                    break;
            }

            return percentValue;
        }

        public Report GetQuizReport(int QuizId, string SourceId, DateTime? FromDate, DateTime? ToDate, int? ResultId, int CompanyId)
        {
            Report quizReport = null;

            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    ToDate = ToDate.HasValue ? ToDate.Value : DateTime.UtcNow.Date;
                    FromDate = FromDate.HasValue ? FromDate.Value : ToDate.Value.AddMonths(-1);

                    //var quizDetailscacheKey = "QuizDetails_QuizId_" + QuizId;
                    //var quizReportcacheKey = "QuizReportList_QuizId_" + QuizId;
                    var quizObj = UOWObj.QuizRepository.GetByID(QuizId);

                    if (quizObj != null && quizObj.CompanyId == CompanyId)
                    {
                         Db.QuizDetails quizDetailsObj = new Db.QuizDetails();
                        //var quizDetailsObj = new List<Db.QuizDetails>();

                        //if (Utility.GetCacheValue("QuizDetails_QuizId_" + QuizId) == null || ConfigurationManager.AppSettings["Caching"].ToString() == "false")

                        //if (AppLocalCache.Get("QuizDetails_QuizId_" + QuizId;) == null)
                        //{
                        //    quizDetailsObj = quizObj.QuizDetails.FirstOrDefault(r => r.Status == (int)StatusEnum.Active && r.State == (int)QuizStateEnum.DRAFTED);
                        //}
                        //else
                        //{
                        //    quizDetailsObj = ((Utility.GetCacheValue(quizDetailscacheKey)) as Db.QuizDetails);
                        //}

                        //var quizDetailsObj = AppLocalCache.GetCacheOrDirectData(quizDetailscacheKey, () =>
                        //{
                        //    return quizObj.QuizDetails.FirstOrDefault(r => r.Status == (int)StatusEnum.Active && r.State == (int)QuizStateEnum.DRAFTED);
                        //});

                        quizDetailsObj = quizObj.QuizDetails.FirstOrDefault(r => r.Status == (int)StatusEnum.Active && r.State == (int)QuizStateEnum.DRAFTED);

                        if (quizDetailsObj != null)
                        {
                            quizReport = new Report();

                            quizReport.Stages = new List<Stage>();
                            quizReport.Results = new List<Report.Result>();
                            quizReport.Questions = new List<Question>();
                            quizReport.TopRecordsDetails = new List<TopRecordsDetail>();

                            quizReport.QuizId = QuizId;
                            quizReport.QuizType = quizObj.QuizType;
                            quizReport.QuizTitle = quizDetailsObj.QuizTitle;

                            List<Db.QuizAttempts> quizAttempts = new List<Db.QuizAttempts>();

                            //if (Utility.GetCacheValue("QuizReportList_QuizId_" + QuizId) == null || ConfigurationManager.AppSettings["Caching"].ToString() == "false")
                            //if (AppLocalCache.Get(quizReportcacheKey) == null )
                            //{
                                quizAttempts = UOWObj.QuizAttemptsRepository.Get(filter: r => r.Mode == "AUDIT"
                                               && r.QuizDetails.ParentQuizId == QuizId
                                               && (string.IsNullOrEmpty(SourceId) ? true : ((r.ConfigurationDetails != null && r.ConfigurationDetails.SourceId == SourceId)
                                                  || (r.WorkPackageInfoId != null && r.WorkPackageInfo.CampaignId == SourceId)))
                                               && r.RecruiterUserId == null && r.CompanyId == CompanyId
                            , includeProperties: "QuizStats, QuizQuestionStats").ToList();
                            //}
                            //else
                            //{
                            //    //quizAttempts = ((Utility.GetCacheValue(quizReportcacheKey)) as List<Db.QuizAttempts>)
                            //    //    .Where(r => (string.IsNullOrEmpty(SourceId) ? true : ((r.ConfigurationDetails != null && r.ConfigurationDetails.SourceId == SourceId)
                            //    //                  || (r.WorkPackageInfoId != null && r.WorkPackageInfo.CampaignId == SourceId)))
                            //    //               && r.RecruiterUserId == null);

                            //    var quizAttemptscache = AppLocalCache.Get<IEnumerable<Db.QuizAttempts>>(quizReportcacheKey);
                            //    if(quizAttemptscache != null && quizAttemptscache.Data !=null  && quizAttemptscache.Data.Any())
                            //    {
                            //        quizAttempts = quizAttemptscache.Data.ToList()
                            //       .Where(r => (string.IsNullOrEmpty(SourceId) ? true : ((r.ConfigurationDetails != null && r.ConfigurationDetails.SourceId == SourceId)
                            //                     || (r.WorkPackageInfoId != null && r.WorkPackageInfo.CampaignId == SourceId)))
                            //                  && r.RecruiterUserId == null);
                            //    }
                            //}

                            var workpackageInfoObj = UOWObj.WorkPackageInfoRepository.Get(r => r.QuizId == QuizId).ToList();

                            //var sentReportAttrObj = workpackageInfoObj.Count(r => (r.CreatedOn.HasValue ? (r.CreatedOn.Value.Date >= FromDate && r.CreatedOn.Value.Date <= ToDate) : false) && (string.IsNullOrEmpty(SourceId) ? true : (r.CampaignId == SourceId)))
                            //    + quizAttempts.Count(r => r.WorkPackageInfoId == null && r.CreatedOn.Date >= FromDate && r.CreatedOn.Date <= ToDate);

                            //var sentReportAttrObj = workpackageInfoObj.Count(r => (r.CreatedOn.HasValue ? (r.CreatedOn.Value.Date >= FromDate && r.CreatedOn.Value.Date <= ToDate) : false) && (string.IsNullOrEmpty(SourceId) ? true : (r.CampaignId == SourceId)));



                            var sentReportAttrObj =  quizAttempts.Count(r => r.WorkPackageInfoId == null  && r.CreatedOn.Date >= FromDate && r.CreatedOn.Date <= ToDate);
                            foreach (var obj in workpackageInfoObj)
                            {
                                if (!quizAttempts.Any(v => v.LeadUserId == obj.LeadUserId && v.QuizDetails.ParentQuizId == obj.QuizId && v.ConfigurationDetailsId == obj.ConfigurationDetailsId && v.WorkPackageInfoId == null))
                                {
                                    if ((obj.CreatedOn.HasValue ? (obj.CreatedOn.Value.Date >= FromDate && obj.CreatedOn.Value.Date <= ToDate) : false) && (string.IsNullOrEmpty(SourceId) ? true : (obj.CampaignId == SourceId)))
                                    {
                                        sentReportAttrObj += 1;
                                    }
                                    
                                }
                            }


                            quizReport.Stages.Add(new Stage()
                            {
                                Id = (int)QuizReportingAttributeEnum.Sent,
                                Value = sentReportAttrObj
                            });

                            if (quizAttempts.Any())
                            {
                                var questionList = quizDetailsObj.QuestionsInQuiz;

                                var resultList = quizDetailsObj.QuizResults;

                                var quizComponentLogsList = UOWObj.QuizComponentLogsRepository.Get(r => r.QuizDetails.ParentQuizId == QuizId).ToList();

                                var quizAttemptsObj = quizAttempts.Where(r => (r.WorkPackageInfoId != null   && r.WorkPackageInfo.CreatedOn.Value.Date >= FromDate && r.WorkPackageInfo.CreatedOn.Value.Date <= ToDate)
                                                                    || (r.WorkPackageInfoId == null && r.CreatedOn.Date >= FromDate && r.CreatedOn.Date <= ToDate)).ToList();

                                #region data for stages

                                var viewsReportAttrObj = quizAttemptsObj.Count(r => r.IsViewed);
                                var startsReportAttrObj = quizAttemptsObj.Count(r => r.QuizStats.Any());
                                var resultsReportAttrObj = quizAttemptsObj.Count(r => r.QuizStats.Any() && (r.QuizStats.FirstOrDefault().ResultId != null || r.QuizStats.FirstOrDefault().CompletedOn.HasValue));
                                //var completedReportAttrObj = quizAttemptsObj.Count(r => r.QuizStats.Any() && r.QuizStats.FirstOrDefault().CompletedOn.HasValue);
                                var leadsReportAttrObj = quizAttemptsObj.Count(r => r.LeadConvertedOn.HasValue && !string.IsNullOrEmpty(r.LeadUserId));

                                quizReport.Stages.Add(new Stage()
                                {
                                    Id = (int)QuizReportingAttributeEnum.Views,
                                    Value = viewsReportAttrObj
                                });

                                quizReport.Stages.Add(new Stage()
                                {
                                    Id = (int)QuizReportingAttributeEnum.Starts,
                                    Value = startsReportAttrObj
                                });

                                quizReport.Stages.Add(new Stage()
                                {
                                    Id = (int)QuizReportingAttributeEnum.Result,
                                    Value = resultsReportAttrObj
                                });

                                //quizReport.Stages.Add(new Stage()
                                //{
                                //    Id = (int)QuizReportingAttributeEnum.Completions,
                                //    Value = completedReportAttrObj
                                //});

                                quizReport.Stages.Add(new Stage()
                                {
                                    Id = (int)QuizReportingAttributeEnum.Leads,
                                    Value = leadsReportAttrObj
                                });

                                #endregion

                                #region data for results

                                //Total Leads Completed Quiz Or Reached to result
                                var completedOrAchievedResultQuizObj = quizAttemptsObj.Select(r => r.QuizStats.Where(q => q.ResultId.HasValue || q.CompletedOn.HasValue));
                                var completedOrAchievedResultQuizObjCount = completedOrAchievedResultQuizObj.Count(r => r.Any());
                                var parentIdList = new List<int>();

                                foreach (var attempt in completedOrAchievedResultQuizObj)
                                {
                                    foreach (var item in attempt.Where(r => r.ResultId.HasValue))
                                    {
                                        var parentResultObj = quizComponentLogsList.FirstOrDefault(r => r.ObjectTypeId == (int)BranchingLogicEnum.RESULT && r.PublishedObjectId == item.ResultId);
                                        if (parentResultObj != null)
                                            parentIdList.Add(parentResultObj.DraftedObjectId);
                                    }
                                }

                                foreach (var resultId in parentIdList.Distinct())
                                {
                                    var result = resultList.FirstOrDefault(r => r.Id == resultId);
                                    quizReport.Results.Add(new Report.Result()
                                    {
                                        ParentResultId = resultId,
                                        ExternalResultTitle = result.Title,
                                        InternalResultTitle = result.InternalTitle,
                                        Value = parentIdList.Count(r => r == resultId)
                                    });
                                }

                                //for no result
                                quizReport.Results.Add(new Report.Result()
                                {
                                    ParentResultId = 0,
                                    ExternalResultTitle = string.Empty,
                                    InternalResultTitle = string.Empty,
                                    Value = completedOrAchievedResultQuizObjCount - parentIdList.Count()
                                });

                                #endregion

                                #region Questions

                                var quesStatsList = quizAttemptsObj.Where(r => ResultId.HasValue
                                        ? (ResultId == 0 ? r.QuizStats.Any(q => q.CompletedOn.HasValue && !q.ResultId.HasValue)
                                                    : r.QuizStats.Any(q => q.ResultId.HasValue && quizComponentLogsList.FirstOrDefault(c => c.ObjectTypeId == (int)BranchingLogicEnum.RESULT && c.PublishedObjectId == q.ResultId).DraftedObjectId == ResultId.Value))
                                        : true)
                                         .Select(r => r.QuizQuestionStats.Where(q => q.Status == (int)StatusEnum.Active && q.CompletedOn.HasValue)).ToList();

                                var quesParentIdList = new List<int>();

                                foreach (var quesStatsObj in quesStatsList)
                                {
                                    foreach (var item in quesStatsObj.Where(r => r.QuestionsInQuiz.Type == (int)BranchingLogicEnum.QUESTION))
                                    {
                                        var parentQuestionObject = quizComponentLogsList.FirstOrDefault(r => r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION && r.PublishedObjectId == item.QuestionId);
                                        if (parentQuestionObject != null)
                                        {
                                            var parentQuesId = parentQuestionObject.DraftedObjectId;
                                            var ques = questionList.FirstOrDefault(r => r.Id == parentQuesId);
                                            var answerList = ques.AnswerOptionsInQuizQuestions;

                                            var question = quizReport.Questions.FirstOrDefault(r => r.ParentQuestionId == parentQuesId);

                                            if (question != null)
                                            {
                                                question.LeadCountForQuestion = question.LeadCountForQuestion + 1;

                                                var ansLst = new List<Question.Answer>();

                                                if (ques.AnswerType == (int)AnswerTypeEnum.Short || ques.AnswerType == (int)AnswerTypeEnum.Long || ques.AnswerType == (int)AnswerTypeEnum.DOB || ques.AnswerType == (int)AnswerTypeEnum.PostCode)
                                                {
                                                    foreach (var ans in item.QuizAnswerStats)
                                                    {
                                                        var parentAnswerObj = quizComponentLogsList.FirstOrDefault(r => r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.PublishedObjectId == ans.AnswerId);
                                                        if (parentAnswerObj != null)
                                                        {
                                                            question.Answers.Add(new Question.Answer()
                                                            {
                                                                ParentAnswerid = parentAnswerObj.DraftedObjectId,
                                                                AnswerText = ans.AnswerText,
                                                                LeadCount = 1,
                                                                LeadId = item.QuizAttempts.LeadUserId,
                                                                CompletedOn = item.CompletedOn.Value
                                                            });
                                                        }
                                                    }
                                                }
                                                else if (ques.AnswerType == (int)AnswerTypeEnum.FullAddress)
                                                {
                                                    var ans = item.QuizAnswerStats.FirstOrDefault();

                                                    var parentAnswerObj = quizComponentLogsList.FirstOrDefault(r => r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.PublishedObjectId == ans.AnswerId);
                                                    if (parentAnswerObj != null)
                                                    {
                                                        question.Answers.Add(new Question.Answer()
                                                        {
                                                            ParentAnswerid = parentAnswerObj.DraftedObjectId,
                                                            AnswerText = string.Join(", ", item.QuizAnswerStats.Select(r => r.AnswerText)),
                                                            LeadCount = 1,
                                                            LeadId = item.QuizAttempts.LeadUserId,
                                                            CompletedOn = item.CompletedOn.Value
                                                        });
                                                    }
                                                }
                                                else if (ques.AnswerType == (int)AnswerTypeEnum.RatingEmoji || ques.AnswerType == (int)AnswerTypeEnum.RatingStarts || ques.AnswerType == (int)AnswerTypeEnum.NPS || ques.AnswerType == (int)AnswerTypeEnum.Availability)
                                                {
                                                    
                                                    foreach (var ans in item.QuizAnswerStats)
                                                    {
                                                        var parentAnswerObj = quizComponentLogsList.FirstOrDefault(r => r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.PublishedObjectId == ans.AnswerId);
                                                        if (parentAnswerObj != null)
                                                        {
                                                            var parentAnswerId = parentAnswerObj.DraftedObjectId;

                                                            var answer = question.Answers.FirstOrDefault(r => r.ParentAnswerid == parentAnswerId && r.AnswerText == ans.AnswerText);
                                                            if (answer != null)
                                                                answer.LeadCount = answer.LeadCount + 1;
                                                            else
                                                            {
                                                                question.Answers.Add(new Question.Answer()
                                                                {
                                                                    ParentAnswerid = parentAnswerId,
                                                                    AnswerText = ans.AnswerText,
                                                                    LeadCount = 1,
                                                                    LeadId = string.Empty
                                                                });
                                                            }

                                                            if ((ques.AnswerType == (int)AnswerTypeEnum.RatingEmoji || ques.AnswerType == (int)AnswerTypeEnum.RatingStarts || ques.AnswerType == (int)AnswerTypeEnum.Availability) && !string.IsNullOrEmpty(ans.Comment))
                                                            {
                                                                question.Comments.Add(new Question.CommentDetails()
                                                                {
                                                                    AnswerText = ans.AnswerText,
                                                                    LeadId = item.QuizAttempts.LeadUserId,
                                                                    Comment = ans.Comment,
                                                                    CompletedOn = item.CompletedOn.Value
                                                                });
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    foreach (var ans in item.QuizAnswerStats)
                                                    {
                                                        var parentAnswerObj = quizComponentLogsList.FirstOrDefault(r => r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.PublishedObjectId == ans.AnswerId);
                                                        if (parentAnswerObj != null)
                                                        {
                                                            var parentAnswerId = parentAnswerObj.DraftedObjectId;
                                                            var answer = question.Answers.FirstOrDefault(r => r.ParentAnswerid == parentAnswerId);

                                                            if (answer != null)
                                                                answer.LeadCount = answer.LeadCount + 1;
                                                            else
                                                            {
                                                                question.Answers.Add(new Question.Answer()
                                                                {
                                                                    ParentAnswerid = parentAnswerId,
                                                                    AnswerText = answerList.FirstOrDefault(r => r.Id == parentAnswerId).Option,
                                                                    LeadCount = 1,
                                                                    LeadId = string.Empty
                                                                });
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                var ansLst = new List<Question.Answer>();
                                                var commentLst = new List<Question.CommentDetails>();
                                                if (ques.AnswerType == (int)AnswerTypeEnum.FullAddress)
                                                {
                                                    var ans = item.QuizAnswerStats.FirstOrDefault();
                                                    var parentAnswerObj = quizComponentLogsList.FirstOrDefault(r => r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.PublishedObjectId == ans.AnswerId);
                                                    if (parentAnswerObj != null)
                                                    {
                                                        ansLst.Add(new Question.Answer()
                                                        {
                                                            ParentAnswerid = parentAnswerObj.DraftedObjectId,
                                                            AnswerText = string.Join(", ", item.QuizAnswerStats.Select(r => r.AnswerText)),
                                                            LeadCount = 1,
                                                            LeadId = item.QuizAttempts.LeadUserId,
                                                            CompletedOn = item.CompletedOn.Value,
                                                        });
                                                    }
                                                }
                                                else
                                                {
                                                    foreach (var ans in item.QuizAnswerStats)
                                                    {
                                                        var parentAnswerObj = quizComponentLogsList.FirstOrDefault(r => r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.PublishedObjectId == ans.AnswerId);
                                                        if (parentAnswerObj != null)
                                                        {
                                                            var parentAnswerId = parentAnswerObj.DraftedObjectId;

                                                            ansLst.Add(new Question.Answer()
                                                            {
                                                                ParentAnswerid = parentAnswerId,
                                                                AnswerText = (ques.AnswerType == (int)AnswerTypeEnum.Short || ques.AnswerType == (int)AnswerTypeEnum.Long || ques.AnswerType == (int)AnswerTypeEnum.DOB || ques.AnswerType == (int)AnswerTypeEnum.PostCode || ques.AnswerType == (int)AnswerTypeEnum.NPS || ques.AnswerType == (int)AnswerTypeEnum.RatingEmoji || ques.AnswerType == (int)AnswerTypeEnum.RatingStarts || ques.AnswerType == (int)AnswerTypeEnum.Availability)
                                                                ? ans.AnswerText : answerList.FirstOrDefault(r => r.Id == parentAnswerId).Option,
                                                                LeadCount = 1,
                                                                LeadId = (ques.AnswerType == (int)AnswerTypeEnum.Short || ques.AnswerType == (int)AnswerTypeEnum.Long || ques.AnswerType == (int)AnswerTypeEnum.DOB || ques.AnswerType == (int)AnswerTypeEnum.PostCode || ques.AnswerType == (int)AnswerTypeEnum.Availability)
                                                                ? item.QuizAttempts.LeadUserId : string.Empty,
                                                                CompletedOn = (ques.AnswerType == (int)AnswerTypeEnum.Short || ques.AnswerType == (int)AnswerTypeEnum.Long || ques.AnswerType == (int)AnswerTypeEnum.DOB || ques.AnswerType == (int)AnswerTypeEnum.PostCode || ques.AnswerType == (int)AnswerTypeEnum.Availability)
                                                                                ? item.CompletedOn.Value : default(DateTime?)
                                                            });

                                                            if ((ques.AnswerType == (int)AnswerTypeEnum.RatingEmoji || ques.AnswerType == (int)AnswerTypeEnum.RatingStarts || ques.AnswerType == (int)AnswerTypeEnum.Availability) && !string.IsNullOrEmpty(ans.Comment))
                                                            {
                                                                commentLst.Add(new Question.CommentDetails()
                                                                {
                                                                    AnswerText = ans.AnswerText,
                                                                    LeadId = item.QuizAttempts.LeadUserId,
                                                                    Comment = ans.Comment,
                                                                    CompletedOn = item.CompletedOn.Value
                                                                });
                                                            }
                                                        }
                                                    }
                                                }

                                                Question.OptionTextforRatingTypeQuestion optionTextforRatingTypeQuestions = null;

                                                if (ques.AnswerType == (int)AnswerTypeEnum.RatingEmoji || ques.AnswerType == (int)AnswerTypeEnum.RatingStarts)
                                                {
                                                    optionTextforRatingTypeQuestions = new Question.OptionTextforRatingTypeQuestion();
                                                    
                                                    var option = answerList.FirstOrDefault(r => r.Status == (int)StatusEnum.Active);
                                                    optionTextforRatingTypeQuestions.OptionTextforRatingOne = option.OptionTextforRatingOne;
                                                    optionTextforRatingTypeQuestions.OptionTextforRatingTwo = option.OptionTextforRatingTwo;
                                                    optionTextforRatingTypeQuestions.OptionTextforRatingThree = option.OptionTextforRatingThree;
                                                    optionTextforRatingTypeQuestions.OptionTextforRatingFour = option.OptionTextforRatingFour;
                                                    optionTextforRatingTypeQuestions.OptionTextforRatingFive = option.OptionTextforRatingFive;
                                                }

                                                quizReport.Questions.Add(new Question()
                                                {
                                                    ParentQuestionId = parentQuesId,
                                                    QuestionTitle = ques.Question,
                                                    QuestionType = ques.AnswerType,
                                                    Answers = ansLst,
                                                    LeadCountForQuestion = 1,
                                                    Comments = commentLst,
                                                    OptionTextforRatingTypeQuestions = optionTextforRatingTypeQuestions
                                                });
                                            }
                                        }
                                    }
                                }


                                #endregion

                                #region TOP3

                                IEnumerable<Db.QuizQuestionStats> quizQuestionStatsList = new List<Db.QuizQuestionStats>();

                                //if (Utility.GetCacheValue("QuizQuestionStatsList_QuizId_" + QuizId) == null || ConfigurationManager.AppSettings["Caching"].ToString() == "false")
                                //{
                                //    quizQuestionStatsList = UOWObj.QuizQuestionStatsRepository.Get((r => r.Status == (int)StatusEnum.Active && r.CompletedOn.HasValue 
                                //    && (r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.RatingEmoji || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.RatingStarts)))
                                //     .Where(r => quizAttemptsObj.Any(q => q.Id == r.QuizAttemptId));
                                //}
                                //else
                                //{
                                //    quizQuestionStatsList = ((Utility.GetCacheValue("QuizQuestionStatsList_QuizId_" + QuizId)) as List<Db.QuizQuestionStats>)
                                //   .Where(r => quizAttemptsObj.Any(q => q.Id == r.QuizAttemptId));
                                //}


                        
                                //if (AppLocalCache.Get("QuizQuestionStatsList_QuizId_" + QuizId) == null)
                                //{
                                    quizQuestionStatsList = UOWObj.QuizQuestionStatsRepository.Get((r => r.Status == (int)StatusEnum.Active && r.CompletedOn.HasValue
                                    && (r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.RatingEmoji || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.RatingStarts)))
                                     .Where(r => quizAttemptsObj.Any(q => q.Id == r.QuizAttemptId));
                                //}

                                //else
                                //{
                                //    var quizQuestionStatsCache = AppLocalCache.Get<IEnumerable<Db.QuizQuestionStats>>("QuizQuestionStatsList_QuizId_" + QuizId);
                                //    if (quizQuestionStatsCache != null && quizQuestionStatsCache.Data != null && quizQuestionStatsCache.Data.Any())
                                //    {
                                //        quizQuestionStatsList = quizQuestionStatsCache.Data.ToList().Where(r => quizAttemptsObj.Any(q => q.Id == r.QuizAttemptId));
                                //    }
                                //}
                               
                                foreach (var parentId in parentIdList.Distinct())
                                {
                                    var quizQuestionStats = quizQuestionStatsList.Where(r => r.QuizAttempts.QuizStats.Any(q => q.ResultId.HasValue
                                    && quizComponentLogsList.FirstOrDefault(c => c.ObjectTypeId == (int)BranchingLogicEnum.RESULT && c.PublishedObjectId == q.ResultId).DraftedObjectId == parentId));

                                    if (quizQuestionStats.Any())
                                    {
                                        var result = resultList.FirstOrDefault(r => r.Id == parentId);

                                        var topRecordsDetailObject = new TopRecordsDetail();
                                        topRecordsDetailObject.ParentResultId = parentId;
                                        topRecordsDetailObject.InternalResultTitle = result.InternalTitle;
                                        topRecordsDetailObject.ExternalResultTitle = result.Title;
                                        topRecordsDetailObject.NumberofLead = quizAttemptsObj.Count(r => r.QuizStats.Any(q => q.ResultId.HasValue && quizComponentLogsList.FirstOrDefault(c => c.ObjectTypeId == (int)BranchingLogicEnum.RESULT && c.PublishedObjectId == q.ResultId.Value).DraftedObjectId == parentId));
                                        topRecordsDetailObject.PositiveThings = new List<TopRecordsDetail.Thing>();
                                        topRecordsDetailObject.NegativeThings = new List<TopRecordsDetail.Thing>();
                                        foreach (var item in quizQuestionStats.Where(r => r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.RatingEmoji || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.RatingStarts).OrderByDescending(r => Convert.ToInt32(r.QuizAnswerStats.FirstOrDefault().AnswerText)).Take(3))
                                        {
                                            topRecordsDetailObject.PositiveThings.Add(new TopRecordsDetail.Thing()
                                            {
                                                Rating = Convert.ToInt32(item.QuizAnswerStats.FirstOrDefault().AnswerText),
                                                TopicTitle = item.QuestionsInQuiz.TopicTitle
                                            });
                                        }

                                        foreach (var item in quizQuestionStats.Where(r => r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.RatingEmoji || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.RatingStarts).OrderBy(r => Convert.ToInt32(r.QuizAnswerStats.FirstOrDefault().AnswerText)).Take(3))
                                        {
                                            topRecordsDetailObject.NegativeThings.Add(new TopRecordsDetail.Thing()
                                            {
                                                Rating = Convert.ToInt32(item.QuizAnswerStats.FirstOrDefault().AnswerText),
                                                TopicTitle = item.QuestionsInQuiz.TopicTitle
                                            });
                                        }

                                        quizReport.TopRecordsDetails.Add(topRecordsDetailObject);
                                    }
                                }

                                #endregion
                            }

                            //if ((AppLocalCache.Get(quizReportcacheKey) == null || AppLocalCache.Get("QuizQuestionStatsList_QuizId_" + QuizId) == null || AppLocalCache.Get(quizDetailscacheKey) == null) && ConfigurationManager.AppSettings["Caching"].ToString() == "true")
                            //if ((AppLocalCache.Get(quizReportcacheKey) == null || AppLocalCache.Get("QuizQuestionStatsList_QuizId_" + QuizId) == null))
                            //{
                            //    WorkPackageService.RefereshCacheHandler(QuizId, (int)ListTypeEnum.Report);
                            //}
                        }
                        else
                        {
                            Status = ResultEnum.Error;
                            ErrorMessage = "Quiz not found for the QuizId " + QuizId;
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
            return quizReport;
        }

        public LeadReportDetails GetQuizLeadReport(int QuizId, string LeadUserId, int CompanyId)
        {
            LeadReportDetails leadReportDetails = new LeadReportDetails();
            leadReportDetails.leadReports = new List<LeadReport>();

            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizObj = UOWObj.QuizRepository.GetByID(QuizId);

                    if (quizObj != null)
                    {
                        leadReportDetails.QuizId = QuizId;
                        leadReportDetails.QuizType = quizObj.QuizType;
                        CompanyModel companyObj = new CompanyModel()
                        {
                            ClientCode = quizObj.Company.ClientCode
                        };
                        leadReportDetails.LeadUserInfo = OWCHelper.GetLeadUserInfo(LeadUserId, companyObj);
                        var quizAttemptList = UOWObj.QuizAttemptsRepository.Get(filter: r => r.Mode == "AUDIT"
                                               && r.QuizDetails.ParentQuizId == QuizId
                                               && r.LeadUserId == LeadUserId 
                            , includeProperties: "QuizDetails, QuizStats, QuizQuestionStats, QuizQuestionStats.QuizAnswerStats");

                        var workpackageInfo = UOWObj.WorkPackageInfoRepository.Get(r => r.QuizId == QuizId && r.LeadUserId == LeadUserId && !r.QuizAttempts.Any());

                        foreach (var workpackageInfoObj in workpackageInfo.OrderByDescending(r => r.Id))
                        {
                            var quizDetails = workpackageInfoObj.Quiz.QuizDetails.LastOrDefault();
                            var leadReport = new LeadReport();
                            leadReport.Results = new List<LeadReport.Result>();
                            leadReport.Questions = new List<LeadReport.Question>();

                            leadReport.QuizTitle = HttpUtility.HtmlDecode(Regex.Replace(QuizService.VariableLinking(quizDetails.QuizTitle, quizDetails, null, true, false, null), "<.*?>", string.Empty));
                            leadReport.SentDate = workpackageInfoObj.CreatedOn.Value;
                            leadReportDetails.leadReports.Add(leadReport);
                        }

                        foreach (var quizAttempt in quizAttemptList.OrderByDescending(r => r.Id))
                        {
                            var quizDetails = quizAttempt.QuizDetails;
                            var quizStats = quizAttempt.QuizStats;
                            var resultList = quizDetails.QuizResults;

                            //for replace dynamic variable
                            var correctAnsCount = 0;
                            var ShowScoreValue = false;
                            var scoreValueTxt = string.Empty;
                            var resultSetting = quizDetails.ResultSettings.FirstOrDefault();

                            if (resultSetting != null && resultSetting.ShowScoreValue.HasValue && resultSetting.ShowScoreValue.Value)
                            {
                                var attemptedQuestions = quizAttempt.QuizQuestionStats.Where(t => t.Status == (int)StatusEnum.Active && t.CompletedOn != null && (t.QuestionsInQuiz.TimerRequired ? true : t.QuizAnswerStats.Any(r => !r.AnswerOptionsInQuizQuestions.IsUnansweredType)));

                                if (quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate || attemptedQuestions.Any(r => (r.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Any(t => t.Status == (int)StatusEnum.Active && t.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && t.IsCorrectForMultipleAnswer.HasValue && t.IsCorrectForMultipleAnswer.Value)) || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single))
                                {
                                    if (quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                        correctAnsCount = attemptedQuestions.Sum(a => a.QuizAnswerStats.Sum(x => x.AnswerOptionsInQuizQuestions.AssociatedScore)).Value;
                                    else
                                        correctAnsCount = attemptedQuestions.Count(a => (a.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && ((a.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Select(s => s.Id).OrderBy(s => s)).SequenceEqual(a.QuizAnswerStats.Select(r => r.AnswerId).OrderBy(s => s)))) || (a.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single && a.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer == true));

                                    ShowScoreValue = true;
                                    scoreValueTxt = string.IsNullOrEmpty(scoreValueTxt) ? string.Empty : scoreValueTxt.Replace("%score%", ' ' + correctAnsCount.ToString() + ' ').Replace("%total%", ' ' + attemptedQuestions.Count(t => (t.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Any(r => r.Status == (int)StatusEnum.Active && r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value)) || t.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single).ToString() + ' ');
                                }
                            }

                            var leadReport = new LeadReport();
                            leadReport.Results = new List<LeadReport.Result>();
                            leadReport.Questions = new List<LeadReport.Question>();

                            leadReport.QuizTitle = HttpUtility.HtmlDecode(Regex.Replace(QuizService.VariableLinking(quizDetails.QuizTitle, quizDetails, quizAttempt, true, ShowScoreValue, scoreValueTxt), "<.*?>", string.Empty));
                            leadReport.SentDate = quizAttempt.WorkPackageInfoId != null ? quizAttempt.WorkPackageInfo.CreatedOn.Value : quizAttempt.CreatedOn;
                            leadReport.StartDate = quizStats.Any() ? quizStats.FirstOrDefault().StartedOn : default(DateTime?);
                            leadReport.CompleteDate = (quizStats.Any() && quizStats.FirstOrDefault().CompletedOn.HasValue) ? quizStats.FirstOrDefault().CompletedOn.Value : default(DateTime?);

                            var questionList = quizDetails.QuestionsInQuiz;

                            #region data for results

                            //Total Leads Completed Quiz Or Reached to result
                            var completedOrAchievedResultQuizObj = quizStats.Where(r => r.ResultId.HasValue || r.CompletedOn.HasValue);

                            foreach (var quizStatsObj in completedOrAchievedResultQuizObj)
                            {
                                if (quizStatsObj.ResultId.HasValue)
                                {
                                    var result = resultList.FirstOrDefault(r => r.Id == quizStatsObj.ResultId);
                                    leadReport.Results.Add(new LeadReport.Result()
                                    {
                                        ResultId = quizStatsObj.ResultId.Value,
                                        ExternalResultTitle = HttpUtility.HtmlDecode(Regex.Replace(QuizService.VariableLinking(result.Title, quizDetails, quizAttempt, false, ShowScoreValue, scoreValueTxt), "<.*?>", string.Empty)),
                                        InternalResultTitle = HttpUtility.HtmlDecode(Regex.Replace(QuizService.VariableLinking(result.InternalTitle, quizDetails, quizAttempt, false, ShowScoreValue, scoreValueTxt), "<.*?>", string.Empty)),
                                        CompleteDate = quizStatsObj.CompletedOn.HasValue ? quizStatsObj.CompletedOn.Value : default(DateTime?)
                                    });
                                }
                                else
                                {
                                    //for no result
                                    leadReport.Results.Add(new LeadReport.Result()
                                    {
                                        ResultId = 0,
                                        ExternalResultTitle = string.Empty,
                                        InternalResultTitle = string.Empty,
                                        CompleteDate = quizStatsObj.CompletedOn.HasValue ? quizStatsObj.CompletedOn.Value : default(DateTime?)
                                    });
                                }
                            }

                            #endregion

                            #region Questions

                            var quesStatsList = quizAttempt.QuizQuestionStats.Where(q => q.Status == (int)StatusEnum.Active && q.CompletedOn.HasValue);

                            foreach (var quesStatsObj in quesStatsList.Where(r => r.QuestionsInQuiz.Type == (int)BranchingLogicEnum.QUESTION))
                            {
                                var questionInQuiz = quesStatsObj.QuestionsInQuiz;

                                var question = new LeadReport.Question();

                                question.QuestionId = quesStatsObj.QuestionId;
                                question.QuestionTitle = HttpUtility.HtmlDecode(Regex.Replace(QuizService.VariableLinking(questionInQuiz.Question, quizDetails, quizAttempt, false, ShowScoreValue, scoreValueTxt), "<.*?>", string.Empty));
                                question.QuestionType = questionInQuiz.AnswerType;
                                question.QuestionImage = questionInQuiz.QuestionImage;

                                LeadReport.Question.OptionTextforRatingTypeQuestion optionTextforRatingTypeQuestions = null;

                                if (questionInQuiz.AnswerType == (int)AnswerTypeEnum.RatingEmoji || questionInQuiz.AnswerType == (int)AnswerTypeEnum.RatingStarts)
                                {
                                    optionTextforRatingTypeQuestions = new LeadReport.Question.OptionTextforRatingTypeQuestion();
                                    var option = questionInQuiz.AnswerOptionsInQuizQuestions.FirstOrDefault(r => r.Status == (int)StatusEnum.Active);
                                    optionTextforRatingTypeQuestions.OptionTextforRatingOne = option.OptionTextforRatingOne;
                                    optionTextforRatingTypeQuestions.OptionTextforRatingTwo = option.OptionTextforRatingTwo;
                                    optionTextforRatingTypeQuestions.OptionTextforRatingThree = option.OptionTextforRatingThree;
                                    optionTextforRatingTypeQuestions.OptionTextforRatingFour = option.OptionTextforRatingFour;
                                    optionTextforRatingTypeQuestions.OptionTextforRatingFive = option.OptionTextforRatingFive;
                                }

                                question.OptionTextforRatingTypeQuestions = optionTextforRatingTypeQuestions;

                                question.Answers = new List<LeadReport.Question.Answer>();
                                question.Comments = new List<LeadReport.Question.CommentDetails>();

                                if (questionInQuiz.AnswerType == (int)AnswerTypeEnum.FullAddress)
                                {
                                    var quizAnswerStatsObj = quesStatsObj.QuizAnswerStats.FirstOrDefault();
                                    var ans = quizAnswerStatsObj.AnswerOptionsInQuizQuestions;

                                    var answerImage = string.Empty;
                                    var publicIdForAnswer = string.Empty;

                                    if (questionInQuiz.ShowAnswerImage.HasValue && questionInQuiz.ShowAnswerImage.Value && ans.EnableMediaFile && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttempt.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.ObjectId == ans.Id))
                                    {
                                        var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttempt.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.ObjectId == ans.Id);
                                        answerImage = mediaObj.ObjectValue;
                                        publicIdForAnswer = mediaObj.ObjectPublicId;
                                    }
                                    else
                                    {
                                        answerImage = questionInQuiz.ShowAnswerImage.HasValue && questionInQuiz.ShowAnswerImage.Value ? ans.OptionImage : string.Empty;
                                        publicIdForAnswer = questionInQuiz.ShowAnswerImage.HasValue && questionInQuiz.ShowAnswerImage.Value ? ans.PublicId : string.Empty;
                                    }

                                    question.Answers.Add(new LeadReport.Question.Answer()
                                    {
                                        AnswerId = quizAnswerStatsObj.AnswerId,
                                        AnswerText = string.Join(", ", quesStatsObj.QuizAnswerStats.Select(r => r.AnswerText)),
                                        OptionImage = answerImage,
                                        PublicId = publicIdForAnswer
                                    });
                                }
                                else
                                {
                                    foreach (var quizAnswerStatsObj in quesStatsObj.QuizAnswerStats)
                                    {
                                        var ans = quizAnswerStatsObj.AnswerOptionsInQuizQuestions;

                                        var answerImage = string.Empty;
                                        var publicIdForAnswer = string.Empty;

                                        if (questionInQuiz.ShowAnswerImage.HasValue && questionInQuiz.ShowAnswerImage.Value && ans.EnableMediaFile && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttempt.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.ObjectId == ans.Id))
                                        {
                                            var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttempt.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.ObjectId == ans.Id);
                                            answerImage = mediaObj.ObjectValue;
                                            publicIdForAnswer = mediaObj.ObjectPublicId;
                                        }
                                        else
                                        {
                                            answerImage = questionInQuiz.ShowAnswerImage.HasValue && questionInQuiz.ShowAnswerImage.Value ? ans.OptionImage : string.Empty;
                                            publicIdForAnswer = questionInQuiz.ShowAnswerImage.HasValue && questionInQuiz.ShowAnswerImage.Value ? ans.PublicId : string.Empty;
                                        }

                                        question.Answers.Add(new LeadReport.Question.Answer()
                                        {
                                            AnswerId = quizAnswerStatsObj.AnswerId,
                                            AnswerText = (questionInQuiz.AnswerType == (int)AnswerTypeEnum.Short || questionInQuiz.AnswerType == (int)AnswerTypeEnum.Long || questionInQuiz.AnswerType == (int)AnswerTypeEnum.DOB || questionInQuiz.AnswerType == (int)AnswerTypeEnum.PostCode || questionInQuiz.AnswerType == (int)AnswerTypeEnum.NPS || questionInQuiz.AnswerType == (int)AnswerTypeEnum.RatingEmoji || questionInQuiz.AnswerType == (int)AnswerTypeEnum.RatingStarts || questionInQuiz.AnswerType == (int)AnswerTypeEnum.Availability || questionInQuiz.AnswerType == (int)AnswerTypeEnum.DatePicker)
                                                                ? quizAnswerStatsObj.AnswerText : HttpUtility.HtmlDecode(Regex.Replace(QuizService.VariableLinking(quizAnswerStatsObj.AnswerOptionsInQuizQuestions.Option, quizDetails, quizAttempt, false, ShowScoreValue, scoreValueTxt), "<.*?>", string.Empty)),
                                            OptionImage = answerImage,
                                            PublicId = publicIdForAnswer
                                        });

                                        if (!string.IsNullOrEmpty(quizAnswerStatsObj.Comment))
                                        {
                                            if (questionInQuiz.AnswerType == (int)AnswerTypeEnum.RatingEmoji || questionInQuiz.AnswerType == (int)AnswerTypeEnum.RatingStarts || questionInQuiz.AnswerType == (int)AnswerTypeEnum.Availability)
                                            {
                                                question.Comments.Add(new LeadReport.Question.CommentDetails()
                                                {
                                                    AnswerText = quizAnswerStatsObj.AnswerText,
                                                    Comment = quizAnswerStatsObj.Comment,
                                                    CompletedOn = quesStatsObj.CompletedOn.Value
                                                });
                                            }
                                        }
                                    }
                                }

                                leadReport.Questions.Add(question);
                            }

                            #endregion

                            leadReportDetails.leadReports.Add(leadReport);
                        }

                        if (leadReportDetails.leadReports != null && leadReportDetails.leadReports.Any())
                            leadReportDetails.leadReports = leadReportDetails.leadReports.OrderByDescending(t => t.SentDate).ToList();
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

            return leadReportDetails;
        }

        public NPSReport GetNPSAutomationReport(int QuizId, string SourceId, int ChartView , DateTime? FromDate, DateTime? ToDate, int? ResultId, int CompanyId)
        {
            NPSReport quizReport = null;

            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    ToDate = ToDate.HasValue ? ToDate.Value : DateTime.UtcNow.Date;
                    FromDate = FromDate.HasValue ? FromDate.Value : ToDate.Value.AddMonths(-1);

                    var quizObj = UOWObj.QuizRepository.GetByID(QuizId);

                    if (quizObj != null && quizObj.QuizType == (int)QuizTypeEnum.NPS && quizObj.CompanyId == CompanyId)
                    {
                         Db.QuizDetails quizDetailsObj = new Db.QuizDetails();                       
                         quizDetailsObj = quizObj.QuizDetails.FirstOrDefault(r => r.Status == (int)StatusEnum.Active && r.State == (int)QuizStateEnum.DRAFTED);

                        if (quizDetailsObj != null)
                        {
                            quizReport = new NPSReport();

                            quizReport.Stages = new List<NPSReport.Stage>();
                            quizReport.Results = new List<NPSReport.Result>();
                            quizReport.Questions = new List<NPSReport.Question>();
                            quizReport.NPSScoreDetails = new List<NPSReport.NPSScoreDetail>();
                            quizReport.TopRecordsDetails = new List<NPSReport.TopRecordsDetail>();

                            quizReport.QuizId = QuizId;
                            quizReport.QuizTitle = quizDetailsObj.QuizTitle;
                            quizReport.QuizType = quizObj.QuizType;

                            IEnumerable<Db.QuizAttempts> quizAttempts = new List<Db.QuizAttempts>();

                                quizAttempts = UOWObj.QuizAttemptsRepository.Get(filter: r => r.Mode == "AUDIT"
                                               && r.QuizDetails.ParentQuizId == QuizId
                                               && (string.IsNullOrEmpty(SourceId) ? true : ((r.ConfigurationDetails != null && r.ConfigurationDetails.SourceId == SourceId)
                                                  || (r.WorkPackageInfoId != null && r.WorkPackageInfo.CampaignId == SourceId)))
                                               && r.RecruiterUserId == null && r.CompanyId == CompanyId
                            , includeProperties: "QuizStats, QuizQuestionStats");

                            var workpackageInfoObj = UOWObj.WorkPackageInfoRepository.Get(r => r.QuizId == QuizId);

                            var sentReportAttrObj = workpackageInfoObj.Count(r => (r.CreatedOn.HasValue ? (r.CreatedOn.Value.Date >= FromDate && r.CreatedOn.Value.Date <= ToDate) : false) && (string.IsNullOrEmpty(SourceId) ? true : (r.CampaignId == SourceId)))
                                + quizAttempts.Count(r => r.WorkPackageInfoId == null && r.CreatedOn.Date >= FromDate && r.CreatedOn.Date <= ToDate);

                            quizReport.Stages.Add(new NPSReport.Stage()
                            {
                                Id = (int)QuizReportingAttributeEnum.Sent,
                                Value = sentReportAttrObj
                            });

                            if (quizAttempts.Any())
                            {                 
                                var questionList = quizDetailsObj.QuestionsInQuiz;

                                var resultList = quizDetailsObj.QuizResults;

                                var quizComponentLogsList = UOWObj.QuizComponentLogsRepository.Get(r => r.QuizDetails.ParentQuizId == QuizId);

                                var quizAttemptsObj = quizAttempts.Where(r => (r.WorkPackageInfoId != null && r.WorkPackageInfo.CreatedOn.Value.Date >= FromDate && r.WorkPackageInfo.CreatedOn.Value.Date <= ToDate)
                                                                    || (r.WorkPackageInfoId == null && r.CreatedOn.Date >= FromDate && r.CreatedOn.Date <= ToDate));

                                #region data for stages

                                var viewsReportAttrObj = quizAttemptsObj.Count(r => r.IsViewed);
                                var startsReportAttrObj = quizAttemptsObj.Count(r => r.QuizStats.Any());
                                var resultsReportAttrObj = quizAttemptsObj.Count(r => r.QuizStats.Any() && (r.QuizStats.FirstOrDefault().ResultId != null || r.QuizStats.FirstOrDefault().CompletedOn.HasValue));
                                var leadsReportAttrObj = quizAttemptsObj.Count(r => r.LeadConvertedOn.HasValue && !string.IsNullOrEmpty(r.LeadUserId));

                                quizReport.Stages.Add(new NPSReport.Stage()
                                {
                                    Id = (int)QuizReportingAttributeEnum.Views,
                                    Value = viewsReportAttrObj
                                });

                                quizReport.Stages.Add(new NPSReport.Stage()
                                {
                                    Id = (int)QuizReportingAttributeEnum.Starts,
                                    Value = startsReportAttrObj
                                });

                                quizReport.Stages.Add(new NPSReport.Stage()
                                {
                                    Id = (int)QuizReportingAttributeEnum.Result,
                                    Value = resultsReportAttrObj
                                });

                                quizReport.Stages.Add(new NPSReport.Stage()
                                {
                                    Id = (int)QuizReportingAttributeEnum.Leads,
                                    Value = leadsReportAttrObj
                                });

                                #endregion

                                #region data for results

                                //Total Leads Completed Quiz Or Reached to result
                                var completedOrAchievedResultQuizObj = quizAttemptsObj.Select(r => r.QuizStats.Where(q => q.ResultId.HasValue || q.CompletedOn.HasValue));
                                var completedOrAchievedResultQuizObjCount = completedOrAchievedResultQuizObj.Count(r => r.Any());
                                var parentIdList = new List<int>();

                                foreach (var attempt in completedOrAchievedResultQuizObj)
                                {
                                    foreach (var item in attempt.Where(r => r.ResultId.HasValue))
                                    {
                                        var parentResultObj = quizComponentLogsList.FirstOrDefault(r => r.ObjectTypeId == (int)BranchingLogicEnum.RESULT && r.PublishedObjectId == item.ResultId);
                                        if (parentResultObj != null)
                                            parentIdList.Add(parentResultObj.DraftedObjectId);
                                    }
                                }

                                foreach (var resultId in parentIdList.Distinct())
                                {
                                    var result = resultList.FirstOrDefault(r => r.Id == resultId);
                                    quizReport.Results.Add(new NPSReport.Result()
                                    {
                                        ParentResultId = resultId,
                                        ExternalResultTitle = result.Title,
                                        InternalResultTitle = result.InternalTitle,
                                        Value = parentIdList.Count(r => r == resultId),
                                        MinScore = result.MinScore.Value,
                                        MaxScore = result.MaxScore.Value
                                    });
                                }

                                #endregion

                                #region Questions

                                var quesStatsList = quizAttemptsObj.Where(r => ResultId.HasValue
                                        ? (ResultId == 0 ? r.QuizStats.Any(q => q.CompletedOn.HasValue && !q.ResultId.HasValue)
                                                    : r.QuizStats.Any(q => q.ResultId.HasValue && quizComponentLogsList.FirstOrDefault(c => c.ObjectTypeId == (int)BranchingLogicEnum.RESULT && c.PublishedObjectId == q.ResultId).DraftedObjectId == ResultId.Value))
                                        : true)
                                         .Select(r => r.QuizQuestionStats.Where(q => q.Status == (int)StatusEnum.Active && q.CompletedOn.HasValue));

                                var quesParentIdList = new List<int>();

                                foreach (var quesStatsObj in quesStatsList)
                                {
                                    foreach (var item in quesStatsObj.Where(r => r.QuestionsInQuiz.Type == (int)BranchingLogicEnum.QUESTION))
                                    {
                                        var parentQuestionObject = quizComponentLogsList.FirstOrDefault(r => r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION && r.PublishedObjectId == item.QuestionId);
                                        if (parentQuestionObject != null)
                                        {
                                            var parentQuesId = parentQuestionObject.DraftedObjectId;
                                            var ques = questionList.FirstOrDefault(r => r.Id == parentQuesId);
                                            var answerList = ques.AnswerOptionsInQuizQuestions;

                                            var question = quizReport.Questions.FirstOrDefault(r => r.ParentQuestionId == parentQuesId);

                                            if (question != null)
                                            {
                                                question.LeadCountForQuestion = question.LeadCountForQuestion + 1;

                                                var ansLst = new List<Question.Answer>();

                                                if (ques.AnswerType == (int)AnswerTypeEnum.Short || ques.AnswerType == (int)AnswerTypeEnum.Long || ques.AnswerType == (int)AnswerTypeEnum.DOB || ques.AnswerType == (int)AnswerTypeEnum.PostCode)
                                                {
                                                    foreach (var ans in item.QuizAnswerStats)
                                                    {
                                                        var parentAnswerObj = quizComponentLogsList.FirstOrDefault(r => r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.PublishedObjectId == ans.AnswerId);
                                                        if (parentAnswerObj != null)
                                                        {
                                                            question.Answers.Add(new NPSReport.Question.Answer()
                                                            {
                                                                ParentAnswerid = parentAnswerObj.DraftedObjectId,
                                                                AnswerText = ans.AnswerText,
                                                                LeadCount = 1,
                                                                LeadId = item.QuizAttempts.LeadUserId,
                                                                CompletedOn = item.CompletedOn.Value
                                                            });
                                                        }
                                                    }
                                                }
                                                else if (ques.AnswerType == (int)AnswerTypeEnum.FullAddress)
                                                {
                                                    var ans = item.QuizAnswerStats.FirstOrDefault();
                                                    var parentAnswerObj = quizComponentLogsList.FirstOrDefault(r => r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.PublishedObjectId == ans.AnswerId);
                                                    if (parentAnswerObj != null)
                                                    {
                                                        question.Answers.Add(new NPSReport.Question.Answer()
                                                        {
                                                            ParentAnswerid = parentAnswerObj.DraftedObjectId,
                                                            AnswerText = string.Join(", ", item.QuizAnswerStats.Select(r => r.AnswerText)),
                                                            LeadCount = 1,
                                                            LeadId = item.QuizAttempts.LeadUserId,
                                                            CompletedOn = item.CompletedOn.Value
                                                        });
                                                    }
                                                }
                                                else if (ques.AnswerType == (int)AnswerTypeEnum.RatingEmoji || ques.AnswerType == (int)AnswerTypeEnum.RatingStarts || ques.AnswerType == (int)AnswerTypeEnum.NPS || ques.AnswerType == (int)AnswerTypeEnum.Availability)
                                                {
                                                    foreach (var ans in item.QuizAnswerStats)
                                                    {
                                                        var parentAnswerObj = quizComponentLogsList.FirstOrDefault(r => r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.PublishedObjectId == ans.AnswerId);
                                                        if (parentAnswerObj != null)
                                                        {
                                                            var parentAnswerId = parentAnswerObj.DraftedObjectId;

                                                            var answer = question.Answers.FirstOrDefault(r => r.ParentAnswerid == parentAnswerId && r.AnswerText == ans.AnswerText);
                                                            if (answer != null)
                                                                answer.LeadCount = answer.LeadCount + 1;
                                                            else
                                                            {
                                                                question.Answers.Add(new NPSReport.Question.Answer()
                                                                {
                                                                    ParentAnswerid = parentAnswerId,
                                                                    AnswerText = ans.AnswerText,
                                                                    LeadCount = 1,
                                                                    LeadId = string.Empty
                                                                });
                                                            }

                                                            if ((ques.AnswerType == (int)AnswerTypeEnum.RatingEmoji || ques.AnswerType == (int)AnswerTypeEnum.RatingStarts || ques.AnswerType == (int)AnswerTypeEnum.Availability || ques.AnswerType == (int)AnswerTypeEnum.NPS) && !string.IsNullOrEmpty(ans.Comment))
                                                            {
                                                                question.Comments.Add(new NPSReport.Question.CommentDetails()
                                                                {
                                                                    AnswerText = ans.AnswerText,
                                                                    LeadId = item.QuizAttempts.LeadUserId,
                                                                    Comment = ans.Comment,
                                                                    CompletedOn = item.CompletedOn.Value
                                                                });
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    foreach (var ans in item.QuizAnswerStats)
                                                    {
                                                        var parentAnswerObj = quizComponentLogsList.FirstOrDefault(r => r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.PublishedObjectId == ans.AnswerId);
                                                        if (parentAnswerObj != null)
                                                        {
                                                            var parentAnswerId = parentAnswerObj.DraftedObjectId;
                                                            var answer = question.Answers.FirstOrDefault(r => r.ParentAnswerid == parentAnswerId);

                                                            if (answer != null)
                                                                answer.LeadCount = answer.LeadCount + 1;
                                                            else
                                                            {
                                                                question.Answers.Add(new NPSReport.Question.Answer()
                                                                {
                                                                    ParentAnswerid = parentAnswerId,
                                                                    AnswerText = answerList.FirstOrDefault(r => r.Id == parentAnswerId).Option,
                                                                    LeadCount = 1,
                                                                    LeadId = string.Empty
                                                                });
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                var ansLst = new List<NPSReport.Question.Answer>();
                                                var commentLst = new List<NPSReport.Question.CommentDetails>();
                                                if (ques.AnswerType == (int)AnswerTypeEnum.FullAddress)
                                                {
                                                    var ans = item.QuizAnswerStats.FirstOrDefault();
                                                    var parentAnswerObj = quizComponentLogsList.FirstOrDefault(r => r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.PublishedObjectId == ans.AnswerId);
                                                    if (parentAnswerObj != null)
                                                    {
                                                        ansLst.Add(new NPSReport.Question.Answer()
                                                        {
                                                            ParentAnswerid = parentAnswerObj.DraftedObjectId,
                                                            AnswerText = string.Join(", ", item.QuizAnswerStats.Select(r => r.AnswerText)),
                                                            LeadCount = 1,
                                                            LeadId = item.QuizAttempts.LeadUserId,
                                                            CompletedOn = item.CompletedOn.Value,
                                                        });
                                                    }
                                                }
                                                else
                                                {
                                                    foreach (var ans in item.QuizAnswerStats)
                                                    {
                                                        var parentAnswerObj = quizComponentLogsList.FirstOrDefault(r => r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.PublishedObjectId == ans.AnswerId);
                                                        if (parentAnswerObj != null)
                                                        {
                                                            var parentAnswerId = parentAnswerObj.DraftedObjectId;

                                                            ansLst.Add(new NPSReport.Question.Answer()
                                                            {
                                                                ParentAnswerid = parentAnswerId,
                                                                AnswerText = (ques.AnswerType == (int)AnswerTypeEnum.Short || ques.AnswerType == (int)AnswerTypeEnum.Long || ques.AnswerType == (int)AnswerTypeEnum.DOB || ques.AnswerType == (int)AnswerTypeEnum.PostCode || ques.AnswerType == (int)AnswerTypeEnum.NPS || ques.AnswerType == (int)AnswerTypeEnum.RatingEmoji || ques.AnswerType == (int)AnswerTypeEnum.Availability)
                                                                ? ans.AnswerText : answerList.FirstOrDefault(r => r.Id == parentAnswerId).Option,
                                                                LeadCount = 1,
                                                                LeadId = (ques.AnswerType == (int)AnswerTypeEnum.Short || ques.AnswerType == (int)AnswerTypeEnum.Long || ques.AnswerType == (int)AnswerTypeEnum.DOB || ques.AnswerType == (int)AnswerTypeEnum.PostCode || ques.AnswerType == (int)AnswerTypeEnum.Availability)
                                                                ? item.QuizAttempts.LeadUserId : string.Empty,
                                                                CompletedOn = (ques.AnswerType == (int)AnswerTypeEnum.Short || ques.AnswerType == (int)AnswerTypeEnum.Long || ques.AnswerType == (int)AnswerTypeEnum.DOB || ques.AnswerType == (int)AnswerTypeEnum.PostCode || ques.AnswerType == (int)AnswerTypeEnum.Availability)
                                                                                ? item.CompletedOn.Value : default(DateTime?)
                                                            });

                                                            if ((ques.AnswerType == (int)AnswerTypeEnum.RatingEmoji || ques.AnswerType == (int)AnswerTypeEnum.RatingStarts || ques.AnswerType == (int)AnswerTypeEnum.Availability || ques.AnswerType == (int)AnswerTypeEnum.NPS) && !string.IsNullOrEmpty(ans.Comment))
                                                            {
                                                                commentLst.Add(new NPSReport.Question.CommentDetails()
                                                                {
                                                                    AnswerText = ans.AnswerText,
                                                                    LeadId = item.QuizAttempts.LeadUserId,
                                                                    Comment = ans.Comment,
                                                                    CompletedOn = item.CompletedOn.Value
                                                                });
                                                            }
                                                        }
                                                    }
                                                }

                                                NPSReport.Question.OptionTextforRatingTypeQuestion optionTextforRatingTypeQuestions = null;

                                                if (ques.AnswerType == (int)AnswerTypeEnum.RatingEmoji || ques.AnswerType == (int)AnswerTypeEnum.RatingStarts)
                                                {
                                                    optionTextforRatingTypeQuestions = new NPSReport.Question.OptionTextforRatingTypeQuestion();
                                                    var option = answerList.FirstOrDefault(r => r.Status == (int)StatusEnum.Active);
                                                    if(option != null) 
                                                    {
                                                        optionTextforRatingTypeQuestions.OptionTextforRatingOne = option.OptionTextforRatingOne;
                                                        optionTextforRatingTypeQuestions.OptionTextforRatingTwo = option.OptionTextforRatingTwo;
                                                        optionTextforRatingTypeQuestions.OptionTextforRatingThree = option.OptionTextforRatingThree;
                                                        optionTextforRatingTypeQuestions.OptionTextforRatingFour = option.OptionTextforRatingFour;
                                                        optionTextforRatingTypeQuestions.OptionTextforRatingFive = option.OptionTextforRatingFive;
                                                    }
                                                }

                                                quizReport.Questions.Add(new NPSReport.Question()
                                                {
                                                    ParentQuestionId = parentQuesId,
                                                    QuestionTitle = ques.Question,
                                                    QuestionType = ques.AnswerType,
                                                    Answers = ansLst,
                                                    LeadCountForQuestion = 1,
                                                    Comments = commentLst,
                                                    OptionTextforRatingTypeQuestions = optionTextforRatingTypeQuestions
                                                });
                                            }
                                        }
                                    }
                                }


                                #endregion

                                #region NPSScoreDetails for each day

                                if (ChartView == (int)ChartViewTypeEnum.Day)
                                {
                                    DateTime? startDate = FromDate;
                                    while (startDate <= ToDate)
                                    {
                                        float detractorResultCount = quizAttemptsObj.Count(r => r.QuizStats.Any(q => q.ResultId.HasValue && q.QuizResults.MinScore >= 0 && q.QuizResults.MaxScore <= 6 && q.CompletedOn.HasValue && q.CompletedOn.Value.Date == startDate.Value.Date));
                                        float passiveResultCount = quizAttemptsObj.Count(r => r.QuizStats.Any(q => q.ResultId.HasValue && q.QuizResults.MinScore >= 7 && q.QuizResults.MaxScore <= 8 && q.CompletedOn.HasValue && q.CompletedOn.Value.Date == startDate.Value.Date));
                                        float promoterResultCount = quizAttemptsObj.Count(r => r.QuizStats.Any(q => q.ResultId.HasValue && q.QuizResults.MinScore >= 9 && q.QuizResults.MaxScore <= 10 && q.CompletedOn.HasValue && q.CompletedOn.Value.Date == startDate.Value.Date));

                                        var total = detractorResultCount + passiveResultCount + promoterResultCount;

                                        quizReport.NPSScoreDetails.Add(new NPSReport.NPSScoreDetail()
                                        {
                                            Date = startDate.Value,
                                            Day = startDate.Value.DayOfWeek,
                                            DayNumber = startDate.Value.Day,
                                            Year = startDate.Value.Year,
                                            NPSScore = total > 0 ? (promoterResultCount / total * 100) - (detractorResultCount / total * 100) : 0
                                        });

                                        startDate = startDate.Value.AddDays(1);
                                    }
                                }
                                
                                if (ChartView == (int)ChartViewTypeEnum.Week)
                                {
                                    DateTime? startDate = FromDate;
                                    while (startDate <= ToDate)
                                    {
                                        int weekCount = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                                                    startDate.Value,
                                                    CultureInfo.CurrentCulture.DateTimeFormat.CalendarWeekRule,
                                                    CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek);

                                        var endDate = startDate.Value.AddDays(-(int)startDate.Value.DayOfWeek).AddDays(7).AddSeconds(-1);

                                        if (endDate > ToDate)
                                            endDate = ToDate.Value;
                                        if (startDate.Value.Year != endDate.Year)
                                            endDate = new DateTime(startDate.Value.Year, startDate.Value.Month , DateTime.DaysInMonth(startDate.Value.Year, startDate.Value.Month));
                                       

                                        float detractorResultCount = quizAttemptsObj.Count(r => r.QuizStats.Any(q => q.ResultId.HasValue && q.QuizResults.MinScore >= 0 && q.QuizResults.MaxScore <= 6 && q.CompletedOn.HasValue && q.CompletedOn.Value.Date >= startDate.Value.Date && q.CompletedOn.Value.Date <= endDate));
                                        float passiveResultCount = quizAttemptsObj.Count(r => r.QuizStats.Any(q => q.ResultId.HasValue && q.QuizResults.MinScore >= 7 && q.QuizResults.MaxScore <= 8 && q.CompletedOn.HasValue && q.CompletedOn.Value.Date >= startDate.Value.Date && q.CompletedOn.Value.Date <= endDate));
                                        float promoterResultCount = quizAttemptsObj.Count(r => r.QuizStats.Any(q => q.ResultId.HasValue && q.QuizResults.MinScore >= 9 && q.QuizResults.MaxScore <= 10 && q.CompletedOn.HasValue && q.CompletedOn.Value.Date >= startDate.Value.Date && q.CompletedOn.Value.Date <= endDate));

                                        var total = detractorResultCount + passiveResultCount + promoterResultCount;

                                        quizReport.NPSScoreDetails.Add(new NPSReport.NPSScoreDetail()
                                        {
                                            Week = weekCount,
                                            Year = startDate.Value.Year,
                                            NPSScore = total > 0 ? (promoterResultCount / total * 100) - (detractorResultCount / total * 100) : 0
                                        });

                                        startDate = endDate.AddDays(1);
                                    }
                                }

                                if (ChartView == (int)ChartViewTypeEnum.Month)
                                {
                                    DateTime? startDate = FromDate;
                                    while (startDate.Value.Month <= ToDate.Value.Month)
                                    {
                                        float detractorResultCount = quizAttemptsObj.Count(r => r.QuizStats.Any(q => q.ResultId.HasValue && q.QuizResults.MinScore >= 0 && q.QuizResults.MaxScore <= 6 && q.CompletedOn.HasValue && q.CompletedOn.Value.Month == startDate.Value.Month));
                                        float passiveResultCount = quizAttemptsObj.Count(r => r.QuizStats.Any(q => q.ResultId.HasValue && q.QuizResults.MinScore >= 7 && q.QuizResults.MaxScore <= 8 && q.CompletedOn.HasValue && q.CompletedOn.Value.Month == startDate.Value.Month));
                                        float promoterResultCount = quizAttemptsObj.Count(r => r.QuizStats.Any(q => q.ResultId.HasValue && q.QuizResults.MinScore >= 9 && q.QuizResults.MaxScore <= 10 && q.CompletedOn.HasValue && q.CompletedOn.Value.Month == startDate.Value.Month));

                                        var total = detractorResultCount + passiveResultCount + promoterResultCount;

                                        
                                        quizReport.NPSScoreDetails.Add(new NPSReport.NPSScoreDetail()
                                        {
                                            MonthNumber = startDate.Value.Month,
                                            MonthName = startDate.Value.ToString("MMMM"),
                                            Year = startDate.Value.Year,
                                            NPSScore = (total > 0) ? (promoterResultCount / total * 100) - (detractorResultCount / total * 100) : 0
                                        });

                                        startDate = startDate.Value.AddMonths(1);
                                    }
                                }

                                if (ChartView == (int)ChartViewTypeEnum.Year)
                                {
                                    DateTime? startDate = FromDate;
                                    while (startDate <= ToDate)
                                    {
                                        float detractorResultCount = quizAttemptsObj.Count(r => r.QuizStats.Any(q => q.ResultId.HasValue && q.QuizResults.MinScore >= 0 && q.QuizResults.MaxScore <= 6 && q.CompletedOn.HasValue && q.CompletedOn.Value.Year == startDate.Value.Year));
                                        float passiveResultCount = quizAttemptsObj.Count(r => r.QuizStats.Any(q => q.ResultId.HasValue && q.QuizResults.MinScore >= 7 && q.QuizResults.MaxScore <= 8 && q.CompletedOn.HasValue && q.CompletedOn.Value.Year == startDate.Value.Year));
                                        float promoterResultCount = quizAttemptsObj.Count(r => r.QuizStats.Any(q => q.ResultId.HasValue && q.QuizResults.MinScore >= 9 && q.QuizResults.MaxScore <= 10 && q.CompletedOn.HasValue && q.CompletedOn.Value.Year == startDate.Value.Year));

                                        var total = detractorResultCount + passiveResultCount + promoterResultCount;

                                        quizReport.NPSScoreDetails.Add(new NPSReport.NPSScoreDetail()
                                        {
                                            Year = startDate.Value.Year,
                                            NPSScore = total > 0 ? (promoterResultCount / total * 100) - (detractorResultCount / total * 100) : 0
                                        });

                                        startDate = startDate.Value.AddYears(1);
                                    }
                                }

                                #endregion

                                #region NPSScore

                                if (quizReport.Results.Any())
                                {
                                    float score = 0;

                                    float detractorResultCount = quizReport.Results.Any(r =>r.MinScore >= 0 && r.MaxScore <= 6) ? quizReport.Results.FirstOrDefault(r => r.MinScore >= 0 && r.MaxScore <= 6).Value : 0;
                                    float passiveResultCount = quizReport.Results.Any(r => r.MinScore >= 7 && r.MaxScore <= 8) ? quizReport.Results.FirstOrDefault(r => r.MinScore >= 7 && r.MaxScore <= 8).Value : 0;
                                    float promoterResultCount = quizReport.Results.Any(r => r.MinScore >= 9 && r.MaxScore <= 10) ? quizReport.Results.FirstOrDefault(r => r.MinScore >= 9 && r.MaxScore <= 10).Value : 0;

                                    var total = detractorResultCount + passiveResultCount + promoterResultCount;
                                    if (total > 0)
                                        score = ((promoterResultCount / total) * 100) - ((detractorResultCount / total) * 100);
                                    quizReport.NPSScore = score;
                                    quizReport.DetractorResultCount = detractorResultCount;
                                    quizReport.PassiveResultCount = passiveResultCount;
                                    quizReport.PromoterResultCount = promoterResultCount;
                                }

                                #endregion

                                #region TOP3

                                IEnumerable<Db.QuizQuestionStats> quizQuestionStatsList = new List<Db.QuizQuestionStats>();


                                quizQuestionStatsList = UOWObj.QuizQuestionStatsRepository.Get(r => r.Status == (int)StatusEnum.Active && r.CompletedOn.HasValue
                                && (r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.RatingEmoji || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.RatingStarts))
                                    .Where(r => quizAttemptsObj.Any(q => q.Id == r.QuizAttemptId));

                                foreach (var parentId in parentIdList.Distinct())
                                {
                                    var quizQuestionStats = quizQuestionStatsList.Where(r => r.QuizAttempts.QuizStats.Any(q => q.ResultId.HasValue
                                    && quizComponentLogsList.FirstOrDefault(c => c.ObjectTypeId == (int)BranchingLogicEnum.RESULT && c.PublishedObjectId == q.ResultId).DraftedObjectId == parentId));

                                    if (quizQuestionStats.Any()) {

                                        var result = resultList.FirstOrDefault(r => r.Id == parentId);

                                        var topRecordsDetailObject = new NPSReport.TopRecordsDetail();
                                        topRecordsDetailObject.ParentResultId = parentId;
                                        topRecordsDetailObject.InternalResultTitle = result.InternalTitle;
                                        topRecordsDetailObject.ExternalResultTitle = result.Title;
                                        topRecordsDetailObject.NumberofLead = quizAttemptsObj.Count(r => r.QuizStats.Any(q => q.ResultId.HasValue && quizComponentLogsList.FirstOrDefault(c => c.ObjectTypeId == (int)BranchingLogicEnum.RESULT && c.PublishedObjectId == q.ResultId.Value).DraftedObjectId == parentId));
                                        topRecordsDetailObject.PositiveThings = new List<NPSReport.TopRecordsDetail.Thing>();
                                        topRecordsDetailObject.NegativeThings = new List<NPSReport.TopRecordsDetail.Thing>();
                                        foreach (var item in quizQuestionStats.Where(r => r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.RatingEmoji || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.RatingStarts).OrderByDescending(r => Convert.ToInt32(r.QuizAnswerStats.FirstOrDefault().AnswerText)).Take(3))
                                        {
                                            topRecordsDetailObject.PositiveThings.Add(new NPSReport.TopRecordsDetail.Thing()
                                            {
                                                Rating = Convert.ToInt32(item.QuizAnswerStats.FirstOrDefault().AnswerText),
                                                TopicTitle = item.QuestionsInQuiz.TopicTitle
                                            });
                                        }

                                        foreach (var item in quizQuestionStats.Where(r => r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.RatingEmoji || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.RatingStarts).OrderBy(r => Convert.ToInt32(r.QuizAnswerStats.FirstOrDefault().AnswerText)).Take(3))
                                        {
                                            topRecordsDetailObject.NegativeThings.Add(new NPSReport.TopRecordsDetail.Thing()
                                            {
                                                Rating = Convert.ToInt32(item.QuizAnswerStats.FirstOrDefault().AnswerText),
                                                TopicTitle = item.QuestionsInQuiz.TopicTitle
                                            });
                                        }

                                        quizReport.TopRecordsDetails.Add(topRecordsDetailObject);
                                    }
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
            return quizReport;
        }

        public Report GetQuizTemplateReport(string TemplateId, DateTime? FromDate, DateTime? ToDate, int? ResultId, CompanyModel CompanyObj)
        {
            Report quizReport = null;

            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    ToDate = ToDate.HasValue ? ToDate.Value : DateTime.UtcNow.Date;
                    FromDate = FromDate.HasValue ? FromDate.Value : ToDate.Value.AddMonths(-1);

                    var templateDetails = OWCHelper.GetTemplateLinkedAutomation(CompanyObj.ClientCode, TemplateId);

                    if (templateDetails != null && templateDetails.QuizId.GetValueOrDefault() > 0)
                    {
                        int QuizId = templateDetails.QuizId.Value;
                        var quizObj = UOWObj.QuizRepository.GetByID(QuizId);

                        if (quizObj != null && quizObj.CompanyId == CompanyObj.Id)
                        {
                            Db.QuizDetails quizDetailsObj = new Db.QuizDetails();
                      
                            quizDetailsObj = quizObj.QuizDetails.FirstOrDefault(r => r.Status == (int)StatusEnum.Active && r.State == (int)QuizStateEnum.DRAFTED);
                            
                            if (quizDetailsObj != null)
                            {
                                quizReport = new Report();

                                quizReport.Stages = new List<Stage>();
                                quizReport.Results = new List<Report.Result>();
                                quizReport.Questions = new List<Question>();
                                quizReport.TopRecordsDetails = new List<TopRecordsDetail>();

                                quizReport.QuizId = QuizId;
                                quizReport.QuizType = quizObj.QuizType;
                                quizReport.QuizTitle = quizDetailsObj.QuizTitle;
                                quizReport.TemplateName = templateDetails.TemplateName;
                                quizReport.TemplateId = templateDetails.ConfigurationId;

                                IEnumerable<Db.QuizAttempts> quizAttempts = new List<Db.QuizAttempts>();

                                quizAttempts = UOWObj.QuizAttemptsRepository.Get(filter: r => r.Mode == "AUDIT"
                                                && r.QuizDetails.ParentQuizId == QuizId
                                                && (templateDetails.AutomationConfigurationIds.Any() && (r.ConfigurationDetails != null && templateDetails.AutomationConfigurationIds.Contains(r.ConfigurationDetails.ConfigurationId)) || (r.WorkPackageInfoId != null && r.WorkPackageInfo.ConfigurationDetails != null && templateDetails.AutomationConfigurationIds.Contains(r.WorkPackageInfo.ConfigurationDetails.ConfigurationId)))
                                                && r.RecruiterUserId == null && r.CompanyId == CompanyObj.Id
                            , includeProperties: "QuizStats, QuizQuestionStats");
                              
                                var workpackageInfoObj = UOWObj.WorkPackageInfoRepository.Get(r => r.QuizId == QuizId && (templateDetails.AutomationConfigurationIds.Any() && templateDetails.AutomationConfigurationIds.Contains(r.ConfigurationDetails.ConfigurationId)));

                                var sentReportAttrObj = workpackageInfoObj.Count(r => (r.CreatedOn.HasValue ? (r.CreatedOn.Value.Date >= FromDate && r.CreatedOn.Value.Date <= ToDate) : false))
                                    + quizAttempts.Count(r => r.WorkPackageInfoId == null && r.CreatedOn.Date >= FromDate && r.CreatedOn.Date <= ToDate);

                                quizReport.Stages.Add(new Stage()
                                {
                                    Id = (int)QuizReportingAttributeEnum.Sent,
                                    Value = sentReportAttrObj
                                });

                                if (quizAttempts.Any())
                                {
                                    var questionList = quizDetailsObj.QuestionsInQuiz;

                                    var resultList = quizDetailsObj.QuizResults;

                                    var quizComponentLogsList = UOWObj.QuizComponentLogsRepository.Get(r => r.QuizDetails.ParentQuizId == QuizId);

                                    var quizAttemptsObj = quizAttempts.Where(r => (r.WorkPackageInfoId != null && r.WorkPackageInfo.CreatedOn.Value.Date >= FromDate && r.WorkPackageInfo.CreatedOn.Value.Date <= ToDate)
                                                                        || (r.WorkPackageInfoId == null && r.CreatedOn.Date >= FromDate && r.CreatedOn.Date <= ToDate));

                                    #region data for stages

                                    var viewsReportAttrObj = quizAttemptsObj.Count(r => r.IsViewed);
                                    var startsReportAttrObj = quizAttemptsObj.Count(r => r.QuizStats.Any());
                                    var resultsReportAttrObj = quizAttemptsObj.Count(r => r.QuizStats.Any() && (r.QuizStats.FirstOrDefault().ResultId != null || r.QuizStats.FirstOrDefault().CompletedOn.HasValue));
                                    //var completedReportAttrObj = quizAttemptsObj.Count(r => r.QuizStats.Any() && r.QuizStats.FirstOrDefault().CompletedOn.HasValue);
                                    var leadsReportAttrObj = quizAttemptsObj.Count(r => r.LeadConvertedOn.HasValue && !string.IsNullOrEmpty(r.LeadUserId));

                                    quizReport.Stages.Add(new Stage()
                                    {
                                        Id = (int)QuizReportingAttributeEnum.Views,
                                        Value = viewsReportAttrObj
                                    });

                                    quizReport.Stages.Add(new Stage()
                                    {
                                        Id = (int)QuizReportingAttributeEnum.Starts,
                                        Value = startsReportAttrObj
                                    });

                                    quizReport.Stages.Add(new Stage()
                                    {
                                        Id = (int)QuizReportingAttributeEnum.Result,
                                        Value = resultsReportAttrObj
                                    });

                                    //quizReport.Stages.Add(new Stage()
                                    //{
                                    //    Id = (int)QuizReportingAttributeEnum.Completions,
                                    //    Value = completedReportAttrObj
                                    //});

                                    quizReport.Stages.Add(new Stage()
                                    {
                                        Id = (int)QuizReportingAttributeEnum.Leads,
                                        Value = leadsReportAttrObj
                                    });

                                    #endregion

                                    #region data for results

                                    //Total Leads Completed Quiz Or Reached to result
                                    var completedOrAchievedResultQuizObj = quizAttemptsObj.Select(r => r.QuizStats.Where(q => q.ResultId.HasValue || q.CompletedOn.HasValue));
                                    var completedOrAchievedResultQuizObjCount = completedOrAchievedResultQuizObj.Count(r => r.Any());
                                    var parentIdList = new List<int>();

                                    foreach (var attempt in completedOrAchievedResultQuizObj)
                                    {
                                        foreach (var item in attempt.Where(r => r.ResultId.HasValue))
                                        {
                                            var parentResultObj = quizComponentLogsList.FirstOrDefault(r => r.ObjectTypeId == (int)BranchingLogicEnum.RESULT && r.PublishedObjectId == item.ResultId);
                                            if (parentResultObj != null)
                                                parentIdList.Add(parentResultObj.DraftedObjectId);
                                        }
                                    }

                                    foreach (var resultId in parentIdList.Distinct())
                                    {
                                        var result = resultList.FirstOrDefault(r => r.Id == resultId);
                                        quizReport.Results.Add(new Report.Result()
                                        {
                                            ParentResultId = resultId,
                                            ExternalResultTitle = result.Title,
                                            InternalResultTitle = result.InternalTitle,
                                            Value = parentIdList.Count(r => r == resultId)
                                        });
                                    }

                                    //for no result
                                    quizReport.Results.Add(new Report.Result()
                                    {
                                        ParentResultId = 0,
                                        ExternalResultTitle = string.Empty,
                                        InternalResultTitle = string.Empty,
                                        Value = completedOrAchievedResultQuizObjCount - parentIdList.Count()
                                    });

                                    #endregion

                                    #region Questions

                                    var quesStatsList = quizAttemptsObj.Where(r => ResultId.HasValue
                                            ? (ResultId == 0 ? r.QuizStats.Any(q => q.CompletedOn.HasValue && !q.ResultId.HasValue)
                                                        : r.QuizStats.Any(q => q.ResultId.HasValue && quizComponentLogsList.FirstOrDefault(c => c.ObjectTypeId == (int)BranchingLogicEnum.RESULT && c.PublishedObjectId == q.ResultId).DraftedObjectId == ResultId.Value))
                                            : true)
                                             .Select(r => r.QuizQuestionStats.Where(q => q.Status == (int)StatusEnum.Active && q.CompletedOn.HasValue));

                                    var quesParentIdList = new List<int>();

                                    foreach (var quesStatsObj in quesStatsList)
                                    {
                                        foreach (var item in quesStatsObj.Where(r => r.QuestionsInQuiz.Type == (int)BranchingLogicEnum.QUESTION))
                                        {
                                            var parentQuestionObject = quizComponentLogsList.FirstOrDefault(r => r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION && r.PublishedObjectId == item.QuestionId);
                                            if (parentQuestionObject != null)
                                            {
                                                var parentQuesId = parentQuestionObject.DraftedObjectId;
                                                var ques = questionList.FirstOrDefault(r => r.Id == parentQuesId);
                                                var answerList = ques.AnswerOptionsInQuizQuestions;

                                                var question = quizReport.Questions.FirstOrDefault(r => r.ParentQuestionId == parentQuesId);

                                                if (question != null)
                                                {
                                                    question.LeadCountForQuestion = question.LeadCountForQuestion + 1;

                                                    var ansLst = new List<Question.Answer>();

                                                    if (ques.AnswerType == (int)AnswerTypeEnum.Short || ques.AnswerType == (int)AnswerTypeEnum.Long || ques.AnswerType == (int)AnswerTypeEnum.DOB || ques.AnswerType == (int)AnswerTypeEnum.PostCode)
                                                    {
                                                        foreach (var ans in item.QuizAnswerStats)
                                                        {
                                                            var parentAnswerObj = quizComponentLogsList.FirstOrDefault(r => r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.PublishedObjectId == ans.AnswerId);
                                                            if (parentAnswerObj != null)
                                                            {
                                                                question.Answers.Add(new Question.Answer()
                                                                {
                                                                    ParentAnswerid = parentAnswerObj.DraftedObjectId,
                                                                    AnswerText = ans.AnswerText,
                                                                    LeadCount = 1,
                                                                    LeadId = item.QuizAttempts.LeadUserId,
                                                                    CompletedOn = item.CompletedOn.Value
                                                                });
                                                            }
                                                        }
                                                    }
                                                    else if (ques.AnswerType == (int)AnswerTypeEnum.FullAddress)
                                                    {
                                                        var ans = item.QuizAnswerStats.FirstOrDefault();

                                                        var parentAnswerObj = quizComponentLogsList.FirstOrDefault(r => r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.PublishedObjectId == ans.AnswerId);
                                                        if (parentAnswerObj != null)
                                                        {
                                                            question.Answers.Add(new Question.Answer()
                                                            {
                                                                ParentAnswerid = parentAnswerObj.DraftedObjectId,
                                                                AnswerText = string.Join(", ", item.QuizAnswerStats.Select(r => r.AnswerText)),
                                                                LeadCount = 1,
                                                                LeadId = item.QuizAttempts.LeadUserId,
                                                                CompletedOn = item.CompletedOn.Value
                                                            });
                                                        }
                                                    }
                                                    else if (ques.AnswerType == (int)AnswerTypeEnum.RatingEmoji || ques.AnswerType == (int)AnswerTypeEnum.RatingStarts || ques.AnswerType == (int)AnswerTypeEnum.NPS)
                                                    {

                                                        foreach (var ans in item.QuizAnswerStats)
                                                        {
                                                            var parentAnswerObj = quizComponentLogsList.FirstOrDefault(r => r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.PublishedObjectId == ans.AnswerId);
                                                            if (parentAnswerObj != null)
                                                            {
                                                                var parentAnswerId = parentAnswerObj.DraftedObjectId;

                                                                var answer = question.Answers.FirstOrDefault(r => r.ParentAnswerid == parentAnswerId && r.AnswerText == ans.AnswerText);
                                                                if (answer != null)
                                                                    answer.LeadCount = answer.LeadCount + 1;
                                                                else
                                                                {
                                                                    question.Answers.Add(new Question.Answer()
                                                                    {
                                                                        ParentAnswerid = parentAnswerId,
                                                                        AnswerText = ans.AnswerText,
                                                                        LeadCount = 1,
                                                                        LeadId = string.Empty
                                                                    });
                                                                }

                                                                if ((ques.AnswerType == (int)AnswerTypeEnum.RatingEmoji || ques.AnswerType == (int)AnswerTypeEnum.RatingStarts) && !string.IsNullOrEmpty(ans.Comment))
                                                                {
                                                                    question.Comments.Add(new Question.CommentDetails()
                                                                    {
                                                                        AnswerText = ans.AnswerText,
                                                                        LeadId = item.QuizAttempts.LeadUserId,
                                                                        Comment = ans.Comment,
                                                                        CompletedOn = item.CompletedOn.Value
                                                                    });
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        foreach (var ans in item.QuizAnswerStats)
                                                        {
                                                            var parentAnswerObj = quizComponentLogsList.FirstOrDefault(r => r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.PublishedObjectId == ans.AnswerId);
                                                            if (parentAnswerObj != null)
                                                            {
                                                                var parentAnswerId = parentAnswerObj.DraftedObjectId;
                                                                var answer = question.Answers.FirstOrDefault(r => r.ParentAnswerid == parentAnswerId);

                                                                if (answer != null)
                                                                    answer.LeadCount = answer.LeadCount + 1;
                                                                else
                                                                {
                                                                    question.Answers.Add(new Question.Answer()
                                                                    {
                                                                        ParentAnswerid = parentAnswerId,
                                                                        AnswerText = answerList.FirstOrDefault(r => r.Id == parentAnswerId).Option,
                                                                        LeadCount = 1,
                                                                        LeadId = string.Empty
                                                                    });
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    var ansLst = new List<Question.Answer>();
                                                    var commentLst = new List<Question.CommentDetails>();
                                                    if (ques.AnswerType == (int)AnswerTypeEnum.FullAddress)
                                                    {
                                                        var ans = item.QuizAnswerStats.FirstOrDefault();
                                                        var parentAnswerObj = quizComponentLogsList.FirstOrDefault(r => r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.PublishedObjectId == ans.AnswerId);
                                                        if (parentAnswerObj != null)
                                                        {
                                                            ansLst.Add(new Question.Answer()
                                                            {
                                                                ParentAnswerid = parentAnswerObj.DraftedObjectId,
                                                                AnswerText = string.Join(", ", item.QuizAnswerStats.Select(r => r.AnswerText)),
                                                                LeadCount = 1,
                                                                LeadId = item.QuizAttempts.LeadUserId,
                                                                CompletedOn = item.CompletedOn.Value,
                                                            });
                                                        }
                                                    }
                                                    else
                                                    {
                                                        foreach (var ans in item.QuizAnswerStats)
                                                        {
                                                            var parentAnswerObj = quizComponentLogsList.FirstOrDefault(r => r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.PublishedObjectId == ans.AnswerId);
                                                            if (parentAnswerObj != null)
                                                            {
                                                                var parentAnswerId = parentAnswerObj.DraftedObjectId;

                                                                ansLst.Add(new Question.Answer()
                                                                {
                                                                    ParentAnswerid = parentAnswerId,
                                                                    AnswerText = (ques.AnswerType == (int)AnswerTypeEnum.Short || ques.AnswerType == (int)AnswerTypeEnum.Long || ques.AnswerType == (int)AnswerTypeEnum.DOB || ques.AnswerType == (int)AnswerTypeEnum.PostCode || ques.AnswerType == (int)AnswerTypeEnum.NPS || ques.AnswerType == (int)AnswerTypeEnum.RatingEmoji || ques.AnswerType == (int)AnswerTypeEnum.RatingStarts)
                                                                    ? ans.AnswerText : answerList.FirstOrDefault(r => r.Id == parentAnswerId).Option,
                                                                    LeadCount = 1,
                                                                    LeadId = (ques.AnswerType == (int)AnswerTypeEnum.Short || ques.AnswerType == (int)AnswerTypeEnum.Long || ques.AnswerType == (int)AnswerTypeEnum.DOB || ques.AnswerType == (int)AnswerTypeEnum.PostCode)
                                                                    ? item.QuizAttempts.LeadUserId : string.Empty,
                                                                    CompletedOn = (ques.AnswerType == (int)AnswerTypeEnum.Short || ques.AnswerType == (int)AnswerTypeEnum.Long || ques.AnswerType == (int)AnswerTypeEnum.DOB || ques.AnswerType == (int)AnswerTypeEnum.PostCode)
                                                                                    ? item.CompletedOn.Value : default(DateTime?)
                                                                });

                                                                if ((ques.AnswerType == (int)AnswerTypeEnum.RatingEmoji || ques.AnswerType == (int)AnswerTypeEnum.RatingStarts) && !string.IsNullOrEmpty(ans.Comment))
                                                                {
                                                                    commentLst.Add(new Question.CommentDetails()
                                                                    {
                                                                        AnswerText = ans.AnswerText,
                                                                        LeadId = item.QuizAttempts.LeadUserId,
                                                                        Comment = ans.Comment,
                                                                        CompletedOn = item.CompletedOn.Value
                                                                    });
                                                                }
                                                            }
                                                        }
                                                    }

                                                    Question.OptionTextforRatingTypeQuestion optionTextforRatingTypeQuestions = null;

                                                    if (ques.AnswerType == (int)AnswerTypeEnum.RatingEmoji || ques.AnswerType == (int)AnswerTypeEnum.RatingStarts)
                                                    {
                                                        optionTextforRatingTypeQuestions = new Question.OptionTextforRatingTypeQuestion();

                                                        var option = answerList.FirstOrDefault(r => r.Status == (int)StatusEnum.Active);
                                                        optionTextforRatingTypeQuestions.OptionTextforRatingOne = option.OptionTextforRatingOne;
                                                        optionTextforRatingTypeQuestions.OptionTextforRatingTwo = option.OptionTextforRatingTwo;
                                                        optionTextforRatingTypeQuestions.OptionTextforRatingThree = option.OptionTextforRatingThree;
                                                        optionTextforRatingTypeQuestions.OptionTextforRatingFour = option.OptionTextforRatingFour;
                                                        optionTextforRatingTypeQuestions.OptionTextforRatingFive = option.OptionTextforRatingFive;
                                                    }

                                                    quizReport.Questions.Add(new Question()
                                                    {
                                                        ParentQuestionId = parentQuesId,
                                                        QuestionTitle = ques.Question,
                                                        QuestionType = ques.AnswerType,
                                                        Answers = ansLst,
                                                        LeadCountForQuestion = 1,
                                                        Comments = commentLst,
                                                        OptionTextforRatingTypeQuestions = optionTextforRatingTypeQuestions
                                                    });
                                                }
                                            }
                                        }
                                    }


                                    #endregion

                                    #region TOP3

                                    IEnumerable<Db.QuizQuestionStats> quizQuestionStatsList = new List<Db.QuizQuestionStats>();

                                   
                                    quizQuestionStatsList = UOWObj.QuizQuestionStatsRepository.Get((r => r.Status == (int)StatusEnum.Active && r.CompletedOn.HasValue
                                    && (r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.RatingEmoji || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.RatingStarts)))
                                        .Where(r => quizAttemptsObj.Any(q => q.Id == r.QuizAttemptId));
                                    
                                    foreach (var parentId in parentIdList.Distinct())
                                    {
                                        var quizQuestionStats = quizQuestionStatsList.Where(r => r.QuizAttempts.QuizStats.Any(q => q.ResultId.HasValue
                                        && quizComponentLogsList.FirstOrDefault(c => c.ObjectTypeId == (int)BranchingLogicEnum.RESULT && c.PublishedObjectId == q.ResultId).DraftedObjectId == parentId));

                                        if (quizQuestionStats.Any())
                                        {
                                            var result = resultList.FirstOrDefault(r => r.Id == parentId);

                                            var topRecordsDetailObject = new TopRecordsDetail();
                                            topRecordsDetailObject.ParentResultId = parentId;
                                            topRecordsDetailObject.InternalResultTitle = result.InternalTitle;
                                            topRecordsDetailObject.ExternalResultTitle = result.Title;
                                            topRecordsDetailObject.NumberofLead = quizAttemptsObj.Count(r => r.QuizStats.Any(q => q.ResultId.HasValue && quizComponentLogsList.FirstOrDefault(c => c.ObjectTypeId == (int)BranchingLogicEnum.RESULT && c.PublishedObjectId == q.ResultId.Value).DraftedObjectId == parentId));
                                            topRecordsDetailObject.PositiveThings = new List<TopRecordsDetail.Thing>();
                                            topRecordsDetailObject.NegativeThings = new List<TopRecordsDetail.Thing>();
                                            foreach (var item in quizQuestionStats.Where(r => r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.RatingEmoji || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.RatingStarts).OrderByDescending(r => Convert.ToInt32(r.QuizAnswerStats.FirstOrDefault().AnswerText)).Take(3))
                                            {
                                                topRecordsDetailObject.PositiveThings.Add(new TopRecordsDetail.Thing()
                                                {
                                                    Rating = Convert.ToInt32(item.QuizAnswerStats.FirstOrDefault().AnswerText),
                                                    TopicTitle = item.QuestionsInQuiz.TopicTitle
                                                });
                                            }

                                            foreach (var item in quizQuestionStats.Where(r => r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.RatingEmoji || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.RatingStarts).OrderBy(r => Convert.ToInt32(r.QuizAnswerStats.FirstOrDefault().AnswerText)).Take(3))
                                            {
                                                topRecordsDetailObject.NegativeThings.Add(new TopRecordsDetail.Thing()
                                                {
                                                    Rating = Convert.ToInt32(item.QuizAnswerStats.FirstOrDefault().AnswerText),
                                                    TopicTitle = item.QuestionsInQuiz.TopicTitle
                                                });
                                            }

                                            quizReport.TopRecordsDetails.Add(topRecordsDetailObject);
                                        }
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
                        else
                        {
                            Status = ResultEnum.Error;
                            ErrorMessage = "Quiz not found for the QuizId " + QuizId;
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Template not found for the TemplateId " + TemplateId;
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
            return quizReport;
        }

        public LeadReportDetails GetQuizTemplateLeadReport(string TemplateId, string LeadUserId, CompanyModel CompanyObj)
        {
            LeadReportDetails leadReportDetails = new LeadReportDetails();
            leadReportDetails.leadReports = new List<LeadReport>();

            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var templateDetails = OWCHelper.GetTemplateLinkedAutomation(CompanyObj.ClientCode, TemplateId);

                    if (templateDetails != null && templateDetails.QuizId.GetValueOrDefault() > 0)
                    {
                        int QuizId = templateDetails.QuizId.Value;
                        var quizObj = UOWObj.QuizRepository.GetByID(QuizId);

                        if (quizObj != null && quizObj.CompanyId == CompanyObj.Id)
                        {
                            leadReportDetails.QuizId = QuizId;
                            leadReportDetails.QuizType = quizObj.QuizType;
                            leadReportDetails.TemplateId = templateDetails.ConfigurationId;
                            leadReportDetails.TemplateName = templateDetails.TemplateName;
                            CompanyModel companyObj = new CompanyModel()
                            {
                                ClientCode = quizObj.Company.ClientCode
                            };
                            leadReportDetails.LeadUserInfo = OWCHelper.GetLeadUserInfo(LeadUserId, companyObj);
                            var quizAttemptList = UOWObj.QuizAttemptsRepository.Get(filter: r => r.Mode == "AUDIT"
                                                   && r.QuizDetails.ParentQuizId == QuizId
                                                   && r.CompanyId == companyObj.Id
                                                   && r.LeadUserId == LeadUserId
                                                   && (templateDetails.AutomationConfigurationIds.Any() && (r.ConfigurationDetails != null && templateDetails.AutomationConfigurationIds.Contains(r.ConfigurationDetails.ConfigurationId)) || (r.WorkPackageInfoId != null && r.WorkPackageInfo.ConfigurationDetails != null
                                                   && templateDetails.AutomationConfigurationIds.Contains(r.WorkPackageInfo.ConfigurationDetails.ConfigurationId)))
                                , includeProperties: "QuizDetails, QuizStats, QuizQuestionStats, QuizQuestionStats.QuizAnswerStats");

                            var workpackageInfo = UOWObj.WorkPackageInfoRepository.Get(r => r.QuizId == QuizId && r.LeadUserId == LeadUserId && !r.QuizAttempts.Any() && (templateDetails.AutomationConfigurationIds.Any() && templateDetails.AutomationConfigurationIds.Contains(r.ConfigurationDetails.ConfigurationId)));

                            foreach (var workpackageInfoObj in workpackageInfo.OrderByDescending(r => r.Id))
                            {
                                var quizDetails = workpackageInfoObj.Quiz.QuizDetails.LastOrDefault();
                                var leadReport = new LeadReport();
                                leadReport.Results = new List<LeadReport.Result>();
                                leadReport.Questions = new List<LeadReport.Question>();
                               
                                leadReport.QuizTitle = HttpUtility.HtmlDecode(Regex.Replace(QuizService.VariableLinking(quizDetails.QuizTitle, quizDetails, null, true, false, null), "<.*?>", string.Empty));
                                leadReport.SentDate = workpackageInfoObj.CreatedOn.Value;
                                leadReportDetails.leadReports.Add(leadReport);
                            }

                            foreach (var quizAttempt in quizAttemptList.OrderByDescending(r => r.Id))
                            {
                                var quizDetails = quizAttempt.QuizDetails;
                                var quizStats = quizAttempt.QuizStats;
                                var resultList = quizDetails.QuizResults;

                                //for replace dynamic variable
                                var correctAnsCount = 0;
                                var ShowScoreValue = false;
                                var scoreValueTxt = string.Empty;
                                var resultSetting = quizDetails.ResultSettings.FirstOrDefault();

                                if (resultSetting != null && resultSetting.ShowScoreValue.HasValue && resultSetting.ShowScoreValue.Value)
                                {
                                    var attemptedQuestions = quizAttempt.QuizQuestionStats.Where(t => t.Status == (int)StatusEnum.Active && t.CompletedOn != null && (t.QuestionsInQuiz.TimerRequired ? true : t.QuizAnswerStats.Any(r => !r.AnswerOptionsInQuizQuestions.IsUnansweredType)));

                                    if (quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate || attemptedQuestions.Any(r => (r.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Any(t => t.Status == (int)StatusEnum.Active && t.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && t.IsCorrectForMultipleAnswer.HasValue && t.IsCorrectForMultipleAnswer.Value)) || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single))
                                    {
                                        if (quizDetails.Quiz.QuizType == (int)QuizTypeEnum.Score || quizDetails.Quiz.QuizType == (int)QuizTypeEnum.ScoreTemplate)
                                            correctAnsCount = attemptedQuestions.Sum(a => a.QuizAnswerStats.Sum(x => x.AnswerOptionsInQuizQuestions.AssociatedScore)).Value;
                                        else
                                            correctAnsCount = attemptedQuestions.Count(a => (a.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && ((a.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Where(r => r.Status == (int)StatusEnum.Active && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value).Select(s => s.Id).OrderBy(s => s)).SequenceEqual(a.QuizAnswerStats.Select(r => r.AnswerId).OrderBy(s => s)))) || (a.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single && a.QuizAnswerStats.FirstOrDefault().AnswerOptionsInQuizQuestions.IsCorrectAnswer == true));

                                        ShowScoreValue = true;
                                        scoreValueTxt = string.IsNullOrEmpty(scoreValueTxt) ? string.Empty : scoreValueTxt.Replace("%score%", ' ' + correctAnsCount.ToString() + ' ').Replace("%total%", ' ' + attemptedQuestions.Count(t => (t.QuestionsInQuiz.AnswerOptionsInQuizQuestions.Any(r => r.Status == (int)StatusEnum.Active && r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Multiple && r.IsCorrectForMultipleAnswer.HasValue && r.IsCorrectForMultipleAnswer.Value)) || t.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.Single).ToString() + ' ');
                                    }
                                }

                                var leadReport = new LeadReport();
                                leadReport.Results = new List<LeadReport.Result>();
                                leadReport.Questions = new List<LeadReport.Question>();

                                leadReport.QuizTitle = HttpUtility.HtmlDecode(Regex.Replace(QuizService.VariableLinking(quizDetails.QuizTitle, quizDetails, quizAttempt, true, ShowScoreValue, scoreValueTxt), "<.*?>", string.Empty));
                                leadReport.SentDate = quizAttempt.WorkPackageInfoId != null ? quizAttempt.WorkPackageInfo.CreatedOn.Value : quizAttempt.CreatedOn;
                                leadReport.StartDate = quizStats.Any() ? quizStats.FirstOrDefault().StartedOn : default(DateTime?);
                                leadReport.CompleteDate = (quizStats.Any() && quizStats.FirstOrDefault().CompletedOn.HasValue) ? quizStats.FirstOrDefault().CompletedOn.Value : default(DateTime?);
                                
                                var questionList = quizDetails.QuestionsInQuiz;

                                #region data for results

                                //Total Leads Completed Quiz Or Reached to result
                                var completedOrAchievedResultQuizObj = quizStats.Where(r => r.ResultId.HasValue || r.CompletedOn.HasValue);

                                foreach (var quizStatsObj in completedOrAchievedResultQuizObj)
                                {
                                    if (quizStatsObj.ResultId.HasValue)
                                    {
                                        var result = resultList.FirstOrDefault(r => r.Id == quizStatsObj.ResultId);
                                        leadReport.Results.Add(new LeadReport.Result()
                                        {
                                            ResultId = quizStatsObj.ResultId.Value,
                                            ExternalResultTitle = HttpUtility.HtmlDecode(Regex.Replace(QuizService.VariableLinking(result.Title, quizDetails, quizAttempt, false, ShowScoreValue, scoreValueTxt), "<.*?>", string.Empty)),
                                            InternalResultTitle = HttpUtility.HtmlDecode(Regex.Replace(QuizService.VariableLinking(result.InternalTitle, quizDetails, quizAttempt, false, ShowScoreValue, scoreValueTxt), "<.*?>", string.Empty)),
                                            CompleteDate = quizStatsObj.CompletedOn.HasValue ? quizStatsObj.CompletedOn.Value : default(DateTime?)
                                        });
                                    }
                                    else
                                    {
                                        //for no result
                                        leadReport.Results.Add(new LeadReport.Result()
                                        {
                                            ResultId = 0,
                                            ExternalResultTitle = string.Empty,
                                            InternalResultTitle = string.Empty,
                                            CompleteDate = quizStatsObj.CompletedOn.HasValue ? quizStatsObj.CompletedOn.Value : default(DateTime?)
                                        });
                                    }
                                }

                                #endregion

                                #region Questions

                                var quesStatsList = quizAttempt.QuizQuestionStats.Where(q => q.Status == (int)StatusEnum.Active && q.CompletedOn.HasValue);

                                foreach (var quesStatsObj in quesStatsList.Where(r => r.QuestionsInQuiz.Type == (int)BranchingLogicEnum.QUESTION))
                                {
                                    var questionInQuiz = quesStatsObj.QuestionsInQuiz;

                                    var question = new LeadReport.Question();

                                    question.QuestionId = quesStatsObj.QuestionId;
                                    question.QuestionTitle = HttpUtility.HtmlDecode(Regex.Replace(QuizService.VariableLinking(questionInQuiz.Question, quizDetails, quizAttempt, false, ShowScoreValue, scoreValueTxt), "<.*?>", string.Empty));
                                    question.QuestionType = questionInQuiz.AnswerType;
                                    question.QuestionImage = questionInQuiz.QuestionImage;

                                    LeadReport.Question.OptionTextforRatingTypeQuestion optionTextforRatingTypeQuestions = null;

                                    if (questionInQuiz.AnswerType == (int)AnswerTypeEnum.RatingEmoji || questionInQuiz.AnswerType == (int)AnswerTypeEnum.RatingStarts)
                                    {
                                        optionTextforRatingTypeQuestions = new LeadReport.Question.OptionTextforRatingTypeQuestion();
                                        var option = questionInQuiz.AnswerOptionsInQuizQuestions.FirstOrDefault(r => r.Status == (int)StatusEnum.Active);
                                        optionTextforRatingTypeQuestions.OptionTextforRatingOne = option.OptionTextforRatingOne;
                                        optionTextforRatingTypeQuestions.OptionTextforRatingTwo = option.OptionTextforRatingTwo;
                                        optionTextforRatingTypeQuestions.OptionTextforRatingThree = option.OptionTextforRatingThree;
                                        optionTextforRatingTypeQuestions.OptionTextforRatingFour = option.OptionTextforRatingFour;
                                        optionTextforRatingTypeQuestions.OptionTextforRatingFive = option.OptionTextforRatingFive;
                                    }

                                    question.OptionTextforRatingTypeQuestions = optionTextforRatingTypeQuestions;

                                    question.Answers = new List<LeadReport.Question.Answer>();
                                    question.Comments = new List<LeadReport.Question.CommentDetails>();

                                    if (questionInQuiz.AnswerType == (int)AnswerTypeEnum.FullAddress)
                                    {
                                        var quizAnswerStatsObj = quesStatsObj.QuizAnswerStats.FirstOrDefault();
                                        var ans = quizAnswerStatsObj.AnswerOptionsInQuizQuestions;

                                        var answerImage = string.Empty;
                                        var publicIdForAnswer = string.Empty;

                                        if (questionInQuiz.ShowAnswerImage.HasValue && questionInQuiz.ShowAnswerImage.Value && ans.EnableMediaFile && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttempt.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.ObjectId == ans.Id))
                                        {
                                            var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttempt.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.ObjectId == ans.Id);
                                            answerImage = mediaObj.ObjectValue;
                                            publicIdForAnswer = mediaObj.ObjectPublicId;
                                        }
                                        else
                                        {
                                            answerImage = questionInQuiz.ShowAnswerImage.HasValue && questionInQuiz.ShowAnswerImage.Value ? ans.OptionImage : string.Empty;
                                            publicIdForAnswer = questionInQuiz.ShowAnswerImage.HasValue && questionInQuiz.ShowAnswerImage.Value ? ans.PublicId : string.Empty;
                                        }

                                        question.Answers.Add(new LeadReport.Question.Answer()
                                        {
                                            AnswerId = quizAnswerStatsObj.AnswerId,
                                            AnswerText = string.Join(", ", quesStatsObj.QuizAnswerStats.Select(r => r.AnswerText)),
                                            OptionImage = answerImage,
                                            PublicId = publicIdForAnswer
                                        });
                                    }
                                    else
                                    {
                                        foreach (var quizAnswerStatsObj in quesStatsObj.QuizAnswerStats)
                                        {
                                            var ans = quizAnswerStatsObj.AnswerOptionsInQuizQuestions;

                                            var answerImage = string.Empty;
                                            var publicIdForAnswer = string.Empty;

                                            if (questionInQuiz.ShowAnswerImage.HasValue && questionInQuiz.ShowAnswerImage.Value && ans.EnableMediaFile && quizDetails.MediaVariablesDetails.Any(r => r.ConfigurationDetailsId == quizAttempt.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.ObjectId == ans.Id))
                                            {
                                                var mediaObj = quizDetails.MediaVariablesDetails.FirstOrDefault(r => r.ConfigurationDetailsId == quizAttempt.ConfigurationDetailsId && r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.ObjectId == ans.Id);
                                                answerImage = mediaObj.ObjectValue;
                                                publicIdForAnswer = mediaObj.ObjectPublicId;
                                            }
                                            else
                                            {
                                                answerImage = questionInQuiz.ShowAnswerImage.HasValue && questionInQuiz.ShowAnswerImage.Value ? ans.OptionImage : string.Empty;
                                                publicIdForAnswer = questionInQuiz.ShowAnswerImage.HasValue && questionInQuiz.ShowAnswerImage.Value ? ans.PublicId : string.Empty;
                                            }

                                            question.Answers.Add(new LeadReport.Question.Answer()
                                            {
                                                AnswerId = quizAnswerStatsObj.AnswerId,
                                                AnswerText = (questionInQuiz.AnswerType == (int)AnswerTypeEnum.Short || questionInQuiz.AnswerType == (int)AnswerTypeEnum.Long || questionInQuiz.AnswerType == (int)AnswerTypeEnum.DOB || questionInQuiz.AnswerType == (int)AnswerTypeEnum.PostCode || questionInQuiz.AnswerType == (int)AnswerTypeEnum.NPS || questionInQuiz.AnswerType == (int)AnswerTypeEnum.RatingEmoji || questionInQuiz.AnswerType == (int)AnswerTypeEnum.RatingStarts)
                                                                    ? quizAnswerStatsObj.AnswerText : HttpUtility.HtmlDecode(Regex.Replace(QuizService.VariableLinking(quizAnswerStatsObj.AnswerOptionsInQuizQuestions.Option, quizDetails, quizAttempt, false, ShowScoreValue, scoreValueTxt), "<.*?>", string.Empty)),
                                                OptionImage = answerImage,
                                                PublicId = publicIdForAnswer
                                            });

                                            if (!string.IsNullOrEmpty(quizAnswerStatsObj.Comment))
                                            {
                                                if (questionInQuiz.AnswerType == (int)AnswerTypeEnum.RatingEmoji || questionInQuiz.AnswerType == (int)AnswerTypeEnum.RatingStarts)
                                                {
                                                    question.Comments.Add(new LeadReport.Question.CommentDetails()
                                                    {
                                                        AnswerText = quizAnswerStatsObj.AnswerText,
                                                        Comment = quizAnswerStatsObj.Comment,
                                                        CompletedOn = quesStatsObj.CompletedOn.Value
                                                    });
                                                }
                                            }
                                        }
                                    }

                                    leadReport.Questions.Add(question);
                                }

                                #endregion

                                leadReportDetails.leadReports.Add(leadReport);
                            }

                            if (leadReportDetails.leadReports != null && leadReportDetails.leadReports.Any())
                                leadReportDetails.leadReports = leadReportDetails.leadReports.OrderByDescending(t => t.SentDate).ToList();
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

            return leadReportDetails;
        }

        public NPSReport GetNPSTemplateAutomationReport(string TemplateId, int ChartView, DateTime? FromDate, DateTime? ToDate, int? ResultId, CompanyModel CompanyObj)
        {
            NPSReport quizReport = null;

            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var templateDetails = OWCHelper.GetTemplateLinkedAutomation(CompanyObj.ClientCode, TemplateId);
                    if (templateDetails != null && templateDetails.QuizId.GetValueOrDefault() > 0)
                    {
                        int QuizId = templateDetails.QuizId.Value;
                        var quizObj = UOWObj.QuizRepository.GetByID(QuizId);

                        ToDate = ToDate.HasValue ? ToDate.Value : DateTime.UtcNow.Date;
                        FromDate = FromDate.HasValue ? FromDate.Value : ToDate.Value.AddMonths(-1);

                        if (quizObj != null && quizObj.QuizType == (int)QuizTypeEnum.NPS && quizObj.CompanyId == CompanyObj.Id)
                        {
                            Db.QuizDetails quizDetailsObj = new Db.QuizDetails();

                            quizDetailsObj =  quizObj.QuizDetails.FirstOrDefault(r => r.Status == (int)StatusEnum.Active && r.State == (int)QuizStateEnum.DRAFTED); ;

                            if (quizDetailsObj != null)
                            {
                                quizReport = new NPSReport();

                                quizReport.Stages = new List<NPSReport.Stage>();
                                quizReport.Results = new List<NPSReport.Result>();
                                quizReport.Questions = new List<NPSReport.Question>();
                                quizReport.NPSScoreDetails = new List<NPSReport.NPSScoreDetail>();
                                quizReport.TopRecordsDetails = new List<NPSReport.TopRecordsDetail>();
                                quizReport.TemplateName = templateDetails.TemplateName;
                                quizReport.TemplateId = templateDetails.ConfigurationId;

                                quizReport.QuizId = QuizId;
                                quizReport.QuizTitle = quizDetailsObj.QuizTitle;
                                quizReport.QuizType = quizObj.QuizType;

                                IEnumerable<Db.QuizAttempts> quizAttempts = new List<Db.QuizAttempts>();

                         
                                quizAttempts = UOWObj.QuizAttemptsRepository.Get(filter: r => r.Mode == "AUDIT"
                                                && r.QuizDetails.ParentQuizId == QuizId
                                                && (templateDetails.AutomationConfigurationIds.Any() && (r.ConfigurationDetails != null && templateDetails.AutomationConfigurationIds.Contains(r.ConfigurationDetails.ConfigurationId)) || (r.WorkPackageInfoId != null && r.WorkPackageInfo.ConfigurationDetails != null && templateDetails.AutomationConfigurationIds.Contains(r.WorkPackageInfo.ConfigurationDetails.ConfigurationId)))
                                                && r.RecruiterUserId == null
                                                && r.CompanyId == CompanyObj.Id
                            , includeProperties: "QuizStats, QuizQuestionStats");
                               
                                var workpackageInfoObj = UOWObj.WorkPackageInfoRepository.Get(r => r.QuizId == QuizId);

                                var sentReportAttrObj = workpackageInfoObj.Count(r => (r.CreatedOn.HasValue ? (r.CreatedOn.Value.Date >= FromDate && r.CreatedOn.Value.Date <= ToDate) : false))
                                    + quizAttempts.Count(r => r.WorkPackageInfoId == null && r.CreatedOn.Date >= FromDate && r.CreatedOn.Date <= ToDate);

                                quizReport.Stages.Add(new NPSReport.Stage()
                                {
                                    Id = (int)QuizReportingAttributeEnum.Sent,
                                    Value = sentReportAttrObj
                                });

                                if (quizAttempts.Any())
                                {
                                    var questionList = quizDetailsObj.QuestionsInQuiz;

                                    var resultList = quizDetailsObj.QuizResults;

                                    var quizComponentLogsList = UOWObj.QuizComponentLogsRepository.Get(r => r.QuizDetails.ParentQuizId == QuizId);

                                    var quizAttemptsObj = quizAttempts.Where(r => (r.WorkPackageInfoId != null && r.WorkPackageInfo.CreatedOn.Value.Date >= FromDate && r.WorkPackageInfo.CreatedOn.Value.Date <= ToDate)
                                                                        || (r.WorkPackageInfoId == null && r.CreatedOn.Date >= FromDate && r.CreatedOn.Date <= ToDate));

                                    #region data for stages

                                    var viewsReportAttrObj = quizAttemptsObj.Count(r => r.IsViewed);
                                    var startsReportAttrObj = quizAttemptsObj.Count(r => r.QuizStats.Any());
                                    var resultsReportAttrObj = quizAttemptsObj.Count(r => r.QuizStats.Any() && (r.QuizStats.FirstOrDefault().ResultId != null || r.QuizStats.FirstOrDefault().CompletedOn.HasValue));
                                    var leadsReportAttrObj = quizAttemptsObj.Count(r => r.LeadConvertedOn.HasValue && !string.IsNullOrEmpty(r.LeadUserId));

                                    quizReport.Stages.Add(new NPSReport.Stage()
                                    {
                                        Id = (int)QuizReportingAttributeEnum.Views,
                                        Value = viewsReportAttrObj
                                    });

                                    quizReport.Stages.Add(new NPSReport.Stage()
                                    {
                                        Id = (int)QuizReportingAttributeEnum.Starts,
                                        Value = startsReportAttrObj
                                    });

                                    quizReport.Stages.Add(new NPSReport.Stage()
                                    {
                                        Id = (int)QuizReportingAttributeEnum.Result,
                                        Value = resultsReportAttrObj
                                    });

                                    quizReport.Stages.Add(new NPSReport.Stage()
                                    {
                                        Id = (int)QuizReportingAttributeEnum.Leads,
                                        Value = leadsReportAttrObj
                                    });

                                    #endregion

                                    #region data for results

                                    //Total Leads Completed Quiz Or Reached to result
                                    var completedOrAchievedResultQuizObj = quizAttemptsObj.Select(r => r.QuizStats.Where(q => q.ResultId.HasValue || q.CompletedOn.HasValue));
                                    var completedOrAchievedResultQuizObjCount = completedOrAchievedResultQuizObj.Count(r => r.Any());
                                    var parentIdList = new List<int>();

                                    foreach (var attempt in completedOrAchievedResultQuizObj)
                                    {
                                        foreach (var item in attempt.Where(r => r.ResultId.HasValue))
                                        {
                                            var parentResultObj = quizComponentLogsList.FirstOrDefault(r => r.ObjectTypeId == (int)BranchingLogicEnum.RESULT && r.PublishedObjectId == item.ResultId);
                                            if (parentResultObj != null)
                                                parentIdList.Add(parentResultObj.DraftedObjectId);
                                        }
                                    }

                                    foreach (var resultId in parentIdList.Distinct())
                                    {
                                        var result = resultList.FirstOrDefault(r => r.Id == resultId);
                                        quizReport.Results.Add(new NPSReport.Result()
                                        {
                                            ParentResultId = resultId,
                                            ExternalResultTitle = result.Title,
                                            InternalResultTitle = result.InternalTitle,
                                            Value = parentIdList.Count(r => r == resultId),
                                            MinScore = result.MinScore.Value,
                                            MaxScore = result.MaxScore.Value
                                        });
                                    }

                                    #endregion

                                    #region Questions

                                    var quesStatsList = quizAttemptsObj.Where(r => ResultId.HasValue
                                            ? (ResultId == 0 ? r.QuizStats.Any(q => q.CompletedOn.HasValue && !q.ResultId.HasValue)
                                                        : r.QuizStats.Any(q => q.ResultId.HasValue && quizComponentLogsList.FirstOrDefault(c => c.ObjectTypeId == (int)BranchingLogicEnum.RESULT && c.PublishedObjectId == q.ResultId).DraftedObjectId == ResultId.Value))
                                            : true)
                                             .Select(r => r.QuizQuestionStats.Where(q => q.Status == (int)StatusEnum.Active && q.CompletedOn.HasValue));

                                    var quesParentIdList = new List<int>();

                                    foreach (var quesStatsObj in quesStatsList)
                                    {
                                        foreach (var item in quesStatsObj.Where(r => r.QuestionsInQuiz.Type == (int)BranchingLogicEnum.QUESTION))
                                        {
                                            var parentQuestionObject = quizComponentLogsList.FirstOrDefault(r => r.ObjectTypeId == (int)BranchingLogicEnum.QUESTION && r.PublishedObjectId == item.QuestionId);
                                            if (parentQuestionObject != null)
                                            {
                                                var parentQuesId = parentQuestionObject.DraftedObjectId;
                                                var ques = questionList.FirstOrDefault(r => r.Id == parentQuesId);
                                                var answerList = ques.AnswerOptionsInQuizQuestions;

                                                var question = quizReport.Questions.FirstOrDefault(r => r.ParentQuestionId == parentQuesId);

                                                if (question != null)
                                                {
                                                    question.LeadCountForQuestion = question.LeadCountForQuestion + 1;

                                                    var ansLst = new List<Question.Answer>();

                                                    if (ques.AnswerType == (int)AnswerTypeEnum.Short || ques.AnswerType == (int)AnswerTypeEnum.Long || ques.AnswerType == (int)AnswerTypeEnum.DOB || ques.AnswerType == (int)AnswerTypeEnum.PostCode)
                                                    {
                                                        foreach (var ans in item.QuizAnswerStats)
                                                        {
                                                            var parentAnswerObj = quizComponentLogsList.FirstOrDefault(r => r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.PublishedObjectId == ans.AnswerId);
                                                            if (parentAnswerObj != null)
                                                            {
                                                                question.Answers.Add(new NPSReport.Question.Answer()
                                                                {
                                                                    ParentAnswerid = parentAnswerObj.DraftedObjectId,
                                                                    AnswerText = ans.AnswerText,
                                                                    LeadCount = 1,
                                                                    LeadId = item.QuizAttempts.LeadUserId,
                                                                    CompletedOn = item.CompletedOn.Value
                                                                });
                                                            }
                                                        }
                                                    }
                                                    else if (ques.AnswerType == (int)AnswerTypeEnum.FullAddress)
                                                    {
                                                        var ans = item.QuizAnswerStats.FirstOrDefault();
                                                        var parentAnswerObj = quizComponentLogsList.FirstOrDefault(r => r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.PublishedObjectId == ans.AnswerId);
                                                        if (parentAnswerObj != null)
                                                        {
                                                            question.Answers.Add(new NPSReport.Question.Answer()
                                                            {
                                                                ParentAnswerid = parentAnswerObj.DraftedObjectId,
                                                                AnswerText = string.Join(", ", item.QuizAnswerStats.Select(r => r.AnswerText)),
                                                                LeadCount = 1,
                                                                LeadId = item.QuizAttempts.LeadUserId,
                                                                CompletedOn = item.CompletedOn.Value
                                                            });
                                                        }
                                                    }
                                                    else if (ques.AnswerType == (int)AnswerTypeEnum.RatingEmoji || ques.AnswerType == (int)AnswerTypeEnum.RatingStarts || ques.AnswerType == (int)AnswerTypeEnum.NPS || ques.AnswerType == (int)AnswerTypeEnum.Availability)
                                                    {
                                                        foreach (var ans in item.QuizAnswerStats)
                                                        {
                                                            var parentAnswerObj = quizComponentLogsList.FirstOrDefault(r => r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.PublishedObjectId == ans.AnswerId);
                                                            if (parentAnswerObj != null)
                                                            {
                                                                var parentAnswerId = parentAnswerObj.DraftedObjectId;

                                                                var answer = question.Answers.FirstOrDefault(r => r.ParentAnswerid == parentAnswerId && r.AnswerText == ans.AnswerText);
                                                                if (answer != null)
                                                                    answer.LeadCount = answer.LeadCount + 1;
                                                                else
                                                                {
                                                                    question.Answers.Add(new NPSReport.Question.Answer()
                                                                    {
                                                                        ParentAnswerid = parentAnswerId,
                                                                        AnswerText = ans.AnswerText,
                                                                        LeadCount = 1,
                                                                        LeadId = string.Empty
                                                                    });
                                                                }

                                                                if ((ques.AnswerType == (int)AnswerTypeEnum.RatingEmoji || ques.AnswerType == (int)AnswerTypeEnum.RatingStarts || ques.AnswerType == (int)AnswerTypeEnum.Availability) && !string.IsNullOrEmpty(ans.Comment))
                                                                {
                                                                    question.Comments.Add(new NPSReport.Question.CommentDetails()
                                                                    {
                                                                        AnswerText = ans.AnswerText,
                                                                        LeadId = item.QuizAttempts.LeadUserId,
                                                                        Comment = ans.Comment,
                                                                        CompletedOn = item.CompletedOn.Value
                                                                    });
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        foreach (var ans in item.QuizAnswerStats)
                                                        {
                                                            var parentAnswerObj = quizComponentLogsList.FirstOrDefault(r => r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.PublishedObjectId == ans.AnswerId);
                                                            if (parentAnswerObj != null)
                                                            {
                                                                var parentAnswerId = parentAnswerObj.DraftedObjectId;
                                                                var answer = question.Answers.FirstOrDefault(r => r.ParentAnswerid == parentAnswerId);

                                                                if (answer != null)
                                                                    answer.LeadCount = answer.LeadCount + 1;
                                                                else
                                                                {
                                                                    question.Answers.Add(new NPSReport.Question.Answer()
                                                                    {
                                                                        ParentAnswerid = parentAnswerId,
                                                                        AnswerText = answerList.FirstOrDefault(r => r.Id == parentAnswerId).Option,
                                                                        LeadCount = 1,
                                                                        LeadId = string.Empty
                                                                    });
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    var ansLst = new List<NPSReport.Question.Answer>();
                                                    var commentLst = new List<NPSReport.Question.CommentDetails>();
                                                    if (ques.AnswerType == (int)AnswerTypeEnum.FullAddress)
                                                    {
                                                        var ans = item.QuizAnswerStats.FirstOrDefault();
                                                        var parentAnswerObj = quizComponentLogsList.FirstOrDefault(r => r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.PublishedObjectId == ans.AnswerId);
                                                        if (parentAnswerObj != null)
                                                        {
                                                            ansLst.Add(new NPSReport.Question.Answer()
                                                            {
                                                                ParentAnswerid = parentAnswerObj.DraftedObjectId,
                                                                AnswerText = string.Join(", ", item.QuizAnswerStats.Select(r => r.AnswerText)),
                                                                LeadCount = 1,
                                                                LeadId = item.QuizAttempts.LeadUserId,
                                                                CompletedOn = item.CompletedOn.Value,
                                                            });
                                                        }
                                                    }
                                                    else
                                                    {
                                                        foreach (var ans in item.QuizAnswerStats)
                                                        {
                                                            var parentAnswerObj = quizComponentLogsList.FirstOrDefault(r => r.ObjectTypeId == (int)BranchingLogicEnum.ANSWER && r.PublishedObjectId == ans.AnswerId);
                                                            if (parentAnswerObj != null)
                                                            {
                                                                var parentAnswerId = parentAnswerObj.DraftedObjectId;

                                                                ansLst.Add(new NPSReport.Question.Answer()
                                                                {
                                                                    ParentAnswerid = parentAnswerId,
                                                                    AnswerText = (ques.AnswerType == (int)AnswerTypeEnum.Short || ques.AnswerType == (int)AnswerTypeEnum.Long || ques.AnswerType == (int)AnswerTypeEnum.DOB || ques.AnswerType == (int)AnswerTypeEnum.PostCode || ques.AnswerType == (int)AnswerTypeEnum.NPS || ques.AnswerType == (int)AnswerTypeEnum.RatingEmoji || ques.AnswerType == (int)AnswerTypeEnum.RatingStarts || ques.AnswerType == (int)AnswerTypeEnum.Availability)
                                                                    ? ans.AnswerText : answerList.FirstOrDefault(r => r.Id == parentAnswerId).Option,
                                                                    LeadCount = 1,
                                                                    LeadId = (ques.AnswerType == (int)AnswerTypeEnum.Short || ques.AnswerType == (int)AnswerTypeEnum.Long || ques.AnswerType == (int)AnswerTypeEnum.DOB || ques.AnswerType == (int)AnswerTypeEnum.PostCode || ques.AnswerType == (int)AnswerTypeEnum.Availability)
                                                                    ? item.QuizAttempts.LeadUserId : string.Empty,
                                                                    CompletedOn = (ques.AnswerType == (int)AnswerTypeEnum.Short || ques.AnswerType == (int)AnswerTypeEnum.Long || ques.AnswerType == (int)AnswerTypeEnum.DOB || ques.AnswerType == (int)AnswerTypeEnum.PostCode || ques.AnswerType == (int)AnswerTypeEnum.Availability)
                                                                                    ? item.CompletedOn.Value : default(DateTime?)
                                                                });

                                                                if ((ques.AnswerType == (int)AnswerTypeEnum.RatingEmoji || ques.AnswerType == (int)AnswerTypeEnum.RatingStarts || ques.AnswerType == (int)AnswerTypeEnum.Availability) && !string.IsNullOrEmpty(ans.Comment))
                                                                {
                                                                    commentLst.Add(new NPSReport.Question.CommentDetails()
                                                                    {
                                                                        AnswerText = ans.AnswerText,
                                                                        LeadId = item.QuizAttempts.LeadUserId,
                                                                        Comment = ans.Comment,
                                                                        CompletedOn = item.CompletedOn.Value
                                                                    });
                                                                }
                                                            }
                                                        }
                                                    }

                                                    NPSReport.Question.OptionTextforRatingTypeQuestion optionTextforRatingTypeQuestions = null;

                                                    if (ques.AnswerType == (int)AnswerTypeEnum.RatingEmoji || ques.AnswerType == (int)AnswerTypeEnum.RatingStarts)
                                                    {
                                                        optionTextforRatingTypeQuestions = new NPSReport.Question.OptionTextforRatingTypeQuestion();
                                                        var option = answerList.FirstOrDefault(r => r.Status == (int)StatusEnum.Active);
                                                        optionTextforRatingTypeQuestions.OptionTextforRatingOne = option.OptionTextforRatingOne;
                                                        optionTextforRatingTypeQuestions.OptionTextforRatingTwo = option.OptionTextforRatingTwo;
                                                        optionTextforRatingTypeQuestions.OptionTextforRatingThree = option.OptionTextforRatingThree;
                                                        optionTextforRatingTypeQuestions.OptionTextforRatingFour = option.OptionTextforRatingFour;
                                                        optionTextforRatingTypeQuestions.OptionTextforRatingFive = option.OptionTextforRatingFive;
                                                    }

                                                    quizReport.Questions.Add(new NPSReport.Question()
                                                    {
                                                        ParentQuestionId = parentQuesId,
                                                        QuestionTitle = ques.Question,
                                                        QuestionType = ques.AnswerType,
                                                        Answers = ansLst,
                                                        LeadCountForQuestion = 1,
                                                        Comments = commentLst,
                                                        OptionTextforRatingTypeQuestions = optionTextforRatingTypeQuestions
                                                    });
                                                }
                                            }
                                        }
                                    }


                                    #endregion

                                    #region NPSScoreDetails for each day

                                    if (ChartView == (int)ChartViewTypeEnum.Day)
                                    {
                                        DateTime? startDate = FromDate;
                                        while (startDate <= ToDate)
                                        {
                                            float detractorResultCount = quizAttemptsObj.Count(r => r.QuizStats.Any(q => q.ResultId.HasValue && q.QuizResults.MinScore >= 0 && q.QuizResults.MaxScore <= 6 && q.CompletedOn.HasValue && q.CompletedOn.Value.Date == startDate.Value.Date));
                                            float passiveResultCount = quizAttemptsObj.Count(r => r.QuizStats.Any(q => q.ResultId.HasValue && q.QuizResults.MinScore >= 7 && q.QuizResults.MaxScore <= 8 && q.CompletedOn.HasValue && q.CompletedOn.Value.Date == startDate.Value.Date));
                                            float promoterResultCount = quizAttemptsObj.Count(r => r.QuizStats.Any(q => q.ResultId.HasValue && q.QuizResults.MinScore >= 9 && q.QuizResults.MaxScore <= 10 && q.CompletedOn.HasValue && q.CompletedOn.Value.Date == startDate.Value.Date));

                                            var total = detractorResultCount + passiveResultCount + promoterResultCount;

                                            quizReport.NPSScoreDetails.Add(new NPSReport.NPSScoreDetail()
                                            {
                                                Date = startDate.Value,
                                                Day = startDate.Value.DayOfWeek,
                                                DayNumber = startDate.Value.Day,
                                                Year = startDate.Value.Year,
                                                NPSScore = total > 0 ? (promoterResultCount / total * 100) - (detractorResultCount / total * 100) : 0
                                            });

                                            startDate = startDate.Value.AddDays(1);
                                        }
                                    }

                                    if (ChartView == (int)ChartViewTypeEnum.Week)
                                    {
                                        DateTime? startDate = FromDate;
                                        while (startDate <= ToDate)
                                        {
                                            int weekCount = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                                                        startDate.Value,
                                                        CultureInfo.CurrentCulture.DateTimeFormat.CalendarWeekRule,
                                                        CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek);

                                            var endDate = startDate.Value.AddDays(-(int)startDate.Value.DayOfWeek).AddDays(7).AddSeconds(-1);

                                            if (endDate > ToDate)
                                                endDate = ToDate.Value;
                                            if (startDate.Value.Year != endDate.Year)
                                                endDate = new DateTime(startDate.Value.Year, startDate.Value.Month, DateTime.DaysInMonth(startDate.Value.Year, startDate.Value.Month));


                                            float detractorResultCount = quizAttemptsObj.Count(r => r.QuizStats.Any(q => q.ResultId.HasValue && q.QuizResults.MinScore >= 0 && q.QuizResults.MaxScore <= 6 && q.CompletedOn.HasValue && q.CompletedOn.Value.Date >= startDate.Value.Date && q.CompletedOn.Value.Date <= endDate));
                                            float passiveResultCount = quizAttemptsObj.Count(r => r.QuizStats.Any(q => q.ResultId.HasValue && q.QuizResults.MinScore >= 7 && q.QuizResults.MaxScore <= 8 && q.CompletedOn.HasValue && q.CompletedOn.Value.Date >= startDate.Value.Date && q.CompletedOn.Value.Date <= endDate));
                                            float promoterResultCount = quizAttemptsObj.Count(r => r.QuizStats.Any(q => q.ResultId.HasValue && q.QuizResults.MinScore >= 9 && q.QuizResults.MaxScore <= 10 && q.CompletedOn.HasValue && q.CompletedOn.Value.Date >= startDate.Value.Date && q.CompletedOn.Value.Date <= endDate));

                                            var total = detractorResultCount + passiveResultCount + promoterResultCount;

                                            quizReport.NPSScoreDetails.Add(new NPSReport.NPSScoreDetail()
                                            {
                                                Week = weekCount,
                                                Year = startDate.Value.Year,
                                                NPSScore = total > 0 ? (promoterResultCount / total * 100) - (detractorResultCount / total * 100) : 0
                                            });

                                            startDate = endDate.AddDays(1);
                                        }
                                    }

                                    if (ChartView == (int)ChartViewTypeEnum.Month)
                                    {
                                        DateTime? startDate = FromDate;
                                        while (startDate.Value.Month <= ToDate.Value.Month)
                                        {
                                            float detractorResultCount = quizAttemptsObj.Count(r => r.QuizStats.Any(q => q.ResultId.HasValue && q.QuizResults.MinScore >= 0 && q.QuizResults.MaxScore <= 6 && q.CompletedOn.HasValue && q.CompletedOn.Value.Month == startDate.Value.Month));
                                            float passiveResultCount = quizAttemptsObj.Count(r => r.QuizStats.Any(q => q.ResultId.HasValue && q.QuizResults.MinScore >= 7 && q.QuizResults.MaxScore <= 8 && q.CompletedOn.HasValue && q.CompletedOn.Value.Month == startDate.Value.Month));
                                            float promoterResultCount = quizAttemptsObj.Count(r => r.QuizStats.Any(q => q.ResultId.HasValue && q.QuizResults.MinScore >= 9 && q.QuizResults.MaxScore <= 10 && q.CompletedOn.HasValue && q.CompletedOn.Value.Month == startDate.Value.Month));

                                            var total = detractorResultCount + passiveResultCount + promoterResultCount;


                                            quizReport.NPSScoreDetails.Add(new NPSReport.NPSScoreDetail()
                                            {
                                                MonthNumber = startDate.Value.Month,
                                                MonthName = startDate.Value.ToString("MMMM"),
                                                Year = startDate.Value.Year,
                                                NPSScore = (total > 0) ? (promoterResultCount / total * 100) - (detractorResultCount / total * 100) : 0
                                            });

                                            startDate = startDate.Value.AddMonths(1);
                                        }
                                    }

                                    if (ChartView == (int)ChartViewTypeEnum.Year)
                                    {
                                        DateTime? startDate = FromDate;
                                        while (startDate <= ToDate)
                                        {
                                            float detractorResultCount = quizAttemptsObj.Count(r => r.QuizStats.Any(q => q.ResultId.HasValue && q.QuizResults.MinScore >= 0 && q.QuizResults.MaxScore <= 6 && q.CompletedOn.HasValue && q.CompletedOn.Value.Year == startDate.Value.Year));
                                            float passiveResultCount = quizAttemptsObj.Count(r => r.QuizStats.Any(q => q.ResultId.HasValue && q.QuizResults.MinScore >= 7 && q.QuizResults.MaxScore <= 8 && q.CompletedOn.HasValue && q.CompletedOn.Value.Year == startDate.Value.Year));
                                            float promoterResultCount = quizAttemptsObj.Count(r => r.QuizStats.Any(q => q.ResultId.HasValue && q.QuizResults.MinScore >= 9 && q.QuizResults.MaxScore <= 10 && q.CompletedOn.HasValue && q.CompletedOn.Value.Year == startDate.Value.Year));

                                            var total = detractorResultCount + passiveResultCount + promoterResultCount;

                                            quizReport.NPSScoreDetails.Add(new NPSReport.NPSScoreDetail()
                                            {
                                                Year = startDate.Value.Year,
                                                NPSScore = total > 0 ? (promoterResultCount / total * 100) - (detractorResultCount / total * 100) : 0
                                            });

                                            startDate = startDate.Value.AddYears(1);
                                        }
                                    }

                                    #endregion

                                    #region NPSScore

                                    if (quizReport.Results.Any())
                                    {
                                        float score = 0;

                                        float detractorResultCount = quizReport.Results.Any(r => r.MinScore >= 0 && r.MaxScore <= 6) ? quizReport.Results.FirstOrDefault(r => r.MinScore >= 0 && r.MaxScore <= 6).Value : 0;
                                        float passiveResultCount = quizReport.Results.Any(r => r.MinScore >= 7 && r.MaxScore <= 8) ? quizReport.Results.FirstOrDefault(r => r.MinScore >= 7 && r.MaxScore <= 8).Value : 0;
                                        float promoterResultCount = quizReport.Results.Any(r => r.MinScore >= 9 && r.MaxScore <= 10) ? quizReport.Results.FirstOrDefault(r => r.MinScore >= 9 && r.MaxScore <= 10).Value : 0;

                                        var total = detractorResultCount + passiveResultCount + promoterResultCount;
                                        if (total > 0)
                                            score = ((promoterResultCount / total) * 100) - ((detractorResultCount / total) * 100);
                                        quizReport.NPSScore = score;
                                        quizReport.DetractorResultCount = detractorResultCount;
                                        quizReport.PassiveResultCount = passiveResultCount;
                                        quizReport.PromoterResultCount = promoterResultCount;
                                    }

                                    #endregion

                                    #region TOP3

                                    IEnumerable<Db.QuizQuestionStats> quizQuestionStatsList = new List<Db.QuizQuestionStats>();

                                    quizQuestionStatsList = UOWObj.QuizQuestionStatsRepository.Get(r => r.Status == (int)StatusEnum.Active && r.CompletedOn.HasValue
                                    && (r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.RatingEmoji || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.RatingStarts))
                                        .Where(r => quizAttemptsObj.Any(q => q.Id == r.QuizAttemptId));
                                   

                                    foreach (var parentId in parentIdList.Distinct())
                                    {
                                        var quizQuestionStats = quizQuestionStatsList.Where(r => r.QuizAttempts.QuizStats.Any(q => q.ResultId.HasValue
                                        && quizComponentLogsList.FirstOrDefault(c => c.ObjectTypeId == (int)BranchingLogicEnum.RESULT && c.PublishedObjectId == q.ResultId).DraftedObjectId == parentId));

                                        if (quizQuestionStats.Any())
                                        {

                                            var result = resultList.FirstOrDefault(r => r.Id == parentId);

                                            var topRecordsDetailObject = new NPSReport.TopRecordsDetail();
                                            topRecordsDetailObject.ParentResultId = parentId;
                                            topRecordsDetailObject.InternalResultTitle = result.InternalTitle;
                                            topRecordsDetailObject.ExternalResultTitle = result.Title;
                                            topRecordsDetailObject.NumberofLead = quizAttemptsObj.Count(r => r.QuizStats.Any(q => q.ResultId.HasValue && quizComponentLogsList.FirstOrDefault(c => c.ObjectTypeId == (int)BranchingLogicEnum.RESULT && c.PublishedObjectId == q.ResultId.Value).DraftedObjectId == parentId));
                                            topRecordsDetailObject.PositiveThings = new List<NPSReport.TopRecordsDetail.Thing>();
                                            topRecordsDetailObject.NegativeThings = new List<NPSReport.TopRecordsDetail.Thing>();
                                            foreach (var item in quizQuestionStats.Where(r => r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.RatingEmoji || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.RatingStarts).OrderByDescending(r => Convert.ToInt32(r.QuizAnswerStats.FirstOrDefault().AnswerText)).Take(3))
                                            {
                                                topRecordsDetailObject.PositiveThings.Add(new NPSReport.TopRecordsDetail.Thing()
                                                {
                                                    Rating = Convert.ToInt32(item.QuizAnswerStats.FirstOrDefault().AnswerText),
                                                    TopicTitle = item.QuestionsInQuiz.TopicTitle
                                                });
                                            }

                                            foreach (var item in quizQuestionStats.Where(r => r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.RatingEmoji || r.QuestionsInQuiz.AnswerType == (int)AnswerTypeEnum.RatingStarts).OrderBy(r => Convert.ToInt32(r.QuizAnswerStats.FirstOrDefault().AnswerText)).Take(3))
                                            {
                                                topRecordsDetailObject.NegativeThings.Add(new NPSReport.TopRecordsDetail.Thing()
                                                {
                                                    Rating = Convert.ToInt32(item.QuizAnswerStats.FirstOrDefault().AnswerText),
                                                    TopicTitle = item.QuestionsInQuiz.TopicTitle
                                                });
                                            }

                                            quizReport.TopRecordsDetails.Add(topRecordsDetailObject);
                                        }
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
            return quizReport;
        }

        public TemplatateDetail GetTemplateQuizDetails(string TemplateId, CompanyModel CompanyObj)
        {
            TemplatateDetail quizReport = null;

            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var templateDetails = OWCHelper.GetTemplateLinkedAutomation(CompanyObj.ClientCode, TemplateId);
                    if (templateDetails != null && templateDetails.QuizId.GetValueOrDefault() > 0)
                    {
                        int QuizId = templateDetails.QuizId.Value;
                        var quizObj = UOWObj.QuizRepository.GetByID(QuizId);

                        if (quizObj != null && quizObj.CompanyId == CompanyObj.Id)
                        {
                            Db.QuizDetails quizDetailsObj = new Db.QuizDetails();

                             quizDetailsObj = quizObj.QuizDetails.FirstOrDefault(r => r.Status == (int)StatusEnum.Active && r.State == (int)QuizStateEnum.DRAFTED); ;
                            

                            if (quizDetailsObj != null)
                            {
                                quizReport = new TemplatateDetail();

                                quizReport.QuizId = QuizId;
                                quizReport.QuizTitle = quizDetailsObj.QuizTitle;
                                quizReport.QuizType = quizObj.QuizType;

                                quizReport.TemplateName = templateDetails.TemplateName;
                                quizReport.TemplateId = templateDetails.ConfigurationId;
                                quizReport.AutomationConfigurationIds = templateDetails.AutomationConfigurationIds;
                            }
                            else
                            {
                                Status = ResultEnum.Error;
                                ErrorMessage = "Quiz not found for the QuizId " + QuizId;
                            }
                        }
                        else
                        {
                            Status = ResultEnum.Error;
                            ErrorMessage = "Quiz not found for the QuizId " + QuizId;
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Template not found for the TemplateId " + TemplateId;
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
            return quizReport;
        }
    }
}
