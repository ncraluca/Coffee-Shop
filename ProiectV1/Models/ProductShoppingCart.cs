
using System.ComponentModel.DataAnnotations.Schema;

namespace ProiectV1.Models
{
    public class ProductShoppingCart //tabela auxiliara pt a face relatia many-to-many dintre Product si Shopping Cart
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // cheie primara compusa (id, productid, orderid)
        public int Id { get; set; }
        public int? ProductId { get; set; }
        public int? ShoppingCartId { get; set; }

        public int Quantity { get; set; } // Cantitatea de produse din cosul de cumparaturi

        public int? Price { get; set; }  // Pretul total pentru produsele cu acel id

        public Product Product { get; set; } //join cu tabela product
        public ShoppingCart ShoppingCart { get; set; } //join cu tabela shopping cart
    }
}
