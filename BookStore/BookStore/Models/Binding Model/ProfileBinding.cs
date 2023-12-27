using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace BookStore.Models.Binding_Model
{
    public class ProfileBinding
    {
        [Key]
        public int UserId { get; set; }

        public string Avatar { get; set; }
        public BookUser User { get; set; }
        [Required]
        [MaxLength(100)]
        [Display(Name = "First name")]
        public string FirstName { get; set; }
        [Required]
        [MaxLength(100)]
		[Display(Name = "Last name")]
		public string LastName { get; set; }
        public string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }
        [Required]
        public string Address { get; set; }
        [Required]
        [Display(Name = "Phone number")]
        [RegularExpression("[0-9]{10}")]
        public string PhoneNumber { get; set; }
        public int RankId { get; set; }
        public Rank Rank { get; set; }
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalSpent { get; set; } = 0;
    }
}
