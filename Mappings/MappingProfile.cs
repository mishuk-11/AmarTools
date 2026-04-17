using AutoMapper;
using AmarTools.InvoiceGenerator.Models;
using AmarTools.InvoiceGenerator.Entities;

namespace AmarTools.InvoiceGenerator.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // 1. ViewModel LineItem → Entity LineItem (Save)
            CreateMap<Models.LineItem, Entities.LineItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.InvoiceId, opt => opt.Ignore())
                .ForMember(dest => dest.Invoice, opt => opt.Ignore());

            // 2. Entity LineItem → ViewModel LineItem (Load)
            CreateMap<Entities.LineItem, Models.LineItem>();

            // 3. ViewModel → Entity Invoice (Save - Create or Update)
            CreateMap<InvoiceViewModel, Invoice>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())

                .ForMember(dest => dest.FromCompany, opt => opt.MapFrom(src => src.From))
                .ForMember(dest => dest.ToCompany, opt => opt.MapFrom(src => src.To))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))

                .AfterMap((src, dest) =>
                {
                    // Set foreign keys
                    if (dest.FromCompany != null)
                        dest.FromCompanyId = dest.FromCompany.Id;

                    if (dest.ToCompany != null)
                        dest.ToCompanyId = dest.ToCompany.Id;

                    // Link line items to invoice
                    foreach (var item in dest.Items)
                    {
                        item.InvoiceId = dest.Id;
                        item.Invoice = dest;
                    }

                    // Ensure discount is not null
                    dest.DiscountAmount = src.DiscountAmount ?? 0;

                    // Map TaxRate (new)
                    dest.TaxRate = src.TaxRate;
                });

            // 4. Entity Invoice → ViewModel Invoice (Load)
            CreateMap<Invoice, InvoiceViewModel>()
                .ForMember(dest => dest.From, opt => opt.MapFrom(src => src.FromCompany))
                .ForMember(dest => dest.To, opt => opt.MapFrom(src => src.ToCompany))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
                .ForMember(dest => dest.DiscountAmount, opt => opt.MapFrom(src => src.DiscountAmount))
                .ForMember(dest => dest.TaxRate, opt => opt.MapFrom(src => src.TaxRate));  // ← Fixed: Now maps TaxRate
        }
    }
}