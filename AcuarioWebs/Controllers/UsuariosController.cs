using AcuarioWebs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AcuarioWebs.Helpers;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;

namespace AcuarioWebs.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly AcuarioContext _context;

        public UsuariosController(AcuarioContext context)
        {
            _context = context;
        }

        // GET: Usuarios
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(int? numPag, string filtro, string buscar)
        {
            if (string.IsNullOrEmpty(buscar))
                buscar = filtro;
            else
                numPag = 1;
            ViewData["filtro"] = buscar;
            var usuario = _context.Usuarios.Include(u => u.IdRolNavigation).AsQueryable();
            if (!string.IsNullOrEmpty(buscar))
                usuario = usuario.Where(x => x.Nombre.ToLower().Contains(buscar.ToLower()));
            int tamPag = 20;
            return View(await PaginatedList<Usuario>.CreateAsync(usuario, numPag ?? 1, tamPag));
        }

        // Exportar a Excel
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportarExcel(string filtro)
        {
            var usuarios = _context.Usuarios.Include(u => u.IdRolNavigation).AsQueryable();

            if (!string.IsNullOrEmpty(filtro))
                usuarios = usuarios.Where(x => x.Nombre.ToLower().Contains(filtro.ToLower()));

            var listaUsuarios = await usuarios.ToListAsync();

            // Configuración para EPPlus 5.x
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Usuarios");

                // Encabezados
                worksheet.Cells[1, 1].Value = "Email";
                worksheet.Cells[1, 2].Value = "Nombre";
                worksheet.Cells[1, 3].Value = "Contraseña";
                worksheet.Cells[1, 4].Value = "Rol";

                // Estilo de encabezados
                using (var range = worksheet.Cells[1, 1, 1, 4])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                // Datos
                int fila = 2;
                foreach (var usuario in listaUsuarios)
                {
                    worksheet.Cells[fila, 1].Value = usuario.Email;
                    worksheet.Cells[fila, 2].Value = usuario.Nombre;
                    worksheet.Cells[fila, 3].Value = usuario.Pass;
                    worksheet.Cells[fila, 4].Value = usuario.IdRolNavigation?.Rol1;
                    fila++;
                }

                worksheet.Cells.AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string fileName = $"Usuarios_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        // Exportar a PDF
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportarPDF(string filtro)
        {
            var usuarios = _context.Usuarios.Include(u => u.IdRolNavigation).AsQueryable();

            if (!string.IsNullOrEmpty(filtro))
                usuarios = usuarios.Where(x => x.Nombre.ToLower().Contains(filtro.ToLower()));

            var listaUsuarios = await usuarios.ToListAsync();

            using (var stream = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                document.Open();

                // Título
                Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                Paragraph title = new Paragraph("Lista de Usuarios", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 20;
                document.Add(title);

                // Tabla
                PdfPTable table = new PdfPTable(4);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 3f, 2.5f, 2f, 2f });

                // Encabezados
                Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                PdfPCell headerCell;

                string[] headers = { "Email", "Nombre", "Contraseña", "Rol" };
                foreach (var header in headers)
                {
                    headerCell = new PdfPCell(new Phrase(header, headerFont));
                    headerCell.BackgroundColor = BaseColor.LightGray;
                    headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
                    headerCell.Padding = 5;
                    table.AddCell(headerCell);
                }

                // Datos
                Font dataFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                foreach (var usuario in listaUsuarios)
                {
                    table.AddCell(new PdfPCell(new Phrase(usuario.Email ?? "", dataFont)) { Padding = 5 });
                    table.AddCell(new PdfPCell(new Phrase(usuario.Nombre ?? "", dataFont)) { Padding = 5 });
                    table.AddCell(new PdfPCell(new Phrase(usuario.Pass ?? "", dataFont)) { Padding = 5 });
                    table.AddCell(new PdfPCell(new Phrase(usuario.IdRolNavigation?.Rol1 ?? "", dataFont)) { Padding = 5 });
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

                string fileName = $"Usuarios_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                return File(stream.ToArray(), "application/pdf", fileName);
            }
        }

        // GET: Usuarios/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(m => m.IdUser == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // GET: Usuarios/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["IdRol"] = new SelectList(_context.Rols, "IdRol", "Rol1");
            return View();
        }

        // POST: Usuarios/Create
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdUser,Email,Nombre,Pass,IdRol")] Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                _context.Add(usuario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdRol"] = new SelectList(_context.Rols, "IdRol", "Rol1", usuario.IdRol);
            return View(usuario);
        }

        // GET: Usuarios/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }
            ViewData["IdRol"] = new SelectList(_context.Rols, "IdRol", "Rol1", usuario.IdRol);
            return View(usuario);
        }

        // POST: Usuarios/Edit/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdUser,Email,Nombre,Pass,IdRol")] Usuario usuario)
        {
            if (id != usuario.IdUser)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(usuario);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(usuario.IdUser))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            ViewData["IdRol"] = new SelectList(_context.Rols, "IdRol", "Rol1", usuario.IdRol);
            return View(usuario);
        }

        [Authorize(Roles = "Admin")]
        // GET: Usuarios/Delete/5
        public JsonResult Delete(int id)
        {
            try
            {
                var usuario = _context.Usuarios.Find(id);

                if (usuario == null)
                    return Json("noencontrado");

                bool tieneAtenciones = _context.Atenciones.Any(a => a.IdUser == id);
                if (tieneAtenciones)
                    return Json("relacion");

                _context.Usuarios.Remove(usuario);
                _context.SaveChanges();

                return Json("ok");
            }
            catch (Exception)
            {
                return Json("error");
            }
        }

        public JsonResult VerificarEmail(string email, int? id)
        {
            bool existe = _context.Usuarios.Any(u => u.Email == email && (!id.HasValue || u.IdUser != id.Value));
            return Json(new { existe });
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.IdUser == id);
        }
    }
}