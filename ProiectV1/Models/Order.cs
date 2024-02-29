using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace ProiectV1.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        public DateTime Date { get; set; }  // data va fi preluata direct din sistem la momentul plasarii unei comenzi
        public string State { get; set; }
        public string Price { get; set; }

        public string? UserId { get; set; } // FK 
        public virtual ApplicationUser? User { get; set; }

        // Constructor care initializeaza data cu cea din server
        public Order()
        {
            Date = DateTime.Now;
        }
        public virtual ICollection<ProductFromOrder>? ProductFromOrders { get; set; }
    }
}
