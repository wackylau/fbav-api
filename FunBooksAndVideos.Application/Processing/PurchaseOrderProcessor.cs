using System.Collections.Concurrent;
using FunBooksAndVideos.Application.Dto;
using FunBooksAndVideos.Application.Exceptions;
using FunBooksAndVideos.Application.Processing.Rules;
using FunBooksAndVideos.Core.Entities;
using FunBooksAndVideos.Core.Enums;
using FunBooksAndVideos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FunBooksAndVideos.Application.Processing;

public class PurchaseOrderProcessor(
    FunBooksDbContext dbContext,
    IEnumerable<IPurchaseOrderRule> rules,
    ILogger<PurchaseOrderProcessor> logger) : IPurchaseOrderProcessor
{
    private static readonly ConcurrentDictionary<string, byte> InProgressRequestIds = new(StringComparer.Ordinal);

    public async Task<PurchaseOrderResponseDto> ProcessAsync(CreatePurchaseOrderRequestDto request, CancellationToken ct = default)
    {
        logger.LogInformation("Starting purchase order processing for RequestId '{RequestId}', OrderId '{OrderId}', CustomerId '{CustomerId}'.", request.RequestId, request.OrderId, request.CustomerId);

        if (string.IsNullOrWhiteSpace(request.RequestId))
        {
            logger.LogWarning("Purchase order processing rejected because RequestId is empty.");
            throw new BusinessException("RequestId is required.");
        }

        var normalizedRequestId = request.RequestId.Trim();

        if (!InProgressRequestIds.TryAdd(normalizedRequestId, 0))
        {
            logger.LogWarning("Purchase order processing rejected because RequestId '{RequestId}' is already in progress.", normalizedRequestId);
            throw new BusinessException("A request is already processing.");
        }

        try
        {
            if (request.OrderId <= 0)
            {
                logger.LogWarning("Purchase order processing rejected for RequestId '{RequestId}' because OrderId '{OrderId}' is invalid.", normalizedRequestId, request.OrderId);
                throw new BusinessException("OrderId must be greater than zero.");
            }

            var purchaseOrder = await dbContext.PurchaseOrders
                .Include(x => x.Items)
                    .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x => x.Id == request.OrderId && x.CustomerId == request.CustomerId, ct);

            if (purchaseOrder == null)
            {
                logger.LogWarning("Purchase order '{OrderId}' was not found for customer '{CustomerId}'.", request.OrderId, request.CustomerId);
                throw new BusinessException($"Purchase order '{request.OrderId}' was not found for customer '{request.CustomerId}'.");
            }

            logger.LogInformation("Purchase order '{OrderId}' loaded. Applying processing rules.", purchaseOrder.Id);
            this.ValidateProcessOrder(purchaseOrder, request);

            foreach (var rule in rules)
            {
                if (!ct.IsCancellationRequested)
                {
                    logger.LogDebug("Applying rule '{RuleName}' for purchase order '{OrderId}'.", rule.GetType().Name, purchaseOrder.Id);
                    try
                    {
                        await rule.ApplyAsync(purchaseOrder, ct);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "An error occurred while applying rule '{RuleName}' for purchase order '{OrderId}'.", rule.GetType().Name, purchaseOrder.Id);
                        throw;
                    }
                }
            }
            await dbContext.SaveChangesAsync(ct);

            logger.LogInformation("Purchase order '{OrderId}' processed successfully.", purchaseOrder.Id);

            return new PurchaseOrderResponseDto
            {
                PurchaseOrderId = purchaseOrder.Id,
                TotalPrice = purchaseOrder.TotalPrice,
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while processing purchase order for RequestId '{RequestId}', OrderId '{OrderId}', CustomerId '{CustomerId}'.", normalizedRequestId, request.OrderId, request.CustomerId);
            throw;
        }
        finally
        {
            InProgressRequestIds.TryRemove(normalizedRequestId, out _);
            logger.LogDebug("RequestId '{RequestId}' removed from in-progress set.", normalizedRequestId);
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
