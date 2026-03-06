using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentAttendanceManagement.Models;

namespace StudentAttendanceManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
   
    public class CourseController : ControllerBase
    {
       
        private readonly StudentAttendanceManagementContext _context;

        public CourseController(StudentAttendanceManagementContext context)
        {
            _context = context;
        }

        #region GetAllCourse
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetCourse()
        {
            var courses = await _context.Courses.ToListAsync();
            return Ok(courses);
        }
        #endregion

        #region GetCourseById
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCourseById(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            return Ok(course);
        }
        #endregion

        #region DeleteCourseById
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourseById(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        #endregion

        #region InsertCourse
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> InsertCourse(Course course)
        {
            course.UserId = 1;
            await _context.Courses.AddAsync(course);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        #endregion

        #region UpdateCourse
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCourse(int id, Course course)
        {
            if (id != course.CourseId)
            {
                return BadRequest();
            }

            var existingCourse = await _context.Courses.FindAsync(id);
            if (existingCourse == null)
            {
                return NotFound();
            }

            existingCourse.CourseName = course.CourseName;
            existingCourse.CourseCode = course.CourseCode;
            existingCourse.Credits = course.Credits;

            _context.Courses.Update(existingCourse);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        #endregion

        #region Get Total Course Count
        [Authorize]
        [HttpGet("TotalCourseCount")]
       
        public async Task<IActionResult> GetTotalCourseCount()
        {
            var total = await _context.Courses.CountAsync();
            return Ok(new { TotalCourses = total });
        }
        #endregion

        #region GetCourseDropdown
        [Authorize]
        [HttpGet("dropdown/courses")]
  
        public async Task<IActionResult> GetCourseDropdown()
        {
            var courses = await _context.Courses
                .Select(c => new
                {
                    c.CourseId,
                    c.CourseName
                })
                .ToListAsync();

            return Ok(courses);
        }
        #endregion
    }
}
