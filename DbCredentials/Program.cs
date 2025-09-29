using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DbCredentials.Data;
using DbCredentials.Models;
using DbCredentials.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using DbCredentials.Repositories;
using DbCredentials.LoggingClass;

namespace DbCredentials
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddScoped<IUserRepository, UserRepository>();
            
            builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();


            builder.Services.AddSingleton<TokenService>();

            var jwt = builder.Configuration.GetSection("Jwt");
            var key = jwt.GetValue<string>("Key");
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                options.SaveToken = true;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.GetValue<string>("Issuer"),
                    ValidAudience = jwt.GetValue<string>("Audience"),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
                };
            });
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddProvider(new FileLoggerProvider("Logs/logs.txt"));
            builder.Logging.AddFilter((category, level) =>
            {
                // Allow only your namespaces (e.g., Controllers, Services)
                return category.StartsWith("UsersController")
                    || category.StartsWith("DbCredentials");
            });
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy
                        .WithOrigins("http://localhost:3000") // React frontend URL
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors();

            app.UseRouting();

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
