using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models
{
    public class Cart
    {
        public int Id { get; set; }

        [ForeignKey(nameof(BookUser))]
        public int UserId { get; set; }
        public BookUser User { get; set; }

        public int IndexTemp { get; set; }

        public ICollection<Order_Details> OrderDetails { get; set; }
    }
}
