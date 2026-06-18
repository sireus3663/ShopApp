using Microsoft.EntityFrameworkCore;
using ShopProject.Models;

namespace ShopProject.Db;

public class AppDbContext : DbContext
{
    public DbSet<User> users { get; set; }
    public DbSet<Product> products { get; set; }
    public DbSet<Order> orders { get; set; }
    public DbSet<Discount> discounts { get; set; }
    public DbSet<Cart> carts { get; set; }
    public DbSet<Favorite> favorites { get; set; }
    public DbSet<Session> sessions { get; set; }
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public AppDbContext() : base()
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>();

        modelBuilder.Entity<Product>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(p => p.SellerId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.UserId).IsRequired();
            entity.Property(o => o.ProductId).IsRequired();
            entity.Property(o => o.Count).IsRequired();
            entity.Property(o => o.Price).IsRequired();
            entity.Property(o => o.CreatedAt).IsRequired();

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(o => o.Product)
                .WithMany()
                .HasForeignKey(o => o.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.UserId).IsRequired();
            entity.Property(c => c.ProductId).IsRequired();
            entity.Property(c => c.Count).IsRequired();

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Product>()
                .WithMany()
                .HasForeignKey(c => c.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.Property(f => f.UserId).IsRequired();
            entity.Property(f => f.ProductId).IsRequired();

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Product>()
                .WithMany()
                .HasForeignKey(f => f.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Discount>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.ProductId).IsRequired();
            entity.Property(d => d.Percent).IsRequired();

            entity.HasOne<Product>()
                .WithOne()
                .HasForeignKey<Discount>(d => d.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.UserId).IsRequired();
            entity.Property(s => s.Token).IsRequired();
            entity.Property(s => s.CreatedAt).IsRequired();
            entity.Property(s => s.ExpiresAt).IsRequired();
            entity.Property(s => s.IsActive).IsRequired();

            entity.HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(s => s.Token);
        });
    }
}