using System.ComponentModel.DataAnnotations;

namespace BackEnd.DTOs
{
    public class LoginRequestDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;

        [Required]
        public string Role { get; set; } = null!; // Admin / Employee
    }
}