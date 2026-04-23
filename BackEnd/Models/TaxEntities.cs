using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackEnd.Models
{
    public class TaxCountry
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(10)]
        public string CountryCode { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public decimal ReferenceFxRate { get; set; }

        public ICollection<TaxRegime> Regimes { get; set; } = new List<TaxRegime>();
    }

    public class TaxRegime
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(20)]
        public string FinancialYear { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Regime { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Category { get; set; } = string.Empty;

        public decimal CessRate { get; set; }

        public decimal RebateThresholdUsd { get; set; }

        public decimal RebateAmountUsd { get; set; }

        [ForeignKey(nameof(TaxCountry))]
        public int TaxCountryId { get; set; }
        public TaxCountry? TaxCountry { get; set; }

        public ICollection<TaxSlab> Slabs { get; set; } = new List<TaxSlab>();
    }

    public class TaxSlab
    {
        [Key]
        public int Id { get; set; }

        public decimal LowerBoundUsd { get; set; }

        public decimal? UpperBoundUsd { get; set; }

        public decimal Rate { get; set; }

        [ForeignKey(nameof(TaxRegime))]
        public int TaxRegimeId { get; set; }
        public TaxRegime? TaxRegime { get; set; }
    }
}