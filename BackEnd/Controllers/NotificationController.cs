using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BackEnd.Data;
using BackEnd.Models;
using BackEnd.DTOs;

namespace BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NotificationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ===============================        // EMPLOYEE: VIEW OWN NOTIFICATIONS
        // ===============================
        [Authorize(Roles = "Employee")]
        [HttpGet("my-notifications")]
        public async Task<IActionResult> GetMyNotifications()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdValue))
                return Unauthorized("User ID not found in token");

            var empId = int.Parse(userIdValue);

            var notifications = await _context.Notifications
                .Where(n => n.Employee_EmpId == empId)
                .OrderByDescending(n => n.NotificationId)
                .Select(n => new NotificationResponseDto
                {
                    NotificationId = n.NotificationId,
                    Message = n.Message,
                    Type = n.Type,
                    Is_Read = n.Is_Read,
                    CreatedDate = n.CreatedDate
                })
                .ToListAsync();

            return Ok(notifications);
        }

        // ===============================
        // EMPLOYEE/ADMIN: MARK AS READ
        // ===============================
        [Authorize(Roles = "Employee,Admin")]
        [HttpPut("{notificationId}/read")]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            try
            {
                var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdValue))
                    return Unauthorized("User ID not found in token");

                if (!int.TryParse(userIdValue, out var empId))
                    return BadRequest("Invalid employee ID in token");

                var userRole = User.FindFirstValue(ClaimTypes.Role);
                Console.WriteLine($"Marking notification {notificationId} as read by user {empId} with role {userRole}");

                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.NotificationId == notificationId);

                if (notification == null)
                {
                    Console.WriteLine($"Notification {notificationId} not found");
                    return NotFound(new { message = "Notification not found" });
                }

                // Allow admin to mark any notification or employee to mark their own
                if (userRole != "Admin" && notification.Employee_EmpId != empId)
                {
                    Console.WriteLine($"Permission denied: User {empId} cannot mark notification owned by {notification.Employee_EmpId}");
                    return Forbid();
                }

                notification.Is_Read = true;
                await _context.SaveChangesAsync();

                Console.WriteLine($"Notification {notificationId} marked as read successfully");
                return Ok(new { message = "Notification marked as read" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error marking notification as read: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = "An error occurred while updating the notification", error = ex.Message });
            }
        }

        // ===============================
        // ADMIN: VIEW ALL NOTIFICATIONS
        // ===============================
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllNotifications()
        {
            var notifications = await _context.Notifications
                .OrderByDescending(n => n.NotificationId)
                .Select(n => new NotificationResponseDto
                {
                    NotificationId = n.NotificationId,
                    Message = n.Message,
                    Type = n.Type,
                    Is_Read = n.Is_Read,
                    CreatedDate = n.CreatedDate
                })
                .ToListAsync();

            return Ok(notifications);
        }
    }
}