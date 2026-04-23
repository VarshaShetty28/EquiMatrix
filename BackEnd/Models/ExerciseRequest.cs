using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackEnd.Models
{
    public class ExerciseRequest
    {
        [Key]
        public int RequestId { get; set; }

        public int Units_Requested { get; set; }

        public DateTime Requested_Date { get; set; }

        [Required, MaxLength(45)]
        public string? Status { get; set; } // Pending / Accepted / Rejected

        public decimal CurrentSharePrice { get; set; }

        public decimal ExerciseAmountUsd { get; set; }

        public decimal TaxableGainUsd { get; set; }

        public decimal TaxAmountUsd { get; set; }

        public decimal NetAmountUsd { get; set; }

        // Foreign Keys
        [ForeignKey(nameof(Employee))]
        public int Employee_EmpId { get; set; }
        public Employee? Employee { get; set; }

        [ForeignKey(nameof(Award))]
        public int Awards_AwardId { get; set; }
        public Award? Award { get; set; }

        [ForeignKey(nameof(VestingSchedule))]
        public int? VestingScheduleId { get; set; }
        public VestingSchedule? VestingSchedule { get; set; }
    }
}