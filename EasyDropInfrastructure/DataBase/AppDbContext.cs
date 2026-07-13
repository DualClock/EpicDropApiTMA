using EasyDropDomain.Models;
using Microsoft.EntityFrameworkCore;


namespace EasyDropInfrastructure.DataBase
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<GameEpic> GameEpics { get; set; }

        public DbSet<GameSteam> GameSteams { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<GameSteam>(entity =>
            {
                entity.HasKey(e => e.Id);

                
                entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).HasMaxLength(2000);

               
                entity.HasIndex(e => e.IsActive);
            });

            modelBuilder.Entity<GameSteam>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
            });

            // Конфигурация GameEpic
            modelBuilder.Entity<GameEpic>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).HasMaxLength(2000);
            });
        }
    }
}
