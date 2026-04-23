using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackEnd.Data;
using BackEnd.Models;
using BackEnd.DTOs;

namespace BackEnd.Controllers
{
    [ApiController]
    [Route("api/internal/bootstrap-admin")]
    public class BootstrapAdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BootstrapAdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateInitialAdmin(BootstrapAdminDto dto)
        {
            //  Step 1: Check if any admin already exists
            bool adminExists = await _context.Employees
                .AnyAsync(e => e.Role == "Admin");

            if (adminExists)
            {
                return Forbid("Admin already exists. Bootstrap endpoint disabled.");
            }

            //  Step 2: Create admin with hashed password
            var admin = new Employee
            {
                EmpName = dto.EmpName,
                EmpEmail = dto.EmpEmail,
                EmpDOJ = DateTime.UtcNow,
                Role = "Admin",
                Password_Hash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            _context.Employees.Add(admin);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
            actionName: "GetEmployeeById",
            controllerName: "Employee",
            routeValues: new { id = admin.EmpId },
            value: new
            {
                message = "Initial admin created successfully",
                adminId = admin.EmpId
            }
        );

        }
    }
}