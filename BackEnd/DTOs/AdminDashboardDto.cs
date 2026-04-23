namespace BackEnd.DTOs
{
    public class AdminDashboardDto
    {
        public int TotalEmployees { get; set; }
        public int TotalAwardsGranted { get; set; }
        public int TotalVestedUnits { get; set; }
        public int PendingRequests { get; set; }
        public int EsopCount { get; set; }
        public int RsuCount { get; set; }
        public int EsopPercentage { get; set; }
        public int RsuPercentage { get; set; }
        public List<AdminDashboardEmployee> Employees { get; set; } = new();
    }

    public class AdminDashboardEmployee
    {
        public int EmpId { get; set; }
        public string EmpName { get; set; } = string.Empty;
        public int Esops { get; set; }
        public string VestingDate { get; set; } = string.Empty;
    }
}