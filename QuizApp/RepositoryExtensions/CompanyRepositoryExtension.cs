using Core.Common.Caching;
using QuizApp.Db;
using QuizApp.RepositoryPattern;
using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.RepositoryExtensions
{
    public static  class CompanyRepositoryExtension
    {
        public static CompanyModel BindCompanyDetails(this GenericRepository<Company> repository, int companyId)
        {
            var companycacheKey = "companyData" + companyId;
            CompanyModel companyObj = new CompanyModel();
            var result = AppLocalCache.GetOrCache(companycacheKey, () =>
            {
                return repository.Get(r => r.CompanyId == companyId).FirstOrDefault();
            });

            if (result != null)
            {
                companyObj.Id = result.Id;
                companyObj.AlternateClientCodes = result.AlternateClientCodes;
                companyObj.ClientCode = result.ClientCode;
                companyObj.CompanyName = result.CompanyName;
                companyObj.CompanyWebsiteUrl = result.CompanyWebsiteUrl;
                companyObj.JobRocketApiAuthorizationBearer = result.JobRocketApiAuthorizationBearer;
                companyObj.JobRocketApiUrl = result.JobRocketApiUrl;
                companyObj.JobRocketClientUrl = result.JobRocketClientUrl;
                companyObj.LeadDashboardApiAuthorizationBearer = result.LeadDashboardApiAuthorizationBearer;
                companyObj.LeadDashboardApiUrl = result.LeadDashboardApiUrl;
                companyObj.LeadDashboardClientUrl = result.LeadDashboardClientUrl;
                companyObj.LogoUrl = result.LogoUrl;
                companyObj.PrimaryBrandingColor = result.PrimaryBrandingColor;
                companyObj.SecondaryBrandingColor = result.SecondaryBrandingColor;

                return companyObj;
            }
            else
            {
                companyObj = new CompanyModel();
            }
            return companyObj;
        }

        public static IEnumerable<Company> GetCompanyByCompanyId(this GenericRepository<Company> repository,int companyId)
        {
            return repository.Get(r => r.CompanyId == companyId);
        }
    }
}