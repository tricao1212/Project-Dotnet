using System.ComponentModel.DataAnnotations;

namespace BookStore.Models
{
    public class Profile
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [Display(Name = "Book image")]
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
        public string PhoneNumber { get; set; }
    }
}
