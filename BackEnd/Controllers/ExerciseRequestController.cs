using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BackEnd.Data;
using BackEnd.Models;
using BackEnd.DTOs;
using BackEnd.Services;

namespace BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExerciseRequestController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ITaxService _taxService;

        public ExerciseRequestController(ApplicationDbContext context, ITaxService taxService)
        {
            _context = context;
            _taxService = taxService;
        }

        // ==================================================
        // EMPLOYEE: CREATE EXERCISE REQUEST
        // Business Rule:
        // Remaining Exercisable Units =
        //   Total Vested Units - Already Approved Exercised Units
        // ==================================================
        [Authorize(Roles = "Employee")]
        [HttpPost]
        public async Task<IActionResult> CreateExerciseRequest(CreateExerciseRequestDto dto)
        {
            // DEBUG: Log received DTO
            Console.WriteLine($"DEBUG: Received DTO - AwardId: {dto.Awards_AwardId}, VestingScheduleId: {dto.VestingScheduleId}, Units: {dto.Units_Requested}");

            var empIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (empIdClaim == null || string.IsNullOrEmpty(empIdClaim.Value)) return Unauthorized();
            int empId = int.Parse(empIdClaim.Value);

            var employee = await _context.Employees.FindAsync(empId);

            // 1 Validate award belongs to employee
            var award = await _context.Awards
                .FirstOrDefaultAsync(a =>
                    a.AwardId == dto.Awards_AwardId &&
                    a.Employee_EmpId == empId
                );

            if (award == null)
                return BadRequest("Invalid award for this employee");

            // 2 Validate vesting schedule belongs to employee and award (optional - for strict validation)
            VestingSchedule? vestingSchedule = null;
            if (dto.VestingScheduleId.HasValue && dto.VestingScheduleId > 0)
            {
                vestingSchedule = await _context.VestingSchedules
                    .FirstOrDefaultAsync(v =>
                        v.Vesting_Id == dto.VestingScheduleId &&
                        v.Awards_AwardId == dto.Awards_AwardId &&
                        v.Employee_EmpId == empId
                    );

                if (vestingSchedule == null)
                    return BadRequest("Invalid vesting schedule for this award");
                
                // Use specific vesting schedule units
                if (vestingSchedule.Units_Vested <= 0)
                    return BadRequest("No vested units available in this vesting schedule");
            }

            // 3 Calculate remaining exercisable units for THIS vesting schedule
            int remainingUnits = 0;
            if (vestingSchedule != null)
            {
                // Deduct only this vesting schedule's already exercised units
                var alreadyExercisedUnits = await _context.ExerciseRequests
                    .Where(r =>
                        r.VestingScheduleId == dto.VestingScheduleId &&
                        r.Employee_EmpId == empId &&
                        r.Status != "Rejected"
                    )
                    .SumAsync(r => r.Units_Requested);

                remainingUnits = vestingSchedule.Units_Vested - alreadyExercisedUnits;
            }
            else
            {
                // Fallback: Calculate total vested units (old behavior)
                var totalVestedUnits = await _context.VestingSchedules
                    .Where(v =>
                        v.Awards_AwardId == dto.Awards_AwardId &&
                        v.Employee_EmpId == empId &&
                        v.Status == "Vested"
                    )
                    .SumAsync(v => v.Units_Vested);

                var alreadyExercisedUnits = await _context.ExerciseRequests
                    .Where(r =>
                        r.Awards_AwardId == dto.Awards_AwardId &&
                        r.Employee_EmpId == empId &&
                        r.Status == "Approved"
                    )
                    .SumAsync(r => r.Units_Requested);

                remainingUnits = totalVestedUnits - alreadyExercisedUnits;
            }

            if (remainingUnits <= 0)
                return BadRequest("No vested units available to exercise for this schedule");

            // 4️ Validate requested units
            if (dto.Units_Requested <= 0)
                return BadRequest("Units requested must be greater than zero");

            if (dto.Units_Requested > remainingUnits)
                return BadRequest("Requested units exceed remaining vested units");

            // 5️ Calculate exercise amount and tax
            var exerciseAmountUsd = (decimal)dto.CurrentSharePrice * dto.Units_Requested;
            var taxableGainUsd = Math.Max(0, (decimal)dto.CurrentSharePrice - (decimal)award.Exercise_Price) * dto.Units_Requested;

            var taxRequest = new TaxCalculationRequestDto
            {
                Country = "US",
                FinancialYear = "2024",
                TaxRegime = "Federal",
                Category = award.Award_Type!, // ESOP or RSU
                AnnualIncomeUsd = taxableGainUsd
            };

            var taxResult = _taxService.CalculateTax(taxRequest);
            var taxAmountUsd = taxResult.NetTaxUsd;
            var netAmountUsd = exerciseAmountUsd - taxAmountUsd;

            // 6️ Create exercise request
            var request = new ExerciseRequest
            {
                Awards_AwardId = dto.Awards_AwardId,
                VestingScheduleId = dto.VestingScheduleId,
                Units_Requested = dto.Units_Requested,
                Requested_Date = DateTime.UtcNow,
                Status = "Pending",
                Employee_EmpId = empId,
                CurrentSharePrice = dto.CurrentSharePrice,
                ExerciseAmountUsd = exerciseAmountUsd,
                TaxableGainUsd = taxableGainUsd,
                TaxAmountUsd = taxAmountUsd,
                NetAmountUsd = netAmountUsd
            };

            _context.ExerciseRequests.Add(request);
            await _context.SaveChangesAsync();

            var admins = await _context.Employees.Where(e => e.Role == "Admin").ToListAsync();
            foreach (var admin in admins)
            {
                var notification = new Notification
                {
                    Message = $"New exercise request submitted by {employee?.EmpName} for {dto.Units_Requested} units",
                    Type = "Alert",
                    Is_Read = false,
                    Employee_EmpId = admin.EmpId,
                    CreatedDate = DateTime.UtcNow
                };
                _context.Notifications.Add(notification);
            }
            await _context.SaveChangesAsync();

            return Created("", new
            {
                message = "Exercise request submitted successfully",
                requestId = request.RequestId,
                exerciseAmountUsd = exerciseAmountUsd,
                taxableGainUsd = taxableGainUsd,
                taxAmountUsd = taxAmountUsd,
                netAmountUsd = netAmountUsd,
                taxDetails = taxResult
            });
        }

        // ==================================================
        // EMPLOYEE: VIEW OWN EXERCISE REQUESTS
        // ==================================================
        [Authorize(Roles = "Employee")]
        [HttpGet("my-requests")]
        public async Task<IActionResult> GetMyExerciseRequests()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdValue))
                return Unauthorized("User ID not found in token");

            var empId = int.Parse(userIdValue);

            var requests = await _context.ExerciseRequests
                .Where(r => r.Employee_EmpId == empId)
                .Select(r => new ExerciseRequestResponseDto
                {
                    RequestId = r.RequestId,
                    Awards_AwardId = r.Awards_AwardId,
                    VestingScheduleId = r.VestingScheduleId,
                    Units_Requested = r.Units_Requested,
                    Requested_Date = r.Requested_Date,
                    Status = r.Status,
                    Employee_EmpId = r.Employee_EmpId,
                    ExerciseAmountUsd = r.ExerciseAmountUsd,
                    TaxableGainUsd = r.TaxableGainUsd,
                    TaxAmountUsd = r.TaxAmountUsd,
                    NetAmountUsd = r.NetAmountUsd
                })
                .ToListAsync();

            return Ok(requests);
        }

        // ==================================================
        // ADMIN: VIEW ALL EXERCISE REQUESTS
        // ==================================================
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllExerciseRequests()
        {
            var requests = await _context.ExerciseRequests
                .Join(
                    _context.Employees,
                    er => er.Employee_EmpId,
                    emp => emp.EmpId,
                    (er, emp) => new
                    {
                        RequestId = er.RequestId,
                        Awards_AwardId = er.Awards_AwardId,
                        Units_Requested = er.Units_Requested,
                        Requested_Date = er.Requested_Date,
                        Status = er.Status,
                        Employee_EmpId = er.Employee_EmpId,
                        EmployeeName = emp.EmpName,
                        ExerciseAmountUsd = er.ExerciseAmountUsd,
                        TaxableGainUsd = er.TaxableGainUsd,
                        TaxAmountUsd = er.TaxAmountUsd,
                        NetAmountUsd = er.NetAmountUsd
                    }
                )
                .ToListAsync();

            return Ok(requests);
        }

        // ==================================================
        // ADMIN: VIEW EXERCISE REQUESTS BY EMPLOYEE ID
        // ==================================================
        [Authorize(Roles = "Admin")]
        [HttpGet("employee/{empId}")]
        public async Task<IActionResult> GetExerciseRequestsByEmployee(int empId)
        {
            var requests = await _context.ExerciseRequests
                .Where(r => r.Employee_EmpId == empId)
                .Select(r => new ExerciseRequestResponseDto
                {
                    RequestId = r.RequestId,
                    Awards_AwardId = r.Awards_AwardId,
                    Units_Requested = r.Units_Requested,
                    Requested_Date = r.Requested_Date,
                    Status = r.Status,
                    Employee_EmpId = r.Employee_EmpId,
                    ExerciseAmountUsd = r.ExerciseAmountUsd,
                    TaxableGainUsd = r.TaxableGainUsd,
                    TaxAmountUsd = r.TaxAmountUsd,
                    NetAmountUsd = r.NetAmountUsd
                })
                .ToListAsync();

            return Ok(requests);
        }

        // ==================================================
        // ADMIN: APPROVE OR REJECT EXERCISE REQUEST
        // ==================================================
        [Authorize(Roles = "Admin")]
        [HttpPut("{requestId}/status")]
        public async Task<IActionResult> UpdateRequestStatus(
            int requestId,
            [FromQuery] string status)
        {
            var empIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (empIdClaim == null || string.IsNullOrEmpty(empIdClaim.Value)) return Unauthorized();
            int adminEmpId = int.Parse(empIdClaim.Value);

            if (status != "Approved" && status != "Rejected")
                return BadRequest("Status must be Approved or Rejected");

            var request = await _context.ExerciseRequests
                .FirstOrDefaultAsync(r => r.RequestId == requestId);

            if (request == null)
                return NotFound("Exercise request not found");

            if (request.Status != "Pending")
                return BadRequest("Only pending requests can be updated");

            request.Status = status;
            await _context.SaveChangesAsync();

            var actionType = status == "Approved" ? "ApproveExercise" : "RejectExercise";
            var auditLog = new AuditLog
            {
                Action_Type = actionType,
                Entity_Type = "ExerciseRequest",
                Entity_Id = requestId,
                Employee_EmpId = adminEmpId
            };
            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            var message = status == "Approved" ? "Exercise request approved" : "Exercise request rejected";
            var notification = new Notification
            {
                Message = message,
                Type = "Info",
                Is_Read = false,
                Employee_EmpId = request.Employee_EmpId,
                CreatedDate = DateTime.UtcNow
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Exercise request {status.ToLower()} successfully", requestId = requestId, status = status });
        }
    }
}
