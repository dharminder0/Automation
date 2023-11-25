using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Helpers
{
    public enum ResultEnum
    {
        Ok = 1,
        Error = 2,
        OkWithMessage = 3,
    }

    public enum QuizTypeEnum
    {
        NPS = 1,
        Assessment = 2,
        Personality = 3,
        Score = 4,
        AssessmentTemplate = 5,
        ScoreTemplate = 6,
        PersonalityTemplate = 7
    }

    public enum CompanyAccessibleLevelEnum
    {
        ViewOnly = 1,
        EditOnly = 2,
        ViewExcept = 3,
        EditExcept = 4
    }

    public enum StatusEnum
    {
        Active = 1,
        Deleted = 2,
        Inactive = 3
    }

    public enum NotificationTypeEnum
    {
        RESULT = 1,
        INVITATION = 2,
        REMINDER = 3
    }

    public enum QuizStateEnum
    {
        DRAFTED = 1,
        PUBLISHED = 2
    }

    public enum BranchingLogicEnum
    {
        START = 1,
        QUESTION = 2,
        ANSWER = 3,
        RESULT = 4,
        RESULTNEXT = 5,
        CONTENT = 6,
        CONTENTNEXT = 7,
        ACTION = 8,
        ACTIONNEXT = 9,
        NONE = 10,
        BADGE = 11,
        BADGENEXT = 12,
        QUESTIONNEXT = 13,
        COVERDETAILS = 14,
        WHATSAPPTEMPLATE = 15,
        WHATSAPPTEMPLATEACTION = 16
    }

    public enum ImageTypeEnum
    {
        Title = 1,
        Description = 2
    }

    public enum WorkPackageStatusEnum
    {
        Pending = 1,
        Sent = 2
    }

    public enum QuizReportingAttributeEnum
    {
        Views = 1,
        Starts = 2,
        Completions = 3,
        Leads = 4,
        Conversion = 5,
        Sent = 6,
        Result = 7
    }

    public enum ReminderTypeEnum
    {
        EMAIL = 1,
        SMS = 2,
        WHATSAPP = 3
    }

    public enum BranchingLogicStartTypeEnum
    {
        QUESTION = 1,
        RESULT = 2,
        CONTENT = 3,
        Action = 4
    }

    public enum ActionTypeEnum
    {
        Appointment = 1,
        ReportEmail = 2,
        Automation = 3,
        LinkWithLeadDashboardAppointment = 4
    }

    public enum UserTypeEnum
    {
        Recruiter = 1,
        Lead = 2,
        Public = 3,
        JobRockAcademy = 4,
        TechnicalRecruiter = 5
    }

    public enum ModuleTypeEnum
    {
        Automation = 1,
        Appointment = 2,
        ELearning = 3,
        Canvas = 4,
        Vacancies = 5,
        Contacts = 6,
        Review = 7,
        Reporting = 8,
        Campaigns = 9
    }

    public enum QuizStatusEnum
    {
        Sent = 1,
        Started = 2,
        Completed = 3
    }
    public enum AnswerTypeEnum
    {
        Single = 1,
        Multiple = 2,
        Short = 3,
        Long = 4,
        DOB = 5,
        DrivingLicense = 6,
        FullAddress = 7,
        PostCode = 8,
        LookingforJobs = 9,
        NPS = 10,
        RatingEmoji = 11,
        RatingStarts = 12,
        Availability = 13,
        FirstName = 14, 
        LastName = 15, 
        Email = 16,
        PhoneNumber = 17,
        DatePicker = 18
    }

    public enum SubAnswerTypeEnum
    {
        PostCode = 1,
        HouseNumber = 2,
        Address = 3,
    }

    public enum OrderByEnum
    {
        Descending = 1,
        Ascending = 2
    }
    public enum BackTypeEnum
    {
        Image = 1,
        Color = 2
    }
    public enum LanguageTypeEnum
    {
        Dutch = 1,
        English = 2
    }
    public enum ListTypeEnum
    {
        Automation = 1,
        Elearning = 2,
        TechnicalRecruiter = 3,
        JobRockAcademy = 4,
        TechnicalRecruiteData = 5,
        AutomationforHub = 6,
        NotificationTemplate = 7,
        Report = 8
    }
    public enum UsageTypeEnum
    {
        Regular = 1,
        Chatbot = 2,
        WhatsAppChatbot = 3
    }

    public enum ChartViewTypeEnum
    {
        Day = 1,
        Week = 2,
        Month = 3,
        Year = 4
    }

    public enum FlowOrderTypeEnum
    {
        ResultThenForm = 1,
        ResultPlusForm = 2,
        FormThenResult = 3
    }

    public enum SignatureTypenum
    {
        individual = 1, //Personal
        group = 2 //Team
    }

    public enum AnswerStructureTypeEnum
    {
        Default = 0,
        List = 1,
        Button = 2
    }

    public enum QueueStatusTypes
    {
        New = 1,
        InProgress = 2,
        Sent = 3,
        Done = 4,
        Error = 5,
        Retry = 6
    }
    public class QueueItemTypes
    {
        public const string InsertQuizAttemptLead = "INSER_QUIZ_ATTEMPT_LEAD";
        public const string InsertSaveLeadTags = "INSERT_SAVE_LEAD_TAGS";
        public const string SendEmailSms = "SEND_EMAIL_SMS";
        public const string SendWhatsapp = "SEND_WHATSAPP";
    }
     
    public class EmojiQuestionOptionsEnglish {

        public const string RatingOne = "Very dissatisfied";
        public const string RatingTwo = "Dissatisfied";
        public const string RatingThree = "Neutral";
        public const string RatingFour = "Satisfied";
        public const string RatingFive = "Very satisfied";

    }
    public class StarQuestionOptionsEnglish {

        public const string RatingOne = "1 Star";
        public const string RatingTwo = "2 Star";
        public const string RatingThree = "3 Star";
        public const string RatingFour = "4 Star";
        public const string RatingFive = "5 Star";

    }

    public class StarQuestionOptionsDutch {

        public const string RatingOne = "Very dissatisfied";
        public const string RatingTwo = "Dissatisfied";
        public const string RatingThree = "Neutral";
        public const string RatingFour = "Satisfied";
        public const string RatingFive = "Very satisfied";

    }

    public class EmojiQuestionOptionsDutch {

        public const string RatingOne = "Zeer ontevreden";
        public const string RatingTwo = "Ontevreden";
        public const string RatingThree = "Neutraal";
        public const string RatingFour = "Tevreden";
        public const string RatingFive = "Zeer tevreden";

    }

    public class AvailableDateOptionsEnglish {

        public const string RatingOne = "Immediately";
        public const string RatingTwo = "Within 3 months";
        public const string RatingThree = "After 3 months";

    }

    public class AvailableDateOptionsDutch {

        public const string RatingOne = "Per direct";
        public const string RatingTwo = "Binnen 3 maanden";
        public const string RatingThree = "Na 3 maanden";

    }

    public enum QuizVariableObjectTypes
    {
        QUIZ = 1,
        QUESTION = 2,
        RESULT = 3,
        CONTENT = 4,
        COVER = 5
    }

    public enum AttemptStatus 
    {
        SENT = 1,
        INPROGRESS = 2,
        ATTEMPTED = 3,
        NOTCOMPLETED = 4
    }

    public enum TaskTypeEnum {
        Invitation = 1

	}


    public enum CommunicationTypeEnum {
        Whatsapp = 1,
        Email = 2,
        SMS = 3
    }
}