using QuizApp.Controllers;
using QuizApp.Services.Service;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Filters;
using Unity;
using Unity.WebApi;

namespace QuizApp
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
            var container = new UnityContainer();

            // register all your components with the container here
            // it is NOT necessary to register your controllers

            // e.g. container.RegisterType<ITestService, TestService>();

            container.RegisterType<IBusinessUserService, BusinessUserService>();
            container.RegisterType<IQuizService, QuizService>();
            container.RegisterType<INotificationTemplateService, NotificationTemplateService>();
            container.RegisterType<IReminderSettingService, ReminderSettingService>();
            container.RegisterType<IReportingService, ReportingService>();
            container.RegisterType<IWorkPackageService, WorkPackageService>();
            container.RegisterType<ICourseService, CourseService>();
            container.RegisterType<ITemplateService, TemplateService>();
            container.RegisterType<IApiUsageLogsService, ApiUsageLogsService>();
            container.RegisterType<IAutomationService, AutomationService>();
            container.RegisterType<IGenericAutomationService, GenericAutomationService>();
            container.RegisterType<IQuizAttemptService, QuizAttemptService>();
            container.RegisterType<IQuizListService, QuizListService>();
            container.RegisterType<IBranchingLogicService, BranchingLogicService>();
            container.RegisterType<IQuizCoverService, QuizCoverService>();
            container.RegisterType<IUpdateBrandingService, UpdateBrandingService>();
            container.RegisterType<IWhatsAppService, WhatsAppService>();
            container.RegisterType<IConfigurationDetailsLogService, ConfigurationDetailsLogService>();
            container.RegisterType<IQuizDuplicateService, QuizDuplicateService>();
            container.RegisterType<IPublishQuizService, PublishQuizService>();
            container.RegisterType<IQuizVariablesService, QuizVariablesService>();
            container.RegisterType<IQuestionService, QuestionService>();
            container.RegisterType<IAutomationDetailsService, AutomationDetailsService>();
            container.RegisterType<ICommunicationService, CommunicationService>();
            container.RegisterType<IFieldSettingService, FieldSettingService>();
            container.RegisterType<IContactService, ContactService>();
            container.RegisterType<IUncompleteQuizService, UncompleteQuizService>();


            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);

            var providers = GlobalConfiguration.Configuration.Services.GetFilterProviders().ToList();

            GlobalConfiguration.Configuration.Services.Add(typeof(System.Web.Http.Filters.IFilterProvider),
                                                            new UnityActionFilterProvider(container));

            var defaultprovider = providers.First(p => p is ActionDescriptorFilterProvider);

            GlobalConfiguration.Configuration.Services.Remove(typeof(System.Web.Http.Filters.IFilterProvider), defaultprovider);

            //GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
        }
    }
}