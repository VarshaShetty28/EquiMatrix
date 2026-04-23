using System.ComponentModel.DataAnnotations;

namespace BackEnd.DTOs
{
    public class CreateAuditLogDto
    {
        [Required]
        public string Action_Type { get; set; } = null!;

        [Required]
        public string Entity_Type { get; set; } = null!;

        public int? Entity_Id { get; set; }

        [Required]
        public int Employee_EmpId { get; set; }
    }
}