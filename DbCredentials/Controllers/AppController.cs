using DbCredentials.DTOs;
using DbCredentials.Repositories;
using DbCredentials.Services;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Mail;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _repo;
    private readonly TokenService _tokenService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserRepository repo, TokenService tokenService, ILogger<UsersController> logger)
    {
        _repo = repo;
        _tokenService = tokenService;
        _logger = logger;
    }

    [HttpPost("signup")]
    public async Task<ActionResult<ApiResponse>> Signup([FromBody] SignupDto dto)
    {
        try
        {
            _logger.LogInformation("Signup API called...");
            //if (!ModelState.IsValid) return Ok(ModelState);
            if (await _repo.CheckEmailExistsAsync(dto.email))
            {
                _logger.LogWarning($"Message: Email already exists with another account : {dto.email}");
                return Ok(new ApiResponse
                {
                    status = "Failure",
                    message = "Email already exists with another account"
                });
            }
            var response = await _repo.CreateUserAsync(dto);
            if(response.status=="Success")
            {
                _logger.LogInformation($"Message: {response.message} : {dto.email}");
            }
            else
            {
                _logger.LogWarning($"Message: {response.message} : {dto.email}");
            }
                return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Exception: {ex.Message} : {dto.email}");
            return Ok( new ApiResponse
            {
                status = "Failure",
                message = $"Unexpected error in Signup: {ex.Message}"
            });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        try
        {
            _logger.LogInformation("Login API called...");
            if (!ModelState.IsValid) return Ok(ModelState);
            _logger.LogInformation($"DTO values => Email: {dto.email}, Password: {(dto.password == null ? "NULL" : "*****")}");
            var user = await _repo.GetByEmailAsync(dto.email);
            if (user == null)
            {
                _logger.LogWarning($"Email does not exist: {dto.email}");
                return Ok(new AuthResponseDto
                    {
                        token = null,
                        status = "Failure",
                        message = "Email does not exist"
                    }
                );

            }  
            if(!await _repo.VerifyPasswordAsync(user, dto.password))
            {
                _logger.LogWarning($"Password Incorrect: {dto.email}");
                return Ok(new AuthResponseDto
                    {
                        token = null,
                        status = "Failure",
                        message = "Password Incorrect"
                    }
                );
            }

            var (token, expires) = _tokenService.CreateToken(user);
            _logger.LogInformation($"Message:Logged In Successfully: {dto.email}");
            return Ok(new AuthResponseDto
            {
                token = token,
                status = "Success",
                message = "Logged In"
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Exception: {ex.Message} : {dto.email}");
            return Ok(new AuthResponseDto
            {
                token = null,
                status = "Failure",
                message = $"Unexpected error in Login: {ex.Message}"
            });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse>> GetById(int id)
    {
        try
        {
            var user = await _repo.GetByIdAsync(id);
            if (user == null)
                return Ok(new ApiResponse
                {
                    status = "Failure",
                    message = "User not found"
                });

            return Ok(new ApiResponse
            {
                status = "Success",
                message = "User retrieved"
            });
        }
        catch (Exception ex)
        {
            return Ok(new ApiResponse
            {
                status = "Failure",
                message = $"Unexpected error in GetById: {ex.Message}"
            });
        }
    }
    private static ConcurrentDictionary<string, (string Otp, DateTime Expiry)> otpStore
            = new ConcurrentDictionary<string, (string, DateTime)>();

    [HttpPost("send-otp")]
    public async Task<ActionResult<ApiResponse>> SendOtp([FromBody] EmailTarget ReqBody)
    {
        try
        {
            _logger.LogInformation($"Otp API called for {ReqBody.Email}");
            if (!await _repo.CheckEmailExistsAsync(ReqBody.Email))
            {
                _logger.LogWarning($"Message:No such email exists");
                return new ApiResponse
                {
                    status = "Failure",
                    message = "No such email exists"
                };
            }
            var random = new Random();
            var otp = random.Next(100000, 999999).ToString();
            otpStore[ReqBody.Email] = (otp, DateTime.UtcNow.AddMinutes(1));

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("<your email>", "<your email app password>"),
                EnableSsl = true,
            };
            var mailMessage = new MailMessage
            {
                From = new MailAddress("HRteam@gmail.com", "Mail Confirmation"),
                Subject = "Your OTP Code",
                Body = $"Your OTP is: {otp}",
                IsBodyHtml = false,
            };
            mailMessage.To.Add(ReqBody.Email);

            await smtpClient.SendMailAsync(mailMessage);

            _logger.LogInformation($"Message:OTP Sent Successfully: {ReqBody.Email}");
            return Ok(new ApiResponse
            {
                status = "Success",
                message = "OTP sent successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Exception: {ex.Message} : {ReqBody.Email}");
            return Ok(new ApiResponse
            {
                status = "Failure",
                message = $"Unexpected error in SendOtp: {ex.Message}"
            });
        }
    }

    [HttpPost("verify-otp")]
    public async Task<ActionResult<ApiResponse>> VerifyOtp([FromBody] OtpVerifyRequest request)
    {
        try
        {
            _logger.LogInformation($"OTP verification called for: {request.Email}");
            if (otpStore.TryGetValue(request.Email, out var entry))
            {
                if (DateTime.UtcNow > entry.Expiry)
                {
                    _logger.LogWarning($"Message: OTP Expired: {request.Email}");
                    return Ok(new ApiResponse
                    {
                        status = "Failure",
                        message = "OTP expired"
                    });
                }

                if (entry.Otp == request.Otp)
                {
                    _logger.LogInformation($"Message: OTP verified successfully: {request.Email}");
                    otpStore.TryRemove(request.Email, out _); // Remove after success
                    return Ok(new ApiResponse
                    {
                        status = "Success",
                        message = "OTP verified successfully"
                    });
                }
                else
                {
                    _logger.LogWarning($"Message: Invalid OTP: {request.Email}");
                    return Ok(new ApiResponse
                    {
                        status = "Failure",
                        message = "Invalid OTP"
                    });
                }
            }

            _logger.LogWarning($"No OTP found for this email: {request.Email}");
            return Ok(new ApiResponse
            {
                status = "Failure",
                message = "No OTP found for this email"
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Error: {ex.Message} : {request.Email}");
            return Ok(new ApiResponse
            {
                status = "Failure",
                message = $"Unexpected error in VerifyOtp: {ex.Message}"
            });
        }
    }
    [HttpPut("{email}/set-password")]
    public async Task<ActionResult<ApiResponse>> SetPassword(string email, [FromBody] SetPassword request)
    {
        try
        {
            _logger.LogInformation($"Set password called for: {email}");
            var result = await _repo.UpdatePasswordAsync(email, request.Password);
            if (result.status == "Success")
            {
                _logger.LogInformation($"Message: {result.message}");
            }
            else
            {
                _logger.LogWarning($"Message: {result.message}");
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Exception: {ex.Message} : {email}");
            return Ok(new ApiResponse
            {
                status = "Failure",
                message = $"Unexpected error in SetPassword: {ex.Message}"
            });
        }
    }
}
