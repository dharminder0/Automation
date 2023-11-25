using QuizApp.Helpers;
using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace QuizApp.Services.Service
{
    public interface IUpdateBrandingService
    {
        ResultEnum Status { get; set; }
        string ErrorMessage { get; set; }
        void UpdateBrandingAndStyle(QuizBrandingAndStyleModel BrandingAndStyleObj, int BusinessUserId, int CompanyId);
    }
}