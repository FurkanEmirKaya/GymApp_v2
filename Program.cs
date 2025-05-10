using GymApp_v1.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>(); 

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Authentication/Login"; // yetkisiz kullan�c�lar� buraya yollar
    });

builder.Services.AddDbContext<DataContext>(options =>
    options.UseMySql(
        configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(configuration.GetConnectionString("DefaultConnection"))
    )
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Varsay�lan route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Authentication}/{action=Login}/{id?}");

// Uygulama ba�lang�c�nda varsay�lan kullan�c�lar� olu�tur
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DataContext>();

    if (!context.Users.Any(u => u.Email == "admin@gym.com"))
    {
        var admin = new User
        {
            Email = "admin@gym.com",
            Password = "123456", // sade �ifre, sadece test i�in
            Role = "Admin",
            Username = "admin"
        };

        context.Users.Add(admin);
        context.SaveChanges();
    }

    if (!context.Users.Any(u => u.Email == "user@gym.com"))
    {
        var normalUser = new User
        {
            Email = "user@gym.com",
            Password = "123456", // sade �ifre, sadece test i�in
            Role = "User",
            Username = "gymuser"
        };

        context.Users.Add(normalUser);
        context.SaveChanges();
    }
}

app.Run();
