using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StudentAttendanceManagement.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace StudentAttendanceManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly StudentAttendanceManagementContext _context;
        private readonly IConfiguration _configuration;

        public UserController(StudentAttendanceManagementContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        #region 🔑 Generate JWT
        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
        new Claim(ClaimTypes.Name, user.Username ?? string.Empty),
        new Claim("Password", user.Password ?? string.Empty), // ⚠️ Not recommended for security
        new Claim(ClaimTypes.Role, user.Role ?? "User")
    };

            var expiryMinutes = Convert.ToDouble(jwtSettings["TokenExpiryMinutes"]);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        #endregion

        #region ✅ Login
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User loginUser)
        {
            if (loginUser == null || string.IsNullOrWhiteSpace(loginUser.Username) || string.IsNullOrWhiteSpace(loginUser.Password))
            {
                return BadRequest(new { message = "Username and password are required." });
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Username.ToLower() == loginUser.Username.ToLower() &&
                    u.Password == loginUser.Password);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            // ✅ Ensure Role always has a value
            if (string.IsNullOrEmpty(user.Role))
                user.Role = "User";

            // ✅ Generate JWT Token
            var token = GenerateJwtToken(user);

            return Ok(new
            {
                token,
                user = new
                {
                    user.UserId,
                    user.Username,
                    user.Role
                }
            });
        }
        #endregion

        #region ✅ Get All Users
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users.ToListAsync();
            
            // Build absolute URLs for images
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}".TrimEnd('/');
            var usersWithFullUrls = users.Select(user => new
            {
                user.UserId,
                user.Username,
                user.Password,
                user.Role,
                user.CreateDate,
                user.ModifyDate,
                ImageUrl = string.IsNullOrEmpty(user.ImageUrl) ? null : $"{baseUrl}/{user.ImageUrl}"
            }).ToList();
            
            return Ok(usersWithFullUrls);
        }
        #endregion

        #region ✅ Get User by ID
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            // Build absolute URL for image
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}".TrimEnd('/');
            var userWithFullUrl = new
            {
                user.UserId,
                user.Username,
                user.Password,
                user.Role,
                user.CreateDate,
                user.ModifyDate,
                ImageUrl = string.IsNullOrEmpty(user.ImageUrl) ? null : $"{baseUrl}/{user.ImageUrl}"
            };

            return Ok(userWithFullUrl);
        }
        #endregion

        #region ✅ Insert User with Image Upload (Admin only)
        [HttpPost("insert-with-image")]
        public async Task<IActionResult> InsertUserWithImage([FromForm] UserDto dto)
        {
            try
            {
                string savedPath = string.Empty;

                // Handle image upload
                if (dto.ImageFile != null && dto.ImageFile.Length > 0)
                {
                    savedPath = ImageHelper.SaveImageToFile(dto.ImageFile); // returns "uploads/unique.jpg"
                    if (string.IsNullOrEmpty(savedPath))
                        return BadRequest("Failed to upload image.");
                }

                // Save only relative path in DB
                var user = new User
                {
                    Username = dto.Username,
                    Password = dto.Password, // ⚠️ hash before saving in production
                    Role = dto.Role ?? "User",
                    ImageUrl = savedPath,
                    CreateDate = DateTime.Now,
                    ModifyDate = DateTime.Now
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Build absolute URL for API response
                var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}".TrimEnd('/');
                var imageFullUrl = string.IsNullOrEmpty(user.ImageUrl) ? null : $"{baseUrl}/{user.ImageUrl}";

                return Ok(new
                {
                    user.UserId,
                    user.Username,
                    user.Role,
                    user.CreateDate,
                    ImageUrl = imageFullUrl // Full URL for frontend
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error occurred while inserting user.", error = ex.Message });
            }
        }
        #endregion

        #region ✅ Update User with Image (Admin only)
        [HttpPut("update-with-image/{id}")]
        public async Task<IActionResult> UpdateUserWithImage(int id, [FromForm] UserDto dto)
        {
            try
            {
                var existingUser = await _context.Users.FindAsync(id);

                if (existingUser == null)
                    return NotFound(new { message = $"User with ID {id} not found." });

                // ✅ Check if username is already taken by another user
                if (!string.IsNullOrEmpty(dto.Username))
                {
                    bool usernameExists = await _context.Users
                        .AnyAsync(u => u.Username == dto.Username && u.UserId != id);

                    if (usernameExists)
                        return Conflict(new { message = $"Username '{dto.Username}' is already taken." });

                    existingUser.Username = dto.Username;
                }

                // Update other properties
                if (!string.IsNullOrEmpty(dto.Password))
                    existingUser.Password = dto.Password; // ⚠️ Hash in real apps

                if (!string.IsNullOrEmpty(dto.Role))
                    existingUser.Role = dto.Role;

                existingUser.ModifyDate = DateTime.Now;

                // ✅ Handle image update
                if (dto.ImageFile != null && dto.ImageFile.Length > 0)
                {
                    // Delete old image if it exists
                    if (!string.IsNullOrEmpty(existingUser.ImageUrl))
                    {
                        try
                        {
                            ImageHelper.DeleteFile(existingUser.ImageUrl);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Image delete error: {ex.Message}");
                        }
                    }

                    // Save new image (relative path)
                    var savedPath = ImageHelper.SaveImageToFile(dto.ImageFile);
                    if (string.IsNullOrEmpty(savedPath))
                        return BadRequest("Failed to upload image.");

                    existingUser.ImageUrl = savedPath;
                }

                await _context.SaveChangesAsync();

                // Build absolute URL for frontend response
                var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}".TrimEnd('/');
                var imageFullUrl = string.IsNullOrEmpty(existingUser.ImageUrl) ? null : $"{baseUrl}/{existingUser.ImageUrl}";

                return Ok(new
                {
                    existingUser.UserId,
                    existingUser.Username,
                    existingUser.Role,
                    existingUser.CreateDate,
                    existingUser.ModifyDate,
                    ImageUrl = imageFullUrl
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Users.Any(u => u.UserId == id))
                    return NotFound(new { message = $"User with ID {id} not found." });
                else
                    return BadRequest(new { message = "Concurrency error occurred while updating the user." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error occurred while updating user with ID {id}.", error = ex.Message });
            }
        }
        #endregion

        #region ✅ Update User (Admin only) - JSON only
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User user)
        {
            if (id != user.UserId)
                return BadRequest("User ID mismatch.");

            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
                return NotFound();

            existingUser.Username = user.Username;
            existingUser.Password = user.Password;
            existingUser.Role = user.Role;
            existingUser.ModifyDate = DateTime.Now;

            _context.Users.Update(existingUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User updated successfully" });
        }
        #endregion

        #region ✅ Delete User (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User deleted successfully" });
        }
        #endregion
    }
}
