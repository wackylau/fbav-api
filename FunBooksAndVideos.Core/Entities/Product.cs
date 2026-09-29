using FunBooksAndVideos.Core.Enums;

namespace FunBooksAndVideos.Core.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ProductType ProductType { get; set; }
    public bool IsPhysical { get; set; }
    public decimal Price { get; set; }
}
