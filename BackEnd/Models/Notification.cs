using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackEnd.Models
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        [Required, MaxLength(255)]
        public string? Message { get; set; }

        [Required, MaxLength(45)]
        public string? Type { get; set; } // Info / Alert / Warning

        public bool Is_Read { get; set; }
        
        public DateTime CreatedDate { get; set; }

        // Foreign Key
        [ForeignKey(nameof(Employee))]
        public int Employee_EmpId { get; set; }
        public Employee? Employee { get; set; }
    }
}