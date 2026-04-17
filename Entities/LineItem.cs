using System.ComponentModel.DataAnnotations.Schema;
namespace AmarTools.InvoiceGenerator.Entities
{
    public class LineItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid InvoiceId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; } = 0;
        public decimal TaxRate { get; set; } = 0;  // ← Added
        [NotMapped]
        public decimal LineTotal => Quantity * UnitPrice;
        public Invoice Invoice { get; set; } = null!;
    }
}