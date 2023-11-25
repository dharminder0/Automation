using Elmah;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;

namespace QuizApp.Helpers
{
    public class ErrorLog
    {
        private static bool _cleanring = false;

        /// <summary>
        /// Log error to Elmah
        /// </summary>
        public static void LogError(Exception ex)
        {
            if (bool.Parse(ConfigurationManager.AppSettings["DisableErrorLog"]))
            {
                return;
            }

            try
            {
                Elmah.ErrorLog.GetDefault(HttpContext.Current).Log(new Error(ex));
                var current = GetCurrentDetails();
                if (!string.IsNullOrWhiteSpace(current))
                {
                    Elmah.ErrorLog.GetDefault(HttpContext.Current).Log(new Error(new DivideByZeroException(current)));
                }

                ClearOldLogs();
            }
            catch
            {
                // uh oh! just keep going
            }
        }

        public static void LogMessage(Exception ex)
        {
            if (bool.Parse(ConfigurationManager.AppSettings["DisableErrorLog"]))
            {
                return;
            }

            try
            {
                Elmah.ErrorLog.GetDefault(HttpContext.Current).Log(new Error(ex));
                if (ex.Message != null && !ex.Message.Contains("LogD"))
                {
                    var current = GetCurrentDetails();
                    if (!string.IsNullOrWhiteSpace(current))
                    {
                        Elmah.ErrorLog.GetDefault(HttpContext.Current).Log(new Error(new DivideByZeroException(current)));
                    }

                    ClearOldLogs();
                }
            }
            catch
            {
                // uh oh! just keep going
            }
        }

        private static string GetCurrentDetails()
        {
            var current = string.Empty;
            if (HttpContext.Current != null)
            {
                var req = HttpContext.Current.Request;
                var url = req.Url.AbsoluteUri;
                var referrer = req.UrlReferrer != null ? req.UrlReferrer.AbsoluteUri : string.Empty;
                var ip = req.RequestContext.HttpContext.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != null
                    ? req.RequestContext.HttpContext.Request["HTTP_X_FORWARDED_FOR"]
                    : req.RequestContext.HttpContext.Request["REMOTE_ADDR"];
                var json = new { url = url, referrer = referrer, ip = ip };
                current = JsonConvert.SerializeObject(json);
            }
            return current;
        }

        private static void ClearOldLogs()
        {
            if (_cleanring) return;
            _cleanring = true;
            ThreadPool.QueueUserWorkItem(state => {
                try
                {
                    // deleting old logs
                    var xmlLog = (Elmah.XmlFileErrorLog)Elmah.ErrorLog.GetDefault(HttpContext.Current);
                    var logPath = xmlLog.LogPath;
                    var directoryInfo = new DirectoryInfo(logPath);
                    if (directoryInfo.Exists)
                    {
                        var old = DateTime.UtcNow.AddDays(-10);
                        var fileSystemInfoArray = directoryInfo.GetFiles("error-*.xml");
                        foreach (var fileInfo in fileSystemInfoArray)
                        {
                            if (fileInfo.CreationTimeUtc < old && fileInfo.LastAccessTimeUtc < old && fileInfo.LastWriteTimeUtc < old)
                            {
                                fileInfo.Delete();
                            }
                        }
                    }
                }
                catch
                {
                    // uh oh! just keep going
                }
                finally
                {
                    _cleanring = false;
                }
            });
        }
    }
}