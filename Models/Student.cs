using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StudentAttendanceManagement.Models;

public partial class Student
{
    public int StudentId { get; set; }   // ✅ Primary Key

    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string RollNumber { get; set; } = null!;
    public string ClassName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public DateTime Createddate { get; set; }
    public DateTime Modifydate { get; set; }
    public int UserId { get; set; }

    [JsonIgnore]
    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    [JsonIgnore]
    public virtual User? User { get; set; } = null!;
}
