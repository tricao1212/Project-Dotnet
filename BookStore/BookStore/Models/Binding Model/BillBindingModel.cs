using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BookStore.Models.Binding_Model
{
    public class BillBindingModel
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public decimal Payment { get; set; }
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }
        [Required]
        [StringLength(maximumLength: 100, MinimumLength = 15)]
        public string Address { get; set; }
        [Required]
        [RegularExpression("[0-9]{10}")]
        public string Phone { get; set; }
        public string Note { get; set; }
        [ForeignKey(nameof(Order))]
        public ICollection<Order> OrderDetails { get; set; }
        public int UserId { get; set; }
        public decimal discount { get; set; }
    }
}
