using Microsoft.AspNetCore.Identity;

namespace ASPRestaurant.Data
{
    public class Client : IdentityUser
    {
        public string FirstName {  get; set; }
        public string LastName { get; set; }
       
        public DateTime CreatedOn { get; set; }
        public ICollection<Reservation> Reservations { get; set; }
    }
}
