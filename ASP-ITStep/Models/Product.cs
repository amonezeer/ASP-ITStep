using System.ComponentModel.DataAnnotations;

namespace ASP_ITStep.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Display(Name = "Назва")]
        public string Name { get; set; }

        [Display(Name = "Ціна")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Display(Name = "Кількість")]
        public int Quantity { get; set; }

        public static List<Product> GetProducts()
        {
            return new List<Product>
            {
                new Product { Id = 1, Name = "iPhone 15", Price = 25000, Quantity = 5 },
                new Product { Id = 2, Name = "Samsung Galaxy S24", Price = 22000, Quantity = 8 },
                new Product { Id = 3, Name = "MacBook Air", Price = 45000, Quantity = 3 },
                new Product { Id = 4, Name = "Dell XPS 13", Price = 35000, Quantity = 7 },
                new Product { Id = 5, Name = "iPad Pro", Price = 30000, Quantity = 12 },
                new Product { Id = 6, Name = "Apple Watch", Price = 12000, Quantity = 15 },
                new Product { Id = 7, Name = "AirPods Pro", Price = 8000, Quantity = 20 },
                new Product { Id = 8, Name = "Sony WH-1000XM5", Price = 9500, Quantity = 6 }
            };
        }
    }
}