using FunBooksAndVideos.Core.Entities;
using FunBooksAndVideos.Core.Enums;
using FunBooksAndVideos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FunBooksAndVideos.Application.Processing.Rules;

public class MembershipActivationRule(
    FunBooksDbContext dbContext,
    ILogger<MembershipActivationRule> logger) : IPurchaseOrderRule
{
    public async Task ApplyAsync(PurchaseOrder order, CancellationToken ct = default)
    {
        logger.LogDebug("Evaluating membership activation rule for order '{OrderId}'.", order.Id);

        var membershipTypes = order.Items
            .Where(x => x.MembershipType != MembershipType.None)
            .Select(x => x.MembershipType!.Value)
            .Distinct()
            .ToList();

        if (membershipTypes.Count == 0)
        {
            logger.LogDebug("No membership products found for order '{OrderId}'.", order.Id);
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
        var addedCount = 0;

        foreach (var membershipId in newMembershipIds)
        {
            dbContext.CustomerMemberships.Add(new CustomerMemberships
            {
                CustomerId = order.CustomerId,
                MembershipId = membershipId
            });

            addedCount++;
        }

        logger.LogInformation("Membership activation rule added {Count} memberships for customer '{CustomerId}' from order '{OrderId}'.", addedCount, order.CustomerId, order.Id);
    }
}
