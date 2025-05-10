using Microsoft.EntityFrameworkCore;

namespace GymApp_v1.Data 
{
    public class DataContext: DbContext 
    {
        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
        }   
        
        public DbSet<Membership> Memberships { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
    }
}