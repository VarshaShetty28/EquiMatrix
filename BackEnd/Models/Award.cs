using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackEnd.Models
{
    public class Award
    {
        [Key]
        public int AwardId { get; set; }

        [Required, MaxLength(45)]
        public string? Award_Type { get; set; } // ESOP / RSU

        public DateTime Grant_Date { get; set; }

        public int Total_Units { get; set; }

        public double Exercise_Price { get; set; }

        // Foreign Key
        [ForeignKey(nameof(Employee))]
        public int Employee_EmpId { get; set; }

        public Employee? Employee { get; set; }

        // Navigation
        public ICollection<ExerciseRequest> ExerciseRequests { get; set; } = new List<ExerciseRequest>();
        public ICollection<VestingSchedule> VestingSchedules { get; set; } = new List<VestingSchedule>();
    }
}