using FluentValidation;
using StudentAttendanceManagement.Models;

namespace StudentAttendanceManagement.Validator
{
    public class StudentValidator : AbstractValidator<Student>
    {
        public StudentValidator()
        {

            RuleFor(x => x.FirstName).NotEmpty().WithMessage("FirstName is Requied");

            RuleFor(x => x.LastName).NotEmpty().WithMessage("LastName is Requied");

            RuleFor(x => x.RollNumber).NotEmpty().WithMessage("Rollnummber is Requied");

            RuleFor(x => x.ClassName).NotEmpty().WithMessage("classname is Requied");

            RuleFor(x => x.Email).NotEmpty().WithMessage("Email is Requied").EmailAddress();

            RuleFor(x => x.Phone).NotEmpty().WithMessage("Phone is Requied").MaximumLength(100);



        }
    }
}
