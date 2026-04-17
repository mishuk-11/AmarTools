namespace AmarTools.InvoiceGenerator.Entities
{
    public class CompanyInfo
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        // Removed: public Invoice? Invoice { get; set; }   ← No longer needed
    }
}