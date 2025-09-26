namespace FlowerShop.Domain
{
    public class FlowerSearchResult
    {
        public int FlowerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? SKU { get; set; }
        public decimal Price { get; set; }
        public int QuantityInStock { get; set; }
        public string? Color { get; set; }
        public decimal? StemLengthCm { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsInStock { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        public int TotalCount { get; set; }
    }
}
