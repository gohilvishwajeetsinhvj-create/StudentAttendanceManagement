using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentAttendanceManagement.Models;

namespace StudentAttendanceManagement.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
   
    public class TeacherController : ControllerBase
    {
        private readonly StudentAttendanceManagementContext _context;

        public TeacherController(StudentAttendanceManagementContext context)
        {
            _context = context;
        }

        #region GetAllTeacher (with pagination & search)
        [HttpGet]
        public async Task<IActionResult> GetTeacher(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? department = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _context.Teachers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(t =>
                    t.FirstName.Contains(searchTerm) ||
                    t.LastName.Contains(searchTerm) ||
                    t.Email.Contains(searchTerm));
            }

            if (!string.IsNullOrWhiteSpace(department))
            {
                query = query.Where(t => t.Department == department);
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(t => t.TeacherId)
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

        #region GetTeacherById
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTeacherById(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null)
            {
                return NotFound();
            }
            return Ok(teacher);
        }
        #endregion

        #region DeleteTeacherById
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTeacherById(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null)
            {
                return NotFound();
            }

            _context.Teachers.Remove(teacher);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        #endregion

        #region InsertTeacher
        [HttpPost]
        public async Task<IActionResult> InsertTeacher(Teacher teacher)
        {
            teacher.UserId = 1;
            await _context.Teachers.AddAsync(teacher);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        #endregion

        #region UpdateTeacher
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTeacher(int id, Teacher teacher)
        {
            if (id != teacher.TeacherId)
            {
                return BadRequest();
            }

            var existingTeacher = await _context.Teachers.FindAsync(id);
            if (existingTeacher == null)
            {
                return NotFound();
            }

            existingTeacher.FirstName = teacher.FirstName;
            existingTeacher.LastName = teacher.LastName;
            existingTeacher.Email = teacher.Email;
            existingTeacher.Phone = teacher.Phone;
            existingTeacher.Department = teacher.Department;

            _context.Teachers.Update(existingTeacher);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        #endregion

        #region
        [HttpGet("TotalTeacherCount")]
        public async Task<IActionResult> GetTotalTeacherCount()
        {
            var totalTeachers = await _context.Teachers.CountAsync();

            return Ok(new { TotalTeachers = totalTeachers });
        }

        #endregion

        #region GetTeacherDropdown
        [HttpGet("dropdown/teachers")]
        public async Task<IActionResult> GetTeacherDropdown()
        {
            var teachers = await _context.Teachers
                .Select(t => new
                {
                    t.TeacherId,
                    FullName = t.FirstName + " " + t.LastName
                }).ToListAsync();

            return Ok(teachers);
        }
        #endregion
    }
}
