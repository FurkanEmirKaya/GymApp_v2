using GymApp_v1.Data;

namespace GymApp_v1.ViewModels
{
    public class MembershipFilterViewModel
    {
        public string? SearchTerm { get; set; }
        public string? TypeFilter { get; set; }
        public string? PriceFilter { get; set; }
        public string? SortBy { get; set; }
        public List<Membership> Memberships { get; set; } = new();
        
        // Filter seçenekleri için
        public List<string> AvailableTypes { get; set; } = new();
        public Subscription? CurrentSubscription { get; set; }
    }
}