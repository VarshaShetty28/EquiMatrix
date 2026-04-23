using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackEnd.Models
{
    public class AuditLog
    {
        [Key]
        public int AuditLogId { get; set; }

        [Required]
        public string Action_Type { get; set; } = null!; 
        // e.g. Login, CreateAward, ApproveExercise

        [Required]
        public string Entity_Type { get; set; } = null!;
        // e.g. Employee, Award, ExerciseRequest

        public int? Entity_Id { get; set; }
        // Optional: AwardId, RequestId, etc.

        [ForeignKey(nameof(Employee))]
        public int Employee_EmpId { get; set; }
        public Employee? Employee { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}