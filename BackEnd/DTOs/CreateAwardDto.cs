using System;
using System.ComponentModel.DataAnnotations;

namespace BackEnd.DTOs
{
    public class CreateAwardDto
    {
        [Required]
        public string Award_Type { get; set; } = null!;

        [Required]
        public DateTime Grant_Date { get; set; }

        [Required]
        public int Total_Units { get; set; }

        [Required]
        public double Exercise_Price { get; set; }

        [Required]
        public int Employee_EmpId { get; set; }
    }

    
    public class UpdateAwardDto
    {
        public int? Total_Units { get; set; }
        public decimal? Exercise_Price { get; set; }
    }


}
