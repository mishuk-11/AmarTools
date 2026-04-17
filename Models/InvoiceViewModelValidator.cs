using FluentValidation;

namespace AmarTools.InvoiceGenerator.Models
{
    public class InvoiceViewModelValidator : AbstractValidator<InvoiceViewModel>
    {
        public InvoiceViewModelValidator()
        {
            // Only enforce these rules when not saving as a draft
            RuleFor(x => x.InvoiceNumber)
                .NotEmpty().WithMessage("Invoice number is required")
                .When(x => !x.IsDraft);

            RuleFor(x => x.From.Name)
                .NotEmpty().WithMessage("Sender name is required")
                .When(x => !x.IsDraft);

            RuleFor(x => x.To.Name)
                .NotEmpty().WithMessage("Client name is required")
                .When(x => !x.IsDraft);

            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("At least one item is required")
                .When(x => !x.IsDraft)
                .Must(items => items.All(i => !string.IsNullOrWhiteSpace(i.Description)))
                .WithMessage("All items must have a description")
                .When(x => !x.IsDraft);
        }
    }
}