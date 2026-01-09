using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AcuarioWebs.Models;
using AcuarioWebs.Helpers;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using Microsoft.AspNetCore.Authorization;

namespace AcuarioWebs.Controllers
{
    public class AtencionesController : Controller
    {
        private readonly AcuarioContext _context;

        public AtencionesController(AcuarioContext context)
        {
            _context = context;
        }

        // GET: Atenciones
        [Authorize(Policy = "AdminOrGerente")]
        public async Task<IActionResult> Index(int? numPag, string filtro, string buscar)
        {
            if (string.IsNullOrEmpty(buscar))
                buscar = filtro;
            else
                numPag = 1;
            ViewData["filtro"] = buscar;
            var atencione = _context.Atenciones
                .Include(a => a.IdAlimentoNavigation)
                .Include(a => a.IdPecesNavigation)
                .Include(a => a.IdUserNavigation)
                .AsQueryable();
            if (!string.IsNullOrEmpty(buscar))
                atencione = atencione.Where(x => x.TipoActividad.ToLower().Contains(buscar.ToLower()));
            int tamPag = 20;
            return View(await PaginatedList<Atencione>.CreateAsync(atencione, numPag ?? 1, tamPag));
        }

        // Exportar a Excel
        [Authorize(Policy = "AdminOrGerente")]
        public async Task<IActionResult> ExportarExcel(string filtro)
        {
            var atenciones = _context.Atenciones
                .Include(a => a.IdAlimentoNavigation)
                .Include(a => a.IdPecesNavigation)
                .Include(a => a.IdUserNavigation)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filtro))
                atenciones = atenciones.Where(x => x.TipoActividad.ToLower().Contains(filtro.ToLower()));

            var listaAtenciones = await atenciones.ToListAsync();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Atenciones");

                // Encabezados
                worksheet.Cells[1, 1].Value = "Tipo Actividad";
                worksheet.Cells[1, 2].Value = "Fecha";
                worksheet.Cells[1, 3].Value = "Alimento";
                worksheet.Cells[1, 4].Value = "Pez";
                worksheet.Cells[1, 5].Value = "Usuario";

                // Estilo de encabezados
                using (var range = worksheet.Cells[1, 1, 1, 5])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightCoral);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                // Datos
                int fila = 2;
                foreach (var atencion in listaAtenciones)
                {
                    worksheet.Cells[fila, 1].Value = atencion.TipoActividad;
                    worksheet.Cells[fila, 2].Value = atencion.Fecha.ToString("dd/MM/yyyy");
                    worksheet.Cells[fila, 3].Value = atencion.IdAlimentoNavigation?.NombreAlimento;
                    worksheet.Cells[fila, 4].Value = atencion.IdPecesNavigation?.NombrePez;
                    worksheet.Cells[fila, 5].Value = atencion.IdUserNavigation?.Nombre;
                    fila++;
                }

                worksheet.Cells.AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string fileName = $"Atenciones_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        // Exportar a PDF
        [Authorize(Policy = "AdminOrGerente")]
        public async Task<IActionResult> ExportarPDF(string filtro)
        {
            var atenciones = _context.Atenciones
                .Include(a => a.IdAlimentoNavigation)
                .Include(a => a.IdPecesNavigation)
                .Include(a => a.IdUserNavigation)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filtro))
                atenciones = atenciones.Where(x => x.TipoActividad.ToLower().Contains(filtro.ToLower()));

            var listaAtenciones = await atenciones.ToListAsync();

            using (var stream = new MemoryStream())
            {
                Document document = new Document(PageSize.A4.Rotate(), 25, 25, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                document.Open();

                // Título
                Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                Paragraph title = new Paragraph("Lista de Atenciones", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 20;
                document.Add(title);

                // Tabla
                PdfPTable table = new PdfPTable(5);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 2.5f, 2f, 2.5f, 2.5f, 2f });

                // Encabezados
                Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                PdfPCell headerCell;

                string[] headers = { "Tipo Actividad", "Fecha", "Alimento", "Pez", "Usuario" };
                foreach (var header in headers)
                {
                    headerCell = new PdfPCell(new Phrase(header, headerFont));
                    headerCell.BackgroundColor = new BaseColor(240, 128, 128); // Coral claro
                    headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
                    headerCell.Padding = 5;
                    table.AddCell(headerCell);
                }

                // Datos
                Font dataFont = FontFactory.GetFont(FontFactory.HELVETICA, 9);
                foreach (var atencion in listaAtenciones)
                {
                    table.AddCell(new PdfPCell(new Phrase(atencion.TipoActividad ?? "", dataFont)) { Padding = 5 });
                    table.AddCell(new PdfPCell(new Phrase(atencion.Fecha.ToString("dd/MM/yyyy"), dataFont)) { Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER });
                    table.AddCell(new PdfPCell(new Phrase(atencion.IdAlimentoNavigation?.NombreAlimento ?? "", dataFont)) { Padding = 5 });
                    table.AddCell(new PdfPCell(new Phrase(atencion.IdPecesNavigation?.NombrePez ?? "", dataFont)) { Padding = 5 });
                    table.AddCell(new PdfPCell(new Phrase(atencion.IdUserNavigation?.Nombre ?? "", dataFont)) { Padding = 5 });
                }

                document.Add(table);

                // Pie de página
                Paragraph footer = new Paragraph($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}",
                    FontFactory.GetFont(FontFactory.HELVETICA, 8));
                footer.Alignment = Element.ALIGN_RIGHT;
                footer.SpacingBefore = 20;
                document.Add(footer);

                document.Close();
                writer.Close();

                string fileName = $"Atenciones_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                return File(stream.ToArray(), "application/pdf", fileName);
            }
        }

        // GET: Atenciones/Details/5
        [Authorize(Policy = "AdminOrGerente")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var atencione = await _context.Atenciones
                .Include(a => a.IdAlimentoNavigation)
                .Include(a => a.IdPecesNavigation)
                .Include(a => a.IdUserNavigation)
                .FirstOrDefaultAsync(m => m.IdAtencion == id);
            if (atencione == null)
            {
                return NotFound();
            }

            return View(atencione);
        }

        // GET: Atenciones/Create
        [Authorize(Policy = "AdminOrGerente")]
        public IActionResult Create()
        {
            ViewData["IdAlimento"] = new SelectList(_context.Alimentoos, "IdAlimento", "NombreAlimento");
            ViewData["IdPeces"] = new SelectList(_context.Peces, "IdPeces", "NombrePez");
            ViewData["IdUser"] = new SelectList(_context.Usuarios, "IdUser", "Nombre");
            return View();
        }

        // POST: Atenciones/Create
        [Authorize(Policy = "AdminOrGerente")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdAtencion,TipoActividad,Fecha,IdUser,IdPeces,IdAlimento")] Atencione atencione)
        {
            if (ModelState.IsValid)
            {
                _context.Add(atencione);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdAlimento"] = new SelectList(_context.Alimentoos, "IdAlimento", "NombreAlimento", atencione.IdAlimento);
            ViewData["IdPeces"] = new SelectList(_context.Peces, "IdPeces", "NombrePez", atencione.IdPeces);
            ViewData["IdUser"] = new SelectList(_context.Usuarios, "IdUser", "Nombre", atencione.IdUser);
            return View(atencione);
        }

        // GET: Atenciones/Edit/5
        [Authorize(Policy = "AdminOrGerente")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var atencione = await _context.Atenciones.FindAsync(id);
            if (atencione == null)
            {
                return NotFound();
            }
            ViewData["IdAlimento"] = new SelectList(_context.Alimentoos, "IdAlimento", "NombreAlimento", atencione.IdAlimento);
            ViewData["IdPeces"] = new SelectList(_context.Peces, "IdPeces", "NombrePez", atencione.IdPeces);
            ViewData["IdUser"] = new SelectList(_context.Usuarios, "IdUser", "Nombre", atencione.IdUser);
            return View(atencione);
        }

        // POST: Atenciones/Edit/5
        [Authorize(Policy = "AdminOrGerente")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdAtencion,TipoActividad,Fecha,IdUser,IdPeces,IdAlimento")] Atencione atencione)
        {
            if (id != atencione.IdAtencion)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(atencione);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AtencioneExists(atencione.IdAtencion))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdAlimento"] = new SelectList(_context.Alimentoos, "IdAlimento", "IdAlimento", atencione.IdAlimento);
            ViewData["IdPeces"] = new SelectList(_context.Peces, "IdPeces", "IdPeces", atencione.IdPeces);
            ViewData["IdUser"] = new SelectList(_context.Usuarios, "IdUser", "IdUser", atencione.IdUser);
            return View(atencione);
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var atencione = await _context.Atenciones.FindAsync(id);
            if (atencione == null)
            {
                return Json("notfound");
            }

            _context.Atenciones.Remove(atencione);
            await _context.SaveChangesAsync();

            return Json("ok");
        }

        public JsonResult VerificarAtencion(int idUser, DateTime fecha, int? idAtencion)
        {
            bool existe = _context.Atenciones.Any(a =>
                a.IdUser == idUser &&
                a.Fecha == fecha &&
                (!idAtencion.HasValue || a.IdAtencion != idAtencion.Value)
            );

            return Json(new { existe });
        }

        private bool AtencioneExists(int id)
        {
            return _context.Atenciones.Any(e => e.IdAtencion == id);
        }
    }
}