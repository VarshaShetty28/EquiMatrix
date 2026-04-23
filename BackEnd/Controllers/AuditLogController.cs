using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using BackEnd.Data;
using BackEnd.DTOs;

namespace BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditLogController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuditLogController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =======================================
        // ADMIN: GET ALL AUDIT LOGS
        // =======================================
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllAuditLogs()
        {
            var logs = await _context.AuditLogs
                .Select(a => new AuditLogResponseDto
                {
                    AuditLogId = a.AuditLogId,
                    Action_Type = a.Action_Type,
                    Entity_Type = a.Entity_Type,
                    Entity_Id = a.Entity_Id,
                    Employee_EmpId = a.Employee_EmpId,
                    CreatedDate = a.CreatedDate
                })
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();

            return Ok(logs);
        }

        // =======================================
        // ADMIN: CREATE AUDIT LOG (MANUAL)
        // =======================================
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateAuditLog(CreateAuditLogDto dto)
        {
            var auditLog = new Models.AuditLog
            {
                Action_Type = dto.Action_Type,
                Entity_Type = dto.Entity_Type,
                Entity_Id = dto.Entity_Id,
                Employee_EmpId = dto.Employee_EmpId
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            return Created(string.Empty, "Audit log created successfully");
        }
    }
}