using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProiectV1.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Titlul produsului este obligatoriu!")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Descrierea produsului este obligatorie!")]
        public string Description { get; set; }

        //adaugam requirements mai tz, momentan vrem sa vedem daca functioneaza overall
        //[Required(ErrorMessage = "Este obligatorie adaugarea unei poze pentru produs!")]
        public string? PictureUrl { get; set; } 

        [Required(ErrorMessage = "Pretul produsului este obligatoriu!")]
        [Range(1, int.MaxValue, ErrorMessage = "Pretul trebuie sa fie cel putin 0")]
        public int Price { get; set; } //aici ar tb fara ?

        public double? Rating { get; set; } //(in caz ca nu s-a dat niciun rating se afiseaza 0.00

        [Required(ErrorMessage = "Categoria din care face produsul este obligatorie!")]
        public int? CategoryId { get; set; } //FK
        public virtual Category? Category { get; set; } //facem join cu tabela Category

        public virtual ICollection<ProductShoppingCart>? ProductShoppingCarts { get; set; } //mai multe cosuri de
                                                                                            //cumparaturi au acelasi produs
        //? deoarece nu e neaparat sa apartina de un cos de cumparaturi

        public virtual ICollection<Comment>? Comments { get; set; } //mai multe comentarii au acelasi produs

        // user-ul care a publicat produsul in magazin
        public string? UserId { get; set; } // FK 
        public virtual ApplicationUser? User { get; set; }

        
        [NotMapped]
        public IEnumerable<SelectListItem>? Categ {  get; set; }

        public virtual ICollection<ProductFromOrder>? ProductFromOrders { get; set; }

    }
}
