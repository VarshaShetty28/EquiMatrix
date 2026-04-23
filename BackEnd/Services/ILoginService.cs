using BackEnd.Models;

namespace BackEnd.Services
{
    public interface ILoginService
    {
        bool VerifyPassword(string plainText, string hashedPassword);
        string CreateJwtToken(Employee employee);
    }
}