using ASP_ITStep.Data.Entities;

namespace ASP_ITStep.Models.Shop
{
    public class ShopGroupPageModel
    {
        public ProductGroup? ProductGroup { get; set; } = null!;
        public List<ProductGroup> ProductGroups { get; set; } = new();
    }
}
