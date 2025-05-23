using SwitchWebManager.Data;
using Microsoft.EntityFrameworkCore;
using SwitchWebManager.Models;



var builder = WebApplication.CreateBuilder(args);

// Добавляем сервисы
builder.Services.AddRazorPages();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=users.db"));


builder.Services.AddAuthentication("Cookies")
    .AddCookie(options =>
    {
        options.Cookie.Name = "AuthCookie";
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Время жизни куки
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Middleware pipeline
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Маршруты
app.MapRazorPages();
app.MapGet("/", () => Results.Redirect("/Index")); // Изменено с /Login на /Index
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated(); // Создаёт базу и таблицы, если их нет

    if (!db.Users.Any())
    {
        var (hash, salt) = LoginModel.HashPassword("12345");
        db.Users.Add(new User
        {
            Username = "admin",
            PasswordHash = hash,
            Salt = salt
        });
        db.SaveChanges();
    }
}

app.Run();

Console.WriteLine($"База данных: {Path.GetFullPath("users.db")}");