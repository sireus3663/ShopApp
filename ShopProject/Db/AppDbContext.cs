using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ShopProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;


namespace ShopProject.Db;
    public class AppDbContext : DbContext
{
    public DbSet<User> users { get; set; }
    public DbSet<Product> products { get; set; }
    public DbSet<Order> orders { get; set; }
    public DbSet<Discount> discounts { get; set; }
    public DbSet<Cart> carts { get; set; }
    public DbSet<Favorite> favorites { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        string jsonString = File.ReadAllText("AppConfig.json");
        JsonNode root = JsonNode.Parse(jsonString);
        options.UseNpgsql(
            root["ConnectionStrings"]?.ToString()
        );
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>();

        modelBuilder.Entity<Order>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Order>()
            .HasOne<Product>()
            .WithMany()
            .HasForeignKey(o => o.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Cart>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Cart>()
            .HasOne<Product>()
            .WithMany()
            .HasForeignKey(c => c.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Favorite>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Favorite>()
            .HasOne<Product>()
            .WithMany()
            .HasForeignKey(f => f.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Discount>()
            .HasOne<Product>()
            .WithOne()
            .HasForeignKey<Discount>(d => d.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Product>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(p => p.SellerId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
