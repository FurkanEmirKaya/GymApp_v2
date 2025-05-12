using GymApp_v1.Data;

namespace GymApp_v1.ViewModels
{
    public class UserFilterViewModel
    {
        public string? SearchTerm { get; set; }
        public string? RoleFilter { get; set; }
        public string? MembershipFilter { get; set; }
        public string? SortBy { get; set; }
        public List<User> Users { get; set; } = new();
        
        // Filter seçenekleri için
        public List<string> AvailableRoles { get; set; } = new();
        public List<Membership> AvailableMemberships { get; set; } = new();
    }
}