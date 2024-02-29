using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProiectV1.Models
{
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<Comment>? Comments {get; set;} //un user (inregistrat) adauga comentarii
        public virtual ShoppingCart? ShoppingCart { get; set; } //un user (inregistrat) are un cos de cumparaturi(fie el gol sau cu ceva)
        public virtual ICollection<Order>? Orders {get; set;} // un user (inregistrat) plaseaza comenzi

        public virtual ICollection<Product>? Products { get; set; } //un user (admin) publica mai multe produse (sau niciunul)

        public virtual ICollection<Request>? Requests { get; set; } //un user(admin/colaborator) poate avea mai multe request-uri

        public virtual ICollection<Address>? Address { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public string? PhoneNumber { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem>? AllRoles { get; set; }

    }
}
