namespace RPG.Data
{
    using Models;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;

    public class DataContext : DbContext
    {
        public DataContext()
        {

        }

        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {

        }

        public DbSet<Player> Players { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Player>()
                .Property(p => p.CreatedOn)
                .HasDefaultValueSql("GETDATE()");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            IConfiguration config = new ConfigurationBuilder()
               .AddUserSecrets<Program>()
               .Build();

            string connectionString = config["ConnectionStrings:DefaultConnection"];

            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}
