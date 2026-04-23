using BackEnd.DTOs;
using BackEnd.Data;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Services
{
    public class TaxService : ITaxService
    {
        private readonly ApplicationDbContext _context;

        public TaxService(ApplicationDbContext context)
        {
            _context = context;
        }

        public TaxCalculationResponseDto CalculateTax(TaxCalculationRequestDto request)
        {
            var slabs = _context.TaxSlabs
                .OrderBy(s => s.LowerBoundUsd)
                .ToList();

            if (!slabs.Any())
                throw new InvalidOperationException("Tax slab configuration is missing. Please seed tax slab data.");

            var breakdown = new List<TaxSlabBreakdownDto>();
            var totalTax = 0m;
            var income = request.AnnualIncomeUsd;

            foreach (var slab in slabs)
            {
                if (income <= slab.LowerBoundUsd)
                    continue;

                var upperBound = slab.UpperBoundUsd ?? income;
                var taxable = Math.Min(income, upperBound) - slab.LowerBoundUsd;
                if (taxable <= 0)
                    continue;

                var taxAmount = Math.Round(taxable * slab.Rate, 2);
                totalTax += taxAmount;

                breakdown.Add(new TaxSlabBreakdownDto
                {
                    LowerBoundUsd = slab.LowerBoundUsd,
                    UpperBoundUsd = slab.UpperBoundUsd,
                    Rate = slab.Rate,
                    TaxableIncomeUsd = Math.Round(taxable, 2),
                    TaxAmountUsd = taxAmount
                });

                if (slab.UpperBoundUsd == null || income <= slab.UpperBoundUsd)
                    break;
            }

            var netTax = Math.Round(totalTax, 2);
            var netIncome = Math.Round(income - netTax, 2);
            var effectiveTaxRate = income > 0 ? Math.Round(netTax / income * 100, 2) : 0m;

            return new TaxCalculationResponseDto
            {
                Country = "US",
                FinancialYear = "2026",
                TaxRegime = "Federal",
                Category = "Single",
                ReferenceFxRate = 1.0m,
                AnnualIncomeUsd = income,
                TotalTaxUsd = netTax,
                TotalCessUsd = 0m,
                RebateUsd = 0m,
                NetTaxUsd = netTax,
                NetIncomeUsd = netIncome,
                EffectiveTaxRate = effectiveTaxRate,
                SlabBreakdown = breakdown
            };
        }
    }
}
