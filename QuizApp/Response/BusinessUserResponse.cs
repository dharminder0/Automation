using Core.Common.Extensions;
using QuizApp.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using static QuizApp.Helpers.Models;

namespace QuizApp.Response
{
    public class BusinessUserResponse
    {
        public int BusinessUserId { get; set; }
        public string AccountLoginType { get; set; }
        public List<string> Permissions { get; set; }
        public string UserToken { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string NickName { get; set; }
        public List<Offices> OfficeList { get; set; }
        public List<Offices> officesParentChildList { get; set; }
        public List<Offices> PreferredOffices { get; set; }
        public string Logo { get; set; }
        public string ActiveLanguage { get; set; }
        public string TimeZone { get; set; }
        public string PhoneNumber { get; set; }
        public long OffsetValue { get; set; }
        public List<string> permissions { get; set; }
        public List<HeaderMenus> HeaderMenus { get; set; }
        public List<HeaderMenusNew> HeaderMenusNew { get; set; }
        public string PrimaryBrandingColor { get; set; }
        public string SecondaryBrandingColor { get; set; }
        public string TertiaryColor { get; set; }
        public string LogoUrl { get; set; }
        public string LogoPublicId { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string CloudinaryKey { get; set; }
        public string CloudinaryCompanyName { get; set; }
        public string CloudinaryApikey { get; set; }
        public string CloudinaryUsername { get; set; }
        public long CloudinaryTimestamp { get; set; }
        public string CloudinaryBaseFolder { get; set; }
        public string UiFooter { get; set; }
        public bool CreateAcademyCourse { get; set; }
        public bool CreateAcademyCourseEnabled { get; set; }
        public bool CreateTemplate { get; set; }
        public bool CreateTemplateEnabled { get; set; }
        public bool IsGlobalOfficeAdmin { get; set; }
        public bool IsCloudinaryReader { get; set; }
        public bool IsCloudinaryAdmin { get; set; }
        public bool CreateTechnicalRecruiterCourse { get; set; }
        public bool CreateTechnicalRecruiterCourseEnabled { get; set; }
        public bool IsAutomationTechnicalRecruiterCoursePermission { get; set; }
        public bool IsAppointmentTechnicalRecruiterCoursePermission { get; set; }
        public bool IsELearningTechnicalRecruiterCoursePermission { get; set; }
        public bool IsCanvasTechnicalRecruiterCoursePermission { get; set; }
        public bool IsVacanciesTechnicalRecruiterCoursePermission { get; set; }
        public bool IsContactsTechnicalRecruiterCoursePermission { get; set; }
        public bool IsReviewTechnicalRecruiterCoursePermission { get; set; }
        public bool IsReportingTechnicalRecruiterCoursePermission { get; set; }
        public bool IsCampaignsTechnicalRecruiterCoursePermission { get; set; }
        public bool IsCreateStandardAutomationPermission { get; set; }
        public bool IsManageElearningPermission { get; set; }
        public bool IsNPSAutomationPermission { get; set; }
        public bool IsContactAutomationReportPermission { get; set; }
        public string Domain { get; set; }
        public bool IsWebChatbotPermission { get; set; }
        public bool IsContactTagsPermission { get; set; }
        public bool IsWhatsAppAsSMSPermission { get; set; }
        public bool IsJRSalesforceEnabled { get; set; }

        public BusinessUserResponse MapEntityToResponse(OWCBusinessUserResponse owcResponseObj, string token, OWCCompanyResponse owcCompanyResponseObj = null, string module= null, string domain = null)
        {
            BusinessUserResponse businessUserResponse = new BusinessUserResponse();

            businessUserResponse.BusinessUserId = owcResponseObj.userId;
            businessUserResponse.AccountLoginType = owcResponseObj.accountLoginType;
            businessUserResponse.Permissions = owcResponseObj.permissions;
            businessUserResponse.CloudinaryKey = owcResponseObj.cloudinarySettings != null ? owcResponseObj.cloudinarySettings.signature : string.Empty;
            businessUserResponse.CloudinaryCompanyName = owcResponseObj.cloudinarySettings != null ? owcResponseObj.cloudinarySettings.CloudName : string.Empty;
            businessUserResponse.CloudinaryApikey = owcResponseObj.cloudinarySettings != null ? owcResponseObj.cloudinarySettings.ApiKey : string.Empty;
            businessUserResponse.CloudinaryUsername = owcResponseObj.cloudinarySettings != null ? owcResponseObj.cloudinarySettings.Username : string.Empty;
            businessUserResponse.CloudinaryTimestamp = owcResponseObj.cloudinarySettings != null ? owcResponseObj.cloudinarySettings.Timestamp : default(long);
            businessUserResponse.UserToken = token;
            businessUserResponse.FirstName = owcResponseObj.firstName;
            businessUserResponse.LastName = owcResponseObj.lastName;
            businessUserResponse.UserName = owcResponseObj.userName;
            businessUserResponse.NickName = owcResponseObj.nickName;
            businessUserResponse.Logo = owcResponseObj.avatar;
            businessUserResponse.LogoPublicId = owcResponseObj.avatarPublicId;
            businessUserResponse.ActiveLanguage = owcResponseObj.activeLanguage;
            businessUserResponse.TimeZone = owcResponseObj.timeZone;
            businessUserResponse.PhoneNumber = owcResponseObj.phoneNumber;
            businessUserResponse.OffsetValue = Convert.ToInt64(-1 * owcResponseObj.offset * 60 * 60 * 1000);
            businessUserResponse.CreateAcademyCourse = owcResponseObj.permissions.Any(s => s.Equals(GlobalSettings.createAcademyCourseKey.ToString(), StringComparison.OrdinalIgnoreCase));
            businessUserResponse.CreateTemplate = owcResponseObj.permissions.Any(s => s.Equals(GlobalSettings.createTemplateKey.ToString(), StringComparison.OrdinalIgnoreCase));
            businessUserResponse.IsGlobalOfficeAdmin = owcResponseObj.permissions.Any(s => s.Equals(GlobalSettings.globalOfficeAdminPermissionKey.ToString(), StringComparison.OrdinalIgnoreCase));
            businessUserResponse.IsCloudinaryReader = owcResponseObj.permissions.Any(s => s.Equals(GlobalSettings.cloudinaryReaderPermissionKey.ToString(), StringComparison.OrdinalIgnoreCase));
            businessUserResponse.IsCloudinaryAdmin = owcResponseObj.permissions.Any(s => s.Equals(GlobalSettings.cloudinaryAdminPermissionKey.ToString(), StringComparison.OrdinalIgnoreCase));
            businessUserResponse.CreateTechnicalRecruiterCourse = owcResponseObj.permissions.Any(s => s.Equals(GlobalSettings.createTechnicalRecruiterSkillsKey.ToString(), StringComparison.OrdinalIgnoreCase));
            businessUserResponse.IsAutomationTechnicalRecruiterCoursePermission = owcResponseObj.permissions.Any(s => s.Equals(GlobalSettings.automationTechnicalRecruiterCoursePermissionKey.ToString(), StringComparison.OrdinalIgnoreCase));
            businessUserResponse.IsAppointmentTechnicalRecruiterCoursePermission = owcResponseObj.permissions.Any(s => s.Equals(GlobalSettings.appointmentTechnicalRecruiterCoursePermissionKey.ToString(), StringComparison.OrdinalIgnoreCase));
            businessUserResponse.IsELearningTechnicalRecruiterCoursePermission = owcResponseObj.permissions.Any(s => s.Equals(GlobalSettings.eLearningTechnicalRecruiterCoursePermissionKey.ToString(), StringComparison.OrdinalIgnoreCase));
            businessUserResponse.IsCanvasTechnicalRecruiterCoursePermission = owcResponseObj.permissions.Any(s => s.Equals(GlobalSettings.canvasTechnicalRecruiterCoursePermissionKey.ToString(), StringComparison.OrdinalIgnoreCase));
            businessUserResponse.IsVacanciesTechnicalRecruiterCoursePermission = owcResponseObj.permissions.Any(s => s.Equals(GlobalSettings.vacanciesTechnicalRecruiterCoursePermissionKey.ToString(), StringComparison.OrdinalIgnoreCase));
            businessUserResponse.IsContactsTechnicalRecruiterCoursePermission = owcResponseObj.permissions.Any(s => s.Equals(GlobalSettings.contactsTechnicalRecruiterCoursePermissionKey.ToString(), StringComparison.OrdinalIgnoreCase));
            businessUserResponse.IsReviewTechnicalRecruiterCoursePermission = owcResponseObj.permissions.Any(s => s.Equals(GlobalSettings.reviewTechnicalRecruiterCoursePermissionKey.ToString(), StringComparison.OrdinalIgnoreCase));
            businessUserResponse.IsReportingTechnicalRecruiterCoursePermission = owcResponseObj.permissions.Any(s => s.Equals(GlobalSettings.reportingTechnicalRecruiterCoursePermissionKey.ToString(), StringComparison.OrdinalIgnoreCase));
            businessUserResponse.IsCampaignsTechnicalRecruiterCoursePermission = owcResponseObj.permissions.Any(s => s.Equals(GlobalSettings.campaignsTechnicalRecruiterCoursePermissionKey.ToString(), StringComparison.OrdinalIgnoreCase));
            businessUserResponse.IsCreateStandardAutomationPermission = owcResponseObj.permissions.Any(s => s.Equals(GlobalSettings.createStandardAutomationPermissionKey.ToString(), StringComparison.OrdinalIgnoreCase));
            businessUserResponse.IsManageElearningPermission = owcResponseObj.permissions.Any(s => s.Equals(GlobalSettings.manageElearningPermissionKey.ToString(), StringComparison.OrdinalIgnoreCase));
            businessUserResponse.IsNPSAutomationPermission = owcResponseObj.permissions.Any(s => s.Equals(GlobalSettings.npsAutomationPermissionKey.ToString(), StringComparison.OrdinalIgnoreCase));
            businessUserResponse.IsContactAutomationReportPermission = owcResponseObj.permissions.Any(s => s.Equals(GlobalSettings.contactAutomationReportPermissionKey.ToString(), StringComparison.OrdinalIgnoreCase));
            businessUserResponse.Domain = domain;
            businessUserResponse.IsWebChatbotPermission = owcResponseObj.permissions.Any(s => s.Equals(GlobalSettings.webChatbotPermissionKey.ToString(), StringComparison.OrdinalIgnoreCase));
            businessUserResponse.IsContactTagsPermission = owcResponseObj.permissions.Any(s => s.Equals(GlobalSettings.contactTagsPermissionKey.ToString(), StringComparison.OrdinalIgnoreCase));
            businessUserResponse.IsWhatsAppAsSMSPermission = owcResponseObj.permissions.Any(s => s.Equals(GlobalSettings.whatsAppAsSMSPermissionKey.ToString(), StringComparison.OrdinalIgnoreCase));
            businessUserResponse.IsJRSalesforceEnabled = owcResponseObj.permissions.Any(s => s.Equals(GlobalSettings.isJRSalesforceEnabled, StringComparison.OrdinalIgnoreCase));

            if (owcCompanyResponseObj != null)
            {
                businessUserResponse.CompanyId = owcCompanyResponseObj.id;
                businessUserResponse.CompanyName = owcCompanyResponseObj.companyName;
                businessUserResponse.PrimaryBrandingColor = owcCompanyResponseObj.primaryBrandingColor;
                businessUserResponse.SecondaryBrandingColor = owcCompanyResponseObj.secondaryBrandingColor;
                businessUserResponse.TertiaryColor = owcCompanyResponseObj.tertiaryColor;
                businessUserResponse.LogoUrl = owcCompanyResponseObj.logoUrl;
                businessUserResponse.CloudinaryBaseFolder = owcCompanyResponseObj.cloudinaryBaseFolder;
                businessUserResponse.UiFooter = owcCompanyResponseObj.uiFooter;
                businessUserResponse.CreateAcademyCourseEnabled = owcCompanyResponseObj.CreateAcademyCourseEnabled;
                businessUserResponse.CreateTemplateEnabled = owcCompanyResponseObj.CreateTemplateEnabled;
                businessUserResponse.CreateTechnicalRecruiterCourseEnabled = owcCompanyResponseObj.CreateTechnicalRecruiterSkillsEnabled;
            }

            businessUserResponse.HeaderMenus = new List<Helpers.Models.HeaderMenus>();

            foreach (var item in owcResponseObj.headerMenus)
            {
                var subMenus = new List<Helpers.Models.HeaderMenus.SubMenus>();

                foreach (var submenu in item.Menus)
                {
                    subMenus.Add(new Helpers.Models.HeaderMenus.SubMenus
                    {
                        Level = submenu.Level,
                        MenuText = submenu.MenuText,
                        MenueUrl = submenu.MenueUrl
                    });
                }

                businessUserResponse.HeaderMenus.Add(new Helpers.Models.HeaderMenus
                {
                    Culture = item.Culture,
                    Menus = subMenus
                });
            }


            businessUserResponse.HeaderMenusNew = new List<Helpers.Models.HeaderMenusNew>();

            if (owcResponseObj.headerMenusNew != null)
            {
                foreach (var item in owcResponseObj.headerMenusNew)
                {
                    var menuList = new List<Helpers.Models.SubMenu>();

                    foreach (var menu in item.Menus)
                    {
                        var subMenusList = new List<Helpers.Models.SubMenu>();

                        if (menu.subMenus != null)
                        {
                            foreach (var subMenuObj in menu.subMenus)
                            {
                                var childSubMenusList = new List<Helpers.Models.SubMenu>();
                                if (subMenuObj.subMenus != null)
                                {
                                    foreach (var childSubMenusObj in subMenuObj.subMenus)
                                    {
                                        childSubMenusList.Add(new Helpers.Models.SubMenu
                                        {
                                            url = childSubMenusObj.url,
                                            name = childSubMenusObj.name,
                                            placement = childSubMenusObj.placement,
                                            sortOrder = childSubMenusObj.sortOrder,
                                            subMenus = null
                                        });
                                    }
                                }
                                subMenusList.Add(new Helpers.Models.SubMenu
                                {
                                    url = subMenuObj.url,
                                    name = subMenuObj.name,
                                    placement = subMenuObj.placement,
                                    sortOrder = subMenuObj.sortOrder,
                                    subMenus = childSubMenusList
                                });
                            }
                        }

                        menuList.Add(new Helpers.Models.SubMenu
                        {
                            url = menu.url,
                            name = menu.name,
                            placement = menu.placement,
                            sortOrder = menu.sortOrder,
                            subMenus = subMenusList,
                            isHighlight = ((string.IsNullOrEmpty(module) || module.EqualsCI(GlobalSettings.module.ToString())) && menu.name.EqualsCI(GlobalSettings.automationMenu.ToString())) 
                                              || (!string.IsNullOrEmpty(module) && module.EqualsCI(GlobalSettings.module_Elearning.ToString()) && menu.name.EqualsCI(GlobalSettings.elearningMenu.ToString()))
                        });
                    }

                    businessUserResponse.HeaderMenusNew.Add(new Helpers.Models.HeaderMenusNew
                    {
                        Culture = item.Culture,
                        Menus = menuList
                    });
                }
            }

            businessUserResponse.OfficeList = new List<Offices>();

            foreach (var item in owcResponseObj.offices)
            {
                businessUserResponse.OfficeList.Add(item);
            }

            businessUserResponse.officesParentChildList = new List<Offices>();

            if (owcResponseObj.officesParentChild != null)
            {
                foreach (var item in owcResponseObj.officesParentChild)
                {
                    businessUserResponse.officesParentChildList.Add(item);
                }
            }

            businessUserResponse.PreferredOffices = new List<Offices>();

            if (owcResponseObj.preferredOffices != null)
            {
                foreach (var item in owcResponseObj.preferredOffices)
                {
                    businessUserResponse.PreferredOffices.Add(item);
                }
            }

            return businessUserResponse;
        }
    }

    public class BusinessUserResponseForOffice
    {
        public int BusinessUserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }

        public BusinessUserResponseForOffice MapEntityToResponse(OWCBusinessUserResponse owcResponseObj, string token, OWCCompanyResponse owcCompanyResponseObj = null, string module = null)
        {
            BusinessUserResponseForOffice businessUserResponse = new BusinessUserResponseForOffice();

            businessUserResponse.BusinessUserId = owcResponseObj.userId;
            businessUserResponse.FirstName = owcResponseObj.firstName;
            businessUserResponse.LastName = owcResponseObj.lastName;
            businessUserResponse.UserName = owcResponseObj.userName;
            return businessUserResponse;
        }
    }
}
