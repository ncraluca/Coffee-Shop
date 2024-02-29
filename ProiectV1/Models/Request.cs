using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProiectV1.Models
{
    public class Request
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Titlul produsului este obligatoriu!")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Descrierea produsului este obligatorie!")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Pretul produsului este obligatoriu!")]
        [Range(1, int.MaxValue, ErrorMessage = "Pretul trebuie sa fie cel putin 1")]
        public int Price { get; set;}

        //adaugam requirements mai tz, momentan vrem sa vedem daca functioneaza overall
        //[Required(ErrorMessage = "Este obligatorie adaugarea unei poze pentru produs!")]
        public string? PictureUrl { get; set; } //aici ar tb fara ?

        


        public double? Rating { get; set; } //aici ar tb fara ? (in caz ca nu s-a dat niciun rating se afiseaza 0.00

        //detalii legate de request- status + data adaugarii
        public string? Status { get; set; }
        public DateTime Date { get; set; }  // data va fi preluata direct din sistem la momentul adaugarii unui request

        [Required(ErrorMessage = "Categoria din care face produsul este obligatorie!")]
        public int? CategoryId { get; set; } //FK
        public virtual Category? Category { get; set; } //facem join cu tabela Category


        // user-ul care face cererea de publicare a produsului in magazin
        public string? UserId { get; set; } // FK 
        public virtual ApplicationUser? User { get; set; }


        [NotMapped]
        public IEnumerable<SelectListItem>? Categ { get; set; }

        // Constructor care initializeaza data cu cea din server
        public Request()
        {
            Date = DateTime.Now;
            Status = "Pending";
        }

    }
}
