using BackEnd.DTOs;
using BackEnd.Services;
using BackEnd.Data;
using BackEnd.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaxController : ControllerBase
    {
        private readonly ITaxService _taxService;
        private readonly ApplicationDbContext _context;
        private readonly MarketPriceService _marketPriceService;

        public TaxController(ITaxService taxService, ApplicationDbContext context, MarketPriceService marketPriceService)
        {
            _taxService = taxService;
            _context = context;
            _marketPriceService = marketPriceService;
        }

        [HttpPost("calculate")]
        [Authorize(Roles = "Admin,Employee")]
        public IActionResult CalculateTax([FromBody] TaxCalculationRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = _taxService.CalculateTax(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // Admin endpoints for managing tax data
        [HttpGet("countries")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTaxCountries()
        {
            var countries = await _context.TaxCountries
                .Include(c => c.Regimes)
                    .ThenInclude(r => r.Slabs)
                .ToListAsync();
            return Ok(countries);
        }

        [HttpPost("seed")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SeedTaxData()
        {
            // Check if data already exists
            if (await _context.TaxSlabs.AnyAsync())
            {
                return BadRequest("Tax slab data already exists");
            }

            // Create a minimal fallback country/regime record for schema compatibility.
            var usCountry = new TaxCountry
            {
                CountryCode = "US",
                Description = "Single-filer federal income tax brackets.",
                ReferenceFxRate = 1.0m
            };

            _context.TaxCountries.Add(usCountry);
            await _context.SaveChangesAsync();

            var baseRegime = new TaxRegime
            {
                FinancialYear = "2026",
                Regime = "Federal",
                Category = "Single",
                CessRate = 0.0m,
                RebateThresholdUsd = 0.0m,
                RebateAmountUsd = 0.0m,
                TaxCountryId = usCountry.Id
            };

            _context.TaxRegimes.Add(baseRegime);
            await _context.SaveChangesAsync();

            var slabs = new[]
            {
                new TaxSlab { LowerBoundUsd = 0.00m, UpperBoundUsd = 12400.00m, Rate = 0.10m, TaxRegimeId = baseRegime.Id },
                new TaxSlab { LowerBoundUsd = 12400.00m, UpperBoundUsd = 50400.00m, Rate = 0.12m, TaxRegimeId = baseRegime.Id },
                new TaxSlab { LowerBoundUsd = 50400.00m, UpperBoundUsd = 105700.00m, Rate = 0.22m, TaxRegimeId = baseRegime.Id },
                new TaxSlab { LowerBoundUsd = 105700.00m, UpperBoundUsd = 201775.00m, Rate = 0.24m, TaxRegimeId = baseRegime.Id },
                new TaxSlab { LowerBoundUsd = 201775.00m, UpperBoundUsd = 256225.00m, Rate = 0.32m, TaxRegimeId = baseRegime.Id },
                new TaxSlab { LowerBoundUsd = 256225.00m, UpperBoundUsd = 640600.00m, Rate = 0.35m, TaxRegimeId = baseRegime.Id },
                new TaxSlab { LowerBoundUsd = 640600.00m, UpperBoundUsd = null, Rate = 0.37m, TaxRegimeId = baseRegime.Id }
            };

            _context.TaxSlabs.AddRange(slabs);
            await _context.SaveChangesAsync();

            return Ok("Tax data seeded successfully");
        }

        [HttpGet("market-price")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> GetMarketPrice([FromQuery] string symbol, [FromQuery] string? date = null)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                return BadRequest(new { error = "Symbol is required" });
            }

            DateTime? parsedDate = null;
            if (!string.IsNullOrWhiteSpace(date))
            {
                if (!DateTime.TryParse(date, out var d))
                {
                    return BadRequest(new { error = "Invalid date format. Use YYYY-MM-DD" });
                }
                parsedDate = d;
            }

            var price = await _marketPriceService.GetAdjustedClosePriceAsync(symbol.ToUpper(), parsedDate);

            if (price == null)
            {
                return NotFound(new { error = "Price not found for the specified symbol and date" });
            }

            return Ok(new
            {
                symbol = symbol.ToUpper(),
                date = parsedDate?.ToString("yyyy-MM-dd") ?? "latest",
                adjustedClosePrice = price
            });
        }

        [HttpGet("market-price-history")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> GetMarketPriceHistory([FromQuery] string symbol, [FromQuery] int days = 30)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                return BadRequest(new { error = "Symbol is required" });
            }

            if (days < 1 || days > 365)
            {
                return BadRequest(new { error = "Days must be between 1 and 365" });
            }

            var history = await _marketPriceService.GetPriceHistoryAsync(symbol.ToUpper(), days);

            return Ok(new
            {
                symbol = symbol.ToUpper(),
                days = days,
                data = history
            });
        }
    }
}
