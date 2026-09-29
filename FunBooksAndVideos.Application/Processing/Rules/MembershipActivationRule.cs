using FunBooksAndVideos.Core.Entities;
using FunBooksAndVideos.Core.Enums;
using FunBooksAndVideos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FunBooksAndVideos.Application.Processing.Rules;

public class MembershipActivationRule(FunBooksDbContext dbContext) : IPurchaseOrderRule
{
    public async Task ApplyAsync(PurchaseOrder order, CancellationToken ct = default)
    {
        var membershipTypes = order.Items
            .Where(x => x.MembershipType != MembershipType.None)
            .Select(x => x.MembershipType!.Value)
            .Distinct()
            .ToList();

        if (membershipTypes.Count == 0)
        {
            return;
        }

        var requestedMembershipIds = membershipTypes
            .Select(x => (int)x)
            .ToList();

        var existingMembershipIds = await dbContext.CustomerMemberships
            .Where(x => x.CustomerId == order.CustomerId && requestedMembershipIds.Contains(x.MembershipId))
            .Select(x => x.MembershipId)
            .ToListAsync(ct);

        var newMembershipIds = requestedMembershipIds.Except(existingMembershipIds);
        foreach (var membershipId in newMembershipIds)
        {
            dbContext.CustomerMemberships.Add(new CustomerMemberships
            {
                CustomerId = order.CustomerId,
                MembershipId = membershipId
            });
        }
    }
}
