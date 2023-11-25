using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Response
{
    public class CompanyResponse
    {
        public string CompanyCode { get; set; }
        public string JobRocketApiUrl { get; set; }
        public string JobRocketApiAuthorizationBearer { get; set; }
        public string JobRocketClientUrl { get; set; }

        public CompanyResponse MapEntityToResponse(OWCCompanyResponse owcCompanyResponseObj)
        {
            CompanyResponse companyResponse = new CompanyResponse()
            {
                CompanyCode = owcCompanyResponseObj.clientCode,
                JobRocketApiUrl = owcCompanyResponseObj.jobRocketApiUrl,
                JobRocketApiAuthorizationBearer = owcCompanyResponseObj.jobRocketApiAuthorizationBearer,
                JobRocketClientUrl = owcCompanyResponseObj.jobRocketClientUrl
            };
            return companyResponse;
        }
    }
}