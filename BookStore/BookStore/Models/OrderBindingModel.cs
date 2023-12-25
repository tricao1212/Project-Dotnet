using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models
{
    public class OrderBindingModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CartId { get; set; }

        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Required]
        [StringLength(maximumLength: 100, MinimumLength = 15)]
        public string Address { get; set; }

        [Required]
        [Display(Name = "Phone number")]
        [RegularExpression("[0-9]{10}")]
        public string Phone { get; set; }

        public string Note { get; set; }

        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

    }
}
