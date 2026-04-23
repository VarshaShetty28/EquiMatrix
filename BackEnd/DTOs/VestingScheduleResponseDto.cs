using System;

namespace BackEnd.DTOs
{
    public class VestingScheduleResponseDto
    {
        public int Vesting_Id { get; set; }
        public int Awards_AwardId { get; set; }
        public string Type { get; set; } = string.Empty;
        public double AwardPrice { get; set; }
        public int Units_Vested { get; set; }
        public int Cumulative_Vested { get; set; }
        public int Remaining_Unvested { get; set; }
        public DateTime Vesting_Date { get; set; }
        public DateTime Final_Vesting_Date { get; set; }
        public double Current_Value { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}