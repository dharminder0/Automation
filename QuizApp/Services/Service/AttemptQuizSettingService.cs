using Core.Common.Extensions;
using Newtonsoft.Json;
using QuizApp.Helpers;
using QuizApp.RepositoryExtensions;
using QuizApp.RepositoryPattern;
using QuizApp.Request;
using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace QuizApp.Services.Service
{
    public partial class QuizAttemptService : QuizAttemptServiceBase, IQuizAttemptService
    {

        public AttemptQuizSetting AttemptQuizSettings(string quizCode)
        {
            string quiztype = string.Empty;
            PublishQuizTmpModel publishQuizTmpModel = new PublishQuizTmpModel();
            publishQuizTmpModel.QuizattemptId = 0;
            publishQuizTmpModel.QuizDetailId = 0;
            publishQuizTmpModel.LeadUserId = string.Empty;
            string quizAttemptSourceId = string.Empty;
            var exObj = new Dictionary<string, object>();
            AttemptQuizSetting quizAnswerSubmit = new AttemptQuizSetting();
            using (var UOWObj = new AutomationUnitOfWork())
            {
                var quizAttemptsObj = UOWObj.QuizAttemptsRepository.GetQuizAttemptsBasicDetailsByCode(quizCode).FirstOrDefault();

                if (quizAttemptsObj == null) {
                    Status = ResultEnum.Error;
                    ErrorMessage = "Quiz not found for the QuizCode " + quizCode;
                    return quizAnswerSubmit;
                }
                quizAttemptSourceId = quizAttemptsObj.SourceId;

                publishQuizTmpModel.QuizattemptId = quizAttemptsObj.Id;
                publishQuizTmpModel.QuizDetailId = quizAttemptsObj.QuizId;
                publishQuizTmpModel.LeadUserId = quizAttemptsObj.LeadUserId;
                publishQuizTmpModel.WorkpackageInfoId = quizAttemptsObj.WorkPackageInfoId.ToIntValue();
                publishQuizTmpModel.ConfigurationDetailId = quizAttemptsObj.ConfigurationDetailsId.ToIntValue();


                var quizDetails = UOWObj.QuizDetailsRepository.GetQuizDetailsbyId(publishQuizTmpModel.QuizDetailId).FirstOrDefault();
                publishQuizTmpModel.QuizVariables = GetQuizVariablesDetails(quizDetails.Id);
                publishQuizTmpModel.QuizTitle = quizDetails.QuizTitle;
                publishQuizTmpModel.ParentQuizid = quizDetails.ParentQuizId;
                publishQuizTmpModel.QuizDetailInfo.QuizCoverTitle = quizDetails.QuizCoverTitle;
                publishQuizTmpModel.QuizDetailInfo.ShowQuizCoverTitle = quizDetails.ShowQuizCoverTitle;
                publishQuizTmpModel.QuizDetailInfo.AutoPlay = quizDetails.AutoPlay;
                publishQuizTmpModel.QuizDetailInfo.SecondsToApply = quizDetails.SecondsToApply ?? "0";
                publishQuizTmpModel.QuizDetailInfo.QuizDescription = quizDetails.QuizDescription;
                publishQuizTmpModel.QuizDetailInfo.ShowDescription = quizDetails.ShowDescription;
                publishQuizTmpModel.QuizDetailInfo.EnableNextButton = quizDetails.EnableNextButton;
                publishQuizTmpModel.QuizDetailInfo.DisplayOrderForTitleImage = quizDetails.DisplayOrderForTitleImage;
                publishQuizTmpModel.QuizDetailInfo.DisplayOrderForTitle = quizDetails.DisplayOrderForTitle;
                publishQuizTmpModel.QuizDetailInfo.DisplayOrderForDescription = quizDetails.DisplayOrderForDescription;
                publishQuizTmpModel.QuizDetailInfo.DisplayOrderForNextButton = quizDetails.DisplayOrderForNextButton;

                publishQuizTmpModel.QuizDetailInfo.QuizCoverImage = quizDetails.QuizCoverImage;
                publishQuizTmpModel.QuizDetailInfo.ShowQuizCoverImage = quizDetails.ShowQuizCoverImage;
                publishQuizTmpModel.QuizDetailInfo.QuizCoverImgXCoordinate = quizDetails.QuizCoverImgXCoordinate;
                publishQuizTmpModel.QuizDetailInfo.QuizCoverImgYCoordinate = quizDetails.QuizCoverImgYCoordinate;
                publishQuizTmpModel.QuizDetailInfo.QuizCoverImgHeight = quizDetails.QuizCoverImgHeight;
                publishQuizTmpModel.QuizDetailInfo.QuizCoverImgWidth = quizDetails.QuizCoverImgWidth;
                publishQuizTmpModel.QuizDetailInfo.QuizCoverImgAttributionLabel = quizDetails.QuizCoverImgAttributionLabel;
                publishQuizTmpModel.QuizDetailInfo.QuizCoverImgAltTag = quizDetails.QuizCoverImgAltTag;
                publishQuizTmpModel.QuizDetailInfo.VideoFrameEnabled = quizDetails.VideoFrameEnabled ?? false;
                publishQuizTmpModel.QuizDetailInfo.QuizStartButtonText = quizDetails.StartButtonText;
                publishQuizTmpModel.QuizDetailInfo.PublicIdForQuizCover = quizDetails.PublicId;
                publishQuizTmpModel.QuizDetailInfo.EnableMediaFile = quizDetails.EnableMediaFile;
                var quiz = UOWObj.QuizRepository.GetQuizById(publishQuizTmpModel.ParentQuizid).FirstOrDefault();
                publishQuizTmpModel.CompanyId = quiz.CompanyId.Value;
                publishQuizTmpModel.CompanyDetails = BindCompanyDetails(quiz.Company);
                publishQuizTmpModel.QuizType = quiz.QuizType;
                quizAnswerSubmit.OfficeId = quiz.AccessibleOfficeId;
                quizAnswerSubmit.CompanyCode = publishQuizTmpModel.CompanyDetails.ClientCode;
                publishQuizTmpModel.CompanyCode = publishQuizTmpModel.CompanyDetails.ClientCode;

                quizAnswerSubmit.UsageType = new List<int>();
                var usageTypeInQuiz = quiz.UsageTypeInQuiz.Where(r => r.QuizId == quiz.Id).Select(v => v.UsageType);

                foreach (var item in usageTypeInQuiz)
                {
                    quizAnswerSubmit.UsageType.Add(item);
                }
            }

            using (var UOWObj = new AutomationUnitOfWork())
            {
                var quizStatsObj = UOWObj.QuizStatsRepository.GetQuizStatsQuizAttemptId(publishQuizTmpModel.QuizattemptId).FirstOrDefault();
                if (quizStatsObj == null)
                {
                    quiztype = "fetch_quiz";
                    publishQuizTmpModel.RequestQuestionId = -1;
                    publishQuizTmpModel.RequestAnswerId = null;
                }
                else
                {
                    if (quizStatsObj.CompletedOn.HasValue)
                    {
                        Status = ResultEnum.Error;
                        ErrorMessage = "Quiz has already been completed.";
                        return quizAnswerSubmit;
                    }

                    else
                    {
                        publishQuizTmpModel.IsQuizStatNotcompleted = false;
                    }
                }
            }


            if (!string.IsNullOrWhiteSpace(publishQuizTmpModel.LeadUserId) && publishQuizTmpModel.LeadUserId != "0")
            {
                //publishQuizTmpModel.LeadDetails = OWCHelper.GetLeadInfo(publishQuizTmpModel.LeadUserId, publishQuizTmpModel.CompanyDetails.ClientCode);
                var commContactInfo = _owchelper.GetCommContactDetails(publishQuizTmpModel.LeadUserId, publishQuizTmpModel.CompanyDetails.ClientCode);
                if (commContactInfo != null && commContactInfo.Any())
                {
                    exObj = (JsonConvert.DeserializeObject<Dictionary<string, object>>(commContactInfo.ToString()));
                    publishQuizTmpModel.ContactObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(commContactInfo.ToString());
                    publishQuizTmpModel.LeadDetails = (JsonConvert.DeserializeObject<QuizLeadInfo>(commContactInfo.ToString()));
                }

                using (var UOWObj = new AutomationUnitOfWork())
                {
                    //var mediaVariablesDetails = UOWObj.MediaVariablesDetailsRepository.Get(r => r.QuizId == publishQuizTmpModel.QuizDetailId);
                    var mediaVariablesDetails = UOWObj.MediaVariablesDetailsRepository.GetMediaVariablesDetailsByQuizId(publishQuizTmpModel.QuizDetailId);

                    if (mediaVariablesDetails != null && mediaVariablesDetails.Any(a => !string.IsNullOrWhiteSpace(a.MediaOwner) && !string.IsNullOrWhiteSpace(a.ProfileMedia)) && !string.IsNullOrWhiteSpace(publishQuizTmpModel.LeadUserId))
                    {
                        //List<int> businessUserIds = new List<int>();
                        //if (publishQuizTmpModel.LeadDetails.SourceOwnerId != 0) businessUserIds.Add(publishQuizTmpModel.LeadDetails.SourceOwnerId);
                        //if (publishQuizTmpModel.LeadDetails.ContactOwnerId != 0) businessUserIds.Add(publishQuizTmpModel.LeadDetails.ContactOwnerId);
                        //var dbUsers = UOWObj.UserTokensRepository.Get(r => r.CompanyId == publishQuizTmpModel.CompanyId && businessUserIds.Distinct().Contains(r.BusinessUserId));
                        //publishQuizTmpModel.LeadOwners = dbUsers.ToList();
                        //publishQuizTmpModel.UserMediaClassifications = dbUsers.Any() ? OWCHelper.GetUserMediaClassification(publishQuizTmpModel.CompanyDetails.ClientCode, dbUsers.Select(a => a.OWCToken).ToList()) : new List<Response.UserMediaClassification>();
                        try
                        {
                            List<int> businessUserIds = new List<int>();

                            if (!string.IsNullOrWhiteSpace(publishQuizTmpModel.LeadDetails?.contactId) && !publishQuizTmpModel.LeadDetails.contactId.ContainsCI("SF-"))
                            {

                                if (!string.IsNullOrWhiteSpace(publishQuizTmpModel.LeadDetails.SourceOwnerId))
                                {
                                    int sourceowwnerId = int.Parse(publishQuizTmpModel.LeadDetails.SourceOwnerId);
                                    if (sourceowwnerId != 0)
                                    {
                                        businessUserIds.Add(sourceowwnerId);
                                    }
                                }

                                if (!string.IsNullOrWhiteSpace(publishQuizTmpModel.LeadDetails.ContactOwnerId))
                                {
                                    int contactOwnerId = int.Parse(publishQuizTmpModel.LeadDetails.ContactOwnerId);
                                    if (contactOwnerId != 0)
                                    {
                                        businessUserIds.Add(contactOwnerId);
                                    }
                                }

                                var dbUsers = UOWObj.UserTokensRepository.Get(r => r.CompanyId == publishQuizTmpModel.CompanyId && businessUserIds.Distinct().Contains(r.BusinessUserId));
                                publishQuizTmpModel.LeadOwners = dbUsers.ToList();
                                publishQuizTmpModel.UserMediaClassifications = dbUsers.Any() ? OWCHelper.GetUserMediaClassification(publishQuizTmpModel.CompanyDetails.ClientCode, dbUsers.Select(a => a.OWCToken).ToList()) : new List<Response.UserMediaClassification>();
                            }
                            else
                            {
                                if(!string.IsNullOrWhiteSpace(publishQuizTmpModel.LeadDetails.SourceOwnerId) || !string.IsNullOrWhiteSpace(publishQuizTmpModel.LeadDetails.ContactOwnerId))
                                {
                                    var externalDetails = _owchelper.GetExternalDetails(publishQuizTmpModel.CompanyDetails.ClientCode, publishQuizTmpModel.LeadDetails.SourceOwnerId);
                                    if (externalDetails != null)
                                    {
                                        var userDetails = JsonConvert.DeserializeObject<UserDto>(externalDetails);
                                        if (userDetails?.Id != 0)
                                        {
                                            businessUserIds.Add(userDetails.Id);
                                            publishQuizTmpModel.LeadDetails.JRSourceOwnerId = userDetails.Id;
                                        }
                                        
                                    }

                                    var externalDetails2 = _owchelper.GetExternalDetails(publishQuizTmpModel.CompanyDetails.ClientCode, publishQuizTmpModel.LeadDetails.ContactOwnerId);
                                    if (externalDetails2 != null)
                                    {
                                        var userDetails = JsonConvert.DeserializeObject<UserDto>(externalDetails2);
                                        if(userDetails?.Id != 0)
                                        {
                                            businessUserIds.Add(userDetails.Id);
                                            publishQuizTmpModel.LeadDetails.JRContactOwnerId = userDetails.Id;
                                        }
                                        
                                    }

                                    if (businessUserIds.Any() && businessUserIds != null)
                                    {
                                        var dbUsers = UOWObj.UserTokensRepository.Get(r => r.CompanyId == publishQuizTmpModel.CompanyId && businessUserIds.Distinct().Contains(r.BusinessUserId));
                                        publishQuizTmpModel.LeadOwners = dbUsers.ToList();
                                        publishQuizTmpModel.UserMediaClassifications = dbUsers.Any() ? OWCHelper.GetUserMediaClassification(publishQuizTmpModel.CompanyDetails.ClientCode, dbUsers.Select(a => a.OWCToken).ToList()) : new List<Response.UserMediaClassification>();
                                    }

                                }

                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }


            var domainList = CommonStaticData.GetCachedClientDomains(publishQuizTmpModel.CompanyDetails);
            if (domainList != null && domainList.Any())
            {
                quizAnswerSubmit.GtmCode = domainList.FirstOrDefault().GtmCode;
                quizAnswerSubmit.FavoriteIconUrl = domainList.FirstOrDefault().FavoriteIconUrl;
            }

            quizAnswerSubmit.QuizBrandingAndStyle = BrandingStyleObj(publishQuizTmpModel.QuizDetailId);
            quizAnswerSubmit.QuizBrandingAndStyle.QuizId = publishQuizTmpModel.ParentQuizid;
            
                QuizAttemptService.FetchRecruiterVariables(publishQuizTmpModel);
            
            quizAnswerSubmit.QuizCoverDetails = SetQuizCover(publishQuizTmpModel);

            quizAnswerSubmit.PrimaryBrandingColor = publishQuizTmpModel.CompanyDetails.PrimaryBrandingColor;
            quizAnswerSubmit.SecondaryBrandingColor = publishQuizTmpModel.CompanyDetails.SecondaryBrandingColor;
            //quizAnswerSubmit.TertiaryColor = publishQuizTmpModel.CompanyDetails.TertiaryColor;
            quizAnswerSubmit.Tag = SetAnswerTags(publishQuizTmpModel.ParentQuizid, publishQuizTmpModel.CompanyCode);

            if (publishQuizTmpModel.ConfigurationDetailId > 0)
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var configurationDetails = UOWObj.ConfigurationDetailsRepository.GetConfigurationDetailsById(publishQuizTmpModel.ConfigurationDetailId).FirstOrDefault();

                    if (configurationDetails != null)
                    {
                        quizAnswerSubmit.SourceId = configurationDetails.SourceId;
                        quizAnswerSubmit.SourceName = configurationDetails.SourceName;
                        quizAnswerSubmit.SourceType = configurationDetails.SourceType;
                        quizAnswerSubmit.PrivacyLink = configurationDetails.PrivacyLink;
                        quizAnswerSubmit.ConfigurationType = configurationDetails.ConfigurationType;
                        quizAnswerSubmit.ConfigurationId = configurationDetails.ConfigurationId;
                        quizAnswerSubmit.LeadFormTitle = configurationDetails.LeadFormTitle;
                        if (!string.IsNullOrWhiteSpace(configurationDetails.PrivacyJson))
                        {
                            quizAnswerSubmit.PrivacyJson = JsonConvert.DeserializeObject<PrivacyDto>(configurationDetails.PrivacyJson);
                        }

                        quizAnswerSubmit.SourceId = string.IsNullOrWhiteSpace(quizAnswerSubmit.SourceId) && !string.IsNullOrWhiteSpace(quizAttemptSourceId) ? quizAttemptSourceId : quizAnswerSubmit.SourceId;
                    }
                }
            }

            quizAnswerSubmit.LoadQuizDetails = true;
            return quizAnswerSubmit;
        }


        private QuizBrandingAndStyleModel BrandingStyleObj(int quizDetaildid)
        {
            QuizBrandingAndStyleModel brandingAndStyleObj = new QuizBrandingAndStyleModel();
            using (var UOWObj = new AutomationUnitOfWork())
            {
                var brandingAndStyle = UOWObj.QuizBrandingAndStyleRepository.GetQuizBrandingAndStyleByQuizId(quizDetaildid).FirstOrDefault();

                if (brandingAndStyle != null)
                {
                    brandingAndStyleObj.ImageFileURL = brandingAndStyle.ImageFileURL ?? string.Empty;
                    //brandingAndStyleObj.PublicIdForFileURL = brandingAndStyle.PublicId ?? string.Empty;
                    brandingAndStyleObj.PublicIdForImageFile = brandingAndStyle.PublicId ?? string.Empty;
                    brandingAndStyleObj.BackgroundColor = brandingAndStyle.BackgroundColor ?? string.Empty;
                    brandingAndStyleObj.ButtonColor = brandingAndStyle.ButtonColor ?? string.Empty;
                    brandingAndStyleObj.OptionColor = brandingAndStyle.OptionColor ?? string.Empty;
                    brandingAndStyleObj.ButtonFontColor = brandingAndStyle.ButtonFontColor ?? string.Empty;
                    brandingAndStyleObj.OptionFontColor = brandingAndStyle.OptionFontColor ?? string.Empty;
                    brandingAndStyleObj.FontColor = brandingAndStyle.FontColor ?? string.Empty;
                    brandingAndStyleObj.ButtonHoverColor = brandingAndStyle.ButtonHoverColor ?? string.Empty;
                    brandingAndStyleObj.ButtonHoverTextColor = brandingAndStyle.ButtonHoverTextColor ?? string.Empty;
                    brandingAndStyleObj.FontType = brandingAndStyle.FontType ?? string.Empty;
                    brandingAndStyleObj.BackgroundColorofSelectedAnswer = brandingAndStyle.BackgroundColorofSelectedAnswer ?? string.Empty;
                    brandingAndStyleObj.BackgroundColorofAnsweronHover = brandingAndStyle.BackgroundColorofAnsweronHover ?? string.Empty;
                    brandingAndStyleObj.AnswerTextColorofSelectedAnswer = brandingAndStyle.AnswerTextColorofSelectedAnswer ?? string.Empty;
                    brandingAndStyleObj.IsBackType = brandingAndStyle.IsBackType;
                    brandingAndStyleObj.BackImageFileURL = brandingAndStyle.BackImageFileURL ?? string.Empty;
                    brandingAndStyleObj.BackColor = brandingAndStyle.BackColor ?? string.Empty;
                    brandingAndStyleObj.Opacity = brandingAndStyle.Opacity ?? string.Empty;
                    brandingAndStyleObj.LogoUrl = brandingAndStyle.LogoUrl ?? string.Empty;
                    brandingAndStyleObj.LogoPublicId = brandingAndStyle.LogoPublicId ?? string.Empty;
                    brandingAndStyleObj.Language = brandingAndStyle.Language;
                    brandingAndStyleObj.BackgroundColorofLogo = brandingAndStyle.BackgroundColorofLogo ?? string.Empty;
                    brandingAndStyleObj.AutomationAlignment = brandingAndStyle.AutomationAlignment ?? string.Empty;
                    brandingAndStyleObj.LogoAlignment = brandingAndStyle.LogoAlignment ?? string.Empty;
                    brandingAndStyleObj.Flip = brandingAndStyle.Flip;                    
                }
            }

            return brandingAndStyleObj;
        }

        private List<Tags> SetAnswerTags(int quizId, string clientcode)
        {
            List<Tags> listTags = new List<Tags>();
            using (var UOWObj = new AutomationUnitOfWork())
            {
                var quizTagDetails = UOWObj.QuizTagDetailsRepository.GetQuizTagDetailsByQuizId(quizId);
                if (quizTagDetails != null && quizTagDetails.Any())
                {
                    var automationTagsList = CommonStaticData.GetCachedAutomationTagsList(clientcode);
                    foreach (var tag in quizTagDetails)
                    {
                        listTags.Add(new Tags()
                        {
                            TagId = tag.TagId,
                            TagName = (automationTagsList != null && automationTagsList.Any(r => r.tagId == tag.TagId)) ? automationTagsList.FirstOrDefault(r => r.tagId == tag.TagId).tagName : string.Empty,
                            TagCode = (automationTagsList != null && automationTagsList.Any(r => r.tagId == tag.TagId)) ? automationTagsList.FirstOrDefault(r => r.tagId == tag.TagId).tagCode : string.Empty
                        });
                    }

                }
            }

            return listTags;
        }

        private QuizCover SetQuizCover(PublishQuizTmpModel publishQuizTmpModel)
        {
            var quizCover = new QuizCover();
            quizCover.QuizType = publishQuizTmpModel.QuizType;
            var msgVariables = publishQuizTmpModel.QuizVariables?.Where(v => v.ObjectTypes == (int)QuizVariableObjectTypes.COVER && v.ObjectId == publishQuizTmpModel.QuizDetailId).FirstOrDefault()?.Variables;
            CommonStaticData.VacancyVariableReplace(publishQuizTmpModel.ContactObject, msgVariables, publishQuizTmpModel.CompanyCode);
            quizCover.QuizTitle = VariableLinking(publishQuizTmpModel.QuizTitle, true, false, null, publishQuizTmpModel, msgVariables);
            quizCover.QuizCoverTitle = VariableLinking(publishQuizTmpModel.QuizDetailInfo.QuizCoverTitle, false, false, null, publishQuizTmpModel, msgVariables);
            quizCover.ShowQuizCoverTitle = publishQuizTmpModel.QuizDetailInfo.ShowQuizCoverTitle;
            quizCover.AutoPlay = publishQuizTmpModel.QuizDetailInfo.AutoPlay;
            quizCover.SecondsToApply = publishQuizTmpModel.QuizDetailInfo.SecondsToApply ?? "0";
            quizCover.QuizDescription = VariableLinking(publishQuizTmpModel.QuizDetailInfo.QuizDescription, false, false, null, publishQuizTmpModel, msgVariables);
            quizCover.ShowDescription = publishQuizTmpModel.QuizDetailInfo.ShowDescription;
            quizCover.EnableNextButton = publishQuizTmpModel.QuizDetailInfo.EnableNextButton;
            quizCover.DisplayOrderForTitleImage = publishQuizTmpModel.QuizDetailInfo.DisplayOrderForTitleImage;
            quizCover.DisplayOrderForTitle = publishQuizTmpModel.QuizDetailInfo.DisplayOrderForTitle;
            quizCover.DisplayOrderForDescription = publishQuizTmpModel.QuizDetailInfo.DisplayOrderForDescription;
            quizCover.DisplayOrderForNextButton = publishQuizTmpModel.QuizDetailInfo.DisplayOrderForNextButton;

            quizCover.QuizCoverImage = publishQuizTmpModel.QuizDetailInfo.QuizCoverImage;
            quizCover.ShowQuizCoverImage = publishQuizTmpModel.QuizDetailInfo.ShowQuizCoverImage;
            quizCover.QuizCoverImgXCoordinate = publishQuizTmpModel.QuizDetailInfo.QuizCoverImgXCoordinate;
            quizCover.QuizCoverImgYCoordinate = publishQuizTmpModel.QuizDetailInfo.QuizCoverImgYCoordinate;
            quizCover.QuizCoverImgHeight = publishQuizTmpModel.QuizDetailInfo.QuizCoverImgHeight;
            quizCover.QuizCoverImgWidth = publishQuizTmpModel.QuizDetailInfo.QuizCoverImgWidth;
            quizCover.QuizCoverImgAttributionLabel = publishQuizTmpModel.QuizDetailInfo.QuizCoverImgAttributionLabel;
            quizCover.QuizCoverImgAltTag = publishQuizTmpModel.QuizDetailInfo.QuizCoverImgAltTag;
            quizCover.VideoFrameEnabled = publishQuizTmpModel.QuizDetailInfo.VideoFrameEnabled ?? false;
            quizCover.QuizStartButtonText = publishQuizTmpModel.QuizDetailInfo.QuizStartButtonText;
            quizCover.PublicIdForQuizCover = publishQuizTmpModel.QuizDetailInfo.PublicIdForQuizCover;




            if (publishQuizTmpModel.QuizDetailInfo.EnableMediaFile)
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    //var mediaObj = UOWObj.MediaVariablesDetailsRepository.Get(r => r.QuizId == publishQuizTmpModel.QuizDetailId && r.ConfigurationDetailsId == publishQuizTmpModel.ConfigurationDetailId && r.ObjectTypeId == (int)BranchingLogicEnum.COVERDETAILS && r.ObjectId == publishQuizTmpModel.QuizDetailId).FirstOrDefault();
                    var mediaObjList = UOWObj.MediaVariablesDetailsRepository.GetMediaVariablesDetailsByQuizId(publishQuizTmpModel.QuizDetailId);
                    if(mediaObjList != null && mediaObjList.Any())
                    {
                        var mediaObj = mediaObjList.Where(r => r.ConfigurationDetailsId == publishQuizTmpModel.ConfigurationDetailId && r.ObjectTypeId == (int)BranchingLogicEnum.COVERDETAILS && r.ObjectId == publishQuizTmpModel.QuizDetailId).FirstOrDefault();

                        if (mediaObj != null)
                        {
                            quizCover.QuizCoverImage = mediaObj.ObjectValue;
                            quizCover.ShowQuizCoverImage = true;
                            quizCover.PublicIdForQuizCover = mediaObj.ObjectPublicId;

                            var newMedia = ExtractDynamicMedia(mediaObj, publishQuizTmpModel.LeadDetails, publishQuizTmpModel.UserMediaClassifications, publishQuizTmpModel.LeadOwners);
                            if (newMedia != null)
                            {
                                quizCover.QuizCoverImage = newMedia.MediaUrl;
                                quizCover.PublicIdForQuizCover = newMedia.MediaPublicId;
                            }
                        }
                    }
                    
                }
            }

            return quizCover;
        }
    }
}