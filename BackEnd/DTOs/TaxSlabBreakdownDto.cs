namespace BackEnd.DTOs
{
    public class TaxSlabBreakdownDto
    {
        public decimal LowerBoundUsd { get; set; }
        public decimal? UpperBoundUsd { get; set; }
        public decimal Rate { get; set; }
        public decimal TaxableIncomeUsd { get; set; }
        public decimal TaxAmountUsd { get; set; }
    }
}
