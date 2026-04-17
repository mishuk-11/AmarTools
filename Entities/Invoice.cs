namespace AmarTools.InvoiceGenerator.Entities
{
    public class Invoice
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string InvoiceNumber { get; set; } = string.Empty;

        public DateTime IssueDate { get; set; }

        public DateTime? DueDate { get; set; }

        public string Currency { get; set; } = "USD";

        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal? TaxRate { get; set; }     // ← Added
        public decimal DiscountAmount { get; set; }
        public decimal GrandTotal { get; set; }

        public string Notes { get; set; } = string.Empty;
        public string Terms { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; } = false;

        public Guid FromCompanyId { get; set; }
        public Guid ToCompanyId { get; set; }

        public CompanyInfo FromCompany { get; set; } = null!;
        public CompanyInfo ToCompany { get; set; } = null!;
        public List<LineItem> Items { get; set; } = new List<LineItem>();
    }
}