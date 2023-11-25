using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Services.Model
{
    public class Base
    {
        public long OffsetValue { get; set; }
        public List<object> ValidationErrors { get; set; }
        public ValidationResult ValidationResult { get; set; }
    }
}