using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BackEnd.Data;
using BackEnd.DTOs;
using BackEnd.Models;

namespace BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VestingScheduleController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public VestingScheduleController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ======================================================
        // ADMIN: VIEW ALL VESTING SCHEDULES
        // ======================================================
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.VestingSchedules
                .Include(v => v.Award)
                .Select(v => new VestingScheduleResponseDto
                {
                    Vesting_Id = v.Vesting_Id,
                    Awards_AwardId = v.Awards_AwardId,
                    Type = v.Award!.Award_Type,
                    AwardPrice = v.Award!.Exercise_Price,

                    Units_Vested = v.Units_Vested,
                    Cumulative_Vested = v.Cumulative_Vested,
                    Remaining_Unvested = v.Award!.Total_Units - v.Cumulative_Vested,

                    Vesting_Date = v.Vesting_Date,
                    Final_Vesting_Date = v.Award!.Grant_Date.AddYears(5),

                    Current_Value = v.Units_Vested * v.Award!.Exercise_Price,
                    Status = v.Status
                })
                .ToListAsync();

            return Ok(data);
        }

        // ======================================================
        // EMPLOYEE: VIEW OWN VESTING SCHEDULE
        // ======================================================
        private async Task<int> RefreshVestedSchedulesAsync(int? empId = null)
        {
            var tomorrow = DateTime.UtcNow.Date.AddDays(1);

            var query = _context.VestingSchedules
                .Where(v => v.Status == "Pending" && v.Vesting_Date < tomorrow);

            if (empId.HasValue)
                query = query.Where(v => v.Employee_EmpId == empId.Value);

            var pendingVestings = await query.ToListAsync();

            foreach (var vesting in pendingVestings)
            {
                vesting.Status = "Vested";
                if (empId.HasValue)
                {
                    var notification = new Notification
                    {
                        Message = $"Shares vested: {vesting.Units_Vested} units from award {vesting.Awards_AwardId}",
                        Type = "Info",
                        Is_Read = false,
                        Employee_EmpId = empId.Value,
                        CreatedDate = DateTime.UtcNow
                    };
                    _context.Notifications.Add(notification);
                }
            }

            if (pendingVestings.Any())
                await _context.SaveChangesAsync();

            return pendingVestings.Count;
        }

        [Authorize(Roles = "Employee")]
        [HttpGet("my-vesting")]
        public async Task<IActionResult> GetMyVesting()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdValue))
                return Unauthorized("User ID not found in token");

            var empId = int.Parse(userIdValue);

            await RefreshVestedSchedulesAsync(empId);

            var data = await _context.VestingSchedules
                .Include(v => v.Award)
                .Where(v => v.Employee_EmpId == empId)
                .Select(v => new VestingScheduleResponseDto
                {
                    Vesting_Id = v.Vesting_Id,
                    Awards_AwardId = v.Awards_AwardId,
                    Type = v.Award!.Award_Type,
                    AwardPrice = v.Award!.Exercise_Price,

                    Units_Vested = v.Units_Vested,
                    Cumulative_Vested = v.Cumulative_Vested,
                    Remaining_Unvested = v.Award!.Total_Units - v.Cumulative_Vested,

                    Vesting_Date = v.Vesting_Date,
                    Final_Vesting_Date = v.Award!.Grant_Date.AddYears(5),

                    Current_Value = v.Units_Vested * v.Award!.Exercise_Price,
                    Status = v.Status
                })
                .ToListAsync();

            return Ok(data);
        }

        [Authorize(Roles = "Employee")]
        [HttpGet("my-vested")]
        public async Task<IActionResult> GetMyVestedSchedules()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdValue))
                return Unauthorized("User ID not found in token");

            var empId = int.Parse(userIdValue);
            await RefreshVestedSchedulesAsync(empId);

            var data = await _context.VestingSchedules
                .Include(v => v.Award)
                .Where(v => v.Employee_EmpId == empId && v.Status == "Vested")
                .Select(v => new VestingScheduleResponseDto
                {
                    Vesting_Id = v.Vesting_Id,
                    Awards_AwardId = v.Awards_AwardId,
                    Type = v.Award!.Award_Type,
                    AwardPrice = v.Award!.Exercise_Price,

                    Units_Vested = v.Units_Vested,
                    Cumulative_Vested = v.Cumulative_Vested,
                    Remaining_Unvested = v.Award!.Total_Units - v.Cumulative_Vested,

                    Vesting_Date = v.Vesting_Date,
                    Final_Vesting_Date = v.Award!.Grant_Date.AddYears(5),

                    Current_Value = v.Units_Vested * v.Award!.Exercise_Price,
                    Status = v.Status
                })
                .ToListAsync();

            return Ok(data);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("refresh-all")]
        public async Task<IActionResult> RefreshAllVestingStatuses()
        {
            var updatedCount = await RefreshVestedSchedulesAsync();
            return Ok(new { updatedCount });
        }
    }
}