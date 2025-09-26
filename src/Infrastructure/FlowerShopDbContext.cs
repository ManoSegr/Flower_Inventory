using FlowerShop.Domain;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.Infrastructure
{
    public class FlowerShopDbContext : DbContext
    {
        public FlowerShopDbContext(DbContextOptions<FlowerShopDbContext> options) : base(options) { }

        public DbSet<Flower> Flowers => Set<Flower>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<FlowerSearchResult> FlowerSearchResults => Set<FlowerSearchResult>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            b.HasDefaultSchema("inv");

            b.Entity<Flower>(e =>
            {
                e.ToTable("Flower");
                e.HasKey(f => f.FlowerId);
                e.Property(f => f.Name).HasMaxLength(150).IsRequired();
                e.Property(f => f.Type).HasMaxLength(50).IsRequired();
                e.Property(f => f.SKU).HasMaxLength(50);
                e.Property(f => f.Price).HasColumnType("decimal(10,2)");
                e.Property(f => f.StemLengthCm).HasColumnType("decimal(5,2)");
                e.Property(f => f.CreatedAt).HasColumnType("datetime2(0)");
                e.Property(f => f.UpdatedAt).HasColumnType("datetime2(0)");
                e.Property(f => f.RowVersion).IsRowVersion();

                e.HasOne(f => f.Category)
                  .WithMany(c => c.Flowers)
                  .HasForeignKey(f => f.CategoryId)
                  .OnDelete(DeleteBehavior.NoAction);
            });

            b.Entity<Category>(e =>
            {
                e.ToTable("Category");
                e.HasKey(c => c.CategoryId);
                e.Property(c => c.Name).HasMaxLength(100).IsRequired();
                e.Property(c => c.RowVersion).IsRowVersion();
                e.Property(c => c.CreatedAt).HasColumnType("datetime2(0)");
            });

            b.Entity<FlowerSearchResult>(e =>
            {
                e.HasNoKey();
                e.ToView(null);
            });
        }
    }
}
