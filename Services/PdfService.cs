using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using AmarTools.InvoiceGenerator.Models;
using System.Globalization;

namespace AmarTools.InvoiceGenerator.Services
{
    public class PdfService : IPdfService
    {
        public Task<byte[]> GeneratePdfAsync(InvoiceViewModel model)
        {
            // Ensure at least one item to avoid empty table
            if (model.Items == null || model.Items.Count == 0)
            {
                model.Items = new List<LineItem>
                {
                    new LineItem
                    {
                        Description = "Sample Item",
                        Quantity = 1,
                        UnitPrice = 0
                    }
                };
            }

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(50);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    // Header
                    page.Header()
                        .Height(80)
                        .AlignCenter()
                        .Text("INVOICE")
                        .FontSize(28)
                        .Bold()
                        .FontColor(Colors.Blue.Darken2);

                    // Content
                    page.Content()
                        .Column(column =>
                        {
                            // Bill From & Bill To
                            column.Item().Row(row =>
                            {
                                // Bill From (Left)
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("Bill From").Bold().FontSize(12);
                                    c.Item().Text(model.From.Name).FontSize(11);
                                    c.Item().Text(model.From.Address).FontSize(10);
                                    if (!string.IsNullOrWhiteSpace(model.From.Email))
                                        c.Item().Text(model.From.Email).FontSize(10);
                                    if (!string.IsNullOrWhiteSpace(model.From.Phone))
                                        c.Item().Text(model.From.Phone).FontSize(10);
                                });

                                row.ConstantItem(50);

                                // Bill To (Right)
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().AlignRight().Text("Bill To").Bold().FontSize(12);
                                    c.Item().AlignRight().Text(model.To.Name).FontSize(11);
                                    c.Item().AlignRight().Text(model.To.Address).FontSize(10);
                                    if (!string.IsNullOrWhiteSpace(model.To.Email))
                                        c.Item().AlignRight().Text(model.To.Email).FontSize(10);
                                    if (!string.IsNullOrWhiteSpace(model.To.Phone))
                                        c.Item().AlignRight().Text(model.To.Phone).FontSize(10);
                                });
                            });

                            column.Item().PaddingTop(20).LineHorizontal(1);

                            // Invoice Details
                            column.Item().PaddingTop(15).Row(row =>
                            {
                                row.RelativeItem().Text($"Invoice Number: {model.InvoiceNumber ?? "N/A"}");
                                row.RelativeItem().AlignRight().Text($"Issue Date: {model.IssueDate:dd MMMM yyyy}");
                            });

                            if (model.DueDate.HasValue)
                            {
                                column.Item().PaddingTop(5).Row(row =>
                                {
                                    row.RelativeItem().Text($"Due Date: {model.DueDate.Value:dd MMMM yyyy}");
                                });
                            }

                            // Line Items Table
                            column.Item().PaddingTop(25).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(5);   // Description
                                    columns.RelativeColumn(2);   // Quantity
                                    columns.RelativeColumn(2);   // Unit Price
                                    columns.RelativeColumn(3);   // Amount
                                });

                                // Table Header
                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(8).Text("Description").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(8).AlignRight().Text("Qty").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(8).AlignRight().Text("Unit Price").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(8).AlignRight().Text("Amount").Bold();
                                });

                                // Table Rows
                                foreach (var item in model.Items)
                                {
                                    table.Cell().Padding(8).Text(item.Description ?? "");
                                    table.Cell().Padding(8).AlignRight().Text(item.Quantity.ToString("F2"));
                                    table.Cell().Padding(8).AlignRight().Text(item.UnitPrice.ToString("C", new CultureInfo("en-US")));
                                    table.Cell().Padding(8).AlignRight().Text(item.LineTotal.ToString("C", new CultureInfo("en-US")));
                                }
                            });

                            // Totals Section
                            column.Item().PaddingTop(20).AlignRight().Column(col =>
                            {
                                col.Item().Text($"Subtotal: {model.Subtotal:C}").FontSize(11);

                                if (model.TaxRate.GetValueOrDefault() > 0)
                                {
                                    col.Item().Text($"Tax ({model.TaxRate}%) : {model.TaxAmount:C}").FontSize(11);
                                }

                                if (model.DiscountAmount.GetValueOrDefault() > 0)
                                {
                                    col.Item().Text($"Discount : -{model.DiscountAmount:C}").FontSize(11);
                                }

                                col.Item().PaddingTop(8)
                                    .Text($"Grand Total: {model.GrandTotal:C}")
                                    .FontSize(14)
                                    .Bold()
                                    .FontColor(Colors.Blue.Darken2);
                            });

                            // Notes
                            if (!string.IsNullOrWhiteSpace(model.Notes))
                            {
                                column.Item().PaddingTop(30).Text("Notes:").Bold().FontSize(11);
                                column.Item().Text(model.Notes);
                            }

                            // Terms & Conditions
                            if (!string.IsNullOrWhiteSpace(model.Terms))
                            {
                                column.Item().PaddingTop(20).Text("Terms & Conditions:").Bold().FontSize(11);
                                column.Item().Text(model.Terms);
                            }
                        });

                    // Footer
                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Generated by AmarTools | Page ");
                            x.CurrentPageNumber();
                            x.Span(" of ");
                            x.TotalPages();
                        });
                });
            });

            return Task.FromResult(document.GeneratePdf());
        }
    }
}