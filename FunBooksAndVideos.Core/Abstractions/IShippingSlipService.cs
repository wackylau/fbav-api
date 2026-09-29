using FunBooksAndVideos.Core.Entities;

namespace FunBooksAndVideos.Core.Abstractions;

public interface IShippingSlipService
{
    Task GenerateForOrderAsync(PurchaseOrder order, CancellationToken cancellationToken = default);
}
