using System.ComponentModel.DataAnnotations;

namespace AmrTools.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? GoogleId { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsEmailVerified { get; set; }
        public string SubscriptionPlan { get; set; } = "Free";
        public bool IsAdmin { get; set; } = false;
        public string Role { get; set; } = "User";

    }
}