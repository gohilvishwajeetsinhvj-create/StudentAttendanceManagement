using FluentValidation;
using StudentAttendanceManagement.Models;

namespace StudentAttendanceManagement.Validators
{
    public class CourseValidator : AbstractValidator<Course>
    {
        public CourseValidator() {


           

            RuleFor(x => x.CourseName).NotEmpty().WithMessage("CourseName is Requied");

            RuleFor(x => x.CourseCode).NotEmpty().WithMessage("CourseCode is Requied");

            RuleFor(x => x.Credits).NotEmpty().WithMessage("Credits is Requied");

           
        }
    }
}
