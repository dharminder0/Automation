using FluentValidation;
using QuizApp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Services.Validator
{
    public class EmailListValidator : AbstractValidator<string>
    {
        public EmailListValidator()
        {
            RuleFor(x => x)
                .EmailAddress()
                .WithMessage("AccessibleUser must have valid emails.");
        }
    }
    public class CreateQuizValidator : AbstractValidator<Model.LocalQuiz>
    {
        public CreateQuizValidator()
        {
            RuleFor(a => a.QuizType).NotEmpty().WithMessage("QuizType is required.");

            RuleFor(a => a.AccessibleOfficeId)
                .Must((model, a) => !string.IsNullOrEmpty(a))
                .WithMessage("OfficeId is required.")
                .When(a => (a.QuizType != QuizTypeEnum.AssessmentTemplate && a.QuizType != QuizTypeEnum.ScoreTemplate && a.QuizType != QuizTypeEnum.PersonalityTemplate) && !a.IsCreateStandardAutomation);

            RuleFor(a => a.IsCreateStandardAutomation)
               .Must((model, a) => a)
               .WithMessage("or Create companywide automation permission is required.")
               .When(a => (a.QuizType != QuizTypeEnum.AssessmentTemplate && a.QuizType != QuizTypeEnum.ScoreTemplate && a.QuizType != QuizTypeEnum.PersonalityTemplate) && string.IsNullOrEmpty(a.AccessibleOfficeId));

            RuleFor(a => a.CategoryId)
                .Must((model, a) => (a != null))
                .WithMessage("CategoryId is required.")
                .When(a => (a.QuizType == QuizTypeEnum.AssessmentTemplate || a.QuizType == QuizTypeEnum.ScoreTemplate || a.QuizType == QuizTypeEnum.PersonalityTemplate));
        }
    }

    public class UpdateleadDataInActionValidator : AbstractValidator<List<string>>
    {
        public UpdateleadDataInActionValidator()
        {

            RuleForEach(a => a)
              .Must((model, a) => a != null)
              .WithMessage("AccessibleUser must not be empty.")
              .SetValidator(new EmailListValidator())
              .When(a => a.Any(b => !string.IsNullOrEmpty(b)));

            // .Must(y => !string.IsNullOrEmpty(y.Name))
            //.WithMessage((testClass, testProperty) => $"TestProperty {testProperty.Id} name can't be null or empty");



            //RuleFor(a => a)
            //    .Must((model, a) => a != null && a.Count > 0)
            //    .WithMessage("AccessibleUser must not be empty.")
            //    .SetCollectionValidator(new EmailListValidator())
            //    .When(a => a.Any(b => !string.IsNullOrEmpty(b)));

        }
    }



}