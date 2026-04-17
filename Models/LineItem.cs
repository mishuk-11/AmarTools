using System.ComponentModel.DataAnnotations;

namespace AmarTools.InvoiceGenerator.Models
{
    public class LineItem
    {
        // Description requirement is enforced by FluentValidation for non-draft saves.
        [Display(Name = "Description / Item")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        [Display(Name = "Qty")]
        public decimal Quantity { get; set; } = 1;

        [Required]
        // Allow a unit price of 0 so drafts/placeholders can be saved. Validation message adjusted.
        [Range(0.0, double.MaxValue, ErrorMessage = "Unit price must be 0 or greater")]
        [Display(Name = "Unit Price")]
        public decimal UnitPrice { get; set; } = 0;

        public decimal LineTotal => Quantity * UnitPrice;
    }
}