using FunBooksAndVideos.Core.Abstractions;
using FunBooksAndVideos.Core.Entities;
using Microsoft.Extensions.Logging;

namespace FunBooksAndVideos.Application.Processing.Rules;

public class PhysicalProductShippingRule(
    IShippingSlipService shippingSlipService,
    ILogger<PhysicalProductShippingRule> logger) : IPurchaseOrderRule
{
    public async Task ApplyAsync(PurchaseOrder order, CancellationToken ct = default)
    {
        logger.LogDebug("Evaluating physical product shipping rule for order '{OrderId}'.", order.Id);

        var hasPhysicalProduct = order.Items.Any(x => x.Product?.IsPhysical == true);

        if (!hasPhysicalProduct)
        {
            logger.LogDebug("No physical products found for order '{OrderId}'. Shipping slip generation skipped.", order.Id);
            return;
        }

        logger.LogInformation("Generating shipping slip for order '{OrderId}'.", order.Id);
        await shippingSlipService.GenerateForOrderAsync(order, ct);
    }
}
