using System.ComponentModel.DataAnnotations;

namespace ProiectV1.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Continutul comentariului este obligatoriu!")]
        public string Text { get; set; }

        public DateTime Date {  get; set; }

        public int Stars { get; set; } // rating (1-5 stele)

        public int? ProductId { get; set; } // FK
        public virtual Product? Product { get; set; }

        public string? UserId { get; set; } // FK 
        public virtual ApplicationUser? User { get; set; }


        public Comment()
        {
            Date= DateTime.Now;
        }
    }
}
