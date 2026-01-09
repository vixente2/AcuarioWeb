using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AcuarioWebs.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AcuarioWebs.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly AcuarioContext _context;

        public DashboardController(AcuarioContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var dashboard = new DashboardViewModel
            {
                TotalUsuarios = await _context.Usuarios.CountAsync(),
                TotalPeces = await _context.Peces.CountAsync(),
                TotalAlimentos = await _context.Alimentoos.CountAsync(),
                TotalPeceras = await _context.Peceraas.CountAsync(),
                TotalAtenciones = await _context.Atenciones.CountAsync(),
                
                // Atenciones del mes actual
                AtencionesEsteMes = await _context.Atenciones
                    .Where(a => a.Fecha.Month == DateTime.Now.Month && a.Fecha.Year == DateTime.Now.Year)
                    .CountAsync(),
                
                // Últimas 5 atenciones
                UltimasAtenciones = await _context.Atenciones
                    .Include(a => a.IdPecesNavigation)
                    .Include(a => a.IdAlimentoNavigation)
                    .Include(a => a.IdUserNavigation)
                    .OrderByDescending(a => a.Fecha)
                    .Take(5)
                    .ToListAsync(),
                
                // Top 5 peces con más atenciones
                PecesConMasAtenciones = await _context.Peces
                    .Select(p => new PezAtencionCount
                    {
                        NombrePez = p.NombrePez,
                        CantidadAtenciones = p.Atenciones.Count
                    })
                    .OrderByDescending(p => p.CantidadAtenciones)
                    .Take(5)
                    .ToListAsync(),
                
                // Atenciones por mes (últimos 6 meses)
                AtencionesPorMes = await ObtenerAtencionesPorMes()
                    
            };

            return View(dashboard);
        }

        private async Task<List<AtencionMensual>> ObtenerAtencionesPorMes()
        {
            var resultado = new List<AtencionMensual>();
            var fechaInicio = DateTime.Now.AddMonths(-5).Date;

            for (int i = 0; i < 6; i++)
            {
                var mes = fechaInicio.AddMonths(i);
                var count = await _context.Atenciones
                    .Where(a => a.Fecha.Year == mes.Year && a.Fecha.Month == mes.Month)
                    .CountAsync();

                resultado.Add(new AtencionMensual
                {
                    Mes = mes.ToString("MMM yyyy"),
                    Cantidad = count
                });
            }

            return resultado;
        }

        // API para gráficos (Chart.js)
        [HttpGet]
        public async Task<JsonResult> ObtenerDatosGraficos()
        {
            var datos = new
            {
                atencionesEsteMes = await _context.Atenciones
                    .Where(a => a.Fecha.Month == DateTime.Now.Month && a.Fecha.Year == DateTime.Now.Year)
                    .CountAsync(),
                
                alimentosMasUsados = await _context.Alimentoos
                    .Select(a => new
                    {
                        nombre = a.NombreAlimento,
                        cantidad = a.Atenciones.Count
                    })
                    .OrderByDescending(a => a.cantidad)
                    .Take(5)
                    .ToListAsync(),

                pecesPorPecera = await _context.Peceraas
    .Select(p => new
    {
        pecera = p.NombrePecera,
        cantidad = p.Peces.Count
    })
    .OrderByDescending(p => p.cantidad) // <-- ordenar descendente por cantidad
    .ToListAsync()
            };

            return Json(datos);
        }
    }

    // ViewModels
    public class DashboardViewModel
    {
        public int TotalUsuarios { get; set; }
        public int TotalPeces { get; set; }
        public int TotalAlimentos { get; set; }
        public int TotalPeceras { get; set; }
        public int TotalAtenciones { get; set; }
        public int AtencionesEsteMes { get; set; }
        public List<Atencione> UltimasAtenciones { get; set; }
        public List<PezAtencionCount> PecesConMasAtenciones { get; set; }
        public List<AtencionMensual> AtencionesPorMes { get; set; }
    }

    public class PezAtencionCount
    {
        public string NombrePez { get; set; }
        public int CantidadAtenciones { get; set; }
    }

    public class AtencionMensual
    {
        public string Mes { get; set; }
        public int Cantidad { get; set; }
    }
}