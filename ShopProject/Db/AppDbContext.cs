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
        options.UseNpgsql(
            "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=123456"
        );
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>();
    }
}