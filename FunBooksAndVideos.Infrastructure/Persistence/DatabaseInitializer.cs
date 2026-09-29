using FunBooksAndVideos.Core.Entities;
using FunBooksAndVideos.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace FunBooksAndVideos.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(FunBooksDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        if (!await dbContext.Memberships.AnyAsync(cancellationToken))
        {
            dbContext.Memberships.AddRange(
                new Membership { Id = (int)MembershipType.BookClub, Name = "Book Club", Price = 10m },
                new Membership { Id = (int)MembershipType.VideoClub, Name = "Video Club", Price = 15m },
                new Membership { Id = (int)MembershipType.Premium, Name = "Premium", Price = 20m });
        }

        if (await dbContext.Customers.AnyAsync(cancellationToken))
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        dbContext.Customers.AddRange(
            new Customer { Name = "Alice" },
            new Customer { Name = "Bob" });

        dbContext.Products.AddRange(
            new Product { Name = "The Girl on the train", Price = 18.50m, ProductType = ProductType.Book, IsPhysical = true },
            new Product { Name = "Comprehensive First Aid Training", Price = 20m, ProductType = ProductType.Video, IsPhysical = false });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
