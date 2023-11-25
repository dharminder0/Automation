using QuizApp.Helpers;
using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizApp.Services.Service
{
    public interface IBusinessUserService
    {
        ResultEnum Status { get; set; }
        string ErrorMessage { get; set; }
        BusinessUser AuthorizeToken(string jwtToken, string companyCode = null);
        void SaveUpdateUserInfo(BusinessUser businessUser);
        object GetClientCountries(string clientCode);
    }
}
