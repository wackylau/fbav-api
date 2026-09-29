using FunBooksAndVideos.Core.Entities;

namespace FunBooksAndVideos.Application.Processing.Rules;

public interface IPurchaseOrderRule
{
    Task ApplyAsync(PurchaseOrder order, CancellationToken ct = default);
}
