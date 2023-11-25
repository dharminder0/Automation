using NLog;
using QuizApp.Services.Service;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Newtonsoft.Json;
using System.Text;
using System.Web;
using QuizApp.Helpers;
using Unity;
using QuizApp.RepositoryPattern;
using Core.Common.Extensions;
using QuizApp.Services.Model;

namespace QuizApp.Controllers
{
    public class BaseController : ApiController
    {
        public NLog.Logger Logger = LogManager.GetCurrentClassLogger();

        protected int BusinessUserId
        {
            get
            {
                object obj = null;
                Request.Properties.TryGetValue("BusinessUserId", out obj);

                return Convert.ToInt32(obj);
            }
        }
        protected bool CreateAcademyCourse
        {
            get
            {
                object obj = null;
                Request.Properties.TryGetValue("CreateAcademyCourse", out obj);

                return Convert.ToBoolean(obj);
            }
        }
        protected bool CreateTechnicalRecruiterCourse
        {
            get
            {
                object obj = null;
                Request.Properties.TryGetValue("CreateTechnicalRecruiterCourse", out obj);

                return Convert.ToBoolean(obj);
            }
        }
        protected bool CreateTemplate
        {
            get
            {
                object obj = null;
                Request.Properties.TryGetValue("CreateTemplate", out obj);

                return Convert.ToBoolean(obj);
            }
        }
        protected CompanyModel CompanyInfo
        {
            get
            {
                object obj = null;
                Request.Properties.TryGetValue("CompanyInfo", out obj);

                return (CompanyModel)obj;
            }
        }

        protected Services.Model.BusinessUser UserInfo
        {
            get
            {
                object obj = null;
                Request.Properties.TryGetValue("UserInfo", out obj);

                return (Services.Model.BusinessUser)obj;
            }
        }

        protected bool IsGlobalOfficeAdmin
        {
            get
            {
                object obj = null;
                Request.Properties.TryGetValue("IsGlobalOfficeAdmin", out obj);

                return Convert.ToBoolean(obj);
            }
        }
    }

    public class AuthorizeTokenFilter : ActionFilterAttribute
    {
        public NLog.Logger Logger = LogManager.GetCurrentClassLogger();

        private IBusinessUserService _iBusinessUserService;
        public bool IsJWTRequired { get; set; } = true;
        Services.Model.BusinessUser UserInfo;

        //This is the magic part - Unity reads this attribute and sets injects the related property.This means no parameters are required in the constructor.

        [Dependency]
        public IBusinessUserService iBusinessUserService
        {
            get { return this._iBusinessUserService; }
            set { this._iBusinessUserService = value; }
        }

        public override void OnActionExecuting(HttpActionContext filterContext)
        {
            var unAuthorizedActionlist = new List<string> {"SendSmsByWorkPackageId","SaveQuizUrlSetting" ,"CheckQuizUrlSettingKey" ,"GetUrlValueByKey" ,"RemoveConfiguration","AddOrUpdateConfigurationDetails" ,"GetDynamicFieldByQuizId" ,"ClearCache" ,"GetAttemptedAutomationByLeads" ,"GetAttemptedAutomationAcheivedResultDetailsByLeads","AttemptQuiz" ,"GetQuizAttemptCode" ,"SaveLeadUserInfo" ,"GetAttemptedQuizDetailByLead", "GetQuestionMappedFieldValues","RemoveTag" ,"UpdateTag" ,"UpdateCategory" ,"RemoveCategory" ,"GetAutomationTags" ,"GetVariablesByQuizId","GetQuizResultAndAction" ,"GetQuizAllResultAndAction" ,"GetSHA256GeneratorValue" ,"ClearOldLogs","AttemptQuizSettings","ClearPreviewQuiz", "GetAutomationDetails", "HSMTemplateDetails", "DeleteVerifyRequest", "AttemptQuizSettings", "UpdateCommunicationStatus", "FieldSyncSetting", "DeleteContactData", "MarkQuizIncomplete" };
            HttpRequestHeaders Headers = filterContext.Request.Headers;

            var actionName = ((System.Web.Http.Controllers.ReflectedHttpActionDescriptor)filterContext.ActionDescriptor).ActionName;

            if (Headers.Contains("ApiSecret"))
            {
                if (Headers.GetValues("ApiSecret").First() != GlobalSettings.apiSecret.ToString())
                {
                    filterContext.Response = filterContext.Request.CreateResponse(HttpStatusCode.Unauthorized, new { data = "Invalid ApiSecret" });
                }
            }
            else
            {
                filterContext.Response = filterContext.Request.CreateResponse(HttpStatusCode.Unauthorized, new { data = "No ApiSecret found" });
            }

            if (IsJWTRequired)
            {
               
                if (!(filterContext.ControllerContext.ControllerDescriptor.ControllerName == "Cron" || filterContext.ControllerContext.ControllerDescriptor.ControllerName == "Integration" || unAuthorizedActionlist.Contains(actionName)) || Headers.Contains("JwtToken"))
                {
                    if (Headers.Contains("JwtToken"))
                    {
                        try
                        {
                            if (Headers.Contains("CompanyCode"))
                            {
                                UserInfo = iBusinessUserService.AuthorizeToken(Headers.GetValues("JwtToken").First(), Headers.GetValues("CompanyCode").First());

                                int businessUserId = UserInfo.BusinessUserId;

                                if (businessUserId == -1)
                                {
                                    filterContext.Response = filterContext.Request.CreateResponse(HttpStatusCode.Unauthorized, new { data = UserInfo.CompanyInfo.JobRocketClientUrl, message = "Invalid JwtToken" });
                                }
                                else
                                {
                                    filterContext.Request.Properties.Add(new KeyValuePair<string, object>("BusinessUserId", businessUserId));
                                    filterContext.Request.Properties.Add(new KeyValuePair<string, object>("CompanyInfo", UserInfo.CompanyInfo));
                                    filterContext.Request.Properties.Add(new KeyValuePair<string, object>("UserInfo", UserInfo));
                                    filterContext.Request.Properties.Add(new KeyValuePair<string, object>("CreateAcademyCourse", UserInfo.CreateAcademyCourse));
                                    filterContext.Request.Properties.Add(new KeyValuePair<string, object>("CreateTechnicalRecruiterCourse", UserInfo.CreateTechnicalRecruiterCourse));
                                    filterContext.Request.Properties.Add(new KeyValuePair<string, object>("CreateTemplate", UserInfo.CreateTemplate));
                                    filterContext.Request.Properties.Add(new KeyValuePair<string, object>("IsGlobalOfficeAdmin", UserInfo.IsGlobalOfficeAdmin));
                                }
                            }
                            else
                            {
                                filterContext.Response = filterContext.Request.CreateResponse(HttpStatusCode.Unauthorized, new { data = "No CompanyCode found" });
                            }
                        }
                        catch (Exception ex)
                        {
                            ErrorLog.LogError(ex);
                            filterContext.Response = filterContext.Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = "Invalid User token" });
                        }
                    }
                    else
                    {
                        filterContext.Response = filterContext.Request.CreateResponse(HttpStatusCode.Unauthorized, new { data = "No JwtToken found" });
                    }
                }
            }
        }
    }

    public class UnityActionFilterProvider : ActionDescriptorFilterProvider, IFilterProvider
    {
        private readonly IUnityContainer container;

        public UnityActionFilterProvider(IUnityContainer container)
        {
            this.container = container;
        }

        public new IEnumerable<FilterInfo> GetFilters(HttpConfiguration configuration, HttpActionDescriptor actionDescriptor)
        {
            var filters = base.GetFilters(configuration, actionDescriptor);

            foreach (var filter in filters)
            {
                container.BuildUp(filter.Instance.GetType(), filter.Instance);
            }

            return filters;
        }
    }

    public class LoggingFilter : ActionFilterAttribute
    {
        private readonly IApiUsageLogsService _apiUsageLogsRepository;

        public LoggingFilter()
        {
            IApiUsageLogsService apiUsageLogsRepository = new ApiUsageLogsService();
            _apiUsageLogsRepository = apiUsageLogsRepository;
        }

        public LoggingFilter(IApiUsageLogsService apiUsageLogsRepository)
        {
            _apiUsageLogsRepository = apiUsageLogsRepository;
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            var notLogActions = new string[] { };
           /* var notLogActionsConfig = ConfigurationManager.AppSettings["NotLogActions"];*/ 
            var notLogActionsConfig = GlobalSettings.notLogActions;
            try
            {

                if (!string.IsNullOrWhiteSpace(notLogActionsConfig))
                    notLogActions = notLogActionsConfig.Split(',').ToArray();

                var actionDescriptor = actionExecutedContext.ActionContext.ActionDescriptor;
                var action = actionDescriptor.ActionName;

                if (notLogActions.Contains(action)) return;

                var controller = actionDescriptor.ControllerDescriptor.ControllerName;
                var url = actionExecutedContext.ActionContext.Request.RequestUri.AbsoluteUri;
                var bodyObj = (actionExecutedContext.ActionContext.Request.Content as ObjectContent);
                var body = string.Empty;

                if (bodyObj != null)
                    body = JsonConvert.SerializeObject(bodyObj.Value);
                else if (actionExecutedContext.ActionContext.ActionArguments.Any())
                {
                    body = JsonConvert.SerializeObject(actionExecutedContext.ActionContext.ActionArguments);
                }

                var resObj = actionExecutedContext.Response.Content as ObjectContent;
                var resBytes = actionExecutedContext.Response.Content as ByteArrayContent;
                var res = string.Empty;
                if (resObj != null)
                {
                    res = JsonConvert.SerializeObject(resObj.Value);
                }
                else if (resBytes != null)
                {
                    res = resBytes.ReadAsStringAsync().Result;
                }

                // log using api
                _apiUsageLogsRepository.Save(controller: controller, action: action, url: url, body: body, requestDate: DateTime.UtcNow, response: res);

            }

            catch (Exception ex)
            {
                var erMessage = string.Format("logging filter error [after]: {0}", ex.Message);
                //ErrorLog.LogError(new DivideByZeroException(erMessage));
            }
        }
    }


    public class DbContextTrackingFilter : ActionFilterAttribute
    {
        private const string RequestIdKey = "requestId";
        public DbContextTrackingFilter(bool useSingletonContext)
        {
            AutomationContextPool.UseSingletonContext(useSingletonContext);
        }
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            AutomationContextPool.TrackNewRequest();
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            AutomationContextPool.UntrackRequest();
        }
    }

    public class ElmahBasicAuthorizeAttribute : System.Web.Mvc.AuthorizeAttribute
    {
        private const string CookieName = "ElmahBasicAuthorization";

        public override void OnAuthorization(System.Web.Mvc.AuthorizationContext filterContext)
        {
            /*var userName = ConfigurationManager.AppSettings["ElmahBasicAuth.UserName"];*/ 
            var userName = GlobalSettings.elmahBasicAuthUserName;
            //var password = ConfigurationManager.AppSettings["ElmahBasicAuth.Password"];
            var password = GlobalSettings.elmahBasicAuthPassword;
           /* var elmah = ConfigurationManager.AppSettings["elmah.mvc.route"];*/ 
            var elmah = GlobalSettings.elmahmvcroute;

            if (string.IsNullOrWhiteSpace(userName) && string.IsNullOrWhiteSpace(password))
            {
                return;
            }
            else if (!filterContext.HttpContext.Request.RawUrl.StartsWith("/" + elmah, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            else
            {
                var authHeader = string.Empty;

                // see if the user already basic authorized
                var cookie = filterContext.HttpContext.Request.Cookies[CookieName];
                //if (cookie != null && !string.IsNullOrWhiteSpace(cookie.Value))
                if (!string.IsNullOrWhiteSpace(cookie?.Value))
                {
                    authHeader = cookie.Value;
                }
                else
                {
                    // See if they've supplied credentials
                    authHeader = filterContext.HttpContext.Request.Headers["Authorization"];
                }

                // decode basic auth
                if (!string.IsNullOrWhiteSpace(authHeader) && authHeader.StartsWith("Basic"))
                {
                    // Parse username and password out of the HTTP headers
                    var authHeaderDecoded = authHeader.Substring("Basic".Length).Trim();
                    var authHeaderBytes = Convert.FromBase64String(authHeaderDecoded);
                    authHeaderDecoded = Encoding.UTF7.GetString(authHeaderBytes);
                    var authUserName = authHeaderDecoded.Split(':')[0];
                    var authPassword = authHeaderDecoded.Split(':')[1];

                    // Validate login attempt
                    if (userName.Equals(authUserName, StringComparison.OrdinalIgnoreCase)
                        && password.Equals(authPassword))
                    {
                        cookie = new HttpCookie(CookieName) { Value = authHeader };
                        filterContext.HttpContext.Response.Cookies.Add(cookie);
                        return;
                    }
                }

                // Force the browser to pop up the login prompt
                filterContext.Result = new System.Web.Mvc.HttpStatusCodeResult(HttpStatusCode.Unauthorized);
                filterContext.HttpContext.Response.StatusCode = 401;
                filterContext.HttpContext.Response.AppendHeader("WWW-Authenticate", "Basic");

                // This gets shown if they click "Cancel" to the login prompt
                filterContext.HttpContext.Response.Write("You must log in to access this URL.");
            }
        }
    }
}
