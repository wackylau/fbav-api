using FunBooksAndVideos.Core.Enums;

namespace FunBooksAndVideos.Core.Entities;

public class PurchaseOrder
{
    public long Id { get; set; }
    public Guid RequestId { get; set; }
    public long CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.New;
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public List<PurchaseOrderItem> Items { get; set; } = new();

    public decimal TotalPrice
    {
        get
        {
            return this.Items?.Sum(i => i.TotalPrice) ?? 0;
        }
    }
}
