using System.ComponentModel.DataAnnotations.Schema;

namespace ASP_ITStep.Data.Entities
{
    public class CartItem
    {
        public Guid Id { get; set; }
        public Guid CartId { get; set; }
        public Guid ProductId { get; set; }

        public int Quatity { get; set; } = 1;

        [Column(TypeName = "decimal(14, 2)")]
        public double Price { get; set; }

        public Guid? DiscountItemId { get; set; }

        public Cart Cart { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
