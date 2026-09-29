namespace FunBooksAndVideos.Core.Entities;

public class Membership
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public List<CustomerMemberships> CustomerMemberships { get; set; } = new();
}
