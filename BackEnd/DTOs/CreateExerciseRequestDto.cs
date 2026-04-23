using System;
using System.ComponentModel.DataAnnotations;

namespace BackEnd.DTOs
{
    public class CreateExerciseRequestDto
    {
        [Required]
        public int Awards_AwardId { get; set; }

        public int? VestingScheduleId { get; set; }

        [Required]
        public int Units_Requested { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Current share price must be greater than zero.")]
        public decimal CurrentSharePrice { get; set; }
    }
}