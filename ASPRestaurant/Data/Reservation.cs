using Microsoft.Identity.Client;

namespace ASPRestaurant.Data
{
    public class Reservation
    {
        public int Id { get; set; }
        public int NumberOfPeople { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public DateTime RegisterOn { get; set; } 
        public string ClientId { get; set; }
        public Client Clients { get; set; }
        public int TableId { get; set; }
        public Table Tables { get; set; }
    }
}
