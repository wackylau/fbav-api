using FunBooksAndVideos.Application.Dto;

namespace FunBooksAndVideos.Application.Processing;

public interface IPurchaseOrderProcessor
{
    Task<PurchaseOrderResponseDto> ProcessAsync(CreatePurchaseOrderRequestDto request, CancellationToken ct = default);
}
