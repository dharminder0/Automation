using QuizApp.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.RepositoryPattern
{
    public class AutomationContextPool
    {
        private const string RequestIdKey = "requestId";
        private static bool _useSingletonContext = false;

        private static QuizAppDBContext _context;
        private static object _locker = new object();

        private static Dictionary<string, QuizAppDBContext> _contextPool = new Dictionary<string, QuizAppDBContext>();

        /// <summary>
        /// Returns an application-lifetime context
        /// </summary>
        /// <returns></returns>
        private static QuizAppDBContext GetSingletonContext()
        {
            lock (_locker)
            {
                if (_context == null)
                    _context = new QuizAppDBContext();
                return _context;
            }
        }

        /// <summary>
        /// Returns a request-lifetime context
        /// </summary>
        /// <returns></returns>
        private static QuizAppDBContext GetScopedContext()
        {
            lock (_contextPool)
            {
                var requestId = HttpContext.Current?.Request?.Headers?.Get(RequestIdKey);
                if (requestId != null)
                {
                    return _contextPool[requestId];
                }
                //In case of background threads that do not reach the http request.
                return GetSingletonContext();
            }
        }

        public static void UseSingletonContext(bool useSingletonContext)
        {
            _useSingletonContext = useSingletonContext;
        }

        public static QuizAppDBContext GetContext()
        {
            if (_useSingletonContext)
                return GetSingletonContext();
            return GetScopedContext();
        }


        public static void TrackNewRequest()
        {
            if (_useSingletonContext)
                return;
            lock (_contextPool)
            {
                var requestId = Guid.NewGuid().ToString();
                HttpContext.Current.Request.Headers.Add(RequestIdKey, requestId);
                _contextPool.Add(requestId, new QuizAppDBContext());
            }
        }

        public static void UntrackRequest()
        {
            if (_useSingletonContext)
                return;
            lock (_contextPool)
            {
                var requestId = HttpContext.Current.Request.Headers.Get(RequestIdKey);
                if (_contextPool.ContainsKey(requestId))
                {
                    var context = _contextPool[requestId];
                    context.Dispose();
                    context = null;
                    _contextPool.Remove(requestId);
                }
            }
        }
    }


    public class AutomationUnitOfWork : IDisposable
    {
        private QuizAppDBContext context;

        public AutomationUnitOfWork()
        {
            context = AutomationContextPool.GetContext();

            context.Database.Log = s => {
                System.Diagnostics.Debug.WriteLine("----------------------------------------------------");
                System.Diagnostics.Debug.WriteLine(s);
                System.Diagnostics.Debug.WriteLine("----------------------------------------------------");
            };
        }

        //public GenericRepository<Tbl_EntityType> GetRepositoryInstance<Tbl_EntityType>() where Tbl_EntityType : class
        //{
        //    return new GenericRepository<Tbl_EntityType>(context);
        //}

        private GenericRepository<AnswerOptionsInQuizQuestions> answerOptionsInQuizQuestionsRepository;
        private GenericRepository<NotificationTemplate> notificationTemplateRepository;
        private GenericRepository<NotificationTemplatesInQuiz> notificationTemplatesInQuizRepository;
        private GenericRepository<QuestionsInQuiz> questionsInQuizRepository;
        private GenericRepository<Quiz> quizRepository;
        private GenericRepository<QuizDetails> quizDetailsRepository;
        private GenericRepository<BranchingLogic> branchingLogicRepository;
        private GenericRepository<QuizBrandingAndStyle> quizBrandingAndStyleRepository;
        private GenericRepository<QuizResults> quizResultsRepository;
        private GenericRepository<RemindersInQuiz> remindersInQuizRepository;
        private GenericRepository<ReminderQueues> reminderQueuesRepository;
        private GenericRepository<ResultSettings> resultSettingsRepository;
        private GenericRepository<UserAccessInQuiz> userAccessInQuizRepository;
        private GenericRepository<UserTokens> userTokensRepository;
        private GenericRepository<QuizAttempts> quizAttemptsRepository;
        private GenericRepository<QuizStats> quizStatsRepository;
        private GenericRepository<QuizQuestionStats> quizQuestionStatsRepository;
        private GenericRepository<WorkPackageInfo> workPackageInfoRepository;
        private GenericRepository<ContentsInQuiz> contentsInQuizRepository;
        private GenericRepository<ActionsInQuiz> actionsInQuizRepository;
        private GenericRepository<CoordinatesInBranchingLogic> coordinatesInBranchingLogicRepository;
        private GenericRepository<UserPermissionsInQuiz> userPermissionsInQuizRepository;
        private GenericRepository<BadgesInQuiz> badgesInQuizRepository;
        private GenericRepository<AttachmentsInQuiz> attachmentsInQuizRepository;
        private GenericRepository<QuizObjectStats> quizObjectStatsRepository;
        private GenericRepository<QuizAnswerStats> quizAnswerStatsRepository;
        private GenericRepository<Variables> variablesRepository;
        private GenericRepository<VariablesDetails> variablesDetailsRepository;
        private GenericRepository<VariableInQuiz> variableInQuizRepository;
        private GenericRepository<TagsInAnswer> tagsInAnswerRepository;
        private GenericRepository<Category> categoryRepository;
        private GenericRepository<PersonalityResultSetting> personalityResultSettingRepository;
        private GenericRepository<PersonalityAnswerResultMapping> personalityAnswerResultMappingRepository;
        private GenericRepository<PendingApiQueue> pendingApiQueueRepository;
        private GenericRepository<AttachmentsInNotificationTemplate> attachmentsInNotificationTemplateRepository;
        private GenericRepository<ModulePermissionsInQuiz> modulePermissionsInQuizRepository;
        private GenericRepository<LinkedCalendarInAction> linkedCalendarInActionRepository;
        private GenericRepository<LeadCalendarDataInAction> leadCalendarDataInActionRepository;
        private GenericRepository<FavoriteQuizByUser> favoriteQuizByUserRepository;
        private GenericRepository<ConfigurationDetails> configurationDetailsRepository;
        private GenericRepository<MediaVariablesDetails> mediaVariablesDetailsRepository;
        private GenericRepository<ResultIdsInConfigurationDetails> resultIdsInConfigurationDetailsRepository;
        private GenericRepository<AttachmentsInConfiguration> attachmentsInConfigurationRepository;
        private GenericRepository<QuizUrlSetting> quizUrlSettingRepository;
        private GenericRepository<ObjectFieldsInAnswer> objectFieldsInAnswerRepository;
        private GenericRepository<UsageTypeInQuiz> usageTypeInQuizRepository;
        private GenericRepository<TemplateParameterInConfigurationDetails> templateParameterInConfigurationDetailsRepository;
        private GenericRepository<ApiUsageLogs> apiUsageLogsRepository;
        private GenericRepository<QuizComponentLogs> quizComponentLogsRepository;
        private GenericRepository<LeadDataInAction> leadDataInActionRepository;
        private GenericRepository<Company> companyRepository;
        private GenericRepository<QuizTagDetails> quizTagDetailsRepository;
        private GenericRepository<AttemptQuizLog> attemptQuizLogRepository;
        private GenericRepository<ExternalActionQueue> externalActionQueueRepository;
        private GenericRepository<Languages> languagesQueueRepository;
        private GenericRepository<QuizVariables> quizVariablesRepository;
        private GenericRepository<WhatsappLogging> whatsappLoggingRepository;
        private GenericRepository<FieldSyncSetting> fieldSyncSettingRepository;


        public GenericRepository<AttemptQuizLog> AttemptQuizLogRepository
        {
            get
            {
                return this.attemptQuizLogRepository ?? new GenericRepository<AttemptQuizLog>(context);
            }
        }

        public GenericRepository<ExternalActionQueue> ExternalActionQueueRepository
        {
            get
            {
                return this.externalActionQueueRepository ?? new GenericRepository<ExternalActionQueue>(context);
            }
        }


        public GenericRepository<QuizTagDetails> QuizTagDetailsRepository
        {
            get
            {
                return this.quizTagDetailsRepository ?? new GenericRepository<QuizTagDetails>(context);
            }
        }

        public GenericRepository<Company> CompanyRepository
        {
            get
            {
                return this.companyRepository ?? new GenericRepository<Company>(context);
            }
        }

        public GenericRepository<LeadDataInAction> LeadDataInActionRepository
        {
            get
            {
                return this.leadDataInActionRepository ?? new GenericRepository<LeadDataInAction>(context);
            }
        }


        public GenericRepository<QuizComponentLogs> QuizComponentLogsRepository
        {
            get
            {
                return this.quizComponentLogsRepository ?? new GenericRepository<QuizComponentLogs>(context);
            }
        }


        public GenericRepository<ApiUsageLogs> ApiUsageLogsRepository
        {
            get
            {
                return this.apiUsageLogsRepository ?? new GenericRepository<ApiUsageLogs>(context);
            }
        }

        public GenericRepository<TemplateParameterInConfigurationDetails> TemplateParameterInConfigurationDetailsRepository
        {
            get
            {
                return this.templateParameterInConfigurationDetailsRepository ?? new GenericRepository<TemplateParameterInConfigurationDetails>(context);
            }
        }

        public GenericRepository<UsageTypeInQuiz> UsageTypeInQuizRepository
        {
            get
            {
                return this.usageTypeInQuizRepository ?? new GenericRepository<UsageTypeInQuiz>(context);
            }
        }

        public GenericRepository<ObjectFieldsInAnswer> ObjectFieldsInAnswerRepository
        {
            get
            {
                return this.objectFieldsInAnswerRepository ?? new GenericRepository<ObjectFieldsInAnswer>(context);
            }
        }

        public GenericRepository<QuizUrlSetting> QuizUrlSettingRepository
        {
            get
            {
                return this.quizUrlSettingRepository ?? new GenericRepository<QuizUrlSetting>(context);
            }
        }

        public GenericRepository<AttachmentsInConfiguration> AttachmentsInConfigurationRepository
        {
            get
            {
                return this.attachmentsInConfigurationRepository ?? new GenericRepository<AttachmentsInConfiguration>(context);
            }
        }

        public GenericRepository<ResultIdsInConfigurationDetails> ResultIdsInConfigurationDetailsRepository
        {
            get
            {
                return this.resultIdsInConfigurationDetailsRepository ?? new GenericRepository<ResultIdsInConfigurationDetails>(context);
            }
        }

        public GenericRepository<MediaVariablesDetails> MediaVariablesDetailsRepository
        {
            get
            {
                return this.mediaVariablesDetailsRepository ?? new GenericRepository<MediaVariablesDetails>(context);
            }
        }

        public GenericRepository<ConfigurationDetails> ConfigurationDetailsRepository
        {
            get
            {
                return this.configurationDetailsRepository ?? new GenericRepository<ConfigurationDetails>(context);
            }
        }

        public GenericRepository<FavoriteQuizByUser> FavoriteQuizByUserRepository
        {
            get
            {
                return this.favoriteQuizByUserRepository ?? new GenericRepository<FavoriteQuizByUser>(context);
            }
        }

        public GenericRepository<LeadCalendarDataInAction> LeadCalendarDataInActionRepository
        {
            get
            {
                return this.leadCalendarDataInActionRepository ?? new GenericRepository<LeadCalendarDataInAction>(context);
            }
        }

        public GenericRepository<LinkedCalendarInAction> LinkedCalendarInActionRepository
        {
            get
            {
                return this.linkedCalendarInActionRepository ?? new GenericRepository<LinkedCalendarInAction>(context);
            }
        }

        public GenericRepository<ModulePermissionsInQuiz> ModulePermissionsInQuizRepository
        {
            get
            {
                return this.modulePermissionsInQuizRepository ?? new GenericRepository<ModulePermissionsInQuiz>(context);
            }
        }

        public GenericRepository<AttachmentsInNotificationTemplate> AttachmentsInNotificationTemplateRepository
        {
            get
            {
                return this.attachmentsInNotificationTemplateRepository ?? new GenericRepository<AttachmentsInNotificationTemplate>(context);
            }
        }

        public GenericRepository<PendingApiQueue> PendingApiQueueRepository
        {
            get
            {
                return this.pendingApiQueueRepository ?? new GenericRepository<PendingApiQueue>(context);
            }
        }

        public GenericRepository<PersonalityAnswerResultMapping> PersonalityAnswerResultMappingRepository
        {
            get
            {
                return this.personalityAnswerResultMappingRepository ?? new GenericRepository<PersonalityAnswerResultMapping>(context);
            }
        }

        public GenericRepository<PersonalityResultSetting> PersonalityResultSettingRepository
        {
            get
            {
                return this.personalityResultSettingRepository ?? new GenericRepository<PersonalityResultSetting>(context);
            }
        }

        public GenericRepository<Category> CategoryRepository
        {
            get
            {
                return this.categoryRepository ?? new GenericRepository<Category>(context);
            }
        }

        public GenericRepository<TagsInAnswer> TagsInAnswerRepository
        {
            get
            {
                return this.tagsInAnswerRepository ?? new GenericRepository<TagsInAnswer>(context);
            }
        }

        public GenericRepository<VariableInQuiz> VariableInQuizRepository
        {
            get
            {
                return this.variableInQuizRepository ?? new GenericRepository<VariableInQuiz>(context);
            }
        }

        public GenericRepository<VariablesDetails> VariablesDetailsRepository
        {
            get
            {
                return this.variablesDetailsRepository ?? new GenericRepository<VariablesDetails>(context);
            }
        }

        public GenericRepository<Variables> VariablesRepository
        {
            get
            {
                return this.variablesRepository ?? new GenericRepository<Variables>(context);
            }
        }

        public GenericRepository<QuizAnswerStats> QuizAnswerStatsRepository
        {
            get
            {
                return this.quizAnswerStatsRepository ?? new GenericRepository<QuizAnswerStats>(context);
            }
        }

        public GenericRepository<QuizObjectStats> QuizObjectStatsRepository
        {
            get
            {
                return this.quizObjectStatsRepository ?? new GenericRepository<QuizObjectStats>(context);
            }
        }

        public GenericRepository<AttachmentsInQuiz> AttachmentsInQuizRepository
        {
            get
            {
                return this.attachmentsInQuizRepository ?? new GenericRepository<AttachmentsInQuiz>(context);
            }
        }

        public GenericRepository<BadgesInQuiz> BadgesInQuizRepository
        {
            get
            {
                return this.badgesInQuizRepository ?? new GenericRepository<BadgesInQuiz>(context);
            }
        }


        public GenericRepository<UserPermissionsInQuiz> UserPermissionsInQuizRepository
        {
            get
            {
                return this.userPermissionsInQuizRepository ?? new GenericRepository<UserPermissionsInQuiz>(context);
            }
        }

        public GenericRepository<CoordinatesInBranchingLogic> CoordinatesInBranchingLogicRepository
        {
            get
            {
                return this.coordinatesInBranchingLogicRepository ?? new GenericRepository<CoordinatesInBranchingLogic>(context);
            }
        }

        public GenericRepository<ActionsInQuiz> ActionsInQuizRepository
        {
            get
            {
                return this.actionsInQuizRepository ?? new GenericRepository<ActionsInQuiz>(context);
            }
        }

        public GenericRepository<ContentsInQuiz> ContentsInQuizRepository
        {
            get
            {
                return this.contentsInQuizRepository ?? new GenericRepository<ContentsInQuiz>(context);
            }
        }

        public GenericRepository<WorkPackageInfo> WorkPackageInfoRepository
        {
            get
            {
                return this.workPackageInfoRepository ?? new GenericRepository<WorkPackageInfo>(context);
            }
        }

        public GenericRepository<QuizQuestionStats> QuizQuestionStatsRepository
        {
            get
            {
                return this.quizQuestionStatsRepository ?? new GenericRepository<QuizQuestionStats>(context);
            }
        }

        public GenericRepository<QuizStats> QuizStatsRepository
        {
            get
            {
                return this.quizStatsRepository ?? new GenericRepository<QuizStats>(context);
            }
        }

        public GenericRepository<QuizAttempts> QuizAttemptsRepository
        {
            get
            {
                return this.quizAttemptsRepository ?? new GenericRepository<QuizAttempts>(context);
            }
        }

        public GenericRepository<UserTokens> UserTokensRepository
        {
            get
            {
                return this.userTokensRepository ?? new GenericRepository<UserTokens>(context);
            }
        }

        public GenericRepository<UserAccessInQuiz> UserAccessInQuizRepository
        {
            get
            {
                return this.userAccessInQuizRepository ?? new GenericRepository<UserAccessInQuiz>(context);
            }
        }

        public GenericRepository<ResultSettings> ResultSettingsRepository
        {
            get
            {
                return this.resultSettingsRepository ?? new GenericRepository<ResultSettings>(context);
            }
        }

        public GenericRepository<ReminderQueues> ReminderQueuesRepository
        {
            get
            {
                return this.reminderQueuesRepository ?? new GenericRepository<ReminderQueues>(context);
            }
        }

        public GenericRepository<RemindersInQuiz> RemindersInQuizRepository
        {
            get
            {
                return this.remindersInQuizRepository ?? new GenericRepository<RemindersInQuiz>(context);
            }
        }

        public GenericRepository<QuizResults> QuizResultsRepository
        {
            get
            {
                return this.quizResultsRepository ?? new GenericRepository<QuizResults>(context);
            }
        }

        public GenericRepository<QuizBrandingAndStyle> QuizBrandingAndStyleRepository
        {
            get
            {
                return this.quizBrandingAndStyleRepository ?? new GenericRepository<QuizBrandingAndStyle>(context);
            }
        }

        public GenericRepository<BranchingLogic> BranchingLogicRepository
        {
            get
            {
                return this.branchingLogicRepository ?? new GenericRepository<BranchingLogic>(context);
            }
        }

        public GenericRepository<QuizDetails> QuizDetailsRepository
        {
            get
            {
                return this.quizDetailsRepository ?? new GenericRepository<QuizDetails>(context);
            }
        }

        public GenericRepository<Quiz> QuizRepository
        {
            get
            {
                return this.quizRepository ?? new GenericRepository<Quiz>(context);
            }
        }

        public GenericRepository<QuestionsInQuiz> QuestionsInQuizRepository
        {
            get
            {
                return this.questionsInQuizRepository ?? new GenericRepository<QuestionsInQuiz>(context);
            }
        }

        public GenericRepository<NotificationTemplatesInQuiz> NotificationTemplatesInQuizRepository
        {
            get
            {
                return this.notificationTemplatesInQuizRepository ?? new GenericRepository<NotificationTemplatesInQuiz>(context);
            }
        }

        public GenericRepository<NotificationTemplate> NotificationTemplateRepository
        {
            get
            {
                return this.notificationTemplateRepository ?? new GenericRepository<NotificationTemplate>(context);
            }
        }

        public GenericRepository<WhatsappLogging> WhatsappLoggingRepository {
            get {
                return this.whatsappLoggingRepository ?? new GenericRepository<WhatsappLogging>(context);
            }
        }
        public void Save()
        {
            try
            {
                context.SaveChanges();
            }
            catch
            {
                throw;
            }
        }

        public GenericRepository<AnswerOptionsInQuizQuestions> AnswerOptionsInQuizQuestionsRepository
        {
            get
            {
                return this.answerOptionsInQuizQuestionsRepository ?? new GenericRepository<AnswerOptionsInQuizQuestions>(context);
            }
        }

        public GenericRepository<Languages> LanguagesRepository
        {
            get
            {
                return this.languagesQueueRepository ?? new GenericRepository<Languages>(context);
            }
        }
        
        public GenericRepository<QuizVariables> QuizVariablesRepository
        {
            get
            {
                return this.quizVariablesRepository ?? new GenericRepository<QuizVariables>(context);
            }
        }

        public GenericRepository<FieldSyncSetting> FieldSyncSettingRepository {
            get {
                return this.fieldSyncSettingRepository ?? new GenericRepository<FieldSyncSetting>(context);
            }
        }



        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context = null;
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


    }
}
