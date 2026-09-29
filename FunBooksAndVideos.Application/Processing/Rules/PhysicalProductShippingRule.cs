using FunBooksAndVideos.Core.Abstractions;
using FunBooksAndVideos.Core.Entities;

namespace FunBooksAndVideos.Application.Processing.Rules;

public class PhysicalProductShippingRule(IShippingSlipService shippingSlipService) : IPurchaseOrderRule
{
    public async Task ApplyAsync(PurchaseOrder order, CancellationToken ct = default)
    {
        var hasPhysicalProduct = order.Items.Any(x => x.Product?.IsPhysical == true);

        if (!hasPhysicalProduct)
        {
            return;
        }

        await shippingSlipService.GenerateForOrderAsync(order, ct);
    }
}
