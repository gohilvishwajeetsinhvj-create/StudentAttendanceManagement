using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StudentAttendanceManagement.Models;

public partial class Attendance
{
    public int AttendanceId { get; set; }

    public int StudentId { get; set; }

    public int CourseId { get; set; }

    public int TeacherId { get; set; }

    public DateTime Date { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public DateTime Modifydate { get; set; }

  

    public int UserId { get; set; }

    [JsonIgnore]
    public virtual Course? Course { get; set; } = null!;

    [JsonIgnore]
    public virtual Student? Student { get; set; } = null!;

    [JsonIgnore]
    public virtual Teacher? Teacher { get; set; } = null!;

    [JsonIgnore]
    public virtual User? User { get; set; } = null!;
}
