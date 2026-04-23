using System.ComponentModel.DataAnnotations;

namespace BackEnd.DTOs
{
    public class CreateNotificationDto
    {
        [Required]
        public int Employee_EmpId { get; set; }

        [Required]
        public string Message { get; set; } = null!;

        [Required]
        public string Type { get; set; } = null!; // Info / Alert
    }
}