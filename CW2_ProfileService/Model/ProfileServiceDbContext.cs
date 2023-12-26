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
