using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GymApp_v1.Models;
using GymApp_v1.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace GymApp_v1.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly DataContext _context;
    
    public HomeController(ILogger<HomeController> logger, DataContext context)
    {
        _logger = logger;
        _context = context;
    }

    [Authorize] // Tüm kullanıcılar için giriş gerekli
    public IActionResult Index()
    {
        // Giriş yapmış kullanıcının bilgilerini alıyoruz
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        
        if (string.IsNullOrEmpty(userEmail))
        {
            return RedirectToAction("Login", "Authentication");
        }
        
        // Rol kontrolü ile yönlendirme
        if (userRole == "Admin")
        {
            return RedirectToAction("ListAllUsers", "Admin");
        }
        else
        {
            return RedirectToAction("Dashboard", "Profile");
        }
    }
    

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}