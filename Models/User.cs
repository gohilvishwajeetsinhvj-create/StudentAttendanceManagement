using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace StudentAttendanceManagement.Models
{
    public partial class User
    {
        [Key]
        public int UserId { get; set; }

        [Required, StringLength(50)]
        public string Username { get; set; } = null!;

        [Required, StringLength(100)]
        public string Password { get; set; } = null!;  // 🔐 Later: store as hashed

        [StringLength(20)]
        public string? Role { get; set; } = "User";

        [StringLength(255)]
        public string? ImageUrl { get; set; }   // ✅ New column (matches DB)

        [NotMapped]
        [JsonIgnore]
        public IFormFile? ImageFile { get; set; }  // ✅ Match frontend naming 

        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
        public DateTime ModifyDate { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

        [JsonIgnore]
        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

        [JsonIgnore]
        public virtual ICollection<Student> Students { get; set; } = new List<Student>();

        [JsonIgnore]
        public virtual ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
    }

    public class UserDto
    {
        public int UserId { get; set; }

        [Required, StringLength(50)]
        public string Username { get; set; } = null!;

        [Required, StringLength(100)]
        public string Password { get; set; } = null!; // ⚠️ Will be hashed before saving

        [StringLength(20)]
        public string? Role { get; set; } = "User";

        [StringLength(255)]
        public string? ImageUrl { get; set; }

        // ✅ Not mapped, only used for file uploads
        [JsonIgnore]
        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
        public DateTime ModifyDate { get; set; } = DateTime.UtcNow;
    }

    public static class ImageHelper
    {
        public static string directory = "Images";
        public static string SaveImageToFile(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return null;
            if (!Directory.Exists($"wwwroot/{directory}"))
            {
                Directory.CreateDirectory($"wwwroot/{directory}");
            }

            string fullPath = $"{directory}/{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";

            using (var stream = new FileStream($"wwwroot/{fullPath}", FileMode.Create))
            {
                imageFile.CopyTo(stream);
            }

            return fullPath;
        }

        public static string DeleteFile(string filePath)
        {
            var path = $"{Directory.GetCurrentDirectory()}/wwwroot/{filePath}";

            if (!System.IO.File.Exists(path)) return "File not found.";

            try
            {
                System.IO.File.Delete(path);
                return "File deleted successfully.";
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }

    public class UserLogin
    {
        public string name { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }

}
