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
    public class AlimentoosController : Controller
    {
        private readonly AcuarioContext _context;

        public AlimentoosController(AcuarioContext context)
        {
            _context = context;
        }

        // GET: Alimentoos

        [Authorize(Policy = "AdminOrGerente")]
        public async Task<IActionResult> Index(int? numPag, string filtro, string buscar)
        {
            if (string.IsNullOrEmpty(buscar))
                buscar = filtro;
            else
                numPag = 1;
            ViewData["filtro"] = buscar;
            var alimento = from c in _context.Alimentoos select c;
            if (!string.IsNullOrEmpty(buscar))
                alimento = alimento.Where(x => x.NombreAlimento.ToLower().Contains(buscar.ToLower()));
            int tamPag = 20;
            return View(await PaginatedList<Alimentoo>.CreateAsync(alimento, numPag ?? 1, tamPag));
        }

        // Exportar a Excel
        [Authorize(Policy = "AdminOrGerente")]
        public async Task<IActionResult> ExportarExcel(string filtro)
        {
            var alimentos = _context.Alimentoos.AsQueryable();

            if (!string.IsNullOrEmpty(filtro))
                alimentos = alimentos.Where(x => x.NombreAlimento.ToLower().Contains(filtro.ToLower()));

            var listaAlimentos = await alimentos.ToListAsync();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Alimentos");

                // Encabezados
                worksheet.Cells[1, 1].Value = "Nombre Alimento";
                worksheet.Cells[1, 2].Value = "Tipo";

                // Estilo de encabezados
                using (var range = worksheet.Cells[1, 1, 1, 2])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                // Datos
                int fila = 2;
                foreach (var alimento in listaAlimentos)
                {
                    worksheet.Cells[fila, 1].Value = alimento.NombreAlimento;
                    worksheet.Cells[fila, 2].Value = alimento.Tipo;
                    fila++;
                }

                worksheet.Cells.AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string fileName = $"Alimentos_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        // Exportar a PDF
        [Authorize(Policy = "AdminOrGerente")]
        public async Task<IActionResult> ExportarPDF(string filtro)
        {
            var alimentos = _context.Alimentoos.AsQueryable();

            if (!string.IsNullOrEmpty(filtro))
                alimentos = alimentos.Where(x => x.NombreAlimento.ToLower().Contains(filtro.ToLower()));

            var listaAlimentos = await alimentos.ToListAsync();

            using (var stream = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                document.Open();

                // Título
                Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                Paragraph title = new Paragraph("Lista de Alimentos", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 20;
                document.Add(title);

                // Tabla
                PdfPTable table = new PdfPTable(2);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 3f, 2f });

                // Encabezados
                Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                PdfPCell headerCell;

                string[] headers = { "Nombre Alimento", "Tipo" };
                foreach (var header in headers)
                {
                    headerCell = new PdfPCell(new Phrase(header, headerFont));
                    headerCell.BackgroundColor = new BaseColor(144, 238, 144); // Verde claro
                    headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
                    headerCell.Padding = 5;
                    table.AddCell(headerCell);
                }

                // Datos
                Font dataFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                foreach (var alimento in listaAlimentos)
                {
                    table.AddCell(new PdfPCell(new Phrase(alimento.NombreAlimento ?? "", dataFont)) { Padding = 5 });
                    table.AddCell(new PdfPCell(new Phrase(alimento.Tipo ?? "", dataFont)) { Padding = 5 });
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

                string fileName = $"Alimentos_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                return File(stream.ToArray(), "application/pdf", fileName);
            }
        }

        // GET: Alimentoos/Details/5
        [Authorize(Policy = "AdminOrGerente")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alimentoo = await _context.Alimentoos
                .FirstOrDefaultAsync(m => m.IdAlimento == id);
            if (alimentoo == null)
            {
                return NotFound();
            }

            return View(alimentoo);
        }

        // GET: Alimentoos/Create
        [Authorize(Policy = "AdminOrGerente")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Alimentoos/Create
        [Authorize(Policy = "AdminOrGerente")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdAlimento,NombreAlimento,Tipo")] Alimentoo alimentoo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(alimentoo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(alimentoo);
        }

        // GET: Alimentoos/Edit/5
        [Authorize(Policy = "AdminOrGerente")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alimentoo = await _context.Alimentoos.FindAsync(id);
            if (alimentoo == null)
            {
                return NotFound();
            }
            return View(alimentoo);
        }

        // POST: Alimentoos/Edit/5
        [Authorize(Policy = "AdminOrGerente")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdAlimento,NombreAlimento,Tipo")] Alimentoo alimentoo)
        {
            if (id != alimentoo.IdAlimento)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(alimentoo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlimentooExists(alimentoo.IdAlimento))
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
            return View(alimentoo);
        }

        // Verificar Alimento (validación de duplicados)
        public JsonResult VerificarAlimento(string nombre, string tipo)
        {
            bool existe = _context.Alimentoos
                .Any(a => a.NombreAlimento.ToLower() == nombre.ToLower() && a.Tipo.ToLower() == tipo.ToLower());

            return Json(new { existe });
        }

        // DELETE: Alimentoos/Delete
        [Authorize(Roles = "Admin")]
        public JsonResult Delete(int id)
        {
            try 
            {
                var alimento = _context.Alimentoos.Find(id);
                if (alimento == null)
                    return Json("noencontrado");

                bool tieneAtenciones = _context.Atenciones.Any(a => a.IdAlimento == id);
                if (tieneAtenciones)
                    return Json("relacion");

                _context.Alimentoos.Remove(alimento);
                _context.SaveChanges();
                return Json("ok");
            }
            catch (Exception)
            {
                return Json("error");
            }
        }

        private bool AlimentooExists(int id)
        {
            return _context.Alimentoos.Any(e => e.IdAlimento == id);
        }
    }
}