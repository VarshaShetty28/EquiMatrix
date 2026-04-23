using System.ComponentModel.DataAnnotations;

namespace BackEnd.DTOs
{
    public class TaxCalculationRequestDto
    {
        [Required]
        public string? Country { get; set; }

        [Required]
        public string? FinancialYear { get; set; }

        [Required]
        public string? TaxRegime { get; set; }

        [Required]
        public string? Category { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Annual income must be a positive number.")]
        public decimal AnnualIncomeUsd { get; set; }
    }
}
