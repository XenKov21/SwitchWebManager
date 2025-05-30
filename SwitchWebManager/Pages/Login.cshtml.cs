using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SwitchWebManager.Data;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using SwitchWebManager.Models;

public class LoginModel : PageModel
{
    private readonly AppDbContext _db;

    public LoginModel(AppDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    [Required]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    public string Password { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == Username);
        if (user != null && VerifyPassword(Password, user)) // Передаём user вместо user.PasswordHash
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username)
        };

            var identity = new ClaimsIdentity(claims, "Cookies");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("Cookies", principal);
            return RedirectToPage("/Index");
        }

        ErrorMessage = "Неверный логин или пароль";
        return Page();
    }
    private bool VerifyPassword(string password, User user)
    {
        byte[] salt = Convert.FromBase64String(user.Salt); // Получаем соль из пользователя
        string hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 10000,
            numBytesRequested: 256 / 8));
        return hash == user.PasswordHash;
    }
    public static (string Hash, string Salt) HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(128 / 8); // Генерируем случайную соль
        string hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 10000,
            numBytesRequested: 256 / 8));
        return (hash, Convert.ToBase64String(salt));
    }
}
