using QuizApp.Db;
using QuizApp.Helpers;
using QuizApp.RepositoryPattern;
using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Core.Common.Extensions;
using Ignite.Security.Lib;
using QuizApp.RepositoryExtensions;

namespace QuizApp.Services.Service
{
    public class BusinessUserService : IBusinessUserService
    {
        private ResultEnum status = ResultEnum.Ok;
        private string errormessage = string.Empty;
        private readonly OWCHelper _owchelper = new OWCHelper();

        public ResultEnum Status { get { return status; } set { status = value; } }
        public string ErrorMessage { get { return errormessage; } set { errormessage = value; } }

        public BusinessUser AuthorizeToken(string jwtToken, string companyCode)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var token = string.Empty;
                    try
                    {
                        var decryptedJwtToken = JwtSecurityService.Decrypt(GlobalSettings.owcSSOSecretKeyForDecrypt.ToString(), jwtToken.Replace(" ", "+"));
                        token = JwtSecurityService.Decode(decryptedJwtToken);
                    }
                    catch (Exception ex)
                    {
                        return new BusinessUser() { BusinessUserId = -1, CompanyInfo = new CompanyModel() };
                    }

                    var userToken = UOWObj.UserTokensRepository.Get(r => r.OWCToken == token && r.Company.ClientCode.Equals(companyCode, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                    if (userToken == null)
                    {
                        var response = OWCHelper.ValidateUserToken("", token, companyCode, false);

                        if (response != null && response.OWCBusinessUserResponse != null && string.IsNullOrEmpty(response.Error))
                        {
                            SaveUpdateUserInfo(OWCHelper.MapOWCResponseToEntity(response));
                        }

                        userToken = UOWObj.UserTokensRepository.Get(r => r.OWCToken == token && r.Company.ClientCode.Equals(companyCode, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    }

                    if (userToken != null)
                    {                       
                        return new BusinessUser()
                        {
                            Id = userToken.Id,
                            BusinessUserId = userToken.BusinessUserId,
                            CreateAcademyCourse = userToken.CreateAcademyCourse,
                            CreateTechnicalRecruiterCourse = userToken.CreateTechnicalRecruiterCourse,
                            CreateTemplate = userToken.CreateTemplate,
                            IsGlobalOfficeAdmin = userToken.IsGlobalOfficeAdmin,
                            IsAutomationTechnicalRecruiterCoursePermission = userToken.IsAutomationTechnicalRecruiterCoursePermission,
                            IsAppointmentTechnicalRecruiterCoursePermission = userToken.IsAppointmentTechnicalRecruiterCoursePermission,
                            IsELearningTechnicalRecruiterCoursePermission = userToken.IsELearningTechnicalRecruiterCoursePermission,
                            IsCanvasTechnicalRecruiterCoursePermission = userToken.IsCanvasTechnicalRecruiterCoursePermission,
                            IsVacanciesTechnicalRecruiterCoursePermission = userToken.IsVacanciesTechnicalRecruiterCoursePermission,
                            IsContactsTechnicalRecruiterCoursePermission = userToken.IsContactsTechnicalRecruiterCoursePermission,
                            IsReviewTechnicalRecruiterCoursePermission = userToken.IsReviewTechnicalRecruiterCoursePermission,
                            IsReportingTechnicalRecruiterCoursePermission = userToken.IsReportingTechnicalRecruiterCoursePermission,
                            IsCampaignsTechnicalRecruiterCoursePermission = userToken.IsCampaignsTechnicalRecruiterCoursePermission,
                            IsCreateStandardAutomationPermission = userToken.IsCreateStandardAutomationPermission,
                            CompanyInfo = new CompanyModel()
                            {
                                Id = userToken.Company.Id,
                                AlternateClientCodes = userToken.Company.AlternateClientCodes,
                                ClientCode = userToken.Company.ClientCode,
                                CompanyName = userToken.Company.CompanyName,
                                CompanyWebsiteUrl = userToken.Company.CompanyWebsiteUrl,
                                JobRocketApiAuthorizationBearer = userToken.Company.JobRocketApiAuthorizationBearer,
                                JobRocketApiUrl = userToken.Company.JobRocketApiUrl,
                                JobRocketClientUrl = userToken.Company.JobRocketClientUrl,
                                LeadDashboardApiAuthorizationBearer = userToken.Company.LeadDashboardApiAuthorizationBearer,
                                LeadDashboardApiUrl = userToken.Company.LeadDashboardApiUrl,
                                LeadDashboardClientUrl = userToken.Company.LeadDashboardClientUrl,
                                LogoUrl = userToken.Company.LogoUrl,
                                PrimaryBrandingColor = userToken.Company.PrimaryBrandingColor,
                                SecondaryBrandingColor = userToken.Company.SecondaryBrandingColor,
                                CreateAcademyCourseEnabled = userToken.Company.CreateAcademyCourseEnabled,
                                CreateTechnicalRecruiterCourseEnabled = userToken.Company.CreateTechnicalRecruiterCourseEnabled,
                                CreateTemplateEnabled = userToken.Company.CreateTemplateEnabled
                            },
                        };
                    }
                }
                return new BusinessUser() { BusinessUserId = -1, CompanyInfo = new CompanyModel() };
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }

        public void SaveUpdateUserInfo(BusinessUser businessUser)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {

                    //var companyObj = UOWObj.CompanyRepository.Get(r => r.CompanyId == businessUser.CompanyInfo.Id).FirstOrDefault();
                    var companyObj = UOWObj.CompanyRepository.GetCompanyByCompanyId(businessUser.CompanyInfo.Id).FirstOrDefault();

                    if (companyObj == null)
                    {
                        companyObj = new Db.Company
                        {
                            CompanyId = businessUser.CompanyInfo.Id,
                            AlternateClientCodes = businessUser.CompanyInfo.AlternateClientCodes,
                            ClientCode = businessUser.CompanyInfo.ClientCode,
                            CompanyName = businessUser.CompanyInfo.CompanyName,
                            CompanyWebsiteUrl = businessUser.CompanyInfo.CompanyWebsiteUrl,
                            JobRocketApiAuthorizationBearer = businessUser.CompanyInfo.JobRocketApiAuthorizationBearer,
                            JobRocketApiUrl = businessUser.CompanyInfo.JobRocketApiUrl,
                            JobRocketClientUrl = businessUser.CompanyInfo.JobRocketClientUrl,
                            LeadDashboardApiAuthorizationBearer = businessUser.CompanyInfo.LeadDashboardApiAuthorizationBearer,
                            LeadDashboardApiUrl = businessUser.CompanyInfo.LeadDashboardApiUrl,
                            LeadDashboardClientUrl = businessUser.CompanyInfo.LeadDashboardClientUrl,
                            LogoUrl = businessUser.CompanyInfo.LogoUrl,
                            PrimaryBrandingColor = businessUser.CompanyInfo.PrimaryBrandingColor,
                            SecondaryBrandingColor = businessUser.CompanyInfo.SecondaryBrandingColor,
                            TertiaryColor = businessUser.CompanyInfo.TertiaryColor,
                            CreateAcademyCourseEnabled = businessUser.CompanyInfo.CreateAcademyCourseEnabled,
                            CreateTechnicalRecruiterCourseEnabled = businessUser.CompanyInfo.CreateTechnicalRecruiterCourseEnabled,
                            CreateTemplateEnabled = businessUser.CompanyInfo.CreateTemplateEnabled,
                            BadgesEnabled = businessUser.CompanyInfo.BadgesEnabled
                        };

                        UOWObj.CompanyRepository.Insert(companyObj);
                    }
                    else
                    {
                        companyObj.AlternateClientCodes = businessUser.CompanyInfo.AlternateClientCodes;
                        companyObj.ClientCode = businessUser.CompanyInfo.ClientCode;
                        companyObj.CompanyName = businessUser.CompanyInfo.CompanyName;
                        companyObj.CompanyWebsiteUrl = businessUser.CompanyInfo.CompanyWebsiteUrl;
                        companyObj.JobRocketApiAuthorizationBearer = businessUser.CompanyInfo.JobRocketApiAuthorizationBearer;
                        companyObj.JobRocketApiUrl = businessUser.CompanyInfo.JobRocketApiUrl;
                        companyObj.JobRocketClientUrl = businessUser.CompanyInfo.JobRocketClientUrl;
                        companyObj.LeadDashboardApiAuthorizationBearer = businessUser.CompanyInfo.LeadDashboardApiAuthorizationBearer;
                        companyObj.LeadDashboardApiUrl = businessUser.CompanyInfo.LeadDashboardApiUrl;
                        companyObj.LeadDashboardClientUrl = businessUser.CompanyInfo.LeadDashboardClientUrl;
                        companyObj.LogoUrl = businessUser.CompanyInfo.LogoUrl;
                        companyObj.PrimaryBrandingColor = businessUser.CompanyInfo.PrimaryBrandingColor;
                        companyObj.SecondaryBrandingColor = businessUser.CompanyInfo.SecondaryBrandingColor;
                        companyObj.TertiaryColor = businessUser.CompanyInfo.TertiaryColor;
                        companyObj.CreateAcademyCourseEnabled = businessUser.CompanyInfo.CreateAcademyCourseEnabled;
                        companyObj.CreateTechnicalRecruiterCourseEnabled = businessUser.CompanyInfo.CreateTechnicalRecruiterCourseEnabled;
                        companyObj.CreateTemplateEnabled = businessUser.CompanyInfo.CreateTemplateEnabled;
                        companyObj.BadgesEnabled = businessUser.CompanyInfo.BadgesEnabled;

                        UOWObj.CompanyRepository.Update(companyObj);
                    }

                    UOWObj.Save();


                    var userObj = UOWObj.UserTokensRepository.Get(r => r.BusinessUserId == businessUser.BusinessUserId && r.Company.CompanyId == businessUser.CompanyInfo.Id).FirstOrDefault();

                    if (userObj == null)
                    {
                        UOWObj.UserTokensRepository.Insert(new UserTokens
                        {
                            BusinessUserId = businessUser.BusinessUserId,
                            OWCToken = businessUser.OWCToken,
                            CompanyId = companyObj.Id,
                            CreateAcademyCourse = businessUser.CreateAcademyCourse,
                            CreateTechnicalRecruiterCourse = businessUser.CreateTechnicalRecruiterCourse,
                            CreateTemplate = businessUser.CreateTemplate,
                            IsGlobalOfficeAdmin = businessUser.IsGlobalOfficeAdmin,
                            IsAutomationTechnicalRecruiterCoursePermission = businessUser.IsAutomationTechnicalRecruiterCoursePermission,
                            IsAppointmentTechnicalRecruiterCoursePermission = businessUser.IsAppointmentTechnicalRecruiterCoursePermission,
                            IsELearningTechnicalRecruiterCoursePermission = businessUser.IsELearningTechnicalRecruiterCoursePermission,
                            IsCanvasTechnicalRecruiterCoursePermission = businessUser.IsCanvasTechnicalRecruiterCoursePermission,
                            IsVacanciesTechnicalRecruiterCoursePermission = businessUser.IsVacanciesTechnicalRecruiterCoursePermission,
                            IsContactsTechnicalRecruiterCoursePermission = businessUser.IsContactsTechnicalRecruiterCoursePermission,
                            IsReviewTechnicalRecruiterCoursePermission = businessUser.IsReviewTechnicalRecruiterCoursePermission,
                            IsReportingTechnicalRecruiterCoursePermission = businessUser.IsReportingTechnicalRecruiterCoursePermission,
                            IsCampaignsTechnicalRecruiterCoursePermission = businessUser.IsCampaignsTechnicalRecruiterCoursePermission,
                            IsCreateStandardAutomationPermission = businessUser.IsCreateStandardAutomationPermission
                        });
                    }
                    else
                    {
                        userObj.OWCToken = businessUser.OWCToken;
                        userObj.CompanyId = companyObj.Id;
                        userObj.CreateAcademyCourse = businessUser.CreateAcademyCourse;
                        userObj.CreateTechnicalRecruiterCourse = businessUser.CreateTechnicalRecruiterCourse;
                        userObj.CreateTemplate = businessUser.CreateTemplate;
                        userObj.IsGlobalOfficeAdmin = businessUser.IsGlobalOfficeAdmin;
                        userObj.IsAutomationTechnicalRecruiterCoursePermission = businessUser.IsAutomationTechnicalRecruiterCoursePermission;
                        userObj.IsAppointmentTechnicalRecruiterCoursePermission = businessUser.IsAppointmentTechnicalRecruiterCoursePermission;
                        userObj.IsELearningTechnicalRecruiterCoursePermission = businessUser.IsELearningTechnicalRecruiterCoursePermission;
                        userObj.IsCanvasTechnicalRecruiterCoursePermission = businessUser.IsCanvasTechnicalRecruiterCoursePermission;
                        userObj.IsVacanciesTechnicalRecruiterCoursePermission = businessUser.IsVacanciesTechnicalRecruiterCoursePermission;
                        userObj.IsContactsTechnicalRecruiterCoursePermission = businessUser.IsContactsTechnicalRecruiterCoursePermission;
                        userObj.IsReviewTechnicalRecruiterCoursePermission = businessUser.IsReviewTechnicalRecruiterCoursePermission;
                        userObj.IsReportingTechnicalRecruiterCoursePermission = businessUser.IsReportingTechnicalRecruiterCoursePermission;
                        userObj.IsCampaignsTechnicalRecruiterCoursePermission = businessUser.IsCampaignsTechnicalRecruiterCoursePermission;
                        userObj.IsCreateStandardAutomationPermission = businessUser.IsCreateStandardAutomationPermission;
                        UOWObj.UserTokensRepository.Update(userObj);
                    }

                    UOWObj.Save();
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
        }


        public object GetClientCountries(string clientCode)
        {
            if(string.IsNullOrWhiteSpace(clientCode))
            {
                return null;
            }
            else
            {
                var list = _owchelper.ClientCountries(clientCode);
                return list;
            }  
        }
    }
}
