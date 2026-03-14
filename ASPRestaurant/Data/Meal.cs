using System.ComponentModel.DataAnnotations.Schema;

namespace ASPRestaurant.Data
{
    public class Meal
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Alergens { get; set; }
        public double Grammage { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }
        public string CoverImage { get; set; }
        public int TypeOrderId { get; set; }
        public TypeOrder TypeOrders {  get; set; }
    }
}
