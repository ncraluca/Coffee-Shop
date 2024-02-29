using System.ComponentModel.DataAnnotations;

namespace ProiectV1.Models
{
    public class ShoppingCart
    {
        [Key]
        public int Id { get; set; }

        //public int Price {  get; set; }

       
        public virtual ICollection<ProductShoppingCart>? ProductShoppingCarts { get; set; } //mai multe produse
                                                                                           //au acelasi cos de cumparaturi
        //? deoarece cosul poate sa fie si cu 0 produse (gol)
        //nu sunt sigura pt ca nu am mai facut relatii many to many

        public string? UserId { get; set; } //FK
        public virtual ApplicationUser? User { get; set; }

    }
}
