using ASP_ITStep.Data.Entities;

namespace ASP_ITStep.Models.Shop
{
    public class ShopCartPageModel
    {
        public IEnumerable<CartItem> ActiveCartItems { get; set; } 
    }
}
