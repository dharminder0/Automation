using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace QuizApp.Helpers
{
    public class NLogHelper
    {
        public static void InitializeNLog()
        {
            LogManager.Configuration = new NLog.Config.LoggingConfiguration();
            var config = LogManager.Configuration;
            FileTarget fileTarget = new FileTarget();
            config.AddTarget("LogFile", fileTarget);
            fileTarget.FileName = "${basedir}/Logs/" + DateTime.Now.ToShortDateString().Replace('/', '-') + ".txt";
            fileTarget.Layout = "[${longdate}] [${level}] [${message}]";
            LoggingRule rule2 = rule2 = new LoggingRule("*", LogLevel.Debug, fileTarget);

            config.LoggingRules.Add(rule2);
            LogManager.Configuration = config;
        }
    }
}