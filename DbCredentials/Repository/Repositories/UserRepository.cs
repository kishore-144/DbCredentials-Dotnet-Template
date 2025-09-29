using DbCredentials.Data;
using DbCredentials.DTOs;
using DbCredentials.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace DbCredentials.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserRepository(AppDbContext db, IPasswordHasher<User> passwordHasher)
        {
            _db = db;
            _passwordHasher = passwordHasher;
        }

      
        public async Task<ApiResponse> CreateUserAsync(SignupDto dto)
        {
            using var transaction = await _db.Database.BeginTransactionAsync(); // ACID starts here
            try
            {
                var user = new User
                {
                    FirstName = dto.firstName,
                    MiddleName = dto.middleName,
                    LastName = dto.lastName,
                    Email = dto.email,
                    PhoneNumber = dto.phoneNumber,
                    Password = _passwordHasher.HashPassword(null, dto.password),
                    CreatedBy = "sample",
                    Dob = DateTime.SpecifyKind(dto.dob.Value, DateTimeKind.Utc),
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = null
                };

                _db.DbCredentials.Add(user);
                await _db.SaveChangesAsync();

                
                user.CreatedBy = user.Id.ToString();
                await _db.SaveChangesAsync();

                await transaction.CommitAsync();

                return new ApiResponse
                {
                    status = "Success",
                    message = "Created Successfully"
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); 
                return new ApiResponse
                {
                    status = "Failure",
                    message = $"Error creating user: {ex.Message}"
                };
            }
        }

        
        public async Task<User> GetByEmailAsync(string email)
        {
            try
            {
                return await _db.DbCredentials.SingleOrDefaultAsync(u => u.Email == email);
            }
            catch
            {
                return null;
            }
        }

      
        public async Task<User> GetByIdAsync(int id)
        {
            try
            {
                return await _db.DbCredentials.FindAsync(id);
            }
            catch
            {
                return null;
            }
        }

        
        public async Task<bool> CheckEmailExistsAsync(string email)
        {
            try
            {
                return await _db.DbCredentials.AnyAsync(u => u.Email == email);
            }
            catch
            {
                return false;
            }
        }

        
        public async Task<bool> VerifyPasswordAsync(User user, string password)
        {
            try
            {
                var result = _passwordHasher.VerifyHashedPassword(user, user.Password, password);
                return result != PasswordVerificationResult.Failed;
            }
            catch
            {
                return false;
            }
        }

        
        public async Task<ApiResponse> UpdatePasswordAsync(string email, string pass)
        {
            using var transaction = await _db.Database.BeginTransactionAsync(); 
            try
            {
                
                var toBeChanged = await _db.DbCredentials
                                           .FirstOrDefaultAsync(u => u.Email == email);

                if (toBeChanged == null)
                {
                    return new ApiResponse
                    {
                        status = "Failure",
                        message = $"No user found with email id: {email}"
                    };
                }

                toBeChanged.Password = _passwordHasher.HashPassword(null, pass);
                await _db.SaveChangesAsync();

                await transaction.CommitAsync();

                return new ApiResponse
                {
                    status = "Success",
                    message = "Password changed successfully"
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ApiResponse
                {
                    status = "Failure",
                    message = $"Exception Occurred: {ex.Message}"
                };
            }
        }
    }
}
