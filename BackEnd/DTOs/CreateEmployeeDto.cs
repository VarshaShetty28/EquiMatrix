using System;
using System.ComponentModel.DataAnnotations;

namespace BackEnd.DTOs
{
    public class CreateEmployeeDto
    {
        [Required]
        public string? EmpName { get; set; }

        [Required, EmailAddress]
        public string? EmpEmail { get; set; }

        [Required]
        public DateTime EmpDOJ { get; set; }

        [Required]
        public string? Password { get; set; }

        [Required]
        public string? Role { get; set; } // Admin / Employee
    }
}