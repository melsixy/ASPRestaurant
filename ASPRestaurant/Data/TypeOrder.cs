namespace ASPRestaurant.Data
{
    public class TypeOrder
    {
        public int Id { get; set; }
        public string Name { get; set; }    
        public DateTime RegisterOn { get; set; }
        public ICollection<Meal> Meals { get; set; }
        public ICollection<Drink> Drinks { get; set; }
    }
}
