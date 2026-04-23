using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using BackEnd.Data;
using BackEnd.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using BackEnd.Services;

namespace BackEnd.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoginService _loginService;

        public AuthController(ApplicationDbContext context, ILoginService loginService)
        {
            _context = context;
            _loginService = loginService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmpEmail == request.Email);

            if (employee == null)
            {
                return Unauthorized("Invalid credentials");
            }

            if (!_loginService.VerifyPassword(request.Password, employee.Password_Hash))
            {
                return Unauthorized("Invalid credentials");
            }

            var token = _loginService.CreateJwtToken(employee);

            return Ok(new LoginResponseDto
            {
                Token = token,
                Role = employee.Role,
                EmployeeId = employee.EmpId
            });
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            return Ok("Logout successful");
        }
    }
}
