using System.ComponentModel.DataAnnotations;

namespace BookStore.Models
{
    public class Coupon
    {
        public int Id { get; set; }
        public string Giftcode { get; set; }
        public double Discount { get; set; }

        [DataType(DataType.Date)]
        public DateTime ExpireDate { get; set; }
    }
}
