using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BookStore.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18, 3)")]
        [Range(0, (double)decimal.MaxValue)]
        public decimal Payment { get; set; }
        public Book Book { get; set; }
        public int BillId { get; set; }
        public Bill Bill { get; set; }
    }
}
