using FluentValidation;
using StudentAttendanceManagement.Models;

namespace StudentAttendanceManagement.Validators
{
    public class TeacherValidator : AbstractValidator<Teacher>
    {
        public TeacherValidator() { 
        
         RuleFor(x => x.FirstName).NotEmpty().WithMessage("FirstName is Requied");

        RuleFor(x => x.LastName).NotEmpty().WithMessage("LastName is Requied");

        RuleFor(x => x.Email).NotEmpty().WithMessage("Email is Requied");

        RuleFor(x => x.Phone).NotEmpty().WithMessage("Phone is Requied").MaximumLength(100);

         RuleFor(x => x.Department).NotEmpty().WithMessage("Department is Requied");

        }

       

    }
}
