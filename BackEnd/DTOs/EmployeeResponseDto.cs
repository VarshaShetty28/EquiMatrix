namespace BackEnd.DTOs
{
    public class EmployeeResponseDto
    {
        public int EmpId { get; set; }                 // internal use
        public string EmpName { get; set; } = null!;
        public string EmpEmail { get; set; } = null!;
        public string Role { get; set; } = null!;
        public DateTime EmpDOJ { get; set; }   // ADD 
    }
}