using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASPRestaurant.Data
{
    public class Drink
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Litre { get; set; } 
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }
    
        public string CoverImage { get; set; }
        public int TypeOrderId { get; set; }
        public TypeOrder TypeOrders { get; set; }
    }
}
