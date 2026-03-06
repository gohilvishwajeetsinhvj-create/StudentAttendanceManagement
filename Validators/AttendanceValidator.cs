using FluentValidation;
using StudentAttendanceManagement.Models;

namespace StudentAttendanceManagement.Validators
{
    public class AttendanceValidator : AbstractValidator<Attendance>
    {
        public AttendanceValidator() {


            RuleFor(x => x.Date).NotEmpty().WithMessage("Date is Requied");

            RuleFor(x => x.Status).NotEmpty().WithMessage("Status is Requied");

          

        }
    }
}
