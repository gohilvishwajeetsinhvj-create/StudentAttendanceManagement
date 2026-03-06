using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StudentAttendanceManagement.Models;

public partial class Teacher
{
    public int TeacherId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Department { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public DateTime ModifyDate { get; set; }

    public int UserId { get; set; }

    [JsonIgnore]
    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    [JsonIgnore]
    public virtual User? User { get; set; } = null!;
}
