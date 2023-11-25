using Core.Common.Extensions;
using Newtonsoft.Json;
using QuizApp.Helpers;
using QuizApp.RepositoryPattern;
using QuizApp.Request;
using QuizApp.Response;
using QuizApp.Services.Model;
using QuizApp.Services.Service;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Http;
using Unity;
using Unity.WebApi;

namespace QuizApp.Cron {
    public class Program {
        public static readonly OWCHelper _owchelper = new OWCHelper();

        static readonly String ExceptionFormatString = "Exception message: {0}{1}Exception Source: {2}{1}Exception StackTrace: {3}{1}";
        static void Main(string[] args) {
            //IQuizService quizService = new QuizService();
            //quizService.PendingApiList();
            var container = new UnityContainer();
            RegisterContainer(container);

            GetReminderQueue(container);




        }

        static void RegisterContainer(UnityContainer container) {



            // register all your components with the container here
            // it is NOT necessary to register your controllers

            // e.g. container.RegisterType<ITestService, TestService>();
            container.RegisterType<IQuizVariablesService, QuizVariablesService>();
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
            container.RegisterType<IConfigurationDetailsLogService, ConfigurationDetailsLogService>();
            container.RegisterType<IQuizDuplicateService, QuizDuplicateService>();

            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);


        }

        static bool GetReminderQueue(UnityContainer container) {
            var quizService = container.Resolve<QuizService>();
            var reminderService = container.Resolve<ReminderSettingService>();
            quizService.PendingApiList();
            bool resultStatus = false;
            var listReminderQueue = reminderService.GetReminderQueue();
            foreach (var item in listReminderQueue) {
                try {
                    if (item.Type == 1) {
                        var obj = new CronRequest {
                            ToEmail = item.ToEmail,
                            Subject = item.Subject,
                            Body = item.Body,
                            Attachments = item.Attachments,
                            ClientCode = item.Company.ClientCode
                        };

                        var sendMailWithAttachmentUrl = GlobalSettings.webApiUrl.ToString() + "/api/v1/Cron/SendMailWithAttachment";
                        var apiSecretKey = GlobalSettings.apiSecret.ToString();
                        resultStatus = JsonConvert.DeserializeObject<bool>(OWCHelper.GetResponseWithApiSecret((sendMailWithAttachmentUrl), apiSecretKey, "POST", JsonConvert.SerializeObject(obj)));

                        if (resultStatus) {
                            item.Sent = resultStatus;
                            item.SentOn = DateTime.UtcNow;

                            reminderService.UpdateReminderQueueStatus(item);
                        }
                    }

                    if (item.Type == 2) {
                        resultStatus = CommunicationHelper.SendSMS(item.ToPhone, item.SMSText, item.Company);

                        if (resultStatus) {
                            item.Sent = resultStatus;
                            item.SentOn = DateTime.UtcNow;

                            reminderService.UpdateReminderQueueStatus(item);
                        }
                    }

                    if (item.Type == 3) {
                        var temp = new TempWorkpackagePush();
                        temp.leadUserInfo = new TempWorkpackagePush.ContactBasicDetails();
                        temp.companyObj = new CompanyModel();
                        temp.LeadUserId = item.WhatsappBody.LeadUserId;
                        temp.leadUserInfo.contactId = item.WhatsappBody.LeadUserId;
                        temp.leadUserInfo.LeadOwnerId = item.WhatsappBody.LeadOwnerId;
                        temp.leadUserInfo.ContactOwnerId = item.WhatsappBody.ContactOwnerId;
                        temp.leadUserInfo.SourceOwnerId = item.WhatsappBody.SourceOwnerId;
                        temp.companyObj.ClientCode = item.WhatsappBody.Clientcode;
                        temp.leadUserInfo = item.LeadUserInfo;

                        resultStatus = WhatsaAppCommunicationHelper.SendWhatsapp(item.WhatsappBody.ContactPhone, item.WhatsappBody.ObjectFields, item.WhatsappBody.StaticObjects, item.WhatsappBody.FollowUpMessage, item.WhatsappBody.MsgVariables, item.WhatsappBody.HsmTemplateId, item.WhatsappBody.LanguageCode, item.WhatsappBody.WorkPackageInfoId, null, item.WhatsappBody.Clientcode, null, temp);

                        if (resultStatus) {
                            item.Sent = resultStatus;
                            item.SentOn = DateTime.UtcNow;

                            reminderService.UpdateReminderQueueStatus(item);
                        }

                    }

                } catch (Exception ex) {
                    String exDetail = String.Format(ExceptionFormatString, ex.Message, Environment.NewLine, ex.Source, ex.StackTrace);
                    /*CommunicationHelper.SendMail(ConfigurationManager.AppSettings["NotifiyEmails"], "Cron Error - Failed for Id: " + item.Id.ToString(), exDetail);*/
                    CommunicationHelper.SendMail(GlobalSettings.notifiyEmails, "Cron Error - Failed for Id: " + item.Id.ToString(), exDetail);
                }
            }
            return true;
        }

    }
}



