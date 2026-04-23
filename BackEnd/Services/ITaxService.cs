using BackEnd.DTOs;

namespace BackEnd.Services
{
    public interface ITaxService
    {
        TaxCalculationResponseDto CalculateTax(TaxCalculationRequestDto request);
    }
}
