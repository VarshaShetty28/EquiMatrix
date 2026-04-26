using System.Collections.Generic;

namespace BackEnd.DTOs
{
    public class TaxCalculationResponseDto
    {
        public string Country { get; set; } = null!;
        public string FinancialYear { get; set; } = null!;
        public string TaxRegime { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string Currency { get; set; } = "USD";
        public string CurrencySymbol { get; set; } = "$";
        public decimal ReferenceFxRate { get; set; }
        public decimal AnnualIncomeUsd { get; set; }
        public decimal TotalTaxUsd { get; set; }
        public decimal TotalCessUsd { get; set; }
        public decimal RebateUsd { get; set; }
        public decimal NetTaxUsd { get; set; }
        public decimal NetIncomeUsd { get; set; }
        public decimal EffectiveTaxRate { get; set; }
        public List<TaxSlabBreakdownDto> SlabBreakdown { get; set; } = new();
    }
}
