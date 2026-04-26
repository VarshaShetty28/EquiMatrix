using System;

namespace BackEnd.DTOs
{
    public class ExerciseRequestResponseDto
    {
        public int RequestId { get; set; }
        public int Awards_AwardId { get; set; }
        public int? VestingScheduleId { get; set; }
        public int Units_Requested { get; set; }
        public DateTime Requested_Date { get; set; }
        public string Status { get; set; } = null!;
        public int Employee_EmpId { get; set; }
        public string Currency { get; set; } = "USD";
        public string CurrencySymbol { get; set; } = "$";
        public decimal ExerciseAmountUsd { get; set; }
        public decimal TaxableGainUsd { get; set; }
        public decimal TaxAmountUsd { get; set; }
        public decimal NetAmountUsd { get; set; }
    }
}