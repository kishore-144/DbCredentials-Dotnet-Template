using DbCredentials.Data;
using DbCredentials.DTOs;
using DbCredentials.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace DbCredentials.Repositories
{
    public interface IUserRepository
    {
        Task<ApiResponse> CreateUserAsync(SignupDto dto);
        Task<User> GetByEmailAsync(string email);
        Task<User> GetByIdAsync(int id);
        Task<bool> CheckEmailExistsAsync(string email);
        Task<bool> VerifyPasswordAsync(User user, string password);
        Task<ApiResponse> UpdatePasswordAsync(string email, string pass);
    }
}