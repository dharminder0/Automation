using System;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Http;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using QuizApp.Helpers;
using QuizApp.Services.Service;

namespace QuizApp
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        static IContainer _container;

        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            if (ConfigurationManager.AppSettings["APPINSIGHTS_INSTRUMENTATIONKEY"] != null) {
                TelemetryConfiguration configuration = TelemetryConfiguration.CreateDefault();
                configuration.InstrumentationKey = ConfigurationManager.AppSettings["APPINSIGHTS_INSTRUMENTATIONKEY"];
                var telemetryClient = new TelemetryClient(configuration);
                TelemetryConfiguration.Active.TelemetryInitializers.Add(new RequestBodyInitializer());
            }

            QuizApp.Helpers.Utility.LoadTemplatesMessages();

            NLogHelper.InitializeNLog();

            FilterConfig.RegisterGlobalFilter(System.Web.Mvc.GlobalFilters.Filters);
            var enabledBackgroundServicesNames = ConfigurationManager.AppSettings["EnabledBackgroundServicesNames"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (enabledBackgroundServicesNames.Contains("DeleteOldLogService")) {
                IApiUsageLogsService commonService = new ApiUsageLogsService();
                ThreadPool.QueueUserWorkItem(commonService.RunDeleteLogsService);
            }

            if (enabledBackgroundServicesNames.Contains("InCompleteQuiz")) {
                IUncompleteQuizService commonService = new UncompleteQuizService();
                ThreadPool.QueueUserWorkItem(commonService.UpdateUncompleteService);
            }

        }

        public class RequestBodyInitializer : ITelemetryInitializer {
            public void Initialize(ITelemetry telemetry) {
                var requestTelemetry = telemetry as RequestTelemetry;
                if (requestTelemetry != null && requestTelemetry.Success == false && Regex.IsMatch(requestTelemetry.Name, "^(POST|PUT)", RegexOptions.IgnoreCase)) {
                    using (var reader = new StreamReader(HttpContext.Current.Request.InputStream)) {
                        string requestBody = reader.ReadToEnd();
                        requestTelemetry.Properties.Add("body", requestBody);
                    }
                }
            }
        }
    }
}
