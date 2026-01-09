using AcuarioWebs.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace AcuarioWebs.Controllers
{
    public class HomeController : Controller
    {
        //private readonly ILogger<HomeController> _logger;
        private AcuarioContext _context;
        //public HomeController(ILogger<HomeController> logger)
        //{
        //    _logger = logger;
        //}
        public HomeController(AcuarioContext context)
        {
            _context = context;
        }
        

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
      
        public async Task<IActionResult> Login(string email, string pass)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
            {
                ViewBag.Error = "Debe ingresar email y/o contraseña";
                return View();
            }
            var user = await _context.Usuarios.Include(x => x.IdRolNavigation).FirstOrDefaultAsync(x => x.Email == email && x.Pass == pass);
            if (user == null)
            {
                ViewBag.Error = "Email y/o contraseña incorrectas.";
                return View();
            }
            // utilizamos claims para guardar la información del usaurio
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.IdUser.ToString()),
                new Claim(ClaimTypes.Name, user.Nombre),
                new Claim(ClaimTypes.Role, user.IdRolNavigation.Rol1),
            };
            //creamos identidad y principal para las coockies
            var claimsIdentify = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentify);
            // Iniciar sesión con autenticación por cookies
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);
            //asignamos a una variable sesión el nombre de usuario ingresado
            HttpContext.Session.SetString("nombre", user.Nombre);
            TempData["nombre"] = HttpContext.Session.GetString("nombre");
            //capturar rol
            //HttpContext.Session.SetString("rol", user.IdRolNavigation.Rol1);
            //TempData["rol"] = HttpContext.Session.GetString("rol");
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Logout()
        {
            // cerrar la sisión 
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData.Clear();
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
