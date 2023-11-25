using QuizApp.Services.Model;
using Swashbuckle.Examples;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Request
{
    public class ReminderSettingRequest
    {
        public string OfficeId { get; set; }
        public int? FirstReminder { get; set; }
        public int? SecondReminder { get; set; }
        public int? ThirdReminder { get; set; }

        public ReminderSetting MapRequestToEntity(ReminderSettingRequest reminderSettingRequestObj)
        {
            ReminderSetting reminderSetting = new ReminderSetting();

            reminderSetting.OfficeId = reminderSettingRequestObj.OfficeId;
            reminderSetting.FirstReminder = reminderSettingRequestObj.FirstReminder;
            reminderSetting.SecondReminder = reminderSettingRequestObj.SecondReminder;
            reminderSetting.ThirdReminder = reminderSettingRequestObj.ThirdReminder;

            return reminderSetting;
        }

        public class ReminderSettingRequestExample : IExamplesProvider
        {
            public object GetExamples()
            {
                return new ReminderSettingRequest
                {
                    OfficeId = string.Empty,
                    FirstReminder = 1,
                    SecondReminder = 2,
                    ThirdReminder = 3
                };
            }
        }
    }
}