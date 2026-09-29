using FunBooksAndVideos.Application.Dto;
using FunBooksAndVideos.Application.Exceptions;
using FunBooksAndVideos.Application.Processing;
using FunBooksAndVideos.Application.Processing.Rules;
using FunBooksAndVideos.Core.Abstractions;
using FunBooksAndVideos.Core.Entities;
using FunBooksAndVideos.Core.Enums;
using FunBooksAndVideos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FunBooksAndVideos.Tests.Processing;

[TestClass]
public class PurchaseOrderProcessorTests
{
    [TestMethod]
    public async Task ProcessAsync_ShouldThrowBusinessException_WhenRequestIdIsMissing()
    {
        var dbContext = CreateDbContext();
        var processor = new PurchaseOrderProcessor(dbContext, []);

        var request = new CreatePurchaseOrderRequestDto
        {
            RequestId = "   ",
            OrderId = 1,
            CustomerId = 1,
            TotalPrice = 10m
        };

        await Assert.ThrowsExactlyAsync<BusinessException>(() => processor.ProcessAsync(request));
    }

    [TestMethod]
    public async Task PhysicalProductShippingRule_ShouldGenerateSlip_WhenOrderHasPhysicalProduct()
    {
        var shippingSlipService = new TrackingShippingSlipService();
        var rule = new PhysicalProductShippingRule(shippingSlipService);
        var order = new PurchaseOrder
        {
            Id = 1,
            CustomerId = 100,
            Items =
            [
                new PurchaseOrderItem
                {
                    Product = new Product { IsPhysical = true },
                    Quantity = 1,
                    UnitPrice = 20m
                }
            ]
        };

        await rule.ApplyAsync(order);

        Assert.AreEqual(1, shippingSlipService.CallCount);
    }

    [TestMethod]
    public async Task PhysicalProductShippingRule_ShouldNotGenerateSlip_WhenOrderHasNoPhysicalProduct()
    {
        var shippingSlipService = new TrackingShippingSlipService();
        var rule = new PhysicalProductShippingRule(shippingSlipService);
        var order = new PurchaseOrder
        {
            Id = 2,
            CustomerId = 200,
            Items =
            [
                new PurchaseOrderItem
                {
                    Product = new Product { IsPhysical = false },
                    Quantity = 1,
                    UnitPrice = 20m
                },
                new PurchaseOrderItem
                {
                    MembershipType = MembershipType.BookClub,
                    Quantity = 1,
                    UnitPrice = 10m
                }
            ]
        };

        await rule.ApplyAsync(order);

        Assert.AreEqual(0, shippingSlipService.CallCount);
    }
    private static FunBooksDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<FunBooksDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new FunBooksDbContext(options);
    }

    private sealed class TrackingShippingSlipService : IShippingSlipService
    {
        public int CallCount { get; private set; }

        public Task GenerateForOrderAsync(PurchaseOrder order, CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.CompletedTask;
        }
    }
}

[TestClass]
public class MembershipActivationRuleTests
{
    [TestMethod]
    public async Task ApplyAsync_ShouldAddMembershipLinks_ForRequestedMemberships()
    {
        await using var dbContext = CreateMembershipDbContext();
        var rule = new MembershipActivationRule(dbContext);
        var order = new PurchaseOrder
        {
            CustomerId = 77,
            Items =
            [
                new PurchaseOrderItem { MembershipType = MembershipType.BookClub, Quantity = 1, UnitPrice = 10m },
                new PurchaseOrderItem { MembershipType = MembershipType.Premium, Quantity = 1, UnitPrice = 20m }
            ]
        };

        await rule.ApplyAsync(order);

        var addedLinks = dbContext.ChangeTracker
            .Entries<CustomerMemberships>()
            .Count(x => x.State == EntityState.Added);

        Assert.AreEqual(2, addedLinks);
    }

    [TestMethod]
    public async Task ApplyAsync_ShouldNotDuplicateExistingMembershipLink()
    {
        await using var dbContext = CreateMembershipDbContext();
        dbContext.CustomerMemberships.Add(new CustomerMemberships
        {
            CustomerId = 88,
            MembershipId = (int)MembershipType.BookClub
        });
        await dbContext.SaveChangesAsync();

        var rule = new MembershipActivationRule(dbContext);
        var order = new PurchaseOrder
        {
            CustomerId = 88,
            Items =
            [
                new PurchaseOrderItem { MembershipType = MembershipType.BookClub, Quantity = 1, UnitPrice = 10m },
                new PurchaseOrderItem { MembershipType = MembershipType.VideoClub, Quantity = 1, UnitPrice = 15m }
            ]
        };

        await rule.ApplyAsync(order);

        var addedLinks = dbContext.ChangeTracker
            .Entries<CustomerMemberships>()
            .Count(x => x.State == EntityState.Added);

        Assert.AreEqual(1, addedLinks);
        Assert.AreEqual((int)MembershipType.VideoClub, dbContext.ChangeTracker.Entries<CustomerMemberships>()
            .Single(x => x.State == EntityState.Added)
            .Entity.MembershipId);
    }

    private static FunBooksDbContext CreateMembershipDbContext()
    {
        var options = new DbContextOptionsBuilder<FunBooksDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new MembershipRuleDbContext(options);
    }

    private sealed class MembershipRuleDbContext(DbContextOptions<FunBooksDbContext> options) : FunBooksDbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CustomerMemberships>()
                .HasKey(x => new { x.CustomerId, x.MembershipId });
        }
    }
}
