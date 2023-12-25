using System.ComponentModel.DataAnnotations;

namespace BookStore.Models
{
    public class Profile
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
        [StringLength(maximumLength: 100, MinimumLength = 15)]
        public string Address { get; set; }

        [Required]
        [Display(Name = "Phone number")]
        [RegularExpression("[0-9]{10}")]
        public string PhoneNumber { get; set; }
    }
}
