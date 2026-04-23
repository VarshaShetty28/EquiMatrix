using System;

namespace BackEnd.DTOs
{
    public class AuditLogResponseDto
    {
        public int AuditLogId { get; set; }
        public string Action_Type { get; set; } = null!;
        public string Entity_Type { get; set; } = null!;
        public int? Entity_Id { get; set; }
        public int Employee_EmpId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}