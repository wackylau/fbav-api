using System.Collections.Concurrent;
using FunBooksAndVideos.Application.Dto;
using FunBooksAndVideos.Application.Exceptions;
using FunBooksAndVideos.Application.Processing.Rules;
using FunBooksAndVideos.Core.Entities;
using FunBooksAndVideos.Core.Enums;
using FunBooksAndVideos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FunBooksAndVideos.Application.Processing;

public class PurchaseOrderProcessor(
    FunBooksDbContext dbContext,
    IEnumerable<IPurchaseOrderRule> rules) : IPurchaseOrderProcessor
{
    private static readonly ConcurrentDictionary<string, byte> InProgressRequestIds = new(StringComparer.Ordinal);

    public async Task<PurchaseOrderResponseDto> ProcessAsync(CreatePurchaseOrderRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.RequestId))
        {
            throw new BusinessException("RequestId is required.");
        }

        var normalizedRequestId = request.RequestId.Trim();

        if (!InProgressRequestIds.TryAdd(normalizedRequestId, 0))
        {
            throw new BusinessException("A request is already processing.");
        }

        try
        {
            if (request.OrderId <= 0)
            {
                throw new BusinessException("OrderId must be greater than zero.");
            }

            var purchaseOrder = await dbContext.PurchaseOrders
                .Include(x => x.Items)
                    .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x => x.Id == request.OrderId && x.CustomerId == request.CustomerId, ct);

            if (purchaseOrder == null)
            {
                throw new BusinessException($"Purchase order '{request.OrderId}' was not found for customer '{request.CustomerId}'.");
            }
            this.ValidateProcessOrder(purchaseOrder, request);

            foreach (var rule in rules)
            {
                if (!ct.IsCancellationRequested)
                {
                    await rule.ApplyAsync(purchaseOrder, ct);
                }
            }
            await dbContext.SaveChangesAsync(ct);

            return new PurchaseOrderResponseDto
            {
                PurchaseOrderId = purchaseOrder.Id,
                TotalPrice = purchaseOrder.TotalPrice,
            };
        }
        finally
        {
            InProgressRequestIds.TryRemove(normalizedRequestId, out _);
        }
    }

    private void ValidateProcessOrder(PurchaseOrder purchaseOrder, CreatePurchaseOrderRequestDto request)
    {
        if (request.CustomerId > 0 && purchaseOrder.CustomerId != request.CustomerId)
        {
            throw new BusinessException("CustomerId does not match the purchase order customer.");
        }
        if (purchaseOrder.Status != PurchaseOrderStatus.Paid)
        {
            throw new BusinessException($"Purchase order '{request.OrderId}' is not paid.");
        }
        if (purchaseOrder.TotalPrice != request.TotalPrice)
        {
            throw new BusinessException("TotalPrice does not match the purchase order total.");
        }
    }
}
