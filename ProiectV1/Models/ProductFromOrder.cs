using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

//DE REFACUT
namespace ProiectV1.Models
{
    public class ProductFromOrder
    {
      
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // cheie primara compusa (id, productid, orderid)
        [Key]
        public int Id { get; set; }
        public int ProductId {  get; set; }
        public int OrderId { get; set; }

        public virtual Product? Product { get; set; }
        public virtual Order? Order { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }

    }
}
