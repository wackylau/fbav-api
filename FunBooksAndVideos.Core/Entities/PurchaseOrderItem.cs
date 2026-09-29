using FunBooksAndVideos.Core.Enums;

namespace FunBooksAndVideos.Core.Entities;

public class PurchaseOrderItem
{
    public int Id { get; set; }
    public int PurchaseOrderId { get; set; }
    public PurchaseOrder PurchaseOrder { get; set; } = null!;
    public int? ProductId { get; set; }
    public Product? Product { get; set; }
    public MembershipType? MembershipType { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice
    {
        get
        {
            return this.Quantity * this.UnitPrice;
        }
    }
}
