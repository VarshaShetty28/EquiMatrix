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
