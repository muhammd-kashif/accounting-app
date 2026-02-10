using AccountingApp.Data;
using AccountingApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace AccountingApp.Services
{
    public interface IAuthService
    {
        Task<User?> LoginAsync(string email, string password);
        Task<(bool Success, string Message)> RegisterAsync(string email, string password, string fullName);
        bool VerifyPassword(string password, string hash);
        string HashPassword(string password);
    }

    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> LoginAsync(string email, string password)
        {
            var normalizedEmail = NormalizeEmail(email);
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);
            if (user == null)
            {
                return null;
            }

            if (!user.IsActive)
            {
                user.IsActive = true;
                await _context.SaveChangesAsync();
            }

            return VerifyPassword(password, user.PasswordHash) ? user : null;
        }

        public async Task<(bool Success, string Message)> RegisterAsync(string email, string password, string fullName)
        {
            var normalizedEmail = NormalizeEmail(email);
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);
            if (existingUser != null)
                return (false, "Email already registered hai");

            var user = new User
            {
                Email = normalizedEmail,
                FullName = fullName,
                PasswordHash = HashPassword(password),
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return (true, "Registration successful!");
        }

        public string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        public bool VerifyPassword(string password, string hash)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hash;
        }

        private static string NormalizeEmail(string email)
        {
            return (email ?? string.Empty).Trim().ToLowerInvariant();
        }
    }
}
