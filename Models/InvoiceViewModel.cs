using System.ComponentModel.DataAnnotations;

namespace AmarTools.InvoiceGenerator.Models
{
    public class InvoiceViewModel
    {
        // When true, validation rules for finalizing an invoice are relaxed to allow saving drafts.
        public bool IsDraft { get; set; } = false;

        // Use entity CompanyInfo from the Entities namespace but keep the view model's LineItem type
        public AmarTools.InvoiceGenerator.Entities.CompanyInfo From { get; set; } = new AmarTools.InvoiceGenerator.Entities.CompanyInfo();
        public AmarTools.InvoiceGenerator.Entities.CompanyInfo To { get; set; } = new AmarTools.InvoiceGenerator.Entities.CompanyInfo();

        public List<LineItem> Items { get; set; } = new List<LineItem>();

        [Required]
        public string? InvoiceNumber { get; set; }

        public DateTime IssueDate { get; set; } = DateTime.Today;
        public DateTime? DueDate { get; set; }

        public string? Currency { get; set; } = "USD";

        public decimal Subtotal { get; set; }
        public decimal? TaxRate { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal GrandTotal { get; set; }

        public string? Notes { get; set; }
        public string? Terms { get; set; }

        public Guid? Id { get; set; }
    }
}