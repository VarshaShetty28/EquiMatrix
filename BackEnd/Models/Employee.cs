using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BackEnd.Models
{
    public class Employee
    {
        [Key]
        public int EmpId { get; set; }

        [Required, MaxLength(45)]
        public string? EmpName { get; set; }

        [Required, MaxLength(45)]
        public string? EmpEmail { get; set; }

        public DateTime EmpDOJ { get; set; }

        [Required, MaxLength(255)]
        public string? Password_Hash { get; set; }

        [Required, MaxLength(45)]
        public string? Role { get; set; }

        // Navigation Properties
        public ICollection<Award>? Awards { get; set; }
        public ICollection<ExerciseRequest>? ExerciseRequests { get; set; }
        public ICollection<VestingSchedule>? VestingSchedules { get; set; }
        public ICollection<Notification>? Notifications { get; set; }
        public ICollection<AuditLog>? AuditLogs { get; set; }
    }
}