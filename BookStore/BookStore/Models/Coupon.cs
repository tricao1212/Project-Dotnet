using System.ComponentModel.DataAnnotations;

namespace BookStore.Models
{
    public class Coupon
    {
        public int Id { get; set; }
        [Required]
        public string Giftcode { get; set; }
        [Required]
        public double Discount { get; set; }

        [DataType(DataType.Date)]
        public DateTime ExpireDate { get; set; }
    }
}
