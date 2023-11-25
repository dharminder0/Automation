using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QuizApp.Db
{
    public class QuizBrandingAndStyle
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("QuizDetails")]
        public int QuizId { get; set; }
        public virtual QuizDetails QuizDetails { get; set; }

        [StringLength(2000)]
        public string ImageFileURL { get; set; }
        public string PublicId { get; set; }

        [StringLength(50)]
        public string BackgroundColor { get; set; }

        [StringLength(50)]
        public string ButtonColor { get; set; }

        [StringLength(50)]
        public string OptionColor { get; set; }

        [StringLength(50)]
        public string ButtonFontColor { get; set; }

        [StringLength(50)]
        public string OptionFontColor { get; set; }

        [StringLength(50)]
        public string FontColor { get; set; }

        [StringLength(200)]
        public string FontType { get; set; }
        
        [StringLength(50)]
        public string ButtonHoverColor { get; set; }
        [StringLength(50)]
        public string ButtonHoverTextColor { get; set; }

        [StringLength(50)]
        public string BackgroundColorofSelectedAnswer { get; set; }

        [StringLength(50)]
        public string BackgroundColorofAnsweronHover { get; set; }

        [StringLength(50)]
        public string AnswerTextColorofSelectedAnswer { get; set; }

        public int IsBackType { get; set; }

        public string BackImageFileURL { get; set; }

        public string BackColor { get; set; }

        public string Opacity { get; set; }

        public string LogoUrl { get; set; }

        public string LogoPublicId { get; set; }

        public string BackgroundColorofLogo { get; set; }

        public string AutomationAlignment { get; set; }

        public string LogoAlignment { get; set; }

        public bool Flip { get; set; }

        public int? Language { get; set; }

        public int State { get; set; }

        public bool ApplyToAll { get; set; }

        public DateTime? LastUpdatedOn { get; set; }

        public int? LastUpdatedBy { get; set; }
    }
}