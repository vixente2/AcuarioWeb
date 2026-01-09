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
    public class PecesController : Controller
    {
        private readonly AcuarioContext _context;

        public PecesController(AcuarioContext context)
        {
            _context = context;
        }

        // GET: Peces
        public async Task<IActionResult> Index(int? numPag, string filtro, string buscar)
        {
            if (string.IsNullOrEmpty(buscar))
                buscar = filtro;
            else
                numPag = 1;
            ViewData["filtro"] = buscar;
            var peces = _context.Peces.Include(p => p.IdPeceraNavigation).AsQueryable();
            if (!string.IsNullOrEmpty(buscar))
                peces = peces.Where(x => x.NombrePez.ToLower().Contains(buscar.ToLower()));
            int tamPag = 20;
            return View(await PaginatedList<Pece>.CreateAsync(peces, numPag ?? 1, tamPag));
        }

        // Exportar a Excel
        public async Task<IActionResult> ExportarExcel(string filtro)
        {
            var peces = _context.Peces.Include(p => p.IdPeceraNavigation).AsQueryable();

            if (!string.IsNullOrEmpty(filtro))
                peces = peces.Where(x => x.NombrePez.ToLower().Contains(filtro.ToLower()));

            var listaPeces = await peces.ToListAsync();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Peces");

                // Encabezados
                worksheet.Cells[1, 1].Value = "Nombre del Pez";
                worksheet.Cells[1, 2].Value = "Especie";
                worksheet.Cells[1, 3].Value = "Edad";
                worksheet.Cells[1, 4].Value = "Pecera";

                // Estilo de encabezados
                using (var range = worksheet.Cells[1, 1, 1, 4])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                // Datos
                int fila = 2;
                foreach (var pez in listaPeces)
                {
                    worksheet.Cells[fila, 1].Value = pez.NombrePez;
                    worksheet.Cells[fila, 2].Value = pez.Especie;
                    worksheet.Cells[fila, 3].Value = pez.Edad;
                    worksheet.Cells[fila, 4].Value = pez.IdPeceraNavigation?.NombrePecera;
                    fila++;
                }

                worksheet.Cells.AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string fileName = $"Peces_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        // Exportar a PDF
        public async Task<IActionResult> ExportarPDF(string filtro)
        {
            var peces = _context.Peces.Include(p => p.IdPeceraNavigation).AsQueryable();

            if (!string.IsNullOrEmpty(filtro))
                peces = peces.Where(x => x.NombrePez.ToLower().Contains(filtro.ToLower()));

            var listaPeces = await peces.ToListAsync();

            using (var stream = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                document.Open();

                // Título
                Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                Paragraph title = new Paragraph("Lista de Peces", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 20;
                document.Add(title);

                // Tabla
                PdfPTable table = new PdfPTable(4);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 3f, 2.5f, 1.5f, 2.5f });

                // Encabezados
                Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                PdfPCell headerCell;

                string[] headers = { "Nombre del Pez", "Especie", "Edad", "Pecera" };
                foreach (var header in headers)
                {
                    headerCell = new PdfPCell(new Phrase(header, headerFont));
                    headerCell.BackgroundColor = new BaseColor(173, 216, 230); // Azul claro
                    headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
                    headerCell.Padding = 5;
                    table.AddCell(headerCell);
                }

                // Datos
                Font dataFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                foreach (var pez in listaPeces)
                {
                    table.AddCell(new PdfPCell(new Phrase(pez.NombrePez ?? "", dataFont)) { Padding = 5 });
                    table.AddCell(new PdfPCell(new Phrase(pez.Especie ?? "", dataFont)) { Padding = 5 });
                    table.AddCell(new PdfPCell(new Phrase(pez.Edad?.ToString() ?? "", dataFont)) { Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER });
                    table.AddCell(new PdfPCell(new Phrase(pez.IdPeceraNavigation?.NombrePecera ?? "", dataFont)) { Padding = 5 });
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

                string fileName = $"Peces_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                return File(stream.ToArray(), "application/pdf", fileName);
            }
        }

        // GET: Peces/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pece = await _context.Peces
                .Include(p => p.IdPeceraNavigation)
                .FirstOrDefaultAsync(m => m.IdPeces == id);
            if (pece == null)
            {
                return NotFound();
            }

            return View(pece);
        }

        // GET: Peces/Create
        public IActionResult Create()
        {
            ViewData["IdPecera"] = new SelectList(_context.Peceraas, "IdPecera", "NombrePecera");
            return View();
        }

        // POST: Peces/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdPeces,NombrePez,Especie,Edad,IdPecera")] Pece pece)
        {
            if (ModelState.IsValid)
            {
                _context.Add(pece);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdPecera"] = new SelectList(_context.Peceraas, "IdPecera", "NombrePecera", pece.IdPecera);
            return View(pece);
        }

        // GET: Peces/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pece = await _context.Peces.FindAsync(id);
            if (pece == null)
            {
                return NotFound();
            }
            ViewData["IdPecera"] = new SelectList(_context.Peceraas, "IdPecera", "NombrePecera", pece.IdPecera);
            return View(pece);
        }

        // POST: Peces/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdPeces,NombrePez,Especie,Edad,IdPecera")] Pece pece)
        {
            if (id != pece.IdPeces)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pece);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PeceExists(pece.IdPeces))
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
            ViewData["IdPecera"] = new SelectList(_context.Peceraas, "IdPecera", "NombrePecera", pece.IdPecera);
            return View(pece);
        }

        // GET: Peces/Delete/5
        [Authorize(Roles = "Admin")]
        public JsonResult Delete(int id)
        {
            try
            {
                var pez = _context.Peces.Find(id);
                if (pez == null)
                    return Json("noencontrado");

                bool tieneAtenciones = _context.Atenciones.Any(a => a.IdPeces == id);
                if (tieneAtenciones)
                    return Json("relacion");

                _context.Peces.Remove(pez);
                _context.SaveChanges();
                return Json("ok");
            }
            catch (Exception)
            {
                return Json("error");
            }
        }

        private bool PeceExists(int id)
        {
            return _context.Peces.Any(e => e.IdPeces == id);
        }
    }
}