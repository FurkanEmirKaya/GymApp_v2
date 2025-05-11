using GymApp_v1.Data;
using GymApp_v2.Services;
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
        ServerVersion.AutoDetect(configuration.GetConnectionString("DefaultConnection")),
        mySqlOptions => mySqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
    )
);

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();

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
    
app.MapControllers(); 
// Uygulama ba�lang�c�nda varsay�lan kullan�c�lar� olu�tur
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DataContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
    if (!context.Users.Any(u => u.Email == "admin@gym.com"))
    {
        var adminHash = hasher.HashPassword(null!, "123456");
        var admin = new User
        {
            Email = "admin@gym.com",
            Password = adminHash, // sadece test için
            Role = "Admin",
            Username = "admin"
        };
        
        context.Users.Add(admin);
        context.SaveChanges();
    }

    if (!context.Users.Any(u => u.Email == "user@gym.com"))
    {   
        var userHash = hasher.HashPassword(null!, "123456");
        var normalUser = new User
        {
            Email = "user@gym.com",
            Password = userHash, // sadece test için
            Role = "User",
            Username = "gymuser"
        };

        context.Users.Add(normalUser);
        context.SaveChanges();
    }
}

app.Run();
