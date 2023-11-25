using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QuizApp.Db
{
    public class Quiz
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int QuizType { get; set; }

        public string AccessibleOfficeId { get; set; }

        public int? CompanyAccessibleLevel { get; set; }               

        public int State { get; set; }

        public string PublishedCode { get; set; }

        public bool QuesAndContentInSameTable { get; set; }

        [ForeignKey("Company")]
        public int? CompanyId { get; set; }
        public virtual Company Company { get; set; }

        [ForeignKey("Category")]
        public int? CategoryId { get; set; }
        public virtual Category Category { get; set; }       

        public virtual ICollection<UserAccessInQuiz> UserAccessInQuiz { get; set; }

        public virtual ICollection<QuizDetails> QuizDetails { get; set; }

        public virtual ICollection<NotificationTemplatesInQuiz> NotificationTemplatesInQuiz { get; set; }

        public virtual ICollection<WorkPackageInfo> WorkPackageInfo { get; set; }

        public virtual ICollection<AttachmentsInQuiz> AttachmentsInQuiz { get; set; }
        public virtual ICollection<UserPermissionsInQuiz> UserPermissionsInQuiz { get; set; }

        public virtual ICollection<QuizTagDetails> QuizTagDetails { get; set; }
        public virtual ICollection<ModulePermissionsInQuiz> ModulePermissionsInQuiz { get; set; }

        public virtual ICollection<FavoriteQuizByUser> FavoriteQuizByUser { get; set; }

        public virtual ICollection<ConfigurationDetails> ConfigurationDetails { get; set; }
        public virtual ICollection<UsageTypeInQuiz> UsageTypeInQuiz { get; set; }
        public bool? IsWhatsAppChatBotOldVersion { get; set; }
    }
}