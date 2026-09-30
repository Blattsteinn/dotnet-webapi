using Microsoft.EntityFrameworkCore;

using FoodOrderingApi.Models.Orders;
using FoodOrderingApi.Models.Places;
using FoodOrderingApi.Models.Users;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace FoodOrderingApi.Data;
public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
        
    }

    public DbSet<Place> Places => Set<Place>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<AddOnCategory> AddOnCategories => Set<AddOnCategory>();
    public DbSet<AddOn> AddOns => Set<AddOn>();
    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderItemAddOn> OrderItemAddOns => Set<OrderItemAddOn>();
    public DbSet<PlaceOwnership> PlaceOwnerships => Set<PlaceOwnership>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<OrderItem>()
            .HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<OrderItemAddOn>()
            .HasOne(x => x.AddOn)
            .WithMany()
            .HasForeignKey(x => x.AddOnId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Order>()
            .HasOne(x => x.Place)
            .WithMany()
            .HasForeignKey(x => x.PlaceId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<PlaceOwnership>(ownership =>
        {
            // a place has exactly one owner.
            // an owner may hold many places.
            ownership.HasIndex(x => x.PlaceId).IsUnique();

            ownership
                .HasOne(x => x.Place)
                .WithOne(p => p.Ownership)
                .HasForeignKey<PlaceOwnership>(x => x.PlaceId)
                .OnDelete(DeleteBehavior.Cascade);

            ownership
                .HasOne(x => x.Owner)
                .WithMany()
                .HasForeignKey(x => x.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    
}