using FlowerShop.Domain;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FlowerShop.Web.Models
{
    public class FlowerListVm
    {
        public IReadOnlyList<Flower> Items { get; set; } = Array.Empty<Flower>();
        public int Total { get; set; }
        public string? Q { get; set; }
        public int? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string Sort { get; set; } = "name";
        public bool Desc { get; set; } = false;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public int TotalPages => Math.Max(1, (int)Math.Ceiling((double)Total / Math.Max(1, PageSize)));
        public SelectList? CategoryOptions { get; set; }
    }
}
