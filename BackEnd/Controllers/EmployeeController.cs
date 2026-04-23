using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using BackEnd.Data;
using BackEnd.Models;
using BackEnd.DTOs;
using BackEnd.Services;
using System.Security.Claims;

namespace BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EmployeeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly MarketPriceService _marketPriceService;

        public EmployeeController(ApplicationDbContext context, MarketPriceService marketPriceService)
        {
            _context = context;
            _marketPriceService = marketPriceService;
        }

        // ===============================
        // ADMIN: GET ALL EMPLOYEES
        // ===============================
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllEmployees()
        {
            var employees = await _context.Employees
                .Select(e => new EmployeeResponseDto
                {
                    EmpId = e.EmpId,
                    EmpName = e.EmpName,
                    EmpEmail = e.EmpEmail,
                    Role = e.Role,
                    EmpDOJ = e.EmpDOJ
                })
                .ToListAsync();

            return Ok(employees);
        }

        // ===============================
        // ADMIN: GET EMPLOYEE BY ID
        // ===============================
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployeeById(int id)
        {
            var employee = await _context.Employees
                .Where(e => e.EmpId == id)
                .Select(e => new EmployeeResponseDto
                {
                    EmpId = e.EmpId,
                    EmpName = e.EmpName,
                    EmpEmail = e.EmpEmail,
                    Role = e.Role,
                    EmpDOJ = e.EmpDOJ
                })
                .FirstOrDefaultAsync();

            if (employee == null)
                return NotFound("Employee not found");

            return Ok(employee);
        }

        // ===============================
        // EMPLOYEE / ADMIN: GET OWN PROFILE
        // ===============================
        [HttpGet("profile")]
        [Authorize(Roles = "Employee,Admin")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
                return Unauthorized("User ID not found in token");

            if (!int.TryParse(userIdClaim.Value, out var employeeId))
                return BadRequest("Invalid employee ID");

            var employee = await _context.Employees
                .Where(e => e.EmpId == employeeId)
                .Select(e => new EmployeeResponseDto
                {
                    EmpId = e.EmpId,
                    EmpName = e.EmpName,
                    EmpEmail = e.EmpEmail,
                    Role = e.Role,
                    EmpDOJ = e.EmpDOJ
                })
                .FirstOrDefaultAsync();

            if (employee == null)
                return NotFound("Employee not found");

            return Ok(employee);
        }

        // ===============================
        // EMPLOYEE / ADMIN: GET DASHBOARD STATS
        // ===============================
        [HttpGet("dashboard")]
        [Authorize(Roles = "Employee,Admin")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
                return Unauthorized("User ID not found in token");

            if (!int.TryParse(userIdClaim.Value, out var employeeId))
                return BadRequest("Invalid employee ID");

            var totalEquityGranted = await GetTotalEquityGrantedAsync(employeeId);
            var exercisedEsop = await GetExercisedEsopUnitsAsync(employeeId);
            var vestedRsu = await GetVestedRsuUnitsAsync(employeeId);

            var shareValue = await GetShareValueAsync(employeeId, exercisedEsop + vestedRsu);

            var dashboardDto = new EmployeeDashboardDto
            {
                TotalEquityGranted = totalEquityGranted,
                ShareValue = shareValue
            };

            return Ok(dashboardDto);
        }

        private async Task<decimal> GetTotalEquityGrantedAsync(int employeeId)
        {
            return await _context.Awards
                .Where(a => a.Employee_EmpId == employeeId)
                .SumAsync(a => (decimal)a.Total_Units);
        }

        private async Task<decimal> GetExercisedEsopUnitsAsync(int employeeId)
        {
            return await _context.ExerciseRequests
                .Where(er => er.Employee_EmpId == employeeId && er.Status == "Accepted")
                .Join(
                    _context.Awards.Where(a => a.Award_Type == "ESOP"),
                    er => er.Awards_AwardId,
                    a => a.AwardId,
                    (er, a) => er.Units_Requested
                )
                .SumAsync(units => (decimal)units);
        }

        private async Task<decimal> GetVestedRsuUnitsAsync(int employeeId)
        {
            return await _context.VestingSchedules
                .Where(vs => vs.Employee_EmpId == employeeId && vs.Status == "Vested")
                .Join(
                    _context.Awards.Where(a => a.Award_Type == "RSU"),
                    vs => vs.Awards_AwardId,
                    a => a.AwardId,
                    (vs, a) => vs.Cumulative_Vested
                )
                .SumAsync(units => (decimal)units);
        }

        private async Task<decimal> GetShareValueAsync(int employeeId, decimal totalShares)
        {
            if (totalShares <= 0)
                return 0;

            var fmv = await _marketPriceService.GetAdjustedClosePriceAsync("IBM");
            if (fmv == null)
                return 0;

            return totalShares * fmv.Value;
        }

        // ===============================
        // ADMIN: CREATE EMPLOYEE
        // ===============================
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateEmployee(CreateEmployeeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // FIXED HERE: use User instead of aUser
            var empIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (empIdClaim == null) return Unauthorized();

            int adminEmpId = int.Parse(empIdClaim.Value);

            var employee = new Employee
            {
                EmpName = dto.EmpName!,
                EmpEmail = dto.EmpEmail!,
                EmpDOJ = dto.EmpDOJ,
                Password_Hash = BCrypt.Net.BCrypt.HashPassword(dto.Password!),
                Role = dto.Role!
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            var auditLog = new AuditLog
            {
                Action_Type = "CreateEmployee",
                Entity_Type = "Employee",
                Entity_Id = employee.EmpId,
                Employee_EmpId = adminEmpId
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEmployeeById), new { id = employee.EmpId }, new
            {
                message = "Employee created successfully",
                employeeId = employee.EmpId
            });
        }

        // ===============================
        // ADMIN: UPDATE EMPLOYEE
        // ===============================
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, UpdateEmployeeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var empIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (empIdClaim == null) return Unauthorized();

            int adminEmpId = int.Parse(empIdClaim.Value);

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
                return NotFound("Employee not found");

            employee.EmpName = dto.EmpName!;
            employee.EmpEmail = dto.EmpEmail!;
            employee.Role = dto.Role!;

            await _context.SaveChangesAsync();

            var auditLog = new AuditLog
            {
                Action_Type = "UpdateEmployee",
                Entity_Type = "Employee",
                Entity_Id = employee.EmpId,
                Employee_EmpId = adminEmpId
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            return Ok("Employee updated successfully");
        }

        // ===============================
        // ADMIN: DELETE EMPLOYEE
        // ===============================>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
                return NotFound("Employee not found");

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return Ok("Employee deleted successfully");
        }

        // ===============================
        // ADMIN: GET ADMIN DASHBOARD
        // ===============================
        [Authorize(Roles = "Admin")]
        [HttpGet("admin-dashboard")]
        public async Task<IActionResult> GetAdminDashboard()
        {
            // Get total employees
            var totalEmployees = await _context.Employees.CountAsync();

            // Get total awards granted
            var totalAwardsGranted = await _context.Awards.SumAsync(a => a.Total_Units);

            // Get total vested units
            var totalVestedUnits = await _context.VestingSchedules
                .Where(vs => vs.Status == "Vested")
                .SumAsync(vs => vs.Cumulative_Vested);

            // Get pending exercise requests
            var pendingRequests = await _context.ExerciseRequests
                .CountAsync(er => er.Status == "Pending");

            // Get ESOP and RSU unit counts (sum of Total_Units)
            var esopUnitCount = await _context.Awards
                .Where(a => a.Award_Type == "ESOP")
                .SumAsync(a => a.Total_Units);
            var rsuUnitCount = await _context.Awards
                .Where(a => a.Award_Type == "RSU")
                .SumAsync(a => a.Total_Units);

            // Calculate percentages (avoid division by zero)
            var totalUnits = esopUnitCount + rsuUnitCount;
            var esopPercentage = totalUnits > 0 ? (int)Math.Round((double)esopUnitCount / totalUnits * 100) : 0;
            var rsuPercentage = totalUnits > 0 ? (int)Math.Round((double)rsuUnitCount / totalUnits * 100) : 0;

            // Get employees with their award counts for the table
            var employees = await _context.Employees
                .Select(e => new AdminDashboardEmployee
                {
                    EmpId = e.EmpId,
                    EmpName = e.EmpName,
                    Esops = _context.Awards.Count(a => a.Employee_EmpId == e.EmpId && a.Award_Type == "ESOP"),
                    VestingDate = _context.VestingSchedules
                        .Where(vs => vs.Employee_EmpId == e.EmpId)
                        .OrderByDescending(vs => vs.Vesting_Date)
                        .Select(vs => vs.Vesting_Date.ToString("yyyy-MM-dd"))
                        .FirstOrDefault() ?? "N/A"
                })
                .ToListAsync();

            var dashboard = new AdminDashboardDto
            {
                TotalEmployees = totalEmployees,
                TotalAwardsGranted = totalAwardsGranted,
                TotalVestedUnits = totalVestedUnits,
                PendingRequests = pendingRequests,
                EsopCount = esopUnitCount,
                RsuCount = rsuUnitCount,
                EsopPercentage = esopPercentage,
                RsuPercentage = rsuPercentage,
                Employees = employees
            };

            return Ok(dashboard);
        }
    }
}