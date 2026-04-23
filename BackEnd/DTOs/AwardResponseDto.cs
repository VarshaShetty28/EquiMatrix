using System;

namespace BackEnd.DTOs
{
    public class AwardResponseDto
    {
        public int AwardId { get; set; }
        public string Award_Type { get; set; } = null!;
        public DateTime Grant_Date { get; set; }
        public int Total_Units { get; set; }
        public double Exercise_Price { get; set; }
        public int Employee_EmpId { get; set; }
    }
}
