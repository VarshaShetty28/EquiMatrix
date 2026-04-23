using System.ComponentModel.DataAnnotations;

namespace BackEnd.DTOs
{
    public class UpdateEmployeeDto
    {
        [Required]
        public string EmpName { get; set; } = null!;

        [Required, EmailAddress]
        public string EmpEmail { get; set; } = null!;

        [Required]
        public string Role { get; set; } = null!;
    }
}
