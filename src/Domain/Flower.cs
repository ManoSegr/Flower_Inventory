using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerShop.Domain
{
    public class Flower
    {
        [Key] public int FlowerId { get; set; }

        [Required] public int CategoryId { get; set; }
        public Category? Category { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Type { get; set; } = string.Empty;  

        [StringLength(50)]
        public string? SKU { get; set; }

        [Column(TypeName = "decimal(10,2)")]             
        public decimal Price { get; set; }

        public int QuantityInStock { get; set; }

        [StringLength(30)]
        public string? Color { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? StemLengthCm { get; set; }

        [StringLength(2083)]
        public string? ImageUrl { get; set; }

        public bool Active { get; set; } = true;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        [Timestamp]     
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
