namespace FunBooksAndVideos.Core.Entities;

public class Customer
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<CustomerMemberships> CustomerMemberships { get; set; } = new();
}
