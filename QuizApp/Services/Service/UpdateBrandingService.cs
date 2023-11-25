using Core.Common.Caching;
using QuizApp.Helpers;
using QuizApp.RepositoryPattern;
using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace QuizApp.Services.Service
{
    public partial class UpdateBrandingService : IUpdateBrandingService
    {
        private ResultEnum status = ResultEnum.Ok;
        private string errormessage = string.Empty;
        private string leadInfoUpdateJson = "{'ContactId': '{ContactId}','ClientCode': '{ClientCode}','campaignName': '{campaignName}','appointmentStatus': '{appointmentStatus}','appointmentDate': '{appointmentDate}','appointmentTypeId': {appointmentTypeId},'appointmentTypeTitle': '{appointmentTypeTitle}','calendarId': {calendarId},'calendarTitle': '{calendarTitle}','appointmentBookedDate': '{appointmentBookedDate}','UserToken': '{UserToken}','SourceId': '{SourceId}'}";
        private string badgesInfoUpdateJson = "[ {'UserId': {UserId},'CourseId': '{CourseId}','CourseBadgeName': '{CourseBadgeName}','CourseBadgeImageUrl': '{CourseBadgeImageUrl}','CourseTitle': '{CourseTitle}'}]";

        public ResultEnum Status { get { return status; } set { status = value; } }
        public string ErrorMessage { get { return errormessage; } set { errormessage = value; } }

        public void UpdateBrandingAndStyle(QuizBrandingAndStyleModel BrandingAndStyleObj, int BusinessUserId, int CompanyId)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizObj = UOWObj.QuizRepository.GetByID(BrandingAndStyleObj.QuizId);

                    if (quizObj != null)
                    {
                     
                        var quizDetailsObj = quizObj.QuizDetails.FirstOrDefault(r => r.State == (int)QuizStateEnum.DRAFTED);

                        if (quizDetailsObj != null)
                        {
                            var brandingAndStyle = quizDetailsObj.QuizBrandingAndStyle.FirstOrDefault();

                            if (brandingAndStyle != null)
                            {
                                brandingAndStyle.ImageFileURL = BrandingAndStyleObj.ImageFileURL;
                                brandingAndStyle.PublicId = BrandingAndStyleObj.PublicIdForFileURL;
                                brandingAndStyle.BackgroundColor = BrandingAndStyleObj.BackgroundColor;
                                brandingAndStyle.ButtonColor = BrandingAndStyleObj.ButtonColor;
                                brandingAndStyle.OptionColor = BrandingAndStyleObj.OptionColor;
                                brandingAndStyle.ButtonFontColor = BrandingAndStyleObj.ButtonFontColor;
                                brandingAndStyle.OptionFontColor = BrandingAndStyleObj.OptionFontColor;
                                brandingAndStyle.FontColor = BrandingAndStyleObj.FontColor;
                                brandingAndStyle.ButtonHoverColor = BrandingAndStyleObj.ButtonHoverColor;
                                brandingAndStyle.ButtonHoverTextColor = BrandingAndStyleObj.ButtonHoverTextColor;
                                brandingAndStyle.BackgroundColorofSelectedAnswer = BrandingAndStyleObj.BackgroundColorofSelectedAnswer;
                                brandingAndStyle.BackgroundColorofAnsweronHover = BrandingAndStyleObj.BackgroundColorofAnsweronHover;
                                brandingAndStyle.AnswerTextColorofSelectedAnswer = BrandingAndStyleObj.AnswerTextColorofSelectedAnswer;
                                brandingAndStyle.FontType = BrandingAndStyleObj.FontType;
                                brandingAndStyle.ApplyToAll = BrandingAndStyleObj.ApplyToAll;
                                brandingAndStyle.LogoUrl = BrandingAndStyleObj.LogoUrl;
                                brandingAndStyle.LogoPublicId = BrandingAndStyleObj.LogoPublicId;
                                brandingAndStyle.BackgroundColorofLogo = BrandingAndStyleObj.BackgroundColorofLogo;
                                brandingAndStyle.AutomationAlignment = BrandingAndStyleObj.AutomationAlignment;
                                brandingAndStyle.LogoAlignment = BrandingAndStyleObj.LogoAlignment;
                                brandingAndStyle.Flip = BrandingAndStyleObj.Flip;
                                brandingAndStyle.Language = BrandingAndStyleObj.Language.HasValue ? BrandingAndStyleObj.Language : 1;

                                if (BrandingAndStyleObj.IsBackType == (int)BackTypeEnum.Image)
                                {
                                    brandingAndStyle.IsBackType = (int)BackTypeEnum.Image;
                                    brandingAndStyle.BackImageFileURL = BrandingAndStyleObj.BackImageFileURL;
                                    brandingAndStyle.Opacity = BrandingAndStyleObj.Opacity;
                                }
                                else if (BrandingAndStyleObj.IsBackType == (int)BackTypeEnum.Color)
                                {
                                    brandingAndStyle.IsBackType = (int)BackTypeEnum.Color;
                                    brandingAndStyle.BackColor = BrandingAndStyleObj.BackColor;
                                    brandingAndStyle.Opacity = BrandingAndStyleObj.Opacity;
                                }
                                brandingAndStyle.LastUpdatedBy = BusinessUserId;
                                brandingAndStyle.LastUpdatedOn = DateTime.UtcNow;

                                UOWObj.QuizBrandingAndStyleRepository.Update(brandingAndStyle);
                            }
                            else
                            {
                                brandingAndStyle = new Db.QuizBrandingAndStyle();

                                brandingAndStyle.QuizId = quizDetailsObj.Id;
                                brandingAndStyle.ImageFileURL = BrandingAndStyleObj.ImageFileURL;
                                brandingAndStyle.PublicId = BrandingAndStyleObj.PublicIdForFileURL;
                                brandingAndStyle.BackgroundColor = BrandingAndStyleObj.BackgroundColor;
                                brandingAndStyle.ButtonColor = BrandingAndStyleObj.ButtonColor;
                                brandingAndStyle.OptionColor = BrandingAndStyleObj.OptionColor;
                                brandingAndStyle.ButtonFontColor = BrandingAndStyleObj.ButtonFontColor;
                                brandingAndStyle.OptionFontColor = BrandingAndStyleObj.OptionFontColor;
                                brandingAndStyle.FontColor = BrandingAndStyleObj.FontColor;
                                brandingAndStyle.FontType = BrandingAndStyleObj.FontType;
                                brandingAndStyle.ButtonHoverColor = BrandingAndStyleObj.ButtonHoverColor;
                                brandingAndStyle.ButtonHoverTextColor = BrandingAndStyleObj.ButtonHoverTextColor;
                                brandingAndStyle.BackgroundColorofSelectedAnswer = BrandingAndStyleObj.BackgroundColorofSelectedAnswer;
                                brandingAndStyle.BackgroundColorofAnsweronHover = BrandingAndStyleObj.BackgroundColorofAnsweronHover;
                                brandingAndStyle.AnswerTextColorofSelectedAnswer = BrandingAndStyleObj.AnswerTextColorofSelectedAnswer;
                                brandingAndStyle.ApplyToAll = BrandingAndStyleObj.ApplyToAll;
                                brandingAndStyle.LogoUrl = BrandingAndStyleObj.LogoUrl;
                                brandingAndStyle.LogoPublicId = BrandingAndStyleObj.LogoPublicId;
                                brandingAndStyle.BackgroundColorofLogo = BrandingAndStyleObj.BackgroundColorofLogo;
                                brandingAndStyle.AutomationAlignment = BrandingAndStyleObj.AutomationAlignment;
                                brandingAndStyle.LogoAlignment = BrandingAndStyleObj.LogoAlignment;
                                brandingAndStyle.Flip = BrandingAndStyleObj.Flip;
                                brandingAndStyle.Language = BrandingAndStyleObj.Language.HasValue ? BrandingAndStyleObj.Language : 1;

                                if (BrandingAndStyleObj.IsBackType == (int)BackTypeEnum.Image)
                                {
                                    brandingAndStyle.IsBackType = (int)BackTypeEnum.Image;
                                    brandingAndStyle.BackImageFileURL = BrandingAndStyleObj.BackImageFileURL;
                                    brandingAndStyle.Opacity = BrandingAndStyleObj.Opacity;
                                }
                                else if (BrandingAndStyleObj.IsBackType == (int)BackTypeEnum.Color)
                                {
                                    brandingAndStyle.IsBackType = (int)BackTypeEnum.Color;
                                    brandingAndStyle.BackColor = BrandingAndStyleObj.BackColor;
                                    brandingAndStyle.Opacity = BrandingAndStyleObj.Opacity;
                                }
                                brandingAndStyle.LastUpdatedBy = BusinessUserId;
                                brandingAndStyle.LastUpdatedOn = DateTime.UtcNow;

                                UOWObj.QuizBrandingAndStyleRepository.Insert(brandingAndStyle);
                            }

                            if (BrandingAndStyleObj.ApplyToAll)
                            {
                                var questionList = quizDetailsObj.QuestionsInQuiz.Where(r => r.Status == (int)StatusEnum.Active);
                                foreach (var ques in questionList)
                                {
                                    ques.NextButtonColor = BrandingAndStyleObj.ButtonColor;
                                    ques.NextButtonTxtColor = BrandingAndStyleObj.ButtonFontColor;
                                }

                                var resultList = quizDetailsObj.QuizResults.Where(r => r.Status == (int)StatusEnum.Active);
                                foreach (var result in resultList)
                                {
                                    result.ActionButtonColor = BrandingAndStyleObj.ButtonColor;
                                    result.ActionButtonTitleColor = BrandingAndStyleObj.ButtonFontColor;
                                }

                                var personalityResultSettingList = quizDetailsObj.PersonalityResultSetting;
                                foreach (var personalityResultSetting in personalityResultSettingList)
                                {
                                    personalityResultSetting.ButtonColor = BrandingAndStyleObj.ButtonColor;
                                    personalityResultSetting.ButtonFontColor = BrandingAndStyleObj.ButtonFontColor;
                                }
                            }

                            quizDetailsObj.LastUpdatedBy = BusinessUserId;
                            quizDetailsObj.LastUpdatedOn = DateTime.UtcNow;

                            quizObj.State = (int)QuizStateEnum.DRAFTED;

                            UOWObj.QuizRepository.Update(quizObj);

                            UOWObj.Save();

                          
                        }
                    }
                    else
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz not found for the QuizId " + BrandingAndStyleObj.QuizId;
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

    }
}