﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QuizApp.Db
{
    public class NotificationTemplatesInQuiz
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("Quiz")]
        public int QuizId { get; set; }
        public virtual Quiz Quiz { get; set; }

        [ForeignKey("NotificationTemplate")]
        public int NotificationTemplateId { get; set; }
        public virtual NotificationTemplate NotificationTemplate { get; set; }
        public int? CompanyId { get; set; }
    }
}