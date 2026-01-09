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
    public class PeceraasController : Controller
    {
        private readonly AcuarioContext _context;

        public PeceraasController(AcuarioContext context)
        {
            _context = context;
        }

        // GET: Peceraas
        public async Task<IActionResult> Index(int? numPag, string filtro, string buscar)
        {
            if (string.IsNullOrEmpty(buscar))
                buscar = filtro;
            else
                numPag = 1;
            ViewData["filtro"] = buscar;
            var pecera = from c in _context.Peceraas select c;
            if (!string.IsNullOrEmpty(buscar))
                pecera = pecera.Where(x => x.NombrePecera.ToLower().Contains(buscar.ToLower()));
            int tamPag = 20;
            return View(await PaginatedList<Peceraa>.CreateAsync(pecera, numPag ?? 1, tamPag));
        }

        // Exportar a Excel
        public async Task<IActionResult> ExportarExcel(string filtro)
        {
            var peceras = _context.Peceraas.AsQueryable();

            if (!string.IsNullOrEmpty(filtro))
                peceras = peceras.Where(x => x.NombrePecera.ToLower().Contains(filtro.ToLower()));

            var listaPeceras = await peceras.ToListAsync();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Peceras");

                // Encabezados
                worksheet.Cells[1, 1].Value = "Nombre Pecera";
                worksheet.Cells[1, 2].Value = "Litros";
                worksheet.Cells[1, 3].Value = "Temperatura";
                worksheet.Cells[1, 4].Value = "Ph";

                // Estilo de encabezados
                using (var range = worksheet.Cells[1, 1, 1, 4])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightSkyBlue);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                // Datos
                int fila = 2;
                foreach (var pecera in listaPeceras)
                {
                    worksheet.Cells[fila, 1].Value = pecera.NombrePecera;
                    worksheet.Cells[fila, 2].Value = pecera.Litros;
                    worksheet.Cells[fila, 3].Value = pecera.Temperatura;
                    worksheet.Cells[fila, 4].Value = pecera.Ph;
                    fila++;
                }

                worksheet.Cells.AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string fileName = $"Peceras_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        // Exportar a PDF
        public async Task<IActionResult> ExportarPDF(string filtro)
        {
            var peceras = _context.Peceraas.AsQueryable();

            if (!string.IsNullOrEmpty(filtro))
                peceras = peceras.Where(x => x.NombrePecera.ToLower().Contains(filtro.ToLower()));

            var listaPeceras = await peceras.ToListAsync();

            using (var stream = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                document.Open();

                // Título
                Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                Paragraph title = new Paragraph("Lista de Peceras", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 20;
                document.Add(title);

                // Tabla
                PdfPTable table = new PdfPTable(4);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 3f, 2f, 2f, 1.5f });

                // Encabezados
                Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                PdfPCell headerCell;

                string[] headers = { "Nombre Pecera", "Litros", "Temperatura", "Ph" };
                foreach (var header in headers)
                {
                    headerCell = new PdfPCell(new Phrase(header, headerFont));
                    headerCell.BackgroundColor = new BaseColor(135, 206, 250); // Azul cielo claro
                    headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
                    headerCell.Padding = 5;
                    table.AddCell(headerCell);
                }

                // Datos
                Font dataFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                foreach (var pecera in listaPeceras)
                {
                    table.AddCell(new PdfPCell(new Phrase(pecera.NombrePecera ?? "", dataFont)) { Padding = 5 });
                    table.AddCell(new PdfPCell(new Phrase(pecera.Litros.ToString() ?? "", dataFont)) { Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER });
                    table.AddCell(new PdfPCell(new Phrase(pecera.Temperatura.ToString() ?? "", dataFont)) { Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER });
                    table.AddCell(new PdfPCell(new Phrase(pecera.Ph.ToString() ?? "", dataFont)) { Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER });
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

                string fileName = $"Peceras_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                return File(stream.ToArray(), "application/pdf", fileName);
            }
        }

        // GET: Peceraas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var peceraa = await _context.Peceraas
                .FirstOrDefaultAsync(m => m.IdPecera == id);
            if (peceraa == null)
            {
                return NotFound();
            }

            return View(peceraa);
        }

        // GET: Peceraas/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Peceraas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdPecera,NombrePecera,Litros,Temperatura,Ph")] Peceraa peceraa)
        {
            if (ModelState.IsValid)
            {
                _context.Add(peceraa);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(peceraa);
        }

        // GET: Peceraas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var peceraa = await _context.Peceraas.FindAsync(id);
            if (peceraa == null)
            {
                return NotFound();
            }
            return View(peceraa);
        }

        // POST: Peceraas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdPecera,NombrePecera,Litros,Temperatura,Ph")] Peceraa peceraa)
        {
            if (id != peceraa.IdPecera)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(peceraa);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PeceraaExists(peceraa.IdPecera))
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
            return View(peceraa);
        }

        // DELETE: Peceraas/Delete/5
        [Authorize(Roles = "Admin")]
        public JsonResult Delete(int id)
        {
            try
            {
                var pecera = _context.Peceraas.Find(id);
                if (pecera == null)
                    return Json("noencontrado");

                var peces = _context.Peces.Where(p => p.IdPecera == id).ToList();
                if (peces.Any())
                {
                    bool tienenAtenciones = _context.Atenciones.Any(a => peces.Select(p => p.IdPeces).Contains(a.IdPeces));
                    if (tienenAtenciones)
                        return Json("relacion");
                    else
                        return Json("relacionPeces");
                }
                _context.Peceraas.Remove(pecera);
                _context.SaveChanges();
                return Json("ok");
            }
            catch (Exception)
            {
                return Json("error");
            }
        }

        public JsonResult VerificarNombre(string nombre, int? id)
        {
            bool existe = _context.Peceraas.Any(p => p.NombrePecera == nombre && (!id.HasValue || p.IdPecera != id.Value));
            return Json(new { existe });
        }

        private bool PeceraaExists(int id)
        {
            return _context.Peceraas.Any(e => e.IdPecera == id);
        }
    }
}