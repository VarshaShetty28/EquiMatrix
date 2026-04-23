namespace BackEnd.DTOs
{
    public class EmployeeDashboardDto
    {
        public decimal TotalEquityGranted { get; set; } // Sum of Total_Units from Awards
        public decimal ShareValue { get; set; } // (Exercised ESOP + Vested RSU) * FMV
    }
}