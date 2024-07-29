using Bend_PSA.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bend_PSA.Context
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<Data> Data { get; set; }
        public DbSet<Error> Error { get; set; }
        public DbSet<Image> Image { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Error>().HasIndex(x => x.DataId);
            modelBuilder.Entity<Image>().HasIndex(x => x.DataId);
            modelBuilder.Entity<Data>().HasIndex(x => x.TimeLine);

            modelBuilder.Entity<Error>()
                .HasOne(child => child.Data)
                .WithMany(parent => parent.Errors)
                .HasForeignKey(child => child.DataId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Image>()
                .HasOne(child => child.Data)
                .WithMany(parent => parent.Images)
                .HasForeignKey(child => child.DataId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
