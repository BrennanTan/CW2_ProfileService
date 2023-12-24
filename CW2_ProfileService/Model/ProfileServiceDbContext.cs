using Microsoft.EntityFrameworkCore;

namespace CW2_ProfileService.Model
{
    public class ProfileServiceDbContext : DbContext
    {

        public ProfileServiceDbContext()
        {
            
        }
        public ProfileServiceDbContext(DbContextOptions<ProfileServiceDbContext> options) : base(options)
        {
        }

        public DbSet<UserProfile> UserProfile { get; set; }
        public DbSet<Trails> Trails { get; set; }
        public DbSet<HikingHistory> HikingHistory { get; set; }
        public DbSet<HikingGroups> HikingGroups { get; set; }
        public DbSet<JoinedHikingGroups> JoinedHikingGroups { get; set; }
        public DbSet<Friends> Friends { get; set; }
        public DbSet<FriendsKey> FriendsKey { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("CW2");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("AppDb");
            optionsBuilder.UseSqlServer(connectionString,
        x => x.MigrationsHistoryTable("__MyMigrationsHistory", "CW2"));
        }
    }
}
