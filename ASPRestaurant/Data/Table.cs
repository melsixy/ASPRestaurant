namespace ASPRestaurant.Data
{
    public class Table
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public int TableNumber { get; set; }
        public int Count { get; set; }
        public ICollection<Reservation> Reservations { get; set; }   
    }
}
