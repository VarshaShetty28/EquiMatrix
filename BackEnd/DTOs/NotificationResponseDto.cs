using System;

namespace BackEnd.DTOs
{
    public class NotificationResponseDto
    {
        public int NotificationId { get; set; }
        public string Message { get; set; } = null!;
        public string Type { get; set; } = null!;
        public bool Is_Read { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}