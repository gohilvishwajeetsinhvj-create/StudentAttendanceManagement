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
    
    public class AttendanceController : ControllerBase
    {
        #region Configuration Fields
        private readonly StudentAttendanceManagementContext _context;
        public AttendanceController(StudentAttendanceManagementContext context)
        {
            _context = context;
        }
        #endregion

        #region GetAllAttendance
        
        [HttpGet]
        public async Task<IActionResult> GetAttendance()
        {
            var att = await _context.Attendances
                .Include(a => a.Student)
                .Include(a => a.Course)
                .Include(a => a.Teacher)
                .ToListAsync();

            var response = att.Select(a => new
            {
                a.AttendanceId,
                a.Date,
                a.Status,
                StudentName = a.Student.FirstName + " " + a.Student.LastName,
                CourseName = a.Course.CourseName,
                TeacherName = a.Teacher.FirstName + " " + a.Teacher.LastName
            });

            return Ok(response);
        }

        #endregion

        #region GetAttendanceById
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAttendanceById(int id)
        {
            var att = await _context.Attendances.FindAsync(id);
            if (att == null)
            {
                return NotFound();
            }
            return Ok(att);
        }
        #endregion

        #region DeleteAttendanceById
       
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAttendanceById(int id)
        {
            var att = await _context.Attendances.FindAsync(id);
            if (att == null)
            {
                return NotFound();
            }

            _context.Attendances.Remove(att);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        #endregion

        #region InsertAttendance
      
        [HttpPost]
        public async Task<IActionResult> InsertAttendance(Attendance att)
        {
            att.UserId = 1;
            await _context.Attendances.AddAsync(att);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        #endregion


        #region
     
        [HttpGet("TeacherWiseAttendance")]
        public async Task<IActionResult> GetTeacherWiseAttendance()
        {
            var attendanceList = await _context.Attendances
                .Include(a => a.Teacher)
                .Include(a => a.Student)
                .Include(a => a.Course)
                .GroupBy(a => new { a.TeacherId, a.Teacher.FirstName })
                .Select(g => new
                {
                    TeacherId = g.Key.TeacherId,
                    TeacherName = g.Key.FirstName,
                    Attendances = g.Select(a => new
                    {
                        a.AttendanceId,
                        a.Date,
                        a.Status,
                        StudentName = a.Student.FirstName + " " + a.Student.LastName,
                        CourseName = a.Course.CourseName
                    }).ToList()
                })
                .ToListAsync();

            return Ok(attendanceList);
        }





        #endregion

        #region UpdateAttendance
       
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAttendance(int id, Attendance att)
        {
            var existingAttendance = await _context.Attendances.FindAsync(id);
            if (existingAttendance == null)
                return NotFound();

            existingAttendance.Date = att.Date;
            existingAttendance.Status = att.Status;
            existingAttendance.StudentId = att.StudentId;
            existingAttendance.CourseId = att.CourseId;
            existingAttendance.TeacherId = att.TeacherId;

            await _context.SaveChangesAsync();
            return NoContent();
        }


        #endregion

        #region Filter
       
        [HttpGet("filter")]
        public async Task<IActionResult> FilterAttendance(
            int? studentId,
            int? courseId,
            int? teacherId,
            string? status,
            DateTime? date,
            int? userId)
        {
            var query = _context.Attendances
                .Include(a => a.Student)
                .Include(a => a.Course)
                .Include(a => a.Teacher)
                .Include(a => a.User)
                .AsQueryable();

            if (studentId.HasValue)
                query = query.Where(a => a.StudentId == studentId.Value);

            if (courseId.HasValue)
                query = query.Where(a => a.CourseId == courseId.Value);

            if (teacherId.HasValue)
                query = query.Where(a => a.TeacherId == teacherId.Value);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(a => a.Status == status);

            if (date.HasValue)
                query = query.Where(a => a.Date.Date == date.Value.Date);

            if (userId.HasValue)
                query = query.Where(a => a.UserId == userId.Value);

            var result = await query.ToListAsync();
            return Ok(result);
        }
        #endregion

        #region Summary
       
        [HttpGet("summary")]
        public async Task<IActionResult> GetAttendanceSummary(DateTime date)
        {
            var summary = await _context.Attendances
                .Where(a => a.Date.Date == date.Date)
                .GroupBy(a => new { a.CourseId, a.TeacherId })
                .Select(g => new
                {
                    g.Key.CourseId,
                    g.Key.TeacherId,
                    Total = g.Count(),
                    Present = g.Count(x => x.Status == "Present"),
                    Absent = g.Count(x => x.Status == "Absent")
                })
                .ToListAsync();

            return Ok(summary);
        }
        #endregion
       
    }
}
