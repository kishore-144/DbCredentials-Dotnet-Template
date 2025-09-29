// DTOs/AuthDtos.cs
using System.ComponentModel.DataAnnotations;

namespace DbCredentials.DTOs
{
    public class SignupDto
    {
        [Required] public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }

        [Required, EmailAddress] public string email { get; set; }
        [Required, Phone] public string phoneNumber { get; set; }
        public DateTime? dob { get; set; }

        [Required, MinLength(6)] public string password { get; set; }
    }

    public class LoginDto
    {
        [Required, EmailAddress] public string email { get; set; }
        [Required] public string password { get; set; }
    }

    public class AuthResponseDto
    {
        public string? token { get; set; }
        public string status { get; set; }
        public string message { get; set; }
    }
    public class ApiResponse
    {
        public string status { get; set; }
        public string message { get; set; }
    }
    public class OtpVerifyRequest
    {
        public string Email { get; set; } 
        public string Otp { get; set; } 
    }

    public class EmailTarget
    {
        public string Email { get; set; }
    }
    public class SetPassword
    {
        public string Password { get; set; }
    }
}
