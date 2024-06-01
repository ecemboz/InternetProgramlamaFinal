using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{
    public class StartpController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            // Kullanıcının zaten login olup olmadığını kontrol edin
            ClaimsPrincipal claimUser = HttpContext.User;
            if (claimUser.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(Logincs logincs)
        {
            // Burada veritabanı doğrulaması yapılacak (Örnek statik kontrol)
            if (logincs.Email == "ornek@mail.com" && logincs.PassWord == "123")
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, logincs.Email),
                    new Claim("OtherProperties", "Example Role")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    IsPersistent = logincs.LoggedStatus
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
                return RedirectToAction("Index", "Home");
            }

            ViewData["OnayMesaji"] = "Kullanıcı Bulunamadı";
            return View();
        }
    }
}
