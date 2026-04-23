using System;
using System.ComponentModel.DataAnnotations;

namespace BackEnd.DTOs
{
    public class CreateVestingScheduleDto
    {
        [Required]
        public DateTime Vesting_Date { get; set; }

        [Required]
        public int Units_Vested { get; set; }

        public int Cumulative_Vested { get; set; }

        [Required]
        public string Status { get; set; } = "Pending";

        [Required]
        public int Awards_AwardId { get; set; }
    }
}