using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentAttendanceManagement.Models;

namespace StudentAttendanceManagement.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    
    public class StudentController : ControllerBase
    {
        private readonly StudentAttendanceManagementContext _context;

        public StudentController(StudentAttendanceManagementContext context)
        {
            _context = context;
        }

        #region Get All Students (with pagination & search)
        [HttpGet]
        public async Task<IActionResult> GetStudents(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? filterClass = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _context.Students.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(s =>
                    s.FirstName.Contains(searchTerm) ||
                    s.LastName.Contains(searchTerm) ||
                    s.RollNumber.Contains(searchTerm) ||
                    s.Email.Contains(searchTerm));
            }

            if (!string.IsNullOrWhiteSpace(filterClass))
            {
                query = query.Where(s => s.ClassName == filterClass);
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(s => s.StudentId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }
        #endregion

        #region Get Student by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetStudentById(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
                return NotFound();

            return Ok(student);
        }
        #endregion

        #region Insert Student
        [HttpPost]
        public async Task<IActionResult> InsertStudent([FromBody] Student student)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // NOTE: Replace this hardcoded UserId with actual user context
            student.UserId = 1;

            await _context.Students.AddAsync(student);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetStudentById), new { id = student.StudentId }, student);
        }
        #endregion

        #region Update Student
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStudent(int id, [FromBody] Student student)
        {
            if (id != student.StudentId)
                return BadRequest();

            var existingStudent = await _context.Students.FindAsync(id);
            if (existingStudent == null)
                return NotFound();

            existingStudent.FirstName = student.FirstName;
            existingStudent.LastName = student.LastName;
            existingStudent.RollNumber = student.RollNumber;
            existingStudent.ClassName = student.ClassName;
            existingStudent.Email = student.Email;
            existingStudent.Phone = student.Phone;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        #endregion

        #region Delete Student
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudentById(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
                return NotFound();

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        #endregion

        #region Get Student Dropdown
        [HttpGet("dropdown/students")]
        public async Task<IActionResult> GetStudentDropdown()
        {
            var students = await _context.Students
                .Select(s => new
                {
                    s.StudentId,
                    FullName = s.FirstName + " " + s.LastName
                })
                .ToListAsync();

            return Ok(students);
        }
        #endregion

        #region Get Total Student Count
        [HttpGet("TotalStudentCount")]
        public async Task<IActionResult> GetTotalStudentCount()
        {
            var total = await _context.Students.CountAsync();
            return Ok(new { TotalStudents = total });
        }
        #endregion

        #region Search Students (Index)
        [HttpGet("search")]
        public async Task<IActionResult> SearchStudents([FromQuery] string? searchTerm, [FromQuery] string? filterClass)
        {
            var query = _context.Students.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(s => s.FirstName.Contains(searchTerm) || s.RollNumber.Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(filterClass))
            {
                query = query.Where(s => s.ClassName == filterClass);
            }

            var result = await query.ToListAsync();

            var classList = await _context.Students
                .Select(s => s.ClassName)
                .Distinct()
                .ToListAsync();

            return Ok(new
            {
                Students = result,
                ClassList = classList
            });
        }
        #endregion

        #region Attendance History with Filters
        [HttpGet("attendance/history")]
        public async Task<IActionResult> AttendanceHistory([FromQuery] string? searchTerm, [FromQuery] string? filterDate)
        {
            var attendanceQuery = _context.Attendances
                .Include(a => a.Student)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                attendanceQuery = attendanceQuery.Where(a =>
                    (a.Student.FirstName + " " + a.Student.LastName).Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(filterDate) && DateTime.TryParse(filterDate, out DateTime date))
            {
                attendanceQuery = attendanceQuery.Where(a => a.Date.Date == date.Date);
            }

            var attendanceList = await attendanceQuery.ToListAsync();

            return Ok(attendanceList);
        }
        #endregion


    }
}
