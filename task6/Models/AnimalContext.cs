using Microsoft.EntityFrameworkCore;
using task6.Models;

namespace task6.Data
{
    public class AnimalContext : DbContext
    {
        public AnimalContext(DbContextOptions<AnimalContext> options) : base(options)
        {
        }

        public DbSet<Animal> Animals { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Animal>().ToTable("Animal");
            modelBuilder.Entity<Animal>().HasKey(a => a.IdAnimal);
            base.OnModelCreating(modelBuilder);
        }
    }
}