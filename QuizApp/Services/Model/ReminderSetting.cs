using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Services.Model
{
    public class ReminderSetting : Base
    {
        public string OfficeId { get; set; }
        public int? FirstReminder { get; set; }
        public int? SecondReminder { get; set; }
        public int? ThirdReminder { get; set; }
    }
}