using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackEnd.Models
{
    public class VestingSchedule
    {
        [Key]
        public int Vesting_Id { get; set; }

        public DateTime Vesting_Date { get; set; }

        public int Units_Vested { get; set; }

        public int Cumulative_Vested { get; set; }

        [Required]
        public string Status { get; set; } = "Pending"; // Pending / Vested

        [ForeignKey(nameof(Employee))]
        public int Employee_EmpId { get; set; }
        public Employee? Employee { get; set; }

        [ForeignKey(nameof(Award))]
        public int Awards_AwardId { get; set; }
        public Award? Award { get; set; }
    }
}