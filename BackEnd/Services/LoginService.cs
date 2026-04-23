using BackEnd.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BackEnd.Services
{
    public class LoginService : ILoginService
    {
        private readonly IConfiguration _config;

        public LoginService(IConfiguration config)
        {
            _config = config;
        }

        public bool VerifyPassword(string plainText, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(plainText, hashedPassword);
        }

        public string CreateJwtToken(Employee employee)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, employee.EmpId.ToString()),
                new Claim(ClaimTypes.Email, employee.EmpEmail ?? string.Empty),
                new Claim(ClaimTypes.Role, employee.Role ?? string.Empty),
                new Claim(ClaimTypes.Name, employee.EmpName ?? string.Empty)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    Convert.ToDouble(_config["Jwt:ExpiryMinutes"])
                ),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}