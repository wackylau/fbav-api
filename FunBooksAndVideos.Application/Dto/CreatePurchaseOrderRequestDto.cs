using FunBooksAndVideos.Core.Enums;

namespace FunBooksAndVideos.Application.Dto;

public class CreatePurchaseOrderRequestDto
{
    public int OrderId { get; set; }
    public string RequestId { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public decimal TotalPrice { get; set; }
    public List<CreatePurchaseOrderProductItemDto> Items { get; set; } = new();
}

public class CreatePurchaseOrderProductItemDto
{
    public int? ProductId { get; set; }
    public MembershipType? MembershipType { get; set; }
    public int Quantity { get; set; } = 1;
}
