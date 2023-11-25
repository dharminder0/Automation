using Core.Common.Extensions;
using Newtonsoft.Json;
using NLog;
using QuizApp.Request;
using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace QuizApp.Helpers
{
    public class CommunicationHelper
    {
        public static NLog.Logger Logger = LogManager.GetCurrentClassLogger();
        public static bool SendMail(string EmailAddress, string Subject, string MailBody)
        {
            bool status = false;
            try
            {
                if (!string.IsNullOrEmpty(MailBody))
                    MailBody = MailBody.Replace("&lt;", "<").Replace("&gt;", " >").Replace("&nbsp;", " ");

                OWCEmailRequest emailRequest = new OWCEmailRequest();

                emailRequest.to = EmailAddress;
                emailRequest.subject = Subject;
                emailRequest.body = MailBody;

                //var communicationUrl = GlobalSettings.communicationApiUrl;
                var serviceUrl = GlobalSettings.communicationApiUrl + "/api/email/SendEmailMessage";

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", GlobalSettings.owcEmailCommunicationBearer.ToString());

                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.Timeout = new TimeSpan(0, 20, 0);

                    HttpResponseMessage result = null;
                    result = client.PostAsJsonAsync(serviceUrl, emailRequest).Result;

                    if (result != null && (result.IsSuccessStatusCode && result.Content != null))
                    {
                        var rslt = result.Content.ReadAsStringAsync().Result;

                        status = rslt == "true" ? true : false;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return status;
        }

        public static bool SendSMS(string PhoneNumber, string Message, CompanyModel Company)
        {
            bool status = false;
            try
            {
                OWCSMSRequest smsRequest = new OWCSMSRequest();

                if (!string.IsNullOrEmpty(Message))
                    Message = Message.Replace("<br/>", "\n");

                switch (Company.ClientCode.ToUpper())
                {
                    case "OLY":
                    case "OLY-ACC":
                        smsRequest.virtualNumber = GlobalSettings.owcSMSCommunicationVNforOLY.ToString();
                        break;                    
                    case "USG":
                    case "USG-TEST":
                    case "USG-PROD":
                        smsRequest.virtualNumber = GlobalSettings.owcSMSCommunicationVNforUSG.ToString();
                        break;
                    case "AT":
                    case "ATATAT":
                        smsRequest.virtualNumber = GlobalSettings.owcSMSCommunicationVNforAethone.ToString();
                        break;
                    default:
                        smsRequest.virtualNumber = GlobalSettings.owcSMSCommunicationVN.ToString();
                        break;
                }

                smsRequest.recipientNumber = PhoneNumber;
                smsRequest.message = Message;
                smsRequest.clientName = Company.CompanyName;
                smsRequest.clientCode = Company.ClientCode;

                //var communicationUrl = GlobalSettings.communicationApiUrl;
                var serviceUrl = GlobalSettings.communicationApiUrl + "/api/v1/sms/messages/send";
                var communicationBearer = GlobalSettings.owcSMSCommunicationBearer.ToString();

                try
                {
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("Authorization", communicationBearer);

                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        client.Timeout = new TimeSpan(0, 20, 0);

                        HttpResponseMessage result = null;
                        result = client.PostAsJsonAsync(serviceUrl, smsRequest).Result;

                        if (result != null && (result.IsSuccessStatusCode && result.Content != null))
                        {
                            var rslt = result.Content.ReadAsStringAsync().Result;

                            var resultDic = JsonConvert.DeserializeObject<Dictionary<string, object>>(rslt);

                            if (resultDic != null)
                                status = resultDic["status"].ToString().ToLower() == "sent";
                        }
                    }
                }
                catch (Exception ex)
                {
                    Services.Service.QuizService.AddPendingApi(serviceUrl, communicationBearer, JsonConvert.SerializeObject(smsRequest), "POST");
                }
            }
            catch (Exception ex)
            {
            }
            return status;
        }

        public static bool SendMailWithAttachment(string EmailAddress, string Subject, string MailBody, string ClientCode, List<Models.FileAttachment> FilePath = null)
        {
            bool status = false;
            try
            {
                if (!string.IsNullOrEmpty(MailBody))
                    MailBody = MailBody.Replace("&lt;", "<").Replace("&gt;", " >").Replace("&nbsp;", " ");
                //var communicationUrl = GlobalSettings.communicationApiUrl.ToString();
                var serviceUrl = GlobalSettings.communicationApiUrl.ToString() + "/api/email/Send";

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", GlobalSettings.owcEmailCommunicationBearer.ToString());
                    using (var content = new MultipartFormDataContent())
                    {
                        if (FilePath != null)
                        {
                            foreach (var file in FilePath)
                            {
                                try
                                {
                                    var blobFile = BlobStorageHelper.DownloadEmailAttachmentBlobFile(file.FileLink);
                                    content.Add(new StreamContent(new MemoryStream(blobFile.Item2)),"file", file.FileName);
                                }
                                catch (Exception ex)
                                {
                                }
                            }
                        }

                        content.Add(new StringContent(EmailAddress), "to");
                        content.Add(new StringContent(Subject), "subject");
                        content.Add(new StringContent(MailBody), "body");
                        content.Add(new StringContent(ClientCode), "clientCode");

                        try
                        {
                            var result = client.PostAsync(serviceUrl, content).Result;

                            if (result != null && (result.IsSuccessStatusCode && result.Content != null))
                            {
                                var rslt = result.Content.ReadAsStringAsync().Result;

                                if (rslt != null)
                                    status = rslt == "true" ? true : false;
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(LogLevel.Error, " error in SendMailWithAttachment Addpending api log done - " + EmailAddress + " Exception-" + ex.Message);
                            CronRequest request = new CronRequest()
                            {
                                ToEmail = EmailAddress,
                                Subject = Subject,
                                ClientCode = ClientCode,
                                Body = MailBody,
                                Attachments = FilePath
                            };
                           
                            var sendMailWithAttachmentUrl = GlobalSettings.webApiUrl.ToString() +"/api/v1/Cron/SendMailWithAttachment";
                            var apiSecretKey = GlobalSettings.apiSecret.ToString();
                            Services.Service.QuizService.AddPendingApi(sendMailWithAttachmentUrl, apiSecretKey, JsonConvert.SerializeObject(request), "POST");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "main error in SendMailWithAttachment done - " + EmailAddress + " Exception-" + ex.Message);
            }
            return status;
        }

        public static bool SendWhatsappMessage(WhatsappMessage Obj)
        {
            bool status = false;
            try
            {
                if (GlobalSettings.EnableWhatsApptempRedirection) {
                    if (Obj.ClientCode.EqualsCI("SFMine") || Obj.ClientCode.EqualsCI("AABHSNTEJH") || Obj.ClientCode.EqualsCI("AABHUX6DCI") || Obj.ClientCode.EqualsCI("SFMADEV")) {
                        Obj.ClientCode = "AABCGFVDCC";
                    }
                }

                //var serviceUrl = ConfigurationManager.AppSettings["OWCWhatsappCommunicationURL"];
                //var communicationUrl =  GlobalSettings.communicationApiUrl;
                var serviceUrl = GlobalSettings.communicationApiUrl + "/api/v1/whatsapp/send-work-package";
                var communicationBearer = GlobalSettings.owcEmailCommunicationBearer.ToString();
                Obj.ModuleName = ConfigurationManager.AppSettings["Module_Name"].ToString();

                try
                {
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("Authorization", GlobalSettings.owcEmailCommunicationBearer.ToString());

                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        client.Timeout = new TimeSpan(0, 20, 0);

                        HttpResponseMessage result = null;
                        result = client.PostAsJsonAsync(serviceUrl, Obj).Result;

                        if (result != null && (result.IsSuccessStatusCode && result.Content != null))
                        {
                            var rslt = result.Content.ReadAsStringAsync().Result;

                            if (rslt != null)
                                status = rslt.Contains("{\"success\":true") ? true : false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Services.Service.QuizService.AddPendingApi(serviceUrl, communicationBearer, JsonConvert.SerializeObject(Obj), "POST");
                }
            }
            catch (Exception ex)
            {
            }
            return status;
        }

        #region Old Method SendMailAsync
        //public static async Task<bool> SendMailAsync(string EmailAddress, string Subject, string MailBody)
        //{
        //    bool status = false;
        //    try
        //    {
        //        if (!string.IsNullOrEmpty(MailBody))
        //            MailBody = MailBody.Replace("&lt;", "<").Replace("&gt;", " >").Replace("&nbsp;", " ");

        //        OWCEmailRequest emailRequest = new OWCEmailRequest();

        //        emailRequest.to = EmailAddress;
        //        emailRequest.subject = Subject;
        //        emailRequest.body = MailBody;

        //        //var communicationUrl = GlobalSettings.communicationApiUrl;
        //        var serviceUrl = GlobalSettings.communicationApiUrl + "/api/email/SendEmailMessage";

        //        using (var client = new HttpClient())
        //        {
        //            client.DefaultRequestHeaders.Add("Authorization", GlobalSettings.owcEmailCommunicationBearer.ToString());

        //            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //            client.Timeout = new TimeSpan(0, 20, 0);

        //            HttpResponseMessage result = null;
        //            result = await client.PostAsJsonAsync(serviceUrl, emailRequest);
        //            if (result != null && (result.IsSuccessStatusCode && result.Content != null))
        //            {
        //                var rslt = result.Content.ReadAsStringAsync().Result;

        //                status = rslt == "true" ? true : false;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //    }

        //    return status;
        //}
        #endregion

        public static async Task<bool> SendSMSAsync(string PhoneNumber, string Message, CompanyModel Company)
        {
            bool status = false;
            try
            {
                OWCSMSRequest smsRequest = new OWCSMSRequest();

                if (!string.IsNullOrEmpty(Message))
                    Message = Message.Replace("<br/>", "\n");

                switch (Company.ClientCode.ToUpper())
                {
                    case "OLY":
                    case "OLY-ACC":
                        smsRequest.virtualNumber = GlobalSettings.owcSMSCommunicationVNforOLY.ToString();
                        break;
                    case "USG":
                    case "USG-TEST":
                    case "USG-PROD":
                        smsRequest.virtualNumber = GlobalSettings.owcSMSCommunicationVNforUSG.ToString();
                        break;
                    case "AT":
                    case "ATATAT":
                        smsRequest.virtualNumber = GlobalSettings.owcSMSCommunicationVNforAethone.ToString();
                        break;
                    default:
                        smsRequest.virtualNumber = GlobalSettings.owcSMSCommunicationVN.ToString();
                        break;
                }

                smsRequest.recipientNumber = PhoneNumber;
                smsRequest.message = Message;
                smsRequest.clientName = Company.CompanyName;
                smsRequest.clientCode = Company.ClientCode;

                //var communicationUrl = GlobalSettings.communicationApiUrl;
                var serviceUrl = GlobalSettings.communicationApiUrl + "/api/v1/sms/messages/send";
                var communicationBearer = GlobalSettings.owcSMSCommunicationBearer.ToString();

                try
                {
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("Authorization", communicationBearer);

                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        client.Timeout = new TimeSpan(0, 20, 0);

                        HttpResponseMessage result = null;
                        result = client.PostAsJsonAsync(serviceUrl, smsRequest).Result;

                        if (result != null && (result.IsSuccessStatusCode && result.Content != null))
                        {
                            var rslt = await result.Content.ReadAsStringAsync();

                            var resultDic = JsonConvert.DeserializeObject<Dictionary<string, object>>(rslt);

                            if (resultDic != null)
                                status = resultDic["status"].ToString().ToLower() == "sent";
                        }
                    }
                }
                catch (Exception ex)
                {
                    Services.Service.QuizService.AddPendingApi(serviceUrl, communicationBearer, JsonConvert.SerializeObject(smsRequest), "POST");
                }
            }
            catch (Exception ex)
            {
            }
            return status;
        }

        public static async Task<bool> SendMailWithAttachmentAsync(string EmailAddress, string Subject, string MailBody, string ClientCode, List<Models.FileAttachment> FilePath = null)
        {
            bool status = false;
            try
            {
                if (!string.IsNullOrEmpty(MailBody))
                    MailBody = MailBody.Replace("&lt;", "<").Replace("&gt;", " >").Replace("&nbsp;", " ");
                //var communicationUrl = GlobalSettings.communicationApiUrl.ToString();
                var serviceUrl = GlobalSettings.communicationApiUrl.ToString() + "/api/email/Send";

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", GlobalSettings.owcEmailCommunicationBearer.ToString());

                    using (var content = new MultipartFormDataContent())
                    {
                        if (FilePath != null)
                        {
                            foreach (var file in FilePath)
                            {
                                try {
                                    var blobFile = BlobStorageHelper.DownloadEmailAttachmentBlobFile(file.FileLink);
                                    content.Add(new StreamContent(new MemoryStream(blobFile.Item2)),"file", file.FileName);
                                }
                                catch (Exception ex)
                                {
                                }
                            }
                        }

                        content.Add(new StringContent(EmailAddress), "to");
                        content.Add(new StringContent(Subject), "subject");
                        content.Add(new StringContent(MailBody), "body");
                        content.Add(new StringContent(ClientCode), "clientCode");

                        try
                        {
                            var result = client.PostAsync(serviceUrl, content).Result;

                            if (result != null && (result.IsSuccessStatusCode && result.Content != null))
                            {
                                var rslt = await result.Content.ReadAsStringAsync();

                                if (rslt != null)
                                    status = rslt == "true" ? true : false;
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(LogLevel.Error, " error in SendMailWithAttachment Addpending api log done - " + EmailAddress + " Exception-" + ex.Message);
                            CronRequest request = new CronRequest()
                            {
                                ToEmail = EmailAddress,
                                Subject = Subject,
                                ClientCode = ClientCode,
                                Body = MailBody,
                                Attachments = FilePath
                            };

                            var sendMailWithAttachmentUrl = GlobalSettings.webApiUrl.ToString() + "/api/v1/Cron/SendMailWithAttachment";
                            var apiSecretKey = GlobalSettings.apiSecret.ToString();
                            Services.Service.QuizService.AddPendingApi(sendMailWithAttachmentUrl, apiSecretKey, JsonConvert.SerializeObject(request), "POST");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "main error in SendMailWithAttachment done - " + EmailAddress + " Exception-" + ex.Message);
            }
            return status;
        }

        public static async Task<bool> SendWhatsappMessageAsnc(WhatsappMessage Obj)
        {
            bool status = false;


            try
            {
                if (GlobalSettings.EnableWhatsApptempRedirection) {
                    if (Obj.ClientCode.EqualsCI("SFMine") || Obj.ClientCode.EqualsCI("AABHSNTEJH") || Obj.ClientCode.EqualsCI("AABHUX6DCI") || Obj.ClientCode.EqualsCI("SFMADEV")) {
                        Obj.ClientCode = "AABCGFVDCC";
                    }
                }
                //var serviceUrl = ConfigurationManager.AppSettings["OWCWhatsappCommunicationURL"];
                //var communicationUrl =  GlobalSettings.communicationApiUrl;
                var serviceUrl = GlobalSettings.communicationApiUrl + "/api/v1/whatsapp/send-work-package";
                var communicationBearer = GlobalSettings.owcEmailCommunicationBearer.ToString();
                Obj.ModuleName = ConfigurationManager.AppSettings["Module_Name"].ToString();

                try
                {
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("Authorization", GlobalSettings.owcEmailCommunicationBearer.ToString());

                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        client.Timeout = new TimeSpan(0, 20, 0);

                        HttpResponseMessage result = null;
                        result = await client.PostAsJsonAsync(serviceUrl, Obj);

                        if (result != null && (result.IsSuccessStatusCode && result.Content != null))
                        {
                            var rslt = result.Content.ReadAsStringAsync().Result;

                            if (rslt != null)
                                status = rslt == "{\"success\":true}" ? true : false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Services.Service.QuizService.AddPendingApi(serviceUrl, communicationBearer, JsonConvert.SerializeObject(Obj), "POST");
                }
            }
            catch (Exception ex)
            {
            }
            return status;
        }



        public static async Task<bool> SendAutomationChatbotStartAsnc(object Obj)
        {
            bool status = false;
            try
            {
                var serviceUrl = GlobalSettings.communicationApiUrl + "/api/v2/whatsapp/automation-chatbot/start";
                var communicationBearer = GlobalSettings.owcEmailCommunicationBearer.ToString();

                try
                {
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("Authorization", GlobalSettings.owcEmailCommunicationBearer.ToString());

                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        client.Timeout = new TimeSpan(0, 20, 0);

                        HttpResponseMessage result = null;
                        result = await client.PostAsJsonAsync(serviceUrl, Obj);

                        if (result != null && (result.IsSuccessStatusCode && result.Content != null))
                        {
                            status = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Services.Service.QuizService.AddPendingApi(serviceUrl, communicationBearer, JsonConvert.SerializeObject(Obj), "POST");
                }
            }
            catch (Exception ex)
            {
            }
            return status;
        }
    }
}
