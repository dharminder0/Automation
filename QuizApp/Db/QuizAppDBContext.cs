namespace QuizApp.Db
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Linq;

    public class QuizAppDBContext : DbContext
    {
        // Your context has been configured to use a 'QuizAppDBContext' connection string from your application's 
        // configuration file (App.config or Web.config). By default, this connection string targets the 
        // 'QuizApp.Db.QuizAppDBContext' database on your LocalDb instance. 
        // 
        // If you wish to target a different database and/or database provider, modify the 'QuizAppDBContext' 
        // connection string in the application configuration file.
        public QuizAppDBContext()
            : base("name=QuizAppDBContext")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Entity<AnswerOptionsInQuizQuestions>()
                .HasMany(e => e.PersonalityAnswerResultMapping)
                .WithRequired(e => e.AnswerOptionsInQuizQuestions)
                .HasForeignKey(e => e.AnswerId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<QuizResults>()
                .HasMany(e => e.PersonalityAnswerResultMapping)
                .WithRequired(e => e.QuizResults)
                .HasForeignKey(e => e.ResultId)
                .WillCascadeOnDelete(false);
        }

        // Add a DbSet for each entity type that you want to include in your model. For more information 
        // on configuring and using a Code First model, see http://go.microsoft.com/fwlink/?LinkId=390109.

        public virtual DbSet<AnswerOptionsInQuizQuestions> AnswerOptionsInQuizQuestions { get; set; }
        public virtual DbSet<NotificationTemplate> NotificationTemplate { get; set; }
        public virtual DbSet<NotificationTemplatesInQuiz> NotificationTemplatesInQuiz { get; set; }
        public virtual DbSet<QuestionsInQuiz> QuestionsInQuiz { get; set; }
        public virtual DbSet<Quiz> Quiz { get; set; }
        public virtual DbSet<QuizDetails> QuizDetails { get; set; }
        public virtual DbSet<BranchingLogic> BranchingLogic { get; set; }
        public virtual DbSet<QuizBrandingAndStyle> QuizBrandingAndStyle { get; set; }
        public virtual DbSet<QuizResults> QuizResults { get; set; }
        public virtual DbSet<RemindersInQuiz> RemindersInQuiz { get; set; }
        public virtual DbSet<ReminderQueues> ReminderQueues { get; set; }
        public virtual DbSet<ResultSettings> ResultSettings { get; set; }
        public virtual DbSet<UserAccessInQuiz> UserAccessInQuiz { get; set; }
        public virtual DbSet<UserTokens> UserTokens { get; set; }
        public virtual DbSet<QuizAttempts> QuizAttempts { get; set; }
        public virtual DbSet<QuizStats> QuizStats { get; set; }
        public virtual DbSet<QuizQuestionStats> QuizQuestionStats { get; set; }
        public virtual DbSet<WorkPackageInfo> WorkPackageInfo { get; set; }
        public virtual DbSet<ContentsInQuiz> ContentsInQuiz { get; set; }
        public virtual DbSet<ActionsInQuiz> ActionsInQuiz { get; set; }
        public virtual DbSet<CoordinatesInBranchingLogic> CoordinatesInBranchingLogic { get; set; }
        public virtual DbSet<UserPermissionsInQuiz> UserPermissionsInQuiz { get; set; }
        public virtual DbSet<BadgesInQuiz> BadgesInQuiz { get; set; }
        public virtual DbSet<AttachmentsInQuiz> AttachmentsInQuiz { get; set; }
        public virtual DbSet<QuizObjectStats> QuizObjectStats { get; set; }
        public virtual DbSet<QuizAnswerStats> QuizAnswerStats { get; set; }
        public virtual DbSet<Variables> Variables { get; set; }
        public virtual DbSet<VariablesDetails> VariablesDetails { get; set; }
        public virtual DbSet<VariableInQuiz> VariableInQuiz { get; set; }
        public virtual DbSet<TagsInAnswer> TagsInAnswer { get; set; }
        public virtual DbSet<Category> Category { get; set; }
        public virtual DbSet<PersonalityResultSetting> PersonalityResultSetting { get; set; }
        public virtual DbSet<PersonalityAnswerResultMapping> PersonalityAnswerResultMapping { get; set; }
        public virtual DbSet<PendingApiQueue> PendingApiQueue { get; set; }
        public virtual DbSet<AttachmentsInNotificationTemplate> AttachmentsInNotificationTemplate { get; set; }
        public virtual DbSet<ModulePermissionsInQuiz> ModulePermissionsInQuiz { get; set; }
        public virtual DbSet<LinkedCalendarInAction> LinkedCalendarInAction { get; set; }
        public virtual DbSet<LeadCalendarDataInAction> LeadCalendarDataInAction { get; set; }
        public virtual DbSet<FavoriteQuizByUser> FavoriteQuizByUser { get; set; }
        public virtual DbSet<ConfigurationDetails> ConfigurationDetails { get; set; }
        public virtual DbSet<MediaVariablesDetails> MediaVariablesDetails { get; set; }
        public virtual DbSet<ResultIdsInConfigurationDetails> ResultIdsInConfigurationDetails { get; set; }
        public virtual DbSet<AttachmentsInConfiguration> AttachmentsInConfiguration { get; set; }
        public virtual DbSet<QuizUrlSetting> QuizUrlSetting { get; set; }
        public virtual DbSet<ObjectFieldsInAnswer> ObjectFieldsInAnswer { get; set; }
        public virtual DbSet<UsageTypeInQuiz> UsageTypeInQuiz { get; set; }
        public virtual DbSet<TemplateParameterInConfigurationDetails> TemplateParameterInConfigurationDetails { get; set; }
        public virtual DbSet<ApiUsageLogs> ApiUsageLogs { get; set; }
        public virtual DbSet<AttemptQuizLog> AttemptQuizLog { get; set; }
        public virtual DbSet<ExternalActionQueue> ExternalActionQueue { get; set; }
        public virtual DbSet<Languages> Languages { get; set; }
        public virtual DbSet<QuizVariables> QuizVariables { get; set; }
        public virtual DbSet<WhatsappLogging> WhatsappLogging { get; set; }
        public virtual DbSet<FieldSyncSetting> FieldSyncSettings { get; set; }
    }
}