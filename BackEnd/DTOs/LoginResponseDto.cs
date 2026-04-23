namespace BackEnd.DTOs
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = null!;
        public string Role { get; set; } = null!;
        public int EmployeeId { get; set; }
    }
}