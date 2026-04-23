using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BackEnd.Data;
using BackEnd.Models;
using BackEnd.DTOs;
using BackEnd.Builders;

namespace BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AwardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AwardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =======================================
        // INTERNAL: Generate Vesting Schedule (5 years)
        // =======================================
        private List<VestingSchedule> GenerateVestingSchedule(Award award)
        {
            var schedules = new List<VestingSchedule>();

            int years = 5;
            int unitsPerYear = award.Total_Units / years;
            int cumulative = 0;

            for (int i = 1; i <= years; i++)
            {
                cumulative += unitsPerYear;

                schedules.Add(new VestingSchedule
                {
                    Awards_AwardId = award.AwardId,
                    Employee_EmpId = award.Employee_EmpId,
                    Vesting_Date = award.Grant_Date.AddYears(i),
                    Units_Vested = unitsPerYear,
                    Cumulative_Vested = cumulative,
                    Status = "Pending"
                });
            }

            return schedules;
        }

        // =======================================
        // ADMIN: CREATE AWARD + AUTO-VESTING
        // =======================================
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateAward(CreateAwardDto dto)
        {
            var empIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (empIdClaim == null) return Unauthorized();
            int empId = int.Parse(empIdClaim.Value);

            var employeeExists = await _context.Employees
                .AnyAsync(e => e.EmpId == dto.Employee_EmpId);

            if (!employeeExists)
                return BadRequest("Employee does not exist");

            var award = new AwardBuilder()
                .WithType(dto.Award_Type)
                .WithGrantDate(dto.Grant_Date)
                .WithTotalUnits(dto.Total_Units)
                .WithExercisePrice(dto.Exercise_Price)
                .ForEmployee(dto.Employee_EmpId)
                .Build();

            _context.Awards.Add(award);
            await _context.SaveChangesAsync();

            var vestingRows = new VestingScheduleBuilder()
                .WithYears(5)
                .WithStatus("Pending")
                .BuildForAward(award);
            _context.VestingSchedules.AddRange(vestingRows);
            await _context.SaveChangesAsync();

            var auditLog = new AuditLog
            {
                Action_Type = "CreateAward",
                Entity_Type = "Award",
                Entity_Id = award.AwardId,
                Employee_EmpId = empId
            };
            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            var notification = new Notification
            {
                Message = "New award granted",
                Type = "Info",
                Is_Read = false,
                Employee_EmpId = dto.Employee_EmpId,
                CreatedDate = DateTime.UtcNow
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return Created("", new { message = "Award and vesting schedule created successfully", awardId = award.AwardId });
        }

        // =======================================
        // ADMIN: GET ALL AWARDS
        // =======================================
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllAwards()
        {
            var awards = await _context.Awards
                .Select(a => new AwardResponseDto
                {
                    AwardId = a.AwardId,
                    Award_Type = a.Award_Type,
                    Grant_Date = a.Grant_Date,
                    Total_Units = a.Total_Units,
                    Exercise_Price = a.Exercise_Price,
                    Employee_EmpId = a.Employee_EmpId
                })
                .ToListAsync();

            return Ok(awards);
        }

        // =======================================
        // ADMIN: GET AWARD BY EMPLOYEE
        // =======================================
        [Authorize(Roles = "Admin")]
        [HttpGet("employee/{empId}")]
        public async Task<IActionResult> GetAwardsByEmployee(int empId)
        {
            var awards = await _context.Awards
                .Where(a => a.Employee_EmpId == empId)
                .Select(a => new AwardResponseDto
                {
                    AwardId = a.AwardId,
                    Award_Type = a.Award_Type,
                    Grant_Date = a.Grant_Date,
                    Total_Units = a.Total_Units,
                    Exercise_Price = a.Exercise_Price,
                    Employee_EmpId = a.Employee_EmpId
                })
                .ToListAsync();

            return Ok(awards);
        }

        // =======================================
        // EMPLOYEE: GET OWN AWARDS
        // =======================================
        [Authorize(Roles = "Employee")]
        [HttpGet("my-awards")]
        public async Task<IActionResult> GetMyAwards()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdValue))
                return Unauthorized("User ID not found in token");

            var empId = int.Parse(userIdValue);

            var awards = await _context.Awards
                .Where(a => a.Employee_EmpId == empId)
                .Select(a => new AwardResponseDto
                {
                    AwardId = a.AwardId,
                    Award_Type = a.Award_Type,
                    Grant_Date = a.Grant_Date,
                    Total_Units = a.Total_Units,
                    Exercise_Price = a.Exercise_Price,
                    Employee_EmpId = a.Employee_EmpId
                })
                .ToListAsync();

            return Ok(awards);
        }

        // =======================================
        // ADMIN: UPDATE AWARD
        // =======================================
        [Authorize(Roles = "Admin")]
        [HttpPut("{awardId}")]
        public async Task<IActionResult> UpdateAward(int awardId, UpdateAwardDto dto)
        {
            var award = await _context.Awards.FindAsync(awardId);

            if (award == null)
                return NotFound("Award not found");

            if (dto.Total_Units.HasValue)
                award.Total_Units = dto.Total_Units.Value;

            if (dto.Exercise_Price.HasValue)
                award.Exercise_Price = (double)dto.Exercise_Price.Value;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Award updated successfully", awardId = award.AwardId });
        }

        // =======================================
        // ADMIN: DELETE AWARD
        // =======================================
        [Authorize(Roles = "Admin")]
        [HttpDelete("{awardId}")]
        public async Task<IActionResult> DeleteAward(int awardId)
        {
            var award = await _context.Awards.FindAsync(awardId);

            if (award == null)
                return NotFound("Award not found");

            // Check if vesting schedules exist for this award
            var vestingSchedulesExist = await _context.VestingSchedules
                .AnyAsync(vs => vs.Awards_AwardId == awardId);

            if (vestingSchedulesExist)
                return BadRequest(new { message = "Cannot delete this award because it has vesting schedules. Please contact support to delete the vesting schedules first." });

            _context.Awards.Remove(award);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Award deleted successfully", awardId = awardId });
        }
    }
}