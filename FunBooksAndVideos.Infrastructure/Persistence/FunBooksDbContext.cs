using FunBooksAndVideos.Core.Abstractions;
using FunBooksAndVideos.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace FunBooksAndVideos.Infrastructure.Persistence;

public class FunBooksDbContext(DbContextOptions<FunBooksDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerMemberships> CustomerMemberships => Set<CustomerMemberships>();
    public DbSet<Membership> Memberships => Set<Membership>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>()
            .Property(x => x.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Membership>()
            .Property(x => x.Id)
            .ValueGeneratedNever();

        modelBuilder.Entity<Membership>()
            .Property(x => x.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<CustomerMemberships>()
            .HasKey(x => new { x.CustomerId, x.MembershipId });

        modelBuilder.Entity<CustomerMemberships>()
            .HasOne(x => x.Customer)
            .WithMany(x => x.CustomerMemberships)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CustomerMemberships>()
            .HasOne(x => x.Membership)
            .WithMany(x => x.CustomerMemberships)
            .HasForeignKey(x => x.MembershipId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PurchaseOrder>()
            .Property(x => x.TotalPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<PurchaseOrder>()
            .Property(x => x.RequestId)
            .HasMaxLength(100);

        modelBuilder.Entity<PurchaseOrder>()
            .HasIndex(x => x.RequestId)
            .IsUnique();

        modelBuilder.Entity<PurchaseOrderItem>()
            .Property(x => x.UnitPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<PurchaseOrderItem>()
            .Property(x => x.TotalPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<PurchaseOrder>()
            .HasMany(x => x.Items)
            .WithOne(x => x.PurchaseOrder)
            .HasForeignKey(x => x.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PurchaseOrderItem>()
            .HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        base.OnModelCreating(modelBuilder);
    }
}
