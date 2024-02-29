using System.ComponentModel.DataAnnotations;

namespace ProiectV1.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Numele categoriei este obligatoriu!")]
        public string CategoryName { get; set; }
        public virtual ICollection<Product>? Products { get; set; } //mai multe produse au aceeasi categorie

        public virtual ICollection<Request>? Requests { get; set; } //mai multe request-uri au aceeasi categorie
    }
}
