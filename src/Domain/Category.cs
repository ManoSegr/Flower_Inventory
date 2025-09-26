namespace FlowerShop.Domain
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        public ICollection<Flower>? Flowers { get; set; }
    }
}
