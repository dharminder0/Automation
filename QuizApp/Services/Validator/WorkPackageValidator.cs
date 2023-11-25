using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Services.Validator
{
    public class WorkPackageValidator : AbstractValidator<Model.WorkPackage>
    {
        public WorkPackageValidator()
        {
            RuleFor(a => a.LeadUserId).NotEmpty().WithMessage("LeadUserId is required.");
            RuleFor(a => a.QuizId).NotEmpty().WithMessage("QuizId is required.");
        }
    }

    public class PushWorkPackageValidator : AbstractValidator<Model.PushWorkPackage>
    {
        public PushWorkPackageValidator()
        {
            RuleFor(a => a.ContactIds).NotEmpty().WithMessage("ContactIds is required.");
            RuleFor(a => a.ConfigurationId).NotEmpty().WithMessage("ConfigurationId is required.");
            RuleFor(a => a.CompanyCode).NotEmpty().WithMessage("CompanyCode is required.");
        }
    }
}