using System.ComponentModel.DataAnnotations;

namespace ProiectV1.Models
{
    public class Address
    {
        [Key]
        public int Id { get; set; }
        public string? UserId { get; set; } // FK 
        public virtual ApplicationUser? User { get; set; }
        public string City { get; set; }
        public string Streetname { get; set; }
        public int Streetnumber { get; set; }
        public int Postcode { get; set; }

    }
}
