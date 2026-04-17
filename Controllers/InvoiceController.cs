using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using System;
using System.Linq;
using System.Threading.Tasks;

using AmarTools.InvoiceGenerator.Data;
using AmarTools.InvoiceGenerator.Entities;
using AmarTools.InvoiceGenerator.Models;
using AmarTools.InvoiceGenerator.Services;

namespace AmarTools.InvoiceGenerator.Controllers
{
    public class InvoiceController(
        ApplicationDbContext context,
        IMapper mapper,
        IPdfService pdfService,
        ILogger<InvoiceController> logger) : Controller
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IMapper _mapper = mapper;
        private readonly IPdfService _pdfService = pdfService;
        private readonly ILogger<InvoiceController> _logger = logger;

        [HttpGet]
        public IActionResult Generate()
        {
            var model = new InvoiceViewModel
            {
                InvoiceNumber = $"INV-{DateTime.Now:yyyy}-{new Random().Next(10000, 99999)}",
                IssueDate = DateTime.Today,
                DueDate = DateTime.Today.AddDays(30),
                Currency = "USD",
                Items = new List<AmarTools.InvoiceGenerator.Models.LineItem>
                {
                    new AmarTools.InvoiceGenerator.Models.LineItem
                    {
                        Description = "Consulting Service",
                        Quantity = 1,
                        UnitPrice = 0
                    }
                }
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult AddItem(InvoiceViewModel model)
        {
            model.Items ??= new List<AmarTools.InvoiceGenerator.Models.LineItem>();
            model.Items.Add(new AmarTools.InvoiceGenerator.Models.LineItem { Description = string.Empty, Quantity = 1, UnitPrice = 0 });
            return PartialView("_LineItemsTable", model);
        }

        [HttpPost]
        public IActionResult RemoveItem(InvoiceViewModel model, int index)
        {
            if (model.Items != null && index >= 0 && index < model.Items.Count)
            {
                model.Items.RemoveAt(index);
            }
            return PartialView("_LineItemsTable", model);
        }

        [HttpPost]
        public IActionResult UpdatePreview(InvoiceViewModel model)
        {
            model.Subtotal = model.Items?.Sum(i => i.Quantity * i.UnitPrice) ?? 0;
            model.TaxAmount = model.Subtotal * (model.TaxRate ?? 0) / 100;
            model.GrandTotal = model.Subtotal + model.TaxAmount - (model.DiscountAmount ?? 0);

            return PartialView("_LivePreview", model);
        }

        [HttpPost]
        public async Task<IActionResult> GeneratePdf(InvoiceViewModel model)
        {
            model.Subtotal = model.Items?.Sum(i => i.Quantity * i.UnitPrice) ?? 0;
            model.TaxAmount = model.Subtotal * (model.TaxRate ?? 0) / 100;
            model.GrandTotal = model.Subtotal + model.TaxAmount - (model.DiscountAmount ?? 0);

            try
            {
                var pdfBytes = await _pdfService.GeneratePdfAsync(model);
                return File(pdfBytes, "application/pdf", $"{model.InvoiceNumber ?? "invoice"}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PDF generation failed");
                return BadRequest($"Error generating PDF: {ex.Message}");
            }
        }

        // ====================== FIXED SAVE (Create + Update) ======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(InvoiceViewModel model)
        {
            // Always recalculate totals
            model.Subtotal = model.Items?.Sum(i => i.Quantity * i.UnitPrice) ?? 0;
            model.TaxAmount = model.Subtotal * (model.TaxRate ?? 0) / 100;
            model.GrandTotal = model.Subtotal + model.TaxAmount - (model.DiscountAmount ?? 0);

            if (!ModelState.IsValid)
            {
                var errors = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                _logger.LogWarning("Validation failed: {Errors}", errors);
                TempData["Error"] = $"Please fix the following: {errors}";
                return View("Generate", model);
            }

            try
            {
                if (model.Id.HasValue && model.Id.Value != Guid.Empty)
                {
                    // UPDATE existing invoice
                    var existing = await _context.Invoices
                        .Include(i => i.FromCompany)
                        .Include(i => i.ToCompany)
                        .Include(i => i.Items)
                        .FirstOrDefaultAsync(i => i.Id == model.Id.Value);

                    if (existing != null)
                    {
                        _mapper.Map(model, existing);

                        existing.Subtotal = model.Subtotal;
                        existing.TaxAmount = model.TaxAmount;
                        existing.GrandTotal = model.GrandTotal;
                        existing.DiscountAmount = model.DiscountAmount ?? 0;
                        existing.UpdatedAt = DateTime.UtcNow;
                        existing.TaxRate = model.TaxRate;   // Important after Bug 4

                        await _context.SaveChangesAsync();

                        if (model.IsDraft)
                        {
                            // Inform the user this was saved as a draft and log server-side
                            TempData["Info"] = $"Draft saved: Invoice {existing.InvoiceNumber}. Some fields may be incomplete.";
                            _logger.LogInformation("Draft saved for existing invoice {InvoiceNumber} ({InvoiceId}) by {User}", existing.InvoiceNumber, existing.Id, User?.Identity?.Name ?? "anonymous");
                        }
                        else
                        {
                            TempData["Success"] = $"Invoice {existing.InvoiceNumber} updated successfully!";
                        }

                        return RedirectToAction(nameof(History));
                    }
                }

                // CREATE new invoice
                var invoiceEntity = _mapper.Map<Invoice>(model);
                invoiceEntity.Subtotal = model.Subtotal;
                invoiceEntity.TaxAmount = model.TaxAmount;
                invoiceEntity.GrandTotal = model.GrandTotal;
                invoiceEntity.DiscountAmount = model.DiscountAmount ?? 0;
                invoiceEntity.CreatedAt = DateTime.UtcNow;
                invoiceEntity.TaxRate = model.TaxRate;

                _context.Invoices.Add(invoiceEntity);
                await _context.SaveChangesAsync();

                if (model.IsDraft)
                {
                    TempData["Info"] = $"Draft saved: Invoice {invoiceEntity.InvoiceNumber}. Some fields may be incomplete.";
                    _logger.LogInformation("Draft saved for new invoice {InvoiceNumber} ({InvoiceId}) by {User}", invoiceEntity.InvoiceNumber, invoiceEntity.Id, User?.Identity?.Name ?? "anonymous");
                }
                else
                {
                    TempData["Success"] = $"Invoice {invoiceEntity.InvoiceNumber} saved successfully!";
                }

                return RedirectToAction(nameof(History));
            }
            catch (DbUpdateException dbEx)
            {
                var innerMsg = dbEx.InnerException?.Message ?? dbEx.Message;
                _logger.LogError(dbEx, "DbUpdateException while saving invoice. Inner: {Inner}", innerMsg);
                TempData["Error"] = $"Database error: {innerMsg}";
                return View("Generate", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error saving invoice");
                TempData["Error"] = $"Error saving invoice: {ex.Message}";
                return View("Generate", model);
            }
        }

        // Rest of the controller (History, Load, Delete) remains the same...
        [HttpGet]
        public async Task<IActionResult> History()
        {
            var invoices = await _context.Invoices
                .AsNoTracking()
                .Include(i => i.FromCompany)
                .Include(i => i.ToCompany)
                .Include(i => i.Items)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            return View(invoices);
        }

        [HttpGet]
        public async Task<IActionResult> Load(Guid id)
        {
            var invoiceEntity = await _context.Invoices
                .AsNoTracking()
                .Include(i => i.FromCompany)
                .Include(i => i.ToCompany)
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoiceEntity == null)
            {
                TempData["Error"] = "Invoice not found.";
                return RedirectToAction(nameof(History));
            }

            var model = _mapper.Map<InvoiceViewModel>(invoiceEntity);
            return View("Generate", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice != null)
            {
                _context.Invoices.Remove(invoice);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Invoice deleted successfully.";
            }
            return RedirectToAction(nameof(History));
        }
    }
}