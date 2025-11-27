using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{   

    [Serializable]  
    public class Product
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Category { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedOn { get; set; }

        public Product() { }    

        public Product(int id, string name, string category, decimal price)
        {
            Id = id;
            Name = name;
            Category = category;
            Price = price;
            CreatedOn = DateTime.UtcNow;
        }

        public override string ToString()
        {
            return $"{Id} - {Name} ({Category}) - ${Price}";
        }

        public override bool Equals(object? obj)
        {
            if (obj is Product product)
            {
                return Id == product.Id &&
                       Name?.Equals(product?.Name ) == true &&
                       Category?.Equals(product?.Category) == true &&
                       Price == product.Price &&
                       CreatedOn == product.CreatedOn;
            }

            return false;
        }
    }

}
