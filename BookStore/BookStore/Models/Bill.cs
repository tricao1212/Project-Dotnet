using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BookStore.Models
{
    public class Bill
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public decimal Payment { get; set; }
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Note { get; set; }
        [ForeignKey(nameof(Order))]
        public ICollection<Order> OrderDetails { get; set; }
        public int UserId { get; set; }
        public BookUser User { get; set; }

        public OrderStatus Status { get; set; }
    }
}
