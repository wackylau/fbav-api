namespace FunBooksAndVideos.Core.Entities;

public class CustomerMemberships
{
    public long CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public int MembershipId { get; set; }
    public Membership Membership { get; set; } = null!;
}
