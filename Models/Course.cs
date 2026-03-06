using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StudentAttendanceManagement.Models;

public partial class Course
{
    public int CourseId { get; set; }

    public string CourseName { get; set; } = null!;

    public string CourseCode { get; set; } = null!;

    public int Credits { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime ModifyDate { get; set; }

    public int UserId { get; set; }

    [JsonIgnore]
    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    [JsonIgnore]
    public virtual User? User { get; set; } = null!;
}
